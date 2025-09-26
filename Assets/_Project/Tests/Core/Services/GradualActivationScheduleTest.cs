using UnityEngine;
using System.Collections;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Debug;

namespace asterivo.Unity60.Tests.Core.Services
{
    /// <summary>
    /// Step 3.7: 谿ｵ髫守噪讖溯・譛牙柑蛹悶せ繧ｱ繧ｸ繝･繝ｼ繝ｫ縺ｮ蛹・峡繝・せ繝・
    /// MigrationScheduler縲：eatureFlagScheduler縲｀igrationProgressTracker縺ｮ邨ｱ蜷亥虚菴懊ｒ繝・せ繝・
    /// </summary>
    public class GradualActivationScheduleTest : MonoBehaviour
    {
        [Header("Test Configuration")]
        [SerializeField] private bool runTestOnStart = false;
        [SerializeField] private bool enableDetailedLogs = true;
        [SerializeField] private bool testAutomaticMode = false;
        [SerializeField] private float acceleratedTimeMultiplier = 100f; // 繝・せ繝域凾髢薙ｒ遏ｭ邵ｮ

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
        /// 蜈ｨ谿ｵ髫守噪譛牙柑蛹悶せ繧ｱ繧ｸ繝･繝ｼ繝ｫ繝・せ繝医・螳溯｡・
        /// </summary>
        [ContextMenu("Run All Schedule Tests")]
        public void RunAllScheduleTestsSync()
        {
            StartCoroutine(RunAllScheduleTests());
        }

        /// <summary>
        /// 蜈ｨ繝・せ繝医・髱槫酔譛溷ｮ溯｡・
        /// </summary>
        private IEnumerator RunAllScheduleTests()
        {
            LogTest("=== Starting Gradual Activation Schedule Tests ===");

            // 繝・せ繝育ｵ先棡繧偵Μ繧ｻ繝・ヨ
            ResetTestResults();

            // 繧ｳ繝ｳ繝昴・繝阪Φ繝医・蛻晄悄蛹・
            yield return StartCoroutine(InitializeComponents());

            // 蜷・ユ繧ｹ繝医ｒ鬆・分縺ｫ螳溯｡・
            yield return StartCoroutine(TestMigrationScheduler());
            yield return StartCoroutine(TestFeatureFlagScheduler());
            yield return StartCoroutine(TestProgressTracker());
            yield return StartCoroutine(TestIntegratedSchedule());

            // 邱丞粋隧穂ｾ｡
            allTestsPassed = migrationSchedulerTestPassed && featureFlagSchedulerTestPassed && 
                           progressTrackerTestPassed && integrationTestPassed;

            // 邨先棡蝣ｱ蜻・
            ReportTestResults();

            LogTest("=== Gradual Activation Schedule Tests Completed ===");
        }

        /// <summary>
        /// 繧ｳ繝ｳ繝昴・繝阪Φ繝医・蛻晄悄蛹・
        /// </summary>
        private IEnumerator InitializeComponents()
        {
            LogTest("Initializing test components...");

            // 繧ｳ繝ｳ繝昴・繝阪Φ繝医・蜿門ｾ励∪縺溘・菴懈・
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

            // 蛻晄悄蛹悶・螳御ｺ・ｒ蠕・ｩ・
            yield return new WaitForSeconds(0.5f);

            LogTest("Components initialized successfully");
        }

        #endregion

        #region Individual Component Tests

        /// <summary>
        /// MigrationScheduler縺ｮ繝・せ繝・
        /// </summary>
        private IEnumerator TestMigrationScheduler()
        {
            LogTest("[TEST 1] Testing MigrationScheduler...");

            bool initialStateValid = false;
            bool startStateValid = false;
            bool advanceStateValid = false;

            try
            {
                // 蛻晄悄迥ｶ諷九・遒ｺ隱・
                var initialStatus = migrationScheduler.GetCurrentStatus();
                initialStateValid = initialStatus.currentPhase != MigrationScheduler.MigrationPhase.NotStarted;

                // 繧ｹ繧ｱ繧ｸ繝･繝ｼ繝ｫ縺ｮ髢句ｧ・
                migrationScheduler.StartSchedule();
            }
            catch (System.Exception ex)
            {
                migrationSchedulerTestPassed = false;
                LogTest($"[TEST 1] MigrationScheduler: 笶・FAILED - {ex.Message}");
                yield break;
            }

            yield return new WaitForSeconds(0.2f);

            try
            {
                // 髢句ｧ句ｾ後・迥ｶ諷狗｢ｺ隱・
                var startStatus = migrationScheduler.GetCurrentStatus();
                startStateValid = startStatus.currentPhase == MigrationScheduler.MigrationPhase.Day1_2_Staging;

                // 謇句虚縺ｧ縺ｮ繝輔ぉ繝ｼ繧ｺ騾ｲ陦後ユ繧ｹ繝・
                migrationScheduler.AdvanceToNextPhase();
            }
            catch (System.Exception ex)
            {
                migrationSchedulerTestPassed = false;
                LogTest($"[TEST 1] MigrationScheduler: 笶・FAILED - {ex.Message}");
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
                LogTest($"[TEST 1] MigrationScheduler: 笶・FAILED - {ex.Message}");
                yield break;
            }

            // 邨先棡蛻､螳・
            migrationSchedulerTestPassed = initialStateValid && startStateValid && advanceStateValid;

            LogTest($"[TEST 1] MigrationScheduler: {(migrationSchedulerTestPassed ? "笨・PASSED" : "笶・FAILED")}");
            LogTest($"  - Initial State: {(initialStateValid ? "笨・ : "笶・)}");
            LogTest($"  - Start State: {(startStateValid ? "笨・ : "笶・)}");  
            LogTest($"  - Advance State: {(advanceStateValid ? "笨・ : "笶・)}");
        }

        /// <summary>
        /// FeatureFlagScheduler縺ｮ繝・せ繝・
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
                // 蛻晄悄繝輔Λ繧ｰ迥ｶ諷九・險倬鹸
                bool initialAudioService = asterivo.Unity60.Core.FeatureFlags.UseNewAudioService;
                bool initialSpatialService = asterivo.Unity60.Core.FeatureFlags.UseNewSpatialService;
                bool initialStealthService = asterivo.Unity60.Core.FeatureFlags.UseNewStealthService;

                // Day 1-2 險ｭ螳壹ｒ繝・せ繝・
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
                LogTest($"[TEST 2] FeatureFlagScheduler: 笶・FAILED - {ex.Message}");
                yield break;
            }

            yield return new WaitForSeconds(0.2f);

            try
            {
                // 繝輔Λ繧ｰ迥ｶ諷九・遒ｺ隱・
                audioServiceSet = asterivo.Unity60.Core.FeatureFlags.UseNewAudioService == true;
                spatialServiceSet = asterivo.Unity60.Core.FeatureFlags.UseNewSpatialService == false;
                stealthServiceSet = asterivo.Unity60.Core.FeatureFlags.UseNewStealthService == false;

                // Day 4 險ｭ螳壹ｒ繝・せ繝・
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
                LogTest($"[TEST 2] FeatureFlagScheduler: 笶・FAILED - {ex.Message}");
                yield break;
            }

            yield return new WaitForSeconds(0.2f);

            try
            {
                // 譛邨ら憾諷九・遒ｺ隱・
                finalAudioServiceSet = asterivo.Unity60.Core.FeatureFlags.UseNewAudioService == true;
                finalSpatialServiceSet = asterivo.Unity60.Core.FeatureFlags.UseNewSpatialService == true;
                finalStealthServiceSet = asterivo.Unity60.Core.FeatureFlags.UseNewStealthService == true;

                // 繝輔Λ繧ｰ迥ｶ諷九・遒ｺ隱・
                audioServiceSet = asterivo.Unity60.Core.FeatureFlags.UseNewAudioService == true;
                spatialServiceSet = asterivo.Unity60.Core.FeatureFlags.UseNewSpatialService == false;
                stealthServiceSet = asterivo.Unity60.Core.FeatureFlags.UseNewStealthService == false;

                // Day 4 險ｭ螳壹ｒ繝・せ繝・
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
                LogTest($"[TEST 2] FeatureFlagScheduler: 笶・FAILED - {ex.Message}");
                yield break;
            }

            yield return new WaitForSeconds(0.2f);

            // 譛邨ら憾諷九・遒ｺ隱・
            finalAudioServiceSet = asterivo.Unity60.Core.FeatureFlags.UseNewAudioService == true;
            finalSpatialServiceSet = asterivo.Unity60.Core.FeatureFlags.UseNewSpatialService == true;
            finalStealthServiceSet = asterivo.Unity60.Core.FeatureFlags.UseNewStealthService == true;

            // 邨先棡蛻､螳・
            featureFlagSchedulerTestPassed = audioServiceSet && spatialServiceSet && stealthServiceSet &&
                                           finalAudioServiceSet && finalSpatialServiceSet && finalStealthServiceSet;

            LogTest($"[TEST 2] FeatureFlagScheduler: {(featureFlagSchedulerTestPassed ? "笨・PASSED" : "笶・FAILED")}");
            LogTest($"  - Day1-2 Audio: {(audioServiceSet ? "笨・ : "笶・)}");
            LogTest($"  - Day1-2 Spatial: {(spatialServiceSet ? "笨・ : "笶・)}");
            LogTest($"  - Day1-2 Stealth: {(stealthServiceSet ? "笨・ : "笶・)}");
            LogTest($"  - Day4 Audio: {(finalAudioServiceSet ? "笨・ : "笶・)}");
            LogTest($"  - Day4 Spatial: {(finalSpatialServiceSet ? "笨・ : "笶・)}");
            LogTest($"  - Day4 Stealth: {(finalStealthServiceSet ? "笨・ : "笶・)}");
        }

        /// <summary>
        /// MigrationProgressTracker縺ｮ繝・せ繝・
        /// </summary>
        private IEnumerator TestProgressTracker()
        {
            LogTest("[TEST 3] Testing MigrationProgressTracker...");

            bool validationWorked = false;
            bool successRateValid = false;

            try
            {
                // 繝輔ぉ繝ｼ繧ｺ髢句ｧ九・險倬鹸繝・せ繝・
                progressTracker.RecordPhaseStart(MigrationScheduler.MigrationPhase.Day1_2_Staging);
            }
            catch (System.Exception ex)
            {
                progressTrackerTestPassed = false;
                LogTest($"[TEST 3] MigrationProgressTracker: 笶・FAILED - {ex.Message}");
                yield break;
            }

            yield return new WaitForSeconds(0.2f);

            try
            {
                // 繝輔ぉ繝ｼ繧ｺ驕ｷ遘ｻ縺ｮ險倬鹸繝・せ繝・ 
                progressTracker.RecordPhaseTransition(
                    MigrationScheduler.MigrationPhase.Day1_2_Staging,
                    MigrationScheduler.MigrationPhase.Day3_SpatialEnabled
                );
            }
            catch (System.Exception ex)
            {
                progressTrackerTestPassed = false;
                LogTest($"[TEST 3] MigrationProgressTracker: 笶・FAILED - {ex.Message}");
                yield break;
            }

            yield return new WaitForSeconds(0.2f);

            try
            {
                // 讀懆ｨｼ讖溯・縺ｮ繝・せ繝・
                var validation = progressTracker.ValidateCurrentPhase();
                validationWorked = validation.timestamp > 0;

                // 謌仙粥邇・・遒ｺ隱・
                float successRate = progressTracker.GetOverallSuccessRate();
                successRateValid = successRate >= 0f && successRate <= 1f;
                LogTest($"  - Validation: {(validationWorked ? "笨・ : "笶・)}");
                LogTest($"  - Success Rate: {(successRateValid ? "笨・ : "笶・)} ({successRate:P1})");
            }
            catch (System.Exception ex)
            {
                progressTrackerTestPassed = false;
                LogTest($"[TEST 3] MigrationProgressTracker: 笶・FAILED - {ex.Message}");
            }
        }

        /// <summary>
        /// 邨ｱ蜷医せ繧ｱ繧ｸ繝･繝ｼ繝ｫ縺ｮ繝・せ繝・
        /// </summary>
        /// <summary>
        /// 邨ｱ蜷医せ繧ｱ繧ｸ繝･繝ｼ繝ｫ縺ｮ繝・せ繝・
        /// </summary>
        private IEnumerator TestIntegratedSchedule()
        {
            LogTest("[TEST 4] Testing Integrated Schedule...");

            bool hasError = false;
            System.Exception caughtException = null;

            // 邨ｱ蜷医ユ繧ｹ繝医・螳溯｡・- yield return 縺ｯ try-catch 縺ｮ螟悶〒螳溯｡・
            yield return StartCoroutine(RunIntegratedScheduleTest());
            // integrationTestPassed縺ｯRunIntegratedScheduleTest蜀・〒險ｭ螳壹＆繧後ｋ

            try
            {
                // 繧ｨ繝ｩ繝ｼ繝√ぉ繝・け蜃ｦ逅・
                if (caughtException != null)
                {
                    throw caughtException;
                }

                LogTest($"[TEST 4] Integrated Schedule: {(integrationTestPassed ? "笨・PASSED" : "笶・FAILED")}");
            }
            catch (System.Exception ex)
            {
                integrationTestPassed = false;
                LogTest($"[TEST 4] Integrated Schedule: 笶・FAILED - {ex.Message}");
            }
        }

        /// <summary>
        /// 邨ｱ蜷医せ繧ｱ繧ｸ繝･繝ｼ繝ｫ繝・せ繝医・螳溯｡・
        /// </summary>
        private IEnumerator RunIntegratedScheduleTest()
        {
            LogTest("Running integrated schedule test...");

            // 繧ｹ繧ｱ繧ｸ繝･繝ｼ繝ｫ縺ｮ繝ｪ繧ｻ繝・ヨ
            migrationScheduler.StartSchedule();
            yield return new WaitForSeconds(0.1f);

            // 蜷・ヵ繧ｧ繝ｼ繧ｺ繧帝・分縺ｫ騾ｲ陦・
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

                // 繝輔ぉ繝ｼ繧ｺ縺ｫ騾ｲ陦・
                migrationScheduler.AdvanceToNextPhase();
                yield return new WaitForSeconds(0.1f);

                // 迴ｾ蝨ｨ縺ｮ迥ｶ諷九ｒ遒ｺ隱・
                var status = migrationScheduler.GetCurrentStatus();
                if (status.currentPhase != targetPhase)
                {
                    LogTest($"笶・Phase mismatch: expected {targetPhase}, got {status.currentPhase}");
                    allPhasesPassed = false;
                    continue;
                }

                // FeatureFlags縺ｮ迥ｶ諷九ｒ遒ｺ隱・
                bool flagsValid = ValidateFeatureFlagsForPhase(targetPhase);
                if (!flagsValid)
                {
                    LogTest($"笶・FeatureFlags invalid for phase {targetPhase}");
                    allPhasesPassed = false;
                }

                // 騾ｲ陦檎憾豕√・讀懆ｨｼ
                var validation = progressTracker.ValidateCurrentPhase();
                if (validation.timestamp <= 0)
                {
                    LogTest($"笶・Progress tracking failed for phase {targetPhase}");
                    allPhasesPassed = false;
                }

                LogTest($"笨・Phase {targetPhase} completed successfully");
            }

            yield return new WaitForSeconds(0.1f);
            integrationTestPassed = allPhasesPassed; // 邨先棡繧偵ヵ繧｣繝ｼ繝ｫ繝峨↓險ｭ螳・
        }

        #endregion

        #region Validation Helpers

        /// <summary>
        /// 謖・ｮ壹ヵ繧ｧ繝ｼ繧ｺ縺ｫ蟇ｾ縺吶ｋFeatureFlags縺ｮ迥ｶ諷九ｒ讀懆ｨｼ
        /// </summary>
        /// <param name="phase">讀懆ｨｼ縺吶ｋ繝輔ぉ繝ｼ繧ｺ</param>
        /// <returns>讀懆ｨｼ邨先棡</returns>
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
        /// 繝・せ繝育ｵ先棡縺ｮ繝ｪ繧ｻ繝・ヨ
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
        /// 繝・せ繝育ｵ先棡縺ｮ蝣ｱ蜻・
        /// </summary>
        private void ReportTestResults()
        {
            LogTest("=== Schedule Test Results ===");
            LogTest($"MigrationScheduler: {(migrationSchedulerTestPassed ? "笨・PASSED" : "笶・FAILED")}");
            LogTest($"FeatureFlagScheduler: {(featureFlagSchedulerTestPassed ? "笨・PASSED" : "笶・FAILED")}");
            LogTest($"MigrationProgressTracker: {(progressTrackerTestPassed ? "笨・PASSED" : "笶・FAILED")}");
            LogTest($"Integration Test: {(integrationTestPassed ? "笨・PASSED" : "笶・FAILED")}");
            LogTest($"Overall Result: {(allTestsPassed ? "脂 ALL TESTS PASSED" : "笶・SOME TESTS FAILED")}");

            if (allTestsPassed)
            {
                ServiceHelper.Log("脂 [SCHEDULE TEST] Step 3.7 Gradual Activation Schedule is working correctly!");
            }
            else
            {
                ServiceHelper.LogError("笶・[SCHEDULE TEST] Some Step 3.7 schedule components need attention.");
            }
        }

        #endregion

        #region Individual Tests

        /// <summary>
        /// MigrationScheduler縺ｮ縺ｿ繝・せ繝・
        /// </summary>
        [ContextMenu("Test MigrationScheduler Only")]
        public void TestMigrationSchedulerOnly()
        {
            StartCoroutine(TestMigrationScheduler());
        }

        /// <summary>
        /// FeatureFlagScheduler縺ｮ縺ｿ繝・せ繝・
        /// </summary>
        [ContextMenu("Test FeatureFlagScheduler Only")]
        public void TestFeatureFlagSchedulerOnly()
        {
            StartCoroutine(TestFeatureFlagScheduler());
        }

        /// <summary>
        /// MigrationProgressTracker縺ｮ縺ｿ繝・せ繝・
        /// </summary>
        [ContextMenu("Test ProgressTracker Only")]
        public void TestProgressTrackerOnly()
        {
            StartCoroutine(TestProgressTracker());
        }

        /// <summary>
        /// 迴ｾ蝨ｨ縺ｮFeatureFlags迥ｶ諷九ｒ繝ｬ繝昴・繝・
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
        /// 謇句虚縺ｧ縺ｮ繧ｹ繧ｱ繧ｸ繝･繝ｼ繝ｫ騾ｲ陦後ユ繧ｹ繝・
        /// </summary>
        [ContextMenu("Manual Schedule Progression")]
        public void ManualScheduleProgression()
        {
            LogTest("Starting manual schedule progression...");
            StartCoroutine(RunManualProgression());
        }

        /// <summary>
        /// 謇句虚騾ｲ陦後・螳溯｡・
        /// </summary>
        private IEnumerator RunManualProgression()
        {
            // 蜷・ヵ繧ｧ繝ｼ繧ｺ繧呈焔蜍輔〒騾ｲ陦・
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

            // 譛邨ら憾諷九Ξ繝昴・繝・
            ReportCurrentFeatureFlags();
            progressTracker.GenerateProgressReport();
        }

        #endregion

        #region Logging

        /// <summary>
        /// 繝・せ繝医Ο繧ｰ縺ｮ蜃ｺ蜉・
        /// </summary>
        /// <param name="message">繝｡繝・そ繝ｼ繧ｸ</param>
        private void LogTest(string message)
        {
            if (enableDetailedLogs)
            {
                ServiceHelper.Log($"[SCHEDULE_TEST] {message}");
            }
        }

        #endregion
    }
}


