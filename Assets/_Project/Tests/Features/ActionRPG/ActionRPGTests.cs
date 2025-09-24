using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Features.ActionRPG;
using asterivo.Unity60.Features.ActionRPG.Services;

namespace asterivo.Unity60.Tests.Features.ActionRPG
{
    /// <summary>
    /// ActionRPG機能のユニットテスト
    /// </summary>
    [TestFixture]
    public class ActionRPGTests
    {
        [SetUp]
        public void Setup()
        {
            // ServiceLocatorをクリア
            ServiceLocator.Clear();
            
            // Bootstrapperの初期化状態をリセット（反射を使用）
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
                "ActionRPGBootstrapperが初期化されていません");

            var actionRPGService = ServiceLocator.TryGet<IActionRPGService>(out var service);
            Assert.IsTrue(actionRPGService, "IActionRPGServiceがServiceLocatorに登録されていません");
            Assert.IsNotNull(service, "IActionRPGServiceがnullです");
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
            Assert.AreEqual(100, experienceGained, "経験値イベントが発行されませんでした");
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
            Assert.AreEqual(80, stats.total, "ルーンの合計数が正しくありません");
            Assert.AreEqual(80, stats.session, "セッションルーン数が正しくありません");
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
            Assert.AreEqual(100, stats.total, "合計ルーン数がリセットされてしまいました");
            Assert.AreEqual(0, stats.session, "セッションルーン数がリセットされていません");
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
                "プレイヤーオブジェクトが正しく設定されていません");

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
            Assert.AreEqual(0, expInfo.currentExp, "初期経験値が0ではありません");
            Assert.AreEqual(1, expInfo.currentLevel, "初期レベルが1ではありません");
        }

        [Test]
        public void ActionRPGBootstrapper_Should_Handle_Multiple_Initialize()
        {
            // Act
            ActionRPGBootstrapper.Initialize();
            ActionRPGBootstrapper.Initialize(); // 2回目の初期化

            // Assert - 2回目の初期化は無視されるべき
            Assert.IsTrue(ActionRPGBootstrapper.IsInitialized);
            
            // ServiceLocatorに1つだけ登録されていることを確認
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
                "Shutdown後も初期化状態が維持されています");
        }

        [Test]
        public void Three_Layer_Architecture_Should_Be_Maintained()
        {
            // Core層のIActionRPGServiceが存在
            Assert.IsNotNull(typeof(IActionRPGService));

            // Feature層のActionRPGServiceRegistryが存在
            Assert.IsNotNull(typeof(ActionRPGServiceRegistry));

            // ActionRPGBootstrapperがFeature層に存在
            Assert.IsNotNull(typeof(ActionRPGBootstrapper));

            // 名前空間の検証
            Assert.IsTrue(typeof(IActionRPGService).Namespace.Contains("Core.Services"),
                "IActionRPGServiceがCore層に配置されていません");
            Assert.IsTrue(typeof(ActionRPGServiceRegistry).Namespace.Contains("Features.ActionRPG"),
                "ActionRPGServiceRegistryがFeature層に配置されていません");
        }
    }
}
