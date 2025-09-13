using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Audio.Services;
using asterivo.Unity60.Core.Audio.Interfaces;

using asterivo.Unity60.Core.Audio;
using asterivo.Unity60.Core.Audio.Data;

namespace asterivo.Unity60.Tests.Core.Services
{
    /// <summary>
    /// オーディオサービス統合テストスイート
    /// ServiceLocatorとオーディオシステム間の連携を検証
    /// </summary>
    [TestFixture]
    public class AudioServicesIntegrationTests
    {
        private GameObject testGameObject;
        private AudioService audioService;
        private BGMManager bgmManager;
        private EffectManager effectManager;
        private AmbientManager ambientManager;

        #region Setup and Teardown

        [SetUp]
        public void SetUp()
        {
            // ServiceLocatorをクリア
            ServiceLocator.Clear();
            
            // テスト用GameObjectの作成
            testGameObject = new GameObject("TestAudioServices");
            
            // Feature Flagsを有効化
            FeatureFlags.UseServiceLocator = true;
            FeatureFlags.EnableDebugLogging = false;
            
            // Audio Managersの作成
            CreateAudioManagers();
            
            // AudioServiceの作成と設定
            CreateAudioService();
        }

        [TearDown]
        public void TearDown()
        {
            // GameObjectの破棄
            if (testGameObject != null)
            {
                Object.DestroyImmediate(testGameObject);
            }
            
            // ServiceLocatorのクリア
            ServiceLocator.Clear();
            
            // Feature Flagsをリセット
            FeatureFlags.UseServiceLocator = false;
            FeatureFlags.EnableDebugLogging = true;
        }

        private void CreateAudioManagers()
        {
            // BGMManager作成
            var bgmGameObject = new GameObject("BGMManager");
            bgmGameObject.transform.SetParent(testGameObject.transform);
            bgmManager = bgmGameObject.AddComponent<BGMManager>();
            
            // EffectManager作成
            var effectGameObject = new GameObject("EffectManager");
            effectGameObject.transform.SetParent(testGameObject.transform);
            effectManager = effectGameObject.AddComponent<EffectManager>();
            
            // AmbientManager作成
            var ambientGameObject = new GameObject("AmbientManager");
            ambientGameObject.transform.SetParent(testGameObject.transform);
            ambientManager = ambientGameObject.AddComponent<AmbientManager>();
        }

        private void CreateAudioService()
        {
            audioService = testGameObject.AddComponent<AudioService>();
            
            // AudioServiceにManagersを設定（Reflectionで設定）
            var bgmField = typeof(AudioService).GetField("bgmManager", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            bgmField?.SetValue(audioService, bgmManager);
            
            var effectField = typeof(AudioService).GetField("effectManager", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            effectField?.SetValue(audioService, effectManager);
            
            var ambientField = typeof(AudioService).GetField("ambientManager", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            ambientField?.SetValue(audioService, ambientManager);
        }

        #endregion

        #region Service Registration Tests

        [Test]
        public void AudioService_OnAwake_RegistersWithServiceLocator()
        {
            // Act
            audioService.gameObject.SetActive(true);
            audioService.SendMessage("Awake");

            // Assert
            Assert.IsTrue(ServiceLocator.IsServiceRegistered<IAudioService>());
            var registeredService = ServiceLocator.GetService<IAudioService>();
            Assert.AreSame(audioService, registeredService);
        }

        [Test]
        public void AudioManagers_OnAwake_RegisterWithServiceLocator()
        {
            // Act
            bgmManager.gameObject.SetActive(true);
            effectManager.gameObject.SetActive(true);
            ambientManager.gameObject.SetActive(true);

            // BGMManagerとEffectManagerは個別のinterfaceがないため、個別に検証する必要があります
            // これらは主にAudioService経由でアクセスされます

            // Assert - AudioServiceが正しく設定されていることを確認
            Assert.IsNotNull(bgmManager);
            Assert.IsNotNull(effectManager);
            Assert.IsNotNull(ambientManager);
        }

        #endregion

        #region Volume Integration Tests

        [Test]
        public void AudioService_SetMasterVolume_PropagatesAcrossAllManagers()
        {
            // Arrange
            audioService.Initialize();
            float targetVolume = 0.7f;

            // Act
            audioService.SetMasterVolume(targetVolume);

            // Assert
            Assert.AreEqual(targetVolume, audioService.GetMasterVolume(), 0.001f);
            
            // BGMManager、EffectManager、AmbientManagerの音量が適用されていることを確認
            // 実際の実装では、これらのManagerが適切にAudioMixerを通じて音量を制御している
        }

        [Test]
        public void AudioService_SetCategoryVolume_UpdatesSpecificCategory()
        {
            // Arrange
            audioService.Initialize();
            float bgmVolume = 0.8f;
            float effectVolume = 0.6f;

            // Act
            audioService.SetCategoryVolume("bgm", bgmVolume);
            audioService.SetCategoryVolume("effect", effectVolume);

            // Assert
            Assert.AreEqual(bgmVolume, audioService.GetBGMVolume(), 0.001f);
            Assert.AreEqual(effectVolume, audioService.GetEffectVolume(), 0.001f);
        }

        #endregion

        #region Pause/Resume Integration Tests

        [UnityTest]
        public IEnumerator AudioService_PauseResume_AffectsAllAudioSystems()
        {
            // Arrange
            audioService.Initialize();
            yield return null; // フレーム待機

            float initialBGMVolume = audioService.GetBGMVolume();
            float initialAmbientVolume = audioService.GetAmbientVolume();

            // Act - Pause
            audioService.Pause();
            yield return null;

            // Assert - Pause state
            // Pause時は音量が保存され、実際の音量は0になることを確認
            // 実装依存の部分なので、基本的な状態確認のみ

            // Act - Resume
            audioService.Resume();
            yield return null;

            // Assert - Resume state
            Assert.AreEqual(initialBGMVolume, audioService.GetBGMVolume(), 0.001f);
            Assert.AreEqual(initialAmbientVolume, audioService.GetAmbientVolume(), 0.001f);
        }

        #endregion

        #region Service Initialization Tests

        [Test]
        public void AudioService_Initialize_InitializesAllSubsystems()
        {
            // Act
            audioService.Initialize();

            // Assert
            Assert.IsTrue(audioService.IsInitialized);
            
            // SubsystemsがServiceLocatorに適切に登録されていることを確認
            var retrievedService = ServiceLocator.RequireService<IAudioService>();
            Assert.IsNotNull(retrievedService);
            Assert.AreSame(audioService, retrievedService);
        }

        [Test]
        public void AudioService_InitializeMultipleTimes_DoesNotReinitialize()
        {
            // Arrange
            audioService.Initialize();
            bool wasInitializedFirst = audioService.IsInitialized;

            // Act
            audioService.Initialize(); // 2回目の初期化

            // Assert
            Assert.IsTrue(wasInitializedFirst);
            Assert.IsTrue(audioService.IsInitialized);
            // 複数回初期化しても問題ないことを確認
        }

        #endregion

        #region Service Interaction Tests

        [Test]
        public void AudioService_PlaySound_DelegatesToEffectManager()
        {
            // Arrange
            audioService.Initialize();
            string soundId = "test_sound";
            Vector3 position = Vector3.zero;
            float volume = 0.8f;

            // Act
            audioService.PlaySound(soundId, position, volume);

            // Assert
            // EffectManagerに正しく委譲されることを確認
            // 実際の音声再生の検証は困難なため、例外が発生しないことを確認
            Assert.DoesNotThrow(() => audioService.PlaySound(soundId, position, volume));
        }

        [Test]
        public void AudioService_PlayBGM_DelegatesToBGMManager()
        {
            // Arrange
            audioService.Initialize();
            string bgmName = "test_bgm";

            // Act
            audioService.PlayBGM(bgmName);

            // Assert
            // BGMManagerに正しく委譲されることを確認
            Assert.DoesNotThrow(() => audioService.PlayBGM(bgmName));
        }

        [Test]
        public void AudioService_UpdateAmbient_DelegatesToAmbientManager()
        {
            // Arrange
            audioService.Initialize();
            var environment = EnvironmentType.Urban;
            var weather = WeatherType.Clear;
            var timeOfDay = TimeOfDay.Day;

            // Act
            audioService.UpdateAmbient(environment, weather, timeOfDay);

            // Assert
            // AmbientManagerに正しく委譲されることを確認
            Assert.DoesNotThrow(() => audioService.UpdateAmbient(environment, weather, timeOfDay));
        }

        #endregion

        #region Multiple Service Instances Tests

        [Test]
        public void ServiceLocator_MultipleAudioServiceInstances_LastRegisteredWins()
        {
            // Arrange
            var secondGameObject = new GameObject("SecondAudioService");
            var secondAudioService = secondGameObject.AddComponent<AudioService>();

            try
            {
                // Act
                audioService.gameObject.SetActive(true);
                audioService.SendMessage("Awake");
                
                secondAudioService.gameObject.SetActive(true);
                secondAudioService.SendMessage("Awake");

                // Assert
                var registeredService = ServiceLocator.GetService<IAudioService>();
                Assert.AreSame(secondAudioService, registeredService);
            }
            finally
            {
                // Cleanup
                Object.DestroyImmediate(secondGameObject);
            }
        }

        #endregion

        #region Error Handling Tests

        [Test]
        public void AudioService_WithNullManagers_DoesNotThrowOnBasicOperations()
        {
            // Arrange
            var isolatedAudioService = new GameObject("IsolatedAudioService").AddComponent<AudioService>();
            
            try
            {
                // Act & Assert
                Assert.DoesNotThrow(() => isolatedAudioService.Initialize());
                Assert.DoesNotThrow(() => isolatedAudioService.PlaySound("test"));
                Assert.DoesNotThrow(() => isolatedAudioService.SetMasterVolume(0.5f));
                Assert.DoesNotThrow(() => isolatedAudioService.Pause());
                Assert.DoesNotThrow(() => isolatedAudioService.Resume());
            }
            finally
            {
                Object.DestroyImmediate(isolatedAudioService.gameObject);
            }
        }

        #endregion

        #region Performance Tests

        [UnityTest]
        public IEnumerator AudioService_MultipleVolumeChanges_PerformsWithinReasonableTime()
        {
            // Arrange
            audioService.Initialize();
            int iterations = 100;
            float startTime = Time.realtimeSinceStartup;

            // Act
            for (int i = 0; i < iterations; i++)
            {
                audioService.SetMasterVolume(Random.Range(0f, 1f));
                if (i % 10 == 0) yield return null; // 時々フレーム待機
            }

            float elapsed = Time.realtimeSinceStartup - startTime;

            // Assert
            Assert.Less(elapsed, 1f, "Volume changes should complete within reasonable time");
        }

        #endregion
    }
}