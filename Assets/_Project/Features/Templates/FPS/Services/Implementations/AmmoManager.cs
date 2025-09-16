using System;
using System.Collections.Generic;
using UnityEngine;

namespace asterivo.Unity60.Features.Templates.FPS.Services
{
    /// <summary>
    /// 弾薬管理サービス実装
    /// ServiceLocator + Event駆動のハイブリッドアーキテクチャ準拠
    /// </summary>
    public class AmmoManager : IAmmoManager
    {
        private readonly Dictionary<WeaponType, AmmoData> _ammoData = new();
        private readonly Dictionary<WeaponType, AmmoConfiguration> _configurations = new();

        // イベント
        public event Action<WeaponType, int, int> OnAmmoChanged; // weaponType, current, reserve
        public event Action<WeaponType> OnAmmoEmpty;
        public event Action<WeaponType> OnReloadStarted;
        public event Action<WeaponType> OnReloadCompleted;

        // 弾薬状態
        public int GetCurrentAmmo(WeaponType weaponType)
        {
            return _ammoData.TryGetValue(weaponType, out var data) ? data.CurrentAmmo : 0;
        }

        public int GetMaxAmmo(WeaponType weaponType)
        {
            return _configurations.TryGetValue(weaponType, out var config) ? config.maxAmmo : 0;
        }

        public int GetReserveAmmo(WeaponType weaponType)
        {
            return _ammoData.TryGetValue(weaponType, out var data) ? data.ReserveAmmo : 0;
        }

        public bool CanReload(WeaponType weaponType)
        {
            if (!_ammoData.TryGetValue(weaponType, out var data) ||
                !_configurations.TryGetValue(weaponType, out var config))
            {
                return false;
            }

            return data.CurrentAmmo < config.maxAmmo && data.ReserveAmmo > 0;
        }

        // 弾薬操作
        public void ConsumeAmmo(WeaponType weaponType, int amount = 1)
        {
            if (!_ammoData.TryGetValue(weaponType, out var data))
            {
                Debug.LogWarning($"[AmmoManager] No ammo data found for weapon type: {weaponType}");
                return;
            }

            if (amount <= 0)
            {
                Debug.LogWarning($"[AmmoManager] Invalid ammo consumption amount: {amount}");
                return;
            }

            int previousAmmo = data.CurrentAmmo;
            data.CurrentAmmo = Mathf.Max(0, data.CurrentAmmo - amount);

            // 弾薬変更イベント発行
            OnAmmoChanged?.Invoke(weaponType, data.CurrentAmmo, data.ReserveAmmo);

            // 弾薬切れイベント発行
            if (data.CurrentAmmo == 0 && previousAmmo > 0)
            {
                OnAmmoEmpty?.Invoke(weaponType);

                // ServiceLocator経由でAudioサービス取得
                var audioService = asterivo.Unity60.Core.ServiceLocator.GetService<asterivo.Unity60.Core.Audio.Interfaces.IAudioService>();
                audioService?.PlaySFX("AmmoEmpty", Vector3.zero);
            }

            Debug.Log($"[AmmoManager] Consumed {amount} ammo for {weaponType}. Current: {data.CurrentAmmo}");
        }

        public void Reload(WeaponType weaponType)
        {
            if (!CanReload(weaponType))
            {
                Debug.LogWarning($"[AmmoManager] Cannot reload weapon type: {weaponType}");
                return;
            }

            if (!_ammoData.TryGetValue(weaponType, out var data) ||
                !_configurations.TryGetValue(weaponType, out var config))
            {
                Debug.LogError($"[AmmoManager] Missing data for weapon type: {weaponType}");
                return;
            }

            // リロード開始イベント発行
            OnReloadStarted?.Invoke(weaponType);

            // ServiceLocator経由でAudioサービス取得
            var audioService = asterivo.Unity60.Core.ServiceLocator.GetService<asterivo.Unity60.Core.Audio.Interfaces.IAudioService>();
            audioService?.PlaySFX("ReloadStart", Vector3.zero);

            // リロード処理（非同期）
            _ = PerformReloadAsync(weaponType, data, config);

            Debug.Log($"[AmmoManager] Started reloading {weaponType}");
        }

        public void AddAmmo(WeaponType weaponType, int amount)
        {
            if (amount <= 0)
            {
                Debug.LogWarning($"[AmmoManager] Invalid ammo addition amount: {amount}");
                return;
            }

            if (!_ammoData.TryGetValue(weaponType, out var data))
            {
                Debug.LogWarning($"[AmmoManager] No ammo data found for weapon type: {weaponType}");
                return;
            }

            if (!_configurations.TryGetValue(weaponType, out var config))
            {
                Debug.LogWarning($"[AmmoManager] No configuration found for weapon type: {weaponType}");
                return;
            }

            // 予備弾薬に追加（上限チェック）
            data.ReserveAmmo = Mathf.Min(config.maxReserveAmmo, data.ReserveAmmo + amount);

            // 弾薬変更イベント発行
            OnAmmoChanged?.Invoke(weaponType, data.CurrentAmmo, data.ReserveAmmo);

            // ServiceLocator経由でAudioサービス取得
            var audioService = asterivo.Unity60.Core.ServiceLocator.GetService<asterivo.Unity60.Core.Audio.Interfaces.IAudioService>();
            audioService?.PlaySFX("AmmoPickup", Vector3.zero);

            Debug.Log($"[AmmoManager] Added {amount} reserve ammo for {weaponType}. Reserve: {data.ReserveAmmo}");
        }

        public void SetAmmo(WeaponType weaponType, int current, int reserve)
        {
            if (!_ammoData.TryGetValue(weaponType, out var data))
            {
                Debug.LogWarning($"[AmmoManager] No ammo data found for weapon type: {weaponType}");
                return;
            }

            if (!_configurations.TryGetValue(weaponType, out var config))
            {
                Debug.LogWarning($"[AmmoManager] No configuration found for weapon type: {weaponType}");
                return;
            }

            // 値の範囲チェック
            data.CurrentAmmo = Mathf.Clamp(current, 0, config.maxAmmo);
            data.ReserveAmmo = Mathf.Clamp(reserve, 0, config.maxReserveAmmo);

            // 弾薬変更イベント発行
            OnAmmoChanged?.Invoke(weaponType, data.CurrentAmmo, data.ReserveAmmo);

            Debug.Log($"[AmmoManager] Set ammo for {weaponType}: {data.CurrentAmmo}/{data.ReserveAmmo}");
        }

        // 弾薬設定
        public void InitializeAmmo(AmmoConfiguration[] configurations)
        {
            if (configurations == null || configurations.Length == 0)
            {
                Debug.LogError("[AmmoManager] No ammo configurations provided");
                return;
            }

            _ammoData.Clear();
            _configurations.Clear();

            foreach (var config in configurations)
            {
                if (config == null)
                {
                    Debug.LogWarning("[AmmoManager] Null configuration found, skipping");
                    continue;
                }

                // 設定を保存
                _configurations[config.weaponType] = config;

                // 初期弾薬データを作成
                _ammoData[config.weaponType] = new AmmoData
                {
                    CurrentAmmo = config.startingAmmo,
                    ReserveAmmo = config.startingReserveAmmo
                };

                Debug.Log($"[AmmoManager] Initialized {config.weaponType}: {config.startingAmmo}/{config.startingReserveAmmo}");
            }

            Debug.Log($"[AmmoManager] Initialized {configurations.Length} weapon ammo configurations");
        }

        public void ResetAllAmmo()
        {
            foreach (var kvp in _configurations)
            {
                var weaponType = kvp.Key;
                var config = kvp.Value;

                if (_ammoData.TryGetValue(weaponType, out var data))
                {
                    data.CurrentAmmo = config.startingAmmo;
                    data.ReserveAmmo = config.startingReserveAmmo;

                    // 弾薬変更イベント発行
                    OnAmmoChanged?.Invoke(weaponType, data.CurrentAmmo, data.ReserveAmmo);
                }
            }

            Debug.Log("[AmmoManager] All ammo reset to starting values");
        }

        // プライベートメソッド
        private async System.Threading.Tasks.Task PerformReloadAsync(WeaponType weaponType, AmmoData data, AmmoConfiguration config)
        {
            try
            {
                // リロード時間待機
                float reloadTime = config.reloadTime * 1000f; // ミリ秒変換
                await System.Threading.Tasks.Task.Delay(Mathf.RoundToInt(reloadTime));

                // リロード実行
                int ammoNeeded = config.maxAmmo - data.CurrentAmmo;
                int ammoToReload = Mathf.Min(ammoNeeded, data.ReserveAmmo);

                data.CurrentAmmo += ammoToReload;
                data.ReserveAmmo -= ammoToReload;

                // リロード完了イベント発行
                OnReloadCompleted?.Invoke(weaponType);

                // 弾薬変更イベント発行
                OnAmmoChanged?.Invoke(weaponType, data.CurrentAmmo, data.ReserveAmmo);

                // ServiceLocator経由でAudioサービス取得
                var audioService = asterivo.Unity60.Core.ServiceLocator.GetService<asterivo.Unity60.Core.Audio.Interfaces.IAudioService>();
                audioService?.PlaySFX("ReloadComplete", Vector3.zero);

                Debug.Log($"[AmmoManager] Completed reloading {weaponType}. Current: {data.CurrentAmmo}, Reserve: {data.ReserveAmmo}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[AmmoManager] Reload failed for {weaponType}: {ex.Message}");
            }
        }

        // 統計・デバッグ情報
        public Dictionary<WeaponType, AmmoStatistics> GetAmmoStatistics()
        {
            var statistics = new Dictionary<WeaponType, AmmoStatistics>();

            foreach (var kvp in _ammoData)
            {
                var weaponType = kvp.Key;
                var data = kvp.Value;
                var config = _configurations.TryGetValue(weaponType, out var cfg) ? cfg : null;

                statistics[weaponType] = new AmmoStatistics
                {
                    WeaponType = weaponType,
                    CurrentAmmo = data.CurrentAmmo,
                    ReserveAmmo = data.ReserveAmmo,
                    MaxAmmo = config?.maxAmmo ?? 0,
                    MaxReserveAmmo = config?.maxReserveAmmo ?? 0,
                    AmmoPercentage = config != null ? (float)data.CurrentAmmo / config.maxAmmo : 0f,
                    ReservePercentage = config != null ? (float)data.ReserveAmmo / config.maxReserveAmmo : 0f
                };
            }

            return statistics;
        }
    }

    /// <summary>
    /// 弾薬データ（実行時管理用）
    /// </summary>
    [System.Serializable]
    public class AmmoData
    {
        public int CurrentAmmo;
        public int ReserveAmmo;
    }

    /// <summary>
    /// 弾薬統計情報
    /// </summary>
    [System.Serializable]
    public class AmmoStatistics
    {
        public WeaponType WeaponType;
        public int CurrentAmmo;
        public int ReserveAmmo;
        public int MaxAmmo;
        public int MaxReserveAmmo;
        public float AmmoPercentage;
        public float ReservePercentage;
    }
}