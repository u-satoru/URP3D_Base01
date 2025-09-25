using UnityEngine;

namespace asterivo.Unity60.Core.Commands.Definitions
{
    /// <summary>
    /// Interaction command definition
    /// Encapsulates player's interaction with environment objects
    ///
    /// Main features:
    /// - Object interaction (doors, switches, NPCs)
    /// - Interaction range and conditions management
    /// - Animation and effects during interaction
    /// - Multiple interaction types support
    /// </summary>
    [System.Serializable]
    public class InteractCommandDefinition : ICommandDefinition
    {
        /// <summary>
        /// Types of interactions
        /// </summary>
        public enum InteractionType
        {
            Instant,        // Instant interaction
            Hold,           // Hold to interact
            Multi,          // Multiple interactions required
            Contextual,     // Context-dependent interaction
            Proximity       // Proximity-based automatic interaction
        }

        [Header("Interaction Parameters")]
        public InteractionType interactionType = InteractionType.Instant;
        public float interactionRange = 2f;
        public LayerMask interactableLayer = -1;
        public string targetTag = "Interactable";

        [Header("Hold Interaction")]
        [Tooltip("Duration required for hold interaction")]
        public float holdDuration = 1f;
        [Tooltip("Can cancel during hold")]
        public bool canCancelHold = true;

        [Header("Multi Interaction")]
        [Tooltip("Required number of interactions")]
        public int requiredInteractions = 3;
        [Tooltip("Maximum interval between interactions")]
        public float maxInteractionInterval = 2f;

        [Header("Requirements")]
        public bool requiresLineOfSight = true;
        public bool requiresFacing = true;
        [Tooltip("Required facing angle in degrees")]
        public float facingAngle = 90f;

        [Header("Animation")]
        public bool playAnimation = true;
        public string animationTrigger = "Interact";
        public float animationDuration = 1f;

        [Header("Effects")]
        public bool showInteractionPrompt = true;
        public string promptText = "Press E to interact";
        public bool showProgressBar = false;

        /// <summary>
        /// Default constructor
        /// </summary>
        public InteractCommandDefinition()
        {
        }

        /// <summary>
        /// Parameterized constructor
        /// </summary>
        public InteractCommandDefinition(InteractionType type, float range, string tag = "Interactable")
        {
            interactionType = type;
            interactionRange = range;
            targetTag = tag;
        }

        /// <summary>
        /// Create interaction command instance
        /// </summary>
        public ICommand CreateCommand(object context = null)
        {
            return new InteractCommand(this, context);
        }
    }

    /// <summary>
    /// Actual implementation of interaction command
    /// </summary>
    public class InteractCommand : ICommand
    {
        private readonly InteractCommandDefinition definition;
        private readonly object context;
        private bool executed = false;
        private GameObject targetObject;
        private bool isInteracting = false;
        private float interactionProgress = 0f;
        private int currentInteractionCount = 0;

        public InteractCommand(InteractCommandDefinition interactDefinition, object executionContext)
        {
            definition = interactDefinition;
            context = executionContext;
        }

        /// <summary>
        /// Execute interaction command
        /// </summary>
        public void Execute()
        {
            if (executed) return;

            // Find interactable target
            targetObject = FindInteractableTarget();
            if (targetObject == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.LogWarning("No interactable target found within range");
#endif
                return;
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Executing {definition.interactionType} interaction with {targetObject.name}");
#endif

            switch (definition.interactionType)
            {
                case InteractCommandDefinition.InteractionType.Instant:
                    ExecuteInstantInteraction();
                    break;
                case InteractCommandDefinition.InteractionType.Hold:
                    StartHoldInteraction();
                    break;
                case InteractCommandDefinition.InteractionType.Multi:
                    ExecuteMultiInteraction();
                    break;
                case InteractCommandDefinition.InteractionType.Contextual:
                    ExecuteContextualInteraction();
                    break;
                case InteractCommandDefinition.InteractionType.Proximity:
                    ExecuteProximityInteraction();
                    break;
            }

            executed = true;
        }

        /// <summary>
        /// Check if command can be executed
        /// </summary>
        public bool CanExecute()
        {
            // Basic executability check
            if (definition.interactionRange <= 0f) return false;

            if (definition.interactionType == InteractCommandDefinition.InteractionType.Hold &&
                definition.holdDuration <= 0f) return false;

            if (definition.interactionType == InteractCommandDefinition.InteractionType.Multi &&
                definition.requiredInteractions <= 0) return false;

            return !executed;
        }

        /// <summary>
        /// Find interactable target within range
        /// </summary>
        private GameObject FindInteractableTarget()
        {
            if (context is not MonoBehaviour mono) return null;

            // Search for objects in range
            Collider[] nearbyObjects = Physics.OverlapSphere(
                mono.transform.position,
                definition.interactionRange,
                definition.interactableLayer);

            GameObject closestTarget = null;
            float closestDistance = float.MaxValue;

            foreach (var obj in nearbyObjects)
            {
                // Tag check
                if (!string.IsNullOrEmpty(definition.targetTag) &&
                    !obj.CompareTag(definition.targetTag))
                    continue;

                // Line of sight check
                if (definition.requiresLineOfSight &&
                    !HasLineOfSight(mono.transform, obj.transform))
                    continue;

                // Facing check
                if (definition.requiresFacing &&
                    !IsFacing(mono.transform, obj.transform))
                    continue;

                // Select closest object
                float distance = Vector3.Distance(mono.transform.position, obj.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTarget = obj.gameObject;
                }
            }

            return closestTarget;
        }

        /// <summary>
        /// Check line of sight
        /// </summary>
        private bool HasLineOfSight(Transform from, Transform to)
        {
            Vector3 direction = to.position - from.position;

            if (Physics.Raycast(from.position, direction.normalized,
                out RaycastHit hit, direction.magnitude))
            {
                return hit.collider.transform == to;
            }

            return true;
        }

        /// <summary>
        /// Check if facing target
        /// </summary>
        private bool IsFacing(Transform from, Transform to)
        {
            Vector3 directionToTarget = (to.position - from.position).normalized;
            float angle = Vector3.Angle(from.forward, directionToTarget);
            return angle <= definition.facingAngle * 0.5f;
        }

        /// <summary>
        /// Execute instant interaction
        /// </summary>
        private void ExecuteInstantInteraction()
        {
            if (targetObject != null)
            {
                // Call interactable component
                var interactable = targetObject.GetComponent<IInteractable>();
                interactable?.OnInteract(context);

                PlayInteractionAnimation();
                ShowInteractionEffect();
            }
        }

        /// <summary>
        /// Start hold interaction
        /// </summary>
        private void StartHoldInteraction()
        {
            isInteracting = true;
            interactionProgress = 0f;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Started hold interaction: {definition.holdDuration}s required");
#endif
        }

        /// <summary>
        /// Execute multi-step interaction
        /// </summary>
        private void ExecuteMultiInteraction()
        {
            currentInteractionCount++;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Multi interaction: {currentInteractionCount}/{definition.requiredInteractions}");
#endif

            if (currentInteractionCount >= definition.requiredInteractions)
            {
                // Complete when required count reached
                CompleteMultiInteraction();
            }
            else
            {
                // Show progress feedback
                ShowProgressFeedback();
            }
        }

        /// <summary>
        /// Execute contextual interaction
        /// </summary>
        private void ExecuteContextualInteraction()
        {
            // Execute different logic based on current context
            var interactable = targetObject?.GetComponent<IContextualInteractable>();
            interactable?.OnContextualInteract(context, GetCurrentContext());
        }

        /// <summary>
        /// Execute proximity-based interaction
        /// </summary>
        private void ExecuteProximityInteraction()
        {
            // Automatically triggered while player is in range
            var interactable = targetObject?.GetComponent<IProximityInteractable>();
            interactable?.OnProximityInteract(context);
        }

        /// <summary>
        /// Complete multi-step interaction
        /// </summary>
        private void CompleteMultiInteraction()
        {
            var interactable = targetObject?.GetComponent<IInteractable>();
            interactable?.OnInteract(context);

            currentInteractionCount = 0;
            ShowInteractionEffect();
        }

        /// <summary>
        /// Update hold interaction (called periodically from outside)
        /// </summary>
        public void UpdateHoldInteraction(float deltaTime)
        {
            if (!isInteracting ||
                definition.interactionType != InteractCommandDefinition.InteractionType.Hold)
                return;

            interactionProgress += deltaTime;

            // Update progress bar
            if (definition.showProgressBar)
            {
                float progress = interactionProgress / definition.holdDuration;
                // TODO: Update UI
            }

            // Check completion
            if (interactionProgress >= definition.holdDuration)
            {
                CompleteHoldInteraction();
            }
        }

        /// <summary>
        /// Complete hold interaction
        /// </summary>
        private void CompleteHoldInteraction()
        {
            isInteracting = false;
            interactionProgress = 0f;

            var interactable = targetObject?.GetComponent<IInteractable>();
            interactable?.OnInteract(context);

            ShowInteractionEffect();
        }

        /// <summary>
        /// Play interaction animation
        /// </summary>
        private void PlayInteractionAnimation()
        {
            if (!definition.playAnimation || context is not MonoBehaviour mono) return;

            var animator = mono.GetComponent<Animator>();
            if (animator != null && !string.IsNullOrEmpty(definition.animationTrigger))
            {
                animator.SetTrigger(definition.animationTrigger);
            }
        }

        /// <summary>
        /// Show interaction effect
        /// </summary>
        private void ShowInteractionEffect()
        {
            // TODO: Particle effects
            // TODO: Sound effects
            // TODO: UI feedback

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log("Showing interaction effect");
#endif
        }

        /// <summary>
        /// Show progress feedback
        /// </summary>
        private void ShowProgressFeedback()
        {
            // TODO: Show progress UI
            // TODO: Sound feedback
        }

        /// <summary>
        /// Get current context information
        /// </summary>
        private object GetCurrentContext()
        {
            // Return context information including time, held items, quest state, etc.
            return new { TimeOfDay = "Day", HasKey = false };
        }

        /// <summary>
        /// Cancel ongoing interaction
        /// </summary>
        public void CancelInteraction()
        {
            if (isInteracting && definition.canCancelHold)
            {
                isInteracting = false;
                interactionProgress = 0f;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.Log("Interaction cancelled");
#endif
            }
        }

        /// <summary>
        /// Properties
        /// </summary>
        public bool IsInteracting => isInteracting;
    }

    /// <summary>
    /// Basic interactable object interface
    /// </summary>
    public interface IInteractable
    {
        void OnInteract(object interactor);
    }

    /// <summary>
    /// Contextual interactable object interface
    /// </summary>
    public interface IContextualInteractable
    {
        void OnContextualInteract(object interactor, object context);
    }

    /// <summary>
    /// Proximity-based interactable object interface
    /// </summary>
    public interface IProximityInteractable
    {
        void OnProximityInteract(object interactor);
    }

    /// <summary>
    /// Undoable interactable object interface
    /// </summary>
    public interface IUndoableInteractable
    {
        void OnUndoInteract(object interactor);
    }
}