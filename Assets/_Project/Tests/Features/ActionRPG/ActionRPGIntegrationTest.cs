using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using asterivo.Unity60.Core;
using asterivo.Unity60.Features.ActionRPG;

namespace asterivo.Unity60.Tests.Features.ActionRPG
{
    /// <summary>
    /// ActionRPG讖溯・縺ｮ邨ｱ蜷医ユ繧ｹ繝・
    /// 螳滄圀縺ｮ繧ｲ繝ｼ繝繧ｷ繝翫Μ繧ｪ縺ｧ縺ｮ蜍穂ｽ懊ｒ讀懆ｨｼ
    /// </summary>
    [TestFixture]
    public class ActionRPGIntegrationTest
    {
        private GameObject testObject;
        private ActionRPGManager actionRPGManager;

        [SetUp]
        public void Setup()
        {
            // ServiceLocator繧偵け繝ｪ繧｢
            ServiceLocator.Clear();

            // 繝・せ繝育畑GameObject繧剃ｽ懈・
            testObject = new GameObject("TestActionRPGManager");
        }

        [TearDown]
        public void TearDown()
        {
            // 繝・せ繝医が繝悶ず繧ｧ繧ｯ繝医ｒ遐ｴ譽・
            if (testObject != null)
            {
                Object.DestroyImmediate(testObject);
            }

            // Bootstrapper繧偵す繝｣繝・ヨ繝繧ｦ繝ｳ
            if (ActionRPGBootstrapper.IsInitialized)
            {
                ActionRPGBootstrapper.Shutdown();
            }

            // ServiceLocator繧偵け繝ｪ繧｢
            ServiceLocator.Clear();
        }

        [UnityTest]
        public IEnumerator ActionRPGManager_Should_Initialize_Automatically()
        {
            // Act - ActionRPGManager繧定ｿｽ蜉
            actionRPGManager = testObject.AddComponent<ActionRPGManager>();

            // Start()縺悟ｮ溯｡後＆繧後ｋ縺ｮ繧貞ｾ・▽
            yield return null;

            // Assert
            Assert.IsTrue(actionRPGManager.IsInitialized,
                "ActionRPGManager縺悟・譛溷喧縺輔ｌ縺ｦ縺・∪縺帙ｓ");
            Assert.IsTrue(ActionRPGBootstrapper.IsInitialized,
                "Bootstrapper縺瑚・蜍募・譛溷喧縺輔ｌ縺ｦ縺・∪縺帙ｓ");
        }

        [UnityTest]
        public IEnumerator ActionRPGManager_Should_Handle_Player_Setup()
        {
            // Arrange
            actionRPGManager = testObject.AddComponent<ActionRPGManager>();
            yield return null;

            var mockPlayer = new GameObject("TestPlayer");

            // Act
            actionRPGManager.SetPlayer(mockPlayer);
            yield return null;

            // Assert
            var service = ServiceLocator.Get<IActionRPGService>();
            var retrievedPlayer = service.GetPlayerGameObject();
            Assert.AreEqual(mockPlayer, retrievedPlayer,
                "繝励Ξ繧､繝､繝ｼ縺梧ｭ｣縺励￥險ｭ螳壹＆繧後※縺・∪縺帙ｓ");

            // Cleanup
            GameObject.DestroyImmediate(mockPlayer);
        }

        [UnityTest]
        public IEnumerator ActionRPGManager_Should_Track_Experience()
        {
            // Arrange
            actionRPGManager = testObject.AddComponent<ActionRPGManager>();
            yield return null;

            // Act
            actionRPGManager.AddExperience(100);
            actionRPGManager.AddExperience(50);
            yield return null;

            // Assert
            var expInfo = actionRPGManager.GetExperienceInfo();
            // 豕ｨ: 螳滄圀縺ｮStatComponent縺後↑縺・・縺ｧ縲∫ｵ碁ｨ灘､縺ｯ霑ｽ蜉縺輔ｌ縺ｪ縺・
            Assert.AreEqual(0, expInfo.currentExp,
                "繝・せ繝育腸蠅・〒縺ｯ邨碁ｨ灘､縺・縺ｮ縺ｾ縺ｾ縺ｧ縺・);
            Assert.AreEqual(1, expInfo.currentLevel,
                "蛻晄悄繝ｬ繝吶Ν縺ｯ1縺ｧ縺・);
        }

        [UnityTest]
        public IEnumerator ActionRPGManager_Should_Track_Rune_Collection()
        {
            // Arrange
            actionRPGManager = testObject.AddComponent<ActionRPGManager>();
            yield return null;

            // Act
            actionRPGManager.NotifyResourceCollected(30);
            actionRPGManager.NotifyResourceCollected(20);
            yield return null;

            // Assert
            var runeStats = actionRPGManager.GetRuneStatistics();
            Assert.AreEqual(50, runeStats.total,
                "繝ｫ繝ｼ繝ｳ縺ｮ蜷郁ｨ域焚縺梧ｭ｣縺励￥縺ゅｊ縺ｾ縺帙ｓ");
            Assert.AreEqual(50, runeStats.session,
                "繧ｻ繝・す繝ｧ繝ｳ繝ｫ繝ｼ繝ｳ謨ｰ縺梧ｭ｣縺励￥縺ゅｊ縺ｾ縺帙ｓ");
        }

        [UnityTest]
        public IEnumerator ActionRPGManager_Should_Reset_Session_Stats()
        {
            // Arrange
            actionRPGManager = testObject.AddComponent<ActionRPGManager>();
            yield return null;

            actionRPGManager.NotifyResourceCollected(100);
            yield return null;

            // Act
            actionRPGManager.ResetSessionStats();
            yield return null;

            // Assert
            var runeStats = actionRPGManager.GetRuneStatistics();
            Assert.AreEqual(100, runeStats.total,
                "蜷郁ｨ医Ν繝ｼ繝ｳ謨ｰ縺後Μ繧ｻ繝・ヨ縺輔ｌ縺ｦ縺励∪縺・∪縺励◆");
            Assert.AreEqual(0, runeStats.session,
                "繧ｻ繝・す繝ｧ繝ｳ繝ｫ繝ｼ繝ｳ謨ｰ縺後Μ繧ｻ繝・ヨ縺輔ｌ縺ｦ縺・∪縺帙ｓ");
        }

        [UnityTest]
        public IEnumerator ActionRPGManager_Should_Handle_Debug_Commands()
        {
            // Arrange
            actionRPGManager = testObject.AddComponent<ActionRPGManager>();
            yield return null;

            int initialRuneCount = actionRPGManager.GetRuneStatistics().total;

            // Act - 繝・せ繝育ｵ碁ｨ灘､繧定ｿｽ蜉
            actionRPGManager.AddTestExperience();
            yield return null;

            // Assert - 繝ｫ繝ｼ繝ｳ繧らｵ碁ｨ灘､縺ｨ縺励※霑ｽ蜉縺輔ｌ繧・
            var newRuneCount = actionRPGManager.GetRuneStatistics().total;
            Assert.AreEqual(initialRuneCount + 100, newRuneCount,
                "繝・せ繝育ｵ碁ｨ灘､縺後Ν繝ｼ繝ｳ縺ｨ縺励※霑ｽ蜉縺輔ｌ縺ｦ縺・∪縺帙ｓ");

            // Act - 螟ｧ驥冗ｵ碁ｨ灘､繧定ｿｽ蜉
            actionRPGManager.AddLargeExperience();
            yield return null;

            // Assert
            var finalRuneCount = actionRPGManager.GetRuneStatistics().total;
            Assert.AreEqual(initialRuneCount + 1100, finalRuneCount,
                "螟ｧ驥冗ｵ碁ｨ灘､縺後Ν繝ｼ繝ｳ縺ｨ縺励※霑ｽ蜉縺輔ｌ縺ｦ縺・∪縺帙ｓ");
        }

        [UnityTest]
        public IEnumerator Multiple_ActionRPGManagers_Should_Share_Service()
        {
            // Arrange
            var manager1 = testObject.AddComponent<ActionRPGManager>();
            var testObject2 = new GameObject("TestActionRPGManager2");
            var manager2 = testObject2.AddComponent<ActionRPGManager>();
            yield return null;

            // Act - 荳譁ｹ縺ｮ繝槭ロ繝ｼ繧ｸ繝｣繝ｼ縺ｧ繝ｫ繝ｼ繝ｳ繧貞庶髮・
            manager1.NotifyResourceCollected(50);
            yield return null;

            // Assert - 荳｡譁ｹ縺ｮ繝槭ロ繝ｼ繧ｸ繝｣繝ｼ縺悟酔縺倥ョ繝ｼ繧ｿ繧貞盾辣ｧ
            var stats1 = manager1.GetRuneStatistics();
            var stats2 = manager2.GetRuneStatistics();
            Assert.AreEqual(stats1.total, stats2.total,
                "隍・焚縺ｮActionRPGManager縺悟酔縺倥し繝ｼ繝薙せ繧貞・譛峨＠縺ｦ縺・∪縺帙ｓ");

            // Cleanup
            GameObject.DestroyImmediate(testObject2);
        }

        [Test]
        public void ServiceLocator_Should_Maintain_ActionRPGService()
        {
            // Arrange
            // Bootstrapper縺ｮ蛻晄悄蛹也憾諷九ｒ繝ｪ繧ｻ繝・ヨ
            var bootstrapperType = typeof(ActionRPGBootstrapper);
            var isInitializedField = bootstrapperType.GetField("_isInitialized",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            isInitializedField?.SetValue(null, false);

            // Act
            ActionRPGBootstrapper.Initialize();

            // Assert
            var actionRPGService = ServiceLocator.Get<IActionRPGService>();
            Assert.IsNotNull(actionRPGService, "ActionRPGService縺悟茜逕ｨ蜿ｯ閭ｽ縺ｧ縺・);
        }
    }
}


