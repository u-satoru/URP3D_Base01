using UnityEngine;
using asterivo.Unity60.Core.Data;

namespace asterivo.Unity60.Core.Events
{
    /// <summary>
    /// GameData用のイベント定義
    /// </summary>
    [CreateAssetMenu(fileName = "GameDataEvent", menuName = "asterivo.Unity60/Events/Game Data Event")]
    public class GameDataEvent : GenericGameEvent<GameData> { }
    
    /// <summary>
    /// GameDataレスポンス用のイベント定義
    /// </summary>
    [CreateAssetMenu(fileName = "GameDataResponseEvent", menuName = "asterivo.Unity60/Events/Game Data Response Event")]
    public class GameDataResponseEvent : GenericGameEvent<GameData> { }
    
    /// <summary>
    /// PlayerData用のイベント定義
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerDataEvent", menuName = "asterivo.Unity60/Events/Player Data Event")]
    public class PlayerDataEvent : GenericGameEvent<PlayerDataPayload> { }
}