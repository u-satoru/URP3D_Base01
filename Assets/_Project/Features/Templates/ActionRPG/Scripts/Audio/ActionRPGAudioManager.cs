using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using asterivo.Unity60.Core.Events;

namespace asterivo.Unity60.Features.Templates.ActionRPG.Audio
{
    /// <summary>
    /// ActionRPGテンプレート用オーディオマネージャー
    /// BGM、効果音、環境音、戦闘音響管理
    /// </summary>
    public class ActionRPGAudioManager : MonoBehaviour
    {
        [Header("オーディオミキサー")]
        [SerializeField] private AudioMixerGroup masterMixerGroup;
        [SerializeField] private AudioMixerGroup musicMixerGroup;
        [SerializeField] private AudioMixerGroup sfxMixerGroup;
        [SerializeField] private AudioMixerGroup ambientMixerGroup;
        [SerializeField] private AudioMixerGroup voiceMixerGroup;
        
        [Header("BGMプレイヤー")]
        [SerializeField] private AudioSource musicAudioSource;
        [SerializeField] private AudioSource ambientAudioSource;
        [SerializeField] private float musicFadeTime = 2f;
        [SerializeField] private bool loopMusic = true;
        
        [Header("効果音プール")]
        [SerializeField] private int sfxPoolSize = 20;
        [SerializeField] private GameObject sfxAudioSourcePrefab;
        [SerializeField] private Transform sfxParent;
        
        [Header("ActionRPG音源")]
        [SerializeField] private ActionRPGAudioClips audioClips;
        
        [Header("動的音響設定")]
        [SerializeField] private bool enableDynamicMusic = true;
        [SerializeField] private bool enableEnvironmentalAudio = true;
        [SerializeField] private float combatMusicFadeSpeed = 3f;
        [SerializeField] private float ambientVolumeInCombat = 0.3f;
        
        [Header("イベント")]
        [SerializeField] private StringGameEvent onMusicChanged;
        [SerializeField] private StringGameEvent onSFXPlayed;
        
        // 内部状態
        public GameAudioState CurrentAudioState { get; private set; } = GameAudioState.Exploration;
        
        // オーディオプール
        private Queue<AudioSource> availableSFXSources = new();
        private List<AudioSource> activeSFXSources = new();
        
        // BGM管理
        private Coroutine musicFadeCoroutine;
        private AudioClip currentMusicClip;
        private AudioClip previousMusicClip;
        
        // 動的音響
        private bool isInCombat = false;
        private float originalAmbientVolume;
        
        public enum GameAudioState
        {
            MainMenu,       // メインメニュー
            Exploration,    // 探索中
            Combat,         // 戦闘中
            Dialogue,       // 会話中
            Cutscene,       // カットシーン
            Victory,        // 勝利
            GameOver        // ゲームオーバー
        }
        
        private void Start()
        {
            InitializeAudioSystem();
            SetupEventListeners();
        }
        
        private void Update()
        {
            UpdateAudioSources();
        }
        
        /// <summary>
        /// オーディオシステム初期化
        /// </summary>
        private void InitializeAudioSystem()
        {
            // BGM設定
            if (musicAudioSource == null)
            {
                var musicGO = new GameObject("MusicAudioSource");
                musicGO.transform.SetParent(transform);
                musicAudioSource = musicGO.AddComponent<AudioSource>();
            }
            
            musicAudioSource.outputAudioMixerGroup = musicMixerGroup;
            musicAudioSource.loop = loopMusic;
            musicAudioSource.playOnAwake = false;
            
            // 環境音設定
            if (ambientAudioSource == null)
            {
                var ambientGO = new GameObject("AmbientAudioSource");
                ambientGO.transform.SetParent(transform);
                ambientAudioSource = ambientGO.AddComponent<AudioSource>();
            }
            
            ambientAudioSource.outputAudioMixerGroup = ambientMixerGroup;
            ambientAudioSource.loop = true;
            ambientAudioSource.playOnAwake = false;
            originalAmbientVolume = ambientAudioSource.volume;
            
            // SFXプール初期化
            InitializeSFXPool();
            
            // ボリューム設定読み込み
            LoadVolumeSettings();
            
            Debug.Log("[ActionRPGAudioManager] オーディオシステム初期化完了");
        }

        /// <summary>
        /// 外部からのオーディオマネージャー初期化
        /// </summary>
        public void Initialize()
        {
            InitializeAudioSystem();
            SetupEventListeners();
            Debug.Log("[ActionRPGAudioManager] External initialization completed");
        }

        /// <summary>
        /// SFXオーディオプール初期化
        /// </summary>
        private void InitializeSFXPool()
        {
            if (sfxParent == null)
            {
                var sfxPoolGO = new GameObject("SFXPool");
                sfxPoolGO.transform.SetParent(transform);
                sfxParent = sfxPoolGO.transform;
            }
            
            for (int i = 0; i < sfxPoolSize; i++)
            {
                GameObject sfxGO;
                
                if (sfxAudioSourcePrefab != null)
                {
                    sfxGO = Instantiate(sfxAudioSourcePrefab, sfxParent);
                }
                else
                {
                    sfxGO = new GameObject($"SFXSource_{i}");
                    sfxGO.transform.SetParent(sfxParent);
                    sfxGO.AddComponent<AudioSource>();
                }
                
                var audioSource = sfxGO.GetComponent<AudioSource>();
                audioSource.outputAudioMixerGroup = sfxMixerGroup;
                audioSource.playOnAwake = false;
                audioSource.gameObject.SetActive(false);
                
                availableSFXSources.Enqueue(audioSource);
            }
        }
        
        /// <summary>
        /// イベントリスナー設定
        /// </summary>
        private void SetupEventListeners()
        {
            // ゲーム状態変更イベントを購読
            // 実際の実装では適切なイベントに接続
        }
        
        /// <summary>
        /// オーディオソース更新
        /// </summary>
        private void UpdateAudioSources()
        {
            // 再生終了したSFXソースをプールに戻す
            for (int i = activeSFXSources.Count - 1; i >= 0; i--)
            {
                var source = activeSFXSources[i];
                if (!source.isPlaying)
                {
                    ReturnSFXSource(source);
                    activeSFXSources.RemoveAt(i);
                }
            }
        }
        
        /// <summary>
        /// BGM再生
        /// </summary>
        public void PlayMusic(AudioClip musicClip, bool fadeIn = true)
        {
            if (musicClip == null) return;
            
            if (currentMusicClip == musicClip && musicAudioSource.isPlaying) return;
            
            previousMusicClip = currentMusicClip;
            currentMusicClip = musicClip;
            
            if (fadeIn && musicAudioSource.isPlaying)
            {
                // フェードアウト→フェードイン
                if (musicFadeCoroutine != null)
                    StopCoroutine(musicFadeCoroutine);
                    
                musicFadeCoroutine = StartCoroutine(FadeMusicTransition(musicClip));
            }
            else
            {
                // 即座に切り替え
                musicAudioSource.clip = musicClip;
                musicAudioSource.Play();
            }
            
            onMusicChanged?.Raise(musicClip.name);
            
            Debug.Log($"[ActionRPGAudioManager] BGM再生: {musicClip.name}");
        }
        
        /// <summary>
        /// BGMフェード遷移
        /// </summary>
        private IEnumerator FadeMusicTransition(AudioClip newClip)
        {
            float originalVolume = musicAudioSource.volume;
            
            // フェードアウト
            while (musicAudioSource.volume > 0)
            {
                musicAudioSource.volume -= originalVolume * Time.deltaTime / musicFadeTime;
                yield return null;
            }
            
            // 新しい楽曲に切り替え
            musicAudioSource.clip = newClip;
            musicAudioSource.Play();
            
            // フェードイン
            while (musicAudioSource.volume < originalVolume)
            {
                musicAudioSource.volume += originalVolume * Time.deltaTime / musicFadeTime;
                yield return null;
            }
            
            musicAudioSource.volume = originalVolume;
        }
        
        /// <summary>
        /// BGM停止
        /// </summary>
        public void StopMusic(bool fadeOut = true)
        {
            if (!musicAudioSource.isPlaying) return;
            
            if (fadeOut)
            {
                if (musicFadeCoroutine != null)
                    StopCoroutine(musicFadeCoroutine);
                    
                musicFadeCoroutine = StartCoroutine(FadeOutMusic());
            }
            else
            {
                musicAudioSource.Stop();
            }
        }
        
        /// <summary>
        /// BGMフェードアウト
        /// </summary>
        private IEnumerator FadeOutMusic()
        {
            float startVolume = musicAudioSource.volume;
            
            while (musicAudioSource.volume > 0)
            {
                musicAudioSource.volume -= startVolume * Time.deltaTime / musicFadeTime;
                yield return null;
            }
            
            musicAudioSource.Stop();
            musicAudioSource.volume = startVolume;
        }
        
        /// <summary>
        /// 効果音再生
        /// </summary>
        public void PlaySFX(AudioClip sfxClip, float volume = 1f, float pitch = 1f, Vector3? worldPosition = null)
        {
            if (sfxClip == null) return;
            
            var audioSource = GetAvailableSFXSource();
            if (audioSource == null) return;
            
            audioSource.clip = sfxClip;
            audioSource.volume = volume;
            audioSource.pitch = pitch;
            
            if (worldPosition.HasValue)
            {
                audioSource.transform.position = worldPosition.Value;
                audioSource.spatialBlend = 1f; // 3D音響
            }
            else
            {
                audioSource.spatialBlend = 0f; // 2D音響
            }
            
            audioSource.gameObject.SetActive(true);
            audioSource.Play();
            activeSFXSources.Add(audioSource);
            
            onSFXPlayed?.Raise(sfxClip.name);
        }
        
        /// <summary>
        /// SFXソース取得
        /// </summary>
        private AudioSource GetAvailableSFXSource()
        {
            if (availableSFXSources.Count > 0)
            {
                return availableSFXSources.Dequeue();
            }
            
            // プールが空の場合、最も古いアクティブソースを再利用
            if (activeSFXSources.Count > 0)
            {
                var oldestSource = activeSFXSources[0];
                activeSFXSources.RemoveAt(0);
                return oldestSource;
            }
            
            return null;
        }
        
        /// <summary>
        /// SFXソースをプールに戻す
        /// </summary>
        private void ReturnSFXSource(AudioSource audioSource)
        {
            audioSource.gameObject.SetActive(false);
            audioSource.clip = null;
            audioSource.volume = 1f;
            audioSource.pitch = 1f;
            availableSFXSources.Enqueue(audioSource);
        }
        
        /// <summary>
        /// 環境音再生
        /// </summary>
        public void PlayAmbient(AudioClip ambientClip, float volume = 0.5f)
        {
            if (ambientClip == null) return;
            
            ambientAudioSource.clip = ambientClip;
            ambientAudioSource.volume = volume;
            ambientAudioSource.Play();
            
            Debug.Log($"[ActionRPGAudioManager] 環境音再生: {ambientClip.name}");
        }
        
        /// <summary>
        /// ゲーム音響状態変更
        /// </summary>
        public void SetAudioState(GameAudioState newState)
        {
            var previousState = CurrentAudioState;
            CurrentAudioState = newState;
            
            // 状態に応じた音響変更
            switch (newState)
            {
                case GameAudioState.Exploration:
                    SetExplorationAudio();
                    break;
                    
                case GameAudioState.Combat:
                    SetCombatAudio();
                    break;
                    
                case GameAudioState.Dialogue:
                    SetDialogueAudio();
                    break;
                    
                case GameAudioState.Victory:
                    SetVictoryAudio();
                    break;
                    
                case GameAudioState.GameOver:
                    SetGameOverAudio();
                    break;
            }
            
            Debug.Log($"[ActionRPGAudioManager] 音響状態変更: {previousState} → {newState}");
        }
        
        /// <summary>
        /// 探索音響設定
        /// </summary>
        private void SetExplorationAudio()
        {
            if (audioClips != null && audioClips.explorationMusic != null)
            {
                PlayMusic(audioClips.explorationMusic);
            }
            
            if (ambientAudioSource.isPlaying)
            {
                ambientAudioSource.volume = originalAmbientVolume;
            }
            
            isInCombat = false;
        }
        
        /// <summary>
        /// 戦闘音響設定
        /// </summary>
        private void SetCombatAudio()
        {
            if (audioClips != null && audioClips.combatMusic != null)
            {
                PlayMusic(audioClips.combatMusic);
            }
            
            // 環境音を小さくする
            if (ambientAudioSource.isPlaying)
            {
                StartCoroutine(FadeAmbientVolume(ambientVolumeInCombat));
            }
            
            isInCombat = true;
        }
        
        /// <summary>
        /// 会話音響設定
        /// </summary>
        private void SetDialogueAudio()
        {
            // BGMを小さくする
            if (musicAudioSource.isPlaying)
            {
                StartCoroutine(FadeMusicVolume(0.3f));
            }
        }
        
        /// <summary>
        /// 勝利音響設定
        /// </summary>
        private void SetVictoryAudio()
        {
            if (audioClips != null && audioClips.victoryMusic != null)
            {
                PlayMusic(audioClips.victoryMusic, false);
            }
        }
        
        /// <summary>
        /// ゲームオーバー音響設定
        /// </summary>
        private void SetGameOverAudio()
        {
            if (audioClips != null && audioClips.gameOverMusic != null)
            {
                PlayMusic(audioClips.gameOverMusic, false);
            }
        }
        
        /// <summary>
        /// 環境音ボリュームフェード
        /// </summary>
        private IEnumerator FadeAmbientVolume(float targetVolume)
        {
            float startVolume = ambientAudioSource.volume;
            float elapsed = 0f;
            
            while (elapsed < 1f)
            {
                elapsed += Time.deltaTime * combatMusicFadeSpeed;
                ambientAudioSource.volume = Mathf.Lerp(startVolume, targetVolume, elapsed);
                yield return null;
            }
            
            ambientAudioSource.volume = targetVolume;
        }
        
        /// <summary>
        /// BGMボリュームフェード
        /// </summary>
        private IEnumerator FadeMusicVolume(float targetVolume)
        {
            float startVolume = musicAudioSource.volume;
            float elapsed = 0f;
            
            while (elapsed < 1f)
            {
                elapsed += Time.deltaTime * 2f;
                musicAudioSource.volume = Mathf.Lerp(startVolume, targetVolume, elapsed);
                yield return null;
            }
            
            musicAudioSource.volume = targetVolume;
        }
        
        /// <summary>
        /// ボリューム設定読み込み
        /// </summary>
        private void LoadVolumeSettings()
        {
            float masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
            float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.8f);
            float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
            
            SetMasterVolume(masterVolume);
            SetMusicVolume(musicVolume);
            SetSFXVolume(sfxVolume);
        }
        
        /// <summary>
        /// マスターボリューム設定
        /// </summary>
        public void SetMasterVolume(float volume)
        {
            if (masterMixerGroup != null && masterMixerGroup.audioMixer != null)
            {
                masterMixerGroup.audioMixer.SetFloat("MasterVolume", Mathf.Log10(volume) * 20);
            }
        }
        
        /// <summary>
        /// 音楽ボリューム設定
        /// </summary>
        public void SetMusicVolume(float volume)
        {
            if (musicMixerGroup != null && musicMixerGroup.audioMixer != null)
            {
                musicMixerGroup.audioMixer.SetFloat("MusicVolume", Mathf.Log10(volume) * 20);
            }
        }
        
        /// <summary>
        /// 効果音ボリューム設定
        /// </summary>
        public void SetSFXVolume(float volume)
        {
            if (sfxMixerGroup != null && sfxMixerGroup.audioMixer != null)
            {
                sfxMixerGroup.audioMixer.SetFloat("SFXVolume", Mathf.Log10(volume) * 20);
            }
        }
        
        // ActionRPG固有の効果音メソッド
        public void PlayAttackSFX(Vector3 position) => PlaySFX(audioClips?.attackSound, 0.8f, 1f, position);
        public void PlayHitSFX(Vector3 position) => PlaySFX(audioClips?.hitSound, 1f, 1f, position);
        public void PlayHealSFX() => PlaySFX(audioClips?.healSound, 0.7f);
        public void PlayLevelUpSFX() => PlaySFX(audioClips?.levelUpSound, 1f);
        public void PlayItemPickupSFX() => PlaySFX(audioClips?.itemPickupSound, 0.6f);
        public void PlaySkillCastSFX(Vector3 position) => PlaySFX(audioClips?.skillCastSound, 0.9f, 1f, position);
    }
    
    /// <summary>
    /// ActionRPG音響クリップコレクション
    /// </summary>
    [System.Serializable]
    public class ActionRPGAudioClips
    {
        [Header("BGM")]
        public AudioClip mainMenuMusic;
        public AudioClip explorationMusic;
        public AudioClip combatMusic;
        public AudioClip victoryMusic;
        public AudioClip gameOverMusic;
        
        [Header("戦闘効果音")]
        public AudioClip attackSound;
        public AudioClip hitSound;
        public AudioClip blockSound;
        public AudioClip criticalHitSound;
        
        [Header("スキル効果音")]
        public AudioClip skillCastSound;
        public AudioClip healSound;
        public AudioClip buffSound;
        public AudioClip debuffSound;
        
        [Header("UI効果音")]
        public AudioClip buttonClickSound;
        public AudioClip menuOpenSound;
        public AudioClip menuCloseSound;
        public AudioClip errorSound;
        
        [Header("ゲームプレイ効果音")]
        public AudioClip itemPickupSound;
        public AudioClip itemEquipSound;
        public AudioClip levelUpSound;
        public AudioClip questCompleteSound;
        public AudioClip doorOpenSound;
        public AudioClip footstepSound;
        
        [Header("環境音")]
        public AudioClip forestAmbient;
        public AudioClip dungeonAmbient;
        public AudioClip townAmbient;
        public AudioClip battleFieldAmbient;
    }
}