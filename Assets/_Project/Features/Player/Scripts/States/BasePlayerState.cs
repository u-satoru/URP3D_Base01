using UnityEngine;

namespace asterivo.Unity60.Player.States
{
    /// <summary>
    /// 基底ステートクラス
    /// </summary>
    public abstract class BasePlayerState : IPlayerState
    {
        protected float stateTime;

        public virtual void Enter(DetailedPlayerStateMachine stateMachine)
        {
            stateTime = 0f;
        }

        public virtual void Exit(DetailedPlayerStateMachine stateMachine) { }

        public virtual void Update(DetailedPlayerStateMachine stateMachine)
        {
            stateTime += Time.deltaTime;
        }

        public virtual void FixedUpdate(DetailedPlayerStateMachine stateMachine) { }

        public virtual void HandleInput(DetailedPlayerStateMachine stateMachine, Vector2 moveInput, bool jumpInput) { }
    }

    // NOTE: The following states now correctly implement IPlayerState through inheritance,
    // which should resolve the compilation errors. Their internal logic may need
    // to be updated later to use the new command-based input.

    public class SprintingState : BasePlayerState
    {
        public override void Enter(DetailedPlayerStateMachine stateMachine)
        {
            base.Enter(stateMachine);
            // animator?.SetBool("IsSprinting", true);
        }

        public override void Exit(DetailedPlayerStateMachine stateMachine)
        {
            // animator?.SetBool("IsSprinting", false);
        }
    }

    public class JumpingState : BasePlayerState
    {
        public override void Enter(DetailedPlayerStateMachine stateMachine)
        {
            base.Enter(stateMachine);
            // animator?.SetTrigger("Jump");
            // animator?.SetBool("IsAirborne", true);
        }

        public override void Exit(DetailedPlayerStateMachine stateMachine)
        {
            // animator?.SetBool("IsAirborne", false);
        }
    }

    public class FallingState : BasePlayerState
    {
        public override void Enter(DetailedPlayerStateMachine stateMachine)
        {
            base.Enter(stateMachine);
            // animator?.SetBool("IsFalling", true);
        }

        public override void Exit(DetailedPlayerStateMachine stateMachine)
        {
            // animator?.SetBool("IsFalling", false);
        }
    }

    public class LandingState : BasePlayerState
    {
        private float landingDuration = 0.3f;

        public override void Update(DetailedPlayerStateMachine stateMachine)
        {
            base.Update(stateMachine);

            if (stateTime >= landingDuration)
            {
                stateMachine.TransitionToState(PlayerStateType.Idle);
            }
        }
    }

    public class CombatState : BasePlayerState
    {
        public override void Enter(DetailedPlayerStateMachine stateMachine)
        {
            base.Enter(stateMachine);
            // animator?.SetBool("InCombat", true);
            // animator?.SetLayerWeight(1, 1f);
        }

        public override void Exit(DetailedPlayerStateMachine stateMachine)
        {
            // animator?.SetBool("InCombat", false);
            // animator?.SetLayerWeight(1, 0f);
        }
    }

    public class CombatIdleState : BasePlayerState
    {
        public override void Enter(DetailedPlayerStateMachine stateMachine)
        {
            base.Enter(stateMachine);
            // animator?.SetBool("CombatIdle", true);
        }

        public override void Exit(DetailedPlayerStateMachine stateMachine)
        {
            // animator?.SetBool("CombatIdle", false);
        }
    }

    public class CombatAttackingState : BasePlayerState
    {
        public override void Enter(DetailedPlayerStateMachine stateMachine)
        {
            base.Enter(stateMachine);
            // animator?.SetTrigger("Attack");
        }
    }

    public class InteractingState : BasePlayerState
    {
        public override void Enter(DetailedPlayerStateMachine stateMachine)
        {
            base.Enter(stateMachine);
            // animator?.SetBool("IsInteracting", true);
        }

        public override void Exit(DetailedPlayerStateMachine stateMachine)
        {
            // animator?.SetBool("IsInteracting", false);
        }
    }

    public class DeadState : BasePlayerState
    {
        public override void Enter(DetailedPlayerStateMachine stateMachine)
        {
            base.Enter(stateMachine);
            // animator?.SetTrigger("Death");
        }
    }
}