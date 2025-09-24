using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using System.Collections.Generic;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Audio.Events;
// using asterivo.Unity60.Core.Debug;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Audio.Interfaces;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Core.Audio.Services
{
    /// <summary>
    /// 遨ｺ髢馴浹髻ｿ繧ｵ繝ｼ繝薙せ・医う繝吶Φ繝磯ｧ・虚蝙具ｼ・    /// Singleton繝代ち繝ｼ繝ｳ繧剃ｽｿ繧上★縲√う繝吶Φ繝育ｵ檎罰縺ｧ蛻ｶ蠕｡
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
        public int Priority => 20; // 遨ｺ髢馴浹髻ｿ縺ｯ蝓ｺ譛ｬ繧ｪ繝ｼ繝・ぅ繧ｪ繧ｷ繧ｹ繝・Β縺ｮ蠕後↓蛻晄悄蛹・        public bool IsInitialized { get; private set; }
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            // ServiceLocator縺ｫ逋ｻ骭ｲ
            if (FeatureFlags.UseServiceLocator)
            {
                ServiceLocator.RegisterService<ISpatialAudioService>(this);
                
                if (FeatureFlags.EnableDebugLogging)
                {
                    EventLogger.LogStatic("[SpatialAudioService] Registered to ServiceLocator");
                }
            }
            
            InitializeAudioSourcePool();
        }
        
        private void Start()
        {
            Initialize();
            
            // 繧､繝吶Φ繝医Μ繧ｹ繝翫・縺ｮ險ｭ螳・            if (onSpatialSoundRequested != null && FeatureFlags.UseEventDrivenAudio)
            {
                // SpatialAudioEvent逕ｨ縺ｮ繝ｪ繧ｹ繝翫・繧定ｿｽ蜉
                // TODO: SpatialAudioEvent蟆ら畑縺ｮ繝ｪ繧ｹ繝翫・縺悟ｿ・ｦ・                // var listener = gameObject.AddComponent<SpatialAudioEventListener>();
                // listener.GameEvent = onSpatialSoundRequested;
                // listener.OnEventRaised.AddListener(HandleSpatialSoundEvent);
                
                // 繧､繝吶Φ繝医Μ繧ｹ繝翫・縺ｨ縺励※逋ｻ骭ｲ縺吶ｋ縺ｫ縺ｯ蛻･縺ｮ譁ｹ豕輔′蠢・ｦ・                // TODO: GenericGameEventListener<SpatialAudioData>繧剃ｽｿ逕ｨ縺吶ｋ縺九・                // 縺ｾ縺溘・IGameEventListener<SpatialAudioData>繧貞ｮ溯｣・☆繧句ｿ・ｦ√′縺ゅｋ
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
                ServiceLocator.GetService<IEventLogger>().LogWarning("[SpatialAudioService] No AudioListener found in scene");
            }
            
            SetupDefaultSettings();
            
            if (onAudioSystemInitialized != null)
            {
                onAudioSystemInitialized.Raise();
            }
            
            IsInitialized = true;
            
            if (FeatureFlags.EnableDebugLogging)
            {
                EventLogger.LogStatic("[SpatialAudioService] Initialization complete");
            }
        }
        
        #endregion
        
        #region ISpatialAudioService Implementation
        
        public void Play3DSound(string soundId, Vector3 position, float maxDistance = 50f, float volume = 1f)
        {
            if (!IsInitialized)
            {
                ServiceLocator.GetService<IEventLogger>().LogWarning("[SpatialAudioService] System not initialized");
                return;
            }
            
            // 繧､繝吶Φ繝磯ｧ・虚繝｢繝ｼ繝峨・蝣ｴ蜷・            if (FeatureFlags.UseEventDrivenAudio && onSpatialSoundRequested != null)
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
                // 逶ｴ謗･螳溯｡・                PlaySpatialSound(soundId, position, maxDistance, volume);
            }
        }
        
        public void CreateMovingSound(string soundId, Transform source, float maxDistance = 50f)
        {
            if (!IsInitialized || source == null) return;
            
            var audioSource = GetAvailableAudioSource();
            if (audioSource == null)
            {
                ServiceLocator.GetService<IEventLogger>().LogWarning("[SpatialAudioService] No available audio sources");
                return;
            }
            
            // 髻ｳ貅舌ｒTransform縺ｫ霑ｽ蠕薙＆縺帙ｋ
            audioSource.transform.SetParent(source);
            audioSource.transform.localPosition = Vector3.zero;
            
            ConfigureAudioSource(audioSource, maxDistance);
            
            // AudioClip縺ｮ隱ｭ縺ｿ霎ｼ縺ｿ・亥ｮ滄圀縺ｮ螳溯｣・〒縺ｯ驕ｩ蛻・↑繝ｪ繧ｽ繝ｼ繧ｹ邂｡逅・′蠢・ｦ・ｼ・            var clip = LoadAudioClip(soundId);
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
            // 繧｢繝ｳ繝薙お繝ｳ繝医し繧ｦ繝ｳ繝峨・2D髻ｳ貅舌→縺励※蜀咲函
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
            // 繝ｬ繧､繧ｭ繝｣繧ｹ繝医〒驕ｮ阡ｽ迚ｩ繧呈､懷・
            var direction = sourcePosition - listenerPosition;
            var distance = direction.magnitude;
            
            if (Physics.Raycast(listenerPosition, direction.normalized, out RaycastHit hit, distance))
            {
                // 驕ｮ阡ｽ迚ｩ縺後≠繧句ｴ蜷医・浹驥上ｒ貂幄｡ｰ
                foreach (var kvp in activeSources)
                {
                    if (Vector3.Distance(kvp.Value.transform.position, sourcePosition) < 1f)
                    {
                        kvp.Value.volume *= (1f - occlusionLevel);
                        
                        // 繝ｭ繝ｼ繝代せ繝輔ぅ繝ｫ繧ｿ縺ｮ驕ｩ逕ｨ・亥ｮ溯｣・′蠢・ｦ・ｼ・                        if (kvp.Value.outputAudioMixerGroup != null)
                        {
                            // AudioMixer縺ｧ繝ｭ繝ｼ繝代せ繝輔ぅ繝ｫ繧ｿ繧貞宛蠕｡
                        }
                    }
                }
            }
        }
        
        public void SetReverbZone(string zoneId, float reverbLevel)
        {
            reverbZones[zoneId] = Mathf.Clamp01(reverbLevel);
            
            // AudioMixer縺ｧ繝ｪ繝舌・繝悶Ξ繝吶Ν繧貞宛蠕｡
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
            
            // 繝励・繝ｫ縺檎ｩｺ縺ｮ蝣ｴ蜷医∵怙繧ょ商縺・い繧ｯ繝・ぅ繝悶↑繧ｽ繝ｼ繧ｹ繧貞・蛻ｩ逕ｨ
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
                
                // 蜀咲函邨ゆｺ・ｾ後↓閾ｪ蜍慕噪縺ｫ霑泌唆
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
            // 螳滄圀縺ｮ螳溯｣・〒縺ｯ縲・←蛻・↑繝ｪ繧ｽ繝ｼ繧ｹ邂｡逅・す繧ｹ繝・Β縺九ｉ隱ｭ縺ｿ霎ｼ繧
            // 縺薙％縺ｧ縺ｯ莉ｮ螳溯｣・            return Resources.Load<AudioClip>($"Audio/{soundId}");
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
            // 繝・ヵ繧ｩ繝ｫ繝医・繝ｭ繝ｼ繝ｫ繧ｪ繝輔き繝ｼ繝悶ｒ險ｭ螳・            if (volumeRolloffCurve == null || volumeRolloffCurve.length == 0)
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