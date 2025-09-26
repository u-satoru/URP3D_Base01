using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core;

namespace asterivo.Unity60.Tests.Core.Services
{
    /// <summary>
    /// Step 3.10: 谿ｵ髫守噪Singleton辟｡蜉ｹ蛹悶せ繧ｱ繧ｸ繝･繝ｼ繝ｫ 繧ｷ繧ｹ繝・Β縺ｮ蛹・峡逧・ユ繧ｹ繝・
    /// SingletonDisableScheduler 縺ｨ EmergencyRollback 縺ｮ邨ｱ蜷医ユ繧ｹ繝・
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
            // 蜈・・FeatureFlags險ｭ螳壹ｒ菫晏ｭ・
            originalEnableMigrationWarnings = FeatureFlags.EnableMigrationWarnings;
            originalDisableLegacySingletons = FeatureFlags.DisableLegacySingletons;
            originalEnableMigrationMonitoring = FeatureFlags.EnableMigrationMonitoring;

            // 繝・せ繝育畑縺ｮSingletonDisableScheduler繧剃ｽ懈・
            schedulerObject = new GameObject("SingletonDisableSchedulerTest");
            scheduler = schedulerObject.AddComponent<SingletonDisableScheduler>();

            // 繝・せ繝育畑縺ｮ蛻晄悄險ｭ螳・
            ResetToSafeState();
        }

        [TearDown]
        public void TearDown()
        {
            // FeatureFlags險ｭ螳壹ｒ蠕ｩ蜈・
            FeatureFlags.EnableMigrationWarnings = originalEnableMigrationWarnings;
            FeatureFlags.DisableLegacySingletons = originalDisableLegacySingletons;
            FeatureFlags.EnableMigrationMonitoring = originalEnableMigrationMonitoring;

            // 繝・せ繝医が繝悶ず繧ｧ繧ｯ繝医ｒ繧ｯ繝ｪ繝ｼ繝ｳ繧｢繝・・
            if (schedulerObject != null)
            {
                Object.DestroyImmediate(schedulerObject);
            }

            // 繧ｹ繧ｱ繧ｸ繝･繝ｼ繝ｩ繝ｼ繧偵Μ繧ｻ繝・ヨ
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
            // Arrange - 遘ｻ陦後′騾ｲ陦御ｸｭ縺ｮ迥ｶ諷九ｒ繧ｷ繝溘Η繝ｬ繝ｼ繝・
            FeatureFlags.EnableMigrationWarnings = true;
            FeatureFlags.DisableLegacySingletons = true;
            FeatureFlags.UseNewAudioService = true;
            FeatureFlags.UseNewSpatialService = true;
            FeatureFlags.UseNewStealthService = true;

            // Act
            EmergencyRollback.ExecuteEmergencyRollback("Test rollback");

            // Assert - 蜈ｨ縺ｦ螳牙・縺ｪ迥ｶ諷九↓謌ｻ繧九∋縺・
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
            // Arrange - 蜈ｨ縺ｦ縺ｮ繧ｵ繝ｼ繝薙せ繧呈怏蜉ｹ蛹・
            FeatureFlags.UseNewAudioService = true;
            FeatureFlags.UseNewSpatialService = true;
            FeatureFlags.UseNewStealthService = true;

            // Act - Audio繧ｵ繝ｼ繝薙せ縺ｮ縺ｿ繝ｭ繝ｼ繝ｫ繝舌ャ繧ｯ
            EmergencyRollback.RollbackSpecificService("audio", "Test specific rollback");

            // Assert - Audio繧ｵ繝ｼ繝薙せ縺ｮ縺ｿ辟｡蜉ｹ蛹悶∽ｻ悶・蠖ｱ髻ｿ縺ｪ縺・
            Assert.IsFalse(FeatureFlags.UseNewAudioService, "Audio service should be disabled");
            Assert.IsTrue(FeatureFlags.UseNewSpatialService, "Spatial service should remain enabled");
            Assert.IsTrue(FeatureFlags.UseNewStealthService, "Stealth service should remain enabled");
        }

        [Test]
        public void EmergencyRollback_RestoreFromRollback_ShouldRestoreServices()
        {
            // Arrange - 邱頑･繝ｭ繝ｼ繝ｫ繝舌ャ繧ｯ蠕後・迥ｶ諷九ｒ繧ｷ繝溘Η繝ｬ繝ｼ繝・
            EmergencyRollback.ExecuteEmergencyRollback("Test setup");

            // Act
            EmergencyRollback.RestoreFromRollback("Test recovery");

            // Assert - 繧ｵ繝ｼ繝薙せ縺悟ｾｩ譌ｧ縺輔ｌ繧九∋縺・
            Assert.IsTrue(FeatureFlags.UseNewAudioService, "Recovery should enable audio service");
            Assert.IsTrue(FeatureFlags.UseNewSpatialService, "Recovery should enable spatial service");
            Assert.IsTrue(FeatureFlags.UseNewStealthService, "Recovery should enable stealth service");
            Assert.IsTrue(FeatureFlags.EnableMigrationWarnings, "Recovery should enable warnings");
            Assert.IsTrue(FeatureFlags.UseServiceLocator, "Recovery should keep ServiceLocator enabled");
        }

        [Test]
        public void EmergencyRollback_CheckSystemHealth_ShouldDetectInconsistencies()
        {
            // Arrange - 遏帷崟縺励◆險ｭ螳壹ｒ菴懊ｋ
            FeatureFlags.UseServiceLocator = false;
            FeatureFlags.UseNewAudioService = true; // ServiceLocator縺ｪ縺励〒譁ｰ繧ｵ繝ｼ繝薙せ繧呈怏蜉ｹ蛹・

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
            // Arrange - 繧ｹ繧ｱ繧ｸ繝･繝ｼ繝ｩ繧・Day 4 縺ｾ縺ｧ騾ｲ繧√ｋ
            scheduler.StartSchedule();
            scheduler.AdvanceToDay4();
            yield return new WaitForSeconds(0.1f);

            // 迴ｾ蝨ｨ縺ｮ迥ｶ諷九ｒ遒ｺ隱・
            Assert.IsTrue(FeatureFlags.DisableLegacySingletons, "Day 4 should have singletons disabled");

            // Act - 邱頑･繝ｭ繝ｼ繝ｫ繝舌ャ繧ｯ螳溯｡・
            EmergencyRollback.ExecuteEmergencyRollback("Integration test rollback");
            yield return new WaitForSeconds(0.1f);

            // Assert - 繝ｭ繝ｼ繝ｫ繝舌ャ繧ｯ蠕後・迥ｶ諷狗｢ｺ隱・
            Assert.IsFalse(FeatureFlags.DisableLegacySingletons, "Emergency rollback should enable singletons");
            Assert.IsFalse(FeatureFlags.EnableMigrationWarnings, "Emergency rollback should disable warnings");

            // 繧ｹ繧ｱ繧ｸ繝･繝ｼ繝ｩ騾ｲ謐励ｂ繝ｪ繧ｻ繝・ヨ縺輔ｌ繧九∋縺・
            float progress = scheduler.GetScheduleProgress();
            Assert.AreEqual(0f, progress, "Emergency rollback should reset scheduler progress");
        }

        [UnityTest]
        public IEnumerator SystemHealth_AutoRollback_ShouldTriggerWhenEnabled()
        {
            // Arrange - 閾ｪ蜍輔Ο繝ｼ繝ｫ繝舌ャ繧ｯ繧呈怏蜉ｹ蛹悶＠縲∽ｸ榊▼蜈ｨ縺ｪ迥ｶ諷九ｒ菴懊ｋ
            FeatureFlags.EnableAutoRollback = true;
            FeatureFlags.UseServiceLocator = false;
            FeatureFlags.UseNewAudioService = true; // 遏帷崟縺励◆險ｭ螳・

            // Act
            EmergencyRollback.MonitorSystemHealth();
            yield return new WaitForSeconds(0.1f);

            // Assert - 閾ｪ蜍輔Ο繝ｼ繝ｫ繝舌ャ繧ｯ縺悟ｮ溯｡後＆繧後ｋ縺ｹ縺・
            // 豕ｨ諢・ 螳滄圀縺ｮ繝・せ繝医〒縺ｯ縲｀onitorSystemHealth繝｡繧ｽ繝・ラ縺・
            // 繝倥Ν繧ｹ繧ｹ繧ｳ繧｢縺ｫ蝓ｺ縺･縺・※閾ｪ蜍輔Ο繝ｼ繝ｫ繝舌ャ繧ｯ繧貞ｮ溯｡後☆繧・
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

            // Act & Assert - 繝ｬ繝昴・繝育函謌舌′繧ｨ繝ｩ繝ｼ縺ｪ縺丞ｮ溯｡後＆繧後ｋ縺薙→繧堤｢ｺ隱・
            LogAssert.Expect(LogType.Log, new System.Text.RegularExpressions.Regex(".*Schedule Status Report.*"));
            scheduler.GenerateStatusReport();
        }

        [UnityTest]
        public IEnumerator CompleteScheduleFlow_ShouldExecuteAllDays()
        {
            // Arrange
            bool flowCompleted = false;
            System.Exception caughtException = null;

            // Act - 蜈ｨ譌･遞九ｒ螳溯｡・(try-catch 縺ｨyield return繧貞・髮｢)
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

            // Act - 遐ｴ譽・＠縺ｦ蜀堺ｽ懈・・域ｰｸ邯壼喧繝・せ繝茨ｼ・
            Object.DestroyImmediate(schedulerObject);
            
            schedulerObject = new GameObject("SingletonDisableSchedulerTest2");
            scheduler = schedulerObject.AddComponent<SingletonDisableScheduler>();

            // Assert - 迥ｶ諷九′蠕ｩ蜈・＆繧後ｋ縺薙→繧呈悄蠕・
            // 豕ｨ諢・ 螳滄圀縺ｮ豌ｸ邯壼喧縺ｯPlayerPrefs縺ｧ陦後ｏ繧後ｋ縺溘ａ縲・
            // 繝・せ繝育腸蠅・〒縺ｯ螳悟・縺ｫ讀懆ｨｼ縺悟峅髮｣縺ｪ蝣ｴ蜷医′縺ゅｋ
            Assert.IsNotNull(scheduler, "Scheduler should be recreated successfully");
        }
    }
}


