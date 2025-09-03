using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace asterivo.Unity60.Core.Events
{
    /// <summary>
    /// パラメータなしの基本イベントチャネル
    /// Unity 6最適化版 - 優先度付きリスナー管理対応
    /// </summary>
    [CreateAssetMenu(fileName = "New Game Event", menuName = "asterivo.Unity60/Events/Game Event")]
    public class GameEvent : ScriptableObject
    {
        // リスナーのHashSetによる高速管理
        private readonly HashSet<GameEventListener> listeners = new HashSet<GameEventListener>();
        
        // 優先度ソート済みリスナーリスト（キャッシュ）
        private List<GameEventListener> sortedListeners;
        private bool isDirty = true;
        
        #if UNITY_EDITOR
        [Header("Debug Settings")]
        [SerializeField] private bool debugMode = false;
        [SerializeField, TextArea(3, 5)] private string eventDescription;
        
        // エディタ用デバッグ情報
        [Header("Runtime Info (Editor Only)")]
        [SerializeField, asterivo.Unity60.Core.Attributes.ReadOnly] private int listenerCount;
        #endif

        /// <summary>
        /// イベントを発火する
        /// </summary>
        public void Raise()
        {
            #if UNITY_EDITOR
            if (debugMode)
            {
                UnityEngine.Debug.Log($"<color=cyan>[GameEvent]</color> '{name}' raised at {Time.time:F2}s with {listeners.Count} listeners", this);
            }
            listenerCount = listeners.Count;
            #endif
            
            // イベントログに記録
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            global::asterivo.Unity60.Core.Debug.EventLogger.LogEvent(name, listeners.Count);
            #endif
            
            // 優先度でソート（必要時のみ）
            if (isDirty)
            {
                RebuildSortedList();
            }
            
            // 逆順で実行（リスナーが自身を削除しても安全）
            for (int i = sortedListeners.Count - 1; i >= 0; i--)
            {
                if (sortedListeners[i] != null && sortedListeners[i].enabled)
                {
                    sortedListeners[i].OnEventRaised();
                }
            }
        }
        
        /// <summary>
        /// 非同期でイベントを発火（フレーム分散）
        /// </summary>
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
        /// リスナーを登録
        /// </summary>
        public void RegisterListener(GameEventListener listener)
        {
            if (listener == null) return;
            
            if (listeners.Add(listener))
            {
                isDirty = true;
                
                #if UNITY_EDITOR
                if (debugMode)
                {
                    UnityEngine.Debug.Log($"<color=green>[GameEvent]</color> Listener registered to '{name}'", this);
                }
                #endif
            }
        }

        /// <summary>
        /// リスナーを解除
        /// </summary>
        public void UnregisterListener(GameEventListener listener)
        {
            if (listener == null) return;
            
            if (listeners.Remove(listener))
            {
                isDirty = true;
                
                #if UNITY_EDITOR
                if (debugMode)
                {
                    UnityEngine.Debug.Log($"<color=yellow>[GameEvent]</color> Listener unregistered from '{name}'", this);
                }
                #endif
            }
        }
        
        /// <summary>
        /// 全リスナーをクリア
        /// </summary>
        public void ClearAllListeners()
        {
            listeners.Clear();
            sortedListeners?.Clear();
            isDirty = true;
        }
        
        /// <summary>
        /// アクティブなリスナー数を取得
        /// </summary>
        public int GetListenerCount() => listeners.Count;
        
        /// <summary>
        /// リスナーリストを再構築
        /// </summary>
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
        /// エディタ用：手動でイベントを発火
        /// </summary>
        [ContextMenu("Raise Event")]
        private void RaiseManually()
        {
            Raise();
        }
        
        /// <summary>
        /// 現在のリスナーをログ出力
        /// </summary>
        [ContextMenu("Log All Listeners")]
        private void LogListeners()
        {
            UnityEngine.Debug.Log($"=== Listeners for '{name}' ===");
            foreach (var listener in listeners)
            {
                if (listener != null)
                {
                    UnityEngine.Debug.Log($"  - {listener.gameObject.name} (Priority: {listener.Priority})", listener);
                }
            }
        }
        #endif
    }
    
    #if UNITY_EDITOR
    // エディタ用のReadOnly属性
    namespace asterivo.Unity60.Core.Attributes
    {
        public class ReadOnlyAttribute : PropertyAttribute { }
    }
    #endif
}