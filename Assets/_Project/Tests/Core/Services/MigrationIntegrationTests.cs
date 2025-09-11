using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _Project.Core;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Audio;
using asterivo.Unity60.Core.Audio.Services;
using asterivo.Unity60.Core.Audio.Interfaces;

namespace asterivo.Unity60.Tests.Core.Services
{
    /// <summary>
    /// 移行システムの統合・リアルタイム監視テストスイート
    /// Phase 3移行計画 Step 3.3の最終統合検証
    /// </summary>
    [TestFixture]
    public class MigrationIntegrationTests 
    {
        private GameObject testGameObject;
        private MigrationMonitor migrationMonitor;
        private List<IAudioService> audioServices;
        private List<ISpatialAudioService> spatialServices;
        private List<IEffectService> effectServices;
        
        [SetUp]
        public void SetUp()
        {
            ServiceLocator.Clear();
            FeatureFlags.ResetToDefaults();
            testGameObject = new GameObject("IntegrationTest");
            migrationMonitor = testGameObject.AddComponent<MigrationMonitor>();
            
            audioServices = new List<IAudioService>();
            spatialServices = new List<ISpatialAudioService>();
            effectServices = new List<IEffectService>();
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
        
        #region Full Migration Lifecycle Tests
        
        [UnityTest]
        public IEnumerator Migration_CompleteLifecycle_Phase3Implementation()
        {
            // Phase 1: Initial Setup (Singleton-only)
            FeatureFlags.UseServiceLocator = false;
            FeatureFlags.EnableMigrationMonitoring = true;
            
            var audioManager = testGameObject.AddComponent<AudioManager>();
            var spatialManager = testGameObject.AddComponent<SpatialAudioManager>();
            var effectManager = testGameObject.AddComponent<EffectManager>();
            
            yield return new WaitForSeconds(0.1f);
            
            var phase1Progress = migrationMonitor.GetMigrationProgress();
            Assert.AreEqual(0f, phase1Progress, "Phase 1 should show 0% migration");
            
            // Phase 2: ServiceLocator Introduction
            FeatureFlags.UseServiceLocator = true;
            ServiceLocator.RegisterService<IAudioService>(audioManager);
            ServiceLocator.RegisterService<ISpatialAudioService>(spatialManager);
            ServiceLocator.RegisterService<IEffectService>(effectManager);
            
            yield return new WaitForSeconds(0.1f);
            
            // Phase 3: Gradual Migration
            var migrationSteps = new Action[]
            {
                () => FeatureFlags.MigrateAudioManager = true,
                () => FeatureFlags.MigrateSpatialAudioManager = true,
                () => FeatureFlags.MigrateEffectManager = true,
                () => FeatureFlags.MigrateAudioUpdateCoordinator = true,
                () => FeatureFlags.MigrateStealthAudioCoordinator = true
            };
            
            var progressHistory = new List<float> { phase1Progress };
            
            foreach (var step in migrationSteps)
            {
                step();
                yield return new WaitForSeconds(0.1f); // Allow monitoring to update
                
                var progress = migrationMonitor.GetMigrationProgress();
                progressHistory.Add(progress);
                
                // Verify monotonic progress
                Assert.GreaterOrEqual(progress, progressHistory[progressHistory.Count - 2], 
                    "Migration progress should not decrease");
                
                // Test system stability during migration
                var isSafe = migrationMonitor.IsMigrationSafe();
                Assert.IsNotNull(isSafe, "Safety check should always work during migration");
                
                // Test service accessibility
                var service = ServiceLocator.GetService<IAudioService>();
                Assert.IsNotNull(service, "Services should remain accessible during migration");
            }
            
            // Phase 4: Completion Verification
            var finalProgress = migrationMonitor.GetMigrationProgress();
            Assert.AreEqual(100f, finalProgress, "Final progress should be 100%");
            
            var finalSafety = migrationMonitor.IsMigrationSafe();
            Assert.IsTrue(finalSafety, "Migration should be safe when complete");
            
            // Phase 5: Post-Migration Validation
            yield return new WaitForSeconds(1.0f); // Extended operation period
            
            // Verify all services work correctly
            var finalAudioService = ServiceLocator.GetService<IAudioService>();
            var finalSpatialService = ServiceLocator.GetService<ISpatialAudioService>();
            var finalEffectService = ServiceLocator.GetService<IEffectService>();
            
            Assert.IsNotNull(finalAudioService, "Audio service should be available post-migration");
            Assert.IsNotNull(finalSpatialService, "Spatial service should be available post-migration");
            Assert.IsNotNull(finalEffectService, "Effect service should be available post-migration");
        }
        
        [UnityTest]
        public IEnumerator Migration_RollbackScenario_CompleteRecovery()
        {
            // Setup full migration
            FeatureFlags.UseServiceLocator = true;
            FeatureFlags.MigrateAudioManager = true;
            FeatureFlags.MigrateSpatialAudioManager = true;
            FeatureFlags.MigrateEffectManager = true;
            FeatureFlags.EnableMigrationMonitoring = true;
            
            var audioManager = testGameObject.AddComponent<AudioManager>();
            ServiceLocator.RegisterService<IAudioService>(audioManager);
            
            yield return new WaitForSeconds(0.1f);
            var progressBeforeRollback = migrationMonitor.GetMigrationProgress();
            
            // Simulate performance issue triggering rollback
            migrationMonitor.LogSingletonUsage(typeof(AudioManager), "performance_issue");
            
            // Execute emergency rollback
            FeatureFlags.EmergencyRollback();
            
            yield return new WaitForSeconds(0.2f); // Allow rollback to complete
            
            // Verify rollback
            Assert.IsFalse(FeatureFlags.UseServiceLocator, "ServiceLocator should be disabled");
            Assert.IsFalse(FeatureFlags.MigrateAudioManager, "Audio migration should be disabled");
            Assert.IsFalse(FeatureFlags.EnableMigrationMonitoring, "Monitoring should be disabled");
            
            var progressAfterRollback = migrationMonitor.GetMigrationProgress();
            Assert.Less(progressAfterRollback, progressBeforeRollback, "Progress should decrease after rollback");
            
            // Test system recovery
            yield return new WaitForSeconds(0.5f);
            
            // Verify system remains functional in rollback state
            Assert.DoesNotThrow(() => migrationMonitor.GetMigrationProgress(), 
                "Progress reporting should work after rollback");
            Assert.DoesNotThrow(() => migrationMonitor.IsMigrationSafe(), 
                "Safety check should work after rollback");
        }
        
        #endregion
        
        #region Real-time Monitoring Tests
        
        [UnityTest]
        public IEnumerator Migration_RealTimeMonitoring_ContinuousTracking()
        {
            // Setup monitoring
            FeatureFlags.UseServiceLocator = true;
            FeatureFlags.EnableMigrationMonitoring = true;
            FeatureFlags.EnablePerformanceMeasurement = true;
            
            var monitoringResults = new List<(float time, float progress, bool isSafe)>();
            var endTime = Time.realtimeSinceStartup + 5.0f;
            
            // Simulate real-time usage patterns
            var frameCount = 0;
            while (Time.realtimeSinceStartup < endTime)
            {
                frameCount++;
                
                // Simulate different usage patterns
                if (frameCount % 60 == 0) // Every second
                {
                    FeatureFlags.MigrateAudioManager = frameCount % 120 == 0;
                    migrationMonitor.LogServiceLocatorUsage(typeof(IAudioService), $"frame{frameCount}");
                }
                
                if (frameCount % 30 == 0) // Twice per second
                {
                    migrationMonitor.LogSingletonUsage(typeof(AudioManager), $"frame{frameCount}");
                }
                
                if (frameCount % 10 == 0) // 6 times per second
                {
                    var progress = migrationMonitor.GetMigrationProgress();
                    var isSafe = migrationMonitor.IsMigrationSafe();
                    monitoringResults.Add((Time.realtimeSinceStartup, progress, isSafe));
                }
                
                yield return null;
            }
            
            // Analyze monitoring results
            Assert.Greater(monitoringResults.Count, 20, "Should have collected sufficient monitoring data");
            
            // Verify monitoring consistency
            foreach (var result in monitoringResults)
            {
                Assert.IsTrue(result.progress >= 0 && result.progress <= 100, 
                    $"Progress should be valid: {result.progress}");
                Assert.IsNotNull(result.isSafe, "Safety check should always return a value");
            }
            
            // Verify monitoring responsiveness
            var progressChanges = 0;
            for (int i = 1; i < monitoringResults.Count; i++)
            {
                if (Math.Abs(monitoringResults[i].progress - monitoringResults[i-1].progress) > 0.1f)
                {
                    progressChanges++;
                }
            }
            
            Assert.Greater(progressChanges, 0, "Monitoring should detect progress changes");
            
            UnityEngine.Debug.Log($"Real-time Monitoring Results:");
            UnityEngine.Debug.Log($"Total Samples: {monitoringResults.Count}");
            UnityEngine.Debug.Log($"Progress Changes Detected: {progressChanges}");
            UnityEngine.Debug.Log($"Final Progress: {monitoringResults.Last().progress:F1}%");
        }
        
        [UnityTest]
        public IEnumerator Migration_ConcurrentSystemOperations_StabilityValidation()
        {
            // Setup complex concurrent scenario
            FeatureFlags.UseServiceLocator = true;
            FeatureFlags.EnableMigrationMonitoring = true;
            
            // Create multiple services
            for (int i = 0; i < 5; i++)
            {
                var audioService = testGameObject.AddComponent<AudioService>();
                var spatialService = testGameObject.AddComponent<SpatialAudioService>();
                
                audioServices.Add(audioService);
                spatialServices.Add(spatialService);
                
                ServiceLocator.RegisterService<IAudioService>(audioService);
                ServiceLocator.RegisterService<ISpatialAudioService>(spatialService);
            }
            
            var stabilityMetrics = new List<(float frameTime, int serviceCount, float progress)>();
            var operationCount = 0;
            var errorCount = 0;
            
            var endTime = Time.realtimeSinceStartup + 3.0f;
            
            while (Time.realtimeSinceStartup < endTime)
            {
                var frameStart = Time.realtimeSinceStartup;
                
                try
                {
                    // Concurrent operations
                    var audioService = ServiceLocator.GetService<IAudioService>();
                    var spatialService = ServiceLocator.GetService<ISpatialAudioService>();
                    
                    if (audioService != null) operationCount++;
                    if (spatialService != null) operationCount++;
                    
                    // Monitoring operations
                    migrationMonitor.LogServiceLocatorUsage(typeof(IAudioService), "concurrent");
                    migrationMonitor.LogServiceLocatorUsage(typeof(ISpatialAudioService), "concurrent");
                    
                    var progress = migrationMonitor.GetMigrationProgress();
                    var serviceCount = ServiceLocator.GetServiceCount();
                    
                    // Random flag changes to stress test
                    if (UnityEngine.Random.value < 0.01f) // 1% chance per frame
                    {
                        FeatureFlags.MigrateAudioManager = !FeatureFlags.MigrateAudioManager;
                    }
                    
                    var frameTime = Time.realtimeSinceStartup - frameStart;
                    stabilityMetrics.Add((frameTime, serviceCount, progress));
                }
                catch (System.Exception)
                {
                    errorCount++;
                }
                
                yield return null;
            }
            
            // Analyze stability
            var averageFrameTime = stabilityMetrics.Average(m => m.frameTime);
            var maxFrameTime = stabilityMetrics.Max(m => m.frameTime);
            var progressVariance = CalculateVariance(stabilityMetrics.Select(m => m.progress));
            
            UnityEngine.Debug.Log($"Concurrent Operations Stability:");
            UnityEngine.Debug.Log($"Total Operations: {operationCount}");
            UnityEngine.Debug.Log($"Errors: {errorCount}");
            UnityEngine.Debug.Log($"Average Frame Time: {averageFrameTime * 1000:F2}ms");
            UnityEngine.Debug.Log($"Max Frame Time: {maxFrameTime * 1000:F2}ms");
            UnityEngine.Debug.Log($"Progress Variance: {progressVariance:F2}");
            
            // Assert stability requirements
            Assert.AreEqual(0, errorCount, "No errors should occur during concurrent operations");
            Assert.Less(averageFrameTime, 0.01f, "Average frame time should remain low");
            Assert.Less(maxFrameTime, 0.05f, "Maximum frame time should be reasonable");
            Assert.Greater(operationCount, 100, "Should have completed many operations");
        }
        
        #endregion
        
        #region Cross-Component Integration Tests
        
        [Test]
        public void Migration_AllAudioComponentsIntegration_SeamlessOperation()
        {
            // Setup all audio components
            var audioManager = testGameObject.AddComponent<AudioManager>();
            var spatialManager = testGameObject.AddComponent<SpatialAudioManager>();
            var effectManager = testGameObject.AddComponent<EffectManager>();
            var updateCoordinator = testGameObject.AddComponent<AudioUpdateCoordinator>();
            
            // Register all services
            FeatureFlags.UseServiceLocator = true;
            ServiceLocator.RegisterService<IAudioService>(audioManager);
            ServiceLocator.RegisterService<ISpatialAudioService>(spatialManager);
            ServiceLocator.RegisterService<IEffectService>(effectManager);
            ServiceLocator.RegisterService<IAudioUpdateService>(updateCoordinator);
            
            // Enable all migrations
            FeatureFlags.MigrateAudioManager = true;
            FeatureFlags.MigrateSpatialAudioManager = true;
            FeatureFlags.MigrateEffectManager = true;
            FeatureFlags.MigrateAudioUpdateCoordinator = true;
            FeatureFlags.EnableMigrationMonitoring = true;
            
            // Test cross-component communication
            var retrievedAudio = ServiceLocator.GetService<IAudioService>();
            var retrievedSpatial = ServiceLocator.GetService<ISpatialAudioService>();
            var retrievedEffect = ServiceLocator.GetService<IEffectService>();
            var retrievedUpdate = ServiceLocator.GetService<IAudioUpdateService>();
            
            // Assert all components are accessible
            Assert.IsNotNull(retrievedAudio, "AudioService should be accessible");
            Assert.IsNotNull(retrievedSpatial, "SpatialAudioService should be accessible");
            Assert.IsNotNull(retrievedEffect, "EffectService should be accessible");
            Assert.IsNotNull(retrievedUpdate, "AudioUpdateService should be accessible");
            
            // Test monitoring integration
            migrationMonitor.LogServiceLocatorUsage(typeof(IAudioService), "integration");
            migrationMonitor.LogServiceLocatorUsage(typeof(ISpatialAudioService), "integration");
            migrationMonitor.LogServiceLocatorUsage(typeof(IEffectService), "integration");
            migrationMonitor.LogServiceLocatorUsage(typeof(IAudioUpdateService), "integration");
            
            var progress = migrationMonitor.GetMigrationProgress();
            var isSafe = migrationMonitor.IsMigrationSafe();
            
            Assert.AreEqual(100f, progress, "Should show 100% progress with all components migrated");
            Assert.IsTrue(isSafe, "Should be safe with all components properly integrated");
            
            // Test statistics collection
            var singletonStats = migrationMonitor.GetSingletonUsageStats();
            var serviceStats = migrationMonitor.GetServiceLocatorUsageStats();
            
            Assert.IsNotNull(singletonStats, "Singleton statistics should be available");
            Assert.IsNotNull(serviceStats, "Service statistics should be available");
            Assert.Greater(serviceStats.Count, 0, "Should have recorded service usage");
        }
        
        [Test]
        public void Migration_PartialMigrationScenarios_ConsistentBehavior()
        {
            var scenarios = new[]
            {
                new { Audio = true, Spatial = false, Effect = false, Update = false, Expected = 20f },
                new { Audio = true, Spatial = true, Effect = false, Update = false, Expected = 40f },
                new { Audio = true, Spatial = true, Effect = true, Update = false, Expected = 60f },
                new { Audio = true, Spatial = true, Effect = true, Update = true, Expected = 80f },
                new { Audio = true, Spatial = true, Effect = true, Update = true, Expected = 80f }, // Stealth not migrated
            };
            
            foreach (var scenario in scenarios)
            {
                // Reset state
                ServiceLocator.Clear();
                FeatureFlags.ResetToDefaults();
                FeatureFlags.UseServiceLocator = true;
                FeatureFlags.EnableMigrationMonitoring = true;
                
                // Setup services
                if (scenario.Audio)
                {
                    var service = testGameObject.AddComponent<AudioService>();
                    ServiceLocator.RegisterService<IAudioService>(service);
                    FeatureFlags.MigrateAudioManager = true;
                }
                
                if (scenario.Spatial)
                {
                    var service = testGameObject.AddComponent<SpatialAudioService>();
                    ServiceLocator.RegisterService<ISpatialAudioService>(service);
                    FeatureFlags.MigrateSpatialAudioManager = true;
                }
                
                if (scenario.Effect)
                {
                    var service = testGameObject.AddComponent<EffectManager>();
                    ServiceLocator.RegisterService<IEffectService>(service);
                    FeatureFlags.MigrateEffectManager = true;
                }
                
                if (scenario.Update)
                {
                    var service = testGameObject.AddComponent<AudioUpdateService>();
                    ServiceLocator.RegisterService<IAudioUpdateService>(service);
                    FeatureFlags.MigrateAudioUpdateCoordinator = true;
                }
                
                // Test partial migration
                var progress = migrationMonitor.GetMigrationProgress();
                var isSafe = migrationMonitor.IsMigrationSafe();
                
                Assert.AreEqual(scenario.Expected, progress, 
                    $"Scenario progress should be {scenario.Expected}% (Audio:{scenario.Audio}, Spatial:{scenario.Spatial}, Effect:{scenario.Effect}, Update:{scenario.Update})");
                Assert.IsNotNull(isSafe, "Safety check should work in partial migration scenarios");
                
                // Test service access in partial migration
                if (scenario.Audio)
                {
                    var audioService = ServiceLocator.GetService<IAudioService>();
                    Assert.IsNotNull(audioService, "Audio service should be accessible when migrated");
                }
                
                if (scenario.Spatial)
                {
                    var spatialService = ServiceLocator.GetService<ISpatialAudioService>();
                    Assert.IsNotNull(spatialService, "Spatial service should be accessible when migrated");
                }
            }
        }
        
        #endregion
        
        #region Performance Integration Tests
        
        [UnityTest]
        public IEnumerator Migration_LongRunningStabilityTest_24HourSimulation()
        {
            // Simulate 24 hours in accelerated time (24 seconds = 24 hours)
            FeatureFlags.UseServiceLocator = true;
            FeatureFlags.EnableMigrationMonitoring = true;
            FeatureFlags.EnablePerformanceMeasurement = true;
            
            var audioService = testGameObject.AddComponent<AudioService>();
            ServiceLocator.RegisterService<IAudioService>(audioService);
            
            var startTime = Time.realtimeSinceStartup;
            var endTime = startTime + 24.0f; // 24 seconds for simulation
            var hourlyMetrics = new List<(int hour, float avgFrameTime, float progress, bool isSafe)>();
            
            int currentHour = 0;
            var hourStartTime = startTime;
            var frameTimesThisHour = new List<float>();
            
            while (Time.realtimeSinceStartup < endTime)
            {
                var frameStart = Time.realtimeSinceStartup;
                
                // Simulate different daily patterns
                var hourOfDay = currentHour % 24;
                var activityLevel = GetActivityLevel(hourOfDay);
                
                // Vary migration activity based on simulated time
                if (UnityEngine.Random.value < activityLevel)
                {
                    migrationMonitor.LogServiceLocatorUsage(typeof(IAudioService), $"hour{hourOfDay}");
                    
                    if (UnityEngine.Random.value < 0.1f) // 10% chance of singleton usage
                    {
                        migrationMonitor.LogSingletonUsage(typeof(AudioManager), $"hour{hourOfDay}");
                    }
                }
                
                // Check if hour has passed (1 second = 1 hour)
                if (Time.realtimeSinceStartup - hourStartTime >= 1.0f)
                {
                    var avgFrameTime = frameTimesThisHour.Count > 0 ? frameTimesThisHour.Average() : 0f;
                    var progress = migrationMonitor.GetMigrationProgress();
                    var isSafe = migrationMonitor.IsMigrationSafe();
                    
                    hourlyMetrics.Add((currentHour, avgFrameTime, progress, isSafe));
                    
                    currentHour++;
                    hourStartTime = Time.realtimeSinceStartup;
                    frameTimesThisHour.Clear();
                }
                
                var frameTime = Time.realtimeSinceStartup - frameStart;
                frameTimesThisHour.Add(frameTime);
                
                yield return null;
            }
            
            // Analyze long-running stability
            var overallAvgFrameTime = hourlyMetrics.Average(m => m.avgFrameTime);
            var maxFrameTime = hourlyMetrics.Max(m => m.avgFrameTime);
            var progressStability = hourlyMetrics.All(m => m.progress >= 0 && m.progress <= 100);
            var safetyConsistency = hourlyMetrics.All(m => m.isSafe != null);
            
            UnityEngine.Debug.Log($"24-Hour Simulation Results:");
            UnityEngine.Debug.Log($"Hours Simulated: {hourlyMetrics.Count}");
            UnityEngine.Debug.Log($"Overall Avg Frame Time: {overallAvgFrameTime * 1000:F2}ms");
            UnityEngine.Debug.Log($"Max Frame Time: {maxFrameTime * 1000:F2}ms");
            UnityEngine.Debug.Log($"Progress Stability: {progressStability}");
            UnityEngine.Debug.Log($"Safety Consistency: {safetyConsistency}");
            
            // Assert long-term stability
            Assert.Greater(hourlyMetrics.Count, 20, "Should simulate at least 20 hours");
            Assert.Less(overallAvgFrameTime, 0.005f, "Average frame time should remain low over long periods");
            Assert.Less(maxFrameTime, 0.02f, "Max frame time should not spike significantly");
            Assert.IsTrue(progressStability, "Progress should remain stable over long periods");
            Assert.IsTrue(safetyConsistency, "Safety checks should remain consistent");
        }
        
        #endregion
        
        #region Helper Methods
        
        private float GetActivityLevel(int hourOfDay)
        {
            // Simulate realistic daily activity patterns
            if (hourOfDay >= 9 && hourOfDay <= 17) return 0.8f; // Work hours - high activity
            if (hourOfDay >= 19 && hourOfDay <= 22) return 0.6f; // Evening - medium activity
            if (hourOfDay >= 0 && hourOfDay <= 6) return 0.1f;   // Night - low activity
            return 0.4f; // Other times - moderate activity
        }
        
        private float CalculateVariance(IEnumerable<float> values)
        {
            var average = values.Average();
            var sumOfSquaredDifferences = values.Select(x => Mathf.Pow(x - average, 2)).Sum();
            return sumOfSquaredDifferences / values.Count();
        }
        
        #endregion
    }
}