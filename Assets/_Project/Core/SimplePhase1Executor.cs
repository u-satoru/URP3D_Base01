using UnityEngine;
using asterivo.Unity60.Core;

/// <summary>
/// Simple Phase 1 Executor - minimal dependencies, direct execution
/// Executes Phase 1 as requested: じっくり考えて実行
/// </summary>
public class SimplePhase1Executor : MonoBehaviour
{
    private void Start()
    {
        // Auto-execute Phase 1 on start
        ExecutePhase1Now();
    }
    
    /// <summary>
    /// Execute Phase 1: 包括的バックアップ作成と最終設定 directly
    /// </summary>
    public void ExecutePhase1Now()
    {
        Debug.Log("=== SIMPLE PHASE 1 EXECUTOR STARTING ===");
        Debug.Log("Executing: じっくり考えて Phase 1: 準備フェーズ を実行");
        
        try
        {
            // Log current FeatureFlags before execution
            Debug.Log($"[Phase1] Pre-execution - DisableLegacySingletons: {FeatureFlags.DisableLegacySingletons}");
            Debug.Log($"[Phase1] Pre-execution - EnableMigrationWarnings: {FeatureFlags.EnableMigrationWarnings}");
            Debug.Log($"[Phase1] Pre-execution - EnableMigrationMonitoring: {FeatureFlags.EnableMigrationMonitoring}");
            
            // Execute Phase 1 comprehensive backup and final settings
            Debug.Log("[Phase1] Calling FeatureFlags.ExecutePhase1ComprehensiveBackupAndFinalSettings()...");
            FeatureFlags.ExecutePhase1ComprehensiveBackupAndFinalSettings();
            
            // Log post-execution state
            Debug.Log($"[Phase1] Post-execution - DisableLegacySingletons: {FeatureFlags.DisableLegacySingletons}");
            Debug.Log($"[Phase1] Post-execution - EnableMigrationWarnings: {FeatureFlags.EnableMigrationWarnings}");
            Debug.Log($"[Phase1] Post-execution - EnableMigrationMonitoring: {FeatureFlags.EnableMigrationMonitoring}");
            
            // Validate Phase 1 completion
            bool phase1Complete = FeatureFlags.DisableLegacySingletons && 
                                !FeatureFlags.EnableMigrationWarnings && 
                                !FeatureFlags.EnableMigrationMonitoring;
            
            if (phase1Complete)
            {
                Debug.Log("✅ PHASE 1 COMPLETED SUCCESSFULLY!");
                Debug.Log("🎯 System ready for Phase 2: Manual Singleton code deletion");
                
                // Mark completion
                PlayerPrefs.SetInt("SimplePhase1_Complete", 1);
                PlayerPrefs.SetString("SimplePhase1_CompletionTime", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                PlayerPrefs.Save();
                
                Debug.Log("📋 NEXT ACTION: Execute Phase 2 per SINGLETON_COMPLETE_REMOVAL_GUIDE.md");
            }
            else
            {
                Debug.LogWarning("⚠️ Phase 1 validation failed - expected state not achieved");
                Debug.LogWarning($"Expected: DisableLegacySingletons=true, EnableMigrationWarnings=false, EnableMigrationMonitoring=false");
                Debug.LogWarning($"Actual: DisableLegacySingletons={FeatureFlags.DisableLegacySingletons}, EnableMigrationWarnings={FeatureFlags.EnableMigrationWarnings}, EnableMigrationMonitoring={FeatureFlags.EnableMigrationMonitoring}");
            }
            
            Debug.Log("=== SIMPLE PHASE 1 EXECUTOR COMPLETED ===");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Simple Phase 1 execution failed: {ex.Message}");
            Debug.LogError($"Stack trace: {ex.StackTrace}");
        }
    }
}
