using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace asterivo.Unity60.Core.Events
{
    /// <summary>
    /// Vector2パラメータ付きイベントチャネル
    /// カメラのルック入力用に最適化
    /// </summary>
    [CreateAssetMenu(fileName = "New Vector2 Game Event", menuName = "asterivo.Unity60/Events/Vector2 Game Event")]
    public class Vector2GameEvent : ScriptableObject
    {
        private readonly HashSet<Vector2GameEventListener> listeners = new HashSet<Vector2GameEventListener>();
        private List<Vector2GameEventListener> sortedListeners;
        private bool isDirty = true;
        
        #if UNITY_EDITOR
        [Header("Debug Settings")]
        [SerializeField] private bool debugMode = false;
        [SerializeField, TextArea(3, 5)] private string eventDescription;
        
        [Header("Runtime Info (Editor Only)")]
        [SerializeField, asterivo.Unity60.Core.Attributes.ReadOnly] private int listenerCount;
        [SerializeField, asterivo.Unity60.Core.Attributes.ReadOnly] private Vector2 lastRaisedValue;
        #endif
        
        /// <summary>
        /// イベントを発行
        /// </summary>
        public void Raise(Vector2 value)
        {
            #if UNITY_EDITOR
            if (debugMode)
            {
                UnityEngine.Debug.Log($"[{name}] Vector2GameEvent Raised: {value}", this);
            }
            lastRaisedValue = value;
            #endif
            
            if (isDirty)
            {
                RebuildSortedList();
            }
            
            // 優先度順でリスナーに通知
            for (int i = 0; i < sortedListeners.Count; i++)
            {
                if (sortedListeners[i] != null && sortedListeners[i].gameObject.activeInHierarchy)
                {
                    sortedListeners[i].OnEventRaised(value);
                }
            }
        }
        
        /// <summary>
        /// リスナーを登録
        /// </summary>
        public void RegisterListener(Vector2GameEventListener listener)
        {
            if (listener == null) return;
            
            if (listeners.Add(listener))
            {
                isDirty = true;
                #if UNITY_EDITOR
                listenerCount = listeners.Count;
                #endif
            }
        }
        
        /// <summary>
        /// リスナーを解除
        /// </summary>
        public void UnregisterListener(Vector2GameEventListener listener)
        {
            if (listener == null) return;
            
            if (listeners.Remove(listener))
            {
                isDirty = true;
                #if UNITY_EDITOR
                listenerCount = listeners.Count;
                #endif
            }
        }
        
        /// <summary>
        /// 優先度順のソート済みリストを再構築
        /// </summary>
        private void RebuildSortedList()
        {
            if (sortedListeners == null)
            {
                sortedListeners = new List<Vector2GameEventListener>();
            }
            
            sortedListeners.Clear();
            sortedListeners.AddRange(listeners.Where(l => l != null));
            sortedListeners.Sort((a, b) => b.Priority.CompareTo(a.Priority));
            
            isDirty = false;
        }
        
        #if UNITY_EDITOR
        /// <summary>
        /// エディタ用のリスナー情報取得
        /// </summary>
        public IReadOnlyCollection<Vector2GameEventListener> GetListeners()
        {
            return listeners;
        }
        
        private void OnValidate()
        {
            listenerCount = listeners?.Count ?? 0;
        }
        #endif
    }
}
