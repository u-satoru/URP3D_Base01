using UnityEngine;
using UnityEngine.Audio;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Audio.Data;
using asterivo.Unity60.Core.Debug;
using asterivo.Unity60.Core.Shared;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Core.Audio
{
    /// <summary>
    /// 最上位のオーディオ制御システム
    /// 既存のステルスオーディオシステムと新規システムを統合管理
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        private static AudioManager instance;
        public static AudioManager Instance => instance;

        [TabGroup("Audio Managers", "System Integration")]
        [Header("New Audio Systems")]
        [SerializeField, Required] private BGMManager bgmManager;
        [SerializeField, Required] private AmbientManager ambientManager;
        [SerializeField, Required] private EffectManager effectManager;

        [TabGroup("Audio Managers", "System Integration")]
        [Header("Existing Systems Integration")]
        [SerializeField, Required] private SpatialAudioManager spatialAudio;
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

        #region Unity Lifecycle

        private void Awake()
        {
            // Singleton パターン
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeAudioManager();
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            ValidateComponents();
            ApplyVolumeSettings();
            
            if (audioSystemInitializedEvent != null)
            {
                audioSystemInitializedEvent.Raise();
            }
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// オーディオマネージャーの初期化
        /// </summary>
        private void InitializeAudioManager()
        {
            // コンポーネントの自動検索（Inspector で設定されていない場合）
            if (spatialAudio == null)
                spatialAudio = SpatialAudioManager.Instance;

            if (dynamicEnvironment == null)
                dynamicEnvironment = FindFirstObjectByType<DynamicAudioEnvironment>();

            if (stealthCoordinator == null)
                stealthCoordinator = GetComponent<StealthAudioCoordinator>();

            isInitialized = true;
            EventLogger.Log("<color=cyan>[AudioManager]</color> Audio system initialized successfully");
        }

        /// <summary>
        /// 必要なコンポーネントの検証
        /// </summary>
        private void ValidateComponents()
        {
            bool hasErrors = false;

            if (spatialAudio == null)
            {
                EventLogger.LogError("[AudioManager] SpatialAudioManager is required but not assigned!");
                hasErrors = true;
            }

            if (stealthCoordinator == null)
            {
                EventLogger.LogWarning("[AudioManager] StealthAudioCoordinator not found, creating default instance");
                stealthCoordinator = gameObject.AddComponent<StealthAudioCoordinator>();
            }

            if (hasErrors)
            {
                EventLogger.LogError("[AudioManager] Critical components missing! Audio system may not function properly.");
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

            EventLogger.Log($"<color=cyan>[AudioManager]</color> Updated audio for game state: {state}, Tension: {tensionLevel:F2}, Stealth Mode: {isStealthModeActive}");
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
            SetMixerVolume("EffectVolume", effectVolume);

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
        Paused,
        GameOver,
        Cutscene
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