using UnityEngine;
using UnityEngine.Audio;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Audio.Data;
// // using asterivo.Unity60.Core.Debug;
// // using asterivo.Unity60.Core.Shared;
using asterivo.Unity60.Core.Audio.Interfaces;
// using asterivo.Unity60.Core.Helpers;
using asterivo.Unity60.Core;
// using asterivo.Unity60.Core.Services; // Removed to avoid circular dependency
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Core.Audio
{
    /// <summary>
    /// 譛荳贋ｽ阪・繧ｪ繝ｼ繝・ぅ繧ｪ蛻ｶ蠕｡繧ｷ繧ｹ繝・Β
    /// 譌｢蟄倥・繧ｹ繝・Ν繧ｹ繧ｪ繝ｼ繝・ぅ繧ｪ繧ｷ繧ｹ繝・Β縺ｨ譁ｰ隕上す繧ｹ繝・Β繧堤ｵｱ蜷育ｮ｡逅・    /// ServiceLocator蟇ｾ蠢懃沿
    /// </summary>
    public class AudioManager : MonoBehaviour, IAudioService, IInitializable
    {
        // 笨・Task 3: Legacy Singleton隴ｦ蜻翫す繧ｹ繝・Β・亥ｾ梧婿莠呈鋤諤ｧ縺ｮ縺溘ａ・・        



        [TabGroup("Audio Managers", "System Integration")]
        [Header("New Audio Systems")]
        [SerializeField, Required] private BGMManager bgmManager;
        [SerializeField, Required] private AmbientManager ambientManager;
        [SerializeField, Required] private EffectManager effectManager;

        [TabGroup("Audio Managers", "System Integration")]
        [Header("Existing Systems Integration")]
        // SpatialAudioService縺ｯServiceLocator邨檎罰縺ｧ蜿門ｾ・(Obsolete SpatialAudioManager縺九ｉ遘ｻ陦・
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

        // 蜀・Κ迥ｶ諷・        private bool isInitialized = false;
        
        // IInitializable螳溯｣・        public int Priority => 5; // 譌ｩ譛溘↓蛻晄悄蛹・        public bool IsInitialized => isInitialized;

        #region Unity Lifecycle

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            
            // ServiceLocator縺ｫ逋ｻ骭ｲ
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
            // ServiceLocator縺九ｉ逋ｻ骭ｲ隗｣髯､
            ServiceLocator.UnregisterService<IAudioService>();
            
            if (FeatureFlags.EnableDebugLogging)
            {
                var eventLogger = ServiceLocator.GetService<IEventLogger>(); if (eventLogger != null) eventLogger.Log("[AudioManager] Unregistered from ServiceLocator");
            }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// IInitializable螳溯｣・- 繧ｪ繝ｼ繝・ぅ繧ｪ繝槭ロ繝ｼ繧ｸ繝｣繝ｼ縺ｮ蛻晄悄蛹・        /// </summary>
        public void Initialize()
        {
            if (isInitialized) return;
            
            // SpatialAudioService縺ｮ蜿門ｾ暦ｼ・erviceLocator蜆ｪ蜈茨ｼ・            if (spatialAudioService == null)
            {
                if (FeatureFlags.UseServiceLocator)
                {
                    spatialAudioService = ServiceLocator.GetService<ISpatialAudioService>();
                }
                
                // 繝輔か繝ｼ繝ｫ繝舌ャ繧ｯ: ServiceHelper邨檎罰縺ｧ讀懃ｴ｢
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
        /// 蠢・ｦ√↑繧ｳ繝ｳ繝昴・繝阪Φ繝医・讀懆ｨｼ
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
        /// AudioUpdateCoordinator縺ｮ蛻晄悄蛹・        /// </summary>
        private void InitializeAudioUpdateCoordinator()
        {
            // ServiceLocator縺九ｉAudioUpdateCoordinator繧貞叙蠕励ｒ隧ｦ縺ｿ繧・            AudioUpdateCoordinator coordinator = null;
            
            if (FeatureFlags.UseServiceLocator)
            {
                // TODO: AudioUpdateCoordinator逕ｨ縺ｮ繧､繝ｳ繧ｿ繝ｼ繝輔ぉ繝ｼ繧ｹ繧剃ｽ懈・蠕後↓譛牙柑蛹・                // coordinator = ServiceLocator.GetService<IAudioUpdateService>() as AudioUpdateCoordinator;
            }
            
            // 繝輔か繝ｼ繝ｫ繝舌ャ繧ｯ: ServiceHelper邨檎罰縺ｧ讀懃ｴ｢
            if (coordinator == null)
            {
                coordinator = ServiceHelper.GetServiceWithFallback<AudioUpdateCoordinator>();
            }
            
            if (coordinator == null)
            {
                // 蟆ら畑縺ｮGameObject繧剃ｽ懈・縺励※AudioUpdateCoordinator繧定ｿｽ蜉
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
        /// 繧ｲ繝ｼ繝迥ｶ諷九↓蠢懊§縺溘が繝ｼ繝・ぅ繧ｪ蛻ｶ蠕｡
        /// </summary>
        public void UpdateAudioForGameState(GameState state, float tensionLevel = 0f)
        {
            if (!isInitialized) return;

            currentGameState = state;
            currentTensionLevel = tensionLevel;
            isStealthModeActive = stealthCoordinator != null && stealthCoordinator.ShouldReduceNonStealthAudio();

            // BGM 縺ｮ譖ｴ譁ｰ
            if (bgmManager != null)
            {
                bgmManager.UpdateForTensionLevel(tensionLevel, isStealthModeActive);
            }

            // 迺ｰ蠅・浹縺ｮ譖ｴ譁ｰ
            if (ambientManager != null)
            {
                ambientManager.UpdateForStealthState(isStealthModeActive);
            }

            // 蜍慕噪迺ｰ蠅・す繧ｹ繝・Β縺ｨ縺ｮ騾｣謳ｺ
            if (dynamicEnvironment != null)
            {
                // DynamicAudioEnvironment 縺ｮ譌｢蟄俶ｩ溯・繧呈ｴｻ逕ｨ
                var (env, weather, time) = dynamicEnvironment.GetCurrentState();
                UpdateAudioForEnvironmentalState(env, weather, time, tensionLevel);
            }

            var eventLogger = ServiceLocator.GetService<IEventLogger>(); if (eventLogger != null) eventLogger.Log($"<color=cyan>[AudioManager]</color> Updated audio for game state: {state}, Tension: {tensionLevel:F2}, Stealth Mode: {isStealthModeActive}");
        }

        /// <summary>
        /// 迺ｰ蠅・憾諷九↓蠢懊§縺滄浹髻ｿ蛻ｶ蠕｡
        /// </summary>
        private void UpdateAudioForEnvironmentalState(EnvironmentType env, WeatherType weather, TimeOfDay time, float tension)
        {
            // 迺ｰ蠅・↓蠢懊§縺檻GM隱ｿ謨ｴ
            if (bgmManager != null)
            {
                bgmManager.UpdateForEnvironment(env, weather, time);
            }

            // 迺ｰ蠅・浹縺ｮ隱ｿ謨ｴ
            if (ambientManager != null)
            {
                ambientManager.UpdateForEnvironment(env, weather, time);
            }
        }

        #endregion

        #region Volume Control

        /// <summary>
        /// 髻ｳ驥剰ｨｭ螳壹ｒ驕ｩ逕ｨ
        /// </summary>
        public void ApplyVolumeSettings()
        {
            if (mainMixer == null) return;

            // Audio Mixer 縺ｮ繝代Λ繝｡繝ｼ繧ｿ繧呈峩譁ｰ
            SetMixerVolume(masterVolumeParam, masterVolume);
            SetMixerVolume(bgmVolumeParam, bgmVolume);
            SetMixerVolume(ambientVolumeParam, ambientVolume);
            SetMixerVolume(effectVolumeParam, effectVolume);
            SetMixerVolume(stealthVolumeParam, stealthAudioVolume);

            // 蛟句挨繝槭ロ繝ｼ繧ｸ繝｣繝ｼ縺ｫ髻ｳ驥剰ｨｭ螳壹ｒ騾夂衍
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
        /// Audio Mixer 縺ｮ髻ｳ驥上ヱ繝ｩ繝｡繝ｼ繧ｿ繧定ｨｭ螳・        /// </summary>
        private void SetMixerVolume(string paramName, float volume)
        {
            if (string.IsNullOrEmpty(paramName)) return;

            // 髻ｳ驥上ｒ dB 縺ｫ螟画鋤 (0-1 縺ｮ range 繧・-80dB - 0dB 縺ｫ螟画鋤)
            float dbValue = volume > AudioConstants.MIN_VOLUME_FOR_DB ? Mathf.Log10(volume) * 20f : AudioConstants.MIN_DB_VALUE;
            mainMixer.SetFloat(paramName, dbValue);
        }

        /// <summary>
        /// 繝槭せ繧ｿ繝ｼ髻ｳ驥上・險ｭ螳・        /// </summary>
        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            ApplyVolumeSettings();
        }

        /// <summary>
        /// BGM髻ｳ驥上・險ｭ螳・        /// </summary>
        public void SetBGMVolume(float volume)
        {
            bgmVolume = Mathf.Clamp01(volume);
            ApplyVolumeSettings();
        }

        /// <summary>
        /// 迺ｰ蠅・浹髻ｳ驥上・險ｭ螳・        /// </summary>
        public void SetAmbientVolume(float volume)
        {
            ambientVolume = Mathf.Clamp01(volume);
            ApplyVolumeSettings();
        }

        /// <summary>
        /// 蜉ｹ譫憺浹髻ｳ驥上・險ｭ螳・        /// </summary>
        public void SetEffectVolume(float volume)
        {
            effectVolume = Mathf.Clamp01(volume);
            ApplyVolumeSettings();
        }

        /// <summary>
        /// 繧ｹ繝・Ν繧ｹ髻ｳ髻ｿ髻ｳ驥上・險ｭ螳・        /// </summary>
        public void SetStealthAudioVolume(float volume)
        {
            stealthAudioVolume = Mathf.Clamp01(volume);
            ApplyVolumeSettings();
        }

        #endregion

        #region Public Interface

        /// <summary>
        /// 邱雁ｼｵ蠎ｦ繝ｬ繝吶Ν縺ｮ譖ｴ譁ｰ
        /// </summary>
        public void UpdateTensionLevel(float tension)
        {
            UpdateAudioForGameState(currentGameState, tension);
        }

        /// <summary>
        /// 繧ｹ繝・Ν繧ｹ繝｢繝ｼ繝峨・蠑ｷ蛻ｶ險ｭ螳・        /// </summary>
        public void SetStealthModeOverride(bool forceStealthMode)
        {
            if (stealthCoordinator != null)
            {
                stealthCoordinator.SetOverrideStealthMode(forceStealthMode);
                UpdateAudioForGameState(currentGameState, currentTensionLevel);
            }
        }

        /// <summary>
        /// 迴ｾ蝨ｨ縺ｮ髻ｳ髻ｿ迥ｶ諷九ｒ蜿門ｾ・        /// </summary>
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
        /// 繧ｪ繝ｼ繝・ぅ繧ｪ繧ｷ繧ｹ繝・Β縺ｮ荳譎ょ●豁｢
        /// </summary>
        public void PauseAllAudio()
        {
            if (bgmManager != null) bgmManager.PauseAll();
            if (ambientManager != null) ambientManager.PauseAll();
            if (effectManager != null) effectManager.StopAllEffects();
        }

        /// <summary>
        /// 繧ｪ繝ｼ繝・ぅ繧ｪ繧ｷ繧ｹ繝・Β縺ｮ蜀埼幕
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
        /// 繧ｵ繧ｦ繝ｳ繝峨ｒ蜀咲函
        /// </summary>
        public void PlaySound(string soundId, Vector3 position = default, float volume = 1f)
        {
            if (!isInitialized)
            {
                var eventLogger = ServiceLocator.GetService<IEventLogger>(); if (eventLogger != null) eventLogger.LogWarning("[AudioManager] System not initialized");
                return;
            }
            
            // 蜉ｹ譫憺浹縺ｨ縺励※蜀咲函
            if (effectManager != null)
            {
                effectManager.PlayEffect(soundId, position, volume * effectVolume * masterVolume);
            }
        }
        
        /// <summary>
        /// 繧ｵ繧ｦ繝ｳ繝峨ｒ蛛懈ｭ｢
        /// </summary>
        public void StopSound(string soundId)
        {
            if (effectManager != null)
            {
                // 蛟句挨蛛懈ｭ｢讖溯・縺後↑縺・◆繧√∝・縺ｦ蛛懈ｭ｢
                effectManager.StopAllEffects();
            }
        }
        
        /// <summary>
        /// 縺吶∋縺ｦ縺ｮ繧ｵ繧ｦ繝ｳ繝峨ｒ蛛懈ｭ｢
        /// </summary>
        public void StopAllSounds()
        {
            PauseAllAudio();
        }
        
        /// <summary>
        /// 繝槭せ繧ｿ繝ｼ繝懊Μ繝･繝ｼ繝繧貞叙蠕・        /// </summary>
        public float GetMasterVolume()
        {
            return masterVolume;
        }
        
        /// <summary>
        /// BGM繝懊Μ繝･繝ｼ繝繧貞叙蠕・        /// </summary>
        public float GetBGMVolume()
        {
            return bgmVolume;
        }
        
        /// <summary>
        /// 繧｢繝ｳ繝薙お繝ｳ繝医・繝ｪ繝･繝ｼ繝繧貞叙蠕・        /// </summary>
        public float GetAmbientVolume()
        {
            return ambientVolume;
        }
        
        /// <summary>
        /// 繧ｨ繝輔ぉ繧ｯ繝医・繝ｪ繝･繝ｼ繝繧貞叙蠕・        /// </summary>
        public float GetEffectVolume()
        {
            return effectVolume;
        }
        
        /// <summary>
        /// 繧ｫ繝・ざ繝ｪ蛻･縺ｮ繝懊Μ繝･繝ｼ繝繧定ｨｭ螳・        /// </summary>
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
        /// 繧ｵ繧ｦ繝ｳ繝峨′蜀咲函荳ｭ縺狗｢ｺ隱・        /// </summary>
        public bool IsPlaying(string soundId)
        {
            // EffectManager縺ｫ縺ｯ蛟句挨縺ｮ迥ｶ諷九メ繧ｧ繝・け讖溯・縺後↑縺・◆繧√∽ｻｮ螳溯｣・            // TODO: 蛟句挨繧ｵ繧ｦ繝ｳ繝峨・蜀咲函迥ｶ諷九ヨ繝ｩ繝・く繝ｳ繧ｰ讖溯・繧定ｿｽ蜉
            return false;
        }
        
        /// <summary>
        /// 荳譎ょ●豁｢
        /// </summary>
        public void Pause()
        {
            PauseAllAudio();
        }
        
        /// <summary>
        /// 蜀埼幕
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
            // 繧ｨ繝・ぅ繧ｿ縺ｧ縺ｮ蛟､螟画峩譎ゅ↓髻ｳ驥上ｒ蜊ｳ蠎ｧ縺ｫ驕ｩ逕ｨ
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
    /// 繧ｲ繝ｼ繝迥ｶ諷九・螳夂ｾｩ
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
    /// 髻ｳ髻ｿ繧ｷ繧ｹ繝・Β迥ｶ諷九・讒矩菴・    /// </summary>
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