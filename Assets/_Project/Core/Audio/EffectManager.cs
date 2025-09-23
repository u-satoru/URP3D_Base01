using UnityEngine;
using UnityEngine.Audio;
using System.Linq;
using System.Collections.Generic;
using asterivo.Unity60.Core.Audio.Data;
using asterivo.Unity60.Core.Audio.Events;
// using asterivo.Unity60.Core.Debug;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Audio.Interfaces;


namespace asterivo.Unity60.Core.Audio
{
    /// <summary>
    /// 繧ｨ繝輔ぉ繧ｯ繝育ｨｮ蛻･縺ｮ蛻玲嫌蝙・    /// </summary>
    public enum EffectType
    {
        UI,
        Interaction, 
        Combat,
        Stealth
    }
    /// <summary>
    /// 蜉ｹ譫憺浹繧ｷ繧ｹ繝・Β縺ｮ邂｡逅・け繝ｩ繧ｹ
    /// 荳闊ｬ逧・↑蜉ｹ譫憺浹縺ｨ繧ｹ繝・Ν繧ｹ繧ｲ繝ｼ繝逕ｨ蜉ｹ譫憺浹縺ｮ邨ｱ蜷育ｮ｡逅・    /// ServiceLocator蟇ｾ蠢懃沿
    /// </summary>
    public class EffectManager : MonoBehaviour, IEffectService, IInitializable
    {
        [Header("Effect Manager Settings")]
        [SerializeField] private int maxConcurrentEffects = 16;
        [SerializeField] private AudioMixerGroup effectMixerGroup;
        
        [Header("Effect Categories")]
        [SerializeField] private bool enableUIEffects = true;
        [SerializeField] private bool enableInteractionEffects = true;
        [SerializeField] private bool enableCombatEffects = true;
        [SerializeField] private bool enableStealthEffects = true;
        
        [Header("Volume Control")]
        [SerializeField] private float uiEffectVolume = 0.8f;
        [SerializeField] private float interactionEffectVolume = 1.0f;
        [SerializeField] private float combatEffectVolume = 1.2f;
        [SerializeField] private float stealthEffectVolume = 0.7f;
        
        [Header("Priority Settings")]
        [SerializeField] private int uiEffectPriority = 64;
        [SerializeField] private int interactionEffectPriority = 128;
        [SerializeField] private int combatEffectPriority = 32;
        [SerializeField] private int stealthEffectPriority = 16;
        
                
// 蜉ｹ譫憺浹繝励・繝ｫ邂｡逅・        private Queue<AudioSource> effectSourcePool = new Queue<AudioSource>();
        private List<AudioSource> activeEffectSources = new List<AudioSource>();
        
        // 蜉ｹ譫憺浹繝・・繧ｿ繝吶・繧ｹ
        private Dictionary<string, SoundDataSO> effectDatabase = new Dictionary<string, SoundDataSO>();
        
        // 莉悶・繧ｪ繝ｼ繝・ぅ繧ｪ繧ｷ繧ｹ繝・Β縺ｨ縺ｮ騾｣謳ｺ・・erviceLocator邨檎罰・・        private IAudioService audioService;
        private ISpatialAudioService spatialAudioService;
        private IStealthAudioService stealthAudioService;
        
        
        
        // IInitializable螳溯｣・        public int Priority => 15; // 繧ｪ繝ｼ繝・ぅ繧ｪ繧ｵ繝ｼ繝薙せ縺ｮ蠕後↓蛻晄悄蛹・        public bool IsInitialized { get; private set; }
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            // 笨・ServiceLocator蟆ら畑螳溯｣・・縺ｿ - Singleton繝代ち繝ｼ繝ｳ螳悟・蜑企勁
            DontDestroyOnLoad(gameObject);
            
            if (FeatureFlags.UseServiceLocator)
            {
                ServiceLocator.RegisterService<IEffectService>(this);
                
                if (FeatureFlags.EnableDebugLogging)
                {
                    EventLogger.LogStatic("[EffectManager] Registered to ServiceLocator as IEffectService");
                }
            }
            
            InitializeEffectSourcePool();
        }
        
        private void Start()
        {
            Initialize();
        }
        
        private void OnDestroy()
        {
            // 笨・ServiceLocator蟆ら畑螳溯｣・・縺ｿ - Singleton繝代ち繝ｼ繝ｳ螳悟・蜑企勁
            if (FeatureFlags.UseServiceLocator)
            {
                ServiceLocator.UnregisterService<IEffectService>();
                
                if (FeatureFlags.EnableDebugLogging)
                {
                    EventLogger.LogStatic("[EffectManager] Unregistered from ServiceLocator");
                }
            }
        }
        
        #endregion
        
        #region IInitializable Implementation
        
        /// <summary>
        /// IInitializable螳溯｣・- 蜉ｹ譫憺浹繧ｷ繧ｹ繝・Β縺ｮ蛻晄悄蛹・        /// </summary>
        public void Initialize()
        {
            if (IsInitialized) return;
            
            // 莉悶・繧ｪ繝ｼ繝・ぅ繧ｪ繧ｵ繝ｼ繝薙せ繧担erviceLocator縺九ｉ蜿門ｾ・            if (FeatureFlags.UseServiceLocator)
            {
                audioService = ServiceLocator.GetService<IAudioService>();
                spatialAudioService = ServiceLocator.GetService<ISpatialAudioService>();
                // TODO: StealthAudioService縺悟ｮ溯｣・＆繧後◆繧画怏蜉ｹ蛹・                // stealthAudioService = ServiceLocator.GetService<IStealthAudioService>();
            }
            
            // 繝輔か繝ｼ繝ｫ繝舌ャ繧ｯ: 譌｢蟄倥・譁ｹ豕・            if (audioService == null)
            {
                var audioManager = FindFirstObjectByType<AudioManager>();
                if (audioManager != null)
                {
                    audioService = audioManager;
                }
            }
            
            LoadEffectDatabase();
            
            IsInitialized = true;
            
            if (FeatureFlags.EnableDebugLogging)
            {
                EventLogger.LogStatic("[EffectManager] Initialization complete");
            }
        }
        
        #endregion
        
        #region IEffectService Implementation
        
        /// <summary>
        /// 蜉ｹ譫憺浹繧貞・逕・        /// </summary>
        public void PlayEffect(string effectId, Vector3 position = default, float volume = 1f)
        {
            if (!IsInitialized)
            {
                ServiceLocator.GetService<IEventLogger>().LogWarning("[EffectManager] System not initialized");
                return;
            }
            
            // 譌｢蟄倥・PlayEffect繝｡繧ｽ繝・ラ繧貞他縺ｳ蜃ｺ縺・            if (HasMethod("PlayEffect", typeof(string), typeof(Vector3), typeof(float)))
            {
                // 譌｢蟄倥・螳溯｣・ｒ菴ｿ逕ｨ・医Μ繝輔Ξ繧ｯ繧ｷ繝ｧ繝ｳ縺ｧ蜻ｼ縺ｳ蜃ｺ縺励∪縺溘・逶ｴ謗･螳溯｣・ｼ・                // TODO: 譌｢蟄倥・PlayEffect繝｡繧ｽ繝・ラ縺ｨ邨ｱ蜷・                PlayEffectWithSource(effectId, position, volume); //ventLogger.Log($"[EffectManager] Playing effect: {effectId} at {position} with volume {volume}");
            }
        }
        
        /// <summary>
        /// 繝ｫ繝ｼ繝怜柑譫憺浹繧帝幕蟋・        /// </summary>
        public int StartLoopingEffect(string effectId, Vector3 position, float volume = 1f)
        {
            if (!IsInitialized) return -1;
            
            // TODO: 繝ｫ繝ｼ繝怜柑譫憺浹縺ｮ螳溯｣・            EventLogger.LogStatic($"[EffectManager] Starting looping effect: {effectId}");
            return 0; // 莉ｮ縺ｮID
        }
        
        /// <summary>
        /// 繝ｫ繝ｼ繝怜柑譫憺浹繧貞●豁｢
        /// </summary>
        public void StopLoopingEffect(int loopId)
        {
            if (!IsInitialized) return;
            
            // TODO: 繝ｫ繝ｼ繝怜柑譫憺浹縺ｮ蛛懈ｭ｢螳溯｣・            EventLogger.LogStatic($"[EffectManager] Stopping looping effect: {loopId}");
        }
        
        /// <summary>
        /// 荳蠎ｦ縺縺大・逕溘☆繧句柑譫憺浹・磯㍾隍・亟豁｢・・        /// </summary>
        public void PlayOneShot(string effectId, Vector3 position = default, float volume = 1f)
        {
            PlayEffect(effectId, position, volume);
        }
        
        /// <summary>
        /// 繝ｩ繝ｳ繝繝縺ｪ蜉ｹ譫憺浹繧貞・逕・        /// </summary>
        public void PlayRandomEffect(string[] effectIds, Vector3 position = default, float volume = 1f)
        {
            if (effectIds != null && effectIds.Length > 0)
            {
                var randomId = effectIds[Random.Range(0, effectIds.Length)];
                PlayEffect(randomId, position, volume);
            }
        }
        
        /// <summary>
        /// 蜉ｹ譫憺浹縺ｮ繝斐ャ繝√ｒ險ｭ螳・        /// </summary>
        public void SetEffectPitch(string effectId, float pitch)
        {
            // TODO: 繝斐ャ繝∬ｨｭ螳壹・螳溯｣・            EventLogger.LogStatic($"[EffectManager] Setting pitch for {effectId}: {pitch}");
        }
        
        /// <summary>
        /// 蜉ｹ譫憺浹繝励・繝ｫ繧偵・繝ｪ繝ｭ繝ｼ繝・        /// </summary>
        public void PreloadEffects(string[] effectIds)
        {
            // TODO: 繝励Μ繝ｭ繝ｼ繝画ｩ溯・縺ｮ螳溯｣・            if (FeatureFlags.EnableDebugLogging)
            {
                EventLogger.LogStatic($"[EffectManager] Preloading {effectIds?.Length ?? 0} effects");
            }
        }
        
        /// <summary>
        /// 蜉ｹ譫憺浹繝励・繝ｫ繧偵け繝ｪ繧｢
        /// </summary>
        public void ClearEffectPool()
        {
            // TODO: 繝励・繝ｫ繧ｯ繝ｪ繧｢讖溯・縺ｮ螳溯｣・            if (FeatureFlags.EnableDebugLogging)
            {
                EventLogger.LogStatic("[EffectManager] Clearing effect pool");
            }
        }
        
        /// <summary>
        /// 繝｡繧ｽ繝・ラ縺ｮ蟄伜惠繝√ぉ繝・け逕ｨ繝倥Ν繝代・
        /// </summary>
        private bool HasMethod(string methodName, params System.Type[] parameterTypes)
        {
            var method = GetType().GetMethod(methodName, parameterTypes);
            return method != null;
        }
        
        #endregion

        #region Public Interface
        
        /// <summary>
        /// 蜉ｹ譫憺浹繧貞・逕滂ｼ井ｸ闊ｬ逧・↑繧､繝ｳ繧ｿ繝ｼ繝輔ぉ繝ｼ繧ｹ・・        /// </summary>
        public AudioSource PlayEffectWithSource(string effectID, Vector3 position = default, float volumeMultiplier = 1f)
        {
            if (!effectDatabase.ContainsKey(effectID))
            {
                ServiceLocator.GetService<IEventLogger>().LogWarning($"[EffectManager] Effect '{effectID}' not found in database");
                return null;
            }
            
            var soundData = effectDatabase[effectID];
            var eventData = AudioEventData.CreateEffectDefault(effectID);
            eventData.worldPosition = position;
            eventData.volume *= volumeMultiplier;
            
            return PlayEffectInternal(soundData, eventData);
        }
        
        /// <summary>
        /// UI蜉ｹ譫憺浹縺ｮ蜀咲函
        /// </summary>
        public AudioSource PlayUIEffect(string effectID, float volumeMultiplier = 1f)
        {
            if (!enableUIEffects) return null;
            
            var eventData = AudioEventData.CreateUIDefault(effectID);
            eventData.volume = uiEffectVolume * volumeMultiplier;
            
            return PlayCategorizedEffect(effectID, eventData, EffectType.UI);
        }
        
        /// <summary>
        /// 繧､繝ｳ繧ｿ繝ｩ繧ｯ繧ｷ繝ｧ繝ｳ蜉ｹ譫憺浹縺ｮ蜀咲函
        /// </summary>
        public AudioSource PlayInteractionEffect(string effectID, Vector3 position, float volumeMultiplier = 1f)
        {
            if (!enableInteractionEffects) return null;
            
            var eventData = AudioEventData.CreateEffectDefault(effectID);
            eventData.worldPosition = position;
            eventData.volume = interactionEffectVolume * volumeMultiplier;
            eventData.affectsStealthGameplay = true;
            
            return PlayCategorizedEffect(effectID, eventData, EffectType.Interaction);
        }
        
        /// <summary>
        /// 謌ｦ髣伜柑譫憺浹縺ｮ蜀咲函
        /// </summary>
        public AudioSource PlayCombatEffect(string effectID, Vector3 position, float volumeMultiplier = 1f)
        {
            if (!enableCombatEffects) return null;
            
            var eventData = AudioEventData.CreateEffectDefault(effectID);
            eventData.worldPosition = position;
            eventData.volume = combatEffectVolume * volumeMultiplier;
            eventData.priority = 0.9f; // 鬮伜━蜈亥ｺｦ
            eventData.affectsStealthGameplay = true;
            eventData.layerPriority = 80;
            
            return PlayCategorizedEffect(effectID, eventData, EffectType.Combat);
        }
        
        /// <summary>
        /// 繧ｹ繝・Ν繧ｹ蜉ｹ譫憺浹縺ｮ蜀咲函
        /// </summary>
        public AudioSource PlayStealthEffect(string effectID, Vector3 position, float hearingRadius, 
            SurfaceMaterial surface = SurfaceMaterial.Default, float volumeMultiplier = 1f)
        {
            if (!enableStealthEffects) return null;
            
            var eventData = AudioEventData.CreateStealthDefault(effectID);
            eventData.worldPosition = position;
            eventData.volume = stealthEffectVolume * volumeMultiplier;
            eventData.hearingRadius = hearingRadius;
            eventData.surfaceType = surface;
            
            return PlayCategorizedEffect(effectID, eventData, EffectType.Stealth);
        }
        
        /// <summary>
        /// 縺吶∋縺ｦ縺ｮ蜉ｹ譫憺浹繧貞●豁｢
        /// </summary>
        public void StopAllEffects()
        {
            foreach (var source in activeEffectSources.ToArray())
            {
                if (source != null && source.isPlaying)
                {
                    ReturnToPool(source);
                }
            }
        }
        
        /// <summary>
        /// 迚ｹ螳壹・繧ｫ繝・ざ繝ｪ縺ｮ蜉ｹ譫憺浹繧貞●豁｢
        /// </summary>
        public void StopEffectsByType(EffectType effectType)
        {
            // 縺薙・螳溯｣・〒縺ｯ邁｡逡･蛹悶・縺溘ａ縲∝・菴灘●豁｢繧定｡後≧
            // 螳滄圀縺ｮ螳溯｣・〒縺ｯ縲∝推AudioSource縺ｫ繧ｿ繧ｰ繧剃ｻ倥￠縺ｦ蛻・｡槭☆繧句ｿ・ｦ√′縺ゅｋ
            foreach (var source in activeEffectSources.ToArray())
            {
                if (source != null && source.isPlaying)
                {
                    // 蜉ｹ譫憺浹繧ｿ繧､繝励↓繧医ｋ蛻､螳夲ｼ・udioSource縺ｮ蜷榊燕縺ｾ縺溘・繧ｿ繧ｰ縺ｧ隴伜挨・・                    string sourceId = source.gameObject.name;
                    bool shouldStop = false;
                    
                    switch (effectType)
                    {
                        case EffectType.UI:
                            shouldStop = sourceId.Contains("UI") || source.spatialBlend == 0f;
                            break;
                        case EffectType.Combat:
                            shouldStop = sourceId.Contains("Combat") || source.priority < 64;
                            break;
                        case EffectType.Interaction:
                            shouldStop = sourceId.Contains("Interaction");
                            break;
                        case EffectType.Stealth:
                            shouldStop = sourceId.Contains("Stealth") || source.maxDistance > 20f;
                            break;
                    }
                    
                    if (shouldStop)
                    {
                        ReturnToPool(source);
                    }
                }
            }
        }
        
        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// 蜉ｹ譫憺浹繝励・繝ｫ縺ｮ蛻晄悄蛹・        /// </summary>
        private void InitializeEffectSourcePool()
        {
            for (int i = 0; i < maxConcurrentEffects; i++)
            {
                var go = new GameObject($"PooledEffectSource_{i}");
                go.transform.SetParent(transform);
                
                var audioSource = go.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 1f; // 繝・ヵ繧ｩ繝ｫ繝医・3D
                audioSource.outputAudioMixerGroup = effectMixerGroup;
                
                effectSourcePool.Enqueue(audioSource);
            }
        }
        
        /// <summary>
        /// 蜉ｹ譫憺浹繝・・繧ｿ繝吶・繧ｹ縺ｮ隱ｭ縺ｿ霎ｼ縺ｿ
        /// 隍・焚縺ｮ繝代せ縺九ｉ蜉ｹ譫憺浹繝・・繧ｿ繧貞庶髮・＠縲√き繝・ざ繝ｪ蛻･縺ｫ謨ｴ逅・＠縺ｾ縺・        /// </summary>
        private void LoadEffectDatabase()
        {
            effectDatabase.Clear();
            
            // 隍・焚縺ｮResources繝代せ縺九ｉ蜉ｹ譫憺浹繧定ｪｭ縺ｿ霎ｼ縺ｿ
            string[] resourcePaths = {
                "Audio/Effects",
                "Audio/Effects/UI",
                "Audio/Effects/Combat",
                "Audio/Effects/Interaction",
                "Audio/Effects/Stealth",
                "Audio/Effects/Environment"
            };
            
            int totalLoaded = 0;
            
            foreach (string path in resourcePaths)
            {
                var effectSounds = Resources.LoadAll<SoundDataSO>(path);
                foreach (var sound in effectSounds)
                {
                    if (sound != null)
                    {
                        // 驥崎､・メ繧ｧ繝・け・育焚縺ｪ繧九ヱ繧ｹ縺ｫ蜷悟錐繝輔ぃ繧､繝ｫ縺後≠繧句ｴ蜷医・蜃ｦ逅・ｼ・                        if (effectDatabase.ContainsKey(sound.name))
                        {
                            ServiceLocator.GetService<IEventLogger>().LogWarning($"[EffectManager] Duplicate effect sound found: {sound.name} in {path}");
                            continue;
                        }
                        
                        effectDatabase[sound.name] = sound;
                        totalLoaded++;
                    }
                }
            }
            
            // ScriptableObjects繝輔か繝ｫ繝縺九ｉ繧よ､懃ｴ｢・医・繝ｭ繧ｸ繧ｧ繧ｯ繝亥崋譛峨・繧ｵ繧ｦ繝ｳ繝峨ョ繝ｼ繧ｿ・・            var customEffects = Resources.LoadAll<SoundDataSO>("ScriptableObjects/Audio/Effects");
            foreach (var effect in customEffects)
            {
                if (effect != null && !effectDatabase.ContainsKey(effect.name))
                {
                    effectDatabase[effect.name] = effect;
                    totalLoaded++;
                }
            }
            
            // 繝・ヵ繧ｩ繝ｫ繝亥柑譫憺浹縺ｮ菴懈・・亥ｿ・ｦ∵怙蟆城剞縺ｮ繧ｵ繧ｦ繝ｳ繝会ｼ・            CreateDefaultEffectsIfNeeded();
            
            EventLogger.LogStatic($"[EffectManager] Loaded {totalLoaded} effect sounds from Resources. " +
                          $"Total effects in database: {effectDatabase.Count}");
                          
            // 繝・ヰ繝・げ諠・ｱ・壼茜逕ｨ蜿ｯ閭ｽ縺ｪ蜉ｹ譫憺浹繝ｪ繧ｹ繝医ｒ蜃ｺ蜉・            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (Application.isEditor)
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.AppendLine("[EffectManager] Available Effects:");
                foreach (var kvp in effectDatabase)
                {
                    sb.AppendLine($"  - {kvp.Key}");
                }
                EventLogger.LogStatic(sb.ToString());
            }
            #endif
        }
        
        /// <summary>
        /// 蝓ｺ譛ｬ逧・↑蜉ｹ譫憺浹縺瑚ｦ九▽縺九ｉ縺ｪ縺・ｴ蜷医↓繝・ヵ繧ｩ繝ｫ繝医ｒ菴懈・
        /// </summary>
        private void CreateDefaultEffectsIfNeeded()
        {
            // 蠢・医・蜉ｹ譫憺浹ID繝ｪ繧ｹ繝・            string[] requiredEffects = {
                "ui_click",
                "ui_hover",
                "footstep_default",
                "item_pickup",
                "door_open",
                "door_close"
            };
            
            int created = 0;
            foreach (string effectId in requiredEffects)
            {
                if (!effectDatabase.ContainsKey(effectId))
                {
                    // 繝・ヵ繧ｩ繝ｫ繝医・SoundDataSO繧貞虚逧・ｽ懈・・亥ｮ溯｡梧凾縺ｮ縺ｿ・・                    var defaultSound = ScriptableObject.CreateInstance<SoundDataSO>();
                    defaultSound.name = effectId;
                    // 莉悶・繝・ヵ繧ｩ繝ｫ繝郁ｨｭ螳壹・ SoundDataSO 縺ｮ蛻晄悄蛟､繧剃ｽｿ逕ｨ
                    
                    effectDatabase[effectId] = defaultSound;
                    created++;
                    
                    ServiceLocator.GetService<IEventLogger>().LogWarning($"[EffectManager] Created default effect: {effectId}");
                }
            }
            
            if (created > 0)
            {
                ServiceLocator.GetService<IEventLogger>().LogWarning($"[EffectManager] Created {created} default effects. " +
                                     "Consider adding proper SoundDataSO assets for these effects.");
            }
        }
        
        /// <summary>
        /// 繧ｫ繝・ざ繝ｪ蛻･蜉ｹ譫憺浹蜀咲函縺ｮ蜀・Κ蜃ｦ逅・        /// </summary>
        private AudioSource PlayCategorizedEffect(string effectID, AudioEventData eventData, EffectType effectType)
        {
            if (!effectDatabase.ContainsKey(effectID))
            {
                ServiceLocator.GetService<IEventLogger>().LogWarning($"[EffectManager] Effect '{effectID}' not found in database");
                return null;
            }
            
            var soundData = effectDatabase[effectID];
            
            // 蜉ｹ譫憺浹繧ｿ繧､繝励↓蠢懊§縺溯ｿｽ蜉險ｭ螳・            ApplyEffectTypeSettings(eventData, effectType);
            
            return PlayEffectInternal(soundData, eventData);
        }
        
        /// <summary>
        /// 蜉ｹ譫憺浹蜀咲函縺ｮ蜀・Κ蜃ｦ逅・        /// </summary>
        private AudioSource PlayEffectInternal(SoundDataSO soundData, AudioEventData eventData)
        {
            // 繧ｹ繝・Ν繧ｹ髻ｳ髻ｿ繧ｷ繧ｹ繝・Β縺後≠繧句ｴ蜷医・縲√◎縺｡繧峨↓蟋碑ｭｲ
            if (spatialAudioService != null && eventData.affectsStealthGameplay)
            {
                spatialAudioService.Play3DSound(eventData.soundID, eventData.worldPosition, eventData.hearingRadius, eventData.volume);
                return null; // Spatial audio service doesn't return AudioSourceioSource);
            }
            
            // 騾壼ｸｸ縺ｮ蜉ｹ譫憺浹蜀咲函蜃ｦ逅・            var audioSource = GetPooledEffectSource();
            if (audioSource == null) return null;
            
            SetupEffectSource(audioSource, soundData, eventData);
            
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
        /// 蜉ｹ譫憺浹繧ｿ繧､繝励↓蠢懊§縺溯ｨｭ螳壹・驕ｩ逕ｨ
        /// </summary>
        private void ApplyEffectTypeSettings(AudioEventData eventData, EffectType effectType)
        {
            switch (effectType)
            {
                case EffectType.UI:
                    eventData.use3D = false;
                    eventData.affectsStealthGameplay = false;
                    eventData.layerPriority = 5;
                    break;
                    
                case EffectType.Interaction:
                    eventData.use3D = true;
                    eventData.affectsStealthGameplay = true;
                    eventData.layerPriority = 60;
                    break;
                    
                case EffectType.Combat:
                    eventData.use3D = true;
                    eventData.affectsStealthGameplay = true;
                    eventData.layerPriority = 80;
                    eventData.priority = 0.9f;
                    break;
                    
                case EffectType.Stealth:
                    eventData.use3D = true;
                    eventData.affectsStealthGameplay = true;
                    eventData.layerPriority = 100;
                    eventData.priority = 0.8f;
                    break;
            }
        }
        
        /// <summary>
        /// 繝励・繝ｫ縺九ｉ繧ｨ繝輔ぉ繧ｯ繝医た繝ｼ繧ｹ繧貞叙蠕・        /// </summary>
        private AudioSource GetPooledEffectSource()
        {
            if (effectSourcePool.Count > 0)
            {
                var audioSource = effectSourcePool.Dequeue();
                activeEffectSources.Add(audioSource);
                return audioSource;
            }
            
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            ProjectDebug.LogWarning("[EffectManager] Effect source pool exhausted");
#endif
            return null;
        }
        
        /// <summary>
        /// 繧ｨ繝輔ぉ繧ｯ繝医た繝ｼ繧ｹ繧偵・繝ｼ繝ｫ縺ｫ霑泌唆
        /// </summary>
        private void ReturnToPool(AudioSource audioSource)
        {
            if (audioSource == null) return;
            
            audioSource.Stop();
            audioSource.clip = null;
            
            activeEffectSources.Remove(audioSource);
            effectSourcePool.Enqueue(audioSource);
        }
        
        /// <summary>
        /// 繧ｨ繝輔ぉ繧ｯ繝医た繝ｼ繧ｹ縺ｮ險ｭ螳・        /// </summary>
        private void SetupEffectSource(AudioSource audioSource, SoundDataSO soundData, AudioEventData eventData)
        {
            audioSource.transform.position = eventData.worldPosition;
            audioSource.volume = eventData.volume;
            audioSource.pitch = eventData.pitch;
            
            if (eventData.use3D)
            {
                audioSource.spatialBlend = soundData.Is3D ? soundData.SpatialBlend : 1f;
                audioSource.minDistance = soundData.MinDistance;
                audioSource.maxDistance = soundData.MaxDistance;
                audioSource.rolloffMode = soundData.RolloffMode;
            }
            else
            {
                audioSource.spatialBlend = 0f; // 2D髻ｳ髻ｿ
            }
            
            // 蜆ｪ蜈亥ｺｦ險ｭ螳・            int unityPriority = Mathf.RoundToInt((1f - eventData.priority) * 256f);
            audioSource.priority = Mathf.Clamp(unityPriority, 0, 256);
        }
        
        /// <summary>
        /// 蜀咲函邨ゆｺ・ｾ後↓繝励・繝ｫ縺ｫ霑泌唆縺吶ｋ繧ｳ繝ｫ繝ｼ繝√Φ
        /// </summary>
        private System.Collections.IEnumerator ReturnToPoolWhenFinished(AudioSource audioSource, float clipLength)
        {
            yield return new WaitForSeconds(clipLength + 0.1f);
            ReturnToPool(audioSource);
        }
        
        
        /// <summary>
        /// 繧ｨ繝輔ぉ繧ｯ繝医ち繧､繝励↓蝓ｺ縺･縺・※險ｭ螳壹ｒ驕ｩ逕ｨ
        /// </summary>
        private void ApplyEffectTypeSettings(AudioSource audioSource, EffectType effectType)
        {
            switch (effectType)
            {
                case EffectType.UI:
                    audioSource.priority = uiEffectPriority;
                    audioSource.volume *= uiEffectVolume;
                    break;
                case EffectType.Interaction:
                    audioSource.priority = interactionEffectPriority;
                    audioSource.volume *= interactionEffectVolume;
                    break;
                case EffectType.Combat:
                    audioSource.priority = combatEffectPriority;
                    audioSource.volume *= combatEffectVolume;
                    break;
                case EffectType.Stealth:
                    audioSource.priority = stealthEffectPriority;
                    audioSource.volume *= stealthEffectVolume;
                    break;
            }
        }
        
        /// <summary>
        /// 繧ｨ繝輔ぉ繧ｯ繝医ち繧､繝玲欠螳壹〒蜉ｹ譫憺浹繧貞・逕・        /// </summary>
        public void PlayEffect(string effectName, EffectType effectType, Vector3 position = default)
        {
            if (!effectDatabase.TryGetValue(effectName, out SoundDataSO soundData))
            {
                ServiceLocator.GetService<IEventLogger>().LogError($"Effect '{effectName}' not found in database");
                return;
            }

            var audioSource = GetPooledEffectSource();
            if (audioSource == null) return;

            // 蝓ｺ譛ｬ險ｭ螳壹ｒ驕ｩ逕ｨ
            audioSource.clip = soundData.GetRandomClip();
            audioSource.volume = soundData.GetRandomVolume();
            audioSource.pitch = soundData.GetRandomPitch();

            // 繧ｨ繝輔ぉ繧ｯ繝医ち繧､繝怜崋譛峨・險ｭ螳壹ｒ驕ｩ逕ｨ
            ApplyEffectTypeSettings(audioSource, effectType);

            // 菴咲ｽｮ險ｭ螳・            if (position != default)
            {
                audioSource.transform.position = position;
                audioSource.spatialBlend = 1f; // 3D髻ｳ髻ｿ
            }
            else
            {
                audioSource.spatialBlend = 0f; // 2D髻ｳ髻ｿ
            }

            audioSource.Play();
            StartCoroutine(ReturnToPoolWhenFinished(audioSource, soundData.GetRandomClip().length));

            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            EventLogger.LogStatic($"Playing effect: {effectName} ({effectType}) with priority: {audioSource.priority}");
            #endif
        }
        
        /// <summary>
        /// UI繧ｵ繧ｦ繝ｳ繝牙・逕溘・繧ｷ繝ｧ繝ｼ繝医き繝・ヨ
        /// </summary>
        public void PlayUIEffect(string effectName)
        {
            PlayEffect(effectName, EffectType.UI);
        }
        
        /// <summary>
        /// 繧､繝ｳ繧ｿ繝ｩ繧ｯ繧ｷ繝ｧ繝ｳ繧ｵ繧ｦ繝ｳ繝牙・逕溘・繧ｷ繝ｧ繝ｼ繝医き繝・ヨ
        /// </summary>
        public void PlayInteractionEffect(string effectName, Vector3 position = default)
        {
            PlayEffect(effectName, EffectType.Interaction, position);
        }
        
        /// <summary>
        /// 謌ｦ髣倥し繧ｦ繝ｳ繝牙・逕溘・繧ｷ繝ｧ繝ｼ繝医き繝・ヨ
        /// </summary>
        public void PlayCombatEffect(string effectName, Vector3 position = default)
        {
            PlayEffect(effectName, EffectType.Combat, position);
        }
        
        /// <summary>
        /// 繧ｹ繝・Ν繧ｹ繧ｵ繧ｦ繝ｳ繝牙・逕溘・繧ｷ繝ｧ繝ｼ繝医き繝・ヨ
        /// </summary>
        public void PlayStealthEffect(string effectName, Vector3 position = default)
        {
            PlayEffect(effectName, EffectType.Stealth, position);
        }
        
        #endregion
        
        #region Public Status API
        
        /// <summary>
        /// 莉ｻ諢上・蜉ｹ譫憺浹縺悟・逕滉ｸｭ縺狗｢ｺ隱・        /// </summary>
        public bool IsPlaying()
        {
            return activeEffectSources.Any(source => source != null && source.isPlaying);
        }
        
        /// <summary>
        /// 謖・ｮ壹＠縺溷柑譫憺浹縺悟・逕滉ｸｭ縺狗｢ｺ隱搾ｼ育ｰ｡逡･螳溯｣・ｼ・        /// </summary>
        public bool IsPlaying(string effectId)
        {
            if (string.IsNullOrEmpty(effectId))
                return false;
                
            return activeEffectSources.Any(source => 
                source != null && 
                source.isPlaying && 
                (source.clip?.name.Contains(effectId) == true || 
                 source.gameObject.name.Contains(effectId)));
        }
        
        /// <summary>
        /// 謖・ｮ壹＠縺溘ち繧､繝励・蜉ｹ譫憺浹縺悟・逕滉ｸｭ縺狗｢ｺ隱・        /// </summary>
        public bool IsPlayingType(EffectType effectType)
        {
            return activeEffectSources.Any(source =>
            {
                if (source == null || !source.isPlaying)
                    return false;
                    
                string sourceId = source.gameObject.name;
                
                return effectType switch
                {
                    EffectType.UI => sourceId.Contains("UI") || source.spatialBlend == 0f,
                    EffectType.Combat => sourceId.Contains("Combat") || source.priority < 64,
                    EffectType.Interaction => sourceId.Contains("Interaction"),
                    EffectType.Stealth => sourceId.Contains("Stealth") || source.maxDistance > 20f,
                    _ => false
                };
            });
        }
        
        /// <summary>
        /// 繧｢繧ｯ繝・ぅ繝悶↑蜉ｹ譫憺浹縺ｮ謨ｰ繧貞叙蠕・        /// </summary>
        public int GetActiveEffectCount()
        {
            return activeEffectSources.Count(source => source != null && source.isPlaying);
        }
        
        /// <summary>
        /// 繧｢繧ｯ繝・ぅ繝悶↑蜉ｹ譫憺浹縺ｮ譛螟ｧ謨ｰ繧貞叙蠕・        /// </summary>
        public int GetMaxConcurrentEffects()
        {
            return maxConcurrentEffects;
        }
        
        /// <summary>
        /// 蜉ｹ譫憺浹繝励・繝ｫ縺ｮ蛻ｩ逕ｨ蜿ｯ閭ｽ謨ｰ繧貞叙蠕・        /// </summary>
        public int GetAvailableEffectSourceCount()
        {
            return effectSourcePool.Count;
        }
        
        /// <summary>
        /// 謖・ｮ壹＠縺溷柑譫憺浹ID縺後ョ繝ｼ繧ｿ繝吶・繧ｹ縺ｫ逋ｻ骭ｲ縺輔ｌ縺ｦ縺・ｋ縺狗｢ｺ隱・        /// </summary>
        public bool HasEffectData(string effectId)
        {
            return !string.IsNullOrEmpty(effectId) && effectDatabase.ContainsKey(effectId);
        }
        
        /// <summary>
        /// 逋ｻ骭ｲ縺輔ｌ縺ｦ縺・ｋ蜉ｹ譫憺浹ID縺ｮ荳隕ｧ繧貞叙蠕・        /// </summary>
        public string[] GetRegisteredEffectIds()
        {
            return effectDatabase.Keys.ToArray();
        }
        
        /// <summary>
        /// 繧ｫ繝・ざ繝ｪ蛻･縺ｮ蜉ｹ譫憺浹譛牙柑迥ｶ諷九ｒ蜿門ｾ・        /// </summary>
        public bool IsCategoryEnabled(EffectType effectType)
        {
            return effectType switch
            {
                EffectType.UI => enableUIEffects,
                EffectType.Interaction => enableInteractionEffects,
                EffectType.Combat => enableCombatEffects,
                EffectType.Stealth => enableStealthEffects,
                _ => false
            };
        }
        
        #endregion
    }
}