// using asterivo.Unity60.Core.Commands;

namespace asterivo.Unity60.Core.Commands
{
    /// <summary>
    /// コマンド�Eールサービスのインターフェース
    /// ServiceLocatorパターンでの依存性注入を支援
    /// </summary>
    public interface ICommandPoolService
    {
        /// <summary>
        /// CommandPoolManagerへの直接アクセス
        /// 高度な制御めE��スタムプ�Eル操作に使用
        /// </summary>
        CommandPoolManager PoolManager { get; }

        /// <summary>
        /// 持E��した型のコマンドをプ�Eルから取得しまぁE        /// </summary>
        /// <typeparam name="T">取得するコマンド�E垁E/typeparam>
        /// <returns>使用可能なコマンドインスタンス</returns>
        T GetCommand<T>() where T : class, ICommand, new();

        /// <summary>
        /// 使用完亁E��たコマンドをプ�Eルに返却しまぁE        /// </summary>
        /// <typeparam name="T">返却するコマンド�E垁E/typeparam>
        /// <param name="command">プ�Eルに返却するコマンドインスタンス</param>
        void ReturnCommand<T>(T command) where T : ICommand;

        /// <summary>
        /// 持E��したコマンド型のプ�Eル統計情報を取得しまぁE        /// </summary>
        /// <typeparam name="T">統計情報を取得するコマンド�E垁E/typeparam>
        /// <returns>コマンド�Eールの統計情報</returns>
        CommandStatistics GetStatistics<T>() where T : ICommand;

        /// <summary>
        /// 全コマンド�EールのチE��チE��惁E��をログに出力しまぁE        /// </summary>
        void LogDebugInfo();

        /// <summary>
        /// サービスの状態確認とチE��チE��惁E��をログに出力しまぁE        /// </summary>
        void LogServiceStatus();

        /// <summary>
        /// サービスのクリーンアチE�E処琁E��実行しまぁE        /// </summary>
        void Cleanup();
    }
}