using UnityEngine;
using System;
using System.Collections.Generic;
// // using asterivo.Unity60.Core.Debug; // Removed to avoid circular dependency

namespace asterivo.Unity60.Core
{
    /// <summary>
    /// Phase 3 Enhanced Feature Flag System
    /// Manages gradual migration, performance monitoring, and rollback functionality
    /// </summary>
    public static class FeatureFlags
    {
        // ========== Existing Base Flags ==========

        /// <summary>
        /// Use new audio system
        /// </summary>
        public static bool UseNewAudioSystem
        {
            get => PlayerPrefs.GetInt("FeatureFlag_UseNewAudioSystem", 0) == 1;
            set => SetFlag("FeatureFlag_UseNewAudioSystem", value);
        }

        /// <summary>
        /// Use Service Locator pattern
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
        /// Completely disable access to Legacy Singletons
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
        /// Allow Singleton usage (for emergency rollback)
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
        /// Unified flag management with change logging
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
        /// Set gradual migration phase preset
        /// </summary>
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
        /// Emergency rollback (revert all to Singleton settings)
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