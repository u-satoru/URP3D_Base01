using UnityEngine;
using Debug = UnityEngine.Debug;
using System.Collections;
using asterivo.Unity60.Player;

namespace asterivo.Unity60.Player.States
{
    /// <summary>
    /// プレイヤーのローリング状態を管理するクラス。
    /// この状態では、プレイヤーは一定時間前方に移動し、外部からの入力を受け付けません。
    /// </summary>
    public class RollingState : IPlayerState
    {
        private float rollSpeed = 8f;
        private float rollDuration = 1.0f;

        /// <summary>
        /// 状態が開始されたときに呼び出されます。
        /// ローリングのタイマーを開始し、アニメーションをトリガーします。
        /// </summary>
        /// <param name="stateMachine">プレイヤーのステートマシン。</param>
        public void Enter(DetailedPlayerStateMachine stateMachine)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log("Entering Rolling State");
#endif
            stateMachine.StartCoroutine(RollTimer(stateMachine));
            
            // アニメーターを取得してローリングアニメーションを再生
            Animator animator = stateMachine.GetComponent<Animator>();
            if (animator != null)
            {
                animator.SetTrigger("Roll");
                animator.SetBool("IsRolling", true);
            }
            
            // ローリング中は無敵時間を設定（例：ダメージを受けない）
            var healthComponent = stateMachine.GetComponent<HealthComponent>();
            if (healthComponent != null)
            {
                healthComponent.SetInvulnerable(true, rollDuration);
            }
        }

        /// <summary>
        /// 状態が終了したときに呼び出されます。
        /// </summary>
        /// <param name="stateMachine">プレイヤーのステートマシン。</param>
        public void Exit(DetailedPlayerStateMachine stateMachine)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log("Exiting Rolling State");
#endif
            
            // アニメーションフラグをリセット
            Animator animator = stateMachine.GetComponent<Animator>();
            if (animator != null)
            {
                animator.SetBool("IsRolling", false);
            }
        }

        /// <summary>
        /// 毎フレーム呼び出されます。
        /// ローリングによる移動処理を適用します。
        /// </summary>
        /// <param name="stateMachine">プレイヤーのステートマシン。</param>
        public void Update(DetailedPlayerStateMachine stateMachine)
        {
            if (stateMachine.CharacterController == null) return;

            Vector3 moveDirection = stateMachine.transform.forward * rollSpeed;

            if (!stateMachine.CharacterController.isGrounded)
            {
                moveDirection.y += Physics.gravity.y * Time.deltaTime;
            }

            stateMachine.CharacterController.Move(moveDirection * Time.deltaTime);
        }

        /// <summary>
        /// 固定フレームレートで呼び出されます。物理演算の更新に使用されます。
        /// この状態では使用されません。
        /// </summary>
        /// <param name="stateMachine">プレイヤーのステートマシン。</param>
        public void FixedUpdate(DetailedPlayerStateMachine stateMachine)
        {
            // Not used
        }

        /// <summary>
        /// プレイヤーの入力を処理します。
        /// ローリング中は入力を無視します。
        /// </summary>
        /// <param name="stateMachine">プレイヤーのステートマシン。</param>
        /// <param name="moveInput">移動入力。</param>
        /// <param name="jumpInput">ジャンプ入力。</param>
        public void HandleInput(DetailedPlayerStateMachine stateMachine, Vector2 moveInput, bool jumpInput)
        {
            // Ignore input during roll
        }

        /// <summary>
        /// ローリングの継続時間を管理し、終了後に歩行状態に遷移します。
        /// </summary>
        /// <param name="stateMachine">プレイヤーのステートマシン。</param>
        private IEnumerator RollTimer(DetailedPlayerStateMachine stateMachine)
        {
            yield return new WaitForSeconds(rollDuration);
            stateMachine.TransitionToState(PlayerStateType.Walking);
        }
    }
}
