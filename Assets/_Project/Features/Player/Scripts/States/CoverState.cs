using UnityEngine;

namespace asterivo.Unity60.Player.States
{
    public class CoverState : IPlayerState
    {
        private float coverMoveSpeed = 2f;
        private bool isPeeking = false;
        private Vector2 _moveInput;

        public void Enter(DetailedPlayerStateMachine stateMachine)
        {
            if (stateMachine.CoverSystem != null)
            {
                stateMachine.CoverSystem.TryEnterCover();
            }

            if (stateMachine.StealthMovement != null)
            {
                stateMachine.StealthMovement.SetStance(Core.Data.MovementStance.Cover);
            }
        }

        public void Exit(DetailedPlayerStateMachine stateMachine)
        {
            if (stateMachine.CoverSystem != null)
            {
                stateMachine.CoverSystem.ExitCover();
            }
            isPeeking = false;
        }

        public void Update(DetailedPlayerStateMachine stateMachine) { }

        public void FixedUpdate(DetailedPlayerStateMachine stateMachine)
        {
            if (stateMachine.CharacterController == null || stateMachine.CoverSystem == null) return;

            // カバー中の移動ロジック
            if (Mathf.Abs(_moveInput.x) > 0.1f)
            {
                Transform transform = stateMachine.transform;
                Vector3 moveDirection = transform.right * _moveInput.x;
                moveDirection.y = 0;

                stateMachine.CharacterController.Move(moveDirection * coverMoveSpeed * Time.fixedDeltaTime);
            }
        }

        public void HandleInput(DetailedPlayerStateMachine stateMachine, Vector2 moveInput, bool jumpInput)
        {
            _moveInput = moveInput;

            // TODO: Peekやカバー解除のコマンドを実装し、それに応じて状態遷移を行う
        }
    }
}
