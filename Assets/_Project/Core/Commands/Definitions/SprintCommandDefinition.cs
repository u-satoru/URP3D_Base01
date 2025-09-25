using UnityEngine;
// using asterivo.Unity60.Core.Commands;

namespace asterivo.Unity60.Core.Commands.Definitions
{
    /// <summary>
    /// Sprint (dash) command definition.
    /// Encapsulates high-speed movement actions for players or AI.
    ///
    /// Main features:
    /// - Sprint speed and duration management
    /// - Stamina consumption system integration
    /// - Sprint restrictions (direction lock, etc.)
    /// - Animation and effect control
    /// </summary>
    [System.Serializable]
    public class SprintCommandDefinition : ICommandDefinition
    {
        /// <summary>
        /// Types of sprint behavior
        /// </summary>
        public enum SprintType
        {
            Burst,      // Short explosive acceleration
            Sustained,  // Sustained high-speed movement
            Dodge,      // Evasion dash
            Charge      // Attack charge
        }

        [Header("Sprint Parameters")]
        public SprintType sprintType = SprintType.Burst;
        public float speedMultiplier = 2f;
        public float maxDuration = 3f;
        public Vector3 direction = Vector3.forward;

        [Header("Stamina System")]
        public float staminaConsumptionRate = 10f; // per second
        public float minimumStaminaRequired = 25f;
        public bool canInterruptOnStaminaDepleted = true;

        [Header("Movement Constraints")]
        public bool lockDirection = false;
        public float directionChangeSpeed = 5f;
        public bool maintainVelocityOnEnd = false;

        [Header("Effects")]
        public float accelerationTime = 0.2f;
        public float decelerationTime = 0.5f;
        public bool showTrailEffect = true;

        /// <summary>
        /// Default constructor
        /// </summary>
        public SprintCommandDefinition()
        {
        }

        /// <summary>
        /// Parameterized constructor
        /// </summary>
        public SprintCommandDefinition(SprintType type, float multiplier, Vector3 sprintDirection)
        {
            sprintType = type;
            speedMultiplier = multiplier;
            direction = sprintDirection.normalized;
        }

        /// <summary>
        /// Check if sprint command can be executed
        /// </summary>
        public bool CanExecute(object context = null)
        {
            // Basic executability check
            if (speedMultiplier <= 1f || maxDuration <= 0f) return false;

            // Direction vector check
            if (direction == Vector3.zero) return false;

            // Additional checks if context exists
            if (context != null)
            {
                // Stamina check
                // Cooldown check
                // Status abnormality check (stun, freeze, etc.)
                // Terrain restriction check (water, steep slopes, etc.)
            }

            return true;
        }

        /// <summary>
        /// Create sprint command instance
        /// </summary>
        public ICommand CreateCommand(object context = null)
        {
            if (!CanExecute(context))
                return null;

            return new SprintCommand(this, context);
        }
    }

    /// <summary>
    /// Actual implementation of sprint command
    /// </summary>
    public class SprintCommand : ICommand
    {
        private SprintCommandDefinition definition;
        private object context;
        private bool executed = false;
        private bool isActive = false;
        private float originalSpeed;
        private float currentDuration = 0f;

        public SprintCommand(SprintCommandDefinition sprintDefinition, object executionContext)
        {
            definition = sprintDefinition;
            context = executionContext;
        }

        /// <summary>
        /// Execute sprint command
        /// </summary>
        public void Execute()
        {
            if (executed) return;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Executing {definition.sprintType} sprint: {definition.speedMultiplier}x speed, {definition.maxDuration}s max duration");
#endif

            // Start sprint state
            isActive = true;
            currentDuration = 0f;

            // Actual sprint processing implemented here
            if (context is MonoBehaviour mono)
            {
                // Save and change movement speed
                // Animation control
                // Start particle effects
                // Sound effects
                // Start stamina consumption (actual implementation needs StaminaSystem integration)
                // Start continuous update processing (Coroutine or UpdateLoop)
            }

            executed = true;
        }

        /// <summary>
        /// Check if command can be executed
        /// </summary>
        public bool CanExecute()
        {
            return !executed && definition.CanExecute(context);
        }

        /// <summary>
        /// Update sprint state (called periodically from external)
        /// </summary>
        public void UpdateSprint(float deltaTime)
        {
            if (!isActive) return;

            currentDuration += deltaTime;

            // Stamina consumption processing
            float staminaConsumed = definition.staminaConsumptionRate * deltaTime;

            // Max duration check
            if (currentDuration >= definition.maxDuration)
            {
                EndSprint();
                return;
            }

            // Stamina depletion check
            if (definition.canInterruptOnStaminaDepleted)
            {
                // Actual implementation needs reference from StaminaSystem
                // if (currentStamina <= 0f) EndSprint();
            }
        }

        /// <summary>
        /// End sprint state
        /// </summary>
        public void EndSprint()
        {
            if (!isActive) return;

            isActive = false;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Sprint ended after {currentDuration:F1} seconds");
#endif

            // Restore speed (based on maintainVelocityOnEnd)
            // Animation control
            // Stop effects
            // Start cooldown
        }

        /// <summary>
        /// Undo operation (force stop sprint)
        /// </summary>
        public void Undo()
        {
            if (!executed) return;

            EndSprint();

            // Restore consumed stamina (partial)
            // Reset state

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log("Sprint command undone");
#endif

            executed = false;
        }

        /// <summary>
        /// Whether this command can be undone
        /// </summary>
        public bool CanUndo => executed;

        /// <summary>
        /// Whether sprint is currently active
        /// </summary>
        public bool IsActive => isActive;
    }
}