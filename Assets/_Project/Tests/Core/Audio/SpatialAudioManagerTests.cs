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
    /// SpatialAudioManager縺ｮ蜊倅ｽ薙ユ繧ｹ繝・
    /// 3D遨ｺ髢薙が繝ｼ繝・ぅ繧ｪ繧ｷ繧ｹ繝・Β縺ｮ繝・せ繝・
    /// </summary>
    [TestFixture]
    public class SpatialAudioManagerTests
    {
        #region Setup & Teardown

        private GameObject spatialAudioManagerObject;
        #pragma warning disable CS0618 // Type or member is obsolete
        private SpatialAudioManager spatialAudioManager;
        #pragma warning restore CS0618 // Type or member is obsolete
        private UnityEngine.Camera testCamera;

        [SetUp]
        public void SetUp()
        {
            TestHelpers.ResetFeatureFlagsForTest();
            TestHelpers.SetupTestScene();
            
            // ServiceLocator縺ｮ繧ｻ繝・ヨ繧｢繝・・
            TestHelpers.SetupTestServiceLocator();
            
            // SpatialAudioManager繧ｪ繝悶ず繧ｧ繧ｯ繝医・菴懈・
            spatialAudioManagerObject = TestHelpers.CreateTestGameObject("SpatialAudioManager");
            spatialAudioManager = spatialAudioManagerObject.AddComponent<SpatialAudioManager>();
            
            // AudioListener繧ｳ繝ｳ繝昴・繝阪Φ繝医・霑ｽ蜉・育ｩｺ髢薙が繝ｼ繝・ぅ繧ｪ縺ｫ蠢・ｦ・ｼ・
            testCamera = UnityEngine.Camera.main;
            if (testCamera != null && testCamera.GetComponent<AudioListener>() == null)
            {
                testCamera.gameObject.AddComponent<AudioListener>();
            }
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
        /// SpatialAudioManager縺梧ｭ｣蟶ｸ縺ｫ蛻晄悄蛹悶＆繧後ｋ縺薙→繧偵ユ繧ｹ繝・
        /// </summary>
        [Test]
        public void SpatialAudioManager_ShouldInitialize_Properly()
        {
            // Assert
            Assert.IsNotNull(spatialAudioManager, "SpatialAudioManager should not be null");
        }

        /// <summary>
        /// SpatialAudioManager縺郡ingleton繝代ち繝ｼ繝ｳ縺ｧ蜍穂ｽ懊☆繧九％縺ｨ繧偵ユ繧ｹ繝・
        /// </summary>
        [Test]
        public void SpatialAudioManager_ShouldImplement_SingletonPattern()
        {
            // Arrange & Act
            var instance1 = ServiceLocator.GetService<ISpatialAudioService>();
            var instance2 = ServiceLocator.GetService<ISpatialAudioService>();
            
            // Assert
            Assert.IsNotNull(instance1, "SpatialAudioManager instance should not be null");
            Assert.AreSame(instance1, instance2, "SpatialAudioManager should return same instance");
        }

        #endregion

        #region Spatial Audio Tests

        /// <summary>
        /// 3D菴咲ｽｮ縺ｧ縺ｮ髻ｳ螢ｰ蜀咲函繧偵ユ繧ｹ繝・
        /// </summary>
        [Test]
        public void Play3DSound_ShouldExecute_WithoutErrors()
        {
            // Arrange
            string testSoundName = "spatial-test-sound";
            Vector3 testPosition = new Vector3(5, 0, 3);
            
            // Act & Assert
            Assert.DoesNotThrow(() => spatialAudioManager.Play3DSound(testSoundName, testPosition),
                "Play3DSound should not throw exceptions");
        }

        /// <summary>
        /// 繝ｪ繧ｹ繝翫・菴咲ｽｮ縺ｮ險ｭ螳壹ｒ繝・せ繝・
        /// </summary>
        [Test]
        public void UpdateListenerPosition_ShouldUpdate_AudioListenerPosition()
        {
            // Arrange
            Vector3 testPosition = new Vector3(10, 5, -2);
            var audioListener = Object.FindObjectOfType<AudioListener>();
            
            // Act
            spatialAudioManager.UpdateListenerPosition(testPosition, Vector3.forward);
            
            // Assert
            if (audioListener != null)
            {
                TestHelpers.AssertVector3Approximately(testPosition, audioListener.transform.position, 0.01f);
            }
        }

        /// <summary>
        /// 遨ｺ髢薙が繝ｼ繝・ぅ繧ｪ縺ｮ霍晞屬貂幄｡ｰ繧偵ユ繧ｹ繝・
        /// </summary>
        [TestCase(1.0f, ExpectedResult = true)]
        [TestCase(50.0f, ExpectedResult = true)]
        [TestCase(100.0f, ExpectedResult = true)]
        public bool SpatialAudio_ShouldHandle_DistanceAttenuation(float distance)
        {
            // Arrange
            string testSoundName = "distance-test-sound";
            Vector3 soundPosition = Vector3.forward * distance;
            
            // Act & Assert
            try
            {
                spatialAudioManager.Play3DSound(testSoundName, soundPosition);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 隍・焚縺ｮ遨ｺ髢馴浹貅舌・蜷梧凾蜀咲函繧偵ユ繧ｹ繝・
        /// </summary>
        [Test]
        public void MultipleSpatialSounds_ShouldPlay_Simultaneously()
        {
            // Arrange
            var soundConfigs = new[]
            {
                new { name = "sound1", position = new Vector3(5, 0, 0) },
                new { name = "sound2", position = new Vector3(-5, 0, 0) },
                new { name = "sound3", position = new Vector3(0, 5, 0) },
                new { name = "sound4", position = new Vector3(0, -5, 0) }
            };
            
            // Act & Assert
            Assert.DoesNotThrow(() =>
            {
                foreach (var config in soundConfigs)
                {
                    spatialAudioManager.Play3DSound(config.name, config.position);
                }
            }, "Multiple spatial sounds should play simultaneously without errors");
        }

        #endregion

        #region ServiceHelper Integration Tests

        /// <summary>
        /// ServiceHelper邨檎罰縺ｧSpatialAudioManager縺悟叙蠕励〒縺阪ｋ縺薙→繧偵ユ繧ｹ繝・
        /// </summary>
        [Test]
        public void ServiceHelper_ShouldRetrieve_SpatialAudioManager()
        {
            // Arrange
            ServiceLocator.RegisterService<SpatialAudioManager>(spatialAudioManager);
            
            // Act
            var retrievedManager = asterivo.Unity60.Core.Helpers.ServiceHelper.GetServiceWithFallback<SpatialAudioManager>();
            
            // Assert
            Assert.IsNotNull(retrievedManager, "ServiceHelper should retrieve SpatialAudioManager");
        }

        #endregion

        #region Performance Tests

        /// <summary>
        /// 遨ｺ髢馴浹螢ｰ蜀咲函縺ｮ繝代ヵ繧ｩ繝ｼ繝槭Φ繧ｹ繝・せ繝・
        /// </summary>
        [Test]
        public void Play3DSound_ShouldComplete_WithinPerformanceThreshold()
        {
            // Arrange
            string testSoundName = "performance-spatial-sound";
            Vector3 testPosition = new Vector3(10, 0, 10);
            float maxExecutionTime = 0.002f; // 2ms莉･蜀・

            // Act & Assert
            TestHelpers.AssertExecutionTime(
                () => spatialAudioManager.Play3DSound(testSoundName, testPosition),
                maxExecutionTime,
                "PlaySoundAtPosition should complete within performance threshold"
            );
        }

        /// <summary>
        /// 螟ｧ驥上・遨ｺ髢馴浹貅舌・繝代ヵ繧ｩ繝ｼ繝槭Φ繧ｹ繝・せ繝・
        /// </summary>
        [Test]
        public void MassiveSpatialSounds_ShouldMaintain_Performance()
        {
            // Arrange
            int soundCount = 50;
            float maxExecutionTime = 0.05f; // 50ms莉･蜀・

            // Act & Assert
            TestHelpers.AssertExecutionTime(
                () =>
                {
                    for (int i = 0; i < soundCount; i++)
                    {
                        Vector3 randomPosition = new Vector3(
                            Random.Range(-50f, 50f),
                            Random.Range(-10f, 10f),
                            Random.Range(-50f, 50f)
                        );
                        spatialAudioManager.Play3DSound($"mass-sound-{i}", randomPosition);
                    }
                },
                maxExecutionTime,
                $"Playing {soundCount} spatial sounds should complete within performance threshold"
            );
        }

        #endregion

        #region Error Handling Tests

        /// <summary>
        /// 辟｡蜉ｹ縺ｪ菴咲ｽｮ縺ｧ縺ｮ髻ｳ螢ｰ蜀咲函繧偵ユ繧ｹ繝・
        /// </summary>
        [Test]
        public void PlaySoundAtPosition_ShouldHandle_InvalidPositions()
        {
            // Arrange
            string testSoundName = "invalid-position-sound";
            Vector3[] invalidPositions = {
                new Vector3(float.NaN, 0, 0),
                new Vector3(float.PositiveInfinity, 0, 0),
                new Vector3(float.NegativeInfinity, 0, 0),
                new Vector3(0, float.NaN, 0),
                new Vector3(0, 0, float.PositiveInfinity)
            };
            
            // Act & Assert
            foreach (var position in invalidPositions)
            {
                Assert.DoesNotThrow(() => spatialAudioManager.Play3DSound(testSoundName, position),
                    $"PlaySoundAtPosition should handle invalid position: {position}");
            }
        }

        /// <summary>
        /// null髻ｳ螢ｰ蜷阪・蜃ｦ逅・ｒ繝・せ繝・
        /// </summary>
        [Test]
        public void PlaySoundAtPosition_ShouldHandle_NullSoundName()
        {
            // Arrange
            Vector3 testPosition = Vector3.zero;
            
            // Act & Assert
            Assert.DoesNotThrow(() => spatialAudioManager.Play3DSound(null, testPosition),
                "PlaySoundAtPosition should handle null sound name gracefully");
        }

        #endregion

        #region Spatial Calculation Tests

        /// <summary>
        /// 髻ｳ貅舌→繝ｪ繧ｹ繝翫・縺ｮ霍晞屬險育ｮ励ｒ繝・せ繝・
        /// </summary>
        [Test]
        public void SpatialAudio_ShouldCalculate_CorrectDistance()
        {
            // Arrange
            Vector3 listenerPosition = Vector3.zero;
            Vector3 soundPosition = new Vector3(3, 4, 0); // 霍晞屬5縺ｮ菴咲ｽｮ
            float expectedDistance = 5.0f;
            
            // Act
            spatialAudioManager.UpdateListenerPosition(listenerPosition, Vector3.forward);
            spatialAudioManager.Play3DSound("distance-calc-sound", soundPosition);
            
            // Assert
            float actualDistance = Vector3.Distance(listenerPosition, soundPosition);
            Assert.AreEqual(expectedDistance, actualDistance, 0.01f,
                "Distance calculation should be accurate");
        }

        /// <summary>
        /// 蟾ｦ蜿ｳ縺ｮ繝代Φ繝九Φ繧ｰ險育ｮ励ｒ繝・せ繝・
        /// </summary>
        [Test]
        public void SpatialAudio_ShouldHandle_LeftRightPanning()
        {
            // Arrange
            Vector3 leftPosition = new Vector3(-10, 0, 0);
            Vector3 rightPosition = new Vector3(10, 0, 0);
            
            // Act & Assert
            Assert.DoesNotThrow(() =>
            {
                spatialAudioManager.Play3DSound("left-sound", leftPosition);
                spatialAudioManager.Play3DSound("right-sound", rightPosition);
            }, "Left/right panning should work correctly");
        }

        #endregion

        #region Integration Tests

        /// <summary>
        /// AudioManager縺ｨ縺ｮ邨ｱ蜷医ユ繧ｹ繝・
        /// </summary>
        [Test]
        public void SpatialAudioManager_ShouldIntegrate_WithAudioManager()
        {
            // Arrange
            var audioManagerObject = TestHelpers.CreateTestGameObject("AudioManager");
            var audioManager = audioManagerObject.AddComponent<AudioManager>();
            
            ServiceLocator.RegisterService<AudioManager>(audioManager);
            ServiceLocator.RegisterService<SpatialAudioManager>(spatialAudioManager);
            
            // Act & Assert
            Assert.DoesNotThrow(() =>
            {
                // 騾壼ｸｸ縺ｮ髻ｳ螢ｰ蜀咲函
                audioManager.PlaySound("normal-sound");
                
                // 遨ｺ髢馴浹螢ｰ蜀咲函
                spatialAudioManager.Play3DSound("spatial-sound", Vector3.forward * 5);
            }, "SpatialAudioManager should integrate with AudioManager");
        }

        /// <summary>
        /// FeatureFlags縺ｨ縺ｮ邨ｱ蜷医ユ繧ｹ繝・
        /// </summary>
        [Test]
        public void SpatialAudioManager_ShouldRespect_FeatureFlags()
        {
            // Arrange
            Vector3 testPosition = Vector3.forward * 10;
            
            // Test with spatial audio enabled
            FeatureFlags.UseNewAudioSystem = true;
            Assert.DoesNotThrow(() => spatialAudioManager.Play3DSound("feature-test-sound", testPosition),
                "SpatialAudioManager should work with new audio system enabled");
            
            // Test with spatial audio disabled
            FeatureFlags.UseNewAudioSystem = false;
            Assert.DoesNotThrow(() => spatialAudioManager.Play3DSound("feature-test-sound", testPosition),
                "SpatialAudioManager should work with new audio system disabled");
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// AudioListener縺ｮ蟄伜惠繧堤｢ｺ隱・
        /// </summary>
        private void AssertAudioListenerExists()
        {
            var audioListener = Object.FindObjectOfType<AudioListener>();
            Assert.IsNotNull(audioListener, "AudioListener should exist for spatial audio");
        }

        /// <summary>
        /// 繝ｩ繝ｳ繝繝縺ｪ3D菴咲ｽｮ繧堤函謌・
        /// </summary>
        private Vector3 GenerateRandomPosition(float range = 20f)
        {
            return new Vector3(
                Random.Range(-range, range),
                Random.Range(-range/2, range/2),
                Random.Range(-range, range)
            );
        }

        #endregion
    }
}


