using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using asterivo.Unity60.Tests.Helpers;
using asterivo.Unity60.Core.Audio;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Core.Audio.Interfaces;

namespace asterivo.Unity60.Tests.Core.Audio
{
    /// <summary>
    /// AudioManagerの単体テスト
    /// Week 2 テストインフラ構築の一環として作成
    /// </summary>
    [TestFixture]
    public class AudioManagerTests
    {
        #region Setup & Teardown

        private GameObject audioManagerObject;
        private AudioManager audioManager;

        [SetUp]
        public void SetUp()
        {
            TestHelpers.ResetFeatureFlagsForTest();
            
            // ServiceLocatorのセットアップ
            TestHelpers.SetupTestServiceLocator();
            
            // AudioManagerオブジェクトの作成
            audioManagerObject = TestHelpers.CreateTestGameObject("AudioManager");
            audioManager = audioManagerObject.AddComponent<AudioManager>();
            
            // AudioSourceコンポーネントの追加
            audioManagerObject.AddComponent<AudioSource>();
        }

        [TearDown]
        public void TearDown()
        {
            TestHelpers.CleanupTestGameObjects();
            TestHelpers.CleanupServiceLocator();;
        }

        #endregion

        #region Basic Functionality Tests

        /// <summary>
        /// AudioManagerが正常に初期化されることをテスト
        /// </summary>
        [Test]
        public void AudioManager_ShouldInitialize_Properly()
        {
            // Assert
            Assert.IsNotNull(audioManager, "AudioManager should not be null");
            TestHelpers.AssertHasComponent<AudioSource>(audioManagerObject);;
        }

        /// <summary>
        /// AudioManagerがSingletonパターンで動作することをテスト
        /// </summary>
        [Test]
        public void AudioManager_ShouldImplement_SingletonPattern()
        {
            // Arrange & Act
            var instance1 = ServiceLocator.GetService<IAudioService>();
            var instance2 = ServiceLocator.GetService<IAudioService>();
            
            // Assert
            Assert.IsNotNull(instance1, "AudioManager instance should not be null");
            Assert.AreSame(instance1, instance2, "AudioManager should return same instance");
        }

        /// <summary>
        /// ServiceLocator経由でAudioManagerが取得できることをテスト
        /// </summary>
        [Test]
        public void AudioManager_ShouldBeAccessible_ViaServiceLocator()
        {
            // Arrange
            ServiceLocator.RegisterService<AudioManager>(audioManager);
            
            // Act
            var retrievedManager = ServiceLocator.GetService<AudioManager>();
            
            // Assert
            Assert.IsNotNull(retrievedManager, "AudioManager should be retrievable via ServiceLocator");
            Assert.AreSame(audioManager, retrievedManager, "Retrieved AudioManager should be the same instance");
        }

        #endregion

        #region Audio Playback Tests

        /// <summary>
        /// 音声再生の基本機能をテスト
        /// </summary>
        [Test]
        public void PlaySound_ShouldExecute_WithoutErrors()
        {
            // Arrange
            string testSoundName = "test-sound";
            
            // Act & Assert
            Assert.DoesNotThrow(() => audioManager.PlaySound(testSoundName),
                "PlaySound should not throw exceptions");
        }

        /// <summary>
        /// 音声停止機能をテスト
        /// </summary>
        [Test]
        public void StopSound_ShouldExecute_WithoutErrors()
        {
            // Arrange
            string testSoundName = "test-sound";
            
            // Act & Assert
            Assert.DoesNotThrow(() => audioManager.StopSound(testSoundName),
                "StopSound should not throw exceptions");
        }

        /// <summary>
        /// 音量設定機能をテスト
        /// </summary>
        [Test]
        public void SetMasterVolume_ShouldUpdate_AudioSourceVolume()
        {
            // Arrange
            float testVolume = 0.7f;
            var audioSource = audioManagerObject.GetComponent<AudioSource>();
            
            // Act
            audioManager.SetMasterVolume(testVolume);
            
            // Assert
            Assert.AreEqual(testVolume, audioSource.volume, 0.01f,
                "AudioSource volume should match the set volume");
        }

        /// <summary>
        /// 音量の境界値テスト
        /// </summary>
        [TestCase(0.0f)]
        [TestCase(1.0f)]
        [TestCase(0.5f)]
        public void SetVolume_ShouldHandle_BoundaryValues(float volume)
        {
            // Act & Assert
            Assert.DoesNotThrow(() => audioManager.SetMasterVolume(volume),
                $"SetVolume should handle boundary value: {volume}");
            
            var audioSource = audioManagerObject.GetComponent<AudioSource>();
            Assert.AreEqual(volume, audioSource.volume, 0.01f,
                $"AudioSource volume should be {volume}");
        }

        /// <summary>
        /// 無効な音量値の処理をテスト
        /// </summary>
        [TestCase(-0.1f)]
        [TestCase(1.1f)]
        public void SetVolume_ShouldClamp_InvalidValues(float invalidVolume)
        {
            // Act
            audioManager.SetMasterVolume(invalidVolume);
            
            // Assert
            var audioSource = audioManagerObject.GetComponent<AudioSource>();
            var clampedVolume = Mathf.Clamp01(invalidVolume);
            Assert.AreEqual(clampedVolume, audioSource.volume, 0.01f,
                $"AudioSource volume should be clamped to valid range");
        }

        #endregion

        #region ServiceHelper Integration Tests

        /// <summary>
        /// ServiceHelperとの統合をテスト
        /// </summary>
        [Test]
        public void ServiceHelper_ShouldRetrieve_AudioManager()
        {
            // Arrange
            ServiceLocator.RegisterService<AudioManager>(audioManager);
            
            // Act
            var retrievedManager = asterivo.Unity60.Core.Helpers.ServiceHelper.GetServiceWithFallback<AudioManager>();
            
            // Assert
            Assert.IsNotNull(retrievedManager, "ServiceHelper should retrieve AudioManager");
        }

        /// <summary>
        /// ServiceLocator無効時のフォールバック動作をテスト
        /// </summary>
        [Test]
        public void ServiceHelper_ShouldFallback_WhenServiceLocatorDisabled()
        {
            // Arrange
            FeatureFlags.UseServiceLocator = false;
            
            // Act
            var retrievedManager = asterivo.Unity60.Core.Helpers.ServiceHelper.GetServiceWithFallback<AudioManager>();
            
            // Assert
            // FindFirstObjectByTypeでAudioManagerが見つかることを確認
            Assert.IsNotNull(retrievedManager, "ServiceHelper should fallback to FindFirstObjectByType");
        }

        #endregion

        #region Performance Tests

        /// <summary>
        /// 音声再生のパフォーマンステスト
        /// </summary>
        [Test]
        public void PlaySound_ShouldComplete_WithinPerformanceThreshold()
        {
            // Arrange
            string testSoundName = "performance-test-sound";
            float maxExecutionTime = 0.001f; // 1ms以内

            // Act & Assert
            TestHelpers.AssertExecutionTime(
                () => audioManager.PlaySound(testSoundName),
                maxExecutionTime,
                "PlaySound should complete within performance threshold"
            );
        }

        /// <summary>
        /// 複数音声の同時再生パフォーマンステスト
        /// </summary>
        [Test]
        public void MultipleSounds_ShouldPlay_WithinPerformanceThreshold()
        {
            // Arrange
            string[] soundNames = { "sound1", "sound2", "sound3", "sound4", "sound5" };
            float maxExecutionTime = 0.005f; // 5ms以内

            // Act & Assert
            TestHelpers.AssertExecutionTime(
                () =>
                {
                    foreach (var soundName in soundNames)
                    {
                        audioManager.PlaySound(soundName);
                    }
                },
                maxExecutionTime,
                "Multiple sounds should play within performance threshold"
            );
        }

        #endregion

        #region Error Handling Tests

        /// <summary>
        /// null引数の処理をテスト
        /// </summary>
        [Test]
        public void PlaySound_ShouldHandle_NullInput()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => audioManager.PlaySound(null),
                "PlaySound should handle null input gracefully");
        }

        /// <summary>
        /// 空文字列引数の処理をテスト
        /// </summary>
        [Test]
        public void PlaySound_ShouldHandle_EmptyString()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => audioManager.PlaySound(""),
                "PlaySound should handle empty string gracefully");
        }

        /// <summary>
        /// 存在しない音声ファイルの処理をテスト
        /// </summary>
        [Test]
        public void PlaySound_ShouldHandle_NonexistentSound()
        {
            // Arrange
            string nonexistentSound = "this-sound-does-not-exist";
            
            // Act & Assert
            Assert.DoesNotThrow(() => audioManager.PlaySound(nonexistentSound),
                "PlaySound should handle nonexistent sound gracefully");
        }

        #endregion

        #region Integration Tests

        /// <summary>
        /// AudioManagerとFeatureFlagsの統合テスト
        /// </summary>
        [Test]
        public void AudioManager_ShouldRespect_FeatureFlags()
        {
            // Arrange
            FeatureFlags.UseNewAudioSystem = true;
            
            // Act & Assert
            Assert.DoesNotThrow(() => audioManager.PlaySound("test-sound"),
                "AudioManager should work with new audio system enabled");
            
            // Test with feature flag disabled
            FeatureFlags.UseNewAudioSystem = false;
            Assert.DoesNotThrow(() => audioManager.PlaySound("test-sound"),
                "AudioManager should work with new audio system disabled");
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// AudioManagerの基本設定を検証
        /// </summary>
        private void AssertAudioManagerBasicSetup()
        {
            Assert.IsNotNull(audioManager, "AudioManager should be initialized");
            Assert.IsNotNull(audioManagerObject.GetComponent<AudioSource>(), "AudioSource should be present");
        }

        /// <summary>
        /// テスト用AudioClipを作成
        /// </summary>
        private AudioClip CreateTestAudioClip()
        {
            // テスト用の空のAudioClipを作成
            return AudioClip.Create("TestClip", 44100, 1, 44100, false);
        }

        #endregion
    }
}