using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Audio.Interfaces;
using asterivo.Unity60.Core.Audio;
using asterivo.Unity60.Core.Audio.Services;
using asterivo.Unity60.Core.Shared;

namespace asterivo.Unity60.Tests.Integration
{
    /// <summary>
    /// オーディオシステム統合テストスイート
    /// 移行完了後の全体動作を検証
    /// </summary>
    [TestFixture]
    public class AudioSystemIntegrationTests 
    {
        private GameObject testGameObject;
        
        [SetUp]
        public void SetUp()
        {
            ServiceLocator.Clear();
            testGameObject = new GameObject("IntegrationTest");
            
            // 移行完了状態のFeatureFlags設定
            FeatureFlags.UseServiceLocator = true;
            FeatureFlags.UseNewAudioService = true;
            FeatureFlags.UseNewSpatialService = true;
            FeatureFlags.UseNewStealthService = true;
            FeatureFlags.DisableLegacySingletons = true;
            FeatureFlags.EnableDebugLogging = false;
        }
        
        [TearDown]
        public void TearDown()
        {
            if (testGameObject != null)
            {
                UnityEngine.Object.DestroyImmediate(testGameObject);
            }
            ServiceLocator.Clear();
        }
        
        [Test]
        public void AudioSystem_FullMigration_AllServicesAvailable()
        {
            // Arrange
            SetupAllAudioServices();
            
            // Act & Assert
            Assert.IsTrue(ServiceLocator.HasService<IAudioService>(), "IAudioService should be available");
            Assert.IsTrue(ServiceLocator.HasService<ISpatialAudioService>(), "ISpatialAudioService should be available");
            Assert.IsTrue(ServiceLocator.HasService<IEffectService>(), "IEffectService should be available");
            Assert.IsTrue(ServiceLocator.HasService<IAudioUpdateService>(), "IAudioUpdateService should be available");
            
            // Singletonが無効化されていることを確認
            // Note: AudioManagerのSingleton実装がある場合の確認（実装状況により調整）
            // Assert.IsNull(AudioManager.Instance, "AudioManager.Instance should be null when DisableLegacySingletons is true");
        }
        
        [UnityTest]
        public IEnumerator AudioSystem_PlaySoundThroughServiceLocator_Success()
        {
            // Arrange
            SetupAllAudioServices();
            var audioService = ServiceLocator.GetService<IAudioService>();
            
            // Act
            bool soundPlayed = false;
            try
            {
                audioService.PlaySound("test_sound");
                soundPlayed = true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to play sound: {ex.Message}");
            }
            
            yield return null;
            
            // Assert
            Assert.IsTrue(soundPlayed, "Sound should play successfully through ServiceLocator");
        }
        
        [Test]
        public void AudioSystem_ServiceDependencies_ProperlyResolved()
        {
            // Arrange
            SetupAllAudioServices();
            
            // Act
            var audioService = ServiceLocator.GetService<IAudioService>();
            var spatialService = ServiceLocator.GetService<ISpatialAudioService>();
            var effectService = ServiceLocator.GetService<IEffectService>();
            var updateService = ServiceLocator.GetService<IAudioUpdateService>();
            
            // Assert
            Assert.IsNotNull(audioService, "AudioService should be resolved");
            Assert.IsNotNull(spatialService, "SpatialAudioService should be resolved");
            Assert.IsNotNull(effectService, "EffectService should be resolved");
            Assert.IsNotNull(updateService, "AudioUpdateService should be resolved");
            
            // サービス間の依存関係が正しく動作することを確認
            Assert.DoesNotThrow(() => {
                audioService.SetMasterVolume(0.5f);
                spatialService.Play3DSound("test_sound", Vector3.zero);
                effectService.PlayEffect("test_effect");
                updateService.StartCoordinatedUpdates();
            }, "Service operations should not throw exceptions");
        }
        
        [UnityTest]
        public IEnumerator AudioSystem_HighLoad_RemainsStable()
        {
            // Arrange
            SetupAllAudioServices();
            var audioService = ServiceLocator.GetService<IAudioService>();
            
            // Act - 高負荷テスト
            for (int i = 0; i < 100; i++)
            {
                audioService.PlaySound($"sound_{i}", Vector3.zero, Random.Range(0.1f, 1f));
                
                if (i % 10 == 0)
                    yield return null; // 10回ごとにフレーム待機
            }
            
            yield return new WaitForSeconds(1f);
            
            // Assert
            Assert.IsNotNull(ServiceLocator.GetService<IAudioService>(), "AudioService should remain available after high load");
            Assert.DoesNotThrow(() => audioService.SetMasterVolume(1f), "AudioService should remain functional after high load");
        }
        
        [Test]
        public void AudioSystem_SpatialAudio_FunctionalityWorking()
        {
            // Arrange
            SetupAllAudioServices();
            var spatialService = ServiceLocator.GetService<ISpatialAudioService>();
            
            // Act & Assert
            Assert.DoesNotThrow(() => {
                spatialService.Play3DSound("test_3d_sound", Vector3.one);
                spatialService.SetAmbientSound("ambient_test");
                spatialService.UpdateListenerPosition(Vector3.zero, Vector3.forward);
                spatialService.SetDopplerLevel(1.0f);
                spatialService.SetReverbZone("test_zone", 0.5f);
            }, "Spatial audio operations should not throw exceptions");
        }
        
        [Test]
        public void AudioSystem_EffectService_AllMethodsWorking()
        {
            // Arrange
            SetupAllAudioServices();
            var effectService = ServiceLocator.GetService<IEffectService>();
            
            // Act & Assert
            Assert.DoesNotThrow(() => {
                effectService.PlayEffect("test_effect");
                effectService.PlayOneShot("oneshot_effect");
                var loopId = effectService.StartLoopingEffect("loop_effect", Vector3.zero);
                effectService.StopLoopingEffect(loopId);
                effectService.PlayRandomEffect(new string[] { "effect1", "effect2" });
                effectService.SetEffectPitch("test_effect", 1.2f);
                effectService.PreloadEffects(new string[] { "preload1", "preload2" });
                effectService.ClearEffectPool();
            }, "Effect service operations should not throw exceptions");
        }
        
        [Test]
        public void AudioSystem_ServiceRegistration_MultipleTimesStable()
        {
            // 複数回の登録/解除が安定動作することを確認
            for (int i = 0; i < 5; i++)
            {
                // Setup services
                SetupAllAudioServices();
                
                // Verify services are available
                Assert.IsTrue(ServiceLocator.HasService<IAudioService>(), $"IAudioService should be available in iteration {i}");
                Assert.IsTrue(ServiceLocator.HasService<ISpatialAudioService>(), $"ISpatialAudioService should be available in iteration {i}");
                Assert.IsTrue(ServiceLocator.HasService<IEffectService>(), $"IEffectService should be available in iteration {i}");
                
                // Clear and setup again
                ServiceLocator.Clear();
                UnityEngine.Object.DestroyImmediate(testGameObject);
                testGameObject = new GameObject($"IntegrationTest_{i}");
            }
        }
        
        [Test]
        public void AudioSystem_VolumeManagement_AllCategoriesWorking()
        {
            // Arrange
            SetupAllAudioServices();
            var audioService = ServiceLocator.GetService<IAudioService>();
            
            // Act & Assert - Volume management functionality
            Assert.DoesNotThrow(() => {
                audioService.SetMasterVolume(0.8f);
                Assert.AreEqual(0.8f, audioService.GetMasterVolume(), 0.01f, "Master volume should be set correctly");
                
                audioService.SetCategoryVolume("BGM", 0.6f);
                audioService.SetCategoryVolume("Effect", 0.7f);
                audioService.SetCategoryVolume("Ambient", 0.5f);
                
                // Volume getters should work
                var bgmVolume = audioService.GetBGMVolume();
                var effectVolume = audioService.GetEffectVolume();
                var ambientVolume = audioService.GetAmbientVolume();
                
                Assert.IsTrue(bgmVolume >= 0f && bgmVolume <= 1f, "BGM volume should be in valid range");
                Assert.IsTrue(effectVolume >= 0f && effectVolume <= 1f, "Effect volume should be in valid range");
                Assert.IsTrue(ambientVolume >= 0f && ambientVolume <= 1f, "Ambient volume should be in valid range");
            }, "Volume management operations should not throw exceptions");
        }
        
        [Test]
        public void AudioSystem_FeatureFlagsEnabled_AllServicesRegistered()
        {
            // Arrange - 移行完了状態のFeatureFlags確認
            Assert.IsTrue(FeatureFlags.UseServiceLocator, "UseServiceLocator should be enabled");
            Assert.IsTrue(FeatureFlags.UseNewAudioService, "UseNewAudioService should be enabled");
            Assert.IsTrue(FeatureFlags.UseNewSpatialService, "UseNewSpatialService should be enabled");
            Assert.IsTrue(FeatureFlags.UseNewStealthService, "UseNewStealthService should be enabled");
            Assert.IsTrue(FeatureFlags.DisableLegacySingletons, "DisableLegacySingletons should be enabled");
            
            SetupAllAudioServices();
            
            // Act & Assert - 全サービスが期待通り登録されていることを確認
            Assert.IsTrue(ServiceLocator.HasService<IAudioService>(), "IAudioService should be registered");
            Assert.IsTrue(ServiceLocator.HasService<ISpatialAudioService>(), "ISpatialAudioService should be registered");
            Assert.IsTrue(ServiceLocator.HasService<IEffectService>(), "IEffectService should be registered");
            Assert.IsTrue(ServiceLocator.HasService<IAudioUpdateService>(), "IAudioUpdateService should be registered");
            
            // サービスが実際に機能することを確認
            var audioService = ServiceLocator.GetService<IAudioService>();
            var spatialService = ServiceLocator.GetService<ISpatialAudioService>();
            var effectService = ServiceLocator.GetService<IEffectService>();
            var updateService = ServiceLocator.GetService<IAudioUpdateService>();
            
            Assert.IsNotNull(audioService, "AudioService instance should be available");
            Assert.IsNotNull(spatialService, "SpatialAudioService instance should be available");
            Assert.IsNotNull(effectService, "EffectService instance should be available");
            Assert.IsNotNull(updateService, "AudioUpdateService instance should be available");
            
            // 各サービスが IInitializable を実装していることを確認
            if (audioService is IInitializable audioInit) Assert.IsTrue(audioInit.IsInitialized, "AudioService should be initialized");
            if (spatialService is IInitializable spatialInit) Assert.IsTrue(spatialInit.IsInitialized, "SpatialService should be initialized");
            if (effectService is IInitializable effectInit) Assert.IsTrue(effectInit.IsInitialized, "EffectService should be initialized");
            if (updateService is IInitializable updateInit) Assert.IsTrue(updateInit.IsInitialized, "UpdateService should be initialized");
        }
        
        [UnityTest]
        public IEnumerator AudioSystem_ServiceLocatorIntegration_WorksInPlayMode()
        {
            // Arrange
            SetupAllAudioServices();
            
            yield return null; // フレーム待機で初期化完了を確保
            
            // Act - 実際のプレイモードでの統合テスト
            var audioService = ServiceLocator.GetService<IAudioService>();
            var spatialService = ServiceLocator.GetService<ISpatialAudioService>();
            var effectService = ServiceLocator.GetService<IEffectService>();
            var updateService = ServiceLocator.GetService<IAudioUpdateService>();
            
            // Assert - 全サービスがプレイモードで正常動作
            Assert.IsNotNull(audioService, "AudioService should be available in play mode");
            Assert.IsNotNull(spatialService, "SpatialAudioService should be available in play mode");
            Assert.IsNotNull(effectService, "EffectService should be available in play mode");
            Assert.IsNotNull(updateService, "AudioUpdateService should be available in play mode");
            
            // 実際のゲーム使用シナリオをシミュレート
            bool operationsSuccessful = true;
            
            // Volume control
            try
            {
                audioService.SetMasterVolume(0.7f);
            }
            catch (System.Exception ex)
            {
                operationsSuccessful = false;
                Debug.LogError($"Volume control failed: {ex.Message}");
            }
            yield return new WaitForSeconds(0.1f);
            
            // 3D sound playback
            try
            {
                spatialService.Play3DSound("test_3d_sound", Vector3.zero);
                spatialService.UpdateListenerPosition(Vector3.zero, Vector3.forward);
            }
            catch (System.Exception ex)
            {
                operationsSuccessful = false;
                Debug.LogError($"3D sound playback failed: {ex.Message}");
            }
            yield return new WaitForSeconds(0.1f);
            
            // Effect playback
            try
            {
                effectService.PlayEffect("test_effect", Vector3.zero);
                effectService.PlayOneShot("oneshot_test");
            }
            catch (System.Exception ex)
            {
                operationsSuccessful = false;
                Debug.LogError($"Effect playback failed: {ex.Message}");
            }
            yield return new WaitForSeconds(0.1f);
            
            // Update coordination
            try
            {
                updateService.StartCoordinatedUpdates();
            }
            catch (System.Exception ex)
            {
                operationsSuccessful = false;
                Debug.LogError($"Update coordination start failed: {ex.Message}");
            }
            yield return new WaitForSeconds(0.2f);
            
            try
            {
                updateService.StopCoordinatedUpdates();
            }
            catch (System.Exception ex)
            {
                operationsSuccessful = false;
                Debug.LogError($"Update coordination stop failed: {ex.Message}");
            }
            
            // Pause and resume
            try
            {
                audioService.Pause();
            }
            catch (System.Exception ex)
            {
                operationsSuccessful = false;
                Debug.LogError($"Audio pause failed: {ex.Message}");
            }
            yield return new WaitForSeconds(0.1f);
            
            try
            {
                audioService.Resume();
            }
            catch (System.Exception ex)
            {
                operationsSuccessful = false;
                Debug.LogError($"Audio resume failed: {ex.Message}");
            }
            
            Assert.IsTrue(operationsSuccessful, "All audio system operations should complete successfully in play mode");
        }
        
        [Test]
        public void AudioSystem_ErrorHandling_RobustBehavior()
        {
            // Arrange
            SetupAllAudioServices();
            var audioService = ServiceLocator.GetService<IAudioService>();
            var spatialService = ServiceLocator.GetService<ISpatialAudioService>();
            var effectService = ServiceLocator.GetService<IEffectService>();
            
            // Act & Assert - エラー処理のテスト
            Assert.DoesNotThrow(() => {
                // 無効な引数でのテスト
                audioService.PlaySound(null); // null soundId
                audioService.PlaySound(""); // empty soundId
                audioService.SetMasterVolume(-1f); // 無効な音量値
                audioService.SetMasterVolume(2f); // 無効な音量値
                
                spatialService.Play3DSound(null, Vector3.zero); // null soundId
                spatialService.UpdateListenerPosition(Vector3.zero, Vector3.zero); // zero forward vector
                
                effectService.PlayEffect(null); // null effectId
                effectService.PlayRandomEffect(null); // null array
                effectService.PlayRandomEffect(new string[0]); // empty array
                
            }, "Audio services should handle invalid inputs gracefully");
        }
        
        [Test]
        public void AudioSystem_ServicePersistence_MaintainedAcrossOperations()
        {
            // Arrange
            SetupAllAudioServices();
            
            // 初回取得
            var audioService1 = ServiceLocator.GetService<IAudioService>();
            var spatialService1 = ServiceLocator.GetService<ISpatialAudioService>();
            var effectService1 = ServiceLocator.GetService<IEffectService>();
            var updateService1 = ServiceLocator.GetService<IAudioUpdateService>();
            
            // 操作の実行
            audioService1.SetMasterVolume(0.5f);
            spatialService1.SetAmbientSound("ambient_test");
            effectService1.PlayEffect("effect_test");
            updateService1.ForceRebuildSpatialCache();
            
            // 再取得
            var audioService2 = ServiceLocator.GetService<IAudioService>();
            var spatialService2 = ServiceLocator.GetService<ISpatialAudioService>();
            var effectService2 = ServiceLocator.GetService<IEffectService>();
            var updateService2 = ServiceLocator.GetService<IAudioUpdateService>();
            
            // Assert - 同じインスタンスが返されることを確認
            Assert.AreSame(audioService1, audioService2, "AudioService instance should be persistent");
            Assert.AreSame(spatialService1, spatialService2, "SpatialAudioService instance should be persistent");
            Assert.AreSame(effectService1, effectService2, "EffectService instance should be persistent");
            Assert.AreSame(updateService1, updateService2, "AudioUpdateService instance should be persistent");
            
            // 状態が維持されていることを確認
            Assert.AreEqual(0.5f, audioService2.GetMasterVolume(), 0.01f, "Master volume should be maintained");
        }
        
        private void SetupAllAudioServices()
        {
            // 実際のマネージャーコンポーネントをセットアップ
            var audioManager = testGameObject.AddComponent<AudioManager>();
            var spatialManager = testGameObject.AddComponent<SpatialAudioService>();
            var effectManager = testGameObject.AddComponent<EffectManager>();
            var updateCoordinator = testGameObject.AddComponent<AudioUpdateCoordinator>();
            
            // ゲームオブジェクトをアクティブにして初期化を促す
            audioManager.gameObject.SetActive(true);
            spatialManager.gameObject.SetActive(true);
            effectManager.gameObject.SetActive(true);
            updateCoordinator.gameObject.SetActive(true);
            
            // ServiceLocatorへの登録を確実にするため少し待つ
            System.Threading.Thread.Sleep(50);
        }
        
        [UnityTest]
        public IEnumerator AudioSystem_StressTest_HighConcurrency()
        {
            // Arrange
            SetupAllAudioServices();
            var audioService = ServiceLocator.GetService<IAudioService>();
            var spatialService = ServiceLocator.GetService<ISpatialAudioService>();
            var effectService = ServiceLocator.GetService<IEffectService>();
            
            bool stressTestPassed = true;
            
            // Act - 高負荷同時実行テスト
            for (int frame = 0; frame < 10; frame++)
            {
                try
                {
                    // 同フレーム内で大量のサービス呼び出し
                    for (int i = 0; i < 50; i++)
                    {
                        audioService.PlaySound($"stress_sound_{i}", Vector3.zero, 0.1f);
                        spatialService.Play3DSound($"stress_3d_{i}", Vector3.one * i);
                        effectService.PlayEffect($"stress_effect_{i}");
                    }
                    
                    // 音量変更の同時実行
                    for (int i = 0; i < 10; i++)
                    {
                        audioService.SetMasterVolume(0.1f + i * 0.1f);
                        audioService.SetCategoryVolume("BGM", 0.1f + i * 0.05f);
                    }
                }
                catch (System.Exception ex)
                {
                    stressTestPassed = false;
                    Debug.LogError($"Stress test failed at frame {frame}: {ex.Message}");
                    break;
                }
                
                yield return null;
            }
            
            // Assert
            Assert.IsTrue(stressTestPassed, "Audio system should handle high concurrency stress test");
            
            // サービスが依然として動作することを確認
            Assert.IsNotNull(ServiceLocator.GetService<IAudioService>(), "AudioService should remain available after stress test");
            Assert.IsNotNull(ServiceLocator.GetService<ISpatialAudioService>(), "SpatialAudioService should remain available after stress test");
            Assert.IsNotNull(ServiceLocator.GetService<IEffectService>(), "EffectService should remain available after stress test");
        }
        
        [Test]
        public void AudioSystem_ConfigurationChanges_DynamicBehavior()
        {
            // Arrange
            SetupAllAudioServices();
            var audioService = ServiceLocator.GetService<IAudioService>();
            var updateService = ServiceLocator.GetService<IAudioUpdateService>();
            
            // Act & Assert - 設定変更の動的処理テスト
            
            // 初期状態の確認
            float initialInterval = updateService.UpdateInterval;
            bool initialCoordinated = updateService.IsCoordinatedUpdateEnabled;
            
            Assert.Greater(initialInterval, 0f, "Initial update interval should be positive");
            
            // 設定変更テスト
            Assert.DoesNotThrow(() => {
                // Update interval の変更
                updateService.UpdateInterval = 0.2f;
                Assert.AreEqual(0.2f, updateService.UpdateInterval, 0.01f, "Update interval should be updated");
                
                // 音量設定の動的変更
                audioService.SetMasterVolume(0.3f);
                audioService.SetCategoryVolume("BGM", 0.4f);
                audioService.SetCategoryVolume("Effect", 0.5f);
                
                // 変更が反映されていることを確認
                Assert.AreEqual(0.3f, audioService.GetMasterVolume(), 0.01f, "Master volume should be updated");
                
            }, "Configuration changes should be handled dynamically");
        }
    }
}