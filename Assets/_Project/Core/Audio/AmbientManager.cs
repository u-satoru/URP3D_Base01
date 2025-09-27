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
    /// 環境音の遷移を管理するクラスです。
    /// AmbientManagerは、環境、天候、時間帯に基づいて適切な環境音を再生し、ステルスモードやマスキング効果もサポートします。
    /// AmbientManagerは、IAudioServiceやISpatialAudioServiceなどのサービスをServiceLocatorから取得して使用します。
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

        // 環境音の遷移を管理するコルーチン
        private AudioSource[] ambientSources;
        private List<AmbientLayer> activeLayers = new List<AmbientLayer>();
        private Dictionary<EnvironmentType, AmbientSoundCollection> environmentSoundLookup;
        private Dictionary<WeatherType, WeatherAmbientCollection> weatherSoundLookup;
        private Dictionary<TimeOfDay, TimeAmbientCollection> timeSoundLookup;

        // ステルス状態の音響調整を担当するコーディネーター
        private StealthAudioCoordinator stealthCoordinator;
        private SpatialAudioManager spatialAudioManager;
        private Transform listenerTransform;

        // 音響の空間配置を管理するマネージャー
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
        /// AmbientManagerの初期化
        /// </summary>
        private void InitializeAmbientManager()
        {
            ambientSources = new AudioSource[ambientSourceCount];
        }

        /// <summary>
        /// AudioSourceのセットアップ
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
                source.spatialBlend = 0f; // ステレオ音声の設定
                source.outputAudioMixerGroup = ambientMixerGroup;
                source.priority = 128; // 音声の優先度

                ambientSources[i] = source;
            }
        }

        /// <summary>
        /// AudioSourceの設定とシステム参照の取得
        /// </summary>
        private void FindSystemReferences()
        {
            if (stealthCoordinator == null)
                stealthCoordinator = FindFirstObjectByType<StealthAudioCoordinator>();

            // SpatialAudioManagerの参照を取得
            if (spatialAudioManager == null)
            {
                spatialAudioManager = GetSpatialAudioManager();
            }

            // AudioListenerの参照を取得
            var audioListener = FindFirstObjectByType<AudioListener>();
            if (audioListener != null)
            {
                listenerTransform = audioListener.transform;
            }
        }

        /// <summary>
        /// ServiceLocatorからSpatialAudioManagerの参照を取得します。
        /// Phase 3以降はServiceLocatorを使用して取得します。
        /// </summary>
        private SpatialAudioManager GetSpatialAudioManager()
        {
            // ServiceLocatorを使用してSpatialAudioManagerを取得する
            if (asterivo.Unity60.Core.FeatureFlags.UseServiceLocator)
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
                    if (eventLogger != null)
                    {
                        eventLogger.LogError($"[AmbientManager] Failed to get SpatialAudioManager from ServiceLocator: {ex.Message}");
                    }
                }
            }

            // ServiceLocatorを使用してSpatialAudioManagerを取得する
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
                if (eventLogger != null)
                {
                    eventLogger.LogError("[AmbientManager] No SpatialAudioManager available and legacy singletons are disabled");
                }
            }

            return null;
        }

        /// <summary>
        /// ルックアップ辞書の構築
        /// </summary>
        private void BuildLookupDictionaries()
        {
            // 環境音のルックアップ辞書を構築
            environmentSoundLookup = new Dictionary<EnvironmentType, AmbientSoundCollection>();
            foreach (var collection in environmentSounds)
            {
                if (collection != null)
                {
                    environmentSoundLookup[collection.environmentType] = collection;
                }
            }

            // 天候音のルックアップ辞書を構築
            weatherSoundLookup = new Dictionary<WeatherType, WeatherAmbientCollection>();
            foreach (var collection in weatherSounds)
            {
                if (collection != null)
                {
                    weatherSoundLookup[collection.weatherType] = collection;
                }
            }

            // 時間帯音のルックアップ辞書を構築
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
        /// ステルス状態の音響調整を担当するコーディネーターの参照を取得
        /// </summary>
        private void StartInitialAmbient()
        {
            UpdateForEnvironment(currentEnvironment, currentWeather, currentTimeOfDay);
        }

        #endregion

        #region Public Interface

        /// <summary>
        /// 環境音の更新
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
        /// ステルス状態の音響調整を担当するコーディネーターの参照を取得
        /// </summary>
        public void UpdateForStealthState(bool stealthModeActive)
        {
            isStealthModeActive = stealthModeActive;

            // ステルス状態の音響調整を担当するコーディネーターの参照を取得
        }

        /// <summary>
        /// ステルス状態の音響調整を担当するコーディネーターの参照を取得
        /// </summary>
        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            UpdateAllSourceVolumes();
        }

        /// <summary>
        /// ステルス状態の音響調整を担当するコーディネーターの参照を取得
        /// </summary>
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
        /// ステルス状態の音響調整を担当するコーディネーターの参照を取得
        /// </summary>
        public void ResumeAll()
        {
            foreach (var source in ambientSources)
            {
                source.UnPause();
            }
        }

        /// <summary>
        /// ステルス状態の音響調整を担当するコーディネーターの参照を取得
        /// </summary>
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
        /// 環境音の遷移を管理するコルーチンを実行します。
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
        /// 天候音の遷移を管理するコルーチンを実行します。
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
        /// 時間帯音の遷移を管理するコルーチンを実行します。
        /// </summary>
        private IEnumerator EnvironmentTransitionCoroutine()
        {
            var targetCollection = environmentSoundLookup.GetValueOrDefault(currentEnvironment);

            if (targetCollection != null)
            {
                // 環境音のルックアップ辞書を構築
                var newLayer = new AmbientLayer
                {
                    collection = targetCollection,
                    layerType = AmbientLayerType.Environment,
                    targetVolume = targetCollection.baseVolume,
                    maskingStrength = targetCollection.maskingStrength,
                    providesStealthMasking = targetCollection.providesStealthMasking
                };

                // 利用可能なオーディオソースを取得
                var availableSource = GetAvailableAmbientSource();
                if (availableSource != null)
                {
                    yield return StartCoroutine(CrossfadeToNewLayer(availableSource, newLayer, environmentTransitionTime));
                }
            }

            environmentTransition = null;
        }

        /// <summary>
        /// 天候音の遷移を管理するコルーチンを実行します。
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
                    providesStealthMasking = true // ステルス状態の音響調整を担当するコーディネーターの参照を取得
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
        /// ステルス状態の音響調整を担当するコーディネーターの参照を取得
        /// </summary>
        private IEnumerator CrossfadeToNewLayer(AudioSource source, AmbientLayer newLayer, float transitionTime)
        {
            // ステルス状態の音響調整を担当するコーディネーターの参照を取得
            var existingLayer = activeLayers.Find(layer => layer.layerType == newLayer.layerType);
            if (existingLayer != null)
            {
                yield return StartCoroutine(FadeOutLayer(existingLayer, transitionTime));
                activeLayers.Remove(existingLayer);
            }

            // ステルス状態の音響調整を担当するコーディネーターの参照を取得
            AudioClip targetClip = GetClipFromLayer(newLayer);
            if (targetClip != null)
            {
                source.clip = targetClip;
                source.volume = 0f;
                source.Play();

                // 現在の時間を追跡
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

                // ステルス状態の音響調整を担当するコーディネーターの参照を取得
                if (newLayer.providesStealthMasking)
                {
                    ambientMaskingActivatedEvent?.Raise();
                }

                // 環境音の遷移を管理するコルーチンを実行します。
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
        /// ステルス状態の音響調整を担当するコーディネーターの参照を取得
        /// </summary>
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
        /// ステルス状態の音響調整を担当するコーディネーターの参照を取得
        /// </summary>
        private float CalculateLayerVolume(AmbientLayer layer)
        {
            float volume = layer.targetVolume * masterVolume;

            // ステルス状態の音響調整を担当するコーディネーターの参照を取得
            if (isStealthModeActive && stealthCoordinator != null)
            {
                volume *= stealthCoordinator.GetCategoryVolumeMultiplier(AudioCategory.Ambient);
            }

            return Mathf.Clamp01(volume);
        }

        /// <summary>
        /// ステルス状態の音響調整を担当するコーディネーターの参照を取得
        /// </summary>
        private void UpdateVolumeForStealthState()
        {
            foreach (var layer in activeLayers)
            {
                if (layer.audioSource != null && layer.audioSource.isPlaying)
                {
                    float targetVolume = CalculateLayerVolume(layer);

                    // 音量が大きく変化する場合のみ補間
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
        /// ステルス状態の音響調整を担当するコーディネーターの参照を取得
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
        /// ステルス状態の音響調整を担当するコーディネーターの参照を取得
        /// </summary>
        private void UpdateMaskingEffects()
        {
            // ステルス状態の音響調整を担当するコーディネーターの参照を取得
            // StealthAudioCoordinator のインスタンスを取得
            stealthCoordinator = FindObjectOfType<StealthAudioCoordinator>();
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// 利用可能なAudioSourceを取得します。
        /// 利用可能なソースがない場合は、最も古くから使用されているソースを返します。
        /// </summary>
        private AudioSource GetAvailableAmbientSource()
        {
            // 利用可能なソースを検索
            foreach (var source in ambientSources)
            {
                if (!source.isPlaying)
                {
                    return source;
                }
            }

            // 利用可能なソースがない場合は、最も古くから使用されているソースを返します。
            return ambientSources[0];
        }

        /// <summary>
        /// 利用可能なAudioClipを取得します。
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
        /// ステルス状態の音響調整を担当するコーディネーターの参照を取得
        /// </summary>
        private void UpdateTimeBasedAmbient()
        {
            var timeCollection = timeSoundLookup.GetValueOrDefault(currentTimeOfDay);
            if (timeCollection != null)
            {
                // ステルス状態の音響調整を担当するコーディネーターの参照を取得
                var timeLayer = new AmbientLayer
                {
                    collection = timeCollection,
                    layerType = AmbientLayerType.TimeOfDay,
                    targetVolume = timeCollection.baseVolume * 0.5f, // ステルス状態の音響調整を担当するコーディネーターの参照を取得
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
                // ステルス状態の音響調整を担当するコーディネーターの参照を取得
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
    /// ステルス状態の音響調整を担当するコーディネーターの参照を取得
    /// </summary>
    public enum AmbientLayerType
    {
        Environment,
        Weather,
        TimeOfDay
    }

    /// <summary>
    /// ステルス状態の音響調整を担当するコーディネーターの参照を取得
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
    /// ステルス状態の音響調整を担当するコーディネーターの参照を取得
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
    /// ステルス状態の音響調整を担当するコーディネーターの参照を取得
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
    /// ステルス状態の音響調整を担当するコーディネーターの参照を取得
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

