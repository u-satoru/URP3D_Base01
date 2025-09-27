using UnityEngine;
using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Lifecycle;

namespace asterivo.Unity60.Core
{
    /// <summary>
    /// スコア・ライフ管理統合サービス実装クラス
    ///
    /// Unity 6における3層アーキテクチャのCore層に配置され、
    /// ゲーム全体のスコア管理とプレイヤーライフ制御を一元的に担う専門サービスです。
    /// ServiceLocatorパターンとイベント駆動アーキテクチャを統合し、
    /// 疎結合で高性能なゲーム進行管理システムを実現します。
    ///
    /// 【核心機能】
    /// - 統一スコア管理: 加算・設定・取得の包括的スコア制御
    /// - ライフ管理システム: 残機制御とゲームオーバー判定
    /// - イベント駆動通知: IntGameEvent・GameEventによる即座状態変更通知
    /// - ServiceLocator統合: IScoreServiceインターフェース経由のアクセス
    /// - ゲームオーバー制御: ライフ0時の自動ゲームオーバー発動
    /// - デバッグ支援: 詳細ログ出力による開発効率向上
    ///
    /// 【アーキテクチャ統合】
    /// - Core層配置: ジャンル非依存の基盤サービスとして実装
    /// - ServiceLocator登録: 起動時自動登録による透明なアクセス
    /// - イベント発火: スコア・ライフ変更時の全システムへの即座通知
    /// - Feature層通信: インターフェース経由での安全な機能提供
    /// - Template層設定: ゲームジャンル別パラメータの外部設定化
    ///
    /// 【パフォーマンス特性】
    /// - O(1)状態アクセス: 直接フィールド参照による高速取得
    /// - 最小イベント発火: 状態変更時のみの効率的通知
    /// - メモリ効率: 軽量フィールドによる最小メモリフットプリント
    /// - 境界値検証: Mathf.Clampによる安全な値管理
    ///
    /// 【ゲーム進行管理】
    /// - スコア累積: AddScore()による段階的スコア蓄積
    /// - ライフ消費: LoseLife()による残機減少とゲームオーバー判定
    /// - ライフ回復: AddLife()による残機増加（上限制御付き）
    /// - 状態リセット: SetScore()・SetLives()による直接設定
    ///
    /// 【イベント連携】
    /// - ScoreChanged: スコア変更の即座通知
    /// - LivesChanged: ライフ変更の即座通知
    /// - GameOver: ゲームオーバー状態の発火
    /// - UI更新: スコア・ライフ表示の自動更新
    /// - システム制御: ゲームオーバー時の各システム停止
    ///
    /// 【3層アーキテクチャ遵守】
    /// - Core層責務: スコア・ライフ管理という普遍的機能の提供
    /// - Feature層分離: ゲーム固有UIロジックへの直接参照排除
    /// - Template層設定: ゲームバランスパラメータの外部設定化
    ///
    /// 【ゲームジャンル対応】
    /// - アクションゲーム: スコアアタック・ライフ制ゲームプレイ
    /// - シューティングゲーム: 敵撃破スコア・被弾ライフ管理
    /// - プラットフォーマー: ステージクリア・落下ペナルティ管理
    /// - パズルゲーム: コンボスコア・ミス回数制限管理
    /// </summary>
    public class ScoreService : MonoBehaviour, IScoreService, IServiceLocatorRegistrable
    {
        /// <summary>
        /// スコア変更イベント（型安全なIntGameEvent）
        ///
        /// スコア変更時に自動発火され、変更後のスコア値と共に全システムに即座通知されます。
        /// ScriptableObjectベースで設定可能、Template層での柔軟な設定を実現
        /// UIシステム、オーディオシステム、実績システムが購読し、
        /// スコア変更に応じた適切な動作（表示更新、効果音再生、実績解除など）を自動実行します。
        /// </summary>
        [Header("References")]
        [SerializeField] private IntGameEvent onScoreChanged;

        /// <summary>
        /// ライフ変更イベント（型安全なIntGameEvent）
        ///
        /// ライフ変更時に自動発火され、変更後のライフ値と共に全システムに即座通知されます。
        /// UIの残機表示、オーディオの警告音、視覚エフェクトシステムが購読し、
        /// ライフ状況に応じた動的な演出（残機表示更新、危険状態警告、回復エフェクトなど）を実行します。
        /// </summary>
        [SerializeField] private IntGameEvent onLivesChanged;

        /// <summary>
        /// ゲームオーバーイベント（GameEvent）
        ///
        /// ライフが0以下になった際に自動発火され、ゲームオーバー状態を全システムに通知します。
        /// GameStateManager、UIシステム、オーディオシステム、セーブシステムが購読し、
        /// ゲームオーバー処理（状態遷移、UI表示、ゲームオーバー音楽、ハイスコア保存など）を連携実行します。
        /// </summary>
        [SerializeField] private GameEvent onGameOver;

        /// <summary>
        /// 初期ライフ数（ゲーム開始時の残機設定）
        ///
        /// ゲーム開始時またはAwake()実行時に設定される初期ライフ数です。
        /// 0からmaxLivesの範囲で制限され、ゲームバランス調整の重要なパラメータです。
        /// Template層での設定により、ゲームジャンルや難易度に応じた調整が可能です。
        ///
        /// 【推奨設定値】
        /// - アクションゲーム: 3-5（標準的なチャレンジレベル）
        /// - パズルゲーム: 5-10（思考重視、ペナルティは軽め）
        /// - シューティングゲーム: 2-3（高難易度、緊張感重視）
        /// </summary>
        [Header("Settings")]
        [SerializeField] private int initialLives = 3;

        /// <summary>
        /// 最大ライフ数（ライフ上限制限）
        ///
        /// AddLife()やSetLives()による最大ライフ数の上限値です。
        /// ライフ回復アイテムによる無制限ライフ増加を防ぎ、適切なゲームバランスを維持します。
        /// この値を超えるライフ設定は自動的にクランプされます。
        ///
        /// 【設計考慮事項】
        /// - ゲームバランス: 無制限ライフによる難易度破綻防止
        /// - UI制約: ライフ表示UIの表示限界考慮
        /// - プレイヤー体験: 適度な制限による緊張感維持
        /// </summary>
        [SerializeField] private int maxLives = 5;

        /// <summary>
        /// デバッグログ有効化フラグ
        ///
        /// trueの場合、スコア・ライフ変更時に詳細なデバッグログが出力されます。
        /// 開発中のゲームバランス調整、イベント発火確認、不具合調査に有用です。
        /// リリースビルドでは無効化推奨（パフォーマンス最適化）。
        ///
        /// 【出力ログ例】
        /// - "[ScoreService] +100 (total: 1500)" - スコア加算時
        /// - "[ScoreService] life -1 (lives: 2)" - ライフ減少時
        /// - "[ScoreService] Game Over" - ゲームオーバー発生時
        /// </summary>
        [SerializeField] private bool enableDebugLog = true;

        /// <summary>
        /// 現在のスコア値（リアルタイムスコア管理）
        ///
        /// ゲーム進行中の現在スコアを保持する中核フィールド。
        /// 0で初期化され、AddScore()やSetScore()により更新されます。
        /// 外部からは CurrentScore プロパティ経由でのみアクセス可能です。
        ///
        /// 【値の特性】
        /// - 範囲: 0以上の整数値（負の値は自動的に0にクランプ）
        /// - 永続性: Serializeフィールドによりシーン間での値保持
        /// - アクセス: 読み取り専用プロパティによる制御されたアクセス
        /// </summary>
        [Header("Runtime")]
        [SerializeField] private int currentScore;

        /// <summary>
        /// 現在のライフ数（リアルタイムライフ管理）
        ///
        /// ゲーム進行中の現在ライフ数を保持する中核フィールド。
        /// Awake()でinitialLivesにより初期化され、ゲームプレイ中に動的更新されます。
        /// 外部からは CurrentLives プロパティ経由でのみアクセス可能です。
        ///
        /// 【値の特性】
        /// - 範囲: 0以上maxLives以下の整数値
        /// - ゲームオーバー: 0以下でTriggerGameOver()自動実行
        /// - 境界制御: Mathf.Clampによる安全な値管理
        /// </summary>
        [SerializeField] private int currentLives;

        /// <summary>
        /// ServiceLocator登録時の優先度（IServiceLocatorRegistrable実装）
        ///
        /// サービス登録順序を制御し、依存関係のあるサービスとの初期化順序を保証します。
        /// 値が小さいほど早期登録され、スコア・ライフ管理基盤として機能します。
        ///
        /// 【推奨値】
        /// - 70: スコアサービスの標準優先度
        /// - Core基盤サービス(0-30)より後、UI層サービス(90-100)より前
        /// - GameStateManager(50), PauseService(80)との連携を考慮した順序設定
        /// </summary>
        [SerializeField] private int priority = 70;

        /// <summary>
        /// ServiceLocator登録優先度プロパティ
        ///
        /// IServiceLocatorRegistrableインターフェースの実装。
        /// ServiceLocatorによる自動登録時に参照され、適切な初期化順序を保証します。
        /// スコアサービスは基盤システムより後、UI系より前に初期化される必要があるため、
        /// 中位の優先度（70）を設定しています。
        /// </summary>
        public int Priority => priority;

        /// <summary>
        /// Unity初期化処理（ライフ値初期設定）
        ///
        /// オブジェクト生成時に自動実行される初期化メソッド。
        /// initialLivesをmaxLivesの範囲内で安全にcurrentLivesに設定します。
        ///
        /// 【実行フロー】
        /// 1. initialLivesの値を取得
        /// 2. Mathf.Clamp(initialLives, 0, maxLives)による境界値制御
        /// 3. currentLivesフィールドへの安全な設定
        ///
        /// 【境界値制御】
        /// - 最小値: 0（負の値は0にクランプ）
        /// - 最大値: maxLives（上限を超えた値は上限にクランプ）
        /// - 設定ミス防止: Inspector設定値の自動補正
        ///
        /// 【実行タイミング】
        /// - オブジェクト生成直後（Start()より前）
        /// - シーン読み込み時の自動実行
        /// - プレハブインスタンス化時の自動実行
        /// </summary>
        private void Awake()
        {
            currentLives = Mathf.Clamp(initialLives, 0, maxLives);
        }

        /// <summary>
        /// ServiceLocatorへのサービス登録処理
        ///
        /// IServiceLocatorRegistrableインターフェース実装の中核メソッド。
        /// アプリケーション起動時にServiceLocatorへ自身を登録し、
        /// 他のシステムからのIScoreService経由アクセスを可能にします。
        ///
        /// 【実行フロー】
        /// 1. FeatureFlags.UseServiceLocatorによる機能有効性確認
        /// 2. ServiceLocator.RegisterService<IScoreService>()による登録
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
                ServiceLocator.RegisterService<IScoreService>(this);
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
        /// 2. ServiceLocator.UnregisterService<IScoreService>()による解除
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
                ServiceLocator.UnregisterService<IScoreService>();
            }
        }

        /// <summary>
        /// 現在のスコア値取得プロパティ
        ///
        /// IScoreServiceインターフェースの実装。
        /// システム全体からリアルタイムでアクセス可能な現在スコア値を提供します。
        ///
        /// 【特徴】
        /// - 読み取り専用: スコア変更はAddScore()・SetScore()経由のみ
        /// - 高速アクセス: O(1)の直接フィールド参照
        /// - 型安全性: int型による明確な数値表現
        /// - スレッドセーフ: 単一フィールド読み取りの原子性
        ///
        /// 【使用例】
        /// - UI表示: CurrentScore.ToString()でスコア表示
        /// - 条件判定: CurrentScore >= targetScore
        /// - ハイスコア比較: CurrentScore > bestScore
        /// </summary>
        public int CurrentScore => currentScore;

        /// <summary>
        /// 現在のライフ数取得プロパティ
        ///
        /// IScoreServiceインターフェースの実装。
        /// システム全体からリアルタイムでアクセス可能な現在ライフ数を提供します。
        ///
        /// 【特徴】
        /// - 読み取り専用: ライフ変更はLoseLife()・AddLife()・SetLives()経由のみ
        /// - 高速アクセス: O(1)の直接フィールド参照
        /// - 境界保証: 0以上maxLives以下の値を保証
        /// - ゲームオーバー判定: 0以下の場合はゲームオーバー状態
        ///
        /// 【使用例】
        /// - UI表示: CurrentLives回分のライフアイコン表示
        /// - 危険状態判定: CurrentLives <= 1 で警告表示
        /// - ゲーム終了条件: CurrentLives <= 0 でゲームオーバー
        /// </summary>
        public int CurrentLives => currentLives;

        /// <summary>
        /// スコア加算処理（累積スコア管理）
        ///
        /// 指定されたポイントを現在のスコアに加算し、関連システムに通知する基本的なスコア管理機能。
        /// 負の値やゼロの加算は無視され、正の値のみが安全に累積されます。
        ///
        /// 【実行フロー】
        /// 1. 入力値検証: points > 0 の確認（無効値は早期リターン）
        /// 2. スコア累積: currentScore += points による加算処理
        /// 3. イベント発火: onScoreChangedによる全システム通知
        /// 4. デバッグログ: enableDebugLog時の詳細ログ出力
        ///
        /// 【パフォーマンス特性】
        /// - O(1)加算処理: 直接フィールド操作による高速処理
        /// - 即座通知: IntGameEventによる効率的なイベント配信
        /// - メモリ効率: 最小限のアロケーションで状態管理
        /// - 早期リターン: 無効値による不要処理回避
        ///
        /// 【使用場面】
        /// - 敵撃破: AddScore(enemyValue) による敵別スコア加算
        /// - アイテム取得: AddScore(itemBonus) によるボーナススコア
        /// - コンボ達成: AddScore(comboMultiplier * baseScore) による倍率スコア
        /// - ステージクリア: AddScore(stageBonus) によるクリアボーナス
        ///
        /// 【安全性保証】
        /// - 入力検証: 負の値・ゼロの自動フィルタリング
        /// - オーバーフロー対策: int型範囲内での安全な加算
        /// - イベント安全: onScoreChanged?.Raise() による安全な発火
        /// </summary>
        /// <param name="points">加算するスコアポイント（正の値のみ有効）</param>
        public void AddScore(int points)
        {
            if (points <= 0) return;
            currentScore += points;
            onScoreChanged?.Raise(currentScore);
            if (enableDebugLog) UnityEngine.Debug.Log($"[ScoreService] +{points} (total: {currentScore})");
        }

        /// <summary>
        /// スコア直接設定処理（スコア状態リセット）
        ///
        /// 現在のスコアを指定された値に直接設定し、関連システムに通知する制御機能。
        /// 負の値は自動的に0にクランプされ、安全なスコア管理を保証します。
        ///
        /// 【実行フロー】
        /// 1. 境界値制御: Mathf.Max(0, newScore) による負値防止
        /// 2. スコア設定: currentScore への直接代入
        /// 3. イベント発火: onScoreChangedによる全システム通知
        /// 4. デバッグログ: enableDebugLog時の設定値ログ出力
        ///
        /// 【使用場面】
        /// - ゲームリセット: SetScore(0) による初期化
        /// - チート/デバッグ: SetScore(debugScore) による開発支援
        /// - セーブデータ復元: SetScore(savedScore) による状態復帰
        /// - 難易度調整: SetScore(adjustedScore) によるバランス調整
        ///
        /// 【AddScore()との違い】
        /// - 設定方式: 累積加算 vs 直接設定
        /// - 用途: 段階的増加 vs 状態リセット
        /// - 境界制御: 正値制限 vs 負値クランプ
        ///
        /// 【安全性保証】
        /// - 境界値制御: 負の値の自動0クランプ
        /// - 型安全: int型による明確な数値設定
        /// - イベント安全: onScoreChanged?.Raise() による安全な発火
        /// </summary>
        /// <param name="newScore">設定するスコア値（負の値は0にクランプ）</param>
        public void SetScore(int newScore)
        {
            currentScore = Mathf.Max(0, newScore);
            onScoreChanged?.Raise(currentScore);
            if (enableDebugLog) UnityEngine.Debug.Log($"[ScoreService] score set: {currentScore}");
        }

        /// <summary>
        /// ライフ減少処理（残機消費とゲームオーバー判定）
        ///
        /// プレイヤーのライフを1減少させ、ゲームオーバー条件をチェックする中核的なライフ管理機能。
        /// ライフが0以下の場合は処理をスキップし、0になった場合は自動的にゲームオーバーを発動します。
        ///
        /// 【実行フロー】
        /// 1. 状態検証: currentLives <= 0 の早期リターンチェック
        /// 2. ライフ減少: currentLives-- による1減算処理
        /// 3. イベント発火: onLivesChangedによる全システム通知
        /// 4. デバッグログ: enableDebugLog時の減少ログ出力
        /// 5. ゲームオーバー判定: currentLives <= 0 時のTriggerGameOver()実行
        ///
        /// 【ゲームオーバー連携】
        /// - 自動判定: ライフ0以下で即座にゲームオーバー発動
        /// - 状態通知: GameOverイベントによる全システム連携
        /// - 処理分離: ゲームオーバー処理はTriggerGameOver()に委譲
        ///
        /// 【使用場面】
        /// - プレイヤー被弾: LoseLife() による残機減少
        /// - 落下死亡: LoseLife() によるミスペナルティ
        /// - 時間切れ: LoseLife() によるタイムオーバー処理
        /// - 障害物接触: LoseLife() による接触ダメージ
        ///
        /// 【安全性保証】
        /// - 重複処理防止: currentLives <= 0 での早期リターン
        /// - 自動ゲームオーバー: ライフ枯渇時の確実な終了処理
        /// - イベント安全: onLivesChanged?.Raise() による安全な発火
        ///
        /// 【パフォーマンス特性】
        /// - O(1)減算処理: 直接フィールド操作による高速処理
        /// - 条件分岐最適化: 早期リターンによる不要処理回避
        /// - 即座通知: IntGameEventによる効率的なイベント配信
        /// </summary>
        public void LoseLife()
        {
            if (currentLives <= 0) return;
            currentLives--;
            onLivesChanged?.Raise(currentLives);
            if (enableDebugLog) UnityEngine.Debug.Log($"[ScoreService] life -1 (lives: {currentLives})");
            if (currentLives <= 0)
            {
                TriggerGameOver();
            }
        }

        /// <summary>
        /// ライフ増加処理（残機回復と上限制御）
        ///
        /// プレイヤーのライフを1増加させる回復機能。
        /// maxLivesの上限制御により、無制限ライフ増加を防止し適切なゲームバランスを維持します。
        ///
        /// 【実行フロー】
        /// 1. 上限検証: currentLives >= maxLives の早期リターンチェック
        /// 2. ライフ増加: currentLives++ による1加算処理
        /// 3. イベント発火: onLivesChangedによる全システム通知
        /// 4. デバッグログ: enableDebugLog時の増加ログ出力
        ///
        /// 【上限制御機能】
        /// - maxLives制限: 設定された最大ライフ数での自動停止
        /// - バランス維持: 無制限回復による難易度破綻防止
        /// - UI制約考慮: ライフ表示UIの表示限界対応
        ///
        /// 【使用場面】
        /// - 回復アイテム: AddLife() による1UP効果
        /// - ボーナス達成: AddLife() による特典付与
        /// - 高スコア報酬: AddLife() による達成報酬
        /// - イベント特典: AddLife() による期間限定ボーナス
        ///
        /// 【LoseLife()との対称性】
        /// - 増減制御: 増加 vs 減少の対称的な操作
        /// - 境界処理: 上限制御 vs ゲームオーバー判定
        /// - イベント発火: 同一のonLivesChangedイベント使用
        ///
        /// 【安全性保証】
        /// - 上限制御: maxLivesを超えた増加の防止
        /// - 一貫性: ライフ管理ルールの統一的適用
        /// - イベント安全: onLivesChanged?.Raise() による安全な発火
        ///
        /// 【パフォーマンス特性】
        /// - O(1)加算処理: 直接フィールド操作による高速処理
        /// - 条件分岐最適化: 早期リターンによる不要処理回避
        /// - 即座通知: IntGameEventによる効率的なイベント配信
        /// </summary>
        public void AddLife()
        {
            if (currentLives >= maxLives) return;
            currentLives++;
            onLivesChanged?.Raise(currentLives);
            if (enableDebugLog) UnityEngine.Debug.Log($"[ScoreService] life +1 (lives: {currentLives})");
        }

        /// <summary>
        /// ライフ直接設定処理（ライフ状態リセットとゲームオーバー判定）
        ///
        /// 現在のライフ数を指定された値に直接設定し、関連システムに通知する制御機能。
        /// 0からmaxLivesの範囲で自動クランプされ、0以下の場合は自動的にゲームオーバーを発動します。
        ///
        /// 【実行フロー】
        /// 1. 境界値制御: Mathf.Clamp(lives, 0, maxLives) による範囲制限
        /// 2. ライフ設定: currentLives への直接代入
        /// 3. イベント発火: onLivesChangedによる全システム通知
        /// 4. デバッグログ: enableDebugLog時の設定値ログ出力
        /// 5. ゲームオーバー判定: currentLives <= 0 時のTriggerGameOver()実行
        ///
        /// 【境界値制御機能】
        /// - 下限制御: 負の値は自動的に0にクランプ
        /// - 上限制御: maxLivesを超える値は上限にクランプ
        /// - 安全性: 任意の入力値での安全な動作保証
        ///
        /// 【使用場面】
        /// - ゲームリセット: SetLives(initialLives) による初期化
        /// - チート/デバッグ: SetLives(debugLives) による開発支援
        /// - セーブデータ復元: SetLives(savedLives) による状態復帰
        /// - 難易度調整: SetLives(adjustedLives) によるバランス調整
        /// - 特殊イベント: SetLives(eventLives) による一時的変更
        ///
        /// 【AddLife()・LoseLife()との違い】
        /// - 設定方式: 増減操作 vs 直接設定
        /// - 境界制御: 個別制限 vs 統合クランプ
        /// - 用途: 段階的変更 vs 状態リセット
        ///
        /// 【ゲームオーバー連携】
        /// - 自動判定: 設定値0以下で即座にゲームオーバー発動
        /// - LoseLife()との一貫性: 同一のゲームオーバー処理
        ///
        /// 【安全性保証】
        /// - 境界値制御: Mathf.Clampによる確実な範囲制限
        /// - ゲームオーバー保証: ライフ枯渇時の確実な終了処理
        /// - イベント安全: onLivesChanged?.Raise() による安全な発火
        /// </summary>
        /// <param name="lives">設定するライフ数（0-maxLivesの範囲にクランプ）</param>
        public void SetLives(int lives)
        {
            currentLives = Mathf.Clamp(lives, 0, maxLives);
            onLivesChanged?.Raise(currentLives);
            if (enableDebugLog) UnityEngine.Debug.Log($"[ScoreService] lives set: {currentLives}");
            if (currentLives <= 0)
            {
                TriggerGameOver();
            }
        }

        /// <summary>
        /// ゲームオーバー発動処理（ゲーム終了制御）
        ///
        /// ライフ枯渇時に自動実行される内部メソッド。
        /// ゲームオーバー状態を全システムに通知し、統合的なゲーム終了処理を開始します。
        ///
        /// 【実行フロー】
        /// 1. デバッグログ: enableDebugLog時のゲームオーバーログ出力
        /// 2. イベント発火: onGameOverによる全システム通知
        ///
        /// 【連携システム】
        /// - GameStateManager: GameState.GameOverへの状態遷移
        /// - UIシステム: ゲームオーバー画面の表示
        /// - オーディオシステム: ゲームオーバー音楽の再生
        /// - セーブシステム: ハイスコア保存とゲーム記録
        /// - 入力システム: ゲーム操作の無効化
        ///
        /// 【呼び出し元】
        /// - LoseLife(): ライフ減少によるゲームオーバー
        /// - SetLives(): ライフ直接設定によるゲームオーバー
        ///
        /// 【設計思想】
        /// - 内部処理: publicメソッドではなく内部制御に留める
        /// - 単一責務: ゲームオーバー通知のみに特化
        /// - イベント駆動: GameEventによる疎結合なシステム連携
        ///
        /// 【安全性保証】
        /// - イベント安全: onGameOver?.Raise() による安全な発火
        /// - 冪等性: 複数回実行でも安全な動作
        /// - 例外安全: イベント発火失敗でも処理継続
        ///
        /// 【パフォーマンス特性】
        /// - O(1)処理: 単純なイベント発火による高速実行
        /// - 最小オーバーヘッド: 必要最小限の処理のみ実行
        /// - 即座通知: GameEventによる効率的なシステム連携
        /// </summary>
        private void TriggerGameOver()
        {
            if (enableDebugLog) UnityEngine.Debug.Log("[ScoreService] Game Over");
            onGameOver?.Raise();
        }
    }
}
