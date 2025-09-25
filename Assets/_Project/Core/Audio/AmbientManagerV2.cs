using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using asterivo.Unity60.Core.Audio.Data;
using asterivo.Unity60.Core.Audio.Events;
using asterivo.Unity60.Core.Audio.Controllers;
using asterivo.Unity60.Core.Events;
// using asterivo.Unity60.Core.Debug;
using asterivo.Unity60.Core;
// using asterivo.Unity60.Core.Shared;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Core.Audio
{
    /// <summary>
    /// 迺ｰ蠅・浹繝槭ロ繝ｼ繧ｸ繝｣繝ｼ V2
    /// 蜊倅ｸ雋ｬ莉ｻ蜴溷援縺ｫ蝓ｺ縺･縺榊・蜑ｲ縺輔ｌ縺溘さ繝ｳ繝医Ο繝ｼ繝ｩ繝ｼ繧堤ｵｱ蜷育ｮ｡逅・    /// 譌ｧAmbientManager縺九ｉ縺ｮ繝ｪ繝輔ぃ繧ｯ繧ｿ繝ｪ繝ｳ繧ｰ迚・    /// </summary>
    public class AmbientManagerV2 : MonoBehaviour
    {
        [TabGroup("Ambient Manager V2", "Controller References")]
        [Header("Specialized Controllers")]
        [SerializeField, Required] private WeatherAmbientController weatherController;
        [SerializeField, Required] private TimeAmbientController timeController;
        [SerializeField, Required] private MaskingEffectController maskingController;

        [TabGroup("Ambient Manager V2", "Environment Sounds")]
        [Header("Environment Ambient Sounds")]
        [SerializeField] private AmbientSoundCollection[] environmentSounds;

        [TabGroup("Ambient Manager V2", "Audio Setup")]
        [Header("Audio Source Pool")]
        [SerializeField, Range(2, 8)] private int environmentSourceCount = AudioConstants.DEFAULT_AUDIOSOURCE_POOL_SIZE;
        [SerializeField] private AudioMixerGroup environmentMixerGroup;

        [TabGroup("Ambient Manager V2", "Transition Settings")]
        [Header("Environment Transition")]
        [SerializeField, Range(0.1f, 5f)] private float environmentTransitionTime = AudioConstants.ENVIRONMENT_TRANSITION_TIME;
        [SerializeField] private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [TabGroup("Ambient Manager V2", "Events")]
        [Header("Event Integration")]
        [SerializeField] private GameEvent ambientSystemInitializedEvent;
        [SerializeField] private AudioEvent environmentSoundTriggeredEvent;

        [TabGroup("Ambient Manager V2", "Runtime Info")]
        [Header("Runtime Information")]
        [SerializeField, ReadOnly] private EnvironmentType currentEnvironment = EnvironmentType.Outdoor;
        [SerializeField, ReadOnly] private bool isStealthModeActive = false;
        [SerializeField, ReadOnly] private float masterVolume = AudioConstants.DEFAULT_AMBIENT_VOLUME;
        [SerializeField, ReadOnly] private int activeEnvironmentSources = 0;

        // 迺ｰ蠅・浹蟆ら畑邂｡逅・        private AudioSource[] environmentSources;
        private System.Collections.Generic.Dictionary<EnvironmentType, AmbientSoundCollection> environmentSoundLookup;
        private System.Collections.Generic.List<EnvironmentAmbientLayer> activeEnvironmentLayers = new System.Collections.Generic.List<EnvironmentAmbientLayer>();
        private Coroutine environmentTransition;

        // 繧ｷ繧ｹ繝・Β蜿ら・
        private StealthAudioCoordinator stealthCoordinator;
        private Transform listenerTransform;

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeAmbientManagerV2();
        }

        private void Start()
        {
            SetupEnvironmentAudioSources();
            FindSystemReferences();
            BuildEnvironmentLookupDictionaries();
            ValidateControllers();
            StartInitialAmbient();
        }

        private void Update()
        {
            // 笨・ServiceLocator蟆ら畑螳溯｣・- AudioUpdateService繧貞叙蠕・            var audioUpdateService = GetAudioUpdateService();
            if (audioUpdateService != null && audioUpdateService.IsCoordinatedUpdateEnabled)
            {
                return; // 蜊碑ｪｿ譖ｴ譁ｰ繧ｷ繧ｹ繝・Β縺悟・逅・☆繧九◆繧√せ繧ｭ繝・・
            }
            
            // 繝輔か繝ｼ繝ｫ繝舌ャ繧ｯ・壼ｾ捺擂縺ｮ譖ｴ譁ｰ蜃ｦ逅・            UpdateVolumeForStealthState();
        }

        #endregion

        #region Initialization

        /// <summary>
        /// AmbientManager V2縺ｮ蛻晄悄蛹・        /// </summary>
        private void InitializeAmbientManagerV2()
        {
            environmentSoundLookup = new System.Collections.Generic.Dictionary<EnvironmentType, AmbientSoundCollection>();
            activeEnvironmentLayers = new System.Collections.Generic.List<EnvironmentAmbientLayer>();
            
            EventLogger.LogStatic("<color=cyan>[AmbientManagerV2]</color> Ambient Manager V2 initialized with controller separation");
        }

        /// <summary>
        /// 迺ｰ蠅・浹逕ｨAudioSource縺ｮ險ｭ螳・        /// </summary>
        private void SetupEnvironmentAudioSources()
        {
            environmentSources = new AudioSource[environmentSourceCount];
            
            for (int i = 0; i < environmentSourceCount; i++)
            {
                GameObject sourceObject = new GameObject($"EnvironmentSource_{i}");
                sourceObject.transform.SetParent(transform);
                
                environmentSources[i] = sourceObject.AddComponent<AudioSource>();
                ConfigureEnvironmentAudioSource(environmentSources[i]);
            }
            
            EventLogger.LogStatic($"<color=cyan>[AmbientManagerV2]</color> Created {environmentSourceCount} environment audio sources");
        }

        /// <summary>
        /// 迺ｰ蠅・浹AudioSource縺ｮ蝓ｺ譛ｬ險ｭ螳・        /// </summary>
        private void ConfigureEnvironmentAudioSource(AudioSource source)
        {
            source.outputAudioMixerGroup = environmentMixerGroup;
            source.loop = true;
            source.playOnAwake = false;
            source.spatialBlend = AudioConstants.SPATIAL_BLEND_2D; // 迺ｰ蠅・浹縺ｯ騾壼ｸｸ2D
            source.volume = 0f; // 蛻晄悄迥ｶ諷九〒縺ｯ辟｡髻ｳ
            source.priority = AudioConstants.AMBIENT_AUDIO_PRIORITY;
        }

        /// <summary>
        /// 繧ｷ繧ｹ繝・Β蜿ら・縺ｮ讀懃ｴ｢
        /// </summary>
        private void FindSystemReferences()
        {
            // StealthAudioCoordinator繧呈､懃ｴ｢
            stealthCoordinator = FindFirstObjectByType<StealthAudioCoordinator>();
            
            // AudioListener繧呈､懃ｴ｢
            var audioListener = FindFirstObjectByType<AudioListener>();
            if (audioListener != null)
            {
                listenerTransform = audioListener.transform;
            }
        }

        /// <summary>
        /// 迺ｰ蠅・浹讀懃ｴ｢霎樊嶌縺ｮ讒狗ｯ・        /// </summary>
        private void BuildEnvironmentLookupDictionaries()
        {
            environmentSoundLookup.Clear();
            
            foreach (var collection in environmentSounds)
            {
                if (collection != null && !environmentSoundLookup.ContainsKey(collection.environmentType))
                {
                    environmentSoundLookup[collection.environmentType] = collection;
                }
            }
            
            EventLogger.LogStatic($"<color=cyan>[AmbientManagerV2]</color> Built lookup for {environmentSoundLookup.Count} environment types");
        }

        /// <summary>
        /// 繧ｳ繝ｳ繝医Ο繝ｼ繝ｩ繝ｼ縺ｮ讀懆ｨｼ
        /// </summary>
        private void ValidateControllers()
        {
            bool hasErrors = false;

            if (weatherController == null)
            {
                ServiceLocator.GetService<IEventLogger>().LogError("[AmbientManagerV2] WeatherAmbientController is required!");
                hasErrors = true;
            }

            if (timeController == null)
            {
                ServiceLocator.GetService<IEventLogger>().LogError("[AmbientManagerV2] TimeAmbientController is required!");
                hasErrors = true;
            }

            if (maskingController == null)
            {
                ServiceLocator.GetService<IEventLogger>().LogError("[AmbientManagerV2] MaskingEffectController is required!");
                hasErrors = true;
            }

            if (!hasErrors)
            {
                EventLogger.LogStatic("<color=green>[AmbientManagerV2]</color> All controllers validated successfully");
            }
        }

        /// <summary>
        /// 蛻晄悄迺ｰ蠅・浹縺ｮ髢句ｧ・        /// </summary>
        private void StartInitialAmbient()
        {
            // 迴ｾ蝨ｨ縺ｮ迺ｰ蠅・↓蠢懊§縺ｦ蛻晄悄髻ｳ髻ｿ繧帝幕蟋・            ChangeEnvironment(currentEnvironment);
            
            if (ambientSystemInitializedEvent != null)
            {
                ambientSystemInitializedEvent.Raise();
            }
        }

        #endregion

        #region Public Interface

        /// <summary>
        /// 迺ｰ蠅・・螟画峩
        /// </summary>
        public void ChangeEnvironment(EnvironmentType newEnvironment)
        {
            if (newEnvironment == currentEnvironment)
                return;

            if (environmentTransition != null)
            {
                StopCoroutine(environmentTransition);
            }

            environmentTransition = StartCoroutine(EnvironmentTransitionCoroutine(newEnvironment));
        }

        /// <summary>
        /// 螟ｩ豌励・螟画峩・・eatherController縺ｫ蟋碑ｭｲ・・        /// </summary>
        public void ChangeWeather(WeatherType newWeather)
        {
            if (weatherController != null)
            {
                weatherController.ChangeWeather(newWeather);
            }
        }

        /// <summary>
        /// 譎る俣蟶ｯ縺ｮ螟画峩・・imeController縺ｫ蟋碑ｭｲ・・        /// </summary>
        public void ChangeTimeOfDay(TimeOfDay newTime)
        {
            if (timeController != null)
            {
                timeController.ChangeTimeOfDay(newTime);
            }
        }

        /// <summary>
        /// 繧ｹ繝・Ν繧ｹ迥ｶ諷九・譖ｴ譁ｰ
        /// </summary>
        public void UpdateForStealthState(bool isStealthActive)
        {
            isStealthModeActive = isStealthActive;
            
            // 繝槭せ繧ｭ繝ｳ繧ｰ繧ｳ繝ｳ繝医Ο繝ｼ繝ｩ繝ｼ縺ｫ騾夂衍
            if (maskingController != null)
            {
                maskingController.SetDynamicMasking(isStealthActive);
            }
        }

        /// <summary>
        /// 迺ｰ蠅・憾諷九・荳諡ｬ譖ｴ譁ｰ
        /// </summary>
        public void UpdateForEnvironment(EnvironmentType env, WeatherType weather, TimeOfDay time)
        {
            ChangeEnvironment(env);
            ChangeWeather(weather);
            ChangeTimeOfDay(time);
        }

        /// <summary>
        /// 繝槭せ繧ｿ繝ｼ繝懊Μ繝･繝ｼ繝縺ｮ險ｭ螳・        /// </summary>
        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            
            // 蜈ｨ繧ｳ繝ｳ繝医Ο繝ｼ繝ｩ繝ｼ縺ｫ髻ｳ驥剰ｨｭ螳壹ｒ莨晄眺
            if (weatherController != null)
                weatherController.SetMasterVolume(masterVolume);
                
            if (timeController != null)
                timeController.SetMasterVolume(masterVolume);
            
            UpdateAllEnvironmentVolumes();
        }

        /// <summary>
        /// 蜈ｨ菴薙・荳譎ょ●豁｢
        /// </summary>
        public void PauseAll()
        {
            foreach (var source in environmentSources)
            {
                if (source != null && source.isPlaying)
                {
                    source.Pause();
                }
            }
            
            if (weatherController != null) weatherController.PauseAll();
            if (timeController != null) timeController.PauseAll();
        }

        /// <summary>
        /// 蜈ｨ菴薙・蜀埼幕
        /// </summary>
        public void ResumeAll()
        {
            foreach (var source in environmentSources)
            {
                if (source != null)
                {
                    source.UnPause();
                }
            }
            
            if (weatherController != null) weatherController.ResumeAll();
            if (timeController != null) timeController.ResumeAll();
        }

        /// <summary>
        /// 蜈ｨ髻ｳ髻ｿ繧ｷ繧ｹ繝・Β縺ｮ蛛懈ｭ｢
        /// </summary>
        public void StopAllAmbientSounds()
        {
            // 迺ｰ蠅・浹縺ｮ蛛懈ｭ｢
            if (environmentTransition != null)
            {
                StopCoroutine(environmentTransition);
                environmentTransition = null;
            }
            
            foreach (var source in environmentSources)
            {
                if (source != null)
                {
                    source.Stop();
                }
            }
            
            activeEnvironmentLayers.Clear();
            
            // 蜷・さ繝ｳ繝医Ο繝ｼ繝ｩ繝ｼ縺ｮ蛛懈ｭ｢
            if (weatherController != null) weatherController.StopAllWeatherSounds();
            if (timeController != null) timeController.StopAllTimeSounds();
            if (maskingController != null) maskingController.StopAllMaskingEffects();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// 迺ｰ蠅・・遘ｻ縺ｮ繧ｳ繝ｫ繝ｼ繝√Φ
        /// </summary>
        private IEnumerator EnvironmentTransitionCoroutine(EnvironmentType newEnvironment)
        {
            currentEnvironment = newEnvironment;

            EventLogger.LogStatic($"<color=cyan>[AmbientManagerV2]</color> Starting environment transition to {newEnvironment}");

            // 譁ｰ縺励＞迺ｰ蠅・浹髻ｿ繧貞叙蠕・            if (!environmentSoundLookup.TryGetValue(newEnvironment, out var environmentCollection))
            {
                ServiceLocator.GetService<IEventLogger>().LogWarning($"[AmbientManagerV2] No sound collection found for environment: {newEnvironment}");
                yield break;
            }

            // 蛻ｩ逕ｨ蜿ｯ閭ｽ縺ｪ繧ｪ繝ｼ繝・ぅ繧ｪ繧ｽ繝ｼ繧ｹ繧呈爾縺・            AudioSource availableSource = GetAvailableEnvironmentSource();
            if (availableSource == null)
            {
                ServiceLocator.GetService<IEventLogger>().LogWarning("[AmbientManagerV2] No available audio sources for environment transition");
                yield break;
            }

            // 譁ｰ縺励＞迺ｰ蠅・Ξ繧､繝､繝ｼ繧剃ｽ懈・
            var newLayer = CreateEnvironmentLayer(environmentCollection, availableSource);
            if (newLayer != null)
            {
                yield return StartCoroutine(CrossfadeToNewEnvironmentLayer(availableSource, newLayer, environmentTransitionTime));
            }

            // 蜿､縺・Ξ繧､繝､繝ｼ繧偵ヵ繧ｧ繝ｼ繝峨い繧ｦ繝・            var layersToRemove = new System.Collections.Generic.List<EnvironmentAmbientLayer>(activeEnvironmentLayers);
            foreach (var layer in layersToRemove)
            {
                if (layer != newLayer)
                {
                    yield return StartCoroutine(FadeOutEnvironmentLayer(layer, environmentTransitionTime));
                }
            }

            EventLogger.LogStatic($"<color=cyan>[AmbientManagerV2]</color> Completed environment transition to {newEnvironment}");

            // 繧､繝吶Φ繝育匱轣ｫ
            if (environmentSoundTriggeredEvent != null)
            {
                var eventData = new AudioEventData
                {
                    audioClip = newLayer?.audioSource?.clip,
                    volume = masterVolume,
                    category = AudioCategory.Ambient,
                    worldPosition = listenerTransform?.position ?? Vector3.zero
                };
                environmentSoundTriggeredEvent.Raise(eventData);
            }
        }

        /// <summary>
        /// 蛻ｩ逕ｨ蜿ｯ閭ｽ縺ｪ迺ｰ蠅・浹AudioSource繧貞叙蠕・        /// </summary>
        private AudioSource GetAvailableEnvironmentSource()
        {
            foreach (var source in environmentSources)
            {
                if (!source.isPlaying)
                {
                    return source;
                }
            }
            return environmentSources[0]; // 蜈ｨ縺ｦ菴ｿ逕ｨ荳ｭ縺ｮ蝣ｴ蜷医・譛蛻昴・繧ゅ・繧剃ｽｿ逕ｨ
        }

        /// <summary>
        /// 迺ｰ蠅・浹繝ｬ繧､繝､繝ｼ縺ｮ菴懈・
        /// </summary>
        private EnvironmentAmbientLayer CreateEnvironmentLayer(AmbientSoundCollection collection, AudioSource audioSource)
        {
            var randomClip = collection.GetRandomClip();
            if (randomClip == null)
            {
                return null;
            }

            var layer = new EnvironmentAmbientLayer
            {
                environmentType = collection.environmentType,
                audioSource = audioSource,
                targetVolume = collection.baseVolume * masterVolume,
                fadeSpeed = 1f / environmentTransitionTime
            };

            audioSource.clip = randomClip;
            audioSource.volume = 0f;
            audioSource.Play();

            return layer;
        }

        /// <summary>
        /// 譁ｰ縺励＞迺ｰ蠅・Ξ繧､繝､繝ｼ縺ｸ縺ｮ繧ｯ繝ｭ繧ｹ繝輔ぉ繝ｼ繝・        /// </summary>
        private IEnumerator CrossfadeToNewEnvironmentLayer(AudioSource source, EnvironmentAmbientLayer layer, float duration)
        {
            float elapsed = 0f;
            float startVolume = source.volume;

            activeEnvironmentLayers.Add(layer);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = transitionCurve.Evaluate(elapsed / duration);
                source.volume = Mathf.Lerp(startVolume, layer.targetVolume, progress);
                yield return null;
            }

            source.volume = layer.targetVolume;
        }

        /// <summary>
        /// 迺ｰ蠅・Ξ繧､繝､繝ｼ縺ｮ繝輔ぉ繝ｼ繝峨い繧ｦ繝・        /// </summary>
        private IEnumerator FadeOutEnvironmentLayer(EnvironmentAmbientLayer layer, float duration)
        {
            if (layer?.audioSource == null) yield break;

            float elapsed = 0f;
            float startVolume = layer.audioSource.volume;

            while (elapsed < duration && layer.audioSource != null)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;
                layer.audioSource.volume = Mathf.Lerp(startVolume, 0f, progress);
                yield return null;
            }

            if (layer.audioSource != null)
            {
                layer.audioSource.Stop();
                layer.audioSource.volume = 0f;
            }

            activeEnvironmentLayers.Remove(layer);
        }

        /// <summary>
        /// 繧ｹ繝・Ν繧ｹ迥ｶ諷九↓蠢懊§縺滄浹驥剰ｪｿ謨ｴ
        /// </summary>
        private void UpdateVolumeForStealthState()
        {
            if (stealthCoordinator != null)
            {
                bool shouldReduce = stealthCoordinator.ShouldReduceNonStealthAudio();
                if (shouldReduce != isStealthModeActive)
                {
                    UpdateForStealthState(shouldReduce);
                }
            }
        }

        /// <summary>
        /// 蜈ｨ迺ｰ蠅・浹髻ｳ驥上・譖ｴ譁ｰ
        /// </summary>
        private void UpdateAllEnvironmentVolumes()
        {
            foreach (var layer in activeEnvironmentLayers)
            {
                if (layer?.audioSource != null)
                {
                    environmentSoundLookup.TryGetValue(layer.environmentType, out var collection);
                    if (collection != null)
                    {
                        layer.targetVolume = collection.baseVolume * masterVolume;
                        layer.audioSource.volume = layer.targetVolume;
                    }
                }
            }

            activeEnvironmentSources = activeEnvironmentLayers.Count;
        }

        #endregion
        
        #region Service Access Methods

        /// <summary>
        /// ServiceLocator蜆ｪ蜈医〒IAudioUpdateService繧貞叙蠕・        /// Phase 3遘ｻ陦後ヱ繧ｿ繝ｼ繝ｳ縺ｮ螳溯｣・        /// </summary>
        private asterivo.Unity60.Core.Audio.Interfaces.IAudioUpdateService GetAudioUpdateService()
        {
            if (asterivo.Unity60.Core.FeatureFlags.UseServiceLocator)
            {
                try
                {
                    return asterivo.Unity60.Core.ServiceLocator.GetService<asterivo.Unity60.Core.Audio.Interfaces.IAudioUpdateService>();
                }
                catch (System.Exception ex)
                {
                    ServiceLocator.GetService<IEventLogger>().LogError($"[AmbientManagerV2] Failed to get IAudioUpdateService from ServiceLocator: {ex.Message}");
                }
            }
            
            // 繝輔か繝ｼ繝ｫ繝舌ャ繧ｯ: FindFirstObjectByType (ServiceLocator蟆ら畑螳溯｣・
            if (asterivo.Unity60.Core.FeatureFlags.AllowSingletonFallback)
            {
                try
                {
                    // 笨・ServiceLocator蟆ら畑螳溯｣・- 逶ｴ謗･AudioUpdateCoordinator繧呈､懃ｴ｢
                    var coordinator = FindFirstObjectByType<AudioUpdateCoordinator>();
                    if (coordinator != null && asterivo.Unity60.Core.FeatureFlags.EnableDebugLogging)
                    {
                        EventLogger.LogStatic("[AmbientManagerV2] Found AudioUpdateCoordinator via FindFirstObjectByType");
                    }
                    
                    return coordinator;
                }
                catch (System.Exception ex)
                {
                    ServiceLocator.GetService<IEventLogger>().LogError($"[AmbientManagerV2] Failed to get AudioUpdateCoordinator: {ex.Message}");
                }
            }
            
            return null;
        }

        #endregion

        #region Helper Classes

        /// <summary>
        /// 迺ｰ蠅・浹繝ｬ繧､繝､繝ｼ
        /// </summary>
        [System.Serializable]
        private class EnvironmentAmbientLayer
        {
            public EnvironmentType environmentType;
            public AudioSource audioSource;
            public float targetVolume;
            public float fadeSpeed;
        }

        #endregion

        #region Editor Helpers

        #if UNITY_EDITOR
        [Button("Test Environment Change")]
        public void TestEnvironmentChange(EnvironmentType testEnvironment)
        {
            if (Application.isPlaying)
            {
                ChangeEnvironment(testEnvironment);
            }
        }

        [Button("Test Weather Change")]
        public void TestWeatherChange(WeatherType testWeather)
        {
            if (Application.isPlaying)
            {
                ChangeWeather(testWeather);
            }
        }

        [Button("Test Time Change")]
        public void TestTimeChange(TimeOfDay testTime)
        {
            if (Application.isPlaying)
            {
                ChangeTimeOfDay(testTime);
            }
        }

        [Button("Stop All Ambient")]
        public void EditorStopAll()
        {
            if (Application.isPlaying)
            {
                StopAllAmbientSounds();
            }
        }
        #endif

        #endregion
    }
}
