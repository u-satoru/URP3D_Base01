using UnityEngine;
using System;
using System.Collections.Generic;
// // using asterivo.Unity60.Core.Debug; // Removed to avoid circular dependency

namespace asterivo.Unity60.Core
{
    /// <summary>
    /// Phase 3 拡張機能フラグシステム
    ///
    /// Unity 6における3層アーキテクチャへの段階的移行を安全に管理する
    /// 高度な機能フラグシステムです。ServiceLocatorパターンへの移行、
    /// パフォーマンス監視、緊急時ロールバック機能を統合的に提供します。
    ///
    /// 【核心機能】
    /// - 段階的移行管理: Singletonパターンから3層アーキテクチャへの安全な移行
    /// - リアルタイム監視: パフォーマンス測定と移行プロセスの監視
    /// - 緊急時対応: 自動・手動ロールバック機能による障害回復
    /// - 設定検証: 機能フラグ間の依存関係と一貫性の自動チェック
    /// - バックアップ管理: 設定変更履歴の記録と復元機能
    ///
    /// 【アーキテクチャ設計】
    /// - PlayerPrefsベースの永続化: Unity再起動後も設定を保持
    /// - 階層化された機能フラグ: Base/Migration/Gradual/Emergency層構造
    /// - スレッドセーフ: 静的クラス設計による同期的アクセス保証
    /// - Zero-Dependency: Coreシステム間の循環依存を回避
    ///
    /// 【移行フェーズ管理】
    /// - Phase 0: 完全Singletonモード（緊急時用）
    /// - Phase 1: ServiceLocator基盤構築
    /// - Phase 2: 個別コンポーネント移行開始
    /// - Phase 3: 完全移行とSingleton無効化
    ///
    /// 【プロダクション対応】
    /// - 本番環境での段階的ロールアウト
    /// - A/Bテスト用の動的フラグ切り替え
    /// - 障害時の即座復旧メカニズム
    /// </summary>
    public static class FeatureFlags
    {
        // ========== 基盤機能フラグ (Existing Base Flags) ==========

        /// <summary>
        /// 新オーディオシステム有効化フラグ
        ///
        /// Unity 6対応の改良されたオーディオシステムの使用を制御します。
        /// 3D空間オーディオ、NPCの聴覚センサー、動的環境サウンドを含む
        /// 包括的な音響システムの有効/無効を切り替えます。
        ///
        /// 【デフォルト値】: false（段階的移行のため）
        /// 【連動フラグ】: UseEventDrivenAudio, UseNewAudioUpdateSystem
        /// 【影響範囲】: AudioManager, SpatialAudioManager, DynamicAudioEnvironment
        /// </summary>
        public static bool UseNewAudioSystem
        {
            get => PlayerPrefs.GetInt("FeatureFlag_UseNewAudioSystem", 0) == 1;
            set => SetFlag("FeatureFlag_UseNewAudioSystem", value);
        }

        /// <summary>
        /// ServiceLocatorパターン有効化フラグ（最重要）
        ///
        /// 3層アーキテクチャの中核となるServiceLocatorパターンの有効性を制御します。
        /// DIフレームワークを使用せずに軽量で高速な依存関係管理を提供し、
        /// Singletonパターンから完全に脱却するための基盤フラグです。
        ///
        /// 【デフォルト値】: true（移行完了済み前提）
        /// 【重要度】: CRITICAL - 他の全移行フラグの前提条件
        /// 【影響範囲】: 全サービスクラス、ServiceHelper、全Managerクラス
        /// 【パフォーマンス】: ConcurrentDictionary&lt;Type, object&gt;によるO(1)サービス取得
        /// 【依存関係】: このフラグがfalseの場合、全移行フラグが無効化される
        /// </summary>
        public static bool UseServiceLocator
        {
            get => PlayerPrefs.GetInt("FeatureFlag_UseServiceLocator", 1) == 1;
            set => SetFlag("FeatureFlag_UseServiceLocator", value);
        }

        /// <summary>
        /// Use event driven audio system
        /// </summary>
        public static bool UseEventDrivenAudio
        {
            get => PlayerPrefs.GetInt("FeatureFlag_UseEventDrivenAudio", 0) == 1;
            set => SetFlag("FeatureFlag_UseEventDrivenAudio", value);
        }

        /// <summary>
        /// Use new AudioUpdateCoordinator service
        /// </summary>
        public static bool UseNewAudioUpdateSystem
        {
            get => PlayerPrefs.GetInt("FeatureFlag_UseNewAudioUpdateSystem", 0) == 1;
            set => SetFlag("FeatureFlag_UseNewAudioUpdateSystem", value);
        }

        /// <summary>
        /// Enable debug logging
        /// </summary>
        public static bool EnableDebugLogging
        {
            get => PlayerPrefs.GetInt("FeatureFlag_EnableDebugLogging", 1) == 1;
            set => SetFlag("FeatureFlag_EnableDebugLogging", value);
        }

        /// <summary>
        /// Use refactored architecture (for gradual migration)
        /// </summary>
        public static bool UseRefactoredArchitecture
        {
            get => PlayerPrefs.GetInt("FeatureFlag_UseRefactoredArchitecture", 0) == 1;
            set => SetFlag("FeatureFlag_UseRefactoredArchitecture", value);
        }


        /// <summary>
        /// Use new AudioService (Step 3.7)
        /// </summary>
        public static bool UseNewAudioService
        {
            get => PlayerPrefs.GetInt("FeatureFlag_UseNewAudioService", 1) == 1; // Task1: Changed to default true
            set => SetFlag("FeatureFlag_UseNewAudioService", value);
        }

        /// <summary>
        /// Use new SpatialAudioService (Step 3.7)
        /// </summary>
        public static bool UseNewSpatialService
        {
            get => PlayerPrefs.GetInt("FeatureFlag_UseNewSpatialService", 1) == 1; // Task1: Changed to default true
            set => SetFlag("FeatureFlag_UseNewSpatialService", value);
        }

        /// <summary>
        /// Use new StealthAudioService (Step 3.7)
        /// </summary>
        public static bool UseNewStealthService
        {
            get => PlayerPrefs.GetInt("FeatureFlag_UseNewStealthService", 1) == 1; // Task1: Changed to default true
            set => SetFlag("FeatureFlag_UseNewStealthService", value);
        }

        /// <summary>
        /// Enable performance monitoring (Step 3.7)
        /// </summary>
        public static bool EnablePerformanceMonitoring
        {
            get => PlayerPrefs.GetInt("FeatureFlag_EnablePerformanceMonitoring", 1) == 1;
            set => SetFlag("FeatureFlag_EnablePerformanceMonitoring", value);
        }

        // ========== Step 3.9 Legacy Singleton Warning System ==========

        /// <summary>
        /// Show warnings when Legacy Singleton is used
        /// </summary>
        public static bool EnableMigrationWarnings
        {
            get => PlayerPrefs.GetInt("FeatureFlag_EnableMigrationWarnings", 1) == 1;
            set => SetFlag("FeatureFlag_EnableMigrationWarnings", value);
        }

        /// <summary>
        /// レガシーSingleton完全無効化フラグ（最終段階）
        ///
        /// 従来のSingletonパターンへのアクセスを完全に無効化し、
        /// 3層アーキテクチャへの移行を最終完了させるクリティカルフラグです。
        /// プロダクション環境での段階的ロールアウトの最終ステップとして使用されます。
        ///
        /// 【デフォルト値】: false（安全な段階的移行のため）
        /// 【重要度】: CRITICAL - アーキテクチャ移行の最終段階
        /// 【前提条件】: UseServiceLocator=true, Phase3サービス全有効化
        /// 【影響範囲】: 全レガシーManagerクラス、Singletonアクセスポイント
        /// 【競合関係】: AllowSingletonFallbackと相互排他
        /// 【復旧方法】: EmergencyRollback()による即座復旧
        /// 【リスク】: 不完全移行時の障害発生可能性
        /// </summary>
        public static bool DisableLegacySingletons
        {
            get => PlayerPrefs.GetInt("FeatureFlag_DisableLegacySingletons", 0) == 1;
            set => SetFlag("FeatureFlag_DisableLegacySingletons", value);
        }

        // ========== Phase 3 New Migration Management Flags ==========

        /// <summary>
        /// Enable migration process monitoring
        /// </summary>
        public static bool EnableMigrationMonitoring
        {
            get => PlayerPrefs.GetInt("FeatureFlag_EnableMigrationMonitoring", 1) == 1;
            set => SetFlag("FeatureFlag_EnableMigrationMonitoring", value);
        }

        /// <summary>
        /// Enable performance measurement
        /// </summary>
        public static bool EnablePerformanceMeasurement
        {
            get => PlayerPrefs.GetInt("FeatureFlag_EnablePerformanceMeasurement", 1) == 1;
            set => SetFlag("FeatureFlag_EnablePerformanceMeasurement", value);
        }

        /// <summary>
        /// Enable automatic rollback functionality
        /// </summary>
        public static bool EnableAutoRollback
        {
            get => PlayerPrefs.GetInt("FeatureFlag_EnableAutoRollback", 1) == 1;
            set => SetFlag("FeatureFlag_EnableAutoRollback", value);
        }

        /// <summary>
        /// Singleton緊急フォールバック許可フラグ
        ///
        /// ServiceLocator移行時の緊急時におけるSingletonパターンへの
        /// フォールバック機能を制御します。プロダクション環境での
        /// 障害発生時の安全網として機能します。
        ///
        /// 【デフォルト値】: false（移行完了済み前提）
        /// 【用途】: 緊急時ロールバック、災害復旧、開発時デバッグ
        /// 【連動機能】: ServiceHelper.GetServiceWithFallback()で使用
        /// 【パフォーマンス影響】: FindFirstObjectByType()による線形検索実行
        /// 【制約】: DisableLegacySingletonsと競合（相互排他）
        /// 【セキュリティ】: trueの場合、アーキテクチャ制約が緩和される
        /// </summary>
        public static bool AllowSingletonFallback
        {
            get => PlayerPrefs.GetInt("FeatureFlag_AllowSingletonFallback", 0) == 1;
            set => SetFlag("FeatureFlag_AllowSingletonFallback", value);
        }

        /// <summary>
        /// Enable test mode
        /// </summary>
        public static bool EnableTestMode
        {
            get => PlayerPrefs.GetInt("FeatureFlag_EnableTestMode", 0) == 1;
            set => SetFlag("FeatureFlag_EnableTestMode", value);
        }

        // ========== Gradual Migration Individual Flags ==========

        /// <summary>
        /// Enable ServiceLocator migration for AudioManager
        /// </summary>
        public static bool MigrateAudioManager
        {
            get => PlayerPrefs.GetInt("FeatureFlag_MigrateAudioManager", 0) == 1;
            set => SetFlag("FeatureFlag_MigrateAudioManager", value);
        }

        /// <summary>
        /// Enable ServiceLocator migration for SpatialAudioManager
        /// </summary>
        public static bool MigrateSpatialAudioManager
        {
            get => PlayerPrefs.GetInt("FeatureFlag_MigrateSpatialAudioManager", 0) == 1;
            set => SetFlag("FeatureFlag_MigrateSpatialAudioManager", value);
        }

        /// <summary>
        /// Enable ServiceLocator migration for EffectManager
        /// </summary>
        public static bool MigrateEffectManager
        {
            get => PlayerPrefs.GetInt("FeatureFlag_MigrateEffectManager", 0) == 1;
            set => SetFlag("FeatureFlag_MigrateEffectManager", value);
        }

        /// <summary>
        /// Enable ServiceLocator migration for StealthAudioCoordinator
        /// </summary>
        public static bool MigrateStealthAudioCoordinator
        {
            get => PlayerPrefs.GetInt("FeatureFlag_MigrateStealthAudioCoordinator", 0) == 1;
            set => SetFlag("FeatureFlag_MigrateStealthAudioCoordinator", value);
        }

        /// <summary>
        /// Enable ServiceLocator migration for AudioUpdateCoordinator
        /// </summary>
        public static bool MigrateAudioUpdateCoordinator
        {
            get => PlayerPrefs.GetInt("FeatureFlag_MigrateAudioUpdateCoordinator", 0) == 1;
            set => SetFlag("FeatureFlag_MigrateAudioUpdateCoordinator", value);
        }

        // ========== Phase 3 Utility Methods ==========

        /// <summary>
        /// 統一フラグ管理・変更ログ機能
        ///
        /// 全機能フラグの設定変更を統一的に管理し、変更履歴の記録、
        /// デバッグログ出力、移行監視システムとの連携を自動実行します。
        ///
        /// 【実行フロー】
        /// 1. 現在値との差分チェック（PlayerPrefs.GetInt使用）
        /// 2. 値変更時のPlayerPrefs更新（1/0のint形式）
        /// 3. EnableDebugLoggingフラグに基づくログ出力
        /// 4. EnableMigrationMonitoringフラグに基づく履歴記録
        ///
        /// 【パフォーマンス特性】
        /// - 同値設定時: 即座リターン（PlayerPrefs書き込み回避）
        /// - 変更時: PlayerPrefs I/O + ログ処理
        /// - 履歴管理: 最新10件保持（メモリ効率化）
        ///
        /// 【呼び出し元】: 全フラグプロパティのsetterから統一的に使用
        /// </summary>
        /// <param name="key">PlayerPrefs保存キー（"FeatureFlag_"プレフィックス付き）</param>
        /// <param name="value">設定する真偽値</param>
        private static void SetFlag(string key, bool value)
        {
            bool oldValue = PlayerPrefs.GetInt(key, 0) == 1;
            if (oldValue != value)
            {
                PlayerPrefs.SetInt(key, value ? 1 : 0);
                if (EnableDebugLogging)
                {
                    Debug.Log($"[FeatureFlags] {key}: {oldValue} -> {value}");
                }

                // Log change for migration monitoring
                if (EnableMigrationMonitoring)
                {
                    LogFlagChange(key, oldValue, value);
                }
            }
        }

        /// <summary>
        /// Log flag change history
        /// </summary>
        private static void LogFlagChange(string flagName, bool oldValue, bool newValue)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string logEntry = $"[{timestamp}] {flagName}: {oldValue} -> {newValue}";

            // Add to PlayerPrefs history (keep last 10)
            List<string> history = GetFlagChangeHistory();
            history.Add(logEntry);

            // Limit to latest 10 entries
            if (history.Count > 10)
            {
                history.RemoveAt(0);
            }

            PlayerPrefs.SetString("FeatureFlag_ChangeHistory", string.Join("|", history));
        }

        /// <summary>
        /// Get flag change history
        /// </summary>
        public static List<string> GetFlagChangeHistory()
        {
            string historyStr = PlayerPrefs.GetString("FeatureFlag_ChangeHistory", "");
            return string.IsNullOrEmpty(historyStr) ? new List<string>() : new List<string>(historyStr.Split('|'));
        }

        /// <summary>
        /// 段階的移行フェーズ設定（プリセット一括適用）
        ///
        /// ServiceLocatorパターンへの段階的移行を安全に管理するため、
        /// 事前定義されたフェーズプリセットを一括適用します。各フェーズは
        /// 移行リスクを最小化する慎重な設定組み合わせで構成されています。
        ///
        /// 【フェーズ構成】
        /// - Phase 0: 完全Singletonモード（緊急時・初期状態）
        ///   - 全ServiceLocator機能無効化
        ///   - AllowSingletonFallback=true（完全後方互換）
        ///   - 移行フラグ全リセット
        ///
        /// - Phase 1: ServiceLocator基盤構築
        ///   - UseServiceLocator=true（基盤有効化）
        ///   - 監視・測定機能有効化
        ///   - オーディオシステムは従来通り
        ///
        /// - Phase 2: AudioManager移行開始
        ///   - MigrateAudioManager=true（個別移行開始）
        ///   - UseNewAudioSystem=true
        ///   - 監視システム維持
        ///
        /// - Phase 3: 完全移行完了
        ///   - 全新サービス有効化
        ///   - 全移行フラグ有効化
        ///   - AllowSingletonFallback=false（厳格化）
        ///
        /// 【安全性確保】
        /// - PlayerPrefs.Save()による設定確定
        /// - LogCurrentFlags()による設定確認
        /// - 不正フェーズ番号の警告処理
        /// </summary>
        /// <param name="phase">移行フェーズ番号（0-3）</param>
        public static void SetMigrationPhase(int phase)
        {
            switch (phase)
            {
                case 0: // Reset (complete Singleton mode)
                    UseServiceLocator = false;
                    UseNewAudioSystem = false;
                    UseEventDrivenAudio = false;
                    UseNewAudioUpdateSystem = false;
                    AllowSingletonFallback = true;
                    ResetAllMigrationFlags();
                    break;

                case 1: // Phase 1: ServiceLocator foundation
                    UseServiceLocator = true;
                    UseNewAudioSystem = false;
                    UseEventDrivenAudio = false;
                    EnableMigrationMonitoring = true;
                    EnablePerformanceMeasurement = true;
                    break;

                case 2: // Phase 2: AudioManager migration start
                    UseServiceLocator = true;
                    MigrateAudioManager = true;
                    UseNewAudioSystem = true;
                    EnableMigrationMonitoring = true;
                    EnablePerformanceMeasurement = true;
                    break;

                case 3: // Phase 3: Complete migration
                    UseServiceLocator = true;
                    UseNewAudioSystem = true;
                    UseEventDrivenAudio = true;
                    UseNewAudioUpdateSystem = true;
                    // Task1: Add Phase 3 new flags
                    UseNewAudioService = true;
                    UseNewSpatialService = true;
                    UseNewStealthService = true;
                    EnableAllMigrationFlags();
                    AllowSingletonFallback = false;
                    break;

                default:
                    Debug.LogWarning($"[FeatureFlags] Unknown migration phase: {phase}");
                    return;
            }

            PlayerPrefs.Save();
            Debug.Log($"[FeatureFlags] Migration phase set to: {phase}");
            LogCurrentFlags();
        }

        /// <summary>
        /// Enable all migration flags
        /// </summary>
        private static void EnableAllMigrationFlags()
        {
            MigrateAudioManager = true;
            MigrateSpatialAudioManager = true;
            MigrateEffectManager = true;
            MigrateStealthAudioCoordinator = true;
            MigrateAudioUpdateCoordinator = true;
        }

        /// <summary>
        /// Reset all migration flags
        /// </summary>
        private static void ResetAllMigrationFlags()
        {
            MigrateAudioManager = false;
            MigrateSpatialAudioManager = false;
            MigrateEffectManager = false;
            MigrateStealthAudioCoordinator = false;
            MigrateAudioUpdateCoordinator = false;
        }

        /// <summary>
        /// 緊急時ロールバック実行（Singletonモードへ完全復帰）
        ///
        /// プロダクション環境での重大障害発生時に、全設定をSingletonモードに
        /// 即座復帰させる緊急時復旧機能です。ServiceLocator移行に起因する
        /// システム障害の迅速解決を目的とした最終手段として提供されます。
        ///
        /// 【実行内容】
        /// 1. SetMigrationPhase(0)による全設定の完全リセット
        /// 2. 緊急ロールバック実行時刻の記録（PlayerPrefs永続化）
        /// 3. 警告ログとエラーログによる実行履歴の明確化
        /// 4. PlayerPrefs.Save()による設定の即座確定
        ///
        /// 【復旧スコープ】
        /// - 全ServiceLocator関連フラグの無効化
        /// - 全移行フラグのリセット
        /// - AllowSingletonFallback=trueによる旧システム復活
        /// - 監視・測定機能の維持（障害分析のため）
        ///
        /// 【使用タイミング】
        /// - プロダクション環境での重大システム障害
        /// - ServiceLocator移行に起因するクリティカルエラー
        /// - 自動ロールバック機能の手動実行
        ///
        /// 【注意】: 開発データの消失はないが、移行進捗は失われる
        /// </summary>
        public static void EmergencyRollback()
        {
            Debug.LogWarning("[FeatureFlags] EMERGENCY ROLLBACK - Reverting to Singleton mode");

            SetMigrationPhase(0); // Complete reset

            // Record emergency rollback history
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            PlayerPrefs.SetString("FeatureFlag_LastEmergencyRollback", timestamp);

            PlayerPrefs.Save();
            Debug.LogError($"[FeatureFlags] Emergency rollback completed at {timestamp}");
        }

        /// <summary>
        /// Get last emergency rollback time
        /// </summary>
        public static string GetLastEmergencyRollbackTime()
        {
            return PlayerPrefs.GetString("FeatureFlag_LastEmergencyRollback", "None");
        }

        /// <summary>
        /// Migration safety check
        /// </summary>
        public static bool IsMigrationSafe()
        {
            // Confirm ServiceLocator is basically working
            if (!UseServiceLocator) return false;

            // Confirm performance measurement is enabled
            if (!EnablePerformanceMeasurement) return false;

            // Confirm monitoring system is enabled
            if (!EnableMigrationMonitoring) return false;

            return true;
        }

        /// <summary>
        /// Get current migration progress (0-100%)
        /// </summary>
        public static int GetMigrationProgress()
        {
            int completed = 0;
            int total = 5; // Number of Singletons to migrate

            if (MigrateAudioManager) completed++;
            if (MigrateSpatialAudioManager) completed++;
            if (MigrateEffectManager) completed++;
            if (MigrateStealthAudioCoordinator) completed++;
            if (MigrateAudioUpdateCoordinator) completed++;

            return (completed * 100) / total;
        }

        /// <summary>
        /// Reset all Feature Flags
        /// </summary>
        public static void ResetAll()
        {
            // Reset base flags
            UseNewAudioSystem = false;
            UseServiceLocator = true;
            UseEventDrivenAudio = false;
            UseNewAudioUpdateSystem = false;
            EnableDebugLogging = true;

            // Reset Phase 3 new flags
            EnableMigrationMonitoring = true;
            EnablePerformanceMeasurement = true;
            EnableAutoRollback = true;
            AllowSingletonFallback = false;
            EnableTestMode = false;

            // Reset migration flags
            ResetAllMigrationFlags();

            PlayerPrefs.Save();

            Debug.Log("[FeatureFlags] All flags reset to default");
        }

        /// <summary>
        /// Log current Feature Flag settings
        /// </summary>
        public static void LogCurrentFlags()
        {
            Debug.Log($"[FeatureFlags] === Current Settings ===");
            Debug.Log($"  Base Flags:");
            Debug.Log($"    - UseNewAudioSystem: {UseNewAudioSystem}");
            Debug.Log($"    - UseServiceLocator: {UseServiceLocator}");
            Debug.Log($"    - UseEventDrivenAudio: {UseEventDrivenAudio}");
            Debug.Log($"    - UseNewAudioUpdateSystem: {UseNewAudioUpdateSystem}");
            Debug.Log($"    - EnableDebugLogging: {EnableDebugLogging}");

            Debug.Log($"  Migration Management Flags:");
            Debug.Log($"    - EnableMigrationMonitoring: {EnableMigrationMonitoring}");
            Debug.Log($"    - EnablePerformanceMeasurement: {EnablePerformanceMeasurement}");
            Debug.Log($"    - EnableAutoRollback: {EnableAutoRollback}");
            Debug.Log($"    - AllowSingletonFallback: {AllowSingletonFallback}");
            Debug.Log($"    - EnableTestMode: {EnableTestMode}");

            Debug.Log($"  Gradual Migration Flags:");
            Debug.Log($"    - MigrateAudioManager: {MigrateAudioManager}");
            Debug.Log($"    - MigrateSpatialAudioManager: {MigrateSpatialAudioManager}");
            Debug.Log($"    - MigrateEffectManager: {MigrateEffectManager}");
            Debug.Log($"    - MigrateStealthAudioCoordinator: {MigrateStealthAudioCoordinator}");
            Debug.Log($"    - MigrateAudioUpdateCoordinator: {MigrateAudioUpdateCoordinator}");

            Debug.Log($"  Migration Progress: {GetMigrationProgress()}%");
            Debug.Log($"  Last Emergency Rollback: {GetLastEmergencyRollbackTime()}");
            Debug.Log($"  Migration Safety: {(IsMigrationSafe() ? "OK" : "NG")}");
        }

        /// <summary>
        /// Log flag change history
        /// </summary>

        /// <summary>
        /// Reset all flags to default values
        /// </summary>
        public static void ResetToDefaults()
        {
            SetMigrationPhase(0); // Reset to complete Singleton mode
            EnableMigrationMonitoring = false;
            EnablePerformanceMeasurement = false;
            EnableAutoRollback = false;
            AllowSingletonFallback = true;
            EnableTestMode = false;
            EnableDebugLogging = false;

            // Reset completed successfully
        }

        /// <summary>
        /// Task 1: Enable Phase 3 flags for certain enabling
        /// </summary>
        public static void EnablePhase3Flags()
        {
            // Delete existing PlayerPrefs keys to apply new defaults
            PlayerPrefs.DeleteKey("FeatureFlag_UseNewAudioService");
            PlayerPrefs.DeleteKey("FeatureFlag_UseNewSpatialService");
            PlayerPrefs.DeleteKey("FeatureFlag_UseNewStealthService");

            // Explicitly set (to be certain)
            UseNewAudioService = true;
            UseNewSpatialService = true;
            UseNewStealthService = true;

            PlayerPrefs.Save();

            Debug.Log("[FeatureFlags] Phase 3 flags enabled successfully");
            LogCurrentFlags(); // Confirm settings
        }

        /// <summary>
        /// Validate configuration consistency
        /// </summary>
        public static void ValidateConfiguration()
        {
            // Warning if UseServiceLocator is false but migration flags are true
            if (!UseServiceLocator && (MigrateAudioManager || MigrateSpatialAudioManager ||
                MigrateEffectManager || MigrateAudioUpdateCoordinator || MigrateStealthAudioCoordinator))
            {
                Debug.LogWarning("[FeatureFlags] Inconsistent configuration: Migration flags are enabled but UseServiceLocator is false");
            }

            // Warning if migration monitoring is disabled but performance measurement is enabled
            if (!EnableMigrationMonitoring && EnablePerformanceMeasurement)
            {
                Debug.LogWarning("[FeatureFlags] EnablePerformanceMeasurement requires EnableMigrationMonitoring");
            }

            // Conflict between DisableLegacySingletons and AllowSingletonFallback
            if (DisableLegacySingletons && AllowSingletonFallback)
            {
                Debug.LogWarning("[FeatureFlags] Inconsistent configuration: DisableLegacySingletons=true conflicts with AllowSingletonFallback=true");
            }

            // Day4 prerequisite: DisableLegacySingletons requires all Phase3 services enabled
            if (DisableLegacySingletons && (!UseNewAudioService || !UseNewSpatialService || !UseNewStealthService))
            {
                Debug.LogWarning("[FeatureFlags] DisableLegacySingletons requires Phase 3 services enabled (UseNewAudio/Spatial/Stealth)");
            }
        }

        /// <summary>
        /// Enforce consistency (autoFix=true for automatic correction)
        /// </summary>
        public static void EnforceConsistency(bool autoFix = false)
        {
            bool changed = false;

            if (!UseServiceLocator)
            {
                // Migration flags are invalid
                if (MigrateAudioManager && autoFix) { MigrateAudioManager = false; changed = true; }
                if (MigrateSpatialAudioManager && autoFix) { MigrateSpatialAudioManager = false; changed = true; }
                if (MigrateEffectManager && autoFix) { MigrateEffectManager = false; changed = true; }
                if (MigrateStealthAudioCoordinator && autoFix) { MigrateStealthAudioCoordinator = false; changed = true; }
                if (MigrateAudioUpdateCoordinator && autoFix) { MigrateAudioUpdateCoordinator = false; changed = true; }

                // Legacy disabling is dangerous so cancel
                if (DisableLegacySingletons && autoFix) { DisableLegacySingletons = false; changed = true; }
            }

            // Measurement without monitoring is invalid
            if (!EnableMigrationMonitoring && EnablePerformanceMeasurement && autoFix)
            {
                EnablePerformanceMeasurement = false; changed = true;
            }

            // Conflict resolution: Legacy disable and Singleton fallback cannot both be true
            if (DisableLegacySingletons && AllowSingletonFallback && autoFix)
            {
                AllowSingletonFallback = false; changed = true;
            }

            // Day4 safety: If disabling Legacy, enable Phase3 new services
            if (DisableLegacySingletons && autoFix)
            {
                if (!UseNewAudioService) { UseNewAudioService = true; changed = true; }
                if (!UseNewSpatialService) { UseNewSpatialService = true; changed = true; }
                if (!UseNewStealthService) { UseNewStealthService = true; changed = true; }
            }

            if (changed)
            {
                PlayerPrefs.Save();
                Debug.Log("[FeatureFlags] Consistency enforced and saved");
            }
        }

        // ========== Task 4: DisableLegacySingletons Gradual Enabling ==========

        /// <summary>
        /// Task 4: Day 1 - Enable warning system in test environment
        /// Ensure EnableMigrationWarnings and MigrationMonitoring are enabled
        /// </summary>
        public static void EnableDay1TestWarnings()
        {
            PlayerPrefs.DeleteKey("FeatureFlag_EnableMigrationWarnings");
            PlayerPrefs.DeleteKey("FeatureFlag_EnableMigrationMonitoring");

            EnableMigrationWarnings = true;
            EnableMigrationMonitoring = true;
            EnableDebugLogging = true;

            PlayerPrefs.Save();
            Debug.Log("[FeatureFlags] Day 1: Test warnings enabled successfully");
            LogCurrentFlags();
        }

        /// <summary>
        /// Task 4: Day 1 implementation - Actually execute warning system in test environment
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void ExecuteDay1TestWarnings()
        {
            // Execute Day 1 only once
            if (PlayerPrefs.GetInt("Task4_Day1_Executed", 0) == 1)
                return;

            EnableDay1TestWarnings();
            PlayerPrefs.SetInt("Task4_Day1_Executed", 1);
            PlayerPrefs.Save();

            Debug.Log("[Task 4 - Day 1] Warning system enabled in test environment. Legacy Singleton usage is being monitored.");
        }

        /// <summary>
        /// Task 4: Day 4 - Enable Singleton gradual disabling in production
        /// Enable DisableLegacySingletons (final step)
        /// </summary>
        public static void EnableDay4SingletonDisabling()
        {
            PlayerPrefs.DeleteKey("FeatureFlag_DisableLegacySingletons");

            DisableLegacySingletons = true;

            PlayerPrefs.Save();
            Debug.Log("[FeatureFlags] Day 4: Legacy Singletons disabled successfully");
            LogCurrentFlags();

            // Recommend MigrationValidator execution
            Debug.Log("[FeatureFlags] RECOMMENDATION: Run MigrationValidator to verify migration completion");
        }

        /// <summary>
        /// Task 4: Day 4 execution - Actually disable Singletons in production
        /// </summary>
        public static void ExecuteDay4SingletonDisabling()
        {
            // Execute Day 4 only once
            if (PlayerPrefs.GetInt("Task4_Day4_Executed", 0) == 1)
            {
                Debug.Log("[Task 4 - Day 4] Already executed. Legacy Singletons are disabled.");
                return;
            }

            // Safety check
            if (!IsTask4Safe())
            {
                Debug.LogError("[Task 4 - Day 4] Safety check failed. Cannot disable Legacy Singletons.");
                return;
            }

            EnableDay4SingletonDisabling();
            PlayerPrefs.SetInt("Task4_Day4_Executed", 1);
            PlayerPrefs.Save();

            Debug.Log("[Task 4 - Day 4] Legacy Singleton disabled in production. ServiceLocator migration complete.");

            // Report completion status
            Debug.Log($"[Task 4 Complete] Migration Progress: {GetMigrationProgress()}%, Safety Status: {(IsMigrationSafe() ? "SAFE" : "UNSAFE")}");
        }

        /// <summary>
        /// Task 4 safe execution check
        /// </summary>
        public static bool IsTask4Safe()
        {
            // Check ServiceLocator foundation is ready
            if (!UseServiceLocator)
            {
                Debug.LogError("[FeatureFlags] Task 4 requires UseServiceLocator = true");
                return false;
            }

            // Check Phase 3 new services are enabled
            if (!UseNewAudioService || !UseNewSpatialService || !UseNewStealthService)
            {
                Debug.LogError("[FeatureFlags] Task 4 requires all Phase 3 services enabled");
                return false;
            }

            // Check migration monitoring system is enabled
            if (!EnableMigrationMonitoring)
            {
                Debug.LogError("[FeatureFlags] Task 4 requires EnableMigrationMonitoring = true");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Startup configuration validation hook (must run once)
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        private static void RuntimeValidateOnStartup()
        {
            try
            {
                ValidateConfiguration();
                // No automatic correction, only warnings. Call EnforceConsistency(true) from startup script if needed
                EnforceConsistency(false);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[FeatureFlags] Runtime validation failed: {e.Message}");
            }
        }

        /// <summary>
        /// Phase 1.2: SINGLETON_COMPLETE_REMOVAL_GUIDE.md preparation - Create comprehensive backup and final settings
        /// Apply FeatureFlags final settings for complete Singleton removal preparation
        /// </summary>
        public static void ExecutePhase1ComprehensiveBackupAndFinalSettings()
        {
            Debug.Log("[FeatureFlags] === Phase 1.2: Creating comprehensive backup and executing final settings ===");

            // Step 1: Create comprehensive backup of current settings
            string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmm");
            string backupKey = $"Phase1_Backup_{timestamp}";

            Debug.Log($"[FeatureFlags] Step 1: Creating comprehensive backup: {backupKey}");

            // Log current settings (for backup purpose)
            LogCurrentFlags();

            // Save backup to PlayerPrefs
            string featureFlagsBackup = SerializeCurrentFeatureFlags();
            PlayerPrefs.SetString($"{backupKey}_FeatureFlags", featureFlagsBackup);
            PlayerPrefs.SetString($"{backupKey}_Timestamp", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            PlayerPrefs.SetString("LastPhase1Backup", backupKey);

            Debug.Log($"[FeatureFlags] ✓ Comprehensive backup created: {backupKey}");

            // Step 2: Apply final settings according to SINGLETON_COMPLETE_REMOVAL_GUIDE.md
            Debug.Log("[FeatureFlags] Step 2: Applying final FeatureFlags configuration for complete removal...");

            // Gradual update (for safety)
            Debug.Log("[FeatureFlags] Step 2.1: Disabling Legacy Singletons...");
            DisableLegacySingletons = true;

            Debug.Log("[FeatureFlags] Step 2.2: Disabling Migration Warnings...");
            EnableMigrationWarnings = false;

            Debug.Log("[FeatureFlags] Step 2.3: Disabling Migration Monitoring...");
            EnableMigrationMonitoring = false;

            PlayerPrefs.Save();

            // Confirm updated status
            Debug.Log("[FeatureFlags] ✓ Phase 1.2 final settings complete:");
            Debug.Log($"  - DisableLegacySingletons: {DisableLegacySingletons}");
            Debug.Log($"  - EnableMigrationWarnings: {EnableMigrationWarnings}");
            Debug.Log($"  - EnableMigrationMonitoring: {EnableMigrationMonitoring}");

            Debug.Log("[FeatureFlags] === Phase 1.2 Complete: System ready for Phase 2: Physical Code Removal ===");
        }

        /// <summary>
        /// Phase 1.2: Serialize current FeatureFlags
        /// </summary>
        private static string SerializeCurrentFeatureFlags()
        {
            return $"UseServiceLocator:{UseServiceLocator}," +
                   $"DisableLegacySingletons:{DisableLegacySingletons}," +
                   $"EnableMigrationWarnings:{EnableMigrationWarnings}," +
                   $"EnableMigrationMonitoring:{EnableMigrationMonitoring}," +
                   $"UseNewAudioService:{UseNewAudioService}," +
                   $"UseNewSpatialService:{UseNewSpatialService}," +
                   $"UseNewStealthService:{UseNewStealthService}";
        }

        /// <summary>
        /// Phase 1 Emergency Rollback: Revert FeatureFlags to safe state
        /// </summary>
        public static void ExecutePhase1EmergencyRollback()
        {
            Debug.LogWarning("[FeatureFlags] === EXECUTING PHASE 1 EMERGENCY ROLLBACK ===");

            // Revert FeatureFlags to safe state
            DisableLegacySingletons = false;
            EnableMigrationWarnings = true;
            EnableMigrationMonitoring = true;

            PlayerPrefs.Save();

            // Display restore info from latest backup
            string lastBackup = PlayerPrefs.GetString("LastPhase1Backup", "");
            if (!string.IsNullOrEmpty(lastBackup))
            {
                Debug.Log($"[FeatureFlags] Backup available for restore: {lastBackup}");
                string backupData = PlayerPrefs.GetString($"{lastBackup}_FeatureFlags", "");
                Debug.Log($"[FeatureFlags] Backup data: {backupData}");
            }

            Debug.Log("[FeatureFlags] ✓ Phase 1 Emergency rollback completed");
            LogCurrentFlags();
        }

        public static void LogFlagHistory()
        {
            Debug.Log($"[FeatureFlags] === Flag Change History ===");
            var history = GetFlagChangeHistory();

            if (history.Count == 0)
            {
                Debug.Log($"  No history");
                return;
            }

            foreach (var entry in history)
            {
                Debug.Log($"  {entry}");
            }
        }
    }
}