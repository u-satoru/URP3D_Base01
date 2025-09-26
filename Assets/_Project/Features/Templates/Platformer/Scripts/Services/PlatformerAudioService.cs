using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using asterivo.Unity60.Core;
using asterivo.Unity60.Features.Templates.Platformer.Settings;

namespace asterivo.Unity60.Features.Templates.Platformer.Services
{
    /// <summary>
    /// Platformer Audio Service・壹・繝ｩ繝・ヨ繝輔か繝ｼ繝槭・髻ｳ髻ｿ繧ｷ繧ｹ繝・Β縺ｮ螳溯｣・
    /// ServiceLocator + Event鬧・虚繧｢繝ｼ繧ｭ繝・け繝√Ε縺ｫ繧医ｋ逍守ｵ仙粋險ｭ險・
    /// Learn & Grow萓｡蛟､螳溽樟・夐浹髻ｿ菴馴ｨ薙↓繧医ｋ蟄ｦ鄙貞柑譫懊・豐｡蜈･諢溷髄荳・
    /// </summary>
    public class PlatformerAudioService : IPlatformerAudioService
    {
        // 險ｭ螳壹・迥ｶ諷狗ｮ｡逅・
        private PlatformerAudioSettings _settings;
        private bool _isInitialized = false;
        private bool _isMuted = false;
        private bool _is3DAudioEnabled = true;

        // 繧ｪ繝ｼ繝・ぅ繧ｪ繧ｳ繝ｳ繝昴・繝阪Φ繝育ｮ｡逅・
        private AudioSource _bgmSource;
        private AudioListener _audioListener;
        private GameObject _audioManagerObject;

        // 繧ｪ繝ｼ繝・ぅ繧ｪ繝励・繝ｫ・・bjectPool譛驕ｩ蛹悶ヱ繧ｿ繝ｼ繝ｳ豢ｻ逕ｨ・・
        private Queue<AudioSource> _audioSourcePool;
        private List<AudioSource> _activeAudioSources;
        private readonly int _poolSize = 20;

        // 髻ｳ驥丞宛蠕｡
        private float _masterVolume = 1f;
        private float _bgmVolume = 0.7f;
        private float _sfxVolume = 0.8f;
        private float _ambientVolume = 0.5f;
        private float _uiVolume = 0.9f;

        // 迺ｰ蠅・浹繝ｻUI髻ｳ髻ｿ
        private AudioSource _ambientSource;
        private AudioSource _uiSource;

        // 雜ｳ髻ｳ繝ｻ遘ｻ蜍暮浹邂｡逅・
        private Dictionary<string, AudioClip[]> _surfaceFootsteps;
        private float _lastFootstepTime = 0f;

        // SFX驥崎､・亟豁｢繝ｻ繧ｯ繝ｼ繝ｫ繝繧ｦ繝ｳ邂｡逅・
        private Dictionary<AudioClip, float> _sfxCooldowns;
        private readonly float _defaultCooldown = 0.1f;

        // 3D髻ｳ髻ｿ繝ｻ繧ｨ繝輔ぉ繧ｯ繝亥宛蠕｡
        private bool _slowMotionActive = false;
        private bool _underwaterActive = false;
        private string _currentAudioZone = "";

        // 繧､繝吶Φ繝・
        public event Action<string> OnBGMChanged;
        public event Action<bool> OnMuteChanged;
        public event Action<float> OnVolumeChanged;
        public event Action<string> OnAudioZoneChanged;
        public event Action<AudioClip, Vector3> OnSFXPlayed;

        // IPlatformerService螳溯｣・・繝ｭ繝代ユ繧｣
        public bool IsInitialized => _isInitialized;
        public bool IsEnabled { get; private set; } = false;

        // 霑ｽ蜉繝励Ο繝代ユ繧｣
        public bool IsMuted => _isMuted;
        public bool Is3DAudioEnabled => _is3DAudioEnabled;
        public float MasterVolume
        {
            get => _masterVolume;
            set => SetMasterVolume(value);
        }

        /// <summary>
        /// 繧ｳ繝ｳ繧ｹ繝医Λ繧ｯ繧ｿ・售erviceLocator邨ｱ蜷亥・譛溷喧
        /// </summary>
        public PlatformerAudioService(PlatformerAudioSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            InitializeInternal(); // 繧ｳ繝ｳ繧ｹ繝医Λ繧ｯ繧ｿ縺ｧ縺ｯ蜀・Κ蛻晄悄蛹悶ｒ蜻ｼ縺ｳ蜃ｺ縺・
        }

        /// <summary>
        /// IPlatformerService: 髻ｳ髻ｿ繧ｷ繧ｹ繝・Β蛻晄悄蛹・
        /// ServiceLocator邨ｱ蜷亥ｯｾ蠢・
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized) return;
            InitializeInternal();
        }

        /// <summary>
        /// 蜀・Κ蛻晄悄蛹門・逅・
        /// </summary>
        private void InitializeInternal()
        {
            try
            {
                // AudioManager繧ｪ繝悶ず繧ｧ繧ｯ繝井ｽ懈・
                _audioManagerObject = new GameObject("PlatformerAudioManager");
                GameObject.DontDestroyOnLoad(_audioManagerObject);

                // 蝓ｺ譛ｬ險ｭ螳夐←逕ｨ
                _masterVolume = _settings.MasterVolume;
                _bgmVolume = _settings.MusicVolume;
                _sfxVolume = _settings.SfxVolume;
                _ambientVolume = _settings.AmbientVolume;

                // AudioSource蛻晄悄蛹・
                InitializeAudioSources();

                // 繧ｪ繝ｼ繝・ぅ繧ｪ繝励・繝ｫ蛻晄悄蛹・
                InitializeAudioPool();

                // 雜ｳ髻ｳ霎樊嶌蛻晄悄蛹・
                InitializeFootstepDictionary();

                // SFX繧ｯ繝ｼ繝ｫ繝繧ｦ繝ｳ霎樊嶌蛻晄悄蛹・
                _sfxCooldowns = new Dictionary<AudioClip, float>();

                _isInitialized = true;
                Debug.Log("[PlatformerAudioService] Audio system initialized successfully");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PlatformerAudioService] Failed to initialize audio system: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// AudioSource 繧ｳ繝ｳ繝昴・繝阪Φ繝亥・譛溷喧
        /// </summary>
        private void InitializeAudioSources()
        {
            // BGM逕ｨAudioSource
            _bgmSource = _audioManagerObject.AddComponent<AudioSource>();
            _bgmSource.loop = true;
            _bgmSource.playOnAwake = false;
            _bgmSource.priority = 128;
            _bgmSource.volume = _bgmVolume;

            // 迺ｰ蠅・浹逕ｨAudioSource
            _ambientSource = _audioManagerObject.AddComponent<AudioSource>();
            _ambientSource.loop = true;
            _ambientSource.playOnAwake = false;
            _ambientSource.priority = 200;
            _ambientSource.volume = _ambientVolume;

            // UI髻ｳ逕ｨAudioSource
            _uiSource = _audioManagerObject.AddComponent<AudioSource>();
            _uiSource.playOnAwake = false;
            _uiSource.priority = 100;
            _uiSource.volume = _uiVolume;

            // AudioListener蜿門ｾ励∪縺溘・菴懈・
            _audioListener = UnityEngine.Camera.main?.GetComponent<AudioListener>();
            if (_audioListener == null)
            {
                var listenerObject = new GameObject("AudioListener");
                _audioListener = listenerObject.AddComponent<AudioListener>();
                GameObject.DontDestroyOnLoad(listenerObject);
            }
        }

        /// <summary>
        /// 繧ｪ繝ｼ繝・ぅ繧ｪ繝励・繝ｫ蛻晄悄蛹厄ｼ・bjectPool譛驕ｩ蛹悶ヱ繧ｿ繝ｼ繝ｳ・・
        /// </summary>
        private void InitializeAudioPool()
        {
            _audioSourcePool = new Queue<AudioSource>();
            _activeAudioSources = new List<AudioSource>();

            for (int i = 0; i < _poolSize; i++)
            {
                var audioSource = _audioManagerObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.priority = 150;
                _audioSourcePool.Enqueue(audioSource);
            }
        }

        /// <summary>
        /// 雜ｳ髻ｳ霎樊嶌蛻晄悄蛹・
        /// </summary>
        private void InitializeFootstepDictionary()
        {
            _surfaceFootsteps = new Dictionary<string, AudioClip[]>();
            // 螳滄圀縺ｮ螳溯｣・〒縺ｯ縲∬ｨｭ螳壹ヵ繧｡繧､繝ｫ縺九ｉ雜ｳ髻ｳ髻ｳ貅舌ｒ隱ｭ縺ｿ霎ｼ縺ｿ
            // 迴ｾ蝨ｨ縺ｯ蝓ｺ譛ｬ險ｭ螳壹→縺励※遨ｺ縺ｮ霎樊嶌繧貞・譛溷喧
        }

        #region BGM邂｡逅・

        // IPlatformerAudioService 繧､繝ｳ繧ｿ繝ｼ繝輔ぉ繝ｼ繧ｹ貅匁侠繝｡繧ｽ繝・ラ・医ヱ繝ｩ繝｡繝ｼ繧ｿ縺ｪ縺暦ｼ・
        public void PlayBackgroundMusic()
        {
            // 繝・ヵ繧ｩ繝ｫ繝・GM縺後≠繧句ｴ蜷医・螳溯｣・ｼ郁ｨｭ螳壹°繧牙叙蠕暦ｼ・
            // 迴ｾ蝨ｨ縺ｯ菴輔ｂ縺励↑縺・ｼ郁ｨｭ螳壽僑蠑ｵ譎ゅ↓螳溯｣・ｼ・
            Debug.LogWarning("[PlatformerAudioService] PlayBackgroundMusic() requires AudioClip parameter or default BGM in settings");
        }

        public void PlayBackgroundMusic(AudioClip clip, bool loop = true, float fadeInDuration = 1f)
        {
            if (clip == null || _bgmSource == null) return;

            if (_bgmSource.isPlaying)
            {
                // 繧ｯ繝ｭ繧ｹ繝輔ぉ繝ｼ繝牙ｮ溯｣・
                _audioManagerObject.GetComponent<MonoBehaviour>()?.StartCoroutine(CrossfadeBGM(clip, loop, fadeInDuration));
            }
            else
            {
                _bgmSource.clip = clip;
                _bgmSource.loop = loop;
                _bgmSource.Play();

                if (fadeInDuration > 0f)
                {
                    _audioManagerObject.GetComponent<MonoBehaviour>()?.StartCoroutine(FadeIn(_bgmSource, fadeInDuration));
                }
            }

            OnBGMChanged?.Invoke(clip.name);
        }

        // IPlatformerAudioService 繧､繝ｳ繧ｿ繝ｼ繝輔ぉ繝ｼ繧ｹ貅匁侠繝｡繧ｽ繝・ラ・医ヱ繝ｩ繝｡繝ｼ繧ｿ縺ｪ縺暦ｼ・
        public void StopBackgroundMusic()
        {
            StopBackgroundMusic(1f); // 繝・ヵ繧ｩ繝ｫ繝医ヵ繧ｧ繝ｼ繝画凾髢薙〒蜻ｼ縺ｳ蜃ｺ縺・
        }

        public void StopBackgroundMusic(float fadeOutDuration = 1f)
        {
            if (_bgmSource == null || !_bgmSource.isPlaying) return;

            if (fadeOutDuration > 0f)
            {
                _audioManagerObject.GetComponent<MonoBehaviour>()?.StartCoroutine(FadeOut(_bgmSource, fadeOutDuration, true));
            }
            else
            {
                _bgmSource.Stop();
            }
        }

        public void PauseBackgroundMusic()
        {
            _bgmSource?.Pause();
        }

        public void ResumeBackgroundMusic()
        {
            _bgmSource?.UnPause();
        }

        public void SetBGMVolume(float volume)
        {
            _bgmVolume = Mathf.Clamp01(volume);
            if (_bgmSource != null)
            {
                _bgmSource.volume = _bgmVolume * _masterVolume;
            }
        }

        #endregion

        #region SFX邂｡逅・

        public void PlaySFX(AudioClip clip, Vector3 position = default, float volume = 1f, float pitch = 1f)
        {
            if (clip == null || _isMuted) return;

            // SFX繧ｯ繝ｼ繝ｫ繝繧ｦ繝ｳ繝√ぉ繝・け
            if (IsSFXOnCooldown(clip)) return;

            var audioSource = GetPooledAudioSource();
            if (audioSource == null) return;

            // 3D菴咲ｽｮ險ｭ螳・
            if (position != default && _is3DAudioEnabled)
            {
                audioSource.transform.position = position;
                audioSource.spatialBlend = 1f; // 3D髻ｳ髻ｿ
            }
            else
            {
                audioSource.spatialBlend = 0f; // 2D髻ｳ髻ｿ
            }

            // 髻ｳ髻ｿ繝代Λ繝｡繝ｼ繧ｿ險ｭ螳・
            audioSource.clip = clip;
            audioSource.volume = volume * _sfxVolume * _masterVolume;
            audioSource.pitch = pitch;
            audioSource.Play();

            // 繧ｯ繝ｼ繝ｫ繝繧ｦ繝ｳ險ｭ螳・
            SetSFXCooldown(clip);

            // 繧､繝吶Φ繝磯夂衍
            OnSFXPlayed?.Invoke(clip, position);

            // 閾ｪ蜍募屓蜿・
            _audioManagerObject.GetComponent<MonoBehaviour>()?.StartCoroutine(ReturnToPoolAfterPlay(audioSource));
        }

        public void PlaySFXAtPosition(AudioClip clip, Vector3 worldPosition, float volume = 1f, float pitch = 1f)
        {
            PlaySFX(clip, worldPosition, volume, pitch);
        }

        public void PlayPlayerSFX(AudioClip clip, float volume = 1f, float pitch = 1f)
        {
            // 繝励Ξ繧､繝､繝ｼ菴咲ｽｮ縺ｧ縺ｮ2D髻ｳ髻ｿ
            PlaySFX(clip, default, volume, pitch);
        }

        public void SetSfxVolume(float volume)
        {
            _sfxVolume = Mathf.Clamp01(volume);
        }

        public void SetMusicVolume(float volume)
        {
            _bgmVolume = Mathf.Clamp01(volume);
            if (_bgmSource != null)
            {
                _bgmSource.volume = _bgmVolume * _masterVolume;
            }
        }

        #endregion

        #region 繝励Λ繝・ヨ繝輔か繝ｼ繝槭・迚ｹ蛹夜浹髻ｿ

        // IPlatformerAudioService 繧､繝ｳ繧ｿ繝ｼ繝輔ぉ繝ｼ繧ｹ貅匁侠繝｡繧ｽ繝・ラ・医ヱ繝ｩ繝｡繝ｼ繧ｿ縺ｪ縺暦ｼ・
        public void PlayJumpSound()
        {
            PlayJumpSound(Vector3.zero, 1f);
        }

        public void PlayJumpSound(Vector3 position, float intensity = 1f)
        {
            if (_settings.JumpSound != null)
            {
                float volume = 0.8f * intensity;
                float pitch = UnityEngine.Random.Range(0.9f, 1.1f);
                PlaySFX(_settings.JumpSound, position, volume, pitch);
            }
        }

        // IPlatformerAudioService 繧､繝ｳ繧ｿ繝ｼ繝輔ぉ繝ｼ繧ｹ貅匁侠繝｡繧ｽ繝・ラ・医ヱ繝ｩ繝｡繝ｼ繧ｿ縺ｪ縺暦ｼ・
        public void PlayLandSound()
        {
            PlayLandingSound(Vector3.zero, 1f);
        }

        public void PlayLandingSound(Vector3 position, float intensity = 1f, string surfaceType = "default")
        {
            if (_settings.LandSound != null)
            {
                float volume = 0.9f * intensity;
                float pitch = UnityEngine.Random.Range(0.8f, 1.2f);
                PlaySFX(_settings.LandSound, position, volume, pitch);
            }
        }

        public void PlayFootstepSound(Vector3 position, float intensity = 1f, string surfaceType = "default")
        {
            // 雜ｳ髻ｳ髢馴囈蛻ｶ蠕｡
            if (Time.time - _lastFootstepTime < 0.3f) return;

            // 陦ｨ髱｢繧ｿ繧､繝怜ｯｾ蠢懆ｶｳ髻ｳ・亥ｰ・擂諡｡蠑ｵ・・
            AudioClip footstepClip = GetFootstepClip(surfaceType);
            if (footstepClip != null)
            {
                float volume = 0.4f * intensity;
                float pitch = UnityEngine.Random.Range(0.9f, 1.1f);
                PlaySFX(footstepClip, position, volume, pitch);
                _lastFootstepTime = Time.time;
            }
        }

        // IPlatformerAudioService 繧､繝ｳ繧ｿ繝ｼ繝輔ぉ繝ｼ繧ｹ貅匁侠繝｡繧ｽ繝・ラ・医ヱ繝ｩ繝｡繝ｼ繧ｿ縺ｪ縺暦ｼ・
        public void PlayCollectibleSound()
        {
            PlayCollectibleSound(Vector3.zero, 1);
        }

        public void PlayCollectibleSound(Vector3 position, int itemValue = 1)
        {
            if (_settings.CollectibleSound != null)
            {
                float volume = 0.7f;
                float pitch = Mathf.Clamp(1f + (itemValue * 0.1f), 1f, 1.5f);
                PlaySFX(_settings.CollectibleSound, position, volume, pitch);
            }
        }

        // IPlatformerAudioService 繧､繝ｳ繧ｿ繝ｼ繝輔ぉ繝ｼ繧ｹ貅匁侠繝｡繧ｽ繝・ラ・医ヱ繝ｩ繝｡繝ｼ繧ｿ縺ｪ縺暦ｼ・
        public void PlayDamageSound()
        {
            PlayDamageSound(Vector3.zero, 1f);
        }

        public void PlayDamageSound(Vector3 position, float damageAmount)
        {
            if (_settings.DamageSound != null)
            {
                float volume = Mathf.Clamp(0.8f + (damageAmount * 0.1f), 0.5f, 1f);
                float pitch = UnityEngine.Random.Range(0.8f, 1.2f);
                PlaySFX(_settings.DamageSound, position, volume, pitch);
            }
        }

        public void PlayHealSound(Vector3 position, float healAmount)
        {
            // 蝗槫ｾｩ髻ｳ・郁ｨｭ螳壹↓霑ｽ蜉縺悟ｿ・ｦ・ｼ・
            if (_settings.CollectibleSound != null) // 證ｫ螳夂噪縺ｫ蜿朱寔髻ｳ繧剃ｽｿ逕ｨ
            {
                float volume = 0.6f;
                float pitch = UnityEngine.Random.Range(0.9f, 1.1f);
                PlaySFX(_settings.CollectibleSound, position, volume, pitch);
            }
        }

        #endregion

        #region 迺ｰ蠅・浹繝ｻUI髻ｳ髻ｿ

        public void PlayAmbientLoop(AudioClip clip, float volume = 0.5f)
        {
            if (clip == null || _ambientSource == null) return;

            _ambientSource.clip = clip;
            _ambientSource.volume = volume * _ambientVolume * _masterVolume;
            _ambientSource.loop = true;
            _ambientSource.Play();
        }

        public void StopAmbientLoop(float fadeOutDuration = 2f)
        {
            if (_ambientSource == null || !_ambientSource.isPlaying) return;

            if (fadeOutDuration > 0f)
            {
                _audioManagerObject.GetComponent<MonoBehaviour>()?.StartCoroutine(FadeOut(_ambientSource, fadeOutDuration, true));
            }
            else
            {
                _ambientSource.Stop();
            }
        }

        public void SetAmbientVolume(float volume)
        {
            _ambientVolume = Mathf.Clamp01(volume);
            if (_ambientSource != null)
            {
                _ambientSource.volume = _ambientVolume * _masterVolume;
            }
        }

        public void PlayUISound(AudioClip clip, float volume = 1f)
        {
            if (clip == null || _uiSource == null) return;

            _uiSource.clip = clip;
            _uiSource.volume = volume * _uiVolume * _masterVolume;
            _uiSource.Play();
        }

        public void PlayMenuNavigateSound()
        {
            // UI蜉ｹ譫憺浹・郁ｨｭ螳壹↓霑ｽ蜉縺悟ｿ・ｦ・ｼ・
        }

        public void PlayMenuSelectSound()
        {
            // UI蜉ｹ譫憺浹・郁ｨｭ螳壹↓霑ｽ蜉縺悟ｿ・ｦ・ｼ・
        }

        public void PlayMenuCancelSound()
        {
            // UI蜉ｹ譫憺浹・郁ｨｭ螳壹↓霑ｽ蜉縺悟ｿ・ｦ・ｼ・
        }

        #endregion

        #region 3D髻ｳ髻ｿ繝ｻ繧ｪ繝ｼ繝・ぅ繧ｪ繧ｾ繝ｼ繝ｳ

        public void EnterAudioZone(string zoneName, AudioClip ambientClip = null, float reverbLevel = 0f)
        {
            _currentAudioZone = zoneName;

            if (ambientClip != null)
            {
                PlayAmbientLoop(ambientClip, 0.5f);
            }

            // 繝ｪ繝舌・繝悶お繝輔ぉ繧ｯ繝郁ｨｭ螳夲ｼ・nity Audio Mixer菴ｿ逕ｨ謗ｨ螂ｨ・・
            // 迴ｾ蝨ｨ縺ｯ蝓ｺ譛ｬ螳溯｣・

            OnAudioZoneChanged?.Invoke(zoneName);
        }

        public void ExitAudioZone(string zoneName)
        {
            if (_currentAudioZone == zoneName)
            {
                _currentAudioZone = "";
                StopAmbientLoop(2f);
                OnAudioZoneChanged?.Invoke("");
            }
        }

        public void SetListenerPosition(Vector3 position, Quaternion rotation)
        {
            if (_audioListener != null)
            {
                _audioListener.transform.position = position;
                _audioListener.transform.rotation = rotation;
            }
        }

        #endregion

        #region 蜍慕噪髻ｳ髻ｿ蛻ｶ蠕｡

        public void SetSlowMotionEffect(bool enabled, float timeScale = 0.5f)
        {
            _slowMotionActive = enabled;

            if (enabled)
            {
                // 蜈ｨAudioSource縺ｮ繝斐ャ繝√ｒ荳九￡繧・
                foreach (var source in _activeAudioSources)
                {
                    if (source != null && source.isPlaying)
                    {
                        source.pitch *= timeScale;
                    }
                }

                if (_bgmSource != null && _bgmSource.isPlaying)
                {
                    _bgmSource.pitch = timeScale;
                }
            }
            else
            {
                // 繝斐ャ繝√ｒ蜈・↓謌ｻ縺・
                foreach (var source in _activeAudioSources)
                {
                    if (source != null)
                    {
                        source.pitch = 1f;
                    }
                }

                if (_bgmSource != null)
                {
                    _bgmSource.pitch = 1f;
                }
            }
        }

        public void SetUnderWaterEffect(bool enabled)
        {
            _underwaterActive = enabled;

            // 繝ｭ繝ｼ繝代せ繝輔ぅ繝ｫ繧ｿ繝ｼ蜉ｹ譫懶ｼ・udioMixer謗ｨ螂ｨ・・
            if (enabled)
            {
                // 豌ｴ荳ｭ蜉ｹ譫懶ｼ夐浹驥乗ｸ幄｡ｰ繝ｻ繝ｭ繝ｼ繝代せ繝輔ぅ繝ｫ繧ｿ繝ｼ
                _masterVolume *= 0.7f;
            }
            else
            {
                // 騾壼ｸｸ迥ｶ諷九↓蠕ｩ蜈・
                _masterVolume = _settings.MasterVolume;
            }

            UpdateAllVolumes();
        }

        public void SetEchoEffect(bool enabled, float echoDelay = 0.1f)
        {
            // 繧ｨ繧ｳ繝ｼ繧ｨ繝輔ぉ繧ｯ繝茨ｼ・udioMixer + EchoFilter謗ｨ螂ｨ・・
            // 蝓ｺ譛ｬ螳溯｣・→縺励※險倬鹸
        }

        #endregion

        #region 髻ｳ髻ｿ險ｭ螳壹・迥ｶ諷・

        public void UpdateSettings(object settings)
        {
            if (settings is PlatformerAudioSettings audioSettings)
            {
                _settings = audioSettings;
                _masterVolume = _settings.MasterVolume;
                _bgmVolume = _settings.MusicVolume;
                _sfxVolume = _settings.SfxVolume;
                _ambientVolume = _settings.AmbientVolume;

                UpdateAllVolumes();
            }
        }

        public void SetMuted(bool muted)
        {
            _isMuted = muted;

            if (_bgmSource != null) _bgmSource.mute = muted;
            if (_ambientSource != null) _ambientSource.mute = muted;
            if (_uiSource != null) _uiSource.mute = muted;

            foreach (var source in _activeAudioSources)
            {
                if (source != null) source.mute = muted;
            }

            OnMuteChanged?.Invoke(muted);
        }

        public void Set3DAudioEnabled(bool enabled)
        {
            _is3DAudioEnabled = enabled;
        }

        public void SetMasterVolume(float volume)
        {
            _masterVolume = Mathf.Clamp01(volume);
            UpdateAllVolumes();
            OnVolumeChanged?.Invoke(_masterVolume);
        }

        private void UpdateAllVolumes()
        {
            if (_bgmSource != null) _bgmSource.volume = _bgmVolume * _masterVolume;
            if (_ambientSource != null) _ambientSource.volume = _ambientVolume * _masterVolume;
            if (_uiSource != null) _uiSource.volume = _uiVolume * _masterVolume;
        }

        #endregion

        #region 繝・ヰ繝・げ繝ｻ險ｺ譁ｭ

        public void ShowAudioInfo()
        {
            Debug.Log("=== Platformer Audio Service Info ===");
            Debug.Log($"Initialized: {_isInitialized}");
            Debug.Log($"Muted: {_isMuted}");
            Debug.Log($"Master Volume: {_masterVolume:F2}");
            Debug.Log($"BGM Volume: {_bgmVolume:F2}");
            Debug.Log($"SFX Volume: {_sfxVolume:F2}");
            Debug.Log($"3D Audio Enabled: {_is3DAudioEnabled}");
            Debug.Log($"Active Audio Sources: {_activeAudioSources.Count}");
            Debug.Log($"Pooled Audio Sources: {_audioSourcePool.Count}");
            Debug.Log($"Current Audio Zone: {_currentAudioZone}");
        }

        public void ValidateAudioSources()
        {
            // 繧｢繧ｯ繝・ぅ繝悶た繝ｼ繧ｹ縺ｮ讀懆ｨｼ
            _activeAudioSources.RemoveAll(source => source == null || !source.isPlaying);
        }

        public AudioSource[] GetActiveAudioSources()
        {
            ValidateAudioSources();
            return _activeAudioSources.ToArray();
        }

        #endregion

        #region 繧ｪ繝ｼ繝・ぅ繧ｪ繝励・繝ｫ邂｡逅・

        public void PreloadAudioClips(AudioClip[] clips)
        {
            // 繧ｪ繝ｼ繝・ぅ繧ｪ繧ｯ繝ｪ繝・・縺ｮ繝励Μ繝ｭ繝ｼ繝牙ｮ溯｣・
            foreach (var clip in clips)
            {
                if (clip != null)
                {
                    clip.LoadAudioData();
                }
            }
        }

        public void ClearAudioPool()
        {
            // 繧｢繧ｯ繝・ぅ繝悶た繝ｼ繧ｹ繧貞●豁｢縺励※繝励・繝ｫ縺ｫ謌ｻ縺・
            foreach (var source in _activeAudioSources)
            {
                if (source != null)
                {
                    source.Stop();
                    _audioSourcePool.Enqueue(source);
                }
            }
            _activeAudioSources.Clear();
        }

        public int GetPooledSourcesCount()
        {
            return _audioSourcePool.Count;
        }

        private AudioSource GetPooledAudioSource()
        {
            if (_audioSourcePool.Count > 0)
            {
                var source = _audioSourcePool.Dequeue();
                _activeAudioSources.Add(source);
                return source;
            }

            // 繝励・繝ｫ縺檎ｩｺ縺ｮ蝣ｴ蜷医∽ｸ譎ら噪縺ｫ譁ｰ縺励＞繧ｽ繝ｼ繧ｹ繧剃ｽ懈・
            var newSource = _audioManagerObject.AddComponent<AudioSource>();
            newSource.playOnAwake = false;
            newSource.priority = 150;
            _activeAudioSources.Add(newSource);

            return newSource;
        }

        private IEnumerator ReturnToPoolAfterPlay(AudioSource source)
        {
            yield return new WaitWhile(() => source.isPlaying);

            if (source != null)
            {
                _activeAudioSources.Remove(source);
                source.clip = null;
                source.volume = 1f;
                source.pitch = 1f;
                source.spatialBlend = 0f;
                _audioSourcePool.Enqueue(source);
            }
        }

        #endregion

        #region 繝倥Ν繝代・繝｡繧ｽ繝・ラ

        private bool IsSFXOnCooldown(AudioClip clip)
        {
            if (_sfxCooldowns.TryGetValue(clip, out float lastPlayTime))
            {
                return Time.time - lastPlayTime < _defaultCooldown;
            }
            return false;
        }

        private void SetSFXCooldown(AudioClip clip)
        {
            _sfxCooldowns[clip] = Time.time;
        }

        private AudioClip GetFootstepClip(string surfaceType)
        {
            if (_surfaceFootsteps.TryGetValue(surfaceType, out AudioClip[] clips) && clips.Length > 0)
            {
                return clips[UnityEngine.Random.Range(0, clips.Length)];
            }
            return null; // 繝・ヵ繧ｩ繝ｫ繝郁ｶｳ髻ｳ縺後≠繧句ｴ蜷医・縺昴ｌ繧定ｿ斐☆
        }

        private IEnumerator CrossfadeBGM(AudioClip newClip, bool loop, float duration)
        {
            float halfDuration = duration * 0.5f;

            // 迴ｾ蝨ｨ縺ｮBGM繧偵ヵ繧ｧ繝ｼ繝峨い繧ｦ繝・
            yield return FadeOut(_bgmSource, halfDuration, false);

            // 譁ｰ縺励＞BGM繧定ｨｭ螳壹＠縺ｦ繝輔ぉ繝ｼ繝峨う繝ｳ
            _bgmSource.clip = newClip;
            _bgmSource.loop = loop;
            _bgmSource.Play();
            yield return FadeIn(_bgmSource, halfDuration);
        }

        private IEnumerator FadeIn(AudioSource source, float duration)
        {
            float targetVolume = source.volume;
            source.volume = 0f;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                source.volume = Mathf.Lerp(0f, targetVolume, elapsed / duration);
                yield return null;
            }

            source.volume = targetVolume;
        }

        private IEnumerator FadeOut(AudioSource source, float duration, bool stopAfterFade)
        {
            float startVolume = source.volume;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                source.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
                yield return null;
            }

            source.volume = 0f;
            if (stopAfterFade)
            {
                source.Stop();
                source.volume = startVolume;
            }
        }

        #endregion

        #region IPlatformerService螳溯｣・

        /// <summary>
        /// IPlatformerService: 繧ｵ繝ｼ繝薙せ譛牙柑蛹・
        /// </summary>
        public void Enable()
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("[PlatformerAudioService] Cannot enable: Service not initialized");
                return;
            }

            IsEnabled = true;
            SetMuted(false);
            Debug.Log("[PlatformerAudioService] Audio service enabled");
        }

        /// <summary>
        /// IPlatformerService: 繧ｵ繝ｼ繝薙せ辟｡蜉ｹ蛹・
        /// </summary>
        public void Disable()
        {
            IsEnabled = false;
            SetMuted(true);
            Debug.Log("[PlatformerAudioService] Audio service disabled");
        }

        /// <summary>
        /// IPlatformerService: 繧ｵ繝ｼ繝薙せ迥ｶ諷九Μ繧ｻ繝・ヨ
        /// </summary>
        public void Reset()
        {
            Debug.Log("[PlatformerAudioService] Resetting audio service");

            // 蜈ｨ繧ｪ繝ｼ繝・ぅ繧ｪ蛛懈ｭ｢
            StopBackgroundMusic(0f);
            StopAmbientLoop(0f);
            ClearAudioPool();

            // 險ｭ螳壹ｒ蛻晄悄迥ｶ諷九↓謌ｻ縺・
            if (_settings != null)
            {
                _masterVolume = _settings.MasterVolume;
                _bgmVolume = _settings.MusicVolume;
                _sfxVolume = _settings.SfxVolume;
                _ambientVolume = _settings.AmbientVolume;
                UpdateAllVolumes();
            }

            // 迥ｶ諷九Μ繧ｻ繝・ヨ
            _slowMotionActive = false;
            _underwaterActive = false;
            _currentAudioZone = "";
            _isMuted = false;

            // 譛牙柑迥ｶ諷九ｒ邯ｭ謖・
            if (_isInitialized)
            {
                IsEnabled = true;
            }
        }

        /// <summary>
        /// IPlatformerService: ServiceLocator邨ｱ蜷域､懆ｨｼ
        /// </summary>
        public bool VerifyServiceLocatorIntegration()
        {
            try
            {
                // ServiceLocator邨ｱ蜷育憾諷九・讀懆ｨｼ
                bool hasValidSettings = _settings != null;
                bool hasAudioManager = _audioManagerObject != null;
                bool hasRequiredSources = _bgmSource != null && _ambientSource != null && _uiSource != null;
                bool hasValidPool = _audioSourcePool != null && _activeAudioSources != null;

                bool isIntegrationValid = hasValidSettings && hasAudioManager && hasRequiredSources && hasValidPool;

                if (!isIntegrationValid)
                {
                    Debug.LogWarning($"[PlatformerAudioService] ServiceLocator integration issues detected: " +
                        $"Settings={hasValidSettings}, Manager={hasAudioManager}, Sources={hasRequiredSources}, Pool={hasValidPool}");
                }
                else
                {
                    Debug.Log("[PlatformerAudioService] ServiceLocator integration verified successfully");
                }

                return isIntegrationValid;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PlatformerAudioService] ServiceLocator integration verification failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// IPlatformerService: 繧ｵ繝ｼ繝薙せ螳溯｡檎憾諷区峩譁ｰ
        /// </summary>
        public void UpdateService(float deltaTime)
        {
            if (!_isInitialized || !IsEnabled) return;

            try
            {
                // 繧｢繧ｯ繝・ぅ繝悶が繝ｼ繝・ぅ繧ｪ繧ｽ繝ｼ繧ｹ縺ｮ讀懆ｨｼ繝ｻ繧ｯ繝ｪ繝ｼ繝ｳ繧｢繝・・
                ValidateAudioSources();

                // SFX繧ｯ繝ｼ繝ｫ繝繧ｦ繝ｳ縺ｮ譖ｴ譁ｰ
                UpdateSFXCooldowns();

                // 3D髻ｳ髻ｿ菴咲ｽｮ縺ｮ譖ｴ譁ｰ・亥ｿ・ｦ√↓蠢懊§縺ｦ・・
                Update3DAudioPositions();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PlatformerAudioService] UpdateService error: {ex.Message}");
            }
        }

        /// <summary>
        /// SFX繧ｯ繝ｼ繝ｫ繝繧ｦ繝ｳ縺ｮ譖ｴ譁ｰ
        /// </summary>
        private void UpdateSFXCooldowns()
        {
            // 譛滄剞蛻・ｌ繧ｯ繝ｼ繝ｫ繝繧ｦ繝ｳ繧偵け繝ｪ繝ｼ繝ｳ繧｢繝・・
            var expiredKeys = new List<AudioClip>();
            foreach (var kvp in _sfxCooldowns)
            {
                if (Time.time - kvp.Value > _defaultCooldown * 2f) // 菴呵｣輔ｒ繧ゅ▲縺ｦ蜑企勁
                {
                    expiredKeys.Add(kvp.Key);
                }
            }

            foreach (var key in expiredKeys)
            {
                _sfxCooldowns.Remove(key);
            }
        }

        /// <summary>
        /// 3D髻ｳ髻ｿ菴咲ｽｮ縺ｮ譖ｴ譁ｰ
        /// </summary>
        private void Update3DAudioPositions()
        {
            // 蠢・ｦ√↓蠢懊§縺ｦ3D AudioSource縺ｮ菴咲ｽｮ繧貞虚逧・↓譖ｴ譁ｰ
            // 迴ｾ蝨ｨ縺ｯ蝓ｺ譛ｬ螳溯｣・ｼ亥ｰ・擂縺ｮ諡｡蠑ｵ繝昴う繝ｳ繝茨ｼ・
        }

        #endregion

        #region IDisposable螳溯｣・

        public void Dispose()
        {
            try
            {
                // 繧､繝吶Φ繝郁ｳｼ隱ｭ隗｣髯､
                OnBGMChanged = null;
                OnMuteChanged = null;
                OnVolumeChanged = null;
                OnAudioZoneChanged = null;
                OnSFXPlayed = null;

                // 繧ｪ繝ｼ繝・ぅ繧ｪ繧ｽ繝ｼ繧ｹ蛛懈ｭ｢繝ｻ繧ｯ繝ｪ繝ｼ繝ｳ繧｢繝・・
                ClearAudioPool();

                // 繧ｪ繝悶ず繧ｧ繧ｯ繝育ｴ譽・
                if (_audioManagerObject != null)
                {
                    GameObject.DestroyImmediate(_audioManagerObject);
                    _audioManagerObject = null;
                }

                _isInitialized = false;
                Debug.Log("[PlatformerAudioService] Audio service disposed successfully");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PlatformerAudioService] Error during disposal: {ex.Message}");
            }
        }

        #endregion
    }
}


