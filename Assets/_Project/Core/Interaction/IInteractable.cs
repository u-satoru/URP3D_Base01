using UnityEngine;

namespace asterivo.Unity60.Core.Interaction
{
    /// <summary>
    /// Interface for objects that can be interacted with by an Interactor.
    /// Provides a standardized interaction system for various game objects.
    /// Integrates with Command Pattern and Event-Driven Architecture.
    /// </summary>
    public interface IInteractable
    {
        #region Properties

        /// <summary>
        /// Whether this object can currently be interacted with
        /// </summary>
        bool CanInteract { get; }

        /// <summary>
        /// The interaction prompt text to display to the player
        /// </summary>
        string InteractionPrompt { get; }

        /// <summary>
        /// The interaction range in world units
        /// </summary>
        float InteractionRange { get; }

        /// <summary>
        /// Priority for interaction when multiple interactables are in range
        /// Higher values have higher priority
        /// </summary>
        int InteractionPriority { get; }

        /// <summary>
        /// Transform for positioning interaction UI elements
        /// </summary>
        Transform InteractionTransform { get; }

        /// <summary>
        /// Whether this interactable should be highlighted when in range
        /// </summary>
        bool HighlightWhenInRange { get; }

        #endregion

        #region Interaction Methods

        /// <summary>
        /// Called when an interactor attempts to interact with this object
        /// </summary>
        /// <param name="interactor">The interactor performing the interaction</param>
        /// <returns>True if interaction was successful</returns>
        bool OnInteract(IInteractor interactor);

        /// <summary>
        /// Called when an interactor enters interaction range
        /// </summary>
        /// <param name="interactor">The interactor that entered range</param>
        void OnInteractionRangeEntered(IInteractor interactor);

        /// <summary>
        /// Called when an interactor exits interaction range
        /// </summary>
        /// <param name="interactor">The interactor that exited range</param>
        void OnInteractionRangeExited(IInteractor interactor);

        /// <summary>
        /// Called when this interactable becomes the primary interaction target
        /// </summary>
        /// <param name="interactor">The interactor that focused on this object</param>
        void OnInteractionFocused(IInteractor interactor);

        /// <summary>
        /// Called when this interactable loses focus as the primary interaction target
        /// </summary>
        /// <param name="interactor">The interactor that lost focus of this object</param>
        void OnInteractionUnfocused(IInteractor interactor);

        #endregion

        #region Validation

        /// <summary>
        /// Check if the specified interactor is allowed to interact with this object
        /// </summary>
        /// <param name="interactor">The interactor to validate</param>
        /// <returns>True if interaction is allowed</returns>
        bool CanInteractWith(IInteractor interactor);

        /// <summary>
        /// Get detailed information about why interaction might not be possible
        /// </summary>
        /// <param name="interactor">The interactor to check</param>
        /// <returns>Interaction status information</returns>
        InteractionStatus GetInteractionStatus(IInteractor interactor);

        #endregion

        #region Events

        /// <summary>
        /// Event fired when interaction state changes (enabled/disabled)
        /// </summary>
        event System.Action<IInteractable, bool> OnInteractionStateChanged;

        /// <summary>
        /// Event fired when interaction occurs
        /// </summary>
        event System.Action<IInteractable, IInteractor> OnInteracted;

        #endregion
    }

    /// <summary>
    /// Interface for objects that can initiate interactions with IInteractable objects.
    /// Provides standardized interaction detection and execution.
    /// </summary>
    public interface IInteractor
    {
        #region Properties

        /// <summary>
        /// Transform for interaction detection origin
        /// </summary>
        Transform InteractionOrigin { get; }

        /// <summary>
        /// Maximum interaction range
        /// </summary>
        float MaxInteractionRange { get; }

        /// <summary>
        /// Current primary interaction target
        /// </summary>
        IInteractable CurrentTarget { get; }

        /// <summary>
        /// Whether this interactor can currently perform interactions
        /// </summary>
        bool CanPerformInteractions { get; }

        /// <summary>
        /// Layer mask for interaction detection
        /// </summary>
        LayerMask InteractionLayerMask { get; }

        #endregion

        #region Interaction Methods

        /// <summary>
        /// Attempt to interact with the current target
        /// </summary>
        /// <returns>True if interaction was successful</returns>
        bool TryInteract();

        /// <summary>
        /// Attempt to interact with a specific interactable
        /// </summary>
        /// <param name="interactable">The target to interact with</param>
        /// <returns>True if interaction was successful</returns>
        bool TryInteractWith(IInteractable interactable);

        /// <summary>
        /// Update interaction detection (typically called in Update)
        /// </summary>
        void UpdateInteractionDetection();

        /// <summary>
        /// Get all interactables currently in range
        /// </summary>
        /// <returns>List of interactables in range</returns>
        System.Collections.Generic.List<IInteractable> GetInteractablesInRange();

        #endregion

        #region Events

        /// <summary>
        /// Event fired when interaction target changes
        /// </summary>
        event System.Action<IInteractor, IInteractable, IInteractable> OnTargetChanged;

        /// <summary>
        /// Event fired when an interaction is performed
        /// </summary>
        event System.Action<IInteractor, IInteractable, bool> OnInteractionPerformed;

        #endregion
    }

    /// <summary>
    /// Describes the current interaction status for validation and feedback
    /// </summary>
    [System.Serializable]
    public struct InteractionStatus
    {
        /// <summary>
        /// Whether interaction is currently possible
        /// </summary>
        public bool CanInteract;

        /// <summary>
        /// Reason why interaction might not be possible
        /// </summary>
        public string Reason;

        /// <summary>
        /// Additional context information
        /// </summary>
        public string Context;

        /// <summary>
        /// Priority level for this interaction
        /// </summary>
        public int Priority;

        /// <summary>
        /// Create a successful interaction status
        /// </summary>
        /// <param name="priority">Interaction priority</param>
        /// <returns>Successful interaction status</returns>
        public static InteractionStatus Success(int priority = 0)
        {
            return new InteractionStatus
            {
                CanInteract = true,
                Reason = "Ready to interact",
                Context = "",
                Priority = priority
            };
        }

        /// <summary>
        /// Create a failed interaction status
        /// </summary>
        /// <param name="reason">Reason for failure</param>
        /// <param name="context">Additional context</param>
        /// <returns>Failed interaction status</returns>
        public static InteractionStatus Failure(string reason, string context = "")
        {
            return new InteractionStatus
            {
                CanInteract = false,
                Reason = reason,
                Context = context,
                Priority = 0
            };
        }
    }

    /// <summary>
    /// Common interaction types for categorization and filtering
    /// </summary>
    public enum InteractionType
    {
        None = 0,
        Pickup = 1,
        Use = 2,
        Examine = 3,
        Talk = 4,
        Open = 5,
        Close = 6,
        Activate = 7,
        Deactivate = 8,
        Equip = 9,
        Drop = 10,
        Custom = 999
    }
}
