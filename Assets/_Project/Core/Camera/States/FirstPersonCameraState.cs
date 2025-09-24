using UnityEngine;

namespace asterivo.Unity60.Core.Camera.States
{
    /// <summary>
    /// First-person camera state that attaches directly to a target with mouse look functionality.
    /// Ideal for FPS games, immersive experiences, and detailed object inspection.
    /// </summary>
    public class FirstPersonCameraState<TController> : BaseCameraState<TController>
        where TController : class, ICameraController
    {
        #region Configuration

        [Header("Look Settings")]
        [SerializeField] private float lookSensitivity = 2f;
        [SerializeField] private bool invertY = false;
        [SerializeField] private Vector2 verticalLookLimits = new Vector2(-90f, 90f);
        [SerializeField] private float smoothSpeed = 10f;

        [Header("Position Settings")]
        [SerializeField] private Vector3 eyeOffset = new Vector3(0, 1.6f, 0);
        [SerializeField] private bool followTargetRotation = false;
        [SerializeField] private float positionLerpSpeed = 15f;

        [Header("FOV Settings")]
        [SerializeField] private float defaultFOV = 60f;
        [SerializeField] private float aimFOV = 30f;
        [SerializeField] private float fovTransitionSpeed = 3f;

        [Header("Head Bob Settings")]
        [SerializeField] private bool enableHeadBob = true;
        [SerializeField] private float bobFrequency = 2f;
        [SerializeField] private float bobAmplitude = 0.05f;
        [SerializeField] private float bobSmoothness = 5f;

        #endregion

        #region Private Fields

        private Transform cameraTransform;
        private UnityEngine.Camera cameraComponent;
        private Vector2 lookAngles;
        private Vector2 targetLookAngles;
        private Vector3 targetPosition;
        private float targetFOV;
        private bool isAiming = false;

        // Head bob variables
        private float bobTimer = 0f;
        private Vector3 bobOffset = Vector3.zero;
        private Vector3 targetBobOffset = Vector3.zero;
        private bool wasMoving = false;

        #endregion

        #region Constructor

        public FirstPersonCameraState() : base("FirstPerson")
        {
            targetFOV = defaultFOV;
        }

        public FirstPersonCameraState(Vector3 eyePos, float sensitivity = 2f) : base("FirstPerson")
        {
            eyeOffset = eyePos;
            lookSensitivity = sensitivity;
            targetFOV = defaultFOV;
        }

        #endregion

        #region BaseCameraState Implementation

        protected override void OnEnterState(TController context)
        {
            cameraTransform = context.CameraTransform;
            cameraComponent = cameraTransform.GetComponent<UnityEngine.Camera>();

            if (cameraTransform != null && context.Target != null)
            {
                // Initialize position and rotation
                targetPosition = context.Target.position + eyeOffset;
                cameraTransform.position = targetPosition;

                // Initialize look angles from current rotation or target rotation
                if (followTargetRotation)
                {
                    lookAngles.y = context.Target.eulerAngles.y;
                    lookAngles.x = 0f;
                }
                else
                {
                    lookAngles.y = cameraTransform.eulerAngles.y;
                    lookAngles.x = cameraTransform.eulerAngles.x;
                }

                targetLookAngles = lookAngles;

                // Set initial FOV
                if (cameraComponent != null)
                {
                    targetFOV = defaultFOV;
                    cameraComponent.fieldOfView = targetFOV;
                }

                // Reset head bob
                bobTimer = 0f;
                bobOffset = Vector3.zero;
            }
        }

        protected override void OnExitState(TController context)
        {
            // Reset any modifications made to the camera
            if (cameraComponent != null)
            {
                cameraComponent.fieldOfView = defaultFOV;
            }
        }

        protected override void OnUpdateState(TController context)
        {
            if (context.Target == null || cameraTransform == null) return;

            // Update target position
            targetPosition = context.Target.position + eyeOffset;

            // Update head bob if enabled
            if (enableHeadBob)
            {
                UpdateHeadBob(context);
            }

            // Smooth look angles
            lookAngles = Vector2.Lerp(lookAngles, targetLookAngles, smoothSpeed * Time.deltaTime);

            // Update FOV
            if (cameraComponent != null && Mathf.Abs(cameraComponent.fieldOfView - targetFOV) > 0.1f)
            {
                cameraComponent.fieldOfView = Mathf.Lerp(cameraComponent.fieldOfView, targetFOV, fovTransitionSpeed * Time.deltaTime);
            }
        }

        protected override void OnLateUpdateState(TController context)
        {
            if (context.Target == null || cameraTransform == null) return;

            // Apply camera position (with head bob if enabled)
            Vector3 finalPosition = targetPosition + bobOffset;
            cameraTransform.position = Vector3.Lerp(cameraTransform.position, finalPosition, positionLerpSpeed * Time.deltaTime);

            // Apply camera rotation
            Quaternion targetRotation = Quaternion.Euler(lookAngles.x, lookAngles.y, 0);
            cameraTransform.rotation = targetRotation;
        }

        protected override void OnHandleInput(TController context, CameraInputData inputData)
        {
            if (cameraTransform == null) return;

            // Process look input
            Vector2 lookInput = inputData.GetProcessedLookInput();
            if (lookInput.sqrMagnitude > 0.001f)
            {
                ProcessLookInput(lookInput, inputData.deltaTime);
            }

            // Handle aiming
            if (inputData.aimPressed)
            {
                StartAiming();
            }
            else if (inputData.aimReleased)
            {
                StopAiming();
            }

            // Handle camera reset
            if (inputData.resetCameraPressed)
            {
                ResetLook(context);
            }
        }

        #endregion

        #region Look Control Methods

        private void ProcessLookInput(Vector2 lookInput, float deltaTime)
        {
            // Apply sensitivity
            lookInput *= lookSensitivity;
            if (invertY) lookInput.y = -lookInput.y;

            // Update target look angles
            targetLookAngles.y += lookInput.x;
            targetLookAngles.x -= lookInput.y;

            // Clamp vertical look
            targetLookAngles.x = ClampAngle(targetLookAngles.x, verticalLookLimits.x, verticalLookLimits.y);

            // Normalize horizontal angle
            targetLookAngles.y = Mathf.Repeat(targetLookAngles.y, 360f);
        }

        private void ResetLook(TController context)
        {
            if (context.Target != null && followTargetRotation)
            {
                targetLookAngles.y = context.Target.eulerAngles.y;
            }
            else
            {
                targetLookAngles.y = 0f;
            }
            targetLookAngles.x = 0f;
        }

        #endregion

        #region Aiming System

        private void StartAiming()
        {
            isAiming = true;
            targetFOV = aimFOV;
            LogDebug("Started aiming - FOV transition to " + aimFOV);
        }

        private void StopAiming()
        {
            isAiming = false;
            targetFOV = defaultFOV;
            LogDebug("Stopped aiming - FOV transition to " + defaultFOV);
        }

        #endregion

        #region Head Bob System

        private void UpdateHeadBob(TController context)
        {
            // Check if the target is moving (this would need to be provided by the target or input data)
            bool isMoving = IsTargetMoving(context);

            if (isMoving)
            {
                // Increment bob timer
                bobTimer += Time.deltaTime * bobFrequency;

                // Calculate bob offset using sine wave
                float bobX = Mathf.Sin(bobTimer) * bobAmplitude * 0.5f;
                float bobY = Mathf.Abs(Mathf.Sin(bobTimer * 2f)) * bobAmplitude;

                targetBobOffset = new Vector3(bobX, bobY, 0);
            }
            else
            {
                // Smoothly return to center when not moving
                targetBobOffset = Vector3.zero;
                if (!wasMoving)
                {
                    bobTimer = 0f;
                }
            }

            // Smooth the bob offset
            bobOffset = Vector3.Lerp(bobOffset, targetBobOffset, bobSmoothness * Time.deltaTime);
            wasMoving = isMoving;
        }

        private bool IsTargetMoving(TController context)
        {
            // This is a simplified check - in a real implementation, you'd want to check
            // the target's velocity or movement state
            if (context.Target == null) return false;

            // For now, assume movement if the target has a Rigidbody with velocity
            var rb = context.Target.GetComponent<Rigidbody>();
            if (rb != null)
            {
                return rb.linearVelocity.magnitude > 0.1f;
            }

            // Alternative: check if position changed significantly since last frame
            // This would require storing the previous position
            return false;
        }

        #endregion

        #region Public Configuration Methods

        /// <summary>
        /// Configure the eye offset from the target position
        /// </summary>
        /// <param name="offset">Eye position offset</param>
        public void SetEyeOffset(Vector3 offset)
        {
            eyeOffset = offset;
        }

        /// <summary>
        /// Configure look sensitivity and inversion
        /// </summary>
        /// <param name="sensitivity">Mouse look sensitivity</param>
        /// <param name="invert">Invert Y axis</param>
        public void SetLookSettings(float sensitivity, bool invert = false)
        {
            lookSensitivity = Mathf.Max(0.1f, sensitivity);
            invertY = invert;
        }

        /// <summary>
        /// Configure vertical look limits
        /// </summary>
        /// <param name="minAngle">Minimum vertical look angle</param>
        /// <param name="maxAngle">Maximum vertical look angle</param>
        public void SetVerticalLookLimits(float minAngle, float maxAngle)
        {
            verticalLookLimits.x = Mathf.Clamp(minAngle, -90f, 90f);
            verticalLookLimits.y = Mathf.Clamp(maxAngle, -90f, 90f);
        }

        /// <summary>
        /// Configure FOV settings for normal and aim modes
        /// </summary>
        /// <param name="normalFOV">Default field of view</param>
        /// <param name="aimingFOV">Field of view when aiming</param>
        public void SetFOVSettings(float normalFOV, float aimingFOV)
        {
            defaultFOV = Mathf.Clamp(normalFOV, 10f, 179f);
            aimFOV = Mathf.Clamp(aimingFOV, 10f, 179f);

            if (!isAiming)
            {
                targetFOV = defaultFOV;
            }
        }

        /// <summary>
        /// Configure head bob settings
        /// </summary>
        /// <param name="enabled">Enable head bob</param>
        /// <param name="frequency">Bob frequency</param>
        /// <param name="amplitude">Bob amplitude</param>
        public void SetHeadBobSettings(bool enabled, float frequency = 2f, float amplitude = 0.05f)
        {
            enableHeadBob = enabled;
            bobFrequency = Mathf.Max(0.1f, frequency);
            bobAmplitude = Mathf.Max(0f, amplitude);
        }

        /// <summary>
        /// Get current look angles
        /// </summary>
        /// <returns>Current look angles (x = vertical, y = horizontal)</returns>
        public Vector2 GetLookAngles()
        {
            return lookAngles;
        }

        /// <summary>
        /// Set look angles directly (useful for cutscenes or forced camera directions)
        /// </summary>
        /// <param name="angles">Target look angles</param>
        /// <param name="immediate">Apply immediately without smoothing</param>
        public void SetLookAngles(Vector2 angles, bool immediate = false)
        {
            targetLookAngles = angles;
            targetLookAngles.x = ClampAngle(targetLookAngles.x, verticalLookLimits.x, verticalLookLimits.y);

            if (immediate)
            {
                lookAngles = targetLookAngles;
            }
        }

        #endregion

        #region Debug Information

        public override string ToString()
        {
            return $"FirstPersonCameraState: LookAngles=({lookAngles.x:F1}, {lookAngles.y:F1}), " +
                   $"FOV={targetFOV:F1}, Aiming={isAiming}, HeadBob={enableHeadBob}";
        }

        #endregion
    }
}