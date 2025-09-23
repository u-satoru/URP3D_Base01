using UnityEngine;
using asterivo.Unity60.Core;
// using asterivo.Unity60.Core.Debug;
// // using asterivo.Unity60.Core.Debug; // Removed to avoid circular dependency

namespace asterivo.Unity60.Core.Services
{
    /// <summary>
    /// Phase 1 逶ｴ謗･螳溯｡檎畑繝ｩ繝ｳ繧ｿ繧､繝繧ｹ繧ｯ繝ｪ繝励ヨ
    /// Unity襍ｷ蜍墓凾縺ｫ閾ｪ蜍慕噪縺ｫPhase 1繧貞ｮ溯｡・    /// </summary>
    public static class Phase1DirectExecution
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void ExecutePhase1Automatically()
        {
            // Phase 1螳溯｡梧ｸ医∩縺九メ繧ｧ繝・け
            if (PlayerPrefs.GetInt("Phase1_ExecutionComplete", 0) == 1)
            {
                ProjectDebug.Log("[Phase1DirectExecution] Phase 1 already completed. Skipping automatic execution.");
                return;
            }
            
            ProjectDebug.Log("[Phase1DirectExecution] === AUTO-EXECUTING PHASE 1: 貅門ｙ繝輔ぉ繝ｼ繧ｺ ===");
            
            try
            {
                // FeatureFlags縺ｮPhase 1繝｡繧ｽ繝・ラ繧堤峩謗･螳溯｡・                FeatureFlags.ExecutePhase1ComprehensiveBackupAndFinalSettings();
                
                // 螳御ｺ・ヵ繝ｩ繧ｰ繧定ｨｭ螳・                PlayerPrefs.SetInt("Phase1_ExecutionComplete", 1);
                PlayerPrefs.SetString("Phase1_CompletionTime", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                PlayerPrefs.Save();
                
                ProjectDebug.Log("[Phase1DirectExecution] 笨・PHASE 1 COMPLETE: System ready for Phase 2 (Manual Code Deletion)");
                ProjectDebug.Log("[Phase1DirectExecution] 搭 NEXT ACTION: Execute Phase 2 per SINGLETON_COMPLETE_REMOVAL_GUIDE.md");
                
                // 迴ｾ蝨ｨ縺ｮ迥ｶ諷九ｒ遒ｺ隱崎｡ｨ遉ｺ
                LogPhase1CompletionStatus();
            }
            catch (System.Exception ex)
            {
                ProjectDebug.LogError($"[Phase1DirectExecution] 笶・Phase 1 execution failed: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Phase 1螳御ｺ・憾豕√ｒ繝ｭ繧ｰ陦ｨ遉ｺ
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
                                
            ProjectDebug.Log($"  Phase 1 Complete: {(phase1Complete ? "笨・YES" : "笶・NO")}");
            
            if (phase1Complete)
            {
                string completionTime = PlayerPrefs.GetString("Phase1_CompletionTime", "Unknown");
                ProjectDebug.Log($"  Completion Time: {completionTime}");
                ProjectDebug.Log("  識 READY FOR PHASE 2: Manual Singleton code deletion");
            }
        }
        
        /// <summary>
        /// Phase 1螳溯｡檎憾豕√ｒ繝ｪ繧ｻ繝・ヨ・医ユ繧ｹ繝育畑・・        /// </summary>
        public static void ResetPhase1ExecutionState()
        {
            PlayerPrefs.DeleteKey("Phase1_ExecutionComplete");
            PlayerPrefs.DeleteKey("Phase1_CompletionTime");
            PlayerPrefs.Save();
            
            ProjectDebug.Log("[Phase1DirectExecution] Phase 1 execution state reset. Will re-execute on next scene load.");
        }
    }
}