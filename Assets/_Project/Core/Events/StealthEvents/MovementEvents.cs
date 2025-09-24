using UnityEngine;
// using asterivo.Unity60.Core.Data;

namespace asterivo.Unity60.Core.Events
{
    [CreateAssetMenu(menuName = "asterivo.Unity60/Core/Events/Stealth/Movement Stance Event", fileName = "SE_MovementStanceEvent")]
    public class MovementStanceEvent : GameEvent<MovementStance> { }
    
    [CreateAssetMenu(menuName = "asterivo.Unity60/Core/Events/Stealth/Movement Info Event", fileName = "SE_MovementInfoEvent")]
    public class MovementInfoEvent : GameEvent<StealthMovementInfo> { }
    
    [CreateAssetMenu(menuName = "asterivo.Unity60/Core/Events/Stealth/Cover Enter Event", fileName = "SE_CoverEnterEvent")]
    public class CoverEnterEvent : GameEvent { }
    
    [CreateAssetMenu(menuName = "asterivo.Unity60/Core/Events/Stealth/Cover Exit Event", fileName = "SE_CoverExitEvent")]
    public class CoverExitEvent : GameEvent { }
    
    public interface IMovementStanceListener : IGameEventListener<MovementStance> { }
    public interface IMovementInfoListener : IGameEventListener<StealthMovementInfo> { }
}