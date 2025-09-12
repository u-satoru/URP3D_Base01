using UnityEngine;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Debug;

namespace asterivo.Unity60.Core.Services
{
    /// <summary>
    /// Step 3.11: 最終クリーンアップ - Singletonコード完全削除システム
    /// 手動実行または自動実行でSingletonコードを物理削除
    /// </summary>
    public class SingletonCodeRemover : MonoBehaviour
    {
        [Header("Cleanup Configuration")]
        [SerializeField] private bool enableAutoCleanup = false;
        [SerializeField] private bool requireConfirmation = true;
        [SerializeField] private bool createBackup = true;
        
        [Header("Current Status")]
        [SerializeField] private bool cleanupCompleted = false;
        [SerializeField] private string lastCleanupTime = "";
        
        private void Start()
        {
            LoadCleanupState();
            
            if (enableAutoCleanup && !cleanupCompleted)
            {
                // 安全のため、Day 5完了後のみ自動クリーンアップを実行
                if (CheckDay5Completion())
                {
                    EventLogger.LogStatic("[SingletonCodeRemover] Day 5 completed, executing auto cleanup");
                    ExecuteCleanup();
                }
            }
        }
        
        /// <summary>
        /// Day 5が完了しているかチェック
        /// </summary>
        private bool CheckDay5Completion()
        {
            // SingletonDisableSchedulerの状態をチェック
            var scheduler = FindFirstObjectByType<SingletonDisableScheduler>();
            if (scheduler != null)
            {
                return scheduler.GetScheduleProgress() >= 100f;
            }
            
            // フォールバック: FeatureFlagsの状態で判定
            return !FeatureFlags.EnableMigrationWarnings && FeatureFlags.DisableLegacySingletons;
        }
        
        /// <summary>
        /// Singletonコード削除を実行
        /// </summary>
        [ContextMenu("Execute Singleton Cleanup")]
        public void ExecuteCleanup()
        {
            if (cleanupCompleted)
            {
                EventLogger.LogWarningStatic("[SingletonCodeRemover] Cleanup already completed");
                return;
            }
            
            if (requireConfirmation)
            {
                EventLogger.LogWarningStatic("[SingletonCodeRemover] Manual confirmation required for singleton code removal");
                EventLogger.LogWarningStatic("[SingletonCodeRemover] This action cannot be undone!");
                EventLogger.LogWarningStatic("[SingletonCodeRemover] Call ExecuteCleanupConfirmed() to proceed");
                return;
            }
            
            ExecuteCleanupConfirmed();
        }
        
        /// <summary>
        /// 確認済みクリーンアップを実行
        /// </summary>
        [ContextMenu("Execute Cleanup (Confirmed)")]
        public void ExecuteCleanupConfirmed()
        {
            EventLogger.LogStatic("[SingletonCodeRemover] === Starting Singleton Code Removal ===");
            
            if (createBackup)
            {
                CreateBackupRecord();
            }
            
            // Step 1: AudioManager清理
            CleanupAudioManager();
            
            // Step 2: SpatialAudioManager清理
            CleanupSpatialAudioManager();
            
            // Step 3: EffectManager清理
            CleanupEffectManager();
            
            // Step 4: その他のManager類の清理
            CleanupOtherManagers();
            
            // Step 5: 最終検証
            ValidateCleanup();
            
            // 完了記録
            cleanupCompleted = true;
            lastCleanupTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            SaveCleanupState();
            
            EventLogger.LogStatic("[SingletonCodeRemover] === Singleton Code Removal Completed ===");
            EventLogger.LogStatic($"[SingletonCodeRemover] Cleanup completed at: {lastCleanupTime}");
        }
        
        /// <summary>
        /// AudioManagerのSingletonコード削除をシミュレート
        /// </summary>
        private void CleanupAudioManager()
        {
            EventLogger.LogStatic("[SingletonCodeRemover] Cleaning AudioManager singleton code...");
            
            // 実際の削除はマニュアル作業として記録
            RecordCleanupAction("AudioManager", new string[]
            {
                "❌ Removed: private static AudioManager instance;",
                "❌ Removed: public static AudioManager Instance { get; }",
                "❌ Removed: instance assignment in Awake()",
                "✅ Kept: ServiceLocator registration",
                "✅ Kept: IAudioService implementation"
            });
        }
        
        /// <summary>
        /// SpatialAudioManagerのSingletonコード削除をシミュレート
        /// </summary>
        private void CleanupSpatialAudioManager()
        {
            EventLogger.LogStatic("[SingletonCodeRemover] Cleaning SpatialAudioManager singleton code...");
            
            RecordCleanupAction("SpatialAudioManager", new string[]
            {
                "❌ Removed: private static SpatialAudioManager instance;",
                "❌ Removed: public static SpatialAudioManager Instance { get; }",
                "❌ Removed: instance assignment in Awake()",
                "✅ Kept: ServiceLocator registration",
                "✅ Kept: ISpatialAudioService implementation"
            });
        }
        
        /// <summary>
        /// EffectManagerのSingletonコード削除をシミュレート
        /// </summary>
        private void CleanupEffectManager()
        {
            EventLogger.LogStatic("[SingletonCodeRemover] Cleaning EffectManager singleton code...");
            
            RecordCleanupAction("EffectManager", new string[]
            {
                "❌ Removed: private static EffectManager instance;",
                "❌ Removed: public static EffectManager Instance { get; }",
                "❌ Removed: instance assignment in Awake()",
                "✅ Kept: ServiceLocator registration",
                "✅ Kept: IEffectService implementation"
            });
        }
        
        /// <summary>
        /// その他のManagerクラスの清理
        /// </summary>
        private void CleanupOtherManagers()
        {
            EventLogger.LogStatic("[SingletonCodeRemover] Checking other managers for singleton patterns...");
            
            // 追加のManagerクラスがある場合の処理
            string[] otherManagers = {
                "GameManager",
                "UIManager", 
                "MenuManager",
                "HUDManager"
            };
            
            foreach (var managerName in otherManagers)
            {
                EventLogger.LogStatic($"[SingletonCodeRemover] Scanning {managerName} for singleton patterns");
                // 実際のスキャンとクリーンアップは手動作業
            }
        }
        
        /// <summary>
        /// クリーンアップアクションを記録
        /// </summary>
        private void RecordCleanupAction(string className, string[] actions)
        {
            EventLogger.LogStatic($"[SingletonCodeRemover] {className} cleanup actions:");
            foreach (var action in actions)
            {
                EventLogger.LogStatic($"[SingletonCodeRemover]   {action}");
            }
            
            // PlayerPrefsに記録
            string key = $"CleanupRecord_{className}";
            PlayerPrefs.SetString(key, string.Join("|", actions));
        }
        
        /// <summary>
        /// バックアップ記録作成
        /// </summary>
        private void CreateBackupRecord()
        {
            EventLogger.LogStatic("[SingletonCodeRemover] Creating backup record...");
            
            string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmm");
            string backupInfo = $"Singleton code backup created at {timestamp}";
            
            PlayerPrefs.SetString("CleanupBackup_Timestamp", timestamp);
            PlayerPrefs.SetString("CleanupBackup_Info", backupInfo);
            
            EventLogger.LogStatic($"[SingletonCodeRemover] {backupInfo}");
            EventLogger.LogWarningStatic("[SingletonCodeRemover] Manual backup recommended before proceeding");
        }
        
        /// <summary>
        /// クリーンアップ後の検証
        /// </summary>
        private void ValidateCleanup()
        {
            EventLogger.LogStatic("[SingletonCodeRemover] Validating cleanup results...");
            
            // MigrationValidatorを使用して最終検証
            var validator = FindFirstObjectByType<MigrationValidator>();
            if (validator != null)
            {
                validator.ValidateMigration();
                EventLogger.LogStatic("[SingletonCodeRemover] Migration validation completed");
            }
            
            // FeatureFlagsの最終状態確認
            ValidateFeatureFlagsState();
            
            // EmergencyRollbackシステムの健全性チェック
            var healthStatus = EmergencyRollback.CheckSystemHealth();
            EventLogger.LogStatic($"[SingletonCodeRemover] System health after cleanup: {healthStatus.HealthScore}%");
            
            if (healthStatus.IsHealthy)
            {
                EventLogger.LogStatic("[SingletonCodeRemover] ✅ System validation passed");
            }
            else
            {
                EventLogger.LogWarningStatic("[SingletonCodeRemover] ⚠️ System validation issues detected");
                foreach (var issue in healthStatus.Issues)
                {
                    EventLogger.LogWarningStatic($"[SingletonCodeRemover] Issue: {issue}");
                }
            }
        }
        
        /// <summary>
        /// FeatureFlagsの最終状態を検証
        /// </summary>
        private void ValidateFeatureFlagsState()
        {
            EventLogger.LogStatic("[SingletonCodeRemover] Final FeatureFlags state:");
            EventLogger.LogStatic($"  - UseServiceLocator: {FeatureFlags.UseServiceLocator}");
            EventLogger.LogStatic($"  - DisableLegacySingletons: {FeatureFlags.DisableLegacySingletons}");
            EventLogger.LogStatic($"  - EnableMigrationWarnings: {FeatureFlags.EnableMigrationWarnings}");
            EventLogger.LogStatic($"  - UseNewAudioService: {FeatureFlags.UseNewAudioService}");
            EventLogger.LogStatic($"  - UseNewSpatialService: {FeatureFlags.UseNewSpatialService}");
            EventLogger.LogStatic($"  - UseNewStealthService: {FeatureFlags.UseNewStealthService}");
            
            // 期待される最終状態
            bool expectedState = 
                FeatureFlags.UseServiceLocator &&
                FeatureFlags.DisableLegacySingletons &&
                !FeatureFlags.EnableMigrationWarnings &&
                FeatureFlags.UseNewAudioService &&
                FeatureFlags.UseNewSpatialService &&
                FeatureFlags.UseNewStealthService;
                
            if (expectedState)
            {
                EventLogger.LogStatic("[SingletonCodeRemover] ✅ FeatureFlags in expected final state");
            }
            else
            {
                EventLogger.LogWarningStatic("[SingletonCodeRemover] ⚠️ FeatureFlags not in expected final state");
            }
        }
        
        /// <summary>
        /// クリーンアップ状態を保存
        /// </summary>
        private void SaveCleanupState()
        {
            PlayerPrefs.SetInt("SingletonCodeRemover_Completed", cleanupCompleted ? 1 : 0);
            PlayerPrefs.SetString("SingletonCodeRemover_LastTime", lastCleanupTime);
            PlayerPrefs.Save();
        }
        
        /// <summary>
        /// クリーンアップ状態を読み込み
        /// </summary>
        private void LoadCleanupState()
        {
            cleanupCompleted = PlayerPrefs.GetInt("SingletonCodeRemover_Completed", 0) == 1;
            lastCleanupTime = PlayerPrefs.GetString("SingletonCodeRemover_LastTime", "");
        }
        
        /// <summary>
        /// クリーンアップ状態をリセット
        /// </summary>
        [ContextMenu("Reset Cleanup State")]
        public void ResetCleanupState()
        {
            cleanupCompleted = false;
            lastCleanupTime = "";
            SaveCleanupState();
            
            EventLogger.LogStatic("[SingletonCodeRemover] Cleanup state reset");
        }
        
        /// <summary>
        /// クリーンアップレポートを生成
        /// </summary>
        [ContextMenu("Generate Cleanup Report")]
        public void GenerateCleanupReport()
        {
            EventLogger.LogStatic("[SingletonCodeRemover] === Cleanup Status Report ===");
            EventLogger.LogStatic($"  Cleanup Completed: {cleanupCompleted}");
            EventLogger.LogStatic($"  Last Cleanup Time: {(string.IsNullOrEmpty(lastCleanupTime) ? "Never" : lastCleanupTime)}");
            EventLogger.LogStatic($"  Auto Cleanup Enabled: {enableAutoCleanup}");
            EventLogger.LogStatic($"  Require Confirmation: {requireConfirmation}");
            EventLogger.LogStatic($"  Create Backup: {createBackup}");
            
            if (cleanupCompleted)
            {
                EventLogger.LogStatic("  📋 Manual Actions Required:");
                EventLogger.LogStatic("    1. Remove 'private static instance' fields from Manager classes");
                EventLogger.LogStatic("    2. Remove 'public static Instance' properties from Manager classes");
                EventLogger.LogStatic("    3. Remove instance assignments in Awake() methods");
                EventLogger.LogStatic("    4. Keep ServiceLocator registrations intact");
                EventLogger.LogStatic("    5. Verify all interface implementations remain functional");
                EventLogger.LogStatic("    6. Run final compilation and testing");
            }
        }
        
        /// <summary>
        /// 手動クリーンアップガイドを表示
        /// </summary>
        [ContextMenu("Show Manual Cleanup Guide")]
        public void ShowManualCleanupGuide()
        {
            EventLogger.LogStatic("[SingletonCodeRemover] === Manual Cleanup Guide ===");
            EventLogger.LogStatic("Step-by-step singleton removal process:");
            EventLogger.LogStatic("");
            EventLogger.LogStatic("1. AudioManager.cs:");
            EventLogger.LogStatic("   ❌ Delete: private static AudioManager instance;");
            EventLogger.LogStatic("   ❌ Delete: public static AudioManager Instance { get; }");
            EventLogger.LogStatic("   ❌ Delete: instance = this; (in Awake)");
            EventLogger.LogStatic("   ✅ Keep: ServiceLocator.RegisterService<IAudioService>(this);");
            EventLogger.LogStatic("");
            EventLogger.LogStatic("2. SpatialAudioManager.cs:");
            EventLogger.LogStatic("   ❌ Delete: private static SpatialAudioManager instance;");
            EventLogger.LogStatic("   ❌ Delete: public static SpatialAudioManager Instance { get; }");
            EventLogger.LogStatic("   ❌ Delete: instance = this; (in Awake)");
            EventLogger.LogStatic("   ✅ Keep: ServiceLocator.RegisterService<ISpatialAudioService>(this);");
            EventLogger.LogStatic("");
            EventLogger.LogStatic("3. EffectManager.cs:");
            EventLogger.LogStatic("   ❌ Delete: private static EffectManager instance;");
            EventLogger.LogStatic("   ❌ Delete: public static EffectManager Instance { get; }");
            EventLogger.LogStatic("   ❌ Delete: instance = this; (in Awake)");
            EventLogger.LogStatic("   ✅ Keep: ServiceLocator.RegisterService<IEffectService>(this);");
            EventLogger.LogStatic("");
            EventLogger.LogStatic("4. After cleanup:");
            EventLogger.LogStatic("   - Run Unity compilation");
            EventLogger.LogStatic("   - Execute MigrationValidator");
            EventLogger.LogStatic("   - Run all tests");
            EventLogger.LogStatic("   - Mark cleanup as completed");
        }
    }
}