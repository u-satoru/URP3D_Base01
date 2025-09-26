using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using asterivo.Unity60.Core.Events;
// using asterivo.Unity60.Core.Commands;
using asterivo.Unity60.Core;

namespace asterivo.Unity60.Core.Camera
{
    /// <summary>
    /// Core camera service implementation using pure ServiceLocator pattern.
    /// Provides centralized camera management with event-driven architecture integration.
    /// Utilizes ServiceLocator exclusively for dependency injection and service registration.
    /// </summary>
    /// <remarks>
    /// Design Philosophy:
    /// This implementation follows the project's core architectural principles:
    /// - ServiceLocator Pattern: Pure ServiceLocator-based service registration and access
    /// - Event-Driven Architecture: Uses GameEvent system for loose coupling
    /// - Command Pattern: Supports camera commands through ICommand interface
    /// - MonoBehaviour Lifecycle: Standard Unity lifecycle with ServiceLocator integration
    /// - Core/Features Layer Separation: Core abstractions bridge to Features implementations
    ///
    /// ServiceLocator Integration:
    /// - Automatic registration on Awake() for lifecycle-managed service access
    /// - Service resolution for dependent systems (Input, Audio, Commands, etc.)
    /// - Proper cleanup and unregistration on destruction
    /// - Thread-safe service registration/unregistration
    ///
    /// Performance Considerations:
    /// - Dictionary-based controller lookup for O(1) access
    /// - Event batching to minimize frame impact
    /// - Efficient initialization with ServiceLocator dependency resolution
    /// - Memory-efficient state management without static overhead
    /// </remarks>
    public class CameraService : MonoBehaviour, ICameraService
    {
        #region ServiceLocator Integration

        [Header("ServiceLocator Configuration")]
        [SerializeField] private bool autoRegisterOnAwake = true;
        [SerializeField] private bool persistAcrossScenes = true;

        #endregion

        #region Serialized Fields

        [Header("Camera Service Configuration")]
        [SerializeField] private bool autoInitialize = true;
        [SerializeField] private string defaultControllerId = "MainCamera";
        [SerializeField] private float transitionDuration = 1.0f;

        [Header("Event Channels")]
        [SerializeField] private GameEvent<ICameraController, ICameraController> onControllerChangedEvent;
        [SerializeField] private GameEvent<System.Enum, System.Enum> onStateChangedEvent;
        [SerializeField] private GameEvent<bool> onServiceActiveChangedEvent;
        [SerializeField] private GameEvent<CameraInputData> onCameraInputEvent;

        [Header("Debug")]
        [SerializeField] private bool enableDebugLogging = false;

        #endregion

        #region Private Fields

        private readonly Dictionary<string, ICameraController> _controllers = new Dictionary<string, ICameraController>();
        private ICameraController _currentController;
        private bool _isActive = true;
        private bool _isInitialized = false;
        private Transform _mainCameraTransform;
        private CameraInputData _lastInputData;

        // ServiceLocator dependencies
        private ICommandPoolService _commandPool;

        #endregion

        #region ICameraService Properties

        /// <summary>
        /// Current active camera controller
        /// </summary>
        public ICameraController CurrentController => _currentController;

        /// <summary>
        /// Current camera state type (for external querying)
        /// </summary>
        public System.Enum CurrentState => _currentController?.CurrentStateType;

        /// <summary>
        /// Whether the camera service is currently active and processing
        /// </summary>
        public bool IsActive
        {
            get => _isActive;
            set => SetServiceActive(value);
        }

        /// <summary>
        /// Main camera transform (for external systems requiring camera position/rotation)
        /// </summary>
        public Transform MainCameraTransform
        {
            get
            {
                if (_mainCameraTransform == null)
                {
                    var mainCamera = UnityEngine.Camera.main;
                    if (mainCamera != null)
                        _mainCameraTransform = mainCamera.transform;
                }
                return _mainCameraTransform;
            }
        }

        #endregion

        #region ICameraService Events

        /// <summary>
        /// Event fired when camera controller changes
        /// </summary>
        public event Action<ICameraController, ICameraController> OnControllerChanged;

        /// <summary>
        /// Event fired when camera state transitions occur
        /// </summary>
        public event Action<System.Enum, System.Enum> OnStateChanged;

        /// <summary>
        /// Event fired when camera service activation state changes
        /// </summary>
        public event Action<bool> OnServiceActiveChanged;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // Configure scene persistence based on settings
            if (persistAcrossScenes)
            {
                DontDestroyOnLoad(gameObject);
            }

            // Register with ServiceLocator if auto-registration is enabled
            if (autoRegisterOnAwake)
            {
                try
                {
                    ServiceLocator.RegisterService<ICameraService>(this);
                    ServiceLocator.RegisterService<CameraService>(this);
                    DebugLog("CameraService registered with ServiceLocator successfully.");
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[CameraService] Failed to register with ServiceLocator: {ex.Message}");
                }
            }

            InitializeServiceDependencies();
        }

        private void Start()
        {
            if (autoInitialize && !_isInitialized)
            {
                Initialize();
            }
        }

        private void Update()
        {
            if (!_isActive || !_isInitialized) return;

            // Process any pending camera updates
            UpdateCameraSystem();
        }

        private void OnDestroy()
        {
            // Shutdown service if it was initialized
            if (_isInitialized)
            {
                Shutdown();
            }

            // Unregister from ServiceLocator if it was auto-registered
            if (autoRegisterOnAwake)
            {
                try
                {
                    ServiceLocator.UnregisterService<ICameraService>();
                    ServiceLocator.UnregisterService<CameraService>();
                    DebugLog("CameraService unregistered from ServiceLocator successfully.");
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[CameraService] Failed to unregister from ServiceLocator: {ex.Message}");
                }
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                // Pause camera processing
                SetServiceActive(false);
            }
            else
            {
                // Resume camera processing
                SetServiceActive(true);
            }
        }

        #endregion

        #region ServiceLocator Dependencies

        /// <summary>
        /// Initialize dependencies through ServiceLocator
        /// </summary>
        private void InitializeServiceDependencies()
        {
            // Get command pool service for camera commands
            _commandPool = ServiceLocator.GetService<ICommandPoolService>();

            if (_commandPool == null)
            {
                DebugLog("CommandPoolService not found in ServiceLocator. Camera commands will not be pooled.", LogType.Warning);
            }
            else
            {
                DebugLog("Successfully resolved CommandPoolService dependency.");
            }

            // Future: Additional service dependencies can be resolved here
            // Example: IAudioService, IInputService, IEventChannelRegistry, etc.
        }

        #endregion

        #region ICameraService Implementation

        /// <summary>
        /// Register a camera controller with the service
        /// </summary>
        /// <param name="controllerId">Unique identifier for the controller</param>
        /// <param name="controller">Camera controller to register</param>
        /// <returns>True if registration was successful</returns>
        public bool RegisterController(string controllerId, ICameraController controller)
        {
            if (string.IsNullOrEmpty(controllerId))
            {
                Debug.LogError("[CameraService] Cannot register controller with null or empty ID.");
                return false;
            }

            if (controller == null)
            {
                Debug.LogError($"[CameraService] Cannot register null controller for ID: {controllerId}");
                return false;
            }

            if (_controllers.ContainsKey(controllerId))
            {
                DebugLog($"Controller ID '{controllerId}' already exists. Replacing with new controller.", LogType.Warning);
            }

            _controllers[controllerId] = controller;

            // Subscribe to controller events
            controller.OnStateChanged += HandleControllerStateChanged;
            controller.OnTargetChanged += HandleControllerTargetChanged;
            controller.OnCameraTransformChanged += HandleControllerTransformChanged;

            DebugLog($"Successfully registered controller: {controllerId}");

            // Set as current if no current controller or if this is the default
            if (_currentController == null || controllerId == defaultControllerId)
            {
                SwitchToController(controllerId, true);
            }

            return true;
        }

        /// <summary>
        /// Unregister a camera controller from the service
        /// </summary>
        /// <param name="controllerId">Unique identifier of the controller to remove</param>
        /// <returns>True if unregistration was successful</returns>
        public bool UnregisterController(string controllerId)
        {
            if (!_controllers.TryGetValue(controllerId, out ICameraController controller))
            {
                DebugLog($"Controller ID '{controllerId}' not found for unregistration.", LogType.Warning);
                return false;
            }

            // Unsubscribe from controller events
            controller.OnStateChanged -= HandleControllerStateChanged;
            controller.OnTargetChanged -= HandleControllerTargetChanged;
            controller.OnCameraTransformChanged -= HandleControllerTransformChanged;

            _controllers.Remove(controllerId);

            // Switch to another controller if this was the current one
            if (_currentController == controller)
            {
                _currentController = null;

                // Try to switch to default or any available controller
                if (_controllers.Count > 0)
                {
                    var nextControllerId = _controllers.ContainsKey(defaultControllerId)
                        ? defaultControllerId
                        : _controllers.Keys.FirstOrDefault();

                    if (nextControllerId != null)
                    {
                        SwitchToController(nextControllerId, true);
                    }
                }
            }

            DebugLog($"Successfully unregistered controller: {controllerId}");
            return true;
        }

        /// <summary>
        /// Switch to a specific camera controller
        /// </summary>
        /// <param name="controllerId">Unique identifier of the target controller</param>
        /// <param name="immediateSwitch">Whether to switch immediately or use transition</param>
        /// <returns>True if switch was successful</returns>
        public bool SwitchToController(string controllerId, bool immediateSwitch = false)
        {
            if (!_controllers.TryGetValue(controllerId, out ICameraController targetController))
            {
                Debug.LogError($"[CameraService] Controller '{controllerId}' not found for switching.");
                return false;
            }

            var previousController = _currentController;

            // Deactivate previous controller
            if (previousController != null && previousController != targetController)
            {
                previousController.IsActive = false;
            }

            // Activate new controller
            _currentController = targetController;
            _currentController.IsActive = true;

            // Fire events
            OnControllerChanged?.Invoke(previousController, _currentController);
            onControllerChangedEvent?.Raise(previousController, _currentController);

            DebugLog($"Switched to controller: {controllerId} (Immediate: {immediateSwitch})");

            return true;
        }

        /// <summary>
        /// Get registered controller by ID
        /// </summary>
        /// <param name="controllerId">Unique identifier of the controller</param>
        /// <returns>Camera controller if found, null otherwise</returns>
        public ICameraController GetController(string controllerId)
        {
            _controllers.TryGetValue(controllerId, out ICameraController controller);
            return controller;
        }

        /// <summary>
        /// Transition current camera to a specific state
        /// </summary>
        /// <param name="stateType">Target camera state</param>
        /// <param name="forceTransition">Whether to force transition even if already in that state</param>
        /// <returns>True if transition was successful</returns>
        public bool TransitionToState(System.Enum stateType, bool forceTransition = false)
        {
            if (_currentController == null)
            {
                DebugLog("No active camera controller for state transition.", LogType.Warning);
                return false;
            }

            if (!forceTransition && _currentController.CurrentStateType?.Equals(stateType) == true)
            {
                DebugLog($"Camera already in state: {stateType}");
                return true;
            }

            bool success = _currentController.TransitionToState(stateType);

            if (success)
            {
                DebugLog($"Camera transitioned to state: {stateType}");
            }
            else
            {
                DebugLog($"Failed to transition camera to state: {stateType}", LogType.Warning);
            }

            return success;
        }

        /// <summary>
        /// Check if the current camera can transition to a specific state
        /// </summary>
        /// <param name="stateType">Target state to check</param>
        /// <returns>True if transition is possible</returns>
        public bool CanTransitionTo(System.Enum stateType)
        {
            if (_currentController == null) return false;

            // For now, we assume all transitions are possible
            // This can be extended with specific transition rules
            return true;
        }

        /// <summary>
        /// Get available states for the current camera controller
        /// </summary>
        /// <returns>Array of available state types</returns>
        public System.Enum[] GetAvailableStates()
        {
            // This would need to be implemented based on specific controller type
            // For now, return empty array
            return new System.Enum[0];
        }

        /// <summary>
        /// Process camera input data
        /// </summary>
        /// <param name="inputData">Camera input data to process</param>
        public void HandleInput(CameraInputData inputData)
        {
            if (!_isActive || _currentController == null) return;

            _lastInputData = inputData;
            _currentController.HandleInput(inputData);

            // Fire input event
            onCameraInputEvent?.Raise(inputData);
        }

        /// <summary>
        /// Set camera input enabled state
        /// </summary>
        /// <param name="enabled">Whether camera input should be processed</param>
        public void SetInputEnabled(bool enabled)
        {
            // Implementation depends on input system integration
            DebugLog($"Camera input enabled: {enabled}");
        }

        /// <summary>
        /// Check if camera input is currently enabled
        /// </summary>
        /// <returns>True if input is enabled</returns>
        public bool IsInputEnabled()
        {
            // Implementation depends on input system integration
            return _isActive;
        }

        /// <summary>
        /// Set the primary target for camera following
        /// </summary>
        /// <param name="target">Target transform to follow</param>
        public void SetTarget(Transform target)
        {
            if (_currentController != null)
            {
                _currentController.Target = target;
                DebugLog($"Set camera target: {(target != null ? target.name : "None")}");
            }
        }

        /// <summary>
        /// Get current camera target
        /// </summary>
        /// <returns>Current target transform</returns>
        public Transform GetTarget()
        {
            return _currentController?.Target;
        }

        /// <summary>
        /// Clear current camera target
        /// </summary>
        public void ClearTarget()
        {
            SetTarget(null);
        }

        /// <summary>
        /// Initialize the camera service
        /// </summary>
        /// <returns>True if initialization was successful</returns>
        public bool Initialize()
        {
            if (_isInitialized)
            {
                DebugLog("CameraService already initialized.");
                return true;
            }

            try
            {
                // Initialize main camera reference
                var mainCamera = UnityEngine.Camera.main;
                if (mainCamera != null)
                {
                    _mainCameraTransform = mainCamera.transform;
                }

                // Additional initialization logic here
                _isInitialized = true;
                DebugLog("CameraService initialized successfully.");

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[CameraService] Initialization failed: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Cleanup and shutdown the camera service
        /// </summary>
        public void Shutdown()
        {
            if (!_isInitialized) return;

            try
            {
                // Cleanup all controllers
                foreach (var kvp in _controllers)
                {
                    var controller = kvp.Value;
                    controller.OnStateChanged -= HandleControllerStateChanged;
                    controller.OnTargetChanged -= HandleControllerTargetChanged;
                    controller.OnCameraTransformChanged -= HandleControllerTransformChanged;
                }

                _controllers.Clear();
                _currentController = null;
                _isInitialized = false;

                DebugLog("CameraService shutdown completed.");
            }
            catch (Exception e)
            {
                Debug.LogError($"[CameraService] Shutdown failed: {e.Message}");
            }
        }

        /// <summary>
        /// Reset camera service to default state
        /// </summary>
        public void Reset()
        {
            Shutdown();
            Initialize();

            DebugLog("CameraService reset to default state.");
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Get current camera position
        /// </summary>
        /// <returns>Camera world position</returns>
        public Vector3 GetCameraPosition()
        {
            if (_currentController != null)
                return _currentController.GetCameraPosition();

            return MainCameraTransform != null ? MainCameraTransform.position : Vector3.zero;
        }

        /// <summary>
        /// Get current camera rotation
        /// </summary>
        /// <returns>Camera world rotation</returns>
        public Quaternion GetCameraRotation()
        {
            if (_currentController != null)
                return _currentController.GetCameraRotation();

            return MainCameraTransform != null ? MainCameraTransform.rotation : Quaternion.identity;
        }

        /// <summary>
        /// Get camera forward direction
        /// </summary>
        /// <returns>Camera forward vector</returns>
        public Vector3 GetCameraForward()
        {
            if (_currentController != null)
                return _currentController.GetCameraForward();

            return MainCameraTransform != null ? MainCameraTransform.forward : Vector3.forward;
        }

        /// <summary>
        /// Check if camera can see a specific point
        /// </summary>
        /// <param name="worldPoint">World point to check</param>
        /// <returns>True if point is within camera view</returns>
        public bool CanSeePoint(Vector3 worldPoint)
        {
            if (_currentController != null)
                return _currentController.CanSeePoint(worldPoint);

            var mainCamera = UnityEngine.Camera.main;
            if (mainCamera == null) return false;

            Vector3 viewportPoint = mainCamera.WorldToViewportPoint(worldPoint);
            return viewportPoint.x >= 0 && viewportPoint.x <= 1 &&
                   viewportPoint.y >= 0 && viewportPoint.y <= 1 &&
                   viewportPoint.z > 0;
        }

        /// <summary>
        /// Get comprehensive debug information about camera service state
        /// </summary>
        /// <returns>Debug info string</returns>
        public string GetDebugInfo()
        {
            var controllerInfo = _currentController != null ? _currentController.GetDebugInfo() : "No active controller";
            var controllerCount = _controllers.Count;
            var isActiveInfo = _isActive ? "Active" : "Inactive";

            return $"CameraService - {isActiveInfo} | Controllers: {controllerCount} | Current: {controllerInfo}";
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Set service active state with event firing
        /// </summary>
        /// <param name="active">Active state</param>
        private void SetServiceActive(bool active)
        {
            if (_isActive != active)
            {
                _isActive = active;

                // Update current controller active state
                if (_currentController != null)
                {
                    _currentController.IsActive = active;
                }

                // Fire events
                OnServiceActiveChanged?.Invoke(_isActive);
                onServiceActiveChangedEvent?.Raise(_isActive);

                DebugLog($"CameraService active state changed: {_isActive}");
            }
        }

        /// <summary>
        /// Update camera system (called each frame)
        /// </summary>
        private void UpdateCameraSystem()
        {
            // Update main camera transform reference
            if (_mainCameraTransform == null)
            {
                var mainCamera = UnityEngine.Camera.main;
                if (mainCamera != null)
                    _mainCameraTransform = mainCamera.transform;
            }

            // Additional per-frame camera system updates here
        }

        /// <summary>
        /// Handle controller state change events
        /// </summary>
        private void HandleControllerStateChanged(System.Enum fromState, System.Enum toState)
        {
            OnStateChanged?.Invoke(fromState, toState);
            onStateChangedEvent?.Raise(fromState, toState);

            DebugLog($"Controller state changed: {fromState} -> {toState}");
        }

        /// <summary>
        /// Handle controller target change events
        /// </summary>
        private void HandleControllerTargetChanged(Transform target)
        {
            DebugLog($"Controller target changed: {(target != null ? target.name : "None")}");
        }

        /// <summary>
        /// Handle controller transform change events
        /// </summary>
        private void HandleControllerTransformChanged(Transform cameraTransform)
        {
            // Update main camera transform reference if needed
            if (_mainCameraTransform != cameraTransform && cameraTransform.CompareTag("MainCamera"))
            {
                _mainCameraTransform = cameraTransform;
            }
        }

        /// <summary>
        /// Debug logging with conditional compilation
        /// </summary>
        private void DebugLog(string message, LogType logType = LogType.Log)
        {
            if (!enableDebugLogging) return;

            switch (logType)
            {
                case LogType.Warning:
                    Debug.LogWarning($"[CameraService] {message}");
                    break;
                case LogType.Error:
                    Debug.LogError($"[CameraService] {message}");
                    break;
                default:
                    Debug.Log($"[CameraService] {message}");
                    break;
            }
        }

        #endregion

        #region Editor Support

#if UNITY_EDITOR
        [ContextMenu("Debug: Print Service Info")]
        private void EditorDebugPrintInfo()
        {
            Debug.Log(GetDebugInfo());
        }

        [ContextMenu("Debug: List Controllers")]
        private void EditorDebugListControllers()
        {
            Debug.Log($"Registered Controllers ({_controllers.Count}):");
            foreach (var kvp in _controllers)
            {
                var isActive = kvp.Value == _currentController ? " (ACTIVE)" : "";
                Debug.Log($"  - {kvp.Key}: {kvp.Value.GetType().Name}{isActive}");
            }
        }
#endif

        #endregion
    }
}

