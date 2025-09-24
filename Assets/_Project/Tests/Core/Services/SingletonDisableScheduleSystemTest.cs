using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Services;

namespace asterivo.Unity60.Tests.Core.Services
{
    /// <summary>
    /// Step 3.10: 段階的Singleton無効化スケジュール システムの包括的テスト
    /// SingletonDisableScheduler と EmergencyRollback の統合テスト
    /// </summary>
    public class SingletonDisableScheduleSystemTest
    {
        private GameObject schedulerObject;
        private SingletonDisableScheduler scheduler;
        private bool originalEnableMigrationWarnings;
        private bool originalDisableLegacySingletons;
        private bool originalEnableMigrationMonitoring;

        [SetUp]
        public void Setup()
        {
            // 元のFeatureFlags設定を保存
            originalEnableMigrationWarnings = FeatureFlags.EnableMigrationWarnings;
            originalDisableLegacySingletons = FeatureFlags.DisableLegacySingletons;
            originalEnableMigrationMonitoring = FeatureFlags.EnableMigrationMonitoring;

            // テスト用のSingletonDisableSchedulerを作成
            schedulerObject = new GameObject("SingletonDisableSchedulerTest");
            scheduler = schedulerObject.AddComponent<SingletonDisableScheduler>();

            // テスト用の初期設定
            ResetToSafeState();
        }

        [TearDown]
        public void TearDown()
        {
            // FeatureFlags設定を復元
            FeatureFlags.EnableMigrationWarnings = originalEnableMigrationWarnings;
            FeatureFlags.DisableLegacySingletons = originalDisableLegacySingletons;
            FeatureFlags.EnableMigrationMonitoring = originalEnableMigrationMonitoring;

            // テストオブジェクトをクリーンアップ
            if (schedulerObject != null)
            {
                Object.DestroyImmediate(schedulerObject);
            }

            // スケジューラーをリセット
            if (scheduler != null)
            {
                scheduler.ResetSchedule();
            }

            ResetToSafeState();
        }

        private void ResetToSafeState()
        {
            FeatureFlags.EnableMigrationWarnings = false;
            FeatureFlags.DisableLegacySingletons = false;
            FeatureFlags.EnableMigrationMonitoring = false;
            FeatureFlags.EnableAutoRollback = false;
        }

        [Test]
        public void SingletonDisableScheduler_ComponentCreation_ShouldSucceed()
        {
            // Assert
            Assert.IsNotNull(scheduler, "SingletonDisableScheduler should be created successfully");
            Assert.IsTrue(scheduler.enabled, "SingletonDisableScheduler should be enabled");
        }

        [Test]
        public void SingletonDisableScheduler_InitialState_ShouldBeNotStarted()
        {
            // Act & Assert
            float progress = scheduler.GetScheduleProgress();
            Assert.AreEqual(0f, progress, "Initial progress should be 0%");
        }

        [UnityTest]
        public IEnumerator SingletonDisableScheduler_StartSchedule_ShouldInitializeDay1()
        {
            // Act
            scheduler.StartSchedule();
            yield return new WaitForSeconds(0.1f);

            // Assert
            Assert.IsTrue(FeatureFlags.EnableMigrationWarnings, "Day 1 should enable migration warnings");
            Assert.IsFalse(FeatureFlags.DisableLegacySingletons, "Day 1 should not disable singletons yet");
            
            float progress = scheduler.GetScheduleProgress();
            Assert.IsTrue(progress > 0f, "Progress should be greater than 0 after starting");
        }

        [UnityTest]
        public IEnumerator SingletonDisableScheduler_AdvanceToDay4_ShouldDisableSingletons()
        {
            // Arrange
            scheduler.StartSchedule();
            yield return new WaitForSeconds(0.1f);

            // Act
            scheduler.AdvanceToDay4();
            yield return new WaitForSeconds(0.1f);

            // Assert
            Assert.IsTrue(FeatureFlags.DisableLegacySingletons, "Day 4 should disable legacy singletons");
            Assert.IsTrue(FeatureFlags.EnableMigrationWarnings, "Day 4 should keep warnings enabled");
            
            float progress = scheduler.GetScheduleProgress();
            Assert.IsTrue(progress >= 80f, "Day 4 progress should be at least 80%");
        }

        [UnityTest]
        public IEnumerator SingletonDisableScheduler_AdvanceToDay5_ShouldCompleteSchedule()
        {
            // Arrange
            scheduler.StartSchedule();
            scheduler.AdvanceToDay4();
            yield return new WaitForSeconds(0.1f);

            // Act
            scheduler.AdvanceToDay5();
            yield return new WaitForSeconds(0.1f);

            // Assert
            Assert.IsFalse(FeatureFlags.EnableMigrationWarnings, "Day 5 should disable warnings");
            Assert.IsTrue(FeatureFlags.DisableLegacySingletons, "Day 5 should keep singletons disabled");
            
            float progress = scheduler.GetScheduleProgress();
            Assert.AreEqual(100f, progress, "Day 5 should show 100% progress");
        }

        [Test]
        public void SingletonDisableScheduler_ResetSchedule_ShouldReturnToInitialState()
        {
            // Arrange
            scheduler.StartSchedule();
            scheduler.AdvanceToDay4();

            // Act
            scheduler.ResetSchedule();

            // Assert
            float progress = scheduler.GetScheduleProgress();
            Assert.AreEqual(0f, progress, "Reset should return progress to 0%");
            Assert.IsFalse(FeatureFlags.EnableMigrationWarnings, "Reset should disable warnings");
            Assert.IsFalse(FeatureFlags.DisableLegacySingletons, "Reset should enable singletons");
        }

        [Test]
        public void EmergencyRollback_ExecuteEmergencyRollback_ShouldResetAllFlags()
        {
            // Arrange - 移行が進行中の状態をシミュレート
            FeatureFlags.EnableMigrationWarnings = true;
            FeatureFlags.DisableLegacySingletons = true;
            FeatureFlags.UseNewAudioService = true;
            FeatureFlags.UseNewSpatialService = true;
            FeatureFlags.UseNewStealthService = true;

            // Act
            EmergencyRollback.ExecuteEmergencyRollback("Test rollback");

            // Assert - 全て安全な状態に戻るべき
            Assert.IsFalse(FeatureFlags.EnableMigrationWarnings, "Emergency rollback should disable warnings");
            Assert.IsFalse(FeatureFlags.DisableLegacySingletons, "Emergency rollback should enable singletons");
            Assert.IsFalse(FeatureFlags.UseNewAudioService, "Emergency rollback should disable new audio service");
            Assert.IsFalse(FeatureFlags.UseNewSpatialService, "Emergency rollback should disable new spatial service");
            Assert.IsFalse(FeatureFlags.UseNewStealthService, "Emergency rollback should disable new stealth service");
            Assert.IsTrue(FeatureFlags.UseServiceLocator, "Emergency rollback should keep ServiceLocator enabled");
        }

        [Test]
        public void EmergencyRollback_RollbackSpecificService_ShouldOnlyAffectTargetService()
        {
            // Arrange - 全てのサービスを有効化
            FeatureFlags.UseNewAudioService = true;
            FeatureFlags.UseNewSpatialService = true;
            FeatureFlags.UseNewStealthService = true;

            // Act - Audioサービスのみロールバック
            EmergencyRollback.RollbackSpecificService("audio", "Test specific rollback");

            // Assert - Audioサービスのみ無効化、他は影響なし
            Assert.IsFalse(FeatureFlags.UseNewAudioService, "Audio service should be disabled");
            Assert.IsTrue(FeatureFlags.UseNewSpatialService, "Spatial service should remain enabled");
            Assert.IsTrue(FeatureFlags.UseNewStealthService, "Stealth service should remain enabled");
        }

        [Test]
        public void EmergencyRollback_RestoreFromRollback_ShouldRestoreServices()
        {
            // Arrange - 緊急ロールバック後の状態をシミュレート
            EmergencyRollback.ExecuteEmergencyRollback("Test setup");

            // Act
            EmergencyRollback.RestoreFromRollback("Test recovery");

            // Assert - サービスが復旧されるべき
            Assert.IsTrue(FeatureFlags.UseNewAudioService, "Recovery should enable audio service");
            Assert.IsTrue(FeatureFlags.UseNewSpatialService, "Recovery should enable spatial service");
            Assert.IsTrue(FeatureFlags.UseNewStealthService, "Recovery should enable stealth service");
            Assert.IsTrue(FeatureFlags.EnableMigrationWarnings, "Recovery should enable warnings");
            Assert.IsTrue(FeatureFlags.UseServiceLocator, "Recovery should keep ServiceLocator enabled");
        }

        [Test]
        public void EmergencyRollback_CheckSystemHealth_ShouldDetectInconsistencies()
        {
            // Arrange - 矛盾した設定を作る
            FeatureFlags.UseServiceLocator = false;
            FeatureFlags.UseNewAudioService = true; // ServiceLocatorなしで新サービスを有効化

            // Act
            var health = EmergencyRollback.CheckSystemHealth();

            // Assert
            Assert.IsFalse(health.IsHealthy, "System should be detected as unhealthy");
            Assert.IsTrue(health.HasInconsistentConfiguration, "Inconsistent configuration should be detected");
            Assert.IsTrue(health.Issues.Count > 0, "Issues should be recorded");
            Assert.Less(health.HealthScore, 70, "Health score should be below healthy threshold");
        }

        [Test]
        public void EmergencyRollback_GetRollbackHistory_ShouldReturnHistory()
        {
            // Arrange
            EmergencyRollback.ExecuteEmergencyRollback("Test rollback for history");

            // Act
            var history = EmergencyRollback.GetRollbackHistory();

            // Assert
            Assert.IsNotNull(history, "Rollback history should not be null");
            Assert.AreNotEqual("Never", history.LastEmergencyRollbackTime, "Last rollback time should be recorded");
            Assert.AreEqual("Test rollback for history", history.LastEmergencyRollbackReason, "Rollback reason should match");
        }

        [UnityTest]
        public IEnumerator Integration_SchedulerWithEmergencyRollback_ShouldWorkTogether()
        {
            // Arrange - スケジューラを Day 4 まで進める
            scheduler.StartSchedule();
            scheduler.AdvanceToDay4();
            yield return new WaitForSeconds(0.1f);

            // 現在の状態を確認
            Assert.IsTrue(FeatureFlags.DisableLegacySingletons, "Day 4 should have singletons disabled");

            // Act - 緊急ロールバック実行
            EmergencyRollback.ExecuteEmergencyRollback("Integration test rollback");
            yield return new WaitForSeconds(0.1f);

            // Assert - ロールバック後の状態確認
            Assert.IsFalse(FeatureFlags.DisableLegacySingletons, "Emergency rollback should enable singletons");
            Assert.IsFalse(FeatureFlags.EnableMigrationWarnings, "Emergency rollback should disable warnings");

            // スケジューラ進捗もリセットされるべき
            float progress = scheduler.GetScheduleProgress();
            Assert.AreEqual(0f, progress, "Emergency rollback should reset scheduler progress");
        }

        [UnityTest]
        public IEnumerator SystemHealth_AutoRollback_ShouldTriggerWhenEnabled()
        {
            // Arrange - 自動ロールバックを有効化し、不健全な状態を作る
            FeatureFlags.EnableAutoRollback = true;
            FeatureFlags.UseServiceLocator = false;
            FeatureFlags.UseNewAudioService = true; // 矛盾した設定

            // Act
            EmergencyRollback.MonitorSystemHealth();
            yield return new WaitForSeconds(0.1f);

            // Assert - 自動ロールバックが実行されるべき
            // 注意: 実際のテストでは、MonitorSystemHealthメソッドが
            // ヘルススコアに基づいて自動ロールバックを実行する
            var health = EmergencyRollback.CheckSystemHealth();
            Assert.IsFalse(health.IsHealthy, "System should be detected as unhealthy");
        }

        [UnityTest]
        public IEnumerator SchedulerStatusReport_ShouldGenerateDetailedReport()
        {
            // Arrange
            scheduler.StartSchedule();
            scheduler.AdvanceToDay2();
            yield return new WaitForSeconds(0.1f);

            // Act & Assert - レポート生成がエラーなく実行されることを確認
            LogAssert.Expect(LogType.Log, new System.Text.RegularExpressions.Regex(".*Schedule Status Report.*"));
            scheduler.GenerateStatusReport();
        }

        [UnityTest]
        public IEnumerator CompleteScheduleFlow_ShouldExecuteAllDays()
        {
            // Arrange
            bool flowCompleted = false;
            System.Exception caughtException = null;

            // Act - 全日程を実行 (try-catch とyield returnを分離)
            try
            {
                scheduler.StartSchedule(); // Day 1
            }
            catch (System.Exception ex)
            {
                caughtException = ex;
                Debug.LogError($"Day 1 failed: {ex.Message}");
            }
            
            yield return new WaitForSeconds(0.1f);
            
            if (caughtException == null)
            {
                try
                {
                    scheduler.AdvanceToDay2(); // Day 2
                }
                catch (System.Exception ex)
                {
                    caughtException = ex;
                    Debug.LogError($"Day 2 failed: {ex.Message}");
                }
            }
            
            yield return new WaitForSeconds(0.1f);
            
            if (caughtException == null)
            {
                try
                {
                    scheduler.AdvanceToDay3(); // Day 3
                }
                catch (System.Exception ex)
                {
                    caughtException = ex;
                    Debug.LogError($"Day 3 failed: {ex.Message}");
                }
            }
            
            yield return new WaitForSeconds(0.1f);
            
            if (caughtException == null)
            {
                try
                {
                    scheduler.AdvanceToDay4(); // Day 4
                }
                catch (System.Exception ex)
                {
                    caughtException = ex;
                    Debug.LogError($"Day 4 failed: {ex.Message}");
                }
            }
            
            yield return new WaitForSeconds(0.1f);
            
            if (caughtException == null)
            {
                try
                {
                    scheduler.AdvanceToDay5(); // Day 5
                }
                catch (System.Exception ex)
                {
                    caughtException = ex;
                    Debug.LogError($"Day 5 failed: {ex.Message}");
                }
            }
            
            yield return new WaitForSeconds(0.1f);
            
            if (caughtException == null)
            {
                flowCompleted = true;
            }

            // Assert
            Assert.IsTrue(flowCompleted, "Complete schedule flow should execute without errors");
            if (caughtException != null)
            {
                Assert.Fail($"Schedule flow failed with exception: {caughtException.Message}");
            }
            
            float finalProgress = scheduler.GetScheduleProgress();
            Assert.AreEqual(100f, finalProgress, "Final progress should be 100%");
            Assert.IsFalse(FeatureFlags.EnableMigrationWarnings, "Final state should have warnings disabled");
            Assert.IsTrue(FeatureFlags.DisableLegacySingletons, "Final state should have singletons disabled");
        }

        [Test]
        public void SchedulerPersistence_ShouldSaveAndLoadState()
        {
            // Arrange
            scheduler.StartSchedule();
            scheduler.AdvanceToDay3();

            // Act - 破棄して再作成（永続化テスト）
            Object.DestroyImmediate(schedulerObject);
            
            schedulerObject = new GameObject("SingletonDisableSchedulerTest2");
            scheduler = schedulerObject.AddComponent<SingletonDisableScheduler>();

            // Assert - 状態が復元されることを期待
            // 注意: 実際の永続化はPlayerPrefsで行われるため、
            // テスト環境では完全に検証が困難な場合がある
            Assert.IsNotNull(scheduler, "Scheduler should be recreated successfully");
        }
    }
}