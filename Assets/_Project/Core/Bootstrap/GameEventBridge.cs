using UnityEngine;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Services;
using System.Collections.Generic;

namespace asterivo.Unity60.Core.Bootstrap
{
    /// <summary>
    /// GameEventとEventManagerを橋渡しするブリッジクラス
    /// ScriptableObjectベースのGameEventとServiceLocatorベースのEventManagerを統合
    /// </summary>
    public class GameEventBridge : MonoBehaviour
    {
        private static GameEventBridge _instance;
        private IEventManager _eventManager;
        private readonly Dictionary<GameEvent, string> _eventMappings = new Dictionary<GameEvent, string>();

        #region Unity Lifecycle

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                Cleanup();
                _instance = null;
            }
        }

        #endregion

        #region Initialization

        private void Initialize()
        {
            // ServiceLocatorからEventManagerを取得
            if (ServiceLocator.TryGet<IEventManager>(out var eventManager))
            {
                _eventManager = eventManager;
                Debug.Log("[GameEventBridge] Connected to EventManager");
            }
            else
            {
                Debug.LogWarning("[GameEventBridge] EventManager not found in ServiceLocator. Creating new instance.");
                _eventManager = new EventManager();
                ServiceLocator.Register<IEventManager>(_eventManager);
            }
        }

        private void Cleanup()
        {
            _eventMappings.Clear();
            _eventManager = null;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// GameEventをEventManagerのイベント名にマッピング
        /// </summary>
        /// <param name="gameEvent">ScriptableObjectのGameEvent</param>
        /// <param name="eventName">EventManagerで使用するイベント名</param>
        public static void MapGameEvent(GameEvent gameEvent, string eventName)
        {
            if (_instance == null || gameEvent == null || string.IsNullOrEmpty(eventName))
                return;

            if (!_instance._eventMappings.ContainsKey(gameEvent))
            {
                _instance._eventMappings[gameEvent] = eventName;

                // GameEventが発火したときにEventManagerにも伝播
                var listener = _instance.gameObject.AddComponent<GameEventListener>();
                listener.Event = gameEvent;
                // ResponseはUnityEventなので、AddListenerでコールバックを登録
                listener.Response.AddListener(() => _instance.OnGameEventRaised(gameEvent));

                Debug.Log($"[GameEventBridge] Mapped GameEvent to '{eventName}'");
            }
        }

        /// <summary>
        /// EventManagerのイベントをGameEventとして発行
        /// </summary>
        /// <param name="eventName">イベント名</param>
        /// <param name="data">イベントデータ</param>
        public static void RaiseAsGameEvent(string eventName, object data = null)
        {
            if (_instance?._eventManager != null)
            {
                _instance._eventManager.RaiseEvent(eventName, data);
            }
        }

        /// <summary>
        /// GameEventをEventManager経由で発行
        /// </summary>
        /// <param name="gameEvent">発行するGameEvent</param>
        public static void RaiseGameEvent(GameEvent gameEvent)
        {
            if (_instance == null || gameEvent == null)
                return;

            gameEvent.Raise();

            // EventManagerにも伝播
            if (_instance._eventMappings.TryGetValue(gameEvent, out var eventName))
            {
                _instance._eventManager?.RaiseEvent(eventName);
            }
        }

        #endregion

        #region Private Methods

        private void OnGameEventRaised(GameEvent gameEvent)
        {
            if (_eventMappings.TryGetValue(gameEvent, out var eventName))
            {
                _eventManager?.RaiseEvent(eventName);
                Debug.Log($"[GameEventBridge] GameEvent raised as '{eventName}'");
            }
        }

        #endregion

        #region Static Helpers

        /// <summary>
        /// ブリッジが初期化されているか確認
        /// </summary>
        public static bool IsInitialized => _instance != null && _instance._eventManager != null;

        /// <summary>
        /// EventManagerへの直接アクセス（必要に応じて）
        /// </summary>
        public static IEventManager GetEventManager()
        {
            return _instance?._eventManager;
        }

        #endregion
    }

    /// <summary>
    /// 型安全なGameEventとEventManagerの統合
    /// </summary>
    /// <typeparam name="T">イベントデータの型</typeparam>
    public class TypedGameEventBridge<T> where T : class
    {
        private readonly string _eventName;
        private readonly IEventManager _eventManager;

        public TypedGameEventBridge(string eventName)
        {
            _eventName = eventName;
            _eventManager = ServiceLocator.TryGet<IEventManager>(out var em) ? em : null;
        }

        /// <summary>
        /// 型安全なイベントを発行
        /// </summary>
        public void Raise(T data)
        {
            _eventManager?.RaiseEvent(_eventName, data);
        }

        /// <summary>
        /// 型安全なイベントを購読
        /// </summary>
        public void Subscribe(System.Action<T> handler)
        {
            _eventManager?.Subscribe(_eventName, handler);
        }

        /// <summary>
        /// 型安全なイベントの購読を解除
        /// </summary>
        public void Unsubscribe(System.Action<T> handler)
        {
            _eventManager?.Unsubscribe(_eventName, handler);
        }
    }
}