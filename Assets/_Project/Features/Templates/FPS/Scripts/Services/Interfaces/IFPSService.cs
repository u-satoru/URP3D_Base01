using System;

namespace asterivo.Unity60.Features.Templates.FPS.Services
{
    /// <summary>
    /// FPS Template サービス基底インターフェース
    /// ServiceLocator + Event駆動ハイブリッドアーキテクチャの核心
    /// 全FPSサービスの共通契約を定義
    /// 詳細設計書準拠：一人称シューティング特化のサービス基盤
    /// </summary>
    public interface IFPSService : IDisposable
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
        /// サービス状態リセット：ラウンド切替・再初期化用
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
