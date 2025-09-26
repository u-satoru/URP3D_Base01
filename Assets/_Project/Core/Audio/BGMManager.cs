using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;
using asterivo.Unity60.Core.Audio.Data;
using asterivo.Unity60.Core.Audio.Events;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Data;
// using asterivo.Unity60.Core.Debug;
// using asterivo.Unity60.Core.Services; // Removed to avoid circular dependency
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Core.Audio
{
    /// <summary>
    /// スチE��スゲーム特化�EBGMマネージャー
    /// 緊張度とゲーム状況に応じた動的BGM制御
    /// </summary>
    public class BGMManager : MonoBehaviour
    {
        [TabGroup("BGM Manager", "Track Categories")]
        [Header("BGM Track Categories")]
        [SerializeField, Required] private BGMTrack[] menuBGM;
        [SerializeField, Required] private BGMTrack[] ambientBGM;        // 平常晁E        [SerializeField, Required] private BGMTrack[] tensionBGM;        // 警戒時
        [SerializeField, Required] private BGMTrack[] combatBGM;         // 戦闘時
        [SerializeField] private BGMTrack[] stealthSuccessBGM;           // スチE��ス成功晁E        [SerializeField] private BGMTrack[] explorationBGM;              // 探索晁E
        [TabGroup("BGM Manager", "Dynamic Control")]
        [Header("Dynamic Control Settings")]
        [SerializeField, Range(0.5f, 5f)] private float crossfadeDuration = 2f;
        [SerializeField] private AnimationCurve tensionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField, Range(0.1f, 1f)] private float stealthVolumeReduction = 0.3f;
        [SerializeField, Range(0f, 2f)] private float environmentTransitionSpeed = 1f;

        [TabGroup("BGM Manager", "Audio Sources")]
        [Header("Audio Source Configuration")]
        [SerializeField] private AudioSource primaryBGMSource;
        [SerializeField] private AudioSource crossfadeBGMSource;
        [SerializeField] private AudioMixerGroup bgmMixerGroup;

        [TabGroup("BGM Manager", "Events")]
        [Header("Event Integration")]
        [SerializeField] private AudioEvent bgmChangeEvent;
        [SerializeField] private GameEvent bgmTrackStartedEvent;
        [SerializeField] private GameEvent bgmTrackEndedEvent;

        [TabGroup("BGM Manager", "Runtime State")]
        [Header("Runtime State Information")]
        [SerializeField, ReadOnly] private BGMTrack currentTrack;
        [SerializeField, ReadOnly] private BGMCategory currentCategory = BGMCategory.Ambient;
        [SerializeField, ReadOnly, Range(0f, 1f)] private float currentTensionLevel = 0f;
        [SerializeField, ReadOnly] private bool isStealthModeActive = false;
        [SerializeField, ReadOnly] private bool isTransitioning = false;
        [SerializeField, ReadOnly] private float masterVolume = 1f;

        // 環墁E��応シスチE��
        private EnvironmentType currentEnvironment = EnvironmentType.Outdoor;
        private WeatherType currentWeather = WeatherType.Clear;
        private TimeOfDay currentTimeOfDay = TimeOfDay.Day;

        // シスチE��連携
        private StealthAudioCoordinator stealthCoordinator;
        private DynamicAudioEnvironment dynamicEnvironment;

        // 冁E��状慁E        private Dictionary<BGMCategory, BGMTrack[]> bgmCategories;
        private Queue<BGMPlaybackRequest> pendingRequests = new Queue<BGMPlaybackRequest>();
        private Coroutine activeTransition;

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeBGMManager();
        }

        private void Start()
        {
            SetupAudioSources();
            FindSystemReferences();
            StartDefaultBGM();
        }

        private void Update()
        {
            ProcessPendingRequests();
            UpdateVolumeForStealthState();
        }

        #endregion

        #region Initialization

        /// <summary>
        /// BGMマネージャーの初期化
        /// </summary>
        private void InitializeBGMManager()
        {
            // BGMカテゴリ辞書の構築
            bgmCategories = new Dictionary<BGMCategory, BGMTrack[]>
            {
                { BGMCategory.Menu, menuBGM },
                { BGMCategory.Ambient, ambientBGM },
                { BGMCategory.Tension, tensionBGM },
                { BGMCategory.Combat, combatBGM },
                { BGMCategory.StealthSuccess, stealthSuccessBGM },
                { BGMCategory.Exploration, explorationBGM }
            };
        }

        /// <summary>
        /// オーチE��オソースのセチE��アチE�E
        /// </summary>
        private void SetupAudioSources()
        {
            // プライマリBGMソースの設宁E            if (primaryBGMSource == null)
            {
                var primaryGO = new GameObject("PrimaryBGMSource");
                primaryGO.transform.SetParent(transform);
                primaryBGMSource = primaryGO.AddComponent<AudioSource>();
            }

            ConfigureAudioSource(primaryBGMSource);

            // クロスフェード用BGMソースの設宁E            if (crossfadeBGMSource == null)
            {
                var crossfadeGO = new GameObject("CrossfadeBGMSource");
                crossfadeGO.transform.SetParent(transform);
                crossfadeBGMSource = crossfadeGO.AddComponent<AudioSource>();
            }

            ConfigureAudioSource(crossfadeBGMSource);
        }

        /// <summary>
        /// オーチE��オソースの共通設宁E        /// </summary>
        private void ConfigureAudioSource(AudioSource source)
        {
            source.playOnAwake = false;
            source.spatialBlend = 0f; // 2D音響
            source.loop = true;
            source.outputAudioMixerGroup = bgmMixerGroup;
            source.priority = 64; // 中程度の優先度
        }

        /// <summary>
        /// シスチE��参�Eの検索
        /// </summary>
        private void FindSystemReferences()
        {
            if (stealthCoordinator == null)
                stealthCoordinator = FindFirstObjectByType<StealthAudioCoordinator>();

            if (dynamicEnvironment == null)
                dynamicEnvironment = FindFirstObjectByType<DynamicAudioEnvironment>();
        }

        /// <summary>
        /// チE��ォルチEGMの開姁E        /// </summary>
        private void StartDefaultBGM()
        {
            // ゲーム開始時のBGM選抁E            var startingTrack = SelectBGMByCategory(BGMCategory.Ambient);
            if (startingTrack != null)
            {
                PlayBGMImmediately(startingTrack);
            }
        }

        #endregion

        #region Public Interface

        /// <summary>
        /// 緊張度に応じたBGM更新
        /// </summary>
        public void UpdateForTensionLevel(float tensionLevel, bool stealthModeActive = false)
        {
            currentTensionLevel = tensionLevel;
            isStealthModeActive = stealthModeActive;

            BGMCategory targetCategory = DetermineBGMCategoryByTension(tensionLevel);
            BGMTrack targetTrack = SelectBGMByCategory(targetCategory);

            if (targetTrack != null && targetTrack != currentTrack)
            {
                if (targetTrack.allowInStealthMode || !stealthModeActive)
                {
                    CrossfadeToBGM(targetTrack);
                }
            }
        }

        /// <summary>
        /// 環墁E��応じたBGM更新
        /// </summary>
        public void UpdateForEnvironment(EnvironmentType environment, WeatherType weather, TimeOfDay timeOfDay)
        {
            currentEnvironment = environment;
            currentWeather = weather;
            currentTimeOfDay = timeOfDay;

            // 環墁E��化に応じたBGM調整
            if (currentTrack != null)
            {
                ApplyEnvironmentalModifications();
            }
        }

        /// <summary>
        /// 特定カテゴリのBGMを再生
        /// </summary>
        public void PlayBGMCategory(BGMCategory category, bool forceImmediate = false)
        {
            var targetTrack = SelectBGMByCategory(category);
            if (targetTrack != null)
            {
                if (forceImmediate || isTransitioning)
                {
                    PlayBGMImmediately(targetTrack);
                }
                else
                {
                    CrossfadeToBGM(targetTrack);
                }
            }
        }

        /// <summary>
        /// BGMの停止
        /// </summary>
        public void StopBGM(float fadeOutTime = 0f)
        {
            if (activeTransition != null)
            {
                StopCoroutine(activeTransition);
            }

            if (fadeOutTime > 0f)
            {
                activeTransition = StartCoroutine(FadeOutCoroutine(fadeOutTime));
            }
            else
            {
                primaryBGMSource.Stop();
                crossfadeBGMSource.Stop();
                currentTrack = null;
            }
        }

        /// <summary>
        /// マスター音量�E設宁E        /// </summary>
        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            UpdateSourceVolumes();
        }

        /// <summary>
        /// BGM一時停止
        /// </summary>
        public void PauseAll()
        {
            primaryBGMSource.Pause();
            crossfadeBGMSource.Pause();
        }

        /// <summary>
        /// BGM再開
        /// </summary>
        public void ResumeAll()
        {
            primaryBGMSource.UnPause();
            crossfadeBGMSource.UnPause();
        }

        #endregion

        #region BGM Selection and Playback

        /// <summary>
        /// 緊張度からBGMカテゴリを決定
        /// </summary>
        private BGMCategory DetermineBGMCategoryByTension(float tension)
        {
            if (tension < 0.2f)
                return BGMCategory.Ambient;
            else if (tension < 0.5f)
                return BGMCategory.Exploration;
            else if (tension < 0.8f)
                return BGMCategory.Tension;
            else
                return BGMCategory.Combat;
        }

        /// <summary>
        /// カテゴリに応じたBGM選択
        /// </summary>
        private BGMTrack SelectBGMByCategory(BGMCategory category)
        {
            if (!bgmCategories.TryGetValue(category, out BGMTrack[] tracks) || tracks.Length == 0)
            {
                ServiceHelper.LogWarning($"[BGMManager] No tracks found for category: {category}");
                return null;
            }

            // 環墁E�E天候�E時間帯に適したトラチE��を優先選抁E            var suitableTracks = FilterTracksByContext(tracks);
            
            if (suitableTracks.Count == 0)
                suitableTracks.AddRange(tracks);

            return suitableTracks[Random.Range(0, suitableTracks.Count)];
        }

        /// <summary>
        /// 現在のコンチE��ストに適したトラチE��をフィルタリング
        /// </summary>
        private List<BGMTrack> FilterTracksByContext(BGMTrack[] tracks)
        {
            var suitableTracks = new List<BGMTrack>();

            foreach (var track in tracks)
            {
                bool isEnvironmentSuitable = track.suitableEnvironments.Length == 0 || 
                    System.Array.Exists(track.suitableEnvironments, env => env == currentEnvironment);

                bool isWeatherSuitable = track.suitableWeather.Length == 0 || 
                    System.Array.Exists(track.suitableWeather, weather => weather == currentWeather);

                bool isTimeSuitable = track.suitableTimeOfDay.Length == 0 || 
                    System.Array.Exists(track.suitableTimeOfDay, time => time == currentTimeOfDay);

                if (isEnvironmentSuitable && isWeatherSuitable && isTimeSuitable)
                {
                    suitableTracks.Add(track);
                }
            }

            return suitableTracks;
        }

        /// <summary>
        /// BGMの即座再生
        /// </summary>
        private void PlayBGMImmediately(BGMTrack track)
        {
            if (track?.clip == null) return;

            // 既存�E生を停止
            primaryBGMSource.Stop();
            crossfadeBGMSource.Stop();

            // 新しいトラチE��を設定�E再生
            primaryBGMSource.clip = track.clip;
            primaryBGMSource.volume = CalculateTargetVolume(track);
            primaryBGMSource.Play();

            currentTrack = track;
            currentCategory = track.category;
            isTransitioning = false;

            // イベント発衁E            bgmTrackStartedEvent?.Raise();
            bgmChangeEvent?.RaiseAtPosition(track.trackName, transform.position);

            ServiceHelper.Log($"<color=green>[BGMManager]</color> Playing BGM: {track.trackName} (Category: {track.category})");
        }

        /// <summary>
        /// スムーズなBGM刁E��替ぁE        /// </summary>
        private void CrossfadeToBGM(BGMTrack newTrack)
        {
            if (newTrack == null || newTrack == currentTrack) return;

            if (activeTransition != null)
            {
                StopCoroutine(activeTransition);
            }

            activeTransition = StartCoroutine(CrossfadeCoroutine(newTrack));
        }

        #endregion

        #region Coroutines

        /// <summary>
        /// クロスフェードコルーチン
        /// </summary>
        private IEnumerator CrossfadeCoroutine(BGMTrack newTrack)
        {
            isTransitioning = true;

            // クロスフェード用ソースに新しいトラチE��を設宁E            crossfadeBGMSource.clip = newTrack.clip;
            crossfadeBGMSource.volume = 0f;
            crossfadeBGMSource.Play();

            float currentTime = 0f;
            float primaryStartVolume = primaryBGMSource.volume;
            float crossfadeTargetVolume = CalculateTargetVolume(newTrack);

            // クロスフェード実衁E            while (currentTime < crossfadeDuration)
            {
                currentTime += Time.deltaTime;
                float t = currentTime / crossfadeDuration;

                // スムーズな遷移カーブを適用
                float smoothT = tensionCurve.Evaluate(t);

                primaryBGMSource.volume = Mathf.Lerp(primaryStartVolume, 0f, smoothT);
                crossfadeBGMSource.volume = Mathf.Lerp(0f, crossfadeTargetVolume, smoothT);

                yield return null;
            }

            // ソースを�Eれ替ぁE            var tempSource = primaryBGMSource;
            primaryBGMSource = crossfadeBGMSource;
            crossfadeBGMSource = tempSource;

            // 古ぁE��ースを停止
            crossfadeBGMSource.Stop();
            crossfadeBGMSource.volume = 0f;

            // 状態更新
            currentTrack = newTrack;
            currentCategory = newTrack.category;
            isTransitioning = false;
            activeTransition = null;

            // イベント発衁E            bgmTrackStartedEvent?.Raise();
            bgmChangeEvent?.RaiseAtPosition(newTrack.trackName, transform.position);

            var eventLogger = ServiceLocator.GetService<IEventLogger>(); if (eventLogger != null) eventLogger.Log($"<color=green>[BGMManager]</color> Crossfaded to BGM: {newTrack.trackName}");
        }

        /// <summary>
        /// フェードアウトコルーチン
        /// </summary>
        private IEnumerator FadeOutCoroutine(float fadeTime)
        {
            float startVolume = primaryBGMSource.volume;
            float currentTime = 0f;

            while (currentTime < fadeTime)
            {
                currentTime += Time.deltaTime;
                float t = currentTime / fadeTime;
                
                primaryBGMSource.volume = Mathf.Lerp(startVolume, 0f, t);
                yield return null;
            }

            primaryBGMSource.Stop();
            currentTrack = null;
            activeTransition = null;
            bgmTrackEndedEvent?.Raise();
        }

        #endregion

        #region Volume and Environmental Control

        /// <summary>
        /// ターゲチE��音量�E計箁E        /// </summary>
        private float CalculateTargetVolume(BGMTrack track)
        {
            float baseVolume = track.volume * masterVolume;

            // スチE��スモード時の音量調整
            if (isStealthModeActive && track.allowInStealthMode)
            {
                baseVolume *= (1f - stealthVolumeReduction);
            }

            // 環墁E��よる音量調整
            baseVolume *= GetEnvironmentalVolumeModifier();

            return Mathf.Clamp01(baseVolume);
        }

        /// <summary>
        /// 環境による音量調整値を取得
        /// </summary>
        private float GetEnvironmentalVolumeModifier()
        {
            float modifier = 1f;

            // 環墁E��よる調整
            modifier *= currentEnvironment switch
            {
                EnvironmentType.Indoor => 0.8f,
                EnvironmentType.Cave => 0.6f,
                EnvironmentType.Forest => 1.2f,
                EnvironmentType.Underwater => 0.4f,
                _ => 1f
            };

            // 天候による調整
            modifier *= currentWeather switch
            {
                WeatherType.Rain => 0.9f,
                WeatherType.Storm => 0.7f,
                WeatherType.Fog => 0.85f,
                _ => 1f
            };

            return modifier;
        }

        /// <summary>
        /// ソース音量�E更新
        /// </summary>
        private void UpdateSourceVolumes()
        {
            if (currentTrack != null)
            {
                primaryBGMSource.volume = CalculateTargetVolume(currentTrack);
            }
        }

        /// <summary>
        /// スチE��ス状態に応じた音量更新
        /// </summary>
        private void UpdateVolumeForStealthState()
        {
            if (currentTrack != null)
            {
                float targetVolume = CalculateTargetVolume(currentTrack);
                
                // スムーズな音量変化
                if (Mathf.Abs(primaryBGMSource.volume - targetVolume) > 0.01f)
                {
                    primaryBGMSource.volume = Mathf.Lerp(
                        primaryBGMSource.volume, 
                        targetVolume, 
                        Time.deltaTime * environmentTransitionSpeed
                    );
                }
            }
        }

        /// <summary>
        /// 環墁E��化に応じた調整を適用
        /// </summary>
        private void ApplyEnvironmentalModifications()
        {
            if (primaryBGMSource.isPlaying)
            {
                // リアルタイムでの環墁E��整
                UpdateSourceVolumes();
                
                // 忁E��に応じて音響フィルターの調整
                ApplyEnvironmentalFilters();
            }
        }

        /// <summary>
        /// 環墁E��響フィルターの適用
        /// </summary>
        private void ApplyEnvironmentalFilters()
        {
            // AudioLowPassFilter めEAudioReverbFilter の調整
            // 実裁E�E具体的な要件に応じて
        }

        #endregion

        #region Request Processing

        /// <summary>
        /// 保留中のリクエストを処琁E        /// </summary>
        private void ProcessPendingRequests()
        {
            if (isTransitioning || pendingRequests.Count == 0) return;

            var request = pendingRequests.Dequeue();
            PlayBGMCategory(request.category, request.immediate);
        }

        #endregion
        
        #region Public Status API
        
        /// <summary>
        /// 現在のBGMが�E生中か確誁E        /// </summary>
        public bool IsPlaying()
        {
            return primaryBGMSource != null && primaryBGMSource.isPlaying;
        }
        
        /// <summary>
        /// 持E��したBGM名が現在再生中か確誁E        /// </summary>
        public bool IsPlaying(string bgmName)
        {
            if (!IsPlaying() || currentTrack == null)
                return false;
                
            // BGM名でのマッチング�E�簡略実裁E��E            return currentTrack.clip != null && 
                   (currentTrack.clip.name.Contains(bgmName) || 
                    currentTrack.trackName.Contains(bgmName));
        }
        
        /// <summary>
        /// 指定したBGMカテゴリが現在再生中か確認
        /// </summary>
        public bool IsPlayingCategory(BGMCategory category)
        {
            return IsPlaying() && currentCategory == category;
        }
        
        /// <summary>
        /// 現在のBGMトラチE��惁E��を取征E        /// </summary>
        public BGMTrack GetCurrentTrack()
        {
            return currentTrack;
        }
        
        /// <summary>
        /// 現在のBGMカテゴリを取得
        /// </summary>
        public BGMCategory GetCurrentCategory()
        {
            return currentCategory;
        }
        
        /// <summary>
        /// 現在の緑張レベルを取征E        /// </summary>
        public float GetCurrentTensionLevel()
        {
            return currentTensionLevel;
        }
        
        /// <summary>
        /// BGM名からBGMカチE��リへの変換
        /// </summary>
        public BGMCategory GetCategoryFromName(string bgmName)
        {
            if (string.IsNullOrEmpty(bgmName))
                return BGMCategory.Ambient;
                
            string lowerName = bgmName.ToLower();
            
            return lowerName switch
            {
                var name when name.Contains("menu") || name.Contains("title") => BGMCategory.Menu,
                var name when name.Contains("combat") || name.Contains("battle") || name.Contains("fight") => BGMCategory.Combat,
                var name when name.Contains("tension") || name.Contains("alert") || name.Contains("suspicious") => BGMCategory.Tension,
                var name when name.Contains("stealth") || name.Contains("success") => BGMCategory.StealthSuccess,
                var name when name.Contains("exploration") || name.Contains("explore") => BGMCategory.Exploration,
                var name when name.Contains("ambient") || name.Contains("normal") || name.Contains("calm") => BGMCategory.Ambient,
                _ => BGMCategory.Ambient
            };
        }
        
        #endregion

        #region Editor Helpers

#if UNITY_EDITOR
        [TabGroup("BGM Manager", "Debug Tools")]
        [Button("Test Tension BGM")]
        public void TestTensionBGM(float testTensionLevel)
        {
            UpdateForTensionLevel(testTensionLevel, false);
        }

        [TabGroup("BGM Manager", "Debug Tools")]
        [Button("Play Category")]
        public void TestPlayCategory(BGMCategory testCategory)
        {
            PlayBGMCategory(testCategory, true);
        }

        [TabGroup("BGM Manager", "Debug Tools")]
        [Button("Stop All BGM")]
        public void TestStopBGM()
        {
            StopBGM(1f);
        }

        private void OnValidate()
        {
            if (Application.isPlaying && primaryBGMSource != null && currentTrack != null)
            {
                UpdateSourceVolumes();
            }
        }
#endif

        #endregion
    }

    #region Supporting Types

    /// <summary>
    /// BGMカテゴリの定義
    /// </summary>
    public enum BGMCategory
    {
        Menu,
        Ambient,
        Exploration,
        Tension,
        Combat,
        StealthSuccess
    }

    /// <summary>
    /// BGMトラチE��惁E��
    /// </summary>
    [System.Serializable]
    public class BGMTrack
    {
        [Header("Basic Settings")]
        public string trackName;
        public AudioClip clip;
        public BGMCategory category = BGMCategory.Ambient;
        [Range(0f, 1f)] public float volume = 0.8f;

        [Header("Playback Settings")]
        public bool looping = true;
        [Range(0f, 5f)] public float fadeInDuration = 2f;
        [Range(0f, 5f)] public float fadeOutDuration = 2f;

        [Header("Stealth Game Integration")]
        [Range(0f, 1f)] public float tensionLevel = 0f;           // こ�E曲が適用される緊張度
        public bool allowInStealthMode = true;                    // スチE��ス中に再生可能ぁE
        [Header("Environmental Context")]
        public EnvironmentType[] suitableEnvironments;            // 適用環墁E        public WeatherType[] suitableWeather;                     // 適用天倁E        public TimeOfDay[] suitableTimeOfDay;                     // 適用時間帯

        public bool IsValidForCurrentConditions(EnvironmentType env, WeatherType weather, TimeOfDay time)
        {
            bool envMatch = suitableEnvironments.Length == 0 || System.Array.Exists(suitableEnvironments, e => e == env);
            bool weatherMatch = suitableWeather.Length == 0 || System.Array.Exists(suitableWeather, w => w == weather);
            bool timeMatch = suitableTimeOfDay.Length == 0 || System.Array.Exists(suitableTimeOfDay, t => t == time);
            
            return envMatch && weatherMatch && timeMatch;
        }
    }

    /// <summary>
    /// BGM再生リクエスチE    /// </summary>
    internal struct BGMPlaybackRequest
    {
        public BGMCategory category;
        public bool immediate;
        public float timestamp;

        public BGMPlaybackRequest(BGMCategory cat, bool imm = false)
        {
            category = cat;
            immediate = imm;
            timestamp = Time.time;
        }
    }

    #endregion
}
