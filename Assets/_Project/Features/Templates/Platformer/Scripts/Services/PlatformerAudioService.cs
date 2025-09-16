using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Features.Templates.Platformer.Settings;

namespace asterivo.Unity60.Features.Templates.Platformer.Services
{
    /// <summary>
    /// Platformer Audio Service：プラットフォーマー音響システムの実装
    /// ServiceLocator + Event駆動アーキテクチャによる疎結合設計
    /// Learn & Grow価値実現：音響体験による学習効果・没入感向上
    /// </summary>
    public class PlatformerAudioService : IPlatformerAudioService
    {
        // 設定・状態管理
        private PlatformerAudioSettings _settings;
        private bool _isInitialized = false;
        private bool _isMuted = false;
        private bool _is3DAudioEnabled = true;

        // オーディオコンポーネント管理
        private AudioSource _bgmSource;
        private AudioListener _audioListener;
        private GameObject _audioManagerObject;

        // オーディオプール（ObjectPool最適化パターン活用）
        private Queue<AudioSource> _audioSourcePool;
        private List<AudioSource> _activeAudioSources;
        private readonly int _poolSize = 20;

        // 音量制御
        private float _masterVolume = 1f;
        private float _bgmVolume = 0.7f;
        private float _sfxVolume = 0.8f;
        private float _ambientVolume = 0.5f;
        private float _uiVolume = 0.9f;

        // 環境音・UI音響
        private AudioSource _ambientSource;
        private AudioSource _uiSource;

        // 足音・移動音管理
        private Dictionary<string, AudioClip[]> _surfaceFootsteps;
        private float _lastFootstepTime = 0f;

        // SFX重複防止・クールダウン管理
        private Dictionary<AudioClip, float> _sfxCooldowns;
        private readonly float _defaultCooldown = 0.1f;

        // 3D音響・エフェクト制御
        private bool _slowMotionActive = false;
        private bool _underwaterActive = false;
        private string _currentAudioZone = "";

        // イベント
        public event Action<string> OnBGMChanged;
        public event Action<bool> OnMuteChanged;
        public event Action<float> OnVolumeChanged;
        public event Action<string> OnAudioZoneChanged;
        public event Action<AudioClip, Vector3> OnSFXPlayed;

        // IPlatformerService実装プロパティ
        public bool IsInitialized => _isInitialized;
        public bool IsEnabled { get; private set; } = false;

        // 追加プロパティ
        public bool IsMuted => _isMuted;
        public bool Is3DAudioEnabled => _is3DAudioEnabled;
        public float MasterVolume
        {
            get => _masterVolume;
            set => SetMasterVolume(value);
        }

        /// <summary>
        /// コンストラクタ：ServiceLocator統合初期化
        /// </summary>
        public PlatformerAudioService(PlatformerAudioSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            InitializeInternal(); // コンストラクタでは内部初期化を呼び出し
        }

        /// <summary>
        /// IPlatformerService: 音響システム初期化
        /// ServiceLocator統合対応
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized) return;
            InitializeInternal();
        }

        /// <summary>
        /// 内部初期化処理
        /// </summary>
        private void InitializeInternal()
        {
            try
            {
                // AudioManagerオブジェクト作成
                _audioManagerObject = new GameObject("PlatformerAudioManager");
                GameObject.DontDestroyOnLoad(_audioManagerObject);

                // 基本設定適用
                _masterVolume = _settings.MasterVolume;
                _bgmVolume = _settings.MusicVolume;
                _sfxVolume = _settings.SfxVolume;
                _ambientVolume = _settings.AmbientVolume;

                // AudioSource初期化
                InitializeAudioSources();

                // オーディオプール初期化
                InitializeAudioPool();

                // 足音辞書初期化
                InitializeFootstepDictionary();

                // SFXクールダウン辞書初期化
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
        /// AudioSource コンポーネント初期化
        /// </summary>
        private void InitializeAudioSources()
        {
            // BGM用AudioSource
            _bgmSource = _audioManagerObject.AddComponent<AudioSource>();
            _bgmSource.loop = true;
            _bgmSource.playOnAwake = false;
            _bgmSource.priority = 128;
            _bgmSource.volume = _bgmVolume;

            // 環境音用AudioSource
            _ambientSource = _audioManagerObject.AddComponent<AudioSource>();
            _ambientSource.loop = true;
            _ambientSource.playOnAwake = false;
            _ambientSource.priority = 200;
            _ambientSource.volume = _ambientVolume;

            // UI音用AudioSource
            _uiSource = _audioManagerObject.AddComponent<AudioSource>();
            _uiSource.playOnAwake = false;
            _uiSource.priority = 100;
            _uiSource.volume = _uiVolume;

            // AudioListener取得または作成
            _audioListener = UnityEngine.Camera.main?.GetComponent<AudioListener>();
            if (_audioListener == null)
            {
                var listenerObject = new GameObject("AudioListener");
                _audioListener = listenerObject.AddComponent<AudioListener>();
                GameObject.DontDestroyOnLoad(listenerObject);
            }
        }

        /// <summary>
        /// オーディオプール初期化（ObjectPool最適化パターン）
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
        /// 足音辞書初期化
        /// </summary>
        private void InitializeFootstepDictionary()
        {
            _surfaceFootsteps = new Dictionary<string, AudioClip[]>();
            // 実際の実装では、設定ファイルから足音音源を読み込み
            // 現在は基本設定として空の辞書を初期化
        }

        #region BGM管理

        // IPlatformerAudioService インターフェース準拠メソッド（パラメータなし）
        public void PlayBackgroundMusic()
        {
            // デフォルトBGMがある場合の実装（設定から取得）
            // 現在は何もしない（設定拡張時に実装）
            Debug.LogWarning("[PlatformerAudioService] PlayBackgroundMusic() requires AudioClip parameter or default BGM in settings");
        }

        public void PlayBackgroundMusic(AudioClip clip, bool loop = true, float fadeInDuration = 1f)
        {
            if (clip == null || _bgmSource == null) return;

            if (_bgmSource.isPlaying)
            {
                // クロスフェード実装
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

        // IPlatformerAudioService インターフェース準拠メソッド（パラメータなし）
        public void StopBackgroundMusic()
        {
            StopBackgroundMusic(1f); // デフォルトフェード時間で呼び出し
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

        #region SFX管理

        public void PlaySFX(AudioClip clip, Vector3 position = default, float volume = 1f, float pitch = 1f)
        {
            if (clip == null || _isMuted) return;

            // SFXクールダウンチェック
            if (IsSFXOnCooldown(clip)) return;

            var audioSource = GetPooledAudioSource();
            if (audioSource == null) return;

            // 3D位置設定
            if (position != default && _is3DAudioEnabled)
            {
                audioSource.transform.position = position;
                audioSource.spatialBlend = 1f; // 3D音響
            }
            else
            {
                audioSource.spatialBlend = 0f; // 2D音響
            }

            // 音響パラメータ設定
            audioSource.clip = clip;
            audioSource.volume = volume * _sfxVolume * _masterVolume;
            audioSource.pitch = pitch;
            audioSource.Play();

            // クールダウン設定
            SetSFXCooldown(clip);

            // イベント通知
            OnSFXPlayed?.Invoke(clip, position);

            // 自動回収
            _audioManagerObject.GetComponent<MonoBehaviour>()?.StartCoroutine(ReturnToPoolAfterPlay(audioSource));
        }

        public void PlaySFXAtPosition(AudioClip clip, Vector3 worldPosition, float volume = 1f, float pitch = 1f)
        {
            PlaySFX(clip, worldPosition, volume, pitch);
        }

        public void PlayPlayerSFX(AudioClip clip, float volume = 1f, float pitch = 1f)
        {
            // プレイヤー位置での2D音響
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

        #region プラットフォーマー特化音響

        // IPlatformerAudioService インターフェース準拠メソッド（パラメータなし）
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

        // IPlatformerAudioService インターフェース準拠メソッド（パラメータなし）
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
            // 足音間隔制御
            if (Time.time - _lastFootstepTime < 0.3f) return;

            // 表面タイプ対応足音（将来拡張）
            AudioClip footstepClip = GetFootstepClip(surfaceType);
            if (footstepClip != null)
            {
                float volume = 0.4f * intensity;
                float pitch = UnityEngine.Random.Range(0.9f, 1.1f);
                PlaySFX(footstepClip, position, volume, pitch);
                _lastFootstepTime = Time.time;
            }
        }

        // IPlatformerAudioService インターフェース準拠メソッド（パラメータなし）
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

        // IPlatformerAudioService インターフェース準拠メソッド（パラメータなし）
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
            // 回復音（設定に追加が必要）
            if (_settings.CollectibleSound != null) // 暫定的に収集音を使用
            {
                float volume = 0.6f;
                float pitch = UnityEngine.Random.Range(0.9f, 1.1f);
                PlaySFX(_settings.CollectibleSound, position, volume, pitch);
            }
        }

        #endregion

        #region 環境音・UI音響

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
            // UI効果音（設定に追加が必要）
        }

        public void PlayMenuSelectSound()
        {
            // UI効果音（設定に追加が必要）
        }

        public void PlayMenuCancelSound()
        {
            // UI効果音（設定に追加が必要）
        }

        #endregion

        #region 3D音響・オーディオゾーン

        public void EnterAudioZone(string zoneName, AudioClip ambientClip = null, float reverbLevel = 0f)
        {
            _currentAudioZone = zoneName;

            if (ambientClip != null)
            {
                PlayAmbientLoop(ambientClip, 0.5f);
            }

            // リバーブエフェクト設定（Unity Audio Mixer使用推奨）
            // 現在は基本実装

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

        #region 動的音響制御

        public void SetSlowMotionEffect(bool enabled, float timeScale = 0.5f)
        {
            _slowMotionActive = enabled;

            if (enabled)
            {
                // 全AudioSourceのピッチを下げる
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
                // ピッチを元に戻す
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

            // ローパスフィルター効果（AudioMixer推奨）
            if (enabled)
            {
                // 水中効果：音量減衰・ローパスフィルター
                _masterVolume *= 0.7f;
            }
            else
            {
                // 通常状態に復元
                _masterVolume = _settings.MasterVolume;
            }

            UpdateAllVolumes();
        }

        public void SetEchoEffect(bool enabled, float echoDelay = 0.1f)
        {
            // エコーエフェクト（AudioMixer + EchoFilter推奨）
            // 基本実装として記録
        }

        #endregion

        #region 音響設定・状態

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

        #region デバッグ・診断

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
            // アクティブソースの検証
            _activeAudioSources.RemoveAll(source => source == null || !source.isPlaying);
        }

        public AudioSource[] GetActiveAudioSources()
        {
            ValidateAudioSources();
            return _activeAudioSources.ToArray();
        }

        #endregion

        #region オーディオプール管理

        public void PreloadAudioClips(AudioClip[] clips)
        {
            // オーディオクリップのプリロード実装
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
            // アクティブソースを停止してプールに戻す
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

            // プールが空の場合、一時的に新しいソースを作成
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

        #region ヘルパーメソッド

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
            return null; // デフォルト足音がある場合はそれを返す
        }

        private IEnumerator CrossfadeBGM(AudioClip newClip, bool loop, float duration)
        {
            float halfDuration = duration * 0.5f;

            // 現在のBGMをフェードアウト
            yield return FadeOut(_bgmSource, halfDuration, false);

            // 新しいBGMを設定してフェードイン
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

        #region IPlatformerService実装

        /// <summary>
        /// IPlatformerService: サービス有効化
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
        /// IPlatformerService: サービス無効化
        /// </summary>
        public void Disable()
        {
            IsEnabled = false;
            SetMuted(true);
            Debug.Log("[PlatformerAudioService] Audio service disabled");
        }

        /// <summary>
        /// IPlatformerService: サービス状態リセット
        /// </summary>
        public void Reset()
        {
            Debug.Log("[PlatformerAudioService] Resetting audio service");

            // 全オーディオ停止
            StopBackgroundMusic(0f);
            StopAmbientLoop(0f);
            ClearAudioPool();

            // 設定を初期状態に戻す
            if (_settings != null)
            {
                _masterVolume = _settings.MasterVolume;
                _bgmVolume = _settings.MusicVolume;
                _sfxVolume = _settings.SfxVolume;
                _ambientVolume = _settings.AmbientVolume;
                UpdateAllVolumes();
            }

            // 状態リセット
            _slowMotionActive = false;
            _underwaterActive = false;
            _currentAudioZone = "";
            _isMuted = false;

            // 有効状態を維持
            if (_isInitialized)
            {
                IsEnabled = true;
            }
        }

        /// <summary>
        /// IPlatformerService: ServiceLocator統合検証
        /// </summary>
        public bool VerifyServiceLocatorIntegration()
        {
            try
            {
                // ServiceLocator統合状態の検証
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
        /// IPlatformerService: サービス実行状態更新
        /// </summary>
        public void UpdateService(float deltaTime)
        {
            if (!_isInitialized || !IsEnabled) return;

            try
            {
                // アクティブオーディオソースの検証・クリーンアップ
                ValidateAudioSources();

                // SFXクールダウンの更新
                UpdateSFXCooldowns();

                // 3D音響位置の更新（必要に応じて）
                Update3DAudioPositions();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PlatformerAudioService] UpdateService error: {ex.Message}");
            }
        }

        /// <summary>
        /// SFXクールダウンの更新
        /// </summary>
        private void UpdateSFXCooldowns()
        {
            // 期限切れクールダウンをクリーンアップ
            var expiredKeys = new List<AudioClip>();
            foreach (var kvp in _sfxCooldowns)
            {
                if (Time.time - kvp.Value > _defaultCooldown * 2f) // 余裕をもって削除
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
        /// 3D音響位置の更新
        /// </summary>
        private void Update3DAudioPositions()
        {
            // 必要に応じて3D AudioSourceの位置を動的に更新
            // 現在は基本実装（将来の拡張ポイント）
        }

        #endregion

        #region IDisposable実装

        public void Dispose()
        {
            try
            {
                // イベント購読解除
                OnBGMChanged = null;
                OnMuteChanged = null;
                OnVolumeChanged = null;
                OnAudioZoneChanged = null;
                OnSFXPlayed = null;

                // オーディオソース停止・クリーンアップ
                ClearAudioPool();

                // オブジェクト破棄
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