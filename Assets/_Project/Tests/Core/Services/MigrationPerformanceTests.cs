using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Audio;
using asterivo.Unity60.Core.Audio.Services;
using asterivo.Unity60.Core.Audio.Interfaces;

namespace asterivo.Unity60.Tests.Core.Services
{
    /// <summary>
    /// 移行システムのパフォーマンス詳細測定テストスイート
    /// Phase 3移行計画 Step 3.3の性能解析実装
    /// </summary>
    [TestFixture]
    public class MigrationPerformanceTests 
    {
        private GameObject testGameObject;
        private MigrationMonitor migrationMonitor;
        private Stopwatch stopwatch;
        
        [SetUp]
        public void SetUp()
        {
            ServiceLocator.Clear();
            FeatureFlags.ResetToDefaults();
            testGameObject = new GameObject("PerformanceTest");
            migrationMonitor = testGameObject.AddComponent<MigrationMonitor>();
            stopwatch = new Stopwatch();
        }
        
        [TearDown]
        public void TearDown()
        {
            ServiceLocator.Clear();
            FeatureFlags.ResetToDefaults();
            if (testGameObject != null)
            {
                UnityEngine.Object.DestroyImmediate(testGameObject);
            }
        }
        
        #region ServiceLocator Performance Analysis
        
        [Test]
        public void ServiceLocator_RegistrationPerformance_DetailedAnalysis()
        {
            // Arrange
            const int testCycles = 5;
            const int servicesPerCycle = 1000;
            var registrationTimes = new List<double>();
            
            // Act & Measure
            for (int cycle = 0; cycle < testCycles; cycle++)
            {
                ServiceLocator.Clear();
                stopwatch.Restart();
                
                for (int i = 0; i < servicesPerCycle; i++)
                {
                    var service = testGameObject.AddComponent<AudioService>();
                    ServiceLocator.RegisterService<IAudioService>(service);
                }
                
                stopwatch.Stop();
                registrationTimes.Add(stopwatch.Elapsed.TotalMilliseconds);
            }
            
            // Assert
            var averageTime = registrationTimes.Average();
            var maxTime = registrationTimes.Max();
            var minTime = registrationTimes.Min();
            var stdDev = CalculateStandardDeviation(registrationTimes);
            
            UnityEngine.Debug.Log($"Registration Performance Analysis:");
            UnityEngine.Debug.Log($"Average: {averageTime:F2}ms, Min: {minTime:F2}ms, Max: {maxTime:F2}ms, StdDev: {stdDev:F2}ms");
            
            Assert.Less(averageTime, 500, "Average registration time should be under 500ms");
            Assert.Less(maxTime, 1000, "Maximum registration time should be under 1000ms");
            Assert.Less(stdDev, 100, "Standard deviation should be low (consistent performance)");
        }
        
        [Test]
        public void ServiceLocator_RetrievalPerformance_ScalabilityTest()
        {
            // Arrange
            var serviceCounts = new int[] { 1, 10, 100, 1000, 5000 };
            var retrievalTimes = new Dictionary<int, double>();
            
            foreach (var serviceCount in serviceCounts)
            {
                // Setup
                ServiceLocator.Clear();
                
                for (int i = 0; i < serviceCount; i++)
                {
                    var service = testGameObject.AddComponent<AudioService>();
                    ServiceLocator.RegisterService<IAudioService>(service);
                }
                
                // Measure retrieval time
                const int retrievalTests = 10000;
                stopwatch.Restart();
                
                for (int i = 0; i < retrievalTests; i++)
                {
                    var service = ServiceLocator.GetService<IAudioService>();
                    Assert.IsNotNull(service);
                }
                
                stopwatch.Stop();
                retrievalTimes[serviceCount] = stopwatch.Elapsed.TotalMilliseconds / retrievalTests;
            }
            
            // Assert
            UnityEngine.Debug.Log("ServiceLocator Scalability Analysis:");
            foreach (var kvp in retrievalTimes)
            {
                UnityEngine.Debug.Log($"Services: {kvp.Key}, Avg Retrieval: {kvp.Value:F4}ms");
                Assert.Less(kvp.Value, 0.1, $"Retrieval time should be under 0.1ms even with {kvp.Key} services");
            }
            
            // Verify O(1) performance (retrieval time shouldn't significantly increase)
            var timeWith1 = retrievalTimes[1];
            var timeWith5000 = retrievalTimes[5000];
            var performanceRatio = timeWith5000 / timeWith1;
            
            Assert.Less(performanceRatio, 10, "Retrieval performance should not degrade significantly with scale");
        }
        
        #endregion
        
        #region Migration Monitor Performance Analysis
        
        [Test]
        public void MigrationMonitor_UpdateCyclePerformance_DetailedProfiling()
        {
            // Arrange
            FeatureFlags.EnableMigrationMonitoring = true;
            var updateTimes = new List<double>();
            const int testCycles = 100;
            
            // Warm up
            for (int i = 0; i < 10; i++)
            {
                migrationMonitor.LogSingletonUsage(typeof(AudioManager), "warmup");
            }
            
            // Act & Measure
            for (int cycle = 0; cycle < testCycles; cycle++)
            {
                stopwatch.Restart();
                
                // Simulate typical monitoring workload
                migrationMonitor.LogSingletonUsage(typeof(AudioManager), $"test{cycle}");
                migrationMonitor.LogServiceLocatorUsage(typeof(IAudioService), $"service{cycle}");
                var progress = migrationMonitor.GetMigrationProgress();
                var isSafe = migrationMonitor.IsMigrationSafe();
                
                stopwatch.Stop();
                updateTimes.Add(stopwatch.Elapsed.TotalMilliseconds);
            }
            
            // Assert
            var averageTime = updateTimes.Average();
            var maxTime = updateTimes.Max();
            var p95Time = updateTimes.OrderBy(x => x).Skip((int)(testCycles * 0.95)).First();
            
            UnityEngine.Debug.Log($"Migration Monitor Performance:");
            UnityEngine.Debug.Log($"Average: {averageTime:F4}ms, Max: {maxTime:F4}ms, P95: {p95Time:F4}ms");
            
            Assert.Less(averageTime, 1.0, "Average update cycle should be under 1ms");
            Assert.Less(maxTime, 5.0, "Maximum update cycle should be under 5ms");
            Assert.Less(p95Time, 2.0, "95th percentile should be under 2ms");
        }
        
        [UnityTest]
        public IEnumerator MigrationMonitor_FrameTimeImpact_MinimalOverhead()
        {
            // Arrange
            FeatureFlags.EnableMigrationMonitoring = true;
            var frameTimesWithoutMonitoring = new List<float>();
            var frameTimesWithMonitoring = new List<float>();
            
            // Measure baseline (monitoring disabled)
            FeatureFlags.EnableMigrationMonitoring = false;
            for (int i = 0; i < 60; i++) // 1 second at 60fps
            {
                frameTimesWithoutMonitoring.Add(Time.unscaledDeltaTime);
                yield return null;
            }
            
            // Measure with monitoring enabled
            FeatureFlags.EnableMigrationMonitoring = true;
            for (int i = 0; i < 60; i++) // 1 second at 60fps
            {
                // Simulate monitoring activity
                migrationMonitor.LogSingletonUsage(typeof(AudioManager), $"frame{i}");
                migrationMonitor.LogServiceLocatorUsage(typeof(IAudioService), $"frame{i}");
                
                frameTimesWithMonitoring.Add(Time.unscaledDeltaTime);
                yield return null;
            }
            
            // Assert
            var avgWithout = frameTimesWithoutMonitoring.Average();
            var avgWith = frameTimesWithMonitoring.Average();
            var overhead = avgWith - avgWithout;
            var overheadPercentage = (overhead / avgWithout) * 100;
            
            UnityEngine.Debug.Log($"Frame Time Impact Analysis:");
            UnityEngine.Debug.Log($"Without Monitoring: {avgWithout * 1000:F2}ms");
            UnityEngine.Debug.Log($"With Monitoring: {avgWith * 1000:F2}ms");
            UnityEngine.Debug.Log($"Overhead: {overhead * 1000:F2}ms ({overheadPercentage:F1}%)");
            
            Assert.Less(overhead, 0.001f, "Monitoring overhead should be under 1ms");
            Assert.Less(overheadPercentage, 5, "Monitoring overhead should be under 5%");
        }
        
        #endregion
        
        #region Memory Usage Analysis
        
        [Test]
        public void Migration_MemoryFootprint_DetailedAnalysis()
        {
            // Arrange
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            System.GC.Collect();
            
            var initialMemory = System.GC.GetTotalMemory(false);
            var memoryCheckpoints = new Dictionary<string, long>();
            
            // Baseline
            memoryCheckpoints["Initial"] = System.GC.GetTotalMemory(false);
            
            // ServiceLocator setup
            var services = new List<IAudioService>();
            for (int i = 0; i < 100; i++)
            {
                var service = testGameObject.AddComponent<AudioService>();
                ServiceLocator.RegisterService<IAudioService>(service);
                services.Add(service);
            }
            memoryCheckpoints["After ServiceLocator Setup"] = System.GC.GetTotalMemory(false);
            
            // Migration monitoring
            FeatureFlags.EnableMigrationMonitoring = true;
            for (int i = 0; i < 1000; i++)
            {
                migrationMonitor.LogSingletonUsage(typeof(AudioManager), $"memory{i}");
                migrationMonitor.LogServiceLocatorUsage(typeof(IAudioService), $"memory{i}");
            }
            memoryCheckpoints["After Migration Logging"] = System.GC.GetTotalMemory(false);
            
            // Generate reports
            for (int i = 0; i < 10; i++)
            {
                var stats = migrationMonitor.GetSingletonUsageStats();
                var serviceStats = migrationMonitor.GetServiceLocatorUsageStats();
            }
            memoryCheckpoints["After Report Generation"] = System.GC.GetTotalMemory(false);
            
            // Cleanup
            ServiceLocator.Clear();
            FeatureFlags.ResetToDefaults();
            System.GC.Collect();
            memoryCheckpoints["After Cleanup"] = System.GC.GetTotalMemory(true);
            
            // Assert
            UnityEngine.Debug.Log("Memory Usage Analysis:");
            long previousMemory = initialMemory;
            foreach (var checkpoint in memoryCheckpoints)
            {
                var memoryDiff = checkpoint.Value - previousMemory;
                var totalDiff = checkpoint.Value - initialMemory;
                UnityEngine.Debug.Log($"{checkpoint.Key}: {checkpoint.Value / 1024}KB (Δ{memoryDiff / 1024}KB, Total Δ{totalDiff / 1024}KB)");
                previousMemory = checkpoint.Value;
            }
            
            var totalMemoryIncrease = memoryCheckpoints["After Cleanup"] - initialMemory;
            Assert.Less(totalMemoryIncrease, 10 * 1024 * 1024, "Total memory increase should be under 10MB");
            
            var serviceLocatorMemory = memoryCheckpoints["After ServiceLocator Setup"] - initialMemory;
            Assert.Less(serviceLocatorMemory, 5 * 1024 * 1024, "ServiceLocator memory should be under 5MB");
        }
        
        #endregion
        
        #region Real-world Simulation Tests
        
        [UnityTest]
        public IEnumerator Migration_RealisticGameplaySimulation_PerformanceValidation()
        {
            // Arrange
            FeatureFlags.UseServiceLocator = true;
            FeatureFlags.EnableMigrationMonitoring = true;
            FeatureFlags.EnablePerformanceMeasurement = true;
            
            // Setup multiple audio services
            var audioManager = testGameObject.AddComponent<AudioManager>();
            var spatialManager = testGameObject.AddComponent<SpatialAudioManager>();
            var effectManager = testGameObject.AddComponent<EffectManager>();
            
            ServiceLocator.RegisterService<IAudioService>(audioManager);
            ServiceLocator.RegisterService<ISpatialAudioService>(spatialManager);
            ServiceLocator.RegisterService<IEffectService>(effectManager);
            
            var performanceMetrics = new List<float>();
            var frameTimeMetrics = new List<float>();
            
            // Act - Simulate 10 seconds of gameplay
            var endTime = Time.realtimeSinceStartup + 10.0f;
            int frameCount = 0;
            
            while (Time.realtimeSinceStartup < endTime)
            {
                var frameStart = Time.realtimeSinceStartup;
                
                // Simulate typical game frame operations
                if (frameCount % 10 == 0) // Every 10 frames
                {
                    var audioService = ServiceLocator.GetService<IAudioService>();
                    migrationMonitor.LogServiceLocatorUsage(typeof(IAudioService), "gameplay");
                }
                
                if (frameCount % 15 == 0) // Every 15 frames
                {
                    var spatialService = ServiceLocator.GetService<ISpatialAudioService>();
                    migrationMonitor.LogServiceLocatorUsage(typeof(ISpatialAudioService), "spatial");
                }
                
                if (frameCount % 30 == 0) // Every 30 frames (1/2 second)
                {
                    var progress = migrationMonitor.GetMigrationProgress();
                    var isSafe = migrationMonitor.IsMigrationSafe();
                }
                
                frameCount++;
                var frameTime = Time.realtimeSinceStartup - frameStart;
                frameTimeMetrics.Add(frameTime);
                
                yield return null;
            }
            
            // Assert
            var averageFrameTime = frameTimeMetrics.Average();
            var maxFrameTime = frameTimeMetrics.Max();
            var framesOver16ms = frameTimeMetrics.Count(t => t > 0.016f);
            var framesOver33ms = frameTimeMetrics.Count(t => t > 0.033f);
            
            UnityEngine.Debug.Log($"Realistic Simulation Results:");
            UnityEngine.Debug.Log($"Total Frames: {frameCount}");
            UnityEngine.Debug.Log($"Average Frame Time: {averageFrameTime * 1000:F2}ms");
            UnityEngine.Debug.Log($"Max Frame Time: {maxFrameTime * 1000:F2}ms");
            UnityEngine.Debug.Log($"Frames > 16ms (60fps): {framesOver16ms} ({(float)framesOver16ms / frameCount * 100:F1}%)");
            UnityEngine.Debug.Log($"Frames > 33ms (30fps): {framesOver33ms} ({(float)framesOver33ms / frameCount * 100:F1}%)");
            
            Assert.Less(averageFrameTime, 0.016f, "Average frame time should maintain 60fps");
            Assert.Less(framesOver33ms, frameCount * 0.01f, "Less than 1% of frames should drop below 30fps");
            Assert.Greater(frameCount, 300, "Should have processed at least 300 frames in 10 seconds");
        }
        
        #endregion
        
        #region Helper Methods
        
        private double CalculateStandardDeviation(List<double> values)
        {
            var average = values.Average();
            var sumOfSquaredDifferences = values.Select(x => Math.Pow(x - average, 2)).Sum();
            return Math.Sqrt(sumOfSquaredDifferences / values.Count);
        }
        
        #endregion
    }
}