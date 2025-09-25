using UnityEngine;

namespace asterivo.Unity60.Core.Camera.States
{
    /// <summary>
    /// Base implementation for camera states providing common functionality.
    /// Provides default implementations and utility methods for derived states.
    /// </summary>
    /// <typeparam name="TController">Type of camera controller</typeparam>
    public abstract class BaseCameraState<TController> : ICameraState<TController>
    {
        #region Protected Fields

        protected bool isInitialized = false;
        protected float stateEnterTime;
        protected string stateName;

        #endregion

        #region Constructor

        protected BaseCameraState(string name)
        {
            stateName = name;
        }

        #endregion

        #region ICameraState Implementation

        public virtual void Enter(TController context)
        {
            stateEnterTime = Time.time;
            isInitialized = true;

            LogDebug($"Entering camera state: {stateName}");
            OnEnterState(context);
        }

        public virtual void Exit(TController context)
        {
            LogDebug($"Exiting camera state: {stateName}");
            OnExitState(context);
            isInitialized = false;
        }

        public virtual void Update(TController context)
        {
            if (!isInitialized) return;
            OnUpdateState(context);
        }

        public virtual void LateUpdate(TController context)
        {
            if (!isInitialized) return;
            OnLateUpdateState(context);
        }

        public virtual void FixedUpdate(TController context)
        {
            if (!isInitialized) return;
            OnFixedUpdateState(context);
        }

        public virtual void HandleInput(TController context, object inputData)
        {
            if (!isInitialized) return;

            if (inputData is CameraInputData cameraInput)
            {
                OnHandleInput(context, cameraInput);
            }
            else
            {
                LogWarning($"Invalid input data type for camera state {stateName}: {inputData?.GetType().Name ?? "null"}");
            }
        }

        #endregion

        #region Abstract Methods

        /// <summary>
        /// Called when entering this camera state.
        /// Override in derived classes to implement state-specific enter logic.
        /// </summary>
        /// <param name="context">Camera controller context</param>
        protected abstract void OnEnterState(TController context);

        /// <summary>
        /// Called when exiting this camera state.
        /// Override in derived classes to implement state-specific exit logic.
        /// </summary>
        /// <param name="context">Camera controller context</param>
        protected abstract void OnExitState(TController context);

        /// <summary>
        /// Called every frame during Update.
        /// Override in derived classes to implement state-specific update logic.
        /// </summary>
        /// <param name="context">Camera controller context</param>
        protected abstract void OnUpdateState(TController context);

        /// <summary>
        /// Called every frame during LateUpdate.
        /// Override in derived classes to implement camera movement and positioning.
        /// </summary>
        /// <param name="context">Camera controller context</param>
        protected abstract void OnLateUpdateState(TController context);

        #endregion

        #region Virtual Methods

        /// <summary>
        /// Called during FixedUpdate for physics-related camera operations.
        /// Override in derived classes if physics integration is needed.
        /// </summary>
        /// <param name="context">Camera controller context</param>
        protected virtual void OnFixedUpdateState(TController context)
        {
            // Default: no physics update needed
        }

        /// <summary>
        /// Called when input is received for this camera state.
        /// Override in derived classes to implement state-specific input handling.
        /// </summary>
        /// <param name="context">Camera controller context</param>
        /// <param name="inputData">Camera input data</param>
        protected virtual void OnHandleInput(TController context, CameraInputData inputData)
        {
            // Default: no input handling
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Get the time elapsed since entering this state
        /// </summary>
        /// <returns>Time in seconds since state was entered</returns>
        protected float GetTimeInState()
        {
            return Time.time - stateEnterTime;
        }

        /// <summary>
        /// Check if the state has been active for a minimum duration
        /// </summary>
        /// <param name="minDuration">Minimum duration in seconds</param>
        /// <returns>True if state has been active for at least the specified duration</returns>
        protected bool HasBeenActiveFor(float minDuration)
        {
            return GetTimeInState() >= minDuration;
        }

        /// <summary>
        /// Smoothly interpolate between two values using Time.deltaTime
        /// </summary>
        /// <param name="current">Current value</param>
        /// <param name="target">Target value</param>
        /// <param name="speed">Interpolation speed</param>
        /// <returns>Interpolated value</returns>
        protected float SmoothMove(float current, float target, float speed)
        {
            return Mathf.Lerp(current, target, speed * Time.deltaTime);
        }

        /// <summary>
        /// Smoothly interpolate between two Vector3 values using Time.deltaTime
        /// </summary>
        /// <param name="current">Current value</param>
        /// <param name="target">Target value</param>
        /// <param name="speed">Interpolation speed</param>
        /// <returns>Interpolated value</returns>
        protected Vector3 SmoothMove(Vector3 current, Vector3 target, float speed)
        {
            return Vector3.Lerp(current, target, speed * Time.deltaTime);
        }

        /// <summary>
        /// Smoothly interpolate between two Quaternion values using Time.deltaTime
        /// </summary>
        /// <param name="current">Current rotation</param>
        /// <param name="target">Target rotation</param>
        /// <param name="speed">Interpolation speed</param>
        /// <returns>Interpolated rotation</returns>
        protected Quaternion SmoothRotate(Quaternion current, Quaternion target, float speed)
        {
            return Quaternion.Lerp(current, target, speed * Time.deltaTime);
        }

        /// <summary>
        /// Apply smooth damping to a float value
        /// </summary>
        /// <param name="current">Current value</param>
        /// <param name="target">Target value</param>
        /// <param name="velocity">Current velocity (modified by function)</param>
        /// <param name="smoothTime">Smooth time</param>
        /// <returns>Smoothed value</returns>
        protected float SmoothDamp(float current, float target, ref float velocity, float smoothTime)
        {
            return Mathf.SmoothDamp(current, target, ref velocity, smoothTime);
        }

        /// <summary>
        /// Apply smooth damping to a Vector3 value
        /// </summary>
        /// <param name="current">Current value</param>
        /// <param name="target">Target value</param>
        /// <param name="velocity">Current velocity (modified by function)</param>
        /// <param name="smoothTime">Smooth time</param>
        /// <returns>Smoothed value</returns>
        protected Vector3 SmoothDamp(Vector3 current, Vector3 target, ref Vector3 velocity, float smoothTime)
        {
            return Vector3.SmoothDamp(current, target, ref velocity, smoothTime);
        }

        /// <summary>
        /// Clamp an angle to a specific range
        /// </summary>
        /// <param name="angle">Angle to clamp</param>
        /// <param name="min">Minimum angle</param>
        /// <param name="max">Maximum angle</param>
        /// <returns>Clamped angle</returns>
        protected float ClampAngle(float angle, float min, float max)
        {
            if (angle < -360f) angle += 360f;
            if (angle > 360f) angle -= 360f;
            return Mathf.Clamp(angle, min, max);
        }

        /// <summary>
        /// Calculate look rotation from input with optional constraints
        /// </summary>
        /// <param name="currentRotation">Current rotation</param>
        /// <param name="lookInput">Look input</param>
        /// <param name="sensitivity">Look sensitivity</param>
        /// <param name="invertY">Invert Y axis</param>
        /// <param name="clampVertical">Clamp vertical rotation</param>
        /// <param name="minVertical">Minimum vertical angle</param>
        /// <param name="maxVertical">Maximum vertical angle</param>
        /// <returns>New rotation</returns>
        protected Quaternion CalculateLookRotation(Quaternion currentRotation, Vector2 lookInput,
            float sensitivity = 1f, bool invertY = false, bool clampVertical = true,
            float minVertical = -80f, float maxVertical = 80f)
        {
            Vector3 eulerAngles = currentRotation.eulerAngles;

            // Apply look input
            eulerAngles.y += lookInput.x * sensitivity;
            eulerAngles.x += lookInput.y * sensitivity * (invertY ? 1f : -1f);

            // Clamp vertical rotation if needed
            if (clampVertical)
            {
                eulerAngles.x = ClampAngle(eulerAngles.x, minVertical, maxVertical);
            }

            return Quaternion.Euler(eulerAngles);
        }

        #endregion

        #region Debug Methods

        protected virtual void LogDebug(string message)
        {
            // Override in derived classes or use a logger service
            Debug.Log($"[CameraState:{stateName}] {message}");
        }

        protected virtual void LogWarning(string message)
        {
            Debug.LogWarning($"[CameraState:{stateName}] {message}");
        }

        protected virtual void LogError(string message)
        {
            Debug.LogError($"[CameraState:{stateName}] {message}");
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Get the name of this camera state
        /// </summary>
        public string StateName => stateName;

        /// <summary>
        /// Check if this state is currently initialized and active
        /// </summary>
        public bool IsInitialized => isInitialized;

        /// <summary>
        /// Get the time when this state was entered
        /// </summary>
        public float StateEnterTime => stateEnterTime;

        #endregion
    }
}
