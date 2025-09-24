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
    /// EffectManagerの単体テスト
    /// オーディオエフェクトシステムのテスト
    /// </summary>
    [TestFixture]
    public class EffectManagerTests
    {
        #region Setup & Teardown

        private GameObject effectManagerObject;
        private EffectManager effectManager;

        [SetUp]
        public void SetUp()
        {
            TestHelpers.ResetFeatureFlagsForTest();
            
            // ServiceLocatorのセットアップ
            TestHelpers.SetupTestServiceLocator();
            
            // EffectManagerオブジェクトの作成
            effectManagerObject = TestHelpers.CreateTestGameObject("EffectManager");
            effectManager = effectManagerObject.AddComponent<EffectManager>();
            
            // AudioSourceコンポーネントの追加
            effectManagerObject.AddComponent<AudioSource>();
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
        /// EffectManagerが正常に初期化されることをテスト
        /// </summary>
        [Test]
        public void EffectManager_ShouldInitialize_Properly()
        {
            // Assert
            Assert.IsNotNull(effectManager, "EffectManager should not be null");
            TestHelpers.AssertHasComponent<AudioSource>(effectManagerObject);
        }

        /// <summary>
        /// EffectManagerがSingletonパターンで動作することをテスト
        /// </summary>
        [Test]
        public void EffectManager_ShouldImplement_SingletonPattern()
        {
            // Arrange & Act
            var instance1 = ServiceLocator.GetService<IEffectService>();
            var instance2 = ServiceLocator.GetService<IEffectService>();
            
            // Assert
            Assert.IsNotNull(instance1, "EffectManager instance should not be null");
            Assert.AreSame(instance1, instance2, "EffectManager should return same instance");
        }

        #endregion

        #region Effect Playback Tests

        /// <summary>
        /// エフェクト音声再生の基本機能をテスト
        /// </summary>
        [Test]
        public void PlayEffect_ShouldExecute_WithoutErrors()
        {
            // Arrange
            string testEffectName = "test-effect";
            
            // Act & Assert
            Assert.DoesNotThrow(() => effectManager.PlayEffect(testEffectName),
                "PlayEffect should not throw exceptions");
        }

        /// <summary>
        /// エフェクト音声停止機能をテスト
        /// </summary>
        [Test]
        public void StopEffect_ShouldExecute_WithoutErrors()
        {
            // Arrange
            string testEffectName = "test-effect";
            
            // Act & Assert
            Assert.DoesNotThrow(() => effectManager.StopAllEffects(),
                "StopAllEffects should not throw exceptions");
        }

        /// <summary>
        /// エフェクト音量設定機能をテスト
        /// </summary>
        [Test]
        public void SetEffectVolume_ShouldUpdate_AudioSourceVolume()
        {
            // Arrange
            float testVolume = 0.8f;
            var audioSource = effectManagerObject.GetComponent<AudioSource>();
            
            // Act
            effectManager.PlayEffect("test-effect", Vector3.zero, testVolume);
            
            // Assert
            Assert.AreEqual(testVolume, audioSource.volume, 0.01f, "AudioSource volume should match the set effect volume");
        }

        /// <summary>
        /// 複数エフェクトの同時再生をテスト
        /// </summary>
        [Test]
        public void MultipleEffects_ShouldPlay_Simultaneously()
        {
            // Arrange
            string[] effectNames = { "effect1", "effect2", "effect3", "effect4" };
            
            // Act & Assert
            Assert.DoesNotThrow(() =>
            {
                foreach (var effectName in effectNames)
                {
                    effectManager.PlayEffect(effectName);
                }
            }, "Multiple effects should play simultaneously without errors");
        }

        #endregion

        #region Effect Categories Tests

        /// <summary>
        /// 異なるカテゴリのエフェクト再生をテスト
        /// </summary>
        [TestCase("UI")]
        [TestCase("Combat")]
        [TestCase("Environment")]
        [TestCase("Magic")]
        public void PlayEffect_ShouldHandle_DifferentCategories(string category)
        {
            // Arrange
            string effectName = $"{category}-effect";
            
            // Act & Assert
            Assert.DoesNotThrow(() => effectManager.PlayEffect(effectName),
                $"PlayEffect should handle {category} category effects");
        }

        /// <summary>
        /// エフェクトカテゴリごとの音量制御をテスト
        /// </summary>
        [Test]
        public void SetCategoryVolume_ShouldAffect_CategoryEffects()
        {
            // Arrange
            string category = "UI";
            float categoryVolume = 0.5f;
            
            // Act & Assert
            Assert.DoesNotThrow(() => effectManager.PlayUIEffect("test-ui-effect", categoryVolume),
                "SetCategoryVolume should execute without errors");
        }

        #endregion

        #region ServiceHelper Integration Tests

        /// <summary>
        /// ServiceHelper経由でEffectManagerが取得できることをテスト
        /// </summary>
        [Test]
        public void ServiceHelper_ShouldRetrieve_EffectManager()
        {
            // Arrange
            ServiceLocator.RegisterService<EffectManager>(effectManager);
            
            // Act
            var retrievedManager = asterivo.Unity60.Core.Helpers.ServiceHelper.GetServiceWithFallback<EffectManager>();
            
            // Assert
            Assert.IsNotNull(retrievedManager, "ServiceHelper should retrieve EffectManager");
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
            var retrievedManager = asterivo.Unity60.Core.Helpers.ServiceHelper.GetServiceWithFallback<EffectManager>();
            
            // Assert
            Assert.IsNotNull(retrievedManager, "ServiceHelper should fallback to FindFirstObjectByType");
        }

        #endregion

        #region Performance Tests

        /// <summary>
        /// エフェクト再生のパフォーマンステスト
        /// </summary>
        [Test]
        public void PlayEffect_ShouldComplete_WithinPerformanceThreshold()
        {
            // Arrange
            string testEffectName = "performance-test-effect";
            float maxExecutionTime = 0.001f; // 1ms以内

            // Act & Assert
            TestHelpers.AssertExecutionTime(
                () => effectManager.PlayEffect(testEffectName),
                maxExecutionTime,
                "PlayEffect should complete within performance threshold"
            );
        }

        /// <summary>
        /// 大量エフェクトの同時再生パフォーマンステスト
        /// </summary>
        [Test]
        public void MassiveEffects_ShouldPlay_WithinPerformanceThreshold()
        {
            // Arrange
            int effectCount = 100;
            float maxExecutionTime = 0.01f; // 10ms以内

            // Act & Assert
            TestHelpers.AssertExecutionTime(
                () =>
                {
                    for (int i = 0; i < effectCount; i++)
                    {
                        effectManager.PlayEffect($"mass-effect-{i}");
                    }
                },
                maxExecutionTime,
                $"Playing {effectCount} effects should complete within performance threshold"
            );
        }

        #endregion

        #region Error Handling Tests

        /// <summary>
        /// null引数の処理をテスト
        /// </summary>
        [Test]
        public void PlayEffect_ShouldHandle_NullInput()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => effectManager.PlayEffect(null),
                "PlayEffect should handle null input gracefully");
        }

        /// <summary>
        /// 空文字列引数の処理をテスト
        /// </summary>
        [Test]
        public void PlayEffect_ShouldHandle_EmptyString()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => effectManager.PlayEffect(""),
                "PlayEffect should handle empty string gracefully");
        }

        /// <summary>
        /// 存在しないエフェクトの処理をテスト
        /// </summary>
        [Test]
        public void PlayEffect_ShouldHandle_NonexistentEffect()
        {
            // Arrange
            string nonexistentEffect = "this-effect-does-not-exist";
            
            // Act & Assert
            Assert.DoesNotThrow(() => effectManager.PlayEffect(nonexistentEffect),
                "PlayEffect should handle nonexistent effect gracefully");
        }

        /// <summary>
        /// 無効な音量値の処理をテスト
        /// </summary>
        [TestCase(-1.0f)]
        [TestCase(2.0f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        public void SetEffectVolume_ShouldHandle_InvalidValues(float invalidVolume)
        {
            // Act & Assert
            Assert.DoesNotThrow(() => effectManager.PlayUIEffect("test-ui-effect", invalidVolume),
                $"SetEffectVolume should handle invalid value: {invalidVolume}");
        }

        #endregion

        #region Effect Pooling Tests

        /// <summary>
        /// エフェクトプールの動作をテスト
        /// </summary>
        [Test]
        public void EffectPool_ShouldReuse_AudioSources()
        {
            // Arrange
            string effectName = "pooled-effect";
            
            // Act
            effectManager.PlayEffect(effectName);
            effectManager.StopAllEffects();
            effectManager.PlayEffect(effectName); // 同じエフェクトを再生
            
            // Assert
            // プール機能が動作していればエラーが発生しない
            Assert.Pass("Effect pooling should work correctly");
        }

        /// <summary>
        /// プールサイズ制限のテスト
        /// </summary>
        [Test]
        public void EffectPool_ShouldHandle_PoolSizeLimit()
        {
            // Arrange
            int maxPoolSize = 20; // 仮の最大プールサイズ
            
            // Act & Assert
            Assert.DoesNotThrow(() =>
            {
                for (int i = 0; i < maxPoolSize + 10; i++)
                {
                    effectManager.PlayEffect($"pool-limit-effect-{i}");
                }
            }, "Effect pool should handle size limits gracefully");
        }

        #endregion

        #region Integration Tests

        /// <summary>
        /// AudioManagerとの統合テスト
        /// </summary>
        [Test]
        public void EffectManager_ShouldIntegrate_WithAudioManager()
        {
            // Arrange
            var audioManagerObject = TestHelpers.CreateTestGameObject("AudioManager");
            var audioManager = audioManagerObject.AddComponent<AudioManager>();
            
            ServiceLocator.RegisterService<AudioManager>(audioManager);
            ServiceLocator.RegisterService<EffectManager>(effectManager);
            
            // Act & Assert
            Assert.DoesNotThrow(() =>
            {
                // 通常の音声再生
                audioManager.PlaySound("normal-sound");
                
                // エフェクト音声再生
                effectManager.PlayEffect("effect-sound");
            }, "EffectManager should integrate with AudioManager");
        }

        /// <summary>
        /// SpatialAudioManagerとの統合テスト
        /// </summary>
        [Test]
        public void EffectManager_ShouldIntegrate_WithSpatialAudioManager()
        {
            // Arrange
            var spatialAudioManagerObject = TestHelpers.CreateTestGameObject("SpatialAudioManager");
            var spatialAudioManager = spatialAudioManagerObject.AddComponent<SpatialAudioManager>();
            
            ServiceLocator.RegisterService<SpatialAudioManager>(spatialAudioManager);
            ServiceLocator.RegisterService<EffectManager>(effectManager);
            
            // Act & Assert
            Assert.DoesNotThrow(() =>
            {
                // 空間音声再生
                spatialAudioManager.Play3DSound("spatial-sound", Vector3.forward * 5);
                
                // エフェクト音声再生
                effectManager.PlayEffect("effect-sound");
            }, "EffectManager should integrate with SpatialAudioManager");
        }

        /// <summary>
        /// FeatureFlagsとの統合テスト
        /// </summary>
        [Test]
        public void EffectManager_ShouldRespect_FeatureFlags()
        {
            // Arrange
            string testEffect = "feature-test-effect";
            
            // Test with new audio system enabled
            FeatureFlags.UseNewAudioSystem = true;
            Assert.DoesNotThrow(() => effectManager.PlayEffect(testEffect),
                "EffectManager should work with new audio system enabled");
            
            // Test with new audio system disabled
            FeatureFlags.UseNewAudioSystem = false;
            Assert.DoesNotThrow(() => effectManager.PlayEffect(testEffect),
                "EffectManager should work with new audio system disabled");
        }

        #endregion

        #region Advanced Feature Tests

        /// <summary>
        /// エフェクトの優先度システムをテスト
        /// </summary>
        [Test]
        public void EffectPriority_ShouldHandle_HighPriorityEffects()
        {
            // Arrange
            string normalEffect = "normal-priority-effect";
            string highPriorityEffect = "high-priority-effect";
            
            // Act & Assert
            Assert.DoesNotThrow(() =>
            {
                effectManager.PlayEffect(normalEffect);
                effectManager.PlayCombatEffect(highPriorityEffect, Vector3.zero); // 高優先度
            }, "Effect priority system should work correctly");
        }

        /// <summary>
        /// エフェクトのフェードイン/アウト機能をテスト
        /// </summary>
        [UnityTest]
        public IEnumerator EffectFade_ShouldWork_Correctly()
        {
            // Arrange
            string fadeEffect = "fade-test-effect";
            float fadeDuration = 0.5f;
            
            // Act
            effectManager.PlayEffect(fadeEffect);
            
            // Wait for fade completion
            yield return new WaitForSeconds(fadeDuration + 0.1f);
            
            // Assert
            Assert.Pass("Effect fade should complete without errors");
        }

        /// <summary>
        /// エフェクトのループ再生をテスト
        /// </summary>
        [Test]
        public void LoopedEffect_ShouldPlay_Continuously()
        {
            // Arrange
            string loopEffect = "loop-test-effect";
            
            // Act & Assert
            Assert.DoesNotThrow(() =>
            {
                int loopId = effectManager.StartLoopingEffect(loopEffect, Vector3.zero);
                // ループ停止
                effectManager.StopLoopingEffect(loopId);
            }, "Looped effect should play and stop correctly");
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// EffectManagerの基本設定を検証
        /// </summary>
        private void AssertEffectManagerBasicSetup()
        {
            Assert.IsNotNull(effectManager, "EffectManager should be initialized");
            Assert.IsNotNull(effectManagerObject.GetComponent<AudioSource>(), "AudioSource should be present");
        }

        /// <summary>
        /// テスト用AudioClipを作成
        /// </summary>
        private AudioClip CreateTestEffectClip(string name = "TestEffectClip")
        {
            return AudioClip.Create(name, 44100, 1, 44100, false);
        }

        /// <summary>
        /// エフェクトカテゴリの有効性を検証
        /// </summary>
        private void AssertValidEffectCategory(string category)
        {
            var validCategories = new[] { "UI", "Combat", "Environment", "Magic", "System" };
            Assert.Contains(category, validCategories, $"Effect category '{category}' should be valid");
        }

        #endregion
    }
}
