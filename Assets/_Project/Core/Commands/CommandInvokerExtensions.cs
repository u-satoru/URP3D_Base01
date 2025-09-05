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
        /// </summary>
        /// <param name="invoker">CommandInvokerのインスタンス</param>
        /// <param name="command">返却するコマンド</param>
        public static void ReturnCommandToPool(this CommandInvoker invoker, ICommand command)
        {
            if (command == null || CommandPool.Instance == null) return;
            
            // タイプ別にプールに返却
            switch (command)
            {
                case DamageCommand damageCommand:
                    CommandPool.Instance.ReturnCommand(damageCommand);
                    break;
                case HealCommand healCommand:
                    CommandPool.Instance.ReturnCommand(healCommand);
                    break;
                default:
                    // 他のコマンドタイプの処理が必要な場合はここに追加
                    UnityEngine.Debug.Log($"CommandInvoker: {command.GetType().Name}はプール化に対応していません。");
                    break;
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
                UnityEngine.Debug.LogWarning("CommandInvoker: nullのコマンドを実行しようとしました。");
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