using UnityEngine;

namespace asterivo.Unity60.Player.States
{
    public interface IPlayerState
    {
        void Enter(DetailedPlayerStateMachine stateMachine);
        void Exit(DetailedPlayerStateMachine stateMachine);
        void Update(DetailedPlayerStateMachine stateMachine);
        void FixedUpdate(DetailedPlayerStateMachine stateMachine);
        void HandleInput(DetailedPlayerStateMachine stateMachine, Vector2 moveInput, bool jumpInput);
    }
}