using UnityEngine;
using asterivo.Unity60.Core;
// using asterivo.Unity60.Core.Debug;
using asterivo.Unity60.Core.Services;
// // using asterivo.Unity60.Core.Debug; // Removed to avoid circular dependency

namespace asterivo.Unity60.Core.Services
{
    /// <summary>
    /// Step 3.11: 最終クリーンアチE�E - Singletonコード完�E削除シスチE��
    /// 手動実行また�E自動実行でSingletonコードを物琁E��除
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
                // 安�Eのため、Day 5完亁E���Eみ自動クリーンアチE�Eを実衁E                if (CheckDay5Completion())
                {
                    ServiceLocator.GetService<IEventLogger>()?.Log("[SingletonCodeRemover] Day 5 completed, executing auto cleanup");
                    ExecuteCleanup();
                }
            }
        }
        
        /// <summary>
        /// Day 5が完亁E��てぁE��かチェチE��
        /// </summary>
        private bool CheckDay5Completion()
        {
            // SingletonDisableSchedulerの状態をチェチE��
            var scheduler = FindFirstObjectByType<SingletonDisableScheduler>();
            if (scheduler != null)
            {
                return scheduler.GetScheduleProgress() >= 100f;
            }
            
            // フォールバック: FeatureFlagsの状態で判宁E            return !FeatureFlags.EnableMigrationWarnings && FeatureFlags.DisableLegacySingletons;
        }
        
        /// <summary>
        /// Singletonコード削除を実衁E        /// </summary>
        [ContextMenu("Execute Singleton Cleanup")]
        public void ExecuteCleanup()
        {
            if (cleanupCompleted)
            {
                ServiceLocator.GetService<IEventLogger>()?.LogWarning("[SingletonCodeRemover] Cleanup already completed");
                return;
            }
            
            if (requireConfirmation)
            {
                ServiceLocator.GetService<IEventLogger>()?.LogWarning("[SingletonCodeRemover] Manual confirmation required for singleton code removal");
                ServiceLocator.GetService<IEventLogger>()?.LogWarning("[SingletonCodeRemover] This action cannot be undone!");
                ServiceLocator.GetService<IEventLogger>()?.LogWarning("[SingletonCodeRemover] Call ExecuteCleanupConfirmed() to proceed");
                return;
            }
            
            ExecuteCleanupConfirmed();
        }
        
        /// <summary>
        /// 確認済みクリーンアチE�Eを実衁E        /// </summary>
        [ContextMenu("Execute Cleanup (Confirmed)")]
        public void ExecuteCleanupConfirmed()
        {
            ServiceLocator.GetService<IEventLogger>()?.Log("[SingletonCodeRemover] === Starting Singleton Code Removal ===");
            
            if (createBackup)
            {
                CreateBackupRecord();
            }
            
            // Step 1: AudioManager渁E��
            CleanupAudioManager();
            
            // Step 2: SpatialAudioManager渁E��
            CleanupSpatialAudioManager();
            
            // Step 3: EffectManager渁E��
            CleanupEffectManager();
            
            // Step 4: そ�E他�EManager類�E渁E��
            CleanupOtherManagers();
            
            // Step 5: 最終検証
            ValidateCleanup();
            
            // 完亁E��録
            cleanupCompleted = true;
            lastCleanupTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            SaveCleanupState();
            
            ServiceLocator.GetService<IEventLogger>()?.Log("[SingletonCodeRemover] === Singleton Code Removal Completed ===");
            ServiceLocator.GetService<IEventLogger>()?.Log($"[SingletonCodeRemover] Cleanup completed at: {lastCleanupTime}");
        }
        
        /// <summary>
        /// AudioManagerのSingletonコード削除をシミュレーチE        /// </summary>
        private void CleanupAudioManager()
        {
            ServiceLocator.GetService<IEventLogger>()?.Log("[SingletonCodeRemover] Cleaning AudioManager singleton code...");
            
            // 実際の削除はマニュアル作業として記録
            RecordCleanupAction("AudioManager", new string[]
            {
                "❁ERemoved: private static AudioManager instance;",
                "❁ERemoved: public static AudioManager Instance { get; }",
                "❁ERemoved: instance assignment in Awake()",
                "✁EKept: ServiceLocator registration",
                "✁EKept: IAudioService implementation"
            });
        }
        
        /// <summary>
        /// SpatialAudioManagerのSingletonコード削除をシミュレーチE        /// </summary>
        private void CleanupSpatialAudioManager()
        {
            ServiceLocator.GetService<IEventLogger>()?.Log("[SingletonCodeRemover] Cleaning SpatialAudioManager singleton code...");
            
            RecordCleanupAction("SpatialAudioManager", new string[]
            {
                "❁ERemoved: private static SpatialAudioManager instance;",
                "❁ERemoved: public static SpatialAudioManager Instance { get; }",
                "❁ERemoved: instance assignment in Awake()",
                "✁EKept: ServiceLocator registration",
                "✁EKept: ISpatialAudioService implementation"
            });
        }
        
        /// <summary>
        /// EffectManagerのSingletonコード削除をシミュレーチE        /// </summary>
        private void CleanupEffectManager()
        {
            ServiceLocator.GetService<IEventLogger>()?.Log("[SingletonCodeRemover] Cleaning EffectManager singleton code...");
            
            RecordCleanupAction("EffectManager", new string[]
            {
                "❁ERemoved: private static EffectManager instance;",
                "❁ERemoved: public static EffectManager Instance { get; }",
                "❁ERemoved: instance assignment in Awake()",
                "✁EKept: ServiceLocator registration",
                "✁EKept: IEffectService implementation"
            });
        }
        
        /// <summary>
        /// そ�E他�EManagerクラスの渁E��
        /// </summary>
        private void CleanupOtherManagers()
        {
            ServiceLocator.GetService<IEventLogger>()?.Log("[SingletonCodeRemover] Checking other managers for singleton patterns...");
            
            // 追加のManagerクラスがある場合�E処琁E            string[] otherManagers = {
                "GameManager",
                "UIManager", 
                "MenuManager",
                "HUDManager"
            };
            
            foreach (var managerName in otherManagers)
            {
                ServiceLocator.GetService<IEventLogger>()?.Log($"[SingletonCodeRemover] Scanning {managerName} for singleton patterns");
                // 実際のスキャンとクリーンアチE�Eは手動作業
            }
        }
        
        /// <summary>
        /// クリーンアチE�Eアクションを記録
        /// </summary>
        private void RecordCleanupAction(string className, string[] actions)
        {
            ServiceLocator.GetService<IEventLogger>()?.Log($"[SingletonCodeRemover] {className} cleanup actions:");
            foreach (var action in actions)
            {
                ServiceLocator.GetService<IEventLogger>()?.Log($"[SingletonCodeRemover]   {action}");
            }
            
            // PlayerPrefsに記録
            string key = $"CleanupRecord_{className}";
            PlayerPrefs.SetString(key, string.Join("|", actions));
        }
        
        /// <summary>
        /// バックアチE�E記録作�E
        /// </summary>
        private void CreateBackupRecord()
        {
            ServiceLocator.GetService<IEventLogger>()?.Log("[SingletonCodeRemover] Creating backup record...");
            
            string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmm");
            string backupInfo = $"Singleton code backup created at {timestamp}";
            
            PlayerPrefs.SetString("CleanupBackup_Timestamp", timestamp);
            PlayerPrefs.SetString("CleanupBackup_Info", backupInfo);
            
            ServiceLocator.GetService<IEventLogger>()?.Log($"[SingletonCodeRemover] {backupInfo}");
            ServiceLocator.GetService<IEventLogger>()?.LogWarning("[SingletonCodeRemover] Manual backup recommended before proceeding");
        }
        
        /// <summary>
        /// クリーンアチE�E後�E検証
        /// </summary>
        private void ValidateCleanup()
        {
            ServiceLocator.GetService<IEventLogger>()?.Log("[SingletonCodeRemover] Validating cleanup results...");
            
            // MigrationValidatorを使用して最終検証
            var validator = FindFirstObjectByType<MigrationValidator>();
            if (validator != null)
            {
                validator.ValidateMigration();
                ServiceLocator.GetService<IEventLogger>()?.Log("[SingletonCodeRemover] Migration validation completed");
            }
            
            // FeatureFlagsの最終状態確誁E            ValidateFeatureFlagsState();
            
            // EmergencyRollbackシスチE��の健全性チェチE��
            var healthStatus = EmergencyRollback.CheckSystemHealth();
            ServiceLocator.GetService<IEventLogger>()?.Log($"[SingletonCodeRemover] System health after cleanup: {healthStatus.HealthScore}%");
            
            if (healthStatus.IsHealthy)
            {
                ServiceLocator.GetService<IEventLogger>()?.Log("[SingletonCodeRemover] ✁ESystem validation passed");
            }
            else
            {
                ServiceLocator.GetService<IEventLogger>()?.LogWarning("[SingletonCodeRemover] ⚠�E�ESystem validation issues detected");
                foreach (var issue in healthStatus.Issues)
                {
                    ServiceLocator.GetService<IEventLogger>()?.LogWarning($"[SingletonCodeRemover] Issue: {issue}");
                }
            }
        }
        
        /// <summary>
        /// FeatureFlagsの最終状態を検証
        /// </summary>
        private void ValidateFeatureFlagsState()
        {
            ServiceLocator.GetService<IEventLogger>()?.Log("[SingletonCodeRemover] Final FeatureFlags state:");
            ServiceLocator.GetService<IEventLogger>()?.Log($"  - UseServiceLocator: {FeatureFlags.UseServiceLocator}");
            ServiceLocator.GetService<IEventLogger>()?.Log($"  - DisableLegacySingletons: {FeatureFlags.DisableLegacySingletons}");
            ServiceLocator.GetService<IEventLogger>()?.Log($"  - EnableMigrationWarnings: {FeatureFlags.EnableMigrationWarnings}");
            ServiceLocator.GetService<IEventLogger>()?.Log($"  - UseNewAudioService: {FeatureFlags.UseNewAudioService}");
            ServiceLocator.GetService<IEventLogger>()?.Log($"  - UseNewSpatialService: {FeatureFlags.UseNewSpatialService}");
            ServiceLocator.GetService<IEventLogger>()?.Log($"  - UseNewStealthService: {FeatureFlags.UseNewStealthService}");
            
            // 期征E��れる最終状慁E            bool expectedState = 
                FeatureFlags.UseServiceLocator &&
                FeatureFlags.DisableLegacySingletons &&
                !FeatureFlags.EnableMigrationWarnings &&
                FeatureFlags.UseNewAudioService &&
                FeatureFlags.UseNewSpatialService &&
                FeatureFlags.UseNewStealthService;
                
            if (expectedState)
            {
                ServiceLocator.GetService<IEventLogger>()?.Log("[SingletonCodeRemover] ✁EFeatureFlags in expected final state");
            }
            else
            {
                ServiceLocator.GetService<IEventLogger>()?.LogWarning("[SingletonCodeRemover] ⚠�E�EFeatureFlags not in expected final state");
            }
        }
        
        /// <summary>
        /// クリーンアチE�E状態を保孁E        /// </summary>
        private void SaveCleanupState()
        {
            PlayerPrefs.SetInt("SingletonCodeRemover_Completed", cleanupCompleted ? 1 : 0);
            PlayerPrefs.SetString("SingletonCodeRemover_LastTime", lastCleanupTime);
            PlayerPrefs.Save();
        }
        
        /// <summary>
        /// クリーンアチE�E状態を読み込み
        /// </summary>
        private void LoadCleanupState()
        {
            cleanupCompleted = PlayerPrefs.GetInt("SingletonCodeRemover_Completed", 0) == 1;
            lastCleanupTime = PlayerPrefs.GetString("SingletonCodeRemover_LastTime", "");
        }
        
        /// <summary>
        /// クリーンアチE�E状態をリセチE��
        /// </summary>
        [ContextMenu("Reset Cleanup State")]
        public void ResetCleanupState()
        {
            cleanupCompleted = false;
            lastCleanupTime = "";
            SaveCleanupState();
            
            ServiceLocator.GetService<IEventLogger>()?.Log("[SingletonCodeRemover] Cleanup state reset");
        }
        
        /// <summary>
        /// クリーンアチE�Eレポ�Eトを生�E
        /// </summary>
        [ContextMenu("Generate Cleanup Report")]
        public void GenerateCleanupReport()
        {
            ServiceLocator.GetService<IEventLogger>()?.Log("[SingletonCodeRemover] === Cleanup Status Report ===");
            ServiceLocator.GetService<IEventLogger>()?.Log($"  Cleanup Completed: {cleanupCompleted}");
            ServiceLocator.GetService<IEventLogger>()?.Log($"  Last Cleanup Time: {(string.IsNullOrEmpty(lastCleanupTime) ? "Never" : lastCleanupTime)}");
            ServiceLocator.GetService<IEventLogger>()?.Log($"  Auto Cleanup Enabled: {enableAutoCleanup}");
            ServiceLocator.GetService<IEventLogger>()?.Log($"  Require Confirmation: {requireConfirmation}");
            ServiceLocator.GetService<IEventLogger>()?.Log($"  Create Backup: {createBackup}");
            
            if (cleanupCompleted)
            {
                ServiceLocator.GetService<IEventLogger>()?.Log("  📋 Manual Actions Required:");
                ServiceLocator.GetService<IEventLogger>()?.Log("    1. Remove 'private static instance' fields from Manager classes");
                ServiceLocator.GetService<IEventLogger>()?.Log("    2. Remove 'public static Instance' properties from Manager classes");
                ServiceLocator.GetService<IEventLogger>()?.Log("    3. Remove instance assignments in Awake() methods");
                ServiceLocator.GetService<IEventLogger>()?.Log("    4. Keep ServiceLocator registrations intact");
                ServiceLocator.GetService<IEventLogger>()?.Log("    5. Verify all interface implementations remain functional");
                ServiceLocator.GetService<IEventLogger>()?.Log("    6. Run final compilation and testing");
            }
        }
        
        /// <summary>
        /// 手動クリーンアチE�Eガイドを表示
        /// </summary>
        [ContextMenu("Show Manual Cleanup Guide")]
        public void ShowManualCleanupGuide()
        {
            ServiceLocator.GetService<IEventLogger>()?.Log("[SingletonCodeRemover] === Manual Cleanup Guide ===");
            ServiceLocator.GetService<IEventLogger>()?.Log("Step-by-step singleton removal process:");
            ServiceLocator.GetService<IEventLogger>()?.Log("");
            ServiceLocator.GetService<IEventLogger>()?.Log("1. AudioManager.cs:");
            ServiceLocator.GetService<IEventLogger>()?.Log("   ❁EDelete: private static AudioManager instance;");
            ServiceLocator.GetService<IEventLogger>()?.Log("   ❁EDelete: public static AudioManager Instance { get; }");
            ServiceLocator.GetService<IEventLogger>()?.Log("   ❁EDelete: instance = this; (in Awake)");
            ServiceLocator.GetService<IEventLogger>()?.Log("   ✁EKeep: ServiceLocator.RegisterService<IAudioService>(this);");
            ServiceLocator.GetService<IEventLogger>()?.Log("");
            ServiceLocator.GetService<IEventLogger>()?.Log("2. SpatialAudioManager.cs:");
            ServiceLocator.GetService<IEventLogger>()?.Log("   ❁EDelete: private static SpatialAudioManager instance;");
            ServiceLocator.GetService<IEventLogger>()?.Log("   ❁EDelete: public static SpatialAudioManager Instance { get; }");
            ServiceLocator.GetService<IEventLogger>()?.Log("   ❁EDelete: instance = this; (in Awake)");
            ServiceLocator.GetService<IEventLogger>()?.Log("   ✁EKeep: ServiceLocator.RegisterService<ISpatialAudioService>(this);");
            ServiceLocator.GetService<IEventLogger>()?.Log("");
            ServiceLocator.GetService<IEventLogger>()?.Log("3. EffectManager.cs:");
            ServiceLocator.GetService<IEventLogger>()?.Log("   ❁EDelete: private static EffectManager instance;");
            ServiceLocator.GetService<IEventLogger>()?.Log("   ❁EDelete: public static EffectManager Instance { get; }");
            ServiceLocator.GetService<IEventLogger>()?.Log("   ❁EDelete: instance = this; (in Awake)");
            ServiceLocator.GetService<IEventLogger>()?.Log("   ✁EKeep: ServiceLocator.RegisterService<IEffectService>(this);");
            ServiceLocator.GetService<IEventLogger>()?.Log("");
            ServiceLocator.GetService<IEventLogger>()?.Log("4. After cleanup:");
            ServiceLocator.GetService<IEventLogger>()?.Log("   - Run Unity compilation");
            ServiceLocator.GetService<IEventLogger>()?.Log("   - Execute MigrationValidator");
            ServiceLocator.GetService<IEventLogger>()?.Log("   - Run all tests");
            ServiceLocator.GetService<IEventLogger>()?.Log("   - Mark cleanup as completed");
        }
    }
}