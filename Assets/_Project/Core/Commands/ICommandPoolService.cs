using asterivo.Unity60.Core.Commands;

namespace asterivo.Unity60.Core.Commands
{
    /// <summary>
    /// コマンドプールサービスのインターフェース
    /// ServiceLocatorパターンでの依存性注入を支援
    /// </summary>
    public interface ICommandPoolService
    {
        /// <summary>
        /// CommandPoolManagerへの直接アクセス
        /// 高度な制御やカスタムプール操作に使用
        /// </summary>
        CommandPoolManager PoolManager { get; }

        /// <summary>
        /// 指定した型のコマンドをプールから取得します
        /// </summary>
        /// <typeparam name="T">取得するコマンドの型</typeparam>
        /// <returns>使用可能なコマンドインスタンス</returns>
        T GetCommand<T>() where T : class, ICommand, new();

        /// <summary>
        /// 使用完了したコマンドをプールに返却します
        /// </summary>
        /// <typeparam name="T">返却するコマンドの型</typeparam>
        /// <param name="command">プールに返却するコマンドインスタンス</param>
        void ReturnCommand<T>(T command) where T : ICommand;

        /// <summary>
        /// 指定したコマンド型のプール統計情報を取得します
        /// </summary>
        /// <typeparam name="T">統計情報を取得するコマンドの型</typeparam>
        /// <returns>コマンドプールの統計情報</returns>
        CommandStatistics GetStatistics<T>() where T : ICommand;

        /// <summary>
        /// 全コマンドプールのデバッグ情報をログに出力します
        /// </summary>
        void LogDebugInfo();

        /// <summary>
        /// サービスの状態確認とデバッグ情報をログに出力します
        /// </summary>
        void LogServiceStatus();

        /// <summary>
        /// サービスのクリーンアップ処理を実行します
        /// </summary>
        void Cleanup();
    }
}