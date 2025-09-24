using System;
using System.Collections.Generic;
using UnityEngine;

namespace asterivo.Unity60.Core.Patterns.StateMachine
{
    /// <summary>
    /// Generic state machine implementation using Dictionary-based high-speed state lookup.
    /// Provides type-safe state management with event-driven architecture support.
    /// </summary>
    /// <typeparam name="TStateType">The enum type used for state identification</typeparam>
    /// <typeparam name="TContext">The context type that states operate on</typeparam>
    public class StateMachine<TStateType, TContext> where TStateType : Enum
    {
        #region Private Fields

        /// <summary>
        /// Dictionary for high-speed state lookup (O(1) access)
        /// </summary>
        private readonly Dictionary<TStateType, IState<TContext>> _states;

        /// <summary>
        /// Current active state
        /// </summary>
        private IState<TContext> _currentState;

        /// <summary>
        /// Current state type identifier
        /// </summary>
        private TStateType _currentStateType;

        /// <summary>
        /// Previous state type identifier for rollback support
        /// </summary>
        private TStateType _previousStateType;

        /// <summary>
        /// Context object that states operate on
        /// </summary>
        private readonly TContext _context;

        /// <summary>
        /// Debug logging enabled flag
        /// </summary>
        private readonly bool _enableDebugLog;

        #endregion

        #region Events

        /// <summary>
        /// Event fired when state changes (old state, new state)
        /// </summary>
        public event Action<TStateType, TStateType> OnStateChanged;

        /// <summary>
        /// Event fired before state transition (from state, to state) - can be used for validation
        /// </summary>
        public event Func<TStateType, TStateType, bool> OnStateTransitionRequested;

        #endregion

        #region Properties

        /// <summary>
        /// Current state type
        /// </summary>
        public TStateType CurrentStateType => _currentStateType;

        /// <summary>
        /// Previous state type
        /// </summary>
        public TStateType PreviousStateType => _previousStateType;

        /// <summary>
        /// Check if state machine is in the specified state
        /// </summary>
        /// <param name="stateType">State to check</param>
        /// <returns>True if currently in the specified state</returns>
        public bool IsInState(TStateType stateType) => EqualityComparer<TStateType>.Default.Equals(_currentStateType, stateType);

        /// <summary>
        /// Check if state machine is in any of the specified states
        /// </summary>
        /// <param name="stateTypes">States to check</param>
        /// <returns>True if currently in any of the specified states</returns>
        public bool IsInAnyState(params TStateType[] stateTypes)
        {
            foreach (var stateType in stateTypes)
            {
                if (IsInState(stateType))
                    return true;
            }
            return false;
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Initialize the state machine with context and optional debug logging
        /// </summary>
        /// <param name="context">Context object that states will operate on</param>
        /// <param name="enableDebugLog">Enable debug logging for state transitions</param>
        public StateMachine(TContext context, bool enableDebugLog = false)
        {
            _context = context;
            _enableDebugLog = enableDebugLog;
            _states = new Dictionary<TStateType, IState<TContext>>();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Register a state with the state machine
        /// </summary>
        /// <param name="stateType">State type identifier</param>
        /// <param name="state">State implementation</param>
        public void RegisterState(TStateType stateType, IState<TContext> state)
        {
            if (_states.ContainsKey(stateType))
            {
                if (_enableDebugLog)
                    Debug.LogWarning($"StateMachine: State {stateType} is already registered. Replacing existing state.");
            }

            _states[stateType] = state;

            if (_enableDebugLog)
                Debug.Log($"StateMachine: Registered state {stateType}");
        }

        /// <summary>
        /// Transition to the specified state
        /// </summary>
        /// <param name="newStateType">Target state to transition to</param>
        /// <returns>True if transition was successful</returns>
        public bool TransitionTo(TStateType newStateType)
        {
            // Check if already in the target state
            if (IsInState(newStateType))
            {
                if (_enableDebugLog)
                    Debug.Log($"StateMachine: Already in state {newStateType}, skipping transition");
                return true;
            }

            // Check if target state is registered
            if (!_states.ContainsKey(newStateType))
            {
                Debug.LogError($"StateMachine: State {newStateType} is not registered");
                return false;
            }

            // Check transition validation
            if (OnStateTransitionRequested != null)
            {
                bool transitionAllowed = OnStateTransitionRequested.Invoke(_currentStateType, newStateType);
                if (!transitionAllowed)
                {
                    if (_enableDebugLog)
                        Debug.Log($"StateMachine: Transition from {_currentStateType} to {newStateType} was blocked by validation");
                    return false;
                }
            }

            // Store previous state
            _previousStateType = _currentStateType;

            // Exit current state
            _currentState?.Exit(_context);

            // Update current state
            _currentStateType = newStateType;
            _currentState = _states[newStateType];

            // Enter new state
            _currentState.Enter(_context);

            // Fire state change event
            OnStateChanged?.Invoke(_previousStateType, _currentStateType);

            if (_enableDebugLog)
                Debug.Log($"StateMachine: Transitioned from {_previousStateType} to {_currentStateType}");

            return true;
        }

        /// <summary>
        /// Transition back to the previous state
        /// </summary>
        /// <returns>True if transition was successful</returns>
        public bool TransitionToPrevious()
        {
            return TransitionTo(_previousStateType);
        }

        /// <summary>
        /// Initialize state machine with default state
        /// </summary>
        /// <param name="initialState">Initial state to start with</param>
        /// <returns>True if initialization was successful</returns>
        public bool Initialize(TStateType initialState)
        {
            if (!_states.ContainsKey(initialState))
            {
                Debug.LogError($"StateMachine: Initial state {initialState} is not registered");
                return false;
            }

            _currentStateType = initialState;
            _previousStateType = initialState;
            _currentState = _states[initialState];
            _currentState.Enter(_context);

            if (_enableDebugLog)
                Debug.Log($"StateMachine: Initialized with state {initialState}");

            return true;
        }

        /// <summary>
        /// Update current state (call from MonoBehaviour.Update)
        /// </summary>
        public void Update()
        {
            _currentState?.Update(_context);
        }

        /// <summary>
        /// Fixed update current state (call from MonoBehaviour.FixedUpdate)
        /// </summary>
        public void FixedUpdate()
        {
            _currentState?.FixedUpdate(_context);
        }

        /// <summary>
        /// Handle input for current state
        /// </summary>
        /// <param name="inputData">Input data to process</param>
        public void HandleInput(object inputData)
        {
            _currentState?.HandleInput(_context, inputData);
        }

        /// <summary>
        /// Force set state without calling Enter/Exit (use with caution)
        /// </summary>
        /// <param name="stateType">State to force set</param>
        public void ForceSetState(TStateType stateType)
        {
            if (!_states.ContainsKey(stateType))
            {
                Debug.LogError($"StateMachine: Cannot force set unregistered state {stateType}");
                return;
            }

            _previousStateType = _currentStateType;
            _currentStateType = stateType;
            _currentState = _states[stateType];

            if (_enableDebugLog)
                Debug.LogWarning($"StateMachine: Force set state to {stateType} (skipped Enter/Exit)");
        }

        /// <summary>
        /// Get all registered state types
        /// </summary>
        /// <returns>Collection of registered state types</returns>
        public IReadOnlyCollection<TStateType> GetRegisteredStates()
        {
            return _states.Keys;
        }

        #endregion
    }
}