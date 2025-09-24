using System;
using UnityEngine;
using UnityEngine.UI;
using asterivo.Unity60.Core;
using asterivo.Unity60.Features.Templates.FPS.Services;

namespace asterivo.Unity60.Features.Templates.FPS.Services
{
    /// <summary>
    /// FPS UI Service Implementation：一人称シューティング特化のUI管理実装
    /// ServiceLocator + Event駆動アーキテクチャによる高度なUI管理システム
    /// 詳細設計書準拠：HUD更新・弾薬表示・ヘルスバー・クロスヘア・スコア表示
    /// </summary>
    public class FPSUIService : MonoBehaviour, IFPSUIService
    {
        [Header("FPS UI Components")]
        [SerializeField] private Canvas _gameUICanvas;
        [SerializeField] private Canvas _pauseMenuCanvas;
        [SerializeField] private Canvas _gameOverCanvas;

        [Header("HUD Elements")]
        [SerializeField] private Slider _healthBar;
        [SerializeField] private Text _ammoText;
        [SerializeField] private Text _weaponNameText;
        [SerializeField] private Text _fireModeText;
        [SerializeField] private Text _scoreText;
        [SerializeField] private Text _killCountText;
        [SerializeField] private Text _timerText;

        [Header("Crosshair Elements")]
        [SerializeField] private GameObject _crosshairContainer;
        [SerializeField] private Image _crosshairImage;
        [SerializeField] private RectTransform _hitMarker;

        [Header("Feedback Elements")]
        [SerializeField] private GameObject _reloadIndicator;
        [SerializeField] private Slider _reloadProgressBar;
        [SerializeField] private Text _alertMessageText;
        [SerializeField] private Text _objectiveText;

        [Header("ServiceLocator Integration")]
        [SerializeField] private bool _enableServiceLocatorIntegration = true;
        [SerializeField] private bool _enableDebugLogging = false;

        // Service State
        public bool IsInitialized { get; private set; }
        public bool IsEnabled { get; private set; }

        // Events - Core UI
        public event Action OnGameUIShown;
        public event Action OnGameUIHidden;
        public event Action OnPauseMenuShown;
        public event Action OnPauseMenuHidden;
        public event Action OnGameOverShown;
        public event Action OnVictoryShown;

        // Events - HUD Updates
        public event Action<int, int> OnHealthChanged;
        public event Action<int, int> OnAmmoChanged;
        public event Action<string, string> OnWeaponChanged;
        public event Action<int> OnScoreChanged;
        public event Action<int, int> OnKillCountChanged;

        // Events - Button Actions
        public event Action OnResumeRequested;
        public event Action OnRestartRequested;
        public event Action OnMainMenuRequested;
        public event Action OnSettingsRequested;
        public event Action OnQuitRequested;

        #region Unity Lifecycle

        private void Awake()
        {
            Initialize();
        }

        private void Start()
        {
            RegisterWithServiceLocator();
        }

        private void OnDestroy()
        {
            Dispose();
        }

        #endregion

        #region IFPSService Implementation

        public void Initialize()
        {
            if (IsInitialized) return;

            ValidateUIComponents();
            SetupInitialUIState();

            IsInitialized = true;
            IsEnabled = true;

            if (_enableDebugLogging)
            {
                Debug.Log("[FPSUIService] Service initialized successfully");
            }
        }

        public void Enable()
        {
            IsEnabled = true;
            if (_gameUICanvas != null)
                _gameUICanvas.gameObject.SetActive(true);
        }

        public void Disable()
        {
            IsEnabled = false;
            HideAllScreens();
        }

        public void Reset()
        {
            HideAllScreens();
            ResetHUDValues();

            if (_enableDebugLogging)
            {
                Debug.Log("[FPSUIService] Service reset completed");
            }
        }

        public bool VerifyServiceLocatorIntegration()
        {
            if (!_enableServiceLocatorIntegration) return true;

            var registeredService = ServiceLocator.GetService<IFPSUIService>();
            bool isRegistered = registeredService == this;

            if (_enableDebugLogging)
            {
                Debug.Log($"[FPSUIService] ServiceLocator integration verified: {isRegistered}");
            }

            return isRegistered;
        }

        public void UpdateService(float deltaTime)
        {
            // フレーム更新が必要な場合はここで実装
            // 現在は基本的なUI更新のみなので空実装
        }

        public void Dispose()
        {
            if (_enableServiceLocatorIntegration)
            {
                try
                {
                    ServiceLocator.UnregisterService<IFPSUIService>();
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[FPSUIService] Failed to unregister from ServiceLocator: {ex.Message}");
                }
            }

            IsInitialized = false;
            IsEnabled = false;
        }

        #endregion

        #region Core UI Management

        public void ShowGameUI()
        {
            if (_gameUICanvas != null)
            {
                _gameUICanvas.gameObject.SetActive(true);
                OnGameUIShown?.Invoke();
            }
        }

        public void HideGameUI()
        {
            if (_gameUICanvas != null)
            {
                _gameUICanvas.gameObject.SetActive(false);
                OnGameUIHidden?.Invoke();
            }
        }

        public void ShowPauseMenu()
        {
            if (_pauseMenuCanvas != null)
            {
                _pauseMenuCanvas.gameObject.SetActive(true);
                OnPauseMenuShown?.Invoke();
            }
        }

        public void HidePauseMenu()
        {
            if (_pauseMenuCanvas != null)
            {
                _pauseMenuCanvas.gameObject.SetActive(false);
                OnPauseMenuHidden?.Invoke();
            }
        }

        public void ShowGameOverScreen()
        {
            if (_gameOverCanvas != null)
            {
                _gameOverCanvas.gameObject.SetActive(true);
                OnGameOverShown?.Invoke();
            }
        }

        public void ShowVictoryScreen()
        {
            // Victory screen implementation
            OnVictoryShown?.Invoke();
        }

        public void HideAllScreens()
        {
            HideGameUI();
            HidePauseMenu();
            if (_gameOverCanvas != null)
                _gameOverCanvas.gameObject.SetActive(false);
        }

        #endregion

        #region HUD Updates

        public void UpdateHealthBar(int currentHealth, int maxHealth)
        {
            if (_healthBar != null)
            {
                _healthBar.value = (float)currentHealth / maxHealth;
                OnHealthChanged?.Invoke(currentHealth, maxHealth);
            }
        }

        public void UpdateAmmoCount(int currentAmmo, int totalAmmo)
        {
            if (_ammoText != null)
            {
                _ammoText.text = $"{currentAmmo}/{totalAmmo}";
                OnAmmoChanged?.Invoke(currentAmmo, totalAmmo);
            }
        }

        public void UpdateWeaponInfo(string weaponName, string fireMode)
        {
            if (_weaponNameText != null)
                _weaponNameText.text = weaponName;

            if (_fireModeText != null)
                _fireModeText.text = fireMode;

            OnWeaponChanged?.Invoke(weaponName, fireMode);
        }

        public void UpdateScore(int score)
        {
            if (_scoreText != null)
            {
                _scoreText.text = $"Score: {score}";
                OnScoreChanged?.Invoke(score);
            }
        }

        public void UpdateKillCount(int kills, int deaths)
        {
            if (_killCountText != null)
            {
                _killCountText.text = $"K/D: {kills}/{deaths}";
                OnKillCountChanged?.Invoke(kills, deaths);
            }
        }

        public void UpdateTimer(float timeInSeconds)
        {
            if (_timerText != null)
            {
                int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
                int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
                _timerText.text = $"{minutes:00}:{seconds:00}";
            }
        }

        #endregion

        #region Crosshair & Aiming UI

        public void ShowCrosshair()
        {
            if (_crosshairContainer != null)
                _crosshairContainer.SetActive(true);
        }

        public void HideCrosshair()
        {
            if (_crosshairContainer != null)
                _crosshairContainer.SetActive(false);
        }

        public void SetCrosshairStyle(CrosshairStyle style)
        {
            // クロスヘアスタイル変更実装
            // スタイルに応じてスプライトやマテリアルを変更
        }

        public void UpdateCrosshairAccuracy(float accuracy)
        {
            // 精度に応じてクロスヘアのサイズや色を調整
            if (_crosshairImage != null)
            {
                float scale = Mathf.Lerp(1.5f, 0.8f, accuracy);
                _crosshairImage.transform.localScale = Vector3.one * scale;
            }
        }

        #endregion

        #region Hit Feedback UI

        public void ShowHitMarker(bool isHeadshot = false)
        {
            if (_hitMarker != null)
            {
                // ヒットマーカー表示ロジック
                _hitMarker.gameObject.SetActive(true);
                // 一定時間後に非表示にするコルーチンを開始
                StartCoroutine(HideHitMarkerAfterDelay(0.2f));
            }
        }

        public void ShowDamageNumber(float damage, Vector3 worldPosition)
        {
            // ダメージ数値表示実装
            // 世界座標をスクリーン座標に変換してダメージテキストを表示
        }

        public void FlashDamageIndicator(Vector3 damageDirection)
        {
            // ダメージ方向インジケーター表示実装
        }

        #endregion

        #region Weapon & Reload UI

        public void ShowReloadIndicator()
        {
            if (_reloadIndicator != null)
                _reloadIndicator.SetActive(true);
        }

        public void HideReloadIndicator()
        {
            if (_reloadIndicator != null)
                _reloadIndicator.SetActive(false);
        }

        public void UpdateReloadProgress(float progress)
        {
            if (_reloadProgressBar != null)
                _reloadProgressBar.value = progress;
        }

        public void ShowWeaponSwitchIndicator(string weaponName)
        {
            // 武器切り替えインジケーター表示
            if (_enableDebugLogging)
            {
                Debug.Log($"[FPSUIService] Weapon switched to: {weaponName}");
            }
        }

        #endregion

        #region Objective & Alert UI

        public void ShowObjectiveUpdate(string objective)
        {
            if (_objectiveText != null)
                _objectiveText.text = objective;
        }

        public void ShowAlertMessage(string message, AlertType alertType)
        {
            if (_alertMessageText != null)
            {
                _alertMessageText.text = message;
                // アラートタイプに応じて色や表示時間を調整
            }
        }

        public void UpdateObjectiveProgress(string objective, float progress)
        {
            ShowObjectiveUpdate($"{objective} ({progress:P0})");
        }

        #endregion

        #region Debug & Diagnostics

        public void ShowUIDebugInfo()
        {
            if (_enableDebugLogging)
            {
                Debug.Log($"[FPSUIService] Debug Info - Initialized: {IsInitialized}, Enabled: {IsEnabled}");
                Debug.Log($"[FPSUIService] ServiceLocator Integration: {VerifyServiceLocatorIntegration()}");
            }
        }

        public void ShowPerformanceMetrics(bool show)
        {
            // パフォーマンスメトリクス表示切り替え
        }

        #endregion

        #region Private Methods

        private void RegisterWithServiceLocator()
        {
            if (_enableServiceLocatorIntegration)
            {
                ServiceLocator.RegisterService<IFPSUIService>(this);

                if (_enableDebugLogging)
                {
                    Debug.Log("[FPSUIService] Registered with ServiceLocator");
                }
            }
        }

        private void ValidateUIComponents()
        {
            // UI コンポーネントの検証
            if (_gameUICanvas == null)
                Debug.LogWarning("[FPSUIService] Game UI Canvas is not assigned");
        }

        private void SetupInitialUIState()
        {
            ShowGameUI();
            HidePauseMenu();
            ShowCrosshair();

            // 初期値設定
            ResetHUDValues();
        }

        private void ResetHUDValues()
        {
            UpdateHealthBar(100, 100);
            UpdateAmmoCount(30, 120);
            UpdateWeaponInfo("Assault Rifle", "Auto");
            UpdateScore(0);
            UpdateKillCount(0, 0);
            UpdateTimer(0f);
        }

        private System.Collections.IEnumerator HideHitMarkerAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (_hitMarker != null)
                _hitMarker.gameObject.SetActive(false);
        }

        #endregion
    }
}
