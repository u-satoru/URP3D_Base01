using UnityEngine;
using Debug = UnityEngine.Debug;
using System.Collections;
using asterivo.Unity60.Player;

namespace asterivo.Unity60.Features.Player.States
{
    /// <summary>
    /// プレイヤーのローリング状態を管琁E��るクラス、E    /// こ�E状態では、�Eレイヤーは一定時間前方に移動し、外部からの入力を受け付けません、E    /// </summary>
    public class RollingState : IPlayerState
    {
        private float rollSpeed = 8f;
        private float rollDuration = 1.0f;

        /// <summary>
        /// 状態が開始されたときに呼び出されます、E        /// ローリングのタイマ�Eを開始し、アニメーションをトリガーします、E        /// </summary>
        /// <param name="stateMachine">プレイヤーのスチE�Eト�Eシン、E/param>
        public void Enter(DetailedPlayerStateMachine stateMachine)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log("Entering Rolling State");
#endif
            stateMachine.StartCoroutine(RollTimer(stateMachine));
            
            // アニメーターを取得してローリングアニメーションを開始
            // TODO: DetailedPlayerStateMachineにAnimatorプロパティを追加するか、イベント経由でアニメーション制御する必要あり
            // Animator animator = stateMachine.GetComponent<Animator>();
            // if (animator != null)
            // {
            //     animator.SetTrigger("Roll");
            //     animator.SetBool("IsRolling", true);
            // }

            // ローリング中は無敵時間を設定（例：ダメージを受けない）
            // TODO: HealthComponentが実装されたら有効化
            // var healthComponent = stateMachine.GetComponent<HealthComponent>();
            // if (healthComponent != null)
            // {
            //     healthComponent.SetInvulnerable(true, rollDuration);
            // }
        }

        /// <summary>
        /// 状態が終亁E��たときに呼び出されます、E        /// </summary>
        /// <param name="stateMachine">プレイヤーのスチE�Eト�Eシン、E/param>
        public void Exit(DetailedPlayerStateMachine stateMachine)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log("Exiting Rolling State");
#endif
            
            // アニメーションフラグをリセット
            // TODO: DetailedPlayerStateMachineにAnimatorプロパティを追加するか、イベント経由でアニメーション制御する必要あり
            // Animator animator = stateMachine.GetComponent<Animator>();
            // if (animator != null)
            // {
            //     animator.SetBool("IsRolling", false);
            // }
        }

        /// <summary>
        /// 毎フレーム呼び出されます、E        /// ローリングによる移動�E琁E��適用します、E        /// </summary>
        /// <param name="stateMachine">プレイヤーのスチE�Eト�Eシン、E/param>
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
        /// 固定フレームレートで呼び出されます。物琁E��算�E更新に使用されます、E        /// こ�E状態では使用されません、E        /// </summary>
        /// <param name="stateMachine">プレイヤーのスチE�Eト�Eシン、E/param>
        public void FixedUpdate(DetailedPlayerStateMachine stateMachine)
        {
            // Not used
        }

        /// <summary>
        /// プレイヤーの入力を処琁E��ます、E        /// ローリング中は入力を無視します、E        /// </summary>
        /// <param name="stateMachine">プレイヤーのスチE�Eト�Eシン、E/param>
        /// <param name="moveInput">移動�E力、E/param>
        /// <param name="jumpInput">ジャンプ�E力、E/param>
        public void HandleInput(DetailedPlayerStateMachine stateMachine, Vector2 moveInput, bool jumpInput)
        {
            // Ignore input during roll
        }

        /// <summary>
        /// ローリングの継続時間を管琁E��、終亁E��に歩行状態に遷移します、E        /// </summary>
        /// <param name="stateMachine">プレイヤーのスチE�Eト�Eシン、E/param>
        private IEnumerator RollTimer(DetailedPlayerStateMachine stateMachine)
        {
            yield return new WaitForSeconds(rollDuration);
            stateMachine.TransitionToState(PlayerStateType.Walking);
        }
    }
}
