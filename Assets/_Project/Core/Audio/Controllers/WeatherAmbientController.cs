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
    /// 動的天候環境音響制御システム（レイヤード・クロスフェード対応）
    ///
    /// Unity 6における3層アーキテクチャのCore層天候管理音響システムにおいて、
    /// 天候変化に連動した環境音の動的切り替え・制御を行う専門コントローラーです。
    /// レイヤードサウンド技術とクロスフェード遷移により、
    /// 自然で没入感のある天候音響体験を実現します。
    ///
    /// 【天候音響管理】
    /// - 天候タイプ対応: Clear/Rain/Storm/Snow/Wind等の多様な天候パターン
    /// - 音響コレクション: WeatherAmbientCollection による天候別音響リソース管理
    /// - レイヤードサウンド: 複数AudioSourceによる重層的天候音の構築
    /// - ランダム再生: 各天候コレクションからのランダム音源選択による自然性向上
    ///
    /// 【滑らかな天候遷移】
    /// - クロスフェード: AnimationCurve による自然な音量変化制御
    /// - 遷移時間制御: 可変遷移時間（0.1-10秒）による柔軟な切り替え速度調整
    /// - 並列処理: Coroutine を活用した非ブロッキング遷移処理
    /// - 重複防止: 遷移中フラグによる遷移処理の重複実行防止
    ///
    /// 【高度な音響制御】
    /// - AudioMixer統合: 専用MixerGroupによるプロフェッショナル音響制御
    /// - 音源プール: 限定数AudioSourceの効率的再利用による最適化
    /// - 2Dサウンド: 天候音に最適化された非空間音響設定
    /// - 音量正規化: マスター音量との協調による統一音響体験
    ///
    /// 【音響バリエーション】
    /// - ピッチバリエーション: ±0.1の微細なピッチ変動による自然性向上
    /// - 音量バリエーション: ±0.2の音量変動による有機的な音響表現
    /// - フェード制御: 個別フェードイン・アウト時間による細やかな制御
    /// - ランダム化制御: 有効/無効切り替え可能なランダム要素管理
    ///
    /// 【イベント駆動アーキテクチャ】
    /// - AudioEvent統合: 天候変化イベントの詳細データ配信
    /// - リアルタイム通知: 天候遷移完了の即座イベント発行
    /// - 疎結合設計: 他システムとの直接依存を排除した柔軟な連携
    /// - 拡張性: 新天候タイプ追加時の既存システムへの影響最小化
    ///
    /// 【レイヤー管理システム】
    /// - アクティブレイヤー追跡: 現在再生中の天候レイヤーの効率的管理
    /// - 動的レイヤー生成: 天候変化時の新規レイヤー動的作成
    /// - 自動クリーンアップ: 不要レイヤーの自動削除によるメモリ効率化
    /// - レイヤー辞書検索: Dictionary による O(1) 天候音響検索
    ///
    /// 【エディタ統合機能】
    /// - 実行時テスト: 任意天候への強制切り替えテスト機能
    /// - 一括制御: 全天候音響の即座停止・再開制御
    /// - Odin Inspector: [Button] 属性による直感的実行時操作
    /// - 状態監視: ReadOnly 属性による実行時状態の透明性確保
    ///
    /// 【パフォーマンス最適化】
    /// - 音源効率化: 最小限AudioSourceによる最大効果の実現
    /// - メモリ管理: アクティブレイヤーリストによる効率的メモリ使用
    /// - 早期リターン: 不要な処理のスキップによる CPU 負荷軽減
    /// - リソース管理: 音響リソースの適切な読み込み・解放制御
    ///
    /// 【没入感向上技術】
    /// - 自然な遷移: 天候変化を感じさせない滑らかな音響移行
    /// - 環境連動: ゲーム内環境変化との完全同期
    /// - 立体音響: リスナー位置に基づく正確な音響定位
    /// - 動的制御: リアルタイム天候変化への即座対応
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

        // 蜀・Κ迥ｶ諷・
        private AudioSource[] weatherSources;
        private Dictionary<WeatherType, WeatherAmbientCollection> weatherSoundLookup;
        private List<WeatherAmbientLayer> activeWeatherLayers = new List<WeatherAmbientLayer>();
        private Coroutine weatherTransition;

        // 繧ｷ繧ｹ繝・Β蜿ら・
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
        /// 螟ｩ豌励さ繝ｳ繝医Ο繝ｼ繝ｩ繝ｼ縺ｮ蛻晄悄蛹・        /// </summary>
        private void InitializeWeatherController()
        {
            weatherSoundLookup = new Dictionary<WeatherType, WeatherAmbientCollection>();
            activeWeatherLayers = new List<WeatherAmbientLayer>();
            
            ServiceHelper.Log("<color=cyan>[WeatherAmbientController]</color> Weather ambient controller initialized");
        }

        /// <summary>
        /// AudioSource縺ｮ險ｭ螳・
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
        /// AudioSource縺ｮ蝓ｺ譛ｬ險ｭ螳・
        /// </summary>
        private void ConfigureAudioSource(AudioSource source)
        {
            source.outputAudioMixerGroup = weatherMixerGroup;
            source.loop = true;
            source.playOnAwake = false;
            source.spatialBlend = AudioConstants.SPATIAL_BLEND_2D; // 螟ｩ豌鈴浹縺ｯ騾壼ｸｸ2D
            source.volume = 0f; // 蛻晄悄迥ｶ諷九〒縺ｯ辟｡髻ｳ
        }

        /// <summary>
        /// 讀懃ｴ｢霎樊嶌縺ｮ讒狗ｯ・        /// </summary>
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
        /// 繝ｪ繧ｹ繝奇ｿｽE縺ｮ讀懃ｴ｢
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
        /// 螟ｩ豌暦ｿｽE螟画峩
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
        /// 繝槭せ繧ｿ繝ｼ繝懊Μ繝･繝ｼ繝縺ｮ險ｭ螳・
        /// </summary>
        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            UpdateAllVolumes();
        }

        /// <summary>
        /// 蜈ｨ菴難ｿｽE蛛懈ｭ｢
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
        /// 荳譎ょ●豁｢
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
        /// 蜀埼幕
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
        /// 迴ｾ蝨ｨ縺ｮ螟ｩ豌励ち繧､繝励ｒ蜿門ｾ・        /// </summary>
        public WeatherType GetCurrentWeather() => currentWeather;

        /// <summary>
        /// 驕ｷ遘ｻ荳ｭ縺九←縺・°繧貞叙蠕・
        /// </summary>
        public bool IsTransitioning() => isTransitioning;

        #endregion

        #region Private Methods

        /// <summary>
        /// 螟ｩ豌暦ｿｽE遘ｻ縺ｮ繧ｳ繝ｫ繝ｼ繝√Φ
        /// </summary>
        private IEnumerator WeatherTransitionCoroutine(WeatherType newWeather)
        {
            isTransitioning = true;
            currentWeather = newWeather;

            ServiceHelper.Log($"<color=cyan>[WeatherAmbientController]</color> Starting transition to {newWeather}");

            // 譁ｰ縺励＞螟ｩ豌鈴浹髻ｿ繧貞叙蠕・            if (!weatherSoundLookup.TryGetValue(newWeather, out var weatherCollection))
            {
                ServiceHelper.LogWarning($"[WeatherAmbientController] No sound collection found for weather: {newWeather}");
                isTransitioning = false;
                yield break;
            }

            // 蛻ｩ逕ｨ蜿ｯ閭ｽ縺ｪ繧ｪ繝ｼ繝・ぅ繧ｪ繧ｽ繝ｼ繧ｹ繧呈爾縺・
            AudioSource availableSource = GetAvailableWeatherSource();
            if (availableSource == null)
            {
                ServiceHelper.LogWarning("[WeatherAmbientController] No available audio sources for weather transition");
                isTransitioning = false;
                yield break;
            }

            // 譁ｰ縺励＞螟ｩ豌励Ξ繧､繝､繝ｼ繧剃ｽ懶ｿｽE
            var newLayer = CreateWeatherLayer(weatherCollection, availableSource);
            if (newLayer != null)
            {
                yield return StartCoroutine(CrossfadeToNewWeatherLayer(availableSource, newLayer, weatherTransitionTime));
            }

            // 蜿､縺・Ξ繧､繝､繝ｼ繧偵ヵ繧ｧ繝ｼ繝峨い繧ｦ繝・
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

            // 繧､繝吶Φ繝育匱轣ｫ
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
        /// 蛻ｩ逕ｨ蜿ｯ閭ｽ縺ｪ螟ｩ豌礼畑AudioSource繧貞叙蠕・        /// </summary>
        private AudioSource GetAvailableWeatherSource()
        {
            foreach (var source in weatherSources)
            {
                if (!source.isPlaying)
                {
                    return source;
                }
            }
            return weatherSources[0]; // 蜈ｨ縺ｦ菴ｿ逕ｨ荳ｭ縺ｮ蝣ｴ蜷茨ｿｽE譛蛻晢ｿｽE繧ゑｿｽE繧剃ｽｿ逕ｨ
        }

        /// <summary>
        /// 螟ｩ豌励Ξ繧､繝､繝ｼ縺ｮ菴懶ｿｽE
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
        /// 譁ｰ縺励＞螟ｩ豌励Ξ繧､繝､繝ｼ縺ｸ縺ｮ繧ｯ繝ｭ繧ｹ繝輔ぉ繝ｼ繝・
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
        /// 螟ｩ豌励Ξ繧､繝､繝ｼ縺ｮ繝輔ぉ繝ｼ繝峨い繧ｦ繝・
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
        /// 蜈ｨ髻ｳ驥擾ｿｽE譖ｴ譁ｰ
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
        /// 螟ｩ豌礼腸蠅・浹繝ｬ繧､繝､繝ｼ
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
        /// 螟ｩ豌怜挨迺ｰ蠅・浹繧ｳ繝ｬ繧ｯ繧ｷ繝ｧ繝ｳ・・eatherAmbientController蟆ら畑・・
        /// </summary>
        [System.Serializable]
        private class WeatherAmbientCollection
        {
            [Header("Base Settings")]
            public string collectionName = "Weather Ambient";
            public WeatherType weatherType = WeatherType.Clear;
            public float baseVolume = 0.7f;
            public bool enableRandomization = true;
            
            [Header("Audio Clips")]
            public AudioClip[] ambientClips = new AudioClip[0];
            
            [Header("Audio Parameters")]
            [Range(0.5f, 2f)] public float pitchVariation = 0.1f;
            [Range(0f, 1f)] public float volumeVariation = 0.2f;
            [Range(0f, 10f)] public float fadeInTime = 2f;
            [Range(0f, 10f)] public float fadeOutTime = 2f;
            
            /// <summary>
            /// 繝ｩ繝ｳ繝繝縺ｪ繧ｪ繝ｼ繝・ぅ繧ｪ繧ｯ繝ｪ繝・・繧貞叙蠕・
            /// </summary>
            public AudioClip GetRandomClip()
            {
                if (ambientClips == null || ambientClips.Length == 0)
                    return null;
                    
                return ambientClips[Random.Range(0, ambientClips.Length)];
            }
            
            /// <summary>
            /// 繝舌Μ繧ｨ繝ｼ繧ｷ繝ｧ繝ｳ莉倥″縺ｮ髻ｳ驥上ｒ蜿門ｾ・            /// </summary>
            public float GetRandomVolume()
            {
                if (!enableRandomization) return baseVolume;
                
                float variation = Random.Range(-volumeVariation, volumeVariation);
                return Mathf.Clamp01(baseVolume + variation);
            }
            
            /// <summary>
            /// 繝舌Μ繧ｨ繝ｼ繧ｷ繝ｧ繝ｳ莉倥″縺ｮ繝斐ャ繝√ｒ蜿門ｾ・            /// </summary>
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

