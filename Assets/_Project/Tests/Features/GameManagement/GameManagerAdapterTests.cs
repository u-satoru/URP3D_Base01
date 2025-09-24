using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Core.Services.Interfaces;
using asterivo.Unity60.Core.Types;
using asterivo.Unity60.Core.Commands;
using asterivo.Unity60.Features.GameManagement;
using asterivo.Unity60.Features.GameManagement.Services;
using asterivo.Unity60.Features.GameManagement.Interfaces;

namespace asterivo.Unity60.Tests.Features.GameManagement
{
    /// <summary>
    /// GameManagerAdapterのテスト
    /// レガシー互換性と新サービスとのブリッジ機能を検証
    /// </summary>
    [TestFixture]
    public class GameManagerAdapterTests
    {
        private GameObject _testObject;
        private GameManagerAdapter _adapter;
        private GameManagerService _gameManagerService;
        private MockEventManager _mockEventManager;

        [SetUp]
        public void Setup()
        {
            // ServiceLocatorをクリア
            ServiceLocator.Clear();

            // MockEventManagerを登録
            _mockEventManager = new MockEventManager();
            ServiceLocator.Register<IEventManager>(_mockEventManager);

            // GameManagerServiceを登録
            _gameManagerService = new GameManagerService();
            ServiceLocator.Register<IGameManager>(_gameManagerService);

            // GameManagerAdapterを持つGameObjectを作成
            _testObject = new GameObject("TestAdapter");
            _adapter = _testObject.AddComponent<GameManagerAdapter>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_testObject != null)
            {
                Object.DestroyImmediate(_testObject);
            }
            ServiceLocator.Clear();
        }

        #region Property Proxy Tests

        [Test]
        public void Adapter_ProxiesCurrentGameState()
        {
            // Arrange
            _gameManagerService.ChangeGameState(GameState.Playing);

            // Act
            var state = _adapter.CurrentGameState;

            // Assert
            Assert.AreEqual(GameState.Playing, state);
        }

        [Test]
        public void Adapter_ProxiesPreviousGameState()
        {
            // Arrange
            _gameManagerService.ChangeGameState(GameState.Playing);
            _gameManagerService.ChangeGameState(GameState.Paused);

            // Act
            var previousState = _adapter.PreviousGameState;

            // Assert
            Assert.AreEqual(GameState.Playing, previousState);
        }

        [Test]
        public void Adapter_ProxiesGameTime()
        {
            // Arrange
            _gameManagerService.ChangeGameState(GameState.Playing);
            _gameManagerService.UpdateGameTime(5.5f);

            // Act
            var time = _adapter.GameTime;

            // Assert
            Assert.AreEqual(5.5f, time, 0.01f);
        }

        [Test]
        public void Adapter_ProxiesIsPaused()
        {
            // Arrange
            _gameManagerService.ChangeGameState(GameState.Playing);
            _gameManagerService.PauseGame();

            // Act
            var isPaused = _adapter.IsPaused;

            // Assert
            Assert.IsTrue(isPaused);
        }

        [Test]
        public void Adapter_ProxiesIsGameOver()
        {
            // Arrange
            _gameManagerService.ChangeGameState(GameState.Playing);
            _gameManagerService.TriggerGameOver();

            // Act
            var isGameOver = _adapter.IsGameOver;

            // Assert
            Assert.IsTrue(isGameOver);
        }

        #endregion

        #region Method Proxy Tests

        [UnityTest]
        public IEnumerator Adapter_ProxiesStartGame()
        {
            // Act
            _adapter.StartGame();
            yield return null;

            // Assert
            Assert.AreEqual(GameState.Loading, _gameManagerService.CurrentGameState);
            Assert.IsTrue(_mockEventManager.WasEventRaised("Game_Started"));
        }

        [UnityTest]
        public IEnumerator Adapter_ProxiesPauseGame()
        {
            // Arrange
            _gameManagerService.ChangeGameState(GameState.Playing);
            yield return null;

            // Act
            _adapter.PauseGame();

            // Assert
            Assert.AreEqual(GameState.Paused, _gameManagerService.CurrentGameState);
            Assert.IsTrue(_mockEventManager.WasEventRaised("Game_Paused"));
        }

        [UnityTest]
        public IEnumerator Adapter_ProxiesResumeGame()
        {
            // Arrange
            _gameManagerService.ChangeGameState(GameState.Playing);
            _gameManagerService.PauseGame();
            yield return null;

            // Act
            _adapter.ResumeGame();

            // Assert
            Assert.AreEqual(GameState.Playing, _gameManagerService.CurrentGameState);
            Assert.IsTrue(_mockEventManager.WasEventRaised("Game_Resumed"));
        }

        [UnityTest]
        public IEnumerator Adapter_ProxiesRestartGame()
        {
            // Act
            _adapter.RestartGame();
            yield return null;

            // Assert
            Assert.AreEqual(GameState.Loading, _gameManagerService.CurrentGameState);
            Assert.IsTrue(_mockEventManager.WasEventRaised("Game_Restarted"));
        }

        [UnityTest]
        public IEnumerator Adapter_ProxiesReturnToMenu()
        {
            // Act
            _adapter.ReturnToMenu();
            yield return null;

            // Assert
            Assert.AreEqual(GameState.Loading, _gameManagerService.CurrentGameState);
            Assert.IsTrue(_mockEventManager.WasEventRaised("Game_ReturnToMainMenu"));
        }

        [UnityTest]
        public IEnumerator Adapter_ProxiesTriggerGameOver()
        {
            // Arrange
            _gameManagerService.ChangeGameState(GameState.Playing);
            yield return null;

            // Act
            _adapter.TriggerGameOver();

            // Assert
            Assert.AreEqual(GameState.GameOver, _gameManagerService.CurrentGameState);
            Assert.IsTrue(_mockEventManager.WasEventRaised("Game_Over"));
        }

        [UnityTest]
        public IEnumerator Adapter_ProxiesTriggerVictory()
        {
            // Arrange
            _gameManagerService.ChangeGameState(GameState.Playing);
            yield return null;

            // Act
            _adapter.TriggerVictory();

            // Assert
            Assert.AreEqual(GameState.Victory, _gameManagerService.CurrentGameState);
            Assert.IsTrue(_mockEventManager.WasEventRaised("Game_Victory"));
        }

        #endregion

        #region Command Proxy Tests

        [Test]
        public void Adapter_ProxiesExecuteCommand()
        {
            // Arrange
            var mockCommand = new MockCommand();

            // Act
            _adapter.ExecuteCommand(mockCommand);

            // Assert
            Assert.IsTrue(mockCommand.WasExecuted);
            Assert.IsTrue(_mockEventManager.WasEventRaised("Game_CommandExecuted"));
        }

        [Test]
        public void Adapter_ProxiesUndoLastCommand()
        {
            // Arrange
            var mockCommand = new MockUndoableCommand();
            _adapter.ExecuteCommand(mockCommand);

            // Act
            _adapter.UndoLastCommand();

            // Assert
            Assert.IsTrue(mockCommand.WasUndone);
            Assert.IsTrue(_mockEventManager.WasEventRaised("Game_CommandUndone"));
        }

        [Test]
        public void Adapter_ProxiesRedoLastCommand()
        {
            // Arrange
            var mockCommand = new MockUndoableCommand();
            _adapter.ExecuteCommand(mockCommand);
            _adapter.UndoLastCommand();

            // Act
            _adapter.RedoLastCommand();

            // Assert
            Assert.AreEqual(2, mockCommand.ExecuteCount);
            Assert.IsTrue(_mockEventManager.WasEventRaised("Game_CommandRedone"));
        }

        #endregion

        #region Service Not Available Tests

        [UnityTest]
        public IEnumerator Adapter_HandlesNullGameManagerService()
        {
            // Arrange - Clear GameManagerService
            ServiceLocator.Unregister<IGameManager>();
            yield return null;

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() =>
            {
                var state = _adapter.CurrentGameState;
                Assert.AreEqual(GameState.MainMenu, state); // Returns default

                _adapter.StartGame(); // Should do nothing
                _adapter.PauseGame(); // Should do nothing
            });
        }

        [UnityTest]
        public IEnumerator Adapter_HandlesNullEventManager()
        {
            // Arrange - Clear EventManager
            ServiceLocator.Unregister<IEventManager>();
            yield return null;

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() =>
            {
                _adapter.StartGame();
            });
        }

        #endregion

        #region Update Loop Tests

        [UnityTest]
        public IEnumerator Adapter_UpdatesGameTime_InUpdateLoop()
        {
            // Arrange
            _gameManagerService.ChangeGameState(GameState.Playing);

            // Act - Simulate Update loop
            float deltaTime = 0.016f; // ~60 FPS
            for (int i = 0; i < 60; i++) // 1 second worth of updates
            {
                _gameManagerService.UpdateGameTime(deltaTime);
            }
            yield return null;

            // Assert
            Assert.AreEqual(0.96f, _adapter.GameTime, 0.01f); // 60 * 0.016 = 0.96
        }

        #endregion
    }

    #region Mock Classes

    public class MockEventManager : IEventManager
    {
        public string ServiceName => "MockEventManager";
        public bool IsServiceActive => true;
        private System.Collections.Generic.List<string> _raisedEvents = new System.Collections.Generic.List<string>();

        public void OnServiceRegistered() { }
        public void OnServiceUnregistered() { }

        public void RaiseEvent(string eventName, object data = null)
        {
            _raisedEvents.Add(eventName);
        }

        public void Subscribe<T>(string eventName, System.Action<T> handler) { }
        public void Unsubscribe<T>(string eventName, System.Action<T> handler) { }
        public void Subscribe(string eventName, System.Action<object> handler) { }
        public void Unsubscribe(string eventName, System.Action<object> handler) { }
        public void UnsubscribeAll(string eventName) { }
        public void Clear() { _raisedEvents.Clear(); }

        public bool WasEventRaised(string eventName) => _raisedEvents.Contains(eventName);
    }

    public class MockCommand : ICommand
    {
        public bool WasExecuted { get; private set; }
        public bool CanExecute() => true;
        public void Execute() => WasExecuted = true;
    }

    public class MockUndoableCommand : ICommand, IUndoableCommand
    {
        public bool WasExecuted { get; private set; }
        public bool WasUndone { get; private set; }
        public int ExecuteCount { get; private set; }

        public bool CanExecute() => true;
        public void Execute()
        {
            WasExecuted = true;
            ExecuteCount++;
        }

        public bool CanUndo() => true;
        public void Undo() => WasUndone = true;
    }

    #endregion
}