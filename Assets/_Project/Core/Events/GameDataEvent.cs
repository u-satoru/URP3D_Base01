using UnityEngine;

namespace asterivo.Unity60.Core.Events
{
    /// <summary>
    /// GameData用のイベント定義
    /// </summary>
    [CreateAssetMenu(fileName = "GameDataEvent", menuName = "asterivo.Unity60/Events/Game Data Event")]
    public class GameDataEvent : GameEvent<GameData> { }
    
    /// <summary>
    /// GameDataレスポンス用のイベント定義
    /// </summary>
    [CreateAssetMenu(fileName = "GameDataResponseEvent", menuName = "asterivo.Unity60/Events/Game Data Response Event")]
    public class GameDataResponseEvent : GameEvent<GameData> { }
    
    /// <summary>
    /// PlayerData用のイベント定義
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerDataEvent", menuName = "asterivo.Unity60/Events/Player Data Event")]
    public class PlayerDataEvent : GameEvent<PlayerDataPayload> { }
}