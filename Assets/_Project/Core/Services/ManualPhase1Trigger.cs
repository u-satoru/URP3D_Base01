using UnityEngine;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Debug;

namespace asterivo.Unity60.Core.Services
{
    /// <summary>
    /// Manual Phase 1 Trigger for SINGLETON_COMPLETE_REMOVAL_GUIDE.md execution
    /// Context Menu driven approach for reliable execution
    /// </summary>
    public class ManualPhase1Trigger : MonoBehaviour
    {
        [Header("Phase 1 Status")]
        [SerializeField] private bool phase1Completed = false;
        
        private void Start()
        {
            CheckPhase1Status();
        }
        
        /// <summary>
        /// Execute Phase 1 manually via Context Menu
        /// </summary>
        [ContextMenu("EXECUTE PHASE 1: Complete Singleton Removal Preparation")]
        public void ExecutePhase1Manual()
        {
            ProjectDebug.Log("[ManualPhase1Trigger] === MANUAL PHASE 1 EXECUTION STARTING ===");
            
            try
            {
                // Check current FeatureFlags before execution
                ProjectDebug.Log("[ManualPhase1Trigger] Pre-execution FeatureFlags status:");
                LogCurrentFeatureFlagsStatus();
                
                // Execute Phase 1 comprehensive backup and final settings
                ProjectDebug.Log("[ManualPhase1Trigger] Calling FeatureFlags.ExecutePhase1ComprehensiveBackupAndFinalSettings()...");
                FeatureFlags.ExecutePhase1ComprehensiveBackupAndFinalSettings();
                
                // Mark as completed
                PlayerPrefs.SetInt("Phase1_ManualExecutionComplete", 1);
                PlayerPrefs.SetString("Phase1_ManualCompletionTime", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                PlayerPrefs.Save();
                
                phase1Completed = true;
                
                // Check FeatureFlags after execution
                ProjectDebug.Log("[ManualPhase1Trigger] Post-execution FeatureFlags status:");
                LogCurrentFeatureFlagsStatus();
                
                ProjectDebug.Log("[ManualPhase1Trigger] ✅ PHASE 1 MANUAL EXECUTION COMPLETED SUCCESSFULLY");
                ProjectDebug.Log("[ManualPhase1Trigger] 📋 NEXT STEP: Execute Phase 2 per SINGLETON_COMPLETE_REMOVAL_GUIDE.md");
                
                ValidatePhase1Completion();
            }
            catch (System.Exception ex)
            {
                ProjectDebug.LogError($"[ManualPhase1Trigger] ❌ Phase 1 manual execution failed: {ex.Message}");
                ProjectDebug.LogError($"[ManualPhase1Trigger] Stack trace: {ex.StackTrace}");
            }
        }
        
        /// <summary>
        /// Check current Phase 1 status
        /// </summary>
        [ContextMenu("Check Phase 1 Status")]
        public void CheckPhase1Status()
        {
            ProjectDebug.Log("[ManualPhase1Trigger] === PHASE 1 STATUS CHECK ===");
            
            bool manuallyCompleted = PlayerPrefs.GetInt("Phase1_ManualExecutionComplete", 0) == 1;
            string completionTime = PlayerPrefs.GetString("Phase1_ManualCompletionTime", "Not executed");
            
            ProjectDebug.Log($"[ManualPhase1Trigger] Manual Execution Completed: {manuallyCompleted}");
            ProjectDebug.Log($"[ManualPhase1Trigger] Completion Time: {completionTime}");
            
            LogCurrentFeatureFlagsStatus();
            ValidatePhase1Completion();
        }
        
        /// <summary>
        /// Log current FeatureFlags status
        /// </summary>
        private void LogCurrentFeatureFlagsStatus()
        {
            ProjectDebug.Log("[ManualPhase1Trigger] Current FeatureFlags:");
            ProjectDebug.Log($"  UseServiceLocator: {FeatureFlags.UseServiceLocator}");
            ProjectDebug.Log($"  DisableLegacySingletons: {FeatureFlags.DisableLegacySingletons}");
            ProjectDebug.Log($"  EnableMigrationWarnings: {FeatureFlags.EnableMigrationWarnings}");
            ProjectDebug.Log($"  EnableMigrationMonitoring: {FeatureFlags.EnableMigrationMonitoring}");
        }
        
        /// <summary>
        /// Validate Phase 1 completion according to SINGLETON_COMPLETE_REMOVAL_GUIDE.md
        /// </summary>
        private void ValidatePhase1Completion()
        {
            bool phase1Complete = FeatureFlags.DisableLegacySingletons && 
                                !FeatureFlags.EnableMigrationWarnings && 
                                !FeatureFlags.EnableMigrationMonitoring;
                                
            if (phase1Complete)
            {
                ProjectDebug.Log("[ManualPhase1Trigger] ✅ PHASE 1 VALIDATION: COMPLETE");
                ProjectDebug.Log("[ManualPhase1Trigger] 🎯 System is ready for Phase 2: Manual Singleton code deletion");
                phase1Completed = true;
            }
            else
            {
                ProjectDebug.Log("[ManualPhase1Trigger] ⚠️ PHASE 1 VALIDATION: INCOMPLETE");
                ProjectDebug.Log("[ManualPhase1Trigger] Required settings for Phase 1 completion:");
                ProjectDebug.Log($"  DisableLegacySingletons must be TRUE (current: {FeatureFlags.DisableLegacySingletons})");
                ProjectDebug.Log($"  EnableMigrationWarnings must be FALSE (current: {FeatureFlags.EnableMigrationWarnings})");
                ProjectDebug.Log($"  EnableMigrationMonitoring must be FALSE (current: {FeatureFlags.EnableMigrationMonitoring})");
                phase1Completed = false;
            }
        }
        
        /// <summary>
        /// Reset Phase 1 execution state (for testing)
        /// </summary>
        [ContextMenu("Reset Phase 1 State")]
        public void ResetPhase1State()
        {
            PlayerPrefs.DeleteKey("Phase1_ManualExecutionComplete");
            PlayerPrefs.DeleteKey("Phase1_ManualCompletionTime");
            PlayerPrefs.Save();
            
            phase1Completed = false;
            
            ProjectDebug.Log("[ManualPhase1Trigger] Phase 1 execution state reset.");
        }
    }
}