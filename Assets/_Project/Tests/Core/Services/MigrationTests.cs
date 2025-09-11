using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using _Project.Core;
using asterivo.Unity60.Core.Audio.Interfaces;
using asterivo.Unity60.Core.Audio.Services;
using asterivo.Unity60.Core.Audio.Interfaces;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Audio;

namespace asterivo.Unity60.Tests.Core.Services
{
    /// <summary>
    /// Singleton→ServiceLocator移行テストスイート
    /// Phase 3移行計画 Step 3.3の実装
    /// </summary>
    [TestFixture]
    public class MigrationTests 
    {
        private GameObject testGameObject;
        private MigrationMonitor migrationMonitor;
        
        [SetUp]
        public void SetUp()
        {
            ServiceLocator.Clear();
            testGameObject = new GameObject("MigrationTest");
            
            // MigrationMonitorを追加
            migrationMonitor = testGameObject.AddComponent<MigrationMonitor>();
            
            // テスト用FeatureFlag設定
            FeatureFlags.UseServiceLocator = true;
            FeatureFlags.EnableDebugLogging = false;
            FeatureFlags.EnableMigrationMonitoring = true;
            FeatureFlags.EnablePerformanceMeasurement = true;
        }
        
        [TearDown]
        public void TearDown()
        {
            if (testGameObject != null)
            {
                Object.DestroyImmediate(testGameObject);
            }
            ServiceLocator.Clear();
            
            // FeatureFlagsをリセット
            FeatureFlags.ResetAll();
        }
        
        #region 基本移行互換性テスト
        
        [Test]
        public void AudioService_ServiceLocator_ReturnsValidInstance() 
        {
            // Arrange
            var audioManager = testGameObject.AddComponent<AudioManager>();
            audioManager.gameObject.SetActive(true);
            
            // Act - ✅ ServiceLocator専用実装テスト
            var serviceLocatorAudio = ServiceLocator.GetService<IAudioService>();
            var directAudio = Object.FindFirstObjectByType<AudioManager>();
            
            // Assert
            Assert.IsNotNull(serviceLocatorAudio, "ServiceLocator AudioService should not be null");
            Assert.IsNotNull(directAudio, "Direct AudioManager should not be null");
            Assert.AreSame(directAudio, serviceLocatorAudio, "ServiceLocator should return the same instance as direct access");
            
            // MigrationMonitorの記録確認
            Assert.IsNotNull(migrationMonitor, "MigrationMonitor should be present");
        }
        
        [Test]
        public void SpatialAudio_ServiceLocator_ReturnsValidInstance() 
        {
            // Arrange
            var spatialManager = testGameObject.AddComponent<SpatialAudioManager>();
            spatialManager.gameObject.SetActive(true);
            
            // Act - ✅ ServiceLocator専用実装テスト
            var serviceLocatorSpatial = ServiceLocator.GetService<ISpatialAudioService>();
            var directSpatial = Object.FindFirstObjectByType<SpatialAudioManager>();
            
            // Assert
            Assert.IsNotNull(serviceLocatorSpatial, "ServiceLocator SpatialAudioService should not be null");
            Assert.IsNotNull(directSpatial, "Direct SpatialAudioManager should not be null");
            Assert.AreSame(directSpatial, serviceLocatorSpatial, "ServiceLocator should return the same instance as direct access");
        }
        
        [Test]
        public void EffectManager_ServiceLocator_ReturnsValidInstance() 
        {
            // Arrange
            var effectManager = testGameObject.AddComponent<EffectManager>();
            effectManager.gameObject.SetActive(true);
            
            // Act - ✅ ServiceLocator専用実装テスト
            var serviceLocatorEffect = ServiceLocator.GetService<IEffectService>();
            var directEffect = Object.FindFirstObjectByType<EffectManager>();
            
            // Assert
            Assert.IsNotNull(serviceLocatorEffect, "ServiceLocator EffectService should not be null");
            Assert.IsNotNull(directEffect, "Direct EffectManager should not be null");
            Assert.AreSame(directEffect, serviceLocatorEffect, "ServiceLocator should return the same instance as direct access");
        }
        
        [Test]
        public void AudioUpdateCoordinator_MigrationCompatibility_Success() 
        {
            // Arrange
            var updateCoordinator = testGameObject.AddComponent<AudioUpdateCoordinator>();
            updateCoordinator.gameObject.SetActive(true);
            
            // Act
            var newUpdate = ServiceLocator.GetService<IAudioUpdateService>();
            
            // Assert
            Assert.IsNotNull(newUpdate, "ServiceLocator AudioUpdateService should not be null");
            Assert.AreSame(updateCoordinator, newUpdate, "UpdateCoordinator should be registered as service");
        }
        
        #endregion
        
        #region ServiceLocator全体テスト
        
        [Test]
        public void ServiceLocator_AllAudioServices_RegisteredSuccessfully()
        {
            // Arrange & Act
            var audioManager = testGameObject.AddComponent<AudioManager>();
            var spatialManager = testGameObject.AddComponent<SpatialAudioManager>();
            var effectManager = testGameObject.AddComponent<EffectManager>();
            var updateCoordinator = testGameObject.AddComponent<AudioUpdateCoordinator>();
            
            audioManager.gameObject.SetActive(true);
            spatialManager.gameObject.SetActive(true);
            effectManager.gameObject.SetActive(true);
            updateCoordinator.gameObject.SetActive(true);
            
            // Assert
            Assert.IsTrue(ServiceLocator.HasService<IAudioService>(), "IAudioService should be registered");
            Assert.IsTrue(ServiceLocator.HasService<ISpatialAudioService>(), "ISpatialAudioService should be registered");
            Assert.IsTrue(ServiceLocator.HasService<IEffectService>(), "IEffectService should be registered");
            Assert.IsTrue(ServiceLocator.HasService<IAudioUpdateService>(), "IAudioUpdateService should be registered");
            
            // サービス数の確認
            int serviceCount = ServiceLocator.GetServiceCount();
            Assert.GreaterOrEqual(serviceCount, 4, "At least 4 audio services should be registered");
        }
        
        [Test]
        public void ServiceLocator_ServiceRetrieval_ConsistentResults()
        {
            // Arrange
            var audioManager = testGameObject.AddComponent<AudioManager>();
            audioManager.gameObject.SetActive(true);
            
            // Act & Assert - 複数回取得で同じインスタンスが返されることを確認
            var service1 = ServiceLocator.GetService<IAudioService>();
            var service2 = ServiceLocator.GetService<IAudioService>();
            var service3 = ServiceLocator.GetService<IAudioService>();
            
            Assert.AreSame(service1, service2, "Multiple calls should return same instance");
            Assert.AreSame(service2, service3, "Consistency should be maintained");
            Assert.IsNotNull(service1, "Service should not be null");
        }
        
        [Test]
        public void ServiceLocator_NullServiceHandling_GracefulDegradation()
        {
            // Act - 登録されていないサービスの取得
            var nonExistentService = ServiceLocator.GetService<IStealthAudioService>();
            
            // Assert
            Assert.IsNull(nonExistentService, "Non-registered service should return null");
            Assert.DoesNotThrow(() => ServiceLocator.GetService<IStealthAudioService>(), 
                               "Getting non-existent service should not throw");
        }
        
        #endregion
        
        #region パフォーマンステスト
        
        [UnityTest]
        public IEnumerator Performance_ServiceLocator_AcceptablePerformance() 
        {
            // Arrange
            var audioManager = testGameObject.AddComponent<AudioManager>();
            audioManager.gameObject.SetActive(true);
            yield return null;
            
            const int iterations = 1000;
            
            // Measure ServiceLocator access time
            var serviceStopwatch = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                var _ = ServiceLocator.GetService<IAudioService>();
            }
            serviceStopwatch.Stop();
            
            // Assert - ✅ ServiceLocator専用パフォーマンステスト
            float serviceTimePerCall = serviceStopwatch.ElapsedMilliseconds / (float)iterations;
            
            Assert.Less(serviceTimePerCall, 0.1f, 
                       "ServiceLocator should be faster than 0.1ms per call");
            
            Debug.Log($"Performance: ServiceLocator={serviceTimePerCall:F4}ms per call");
        }
        
        [UnityTest]
        public IEnumerator Performance_MultipleServiceAccess_AcceptableTime()
        {
            // Arrange
            var audioManager = testGameObject.AddComponent<AudioManager>();
            var spatialManager = testGameObject.AddComponent<SpatialAudioManager>();
            var effectManager = testGameObject.AddComponent<EffectManager>();
            
            audioManager.gameObject.SetActive(true);
            spatialManager.gameObject.SetActive(true);
            effectManager.gameObject.SetActive(true);
            yield return null;
            
            const int iterations = 500;
            
            // Measure multiple service access time
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            for (int i = 0; i < iterations; i++) 
            {
                var audioService = ServiceLocator.GetService<IAudioService>();
                var spatialService = ServiceLocator.GetService<ISpatialAudioService>();
                var effectService = ServiceLocator.GetService<IEffectService>();
                
                Assert.IsNotNull(audioService);
                Assert.IsNotNull(spatialService);
                Assert.IsNotNull(effectService);
                
                if (i % 50 == 0) yield return null; // フレーム待機
            }
            
            stopwatch.Stop();
            
            // Assert
            float totalTimeMs = stopwatch.ElapsedMilliseconds;
            float timePerIteration = totalTimeMs / iterations;
            
            Assert.Less(totalTimeMs, 1000f, "Total time should be under 1 second");
            Assert.Less(timePerIteration, 2f, "Time per iteration should be under 2ms");
            
            Debug.Log($"Performance: {totalTimeMs}ms total, {timePerIteration:F3}ms per iteration");
        }
        
        #endregion
        
        #region FeatureFlags制御テスト
        
        [Test]
        public void FeatureFlags_MigrationControl_WorksCorrectly()
        {
            // Test basic feature flag controls
            FeatureFlags.UseNewAudioSystem = true;
            Assert.IsTrue(FeatureFlags.UseNewAudioSystem, "UseNewAudioSystem flag should be settable");
            
            FeatureFlags.EnableMigrationMonitoring = true;
            Assert.IsTrue(FeatureFlags.EnableMigrationMonitoring, "EnableMigrationMonitoring flag should be settable");
            
            FeatureFlags.EnablePerformanceMeasurement = false;
            Assert.IsFalse(FeatureFlags.EnablePerformanceMeasurement, "EnablePerformanceMeasurement flag should be settable");
        }
        
        [Test]
        public void FeatureFlags_MigrationPhaseControl_StagesWork()
        {
            // Test migration phase control
            FeatureFlags.SetMigrationPhase(0); // Reset
            Assert.IsFalse(FeatureFlags.UseNewAudioSystem, "Phase 0 should disable new audio system");
            Assert.IsTrue(FeatureFlags.AllowSingletonFallback, "Phase 0 should allow singleton fallback");
            
            FeatureFlags.SetMigrationPhase(1); // ServiceLocator preparation
            Assert.IsTrue(FeatureFlags.UseServiceLocator, "Phase 1 should enable ServiceLocator");
            Assert.IsTrue(FeatureFlags.EnableMigrationMonitoring, "Phase 1 should enable monitoring");
            
            FeatureFlags.SetMigrationPhase(3); // Complete migration
            Assert.IsTrue(FeatureFlags.UseNewAudioSystem, "Phase 3 should enable new audio system");
            Assert.IsFalse(FeatureFlags.AllowSingletonFallback, "Phase 3 should disable singleton fallback");
        }
        
        [Test]
        public void FeatureFlags_EmergencyRollback_ResetsCorrectly()
        {
            // Arrange - Set some flags to non-default values
            FeatureFlags.UseNewAudioSystem = true;
            FeatureFlags.MigrateAudioManager = true;
            FeatureFlags.AllowSingletonFallback = false;
            
            // Act
            FeatureFlags.EmergencyRollback();
            
            // Assert
            Assert.IsFalse(FeatureFlags.UseNewAudioSystem, "Emergency rollback should disable new audio system");
            Assert.IsFalse(FeatureFlags.MigrateAudioManager, "Emergency rollback should disable migration flags");
            Assert.IsTrue(FeatureFlags.AllowSingletonFallback, "Emergency rollback should enable singleton fallback");
            
            // Check emergency rollback timestamp
            string rollbackTime = FeatureFlags.GetLastEmergencyRollbackTime();
            Assert.AreNotEqual("なし", rollbackTime, "Emergency rollback time should be recorded");
        }
        
        #endregion
        
        #region MigrationMonitor統合テスト
        
        [Test]
        public void MigrationMonitor_ProgressTracking_UpdatesCorrectly()
        {
            // Arrange
            var audioManager = testGameObject.AddComponent<AudioManager>();
            audioManager.gameObject.SetActive(true);
            
            // FeatureFlagsを設定
            FeatureFlags.MigrateAudioManager = true;
            
            // Act - MonitorのUpdate処理を手動実行
            migrationMonitor.SendMessage("MonitorMigrationProgress", SendMessageOptions.DontRequireReceiver);
            
            // Assert
            float progress = migrationMonitor.GetMigrationProgress();
            Assert.Greater(progress, 0f, "Progress should be greater than 0 when services are migrated");
            Assert.LessOrEqual(progress, 100f, "Progress should not exceed 100%");
            
            bool audioMigrated = migrationMonitor.IsServiceMigrated("audio");
            Assert.IsTrue(audioMigrated, "Audio service should be marked as migrated");
        }
        
        [Test]
        public void MigrationMonitor_SafetyCheck_ValidatesCorrectly()
        {
            // Arrange
            FeatureFlags.UseServiceLocator = true;
            FeatureFlags.EnableMigrationMonitoring = true;
            FeatureFlags.EnablePerformanceMeasurement = true;
            
            // Act
            bool isSafe = migrationMonitor.IsMigrationSafe();
            
            // Assert
            Assert.IsTrue(isSafe, "Migration should be safe with proper flags enabled");
            
            // Test unsafe condition
            FeatureFlags.UseServiceLocator = false;
            bool isUnsafe = migrationMonitor.IsMigrationSafe();
            Assert.IsFalse(isUnsafe, "Migration should be unsafe without ServiceLocator");
        }
        
        [Test]
        public void MigrationMonitor_UsageLogging_RecordsCorrectly()
        {
            // Arrange
            var audioManager = testGameObject.AddComponent<AudioManager>();
            audioManager.gameObject.SetActive(true);
            
            // Act - Log some usage
            migrationMonitor.LogSingletonUsage(typeof(AudioManager), "TestLocation");
            migrationMonitor.LogServiceLocatorUsage(typeof(IAudioService), "TestLocation");
            
            // Assert
            var singletonStats = migrationMonitor.GetSingletonUsageStats();
            var serviceStats = migrationMonitor.GetServiceLocatorUsageStats();
            
            Assert.IsTrue(singletonStats.ContainsKey(typeof(AudioManager)), "Singleton usage should be recorded");
            Assert.IsTrue(serviceStats.ContainsKey(typeof(IAudioService)), "ServiceLocator usage should be recorded");
            
            Assert.AreEqual(1, singletonStats[typeof(AudioManager)], "Singleton usage count should be 1");
            Assert.AreEqual(1, serviceStats[typeof(IAudioService)], "ServiceLocator usage count should be 1");
        }
        
        #endregion
        
        #region 統合シナリオテスト
        
        [UnityTest]
        public IEnumerator Integration_CompleteAudioSystemMigration_Success()
        {
            // Arrange - 全オーディオシステムをセットアップ
            var audioManager = testGameObject.AddComponent<AudioManager>();
            var spatialManager = testGameObject.AddComponent<SpatialAudioManager>();
            var effectManager = testGameObject.AddComponent<EffectManager>();
            var updateCoordinator = testGameObject.AddComponent<AudioUpdateCoordinator>();
            
            audioManager.gameObject.SetActive(true);
            spatialManager.gameObject.SetActive(true);
            effectManager.gameObject.SetActive(true);
            updateCoordinator.gameObject.SetActive(true);
            
            yield return null; // 初期化待機
            
            // Act - 移行フラグを段階的に有効化
            FeatureFlags.SetMigrationPhase(2); // Phase 2: AudioManager migration start
            yield return null;
            
            // Assert - Phase 2の状態確認
            Assert.IsTrue(FeatureFlags.UseServiceLocator, "ServiceLocator should be enabled in Phase 2");
            Assert.IsTrue(FeatureFlags.MigrateAudioManager, "AudioManager migration should be enabled");
            Assert.IsTrue(FeatureFlags.UseNewAudioSystem, "New audio system should be enabled");
            
            // サービス機能テスト
            var audioService = ServiceLocator.GetService<IAudioService>();
            Assert.IsNotNull(audioService, "AudioService should be available");
            
            Assert.DoesNotThrow(() => audioService.SetMasterVolume(0.5f), "AudioService should function correctly");
            
            // Progress確認
            float progress = migrationMonitor.GetMigrationProgress();
            Assert.Greater(progress, 0f, "Migration progress should be recorded");
            
            yield return new WaitForSeconds(0.1f); // 監視システムの更新待機
        }
        
        [UnityTest]
        public IEnumerator Integration_ServiceLocator_ContinuesWorking()
        {
            // Arrange - ServiceLocator実装状態を作成
            var audioManager = testGameObject.AddComponent<AudioManager>();
            audioManager.gameObject.SetActive(true);
            
            FeatureFlags.SetMigrationPhase(3); // Complete migration
            yield return null;
            
            // Act - ✅ ServiceLocator専用実装テスト
            yield return null;
            
            // Assert - ServiceLocator状態確認
            Assert.IsTrue(FeatureFlags.UseServiceLocator, "ServiceLocator should be enabled");
            
            // システムが正常動作することを確認
            var serviceLocatorAudio = ServiceLocator.GetService<IAudioService>();
            Assert.IsNotNull(serviceLocatorAudio, "ServiceLocator system should work");
            
            Assert.DoesNotThrow(() => serviceLocatorAudio.SetMasterVolume(1f), "ServiceLocator system should function properly");
            
            yield return new WaitForSeconds(0.1f);
        }
        
        #endregion
        
        #region エラーハンドリングテスト
        
        [Test]
        public void ErrorHandling_MultipleServiceRegistration_HandlesGracefully()
        {
            // Arrange
            var audioManager1 = testGameObject.AddComponent<AudioManager>();
            var audioGameObject2 = new GameObject("AudioManager2");
            var audioManager2 = audioGameObject2.AddComponent<AudioManager>();
            
            try
            {
                // Act - 同じサービスを複数回登録
                audioManager1.gameObject.SetActive(true);
                audioManager2.gameObject.SetActive(true);
                
                // Assert - 後から登録したサービスが有効になることを確認
                var currentService = ServiceLocator.GetService<IAudioService>();
                Assert.IsNotNull(currentService, "Service should be available despite multiple registrations");
                
                // Warning logが出力されることを期待（実際のログ確認は困難だが、例外が出ないことを確認）
                Assert.DoesNotThrow(() => ServiceLocator.GetService<IAudioService>(), 
                                   "Multiple registrations should not cause exceptions");
            }
            finally
            {
                // Cleanup
                if (audioGameObject2 != null)
                    Object.DestroyImmediate(audioGameObject2);
            }
        }
        
        [Test]
        public void ErrorHandling_ServiceLocatorClear_HandlesGracefully()
        {
            // Arrange
            var audioManager = testGameObject.AddComponent<AudioManager>();
            audioManager.gameObject.SetActive(true);
            
            var service = ServiceLocator.GetService<IAudioService>();
            Assert.IsNotNull(service, "Service should be available initially");
            
            // Act
            ServiceLocator.Clear();
            
            // Assert
            var clearedService = ServiceLocator.GetService<IAudioService>();
            Assert.IsNull(clearedService, "Service should be null after clear");
            
            Assert.DoesNotThrow(() => ServiceLocator.GetService<IAudioService>(), 
                               "Getting service after clear should not throw");
        }
        
        #endregion
    }
}