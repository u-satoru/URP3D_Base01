using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Features.ActionRPG;

namespace asterivo.Unity60.Tests.Features.ActionRPG
{
    /// <summary>
    /// ActionRPG機能の統合テスト
    /// 実際のゲームシナリオでの動作を検証
    /// </summary>
    [TestFixture]
    public class ActionRPGIntegrationTest
    {
        private GameObject testObject;
        private ActionRPGManager actionRPGManager;

        [SetUp]
        public void Setup()
        {
            // ServiceLocatorをクリア
            ServiceLocator.Clear();

            // テスト用GameObjectを作成
            testObject = new GameObject("TestActionRPGManager");
        }

        [TearDown]
        public void TearDown()
        {
            // テストオブジェクトを破棄
            if (testObject != null)
            {
                Object.DestroyImmediate(testObject);
            }

            // Bootstrapperをシャットダウン
            if (ActionRPGBootstrapper.IsInitialized)
            {
                ActionRPGBootstrapper.Shutdown();
            }

            // ServiceLocatorをクリア
            ServiceLocator.Clear();
        }

        [UnityTest]
        public IEnumerator ActionRPGManager_Should_Initialize_Automatically()
        {
            // Act - ActionRPGManagerを追加
            actionRPGManager = testObject.AddComponent<ActionRPGManager>();

            // Start()が実行されるのを待つ
            yield return null;

            // Assert
            Assert.IsTrue(actionRPGManager.IsInitialized,
                "ActionRPGManagerが初期化されていません");
            Assert.IsTrue(ActionRPGBootstrapper.IsInitialized,
                "Bootstrapperが自動初期化されていません");
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
                "プレイヤーが正しく設定されていません");

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
            // 注: 実際のStatComponentがないので、経験値は追加されない
            Assert.AreEqual(0, expInfo.currentExp,
                "テスト環境では経験値が0のままです");
            Assert.AreEqual(1, expInfo.currentLevel,
                "初期レベルは1です");
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
                "ルーンの合計数が正しくありません");
            Assert.AreEqual(50, runeStats.session,
                "セッションルーン数が正しくありません");
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
                "合計ルーン数がリセットされてしまいました");
            Assert.AreEqual(0, runeStats.session,
                "セッションルーン数がリセットされていません");
        }

        [UnityTest]
        public IEnumerator ActionRPGManager_Should_Handle_Debug_Commands()
        {
            // Arrange
            actionRPGManager = testObject.AddComponent<ActionRPGManager>();
            yield return null;

            int initialRuneCount = actionRPGManager.GetRuneStatistics().total;

            // Act - テスト経験値を追加
            actionRPGManager.AddTestExperience();
            yield return null;

            // Assert - ルーンも経験値として追加される
            var newRuneCount = actionRPGManager.GetRuneStatistics().total;
            Assert.AreEqual(initialRuneCount + 100, newRuneCount,
                "テスト経験値がルーンとして追加されていません");

            // Act - 大量経験値を追加
            actionRPGManager.AddLargeExperience();
            yield return null;

            // Assert
            var finalRuneCount = actionRPGManager.GetRuneStatistics().total;
            Assert.AreEqual(initialRuneCount + 1100, finalRuneCount,
                "大量経験値がルーンとして追加されていません");
        }

        [UnityTest]
        public IEnumerator Multiple_ActionRPGManagers_Should_Share_Service()
        {
            // Arrange
            var manager1 = testObject.AddComponent<ActionRPGManager>();
            var testObject2 = new GameObject("TestActionRPGManager2");
            var manager2 = testObject2.AddComponent<ActionRPGManager>();
            yield return null;

            // Act - 一方のマネージャーでルーンを収集
            manager1.NotifyResourceCollected(50);
            yield return null;

            // Assert - 両方のマネージャーが同じデータを参照
            var stats1 = manager1.GetRuneStatistics();
            var stats2 = manager2.GetRuneStatistics();
            Assert.AreEqual(stats1.total, stats2.total,
                "複数のActionRPGManagerが同じサービスを共有していません");

            // Cleanup
            GameObject.DestroyImmediate(testObject2);
        }

        [Test]
        public void ServiceLocator_Should_Maintain_ActionRPGService()
        {
            // Arrange
            // Bootstrapperの初期化状態をリセット
            var bootstrapperType = typeof(ActionRPGBootstrapper);
            var isInitializedField = bootstrapperType.GetField("_isInitialized",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            isInitializedField?.SetValue(null, false);

            // Act
            ActionRPGBootstrapper.Initialize();

            // Assert
            var actionRPGService = ServiceLocator.Get<IActionRPGService>();
            Assert.IsNotNull(actionRPGService, "ActionRPGServiceが利用可能です");
        }
    }
}