using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Services;

namespace asterivo.Unity60.Tests.Core.Services
{
    /// <summary>
    /// Step 3.11: 最終クリーンアップ システムのテスト
    /// SingletonCodeRemover の機能テスト
    /// </summary>
    public class SingletonCodeRemovalTest
    {
        private GameObject removerObject;
        private SingletonCodeRemover remover;
        private bool originalCleanupState;

        [SetUp]
        public void Setup()
        {
            // テスト用のSingletonCodeRemoverを作成
            removerObject = new GameObject("SingletonCodeRemoverTest");
            remover = removerObject.AddComponent<SingletonCodeRemover>();

            // 元の状態を保存
            originalCleanupState = PlayerPrefs.GetInt("SingletonCodeRemover_Completed", 0) == 1;
            
            // テスト用にリセット
            remover.ResetCleanupState();
        }

        [TearDown]
        public void TearDown()
        {
            // 元の状態を復元
            PlayerPrefs.SetInt("SingletonCodeRemover_Completed", originalCleanupState ? 1 : 0);
            PlayerPrefs.Save();

            // テストオブジェクトをクリーンアップ
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

            // Assert - デフォルトでは確認が必要で、実際のクリーンアップは実行されない
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex(".*Manual confirmation required.*"));
        }

        [UnityTest]
        public IEnumerator SingletonCodeRemover_ExecuteCleanupConfirmed_ShouldCompleteCleanup()
        {
            // Act
            remover.ExecuteCleanupConfirmed();
            yield return new WaitForSeconds(0.1f);

            // Assert - クリーンアップが完了することを確認
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
            // Arrange - システムを完全移行状態にセットアップ
            asterivo.Unity60.Core.FeatureFlags.UseServiceLocator = true;
            asterivo.Unity60.Core.FeatureFlags.DisableLegacySingletons = true;
            asterivo.Unity60.Core.FeatureFlags.EnableMigrationWarnings = false;
            asterivo.Unity60.Core.FeatureFlags.UseNewAudioService = true;
            asterivo.Unity60.Core.FeatureFlags.UseNewSpatialService = true;
            asterivo.Unity60.Core.FeatureFlags.UseNewStealthService = true;

            // Act
            remover.ExecuteCleanupConfirmed();
            yield return new WaitForSeconds(0.1f);

            // Assert - システムが健全な状態になることを確認
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

            // Assert - バックアップ記録が作成されることを確認
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

            // Assert - 各Managerのクリーンアップ記録が作成されることを確認
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
            // Arrange - 完了状態をシミュレート
            PlayerPrefs.SetInt("SingletonCodeRemover_Completed", 1);
            PlayerPrefs.SetString("SingletonCodeRemover_LastTime", "2024-01-01 12:00:00");

            // Act
            remover.ResetCleanupState();

            // Assert - 状態がリセットされることを確認
            bool completed = PlayerPrefs.GetInt("SingletonCodeRemover_Completed", 0) == 1;
            string lastTime = PlayerPrefs.GetString("SingletonCodeRemover_LastTime", "");
            
            Assert.IsFalse(completed, "Cleanup completed flag should be reset");
            Assert.IsEmpty(lastTime, "Last cleanup time should be cleared");
        }

        [UnityTest]
        public IEnumerator IntegrationTest_Day5ToCleanup_ShouldWorkSeamlessly()
        {
            // Arrange - Day 5完了状態をシミュレート
            FeatureFlags.EnableMigrationWarnings = false;
            FeatureFlags.DisableLegacySingletons = true;
            FeatureFlags.UseServiceLocator = true;

            // SingletonDisableSchedulerも作成
            var schedulerObj = new GameObject("TestScheduler");
            var scheduler = schedulerObj.AddComponent<SingletonDisableScheduler>();
            
            try
            {
                scheduler.StartSchedule();
                scheduler.AdvanceToDay5();
                yield return new WaitForSeconds(0.1f);

                // Act - クリーンアップ実行
                remover.ExecuteCleanupConfirmed();
                yield return new WaitForSeconds(0.1f);

                // Assert - 統合的に動作することを確認
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
            // Arrange - MigrationValidatorを作成
            var validatorObj = new GameObject("TestValidator");
            var validator = validatorObj.AddComponent<MigrationValidator>();
            
            try
            {
                // Act
                remover.ExecuteCleanupConfirmed();
                yield return new WaitForSeconds(0.1f);

                // Assert - MigrationValidatorが実行されることを確認
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
            // Arrange - 完全移行状態をセットアップ
            FeatureFlags.UseServiceLocator = true;
            FeatureFlags.DisableLegacySingletons = true;
            FeatureFlags.EnableMigrationWarnings = false;
            FeatureFlags.UseNewAudioService = true;
            FeatureFlags.UseNewSpatialService = true;
            FeatureFlags.UseNewStealthService = true;

            // Act
            remover.ExecuteCleanupConfirmed();
            yield return new WaitForSeconds(0.1f);

            // Assert - FeatureFlagsの最終状態が正しいことを確認
            LogAssert.Expect(LogType.Log, new System.Text.RegularExpressions.Regex(".*FeatureFlags in expected final state.*"));
        }

        [UnityTest]
        public IEnumerator SingletonCodeRemover_ErrorHandling_ShouldHandleIncompleteSetup()
        {
            // Arrange - 不完全な状態をセットアップ（Day 5未完了）
            FeatureFlags.EnableMigrationWarnings = true; // まだ警告が有効
            FeatureFlags.DisableLegacySingletons = false; // まだSingletonが有効

            // Act
            remover.ExecuteCleanupConfirmed();
            yield return new WaitForSeconds(0.1f);

            // Assert - 警告やエラーが適切に報告されることを確認
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex(".*not in expected final state.*"));
        }
    }
}
