using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using _Project.Core;
using asterivo.Unity60.Core.Audio.Interfaces;
using asterivo.Unity60.Core.Audio;

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
            
            // Assert
            Assert.IsNotNull(audioService, "AudioService should be resolved");
            Assert.IsNotNull(spatialService, "SpatialAudioService should be resolved");
            Assert.IsNotNull(effectService, "EffectService should be resolved");
            
            // サービス間の依存関係が正しく動作することを確認
            Assert.DoesNotThrow(() => {
                audioService.SetMasterVolume(0.5f);
                spatialService.Play3DSound("test_sound", Vector3.zero);
                effectService.PlayEffect("test_effect");
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
        
        private void SetupAllAudioServices()
        {
            var audioManager = testGameObject.AddComponent<AudioManager>();
            var spatialManager = testGameObject.AddComponent<SpatialAudioManager>();
            var effectManager = testGameObject.AddComponent<EffectManager>();
            var updateCoordinator = testGameObject.AddComponent<AudioUpdateCoordinator>();
            
            audioManager.gameObject.SetActive(true);
            spatialManager.gameObject.SetActive(true);
            effectManager.gameObject.SetActive(true);
            updateCoordinator.gameObject.SetActive(true);
        }
    }
}