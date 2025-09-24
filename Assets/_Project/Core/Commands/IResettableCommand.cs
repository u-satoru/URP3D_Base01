namespace asterivo.Unity60.Core.Commands
{
    /// <summary>
    /// プール化可能なコマンドのためのインターフェース
    /// コマンドの状態をリセットし、新しいパラメーターで初期化できることを示します。
    /// </summary>
    public interface IResettableCommand : ICommand
    {
        /// <summary>
        /// コマンドの状態をリセットし、プールに返却する準備をします。
        /// </summary>
        void Reset();
        
        /// <summary>
        /// 新しいパラメーターでコマンドを初期化します。
        /// </summary>
        /// <param name="parameters">初期化に必要なパラメーター配列</param>
        void Initialize(params object[] parameters);
    }
}
