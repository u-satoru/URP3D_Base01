using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Types;
using asterivo.Unity60.Features.GameManagement.Services;
using asterivo.Unity60.Features.GameManagement.Interfaces;
using asterivo.Unity60.Features.GameManagement.Events;

namespace asterivo.Unity60.Tests.Features.GameManagement
{
    /// <summary>
    /// GameManagement繧､繝吶Φ繝医す繧ｹ繝・Β縺ｮ蜍穂ｽ懊ユ繧ｹ繝・
    /// EventManager縺ｨGameManagerService縺ｮ騾｣謳ｺ繧呈､懆ｨｼ
    /// </summary>
    [TestFixture]
    public class GameManagementEventSystemTests
    {
        private EventManager _eventManager;
        private IGameManager _gameManager;
        private List<object> _receivedEvents;
        private Dictionary<string, int> _eventCounts;

        [SetUp]
        public void Setup()
        {
            // ServiceLocator繧偵け繝ｪ繧｢
            ServiceLocator.Clear();

            // EventManager繧堤匳骭ｲ
            _eventManager = new EventManager();
            ServiceLocator.Register<IEventManager>(_eventManager);

            // GameManagerService繧堤匳骭ｲ
            _gameManager = new GameManagerService();
            ServiceLocator.Register<IGameManager>(_gameManager);

            // 繧､繝吶Φ繝医Ξ繧ｷ繝ｼ繝舌・蛻晄悄蛹・
            _receivedEvents = new List<object>();
            _eventCounts = new Dictionary<string, int>();
        }

        [TearDown]
        public void TearDown()
        {
            _eventManager.Clear();
            ServiceLocator.Clear();
        }

        #region Event Subscription Tests

        [Test]
        public void EventManager_RaisesAndReceivesGameEvents()
        {
            // Arrange
            bool eventReceived = false;
            string testEventName = GameManagementEventNames.OnGameStarted;

            _eventManager.Subscribe<object>(testEventName, (data) =>
            {
                eventReceived = true;
            });

            // Act
            _eventManager.RaiseEvent(testEventName, null);

            // Assert
            Assert.IsTrue(eventReceived);
        }

        [Test]
        public void GameManager_RaisesGameStateChangedEvent()
        {
            // Arrange
            GameStateChangeData capturedData = default;
            _eventManager.Subscribe<GameStateChangeData>(
                GameManagementEventNames.OnGameStateChanged,
                (data) => capturedData = data
            );

            // Act
            _gameManager.ChangeGameState(GameState.Playing);

            // Assert
            Assert.AreEqual(GameState.MainMenu, capturedData.PreviousState);
            Assert.AreEqual(GameState.Playing, capturedData.NewState);
        }

        [Test]
        public void GameManager_RaisesGameStartedEvent()
        {
            // Arrange
            bool eventReceived = false;
            _eventManager.Subscribe<object>(
                GameManagementEventNames.OnGameStarted,
                (data) => eventReceived = true
            );

            // Act
            _gameManager.StartGame();

            // Assert
            Assert.IsTrue(eventReceived);
        }

        [Test]
        public void GameManager_RaisesGamePausedEvent()
        {
            // Arrange
            bool eventReceived = false;
            _gameManager.ChangeGameState(GameState.Playing);

            _eventManager.Subscribe<object>(
                GameManagementEventNames.OnGamePaused,
                (data) => eventReceived = true
            );

            // Act
            _gameManager.PauseGame();

            // Assert
            Assert.IsTrue(eventReceived);
        }

        [Test]
        public void GameManager_RaisesGameResumedEvent()
        {
            // Arrange
            bool eventReceived = false;
            _gameManager.ChangeGameState(GameState.Playing);
            _gameManager.PauseGame();

            _eventManager.Subscribe<object>(
                GameManagementEventNames.OnGameResumed,
                (data) => eventReceived = true
            );

            // Act
            _gameManager.ResumeGame();

            // Assert
            Assert.IsTrue(eventReceived);
        }

        [Test]
        public void GameManager_RaisesGameOverEvent()
        {
            // Arrange
            bool eventReceived = false;
            _gameManager.ChangeGameState(GameState.Playing);

            _eventManager.Subscribe<object>(
                GameManagementEventNames.OnGameOver,
                (data) => eventReceived = true
            );

            // Act
            _gameManager.TriggerGameOver();

            // Assert
            Assert.IsTrue(eventReceived);
        }

        [Test]
        public void GameManager_RaisesVictoryEvent()
        {
            // Arrange
            bool eventReceived = false;
            _gameManager.ChangeGameState(GameState.Playing);

            _eventManager.Subscribe<object>(
                GameManagementEventNames.OnGameVictory,
                (data) => eventReceived = true
            );

            // Act
            _gameManager.TriggerVictory();

            // Assert
            Assert.IsTrue(eventReceived);
        }

        #endregion

        #region Command Event Tests

        [Test]
        public void GameManager_RaisesCommandExecutedEvent()
        {
            // Arrange
            bool eventReceived = false;
            _eventManager.Subscribe<object>(
                GameManagementEventNames.OnCommandExecuted,
                (data) => eventReceived = true
            );

            var mockCommand = new TestCommand();

            // Act
            _gameManager.ExecuteCommand(mockCommand);

            // Assert
            Assert.IsTrue(eventReceived);
        }

        [Test]
        public void GameManager_RaisesCommandUndoneEvent()
        {
            // Arrange
            bool eventReceived = false;
            var mockCommand = new TestUndoableCommand();
            _gameManager.ExecuteCommand(mockCommand);

            _eventManager.Subscribe<object>(
                GameManagementEventNames.OnCommandUndone,
                (data) => eventReceived = true
            );

            // Act
            _gameManager.UndoLastCommand();

            // Assert
            Assert.IsTrue(eventReceived);
        }

        [Test]
        public void GameManager_RaisesCommandRedoneEvent()
        {
            // Arrange
            bool eventReceived = false;
            var mockCommand = new TestUndoableCommand();
            _gameManager.ExecuteCommand(mockCommand);
            _gameManager.UndoLastCommand();

            _eventManager.Subscribe<object>(
                GameManagementEventNames.OnCommandRedone,
                (data) => eventReceived = true
            );

            // Act
            _gameManager.RedoLastCommand();

            // Assert
            Assert.IsTrue(eventReceived);
        }

        #endregion

        #region Event Unsubscription Tests

        [Test]
        public void EventManager_UnsubscribeStopsReceivingEvents()
        {
            // Arrange
            int callCount = 0;
            System.Action<object> handler = (data) => callCount++;
            string eventName = GameManagementEventNames.OnGameStarted;

            _eventManager.Subscribe(eventName, handler);

            // Act
            _eventManager.RaiseEvent(eventName, null);
            Assert.AreEqual(1, callCount);

            _eventManager.Unsubscribe(eventName, handler);
            _eventManager.RaiseEvent(eventName, null);

            // Assert
            Assert.AreEqual(1, callCount); // Should still be 1
        }

        [Test]
        public void EventManager_UnsubscribeAll_ClearsAllHandlers()
        {
            // Arrange
            int callCount = 0;
            string eventName = GameManagementEventNames.OnGameStarted;

            _eventManager.Subscribe<object>(eventName, (data) => callCount++);
            _eventManager.Subscribe<object>(eventName, (data) => callCount++);
            _eventManager.Subscribe<object>(eventName, (data) => callCount++);

            // Act
            _eventManager.RaiseEvent(eventName, null);
            Assert.AreEqual(3, callCount);

            _eventManager.UnsubscribeAll(eventName);
            _eventManager.RaiseEvent(eventName, null);

            // Assert
            Assert.AreEqual(3, callCount); // Should still be 3
        }

        #endregion

        #region Game Time Event Tests

        [Test]
        public void GameManager_RaisesGameTimeUpdatedEvent()
        {
            // Arrange
            bool eventReceived = false;
            float receivedTime = 0f;

            _gameManager.ChangeGameState(GameState.Playing);

            _eventManager.Subscribe<float>(
                GameManagementEventNames.OnGameTimeUpdated,
                (time) =>
                {
                    eventReceived = true;
                    receivedTime = time;
                }
            );

            // Act - Update time to pass 1 second threshold
            _gameManager.UpdateGameTime(0.5f);
            _gameManager.UpdateGameTime(0.6f);

            // Assert
            Assert.IsTrue(eventReceived);
            Assert.AreEqual(1.1f, receivedTime, 0.01f);
        }

        #endregion

        #region Multiple Subscriber Tests

        [Test]
        public void MultipleSubscribers_AllReceiveEvents()
        {
            // Arrange
            int subscriber1Count = 0;
            int subscriber2Count = 0;
            int subscriber3Count = 0;
            string eventName = GameManagementEventNames.OnGameStarted;

            _eventManager.Subscribe<object>(eventName, (data) => subscriber1Count++);
            _eventManager.Subscribe<object>(eventName, (data) => subscriber2Count++);
            _eventManager.Subscribe<object>(eventName, (data) => subscriber3Count++);

            // Act
            _gameManager.StartGame();

            // Assert
            Assert.AreEqual(1, subscriber1Count);
            Assert.AreEqual(1, subscriber2Count);
            Assert.AreEqual(1, subscriber3Count);
        }

        [Test]
        public void TypedEvents_WorkCorrectly()
        {
            // Arrange
            GameStateChangeData capturedStateChange = default;
            float capturedTime = 0f;

            _eventManager.Subscribe<GameStateChangeData>(
                GameManagementEventNames.OnGameStateChanged,
                (data) => capturedStateChange = data
            );

            _eventManager.Subscribe<float>(
                GameManagementEventNames.OnGameTimeUpdated,
                (time) => capturedTime = time
            );

            // Act
            _gameManager.ChangeGameState(GameState.Playing);
            _gameManager.UpdateGameTime(1.5f);

            // Assert
            Assert.AreEqual(GameState.Playing, capturedStateChange.NewState);
            Assert.AreEqual(1.5f, capturedTime, 0.01f);
        }

        #endregion

        #region Event Chain Tests

        [Test]
        public void EventChain_GameStartToPlayingFlow()
        {
            // Arrange
            List<string> eventSequence = new List<string>();

            _eventManager.Subscribe<object>(GameManagementEventNames.OnGameStarted,
                (data) => eventSequence.Add("Started"));

            _eventManager.Subscribe<GameStateChangeData>(GameManagementEventNames.OnGameStateChanged,
                (data) => eventSequence.Add($"State:{data.NewState}"));

            // Act
            _gameManager.StartGame();

            // Assert
            Assert.Contains("State:Loading", eventSequence);
            Assert.Contains("Started", eventSequence);
        }

        [Test]
        public void EventChain_GameOverFlow()
        {
            // Arrange
            List<string> eventSequence = new List<string>();

            _gameManager.ChangeGameState(GameState.Playing);

            _eventManager.Subscribe<GameStateChangeData>(GameManagementEventNames.OnGameStateChanged,
                (data) => eventSequence.Add($"State:{data.NewState}"));

            _eventManager.Subscribe<object>(GameManagementEventNames.OnGameOver,
                (data) => eventSequence.Add("GameOver"));

            // Act
            _gameManager.TriggerGameOver();

            // Assert
            Assert.Contains("State:GameOver", eventSequence);
            Assert.Contains("GameOver", eventSequence);
        }

        #endregion
    }

    #region Test Helper Classes

    public class TestCommand : Core.Commands.ICommand
    {
        public bool CanExecute() => true;
        public void Execute() { }
    }

    public class TestUndoableCommand : Core.Commands.ICommand, Core.Commands.IUndoableCommand
    {
        public bool CanExecute() => true;
        public void Execute() { }
        public bool CanUndo() => true;
        public void Undo() { }
    }

    #endregion
}


