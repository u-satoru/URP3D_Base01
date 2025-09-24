using UnityEngine;
// using asterivo.Unity60.Core.Commands;

namespace asterivo.Unity60.Core.Camera
{
    /// <summary>
    /// Interface for camera controllers to provide external system integration.
    /// Enables Command Pattern integration and state management access.
    /// </summary>
    public interface ICameraController
    {
        #region Properties

        /// <summary>
        /// Current camera transform
        /// </summary>
        Transform CameraTransform { get; }

        /// <summary>
        /// Current target that the camera is following
        /// </summary>
        Transform Target { get; set; }

        /// <summary>
        /// Current camera state type (for external querying)
        /// </summary>
        System.Enum CurrentStateType { get; }

        /// <summary>
        /// Whether the camera controller is currently active
        /// </summary>
        bool IsActive { get; set; }

        #endregion

        #region Camera Control Methods

        /// <summary>
        /// Set the target for the camera to follow
        /// </summary>
        /// <param name="target">Target transform</param>
        void SetTarget(Transform target);

        /// <summary>
        /// Transition to a specific camera state
        /// </summary>
        /// <param name="stateType">Target state type</param>
        /// <returns>True if transition was successful</returns>
        bool TransitionToState(System.Enum stateType);

        /// <summary>
        /// Handle input for camera control
        /// </summary>
        /// <param name="inputData">Camera input data</param>
        void HandleInput(CameraInputData inputData);

        /// <summary>
        /// Execute a camera command through the Command Pattern
        /// </summary>
        /// <param name="command">Command to execute</param>
        /// <returns>True if command was executed successfully</returns>
        bool ExecuteCommand(ICommand command);

        #endregion

        #region Events

        /// <summary>
        /// Event fired when camera state changes
        /// </summary>
        event System.Action<System.Enum, System.Enum> OnStateChanged;

        /// <summary>
        /// Event fired when camera target changes
        /// </summary>
        event System.Action<Transform> OnTargetChanged;

        /// <summary>
        /// Event fired when camera transform updates
        /// </summary>
        event System.Action<Transform> OnCameraTransformChanged;

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
        /// Get debug information about current camera state
        /// </summary>
        /// <returns>Debug info string</returns>
        string GetDebugInfo();

        #endregion
    }
}
