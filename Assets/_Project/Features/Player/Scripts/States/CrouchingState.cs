using UnityEngine;

namespace asterivo.Unity60.Player.States
{
    public class CrouchingState : IPlayerState
    {
        private float crouchSpeed = 2.5f;
        private float originalHeight;
        private float crouchHeight = 1.2f;
        private Vector2 _moveInput;

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

        public void Update(DetailedPlayerStateMachine stateMachine) { }

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

        public void HandleInput(DetailedPlayerStateMachine stateMachine, Vector2 moveInput, bool jumpInput)
        {
            _moveInput = moveInput;

            // TODO: Crouchコマンドを実装し、それに応じて状態遷移を行う必要がある
            if (jumpInput)
            {
                if (CanStandUp(stateMachine))
                {
                    // stateMachine.TransitionToState(PlayerStateType.Jumping);
                }
            }
        }

        private bool CanStandUp(DetailedPlayerStateMachine stateMachine)
        {
            RaycastHit hit;
            Vector3 origin = stateMachine.transform.position + Vector3.up * crouchHeight;
            float checkDistance = originalHeight - crouchHeight;

            return !Physics.SphereCast(origin, 0.3f, Vector3.up, out hit, checkDistance);
        }
    }
}
