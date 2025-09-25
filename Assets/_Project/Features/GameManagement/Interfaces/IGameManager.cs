using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Core.Types;
using asterivo.Unity60.Core.Commands;

namespace asterivo.Unity60.Features.GameManagement.Interfaces
{
    /// <summary>
    /// ゲーム管理サービスのインターフェース
    /// ゲームの全体的な制御と状態管理を提供
    /// </summary>
    public interface IGameManager : IService
    {
        /// <summary>
        /// 現在のゲーム状態
        /// </summary>
        GameState CurrentGameState { get; }

        /// <summary>
        /// 前のゲーム状態
        /// </summary>
        GameState PreviousGameState { get; }

        /// <summary>
        /// ゲーム経過時間
        /// </summary>
        float GameTime { get; }

        /// <summary>
        /// ゲームがポーズ中かどうか
        /// </summary>
        bool IsPaused { get; }

        /// <summary>
        /// ゲームオーバー状態かどうか
        /// </summary>
        bool IsGameOver { get; }

        /// <summary>
        /// ゲーム状態を変更
        /// </summary>
        /// <param name="newState">新しいゲーム状態</param>
        void ChangeGameState(GameState newState);

        /// <summary>
        /// ゲームを開始
        /// </summary>
        void StartGame();

        /// <summary>
        /// ゲームを一時停止
        /// </summary>
        void PauseGame();

        /// <summary>
        /// ゲームを再開
        /// </summary>
        void ResumeGame();

        /// <summary>
        /// ゲームをリスタート
        /// </summary>
        void RestartGame();

        /// <summary>
        /// ゲームを終了
        /// </summary>
        void QuitGame();

        /// <summary>
        /// メインメニューに戻る
        /// </summary>
        void ReturnToMenu();

        /// <summary>
        /// ゲームオーバー処理
        /// </summary>
        void TriggerGameOver();

        /// <summary>
        /// 勝利処理
        /// </summary>
        void TriggerVictory();

        /// <summary>
        /// コマンドを実行
        /// </summary>
        /// <param name="command">実行するコマンド</param>
        void ExecuteCommand(ICommand command);

        /// <summary>
        /// 最後のコマンドを取り消し
        /// </summary>
        void UndoLastCommand();

        /// <summary>
        /// 最後に取り消したコマンドをやり直し
        /// </summary>
        void RedoLastCommand();

        /// <summary>
        /// ポーズ状態を切り替え
        /// </summary>
        void TogglePause();
    }
}
