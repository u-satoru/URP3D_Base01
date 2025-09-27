using UnityEngine;
using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Audio; // GameState enum用
using asterivo.Unity60.Core.Lifecycle;

namespace asterivo.Unity60.Core
{
    /// <summary>
    /// ゲームポーズ制御統合サービス実装クラス
    ///
    /// Unity 6における3層アーキテクチャのCore層に配置され、
    /// ゲーム全体のポーズ・一時停止機能を一元的に管理する専門サービスです。
    /// ServiceLocatorパターンとイベント駆動アーキテクチャを統合し、
    /// 疎結合で高性能なポーズ制御システムを実現します。
    ///
    /// 【核心機能】
    /// - 統一ポーズ制御: Time.timeScaleとGameStateの連動管理
    /// - イベント駆動通知: BoolGameEventによる即座状態変更通知
    /// - ServiceLocator統合: IPauseServiceインターフェース経由のアクセス
    /// - 状態同期管理: GameStateManagerとの自動連携
    /// - 冪等性保証: 同一状態変更での不要処理回避
    ///
    /// 【アーキテクチャ統合】
    /// - Core層配置: ジャンル非依存の基盤サービスとして実装
    /// - ServiceLocator登録: 起動時自動登録による透明なアクセス
    /// - イベント発火: 状態変更時の全システムへの即座通知
    /// - Feature層通信: インターフェース経由での安全な機能提供
    /// - GameStateManager連携: ポーズ状態の統合管理
    ///
    /// 【パフォーマンス特性】
    /// - O(1)状態アクセス: 直接フィールド参照による高速取得
    /// - 最小Time.timeScale変更: 状態変更時のみの効率的制御
    /// - メモリ効率: 軽量フィールドによる最小メモリフットプリント
    /// - 冪等性保証: 同一状態変更での不要処理回避
    ///
    /// 【ポーズ制御パターン】
    /// - Gameplay ⇄ Paused: 基本的なポーズ・復帰サイクル
    /// - Time.timeScale制御: 0f（完全停止）⇄ 1f（通常速度）
    /// - UI応答性維持: ポーズ中もUI操作は正常動作
    /// - オーディオ制御連携: BGM・SEの適切な一時停止
    ///
    /// 【イベント連携】
    /// - PauseStateChanged: ポーズ状態変更の即座通知
    /// - UI更新: ポーズメニューの表示/非表示切り替え
    /// - システム制御: ポーズ中の各システム動作調整
    /// - オーディオ管理: ポーズ状態に応じた音響制御
    ///
    /// 【3層アーキテクチャ遵守】
    /// - Core層責務: ポーズ制御という普遍的機能の提供
    /// - Feature層分離: ゲーム固有UIロジックへの直接参照排除
    /// - Template層設定: ポーズ動作の外部設定化
    /// </summary>
    public class PauseService : MonoBehaviour, IPauseService, IServiceLocatorRegistrable
    {
        /// <summary>
        /// ポーズ状態変更イベント（型安全なBoolGameEvent）
        ///
        /// ポーズ状態変更時に自動発火され、全システムに即座通知されます。
        /// ScriptableObjectベースで設定可能、Template層での柔軟な設定を実現
        /// UIシステム、オーディオシステム、入力システムが購読し、
        /// ポーズ状態に応じた適切な動作を自動実行します。
        /// </summary>
        [Header("Events")] [SerializeField] private BoolGameEvent onPauseStateChanged;

        /// <summary>
        /// ポーズ時のTime.timeScale制御フラグ
        ///
        /// trueの場合、ポーズ時にTime.timeScale = 0f、復帰時に1fに設定されます。
        /// falseの場合、Time.timeScaleは変更されず、手動制御が可能です。
        /// 一部のゲームジャンル（リアルタイム戦略ゲームなど）では、
        /// Time.timeScaleを独自制御したい場合があるため、設定可能にしています。
        /// </summary>
        [Header("Settings")] [SerializeField] private bool pauseTimeOnPause = true;

        /// <summary>
        /// 現在のポーズ状態（リアルタイム状態管理）
        ///
        /// ゲーム全体の現在ポーズ状態を保持する中核フィールド。
        /// false（通常再生）で初期化され、ゲームライフサイクル全体を通じて管理されます。
        /// 外部からは IsPaused プロパティ経由でのみアクセス可能です。
        /// </summary>
        [Header("Runtime")] [SerializeField] private bool isPaused = false;

        /// <summary>
        /// ServiceLocator登録時の優先度（IServiceLocatorRegistrable実装）
        ///
        /// サービス登録順序を制御し、依存関係のあるサービスとの初期化順序を保証します。
        /// 値が小さいほど早期登録され、ポーズ制御基盤として機能します。
        ///
        /// 【推奨値】
        /// - 80: ポーズサービスの標準優先度
        /// - Core基盤サービス(0-30)より後、UI層サービス(90-100)より前
        /// - GameStateManager(50)との連携を考慮した順序設定
        /// </summary>
        // GameManager reference removed - Core層からFeatures層への参照禁止
        [SerializeField] private int priority = 80;

        /// <summary>
        /// ServiceLocator登録優先度プロパティ
        ///
        /// IServiceLocatorRegistrableインターフェースの実装。
        /// ServiceLocatorによる自動登録時に参照され、適切な初期化順序を保証します。
        /// ポーズサービスは他のUIやオーディオサービスより先に初期化される必要があるため、
        /// 中間的な優先度（80）を設定しています。
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
        /// 他のシステムからのIPauseService経由アクセスを可能にします。
        ///
        /// 【実行フロー】
        /// 1. FeatureFlags.UseServiceLocatorによる機能有効性確認
        /// 2. ServiceLocator.RegisterService<IPauseService>()による登録
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
                ServiceLocator.RegisterService<IPauseService>(this);
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
        /// 2. ServiceLocator.UnregisterService<IPauseService>()による解除
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
                ServiceLocator.UnregisterService<IPauseService>();
            }
        }

        /// <summary>
        /// 現在のポーズ状態取得プロパティ
        ///
        /// IPauseServiceインターフェースの実装。
        /// システム全体からリアルタイムでアクセス可能な現在ポーズ状態を提供します。
        ///
        /// 【特徴】
        /// - 読み取り専用: 状態変更はSetPauseState()経由のみ
        /// - 高速アクセス: O(1)の直接フィールド参照
        /// - 型安全性: bool型による明確な状態表現
        /// - スレッドセーフ: 単一フィールド読み取りの原子性
        ///
        /// 【使用例】
        /// - UI表示制御: IsPaused == true でポーズメニュー表示
        /// - 入力制御: IsPaused == false でゲーム入力有効化
        /// - 条件分岐: if (IsPaused) { /* ポーズ中処理 */ }
        /// </summary>
        public bool IsPaused => isPaused;

        /// <summary>
        /// ポーズ状態切り替えメソッド（トグル実装）
        ///
        /// 現在のポーズ状態を反転させる便利メソッド。
        /// 内部的にSetPauseState(!isPaused)を呼び出し、
        /// 完全な状態管理と通知システムを活用します。
        ///
        /// 【動作パターン】
        /// - 通常再生中 → ポーズ状態へ遷移
        /// - ポーズ中 → 通常再生へ復帰
        ///
        /// 【使用場面】
        /// - キーボード入力（Escキーなど）による一発切り替え
        /// - UI Button.OnClick イベントでの簡単実装
        /// - デバッグ用の開発者コマンド
        /// </summary>
        public void TogglePause() => SetPauseState(!isPaused);

        /// <summary>
        /// ポーズ状態設定処理（中核メソッド）
        ///
        /// ポーズ状態を安全に変更し、関連システムに即座通知する中核的なポーズ管理機能。
        /// 冪等性を保証し、Time.timeScale制御、GameState連携、イベント発火を統合管理します。
        ///
        /// 【実行フロー】
        /// 1. 冪等性チェック: 同一状態変更の重複実行回避
        /// 2. 状態更新: isPaused フィールドの更新
        /// 3. Time.timeScale制御: pauseTimeOnPauseフラグに応じた時間制御
        /// 4. GameState連携: ServiceLocator経由でのGameStateManager通知
        /// 5. イベント発火: onPauseStateChangedによる全システム通知
        ///
        /// 【Time.timeScale制御】
        /// - ポーズ時: Time.timeScale = 0f（完全停止）
        /// - 復帰時: Time.timeScale = 1f（通常速度）
        /// - 無効時: Time.timeScaleは変更されない（手動制御）
        ///
        /// 【GameState連携】
        /// - ポーズ時: GameState.Paused への遷移
        /// - 復帰時: GameState.Gameplay への復帰
        /// - ServiceLocator経由での安全なアクセス
        ///
        /// 【イベント通知】
        /// - BoolGameEvent による型安全な状態通知
        /// - 購読システムへの即座反映
        /// - UI、オーディオ、入力システムの自動調整
        ///
        /// 【パフォーマンス特性】
        /// - O(1)状態更新: 直接フィールド代入による高速処理
        /// - 冪等性保証: 不要な処理とイベント発火の回避
        /// - 即座通知: ServiceLocatorとGameEventによる効率的な通信
        /// - メモリ効率: 最小限のアロケーションで状態管理
        ///
        /// 【エラーハンドリング】
        /// - null安全: ServiceLocator.GetService()?.ChangeGameState() による安全な呼び出し
        /// - イベント安全: onPauseStateChanged?.Raise() による安全な発火
        /// - 例外安全: 個別処理失敗でも状態は正常更新
        ///
        /// 【使用例】
        /// - ポーズ実行: SetPauseState(true)
        /// - 復帰実行: SetPauseState(false)
        /// - 条件付きポーズ: SetPauseState(shouldPause)
        /// </summary>
        /// <param name="paused">設定するポーズ状態（true: ポーズ, false: 通常再生）</param>
        public void SetPauseState(bool paused)
        {
            if (isPaused == paused) return;
            isPaused = paused;
            if (pauseTimeOnPause)
            {
                Time.timeScale = paused ? 0f : 1f;
            }
            var gsm = ServiceLocator.GetService<IGameStateManager>();
            gsm?.ChangeGameState(paused ? GameState.Paused : GameState.Gameplay);
            onPauseStateChanged?.Raise(isPaused);
        }

        /// <summary>
        /// ゲーム復帰専用メソッド（明示的復帰実装）
        ///
        /// ポーズ状態から通常再生状態への復帰を明示的に実行する便利メソッド。
        /// 内部的にSetPauseState(false)を呼び出し、
        /// 完全な復帰処理と通知システムを活用します。
        ///
        /// 【SetPauseState(false)との違い】
        /// - 意図の明確化: 復帰専用の意味的な名前
        /// - API一貫性: ポーズ・復帰の対称的なインターフェース
        /// - 可読性向上: コードの意図がより明確
        ///
        /// 【使用場面】
        /// - ポーズメニューの「Resume」ボタン
        /// - ゲーム復帰イベントの明示的処理
        /// - チュートリアルやカットシーン終了後の復帰
        /// - 外部システムからの復帰指示
        /// </summary>
        public void ResumeGame() => SetPauseState(false);
    }
}
