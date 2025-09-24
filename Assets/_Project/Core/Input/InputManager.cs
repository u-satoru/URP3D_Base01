using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Services;

namespace asterivo.Unity60.Core.Input
{
    /// <summary>
    /// Central input management system that coordinates input handling across all registered handlers.
    /// Integrates with Unity's Input System and provides event-driven input distribution.
    /// </summary>
    [DefaultExecutionOrder(-100)] // Execute early to ensure input is processed before other systems
    public class InputManager : MonoBehaviour, IInputService, IService
    {
        [Header("Input Configuration")]
        [SerializeField] private InputContext currentContext = InputContext.Gameplay;
        [SerializeField] private bool enableInputDebugging = false;
        [SerializeField] private float inputProcessingTimeThreshold = 0.1f; // Log warning if input processing takes longer

        [Header("Event Channels")]
        [SerializeField] private GameEvent<InputActionData> onInputActionEvent;
        [SerializeField] private GameEvent<MovementInputData> onMovementInputEvent;
        [SerializeField] private GameEvent<CombatInputData> onCombatInputEvent;
        [SerializeField] private GameEvent<UIInputData> onUIInputEvent;
        [SerializeField] private GameEvent<InputContext> onInputContextChangedEvent;

        // Handler management
        private readonly List<IInputHandler> registeredHandlers = new List<IInputHandler>();
        private readonly Dictionary<InputPriority, List<IInputHandler>> prioritizedHandlers = new Dictionary<InputPriority, List<IInputHandler>>();
        private readonly Dictionary<InputContext, List<IInputHandler>> contextHandlers = new Dictionary<InputContext, List<IInputHandler>>();

        // Current input state
        private InputStateData currentInputState;
        private bool isInputEnabled = true;

        // Performance tracking
        private float lastInputProcessTime;
        private int frameInputCount;

        // Service interface events
        public event Action<InputContext> OnInputContextChanged;
        public event Action<bool> OnInputEnabledChanged;

        // Service interface properties
        public InputContext CurrentContext => currentContext;
        public bool IsInputEnabled => isInputEnabled;

        #region Unity Lifecycle

        private void Awake()
        {
            InitializePriorityDictionaries();
            currentInputState = InputStateData.Create(currentContext);

            // Register as service
            ServiceLocator.RegisterService<InputManager>(this);
        }

        private void OnDestroy()
        {
            ServiceLocator.UnregisterService<InputManager>();
        }

        private void LateUpdate()
        {
            // Process any queued input events at the end of frame
            ProcessQueuedInputEvents();

            // Reset frame counters
            frameInputCount = 0;
        }

        #endregion

        #region Public API

        /// <summary>
        /// Register an input handler with the manager
        /// </summary>
        /// <param name="handler">The handler to register</param>
        public void RegisterHandler(IInputHandler handler)
        {
            if (handler == null || registeredHandlers.Contains(handler))
                return;

            registeredHandlers.Add(handler);
            AddToPriorityList(handler);
            AddToContextList(handler);

            if (enableInputDebugging)
                Debug.Log($"[InputManager] Registered handler: {handler.GetType().Name} (Priority: {handler.Priority}, Context: {handler.SupportedContext})");
        }

        /// <summary>
        /// Unregister an input handler from the manager
        /// </summary>
        /// <param name="handler">The handler to unregister</param>
        public void UnregisterHandler(IInputHandler handler)
        {
            if (handler == null || !registeredHandlers.Contains(handler))
                return;

            registeredHandlers.Remove(handler);
            RemoveFromPriorityList(handler);
            RemoveFromContextList(handler);

            if (enableInputDebugging)
                Debug.Log($"[InputManager] Unregistered handler: {handler.GetType().Name}");
        }

        /// <summary>
        /// Change the current input context
        /// </summary>
        /// <param name="newContext">The new input context</param>
        public void SetInputContext(InputContext newContext)
        {
            if (currentContext == newContext)
                return;

            var oldContext = currentContext;
            currentContext = newContext;
            currentInputState.context = newContext;

            // Notify all systems of context change
            onInputContextChangedEvent?.Raise(newContext);

            if (enableInputDebugging)
                Debug.Log($"[InputManager] Input context changed: {oldContext} -> {newContext}");
        }

        /// <summary>
        /// Enable or disable input processing
        /// </summary>
        /// <param name="enabled">Whether input should be enabled</param>
        public void SetInputEnabled(bool enabled)
        {
            isInputEnabled = enabled;

            if (enableInputDebugging)
                Debug.Log($"[InputManager] Input enabled: {enabled}");
        }

        /// <summary>
        /// Process a generic input action
        /// </summary>
        /// <param name="inputData">The input action data</param>
        public void ProcessInputAction(InputActionData inputData)
        {
            if (!isInputEnabled || !ShouldProcessInput(inputData))
                return;

            var startTime = Time.realtimeSinceStartup;
            frameInputCount++;

            // Distribute to event system
            onInputActionEvent?.Raise(inputData);

            // Process through priority-ordered handlers
            ProcessInputThroughHandlers(inputData, (handler, data) => handler.HandleInput(data));

            TrackInputPerformance(startTime, "InputAction");
        }

        /// <summary>
        /// Process movement input
        /// </summary>
        /// <param name="movementData">The movement input data</param>
        public void ProcessMovementInput(MovementInputData movementData)
        {
            if (!isInputEnabled)
                return;

            var startTime = Time.realtimeSinceStartup;
            frameInputCount++;

            // Update current state
            currentInputState.movement = movementData;

            // Distribute to event system
            onMovementInputEvent?.Raise(movementData);

            // Process through priority-ordered handlers
            ProcessInputThroughHandlers(movementData, (handler, data) => handler.HandleMovementInput(data));

            TrackInputPerformance(startTime, "MovementInput");
        }

        /// <summary>
        /// Process combat input
        /// </summary>
        /// <param name="combatData">The combat input data</param>
        public void ProcessCombatInput(CombatInputData combatData)
        {
            if (!isInputEnabled)
                return;

            var startTime = Time.realtimeSinceStartup;
            frameInputCount++;

            // Update current state
            currentInputState.combat = combatData;

            // Distribute to event system
            onCombatInputEvent?.Raise(combatData);

            // Process through priority-ordered handlers
            ProcessInputThroughHandlers(combatData, (handler, data) => handler.HandleCombatInput(data));

            TrackInputPerformance(startTime, "CombatInput");
        }

        /// <summary>
        /// Process UI input
        /// </summary>
        /// <param name="uiData">The UI input data</param>
        public void ProcessUIInput(UIInputData uiData)
        {
            if (!isInputEnabled)
                return;

            var startTime = Time.realtimeSinceStartup;
            frameInputCount++;

            // Update current state
            currentInputState.ui = uiData;

            // Distribute to event system
            onUIInputEvent?.Raise(uiData);

            // Process through priority-ordered handlers
            ProcessInputThroughHandlers(uiData, (handler, data) => handler.HandleUIInput(data));

            TrackInputPerformance(startTime, "UIInput");
        }

        #endregion

        #region Private Methods

        private void InitializePriorityDictionaries()
        {
            foreach (InputPriority priority in Enum.GetValues(typeof(InputPriority)))
            {
                prioritizedHandlers[priority] = new List<IInputHandler>();
            }

            foreach (InputContext context in Enum.GetValues(typeof(InputContext)))
            {
                contextHandlers[context] = new List<IInputHandler>();
            }
        }

        private void AddToPriorityList(IInputHandler handler)
        {
            if (prioritizedHandlers.ContainsKey(handler.Priority))
            {
                prioritizedHandlers[handler.Priority].Add(handler);
            }
        }

        private void RemoveFromPriorityList(IInputHandler handler)
        {
            if (prioritizedHandlers.ContainsKey(handler.Priority))
            {
                prioritizedHandlers[handler.Priority].Remove(handler);
            }
        }

        private void AddToContextList(IInputHandler handler)
        {
            if (contextHandlers.ContainsKey(handler.SupportedContext))
            {
                contextHandlers[handler.SupportedContext].Add(handler);
            }
        }

        private void RemoveFromContextList(IInputHandler handler)
        {
            if (contextHandlers.ContainsKey(handler.SupportedContext))
            {
                contextHandlers[handler.SupportedContext].Remove(handler);
            }
        }

        private bool ShouldProcessInput(InputActionData inputData)
        {
            return currentInputState.ShouldProcessInput(currentContext);
        }

        private void ProcessInputThroughHandlers<T>(T inputData, Func<IInputHandler, T, bool> handlerMethod) where T : struct
        {
            // Get handlers for current context, ordered by priority
            var relevantHandlers = GetOrderedHandlersForContext(currentContext);

            foreach (var handler in relevantHandlers)
            {
                if (!handler.IsActive || !handler.ShouldProcessInput(currentContext))
                    continue;

                try
                {
                    // If handler consumes the input, stop processing
                    if (handlerMethod(handler, inputData))
                    {
                        if (enableInputDebugging)
                            Debug.Log($"[InputManager] Input consumed by: {handler.GetType().Name}");
                        break;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[InputManager] Error processing input in handler {handler.GetType().Name}: {e.Message}");
                }
            }
        }

        private IEnumerable<IInputHandler> GetOrderedHandlersForContext(InputContext context)
        {
            var relevantHandlers = new List<IInputHandler>();

            // Get handlers for specific context
            if (contextHandlers.ContainsKey(context))
            {
                relevantHandlers.AddRange(contextHandlers[context]);
            }

            // Add gameplay handlers if not in gameplay context (they serve as fallback)
            if (context != InputContext.Gameplay && contextHandlers.ContainsKey(InputContext.Gameplay))
            {
                relevantHandlers.AddRange(contextHandlers[InputContext.Gameplay]);
            }

            // Order by priority (highest first)
            return relevantHandlers
                .Where(h => h.IsActive)
                .OrderByDescending(h => (int)h.Priority)
                .Distinct();
        }

        private void ProcessQueuedInputEvents()
        {
            // This method can be extended to handle any queued input events
            // Currently, all input is processed immediately
        }

        private void TrackInputPerformance(float startTime, string inputType)
        {
            var processingTime = Time.realtimeSinceStartup - startTime;
            lastInputProcessTime = processingTime;

            if (processingTime > inputProcessingTimeThreshold)
            {
                Debug.LogWarning($"[InputManager] {inputType} processing took {processingTime:F4}s (threshold: {inputProcessingTimeThreshold:F4}s)");
            }

            if (enableInputDebugging && frameInputCount == 1) // Log once per frame
            {
                Debug.Log($"[InputManager] Frame input processing: {processingTime:F4}s");
            }
        }

        #endregion

        #region Debug Information

        /// <summary>
        /// Get current input system statistics for debugging
        /// </summary>
        /// <returns>Formatted debug information</returns>
        public string GetDebugInfo()
        {
            return $"InputManager Debug Info:\n" +
                   $"Context: {currentContext}\n" +
                   $"Enabled: {isInputEnabled}\n" +
                   $"Registered Handlers: {registeredHandlers.Count}\n" +
                   $"Frame Input Count: {frameInputCount}\n" +
                   $"Last Process Time: {lastInputProcessTime:F4}s\n" +
                   $"Active Handlers: {registeredHandlers.Count(h => h.IsActive)}";
        }

        /// <summary>
        /// Get current input state for debugging
        /// </summary>
        /// <returns>Current input state</returns>
        public InputStateData GetCurrentInputState()
        {
            return currentInputState;
        }

        #endregion
    }
}