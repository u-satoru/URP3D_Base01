using UnityEngine;
using System.Collections;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Core.Debug;

namespace asterivo.Unity60.Tests.Core.Services
{
    /// <summary>
    /// Step 3.7: 段階的機能有効化スケジュールの包括テスト
    /// MigrationScheduler、FeatureFlagScheduler、MigrationProgressTrackerの統合動作をテスト
    /// </summary>
    public class GradualActivationScheduleTest : MonoBehaviour
    {
        [Header("Test Configuration")]
        [SerializeField] private bool runTestOnStart = false;
        [SerializeField] private bool enableDetailedLogs = true;

        [Header("Test Results")]
        [SerializeField] private bool migrationSchedulerTestPassed;
        [SerializeField] private bool featureFlagSchedulerTestPassed;  
        [SerializeField] private bool progressTrackerTestPassed;
        [SerializeField] private bool integrationTestPassed;
        [SerializeField] private bool allTestsPassed;

        [Header("Component References")]
        [SerializeField] private MigrationScheduler migrationScheduler;
        [SerializeField] private FeatureFlagScheduler flagScheduler;
        [SerializeField] private MigrationProgressTracker progressTracker;

        private void Start()
        {
            if (runTestOnStart)
            {
                StartCoroutine(RunAllScheduleTests());
            }
        }

        #region Test Execution

        /// <summary>
        /// 全段階的有効化スケジュールテストの実行
        /// </summary>
        [ContextMenu("Run All Schedule Tests")]
        public void RunAllScheduleTestsSync()
        {
            StartCoroutine(RunAllScheduleTests());
        }

        /// <summary>
        /// 全テストの非同期実行
        /// </summary>
        private IEnumerator RunAllScheduleTests()
        {
            LogTest("=== Starting Gradual Activation Schedule Tests ===");

            // テスト結果をリセット
            ResetTestResults();

            // コンポーネントの初期化
            yield return StartCoroutine(InitializeComponents());

            // 各テストを順番に実行
            yield return StartCoroutine(TestMigrationScheduler());
            yield return StartCoroutine(TestFeatureFlagScheduler());
            yield return StartCoroutine(TestProgressTracker());
            yield return StartCoroutine(TestIntegratedSchedule());

            // 総合評価
            allTestsPassed = migrationSchedulerTestPassed && featureFlagSchedulerTestPassed && 
                           progressTrackerTestPassed && integrationTestPassed;

            // 結果報告
            ReportTestResults();

            LogTest("=== Gradual Activation Schedule Tests Completed ===");
        }

        /// <summary>
        /// コンポーネントの初期化
        /// </summary>
        private IEnumerator InitializeComponents()
        {
            LogTest("Initializing test components...");

            // コンポーネントの取得または作成
            migrationScheduler = GetComponent<MigrationScheduler>();
            if (migrationScheduler == null)
            {
                migrationScheduler = gameObject.AddComponent<MigrationScheduler>();
            }

            flagScheduler = GetComponent<FeatureFlagScheduler>();
            if (flagScheduler == null)
            {
                flagScheduler = gameObject.AddComponent<FeatureFlagScheduler>();
            }

            progressTracker = GetComponent<MigrationProgressTracker>();
            if (progressTracker == null)
            {
                progressTracker = gameObject.AddComponent<MigrationProgressTracker>();
            }

            // 初期化の完了を待機
            yield return new WaitForSeconds(0.5f);

            LogTest("Components initialized successfully");
        }

        #endregion

        #region Individual Component Tests

        /// <summary>
        /// MigrationSchedulerのテスト
        /// </summary>
        private IEnumerator TestMigrationScheduler()
        {
            LogTest("[TEST 1] Testing MigrationScheduler...");

            bool initialStateValid = false;
            bool startStateValid = false;
            bool advanceStateValid = false;

            try
            {
                // 初期状態の確認
                var initialStatus = migrationScheduler.GetCurrentStatus();
                initialStateValid = initialStatus.currentPhase != MigrationScheduler.MigrationPhase.NotStarted;

                // スケジュールの開始
                migrationScheduler.StartSchedule();
            }
            catch (System.Exception ex)
            {
                migrationSchedulerTestPassed = false;
                LogTest($"[TEST 1] MigrationScheduler: ❌ FAILED - {ex.Message}");
                yield break;
            }

            yield return new WaitForSeconds(0.2f);

            try
            {
                // 開始後の状態確認
                var startStatus = migrationScheduler.GetCurrentStatus();
                startStateValid = startStatus.currentPhase == MigrationScheduler.MigrationPhase.Day1_2_Staging;

                // 手動でのフェーズ進行テスト
                migrationScheduler.AdvanceToNextPhase();
            }
            catch (System.Exception ex)
            {
                migrationSchedulerTestPassed = false;
                LogTest($"[TEST 1] MigrationScheduler: ❌ FAILED - {ex.Message}");
                yield break;
            }

            yield return new WaitForSeconds(0.2f);

            try
            {
                var advancedStatus = migrationScheduler.GetCurrentStatus();
                advanceStateValid = advancedStatus.currentPhase == MigrationScheduler.MigrationPhase.Day3_SpatialEnabled;

            }
            catch (System.Exception ex)
            {
                migrationSchedulerTestPassed = false;
                LogTest($"[TEST 1] MigrationScheduler: ❌ FAILED - {ex.Message}");
                yield break;
            }

            // 結果判定
            migrationSchedulerTestPassed = initialStateValid && startStateValid && advanceStateValid;

            LogTest($"[TEST 1] MigrationScheduler: {(migrationSchedulerTestPassed ? "✅ PASSED" : "❌ FAILED")}");
            LogTest($"  - Initial State: {(initialStateValid ? "✅" : "❌")}");
            LogTest($"  - Start State: {(startStateValid ? "✅" : "❌")}");  
            LogTest($"  - Advance State: {(advanceStateValid ? "✅" : "❌")}");
        }

        /// <summary>
        /// FeatureFlagSchedulerのテスト
        /// </summary>
        private IEnumerator TestFeatureFlagScheduler()
        {
            LogTest("[TEST 2] Testing FeatureFlagScheduler...");

            bool audioServiceSet = false;
            bool spatialServiceSet = false;
            bool stealthServiceSet = false;
            bool finalAudioServiceSet = false;
            bool finalSpatialServiceSet = false;
            bool finalStealthServiceSet = false;

            try
            {
                // 初期フラグ状態の記録
                bool initialAudioService = asterivo.Unity60.Core.FeatureFlags.UseNewAudioService;
                bool initialSpatialService = asterivo.Unity60.Core.FeatureFlags.UseNewSpatialService;
                bool initialStealthService = asterivo.Unity60.Core.FeatureFlags.UseNewStealthService;

                // Day 1-2 設定をテスト
                var day12Config = new MigrationScheduler.PhaseConfiguration
                {
                    phase = MigrationScheduler.MigrationPhase.Day1_2_Staging,
                    phaseName = "Test Day 1-2",
                    useNewAudioService = true,
                    useNewSpatialService = false,
                    useNewStealthService = false,
                    enablePerformanceMonitoring = true,
                    description = "Test configuration"
                };

                flagScheduler.ApplyPhaseConfiguration(day12Config);
            }
            catch (System.Exception ex)
            {
                featureFlagSchedulerTestPassed = false;
                LogTest($"[TEST 2] FeatureFlagScheduler: ❌ FAILED - {ex.Message}");
                yield break;
            }

            yield return new WaitForSeconds(0.2f);

            try
            {
                // フラグ状態の確認
                audioServiceSet = asterivo.Unity60.Core.FeatureFlags.UseNewAudioService == true;
                spatialServiceSet = asterivo.Unity60.Core.FeatureFlags.UseNewSpatialService == false;
                stealthServiceSet = asterivo.Unity60.Core.FeatureFlags.UseNewStealthService == false;

                // Day 4 設定をテスト
                var day4Config = new MigrationScheduler.PhaseConfiguration
                {
                    phase = MigrationScheduler.MigrationPhase.Day4_StealthEnabled,
                    phaseName = "Test Day 4",
                    useNewAudioService = true,
                    useNewSpatialService = true,
                    useNewStealthService = true,
                    enablePerformanceMonitoring = true,
                    description = "Test configuration"
                };

                flagScheduler.ApplyPhaseConfiguration(day4Config);
            }
            catch (System.Exception ex)
            {
                featureFlagSchedulerTestPassed = false;
                LogTest($"[TEST 2] FeatureFlagScheduler: ❌ FAILED - {ex.Message}");
                yield break;
            }

            yield return new WaitForSeconds(0.2f);

            try
            {
                // 最終状態の確認
                finalAudioServiceSet = asterivo.Unity60.Core.FeatureFlags.UseNewAudioService == true;
                finalSpatialServiceSet = asterivo.Unity60.Core.FeatureFlags.UseNewSpatialService == true;
                finalStealthServiceSet = asterivo.Unity60.Core.FeatureFlags.UseNewStealthService == true;

                // フラグ状態の確認
                audioServiceSet = asterivo.Unity60.Core.FeatureFlags.UseNewAudioService == true;
                spatialServiceSet = asterivo.Unity60.Core.FeatureFlags.UseNewSpatialService == false;
                stealthServiceSet = asterivo.Unity60.Core.FeatureFlags.UseNewStealthService == false;

                // Day 4 設定をテスト
                var day4Config = new MigrationScheduler.PhaseConfiguration
                {
                    phase = MigrationScheduler.MigrationPhase.Day4_StealthEnabled,
                    phaseName = "Test Day 4",
                    useNewAudioService = true,
                    useNewSpatialService = true,
                    useNewStealthService = true,
                    enablePerformanceMonitoring = true,
                    description = "Test configuration"
                };

                flagScheduler.ApplyPhaseConfiguration(day4Config);
            }
            catch (System.Exception ex)
            {
                featureFlagSchedulerTestPassed = false;
                LogTest($"[TEST 2] FeatureFlagScheduler: ❌ FAILED - {ex.Message}");
                yield break;
            }

            yield return new WaitForSeconds(0.2f);

            // 最終状態の確認
            finalAudioServiceSet = asterivo.Unity60.Core.FeatureFlags.UseNewAudioService == true;
            finalSpatialServiceSet = asterivo.Unity60.Core.FeatureFlags.UseNewSpatialService == true;
            finalStealthServiceSet = asterivo.Unity60.Core.FeatureFlags.UseNewStealthService == true;

            // 結果判定
            featureFlagSchedulerTestPassed = audioServiceSet && spatialServiceSet && stealthServiceSet &&
                                           finalAudioServiceSet && finalSpatialServiceSet && finalStealthServiceSet;

            LogTest($"[TEST 2] FeatureFlagScheduler: {(featureFlagSchedulerTestPassed ? "✅ PASSED" : "❌ FAILED")}");
            LogTest($"  - Day1-2 Audio: {(audioServiceSet ? "✅" : "❌")}");
            LogTest($"  - Day1-2 Spatial: {(spatialServiceSet ? "✅" : "❌")}");
            LogTest($"  - Day1-2 Stealth: {(stealthServiceSet ? "✅" : "❌")}");
            LogTest($"  - Day4 Audio: {(finalAudioServiceSet ? "✅" : "❌")}");
            LogTest($"  - Day4 Spatial: {(finalSpatialServiceSet ? "✅" : "❌")}");
            LogTest($"  - Day4 Stealth: {(finalStealthServiceSet ? "✅" : "❌")}");
        }

        /// <summary>
        /// MigrationProgressTrackerのテスト
        /// </summary>
        private IEnumerator TestProgressTracker()
        {
            LogTest("[TEST 3] Testing MigrationProgressTracker...");

            bool validationWorked = false;
            bool successRateValid = false;

            try
            {
                // フェーズ開始の記録テスト
                progressTracker.RecordPhaseStart(MigrationScheduler.MigrationPhase.Day1_2_Staging);
            }
            catch (System.Exception ex)
            {
                progressTrackerTestPassed = false;
                LogTest($"[TEST 3] MigrationProgressTracker: ❌ FAILED - {ex.Message}");
                yield break;
            }

            yield return new WaitForSeconds(0.2f);

            try
            {
                // フェーズ遷移の記録テスト  
                progressTracker.RecordPhaseTransition(
                    MigrationScheduler.MigrationPhase.Day1_2_Staging,
                    MigrationScheduler.MigrationPhase.Day3_SpatialEnabled
                );
            }
            catch (System.Exception ex)
            {
                progressTrackerTestPassed = false;
                LogTest($"[TEST 3] MigrationProgressTracker: ❌ FAILED - {ex.Message}");
                yield break;
            }

            yield return new WaitForSeconds(0.2f);

            try
            {
                // 検証機能のテスト
                var validation = progressTracker.ValidateCurrentPhase();
                validationWorked = validation.timestamp > 0;

                // 成功率の確認
                float successRate = progressTracker.GetOverallSuccessRate();
                successRateValid = successRate >= 0f && successRate <= 1f;
                LogTest($"  - Validation: {(validationWorked ? "✅" : "❌")}");
                LogTest($"  - Success Rate: {(successRateValid ? "✅" : "❌")} ({successRate:P1})");
            }
            catch (System.Exception ex)
            {
                progressTrackerTestPassed = false;
                LogTest($"[TEST 3] MigrationProgressTracker: ❌ FAILED - {ex.Message}");
            }
        }

        /// <summary>
        /// 統合スケジュールのテスト
        /// </summary>
        /// <summary>
        /// 統合スケジュールのテスト
        /// </summary>
        private IEnumerator TestIntegratedSchedule()
        {
            LogTest("[TEST 4] Testing Integrated Schedule...");

            System.Exception caughtException = null;

            // 統合テストの実行 - yield return は try-catch の外で実行
            yield return StartCoroutine(RunIntegratedScheduleTest());
            // integrationTestPassedはRunIntegratedScheduleTest内で設定される

            try
            {
                // エラーチェック処理
                if (caughtException != null)
                {
                    throw caughtException;
                }

                LogTest($"[TEST 4] Integrated Schedule: {(integrationTestPassed ? "✅ PASSED" : "❌ FAILED")}");
            }
            catch (System.Exception ex)
            {
                integrationTestPassed = false;
                LogTest($"[TEST 4] Integrated Schedule: ❌ FAILED - {ex.Message}");
            }
        }

        /// <summary>
        /// 統合スケジュールテストの実行
        /// </summary>
        private IEnumerator RunIntegratedScheduleTest()
        {
            LogTest("Running integrated schedule test...");

            // スケジュールのリセット
            migrationScheduler.StartSchedule();
            yield return new WaitForSeconds(0.1f);

            // 各フェーズを順番に進行
            var phases = new[]
            {
                MigrationScheduler.MigrationPhase.Day1_2_Staging,
                MigrationScheduler.MigrationPhase.Day3_SpatialEnabled,
                MigrationScheduler.MigrationPhase.Day4_StealthEnabled,
                MigrationScheduler.MigrationPhase.Day5_Validation
            };

            bool allPhasesPassed = true;

            foreach (var targetPhase in phases)
            {
                LogTest($"Testing phase: {targetPhase}");

                // フェーズに進行
                migrationScheduler.AdvanceToNextPhase();
                yield return new WaitForSeconds(0.1f);

                // 現在の状態を確認
                var status = migrationScheduler.GetCurrentStatus();
                if (status.currentPhase != targetPhase)
                {
                    LogTest($"❌ Phase mismatch: expected {targetPhase}, got {status.currentPhase}");
                    allPhasesPassed = false;
                    continue;
                }

                // FeatureFlagsの状態を確認
                bool flagsValid = ValidateFeatureFlagsForPhase(targetPhase);
                if (!flagsValid)
                {
                    LogTest($"❌ FeatureFlags invalid for phase {targetPhase}");
                    allPhasesPassed = false;
                }

                // 進行状況の検証
                var validation = progressTracker.ValidateCurrentPhase();
                if (validation.timestamp <= 0)
                {
                    LogTest($"❌ Progress tracking failed for phase {targetPhase}");
                    allPhasesPassed = false;
                }

                LogTest($"✅ Phase {targetPhase} completed successfully");
            }

            yield return new WaitForSeconds(0.1f);
            integrationTestPassed = allPhasesPassed; // 結果をフィールドに設定
        }

        #endregion

        #region Validation Helpers

        /// <summary>
        /// 指定フェーズに対するFeatureFlagsの状態を検証
        /// </summary>
        /// <param name="phase">検証するフェーズ</param>
        /// <returns>検証結果</returns>
        private bool ValidateFeatureFlagsForPhase(MigrationScheduler.MigrationPhase phase)
        {
            switch (phase)
            {
                case MigrationScheduler.MigrationPhase.Day1_2_Staging:
                    return asterivo.Unity60.Core.FeatureFlags.UseNewAudioService == true &&
                           asterivo.Unity60.Core.FeatureFlags.UseNewSpatialService == false &&
                           asterivo.Unity60.Core.FeatureFlags.UseNewStealthService == false;

                case MigrationScheduler.MigrationPhase.Day3_SpatialEnabled:
                    return asterivo.Unity60.Core.FeatureFlags.UseNewAudioService == true &&
                           asterivo.Unity60.Core.FeatureFlags.UseNewSpatialService == true &&
                           asterivo.Unity60.Core.FeatureFlags.UseNewStealthService == false;

                case MigrationScheduler.MigrationPhase.Day4_StealthEnabled:
                case MigrationScheduler.MigrationPhase.Day5_Validation:
                    return asterivo.Unity60.Core.FeatureFlags.UseNewAudioService == true &&
                           asterivo.Unity60.Core.FeatureFlags.UseNewSpatialService == true &&
                           asterivo.Unity60.Core.FeatureFlags.UseNewStealthService == true;

                default:
                    return false;
            }
        }

        #endregion

        #region Test Management

        /// <summary>
        /// テスト結果のリセット
        /// </summary>
        private void ResetTestResults()
        {
            migrationSchedulerTestPassed = false;
            featureFlagSchedulerTestPassed = false;
            progressTrackerTestPassed = false;
            integrationTestPassed = false;
            allTestsPassed = false;
        }

        /// <summary>
        /// テスト結果の報告
        /// </summary>
        private void ReportTestResults()
        {
            LogTest("=== Schedule Test Results ===");
            LogTest($"MigrationScheduler: {(migrationSchedulerTestPassed ? "✅ PASSED" : "❌ FAILED")}");
            LogTest($"FeatureFlagScheduler: {(featureFlagSchedulerTestPassed ? "✅ PASSED" : "❌ FAILED")}");
            LogTest($"MigrationProgressTracker: {(progressTrackerTestPassed ? "✅ PASSED" : "❌ FAILED")}");
            LogTest($"Integration Test: {(integrationTestPassed ? "✅ PASSED" : "❌ FAILED")}");
            LogTest($"Overall Result: {(allTestsPassed ? "🎉 ALL TESTS PASSED" : "❌ SOME TESTS FAILED")}");

            if (allTestsPassed)
            {
                EventLogger.LogStatic("🎉 [SCHEDULE TEST] Step 3.7 Gradual Activation Schedule is working correctly!");
            }
            else
            // Intentional test of deprecated EventLogger static method
#pragma warning disable CS0618
            {
                EventLogger.LogErrorStatic("❌ [SCHEDULE TEST] Some Step 3.7 schedule components need attention.");
            }
#pragma warning restore CS0618
        }

        #endregion

        #region Individual Tests

        /// <summary>
        /// MigrationSchedulerのみテスト
        /// </summary>
        [ContextMenu("Test MigrationScheduler Only")]
        public void TestMigrationSchedulerOnly()
        {
            StartCoroutine(TestMigrationScheduler());
        }

        /// <summary>
        /// FeatureFlagSchedulerのみテスト
        /// </summary>
        [ContextMenu("Test FeatureFlagScheduler Only")]
        public void TestFeatureFlagSchedulerOnly()
        {
            StartCoroutine(TestFeatureFlagScheduler());
        }

        /// <summary>
        /// MigrationProgressTrackerのみテスト
        /// </summary>
        [ContextMenu("Test ProgressTracker Only")]
        public void TestProgressTrackerOnly()
        {
            StartCoroutine(TestProgressTracker());
        }

        /// <summary>
        /// 現在のFeatureFlags状態をレポート
        /// </summary>
        [ContextMenu("Report Current FeatureFlags")]
        public void ReportCurrentFeatureFlags()
        {
            LogTest("=== Current FeatureFlags State ===");
            LogTest($"UseServiceLocator: {asterivo.Unity60.Core.FeatureFlags.UseServiceLocator}");
            LogTest($"UseNewAudioService: {asterivo.Unity60.Core.FeatureFlags.UseNewAudioService}");
            LogTest($"UseNewSpatialService: {asterivo.Unity60.Core.FeatureFlags.UseNewSpatialService}");
            LogTest($"UseNewStealthService: {asterivo.Unity60.Core.FeatureFlags.UseNewStealthService}");
            LogTest($"AllowSingletonFallback: {asterivo.Unity60.Core.FeatureFlags.AllowSingletonFallback}");
            LogTest($"EnablePerformanceMonitoring: {asterivo.Unity60.Core.FeatureFlags.EnablePerformanceMonitoring}");
        }

        /// <summary>
        /// 手動でのスケジュール進行テスト
        /// </summary>
        [ContextMenu("Manual Schedule Progression")]
        public void ManualScheduleProgression()
        {
            LogTest("Starting manual schedule progression...");
            StartCoroutine(RunManualProgression());
        }

        /// <summary>
        /// 手動進行の実行
        /// </summary>
        private IEnumerator RunManualProgression()
        {
            // 各フェーズを手動で進行
            migrationScheduler.StartSchedule();
            LogTest("Phase 1: Day 1-2 Staging started");
            yield return new WaitForSeconds(1f);

            migrationScheduler.AdvanceToNextPhase();
            LogTest("Phase 2: Day 3 SpatialEnabled started");
            yield return new WaitForSeconds(1f);

            migrationScheduler.AdvanceToNextPhase();
            LogTest("Phase 3: Day 4 StealthEnabled started");
            yield return new WaitForSeconds(1f);

            migrationScheduler.AdvanceToNextPhase();
            LogTest("Phase 4: Day 5 Validation started");
            yield return new WaitForSeconds(1f);

            migrationScheduler.AdvanceToNextPhase();
            LogTest("Schedule completed!");

            // 最終状態レポート
            ReportCurrentFeatureFlags();
            progressTracker.GenerateProgressReport();
        }

        #endregion

        #region Logging

        /// <summary>
        /// テストログの出力
        /// </summary>
        /// <param name="message">メッセージ</param>
        private void LogTest(string message)
        {
            if (enableDetailedLogs)
            {
                EventLogger.LogStatic($"[SCHEDULE_TEST] {message}");
            }
        }

        #endregion
    }
}