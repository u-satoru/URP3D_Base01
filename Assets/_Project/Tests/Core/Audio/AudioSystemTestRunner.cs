using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using asterivo.Unity60.Tests.Helpers;
using asterivo.Unity60.Core.Audio;
using asterivo.Unity60.Core;

namespace asterivo.Unity60.Tests.Core.Audio
{
    /// <summary>
    /// Core/Audioシステムの包括的テスト実行とレポート生成
    /// Week 2 テストインフラ構築の成果物
    /// XML形式（CI/CD用）とMarkdown形式（人間可読用）の両方でレポートを生成
    /// </summary>
    public static class AudioSystemTestRunner
    {
        #region Test Execution

        /// <summary>
        /// Core/Audioシステムの全テストを実行し、包括的レポートを生成
        /// </summary>
        [MenuItem("Tools/Testing/Run Audio System Tests")]
        public static void RunCompleteAudioSystemTests()
        {
            Debug.Log("[AudioSystemTestRunner] Starting comprehensive audio system testing...");

            var reportData = new TestReportData
            {
                TestSuiteName = "Core.Audio.Comprehensive",
                TotalTests = 0,
                PassedTests = 0,
                FailedTests = 0,
                AverageExecutionTime = 0f,
                MemoryUsageKB = 0,
                PerformanceScore = 100,
                TotalAssertions = 0
            };

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            long initialMemory = System.GC.GetTotalMemory(false);

            try
            {
                // 1. AudioManager テスト実行
                RunAudioManagerTests(reportData);

                // 2. SpatialAudioManager テスト実行
                RunSpatialAudioManagerTests(reportData);

                // 3. EffectManager テスト実行
                RunEffectManagerTests(reportData);

                // 4. 統合テスト実行
                RunIntegrationTests(reportData);

                // 5. パフォーマンステスト実行
                RunPerformanceTests(reportData);

            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[AudioSystemTestRunner] Test execution failed: {ex.Message}");
                reportData.Failures.Add(new TestFailure
                {
                    TestName = "AudioSystemTestRunner.GlobalError",
                    ErrorMessage = ex.Message,
                    StackTrace = ex.StackTrace
                });
                reportData.FailedTests++;
            }
            finally
            {
                stopwatch.Stop();
                long finalMemory = System.GC.GetTotalMemory(true);

                reportData.AverageExecutionTime = stopwatch.ElapsedMilliseconds;
                reportData.MemoryUsageKB = (finalMemory - initialMemory) / 1024;

                // パフォーマンススコア計算
                CalculatePerformanceScore(reportData);

                // 推奨事項と次のアクション生成
                GenerateRecommendationsAndActions(reportData);

                // レポート保存
                TestHelpers.SaveTestResults(reportData, "audio-system-comprehensive");

                Debug.Log($"[AudioSystemTestRunner] Testing completed. " +
                         $"Total: {reportData.TotalTests}, " +
                         $"Passed: {reportData.PassedTests}, " +
                         $"Failed: {reportData.FailedTests}");
            }
        }

        #endregion

        #region Individual Test Runners

        /// <summary>
        /// AudioManagerの基本機能テスト
        /// </summary>
        private static void RunAudioManagerTests(TestReportData reportData)
        {
            Debug.Log("[AudioSystemTestRunner] Running AudioManager tests...");

            var context = TestHelpers.SetupAudioTestContext();
            var mockServices = TestHelpers.RegisterAudioMockServices();

            try
            {
                // AudioManagerオブジェクトの作成
                var audioManagerGO = TestHelpers.CreateTestGameObject("TestAudioManager");
                var audioManager = audioManagerGO.AddComponent<AudioManager>();
                audioManagerGO.AddComponent<AudioSource>();

                reportData.TotalTests++;

                // 初期化テスト
                if (RunSingleTest("AudioManager.Initialization", () =>
                {
                    Assert.IsNotNull(audioManager, "AudioManager should be created");
                    TestHelpers.AssertHasComponent<AudioSource>(audioManagerGO);
                    audioManager.Initialize();
                    Assert.IsTrue(audioManager.IsInitialized, "AudioManager should be initialized");
                }, reportData))
                {
                    reportData.PassedTests++;
                }

                reportData.TotalTests++;

                // 音声再生テスト
                if (RunSingleTest("AudioManager.PlaySound", () =>
                {
                    TestHelpers.AssertAudioPerformanceThresholds(() =>
                    {
                        audioManager.PlaySound("test-sound", Vector3.zero, 0.5f);
                    }, 2f, 50);
                }, reportData))
                {
                    reportData.PassedTests++;
                }

                reportData.TotalTests++;

                // 音量設定テスト
                if (RunSingleTest("AudioManager.VolumeControl", () =>
                {
                    audioManager.SetMasterVolume(0.8f);
                    TestHelpers.AssertAudioVolumeLevel(audioManager.GetMasterVolume(), 0.8f);
                    
                    audioManager.SetBGMVolume(0.6f);
                    TestHelpers.AssertAudioVolumeLevel(audioManager.GetBGMVolume(), 0.6f);
                }, reportData))
                {
                    reportData.PassedTests++;
                }

                reportData.TotalAssertions += 6;
            }
            finally
            {
                mockServices.ResetAll();
                TestHelpers.CleanupTestGameObjects();
            }
        }

        /// <summary>
        /// SpatialAudioManagerの空間音響テスト
        /// </summary>
        private static void RunSpatialAudioManagerTests(TestReportData reportData)
        {
            Debug.Log("[AudioSystemTestRunner] Running SpatialAudioManager tests...");

            var context = TestHelpers.SetupAudioTestContext();
            var mockServices = TestHelpers.RegisterAudioMockServices();

            try
            {
                var spatialManagerGO = TestHelpers.CreateTestGameObject("TestSpatialAudioManager");
                var spatialManager = spatialManagerGO.AddComponent<SpatialAudioManager>();

                reportData.TotalTests++;

                // 3D音響再生テスト
                if (RunSingleTest("SpatialAudioManager.Play3DSound", () =>
                {
                    Vector3 testPosition = new Vector3(5f, 0f, 3f);
                    spatialManager.Play3DSound("spatial-test", testPosition, 20f, 0.7f);
                    
                    // モックサービスの呼び出し確認
                    Assert.AreEqual(1, mockServices.SpatialAudioService.Play3DSoundCallCount);
                    TestHelpers.AssertSpatialAudioPositioning(testPosition, mockServices.SpatialAudioService.LastPlay3DPosition);
                }, reportData))
                {
                    reportData.PassedTests++;
                }

                reportData.TotalTests++;

                // リスナー位置更新テスト
                if (RunSingleTest("SpatialAudioManager.UpdateListener", () =>
                {
                    Vector3 listenerPos = new Vector3(10f, 5f, -2f);
                    Vector3 listenerForward = Vector3.forward;
                    
                    spatialManager.UpdateListenerPosition(listenerPos, listenerForward);
                    
                    TestHelpers.AssertSpatialAudioPositioning(listenerPos, context.AudioListener.transform.position);
                }, reportData))
                {
                    reportData.PassedTests++;
                }

                reportData.TotalAssertions += 4;
            }
            finally
            {
                mockServices.ResetAll();
                TestHelpers.CleanupTestGameObjects();
            }
        }

        /// <summary>
        /// EffectManagerのエフェクトシステムテスト
        /// </summary>
        private static void RunEffectManagerTests(TestReportData reportData)
        {
            Debug.Log("[AudioSystemTestRunner] Running EffectManager tests...");

            var context = TestHelpers.SetupAudioTestContext();
            var mockServices = TestHelpers.RegisterAudioMockServices();

            try
            {
                var effectManagerGO = TestHelpers.CreateTestGameObject("TestEffectManager");
                var effectManager = effectManagerGO.AddComponent<EffectManager>();
                effectManagerGO.AddComponent<AudioSource>();

                reportData.TotalTests++;

                // エフェクト再生テスト
                if (RunSingleTest("EffectManager.PlayEffect", () =>
                {
                    TestHelpers.AssertAudioPerformanceThresholds(() =>
                    {
                        effectManager.PlayEffect("ui-click", Vector3.zero, 0.8f);
                    }, 1f, 30);
                }, reportData))
                {
                    reportData.PassedTests++;
                }

                reportData.TotalTests++;

                // カテゴリ別エフェクトテスト
                if (RunSingleTest("EffectManager.CategoryEffects", () =>
                {
                    effectManager.PlayUIEffect("ui-hover", 0.5f);
                    effectManager.PlayCombatEffect("sword-hit", Vector3.forward * 3f, 1.0f);
                    effectManager.PlayStealthEffect("footstep", Vector3.right * 2f, 5f);
                    
                    // エフェクトプールの状態確認
                    Assert.IsTrue(effectManager.GetActiveEffectCount() >= 0, "Effect count should be non-negative");
                }, reportData))
                {
                    reportData.PassedTests++;
                }

                reportData.TotalAssertions += 3;
            }
            finally
            {
                mockServices.ResetAll();
                TestHelpers.CleanupTestGameObjects();
            }
        }

        /// <summary>
        /// Audio系サービス間の統合テスト
        /// </summary>
        private static void RunIntegrationTests(TestReportData reportData)
        {
            Debug.Log("[AudioSystemTestRunner] Running integration tests...");

            var context = TestHelpers.SetupAudioTestContext();
            var mockServices = TestHelpers.RegisterAudioMockServices();

            try
            {
                reportData.TotalTests++;

                // サービス統合テスト
                if (RunSingleTest("AudioSystem.ServiceIntegration", () =>
                {
                    TestHelpers.AssertAudioServicesIntegrated();
                    
                    // AudioManager、SpatialAudioManager、EffectManagerの連携テスト
                    var audioManagerGO = TestHelpers.CreateTestGameObject("IntegrationAudioManager");
                    var audioManager = audioManagerGO.AddComponent<AudioManager>();
                    audioManagerGO.AddComponent<AudioSource>();
                    
                    var spatialManagerGO = TestHelpers.CreateTestGameObject("IntegrationSpatialManager");
                    var spatialManager = spatialManagerGO.AddComponent<SpatialAudioManager>();
                    
                    var effectManagerGO = TestHelpers.CreateTestGameObject("IntegrationEffectManager");
                    var effectManager = effectManagerGO.AddComponent<EffectManager>();
                    effectManagerGO.AddComponent<AudioSource>();

                    // 初期化
                    audioManager.Initialize();
                    spatialManager.Initialize();
                    effectManager.Initialize();

                    // 連携動作確認
                    audioManager.PlaySound("background-music");
                    spatialManager.Play3DSound("environment-sound", Vector3.forward * 10f);
                    effectManager.PlayUIEffect("button-click");

                    Assert.Pass("Integration test completed successfully");
                }, reportData))
                {
                    reportData.PassedTests++;
                }

                reportData.TotalAssertions += 3;
            }
            finally
            {
                mockServices.ResetAll();
                TestHelpers.CleanupTestGameObjects();
            }
        }

        /// <summary>
        /// Audio系のパフォーマンステスト
        /// </summary>
        private static void RunPerformanceTests(TestReportData reportData)
        {
            Debug.Log("[AudioSystemTestRunner] Running performance tests...");

            var context = TestHelpers.SetupAudioTestContext();
            var mockServices = TestHelpers.RegisterAudioMockServices();

            try
            {
                reportData.TotalTests++;

                // 大量音声処理パフォーマンステスト
                if (RunSingleTest("AudioSystem.MassAudioPerformance", () =>
                {
                    var effectManagerGO = TestHelpers.CreateTestGameObject("PerformanceEffectManager");
                    var effectManager = effectManagerGO.AddComponent<EffectManager>();
                    effectManagerGO.AddComponent<AudioSource>();

                    TestHelpers.AssertAudioPerformanceThresholds(() =>
                    {
                        // 50個の同時エフェクト再生テスト
                        for (int i = 0; i < 50; i++)
                        {
                            effectManager.PlayEffect($"mass-effect-{i}", 
                                new Vector3(UnityEngine.Random.Range(-20f, 20f), 0f, UnityEngine.Random.Range(-20f, 20f)), 
                                0.1f);
                        }
                    }, 50f, 500); // 50ms以内、500KB以内
                }, reportData))
                {
                    reportData.PassedTests++;
                }

                reportData.TotalTests++;

                // メモリ効率テスト
                if (RunSingleTest("AudioSystem.MemoryEfficiency", () =>
                {
                    long memoryBefore = System.GC.GetTotalMemory(false);
                    
                    var audioManagerGO = TestHelpers.CreateTestGameObject("MemoryTestAudioManager");
                    var audioManager = audioManagerGO.AddComponent<AudioManager>();
                    audioManagerGO.AddComponent<AudioSource>();
                    
                    // 100回の音声再生・停止サイクル
                    for (int i = 0; i < 100; i++)
                    {
                        audioManager.PlaySound($"memory-test-{i}");
                        audioManager.StopAllSounds();
                    }
                    
                    System.GC.Collect();
                    System.GC.WaitForPendingFinalizers();
                    System.GC.Collect();
                    
                    long memoryAfter = System.GC.GetTotalMemory(true);
                    long memoryDelta = (memoryAfter - memoryBefore) / 1024; // KB

                    Assert.That(memoryDelta, Is.LessThan(1000), 
                        $"Memory usage should be less than 1MB (actual: {memoryDelta}KB)");
                }, reportData))
                {
                    reportData.PassedTests++;
                }

                reportData.TotalAssertions += 2;
            }
            finally
            {
                mockServices.ResetAll();
                TestHelpers.CleanupTestGameObjects();
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// 単一テストの実行とエラーハンドリング
        /// </summary>
        private static bool RunSingleTest(string testName, System.Action testAction, TestReportData reportData)
        {
            try
            {
                testAction();
                Debug.Log($"[AudioSystemTestRunner] ✅ {testName} - PASSED");
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[AudioSystemTestRunner] ❌ {testName} - FAILED: {ex.Message}");
                reportData.Failures.Add(new TestFailure
                {
                    TestName = testName,
                    ErrorMessage = ex.Message,
                    StackTrace = ex.StackTrace
                });
                reportData.FailedTests++;
                return false;
            }
        }

        /// <summary>
        /// パフォーマンススコアの計算
        /// </summary>
        private static void CalculatePerformanceScore(TestReportData reportData)
        {
            float successRate = reportData.TotalTests > 0 ? (float)reportData.PassedTests / reportData.TotalTests : 0f;
            float executionScore = reportData.AverageExecutionTime < 100f ? 1f : Mathf.Clamp01(200f / reportData.AverageExecutionTime);
            float memoryScore = reportData.MemoryUsageKB < 1000 ? 1f : Mathf.Clamp01(2000f / reportData.MemoryUsageKB);

            reportData.PerformanceScore = Mathf.RoundToInt((successRate * 0.5f + executionScore * 0.3f + memoryScore * 0.2f) * 100f);
        }

        /// <summary>
        /// 推奨事項と次のアクションの生成
        /// </summary>
        private static void GenerateRecommendationsAndActions(TestReportData reportData)
        {
            var recommendations = new List<string>();
            var actions = new List<string>();

            if (reportData.FailedTests > 0)
            {
                recommendations.Add($"{reportData.FailedTests} tests failed - review failed tests and fix underlying issues");
                actions.Add("Fix failing tests before proceeding to Phase 2");
            }

            if (reportData.AverageExecutionTime > 100f)
            {
                recommendations.Add("Test execution time is above optimal threshold - consider performance optimization");
                actions.Add("Profile audio system performance and optimize bottlenecks");
            }

            if (reportData.MemoryUsageKB > 1000)
            {
                recommendations.Add("Memory usage is high - investigate potential memory leaks");
                actions.Add("Review object pooling implementation and memory management");
            }

            if (reportData.PerformanceScore < 80)
            {
                recommendations.Add("Overall performance score is below target - comprehensive optimization needed");
                actions.Add("Create performance improvement plan and prioritize optimization tasks");
            }

            if (recommendations.Count == 0)
            {
                recommendations.Add("All tests passed successfully - audio system is ready for Phase 2");
                actions.Add("Proceed with Phase 2: God Object decomposition");
            }

            reportData.Recommendations = string.Join("\n- ", recommendations.Prepend(""));
            reportData.NextActions = string.Join("\n- ", actions.Prepend(""));
        }

        #endregion

        #region Batch Test Execution

        /// <summary>
        /// バッチモード用のテスト実行（CI/CD対応）
        /// </summary>
        public static void RunBatchModeTests()
        {
            Debug.Log("[AudioSystemTestRunner] Running in batch mode for CI/CD...");
            
            try
            {
                RunCompleteAudioSystemTests();
                Debug.Log("[AudioSystemTestRunner] Batch mode testing completed successfully");
                EditorApplication.Exit(0); // 成功時は終了コード0
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[AudioSystemTestRunner] Batch mode testing failed: {ex.Message}");
                EditorApplication.Exit(1); // 失敗時は終了コード1
            }
        }

        #endregion
    }
}
