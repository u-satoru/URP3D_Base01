using UnityEngine;

namespace asterivo.Unity60.Player.States
{
    public class CoverState : IPlayerState
    {
        private float coverMoveSpeed = 2f;
        private Vector3 coverNormal;
        private bool isPeeking = false;
        
        public void Enter(PlayerStateMachine stateMachine)
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
        
        public void Exit(PlayerStateMachine stateMachine)
        {
            if (stateMachine.CoverSystem != null)
            {
                stateMachine.CoverSystem.ExitCover();
            }
            
            isPeeking = false;
        }
        
        public void Update(PlayerStateMachine stateMachine)
        {
        }
        
        public void FixedUpdate(PlayerStateMachine stateMachine)
        {
            if (stateMachine.CharacterController != null && stateMachine.CoverSystem != null)
            {
                Vector2 input = GetMovementInput();
                
                if (Mathf.Abs(input.x) > 0.1f)
                {
                    Transform transform = stateMachine.transform;
                    Vector3 moveDirection = transform.right * input.x;
                    moveDirection.y = 0;
                    
                    stateMachine.CharacterController.Move(moveDirection * coverMoveSpeed * Time.fixedDeltaTime);
                }
            }
        }
        
        public void HandleInput(PlayerStateMachine stateMachine)
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                stateMachine.TransitionToState(PlayerStateMachine.PlayerStateType.Idle);
            }
            
            if (Input.GetKey(KeyCode.E))
            {
                HandlePeeking(stateMachine, PeekDirection.Right);
            }
            else if (Input.GetKey(KeyCode.Q))
            {
                HandlePeeking(stateMachine, PeekDirection.Left);
            }
            else if (Input.GetKey(KeyCode.Space))
            {
                HandlePeeking(stateMachine, PeekDirection.Over);
            }
            else
            {
                if (isPeeking)
                {
                    StopPeeking(stateMachine);
                }
            }
            
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                stateMachine.TransitionToState(PlayerStateMachine.PlayerStateType.Running);
            }
        }
        
        private void HandlePeeking(PlayerStateMachine stateMachine, PeekDirection direction)
        {
            if (stateMachine.CoverSystem == null) return;
            
            switch (direction)
            {
                case PeekDirection.Left:
                    stateMachine.CoverSystem.PeekLeft();
                    break;
                case PeekDirection.Right:
                    stateMachine.CoverSystem.PeekRight();
                    break;
                case PeekDirection.Over:
                    stateMachine.CoverSystem.PeekOver();
                    break;
            }
            
            isPeeking = true;
        }
        
        private void StopPeeking(PlayerStateMachine stateMachine)
        {
            isPeeking = false;
        }
        
        private Vector2 GetMovementInput()
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            return new Vector2(horizontal, vertical);
        }
        
        private enum PeekDirection
        {
            Left,
            Right,
            Over
        }
    }
}