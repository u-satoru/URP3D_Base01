using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Core.Services.Interfaces;
using asterivo.Unity60.Core.Services.Implementations;
using asterivo.Unity60.Core.Types;
using asterivo.Unity60.Core.Events;

namespace asterivo.Unity60.Tests.Features.GameManagement
{
    /// <summary>
    /// GameStateManagerServiceの統合テスト
    /// MonoBehaviourとしての動作とServiceLocator統合を検証
    /// </summary>
    [TestFixture]
    public class GameStateManagerIntegrationTests
    {
        private GameObject _testObject;
        private GameStateManagerService _stateManager;
        private TestEventManager _eventManager;

        [SetUp]
        public void Setup()
        {
            // ServiceLocatorをクリア
            ServiceLocator.Clear();

            // EventManagerを登録
            _eventManager = new TestEventManager();
            ServiceLocator.Register<IEventManager>(_eventManager);

            // GameStateManagerServiceを持つGameObjectを作成
            _testObject = new GameObject("TestStateManager");
            _stateManager = _testObject.AddComponent<GameStateManagerService>();
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

        #region Service Registration Tests

        [UnityTest]
        public IEnumerator GameStateManager_RegistersWithServiceLocator()
        {
            // Act
            _stateManager.RegisterServices();
            yield return null;

            // Assert
            Assert.IsTrue(ServiceLocator.TryGet<IGameStateManager>(out var service));
            Assert.IsNotNull(service);
            Assert.AreSame(_stateManager, service);
        }

        [UnityTest]
        public IEnumerator GameStateManager_UnregistersFromServiceLocator()
        {
            // Arrange
            _stateManager.RegisterServices();
            yield return null;

            // Act
            _stateManager.UnregisterServices();

            // Assert
            Assert.IsFalse(ServiceLocator.TryGet<IGameStateManager>(out var service));
        }

        [Test]
        public void GameStateManager_ImplementsIService()
        {
            // Assert
            Assert.IsTrue(_stateManager is IService);
            Assert.AreEqual("GameStateManagerService", _stateManager.ServiceName);
        }

        [Test]
        public void OnServiceRegistered_InitializesCorrectly()
        {
            // Act
            _stateManager.OnServiceRegistered();

            // Assert
            Assert.IsTrue(_stateManager.IsServiceActive);
            Assert.AreEqual(GameState.MainMenu, _stateManager.CurrentGameState);
            Assert.AreEqual(GameState.MainMenu, _stateManager.PreviousGameState);
        }

        [Test]
        public void OnServiceUnregistered_DeactivatesService()
        {
            // Arrange
            _stateManager.OnServiceRegistered();

            // Act
            _stateManager.OnServiceUnregistered();

            // Assert
            Assert.IsFalse(_stateManager.IsServiceActive);
        }

        #endregion

        #region Game State Management Tests

        [Test]
        public void ChangeGameState_UpdatesStates()
        {
            // Arrange
            _stateManager.OnServiceRegistered();
            var initialState = _stateManager.CurrentGameState;

            // Act
            _stateManager.ChangeGameState(GameState.Playing);

            // Assert
            Assert.AreEqual(GameState.Playing, _stateManager.CurrentGameState);
            Assert.AreEqual(initialState, _stateManager.PreviousGameState);
        }

        [Test]
        public void ChangeGameState_DoesNotUpdate_WhenSameState()
        {
            // Arrange
            _stateManager.OnServiceRegistered();
            _stateManager.ChangeGameState(GameState.Playing);
            var previousState = _stateManager.PreviousGameState;

            // Act
            _stateManager.ChangeGameState(GameState.Playing);

            // Assert
            Assert.AreEqual(previousState, _stateManager.PreviousGameState);
        }

        [Test]
        public void ChangeGameState_RaisesEvent_ThroughEventManager()
        {
            // Arrange
            _stateManager.OnServiceRegistered();

            // Act
            _stateManager.ChangeGameState(GameState.Playing);

            // Assert
            Assert.IsTrue(_eventManager.WasEventRaised("GameStateChanged"));

            var eventData = _eventManager.GetLastEventData<GameStateChangeEventData>("GameStateChanged");
            Assert.AreEqual(GameState.MainMenu, eventData.PreviousState);
            Assert.AreEqual(GameState.Playing, eventData.NewState);
        }

        [UnityTest]
        public IEnumerator GameStateManager_WorksWithScriptableObjectEvent()
        {
            // Arrange
            var scriptableEvent = ScriptableObject.CreateInstance<GameEvent<GameState>>();
            var eventReceived = false;
            GameState receivedState = GameState.MainMenu;

            // リフレクションでprivateフィールドを設定
            var fieldInfo = typeof(GameStateManagerService).GetField("gameStateChangedEvent",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (fieldInfo != null)
            {
                fieldInfo.SetValue(_stateManager, scriptableEvent);
            }

            // イベントリスナーを設定
            var listener = new TestGameStateListener();
            listener.OnEventRaised = (state) =>
            {
                eventReceived = true;
                receivedState = state;
            };
            scriptableEvent.RegisterListener(listener);

            _stateManager.OnServiceRegistered();

            // Act
            _stateManager.ChangeGameState(GameState.Playing);
            yield return null;

            // Assert
            Assert.IsTrue(eventReceived);
            Assert.AreEqual(GameState.Playing, receivedState);

            // Cleanup
            scriptableEvent.UnregisterListener(listener);
            Object.DestroyImmediate(scriptableEvent);
        }

        #endregion

        #region State Properties Tests

        [Test]
        public void IsGameOver_AlwaysReturnsFalse()
        {
            // GameStateManagerServiceはゲームオーバー判定をScoreServiceに委譲
            Assert.IsFalse(_stateManager.IsGameOver);
        }

        [Test]
        public void Priority_ReturnsCorrectValue()
        {
            // デフォルト値を確認
            Assert.AreEqual(50, _stateManager.Priority);
        }

        #endregion

        #region State Transition Tests

        [Test]
        public void StateTransitions_WorkCorrectly()
        {
            // Arrange
            _stateManager.OnServiceRegistered();

            // MainMenu -> Loading
            _stateManager.ChangeGameState(GameState.Loading);
            Assert.AreEqual(GameState.Loading, _stateManager.CurrentGameState);

            // Loading -> Playing
            _stateManager.ChangeGameState(GameState.Playing);
            Assert.AreEqual(GameState.Playing, _stateManager.CurrentGameState);

            // Playing -> Paused
            _stateManager.ChangeGameState(GameState.Paused);
            Assert.AreEqual(GameState.Paused, _stateManager.CurrentGameState);

            // Paused -> Playing
            _stateManager.ChangeGameState(GameState.Playing);
            Assert.AreEqual(GameState.Playing, _stateManager.CurrentGameState);

            // Playing -> GameOver
            _stateManager.ChangeGameState(GameState.GameOver);
            Assert.AreEqual(GameState.GameOver, _stateManager.CurrentGameState);
        }

        [Test]
        public void StateAliases_WorkCorrectly()
        {
            // Arrange
            _stateManager.OnServiceRegistered();

            // Test Playing = Gameplay alias
            _stateManager.ChangeGameState(GameState.Playing);
            Assert.AreEqual(GameState.Gameplay, _stateManager.CurrentGameState);

            // Test InGame = Gameplay alias
            _stateManager.ChangeGameState(GameState.InGame);
            Assert.AreEqual(GameState.Gameplay, _stateManager.CurrentGameState);
        }

        #endregion
    }

    #region Test Helper Classes

    public class TestEventManager : IEventManager
    {
        private System.Collections.Generic.Dictionary<string, object> _lastEventData =
            new System.Collections.Generic.Dictionary<string, object>();
        private System.Collections.Generic.HashSet<string> _raisedEvents =
            new System.Collections.Generic.HashSet<string>();

        public string ServiceName => "TestEventManager";
        public bool IsServiceActive => true;

        public void OnServiceRegistered() { }
        public void OnServiceUnregistered() { }

        public void RaiseEvent(string eventName, object data = null)
        {
            _raisedEvents.Add(eventName);
            _lastEventData[eventName] = data;
        }

        public void Subscribe<T>(string eventName, System.Action<T> handler) { }
        public void Unsubscribe<T>(string eventName, System.Action<T> handler) { }
        public void Subscribe(string eventName, System.Action<object> handler) { }
        public void Unsubscribe(string eventName, System.Action<object> handler) { }
        public void UnsubscribeAll(string eventName) { }
        public void Clear()
        {
            _raisedEvents.Clear();
            _lastEventData.Clear();
        }

        public bool WasEventRaised(string eventName) => _raisedEvents.Contains(eventName);

        public T GetLastEventData<T>(string eventName)
        {
            if (_lastEventData.TryGetValue(eventName, out var data))
                return (T)data;
            return default(T);
        }
    }

    public class TestGameStateListener : IGameEventListener<GameState>
    {
        public System.Action<GameState> OnEventRaised { get; set; }

        void IGameEventListener<GameState>.OnEventRaised(GameState value)
        {
            OnEventRaised?.Invoke(value);
        }
    }

    #endregion
}
