using UnityEngine;
using Debug = UnityEngine.Debug;
using System.Collections.Generic;
using asterivo.Unity60.Core.Events;

namespace asterivo.Unity60.Camera.States
{
    public class CameraStateMachine : MonoBehaviour
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
        
        public UnityEngine.Camera MainCamera => mainCamera;
        public Transform CameraRig => cameraRig;
        public Transform FollowTarget => followTarget;
        public FirstPersonSettings FirstPersonSettings => firstPersonSettings;
        public ThirdPersonSettings ThirdPersonSettings => thirdPersonSettings;
        public CoverViewSettings CoverSettings => coverSettings;
        public AimSettings AimSettings => aimSettings;
        
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
            TransitionToState(CameraStateType.ThirdPerson);
        }
        
        private void Update()
        {
            currentState?.HandleInput(this);
            currentState?.Update(this);
        }
        
        private void LateUpdate()
        {
            currentState?.LateUpdate(this);
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
            
            previousStateType = currentStateType;
            currentState?.Exit(this);
            
            currentStateType = newStateType;
            currentState = states[newStateType];
            currentState.Enter(this);
            
            onCameraStateChanged?.Raise(currentStateType);
        }
        
        public void ReturnToPreviousState()
        {
            TransitionToState(previousStateType);
        }
        
        public void SetFollowTarget(Transform target)
        {
            followTarget = target;
        }
        
        public CameraStateType GetCurrentStateType() => currentStateType;
        public bool IsInState(CameraStateType stateType) => currentStateType == stateType;
    }
}