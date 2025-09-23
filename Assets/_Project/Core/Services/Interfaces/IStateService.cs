using System.Collections.Generic;
using asterivo.Unity60.Core.Patterns;

namespace asterivo.Unity60.Core.Services
{
    /// <summary>
    /// StateManagement機能のサービスインターフェース
    /// ServiceLocatorに登録して使用する
    /// </summary>
    public interface IStateService : IService
    {
        /// <summary>
        /// 状態ハンドラーを登録
        /// </summary>
        /// <param name="handler">登録するハンドラー</param>
        void RegisterHandler(IStateHandler handler);

        /// <summary>
        /// 状態ハンドラーを取得
        /// </summary>
        /// <param name="state">対象の状態(int型で疎結合)</param>
        /// <returns>対応するハンドラー、存在しない場合はnull</returns>
        IStateHandler GetHandler(int state);

        /// <summary>
        /// 指定した状態のハンドラーが登録されているかチェック
        /// </summary>
        /// <param name="state">チェックする状態</param>
        /// <returns>ハンドラーが存在する場合はtrue</returns>
        bool HasHandler(int state);

        /// <summary>
        /// 登録されている全ての状態を取得
        /// </summary>
        /// <returns>登録済み状態のコレクション</returns>
        IEnumerable<int> GetRegisteredStates();

        /// <summary>
        /// 全てのハンドラーをクリア
        /// </summary>
        void ClearHandlers();
    }
}