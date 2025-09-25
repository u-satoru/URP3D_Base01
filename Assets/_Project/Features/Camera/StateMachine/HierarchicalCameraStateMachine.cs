using UnityEngine;
using System.Collections.Generic;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Camera;
using asterivo.Unity60.Core.Patterns.StateMachine;
using asterivo.Unity60.Core.StateMachine;
using asterivo.Unity60.Features.Camera.ViewMode;
using asterivo.Unity60.Features.Camera.States;
using asterivo.Unity60.Features.Camera.StateMachine.Hierarchical;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Features.Camera.StateMachine
{
    /// <summary>
    /// 階層化ステートマシンを使用するカメラコントローラー
    /// 従来のCameraStateMachineの階層化版
    /// </summary>
    public class HierarchicalCameraStateMachine : MonoBehaviour, ICameraController
    {
        [TabGroup("Camera Control", "State Management")]
        [ReadOnly]
        [LabelText("Current State")]
        [SerializeField] private string currentStateName;

        [TabGroup("Camera Control", "State Management")]
        [ReadOnly]
        [LabelText("Current Child State")]
        [SerializeField] private string currentChildStateName;

        [TabGroup("Camera Control", "Components")]
        [Required]
        [LabelText("Main Camera")]
        [SerializeField] private UnityEngine.Camera mainCamera;

        [TabGroup("Camera Control", "Components")]
        [LabelText("Camera Rig")]
        [SerializeField] private Transform cameraRig;

        [TabGroup("Camera Control", "Components")]
        [LabelText("Follow Target")]
        [SerializeField] private Transform followTarget;

        [TabGroup("Camera Control", "Settings")]
        [LabelText("First Person Settings")]
        [SerializeField] private FirstPersonSettings firstPersonSettings;

        [TabGroup("Camera Control", "Settings")]
        [LabelText("Third Person Settings")]
        [SerializeField] private ThirdPersonSettings thirdPersonSettings;

        [TabGroup("Camera Control", "Settings")]
        [LabelText("Cover Settings")]
        [SerializeField] private CoverViewSettings coverSettings;

        [TabGroup("Camera Control", "Settings")]
        [LabelText("Aim Settings")]
        [SerializeField] private AimSettings aimSettings;

        [TabGroup("Camera Control", "Events")]
        [LabelText("Camera State Changed")]
        [SerializeField] private GameEvent<System.Enum> onCameraStateChanged;

        // Hierarchical State Machine
        private CameraContext context;
        private HierarchicalState<CameraContext> currentState;
        private Dictionary<string, HierarchicalState<CameraContext>> states;

        // State instances
        private asterivo.Unity60.Features.Camera.StateMachine.Hierarchical.ThirdPersonCameraState thirdPersonState;
        private asterivo.Unity60.Features.Camera.StateMachine.Hierarchical.AimCameraState aimState;

        // ICameraController Properties
        public Transform CameraTransform => mainCamera?.transform ?? transform;
        public Transform Target
        {
            get => followTarget;
            set => SetTarget(value);
        }
        public System.Enum CurrentStateType => GetCurrentStateEnum();
        public bool IsActive
        {
            get => context?.IsActive ?? true;
            set => SetActive(value);
        }

        // ICameraController Events
        public event System.Action<System.Enum, System.Enum> OnStateChanged;
        public event System.Action<Transform> OnTargetChanged;
        public event System.Action<Transform> OnCameraTransformChanged;

        // Properties for external access
        public CameraContext Context => context;
        public UnityEngine.Camera MainCamera => mainCamera;

        private void Awake()
        {
            InitializeComponents();
            InitializeContext();
            InitializeStates();
        }

        private void InitializeComponents()
        {
            if (mainCamera == null)
                mainCamera = UnityEngine.Camera.main;

            if (cameraRig == null)
                cameraRig = transform;
        }

        private void InitializeContext()
        {
            context = new CameraContext(
                transform,
                mainCamera,
                cameraRig,
                followTarget,
                firstPersonSettings,
                thirdPersonSettings,
                coverSettings,
                aimSettings,
                onCameraStateChanged
            );
        }

        private void InitializeStates()
        {
            // 階層化ステートの作成
            thirdPersonState = new asterivo.Unity60.Features.Camera.StateMachine.Hierarchical.ThirdPersonCameraState();
            aimState = new asterivo.Unity60.Features.Camera.StateMachine.Hierarchical.AimCameraState();

            states = new Dictionary<string, HierarchicalState<CameraContext>>
            {
                { "ThirdPerson", thirdPersonState },
                { "Aim", aimState }
            };

            // 状態を初期化 - HierarchicalStateは自動で初期化されるため処理不要
            // foreach (var state in states.Values)
            // {
            //     state.Initialize(); // HierarchicalStateにInitialize()メソッドは存在しない
            // }
        }

        private void Start()
        {
            RegisterWithCameraService();
            TransitionToState("ThirdPerson");
        }

        private void RegisterWithCameraService()
        {
            // TODO: CameraServiceとの連携実装
            Debug.Log($"[HierarchicalCameraStateMachine] '{gameObject.name}' initialized");
        }

        private void Update()
        {
            UpdateContext();
            UpdateCurrentState();
            UpdateInspectorValues();
            CheckStateTransitions();
        }

        private void LateUpdate()
        {
            UpdateCurrentStateLateUpdate();
            OnCameraTransformChanged?.Invoke(CameraTransform);
        }

        private void UpdateContext()
        {
            context.UpdateStateTimer();
            // 他の必要なコンテキスト更新
        }

        private void UpdateCurrentState()
        {
            currentState?.Update(context);
        }

        private void UpdateCurrentStateLateUpdate()
        {
            // カメラの最終位置・回転更新
            // 階層化状態での詳細なカメラ制御は子状態で処理される
        }

        private void UpdateInspectorValues()
        {
            currentStateName = GetCurrentStateName() ?? "None";
            currentChildStateName = currentState?.GetCurrentChildStateKey() ?? "None";
        }

        private void CheckStateTransitions()
        {
            // 状態遷移ロジック
            if (currentState == thirdPersonState)
            {
                // ThirdPerson -> Aim: エイム入力
                if (context.AimPressed)
                {
                    TransitionToState("Aim");
                }
            }
            else if (currentState == aimState)
            {
                // Aim -> ThirdPerson: エイム解除
                if (context.AimReleased)
                {
                    TransitionToState("ThirdPerson");
                }
            }
        }

        public void TransitionToState(string stateName)
        {
            if (!states.ContainsKey(stateName))
            {
                Debug.LogWarning($"[HierarchicalCameraStateMachine] State '{stateName}' not found");
                return;
            }

            if (currentState != null && GetCurrentStateName() == stateName)
                return;

            var previousState = GetCurrentStateName();

            // 現在の状態を終了
            currentState?.Exit(context);

            // 新しい状態に遷移
            currentState = states[stateName];
            context.ResetStateTimer();
            currentState.Enter(context);

            // イベントを発火
            OnStateChanged?.Invoke(GetStateEnum(previousState), GetStateEnum(stateName));
            onCameraStateChanged?.Raise(GetStateEnum(stateName));

            Debug.Log($"[HierarchicalCameraStateMachine] Transitioned to state: {stateName}");
        }

        // ICameraController Implementation
        public bool TransitionToState(System.Enum stateType)
        {
            if (stateType is CameraStateType cameraStateType)
            {
                string stateName = cameraStateType.ToString();
                if (states.ContainsKey(stateName))
                {
                    TransitionToState(stateName);
                    return true;
                }
                else
                {
                    Debug.LogWarning($"[HierarchicalCameraStateMachine] State {stateName} not implemented");
                    return false;
                }
            }
            else
            {
                Debug.LogError($"Invalid state type {stateType}. Expected CameraStateType.");
                return false;
            }
        }

        public void HandleInput(CameraInputData inputData)
        {
            if (!context.IsActive) return;

            // コンテキストに入力を設定
            context.UpdateInput(
                inputData.lookInput,
                inputData.zoomInput,
                inputData.cameraModeTogglePressed,
                inputData.aimPressed,
                inputData.aimReleased,
                inputData.resetCameraPressed
            );

            // リセット処理
            if (inputData.resetCameraPressed)
            {
                OnResetRequested();
            }
        }

        public bool ExecuteCommand(Core.Commands.ICommand command)
        {
            if (!context.IsActive)
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

        public void SetTarget(Transform target)
        {
            Transform previousTarget = followTarget;
            followTarget = target;
            context.SetTarget(target);

            if (followTarget != previousTarget)
            {
                OnTargetChanged?.Invoke(followTarget);
                Debug.Log($"Camera target changed to: {(followTarget != null ? followTarget.name : "None")}");
            }
        }

        private void SetActive(bool active)
        {
            if (context != null)
            {
                context.IsActive = active;
            }

            if (mainCamera != null)
            {
                mainCamera.enabled = active;
            }

            Debug.Log($"Camera controller {(active ? "activated" : "deactivated")}");
        }

        // ICameraController Utility Methods
        public Vector3 GetCameraPosition()
        {
            return context?.GetCameraPosition() ?? transform.position;
        }

        public Quaternion GetCameraRotation()
        {
            return context?.GetCameraRotation() ?? transform.rotation;
        }

        public Vector3 GetCameraForward()
        {
            return context?.GetCameraForward() ?? transform.forward;
        }

        public bool CanSeePoint(Vector3 worldPoint)
        {
            return context?.CanSeePoint(worldPoint) ?? false;
        }

        public string GetDebugInfo()
        {
            return context?.GetDebugInfo() ?? "Context not initialized";
        }

        // Utility Methods
        public bool IsInState(string stateName)
        {
            return GetCurrentStateName() == stateName;
        }

        public bool IsInChildState(string childStateName)
        {
            return currentState?.GetCurrentChildStateKey() == childStateName;
        }

        private void OnResetRequested()
        {
            if (currentState == thirdPersonState)
            {
                thirdPersonState.OnResetRequested(context);
            }
            else if (currentState == aimState)
            {
                aimState.OnResetRequested(context);
            }
        }

                // Helper method to get current state name
        private string GetCurrentStateName()
        {
            if (currentState == null) return null;
            
            // Get state name from the dictionary by finding the key for the current state
            foreach (var kvp in states)
            {
                if (kvp.Value == currentState)
                    return kvp.Key;
            }
            
            return currentState.GetType().Name; // Fallback to type name
        }

        
// Enum conversion utilities
        private System.Enum GetCurrentStateEnum()
        {
            return GetStateEnum(GetCurrentStateName());
        }

        private System.Enum GetStateEnum(string stateName)
        {
            return stateName switch
            {
                "ThirdPerson" => CameraStateType.ThirdPerson,
                "Aim" => CameraStateType.Aim,
                "FirstPerson" => CameraStateType.FirstPerson,
                "Cover" => CameraStateType.Cover,
                _ => CameraStateType.ThirdPerson
            };
        }

        // デバッグ用メソッド
        [Button("Get State Info")]
        private void LogStateInfo()
        {
            if (currentState != null)
            {
                Debug.Log($"Current State: {GetCurrentStateName()}");
                Debug.Log($"Current Child: {currentState.GetCurrentChildStateKey()}");
                Debug.Log($"State History: {string.Join(", ", currentState.GetStateHistory())}");
                Debug.Log($"Context Info: {context.GetDebugInfo()}");
            }
        }

        [Button("Force Third Person")]
        private void ForceThirdPersonState()
        {
            if (Application.isPlaying)
            {
                TransitionToState("ThirdPerson");
            }
        }

        [Button("Force Aim State")]
        private void ForceAimState()
        {
            if (Application.isPlaying)
            {
                TransitionToState("Aim");
            }
        }
    }

    // Camera State Type Enum
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
}