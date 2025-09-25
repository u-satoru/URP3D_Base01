using UnityEngine;
using System.Collections.Generic;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Features.Templates.TPS.Combat.Data;

namespace asterivo.Unity60.Features.Templates.TPS.Player
{
    /// <summary>
    /// TPS Player Inventory - Manages player inventory system
    /// Integrates with ServiceLocator pattern and event-driven architecture
    /// </summary>
    public class TPSPlayerInventory : MonoBehaviour
    {
        [Header("Inventory Configuration")]
        [SerializeField] private int _maxInventorySlots = 20;
        [SerializeField] private List<WeaponData> _startingWeapons = new List<WeaponData>();

        [Header("Current Inventory")]
        [SerializeField] private List<WeaponData> _weapons = new List<WeaponData>();
        [SerializeField] private int _currentWeaponIndex = 0;

        [Header("Audio Configuration")]
        [SerializeField] private AudioClip _inventoryPickupSound;
        [SerializeField] private AudioClip _weaponSwitchSound;

        // ServiceLocator-managed dependencies
        private IAudioManager _audioManager;

        // Properties
        public int MaxSlots => _maxInventorySlots;
        public List<WeaponData> Weapons => _weapons;
        public WeaponData CurrentWeapon => _weapons.Count > 0 && _currentWeaponIndex < _weapons.Count ? _weapons[_currentWeaponIndex] : null;
        public int CurrentWeaponIndex => _currentWeaponIndex;

        // Events
        public System.Action<WeaponData> OnWeaponAdded;
        public System.Action<WeaponData> OnWeaponRemoved;
        public System.Action<WeaponData> OnWeaponSwitched;

        private void Awake()
        {
            InitializeInventory();
        }

        private void Start()
        {
            // Initialize ServiceLocator dependencies
            _audioManager = ServiceLocator.GetService<IAudioManager>();

            // Add starting weapons
            foreach (var weapon in _startingWeapons)
            {
                if (weapon != null)
                {
                    AddWeapon(weapon);
                }
            }
        }

        private void InitializeInventory()
        {
            _weapons = new List<WeaponData>();
        }

        /// <summary>
        /// Add a weapon to the inventory
        /// </summary>
        public bool AddWeapon(WeaponData weapon)
        {
            if (weapon == null) return false;
            if (_weapons.Count >= _maxInventorySlots) return false;

            _weapons.Add(weapon);
            OnWeaponAdded?.Invoke(weapon);

            // Play sound effect
            if (_audioManager != null && _inventoryPickupSound != null)
                _audioManager.PlaySFX(_inventoryPickupSound);

            return true;
        }

        /// <summary>
        /// Remove a weapon from the inventory
        /// </summary>
        public bool RemoveWeapon(WeaponData weapon)
        {
            if (weapon == null) return false;

            int index = _weapons.IndexOf(weapon);
            if (index == -1) return false;

            _weapons.RemoveAt(index);

            // Adjust current weapon index if needed
            if (_currentWeaponIndex >= _weapons.Count)
            {
                _currentWeaponIndex = Mathf.Max(0, _weapons.Count - 1);
            }

            OnWeaponRemoved?.Invoke(weapon);
            return true;
        }

        /// <summary>
        /// Switch to next weapon
        /// </summary>
        public void SwitchToNextWeapon()
        {
            if (_weapons.Count <= 1) return;

            _currentWeaponIndex = (_currentWeaponIndex + 1) % _weapons.Count;
            OnWeaponSwitched?.Invoke(CurrentWeapon);

            // Play sound effect
            if (_audioManager != null && _weaponSwitchSound != null)
                _audioManager.PlaySFX(_weaponSwitchSound);
        }

        /// <summary>
        /// Switch to previous weapon
        /// </summary>
        public void SwitchToPreviousWeapon()
        {
            if (_weapons.Count <= 1) return;

            _currentWeaponIndex = (_currentWeaponIndex - 1 + _weapons.Count) % _weapons.Count;
            OnWeaponSwitched?.Invoke(CurrentWeapon);

            // Play sound effect
            if (_audioManager != null && _weaponSwitchSound != null)
                _audioManager.PlaySFX(_weaponSwitchSound);
        }

        /// <summary>
        /// Switch to specific weapon by index
        /// </summary>
        public bool SwitchToWeapon(int index)
        {
            if (index < 0 || index >= _weapons.Count) return false;

            _currentWeaponIndex = index;
            OnWeaponSwitched?.Invoke(CurrentWeapon);

            // Play sound effect
            if (_audioManager != null && _weaponSwitchSound != null)
                _audioManager.PlaySFX(_weaponSwitchSound);

            return true;
        }

        /// <summary>
        /// Check if weapon exists in inventory
        /// </summary>
        public bool HasWeapon(WeaponData weapon)
        {
            return _weapons.Contains(weapon);
        }

        /// <summary>
        /// Get weapon count
        /// </summary>
        public int GetWeaponCount()
        {
            return _weapons.Count;
        }
    }
}
