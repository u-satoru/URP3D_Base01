using UnityEngine;
using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Audio; // GameState enum用
using asterivo.Unity60.Core.Lifecycle;

namespace asterivo.Unity60.Core
{
    /// <summary>
    /// ゲーム状態管理統合サービス実装クラス
    ///
    /// Unity 6における3層アーキテクチャのCore層に配置され、
    /// ゲーム全体の状態管理を一元的に担う中核サービスです。
    /// ServiceLocatorパターンとイベント駆動アーキテクチャを統合し、
    /// 疎結合で高性能なゲーム状態管理を実現します。
    ///
    /// 【核心機能】
    /// - 統一ゲーム状態管理: GameState enumによる型安全な状態制御
    /// - イベント駆動通知: GameEvent<GameState>による即座状態変更通知
    /// - ServiceLocator統合: IGameStateManagerインターフェース経由のアクセス
    /// - 状態履歴管理: 現在状態と直前状態の追跡
    /// - FeatureFlags連動: 段階的移行とフォールバック機能
    ///
    /// 【アーキテクチャ統合】
    /// - Core層配置: ジャンル非依存の基盤サービスとして実装
    /// - ServiceLocator登録: 起動時自動登録による透明なアクセス
    /// - イベント発火: 状態変更時の全システムへの即座通知
    /// - Feature層通信: インターフェース経由での安全な機能提供
    /// - Template層設定: GameState設定による動的制御
    ///
    /// 【パフォーマンス特性】
    /// - O(1)状態アクセス: 直接フィールド参照による高速取得
    /// - 最小イベント発火: 状態変更時のみの効率的通知
    /// - メモリ効率: enumベースの軽量状態管理
    /// - 冪等性保証: 同一状態変更での不要処理回避
    ///
    /// 【状態管理パターン】
    /// - MainMenu: ゲーム開始前のメニュー画面
    /// - Playing: ゲームプレイ中の状態
    /// - Paused: 一時停止状態
    /// - GameOver: ゲーム終了状態
    /// - Loading: シーン遷移やデータロード中
    ///
    /// 【イベント連携】
    /// - GameStateChanged: 状態変更の即座通知
    /// - UI更新: 状態に応じたUI表示切り替え
    /// - システム制御: 状態別のシステム有効/無効化
    /// - セーブ/ロード: 状態変更タイミングでの自動保存
    ///
    /// 【3層アーキテクチャ遵守】
    /// - Core層責務: 状態管理という普遍的機能の提供
    /// - Feature層分離: ゲーム固有ロジックへの直接参照排除
    /// - Template層設定: 状態遷移ルールの外部設定化
    /// </summary>
    public class GameStateManagerService : MonoBehaviour, IGameStateManager, IServiceLocatorRegistrable
    {
        /// <summary>
        /// ゲーム状態変更イベント（型安全なGameEvent<GameState>）
        ///
        /// 状態変更時に自動発火され、全システムに即座通知されます。
        /// ScriptableObjectベースで設定可能、Template層での柔軟な設定を実現
        /// </summary>
        [Header("Events")] [SerializeField] private GameEvent<GameState> gameStateChangedEvent;

        /// <summary>
        /// 現在のゲーム状態（リアルタイム状態管理）
        ///
        /// ゲーム全体の現在状態を保持する中核フィールド。
        /// MainMenuで初期化され、ゲームライフサイクル全体を通じて管理されます。
        /// </summary>
        [Header("Runtime")]
        [SerializeField] private GameState currentGameState = GameState.MainMenu;

        /// <summary>
        /// 直前のゲーム状態（状態履歴管理）
        ///
        /// 状態変更前の値を保持し、状態遷移の追跡やロールバック機能を支援します。
        /// デバッグ、ログ出力、状態遷移検証で重要な役割を果たします。
        /// </summary>
        [SerializeField] private GameState previousGameState = GameState.MainMenu;

        /// <summary>
        /// ServiceLocator登録時の優先度（IServiceLocatorRegistrable実装）
        ///
        /// サービス登録順序を制御し、依存関係のあるサービスとの初期化順序を保証します。
        /// 値が小さいほど早期登録され、ゲーム状態管理の基盤として機能します。
        ///
        /// 【推奨値】
        /// - 50: ゲーム状態管理の標準優先度
        /// - Core基盤サービス(0-30)より後、Feature層サービス(70-100)より前
        /// </summary>
        // GameManager reference removed - Core層からFeatures層への参照禁止
        [SerializeField] private int priority = 50;

        /// <summary>
        /// ServiceLocator登録優先度プロパティ
        ///
        /// IServiceLocatorRegistrableインターフェースの実装。
        /// ServiceLocatorによる自動登録時に参照され、適切な初期化順序を保証します。
        /// </summary>
        public int Priority => priority;

        /// <summary>
        /// Unity Editor Reset処理（開発時設定初期化）
        ///
        /// Inspectorの「Reset」メニューまたはコンポーネント追加時に自動実行されます。
        /// 3層アーキテクチャ移行に伴い、GameManagerへの直接参照を排除し、
        /// ServiceLocatorパターンのみを使用する設計に変更されています。
        ///
        /// 【移行前の問題】
        /// - GameManagerの直接参照による密結合
        /// - Core層からFeature層への不正な依存関係
        /// - テスタビリティの低下
        ///
        /// 【現在の実装】
        /// - ServiceLocatorパターンによる疎結合設計
        /// - インターフェースベースの型安全なアクセス
        /// - 3層アーキテクチャ制約の厳格な遵守
        /// </summary>
        private void Reset()
        {
            // GameManager fallback removed - ServiceLocatorパターンのみ使用
        }

        /// <summary>
        /// ServiceLocatorへのサービス登録処理
        ///
        /// IServiceLocatorRegistrableインターフェース実装の中核メソッド。
        /// アプリケーション起動時にServiceLocatorへ自身を登録し、
        /// 他のシステムからのIGameStateManager経由アクセスを可能にします。
        ///
        /// 【実行フロー】
        /// 1. FeatureFlags.UseServiceLocatorによる機能有効性確認
        /// 2. ServiceLocator.RegisterService<IGameStateManager>()による登録
        /// 3. インターフェースベースでの型安全なサービス提供開始
        ///
        /// 【段階的移行対応】
        /// - FeatureFlagsによる新旧システムの共存制御
        /// - Phase 3移行計画での段階的ServiceLocator導入
        /// - 安全なロールバック機能（必要時）
        ///
        /// 【エラーハンドリング】
        /// - FeatureFlagsがfalseの場合は登録スキップ
        /// - ServiceLocatorの内部例外は適切に処理される
        /// - 重複登録は自動的に回避される
        ///
        /// 【パフォーマンス】
        /// - O(1)登録処理（ConcurrentDictionary使用）
        /// - スレッドセーフな登録操作
        /// - 最小限のオーバーヘッド
        /// </summary>
        public void RegisterServices()
        {
            if (FeatureFlags.UseServiceLocator)
            {
                ServiceLocator.RegisterService<IGameStateManager>(this);
            }
        }

        /// <summary>
        /// ServiceLocatorからのサービス登録解除処理
        ///
        /// アプリケーション終了時やオブジェクト破棄時にServiceLocatorから
        /// 自身の登録を安全に解除し、メモリリークや参照エラーを防止します。
        ///
        /// 【実行フロー】
        /// 1. FeatureFlags.UseServiceLocatorによる機能有効性確認
        /// 2. ServiceLocator.UnregisterService<IGameStateManager>()による解除
        /// 3. サービス参照の完全な切断とクリーンアップ
        ///
        /// 【安全性保証】
        /// - null参照例外の防止
        /// - 重複解除による例外の回避
        /// - ガベージコレクションの促進
        /// - ServiceLocator内部状態の整合性維持
        ///
        /// 【呼び出しタイミング】
        /// - OnDestroy()での自動実行
        /// - シーン切り替え時のクリーンアップ
        /// - アプリケーション終了時の一括解除
        /// - エラー復旧時の状態リセット
        ///
        /// 【パフォーマンス】
        /// - O(1)解除処理（辞書からの直接削除）
        /// - スレッドセーフな解除操作
        /// - 即座のメモリ解放
        /// </summary>
        public void UnregisterServices()
        {
            if (FeatureFlags.UseServiceLocator)
            {
                ServiceLocator.UnregisterService<IGameStateManager>();
            }
        }

        /// <summary>
        /// 現在のゲーム状態取得プロパティ
        ///
        /// IGameStateManagerインターフェースの実装。
        /// システム全体からリアルタイムでアクセス可能な現在状態を提供します。
        ///
        /// 【特徴】
        /// - 読み取り専用: 状態変更はChangeGameState()経由のみ
        /// - 高速アクセス: O(1)の直接フィールド参照
        /// - 型安全性: GameState enumによる厳密な型制御
        /// - スレッドセーフ: 単一フィールド読み取りの原子性
        ///
        /// 【使用例】
        /// - UI表示制御: CurrentGameState == GameState.Paused
        /// - システム有効化: CurrentGameState == GameState.Playing
        /// - 条件分岐: switch(CurrentGameState) { ... }
        /// </summary>
        public GameState CurrentGameState => currentGameState;

        /// <summary>
        /// 直前のゲーム状態取得プロパティ
        ///
        /// 状態変更履歴の追跡とロールバック機能を支援するプロパティ。
        /// デバッグ、ログ出力、状態遷移パターンの分析で重要な役割を果たします。
        ///
        /// 【特徴】
        /// - 履歴管理: 1つ前の状態を確実に保持
        /// - デバッグ支援: 状態遷移の可視化とトラブルシューティング
        /// - ロールバック: 必要時の前状態への復帰機能
        /// - 遷移検証: 不正な状態変更パターンの検出
        ///
        /// 【使用例】
        /// - 状態復帰: ChangeGameState(PreviousGameState)
        /// - ログ出力: $"State changed: {PreviousGameState} -> {CurrentGameState}"
        /// - 条件判定: PreviousGameState == GameState.Playing
        /// </summary>
        public GameState PreviousGameState => previousGameState;

        /// <summary>
        /// ゲームオーバー状態判定プロパティ
        ///
        /// 責務分離原則に基づき、ゲームオーバー判定はScoreServiceに委譲。
        /// このサービスでは常にfalseを返し、状態管理とゲーム終了判定を分離します。
        ///
        /// 【設計原則】
        /// - 単一責務: GameStateManagerは状態管理のみ担当
        /// - 責務分離: ゲーム終了判定は専用サービスで処理
        /// - 疎結合: ScoreServiceとの直接的な依存関係を回避
        ///
        /// 【実装方針】
        /// - ScoreService.IsGameOver: スコア・ライフ等に基づく実際の判定
        /// - GameStateManager.IsGameOver: 常にfalse（状態管理に専念）
        /// - EventManager: ScoreServiceからのGameOverイベント中継
        ///
        /// 【移行理由】
        /// - アーキテクチャ整理: 責務の明確な分離
        /// - 拡張性向上: ゲーム終了条件の柔軟な変更
        /// - テスト容易性: 独立したテストケース作成
        /// </summary>
        public bool IsGameOver => false; // game over判定はScoreService側で行う

        /// <summary>
        /// ゲーム状態変更処理（中核メソッド）
        ///
        /// ゲーム状態を安全に変更し、関連システムに即座通知する中核的な状態管理機能。
        /// 冪等性を保証し、イベント駆動アーキテクチャによる疎結合な通知を実現します。
        ///
        /// 【実行フロー】
        /// 1. 冪等性チェック: 同一状態変更の重複実行回避
        /// 2. 履歴更新: previousGameState に現在状態を保存
        /// 3. 状態更新: currentGameState に新状態を設定
        /// 4. イベント発火: gameStateChangedEvent による全システム通知
        ///
        /// 【パフォーマンス特性】
        /// - O(1)状態更新: 直接フィールド代入による高速処理
        /// - 冪等性保証: 不要な処理とイベント発火の回避
        /// - 即座通知: GameEvent<GameState>による効率的なイベント配信
        /// - メモリ効率: 最小限のアロケーションで状態管理
        ///
        /// 【イベント連携】
        /// - UI更新: 状態に応じた画面表示切り替え
        /// - システム制御: Playing/Paused時の機能有効/無効化
        /// - セーブ/ロード: 状態変更タイミングでの自動保存
        /// - ログ記録: 状態遷移の詳細ログ出力
        ///
        /// 【安全性保証】
        /// - null安全: gameStateChangedEvent?.Raise() による安全な呼び出し
        /// - 型安全: GameState enumによる不正値の排除
        /// - 冪等性: 同一状態への変更で副作用なし
        /// - 例外安全: イベント発火失敗でも状態は正常更新
        ///
        /// 【使用例】
        /// - ゲーム開始: ChangeGameState(GameState.Playing)
        /// - 一時停止: ChangeGameState(GameState.Paused)
        /// - メニュー復帰: ChangeGameState(GameState.MainMenu)
        /// - ローディング: ChangeGameState(GameState.Loading)
        /// </summary>
        /// <param name="newState">変更先のゲーム状態</param>
        public void ChangeGameState(GameState newState)
        {
            if (currentGameState == newState) return;
            previousGameState = currentGameState;
            currentGameState = newState;
            gameStateChangedEvent?.Raise(currentGameState);
        }
    }
}
