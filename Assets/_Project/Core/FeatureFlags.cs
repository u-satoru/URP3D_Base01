using UnityEngine;
using System;
using System.Collections.Generic;

namespace _Project.Core
{
    /// <summary>
    /// Phase 3 強化版 Feature Flag システム
    /// 段階的移行、パフォーマンス監視、ロールバック機能を統合
    /// </summary>
    public static class FeatureFlags
    {
        // ========== 既存の基本フラグ ==========
        
        /// <summary>
        /// 新しいオーディオシステムを使用するか
        /// </summary>
        public static bool UseNewAudioSystem 
        {
            get => PlayerPrefs.GetInt("FeatureFlag_UseNewAudioSystem", 0) == 1;
            set => SetFlag("FeatureFlag_UseNewAudioSystem", value);
        }
        
        /// <summary>
        /// Service Locatorパターンを使用するか
        /// </summary>
        public static bool UseServiceLocator
        {
            get => PlayerPrefs.GetInt("FeatureFlag_UseServiceLocator", 1) == 1;
            set => SetFlag("FeatureFlag_UseServiceLocator", value);
        }
        
        /// <summary>
        /// イベント駆動音響システムを使用するか
        /// </summary>
        public static bool UseEventDrivenAudio
        {
            get => PlayerPrefs.GetInt("FeatureFlag_UseEventDrivenAudio", 0) == 1;
            set => SetFlag("FeatureFlag_UseEventDrivenAudio", value);
        }
        
        /// <summary>
        /// 新しいAudioUpdateCoordinatorサービスを使用するか
        /// </summary>
        public static bool UseNewAudioUpdateSystem
        {
            get => PlayerPrefs.GetInt("FeatureFlag_UseNewAudioUpdateSystem", 0) == 1;
            set => SetFlag("FeatureFlag_UseNewAudioUpdateSystem", value);
        }
        
        /// <summary>
        /// デバッグログを有効にするか
        /// </summary>
        public static bool EnableDebugLogging
        {
            get => PlayerPrefs.GetInt("FeatureFlag_EnableDebugLogging", 1) == 1;
            set => SetFlag("FeatureFlag_EnableDebugLogging", value);
        }
        
                
        /// <summary>
        /// 新しいAudioServiceを使用するか（Step 3.7用）
        /// </summary>
        public static bool UseNewAudioService
        {
            get => PlayerPrefs.GetInt("FeatureFlag_UseNewAudioService", 1) == 1; // ✅ Task1: デフォルト値を1に変更
            set => SetFlag("FeatureFlag_UseNewAudioService", value);
        }
        
        /// <summary>
        /// 新しいSpatialAudioServiceを使用するか（Step 3.7用）
        /// </summary>
        public static bool UseNewSpatialService
        {
            get => PlayerPrefs.GetInt("FeatureFlag_UseNewSpatialService", 1) == 1; // ✅ Task1: デフォルト値を1に変更
            set => SetFlag("FeatureFlag_UseNewSpatialService", value);
        }
        
        /// <summary>
        /// 新しいStealthAudioServiceを使用するか（Step 3.7用）
        /// </summary>
        public static bool UseNewStealthService
        {
            get => PlayerPrefs.GetInt("FeatureFlag_UseNewStealthService", 1) == 1; // ✅ Task1: デフォルト値を1に変更
            set => SetFlag("FeatureFlag_UseNewStealthService", value);
        }
        
        /// <summary>
        /// パフォーマンス監視を有効にするか（Step 3.7用）
        /// </summary>
        public static bool EnablePerformanceMonitoring
        {
            get => PlayerPrefs.GetInt("FeatureFlag_EnablePerformanceMonitoring", 1) == 1;
            set => SetFlag("FeatureFlag_EnablePerformanceMonitoring", value);
        }
// ========== Step 3.9 Legacy Singleton警告システム ==========
        
        /// <summary>
        /// Legacy Singleton使用時に警告を表示するか
        /// </summary>
        public static bool EnableMigrationWarnings
        {
            get => PlayerPrefs.GetInt("FeatureFlag_EnableMigrationWarnings", 1) == 1;
            set => SetFlag("FeatureFlag_EnableMigrationWarnings", value);
        }
        
        /// <summary>
        /// Legacy Singletonへのアクセスを完全に禁止するか
        /// </summary>
        public static bool DisableLegacySingletons
        {
            get => PlayerPrefs.GetInt("FeatureFlag_DisableLegacySingletons", 0) == 1;
            set => SetFlag("FeatureFlag_DisableLegacySingletons", value);
        }
        
// ========== Phase 3 新規移行管理フラグ ==========
        
        /// <summary>
        /// 移行プロセス監視を有効にするか
        /// </summary>
        public static bool EnableMigrationMonitoring
        {
            get => PlayerPrefs.GetInt("FeatureFlag_EnableMigrationMonitoring", 1) == 1;
            set => SetFlag("FeatureFlag_EnableMigrationMonitoring", value);
        }
        
        /// <summary>
        /// パフォーマンス測定を有効にするか
        /// </summary>
        public static bool EnablePerformanceMeasurement
        {
            get => PlayerPrefs.GetInt("FeatureFlag_EnablePerformanceMeasurement", 1) == 1;
            set => SetFlag("FeatureFlag_EnablePerformanceMeasurement", value);
        }
        
        /// <summary>
        /// 自動ロールバック機能を有効にするか
        /// </summary>
        public static bool EnableAutoRollback
        {
            get => PlayerPrefs.GetInt("FeatureFlag_EnableAutoRollback", 1) == 1;
            set => SetFlag("FeatureFlag_EnableAutoRollback", value);
        }
        
        /// <summary>
        /// Singletonの使用を許可するか（緊急時ロールバック用）
        /// </summary>
        public static bool AllowSingletonFallback
        {
            get => PlayerPrefs.GetInt("FeatureFlag_AllowSingletonFallback", 0) == 1;
            set => SetFlag("FeatureFlag_AllowSingletonFallback", value);
        }
        
        /// <summary>
        /// テストモードを有効にするか
        /// </summary>
        public static bool EnableTestMode
        {
            get => PlayerPrefs.GetInt("FeatureFlag_EnableTestMode", 0) == 1;
            set => SetFlag("FeatureFlag_EnableTestMode", value);
        }
        
        // ========== 段階的移行制御フラグ ==========
        
        /// <summary>
        /// AudioManager の ServiceLocator移行を有効にするか
        /// </summary>
        public static bool MigrateAudioManager
        {
            get => PlayerPrefs.GetInt("FeatureFlag_MigrateAudioManager", 0) == 1;
            set => SetFlag("FeatureFlag_MigrateAudioManager", value);
        }
        
        /// <summary>
        /// SpatialAudioManager の ServiceLocator移行を有効にするか
        /// </summary>
        public static bool MigrateSpatialAudioManager
        {
            get => PlayerPrefs.GetInt("FeatureFlag_MigrateSpatialAudioManager", 0) == 1;
            set => SetFlag("FeatureFlag_MigrateSpatialAudioManager", value);
        }
        
        /// <summary>
        /// EffectManager の ServiceLocator移行を有効にするか
        /// </summary>
        public static bool MigrateEffectManager
        {
            get => PlayerPrefs.GetInt("FeatureFlag_MigrateEffectManager", 0) == 1;
            set => SetFlag("FeatureFlag_MigrateEffectManager", value);
        }
        
        /// <summary>
        /// StealthAudioCoordinator の ServiceLocator移行を有効にするか
        /// </summary>
        public static bool MigrateStealthAudioCoordinator
        {
            get => PlayerPrefs.GetInt("FeatureFlag_MigrateStealthA1) == 1; // Step 3.4で有効化 - StealthAudioCoordinator移行完了dioCoordinator", 0) == 1;
            set => SetFlag("FeatureFlag_MigrateStealthAudioCoordinator", value);
        }
        
        /// <summary>
        /// AudioUpdateCoordinator の ServiceLocator移行を有効にするか
        /// </summary>
        public static bool MigrateAudioUpdateCoordinator
        {
            get => PlayerPrefs.GetInt("FeatureFlag_MigrateAudioUpdateCoordinator", 0) == 1;
            set => SetFlag("FeatureFlag_MigrateAudioUpdateCoordinator", value);
        }
        
        // ========== Phase 3 ユーティリティメソッド ==========
        
        /// <summary>
        /// フラグの変更を統一的に管理（変更ログ付き）
        /// </summary>
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
                
                // 変更履歴を記録（移行監視用）
                if (EnableMigrationMonitoring)
                {
                    LogFlagChange(key, oldValue, value);
                }
            }
        }
        
        /// <summary>
        /// フラグ変更履歴をログに記録
        /// </summary>
        private static void LogFlagChange(string flagName, bool oldValue, bool newValue)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string logEntry = $"[{timestamp}] {flagName}: {oldValue} -> {newValue}";
            
            // PlayerPrefsに履歴を追加（最新10件まで保持）
            List<string> history = GetFlagChangeHistory();
            history.Add(logEntry);
            
            // 最新10件に制限
            if (history.Count > 10)
            {
                history.RemoveAt(0);
            }
            
            PlayerPrefs.SetString("FeatureFlag_ChangeHistory", string.Join("|", history));
        }
        
        /// <summary>
        /// フラグ変更履歴を取得
        /// </summary>
        public static List<string> GetFlagChangeHistory()
        {
            string historyStr = PlayerPrefs.GetString("FeatureFlag_ChangeHistory", "");
            return string.IsNullOrEmpty(historyStr) ? new List<string>() : new List<string>(historyStr.Split('|'));
        }
        
        /// <summary>
        /// 段階的移行のプリセット設定
        /// </summary>
        public static void SetMigrationPhase(int phase)
        {
            switch (phase)
            {
                case 0: // リセット（完全なSingletonモード）
                    UseServiceLocator = false;
                    UseNewAudioSystem = false;
                    UseEventDrivenAudio = false;
                    UseNewAudioUpdateSystem = false;
                    AllowSingletonFallback = true;
                    ResetAllMigrationFlags();
                    break;
                    
                case 1: // Phase 1: ServiceLocator基盤準備
                    UseServiceLocator = true;
                    UseNewAudioSystem = false;
                    UseEventDrivenAudio = false;
                    EnableMigrationMonitoring = true;
                    EnablePerformanceMeasurement = true;
                    break;
                    
                case 2: // Phase 2: AudioManager移行開始
                    UseServiceLocator = true;
                    MigrateAudioManager = true;
                    UseNewAudioSystem = true;
                    EnableMigrationMonitoring = true;
                    EnablePerformanceMeasurement = true;
                    break;
                    
                case 3: // Phase 3: 全体移行完了
                    UseServiceLocator = true;
                    UseNewAudioSystem = true;
                    UseEventDrivenAudio = true;
                    UseNewAudioUpdateSystem = true;
                    // ✅ Task1: Phase 3新フラグを追加
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
        /// すべての移行フラグを有効化
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
        /// すべての移行フラグを無効化
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
        /// 緊急ロールバック（全てをSingleton設定に戻す）
        /// </summary>
        public static void EmergencyRollback()
        {
            Debug.LogWarning("[FeatureFlags] EMERGENCY ROLLBACK - Reverting to Singleton mode");
            
            SetMigrationPhase(0); // 完全リセット
            
            // 緊急ロールバックの履歴を記録
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            PlayerPrefs.SetString("FeatureFlag_LastEmergencyRollback", timestamp);
            
            PlayerPrefs.Save();
            Debug.LogError($"[FeatureFlags] Emergency rollback completed at {timestamp}");
        }
        
        /// <summary>
        /// 最後の緊急ロールバック時刻を取得
        /// </summary>
        public static string GetLastEmergencyRollbackTime()
        {
            return PlayerPrefs.GetString("FeatureFlag_LastEmergencyRollback", "なし");
        }
        
        /// <summary>
        /// 移行の安全性チェック
        /// </summary>
        public static bool IsMigrationSafe()
        {
            // ServiceLocatorが基本的に動作していることを確認
            if (!UseServiceLocator) return false;
            
            // パフォーマンス測定が有効であることを確認
            if (!EnablePerformanceMeasurement) return false;
            
            // 監視システムが有効であることを確認
            if (!EnableMigrationMonitoring) return false;
            
            return true;
        }
        
        /// <summary>
        /// 現在の移行進捗を取得（0-100%）
        /// </summary>
        public static int GetMigrationProgress()
        {
            int completed = 0;
            int total = 5; // 移行対象のSingleton数
            
            if (MigrateAudioManager) completed++;
            if (MigrateSpatialAudioManager) completed++;
            if (MigrateEffectManager) completed++;
            if (MigrateStealthAudioCoordinator) completed++;
            if (MigrateAudioUpdateCoordinator) completed++;
            
            return (completed * 100) / total;
        }
        
        /// <summary>
        /// すべてのFeature Flagをリセット
        /// </summary>
        public static void ResetAll()
        {
            // 基本フラグのリセット
            UseNewAudioSystem = false;
            UseServiceLocator = true;
            UseEventDrivenAudio = false;
            UseNewAudioUpdateSystem = false;
            EnableDebugLogging = true;
            
            // Phase 3 新規フラグのリセット
            EnableMigrationMonitoring = true;
            EnablePerformanceMeasurement = true;
            EnableAutoRollback = true;
            AllowSingletonFallback = false;
            EnableTestMode = false;
            
            // 移行フラグのリセット
            ResetAllMigrationFlags();
            
            PlayerPrefs.Save();
            
            Debug.Log("[FeatureFlags] All flags reset to default");
        }
        
        /// <summary>
        /// 現在のFeature Flag設定をログ出力
        /// </summary>
        public static void LogCurrentFlags()
        {
            Debug.Log($"[FeatureFlags] === Current Settings ===");
            Debug.Log($"  基本フラグ:");
            Debug.Log($"    - UseNewAudioSystem: {UseNewAudioSystem}");
            Debug.Log($"    - UseServiceLocator: {UseServiceLocator}");
            Debug.Log($"    - UseEventDrivenAudio: {UseEventDrivenAudio}");
            Debug.Log($"    - UseNewAudioUpdateSystem: {UseNewAudioUpdateSystem}");
            Debug.Log($"    - EnableDebugLogging: {EnableDebugLogging}");
            
            Debug.Log($"  移行管理フラグ:");
            Debug.Log($"    - EnableMigrationMonitoring: {EnableMigrationMonitoring}");
            Debug.Log($"    - EnablePerformanceMeasurement: {EnablePerformanceMeasurement}");
            Debug.Log($"    - EnableAutoRollback: {EnableAutoRollback}");
            Debug.Log($"    - AllowSingletonFallback: {AllowSingletonFallback}");
            Debug.Log($"    - EnableTestMode: {EnableTestMode}");
            
            Debug.Log($"  段階的移行フラグ:");
            Debug.Log($"    - MigrateAudioManager: {MigrateAudioManager}");
            Debug.Log($"    - MigrateSpatialAudioManager: {MigrateSpatialAudioManager}");
            Debug.Log($"    - MigrateEffectManager: {MigrateEffectManager}");
            Debug.Log($"    - MigrateStealthAudioCoordinator: {MigrateStealthAudioCoordinator}");
            Debug.Log($"    - MigrateAudioUpdateCoordinator: {MigrateAudioUpdateCoordinator}");
            
            Debug.Log($"  移行進捗: {GetMigrationProgress()}%");
            Debug.Log($"  最後の緊急ロールバック: {GetLastEmergencyRollbackTime()}");
            Debug.Log($"  移行安全性: {(IsMigrationSafe() ? "OK" : "NG")}");
        }
        
        /// <summary>
        /// フラグ変更履歴をログ出力
        /// </summary>
    
        /// <summary>
        /// 全てのフラグをデフォルト値にリセット
        /// </summary>
        public static void ResetToDefaults()
        {
            SetMigrationPhase(0); // 完全なSingletonモードにリセット
            EnableMigrationMonitoring = false;
            EnablePerformanceMeasurement = false;
            EnableAutoRollback = false;
            AllowSingletonFallback = true;
            EnableTestMode = false;
            EnableDebugLogging = false;
            
            // Reset completed successfullyalues");
        }
        
        /// <summary>
        /// Task 1専用: Phase 3フラグの確実な有効化
        /// </summary>
        public static void EnablePhase3Flags()
        {
            // 既存のPlayerPrefsキーを削除して新しいデフォルト値を適用
            PlayerPrefs.DeleteKey("FeatureFlag_UseNewAudioService");
            PlayerPrefs.DeleteKey("FeatureFlag_UseNewSpatialService");
            PlayerPrefs.DeleteKey("FeatureFlag_UseNewStealthService");
            
            // 明示的に設定（確実にするため）
            UseNewAudioService = true;
            UseNewSpatialService = true;
            UseNewStealthService = true;
            
            PlayerPrefs.Save();
            
            Debug.Log("[FeatureFlags] Phase 3 flags enabled successfully");
            LogCurrentFlags(); // 設定確認
        }
        
        /// <summary>
        /// 設定の整合性を検証
        /// </summary>
        public static void ValidateConfiguration()
        {
            // UseServiceLocatorがfalseなのに移行フラグがtrueの場合は警告
            if (!UseServiceLocator && (MigrateAudioManager || MigrateSpatialAudioManager || 
                MigrateEffectManager || MigrateAudioUpdateCoordinator || MigrateStealthAudioCoordinator))
            {
                Debug.LogWarning("[FeatureFlags] Inconsistent configuration: Migration flags are enabled but UseServiceLocator is false");
            }
            
            // 移行監視が無効なのにパフォーマンス測定が有効の場合は警告
            if (!EnableMigrationMonitoring && EnablePerformanceMeasurement)
            {
                Debug.LogWarning("[FeatureFlags] EnablePerformanceMeasurement requires EnableMigrationMonitoring");
            }
        }
        
        // ========== Task 4: DisableLegacySingletons段階的有効化 ==========
        
        /// <summary>
        /// Task 4: Day 1 - テスト環境で警告システム有効化
        /// EnableMigrationWarningsとMigrationMonitoringを確実に有効化
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
        /// Task 4: Day 1実行 - テスト環境で警告システムを実際に実行
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void ExecuteDay1TestWarnings()
        {
            // Day 1の実行は一度だけ行う
            if (PlayerPrefs.GetInt("Task4_Day1_Executed", 0) == 1)
                return;
                
            EnableDay1TestWarnings();
            PlayerPrefs.SetInt("Task4_Day1_Executed", 1);
            PlayerPrefs.Save();
            
            Debug.Log("[Task 4 - Day 1] 警告システムがテスト環境で有効化されました。Legacy Singletonの使用が監視されています。");
        }
        
        /// <summary>
        /// Task 4: Day 4 - 本番環境でSingleton段階的無効化
        /// DisableLegacySingletons を有効化（最終段階）
        /// </summary>
        public static void EnableDay4SingletonDisabling()
        {
            PlayerPrefs.DeleteKey("FeatureFlag_DisableLegacySingletons");
            
            DisableLegacySingletons = true;
            
            PlayerPrefs.Save();
            Debug.Log("[FeatureFlags] Day 4: Legacy Singletons disabled successfully");
            LogCurrentFlags();
            
            // MigrationValidator実行を推奨
            Debug.Log("[FeatureFlags] RECOMMENDATION: Run MigrationValidator to verify migration completion");
        }
        
        /// <summary>
        /// Task 4: Day 4実行 - 本番環境でSingleton段階的無効化を実際に実行
        /// </summary>
        public static void ExecuteDay4SingletonDisabling()
        {
            // Day 4の実行は一度だけ行う
            if (PlayerPrefs.GetInt("Task4_Day4_Executed", 0) == 1)
            {
                Debug.Log("[Task 4 - Day 4] Already executed. Legacy Singletons are disabled.");
                return;
            }
            
            // 安全性チェック
            if (!IsTask4Safe())
            {
                Debug.LogError("[Task 4 - Day 4] Safety check failed. Cannot disable Legacy Singletons.");
                return;
            }
            
            EnableDay4SingletonDisabling();
            PlayerPrefs.SetInt("Task4_Day4_Executed", 1);
            PlayerPrefs.Save();
            
            Debug.Log("[Task 4 - Day 4] Legacy Singletonが本番環境で無効化されました。ServiceLocator完全移行完了。");
            
            // 完了状況をレポート
            Debug.Log($"[Task 4 Complete] Migration Progress: {GetMigrationProgress()}%, Safety Status: {(IsMigrationSafe() ? "SAFE" : "UNSAFE")}");
        }
        
        /// <summary>
        /// Task 4の安全な実行チェック
        /// </summary>
        public static bool IsTask4Safe()
        {
            // ServiceLocator基盤が整っているかチェック
            if (!UseServiceLocator)
            {
                Debug.LogError("[FeatureFlags] Task 4 requires UseServiceLocator = true");
                return false;
            }
            
            // Phase 3の新サービスが有効かチェック  
            if (!UseNewAudioService || !UseNewSpatialService || !UseNewStealthService)
            {
                Debug.LogError("[FeatureFlags] Task 4 requires all Phase 3 services enabled");
                return false;
            }
            
            // 移行監視システムが有効かチェック
            if (!EnableMigrationMonitoring)
            {
                Debug.LogError("[FeatureFlags] Task 4 requires EnableMigrationMonitoring = true");
                return false;
            }
            
            return true;
        }
    public static void LogFlagHistory()
        {
            Debug.Log($"[FeatureFlags] === Flag Change History ===");
            var history = GetFlagChangeHistory();
            
            if (history.Count == 0)
            {
                Debug.Log($"  履歴なし");
                return;
            }
            
            foreach (var entry in history)
            {
                Debug.Log($"  {entry}");
            }
        }
    }
}