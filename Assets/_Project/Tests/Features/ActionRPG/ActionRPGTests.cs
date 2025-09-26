using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using asterivo.Unity60.Core;
using asterivo.Unity60.Features.ActionRPG;
using asterivo.Unity60.Features.ActionRPG.Services;

namespace asterivo.Unity60.Tests.Features.ActionRPG
{
    /// <summary>
    /// ActionRPG讖溯・縺ｮ繝ｦ繝九ャ繝医ユ繧ｹ繝・
    /// </summary>
    [TestFixture]
    public class ActionRPGTests
    {
        [SetUp]
        public void Setup()
        {
            // ServiceLocator繧偵け繝ｪ繧｢
            ServiceLocator.Clear();
            
            // Bootstrapper縺ｮ蛻晄悄蛹也憾諷九ｒ繝ｪ繧ｻ繝・ヨ・亥渚蟆・ｒ菴ｿ逕ｨ・・
            var bootstrapperType = typeof(ActionRPGBootstrapper);
            var isInitializedField = bootstrapperType.GetField("_isInitialized", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            isInitializedField?.SetValue(null, false);
        }

        [TearDown]
        public void TearDown()
        {
            ActionRPGBootstrapper.Shutdown();
            ServiceLocator.Clear();
        }

        [Test]
        public void ActionRPGBootstrapper_Should_Initialize()
        {
            // Act
            ActionRPGBootstrapper.Initialize();

            // Assert
            Assert.IsTrue(ActionRPGBootstrapper.IsInitialized,
                "ActionRPGBootstrapper縺悟・譛溷喧縺輔ｌ縺ｦ縺・∪縺帙ｓ");

            var actionRPGService = ServiceLocator.TryGet<IActionRPGService>(out var service);
            Assert.IsTrue(actionRPGService, "IActionRPGService縺郡erviceLocator縺ｫ逋ｻ骭ｲ縺輔ｌ縺ｦ縺・∪縺帙ｓ");
            Assert.IsNotNull(service, "IActionRPGService縺系ull縺ｧ縺・);
        }

        [Test]
        public void ActionRPGService_Should_Add_Experience()
        {
            // Arrange
            ActionRPGBootstrapper.Initialize();
            var service = ServiceLocator.Get<IActionRPGService>();
            int experienceGained = 0;

            service.OnExperienceGained += (amount) => { experienceGained = amount; };

            // Act
            service.AddExperience(100);

            // Assert
            Assert.AreEqual(100, experienceGained, "邨碁ｨ灘､繧､繝吶Φ繝医′逋ｺ陦後＆繧後∪縺帙ｓ縺ｧ縺励◆");
        }

        [Test]
        public void ActionRPGService_Should_Track_Rune_Collection()
        {
            // Arrange
            ActionRPGBootstrapper.Initialize();
            var service = ServiceLocator.Get<IActionRPGService>();

            // Act
            service.NotifyResourceCollected(50);
            service.NotifyResourceCollected(30);

            // Assert
            var registry = ActionRPGBootstrapper.GetServiceRegistry();
            var stats = registry.GetRuneStatistics();
            Assert.AreEqual(80, stats.total, "繝ｫ繝ｼ繝ｳ縺ｮ蜷郁ｨ域焚縺梧ｭ｣縺励￥縺ゅｊ縺ｾ縺帙ｓ");
            Assert.AreEqual(80, stats.session, "繧ｻ繝・す繝ｧ繝ｳ繝ｫ繝ｼ繝ｳ謨ｰ縺梧ｭ｣縺励￥縺ゅｊ縺ｾ縺帙ｓ");
        }

        [Test]
        public void ActionRPGService_Should_Reset_Session_Stats()
        {
            // Arrange
            ActionRPGBootstrapper.Initialize();
            var service = ServiceLocator.Get<IActionRPGService>();
            var registry = ActionRPGBootstrapper.GetServiceRegistry();

            service.NotifyResourceCollected(100);

            // Act
            registry.ResetSessionStats();

            // Assert
            var stats = registry.GetRuneStatistics();
            Assert.AreEqual(100, stats.total, "蜷郁ｨ医Ν繝ｼ繝ｳ謨ｰ縺後Μ繧ｻ繝・ヨ縺輔ｌ縺ｦ縺励∪縺・∪縺励◆");
            Assert.AreEqual(0, stats.session, "繧ｻ繝・す繝ｧ繝ｳ繝ｫ繝ｼ繝ｳ謨ｰ縺後Μ繧ｻ繝・ヨ縺輔ｌ縺ｦ縺・∪縺帙ｓ");
        }

        [Test]
        public void ActionRPGService_Should_Handle_Player_Assignment()
        {
            // Arrange
            ActionRPGBootstrapper.Initialize();
            var service = ServiceLocator.Get<IActionRPGService>();
            var registry = ActionRPGBootstrapper.GetServiceRegistry();

            var mockPlayer = new GameObject("TestPlayer");

            // Act
            registry.SetPlayerGameObject(mockPlayer);

            // Assert
            var retrievedPlayer = service.GetPlayerGameObject();
            Assert.AreEqual(mockPlayer, retrievedPlayer,
                "繝励Ξ繧､繝､繝ｼ繧ｪ繝悶ず繧ｧ繧ｯ繝医′豁｣縺励￥險ｭ螳壹＆繧後※縺・∪縺帙ｓ");

            // Cleanup
            GameObject.DestroyImmediate(mockPlayer);
        }

        [Test]
        public void ActionRPGService_Should_Provide_Experience_Info()
        {
            // Arrange
            ActionRPGBootstrapper.Initialize();
            var service = ServiceLocator.Get<IActionRPGService>();

            // Act
            var expInfo = service.GetExperienceInfo();

            // Assert
            Assert.AreEqual(0, expInfo.currentExp, "蛻晄悄邨碁ｨ灘､縺・縺ｧ縺ｯ縺ゅｊ縺ｾ縺帙ｓ");
            Assert.AreEqual(1, expInfo.currentLevel, "蛻晄悄繝ｬ繝吶Ν縺・縺ｧ縺ｯ縺ゅｊ縺ｾ縺帙ｓ");
        }

        [Test]
        public void ActionRPGBootstrapper_Should_Handle_Multiple_Initialize()
        {
            // Act
            ActionRPGBootstrapper.Initialize();
            ActionRPGBootstrapper.Initialize(); // 2蝗樒岼縺ｮ蛻晄悄蛹・

            // Assert - 2蝗樒岼縺ｮ蛻晄悄蛹悶・辟｡隕悶＆繧後ｋ縺ｹ縺・
            Assert.IsTrue(ActionRPGBootstrapper.IsInitialized);
            
            // ServiceLocator縺ｫ1縺､縺縺醍匳骭ｲ縺輔ｌ縺ｦ縺・ｋ縺薙→繧堤｢ｺ隱・
            Assert.IsTrue(ServiceLocator.TryGet<IActionRPGService>(out var service));
            Assert.IsNotNull(service);
        }

        [Test]
        public void ActionRPGBootstrapper_Should_Handle_Shutdown()
        {
            // Arrange
            ActionRPGBootstrapper.Initialize();
            Assert.IsTrue(ActionRPGBootstrapper.IsInitialized);

            // Act
            ActionRPGBootstrapper.Shutdown();

            // Assert
            Assert.IsFalse(ActionRPGBootstrapper.IsInitialized,
                "Shutdown蠕後ｂ蛻晄悄蛹也憾諷九′邯ｭ謖√＆繧後※縺・∪縺・);
        }

        [Test]
        public void Three_Layer_Architecture_Should_Be_Maintained()
        {
            // Core螻､縺ｮIActionRPGService縺悟ｭ伜惠
            Assert.IsNotNull(typeof(IActionRPGService));

            // Feature螻､縺ｮActionRPGServiceRegistry縺悟ｭ伜惠
            Assert.IsNotNull(typeof(ActionRPGServiceRegistry));

            // ActionRPGBootstrapper縺熊eature螻､縺ｫ蟄伜惠
            Assert.IsNotNull(typeof(ActionRPGBootstrapper));

            // 蜷榊燕遨ｺ髢薙・讀懆ｨｼ
            Assert.IsTrue(typeof(IActionRPGService).Namespace.Contains("Core.Services"),
                "IActionRPGService縺靴ore螻､縺ｫ驟咲ｽｮ縺輔ｌ縺ｦ縺・∪縺帙ｓ");
            Assert.IsTrue(typeof(ActionRPGServiceRegistry).Namespace.Contains("Features.ActionRPG"),
                "ActionRPGServiceRegistry縺熊eature螻､縺ｫ驟咲ｽｮ縺輔ｌ縺ｦ縺・∪縺帙ｓ");
        }
    }
}


