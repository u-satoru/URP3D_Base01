using UnityEngine;
using asterivo.Unity60.Core;
// using asterivo.Unity60.Core.Debug;
// // using asterivo.Unity60.Core.Debug; // Removed to avoid circular dependency

namespace asterivo.Unity60.Core.Services
{
    /// <summary>
    /// Phase 1 直接実行用ランタイムスクリプト
    /// Unity起動時に自動的にPhase 1を実衁E    /// </summary>
    public static class Phase1DirectExecution
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void ExecutePhase1Automatically()
        {
            // Phase 1実行済みかチェチE��
            if (PlayerPrefs.GetInt("Phase1_ExecutionComplete", 0) == 1)
            {
                ProjectDebug.Log("[Phase1DirectExecution] Phase 1 already completed. Skipping automatic execution.");
                return;
            }
            
            ProjectDebug.Log("[Phase1DirectExecution] === AUTO-EXECUTING PHASE 1: 準備フェーズ ===");
            
            try
            {
                // FeatureFlagsのPhase 1メソチE��を直接実衁E                FeatureFlags.ExecutePhase1ComprehensiveBackupAndFinalSettings();
                
                // 完亁E��ラグを設宁E                PlayerPrefs.SetInt("Phase1_ExecutionComplete", 1);
                PlayerPrefs.SetString("Phase1_CompletionTime", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                PlayerPrefs.Save();
                
                ProjectDebug.Log("[Phase1DirectExecution] ✁EPHASE 1 COMPLETE: System ready for Phase 2 (Manual Code Deletion)");
                ProjectDebug.Log("[Phase1DirectExecution] 📋 NEXT ACTION: Execute Phase 2 per SINGLETON_COMPLETE_REMOVAL_GUIDE.md");
                
                // 現在の状態を確認表示
                LogPhase1CompletionStatus();
            }
            catch (System.Exception ex)
            {
                ProjectDebug.LogError($"[Phase1DirectExecution] ❁EPhase 1 execution failed: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Phase 1完亁E��況をログ表示
        /// </summary>
        public static void LogPhase1CompletionStatus()
        {
            ProjectDebug.Log("[Phase1DirectExecution] === Phase 1 Completion Status ===");
            ProjectDebug.Log($"  DisableLegacySingletons: {FeatureFlags.DisableLegacySingletons}");
            ProjectDebug.Log($"  EnableMigrationWarnings: {FeatureFlags.EnableMigrationWarnings}");
            ProjectDebug.Log($"  EnableMigrationMonitoring: {FeatureFlags.EnableMigrationMonitoring}");
            
            bool phase1Complete = FeatureFlags.DisableLegacySingletons && 
                                !FeatureFlags.EnableMigrationWarnings && 
                                !FeatureFlags.EnableMigrationMonitoring;
                                
            ProjectDebug.Log($"  Phase 1 Complete: {(phase1Complete ? "✁EYES" : "❁ENO")}");
            
            if (phase1Complete)
            {
                string completionTime = PlayerPrefs.GetString("Phase1_CompletionTime", "Unknown");
                ProjectDebug.Log($"  Completion Time: {completionTime}");
                ProjectDebug.Log("  🎯 READY FOR PHASE 2: Manual Singleton code deletion");
            }
        }
        
        /// <summary>
        /// Phase 1実行状況をリセチE���E�テスト用�E�E        /// </summary>
        public static void ResetPhase1ExecutionState()
        {
            PlayerPrefs.DeleteKey("Phase1_ExecutionComplete");
            PlayerPrefs.DeleteKey("Phase1_CompletionTime");
            PlayerPrefs.Save();
            
            ProjectDebug.Log("[Phase1DirectExecution] Phase 1 execution state reset. Will re-execute on next scene load.");
        }
    }
}