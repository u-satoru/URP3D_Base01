using UnityEngine;
using asterivo.Unity60.Core.Player;
using asterivo.Unity60.Player;  // PlayerStateMachineクラスの参照に必要

namespace asterivo.Unity60.Player.States
{
    /// <summary>
    /// 基底ステートクラス
    /// </summary>
    public abstract class BasePlayerState
    {
        protected PlayerStateMachine stateMachine;
        protected Animator animator;
        protected float stateTime;
        
        protected BasePlayerState(PlayerStateMachine sm, Animator anim)
        {
            stateMachine = sm;
            animator = anim;
        }
        
        public virtual void Enter()
        {
            stateTime = 0f;
        }
        
        public virtual void Update()
        {
            stateTime += Time.deltaTime;
        }
        
        public virtual void FixedUpdate() { }
        public virtual void LateUpdate() { }
        public virtual void Exit() { }
        
        public virtual bool CanTransitionTo(PlayerState state)
        {
            return true;
        }
    }
    
    public class SprintingState : BasePlayerState
    {
        public SprintingState(PlayerStateMachine sm, Animator anim) : base(sm, anim) { }
        
        public override void Enter()
        {
            base.Enter();
            animator?.SetBool("IsSprinting", true);
        }
        
        public override void Exit()
        {
            animator?.SetBool("IsSprinting", false);
        }
    }
    
    public class JumpingState : BasePlayerState
    {
        public JumpingState(PlayerStateMachine sm, Animator anim) : base(sm, anim) { }
        
        public override void Enter()
        {
            base.Enter();
            animator?.SetTrigger("Jump");
            animator?.SetBool("IsAirborne", true);
        }
        
        public override void Exit()
        {
            animator?.SetBool("IsAirborne", false);
        }
        
        public override bool CanTransitionTo(PlayerState state)
        {
            // ジャンプ中は落下または着地のみ許可
            return state == PlayerState.Falling || state == PlayerState.Landing;
        }
    }
    
    public class FallingState : BasePlayerState
    {
        public FallingState(PlayerStateMachine sm, Animator anim) : base(sm, anim) { }
        
        public override void Enter()
        {
            base.Enter();
            animator?.SetBool("IsFalling", true);
        }
        
        public override void Exit()
        {
            animator?.SetBool("IsFalling", false);
        }
    }
    
    public class LandingState : BasePlayerState
    {
        private float landingDuration = 0.3f;
        
        public LandingState(PlayerStateMachine sm, Animator anim) : base(sm, anim) { }
        
        public override void Enter()
        {
            base.Enter();
            animator?.SetTrigger("Land");
        }
        
        public override void Update()
        {
            base.Update();
            
            // 着地アニメーション終了後、自動的にアイドルへ
            if (stateTime >= landingDuration)
            {
                stateMachine.TransitionToState(PlayerStateMachine.PlayerStateType.Idle);
            }
        }
    }
    
    public class CombatState : BasePlayerState
    {
        public CombatState(PlayerStateMachine sm, Animator anim) : base(sm, anim) { }
        
        public override void Enter()
        {
            base.Enter();
            animator?.SetBool("InCombat", true);
            animator?.SetLayerWeight(1, 1f); // 戦闘レイヤー有効化
        }
        
        public override void Exit()
        {
            animator?.SetBool("InCombat", false);
            animator?.SetLayerWeight(1, 0f);
        }
    }
    
    public class CombatIdleState : BasePlayerState
    {
        public CombatIdleState(PlayerStateMachine sm, Animator anim) : base(sm, anim) { }
        
        public override void Enter()
        {
            base.Enter();
            animator?.SetBool("CombatIdle", true);
        }
        
        public override void Exit()
        {
            animator?.SetBool("CombatIdle", false);
        }
    }
    
    public class CombatAttackingState : BasePlayerState
    {
        public CombatAttackingState(PlayerStateMachine sm, Animator anim) : base(sm, anim) { }
        
        public override void Enter()
        {
            base.Enter();
            animator?.SetTrigger("Attack");
        }
    }
    
    public class InteractingState : BasePlayerState
    {
        public InteractingState(PlayerStateMachine sm, Animator anim) : base(sm, anim) { }
        
        public override void Enter()
        {
            base.Enter();
            animator?.SetBool("IsInteracting", true);
        }
        
        public override void Exit()
        {
            animator?.SetBool("IsInteracting", false);
        }
        
        public override bool CanTransitionTo(PlayerState state)
        {
            // インタラクション中は移動不可
            return state == PlayerState.Idle;
        }
    }
    
    public class DeadState : BasePlayerState
    {
        public DeadState(PlayerStateMachine sm, Animator anim) : base(sm, anim) { }
        
        public override void Enter()
        {
            base.Enter();
            animator?.SetTrigger("Death");
        }
        
        public override bool CanTransitionTo(PlayerState state)
        {
            // リスポーン時のみアイドルへ遷移可能
            return state == PlayerState.Idle;
        }
    }
}