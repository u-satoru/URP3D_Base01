using UnityEngine;
using asterivo.Unity60.Core;
// using asterivo.Unity60.Core.Debug;
// // using asterivo.Unity60.Core.Debug; // Removed to avoid circular dependency

namespace asterivo.Unity60.Core.Services
{
    /// <summary>
    /// Direct Phase 1 execution approach
    /// Static method calls for immediate execution
    /// </summary>
    public static class DirectPhase1Call
    {
        /// <summary>
        /// Directly execute Phase 1 preparation
        /// Called from Unity Console or other scripts
        /// </summary>
        public static void ExecutePhase1Now()
        {
            ProjectDebug.Log("=== DIRECT PHASE 1 EXECUTION STARTING ===");
            
            try
            {
                // Log current state
                ProjectDebug.Log("Pre-execution FeatureFlags:");
                ProjectDebug.Log($"  DisableLegacySingletons: {FeatureFlags.DisableLegacySingletons}");
                ProjectDebug.Log($"  EnableMigrationWarnings: {FeatureFlags.EnableMigrationWarnings}");
                ProjectDebug.Log($"  EnableMigrationMonitoring: {FeatureFlags.EnableMigrationMonitoring}");
                
                // Execute the Phase 1 method
                ProjectDebug.Log("Executing FeatureFlags.ExecutePhase1ComprehensiveBackupAndFinalSettings()...");
                FeatureFlags.ExecutePhase1ComprehensiveBackupAndFinalSettings();
                
                // Log post-execution state
                ProjectDebug.Log("Post-execution FeatureFlags:");
                ProjectDebug.Log($"  DisableLegacySingletons: {FeatureFlags.DisableLegacySingletons}");
                ProjectDebug.Log($"  EnableMigrationWarnings: {FeatureFlags.EnableMigrationWarnings}");
                ProjectDebug.Log($"  EnableMigrationMonitoring: {FeatureFlags.EnableMigrationMonitoring}");
                
                // Validate completion
                bool phase1Complete = FeatureFlags.DisableLegacySingletons && 
                                    !FeatureFlags.EnableMigrationWarnings && 
                                    !FeatureFlags.EnableMigrationMonitoring;
                
                if (phase1Complete)
                {
                    ProjectDebug.Log("笨・PHASE 1 COMPLETED SUCCESSFULLY!");
                    ProjectDebug.Log("識 Ready for Phase 2: Manual Singleton code deletion");
                    
                    // Save completion status
                    PlayerPrefs.SetInt("Phase1_DirectExecutionComplete", 1);
                    PlayerPrefs.SetString("Phase1_DirectCompletionTime", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    PlayerPrefs.Save();
                }
                else
                {
                    ProjectDebug.LogWarning("笞・・PHASE 1 INCOMPLETE - Settings not in expected state");
                }
                
                ProjectDebug.Log("=== DIRECT PHASE 1 EXECUTION FINISHED ===");
            }
            catch (System.Exception ex)
            {
                ProjectDebug.LogError($"笶・Direct Phase 1 execution failed: {ex.Message}");
                ProjectDebug.LogError($"Stack trace: {ex.StackTrace}");
            }
        }
        
        /// <summary>
        /// Runtime method to execute Phase 1 when called
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void CheckAndExecutePhase1()
        {
            // Force execute for final Phase 1 confirmation (縺倥▲縺上ｊ閠・∴縺ｦ)
            ProjectDebug.Log("=== FINAL Phase 1 EXECUTION (縺倥▲縺上ｊ閠・∴縺ｦ) ===");
            ProjectDebug.Log("Force executing Phase 1 for final completion verification...");
            ExecutePhase1Now();
        }
    }
}
