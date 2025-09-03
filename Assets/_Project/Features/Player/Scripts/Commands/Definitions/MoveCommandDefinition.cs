using UnityEngine;
using asterivo.Unity60.Core.Commands;

namespace asterivo.Unity60.Player.Commands
{
    [System.Serializable]
    public class MoveCommandDefinition : ICommandDefinition
    {
        public Vector2 Direction;

        public MoveCommandDefinition(Vector2 direction)
        {
            Direction = direction;
        }

        public bool CanExecute(object context = null)
        {
            // Move can always be executed if the player exists
            return context != null;
        }

        public ICommand CreateCommand(object context = null)
        {
            var stateMachine = context as States.DetailedPlayerStateMachine;
            if (stateMachine == null)
                return null;
                
            return new MoveCommand(stateMachine, this);
        }
    }
}
