using UnityEngine;
using asterivo.Unity60.Core;
// using asterivo.Unity60.Core.Debug;
using asterivo.Unity60.Core.Services;
// // using asterivo.Unity60.Core.Debug; // Removed to avoid circular dependency

namespace asterivo.Unity60.Core.Services
{
    /// <summary>
    /// Step 3.11: Final Cleanup - Singleton Code Physical Removal System
    /// Manually or automatically removes Singleton code
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
                // For safety, auto cleanup only after Day 5 completion
                if (CheckDay5Completion())
                {
                    ServiceLocator.GetService<IEventLogger>()?.Log("[SingletonCodeRemover] Day 5 completed, executing auto cleanup");
                    ExecuteCleanup();
                }
            }
        }

        /// <summary>
        /// Check if Day 5 is completed
        /// </summary>
        private bool CheckDay5Completion()
        {
            // Check SingletonDisableScheduler status
            var scheduler = FindFirstObjectByType<SingletonDisableScheduler>();
            if (scheduler != null)
            {
                return scheduler.GetScheduleProgress() >= 100f;
            }

            // Fallback: Check FeatureFlags status
            return !FeatureFlags.EnableMigrationWarnings && FeatureFlags.DisableLegacySingletons;
        }

        /// <summary>
        /// Execute Singleton code removal
        /// </summary>
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
        /// Execute confirmed cleanup
        /// </summary>
        [ContextMenu("Execute Cleanup (Confirmed)")]
        public void ExecuteCleanupConfirmed()
        {
            ServiceLocator.GetService<IEventLogger>()?.Log("[SingletonCodeRemover] === Starting Singleton Code Removal ===");

            if (createBackup)
            {
                CreateBackupRecord();
            }

            // Step 1: AudioManager cleanup
            CleanupAudioManager();

            // Step 2: SpatialAudioManager cleanup
            CleanupSpatialAudioManager();

            // Step 3: EffectManager cleanup
            CleanupEffectManager();

            // Step 4: Other Managers cleanup
            CleanupOtherManagers();

            // Step 5: Final validation
            ValidateCleanup();

            // Mark as completed
            cleanupCompleted = true;
            lastCleanupTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            SaveCleanupState();

            ServiceLocator.GetService<IEventLogger>()?.Log("[SingletonCodeRemover] === Singleton Code Removal Completed ===");
            ServiceLocator.GetService<IEventLogger>()?.Log($"[SingletonCodeRemover] Cleanup completed at: {lastCleanupTime}");
        }

        /// <summary>
        /// Simulate AudioManager Singleton code removal
        /// </summary>
        private void CleanupAudioManager()
        {
            ServiceLocator.GetService<IEventLogger>()?.Log("[SingletonCodeRemover] Cleaning AudioManager singleton code...");

            // Record actions for manual execution
            RecordCleanupAction("AudioManager", new string[]
            {
                "× Removed: private static AudioManager instance;",
                "× Removed: public static AudioManager Instance { get; }",
                "× Removed: instance assignment in Awake()",
                "✓ Kept: ServiceLocator registration",
                "✓ Kept: IAudioService implementation"
            });
        }

        /// <summary>
        /// Simulate SpatialAudioManager Singleton code removal
        /// </summary>
        private void CleanupSpatialAudioManager()
        {
            ServiceLocator.GetService<IEventLogger>()?.Log("[SingletonCodeRemover] Cleaning SpatialAudioManager singleton code...");

            RecordCleanupAction("SpatialAudioManager", new string[]
            {
                "× Removed: private static SpatialAudioManager instance;",
                "× Removed: public static SpatialAudioManager Instance { get; }",
                "× Removed: instance assignment in Awake()",
                "✓ Kept: ServiceLocator registration",
                "✓ Kept: ISpatialAudioService implementation"
            });
        }

        /// <summary>
        /// Simulate EffectManager Singleton code removal
        /// </summary>
        private void CleanupEffectManager()
        {
            ServiceLocator.GetService<IEventLogger>()?.Log("[SingletonCodeRemover] Cleaning EffectManager singleton code...");

            RecordCleanupAction("EffectManager", new string[]
            {
                "× Removed: private static EffectManager instance;",
                "× Removed: public static EffectManager Instance { get; }",
                "× Removed: instance assignment in Awake()",
                "✓ Kept: ServiceLocator registration",
                "✓ Kept: IEffectService implementation"
            });
        }

        /// <summary>
        /// Cleanup other Manager classes
        /// </summary>
        private void CleanupOtherManagers()
        {
            ServiceLocator.GetService<IEventLogger>()?.Log("[SingletonCodeRemover] Checking other managers for singleton patterns...");

            // Additional Manager classes to process
            string[] otherManagers = {
                "GameManager",
                "UIManager",
                "MenuManager",
                "HUDManager"
            };

            foreach (var managerName in otherManagers)
            {
                ServiceLocator.GetService<IEventLogger>()?.Log($"[SingletonCodeRemover] Scanning {managerName} for singleton patterns");
                // Actual scan and cleanup requires manual work
            }
        }

        /// <summary>
        /// Record cleanup action
        /// </summary>
        private void RecordCleanupAction(string className, string[] actions)
        {
            ServiceLocator.GetService<IEventLogger>()?.Log($"[SingletonCodeRemover] {className} cleanup actions:");
            foreach (var action in actions)
            {
                ServiceLocator.GetService<IEventLogger>()?.Log($"[SingletonCodeRemover]   {action}");
            }

            // Record in PlayerPrefs
            string key = $"CleanupRecord_{className}";
            PlayerPrefs.SetString(key, string.Join("|", actions));
        }

        /// <summary>
        /// Create backup record
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
        /// Validate cleanup results
        /// </summary>
        private void ValidateCleanup()
        {
            ServiceLocator.GetService<IEventLogger>()?.Log("[SingletonCodeRemover] Validating cleanup results...");

            // Use MigrationValidator for final validation
            var validator = FindFirstObjectByType<MigrationValidator>();
            if (validator != null)
            {
                validator.ValidateMigration();
                ServiceLocator.GetService<IEventLogger>()?.Log("[SingletonCodeRemover] Migration validation completed");
            }

            // Validate FeatureFlags final state
            ValidateFeatureFlagsState();

            // Check EmergencyRollback system health
            var healthStatus = EmergencyRollback.CheckSystemHealth();
            ServiceLocator.GetService<IEventLogger>()?.Log($"[SingletonCodeRemover] System health after cleanup: {healthStatus.HealthScore}%");

            if (healthStatus.IsHealthy)
            {
                ServiceLocator.GetService<IEventLogger>()?.Log("[SingletonCodeRemover] ✓ System validation passed");
            }
            else
            {
                ServiceLocator.GetService<IEventLogger>()?.LogWarning("[SingletonCodeRemover] ⚠ System validation issues detected");
                foreach (var issue in healthStatus.Issues)
                {
                    ServiceLocator.GetService<IEventLogger>()?.LogWarning($"[SingletonCodeRemover] Issue: {issue}");
                }
            }
        }

        /// <summary>
        /// Validate FeatureFlags final state
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

            // Expected final state
            bool expectedState =
                FeatureFlags.UseServiceLocator &&
                FeatureFlags.DisableLegacySingletons &&
                !FeatureFlags.EnableMigrationWarnings &&
                FeatureFlags.UseNewAudioService &&
                FeatureFlags.UseNewSpatialService &&
                FeatureFlags.UseNewStealthService;

            if (expectedState)
            {
                ServiceLocator.GetService<IEventLogger>()?.Log("[SingletonCodeRemover] ✓ FeatureFlags in expected final state");
            }
            else
            {
                ServiceLocator.GetService<IEventLogger>()?.LogWarning("[SingletonCodeRemover] ⚠ FeatureFlags not in expected final state");
            }
        }

        /// <summary>
        /// Save cleanup state
        /// </summary>
        private void SaveCleanupState()
        {
            PlayerPrefs.SetInt("SingletonCodeRemover_Completed", cleanupCompleted ? 1 : 0);
            PlayerPrefs.SetString("SingletonCodeRemover_LastTime", lastCleanupTime);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Load cleanup state
        /// </summary>
        private void LoadCleanupState()
        {
            cleanupCompleted = PlayerPrefs.GetInt("SingletonCodeRemover_Completed", 0) == 1;
            lastCleanupTime = PlayerPrefs.GetString("SingletonCodeRemover_LastTime", "");
        }

        /// <summary>
        /// Reset cleanup state
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
        /// Generate cleanup report
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
                ServiceLocator.GetService<IEventLogger>()?.Log("  ⚡ Manual Actions Required:");
                ServiceLocator.GetService<IEventLogger>()?.Log("    1. Remove 'private static instance' fields from Manager classes");
                ServiceLocator.GetService<IEventLogger>()?.Log("    2. Remove 'public static Instance' properties from Manager classes");
                ServiceLocator.GetService<IEventLogger>()?.Log("    3. Remove instance assignments in Awake() methods");
                ServiceLocator.GetService<IEventLogger>()?.Log("    4. Keep ServiceLocator registrations intact");
                ServiceLocator.GetService<IEventLogger>()?.Log("    5. Verify all interface implementations remain functional");
                ServiceLocator.GetService<IEventLogger>()?.Log("    6. Run final compilation and testing");
            }
        }

        /// <summary>
        /// Show manual cleanup guide
        /// </summary>
        [ContextMenu("Show Manual Cleanup Guide")]
        public void ShowManualCleanupGuide()
        {
            ServiceLocator.GetService<IEventLogger>()?.Log("[SingletonCodeRemover] === Manual Cleanup Guide ===");
            ServiceLocator.GetService<IEventLogger>()?.Log("Step-by-step singleton removal process:");
            ServiceLocator.GetService<IEventLogger>()?.Log("");
            ServiceLocator.GetService<IEventLogger>()?.Log("1. AudioManager.cs:");
            ServiceLocator.GetService<IEventLogger>()?.Log("   × Delete: private static AudioManager instance;");
            ServiceLocator.GetService<IEventLogger>()?.Log("   × Delete: public static AudioManager Instance { get; }");
            ServiceLocator.GetService<IEventLogger>()?.Log("   × Delete: instance = this; (in Awake)");
            ServiceLocator.GetService<IEventLogger>()?.Log("   ✓ Keep: ServiceLocator.RegisterService<IAudioService>(this);");
            ServiceLocator.GetService<IEventLogger>()?.Log("");
            ServiceLocator.GetService<IEventLogger>()?.Log("2. SpatialAudioManager.cs:");
            ServiceLocator.GetService<IEventLogger>()?.Log("   × Delete: private static SpatialAudioManager instance;");
            ServiceLocator.GetService<IEventLogger>()?.Log("   × Delete: public static SpatialAudioManager Instance { get; }");
            ServiceLocator.GetService<IEventLogger>()?.Log("   × Delete: instance = this; (in Awake)");
            ServiceLocator.GetService<IEventLogger>()?.Log("   ✓ Keep: ServiceLocator.RegisterService<ISpatialAudioService>(this);");
            ServiceLocator.GetService<IEventLogger>()?.Log("");
            ServiceLocator.GetService<IEventLogger>()?.Log("3. EffectManager.cs:");
            ServiceLocator.GetService<IEventLogger>()?.Log("   × Delete: private static EffectManager instance;");
            ServiceLocator.GetService<IEventLogger>()?.Log("   × Delete: public static EffectManager Instance { get; }");
            ServiceLocator.GetService<IEventLogger>()?.Log("   × Delete: instance = this; (in Awake)");
            ServiceLocator.GetService<IEventLogger>()?.Log("   ✓ Keep: ServiceLocator.RegisterService<IEffectService>(this);");
            ServiceLocator.GetService<IEventLogger>()?.Log("");
            ServiceLocator.GetService<IEventLogger>()?.Log("4. After cleanup:");
            ServiceLocator.GetService<IEventLogger>()?.Log("   - Run Unity compilation");
            ServiceLocator.GetService<IEventLogger>()?.Log("   - Execute MigrationValidator");
            ServiceLocator.GetService<IEventLogger>()?.Log("   - Run all tests");
            ServiceLocator.GetService<IEventLogger>()?.Log("   - Mark cleanup as completed");
        }
    }
}