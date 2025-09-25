
using UnityEngine;

namespace asterivo.Unity60.Features.Player.States
{
    /// <summary>
    /// プレイヤーのジャンプ状態を管琁E��るクラス、E    /// </summary>
    public class JumpingState : IPlayerState
    {
        private Vector3 _playerVelocity;
        private bool _isGrounded;
        private float _jumpHeight = 1.5f;
        private float _gravityValue = -9.81f;

        /// <summary>
        /// 状態が開始されたときに呼び出されます、E        /// プレイヤーに上方向�E速度を与えてジャンプさせます、E        /// </summary>
        /// <param name="stateMachine">プレイヤーのスチE�Eト�Eシン、E/param>
        public void Enter(DetailedPlayerStateMachine stateMachine)
        {
            _isGrounded = false;
            _playerVelocity.y += Mathf.Sqrt(_jumpHeight * -3.0f * _gravityValue);
            
            if (stateMachine.StealthMovement != null)
            {
                stateMachine.StealthMovement.SetStance(Core.Data.MovementStance.Standing);
            }
        }

        /// <summary>
        /// 状態が終亁E��たときに呼び出されます、E        /// </summary>
        /// <param name="stateMachine">プレイヤーのスチE�Eト�Eシン、E/param>
        public void Exit(DetailedPlayerStateMachine stateMachine)
        {
        }

        /// <summary>
        /// 毎フレーム呼び出されます、E        /// 着地判定を行います、E        /// </summary>
        /// <param name="stateMachine">プレイヤーのスチE�Eト�Eシン、E/param>
        public void Update(DetailedPlayerStateMachine stateMachine)
        {
            _isGrounded = stateMachine.CharacterController.isGrounded;
            if (_isGrounded && _playerVelocity.y < 0)
            {
                stateMachine.TransitionToState(PlayerStateType.Idle);
            }
        }

        /// <summary>
        /// 固定フレームレートで呼び出されます、E        /// 重力を適用し、�Eレイヤーを移動させます、E        /// </summary>
        /// <param name="stateMachine">プレイヤーのスチE�Eト�Eシン、E/param>
        public void FixedUpdate(DetailedPlayerStateMachine stateMachine)
        {
            if (stateMachine.CharacterController == null) return;

            _playerVelocity.y += _gravityValue * Time.fixedDeltaTime;
            stateMachine.CharacterController.Move(_playerVelocity * Time.fixedDeltaTime);
        }

        /// <summary>
        /// プレイヤーの入力を処琁E��ます。（この状態では入力�E無視されます！E        /// </summary>
        /// <param name="stateMachine">プレイヤーのスチE�Eト�Eシン、E/param>
        /// <param name="moveInput">移動�E力、E/param>
        /// <param name="jumpInput">ジャンプ�E力、E/param>
        public void HandleInput(DetailedPlayerStateMachine stateMachine, Vector2 moveInput, bool jumpInput)
        {
            // ジャンプ中は他の入力を受け付けない
        }
    }
}
