using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;
using asterivo.Unity60.Core.Audio.Data;
using asterivo.Unity60.Core.Audio.Events;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Debug;
using asterivo.Unity60.Core.Audio.Interfaces;
using _Project.Core;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Core.Audio
{
    /// <summary>
    /// 環境音マネージャー
    /// ステルスゲームに特化したマスキング効果付き環境音システム
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

        // オーディオソース管理
        private AudioSource[] ambientSources;
        private List<AmbientLayer> activeLayers = new List<AmbientLayer>();
        private Dictionary<EnvironmentType, AmbientSoundCollection> environmentSoundLookup;
        private Dictionary<WeatherType, WeatherAmbientCollection> weatherSoundLookup;
        private Dictionary<TimeOfDay, TimeAmbientCollection> timeSoundLookup;

        // システム連携
        private StealthAudioCoordinator stealthCoordinator;
        private SpatialAudioManager spatialAudioManager;
        private Transform listenerTransform;

        // 遷移制御
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
        /// 環境音マネージャーの初期化
        /// </summary>
        private void InitializeAmbientManager()
        {
            ambientSources = new AudioSource[ambientSourceCount];
        }

        /// <summary>
        /// オーディオソースのセットアップ
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
                source.spatialBlend = 0f; // 環境音は通常2D
                source.outputAudioMixerGroup = ambientMixerGroup;
                source.priority = 128; // 低優先度

                ambientSources[i] = source;
            }
        }

        /// <summary>
        /// システム参照の検索
        /// </summary>
        /// <summary>
        /// システム参照の検索
        /// Phase 3移行パターン: ServiceLocator優先、Singletonフォールバック
        /// </summary>
        private void FindSystemReferences()
        {
            if (stealthCoordinator == null)
                stealthCoordinator = FindFirstObjectByType<StealthAudioCoordinator>();

            // SpatialAudioManager取得: ServiceLocator優先、Singletonフォールバック
            if (spatialAudioManager == null)
            {
                spatialAudioManager = GetSpatialAudioManager();
            }

            // リスナーの検索
            var audioListener = FindFirstObjectByType<AudioListener>();
            if (audioListener != null)
            {
                listenerTransform = audioListener.transform;
            }
        }
        
        /// <summary>
        /// ServiceLocator優先でSpatialAudioManagerを取得
        /// Phase 3移行パターンの実装
        /// </summary>
        private SpatialAudioManager GetSpatialAudioManager()
        {
            // ServiceLocator経由での取得を試みる
            if (FeatureFlags.UseServiceLocator)
            {
                try
                {
                    var spatialService = ServiceLocator.GetService<ISpatialAudioService>();
                    if (spatialService is SpatialAudioManager manager)
                    {
                        if (FeatureFlags.EnableDebugLogging)
                        {
                            EventLogger.Log("[AmbientManager] Successfully retrieved SpatialAudioManager from ServiceLocator");
                        }
                        return manager;
                    }
                }
                catch (System.Exception ex)
                {
                    EventLogger.LogError($"[AmbientManager] Failed to get SpatialAudioManager from ServiceLocator: {ex.Message}");
                }
            }
            
            // ✅ ServiceLocator専用実装 - 直接SpatialAudioManagerを検索
            var spatialAudioManager = FindFirstObjectByType<SpatialAudioManager>();
            if (spatialAudioManager != null)
            {
                if (FeatureFlags.EnableDebugLogging)
                {
                    EventLogger.Log("[AmbientManager] Found SpatialAudioManager via FindFirstObjectByType");
                }
                return spatialAudioManager;
            }
            else
            {
                EventLogger.LogError("[AmbientManager] No SpatialAudioManager available and legacy singletons are disabled");
            }
            
            return null;
        }

        /// <summary>
        /// ルックアップ辞書の構築
        /// </summary>
        private void BuildLookupDictionaries()
        {
            // 環境音辞書
            environmentSoundLookup = new Dictionary<EnvironmentType, AmbientSoundCollection>();
            foreach (var collection in environmentSounds)
            {
                if (collection != null)
                {
                    environmentSoundLookup[collection.environmentType] = collection;
                }
            }

            // 天候音辞書
            weatherSoundLookup = new Dictionary<WeatherType, WeatherAmbientCollection>();
            foreach (var collection in weatherSounds)
            {
                if (collection != null)
                {
                    weatherSoundLookup[collection.weatherType] = collection;
                }
            }

            // 時間帯音辞書
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
        /// 初期環境音の開始
        /// </summary>
        private void StartInitialAmbient()
        {
            UpdateForEnvironment(currentEnvironment, currentWeather, currentTimeOfDay);
        }

        #endregion

        #region Public Interface

        /// <summary>
        /// 環境に応じた環境音更新
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

            EventLogger.Log($"<color=green>[AmbientManager]</color> Environment updated - Env: {environment}, Weather: {weather}, Time: {timeOfDay}");
        }

        /// <summary>
        /// ステルス状態に応じた更新
        /// </summary>
        public void UpdateForStealthState(bool stealthModeActive)
        {
            isStealthModeActive = stealthModeActive;
            
            // ステルス状態に応じた音量調整は UpdateVolumeForStealthState で処理
        }

        /// <summary>
        /// マスター音量の設定
        /// </summary>
        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            UpdateAllSourceVolumes();
        }

        /// <summary>
        /// 全環境音の一時停止
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
        /// 全環境音の再開
        /// </summary>
        public void ResumeAll()
        {
            foreach (var source in ambientSources)
            {
                source.UnPause();
            }
        }

        /// <summary>
        /// 特定位置での環境音マスキング強度を取得
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
        /// 環境音の遷移
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
        /// 天候音の遷移
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
        /// 環境遷移のコルーチン
        /// </summary>
        private IEnumerator EnvironmentTransitionCoroutine()
        {
            var targetCollection = environmentSoundLookup.GetValueOrDefault(currentEnvironment);
            
            if (targetCollection != null)
            {
                // 新しい環境レイヤーを準備
                var newLayer = new AmbientLayer
                {
                    collection = targetCollection,
                    layerType = AmbientLayerType.Environment,
                    targetVolume = targetCollection.baseVolume,
                    maskingStrength = targetCollection.maskingStrength,
                    providesStealthMasking = targetCollection.providesStealthMasking
                };

                // 利用可能なソースを検索
                var availableSource = GetAvailableAmbientSource();
                if (availableSource != null)
                {
                    yield return StartCoroutine(CrossfadeToNewLayer(availableSource, newLayer, environmentTransitionTime));
                }
            }

            environmentTransition = null;
        }

        /// <summary>
        /// 天候遷移のコルーチン
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
                    providesStealthMasking = true // 天候は通常マスキング効果あり
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
        /// 新しいレイヤーへのクロスフェード
        /// </summary>
        private IEnumerator CrossfadeToNewLayer(AudioSource source, AmbientLayer newLayer, float transitionTime)
        {
            // 古いレイヤーのフェードアウト（同じタイプがあれば）
            var existingLayer = activeLayers.Find(layer => layer.layerType == newLayer.layerType);
            if (existingLayer != null)
            {
                yield return StartCoroutine(FadeOutLayer(existingLayer, transitionTime));
                activeLayers.Remove(existingLayer);
            }

            // 新しいレイヤーの設定
            AudioClip targetClip = GetClipFromLayer(newLayer);
            if (targetClip != null)
            {
                source.clip = targetClip;
                source.volume = 0f;
                source.Play();

                // フェードイン
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

                // イベント発行
                if (newLayer.providesStealthMasking)
                {
                    ambientMaskingActivatedEvent?.Raise();
                }

                // ステルス音響イベントの発行
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
        /// レイヤーのフェードアウト
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
        /// レイヤーの音量計算
        /// </summary>
        private float CalculateLayerVolume(AmbientLayer layer)
        {
            float volume = layer.targetVolume * masterVolume;

            // ステルスモード時の調整
            if (isStealthModeActive && stealthCoordinator != null)
            {
                volume *= stealthCoordinator.GetCategoryVolumeMultiplier(AudioCategory.Ambient);
            }

            return Mathf.Clamp01(volume);
        }

        /// <summary>
        /// ステルス状態に応じた音量更新
        /// </summary>
        private void UpdateVolumeForStealthState()
        {
            foreach (var layer in activeLayers)
            {
                if (layer.audioSource != null && layer.audioSource.isPlaying)
                {
                    float targetVolume = CalculateLayerVolume(layer);
                    
                    // スムーズな音量変化
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
        /// 全ソースの音量更新
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
        /// マスキング効果の更新
        /// </summary>
        private void UpdateMaskingEffects()
        {
            // マスキング効果の動的計算と適用
            // 実装は StealthAudioCoordinator との連携で行う
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// 利用可能な環境音ソースを取得
        /// </summary>
        private AudioSource GetAvailableAmbientSource()
        {
            // 使用中でないソースを検索
            foreach (var source in ambientSources)
            {
                if (!source.isPlaying)
                {
                    return source;
                }
            }

            // 全て使用中の場合は最も古いものを置き換え
            return ambientSources[0];
        }

        /// <summary>
        /// レイヤーからクリップを取得
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
        /// 時間帯ベースの環境音更新
        /// </summary>
        private void UpdateTimeBasedAmbient()
        {
            var timeCollection = timeSoundLookup.GetValueOrDefault(currentTimeOfDay);
            if (timeCollection != null)
            {
                // 時間帯の環境音は低優先度で背景に追加
                var timeLayer = new AmbientLayer
                {
                    collection = timeCollection,
                    layerType = AmbientLayerType.TimeOfDay,
                    targetVolume = timeCollection.baseVolume * 0.5f, // 時間帯音は控えめに
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
                // マスキング範囲の表示
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
    /// 環境音レイヤーの種類
    /// </summary>
    public enum AmbientLayerType
    {
        Environment,
        Weather,
        TimeOfDay
    }

    /// <summary>
    /// アクティブな環境音レイヤー
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
    /// 環境音コレクション
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
    /// 天候環境音コレクション
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
    /// 時間帯環境音コレクション
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