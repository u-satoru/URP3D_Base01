namespace asterivo.Unity60.Core.Commands
{
    /// <summary>
    /// Interface for command definitions that can be serialized and used to create commands
    /// </summary>
    public interface ICommandDefinition
    {
        /// <summary>
        /// Checks if the command can be executed in the given context
        /// </summary>
        bool CanExecute(object context = null);
        
        /// <summary>
        /// Creates a command instance from this definition
        /// </summary>
        ICommand CreateCommand(object context = null);
    }
}