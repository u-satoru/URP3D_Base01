using UnityEngine;
// using asterivo.Unity60.Core.Player; // Removed to avoid circular dependency

namespace asterivo.Unity60.Core.Patterns
{
    /// <summary>
    /// Strategy パターン状態処理インターフェース（循環依存回避・3層アーキテクチャ対応）
    ///
    /// Unity 6における3層アーキテクチャのCore層状態管理システムにおいて、
    /// Strategy パターンによる状態処理の統一インターフェースを定義します。
    /// 循環依存の回避とアーキテクチャの疎結合化により、
    /// 拡張性と保守性の高い状態管理システムを実現します。
    ///
    /// 【Strategy パターン実装】
    /// - アルゴリズム分離: 各状態の処理ロジックを独立したStrategyとして実装
    /// - 動的切り替え: 実行時での状態処理アルゴリズムの動的変更対応
    /// - 拡張性: 新規状態追加時の既存コードへの影響最小化
    /// - カプセル化: 状態固有処理の適切な隠蔽と責務分離
    ///
    /// 【循環依存回避設計】
    /// - int型状態ID: PlayerState等の具体型依存を排除
    /// - インターフェース分離: コンテキストとハンドラーの適切な分離
    /// - 疎結合アーキテクチャ: Core層の独立性確保
    /// - 型安全性維持: 実行時型チェックによる安全性確保
    ///
    /// 【3層アーキテクチャ統合】
    /// - Core層基盤: ジャンル非依存の状態処理機構として配置
    /// - Feature層活用: 具体的状態ハンドラーの実装基盤提供
    /// - Template層設定: ジャンル特化状態シーケンスの構築支援
    /// - 依存方向制御: 下位層から上位層への一方向依存維持
    ///
    /// 【状態管理統合】
    /// - ライフサイクル管理: OnEnter/OnExit による適切な状態遷移制御
    /// - コンテキスト活用: IStateContext による状態間情報共有
    /// - デバッグ支援: 統一ログ出力による状態遷移の可視化
    /// - パフォーマンス最適化: 軽量インターフェースによる高速状態処理
    ///
    /// 【実装パターン】
    /// - Registry連携: StateHandlerRegistry による状態ハンドラー管理
    /// - ファクトリー統合: 状態ハンドラーの動的生成・登録
    /// - コマンド統合: コマンドパターンとの協調による複合動作
    /// - イベント統合: 状態変化イベントの統一通知機構
    /// </summary>
    public interface IStateHandler
    {
        /// <summary>
        /// 状態に入る際の処理
        /// </summary>
        /// <param name="context">状態のコンテキスト</param>
        void OnEnter(IStateContext context);
        
        /// <summary>
        /// 状態から出る際の処理
        /// </summary>
        /// <param name="context">状態のコンテキスト</param>
        void OnExit(IStateContext context);
        
        /// <summary>
        /// 処理対象の状態
        /// </summary>
        int HandledState { get; } // Changed from PlayerState to int to avoid circular dependency
    }
    
    /// <summary>
    /// 状態処理統合コンテキストインターフェース（疎結合・デバッグ統合）
    ///
    /// Unity 6における3層アーキテクチャのCore層状態管理システムにおいて、
    /// 状態ハンドラーが実行時に参照する統一コンテキストインターフェースです。
    /// 疎結合設計とデバッグ機能統合により、
    /// 状態間での安全な情報共有とシステム監視を実現します。
    ///
    /// 【コンテキスト責務】
    /// - 状態間情報共有: 複数状態で必要な共通情報への統一アクセス
    /// - デバッグ制御: 実行時デバッグレベルの動的制御
    /// - ログ統合: 状態遷移ログの統一出力システム
    /// - 疎結合維持: 状態ハンドラーと具体実装の分離
    ///
    /// 【デバッグシステム統合】
    /// - 実行時制御: IsDebugEnabled による動的デバッグ切り替え
    /// - 統一ログ: Log() メソッドによる一貫したログ出力
    /// - パフォーマンス配慮: デバッグ無効時の処理オーバーヘッド最小化
    /// - 開発支援: 状態遷移の可視化とトラブルシューティング支援
    ///
    /// 【アーキテクチャ統合】
    /// - ServiceLocator連携: ログサービスとの統合対応
    /// - イベント連携: 状態変化イベント発行の基盤提供
    /// - 設定駆動: FeatureFlags による機能制御対応
    /// - 拡張性: 新規コンテキスト情報の追加容易性確保
    /// </summary>
    public interface IStateContext
    {
        /// <summary>
        /// デバッグログが有効かどうか
        /// </summary>
        bool IsDebugEnabled { get; }
        
        /// <summary>
        /// ログメッセージを出力
        /// </summary>
        /// <param name="message">ログメッセージ</param>
        void Log(string message);
    }
}
