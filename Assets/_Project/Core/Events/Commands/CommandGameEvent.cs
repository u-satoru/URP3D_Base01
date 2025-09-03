using UnityEngine;
using asterivo.Unity60.Core.Commands;

namespace asterivo.Unity60.Core.Events
{
    /// <summary>
    /// ScriptableObject event for transmitting commands through the event system
    /// </summary>
    [CreateAssetMenu(fileName = "CommandEvent", menuName = "asterivo.Unity60/Events/Command Event")]
    public class CommandGameEvent : GenericGameEvent<ICommand>
    {
        [Header("Command Event Settings")]
        [SerializeField] private bool logCommandExecution = false;
        
        public new void Raise(ICommand value)
        {
            if (logCommandExecution && value != null)
            {
                Debug.Log($"[CommandEvent] Raising command: {value.GetType().Name}");
            }
            
            base.Raise(value);
        }
    }
}