using System;
using UnityEngine;
using asterivo.Unity60.Core.Events;

namespace asterivo.Unity60.Core.Camera
{
    /// <summary>
    /// Core layer camera service interface for centralized camera management.
    /// Provides ServiceLocator integration and event-driven camera control.
    /// Bridges Core layer generic architecture with Features layer concrete implementations.
    /// </summary>
    /// <remarks>
    /// Design Philosophy:
    /// This interface serves as the primary access point for camera functionality across the entire application.
    /// It integrates with the ServiceLocator pattern for dependency injection and uses GameEvent for
    /// event-driven communication with other systems.
    ///
    /// Key Design Principles:
    /// - ServiceLocator Integration: Registered as a singleton service for global access
    /// - Event-Driven Communication: Uses GameEvent system for loose coupling
    /// - State Management: Provides centralized camera state transitions
    /// - Features Layer Bridge: Connects Core abstractions with concrete implementations
    /// - Multi-Camera Support: Handles multiple camera controllers and transitions
    ///
    /// Integration Points:
    /// - PlayerStateMachine: Responds to player state changes for camera adjustments
    /// - Input System: Processes camera input through centralized service
    /// - Cinemachine: Manages VirtualCamera priorities and transitions
    /// - UI Systems: Provides camera state information for UI updates
    ///
    /// Usage Examples:
    /// - Game templates: Each template registers appropriate camera configurations
    /// - Runtime switching: Dynamic camera behavior changes based on game state
    /// - Debug systems: Centralized camera state monitoring and control
    /// - Save/Load: Persistent camera configuration across game sessions
    /// </remarks>
    public interface ICameraService
    {
        #region Properties

        /// <summary>
        /// Current active camera controller
        /// </summary>
        ICameraController CurrentController { get; }

        /// <summary>
        /// Current camera state type (for external querying)
        /// </summary>
        System.Enum CurrentState { get; }

        /// <summary>
        /// Whether the camera service is currently active and processing
        /// </summary>
        bool IsActive { get; set; }

        /// <summary>
        /// Main camera transform (for external systems requiring camera position/rotation)
        /// </summary>
        Transform MainCameraTransform { get; }

        #endregion

        #region Events

        /// <summary>
        /// Event fired when camera controller changes
        /// </summary>
        event Action<ICameraController, ICameraController> OnControllerChanged;

        /// <summary>
        /// Event fired when camera state transitions occur
        /// </summary>
        event Action<System.Enum, System.Enum> OnStateChanged;

        /// <summary>
        /// Event fired when camera service activation state changes
        /// </summary>
        event Action<bool> OnServiceActiveChanged;

        #endregion

        #region Camera Controller Management

        /// <summary>
        /// Register a camera controller with the service
        /// </summary>
        /// <param name="controllerId">Unique identifier for the controller</param>
        /// <param name="controller">Camera controller to register</param>
        /// <returns>True if registration was successful</returns>
        bool RegisterController(string controllerId, ICameraController controller);

        /// <summary>
        /// Unregister a camera controller from the service
        /// </summary>
        /// <param name="controllerId">Unique identifier of the controller to remove</param>
        /// <returns>True if unregistration was successful</returns>
        bool UnregisterController(string controllerId);

        /// <summary>
        /// Switch to a specific camera controller
        /// </summary>
        /// <param name="controllerId">Unique identifier of the target controller</param>
        /// <param name="immediateSwitch">Whether to switch immediately or use transition</param>
        /// <returns>True if switch was successful</returns>
        bool SwitchToController(string controllerId, bool immediateSwitch = false);

        /// <summary>
        /// Get registered controller by ID
        /// </summary>
        /// <param name="controllerId">Unique identifier of the controller</param>
        /// <returns>Camera controller if found, null otherwise</returns>
        ICameraController GetController(string controllerId);

        #endregion

        #region State Management

        /// <summary>
        /// Transition current camera to a specific state
        /// </summary>
        /// <param name="stateType">Target camera state</param>
        /// <param name="forceTransition">Whether to force transition even if already in that state</param>
        /// <returns>True if transition was successful</returns>
        bool TransitionToState(System.Enum stateType, bool forceTransition = false);

        /// <summary>
        /// Check if the current camera can transition to a specific state
        /// </summary>
        /// <param name="stateType">Target state to check</param>
        /// <returns>True if transition is possible</returns>
        bool CanTransitionTo(System.Enum stateType);

        /// <summary>
        /// Get available states for the current camera controller
        /// </summary>
        /// <returns>Array of available state types</returns>
        System.Enum[] GetAvailableStates();

        #endregion

        #region Input Handling

        /// <summary>
        /// Process camera input data
        /// </summary>
        /// <param name="inputData">Camera input data to process</param>
        void HandleInput(CameraInputData inputData);

        /// <summary>
        /// Set camera input enabled state
        /// </summary>
        /// <param name="enabled">Whether camera input should be processed</param>
        void SetInputEnabled(bool enabled);

        /// <summary>
        /// Check if camera input is currently enabled
        /// </summary>
        /// <returns>True if input is enabled</returns>
        bool IsInputEnabled();

        #endregion

        #region Target Management

        /// <summary>
        /// Set the primary target for camera following
        /// </summary>
        /// <param name="target">Target transform to follow</param>
        void SetTarget(Transform target);

        /// <summary>
        /// Get current camera target
        /// </summary>
        /// <returns>Current target transform</returns>
        Transform GetTarget();

        /// <summary>
        /// Clear current camera target
        /// </summary>
        void ClearTarget();

        #endregion

        #region Service Lifecycle

        /// <summary>
        /// Initialize the camera service
        /// </summary>
        /// <returns>True if initialization was successful</returns>
        bool Initialize();

        /// <summary>
        /// Cleanup and shutdown the camera service
        /// </summary>
        void Shutdown();

        /// <summary>
        /// Reset camera service to default state
        /// </summary>
        void Reset();

        #endregion

        #region Utility Methods

        /// <summary>
        /// Get current camera position
        /// </summary>
        /// <returns>Camera world position</returns>
        Vector3 GetCameraPosition();

        /// <summary>
        /// Get current camera rotation
        /// </summary>
        /// <returns>Camera world rotation</returns>
        Quaternion GetCameraRotation();

        /// <summary>
        /// Get camera forward direction
        /// </summary>
        /// <returns>Camera forward vector</returns>
        Vector3 GetCameraForward();

        /// <summary>
        /// Check if camera can see a specific point
        /// </summary>
        /// <param name="worldPoint">World point to check</param>
        /// <returns>True if point is within camera view</returns>
        bool CanSeePoint(Vector3 worldPoint);

        /// <summary>
        /// Get comprehensive debug information about camera service state
        /// </summary>
        /// <returns>Debug info string</returns>
        string GetDebugInfo();

        #endregion
    }
}