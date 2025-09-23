using asterivo.Unity60.Core;
using UnityEngine;
using System.Collections.Generic;
using asterivo.Unity60.Core.Audio.Data;
using asterivo.Unity60.Core.Events;
// using asterivo.Unity60.Core.Debug;
using asterivo.Unity60.Core.Audio.Interfaces;
// using asterivo.Unity60.Core.Data;
// using asterivo.Unity60.Core.Services; // Removed to avoid circular dependency
using Sirenix.OdinInspector;
// using asterivo.Unity60.Core.Helpers;

namespace asterivo.Unity60.Core.Audio
{
    /// <summary>
    /// 繧ｹ繝・Ν繧ｹ繧ｲ繝ｼ繝繝励Ξ繧､縺ｨ荳闊ｬ繧ｪ繝ｼ繝・ぅ繧ｪ縺ｮ蜊碑ｪｿ蛻ｶ蠕｡
    /// NPC縺ｮ閨ｴ隕壹そ繝ｳ繧ｵ繝ｼ繧ｷ繧ｹ繝・Β縺ｨ縺ｮ騾｣謳ｺ繧堤ｮ｡逅・    /// ServiceLocator蟇ｾ蠢懃沿
    /// </summary>
    public class StealthAudioCoordinator : MonoBehaviour, IStealthAudioService
    {
        [TabGroup("Stealth Coordinator", "AI Integration")]
        [Header("AI Integration Settings")]
        [SerializeField, Range(0f, 1f)] private float aiAlertThreshold = 0.5f;
        [SerializeField, Range(1f, 10f)] private float playerHidingRadius = 3f;
        [SerializeField] private LayerMask aiLayerMask = -1;

        [TabGroup("Stealth Coordinator", "Audio Reduction")]
        [Header("Audio Reduction Settings")]
        [SerializeField, Range(0f, 1f)] private float bgmReductionAmount = 0.4f;
        [SerializeField, Range(0f, 1f)] private float ambientReductionAmount = 0.6f;
        [SerializeField, Range(0f, 1f)] private float effectReductionAmount = 0.3f;

        [TabGroup("Stealth Coordinator", "Masking System")]
        [Header("Masking Effect Settings")]
        [SerializeField, Range(0f, 1f)] private float baseMaskingStrength = 0.2f;
        [SerializeField] private AnimationCurve weatherMaskingCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 0.8f);
        [SerializeField] private AnimationCurve timeMaskingCurve = AnimationCurve.EaseInOut(0f, 0.2f, 1f, 0.05f);

        [TabGroup("Stealth Coordinator", "Critical Actions")]
        [Header("Critical Stealth Actions")]
        [SerializeField] private string[] criticalActionTags = { "Lockpicking", "Hacking", "Infiltration" };
        [SerializeField, Range(0f, 3f)] private float criticalActionRadius = 2f;

        [TabGroup("Stealth Coordinator", "Events")]
        [Header("Event Integration")]
        [SerializeField] private GameEvent stealthModeActivatedEvent;
        [SerializeField] private GameEvent stealthModeDeactivatedEvent;
        [SerializeField] private GameEvent maskingLevelChangedEvent;

        [TabGroup("Stealth Coordinator", "Runtime Info")]
        [Header("Runtime Information")]
        [SerializeField, ReadOnly] private bool isStealthModeActive;
        [SerializeField, ReadOnly] private bool isOverrideActive;
        [SerializeField, ReadOnly] private float currentMaskingLevel;
        [SerializeField, ReadOnly] private int nearbyAlertAICount;

        // 蜀・Κ迥ｶ諷狗ｮ｡逅・        private Transform playerTransform;
        private List<Transform> nearbyAI = new List<Transform>();
        private Dictionary<AudioCategory, float> categoryVolumeMultipliers = new Dictionary<AudioCategory, float>();
        private bool previousStealthModeState = false;
        private float globalMaskingStrength = 0f;

        // 繧ｷ繧ｹ繝・Β騾｣謳ｺ・・erviceLocator邨檎罰・・        private IAudioService audioService;
        private AudioManager audioManager;
        private DynamicAudioEnvironment dynamicEnvironment;
        
        // 笨・Singleton 繝代ち繝ｼ繝ｳ繧貞ｮ悟・蜑企勁 - ServiceLocator蟆ら畑螳溯｣・        
        // 蛻晄悄蛹也憾諷狗ｮ｡逅・        public bool IsInitialized { get; private set; }

        #region Unity Lifecycle

        private void Awake()
        {
            // 笨・ServiceLocator蟆ら畑螳溯｣・・縺ｿ - Singleton繝代ち繝ｼ繝ｳ螳悟・蜑企勁
            DontDestroyOnLoad(gameObject);
            
            // ServiceLocator縺ｫ逋ｻ骭ｲ
            if (asterivo.Unity60.Core.FeatureFlags.UseServiceLocator)
            {
                try
                {
                    ServiceLocator.RegisterService<IStealthAudioService>(this);
                    
                    if (asterivo.Unity60.Core.FeatureFlags.EnableDebugLogging)
                    {
                        var eventLogger = ServiceLocator.GetService<IEventLogger>();
                        if (eventLogger != null) {
                            eventLogger.Log("[StealthAudioCoordinator] Successfully registered to ServiceLocator as IStealthAudioService");
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    var eventLogger = ServiceLocator.GetService<IEventLogger>();
                    if (eventLogger != null) {
                        eventLogger.LogError($"[StealthAudioCoordinator] Failed to register to ServiceLocator: {ex.Message}");
                    }
                }
            }
            else
            {
                var eventLogger = ServiceLocator.GetService<IEventLogger>();
                if (eventLogger != null) {
                    eventLogger.LogWarning("[StealthAudioCoordinator] ServiceLocator is disabled - service not registered");
                }
            }
            
            InitializeCoordinator();
        }

        private void Start()
        {
            Initialize();
        }

        private void Update()
        {
            UpdateNearbyAIDetection();
            UpdateStealthModeState();
            UpdateMaskingEffects();
        }
        
        private void OnDestroy()
        {
            // 笨・ServiceLocator蟆ら畑螳溯｣・・縺ｿ - Singleton繝代ち繝ｼ繝ｳ螳悟・蜑企勁
            // ServiceLocator縺九ｉ逋ｻ骭ｲ隗｣髯､
            if (asterivo.Unity60.Core.FeatureFlags.UseServiceLocator)
            {
                try
                {
                    ServiceLocator.UnregisterService<IStealthAudioService>();
                    
                    if (asterivo.Unity60.Core.FeatureFlags.EnableDebugLogging)
                    {
                        var eventLogger = ServiceLocator.GetService<IEventLogger>();
                        if (eventLogger != null) {
                            eventLogger.Log("[StealthAudioCoordinator] Unregistered from ServiceLocator");
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    var eventLogger = ServiceLocator.GetService<IEventLogger>();
                    if (eventLogger != null) {
                        eventLogger.LogError($"[StealthAudioCoordinator] Failed to unregister from ServiceLocator: {ex.Message}");
                    }
                }
            }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// 繧ｳ繝ｼ繝・ぅ繝阪・繧ｿ繝ｼ縺ｮ蛻晄悄蛹・        /// </summary>
        private void InitializeCoordinator()
        {
            // 繝励Ξ繧､繝､繝ｼ繝医Λ繝ｳ繧ｹ繝輔か繝ｼ繝縺ｮ讀懃ｴ｢
            var player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
            else
            {
                var eventLogger = ServiceLocator.GetService<IEventLogger>();
                if (eventLogger != null) {
                    eventLogger.LogWarning("[StealthAudioCoordinator] Player object not found! Please assign a Player tag.");
                }
            }
        }

        /// <summary>
        /// 繧ｷ繧ｹ繝・Β蜿ら・縺ｮ讀懃ｴ｢
        /// Phase 3 遘ｻ陦後ヱ繧ｿ繝ｼ繝ｳ: ServiceLocator蜆ｪ蜈医ヾingleton 繝輔か繝ｼ繝ｫ繝舌ャ繧ｯ
        /// </summary>
        private void FindSystemReferences()
        {
            // ServiceLocator邨檎罰縺ｧAudioService繧貞叙蠕・            if (asterivo.Unity60.Core.FeatureFlags.UseServiceLocator && audioService == null)
            {
                try
                {
                    audioService = ServiceLocator.GetService<IAudioService>();
                    
                    if (audioService != null)
                    {
                        if (asterivo.Unity60.Core.FeatureFlags.EnableDebugLogging)
                        {
                            var eventLogger = ServiceLocator.GetService<IEventLogger>();
                            if (eventLogger != null) {
                                eventLogger.Log("[StealthAudioCoordinator] Successfully retrieved AudioService from ServiceLocator");
                            }
                        }
                    }
                    else
                    {
                        if (asterivo.Unity60.Core.FeatureFlags.EnableMigrationMonitoring)
                        {
                            var eventLogger = ServiceLocator.GetService<IEventLogger>();
                            if (eventLogger != null) {
                                eventLogger.LogWarning("[StealthAudioCoordinator] ServiceLocator returned null for IAudioService, falling back to legacy approach");
                            }
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    var eventLogger = ServiceLocator.GetService<IEventLogger>();
                    if (eventLogger != null) {
                        eventLogger.LogError($"[StealthAudioCoordinator] Failed to retrieve AudioService from ServiceLocator: {ex.Message}");
                    }
                }
            }
            
            // 笨・ServiceLocator蟆ら畑螳溯｣・- 逶ｴ謗･AudioManager繧呈､懃ｴ｢
            if (audioManager == null)
            {
                try
                {
                    audioManager = ServiceHelper.GetServiceWithFallback<AudioManager>();
                    if (audioManager != null && asterivo.Unity60.Core.FeatureFlags.EnableDebugLogging)
                    {
                        var eventLogger = ServiceLocator.GetService<IEventLogger>();
                        if (eventLogger != null) {
                            eventLogger.Log("[StealthAudioCoordinator] Found AudioManager via FindFirstObjectByType");
                        }
                    }
                    else if (audioManager == null)
                    {
                        var eventLogger = ServiceLocator.GetService<IEventLogger>();
                        if (eventLogger != null) {
                            eventLogger.LogError("[StealthAudioCoordinator] No AudioManager found");
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    var eventLogger = ServiceLocator.GetService<IEventLogger>();
                    if (eventLogger != null) {
                        eventLogger.LogError($"[StealthAudioCoordinator] Failed to get AudioManager: {ex.Message}");
                    }
                }
            }

            if (dynamicEnvironment == null)
                dynamicEnvironment = ServiceHelper.GetServiceWithFallback<DynamicAudioEnvironment>();
        }

        /// <summary>
        /// 繧ｫ繝・ざ繝ｪ蛻･髻ｳ驥丞咲紫縺ｮ蛻晄悄蛹・        /// </summary>
        private void InitializeCategoryMultipliers()
        {
            categoryVolumeMultipliers[AudioCategory.BGM] = 1f;
            categoryVolumeMultipliers[AudioCategory.Ambient] = 1f;
            categoryVolumeMultipliers[AudioCategory.Effect] = 1f;
            categoryVolumeMultipliers[AudioCategory.Stealth] = 1f;
            categoryVolumeMultipliers[AudioCategory.UI] = 1f;
        }
        
        /// <summary>
        /// IInitializable螳溯｣・- 繧ｹ繝・Ν繧ｹ繧ｪ繝ｼ繝・ぅ繧ｪ繧ｳ繝ｼ繝・ぅ繝阪・繧ｿ繝ｼ縺ｮ蛻晄悄蛹・        /// </summary>
        public void Initialize()
        {
            if (IsInitialized) return;
            
            FindSystemReferences();
            InitializeCategoryMultipliers();
            
            IsInitialized = true;
            
            // 繝・ヰ繝・げ繝ｭ繧ｰ (荳譎ら噪縺ｫ邁｡邏蛹・
            { 
                var eventLogger = ServiceLocator.GetService<IEventLogger>(); 
                if (eventLogger != null) eventLogger.Log("[StealthAudioCoordinator] Initialization complete");
            }
        }

        #endregion
        
        #region IStealthAudioService Implementation
        
        /// <summary>
        /// 雜ｳ髻ｳ繧堤函謌・        /// </summary>
        public void CreateFootstep(Vector3 position, float intensity, string surfaceType)
        {
            if (!IsInitialized)
            {
                { 
                    var eventLogger = ServiceLocator.GetService<IEventLogger>(); 
                    if (eventLogger != null) eventLogger.LogWarning("[StealthAudioCoordinator] System not initialized");
                }
                return;
            }
            
            // TODO: 陦ｨ髱｢繧ｿ繧､繝励↓蠢懊§縺溯ｶｳ髻ｳ縺ｮ逕滓・
            // 繝・ヰ繝・げ繝ｭ繧ｰ蜃ｺ蜉・            { 
                var eventLogger = ServiceLocator.GetService<IEventLogger>(); 
                if (eventLogger != null) eventLogger.Log($"[StealthAudioCoordinator] Creating footstep at {position}, intensity: {intensity}, surface: {surfaceType}");
            }
        }
        
        /// <summary>
        /// 迺ｰ蠅・ヮ繧､繧ｺ繝ｬ繝吶Ν繧定ｨｭ螳夲ｼ医・繧ｹ繧ｭ繝ｳ繧ｰ蜉ｹ譫懃畑・・        /// </summary>
        public void SetEnvironmentNoiseLevel(float level)
        {
            if (!IsInitialized) return;
            
            globalMaskingStrength = Mathf.Clamp01(level);
            
            // 繝・ヰ繝・げ繝ｭ繧ｰ蜃ｺ蜉・            { 
                var eventLogger = ServiceLocator.GetService<IEventLogger>(); 
                if (eventLogger != null) eventLogger.Log($"[StealthAudioCoordinator] Environment noise level set to: {level}");
            }
        }
        
        /// <summary>
        /// NPC縺ｫ閨槭％縺医ｋ髻ｳ繧堤函謌・        /// </summary>
        public void EmitDetectableSound(Vector3 position, float radius, float intensity, string soundType)
        {
            if (!IsInitialized) return;
            
            // TODO: NPC縺ｮ閨ｴ隕壹そ繝ｳ繧ｵ繝ｼ縺ｸ縺ｮ騾夂衍
            NotifyAuditorySensors(position, radius, intensity);
            
            // 繝・ヰ繝・げ繝ｭ繧ｰ蜃ｺ蜉・            { 
                var eventLogger = ServiceLocator.GetService<IEventLogger>(); 
                if (eventLogger != null) eventLogger.Log($"[StealthAudioCoordinator] Detectable sound emitted: {soundType} at {position}");
            }
        }
        
        /// <summary>
        /// 豕ｨ諢上ｒ蠑輔￥髻ｳ繧貞・逕・        /// </summary>
        public void PlayDistraction(Vector3 position, float radius)
        {
            if (!IsInitialized) return;
            
            EmitDetectableSound(position, radius, 0.8f, "Distraction");
        }
        
        /// <summary>
        /// 隴ｦ謌偵Ξ繝吶Ν縺ｫ蠢懊§縺檻GM繧定ｨｭ螳・        /// </summary>
        public void SetAlertLevelMusic(AlertLevel level)
        {
            if (!IsInitialized || audioService == null) return;
            
            // TODO: 隴ｦ謌偵Ξ繝吶Ν縺ｫ蠢懊§縺檻GM蛻・ｊ譖ｿ縺・            string bgmName = level switch
            {
                AlertLevel.Relaxed => "Normal",
                AlertLevel.Suspicious => "Suspicious",
                AlertLevel.Investigating => "Alert",
                AlertLevel.Alert => "Combat",
                _ => "Normal"
            };
            
            // audioService.PlayBGM(bgmName); // TODO: IBGMService縺悟ｿ・ｦ・            
            // 繝・ヰ繝・げ繝ｭ繧ｰ蜃ｺ蜉・            { 
                var eventLogger = ServiceLocator.GetService<IEventLogger>(); 
                if (eventLogger != null) eventLogger.Log($"[StealthAudioCoordinator] Alert level music set: {level} -> {bgmName}");
            }
        }
        
        /// <summary>
        /// 繧ｪ繝ｼ繝・ぅ繧ｪ繝槭せ繧ｭ繝ｳ繧ｰ蜉ｹ譫懊ｒ驕ｩ逕ｨ
        /// </summary>
        public void ApplyAudioMasking(float maskingLevel)
        {
            if (!IsInitialized) return;
            
            currentMaskingLevel = Mathf.Clamp01(maskingLevel);
            
            // 譌｢蟄倥・繝槭せ繧ｭ繝ｳ繧ｰ繧ｷ繧ｹ繝・Β繧剃ｽｿ逕ｨ
            UpdateMaskingEffects();
            
            if (maskingLevelChangedEvent != null)
            {
                maskingLevelChangedEvent.Raise();
            }
        }
        
        /// <summary>
        /// NPC縺ｮ閨ｴ隕壹そ繝ｳ繧ｵ繝ｼ縺ｫ繧ｵ繧ｦ繝ｳ繝峨う繝吶Φ繝医ｒ騾夂衍
        /// </summary>
        public void NotifyAuditorySensors(Vector3 origin, float radius, float intensity)
        {
            if (!IsInitialized) return;
            
            // TODO: AI繧ｷ繧ｹ繝・Β縺ｨ縺ｮ騾｣謳ｺ螳溯｣・            // 繝・ヰ繝・げ繝ｭ繧ｰ蜃ｺ蜉・            { 
                var eventLogger = ServiceLocator.GetService<IEventLogger>(); 
                if (eventLogger != null) eventLogger.Log($"[StealthAudioCoordinator] Notifying auditory sensors: origin={origin}, radius={radius}, intensity={intensity}");
            }
        }
        
        /// <summary>
        /// 繝励Ξ繧､繝､繝ｼ縺ｮ髫蟇・ｺｦ縺ｫ蠢懊§縺滄浹髻ｿ隱ｿ謨ｴ
        /// </summary>
        public void AdjustStealthAudio(float stealthLevel)
        {
            if (!IsInitialized) return;
            
            // 繧ｹ繝・Ν繧ｹ繝ｬ繝吶Ν縺ｫ蠢懊§縺ｦ髻ｳ驥上ｒ隱ｿ謨ｴ
            float volumeReduction = 1f - (stealthLevel * 0.3f); // 譛螟ｧ30%貂幃浹
            
            if (audioService != null)
            {
                audioService.SetCategoryVolume("bgm", bgmReductionAmount * volumeReduction);
                audioService.SetCategoryVolume("ambient", ambientReductionAmount * volumeReduction);
                audioService.SetCategoryVolume("effect", effectReductionAmount * volumeReduction);
            }
            
            // 繝・ヰ繝・げ繝ｭ繧ｰ蜃ｺ蜉・            { 
                var eventLogger = ServiceLocator.GetService<IEventLogger>(); 
                if (eventLogger != null) eventLogger.Log($"[StealthAudioCoordinator] Stealth audio adjusted: level={stealthLevel}, reduction={volumeReduction}");
            }
        }

        #endregion

        #region Stealth Mode Detection

        /// <summary>
        /// 霑代￥縺ｮAI讀懷・縺ｮ譖ｴ譁ｰ
        /// </summary>
        private void UpdateNearbyAIDetection()
        {
            if (playerTransform == null) return;

            nearbyAI.Clear();
            nearbyAlertAICount = 0;

            // 蜻ｨ蝗ｲ縺ｮAI繧ｨ繝ｼ繧ｸ繧ｧ繝ｳ繝医ｒ讀懃ｴ｢
            Collider[] nearbyColliders = Physics.OverlapSphere(
                playerTransform.position, 
                playerHidingRadius * 2f, 
                aiLayerMask
            );

            foreach (var collider in nearbyColliders)
            {
                if (collider.CompareTag("AI") || collider.CompareTag("Enemy"))
                {
                    nearbyAI.Add(collider.transform);

                    // AI 縺ｮ隴ｦ謌偵Ξ繝吶Ν繧堤｢ｺ隱・                    var aiController = collider.GetComponent<IGameStateProvider>();
                    if (aiController != null && aiController.GetAlertLevel() > aiAlertThreshold)
                    {
                        nearbyAlertAICount++;
                    }
                }
            }
        }

        /// <summary>
        /// 繧ｹ繝・Ν繧ｹ繝｢繝ｼ繝臥憾諷九・譖ｴ譁ｰ
        /// </summary>
        private void UpdateStealthModeState()
        {
            bool newStealthMode = ShouldReduceNonStealthAudio();

            if (newStealthMode != previousStealthModeState)
            {
                isStealthModeActive = newStealthMode;
                previousStealthModeState = newStealthMode;

                // 髻ｳ驥丞咲紫縺ｮ譖ｴ譁ｰ
                UpdateCategoryVolumeMultipliers();

                // 繧､繝吶Φ繝育匱陦・                if (newStealthMode)
                {
                    stealthModeActivatedEvent?.Raise();
                }
                else
                {
                    stealthModeDeactivatedEvent?.Raise();
                }

                { 
                    var eventLogger = ServiceLocator.GetService<IEventLogger>(); 
                    if (eventLogger != null) eventLogger.Log($"<color=orange>[StealthAudioCoordinator]</color> Stealth mode {(newStealthMode ? "activated" : "deactivated")}");
                }
            }
        }

        #endregion

        #region Public Interface

        /// <summary>
        /// 髱槭せ繝・Ν繧ｹ髻ｳ髻ｿ繧呈椛蛻ｶ縺吶∋縺阪°繧貞愛螳・        /// </summary>
        public bool ShouldReduceNonStealthAudio()
        {
            if (isOverrideActive)
                return isStealthModeActive;

            // 繝励Ξ繧､繝､繝ｼ縺碁國繧後Δ繝ｼ繝峨・譎・            if (IsPlayerInHidingMode())
                return true;

            // 霑代￥縺ｮAI縺瑚ｭｦ謌堤憾諷九・譎・            if (nearbyAlertAICount > 0)
                return true;

            // 驥崎ｦ√↑繧ｹ繝・Ν繧ｹ繧｢繧ｯ繧ｷ繝ｧ繝ｳ荳ｭ
            if (IsPerformingCriticalStealthAction())
                return true;

            return false;
        }

        /// <summary>
        /// 繝槭せ繧ｭ繝ｳ繧ｰ蜉ｹ譫懊・險育ｮ・        /// </summary>
        public float CalculateMaskingEffect(Vector3 soundPosition, AudioEventData audioData)
        {
            float totalMasking = baseMaskingStrength;

            // BGM縺ｫ繧医ｋ繝槭せ繧ｭ繝ｳ繧ｰ
            if (IsBGMPlaying())
            {
                totalMasking += GetBGMMaskingStrength() * 0.3f;
            }

            // 迺ｰ蠅・浹縺ｫ繧医ｋ繝槭せ繧ｭ繝ｳ繧ｰ
            totalMasking += GetEnvironmentalMaskingAt(soundPosition) * 0.5f;

            // 螟ｩ蛟吶・譎る俣蟶ｯ縺ｫ繧医ｋ繝槭せ繧ｭ繝ｳ繧ｰ
            if (dynamicEnvironment != null)
            {
                var (env, weather, time) = dynamicEnvironment.GetCurrentState();
                totalMasking += GetWeatherMaskingEffect(weather) * 0.4f;
                totalMasking += GetTimeMaskingEffect(time) * 0.2f;
            }

            // 髻ｳ髻ｿ繧ｫ繝・ざ繝ｪ縺ｫ繧医ｋ隱ｿ謨ｴ
            totalMasking *= GetCategoryMaskingMultiplier(audioData.category);

            return Mathf.Clamp01(totalMasking);
        }

        /// <summary>
        /// NPC縺ｮ閨ｴ隕壹す繧ｹ繝・Β縺ｸ縺ｮ蠖ｱ髻ｿ蠎ｦ繧定ｨ育ｮ・        /// </summary>
        public float GetNPCAudibilityMultiplier(AudioEventData audioData)
        {
            if (!audioData.affectsStealthGameplay)
                return 0f; // 繧ｹ繝・Ν繧ｹ縺ｫ蠖ｱ髻ｿ縺励↑縺・浹縺ｯ NPC縺梧─遏･縺励↑縺・
            float maskingEffect = CalculateMaskingEffect(audioData.worldPosition, audioData);
            float audibilityMultiplier = 1f - maskingEffect;

            // 繧ｹ繝・Ν繧ｹ繝｢繝ｼ繝画凾縺ｮ霑ｽ蜉貂幄｡ｰ
            if (isStealthModeActive && audioData.canBeDuckedByTension)
            {
                audibilityMultiplier *= 0.7f;
            }

            return Mathf.Clamp01(audibilityMultiplier);
        }

        /// <summary>
        /// 髻ｳ髻ｿ繧ｫ繝・ざ繝ｪ縺ｮ髻ｳ驥丞咲紫繧貞叙蠕・        /// </summary>
        public float GetCategoryVolumeMultiplier(AudioCategory category)
        {
            return categoryVolumeMultipliers.TryGetValue(category, out float multiplier) ? multiplier : 1f;
        }

        /// <summary>
        /// 繧ｹ繝・Ν繧ｹ繝｢繝ｼ繝峨・蠑ｷ蛻ｶ險ｭ螳・        /// </summary>
        public void SetOverrideStealthMode(bool forceStealthMode)
        {
            isOverrideActive = true;
            isStealthModeActive = forceStealthMode;
            UpdateCategoryVolumeMultipliers();
        }

        /// <summary>
        /// 繧ｪ繝ｼ繝舌・繝ｩ繧､繝峨・隗｣髯､
        /// </summary>
        public void ClearStealthModeOverride()
        {
            isOverrideActive = false;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// 繝励Ξ繧､繝､繝ｼ縺碁國繧後Δ繝ｼ繝峨°繧貞愛螳・        /// </summary>
        private bool IsPlayerInHidingMode()
        {
            if (playerTransform == null) return false;

            // 繝励Ξ繧､繝､繝ｼ繧ｳ繝ｳ繝医Ο繝ｼ繝ｩ繝ｼ縺九ｉ縺ｮ迥ｶ諷句叙蠕励ｒ隧ｦ縺ｿ繧・            var playerController = playerTransform.GetComponent<IGameStateProvider>();
            if (playerController != null)
            {
                return playerController.IsInHidingMode();
            }

            // 繝輔か繝ｼ繝ｫ繝舌ャ繧ｯ・夊ｿ代￥縺ｮ髫繧悟ｴ謇繧ｪ繝悶ず繧ｧ繧ｯ繝医ｒ繝√ぉ繝・け
            Collider[] hideSpots = Physics.OverlapSphere(
                playerTransform.position, 
                playerHidingRadius, 
                LayerMask.GetMask("HideSpot")
            );

            return hideSpots.Length > 0;
        }

        /// <summary>
        /// 驥崎ｦ√↑繧ｹ繝・Ν繧ｹ繧｢繧ｯ繧ｷ繝ｧ繝ｳ荳ｭ縺九ｒ蛻､螳・        /// </summary>
        private bool IsPerformingCriticalStealthAction()
        {
            if (playerTransform == null) return false;

            // 蜻ｨ蝗ｲ縺ｮ驥崎ｦ√い繧ｯ繧ｷ繝ｧ繝ｳ繧ｪ繝悶ず繧ｧ繧ｯ繝医ｒ繝√ぉ繝・け
            Collider[] actionObjects = Physics.OverlapSphere(
                playerTransform.position, 
                criticalActionRadius
            );

            foreach (var obj in actionObjects)
            {
                foreach (var tag in criticalActionTags)
                {
                    if (obj.CompareTag(tag))
                    {
                        // 繧ｪ繝悶ず繧ｧ繧ｯ繝医′繧｢繧ｯ繝・ぅ繝也憾諷九°繧堤｢ｺ隱・                        var interactable = obj.GetComponent<IGameStateProvider>();
                        if (interactable != null && interactable.IsBeingUsed())
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 繧ｫ繝・ざ繝ｪ蛻･髻ｳ驥丞咲紫縺ｮ譖ｴ譁ｰ
        /// </summary>
        private void UpdateCategoryVolumeMultipliers()
        {
            if (isStealthModeActive)
            {
                categoryVolumeMultipliers[AudioCategory.BGM] = 1f - bgmReductionAmount;
                categoryVolumeMultipliers[AudioCategory.Ambient] = 1f - ambientReductionAmount;
                categoryVolumeMultipliers[AudioCategory.Effect] = 1f - effectReductionAmount;
                categoryVolumeMultipliers[AudioCategory.Stealth] = 1f; // 繧ｹ繝・Ν繧ｹ髻ｳ縺ｯ邯ｭ謖・                categoryVolumeMultipliers[AudioCategory.UI] = 1f; // UI髻ｳ縺ｯ邯ｭ謖・            }
            else
            {
                // 騾壼ｸｸ迥ｶ諷九↓蠕ｩ蟶ｰ
                categoryVolumeMultipliers[AudioCategory.BGM] = 1f;
                categoryVolumeMultipliers[AudioCategory.Ambient] = 1f;
                categoryVolumeMultipliers[AudioCategory.Effect] = 1f;
                categoryVolumeMultipliers[AudioCategory.Stealth] = 1f;
                categoryVolumeMultipliers[AudioCategory.UI] = 1f;
            }
        }

        /// <summary>
        /// 繝槭せ繧ｭ繝ｳ繧ｰ蜉ｹ譫懊・騾夂衍繧貞女縺代ｋ
        /// </summary>
        public void NotifyMaskingEffect(Vector3 position, float strength, float radius)
        {
            if (playerTransform == null) return;
            
            float distance = Vector3.Distance(playerTransform.position, position);
            if (distance > radius) return;
            
            // 繝励Ξ繧､繝､繝ｼ蜻ｨ霎ｺ縺ｮ繝槭せ繧ｭ繝ｳ繧ｰ蜉ｹ譫懊→縺励※險倬鹸
            // 縺薙・諠・ｱ縺ｯ髻ｳ髻ｿ繧ｷ繧ｹ繝・Β縺ｮ繝槭せ繧ｭ繝ｳ繧ｰ險育ｮ励↓菴ｿ逕ｨ縺輔ｌ繧・            float normalizedDistance = distance / radius;
            float effectiveStrength = strength * (1f - normalizedDistance);
            
            // 繧ｰ繝ｭ繝ｼ繝舌Ν繝槭せ繧ｭ繝ｳ繧ｰ蠑ｷ蠎ｦ繧剃ｸ譎ら噪縺ｫ蠅怜刈
            globalMaskingStrength = Mathf.Max(globalMaskingStrength, effectiveStrength * 0.5f);
            
            { 
                var eventLogger = ServiceLocator.GetService<IEventLogger>(); 
                if (eventLogger != null) eventLogger.Log($"<color=purple>[StealthAudioCoordinator]</color> Masking effect applied: strength={effectiveStrength:F2}, distance={distance:F1}m");
            }
        }

        /// <summary>
        /// 繝槭せ繧ｭ繝ｳ繧ｰ蜉ｹ譫懊・譖ｴ譁ｰ
        /// </summary>
        private void UpdateMaskingEffects()
        {
            if (playerTransform == null) return;

            // 繝励Ξ繧､繝､繝ｼ菴咲ｽｮ縺ｧ縺ｮ繝槭せ繧ｭ繝ｳ繧ｰ蜉ｹ譫懊ｒ險育ｮ・            var dummyAudioData = AudioEventData.CreateDefault("MaskingCalculation");
            dummyAudioData.worldPosition = playerTransform.position;

            float newMaskingLevel = CalculateMaskingEffect(playerTransform.position, dummyAudioData);

            if (Mathf.Abs(newMaskingLevel - currentMaskingLevel) > 0.05f)
            {
                currentMaskingLevel = newMaskingLevel;
                maskingLevelChangedEvent?.Raise();
            }
        }

        /// <summary>
        /// BGM縺悟・逕滉ｸｭ縺九ｒ遒ｺ隱・        /// </summary>
        private bool IsBGMPlaying()
        {
            // AudioManager縺九ｉ BGM 縺ｮ蜀咲函迥ｶ諷九ｒ蜿門ｾ・            if (audioManager != null)
            {
                // 螳溯｣・・ AudioManager 縺ｮ API 縺ｫ萓晏ｭ・                return true; // 繝励Ξ繝ｼ繧ｹ繝帙Ν繝繝ｼ
            }
            return false;
        }

        /// <summary>
        /// BGM縺ｮ繝槭せ繧ｭ繝ｳ繧ｰ蠑ｷ蠎ｦ繧貞叙蠕・        /// </summary>
        private float GetBGMMaskingStrength()
        {
            return 0.3f; // 蝓ｺ譛ｬ逧・↑BGM繝槭せ繧ｭ繝ｳ繧ｰ蠑ｷ蠎ｦ
        }

        /// <summary>
        /// 迺ｰ蠅・↓繧医ｋ繝槭せ繧ｭ繝ｳ繧ｰ蜉ｹ譫懊ｒ蜿門ｾ・        /// </summary>
        private float GetEnvironmentalMaskingAt(Vector3 position)
        {
            // DynamicAudioEnvironment 縺九ｉ迺ｰ蠅・ュ蝣ｱ繧貞叙蠕・            if (dynamicEnvironment != null)
            {
                return dynamicEnvironment.GetCurrentMaskingLevel();
            }
            return 0f;
        }

        /// <summary>
        /// 螟ｩ蛟吶↓繧医ｋ繝槭せ繧ｭ繝ｳ繧ｰ蜉ｹ譫・        /// </summary>
        private float GetWeatherMaskingEffect(WeatherType weather)
        {
            float weatherValue = (float)weather / (System.Enum.GetValues(typeof(WeatherType)).Length - 1);
            return weatherMaskingCurve.Evaluate(weatherValue);
        }

        /// <summary>
        /// 譎る俣蟶ｯ縺ｫ繧医ｋ繝槭せ繧ｭ繝ｳ繧ｰ蜉ｹ譫・        /// </summary>
        private float GetTimeMaskingEffect(TimeOfDay time)
        {
            float timeValue = (float)time / (System.Enum.GetValues(typeof(TimeOfDay)).Length - 1);
            return timeMaskingCurve.Evaluate(timeValue);
        }

        /// <summary>
        /// 繧ｫ繝・ざ繝ｪ縺ｫ繧医ｋ繝槭せ繧ｭ繝ｳ繧ｰ蛟咲紫
        /// </summary>
        private float GetCategoryMaskingMultiplier(AudioCategory category)
        {
            return category switch
            {
                AudioCategory.Stealth => 1f,   // 繧ｹ繝・Ν繧ｹ髻ｳ縺ｯ螳悟・縺ｪ繝槭せ繧ｭ繝ｳ繧ｰ蟇ｾ雎｡
                AudioCategory.Effect => 0.8f,  // 蜉ｹ譫憺浹縺ｯ驛ｨ蛻・噪縺ｫ繝槭せ繧ｯ
                AudioCategory.Ambient => 0.3f, // 迺ｰ蠅・浹縺ｯ霆ｽ縺上・繧ｹ繧ｯ
                AudioCategory.BGM => 0.1f,     // BGM縺ｯ谿・←繝槭せ繧ｯ縺輔ｌ縺ｪ縺・                AudioCategory.UI => 0f,        // UI髻ｳ縺ｯ繝槭せ繧ｯ縺輔ｌ縺ｪ縺・                _ => 1f
            };
        }

        #endregion

        #region Editor Helpers

#if UNITY_EDITOR
        [TabGroup("Stealth Coordinator", "Debug Tools")]
        [Button("Test Stealth Mode Activation")]
        public void TestStealthModeActivation()
        {
            SetOverrideStealthMode(!isStealthModeActive);
        }

        [TabGroup("Stealth Coordinator", "Debug Tools")]
        [Button("Calculate Current Masking")]
        public void DebugCalculateCurrentMasking()
        {
            if (playerTransform != null)
            {
                var testData = AudioEventData.CreateStealthDefault("DebugTest");
                testData.worldPosition = playerTransform.position;
                
                float masking = CalculateMaskingEffect(playerTransform.position, testData);
                { 
                    var eventLogger = ServiceLocator.GetService<IEventLogger>(); 
                    if (eventLogger != null) eventLogger.Log($"Current masking level at player position: {masking:F2}");
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (playerTransform == null) return;

            // 繝励Ξ繧､繝､繝ｼ髫繧檎ｯ・峇縺ｮ陦ｨ遉ｺ
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(playerTransform.position, playerHidingRadius);

            // 驥崎ｦ√い繧ｯ繧ｷ繝ｧ繝ｳ遽・峇縺ｮ陦ｨ遉ｺ
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(playerTransform.position, criticalActionRadius);

            // 霑代￥縺ｮAI縺ｮ陦ｨ遉ｺ
            Gizmos.color = Color.yellow;
            foreach (var ai in nearbyAI)
            {
                if (ai != null)
                {
                    Gizmos.DrawLine(playerTransform.position, ai.position);
                    Gizmos.DrawWireCube(ai.position, Vector3.one);
                }
            }
        }
#endif

        #region IStealthAudioService Implementation

        /// <summary>
        /// 逶ｮ讓咎＃謌先凾縺ｮ繧ｵ繧ｦ繝ｳ繝峨ｒ蜀咲函
        /// </summary>
        /// <param name="withBonus">繝懊・繝翫せ莉倥″縺九←縺・°</param>
        public void PlayObjectiveCompleteSound(bool withBonus)
        {
            if (!IsInitialized || audioService == null) 
            {
                var eventLogger = ServiceLocator.GetService<IEventLogger>();
                if (eventLogger != null && FeatureFlags.EnableDebugLogging)
                {
                    eventLogger.LogWarning("[StealthAudioCoordinator] Cannot play objective complete sound - system not initialized");
                }
                return;
            }

            try
            {
                // Determine which sound effect to play based on bonus status
                string soundEffect = withBonus ? "objective_complete_bonus" : "objective_complete";
                float volume = withBonus ? 1.0f : 0.8f;
                
                // Play the objective complete sound effect
                // Using the effect category for objective completion sounds
                audioService.PlaySound(soundEffect, 
                    playerTransform != null ? playerTransform.position : Vector3.zero, 
                    volume);;
                
                // If in stealth mode, apply appropriate volume adjustments
                if (isStealthModeActive)
                {
                    // Apply stealth mode volume reduction to maintain stealth atmosphere
                    float stealthVolume = volume * (1f - effectReductionAmount);
                    audioService.SetCategoryVolume("effect", stealthVolume);
                }
                
                // Log the action if debug logging is enabled
                if (FeatureFlags.EnableDebugLogging)
                {
                    var eventLogger = ServiceLocator.GetService<IEventLogger>();
                    if (eventLogger != null)
                    {
                        eventLogger.Log($"[StealthAudioCoordinator] Objective complete sound played: withBonus={withBonus}, volume={volume:F2}");
                    }
                }
                
                // Raise event for objective completion if events are configured
                if (withBonus && stealthModeActivatedEvent != null)
                {
                    // Could trigger a special event for bonus objectives
                    // This is optional and can be customized based on game requirements
                }
            }
            catch (System.Exception ex)
            {
                var eventLogger = ServiceLocator.GetService<IEventLogger>();
                if (eventLogger != null)
                {
                    eventLogger.LogError($"[StealthAudioCoordinator] Failed to play objective complete sound: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 繝励Ξ繧､繝､繝ｼ縺碁國阡ｽ迥ｶ諷九↓蜈･縺｣縺滓凾縺ｮ髻ｳ髻ｿ隱ｿ謨ｴ
        /// </summary>
        /// <param name="concealmentLevel">髫阡ｽ繝ｬ繝吶Ν (0.0 - 1.0)</param>
        public void OnPlayerConcealed(float concealmentLevel)
        {
            if (!IsInitialized)
            {
                var eventLogger = ServiceLocator.GetService<IEventLogger>();
                if (eventLogger != null)
                {
                    eventLogger.LogWarning("[StealthAudioCoordinator] System not initialized for OnPlayerConcealed");
                }
                return;
            }

            // 髫阡ｽ繝ｬ繝吶Ν縺ｫ蠢懊§縺溘せ繝・Ν繧ｹ髻ｳ髻ｿ隱ｿ謨ｴ
            AdjustStealthAudio(concealmentLevel);

            // 繝槭せ繧ｭ繝ｳ繧ｰ蜉ｹ譫懊・驕ｩ逕ｨ
            ApplyAudioMasking(concealmentLevel * 0.8f);

            // 繧ｹ繝・Ν繧ｹ繝｢繝ｼ繝峨・蠑ｷ蛻ｶ譛牙柑蛹・            SetOverrideStealthMode(true);

            if (FeatureFlags.EnableDebugLogging)
            {
                var eventLogger = ServiceLocator.GetService<IEventLogger>();
                if (eventLogger != null)
                {
                    eventLogger.Log($"[StealthAudioCoordinator] Player concealed with level: {concealmentLevel:F2}");
                }
            }
        }

        /// <summary>
        /// 繝励Ξ繧､繝､繝ｼ縺碁愆蜃ｺ迥ｶ諷九↓縺ｪ縺｣縺滓凾縺ｮ髻ｳ髻ｿ隱ｿ謨ｴ
        /// </summary>
        public void OnPlayerExposed()
        {
            if (!IsInitialized)
            {
                var eventLogger = ServiceLocator.GetService<IEventLogger>();
                if (eventLogger != null)
                {
                    eventLogger.LogWarning("[StealthAudioCoordinator] System not initialized for OnPlayerExposed");
                }
                return;
            }

            // 繧ｹ繝・Ν繧ｹ髻ｳ髻ｿ隱ｿ謨ｴ繧定ｧ｣髯､
            AdjustStealthAudio(0f);

            // 繝槭せ繧ｭ繝ｳ繧ｰ蜉ｹ譫懊ｒ隗｣髯､
            ApplyAudioMasking(0f);

            // 繧ｹ繝・Ν繧ｹ繝｢繝ｼ繝峨が繝ｼ繝舌・繝ｩ繧､繝峨ｒ隗｣髯､
            ClearStealthModeOverride();

            if (FeatureFlags.EnableDebugLogging)
            {
                var eventLogger = ServiceLocator.GetService<IEventLogger>();
                if (eventLogger != null)
                {
                    eventLogger.Log("[StealthAudioCoordinator] Player exposed, audio adjustments cleared");
                }
            }
        }

        #endregion

        #endregion
    }
}
