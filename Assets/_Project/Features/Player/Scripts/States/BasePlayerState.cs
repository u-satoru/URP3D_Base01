using UnityEngine;

namespace asterivo.Unity60.Player.States
{
    /// <summary>
    /// プレイヤーの各状態の基底クラスです。
    /// IPlayerStateインターフェースを実装し、共通の振る舞いや状態管理を提供します。
    /// </summary>
    public abstract class BasePlayerState : IPlayerState
    {
        /// <summary>
        /// 現在の状態に入ってからの経過時間。
        /// </summary>
        protected float stateTime;

        /// <summary>
        /// この状態に遷移した際に一度だけ呼び出されます。
        /// 状態の初期化処理を行い、stateTimeをリセットします。
        /// </summary>
        /// <param name="stateMachine">このステートを管理するステートマシン。</param>
        public virtual void Enter(DetailedPlayerStateMachine stateMachine)
        {
            stateTime = 0f;
        }

        /// <summary>
        /// この状態から他の状態に遷移する際に一度だけ呼び出されます。
        /// 状態の終了処理を行います。
        /// </summary>
        /// <param name="stateMachine">このステートを管理するステートマシン。</param>
        public virtual void Exit(DetailedPlayerStateMachine stateMachine) { }

        /// <summary>
        /// この状態である間、毎フレーム呼び出されます。
        /// stateTimeを更新します。
        /// </summary>
        /// <param name="stateMachine">このステートを管理するステートマシン。</param>
        public virtual void Update(DetailedPlayerStateMachine stateMachine)
        {
            stateTime += Time.deltaTime;
        }

        /// <summary>
        /// この状態である間、固定時間間隔で呼び出されます。
        /// 物理演算関連のロジックを処理します。
        /// </summary>
        /// <param name="stateMachine">このステートを管理するステートマシン。</param>
        public virtual void FixedUpdate(DetailedPlayerStateMachine stateMachine) { }

        /// <summary>
        /// プレイヤーからの入力を処理します。
        /// </summary>
        /// <param name="stateMachine">このステートを管理するステートマシン。</param>
        /// <param name="moveInput">移動入力ベクトル。</param>
        /// <param name="jumpInput">ジャンプ入力フラグ。</param>
        public virtual void HandleInput(DetailedPlayerStateMachine stateMachine, Vector2 moveInput, bool jumpInput) { }
    }

    // NOTE: The following states now correctly implement IPlayerState through inheritance,
    // which should resolve the compilation errors. Their internal logic may need
    // to be updated later to use the new command-based input.

    /// <summary>
    /// プレイヤーの走行状態を管理します。
    /// </summary>
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
    /// プレイヤーの落下状態を管理します。
    /// </summary>
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
    /// プレイヤーの着地状態を管理します。
    /// </summary>
    public class LandingState : BasePlayerState
    {
        /// <summary>
        /// 着地アニメーションの持続時間。
        /// </summary>
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
    /// プレイヤーの戦闘状態を管理します。
    /// </summary>
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
    /// プレイヤーの戦闘待機状態を管理します。
    /// </summary>
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
    /// プレイヤーの戦闘攻撃状態を管理します。
    /// </summary>
    public class CombatAttackingState : BasePlayerState
    {
        public override void Enter(DetailedPlayerStateMachine stateMachine)
        {
            base.Enter(stateMachine);
            // animator?.SetTrigger("Attack");
        }
    }

    /// <summary>
    /// プレイヤーのインタラクト状態を管理します。
    /// </summary>
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
    /// プレイヤーの死亡状態を管理します。
    /// </summary>
    public class DeadState : BasePlayerState
    {
        public override void Enter(DetailedPlayerStateMachine stateMachine)
        {
            base.Enter(stateMachine);
            // animator?.SetTrigger("Death");
        }
    }
}