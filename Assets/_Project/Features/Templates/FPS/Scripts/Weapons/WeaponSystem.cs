using UnityEngine;
using System.Collections;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Commands;
using asterivo.Unity60.Core;
using asterivo.Unity60.Features.Templates.FPS.Weapons.Commands;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Features.Templates.FPS.Weapons
{
    /// <summary>
    /// FPS武器システムのメインクラス
    /// IWeaponインターフェースを実装し、射撃・リロード・エイム機能を提供
    /// Event-DrivenアーキテクチャとCommand Patternを統合
    /// </summary>
    public class WeaponSystem : MonoBehaviour, IWeapon
    {
        [TabGroup("Weapon", "Data")]
        [LabelText("Weapon Data")]
        [SerializeField] private WeaponData weaponData;
        
        [TabGroup("Weapon", "Transform")]
        [LabelText("Muzzle Transform")]
        [SerializeField] private Transform muzzleTransform;
        
        [TabGroup("Weapon", "Transform")]
        [LabelText("Shell Ejection Point")]
        [SerializeField] private Transform shellEjectionPoint;
        
        [TabGroup("Weapon", "Audio")]
        [LabelText("Audio Source")]
        [SerializeField] private AudioSource weaponAudioSource;
        
        [TabGroup("Events", "Weapon Events")]
        [LabelText("On Weapon Fired")]
        [SerializeField] private GameEvent onWeaponFired;
        
        [LabelText("On Weapon Empty")]
        [SerializeField] private GameEvent onWeaponEmpty;
        
        [LabelText("On Reload Started")]
        [SerializeField] private GameEvent onReloadStarted;
        
        [LabelText("On Reload Completed")]
        [SerializeField] private GameEvent onReloadCompleted;
        
        [TabGroup("Events", "Command Events")]
        [LabelText("Command Definition Event")]
        [SerializeField] private CommandDefinitionGameEvent onCommandIssued;
        
        // Private fields for weapon state
        private int currentAmmo;
        private int reserveAmmo;
        private bool isReloading;
        private bool isAiming;
        private float lastShotTime;
        private int burstShotsFired;
        // private bool isFiring; // Removed unused field
        
        // Command references
        private CommandInvoker commandInvoker;
        
        [TabGroup("Debug", "Status")]
        [ReadOnly]
        [ShowInInspector]
        #pragma warning disable 0414 // Suppress unused field warning for Inspector display
        [LabelText("Current State")]
        private string currentState = "Ready";
        
        [ReadOnly]
        [ShowInInspector]
        [LabelText("Ammo Status")]
        private string ammoStatus = "0/0";
        
        // Properties from IWeapon
        public WeaponData WeaponData => weaponData;
        public int CurrentAmmo => currentAmmo;
        public int MaxAmmo => weaponData?.MagazineSize ?? 0;
        public int ReserveAmmo => reserveAmmo;
        public bool CanShoot => !isReloading && currentAmmo > 0 && CanFireBasedOnRate();
        public bool IsReloading => isReloading;
        public bool IsAiming => isAiming;
        
        private void Awake()
        {
            InitializeWeapon();
            InitializeCommands();
        }
        
        private void Start()
        {
            if (weaponData != null)
            {
                currentAmmo = weaponData.MagazineSize;
                reserveAmmo = weaponData.MaxReserveAmmo;
                UpdateAmmoStatus();
            }
        }
        
        private void Update()
        {
            UpdateWeapon();
            UpdateDebugInfo();
        }
        
        public bool Shoot(Vector3 origin, Vector3 direction)
        {
            if (!CanShoot)
            {
                // 弾薬切れの場合は空撃ち音を再生
                if (currentAmmo <= 0)
                {
                    PlayEmptySound();
                    onWeaponEmpty?.Raise();
                }
                return false;
            }
            
            // 射撃コマンドを作成・実行
            var shootCommand = new ShootCommand(origin, direction, weaponData);
            commandInvoker?.ExecuteCommand(shootCommand);
            
            // 弾薬を消費
            currentAmmo--;
            lastShotTime = Time.time;
            
            // バースト射撃の管理
            if (weaponData.FireMode == FireMode.Burst)
            {
                burstShotsFired++;
                if (burstShotsFired >= weaponData.BurstCount)
                {
                    burstShotsFired = 0;
                    // Burst completed;
                }
            }
            
            // エフェクトと音声を再生
            PlayShootEffects();
            PlayShootSound();
            
            // イベント発行
            onWeaponFired?.Raise();
            
            UpdateAmmoStatus();
            
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"WeaponSystem: Fired {weaponData.WeaponName} - Ammo: {currentAmmo}/{MaxAmmo}");
#endif
            
            return true;
        }
        
        public bool StartReload()
        {
            if (isReloading || currentAmmo >= MaxAmmo || reserveAmmo <= 0)
            {
                return false;
            }
            
            StartCoroutine(ReloadCoroutine());
            return true;
        }
        
        public void StartAiming()
        {
            isAiming = true;
            currentState = "Aiming";
        }
        
        public void StopAiming()
        {
            isAiming = false;
            currentState = "Ready";
        }
        
        public void UpdateWeapon()
        {
            // 武器の継続的な更新処理
            // 反動回復など
        }
        
        private void InitializeWeapon()
        {
            if (weaponAudioSource == null)
            {
                weaponAudioSource = GetComponent<AudioSource>();
                if (weaponAudioSource == null)
                {
                    weaponAudioSource = gameObject.AddComponent<AudioSource>();
                }
            }
            
            if (muzzleTransform == null)
            {
                muzzleTransform = transform;
            }
        }
        
        private void InitializeCommands()
        {
            commandInvoker = ServiceLocator.GetService<CommandInvoker>();
            if (commandInvoker == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogWarning("WeaponSystem: CommandInvoker not found in ServiceLocator");
#endif
            }
        }
        
        private bool CanFireBasedOnRate()
        {
            float timeSinceLastShot = Time.time - lastShotTime;
            return timeSinceLastShot >= weaponData.FireInterval;
        }
        
        private IEnumerator ReloadCoroutine()
        {
            isReloading = true;
            currentState = "Reloading";
            
            onReloadStarted?.Raise();
            PlayReloadSound();
            
            yield return new WaitForSeconds(weaponData.ReloadTime);
            
            // 弾薬補充計算
            int ammoNeeded = MaxAmmo - currentAmmo;
            int ammoToReload = Mathf.Min(ammoNeeded, reserveAmmo);
            
            currentAmmo += ammoToReload;
            reserveAmmo -= ammoToReload;
            
            isReloading = false;
            currentState = "Ready";
            
            onReloadCompleted?.Raise();
            UpdateAmmoStatus();
            
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"WeaponSystem: Reload completed - Ammo: {currentAmmo}/{MaxAmmo}, Reserve: {reserveAmmo}");
#endif
        }
        
        private void PlayShootEffects()
        {
            // マズルフラッシュ
            if (weaponData.MuzzleFlashPrefab != null && muzzleTransform != null)
            {
                var muzzleFlash = Instantiate(weaponData.MuzzleFlashPrefab, muzzleTransform.position, muzzleTransform.rotation);
                Destroy(muzzleFlash, 0.1f);
            }
            
            // 薬莢排出
            if (weaponData.ShellEjectionPrefab != null && shellEjectionPoint != null)
            {
                var shell = Instantiate(weaponData.ShellEjectionPrefab, shellEjectionPoint.position, shellEjectionPoint.rotation);
                Destroy(shell, 5f);
            }
        }
        
        private void PlayShootSound()
        {
            if (weaponData.FireSound != null && weaponAudioSource != null)
            {
                weaponAudioSource.volume = weaponData.AudioVolume;
                weaponAudioSource.PlayOneShot(weaponData.FireSound);
            }
        }
        
        private void PlayReloadSound()
        {
            if (weaponData.ReloadSound != null && weaponAudioSource != null)
            {
                weaponAudioSource.volume = weaponData.AudioVolume;
                weaponAudioSource.PlayOneShot(weaponData.ReloadSound);
            }
        }
        
        private void PlayEmptySound()
        {
            if (weaponData.EmptySound != null && weaponAudioSource != null)
            {
                weaponAudioSource.volume = weaponData.AudioVolume;
                weaponAudioSource.PlayOneShot(weaponData.EmptySound);
            }
        }
        
        private void UpdateAmmoStatus()
        {
            ammoStatus = $"{currentAmmo}/{MaxAmmo} ({reserveAmmo})";
        }
        
        private void UpdateDebugInfo()
        {
            if (isReloading)
                currentState = "Reloading";
            else if (isAiming)
                currentState = "Aiming";
            else if (currentAmmo <= 0)
                currentState = "Empty";
            else
                currentState = "Ready";
        }
    }
}