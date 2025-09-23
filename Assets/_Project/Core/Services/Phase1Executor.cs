using UnityEngine;
// using asterivo.Unity60.Core.Debug;
// // using asterivo.Unity60.Core.Debug; // Removed to avoid circular dependency

namespace asterivo.Unity60.Core.Services
{
    /// <summary>
    /// Phase 1: 準備フェーズ 実行用コンポ�EネンチE    /// SINGLETON_COMPLETE_REMOVAL_GUIDE.md Phase 1 implementation
    /// </summary>
    public class Phase1Executor : MonoBehaviour
    {
        [Header("Phase 1 Configuration")]
        [SerializeField] private bool enableAutoExecution = false;
        [SerializeField] private bool requireManualConfirmation = true;
        
        private void Start()
        {
            if (enableAutoExecution && !requireManualConfirmation)
            {
                ExecutePhase1Preparation();
            }
        }
        
        /// <summary>
        /// Phase 1.1 & 1.2: 匁E��皁E��チE��アチE�E作�Eと最終設定を実衁E        /// Context Menu経由で手動実衁E        /// </summary>
        [ContextMenu("Execute Phase 1: Comprehensive Backup & Final Settings")]
        public void ExecutePhase1Preparation()
        {
            ProjectDebug.Log("[Phase1Executor] Starting Phase 1: 準備フェーズ execution...");
            
            try
            {
                // Phase 1.1 & 1.2: FeatureFlagsメソチE��を直接呼び出ぁE                FeatureFlags.ExecutePhase1ComprehensiveBackupAndFinalSettings();
                
                ProjectDebug.Log("[Phase1Executor] ✁EPhase 1 完亁E System ready for Phase 2 (Physical Code Removal)");
                ProjectDebug.Log("[Phase1Executor] 📋 Next Step: Manually execute Phase 2 code deletion per SINGLETON_COMPLETE_REMOVAL_GUIDE.md");
            }
            catch (System.Exception ex)
            {
                ProjectDebug.LogError($"[Phase1Executor] ❁EPhase 1 execution failed: {ex.Message}");
                
                // 緊急ロールバック提桁E                ProjectDebug.LogWarning("[Phase1Executor] Consider emergency rollback via Context Menu");
            }
        }
        
        /// <summary>
        /// 緊急ロールバック実衁E        /// </summary>
        [ContextMenu("Emergency Rollback Phase 1")]
        public void ExecuteEmergencyRollback()
        {
            ProjectDebug.LogWarning("[Phase1Executor] Executing emergency rollback...");
            
            FeatureFlags.ExecutePhase1EmergencyRollback();
            
            ProjectDebug.Log("[Phase1Executor] ✁EEmergency rollback completed");
        }
        
        /// <summary>
        /// 現在のFeatureFlags状態を確誁E        /// </summary>
        [ContextMenu("Check Current FeatureFlags Status")]
        public void CheckCurrentStatus()
        {
            ProjectDebug.Log("[Phase1Executor] Current FeatureFlags Status:");
            FeatureFlags.LogCurrentFlags();
            
            // Phase 1完亁E��宁E            bool phase1Ready = FeatureFlags.DisableLegacySingletons && 
                             !FeatureFlags.EnableMigrationWarnings && 
                             !FeatureFlags.EnableMigrationMonitoring;
                             
            if (phase1Ready)
            {
                ProjectDebug.Log("[Phase1Executor] ✁EPhase 1 COMPLETED - Ready for Phase 2");
            }
            else
            {
                ProjectDebug.Log("[Phase1Executor] ⚠�E�EPhase 1 NOT COMPLETED - Execute Phase 1 preparation first");
            }
        }
    }
}