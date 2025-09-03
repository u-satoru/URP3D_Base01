using UnityEngine;

namespace asterivo.Unity60.Player.States
{
    public class WalkingState : IPlayerState
    {
        private float walkSpeed = 4f;
        private Vector2 _moveInput;

        public void Enter(DetailedPlayerStateMachine stateMachine)
        {
            if (stateMachine.StealthMovement != null)
            {
                stateMachine.StealthMovement.SetStance(Core.Data.MovementStance.Standing);
            }
        }

        public void Exit(DetailedPlayerStateMachine stateMachine)
        {
            _moveInput = Vector2.zero;
        }

        public void Update(DetailedPlayerStateMachine stateMachine)
        {
        }

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