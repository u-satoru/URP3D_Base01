using UnityEngine;
using UnityEngine.Events;

namespace Unity6.Core.Events
{
    /// <summary>
    /// Vector2パラメータ付きイベントリスナー
    /// カメラルック入力用
    /// </summary>
    public class Vector2GameEventListener : MonoBehaviour, IGameEventListener<Vector2>
    {
        [Header("Event Settings")]
        [SerializeField] private Vector2GameEvent gameEvent;
        [SerializeField] private int priority = 0;
        
        [Header("Response")]
        [SerializeField] private Vector2UnityEvent response;
        
        [Header("Debug")]
        [SerializeField] private bool debugMode = false;
        
        public int Priority => priority;
        public Vector2GameEvent GameEvent => gameEvent;
        public Vector2UnityEvent Response => response;
        
        private void OnEnable()
        {
            if (gameEvent != null)
            {
                gameEvent.RegisterListener(this);
            }
            else if (debugMode)
            {
                Debug.LogWarning($"[{name}] GameEvent is not assigned!", this);
            }
        }
        
        private void OnDisable()
        {
            if (gameEvent != null)
            {
                gameEvent.UnregisterListener(this);
            }
        }
        
        /// <summary>
        /// Vector2GameEventが発生した時の処理
        /// </summary>
        /// <param name="value">Vector2値</param>
        public void OnEventRaised(Vector2 value)
        {
            // メソッドが設定されている場合のみ実行
            if (Response != null)
            {
                Response.Invoke(value);
            }
        }
        
        /// <summary>
        /// 手動でイベントを発行（テスト用）
        /// </summary>
        [ContextMenu("Raise Event (Test)")]
        public void RaiseEventTest()
        {
            if (gameEvent != null)
            {
                gameEvent.Raise(Vector2.one);
            }
        }
        
        #if UNITY_EDITOR
        private void OnValidate()
        {
            if (gameEvent == null && debugMode)
            {
                Debug.LogWarning($"[{name}] GameEvent reference is missing!", this);
            }
        }
        #endif
    }
    
    /// <summary>
    /// Vector2パラメータ用UnityEvent
    /// </summary>
    [System.Serializable]
    public class Vector2UnityEvent : UnityEvent<Vector2> { }
}
