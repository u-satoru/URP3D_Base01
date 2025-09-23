using UnityEngine;
// using asterivo.Unity60.Core.Data;

namespace asterivo.Unity60.Core.Events
{
    [CreateAssetMenu(menuName = "asterivo.Unity60/Core/Events/Stealth/Alert Level Event", fileName = "SE_AlertLevelEvent")]
    public class AlertLevelEvent : GameEvent<AlertLevel> { }
    
    [CreateAssetMenu(menuName = "asterivo.Unity60/Core/Events/Stealth/Alert State Event", fileName = "SE_AlertStateEvent")]
    public class AlertStateEvent : GameEvent<AlertStateInfo> { }
    
    public interface IAlertLevelListener : IGameEventListener<AlertLevel> { }
    public interface IAlertStateListener : IGameEventListener<AlertStateInfo> { }
}