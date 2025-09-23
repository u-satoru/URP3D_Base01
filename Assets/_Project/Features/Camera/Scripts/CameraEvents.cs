using UnityEngine;
using asterivo.Unity60.Core.Events;

namespace asterivo.Unity60.Features.Camera
{
    /// <summary>
    /// カメラの状態
    /// </summary>
    public enum CameraState
    {
        Follow,     // プレイヤー追従
        Aim,        // エイムモード
        Combat,     // 戦闘モード
        Dialogue,   // 会話モード
        Cutscene,   // カットシーン
        Free,       // 自由カメラ
        Fixed,      // 固定カメラ
        Orbit       // オービットカメラ
    }
    
    /// <summary>
    /// カメラ状態変更イベント
    /// </summary>
    [CreateAssetMenu(fileName = "CameraStateEvent", menuName = "asterivo.Unity60/Events/Camera State Event")]
    public class CameraStateEvent : GameEvent<CameraState> { }
    
    /// <summary>
    /// カメラ状態イベントリスナーインターフェース
    /// </summary>
    public interface ICameraStateEventListener : IGameEventListener<CameraState> { }
    
    /// <summary>
    /// Vector2入力イベント（ルック入力用）
    /// </summary>
    [CreateAssetMenu(fileName = "Vector2GameEvent", menuName = "asterivo.Unity60/Events/Vector2 Game Event")]
    public class Vector2GameEvent : GameEvent<Vector2> { }
    
    /// <summary>
    /// Vector2イベントリスナーインターフェース
    /// </summary>
    public interface IVector2GameEventListener : IGameEventListener<Vector2> { }
}