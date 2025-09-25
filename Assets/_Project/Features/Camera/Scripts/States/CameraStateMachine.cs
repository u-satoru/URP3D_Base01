using UnityEngine;
using Debug = UnityEngine.Debug;
using System.Collections.Generic;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Camera;
using asterivo.Unity60.Core.Commands;
using asterivo.Unity60.Core;
using asterivo.Unity60.Features.Camera.ViewMode;

namespace asterivo.Unity60.Features.Camera.States
{
    public class CameraStateMachine : MonoBehaviour, ICameraController
    {
        [Header("State Management")]
        [SerializeField] private CameraStateType currentStateType;
        [SerializeField] private CameraStateType previousStateType;
        
        [Header("Camera Components")]
        [SerializeField] private UnityEngine.Camera mainCamera;
        [SerializeField] private Transform cameraRig;
        [SerializeField] private Transform followTarget;
        
        [Header("View Settings")]
        [SerializeField] private FirstPersonSettings firstPersonSettings;
        [SerializeField] private ThirdPersonSettings thirdPersonSettings;
        [SerializeField] private CoverViewSettings coverSettings;
        [SerializeField] private AimSettings aimSettings;
        
        [Header("Events")]
        [SerializeField] private GameEvent<CameraStateType> onCameraStateChanged;

        private ICameraState currentState;
        private Dictionary<CameraStateType, ICameraState> states;
        private bool _isActive = true;

        #region ICameraController Events

        public event System.Action<System.Enum, System.Enum> OnStateChanged;
        public event System.Action<Transform> OnTargetChanged;
        public event System.Action<Transform> OnCameraTransformChanged;

        #endregion
        
        public enum CameraStateType
        {
            FirstPerson,
            ThirdPerson,
            Aim,
            Cover,
            Cinematic,
            Death,
            Menu
        }
        
        #region ICameraController Properties

        public Transform CameraTransform => mainCamera?.transform ?? transform;
        public Transform Target
        {
            get => followTarget;
            set => SetTarget(value);
        }
        public System.Enum CurrentStateType => currentStateType;
        public bool IsActive
        {
            get => _isActive;
            set => SetActive(value);
        }

        #endregion

        #region Legacy Properties

        public UnityEngine.Camera MainCamera => mainCamera;
        public Transform CameraRig => cameraRig;
        public Transform FollowTarget => followTarget;
        public FirstPersonSettings FirstPersonSettings => firstPersonSettings;
        public ThirdPersonSettings ThirdPersonSettings => thirdPersonSettings;
        public CoverViewSettings CoverSettings => coverSettings;
        public AimSettings AimSettings => aimSettings;

        #endregion
        
        private void Awake()
        {
            InitializeComponents();
            InitializeStates();
        }
        
        private void InitializeComponents()
        {
            if (mainCamera == null)
                mainCamera = UnityEngine.Camera.main;
                
            if (cameraRig == null)
                cameraRig = transform;
        }
        
        private void InitializeStates()
        {
            states = new Dictionary<CameraStateType, ICameraState>
            {
                { CameraStateType.FirstPerson, new FirstPersonCameraState() },
                { CameraStateType.ThirdPerson, new ThirdPersonCameraState() },
                { CameraStateType.Aim, new AimCameraState() },
                { CameraStateType.Cover, new CoverCameraState() }
            };
        }
        
        private void Start()
        {
            RegisterWithCameraService();
            TransitionToState(CameraStateType.ThirdPerson);
        }

        private void RegisterWithCameraService()
        {
            var cameraService = ServiceLocator.GetService<ICameraService>();
            if (cameraService != null)
            {
                cameraService.RegisterController(gameObject.name, this);
                Debug.Log($"CameraStateMachine '{gameObject.name}' registered with CameraService");
            }
            else
            {
                Debug.LogWarning("CameraService not found in ServiceLocator. Camera registration failed.");
            }
        }
        
        private void Update()
        {
            currentState?.HandleInput(this);
            currentState?.Update(this);
        }
        
        private void LateUpdate()
        {
            currentState?.LateUpdate(this);

            // Fire camera transform changed event
            OnCameraTransformChanged?.Invoke(CameraTransform);
        }
        
        public void TransitionToState(CameraStateType newStateType)
        {
            if (currentStateType == newStateType && currentState != null)
                return;

            if (!states.ContainsKey(newStateType))
            {
                Debug.LogWarning($"Camera state {newStateType} not found");
                return;
            }

            var previousState = (System.Enum)previousStateType;
            previousStateType = currentStateType;
            currentState?.Exit(this);

            currentStateType = newStateType;
            currentState = states[newStateType];
            currentState.Enter(this);

            onCameraStateChanged?.Raise(currentStateType);
            OnStateChanged?.Invoke(previousState, currentStateType);
        }

        #region ICameraController Implementation

        /// <summary>
        /// Transition to a specific camera state (ICameraController implementation)
        /// </summary>
        /// <param name="stateType">Target state type</param>
        /// <returns>True if transition was successful</returns>
        public bool TransitionToState(System.Enum stateType)
        {
            if (stateType is CameraStateType cameraStateType)
            {
                if (states.ContainsKey(cameraStateType))
                {
                    TransitionToState(cameraStateType);
                    return true;
                }
                else
                {
                    Debug.LogWarning($"Camera state {cameraStateType} not found");
                    return false;
                }
            }
            else
            {
                Debug.LogError($"Invalid state type {stateType}. Expected CameraStateType.");
                return false;
            }
        }

        /// <summary>
        /// Handle input for camera control (ICameraController implementation)
        /// </summary>
        /// <param name="inputData">Camera input data</param>
        public void HandleInput(CameraInputData inputData)
        {
            if (!_isActive) return;

            // Handle camera mode toggle
            if (inputData.cameraModeTogglePressed)
            {
                ToggleCameraMode();
            }

            // Handle aim mode
            if (inputData.aimPressed)
            {
                TransitionToState(CameraStateType.Aim);
            }
            else if (inputData.aimReleased && currentStateType == CameraStateType.Aim)
            {
                ReturnToPreviousState();
            }

            // Handle camera reset
            if (inputData.resetCameraPressed)
            {
                ResetCamera();
            }

            // Pass input to current state for state-specific handling
            currentState?.HandleInput(this);
        }

        /// <summary>
        /// Execute a camera command through the Command Pattern (ICameraController implementation)
        /// </summary>
        /// <param name="command">Command to execute</param>
        /// <returns>True if command was executed successfully</returns>
        public bool ExecuteCommand(ICommand command)
        {
            if (!_isActive)
            {
                Debug.LogWarning("Cannot execute command: Camera controller is not active");
                return false;
            }

            if (command == null)
            {
                Debug.LogError("Cannot execute null command");
                return false;
            }

            try
            {
                command.Execute();
                Debug.Log($"Executed camera command: {command.GetType().Name}");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error executing camera command {command.GetType().Name}: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Set the target for the camera to follow (ICameraController implementation)
        /// </summary>
        /// <param name="target">Target transform</param>
        public void SetTarget(Transform target)
        {
            Transform previousTarget = followTarget;
            followTarget = target;

            if (followTarget != previousTarget)
            {
                OnTargetChanged?.Invoke(followTarget);
                Debug.Log($"Camera target changed to: {(followTarget != null ? followTarget.name : "None")}");
            }
        }

        /// <summary>
        /// Set camera controller active state
        /// </summary>
        /// <param name="active">Active state</param>
        private void SetActive(bool active)
        {
            _isActive = active;
            if (mainCamera != null)
            {
                mainCamera.enabled = active;
            }

            Debug.Log($"Camera controller {(active ? "activated" : "deactivated")}");
        }

        #endregion

        #region ICameraController Utility Methods

        /// <summary>
        /// Get current camera position
        /// </summary>
        /// <returns>Camera world position</returns>
        public Vector3 GetCameraPosition()
        {
            return mainCamera != null ? mainCamera.transform.position : transform.position;
        }

        /// <summary>
        /// Get current camera rotation
        /// </summary>
        /// <returns>Camera world rotation</returns>
        public Quaternion GetCameraRotation()
        {
            return mainCamera != null ? mainCamera.transform.rotation : transform.rotation;
        }

        /// <summary>
        /// Get camera forward direction
        /// </summary>
        /// <returns>Camera forward vector</returns>
        public Vector3 GetCameraForward()
        {
            return mainCamera != null ? mainCamera.transform.forward : transform.forward;
        }

        /// <summary>
        /// Check if camera can see a specific point
        /// </summary>
        /// <param name="worldPoint">World point to check</param>
        /// <returns>True if point is within camera view</returns>
        public bool CanSeePoint(Vector3 worldPoint)
        {
            if (mainCamera == null) return false;

            Vector3 viewportPoint = mainCamera.WorldToViewportPoint(worldPoint);
            return viewportPoint.x >= 0 && viewportPoint.x <= 1 &&
                   viewportPoint.y >= 0 && viewportPoint.y <= 1 &&
                   viewportPoint.z > 0;
        }

        /// <summary>
        /// Get debug information about current camera state
        /// </summary>
        /// <returns>Debug info string</returns>
        public string GetDebugInfo()
        {
            return $"State: {currentStateType}, Target: {(followTarget != null ? followTarget.name : "None")}, " +
                   $"Position: {GetCameraPosition():F2}, Active: {_isActive}";
        }

        #endregion
        
        public void ReturnToPreviousState()
        {
            TransitionToState(previousStateType);
        }
        
        public void SetFollowTarget(Transform target)
        {
            SetTarget(target); // Use ICameraController SetTarget method
        }
        
        public CameraStateType GetCurrentStateType() => currentStateType;
        public bool IsInState(CameraStateType stateType) => currentStateType == stateType;

        #region Camera Mode Management

        /// <summary>
        /// Toggle between first person and third person camera modes
        /// </summary>
        private void ToggleCameraMode()
        {
            switch (currentStateType)
            {
                case CameraStateType.FirstPerson:
                    TransitionToState(CameraStateType.ThirdPerson);
                    break;
                case CameraStateType.ThirdPerson:
                    TransitionToState(CameraStateType.FirstPerson);
                    break;
                default:
                    // For other states, default to third person
                    TransitionToState(CameraStateType.ThirdPerson);
                    break;
            }
        }

        /// <summary>
        /// Reset camera to default behind-target position
        /// </summary>
        private void ResetCamera()
        {
            // Implementation depends on current state
            currentState?.OnResetRequested(this);
        }

        #endregion
    }
}
