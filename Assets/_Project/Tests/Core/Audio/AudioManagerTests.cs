using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using asterivo.Unity60.Tests.Helpers;
using asterivo.Unity60.Core.Audio;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Audio.Interfaces;

namespace asterivo.Unity60.Tests.Core.Audio
{
    /// <summary>
    /// AudioManager縺ｮ蜊倅ｽ薙ユ繧ｹ繝・
    /// Week 2 繝・せ繝医う繝ｳ繝輔Λ讒狗ｯ峨・荳迺ｰ縺ｨ縺励※菴懈・
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
            
            // ServiceLocator縺ｮ繧ｻ繝・ヨ繧｢繝・・
            TestHelpers.SetupTestServiceLocator();
            
            // AudioManager繧ｪ繝悶ず繧ｧ繧ｯ繝医・菴懈・
            audioManagerObject = TestHelpers.CreateTestGameObject("AudioManager");
            audioManager = audioManagerObject.AddComponent<AudioManager>();
            
            // AudioSource繧ｳ繝ｳ繝昴・繝阪Φ繝医・霑ｽ蜉
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
        /// AudioManager縺梧ｭ｣蟶ｸ縺ｫ蛻晄悄蛹悶＆繧後ｋ縺薙→繧偵ユ繧ｹ繝・
        /// </summary>
        [Test]
        public void AudioManager_ShouldInitialize_Properly()
        {
            // Assert
            Assert.IsNotNull(audioManager, "AudioManager should not be null");
            TestHelpers.AssertHasComponent<AudioSource>(audioManagerObject);;
        }

        /// <summary>
        /// AudioManager縺郡ingleton繝代ち繝ｼ繝ｳ縺ｧ蜍穂ｽ懊☆繧九％縺ｨ繧偵ユ繧ｹ繝・
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
        /// ServiceLocator邨檎罰縺ｧAudioManager縺悟叙蠕励〒縺阪ｋ縺薙→繧偵ユ繧ｹ繝・
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
        /// 髻ｳ螢ｰ蜀咲函縺ｮ蝓ｺ譛ｬ讖溯・繧偵ユ繧ｹ繝・
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
        /// 髻ｳ螢ｰ蛛懈ｭ｢讖溯・繧偵ユ繧ｹ繝・
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
        /// 髻ｳ驥剰ｨｭ螳壽ｩ溯・繧偵ユ繧ｹ繝・
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
        /// 髻ｳ驥上・蠅・阜蛟､繝・せ繝・
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
        /// 辟｡蜉ｹ縺ｪ髻ｳ驥丞､縺ｮ蜃ｦ逅・ｒ繝・せ繝・
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
        /// ServiceHelper縺ｨ縺ｮ邨ｱ蜷医ｒ繝・せ繝・
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
        /// ServiceLocator辟｡蜉ｹ譎ゅ・繝輔か繝ｼ繝ｫ繝舌ャ繧ｯ蜍穂ｽ懊ｒ繝・せ繝・
        /// </summary>
        [Test]
        public void ServiceHelper_ShouldFallback_WhenServiceLocatorDisabled()
        {
            // Arrange
            FeatureFlags.UseServiceLocator = false;
            
            // Act
            var retrievedManager = asterivo.Unity60.Core.Helpers.ServiceHelper.GetServiceWithFallback<AudioManager>();
            
            // Assert
            // FindFirstObjectByType縺ｧAudioManager縺瑚ｦ九▽縺九ｋ縺薙→繧堤｢ｺ隱・
            Assert.IsNotNull(retrievedManager, "ServiceHelper should fallback to FindFirstObjectByType");
        }

        #endregion

        #region Performance Tests

        /// <summary>
        /// 髻ｳ螢ｰ蜀咲函縺ｮ繝代ヵ繧ｩ繝ｼ繝槭Φ繧ｹ繝・せ繝・
        /// </summary>
        [Test]
        public void PlaySound_ShouldComplete_WithinPerformanceThreshold()
        {
            // Arrange
            string testSoundName = "performance-test-sound";
            float maxExecutionTime = 0.001f; // 1ms莉･蜀・

            // Act & Assert
            TestHelpers.AssertExecutionTime(
                () => audioManager.PlaySound(testSoundName),
                maxExecutionTime,
                "PlaySound should complete within performance threshold"
            );
        }

        /// <summary>
        /// 隍・焚髻ｳ螢ｰ縺ｮ蜷梧凾蜀咲函繝代ヵ繧ｩ繝ｼ繝槭Φ繧ｹ繝・せ繝・
        /// </summary>
        [Test]
        public void MultipleSounds_ShouldPlay_WithinPerformanceThreshold()
        {
            // Arrange
            string[] soundNames = { "sound1", "sound2", "sound3", "sound4", "sound5" };
            float maxExecutionTime = 0.005f; // 5ms莉･蜀・

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
        /// null蠑墓焚縺ｮ蜃ｦ逅・ｒ繝・せ繝・
        /// </summary>
        [Test]
        public void PlaySound_ShouldHandle_NullInput()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => audioManager.PlaySound(null),
                "PlaySound should handle null input gracefully");
        }

        /// <summary>
        /// 遨ｺ譁・ｭ怜・蠑墓焚縺ｮ蜃ｦ逅・ｒ繝・せ繝・
        /// </summary>
        [Test]
        public void PlaySound_ShouldHandle_EmptyString()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => audioManager.PlaySound(""),
                "PlaySound should handle empty string gracefully");
        }

        /// <summary>
        /// 蟄伜惠縺励↑縺・浹螢ｰ繝輔ぃ繧､繝ｫ縺ｮ蜃ｦ逅・ｒ繝・せ繝・
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
        /// AudioManager縺ｨFeatureFlags縺ｮ邨ｱ蜷医ユ繧ｹ繝・
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
        /// AudioManager縺ｮ蝓ｺ譛ｬ險ｭ螳壹ｒ讀懆ｨｼ
        /// </summary>
        private void AssertAudioManagerBasicSetup()
        {
            Assert.IsNotNull(audioManager, "AudioManager should be initialized");
            Assert.IsNotNull(audioManagerObject.GetComponent<AudioSource>(), "AudioSource should be present");
        }

        /// <summary>
        /// 繝・せ繝育畑AudioClip繧剃ｽ懈・
        /// </summary>
        private AudioClip CreateTestAudioClip()
        {
            // 繝・せ繝育畑縺ｮ遨ｺ縺ｮAudioClip繧剃ｽ懈・
            return AudioClip.Create("TestClip", 44100, 1, 44100, false);
        }

        #endregion
    }
}


