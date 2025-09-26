using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core;

namespace asterivo.Unity60.Tests.Core.Services
{
    /// <summary>
    /// Step 3.11: 譛邨ゅけ繝ｪ繝ｼ繝ｳ繧｢繝・・ 繧ｷ繧ｹ繝・Β縺ｮ繝・せ繝・
    /// SingletonCodeRemover 縺ｮ讖溯・繝・せ繝・
    /// </summary>
    public class SingletonCodeRemovalTest
    {
        private GameObject removerObject;
        private SingletonCodeRemover remover;
        private bool originalCleanupState;

        [SetUp]
        public void Setup()
        {
            // 繝・せ繝育畑縺ｮSingletonCodeRemover繧剃ｽ懈・
            removerObject = new GameObject("SingletonCodeRemoverTest");
            remover = removerObject.AddComponent<SingletonCodeRemover>();

            // 蜈・・迥ｶ諷九ｒ菫晏ｭ・
            originalCleanupState = PlayerPrefs.GetInt("SingletonCodeRemover_Completed", 0) == 1;
            
            // 繝・せ繝育畑縺ｫ繝ｪ繧ｻ繝・ヨ
            remover.ResetCleanupState();
        }

        [TearDown]
        public void TearDown()
        {
            // 蜈・・迥ｶ諷九ｒ蠕ｩ蜈・
            PlayerPrefs.SetInt("SingletonCodeRemover_Completed", originalCleanupState ? 1 : 0);
            PlayerPrefs.Save();

            // 繝・せ繝医が繝悶ず繧ｧ繧ｯ繝医ｒ繧ｯ繝ｪ繝ｼ繝ｳ繧｢繝・・
            if (removerObject != null)
            {
                Object.DestroyImmediate(removerObject);
            }
        }

        [Test]
        public void SingletonCodeRemover_ComponentCreation_ShouldSucceed()
        {
            // Assert
            Assert.IsNotNull(remover, "SingletonCodeRemover should be created successfully");
            Assert.IsTrue(remover.enabled, "SingletonCodeRemover should be enabled");
        }

        [UnityTest]
        public IEnumerator SingletonCodeRemover_ExecuteCleanup_ShouldRequireConfirmation()
        {
            // Act
            remover.ExecuteCleanup();
            yield return new WaitForSeconds(0.1f);

            // Assert - 繝・ヵ繧ｩ繝ｫ繝医〒縺ｯ遒ｺ隱阪′蠢・ｦ√〒縲∝ｮ滄圀縺ｮ繧ｯ繝ｪ繝ｼ繝ｳ繧｢繝・・縺ｯ螳溯｡後＆繧後↑縺・
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex(".*Manual confirmation required.*"));
        }

        [UnityTest]
        public IEnumerator SingletonCodeRemover_ExecuteCleanupConfirmed_ShouldCompleteCleanup()
        {
            // Act
            remover.ExecuteCleanupConfirmed();
            yield return new WaitForSeconds(0.1f);

            // Assert - 繧ｯ繝ｪ繝ｼ繝ｳ繧｢繝・・縺悟ｮ御ｺ・☆繧九％縺ｨ繧堤｢ｺ隱・
            LogAssert.Expect(LogType.Log, new System.Text.RegularExpressions.Regex(".*Singleton Code Removal Completed.*"));
        }

        [Test]
        public void SingletonCodeRemover_GenerateCleanupReport_ShouldDisplayStatus()
        {
            // Act & Assert
            LogAssert.Expect(LogType.Log, new System.Text.RegularExpressions.Regex(".*Cleanup Status Report.*"));
            remover.GenerateCleanupReport();
        }

        [Test]
        public void SingletonCodeRemover_ShowManualCleanupGuide_ShouldDisplayGuide()
        {
            // Act & Assert
            LogAssert.Expect(LogType.Log, new System.Text.RegularExpressions.Regex(".*Manual Cleanup Guide.*"));
            remover.ShowManualCleanupGuide();
        }

        [UnityTest]
        public IEnumerator SingletonCodeRemover_CleanupValidation_ShouldVerifySystemState()
        {
            // Arrange - 繧ｷ繧ｹ繝・Β繧貞ｮ悟・遘ｻ陦檎憾諷九↓繧ｻ繝・ヨ繧｢繝・・
            asterivo.Unity60.Core.FeatureFlags.UseServiceLocator = true;
            asterivo.Unity60.Core.FeatureFlags.DisableLegacySingletons = true;
            asterivo.Unity60.Core.FeatureFlags.EnableMigrationWarnings = false;
            asterivo.Unity60.Core.FeatureFlags.UseNewAudioService = true;
            asterivo.Unity60.Core.FeatureFlags.UseNewSpatialService = true;
            asterivo.Unity60.Core.FeatureFlags.UseNewStealthService = true;

            // Act
            remover.ExecuteCleanupConfirmed();
            yield return new WaitForSeconds(0.1f);

            // Assert - 繧ｷ繧ｹ繝・Β縺悟▼蜈ｨ縺ｪ迥ｶ諷九↓縺ｪ繧九％縺ｨ繧堤｢ｺ隱・
            var healthStatus = EmergencyRollback.CheckSystemHealth();
            Assert.IsTrue(healthStatus.IsHealthy || healthStatus.HealthScore >= 70, 
                $"System should be healthy after cleanup, score: {healthStatus.HealthScore}");
        }

        [UnityTest]
        public IEnumerator SingletonCodeRemover_BackupCreation_ShouldRecordBackup()
        {
            // Act
            remover.ExecuteCleanupConfirmed();
            yield return new WaitForSeconds(0.1f);

            // Assert - 繝舌ャ繧ｯ繧｢繝・・險倬鹸縺御ｽ懈・縺輔ｌ繧九％縺ｨ繧堤｢ｺ隱・
            string backupTimestamp = PlayerPrefs.GetString("CleanupBackup_Timestamp", "");
            Assert.IsNotEmpty(backupTimestamp, "Backup timestamp should be recorded");

            string backupInfo = PlayerPrefs.GetString("CleanupBackup_Info", "");
            Assert.IsNotEmpty(backupInfo, "Backup info should be recorded");
        }

        [UnityTest]
        public IEnumerator SingletonCodeRemover_CleanupRecords_ShouldTrackAllManagers()
        {
            // Act
            remover.ExecuteCleanupConfirmed();
            yield return new WaitForSeconds(0.1f);

            // Assert - 蜷Мanager縺ｮ繧ｯ繝ｪ繝ｼ繝ｳ繧｢繝・・險倬鹸縺御ｽ懈・縺輔ｌ繧九％縺ｨ繧堤｢ｺ隱・
            string[] expectedManagers = { "AudioManager", "SpatialAudioManager", "EffectManager" };
            
            foreach (var manager in expectedManagers)
            {
                string recordKey = $"CleanupRecord_{manager}";
                string record = PlayerPrefs.GetString(recordKey, "");
                Assert.IsNotEmpty(record, $"Cleanup record for {manager} should exist");
                Assert.IsTrue(record.Contains("Removed"), $"Record for {manager} should contain removal actions");
                Assert.IsTrue(record.Contains("Kept"), $"Record for {manager} should contain kept elements");
            }
        }

        [Test]
        public void SingletonCodeRemover_ResetCleanupState_ShouldClearState()
        {
            // Arrange - 螳御ｺ・憾諷九ｒ繧ｷ繝溘Η繝ｬ繝ｼ繝・
            PlayerPrefs.SetInt("SingletonCodeRemover_Completed", 1);
            PlayerPrefs.SetString("SingletonCodeRemover_LastTime", "2024-01-01 12:00:00");

            // Act
            remover.ResetCleanupState();

            // Assert - 迥ｶ諷九′繝ｪ繧ｻ繝・ヨ縺輔ｌ繧九％縺ｨ繧堤｢ｺ隱・
            bool completed = PlayerPrefs.GetInt("SingletonCodeRemover_Completed", 0) == 1;
            string lastTime = PlayerPrefs.GetString("SingletonCodeRemover_LastTime", "");
            
            Assert.IsFalse(completed, "Cleanup completed flag should be reset");
            Assert.IsEmpty(lastTime, "Last cleanup time should be cleared");
        }

        [UnityTest]
        public IEnumerator IntegrationTest_Day5ToCleanup_ShouldWorkSeamlessly()
        {
            // Arrange - Day 5螳御ｺ・憾諷九ｒ繧ｷ繝溘Η繝ｬ繝ｼ繝・
            FeatureFlags.EnableMigrationWarnings = false;
            FeatureFlags.DisableLegacySingletons = true;
            FeatureFlags.UseServiceLocator = true;

            // SingletonDisableScheduler繧ゆｽ懈・
            var schedulerObj = new GameObject("TestScheduler");
            var scheduler = schedulerObj.AddComponent<SingletonDisableScheduler>();
            
            try
            {
                scheduler.StartSchedule();
                scheduler.AdvanceToDay5();
                yield return new WaitForSeconds(0.1f);

                // Act - 繧ｯ繝ｪ繝ｼ繝ｳ繧｢繝・・螳溯｡・
                remover.ExecuteCleanupConfirmed();
                yield return new WaitForSeconds(0.1f);

                // Assert - 邨ｱ蜷育噪縺ｫ蜍穂ｽ懊☆繧九％縺ｨ繧堤｢ｺ隱・
                Assert.AreEqual(100f, scheduler.GetScheduleProgress(), "Scheduler should show 100% progress");
                
                LogAssert.Expect(LogType.Log, new System.Text.RegularExpressions.Regex(".*Cleanup completed.*"));
            }
            finally
            {
                if (schedulerObj != null)
                {
                    Object.DestroyImmediate(schedulerObj);
                }
            }
        }

        [UnityTest]
        public IEnumerator SingletonCodeRemover_FinalValidation_ShouldRunMigrationValidator()
        {
            // Arrange - MigrationValidator繧剃ｽ懈・
            var validatorObj = new GameObject("TestValidator");
            var validator = validatorObj.AddComponent<MigrationValidator>();
            
            try
            {
                // Act
                remover.ExecuteCleanupConfirmed();
                yield return new WaitForSeconds(0.1f);

                // Assert - MigrationValidator縺悟ｮ溯｡後＆繧後ｋ縺薙→繧堤｢ｺ隱・
                LogAssert.Expect(LogType.Log, new System.Text.RegularExpressions.Regex(".*Migration validation completed.*"));
            }
            finally
            {
                if (validatorObj != null)
                {
                    Object.DestroyImmediate(validatorObj);
                }
            }
        }

        [UnityTest]
        public IEnumerator SingletonCodeRemover_FeatureFlagsValidation_ShouldCheckFinalState()
        {
            // Arrange - 螳悟・遘ｻ陦檎憾諷九ｒ繧ｻ繝・ヨ繧｢繝・・
            FeatureFlags.UseServiceLocator = true;
            FeatureFlags.DisableLegacySingletons = true;
            FeatureFlags.EnableMigrationWarnings = false;
            FeatureFlags.UseNewAudioService = true;
            FeatureFlags.UseNewSpatialService = true;
            FeatureFlags.UseNewStealthService = true;

            // Act
            remover.ExecuteCleanupConfirmed();
            yield return new WaitForSeconds(0.1f);

            // Assert - FeatureFlags縺ｮ譛邨ら憾諷九′豁｣縺励＞縺薙→繧堤｢ｺ隱・
            LogAssert.Expect(LogType.Log, new System.Text.RegularExpressions.Regex(".*FeatureFlags in expected final state.*"));
        }

        [UnityTest]
        public IEnumerator SingletonCodeRemover_ErrorHandling_ShouldHandleIncompleteSetup()
        {
            // Arrange - 荳榊ｮ悟・縺ｪ迥ｶ諷九ｒ繧ｻ繝・ヨ繧｢繝・・・・ay 5譛ｪ螳御ｺ・ｼ・
            FeatureFlags.EnableMigrationWarnings = true; // 縺ｾ縺隴ｦ蜻翫′譛牙柑
            FeatureFlags.DisableLegacySingletons = false; // 縺ｾ縺Singleton縺梧怏蜉ｹ

            // Act
            remover.ExecuteCleanupConfirmed();
            yield return new WaitForSeconds(0.1f);

            // Assert - 隴ｦ蜻翫ｄ繧ｨ繝ｩ繝ｼ縺碁←蛻・↓蝣ｱ蜻翫＆繧後ｋ縺薙→繧堤｢ｺ隱・
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex(".*not in expected final state.*"));
        }
    }
}


