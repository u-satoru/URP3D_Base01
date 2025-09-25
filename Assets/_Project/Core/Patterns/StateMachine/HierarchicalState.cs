using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using asterivo.Unity60.Core.StateMachine.Interfaces;
using asterivo.Unity60.Core.StateMachine.Debug;

namespace asterivo.Unity60.Core.StateMachine
{
    /// <summary>
    /// Abstract base class for hierarchical states that support child states.
    /// Provides comprehensive hierarchical state management with debugging capabilities.
    /// </summary>
    /// <typeparam name="TContext">The context type that states operate on</typeparam>
    public abstract class HierarchicalState<TContext> : IHierarchicalState<TContext>
    {
        #region Private Fields

        /// <summary>
        /// Dictionary for child state management (O(1) access)
        /// </summary>
        protected Dictionary<string, asterivo.Unity60.Core.Patterns.StateMachine.IState<TContext>> childStates;

        /// <summary>
        /// Currently active child state
        /// </summary>
        protected asterivo.Unity60.Core.Patterns.StateMachine.IState<TContext> currentChildState;

        /// <summary>
        /// Key of the default child state to activate on entry
        /// </summary>
        protected string defaultChildStateKey;

        /// <summary>
        /// Stack for state history management
        /// </summary>
        protected Stack<string> stateHistory;

        /// <summary>
        /// Maximum size of state history (memory management)
        /// </summary>
        protected int maxHistorySize = 10;

        /// <summary>
        /// Debug information for state tracking
        /// </summary>
        protected StateDebugInfo debugInfo;

        /// <summary>
        /// Flag to enable/disable debug logging
        /// </summary>
        protected bool enableDebugLogging = false;

        #endregion

        #region Constructor

        /// <summary>
        /// Initialize the hierarchical state
        /// </summary>
        public HierarchicalState()
        {
            childStates = new Dictionary<string, asterivo.Unity60.Core.Patterns.StateMachine.IState<TContext>>();
            stateHistory = new Stack<string>();
            debugInfo = new StateDebugInfo();
            InitializeChildStates();
            defaultChildStateKey = GetDefaultChildStateKey();
        }

        #endregion

        #region IState<TContext> Implementation

        /// <summary>
        /// Called when entering this hierarchical state
        /// </summary>
        /// <param name="context">The context object this state operates on</param>
        public virtual void Enter(TContext context)
        {
            LogStateTransition("Enter", GetType().Name);

            // Execute parent state enter logic
            OnParentEnter(context);

            // Transition to default child state if specified
            if (!string.IsNullOrEmpty(defaultChildStateKey))
            {
                TransitionToChild(defaultChildStateKey, context);
            }
        }

        /// <summary>
        /// Called every frame while in this hierarchical state
        /// </summary>
        /// <param name="context">The context object this state operates on</param>
        public virtual void Update(TContext context)
        {
            // Execute parent state update logic
            OnParentUpdate(context);

            // Update current child state
            currentChildState?.Update(context);
        }

        /// <summary>
        /// Called when exiting this hierarchical state
        /// </summary>
        /// <param name="context">The context object this state operates on</param>
        public virtual void Exit(TContext context)
        {
            LogStateTransition("Exit", GetType().Name);

            // Exit current child state
            currentChildState?.Exit(context);
            currentChildState = null;

            // Execute parent state exit logic
            OnParentExit(context);
        }

        /// <summary>
        /// Called at fixed intervals for physics-related logic
        /// </summary>
        /// <param name="context">The context object this state operates on</param>
        public virtual void FixedUpdate(TContext context)
        {
            // Execute parent state fixed update logic
            OnParentFixedUpdate(context);

            // Fixed update current child state
            currentChildState?.FixedUpdate(context);
        }

        /// <summary>
        /// Handle input events for this hierarchical state
        /// </summary>
        /// <param name="context">The context object this state operates on</param>
        /// <param name="inputData">Input data to process</param>
        public virtual void HandleInput(TContext context, object inputData)
        {
            // Handle input at parent level first
            OnParentHandleInput(context, inputData);

            // Forward input to current child state
            currentChildState?.HandleInput(context, inputData);
        }

        #endregion

        #region IHierarchicalState<TContext> Implementation

        /// <summary>
        /// Transition to a specified child state
        /// </summary>
        /// <param name="childStateKey">Key identifier of the child state</param>
        /// <param name="context">The context object this state operates on</param>
        public void TransitionToChild(string childStateKey, TContext context)
        {
            if (!childStates.ContainsKey(childStateKey))
            {
                LogError($"Child state '{childStateKey}' not found in {GetType().Name}");
                return;
            }

            // Exit current child state
            if (currentChildState != null)
            {
                currentChildState.Exit(context);
                RecordStateHistory(GetCurrentChildStateKey());
            }

            // Transition to new child state
            currentChildState = childStates[childStateKey];
            currentChildState.Enter(context);

            LogStateTransition("ChildTransition", $"{GetType().Name} -> {childStateKey}");
        }

        /// <summary>
        /// Check if a specified child state exists
        /// </summary>
        /// <param name="childStateKey">Key identifier of the child state</param>
        /// <returns>True if the child state exists</returns>
        public bool HasChildState(string childStateKey)
        {
            return childStates.ContainsKey(childStateKey);
        }

        /// <summary>
        /// Get the key of the currently active child state
        /// </summary>
        /// <returns>Key of the current child state, or null if no child state is active</returns>
        public string GetCurrentChildStateKey()
        {
            if (currentChildState == null) return null;

            return childStates.FirstOrDefault(kvp => kvp.Value == currentChildState).Key;
        }

        /// <summary>
        /// Get read-only collection of all child states
        /// </summary>
        /// <returns>Read-only dictionary of child states</returns>
        public IReadOnlyDictionary<string, asterivo.Unity60.Core.Patterns.StateMachine.IState<TContext>> GetChildStates()
        {
            return new ReadOnlyDictionary<string, asterivo.Unity60.Core.Patterns.StateMachine.IState<TContext>>(childStates);
        }

        /// <summary>
        /// Check if transition to specified child state is allowed
        /// </summary>
        /// <param name="childStateKey">Key identifier of the child state</param>
        /// <param name="context">The context object this state operates on</param>
        /// <returns>True if transition is allowed</returns>
        public bool CanTransitionToChild(string childStateKey, TContext context)
        {
            return HasChildState(childStateKey) && ValidateChildTransition(childStateKey, context);
        }

        /// <summary>
        /// Get the key of the previous child state
        /// </summary>
        /// <returns>Key of the previous child state, or null if no previous state</returns>
        public string GetPreviousChildState()
        {
            return stateHistory.Count > 0 ? stateHistory.Peek() : null;
        }

        /// <summary>
        /// Get the history of child state transitions
        /// </summary>
        /// <returns>Read-only collection of state transition history</returns>
        public IReadOnlyCollection<string> GetStateHistory()
        {
            return System.Array.AsReadOnly(stateHistory.ToArray());
        }

        #endregion

        #region Abstract Methods (Must be implemented by subclasses)

        /// <summary>
        /// Initialize child states - must be implemented by subclasses
        /// </summary>
        protected abstract void InitializeChildStates();

        /// <summary>
        /// Get the default child state key - must be implemented by subclasses
        /// </summary>
        /// <returns>Key of the default child state</returns>
        protected abstract string GetDefaultChildStateKey();

        #endregion

        #region Virtual Methods (Can be overridden by subclasses)

        /// <summary>
        /// Validate child state transition - can be overridden for custom validation logic
        /// </summary>
        /// <param name="childStateKey">Key of the target child state</param>
        /// <param name="context">The context object this state operates on</param>
        /// <returns>True if transition is valid</returns>
        protected virtual bool ValidateChildTransition(string childStateKey, TContext context)
        {
            return true;
        }

        /// <summary>
        /// Parent state enter logic - can be overridden for custom behavior
        /// </summary>
        /// <param name="context">The context object this state operates on</param>
        protected virtual void OnParentEnter(TContext context) { }

        /// <summary>
        /// Parent state update logic - can be overridden for custom behavior
        /// </summary>
        /// <param name="context">The context object this state operates on</param>
        protected virtual void OnParentUpdate(TContext context) { }

        /// <summary>
        /// Parent state exit logic - can be overridden for custom behavior
        /// </summary>
        /// <param name="context">The context object this state operates on</param>
        protected virtual void OnParentExit(TContext context) { }

        /// <summary>
        /// Parent state fixed update logic - can be overridden for custom behavior
        /// </summary>
        /// <param name="context">The context object this state operates on</param>
        protected virtual void OnParentFixedUpdate(TContext context) { }

        /// <summary>
        /// Parent state input handling - can be overridden for custom behavior
        /// </summary>
        /// <param name="context">The context object this state operates on</param>
        /// <param name="inputData">Input data to process</param>
        protected virtual void OnParentHandleInput(TContext context, object inputData) { }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Add a child state to this hierarchical state
        /// </summary>
        /// <param name="key">Key identifier for the child state</param>
        /// <param name="state">The child state instance</param>
        protected void AddChildState(string key, asterivo.Unity60.Core.Patterns.StateMachine.IState<TContext> state)
        {
            if (childStates.ContainsKey(key))
            {
                LogWarning($"Child state '{key}' already exists in {GetType().Name}. Overwriting.");
            }

            childStates[key] = state;
        }

        /// <summary>
        /// Remove a child state from this hierarchical state
        /// </summary>
        /// <param name="key">Key identifier of the child state to remove</param>
        protected void RemoveChildState(string key)
        {
            if (childStates.ContainsKey(key))
            {
                if (currentChildState == childStates[key])
                {
                    currentChildState = null;
                }
                childStates.Remove(key);
            }
        }

        /// <summary>
        /// Record state transition in history
        /// </summary>
        /// <param name="stateKey">Key of the state to record</param>
        private void RecordStateHistory(string stateKey)
        {
            if (string.IsNullOrEmpty(stateKey)) return;

            stateHistory.Push(stateKey);

            // Limit history size to prevent memory issues
            while (stateHistory.Count > maxHistorySize)
            {
                var tempStack = new Stack<string>();
                for (int i = 0; i < maxHistorySize; i++)
                {
                    if (stateHistory.Count > 0)
                        tempStack.Push(stateHistory.Pop());
                }
                stateHistory = tempStack;
            }
        }

        #endregion

        #region Debug Methods

        /// <summary>
        /// Log state transition for debugging
        /// </summary>
        /// <param name="transitionType">Type of transition</param>
        /// <param name="details">Transition details</param>
        private void LogStateTransition(string transitionType, string details)
        {
            if (!enableDebugLogging) return;

            debugInfo.RecordTransition(transitionType, details, DateTime.UtcNow);

            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"[HierarchicalState] {transitionType}: {details}");
            #endif
        }

        /// <summary>
        /// Log error message
        /// </summary>
        /// <param name="message">Error message</param>
        private void LogError(string message)
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.LogError($"[HierarchicalState Error] {message}");
            #endif
        }

        /// <summary>
        /// Log warning message
        /// </summary>
        /// <param name="message">Warning message</param>
        private void LogWarning(string message)
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.LogWarning($"[HierarchicalState Warning] {message}");
            #endif
        }

        /// <summary>
        /// Get debug information for this hierarchical state
        /// </summary>
        /// <returns>Debug information container</returns>
        public StateDebugInfo GetDebugInfo()
        {
            return debugInfo;
        }

        /// <summary>
        /// Enable or disable debug logging
        /// </summary>
        /// <param name="enabled">True to enable debug logging</param>
        public void SetDebugLogging(bool enabled)
        {
            enableDebugLogging = enabled;
        }

        #endregion
    }
}
