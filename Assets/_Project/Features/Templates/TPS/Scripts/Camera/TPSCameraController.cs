using UnityEngine;
using asterivo.Unity60.Core.Events;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Features.Templates.TPS.Camera
{
    /// <summary>
    /// TPS Camera Controller
    /// Third-person camera controller with basic camera controls
    /// Provides smooth camera following, aiming modes, and cover system integration
    /// </summary>
    public class TPSCameraController : MonoBehaviour
    {
        public enum CameraMode
        {
            Normal,
            Aiming,
            Cover
        }

        [TabGroup("TPS Camera", "Basic Settings")]
        [LabelText("Follow Target")]
        [SerializeField] private Transform followTarget;

        [LabelText("Camera Distance")]
        [PropertyRange(2f, 10f)]
        [SerializeField] private float normalDistance = 5f;

        [LabelText("Camera Height")]
        [PropertyRange(1f, 5f)]
        [SerializeField] private float cameraHeight = 2f;

        [LabelText("Mouse Sensitivity")]
        [PropertyRange(0.5f, 5f)]
        [SerializeField] private float mouseSensitivity = 2f;

        [TabGroup("TPS Camera", "Aiming Settings")]
        [LabelText("Aim Distance")]
        [PropertyRange(1f, 5f)]
        [SerializeField] private float aimDistance = 2.5f;

        [LabelText("Aim FOV")]
        [PropertyRange(30f, 90f)]
        [SerializeField] private float aimFOV = 45f;

        [LabelText("Normal FOV")]
        [PropertyRange(60f, 90f)]
        [SerializeField] private float normalFOV = 60f;

        [TabGroup("Events")]
        [LabelText("On Camera Mode Changed")]
        [SerializeField] private GameEvent onCameraModeChanged;

        private UnityEngine.Camera cameraComponent;
        private CameraMode currentMode = CameraMode.Normal;
        private Vector2 mouseInput;
        private float currentYaw;
        private float currentPitch;
        private float targetDistance;
        private float targetFOV;

        [TabGroup("Debug")]
        [ReadOnly, ShowInInspector]
        private CameraMode debugCurrentMode => currentMode;

private void Awake()
        {
            cameraComponent = GetComponent<UnityEngine.Camera>();
            if (cameraComponent == null)
                cameraComponent = UnityEngine.Camera.main;

            targetDistance = normalDistance;
            targetFOV = normalFOV;
        }

        private void Start()
        {
            SetCameraMode(CameraMode.Normal);
        }

        private void Update()
        {
            if (followTarget == null) return;

            HandleInput();
            UpdateCameraPosition();
            UpdateCameraSettings();
        }

        private void HandleInput()
        {
            // Mouse input for camera rotation
            mouseInput.x = Input.GetAxis("Mouse X");
            mouseInput.y = Input.GetAxis("Mouse Y");

            currentYaw += mouseInput.x * mouseSensitivity;
            currentPitch -= mouseInput.y * mouseSensitivity;
            currentPitch = Mathf.Clamp(currentPitch, -30f, 60f);
        }

        private void UpdateCameraPosition()
        {
            // Calculate desired position
            Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0);
            Vector3 direction = rotation * Vector3.back;
            Vector3 targetPosition = followTarget.position + Vector3.up * cameraHeight + direction * targetDistance;

            // Apply position smoothly
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 10f);

            // Look at target
            Vector3 lookTarget = followTarget.position + Vector3.up * cameraHeight;
            transform.LookAt(lookTarget);
        }

        private void UpdateCameraSettings()
        {
            // Smooth FOV transition
            if (cameraComponent != null)
            {
                cameraComponent.fieldOfView = Mathf.Lerp(cameraComponent.fieldOfView, targetFOV, Time.deltaTime * 5f);
            }

            // Smooth distance transition
            targetDistance = Mathf.Lerp(targetDistance, GetTargetDistance(), Time.deltaTime * 5f);
        }

        private float GetTargetDistance()
        {
            return currentMode switch
            {
                CameraMode.Aiming => aimDistance,
                CameraMode.Cover => normalDistance * 0.8f,
                _ => normalDistance
            };
        }

        public void SetCameraMode(CameraMode newMode)
        {
            if (currentMode == newMode) return;

            currentMode = newMode;

            targetFOV = currentMode switch
            {
                CameraMode.Aiming => aimFOV,
                _ => normalFOV
            };

            onCameraModeChanged?.Raise();

            Debug.Log($"[TPS Camera] Mode changed to: {newMode}");
        }

        public void SetAimingMode(bool isAiming)
        {
            SetCameraMode(isAiming ? CameraMode.Aiming : CameraMode.Normal);
        }

        public void SetCoverMode(bool inCover)
        {
            SetCameraMode(inCover ? CameraMode.Cover : CameraMode.Normal);
        }

        public void Initialize(Transform target, TPSTemplateConfiguration configuration = null)
        {
            followTarget = target;
            
            if (configuration != null)
            {
                // Apply configuration settings
                normalDistance = configuration.CameraDistance;
                cameraHeight = configuration.CameraHeight;
                mouseSensitivity = configuration.CameraSensitivity;
                
                targetDistance = normalDistance;
                targetFOV = normalFOV;
                
                Debug.Log($"[TPS Camera] Initialized with target: {target.name} and configuration");
            }
            else
            {
                Debug.Log($"[TPS Camera Fallback] Initialized with target: {target.name} (no configuration)");
            }
        }

        // Public properties
        public CameraMode CurrentMode => currentMode;
        public bool IsAiming => currentMode == CameraMode.Aiming;
        public Transform Target => followTarget;

        private void OnDrawGizmosSelected()
        {
            if (followTarget == null) return;

            // Draw camera position and direction
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(followTarget.position + Vector3.up * cameraHeight, 0.2f);

            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, transform.forward * 3f);
        }
    }
}