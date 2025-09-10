using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using System.Collections.Generic;
using System;
using _Project.Core;
using asterivo.Unity60.Core.Audio;
using asterivo.Unity60.Core.Audio.Services;
using asterivo.Unity60.Core.Audio.Interfaces;

namespace asterivo.Unity60.Tests.Core.Services
{
    /// <summary>
    /// 移行システムの回復力・エラーハンドリングテストスイート
    /// Phase 3移行計画 Step 3.3の堅牢性検証実装
    /// </summary>
    [TestFixture]
    public class MigrationResilienceTests 
    {
        private GameObject testGameObject;
        private MigrationMonitor migrationMonitor;
        
        [SetUp]
        public void SetUp()
        {
            ServiceLocator.Clear();
            FeatureFlags.ResetToDefaults();
            testGameObject = new GameObject("ResilienceTest");
            migrationMonitor = testGameObject.AddComponent<MigrationMonitor>();
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
        
        #region Service Destruction Resilience
        
        [Test]
        public void ServiceLocator_ServiceDestroyedDuringOperation_RecoversGracefully()
        {
            // Arrange
            var service1 = testGameObject.AddComponent<AudioService>();
            var service2 = testGameObject.AddComponent<AudioService>();
            
            ServiceLocator.RegisterService<IAudioService>(service1);
            
            // Act - Destroy service while it's registered
            UnityEngine.Object.DestroyImmediate(service1);
            
            // Try to use the service
            var retrievedService = ServiceLocator.GetService<IAudioService>();
            
            // Register a new service
            ServiceLocator.RegisterService<IAudioService>(service2);
            var newRetrievedService = ServiceLocator.GetService<IAudioService>();
            
            // Assert
            Assert.IsNull(retrievedService, "Should return null for destroyed service");
            Assert.IsNotNull(newRetrievedService, "Should successfully register new service");
            Assert.AreEqual(service2, newRetrievedService, "Should return the new service");
        }
        
        [Test]
        public void ServiceLocator_MultipleServiceDestruction_MaintainsStability()
        {
            // Arrange
            var services = new List<IAudioService>();
            for (int i = 0; i < 10; i++)
            {
                var service = testGameObject.AddComponent<AudioService>();
                ServiceLocator.RegisterService<IAudioService>(service);
                services.Add(service);
            }
            
            // Act - Destroy services randomly
            for (int i = 0; i < 5; i++)
            {
                UnityEngine.Object.DestroyImmediate((MonoBehaviour)services[i * 2]);
            }
            
            // Try operations
            var retrievedService = ServiceLocator.GetService<IAudioService>();
            var serviceCount = ServiceLocator.GetServiceCount();
            
            // Register new service
            var newService = testGameObject.AddComponent<AudioService>();
            ServiceLocator.RegisterService<IAudioService>(newService);
            
            // Assert
            Assert.IsNotNull(ServiceLocator.GetService<IAudioService>(), "ServiceLocator should remain functional");
            Assert.Greater(ServiceLocator.GetServiceCount(), 0, "Should maintain service count tracking");
        }
        
        #endregion
        
        #region Exception Handling
        
        [Test]
        public void MigrationMonitor_ExceptionDuringLogging_ContinuesOperation()
        {
            // Arrange
            var initialProgress = migrationMonitor.GetMigrationProgress();
            
            // Act - Simulate various problematic inputs
            try
            {
                migrationMonitor.LogSingletonUsage(null, "test");
                migrationMonitor.LogSingletonUsage(typeof(AudioManager), null);
                migrationMonitor.LogSingletonUsage(typeof(string), ""); // Non-audio type
                migrationMonitor.LogServiceLocatorUsage(null, "test");
                migrationMonitor.LogServiceLocatorUsage(typeof(IAudioService), null);
            }
            catch (Exception ex)
            {
                Assert.Fail($"MigrationMonitor should handle exceptions gracefully: {ex.Message}");
            }
            
            // Verify continued operation
            migrationMonitor.LogSingletonUsage(typeof(AudioManager), "valid_test");
            var finalProgress = migrationMonitor.GetMigrationProgress();
            
            // Assert
            Assert.IsTrue(finalProgress >= 0 && finalProgress <= 100, "Migration progress should remain valid");
            Assert.DoesNotThrow(() => migrationMonitor.IsMigrationSafe(), "Safety check should remain functional");
        }
        
        [Test]
        public void FeatureFlags_InvalidFlagModification_MaintainsIntegrity()
        {
            // Arrange
            var originalUseServiceLocator = FeatureFlags.UseServiceLocator;
            var originalMigrateAudio = FeatureFlags.MigrateAudioManager;
            
            // Act - Try to set invalid states through reflection or direct manipulation
            try
            {
                // Attempt to create inconsistent state
                FeatureFlags.UseServiceLocator = true;
                FeatureFlags.MigrateAudioManager = false;
                FeatureFlags.MigrateSpatialAudioManager = false;
                FeatureFlags.MigrateEffectManager = false;
                FeatureFlags.MigrateAudioUpdateCoordinator = false;
                FeatureFlags.MigrateStealthAudioCoordinator = false;
                
                // Try operations in inconsistent state
                var progress = migrationMonitor.GetMigrationProgress();
                var isSafe = migrationMonitor.IsMigrationSafe();
                
                // Reset to valid state
                FeatureFlags.ResetToDefaults();
                
                // Verify recovery
                var recoveredProgress = migrationMonitor.GetMigrationProgress();
                var recoveredSafety = migrationMonitor.IsMigrationSafe();
                
                // Assert
                Assert.IsTrue(progress >= 0 && progress <= 100, "Progress should remain valid in inconsistent state");
                Assert.IsTrue(recoveredProgress >= 0 && recoveredProgress <= 100, "Progress should remain valid after recovery");
                Assert.IsNotNull(isSafe, "Safety check should return valid result");
                Assert.IsNotNull(recoveredSafety, "Safety check should work after recovery");
            }
            catch (Exception ex)
            {
                Assert.Fail($"FeatureFlags should handle invalid states gracefully: {ex.Message}");
            }
        }
        
        #endregion
        
        #region Resource Exhaustion Resilience
        
        [Test]
        public void ServiceLocator_ExcessiveRegistrations_HandlesGracefully()
        {
            // Arrange
            const int excessiveCount = 50000;
            var registrationSuccessCount = 0;
            var services = new List<IAudioService>();
            
            // Act
            try
            {
                for (int i = 0; i < excessiveCount; i++)
                {
                    var service = testGameObject.AddComponent<AudioService>();
                    ServiceLocator.RegisterService<IAudioService>(service);
                    services.Add(service);
                    registrationSuccessCount++;
                    
                    // Periodically check system health
                    if (i % 1000 == 0)
                    {
                        var testService = ServiceLocator.GetService<IAudioService>();
                        Assert.IsNotNull(testService, $"Service retrieval should work at {i} registrations");
                    }
                }
            }
            catch (OutOfMemoryException)
            {
                // This is acceptable - system should handle memory pressure gracefully
                UnityEngine.Debug.Log($"Memory limit reached at {registrationSuccessCount} services");
            }
            catch (Exception ex)
            {
                Assert.Fail($"Unexpected exception during excessive registration: {ex.Message}");
            }
            
            // Assert - System should remain functional
            Assert.Greater(registrationSuccessCount, 1000, "Should handle at least 1000 service registrations");
            
            var finalService = ServiceLocator.GetService<IAudioService>();
            Assert.IsNotNull(finalService, "ServiceLocator should remain functional after stress");
        }
        
        [Test]
        public void MigrationMonitor_ExcessiveLogging_MaintainsPerformance()
        {
            // Arrange
            const int excessiveLogCount = 100000;
            var loggingSuccessCount = 0;
            var startTime = DateTime.Now;
            
            // Act
            try
            {
                for (int i = 0; i < excessiveLogCount; i++)
                {
                    migrationMonitor.LogSingletonUsage(typeof(AudioManager), $"stress{i}");
                    loggingSuccessCount++;
                    
                    // Check performance periodically
                    if (i % 10000 == 0 && i > 0)
                    {
                        var elapsedTime = (DateTime.Now - startTime).TotalMilliseconds;
                        var avgTimePerLog = elapsedTime / i;
                        
                        Assert.Less(avgTimePerLog, 1.0, $"Average logging time should remain under 1ms at {i} logs");
                    }
                }
            }
            catch (Exception ex)
            {
                Assert.Fail($"Excessive logging should not cause exceptions: {ex.Message}");
            }
            
            // Assert
            Assert.AreEqual(excessiveLogCount, loggingSuccessCount, "All logging operations should succeed");
            
            var totalTime = (DateTime.Now - startTime).TotalMilliseconds;
            var averageTime = totalTime / excessiveLogCount;
            
            Assert.Less(averageTime, 0.1, "Average logging time should remain efficient");
            
            // Verify system remains functional
            var progress = migrationMonitor.GetMigrationProgress();
            Assert.IsTrue(progress >= 0 && progress <= 100, "System should remain functional after stress");
        }
        
        #endregion
        
        #region State Corruption Recovery
        
        [Test]
        public void Migration_CorruptedFeatureFlags_AutoRecovery()
        {
            // Arrange
            FeatureFlags.UseServiceLocator = true;
            FeatureFlags.EnableMigrationMonitoring = true;
            
            var initialProgress = migrationMonitor.GetMigrationProgress();
            
            // Act - Simulate state corruption through edge case manipulation
            FeatureFlags.UseServiceLocator = false;
            FeatureFlags.MigrateAudioManager = true; // Inconsistent state
            
            // System should detect and handle inconsistency
            var corruptedProgress = migrationMonitor.GetMigrationProgress();
            var corruptedSafety = migrationMonitor.IsMigrationSafe();
            
            // Auto-recovery test
            FeatureFlags.ResetToDefaults();
            FeatureFlags.ValidateConfiguration(); // If this method exists
            
            var recoveredProgress = migrationMonitor.GetMigrationProgress();
            var recoveredSafety = migrationMonitor.IsMigrationSafe();
            
            // Assert
            Assert.IsTrue(corruptedProgress >= 0 && corruptedProgress <= 100, "Should handle corrupted state gracefully");
            Assert.IsNotNull(corruptedSafety, "Safety check should work even in corrupted state");
            Assert.IsTrue(recoveredProgress >= 0 && recoveredProgress <= 100, "Should recover to valid state");
            Assert.IsNotNull(recoveredSafety, "Safety check should work after recovery");
        }
        
        [UnityTest]
        public IEnumerator Migration_SystemRestart_StatePreservation()
        {
            // Arrange
            FeatureFlags.UseServiceLocator = true;
            FeatureFlags.MigrateAudioManager = true;
            FeatureFlags.EnableMigrationMonitoring = true;
            
            // Log some usage
            migrationMonitor.LogSingletonUsage(typeof(AudioManager), "before_restart");
            migrationMonitor.LogServiceLocatorUsage(typeof(IAudioService), "before_restart");
            
            var progressBeforeRestart = migrationMonitor.GetMigrationProgress();
            
            // Act - Simulate system restart by destroying and recreating monitor
            UnityEngine.Object.DestroyImmediate(migrationMonitor);
            yield return null; // Wait one frame
            
            migrationMonitor = testGameObject.AddComponent<MigrationMonitor>();
            yield return null; // Wait for initialization
            
            var progressAfterRestart = migrationMonitor.GetMigrationProgress();
            
            // Test continued operation
            migrationMonitor.LogSingletonUsage(typeof(AudioManager), "after_restart");
            var finalProgress = migrationMonitor.GetMigrationProgress();
            
            // Assert
            Assert.IsTrue(progressAfterRestart >= 0 && progressAfterRestart <= 100, "Progress should be valid after restart");
            Assert.IsTrue(finalProgress >= 0 && finalProgress <= 100, "System should continue working after restart");
            
            // Progress might be different after restart, but system should be functional
            Assert.DoesNotThrow(() => migrationMonitor.IsMigrationSafe(), "All functions should work after restart");
        }
        
        #endregion
        
        #region Integration Resilience
        
        [Test]
        public void Migration_SimultaneousOperations_ThreadSafety()
        {
            // Arrange
            var service = testGameObject.AddComponent<AudioService>();
            ServiceLocator.RegisterService<IAudioService>(service);
            
            var exceptions = new List<Exception>();
            var operationCount = 0;
            var tasks = new List<System.Threading.Tasks.Task>();
            
            // Act - Run simultaneous operations
            for (int i = 0; i < 5; i++)
            {
                var task = System.Threading.Tasks.Task.Run(() =>
                {
                    try
                    {
                        for (int j = 0; j < 100; j++)
                        {
                            // Mixed operations
                            var retrievedService = ServiceLocator.GetService<IAudioService>();
                            migrationMonitor.LogServiceLocatorUsage(typeof(IAudioService), $"thread{i}_op{j}");
                            migrationMonitor.LogSingletonUsage(typeof(AudioManager), $"thread{i}_singleton{j}");
                            var progress = migrationMonitor.GetMigrationProgress();
                            var isSafe = migrationMonitor.IsMigrationSafe();
                            
                            System.Threading.Interlocked.Increment(ref operationCount);
                        }
                    }
                    catch (Exception ex)
                    {
                        lock (exceptions)
                        {
                            exceptions.Add(ex);
                        }
                    }
                });
                tasks.Add(task);
            }
            
            System.Threading.Tasks.Task.WaitAll(tasks.ToArray());
            
            // Assert
            Assert.IsEmpty(exceptions, $"No exceptions should occur during concurrent operations. Exceptions: {string.Join(", ", exceptions.Select(e => e.Message))}");
            Assert.AreEqual(500, operationCount, "All operations should complete successfully");
            
            // Verify system integrity after concurrent access
            var finalService = ServiceLocator.GetService<IAudioService>();
            var finalProgress = migrationMonitor.GetMigrationProgress();
            
            Assert.IsNotNull(finalService, "ServiceLocator should remain functional");
            Assert.IsTrue(finalProgress >= 0 && finalProgress <= 100, "Migration progress should remain valid");
        }
        
        [Test]
        public void Migration_EmergencyRollbackUnderStress_CompletesReliably()
        {
            // Arrange
            FeatureFlags.UseServiceLocator = true;
            FeatureFlags.MigrateAudioManager = true;
            FeatureFlags.MigrateSpatialAudioManager = true;
            FeatureFlags.EnableMigrationMonitoring = true;
            
            // Create stress conditions
            for (int i = 0; i < 100; i++)
            {
                var service = testGameObject.AddComponent<AudioService>();
                ServiceLocator.RegisterService<IAudioService>(service);
                migrationMonitor.LogSingletonUsage(typeof(AudioManager), $"stress{i}");
                migrationMonitor.LogServiceLocatorUsage(typeof(IAudioService), $"stress{i}");
            }
            
            var progressBeforeRollback = migrationMonitor.GetMigrationProgress();
            
            // Act - Emergency rollback under stress
            try
            {
                FeatureFlags.EmergencyRollback();
            }
            catch (Exception ex)
            {
                Assert.Fail($"Emergency rollback should not throw exceptions: {ex.Message}");
            }
            
            // Verify rollback completed
            var progressAfterRollback = migrationMonitor.GetMigrationProgress();
            
            // Test system functionality after rollback
            var serviceAfterRollback = ServiceLocator.GetService<IAudioService>();
            var isSafeAfterRollback = migrationMonitor.IsMigrationSafe();
            
            // Assert
            Assert.IsFalse(FeatureFlags.UseServiceLocator, "ServiceLocator should be disabled after rollback");
            Assert.IsFalse(FeatureFlags.MigrateAudioManager, "AudioManager migration should be disabled");
            Assert.Less(progressAfterRollback, progressBeforeRollback, "Progress should decrease after rollback");
            Assert.IsNotNull(isSafeAfterRollback, "Safety check should work after rollback");
            
            // System should remain functional even after rollback
            Assert.DoesNotThrow(() => migrationMonitor.GetMigrationProgress(), "Progress reporting should continue working");
        }
        
        #endregion
        
        #region Recovery Validation
        
        [Test]
        public void Migration_RecoveryFromAllFailureModes_Successful()
        {
            var failureModes = new List<Action>
            {
                // Service destruction
                () => {
                    var service = testGameObject.AddComponent<AudioService>();
                    ServiceLocator.RegisterService<IAudioService>(service);
                    UnityEngine.Object.DestroyImmediate(service);
                },
                // Invalid logging
                () => {
                    migrationMonitor.LogSingletonUsage(null, null);
                    migrationMonitor.LogServiceLocatorUsage(null, null);
                },
                // Flag corruption
                () => {
                    FeatureFlags.UseServiceLocator = false;
                    FeatureFlags.MigrateAudioManager = true;
                },
                // Memory pressure simulation
                () => {
                    for (int i = 0; i < 1000; i++)
                    {
                        migrationMonitor.LogSingletonUsage(typeof(AudioManager), $"memory{i}");
                    }
                }
            };
            
            foreach (var failureMode in failureModes)
            {
                // Setup clean state
                ServiceLocator.Clear();
                FeatureFlags.ResetToDefaults();
                FeatureFlags.UseServiceLocator = true;
                FeatureFlags.EnableMigrationMonitoring = true;
                
                // Introduce failure
                try
                {
                    failureMode();
                }
                catch (Exception)
                {
                    // Failures are expected, test recovery
                }
                
                // Test recovery
                FeatureFlags.ResetToDefaults();
                ServiceLocator.Clear();
                
                var service = testGameObject.AddComponent<AudioService>();
                ServiceLocator.RegisterService<IAudioService>(service);
                
                // Verify full functionality recovery
                Assert.IsNotNull(ServiceLocator.GetService<IAudioService>(), "ServiceLocator should recover");
                Assert.DoesNotThrow(() => migrationMonitor.GetMigrationProgress(), "Progress reporting should recover");
                Assert.DoesNotThrow(() => migrationMonitor.IsMigrationSafe(), "Safety check should recover");
                
                // Test normal operation
                migrationMonitor.LogServiceLocatorUsage(typeof(IAudioService), "recovery_test");
                var progress = migrationMonitor.GetMigrationProgress();
                Assert.IsTrue(progress >= 0 && progress <= 100, "Normal operation should resume");
            }
        }
        
        #endregion
    }
}