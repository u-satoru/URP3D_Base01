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
    /// 繧ｹ繝・・ｽ・ｽ繧ｹ繧ｲ繝ｼ繝迚ｹ蛹厄ｿｽEBGM繝槭ロ繝ｼ繧ｸ繝｣繝ｼ
    /// 邱雁ｼｵ蠎ｦ縺ｨ繧ｲ繝ｼ繝迥ｶ豕√↓蠢懊§縺溷虚逧ВGM蛻ｶ蠕｡
    /// </summary>
    public class BGMManager : MonoBehaviour
    {
        [TabGroup("BGM Manager", "Track Categories")]
        [Header("BGM Track Categories")]
        [SerializeField, Required] private BGMTrack[] menuBGM;
        [SerializeField, Required] private BGMTrack[] ambientBGM;        // 蟷ｳ蟶ｸ譎・
        [SerializeField, Required] private BGMTrack[] tensionBGM;        // 隴ｦ謌呈凾
        [SerializeField, Required] private BGMTrack[] combatBGM;         // 謌ｦ髣俶凾
        [SerializeField] private BGMTrack[] stealthSuccessBGM;           // 繧ｹ繝・Ν繧ｹ謌仙粥譎・
        [SerializeField] private BGMTrack[] explorationBGM;              // 謗｢邏｢譎・

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

        // 迺ｰ蠅・・ｽ・ｽ蠢懊す繧ｹ繝・・ｽ・ｽ
        private EnvironmentType currentEnvironment = EnvironmentType.Outdoor;
        private WeatherType currentWeather = WeatherType.Clear;
        private TimeOfDay currentTimeOfDay = TimeOfDay.Day;

        // 繧ｷ繧ｹ繝・・ｽ・ｽ騾｣謳ｺ
        private StealthAudioCoordinator stealthCoordinator;
        private DynamicAudioEnvironment dynamicEnvironment;

        // 蜀・・ｽ・ｽ迥ｶ諷・        private Dictionary<BGMCategory, BGMTrack[]> bgmCategories;
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
        /// BGM繝槭ロ繝ｼ繧ｸ繝｣繝ｼ縺ｮ蛻晄悄蛹・
        /// </summary>
        private void InitializeBGMManager()
        {
            // BGM繧ｫ繝・ざ繝ｪ霎樊嶌縺ｮ讒狗ｯ・
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
        /// 繧ｪ繝ｼ繝・・ｽ・ｽ繧ｪ繧ｽ繝ｼ繧ｹ縺ｮ繧ｻ繝・・ｽ・ｽ繧｢繝・・ｽE
        /// </summary>
        private void SetupAudioSources()
        {
            // 繝励Λ繧､繝槭ΜBGM繧ｽ繝ｼ繧ｹ縺ｮ險ｭ螳・            if (primaryBGMSource == null)
            {
                var primaryGO = new GameObject("PrimaryBGMSource");
                primaryGO.transform.SetParent(transform);
                primaryBGMSource = primaryGO.AddComponent<AudioSource>();
            }

            ConfigureAudioSource(primaryBGMSource);

            // 繧ｯ繝ｭ繧ｹ繝輔ぉ繝ｼ繝臥畑BGM繧ｽ繝ｼ繧ｹ縺ｮ險ｭ螳・            if (crossfadeBGMSource == null)
            {
                var crossfadeGO = new GameObject("CrossfadeBGMSource");
                crossfadeGO.transform.SetParent(transform);
                crossfadeBGMSource = crossfadeGO.AddComponent<AudioSource>();
            }

            ConfigureAudioSource(crossfadeBGMSource);
        }

        /// <summary>
        /// 繧ｪ繝ｼ繝・・ｽ・ｽ繧ｪ繧ｽ繝ｼ繧ｹ縺ｮ蜈ｱ騾夊ｨｭ螳・        /// </summary>
        private void ConfigureAudioSource(AudioSource source)
        {
            source.playOnAwake = false;
            source.spatialBlend = 0f; // 2D髻ｳ髻ｿ
            source.loop = true;
            source.outputAudioMixerGroup = bgmMixerGroup;
            source.priority = 64; // 荳ｭ遞句ｺｦ縺ｮ蜆ｪ蜈亥ｺｦ
        }

        /// <summary>
        /// 繧ｷ繧ｹ繝・・ｽ・ｽ蜿ゑｿｽE縺ｮ讀懃ｴ｢
        /// </summary>
        private void FindSystemReferences()
        {
            if (stealthCoordinator == null)
                stealthCoordinator = FindFirstObjectByType<StealthAudioCoordinator>();

            if (dynamicEnvironment == null)
                dynamicEnvironment = FindFirstObjectByType<DynamicAudioEnvironment>();
        }

        /// <summary>
        /// 繝・・ｽ・ｽ繧ｩ繝ｫ繝・GM縺ｮ髢句ｧ・        /// </summary>
        private void StartDefaultBGM()
        {
            // 繧ｲ繝ｼ繝髢句ｧ区凾縺ｮBGM驕ｸ謚・            var startingTrack = SelectBGMByCategory(BGMCategory.Ambient);
            if (startingTrack != null)
            {
                PlayBGMImmediately(startingTrack);
            }
        }

        #endregion

        #region Public Interface

        /// <summary>
        /// 邱雁ｼｵ蠎ｦ縺ｫ蠢懊§縺檻GM譖ｴ譁ｰ
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
        /// 迺ｰ蠅・・ｽ・ｽ蠢懊§縺檻GM譖ｴ譁ｰ
        /// </summary>
        public void UpdateForEnvironment(EnvironmentType environment, WeatherType weather, TimeOfDay timeOfDay)
        {
            currentEnvironment = environment;
            currentWeather = weather;
            currentTimeOfDay = timeOfDay;

            // 迺ｰ蠅・・ｽ・ｽ蛹悶↓蠢懊§縺檻GM隱ｿ謨ｴ
            if (currentTrack != null)
            {
                ApplyEnvironmentalModifications();
            }
        }

        /// <summary>
        /// 迚ｹ螳壹き繝・ざ繝ｪ縺ｮBGM繧貞・逕・
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
        /// BGM縺ｮ蛛懈ｭ｢
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
        /// 繝槭せ繧ｿ繝ｼ髻ｳ驥擾ｿｽE險ｭ螳・        /// </summary>
        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            UpdateSourceVolumes();
        }

        /// <summary>
        /// BGM荳譎ょ●豁｢
        /// </summary>
        public void PauseAll()
        {
            primaryBGMSource.Pause();
            crossfadeBGMSource.Pause();
        }

        /// <summary>
        /// BGM蜀埼幕
        /// </summary>
        public void ResumeAll()
        {
            primaryBGMSource.UnPause();
            crossfadeBGMSource.UnPause();
        }

        #endregion

        #region BGM Selection and Playback

        /// <summary>
        /// 邱雁ｼｵ蠎ｦ縺九ｉBGM繧ｫ繝・ざ繝ｪ繧呈ｱｺ螳・
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
        /// 繧ｫ繝・ざ繝ｪ縺ｫ蠢懊§縺檻GM驕ｸ謚・
        /// </summary>
        private BGMTrack SelectBGMByCategory(BGMCategory category)
        {
            if (!bgmCategories.TryGetValue(category, out BGMTrack[] tracks) || tracks.Length == 0)
            {
                ServiceHelper.LogWarning($"[BGMManager] No tracks found for category: {category}");
                return null;
            }

            // 迺ｰ蠅・・ｽE螟ｩ蛟呻ｿｽE譎る俣蟶ｯ縺ｫ驕ｩ縺励◆繝医Λ繝・・ｽ・ｽ繧貞━蜈磯∈謚・            var suitableTracks = FilterTracksByContext(tracks);
            
            if (suitableTracks.Count == 0)
                suitableTracks.AddRange(tracks);

            return suitableTracks[Random.Range(0, suitableTracks.Count)];
        }

        /// <summary>
        /// 迴ｾ蝨ｨ縺ｮ繧ｳ繝ｳ繝・・ｽ・ｽ繧ｹ繝医↓驕ｩ縺励◆繝医Λ繝・・ｽ・ｽ繧偵ヵ繧｣繝ｫ繧ｿ繝ｪ繝ｳ繧ｰ
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
        /// BGM縺ｮ蜊ｳ蠎ｧ蜀咲函
        /// </summary>
        private void PlayBGMImmediately(BGMTrack track)
        {
            if (track?.clip == null) return;

            // 譌｢蟄假ｿｽE逕溘ｒ蛛懈ｭ｢
            primaryBGMSource.Stop();
            crossfadeBGMSource.Stop();

            // 譁ｰ縺励＞繝医Λ繝・・ｽ・ｽ繧定ｨｭ螳夲ｿｽE蜀咲函
            primaryBGMSource.clip = track.clip;
            primaryBGMSource.volume = CalculateTargetVolume(track);
            primaryBGMSource.Play();

            currentTrack = track;
            currentCategory = track.category;
            isTransitioning = false;

            // 繧､繝吶Φ繝育匱陦・            bgmTrackStartedEvent?.Raise();
            bgmChangeEvent?.RaiseAtPosition(track.trackName, transform.position);

            ServiceHelper.Log($"<color=green>[BGMManager]</color> Playing BGM: {track.trackName} (Category: {track.category})");
        }

        /// <summary>
        /// 繧ｹ繝繝ｼ繧ｺ縺ｪBGM蛻・・ｽ・ｽ譖ｿ縺・        /// </summary>
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
        /// 繧ｯ繝ｭ繧ｹ繝輔ぉ繝ｼ繝峨さ繝ｫ繝ｼ繝√Φ
        /// </summary>
        private IEnumerator CrossfadeCoroutine(BGMTrack newTrack)
        {
            isTransitioning = true;

            // 繧ｯ繝ｭ繧ｹ繝輔ぉ繝ｼ繝臥畑繧ｽ繝ｼ繧ｹ縺ｫ譁ｰ縺励＞繝医Λ繝・・ｽ・ｽ繧定ｨｭ螳・            crossfadeBGMSource.clip = newTrack.clip;
            crossfadeBGMSource.volume = 0f;
            crossfadeBGMSource.Play();

            float currentTime = 0f;
            float primaryStartVolume = primaryBGMSource.volume;
            float crossfadeTargetVolume = CalculateTargetVolume(newTrack);

            // 繧ｯ繝ｭ繧ｹ繝輔ぉ繝ｼ繝牙ｮ溯｡・            while (currentTime < crossfadeDuration)
            {
                currentTime += Time.deltaTime;
                float t = currentTime / crossfadeDuration;

                // 繧ｹ繝繝ｼ繧ｺ縺ｪ驕ｷ遘ｻ繧ｫ繝ｼ繝悶ｒ驕ｩ逕ｨ
                float smoothT = tensionCurve.Evaluate(t);

                primaryBGMSource.volume = Mathf.Lerp(primaryStartVolume, 0f, smoothT);
                crossfadeBGMSource.volume = Mathf.Lerp(0f, crossfadeTargetVolume, smoothT);

                yield return null;
            }

            // 繧ｽ繝ｼ繧ｹ繧抵ｿｽE繧梧崛縺・            var tempSource = primaryBGMSource;
            primaryBGMSource = crossfadeBGMSource;
            crossfadeBGMSource = tempSource;

            // 蜿､縺・・ｽ・ｽ繝ｼ繧ｹ繧貞●豁｢
            crossfadeBGMSource.Stop();
            crossfadeBGMSource.volume = 0f;

            // 迥ｶ諷区峩譁ｰ
            currentTrack = newTrack;
            currentCategory = newTrack.category;
            isTransitioning = false;
            activeTransition = null;

            // 繧､繝吶Φ繝育匱陦・            bgmTrackStartedEvent?.Raise();
            bgmChangeEvent?.RaiseAtPosition(newTrack.trackName, transform.position);

            var eventLogger = ServiceLocator.GetService<IEventLogger>(); if (eventLogger != null) eventLogger.Log($"<color=green>[BGMManager]</color> Crossfaded to BGM: {newTrack.trackName}");
        }

        /// <summary>
        /// 繝輔ぉ繝ｼ繝峨い繧ｦ繝医さ繝ｫ繝ｼ繝√Φ
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
        /// 繧ｿ繝ｼ繧ｲ繝・・ｽ・ｽ髻ｳ驥擾ｿｽE險育ｮ・        /// </summary>
        private float CalculateTargetVolume(BGMTrack track)
        {
            float baseVolume = track.volume * masterVolume;

            // 繧ｹ繝・・ｽ・ｽ繧ｹ繝｢繝ｼ繝画凾縺ｮ髻ｳ驥剰ｪｿ謨ｴ
            if (isStealthModeActive && track.allowInStealthMode)
            {
                baseVolume *= (1f - stealthVolumeReduction);
            }

            // 迺ｰ蠅・・ｽ・ｽ繧医ｋ髻ｳ驥剰ｪｿ謨ｴ
            baseVolume *= GetEnvironmentalVolumeModifier();

            return Mathf.Clamp01(baseVolume);
        }

        /// <summary>
        /// 迺ｰ蠅・↓繧医ｋ髻ｳ驥剰ｪｿ謨ｴ蛟､繧貞叙蠕・
        /// </summary>
        private float GetEnvironmentalVolumeModifier()
        {
            float modifier = 1f;

            // 迺ｰ蠅・・ｽ・ｽ繧医ｋ隱ｿ謨ｴ
            modifier *= currentEnvironment switch
            {
                EnvironmentType.Indoor => 0.8f,
                EnvironmentType.Cave => 0.6f,
                EnvironmentType.Forest => 1.2f,
                EnvironmentType.Underwater => 0.4f,
                _ => 1f
            };

            // 螟ｩ蛟吶↓繧医ｋ隱ｿ謨ｴ
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
        /// 繧ｽ繝ｼ繧ｹ髻ｳ驥擾ｿｽE譖ｴ譁ｰ
        /// </summary>
        private void UpdateSourceVolumes()
        {
            if (currentTrack != null)
            {
                primaryBGMSource.volume = CalculateTargetVolume(currentTrack);
            }
        }

        /// <summary>
        /// 繧ｹ繝・・ｽ・ｽ繧ｹ迥ｶ諷九↓蠢懊§縺滄浹驥乗峩譁ｰ
        /// </summary>
        private void UpdateVolumeForStealthState()
        {
            if (currentTrack != null)
            {
                float targetVolume = CalculateTargetVolume(currentTrack);
                
                // 繧ｹ繝繝ｼ繧ｺ縺ｪ髻ｳ驥丞､牙喧
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
        /// 迺ｰ蠅・・ｽ・ｽ蛹悶↓蠢懊§縺溯ｪｿ謨ｴ繧帝←逕ｨ
        /// </summary>
        private void ApplyEnvironmentalModifications()
        {
            if (primaryBGMSource.isPlaying)
            {
                // 繝ｪ繧｢繝ｫ繧ｿ繧､繝縺ｧ縺ｮ迺ｰ蠅・・ｽ・ｽ謨ｴ
                UpdateSourceVolumes();
                
                // 蠢・・ｽ・ｽ縺ｫ蠢懊§縺ｦ髻ｳ髻ｿ繝輔ぅ繝ｫ繧ｿ繝ｼ縺ｮ隱ｿ謨ｴ
                ApplyEnvironmentalFilters();
            }
        }

        /// <summary>
        /// 迺ｰ蠅・・ｽ・ｽ髻ｿ繝輔ぅ繝ｫ繧ｿ繝ｼ縺ｮ驕ｩ逕ｨ
        /// </summary>
        private void ApplyEnvironmentalFilters()
        {
            // AudioLowPassFilter 繧・AudioReverbFilter 縺ｮ隱ｿ謨ｴ
            // 螳溯｣・・ｽE蜈ｷ菴鍋噪縺ｪ隕∽ｻｶ縺ｫ蠢懊§縺ｦ
        }

        #endregion

        #region Request Processing

        /// <summary>
        /// 菫晉蕗荳ｭ縺ｮ繝ｪ繧ｯ繧ｨ繧ｹ繝医ｒ蜃ｦ逅・        /// </summary>
        private void ProcessPendingRequests()
        {
            if (isTransitioning || pendingRequests.Count == 0) return;

            var request = pendingRequests.Dequeue();
            PlayBGMCategory(request.category, request.immediate);
        }

        #endregion
        
        #region Public Status API
        
        /// <summary>
        /// 迴ｾ蝨ｨ縺ｮBGM縺鯉ｿｽE逕滉ｸｭ縺狗｢ｺ隱・        /// </summary>
        public bool IsPlaying()
        {
            return primaryBGMSource != null && primaryBGMSource.isPlaying;
        }
        
        /// <summary>
        /// 謖・・ｽ・ｽ縺励◆BGM蜷阪′迴ｾ蝨ｨ蜀咲函荳ｭ縺狗｢ｺ隱・        /// </summary>
        public bool IsPlaying(string bgmName)
        {
            if (!IsPlaying() || currentTrack == null)
                return false;
                
            // BGM蜷阪〒縺ｮ繝槭ャ繝√Φ繧ｰ・ｽE・ｽ邁｡逡･螳溯｣・・ｽ・ｽE            return currentTrack.clip != null && 
                   (currentTrack.clip.name.Contains(bgmName) || 
                    currentTrack.trackName.Contains(bgmName));
        }
        
        /// <summary>
        /// 謖・ｮ壹＠縺檻GM繧ｫ繝・ざ繝ｪ縺檎樟蝨ｨ蜀咲函荳ｭ縺狗｢ｺ隱・
        /// </summary>
        public bool IsPlayingCategory(BGMCategory category)
        {
            return IsPlaying() && currentCategory == category;
        }
        
        /// <summary>
        /// 迴ｾ蝨ｨ縺ｮBGM繝医Λ繝・・ｽ・ｽ諠・・ｽ・ｽ繧貞叙蠕・        /// </summary>
        public BGMTrack GetCurrentTrack()
        {
            return currentTrack;
        }
        
        /// <summary>
        /// 迴ｾ蝨ｨ縺ｮBGM繧ｫ繝・ざ繝ｪ繧貞叙蠕・
        /// </summary>
        public BGMCategory GetCurrentCategory()
        {
            return currentCategory;
        }
        
        /// <summary>
        /// 迴ｾ蝨ｨ縺ｮ邱大ｼｵ繝ｬ繝吶Ν繧貞叙蠕・        /// </summary>
        public float GetCurrentTensionLevel()
        {
            return currentTensionLevel;
        }
        
        /// <summary>
        /// BGM蜷阪°繧隠GM繧ｫ繝・・ｽ・ｽ繝ｪ縺ｸ縺ｮ螟画鋤
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
    /// BGM繧ｫ繝・ざ繝ｪ縺ｮ螳夂ｾｩ
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
    /// BGM繝医Λ繝・・ｽ・ｽ諠・・ｽ・ｽ
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
        [Range(0f, 1f)] public float tensionLevel = 0f;           // 縺難ｿｽE譖ｲ縺碁←逕ｨ縺輔ｌ繧狗ｷ雁ｼｵ蠎ｦ
        public bool allowInStealthMode = true;                    // 繧ｹ繝・・ｽ・ｽ繧ｹ荳ｭ縺ｫ蜀咲函蜿ｯ閭ｽ縺・
        [Header("Environmental Context")]
        public EnvironmentType[] suitableEnvironments;            // 驕ｩ逕ｨ迺ｰ蠅・
        public WeatherType[] suitableWeather;                     // 驕ｩ逕ｨ螟ｩ蛟・
        public TimeOfDay[] suitableTimeOfDay;                     // 驕ｩ逕ｨ譎る俣蟶ｯ

        public bool IsValidForCurrentConditions(EnvironmentType env, WeatherType weather, TimeOfDay time)
        {
            bool envMatch = suitableEnvironments.Length == 0 || System.Array.Exists(suitableEnvironments, e => e == env);
            bool weatherMatch = suitableWeather.Length == 0 || System.Array.Exists(suitableWeather, w => w == weather);
            bool timeMatch = suitableTimeOfDay.Length == 0 || System.Array.Exists(suitableTimeOfDay, t => t == time);
            
            return envMatch && weatherMatch && timeMatch;
        }
    }

    /// <summary>
    /// BGM蜀咲函繝ｪ繧ｯ繧ｨ繧ｹ繝・    /// </summary>
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

