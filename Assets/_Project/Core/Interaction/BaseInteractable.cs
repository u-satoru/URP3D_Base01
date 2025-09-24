using UnityEngine;
using asterivo.Unity60.Core.Events;

namespace asterivo.Unity60.Core.Interaction
{
    /// <summary>
    /// Base implementation of IInteractable that provides common functionality.
    /// Concrete interactables can inherit from this class for consistent behavior.
    /// Integrates with Event-Driven Architecture and Command Pattern.
    /// </summary>
    public abstract class BaseInteractable : MonoBehaviour, IInteractable
    {
        #region Serialized Fields

        [Header("Interaction Settings")]
        [SerializeField] protected string interactionPrompt = "Interact";
        [SerializeField] protected float interactionRange = 2f;
        [SerializeField] protected int interactionPriority = 0;
        [SerializeField] protected bool highlightWhenInRange = true;
        [SerializeField] protected bool canInteract = true;

        [Header("Transform Settings")]
        [SerializeField] protected Transform interactionTransform;

        [Header("Event Channels")]
        [SerializeField] protected GameEvent<IInteractable> onInteractionStateChanged;
        [SerializeField] protected GameEvent<IInteractable> onInteracted;

        [Header("Debug")]
        [SerializeField] protected bool enableDebugDrawing = true;
        [SerializeField] protected Color rangeColor = Color.green;
        [SerializeField] protected Color disabledColor = Color.red;

        #endregion

        #region Private Fields

        private bool _wasCanInteract;
        private readonly System.Collections.Generic.HashSet<IInteractor> _interactorsInRange = new();
        private readonly System.Collections.Generic.HashSet<IInteractor> _focusedInteractors = new();

        #endregion

        #region IInteractable Properties

        /// <summary>
        /// Whether this object can currently be interacted with
        /// </summary>
        public virtual bool CanInteract => canInteract && enabled && gameObject.activeInHierarchy;

        /// <summary>
        /// The interaction prompt text to display to the player
        /// </summary>
        public virtual string InteractionPrompt => interactionPrompt;

        /// <summary>
        /// The interaction range in world units
        /// </summary>
        public virtual float InteractionRange => interactionRange;

        /// <summary>
        /// Priority for interaction when multiple interactables are in range
        /// Higher values have higher priority
        /// </summary>
        public virtual int InteractionPriority => interactionPriority;

        /// <summary>
        /// Transform for positioning interaction UI elements
        /// </summary>
        public virtual Transform InteractionTransform => interactionTransform != null ? interactionTransform : transform;

        /// <summary>
        /// Whether this interactable should be highlighted when in range
        /// </summary>
        public virtual bool HighlightWhenInRange => highlightWhenInRange;

        #endregion

        #region IInteractable Events

        /// <summary>
        /// Event fired when interaction state changes (enabled/disabled)
        /// </summary>
        public event System.Action<IInteractable, bool> OnInteractionStateChanged;

        /// <summary>
        /// Event fired when interaction occurs
        /// </summary>
        public event System.Action<IInteractable, IInteractor> OnInteracted;

        #endregion

        #region Unity Lifecycle

        protected virtual void Awake()
        {
            InitializeComponents();
        }

        protected virtual void Start()
        {
            _wasCanInteract = CanInteract;
        }

        protected virtual void Update()
        {
            CheckForStateChanges();
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize required components
        /// </summary>
        protected virtual void InitializeComponents()
        {
            // Use this transform if no specific interaction transform is set
            if (interactionTransform == null)
            {
                interactionTransform = transform;
            }
        }

        #endregion

        #region IInteractable Implementation

        /// <summary>
        /// Called when an interactor attempts to interact with this object
        /// </summary>
        /// <param name="interactor">The interactor performing the interaction</param>
        /// <returns>True if interaction was successful</returns>
        public bool OnInteract(IInteractor interactor)
        {
            if (!CanInteract || !CanInteractWith(interactor))
            {
                return false;
            }

            try
            {
                bool success = PerformInteraction(interactor);

                if (success)
                {
                    FireInteracted(interactor);
                }

                return success;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"BaseInteractable: Error during interaction with {interactor}: {e.Message}", this);
                return false;
            }
        }

        /// <summary>
        /// Called when an interactor enters interaction range
        /// </summary>
        /// <param name="interactor">The interactor that entered range</param>
        public virtual void OnInteractionRangeEntered(IInteractor interactor)
        {
            _interactorsInRange.Add(interactor);
            OnInteractorEnteredRange(interactor);
        }

        /// <summary>
        /// Called when an interactor exits interaction range
        /// </summary>
        /// <param name="interactor">The interactor that exited range</param>
        public virtual void OnInteractionRangeExited(IInteractor interactor)
        {
            _interactorsInRange.Remove(interactor);
            _focusedInteractors.Remove(interactor);
            OnInteractorExitedRange(interactor);
        }

        /// <summary>
        /// Called when this interactable becomes the primary interaction target
        /// </summary>
        /// <param name="interactor">The interactor that focused on this object</param>
        public virtual void OnInteractionFocused(IInteractor interactor)
        {
            _focusedInteractors.Add(interactor);
            OnInteractorFocused(interactor);
        }

        /// <summary>
        /// Called when this interactable loses focus as the primary interaction target
        /// </summary>
        /// <param name="interactor">The interactor that lost focus of this object</param>
        public virtual void OnInteractionUnfocused(IInteractor interactor)
        {
            _focusedInteractors.Remove(interactor);
            OnInteractorUnfocused(interactor);
        }

        /// <summary>
        /// Check if the specified interactor is allowed to interact with this object
        /// </summary>
        /// <param name="interactor">The interactor to validate</param>
        /// <returns>True if interaction is allowed</returns>
        public virtual bool CanInteractWith(IInteractor interactor)
        {
            return interactor != null && ValidateInteractor(interactor);
        }

        /// <summary>
        /// Get detailed information about why interaction might not be possible
        /// </summary>
        /// <param name="interactor">The interactor to check</param>
        /// <returns>Interaction status information</returns>
        public virtual InteractionStatus GetInteractionStatus(IInteractor interactor)
        {
            if (!CanInteract)
            {
                return InteractionStatus.Failure("Interactable is disabled", $"Object: {gameObject.name}");
            }

            if (interactor == null)
            {
                return InteractionStatus.Failure("Invalid interactor", "Interactor is null");
            }

            if (!CanInteractWith(interactor))
            {
                return InteractionStatus.Failure("Interaction not allowed", GetInteractionDenialReason(interactor));
            }

            float distance = Vector3.Distance(InteractionTransform.position, interactor.InteractionOrigin.position);
            if (distance > InteractionRange)
            {
                return InteractionStatus.Failure("Out of range", $"Distance: {distance:F1}m, Required: {InteractionRange:F1}m");
            }

            return InteractionStatus.Success(InteractionPriority);
        }

        #endregion

        #region Abstract Methods

        /// <summary>
        /// Perform the actual interaction logic
        /// Override this method in derived classes to implement specific interaction behavior
        /// </summary>
        /// <param name="interactor">The interactor performing the interaction</param>
        /// <returns>True if interaction was successful</returns>
        protected abstract bool PerformInteraction(IInteractor interactor);

        #endregion

        #region Virtual Methods

        /// <summary>
        /// Validate if the interactor is allowed to interact with this object
        /// Override this method to implement custom validation logic
        /// </summary>
        /// <param name="interactor">The interactor to validate</param>
        /// <returns>True if interaction is allowed</returns>
        protected virtual bool ValidateInteractor(IInteractor interactor)
        {
            return true; // Default: allow all interactors
        }

        /// <summary>
        /// Get the reason why interaction is denied for a specific interactor
        /// Override this method to provide detailed denial reasons
        /// </summary>
        /// <param name="interactor">The interactor being denied</param>
        /// <returns>Reason for denial</returns>
        protected virtual string GetInteractionDenialReason(IInteractor interactor)
        {
            return "Interaction not allowed";
        }

        /// <summary>
        /// Called when an interactor enters interaction range
        /// Override this method to implement custom range enter behavior
        /// </summary>
        /// <param name="interactor">The interactor that entered range</param>
        protected virtual void OnInteractorEnteredRange(IInteractor interactor)
        {
            // Default: no additional behavior
        }

        /// <summary>
        /// Called when an interactor exits interaction range
        /// Override this method to implement custom range exit behavior
        /// </summary>
        /// <param name="interactor">The interactor that exited range</param>
        protected virtual void OnInteractorExitedRange(IInteractor interactor)
        {
            // Default: no additional behavior
        }

        /// <summary>
        /// Called when this interactable becomes the primary interaction target
        /// Override this method to implement custom focus behavior
        /// </summary>
        /// <param name="interactor">The interactor that focused on this object</param>
        protected virtual void OnInteractorFocused(IInteractor interactor)
        {
            // Default: no additional behavior
        }

        /// <summary>
        /// Called when this interactable loses focus as the primary interaction target
        /// Override this method to implement custom unfocus behavior
        /// </summary>
        /// <param name="interactor">The interactor that lost focus of this object</param>
        protected virtual void OnInteractorUnfocused(IInteractor interactor)
        {
            // Default: no additional behavior
        }

        #endregion

        #region Public Utility Methods

        /// <summary>
        /// Enable or disable interaction capabilities
        /// </summary>
        /// <param name="enabled">Enable state</param>
        public virtual void SetInteractionEnabled(bool enabled)
        {
            canInteract = enabled;
        }

        /// <summary>
        /// Update the interaction prompt text
        /// </summary>
        /// <param name="newPrompt">New prompt text</param>
        public virtual void SetInteractionPrompt(string newPrompt)
        {
            interactionPrompt = newPrompt;
        }

        /// <summary>
        /// Update the interaction range
        /// </summary>
        /// <param name="newRange">New interaction range</param>
        public virtual void SetInteractionRange(float newRange)
        {
            interactionRange = Mathf.Max(0f, newRange);
        }

        /// <summary>
        /// Update the interaction priority
        /// </summary>
        /// <param name="newPriority">New priority value</param>
        public virtual void SetInteractionPriority(int newPriority)
        {
            interactionPriority = newPriority;
        }

        /// <summary>
        /// Get all interactors currently in range
        /// </summary>
        /// <returns>List of interactors in range</returns>
        public System.Collections.Generic.List<IInteractor> GetInteractorsInRange()
        {
            return new System.Collections.Generic.List<IInteractor>(_interactorsInRange);
        }

        /// <summary>
        /// Get all interactors currently focused on this object
        /// </summary>
        /// <returns>List of focused interactors</returns>
        public System.Collections.Generic.List<IInteractor> GetFocusedInteractors()
        {
            return new System.Collections.Generic.List<IInteractor>(_focusedInteractors);
        }

        /// <summary>
        /// Check if a specific interactor is in range
        /// </summary>
        /// <param name="interactor">The interactor to check</param>
        /// <returns>True if interactor is in range</returns>
        public bool IsInteractorInRange(IInteractor interactor)
        {
            return _interactorsInRange.Contains(interactor);
        }

        /// <summary>
        /// Check if a specific interactor is focused on this object
        /// </summary>
        /// <param name="interactor">The interactor to check</param>
        /// <returns>True if interactor is focused</returns>
        public bool IsInteractorFocused(IInteractor interactor)
        {
            return _focusedInteractors.Contains(interactor);
        }

        /// <summary>
        /// Get interaction debug information
        /// </summary>
        /// <returns>Debug information string</returns>
        public virtual string GetInteractionDebugInfo()
        {
            return $"Prompt: {InteractionPrompt}, " +
                   $"Range: {InteractionRange:F1}m, " +
                   $"Priority: {InteractionPriority}, " +
                   $"Can Interact: {CanInteract}, " +
                   $"In Range: {_interactorsInRange.Count}, " +
                   $"Focused: {_focusedInteractors.Count}";
        }

        #endregion

        #region Event Management

        /// <summary>
        /// Check for state changes and fire events accordingly
        /// </summary>
        private void CheckForStateChanges()
        {
            bool currentCanInteract = CanInteract;
            if (currentCanInteract != _wasCanInteract)
            {
                FireInteractionStateChanged(currentCanInteract);
                _wasCanInteract = currentCanInteract;
            }
        }

        /// <summary>
        /// Fire interaction state changed event
        /// </summary>
        /// <param name="canInteract">New interaction state</param>
        protected virtual void FireInteractionStateChanged(bool canInteract)
        {
            OnInteractionStateChanged?.Invoke(this, canInteract);
            onInteractionStateChanged?.Raise(this);
        }

        /// <summary>
        /// Fire interaction performed event
        /// </summary>
        /// <param name="interactor">The interactor that performed the interaction</param>
        protected virtual void FireInteracted(IInteractor interactor)
        {
            OnInteracted?.Invoke(this, interactor);
            onInteracted?.Raise(this);
        }

        #endregion

        #region Debug Drawing

        protected virtual void OnDrawGizmosSelected()
        {
            if (!enableDebugDrawing) return;

            Vector3 position = InteractionTransform != null ? InteractionTransform.position : transform.position;

            // Draw interaction range
            Gizmos.color = CanInteract ? rangeColor : disabledColor;
            Gizmos.DrawWireSphere(position, InteractionRange);

            // Draw priority indicator
            if (InteractionPriority > 0)
            {
                Gizmos.color = Color.yellow;
                Vector3 priorityPos = position + Vector3.up * (InteractionRange + 0.5f);
                Gizmos.DrawWireCube(priorityPos, Vector3.one * 0.2f * InteractionPriority);
            }

            // Draw interactors in range
            Gizmos.color = Color.cyan;
            foreach (var interactor in _interactorsInRange)
            {
                if (interactor?.InteractionOrigin != null)
                {
                    Gizmos.DrawLine(position, interactor.InteractionOrigin.position);
                }
            }

            // Draw focused interactors
            Gizmos.color = Color.magenta;
            foreach (var interactor in _focusedInteractors)
            {
                if (interactor?.InteractionOrigin != null)
                {
                    Vector3 interactorPos = interactor.InteractionOrigin.position;
                    Gizmos.DrawWireCube(interactorPos, Vector3.one * 0.3f);
                }
            }
        }

        #endregion
    }
}
