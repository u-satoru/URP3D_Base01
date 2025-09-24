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
    /// 遨ｺ髢馴浹髻ｿ繧ｷ繧ｹ繝・Β縺ｮ荳ｭ螟ｮ邂｡逅・け繝ｩ繧ｹ・医Ξ繧ｬ繧ｷ繝ｼ・・    /// 繧ｹ繝・Ν繧ｹ繧ｲ繝ｼ繝逕ｨ縺ｮ鬮伜ｺｦ縺ｪ3D髻ｳ髻ｿ蜃ｦ逅・ｒ諡・ｽ・    /// 譁ｰ縺励＞SpatialAudioService縺ｸ縺ｮ遘ｻ陦後ｒ謗ｨ螂ｨ
    /// </summary>
    [System.Obsolete("Use SpatialAudioService instead. This class will be removed in future versions.")]
    public class SpatialAudioManager : MonoBehaviour, ISpatialAudioService, IInitializable
    {
        // 笨・Task 3: Legacy Singleton隴ｦ蜻翫す繧ｹ繝・Β・亥ｾ梧婿莠呈鋤諤ｧ縺ｮ縺溘ａ・・        


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
        
        // 繧ｪ繝ｼ繝・ぅ繧ｪ繧ｽ繝ｼ繧ｹ繝励・繝ｫ
        private Queue<AudioSource> audioSourcePool = new Queue<AudioSource>();
        private List<AudioSource> activeAudioSources = new List<AudioSource>();
        
        // 閨ｴ蜿冶・ｼ磯壼ｸｸ縺ｯ繝励Ξ繧､繝､繝ｼ・・        private Transform listener;
        private AudioListener audioListener;
        
        // 繧ｪ繧ｯ繝ｫ繝ｼ繧ｸ繝ｧ繝ｳ繝√ぉ繝・け逕ｨ
        private Dictionary<AudioSource, float> occlusionValues = new Dictionary<AudioSource, float>();
        
        // 笨・Singleton 繝代ち繝ｼ繝ｳ繧貞ｮ悟・蜑企勁 - ServiceLocator蟆ら畑螳溯｣・        
        // IInitializable螳溯｣・        public int Priority => 20; // 遨ｺ髢馴浹髻ｿ縺ｯ蝓ｺ譛ｬ繧ｪ繝ｼ繝・ぅ繧ｪ繧ｷ繧ｹ繝・Β縺ｮ蠕後↓蛻晄悄蛹・        public bool IsInitialized { get; private set; }
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            // 笨・Task 3: Legacy Singleton隴ｦ蜻翫す繧ｹ繝・Β逕ｨ縺ｮinstance險ｭ螳・            
            
            DontDestroyOnLoad(gameObject);
            
            // ServiceLocator縺ｫ逋ｻ骭ｲ
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
            // 笨・ServiceLocator蟆ら畑螳溯｣・・縺ｿ - Singleton繝代ち繝ｼ繝ｳ螳悟・蜑企勁
            // ServiceLocator縺九ｉ逋ｻ骭ｲ隗｣髯､
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
        /// IInitializable螳溯｣・- 遨ｺ髢馴浹髻ｿ繧ｷ繧ｹ繝・Β縺ｮ蛻晄悄蛹・        /// </summary>
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
        /// 3D遨ｺ髢薙〒繧ｵ繧ｦ繝ｳ繝峨ｒ蜀咲函
        /// </summary>
        public void Play3DSound(string soundId, Vector3 position, float maxDistance = 50f, float volume = 1f)
        {
            if (!IsInitialized)
            {
                EventLogger.LogWarningStatic("[SpatialAudioManager] System not initialized");
                return;
            }
            
            // 譌｢蟄倥・讖溯・繧剃ｽｿ逕ｨ・・oundDataSO繧剃ｽ懈・縺励※菴ｿ逕ｨ・・            var soundData = CreateDefaultSoundData(soundId);
            if (soundData != null)
            {
                PlaySoundAtPosition(soundData, position, volume);
            }
        }
        
        /// <summary>
        /// 遘ｻ蜍輔☆繧矩浹貅舌ｒ菴懈・
        /// </summary>
        public void CreateMovingSound(string soundId, Transform source, float maxDistance = 50f)
        {
            if (!IsInitialized || source == null) return;
            
            // TODO: 遘ｻ蜍輔☆繧矩浹貅舌・螳溯｣・            EventLogger.LogStatic($"[SpatialAudioManager] Creating moving sound: {soundId}");
        }
        
        /// <summary>
        /// 迺ｰ蠅・浹繧定ｨｭ螳・        /// </summary>
        public void SetAmbientSound(string soundId, float volume = 0.5f)
        {
            if (!IsInitialized) return;
            
            // TODO: 迺ｰ蠅・浹縺ｮ螳溯｣・            EventLogger.LogStatic($"[SpatialAudioManager] Setting ambient sound: {soundId}");
        }
        
        /// <summary>
        /// 繧ｪ繧ｯ繝ｫ繝ｼ繧ｸ繝ｧ繝ｳ・磯・阡ｽ・峨ｒ譖ｴ譁ｰ
        /// </summary>
        public void UpdateOcclusion(Vector3 listenerPosition, Vector3 sourcePosition, float occlusionLevel)
        {
            // 譌｢蟄倥・繧ｪ繧ｯ繝ｫ繝ｼ繧ｸ繝ｧ繝ｳ讖溯・繧剃ｽｿ逕ｨ縺励※譖ｴ譁ｰ
            // 螳溯｣・・譌｢蟄倥・UpdateOcclusion繝｡繧ｽ繝・ラ縺ｧ陦後ｏ繧後※縺・ｋ
        }
        
        /// <summary>
        /// 繝ｪ繝舌・繝悶だ繝ｼ繝ｳ繧定ｨｭ螳・        /// </summary>
        public void SetReverbZone(string zoneId, float reverbLevel)
        {
            if (!IsInitialized) return;
            
            // TODO: 繝ｪ繝舌・繝悶だ繝ｼ繝ｳ縺ｮ螳溯｣・            EventLogger.LogStatic($"[SpatialAudioManager] Setting reverb zone: {zoneId}, level: {reverbLevel}");
        }
        
        /// <summary>
        /// 繝峨ャ繝励Λ繝ｼ蜉ｹ譫懊・蠑ｷ蠎ｦ繧定ｨｭ螳・        /// </summary>
        public void SetDopplerLevel(float level)
        {
            if (!IsInitialized) return;
            
            // TODO: 繝峨ャ繝励Λ繝ｼ繝ｬ繝吶Ν縺ｮ螳溯｣・            EventLogger.LogStatic($"[SpatialAudioManager] Setting Doppler level: {level}");
        }
        
        /// <summary>
        /// 繝ｪ繧ｹ繝翫・縺ｮ菴咲ｽｮ繧呈峩譁ｰ
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
        /// 繝・ヵ繧ｩ繝ｫ繝医・SoundDataSO繧剃ｽ懈・
        /// </summary>
        private SoundDataSO CreateDefaultSoundData(string soundId)
        {
            // 邁｡逡･螳溯｣・ 螳滄圀縺ｯ繝ｪ繧ｽ繝ｼ繧ｹ邂｡逅・す繧ｹ繝・Β縺九ｉ蜿門ｾ・            var soundData = ScriptableObject.CreateInstance<SoundDataSO>();
            // TODO: soundId縺九ｉAudioClip繧貞叙蠕励＠縺ｦ險ｭ螳・            return soundData;
        }
        
        #endregion

        #region Public Interface
        
        /// <summary>
        /// 遨ｺ髢馴浹髻ｿ縺ｧ繧ｵ繧ｦ繝ｳ繝峨ｒ蜀咲函
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
        /// AudioEventData繧剃ｽｿ逕ｨ縺励※繧ｵ繧ｦ繝ｳ繝峨ｒ蜀咲函
        /// </summary>
        public AudioSource PlaySoundFromEvent(AudioEventData eventData, SoundDataSO soundData)
        {
            if (soundData == null) return null;
            
            var audioSource = PlayCategorizedSound(soundData, eventData.worldPosition, eventData.category, eventData.volume);
            
            if (audioSource != null)
            {
                // 繧､繝吶Φ繝医ョ繝ｼ繧ｿ縺ｮ霑ｽ蜉險ｭ螳壹ｒ驕ｩ逕ｨ
                audioSource.pitch = soundData.GetRandomPitch() * eventData.pitch;
                
                // 陦ｨ髱｢譚占ｳｪ縺ｫ繧医ｋ隱ｿ謨ｴ
                ApplySurfaceModifications(audioSource, eventData, soundData);
                
                // 蜆ｪ蜈亥ｺｦ縺ｫ蠢懊§縺溷・逅・                ApplyPrioritySettings(audioSource, eventData);
            }
            
            return audioSource;
        }
        
        /// <summary>
        /// 繧ｫ繝・ざ繝ｪ蟇ｾ蠢懊・髻ｳ髻ｿ蜀咲函繧ｷ繧ｹ繝・Β
        /// </summary>
        public AudioSource PlayCategorizedSound(SoundDataSO soundData, Vector3 position, 
            AudioCategory category, float volumeMultiplier = 1f)
        {
            if (soundData == null) return null;
            
            var audioSource = GetPooledAudioSource();
            if (audioSource == null) return null;
            
            // 繧ｫ繝・ざ繝ｪ縺ｫ蠢懊§縺溘Α繧ｭ繧ｵ繝ｼ繧ｰ繝ｫ繝ｼ繝苓ｨｭ螳・            SetupCategorySettings(audioSource, category, soundData);
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
        /// 繧ｫ繝・ざ繝ｪ縺ｫ蠢懊§縺滄浹髻ｿ險ｭ螳・        /// </summary>
        private void SetupCategorySettings(AudioSource audioSource, AudioCategory category, SoundDataSO soundData)
        {
            switch (category)
            {
                case AudioCategory.BGM:
                    audioSource.outputAudioMixerGroup = bgmMixerGroup;
                    audioSource.spatialBlend = 0f; // BGM縺ｯ2D髻ｳ髻ｿ
                    audioSource.loop = true; // BGM縺ｯ蝓ｺ譛ｬ逧・↓繝ｫ繝ｼ繝・                    break;
                    
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
                    // UI縺ｯ繝溘く繧ｵ繝ｼ繧ｰ繝ｫ繝ｼ繝励ｒ菴ｿ繧上↑縺・ｴ蜷医′螟壹＞
                    audioSource.spatialBlend = 0f; // UI髻ｳ髻ｿ縺ｯ蟶ｸ縺ｫ2D
                    break;
            }
        }
        
        /// <summary>
        /// 蜆ｪ蜈亥ｺｦ險ｭ螳壹・驕ｩ逕ｨ
        /// </summary>
        private void ApplyPrioritySettings(AudioSource audioSource, AudioEventData eventData)
        {
            // Unity AudioSource 縺ｮ priority 縺ｯ 0-256 縺ｮ遽・峇・井ｽ弱＞蛟､縺ｻ縺ｩ鬮伜━蜈亥ｺｦ・・            int unityPriority = Mathf.RoundToInt((1f - eventData.priority) * 256f);
            audioSource.priority = Mathf.Clamp(unityPriority, 0, 256);
            
            // 繝ｬ繧､繝､繝ｼ蜆ｪ蜈亥ｺｦ縺ｫ繧医ｋ霑ｽ蜉隱ｿ謨ｴ
            if (eventData.layerPriority > 50)
            {
                audioSource.priority = Mathf.Max(0, audioSource.priority - 50);
            }
        }
        
        /// <summary>
        /// 髻ｳ貅宣俣縺ｮ霍晞屬縺ｫ蝓ｺ縺･縺城浹驥剰ｨ育ｮ・        /// </summary>
        public float CalculateVolumeAtDistance(float distance, float maxHearingRadius)
        {
            if (distance <= 0f) return 1f;
            if (distance >= maxHearingRadius) return 0f;
            
            float normalizedDistance = distance / maxHearingRadius;
            return distanceAttenuationCurve.Evaluate(normalizedDistance) * globalHearingMultiplier;
        }
        
        /// <summary>
        /// 髻ｳ貅舌′閨槭％縺医ｋ縺九←縺・°繧貞愛螳・        /// </summary>
        public bool IsAudibleAtPosition(Vector3 soundPosition, float hearingRadius, Vector3 listenerPosition)
        {
            float distance = Vector3.Distance(soundPosition, listenerPosition);
            float volume = CalculateVolumeAtDistance(distance, hearingRadius);
            
            // 繧ｪ繧ｯ繝ｫ繝ｼ繧ｸ繝ｧ繝ｳ繧り・・
            if (enableOcclusion)
            {
                float occlusion = CalculateOcclusion(soundPosition, listenerPosition);
                volume *= (1f - occlusion);
            }
            
            return volume > 0.01f; // 譛蟆城明蛟､
        }
        
        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// 繧ｪ繝ｼ繝・ぅ繧ｪ繧ｽ繝ｼ繧ｹ繝励・繝ｫ縺ｮ蛻晄悄蛹・        /// </summary>
        private void InitializeAudioSourcePool()
        {
            for (int i = 0; i < maxConcurrentSounds; i++)
            {
                var go = new GameObject($"PooledAudioSource_{i}");
                go.transform.SetParent(transform);
                
                var audioSource = go.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 1f; // 3D髻ｳ髻ｿ
                
                audioSourcePool.Enqueue(audioSource);
            }
        }
        
        /// <summary>
        /// AudioListener繧呈､懃ｴ｢
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
        /// 繝励・繝ｫ縺九ｉ繧ｪ繝ｼ繝・ぅ繧ｪ繧ｽ繝ｼ繧ｹ繧貞叙蠕・        /// </summary>
        private AudioSource GetPooledAudioSource()
        {
            if (audioSourcePool.Count > 0)
            {
                var audioSource = audioSourcePool.Dequeue();
                activeAudioSources.Add(audioSource);
                return audioSource;
            }
            
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            ProjectDebug.LogWarning("[SpatialAudioManager] 繧ｪ繝ｼ繝・ぅ繧ｪ繧ｽ繝ｼ繧ｹ繝励・繝ｫ縺梧椡貂・＠縺ｾ縺励◆");
#endif
            return null;
        }
        
        /// <summary>
        /// 繧ｪ繝ｼ繝・ぅ繧ｪ繧ｽ繝ｼ繧ｹ繧偵・繝ｼ繝ｫ縺ｫ霑泌唆
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
        /// 繧ｪ繝ｼ繝・ぅ繧ｪ繧ｽ繝ｼ繧ｹ縺ｮ險ｭ螳・        /// </summary>
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
        /// 陦ｨ髱｢譚占ｳｪ縺ｫ繧医ｋ髻ｳ髻ｿ蜉ｹ譫懊ｒ驕ｩ逕ｨ
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
        /// 繧ｪ繧ｯ繝ｫ繝ｼ繧ｸ繝ｧ繝ｳ・磯・阡ｽ・峨・險育ｮ・        /// </summary>
        private float CalculateOcclusion(Vector3 soundPosition, Vector3 listenerPosition)
        {
            if (listener == null) return 0f;
            
            Vector3 direction = listenerPosition - soundPosition;
            float distance = direction.magnitude;
            
            if (Physics.Raycast(soundPosition, direction.normalized, out RaycastHit hit, distance, obstacleLayerMask))
            {
                // 髫懷ｮｳ迚ｩ縺ｾ縺ｧ縺ｮ霍晞屬縺ｮ蜑ｲ蜷医〒驕ｮ阡ｽ蠎ｦ繧定ｨ育ｮ・                float occlusionFactor = hit.distance / distance;
                return Mathf.Lerp(maxOcclusionReduction, 0f, occlusionFactor);
            }
            
            return 0f;
        }
        
        /// <summary>
        /// 蜈ｨ繧｢繧ｯ繝・ぅ繝悶↑髻ｳ貅舌・繧ｪ繧ｯ繝ｫ繝ｼ繧ｸ繝ｧ繝ｳ繧呈峩譁ｰ
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
                    
                    // 髻ｳ驥上↓繧ｪ繧ｯ繝ｫ繝ｼ繧ｸ繝ｧ繝ｳ繧帝←逕ｨ
                    // 豕ｨ諢・ 縺薙％縺ｧ縺ｯ邁｡逡･蛹悶・縺溘ａ逶ｴ謗･髻ｳ驥上ｒ螟画峩縺励※縺・∪縺吶′縲・                    // 螳滄圀縺ｫ縺ｯLowPassFilter縺ｪ縺ｩ繧剃ｽｿ逕ｨ縺吶ｋ譁ｹ縺瑚・辟ｶ縺ｧ縺・                }
            }
        }
        
        /// <summary>
        /// 蜀咲函邨ゆｺ・ｾ後↓繝励・繝ｫ縺ｫ霑泌唆縺吶ｋ繧ｳ繝ｫ繝ｼ繝√Φ
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
            
            // 繧｢繧ｯ繝・ぅ繝悶↑髻ｳ貅舌・蜿ｯ隕門喧
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