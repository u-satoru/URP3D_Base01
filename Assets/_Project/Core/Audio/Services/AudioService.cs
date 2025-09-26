using UnityEngine;
using UnityEngine.Audio;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Audio.Data;
// using asterivo.Unity60.Core.Shared;
// using asterivo.Unity60.Core.Debug;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Audio.Interfaces;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Core.Audio.Services
{
    /// <summary>
    /// AudioManagerのService化実裁E    /// Singletonパターンを使わず、ServiceLocator経由でアクセス
    /// </summary>
    public class AudioService : MonoBehaviour, IAudioService, IInitializable
    {
        [TabGroup("Audio Managers", "System Integration")]
        [Header("New Audio Systems")]
        [SerializeField, Required] private BGMManager bgmManager;
        [SerializeField, Required] private AmbientManager ambientManager;
        [SerializeField, Required] private EffectManager effectManager;

        [TabGroup("Audio Managers", "System Integration")]
        [Header("Existing Systems Integration")]
        [SerializeField] private SpatialAudioManager spatialAudio;
        [SerializeField] private DynamicAudioEnvironment dynamicEnvironment;

        [TabGroup("Audio Managers", "Volume Controls")]
        [Header("Master Volume Controls")]
        [Range(0f, 1f), SerializeField] private float masterVolume = AudioConstants.DEFAULT_MASTER_VOLUME;
        [Range(0f, 1f), SerializeField] private float bgmVolume = AudioConstants.DEFAULT_BGM_VOLUME;
        [Range(0f, 1f), SerializeField] private float ambientVolume = AudioConstants.DEFAULT_AMBIENT_VOLUME;
        [Range(0f, 1f), SerializeField] private float effectVolume = AudioConstants.DEFAULT_EFFECT_VOLUME;
        [Range(0f, 1f), SerializeField] private float stealthAudioVolume = AudioConstants.DEFAULT_STEALTH_VOLUME;

        [TabGroup("Audio Managers", "Stealth Integration")]
        [Header("Stealth Integration")]
        [SerializeField] private StealthAudioCoordinator stealthCoordinator;

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

        // IInitializable実装
        public int Priority => 10; // オーディオシステムは早めに初期化
        public bool IsInitialized { get; private set; }

        private bool isPaused = false;
        private float pausedBGMVolume = 0f;
        private float pausedAmbientVolume = 0f;

        #region Unity Lifecycle

        private void Awake()
        {
            // ServiceLocatorに登録
            if (FeatureFlags.UseServiceLocator)
            {
                ServiceLocator.RegisterService<IAudioService>(this);
                
                if (FeatureFlags.EnableDebugLogging)
                {
                    ServiceHelper.Log("[AudioService] Registered to ServiceLocator");
                }
            }
        }

        private void Start()
        {
            Initialize();
        }

        private void OnDestroy()
        {
            // ServiceLocatorから登録解除
            if (FeatureFlags.UseServiceLocator)
            {
                ServiceLocator.UnregisterService<IAudioService>();
            }
        }

        #endregion

        #region IInitializable Implementation

        public void Initialize()
        {
            if (IsInitialized) return;

            ValidateComponents();
            ApplyVolumeSettings();
            InitializeSubsystems();

            if (audioSystemInitializedEvent != null)
            {
                audioSystemInitializedEvent.Raise();
            }

            IsInitialized = true;

            if (FeatureFlags.EnableDebugLogging)
            {
                ServiceHelper.Log("[AudioService] Initialization complete");
            }
        }

        #endregion

        #region IAudioService Implementation

        public void PlaySound(string soundId, Vector3 position = default, float volume = 1f)
        {
            if (!IsInitialized)
            {
                ServiceLocator.GetService<IEventLogger>().LogWarning("[AudioService] System not initialized");
                return;
            }

            // 効果音として再生
            if (effectManager != null)
            {
                // EffectManagerのPlayEffectは(string, Vector3, float)の頁E                effectManager.PlayEffect(soundId, position, volume * effectVolume * masterVolume);
            }
        }

        public void StopSound(string soundId)
        {
            if (effectManager != null)
            {
                // EffectManagerにはStopEffectがなぁE��め、StopAllEffectsを使用
                effectManager.StopAllEffects();
            }
        }

        public void StopAllSounds()
        {
            if (bgmManager != null) bgmManager.StopBGM(0f);
            if (ambientManager != null) ambientManager.PauseAll(); // AmbientManagerにはStopメソチE��がなぁE            if (effectManager != null) effectManager.StopAllEffects();
        }

        public float GetMasterVolume()
        {
            return masterVolume;
        }
        
        public float GetBGMVolume()
        {
            return bgmVolume;
        }
        
        public float GetAmbientVolume()
        {
            return ambientVolume;
        }
        
        public float GetEffectVolume()
        {
            return effectVolume;
        }
        
        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            ApplyVolumeSettings();
            
            if (volumeSettingsChangedEvent != null)
            {
                volumeSettingsChangedEvent.Raise();
            }
        }

        public void SetCategoryVolume(string category, float volume)
        {
            volume = Mathf.Clamp01(volume);
            
            switch (category.ToLower())
            {
                case "bgm":
                    bgmVolume = volume;
                    if (mainMixer != null)
                        mainMixer.SetFloat(bgmVolumeParam, ConvertToDecibel(bgmVolume * masterVolume));
                    break;
                    
                case "ambient":
                    ambientVolume = volume;
                    if (mainMixer != null)
                        mainMixer.SetFloat(ambientVolumeParam, ConvertToDecibel(ambientVolume * masterVolume));
                    break;
                    
                case "effect":
                case "sfx":
                    effectVolume = volume;
                    if (mainMixer != null)
                        mainMixer.SetFloat(effectVolumeParam, ConvertToDecibel(effectVolume * masterVolume));
                    break;
                    
                case "stealth":
                    stealthAudioVolume = volume;
                    if (mainMixer != null)
                        mainMixer.SetFloat(stealthVolumeParam, ConvertToDecibel(stealthAudioVolume * masterVolume));
                    break;
                    
                default:
                    ServiceLocator.GetService<IEventLogger>().LogWarning($"[AudioService] Unknown category: {category}");
                    break;
            }
            
            if (volumeSettingsChangedEvent != null)
            {
                volumeSettingsChangedEvent.Raise();
            }
        }

        public bool IsPlaying(string soundId)
        {
            if (string.IsNullOrEmpty(soundId))
                return false;
                
            // EffectManagerでのチェチE��
            if (effectManager != null && effectManager.IsPlaying(soundId))
                return true;
                
            // BGMManagerでのチェチE��
            if (bgmManager != null && bgmManager.IsPlaying(soundId))
                return true;
                
            return false;
        }

        public void Pause()
        {
            if (isPaused) return;
            
            isPaused = true;
            
            // BGMの一時停止�E�音量を保存してからゼロに�E�E            if (bgmManager != null)
            {
                // 現在の音量を保孁E                pausedBGMVolume = bgmVolume;
                bgmManager.SetMasterVolume(0f);
            }
            
            // アンビエント�E一時停止
            if (ambientManager != null)
            {
                pausedAmbientVolume = ambientVolume;
                ambientManager.SetMasterVolume(0f);
            }
            
            // 効果音の停止�E�一時停止ではなく停止�E�E            if (effectManager != null)
            {
                effectManager.StopAllEffects();
            }
            
            if (FeatureFlags.EnableDebugLogging)
            {
                ServiceHelper.Log("[AudioService] Audio paused");
            }
        }

        public void Resume()
        {
            if (!isPaused) return;
            
            isPaused = false;
            
            // BGMの再開�E�保存された音量で再開�E�E            if (bgmManager != null)
            {
                bgmManager.SetMasterVolume(pausedBGMVolume > 0 ? pausedBGMVolume : bgmVolume);
                pausedBGMVolume = 0f;
            }
            
            // アンビエント�E再開
            if (ambientManager != null)
            {
                ambientManager.SetMasterVolume(pausedAmbientVolume > 0 ? pausedAmbientVolume : ambientVolume);
                pausedAmbientVolume = 0f;
            }
            
            // 効果音は停止されたため、新しい音が�E生されるまで征E��E            
            if (FeatureFlags.EnableDebugLogging)
            {
                ServiceHelper.Log("[AudioService] Audio resumed");
            }
        }

        #endregion

        #region Private Methods

        private void ValidateComponents()
        {
            if (bgmManager == null)
                ServiceLocator.GetService<IEventLogger>().LogWarning("[AudioService] BGMManager is not assigned");
            
            if (ambientManager == null)
                ServiceLocator.GetService<IEventLogger>().LogWarning("[AudioService] AmbientManager is not assigned");
            
            if (effectManager == null)
                ServiceLocator.GetService<IEventLogger>().LogWarning("[AudioService] EffectManager is not assigned");
        }

        private void InitializeSubsystems()
        {
            // 空間音響マネージャーの初期化！Eingleton依存を削除�E�E            // TODO: SpatialAudioManagerがISpatialAudioServiceを実裁E��る忁E��がある
            // if (spatialAudio != null && FeatureFlags.UseServiceLocator)
            // {
            //     ServiceLocator.RegisterService<ISpatialAudioService>(spatialAudio);
            // }

            // スチE��スオーチE��オコーチE��ネ�Eターの初期匁E            // TODO: StealthAudioCoordinatorがIStealthAudioServiceを実裁E��る忁E��がある
            // if (stealthCoordinator != null && FeatureFlags.UseServiceLocator)
            // {
            //     ServiceLocator.RegisterService<IStealthAudioService>(stealthCoordinator);
            // }
        }

        private void ApplyVolumeSettings()
        {
            if (mainMixer == null) return;

            mainMixer.SetFloat(masterVolumeParam, ConvertToDecibel(masterVolume));
            mainMixer.SetFloat(bgmVolumeParam, ConvertToDecibel(bgmVolume * masterVolume));
            mainMixer.SetFloat(ambientVolumeParam, ConvertToDecibel(ambientVolume * masterVolume));
            mainMixer.SetFloat(effectVolumeParam, ConvertToDecibel(effectVolume * masterVolume));
            mainMixer.SetFloat(stealthVolumeParam, ConvertToDecibel(stealthAudioVolume * masterVolume));
        }

        private float ConvertToDecibel(float linear)
        {
            if (linear <= 0f) return -80f;
            return Mathf.Log10(linear) * 20f;
        }

        #endregion

        #region Public API

        /// <summary>
        /// BGMを�E甁E        /// </summary>
        public void PlayBGM(string bgmName, float fadeTime = 1f)
        {
            if (bgmManager != null)
            {
                // BGMCategoryを使灁E��E��がある
                // TODO: bgmNameからBGMCategoryへの変換ロジチE��が忁E��E                // BGM名からBGMCategoryへの変換
                var category = bgmManager.GetCategoryFromName(bgmName);
                bgmManager.PlayBGMCategory(category, fadeTime <= 0);
                
                if (FeatureFlags.EnableDebugLogging)
                {
                    ServiceHelper.Log($"[AudioService] Playing BGM: {bgmName} -> {category}, fadeTime: {fadeTime}");
                }
            }
        }

        /// <summary>
        /// アンビエントサウンドを更新�E�EmbientManagerにはPlayAmbientメソチE��がなぁE��E        /// </summary>
        public void UpdateAmbient(EnvironmentType environment, WeatherType weather, TimeOfDay timeOfDay)
        {
            if (ambientManager != null)
            {
                ambientManager.UpdateForEnvironment(environment, weather, timeOfDay);
            }
        }

        /// <summary>
        /// ゲーム状態を更新
        /// </summary>
        public void UpdateGameState(GameState newState)
        {
            currentGameState = newState;
            
            // ゲーム状態に応じた音響調整
            switch (newState)
            {
                case GameState.MainMenu:
                    PlayBGM("MainMenu");
                    break;
                case GameState.InGame:
                    PlayBGM("Gameplay");
                    break;
                case GameState.Paused:
                    Pause();
                    break;
            }
        }

        #endregion
    }
}

// GameState enumはAudioManager.csで定義されている
