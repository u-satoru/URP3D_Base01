using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Audio.Interfaces;
using asterivo.Unity60.Core.Audio;

namespace asterivo.Unity60.Tests.Performance
{
    /// <summary>
    /// ServiceLocatorストレステストスイート
    /// Step 4.1: 高負荷状況下でのServiceLocatorの安定性とパフォーマンス検証
    /// </summary>
    [TestFixture]
    public class ServiceLocatorStressTests
    {
        private GameObject testGameObject;
        private List<GameObject> testObjects;
        
        private const int STRESS_ITERATIONS = 100000;
        private const int CONCURRENT_SERVICES_COUNT = 50;
        private const float MAX_STRESS_TIME_SECONDS = 10f;
        
        [SetUp]
        public void SetUp()
        {
            ServiceLocator.Clear();
            testGameObject = new GameObject("StressTest");
            testObjects = new List<GameObject>();
            FeatureFlags.UseServiceLocator = true;
            FeatureFlags.EnableDebugLogging = false; // ストレステスト中はログ無効
        }
        
        [TearDown]
        public void TearDown()
        {
            if (testGameObject != null)
            {
                Object.DestroyImmediate(testGameObject);
            }
            
            foreach (var obj in testObjects)
            {
                if (obj != null)
                {
                    Object.DestroyImmediate(obj);
                }
            }
            testObjects.Clear();
            
            ServiceLocator.Clear();
            FeatureFlags.EnableDebugLogging = true;
        }
        
        /// <summary>
        /// 極端に高い頻度でのサービス取得ストレステスト
        /// CPUとメモリの負荷限界をテスト
        /// </summary>
        [Test]
        public void ServiceLocator_ExtremeHighFrequency_StressTest()
        {
            // Arrange
            var audioManager = testGameObject.AddComponent<TestAudioService>();
            audioManager.gameObject.SetActive(true);
            
            var stopwatch = Stopwatch.StartNew();
            int successfulCalls = 0;
            int failedCalls = 0;
            
            // Extreme high frequency access
            for (int i = 0; i < STRESS_ITERATIONS; i++)
            {
                try
                {
                    var service = ServiceLocator.GetService<IAudioService>();
                    if (service != null)
                    {
                        successfulCalls++;
                    }
                    else
                    {
                        failedCalls++;
                    }
                }
                catch (System.Exception ex)
                {
                    failedCalls++;
                    UnityEngine.Debug.LogWarning($"Exception at iteration {i}: {ex.Message}");
                }
            }
            
            stopwatch.Stop();
            
            // Assert
            float elapsedSeconds = stopwatch.ElapsedMilliseconds / 1000f;
            Assert.Less(elapsedSeconds, MAX_STRESS_TIME_SECONDS, 
                       $"Extreme stress test should complete within {MAX_STRESS_TIME_SECONDS} seconds. Actual: {elapsedSeconds:F2}s");
            
            Assert.Greater(successfulCalls, STRESS_ITERATIONS * 0.95f, 
                          $"At least 95% of calls should succeed. Success rate: {(float)successfulCalls / STRESS_ITERATIONS * 100:F1}%");
            
            float callsPerSecond = STRESS_ITERATIONS / elapsedSeconds;
            
            UnityEngine.Debug.Log($"[Stress] Extreme frequency test: {STRESS_ITERATIONS} calls in {elapsedSeconds:F2}s ({callsPerSecond:F0} calls/sec)");
            UnityEngine.Debug.Log($"[Stress] Success: {successfulCalls}, Failed: {failedCalls}, Success rate: {(float)successfulCalls / STRESS_ITERATIONS * 100:F1}%");
        }
        
        /// <summary>
        /// 複数サービスの同時大量登録と取得のストレステスト
        /// システムの同時並行処理能力をテスト
        /// </summary>
        [Test]
        public void ServiceLocator_MassiveConcurrentServices_StressTest()
        {
            var stopwatch = Stopwatch.StartNew();
            
            // 大量のサービスを同時登録
            for (int i = 0; i < CONCURRENT_SERVICES_COUNT; i++)
            {
                var serviceObj = new GameObject($"ConcurrentService_{i}");
                testObjects.Add(serviceObj);
                
                // 異なるタイプのサービスをランダムに登録
                switch (i % 3)
                {
                    case 0:
                        var audioService = serviceObj.AddComponent<TestAudioService>();
                        ServiceLocator.RegisterService<IAudioService>(audioService);
                        break;
                    case 1:
                        var spatialService = serviceObj.AddComponent<TestSpatialAudioService>();
                        ServiceLocator.RegisterService<ISpatialAudioService>(spatialService);
                        break;
                    case 2:
                        var effectService = serviceObj.AddComponent<TestEffectService>();
                        ServiceLocator.RegisterService<IEffectService>(effectService);
                        break;
                }
            }
            
            // 全サービスに対して大量アクセス
            for (int i = 0; i < 1000; i++)
            {
                var audioService = ServiceLocator.GetService<IAudioService>();
                var spatialService = ServiceLocator.GetService<ISpatialAudioService>();
                var effectService = ServiceLocator.GetService<IEffectService>();
                
                // サービスの存在確認
                Assert.IsNotNull(audioService, $"AudioService should be available at access {i}");
                
                // サービスの基本機能テスト
                if (i % 100 == 0) // 100回に1回詳細テスト
                {
                    audioService.PlaySound($"stress_test_sound_{i}");
                    if (spatialService != null)
                    {
                        spatialService.Play3DSound($"spatial_sound_{i}", Vector3.zero);
                    }
                    if (effectService != null)
                    {
                        effectService.PlayEffect($"effect_{i}", Vector3.zero);
                    }
                }
            }
            
            stopwatch.Stop();
            
            // Assert
            float elapsedSeconds = stopwatch.ElapsedMilliseconds / 1000f;
            Assert.Less(elapsedSeconds, MAX_STRESS_TIME_SECONDS, 
                       $"Massive concurrent services test should complete within {MAX_STRESS_TIME_SECONDS} seconds. Actual: {elapsedSeconds:F2}s");
            
            UnityEngine.Debug.Log($"[Stress] Massive concurrent services test: {CONCURRENT_SERVICES_COUNT} services, completed in {elapsedSeconds:F2}s");
        }
        
        /// <summary>
        /// サービス登録と解除の高頻度繰り返しストレステスト
        /// メモリ断片化と登録解除処理の安定性をテスト
        /// </summary>
        [Test]
        public void ServiceLocator_HighFrequencyRegistrationUnregistration_StressTest()
        {
            var stopwatch = Stopwatch.StartNew();
            int registrationCycles = 1000;
            
            for (int cycle = 0; cycle < registrationCycles; cycle++)
            {
                // サービス登録
                var serviceObj = new GameObject($"CycleService_{cycle}");
                var audioService = serviceObj.AddComponent<TestAudioService>();
                ServiceLocator.RegisterService<IAudioService>(audioService);
                
                // サービス使用
                var retrievedService = ServiceLocator.GetService<IAudioService>();
                Assert.IsNotNull(retrievedService, $"Service should be retrievable at cycle {cycle}");
                Assert.AreSame(audioService, retrievedService, $"Retrieved service should match registered service at cycle {cycle}");
                
                // サービスの機能実行
                retrievedService.PlaySound($"cycle_test_{cycle}");
                
                // サービス解除
                Object.DestroyImmediate(serviceObj);
                ServiceLocator.Clear();
                
                // 解除後の状態確認
                var clearedService = ServiceLocator.GetService<IAudioService>();
                Assert.IsNull(clearedService, $"Service should be null after clear at cycle {cycle}");
            }
            
            stopwatch.Stop();
            
            // Assert
            float elapsedSeconds = stopwatch.ElapsedMilliseconds / 1000f;
            Assert.Less(elapsedSeconds, MAX_STRESS_TIME_SECONDS, 
                       $"High frequency registration/unregistration test should complete within {MAX_STRESS_TIME_SECONDS} seconds. Actual: {elapsedSeconds:F2}s");
            
            float cyclesPerSecond = registrationCycles / elapsedSeconds;
            
            UnityEngine.Debug.Log($"[Stress] Registration/Unregistration cycles: {registrationCycles} in {elapsedSeconds:F2}s ({cyclesPerSecond:F1} cycles/sec)");
        }
        
        /// <summary>
        /// エラー状況下でのServiceLocatorの安定性テスト
        /// 異常な状況や不正な操作に対する堅牢性をテスト
        /// </summary>
        [Test]
        public void ServiceLocator_ErrorConditions_StabilityTest()
        {
            int successfulOperations = 0;
            int expectedExceptions = 0;
            int unexpectedExceptions = 0;
            
            var stopwatch = Stopwatch.StartNew();
            
            for (int i = 0; i < 1000; i++)
            {
                try
                {
                    // 正常なサービス操作
                    if (i % 5 == 0)
                    {
                        var serviceObj = new GameObject($"ErrorTestService_{i}");
                        testObjects.Add(serviceObj);
                        var audioService = serviceObj.AddComponent<TestAudioService>();
                        ServiceLocator.RegisterService<IAudioService>(audioService);
                        successfulOperations++;
                    }
                    
                    // サービス取得
                    var service = ServiceLocator.GetService<IAudioService>();
                    if (service != null)
                    {
                        service.PlaySound($"error_test_{i}");
                        successfulOperations++;
                    }
                    
                    // 存在しないサービスの取得（エラーではない）
                    var nonExistentService = ServiceLocator.GetService<ISpatialAudioService>();
                    successfulOperations++; // nullが返されることは正常
                    
                    // ランダムなClear操作
                    if (i % 10 == 0)
                    {
                        ServiceLocator.Clear();
                        successfulOperations++;
                    }
                }
                catch (System.ArgumentException)
                {
                    expectedExceptions++; // 予期される例外
                }
                catch (System.Exception ex)
                {
                    unexpectedExceptions++;
                    UnityEngine.Debug.LogWarning($"Unexpected exception at iteration {i}: {ex.Message}");
                }
            }
            
            stopwatch.Stop();
            
            // Assert
            float elapsedSeconds = stopwatch.ElapsedMilliseconds / 1000f;
            Assert.Less(elapsedSeconds, MAX_STRESS_TIME_SECONDS, 
                       $"Error conditions test should complete within {MAX_STRESS_TIME_SECONDS} seconds. Actual: {elapsedSeconds:F2}s");
            
            Assert.LessOrEqual(unexpectedExceptions, 5, 
                              $"Unexpected exceptions should be minimal. Count: {unexpectedExceptions}");
            
            float successRate = (float)successfulOperations / (successfulOperations + expectedExceptions + unexpectedExceptions) * 100;
            Assert.Greater(successRate, 90f, 
                          $"Success rate should be above 90%. Actual: {successRate:F1}%");
            
            UnityEngine.Debug.Log($"[Stress] Error conditions test: {successfulOperations} successful, {expectedExceptions} expected errors, {unexpectedExceptions} unexpected errors");
            UnityEngine.Debug.Log($"[Stress] Success rate: {successRate:F1}%, Duration: {elapsedSeconds:F2}s");
        }
        
        /// <summary>
        /// 長時間連続実行でのServiceLocatorの安定性テスト
        /// メモリリークや性能劣化がないことを確認
        /// </summary>
        [UnityTest]
        public IEnumerator ServiceLocator_LongTermStability_Test()
        {
            const int testDurationSeconds = 5; // 5秒間の連続テスト
            const int operationsPerFrame = 100;
            
            // 初期メモリ状態を記録
            System.GC.Collect();
            long initialMemory = System.GC.GetTotalMemory(false);
            
            var audioManager = testGameObject.AddComponent<TestAudioService>();
            audioManager.gameObject.SetActive(true);
            
            float startTime = Time.realtimeSinceStartup;
            int totalOperations = 0;
            int frameCount = 0;
            
            while (Time.realtimeSinceStartup - startTime < testDurationSeconds)
            {
                // 毎フレーム一定数の操作を実行
                for (int i = 0; i < operationsPerFrame; i++)
                {
                    var service = ServiceLocator.GetService<IAudioService>();
                    Assert.IsNotNull(service, $"Service should be available during long-term test at operation {totalOperations}");
                    
                    service.PlaySound($"longterm_test_{totalOperations}");
                    totalOperations++;
                }
                
                frameCount++;
                yield return null; // 次のフレームまで待機
            }
            
            // 最終メモリ状態をチェック
            System.GC.Collect();
            long finalMemory = System.GC.GetTotalMemory(false);
            long memoryDiff = finalMemory - initialMemory;
            
            float actualDuration = Time.realtimeSinceStartup - startTime;
            float operationsPerSecond = totalOperations / actualDuration;
            
            // Assert
            Assert.Less(memoryDiff, 10 * 1024 * 1024, // 10MB以下の増加
                       $"Memory increase should be reasonable. Actual increase: {memoryDiff / 1024 / 1024:F1}MB");
            
            Assert.Greater(operationsPerSecond, 1000f, 
                          $"Should maintain reasonable throughput. Actual: {operationsPerSecond:F0} ops/sec");
            
            UnityEngine.Debug.Log($"[Stress] Long-term stability test: {totalOperations} operations over {actualDuration:F1}s ({operationsPerSecond:F0} ops/sec)");
            UnityEngine.Debug.Log($"[Stress] Memory change: {memoryDiff / 1024:F0}KB, Frames: {frameCount}");
        }
    }
    
    #region Test Helper Services
    
    /// <summary>
    /// ストレステスト用シンプルなオーディオサービス実装
    /// </summary>
    public class TestAudioService : MonoBehaviour, IAudioService, IInitializable
    {
        public int Priority => 0;
        public bool IsInitialized { get; private set; } = false;
        
        void Start()
        {
            Initialize();
        }
        
        public void Initialize()
        {
            IsInitialized = true;
        }
        
        public void PlaySound(string soundId, Vector3 position = default, float volume = 1f) 
        {
            // ストレステスト用のダミー実装
        }
        
        public void StopSound(string soundId) { }
        public void StopAllSounds() { }
        public float GetMasterVolume() => 1f;
        public void SetMasterVolume(float volume) { }
        public float GetBGMVolume() => 1f;
        public float GetAmbientVolume() => 1f;
        public float GetEffectVolume() => 1f;
        public void SetCategoryVolume(string category, float volume) { }
        public bool IsPlaying(string soundId) => false;
        public void Pause() { }
        public void Resume() { }
    }
    
    /// <summary>
    /// ストレステスト用空間オーディオサービス実装
    /// </summary>
    public class TestSpatialAudioService : MonoBehaviour, ISpatialAudioService, IInitializable
    {
        public int Priority => 0;
        public bool IsInitialized { get; private set; } = false;
        
        void Start()
        {
            Initialize();
        }
        
        public void Initialize()
        {
            IsInitialized = true;
        }
        
        public void Play3DSound(string soundId, Vector3 position, float maxDistance = 50f, float volume = 1f) { }
        public void CreateMovingSound(string soundId, Transform source, float maxDistance = 50f) { }
        public void SetAmbientSound(string soundId, float volume = 0.5f) { }
        public void UpdateOcclusion(Vector3 listenerPosition, Vector3 sourcePosition, float occlusionLevel) { }
        public void SetReverbZone(string zoneId, float reverbLevel) { }
        public void SetDopplerLevel(float level) { }
        public void UpdateListenerPosition(Vector3 position, Vector3 forward) { }
    }
    
    /// <summary>
    /// ストレステスト用エフェクトサービス実装
    /// </summary>
    public class TestEffectService : MonoBehaviour, IEffectService, IInitializable
    {
        public int Priority => 0;
        public bool IsInitialized { get; private set; } = false;
        
        void Start()
        {
            Initialize();
        }
        
        public void Initialize()
        {
            IsInitialized = true;
        }
        
        public void PlayEffect(string effectId, Vector3 position = default, float volume = 1f) { }
        public int StartLoopingEffect(string effectId, Vector3 position, float volume = 1f) => 0;
        public void StopLoopingEffect(int loopId) { }
        public void PlayOneShot(string effectId, Vector3 position = default, float volume = 1f) { }
        public void PlayRandomEffect(string[] effectIds, Vector3 position = default, float volume = 1f) { }
        public void SetEffectPitch(string effectId, float pitch) { }
        public void PreloadEffects(string[] effectIds) { }
        public void ClearEffectPool() { }
    }
    
    #endregion
}