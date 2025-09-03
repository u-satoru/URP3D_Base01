using UnityEngine;
using asterivo.Unity60.Core.Commands;
using asterivo.Unity60.Core.Events;

namespace asterivo.Unity60.Core.Events
{
    [CreateAssetMenu(fileName = "CommandDefinitionEvent", menuName = "asterivo.Unity60/Core/Events/Command Definition Event")]
    public class CommandDefinitionGameEvent : GenericGameEvent<ICommandDefinition>
    {
    }
}