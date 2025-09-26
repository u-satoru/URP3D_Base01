using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Types;
using asterivo.Unity60.Core.Commands;
using asterivo.Unity60.Features.GameManagement.Services;
using asterivo.Unity60.Features.GameManagement.Interfaces;
using asterivo.Unity60.Features.GameManagement.Events;

namespace asterivo.Unity60.Tests.Features.GameManagement
{
    /// <summary>
    /// GameManagerService縺ｮ蜊倅ｽ薙ユ繧ｹ繝・
    /// ServiceLocator繝代ち繝ｼ繝ｳ縺ｨ繧､繝吶Φ繝磯ｧ・虚縺ｮ蜍穂ｽ懊ｒ讀懆ｨｼ
    /// </summary>
    [TestFixture]
    public class GameManagerServiceTests
    {
        private GameManagerService _gameManagerService;
        private MockEventManager _mockEventManager;

        [SetUp]
        public void Setup()
        {
            // ServiceLocator繧偵け繝ｪ繧｢
            ServiceLocator.Clear();

            // MockEventManager繧堤匳骭ｲ
            _mockEventManager = new MockEventManager();
            ServiceLocator.Register<IEventManager>(_mockEventManager);

            // GameManagerService繧剃ｽ懈・縺励※逋ｻ骭ｲ
            _gameManagerService = new GameManagerService();
            ServiceLocator.Register<IGameManager>(_gameManagerService);
        }

        [TearDown]
        public void TearDown()
        {
            ServiceLocator.Clear();
        }

        #region Initialization Tests

        [Test]
        public void GameManagerService_Initializes_WithCorrectDefaults()
        {
            // Assert
            Assert.IsNotNull(_gameManagerService);
            Assert.AreEqual("GameManagerService", _gameManagerService.ServiceName);
            Assert.IsTrue(_gameManagerService.IsServiceActive);
            Assert.AreEqual(GameState.MainMenu, _gameManagerService.CurrentGameState);
            Assert.AreEqual(GameState.MainMenu, _gameManagerService.PreviousGameState);
            Assert.AreEqual(0f, _gameManagerService.GameTime);
            Assert.IsFalse(_gameManagerService.IsPaused);
            Assert.IsFalse(_gameManagerService.IsGameOver);
        }

        [Test]
        public void ServiceName_Returns_CorrectName()
        {
            // Assert
            Assert.AreEqual("GameManagerService", _gameManagerService.ServiceName);
        }

        #endregion

        #region Game State Management Tests

        [Test]
        public void ChangeGameState_Updates_CurrentAndPreviousState()
        {
            // Arrange
            var initialState = _gameManagerService.CurrentGameState;

            // Act
            _gameManagerService.ChangeGameState(GameState.Playing);

            // Assert
            Assert.AreEqual(GameState.Playing, _gameManagerService.CurrentGameState);
            Assert.AreEqual(initialState, _gameManagerService.PreviousGameState);
        }

        [Test]
        public void ChangeGameState_DoesNotUpdate_WhenSameState()
        {
            // Arrange
            _gameManagerService.ChangeGameState(GameState.Playing);
            var eventCountBefore = _mockEventManager.RaisedEvents.Count;

            // Act
            _gameManagerService.ChangeGameState(GameState.Playing);

            // Assert
            Assert.AreEqual(eventCountBefore, _mockEventManager.RaisedEvents.Count);
        }

        [Test]
        public void ChangeGameState_RaisesEvent_OnStateChange()
        {
            // Act
            _gameManagerService.ChangeGameState(GameState.Playing);

            // Assert
            Assert.IsTrue(_mockEventManager.WasEventRaised(GameManagementEventNames.OnGameStateChanged));
        }

        [Test]
        public void StartGame_ChangesState_ToLoading()
        {
            // Act
            _gameManagerService.StartGame();

            // Assert
            Assert.AreEqual(GameState.Loading, _gameManagerService.CurrentGameState);
            Assert.IsTrue(_mockEventManager.WasEventRaised(GameManagementEventNames.OnGameStarted));
        }

        [Test]
        public void StartGame_ResetsGameTime_AndGameOverFlag()
        {
            // Arrange
            _gameManagerService.UpdateGameTime(100f);
            _gameManagerService.TriggerGameOver();

            // Act
            _gameManagerService.StartGame();

            // Assert
            Assert.AreEqual(0f, _gameManagerService.GameTime);
            Assert.IsFalse(_gameManagerService.IsGameOver);
        }

        [Test]
        public void PauseGame_OnlyWorks_WhenPlaying()
        {
            // Arrange
            _gameManagerService.ChangeGameState(GameState.Playing);

            // Act
            _gameManagerService.PauseGame();

            // Assert
            Assert.AreEqual(GameState.Paused, _gameManagerService.CurrentGameState);
            Assert.IsTrue(_gameManagerService.IsPaused);
            Assert.IsTrue(_mockEventManager.WasEventRaised(GameManagementEventNames.OnGamePaused));
        }

        [Test]
        public void PauseGame_DoesNothing_WhenNotPlaying()
        {
            // Arrange
            _gameManagerService.ChangeGameState(GameState.MainMenu);

            // Act
            _gameManagerService.PauseGame();

            // Assert
            Assert.AreEqual(GameState.MainMenu, _gameManagerService.CurrentGameState);
            Assert.IsFalse(_gameManagerService.IsPaused);
        }

        [Test]
        public void ResumeGame_OnlyWorks_WhenPaused()
        {
            // Arrange
            _gameManagerService.ChangeGameState(GameState.Playing);
            _gameManagerService.PauseGame();

            // Act
            _gameManagerService.ResumeGame();

            // Assert
            Assert.AreEqual(GameState.Playing, _gameManagerService.CurrentGameState);
            Assert.IsFalse(_gameManagerService.IsPaused);
            Assert.IsTrue(_mockEventManager.WasEventRaised(GameManagementEventNames.OnGameResumed));
        }

        [Test]
        public void TriggerGameOver_SetsGameOverFlag()
        {
            // Arrange
            _gameManagerService.ChangeGameState(GameState.Playing);

            // Act
            _gameManagerService.TriggerGameOver();

            // Assert
            Assert.AreEqual(GameState.GameOver, _gameManagerService.CurrentGameState);
            Assert.IsTrue(_gameManagerService.IsGameOver);
            Assert.IsTrue(_mockEventManager.WasEventRaised(GameManagementEventNames.OnGameOver));
        }

        [Test]
        public void TriggerVictory_ChangesState_ToVictory()
        {
            // Arrange
            _gameManagerService.ChangeGameState(GameState.Playing);

            // Act
            _gameManagerService.TriggerVictory();

            // Assert
            Assert.AreEqual(GameState.Victory, _gameManagerService.CurrentGameState);
            Assert.IsTrue(_mockEventManager.WasEventRaised(GameManagementEventNames.OnGameVictory));
        }

        [Test]
        public void TogglePause_Toggles_BetweenPlayingAndPaused()
        {
            // Arrange
            _gameManagerService.ChangeGameState(GameState.Playing);

            // Act - Pause
            _gameManagerService.TogglePause();
            Assert.AreEqual(GameState.Paused, _gameManagerService.CurrentGameState);

            // Act - Resume
            _gameManagerService.TogglePause();
            Assert.AreEqual(GameState.Playing, _gameManagerService.CurrentGameState);
        }

        #endregion

        #region Command System Tests

        [Test]
        public void ExecuteCommand_Executes_ValidCommand()
        {
            // Arrange
            var mockCommand = new MockCommand { CanExecuteResult = true };

            // Act
            _gameManagerService.ExecuteCommand(mockCommand);

            // Assert
            Assert.IsTrue(mockCommand.WasExecuted);
            Assert.IsTrue(_mockEventManager.WasEventRaised(GameManagementEventNames.OnCommandExecuted));
        }

        [Test]
        public void ExecuteCommand_DoesNotExecute_InvalidCommand()
        {
            // Arrange
            var mockCommand = new MockCommand { CanExecuteResult = false };

            // Act
            _gameManagerService.ExecuteCommand(mockCommand);

            // Assert
            Assert.IsFalse(mockCommand.WasExecuted);
        }

        [Test]
        public void ExecuteCommand_AddsToUndoStack()
        {
            // Arrange
            var mockCommand = new MockUndoableCommand { CanExecuteResult = true };

            // Act
            _gameManagerService.ExecuteCommand(mockCommand);
            _gameManagerService.UndoLastCommand();

            // Assert
            Assert.IsTrue(mockCommand.WasUndone);
            Assert.IsTrue(_mockEventManager.WasEventRaised(GameManagementEventNames.OnCommandUndone));
        }

        [Test]
        public void UndoLastCommand_Works_WithUndoableCommand()
        {
            // Arrange
            var mockCommand = new MockUndoableCommand { CanExecuteResult = true, CanUndoResult = true };
            _gameManagerService.ExecuteCommand(mockCommand);

            // Act
            _gameManagerService.UndoLastCommand();

            // Assert
            Assert.IsTrue(mockCommand.WasUndone);
        }

        [Test]
        public void RedoLastCommand_Works_AfterUndo()
        {
            // Arrange
            var mockCommand = new MockUndoableCommand { CanExecuteResult = true, CanUndoResult = true };
            _gameManagerService.ExecuteCommand(mockCommand);
            _gameManagerService.UndoLastCommand();

            // Act
            _gameManagerService.RedoLastCommand();

            // Assert
            Assert.AreEqual(2, mockCommand.ExecuteCount); // 蛻晏屓螳溯｡・+ Redo
            Assert.IsTrue(_mockEventManager.WasEventRaised(GameManagementEventNames.OnCommandRedone));
        }

        #endregion

        #region Game Time Tests

        [Test]
        public void UpdateGameTime_Updates_WhenPlaying()
        {
            // Arrange
            _gameManagerService.ChangeGameState(GameState.Playing);

            // Act
            _gameManagerService.UpdateGameTime(1.5f);

            // Assert
            Assert.AreEqual(1.5f, _gameManagerService.GameTime);
        }

        [Test]
        public void UpdateGameTime_DoesNotUpdate_WhenPaused()
        {
            // Arrange
            _gameManagerService.ChangeGameState(GameState.Playing);
            _gameManagerService.PauseGame();

            // Act
            _gameManagerService.UpdateGameTime(1.5f);

            // Assert
            Assert.AreEqual(0f, _gameManagerService.GameTime);
        }

        [Test]
        public void UpdateGameTime_RaisesEvent_OnWholeSecond()
        {
            // Arrange
            _gameManagerService.ChangeGameState(GameState.Playing);

            // Act
            _gameManagerService.UpdateGameTime(0.5f);
            _gameManagerService.UpdateGameTime(0.6f); // 邱剰ｨ・.1遘・

            // Assert
            Assert.IsTrue(_mockEventManager.WasEventRaised(GameManagementEventNames.OnGameTimeUpdated));
        }

        #endregion

        #region Service Lifecycle Tests

        [Test]
        public void OnServiceUnregistered_Deactivates_Service()
        {
            // Act
            _gameManagerService.OnServiceUnregistered();

            // Assert
            Assert.IsFalse(_gameManagerService.IsServiceActive);
            Assert.IsTrue(_mockEventManager.WasEventRaised(GameManagementEventNames.OnGameManagerShutdown));
        }

        [Test]
        public void RestartGame_Resets_GameState()
        {
            // Arrange
            _gameManagerService.ChangeGameState(GameState.Playing);
            _gameManagerService.UpdateGameTime(100f);
            _gameManagerService.ExecuteCommand(new MockCommand());

            // Act
            _gameManagerService.RestartGame();

            // Assert
            Assert.AreEqual(GameState.Loading, _gameManagerService.CurrentGameState);
            Assert.AreEqual(0f, _gameManagerService.GameTime);
            Assert.IsFalse(_gameManagerService.IsGameOver);
            Assert.IsTrue(_mockEventManager.WasEventRaised(GameManagementEventNames.OnGameRestarted));
        }

        [Test]
        public void ReturnToMenu_ChangesState_ToLoading()
        {
            // Arrange
            _gameManagerService.ChangeGameState(GameState.Playing);

            // Act
            _gameManagerService.ReturnToMenu();

            // Assert
            Assert.AreEqual(GameState.Loading, _gameManagerService.CurrentGameState);
            Assert.AreEqual(0f, _gameManagerService.GameTime);
            Assert.IsTrue(_mockEventManager.WasEventRaised(GameManagementEventNames.OnReturnToMainMenu));
        }

        #endregion
    }

    #region Mock Classes

    public class MockEventManager : IEventManager
    {
        public string ServiceName => "MockEventManager";
        public bool IsServiceActive => true;
        public System.Collections.Generic.List<string> RaisedEvents { get; } = new System.Collections.Generic.List<string>();

        public void OnServiceRegistered() { }
        public void OnServiceUnregistered() { }

        public void RaiseEvent(string eventName, object data = null)
        {
            RaisedEvents.Add(eventName);
        }

        public void Subscribe<T>(string eventName, System.Action<T> handler) { }
        public void Unsubscribe<T>(string eventName, System.Action<T> handler) { }
        public void Subscribe(string eventName, System.Action<object> handler) { }
        public void Unsubscribe(string eventName, System.Action<object> handler) { }
        public void UnsubscribeAll(string eventName) { }
        public void Clear() { }

        public bool WasEventRaised(string eventName) => RaisedEvents.Contains(eventName);
    }

    public class MockCommand : ICommand
    {
        public bool CanExecuteResult { get; set; } = true;
        public bool WasExecuted { get; private set; }
        public int ExecuteCount { get; private set; }

        public bool CanExecute() => CanExecuteResult;
        public void Execute()
        {
            WasExecuted = true;
            ExecuteCount++;
        }
    }

    public class MockUndoableCommand : MockCommand, IUndoableCommand
    {
        public bool CanUndoResult { get; set; } = true;
        public bool WasUndone { get; private set; }

        public bool CanUndo() => CanUndoResult;
        public void Undo() => WasUndone = true;
    }

    #endregion
}


