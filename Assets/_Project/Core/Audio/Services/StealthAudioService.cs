using asterivo.Unity60.Core;
// using asterivo.Unity60.Core.Data;
using UnityEngine;
using System.Collections.Generic;
using asterivo.Unity60.Core.Audio.Data;
using asterivo.Unity60.Core.Events;
// using asterivo.Unity60.Core.Debug;
using Sirenix.OdinInspector;
using asterivo.Unity60.Core.Audio.Interfaces;

namespace asterivo.Unity60.Core.Audio.Services
{
    /// <summary>
    /// 繧ｹ繝・Ν繧ｹ繧ｪ繝ｼ繝・ぅ繧ｪ繧ｵ繝ｼ繝薙せ (ServiceLocator蟆ら畑)
    /// 蠕捺擂縺ｮStealthAudioCoordinator縺九ｉ螳悟・遘ｻ陦後＠縺滓眠螳溯｣・    /// Phase 3 Step 3.5 - ServiceLocator螳悟・遘ｻ陦檎沿
    /// </summary>
    public class StealthAudioService : MonoBehaviour, IStealthAudioService, IInitializable
    {
        [TabGroup("Stealth Service", "Settings")]
        [Header("AI Integration Settings")]
        [SerializeField, Range(0f, 1f)] private float aiAlertThreshold = 0.5f;
        [SerializeField, Range(1f, 10f)] private float playerHidingRadius = 3f;
        [SerializeField] private LayerMask aiLayerMask = -1;

        [TabGroup("Stealth Service", "Audio")]
        [Header("Audio Reduction Settings")]
        [SerializeField, Range(0f, 1f)] private float bgmReductionAmount = 0.4f;
        [SerializeField, Range(0f, 1f)] private float ambientReductionAmount = 0.6f;
        [SerializeField, Range(0f, 1f)] private float effectReductionAmount = 0.3f;

        [TabGroup("Stealth Service", "Events")]
        [Header("Event Integration")]
        [SerializeField] private GameEvent stealthModeActivatedEvent;
        [SerializeField] private GameEvent stealthModeDeactivatedEvent;
        [SerializeField] private GameEvent maskingLevelChangedEvent;

        [TabGroup("Stealth Service", "Runtime")]
        [Header("Player Reference")]
        [SerializeField] private Transform playerTransform;

        [Header("Runtime Information")]
        [SerializeField, ReadOnly] private bool isStealthModeActive;
        [SerializeField, ReadOnly] private float currentMaskingLevel;
        [SerializeField, ReadOnly] private int nearbyAlertAICount;
        [SerializeField, ReadOnly] private bool isServiceRegistered;

        // IInitializable螳溯｣・        public int Priority => 25;
        public bool IsInitialized { get; private set; }

        // 蜀・Κ迥ｶ諷狗ｮ｡逅・        private IAudioService audioService;
        private List<Transform> nearbyAI = new List<Transform>();
        private Dictionary<AudioCategory, float> categoryVolumeMultipliers = new Dictionary<AudioCategory, float>();
        
        // 繧ｹ繝・Ν繧ｹ讀懷・迥ｶ諷・        private bool previousStealthModeState = false;
        private float globalMaskingStrength = 0f;

        #region Unity Lifecycle

        private void Awake()
        {
            RegisterToServiceLocator();
        }

        private void Start()
        {
            Initialize();
        }

        private void Update()
        {
            if (!IsInitialized) return;

            UpdateNearbyAIDetection();
            UpdateStealthModeState();
            UpdateMaskingEffects();
        }

        private void OnDestroy()
        {
            UnregisterFromServiceLocator();
        }

        #endregion

        #region ServiceLocator Integration

        /// <summary>
        /// ServiceLocator縺ｸ縺ｮ逋ｻ骭ｲ
        /// </summary>
        private void RegisterToServiceLocator()
        {
            if (FeatureFlags.UseServiceLocator && FeatureFlags.MigrateStealthAudioCoordinator)
            {
                try
                {
                    ServiceLocator.RegisterService<IStealthAudioService>(this);
                    isServiceRegistered = true;
                    
                    if (FeatureFlags.EnableDebugLogging)
                    {
                        EventLogger.LogStatic("[StealthAudioService] Successfully registered to ServiceLocator as IStealthAudioService");
                    }
                }
                catch (System.Exception ex)
                {
                    ServiceLocator.GetService<IEventLogger>().LogError($"[StealthAudioService] Failed to register to ServiceLocator: {ex.Message}");
                    isServiceRegistered = false;
                }
            }
        }

        /// <summary>
        /// ServiceLocator縺九ｉ縺ｮ逋ｻ骭ｲ隗｣髯､
        /// </summary>
        private void UnregisterFromServiceLocator()
        {
            if (isServiceRegistered && FeatureFlags.UseServiceLocator)
            {
                try
                {
                    ServiceLocator.UnregisterService<IStealthAudioService>();
                    
                    if (FeatureFlags.EnableDebugLogging)
                    {
                        EventLogger.LogStatic("[StealthAudioService] Unregistered from ServiceLocator");
                    }
                }
                catch (System.Exception ex)
                {
                    ServiceLocator.GetService<IEventLogger>().LogError($"[StealthAudioService] Failed to unregister from ServiceLocator: {ex.Message}");
                }
                finally
                {
                    isServiceRegistered = false;
                }
            }
        }

        #endregion

        #region IInitializable Implementation

        public void Initialize()
        {
            if (IsInitialized) return;
            
            // 繝励Ξ繧､繝､繝ｼ蜿ら・縺ｮ蜿門ｾ・            FindPlayerReference();
            
            // AudioService縺ｮ蜿門ｾ・            GetAudioServiceReference();
            
            // 繧ｫ繝・ざ繝ｪ蛻･髻ｳ驥丞咲紫縺ｮ蛻晄悄蛹・            InitializeCategoryMultipliers();
            
            IsInitialized = true;
            
            if (FeatureFlags.EnableDebugLogging)
            {
                EventLogger.LogStatic("[StealthAudioService] Initialization complete");
            }
        }

        /// <summary>
        /// 繝励Ξ繧､繝､繝ｼ蜿ら・縺ｮ讀懃ｴ｢ (SerializeField邨檎罰 - 繧｢繝ｼ繧ｭ繝・け繝√Ε貅匁侠)
        /// </summary>
        private void FindPlayerReference()
        {
            // Note: Core螻､縺九ｉFeatures螻､縺ｸ縺ｮ逶ｴ謗･蜿ら・縺ｯ繧｢繝ｼ繧ｭ繝・け繝√Ε驕募渚縺ｮ縺溘ａ
            // SerializeField 縺ｫ繧医ｋ Inspector險ｭ螳壹ｒ謗ｨ螂ｨ
            if (playerTransform == null)
            {
                ServiceLocator.GetService<IEventLogger>().LogWarning("[StealthAudioService] Player Transform not assigned! Please set in Inspector.");
            }
            else
            {
                if (FeatureFlags.EnableDebugLogging)
                {
                    EventLogger.LogStatic("[StealthAudioService] Player reference found via Inspector");
                }
            }
        }

        /// <summary>
        /// AudioService蜿ら・縺ｮ蜿門ｾ・        /// </summary>
        private void GetAudioServiceReference()
        {
            if (FeatureFlags.UseServiceLocator)
            {
                try
                {
                    audioService = ServiceLocator.GetService<IAudioService>();
                    
                    if (audioService != null && FeatureFlags.EnableDebugLogging)
                    {
                        EventLogger.LogStatic("[StealthAudioService] Successfully retrieved AudioService from ServiceLocator");
                    }
                }
                catch (System.Exception ex)
                {
                    ServiceLocator.GetService<IEventLogger>().LogError($"[StealthAudioService] Failed to retrieve AudioService from ServiceLocator: {ex.Message}");
                }
            }
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

        #endregion

        #region IStealthAudioService Implementation

        public void CreateFootstep(Vector3 position, float intensity, string surfaceType)
        {
            if (!IsInitialized) return;
            
            // 雜ｳ髻ｳ逕滓・繝ｭ繧ｸ繝・け (蠕捺擂縺ｮStealthAudioCoordinator縺九ｉ遘ｻ讀・
            if (FeatureFlags.EnableDebugLogging)
            {
                EventLogger.LogStatic($"[StealthAudioService] Creating footstep at {position}, intensity: {intensity}, surface: {surfaceType}");
            }

            // TODO: 螳滄圀縺ｮ雜ｳ髻ｳ逕滓・螳溯｣・            // - 陦ｨ髱｢繧ｿ繧､繝励↓蠢懊§縺滄浹螢ｰ驕ｸ謚・            // - 繧､繝ｳ繝・Φ繧ｷ繝・ぅ縺ｫ蝓ｺ縺･縺城浹驥剰ｪｿ謨ｴ
            // - NPC縺ｮ閨ｴ隕壹そ繝ｳ繧ｵ繝ｼ縺ｸ縺ｮ騾夂衍
        }

        public void SetEnvironmentNoiseLevel(float level)
        {
            if (!IsInitialized) return;
            
            globalMaskingStrength = Mathf.Clamp01(level);
            
            if (FeatureFlags.EnableDebugLogging)
            {
                EventLogger.LogStatic($"[StealthAudioService] Environment noise level set to: {level}");
            }
        }

        public void EmitDetectableSound(Vector3 position, float radius, float intensity, string soundType)
        {
            if (!IsInitialized) return;
            
            NotifyAuditorySensors(position, radius, intensity);
            
            if (FeatureFlags.EnableDebugLogging)
            {
                EventLogger.LogStatic($"[StealthAudioService] Detectable sound emitted: {soundType} at {position}");
            }
        }

        public void PlayDistraction(Vector3 position, float radius)
        {
            if (!IsInitialized) return;
            
            EmitDetectableSound(position, radius, 0.8f, "Distraction");
        }

        public void SetAlertLevelMusic(AlertLevel level)
        {
            if (!IsInitialized) return;
            
            string bgmName = level switch
            {
                AlertLevel.Relaxed => "Normal",
                AlertLevel.Suspicious => "Suspicious",
                AlertLevel.Investigating => "Alert",
                AlertLevel.Alert => "Combat",
                _ => "Normal"
            };
            
            // TODO: IBGMService縺悟ｿ・ｦ・            // audioService.PlayBGM(bgmName); 
            
            if (FeatureFlags.EnableDebugLogging)
            {
                EventLogger.LogStatic($"[StealthAudioService] Alert level music set: {level} -> {bgmName}");
            }
        }

        public void ApplyAudioMasking(float maskingLevel)
        {
            if (!IsInitialized) return;
            
            currentMaskingLevel = Mathf.Clamp01(maskingLevel);
            
            if (maskingLevelChangedEvent != null)
            {
                maskingLevelChangedEvent.Raise();
            }

            if (FeatureFlags.EnableDebugLogging)
            {
                EventLogger.LogStatic($"[StealthAudioService] Audio masking applied: {maskingLevel}");
            }
        }

        public void NotifyAuditorySensors(Vector3 origin, float radius, float intensity)
        {
            if (!IsInitialized) return;
            
            // AI閨ｴ隕壹そ繝ｳ繧ｵ繝ｼ縺ｸ縺ｮ騾夂衍繝ｭ繧ｸ繝・け
            if (FeatureFlags.EnableDebugLogging)
            {
                EventLogger.LogStatic($"[StealthAudioService] Notifying auditory sensors: origin={origin}, radius={radius}, intensity={intensity}");
            }

            // TODO: AI 繧ｷ繧ｹ繝・Β縺ｨ縺ｮ騾｣謳ｺ螳溯｣・        }

        public void AdjustStealthAudio(float stealthLevel)
        {
            if (!IsInitialized || audioService == null) return;
            
            float volumeReduction = 1f - (stealthLevel * 0.3f);
            
            try
            {
                audioService.SetCategoryVolume("bgm", bgmReductionAmount * volumeReduction);
                audioService.SetCategoryVolume("ambient", ambientReductionAmount * volumeReduction);
                audioService.SetCategoryVolume("effect", effectReductionAmount * volumeReduction);
                
                if (FeatureFlags.EnableDebugLogging)
                {
                    EventLogger.LogStatic($"[StealthAudioService] Stealth audio adjusted: level={stealthLevel}, reduction={volumeReduction}");
                }
            }
            catch (System.Exception ex)
            {
                ServiceLocator.GetService<IEventLogger>().LogError($"[StealthAudioService] Failed to adjust stealth audio: {ex.Message}");
            }
        }

        public void PlayObjectiveCompleteSound(bool withBonus)
        {
            if (!IsInitialized || audioService == null) return;
            
            try
            {
                string soundEffect = withBonus ? "objective_complete_bonus" : "objective_complete";
                float volume = withBonus ? 1.0f : 0.8f;
                
                // Play the objective complete sound effect
                audioService.PlaySound(soundEffect, Vector3.zero, volume);
                
                if (FeatureFlags.EnableDebugLogging)
                {
                    EventLogger.LogStatic($"[StealthAudioService] Objective complete sound played: withBonus={withBonus}");
                }
            }
            catch (System.Exception ex)
            {
                ServiceLocator.GetService<IEventLogger>().LogError($"[StealthAudioService] Failed to play objective complete sound: {ex.Message}");
            }
        }

        #endregion

        #region Public Interface

        /// <summary>
        /// 髱槭せ繝・Ν繧ｹ髻ｳ髻ｿ繧呈椛蛻ｶ縺吶∋縺阪°繧貞愛螳・        /// </summary>
        public bool ShouldReduceNonStealthAudio()
        {
            if (!IsInitialized) return false;

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
            if (!IsInitialized) return 0f;

            float totalMasking = globalMaskingStrength;

            // 迺ｰ蠅・ｦ∝屏縺ｫ繧医ｋ繝槭せ繧ｭ繝ｳ繧ｰ蜉ｹ譫懊ｒ霑ｽ蜉
            // TODO: 繧医ｊ隧ｳ邏ｰ縺ｪ繝槭せ繧ｭ繝ｳ繧ｰ險育ｮ励Ο繧ｸ繝・け繧貞ｮ溯｣・
            return Mathf.Clamp01(totalMasking);
        }

        /// <summary>
        /// NPC縺ｮ閨ｴ隕壹す繧ｹ繝・Β縺ｸ縺ｮ蠖ｱ髻ｿ蠎ｦ繧定ｨ育ｮ・        /// </summary>
        public float GetNPCAudibilityMultiplier(AudioEventData audioData)
        {
            if (!IsInitialized || !audioData.affectsStealthGameplay)
                return 0f;

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
        /// 繧ｫ繝・ざ繝ｪ髻ｳ驥丞咲紫繧貞叙蠕・        /// </summary>
        public float GetCategoryVolumeMultiplier(AudioCategory category)
        {
            return categoryVolumeMultipliers.TryGetValue(category, out float multiplier) ? multiplier : 1f;
        }

        #endregion

        #region Private Methods

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

                    // AI縺ｮ隴ｦ謌偵Ξ繝吶Ν繧堤｢ｺ隱・                    var aiController = collider.GetComponent<IGameStateProvider>();
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

                if (FeatureFlags.EnableDebugLogging)
                {
                    EventLogger.LogStatic($"<color=orange>[StealthAudioService]</color> Stealth mode {(newStealthMode ? "activated" : "deactivated")}");
                }
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
        /// 繝励Ξ繧､繝､繝ｼ縺碁國繧後Δ繝ｼ繝峨°繧貞愛螳・        /// </summary>
        private bool IsPlayerInHidingMode()
        {
            if (playerTransform == null) return false;

            // 繝励Ξ繧､繝､繝ｼ繧ｳ繝ｳ繝医Ο繝ｼ繝ｩ繝ｼ縺九ｉ縺ｮ迥ｶ諷句叙蠕励ｒ隧ｦ縺ｿ繧・            var playerController = playerTransform.GetComponent<IGameStateProvider>();
            if (playerController != null)
            {
                return playerController.IsInHidingMode();
            }

            // 繝輔か繝ｼ繝ｫ繝舌ャ繧ｯ: 霑代￥縺ｮ髫繧悟ｴ謇繧ｪ繝悶ず繧ｧ繧ｯ繝医ｒ繝√ぉ繝・け
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

            // TODO: 驥崎ｦ√↑繧ｹ繝・Ν繧ｹ繧｢繧ｯ繧ｷ繝ｧ繝ｳ縺ｮ蛻､螳壹Ο繧ｸ繝・け繧貞ｮ溯｣・            return false;
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

        #endregion

        #region Editor Support

#if UNITY_EDITOR
        [TabGroup("Stealth Service", "Debug")]
        [Button("Test Stealth Mode")]
        private void TestStealthMode()
        {
            isStealthModeActive = !isStealthModeActive;
            UpdateCategoryVolumeMultipliers();
            
            if (FeatureFlags.EnableDebugLogging)
            {
                EventLogger.LogStatic($"[StealthAudioService] Stealth mode {(isStealthModeActive ? "activated" : "deactivated")}");
            }
        }

        [Button("Test Service Registration")]
        private void TestServiceRegistration()
        {
            var service = ServiceLocator.GetService<IStealthAudioService>();
            if (service != null)
            {
                EventLogger.LogStatic($"[StealthAudioService] 笨・Service successfully retrieved from ServiceLocator");
            }
            else
            {
                ServiceLocator.GetService<IEventLogger>().LogError($"[StealthAudioService] 笶・Service not found in ServiceLocator");
            }
        }

        [Button("Force Initialize")]
        private void ForceInitialize()
        {
            IsInitialized = false;
            Initialize();
        }

        private void OnDrawGizmosSelected()
        {
            if (playerTransform == null) return;

            // 繝励Ξ繧､繝､繝ｼ髫繧檎ｯ・峇縺ｮ陦ｨ遉ｺ
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(playerTransform.position, playerHidingRadius);

            // AI讀懷・遽・峇縺ｮ陦ｨ遉ｺ
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(playerTransform.position, playerHidingRadius * 2f);

            // 霑代￥縺ｮAI縺ｮ陦ｨ遉ｺ
            Gizmos.color = Color.red;
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

        #endregion
    }
}