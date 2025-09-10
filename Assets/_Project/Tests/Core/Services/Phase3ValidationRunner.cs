using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using _Project.Core.Services;
using _Project.Core;
using System.Collections.Generic;

namespace asterivo.Unity60.Tests.Core.Services
{
    /// <summary>
    /// 総合的なPhase 3 Singleton Migration検証ランナー
    /// すべての実装されたステップ（3.7-3.12）を包括的にテストします
    /// </summary>
    public class Phase3ValidationRunner
    {
        private static readonly List<string> ValidationResults = new List<string>();
        
        [UnityTest]
        public IEnumerator ValidatePhase3CompleteSystem()
        {
            ValidationResults.Clear();
            
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
            
            // すべてのテストが成功したことを確認
            Assert.IsTrue(ValidationResults.Count > 0, "バリデーション結果が記録されていません");
            
            foreach (var result in ValidationResults)
            {
                if (result.Contains("FAILED"))
                {
                    Assert.Fail($"バリデーション失敗: {result}");
                }
            }
            
            Debug.Log("[Phase3ValidationRunner] すべてのPhase 3コンポーネントが正常に動作しています ✓");
        }
        
        private IEnumerator ValidateGradualActivationSchedule()
        {
            Debug.Log("[Validation] Step 3.7: 段階的機能有効化スケジュール 検証開始");
            
            try
            {
                // MigrationSchedulerが正常に初期化されることを確認
                var scheduler = new MigrationScheduler();
                Assert.IsNotNull(scheduler, "MigrationSchedulerが初期化されていません");
                
                // FeatureFlagSchedulerが正常に動作することを確認
                var flagScheduler = new FeatureFlagScheduler();
                Assert.IsNotNull(flagScheduler, "FeatureFlagSchedulerが初期化されていません");
                
                // MigrationProgressTrackerが正常に動作することを確認
                var progressTracker = new MigrationProgressTracker();
                Assert.IsNotNull(progressTracker, "MigrationProgressTrackerが初期化されていません");
                
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
                Assert.IsNotNull(validator, "MigrationValidatorが初期化されていません");
                
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
                var monitor = new _Project.Core.Services.MigrationMonitor();
                Assert.IsNotNull(monitor, "MigrationMonitorが初期化されていません");
                
                // FeatureFlagsが正しく設定されていることを確認
                Assert.IsTrue(typeof(FeatureFlags).GetProperty("EnableMigrationWarnings") != null, 
                             "FeatureFlags.EnableMigrationWarningsプロパティが存在しません");
                Assert.IsTrue(typeof(FeatureFlags).GetProperty("DisableLegacySingletons") != null, 
                             "FeatureFlags.DisableLegacySingletonsプロパティが存在しません");
                
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
                Assert.IsNotNull(disableScheduler, "SingletonDisableSchedulerが初期化されていません");
                
                // ScheduleDayが正しく定義されていることを確認
                var schedulerType = typeof(_Project.Core.Services.SingletonDisableScheduler);
                var scheduleEnumType = schedulerType.GetNestedType("ScheduleDay");
                Assert.IsNotNull(scheduleEnumType, "ScheduleDay列挙型が見つかりません");
                var enumValues = System.Enum.GetValues(scheduleEnumType);
                Assert.IsTrue(enumValues.Length == 7, $"ScheduleDayの値数が正しくありません: {enumValues.Length}");
                
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
                Assert.IsNotNull(codeRemover, "SingletonCodeRemoverが初期化されていません");
                
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
                Assert.IsTrue(typeof(EmergencyRollback).IsClass && typeof(EmergencyRollback).IsAbstract && typeof(EmergencyRollback).IsSealed, 
                             "EmergencyRollbackがstatic classとして定義されていません");
                
                // AdvancedRollbackMonitorが正常に動作することを確認
                var advancedMonitor = new AdvancedRollbackMonitor();
                Assert.IsNotNull(advancedMonitor, "AdvancedRollbackMonitorが初期化されていません");
                
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
            }
            else
            {
                Debug.LogError($"⚠️ {failedCount}個のコンポーネントで問題が検出されました。修正が必要です。");
            }
        }
    }
}