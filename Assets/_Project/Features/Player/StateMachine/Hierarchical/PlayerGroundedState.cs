using UnityEngine;
using asterivo.Unity60.Core.StateMachine;

namespace asterivo.Unity60.Features.Player.StateMachine.Hierarchical
{
    /// <summary>
    /// Hierarchical state representing the player being on the ground.
    /// Manages child states like Idle, Walk, Run, and Crouch.
    /// Implements the example from 階層化ステートマシン詳細設計書.md lines 510-603.
    /// </summary>
    public class PlayerGroundedState : HierarchicalState<PlayerContext>
    {
        #region HierarchicalState Implementation

        /// <summary>
        /// Initialize child states for grounded player
        /// </summary>
        protected override void InitializeChildStates()
        {
            // Register child states as per design document
            AddChildState("Idle", new PlayerIdleState());
            AddChildState("Walk", new PlayerWalkState());
            AddChildState("Run", new PlayerRunState());
            AddChildState("Crouch", new PlayerCrouchState());

            // Set default state
            defaultChildStateKey = GetDefaultChildStateKey();
        }

        /// <summary>
        /// Get the default child state key
        /// </summary>
        /// <returns>Default child state identifier</returns>
        protected override string GetDefaultChildStateKey()
        {
            return "Idle";
        }

        /// <summary>
        /// Validate child state transitions
        /// </summary>
        /// <param name="childStateKey">Target child state</param>
        /// <param name="context">Player context</param>
        /// <returns>True if transition is valid</returns>
        protected override bool ValidateChildTransition(string childStateKey, PlayerContext context)
        {
            // Implement validation logic as per design document
            switch (childStateKey)
            {
                case "Run":
                    return context.InputManager.IsSprintPressed() && context.HasStamina();
                case "Crouch":
                    return context.InputManager.IsCrouchPressed() && !context.IsInCombat;
                default:
                    return true;
            }
        }

        #endregion

        #region Parent State Logic

        /// <summary>
        /// Parent state enter logic - grounded state initialization
        /// </summary>
        /// <param name="context">Player context</param>
        protected override void OnParentEnter(PlayerContext context)
        {
            // Grounded state specific initialization as per design document
            context.SetGrounded(true);
            context.PhysicsController.EnableGroundedPhysics();

            // Audio environment setting
            context.AudioManager.SetEnvironmentType(AudioEnvironmentType.Grounded);

            // Debug logging
            if (enableDebugLogging)
            {
                Debug.Log("[PlayerGroundedState] Entered grounded state");
            }
        }

        /// <summary>
        /// Parent state update logic - grounded state management
        /// </summary>
        /// <param name="context">Player context</param>
        protected override void OnParentUpdate(PlayerContext context)
        {
            // Grounded state common update processing as per design document
            CheckGroundedStatus(context);
            HandleMovementInput(context);
        }

        /// <summary>
        /// Parent state exit logic - grounded state cleanup
        /// </summary>
        /// <param name="context">Player context</param>
        protected override void OnParentExit(PlayerContext context)
        {
            // Grounded state termination processing
            context.SetGrounded(false);

            if (enableDebugLogging)
            {
                Debug.Log("[PlayerGroundedState] Exited grounded state");
            }
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Check if player is still grounded and handle state transitions
        /// </summary>
        /// <param name="context">Player context</param>
        private void CheckGroundedStatus(PlayerContext context)
        {
            if (!context.PhysicsController.IsGrounded())
            {
                // Trigger transition to airborne state
                context.StateMachine.RequestTransition("Airborne");
            }
        }

        /// <summary>
        /// Handle movement input and child state transitions
        /// </summary>
        /// <param name="context">Player context</param>
        private void HandleMovementInput(PlayerContext context)
        {
            var input = context.InputManager.GetMovementInput();

            if (input.magnitude > 0.1f)
            {
                // Movement input detected
                if (context.InputManager.IsSprintPressed() && context.HasStamina())
                {
                    TransitionToChild("Run", context);
                }
                else
                {
                    TransitionToChild("Walk", context);
                }
            }
            else if (context.InputManager.IsCrouchPressed())
            {
                // Crouch input
                TransitionToChild("Crouch", context);
            }
            else
            {
                // No movement input
                TransitionToChild("Idle", context);
            }
        }

        #endregion
    }

    #region Child State Implementations

    /// <summary>
    /// Player idle state - standing still
    /// </summary>
    public class PlayerIdleState : asterivo.Unity60.Core.Patterns.StateMachine.IState<PlayerContext>
    {
        public void Enter(PlayerContext context)
        {
            context.Velocity = Vector3.zero;
            Debug.Log("[PlayerIdleState] Player is now idle");
        }

        public void Update(PlayerContext context)
        {
            // Idle state logic - minimal movement, stamina recovery
            context.RestoreStamina(Time.deltaTime * 5f);
        }

        public void Exit(PlayerContext context)
        {
            Debug.Log("[PlayerIdleState] Exiting idle state");
        }

        public void FixedUpdate(PlayerContext context) { }
        public void HandleInput(PlayerContext context, object inputData) { }
    }

    /// <summary>
    /// Player walk state - normal movement
    /// </summary>
    public class PlayerWalkState : asterivo.Unity60.Core.Patterns.StateMachine.IState<PlayerContext>
    {
        public void Enter(PlayerContext context)
        {
            Debug.Log("[PlayerWalkState] Player is now walking");
        }

        public void Update(PlayerContext context)
        {
            // Walk state logic
            var input = context.InputManager.GetMovementInput();
            context.Velocity = new Vector3(input.x, 0, input.y) * context.MovementSpeed;
            
            // Slight stamina recovery while walking
            context.RestoreStamina(Time.deltaTime * 2f);
        }

        public void Exit(PlayerContext context)
        {
            Debug.Log("[PlayerWalkState] Exiting walk state");
        }

        public void FixedUpdate(PlayerContext context) { }
        public void HandleInput(PlayerContext context, object inputData) { }
    }

    /// <summary>
    /// Player run state - fast movement with stamina consumption
    /// </summary>
    public class PlayerRunState : asterivo.Unity60.Core.Patterns.StateMachine.IState<PlayerContext>
    {
        public void Enter(PlayerContext context)
        {
            Debug.Log("[PlayerRunState] Player is now running");
        }

        public void Update(PlayerContext context)
        {
            // Run state logic
            var input = context.InputManager.GetMovementInput();
            context.Velocity = new Vector3(input.x, 0, input.y) * context.MovementSpeed * 1.5f;
            
            // Consume stamina while running
            context.ConsumeStamina(Time.deltaTime * 10f);
        }

        public void Exit(PlayerContext context)
        {
            Debug.Log("[PlayerRunState] Exiting run state");
        }

        public void FixedUpdate(PlayerContext context) { }
        public void HandleInput(PlayerContext context, object inputData) { }
    }

    /// <summary>
    /// Player crouch state - stealth movement
    /// </summary>
    public class PlayerCrouchState : asterivo.Unity60.Core.Patterns.StateMachine.IState<PlayerContext>
    {
        public void Enter(PlayerContext context)
        {
            Debug.Log("[PlayerCrouchState] Player is now crouching");
        }

        public void Update(PlayerContext context)
        {
            // Crouch state logic
            var input = context.InputManager.GetMovementInput();
            context.Velocity = new Vector3(input.x, 0, input.y) * context.MovementSpeed * 0.5f;
            
            // Stamina recovery while crouching
            context.RestoreStamina(Time.deltaTime * 3f);
        }

        public void Exit(PlayerContext context)
        {
            Debug.Log("[PlayerCrouchState] Exiting crouch state");
        }

        public void FixedUpdate(PlayerContext context) { }
        public void HandleInput(PlayerContext context, object inputData) { }
    }

    #endregion
}