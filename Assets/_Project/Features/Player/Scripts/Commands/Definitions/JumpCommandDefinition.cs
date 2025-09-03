using asterivo.Unity60.Core.Commands;

namespace asterivo.Unity60.Player.Commands
{
    [System.Serializable]
    public class JumpCommandDefinition : ICommandDefinition
    {
        public bool CanExecute(object context = null)
        {
            // Jump can always be executed if the player exists
            return context != null;
        }

        public ICommand CreateCommand(object context = null)
        {
            var stateMachine = context as States.DetailedPlayerStateMachine;
            if (stateMachine == null)
                return null;
                
            return new JumpCommand(stateMachine, this);
        }
    }
}
