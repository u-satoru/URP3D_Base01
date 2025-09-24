using UnityEngine;

namespace asterivo.Unity60.Core.Camera
{
    /// <summary>
    /// Input data structure for camera control communication between systems.
    /// Provides type-safe input handling and cross-platform compatibility.
    /// </summary>
    [System.Serializable]
    public struct CameraInputData
    {
        #region Input Fields

        /// <summary>
        /// Look input from mouse/right stick (delta values)
        /// </summary>
        public Vector2 lookInput;

        /// <summary>
        /// Zoom input from scroll wheel or triggers
        /// </summary>
        public float zoomInput;

        /// <summary>
        /// Camera mode change requests (switch between first/third person, etc.)
        /// </summary>
        public bool cameraModeTogglePressed;

        /// <summary>
        /// Aim/focus mode input (right mouse button, left trigger)
        /// </summary>
        public bool aimPressed;
        public bool aimHeld;
        public bool aimReleased;

        /// <summary>
        /// Camera reset input (center camera behind player)
        /// </summary>
        public bool resetCameraPressed;

        /// <summary>
        /// Free look mode input (usually middle mouse or shoulder button)
        /// </summary>
        public bool freeLookHeld;

        #endregion

        #region Context Data

        /// <summary>
        /// Transform that the camera should follow/look at
        /// </summary>
        public Transform targetTransform;

        /// <summary>
        /// Player's movement input for camera adjustment
        /// </summary>
        public Vector2 movementInput;

        /// <summary>
        /// Player's current velocity for prediction
        /// </summary>
        public Vector3 targetVelocity;

        /// <summary>
        /// Current frame's delta time
        /// </summary>
        public float deltaTime;

        #endregion

        #region Sensitivity Settings

        /// <summary>
        /// Look sensitivity multiplier
        /// </summary>
        public float lookSensitivity;

        /// <summary>
        /// Zoom sensitivity multiplier
        /// </summary>
        public float zoomSensitivity;

        /// <summary>
        /// Whether to invert Y-axis
        /// </summary>
        public bool invertY;

        #endregion

        #region Factory Methods

        /// <summary>
        /// Create camera input data from player input system
        /// </summary>
        /// <param name="lookDelta">Look input delta</param>
        /// <param name="target">Target to follow</param>
        /// <param name="sensitivity">Look sensitivity</param>
        /// <param name="invertY">Invert Y axis</param>
        /// <returns>Configured camera input data</returns>
        public static CameraInputData Create(Vector2 lookDelta, Transform target, float sensitivity = 1f, bool invertY = false)
        {
            return new CameraInputData
            {
                lookInput = lookDelta,
                targetTransform = target,
                lookSensitivity = sensitivity,
                invertY = invertY,
                deltaTime = Time.deltaTime
            };
        }

        /// <summary>
        /// Create empty camera input data for default state
        /// </summary>
        /// <returns>Default camera input data</returns>
        public static CameraInputData Empty()
        {
            return new CameraInputData
            {
                lookSensitivity = 1f,
                zoomSensitivity = 1f,
                deltaTime = Time.deltaTime
            };
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Apply sensitivity and inversion to look input
        /// </summary>
        /// <returns>Processed look input</returns>
        public Vector2 GetProcessedLookInput()
        {
            Vector2 processed = lookInput * lookSensitivity;
            if (invertY)
            {
                processed.y = -processed.y;
            }
            return processed;
        }

        /// <summary>
        /// Get processed zoom input with sensitivity
        /// </summary>
        /// <returns>Processed zoom input</returns>
        public float GetProcessedZoomInput()
        {
            return zoomInput * zoomSensitivity;
        }

        /// <summary>
        /// Check if any camera input is active this frame
        /// </summary>
        /// <returns>True if any input is active</returns>
        public bool HasInput()
        {
            return lookInput.sqrMagnitude > 0.001f ||
                   Mathf.Abs(zoomInput) > 0.001f ||
                   cameraModeTogglePressed ||
                   aimPressed ||
                   resetCameraPressed ||
                   freeLookHeld;
        }

        /// <summary>
        /// Get target position with prediction based on velocity
        /// </summary>
        /// <param name="predictionTime">Time to predict ahead</param>
        /// <returns>Predicted target position</returns>
        public Vector3 GetPredictedTargetPosition(float predictionTime = 0.1f)
        {
            if (targetTransform == null) return Vector3.zero;

            return targetTransform.position + (targetVelocity * predictionTime);
        }

        /// <summary>
        /// Get debug information for camera input
        /// </summary>
        /// <returns>Debug info string</returns>
        public string GetDebugInfo()
        {
            return $"Look: {lookInput:F2}, Zoom: {zoomInput:F2}, " +
                   $"Aim: {aimHeld}, Target: {(targetTransform != null ? targetTransform.name : "None")}";
        }

        #endregion
    }
}