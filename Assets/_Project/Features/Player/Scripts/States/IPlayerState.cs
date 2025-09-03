using UnityEngine;

namespace asterivo.Unity60.Player.States
{
    public interface IPlayerState
    {
        void Enter(PlayerStateMachine stateMachine);
        void Exit(PlayerStateMachine stateMachine);
        void Update(PlayerStateMachine stateMachine);
        void FixedUpdate(PlayerStateMachine stateMachine);
        void HandleInput(PlayerStateMachine stateMachine);
    }
}