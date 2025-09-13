using UnityEngine;

namespace asterivo.Unity60.Player.States
{
    /// <summary>
    /// プレイヤーのしゃがみ状態を管理するクラス。
    /// </summary>
    public class CrouchingState : IPlayerState
    {
        private float crouchSpeed = 2.5f;
        private float originalHeight;
        private float crouchHeight = 1.2f;
        private Vector2 _moveInput;

        /// <summary>
        /// 状態が開始されたときに呼び出されます。
        /// キャラクターの高さをしゃがみ状態に変更し、姿勢を更新します。
        /// </summary>
        /// <param name="stateMachine">プレイヤーのステートマシン。</param>
        public void Enter(DetailedPlayerStateMachine stateMachine)
        {
            if (stateMachine.CharacterController != null)
            {
                originalHeight = stateMachine.CharacterController.height;
                stateMachine.CharacterController.height = crouchHeight;

                Vector3 center = stateMachine.CharacterController.center;
                center.y = crouchHeight / 2f;
                stateMachine.CharacterController.center = center;
            }

            if (stateMachine.StealthMovement != null)
            {
                stateMachine.StealthMovement.SetStance(Core.Data.MovementStance.Crouching);
            }
        }

        /// <summary>
        /// 状態が終了したときに呼び出されます。
        /// キャラクターの高さを元の高さに戻します。
        /// </summary>
        /// <param name="stateMachine">プレイヤーのステートマシン。</param>
        public void Exit(DetailedPlayerStateMachine stateMachine)
        {
            if (stateMachine.CharacterController != null)
            {
                stateMachine.CharacterController.height = originalHeight;

                Vector3 center = stateMachine.CharacterController.center;
                center.y = originalHeight / 2f;
                stateMachine.CharacterController.center = center;
            }
        }

        /// <summary>
        /// 毎フレーム呼び出されます。
        /// </summary>
        /// <param name="stateMachine">プレイヤーのステートマシン。</param>
        public void Update(DetailedPlayerStateMachine stateMachine) { }

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

            stateMachine.CharacterController.Move(moveDirection.normalized * crouchSpeed * Time.fixedDeltaTime);
        }

        /// <summary>
        /// プレイヤーの入力を処理し、他の状態への遷移を判断します。
        /// </summary>
        /// <param name="stateMachine">プレイヤーのステートマシン。</param>
        /// <param name="moveInput">移動入力。</param>
        /// <param name="jumpInput">ジャンプ入力（この状態では立ち上がりに使用）。</param>
        public void HandleInput(DetailedPlayerStateMachine stateMachine, Vector2 moveInput, bool jumpInput)
        {
            _moveInput = moveInput;

            // ジャンプ入力でしゃがみ解除（立ち上がり）
            if (jumpInput)
            {
                if (CanStandUp(stateMachine))
                {
                    // 立ち上がりが可能な場合、移動入力に応じて適切な状態に遷移
                    if (moveInput.magnitude > 0.1f)
                    {
                        stateMachine.TransitionToState(PlayerStateType.Walking);
                    }
                    else
                    {
                        stateMachine.TransitionToState(PlayerStateType.Idle);
                    }
                    return;
                }
            }
            
            // しゃがみ状態での移動は低速移動として継続
            // 他の姿勢への遷移（匍匐など）
            if (Input.GetKeyDown(KeyCode.X))
            {
                stateMachine.TransitionToState(PlayerStateType.Prone);
            }
        }

        /// <summary>
        /// プレイヤーが立ち上がれるかどうか（頭上に障害物がないか）を確認します。
        /// </summary>
        /// <param name="stateMachine">プレイヤーのステートマシン。</param>
        /// <returns>立ち上がれる場合はtrue、そうでない場合はfalse。</returns>
        private bool CanStandUp(DetailedPlayerStateMachine stateMachine)
        {
            RaycastHit hit;
            Vector3 origin = stateMachine.transform.position + Vector3.up * crouchHeight;
            float checkDistance = originalHeight - crouchHeight;

            return !Physics.SphereCast(origin, 0.3f, Vector3.up, out hit, checkDistance);
        }
    }
}
