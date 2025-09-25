using System;
using System.Collections.Generic;
using UnityEngine;

namespace asterivo.Unity60.Core.Services
{
    /// <summary>
    /// イベントマネージャーの実装
    /// ScriptableObjectベースのGameEventと連携し、ServiceLocatorパターンで使用される
    /// </summary>
    public class EventManager : IEventManager
    {
        private readonly Dictionary<string, List<Delegate>> _eventHandlers = new Dictionary<string, List<Delegate>>();
        private readonly object _lock = new object();
        private bool _isInitialized = false;

        #region IService Implementation

        public void OnServiceRegistered()
        {
            if (_isInitialized) return;

            _eventHandlers.Clear();
            _isInitialized = true;
            Debug.Log("[EventManager] Service Registered");
        }

        public void OnServiceUnregistered()
        {
            Clear();
            _isInitialized = false;
            Debug.Log("[EventManager] Service Unregistered");
        }

        public bool IsServiceActive => _isInitialized;

        public string ServiceName => "EventManager";

        #endregion

        #region IEventManager Implementation

        public void RaiseEvent(string eventName, object data = null)
        {
            if (string.IsNullOrEmpty(eventName)) return;

            lock (_lock)
            {
                if (_eventHandlers.TryGetValue(eventName, out var handlers))
                {
                    // ハンドラーのコピーを作成して、イベント処理中の変更を安全にする
                    var handlersCopy = new List<Delegate>(handlers);
                    foreach (var handler in handlersCopy)
                    {
                        try
                        {
                            if (handler is Action<object> actionHandler)
                            {
                                actionHandler.Invoke(data);
                            }
                            else if (handler is Action simpleHandler && data == null)
                            {
                                simpleHandler.Invoke();
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"[EventManager] Error raising event '{eventName}': {ex.Message}");
                        }
                    }
                }
            }
        }

        public void Subscribe(string eventName, Action<object> handler)
        {
            if (string.IsNullOrEmpty(eventName) || handler == null) return;

            lock (_lock)
            {
                if (!_eventHandlers.TryGetValue(eventName, out var handlers))
                {
                    handlers = new List<Delegate>();
                    _eventHandlers[eventName] = handlers;
                }

                if (!handlers.Contains(handler))
                {
                    handlers.Add(handler);
                    Debug.Log($"[EventManager] Subscribed to event '{eventName}'");
                }
            }
        }

        public void Unsubscribe(string eventName, Action<object> handler)
        {
            if (string.IsNullOrEmpty(eventName) || handler == null) return;

            lock (_lock)
            {
                if (_eventHandlers.TryGetValue(eventName, out var handlers))
                {
                    if (handlers.Remove(handler))
                    {
                        Debug.Log($"[EventManager] Unsubscribed from event '{eventName}'");

                        // ハンドラーリストが空になったら削除
                        if (handlers.Count == 0)
                        {
                            _eventHandlers.Remove(eventName);
                        }
                    }
                }
            }
        }

        public void RaiseEvent<T>(string eventName, T data) where T : class
        {
            if (string.IsNullOrEmpty(eventName)) return;

            lock (_lock)
            {
                if (_eventHandlers.TryGetValue(eventName, out var handlers))
                {
                    var handlersCopy = new List<Delegate>(handlers);
                    foreach (var handler in handlersCopy)
                    {
                        try
                        {
                            if (handler is Action<T> typedHandler)
                            {
                                typedHandler.Invoke(data);
                            }
                            else if (handler is Action<object> objectHandler)
                            {
                                objectHandler.Invoke(data);
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"[EventManager] Error raising typed event '{eventName}': {ex.Message}");
                        }
                    }
                }
            }
        }

        public void Subscribe<T>(string eventName, Action<T> handler) where T : class
        {
            if (string.IsNullOrEmpty(eventName) || handler == null) return;

            lock (_lock)
            {
                if (!_eventHandlers.TryGetValue(eventName, out var handlers))
                {
                    handlers = new List<Delegate>();
                    _eventHandlers[eventName] = handlers;
                }

                if (!handlers.Contains(handler))
                {
                    handlers.Add(handler);
                    Debug.Log($"[EventManager] Subscribed to typed event '{eventName}'");
                }
            }
        }

        public void Unsubscribe<T>(string eventName, Action<T> handler) where T : class
        {
            if (string.IsNullOrEmpty(eventName) || handler == null) return;

            lock (_lock)
            {
                if (_eventHandlers.TryGetValue(eventName, out var handlers))
                {
                    if (handlers.Remove(handler))
                    {
                        Debug.Log($"[EventManager] Unsubscribed from typed event '{eventName}'");

                        if (handlers.Count == 0)
                        {
                            _eventHandlers.Remove(eventName);
                        }
                    }
                }
            }
        }

        public void UnsubscribeAll(string eventName)
        {
            if (string.IsNullOrEmpty(eventName)) return;

            lock (_lock)
            {
                if (_eventHandlers.Remove(eventName))
                {
                    Debug.Log($"[EventManager] Unsubscribed all from event '{eventName}'");
                }
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _eventHandlers.Clear();
                Debug.Log("[EventManager] Cleared all event subscriptions");
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// 登録されているイベントの数を取得
        /// </summary>
        public int GetEventCount()
        {
            lock (_lock)
            {
                return _eventHandlers.Count;
            }
        }

        /// <summary>
        /// 特定のイベントの購読者数を取得
        /// </summary>
        public int GetSubscriberCount(string eventName)
        {
            lock (_lock)
            {
                if (_eventHandlers.TryGetValue(eventName, out var handlers))
                {
                    return handlers.Count;
                }
                return 0;
            }
        }

        /// <summary>
        /// イベントが登録されているかチェック
        /// </summary>
        public bool HasEvent(string eventName)
        {
            lock (_lock)
            {
                return _eventHandlers.ContainsKey(eventName);
            }
        }

        #endregion
    }
}
