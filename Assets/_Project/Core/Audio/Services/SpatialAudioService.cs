using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using System.Collections.Generic;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Audio.Events;
using asterivo.Unity60.Core.Debug;
using asterivo.Unity60.Core.Audio.Interfaces;
using _Project.Core;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Core.Audio.Services
{
    /// <summary>
    /// 空間音響サービス（イベント駆動型）
    /// Singletonパターンを使わず、イベント経由で制御
    /// </summary>
    public class SpatialAudioService : MonoBehaviour, ISpatialAudioService, IInitializable
    {
        [Header("Audio Configuration")]
        [SerializeField] private AudioMixerGroup spatialMixerGroup;
        [SerializeField] private int maxAudioSources = 32;
        [SerializeField] private GameObject audioSourcePrefab;
        
        [Header("Spatial Settings")]
        [SerializeField, Range(0f, 5f)] private float dopplerLevel = 1f;
        [SerializeField] private AnimationCurve volumeRolloffCurve;
        [SerializeField] private float defaultMaxDistance = 50f;
        
        [Header("Events")]
        [SerializeField] private SpatialAudioEvent onSpatialSoundRequested;
        [SerializeField] private GameEvent onAudioSystemInitialized;
        
        [Header("Audio Source Pool")]
        [SerializeField, ReadOnly] private List<AudioSource> availableSources = new List<AudioSource>();
        [SerializeField, ReadOnly] private Dictionary<string, AudioSource> activeSources = new Dictionary<string, AudioSource>();
        
        // Occlusion and reverb settings
        private Dictionary<string, float> reverbZones = new Dictionary<string, float>();
        private AudioListener mainListener;
        
        // IInitializable
        public int Priority => 20; // 空間音響は基本オーディオシステムの後に初期化
        public bool IsInitialized { get; private set; }
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            // ServiceLocatorに登録
            if (FeatureFlags.UseServiceLocator)
            {
                ServiceLocator.RegisterService<ISpatialAudioService>(this);
                
                if (FeatureFlags.EnableDebugLogging)
                {
                    EventLogger.Log("[SpatialAudioService] Registered to ServiceLocator");
                }
            }
            
            InitializeAudioSourcePool();
        }
        
        private void Start()
        {
            Initialize();
            
            // イベントリスナーの設定
            if (onSpatialSoundRequested != null && FeatureFlags.UseEventDrivenAudio)
            {
                // SpatialAudioEvent用のリスナーを追加
                // TODO: SpatialAudioEvent専用のリスナーが必要
                // var listener = gameObject.AddComponent<SpatialAudioEventListener>();
                // listener.GameEvent = onSpatialSoundRequested;
                // listener.OnEventRaised.AddListener(HandleSpatialSoundEvent);
                
                // イベントリスナーとして登録するには別の方法が必要
                // TODO: GenericGameEventListener<SpatialAudioData>を使用するか、
                // またはIGameEventListener<SpatialAudioData>を実装する必要がある
            }
        }
        
        private void OnDestroy()
        {
            if (FeatureFlags.UseServiceLocator)
            {
                ServiceLocator.UnregisterService<ISpatialAudioService>();
            }
            
            CleanupAudioSources();
        }
        
        #endregion
        
        #region IInitializable Implementation
        
        public void Initialize()
        {
            if (IsInitialized) return;
            
            mainListener = FindFirstObjectByType<AudioListener>();
            if (mainListener == null)
            {
                EventLogger.LogWarning("[SpatialAudioService] No AudioListener found in scene");
            }
            
            SetupDefaultSettings();
            
            if (onAudioSystemInitialized != null)
            {
                onAudioSystemInitialized.Raise();
            }
            
            IsInitialized = true;
            
            if (FeatureFlags.EnableDebugLogging)
            {
                EventLogger.Log("[SpatialAudioService] Initialization complete");
            }
        }
        
        #endregion
        
        #region ISpatialAudioService Implementation
        
        public void Play3DSound(string soundId, Vector3 position, float maxDistance = 50f, float volume = 1f)
        {
            if (!IsInitialized)
            {
                EventLogger.LogWarning("[SpatialAudioService] System not initialized");
                return;
            }
            
            // イベント駆動モードの場合
            if (FeatureFlags.UseEventDrivenAudio && onSpatialSoundRequested != null)
            {
                var data = new SpatialAudioData
                {
                    soundId = soundId,
                    position = position,
                    maxDistance = maxDistance,
                    volume = volume,
                    eventType = AudioEventType.Play
                };
                
                onSpatialSoundRequested.Raise(data);
            }
            else
            {
                // 直接実行
                PlaySpatialSound(soundId, position, maxDistance, volume);
            }
        }
        
        public void CreateMovingSound(string soundId, Transform source, float maxDistance = 50f)
        {
            if (!IsInitialized || source == null) return;
            
            var audioSource = GetAvailableAudioSource();
            if (audioSource == null)
            {
                EventLogger.LogWarning("[SpatialAudioService] No available audio sources");
                return;
            }
            
            // 音源をTransformに追従させる
            audioSource.transform.SetParent(source);
            audioSource.transform.localPosition = Vector3.zero;
            
            ConfigureAudioSource(audioSource, maxDistance);
            
            // AudioClipの読み込み（実際の実装では適切なリソース管理が必要）
            var clip = LoadAudioClip(soundId);
            if (clip != null)
            {
                audioSource.clip = clip;
                audioSource.loop = true;
                audioSource.Play();
                
                activeSources[soundId] = audioSource;
            }
        }
        
        public void SetAmbientSound(string soundId, float volume = 0.5f)
        {
            // アンビエントサウンドは2D音源として再生
            var audioSource = GetAvailableAudioSource();
            if (audioSource == null) return;
            
            audioSource.spatialBlend = 0f; // 2D sound
            audioSource.volume = volume;
            
            var clip = LoadAudioClip(soundId);
            if (clip != null)
            {
                audioSource.clip = clip;
                audioSource.loop = true;
                audioSource.Play();
                
                activeSources[$"ambient_{soundId}"] = audioSource;
            }
        }
        
        public void UpdateOcclusion(Vector3 listenerPosition, Vector3 sourcePosition, float occlusionLevel)
        {
            // レイキャストで遮蔽物を検出
            var direction = sourcePosition - listenerPosition;
            var distance = direction.magnitude;
            
            if (Physics.Raycast(listenerPosition, direction.normalized, out RaycastHit hit, distance))
            {
                // 遮蔽物がある場合、音量を減衰
                foreach (var kvp in activeSources)
                {
                    if (Vector3.Distance(kvp.Value.transform.position, sourcePosition) < 1f)
                    {
                        kvp.Value.volume *= (1f - occlusionLevel);
                        
                        // ローパスフィルタの適用（実装が必要）
                        if (kvp.Value.outputAudioMixerGroup != null)
                        {
                            // AudioMixerでローパスフィルタを制御
                        }
                    }
                }
            }
        }
        
        public void SetReverbZone(string zoneId, float reverbLevel)
        {
            reverbZones[zoneId] = Mathf.Clamp01(reverbLevel);
            
            // AudioMixerでリバーブレベルを制御
            if (spatialMixerGroup != null)
            {
                spatialMixerGroup.audioMixer.SetFloat($"Reverb_{zoneId}", reverbLevel);
            }
        }
        
        public void SetDopplerLevel(float level)
        {
            dopplerLevel = Mathf.Clamp(level, 0f, 5f);
            
            foreach (var audioSource in activeSources.Values)
            {
                audioSource.dopplerLevel = dopplerLevel;
            }
        }
        
        public void UpdateListenerPosition(Vector3 position, Vector3 forward)
        {
            if (mainListener != null)
            {
                mainListener.transform.position = position;
                mainListener.transform.forward = forward;
            }
        }
        
        #endregion
        
        #region Private Methods
        
        private void InitializeAudioSourcePool()
        {
            var poolContainer = new GameObject("SpatialAudioSourcePool");
            poolContainer.transform.SetParent(transform);
            
            for (int i = 0; i < maxAudioSources; i++)
            {
                GameObject sourceObj;
                
                if (audioSourcePrefab != null)
                {
                    sourceObj = Instantiate(audioSourcePrefab, poolContainer.transform);
                }
                else
                {
                    sourceObj = new GameObject($"AudioSource_{i}");
                    sourceObj.transform.SetParent(poolContainer.transform);
                    sourceObj.AddComponent<AudioSource>();
                }
                
                var audioSource = sourceObj.GetComponent<AudioSource>();
                ConfigureAudioSource(audioSource, defaultMaxDistance);
                
                sourceObj.SetActive(false);
                availableSources.Add(audioSource);
            }
        }
        
        private void ConfigureAudioSource(AudioSource source, float maxDistance)
        {
            source.spatialBlend = 1f; // 3D sound
            source.rolloffMode = AudioRolloffMode.Custom;
            
            if (volumeRolloffCurve != null)
            {
                source.SetCustomCurve(AudioSourceCurveType.CustomRolloff, volumeRolloffCurve);
            }
            
            source.maxDistance = maxDistance;
            source.dopplerLevel = dopplerLevel;
            
            if (spatialMixerGroup != null)
            {
                source.outputAudioMixerGroup = spatialMixerGroup;
            }
        }
        
        private AudioSource GetAvailableAudioSource()
        {
            if (availableSources.Count > 0)
            {
                var source = availableSources[0];
                availableSources.RemoveAt(0);
                source.gameObject.SetActive(true);
                return source;
            }
            
            // プールが空の場合、最も古いアクティブなソースを再利用
            if (activeSources.Count > 0)
            {
                var oldestKey = new List<string>(activeSources.Keys)[0];
                var source = activeSources[oldestKey];
                activeSources.Remove(oldestKey);
                source.Stop();
                return source;
            }
            
            return null;
        }
        
        private void ReturnAudioSource(AudioSource source)
        {
            if (source == null) return;
            
            source.Stop();
            source.clip = null;
            source.transform.SetParent(transform);
            source.transform.localPosition = Vector3.zero;
            source.gameObject.SetActive(false);
            
            availableSources.Add(source);
        }
        
        private void PlaySpatialSound(string soundId, Vector3 position, float maxDistance, float volume)
        {
            var audioSource = GetAvailableAudioSource();
            if (audioSource == null) return;
            
            audioSource.transform.position = position;
            ConfigureAudioSource(audioSource, maxDistance);
            audioSource.volume = volume;
            
            var clip = LoadAudioClip(soundId);
            if (clip != null)
            {
                audioSource.clip = clip;
                audioSource.Play();
                
                activeSources[soundId] = audioSource;
                
                // 再生終了後に自動的に返却
                StartCoroutine(ReturnSourceAfterPlay(audioSource, clip.length, soundId));
            }
        }
        
        private System.Collections.IEnumerator ReturnSourceAfterPlay(AudioSource source, float duration, string soundId)
        {
            yield return new WaitForSeconds(duration);
            
            if (activeSources.ContainsKey(soundId))
            {
                activeSources.Remove(soundId);
            }
            
            ReturnAudioSource(source);
        }
        
        private AudioClip LoadAudioClip(string soundId)
        {
            // 実際の実装では、適切なリソース管理システムから読み込む
            // ここでは仮実装
            return Resources.Load<AudioClip>($"Audio/{soundId}");
        }
        
        private void HandleSpatialSoundEvent(SpatialAudioData data)
        {
            switch (data.eventType)
            {
                case AudioEventType.Play:
                    PlaySpatialSound(data.soundId, data.position, data.maxDistance, data.volume);
                    break;
                    
                case AudioEventType.Stop:
                    if (activeSources.TryGetValue(data.soundId, out var source))
                    {
                        activeSources.Remove(data.soundId);
                        ReturnAudioSource(source);
                    }
                    break;
                    
                case AudioEventType.Update:
                    if (activeSources.TryGetValue(data.soundId, out var updateSource))
                    {
                        updateSource.transform.position = data.position;
                        updateSource.volume = data.volume;
                    }
                    break;
            }
        }
        
        private void SetupDefaultSettings()
        {
            // デフォルトのロールオフカーブを設定
            if (volumeRolloffCurve == null || volumeRolloffCurve.length == 0)
            {
                volumeRolloffCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
            }
        }
        
        private void CleanupAudioSources()
        {
            foreach (var source in activeSources.Values)
            {
                if (source != null)
                {
                    source.Stop();
                    Destroy(source.gameObject);
                }
            }
            
            foreach (var source in availableSources)
            {
                if (source != null)
                {
                    Destroy(source.gameObject);
                }
            }
            
            activeSources.Clear();
            availableSources.Clear();
        }
        
        #endregion
    }
}