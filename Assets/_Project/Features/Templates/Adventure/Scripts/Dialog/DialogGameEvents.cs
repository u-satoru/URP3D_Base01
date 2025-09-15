using UnityEngine;
using asterivo.Unity60.Core.Events;

namespace asterivo.Unity60.Features.Templates.Adventure.Dialog
{
    /// <summary>
    /// Dialog started event - carries information about dialog initiation
    /// </summary>
    [CreateAssetMenu(fileName = "DialogStartedEvent", menuName = "Adventure Template/Events/Dialog Started Event")]
    public class DialogStartedGameEvent : GameEvent<DialogStartedEventData> { }

    /// <summary>
    /// Dialog ended event - carries information about dialog completion
    /// </summary>
    [CreateAssetMenu(fileName = "DialogEndedEvent", menuName = "Adventure Template/Events/Dialog Ended Event")]
    public class DialogEndedGameEvent : GameEvent<DialogEndedEventData> { }

    /// <summary>
    /// Dialog choice made event - carries information about user choice selection
    /// </summary>
    [CreateAssetMenu(fileName = "DialogChoiceMadeEvent", menuName = "Adventure Template/Events/Dialog Choice Made Event")]
    public class DialogChoiceMadeGameEvent : GameEvent<DialogChoiceMadeEventData> { }

    /// <summary>
    /// Dialog node changed event - carries information about dialog progression
    /// </summary>
    [CreateAssetMenu(fileName = "DialogNodeChangedEvent", menuName = "Adventure Template/Events/Dialog Node Changed Event")]
    public class DialogNodeChangedGameEvent : GameEvent<DialogNodeChangedEventData> { }
}