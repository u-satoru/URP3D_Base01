using UnityEngine;
using UnityEngine.UI;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Audio.Interfaces;
using asterivo.Unity60.Core.Debug;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Features.UI.Scripts
{
    /// <summary>
    /// オーディオ・ステルス設定UI コンポーネント
    /// Step 3.6 パターン2: UIコンポーネントでの段階的更新例
    /// </summary>
    public class AudioStealthSettingsUI : MonoBehaviour
    {
        [TabGroup("UI Settings", "Service Dependencies")]
        [Header("Migration Settings")]
        [SerializeField] private bool useServiceLocator = true;
        [SerializeField] private bool enableDebugLogs = true;

        [TabGroup("UI Settings", "UI References")]
        [Header("Audio Controls")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider bgmVolumeSlider;
        [SerializeField] private Slider effectVolumeSlider;
        [SerializeField] private Slider ambientVolumeSlider;
        
        [TabGroup("UI Settings", "UI References")]
        [Header("Stealth Controls")]
        [SerializeField] private Slider stealthSensitivitySlider;
        [SerializeField] private Slider environmentNoiseSlider;
        [SerializeField] private Toggle audioMaskingToggle;
        [SerializeField] private Button testDistractionButton;

        [TabGroup("UI Settings", "UI References")]
        [Header("Display Elements")]
        [SerializeField] private Text serviceStatusText;
        [SerializeField] private Text migrationStatusText;

        [TabGroup("UI Settings", "Runtime Info")]
        [Header("Runtime Information")]
        [SerializeField, ReadOnly] private bool isServiceAvailable;
        [SerializeField, ReadOnly] private string currentServiceType;
        [SerializeField, ReadOnly] private bool usingServiceLocator;

        // Service References
        private IAudioService audioService;
        private IStealthAudioService stealthAudioService;

        #region Unity Lifecycle

        private void Start()
        {
            InitializeAudioServices();
            SetupUICallbacks();
            UpdateUI();
        }

        #endregion

        #region Service Initialization

        /// <summary>
        /// オーディオサービスの段階的初期化
        /// Phase 3 計画書のパターン2実装
        /// </summary>
        private void InitializeAudioServices()
        {
            LogDebug("[AudioStealthSettingsUI] Initializing audio services...");

            // ServiceLocator優先、フォールバック付き
            if (useServiceLocator && FeatureFlags.UseServiceLocator)
            {
                InitializeWithServiceLocator();
            }
            else
            {
                InitializeWithLegacyFallback();
            }

            // UI状態の更新
            UpdateServiceStatus();
        }

        /// <summary>
        /// ServiceLocatorを使用した初期化
        /// </summary>
        private void InitializeWithServiceLocator()
        {
            try
            {
                audioService = ServiceLocator.GetService<IAudioService>();
                stealthAudioService = ServiceLocator.GetService<IStealthAudioService>();

                if (audioService != null)
                {
                    usingServiceLocator = true;
                    currentServiceType = audioService.GetType().Name;
                    isServiceAvailable = true;

                    LogDebug("[AudioStealthSettingsUI] Successfully initialized with ServiceLocator");
                }
                else
                {
                    LogDebug("[AudioStealthSettingsUI] ServiceLocator services not available, using fallback");
                    InitializeWithLegacyFallback();
                }
            }
            catch (System.Exception ex)
            {
                ServiceLocator.GetService<IEventLogger>()?.LogError($"[AudioStealthSettingsUI] ServiceLocator initialization failed: {ex.Message}");
                InitializeWithLegacyFallback();
            }
        }

        /// <summary>
        /// レガシーフォールバックでの初期化
        /// </summary>
        private void InitializeWithLegacyFallback()
        {
            if (FeatureFlags.AllowSingletonFallback)
            {
                try
                {
#pragma warning disable CS0618
                    // 従来のSingleton方式（フォールバック）
                    var legacyAudioManager = FindFirstObjectByType<asterivo.Unity60.Core.Audio.AudioManager>();
                    var legacyStealthCoordinator = FindFirstObjectByType<asterivo.Unity60.Core.Audio.StealthAudioCoordinator>();

                    if (legacyAudioManager != null)
                    {
                        audioService = legacyAudioManager;
                        currentServiceType = "AudioManager (Legacy)";
                        isServiceAvailable = true;
                    }

                    if (legacyStealthCoordinator != null)
                    {
                        stealthAudioService = legacyStealthCoordinator;
                    }
#pragma warning restore CS0618

                    usingServiceLocator = false;

                    if (FeatureFlags.EnableMigrationMonitoring)
                    {
                        ServiceLocator.GetService<IEventLogger>()?.LogWarning("[AudioStealthSettingsUI] Falling back to legacy AudioManager.Instance");
                    }
                }
                catch (System.Exception ex)
                {
                    ServiceLocator.GetService<IEventLogger>()?.LogError($"[AudioStealthSettingsUI] Legacy fallback failed: {ex.Message}");
                }
            }
            else
            {
                ServiceLocator.GetService<IEventLogger>()?.LogError("[AudioStealthSettingsUI] No audio service available and legacy singletons are disabled");
            }
        }

        /// <summary>
        /// サービス状態の更新
        /// </summary>
        private void UpdateServiceStatus()
        {
            if (serviceStatusText != null)
            {
                if (isServiceAvailable)
                {
                    serviceStatusText.text = $"Audio Service: {currentServiceType}";
                    serviceStatusText.color = Color.green;
                }
                else
                {
                    serviceStatusText.text = "Audio Service: Not Available";
                    serviceStatusText.color = Color.red;
                }
            }

            if (migrationStatusText != null)
            {
                string migrationStatus = usingServiceLocator ? "ServiceLocator" : "Legacy Singleton";
                migrationStatusText.text = $"Method: {migrationStatus}";
                migrationStatusText.color = usingServiceLocator ? Color.green : Color.yellow;
            }
        }

        #endregion

        #region UI Setup

        /// <summary>
        /// UIコールバックの設定
        /// </summary>
        private void SetupUICallbacks()
        {
            // Audio Volume Controls
            if (masterVolumeSlider != null)
                masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);

            if (bgmVolumeSlider != null)
                bgmVolumeSlider.onValueChanged.AddListener(SetBGMVolume);

            if (effectVolumeSlider != null)
                effectVolumeSlider.onValueChanged.AddListener(SetEffectVolume);

            if (ambientVolumeSlider != null)
                ambientVolumeSlider.onValueChanged.AddListener(SetAmbientVolume);

            // Stealth Controls
            if (stealthSensitivitySlider != null)
                stealthSensitivitySlider.onValueChanged.AddListener(SetStealthSensitivity);

            if (environmentNoiseSlider != null)
                environmentNoiseSlider.onValueChanged.AddListener(SetEnvironmentNoise);

            if (audioMaskingToggle != null)
                audioMaskingToggle.onValueChanged.AddListener(SetAudioMasking);

            if (testDistractionButton != null)
                testDistractionButton.onClick.AddListener(TestDistraction);
        }

        /// <summary>
        /// UI要素の現在値更新
        /// </summary>
        private void UpdateUI()
        {
            if (!isServiceAvailable || audioService == null) return;

            try
            {
                // Audio Volumesの現在値を取得してUIに反映
                if (masterVolumeSlider != null)
                    masterVolumeSlider.value = audioService.GetMasterVolume();

                if (bgmVolumeSlider != null)
                    bgmVolumeSlider.value = audioService.GetBGMVolume();

                if (effectVolumeSlider != null)
                    effectVolumeSlider.value = audioService.GetEffectVolume();

                if (ambientVolumeSlider != null)
                    ambientVolumeSlider.value = audioService.GetAmbientVolume();

                LogDebug("[AudioStealthSettingsUI] UI updated with current audio settings");
            }
            catch (System.Exception ex)
            {
                ServiceLocator.GetService<IEventLogger>()?.LogError($"[AudioStealthSettingsUI] Failed to update UI: {ex.Message}");
            }
        }

        #endregion

        #region Audio Controls

        /// <summary>
        /// マスター音量の設定
        /// </summary>
        public void SetMasterVolume(float volume)
        {
            if (audioService != null)
            {
                try
                {
                    audioService.SetMasterVolume(volume);
                    LogDebug($"[AudioStealthSettingsUI] Master volume set to: {volume:F2}");
                }
                catch (System.Exception ex)
                {
                    ServiceLocator.GetService<IEventLogger>()?.LogError($"[AudioStealthSettingsUI] Failed to set master volume: {ex.Message}");
                }
            }
            else
            {
                ServiceLocator.GetService<IEventLogger>()?.LogError("[AudioStealthSettingsUI] No audio service available");
            }
        }

        /// <summary>
        /// BGM音量の設定
        /// </summary>
        public void SetBGMVolume(float volume)
        {
            if (audioService != null)
            {
                try
                {
                    audioService.SetCategoryVolume("bgm", volume);
                    LogDebug($"[AudioStealthSettingsUI] BGM volume set to: {volume:F2}");
                }
                catch (System.Exception ex)
                {
                    ServiceLocator.GetService<IEventLogger>()?.LogError($"[AudioStealthSettingsUI] Failed to set BGM volume: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 効果音音量の設定
        /// </summary>
        public void SetEffectVolume(float volume)
        {
            if (audioService != null)
            {
                try
                {
                    audioService.SetCategoryVolume("effect", volume);
                    LogDebug($"[AudioStealthSettingsUI] Effect volume set to: {volume:F2}");
                }
                catch (System.Exception ex)
                {
                    ServiceLocator.GetService<IEventLogger>()?.LogError($"[AudioStealthSettingsUI] Failed to set effect volume: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 環境音音量の設定
        /// </summary>
        public void SetAmbientVolume(float volume)
        {
            if (audioService != null)
            {
                try
                {
                    audioService.SetCategoryVolume("ambient", volume);
                    LogDebug($"[AudioStealthSettingsUI] Ambient volume set to: {volume:F2}");
                }
                catch (System.Exception ex)
                {
                    ServiceLocator.GetService<IEventLogger>()?.LogError($"[AudioStealthSettingsUI] Failed to set ambient volume: {ex.Message}");
                }
            }
        }

        #endregion

        #region Stealth Controls

        /// <summary>
        /// ステルス感度の設定
        /// </summary>
        public void SetStealthSensitivity(float sensitivity)
        {
            if (stealthAudioService != null)
            {
                try
                {
                    stealthAudioService.AdjustStealthAudio(sensitivity);
                    LogDebug($"[AudioStealthSettingsUI] Stealth sensitivity set to: {sensitivity:F2}");
                }
                catch (System.Exception ex)
                {
                    ServiceLocator.GetService<IEventLogger>()?.LogError($"[AudioStealthSettingsUI] Failed to set stealth sensitivity: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 環境ノイズレベルの設定
        /// </summary>
        public void SetEnvironmentNoise(float noiseLevel)
        {
            if (stealthAudioService != null)
            {
                try
                {
                    stealthAudioService.SetEnvironmentNoiseLevel(noiseLevel);
                    LogDebug($"[AudioStealthSettingsUI] Environment noise level set to: {noiseLevel:F2}");
                }
                catch (System.Exception ex)
                {
                    ServiceLocator.GetService<IEventLogger>()?.LogError($"[AudioStealthSettingsUI] Failed to set environment noise: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// オーディオマスキングの切り替え
        /// </summary>
        public void SetAudioMasking(bool enabled)
        {
            if (stealthAudioService != null)
            {
                try
                {
                    float maskingLevel = enabled ? 0.5f : 0f;
                    stealthAudioService.ApplyAudioMasking(maskingLevel);
                    LogDebug($"[AudioStealthSettingsUI] Audio masking {(enabled ? "enabled" : "disabled")}");
                }
                catch (System.Exception ex)
                {
                    ServiceLocator.GetService<IEventLogger>()?.LogError($"[AudioStealthSettingsUI] Failed to set audio masking: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 注意をそらす音のテスト
        /// </summary>
        public void TestDistraction()
        {
            if (stealthAudioService != null)
            {
                try
                {
                    Vector3 testPosition = Vector3.zero;
                    stealthAudioService.PlayDistraction(testPosition, 10f);
                    LogDebug("[AudioStealthSettingsUI] Test distraction played");
                }
                catch (System.Exception ex)
                {
                    ServiceLocator.GetService<IEventLogger>()?.LogError($"[AudioStealthSettingsUI] Failed to play test distraction: {ex.Message}");
                }
            }
        }

        #endregion

        #region Debug and Testing

        /// <summary>
        /// サービス再初期化
        /// </summary>
        [TabGroup("UI Settings", "Debug")]
        [Button("Reinitialize Services")]
        public void ReinitializeServices()
        {
            audioService = null;
            stealthAudioService = null;
            isServiceAvailable = false;
            
            InitializeAudioServices();
            UpdateUI();
            
            LogDebug("[AudioStealthSettingsUI] Services reinitialized");
        }

        /// <summary>
        /// ServiceLocator使用切り替え
        /// </summary>
        [Button("Toggle ServiceLocator")]
        public void ToggleServiceLocator()
        {
            useServiceLocator = !useServiceLocator;
            ReinitializeServices();
            
            LogDebug($"[AudioStealthSettingsUI] ServiceLocator usage toggled: {useServiceLocator}");
        }

        /// <summary>
        /// 全機能テスト
        /// </summary>
        [Button("Test All Audio Functions")]
        public void TestAllAudioFunctions()
        {
            if (!isServiceAvailable)
            {
                ServiceLocator.GetService<IEventLogger>()?.LogError("[AudioStealthSettingsUI] Services not available for testing");
                return;
            }

            LogDebug("[AudioStealthSettingsUI] Testing all audio functions...");

            try
            {
                // 音量調整テスト
                if (audioService != null)
                {
                    audioService.SetMasterVolume(0.8f);
                    audioService.SetCategoryVolume("bgm", 0.7f);
                    audioService.SetCategoryVolume("effect", 0.6f);
                }

                // ステルス機能テスト
                if (stealthAudioService != null)
                {
                    stealthAudioService.SetEnvironmentNoiseLevel(0.4f);
                    stealthAudioService.AdjustStealthAudio(0.5f);
                    stealthAudioService.ApplyAudioMasking(0.3f);
                }

                LogDebug("[AudioStealthSettingsUI] ✅ All audio function tests completed successfully");
            }
            catch (System.Exception ex)
            {
                ServiceLocator.GetService<IEventLogger>()?.LogError($"[AudioStealthSettingsUI] ❌ Audio function test failed: {ex.Message}");
            }
        }

        /// <summary>
        /// 状態デバッグ表示
        /// </summary>
        [Button("Debug Service State")]
        public void DebugServiceState()
        {
            LogDebug("=== AudioStealthSettingsUI Service State ===");
            LogDebug($"Use ServiceLocator: {useServiceLocator}");
            LogDebug($"Service Available: {isServiceAvailable}");
            LogDebug($"Using ServiceLocator: {usingServiceLocator}");
            LogDebug($"Current Service Type: {currentServiceType}");
            LogDebug($"Audio Service: {(audioService != null ? audioService.GetType().Name : "null")}");
            LogDebug($"Stealth Service: {(stealthAudioService != null ? stealthAudioService.GetType().Name : "null")}");
        }

        private void LogDebug(string message)
        {
            if (enableDebugLogs && FeatureFlags.EnableDebugLogging)
            {
                ServiceLocator.GetService<IEventLogger>()?.Log(message);
            }
        }

        #endregion
    }
}