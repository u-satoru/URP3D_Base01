using UnityEngine;
using UnityEngine.InputSystem;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Commands;
using asterivo.Unity60.Features.Templates.FPS.Weapons;
using asterivo.Unity60.Features.Templates.FPS.Weapons.Commands;
using System.Collections.Generic;

namespace asterivo.Unity60.Features.Templates.FPS.Weapons
{
    /// <summary>
    /// FPS武器管理システム
    /// 複数の武器の切り替え、弾薬管理、射撃制御を統合管理
    /// </summary>
    public class FPSWeaponManager : MonoBehaviour
    {
        [Header("Weapon Settings")]
        [SerializeField] private List<WeaponData> availableWeapons = new List<WeaponData>();
        [SerializeField] private int currentWeaponIndex = 0;
        [SerializeField] private Transform weaponHolder;
        [SerializeField] private Transform muzzlePoint;
        
        [Header("Ammo System")]
        [SerializeField] private int maxAmmoPerType = 300;
        [SerializeField] private Dictionary<string, int> ammoInventory = new Dictionary<string, int>();
        
        [Header("Events")]
        [SerializeField] private GameEvent onWeaponChanged;
        [SerializeField] private GameEvent onAmmoChanged;
        [SerializeField] private GameEvent onWeaponFired;
        [SerializeField] private GameEvent onReloadStarted;
        [SerializeField] private GameEvent onReloadCompleted;
        
        private WeaponSystem currentWeaponSystem;
        private IWeapon currentWeapon;
        private bool isReloading;
        private bool isAiming;
        private PlayerInput playerInput;
        
        // Properties
        public WeaponData CurrentWeaponData => availableWeapons.Count > 0 ? availableWeapons[currentWeaponIndex] : null;
        public IWeapon CurrentWeapon => currentWeapon;
        public bool IsReloading => isReloading;
        public bool IsAiming => isAiming;
        public int CurrentAmmo => currentWeapon?.CurrentAmmo ?? 0;
        public int MaxAmmo => currentWeapon?.MaxAmmo ?? 0;
        public Transform MuzzlePoint => muzzlePoint;
        
        private void Awake()
        {
            playerInput = GetComponent<PlayerInput>();
            currentWeaponSystem = GetComponent<WeaponSystem>();
            
            if (weaponHolder == null)
                weaponHolder = transform;
                
            InitializeAmmoInventory();
        }
        
        private void Start()
        {
            if (availableWeapons.Count > 0)
            {
                EquipWeapon(0);
            }
            
            SetupInputCallbacks();
        }
        
        /// <summary>
        /// 弾薬インベントリ初期化
        /// </summary>
        private void InitializeAmmoInventory()
        {
            foreach (var weaponData in availableWeapons)
            {
                if (!ammoInventory.ContainsKey(weaponData.WeaponType.ToString()))
                {
                    ammoInventory[weaponData.WeaponType.ToString()] = maxAmmoPerType;
                }
            }
        }
        
        /// <summary>
        /// 入力コールバック設定
        /// </summary>
        private void SetupInputCallbacks()
        {
            if (playerInput != null)
            {
                var fireAction = playerInput.actions["Fire"];
                var reloadAction = playerInput.actions["Reload"];
                var weaponSwitch = playerInput.actions["WeaponSwitch"];
                var aimAction = playerInput.actions["Aim"];
                
                if (fireAction != null)
                {
                    fireAction.performed += OnFirePerformed;
                    fireAction.canceled += OnFireCanceled;
                }
                
                if (reloadAction != null)
                    reloadAction.performed += OnReloadPerformed;
                    
                if (weaponSwitch != null)
                    weaponSwitch.performed += OnWeaponSwitchPerformed;
                    
                if (aimAction != null)
                {
                    aimAction.performed += OnAimPerformed;
                    aimAction.canceled += OnAimCanceled;
                }
            }
        }
        
        /// <summary>
        /// 武器装備
        /// </summary>
        public void EquipWeapon(int weaponIndex)
        {
            if (weaponIndex < 0 || weaponIndex >= availableWeapons.Count)
                return;
                
            // 現在の武器を解除
            if (currentWeapon != null)
            {
                // 武器の無効化処理
                currentWeapon = null;
            }
            
            currentWeaponIndex = weaponIndex;
            var weaponData = availableWeapons[currentWeaponIndex];
            
            // 新しい武器を装備
            if (currentWeaponSystem != null)
            {
                currentWeaponSystem.weaponData = weaponData;
                currentWeapon = currentWeaponSystem;
                if (currentWeapon != null)
                {
                    // 武器の初期化
                    var availableAmmo = GetAvailableAmmo(weaponData.WeaponType.ToString());
                    // 武器に弾薬を設定（実装は武器側による）
                    
                    onWeaponChanged?.Raise();
                    onAmmoChanged?.Raise();
                    
                    Debug.Log($"[FPSWeaponManager] Equipped weapon: {weaponData.WeaponName}");
                }
            }
        }
        
        /// <summary>
        /// 射撃処理
        /// </summary>
        public void Fire()
        {
            if (currentWeapon == null || isReloading)
                return;
                
            if (currentWeapon.CanShoot)
            {
                if (currentWeaponSystem != null)
                {
                    var shootCommand = new ShootCommand(muzzlePoint.position, muzzlePoint.forward, CurrentWeaponData);
                    shootCommand.Execute();
                    
                    onWeaponFired?.Raise();
                    onAmmoChanged?.Raise();
                }
            }
        }
        
        /// <summary>
        /// リロード処理
        /// </summary>
        public void Reload()
        {
            if (currentWeapon == null || isReloading)
                return;
                
            var weaponData = CurrentWeaponData;
            if (weaponData == null)
                return;
                
            var availableAmmo = GetAvailableAmmo(weaponData.WeaponType.ToString());
            if (availableAmmo <= 0)
            {
                Debug.Log("[FPSWeaponManager] No ammo available for reload");
                return;
            }
            
            StartCoroutine(ReloadCoroutine(weaponData));
        }
        
        /// <summary>
        /// リロードコルーチン
        /// </summary>
        private System.Collections.IEnumerator ReloadCoroutine(WeaponData weaponData)
        {
            isReloading = true;
            onReloadStarted?.Raise();
            
            Debug.Log($"[FPSWeaponManager] Reloading {weaponData.WeaponName}...");
            
            yield return new UnityEngine.WaitForSeconds(weaponData.ReloadTime);
            
            // 弾薬計算
            var neededAmmo = weaponData.MagazineSize - currentWeapon.CurrentAmmo;
            var availableAmmo = GetAvailableAmmo(weaponData.WeaponType.ToString());
            var ammoToReload = Mathf.Min(neededAmmo, availableAmmo);
            
            // 弾薬消費
            ConsumeAmmo(weaponData.WeaponType.ToString(), ammoToReload);
            
            // 武器にリロード実行
            if (currentWeaponSystem != null)
            {
                var reloadCommand = new ReloadCommand(currentWeapon);
                reloadCommand.Execute();
            }
            
            isReloading = false;
            onReloadCompleted?.Raise();
            onAmmoChanged?.Raise();
            
            Debug.Log($"[FPSWeaponManager] Reload completed. Added {ammoToReload} rounds");
        }
        
        /// <summary>
        /// 利用可能弾薬数取得
        /// </summary>
        private int GetAvailableAmmo(string ammoType)
        {
            return ammoInventory.ContainsKey(ammoType) ? ammoInventory[ammoType] : 0;
        }
        
        /// <summary>
        /// 弾薬消費
        /// </summary>
        private void ConsumeAmmo(string ammoType, int amount)
        {
            if (ammoInventory.ContainsKey(ammoType))
            {
                ammoInventory[ammoType] = Mathf.Max(0, ammoInventory[ammoType] - amount);
            }
        }
        
        /// <summary>
        /// 弾薬追加
        /// </summary>
        public void AddAmmo(string ammoType, int amount)
        {
            if (!ammoInventory.ContainsKey(ammoType))
                ammoInventory[ammoType] = 0;
                
            ammoInventory[ammoType] = Mathf.Min(maxAmmoPerType, ammoInventory[ammoType] + amount);
            onAmmoChanged?.Raise();
            
            Debug.Log($"[FPSWeaponManager] Added {amount} {ammoType} ammo");
        }
        
        // Input Callbacks
        private void OnFirePerformed(InputAction.CallbackContext context)
        {
            Fire();
        }
        
        private void OnFireCanceled(InputAction.CallbackContext context)
        {
            // 連射停止処理（必要に応じて）
        }
        
        private void OnReloadPerformed(InputAction.CallbackContext context)
        {
            Reload();
        }
        
        private void OnWeaponSwitchPerformed(InputAction.CallbackContext context)
        {
            float value = context.ReadValue<float>();
            if (value > 0)
            {
                SwitchToNextWeapon();
            }
            else if (value < 0)
            {
                SwitchToPreviousWeapon();
            }
        }
        
        private void OnAimPerformed(InputAction.CallbackContext context)
        {
            isAiming = true;
        }
        
        private void OnAimCanceled(InputAction.CallbackContext context)
        {
            isAiming = false;
        }
        
        /// <summary>
        /// 次の武器に切り替え
        /// </summary>
        public void SwitchToNextWeapon()
        {
            if (availableWeapons.Count <= 1) return;
            
            int nextIndex = (currentWeaponIndex + 1) % availableWeapons.Count;
            EquipWeapon(nextIndex);
        }
        
        /// <summary>
        /// 前の武器に切り替え
        /// </summary>
        public void SwitchToPreviousWeapon()
        {
            if (availableWeapons.Count <= 1) return;
            
            int prevIndex = (currentWeaponIndex - 1 + availableWeapons.Count) % availableWeapons.Count;
            EquipWeapon(prevIndex);
        }
        
        private void OnDestroy()
        {
            // 入力コールバックの解除
            if (playerInput != null)
            {
                var fireAction = playerInput.actions["Fire"];
                var reloadAction = playerInput.actions["Reload"];
                var weaponSwitch = playerInput.actions["WeaponSwitch"];
                var aimAction = playerInput.actions["Aim"];
                
                if (fireAction != null)
                {
                    fireAction.performed -= OnFirePerformed;
                    fireAction.canceled -= OnFireCanceled;
                }
                
                if (reloadAction != null)
                    reloadAction.performed -= OnReloadPerformed;
                    
                if (weaponSwitch != null)
                    weaponSwitch.performed -= OnWeaponSwitchPerformed;
                    
                if (aimAction != null)
                {
                    aimAction.performed -= OnAimPerformed;
                    aimAction.canceled -= OnAimCanceled;
                }
            }
        }
    }
}
