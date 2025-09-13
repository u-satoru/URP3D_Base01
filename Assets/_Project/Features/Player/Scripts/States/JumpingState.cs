
using UnityEngine;

namespace asterivo.Unity60.Player.States
{
    /// <summary>
    /// プレイヤーのジャンプ状態を管理するクラス。
    /// </summary>
    public class JumpingState : IPlayerState
    {
        private Vector3 _playerVelocity;
        private bool _isGrounded;
        private float _jumpHeight = 1.5f;
        private float _gravityValue = -9.81f;

        /// <summary>
        /// 状態が開始されたときに呼び出されます。
        /// プレイヤーに上方向の速度を与えてジャンプさせます。
        /// </summary>
        /// <param name="stateMachine">プレイヤーのステートマシン。</param>
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
        /// 状態が終了したときに呼び出されます。
        /// </summary>
        /// <param name="stateMachine">プレイヤーのステートマシン。</param>
        public void Exit(DetailedPlayerStateMachine stateMachine)
        {
        }

        /// <summary>
        /// 毎フレーム呼び出されます。
        /// 着地判定を行います。
        /// </summary>
        /// <param name="stateMachine">プレイヤーのステートマシン。</param>
        public void Update(DetailedPlayerStateMachine stateMachine)
        {
            _isGrounded = stateMachine.CharacterController.isGrounded;
            if (_isGrounded && _playerVelocity.y < 0)
            {
                stateMachine.TransitionToState(PlayerStateType.Idle);
            }
        }

        /// <summary>
        /// 固定フレームレートで呼び出されます。
        /// 重力を適用し、プレイヤーを移動させます。
        /// </summary>
        /// <param name="stateMachine">プレイヤーのステートマシン。</param>
        public void FixedUpdate(DetailedPlayerStateMachine stateMachine)
        {
            if (stateMachine.CharacterController == null) return;

            _playerVelocity.y += _gravityValue * Time.fixedDeltaTime;
            stateMachine.CharacterController.Move(_playerVelocity * Time.fixedDeltaTime);
        }

        /// <summary>
        /// プレイヤーの入力を処理します。（この状態では入力は無視されます）
        /// </summary>
        /// <param name="stateMachine">プレイヤーのステートマシン。</param>
        /// <param name="moveInput">移動入力。</param>
        /// <param name="jumpInput">ジャンプ入力。</param>
        public void HandleInput(DetailedPlayerStateMachine stateMachine, Vector2 moveInput, bool jumpInput)
        {
            // ジャンプ中は他の入力を受け付けない
        }
    }
}
