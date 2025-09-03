using UnityEngine;

namespace asterivo.Unity60.Player.States
{
    public class RunningState : IPlayerState
    {
        private float runSpeed = 7f;
        private Vector2 _moveInput;
        
        public void Enter(DetailedPlayerStateMachine stateMachine)
        {
            if (stateMachine.StealthMovement != null)
            {
                stateMachine.StealthMovement.SetStance(Core.Data.MovementStance.Standing);
                stateMachine.StealthMovement.SetRunning(true);
            }
        }
        
        public void Exit(DetailedPlayerStateMachine stateMachine)
        {
            if (stateMachine.StealthMovement != null)
            {
                stateMachine.StealthMovement.SetRunning(false);
            }
            _moveInput = Vector2.zero;
        }
        
        public void Update(DetailedPlayerStateMachine stateMachine)
        {
        }
        
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