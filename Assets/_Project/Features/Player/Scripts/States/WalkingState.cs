using UnityEngine;

namespace asterivo.Unity60.Player.States
{
    /// <summary>
    /// プレイヤーの歩行状態を管理するクラス。
    /// </summary>
    public class WalkingState : IPlayerState
    {
        private float walkSpeed = 4f;
        private Vector2 _moveInput;

        /// <summary>
        /// 状態が開始されたときに呼び出されます。
        /// プレイヤーの姿勢を立位に設定します。
        /// </summary>
        /// <param name="stateMachine">プレイヤーのステートマシン。</param>
        public void Enter(DetailedPlayerStateMachine stateMachine)
        {
            if (stateMachine.StealthMovement != null)
            {
                stateMachine.StealthMovement.SetStance(Core.Data.MovementStance.Standing);
            }
        }

        /// <summary>
        /// 状態が終了したときに呼び出されます。
        /// 移動入力をリセットします。
        /// </summary>
        /// <param name="stateMachine">プレイヤーのステートマシン。</param>
        public void Exit(DetailedPlayerStateMachine stateMachine)
        {
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

            // NOTE: これは簡略化された移動実装です。
            // 本来はカメラの向きや加速度、重力などを考慮する必要があります。
            Transform transform = stateMachine.transform;
            Vector3 moveDirection = transform.right * _moveInput.x + transform.forward * _moveInput.y;
            moveDirection.y = 0;

            // 接地していない場合は重力を適用
            if (!stateMachine.CharacterController.isGrounded)
            {
                moveDirection.y += Physics.gravity.y * Time.fixedDeltaTime;
            }

            stateMachine.CharacterController.Move(moveDirection.normalized * walkSpeed * Time.fixedDeltaTime);
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
            
            // State transitions - スプリント入力でRunning状態に遷移
            var playerController = stateMachine.GetComponent<PlayerController>();
            if (playerController != null && playerController.IsSprintPressed)
            {
                stateMachine.TransitionToState(PlayerStateType.Running);
                return;
            }
            else if (Input.GetKeyDown(KeyCode.C))
            {
                stateMachine.TransitionToState(PlayerStateType.Crouching);
            }
            else if (Input.GetKeyDown(KeyCode.Z))
            {
                stateMachine.TransitionToState(PlayerStateType.Prone);
            }
            
            if (Input.GetKeyDown(KeyCode.Q) && stateMachine.CoverSystem != null)
            {
                if (stateMachine.CoverSystem.GetAvailableCovers().Count > 0)
                {
                    stateMachine.TransitionToState(PlayerStateType.InCover);
                }
            }
        }
    }
}
