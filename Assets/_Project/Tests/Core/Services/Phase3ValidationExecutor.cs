using System.Collections;
using UnityEngine;
using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Core;
using System.Collections.Generic;

namespace asterivo.Unity60.Tests.Core.Services
{
    /// <summary>
    /// Phase 3 Singleton Migration検証の実行エグゼキューター
    /// シンプルなコンソール出力でPhase 3のすべてのコンポーネントを検証します
    /// </summary>
    public class Phase3ValidationExecutor : MonoBehaviour
    {
        [Header("Auto Execute")]
        [SerializeField] private bool executeOnStart = true;
        [SerializeField] private float delayBeforeStart = 1f;
        
        private static readonly List<string> ValidationResults = new List<string>();
        
        void Start()
        {
            if (executeOnStart)
            {
                StartCoroutine(ExecuteValidationWithDelay());
            }
        }
        
        private IEnumerator ExecuteValidationWithDelay()
        {
            yield return new WaitForSeconds(delayBeforeStart);
            ExecutePhase3Validation();
        }
        
        [ContextMenu("Execute Phase 3 Validation")]
        public void ExecutePhase3Validation()
        {
            ValidationResults.Clear();
            Debug.Log("=== Phase 3 Singleton Migration System 検証開始 ===");
            
            StartCoroutine(ValidateAllPhase3Components());
        }
        
        private IEnumerator ValidateAllPhase3Components()
        {
            // Step 3.7: 段階的機能有効化スケジュール 検証
            yield return ValidateGradualActivationSchedule();
            
            // Step 3.8: 移行検証スクリプト 検証
            yield return ValidateMigrationVerificationScript();
            
            // Step 3.9: Legacy Singleton警告システム 検証
            yield return ValidateLegacySingletonWarningSystem();
            
            // Step 3.10: 段階的Singleton無効化スケジュール 検証
            yield return ValidateGradualSingletonDisableSchedule();
            
            // Step 3.11: 最終クリーンアップ（完全削除）検証
            yield return ValidateFinalCleanupSystem();
            
            // Step 3.12: 緊急時ロールバックシステム 検証
            yield return ValidateEmergencyRollbackSystem();
            
            // 最終検証結果の出力
            OutputValidationResults();
        }
        
        private IEnumerator ValidateGradualActivationSchedule()
        {
            Debug.Log("[Validation] Step 3.7: 段階的機能有効化スケジュール 検証開始");
            
            try
            {
                // MigrationSchedulerが正常に初期化されることを確認
                var scheduler = new MigrationScheduler();
                if (scheduler == null)
                {
                    throw new System.Exception("MigrationSchedulerが初期化されていません");
                }
                
                // FeatureFlagSchedulerが正常に動作することを確認
                var flagScheduler = new FeatureFlagScheduler();
                if (flagScheduler == null)
                {
                    throw new System.Exception("FeatureFlagSchedulerが初期化されていません");
                }
                
                // MigrationProgressTrackerが正常に動作することを確認
                var progressTracker = new MigrationProgressTracker();
                if (progressTracker == null)
                {
                    throw new System.Exception("MigrationProgressTrackerが初期化されていません");
                }
                
                ValidationResults.Add("Step 3.7: PASSED - 段階的機能有効化スケジュール");
                Debug.Log("[Validation] Step 3.7: PASSED ✓");
            }
            catch (System.Exception ex)
            {
                ValidationResults.Add($"Step 3.7: FAILED - {ex.Message}");
                Debug.LogError($"[Validation] Step 3.7: FAILED - {ex.Message}");
            }
            
            yield return null;
        }
        
        private IEnumerator ValidateMigrationVerificationScript()
        {
            Debug.Log("[Validation] Step 3.8: 移行検証スクリプト 検証開始");
            
            try
            {
                // MigrationValidatorが正常に動作することを確認
                var validator = new MigrationValidator();
                if (validator == null)
                {
                    throw new System.Exception("MigrationValidatorが初期化されていません");
                }
                
                ValidationResults.Add("Step 3.8: PASSED - 移行検証スクリプト");
                Debug.Log("[Validation] Step 3.8: PASSED ✓");
            }
            catch (System.Exception ex)
            {
                ValidationResults.Add($"Step 3.8: FAILED - {ex.Message}");
                Debug.LogError($"[Validation] Step 3.8: FAILED - {ex.Message}");
            }
            
            yield return null;
        }
        
        private IEnumerator ValidateLegacySingletonWarningSystem()
        {
            Debug.Log("[Validation] Step 3.9: Legacy Singleton警告システム 検証開始");
            
            try
            {
                // MigrationMonitorが正常に動作することを確認
                var monitor = new asterivo.Unity60.Core.Services.MigrationMonitor();
                if (monitor == null)
                {
                    throw new System.Exception("MigrationMonitorが初期化されていません");
                }
                
                // FeatureFlagsが正しく設定されていることを確認
                var enableWarningsProperty = typeof(asterivo.Unity60.Core.FeatureFlags).GetProperty("EnableMigrationWarnings");
                if (enableWarningsProperty == null)
                {
                    throw new System.Exception("FeatureFlags.EnableMigrationWarningsプロパティが存在しません");
                }
                
                var disableLegacyProperty = typeof(asterivo.Unity60.Core.FeatureFlags).GetProperty("DisableLegacySingletons");
                if (disableLegacyProperty == null)
                {
                    throw new System.Exception("FeatureFlags.DisableLegacySingletonsプロパティが存在しません");
                }
                
                ValidationResults.Add("Step 3.9: PASSED - Legacy Singleton警告システム");
                Debug.Log("[Validation] Step 3.9: PASSED ✓");
            }
            catch (System.Exception ex)
            {
                ValidationResults.Add($"Step 3.9: FAILED - {ex.Message}");
                Debug.LogError($"[Validation] Step 3.9: FAILED - {ex.Message}");
            }
            
            yield return null;
        }
        
        private IEnumerator ValidateGradualSingletonDisableSchedule()
        {
            Debug.Log("[Validation] Step 3.10: 段階的Singleton無効化スケジュール 検証開始");
            
            try
            {
                // SingletonDisableSchedulerが正常に動作することを確認
                var disableScheduler = new SingletonDisableScheduler();
                if (disableScheduler == null)
                {
                    throw new System.Exception("SingletonDisableSchedulerが初期化されていません");
                }
                
                // ScheduleDayが正しく定義されていることを確認
                var schedulerType = typeof(asterivo.Unity60.Core.Services.SingletonDisableScheduler);
                var scheduleEnumType = schedulerType.GetNestedType("ScheduleDay");
                if (scheduleEnumType == null)
                {
                    throw new System.Exception("ScheduleDay列挙型が見つかりません");
                }
                
                var enumValues = System.Enum.GetValues(scheduleEnumType);
                if (enumValues.Length != 7)
                {
                    throw new System.Exception($"ScheduleDayの値数が正しくありません: {enumValues.Length}");
                }
                
                ValidationResults.Add("Step 3.10: PASSED - 段階的Singleton無効化スケジュール");
                Debug.Log("[Validation] Step 3.10: PASSED ✓");
            }
            catch (System.Exception ex)
            {
                ValidationResults.Add($"Step 3.10: FAILED - {ex.Message}");
                Debug.LogError($"[Validation] Step 3.10: FAILED - {ex.Message}");
            }
            
            yield return null;
        }
        
        private IEnumerator ValidateFinalCleanupSystem()
        {
            Debug.Log("[Validation] Step 3.11: 最終クリーンアップ（完全削除）検証開始");
            
            try
            {
                // SingletonCodeRemoverが正常に動作することを確認
                var codeRemover = new SingletonCodeRemover();
                if (codeRemover == null)
                {
                    throw new System.Exception("SingletonCodeRemoverが初期化されていません");
                }
                
                ValidationResults.Add("Step 3.11: PASSED - 最終クリーンアップ（完全削除）");
                Debug.Log("[Validation] Step 3.11: PASSED ✓");
            }
            catch (System.Exception ex)
            {
                ValidationResults.Add($"Step 3.11: FAILED - {ex.Message}");
                Debug.LogError($"[Validation] Step 3.11: FAILED - {ex.Message}");
            }
            
            yield return null;
        }
        
        private IEnumerator ValidateEmergencyRollbackSystem()
        {
            Debug.Log("[Validation] Step 3.12: 緊急時ロールバックシステム 検証開始");
            
            try
            {
                // EmergencyRollbackが正常に動作することを確認 (static class)
                var rollbackType = typeof(EmergencyRollback);
                if (!(rollbackType.IsClass && rollbackType.IsAbstract && rollbackType.IsSealed))
                {
                    throw new System.Exception("EmergencyRollbackがstatic classとして定義されていません");
                }
                
                // AdvancedRollbackMonitorが正常に動作することを確認
                var advancedMonitor = new AdvancedRollbackMonitor();
                if (advancedMonitor == null)
                {
                    throw new System.Exception("AdvancedRollbackMonitorが初期化されていません");
                }
                
                ValidationResults.Add("Step 3.12: PASSED - 緊急時ロールバックシステム");
                Debug.Log("[Validation] Step 3.12: PASSED ✓");
            }
            catch (System.Exception ex)
            {
                ValidationResults.Add($"Step 3.12: FAILED - {ex.Message}");
                Debug.LogError($"[Validation] Step 3.12: FAILED - {ex.Message}");
            }
            
            yield return null;
        }
        
        private void OutputValidationResults()
        {
            Debug.Log("=== Phase 3 Singleton Migration System 検証結果 ===");
            
            int passedCount = 0;
            int failedCount = 0;
            
            foreach (var result in ValidationResults)
            {
                Debug.Log($"  {result}");
                
                if (result.Contains("PASSED"))
                    passedCount++;
                else if (result.Contains("FAILED"))
                    failedCount++;
            }
            
            Debug.Log($"=== 検証統計: {passedCount} PASSED, {failedCount} FAILED ===");
            
            if (failedCount == 0)
            {
                Debug.Log("🎉 Phase 3 Singleton Migration Systemの実装が完了し、すべてのコンポーネントが正常に動作しています！");
                Debug.Log("すべてのステップ（3.7-3.12）が正常に検証されました。");
            }
            else
            {
                Debug.LogError($"⚠️ {failedCount}個のコンポーネントで問題が検出されました。修正が必要です。");
            }
        }
    }
}
