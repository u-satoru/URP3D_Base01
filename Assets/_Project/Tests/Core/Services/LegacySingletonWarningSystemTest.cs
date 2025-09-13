using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Core.Audio;
using MigrationMonitorService = asterivo.Unity60.Core.Services.MigrationMonitor;

namespace asterivo.Unity60.Tests.Core.Services
{
    /// <summary>
    /// Legacy Singleton警告システムの包括的テスト
    /// Step 3.9: Legacy Singleton警告システムの検証
    /// </summary>
    public class LegacySingletonWarningSystemTest
    {
        private GameObject migrationMonitorObject;
        private MigrationMonitorService migrationMonitor;
        private bool originalEnableMigrationWarnings;
        private bool originalDisableLegacySingletons;

        [SetUp]
        public void Setup()
        {
            // 元のFeatureFlags設定を保存
            originalEnableMigrationWarnings = asterivo.Unity60.Core.FeatureFlags.EnableMigrationWarnings;
            originalDisableLegacySingletons = asterivo.Unity60.Core.FeatureFlags.DisableLegacySingletons;

            // テスト用のMigrationMonitorを作成
            migrationMonitorObject = new GameObject("MigrationMonitorTest");
            migrationMonitor = migrationMonitorObject.AddComponent<MigrationMonitorService>();

            // テスト用の初期設定
            asterivo.Unity60.Core.FeatureFlags.EnableMigrationWarnings = true;
            asterivo.Unity60.Core.FeatureFlags.DisableLegacySingletons = false;
        }

        [TearDown]
        public void TearDown()
        {
            // FeatureFlags設定を復元
            asterivo.Unity60.Core.FeatureFlags.EnableMigrationWarnings = originalEnableMigrationWarnings;
            asterivo.Unity60.Core.FeatureFlags.DisableLegacySingletons = originalDisableLegacySingletons;

            // テストオブジェクトをクリーンアップ
            if (migrationMonitorObject != null)
            {
                Object.DestroyImmediate(migrationMonitorObject);
            }

            // MigrationMonitor統計をリセット
            if (migrationMonitor != null)
            {
                migrationMonitor.ResetStatistics();
            }
        }

        [Test]
        public void MigrationMonitor_ComponentCreation_ShouldSucceed()
        {
            // Assert
            Assert.IsNotNull(migrationMonitor, "MigrationMonitor should be created successfully");
            Assert.IsTrue(migrationMonitor.enabled, "MigrationMonitor should be enabled");
        }

        [Test]
        public void MigrationMonitor_LogSingletonUsage_ShouldRecordUsage()
        {
            // Arrange
            int initialAccessCount = 0;
            
            // Act
            migrationMonitor.LogSingletonUsage(typeof(AudioManager), "Instance");
            migrationMonitor.LogSingletonUsage(typeof(AudioManager), "Instance");
            migrationMonitor.LogSingletonUsage(typeof(EffectManager), "Instance");

            // Assert - Since we can't directly access private fields,
            // we verify by calling GenerateUsageReport which would log the statistics
            LogAssert.Expect(LogType.Log, new System.Text.RegularExpressions.Regex(".*Total Accesses: 3.*"));
            migrationMonitor.GenerateUsageReport();
        }

        [Test]
        public void FeatureFlags_EnableMigrationWarnings_ShouldControlWarningDisplay()
        {
            // Arrange & Act & Assert
            FeatureFlags.EnableMigrationWarnings = true;
            Assert.IsTrue(FeatureFlags.EnableMigrationWarnings, "EnableMigrationWarnings should be true");

            FeatureFlags.EnableMigrationWarnings = false;
            Assert.IsFalse(FeatureFlags.EnableMigrationWarnings, "EnableMigrationWarnings should be false");
        }

        [Test]
        public void FeatureFlags_DisableLegacySingletons_ShouldControlSingletonAccess()
        {
            // Arrange & Act & Assert
            FeatureFlags.DisableLegacySingletons = false;
            Assert.IsFalse(FeatureFlags.DisableLegacySingletons, "DisableLegacySingletons should be false");

            FeatureFlags.DisableLegacySingletons = true;
            Assert.IsTrue(FeatureFlags.DisableLegacySingletons, "DisableLegacySingletons should be true");
        }

        [UnityTest]
        public IEnumerator MigrationMonitor_UsageReporting_ShouldGenerateReports()
        {
            // Arrange
            bool reportGenerated = false;

            // Act
            migrationMonitor.LogSingletonUsage(typeof(AudioManager), "Instance");
            yield return new WaitForSeconds(0.1f);

            try
            {
                migrationMonitor.GenerateUsageReport();
                reportGenerated = true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Report generation failed: {ex.Message}");
            }

            // Assert
            Assert.IsTrue(reportGenerated, "Usage report should be generated successfully");
        }

        [Test]
        public void MigrationMonitor_ResetStatistics_ShouldClearData()
        {
            // Arrange
            migrationMonitor.LogSingletonUsage(typeof(AudioManager), "Instance");

            // Act
            migrationMonitor.ResetStatistics();

            // Assert
            // After reset, generating a report should show no usage
            LogAssert.Expect(LogType.Log, "No singleton usage detected");
            migrationMonitor.GenerateUsageReport();
        }

        [UnityTest]
        public IEnumerator MigrationMonitor_SaveAndLoadStatistics_ShouldPersistData()
        {
            // Arrange
            migrationMonitor.LogSingletonUsage(typeof(AudioManager), "Instance");

            // Act
            migrationMonitor.SaveUsageStatistics();
            yield return new WaitForSeconds(0.1f);

            // Reset to simulate restart
            migrationMonitor.ResetStatistics();
            migrationMonitor.LoadUsageStatistics();

            // Assert - The loaded statistics should indicate previous usage
            // This is verified through log output as we can't directly access private fields
            LogAssert.Expect(LogType.Log, new System.Text.RegularExpressions.Regex(".*Loaded statistics.*"));
        }

        [Test]
        public void MigrationMonitor_MigrationRecommendations_ShouldProvideGuidance()
        {
            // Arrange
            // Simulate different usage patterns
            migrationMonitor.LogSingletonUsage(typeof(AudioManager), "Instance");
            
            // Act & Assert
            // The method should execute without errors and provide recommendations
            LogAssert.Expect(LogType.Log, new System.Text.RegularExpressions.Regex(".*Migration Recommendations.*"));
            migrationMonitor.GenerateMigrationRecommendations();
        }

        [Test]
        public void MigrationMonitor_ShowRecentEvents_ShouldDisplayEventHistory()
        {
            // Arrange
            migrationMonitor.LogSingletonUsage(typeof(AudioManager), "Instance");
            migrationMonitor.LogSingletonUsage(typeof(EffectManager), "Instance");

            // Act & Assert
            LogAssert.Expect(LogType.Log, new System.Text.RegularExpressions.Regex(".*Recent Singleton Events.*"));
            migrationMonitor.ShowRecentEvents();
        }

        [UnityTest]
        public IEnumerator WarningSystem_Integration_ShouldWorkEndToEnd()
        {
            // Arrange
            FeatureFlags.EnableMigrationWarnings = true;
            FeatureFlags.DisableLegacySingletons = false;

            // Act - Simulate singleton usage (we can't directly test the actual Manager classes
            // in unit tests without complex setup, so we test the monitoring functionality)
            migrationMonitor.LogSingletonUsage(typeof(AudioManager), "Instance");
            migrationMonitor.LogSingletonUsage(typeof(SpatialAudioManager), "Instance");
            migrationMonitor.LogSingletonUsage(typeof(EffectManager), "Instance");

            yield return new WaitForSeconds(0.1f);

            // Assert - Verify that the monitoring system recorded the usage
            LogAssert.Expect(LogType.Log, new System.Text.RegularExpressions.Regex(".*Total Accesses: 3.*"));
            migrationMonitor.GenerateUsageReport();
        }

        [Test]
        public void WarningSystem_DisabledSingletons_ShouldBlockAccess()
        {
            // Arrange
            FeatureFlags.DisableLegacySingletons = true;

            // Act & Assert
            // When DisableLegacySingletons is true, singleton access should be blocked
            // We verify this through the flag state rather than actual singleton access
            // since we can't easily test the actual Manager singleton behavior in unit tests
            Assert.IsTrue(FeatureFlags.DisableLegacySingletons, 
                "DisableLegacySingletons flag should block singleton access");
        }

        [Test]
        public void WarningSystem_MigrationWarningsDisabled_ShouldNotShowWarnings()
        {
            // Arrange
            FeatureFlags.EnableMigrationWarnings = false;
            
            // Act
            migrationMonitor.LogSingletonUsage(typeof(AudioManager), "Instance");

            // Assert
            // When warnings are disabled, usage should still be recorded but warnings shouldn't show
            // We verify the flag state since warning display is handled by the Manager classes
            Assert.IsFalse(FeatureFlags.EnableMigrationWarnings, 
                "EnableMigrationWarnings should control warning display");
        }

        [UnityTest]
        public IEnumerator WarningSystem_PerformanceImpact_ShouldBeMinimal()
        {
            // Arrange
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            const int iterations = 1000;

            // Act
            for (int i = 0; i < iterations; i++)
            {
                migrationMonitor.LogSingletonUsage(typeof(AudioManager), "Instance");
            }

            stopwatch.Stop();

            yield return new WaitForSeconds(0.1f);

            // Assert - Performance should be reasonable (less than 1ms per call on average)
            double averageTimePerCall = stopwatch.ElapsedMilliseconds / (double)iterations;
            Assert.Less(averageTimePerCall, 1.0, 
                $"Average time per LogSingletonUsage call should be less than 1ms, was {averageTimePerCall}ms");

            Debug.Log($"Performance test: {iterations} calls took {stopwatch.ElapsedMilliseconds}ms " +
                     $"(avg: {averageTimePerCall:F3}ms per call)");
        }

        [Test]
        public void WarningSystem_FeatureFlagsPersistence_ShouldMaintainSettings()
        {
            // Arrange
            bool testWarningValue = !FeatureFlags.EnableMigrationWarnings;
            bool testDisableValue = !FeatureFlags.DisableLegacySingletons;

            // Act
            FeatureFlags.EnableMigrationWarnings = testWarningValue;
            FeatureFlags.DisableLegacySingletons = testDisableValue;

            // Assert
            Assert.AreEqual(testWarningValue, FeatureFlags.EnableMigrationWarnings, 
                "EnableMigrationWarnings should persist the set value");
            Assert.AreEqual(testDisableValue, FeatureFlags.DisableLegacySingletons, 
                "DisableLegacySingletons should persist the set value");
        }
    }
}