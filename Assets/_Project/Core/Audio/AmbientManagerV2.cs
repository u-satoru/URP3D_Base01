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
    /// 環墁E��マネージャー V2
    /// 単一責任原則に基づき�E割されたコントローラーを統合管琁E    /// 旧AmbientManagerからのリファクタリング牁E    /// </summary>
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

        // 環墁E��専用管琁E        private AudioSource[] environmentSources;
        private System.Collections.Generic.Dictionary<EnvironmentType, AmbientSoundCollection> environmentSoundLookup;
        private System.Collections.Generic.List<EnvironmentAmbientLayer> activeEnvironmentLayers = new System.Collections.Generic.List<EnvironmentAmbientLayer>();
        private Coroutine environmentTransition;

        // シスチE��参�E
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
            // ✁EServiceLocator専用実裁E- AudioUpdateServiceを取征E            var audioUpdateService = GetAudioUpdateService();
            if (audioUpdateService != null && audioUpdateService.IsCoordinatedUpdateEnabled)
            {
                return; // 協調更新シスチE��が�E琁E��るためスキチE�E
            }
            
            // フォールバック�E�従来の更新処琁E            UpdateVolumeForStealthState();
        }

        #endregion

        #region Initialization

        /// <summary>
        /// AmbientManager V2の初期匁E        /// </summary>
        private void InitializeAmbientManagerV2()
        {
            environmentSoundLookup = new System.Collections.Generic.Dictionary<EnvironmentType, AmbientSoundCollection>();
            activeEnvironmentLayers = new System.Collections.Generic.List<EnvironmentAmbientLayer>();
            
            EventLogger.LogStatic("<color=cyan>[AmbientManagerV2]</color> Ambient Manager V2 initialized with controller separation");
        }

        /// <summary>
        /// 環墁E��用AudioSourceの設宁E        /// </summary>
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
        /// 環墁E��AudioSourceの基本設宁E        /// </summary>
        private void ConfigureEnvironmentAudioSource(AudioSource source)
        {
            source.outputAudioMixerGroup = environmentMixerGroup;
            source.loop = true;
            source.playOnAwake = false;
            source.spatialBlend = AudioConstants.SPATIAL_BLEND_2D; // 環墁E��は通常2D
            source.volume = 0f; // 初期状態では無音
            source.priority = AudioConstants.AMBIENT_AUDIO_PRIORITY;
        }

        /// <summary>
        /// シスチE��参�Eの検索
        /// </summary>
        private void FindSystemReferences()
        {
            // StealthAudioCoordinatorを検索
            stealthCoordinator = FindFirstObjectByType<StealthAudioCoordinator>();
            
            // AudioListenerを検索
            var audioListener = FindFirstObjectByType<AudioListener>();
            if (audioListener != null)
            {
                listenerTransform = audioListener.transform;
            }
        }

        /// <summary>
        /// 環墁E��検索辞書の構篁E        /// </summary>
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
        /// コントローラーの検証
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
        /// 初期環墁E��の開姁E        /// </summary>
        private void StartInitialAmbient()
        {
            // 現在の環墁E��応じて初期音響を開姁E            ChangeEnvironment(currentEnvironment);
            
            if (ambientSystemInitializedEvent != null)
            {
                ambientSystemInitializedEvent.Raise();
            }
        }

        #endregion

        #region Public Interface

        /// <summary>
        /// 環墁E�E変更
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
        /// 天気�E変更�E�EeatherControllerに委譲�E�E        /// </summary>
        public void ChangeWeather(WeatherType newWeather)
        {
            if (weatherController != null)
            {
                weatherController.ChangeWeather(newWeather);
            }
        }

        /// <summary>
        /// 時間帯の変更�E�EimeControllerに委譲�E�E        /// </summary>
        public void ChangeTimeOfDay(TimeOfDay newTime)
        {
            if (timeController != null)
            {
                timeController.ChangeTimeOfDay(newTime);
            }
        }

        /// <summary>
        /// スチE��ス状態�E更新
        /// </summary>
        public void UpdateForStealthState(bool isStealthActive)
        {
            isStealthModeActive = isStealthActive;
            
            // マスキングコントローラーに通知
            if (maskingController != null)
            {
                maskingController.SetDynamicMasking(isStealthActive);
            }
        }

        /// <summary>
        /// 環墁E��態�E一括更新
        /// </summary>
        public void UpdateForEnvironment(EnvironmentType env, WeatherType weather, TimeOfDay time)
        {
            ChangeEnvironment(env);
            ChangeWeather(weather);
            ChangeTimeOfDay(time);
        }

        /// <summary>
        /// マスターボリュームの設宁E        /// </summary>
        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            
            // 全コントローラーに音量設定を伝播
            if (weatherController != null)
                weatherController.SetMasterVolume(masterVolume);
                
            if (timeController != null)
                timeController.SetMasterVolume(masterVolume);
            
            UpdateAllEnvironmentVolumes();
        }

        /// <summary>
        /// 全体�E一時停止
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
        /// 全体�E再開
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
        /// 全音響シスチE��の停止
        /// </summary>
        public void StopAllAmbientSounds()
        {
            // 環墁E��の停止
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
            
            // 吁E��ントローラーの停止
            if (weatherController != null) weatherController.StopAllWeatherSounds();
            if (timeController != null) timeController.StopAllTimeSounds();
            if (maskingController != null) maskingController.StopAllMaskingEffects();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// 環墁E�E移のコルーチン
        /// </summary>
        private IEnumerator EnvironmentTransitionCoroutine(EnvironmentType newEnvironment)
        {
            currentEnvironment = newEnvironment;

            EventLogger.LogStatic($"<color=cyan>[AmbientManagerV2]</color> Starting environment transition to {newEnvironment}");

            // 新しい環墁E��響を取征E            if (!environmentSoundLookup.TryGetValue(newEnvironment, out var environmentCollection))
            {
                ServiceLocator.GetService<IEventLogger>().LogWarning($"[AmbientManagerV2] No sound collection found for environment: {newEnvironment}");
                yield break;
            }

            // 利用可能なオーチE��オソースを探ぁE            AudioSource availableSource = GetAvailableEnvironmentSource();
            if (availableSource == null)
            {
                ServiceLocator.GetService<IEventLogger>().LogWarning("[AmbientManagerV2] No available audio sources for environment transition");
                yield break;
            }

            // 新しい環墁E��イヤーを作�E
            var newLayer = CreateEnvironmentLayer(environmentCollection, availableSource);
            if (newLayer != null)
            {
                yield return StartCoroutine(CrossfadeToNewEnvironmentLayer(availableSource, newLayer, environmentTransitionTime));
            }

            // 古ぁE��イヤーをフェードアウチE            var layersToRemove = new System.Collections.Generic.List<EnvironmentAmbientLayer>(activeEnvironmentLayers);
            foreach (var layer in layersToRemove)
            {
                if (layer != newLayer)
                {
                    yield return StartCoroutine(FadeOutEnvironmentLayer(layer, environmentTransitionTime));
                }
            }

            EventLogger.LogStatic($"<color=cyan>[AmbientManagerV2]</color> Completed environment transition to {newEnvironment}");

            // イベント発火
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
        /// 利用可能な環墁E��AudioSourceを取征E        /// </summary>
        private AudioSource GetAvailableEnvironmentSource()
        {
            foreach (var source in environmentSources)
            {
                if (!source.isPlaying)
                {
                    return source;
                }
            }
            return environmentSources[0]; // 全て使用中の場合�E最初�Eも�Eを使用
        }

        /// <summary>
        /// 環墁E��レイヤーの作�E
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
        /// 新しい環墁E��イヤーへのクロスフェーチE        /// </summary>
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
        /// 環墁E��イヤーのフェードアウチE        /// </summary>
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
        /// スチE��ス状態に応じた音量調整
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
        /// 全環墁E��音量�E更新
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
        /// ServiceLocator優先でIAudioUpdateServiceを取征E        /// Phase 3移行パターンの実裁E        /// </summary>
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
            
            // フォールバック: FindFirstObjectByType (ServiceLocator専用実裁E
            if (asterivo.Unity60.Core.FeatureFlags.AllowSingletonFallback)
            {
                try
                {
                    // ✁EServiceLocator専用実裁E- 直接AudioUpdateCoordinatorを検索
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
        /// 環墁E��レイヤー
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