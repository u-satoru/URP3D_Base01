using UnityEngine;
// using asterivo.Unity60.Core.Data;

namespace asterivo.Unity60.Core.Events
{
    [CreateAssetMenu(menuName = "asterivo.Unity60/Core/Events/Stealth/Detection Event", fileName = "SE_DetectionEvent")]
    public class DetectionEvent : GameEvent<DetectionInfo> { }
    
    [CreateAssetMenu(menuName = "asterivo.Unity60/Core/Events/Stealth/Player Detected Event", fileName = "SE_PlayerDetectedEvent")]
    public class PlayerDetectedEvent : GameEvent { }
    
    [CreateAssetMenu(menuName = "asterivo.Unity60/Core/Events/Stealth/Player Lost Event", fileName = "SE_PlayerLostEvent")]
    public class PlayerLostEvent : GameEvent { }
    
    [CreateAssetMenu(menuName = "asterivo.Unity60/Core/Events/Stealth/Stealth Kill Event", fileName = "SE_StealthKillEvent")]
    public class StealthKillEvent : GameEvent { }
    
    public interface IDetectionListener : IGameEventListener<DetectionInfo> { }
}
