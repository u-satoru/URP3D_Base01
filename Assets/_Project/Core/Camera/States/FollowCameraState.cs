using UnityEngine;

namespace asterivo.Unity60.Core.Camera.States
{
    /// <summary>
    /// Third-person camera state that follows a target with configurable offset and smooth movement.
    /// Ideal for platformers, action games, and third-person experiences.
    /// </summary>
    public class FollowCameraState<TController> : BaseCameraState<TController>
        where TController : class, ICameraController
    {
        #region Configuration

        [Header("Follow Settings")]
        [SerializeField] private Vector3 followOffset = new Vector3(0, 2, -5);
        [SerializeField] private float followSpeed = 5f;
        [SerializeField] private float rotationSpeed = 3f;
        [SerializeField] private bool useFixedOffset = true;

        [Header("Look Settings")]
        [SerializeField] private float lookSensitivity = 2f;
        [SerializeField] private bool invertY = false;
        [SerializeField] private Vector2 verticalLookLimits = new Vector2(-30f, 60f);

        [Header("Distance Control")]
        [SerializeField] private float minDistance = 2f;
        [SerializeField] private float maxDistance = 10f;
        [SerializeField] private float zoomSpeed = 2f;

        #endregion

        #region Private Fields

        private Transform cameraTransform;
        private Vector3 currentOffset;
        private float currentDistance;
        private Vector2 lookAngles;
        private Vector3 velocity;

        #endregion

        #region Constructor

        public FollowCameraState() : base("FollowCamera")
        {
            currentOffset = followOffset;
            currentDistance = followOffset.magnitude;
        }

        public FollowCameraState(Vector3 offset, float speed = 5f) : base("FollowCamera")
        {
            followOffset = offset;
            followSpeed = speed;
            currentOffset = followOffset;
            currentDistance = followOffset.magnitude;
        }

        #endregion

        #region BaseCameraState Implementation

        protected override void OnEnterState(TController context)
        {
            cameraTransform = context.CameraTransform;

            if (cameraTransform != null && context.Target != null)
            {
                // Initialize camera position based on target
                Vector3 targetPosition = context.Target.position + followOffset;
                cameraTransform.position = targetPosition;
                cameraTransform.LookAt(context.Target.position);

                // Initialize look angles from current rotation
                lookAngles.y = cameraTransform.eulerAngles.y;
                lookAngles.x = cameraTransform.eulerAngles.x;
            }
        }

        protected override void OnExitState(TController context)
        {
            // Clean up any state-specific resources if needed
        }

        protected override void OnUpdateState(TController context)
        {
            if (context.Target == null || cameraTransform == null) return;

            // Update camera position and rotation based on target
            UpdateCameraPosition(context);
        }

        protected override void OnLateUpdateState(TController context)
        {
            if (context.Target == null || cameraTransform == null) return;

            // Apply final camera positioning in LateUpdate for smooth following
            ApplyCameraTransform(context);
        }

        protected override void OnHandleInput(TController context, CameraInputData inputData)
        {
            if (context.Target == null || cameraTransform == null) return;

            // Process look input
            Vector2 lookInput = inputData.GetProcessedLookInput();
            if (lookInput.sqrMagnitude > 0.001f)
            {
                ProcessLookInput(lookInput, inputData.deltaTime);
            }

            // Process zoom input
            float zoomInput = inputData.GetProcessedZoomInput();
            if (Mathf.Abs(zoomInput) > 0.001f)
            {
                ProcessZoomInput(zoomInput, inputData.deltaTime);
            }

            // Handle camera reset
            if (inputData.resetCameraPressed)
            {
                ResetCameraPosition(context);
            }
        }

        #endregion

        #region Camera Control Methods

        private void UpdateCameraPosition(TController context)
        {
            Vector3 targetPosition = context.Target.position;

            if (useFixedOffset)
            {
                // Fixed offset relative to target
                Vector3 desiredPosition = targetPosition + currentOffset;
                cameraTransform.position = SmoothMove(cameraTransform.position, desiredPosition, followSpeed);
            }
            else
            {
                // Orbital camera based on look angles and distance
                Quaternion rotation = Quaternion.Euler(lookAngles.x, lookAngles.y, 0);
                Vector3 direction = rotation * Vector3.back;
                Vector3 desiredPosition = targetPosition + direction * currentDistance;

                cameraTransform.position = SmoothDamp(cameraTransform.position, desiredPosition, ref velocity, 1f / followSpeed);
            }
        }

        private void ApplyCameraTransform(TController context)
        {
            if (useFixedOffset)
            {
                // Look at target with optional offset
                Vector3 lookTarget = context.Target.position + Vector3.up * 1.5f;
                Vector3 direction = (lookTarget - cameraTransform.position).normalized;
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                cameraTransform.rotation = SmoothRotate(cameraTransform.rotation, targetRotation, rotationSpeed);
            }
            else
            {
                // Apply orbital rotation
                Quaternion targetRotation = Quaternion.Euler(lookAngles.x, lookAngles.y, 0);
                cameraTransform.rotation = SmoothRotate(cameraTransform.rotation, targetRotation, rotationSpeed);
            }
        }

        private void ProcessLookInput(Vector2 lookInput, float deltaTime)
        {
            // Apply sensitivity and inversion
            lookInput *= lookSensitivity * deltaTime * 100f;
            if (invertY) lookInput.y = -lookInput.y;

            // Update look angles
            lookAngles.y += lookInput.x;
            lookAngles.x -= lookInput.y;

            // Clamp vertical look
            lookAngles.x = ClampAngle(lookAngles.x, verticalLookLimits.x, verticalLookLimits.y);

            // Normalize horizontal angle
            lookAngles.y = Mathf.Repeat(lookAngles.y, 360f);
        }

        private void ProcessZoomInput(float zoomInput, float deltaTime)
        {
            currentDistance = Mathf.Clamp(currentDistance - zoomInput * zoomSpeed * deltaTime, minDistance, maxDistance);

            if (useFixedOffset)
            {
                // Scale the offset based on zoom
                currentOffset = followOffset.normalized * currentDistance;
            }
        }

        private void ResetCameraPosition(TController context)
        {
            if (context.Target == null) return;

            // Reset to default position behind target
            Vector3 targetForward = context.Target.forward;
            if (targetForward.sqrMagnitude < 0.1f)
                targetForward = Vector3.forward;

            lookAngles.y = Quaternion.LookRotation(targetForward).eulerAngles.y + 180f;
            lookAngles.x = 0f;
            currentDistance = followOffset.magnitude;
            currentOffset = followOffset;
        }

        #endregion

        #region Public Configuration Methods

        /// <summary>
        /// Configure the follow offset for this camera state
        /// </summary>
        /// <param name="offset">New follow offset</param>
        public void SetFollowOffset(Vector3 offset)
        {
            followOffset = offset;
            currentOffset = offset;
            currentDistance = offset.magnitude;
        }

        /// <summary>
        /// Configure the follow and rotation speeds
        /// </summary>
        /// <param name="followSpd">Follow speed</param>
        /// <param name="rotationSpd">Rotation speed</param>
        public void SetSpeeds(float followSpd, float rotationSpd)
        {
            followSpeed = Mathf.Max(0.1f, followSpd);
            rotationSpeed = Mathf.Max(0.1f, rotationSpd);
        }

        /// <summary>
        /// Configure zoom constraints
        /// </summary>
        /// <param name="min">Minimum distance</param>
        /// <param name="max">Maximum distance</param>
        public void SetZoomLimits(float min, float max)
        {
            minDistance = Mathf.Max(0.1f, min);
            maxDistance = Mathf.Max(minDistance + 0.1f, max);
            currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);
        }

        /// <summary>
        /// Enable or disable fixed offset mode
        /// </summary>
        /// <param name="useFixed">Use fixed offset (true) or orbital camera (false)</param>
        public void SetFixedOffsetMode(bool useFixed)
        {
            useFixedOffset = useFixed;
        }

        #endregion

        #region Debug Information

        public override string ToString()
        {
            return $"FollowCameraState: Offset={currentOffset:F2}, Distance={currentDistance:F2}, " +
                   $"LookAngles=({lookAngles.x:F1}, {lookAngles.y:F1}), FixedOffset={useFixedOffset}";
        }

        #endregion
    }
}
