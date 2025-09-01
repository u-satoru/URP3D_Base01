using UnityEngine;
using UnityEngine.Events;

namespace Unity6.Core.Events
{
    /// <summary>
    /// カメラ状態の列挙型
    /// </summary>
    public enum CameraState
    {
        Follow,          // 通常の追従カメラ
        Aim,             // エイム時のカメラ
        Combat,          // 戦闘時のカメラ
        Cinematic,       // シネマティックカメラ
        Dead,            // 死亡時のカメラ
        Menu,            // メニュー時のカメラ
        Cutscene,        // カットシーン
        FreeLook,        // フリールック
        FirstPerson,     // 一人称視点
        ThirdPerson      // 三人称視点
    }
    
    /// <summary>
    /// カメラ状態変更イベント
    /// </summary>
    [CreateAssetMenu(fileName = "New Camera State Event", menuName = "Unity6/Events/Camera State Event")]
    public class CameraStateEvent : GenericGameEvent<CameraState>
    {
        #if UNITY_EDITOR
        [Header("Camera State Event Settings")]
        [SerializeField, TextArea(2, 4)] 
        private string cameraDescription = "カメラ状態変更イベント";
        #endif
    }
    
    /// <summary>
    /// カメラ状態イベントリスナー
    /// </summary>
    public class CameraStateEventListener : GenericGameEventListener<CameraState, CameraStateEvent, UnityCameraStateEvent> { }
    
    /// <summary>
    /// カメラ状態用UnityEvent
    /// </summary>
    [System.Serializable]
    public class UnityCameraStateEvent : UnityEvent<CameraState> { }
}
