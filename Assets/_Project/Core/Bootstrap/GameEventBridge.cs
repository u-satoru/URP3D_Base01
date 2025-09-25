using UnityEngine;
using asterivo.Unity60.Core.Services.Interfaces;
// using asterivo.Unity60.Core.Services.Interfaces;
using asterivo.Unity60.Core.Events;
using System.Collections.Generic;

namespace asterivo.Unity60.Core.Bootstrap
{
    /// <summary>
    /// GameEvent縺ｨEventManager繧呈ｩ区ｸ｡縺励☆繧九ヶ繝ｪ繝・ず繧ｯ繝ｩ繧ｹ
    /// ScriptableObject繝吶・繧ｹ縺ｮGameEvent縺ｨServiceLocator繝吶・繧ｹ縺ｮEventManager繧堤ｵｱ蜷・
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
            // ServiceLocator縺九ｉEventManager繧貞叙蠕・
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
        /// GameEvent繧脱ventManager縺ｮ繧､繝吶Φ繝亥錐縺ｫ繝槭ャ繝斐Φ繧ｰ
        /// </summary>
        /// <param name="gameEvent">ScriptableObject縺ｮGameEvent</param>
        /// <param name="eventName">EventManager縺ｧ菴ｿ逕ｨ縺吶ｋ繧､繝吶Φ繝亥錐</param>
        public static void MapGameEvent(GameEvent gameEvent, string eventName)
        {
            if (_instance == null || gameEvent == null || string.IsNullOrEmpty(eventName))
                return;

            if (!_instance._eventMappings.ContainsKey(gameEvent))
            {
                _instance._eventMappings[gameEvent] = eventName;

                // GameEvent縺檎匱轣ｫ縺励◆縺ｨ縺阪↓EventManager縺ｫ繧ゆｼ晄眺
                var listener = _instance.gameObject.AddComponent<GameEventListener>();
                listener.Event = gameEvent;
                listener.Response = new UnityEngine.Events.UnityEvent();
                listener.Response.AddListener(() => _instance.OnGameEventRaised(gameEvent));

                Debug.Log($"[GameEventBridge] Mapped GameEvent to '{eventName}'");
            }
        }

        /// <summary>
        /// EventManager縺ｮ繧､繝吶Φ繝医ｒGameEvent縺ｨ縺励※逋ｺ陦・
        /// </summary>
        /// <param name="eventName">繧､繝吶Φ繝亥錐</param>
        /// <param name="data">繧､繝吶Φ繝医ョ繝ｼ繧ｿ</param>
        public static void RaiseAsGameEvent(string eventName, object data = null)
        {
            if (_instance?._eventManager != null)
            {
                _instance._eventManager.RaiseEvent(eventName, data);
            }
        }

        /// <summary>
        /// GameEvent繧脱ventManager邨檎罰縺ｧ逋ｺ陦・
        /// </summary>
        /// <param name="gameEvent">逋ｺ陦後☆繧季ameEvent</param>
        public static void RaiseGameEvent(GameEvent gameEvent)
        {
            if (_instance == null || gameEvent == null)
                return;

            gameEvent.Raise();

            // EventManager縺ｫ繧ゆｼ晄眺
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
        /// 繝悶Μ繝・ず縺悟・譛溷喧縺輔ｌ縺ｦ縺・ｋ縺狗｢ｺ隱・
        /// </summary>
        public static bool IsInitialized => _instance != null && _instance._eventManager != null;

        /// <summary>
        /// EventManager縺ｸ縺ｮ逶ｴ謗･繧｢繧ｯ繧ｻ繧ｹ・亥ｿ・ｦ√↓蠢懊§縺ｦ・・
        /// </summary>
        public static IEventManager GetEventManager()
        {
            return _instance?._eventManager;
        }

        #endregion
    }

    /// <summary>
    /// 蝙句ｮ牙・縺ｪGameEvent縺ｨEventManager縺ｮ邨ｱ蜷・
    /// </summary>
    /// <typeparam name="T">繧､繝吶Φ繝医ョ繝ｼ繧ｿ縺ｮ蝙・/typeparam>
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
        /// 蝙句ｮ牙・縺ｪ繧､繝吶Φ繝医ｒ逋ｺ陦・
        /// </summary>
        public void Raise(T data)
        {
            _eventManager?.RaiseEvent(_eventName, data);
        }

        /// <summary>
        /// 蝙句ｮ牙・縺ｪ繧､繝吶Φ繝医ｒ雉ｼ隱ｭ
        /// </summary>
        public void Subscribe(System.Action<T> handler)
        {
            _eventManager?.Subscribe(_eventName, handler);
        }

        /// <summary>
        /// 蝙句ｮ牙・縺ｪ繧､繝吶Φ繝医・雉ｼ隱ｭ繧定ｧ｣髯､
        /// </summary>
        public void Unsubscribe(System.Action<T> handler)
        {
            _eventManager?.Unsubscribe(_eventName, handler);
        }
    }
}

