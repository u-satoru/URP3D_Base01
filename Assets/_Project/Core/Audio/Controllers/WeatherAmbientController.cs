using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;
using asterivo.Unity60.Core.Audio.Data;
using asterivo.Unity60.Core.Audio.Events;
using asterivo.Unity60.Core.Events;
// using asterivo.Unity60.Core.Debug;
// using asterivo.Unity60.Core.Shared;
using asterivo.Unity60.Core.Data;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Core.Audio.Controllers
{
    /// <summary>
    /// 天気に依存する環境音制御システム
    /// AmbientManagerから刁E��された天気音響専用コントローラー
    /// </summary>
    public class WeatherAmbientController : MonoBehaviour
    {
        [Header("Weather Sound Collections")]
        [SerializeField] private WeatherAmbientCollection[] weatherSounds;

        [Header("Audio Source Settings")]
        [SerializeField] private AudioMixerGroup weatherMixerGroup;
        [SerializeField, Range(1, 4)] private int weatherSourceCount = 2;

        [Header("Transition Settings")]
        [SerializeField, Range(0.1f, 10f)] private float weatherTransitionTime = AudioConstants.WEATHER_TRANSITION_TIME;
        [SerializeField] private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("Events")]
        [SerializeField] private AudioEvent weatherSoundTriggeredEvent;

        [Header("Runtime Info")]
        [SerializeField, ReadOnly] private WeatherType currentWeather = WeatherType.Clear;
        [SerializeField, ReadOnly] private bool isTransitioning = false;
        [SerializeField, ReadOnly] private float masterVolume = AudioConstants.DEFAULT_AMBIENT_VOLUME;

        // 内部状態
        private AudioSource[] weatherSources;
        private Dictionary<WeatherType, WeatherAmbientCollection> weatherSoundLookup;
        private List<WeatherAmbientLayer> activeWeatherLayers = new List<WeatherAmbientLayer>();
        private Coroutine weatherTransition;

        // システム参照
        private Transform listenerTransform;

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeWeatherController();
        }

        private void Start()
        {
            SetupAudioSources();
            BuildLookupDictionaries();
            FindListenerTransform();
        }

        #endregion

        #region Initialization

        /// <summary>
        /// 天気コントローラーの初期匁E        /// </summary>
        private void InitializeWeatherController()
        {
            weatherSoundLookup = new Dictionary<WeatherType, WeatherAmbientCollection>();
            activeWeatherLayers = new List<WeatherAmbientLayer>();
            
            ServiceHelper.Log("<color=cyan>[WeatherAmbientController]</color> Weather ambient controller initialized");
        }

        /// <summary>
        /// AudioSourceの設定
        /// </summary>
        private void SetupAudioSources()
        {
            weatherSources = new AudioSource[weatherSourceCount];
            
            for (int i = 0; i < weatherSourceCount; i++)
            {
                GameObject sourceObject = new GameObject($"WeatherSource_{i}");
                sourceObject.transform.SetParent(transform);
                
                weatherSources[i] = sourceObject.AddComponent<AudioSource>();
                ConfigureAudioSource(weatherSources[i]);
            }
        }

        /// <summary>
        /// AudioSourceの基本設定
        /// </summary>
        private void ConfigureAudioSource(AudioSource source)
        {
            source.outputAudioMixerGroup = weatherMixerGroup;
            source.loop = true;
            source.playOnAwake = false;
            source.spatialBlend = AudioConstants.SPATIAL_BLEND_2D; // 天気音は通常2D
            source.volume = 0f; // 初期状態では無音
        }

        /// <summary>
        /// 検索辞書の構篁E        /// </summary>
        private void BuildLookupDictionaries()
        {
            weatherSoundLookup.Clear();
            
            foreach (var collection in weatherSounds)
            {
                if (collection != null && !weatherSoundLookup.ContainsKey(collection.weatherType))
                {
                    weatherSoundLookup[collection.weatherType] = collection;
                }
            }
            
            ServiceHelper.Log($"<color=cyan>[WeatherAmbientController]</color> Built lookup for {weatherSoundLookup.Count} weather types");
        }

        /// <summary>
        /// リスナ�Eの検索
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
        /// 天気�E変更
        /// </summary>
        public void ChangeWeather(WeatherType newWeather)
        {
            if (newWeather == currentWeather && !isTransitioning)
                return;

            if (weatherTransition != null)
            {
                StopCoroutine(weatherTransition);
            }

            weatherTransition = StartCoroutine(WeatherTransitionCoroutine(newWeather));
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
        /// 全体�E停止
        /// </summary>
        public void StopAllWeatherSounds()
        {
            if (weatherTransition != null)
            {
                StopCoroutine(weatherTransition);
                weatherTransition = null;
            }

            foreach (var source in weatherSources)
            {
                if (source != null)
                {
                    source.Stop();
                }
            }

            activeWeatherLayers.Clear();
            isTransitioning = false;
        }

        /// <summary>
        /// 一時停止
        /// </summary>
        public void PauseAll()
        {
            foreach (var source in weatherSources)
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
            foreach (var source in weatherSources)
            {
                if (source != null)
                {
                    source.UnPause();
                }
            }
        }

        /// <summary>
        /// 現在の天気タイプを取征E        /// </summary>
        public WeatherType GetCurrentWeather() => currentWeather;

        /// <summary>
        /// 遷移中かどうかを取得
        /// </summary>
        public bool IsTransitioning() => isTransitioning;

        #endregion

        #region Private Methods

        /// <summary>
        /// 天気�E移のコルーチン
        /// </summary>
        private IEnumerator WeatherTransitionCoroutine(WeatherType newWeather)
        {
            isTransitioning = true;
            currentWeather = newWeather;

            ServiceHelper.Log($"<color=cyan>[WeatherAmbientController]</color> Starting transition to {newWeather}");

            // 新しい天気音響を取征E            if (!weatherSoundLookup.TryGetValue(newWeather, out var weatherCollection))
            {
                ServiceHelper.LogWarning($"[WeatherAmbientController] No sound collection found for weather: {newWeather}");
                isTransitioning = false;
                yield break;
            }

            // 利用可能なオーディオソースを探す
            AudioSource availableSource = GetAvailableWeatherSource();
            if (availableSource == null)
            {
                ServiceHelper.LogWarning("[WeatherAmbientController] No available audio sources for weather transition");
                isTransitioning = false;
                yield break;
            }

            // 新しい天気レイヤーを作�E
            var newLayer = CreateWeatherLayer(weatherCollection, availableSource);
            if (newLayer != null)
            {
                yield return StartCoroutine(CrossfadeToNewWeatherLayer(availableSource, newLayer, weatherTransitionTime));
            }

            // 古いレイヤーをフェードアウト
            var layersToRemove = new List<WeatherAmbientLayer>(activeWeatherLayers);
            foreach (var layer in layersToRemove)
            {
                if (layer != newLayer)
                {
                    yield return StartCoroutine(FadeOutWeatherLayer(layer, weatherTransitionTime));
                }
            }

            isTransitioning = false;
            ServiceHelper.Log($"<color=cyan>[WeatherAmbientController]</color> Completed transition to {newWeather}");

            // イベント発火
            if (weatherSoundTriggeredEvent != null)
            {
                var eventData = new AudioEventData
                {
                    audioClip = newLayer?.audioSource?.clip,
                    volume = masterVolume,
                    category = AudioCategory.Ambient,
                    worldPosition = listenerTransform?.position ?? Vector3.zero
                };
                weatherSoundTriggeredEvent.Raise(eventData);
            }
        }

        /// <summary>
        /// 利用可能な天気用AudioSourceを取征E        /// </summary>
        private AudioSource GetAvailableWeatherSource()
        {
            foreach (var source in weatherSources)
            {
                if (!source.isPlaying)
                {
                    return source;
                }
            }
            return weatherSources[0]; // 全て使用中の場合�E最初�Eも�Eを使用
        }

        /// <summary>
        /// 天気レイヤーの作�E
        /// </summary>
        private WeatherAmbientLayer CreateWeatherLayer(WeatherAmbientCollection collection, AudioSource audioSource)
        {
            var randomClip = collection.GetRandomClip();
            if (randomClip == null)
            {
                return null;
            }

            var layer = new WeatherAmbientLayer
            {
                weatherType = collection.weatherType,
                audioSource = audioSource,
                targetVolume = collection.baseVolume * masterVolume,
                fadeSpeed = 1f / weatherTransitionTime
            };

            audioSource.clip = randomClip;
            audioSource.volume = 0f;
            audioSource.Play();

            return layer;
        }

        /// <summary>
        /// 新しい天気レイヤーへのクロスフェード
        /// </summary>
        private IEnumerator CrossfadeToNewWeatherLayer(AudioSource source, WeatherAmbientLayer layer, float duration)
        {
            float elapsed = 0f;
            float startVolume = source.volume;

            activeWeatherLayers.Add(layer);

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
        /// 天気レイヤーのフェードアウト
        /// </summary>
        private IEnumerator FadeOutWeatherLayer(WeatherAmbientLayer layer, float duration)
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

            activeWeatherLayers.Remove(layer);
        }

        /// <summary>
        /// 全音量�E更新
        /// </summary>
        private void UpdateAllVolumes()
        {
            foreach (var layer in activeWeatherLayers)
            {
                if (layer?.audioSource != null)
                {
                    var collection = weatherSoundLookup.GetValueOrDefault(layer.weatherType);
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
        /// 天気環境音レイヤー
        /// </summary>
        [System.Serializable]
        private class WeatherAmbientLayer
        {
            public WeatherType weatherType;
            public AudioSource audioSource;
            public float targetVolume;
            public float fadeSpeed;
        }

        /// <summary>
        /// 天気別環境音コレクション（WeatherAmbientController専用）
        /// </summary>
        [System.Serializable]
        private class WeatherAmbientCollection
        {
            [Header("基本設定")]
            public string collectionName = "Weather Ambient";
            public WeatherType weatherType = WeatherType.Clear;
            public float baseVolume = 0.7f;
            public bool enableRandomization = true;
            
            [Header("オーディオクリップ")]
            public AudioClip[] ambientClips = new AudioClip[0];
            
            [Header("音響パラメータ")]
            [Range(0.5f, 2f)] public float pitchVariation = 0.1f;
            [Range(0f, 1f)] public float volumeVariation = 0.2f;
            [Range(0f, 10f)] public float fadeInTime = 2f;
            [Range(0f, 10f)] public float fadeOutTime = 2f;
            
            /// <summary>
            /// ランダムなオーディオクリップを取得
            /// </summary>
            public AudioClip GetRandomClip()
            {
                if (ambientClips == null || ambientClips.Length == 0)
                    return null;
                    
                return ambientClips[Random.Range(0, ambientClips.Length)];
            }
            
            /// <summary>
            /// バリエーション付きの音量を取征E            /// </summary>
            public float GetRandomVolume()
            {
                if (!enableRandomization) return baseVolume;
                
                float variation = Random.Range(-volumeVariation, volumeVariation);
                return Mathf.Clamp01(baseVolume + variation);
            }
            
            /// <summary>
            /// バリエーション付きのピッチを取征E            /// </summary>
            public float GetRandomPitch()
            {
                if (!enableRandomization) return 1f;
                
                return 1f + Random.Range(-pitchVariation, pitchVariation);
            }
        }

        #endregion

        #region Editor Helpers

        #if UNITY_EDITOR
        [Button("Test Weather Change")]
        public void TestWeatherChange(WeatherType testWeather)
        {
            if (Application.isPlaying)
            {
                ChangeWeather(testWeather);
            }
        }

        [Button("Stop All Weather")]
        public void EditorStopAll()
        {
            if (Application.isPlaying)
            {
                StopAllWeatherSounds();
            }
        }
        #endif

        #endregion
    }
}
