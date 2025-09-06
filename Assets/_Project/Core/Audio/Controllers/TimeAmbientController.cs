using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;
using asterivo.Unity60.Core.Audio.Data;
using asterivo.Unity60.Core.Audio.Events;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Debug;
using asterivo.Unity60.Core.Shared;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Core.Audio.Controllers
{
    /// <summary>
    /// 時間帯に依存する環境音制御システム
    /// AmbientManagerから分離された時間音響専用コントローラー
    /// </summary>
    public class TimeAmbientController : MonoBehaviour
    {
        [Header("Time Sound Collections")]
        [SerializeField] private TimeAmbientCollection[] timeSounds;

        [Header("Audio Source Settings")]
        [SerializeField] private AudioMixerGroup timeMixerGroup;
        [SerializeField, Range(1, 4)] private int timeSourceCount = 2;

        [Header("Automatic Time Updates")]
        [SerializeField] private bool enableAutomaticTimeUpdates = true;
        [SerializeField, ShowIf("enableAutomaticTimeUpdates")] private float timeUpdateInterval = 60f; // 1分間隔でチェック

        [Header("Transition Settings")]
        [SerializeField, Range(0.1f, 30f)] private float timeTransitionDuration = 15f; // 時間遷移は長めに設定
        [SerializeField] private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("Events")]
        [SerializeField] private AudioEvent timeSoundTriggeredEvent;

        [Header("Runtime Info")]
        [SerializeField, ReadOnly] private TimeOfDay currentTimeOfDay = TimeOfDay.Day;
        [SerializeField, ReadOnly] private bool isTransitioning = false;
        [SerializeField, ReadOnly] private float masterVolume = AudioConstants.DEFAULT_AMBIENT_VOLUME;
        [SerializeField, ReadOnly] private float nextUpdateTime;

        // 内部状態
        private AudioSource[] timeSources;
        private Dictionary<TimeOfDay, TimeAmbientCollection> timeSoundLookup;
        private List<TimeAmbientLayer> activeTimeLayers = new List<TimeAmbientLayer>();
        private Coroutine timeTransition;

        // システム参照
        private Transform listenerTransform;

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeTimeController();
        }

        private void Start()
        {
            SetupAudioSources();
            BuildLookupDictionaries();
            FindListenerTransform();
            
            if (enableAutomaticTimeUpdates)
            {
                InvokeRepeating(nameof(CheckTimeOfDay), 0f, timeUpdateInterval);
            }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// 時間コントローラーの初期化
        /// </summary>
        private void InitializeTimeController()
        {
            timeSoundLookup = new Dictionary<TimeOfDay, TimeAmbientCollection>();
            activeTimeLayers = new List<TimeAmbientLayer>();
            
            EventLogger.Log("<color=cyan>[TimeAmbientController]</color> Time ambient controller initialized");
        }

        /// <summary>
        /// AudioSourceの設定
        /// </summary>
        private void SetupAudioSources()
        {
            timeSources = new AudioSource[timeSourceCount];
            
            for (int i = 0; i < timeSourceCount; i++)
            {
                GameObject sourceObject = new GameObject($"TimeSource_{i}");
                sourceObject.transform.SetParent(transform);
                
                timeSources[i] = sourceObject.AddComponent<AudioSource>();
                ConfigureAudioSource(timeSources[i]);
            }
        }

        /// <summary>
        /// AudioSourceの基本設定
        /// </summary>
        private void ConfigureAudioSource(AudioSource source)
        {
            source.outputAudioMixerGroup = timeMixerGroup;
            source.loop = true;
            source.playOnAwake = false;
            source.spatialBlend = AudioConstants.SPATIAL_BLEND_2D; // 時間音は通常2D
            source.volume = 0f; // 初期状態では無音
        }

        /// <summary>
        /// 検索辞書の構築
        /// </summary>
        private void BuildLookupDictionaries()
        {
            timeSoundLookup.Clear();
            
            foreach (var collection in timeSounds)
            {
                if (collection != null && !timeSoundLookup.ContainsKey(collection.timeOfDay))
                {
                    timeSoundLookup[collection.timeOfDay] = collection;
                }
            }
            
            EventLogger.Log($"<color=cyan>[TimeAmbientController]</color> Built lookup for {timeSoundLookup.Count} time periods");
        }

        /// <summary>
        /// リスナーの検索
        /// </summary>
        private void FindListenerTransform()
        {
            var audioListener = FindFirstObjectByType<AudioListener>();
            if (audioListener != null)
            {
                listenerTransform = audioListener.transform;
            }
        }

        #endregion

        #region Public Interface

        /// <summary>
        /// 時間帯の変更
        /// </summary>
        public void ChangeTimeOfDay(TimeOfDay newTime)
        {
            if (newTime == currentTimeOfDay && !isTransitioning)
                return;

            if (timeTransition != null)
            {
                StopCoroutine(timeTransition);
            }

            timeTransition = StartCoroutine(TimeTransitionCoroutine(newTime));
        }

        /// <summary>
        /// マスターボリュームの設定
        /// </summary>
        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            UpdateAllVolumes();
        }

        /// <summary>
        /// 自動時間更新の有効/無効
        /// </summary>
        public void SetAutomaticTimeUpdates(bool enabled)
        {
            enableAutomaticTimeUpdates = enabled;
            
            if (enabled)
            {
                InvokeRepeating(nameof(CheckTimeOfDay), 0f, timeUpdateInterval);
            }
            else
            {
                CancelInvoke(nameof(CheckTimeOfDay));
            }
        }

        /// <summary>
        /// 全体の停止
        /// </summary>
        public void StopAllTimeSounds()
        {
            if (timeTransition != null)
            {
                StopCoroutine(timeTransition);
                timeTransition = null;
            }

            foreach (var source in timeSources)
            {
                if (source != null)
                {
                    source.Stop();
                }
            }

            activeTimeLayers.Clear();
            isTransitioning = false;
        }

        /// <summary>
        /// 一時停止
        /// </summary>
        public void PauseAll()
        {
            foreach (var source in timeSources)
            {
                if (source != null && source.isPlaying)
                {
                    source.Pause();
                }
            }
        }

        /// <summary>
        /// 再開
        /// </summary>
        public void ResumeAll()
        {
            foreach (var source in timeSources)
            {
                if (source != null)
                {
                    source.UnPause();
                }
            }
        }

        /// <summary>
        /// 現在の時間帯を取得
        /// </summary>
        public TimeOfDay GetCurrentTimeOfDay() => currentTimeOfDay;

        /// <summary>
        /// 遷移中かどうかを取得
        /// </summary>
        public bool IsTransitioning() => isTransitioning;

        #endregion

        #region Private Methods

        /// <summary>
        /// システム時刻に基づく時間帯チェック
        /// </summary>
        private void CheckTimeOfDay()
        {
            var systemTime = System.DateTime.Now;
            TimeOfDay detectedTime = DetermineTimeOfDayFromSystemTime(systemTime);
            
            if (detectedTime != currentTimeOfDay)
            {
                EventLogger.Log($"<color=cyan>[TimeAmbientController]</color> Automatic time change detected: {currentTimeOfDay} -> {detectedTime}");
                ChangeTimeOfDay(detectedTime);
            }
            
            nextUpdateTime = Time.time + timeUpdateInterval;
        }

        /// <summary>
        /// システム時刻から時間帯を判定
        /// </summary>
        private TimeOfDay DetermineTimeOfDayFromSystemTime(System.DateTime time)
        {
            int hour = time.Hour;
            
            if (hour >= 6 && hour < 12)
                return TimeOfDay.Day; // Morning is mapped to Day
            else if (hour >= 12 && hour < 18)
                return TimeOfDay.Day;
            else if (hour >= 18 && hour < 22)
                return TimeOfDay.Evening;
            else
                return TimeOfDay.Night;
        }

        /// <summary>
        /// 時間遷移のコルーチン
        /// </summary>
        private IEnumerator TimeTransitionCoroutine(TimeOfDay newTime)
        {
            isTransitioning = true;
            currentTimeOfDay = newTime;

            EventLogger.Log($"<color=cyan>[TimeAmbientController]</color> Starting transition to {newTime}");

            // 新しい時間音響を取得
            if (!timeSoundLookup.TryGetValue(newTime, out var timeCollection))
            {
                EventLogger.LogWarning($"[TimeAmbientController] No sound collection found for time: {newTime}");
                isTransitioning = false;
                yield break;
            }

            // 利用可能なオーディオソースを探す
            AudioSource availableSource = GetAvailableTimeSource();
            if (availableSource == null)
            {
                EventLogger.LogWarning("[TimeAmbientController] No available audio sources for time transition");
                isTransitioning = false;
                yield break;
            }

            // 新しい時間レイヤーを作成
            var newLayer = CreateTimeLayer(timeCollection, availableSource);
            if (newLayer != null)
            {
                yield return StartCoroutine(CrossfadeToNewTimeLayer(availableSource, newLayer, timeTransitionDuration));
            }

            // 古いレイヤーをフェードアウト
            var layersToRemove = new List<TimeAmbientLayer>(activeTimeLayers);
            foreach (var layer in layersToRemove)
            {
                if (layer != newLayer)
                {
                    yield return StartCoroutine(FadeOutTimeLayer(layer, timeTransitionDuration));
                }
            }

            isTransitioning = false;
            EventLogger.Log($"<color=cyan>[TimeAmbientController]</color> Completed transition to {newTime}");

            // イベント発火
            if (timeSoundTriggeredEvent != null)
            {
                var eventData = new AudioEventData
                {
                    audioClip = newLayer?.audioSource?.clip,
                    volume = masterVolume,
                    category = AudioCategory.Ambient,
                    worldPosition = listenerTransform?.position ?? Vector3.zero
                };
                timeSoundTriggeredEvent.Raise(eventData);
            }
        }

        /// <summary>
        /// 利用可能な時間用AudioSourceを取得
        /// </summary>
        private AudioSource GetAvailableTimeSource()
        {
            foreach (var source in timeSources)
            {
                if (!source.isPlaying)
                {
                    return source;
                }
            }
            return timeSources[0]; // 全て使用中の場合は最初のものを使用
        }

        /// <summary>
        /// 時間レイヤーの作成
        /// </summary>
        private TimeAmbientLayer CreateTimeLayer(TimeAmbientCollection collection, AudioSource audioSource)
        {
            var randomClip = collection.GetRandomClip();
            if (randomClip == null)
            {
                return null;
            }

            var layer = new TimeAmbientLayer
            {
                timeOfDay = collection.timeOfDay,
                audioSource = audioSource,
                targetVolume = collection.baseVolume * masterVolume,
                fadeSpeed = 1f / timeTransitionDuration
            };

            audioSource.clip = randomClip;
            audioSource.volume = 0f;
            audioSource.Play();

            return layer;
        }

        /// <summary>
        /// 新しい時間レイヤーへのクロスフェード
        /// </summary>
        private IEnumerator CrossfadeToNewTimeLayer(AudioSource source, TimeAmbientLayer layer, float duration)
        {
            float elapsed = 0f;
            float startVolume = source.volume;

            activeTimeLayers.Add(layer);

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
        /// 時間レイヤーのフェードアウト
        /// </summary>
        private IEnumerator FadeOutTimeLayer(TimeAmbientLayer layer, float duration)
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

            activeTimeLayers.Remove(layer);
        }

        /// <summary>
        /// 全音量の更新
        /// </summary>
        private void UpdateAllVolumes()
        {
            foreach (var layer in activeTimeLayers)
            {
                if (layer?.audioSource != null)
                {
                    var collection = timeSoundLookup.GetValueOrDefault(layer.timeOfDay);
                    if (collection != null)
                    {
                        layer.targetVolume = collection.baseVolume * masterVolume;
                        layer.audioSource.volume = layer.targetVolume;
                    }
                }
            }
        }

        #endregion

        #region Helper Classes

        /// <summary>
        /// 時間環境音レイヤー
        /// </summary>
        [System.Serializable]
        private class TimeAmbientLayer
        {
            public TimeOfDay timeOfDay;
            public AudioSource audioSource;
            public float targetVolume;
            public float fadeSpeed;
        }

        #endregion

        #region Editor Helpers

        #if UNITY_EDITOR
        [Button("Test Time Change")]
        public void TestTimeChange(TimeOfDay testTime)
        {
            if (Application.isPlaying)
            {
                ChangeTimeOfDay(testTime);
            }
        }

        [Button("Stop All Time Sounds")]
        public void EditorStopAll()
        {
            if (Application.isPlaying)
            {
                StopAllTimeSounds();
            }
        }

        [Button("Force Time Check")]
        public void ForceTimeCheck()
        {
            if (Application.isPlaying)
            {
                CheckTimeOfDay();
            }
        }
        #endif

        #endregion
    }
}