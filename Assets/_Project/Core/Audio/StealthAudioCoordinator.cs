using asterivo.Unity60.Core;
using UnityEngine;
using System.Collections.Generic;
using asterivo.Unity60.Core.Audio.Data;
using asterivo.Unity60.Core.Events;
// using asterivo.Unity60.Core.Debug;
using asterivo.Unity60.Core.Audio.Interfaces;
using asterivo.Unity60.Core.Data;
// using asterivo.Unity60.Core.Services; // Removed to avoid circular dependency
using Sirenix.OdinInspector;
// using asterivo.Unity60.Core.Helpers;

namespace asterivo.Unity60.Core.Audio
{
    /// <summary>
    /// Stealth gameplay and general audio coordination control
    /// Integration with NPC auditory sensor system
    /// ServiceLocator compatible version
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

        // Internal state management
        private Transform playerTransform;
        private List<Transform> nearbyAI = new List<Transform>();
        private Dictionary<AudioCategory, float> categoryVolumeMultipliers = new Dictionary<AudioCategory, float>();
        private bool previousStealthModeState = false;
        private float globalMaskingStrength = 0f;

        // System integration (via ServiceLocator)
        private IAudioService audioService;
        private AudioManager audioManager;
        private DynamicAudioEnvironment dynamicEnvironment;

        // Note: Singleton pattern completely removed - ServiceLocator-only implementation
        // Initialization state management
        public bool IsInitialized { get; private set; }

        #region Unity Lifecycle

        private void Awake()
        {
            // Note: ServiceLocator-only implementation - Singleton pattern completely removed
            DontDestroyOnLoad(gameObject);

            // Register to ServiceLocator
            if (asterivo.Unity60.Core.FeatureFlags.UseServiceLocator)
            {
                try
                {
                    ServiceLocator.RegisterService<IStealthAudioService>(this);

                    if (asterivo.Unity60.Core.FeatureFlags.EnableDebugLogging)
                    {
                        var eventLogger = ServiceLocator.GetService<IEventLogger>();
                        if (eventLogger != null)
                        {
                            eventLogger.Log("[StealthAudioCoordinator] Successfully registered to ServiceLocator as IStealthAudioService");
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    var eventLogger = ServiceLocator.GetService<IEventLogger>();
                    if (eventLogger != null)
                    {
                        eventLogger.LogError($"[StealthAudioCoordinator] Failed to register to ServiceLocator: {ex.Message}");
                    }
                }
            }
            else
            {
                var eventLogger = ServiceLocator.GetService<IEventLogger>();
                if (eventLogger != null)
                {
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
            // Note: ServiceLocator-only implementation - Singleton pattern completely removed
            // Unregister from ServiceLocator
            if (asterivo.Unity60.Core.FeatureFlags.UseServiceLocator)
            {
                try
                {
                    ServiceLocator.UnregisterService<IStealthAudioService>();

                    if (asterivo.Unity60.Core.FeatureFlags.EnableDebugLogging)
                    {
                        var eventLogger = ServiceLocator.GetService<IEventLogger>();
                        if (eventLogger != null)
                        {
                            eventLogger.Log("[StealthAudioCoordinator] Unregistered from ServiceLocator");
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    var eventLogger = ServiceLocator.GetService<IEventLogger>();
                    if (eventLogger != null)
                    {
                        eventLogger.LogError($"[StealthAudioCoordinator] Failed to unregister from ServiceLocator: {ex.Message}");
                    }
                }
            }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize coordinator
        /// </summary>
        private void InitializeCoordinator()
        {
            // Search for player transform
            var player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
            else
            {
                var eventLogger = ServiceLocator.GetService<IEventLogger>();
                if (eventLogger != null)
                {
                    eventLogger.LogWarning("[StealthAudioCoordinator] Player object not found! Please assign a Player tag.");
                }
            }
        }

        /// <summary>
        /// Find system references
        /// Phase 3 migration pattern: ServiceLocator priority, Singleton fallback
        /// </summary>
        private void FindSystemReferences()
        {
            // Get AudioService via ServiceLocator
            if (asterivo.Unity60.Core.FeatureFlags.UseServiceLocator && audioService == null)
            {
                try
                {
                    audioService = ServiceLocator.GetService<IAudioService>();

                    if (audioService != null)
                    {
                        if (asterivo.Unity60.Core.FeatureFlags.EnableDebugLogging)
                        {
                            var eventLogger = ServiceLocator.GetService<IEventLogger>();
                            if (eventLogger != null)
                            {
                                eventLogger.Log("[StealthAudioCoordinator] Successfully retrieved AudioService from ServiceLocator");
                            }
                        }
                    }
                    else
                    {
                        if (asterivo.Unity60.Core.FeatureFlags.EnableMigrationMonitoring)
                        {
                            var eventLogger = ServiceLocator.GetService<IEventLogger>();
                            if (eventLogger != null)
                            {
                                eventLogger.LogWarning("[StealthAudioCoordinator] ServiceLocator returned null for IAudioService, falling back to legacy approach");
                            }
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    var eventLogger = ServiceLocator.GetService<IEventLogger>();
                    if (eventLogger != null)
                    {
                        eventLogger.LogError($"[StealthAudioCoordinator] Failed to retrieve AudioService from ServiceLocator: {ex.Message}");
                    }
                }
            }

            // Note: ServiceLocator-only implementation - Search for AudioManager directly
            if (audioManager == null)
            {
                try
                {
                    audioManager = ServiceHelper.GetServiceWithFallback<AudioManager>();
                    if (audioManager != null && asterivo.Unity60.Core.FeatureFlags.EnableDebugLogging)
                    {
                        var eventLogger = ServiceLocator.GetService<IEventLogger>();
                        if (eventLogger != null)
                        {
                            eventLogger.Log("[StealthAudioCoordinator] Found AudioManager via FindFirstObjectByType");
                        }
                    }
                    else if (audioManager == null)
                    {
                        var eventLogger = ServiceLocator.GetService<IEventLogger>();
                        if (eventLogger != null)
                        {
                            eventLogger.LogError("[StealthAudioCoordinator] No AudioManager found");
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    var eventLogger = ServiceLocator.GetService<IEventLogger>();
                    if (eventLogger != null)
                    {
                        eventLogger.LogError($"[StealthAudioCoordinator] Failed to get AudioManager: {ex.Message}");
                    }
                }
            }

            if (dynamicEnvironment == null)
                dynamicEnvironment = ServiceHelper.GetServiceWithFallback<DynamicAudioEnvironment>();
        }

        /// <summary>
        /// Initialize category volume multipliers
        /// </summary>
        private void InitializeCategoryMultipliers()
        {
            categoryVolumeMultipliers[AudioCategory.BGM] = 1f;
            categoryVolumeMultipliers[AudioCategory.Ambient] = 1f;
            categoryVolumeMultipliers[AudioCategory.Effect] = 1f;
            categoryVolumeMultipliers[AudioCategory.Stealth] = 1f;
            categoryVolumeMultipliers[AudioCategory.UI] = 1f;
        }

        /// <summary>
        /// IInitializable implementation - Initialize stealth audio coordinator
        /// </summary>
        public void Initialize()
        {
            if (IsInitialized) return;

            FindSystemReferences();
            InitializeCategoryMultipliers();

            IsInitialized = true;

            // Debug log (simplified temporarily)
            {
                var eventLogger = ServiceLocator.GetService<IEventLogger>();
                if (eventLogger != null) eventLogger.Log("[StealthAudioCoordinator] Initialization complete");
            }
        }

        #endregion

        #region IStealthAudioService Implementation

        /// <summary>
        /// Create footstep sound
        /// </summary>
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

            // TODO: Create footstep based on surface type
            // Debug log output
            {
                var eventLogger = ServiceLocator.GetService<IEventLogger>();
                if (eventLogger != null) eventLogger.Log($"[StealthAudioCoordinator] Creating footstep at {position}, intensity: {intensity}, surface: {surfaceType}");
            }
        }

        /// <summary>
        /// Set environment noise level (for masking effect)
        /// </summary>
        public void SetEnvironmentNoiseLevel(float level)
        {
            if (!IsInitialized) return;

            globalMaskingStrength = Mathf.Clamp01(level);

            // Debug log output
            {
                var eventLogger = ServiceLocator.GetService<IEventLogger>();
                if (eventLogger != null) eventLogger.Log($"[StealthAudioCoordinator] Environment noise level set to: {level}");
            }
        }

        /// <summary>
        /// Emit sound detectable by NPCs
        /// </summary>
        public void EmitDetectableSound(Vector3 position, float radius, float intensity, string soundType)
        {
            if (!IsInitialized) return;

            // TODO: Notify NPC auditory sensors
            NotifyAuditorySensors(position, radius, intensity);

            // Debug log output
            {
                var eventLogger = ServiceLocator.GetService<IEventLogger>();
                if (eventLogger != null) eventLogger.Log($"[StealthAudioCoordinator] Detectable sound emitted: {soundType} at {position}");
            }
        }

        /// <summary>
        /// Play distraction sound
        /// </summary>
        public void PlayDistraction(Vector3 position, float radius)
        {
            if (!IsInitialized) return;

            EmitDetectableSound(position, radius, 0.8f, "Distraction");
        }

        /// <summary>
        /// Set BGM according to alert level
        /// </summary>
        public void SetAlertLevelMusic(AlertLevel level)
        {
            if (!IsInitialized || audioService == null) return;

            // TODO: Switch BGM according to alert level
            string bgmName = level switch
            {
                AlertLevel.Relaxed => "Normal",
                AlertLevel.Suspicious => "Suspicious",
                AlertLevel.Investigating => "Alert",
                AlertLevel.Alert => "Combat",
                _ => "Normal"
            };

            // audioService.PlayBGM(bgmName); // TODO: IBGMService needed
            // Debug log output
            {
                var eventLogger = ServiceLocator.GetService<IEventLogger>();
                if (eventLogger != null) eventLogger.Log($"[StealthAudioCoordinator] Alert level music set: {level} -> {bgmName}");
            }
        }

        /// <summary>
        /// Apply audio masking effect
        /// </summary>
        public void ApplyAudioMasking(float maskingLevel)
        {
            if (!IsInitialized) return;

            currentMaskingLevel = Mathf.Clamp01(maskingLevel);

            // Use existing masking system
            UpdateMaskingEffects();

            if (maskingLevelChangedEvent != null)
            {
                maskingLevelChangedEvent.Raise();
            }
        }

        /// <summary>
        /// Notify NPC auditory sensors of sound event
        /// </summary>
        public void NotifyAuditorySensors(Vector3 origin, float radius, float intensity)
        {
            if (!IsInitialized) return;

            // TODO: AI system integration implementation
            // Debug log output
            {
                var eventLogger = ServiceLocator.GetService<IEventLogger>();
                if (eventLogger != null) eventLogger.Log($"[StealthAudioCoordinator] Notifying auditory sensors: origin={origin}, radius={radius}, intensity={intensity}");
            }
        }

        /// <summary>
        /// Adjust audio based on player stealth
        /// </summary>
        public void AdjustStealthAudio(float stealthLevel)
        {
            if (!IsInitialized) return;

            // Adjust volume based on stealth level
            float volumeReduction = 1f - (stealthLevel * 0.3f); // Max 30% reduction

            if (audioService != null)
            {
                audioService.SetCategoryVolume("bgm", bgmReductionAmount * volumeReduction);
                audioService.SetCategoryVolume("ambient", ambientReductionAmount * volumeReduction);
                audioService.SetCategoryVolume("effect", effectReductionAmount * volumeReduction);
            }

            // Debug log output
            {
                var eventLogger = ServiceLocator.GetService<IEventLogger>();
                if (eventLogger != null) eventLogger.Log($"[StealthAudioCoordinator] Stealth audio adjusted: level={stealthLevel}, reduction={volumeReduction}");
            }
        }

        #endregion

        #region Stealth Mode Detection

        /// <summary>
        /// Update nearby AI detection
        /// </summary>
        private void UpdateNearbyAIDetection()
        {
            if (playerTransform == null) return;

            nearbyAI.Clear();
            nearbyAlertAICount = 0;

            // Search for nearby AI agents
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

                    // Check AI alert level
                    var aiController = collider.GetComponent<IGameStateProvider>();
                    if (aiController != null && aiController.GetAlertLevel() > aiAlertThreshold)
                    {
                        nearbyAlertAICount++;
                    }
                }
            }
        }

        /// <summary>
        /// Update stealth mode state
        /// </summary>
        private void UpdateStealthModeState()
        {
            bool newStealthMode = ShouldReduceNonStealthAudio();

            if (newStealthMode != previousStealthModeState)
            {
                isStealthModeActive = newStealthMode;
                previousStealthModeState = newStealthMode;

                // Update volume multipliers
                UpdateCategoryVolumeMultipliers();

                // Raise events
                if (newStealthMode)
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
        /// Determine if non-stealth audio should be reduced
        /// </summary>
        public bool ShouldReduceNonStealthAudio()
        {
            if (isOverrideActive)
                return isStealthModeActive;

            // Player is in hiding mode
            if (IsPlayerInHidingMode())
                return true;

            // Nearby AI are alert
            if (nearbyAlertAICount > 0)
                return true;

            // Performing critical stealth action
            if (IsPerformingCriticalStealthAction())
                return true;

            return false;
        }

        /// <summary>
        /// Calculate masking effect
        /// </summary>
        public float CalculateMaskingEffect(Vector3 soundPosition, AudioEventData audioData)
        {
            float totalMasking = baseMaskingStrength;

            // BGM masking
            if (IsBGMPlaying())
            {
                totalMasking += GetBGMMaskingStrength() * 0.3f;
            }

            // Environmental masking
            totalMasking += GetEnvironmentalMaskingAt(soundPosition) * 0.5f;

            // Weather and time masking
            if (dynamicEnvironment != null)
            {
                var (env, weather, time) = dynamicEnvironment.GetCurrentState();
                totalMasking += GetWeatherMaskingEffect(weather) * 0.4f;
                totalMasking += GetTimeMaskingEffect(time) * 0.2f;
            }

            // Category-based adjustment
            totalMasking *= GetCategoryMaskingMultiplier(audioData.category);

            return Mathf.Clamp01(totalMasking);
        }

        /// <summary>
        /// Calculate influence on NPC auditory system
        /// </summary>
        public float GetNPCAudibilityMultiplier(AudioEventData audioData)
        {
            if (!audioData.affectsStealthGameplay)
                return 0f; // NPCs don't detect sounds that don't affect stealth

            float maskingEffect = CalculateMaskingEffect(audioData.worldPosition, audioData);
            float audibilityMultiplier = 1f - maskingEffect;

            // Additional reduction in stealth mode
            if (isStealthModeActive && audioData.canBeDuckedByTension)
            {
                audibilityMultiplier *= 0.7f;
            }

            return Mathf.Clamp01(audibilityMultiplier);
        }

        /// <summary>
        /// Get volume multiplier for audio category
        /// </summary>
        public float GetCategoryVolumeMultiplier(AudioCategory category)
        {
            return categoryVolumeMultipliers.TryGetValue(category, out float multiplier) ? multiplier : 1f;
        }

        /// <summary>
        /// Force stealth mode setting
        /// </summary>
        public void SetOverrideStealthMode(bool forceStealthMode)
        {
            isOverrideActive = true;
            isStealthModeActive = forceStealthMode;
            UpdateCategoryVolumeMultipliers();
        }

        /// <summary>
        /// Clear override
        /// </summary>
        public void ClearStealthModeOverride()
        {
            isOverrideActive = false;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Determine if player is in hiding mode
        /// </summary>
        private bool IsPlayerInHidingMode()
        {
            if (playerTransform == null) return false;

            // Try to get state from player controller
            var playerController = playerTransform.GetComponent<IGameStateProvider>();
            if (playerController != null)
            {
                return playerController.IsInHidingMode();
            }

            // Fallback: check for nearby hiding spot objects
            Collider[] hideSpots = Physics.OverlapSphere(
                playerTransform.position,
                playerHidingRadius,
                LayerMask.GetMask("HideSpot")
            );

            return hideSpots.Length > 0;
        }

        /// <summary>
        /// Determine if performing critical stealth action
        /// </summary>
        private bool IsPerformingCriticalStealthAction()
        {
            if (playerTransform == null) return false;

            // Check nearby critical action objects
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
                        // Check if object is in active state
                        var interactable = obj.GetComponent<IGameStateProvider>();
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
        /// Update category volume multipliers
        /// </summary>
        private void UpdateCategoryVolumeMultipliers()
        {
            if (isStealthModeActive)
            {
                categoryVolumeMultipliers[AudioCategory.BGM] = 1f - bgmReductionAmount;
                categoryVolumeMultipliers[AudioCategory.Ambient] = 1f - ambientReductionAmount;
                categoryVolumeMultipliers[AudioCategory.Effect] = 1f - effectReductionAmount;
                categoryVolumeMultipliers[AudioCategory.Stealth] = 1f; // Maintain stealth sounds
                categoryVolumeMultipliers[AudioCategory.UI] = 1f; // Maintain UI sounds
            }
            else
            {
                // Return to normal state
                categoryVolumeMultipliers[AudioCategory.BGM] = 1f;
                categoryVolumeMultipliers[AudioCategory.Ambient] = 1f;
                categoryVolumeMultipliers[AudioCategory.Effect] = 1f;
                categoryVolumeMultipliers[AudioCategory.Stealth] = 1f;
                categoryVolumeMultipliers[AudioCategory.UI] = 1f;
            }
        }

        /// <summary>
        /// Receive masking effect notification
        /// </summary>
        public void NotifyMaskingEffect(Vector3 position, float strength, float radius)
        {
            if (playerTransform == null) return;

            float distance = Vector3.Distance(playerTransform.position, position);
            if (distance > radius) return;

            // Record as masking effect around player
            // This information is used for audio system masking calculations
            float normalizedDistance = distance / radius;
            float effectiveStrength = strength * (1f - normalizedDistance);

            // Temporarily increase global masking strength
            globalMaskingStrength = Mathf.Max(globalMaskingStrength, effectiveStrength * 0.5f);

            {
                var eventLogger = ServiceLocator.GetService<IEventLogger>();
                if (eventLogger != null) eventLogger.Log($"<color=purple>[StealthAudioCoordinator]</color> Masking effect applied: strength={effectiveStrength:F2}, distance={distance:F1}m");
            }
        }

        /// <summary>
        /// Update masking effects
        /// </summary>
        private void UpdateMaskingEffects()
        {
            if (playerTransform == null) return;

            // Calculate masking effect at player position
            var dummyAudioData = AudioEventData.CreateDefault("MaskingCalculation");
            dummyAudioData.worldPosition = playerTransform.position;

            float newMaskingLevel = CalculateMaskingEffect(playerTransform.position, dummyAudioData);

            if (Mathf.Abs(newMaskingLevel - currentMaskingLevel) > 0.05f)
            {
                currentMaskingLevel = newMaskingLevel;
                maskingLevelChangedEvent?.Raise();
            }
        }

        /// <summary>
        /// Check if BGM is playing
        /// </summary>
        private bool IsBGMPlaying()
        {
            // Get BGM playback state from AudioManager
            if (audioManager != null)
            {
                // Implementation depends on AudioManager API
                return true; // Placeholder
            }
            return false;
        }

        /// <summary>
        /// Get BGM masking strength
        /// </summary>
        private float GetBGMMaskingStrength()
        {
            return 0.3f; // Base BGM masking strength
        }

        /// <summary>
        /// Get environmental masking effect
        /// </summary>
        private float GetEnvironmentalMaskingAt(Vector3 position)
        {
            // Get environmental info from DynamicAudioEnvironment
            if (dynamicEnvironment != null)
            {
                return dynamicEnvironment.GetCurrentMaskingLevel();
            }
            return 0f;
        }

        /// <summary>
        /// Get weather masking effect
        /// </summary>
        private float GetWeatherMaskingEffect(WeatherType weather)
        {
            float weatherValue = (float)weather / (System.Enum.GetValues(typeof(WeatherType)).Length - 1);
            return weatherMaskingCurve.Evaluate(weatherValue);
        }

        /// <summary>
        /// Get time masking effect
        /// </summary>
        private float GetTimeMaskingEffect(TimeOfDay time)
        {
            float timeValue = (float)time / (System.Enum.GetValues(typeof(TimeOfDay)).Length - 1);
            return timeMaskingCurve.Evaluate(timeValue);
        }

        /// <summary>
        /// Get category masking multiplier
        /// </summary>
        private float GetCategoryMaskingMultiplier(AudioCategory category)
        {
            return category switch
            {
                AudioCategory.Stealth => 1f,   // Stealth sounds fully masked
                AudioCategory.Effect => 0.8f,   // Effects partially masked
                AudioCategory.Ambient => 0.3f,  // Ambient lightly masked
                AudioCategory.BGM => 0.1f,      // BGM barely masked
                AudioCategory.UI => 0f,         // UI sounds not masked
                _ => 1f
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

            // Show player hiding radius
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(playerTransform.position, playerHidingRadius);

            // Show critical action radius
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(playerTransform.position, criticalActionRadius);

            // Show nearby AI
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
        /// Play objective complete sound
        /// </summary>
        /// <param name="withBonus">Whether with bonus or not</param>
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
                    volume);

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
        /// Audio adjustment when player enters concealment state
        /// </summary>
        /// <param name="concealmentLevel">Concealment level (0.0 - 1.0)</param>
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

            // Adjust stealth audio based on concealment level
            AdjustStealthAudio(concealmentLevel);

            // Apply masking effect
            ApplyAudioMasking(concealmentLevel * 0.8f);

            // Force enable stealth mode
            SetOverrideStealthMode(true);

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
        /// Audio adjustment when player becomes exposed
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

            // Clear stealth audio adjustment
            AdjustStealthAudio(0f);

            // Clear masking effect
            ApplyAudioMasking(0f);

            // Clear stealth mode override
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