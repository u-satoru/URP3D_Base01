using UnityEngine;
using asterivo.Unity60.Core.Commands;
using asterivo.Unity60.Core.Patterns.StateMachine;
using System.Collections.Generic;

namespace asterivo.Unity60.Core.Camera
{
    /// <summary>
    /// Generic camera controller implementation using State Machine pattern.
    /// Integrates with Event-Driven Architecture and Command Pattern.
    /// Provides foundation for all camera control systems.
    /// </summary>
    [RequireComponent(typeof(UnityEngine.Camera))]
    public abstract class CameraController<TStateType> : MonoBehaviour, ICameraController
        where TStateType : System.Enum
    {
        #region Serialized Fields

        [Header("Camera Controller Settings")]
        [SerializeField] protected Transform target;
        [SerializeField] protected TStateType initialState;
        [SerializeField] protected bool autoStart = true;

        [Header("Debug")]
        [SerializeField] protected bool enableDebugLog = false;
        [SerializeField] protected bool enableDebugDrawing = false;

        #endregion

        #region Protected Fields

        protected StateMachine<TStateType, CameraController<TStateType>> stateMachine;
        protected UnityEngine.Camera cameraComponent;
        protected Dictionary<TStateType, ICameraState<CameraController<TStateType>>> states;

        protected bool isActive = true;
        protected CameraInputData lastInputData;

        #endregion

        #region ICameraController Properties

        public Transform CameraTransform => transform;
        public Transform Target
        {
            get => target;
            set => SetTarget(value);
        }
        public System.Enum CurrentStateType => stateMachine != null ? (System.Enum)stateMachine.CurrentStateType : default(TStateType);
        public bool IsActive
        {
            get => isActive;
            set => SetActive(value);
        }

        #endregion

        #region ICameraController Events

        public event System.Action<System.Enum, System.Enum> OnStateChanged;
        public event System.Action<Transform> OnTargetChanged;
        public event System.Action<Transform> OnCameraTransformChanged;

        #endregion

        #region Unity Lifecycle

        protected virtual void Awake()
        {
            InitializeComponents();
            InitializeStates();
            InitializeStateMachine();
        }

        protected virtual void Start()
        {
            if (autoStart)
            {
                StartCameraController();
            }
        }

        protected virtual void Update()
        {
            if (!isActive) return;

            stateMachine?.Update();
            CheckForStateChanges();
        }

        protected virtual void LateUpdate()
        {
            if (!isActive) return;

            // Note: StateMachine doesn't have LateUpdate - call directly on current state if needed
            // For now, we'll handle LateUpdate logic here or in derived classes
            FireCameraTransformChanged();
        }

        protected virtual void FixedUpdate()
        {
            if (!isActive) return;

            stateMachine?.FixedUpdate();
        }

        #endregion

        #region Abstract Methods

        /// <summary>
        /// Initialize camera states dictionary.
        /// Override in derived classes to define specific camera states.
        /// </summary>
        protected abstract void InitializeStates();

        /// <summary>
        /// Get the default initial state for this camera controller.
        /// Override in derived classes to define default behavior.
        /// </summary>
        /// <returns>Default initial state</returns>
        protected abstract TStateType GetDefaultInitialState();

        #endregion

        #region Initialization

        protected virtual void InitializeComponents()
        {
            cameraComponent = GetComponent<UnityEngine.Camera>();
            if (cameraComponent == null)
            {
                LogWarning("Camera component not found! Adding default camera component.");
                cameraComponent = gameObject.AddComponent<UnityEngine.Camera>();
            }

            states = new Dictionary<TStateType, ICameraState<CameraController<TStateType>>>();
        }

        protected virtual void InitializeStateMachine()
        {
            if (states.Count == 0)
            {
                LogError("No camera states initialized! Cannot create state machine.");
                return;
            }

            stateMachine = new StateMachine<TStateType, CameraController<TStateType>>(this);
            stateMachine.OnStateChanged += HandleStateChanged;

            // Set initial state
            TStateType startState = initialState.Equals(default(TStateType)) ? GetDefaultInitialState() : initialState;
            if (!stateMachine.TransitionTo(startState))
            {
                LogError($"Failed to transition to initial state: {startState}");
            }
        }

        protected virtual void StartCameraController()
        {
            if (target == null)
            {
                LogWarning("No target set for camera controller.");
            }

            LogDebug($"Camera controller started with state: {CurrentStateType}");
        }

        #endregion

        #region ICameraController Implementation

        public virtual void SetTarget(Transform newTarget)
        {
            Transform previousTarget = target;
            target = newTarget;

            if (target != previousTarget)
            {
                OnTargetChanged?.Invoke(target);
                LogDebug($"Camera target changed to: {(target != null ? target.name : "None")}");
            }
        }

        public virtual bool TransitionToState(System.Enum stateType)
        {
            if (!(stateType is TStateType typedState))
            {
                LogError($"Invalid state type: {stateType}. Expected {typeof(TStateType).Name}");
                return false;
            }

            return stateMachine?.TransitionTo(typedState) ?? false;
        }

        public virtual void HandleInput(CameraInputData inputData)
        {
            if (!isActive) return;

            lastInputData = inputData;
            stateMachine?.HandleInput(inputData);
        }

        public virtual bool ExecuteCommand(ICommand command)
        {
            if (!isActive)
            {
                LogWarning("Cannot execute command: Camera controller is not active");
                return false;
            }

            if (command == null)
            {
                LogError("Cannot execute null command");
                return false;
            }

            try
            {
                command.Execute();
                LogDebug($"Executed camera command: {command.GetType().Name}");
                return true;
            }
            catch (System.Exception e)
            {
                LogError($"Error executing camera command {command.GetType().Name}: {e.Message}");
                return false;
            }
        }

        #endregion

        #region Utility Methods

        public Vector3 GetCameraPosition() => cameraComponent.transform.position;
        public Quaternion GetCameraRotation() => cameraComponent.transform.rotation;
        public Vector3 GetCameraForward() => cameraComponent.transform.forward;

        public virtual bool CanSeePoint(Vector3 worldPoint)
        {
            if (cameraComponent == null) return false;

            Vector3 viewportPoint = cameraComponent.WorldToViewportPoint(worldPoint);
            return viewportPoint.x >= 0 && viewportPoint.x <= 1 &&
                   viewportPoint.y >= 0 && viewportPoint.y <= 1 &&
                   viewportPoint.z > 0;
        }

        public virtual string GetDebugInfo()
        {
            return $"State: {CurrentStateType}, Target: {(target != null ? target.name : "None")}, " +
                   $"Position: {GetCameraPosition():F2}, Active: {isActive}";
        }

        #endregion

        #region Protected Helper Methods

        protected virtual void SetActive(bool active)
        {
            isActive = active;
            cameraComponent.enabled = active;

            LogDebug($"Camera controller {(active ? "activated" : "deactivated")}");
        }

        protected virtual void CheckForStateChanges()
        {
            // Override in derived classes for custom state change logic
        }

        protected virtual void HandleStateChanged(TStateType fromState, TStateType toState)
        {
            OnStateChanged?.Invoke(fromState, toState);
            LogDebug($"Camera state changed: {fromState} -> {toState}");
        }

        protected virtual void FireCameraTransformChanged()
        {
            OnCameraTransformChanged?.Invoke(transform);
        }

        #endregion

        #region State Management Helpers

        /// <summary>
        /// Add a state to the state machine
        /// </summary>
        /// <param name="stateType">State type</param>
        /// <param name="state">State implementation</param>
        protected void AddState(TStateType stateType, ICameraState<CameraController<TStateType>> state)
        {
            if (states.ContainsKey(stateType))
            {
                LogWarning($"State {stateType} already exists. Replacing with new implementation.");
            }

            states[stateType] = state;
        }

        /// <summary>
        /// Get current state instance
        /// </summary>
        /// <returns>Current state or null</returns>
        protected ICameraState<CameraController<TStateType>> GetCurrentState()
        {
            // Note: StateMachine doesn't expose current state instance directly
            // This method would need to be refactored or removed if direct state access is needed
            return null;
        }

        /// <summary>
        /// Check if a specific state is currently active
        /// </summary>
        /// <param name="stateType">State to check</param>
        /// <returns>True if state is active</returns>
        protected bool IsInState(TStateType stateType)
        {
            return stateMachine != null && stateMachine.CurrentStateType.Equals(stateType);
        }

        #endregion

        #region Debug Methods

        protected void LogDebug(string message)
        {
            if (enableDebugLog)
            {
                Debug.Log($"[CameraController] {message}", this);
            }
        }

        protected void LogWarning(string message)
        {
            Debug.LogWarning($"[CameraController] {message}", this);
        }

        protected void LogError(string message)
        {
            Debug.LogError($"[CameraController] {message}", this);
        }

        #endregion

        #region Debug Drawing

        protected virtual void OnDrawGizmosSelected()
        {
            if (!enableDebugDrawing) return;

            // Draw camera frustum
            if (cameraComponent != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
                Gizmos.DrawFrustum(Vector3.zero, cameraComponent.fieldOfView, cameraComponent.farClipPlane,
                                  cameraComponent.nearClipPlane, cameraComponent.aspect);
                Gizmos.matrix = Matrix4x4.identity;
            }

            // Draw target connection
            if (target != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, target.position);
                Gizmos.DrawWireSphere(target.position, 0.5f);
            }

            // Draw current state info
            if (Application.isPlaying && CurrentStateType != null)
            {
                Gizmos.color = Color.cyan;
                Vector3 labelPos = transform.position + Vector3.up * 2f;

                #if UNITY_EDITOR
                UnityEditor.Handles.Label(labelPos, $"State: {CurrentStateType}");
                #endif
            }
        }

        #endregion
    }
}

