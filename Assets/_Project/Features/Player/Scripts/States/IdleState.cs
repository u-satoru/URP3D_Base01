using UnityEngine;

namespace asterivo.Unity60.Player.States
{
    public class IdleState : IPlayerState
    {
        private float idleTime;
        
        public void Enter(DetailedPlayerStateMachine stateMachine)
        {
            idleTime = 0f;
            
            if (stateMachine.StealthMovement != null)
            {
                stateMachine.StealthMovement.SetStance(Core.Data.MovementStance.Standing);
            }
        }
        
        public void Exit(DetailedPlayerStateMachine stateMachine)
        {
        }
        
        public void Update(DetailedPlayerStateMachine stateMachine)
        {
            idleTime += Time.deltaTime;
        }
        
        public void FixedUpdate(DetailedPlayerStateMachine stateMachine)
        {
        }

        public void HandleInput(DetailedPlayerStateMachine stateMachine, Vector2 moveInput, bool jumpInput)
        {
            if (jumpInput)
            {
                // TODO: JumpingStateへの遷移を実装
                // stateMachine.TransitionToState(PlayerStateType.Jumping);
                return;
            }

            if (moveInput.magnitude > 0.1f)
            {
                stateMachine.TransitionToState(PlayerStateType.Walking);
            }
        }
    }
}