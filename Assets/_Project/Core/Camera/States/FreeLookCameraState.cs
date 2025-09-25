using UnityEngine;

namespace asterivo.Unity60.Core.Camera.States
{
    /// <summary>
    /// Free-floating camera state with unrestricted movement and rotation.
    /// Ideal for debugging, level editing, cinematics, and spectator modes.
    /// </summary>
    public class FreeLookCameraState<TController> : BaseCameraState<TController>
        where TController : class, ICameraController
    {
        #region Configuration

        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float fastMoveMultiplier = 3f;
        [SerializeField] private float slowMoveMultiplier = 0.3f;
        [SerializeField] private float acceleration = 10f;
        [SerializeField] private float deceleration = 10f;

        [Header("Look Settings")]
        [SerializeField] private float lookSensitivity = 2f;
        [SerializeField] private bool invertY = false;
        [SerializeField] private float smoothSpeed = 5f;

        [Header("Zoom Settings")]
        [SerializeField] private float zoomSpeed = 2f;
        [SerializeField] private float minFOV = 10f;
        [SerializeField] private float maxFOV = 120f;
        [SerializeField] private float defaultFOV = 60f;

        [Header("Focus Settings")]
        [SerializeField] private bool canFocusOnTarget = true;
        [SerializeField] private float focusDistance = 5f;
        [SerializeField] private float focusSpeed = 3f;

        [Header("Constraints")]
        [SerializeField] private bool useMovementBounds = false;
        [SerializeField] private Bounds movementBounds = new Bounds(Vector3.zero, Vector3.one * 100f);

        #endregion

        #region Private Fields

        private Transform cameraTransform;
        private UnityEngine.Camera cameraComponent;
        private Vector2 lookAngles;
        private Vector2 targetLookAngles;
        private Vector3 velocity = Vector3.zero;
        private Vector3 targetVelocity = Vector3.zero;
        private float currentFOV;
        private bool isFocusing = false;
        private Vector3 focusTarget;

        // Input tracking
        private Vector3 currentMoveInput;
        private bool fastMode = false;
        private bool slowMode = false;

        #endregion

        #region Constructor

        public FreeLookCameraState() : base("FreeLook")
        {
            currentFOV = defaultFOV;
        }

        public FreeLookCameraState(float moveSpd, float lookSens) : base("FreeLook")
        {
            moveSpeed = moveSpd;
            lookSensitivity = lookSens;
            currentFOV = defaultFOV;
        }

        #endregion

        #region BaseCameraState Implementation

        protected override void OnEnterState(TController context)
        {
            cameraTransform = context.CameraTransform;
            cameraComponent = cameraTransform?.GetComponent<UnityEngine.Camera>();

            if (cameraTransform != null)
            {
                // Initialize look angles from current rotation
                Vector3 eulerAngles = cameraTransform.eulerAngles;
                lookAngles.x = eulerAngles.x;
                lookAngles.y = eulerAngles.y;

                // Handle angle wrapping for X axis
                if (lookAngles.x > 180f)
                    lookAngles.x -= 360f;

                targetLookAngles = lookAngles;

                // Initialize FOV
                if (cameraComponent != null)
                {
                    currentFOV = cameraComponent.fieldOfView;
                }

                // Reset movement
                velocity = Vector3.zero;
                targetVelocity = Vector3.zero;
                isFocusing = false;
            }
        }

        protected override void OnExitState(TController context)
        {
            // Stop any ongoing movement
            velocity = Vector3.zero;
            targetVelocity = Vector3.zero;
            isFocusing = false;
        }

        protected override void OnUpdateState(TController context)
        {
            if (cameraTransform == null) return;

            // Update movement
            UpdateMovement();

            // Update look smoothing
            if (Vector2.Distance(lookAngles, targetLookAngles) > 0.01f)
            {
                lookAngles = Vector2.Lerp(lookAngles, targetLookAngles, smoothSpeed * Time.deltaTime);
            }

            // Update FOV
            if (cameraComponent != null && Mathf.Abs(cameraComponent.fieldOfView - currentFOV) > 0.1f)
            {
                cameraComponent.fieldOfView = Mathf.Lerp(cameraComponent.fieldOfView, currentFOV, smoothSpeed * Time.deltaTime);
            }

            // Handle focus mode
            if (isFocusing && context.Target != null)
            {
                UpdateFocusMode(context);
            }
        }

        protected override void OnLateUpdateState(TController context)
        {
            if (cameraTransform == null) return;

            // Apply movement
            ApplyMovement();

            // Apply rotation
            ApplyRotation();
        }

        protected override void OnHandleInput(TController context, CameraInputData inputData)
        {
            if (cameraTransform == null) return;

            // Process look input
            Vector2 lookInput = inputData.GetProcessedLookInput();
            if (lookInput.sqrMagnitude > 0.001f)
            {
                ProcessLookInput(lookInput);
            }

            // Process movement input
            ProcessMovementInput(inputData.movementInput);

            // Process zoom input
            float zoomInput = inputData.GetProcessedZoomInput();
            if (Mathf.Abs(zoomInput) > 0.001f)
            {
                ProcessZoomInput(zoomInput);
            }

            // Handle focus toggle
            if (inputData.aimPressed && canFocusOnTarget && context.Target != null)
            {
                ToggleFocusMode(context);
            }

            // Handle camera reset
            if (inputData.resetCameraPressed)
            {
                ResetCamera(context);
            }

            // Handle speed modifiers (this would need additional input data)
            // For now, we'll check for typical gaming conventions
            fastMode = inputData.freeLookHeld; // Assuming this can be repurposed
            // slowMode would need another input binding
        }

        #endregion

        #region Movement System

        private void ProcessMovementInput(Vector2 inputVector)
        {
            // Convert 2D input to 3D movement relative to camera orientation
            Vector3 forward = cameraTransform.forward;
            Vector3 right = cameraTransform.right;
            Vector3 up = Vector3.up; // Always use world up for flying

            // Calculate movement direction
            Vector3 moveDirection = (forward * inputVector.y + right * inputVector.x).normalized;

            // Add vertical movement (could be bound to additional keys)
            // For now, we'll use a simple approach where movement input Y also affects vertical movement slightly
            currentMoveInput = moveDirection;
        }

        private void UpdateMovement()
        {
            // Calculate target velocity based on input and speed modifiers
            float currentSpeed = moveSpeed;

            if (fastMode)
                currentSpeed *= fastMoveMultiplier;
            else if (slowMode)
                currentSpeed *= slowMoveMultiplier;

            targetVelocity = currentMoveInput * currentSpeed;

            // Apply acceleration/deceleration
            float lerpSpeed = (targetVelocity.magnitude > velocity.magnitude) ? acceleration : deceleration;
            velocity = Vector3.Lerp(velocity, targetVelocity, lerpSpeed * Time.deltaTime);
        }

        private void ApplyMovement()
        {
            if (velocity.magnitude < 0.01f) return;

            Vector3 movement = velocity * Time.deltaTime;
            Vector3 newPosition = cameraTransform.position + movement;

            // Apply movement bounds if enabled
            if (useMovementBounds)
            {
                newPosition = ConstrainToBounds(newPosition);
            }

            cameraTransform.position = newPosition;
        }

        private Vector3 ConstrainToBounds(Vector3 position)
        {
            Vector3 min = movementBounds.min;
            Vector3 max = movementBounds.max;

            position.x = Mathf.Clamp(position.x, min.x, max.x);
            position.y = Mathf.Clamp(position.y, min.y, max.y);
            position.z = Mathf.Clamp(position.z, min.z, max.z);

            return position;
        }

        #endregion

        #region Look System

        private void ProcessLookInput(Vector2 lookInput)
        {
            // Apply sensitivity and inversion
            lookInput *= lookSensitivity;
            if (invertY) lookInput.y = -lookInput.y;

            // Update target look angles
            targetLookAngles.y += lookInput.x;
            targetLookAngles.x -= lookInput.y;

            // Clamp vertical rotation to avoid gimbal lock
            targetLookAngles.x = Mathf.Clamp(targetLookAngles.x, -89f, 89f);

            // Normalize horizontal angle
            targetLookAngles.y = Mathf.Repeat(targetLookAngles.y, 360f);
        }

        private void ApplyRotation()
        {
            Quaternion targetRotation = Quaternion.Euler(lookAngles.x, lookAngles.y, 0);
            cameraTransform.rotation = targetRotation;
        }

        #endregion

        #region Zoom System

        private void ProcessZoomInput(float zoomInput)
        {
            currentFOV = Mathf.Clamp(currentFOV - zoomInput * zoomSpeed, minFOV, maxFOV);
        }

        #endregion

        #region Focus System

        private void ToggleFocusMode(TController context)
        {
            if (context.Target == null) return;

            isFocusing = !isFocusing;

            if (isFocusing)
            {
                focusTarget = context.Target.position;
                LogDebug("Focusing on target: " + context.Target.name);
            }
            else
            {
                LogDebug("Focus mode disabled");
            }
        }

        private void UpdateFocusMode(TController context)
        {
            if (context.Target == null)
            {
                isFocusing = false;
                return;
            }

            // Update focus target position
            focusTarget = context.Target.position;

            // Calculate desired position (orbit around target)
            Vector3 direction = (cameraTransform.position - focusTarget).normalized;
            Vector3 desiredPosition = focusTarget + direction * focusDistance;

            // Smoothly move to desired position
            cameraTransform.position = Vector3.Lerp(cameraTransform.position, desiredPosition, focusSpeed * Time.deltaTime);

            // Look at target
            Vector3 lookDirection = (focusTarget - cameraTransform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(lookDirection);

            // Convert to euler angles and update target look angles
            Vector3 eulerAngles = lookRotation.eulerAngles;
            targetLookAngles.x = eulerAngles.x;
            targetLookAngles.y = eulerAngles.y;

            // Handle angle wrapping
            if (targetLookAngles.x > 180f)
                targetLookAngles.x -= 360f;
        }

        #endregion

        #region Utility Methods

        private void ResetCamera(TController context)
        {
            // Reset to default state
            currentFOV = defaultFOV;
            velocity = Vector3.zero;
            targetVelocity = Vector3.zero;
            isFocusing = false;

            if (context.Target != null)
            {
                // Position camera to look at target from a reasonable distance
                Vector3 offset = new Vector3(0, 2, -5);
                cameraTransform.position = context.Target.position + offset;

                Vector3 direction = (context.Target.position - cameraTransform.position).normalized;
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                Vector3 eulerAngles = lookRotation.eulerAngles;

                targetLookAngles.x = eulerAngles.x;
                targetLookAngles.y = eulerAngles.y;

                if (targetLookAngles.x > 180f)
                    targetLookAngles.x -= 360f;

                lookAngles = targetLookAngles;
            }
            else
            {
                // Reset to neutral orientation
                targetLookAngles = Vector2.zero;
                lookAngles = targetLookAngles;
            }
        }

        #endregion

        #region Public Configuration Methods

        /// <summary>
        /// Configure movement speeds
        /// </summary>
        /// <param name="baseSpeed">Base movement speed</param>
        /// <param name="fastMultiplier">Fast mode multiplier</param>
        /// <param name="slowMultiplier">Slow mode multiplier</param>
        public void SetMovementSpeeds(float baseSpeed, float fastMultiplier = 3f, float slowMultiplier = 0.3f)
        {
            moveSpeed = Mathf.Max(0.1f, baseSpeed);
            fastMoveMultiplier = Mathf.Max(1f, fastMultiplier);
            slowMoveMultiplier = Mathf.Clamp(slowMultiplier, 0.1f, 1f);
        }

        /// <summary>
        /// Configure look sensitivity and behavior
        /// </summary>
        /// <param name="sensitivity">Look sensitivity</param>
        /// <param name="invert">Invert Y axis</param>
        /// <param name="smooth">Smoothing speed</param>
        public void SetLookSettings(float sensitivity, bool invert = false, float smooth = 5f)
        {
            lookSensitivity = Mathf.Max(0.1f, sensitivity);
            invertY = invert;
            smoothSpeed = Mathf.Max(0.1f, smooth);
        }

        /// <summary>
        /// Configure zoom settings
        /// </summary>
        /// <param name="speed">Zoom speed</param>
        /// <param name="min">Minimum FOV</param>
        /// <param name="max">Maximum FOV</param>
        public void SetZoomSettings(float speed, float min = 10f, float max = 120f)
        {
            zoomSpeed = Mathf.Max(0.1f, speed);
            minFOV = Mathf.Clamp(min, 1f, 179f);
            maxFOV = Mathf.Clamp(max, minFOV + 1f, 179f);
        }

        /// <summary>
        /// Configure movement bounds
        /// </summary>
        /// <param name="bounds">Movement bounds</param>
        /// <param name="enabled">Enable bounds checking</param>
        public void SetMovementBounds(Bounds bounds, bool enabled = true)
        {
            movementBounds = bounds;
            useMovementBounds = enabled;
        }

        /// <summary>
        /// Configure focus mode settings
        /// </summary>
        /// <param name="enabled">Enable focus mode</param>
        /// <param name="distance">Focus distance</param>
        /// <param name="speed">Focus transition speed</param>
        public void SetFocusSettings(bool enabled, float distance = 5f, float speed = 3f)
        {
            canFocusOnTarget = enabled;
            focusDistance = Mathf.Max(0.1f, distance);
            focusSpeed = Mathf.Max(0.1f, speed);
        }

        /// <summary>
        /// Get current camera velocity
        /// </summary>
        /// <returns>Current movement velocity</returns>
        public Vector3 GetVelocity()
        {
            return velocity;
        }

        /// <summary>
        /// Check if currently in focus mode
        /// </summary>
        /// <returns>True if focusing on target</returns>
        public bool IsFocusing()
        {
            return isFocusing;
        }

        /// <summary>
        /// Manually set camera position and rotation
        /// </summary>
        /// <param name="position">New position</param>
        /// <param name="rotation">New rotation</param>
        /// <param name="immediate">Apply immediately without smoothing</param>
        public void SetCameraTransform(Vector3 position, Quaternion rotation, bool immediate = false)
        {
            if (cameraTransform == null) return;

            Vector3 eulerAngles = rotation.eulerAngles;
            targetLookAngles.x = eulerAngles.x;
            targetLookAngles.y = eulerAngles.y;

            if (targetLookAngles.x > 180f)
                targetLookAngles.x -= 360f;

            if (immediate)
            {
                cameraTransform.position = position;
                cameraTransform.rotation = rotation;
                lookAngles = targetLookAngles;
                velocity = Vector3.zero;
            }
            else
            {
                // The position will be smoothly interpolated in the next update
                cameraTransform.position = position;
            }
        }

        #endregion

        #region Debug Information

        public override string ToString()
        {
            return $"FreeLookCameraState: Pos={cameraTransform?.position:F2}, " +
                   $"LookAngles=({lookAngles.x:F1}, {lookAngles.y:F1}), " +
                   $"Velocity={velocity.magnitude:F2}, FOV={currentFOV:F1}, Focusing={isFocusing}";
        }

        #endregion
    }
}
