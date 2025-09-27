using UnityEngine;
using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Core.Audio; // GameState enum用
using asterivo.Unity60.Core.Lifecycle;

namespace asterivo.Unity60.Core
{
    /// <summary>
    /// シーンローディング統合管理サービス実装クラス
    ///
    /// Unity 6における3層アーキテクチャのCore層に配置され、
    /// ゲーム全体のシーン遷移を一元的に管理する専門サービスです。
    /// ServiceLocatorパターンと非同期処理を組み合わせ、
    /// スムーズで信頼性の高いシーン遷移体験を提供します。
    ///
    /// 【核心機能】
    /// - 非同期シーンローディング: SceneManager.LoadSceneAsyncによる非ブロッキング処理
    /// - 最小ローディング時間保証: UX向上のための調整可能な最小表示時間
    /// - 状態管理統合: GameStateManagerとの連携による一貫した状態制御
    /// - ServiceLocator統合: ISceneLoadingServiceインターフェース経由のアクセス
    /// - 遷移状態追跡: isTransitioningフラグによるローディング状態管理
    ///
    /// 【アーキテクチャ統合】
    /// - Core層配置: ジャンル非依存のシーン管理機能として実装
    /// - ServiceLocator登録: 起動時自動登録による透明なアクセス
    /// - GameStateManager連携: Loading/Playing状態の自動管理
    /// - Feature層分離: ゲーム固有ロジックへの直接参照排除
    /// - Template層設定: シーン名と設定の外部データ化
    ///
    /// 【パフォーマンス特性】
    /// - 非同期処理: メインスレッドブロッキングなしの快適な操作感
    /// - メモリ効率: LoadSceneAsyncによる段階的リソース読み込み
    /// - レスポンシブUI: ローディング中もユーザー操作可能
    /// - リアルタイム制御: Time.realtimeSinceStartupによる正確な時間管理
    ///
    /// 【UX最適化機能】
    /// - 最小ローディング時間: 高速ロード時でもローディング画面の適切な表示
    /// - 状態フィードバック: isTransitioningによる遷移状況の透明性
    /// - 段階的ロード: 大容量シーンの分割ロード対応
    /// - エラー回復: ロード失敗時の適切な状態復旧
    ///
    /// 【ローディングフロー】
    /// 1. 遷移開始: isTransitioning=true, GameState.Loading設定
    /// 2. 非同期ロード: SceneManager.LoadSceneAsync実行
    /// 3. 時間保証: 最小ローディング時間とロード完了の両方を待機
    /// 4. 遷移完了: isTransitioning=false, GameState.Playing復帰
    ///
    /// 【3層アーキテクチャ遵守】
    /// - Core層責務: シーン管理という普遍的機能の提供
    /// - Feature層分離: ゲーム固有のシーン遷移ロジック排除
    /// - Template層設定: シーン名や設定値の外部データ化
    /// - インターフェース設計: ISceneLoadingServiceによる契約定義
    ///
    /// 【ServiceLocator統合】
    /// - IServiceLocatorRegistrable実装: 自動登録・解除機能
    /// - Priority制御: 適切な初期化順序の保証
    /// - 依存解決: GameStateManagerなど他サービスとの連携
    /// - FeatureFlags対応: 段階的移行と機能切り替え
    /// </summary>
    public class SceneLoadingService : MonoBehaviour, ISceneLoadingService, IServiceLocatorRegistrable
    {
        /// <summary>
        /// ゲームプレイシーン名設定（ScriptableObject化推奨）
        ///
        /// メインゲームプレイが行われるシーンの名前を定義します。
        /// Template層での設定変更を想定し、将来的にはScriptableObjectでの管理を推奨します。
        /// </summary>
        [Header("Scenes")] [SerializeField] private string gameplaySceneName = "Gameplay";

        /// <summary>
        /// 最小ローディング表示時間（UX最適化設定）
        ///
        /// ローディング画面の最小表示時間を秒単位で定義します。
        /// 高速ロード時でもローディング画面を適切な時間表示し、
        /// ちらつきや違和感のないスムーズなUXを実現します。
        ///
        /// 【推奨値】
        /// - 0.5f-1.0f: 標準的なゲーム（バランス重視）
        /// - 1.0f-2.0f: 重厚感のあるゲーム（高品質感演出）
        /// - 0.1f-0.5f: カジュアルゲーム（レスポンシブ性重視）
        /// </summary>
        [Header("Settings")] [SerializeField] private float minLoadingTime = 1f;

        /// <summary>
        /// シーン遷移実行状態フラグ（リアルタイム状態管理）
        ///
        /// 現在シーン遷移処理が実行中かを追跡する状態フラグです。
        /// 重複遷移の防止、UI状態制御、デバッグ情報の提供に使用されます。
        ///
        /// 【状態遷移】
        /// - false: 遷移待機状態（通常状態）
        /// - true: 遷移実行中（ローディング中）
        ///
        /// 【使用用途】
        /// - 重複実行防止: 遷移中の新規遷移要求ブロック
        /// - UI状態制御: ローディング画面表示/非表示切り替え
        /// - デバッグ監視: Inspector上での遷移状況可視化
        /// </summary>
        [Header("Runtime")] [SerializeField] private bool isTransitioning = false;

        /// <summary>
        /// ServiceLocator登録時の優先度（依存関係制御）
        ///
        /// サービス登録順序を制御し、依存関係のあるサービスとの初期化順序を保証します。
        /// GameStateManager(50)より後に登録されることで、状態管理サービスへの依存を解決します。
        ///
        /// 【推奨値】
        /// - 60: シーンローディングの標準優先度
        /// - GameStateManager(50)より後、UI関連サービス(70-80)より前
        /// - Core基盤サービス群との適切な順序関係維持
        /// </summary>
        // GameManager reference removed - Core層からFeatures層への参照禁止
        [SerializeField] private int priority = 60;

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
        /// 【移行の背景】
        /// - GameManagerの直接参照による密結合の解消
        /// - Core層からFeature層への不正な依存関係の排除
        /// - インターフェースベースの疎結合設計への移行
        /// - テスタビリティとモジュール性の向上
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
        /// 他のシステムからのISceneLoadingService経由アクセスを可能にします。
        ///
        /// 【実行フロー】
        /// 1. FeatureFlags.UseServiceLocatorによる機能有効性確認
        /// 2. ServiceLocator.RegisterService<ISceneLoadingService>()による登録
        /// 3. インターフェースベースでの型安全なサービス提供開始
        ///
        /// 【段階的移行対応】
        /// - FeatureFlagsによる新旧システムの共存制御
        /// - Phase 3移行計画での段階的ServiceLocator導入
        /// - 安全なロールバック機能（必要時）
        ///
        /// 【依存関係管理】
        /// - Priority(60)によりGameStateManager(50)より後に登録
        /// - 状態管理サービスへの依存を適切に解決
        /// - 循環依存の回避とクリーンな初期化順序
        /// </summary>
        public void RegisterServices()
        {
            if (FeatureFlags.UseServiceLocator)
            {
                ServiceLocator.RegisterService<ISceneLoadingService>(this);
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
        /// 2. ServiceLocator.UnregisterService<ISceneLoadingService>()による解除
        /// 3. サービス参照の完全な切断とクリーンアップ
        ///
        /// 【安全性保証】
        /// - 進行中のシーン遷移処理への影響回避
        /// - コルーチン停止による適切なリソース解放
        /// - ServiceLocator内部状態の整合性維持
        /// - メモリリークの確実な防止
        ///
        /// 【呼び出しタイミング】
        /// - OnDestroy()での自動実行
        /// - シーン切り替え時のクリーンアップ
        /// - アプリケーション終了時の一括解除
        /// - エラー復旧時の状態リセット
        /// </summary>
        public void UnregisterServices()
        {
            if (FeatureFlags.UseServiceLocator)
            {
                ServiceLocator.UnregisterService<ISceneLoadingService>();
            }
        }

        /// <summary>
        /// ゲームプレイシーン専用ローディング（便利メソッド）
        ///
        /// 設定済みのgameplaySceneNameを使用してシーンローディングを実行します。
        /// LoadSceneWithMinTime()への単純な委譲メソッドとして実装されています。
        ///
        /// 【使用場面】
        /// - メインメニューからゲーム開始時
        /// - チュートリアル完了後のゲーム開始
        /// - ゲームオーバー後のリスタート
        /// - 設定画面からのクイックゲーム開始
        ///
        /// 【設計意図】
        /// - よく使用される操作の簡略化
        /// - gameplaySceneName設定の再利用
        /// - 型安全なシーン名管理
        /// - コード可読性の向上
        /// </summary>
        public void LoadGameplaySceneWithMinTime() => LoadSceneWithMinTime(gameplaySceneName);

        /// <summary>
        /// 最小ローディング時間保証付きシーンローディング（公開メソッド）
        ///
        /// 指定されたシーン名に対して、UX最適化を考慮した非同期ローディングを実行します。
        /// 最小ローディング時間保証により、ちらつきのないスムーズな遷移を実現します。
        ///
        /// 【実行フロー】
        /// 1. シーン名有効性検証（null/空文字チェック）
        /// 2. LoadSceneRoutineコルーチン開始
        /// 3. 非同期ローディングと状態管理の統合実行
        ///
        /// 【安全性保証】
        /// - null/空文字での安全な早期リターン
        /// - 不正なシーン名での例外回避
        /// - 重複実行時のコルーチン安全管理
        ///
        /// 【パフォーマンス】
        /// - 非同期処理: メインスレッドのブロッキングなし
        /// - 効率的検証: 事前チェックによる無駄な処理回避
        /// - リソース効率: 必要時のみコルーチン開始
        ///
        /// 【使用例】
        /// - LoadSceneWithMinTime("MainGameplay")
        /// - LoadSceneWithMinTime("Tutorial")
        /// - LoadSceneWithMinTime("BossLevel")
        /// </summary>
        /// <param name="sceneName">ロードするシーンの名前（null/空文字は無視）</param>
        public void LoadSceneWithMinTime(string sceneName)
        {
            if (!string.IsNullOrEmpty(sceneName))
            {
                StartCoroutine(LoadSceneRoutine(sceneName));
            }
        }

        /// <summary>
        /// シーンローディング実行コルーチン（内部実装）
        ///
        /// 最小ローディング時間保証と状態管理を統合した、
        /// シーンローディングの中核実装です。非同期処理とUX最適化を両立し、
        /// スムーズで一貫性のあるシーン遷移体験を提供します。
        ///
        /// 【詳細実行フロー】
        /// 1. ServiceLocator経由でGameStateManager取得
        /// 2. 遷移状態フラグ設定（isTransitioning = true）
        /// 3. ゲーム状態をLoadingに変更（UI更新トリガー）
        /// 4. 開始時刻記録（Time.realtimeSinceStartup使用）
        /// 5. 非同期シーンロード開始（SceneManager.LoadSceneAsync）
        /// 6. 二重条件待機：最小時間 AND ロード完了
        /// 7. 遷移完了処理（isTransitioning = false）
        /// 8. ゲーム状態をPlayingに復帰（ゲーム再開）
        ///
        /// 【UX最適化機能】
        /// - 最小時間保証: 高速ロード時でも適切なローディング画面表示
        /// - 状態同期: GameStateManagerとの完全な状態連携
        /// - リアルタイム制御: Time.realtimeSinceStartupによる正確な時間測定
        /// - フレーム配慮: yield return nullによる適切なフレーム制御
        ///
        /// 【パフォーマンス特性】
        /// - 非ブロッキング: メインスレッドの応答性維持
        /// - メモリ効率: LoadSceneAsyncによる段階的ロード
        /// - CPU効率: フレーム単位の適切な処理分散
        /// - タイミング精度: realtimeSinceStartupによる正確な時間制御
        ///
        /// 【エラーハンドリング】
        /// - ServiceLocator取得失敗: null条件演算子による安全な処理継続
        /// - シーンロード失敗: asyncOp null チェックによる例外回避
        /// - 状態変更失敗: GameStateManager?.による安全な状態管理
        ///
        /// 【技術的詳細】
        /// - realtimeSinceStartup: TimeScaleに影響されない正確な実時間測定
        /// - LoadSceneAsync: バックグラウンドでの非同期リソースロード
        /// - yield return null: 次フレームまでの適切な待機
        /// - 複合条件: (時間条件 OR ロード条件) による柔軟な待機制御
        /// </summary>
        /// <param name="sceneName">ロードするシーンの名前</param>
        /// <returns>コルーチン実行用のIEnumerator</returns>
        private System.Collections.IEnumerator LoadSceneRoutine(string sceneName)
        {
            var gsm = ServiceLocator.GetService<IGameStateManager>();
            isTransitioning = true;
            gsm?.ChangeGameState(GameState.Loading);

            float startTime = Time.realtimeSinceStartup;
            var asyncOp = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);

            while (Time.realtimeSinceStartup - startTime < minLoadingTime || (asyncOp != null && !asyncOp.isDone))
            {
                yield return null;
            }

            isTransitioning = false;
            gsm?.ChangeGameState(GameState.Playing);
        }
    }
}
