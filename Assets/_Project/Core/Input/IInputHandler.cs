using UnityEngine;

namespace asterivo.Unity60.Core.Input
{
    /// <summary>
    /// Interface for handling input events in a type-safe manner.
    /// Provides a contract for input processing with context and priority management.
    /// </summary>
    public interface IInputHandler
    {
        /// <summary>
        /// The input context this handler operates in
        /// </summary>
        InputContext SupportedContext { get; }

        /// <summary>
        /// Priority level for input conflict resolution
        /// </summary>
        InputPriority Priority { get; }

        /// <summary>
        /// Whether this handler is currently active and should receive input
        /// </summary>
        bool IsActive { get; set; }

        /// <summary>
        /// Handle a generic input action
        /// </summary>
        /// <param name="inputData">The input action data</param>
        /// <returns>True if the input was consumed and should not be processed by lower priority handlers</returns>
        bool HandleInput(InputActionData inputData);

        /// <summary>
        /// Handle movement input
        /// </summary>
        /// <param name="movementData">Movement input data</param>
        /// <returns>True if the input was consumed</returns>
        bool HandleMovementInput(MovementInputData movementData);

        /// <summary>
        /// Handle combat input
        /// </summary>
        /// <param name="combatData">Combat input data</param>
        /// <returns>True if the input was consumed</returns>
        bool HandleCombatInput(CombatInputData combatData);

        /// <summary>
        /// Handle UI input
        /// </summary>
        /// <param name="uiData">UI input data</param>
        /// <returns>True if the input was consumed</returns>
        bool HandleUIInput(UIInputData uiData);

        /// <summary>
        /// Check if this handler should process input in the given context
        /// </summary>
        /// <param name="context">The current input context</param>
        /// <returns>True if this handler should process input</returns>
        bool ShouldProcessInput(InputContext context);
    }

    /// <summary>
    /// Generic input handler interface for typed input handling
    /// </summary>
    /// <typeparam name="T">The type of input data this handler processes</typeparam>
    public interface IInputHandler<T> : IInputHandler where T : struct
    {
        /// <summary>
        /// Handle typed input data
        /// </summary>
        /// <param name="inputData">The typed input data</param>
        /// <returns>True if the input was consumed</returns>
        bool HandleTypedInput(T inputData);
    }

    /// <summary>
    /// Base abstract class for input handlers with common functionality
    /// </summary>
    public abstract class BaseInputHandler : MonoBehaviour, IInputHandler
    {
        [Header("Input Handler Configuration")]
        [SerializeField] protected InputContext supportedContext = InputContext.Gameplay;
        [SerializeField] protected InputPriority priority = InputPriority.Normal;
        [SerializeField] protected bool isActive = true;

        public InputContext SupportedContext => supportedContext;
        public InputPriority Priority => priority;
        public bool IsActive { get => isActive; set => isActive = value; }

        public virtual bool ShouldProcessInput(InputContext context)
        {
            return IsActive && (SupportedContext == context || SupportedContext == InputContext.Gameplay);
        }

        public abstract bool HandleInput(InputActionData inputData);
        public abstract bool HandleMovementInput(MovementInputData movementData);
        public abstract bool HandleCombatInput(CombatInputData combatData);
        public abstract bool HandleUIInput(UIInputData uiData);

        protected virtual void OnEnable()
        {
            // Register with InputManager when available
            var inputManager = FindObjectOfType<InputManager>();
            if (inputManager != null)
            {
                inputManager.RegisterHandler(this);
            }
        }

        protected virtual void OnDisable()
        {
            // Unregister from InputManager when available
            var inputManager = FindObjectOfType<InputManager>();
            if (inputManager != null)
            {
                inputManager.UnregisterHandler(this);
            }
        }
    }
}