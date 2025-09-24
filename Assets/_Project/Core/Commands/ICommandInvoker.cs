namespace asterivo.Unity60.Core.Commands
{
    /// <summary>
    /// コマンド実行とUndo/Redo機能を提供するインターフェース
    /// CommandInvokerクラスの公開APIを定義し、
    /// 依存性注入とテスタビリティを向上させます
    /// </summary>
    public interface ICommandInvoker
    {
        /// <summary>
        /// Undo操作が可能かどうか
        /// </summary>
        bool CanUndo { get; }

        /// <summary>
        /// Redo操作が可能かどうか
        /// </summary>
        bool CanRedo { get; }

        /// <summary>
        /// Undoスタックに保存されているコマンドの数
        /// </summary>
        int UndoStackCount { get; }

        /// <summary>
        /// Redoスタックに保存されているコマンドの数
        /// </summary>
        int RedoStackCount { get; }

        /// <summary>
        /// コマンドを実行し、Undo履歴に追加します
        /// </summary>
        /// <param name="command">実行するコマンド</param>
        void ExecuteCommand(ICommand command);

        /// <summary>
        /// 最後に実行したコマンドを取り消します
        /// </summary>
        /// <returns>Undo操作が成功した場合はtrue</returns>
        bool Undo();

        /// <summary>
        /// 最後に取り消したコマンドを再実行します
        /// </summary>
        /// <returns>Redo操作が成功した場合はtrue</returns>
        bool Redo();

        /// <summary>
        /// Undo/Redo履歴をクリアします
        /// </summary>
        void ClearHistory();
    }
}