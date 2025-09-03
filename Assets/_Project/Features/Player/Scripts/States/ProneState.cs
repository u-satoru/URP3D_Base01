using UnityEngine;

namespace asterivo.Unity60.Player.States
{
    public class ProneState : IPlayerState
    {
        private float proneSpeed = 1f;
        private Vector3 moveDirection;
        private float originalHeight;
        private float proneHeight = 0.5f;
        
        public void Enter(PlayerStateMachine stateMachine)
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
                
                stateMachine.CharacterController.Move(moveDirection * proneSpeed * Time.fixedDeltaTime);
            }
        }
        
        public void HandleInput(PlayerStateMachine stateMachine)
        {
            if (Input.GetKeyUp(KeyCode.Z) || Input.GetKeyDown(KeyCode.Z))
            {
                if (CanStandUp(stateMachine))
                {
                    Vector2 moveInput = GetMovementInput();
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
            
            if (Input.GetKeyDown(KeyCode.C))
            {
                if (CanCrouch(stateMachine))
                {
                    stateMachine.TransitionToState(PlayerStateMachine.PlayerStateType.Crouching);
                }
            }
        }
        
        private bool CanStandUp(PlayerStateMachine stateMachine)
        {
            RaycastHit hit;
            Vector3 origin = stateMachine.transform.position + Vector3.up * proneHeight;
            float checkDistance = originalHeight - proneHeight;
            
            return !Physics.SphereCast(origin, 0.3f, Vector3.up, out hit, checkDistance);
        }
        
        private bool CanCrouch(PlayerStateMachine stateMachine)
        {
            RaycastHit hit;
            Vector3 origin = stateMachine.transform.position + Vector3.up * proneHeight;
            float checkDistance = 1.2f - proneHeight;
            
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