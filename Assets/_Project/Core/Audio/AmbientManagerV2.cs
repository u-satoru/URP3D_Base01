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
using asterivo.Unity60.Core.Data;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Core.Audio
{
    /// <summary>
    /// AmbientManager V2
    /// 進化した環境音管理システムで、より高度な音響体験を提供します。
    /// </summary>
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

        private AudioSource[] environmentSources;
        private System.Collections.Generic.Dictionary<EnvironmentType, AmbientSoundCollection> environmentSoundLookup;
        private System.Collections.Generic.List<EnvironmentAmbientLayer> activeEnvironmentLayers = new System.Collections.Generic.List<EnvironmentAmbientLayer>();
        private Coroutine environmentTransition;

        /// <summary>
        /// ステルス状態の音響調整を担当するコーディネーター
        /// </summary>
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
            // ServiceLocator実装 - AudioUpdateServiceを取得
            var audioUpdateService = GetAudioUpdateService();
            if (audioUpdateService != null && audioUpdateService.IsCoordinatedUpdateEnabled)
            {
                return; // 音声更新がコーディネートされている場合は処理をスキップ
            }

            // ステルス状態の音響調整を行う
            UpdateVolumeForStealthState();
        }

        #endregion

        #region Initialization

        /// <summary>
        /// AmbientManager V2の初期化
        /// </summary>
        private void InitializeAmbientManagerV2()
        {
            environmentSoundLookup = new System.Collections.Generic.Dictionary<EnvironmentType, AmbientSoundCollection>();
            activeEnvironmentLayers = new System.Collections.Generic.List<EnvironmentAmbientLayer>();

            ServiceHelper.Log("<color=cyan>[AmbientManagerV2]</color> Ambient Manager V2 initialized with controller separation");
        }

        /// <summary>
        /// AudioSourceのセットアップ
        /// </summary>
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

            ServiceHelper.Log($"<color=cyan>[AmbientManagerV2]</color> Created {environmentSourceCount} environment audio sources");
        }

        /// <summary>
        /// AudioSourceの設定
        /// </summary>
        private void ConfigureEnvironmentAudioSource(AudioSource source)
        {
            source.outputAudioMixerGroup = environmentMixerGroup;
            source.loop = true;
            source.playOnAwake = false;
            source.spatialBlend = AudioConstants.SPATIAL_BLEND_2D; // ステレオ音声の設定
            source.volume = 0f; // 初期音量は0に設定
            source.clip = null;
            source.priority = AudioConstants.AMBIENT_AUDIO_PRIORITY;
        }

        /// <summary>
        /// ステルス状態の音響調整を担当するコーディネーターの参照を取得
        /// </summary>
        private void FindSystemReferences()
        {
            // StealthAudioCoordinatorの参照を取得
            stealthCoordinator = FindFirstObjectByType<StealthAudioCoordinator>();

            // AudioListenerの参照を取得
            var audioListener = FindFirstObjectByType<AudioListener>();
            if (audioListener != null)
            {
                listenerTransform = audioListener.transform;
            }
        }

        /// <summary>
        /// 環境音のルックアップ辞書を構築
        /// </summary>
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

            ServiceHelper.Log($"<color=cyan>[AmbientManagerV2]</color> Built lookup for {environmentSoundLookup.Count} environment types");
        }

        /// <summary>
        /// ステレオ音声の設定
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
                ServiceHelper.Log("<color=green>[AmbientManagerV2]</color> All controllers validated successfully");
            }
        }

        /// <summary>
        /// 初期環境音の再生開始
        /// </summary>
        private void StartInitialAmbient()
        {
            // 初期環境音の再生を開始
            ChangeEnvironment(currentEnvironment);

            if (ambientSystemInitializedEvent != null)
            {
                ambientSystemInitializedEvent.Raise();
            }
        }

        #endregion

        #region Public Interface

        /// <summary>
        /// 環境音の変更
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
        /// 天候の変更
        /// </summary>
        public void ChangeWeather(WeatherType newWeather)
        {
            if (weatherController != null)
            {
                weatherController.ChangeWeather(newWeather);
            }
        }

        /// <summary>
        /// 時間帯の変更
        /// </summary>
        public void ChangeTimeOfDay(TimeOfDay newTime)
        {
            if (timeController != null)
            {
                timeController.ChangeTimeOfDay(newTime);
            }
        }

        /// <summary>
        /// ステルス状態の音響調整を担当するコーディネーターの参照を取得
        /// </summary>
        public void UpdateForStealthState(bool isStealthActive)
        {
            isStealthModeActive = isStealthActive;

            // ステルス状態の音響調整を担当するコーディネーターの参照を取得
            if (maskingController != null)
            {
                maskingController.SetDynamicMasking(isStealthActive);
            }
        }

        /// <summary>
        /// ステルス状態の音響調整を担当するコーディネーターの参照を取得
        /// </summary>
        public void UpdateForEnvironment(EnvironmentType env, WeatherType weather, TimeOfDay time)
        {
            ChangeEnvironment(env);
            ChangeWeather(weather);
            ChangeTimeOfDay(time);
        }

        /// <summary>
        /// ステルス状態の音響調整を担当するコーディネーターの参照を取得
        /// </summary>
        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);

            // ステルス状態の音響調整を担当するコーディネーターの参照を取得
            if (weatherController != null)
                weatherController.SetMasterVolume(masterVolume);

            if (timeController != null)
                timeController.SetMasterVolume(masterVolume);

            UpdateAllEnvironmentVolumes();
        }

        /// <summary>
        /// 全ての環境音を一時停止
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
        /// 全ての環境音を再開
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
        /// 全ての環境音を停止
        /// </summary>
        public void StopAllAmbientSounds()
        {
            // 環境音の停止
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

            // ステルス状態の音響調整を担当するコーディネーターの参照を取得
            if (weatherController != null) weatherController.StopAllWeatherSounds();
            if (timeController != null) timeController.StopAllTimeSounds();
            if (maskingController != null) maskingController.StopAllMaskingEffects();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// 環境音の遷移を管理するコルーチン
        /// </summary>
        private IEnumerator EnvironmentTransitionCoroutine(EnvironmentType newEnvironment)
        {
            currentEnvironment = newEnvironment;

            ServiceHelper.Log($"<color=cyan>[AmbientManagerV2]</color> Starting environment transition to {newEnvironment}");

            // 環境音のコレクションを取得
            if (!environmentSoundLookup.TryGetValue(newEnvironment, out var environmentCollection))
            {
                ServiceLocator.GetService<IEventLogger>().LogWarning($"[AmbientManagerV2] No sound collection found for environment: {newEnvironment}");
                yield break;
            }

            // 利用可能なAudioSourceを取得
            AudioSource availableSource = GetAvailableEnvironmentSource();
            if (availableSource == null)
            {
                ServiceLocator.GetService<IEventLogger>().LogWarning("[AmbientManagerV2] No available audio sources for environment transition");
                yield break;
            }

            // 新しい環境音のレイヤーを作成
            var newLayer = CreateEnvironmentLayer(environmentCollection, availableSource);
            if (newLayer != null)
            {
                yield return StartCoroutine(CrossfadeToNewEnvironmentLayer(availableSource, newLayer, environmentTransitionTime));
            }

            // 既存の環境音レイヤーを取得
            var layersToRemove = new System.Collections.Generic.List<EnvironmentAmbientLayer>(activeEnvironmentLayers);
            foreach (var layer in layersToRemove)
            {
                if (layer != newLayer)
                {
                    yield return StartCoroutine(FadeOutEnvironmentLayer(layer, environmentTransitionTime));
                }
            }

            ServiceHelper.Log($"<color=cyan>[AmbientManagerV2]</color> Completed environment transition to {newEnvironment}");

            // 環境音トリガーイベントの発火
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
        /// 環境音の遷移を管理するコルーチン
        /// </summary>
        private AudioSource GetAvailableEnvironmentSource()
        {
            foreach (var source in environmentSources)
            {
                if (!source.isPlaying)
                {
                    return source;
                }
            }
            return environmentSources[0]; // 利用可能なAudioSourceがない場合は最初のソースを返す
        }

        /// <summary>
        /// 環境音の遷移を管理するコルーチン
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
        /// 環境音の遷移を管理するコルーチン
        /// </summary>
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
        /// 環境音の遷移を管理するコルーチン
        /// </summary>
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
        /// ステルス状態における音量の更新
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
        /// 環境音の遷移を管理するコルーチン
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
        /// ServiceLocatorからIAudioUpdateServiceを取得
        /// Phase 3以降はServiceLocatorを使用する
        /// </summary>
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

            // シングルトンフォールバックを許可: FindFirstObjectByType (ServiceLocatorから取得できない場合)
            if (asterivo.Unity60.Core.FeatureFlags.AllowSingletonFallback)
            {
                try
                {
                    // ServiceLocatorから取得できない場合はFindFirstObjectByTypeを使用
                    var coordinator = FindFirstObjectByType<AudioUpdateCoordinator>();
                    if (coordinator != null && asterivo.Unity60.Core.FeatureFlags.EnableDebugLogging)
                    {
                        ServiceHelper.Log("[AmbientManagerV2] Found AudioUpdateCoordinator via FindFirstObjectByType");
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
        /// 環境音の遷移を管理するコルーチン
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
