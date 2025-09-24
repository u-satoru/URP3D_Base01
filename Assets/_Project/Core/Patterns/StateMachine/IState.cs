using UnityEngine;

namespace asterivo.Unity60.Core.Patterns.StateMachine
{
    /// <summary>
    /// Generic state interface for state machine implementations.
    /// Provides a reusable state pattern that can be used across different systems.
    /// </summary>
    /// <typeparam name="TContext">The context type that states operate on</typeparam>
    public interface IState<TContext>
    {
        /// <summary>
        /// Called once when entering this state.
        /// Used for initialization and setup.
        /// </summary>
        /// <param name="context">The context object this state operates on</param>
        void Enter(TContext context);

        /// <summary>
        /// Called once when exiting this state.
        /// Used for cleanup and finalization.
        /// </summary>
        /// <param name="context">The context object this state operates on</param>
        void Exit(TContext context);

        /// <summary>
        /// Called every frame while in this state.
        /// Used for time-dependent logic and general updates.
        /// </summary>
        /// <param name="context">The context object this state operates on</param>
        void Update(TContext context);

        /// <summary>
        /// Called at fixed intervals while in this state.
        /// Used for physics-related logic and calculations.
        /// </summary>
        /// <param name="context">The context object this state operates on</param>
        void FixedUpdate(TContext context);

        /// <summary>
        /// Handle input events while in this state.
        /// </summary>
        /// <param name="context">The context object this state operates on</param>
        /// <param name="inputData">Input data to process</param>
        void HandleInput(TContext context, object inputData);
    }

    /// <summary>
    /// Base interface for state types.
    /// Used for type-safe state identification.
    /// </summary>
    public interface IStateType
    {
        /// <summary>
        /// The unique identifier for this state type.
        /// </summary>
        int StateId { get; }

        /// <summary>
        /// Human-readable name for this state type.
        /// </summary>
        string StateName { get; }
    }
}