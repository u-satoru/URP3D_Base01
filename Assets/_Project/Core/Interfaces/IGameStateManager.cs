using asterivo.Unity60.Core.Types; // GameState enum用

namespace asterivo.Unity60.Core.Services
{
    /// <summary>
    /// ゲーム状態統合管理インターフェース（階層化ステートマシン統合・3層アーキテクチャ対応）
    ///
    /// Unity 6における3層アーキテクチャのCore層ゲーム状態基盤において、
    /// アプリケーション全体のゲーム状態を一元管理する中核インターフェースです。
    /// 階層化ステートマシンパターンとGameState列挙型により、
    /// 明確な状態遷移、履歴管理、イベント駆動状態変更を実現します。
    ///
    /// 【ゲーム状態管理システム】
    /// - Current State: リアルタイムゲーム状態の即座取得
    /// - Previous State: 状態履歴による前状態への復帰支援
    /// - State History: 状態変更履歴の完全記録と分析
    /// - Game Over Detection: ゲーム終了状態の自動判定
    ///
    /// 【階層化ステートマシン統合】
    /// - HSM Integration: 階層化ステートマシンとの完全統合
    /// - State Nesting: ゲーム状態の階層的管理（メニュー→設定等）
    /// - Transition Rules: 状態遷移ルールの定義と検証
    /// - Event-Driven Changes: GameEventによる状態変更トリガー
    ///
    /// 【3層アーキテクチャ連携】
    /// - Core Layer Foundation: ゲーム状態の基盤管理機能提供
    /// - Feature Layer Integration: 具体機能での状態依存処理
    /// - Template Layer Configuration: ジャンル特化状態フロー定義
    /// - Cross-Layer Communication: 状態変更の全層通知システム
    ///
    /// 【GameState列挙型活用】
    /// - Type Safety: GameState列挙型による型安全な状態管理
    /// - State Validation: 不正状態遷移の自動検出と防止
    /// - Debug Support: エディタでの状態可視化とデバッグ支援
    /// - Serialization: ゲーム状態のセーブ・ロード対応
    ///
    /// 【パフォーマンス最適化】
    /// - O(1) State Access: 高速な状態取得と判定処理
    /// - Event Batching: 複数状態変更の効率的一括処理
    /// - Memory Efficient: 軽量な状態履歴管理
    /// - GC Friendly: ガベージコレクション負荷の最小化
    ///
    /// 【使用パターンとベストプラクティス】
    /// - State-Driven Logic: 状態ベースの条件分岐とロジック制御
    /// - Menu System: UI画面遷移との完全連携
    /// - Game Flow: メインゲーム→ポーズ→設定の状態フロー管理
    /// - Save System: ゲーム状態の永続化と復元システム
    ///
    /// 【Template層での活用例】
    /// - Stealth Template: 隠密→発見→追跡状態の管理
    /// - FPS Template: メニュー→ゲーム→リザルト状態の制御
    /// - RPG Template: フィールド→バトル→メニュー状態の切り替え
    /// - Horror Template: 探索→イベント→エンディング状態の進行
    /// </summary>
    public interface IGameStateManager
    {
        GameState CurrentGameState { get; }
        GameState PreviousGameState { get; }
        bool IsGameOver { get; }

        void ChangeGameState(GameState newState);
    }
}
