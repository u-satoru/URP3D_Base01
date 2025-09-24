using UnityEngine;

namespace asterivo.Unity60.Core.Camera
{
    /// <summary>
    /// Generic camera state interface for the Core layer.
    /// Provides a reusable foundation for camera state management across all game templates.
    /// Integrates with Event-Driven Architecture and ServiceLocator pattern.
    /// </summary>
    /// <remarks>
    /// Design Philosophy:
    /// This Core layer interface provides essential camera state management functionality
    /// that can be inherited and extended by Feature-specific camera implementations.
    ///
    /// Key Design Principles:
    /// - Generic context parameter for flexibility across different camera controllers
    /// - Event-driven state transitions for loose coupling
    /// - Unity lifecycle integration (Update, LateUpdate, FixedUpdate)
    /// - Input abstraction for cross-platform compatibility
    ///
    /// Usage Examples:
    /// - Template cameras: FPS, TPS, Platformer, Stealth camera states
    /// - Cinematic cameras: Cutscene, dialogue, transition cameras
    /// - UI cameras: Menu, inventory, settings camera states
    /// - Special cameras: Death, spectator, replay camera states
    ///
    /// Integration with Features:
    /// Core layer camera states provide the foundation, while Feature layer
    /// implementations add specific behaviors like Cinemachine integration,
    /// advanced physics, or template-specific mechanics.
    /// </remarks>
    /// <typeparam name="TContext">The camera controller context type</typeparam>
    public interface ICameraState<TContext>
    {
        /// <summary>
        /// Called when transitioning into this camera state.
        /// Initialize camera parameters, settings, and prepare for operation.
        /// </summary>
        /// <param name="context">The camera controller managing this state</param>
        /// <remarks>
        /// Implementation guidelines:
        /// - Set up camera position, rotation, and field of view
        /// - Configure input sensitivity and constraints for this state
        /// - Initialize any state-specific UI elements or overlays
        /// - Prepare smooth transition animations from previous state
        /// - Subscribe to relevant events or input channels
        ///
        /// Performance considerations:
        /// - Minimize expensive operations in Enter() as it's called during transitions
        /// - Use object pooling for temporary objects if applicable
        /// - Cache frequently used components during Enter() rather than in Update()
        /// </remarks>
        void Enter(TContext context);

        /// <summary>
        /// Called when transitioning out of this camera state.
        /// Clean up resources and prepare for the next state.
        /// </summary>
        /// <param name="context">The camera controller managing this state</param>
        /// <remarks>
        /// Implementation guidelines:
        /// - Save any persistent state data before exiting
        /// - Clean up temporary objects, effects, or UI elements
        /// - Unsubscribe from events to prevent memory leaks
        /// - Prepare transition data for the next state if needed
        /// - Stop any running coroutines or animations
        ///
        /// Transition considerations:
        /// - Ensure smooth visual transitions by not abruptly changing camera properties
        /// - Consider using fade effects or blend curves for jarring state changes
        /// - Pass relevant data to the next state through the context parameter
        /// </remarks>
        void Exit(TContext context);

        /// <summary>
        /// Called every frame to update camera state logic.
        /// Handle state-specific behaviors, conditions, and non-rendering updates.
        /// </summary>
        /// <param name="context">The camera controller managing this state</param>
        /// <remarks>
        /// Update responsibilities:
        /// - Process state-specific logic and decision making
        /// - Check for state transition conditions
        /// - Update non-visual elements like UI indicators
        /// - Process input for state changes (not camera movement)
        /// - Handle timeout or duration-based state logic
        ///
        /// Performance guidelines:
        /// - Avoid heavy calculations; use caching where possible
        /// - Don't directly modify camera transform here - use LateUpdate instead
        /// - Consider frame rate independence for any timed operations
        /// - Use early exits to minimize unnecessary processing
        /// </remarks>
        void Update(TContext context);

        /// <summary>
        /// Called after all Update() methods have completed.
        /// Handle camera position, rotation, and visual updates here.
        /// </summary>
        /// <param name="context">The camera controller managing this state</param>
        /// <remarks>
        /// LateUpdate responsibilities:
        /// - Apply camera position and rotation changes
        /// - Follow targets or apply physics-based movement
        /// - Handle collision detection and obstacle avoidance
        /// - Apply visual effects like screen shake or smoothing
        /// - Update Cinemachine virtual cameras if used
        ///
        /// Why LateUpdate:
        /// - Ensures camera updates after all other objects have moved
        /// - Provides smooth, lag-free camera following
        /// - Essential for third-person cameras and target tracking
        /// - Prevents visual artifacts from frame-to-frame inconsistencies
        ///
        /// Performance critical:
        /// - This is often the most expensive camera operation
        /// - Minimize complex calculations and prefer precomputed values
        /// - Use LOD systems for distance-based optimizations
        /// </remarks>
        void LateUpdate(TContext context);

        /// <summary>
        /// Called at fixed intervals for physics-related camera updates.
        /// Handle camera physics, collision detection, and fixed-timestep operations.
        /// </summary>
        /// <param name="context">The camera controller managing this state</param>
        /// <remarks>
        /// FixedUpdate use cases:
        /// - Physics-based camera movement and collision
        /// - Consistent camera shake or vibration effects
        /// - Fixed-rate interpolation for smooth motion
        /// - Integration with physics systems for realistic camera behavior
        ///
        /// When to use FixedUpdate vs LateUpdate:
        /// - FixedUpdate: Physics interactions, collision detection
        /// - LateUpdate: Visual smoothing, target following, UI updates
        ///
        /// Implementation notes:
        /// - Not all camera states need FixedUpdate; can be left empty
        /// - Use Time.fixedDeltaTime for frame-rate independent calculations
        /// - Coordinate with physics systems for realistic behavior
        /// </remarks>
        void FixedUpdate(TContext context);

        /// <summary>
        /// Handle input specific to this camera state.
        /// Process camera control inputs and state transition inputs.
        /// </summary>
        /// <param name="context">The camera controller managing this state</param>
        /// <param name="inputData">Input data relevant to camera control</param>
        /// <remarks>
        /// Input handling responsibilities:
        /// - Process look/rotation input with state-specific sensitivity
        /// - Handle zoom, field of view, and camera mode changes
        /// - Detect input combinations for state transitions
        /// - Apply input filtering based on current state conditions
        /// - Support multiple input methods (mouse, gamepad, touch)
        ///
        /// Input design patterns:
        /// - Use input data structures for type safety and performance
        /// - Support platform-specific input handling (PC, Console, Mobile)
        /// - Implement input deadzone and sensitivity curves
        /// - Provide accessibility options for different input methods
        ///
        /// State transition input:
        /// - Handle inputs that trigger camera state changes
        /// - Validate transition conditions before changing states
        /// - Provide visual feedback for invalid transition attempts
        /// </remarks>
        void HandleInput(TContext context, object inputData);
    }
}
