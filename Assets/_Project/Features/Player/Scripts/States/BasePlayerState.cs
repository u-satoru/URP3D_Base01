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

    /// <summary>
    /// ジャンプ状態 - プレイヤーがジャンプ中の状態を管理
    /// </summary>
    public class JumpingState : BasePlayerState
    {
        private float jumpStartTime;
        private float jumpDuration = 0.5f; // ジャンプ継続時間
        private bool isGrounded = false;
        
        public override void Enter(DetailedPlayerStateMachine stateMachine)
        {
            base.Enter(stateMachine);
            jumpStartTime = Time.time;
            isGrounded = false;
            
            // ジャンプ力を適用（Rigidbodyがある場合）
            var rigidbody = stateMachine.GetComponent<Rigidbody>();
            if (rigidbody != null)
            {
                rigidbody.linearVelocity = new Vector3(rigidbody.linearVelocity.x, 8f, rigidbody.linearVelocity.z);
            }
            
            // ステルス状態を立ち姿勢に設定
            if (stateMachine.StealthMovement != null)
            {
                stateMachine.StealthMovement.SetStance(Core.Data.MovementStance.Standing);
            }
            
            Debug.Log("Player entered Jumping state");
            // animator?.SetTrigger("Jump");
            // animator?.SetBool("IsAirborne", true);
        }

        public override void Exit(DetailedPlayerStateMachine stateMachine)
        {
            Debug.Log("Player exited Jumping state");
            // animator?.SetBool("IsAirborne", false);
        }
        
        public override void Update(DetailedPlayerStateMachine stateMachine)
        {
            base.Update(stateMachine);
            
            // 地面接触判定（簡易版）
            CheckGroundContact(stateMachine);
            
            // ジャンプ時間経過または着地で状態遷移
            if (isGrounded || Time.time - jumpStartTime > jumpDuration)
            {
                TransitionToGroundState(stateMachine);
            }
        }

        public override void HandleInput(DetailedPlayerStateMachine stateMachine, Vector2 moveInput, bool jumpInput)
        {
            // ジャンプ中は追加のジャンプ入力を無視
            // 移動入力のみ受け付けて空中での方向制御を可能にする
            
            var rigidbody = stateMachine.GetComponent<Rigidbody>();
            if (rigidbody != null && moveInput.magnitude > 0.1f)
            {
                // 空中での微調整移動
                Vector3 moveDirection = new Vector3(moveInput.x, 0, moveInput.y) * 2f;
                rigidbody.AddForce(moveDirection, ForceMode.Acceleration);
            }
        }
        
        /// <summary>
        /// 簡易的な地面接触判定
        /// </summary>
        private void CheckGroundContact(DetailedPlayerStateMachine stateMachine)
        {
            // Raycastによる地面判定
            RaycastHit hit;
            Vector3 origin = stateMachine.transform.position + Vector3.up * 0.1f;
            
            if (Physics.Raycast(origin, Vector3.down, out hit, 1.2f))
            {
                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Ground") || 
                    hit.collider.CompareTag("Ground"))
                {
                    isGrounded = true;
                }
            }
        }
        
        /// <summary>
        /// 着地後の状態遷移を決定
        /// </summary>
        private void TransitionToGroundState(DetailedPlayerStateMachine stateMachine)
        {
            // 現在の移動入力に基づいて適切な状態に遷移
            var playerController = stateMachine.GetComponent<PlayerController>();
            if (playerController != null)
            {
                // PlayerControllerから現在の入力を取得（実装に合わせて調整が必要）
                stateMachine.TransitionToState(PlayerStateType.Idle);
            }
            else
            {
                // フォールバック：アイドル状態に遷移
                stateMachine.TransitionToState(PlayerStateType.Idle);
            }
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