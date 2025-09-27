using UnityEngine;

namespace asterivo.Unity60.Features.Templates.TPS.Player.StateMachine.States
{
    /// <summary>
    /// Rolling state - Evasive roll maneuver (placeholder implementation)
    /// </summary>
    public class RollingState : BasePlayerState
    {
        public override PlayerState StateType => PlayerState.Rolling;

        public override void Enter()
        {
            // TODO: Implement rolling mechanics
            Debug.Log("[RollingState] Rolling state entered - placeholder implementation");
        }

        public override void Update()
        {
            // TODO: Implement rolling movement and animation
        }

        public override void Exit()
        {
            // TODO: Implement rolling exit logic
        }
    }
}
