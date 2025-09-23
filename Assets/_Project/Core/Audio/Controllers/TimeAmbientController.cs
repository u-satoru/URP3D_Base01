using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;
using asterivo.Unity60.Core.Audio.Data;
using asterivo.Unity60.Core.Audio.Events;
using asterivo.Unity60.Core.Events;
// using asterivo.Unity60.Core.Debug;
using asterivo.Unity60.Core;
// using asterivo.Unity60.Core.Shared;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Core.Audio.Controllers
{
    /// <summary>
    /// 譎る俣蟶ｯ縺ｫ萓晏ｭ倥☆繧狗腸蠅・浹蛻ｶ蠕｡繧ｷ繧ｹ繝・Β
    /// AmbientManager縺九ｉ蛻・屬縺輔ｌ縺滓凾髢馴浹髻ｿ蟆ら畑繧ｳ繝ｳ繝医Ο繝ｼ繝ｩ繝ｼ
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
        [SerializeField, ShowIf("enableAutomaticTimeUpdates")] private float timeUpdateInterval = 60f; // 1蛻・俣髫斐〒繝√ぉ繝・け

        [Header("Transition Settings")]
        [SerializeField, Range(0.1f, 30f)] private float timeTransitionDuration = 15f; // 譎る俣驕ｷ遘ｻ縺ｯ髟ｷ繧√↓險ｭ螳・        [SerializeField] private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("Events")]
        [SerializeField] private AudioEvent timeSoundTriggeredEvent;

        [Header("Runtime Info")]
        [SerializeField, ReadOnly] private TimeOfDay currentTimeOfDay = TimeOfDay.Day;
        [SerializeField, ReadOnly] private bool isTransitioning = false;
        [SerializeField, ReadOnly] private float masterVolume = AudioConstants.DEFAULT_AMBIENT_VOLUME;
        [SerializeField, ReadOnly] private float nextUpdateTime;

        // 蜀・Κ迥ｶ諷・        private AudioSource[] timeSources;
        private Dictionary<TimeOfDay, TimeAmbientCollection> timeSoundLookup;
        private List<TimeAmbientLayer> activeTimeLayers = new List<TimeAmbientLayer>();
        private Coroutine timeTransition;

        // 繧ｷ繧ｹ繝・Β蜿ら・
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
                // 笨・ServiceLocator蟆ら畑螳溯｣・- IAudioUpdateService繧貞叙蠕・                if (asterivo.Unity60.Core.FeatureFlags.UseServiceLocator)
                {
                    try
                    {
                        var audioUpdateService = asterivo.Unity60.Core.ServiceLocator.GetService<asterivo.Unity60.Core.Audio.Interfaces.IAudioUpdateService>();
                        if (audioUpdateService is AudioUpdateCoordinator coordinator && coordinator.enabled)
                        {
                            coordinator.OnAudioSystemSync += OnAudioSystemSync;
                            EventLogger.LogStatic("<color=cyan>[TimeAmbientController]</color> Registered with AudioUpdateCoordinator via ServiceLocator");
                            return; // 逋ｻ骭ｲ謌仙粥縺ｮ縺溘ａ邨ゆｺ・                        }
                    }
                    catch (System.Exception ex)
                    {
                        ServiceLocator.GetService<IEventLogger>().LogError($"[TimeAmbientController] Failed to get IAudioUpdateService from ServiceLocator: {ex.Message}");
                    }
                }
                
                // 繝輔か繝ｼ繝ｫ繝舌ャ繧ｯ: 逶ｴ謗･讀懃ｴ｢
                var fallbackCoordinator = FindFirstObjectByType<AudioUpdateCoordinator>();
                if (fallbackCoordinator != null && fallbackCoordinator.enabled)
                {
                    fallbackCoordinator.OnAudioSystemSync += OnAudioSystemSync;
                    EventLogger.LogStatic("<color=cyan>[TimeAmbientController]</color> Registered with AudioUpdateCoordinator via fallback");
                }
                else
                {
                    // 繝輔か繝ｼ繝ｫ繝舌ャ繧ｯ・壼ｾ捺擂縺ｮInvokeRepeating
                    InvokeRepeating(nameof(CheckTimeOfDay), 0f, timeUpdateInterval);
                }
            }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// 譎る俣繧ｳ繝ｳ繝医Ο繝ｼ繝ｩ繝ｼ縺ｮ蛻晄悄蛹・        /// </summary>
        private void InitializeTimeController()
        {
            timeSoundLookup = new Dictionary<TimeOfDay, TimeAmbientCollection>();
            activeTimeLayers = new List<TimeAmbientLayer>();
            
            EventLogger.LogStatic("<color=cyan>[TimeAmbientController]</color> Time ambient controller initialized");
        }

        /// <summary>
        /// AudioSource縺ｮ險ｭ螳・        /// </summary>
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
        /// AudioSource縺ｮ蝓ｺ譛ｬ險ｭ螳・        /// </summary>
        private void ConfigureAudioSource(AudioSource source)
        {
            source.outputAudioMixerGroup = timeMixerGroup;
            source.loop = true;
            source.playOnAwake = false;
            source.spatialBlend = AudioConstants.SPATIAL_BLEND_2D; // 譎る俣髻ｳ縺ｯ騾壼ｸｸ2D
            source.volume = 0f; // 蛻晄悄迥ｶ諷九〒縺ｯ辟｡髻ｳ
        }

        /// <summary>
        /// 讀懃ｴ｢霎樊嶌縺ｮ讒狗ｯ・        /// </summary>
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
            
            EventLogger.LogStatic($"<color=cyan>[TimeAmbientController]</color> Built lookup for {timeSoundLookup.Count} time periods");
        }

        /// <summary>
        /// 繝ｪ繧ｹ繝翫・縺ｮ讀懃ｴ｢
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
        /// 譎る俣蟶ｯ縺ｮ螟画峩
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
        /// 繝槭せ繧ｿ繝ｼ繝懊Μ繝･繝ｼ繝縺ｮ險ｭ螳・        /// </summary>
        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            UpdateAllVolumes();
        }

        /// <summary>
        /// 閾ｪ蜍墓凾髢捺峩譁ｰ縺ｮ譛牙柑/辟｡蜉ｹ
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
        /// 蜈ｨ菴薙・蛛懈ｭ｢
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
        /// 荳譎ょ●豁｢
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
        /// 蜀埼幕
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
        /// 迴ｾ蝨ｨ縺ｮ譎る俣蟶ｯ繧貞叙蠕・        /// </summary>
        public TimeOfDay GetCurrentTimeOfDay() => currentTimeOfDay;

        /// <summary>
        /// 驕ｷ遘ｻ荳ｭ縺九←縺・°繧貞叙蠕・        /// </summary>
        public bool IsTransitioning() => isTransitioning;

        #endregion

        #region Private Methods

        /// <summary>
        /// 繧ｷ繧ｹ繝・Β譎ょ綾縺ｫ蝓ｺ縺･縺乗凾髢灘ｸｯ繝√ぉ繝・け
        /// </summary>
        private void CheckTimeOfDay()
        {
            var systemTime = System.DateTime.Now;
            TimeOfDay detectedTime = DetermineTimeOfDayFromSystemTime(systemTime);
            
            if (detectedTime != currentTimeOfDay)
            {
                EventLogger.LogStatic($"<color=cyan>[TimeAmbientController]</color> Automatic time change detected: {currentTimeOfDay} -> {detectedTime}");
                ChangeTimeOfDay(detectedTime);
            }
            
            nextUpdateTime = Time.time + timeUpdateInterval;
        }

        /// <summary>
        /// 繧ｷ繧ｹ繝・Β譎ょ綾縺九ｉ譎る俣蟶ｯ繧貞愛螳・        /// </summary>
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
        /// 譎る俣驕ｷ遘ｻ縺ｮ繧ｳ繝ｫ繝ｼ繝√Φ
        /// </summary>
        private IEnumerator TimeTransitionCoroutine(TimeOfDay newTime)
        {
            isTransitioning = true;
            currentTimeOfDay = newTime;

            EventLogger.LogStatic($"<color=cyan>[TimeAmbientController]</color> Starting transition to {newTime}");

            // 譁ｰ縺励＞譎る俣髻ｳ髻ｿ繧貞叙蠕・            if (!timeSoundLookup.TryGetValue(newTime, out var timeCollection))
            {
                EventLogger.LogWarningStatic($"[TimeAmbientController] No sound collection found for time: {newTime}");
                isTransitioning = false;
                yield break;
            }

            // 蛻ｩ逕ｨ蜿ｯ閭ｽ縺ｪ繧ｪ繝ｼ繝・ぅ繧ｪ繧ｽ繝ｼ繧ｹ繧呈爾縺・            AudioSource availableSource = GetAvailableTimeSource();
            if (availableSource == null)
            {
                EventLogger.LogWarningStatic("[TimeAmbientController] No available audio sources for time transition");
                isTransitioning = false;
                yield break;
            }

            // 譁ｰ縺励＞譎る俣繝ｬ繧､繝､繝ｼ繧剃ｽ懈・
            var newLayer = CreateTimeLayer(timeCollection, availableSource);
            if (newLayer != null)
            {
                yield return StartCoroutine(CrossfadeToNewTimeLayer(availableSource, newLayer, timeTransitionDuration));
            }

            // 蜿､縺・Ξ繧､繝､繝ｼ繧偵ヵ繧ｧ繝ｼ繝峨い繧ｦ繝・            var layersToRemove = new List<TimeAmbientLayer>(activeTimeLayers);
            foreach (var layer in layersToRemove)
            {
                if (layer != newLayer)
                {
                    yield return StartCoroutine(FadeOutTimeLayer(layer, timeTransitionDuration));
                }
            }

            isTransitioning = false;
            EventLogger.LogStatic($"<color=cyan>[TimeAmbientController]</color> Completed transition to {newTime}");

            // 繧､繝吶Φ繝育匱轣ｫ
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
        /// 蛻ｩ逕ｨ蜿ｯ閭ｽ縺ｪ譎る俣逕ｨAudioSource繧貞叙蠕・        /// </summary>
        private AudioSource GetAvailableTimeSource()
        {
            foreach (var source in timeSources)
            {
                if (!source.isPlaying)
                {
                    return source;
                }
            }
            return timeSources[0]; // 蜈ｨ縺ｦ菴ｿ逕ｨ荳ｭ縺ｮ蝣ｴ蜷医・譛蛻昴・繧ゅ・繧剃ｽｿ逕ｨ
        }

        /// <summary>
        /// 譎る俣繝ｬ繧､繝､繝ｼ縺ｮ菴懈・
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
        /// 譁ｰ縺励＞譎る俣繝ｬ繧､繝､繝ｼ縺ｸ縺ｮ繧ｯ繝ｭ繧ｹ繝輔ぉ繝ｼ繝・        /// </summary>
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
        /// 譎る俣繝ｬ繧､繝､繝ｼ縺ｮ繝輔ぉ繝ｼ繝峨い繧ｦ繝・        /// </summary>
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
        /// 蜈ｨ髻ｳ驥上・譖ｴ譁ｰ
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
        /// 譎る俣迺ｰ蠅・浹繝ｬ繧､繝､繝ｼ
        /// </summary>
        [System.Serializable]
        private class TimeAmbientLayer
        {
            public TimeOfDay timeOfDay;
            public AudioSource audioSource;
            public float targetVolume;
            public float fadeSpeed;
        }

        /// <summary>
        /// AudioUpdateCoordinator縺九ｉ縺ｮ蜷梧悄繧ｳ繝ｼ繝ｫ繝舌ャ繧ｯ
        /// </summary>
        private void OnAudioSystemSync(AudioSystemSyncData syncData)
        {
            // 譎る俣螟画峩縺梧､懷・縺輔ｌ縺溷ｴ蜷医・縺ｿ蜃ｦ逅・            if (syncData.timeChanged && syncData.currentTimeOfDay != currentTimeOfDay)
            {
                EventLogger.LogStatic($"<color=cyan>[TimeAmbientController]</color> Time change detected via coordinator: {currentTimeOfDay} -> {syncData.currentTimeOfDay}");
                ChangeTimeOfDay(syncData.currentTimeOfDay);
            }
            
            // 繧ｹ繝・Ν繧ｹ迥ｶ諷九↓蠢懊§縺滄浹驥剰ｪｿ謨ｴ
            if (syncData.stealthStateChanged)
            {
                float volumeMultiplier = syncData.isStealthActive ? 0.6f : 1f;
                SetMasterVolume(syncData.ambientVolume * volumeMultiplier);
            }
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