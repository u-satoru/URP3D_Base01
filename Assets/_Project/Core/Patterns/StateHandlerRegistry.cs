using System;
using asterivo.Unity60.Core.Services.Interfaces;
using System.Collections.Generic;
// using asterivo.Unity60.Core.Services.Interfaces;
using UnityEngine;

namespace asterivo.Unity60.Core.Patterns
{
    /// <summary>
    /// 状態ハンドラーのレジストリ�E�Eactory + Registry パターン�E�E
    /// ServiceLocatorに登録して使用するサービス実裁E
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
        /// サービスの初期匁E
        /// </summary>
        public void Initialize()
        {
            if (!isInitialized)
            {
                Debug.Log("[StateHandlerRegistry] Initializing StateService");
                isInitialized = true;
                // Feature層から忁E��に応じてHandlerを登録する
            }
        }

        /// <summary>
        /// サービスのシャチE��ダウン
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
        /// 状態ハンドラーを取征E
        /// </summary>
        /// <param name="state">対象の状慁E/param>
        /// <returns>対応するハンドラー、存在しなぁE��合�Enull</returns>
        public IStateHandler GetHandler(int state) // Changed from PlayerState to int
        {
            handlers.TryGetValue(state, out IStateHandler handler);
            return handler;
        }
        
        /// <summary>
        /// 持E��した状態�Eハンドラーが登録されてぁE��かチェチE��
        /// </summary>
        /// <param name="state">チェチE��する状慁E/param>
        /// <returns>ハンドラーが存在する場合�Etrue</returns>
        public bool HasHandler(int state) // Changed from PlayerState to int
        {
            return handlers.ContainsKey(state);
        }
        
        /// <summary>
        /// 登録されてぁE��全ての状態を取征E
        /// </summary>
        /// <returns>登録済み状態�Eコレクション</returns>
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

