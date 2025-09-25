using UnityEngine;

namespace asterivo.Unity60.Features.Player.States
{
    /// <summary>
    /// プレイヤーのしゃがみ状態を管琁E��るクラス、E    /// </summary>
    public class CrouchingState : IPlayerState
    {
        private float crouchSpeed = 2.5f;
        private float originalHeight;
        private float crouchHeight = 1.2f;
        private Vector2 _moveInput;

        /// <summary>
        /// 状態が開始されたときに呼び出されます、E        /// キャラクターの高さをしめE��み状態に変更し、姿勢を更新します、E        /// </summary>
        /// <param name="stateMachine">プレイヤーのスチE�Eト�Eシン、E/param>
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
        /// 状態が終亁E��たときに呼び出されます、E        /// キャラクターの高さを�Eの高さに戻します、E        /// </summary>
        /// <param name="stateMachine">プレイヤーのスチE�Eト�Eシン、E/param>
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
        /// 毎フレーム呼び出されます、E        /// </summary>
        /// <param name="stateMachine">プレイヤーのスチE�Eト�Eシン、E/param>
        public void Update(DetailedPlayerStateMachine stateMachine) { }

        /// <summary>
        /// 固定フレームレートで呼び出されます、E        /// 入力に基づぁE��キャラクターの移動を処琁E��ます、E        /// </summary>
        /// <param name="stateMachine">プレイヤーのスチE�Eト�Eシン、E/param>
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
        /// プレイヤーの入力を処琁E��、他�E状態への遷移を判断します、E        /// </summary>
        /// <param name="stateMachine">プレイヤーのスチE�Eト�Eシン、E/param>
        /// <param name="moveInput">移動�E力、E/param>
        /// <param name="jumpInput">ジャンプ�E力（この状態では立ち上がりに使用�E�、E/param>
        public void HandleInput(DetailedPlayerStateMachine stateMachine, Vector2 moveInput, bool jumpInput)
        {
            _moveInput = moveInput;

            // ジャンプ�E力でしゃがみ解除�E�立ち上がり！E            if (jumpInput)
            {
                if (CanStandUp(stateMachine))
                {
                    // 立ち上がりが可能な場合、移動�E力に応じて適刁E��状態に遷移
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
            
            // しゃがみ状態での移動�E低速移動として継綁E            // 他�E姿勢への遷移�E�匍匐など�E�E            if (Input.GetKeyDown(KeyCode.X))
            {
                stateMachine.TransitionToState(PlayerStateType.Prone);
            }
        }

        /// <summary>
        /// プレイヤーが立ち上がれるかどぁE���E�頭上に障害物がなぁE���E�を確認します、E        /// </summary>
        /// <param name="stateMachine">プレイヤーのスチE�Eト�Eシン、E/param>
        /// <returns>立ち上がれる場合�Etrue、そぁE��なぁE��合�Efalse、E/returns>
        private bool CanStandUp(DetailedPlayerStateMachine stateMachine)
        {
            RaycastHit hit;
            Vector3 origin = stateMachine.transform.position + Vector3.up * crouchHeight;
            float checkDistance = originalHeight - crouchHeight;

            return !Physics.SphereCast(origin, 0.3f, Vector3.up, out hit, checkDistance);
        }
    }
}
