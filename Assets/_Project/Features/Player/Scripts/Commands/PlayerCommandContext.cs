using UnityEngine;
using asterivo.Unity60.Player.States;

namespace asterivo.Unity60.Player.Commands
{
    /// <summary>
    /// Context object for player commands containing necessary references
    /// </summary>
    public class PlayerCommandContext
    {
        public DetailedPlayerStateMachine StateMachine { get; }
        public Transform Transform { get; }
        public CharacterController CharacterController { get; }
        
        public PlayerCommandContext(DetailedPlayerStateMachine stateMachine)
        {
            StateMachine = stateMachine;
            Transform = stateMachine?.transform;
            CharacterController = stateMachine?.GetComponent<CharacterController>();
        }
        
        public PlayerCommandContext(DetailedPlayerStateMachine stateMachine, 
                                  Transform transform, 
                                  CharacterController characterController)
        {
            StateMachine = stateMachine;
            Transform = transform;
            CharacterController = characterController;
        }
    }
}