using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace asterivo.Unity60.Core.Events
{
    /// <summary>
    /// 型付きパラメータを持つジェネリックイベントチャネル
    /// </summary>
    public abstract class GenericGameEvent<T> : ScriptableObject
    {
        private readonly HashSet<IGameEventListener<T>> listeners = new HashSet<IGameEventListener<T>>();
        
        [Header("Event Settings")]
        [SerializeField] private bool cacheLastValue = false;
        [SerializeField] private T defaultValue;
        
        private T lastValue;
        private bool hasValue = false;
        
        #if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool debugMode = false;
        #endif
        
        /// <summary>
        /// 値を伴ってイベントを発火
        /// </summary>
        public void Raise(T value)
        {
            if (cacheLastValue)
            {
                lastValue = value;
                hasValue = true;
            }
            
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (debugMode)
            {
                UnityEngine.Debug.Log($"<color=cyan>[GenericEvent]</color> '{name}' raised with value: {value}", this);
            }
            #endif
            
            // イベントログに記録（ペイロード付き）
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (global::asterivo.Unity60.Core.Debug.EventLogger.IsEnabled)
            {
                global::asterivo.Unity60.Core.Debug.EventLogger.LogEventWithPayload(name, listeners.Count, value);
            }
            #endif
            
            // ToArrayで安全なイテレーション
            var activeListeners = listeners.Where(l => l != null).ToArray();
            
            foreach (var listener in activeListeners)
            {
                listener.OnEventRaised(value);
            }
        }
        
        /// <summary>
        /// デフォルト値でイベントを発火
        /// </summary>
        public void RaiseDefault()
        {
            Raise(defaultValue);
        }
        
        /// <summary>
        /// 最後の値を取得（キャッシュが有効な場合）
        /// </summary>
        public T GetLastValue()
        {
            if (!cacheLastValue || !hasValue)
            {
                return defaultValue;
            }
            return lastValue;
        }
        
        /// <summary>
        /// キャッシュされた値があるか
        /// </summary>
        public bool HasCachedValue() => cacheLastValue && hasValue;
        
        /// <summary>
        /// リスナーを登録
        /// </summary>
        public void RegisterListener(IGameEventListener<T> listener)
        {
            if (listener == null) return;
            
            listeners.Add(listener);
            
            // 初回登録時にキャッシュ値があれば通知
            if (cacheLastValue && hasValue)
            {
                listener.OnEventRaised(lastValue);
            }
        }
        
        /// <summary>
        /// リスナーを解除
        /// </summary>
        public void UnregisterListener(IGameEventListener<T> listener)
        {
            if (listener == null) return;
            listeners.Remove(listener);
        }
        
        
        /// <summary>
        /// 全リスナーをクリア
        /// </summary>
        public void ClearAllListeners()
        {
            listeners.Clear();
        }
        
        /// <summary>
        /// キャッシュをクリア
        /// </summary>
        public void ClearCache()
        {
            lastValue = defaultValue;
            hasValue = false;
        }
        
        public int GetListenerCount() => listeners.Count;
        
        #if UNITY_EDITOR
        [ContextMenu("Raise with Default Value")]
        private void RaiseManually()
        {
            RaiseDefault();
        }
        #endif
    }
    
    // 具体的な型の実装
    [CreateAssetMenu(fileName = "New Float Event", menuName = "asterivo.Unity60/Events/Float Event")]
    public class FloatGameEvent : GenericGameEvent<float> { }
    
    [CreateAssetMenu(fileName = "New Int Event", menuName = "asterivo.Unity60/Events/Int Event")]
    public class IntGameEvent : GenericGameEvent<int> { }
    
    [CreateAssetMenu(fileName = "New Bool Event", menuName = "asterivo.Unity60/Events/Bool Event")]
    public class BoolGameEvent : GenericGameEvent<bool> { }
    
    [CreateAssetMenu(fileName = "New String Event", menuName = "asterivo.Unity60/Events/String Event")]
    public class StringGameEvent : GenericGameEvent<string> { }
    
    [CreateAssetMenu(fileName = "New Vector3 Event", menuName = "asterivo.Unity60/Events/Vector3 Event")]
    public class Vector3GameEvent : GenericGameEvent<Vector3> { }
    
    [CreateAssetMenu(fileName = "New GameObject Event", menuName = "asterivo.Unity60/Events/GameObject Event")]
    public class GameObjectGameEvent : GenericGameEvent<GameObject> { }
}