using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace asterivo.Unity60.Core.StateMachine.Interfaces
{
    /// <summary>
    /// Interface for hierarchical state implementations that support child states.
    /// Extends the base IState interface with hierarchical functionality.
    /// </summary>
    /// <typeparam name="TContext">The context type that states operate on</typeparam>
    public interface IHierarchicalState<TContext> : asterivo.Unity60.Core.Patterns.StateMachine.IState<TContext>
    {
        /// <summary>
        /// Transition to a specified child state
        /// </summary>
        /// <param name="childStateKey">Key identifier of the child state to transition to</param>
        /// <param name="context">The context object this state operates on</param>
        void TransitionToChild(string childStateKey, TContext context);

        /// <summary>
        /// Check if a specified child state exists
        /// </summary>
        /// <param name="childStateKey">Key identifier of the child state to check</param>
        /// <returns>True if the child state exists</returns>
        bool HasChildState(string childStateKey);

        /// <summary>
        /// Get the key of the currently active child state
        /// </summary>
        /// <returns>Key of the current child state, or null if no child state is active</returns>
        string GetCurrentChildStateKey();

        /// <summary>
        /// Get read-only collection of all child states
        /// </summary>
        /// <returns>Read-only dictionary of child states with their keys</returns>
        IReadOnlyDictionary<string, asterivo.Unity60.Core.Patterns.StateMachine.IState<TContext>> GetChildStates();

        /// <summary>
        /// Check if transition to specified child state is allowed
        /// </summary>
        /// <param name="childStateKey">Key identifier of the child state</param>
        /// <param name="context">The context object this state operates on</param>
        /// <returns>True if transition is allowed</returns>
        bool CanTransitionToChild(string childStateKey, TContext context);

        /// <summary>
        /// Get the key of the previous child state
        /// </summary>
        /// <returns>Key of the previous child state, or null if no previous state</returns>
        string GetPreviousChildState();

        /// <summary>
        /// Get the history of child state transitions
        /// </summary>
        /// <returns>Read-only collection of state transition history</returns>
        IReadOnlyCollection<string> GetStateHistory();
    }
}
