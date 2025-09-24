using UnityEngine;
using UnityEngine.Events;

namespace asterivo.Unity60.Core.Events
{
    /// <summary>
    /// GameEventのリスナーコンポーネント
    /// UnityEventを介してInspectorから応答を設定可能
    /// </summary>
    public class GameEventListener : MonoBehaviour, IGameEventListener
    {
        [Header("Event Settings")]
        [SerializeField] private GameEvent gameEvent;
        [SerializeField] private int priority = 0;
        [SerializeField] private float responseDelay = 0f;
        
        [Header("Response")]
        [SerializeField] private UnityEvent response;
        
        [Header("Advanced Settings")]
        [SerializeField] private bool registerOnAwake = false;
        [SerializeField] private bool oneShot = false;
        
        private bool hasBeenTriggered = false;
        
        /// <summary>
        /// Responseプロパティ（修正：publicアクセサを追加）
        /// </summary>
        public UnityEvent Response => response;
        
        /// <summary>
        /// 優先度（高い値が先に実行される）
        /// </summary>
        public int Priority => priority;
        
        /// <summary>
        /// 関連付けられたイベント
        /// </summary>
        public GameEvent Event
        {
            get => gameEvent;
            set
            {
                if (gameEvent != null)
                {
                    gameEvent.UnregisterListener(this);
                }
                gameEvent = value;
                if (gameEvent != null && enabled)
                {
                    gameEvent.RegisterListener(this);
                }
            }
        }
        
        private void Awake()
        {
            if (registerOnAwake && gameEvent != null)
            {
                gameEvent.RegisterListener(this);
            }
        }
        
        private void OnEnable()
        {
            if (!registerOnAwake && gameEvent != null)
            {
                gameEvent.RegisterListener(this);
            }
        }
        
        private void OnDisable()
        {
            if (gameEvent != null)
            {
                gameEvent.UnregisterListener(this);
            }
        }
        
        private void OnDestroy()
        {
            if (gameEvent != null)
            {
                gameEvent.UnregisterListener(this);
            }
        }
        
        /// <summary>
        /// イベント受信時の処理
        /// </summary>
        public void OnEventRaised()
        {
            // ワンショットモードで既に実行済みなら無視
            if (oneShot && hasBeenTriggered)
            {
                return;
            }
            
            hasBeenTriggered = true;
            
            // 遅延実行
            if (responseDelay > 0)
            {
                StartCoroutine(DelayedResponse());
            }
            else
            {
                ExecuteResponse();
            }
        }
        
        private System.Collections.IEnumerator DelayedResponse()
        {
            yield return new WaitForSeconds(responseDelay);
            ExecuteResponse();
        }
        
        private void ExecuteResponse()
        {
            response?.Invoke();
            
            // ワンショットモードなら自動で登録解除
            if (oneShot && gameEvent != null)
            {
                gameEvent.UnregisterListener(this);
                enabled = false;
            }
        }
        
        /// <summary>
        /// ワンショットフラグをリセット
        /// </summary>
        public void ResetOneShot()
        {
            hasBeenTriggered = false;
        }
        
        #if UNITY_EDITOR
        /// <summary>
        /// エディタ用：手動でレスポンスを実行
        /// </summary>
        [ContextMenu("Trigger Response")]
        private void TriggerResponseManually()
        {
            OnEventRaised();
        }
        
        private void OnValidate()
        {
            // 優先度の範囲制限
            priority = Mathf.Clamp(priority, -999, 999);
        }
        #endif
    }
}
