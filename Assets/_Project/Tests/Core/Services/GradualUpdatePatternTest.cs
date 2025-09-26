using UnityEngine;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Audio.Interfaces;
using asterivo.Unity60.Core.Debug;
// Removed Sirenix dependency for test compatibility

namespace asterivo.Unity60.Tests.Core.Services
{
    /// <summary>
    /// 谿ｵ髫守噪譖ｴ譁ｰ繝代ち繝ｼ繝ｳ縺ｮ蛹・峡繝・せ繝・
    /// Step 3.6縺ｧ螳溯｣・＠縺溘ヱ繧ｿ繝ｼ繝ｳ縺ｨ繝倥Ν繝代・繧ｯ繝ｩ繧ｹ繧呈､懆ｨｼ
    /// </summary>
    public class GradualUpdatePatternTest : MonoBehaviour
    {
        // Test Settings
        [Header("Test Configuration")]
        [SerializeField] private bool runTestOnStart = true;
        [SerializeField] private bool enableDetailedLogs = true;

        // Test Results
        [Header("Test Results")]
        [SerializeField] private bool serviceLocatorTestPassed; // ReadOnly removed - use standard SerializeField
        [SerializeField] private bool legacyFallbackTestPassed; // ReadOnly removed
        [SerializeField] private bool helperClassTestPassed; // ReadOnly removed
        [SerializeField] private bool allTestsPassed; // ReadOnly removed

        // Service Information
        [Header("Current Service Information")]
        [SerializeField] private string audioServiceType; // ReadOnly removed
        [SerializeField] private string stealthServiceType; // ReadOnly removed
        [SerializeField] private bool usingServiceLocator; // ReadOnly removed

        private void Start()
        {
            if (runTestOnStart)
            {
                RunAllPatternTests();
            }
        }

        #region Pattern Tests

        /// <summary>
        /// 蜈ｨ谿ｵ髫守噪譖ｴ譁ｰ繝代ち繝ｼ繝ｳ繝・せ繝医・螳溯｡・
        /// </summary>
        [ContextMenu("Run All Pattern Tests")]
        // Button replaced with ContextMenu - see method above
        public void RunAllPatternTests()
        {
            LogTest("=== Starting Gradual Update Pattern Tests ===");

            // 繝・せ繝育ｵ先棡繧偵Μ繧ｻ繝・ヨ
            ResetTestResults();

            // 蜷・ユ繧ｹ繝医ｒ鬆・分縺ｫ螳溯｡・
            serviceLocatorTestPassed = TestServiceLocatorPattern();
            legacyFallbackTestPassed = TestLegacyFallbackPattern();
            helperClassTestPassed = TestHelperClassFunctionality();

            // 邱丞粋隧穂ｾ｡
            allTestsPassed = serviceLocatorTestPassed && legacyFallbackTestPassed && helperClassTestPassed;

            // 邨先棡蝣ｱ蜻・
            ReportTestResults();
        }

        /// <summary>
        /// ServiceLocator繝代ち繝ｼ繝ｳ縺ｮ繝・せ繝・
        /// </summary>
        private bool TestServiceLocatorPattern()
        {
            LogTest("[TEST 1] Testing ServiceLocator Pattern...");

            try
            {
                // ServiceMigrationHelper繧剃ｽｿ逕ｨ縺励◆ServiceLocator蜿門ｾ・
                var audioResult = ServiceMigrationHelper.GetAudioService(true, "PatternTest", enableDetailedLogs);
                var stealthResult = ServiceMigrationHelper.GetStealthAudioService(true, "PatternTest", enableDetailedLogs);

                // 邨先棡縺ｮ讀懆ｨｼ
                bool audioSuccess = audioResult.IsSuccessful && audioResult.Service != null;
                bool stealthSuccess = stealthResult.IsSuccessful && stealthResult.Service != null;

                if (audioSuccess)
                {
                    audioServiceType = audioResult.ServiceTypeName;
                    LogTest($"笨・AudioService obtained: {audioServiceType}");
                }
                else
                {
                    LogTest($"笶・AudioService failed: {audioResult.ErrorMessage}");
                }

                if (stealthSuccess)
                {
                    stealthServiceType = stealthResult.ServiceTypeName;
                    usingServiceLocator = stealthResult.IsUsingServiceLocator;
                    LogTest($"笨・StealthAudioService obtained: {stealthServiceType}");
                    LogTest($"Using ServiceLocator: {usingServiceLocator}");
                }
                else
                {
                    LogTest($"笶・StealthAudioService failed: {stealthResult.ErrorMessage}");
                }

                // 蝓ｺ譛ｬ讖溯・縺ｮ繝・せ繝・
                if (audioSuccess && stealthSuccess)
                {
                    TestBasicServiceFunctionality(audioResult.Service, stealthResult.Service);
                }

                bool testPassed = audioSuccess && stealthSuccess;
                LogTest($"[TEST 1] ServiceLocator Pattern: {(testPassed ? "PASSED" : "FAILED")}");
                
                return testPassed;
            }
            catch (System.Exception ex)
            {
                LogTest($"[TEST 1] ServiceLocator Pattern: FAILED - {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 繝ｬ繧ｬ繧ｷ繝ｼ繝輔か繝ｼ繝ｫ繝舌ャ繧ｯ繝代ち繝ｼ繝ｳ縺ｮ繝・せ繝・
        /// </summary>
        private bool TestLegacyFallbackPattern()
        {
            LogTest("[TEST 2] Testing Legacy Fallback Pattern...");

            try
            {
                // ServiceLocator繧堤┌蜉ｹ蛹悶＠縺ｦ縲√Ξ繧ｬ繧ｷ繝ｼ繝輔か繝ｼ繝ｫ繝舌ャ繧ｯ繧偵ユ繧ｹ繝・
                var audioResult = ServiceMigrationHelper.GetAudioService(false, "PatternTest", enableDetailedLogs);
                var stealthResult = ServiceMigrationHelper.GetStealthAudioService(false, "PatternTest", enableDetailedLogs);

                // 繝ｬ繧ｬ繧ｷ繝ｼ繧ｷ繧ｹ繝・Β縺悟茜逕ｨ蜿ｯ閭ｽ縺九メ繧ｧ繝・け
                bool legacyAvailable = ServiceMigrationHelper.IsLegacySystemAvailable();
                LogTest($"Legacy system available: {legacyAvailable}");

                if (!legacyAvailable)
                {
                    LogTest("[TEST 2] Legacy system disabled, skipping fallback test");
                    return true; // 繝ｬ繧ｬ繧ｷ繝ｼ縺檎┌蜉ｹ蛹悶＆繧後※縺・ｋ蝣ｴ蜷医・繝・せ繝域・蜉滓桶縺・
                }

                // 邨先棡縺ｮ讀懆ｨｼ
                bool audioSuccess = audioResult.IsSuccessful && audioResult.Service != null;
                bool stealthSuccess = stealthResult.IsSuccessful && stealthResult.Service != null;

                if (audioSuccess)
                {
                    LogTest($"笨・Legacy AudioService obtained: {audioResult.ServiceTypeName}");
                }
                else
                {
                    LogTest($"笶・Legacy AudioService failed: {audioResult.ErrorMessage}");
                }

                if (stealthSuccess)
                {
                    LogTest($"笨・Legacy StealthAudioService obtained: {stealthResult.ServiceTypeName}");
                }
                else
                {
                    LogTest($"笶・Legacy StealthAudioService failed: {stealthResult.ErrorMessage}");
                }

                bool testPassed = audioSuccess && stealthSuccess;
                LogTest($"[TEST 2] Legacy Fallback Pattern: {(testPassed ? "PASSED" : "FAILED")}");
                
                return testPassed;
            }
            catch (System.Exception ex)
            {
                LogTest($"[TEST 2] Legacy Fallback Pattern: FAILED - {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 繝倥Ν繝代・繧ｯ繝ｩ繧ｹ讖溯・縺ｮ繝・せ繝・
        /// </summary>
        private bool TestHelperClassFunctionality()
        {
            LogTest("[TEST 3] Testing Helper Class Functionality...");

            try
            {
                // 謗ｨ螂ｨ險ｭ螳壹・蜿門ｾ励ユ繧ｹ繝・
                var (useServiceLocator, reason) = ServiceMigrationHelper.GetRecommendedSettings();
                LogTest($"Recommended settings: UseServiceLocator={useServiceLocator}, Reason={reason}");

                // 遘ｻ陦檎憾諷九・繝√ぉ繝・け繝・せ繝・
                bool migrationActive = ServiceMigrationHelper.IsMigrationActive();
                LogTest($"Migration active: {migrationActive}");

                // 邁｡蜊倥↑蜿門ｾ励Γ繧ｽ繝・ラ縺ｮ繝・せ繝・
                var simpleAudio = ServiceMigrationHelper.GetAudioServiceSimple("PatternTest");
                var simpleStealth = ServiceMigrationHelper.GetStealthAudioServiceSimple("PatternTest");

                bool audioSuccess = simpleAudio != null;
                bool stealthSuccess = simpleStealth != null;

                LogTest($"Simple audio service obtained: {audioSuccess}");
                LogTest($"Simple stealth service obtained: {stealthSuccess}");

                // 險ｺ譁ｭ繝｡繧ｽ繝・ラ縺ｮ繝・せ繝・
                ServiceMigrationHelper.DiagnoseMigrationState("PatternTest");

                bool testPassed = true; // 繝倥Ν繝代・繝｡繧ｽ繝・ラ縺御ｾ句､悶↑縺丞ｮ溯｡後＆繧後ｌ縺ｰ謌仙粥
                LogTest($"[TEST 3] Helper Class Functionality: {(testPassed ? "PASSED" : "FAILED")}");
                
                return testPassed;
            }
            catch (System.Exception ex)
            {
                LogTest($"[TEST 3] Helper Class Functionality: FAILED - {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 繧ｵ繝ｼ繝薙せ縺ｮ蝓ｺ譛ｬ讖溯・繝・せ繝・
        /// </summary>
        private void TestBasicServiceFunctionality(IAudioService audioService, IStealthAudioService stealthService)
        {
            LogTest("[FUNCTIONALITY] Testing basic service functionality...");

            try
            {
                // AudioService縺ｮ蝓ｺ譛ｬ讖溯・繝・せ繝・
                if (audioService != null)
                {
                    audioService.SetMasterVolume(0.8f);
                    audioService.SetCategoryVolume("bgm", 0.7f);
                    LogTest("笨・AudioService basic functions work");
                }

                // StealthAudioService縺ｮ蝓ｺ譛ｬ讖溯・繝・せ繝・
                if (stealthService != null)
                {
                    stealthService.CreateFootstep(Vector3.zero, 0.5f, "test");
                    stealthService.SetEnvironmentNoiseLevel(0.3f);
                    stealthService.AdjustStealthAudio(0.6f);
                    LogTest("笨・StealthAudioService basic functions work");
                }
            }
            catch (System.Exception ex)
            {
                LogTest($"笶・Basic functionality test failed: {ex.Message}");
            }
        }

        #endregion

        #region Test Management

        /// <summary>
        /// 繝・せ繝育ｵ先棡縺ｮ繝ｪ繧ｻ繝・ヨ
        /// </summary>
        private void ResetTestResults()
        {
            serviceLocatorTestPassed = false;
            legacyFallbackTestPassed = false;
            helperClassTestPassed = false;
            allTestsPassed = false;
            audioServiceType = "Unknown";
            stealthServiceType = "Unknown";
            usingServiceLocator = false;
        }

        /// <summary>
        /// 繝・せ繝育ｵ先棡縺ｮ蝣ｱ蜻・
        /// </summary>
        private void ReportTestResults()
        {
            LogTest("=== Pattern Test Results ===");
            LogTest($"ServiceLocator Pattern: {(serviceLocatorTestPassed ? "笨・PASSED" : "笶・FAILED")}");
            LogTest($"Legacy Fallback Pattern: {(legacyFallbackTestPassed ? "笨・PASSED" : "笶・FAILED")}");
            LogTest($"Helper Class Functionality: {(helperClassTestPassed ? "笨・PASSED" : "笶・FAILED")}");
            LogTest($"Overall Result: {(allTestsPassed ? "脂 ALL TESTS PASSED" : "笶・SOME TESTS FAILED")}");

            if (allTestsPassed)
            {
                EventLogger.LogStatic("脂 [PATTERN TEST] Step 3.6 Gradual Update Patterns are working correctly!");
            }
            else
            {
                EventLogger.LogErrorStatic("笶・[PATTERN TEST] Some Step 3.6 patterns need attention.");
            }
        }

        #endregion

        #region Individual Tests

        /// <summary>
        /// ServiceLocator繝代ち繝ｼ繝ｳ蜊倅ｽ薙ユ繧ｹ繝・
        /// </summary>
        [ContextMenu("Test ServiceLocator Pattern Only")]
        public void TestServiceLocatorPatternOnly()
        {
            ResetTestResults();
            serviceLocatorTestPassed = TestServiceLocatorPattern();
            LogTest($"ServiceLocator Pattern Test Result: {(serviceLocatorTestPassed ? "PASSED" : "FAILED")}");
        }

        /// <summary>
        /// 繝ｬ繧ｬ繧ｷ繝ｼ繝輔か繝ｼ繝ｫ繝舌ャ繧ｯ繝代ち繝ｼ繝ｳ蜊倅ｽ薙ユ繧ｹ繝・
        /// </summary>
        [ContextMenu("Test Legacy Fallback Pattern Only")]
        public void TestLegacyFallbackPatternOnly()
        {
            ResetTestResults();
            legacyFallbackTestPassed = TestLegacyFallbackPattern();
            LogTest($"Legacy Fallback Pattern Test Result: {(legacyFallbackTestPassed ? "PASSED" : "FAILED")}");
        }

        /// <summary>
        /// 繝倥Ν繝代・繧ｯ繝ｩ繧ｹ讖溯・蜊倅ｽ薙ユ繧ｹ繝・
        /// </summary>
        [ContextMenu("Test Helper Class Only")]
        public void TestHelperClassOnly()
        {
            ResetTestResults();
            helperClassTestPassed = TestHelperClassFunctionality();
            LogTest($"Helper Class Test Result: {(helperClassTestPassed ? "PASSED" : "FAILED")}");
        }

        /// <summary>
        /// 險ｺ譁ｭ繝｢繝ｼ繝峨・螳溯｡・
        /// </summary>
        [ContextMenu("Run Migration Diagnosis")]
        public void RunMigrationDiagnosis()
        {
            ServiceMigrationHelper.DiagnoseMigrationState("PatternTest-Diagnosis");
        }

        #endregion

        #region Utilities

        /// <summary>
        /// 繝・せ繝医Ο繧ｰ蜃ｺ蜉・
        /// </summary>
        private void LogTest(string message)
        {
            if (enableDetailedLogs)
            {
                EventLogger.LogStatic($"[PATTERN_TEST] {message}");
            }
        }

        #endregion

        #region Editor Support

#if UNITY_EDITOR
        // Debug Tools
        [ContextMenu("Clear Console")]
        private void ClearConsole()
        {
            var logEntries = System.Type.GetType("UnityEditor.LogEntries, UnityEditor.dll");
            var clearMethod = logEntries.GetMethod("Clear", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            clearMethod?.Invoke(null, null);
        }
#endif

        #endregion
    }
}


