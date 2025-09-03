using UnityEngine;

namespace asterivo.Unity60.Player.States
{
    public class IdleState : IPlayerState
    {
        private float idleTime;
        
        public void Enter(PlayerStateMachine stateMachine)
        {
            idleTime = 0f;
            
            if (stateMachine.StealthMovement != null)
            {
                stateMachine.StealthMovement.SetStance(Core.Data.MovementStance.Standing);
            }
        }
        
        public void Exit(PlayerStateMachine stateMachine)
        {
        }
        
        public void Update(PlayerStateMachine stateMachine)
        {
            idleTime += Time.deltaTime;
        }
        
        public void FixedUpdate(PlayerStateMachine stateMachine)
        {
        }
        
        public void HandleInput(PlayerStateMachine stateMachine)
        {
            Vector2 moveInput = GetMovementInput();
            
            if (moveInput.magnitude > 0.1f)
            {
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    stateMachine.TransitionToState(PlayerStateMachine.PlayerStateType.Running);
                }
                else if (Input.GetKey(KeyCode.C))
                {
                    stateMachine.TransitionToState(PlayerStateMachine.PlayerStateType.Crouching);
                }
                else
                {
                    stateMachine.TransitionToState(PlayerStateMachine.PlayerStateType.Walking);
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
        
        private Vector2 GetMovementInput()
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            return new Vector2(horizontal, vertical);
        }
    }
}