using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using asterivo.Unity60.Features.Templates.TPS.Player;
using asterivo.Unity60.Features.Templates.TPS.Player.StateMachine.States;

namespace asterivo.Unity60.Features.Templates.TPS.Player.StateMachine
{
    /// <summary>
    /// TPS Player State Machine - High-performance Dictionary-based state management
    /// Implements event-driven state transitions with ServiceLocator integration
    /// </summary>
    public class TPSPlayerStateMachine
    {
        // Dictionary for O(1) state lookup performance
        private readonly Dictionary<PlayerState, IPlayerState> _states = new Dictionary<PlayerState, IPlayerState>();

        private IPlayerState _currentState;
        private PlayerState _currentStateType;
        private TPSPlayerController _controller;

        // Events for state change notifications
        public UnityEvent<PlayerState> OnStateChanged = new UnityEvent<PlayerState>();
        public UnityEvent<PlayerState, PlayerState> OnStateTransition = new UnityEvent<PlayerState, PlayerState>();

        public PlayerState CurrentStateType => _currentStateType;
        public IPlayerState CurrentState => _currentState;

        /// <summary>
        /// Initialize the state machine with player controller reference
        /// </summary>
        public void Initialize(TPSPlayerController controller)
        {
            _controller = controller;
            InitializeStates();

            // Start with Idle state
            ChangeState(PlayerState.Idle);
        }

        /// <summary>
        /// Initialize all state instances
        /// </summary>
        private void InitializeStates()
        {
            // Locomotion states
            RegisterState(PlayerState.Idle, new IdleState());
            RegisterState(PlayerState.Walking, new WalkingState());
            RegisterState(PlayerState.Running, new RunningState());
            RegisterState(PlayerState.Crouching, new CrouchingState());

            // Aerial states
            RegisterState(PlayerState.Jumping, new JumpingState());
            RegisterState(PlayerState.Falling, new FallingState());
            RegisterState(PlayerState.Landing, new LandingState());

            // Action states
            RegisterState(PlayerState.Rolling, new RollingState());
            RegisterState(PlayerState.InCover, new CoverState());
            RegisterState(PlayerState.Aiming, new AimingState());

            // Combat states
            RegisterState(PlayerState.Shooting, new ShootingState());
            RegisterState(PlayerState.Reloading, new ReloadingState());
            RegisterState(PlayerState.MeleeAttack, new MeleeAttackState());

            // Reaction states
            RegisterState(PlayerState.TakingDamage, new TakingDamageState());
            RegisterState(PlayerState.Dead, new DeadState());
        }

        /// <summary>
        /// Register a state in the dictionary
        /// </summary>
        private void RegisterState(PlayerState stateType, IPlayerState state)
        {
            if (state is BasePlayerState baseState)
            {
                baseState.Initialize(_controller);
            }

            _states[stateType] = state;
        }

        /// <summary>
        /// Change to a new state
        /// </summary>
        public void ChangeState(PlayerState newStateType)
        {
            // Check if state exists
            if (!_states.ContainsKey(newStateType))
            {
                Debug.LogError($"[TPSPlayerStateMachine] State {newStateType} not registered!");
                return;
            }

            // Don't change to the same state
            if (_currentStateType == newStateType)
                return;

            PlayerState previousStateType = _currentStateType;

            // Exit current state
            _currentState?.Exit();

            // Change to new state
            _currentStateType = newStateType;
            _currentState = _states[newStateType];

            // Enter new state
            _currentState.Enter();

            // Notify listeners
            OnStateChanged?.Invoke(newStateType);
            OnStateTransition?.Invoke(previousStateType, newStateType);

            Debug.Log($"[TPSPlayerStateMachine] State changed: {previousStateType} -> {newStateType}");
        }

        /// <summary>
        /// Transition to a new state (public interface for external calls)
        /// </summary>
        public void TransitionToState(PlayerState newStateType)
        {
            ChangeState(newStateType);
        }

        /// <summary>
        /// Update current state
        /// </summary>
        public void Update()
        {
            _currentState?.Update();

            // Handle automatic state transitions based on input and conditions
            HandleStateTransitions();
        }

        /// <summary>
        /// Handle automatic state transitions
        /// </summary>
        private void HandleStateTransitions()
        {
            if (_controller == null) return;

            // Handle death state - highest priority
            if (_controller.Health.IsDead)
            {
                ChangeState(PlayerState.Dead);
                return;
            }

            // Handle damage state
            if (_controller.Health.IsRecentlyDamaged)
            {
                ChangeState(PlayerState.TakingDamage);
                return;
            }

            // Handle ground state transitions
            if (_controller.IsGrounded)
            {
                HandleGroundedStateTransitions();
            }
            else
            {
                HandleAerialStateTransitions();
            }
        }

        /// <summary>
        /// Handle state transitions when player is grounded
        /// </summary>
        private void HandleGroundedStateTransitions()
        {
            Vector2 moveInput = _controller.GetMovementInput();
            bool isMoving = moveInput.magnitude > 0.1f;

            // Landing state
            if (_currentStateType == PlayerState.Falling || _currentStateType == PlayerState.Jumping)
            {
                ChangeState(PlayerState.Landing);
                return;
            }

            // Jump input
            if (_controller.IsJumpPressed() && CanJump())
            {
                ChangeState(PlayerState.Jumping);
                return;
            }

            // Crouch input
            if (_controller.IsCrouchPressed())
            {
                if (_currentStateType != PlayerState.Crouching)
                {
                    ChangeState(PlayerState.Crouching);
                    return;
                }
            }
            else if (_currentStateType == PlayerState.Crouching)
            {
                // Exit crouch
                ChangeState(isMoving ? PlayerState.Walking : PlayerState.Idle);
                return;
            }

            // Movement-based transitions
            if (isMoving)
            {
                if (_controller.IsSprintHeld() && CanSprint())
                {
                    ChangeState(PlayerState.Running);
                }
                else if (_currentStateType != PlayerState.Walking && _currentStateType != PlayerState.Running)
                {
                    ChangeState(PlayerState.Walking);
                }
                else if (_currentStateType == PlayerState.Running && !_controller.IsSprintHeld())
                {
                    ChangeState(PlayerState.Walking);
                }
            }
            else
            {
                // Not moving
                if (_currentStateType == PlayerState.Walking || _currentStateType == PlayerState.Running)
                {
                    ChangeState(PlayerState.Idle);
                }
            }
        }

        /// <summary>
        /// Handle state transitions when player is in air
        /// </summary>
        private void HandleAerialStateTransitions()
        {
            if (_currentStateType != PlayerState.Jumping && _currentStateType != PlayerState.Falling)
            {
                ChangeState(PlayerState.Falling);
            }
        }

        /// <summary>
        /// Check if player can jump
        /// </summary>
        private bool CanJump()
        {
            return _controller.IsGrounded &&
                   (_currentStateType == PlayerState.Idle ||
                    _currentStateType == PlayerState.Walking ||
                    _currentStateType == PlayerState.Running);
        }

        /// <summary>
        /// Check if player can sprint
        /// </summary>
        private bool CanSprint()
        {
            // Add stamina check here if needed
            return true;
        }

        /// <summary>
        /// Force a specific state (for external systems)
        /// </summary>
        public void ForceState(PlayerState stateType)
        {
            ChangeState(stateType);
        }

        /// <summary>
        /// Check if a specific state is current
        /// </summary>
        public bool IsInState(PlayerState stateType)
        {
            return _currentStateType == stateType;
        }

        /// <summary>
        /// Check if current state allows certain actions
        /// </summary>
        public bool CanPerformAction(PlayerAction action)
        {
            return action switch
            {
                PlayerAction.Move => _currentStateType != PlayerState.Dead &&
                                   _currentStateType != PlayerState.TakingDamage,
                PlayerAction.Jump => CanJump(),
                PlayerAction.Shoot => _currentStateType != PlayerState.Dead &&
                                    _currentStateType != PlayerState.Reloading,
                PlayerAction.Reload => _currentStateType != PlayerState.Dead &&
                                     _currentStateType != PlayerState.Shooting,
                _ => false
            };
        }

        /// <summary>
        /// Clean up state machine
        /// </summary>
        public void Cleanup()
        {
            _currentState?.Exit();
            _currentState = null;
            _states.Clear();
            OnStateChanged?.RemoveAllListeners();
            OnStateTransition?.RemoveAllListeners();
        }
    }

    /// <summary>
    /// Player actions for state validation
    /// </summary>
    public enum PlayerAction
    {
        Move,
        Jump,
        Sprint,
        Crouch,
        Shoot,
        Reload,
        Melee,
        TakeCover
    }
}
