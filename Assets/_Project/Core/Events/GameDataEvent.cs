using UnityEngine;
using Unity6.Core.Data;

namespace Unity6.Core.Events
{
    /// <summary>
    /// GameData用のイベント定義
    /// </summary>
    [CreateAssetMenu(fileName = "GameDataEvent", menuName = "Unity6/Events/Game Data Event")]
    public class GameDataEvent : GenericGameEvent<GameData> { }
    
    /// <summary>
    /// GameDataレスポンス用のイベント定義
    /// </summary>
    [CreateAssetMenu(fileName = "GameDataResponseEvent", menuName = "Unity6/Events/Game Data Response Event")]
    public class GameDataResponseEvent : GenericGameEvent<GameData> { }
    
    /// <summary>
    /// PlayerData用のイベント定義
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerDataEvent", menuName = "Unity6/Events/Player Data Event")]
    public class PlayerDataEvent : GenericGameEvent<PlayerDataPayload> { }
}