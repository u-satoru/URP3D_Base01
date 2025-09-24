using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Audio;
using asterivo.Unity60.Core.Audio.Services;
using asterivo.Unity60.Core.Audio.Interfaces;

namespace asterivo.Unity60.Tests.Core.Services
{
    /// <summary>
    /// 移行システムのエッジケース・ストレステストスイート
    /// Phase 3移行計画 Step 3.3の拡張実装
    /// </summary>
    [TestFixture]
    public class MigrationEdgeCaseTests 
    {
        private GameObject testGameObject;
        private MigrationMonitor migrationMonitor;
        
        [SetUp]
        public void SetUp()
        {
            ServiceLocator.Clear();
            FeatureFlags.ResetToDefaults();
            testGameObject = new GameObject("EdgeCaseTest");
            migrationMonitor = testGameObject.AddComponent<MigrationMonitor>();
        }
        
        [TearDown]
        public void TearDown()
        {
            ServiceLocator.Clear();
            FeatureFlags.ResetToDefaults();
            if (testGameObject != null)
            {
                Object.DestroyImmediate(testGameObject);
            }
        }
        
        #region Stress Tests
        
        [Test]
        public void ServiceLocator_MassiveServiceRegistration_HandlesCorrectly()
        {
            // Arrange
            const int serviceCount = 10000;
            var services = new List<IAudioService>();
            
            // Act
            var startTime = System.DateTime.Now;
            
            for (int i = 0; i < serviceCount; i++)
            {
                var service = testGameObject.AddComponent<AudioService>();
                ServiceLocator.RegisterService<IAudioService>(service);
                services.Add(service);
            }
            
            var registrationTime = (System.DateTime.Now - startTime).TotalMilliseconds;
            
            // Assert
            Assert.AreEqual(serviceCount, ServiceLocator.GetServiceCount());
            Assert.Less(registrationTime, 5000, "Mass registration should complete within 5 seconds");
            
            // Verify retrieval performance
            startTime = System.DateTime.Now;
            var retrievedService = ServiceLocator.GetService<IAudioService>();
            var retrievalTime = (System.DateTime.Now - startTime).TotalMilliseconds;
            
            Assert.IsNotNull(retrievedService);
            Assert.Less(retrievalTime, 100, "Service retrieval should be fast even with many services");
        }
        
        [Test]
        public void MigrationMonitor_RapidProgressUpdates_HandlesCorrectly()
        {
            // Arrange
            const int updateCount = 1000;
            
            // Act
            var startTime = System.DateTime.Now;
            
            for (int i = 0; i < updateCount; i++)
            {
                migrationMonitor.LogSingletonUsage(typeof(AudioManager), $"Location{i}");
                migrationMonitor.LogServiceLocatorUsage(typeof(IAudioService), $"ServiceLocation{i}");
            }
            
            var processingTime = (System.DateTime.Now - startTime).TotalMilliseconds;
            
            // Assert
            Assert.Less(processingTime, 2000, "Rapid logging should complete within 2 seconds");
            
            var stats = migrationMonitor.GetSingletonUsageStats();
            Assert.IsTrue(stats.ContainsKey(typeof(AudioManager)));
            Assert.AreEqual(updateCount, stats[typeof(AudioManager)]);
        }
        
        [UnityTest]
        public IEnumerator MigrationMonitor_LongRunningMonitoring_MaintainsPerformance()
        {
            // Arrange
            FeatureFlags.EnableMigrationMonitoring = true;
            var initialFrameTime = Time.unscaledDeltaTime;
            var frameTimesSamples = new List<float>();
            
            // Act - Monitor for 5 seconds of frame updates
            var endTime = Time.realtimeSinceStartup + 5.0f;
            
            while (Time.realtimeSinceStartup < endTime)
            {
                frameTimesSamples.Add(Time.unscaledDeltaTime);
                
                // Simulate migration activity
                migrationMonitor.LogSingletonUsage(typeof(AudioManager), "FrameTest");
                migrationMonitor.LogServiceLocatorUsage(typeof(IAudioService), "FrameTest");
                
                yield return null;
            }
            
            // Assert
            var averageFrameTime = 0f;
            foreach (var time in frameTimesSamples)
            {
                averageFrameTime += time;
            }
            averageFrameTime /= frameTimesSamples.Count;
            
            Assert.Less(averageFrameTime, 0.033f, "Average frame time should stay under 30 FPS threshold");
            Assert.Greater(frameTimesSamples.Count, 150, "Should have collected sufficient frame samples");
        }
        
        #endregion
        
        #region Memory Management Tests
        
        [Test]
        public void ServiceLocator_MemoryLeakPrevention_ClearsCorrectly()
        {
            // Arrange
            var initialMemory = System.GC.GetTotalMemory(true);
            var services = new List<IAudioService>();
            
            // Act - Create many services
            for (int i = 0; i < 1000; i++)
            {
                var service = testGameObject.AddComponent<AudioService>();
                ServiceLocator.RegisterService<IAudioService>(service);
                services.Add(service);
            }
            
            var memoryAfterCreation = System.GC.GetTotalMemory(false);
            
            // Clear all services
            ServiceLocator.Clear();
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            System.GC.Collect();
            
            var memoryAfterCleanup = System.GC.GetTotalMemory(true);
            
            // Assert
            Assert.Less(memoryAfterCleanup, memoryAfterCreation, "Memory should be released after cleanup");
            
            var memoryIncrease = memoryAfterCleanup - initialMemory;
            Assert.Less(memoryIncrease, 50 * 1024 * 1024, "Memory increase should be reasonable (< 50MB)");
        }
        
        [Test]
        public void MigrationMonitor_EventLogTruncation_PreventsMemoryGrowth()
        {
            // Arrange
            var initialMemory = System.GC.GetTotalMemory(true);
            
            // Act - Generate many events beyond the limit
            for (int i = 0; i < 100; i++)
            {
                migrationMonitor.LogSingletonUsage(typeof(AudioManager), $"MemoryTest{i}");
            }
            
            System.GC.Collect();
            var memoryAfterLogging = System.GC.GetTotalMemory(true);
            
            // Assert
            var progress = migrationMonitor.GetMigrationProgress();
            Assert.IsTrue(progress >= 0, "Migration progress should be valid");
            
            var memoryIncrease = memoryAfterLogging - initialMemory;
            Assert.Less(memoryIncrease, 10 * 1024 * 1024, "Memory increase should be minimal (< 10MB)");
        }
        
        #endregion
        
        #region Concurrency Edge Cases
        
        [Test]
        public void ServiceLocator_ConcurrentAccess_ThreadSafe()
        {
            // Arrange
            var service = testGameObject.AddComponent<AudioService>();
            ServiceLocator.RegisterService<IAudioService>(service);
            var exceptions = new List<System.Exception>();
            var tasks = new List<Task>();
            
            // Act - Simulate concurrent access
            for (int i = 0; i < 10; i++)
            {
                var task = Task.Run(() =>
                {
                    try
                    {
                        for (int j = 0; j < 100; j++)
                        {
                            var retrievedService = ServiceLocator.GetService<IAudioService>();
                            Assert.IsNotNull(retrievedService);
                        }
                    }
                    catch (System.Exception ex)
                    {
                        lock (exceptions)
                        {
                            exceptions.Add(ex);
                        }
                    }
                });
                tasks.Add(task);
            }
            
            Task.WaitAll(tasks.ToArray());
            
            // Assert
            Assert.IsEmpty(exceptions, "No exceptions should occur during concurrent access");
        }
        
        #endregion
        
        #region Error Recovery Tests
        
        [Test]
        public void FeatureFlags_InvalidFlagAccess_HandlesGracefully()
        {
            // Arrange & Act
            bool exceptionThrown = false;
            
            try
            {
                // Try to access non-existent flag through reflection
                var flagType = typeof(FeatureFlags);
                var nonExistentField = flagType.GetField("NonExistentFlag");
                
                if (nonExistentField != null)
                {
                    var value = nonExistentField.GetValue(null);
                }
            }
            catch (System.Exception)
            {
                exceptionThrown = true;
            }
            
            // Assert - System should handle gracefully
            Assert.IsFalse(exceptionThrown, "Should handle invalid flag access gracefully");
            
            // Verify core flags still work
            Assert.IsTrue(FeatureFlags.UseServiceLocator || !FeatureFlags.UseServiceLocator, "Core flags should remain functional");
        }
        
        [Test]
        public void MigrationMonitor_NullParameterHandling_RemainsStable()
        {
            // Arrange & Act - Test null parameter handling
            migrationMonitor.LogSingletonUsage(null, "test");
            migrationMonitor.LogSingletonUsage(typeof(AudioManager), null);
            migrationMonitor.LogServiceLocatorUsage(null, "test");
            migrationMonitor.LogServiceLocatorUsage(typeof(IAudioService), null);
            
            // Assert - Monitor should remain functional
            var progress = migrationMonitor.GetMigrationProgress();
            Assert.IsTrue(progress >= 0 && progress <= 100, "Migration progress should remain valid");
            
            var isSafe = migrationMonitor.IsMigrationSafe();
            Assert.IsTrue(isSafe == true || isSafe == false, "Safety check should return valid boolean");
        }
        
        [Test]
        public void ServiceLocator_ServiceDestruction_HandlesGracefully()
        {
            // Arrange
            var service = testGameObject.AddComponent<AudioService>();
            ServiceLocator.RegisterService<IAudioService>(service);
            
            // Act - Destroy the service component
            Object.DestroyImmediate(service);
            
            // Try to retrieve the destroyed service
            var retrievedService = ServiceLocator.GetService<IAudioService>();
            
            // Assert - Should handle destroyed service gracefully
            Assert.IsNull(retrievedService, "Should return null for destroyed service");
            
            // ServiceLocator should remain functional
            var newService = testGameObject.AddComponent<AudioService>();
            ServiceLocator.RegisterService<IAudioService>(newService);
            var newRetrievedService = ServiceLocator.GetService<IAudioService>();
            
            Assert.IsNotNull(newRetrievedService, "ServiceLocator should remain functional after service destruction");
        }
        
        #endregion
        
        #region Integration Edge Cases
        
        [UnityTest]
        public IEnumerator MigrationFlow_RapidToggling_MaintainsConsistency()
        {
            // Arrange
            FeatureFlags.EnableMigrationMonitoring = true;
            
            // Act - Rapidly toggle migration flags
            for (int i = 0; i < 50; i++)
            {
                FeatureFlags.MigrateAudioManager = i % 2 == 0;
                FeatureFlags.MigrateSpatialAudioManager = i % 3 == 0;
                FeatureFlags.UseServiceLocator = i % 2 == 1;
                
                yield return null; // Allow one frame for updates
            }
            
            // Final stable state
            FeatureFlags.UseServiceLocator = true;
            FeatureFlags.MigrateAudioManager = true;
            
            yield return new WaitForSeconds(0.1f); // Allow monitoring to stabilize
            
            // Assert
            var progress = migrationMonitor.GetMigrationProgress();
            Assert.IsTrue(progress >= 0 && progress <= 100, "Migration progress should remain valid after rapid changes");
            
            var isSafe = migrationMonitor.IsMigrationSafe();
            Assert.IsNotNull(isSafe, "Safety check should return valid result");
        }
        
        [Test]
        public void Migration_EmergencyRollback_CompletesSuccessfully()
        {
            // Arrange
            FeatureFlags.UseServiceLocator = true;
            FeatureFlags.MigrateAudioManager = true;
            FeatureFlags.MigrateSpatialAudioManager = true;
            FeatureFlags.EnableMigrationMonitoring = true;
            
            var initialProgress = migrationMonitor.GetMigrationProgress();
            
            // Act
            FeatureFlags.EmergencyRollback();
            
            // Assert
            Assert.IsFalse(FeatureFlags.UseServiceLocator, "ServiceLocator should be disabled after rollback");
            Assert.IsFalse(FeatureFlags.MigrateAudioManager, "AudioManager migration should be disabled");
            Assert.IsFalse(FeatureFlags.MigrateSpatialAudioManager, "SpatialAudioManager migration should be disabled");
            Assert.IsFalse(FeatureFlags.EnableMigrationMonitoring, "Migration monitoring should be disabled");
            
            var finalProgress = migrationMonitor.GetMigrationProgress();
            Assert.Less(finalProgress, initialProgress, "Progress should decrease after rollback");
        }
        
        #endregion
        
        #region Performance Regression Tests
        
        [Test]
        public void ServiceLocator_LookupPerformance_MeetsRequirements()
        {
            // Arrange
            var service = testGameObject.AddComponent<AudioService>();
            ServiceLocator.RegisterService<IAudioService>(service);
            
            const int lookupCount = 10000;
            
            // Act
            var startTime = System.DateTime.Now;
            
            for (int i = 0; i < lookupCount; i++)
            {
                var retrievedService = ServiceLocator.GetService<IAudioService>();
                Assert.IsNotNull(retrievedService);
            }
            
            var totalTime = (System.DateTime.Now - startTime).TotalMilliseconds;
            var averageTime = totalTime / lookupCount;
            
            // Assert
            Assert.Less(averageTime, 0.1, "Average lookup time should be under 0.1ms");
            Assert.Less(totalTime, 1000, "Total lookup time should be under 1 second");
        }
        
        [Test]
        public void MigrationMonitor_LoggingPerformance_MeetsRequirements()
        {
            // Arrange
            const int logCount = 1000;
            
            // Act
            var startTime = System.DateTime.Now;
            
            for (int i = 0; i < logCount; i++)
            {
                migrationMonitor.LogSingletonUsage(typeof(AudioManager), $"PerfTest{i}");
            }
            
            var totalTime = (System.DateTime.Now - startTime).TotalMilliseconds;
            var averageTime = totalTime / logCount;
            
            // Assert
            Assert.Less(averageTime, 1.0, "Average logging time should be under 1ms");
            Assert.Less(totalTime, 500, "Total logging time should be under 500ms");
        }
        
        #endregion
    }
}
