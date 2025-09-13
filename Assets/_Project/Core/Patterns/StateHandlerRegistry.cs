using System;
using System.Collections.Generic;
using asterivo.Unity60.Core.Player;
using asterivo.Unity60.Core.Patterns.StateHandlers;

namespace asterivo.Unity60.Core.Patterns
{
    /// <summary>
    /// 状態ハンドラーのレジストリ（Factory + Registry パターン）
    /// </summary>
    public class StateHandlerRegistry
    {
        private readonly Dictionary<PlayerState, IStateHandler> handlers;
        
        public StateHandlerRegistry()
        {
            handlers = new Dictionary<PlayerState, IStateHandler>();
            RegisterDefaultHandlers();
        }
        
        /// <summary>
        /// デフォルトの状態ハンドラーを登録
        /// </summary>
        private void RegisterDefaultHandlers()
        {
            RegisterHandler(new IdleStateHandler());
            RegisterHandler(new WalkingStateHandler());
            RegisterHandler(new RunningStateHandler());
            RegisterHandler(new SprintingStateHandler());
            RegisterHandler(new JumpingStateHandler());
            RegisterHandler(new FallingStateHandler());
            RegisterHandler(new LandingStateHandler());
            RegisterHandler(new CombatStateHandler());
            RegisterHandler(new CombatAttackingStateHandler());
            RegisterHandler(new InteractingStateHandler());
            RegisterHandler(new DeadStateHandler());
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
        public IStateHandler GetHandler(PlayerState state)
        {
            handlers.TryGetValue(state, out IStateHandler handler);
            return handler;
        }
        
        /// <summary>
        /// 指定した状態のハンドラーが登録されているかチェック
        /// </summary>
        /// <param name="state">チェックする状態</param>
        /// <returns>ハンドラーが存在する場合はtrue</returns>
        public bool HasHandler(PlayerState state)
        {
            return handlers.ContainsKey(state);
        }
        
        /// <summary>
        /// 登録されている全ての状態を取得
        /// </summary>
        /// <returns>登録済み状態のコレクション</returns>
        public IEnumerable<PlayerState> GetRegisteredStates()
        {
            return handlers.Keys;
        }
    }
}