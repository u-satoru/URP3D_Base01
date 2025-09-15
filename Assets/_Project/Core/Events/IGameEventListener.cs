using UnityEngine;
using UnityEngine.Events;

namespace asterivo.Unity60.Core.Events
{
    /// <summary>
    /// 型付きイベントリスナーのインターフェース。
    /// GameEventシステムでイベントを受け取るコンポーネントが実装すべきインターフェースです。
    /// 
    /// 使用例：
    /// - プレイヤーのHPが変化した時の通知受信
    /// - アイテム収集時の通知受信
    /// - ゲーム状態変更時の通知受信
    /// </summary>
    /// <typeparam name="T">イベントで送信される値の型</typeparam>
    public interface IGameEventListener<T>
    {
        /// <summary>
        /// イベントが発生した際に呼び出されるメソッド。
        /// </summary>
        /// <param name="value">イベントと共に送信される値</param>
        void OnEventRaised(T value);

        /// <summary>
        /// Priority for event processing (higher values process first)
        /// </summary>
        int Priority => 0;
    }
    
    /// <summary>
    /// 型付きイベントリスナーの基底クラス。
    /// UnityのInspectorで設定可能な汎用的なイベントリスナー実装を提供します。
    /// 
    /// 主な機能：
    /// - 優先度付きイベント処理
    /// - 値の保存機能
    /// - 遅延レスポンス機能
    /// - デバッグログ機能
    /// - UnityEventとの統合
    /// </summary>
    /// <typeparam name="T">イベントデータの型</typeparam>
    /// <typeparam name="TEvent">対応するGameEventの型</typeparam>
    /// <typeparam name="TUnityEvent">対応するUnityEventの型</typeparam>
    public abstract class GenericGameEventListener<T, TEvent, TUnityEvent> : MonoBehaviour, IGameEventListener<T>
        where TEvent : GameEvent<T>
        where TUnityEvent : UnityEvent<T>
    {
        [Header("Event Settings")]
        [Tooltip("リスニングする対象のGameEventアセット")]
        [SerializeField] protected TEvent gameEvent;
        
        [Tooltip("イベント処理の優先度。高いほど先に実行される")]
        [SerializeField] protected int priority = 0;
        
        [Header("Response")]
        [Tooltip("イベント受信時に実行するUnityEvent")]
        [SerializeField] protected TUnityEvent response;
        
        [Header("Value Processing")]
        [Tooltip("受信した値を保存するかどうか")]
        [SerializeField] protected bool storeValue = false;
        
        [Tooltip("保存された最新のイベント値（storeValue=trueの場合のみ）")]
        [SerializeField] protected T storedValue;
        
        [Header("Advanced")]
        [Tooltip("イベント受信時にログを出力するかどうか")]
        [SerializeField] protected bool logOnReceive = false;
        
        [Tooltip("レスポンスの遅延時間（秒）。0の場合は即座に実行")]
        [SerializeField] protected float responseDelay = 0f;
        
        /// <summary>
        /// このリスナーの優先度を取得します。高い値ほど先に実行されます。
        /// </summary>
        public int Priority => priority;
        
        /// <summary>
        /// リスニング対象のGameEventを取得または設定します。
        /// </summary>
        public TEvent GameEvent 
        { 
            get => gameEvent; 
            set => gameEvent = value; 
        }
        
        /// <summary>
        /// イベント受信時に実行されるUnityEventを取得します。
        /// </summary>
        public TUnityEvent Response => response;
        
        /// <summary>
        /// コンポーネントが有効化された際に呼び出されます。
        /// GameEventにリスナーとして自身を登録します。
        /// </summary>
        protected virtual void OnEnable()
        {
            if (gameEvent != null)
            {
                gameEvent.RegisterListener(this);
            }
        }
        
        /// <summary>
        /// コンポーネントが無効化された際に呼び出されます。
        /// GameEventからリスナーとして自身を登録解除します。
        /// </summary>
        protected virtual void OnDisable()
        {
            if (gameEvent != null)
            {
                gameEvent.UnregisterListener(this);
            }
        }
        
        /// <summary>
        /// イベントが発生した際に呼び出されるメソッド。
        /// IGameEventListener&lt;T&gt;インターフェースの実装です。
        /// </summary>
        /// <param name="value">イベントと共に送信された値</param>
        public virtual void OnEventRaised(T value)
        {
            // 値の保存機能
            if (storeValue)
            {
                storedValue = value;
            }
            
            // デバッグログ機能
            if (logOnReceive)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.Log($"<color=lime>[Listener]</color> {gameObject.name} received: {value}", this);
#endif
            }
            
            // 遅延レスポンス機能
            if (responseDelay > 0)
            {
                StartCoroutine(DelayedResponse(value));
            }
            else
            {
                response?.Invoke(value);
            }
        }
        
        /// <summary>
        /// 指定された遅延時間後にレスポンスを実行するコルーチン。
        /// </summary>
        /// <param name="value">レスポンスに渡す値</param>
        /// <returns>コルーチン</returns>
        private System.Collections.IEnumerator DelayedResponse(T value)
        {
            yield return new WaitForSeconds(responseDelay);
            response?.Invoke(value);
        }
        
        /// <summary>
        /// 保存された最新のイベント値を取得します。
        /// storeValue=trueの場合のみ有効な値が返されます。
        /// </summary>
        /// <returns>保存された値</returns>
        public T GetStoredValue() => storedValue;
    }
    
    // ===== 具体的な型のリスナー実装 =====
    // 各型に特化したGameEventListenerクラス。
    // Inspector上で直接使用でき、対応するGameEventアセットを割り当て可能です。
    
    /// <summary>
    /// float値を扱うGameEventListener。数値データ（HP、スコア、時間など）の受信に使用。
    /// </summary>
    public class FloatGameEventListener : GenericGameEventListener<float, FloatGameEvent, UnityFloatEvent> { }
    
    /// <summary>
    /// int値を扱うGameEventListener。整数データ（レベル、アイテム数など）の受信に使用。
    /// </summary>
    public class IntGameEventListener : GenericGameEventListener<int, IntGameEvent, UnityIntEvent> { }
    
    /// <summary>
    /// bool値を扱うGameEventListener。フラグやスイッチ状態の受信に使用。
    /// </summary>
    public class BoolGameEventListener : GenericGameEventListener<bool, BoolGameEvent, UnityBoolEvent> { }
    
    /// <summary>
    /// string値を扱うGameEventListener。テキストやメッセージの受信に使用。
    /// </summary>
    public class StringGameEventListener : GenericGameEventListener<string, StringGameEvent, UnityStringEvent> { }
    
    /// <summary>
    /// Vector3値を扱うGameEventListener。位置や方向データの受信に使用。
    /// </summary>
    public class Vector3GameEventListener : GenericGameEventListener<Vector3, Vector3GameEvent, UnityVector3Event> { }
    
    /// <summary>
    /// GameObject値を扱うGameEventListener。オブジェクト参照の受信に使用。
    /// </summary>
    public class GameObjectEventListener : GenericGameEventListener<GameObject, GameObjectGameEvent, UnityGameObjectEvent> { }
    
    // ===== カスタムUnityEvent定義 =====
    // 各型に対応するUnityEventクラス。Inspector上で設定可能な型付きUnityEventです。
    
    /// <summary>
    /// float値を受け取るUnityEvent。Inspector上でfloat引数を持つメソッドを設定可能。
    /// </summary>
    [System.Serializable]
    public class UnityFloatEvent : UnityEvent<float> { }
    
    /// <summary>
    /// int値を受け取るUnityEvent。Inspector上でint引数を持つメソッドを設定可能。
    /// </summary>
    [System.Serializable]
    public class UnityIntEvent : UnityEvent<int> { }
    
    /// <summary>
    /// bool値を受け取るUnityEvent。Inspector上でbool引数を持つメソッドを設定可能。
    /// </summary>
    [System.Serializable]
    public class UnityBoolEvent : UnityEvent<bool> { }
    
    /// <summary>
    /// string値を受け取るUnityEvent。Inspector上でstring引数を持つメソッドを設定可能。
    /// </summary>
    [System.Serializable]
    public class UnityStringEvent : UnityEvent<string> { }
    
    /// <summary>
    /// Vector3値を受け取るUnityEvent。Inspector上でVector3引数を持つメソッドを設定可能。
    /// </summary>
    [System.Serializable]
    public class UnityVector3Event : UnityEvent<Vector3> { }
    
    /// <summary>
    /// GameObject値を受け取るUnityEvent。Inspector上でGameObject引数を持つメソッドを設定可能。
    /// </summary>
    [System.Serializable]
    public class UnityGameObjectEvent : UnityEvent<GameObject> { }
}