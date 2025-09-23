using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;
using asterivo.Unity60.Core.Audio.Data;
using asterivo.Unity60.Core.Audio.Events;
using asterivo.Unity60.Core.Events;
// using asterivo.Unity60.Core.Shared;
using asterivo.Unity60.Core.Audio.Interfaces;
// using asterivo.Unity60.Core.Debug;


namespace asterivo.Unity60.Core.Audio
{
    /// <summary>
    /// 空間音響シスチE��の中央管琁E��ラス�E�レガシー�E�E    /// スチE��スゲーム用の高度な3D音響処琁E��拁E��E    /// 新しいSpatialAudioServiceへの移行を推奨
    /// </summary>
    [System.Obsolete("Use SpatialAudioService instead. This class will be removed in future versions.")]
    public class SpatialAudioManager : MonoBehaviour, ISpatialAudioService, IInitializable
    {
        // ✁ETask 3: Legacy Singleton警告シスチE���E�後方互換性のため�E�E        


        [Header("Audio Manager Settings")]
        [SerializeField] private AudioMixer mainMixer;
        [SerializeField] private int maxConcurrentSounds = AudioConstants.MAX_CONCURRENT_SOUNDS;
        [SerializeField] private LayerMask obstacleLayerMask = -1;
        
        [Header("Distance Attenuation")]
        [SerializeField] private AnimationCurve distanceAttenuationCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
        [SerializeField] private float globalHearingMultiplier = AudioConstants.DEFAULT_MASTER_VOLUME;
        
        [Header("Occlusion System")]
        [SerializeField] private bool enableOcclusion = true;
        [SerializeField] private float occlusionCheckInterval = AudioConstants.OCCLUSION_CHECK_INTERVAL;
        [SerializeField] private float maxOcclusionReduction = AudioConstants.MAX_OCCLUSION_REDUCTION;
        
        [Header("Environment Reverb")]
        [SerializeField] private AudioReverbZone[] reverbZones;
        
        [Header("Audio Categories")]
        [SerializeField] private AudioMixerGroup bgmMixerGroup;
        [SerializeField] private AudioMixerGroup ambientMixerGroup;
        [SerializeField] private AudioMixerGroup effectMixerGroup;
        [SerializeField] private AudioMixerGroup stealthMixerGroup;
        
        // オーチE��オソースプ�Eル
        private Queue<AudioSource> audioSourcePool = new Queue<AudioSource>();
        private List<AudioSource> activeAudioSources = new List<AudioSource>();
        
        // 聴取老E��通常はプレイヤー�E�E        private Transform listener;
        private AudioListener audioListener;
        
        // オクルージョンチェチE��用
        private Dictionary<AudioSource, float> occlusionValues = new Dictionary<AudioSource, float>();
        
        // ✁ESingleton パターンを完�E削除 - ServiceLocator専用実裁E        
        // IInitializable実裁E        public int Priority => 20; // 空間音響は基本オーチE��オシスチE��の後に初期匁E        public bool IsInitialized { get; private set; }
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            // ✁ETask 3: Legacy Singleton警告シスチE��用のinstance設宁E            
            
            DontDestroyOnLoad(gameObject);
            
            // ServiceLocatorに登録
            if (FeatureFlags.UseServiceLocator)
            {
                ServiceLocator.RegisterService<ISpatialAudioService>(this);
                
                if (FeatureFlags.EnableDebugLogging)
                {
                    EventLogger.LogStatic("[SpatialAudioManager] Registered to ServiceLocator as ISpatialAudioService");
                }
            }
            else
            {
                EventLogger.LogWarningStatic("[SpatialAudioManager] ServiceLocator is disabled - service not registered");
            }
            
            InitializeAudioSourcePool();
            FindAudioListener();
        }
        
        private void Start()
        {
            Initialize();
        }
        
        private void OnDestroy()
        {
            // ✁EServiceLocator専用実裁E�Eみ - Singletonパターン完�E削除
            // ServiceLocatorから登録解除
            if (FeatureFlags.UseServiceLocator)
            {
                ServiceLocator.UnregisterService<ISpatialAudioService>();
                
                if (FeatureFlags.EnableDebugLogging)
                {
                    EventLogger.LogStatic("[SpatialAudioManager] Unregistered from ServiceLocator");
                }
            }
        }
        
        #endregion
        
        #region IInitializable Implementation
        
        /// <summary>
        /// IInitializable実裁E- 空間音響シスチE��の初期匁E        /// </summary>
        public void Initialize()
        {
            if (IsInitialized) return;
            
            if (enableOcclusion)
            {
                InvokeRepeating(nameof(UpdateOcclusion), 0f, occlusionCheckInterval);
            }
            
            IsInitialized = true;
            
            if (FeatureFlags.EnableDebugLogging)
            {
                EventLogger.LogStatic("[SpatialAudioManager] Initialization complete (Legacy)");
            }
        }
        
        #endregion
        
        #region ISpatialAudioService Implementation
        
        /// <summary>
        /// 3D空間でサウンドを再生
        /// </summary>
        public void Play3DSound(string soundId, Vector3 position, float maxDistance = 50f, float volume = 1f)
        {
            if (!IsInitialized)
            {
                EventLogger.LogWarningStatic("[SpatialAudioManager] System not initialized");
                return;
            }
            
            // 既存�E機�Eを使用�E�EoundDataSOを作�Eして使用�E�E            var soundData = CreateDefaultSoundData(soundId);
            if (soundData != null)
            {
                PlaySoundAtPosition(soundData, position, volume);
            }
        }
        
        /// <summary>
        /// 移動する音源を作�E
        /// </summary>
        public void CreateMovingSound(string soundId, Transform source, float maxDistance = 50f)
        {
            if (!IsInitialized || source == null) return;
            
            // TODO: 移動する音源�E実裁E            EventLogger.LogStatic($"[SpatialAudioManager] Creating moving sound: {soundId}");
        }
        
        /// <summary>
        /// 環墁E��を設宁E        /// </summary>
        public void SetAmbientSound(string soundId, float volume = 0.5f)
        {
            if (!IsInitialized) return;
            
            // TODO: 環墁E��の実裁E            EventLogger.LogStatic($"[SpatialAudioManager] Setting ambient sound: {soundId}");
        }
        
        /// <summary>
        /// オクルージョン�E��E蔽�E�を更新
        /// </summary>
        public void UpdateOcclusion(Vector3 listenerPosition, Vector3 sourcePosition, float occlusionLevel)
        {
            // 既存�Eオクルージョン機�Eを使用して更新
            // 実裁E�E既存�EUpdateOcclusionメソチE��で行われてぁE��
        }
        
        /// <summary>
        /// リバ�Eブゾーンを設宁E        /// </summary>
        public void SetReverbZone(string zoneId, float reverbLevel)
        {
            if (!IsInitialized) return;
            
            // TODO: リバ�Eブゾーンの実裁E            EventLogger.LogStatic($"[SpatialAudioManager] Setting reverb zone: {zoneId}, level: {reverbLevel}");
        }
        
        /// <summary>
        /// ドップラー効果�E強度を設宁E        /// </summary>
        public void SetDopplerLevel(float level)
        {
            if (!IsInitialized) return;
            
            // TODO: ドップラーレベルの実裁E            EventLogger.LogStatic($"[SpatialAudioManager] Setting Doppler level: {level}");
        }
        
        /// <summary>
        /// リスナ�Eの位置を更新
        /// </summary>
        public void UpdateListenerPosition(Vector3 position, Vector3 forward)
        {
            if (audioListener != null)
            {
                audioListener.transform.position = position;
                audioListener.transform.forward = forward;
            }
        }
        
        /// <summary>
        /// チE��ォルト�ESoundDataSOを作�E
        /// </summary>
        private SoundDataSO CreateDefaultSoundData(string soundId)
        {
            // 簡略実裁E 実際はリソース管琁E��スチE��から取征E            var soundData = ScriptableObject.CreateInstance<SoundDataSO>();
            // TODO: soundIdからAudioClipを取得して設宁E            return soundData;
        }
        
        #endregion

        #region Public Interface
        
        /// <summary>
        /// 空間音響でサウンドを再生
        /// </summary>
        public AudioSource PlaySoundAtPosition(SoundDataSO soundData, Vector3 position, float volumeMultiplier = 1f)
        {
            if (soundData == null) return null;
            
            var audioSource = GetPooledAudioSource();
            if (audioSource == null) return null;
            
            SetupAudioSource(audioSource, soundData, position, volumeMultiplier);
            
            var clip = soundData.GetRandomClip();
            if (clip != null)
            {
                audioSource.clip = clip;
                audioSource.Play();
                
                StartCoroutine(ReturnToPoolWhenFinished(audioSource, clip.length));
            }
            
            return audioSource;
        }
        
        /// <summary>
        /// AudioEventDataを使用してサウンドを再生
        /// </summary>
        public AudioSource PlaySoundFromEvent(AudioEventData eventData, SoundDataSO soundData)
        {
            if (soundData == null) return null;
            
            var audioSource = PlayCategorizedSound(soundData, eventData.worldPosition, eventData.category, eventData.volume);
            
            if (audioSource != null)
            {
                // イベントデータの追加設定を適用
                audioSource.pitch = soundData.GetRandomPitch() * eventData.pitch;
                
                // 表面材質による調整
                ApplySurfaceModifications(audioSource, eventData, soundData);
                
                // 優先度に応じた�E琁E                ApplyPrioritySettings(audioSource, eventData);
            }
            
            return audioSource;
        }
        
        /// <summary>
        /// カチE��リ対応�E音響再生シスチE��
        /// </summary>
        public AudioSource PlayCategorizedSound(SoundDataSO soundData, Vector3 position, 
            AudioCategory category, float volumeMultiplier = 1f)
        {
            if (soundData == null) return null;
            
            var audioSource = GetPooledAudioSource();
            if (audioSource == null) return null;
            
            // カチE��リに応じたミキサーグループ設宁E            SetupCategorySettings(audioSource, category, soundData);
            SetupAudioSource(audioSource, soundData, position, volumeMultiplier);
            
            var clip = soundData.GetRandomClip();
            if (clip != null)
            {
                audioSource.clip = clip;
                audioSource.Play();
                
                StartCoroutine(ReturnToPoolWhenFinished(audioSource, clip.length));
            }
            
            return audioSource;
        }
        
        /// <summary>
        /// カチE��リに応じた音響設宁E        /// </summary>
        private void SetupCategorySettings(AudioSource audioSource, AudioCategory category, SoundDataSO soundData)
        {
            switch (category)
            {
                case AudioCategory.BGM:
                    audioSource.outputAudioMixerGroup = bgmMixerGroup;
                    audioSource.spatialBlend = 0f; // BGMは2D音響
                    audioSource.loop = true; // BGMは基本皁E��ルーチE                    break;
                    
                case AudioCategory.Ambient:
                    audioSource.outputAudioMixerGroup = ambientMixerGroup;
                    audioSource.spatialBlend = soundData.Is3D ? soundData.SpatialBlend : 0f;
                    break;
                    
                case AudioCategory.Effect:
                    audioSource.outputAudioMixerGroup = effectMixerGroup;
                    audioSource.spatialBlend = soundData.Is3D ? soundData.SpatialBlend : 0f;
                    break;
                    
                case AudioCategory.Stealth:
                    audioSource.outputAudioMixerGroup = stealthMixerGroup;
                    audioSource.spatialBlend = soundData.Is3D ? soundData.SpatialBlend : 1f;
                    break;
                    
                case AudioCategory.UI:
                    // UIはミキサーグループを使わなぁE��合が多い
                    audioSource.spatialBlend = 0f; // UI音響は常に2D
                    break;
            }
        }
        
        /// <summary>
        /// 優先度設定�E適用
        /// </summary>
        private void ApplyPrioritySettings(AudioSource audioSource, AudioEventData eventData)
        {
            // Unity AudioSource の priority は 0-256 の篁E���E�低い値ほど高優先度�E�E            int unityPriority = Mathf.RoundToInt((1f - eventData.priority) * 256f);
            audioSource.priority = Mathf.Clamp(unityPriority, 0, 256);
            
            // レイヤー優先度による追加調整
            if (eventData.layerPriority > 50)
            {
                audioSource.priority = Mathf.Max(0, audioSource.priority - 50);
            }
        }
        
        /// <summary>
        /// 音源間の距離に基づく音量計箁E        /// </summary>
        public float CalculateVolumeAtDistance(float distance, float maxHearingRadius)
        {
            if (distance <= 0f) return 1f;
            if (distance >= maxHearingRadius) return 0f;
            
            float normalizedDistance = distance / maxHearingRadius;
            return distanceAttenuationCurve.Evaluate(normalizedDistance) * globalHearingMultiplier;
        }
        
        /// <summary>
        /// 音源が聞こえるかどぁE��を判宁E        /// </summary>
        public bool IsAudibleAtPosition(Vector3 soundPosition, float hearingRadius, Vector3 listenerPosition)
        {
            float distance = Vector3.Distance(soundPosition, listenerPosition);
            float volume = CalculateVolumeAtDistance(distance, hearingRadius);
            
            // オクルージョンも老E�E
            if (enableOcclusion)
            {
                float occlusion = CalculateOcclusion(soundPosition, listenerPosition);
                volume *= (1f - occlusion);
            }
            
            return volume > 0.01f; // 最小閾値
        }
        
        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// オーチE��オソースプ�Eルの初期匁E        /// </summary>
        private void InitializeAudioSourcePool()
        {
            for (int i = 0; i < maxConcurrentSounds; i++)
            {
                var go = new GameObject($"PooledAudioSource_{i}");
                go.transform.SetParent(transform);
                
                var audioSource = go.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 1f; // 3D音響
                
                audioSourcePool.Enqueue(audioSource);
            }
        }
        
        /// <summary>
        /// AudioListenerを検索
        /// </summary>
        private void FindAudioListener()
        {
            audioListener = FindFirstObjectByType<AudioListener>();
            if (audioListener != null)
            {
                listener = audioListener.transform;
            }
        }
        
        /// <summary>
        /// プ�EルからオーチE��オソースを取征E        /// </summary>
        private AudioSource GetPooledAudioSource()
        {
            if (audioSourcePool.Count > 0)
            {
                var audioSource = audioSourcePool.Dequeue();
                activeAudioSources.Add(audioSource);
                return audioSource;
            }
            
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            ProjectDebug.LogWarning("[SpatialAudioManager] オーチE��オソースプ�Eルが枯渁E��ました");
#endif
            return null;
        }
        
        /// <summary>
        /// オーチE��オソースを�Eールに返却
        /// </summary>
        private void ReturnToPool(AudioSource audioSource)
        {
            if (audioSource == null) return;
            
            audioSource.Stop();
            audioSource.clip = null;
            
            activeAudioSources.Remove(audioSource);
            audioSourcePool.Enqueue(audioSource);
            
            occlusionValues.Remove(audioSource);
        }
        
        /// <summary>
        /// オーチE��オソースの設宁E        /// </summary>
        private void SetupAudioSource(AudioSource audioSource, SoundDataSO soundData, Vector3 position, float volumeMultiplier)
        {
            audioSource.transform.position = position;
            audioSource.volume = soundData.GetRandomVolume() * volumeMultiplier;
            audioSource.pitch = soundData.GetRandomPitch();
            
            if (soundData.Is3D)
            {
                audioSource.spatialBlend = soundData.SpatialBlend;
                audioSource.minDistance = soundData.MinDistance;
                audioSource.maxDistance = soundData.MaxDistance;
                audioSource.rolloffMode = soundData.RolloffMode;
            }
            
            if (soundData.MixerGroup != null)
            {
                audioSource.outputAudioMixerGroup = soundData.MixerGroup;
            }
        }
        
        /// <summary>
        /// 表面材質による音響効果を適用
        /// </summary>
        private void ApplySurfaceModifications(AudioSource audioSource, AudioEventData eventData, SoundDataSO soundData)
        {
            if (eventData.surfaceType != SurfaceMaterial.Default)
            {
                float surfaceVolumeMultiplier = soundData.GetVolumeMultiplierForSurface(eventData.surfaceType);
                audioSource.volume *= surfaceVolumeMultiplier;
            }
        }
        
        /// <summary>
        /// オクルージョン�E��E蔽�E��E計箁E        /// </summary>
        private float CalculateOcclusion(Vector3 soundPosition, Vector3 listenerPosition)
        {
            if (listener == null) return 0f;
            
            Vector3 direction = listenerPosition - soundPosition;
            float distance = direction.magnitude;
            
            if (Physics.Raycast(soundPosition, direction.normalized, out RaycastHit hit, distance, obstacleLayerMask))
            {
                // 障害物までの距離の割合で遮蔽度を計箁E                float occlusionFactor = hit.distance / distance;
                return Mathf.Lerp(maxOcclusionReduction, 0f, occlusionFactor);
            }
            
            return 0f;
        }
        
        /// <summary>
        /// 全アクチE��ブな音源�Eオクルージョンを更新
        /// </summary>
        private void UpdateOcclusion()
        {
            if (listener == null) return;
            
            foreach (var audioSource in activeAudioSources)
            {
                if (audioSource != null && audioSource.isPlaying)
                {
                    float occlusion = CalculateOcclusion(audioSource.transform.position, listener.position);
                    occlusionValues[audioSource] = occlusion;
                    
                    // 音量にオクルージョンを適用
                    // 注愁E ここでは簡略化�Eため直接音量を変更してぁE��すが、E                    // 実際にはLowPassFilterなどを使用する方が�E然でぁE                }
            }
        }
        
        /// <summary>
        /// 再生終亁E��にプ�Eルに返却するコルーチン
        /// </summary>
        private System.Collections.IEnumerator ReturnToPoolWhenFinished(AudioSource audioSource, float clipLength)
        {
            yield return new WaitForSeconds(clipLength + 0.1f);
            ReturnToPool(audioSource);
        }
        
        #endregion
        
        #region Editor Helpers
        
        #if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (listener == null) return;
            
            // アクチE��ブな音源�E可視化
            Gizmos.color = Color.yellow;
            foreach (var audioSource in activeAudioSources)
            {
                if (audioSource != null && audioSource.isPlaying)
                {
                    Gizmos.DrawWireSphere(audioSource.transform.position, 2f);
                    Gizmos.DrawLine(listener.position, audioSource.transform.position);
                }
            }
        }
        #endif
        
        #endregion
    }
}