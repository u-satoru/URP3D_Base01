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
    /// AudioManager縺ｮService蛹門ｮ溯｣・    /// Singleton繝代ち繝ｼ繝ｳ繧剃ｽｿ繧上★縲ヾerviceLocator邨檎罰縺ｧ繧｢繧ｯ繧ｻ繧ｹ
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

        // IInitializable螳溯｣・        public int Priority => 10; // 繧ｪ繝ｼ繝・ぅ繧ｪ繧ｷ繧ｹ繝・Β縺ｯ譌ｩ繧√↓蛻晄悄蛹・        public bool IsInitialized { get; private set; }

        private bool isPaused = false;
        private float pausedBGMVolume = 0f;
        private float pausedAmbientVolume = 0f;

        #region Unity Lifecycle

        private void Awake()
        {
            // ServiceLocator縺ｫ逋ｻ骭ｲ
            if (FeatureFlags.UseServiceLocator)
            {
                ServiceLocator.RegisterService<IAudioService>(this);
                
                if (FeatureFlags.EnableDebugLogging)
                {
                    EventLogger.LogStatic("[AudioService] Registered to ServiceLocator");
                }
            }
        }

        private void Start()
        {
            Initialize();
        }

        private void OnDestroy()
        {
            // ServiceLocator縺九ｉ逋ｻ骭ｲ隗｣髯､
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
                EventLogger.LogStatic("[AudioService] Initialization complete");
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

            // 蜉ｹ譫憺浹縺ｨ縺励※蜀咲函
            if (effectManager != null)
            {
                // EffectManager縺ｮPlayEffect縺ｯ(string, Vector3, float)縺ｮ鬆・                effectManager.PlayEffect(soundId, position, volume * effectVolume * masterVolume);
            }
        }

        public void StopSound(string soundId)
        {
            if (effectManager != null)
            {
                // EffectManager縺ｫ縺ｯStopEffect縺後↑縺・◆繧√ヾtopAllEffects繧剃ｽｿ逕ｨ
                effectManager.StopAllEffects();
            }
        }

        public void StopAllSounds()
        {
            if (bgmManager != null) bgmManager.StopBGM(0f);
            if (ambientManager != null) ambientManager.PauseAll(); // AmbientManager縺ｫ縺ｯStop繝｡繧ｽ繝・ラ縺後↑縺・            if (effectManager != null) effectManager.StopAllEffects();
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
                
            // EffectManager縺ｧ縺ｮ繝√ぉ繝・け
            if (effectManager != null && effectManager.IsPlaying(soundId))
                return true;
                
            // BGMManager縺ｧ縺ｮ繝√ぉ繝・け
            if (bgmManager != null && bgmManager.IsPlaying(soundId))
                return true;
                
            return false;
        }

        public void Pause()
        {
            if (isPaused) return;
            
            isPaused = true;
            
            // BGM縺ｮ荳譎ょ●豁｢・磯浹驥上ｒ菫晏ｭ倥＠縺ｦ縺九ｉ繧ｼ繝ｭ縺ｫ・・            if (bgmManager != null)
            {
                // 迴ｾ蝨ｨ縺ｮ髻ｳ驥上ｒ菫晏ｭ・                pausedBGMVolume = bgmVolume;
                bgmManager.SetMasterVolume(0f);
            }
            
            // 繧｢繝ｳ繝薙お繝ｳ繝医・荳譎ょ●豁｢
            if (ambientManager != null)
            {
                pausedAmbientVolume = ambientVolume;
                ambientManager.SetMasterVolume(0f);
            }
            
            // 蜉ｹ譫憺浹縺ｮ蛛懈ｭ｢・井ｸ譎ょ●豁｢縺ｧ縺ｯ縺ｪ縺丞●豁｢・・            if (effectManager != null)
            {
                effectManager.StopAllEffects();
            }
            
            if (FeatureFlags.EnableDebugLogging)
            {
                EventLogger.LogStatic("[AudioService] Audio paused");
            }
        }

        public void Resume()
        {
            if (!isPaused) return;
            
            isPaused = false;
            
            // BGM縺ｮ蜀埼幕・井ｿ晏ｭ倥＆繧後◆髻ｳ驥上〒蜀埼幕・・            if (bgmManager != null)
            {
                bgmManager.SetMasterVolume(pausedBGMVolume > 0 ? pausedBGMVolume : bgmVolume);
                pausedBGMVolume = 0f;
            }
            
            // 繧｢繝ｳ繝薙お繝ｳ繝医・蜀埼幕
            if (ambientManager != null)
            {
                ambientManager.SetMasterVolume(pausedAmbientVolume > 0 ? pausedAmbientVolume : ambientVolume);
                pausedAmbientVolume = 0f;
            }
            
            // 蜉ｹ譫憺浹縺ｯ蛛懈ｭ｢縺輔ｌ縺溘◆繧√∵眠縺励＞髻ｳ縺悟・逕溘＆繧後ｋ縺ｾ縺ｧ蠕・ｩ・            
            if (FeatureFlags.EnableDebugLogging)
            {
                EventLogger.LogStatic("[AudioService] Audio resumed");
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
            // 遨ｺ髢馴浹髻ｿ繝槭ロ繝ｼ繧ｸ繝｣繝ｼ縺ｮ蛻晄悄蛹厄ｼ・ingleton萓晏ｭ倥ｒ蜑企勁・・            // TODO: SpatialAudioManager縺栗SpatialAudioService繧貞ｮ溯｣・☆繧句ｿ・ｦ√′縺ゅｋ
            // if (spatialAudio != null && FeatureFlags.UseServiceLocator)
            // {
            //     ServiceLocator.RegisterService<ISpatialAudioService>(spatialAudio);
            // }

            // 繧ｹ繝・Ν繧ｹ繧ｪ繝ｼ繝・ぅ繧ｪ繧ｳ繝ｼ繝・ぅ繝阪・繧ｿ繝ｼ縺ｮ蛻晄悄蛹・            // TODO: StealthAudioCoordinator縺栗StealthAudioService繧貞ｮ溯｣・☆繧句ｿ・ｦ√′縺ゅｋ
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
        /// BGM繧貞・逕・        /// </summary>
        public void PlayBGM(string bgmName, float fadeTime = 1f)
        {
            if (bgmManager != null)
            {
                // BGMCategory繧剃ｽｿ轣・ｿ・ｦ√′縺ゅｋ
                // TODO: bgmName縺九ｉBGMCategory縺ｸ縺ｮ螟画鋤繝ｭ繧ｸ繝・け縺悟ｿ・ｦ・                // BGM蜷阪°繧隠GMCategory縺ｸ縺ｮ螟画鋤
                var category = bgmManager.GetCategoryFromName(bgmName);
                bgmManager.PlayBGMCategory(category, fadeTime <= 0);
                
                if (FeatureFlags.EnableDebugLogging)
                {
                    EventLogger.LogStatic($"[AudioService] Playing BGM: {bgmName} -> {category}, fadeTime: {fadeTime}");
                }
            }
        }

        /// <summary>
        /// 繧｢繝ｳ繝薙お繝ｳ繝医し繧ｦ繝ｳ繝峨ｒ譖ｴ譁ｰ・・mbientManager縺ｫ縺ｯPlayAmbient繝｡繧ｽ繝・ラ縺後↑縺・ｼ・        /// </summary>
        public void UpdateAmbient(EnvironmentType environment, WeatherType weather, TimeOfDay timeOfDay)
        {
            if (ambientManager != null)
            {
                ambientManager.UpdateForEnvironment(environment, weather, timeOfDay);
            }
        }

        /// <summary>
        /// 繧ｲ繝ｼ繝迥ｶ諷九ｒ譖ｴ譁ｰ
        /// </summary>
        public void UpdateGameState(GameState newState)
        {
            currentGameState = newState;
            
            // 繧ｲ繝ｼ繝迥ｶ諷九↓蠢懊§縺滄浹髻ｿ隱ｿ謨ｴ
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

    // GameState enum縺ｯAudioManager.cs縺ｧ螳夂ｾｩ縺輔ｌ縺ｦ縺・∪縺・}