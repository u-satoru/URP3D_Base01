using System;
using UnityEngine;

namespace asterivo.Unity60.Features.Templates.FPS.Services
{
    /// <summary>
    /// 武器管理サービス実装
    /// ServiceLocator + Event駆動のハイブリッドアーキテクチャ準拠
    /// </summary>
    public class WeaponManager : IWeaponManager
    {
        private IWeaponData[] _weapons;
        private int _currentWeaponIndex = 0;
        private bool _isReloading = false;
        private float _lastFireTime = 0f;

        // プロパティ実装
        public IWeaponData CurrentWeapon =>
            _weapons != null && _currentWeaponIndex < _weapons.Length ? _weapons[_currentWeaponIndex] : null;

        public bool CanShoot =>
            CurrentWeapon != null &&
            CurrentWeapon.CurrentAmmo > 0 &&
            !_isReloading &&
            Time.time - _lastFireTime >= (60f / CurrentWeapon.FireRate);

        public bool CanReload =>
            CurrentWeapon != null &&
            !_isReloading &&
            CurrentWeapon.CurrentAmmo < CurrentWeapon.MaxAmmo &&
            CurrentWeapon.ReserveAmmo > 0;

        // イベント
        public event Action<IWeaponData> OnWeaponSwitched;
        public event Action<int> OnAmmoChanged;

        // 初期化
        public void InitializeWeapons(IWeaponData[] weapons)
        {
            _weapons = weapons;
            if (_weapons != null && _weapons.Length > 0)
            {
                _currentWeaponIndex = 0;
                OnWeaponSwitched?.Invoke(CurrentWeapon);
                OnAmmoChanged?.Invoke(CurrentWeapon.CurrentAmmo);
            }
        }

        // 武器切り替え
        public void SwitchWeapon(int weaponIndex)
        {
            if (_weapons == null || weaponIndex < 0 || weaponIndex >= _weapons.Length)
            {
                Debug.LogWarning($"[WeaponManager] Invalid weapon index: {weaponIndex}");
                return;
            }

            if (_currentWeaponIndex != weaponIndex)
            {
                _currentWeaponIndex = weaponIndex;
                _isReloading = false; // 武器切り替え時はリロード停止

                OnWeaponSwitched?.Invoke(CurrentWeapon);
                OnAmmoChanged?.Invoke(CurrentWeapon.CurrentAmmo);

                Debug.Log($"[WeaponManager] Switched to weapon: {CurrentWeapon.WeaponName}");
            }
        }

        // 射撃
        public void Fire(Vector3 origin, Vector3 direction)
        {
            if (!CanShoot)
            {
                Debug.LogWarning("[WeaponManager] Cannot fire weapon");
                return;
            }

            // 弾薬消費
            CurrentWeapon.SetAmmo(CurrentWeapon.CurrentAmmo - 1);
            _lastFireTime = Time.time;

            // ServiceLocator経由でオーディオサービス取得
            var audioService = asterivo.Unity60.Core.ServiceLocator.GetService<asterivo.Unity60.Core.Audio.Interfaces.IAudioService>();
            audioService?.PlaySFX(CurrentWeapon.FireSound, origin);

            // 武器データの射撃メソッド実行
            CurrentWeapon.FireWeapon(origin, direction);

            // イベント通知
            OnAmmoChanged?.Invoke(CurrentWeapon.CurrentAmmo);

            Debug.Log($"[WeaponManager] Fired {CurrentWeapon.WeaponName}. Ammo: {CurrentWeapon.CurrentAmmo}/{CurrentWeapon.MaxAmmo}");
        }

        // リロード開始
        public void StartReload()
        {
            if (!CanReload)
            {
                Debug.LogWarning("[WeaponManager] Cannot reload weapon");
                return;
            }

            _isReloading = true;

            // リロード音再生
            var audioService = asterivo.Unity60.Core.ServiceLocator.GetService<asterivo.Unity60.Core.Audio.Interfaces.IAudioService>();
            audioService?.PlaySFX(CurrentWeapon.ReloadSound, Vector3.zero);

            Debug.Log($"[WeaponManager] Started reloading {CurrentWeapon.WeaponName}");

            // リロード完了の遅延処理（実際の実装ではコルーチンやUniTaskを使用）
            _ = PerformReloadAsync();
        }

        private async System.Threading.Tasks.Task PerformReloadAsync()
        {
            try
            {
                // リロード時間待機
                await System.Threading.Tasks.Task.Delay(Mathf.RoundToInt(CurrentWeapon.ReloadTime * 1000));

                if (CurrentWeapon != null && _isReloading)
                {
                    // 弾薬計算
                    int ammoNeeded = CurrentWeapon.MaxAmmo - CurrentWeapon.CurrentAmmo;
                    int ammoToReload = Mathf.Min(ammoNeeded, CurrentWeapon.ReserveAmmo);

                    // リロード実行
                    CurrentWeapon.SetAmmo(CurrentWeapon.CurrentAmmo + ammoToReload);

                    _isReloading = false;

                    // イベント通知
                    OnAmmoChanged?.Invoke(CurrentWeapon.CurrentAmmo);

                    Debug.Log($"[WeaponManager] Reloaded {CurrentWeapon.WeaponName}. Ammo: {CurrentWeapon.CurrentAmmo}/{CurrentWeapon.MaxAmmo}");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[WeaponManager] Reload failed: {ex.Message}");
                _isReloading = false;
            }
        }
    }
}