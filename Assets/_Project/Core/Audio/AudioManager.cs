using UnityEngine;
using UnityEngine.Audio;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Audio.Data;
using asterivo.Unity60.Core.Debug;
using asterivo.Unity60.Core.Shared;
using asterivo.Unity60.Core.Audio.Interfaces;
using asterivo.Unity60.Core.Helpers;
using asterivo.Unity60.Core;
// using asterivo.Unity60.Core.Services; // Removed to avoid circular dependency
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Core.Audio
{
    /// <summary>
    /// 最上位のオーディオ制御システム
    /// 既存のステルスオーディオシステムと新規システムを統合管理
    /// ServiceLocator対応版
    /// </summary>
    public class AudioManager : MonoBehaviour, IAudioService, IInitializable
    {
        // ✅ Task 3: Legacy Singleton警告システム（後方互換性のため）
        



        [TabGroup("Audio Managers", "System Integration")]
        [Header("New Audio Systems")]
        [SerializeField, Required] private BGMManager bgmManager;
        [SerializeField, Required] private AmbientManager ambientManager;
        [SerializeField, Required] private EffectManager effectManager;

        [TabGroup("Audio Managers", "System Integration")]
        [Header("Existing Systems Integration")]
        // SpatialAudioServiceはServiceLocator経由で取得 (Obsolete SpatialAudioManagerから移行)
        private ISpatialAudioService spatialAudioService;
        [SerializeField, Required] private DynamicAudioEnvironment dynamicEnvironment;

        [TabGroup("Audio Managers", "Volume Controls")]
        [Header("Master Volume Controls")]
        [Range(0f, 1f), SerializeField] private float masterVolume = AudioConstants.DEFAULT_MASTER_VOLUME;
        [Range(0f, 1f), SerializeField] private float bgmVolume = AudioConstants.DEFAULT_BGM_VOLUME;
        [Range(0f, 1f), SerializeField] private float ambientVolume = AudioConstants.DEFAULT_AMBIENT_VOLUME;
        [Range(0f, 1f), SerializeField] private float effectVolume = AudioConstants.DEFAULT_EFFECT_VOLUME;
        [Range(0f, 1f), SerializeField] private float stealthAudioVolume = AudioConstants.DEFAULT_STEALTH_VOLUME;

        [TabGroup("Audio Managers", "Stealth Integration")]
        [Header("Stealth Integration")]
        [SerializeField, Required] private StealthAudioCoordinator stealthCoordinator;

        [TabGroup("Audio Managers", "Audio Mixer")]
        [Header("Audio Mixer Configuration")]
        [SerializeField] private AudioMixer mainMixer;
        [SerializeField] private string masterVolumeParam = AudioConstants.MIXER_MASTER_VOLUME;
        [SerializeField] private string bgmVolumeParam = AudioConstants.MIXER_BGM_VOLUME;
        [SerializeField] private string ambientVolumeParam = AudioConstants.MIXER_AMBIENT_VOLUME;
        [SerializeField] private string effectVolumeParam = AudioConstants.MIXER_EFFECT_VOLUME;
        [SerializeField] private string stealthVolumeParam = AudioConstants.MIXER_STEALTH_VOLUME;

        [TabGroup("Audio Managers", "Events")]
        [Header("Audio Events")]
        [SerializeField] private GameEvent audioSystemInitializedEvent;
        [SerializeField] private GameEvent volumeSettingsChangedEvent;

        [TabGroup("Audio Managers", "Runtime Info")]
        [Header("Runtime Information")]
        [SerializeField, ReadOnly] private GameState currentGameState;
        [SerializeField, ReadOnly] private float currentTensionLevel;
        [SerializeField, ReadOnly] private bool isStealthModeActive;

        // 内部状態
        private bool isInitialized = false;
        
        // IInitializable実装
        public int Priority => 5; // 早期に初期化
        public bool IsInitialized => isInitialized;

        #region Unity Lifecycle

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            
            // ServiceLocatorに登録
            ServiceLocator.RegisterService<IAudioService>(this);
            
            if (FeatureFlags.EnableDebugLogging)
            {
                var eventLogger = ServiceLocator.GetService<IEventLogger>(); if (eventLogger != null) eventLogger.Log("[AudioManager] Registered to ServiceLocator as IAudioService");
            }
        }

        private void Start()
        {
            Initialize();
        }

        private void OnDestroy()
        {
            // ServiceLocatorから登録解除
            ServiceLocator.UnregisterService<IAudioService>();
            
            if (FeatureFlags.EnableDebugLogging)
            {
                var eventLogger = ServiceLocator.GetService<IEventLogger>(); if (eventLogger != null) eventLogger.Log("[AudioManager] Unregistered from ServiceLocator");
            }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// IInitializable実装 - オーディオマネージャーの初期化
        /// </summary>
        public void Initialize()
        {
            if (isInitialized) return;
            
            // SpatialAudioServiceの取得（ServiceLocator優先）
            if (spatialAudioService == null)
            {
                if (FeatureFlags.UseServiceLocator)
                {
                    spatialAudioService = ServiceLocator.GetService<ISpatialAudioService>();
                }
                
                // フォールバック: ServiceHelper経由で検索
                if (spatialAudioService == null)
                {
                    spatialAudioService = ServiceHelper.GetServiceWithFallback<ISpatialAudioService>();
                }
            }

            if (dynamicEnvironment == null)
                dynamicEnvironment = ServiceHelper.GetServiceWithFallback<DynamicAudioEnvironment>();

            if (stealthCoordinator == null)
                stealthCoordinator = GetComponent<StealthAudioCoordinator>();
            
            ValidateComponents();
            ApplyVolumeSettings();
            InitializeAudioUpdateCoordinator();
            
            if (audioSystemInitializedEvent != null)
            {
                audioSystemInitializedEvent.Raise();
            }

            isInitialized = true;
            
            if (FeatureFlags.EnableDebugLogging)
            {
                var eventLogger = ServiceLocator.GetService<IEventLogger>(); if (eventLogger != null) eventLogger.Log("<color=cyan>[AudioManager]</color> Audio system initialized successfully");
            }
        }

        /// <summary>
        /// 必要なコンポーネントの検証
        /// </summary>
        private void ValidateComponents()
        {
            bool hasErrors = false;

            if (spatialAudioService == null)
            {
                var eventLogger = ServiceLocator.GetService<IEventLogger>(); if (eventLogger != null) eventLogger.LogError("[AudioManager] SpatialAudioService is required but not available!");
                hasErrors = true;
            }

            if (stealthCoordinator == null)
            {
                var eventLogger = ServiceLocator.GetService<IEventLogger>(); if (eventLogger != null) eventLogger.LogWarning("[AudioManager] StealthAudioCoordinator not found, creating default instance");
                stealthCoordinator = gameObject.AddComponent<StealthAudioCoordinator>();
            }

            if (hasErrors)
            {
                var eventLogger = ServiceLocator.GetService<IEventLogger>(); if (eventLogger != null) eventLogger.LogError("[AudioManager] Critical components missing! Audio system may not function properly.");
            }
        }

        /// <summary>
        /// AudioUpdateCoordinatorの初期化
        /// </summary>
        private void InitializeAudioUpdateCoordinator()
        {
            // ServiceLocatorからAudioUpdateCoordinatorを取得を試みる
            AudioUpdateCoordinator coordinator = null;
            
            if (FeatureFlags.UseServiceLocator)
            {
                // TODO: AudioUpdateCoordinator用のインターフェースを作成後に有効化
                // coordinator = ServiceLocator.GetService<IAudioUpdateService>() as AudioUpdateCoordinator;
            }
            
            // フォールバック: ServiceHelper経由で検索
            if (coordinator == null)
            {
                coordinator = ServiceHelper.GetServiceWithFallback<AudioUpdateCoordinator>();
            }
            
            if (coordinator == null)
            {
                // 専用のGameObjectを作成してAudioUpdateCoordinatorを追加
                GameObject coordinatorObject = new GameObject("AudioUpdateCoordinator");
                coordinatorObject.transform.SetParent(transform);
                coordinator = coordinatorObject.AddComponent<AudioUpdateCoordinator>();
                
                if (FeatureFlags.EnableDebugLogging)
                {
                    var eventLogger = ServiceLocator.GetService<IEventLogger>(); if (eventLogger != null) eventLogger.Log("<color=cyan>[AudioManager]</color> Created AudioUpdateCoordinator for optimized updates");
                }
            }
            else if (FeatureFlags.EnableDebugLogging)
            {
                var eventLogger = ServiceLocator.GetService<IEventLogger>(); if (eventLogger != null) eventLogger.Log("<color=cyan>[AudioManager]</color> Found existing AudioUpdateCoordinator");
            }
        }

        #endregion

        #region Game State Integration

        /// <summary>
        /// ゲーム状態に応じたオーディオ制御
        /// </summary>
        public void UpdateAudioForGameState(GameState state, float tensionLevel = 0f)
        {
            if (!isInitialized) return;

            currentGameState = state;
            currentTensionLevel = tensionLevel;
            isStealthModeActive = stealthCoordinator != null && stealthCoordinator.ShouldReduceNonStealthAudio();

            // BGM の更新
            if (bgmManager != null)
            {
                bgmManager.UpdateForTensionLevel(tensionLevel, isStealthModeActive);
            }

            // 環境音の更新
            if (ambientManager != null)
            {
                ambientManager.UpdateForStealthState(isStealthModeActive);
            }

            // 動的環境システムとの連携
            if (dynamicEnvironment != null)
            {
                // DynamicAudioEnvironment の既存機能を活用
                var (env, weather, time) = dynamicEnvironment.GetCurrentState();
                UpdateAudioForEnvironmentalState(env, weather, time, tensionLevel);
            }

            var eventLogger = ServiceLocator.GetService<IEventLogger>(); if (eventLogger != null) eventLogger.Log($"<color=cyan>[AudioManager]</color> Updated audio for game state: {state}, Tension: {tensionLevel:F2}, Stealth Mode: {isStealthModeActive}");
        }

        /// <summary>
        /// 環境状態に応じた音響制御
        /// </summary>
        private void UpdateAudioForEnvironmentalState(EnvironmentType env, WeatherType weather, TimeOfDay time, float tension)
        {
            // 環境に応じたBGM調整
            if (bgmManager != null)
            {
                bgmManager.UpdateForEnvironment(env, weather, time);
            }

            // 環境音の調整
            if (ambientManager != null)
            {
                ambientManager.UpdateForEnvironment(env, weather, time);
            }
        }

        #endregion

        #region Volume Control

        /// <summary>
        /// 音量設定を適用
        /// </summary>
        public void ApplyVolumeSettings()
        {
            if (mainMixer == null) return;

            // Audio Mixer のパラメータを更新
            SetMixerVolume(masterVolumeParam, masterVolume);
            SetMixerVolume(bgmVolumeParam, bgmVolume);
            SetMixerVolume(ambientVolumeParam, ambientVolume);
            SetMixerVolume(effectVolumeParam, effectVolume);
            SetMixerVolume(stealthVolumeParam, stealthAudioVolume);

            // 個別マネージャーに音量設定を通知
            if (bgmManager != null)
                bgmManager.SetMasterVolume(bgmVolume);

            if (ambientManager != null)
                ambientManager.SetMasterVolume(ambientVolume);

            // Effect volume is controlled by the AudioMixer
            SetMixerVolume(effectVolumeParam, effectVolume);

            if (volumeSettingsChangedEvent != null)
            {
                volumeSettingsChangedEvent.Raise();
            }
        }

        /// <summary>
        /// Audio Mixer の音量パラメータを設定
        /// </summary>
        private void SetMixerVolume(string paramName, float volume)
        {
            if (string.IsNullOrEmpty(paramName)) return;

            // 音量を dB に変換 (0-1 の range を -80dB - 0dB に変換)
            float dbValue = volume > AudioConstants.MIN_VOLUME_FOR_DB ? Mathf.Log10(volume) * 20f : AudioConstants.MIN_DB_VALUE;
            mainMixer.SetFloat(paramName, dbValue);
        }

        /// <summary>
        /// マスター音量の設定
        /// </summary>
        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            ApplyVolumeSettings();
        }

        /// <summary>
        /// BGM音量の設定
        /// </summary>
        public void SetBGMVolume(float volume)
        {
            bgmVolume = Mathf.Clamp01(volume);
            ApplyVolumeSettings();
        }

        /// <summary>
        /// 環境音音量の設定
        /// </summary>
        public void SetAmbientVolume(float volume)
        {
            ambientVolume = Mathf.Clamp01(volume);
            ApplyVolumeSettings();
        }

        /// <summary>
        /// 効果音音量の設定
        /// </summary>
        public void SetEffectVolume(float volume)
        {
            effectVolume = Mathf.Clamp01(volume);
            ApplyVolumeSettings();
        }

        /// <summary>
        /// ステルス音響音量の設定
        /// </summary>
        public void SetStealthAudioVolume(float volume)
        {
            stealthAudioVolume = Mathf.Clamp01(volume);
            ApplyVolumeSettings();
        }

        #endregion

        #region Public Interface

        /// <summary>
        /// 緊張度レベルの更新
        /// </summary>
        public void UpdateTensionLevel(float tension)
        {
            UpdateAudioForGameState(currentGameState, tension);
        }

        /// <summary>
        /// ステルスモードの強制設定
        /// </summary>
        public void SetStealthModeOverride(bool forceStealthMode)
        {
            if (stealthCoordinator != null)
            {
                stealthCoordinator.SetOverrideStealthMode(forceStealthMode);
                UpdateAudioForGameState(currentGameState, currentTensionLevel);
            }
        }

        /// <summary>
        /// 現在の音響状態を取得
        /// </summary>
        public AudioSystemState GetCurrentAudioState()
        {
            return new AudioSystemState
            {
                gameState = currentGameState,
                tensionLevel = currentTensionLevel,
                isStealthModeActive = isStealthModeActive,
                masterVolume = masterVolume,
                bgmVolume = bgmVolume,
                ambientVolume = ambientVolume,
                effectVolume = effectVolume,
                stealthAudioVolume = stealthAudioVolume
            };
        }

        /// <summary>
        /// オーディオシステムの一時停止
        /// </summary>
        public void PauseAllAudio()
        {
            if (bgmManager != null) bgmManager.PauseAll();
            if (ambientManager != null) ambientManager.PauseAll();
            if (effectManager != null) effectManager.StopAllEffects();
        }

        /// <summary>
        /// オーディオシステムの再開
        /// </summary>
        public void ResumeAllAudio()
        {
            if (bgmManager != null) bgmManager.ResumeAll();
            if (ambientManager != null) ambientManager.ResumeAll();
            // Effects resume automatically when new sounds are played
        }

        #endregion
        
        #region IAudioService Implementation
        
        /// <summary>
        /// サウンドを再生
        /// </summary>
        public void PlaySound(string soundId, Vector3 position = default, float volume = 1f)
        {
            if (!isInitialized)
            {
                var eventLogger = ServiceLocator.GetService<IEventLogger>(); if (eventLogger != null) eventLogger.LogWarning("[AudioManager] System not initialized");
                return;
            }
            
            // 効果音として再生
            if (effectManager != null)
            {
                effectManager.PlayEffect(soundId, position, volume * effectVolume * masterVolume);
            }
        }
        
        /// <summary>
        /// サウンドを停止
        /// </summary>
        public void StopSound(string soundId)
        {
            if (effectManager != null)
            {
                // 個別停止機能がないため、全て停止
                effectManager.StopAllEffects();
            }
        }
        
        /// <summary>
        /// すべてのサウンドを停止
        /// </summary>
        public void StopAllSounds()
        {
            PauseAllAudio();
        }
        
        /// <summary>
        /// マスターボリュームを取得
        /// </summary>
        public float GetMasterVolume()
        {
            return masterVolume;
        }
        
        /// <summary>
        /// BGMボリュームを取得
        /// </summary>
        public float GetBGMVolume()
        {
            return bgmVolume;
        }
        
        /// <summary>
        /// アンビエントボリュームを取得
        /// </summary>
        public float GetAmbientVolume()
        {
            return ambientVolume;
        }
        
        /// <summary>
        /// エフェクトボリュームを取得
        /// </summary>
        public float GetEffectVolume()
        {
            return effectVolume;
        }
        
        /// <summary>
        /// カテゴリ別のボリュームを設定
        /// </summary>
        public void SetCategoryVolume(string category, float volume)
        {
            volume = Mathf.Clamp01(volume);
            
            switch (category.ToLower())
            {
                case "bgm":
                    SetBGMVolume(volume);
                    break;
                case "ambient":
                    SetAmbientVolume(volume);
                    break;
                case "effect":
                case "sfx":
                    SetEffectVolume(volume);
                    break;
                case "stealth":
                    SetStealthAudioVolume(volume);
                    break;
                default:
                    var eventLogger = ServiceLocator.GetService<IEventLogger>(); if (eventLogger != null) eventLogger.LogWarning($"[AudioManager] Unknown category: {category}");
                    break;
            }
        }
        
        /// <summary>
        /// サウンドが再生中か確認
        /// </summary>
        public bool IsPlaying(string soundId)
        {
            // EffectManagerには個別の状態チェック機能がないため、仮実装
            // TODO: 個別サウンドの再生状態トラッキング機能を追加
            return false;
        }
        
        /// <summary>
        /// 一時停止
        /// </summary>
        public void Pause()
        {
            PauseAllAudio();
        }
        
        /// <summary>
        /// 再開
        /// </summary>
        public void Resume()
        {
            ResumeAllAudio();
        }
        
        #endregion

        #region Editor Helpers

#if UNITY_EDITOR
        [TabGroup("Audio Managers", "Debug Tools")]
        [Button("Test Tension Level")]
        public void TestTensionLevel(float testTension)
        {
            UpdateTensionLevel(testTension);
        }

        [TabGroup("Audio Managers", "Debug Tools")]
        [Button("Toggle Stealth Mode")]
        public void TestStealthMode()
        {
            SetStealthModeOverride(!isStealthModeActive);
        }

        [TabGroup("Audio Managers", "Debug Tools")]
        [Button("Apply Volume Settings")]
        public void EditorApplyVolumeSettings()
        {
            ApplyVolumeSettings();
        }

        private void OnValidate()
        {
            // エディタでの値変更時に音量を即座に適用
            if (Application.isPlaying && isInitialized)
            {
                ApplyVolumeSettings();
            }
        }
#endif

        #endregion
    }

    #region Supporting Types

    /// <summary>
    /// ゲーム状態の定義
    /// </summary>
    public enum GameState
    {
        MainMenu,
        Loading,
        Gameplay,
        Playing = Gameplay,  // Alias for backwards compatibility
        Paused,
        GameOver,
        Victory,             // Added for game completion
        Cutscene,
        InGame = Gameplay    // Alias for audio system
    }

    /// <summary>
    /// 音響システム状態の構造体
    /// </summary>
    [System.Serializable]
    public struct AudioSystemState
    {
        public GameState gameState;
        public float tensionLevel;
        public bool isStealthModeActive;
        public float masterVolume;
        public float bgmVolume;
        public float ambientVolume;
        public float effectVolume;
        public float stealthAudioVolume;
    }

    #endregion
}