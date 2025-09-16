using System;
using UnityEngine;
using UnityEngine.UI;

namespace asterivo.Unity60.Features.Templates.FPS.Services
{
    /// <summary>
    /// FPS UI管理サービス実装
    /// ObjectPool統合UI要素管理 + ServiceLocator + Event駆動のハイブリッドアーキテクチャ準拠
    /// </summary>
    public class FPSUIService : IFPSUIService
    {
        // UI要素参照
        private GameObject _crosshair;
        private GameObject _ammoCounter;
        private GameObject _healthBar;
        private GameObject _weaponInfo;

        // UI表示状態
        private bool _crosshairVisible = true;
        private bool _ammoCounterVisible = true;
        private bool _healthBarVisible = true;
        private bool _weaponInfoVisible = true;

        // UI設定
        private FPSUIConfiguration _currentConfig;

        // イベント
        public event Action<bool> OnCrosshairVisibilityChanged;
        public event Action<float> OnHealthBarUpdated;

        // UI要素表示/非表示制御
        public void ShowCrosshair(bool show)
        {
            _crosshairVisible = show;

            if (_crosshair != null)
            {
                _crosshair.SetActive(show);
            }

            OnCrosshairVisibilityChanged?.Invoke(show);
            Debug.Log($"[FPSUIService] Crosshair visibility: {show}");
        }

        public void ShowAmmoCounter(bool show)
        {
            _ammoCounterVisible = show;

            if (_ammoCounter != null)
            {
                _ammoCounter.SetActive(show);
            }

            Debug.Log($"[FPSUIService] Ammo counter visibility: {show}");
        }

        public void ShowHealthBar(bool show)
        {
            _healthBarVisible = show;

            if (_healthBar != null)
            {
                _healthBar.SetActive(show);
            }

            Debug.Log($"[FPSUIService] Health bar visibility: {show}");
        }

        public void ShowWeaponInfo(bool show)
        {
            _weaponInfoVisible = show;

            if (_weaponInfo != null)
            {
                _weaponInfo.SetActive(show);
            }

            Debug.Log($"[FPSUIService] Weapon info visibility: {show}");
        }

        // 動的UI要素更新
        public void UpdateAmmoDisplay(int currentAmmo, int maxAmmo, int reserveAmmo)
        {
            if (_ammoCounter != null)
            {
                var ammoText = _ammoCounter.GetComponentInChildren<Text>();
                if (ammoText != null)
                {
                    ammoText.text = $"{currentAmmo}/{maxAmmo} ({reserveAmmo})";

                    // 弾薬不足時の色変更
                    if (currentAmmo <= 0)
                    {
                        ammoText.color = Color.red;
                    }
                    else if (currentAmmo <= maxAmmo * 0.25f)
                    {
                        ammoText.color = Color.yellow;
                    }
                    else
                    {
                        ammoText.color = _currentConfig?.ammoCounterColor ?? Color.white;
                    }
                }
            }

            Debug.Log($"[FPSUIService] Ammo display updated: {currentAmmo}/{maxAmmo} ({reserveAmmo})");
        }

        public void UpdateHealthDisplay(float currentHealth, float maxHealth)
        {
            if (_healthBar != null)
            {
                var healthSlider = _healthBar.GetComponent<Slider>();
                if (healthSlider != null)
                {
                    healthSlider.value = currentHealth / maxHealth;

                    // ヘルス低下時の色変更
                    var fillImage = healthSlider.fillRect?.GetComponent<Image>();
                    if (fillImage != null)
                    {
                        float healthRatio = currentHealth / maxHealth;
                        if (healthRatio <= 0.25f)
                        {
                            fillImage.color = Color.red;
                        }
                        else if (healthRatio <= 0.5f)
                        {
                            fillImage.color = Color.yellow;
                        }
                        else
                        {
                            fillImage.color = _currentConfig?.healthBarColor ?? Color.green;
                        }
                    }
                }

                var healthText = _healthBar.GetComponentInChildren<Text>();
                if (healthText != null)
                {
                    healthText.text = $"{currentHealth:F0}/{maxHealth:F0}";
                }
            }

            OnHealthBarUpdated?.Invoke(currentHealth / maxHealth);
            Debug.Log($"[FPSUIService] Health display updated: {currentHealth}/{maxHealth}");
        }

        public void UpdateWeaponDisplay(string weaponName, WeaponType weaponType)
        {
            if (_weaponInfo != null)
            {
                var weaponNameText = _weaponInfo.GetComponentInChildren<Text>();
                if (weaponNameText != null)
                {
                    weaponNameText.text = weaponName;
                }

                // 武器タイプ別のアイコン表示（アイコンがある場合）
                var weaponIcon = _weaponInfo.GetComponentInChildren<Image>();
                if (weaponIcon != null)
                {
                    // ServiceLocator経由でデータサービス取得
                    // var dataService = asterivo.Unity60.Core.ServiceLocator.GetService<IFPSDataService>();
                    // weaponIcon.sprite = dataService?.GetWeaponIcon(weaponType);
                }
            }

            Debug.Log($"[FPSUIService] Weapon display updated: {weaponName} ({weaponType})");
        }

        public void UpdateCrosshair(CrosshairState state)
        {
            if (_crosshair != null)
            {
                var crosshairImage = _crosshair.GetComponent<Image>();
                if (crosshairImage != null)
                {
                    Color crosshairColor = _currentConfig?.crosshairColor ?? Color.white;

                    switch (state)
                    {
                        case CrosshairState.Normal:
                            crosshairImage.color = crosshairColor;
                            crosshairImage.transform.localScale = Vector3.one;
                            break;

                        case CrosshairState.Aiming:
                            crosshairImage.color = crosshairColor;
                            crosshairImage.transform.localScale = Vector3.one * 0.7f; // 照準時は縮小
                            break;

                        case CrosshairState.Firing:
                            crosshairImage.color = Color.yellow;
                            crosshairImage.transform.localScale = Vector3.one * 1.2f; // 発砲時は拡大
                            break;

                        case CrosshairState.Reloading:
                            crosshairImage.color = Color.gray;
                            crosshairImage.transform.localScale = Vector3.one;
                            break;

                        case CrosshairState.Hidden:
                            ShowCrosshair(false);
                            return;
                    }

                    ShowCrosshair(true);
                }
            }

            Debug.Log($"[FPSUIService] Crosshair updated: {state}");
        }

        // ダメージ・エフェクトUI（ObjectPool活用）
        public void ShowDamageIndicator(Vector3 damageSource, float damageAmount)
        {
            // ObjectPool経由でダメージインジケーター取得
            var damageIndicator = CreateUIElement<DamageIndicator>();
            if (damageIndicator != null)
            {
                damageIndicator.ShowDamage(damageSource, damageAmount);

                // 数秒後にプールに返却
                _ = System.Threading.Tasks.Task.Delay(2000).ContinueWith(_ => {
                    if (damageIndicator != null)
                    {
                        ReturnUIElement(damageIndicator);
                    }
                });
            }

            // ServiceLocator経由でAudioサービス取得
            var audioService = asterivo.Unity60.Core.ServiceLocator.GetService<asterivo.Unity60.Core.Audio.Interfaces.IAudioService>();
            audioService?.PlaySFX("DamageIndicator", Vector3.zero);

            Debug.Log($"[FPSUIService] Damage indicator shown: {damageAmount} from {damageSource}");
        }

        public void ShowHitMarker()
        {
            var hitMarker = CreateUIElement<HitMarker>();
            if (hitMarker != null)
            {
                hitMarker.ShowHit();

                // 短時間後にプールに返却
                _ = System.Threading.Tasks.Task.Delay(500).ContinueWith(_ => {
                    if (hitMarker != null)
                    {
                        ReturnUIElement(hitMarker);
                    }
                });
            }

            // ServiceLocator経由でAudioサービス取得
            var audioService = asterivo.Unity60.Core.ServiceLocator.GetService<asterivo.Unity60.Core.Audio.Interfaces.IAudioService>();
            audioService?.PlaySFX("HitMarker", Vector3.zero);

            Debug.Log("[FPSUIService] Hit marker shown");
        }

        public void ShowKillMarker()
        {
            var killMarker = CreateUIElement<KillMarker>();
            if (killMarker != null)
            {
                killMarker.ShowKill();

                // 短時間後にプールに返却
                _ = System.Threading.Tasks.Task.Delay(1000).ContinueWith(_ => {
                    if (killMarker != null)
                    {
                        ReturnUIElement(killMarker);
                    }
                });
            }

            // ServiceLocator経由でAudioサービス取得
            var audioService = asterivo.Unity60.Core.ServiceLocator.GetService<asterivo.Unity60.Core.Audio.Interfaces.IAudioService>();
            audioService?.PlaySFX("KillMarker", Vector3.zero);

            Debug.Log("[FPSUIService] Kill marker shown");
        }

        // UI要素生成（ObjectPool統合）
        public T CreateUIElement<T>() where T : Component
        {
            // ObjectPool統合実装予定
            // 現在は通常のInstantiate使用
            var prefab = GetUIElementPrefab<T>();
            if (prefab != null)
            {
                var instance = UnityEngine.Object.Instantiate(prefab);
                return instance.GetComponent<T>();
            }

            Debug.LogWarning($"[FPSUIService] UI element prefab not found for type: {typeof(T).Name}");
            return null;
        }

        public void ReturnUIElement<T>(T element) where T : Component
        {
            if (element != null)
            {
                // ObjectPool統合実装予定
                // 現在は通常のDestroy使用
                UnityEngine.Object.Destroy(element.gameObject);
            }
        }

        // UI設定適用
        public void ApplyUIConfiguration(FPSUIConfiguration config)
        {
            if (config == null)
            {
                Debug.LogWarning("[FPSUIService] UI configuration is null");
                return;
            }

            _currentConfig = config;

            // 表示設定適用
            ShowCrosshair(config.showCrosshair);
            ShowAmmoCounter(config.showAmmoCounter);
            ShowHealthBar(config.showHealthBar);
            ShowWeaponInfo(config.showWeaponInfo);

            // 色設定適用
            ApplyColorSettings(config);

            Debug.Log("[FPSUIService] UI configuration applied");
        }

        // プライベートメソッド
        private void ApplyColorSettings(FPSUIConfiguration config)
        {
            // クロスヘアの色設定
            if (_crosshair != null)
            {
                var crosshairImage = _crosshair.GetComponent<Image>();
                if (crosshairImage != null)
                {
                    crosshairImage.color = config.crosshairColor;
                }
            }

            // ヘルスバーの色設定
            if (_healthBar != null)
            {
                var healthSlider = _healthBar.GetComponent<Slider>();
                if (healthSlider?.fillRect != null)
                {
                    var fillImage = healthSlider.fillRect.GetComponent<Image>();
                    if (fillImage != null)
                    {
                        fillImage.color = config.healthBarColor;
                    }
                }
            }

            // 弾薬カウンターの色設定
            if (_ammoCounter != null)
            {
                var ammoText = _ammoCounter.GetComponentInChildren<Text>();
                if (ammoText != null)
                {
                    ammoText.color = config.ammoCounterColor;
                }
            }
        }

        private GameObject GetUIElementPrefab<T>() where T : Component
        {
            // プレハブリソース管理実装予定
            // Resources.Loadまたはアセット管理システム使用
            string prefabName = typeof(T).Name + "Prefab";
            return Resources.Load<GameObject>($"UI/FPS/{prefabName}");
        }

        // 初期化
        public void InitializeUI(Canvas uiCanvas)
        {
            if (uiCanvas == null)
            {
                Debug.LogError("[FPSUIService] UI Canvas is null");
                return;
            }

            // UI要素の検索・初期化
            FindUIElements(uiCanvas);

            Debug.Log("[FPSUIService] UI initialized");
        }

        private void FindUIElements(Canvas uiCanvas)
        {
            _crosshair = FindUIElement(uiCanvas, "Crosshair");
            _ammoCounter = FindUIElement(uiCanvas, "AmmoCounter");
            _healthBar = FindUIElement(uiCanvas, "HealthBar");
            _weaponInfo = FindUIElement(uiCanvas, "WeaponInfo");
        }

        private GameObject FindUIElement(Canvas canvas, string elementName)
        {
            var element = canvas.transform.Find(elementName);
            return element?.gameObject;
        }
    }

    // UI要素コンポーネント（ObjectPool対応）
    public class DamageIndicator : MonoBehaviour
    {
        public void ShowDamage(Vector3 source, float amount)
        {
            // ダメージインジケーター表示実装
        }
    }

    public class HitMarker : MonoBehaviour
    {
        public void ShowHit()
        {
            // ヒットマーカー表示実装
        }
    }

    public class KillMarker : MonoBehaviour
    {
        public void ShowKill()
        {
            // キルマーカー表示実装
        }
    }
}