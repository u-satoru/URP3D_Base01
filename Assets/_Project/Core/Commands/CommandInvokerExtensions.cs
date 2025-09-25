using UnityEngine;

namespace asterivo.Unity60.Core.Commands
{
    /// <summary>
    /// CommandInvokerにプール化機能を追加する拡張メソッド
    /// </summary>
    public static class CommandInvokerExtensions
    {
        /// <summary>
        /// コマンドを適切なプールに返却します。
        /// 新しいCommandPoolServiceを使用してタイプ安全な返却を行います。
        /// </summary>
        /// <param name="invoker">CommandInvokerのインスタンス</param>
        /// <param name="command">返却するコマンド</param>
        public static void ReturnCommandToPool(this CommandInvoker invoker, ICommand command)
        {
            if (command == null) return;
            
            // ServiceLocator経由でCommandPoolServiceを使用
            var poolService = ServiceLocator.GetService<ICommandPoolService>();
            if (poolService != null)
            {
                poolService.ReturnCommand(command);
            }
            else
            {
                // フォールバック：CommandPoolServiceが利用できない場合
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.LogWarning($"CommandPoolService not available, cannot return {command.GetType().Name} to pool");
#endif
            }
        }
        
        /// <summary>
        /// プール化対応のコマンド実行メソッド
        /// </summary>
        /// <param name="invoker">CommandInvokerのインスタンス</param>
        /// <param name="command">実行するコマンド</param>
        public static void ExecuteCommandWithPooling(this CommandInvoker invoker, ICommand command)
        {
            if (command == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.LogWarning("CommandInvoker: nullのコマンドを実行しようとしました。");
#endif
                return;
            }
            
            // 既存のExecuteCommandメソッドを呼び出し
            invoker.ExecuteCommand(command);
            
            // Undoが不要またはUndoをサポートしないコマンドはプールに返却
            // （UndoStackに追加されたコマンドは後でUndo実行時に返却）
            if (!command.CanUndo)
            {
                invoker.ReturnCommandToPool(command);
            }
        }
    }
}
