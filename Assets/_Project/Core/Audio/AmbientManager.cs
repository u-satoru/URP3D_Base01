using asterivo.Unity60.Core;
using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;
using asterivo.Unity60.Core.Audio.Data;
using asterivo.Unity60.Core.Audio.Events;
using asterivo.Unity60.Core.Events;
// using asterivo.Unity60.Core.Debug;
using asterivo.Unity60.Core.Audio.Interfaces;
// using asterivo.Unity60.Core.Services; // Removed to avoid circular dependency
using asterivo.Unity60.Core.Data;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Core.Audio
{
    /// <summary>
    /// 迺ｰ蠅・・ｽ・ｽ繝槭ロ繝ｼ繧ｸ繝｣繝ｼ
    /// 繧ｹ繝・Ν繧ｹ繧ｲ繝ｼ繝縺ｫ迚ｹ蛹悶＠縺溘・繧ｹ繧ｭ繝ｳ繧ｰ蜉ｹ譫應ｻ倥″迺ｰ蠅・浹繧ｷ繧ｹ繝・Β
    /// </summary>
    public class AmbientManager : MonoBehaviour
    {
        [TabGroup("Ambient Manager", "Sound Libraries")]
        [Header("Environment Ambient Sounds")]
        [SerializeField] private AmbientSoundCollection[] environmentSounds;

        [TabGroup("Ambient Manager", "Weather Systems")]
        [Header("Weather Ambient Sounds")]
        [SerializeField] private WeatherAmbientCollection[] weatherSounds;

        [TabGroup("Ambient Manager", "Time-based")]
        [Header("Time of Day Ambient Sounds")]
        [SerializeField] private TimeAmbientCollection[] timeSounds;

        [TabGroup("Ambient Manager", "Audio Sources")]
        [Header("Audio Source Pool")]
        [SerializeField, Range(2, 8)] private int ambientSourceCount = 4;
        [SerializeField] private AudioMixerGroup ambientMixerGroup;

        [TabGroup("Ambient Manager", "Masking System")]
        [Header("Masking Effect Settings")]
        [SerializeField, Range(0f, 1f)] private float globalMaskingStrength = 0.3f;
        [SerializeField, Range(0f, 2f)] private float maskingRadius = 15f;
        [SerializeField] private LayerMask stealthSoundLayerMask = -1;

        [TabGroup("Ambient Manager", "Dynamic Control")]
        [Header("Dynamic Control")]
        [SerializeField, Range(0.1f, 5f)] private float environmentTransitionTime = 2f;
        [SerializeField, Range(0.1f, 10f)] private float weatherTransitionTime = 3f;
        [SerializeField] private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [TabGroup("Ambient Manager", "Events")]
        [Header("Event Integration")]
        [SerializeField] private GameEvent ambientMaskingActivatedEvent;
        [SerializeField] private AudioEvent ambientSoundTriggeredEvent;

        [TabGroup("Ambient Manager", "Runtime Info")]
        [Header("Runtime Information")]
        [SerializeField, ReadOnly] private EnvironmentType currentEnvironment = EnvironmentType.Outdoor;
        [SerializeField, ReadOnly] private WeatherType currentWeather = WeatherType.Clear;
        [SerializeField, ReadOnly] private TimeOfDay currentTimeOfDay = TimeOfDay.Day;
        [SerializeField, ReadOnly] private bool isStealthModeActive = false;
        [SerializeField, ReadOnly] private float masterVolume = 1f;
        [SerializeField, ReadOnly] private int activeAmbientSources = 0;

        // 繧ｪ繝ｼ繝・ぅ繧ｪ繧ｽ繝ｼ繧ｹ邂｡逅・
        private AudioSource[] ambientSources;
        private List<AmbientLayer> activeLayers = new List<AmbientLayer>();
        private Dictionary<EnvironmentType, AmbientSoundCollection> environmentSoundLookup;
        private Dictionary<WeatherType, WeatherAmbientCollection> weatherSoundLookup;
        private Dictionary<TimeOfDay, TimeAmbientCollection> timeSoundLookup;

        // 繧ｷ繧ｹ繝・Β騾｣謳ｺ
        private StealthAudioCoordinator stealthCoordinator;
        private SpatialAudioManager spatialAudioManager;
        private Transform listenerTransform;

        // 驕ｷ遘ｻ蛻ｶ蠕｡
        private Coroutine environmentTransition;
        private Coroutine weatherTransition;

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeAmbientManager();
        }

        private void Start()
        {
            SetupAudioSources();
            FindSystemReferences();
            BuildLookupDictionaries();
            StartInitialAmbient();
        }

        private void Update()
        {
            UpdateMaskingEffects();
            UpdateVolumeForStealthState();
        }

        #endregion

        #region Initialization

        /// <summary>
        /// 迺ｰ蠅・・ｽ・ｽ繝槭ロ繝ｼ繧ｸ繝｣繝ｼ縺ｮ蛻晄悄蛹・        /// </summary>
        private void InitializeAmbientManager()
        {
            ambientSources = new AudioSource[ambientSourceCount];
        }

        /// <summary>
        /// 繧ｪ繝ｼ繝・ぅ繧ｪ繧ｽ繝ｼ繧ｹ縺ｮ繧ｻ繝・ヨ繧｢繝・・
        /// </summary>
        private void SetupAudioSources()
        {
            for (int i = 0; i < ambientSourceCount; i++)
            {
                var sourceGO = new GameObject($"AmbientSource_{i}");
                sourceGO.transform.SetParent(transform);

                var source = sourceGO.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.loop = true;
                source.spatialBlend = 0f; // 迺ｰ蠅・・ｽ・ｽ縺ｯ騾壼ｸｸ2D
                source.outputAudioMixerGroup = ambientMixerGroup;
                source.priority = 128; // 菴主━蜈亥ｺｦ

                ambientSources[i] = source;
            }
        }

        /// <summary>
        /// 繧ｷ繧ｹ繝・Β蜿ら・縺ｮ讀懃ｴ｢
        /// </summary>
        /// <summary>
        /// 繧ｷ繧ｹ繝・Β蜿ら・縺ｮ讀懃ｴ｢
        /// Phase 3遘ｻ陦後ヱ繧ｿ繝ｼ繝ｳ: ServiceLocator蜆ｪ蜈医ヾingleton繝輔か繝ｼ繝ｫ繝舌ャ繧ｯ
        /// </summary>
        private void FindSystemReferences()
        {
            if (stealthCoordinator == null)
                stealthCoordinator = FindFirstObjectByType<StealthAudioCoordinator>();

            // SpatialAudioManager蜿門ｾ・ ServiceLocator蜆ｪ蜈医ヾingleton繝輔か繝ｼ繝ｫ繝舌ャ繧ｯ
            if (spatialAudioManager == null)
            {
                spatialAudioManager = GetSpatialAudioManager();
            }

            // 繝ｪ繧ｹ繝翫・縺ｮ讀懃ｴ｢
            var audioListener = FindFirstObjectByType<AudioListener>();
            if (audioListener != null)
            {
                listenerTransform = audioListener.transform;
            }
        }
        
        /// <summary>
        /// ServiceLocator蜆ｪ蜈医〒SpatialAudioManager繧貞叙蠕・        /// Phase 3遘ｻ陦後ヱ繧ｿ繝ｼ繝ｳ縺ｮ螳溯｣・        /// </summary>
        private SpatialAudioManager GetSpatialAudioManager()
        {
            // ServiceLocator邨檎罰縺ｧ縺ｮ蜿門ｾ励ｒ隧ｦ縺ｿ繧・            if (asterivo.Unity60.Core.FeatureFlags.UseServiceLocator)
            {
                try
                {
                    var spatialService = ServiceLocator.GetService<ISpatialAudioService>();
                    if (spatialService is SpatialAudioManager manager)
                    {
                        if (asterivo.Unity60.Core.FeatureFlags.EnableDebugLogging)
                        {
                            var eventLogger = ServiceLocator.GetService<IEventLogger>(); if (eventLogger != null) eventLogger.Log("[AmbientManager] Successfully retrieved SpatialAudioManager from ServiceLocator");
                        }
                        return manager;
                    }
                }
                catch (System.Exception ex)
                {
                    var eventLogger = ServiceLocator.GetService<IEventLogger>();
                    if (eventLogger != null) {
                        eventLogger.LogError($"[AmbientManager] Failed to get SpatialAudioManager from ServiceLocator: {ex.Message}");
                    }
                }
            }
            
            // 笨・ServiceLocator蟆ら畑螳溯｣・- 逶ｴ謗･SpatialAudioManager繧呈､懃ｴ｢
            var spatialAudioManager = FindFirstObjectByType<SpatialAudioManager>();
            if (spatialAudioManager != null)
            {
                if (FeatureFlags.EnableDebugLogging)
                {
                    var eventLogger = ServiceLocator.GetService<IEventLogger>(); if (eventLogger != null) eventLogger.Log("[AmbientManager] Found SpatialAudioManager via FindFirstObjectByType");
                }
                return spatialAudioManager;
            }
            else
            {
                var eventLogger = ServiceLocator.GetService<IEventLogger>();
                if (eventLogger != null) {
                    eventLogger.LogError("[AmbientManager] No SpatialAudioManager available and legacy singletons are disabled");
                }
            }
            
            return null;
        }

        /// <summary>
        /// 繝ｫ繝ｼ繝医Ξ繧､繝､繝ｼ霎樊嶌縺ｮ讒狗ｯ・
        /// </summary>
        private void BuildLookupDictionaries()
        {
            // 迺ｰ蠅・・ｽ・ｽ霎樊嶌
            environmentSoundLookup = new Dictionary<EnvironmentType, AmbientSoundCollection>();
            foreach (var collection in environmentSounds)
            {
                if (collection != null)
                {
                    environmentSoundLookup[collection.environmentType] = collection;
                }
            }

            // 螟ｩ蛟咎浹霎樊嶌
            weatherSoundLookup = new Dictionary<WeatherType, WeatherAmbientCollection>();
            foreach (var collection in weatherSounds)
            {
                if (collection != null)
                {
                    weatherSoundLookup[collection.weatherType] = collection;
                }
            }

            // 譎る俣蟶ｯ髻ｳ霎樊嶌
            timeSoundLookup = new Dictionary<TimeOfDay, TimeAmbientCollection>();
            foreach (var collection in timeSounds)
            {
                if (collection != null)
                {
                    timeSoundLookup[collection.timeOfDay] = collection;
                }
            }
        }

        /// <summary>
        /// 蛻晄悄迺ｰ蠅・・ｽ・ｽ縺ｮ髢句ｧ・        /// </summary>
        private void StartInitialAmbient()
        {
            UpdateForEnvironment(currentEnvironment, currentWeather, currentTimeOfDay);
        }

        #endregion

        #region Public Interface

        /// <summary>
        /// 迺ｰ蠅・・ｽ・ｽ蠢懊§縺溽腸蠅・・ｽ・ｽ譖ｴ譁ｰ
        /// </summary>
        public void UpdateForEnvironment(EnvironmentType environment, WeatherType weather, TimeOfDay timeOfDay)
        {
            bool environmentChanged = currentEnvironment != environment;
            bool weatherChanged = currentWeather != weather;
            bool timeChanged = currentTimeOfDay != timeOfDay;

            currentEnvironment = environment;
            currentWeather = weather;
            currentTimeOfDay = timeOfDay;

            if (environmentChanged)
            {
                TransitionEnvironmentAmbient();
            }

            if (weatherChanged)
            {
                TransitionWeatherAmbient();
            }

            if (timeChanged)
            {
                UpdateTimeBasedAmbient();
            }

            var eventLogger = ServiceLocator.GetService<IEventLogger>(); if (eventLogger != null) eventLogger.Log($"<color=green>[AmbientManager]</color> Environment updated - Env: {environment}, Weather: {weather}, Time: {timeOfDay}");
        }

        /// <summary>
        /// 繧ｹ繝・Ν繧ｹ迥ｶ諷九↓蠢懊§縺滓峩譁ｰ
        /// </summary>
        public void UpdateForStealthState(bool stealthModeActive)
        {
            isStealthModeActive = stealthModeActive;

            // 繧ｹ繝・Ν繧ｹ迥ｶ諷九↓蠢懊§縺滄浹驥剰ｪｿ謨ｴ縺ｯ UpdateVolumeForStealthState 縺ｧ蜃ｦ逅・
        }

        /// <summary>
        /// 繝槭せ繧ｿ繝ｼ髻ｳ驥上・險ｭ螳・
        /// </summary>
        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            UpdateAllSourceVolumes();
        }

        /// <summary>
        /// 蜈ｨ迺ｰ蠅・・ｽ・ｽ縺ｮ荳譎ょ●豁｢
        /// </summary>
        public void PauseAll()
        {
            foreach (var source in ambientSources)
            {
                if (source.isPlaying)
                {
                    source.Pause();
                }
            }
        }

        /// <summary>
        /// 蜈ｨ迺ｰ蠅・・ｽ・ｽ縺ｮ蜀埼幕
        /// </summary>
        public void ResumeAll()
        {
            foreach (var source in ambientSources)
            {
                source.UnPause();
            }
        }

        /// <summary>
        /// 迚ｹ螳壻ｽ咲ｽｮ縺ｧ縺ｮ迺ｰ蠅・・ｽ・ｽ繝槭せ繧ｭ繝ｳ繧ｰ蠑ｷ蠎ｦ繧貞叙蠕・        /// </summary>
        public float GetMaskingStrengthAtPosition(Vector3 position)
        {
            float totalMasking = 0f;

            foreach (var layer in activeLayers)
            {
                if (layer.providesStealthMasking)
                {
                    float distance = Vector3.Distance(position, listenerTransform.position);
                    if (distance <= maskingRadius)
                    {
                        float distanceFactor = 1f - (distance / maskingRadius);
                        totalMasking += layer.maskingStrength * distanceFactor;
                    }
                }
            }

            return Mathf.Clamp01(totalMasking * globalMaskingStrength);
        }

        #endregion

        #region Environment Transitions

        /// <summary>
        /// 迺ｰ蠅・・ｽ・ｽ縺ｮ驕ｷ遘ｻ
        /// </summary>
        private void TransitionEnvironmentAmbient()
        {
            if (environmentTransition != null)
            {
                StopCoroutine(environmentTransition);
            }

            environmentTransition = StartCoroutine(EnvironmentTransitionCoroutine());
        }

        /// <summary>
        /// 螟ｩ蛟咎浹縺ｮ驕ｷ遘ｻ
        /// </summary>
        private void TransitionWeatherAmbient()
        {
            if (weatherTransition != null)
            {
                StopCoroutine(weatherTransition);
            }

            weatherTransition = StartCoroutine(WeatherTransitionCoroutine());
        }

        /// <summary>
        /// 迺ｰ蠅・浹驕ｷ遘ｻ縺ｮ繧ｳ繝ｫ繝ｼ繝√Φ
        /// </summary>
        private IEnumerator EnvironmentTransitionCoroutine()
        {
            var targetCollection = environmentSoundLookup.GetValueOrDefault(currentEnvironment);
            
            if (targetCollection != null)
            {
                // 譁ｰ縺励＞迺ｰ蠅・・ｽ・ｽ繧､繝､繝ｼ繧呈ｺ門ｙ
                var newLayer = new AmbientLayer
                {
                    collection = targetCollection,
                    layerType = AmbientLayerType.Environment,
                    targetVolume = targetCollection.baseVolume,
                    maskingStrength = targetCollection.maskingStrength,
                    providesStealthMasking = targetCollection.providesStealthMasking
                };

                // 蛻ｩ逕ｨ蜿ｯ閭ｽ縺ｪ繧ｽ繝ｼ繧ｹ繧呈､懃ｴ｢
                var availableSource = GetAvailableAmbientSource();
                if (availableSource != null)
                {
                    yield return StartCoroutine(CrossfadeToNewLayer(availableSource, newLayer, environmentTransitionTime));
                }
            }

            environmentTransition = null;
        }

        /// <summary>
        /// 螟ｩ蛟咎・遘ｻ縺ｮ繧ｳ繝ｫ繝ｼ繝√Φ
        /// </summary>
        private IEnumerator WeatherTransitionCoroutine()
        {
            var targetCollection = weatherSoundLookup.GetValueOrDefault(currentWeather);
            
            if (targetCollection != null && targetCollection.weatherClip != null)
            {
                var newLayer = new AmbientLayer
                {
                    collection = targetCollection,
                    layerType = AmbientLayerType.Weather,
                    targetVolume = targetCollection.intensity,
                    maskingStrength = targetCollection.maskingEffect,
                    providesStealthMasking = true // 螟ｩ蛟吶・騾壼ｸｸ繝槭せ繧ｭ繝ｳ繧ｰ蜉ｹ譫懊≠繧・
                };

                var availableSource = GetAvailableAmbientSource();
                if (availableSource != null)
                {
                    yield return StartCoroutine(CrossfadeToNewLayer(availableSource, newLayer, weatherTransitionTime));
                }
            }

            weatherTransition = null;
        }

        /// <summary>
        /// 譁ｰ縺励＞繝ｬ繧､繝､繝ｼ縺ｸ縺ｮ繧ｯ繝ｭ繧ｹ繝輔ぉ繝ｼ繝・        /// </summary>
        private IEnumerator CrossfadeToNewLayer(AudioSource source, AmbientLayer newLayer, float transitionTime)
        {
            // 蜿､縺・Ξ繧､繝､繝ｼ縺ｮ繝輔ぉ繝ｼ繝峨い繧ｦ繝茨ｼ亥酔縺倥ち繧､繝励′縺ゅｌ縺ｰ・・
            var existingLayer = activeLayers.Find(layer => layer.layerType == newLayer.layerType);
            if (existingLayer != null)
            {
                yield return StartCoroutine(FadeOutLayer(existingLayer, transitionTime));
                activeLayers.Remove(existingLayer);
            }

            // 譁ｰ縺励＞繝ｬ繧､繝､繝ｼ縺ｮ險ｭ螳・            AudioClip targetClip = GetClipFromLayer(newLayer);
            if (targetClip != null)
            {
                source.clip = targetClip;
                source.volume = 0f;
                source.Play();

                // 繝輔ぉ繝ｼ繝峨う繝ｳ
                float currentTime = 0f;
                float targetVolume = CalculateLayerVolume(newLayer);

                while (currentTime < transitionTime)
                {
                    currentTime += Time.deltaTime;
                    float t = transitionCurve.Evaluate(currentTime / transitionTime);
                    source.volume = Mathf.Lerp(0f, targetVolume, t);
                    yield return null;
                }

                source.volume = targetVolume;
                newLayer.audioSource = source;
                activeLayers.Add(newLayer);

                // 繧､繝吶Φ繝育匱陦・                if (newLayer.providesStealthMasking)
                {
                    ambientMaskingActivatedEvent?.Raise();
                }

                // 繧ｹ繝・Ν繧ｹ髻ｳ髻ｿ繧､繝吶Φ繝医・逋ｺ轣ｫ
                if (ambientSoundTriggeredEvent != null)
                {
                    var eventData = AudioEventData.CreateAmbientDefault(targetClip.name);
                    eventData.maskingStrength = newLayer.maskingStrength;
                    ambientSoundTriggeredEvent.Raise(eventData);
                }
            }

            activeAmbientSources = activeLayers.Count;
        }

        /// <summary>
        /// 繝ｬ繧､繝､繝ｼ縺ｮ繝輔ぉ繝ｼ繝峨い繧ｦ繝・        /// </summary>
        private IEnumerator FadeOutLayer(AmbientLayer layer, float fadeTime)
        {
            if (layer.audioSource == null) yield break;

            float startVolume = layer.audioSource.volume;
            float currentTime = 0f;

            while (currentTime < fadeTime)
            {
                currentTime += Time.deltaTime;
                float t = currentTime / fadeTime;
                layer.audioSource.volume = Mathf.Lerp(startVolume, 0f, t);
                yield return null;
            }

            layer.audioSource.Stop();
            layer.audioSource.volume = 0f;
        }

        #endregion

        #region Volume and Masking Control

        /// <summary>
        /// 繝ｬ繧､繝､繝ｼ縺ｮ髻ｳ驥剰ｨ育ｮ・        /// </summary>
        private float CalculateLayerVolume(AmbientLayer layer)
        {
            float volume = layer.targetVolume * masterVolume;

            // 繧ｹ繝・Ν繧ｹ繝｢繝ｼ繝画凾縺ｮ隱ｿ謨ｴ
            if (isStealthModeActive && stealthCoordinator != null)
            {
                volume *= stealthCoordinator.GetCategoryVolumeMultiplier(AudioCategory.Ambient);
            }

            return Mathf.Clamp01(volume);
        }

        /// <summary>
        /// 繧ｹ繝・Ν繧ｹ迥ｶ諷九↓蠢懊§縺滄浹驥乗峩譁ｰ
        /// </summary>
        private void UpdateVolumeForStealthState()
        {
            foreach (var layer in activeLayers)
            {
                if (layer.audioSource != null && layer.audioSource.isPlaying)
                {
                    float targetVolume = CalculateLayerVolume(layer);
                    
                    // 繧ｹ繝繝ｼ繧ｺ縺ｪ髻ｳ驥丞､牙喧
                    if (Mathf.Abs(layer.audioSource.volume - targetVolume) > 0.01f)
                    {
                        layer.audioSource.volume = Mathf.Lerp(
                            layer.audioSource.volume, 
                            targetVolume, 
                            Time.deltaTime * 2f
                        );
                    }
                }
            }
        }

        /// <summary>
        /// 蜈ｨ繧ｽ繝ｼ繧ｹ縺ｮ髻ｳ驥乗峩譁ｰ
        /// </summary>
        private void UpdateAllSourceVolumes()
        {
            foreach (var layer in activeLayers)
            {
                if (layer.audioSource != null)
                {
                    layer.audioSource.volume = CalculateLayerVolume(layer);
                }
            }
        }

        /// <summary>
        /// 繝槭せ繧ｭ繝ｳ繧ｰ蜉ｹ譫懊・譖ｴ譁ｰ
        /// </summary>
        private void UpdateMaskingEffects()
        {
            // 繝槭せ繧ｭ繝ｳ繧ｰ蜉ｹ譫懊・蜍慕噪險育ｮ励→驕ｩ逕ｨ
            // 螳滄圀縺ｯ StealthAudioCoordinator 縺ｨ縺ｮ騾｣謳ｺ縺ｧ陦後≧
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// 蛻ｩ逕ｨ蜿ｯ閭ｽ縺ｪ迺ｰ蠅・・ｽ・ｽ繧ｽ繝ｼ繧ｹ繧貞叙蠕・        /// </summary>
        private AudioSource GetAvailableAmbientSource()
        {
            // 菴ｿ逕ｨ荳ｭ縺ｧ縺ｪ縺・・ｽ・ｽ繝ｼ繧ｹ繧呈､懃ｴ｢
            foreach (var source in ambientSources)
            {
                if (!source.isPlaying)
                {
                    return source;
                }
            }

            // 蜈ｨ縺ｦ菴ｿ逕ｨ荳ｭ縺ｮ蝣ｴ蜷医・譛繧ょ商縺・ｂ縺ｮ繧堤ｽｮ縺肴鋤縺・
            return ambientSources[0];
        }

        /// <summary>
        /// 繝ｬ繧､繝､繝ｼ縺九ｉ繧ｯ繝ｪ繝・・繧貞叙蠕・
        /// </summary>
        private AudioClip GetClipFromLayer(AmbientLayer layer)
        {
            switch (layer.layerType)
            {
                case AmbientLayerType.Environment:
                    if (layer.collection is AmbientSoundCollection envCollection)
                        return envCollection.GetRandomClip();
                    break;
                    
                case AmbientLayerType.Weather:
                    if (layer.collection is WeatherAmbientCollection weatherCollection)
                        return weatherCollection.weatherClip;
                    break;
                    
                case AmbientLayerType.TimeOfDay:
                    if (layer.collection is TimeAmbientCollection timeCollection)
                        return timeCollection.GetRandomClip();
                    break;
            }
            return null;
        }

        /// <summary>
        /// 譎る俣蟶ｯ繝吶・繧ｹ縺ｮ迺ｰ蠅・浹譖ｴ譁ｰ
        /// </summary>
        private void UpdateTimeBasedAmbient()
        {
            var timeCollection = timeSoundLookup.GetValueOrDefault(currentTimeOfDay);
            if (timeCollection != null)
            {
                // 譎る俣蟶ｯ縺ｮ迺ｰ蠅・・ｽ・ｽ縺ｯ菴主━蜈亥ｺｦ縺ｧ閭梧勹縺ｫ霑ｽ蜉
                var timeLayer = new AmbientLayer
                {
                    collection = timeCollection,
                    layerType = AmbientLayerType.TimeOfDay,
                    targetVolume = timeCollection.baseVolume * 0.5f, // 譎る俣蟶ｯ髻ｳ縺ｯ謗ｧ縺医ａ縺ｫ
                    maskingStrength = 0.1f,
                    providesStealthMasking = false
                };

                var source = GetAvailableAmbientSource();
                if (source != null)
                {
                    StartCoroutine(CrossfadeToNewLayer(source, timeLayer, 1f));
                }
            }
        }

        #endregion

        #region Editor Helpers

#if UNITY_EDITOR
        [TabGroup("Ambient Manager", "Debug Tools")]
        [Button("Test Environment Transition")]
        public void TestEnvironmentTransition(EnvironmentType testEnvironment)
        {
            UpdateForEnvironment(testEnvironment, currentWeather, currentTimeOfDay);
        }

        [TabGroup("Ambient Manager", "Debug Tools")]
        [Button("Test Weather Change")]
        public void TestWeatherChange(WeatherType testWeather)
        {
            UpdateForEnvironment(currentEnvironment, testWeather, currentTimeOfDay);
        }

        [TabGroup("Ambient Manager", "Debug Tools")]
        [Button("Stop All Ambient")]
        public void TestStopAll()
        {
            foreach (var source in ambientSources)
            {
                source.Stop();
            }
            activeLayers.Clear();
            activeAmbientSources = 0;
        }

        private void OnDrawGizmosSelected()
        {
            if (listenerTransform != null)
            {
                // 繝槭せ繧ｭ繝ｳ繧ｰ遽・・ｽ・ｽ縺ｮ陦ｨ遉ｺ
                Gizmos.color = new Color(0f, 1f, 0f, 0.2f);
                Gizmos.DrawSphere(listenerTransform.position, maskingRadius);
                
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(listenerTransform.position, maskingRadius);
            }
        }
#endif

        #endregion
    }

    #region Supporting Types

    /// <summary>
    /// 迺ｰ蠅・・ｽ・ｽ繝ｬ繧､繝､繝ｼ縺ｮ遞ｮ鬘・    /// </summary>
    public enum AmbientLayerType
    {
        Environment,
        Weather,
        TimeOfDay
    }

    /// <summary>
    /// 繧｢繧ｯ繝・・ｽ・ｽ繝悶↑迺ｰ蠅・・ｽ・ｽ繝ｬ繧､繝､繝ｼ
    /// </summary>
    [System.Serializable]
    public class AmbientLayer
    {
        public object collection;
        public AmbientLayerType layerType;
        public AudioSource audioSource;
        public float targetVolume;
        public float maskingStrength;
        public bool providesStealthMasking;
    }

    /// <summary>
    /// 迺ｰ蠅・・ｽ・ｽ繧ｳ繝ｬ繧ｯ繧ｷ繝ｧ繝ｳ
    /// </summary>
    [System.Serializable]
    public class AmbientSoundCollection
    {
        public EnvironmentType environmentType;
        public AudioClip[] ambientClips;
        [Range(0f, 1f)] public float baseVolume = 0.6f;
        [Range(0f, 1f)] public float maskingStrength = 0.3f;
        public bool providesStealthMasking = true;

        public AudioClip GetRandomClip()
        {
            return ambientClips != null && ambientClips.Length > 0 
                ? ambientClips[Random.Range(0, ambientClips.Length)] 
                : null;
        }
    }

    /// <summary>
    /// 螟ｩ蛟咏腸蠅・・ｽ・ｽ繧ｳ繝ｬ繧ｯ繧ｷ繝ｧ繝ｳ
    /// </summary>
    [System.Serializable]
    public class WeatherAmbientCollection
    {
        public WeatherType weatherType;
        public AudioClip weatherClip;
        [Range(0f, 1f)] public float intensity = 0.8f;
        [Range(0f, 1f)] public float maskingEffect = 0.5f;
    }

    /// <summary>
    /// 譎る俣蟶ｯ迺ｰ蠅・・ｽ・ｽ繧ｳ繝ｬ繧ｯ繧ｷ繝ｧ繝ｳ
    /// </summary>
    [System.Serializable]
    public class TimeAmbientCollection
    {
        public TimeOfDay timeOfDay;
        public AudioClip[] timeClips;
        [Range(0f, 1f)] public float baseVolume = 0.4f;

        public AudioClip GetRandomClip()
        {
            return timeClips != null && timeClips.Length > 0 
                ? timeClips[Random.Range(0, timeClips.Length)] 
                : null;
        }
    }

    #endregion
}

