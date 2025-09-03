using UnityEngine;

namespace asterivo.Unity60.Player.States
{
    public class ProneState : IPlayerState
    {
        private float proneSpeed = 1f;
        private float originalHeight;
        private float proneHeight = 0.5f;
        private Vector2 _moveInput;

        public void Enter(DetailedPlayerStateMachine stateMachine)
        {
            if (stateMachine.CharacterController != null)
            {
                originalHeight = stateMachine.CharacterController.height;
                stateMachine.CharacterController.height = proneHeight;

                Vector3 center = stateMachine.CharacterController.center;
                center.y = proneHeight / 2f;
                stateMachine.CharacterController.center = center;
            }

            if (stateMachine.StealthMovement != null)
            {
                stateMachine.StealthMovement.SetStance(Core.Data.MovementStance.Prone);
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

            stateMachine.CharacterController.Move(moveDirection.normalized * proneSpeed * Time.fixedDeltaTime);
        }

        public void HandleInput(DetailedPlayerStateMachine stateMachine, Vector2 moveInput, bool jumpInput)
        {
            _moveInput = moveInput;
            
            // ジャンプ入力で匍匐解除（立ち上がり）
            if (jumpInput)
            {
                if (CanStandUp(stateMachine))
                {
                    // 直接立ち上がりが可能な場合
                    if (moveInput.magnitude > 0.1f)
                    {
                        stateMachine.TransitionToState(PlayerStateType.Walking);
                    }
                    else
                    {
                        stateMachine.TransitionToState(PlayerStateType.Idle);
                    }
                }
                else
                {
                    // 障害物がある場合はしゃがみ状態へ
                    stateMachine.TransitionToState(PlayerStateType.Crouching);
                }
                return;
            }
            
            // 匍匐状態からしゃがみへの遷移（段階的な立ち上がり）
            if (Input.GetKeyDown(KeyCode.C))
            {
                stateMachine.TransitionToState(PlayerStateType.Crouching);
            }
        }

        private bool CanStandUp(DetailedPlayerStateMachine stateMachine)
        {
            RaycastHit hit;
            Vector3 origin = stateMachine.transform.position + Vector3.up * proneHeight;
            float checkDistance = originalHeight - proneHeight;

            return !Physics.SphereCast(origin, 0.3f, Vector3.up, out hit, checkDistance);
        }

        private bool CanCrouch(DetailedPlayerStateMachine stateMachine)
        {
            RaycastHit hit;
            Vector3 origin = stateMachine.transform.position + Vector3.up * proneHeight;
            float checkDistance = 1.2f - proneHeight; // Assuming crouch height is 1.2f

            return !Physics.SphereCast(origin, 0.3f, Vector3.up, out hit, checkDistance);
        }
    }
}
