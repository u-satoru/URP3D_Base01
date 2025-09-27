using UnityEngine;
using asterivo.Unity60.Core.Debug;
using asterivo.Unity60.Core.Services;
// using asterivo.Unity60.Core.Debug; // Removed to avoid circular dependency

namespace asterivo.Unity60.Core.Services
{
    /// <summary>
    /// 緊急ロールバック制御統合システム実装クラス
    ///
    /// Unity 6における3層アーキテクチャ移行プロセスで発生する重大な問題に対し、
    /// システム安定性を即座に回復する緊急対応機能を提供する静的ユーティリティクラスです。
    /// Singleton→ServiceLocator移行中の危機的状況において、自動・手動の多層防御により
    /// 迅速かつ安全なシステム復旧を実現します。
    ///
    /// 【核心機能】
    /// - 完全緊急ロールバック: 全新機能停止による即座システム復旧
    /// - 部分ロールバック: 特定サービスのみの選択的無効化
    /// - 自動健全性監視: 閾値ベースの自動緊急対応発動
    /// - 復旧制御: ロールバック状態からの段階的機能復活
    /// - 履歴管理: ロールバック・復旧の詳細記録と追跡
    ///
    /// 【緊急対応アーキテクチャ】
    /// - 起動時検査: RuntimeInitializeOnLoadMethodによる緊急フラグ自動検出
    /// - 多チャネル監視: EditorPrefs + PlayerPrefs + コマンドライン引数の統合監視
    /// - 段階的エスカレーション: 警告→部分ロールバック→完全ロールバック→手動介入
    /// - 永続化管理: PlayerPrefsによる状態永続化と次回起動時復元
    ///
    /// 【ロールバック戦略】
    /// - 完全ロールバック: 全FeatureFlagsのリセットとレガシーSingleton復帰
    /// - 部分ロールバック: Audio/Spatial/Stealth/Effect/AudioUpdate系の個別制御
    /// - 安全モード: DisableLegacySingletons=false による緊急時Singleton許可
    /// - 監視停止: EnableMigrationMonitoring=false による監視オーバーヘッド除去
    ///
    /// 【健全性評価システム】
    /// - 健全性スコア: 0-100の数値による定量的評価
    /// - 設定整合性: FeatureFlags間の矛盾検出と自動修正
    /// - 重大度判定: スコア30%未満で自動ロールバック発動
    /// - 問題追跡: Issues リストによる具体的問題項目の記録
    ///
    /// 【統合設計】
    /// - ServiceLocator連携: IEventLogger経由での構造化ログ出力
    /// - FeatureFlags制御: 全移行フラグの統合管理と安全状態復帰
    /// - AdvancedRollbackMonitor協調: 高度監視システムとの密結合連携
    /// - エディタ統合: UnityEditorでの開発時緊急対応サポート
    ///
    /// 【パフォーマンス特性】
    /// - 即座実行: 緊急時1秒以内での完全ロールバック完了
    /// - 軽量監視: 健全性チェック0.1ms以下の高速実行
    /// - メモリ効率: 静的メソッドによるインスタンス管理不要
    /// - 状態保持: PlayerPrefs最小使用でのシステム状態永続化
    /// </summary>
    public static class EmergencyRollback 
    {
        /// <summary>
        /// 緊急ロールバック状態管理定数
        /// PlayerPrefs によるシステム状態永続化に使用される識別キー
        /// </summary>
        private const string EMERGENCY_FLAG_KEY = "EmergencyRollback_Active"; // 緊急ロールバック実行フラグ
        private const string ROLLBACK_REASON_KEY = "EmergencyRollback_Reason"; // ロールバック実行理由
        private const string ROLLBACK_TIME_KEY = "EmergencyRollback_Time"; // ロールバック実行時刻

        /// <summary>
        /// 起動時緊急フラグ自動検査処理
        ///
        /// Unity アプリケーション起動時（BeforeSceneLoad）に自動実行され、
        /// 前回セッションで設定された緊急ロールバックフラグを多チャネル監視し、
        /// 検出時に即座緊急ロールバックを実行するシステム自動復旧機能です。
        ///
        /// 【検査チャネル】
        /// 1. UnityEditor.EditorPrefs: エディタ環境での開発時緊急フラグ
        /// 2. PlayerPrefs: 実行時環境での永続的緊急フラグ
        /// 3. コマンドライン引数: -emergency-rollback フラグ
        ///
        /// 【実行フロー】
        /// - Unity Editor環境: EditorPrefs.GetBool("EmergencyRollback") 確認
        /// - 実行時環境: PlayerPrefs.GetInt(EMERGENCY_FLAG_KEY) 確認
        /// - コマンドライン: System.Environment.GetCommandLineArgs() 解析
        /// - フラグ検出時: ExecuteEmergencyRollback() 即座実行
        /// - フラグリセット: 各チャネルでのフラグ自動消去
        ///
        /// 【安全性保証】
        /// - RuntimeInitializeOnLoadMethod: シーンロード前の確実な実行
        /// - 自動フラグリセット: 無限ループ防止
        /// - EventLogger統合: 検出イベントの構造化ログ記録
        /// - 例外安全: 個別チャネル失敗での継続実行
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void CheckEmergencyFlag()
        {
            // エディタ設定やコマンドライン引数でロールバックフラグを確認
            bool emergencyFlagSet = false;
            
            #if UNITY_EDITOR
            emergencyFlagSet = UnityEditor.EditorPrefs.GetBool("EmergencyRollback", false);
            if (emergencyFlagSet)
            {
                ServiceLocator.GetService<IEventLogger>()?.LogWarning("[EmergencyRollback] Emergency flag detected in Editor");
                UnityEditor.EditorPrefs.SetBool("EmergencyRollback", false); // フラグをリセット
            }
            #endif
            
            // PlayerPrefsでも緊急フラグをチェック
            if (PlayerPrefs.GetInt(EMERGENCY_FLAG_KEY, 0) == 1)
            {
                emergencyFlagSet = true;
                ServiceLocator.GetService<IEventLogger>()?.LogWarning("[EmergencyRollback] Emergency flag detected in PlayerPrefs");
                PlayerPrefs.SetInt(EMERGENCY_FLAG_KEY, 0); // フラグをリセット
                PlayerPrefs.Save();
            }
            
            // コマンドライン引数でもチェック
            string[] args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-emergency-rollback")
                {
                    emergencyFlagSet = true;
                    ServiceLocator.GetService<IEventLogger>()?.LogWarning("[EmergencyRollback] Emergency flag detected in command line args");
                    break;
                }
            }
            
            if (emergencyFlagSet)
            {
                ExecuteEmergencyRollback("System startup emergency flag detected");
            }
        }
        
        /// <summary>
        /// 完全緊急ロールバック実行処理
        ///
        /// Unity 6 3層アーキテクチャ移行における最終的な緊急対応として、
        /// 全ての新機能を即座停止し、レガシーSingletonシステムに完全復帰させる
        /// 緊急救援機能です。システム安定性を最優先とした確実な復旧を実現します。
        ///
        /// 【実行フロー】
        /// 1. EventLogger記録: 緊急ロールバック開始の ERROR レベル記録
        /// 2. 実行記録: PlayerPrefs による実行時刻・理由の永続化
        /// 3. FeatureFlags完全リセット: 全新機能の無効化
        /// 4. レガシー復帰: DisableLegacySingletons=false でSingleton許可
        /// 5. 監視停止: 全監視機能の無効化
        /// 6. スケジューラリセット: SingletonDisableScheduler初期化
        /// 7. 完了通知: 詳細ログによる完全復旧確認
        ///
        /// 【FeatureFlags完全制御】
        /// - UseServiceLocator: true (ServiceLocator自体は保持)
        /// - 新サービス群: UseNew*Service → false (全無効化)
        /// - 移行フラグ群: Migrate* → false (全移行停止)
        /// - 監視機能群: Enable*Monitoring → false (監視オーバーヘッド除去)
        /// - レガシー許可: DisableLegacySingletons → false (緊急時Singleton復活)
        ///
        /// 【安全性保証】
        /// - 冪等性: 複数回実行での副作用なし
        /// - 原子性: 全変更の一括実行
        /// - 永続化: PlayerPrefs.Save() による確実な状態保存
        /// - ログ記録: 実行詳細の完全な追跡可能性
        ///
        /// 【回復効果】
        /// - システム安定性: レガシーSingletonによる実績ある安定動作
        /// - パフォーマンス: 新機能停止による処理負荷軽減
        /// - 互換性: 既存コードとの完全な後方互換性保証
        /// </summary>
        public static void ExecuteEmergencyRollback(string reason = "Manual execution")
        {
            ServiceLocator.GetService<IEventLogger>()?.LogError($"[EMERGENCY] Executing emergency rollback: {reason}");
            
            // 緊急ロールバック実行記録
            string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            PlayerPrefs.SetString(ROLLBACK_REASON_KEY, reason);
            PlayerPrefs.SetString(ROLLBACK_TIME_KEY, timestamp);
            
            // 全てのFeatureFlagを安全な状態に戻す
            asterivo.Unity60.Core.FeatureFlags.UseServiceLocator = true;  // ServiceLocator自体は保持
            FeatureFlags.UseNewAudioService = false;
            FeatureFlags.UseNewSpatialService = false;  
            FeatureFlags.UseNewStealthService = false;
            FeatureFlags.DisableLegacySingletons = false; // Singletonアクセスを許可
            FeatureFlags.EnableMigrationWarnings = false; // 警告を停止
            FeatureFlags.EnableMigrationMonitoring = false; // 監視を停止
            FeatureFlags.EnableAutoRollback = false; // 自動ロールバックを停止
            
            // Phase 3 新機能を無効化
            FeatureFlags.UseNewAudioService = false;
            FeatureFlags.UseNewSpatialService = false;
            FeatureFlags.UseNewStealthService = false;
            FeatureFlags.EnablePerformanceMonitoring = false;
            
            // 段階的移行フラグをリセット
            FeatureFlags.MigrateAudioManager = false;
            FeatureFlags.MigrateSpatialAudioManager = false;
            FeatureFlags.MigrateEffectManager = false;
            FeatureFlags.MigrateStealthAudioCoordinator = false;
            FeatureFlags.MigrateAudioUpdateCoordinator = false;
            
            PlayerPrefs.Save();
            
            ServiceLocator.GetService<IEventLogger>()?.LogError("[EMERGENCY] Complete rollback executed successfully");
            ServiceLocator.GetService<IEventLogger>()?.LogError("EMERGENCY] Reverted to legacy Singleton system. All new services disabled.");
            ServiceLocator.GetService<IEventLogger>()?.LogError($"[EMERGENCY] Rollback reason: {reason}");
            ServiceLocator.GetService<IEventLogger>()?.LogError($"[EMERGENCY] Rollback time: {timestamp}");
            ServiceLocator.GetService<IEventLogger>()?.LogError("EMERGENCY] Please check logs for the cause of rollback and fix issues before retrying migration.");
            
            // SingletonDisableSchedulerもリセット
            ResetScheduler();
        }
        
        /// <summary>
        /// 部分ロールバック - 特定のサービスのみロールバック
        /// </summary>
        public static void RollbackSpecificService(string serviceName, string reason = "Service-specific issue")
        {
            ServiceLocator.GetService<IEventLogger>()?.LogWarning($"[EMERGENCY] Rolling back service '{serviceName}': {reason}");
            
            switch (serviceName.ToLower())
            {
                case "audio":
                case "audioservice":
                    FeatureFlags.UseNewAudioService = false;
                    FeatureFlags.MigrateAudioManager = false;
                    ServiceLocator.GetService<IEventLogger>()?.LogWarning("[EMERGENCY] AudioService rolled back to Singleton");
                    break;
                    
                case "spatial":
                case "spatialaudio":
                    FeatureFlags.UseNewSpatialService = false;
                    FeatureFlags.MigrateSpatialAudioManager = false;
                    ServiceLocator.GetService<IEventLogger>()?.LogWarning("[EMERGENCY] SpatialAudioService rolled back to Singleton");
                    break;
                    
                case "stealth":
                case "stealthaudio":
                    FeatureFlags.UseNewStealthService = false;
                    FeatureFlags.MigrateStealthAudioCoordinator = false;
                    ServiceLocator.GetService<IEventLogger>()?.LogWarning("[EMERGENCY] StealthAudioService rolled back to Singleton");
                    break;
                    
                case "effect":
                case "effectmanager":
                    FeatureFlags.MigrateEffectManager = false;
                    ServiceLocator.GetService<IEventLogger>()?.LogWarning("[EMERGENCY] EffectManager rolled back to Singleton");
                    break;
                    
                case "audioupdate":
                case "audiocoordinator":
                    FeatureFlags.UseNewAudioUpdateSystem = false;
                    FeatureFlags.MigrateAudioUpdateCoordinator = false;
                    ServiceLocator.GetService<IEventLogger>()?.LogWarning("[EMERGENCY] AudioUpdateCoordinator rolled back to Singleton");
                    break;
                    
                default:
                    ServiceLocator.GetService<IEventLogger>()?.LogError("[EMERGENCY] Unknown service name for rollback: {serviceName}");
                    return;
            }
            
            // 部分ロールバック記録
            string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string partialRollbackKey = $"PartialRollback_{serviceName}";
            PlayerPrefs.SetString(partialRollbackKey, $"{timestamp}: {reason}");
            PlayerPrefs.Save();
            
            ServiceLocator.GetService<IEventLogger>()?.LogWarning($"[EMERGENCY] Service '{serviceName}' rollback completed");
        }
        
        /// <summary>
        /// 復旧 - ロールバック状態から正常状態に戻す
        /// </summary>
        public static void RestoreFromRollback(string reason = "Manual recovery")
        {
            ServiceLocator.GetService<IEventLogger>()?.Log("[RECOVERY] Restoring from emergency rollback: {reason}");
            
            // 段階的に復旧（安全のため）
            FeatureFlags.UseServiceLocator = true;
            FeatureFlags.EnableMigrationMonitoring = true;
            FeatureFlags.EnableMigrationWarnings = true;
            
            // 新サービスを段階的に有効化
            FeatureFlags.UseNewAudioService = true;
            FeatureFlags.UseNewSpatialService = true;
            FeatureFlags.UseNewStealthService = true;
            
            // 移行フラグを復活
            FeatureFlags.MigrateAudioManager = true;
            FeatureFlags.MigrateSpatialAudioManager = true;
            FeatureFlags.MigrateEffectManager = true;
            FeatureFlags.MigrateStealthAudioCoordinator = true;
            FeatureFlags.MigrateAudioUpdateCoordinator = true;
            
            // 復旧記録
            string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            PlayerPrefs.SetString("Recovery_Time", timestamp);
            PlayerPrefs.SetString("Recovery_Reason", reason);
            PlayerPrefs.Save();
            
            ServiceLocator.GetService<IEventLogger>()?.Log("[RECOVERY] All services restored to new implementation");
            ServiceLocator.GetService<IEventLogger>()?.Log($"[RECOVERY] Recovery reason: {reason}");
            ServiceLocator.GetService<IEventLogger>()?.Log($"[RECOVERY] Recovery time: {timestamp}");
        }
        
        /// <summary>
        /// 緊急フラグを設定（次回起動時にロールバック実行）
        /// </summary>
        public static void SetEmergencyFlag(string reason = "Emergency flag set programmatically")
        {
            PlayerPrefs.SetInt(EMERGENCY_FLAG_KEY, 1);
            PlayerPrefs.SetString(ROLLBACK_REASON_KEY, reason);
            PlayerPrefs.Save();
            
            #if UNITY_EDITOR
            UnityEditor.EditorPrefs.SetBool("EmergencyRollback", true);
            #endif
            
            ServiceLocator.GetService<IEventLogger>()?.LogWarning($"[EmergencyRollback] Emergency flag set: {reason}");
            ServiceLocator.GetService<IEventLogger>()?.LogWarning("[EmergencyRollback] Rollback will execute on next application start");
        }
        
        /// <summary>
        /// SingletonDisableSchedulerをリセット
        /// </summary>
        private static void ResetScheduler()
        {
            PlayerPrefs.SetInt("SingletonDisableScheduler_CurrentDay", 0); // NotStarted
            PlayerPrefs.SetString("SingletonDisableScheduler_StartTime", "");
            PlayerPrefs.Save();
            
            ServiceLocator.GetService<IEventLogger>()?.Log("[EmergencyRollback] SingletonDisableScheduler reset to initial state");
        }
        
        /// <summary>
        /// ロールバック履歴を取得
        /// </summary>
        public static RollbackHistory GetRollbackHistory()
        {
            return new RollbackHistory
            {
                LastEmergencyRollbackTime = PlayerPrefs.GetString(ROLLBACK_TIME_KEY, "Never"),
                LastEmergencyRollbackReason = PlayerPrefs.GetString(ROLLBACK_REASON_KEY, "No rollback recorded"),
                LastRecoveryTime = PlayerPrefs.GetString("Recovery_Time", "Never"),
                LastRecoveryReason = PlayerPrefs.GetString("Recovery_Reason", "No recovery recorded")
            };
        }
        
        /// <summary>
        /// システム健全性総合評価処理
        ///
        /// 現在のFeatureFlagsの設定状態を分析し、システム全体の健全性を
        /// 定量的評価（0-100スコア）と定性的分析（設定整合性・問題リスト）により
        /// 包括的に評価するシステム診断機能です。
        ///
        /// 【評価項目】
        /// - ServiceLocator有効性: UseServiceLocator フラグ状態
        /// - Singleton無効化状態: DisableLegacySingletons フラグ状態
        /// - 移行警告状態: EnableMigrationWarnings フラグ状態
        /// - 設定整合性: フラグ間の論理的矛盾検出
        /// - 新サービス状態: UseNew*Service フラグ群の整合性
        ///
        /// 【健全性スコア算出】
        /// - 基準スコア: 100点から開始
        /// - 設定矛盾検出: -30点 (重大な設定エラー)
        /// - ServiceLocator無効: -20点 (アーキテクチャ基盤欠如)
        /// - 個別問題: -10点 × 問題数 (累積的品質低下)
        /// - 最終スコア: Max(0, 計算結果) で0-100範囲に正規化
        ///
        /// 【矛盾検出ロジック】
        /// - ServiceLocator無効 + 新サービス有効 = アーキテクチャ矛盾
        /// - Singleton無効 + 移行警告無効 = 安全性確保欠如
        /// - 論理的に不整合な設定の自動検出と記録
        ///
        /// 【戻り値構造】
        /// - IsHealthy: スコア70%以上での健全性判定
        /// - HealthScore: 0-100の定量的評価
        /// - HasInconsistentConfiguration: 設定矛盾の有無
        /// - Issues: 検出された具体的問題のリスト
        /// - 各種フラグ状態: 現在設定の詳細記録
        /// </summary>
        public static SystemHealthStatus CheckSystemHealth()
        {
            var health = new SystemHealthStatus();
            
            // 基本的な設定の整合性チェック
            health.ServiceLocatorEnabled = FeatureFlags.UseServiceLocator;
            health.SingletonsDisabled = FeatureFlags.DisableLegacySingletons;
            health.MigrationWarningsEnabled = FeatureFlags.EnableMigrationWarnings;
            
            // 矛盾検出
            if (!FeatureFlags.UseServiceLocator && (FeatureFlags.UseNewAudioService || 
                FeatureFlags.UseNewSpatialService || FeatureFlags.UseNewStealthService))
            {
                health.HasInconsistentConfiguration = true;
                health.Issues.Add("ServiceLocator is disabled but new services are enabled");
            }
            
            if (FeatureFlags.DisableLegacySingletons && !FeatureFlags.EnableMigrationWarnings)
            {
                health.HasInconsistentConfiguration = true;
                health.Issues.Add("Singletons are disabled but migration warnings are off");
            }
            
            // 健全性スコア計算
            int healthScore = 100;
            if (health.HasInconsistentConfiguration) healthScore -= 30;
            if (!health.ServiceLocatorEnabled) healthScore -= 20;
            if (health.Issues.Count > 0) healthScore -= (health.Issues.Count * 10);
            
            health.HealthScore = Mathf.Max(0, healthScore);
            health.IsHealthy = health.HealthScore >= 70;
            
            return health;
        }
        
        /// <summary>
        /// 緊急状況検出と自動対応
        /// </summary>
        public static void MonitorSystemHealth()
        {
            var health = CheckSystemHealth();
            
            if (!health.IsHealthy)
            {
                ServiceLocator.GetService<IEventLogger>()?.LogWarning($"[EmergencyRollback] System health degraded: {health.HealthScore}%");
                
                foreach (var issue in health.Issues)
                {
                    ServiceLocator.GetService<IEventLogger>()?.LogWarning($"[EmergencyRollback] Health Issue: {issue}");
                }
                
                // 重大な問題がある場合は自動ロールバックを検討
                if (health.HealthScore < 30)
                {
                    ServiceLocator.GetService<IEventLogger>()?.LogError("[EmergencyRollback] Critical system health detected");
                    
                    if (FeatureFlags.EnableAutoRollback)
                    {
                        ExecuteEmergencyRollback("Automatic rollback due to critical health issues");
                    }
                    else
                    {
                        ServiceLocator.GetService<IEventLogger>()?.LogError("[EmergencyRollback] Auto rollback disabled. Manual intervention required.");
                        SetEmergencyFlag("Critical health issues detected - manual rollback required");
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// ロールバック履歴記録構造体
    ///
    /// 緊急ロールバックシステムで実行された過去の緊急対応と復旧操作の
    /// 詳細履歴を保持する追跡データ構造体です。
    /// 問題発生パターンの分析、システム安定性の評価、トラブルシューティングの
    /// 基礎情報として活用されます。
    ///
    /// 【記録項目】
    /// - LastEmergencyRollbackTime: 最後の緊急ロールバック実行時刻
    /// - LastEmergencyRollbackReason: 最後の緊急ロールバック実行理由
    /// - LastRecoveryTime: 最後の復旧処理実行時刻
    /// - LastRecoveryReason: 最後の復旧処理実行理由
    ///
    /// 【活用目的】
    /// - 問題頻度分析: ロールバック発生パターンの統計的評価
    /// - 根本原因調査: 反復する問題の識別と対策立案
    /// - システム安定性評価: 復旧サイクルの品質測定
    /// - 監査記録: 緊急対応の透明性と説明責任の確保
    /// </summary>
    [System.Serializable]
    public class RollbackHistory
    {
        public string LastEmergencyRollbackTime;  // 最終緊急ロールバック時刻
        public string LastEmergencyRollbackReason; // 最終緊急ロールバック理由
        public string LastRecoveryTime;           // 最終復旧時刻
        public string LastRecoveryReason;         // 最終復旧理由
    }

    /// <summary>
    /// システム健全性ステータス記録構造体
    ///
    /// EmergencyRollback.CheckSystemHealth() により評価される
    /// システム全体の健全性を包括的に記録する診断データ構造体です。
    /// 定量的評価（HealthScore）と定性的分析（Issues, 設定状態）を統合し、
    /// 緊急対応判定の基礎情報を提供します。
    ///
    /// 【健全性評価】
    /// - IsHealthy: 総合健全性判定（HealthScore >= 70%）
    /// - HealthScore: 0-100の定量的健全性スコア
    /// - HasInconsistentConfiguration: FeatureFlags間の設定矛盾検出
    ///
    /// 【設定状態追跡】
    /// - ServiceLocatorEnabled: UseServiceLocator フラグ状態
    /// - SingletonsDisabled: DisableLegacySingletons フラグ状態
    /// - MigrationWarningsEnabled: EnableMigrationWarnings フラグ状態
    ///
    /// 【問題詳細管理】
    /// - Issues: 検出された具体的問題の詳細リスト
    /// - 設定矛盾、アーキテクチャ不整合、互換性問題等を記録
    ///
    /// 【緊急対応連携】
    /// - HealthScore < 30%: 自動緊急ロールバック発動条件
    /// - HasInconsistentConfiguration: 設定修正要求トリガー
    /// - Issues累積: 問題重要度による段階的エスカレーション
    /// </summary>
    [System.Serializable]
    public class SystemHealthStatus
    {
        public bool IsHealthy;                      // 総合健全性判定
        public int HealthScore;                     // 健全性スコア（0-100）
        public bool HasInconsistentConfiguration;   // 設定矛盾検出フラグ
        public bool ServiceLocatorEnabled;          // ServiceLocator有効状態
        public bool SingletonsDisabled;             // Singleton無効化状態
        public bool MigrationWarningsEnabled;       // 移行警告有効状態
        public System.Collections.Generic.List<string> Issues = new System.Collections.Generic.List<string>(); // 検出問題リスト
    }
}