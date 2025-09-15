using UnityEngine;
// using asterivo.Unity60.Core.Commands; // Removed to avoid circular dependency
using asterivo.Unity60.Core.Events;

namespace asterivo.Unity60.Core.Events
{
    [CreateAssetMenu(fileName = "CommandDefinitionEvent", menuName = "asterivo.Unity60/Core/Events/Command Definition Event")]
    public class CommandDefinitionGameEvent : GameEvent<object> // Changed from ICommandDefinition to avoid circular dependency
    {
    }
}