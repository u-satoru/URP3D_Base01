using System;
using UnityEngine;

namespace asterivo.Unity60.Core.Input
{
    /// <summary>
    /// Service interface for input management system.
    /// Provides a contract for input handling, context management, and handler registration.
    /// </summary>
    public interface IInputService
    {
        /// <summary>
        /// Current input context
        /// </summary>
        InputContext CurrentContext { get; }

        /// <summary>
        /// Whether input processing is currently enabled
        /// </summary>
        bool IsInputEnabled { get; }

        /// <summary>
        /// Event raised when input context changes
        /// </summary>
        event Action<InputContext> OnInputContextChanged;

        /// <summary>
        /// Event raised when input is enabled or disabled
        /// </summary>
        event Action<bool> OnInputEnabledChanged;

        /// <summary>
        /// Register an input handler with the service
        /// </summary>
        /// <param name="handler">The handler to register</param>
        void RegisterHandler(IInputHandler handler);

        /// <summary>
        /// Unregister an input handler from the service
        /// </summary>
        /// <param name="handler">The handler to unregister</param>
        void UnregisterHandler(IInputHandler handler);

        /// <summary>
        /// Change the current input context
        /// </summary>
        /// <param name="newContext">The new input context</param>
        void SetInputContext(InputContext newContext);

        /// <summary>
        /// Enable or disable input processing
        /// </summary>
        /// <param name="enabled">Whether input should be enabled</param>
        void SetInputEnabled(bool enabled);

        /// <summary>
        /// Process a generic input action
        /// </summary>
        /// <param name="inputData">The input action data</param>
        void ProcessInputAction(InputActionData inputData);

        /// <summary>
        /// Process movement input
        /// </summary>
        /// <param name="movementData">The movement input data</param>
        void ProcessMovementInput(MovementInputData movementData);

        /// <summary>
        /// Process combat input
        /// </summary>
        /// <param name="combatData">The combat input data</param>
        void ProcessCombatInput(CombatInputData combatData);

        /// <summary>
        /// Process UI input
        /// </summary>
        /// <param name="uiData">The UI input data</param>
        void ProcessUIInput(UIInputData uiData);

        /// <summary>
        /// Get current input state
        /// </summary>
        /// <returns>Current input state data</returns>
        InputStateData GetCurrentInputState();

        /// <summary>
        /// Get debug information about the input system
        /// </summary>
        /// <returns>Formatted debug information</returns>
        string GetDebugInfo();
    }
}