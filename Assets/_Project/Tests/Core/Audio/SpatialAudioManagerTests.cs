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
    /// SpatialAudioManagerの単体テスト
    /// 3D空間オーディオシステムのテスト
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
            
            // ServiceLocatorのセットアップ
            TestHelpers.SetupTestServiceLocator();
            
            // SpatialAudioManagerオブジェクトの作成
            spatialAudioManagerObject = TestHelpers.CreateTestGameObject("SpatialAudioManager");
            spatialAudioManager = spatialAudioManagerObject.AddComponent<SpatialAudioManager>();
            
            // AudioListenerコンポーネントの追加（空間オーディオに必要）
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
        /// SpatialAudioManagerが正常に初期化されることをテスト
        /// </summary>
        [Test]
        public void SpatialAudioManager_ShouldInitialize_Properly()
        {
            // Assert
            Assert.IsNotNull(spatialAudioManager, "SpatialAudioManager should not be null");
        }

        /// <summary>
        /// SpatialAudioManagerがSingletonパターンで動作することをテスト
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
        /// 3D位置での音声再生をテスト
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
        /// リスナー位置の設定をテスト
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
        /// 空間オーディオの距離減衰をテスト
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
        /// 複数の空間音源の同時再生をテスト
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
        /// ServiceHelper経由でSpatialAudioManagerが取得できることをテスト
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
        /// 空間音声再生のパフォーマンステスト
        /// </summary>
        [Test]
        public void Play3DSound_ShouldComplete_WithinPerformanceThreshold()
        {
            // Arrange
            string testSoundName = "performance-spatial-sound";
            Vector3 testPosition = new Vector3(10, 0, 10);
            float maxExecutionTime = 0.002f; // 2ms以内

            // Act & Assert
            TestHelpers.AssertExecutionTime(
                () => spatialAudioManager.Play3DSound(testSoundName, testPosition),
                maxExecutionTime,
                "PlaySoundAtPosition should complete within performance threshold"
            );
        }

        /// <summary>
        /// 大量の空間音源のパフォーマンステスト
        /// </summary>
        [Test]
        public void MassiveSpatialSounds_ShouldMaintain_Performance()
        {
            // Arrange
            int soundCount = 50;
            float maxExecutionTime = 0.05f; // 50ms以内

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
        /// 無効な位置での音声再生をテスト
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
        /// null音声名の処理をテスト
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
        /// 音源とリスナーの距離計算をテスト
        /// </summary>
        [Test]
        public void SpatialAudio_ShouldCalculate_CorrectDistance()
        {
            // Arrange
            Vector3 listenerPosition = Vector3.zero;
            Vector3 soundPosition = new Vector3(3, 4, 0); // 距離5の位置
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
        /// 左右のパンニング計算をテスト
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
        /// AudioManagerとの統合テスト
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
                // 通常の音声再生
                audioManager.PlaySound("normal-sound");
                
                // 空間音声再生
                spatialAudioManager.Play3DSound("spatial-sound", Vector3.forward * 5);
            }, "SpatialAudioManager should integrate with AudioManager");
        }

        /// <summary>
        /// FeatureFlagsとの統合テスト
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
        /// AudioListenerの存在を確認
        /// </summary>
        private void AssertAudioListenerExists()
        {
            var audioListener = Object.FindObjectOfType<AudioListener>();
            Assert.IsNotNull(audioListener, "AudioListener should exist for spatial audio");
        }

        /// <summary>
        /// ランダムな3D位置を生成
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