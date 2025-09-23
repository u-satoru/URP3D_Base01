using UnityEngine;
using System.Collections.Generic;
// // using asterivo.Unity60.Core.Debug; // Removed to avoid circular dependency
using System.Linq;

namespace asterivo.Unity60.Core.Events
{
    /// <summary>
    /// パラメータなし�E基本イベントチャネル
    /// Unity 6最適化版 - 優先度付きリスナ�E管琁E��忁E    /// </summary>
    [CreateAssetMenu(fileName = "New Game Event", menuName = "asterivo.Unity60/Events/Game Event")]
    public class GameEvent : ScriptableObject
    {
        // リスナ�EのHashSetによる高速管琁E        private readonly HashSet<IGameEventListener> listeners = new HashSet<IGameEventListener>();

        // 優先度ソート済みリスナ�Eリスト（キャチE��ュ�E�E        private List<IGameEventListener> sortedListeners;
        private bool isDirty = true;
        
        #if UNITY_EDITOR
        [Header("Debug Settings")]
        [SerializeField] private bool debugMode = false;
        [SerializeField, TextArea(3, 5)] private string eventDescription;
        
        // エチE��タ用チE��チE��惁E��
        [Header("Runtime Info (Editor Only)")]
        [SerializeField, asterivo.Unity60.Core.Attributes.ReadOnly] private int listenerCount;
        #endif

        /// <summary>
        /// イベントを発火する
        /// </summary>
        public void Raise()
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (debugMode)
            {
                UnityEngine.Debug.Log($"<color=cyan>[GameEvent]</color> '{name}' raised at {Time.time:F2}s with {listeners.Count} listeners", this);
            }
            #endif
            #if UNITY_EDITOR
            listenerCount = listeners.Count;
            #endif
            
            // イベントログに記録�E�簡略化版�E�E            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"[GameEvent] {name} raised to {listeners.Count} listeners");
            #endif
            
            // 優先度でソート（忁E��時のみ�E�E            if (isDirty)
            {
                RebuildSortedList();
            }
            
            // 送E��E��実行（リスナ�Eが�E身を削除しても安�E�E�E            for (int i = sortedListeners.Count - 1; i >= 0; i--)
            {
                if (sortedListeners[i] != null && sortedListeners[i].enabled)
                {
                    sortedListeners[i].OnEventRaised();
                }
            }
        }
        
        /// <summary>
        /// 非同期でイベントを発火�E�フレーム刁E���E�E        /// </summary>
        public System.Collections.IEnumerator RaiseAsync()
        {
            if (isDirty)
            {
                RebuildSortedList();
            }
            
            foreach (var listener in sortedListeners)
            {
                if (listener != null && listener.enabled)
                {
                    listener.OnEventRaised();
                    yield return null; // 次フレームへ
                }
            }
        }

        /// <summary>
        /// リスナ�Eを登録
        /// </summary>
        public void RegisterListener(IGameEventListener listener)
        {
            if (listener == null) return;
            
            if (listeners.Add(listener))
            {
                isDirty = true;
                
                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                if (debugMode)
                {
                    UnityEngine.Debug.Log($"<color=green>[GameEvent]</color> Listener registered to '{name}'", this);
                }
                #endif
            }
        }

        /// <summary>
        /// リスナ�Eを解除
        /// </summary>
        public void UnregisterListener(IGameEventListener listener)
        {
            if (listener == null) return;
            
            if (listeners.Remove(listener))
            {
                isDirty = true;
                
                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                if (debugMode)
                {
                    UnityEngine.Debug.Log($"<color=yellow>[GameEvent]</color> Listener unregistered from '{name}'", this);
                }
                #endif
            }
        }
        
        /// <summary>
        /// 全リスナ�Eをクリア
        /// </summary>
        public void ClearAllListeners()
        {
            listeners.Clear();
            sortedListeners?.Clear();
            isDirty = true;
        }
        
        /// <summary>
        /// アクチE��ブなリスナ�E数を取征E        /// </summary>
        public int GetListenerCount() => listeners.Count;
        
        /// <summary>
        /// リスナ�Eリストを再構篁E        /// </summary>
        private void RebuildSortedList()
        {
            sortedListeners = listeners
                .Where(l => l != null)
                .OrderByDescending(l => l.Priority)
                .ToList();
            isDirty = false;
        }
        
        #if UNITY_EDITOR
        /// <summary>
        /// エチE��タ用�E�手動でイベントを発火
        /// </summary>
        [ContextMenu("Raise Event")]
        private void RaiseManually()
        {
            Raise();
        }
        
        /// <summary>
        /// 現在のリスナ�Eをログ出劁E        /// </summary>
        [ContextMenu("Log All Listeners")]
        private void LogListeners()
        {
            UnityEngine.Debug.Log($"=== Listeners for '{name}' ===");
            foreach (var listener in listeners)
            {
                if (listener != null)
                {
                    var component = listener as Component;
                    if (component != null)
                    {
                        UnityEngine.Debug.Log($"  - {component.gameObject.name}.{listener.GetType().Name} (Priority: {listener.Priority})", component);
                    }
                    else
                    {
                        UnityEngine.Debug.Log($"  - {listener.GetType().Name} (Priority: {listener.Priority})");
                    }
                }
            }
        }
        #endif
    }
    
    #if UNITY_EDITOR
    // エチE��タ用のReadOnly属性
    namespace asterivo.Unity60.Core.Attributes
    {
        public class ReadOnlyAttribute : PropertyAttribute { }
    }
    #endif
}