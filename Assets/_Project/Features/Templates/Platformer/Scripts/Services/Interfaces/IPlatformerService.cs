using System;

namespace asterivo.Unity60.Features.Templates.Platformer.Services
{
    /// <summary>
    /// Platformer Template サービス基底インターフェース
    /// ServiceLocator + Event駆動ハイブリッドアーキテクチャの核心
    /// 全Platformerサービスの共通契約を定義
    /// </summary>
    public interface IPlatformerService : IDisposable
    {
        /// <summary>
        /// サービス初期化状態
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// サービス有効状態
        /// </summary>
        bool IsEnabled { get; }

        /// <summary>
        /// サービス初期化：設定ベース・依存関係解決
        /// </summary>
        void Initialize();

        /// <summary>
        /// サービス有効化：実行時制御
        /// </summary>
        void Enable();

        /// <summary>
        /// サービス無効化：一時停止
        /// </summary>
        void Disable();

        /// <summary>
        /// サービス状態リセット：レベル切替・再初期化用
        /// </summary>
        void Reset();

        /// <summary>
        /// ServiceLocator統合検証：依存関係確認
        /// </summary>
        bool VerifyServiceLocatorIntegration();

        /// <summary>
        /// サービス実行状態更新：フレーム処理（必要に応じて）
        /// </summary>
        void UpdateService(float deltaTime);
    }
}
