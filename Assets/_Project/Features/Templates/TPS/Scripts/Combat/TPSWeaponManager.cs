using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using asterivo.Unity60.Core;
using asterivo.Unity60.Features.Templates.TPS.Combat.Data;
using asterivo.Unity60.Features.Templates.TPS.Services;

namespace asterivo.Unity60.Features.Templates.TPS.Combat
{
    /// <summary>
    /// TPS Weapon Manager - Handles weapon switching, ammunition, and combat mechanics
    /// Integrates with ServiceLocator pattern and event-driven architecture
    /// </summary>
    public class TPSWeaponManager : MonoBehaviour
    {
        [Header("Weapon Configuration")]
        [SerializeField] private List<WeaponData> _availableWeapons = new List<WeaponData>();
        [SerializeField] private Transform _weaponAttachPoint;
        [SerializeField] private Transform _muzzlePoint;

        [Header("ServiceLocator Dependencies")]
        [SerializeField] private TPSServiceManager _serviceManager;

        [Header("Ammunition System")]
        [SerializeField] private Dictionary<AmmoType, int> _ammoInventory = new Dictionary<AmmoType, int>();

        // Current weapon state
        private WeaponData _currentWeaponData;
        private GameObject _currentWeaponInstance;
        private int _currentWeaponIndex = 0;
        private int _currentAmmoInMagazine;
        private float _lastFireTime;
        private bool _isReloading;
        private bool _isEquipping;

        // ServiceLocator-managed dependencies
        private IAudioManager _audioManager;
        private IGameEventManager _eventManager;

        // Events for weapon state changes
        [Header("Weapon Events")]
        public UnityEvent<WeaponData> OnWeaponEquipped;
        public UnityEvent<int, int> OnAmmoChanged; // current in magazine, total ammo
        public UnityEvent OnReloadStarted;
        public UnityEvent OnReloadCompleted;
        public UnityEvent OnWeaponFired;
        public UnityEvent OnWeaponEmpty;

        // Properties
        public WeaponData CurrentWeapon => _currentWeaponData;
        public int CurrentAmmoInMagazine => _currentAmmoInMagazine;
        public bool IsReloading => _isReloading;
        public bool IsEquipping => _isEquipping;
        public bool CanFire => !_isReloading && !_isEquipping && _currentAmmoInMagazine > 0 && 
                              Time.time >= _lastFireTime + _currentWeaponData.FireInterval;
        public Transform MuzzlePoint => _muzzlePoint;

        private void Awake()
        {
            InitializeAmmoInventory();
        }

        private void Start()
        {
            InitializeServices();
            EquipWeapon(0); // Equip first weapon by default
        }

        /// <summary>
        /// Initialize ServiceLocator dependencies
        /// </summary>
        private void InitializeServices()
        {
            if (_serviceManager == null)
            {
                _serviceManager = FindObjectOfType<TPSServiceManager>();
                if (_serviceManager == null)
                {
                    Debug.LogError("[TPSWeaponManager] TPSServiceManager not found in scene!");
                    return;
                }
            }

            _serviceManager.InitializeServices();

            // Get services from ServiceLocator
            _audioManager = _serviceManager.GetAudioManager();
            _eventManager = _serviceManager.GetGameEventManager();
        }

        /// <summary>
        /// Initialize ammunition inventory
        /// </summary>
        private void InitializeAmmoInventory()
        {
            // Initialize ammo inventory for all ammo types
            foreach (AmmoType ammoType in System.Enum.GetValues(typeof(AmmoType)))
            {
                _ammoInventory[ammoType] = GetDefaultAmmoForType(ammoType);
            }
        }

        /// <summary>
        /// Get default ammo amount for each ammo type
        /// </summary>
        private int GetDefaultAmmoForType(AmmoType ammoType)
        {
            return ammoType switch
            {
                AmmoType.Pistol => 120,
                AmmoType.Rifle => 180,
                AmmoType.Shotgun => 60,
                AmmoType.Sniper => 30,
                AmmoType.Grenade => 6,
                _ => 100
            };
        }

        /// <summary>
        /// Equip weapon by index
        /// </summary>
        public void EquipWeapon(int weaponIndex)
        {
            if (weaponIndex < 0 || weaponIndex >= _availableWeapons.Count || _isEquipping)
                return;

            if (weaponIndex == _currentWeaponIndex && _currentWeaponData != null)
                return; // Already equipped

            StartCoroutine(EquipWeaponCoroutine(weaponIndex));
        }

        /// <summary>
        /// Equip next weapon in inventory
        /// </summary>
        public void EquipNextWeapon()
        {
            int nextIndex = (_currentWeaponIndex + 1) % _availableWeapons.Count;
            EquipWeapon(nextIndex);
        }

        /// <summary>
        /// Equip previous weapon in inventory
        /// </summary>
        public void EquipPreviousWeapon()
        {
            int prevIndex = _currentWeaponIndex - 1;
            if (prevIndex < 0) prevIndex = _availableWeapons.Count - 1;
            EquipWeapon(prevIndex);
        }

        /// <summary>
        /// Weapon equipping coroutine
        /// </summary>
        private System.Collections.IEnumerator EquipWeaponCoroutine(int weaponIndex)
        {
            _isEquipping = true;

            // Unequip current weapon if any
            if (_currentWeaponInstance != null)
            {
                Destroy(_currentWeaponInstance);
            }

            // Get new weapon data
            _currentWeaponData = _availableWeapons[weaponIndex];
            _currentWeaponIndex = weaponIndex;

            // Play equip sound
            if (_audioManager != null && _currentWeaponData.EquipSound != null)
            {
                _audioManager.PlaySFX(_currentWeaponData.EquipSound);
            }

            // Wait for equip time
            yield return new WaitForSeconds(_currentWeaponData.EquipTime);

            // Instantiate new weapon
            if (_currentWeaponData.WeaponPrefab != null && _weaponAttachPoint != null)
            {
                _currentWeaponInstance = Instantiate(_currentWeaponData.WeaponPrefab, _weaponAttachPoint);
                _currentWeaponInstance.transform.localPosition = Vector3.zero;
                _currentWeaponInstance.transform.localRotation = Quaternion.identity;
            }

            // Initialize ammo for current weapon
            _currentAmmoInMagazine = _currentWeaponData.MagazineSize;

            _isEquipping = false;

            // Fire events
            OnWeaponEquipped?.Invoke(_currentWeaponData);
            UpdateAmmoDisplay();

            Debug.Log($"[TPSWeaponManager] Equipped weapon: {_currentWeaponData.WeaponName}");
        }

        /// <summary>
        /// Fire current weapon
        /// </summary>
        public bool FireWeapon()
        {
            if (!CanFire) return false;

            // Consume ammo
            _currentAmmoInMagazine--;
            _lastFireTime = Time.time;

            // Play fire sound
            if (_audioManager != null && _currentWeaponData.FireSound != null)
            {
                _audioManager.PlaySFX(_currentWeaponData.FireSound);
            }

            // Fire weapon logic (raycast, damage, effects)
            PerformWeaponFire();

            // Fire events
            OnWeaponFired?.Invoke();
            UpdateAmmoDisplay();

            // Check if magazine is empty
            if (_currentAmmoInMagazine <= 0)
            {
                OnWeaponEmpty?.Invoke();
            }

            Debug.Log($"[TPSWeaponManager] Fired {_currentWeaponData.WeaponName}. Ammo: {_currentAmmoInMagazine}/{GetTotalAmmo(_currentWeaponData.AmmoType)}");
            return true;
        }

        /// <summary>
        /// Perform actual weapon firing logic
        /// </summary>
        private void PerformWeaponFire()
        {
            if (_muzzlePoint == null) return;

            // Calculate spread
            Vector3 fireDirection = _muzzlePoint.forward;
            if (_currentWeaponData.SpreadAngle > 0)
            {
                float spread = _currentWeaponData.SpreadAngle * (1f - _currentWeaponData.Accuracy);
                fireDirection = AddSpreadToDirection(fireDirection, spread);
            }

            // Perform raycast
            if (Physics.Raycast(_muzzlePoint.position, fireDirection, out RaycastHit hit, _currentWeaponData.DamageRange))
            {
                // Calculate damage based on distance
                float distance = Vector3.Distance(_muzzlePoint.position, hit.point);
                float damage = _currentWeaponData.GetDamageAtDistance(distance);

                // Apply headshot multiplier if hitting head
                if (hit.collider.CompareTag("Head"))
                {
                    damage *= _currentWeaponData.HeadShotMultiplier;
                }

                // Deal damage to target
                var healthComponent = hit.collider.GetComponent<IHealthInterface>();
                if (healthComponent != null)
                {
                    healthComponent.TakeDamage(damage);
                }

                // Spawn impact effect
                if (_currentWeaponData.BulletImpactPrefab != null)
                {
                    Instantiate(_currentWeaponData.BulletImpactPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                }

                Debug.Log($"[TPSWeaponManager] Hit {hit.collider.name} for {damage} damage at distance {distance}m");
            }

            // Spawn muzzle flash
            if (_currentWeaponData.MuzzleFlashPrefab != null)
            {
                var muzzleFlash = Instantiate(_currentWeaponData.MuzzleFlashPrefab, _muzzlePoint.position, _muzzlePoint.rotation);
                Destroy(muzzleFlash, 0.1f);
            }
        }

        /// <summary>
        /// Add spread to fire direction
        /// </summary>
        private Vector3 AddSpreadToDirection(Vector3 direction, float spreadAngle)
        {
            float spreadX = Random.Range(-spreadAngle, spreadAngle);
            float spreadY = Random.Range(-spreadAngle, spreadAngle);
            
            Quaternion spread = Quaternion.Euler(spreadX, spreadY, 0);
            return spread * direction;
        }

        /// <summary>
        /// Reload current weapon
        /// </summary>
        public bool ReloadWeapon()
        {
            if (_isReloading || _currentAmmoInMagazine >= _currentWeaponData.MagazineSize)
                return false;

            int availableAmmo = GetTotalAmmo(_currentWeaponData.AmmoType);
            if (availableAmmo <= 0) return false;

            StartCoroutine(ReloadCoroutine());
            return true;
        }

        /// <summary>
        /// Reload coroutine
        /// </summary>
        private System.Collections.IEnumerator ReloadCoroutine()
        {
            _isReloading = true;
            OnReloadStarted?.Invoke();

            // Play reload sound
            if (_audioManager != null && _currentWeaponData.ReloadSound != null)
            {
                _audioManager.PlaySFX(_currentWeaponData.ReloadSound);
            }

            Debug.Log($"[TPSWeaponManager] Reloading {_currentWeaponData.WeaponName}...");

            // Wait for reload time
            yield return new WaitForSeconds(_currentWeaponData.ReloadTime);

            // Calculate ammo to reload
            int ammoNeeded = _currentWeaponData.MagazineSize - _currentAmmoInMagazine;
            int availableAmmo = GetTotalAmmo(_currentWeaponData.AmmoType);
            int ammoToReload = Mathf.Min(ammoNeeded, availableAmmo);

            // Update ammo counts
            _currentAmmoInMagazine += ammoToReload;
            ConsumeAmmo(_currentWeaponData.AmmoType, ammoToReload);

            _isReloading = false;
            OnReloadCompleted?.Invoke();
            UpdateAmmoDisplay();

            Debug.Log($"[TPSWeaponManager] Reload completed. Ammo: {_currentAmmoInMagazine}/{GetTotalAmmo(_currentWeaponData.AmmoType)}");
        }

        /// <summary>
        /// Get total ammo for ammo type
        /// </summary>
        public int GetTotalAmmo(AmmoType ammoType)
        {
            return _ammoInventory.TryGetValue(ammoType, out int ammo) ? ammo : 0;
        }

        /// <summary>
        /// Add ammo to inventory
        /// </summary>
        public void AddAmmo(AmmoType ammoType, int amount)
        {
            if (_ammoInventory.ContainsKey(ammoType))
            {
                _ammoInventory[ammoType] += amount;
            }
            else
            {
                _ammoInventory[ammoType] = amount;
            }

            UpdateAmmoDisplay();
        }

        /// <summary>
        /// Consume ammo from inventory
        /// </summary>
        private void ConsumeAmmo(AmmoType ammoType, int amount)
        {
            if (_ammoInventory.ContainsKey(ammoType))
            {
                _ammoInventory[ammoType] = Mathf.Max(0, _ammoInventory[ammoType] - amount);
            }
        }

        /// <summary>
        /// Update ammo display
        /// </summary>
        private void UpdateAmmoDisplay()
        {
            if (_currentWeaponData != null)
            {
                int totalAmmo = GetTotalAmmo(_currentWeaponData.AmmoType);
                OnAmmoChanged?.Invoke(_currentAmmoInMagazine, totalAmmo);
            }
        }

        /// <summary>
        /// Interface for health components
        /// </summary>
        public interface IHealthInterface
        {
            void TakeDamage(float damage);
        }
    }
}


