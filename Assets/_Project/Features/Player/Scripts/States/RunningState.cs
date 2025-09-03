using UnityEngine;

namespace asterivo.Unity60.Player.States
{
    public class RunningState : IPlayerState
    {
        private float runSpeed = 7f;
        private Vector3 moveDirection;
        
        public void Enter(PlayerStateMachine stateMachine)
        {
            if (stateMachine.StealthMovement != null)
            {
                stateMachine.StealthMovement.SetStance(Core.Data.MovementStance.Standing);
            }
        }
        
        public void Exit(PlayerStateMachine stateMachine)
        {
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
                
                stateMachine.CharacterController.Move(moveDirection * runSpeed * Time.fixedDeltaTime);
            }
        }
        
        public void HandleInput(PlayerStateMachine stateMachine)
        {
            Vector2 moveInput = GetMovementInput();
            
            if (moveInput.magnitude < 0.1f)
            {
                stateMachine.TransitionToState(PlayerStateMachine.PlayerStateType.Idle);
                return;
            }
            
            if (!Input.GetKey(KeyCode.LeftShift))
            {
                stateMachine.TransitionToState(PlayerStateMachine.PlayerStateType.Walking);
            }
            
            if (Input.GetKeyDown(KeyCode.C))
            {
                stateMachine.TransitionToState(PlayerStateMachine.PlayerStateType.Crouching);
            }
        }
        
        private Vector2 GetMovementInput()
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            return new Vector2(horizontal, vertical);
        }
    }
}