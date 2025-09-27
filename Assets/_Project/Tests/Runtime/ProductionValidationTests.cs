using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Audio.Interfaces;
using asterivo.Unity60.Core.Audio;
// using asterivo.Unity60.Core.Shared; // Not needed - types are in Core namespace

namespace asterivo.Unity60.Tests.Runtime
{
    /// <summary>
    /// 本番環境での安定動作確認テストスイート
    /// Phase 4: Task 6 - 本番環境での安定動作確認
    /// </summary>
    public class ProductionValidationTests : MonoBehaviour
    {
        [Header("Test Configuration")]
        [SerializeField] private bool autoRunOnStart = true;
        [SerializeField] private float testInterval = 1f;
        [SerializeField] private int maxTestCycles = 5;

        [Header("Test Results")]
        [SerializeField] private bool allTestsPassed = false;
        [SerializeField] private List<string> testResults = new List<string>();
        [SerializeField] private List<string> errorMessages = new List<string>();

        private int currentTestCycle = 0;

        #region Unity Lifecycle

        private void Start()
        {
            if (autoRunOnStart)
            {
                StartCoroutine(RunProductionValidationSuite());
            }
        }

        #endregion

        #region Public Interface

        /// <summary>
        /// 本番環境検証テストスイートを手動実行
        /// </summary>
        [ContextMenu("Run Production Validation")]
        public void RunProductionValidation()
        {
            StartCoroutine(RunProductionValidationSuite());
        }

        /// <summary>
        /// テスト結果をコンソールに出力
        /// </summary>
        [ContextMenu("Print Test Results")]
        public void PrintTestResults()
        {
            Debug.Log("[ProductionValidation] === Test Results ===");
            Debug.Log($"Overall Result: {(allTestsPassed ? "PASSED" : "FAILED")}");
            Debug.Log($"Test Cycles Completed: {currentTestCycle}/{maxTestCycles}");
            
            Debug.Log("Test Results:");
            foreach (var result in testResults)
            {
                Debug.Log($"  {result}");
            }

            if (errorMessages.Count > 0)
            {
                Debug.LogError("Error Messages:");
                foreach (var error in errorMessages)
                {
                    Debug.LogError($"  {error}");
                }
            }
        }

        #endregion

        #region Test Suite

        /// <summary>
        /// 本番環境検証テストスイートのメイン実行
        /// </summary>
        private IEnumerator RunProductionValidationSuite()
        {
            Debug.Log("[ProductionValidation] Starting Production Validation Test Suite");
            
            // テスト結果をリセット
            testResults.Clear();
            errorMessages.Clear();
            allTestsPassed = true;
            currentTestCycle = 0;

            // Phase 4 検証テストを順次実行
            yield return StartCoroutine(ValidateFeatureFlags());
            yield return StartCoroutine(ValidateServiceLocatorStatus());
            yield return StartCoroutine(ValidateAllServicesRegistration());
            yield return StartCoroutine(ValidateServiceFunctionality());
            yield return StartCoroutine(ValidatePerformanceBaseline());
            yield return StartCoroutine(ValidateMemoryStability());
            yield return StartCoroutine(ValidateErrorHandling());
            
            // 継続的安定性テスト
            for (currentTestCycle = 1; currentTestCycle <= maxTestCycles; currentTestCycle++)
            {
                Debug.Log($"[ProductionValidation] Running stability test cycle {currentTestCycle}/{maxTestCycles}");
                yield return StartCoroutine(RunStabilityCycle());
                yield return new WaitForSeconds(testInterval);
            }

            // 最終結果判定
            FinalizeTestResults();
        }

        /// <summary>
        /// FeatureFlagsの設定確認
        /// </summary>
        private IEnumerator ValidateFeatureFlags()
        {
            Debug.Log("[ProductionValidation] Validating FeatureFlags configuration...");
            
            try
            {
                // Phase 3 完了状態の確認
                bool flagsValid = true;
                string flagStatus = "";

                // 必須フラグの確認
                if (!FeatureFlags.UseServiceLocator) { flagsValid = false; flagStatus += "UseServiceLocator=false "; }
                if (!FeatureFlags.UseNewAudioService) { flagsValid = false; flagStatus += "UseNewAudioService=false "; }
                if (!FeatureFlags.UseNewSpatialService) { flagsValid = false; flagStatus += "UseNewSpatialService=false "; }
                if (!FeatureFlags.UseNewStealthService) { flagsValid = false; flagStatus += "UseNewStealthService=false "; }
                if (!FeatureFlags.DisableLegacySingletons) { flagsValid = false; flagStatus += "DisableLegacySingletons=false "; }

                if (flagsValid)
                {
                    AddTestResult("✅ FeatureFlags: All Phase 3 flags correctly configured");
                }
                else
                {
                    AddTestError($"❌ FeatureFlags: Invalid configuration - {flagStatus}");
                }

                // フラグの詳細ログ出力
                FeatureFlags.LogCurrentFlags();
                
            }
            catch (System.Exception ex)
            {
                AddTestError($"❌ FeatureFlags validation failed: {ex.Message}");
            }

            yield return null;
        }

        /// <summary>
        /// ServiceLocatorの状態確認
        /// </summary>
        private IEnumerator ValidateServiceLocatorStatus()
        {
            Debug.Log("[ProductionValidation] Validating ServiceLocator status...");
            
            try
            {
                bool serviceLocatorWorking = true;
                
                // ServiceLocatorの基本動作確認
                if (ServiceLocator.HasService<IAudioService>() &&
                    ServiceLocator.HasService<ISpatialAudioService>() &&
                    ServiceLocator.HasService<IEffectService>() &&
                    ServiceLocator.HasService<IAudioUpdateService>())
                {
                    AddTestResult("✅ ServiceLocator: All core services registered");
                }
                else
                {
                    AddTestError("❌ ServiceLocator: Missing core service registrations");
                    serviceLocatorWorking = false;
                }

                // サービス取得の動作確認
                if (serviceLocatorWorking)
                {
                    var audioService = ServiceLocator.GetService<IAudioService>();
                    var spatialService = ServiceLocator.GetService<ISpatialAudioService>();
                    var effectService = ServiceLocator.GetService<IEffectService>();
                    var updateService = ServiceLocator.GetService<IAudioUpdateService>();

                    if (audioService != null && spatialService != null && effectService != null && updateService != null)
                    {
                        AddTestResult("✅ ServiceLocator: All services retrievable");
                    }
                    else
                    {
                        AddTestError("❌ ServiceLocator: Failed to retrieve some services");
                    }
                }
            }
            catch (System.Exception ex)
            {
                AddTestError($"❌ ServiceLocator validation failed: {ex.Message}");
            }

            yield return null;
        }

        /// <summary>
        /// 全サービスの登録状況確認
        /// </summary>
        private IEnumerator ValidateAllServicesRegistration()
        {
            Debug.Log("[ProductionValidation] Validating all services registration...");
            
            try
            {
                // 各サービスの登録確認
                int registeredServices = 0;
                List<string> missingServices = new List<string>();

                if (ServiceLocator.HasService<IAudioService>()) 
                    registeredServices++;
                else 
                    missingServices.Add("IAudioService");

                if (ServiceLocator.HasService<ISpatialAudioService>()) 
                    registeredServices++;
                else 
                    missingServices.Add("ISpatialAudioService");

                if (ServiceLocator.HasService<IEffectService>()) 
                    registeredServices++;
                else 
                    missingServices.Add("IEffectService");

                if (ServiceLocator.HasService<IAudioUpdateService>()) 
                    registeredServices++;
                else 
                    missingServices.Add("IAudioUpdateService");

                if (registeredServices == 4)
                {
                    AddTestResult($"✅ Service Registration: All {registeredServices} services registered");
                }
                else
                {
                    AddTestError($"❌ Service Registration: Only {registeredServices}/4 services registered. Missing: {string.Join(", ", missingServices)}");
                }
            }
            catch (System.Exception ex)
            {
                AddTestError($"❌ Service registration validation failed: {ex.Message}");
            }

            yield return null;
        }

        /// <summary>
        /// サービス機能性確認
        /// </summary>
        private IEnumerator ValidateServiceFunctionality()
        {
            Debug.Log("[ProductionValidation] Validating service functionality...");
            
            try
            {
                var audioService = ServiceLocator.GetService<IAudioService>();
                var spatialService = ServiceLocator.GetService<ISpatialAudioService>();
                var effectService = ServiceLocator.GetService<IEffectService>();
                var updateService = ServiceLocator.GetService<IAudioUpdateService>();

                bool functionalityPassed = true;

                // AudioService functionality
                try
                {
                    audioService.SetMasterVolume(0.7f);
                    float volume = audioService.GetMasterVolume();
                    if (Mathf.Abs(volume - 0.7f) < 0.1f)
                    {
                        AddTestResult("✅ AudioService: Volume control working");
                    }
                    else
                    {
                        AddTestError($"❌ AudioService: Volume control failed - expected 0.7, got {volume}");
                        functionalityPassed = false;
                    }
                }
                catch (System.Exception ex)
                {
                    AddTestError($"❌ AudioService functionality failed: {ex.Message}");
                    functionalityPassed = false;
                }

                // SpatialAudioService functionality
                try
                {
                    spatialService.UpdateListenerPosition(Vector3.zero, Vector3.forward);
                    spatialService.SetDopplerLevel(1.0f);
                    AddTestResult("✅ SpatialAudioService: Basic operations working");
                }
                catch (System.Exception ex)
                {
                    AddTestError($"❌ SpatialAudioService functionality failed: {ex.Message}");
                    functionalityPassed = false;
                }

                // EffectService functionality
                try
                {
                    effectService.SetEffectPitch("test", 1.0f);
                    effectService.ClearEffectPool();
                    AddTestResult("✅ EffectService: Basic operations working");
                }
                catch (System.Exception ex)
                {
                    AddTestError($"❌ EffectService functionality failed: {ex.Message}");
                    functionalityPassed = false;
                }

                // AudioUpdateService functionality
                try
                {
                    float interval = updateService.UpdateInterval;
                    bool coordinated = updateService.IsCoordinatedUpdateEnabled;
                    updateService.ForceRebuildSpatialCache();
                    AddTestResult("✅ AudioUpdateService: Basic operations working");
                }
                catch (System.Exception ex)
                {
                    AddTestError($"❌ AudioUpdateService functionality failed: {ex.Message}");
                    functionalityPassed = false;
                }

                if (functionalityPassed)
                {
                    AddTestResult("✅ Service Functionality: All services operational");
                }
            }
            catch (System.Exception ex)
            {
                AddTestError($"❌ Service functionality validation failed: {ex.Message}");
            }

            yield return null;
        }

        /// <summary>
        /// パフォーマンスベースライン確認
        /// </summary>
        private IEnumerator ValidatePerformanceBaseline()
        {
            Debug.Log("[ProductionValidation] Validating performance baseline...");
            
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            bool performanceTestPassed = true;
            
            // パフォーマンステスト実行
            const int iterations = 1000;
            for (int i = 0; i < iterations; i++)
            {
                try
                {
                    var audioService = ServiceLocator.GetService<IAudioService>();
                    var spatialService = ServiceLocator.GetService<ISpatialAudioService>();
                    var effectService = ServiceLocator.GetService<IEffectService>();
                    
                    if (audioService == null || spatialService == null || effectService == null)
                    {
                        AddTestError("❌ Performance: Service retrieval failed during stress test");
                        performanceTestPassed = false;
                        break;
                    }
                }
                catch (System.Exception ex)
                {
                    AddTestError($"❌ Performance validation failed at iteration {i}: {ex.Message}");
                    performanceTestPassed = false;
                    break;
                }
                
                if (i % 100 == 0)
                {
                    yield return null; // フレーム待機
                }
            }
            
            if (performanceTestPassed)
            {
                stopwatch.Stop();
                float avgTimePerCall = stopwatch.ElapsedMilliseconds / (float)iterations;
                
                if (avgTimePerCall < 0.1f) // 0.1ms以下
                {
                    AddTestResult($"✅ Performance: Service access time acceptable ({avgTimePerCall:F6}ms per call)");
                }
                else
                {
                    AddTestError($"❌ Performance: Service access time too slow ({avgTimePerCall:F6}ms per call)");
                }
            }
            
            yield return null;
        }

        /// <summary>
        /// メモリ安定性確認
        /// </summary>
        private IEnumerator ValidateMemoryStability()
        {
            Debug.Log("[ProductionValidation] Validating memory stability...");
            
            long initialMemory = 0;
            bool memoryTestPassed = true;
            
            try
            {
                initialMemory = System.GC.GetTotalMemory(true);
            }
            catch (System.Exception ex)
            {
                AddTestError($"❌ Memory stability initial memory check failed: {ex.Message}");
                memoryTestPassed = false;
            }
            
            if (memoryTestPassed)
            {
                // メモリストレステスト
                for (int cycle = 0; cycle < 10; cycle++)
                {
                    try
                    {
                        for (int i = 0; i < 100; i++)
                        {
                            var service = ServiceLocator.GetService<IAudioService>();
                            if (service != null)
                            {
                                service.PlaySound($"memory_test_{i}", Vector3.zero, 0.1f);
                            }
                        }
                    }
                    catch (System.Exception ex)
                    {
                        AddTestError($"❌ Memory stability test failed at cycle {cycle}: {ex.Message}");
                        memoryTestPassed = false;
                        break;
                    }
                    
                    yield return null;
                }
                
                if (memoryTestPassed)
                {
                    // ガベージコレクションを強制実行
                    try
                    {
                        System.GC.Collect();
                        System.GC.WaitForPendingFinalizers();
                        System.GC.Collect();
                    }
                    catch (System.Exception ex)
                    {
                        AddTestError($"❌ Memory stability GC failed: {ex.Message}");
                        memoryTestPassed = false;
                    }
                    
                    yield return new WaitForSeconds(0.1f);
                    
                    if (memoryTestPassed)
                    {
                        try
                        {
                            long finalMemory = System.GC.GetTotalMemory(true);
                            long memoryDiff = finalMemory - initialMemory;
                            
                            if (memoryDiff < 1024 * 1024) // 1MB以下の増加
                            {
                                AddTestResult($"✅ Memory Stability: Memory usage stable ({memoryDiff / 1024}KB increase)");
                            }
                            else
                            {
                                AddTestError($"❌ Memory Stability: Excessive memory usage ({memoryDiff / 1024}KB increase)");
                            }
                        }
                        catch (System.Exception ex)
                        {
                            AddTestError($"❌ Memory stability final check failed: {ex.Message}");
                        }
                    }
                }
            }
            
            yield return null;
        }

        /// <summary>
        /// エラーハンドリング確認
        /// </summary>
        private IEnumerator ValidateErrorHandling()
        {
            Debug.Log("[ProductionValidation] Validating error handling...");
            
            try
            {
                var audioService = ServiceLocator.GetService<IAudioService>();
                var spatialService = ServiceLocator.GetService<ISpatialAudioService>();
                var effectService = ServiceLocator.GetService<IEffectService>();

                bool errorHandlingPassed = true;

                // 無効な引数でのテスト
                try
                {
                    audioService.PlaySound(null);
                    audioService.SetMasterVolume(-1f);
                    spatialService.Play3DSound("", Vector3.zero);
                    effectService.PlayRandomEffect(null);
                    
                    AddTestResult("✅ Error Handling: Invalid inputs handled gracefully");
                }
                catch (System.Exception ex)
                {
                    AddTestError($"❌ Error Handling: Exception thrown for invalid inputs: {ex.Message}");
                    errorHandlingPassed = false;
                }

                if (errorHandlingPassed)
                {
                    AddTestResult("✅ Error Handling: All error cases handled properly");
                }
            }
            catch (System.Exception ex)
            {
                AddTestError($"❌ Error handling validation failed: {ex.Message}");
            }

            yield return null;
        }

        /// <summary>
        /// 安定性サイクルテスト
        /// </summary>
        private IEnumerator RunStabilityCycle()
        {
            try
            {
                // 基本的なサービス操作を実行
                var audioService = ServiceLocator.GetService<IAudioService>();
                var spatialService = ServiceLocator.GetService<ISpatialAudioService>();
                var effectService = ServiceLocator.GetService<IEffectService>();
                var updateService = ServiceLocator.GetService<IAudioUpdateService>();

                if (audioService == null || spatialService == null || effectService == null || updateService == null)
                {
                    AddTestError($"❌ Stability Cycle {currentTestCycle}: Service unavailable");
                    yield break;
                }

                // 各サービスの操作を実行
                audioService.SetMasterVolume(0.5f + (currentTestCycle * 0.1f) % 0.5f);
                spatialService.UpdateListenerPosition(Vector3.zero, Vector3.forward);
                effectService.ClearEffectPool();
                updateService.ForceRebuildSpatialCache();

                AddTestResult($"✅ Stability Cycle {currentTestCycle}: All services operational");
                
            }
            catch (System.Exception ex)
            {
                AddTestError($"❌ Stability Cycle {currentTestCycle} failed: {ex.Message}");
            }

            yield return null;
        }

        /// <summary>
        /// 最終テスト結果の確定
        /// </summary>
        private void FinalizeTestResults()
        {
            allTestsPassed = errorMessages.Count == 0;
            
            Debug.Log($"[ProductionValidation] === FINAL RESULTS ===");
            Debug.Log($"Overall Status: {(allTestsPassed ? "✅ PASSED" : "❌ FAILED")}");
            Debug.Log($"Tests Passed: {testResults.Count}");
            Debug.Log($"Tests Failed: {errorMessages.Count}");
            Debug.Log($"Stability Cycles: {currentTestCycle}/{maxTestCycles}");

            if (allTestsPassed)
            {
                Debug.Log("🎉 Production validation completed successfully! System is stable for production use.");
            }
            else
            {
                Debug.LogError("⚠️ Production validation failed! Please review errors before deploying.");
                
                Debug.LogError("Failed Tests:");
                foreach (var error in errorMessages)
                {
                    Debug.LogError($"  {error}");
                }
            }
        }

        #endregion

        #region Helper Methods

        private void AddTestResult(string result)
        {
            testResults.Add(result);
            Debug.Log($"[ProductionValidation] {result}");
        }

        private void AddTestError(string error)
        {
            errorMessages.Add(error);
            allTestsPassed = false;
            Debug.LogError($"[ProductionValidation] {error}");
        }

        #endregion

        #region Editor Helpers

#if UNITY_EDITOR
        [ContextMenu("Reset Test Results")]
        public void ResetTestResults()
        {
            testResults.Clear();
            errorMessages.Clear();
            allTestsPassed = false;
            currentTestCycle = 0;
            Debug.Log("[ProductionValidation] Test results reset");
        }
#endif

        #endregion
    }
}
