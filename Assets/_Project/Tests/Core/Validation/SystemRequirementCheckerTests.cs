using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections.Generic;
using System.Linq;
using asterivo.Unity60.Core.Editor;

namespace asterivo.Unity60.Tests.Core.Validation
{
    /// <summary>
    /// SystemRequirementChecker のユニットテスト
    /// TASK-003.1 システム要件チェック機能の品質保証
    /// </summary>
    [TestFixture]
    public class SystemRequirementCheckerTests
    {
        #region Setup and Teardown
        
        [SetUp]
        public void Setup()
        {
            // テスト前の準備処理
            Debug.Log("[SystemRequirementCheckerTests] Setting up test environment...");
        }
        
        [TearDown]
        public void TearDown()
        {
            // テスト後のクリーンアップ処理
            Debug.Log("[SystemRequirementCheckerTests] Cleaning up test environment...");
        }
        
        #endregion
        
        #region Integration Tests
        
        /// <summary>
        /// システム要件チェック全体の統合テスト
        /// </summary>
        [Test]
        public void CheckAllRequirements_ShouldReturnValidReport()
        {
            // Arrange
            Debug.Log("[Test] Starting comprehensive system requirements check...");
            
            // Act
            var report = SystemRequirementChecker.CheckAllRequirements();
            
            // Assert
            Assert.IsNotNull(report, "Report should not be null");
            Assert.IsNotNull(report.results, "Results list should not be null");
            Assert.IsTrue(report.results.Count > 0, "Should have at least one check result");
            Assert.IsNotNull(report.summary, "Summary should not be null");
            Assert.IsTrue(report.checkTime != default, "Check time should be set");
            
            Debug.Log($"[Test] Report generated with {report.results.Count} checks");
            Debug.Log($"[Test] Overall validity: {report.isValid}");
            Debug.Log($"[Test] Summary: {report.summary}");
        }
        
        /// <summary>
        /// 必須要件の検証テスト
        /// </summary>
        [Test]
        public void CheckAllRequirements_ShouldIncludeRequiredChecks()
        {
            // Arrange & Act
            var report = SystemRequirementChecker.CheckAllRequirements();
            
            // Assert - 必須チェック項目が含まれていることを確認
            var checkNames = report.results.Select(r => r.checkName).ToList();
            
            Assert.IsTrue(checkNames.Any(name => name.Contains("Unity Version")), 
                "Should include Unity Version check");
            Assert.IsTrue(checkNames.Any(name => name.Contains("IDE Availability")), 
                "Should include IDE Availability check");
            Assert.IsTrue(checkNames.Any(name => name.Contains("Git Configuration")), 
                "Should include Git Configuration check");
            Assert.IsTrue(checkNames.Any(name => name.Contains("Unity Hub")), 
                "Should include Unity Hub check");
            Assert.IsTrue(checkNames.Any(name => name.Contains("Required Package")), 
                "Should include Required Package checks");
            
            Debug.Log($"[Test] Found check items: {string.Join(", ", checkNames)}");
        }
        
        /// <summary>
        /// Unity バージョンチェック特化テスト
        /// </summary>
        [Test]
        public void CheckAllRequirements_UnityVersionCheck_ShouldPass()
        {
            // Arrange & Act
            var report = SystemRequirementChecker.CheckAllRequirements();
            
            // Assert
            var unityVersionResult = report.results.FirstOrDefault(r => r.checkName.Contains("Unity Version"));
            
            Assert.IsNotNull(unityVersionResult, "Unity Version check should be present");
            Assert.IsTrue(unityVersionResult.isPassed, 
                $"Unity Version check should pass. Current version: {Application.unityVersion}");
            Assert.AreEqual(SystemRequirementChecker.RequirementSeverity.Required, 
                unityVersionResult.severity, "Unity Version should be marked as Required");
            
            Debug.Log($"[Test] Unity Version check result: {unityVersionResult.message}");
        }
        
        /// <summary>
        /// パッケージ要件チェックテスト
        /// </summary>
        [Test]
        public void CheckAllRequirements_ShouldCheckRequiredPackages()
        {
            // Arrange & Act
            var report = SystemRequirementChecker.CheckAllRequirements();
            
            // Assert
            var packageResults = report.results.Where(r => r.checkName.Contains("Package")).ToList();
            
            Assert.IsTrue(packageResults.Count > 0, "Should have package check results");
            
            // URP パッケージの存在確認
            var urpCheck = packageResults.FirstOrDefault(r => r.checkName.Contains("render-pipelines.universal"));
            Assert.IsNotNull(urpCheck, "Should check URP package");
            
            // Input System パッケージの存在確認
            var inputSystemCheck = packageResults.FirstOrDefault(r => r.checkName.Contains("inputsystem"));
            Assert.IsNotNull(inputSystemCheck, "Should check Input System package");
            
            // Cinemachine パッケージの存在確認
            var cinemachineCheck = packageResults.FirstOrDefault(r => r.checkName.Contains("cinemachine"));
            Assert.IsNotNull(cinemachineCheck, "Should check Cinemachine package");
            
            Debug.Log($"[Test] Package checks completed: {packageResults.Count} packages verified");
            
            foreach (var packageResult in packageResults)
            {
                Debug.Log($"[Test] {packageResult.checkName}: {(packageResult.isPassed ? "PASS" : "FAIL")} - {packageResult.message}");
            }
        }
        
        #endregion
        
        #region Validation Tests
        
        /// <summary>
        /// レポート構造の妥当性テスト
        /// </summary>
        [Test]
        public void SystemRequirementReport_ShouldHaveValidStructure()
        {
            // Arrange & Act
            var report = SystemRequirementChecker.CheckAllRequirements();
            
            // Assert
            // 各結果項目の検証
            foreach (var result in report.results)
            {
                Assert.IsNotNull(result.checkName, "Check name should not be null");
                Assert.IsTrue(!string.IsNullOrWhiteSpace(result.checkName), "Check name should not be empty");
                Assert.IsNotNull(result.message, "Message should not be null");
                Assert.IsTrue(!string.IsNullOrWhiteSpace(result.message), "Message should not be empty");
                Assert.IsNotNull(result.recommendation, "Recommendation should not be null");
                
                // Severity は enum なので値が有効かチェック
                Assert.IsTrue(System.Enum.IsDefined(typeof(SystemRequirementChecker.RequirementSeverity), result.severity),
                    $"Invalid severity value for {result.checkName}");
            }
            
            Debug.Log("[Test] Report structure validation passed");
        }
        
        /// <summary>
        /// 重要度別結果の分類テスト
        /// </summary>
        [Test]
        public void CheckAllRequirements_ShouldClassifyResultsBySeverity()
        {
            // Arrange & Act
            var report = SystemRequirementChecker.CheckAllRequirements();
            
            // Assert
            var requiredChecks = report.results.Where(r => r.severity == SystemRequirementChecker.RequirementSeverity.Required).ToList();
            var importantChecks = report.results.Where(r => r.severity == SystemRequirementChecker.RequirementSeverity.Important).ToList();
            var optionalChecks = report.results.Where(r => r.severity == SystemRequirementChecker.RequirementSeverity.Optional).ToList();
            
            Assert.IsTrue(requiredChecks.Count > 0, "Should have at least one required check");
            
            Debug.Log($"[Test] Severity classification:");
            Debug.Log($"  - Required: {requiredChecks.Count}");
            Debug.Log($"  - Important: {importantChecks.Count}");
            Debug.Log($"  - Optional: {optionalChecks.Count}");
            
            // 必須チェックが失敗している場合、レポート全体も失敗とする
            var failedRequiredChecks = requiredChecks.Where(r => !r.isPassed).ToList();
            if (failedRequiredChecks.Count > 0)
            {
                Assert.IsFalse(report.isValid, "Report should be invalid if required checks fail");
                Debug.Log($"[Test] Found {failedRequiredChecks.Count} failed required checks - report correctly marked as invalid");
            }
            else
            {
                Debug.Log("[Test] All required checks passed");
            }
        }
        
        #endregion
        
        #region Performance Tests
        
        /// <summary>
        /// パフォーマンステスト - チェック処理は5秒以内に完了すること
        /// </summary>
        [Test]
        [Timeout(5000)]
        public void CheckAllRequirements_ShouldCompleteWithinTimeLimit()
        {
            // Arrange
            var startTime = System.DateTime.Now;
            
            // Act
            var report = SystemRequirementChecker.CheckAllRequirements();
            
            // Assert
            var endTime = System.DateTime.Now;
            var duration = endTime - startTime;
            
            Assert.IsTrue(duration.TotalSeconds < 5.0, 
                $"System requirements check should complete within 5 seconds, but took {duration.TotalSeconds:F2} seconds");
            
            Debug.Log($"[Test] Performance test passed - completed in {duration.TotalMilliseconds:F0}ms");
        }
        
        #endregion
        
        #region Error Handling Tests
        
        /// <summary>
        /// エラーハンドリングテスト - 例外が発生しても適切にレポートを返すこと
        /// </summary>
        [Test]
        public void CheckAllRequirements_ShouldHandleExceptionsGracefully()
        {
            // Arrange & Act
            var report = SystemRequirementChecker.CheckAllRequirements();
            
            // Assert
            Assert.IsNotNull(report, "Should return a report even if some checks fail");
            Assert.IsNotNull(report.results, "Should have results even if some checks fail");
            Assert.IsNotNull(report.summary, "Should have a summary even if some checks fail");
            
            // 各結果にエラーメッセージが適切に設定されていることを確認
            foreach (var result in report.results.Where(r => !r.isPassed))
            {
                Assert.IsTrue(!string.IsNullOrWhiteSpace(result.recommendation), 
                    $"Failed check '{result.checkName}' should have a recommendation");
            }
            
            Debug.Log("[Test] Error handling test completed successfully");
        }
        
        #endregion
        
        #region Menu Integration Tests
        
        /// <summary>
        /// メニュー統合テスト - メニューから実行可能であることを確認
        /// </summary>
        [Test]
        public void MenuIntegration_CheckAllRequirementsFromMenu_ShouldExecuteWithoutErrors()
        {
            // Arrange & Act & Assert
            try
            {
                // メニューメソッドを直接呼び出し
                SystemRequirementChecker.CheckAllRequirementsFromMenu();
                
                // 例外が発生しなければ成功
                Assert.Pass("Menu integration executed without errors");
            }
            catch (System.Exception ex)
            {
                Assert.Fail($"Menu integration failed with exception: {ex.Message}");
            }
        }
        
        #endregion
        
        #region Utility Tests
        
        /// <summary>
        /// ユーティリティテスト - レポートの日時が正しく設定されることを確認
        /// </summary>
        [Test]
        public void SystemRequirementReport_ShouldHaveRecentCheckTime()
        {
            // Arrange
            var beforeTime = System.DateTime.Now.AddSeconds(-1);
            
            // Act
            var report = SystemRequirementChecker.CheckAllRequirements();
            
            // Assert
            var afterTime = System.DateTime.Now.AddSeconds(1);
            
            Assert.IsTrue(report.checkTime >= beforeTime && report.checkTime <= afterTime,
                $"Check time should be recent: {report.checkTime:yyyy-MM-dd HH:mm:ss}");
            
            Debug.Log($"[Test] Check time validation passed: {report.checkTime:yyyy-MM-dd HH:mm:ss}");
        }
        
        /// <summary>
        /// サマリー生成テスト
        /// </summary>
        [Test]
        public void SystemRequirementReport_ShouldHaveMeaningfulSummary()
        {
            // Arrange & Act
            var report = SystemRequirementChecker.CheckAllRequirements();
            
            // Assert
            Assert.IsTrue(!string.IsNullOrWhiteSpace(report.summary), "Summary should not be empty");
            Assert.IsTrue(report.summary.Contains("Total Checks"), "Summary should include total check count");
            Assert.IsTrue(report.summary.Contains("Passed"), "Summary should include passed count");
            Assert.IsTrue(report.summary.Contains("Overall Status"), "Summary should include overall status");
            
            Debug.Log($"[Test] Summary validation passed. Summary length: {report.summary.Length} characters");
        }
        
        #endregion
    }
}
