using UnityEngine;

namespace asterivo.Unity60.Features.Player.States
{
    /// <summary>
    /// プレイヤーの吁E��態�E基底クラスです、E    /// IPlayerStateインターフェースを実裁E��、�E通�E振る�EぁE��状態管琁E��提供します、E    /// </summary>
    public abstract class BasePlayerState : IPlayerState
    {
        /// <summary>
        /// 現在の状態に入ってからの経過時間、E        /// </summary>
        protected float stateTime;

        /// <summary>
        /// こ�E状態に遷移した際に一度だけ呼び出されます、E        /// 状態�E初期化�E琁E��行い、stateTimeをリセチE��します、E        /// </summary>
        /// <param name="stateMachine">こ�EスチE�Eトを管琁E��るスチE�Eト�Eシン、E/param>
        public virtual void Enter(DetailedPlayerStateMachine stateMachine)
        {
            stateTime = 0f;
        }

        /// <summary>
        /// こ�E状態から他�E状態に遷移する際に一度だけ呼び出されます、E        /// 状態�E終亁E�E琁E��行います、E        /// </summary>
        /// <param name="stateMachine">こ�EスチE�Eトを管琁E��るスチE�Eト�Eシン、E/param>
        public virtual void Exit(DetailedPlayerStateMachine stateMachine) { }

        /// <summary>
        /// こ�E状態である間、毎フレーム呼び出されます、E        /// stateTimeを更新します、E        /// </summary>
        /// <param name="stateMachine">こ�EスチE�Eトを管琁E��るスチE�Eト�Eシン、E/param>
        public virtual void Update(DetailedPlayerStateMachine stateMachine)
        {
            stateTime += Time.deltaTime;
        }

        /// <summary>
        /// こ�E状態である間、固定時間間隔で呼び出されます、E        /// 物琁E��算関連のロジチE��を�E琁E��ます、E        /// </summary>
        /// <param name="stateMachine">こ�EスチE�Eトを管琁E��るスチE�Eト�Eシン、E/param>
        public virtual void FixedUpdate(DetailedPlayerStateMachine stateMachine) { }

        /// <summary>
        /// プレイヤーからの入力を処琁E��ます、E        /// </summary>
        /// <param name="stateMachine">こ�EスチE�Eトを管琁E��るスチE�Eト�Eシン、E/param>
        /// <param name="moveInput">移動�E力�Eクトル、E/param>
        /// <param name="jumpInput">ジャンプ�E力フラグ、E/param>
        public virtual void HandleInput(DetailedPlayerStateMachine stateMachine, Vector2 moveInput, bool jumpInput) { }
    }

    // NOTE: The following states now correctly implement IPlayerState through inheritance,
    // which should resolve the compilation errors. Their internal logic may need
    // to be updated later to use the new command-based input.

    /// <summary>
    /// プレイヤーの走行状態を管琁E��ます、E    /// </summary>
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


    /// <summary>
    /// プレイヤーの落下状態を管琁E��ます、E    /// </summary>
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

    /// <summary>
    /// プレイヤーの着地状態を管琁E��ます、E    /// </summary>
    public class LandingState : BasePlayerState
    {
        /// <summary>
        /// 着地アニメーションの持続時間、E        /// </summary>
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

    /// <summary>
    /// プレイヤーの戦闘状態を管琁E��ます、E    /// </summary>
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

    /// <summary>
    /// プレイヤーの戦闘征E��状態を管琁E��ます、E    /// </summary>
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

    /// <summary>
    /// プレイヤーの戦闘攻撁E��態を管琁E��ます、E    /// </summary>
    public class CombatAttackingState : BasePlayerState
    {
        public override void Enter(DetailedPlayerStateMachine stateMachine)
        {
            base.Enter(stateMachine);
            // animator?.SetTrigger("Attack");
        }
    }

    /// <summary>
    /// プレイヤーのインタラクト状態を管琁E��ます、E    /// </summary>
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

    /// <summary>
    /// プレイヤーの死亡状態を管琁E��ます、E    /// </summary>
    public class DeadState : BasePlayerState
    {
        public override void Enter(DetailedPlayerStateMachine stateMachine)
        {
            base.Enter(stateMachine);
            // animator?.SetTrigger("Death");
        }
    }
}
