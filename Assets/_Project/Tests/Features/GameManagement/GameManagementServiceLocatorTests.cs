using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Core.Services.Interfaces;
using asterivo.Unity60.Core.Bootstrap;
using asterivo.Unity60.Features.GameManagement.Services;
using asterivo.Unity60.Features.GameManagement.Interfaces;
using asterivo.Unity60.Features.Combat.Services;
using asterivo.Unity60.Features.Combat.Interfaces;

namespace asterivo.Unity60.Tests.Features.GameManagement
{
    /// <summary>
    /// ServiceLocatorとGameManagement機能の統合テスト
    /// GameBootstrapperとの連携を検証
    /// </summary>
    [TestFixture]
    public class GameManagementServiceLocatorTests
    {
        private GameObject _bootstrapperObject;
        private GameBootstrapper _bootstrapper;

        [SetUp]
        public void Setup()
        {
            // ServiceLocatorをクリア
            ServiceLocator.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            if (_bootstrapperObject != null)
            {
                Object.DestroyImmediate(_bootstrapperObject);
            }
            ServiceLocator.Clear();
        }

        #region GameBootstrapper Integration Tests

        [UnityTest]
        public IEnumerator GameBootstrapper_RegistersGameManagerService()
        {
            // Arrange
            _bootstrapperObject = new GameObject("TestBootstrapper");
            _bootstrapper = _bootstrapperObject.AddComponent<GameBootstrapper>();

            // Wait for Awake and initialization
            yield return null;

            // Act & Assert
            Assert.IsTrue(ServiceLocator.TryGet<IGameManager>(out var gameManager));
            Assert.IsNotNull(gameManager);
            Assert.IsTrue(gameManager.IsServiceActive);
            Assert.AreEqual("GameManagerService", gameManager.ServiceName);
        }

        [UnityTest]
        public IEnumerator GameBootstrapper_RegistersMultipleServices()
        {
            // Arrange
            _bootstrapperObject = new GameObject("TestBootstrapper");
            _bootstrapper = _bootstrapperObject.AddComponent<GameBootstrapper>();

            // Wait for Awake and initialization
            yield return null;

            // Assert - EventManager
            Assert.IsTrue(ServiceLocator.TryGet<IEventManager>(out var eventManager));
            Assert.IsNotNull(eventManager);

            // Assert - CombatService
            Assert.IsTrue(ServiceLocator.TryGet<ICombatService>(out var combatService));
            Assert.IsNotNull(combatService);

            // Assert - GameManagerService
            Assert.IsTrue(ServiceLocator.TryGet<IGameManager>(out var gameManager));
            Assert.IsNotNull(gameManager);
        }

        #endregion

        #region Service Registration Tests

        [Test]
        public void ServiceLocator_CanRegisterAndRetrieveGameManager()
        {
            // Arrange
            var gameManager = new GameManagerService();

            // Act
            ServiceLocator.Register<IGameManager>(gameManager);
            var retrieved = ServiceLocator.Get<IGameManager>();

            // Assert
            Assert.IsNotNull(retrieved);
            Assert.AreSame(gameManager, retrieved);
            Assert.IsTrue(retrieved.IsServiceActive);
        }

        [Test]
        public void ServiceLocator_TryGet_ReturnsFalseWhenNotRegistered()
        {
            // Act
            bool result = ServiceLocator.TryGet<IGameManager>(out var service);

            // Assert
            Assert.IsFalse(result);
            Assert.IsNull(service);
        }

        [Test]
        public void ServiceLocator_Clear_RemovesGameManager()
        {
            // Arrange
            var gameManager = new GameManagerService();
            ServiceLocator.Register<IGameManager>(gameManager);

            // Act
            ServiceLocator.Clear();

            // Assert
            Assert.IsFalse(ServiceLocator.TryGet<IGameManager>(out var service));
            Assert.IsFalse(gameManager.IsServiceActive);
        }

        #endregion

        #region Service Interaction Tests

        [Test]
        public void GameManager_WorksWithEventManager()
        {
            // Arrange
            var eventManager = new EventManager();
            var gameManager = new GameManagerService();

            ServiceLocator.Register<IEventManager>(eventManager);
            ServiceLocator.Register<IGameManager>(gameManager);

            // Act & Assert
            Assert.IsTrue(gameManager.IsServiceActive);
            Assert.IsTrue(eventManager.IsServiceActive);

            // Test interaction
            Assert.DoesNotThrow(() => gameManager.ChangeGameState(Core.Types.GameState.Playing));
        }

        [Test]
        public void MultipleServices_CanCoexist()
        {
            // Arrange
            var eventManager = new EventManager();
            var combatService = new CombatService();
            var gameManager = new GameManagerService();

            // Act
            ServiceLocator.Register<IEventManager>(eventManager);
            ServiceLocator.Register<ICombatService>(combatService);
            ServiceLocator.Register<IGameManager>(gameManager);

            // Assert
            Assert.IsNotNull(ServiceLocator.Get<IEventManager>());
            Assert.IsNotNull(ServiceLocator.Get<ICombatService>());
            Assert.IsNotNull(ServiceLocator.Get<IGameManager>());

            // Verify they are different instances
            Assert.AreNotSame(
                ServiceLocator.Get<IEventManager>(),
                ServiceLocator.Get<IGameManager>()
            );
        }

        [Test]
        public void GameManager_OnServiceUnregistered_ClearsState()
        {
            // Arrange
            var eventManager = new EventManager();
            var gameManager = new GameManagerService();

            ServiceLocator.Register<IEventManager>(eventManager);
            ServiceLocator.Register<IGameManager>(gameManager);

            gameManager.ChangeGameState(Core.Types.GameState.Playing);
            gameManager.UpdateGameTime(100f);

            // Act
            gameManager.OnServiceUnregistered();

            // Assert
            Assert.IsFalse(gameManager.IsServiceActive);
            // State is preserved but service is inactive
            Assert.AreEqual(Core.Types.GameState.Playing, gameManager.CurrentGameState);
        }

        #endregion

        #region Service Dependency Tests

        [Test]
        public void GameManager_HandlesNullEventManagerGracefully()
        {
            // Arrange - Register GameManager without EventManager
            var gameManager = new GameManagerService();
            ServiceLocator.Register<IGameManager>(gameManager);

            // Act - Should not throw even without EventManager
            Assert.DoesNotThrow(() =>
            {
                gameManager.ChangeGameState(Core.Types.GameState.Playing);
                gameManager.StartGame();
                gameManager.PauseGame();
            });
        }

        [Test]
        public void GameManager_HandlesNullSceneLoaderGracefully()
        {
            // Arrange
            var eventManager = new EventManager();
            var gameManager = new GameManagerService();

            ServiceLocator.Register<IEventManager>(eventManager);
            ServiceLocator.Register<IGameManager>(gameManager);

            // Act - Should handle missing SceneLoadingService
            Assert.DoesNotThrow(() =>
            {
                gameManager.StartGame();
                gameManager.RestartGame();
                gameManager.ReturnToMenu();
            });

            // Assert - State should still change appropriately
            Assert.AreEqual(Core.Types.GameState.Loading, gameManager.CurrentGameState);
        }

        #endregion

        #region Service Priority Tests

        [Test]
        public void ServiceRegistration_SupportsMultipleImplementations()
        {
            // This test verifies that we can have multiple game management services
            // working together (e.g., GameManagerService and GameStateManagerService)

            // Arrange
            var eventManager = new EventManager();
            var gameManager = new GameManagerService();
            var stateManagerObject = new GameObject("StateManager");
            var stateManager = stateManagerObject.AddComponent<Core.Services.Implementations.GameStateManagerService>();

            try
            {
                // Act
                ServiceLocator.Register<IEventManager>(eventManager);
                ServiceLocator.Register<IGameManager>(gameManager);
                stateManager.RegisterServices();

                // Assert
                Assert.IsTrue(ServiceLocator.TryGet<IGameManager>(out var gm));
                Assert.IsTrue(ServiceLocator.TryGet<IGameStateManager>(out var sm));
                Assert.IsNotNull(gm);
                Assert.IsNotNull(sm);
            }
            finally
            {
                Object.DestroyImmediate(stateManagerObject);
            }
        }

        #endregion

        #region Service Replacement Tests

        [Test]
        public void ServiceLocator_AllowsServiceReplacement()
        {
            // Arrange
            var gameManager1 = new GameManagerService();
            var gameManager2 = new GameManagerService();

            // Act
            ServiceLocator.Register<IGameManager>(gameManager1);
            ServiceLocator.Unregister<IGameManager>();
            ServiceLocator.Register<IGameManager>(gameManager2);

            // Assert
            var retrieved = ServiceLocator.Get<IGameManager>();
            Assert.AreSame(gameManager2, retrieved);
            Assert.AreNotSame(gameManager1, retrieved);
        }

        #endregion
    }
}
