using UnityEngine;

namespace asterivo.Unity60.Features.Player.States
{
    /// <summary>
    /// プレイヤーの歩行状態を管琁E��るクラス、E    /// </summary>
    public class WalkingState : IPlayerState
    {
        private float walkSpeed = 4f;
        private Vector2 _moveInput;

        /// <summary>
        /// 状態が開始されたときに呼び出されます、E        /// プレイヤーの姿勢を立位に設定します、E        /// </summary>
        /// <param name="stateMachine">プレイヤーのスチE�Eト�Eシン、E/param>
        public void Enter(DetailedPlayerStateMachine stateMachine)
        {
            if (stateMachine.StealthMovement != null)
            {
                stateMachine.StealthMovement.SetStance(Core.Data.MovementStance.Standing);
            }
        }

        /// <summary>
        /// 状態が終亁E��たときに呼び出されます、E        /// 移動�E力をリセチE��します、E        /// </summary>
        /// <param name="stateMachine">プレイヤーのスチE�Eト�Eシン、E/param>
        public void Exit(DetailedPlayerStateMachine stateMachine)
        {
            _moveInput = Vector2.zero;
        }

        /// <summary>
        /// 毎フレーム呼び出されます、E        /// </summary>
        /// <param name="stateMachine">プレイヤーのスチE�Eト�Eシン、E/param>
        public void Update(DetailedPlayerStateMachine stateMachine)
        {
        }

        /// <summary>
        /// 固定フレームレートで呼び出されます、E        /// 入力に基づぁE��キャラクターの移動を処琁E��ます、E        /// </summary>
        /// <param name="stateMachine">プレイヤーのスチE�Eト�Eシン、E/param>
        public void FixedUpdate(DetailedPlayerStateMachine stateMachine)
        {
            if (stateMachine.CharacterController == null) return;

            // NOTE: これは簡略化された移動実装です。
            // 本来はカメラの向きや速度、重力などを考慮する必要があります。
            Vector3 moveDirection = stateMachine.transform.right * _moveInput.x + stateMachine.transform.forward * _moveInput.y;
            moveDirection.y = 0;

            // 接地していない場合は重力を適用
            if (!stateMachine.CharacterController.isGrounded)
            {
                moveDirection.y += Physics.gravity.y * Time.fixedDeltaTime;
            }

            stateMachine.CharacterController.Move(moveDirection.normalized * walkSpeed * Time.fixedDeltaTime);
        }

        /// <summary>
        /// プレイヤーの入力を処琁E��、他�E状態への遷移を判断します、E        /// </summary>
        /// <param name="stateMachine">プレイヤーのスチE�Eト�Eシン、E/param>
        /// <param name="moveInput">移動�E力、E/param>
        /// <param name="jumpInput">ジャンプ�E力、E/param>
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
            
            // State transitions - スプリント�E力でRunning状態に遷移
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
