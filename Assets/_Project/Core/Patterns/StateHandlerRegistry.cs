using System;
using System.Collections.Generic;
using asterivo.Unity60.Core.Services;
using UnityEngine;

namespace asterivo.Unity60.Core.Patterns
{
    /// <summary>
    /// 状態ハンドラーのレジストリ（Factory + Registry パターン）
    /// ServiceLocatorに登録して使用するサービス実装
    /// </summary>
    public class StateHandlerRegistry : IStateService
    {
        private readonly Dictionary<int, IStateHandler> handlers;
        private bool isInitialized = false;

        public StateHandlerRegistry()
        {
            handlers = new Dictionary<int, IStateHandler>();
        }

        /// <summary>
        /// サービスの初期化
        /// </summary>
        public void Initialize()
        {
            if (!isInitialized)
            {
                Debug.Log("[StateHandlerRegistry] Initializing StateService");
                isInitialized = true;
                // Feature層から必要に応じてHandlerを登録する
            }
        }

        /// <summary>
        /// サービスのシャットダウン
        /// </summary>
        public void Shutdown()
        {
            if (isInitialized)
            {
                Debug.Log("[StateHandlerRegistry] Shutting down StateService");
                ClearHandlers();
                isInitialized = false;
            }
        }
        
        /// <summary>
        /// 状態ハンドラーを登録
        /// </summary>
        /// <param name="handler">登録するハンドラー</param>
        public void RegisterHandler(IStateHandler handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));
                
            handlers[handler.HandledState] = handler;
        }
        
        /// <summary>
        /// 状態ハンドラーを取得
        /// </summary>
        /// <param name="state">対象の状態</param>
        /// <returns>対応するハンドラー、存在しない場合はnull</returns>
        public IStateHandler GetHandler(int state) // Changed from PlayerState to int
        {
            handlers.TryGetValue(state, out IStateHandler handler);
            return handler;
        }
        
        /// <summary>
        /// 指定した状態のハンドラーが登録されているかチェック
        /// </summary>
        /// <param name="state">チェックする状態</param>
        /// <returns>ハンドラーが存在する場合はtrue</returns>
        public bool HasHandler(int state) // Changed from PlayerState to int
        {
            return handlers.ContainsKey(state);
        }
        
        /// <summary>
        /// 登録されている全ての状態を取得
        /// </summary>
        /// <returns>登録済み状態のコレクション</returns>
        public IEnumerable<int> GetRegisteredStates() // Changed from PlayerState to int
        {
            return handlers.Keys;
        }

        /// <summary>
        /// 全てのハンドラーをクリア
        /// </summary>
        public void ClearHandlers()
        {
            handlers.Clear();
        }
    }
}
