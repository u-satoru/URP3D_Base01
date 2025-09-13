using UnityEngine;

namespace asterivo.Unity60.Player.States
{
    /// <summary>
    /// プレイヤーの走行状態を管理するクラス。
    /// </summary>
    public class RunningState : IPlayerState
    {
        private float runSpeed = 7f;
        private Vector2 _moveInput;
        
        /// <summary>
        /// 状態が開始されたときに呼び出されます。
        /// プレイヤーの姿勢を立位にし、走行状態フラグを設定します。
        /// </summary>
        /// <param name="stateMachine">プレイヤーのステートマシン。</param>
        public void Enter(DetailedPlayerStateMachine stateMachine)
        {
            if (stateMachine.StealthMovement != null)
            {
                stateMachine.StealthMovement.SetStance(Core.Data.MovementStance.Standing);
                stateMachine.StealthMovement.SetRunning(true);
            }
        }
        
        /// <summary>
        /// 状態が終了したときに呼び出されます。
        /// 走行状態フラグを解除し、移動入力をリセットします。
        /// </summary>
        /// <param name="stateMachine">プレイヤーのステートマシン。</param>
        public void Exit(DetailedPlayerStateMachine stateMachine)
        {
            if (stateMachine.StealthMovement != null)
            {
                stateMachine.StealthMovement.SetRunning(false);
            }
            _moveInput = Vector2.zero;
        }
        
        /// <summary>
        /// 毎フレーム呼び出されます。
        /// </summary>
        /// <param name="stateMachine">プレイヤーのステートマシン。</param>
        public void Update(DetailedPlayerStateMachine stateMachine)
        {
        }
        
        /// <summary>
        /// 固定フレームレートで呼び出されます。
        /// 入力に基づいてキャラクターの移動を処理します。
        /// </summary>
        /// <param name="stateMachine">プレイヤーのステートマシン。</param>
        public void FixedUpdate(DetailedPlayerStateMachine stateMachine)
        {
            if (stateMachine.CharacterController == null) return;

            Transform transform = stateMachine.transform;
            Vector3 moveDirection = transform.right * _moveInput.x + transform.forward * _moveInput.y;
            moveDirection.y = 0;

            if (!stateMachine.CharacterController.isGrounded)
            {
                moveDirection.y += Physics.gravity.y * Time.fixedDeltaTime;
            }

            stateMachine.CharacterController.Move(moveDirection.normalized * runSpeed * Time.fixedDeltaTime);
        }

        /// <summary>
        /// プレイヤーの入力を処理し、他の状態への遷移を判断します。
        /// </summary>
        /// <param name="stateMachine">プレイヤーのステートマシン。</param>
        /// <param name="moveInput">移動入力。</param>
        /// <param name="jumpInput">ジャンプ入力。</param>
        public void HandleInput(DetailedPlayerStateMachine stateMachine, Vector2 moveInput, bool jumpInput)
        {
            _moveInput = moveInput;

            if (jumpInput)
            {
                stateMachine.TransitionToState(PlayerStateType.Jumping);
                return;
            }

            if (moveInput.magnitude < 0.1f)
            {
                stateMachine.TransitionToState(PlayerStateType.Idle);
                return;
            }
            
            // スプリント入力制御：PlayerControllerからスプリント状態を取得
            var playerController = stateMachine.GetComponent<PlayerController>();
            if (playerController != null && !playerController.IsSprintPressed)
            {
                // スプリントが離されたら歩行状態に遷移
                stateMachine.TransitionToState(PlayerStateType.Walking);
                return;
            }
        }
    }
}
