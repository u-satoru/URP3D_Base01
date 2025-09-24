using UnityEngine;
// using asterivo.Unity60.Core.Debug;
// // using asterivo.Unity60.Core.Debug; // Removed to avoid circular dependency

namespace asterivo.Unity60.Core.Services
{
    /// <summary>
    /// Phase 1: 貅門ｙ繝輔ぉ繝ｼ繧ｺ 螳溯｡檎畑繧ｳ繝ｳ繝昴・繝阪Φ繝・    /// SINGLETON_COMPLETE_REMOVAL_GUIDE.md Phase 1 implementation
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
        /// Phase 1.1 & 1.2: 蛹・峡逧・ヰ繝・け繧｢繝・・菴懈・縺ｨ譛邨りｨｭ螳壹ｒ螳溯｡・        /// Context Menu邨檎罰縺ｧ謇句虚螳溯｡・        /// </summary>
        [ContextMenu("Execute Phase 1: Comprehensive Backup & Final Settings")]
        public void ExecutePhase1Preparation()
        {
            ProjectDebug.Log("[Phase1Executor] Starting Phase 1: 貅門ｙ繝輔ぉ繝ｼ繧ｺ execution...");
            
            try
            {
                // Phase 1.1 & 1.2: FeatureFlags繝｡繧ｽ繝・ラ繧堤峩謗･蜻ｼ縺ｳ蜃ｺ縺・                FeatureFlags.ExecutePhase1ComprehensiveBackupAndFinalSettings();
                
                ProjectDebug.Log("[Phase1Executor] 笨・Phase 1 螳御ｺ・ System ready for Phase 2 (Physical Code Removal)");
                ProjectDebug.Log("[Phase1Executor] 搭 Next Step: Manually execute Phase 2 code deletion per SINGLETON_COMPLETE_REMOVAL_GUIDE.md");
            }
            catch (System.Exception ex)
            {
                ProjectDebug.LogError($"[Phase1Executor] 笶・Phase 1 execution failed: {ex.Message}");
                
                // 邱頑･繝ｭ繝ｼ繝ｫ繝舌ャ繧ｯ謠先｡・                ProjectDebug.LogWarning("[Phase1Executor] Consider emergency rollback via Context Menu");
            }
        }
        
        /// <summary>
        /// 邱頑･繝ｭ繝ｼ繝ｫ繝舌ャ繧ｯ螳溯｡・        /// </summary>
        [ContextMenu("Emergency Rollback Phase 1")]
        public void ExecuteEmergencyRollback()
        {
            ProjectDebug.LogWarning("[Phase1Executor] Executing emergency rollback...");
            
            FeatureFlags.ExecutePhase1EmergencyRollback();
            
            ProjectDebug.Log("[Phase1Executor] 笨・Emergency rollback completed");
        }
        
        /// <summary>
        /// 迴ｾ蝨ｨ縺ｮFeatureFlags迥ｶ諷九ｒ遒ｺ隱・        /// </summary>
        [ContextMenu("Check Current FeatureFlags Status")]
        public void CheckCurrentStatus()
        {
            ProjectDebug.Log("[Phase1Executor] Current FeatureFlags Status:");
            FeatureFlags.LogCurrentFlags();
            
            // Phase 1螳御ｺ・愛螳・            bool phase1Ready = FeatureFlags.DisableLegacySingletons && 
                             !FeatureFlags.EnableMigrationWarnings && 
                             !FeatureFlags.EnableMigrationMonitoring;
                             
            if (phase1Ready)
            {
                ProjectDebug.Log("[Phase1Executor] 笨・Phase 1 COMPLETED - Ready for Phase 2");
            }
            else
            {
                ProjectDebug.Log("[Phase1Executor] 笞・・Phase 1 NOT COMPLETED - Execute Phase 1 preparation first");
            }
        }
    }
}
