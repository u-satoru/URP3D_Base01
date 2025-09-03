using UnityEngine;
using UnityEngine.Events;

namespace asterivo.Unity60.Core.Events
{
    /// <summary>
    /// 型付きイベントリスナーのインターフェース
    /// </summary>
    public interface IGameEventListener<T>
    {
        void OnEventRaised(T value);
    }
    
    /// <summary>
    /// 型付きイベントリスナーの基底クラス
    /// </summary>
    public abstract class GenericGameEventListener<T, TEvent, TUnityEvent> : MonoBehaviour, IGameEventListener<T>
        where TEvent : GenericGameEvent<T>
        where TUnityEvent : UnityEvent<T>
    {
        [Header("Event Settings")]
        [SerializeField] protected TEvent gameEvent;
        [SerializeField] protected int priority = 0;
        
        [Header("Response")]
        [SerializeField] protected TUnityEvent response;
        
        [Header("Value Processing")]
        [SerializeField] protected bool storeValue = false;
        [SerializeField] protected T storedValue;
        
        [Header("Advanced")]
        [SerializeField] protected bool logOnReceive = false;
        [SerializeField] protected float responseDelay = 0f;
        
        public int Priority => priority;
        public TEvent GameEvent 
        { 
            get => gameEvent; 
            set => gameEvent = value; 
        }
        public TUnityEvent Response => response;
        
        protected virtual void OnEnable()
        {
            if (gameEvent != null)
            {
                gameEvent.RegisterListener(this);
            }
        }
        
        protected virtual void OnDisable()
        {
            if (gameEvent != null)
            {
                gameEvent.UnregisterListener(this);
            }
        }
        
        public virtual void OnEventRaised(T value)
        {
            if (storeValue)
            {
                storedValue = value;
            }
            
            if (logOnReceive)
            {
                UnityEngine.Debug.Log($"<color=lime>[Listener]</color> {gameObject.name} received: {value}", this);
            }
            
            if (responseDelay > 0)
            {
                StartCoroutine(DelayedResponse(value));
            }
            else
            {
                response?.Invoke(value);
            }
        }
        
        private System.Collections.IEnumerator DelayedResponse(T value)
        {
            yield return new WaitForSeconds(responseDelay);
            response?.Invoke(value);
        }
        
        public T GetStoredValue() => storedValue;
    }
    
    // 具体的な型のリスナー実装
    public class FloatGameEventListener : GenericGameEventListener<float, FloatGameEvent, UnityFloatEvent> { }
    
    public class IntGameEventListener : GenericGameEventListener<int, IntGameEvent, UnityIntEvent> { }
    
    public class BoolGameEventListener : GenericGameEventListener<bool, BoolGameEvent, UnityBoolEvent> { }
    
    public class StringGameEventListener : GenericGameEventListener<string, StringGameEvent, UnityStringEvent> { }
    
    public class Vector3GameEventListener : GenericGameEventListener<Vector3, Vector3GameEvent, UnityVector3Event> { }
    
    public class GameObjectEventListener : GenericGameEventListener<GameObject, GameObjectGameEvent, UnityGameObjectEvent> { }
    
    // カスタムUnityEvent定義
    [System.Serializable]
    public class UnityFloatEvent : UnityEvent<float> { }
    
    [System.Serializable]
    public class UnityIntEvent : UnityEvent<int> { }
    
    [System.Serializable]
    public class UnityBoolEvent : UnityEvent<bool> { }
    
    [System.Serializable]
    public class UnityStringEvent : UnityEvent<string> { }
    
    [System.Serializable]
    public class UnityVector3Event : UnityEvent<Vector3> { }
    
    [System.Serializable]
    public class UnityGameObjectEvent : UnityEvent<GameObject> { }
}