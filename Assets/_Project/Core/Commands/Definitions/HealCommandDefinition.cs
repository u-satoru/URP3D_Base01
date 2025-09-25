using UnityEngine;
// using asterivo.Unity60.Core.Commands;

namespace asterivo.Unity60.Core.Commands.Definitions
{
    /// <summary>
    /// Heal command definition.
    /// Encapsulates healing actions for players or AI.
    ///
    /// Main features:
    /// - Health, Mana, Stamina recovery
    /// - Heal types management (instant, overtime, area)
    /// - Item and skill integration
    /// - Healing effects and animation control
    /// </summary>
    [System.Serializable]
    public class HealCommandDefinition : ICommandDefinition
    {
        /// <summary>
        /// Types of healing
        /// </summary>
        public enum HealType
        {
            Instant,        // Instant heal
            Overtime,       // Heal over time
            Area,           // Area heal
            Percentage,     // Percentage heal
            Full            // Full heal
        }

        /// <summary>
        /// Resource type to heal
        /// </summary>
        public enum ResourceType
        {
            Health,         // Health
            Mana,           // Mana
            Stamina,        // Stamina
            All             // All resources
        }

        [Header("Heal Parameters")]
        public HealType healType = HealType.Instant;
        public ResourceType targetResource = ResourceType.Health;
        public float healAmount = 50f;
        public float percentage = 0f; // Used for percentage healing (0-1)

        [Header("Overtime Settings")]
        [Tooltip("Total duration for heal over time")]
        public float duration = 5f;
        [Tooltip("Interval between heals")]
        public float tickInterval = 1f;

        [Header("Area Settings")]
        [Tooltip("Healing effect radius")]
        public float radius = 3f;
        [Tooltip("Target layer for area healing")]
        public LayerMask targetLayer = -1;
        [Tooltip("Maximum number of targets")]
        public int maxTargets = 5;

        [Header("Requirements")]
        public bool consumeItem = false;
        public string requiredItemId = "";
        public int requiredItemCount = 1;

        [Header("Cooldown")]
        public float cooldownTime = 1f;
        public bool sharedCooldown = false;
        public string cooldownGroup = "healing";

        [Header("Animation")]
        public bool playAnimation = true;
        public string animationTrigger = "Heal";
        public float animationDuration = 1f;

        [Header("Effects")]
        public bool showHealEffect = true;
        public GameObject healEffectPrefab;
        public Color healEffectColor = Color.green;
        public float effectDuration = 2f;

        [Header("Sound")]
        public bool playSound = true;
        public string healSoundName = "Heal";
        public float soundVolume = 1f;

        [Header("Restrictions")]
        public bool canSelfCast = true;
        public bool requiresTarget = false;
        public bool requiresLineOfSight = false;
        public float maxCastRange = 10f;

        /// <summary>
        /// Default constructor
        /// </summary>
        public HealCommandDefinition()
        {
        }

        /// <summary>
        /// Parameterized constructor
        /// </summary>
        public HealCommandDefinition(HealType type, ResourceType resource, float amount)
        {
            healType = type;
            targetResource = resource;
            healAmount = amount;
        }

        /// <summary>
        /// Create heal command instance
        /// </summary>
        public ICommand CreateCommand(object context = null)
        {
            return new HealCommand(this, context);
        }
    }

    /// <summary>
    /// Actual implementation of heal command
    /// </summary>
    public class HealCommand : ICommand
    {
        private readonly HealCommandDefinition definition;
        private readonly object context;
        private bool executed = false;
        private float healedAmount = 0f;
        private bool isActive = false; // For overtime healing

        public HealCommand(HealCommandDefinition healDefinition, object executionContext)
        {
            definition = healDefinition;
            context = executionContext;
        }

        /// <summary>
        /// Execute heal command
        /// </summary>
        public void Execute()
        {
            if (executed) return;

            // Check requirements
            if (!CheckRequirements())
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.LogWarning("Heal requirements not met");
#endif
                return;
            }

            // Get heal targets
            var targets = GetHealTargets();
            if (targets == null || targets.Length == 0)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.LogWarning("No valid heal targets found");
#endif
                return;
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Executing {definition.healType} heal for {definition.healAmount} on {targets.Length} targets");
#endif

            // Apply healing based on type
            switch (definition.healType)
            {
                case HealCommandDefinition.HealType.Instant:
                    ExecuteInstantHeal(targets);
                    break;
                case HealCommandDefinition.HealType.Overtime:
                    StartOvertimeHeal(targets);
                    break;
                case HealCommandDefinition.HealType.Area:
                    ExecuteAreaHeal();
                    break;
                case HealCommandDefinition.HealType.Percentage:
                    ExecutePercentageHeal(targets);
                    break;
                case HealCommandDefinition.HealType.Full:
                    ExecuteFullHeal(targets);
                    break;
            }

            // Consume items if required
            if (definition.consumeItem)
            {
                ConsumeRequiredItems();
            }

            // Play effects
            PlayEffects(targets);

            executed = true;
        }

        /// <summary>
        /// Check if command can be executed
        /// </summary>
        public bool CanExecute()
        {
            // Basic executability check
            if (definition.healAmount <= 0f && definition.healType != HealCommandDefinition.HealType.Full)
                return false;

            // Resource check
            if (definition.consumeItem && !HasRequiredItems())
                return false;

            // Cooldown check
            // TODO: Implement cooldown system integration

            return !executed;
        }

        /// <summary>
        /// Check if requirements are met
        /// </summary>
        private bool CheckRequirements()
        {
            if (definition.consumeItem)
            {
                return HasRequiredItems();
            }
            return true;
        }

        /// <summary>
        /// Check if required items are available
        /// </summary>
        private bool HasRequiredItems()
        {
            // TODO: Integrate with inventory system
            return true;
        }

        /// <summary>
        /// Consume required items
        /// </summary>
        private void ConsumeRequiredItems()
        {
            // TODO: Integrate with inventory system
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Consuming {definition.requiredItemCount} x {definition.requiredItemId}");
#endif
        }

        /// <summary>
        /// Get heal targets based on configuration
        /// </summary>
        private MonoBehaviour[] GetHealTargets()
        {
            if (context is MonoBehaviour mono)
            {
                if (definition.healType == HealCommandDefinition.HealType.Area)
                {
                    return GetAreaTargets(mono.transform.position);
                }
                else
                {
                    return new[] { mono };
                }
            }
            return null;
        }

        /// <summary>
        /// Get area heal targets
        /// </summary>
        private MonoBehaviour[] GetAreaTargets(Vector3 center)
        {
            Collider[] colliders = Physics.OverlapSphere(center, definition.radius, definition.targetLayer);
            System.Collections.Generic.List<MonoBehaviour> targets = new System.Collections.Generic.List<MonoBehaviour>();

            int count = 0;
            foreach (var collider in colliders)
            {
                if (count >= definition.maxTargets) break;

                var mono = collider.GetComponent<MonoBehaviour>();
                if (mono != null)
                {
                    targets.Add(mono);
                    count++;
                }
            }

            return targets.ToArray();
        }

        /// <summary>
        /// Execute instant healing
        /// </summary>
        private void ExecuteInstantHeal(MonoBehaviour[] targets)
        {
            foreach (var target in targets)
            {
                healedAmount += ApplyHealing(target, definition.healAmount);
            }
        }

        /// <summary>
        /// Start healing over time
        /// </summary>
        private void StartOvertimeHeal(MonoBehaviour[] targets)
        {
            isActive = true;
            float healPerTick = definition.healAmount / (definition.duration / definition.tickInterval);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Starting overtime heal: {healPerTick} per {definition.tickInterval}s for {definition.duration}s");
#endif
            // TODO: Implement actual overtime healing with coroutine or update system
        }

        /// <summary>
        /// Execute area healing
        /// </summary>
        private void ExecuteAreaHeal()
        {
            var targets = GetAreaTargets(((MonoBehaviour)context).transform.position);
            ExecuteInstantHeal(targets);
        }

        /// <summary>
        /// Execute percentage healing
        /// </summary>
        private void ExecutePercentageHeal(MonoBehaviour[] targets)
        {
            foreach (var target in targets)
            {
                // TODO: Get max health from health component
                float maxHealth = 100f; // Placeholder
                float healAmount = maxHealth * definition.percentage;
                healedAmount += ApplyHealing(target, healAmount);
            }
        }

        /// <summary>
        /// Execute full healing
        /// </summary>
        private void ExecuteFullHeal(MonoBehaviour[] targets)
        {
            foreach (var target in targets)
            {
                // TODO: Get and restore to max health
                float maxHealth = 100f; // Placeholder
                healedAmount += ApplyHealing(target, maxHealth);
            }
        }

        /// <summary>
        /// Apply healing to target
        /// </summary>
        private float ApplyHealing(MonoBehaviour target, float amount)
        {
            if (target == null) return 0f;

            // TODO: Integrate with actual health/resource system
            float actualHealAmount = amount;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Healing {target.name} for {actualHealAmount} {definition.targetResource}");
#endif

            // Apply to appropriate resource
            switch (definition.targetResource)
            {
                case HealCommandDefinition.ResourceType.Health:
                    // healthSystem.Heal(actualHealAmount);
                    break;
                case HealCommandDefinition.ResourceType.Mana:
                    // manaSystem.RestoreMana(actualHealAmount);
                    break;
                case HealCommandDefinition.ResourceType.Stamina:
                    // staminaSystem.RestoreStamina(actualHealAmount);
                    break;
                case HealCommandDefinition.ResourceType.All:
                    // Restore all resources
                    break;
            }

            return actualHealAmount;
        }

        /// <summary>
        /// Play healing effects
        /// </summary>
        private void PlayEffects(MonoBehaviour[] targets)
        {
            if (!definition.showHealEffect && !definition.playSound) return;

            foreach (var target in targets)
            {
                // Visual effect
                if (definition.showHealEffect && definition.healEffectPrefab != null)
                {
                    var effect = Object.Instantiate(definition.healEffectPrefab,
                        target.transform.position,
                        Quaternion.identity);
                    Object.Destroy(effect, definition.effectDuration);
                }

                // Sound effect
                if (definition.playSound)
                {
                    // TODO: Integrate with audio system
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    UnityEngine.Debug.Log($"Playing heal sound: {definition.healSoundName}");
#endif
                }

                // Animation
                if (definition.playAnimation)
                {
                    var animator = target.GetComponent<Animator>();
                    if (animator != null && !string.IsNullOrEmpty(definition.animationTrigger))
                    {
                        animator.SetTrigger(definition.animationTrigger);
                    }
                }
            }
        }

        /// <summary>
        /// Update overtime healing (called from external update loop)
        /// </summary>
        public void UpdateOvertimeHeal(float deltaTime)
        {
            if (!isActive || definition.healType != HealCommandDefinition.HealType.Overtime) return;

            // TODO: Implement actual overtime healing logic
        }

        /// <summary>
        /// Stop overtime healing
        /// </summary>
        public void StopOvertimeHeal()
        {
            if (isActive)
            {
                isActive = false;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.Log("Stopped overtime healing");
#endif
            }
        }

        /// <summary>
        /// Properties
        /// </summary>
        public bool IsActive => isActive;
        public float HealedAmount => healedAmount;
    }
}