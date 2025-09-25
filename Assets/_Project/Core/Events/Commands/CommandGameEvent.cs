using UnityEngine;
// // using asterivo.Unity60.Core.Commands; // Removed to avoid circular dependency

namespace asterivo.Unity60.Core.Events
{
    /// <summary>
    /// ScriptableObject event for transmitting commands through the event system
    /// </summary>
    [CreateAssetMenu(fileName = "CommandEvent", menuName = "asterivo.Unity60/Events/Command Event")]
    public class CommandGameEvent : GameEvent<object> // Changed from ICommand to avoid circular dependency
    {
        [Header("Command Event Settings")]
        [SerializeField] private bool logCommandExecution = false;
        
        // Temporarily commented out to avoid circular dependency with ICommand
        /*
        public new void Raise(ICommand value)
        {
            if (logCommandExecution && value != null)
            {
                UnityEngine.Debug.Log($"[CommandEvent] Raising command: {value.GetType().Name}");
            }
            
            base.Raise(value);
        }
        */
    }
}
