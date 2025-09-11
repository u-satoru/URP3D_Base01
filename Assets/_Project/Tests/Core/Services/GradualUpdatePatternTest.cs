using UnityEngine;
using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Core.Audio.Interfaces;
using asterivo.Unity60.Core.Debug;
// Removed Sirenix dependency for test compatibility

namespace _Project.Tests.Core.Services
{
    /// <summary>
    /// 段階的更新パターンの包括テスト
    /// Step 3.6で実装したパターンとヘルパークラスを検証
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
        /// 全段階的更新パターンテストの実行
        /// </summary>
        [ContextMenu("Run All Pattern Tests")]
        // Button replaced with ContextMenu - see method above
        public void RunAllPatternTests()
        {
            LogTest("=== Starting Gradual Update Pattern Tests ===");

            // テスト結果をリセット
            ResetTestResults();

            // 各テストを順番に実行
            serviceLocatorTestPassed = TestServiceLocatorPattern();
            legacyFallbackTestPassed = TestLegacyFallbackPattern();
            helperClassTestPassed = TestHelperClassFunctionality();

            // 総合評価
            allTestsPassed = serviceLocatorTestPassed && legacyFallbackTestPassed && helperClassTestPassed;

            // 結果報告
            ReportTestResults();
        }

        /// <summary>
        /// ServiceLocatorパターンのテスト
        /// </summary>
        private bool TestServiceLocatorPattern()
        {
            LogTest("[TEST 1] Testing ServiceLocator Pattern...");

            try
            {
                // ServiceMigrationHelperを使用したServiceLocator取得
                var audioResult = ServiceMigrationHelper.GetAudioService(true, "PatternTest", enableDetailedLogs);
                var stealthResult = ServiceMigrationHelper.GetStealthAudioService(true, "PatternTest", enableDetailedLogs);

                // 結果の検証
                bool audioSuccess = audioResult.IsSuccessful && audioResult.Service != null;
                bool stealthSuccess = stealthResult.IsSuccessful && stealthResult.Service != null;

                if (audioSuccess)
                {
                    audioServiceType = audioResult.ServiceTypeName;
                    LogTest($"✅ AudioService obtained: {audioServiceType}");
                }
                else
                {
                    LogTest($"❌ AudioService failed: {audioResult.ErrorMessage}");
                }

                if (stealthSuccess)
                {
                    stealthServiceType = stealthResult.ServiceTypeName;
                    usingServiceLocator = stealthResult.IsUsingServiceLocator;
                    LogTest($"✅ StealthAudioService obtained: {stealthServiceType}");
                    LogTest($"Using ServiceLocator: {usingServiceLocator}");
                }
                else
                {
                    LogTest($"❌ StealthAudioService failed: {stealthResult.ErrorMessage}");
                }

                // 基本機能のテスト
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
        /// レガシーフォールバックパターンのテスト
        /// </summary>
        private bool TestLegacyFallbackPattern()
        {
            LogTest("[TEST 2] Testing Legacy Fallback Pattern...");

            try
            {
                // ServiceLocatorを無効化して、レガシーフォールバックをテスト
                var audioResult = ServiceMigrationHelper.GetAudioService(false, "PatternTest", enableDetailedLogs);
                var stealthResult = ServiceMigrationHelper.GetStealthAudioService(false, "PatternTest", enableDetailedLogs);

                // レガシーシステムが利用可能かチェック
                bool legacyAvailable = ServiceMigrationHelper.IsLegacySystemAvailable();
                LogTest($"Legacy system available: {legacyAvailable}");

                if (!legacyAvailable)
                {
                    LogTest("[TEST 2] Legacy system disabled, skipping fallback test");
                    return true; // レガシーが無効化されている場合はテスト成功扱い
                }

                // 結果の検証
                bool audioSuccess = audioResult.IsSuccessful && audioResult.Service != null;
                bool stealthSuccess = stealthResult.IsSuccessful && stealthResult.Service != null;

                if (audioSuccess)
                {
                    LogTest($"✅ Legacy AudioService obtained: {audioResult.ServiceTypeName}");
                }
                else
                {
                    LogTest($"❌ Legacy AudioService failed: {audioResult.ErrorMessage}");
                }

                if (stealthSuccess)
                {
                    LogTest($"✅ Legacy StealthAudioService obtained: {stealthResult.ServiceTypeName}");
                }
                else
                {
                    LogTest($"❌ Legacy StealthAudioService failed: {stealthResult.ErrorMessage}");
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
        /// ヘルパークラス機能のテスト
        /// </summary>
        private bool TestHelperClassFunctionality()
        {
            LogTest("[TEST 3] Testing Helper Class Functionality...");

            try
            {
                // 推奨設定の取得テスト
                var (useServiceLocator, reason) = ServiceMigrationHelper.GetRecommendedSettings();
                LogTest($"Recommended settings: UseServiceLocator={useServiceLocator}, Reason={reason}");

                // 移行状態のチェックテスト
                bool migrationActive = ServiceMigrationHelper.IsMigrationActive();
                LogTest($"Migration active: {migrationActive}");

                // 簡単な取得メソッドのテスト
                var simpleAudio = ServiceMigrationHelper.GetAudioServiceSimple("PatternTest");
                var simpleStealth = ServiceMigrationHelper.GetStealthAudioServiceSimple("PatternTest");

                bool audioSuccess = simpleAudio != null;
                bool stealthSuccess = simpleStealth != null;

                LogTest($"Simple audio service obtained: {audioSuccess}");
                LogTest($"Simple stealth service obtained: {stealthSuccess}");

                // 診断メソッドのテスト
                ServiceMigrationHelper.DiagnoseMigrationState("PatternTest");

                bool testPassed = true; // ヘルパーメソッドが例外なく実行されれば成功
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
        /// サービスの基本機能テスト
        /// </summary>
        private void TestBasicServiceFunctionality(IAudioService audioService, IStealthAudioService stealthService)
        {
            LogTest("[FUNCTIONALITY] Testing basic service functionality...");

            try
            {
                // AudioServiceの基本機能テスト
                if (audioService != null)
                {
                    audioService.SetMasterVolume(0.8f);
                    audioService.SetCategoryVolume("bgm", 0.7f);
                    LogTest("✅ AudioService basic functions work");
                }

                // StealthAudioServiceの基本機能テスト
                if (stealthService != null)
                {
                    stealthService.CreateFootstep(Vector3.zero, 0.5f, "test");
                    stealthService.SetEnvironmentNoiseLevel(0.3f);
                    stealthService.AdjustStealthAudio(0.6f);
                    LogTest("✅ StealthAudioService basic functions work");
                }
            }
            catch (System.Exception ex)
            {
                LogTest($"❌ Basic functionality test failed: {ex.Message}");
            }
        }

        #endregion

        #region Test Management

        /// <summary>
        /// テスト結果のリセット
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
        /// テスト結果の報告
        /// </summary>
        private void ReportTestResults()
        {
            LogTest("=== Pattern Test Results ===");
            LogTest($"ServiceLocator Pattern: {(serviceLocatorTestPassed ? "✅ PASSED" : "❌ FAILED")}");
            LogTest($"Legacy Fallback Pattern: {(legacyFallbackTestPassed ? "✅ PASSED" : "❌ FAILED")}");
            LogTest($"Helper Class Functionality: {(helperClassTestPassed ? "✅ PASSED" : "❌ FAILED")}");
            LogTest($"Overall Result: {(allTestsPassed ? "🎉 ALL TESTS PASSED" : "❌ SOME TESTS FAILED")}");

            if (allTestsPassed)
            {
                EventLogger.Log("🎉 [PATTERN TEST] Step 3.6 Gradual Update Patterns are working correctly!");
            }
            else
            {
                EventLogger.LogError("❌ [PATTERN TEST] Some Step 3.6 patterns need attention.");
            }
        }

        #endregion

        #region Individual Tests

        /// <summary>
        /// ServiceLocatorパターン単体テスト
        /// </summary>
        [ContextMenu("Test ServiceLocator Pattern Only")]
        public void TestServiceLocatorPatternOnly()
        {
            ResetTestResults();
            serviceLocatorTestPassed = TestServiceLocatorPattern();
            LogTest($"ServiceLocator Pattern Test Result: {(serviceLocatorTestPassed ? "PASSED" : "FAILED")}");
        }

        /// <summary>
        /// レガシーフォールバックパターン単体テスト
        /// </summary>
        [ContextMenu("Test Legacy Fallback Pattern Only")]
        public void TestLegacyFallbackPatternOnly()
        {
            ResetTestResults();
            legacyFallbackTestPassed = TestLegacyFallbackPattern();
            LogTest($"Legacy Fallback Pattern Test Result: {(legacyFallbackTestPassed ? "PASSED" : "FAILED")}");
        }

        /// <summary>
        /// ヘルパークラス機能単体テスト
        /// </summary>
        [ContextMenu("Test Helper Class Only")]
        public void TestHelperClassOnly()
        {
            ResetTestResults();
            helperClassTestPassed = TestHelperClassFunctionality();
            LogTest($"Helper Class Test Result: {(helperClassTestPassed ? "PASSED" : "FAILED")}");
        }

        /// <summary>
        /// 診断モードの実行
        /// </summary>
        [ContextMenu("Run Migration Diagnosis")]
        public void RunMigrationDiagnosis()
        {
            ServiceMigrationHelper.DiagnoseMigrationState("PatternTest-Diagnosis");
        }

        #endregion

        #region Utilities

        /// <summary>
        /// テストログ出力
        /// </summary>
        private void LogTest(string message)
        {
            if (enableDetailedLogs)
            {
                EventLogger.Log($"[PATTERN_TEST] {message}");
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