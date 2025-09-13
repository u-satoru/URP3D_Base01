using UnityEngine;
using asterivo.Unity60.Core.Debug;
// using asterivo.Unity60.Core.Debug; // Removed to avoid circular dependency

namespace asterivo.Unity60.Core.Services
{
    /// <summary>
    /// Phase 1: 準備フェーズ 実行用コンポーネント
    /// SINGLETON_COMPLETE_REMOVAL_GUIDE.md Phase 1 implementation
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
        /// Phase 1.1 & 1.2: 包括的バックアップ作成と最終設定を実行
        /// Context Menu経由で手動実行
        /// </summary>
        [ContextMenu("Execute Phase 1: Comprehensive Backup & Final Settings")]
        public void ExecutePhase1Preparation()
        {
            ProjectDebug.Log("[Phase1Executor] Starting Phase 1: 準備フェーズ execution...");
            
            try
            {
                // Phase 1.1 & 1.2: FeatureFlagsメソッドを直接呼び出し
                FeatureFlags.ExecutePhase1ComprehensiveBackupAndFinalSettings();
                
                ProjectDebug.Log("[Phase1Executor] ✅ Phase 1 完了: System ready for Phase 2 (Physical Code Removal)");
                ProjectDebug.Log("[Phase1Executor] 📋 Next Step: Manually execute Phase 2 code deletion per SINGLETON_COMPLETE_REMOVAL_GUIDE.md");
            }
            catch (System.Exception ex)
            {
                ProjectDebug.LogError($"[Phase1Executor] ❌ Phase 1 execution failed: {ex.Message}");
                
                // 緊急ロールバック提案
                ProjectDebug.LogWarning("[Phase1Executor] Consider emergency rollback via Context Menu");
            }
        }
        
        /// <summary>
        /// 緊急ロールバック実行
        /// </summary>
        [ContextMenu("Emergency Rollback Phase 1")]
        public void ExecuteEmergencyRollback()
        {
            ProjectDebug.LogWarning("[Phase1Executor] Executing emergency rollback...");
            
            FeatureFlags.ExecutePhase1EmergencyRollback();
            
            ProjectDebug.Log("[Phase1Executor] ✅ Emergency rollback completed");
        }
        
        /// <summary>
        /// 現在のFeatureFlags状態を確認
        /// </summary>
        [ContextMenu("Check Current FeatureFlags Status")]
        public void CheckCurrentStatus()
        {
            ProjectDebug.Log("[Phase1Executor] Current FeatureFlags Status:");
            FeatureFlags.LogCurrentFlags();
            
            // Phase 1完了判定
            bool phase1Ready = FeatureFlags.DisableLegacySingletons && 
                             !FeatureFlags.EnableMigrationWarnings && 
                             !FeatureFlags.EnableMigrationMonitoring;
                             
            if (phase1Ready)
            {
                ProjectDebug.Log("[Phase1Executor] ✅ Phase 1 COMPLETED - Ready for Phase 2");
            }
            else
            {
                ProjectDebug.Log("[Phase1Executor] ⚠️ Phase 1 NOT COMPLETED - Execute Phase 1 preparation first");
            }
        }
    }
}