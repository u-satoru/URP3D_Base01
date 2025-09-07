using UnityEngine;
using asterivo.Unity60.Core.Player;

namespace asterivo.Unity60.Core.Patterns
{
    /// <summary>
    /// Strategyパターンによる状態処理のインターフェース
    /// </summary>
    public interface IStateHandler
    {
        /// <summary>
        /// 状態に入る際の処理
        /// </summary>
        /// <param name="context">状態のコンテキスト</param>
        void OnEnter(IStateContext context);
        
        /// <summary>
        /// 状態から出る際の処理
        /// </summary>
        /// <param name="context">状態のコンテキスト</param>
        void OnExit(IStateContext context);
        
        /// <summary>
        /// 処理対象の状態
        /// </summary>
        PlayerState HandledState { get; }
    }
    
    /// <summary>
    /// 状態ハンドラーが参照するコンテキスト
    /// </summary>
    public interface IStateContext
    {
        /// <summary>
        /// デバッグログが有効かどうか
        /// </summary>
        bool IsDebugEnabled { get; }
        
        /// <summary>
        /// ログメッセージを出力
        /// </summary>
        /// <param name="message">ログメッセージ</param>
        void Log(string message);
    }
}