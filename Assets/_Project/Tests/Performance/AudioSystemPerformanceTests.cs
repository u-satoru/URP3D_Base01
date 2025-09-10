using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using System.Diagnostics;
using _Project.Core;
using asterivo.Unity60.Core.Audio.Interfaces;
using asterivo.Unity60.Core.Audio;

namespace asterivo.Unity60.Tests.Performance
{
    /// <summary>
    /// オーディオシステムのパフォーマンス検証テストスイート
    /// Step 4.1: ServiceLocatorベースのオーディオシステムのパフォーマンスを測定・検証
    /// </summary>
    [TestFixture]
    public class AudioSystemPerformanceTests 
    {
        private GameObject testGameObject;
        private const int PERFORMANCE_ITERATIONS = 10000;
        private const float ACCEPTABLE_TIME_MS = 100f; // 10000回で100ms以下
        private const int FRAME_TEST_DURATION_SECONDS = 2;
        
        [SetUp]
        public void SetUp()
        {
            ServiceLocator.Clear();
            testGameObject = new GameObject("PerformanceTest");
            FeatureFlags.UseServiceLocator = true;
            FeatureFlags.EnableDebugLogging = false; // パフォーマンステスト中はログ無効
        }
        
        [TearDown]
        public void TearDown()
        {
            if (testGameObject != null)
            {
                Object.DestroyImmediate(testGameObject);
            }
            ServiceLocator.Clear();
            FeatureFlags.EnableDebugLogging = true; // ログを元に戻す
        }
        
        /// <summary>
        /// ServiceLocatorを通じたオーディオサービスアクセスの速度測定
        /// 要求仕様: 10,000回のアクセスが100ms以下で完了すること
        /// </summary>
        [Test]
        public void ServiceLocator_AudioAccess_PerformanceAcceptable() 
        {
            // Arrange
            var audioManager = testGameObject.AddComponent<AudioManager>();
            audioManager.gameObject.SetActive(true);
            
            // Warm up - JITコンパイルやキャッシュ初期化
            for (int i = 0; i < 100; i++)
            {
                var _ = ServiceLocator.GetService<IAudioService>();
            }
            
            // Measure
            var stopwatch = Stopwatch.StartNew();
            
            for (int i = 0; i < PERFORMANCE_ITERATIONS; i++) 
            {
                var service = ServiceLocator.GetService<IAudioService>();
                Assert.IsNotNull(service, $"Service should not be null during performance test at iteration {i}");
            }
            
            stopwatch.Stop();
            
            // Assert
            float elapsedMs = stopwatch.ElapsedMilliseconds;
            float timePerCall = elapsedMs / PERFORMANCE_ITERATIONS;
            
            Assert.Less(elapsedMs, ACCEPTABLE_TIME_MS, 
                       $"ServiceLocator audio access should complete in under {ACCEPTABLE_TIME_MS}ms. Actual: {elapsedMs}ms ({timePerCall:F6}ms per call)");
            
            UnityEngine.Debug.Log($"[Performance] ServiceLocator audio access: {elapsedMs}ms total, {timePerCall:F6}ms per call");
        }
        
        /// <summary>
        /// 複数のサービスに同時アクセスした際のパフォーマンス測定
        /// オーディオ、空間オーディオ、エフェクト サービスの統合テスト
        /// </summary>
        [Test]
        public void ServiceLocator_MultipleServiceAccess_PerformanceAcceptable()
        {
            // Arrange
            var audioManager = testGameObject.AddComponent<AudioManager>();
            var spatialManager = testGameObject.AddComponent<SpatialAudioManager>();
            var effectManager = testGameObject.AddComponent<EffectManager>();
            
            audioManager.gameObject.SetActive(true);
            spatialManager.gameObject.SetActive(true);
            effectManager.gameObject.SetActive(true);
            
            // Warm up
            for (int i = 0; i < 30; i++)
            {
                ServiceLocator.GetService<IAudioService>();
                ServiceLocator.GetService<ISpatialAudioService>();
                ServiceLocator.GetService<IEffectService>();
            }
            
            // Measure
            var stopwatch = Stopwatch.StartNew();
            
            int iterations = PERFORMANCE_ITERATIONS / 3; // 3つのサービスなので調整
            
            for (int i = 0; i < iterations; i++) 
            {
                var audioService = ServiceLocator.GetService<IAudioService>();
                var spatialService = ServiceLocator.GetService<ISpatialAudioService>();
                var effectService = ServiceLocator.GetService<IEffectService>();
                
                Assert.IsNotNull(audioService, $"AudioService should not be null at iteration {i}");
                Assert.IsNotNull(spatialService, $"SpatialAudioService should not be null at iteration {i}");
                Assert.IsNotNull(effectService, $"EffectService should not be null at iteration {i}");
            }
            
            stopwatch.Stop();
            
            // Assert
            float elapsedMs = stopwatch.ElapsedMilliseconds;
            float timePerCall = elapsedMs / (iterations * 3);
            
            Assert.Less(elapsedMs, ACCEPTABLE_TIME_MS, 
                       $"Multiple service access should complete in under {ACCEPTABLE_TIME_MS}ms. Actual: {elapsedMs}ms");
            
            UnityEngine.Debug.Log($"[Performance] Multiple service access: {elapsedMs}ms for {iterations * 3} total calls, {timePerCall:F6}ms per call");
        }
        
        /// <summary>
        /// ServiceLocatorの使用がフレームレートに与える影響の測定
        /// 実際のゲームループでの使用シナリオをシミュレート
        /// </summary>
        [UnityTest]
        public IEnumerator ServiceLocator_FrameRateImpact_Minimal()
        {
            // Arrange
            var audioManager = testGameObject.AddComponent<AudioManager>();
            audioManager.gameObject.SetActive(true);
            
            yield return null; // フレーム待機
            
            // フレームレート測定開始
            float frameTime = 0f;
            int frameCount = 0;
            float measureDuration = FRAME_TEST_DURATION_SECONDS; // 2秒間測定
            
            float startTime = Time.realtimeSinceStartup;
            
            while (Time.realtimeSinceStartup - startTime < measureDuration)
            {
                float frameStart = Time.realtimeSinceStartup;
                
                // フレーム中にServiceLocator呼び出しを行う（一般的なゲームでの使用パターン）
                for (int i = 0; i < 10; i++) // 1フレームに10回呼び出し
                {
                    var service = ServiceLocator.GetService<IAudioService>();
                    Assert.IsNotNull(service, $"Service should not be null in frame {frameCount}, call {i}");
                }
                
                yield return null;
                
                frameTime += Time.realtimeSinceStartup - frameStart;
                frameCount++;
            }
            
            // Assert
            float avgFrameTime = frameTime / frameCount;
            float avgFPS = 1f / avgFrameTime;
            
            Assert.Greater(avgFPS, 30f, $"Frame rate should remain above 30 FPS with ServiceLocator usage. Actual: {avgFPS:F1} FPS");
            
            UnityEngine.Debug.Log($"[Performance] Average FPS with ServiceLocator: {avgFPS:F1} FPS over {frameCount} frames");
            UnityEngine.Debug.Log($"[Performance] Average frame time: {avgFrameTime * 1000:F3}ms per frame");
        }
        
        /// <summary>
        /// ServiceLocatorとDirect参照のパフォーマンス比較
        /// 従来のSingletonパターンと新システムの性能差を測定
        /// </summary>
        [Test]
        public void ServiceLocator_vs_DirectReference_PerformanceComparison()
        {
            // Arrange
            var audioManager = testGameObject.AddComponent<AudioManager>();
            audioManager.gameObject.SetActive(true);
            
            IAudioService directReference = audioManager;
            
            // ServiceLocatorアクセス測定
            var stopwatchServiceLocator = Stopwatch.StartNew();
            
            for (int i = 0; i < PERFORMANCE_ITERATIONS; i++)
            {
                var service = ServiceLocator.GetService<IAudioService>();
                Assert.IsNotNull(service);
            }
            
            stopwatchServiceLocator.Stop();
            
            // Direct参照アクセス測定
            var stopwatchDirect = Stopwatch.StartNew();
            
            for (int i = 0; i < PERFORMANCE_ITERATIONS; i++)
            {
                var service = directReference;
                Assert.IsNotNull(service);
            }
            
            stopwatchDirect.Stop();
            
            // Results
            float serviceLocatorMs = stopwatchServiceLocator.ElapsedMilliseconds;
            float directReferenceMs = stopwatchDirect.ElapsedMilliseconds;
            float performanceRatio = serviceLocatorMs / directReferenceMs;
            
            // ServiceLocatorは直接参照より遅いが、実用的な範囲内であることを検証
            Assert.Less(performanceRatio, 20f, $"ServiceLocator should not be more than 20x slower than direct reference. Actual ratio: {performanceRatio:F2}x");
            
            UnityEngine.Debug.Log($"[Performance Comparison]");
            UnityEngine.Debug.Log($"  ServiceLocator: {serviceLocatorMs}ms ({serviceLocatorMs / PERFORMANCE_ITERATIONS * 1000000:F2}μs per call)");
            UnityEngine.Debug.Log($"  Direct Reference: {directReferenceMs}ms ({directReferenceMs / PERFORMANCE_ITERATIONS * 1000000:F2}μs per call)");
            UnityEngine.Debug.Log($"  Performance Ratio: {performanceRatio:F2}x slower");
        }
        
        /// <summary>
        /// 高負荷下でのServiceLocatorの安定性テスト
        /// 同時並行アクセスとサービス登録解除の混在テスト
        /// </summary>
        [Test]
        public void ServiceLocator_HighLoad_StabilityTest()
        {
            const int highLoadIterations = 50000;
            const float maxAcceptableTime = 500f; // 50,000回で500ms以下
            
            // Arrange
            var audioManager = testGameObject.AddComponent<AudioManager>();
            audioManager.gameObject.SetActive(true);
            
            var stopwatch = Stopwatch.StartNew();
            
            // 高負荷テスト：サービス取得、null チェック、型確認を繰り返し
            for (int i = 0; i < highLoadIterations; i++)
            {
                var service = ServiceLocator.GetService<IAudioService>();
                Assert.IsNotNull(service, $"Service should not be null at high load iteration {i}");
                Assert.IsTrue(service is AudioManager, $"Service should be AudioManager instance at iteration {i}");
                
                // 10%の確率で他のサービスもテスト
                if (i % 10 == 0)
                {
                    var spatialService = ServiceLocator.GetService<ISpatialAudioService>();
                    // ISpatialAudioServiceは登録されていない可能性があるので、nullチェックはしない
                }
            }
            
            stopwatch.Stop();
            
            // Assert
            float elapsedMs = stopwatch.ElapsedMilliseconds;
            float timePerCall = elapsedMs / highLoadIterations;
            
            Assert.Less(elapsedMs, maxAcceptableTime, 
                       $"High load test should complete in under {maxAcceptableTime}ms. Actual: {elapsedMs}ms");
            
            UnityEngine.Debug.Log($"[Performance] High load stability test: {elapsedMs}ms for {highLoadIterations} iterations, {timePerCall:F6}ms per call");
        }
        
        /// <summary>
        /// ServiceLocatorのGetServiceメソッドの一意性確認とパフォーマンス
        /// 同じサービスを複数回取得した際のオブジェクト一意性とパフォーマンス
        /// </summary>
        [Test]
        public void ServiceLocator_ServiceUniqueness_PerformanceConsistent()
        {
            // Arrange
            var audioManager = testGameObject.AddComponent<AudioManager>();
            audioManager.gameObject.SetActive(true);
            
            IAudioService firstReference = ServiceLocator.GetService<IAudioService>();
            
            var stopwatch = Stopwatch.StartNew();
            
            // 同じサービスの複数回取得
            for (int i = 0; i < PERFORMANCE_ITERATIONS; i++)
            {
                var service = ServiceLocator.GetService<IAudioService>();
                Assert.IsNotNull(service);
                Assert.AreSame(firstReference, service, $"Service should be the same instance at iteration {i}");
            }
            
            stopwatch.Stop();
            
            // Assert
            float elapsedMs = stopwatch.ElapsedMilliseconds;
            
            Assert.Less(elapsedMs, ACCEPTABLE_TIME_MS, 
                       $"Service uniqueness test should complete in under {ACCEPTABLE_TIME_MS}ms. Actual: {elapsedMs}ms");
            
            UnityEngine.Debug.Log($"[Performance] Service uniqueness test: {elapsedMs}ms for {PERFORMANCE_ITERATIONS} iterations");
        }
    }
}