using UnityEngine;
using Unity6.Core.Player;

namespace Unity6.Core.Events
{
    /// <summary>
    /// PlayerState用のイベント定義（enum値で通信）
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerStateEvent", menuName = "Unity6/Events/Player State Event")]
    public class PlayerStateEvent : GenericGameEvent<PlayerState> { }
    
    /// <summary>
    /// PlayerStateイベントリスナー
    /// </summary>
    public class PlayerStateEventListener : GenericGameEventListener<PlayerState, PlayerStateEvent, UnityPlayerStateEvent> { }
    
    /// <summary>
    /// PlayerState用のUnityEvent
    /// </summary>
    [System.Serializable]
    public class UnityPlayerStateEvent : UnityEngine.Events.UnityEvent<PlayerState> { }
    
    /// <summary>
    /// GameState用のイベント定義
    /// </summary>
    [CreateAssetMenu(fileName = "GameStateEvent", menuName = "Unity6/Events/Game State Event")]
    public class GameStateEvent : GenericGameEvent<GameState> { }
    
    /// <summary>
    /// GameStateイベントリスナー
    /// </summary>
    public class GameStateEventListener : GenericGameEventListener<GameState, GameStateEvent, UnityGameStateEvent> { }
    
    /// <summary>
    /// GameState用のUnityEvent
    /// </summary>
    [System.Serializable]
    public class UnityGameStateEvent : UnityEngine.Events.UnityEvent<GameState> { }
}