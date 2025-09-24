namespace asterivo.Unity60.Core.Commands
{
    /// <summary>
    /// Base interface for all commands in the system
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// Executes the command
        /// </summary>
        void Execute();
        void Undo();
        
        /// <summary>
        /// Indicates whether this command supports undo
        /// </summary>
        bool CanUndo { get; }

    }
}
