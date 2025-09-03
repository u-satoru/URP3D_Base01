using UnityEngine;
using System.Collections.Generic;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Data;

namespace asterivo.Unity60.Player.States
{
    public enum PlayerStateType
    {
        Idle,
        Walking,
        Running,
        Jumping,
        Crouching,
        Prone,
        InCover,
        Climbing,
        Swimming,
        Dead
    }

    /// <summary>
    /// 詳細なプレイヤー状態管理システム
    /// ステートパターンを使用した低レベル状態制御
    /// </summary>
    public class DetailedPlayerStateMachine : MonoBehaviour
    {
        [Header("State Management")]
        [SerializeField] private PlayerStateType currentStateType;
        [SerializeField] private PlayerStateType previousStateType;
        
        [Header("Components")]
        [SerializeField] private CharacterController characterController;
        [SerializeField] private StealthMovementController stealthMovement;
        [SerializeField] private CoverSystem coverSystem;
        
        [Header("Events")]
        [SerializeField] private GenericGameEvent<PlayerStateType> onStateChanged;
        [SerializeField] private MovementStanceEvent onStanceChanged;
        
        private IPlayerState currentState;
        private Dictionary<PlayerStateType, IPlayerState> states;
        
        public CharacterController CharacterController => characterController;
        public StealthMovementController StealthMovement => stealthMovement;
        public CoverSystem CoverSystem => coverSystem;
        
        private void Awake()
        {
            InitializeComponents();
            InitializeStates();
        }
        
        private void InitializeComponents()
        {
            if (characterController == null)
                characterController = GetComponent<CharacterController>();
                
            if (stealthMovement == null)
                stealthMovement = GetComponent<StealthMovementController>();
                
            if (coverSystem == null)
                coverSystem = GetComponent<CoverSystem>();
        }
        
        private void InitializeStates()
        {
            states = new Dictionary<PlayerStateType, IPlayerState>
            {
                { PlayerStateType.Idle, new IdleState() },
                { PlayerStateType.Walking, new WalkingState() },
                { PlayerStateType.Running, new RunningState() },
                { PlayerStateType.Jumping, new JumpingState() },
                { PlayerStateType.Crouching, new CrouchingState() },
                { PlayerStateType.Prone, new ProneState() },
                { PlayerStateType.InCover, new CoverState() }
            };
        }
        
        private void Start()
        {
            TransitionToState(PlayerStateType.Idle);
        }
        
        private void Update()
        {
            currentState?.Update(this);
        }

        public void HandleInput(Vector2 moveInput, bool jumpInput)
        {
            currentState?.HandleInput(this, moveInput, jumpInput);
        }
        
        private void FixedUpdate()
        {
            currentState?.FixedUpdate(this);
        }
        
        public void TransitionToState(PlayerStateType newStateType)
        {
            if (currentStateType == newStateType && currentState != null)
                return;
                
            if (!states.ContainsKey(newStateType))
            {
                Debug.LogWarning($"State {newStateType} not found in state machine");
                return;
            }
            
            previousStateType = currentStateType;
            currentState?.Exit(this);
            
            currentStateType = newStateType;
            currentState = states[newStateType];
            currentState.Enter(this);
            
            onStateChanged?.Raise(currentStateType);
            
            UpdateMovementStance(newStateType);
        }
        
        private void UpdateMovementStance(PlayerStateType stateType)
        {
            MovementStance stance = stateType switch
            {
                PlayerStateType.Crouching => MovementStance.Crouching,
                PlayerStateType.Prone => MovementStance.Prone,
                PlayerStateType.InCover => MovementStance.Cover,
                _ => MovementStance.Standing
            };
            
            onStanceChanged?.Raise(stance);
        }
        
        public void ReturnToPreviousState()
        {
            TransitionToState(previousStateType);
        }
        
        public PlayerStateType GetCurrentStateType() => currentStateType;
        public PlayerStateType GetPreviousStateType() => previousStateType;
        
        public bool IsInState(PlayerStateType stateType)
        {
            return currentStateType == stateType;
        }
        
        public bool CanTransitionTo(PlayerStateType targetState)
        {
            return targetState switch
            {
                PlayerStateType.Dead => true,
                PlayerStateType.InCover => coverSystem != null && coverSystem.GetAvailableCovers().Count > 0,
                PlayerStateType.Prone => currentStateType != PlayerStateType.InCover,
                _ => true
            };
        }
    }
}