using UnityEngine;
// using asterivo.Unity60.Core.Player; // Player moved to Features
// using asterivo.Unity60.Core.Audio; // Temporarily commented to avoid circular dependency

namespace asterivo.Unity60.Core.Events
{
    /// <summary>
    /// PlayerState用のイベント定義（enum値で通信）
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerStateEvent", menuName = "asterivo.Unity60/Events/Player State Event")]
    public class PlayerStateEvent : GameEvent<int> { } // Changed from PlayerState to int to avoid circular dependency
    
    /// <summary>
    /// PlayerStateイベントリスナー（一時的にコメントアウト - 型参照問題のため）
    /// </summary>
    // public class PlayerStateEventListener : GenericGameEventListener<PlayerState, PlayerStateEvent, UnityPlayerStateEvent> { }
    
    /// <summary>
    /// PlayerState用のUnityEvent（一時的にコメントアウト - 型参照問題のため）
    /// </summary>
    // [System.Serializable]
    // public class UnityPlayerStateEvent : UnityEngine.Events.UnityEvent<PlayerState> { }
    
    /// <summary>
    /// GameState用のイベント定義
    /// </summary>
    [CreateAssetMenu(fileName = "GameStateEvent", menuName = "asterivo.Unity60/Events/Game State Event")]
    public class GameStateEvent : GameEvent<int> { } // Using int to avoid circular dependency
    
    /// <summary>
    /// GameStateイベントリスナー（一時的にコメントアウト - 型参照問題のため）
    /// </summary>
    // public class GameStateEventListener : GenericGameEventListener<GameState, GameStateEvent, UnityGameStateEvent> { }
    
    /// <summary>
    /// GameState用のUnityEvent（一時的にコメントアウト - 型参照問題のため）
    /// </summary>
    // [System.Serializable]
    // public class UnityGameStateEvent : UnityEngine.Events.UnityEvent<GameState> { }
}
