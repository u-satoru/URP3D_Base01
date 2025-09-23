using UnityEngine;
using System;
using System.Collections.Generic;
// // using asterivo.Unity60.Core.Debug; // Removed to avoid circular dependency

namespace asterivo.Unity60.Core
{
    /// <summary>
    /// Phase 3 強化版 Feature Flag シスチE��
    /// 段階的移行、パフォーマンス監視、ロールバック機�Eを統吁E    /// </summary>
    public static class FeatureFlags
    {
        // ========== 既存�E基本フラグ ==========
        
        /// <summary>
        /// 新しいオーチE��オシスチE��を使用するぁE        /// </summary>
        public static bool UseNewAudioSystem 
        {
            get => PlayerPrefs.GetInt("FeatureFlag_UseNewAudioSystem", 0) == 1;
            set => SetFlag("FeatureFlag_UseNewAudioSystem", value);
        }
        
        /// <summary>
        /// Service Locatorパターンを使用するぁE        /// </summary>
        public static bool UseServiceLocator
        {
            get => PlayerPrefs.GetInt("FeatureFlag_UseServiceLocator", 1) == 1;
            set => SetFlag("FeatureFlag_UseServiceLocator", value);
        }
        
        /// <summary>
        /// イベント駁E��音響シスチE��を使用するぁE        /// </summary>
        public static bool UseEventDrivenAudio
        {
            get => PlayerPrefs.GetInt("FeatureFlag_UseEventDrivenAudio", 0) == 1;
            set => SetFlag("FeatureFlag_UseEventDrivenAudio", value);
        }
        
        /// <summary>
        /// 新しいAudioUpdateCoordinatorサービスを使用するぁE        /// </summary>
        public static bool UseNewAudioUpdateSystem
        {
            get => PlayerPrefs.GetInt("FeatureFlag_UseNewAudioUpdateSystem", 0) == 1;
            set => SetFlag("FeatureFlag_UseNewAudioUpdateSystem", value);
        }
        
        /// <summary>
        /// チE��チE��ログを有効にするぁE        /// </summary>
        public static bool EnableDebugLogging
        {
            get => PlayerPrefs.GetInt("FeatureFlag_EnableDebugLogging", 1) == 1;
            set => SetFlag("FeatureFlag_EnableDebugLogging", value);
        }
        
        /// <summary>
        /// リファクタリング後�EアーキチE��チャを使用するか（段階的移行用�E�E        /// </summary>
        public static bool UseRefactoredArchitecture
        {
            get => PlayerPrefs.GetInt("FeatureFlag_UseRefactoredArchitecture", 0) == 1;
            set => SetFlag("FeatureFlag_UseRefactoredArchitecture", value);
        }
        
                
        /// <summary>
        /// 新しいAudioServiceを使用するか！Etep 3.7用�E�E        /// </summary>
        public static bool UseNewAudioService
        {
            get => PlayerPrefs.GetInt("FeatureFlag_UseNewAudioService", 1) == 1; // ✁ETask1: チE��ォルト値めEに変更
            set => SetFlag("FeatureFlag_UseNewAudioService", value);
        }
        
        /// <summary>
        /// 新しいSpatialAudioServiceを使用するか！Etep 3.7用�E�E        /// </summary>
        public static bool UseNewSpatialService
        {
            get => PlayerPrefs.GetInt("FeatureFlag_UseNewSpatialService", 1) == 1; // ✁ETask1: チE��ォルト値めEに変更
            set => SetFlag("FeatureFlag_UseNewSpatialService", value);
        }
        
        /// <summary>
        /// 新しいStealthAudioServiceを使用するか！Etep 3.7用�E�E        /// </summary>
        public static bool UseNewStealthService
        {
            get => PlayerPrefs.GetInt("FeatureFlag_UseNewStealthService", 1) == 1; // ✁ETask1: チE��ォルト値めEに変更
            set => SetFlag("FeatureFlag_UseNewStealthService", value);
        }
        
        /// <summary>
        /// パフォーマンス監視を有効にするか！Etep 3.7用�E�E        /// </summary>
        public static bool EnablePerformanceMonitoring
        {
            get => PlayerPrefs.GetInt("FeatureFlag_EnablePerformanceMonitoring", 1) == 1;
            set => SetFlag("FeatureFlag_EnablePerformanceMonitoring", value);
        }
// ========== Step 3.9 Legacy Singleton警告シスチE�� ==========
        
        /// <summary>
        /// Legacy Singleton使用時に警告を表示するぁE        /// </summary>
        public static bool EnableMigrationWarnings
        {
            get => PlayerPrefs.GetInt("FeatureFlag_EnableMigrationWarnings", 1) == 1;
            set => SetFlag("FeatureFlag_EnableMigrationWarnings", value);
        }
        
        /// <summary>
        /// Legacy Singletonへのアクセスを完�Eに禁止するぁE        /// </summary>
        public static bool DisableLegacySingletons
        {
            get => PlayerPrefs.GetInt("FeatureFlag_DisableLegacySingletons", 0) == 1;
            set => SetFlag("FeatureFlag_DisableLegacySingletons", value);
        }
        
// ========== Phase 3 新規移行管琁E��ラグ ==========
        
        /// <summary>
        /// 移行�Eロセス監視を有効にするぁE        /// </summary>
        public static bool EnableMigrationMonitoring
        {
            get => PlayerPrefs.GetInt("FeatureFlag_EnableMigrationMonitoring", 1) == 1;
            set => SetFlag("FeatureFlag_EnableMigrationMonitoring", value);
        }
        
        /// <summary>
        /// パフォーマンス測定を有効にするぁE        /// </summary>
        public static bool EnablePerformanceMeasurement
        {
            get => PlayerPrefs.GetInt("FeatureFlag_EnablePerformanceMeasurement", 1) == 1;
            set => SetFlag("FeatureFlag_EnablePerformanceMeasurement", value);
        }
        
        /// <summary>
        /// 自動ロールバック機�Eを有効にするぁE        /// </summary>
        public static bool EnableAutoRollback
        {
            get => PlayerPrefs.GetInt("FeatureFlag_EnableAutoRollback", 1) == 1;
            set => SetFlag("FeatureFlag_EnableAutoRollback", value);
        }
        
        /// <summary>
        /// Singletonの使用を許可するか（緊急時ロールバック用�E�E        /// </summary>
        public static bool AllowSingletonFallback
        {
            get => PlayerPrefs.GetInt("FeatureFlag_AllowSingletonFallback", 0) == 1;
            set => SetFlag("FeatureFlag_AllowSingletonFallback", value);
        }
        
        /// <summary>
        /// チE��トモードを有効にするぁE        /// </summary>
        public static bool EnableTestMode
        {
            get => PlayerPrefs.GetInt("FeatureFlag_EnableTestMode", 0) == 1;
            set => SetFlag("FeatureFlag_EnableTestMode", value);
        }
        
        // ========== 段階的移行制御フラグ ==========
        
        /// <summary>
        /// AudioManager の ServiceLocator移行を有効にするぁE        /// </summary>
        public static bool MigrateAudioManager
        {
            get => PlayerPrefs.GetInt("FeatureFlag_MigrateAudioManager", 0) == 1;
            set => SetFlag("FeatureFlag_MigrateAudioManager", value);
        }
        
        /// <summary>
        /// SpatialAudioManager の ServiceLocator移行を有効にするぁE        /// </summary>
        public static bool MigrateSpatialAudioManager
        {
            get => PlayerPrefs.GetInt("FeatureFlag_MigrateSpatialAudioManager", 0) == 1;
            set => SetFlag("FeatureFlag_MigrateSpatialAudioManager", value);
        }
        
        /// <summary>
        /// EffectManager の ServiceLocator移行を有効にするぁE        /// </summary>
        public static bool MigrateEffectManager
        {
            get => PlayerPrefs.GetInt("FeatureFlag_MigrateEffectManager", 0) == 1;
            set => SetFlag("FeatureFlag_MigrateEffectManager", value);
        }
        
        /// <summary>
        /// StealthAudioCoordinator の ServiceLocator移行を有効にするぁE        /// </summary>
        public static bool MigrateStealthAudioCoordinator
        {
            get => PlayerPrefs.GetInt("FeatureFlag_MigrateStealthAudioCoordinator", 0) == 1;
            set => SetFlag("FeatureFlag_MigrateStealthAudioCoordinator", value);
        }
        
        /// <summary>
        /// AudioUpdateCoordinator の ServiceLocator移行を有効にするぁE        /// </summary>
        public static bool MigrateAudioUpdateCoordinator
        {
            get => PlayerPrefs.GetInt("FeatureFlag_MigrateAudioUpdateCoordinator", 0) == 1;
            set => SetFlag("FeatureFlag_MigrateAudioUpdateCoordinator", value);
        }
        
        // ========== Phase 3 ユーチE��リチE��メソチE�� ==========
        
        /// <summary>
        /// フラグの変更を統一皁E��管琁E��変更ログ付き�E�E        /// </summary>
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
                
                // 変更履歴を記録�E�移行監視用�E�E                if (EnableMigrationMonitoring)
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
            
            // PlayerPrefsに履歴を追加�E�最新10件まで保持�E�E            List<string> history = GetFlagChangeHistory();
            history.Add(logEntry);
            
            // 最新10件に制陁E            if (history.Count > 10)
            {
                history.RemoveAt(0);
            }
            
            PlayerPrefs.SetString("FeatureFlag_ChangeHistory", string.Join("|", history));
        }
        
        /// <summary>
        /// フラグ変更履歴を取征E        /// </summary>
        public static List<string> GetFlagChangeHistory()
        {
            string historyStr = PlayerPrefs.GetString("FeatureFlag_ChangeHistory", "");
            return string.IsNullOrEmpty(historyStr) ? new List<string>() : new List<string>(historyStr.Split('|'));
        }
        
        /// <summary>
        /// 段階的移行�EプリセチE��設宁E        /// </summary>
        public static void SetMigrationPhase(int phase)
        {
            switch (phase)
            {
                case 0: // リセチE���E�完�EなSingletonモード！E                    UseServiceLocator = false;
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
                    
                case 2: // Phase 2: AudioManager移行開姁E                    UseServiceLocator = true;
                    MigrateAudioManager = true;
                    UseNewAudioSystem = true;
                    EnableMigrationMonitoring = true;
                    EnablePerformanceMeasurement = true;
                    break;
                    
                case 3: // Phase 3: 全体移行完亁E                    UseServiceLocator = true;
                    UseNewAudioSystem = true;
                    UseEventDrivenAudio = true;
                    UseNewAudioUpdateSystem = true;
                    // ✁ETask1: Phase 3新フラグを追加
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
        /// すべての移行フラグを有効匁E        /// </summary>
        private static void EnableAllMigrationFlags()
        {
            MigrateAudioManager = true;
            MigrateSpatialAudioManager = true;
            MigrateEffectManager = true;
            MigrateStealthAudioCoordinator = true;
            MigrateAudioUpdateCoordinator = true;
        }
        
        /// <summary>
        /// すべての移行フラグを無効匁E        /// </summary>
        private static void ResetAllMigrationFlags()
        {
            MigrateAudioManager = false;
            MigrateSpatialAudioManager = false;
            MigrateEffectManager = false;
            MigrateStealthAudioCoordinator = false;
            MigrateAudioUpdateCoordinator = false;
        }
        
        /// <summary>
        /// 緊急ロールバック�E��EてをSingleton設定に戻す！E        /// </summary>
        public static void EmergencyRollback()
        {
            Debug.LogWarning("[FeatureFlags] EMERGENCY ROLLBACK - Reverting to Singleton mode");
            
            SetMigrationPhase(0); // 完�EリセチE��
            
            // 緊急ロールバックの履歴を記録
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            PlayerPrefs.SetString("FeatureFlag_LastEmergencyRollback", timestamp);
            
            PlayerPrefs.Save();
            Debug.LogError($"[FeatureFlags] Emergency rollback completed at {timestamp}");
        }
        
        /// <summary>
        /// 最後�E緊急ロールバック時刻を取征E        /// </summary>
        public static string GetLastEmergencyRollbackTime()
        {
            return PlayerPrefs.GetString("FeatureFlag_LastEmergencyRollback", "なぁE);
        }
        
        /// <summary>
        /// 移行�E安�E性チェチE��
        /// </summary>
        public static bool IsMigrationSafe()
        {
            // ServiceLocatorが基本皁E��動作してぁE��ことを確誁E            if (!UseServiceLocator) return false;
            
            // パフォーマンス測定が有効であることを確誁E            if (!EnablePerformanceMeasurement) return false;
            
            // 監視シスチE��が有効であることを確誁E            if (!EnableMigrationMonitoring) return false;
            
            return true;
        }
        
        /// <summary>
        /// 現在の移行進捗を取得！E-100%�E�E        /// </summary>
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
        /// すべてのFeature FlagをリセチE��
        /// </summary>
        public static void ResetAll()
        {
            // 基本フラグのリセチE��
            UseNewAudioSystem = false;
            UseServiceLocator = true;
            UseEventDrivenAudio = false;
            UseNewAudioUpdateSystem = false;
            EnableDebugLogging = true;
            
            // Phase 3 新規フラグのリセチE��
            EnableMigrationMonitoring = true;
            EnablePerformanceMeasurement = true;
            EnableAutoRollback = true;
            AllowSingletonFallback = false;
            EnableTestMode = false;
            
            // 移行フラグのリセチE��
            ResetAllMigrationFlags();
            
            PlayerPrefs.Save();
            
            Debug.Log("[FeatureFlags] All flags reset to default");
        }
        
        /// <summary>
        /// 現在のFeature Flag設定をログ出劁E        /// </summary>
        public static void LogCurrentFlags()
        {
            Debug.Log($"[FeatureFlags] === Current Settings ===");
            Debug.Log($"  基本フラグ:");
            Debug.Log($"    - UseNewAudioSystem: {UseNewAudioSystem}");
            Debug.Log($"    - UseServiceLocator: {UseServiceLocator}");
            Debug.Log($"    - UseEventDrivenAudio: {UseEventDrivenAudio}");
            Debug.Log($"    - UseNewAudioUpdateSystem: {UseNewAudioUpdateSystem}");
            Debug.Log($"    - EnableDebugLogging: {EnableDebugLogging}");
            
            Debug.Log($"  移行管琁E��ラグ:");
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
            
            Debug.Log($"  移行進捁E {GetMigrationProgress()}%");
            Debug.Log($"  最後�E緊急ロールバック: {GetLastEmergencyRollbackTime()}");
            Debug.Log($"  移行安�E性: {(IsMigrationSafe() ? "OK" : "NG")}");
        }
        
        /// <summary>
        /// フラグ変更履歴をログ出劁E        /// </summary>
    
        /// <summary>
        /// 全てのフラグをデフォルト値にリセチE��
        /// </summary>
        public static void ResetToDefaults()
        {
            SetMigrationPhase(0); // 完�EなSingletonモードにリセチE��
            EnableMigrationMonitoring = false;
            EnablePerformanceMeasurement = false;
            EnableAutoRollback = false;
            AllowSingletonFallback = true;
            EnableTestMode = false;
            EnableDebugLogging = false;
            
            // Reset completed successfullyalues");
        }
        
        /// <summary>
        /// Task 1専用: Phase 3フラグの確実な有効匁E        /// </summary>
        public static void EnablePhase3Flags()
        {
            // 既存�EPlayerPrefsキーを削除して新しいチE��ォルト値を適用
            PlayerPrefs.DeleteKey("FeatureFlag_UseNewAudioService");
            PlayerPrefs.DeleteKey("FeatureFlag_UseNewSpatialService");
            PlayerPrefs.DeleteKey("FeatureFlag_UseNewStealthService");
            
            // 明示皁E��設定（確実にするため�E�E            UseNewAudioService = true;
            UseNewSpatialService = true;
            UseNewStealthService = true;
            
            PlayerPrefs.Save();
            
            Debug.Log("[FeatureFlags] Phase 3 flags enabled successfully");
            LogCurrentFlags(); // 設定確誁E        }
        
        /// <summary>
        /// 設定�E整合性を検証
        /// </summary>
        public static void ValidateConfiguration()
        {
            // UseServiceLocatorがfalseなのに移行フラグがtrueの場合�E警呁E            if (!UseServiceLocator && (MigrateAudioManager || MigrateSpatialAudioManager || 
                MigrateEffectManager || MigrateAudioUpdateCoordinator || MigrateStealthAudioCoordinator))
            {
                Debug.LogWarning("[FeatureFlags] Inconsistent configuration: Migration flags are enabled but UseServiceLocator is false");
            }
            
            // 移行監視が無効なのにパフォーマンス測定が有効の場合�E警呁E            if (!EnableMigrationMonitoring && EnablePerformanceMeasurement)
            {
                Debug.LogWarning("[FeatureFlags] EnablePerformanceMeasurement requires EnableMigrationMonitoring");
            }
            
            // DisableLegacySingletons と AllowSingletonFallback の競吁E            if (DisableLegacySingletons && AllowSingletonFallback)
            {
                Debug.LogWarning("[FeatureFlags] Inconsistent configuration: DisableLegacySingletons=true conflicts with AllowSingletonFallback=true");
            }
            
            // Day4前提: DisableLegacySingletonsが有効ならPhase3新サービスは全て有効が安�E
            if (DisableLegacySingletons && (!UseNewAudioService || !UseNewSpatialService || !UseNewStealthService))
            {
                Debug.LogWarning("[FeatureFlags] DisableLegacySingletons requires Phase 3 services enabled (UseNewAudio/Spatial/Stealth)");
            }
        }

        /// <summary>
        /// 整合性を強制�E�EutoFix=trueで安�E側に自動補正�E�E        /// </summary>
        public static void EnforceConsistency(bool autoFix = false)
        {
            bool changed = false;
            
            if (!UseServiceLocator)
            {
                // 移行フラグは無効が安�E
                if (MigrateAudioManager && autoFix) { MigrateAudioManager = false; changed = true; }
                if (MigrateSpatialAudioManager && autoFix) { MigrateSpatialAudioManager = false; changed = true; }
                if (MigrateEffectManager && autoFix) { MigrateEffectManager = false; changed = true; }
                if (MigrateStealthAudioCoordinator && autoFix) { MigrateStealthAudioCoordinator = false; changed = true; }
                if (MigrateAudioUpdateCoordinator && autoFix) { MigrateAudioUpdateCoordinator = false; changed = true; }
                
                // Legacy無効化�E危険なので解除
                if (DisableLegacySingletons && autoFix) { DisableLegacySingletons = false; changed = true; }
            }

            // 監視なしで計測は無効匁E            if (!EnableMigrationMonitoring && EnablePerformanceMeasurement && autoFix)
            {
                EnablePerformanceMeasurement = false; changed = true;
            }

            // 競合解涁E Legacy無効化とSingletonフォールバック同時は不可
            if (DisableLegacySingletons && AllowSingletonFallback && autoFix)
            {
                AllowSingletonFallback = false; changed = true;
            }

            // Day4安�E性: Legacyを無効にするならPhase3新サービスを有効匁E            if (DisableLegacySingletons && autoFix)
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
        
        // ========== Task 4: DisableLegacySingletons段階的有効匁E==========
        
        /// <summary>
        /// Task 4: Day 1 - チE��ト環墁E��警告シスチE��有効匁E        /// EnableMigrationWarningsとMigrationMonitoringを確実に有効匁E        /// </summary>
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
        /// Task 4: Day 1実衁E- チE��ト環墁E��警告シスチE��を実際に実衁E        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void ExecuteDay1TestWarnings()
        {
            // Day 1の実行�E一度だけ行う
            if (PlayerPrefs.GetInt("Task4_Day1_Executed", 0) == 1)
                return;
                
            EnableDay1TestWarnings();
            PlayerPrefs.SetInt("Task4_Day1_Executed", 1);
            PlayerPrefs.Save();
            
            Debug.Log("[Task 4 - Day 1] 警告シスチE��がテスト環墁E��有効化されました、Eegacy Singletonの使用が監視されてぁE��す、E);
        }
        
        /// <summary>
        /// Task 4: Day 4 - 本番環墁E��Singleton段階的無効匁E        /// DisableLegacySingletons を有効化（最終段階！E        /// </summary>
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
        /// Task 4: Day 4実衁E- 本番環墁E��Singleton段階的無効化を実際に実衁E        /// </summary>
        public static void ExecuteDay4SingletonDisabling()
        {
            // Day 4の実行�E一度だけ行う
            if (PlayerPrefs.GetInt("Task4_Day4_Executed", 0) == 1)
            {
                Debug.Log("[Task 4 - Day 4] Already executed. Legacy Singletons are disabled.");
                return;
            }
            
            // 安�E性チェチE��
            if (!IsTask4Safe())
            {
                Debug.LogError("[Task 4 - Day 4] Safety check failed. Cannot disable Legacy Singletons.");
                return;
            }
            
            EnableDay4SingletonDisabling();
            PlayerPrefs.SetInt("Task4_Day4_Executed", 1);
            PlayerPrefs.Save();
            
            Debug.Log("[Task 4 - Day 4] Legacy Singletonが本番環墁E��無効化されました。ServiceLocator完�E移行完亁E��E);
            
            // 完亁E��況をレポ�EチE            Debug.Log($"[Task 4 Complete] Migration Progress: {GetMigrationProgress()}%, Safety Status: {(IsMigrationSafe() ? "SAFE" : "UNSAFE")}");
        }
        
        /// <summary>
        /// Task 4の安�Eな実行チェチE��
        /// </summary>
        public static bool IsTask4Safe()
        {
            // ServiceLocator基盤が整ってぁE��かチェチE��
            if (!UseServiceLocator)
            {
                Debug.LogError("[FeatureFlags] Task 4 requires UseServiceLocator = true");
                return false;
            }
            
            // Phase 3の新サービスが有効かチェチE��  
            if (!UseNewAudioService || !UseNewSpatialService || !UseNewStealthService)
            {
                Debug.LogError("[FeatureFlags] Task 4 requires all Phase 3 services enabled");
                return false;
            }
            
            // 移行監視シスチE��が有効かチェチE��
            if (!EnableMigrationMonitoring)
            {
                Debug.LogError("[FeatureFlags] Task 4 requires EnableMigrationMonitoring = true");
                return false;
            }
            
            return true;
        }

        /// <summary>
        /// 起動時の構�E検証フック�E�忁E��一度実行！E        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        private static void RuntimeValidateOnStartup()
        {
            try
            {
                ValidateConfiguration();
                // 自動修正は行わず警告�Eみ。忁E��に応じて起動スクリプトからEnforceConsistency(true)を呼び出ぁE                EnforceConsistency(false);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[FeatureFlags] Runtime validation failed: {e.Message}");
            }
        }
        
        /// <summary>
        /// Phase 1.2: SINGLETON_COMPLETE_REMOVAL_GUIDE.md準拠 - 匁E��皁E��チE��アチE�E作�Eと最終設宁E        /// 完�ESingleton削除準備のためのFeatureFlags最終設定を適用
        /// </summary>
        public static void ExecutePhase1ComprehensiveBackupAndFinalSettings()
        {
            Debug.Log("[FeatureFlags] === Phase 1.2: 匁E��皁E��チE��アチE�E作�Eと最終設定実衁E===");
            
            // Step 1: 現在設定�EバックアチE�E作�E
            string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmm");
            string backupKey = $"Phase1_Backup_{timestamp}";
            
            Debug.Log($"[FeatureFlags] Step 1: Creating comprehensive backup: {backupKey}");
            
            // 現在の設定をログ出力（バチE��アチE�E目皁E��E            LogCurrentFlags();
            
            // バックアチE�EをPlayerPrefsに保孁E            string featureFlagsBackup = SerializeCurrentFeatureFlags();
            PlayerPrefs.SetString($"{backupKey}_FeatureFlags", featureFlagsBackup);
            PlayerPrefs.SetString($"{backupKey}_Timestamp", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            PlayerPrefs.SetString("LastPhase1Backup", backupKey);
            
            Debug.Log($"[FeatureFlags] ✁EComprehensive backup created: {backupKey}");
            
            // Step 2: SINGLETON_COMPLETE_REMOVAL_GUIDE.mdに従った最終設定適用
            Debug.Log("[FeatureFlags] Step 2: Applying final FeatureFlags configuration for complete removal...");
            
            // 段階的更新�E�安�E性確保！E            Debug.Log("[FeatureFlags] Step 2.1: Disabling Legacy Singletons...");
            DisableLegacySingletons = true;
            
            Debug.Log("[FeatureFlags] Step 2.2: Disabling Migration Warnings...");
            EnableMigrationWarnings = false;
            
            Debug.Log("[FeatureFlags] Step 2.3: Disabling Migration Monitoring...");
            EnableMigrationMonitoring = false;
            
            PlayerPrefs.Save();
            
            // 更新後状態確誁E            Debug.Log("[FeatureFlags] ✁EPhase 1.2 最終設定完亁E");
            Debug.Log($"  - DisableLegacySingletons: {DisableLegacySingletons}");
            Debug.Log($"  - EnableMigrationWarnings: {EnableMigrationWarnings}");
            Debug.Log($"  - EnableMigrationMonitoring: {EnableMigrationMonitoring}");
            
            Debug.Log("[FeatureFlags] === Phase 1.2 完亁E System ready for Phase 2: Physical Code Removal ===");
        }
        
        /// <summary>
        /// Phase 1.2用: 現在のFeatureFlagsをシリアライズ
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
        /// Phase 1緊急ロールバック: FeatureFlagsを安�Eな状態に戻ぁE        /// </summary>
        public static void ExecutePhase1EmergencyRollback()
        {
            Debug.LogWarning("[FeatureFlags] === EXECUTING PHASE 1 EMERGENCY ROLLBACK ===");
            
            // FeatureFlagsを安�Eな状態に戻ぁE            DisableLegacySingletons = false;
            EnableMigrationWarnings = true;
            EnableMigrationMonitoring = true;
            
            PlayerPrefs.Save();
            
            // 最新バックアチE�Eから復旧惁E��を表示
            string lastBackup = PlayerPrefs.GetString("LastPhase1Backup", "");
            if (!string.IsNullOrEmpty(lastBackup))
            {
                Debug.Log($"[FeatureFlags] Backup available for restore: {lastBackup}");
                string backupData = PlayerPrefs.GetString($"{lastBackup}_FeatureFlags", "");
                Debug.Log($"[FeatureFlags] Backup data: {backupData}");
            }
            
            Debug.Log("[FeatureFlags] ✁EPhase 1 Emergency rollback completed");
            LogCurrentFlags();
        }
        
    public static void LogFlagHistory()
        {
            Debug.Log($"[FeatureFlags] === Flag Change History ===");
            var history = GetFlagChangeHistory();
            
            if (history.Count == 0)
            {
                Debug.Log($"  履歴なぁE);
                return;
            }
            
            foreach (var entry in history)
            {
                Debug.Log($"  {entry}");
            }
        }
    }
}
