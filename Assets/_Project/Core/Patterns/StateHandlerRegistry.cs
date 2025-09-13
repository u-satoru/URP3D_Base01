using System;
using System.Collections.Generic;
// using asterivo.Unity60.Core.Player; // Removed to avoid circular dependency
// using asterivo.Unity60.Core.Patterns.StateHandlers; // Moved to Features

namespace asterivo.Unity60.Core.Patterns
{
    /// <summary>
    /// 状態ハンドラーのレジストリ（Factory + Registry パターン）
    /// </summary>
    public class StateHandlerRegistry
    {
        private readonly Dictionary<int, IStateHandler> handlers; // Changed from PlayerState to int
        
        public StateHandlerRegistry()
        {
            handlers = new Dictionary<int, IStateHandler>();
            RegisterDefaultHandlers();
        }
        
        /// <summary>
        /// デフォルトの状態ハンドラーを登録
        /// </summary>
        private void RegisterDefaultHandlers()
        {
            // Temporarily commented out - handlers moved to Features assembly
            // RegisterHandler(new IdleStateHandler());
            // RegisterHandler(new WalkingStateHandler());
            // RegisterHandler(new RunningStateHandler());
            // RegisterHandler(new SprintingStateHandler());
            // RegisterHandler(new JumpingStateHandler());
            // RegisterHandler(new FallingStateHandler());
            // RegisterHandler(new LandingStateHandler());
            // RegisterHandler(new CombatStateHandler());
            // RegisterHandler(new CombatAttackingStateHandler());
            // RegisterHandler(new InteractingStateHandler());
            // RegisterHandler(new DeadStateHandler());
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
    }
}