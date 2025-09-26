using UnityEngine;
using asterivo.Unity60.Core.StateMachine;
using asterivo.Unity60.Core.Audio;

namespace asterivo.Unity60.Features.Player.StateMachine.Hierarchical
{
    /// <summary>
    /// Hierarchical state representing the player being airborne.
    /// Manages child states like Jump, Fall, and Glide.
    /// Implements the example from 階層化ステートマシン詳細設計書.md lines 608-679.
    /// </summary>
    public class PlayerAirborneState : HierarchicalState<PlayerContext>
    {
        #region HierarchicalState Implementation

        /// <summary>
        /// Initialize child states for airborne player
        /// </summary>
        protected override void InitializeChildStates()
        {
            // Register child states as per design document
            AddChildState("Jump", new PlayerJumpState());
            AddChildState("Fall", new PlayerFallState());
            AddChildState("Glide", new PlayerGlideState());

            // Set default state
            defaultChildStateKey = GetDefaultChildStateKey();
        }

        /// <summary>
        /// Get the default child state key
        /// </summary>
        /// <returns>Default child state identifier</returns>
        protected override string GetDefaultChildStateKey()
        {
            return "Fall";
        }

        #endregion

        #region Parent State Logic

        /// <summary>
        /// Parent state enter logic - airborne state initialization
        /// </summary>
        /// <param name="context">Player context</param>
        protected override void OnParentEnter(PlayerContext context)
        {
            // Airborne state specific initialization as per design document
            context.SetGrounded(false);
            context.PhysicsController.EnableAirbornePhysics();
            context.AudioManager.SetEnvironmentType(AudioEnvironmentType.Airborne);

            // Determine initial child state based on current vertical velocity
            if (context.PhysicsController.GetVerticalVelocity() > 0)
            {
                TransitionToChild("Jump", context);
            }
            else
            {
                TransitionToChild("Fall", context);
            }

            // Debug logging
            if (enableDebugLogging)
            {
                Debug.Log("[PlayerAirborneState] Entered airborne state");
            }
        }

        /// <summary>
        /// Parent state update logic - airborne state management
        /// </summary>
        /// <param name="context">Player context</param>
        protected override void OnParentUpdate(PlayerContext context)
        {
            // Airborne state common update processing as per design document
            CheckLandingStatus(context);
            HandleAirMovement(context);
        }

        /// <summary>
        /// Parent state exit logic - airborne state cleanup
        /// </summary>
        /// <param name="context">Player context</param>
        protected override void OnParentExit(PlayerContext context)
        {
            if (enableDebugLogging)
            {
                Debug.Log("[PlayerAirborneState] Exited airborne state");
            }
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Check if player has landed and handle state transitions
        /// </summary>
        /// <param name="context">Player context</param>
        private void CheckLandingStatus(PlayerContext context)
        {
            if (context.PhysicsController.IsGrounded())
            {
                // Trigger transition to grounded state
                context.StateMachine.RequestTransition("Grounded");
            }
        }

        /// <summary>
        /// Handle air movement and child state transitions based on vertical velocity
        /// </summary>
        /// <param name="context">Player context</param>
        private void HandleAirMovement(PlayerContext context)
        {
            var verticalVelocity = context.PhysicsController.GetVerticalVelocity();

            if (verticalVelocity > 0.5f && GetCurrentChildStateKey() != "Jump")
            {
                // Moving upward - transition to jump state
                TransitionToChild("Jump", context);
            }
            else if (verticalVelocity < -0.5f && GetCurrentChildStateKey() != "Fall")
            {
                // Moving downward - check for glide or fall
                if (context.InputManager.IsGlidePressed() && context.HasStamina())
                {
                    TransitionToChild("Glide", context);
                }
                else
                {
                    TransitionToChild("Fall", context);
                }
            }
        }

        #endregion
    }

    #region Child State Implementations

    /// <summary>
    /// Player jump state - upward movement
    /// </summary>
    public class PlayerJumpState : asterivo.Unity60.Core.Patterns.StateMachine.IState<PlayerContext>
    {
        public void Enter(PlayerContext context)
        {
            Debug.Log("[PlayerJumpState] Player is jumping");
        }

        public void Update(PlayerContext context)
        {
            // Jump state logic - allow air control
            var input = context.InputManager.GetMovementInput();
            if (input.magnitude > 0.1f)
            {
                // Apply air control (reduced compared to ground movement)
                var airControl = new Vector3(input.x, 0, input.y) * context.MovementSpeed * 0.3f;
                context.Velocity = new Vector3(airControl.x, context.Velocity.y, airControl.z);
            }

            // Consume small amount of stamina during jump
            context.ConsumeStamina(Time.deltaTime * 2f);
        }

        public void Exit(PlayerContext context)
        {
            Debug.Log("[PlayerJumpState] Exiting jump state");
        }

        public void FixedUpdate(PlayerContext context) { }
        public void HandleInput(PlayerContext context, object inputData) { }
    }

    /// <summary>
    /// Player fall state - downward movement
    /// </summary>
    public class PlayerFallState : asterivo.Unity60.Core.Patterns.StateMachine.IState<PlayerContext>
    {
        public void Enter(PlayerContext context)
        {
            Debug.Log("[PlayerFallState] Player is falling");
        }

        public void Update(PlayerContext context)
        {
            // Fall state logic - limited air control
            var input = context.InputManager.GetMovementInput();
            if (input.magnitude > 0.1f)
            {
                // Apply minimal air control while falling
                var airControl = new Vector3(input.x, 0, input.y) * context.MovementSpeed * 0.2f;
                context.Velocity = new Vector3(airControl.x, context.Velocity.y, airControl.z);
            }

            // Slight stamina recovery during fall
            context.RestoreStamina(Time.deltaTime * 1f);
        }

        public void Exit(PlayerContext context)
        {
            Debug.Log("[PlayerFallState] Exiting fall state");
        }

        public void FixedUpdate(PlayerContext context) { }
        public void HandleInput(PlayerContext context, object inputData) { }
    }

    /// <summary>
    /// Player glide state - controlled falling with stamina consumption
    /// </summary>
    public class PlayerGlideState : asterivo.Unity60.Core.Patterns.StateMachine.IState<PlayerContext>
    {
        public void Enter(PlayerContext context)
        {
            Debug.Log("[PlayerGlideState] Player is gliding");
        }

        public void Update(PlayerContext context)
        {
            // Glide state logic - enhanced air control with stamina cost
            var input = context.InputManager.GetMovementInput();
            if (input.magnitude > 0.1f)
            {
                // Apply enhanced air control while gliding
                var airControl = new Vector3(input.x, 0, input.y) * context.MovementSpeed * 0.6f;
                context.Velocity = new Vector3(airControl.x, context.Velocity.y * 0.8f, airControl.z); // Reduce fall speed
            }

            // Consume stamina while gliding
            context.ConsumeStamina(Time.deltaTime * 8f);

            // Stop gliding if out of stamina
            if (!context.HasStamina())
            {
                Debug.Log("[PlayerGlideState] Out of stamina, transitioning to fall");
                // Note: This would typically trigger a transition back to parent state
                // which would then evaluate and transition to Fall state
            }
        }

        public void Exit(PlayerContext context)
        {
            Debug.Log("[PlayerGlideState] Exiting glide state");
        }

        public void FixedUpdate(PlayerContext context) { }
        public void HandleInput(PlayerContext context, object inputData) { }
    }

    #endregion

    #region Extension Methods for Player Context

    /// <summary>
    /// Extension methods for PlayerContext to support gliding
    /// </summary>
    public static class PlayerContextExtensions
    {
        /// <summary>
        /// Check if player can glide
        /// </summary>
        /// <param name="context">Player context</param>
        /// <returns>True if player can glide</returns>
        public static bool CanGlide(this PlayerContext context)
        {
            return context.HasStamina(10f) && !context.IsInCombat;
        }
    }

    #endregion
}