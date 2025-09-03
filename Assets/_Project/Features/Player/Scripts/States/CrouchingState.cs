using UnityEngine;

namespace asterivo.Unity60.Player.States
{
    public class CrouchingState : IPlayerState
    {
        private float crouchSpeed = 2.5f;
        private Vector3 moveDirection;
        private float originalHeight;
        private float crouchHeight = 1.2f;
        
        public void Enter(PlayerStateMachine stateMachine)
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
        
        public void Exit(PlayerStateMachine stateMachine)
        {
            if (stateMachine.CharacterController != null)
            {
                stateMachine.CharacterController.height = originalHeight;
                
                Vector3 center = stateMachine.CharacterController.center;
                center.y = originalHeight / 2f;
                stateMachine.CharacterController.center = center;
            }
            
            moveDirection = Vector3.zero;
        }
        
        public void Update(PlayerStateMachine stateMachine)
        {
        }
        
        public void FixedUpdate(PlayerStateMachine stateMachine)
        {
            if (stateMachine.CharacterController != null)
            {
                Vector2 input = GetMovementInput();
                Transform transform = stateMachine.transform;
                
                moveDirection = transform.right * input.x + transform.forward * input.y;
                moveDirection.y = 0;
                moveDirection.Normalize();
                
                if (!stateMachine.CharacterController.isGrounded)
                {
                    moveDirection.y = -9.81f * Time.fixedDeltaTime;
                }
                
                stateMachine.CharacterController.Move(moveDirection * crouchSpeed * Time.fixedDeltaTime);
            }
        }
        
        public void HandleInput(PlayerStateMachine stateMachine)
        {
            Vector2 moveInput = GetMovementInput();
            
            if (Input.GetKeyUp(KeyCode.C) || Input.GetKeyDown(KeyCode.C))
            {
                if (CanStandUp(stateMachine))
                {
                    if (moveInput.magnitude > 0.1f)
                    {
                        stateMachine.TransitionToState(PlayerStateMachine.PlayerStateType.Walking);
                    }
                    else
                    {
                        stateMachine.TransitionToState(PlayerStateMachine.PlayerStateType.Idle);
                    }
                }
            }
            
            if (Input.GetKeyDown(KeyCode.Z))
            {
                stateMachine.TransitionToState(PlayerStateMachine.PlayerStateType.Prone);
            }
            
            if (Input.GetKeyDown(KeyCode.Q) && stateMachine.CoverSystem != null)
            {
                if (stateMachine.CoverSystem.GetAvailableCovers().Count > 0)
                {
                    stateMachine.TransitionToState(PlayerStateMachine.PlayerStateType.InCover);
                }
            }
        }
        
        private bool CanStandUp(PlayerStateMachine stateMachine)
        {
            RaycastHit hit;
            Vector3 origin = stateMachine.transform.position + Vector3.up * crouchHeight;
            float checkDistance = originalHeight - crouchHeight;
            
            return !Physics.SphereCast(origin, 0.3f, Vector3.up, out hit, checkDistance);
        }
        
        private Vector2 GetMovementInput()
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            return new Vector2(horizontal, vertical);
        }
    }
}