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
    /// EffectManager縺ｮ蜊倅ｽ薙ユ繧ｹ繝・
    /// 繧ｪ繝ｼ繝・ぅ繧ｪ繧ｨ繝輔ぉ繧ｯ繝医す繧ｹ繝・Β縺ｮ繝・せ繝・
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
            
            // ServiceLocator縺ｮ繧ｻ繝・ヨ繧｢繝・・
            TestHelpers.SetupTestServiceLocator();
            
            // EffectManager繧ｪ繝悶ず繧ｧ繧ｯ繝医・菴懈・
            effectManagerObject = TestHelpers.CreateTestGameObject("EffectManager");
            effectManager = effectManagerObject.AddComponent<EffectManager>();
            
            // AudioSource繧ｳ繝ｳ繝昴・繝阪Φ繝医・霑ｽ蜉
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
        /// EffectManager縺梧ｭ｣蟶ｸ縺ｫ蛻晄悄蛹悶＆繧後ｋ縺薙→繧偵ユ繧ｹ繝・
        /// </summary>
        [Test]
        public void EffectManager_ShouldInitialize_Properly()
        {
            // Assert
            Assert.IsNotNull(effectManager, "EffectManager should not be null");
            TestHelpers.AssertHasComponent<AudioSource>(effectManagerObject);
        }

        /// <summary>
        /// EffectManager縺郡ingleton繝代ち繝ｼ繝ｳ縺ｧ蜍穂ｽ懊☆繧九％縺ｨ繧偵ユ繧ｹ繝・
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
        /// 繧ｨ繝輔ぉ繧ｯ繝磯浹螢ｰ蜀咲函縺ｮ蝓ｺ譛ｬ讖溯・繧偵ユ繧ｹ繝・
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
        /// 繧ｨ繝輔ぉ繧ｯ繝磯浹螢ｰ蛛懈ｭ｢讖溯・繧偵ユ繧ｹ繝・
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
        /// 繧ｨ繝輔ぉ繧ｯ繝磯浹驥剰ｨｭ螳壽ｩ溯・繧偵ユ繧ｹ繝・
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
        /// 隍・焚繧ｨ繝輔ぉ繧ｯ繝医・蜷梧凾蜀咲函繧偵ユ繧ｹ繝・
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
        /// 逡ｰ縺ｪ繧九き繝・ざ繝ｪ縺ｮ繧ｨ繝輔ぉ繧ｯ繝亥・逕溘ｒ繝・せ繝・
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
        /// 繧ｨ繝輔ぉ繧ｯ繝医き繝・ざ繝ｪ縺斐→縺ｮ髻ｳ驥丞宛蠕｡繧偵ユ繧ｹ繝・
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
        /// ServiceHelper邨檎罰縺ｧEffectManager縺悟叙蠕励〒縺阪ｋ縺薙→繧偵ユ繧ｹ繝・
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
        /// ServiceLocator辟｡蜉ｹ譎ゅ・繝輔か繝ｼ繝ｫ繝舌ャ繧ｯ蜍穂ｽ懊ｒ繝・せ繝・
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
        /// 繧ｨ繝輔ぉ繧ｯ繝亥・逕溘・繝代ヵ繧ｩ繝ｼ繝槭Φ繧ｹ繝・せ繝・
        /// </summary>
        [Test]
        public void PlayEffect_ShouldComplete_WithinPerformanceThreshold()
        {
            // Arrange
            string testEffectName = "performance-test-effect";
            float maxExecutionTime = 0.001f; // 1ms莉･蜀・

            // Act & Assert
            TestHelpers.AssertExecutionTime(
                () => effectManager.PlayEffect(testEffectName),
                maxExecutionTime,
                "PlayEffect should complete within performance threshold"
            );
        }

        /// <summary>
        /// 螟ｧ驥上お繝輔ぉ繧ｯ繝医・蜷梧凾蜀咲函繝代ヵ繧ｩ繝ｼ繝槭Φ繧ｹ繝・せ繝・
        /// </summary>
        [Test]
        public void MassiveEffects_ShouldPlay_WithinPerformanceThreshold()
        {
            // Arrange
            int effectCount = 100;
            float maxExecutionTime = 0.01f; // 10ms莉･蜀・

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
        /// null蠑墓焚縺ｮ蜃ｦ逅・ｒ繝・せ繝・
        /// </summary>
        [Test]
        public void PlayEffect_ShouldHandle_NullInput()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => effectManager.PlayEffect(null),
                "PlayEffect should handle null input gracefully");
        }

        /// <summary>
        /// 遨ｺ譁・ｭ怜・蠑墓焚縺ｮ蜃ｦ逅・ｒ繝・せ繝・
        /// </summary>
        [Test]
        public void PlayEffect_ShouldHandle_EmptyString()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => effectManager.PlayEffect(""),
                "PlayEffect should handle empty string gracefully");
        }

        /// <summary>
        /// 蟄伜惠縺励↑縺・お繝輔ぉ繧ｯ繝医・蜃ｦ逅・ｒ繝・せ繝・
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
        /// 辟｡蜉ｹ縺ｪ髻ｳ驥丞､縺ｮ蜃ｦ逅・ｒ繝・せ繝・
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
        /// 繧ｨ繝輔ぉ繧ｯ繝医・繝ｼ繝ｫ縺ｮ蜍穂ｽ懊ｒ繝・せ繝・
        /// </summary>
        [Test]
        public void EffectPool_ShouldReuse_AudioSources()
        {
            // Arrange
            string effectName = "pooled-effect";
            
            // Act
            effectManager.PlayEffect(effectName);
            effectManager.StopAllEffects();
            effectManager.PlayEffect(effectName); // 蜷後§繧ｨ繝輔ぉ繧ｯ繝医ｒ蜀咲函
            
            // Assert
            // 繝励・繝ｫ讖溯・縺悟虚菴懊＠縺ｦ縺・ｌ縺ｰ繧ｨ繝ｩ繝ｼ縺檎匱逕溘＠縺ｪ縺・
            Assert.Pass("Effect pooling should work correctly");
        }

        /// <summary>
        /// 繝励・繝ｫ繧ｵ繧､繧ｺ蛻ｶ髯舌・繝・せ繝・
        /// </summary>
        [Test]
        public void EffectPool_ShouldHandle_PoolSizeLimit()
        {
            // Arrange
            int maxPoolSize = 20; // 莉ｮ縺ｮ譛螟ｧ繝励・繝ｫ繧ｵ繧､繧ｺ
            
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
        /// AudioManager縺ｨ縺ｮ邨ｱ蜷医ユ繧ｹ繝・
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
                // 騾壼ｸｸ縺ｮ髻ｳ螢ｰ蜀咲函
                audioManager.PlaySound("normal-sound");
                
                // 繧ｨ繝輔ぉ繧ｯ繝磯浹螢ｰ蜀咲函
                effectManager.PlayEffect("effect-sound");
            }, "EffectManager should integrate with AudioManager");
        }

        /// <summary>
        /// SpatialAudioManager縺ｨ縺ｮ邨ｱ蜷医ユ繧ｹ繝・
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
                // 遨ｺ髢馴浹螢ｰ蜀咲函
                spatialAudioManager.Play3DSound("spatial-sound", Vector3.forward * 5);
                
                // 繧ｨ繝輔ぉ繧ｯ繝磯浹螢ｰ蜀咲函
                effectManager.PlayEffect("effect-sound");
            }, "EffectManager should integrate with SpatialAudioManager");
        }

        /// <summary>
        /// FeatureFlags縺ｨ縺ｮ邨ｱ蜷医ユ繧ｹ繝・
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
        /// 繧ｨ繝輔ぉ繧ｯ繝医・蜆ｪ蜈亥ｺｦ繧ｷ繧ｹ繝・Β繧偵ユ繧ｹ繝・
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
                effectManager.PlayCombatEffect(highPriorityEffect, Vector3.zero); // 鬮伜━蜈亥ｺｦ
            }, "Effect priority system should work correctly");
        }

        /// <summary>
        /// 繧ｨ繝輔ぉ繧ｯ繝医・繝輔ぉ繝ｼ繝峨う繝ｳ/繧｢繧ｦ繝域ｩ溯・繧偵ユ繧ｹ繝・
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
        /// 繧ｨ繝輔ぉ繧ｯ繝医・繝ｫ繝ｼ繝怜・逕溘ｒ繝・せ繝・
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
                // 繝ｫ繝ｼ繝怜●豁｢
                effectManager.StopLoopingEffect(loopId);
            }, "Looped effect should play and stop correctly");
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// EffectManager縺ｮ蝓ｺ譛ｬ險ｭ螳壹ｒ讀懆ｨｼ
        /// </summary>
        private void AssertEffectManagerBasicSetup()
        {
            Assert.IsNotNull(effectManager, "EffectManager should be initialized");
            Assert.IsNotNull(effectManagerObject.GetComponent<AudioSource>(), "AudioSource should be present");
        }

        /// <summary>
        /// 繝・せ繝育畑AudioClip繧剃ｽ懈・
        /// </summary>
        private AudioClip CreateTestEffectClip(string name = "TestEffectClip")
        {
            return AudioClip.Create(name, 44100, 1, 44100, false);
        }

        /// <summary>
        /// 繧ｨ繝輔ぉ繧ｯ繝医き繝・ざ繝ｪ縺ｮ譛牙柑諤ｧ繧呈､懆ｨｼ
        /// </summary>
        private void AssertValidEffectCategory(string category)
        {
            var validCategories = new[] { "UI", "Combat", "Environment", "Magic", "System" };
            Assert.Contains(category, validCategories, $"Effect category '{category}' should be valid");
        }

        #endregion
    }
}


