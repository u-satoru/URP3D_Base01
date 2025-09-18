using UnityEngine;

namespace asterivo.Unity60.Features.Templates.TPS.Player.StateMachine.States
{
    /// <summary>
    /// Aiming state - Player is aiming weapon (placeholder implementation)
    /// </summary>
    public class AimingState : BasePlayerState
    {
        public override PlayerState StateType => PlayerState.Aiming;

        public override void Enter()
        {
            // TODO: Implement aiming mechanics
            Debug.Log("[AimingState] Aiming state entered - placeholder implementation");
        }

        public override void Update()
        {
            // TODO: Implement aiming control and camera adjustment
        }

        public override void Exit()
        {
            // TODO: Implement aiming exit logic
        }
    }
}