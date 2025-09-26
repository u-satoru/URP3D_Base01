using asterivo.Unity60.Core.Commands;

namespace asterivo.Unity60.Core
{
    /// <summary>
    /// コマンドプール管理サービスのインターフェース
    /// </summary>
    public interface ICommandPoolService : IService
    {
        /// <summary>
        /// コマンドを取得
        /// </summary>
        T GetCommand<T>() where T : class, ICommand, new();

        /// <summary>
        /// コマンドを返却
        /// </summary>
        void ReturnCommand<T>(T command) where T : class, ICommand;
    }
}