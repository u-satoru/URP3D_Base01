using System;

namespace asterivo.Unity60.Core.Services
{
    /// <summary>
    /// イベントマネージャーのインターフェース
    /// ServiceLocatorパターンで使用され、GameEventと連携する
    /// </summary>
    public interface IEventManager : IService
    {
        /// <summary>
        /// イベントを発行する
        /// </summary>
        /// <param name="eventName">イベント名</param>
        /// <param name="data">イベントデータ（任意）</param>
        void RaiseEvent(string eventName, object data = null);

        /// <summary>
        /// イベントを購読する
        /// </summary>
        /// <param name="eventName">イベント名</param>
        /// <param name="handler">イベントハンドラー</param>
        void Subscribe(string eventName, Action<object> handler);

        /// <summary>
        /// イベントの購読を解除する
        /// </summary>
        /// <param name="eventName">イベント名</param>
        /// <param name="handler">イベントハンドラー</param>
        void Unsubscribe(string eventName, Action<object> handler);

        /// <summary>
        /// 型安全なイベントを発行する
        /// </summary>
        /// <typeparam name="T">イベントデータの型</typeparam>
        /// <param name="eventName">イベント名</param>
        /// <param name="data">イベントデータ</param>
        void RaiseEvent<T>(string eventName, T data) where T : class;

        /// <summary>
        /// 型安全なイベントを購読する
        /// </summary>
        /// <typeparam name="T">イベントデータの型</typeparam>
        /// <param name="eventName">イベント名</param>
        /// <param name="handler">イベントハンドラー</param>
        void Subscribe<T>(string eventName, Action<T> handler) where T : class;

        /// <summary>
        /// 型安全なイベントの購読を解除する
        /// </summary>
        /// <typeparam name="T">イベントデータの型</typeparam>
        /// <param name="eventName">イベント名</param>
        /// <param name="handler">イベントハンドラー</param>
        void Unsubscribe<T>(string eventName, Action<T> handler) where T : class;

        /// <summary>
        /// 特定のイベントのすべての購読を解除する
        /// </summary>
        /// <param name="eventName">イベント名</param>
        void UnsubscribeAll(string eventName);

        /// <summary>
        /// すべてのイベントの購読を解除する
        /// </summary>
        void Clear();
    }
}
