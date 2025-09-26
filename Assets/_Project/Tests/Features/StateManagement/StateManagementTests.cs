using NUnit.Framework;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Patterns;
using asterivo.Unity60.Features.StateManagement;
using asterivo.Unity60.Features.Player;

namespace asterivo.Unity60.Tests.Features.StateManagement
{
    /// <summary>
    /// StateManagement讖溯・縺ｮ繝ｦ繝九ャ繝医ユ繧ｹ繝・
    /// </summary>
    [TestFixture]
    public class StateManagementTests
    {
        private IStateService stateService;

        [SetUp]
        public void Setup()
        {
            // ServiceLocator繧偵け繝ｪ繧｢
            ServiceLocator.Clear();

            // StateManagement繧貞・譛溷喧
            StateManagementBootstrapper.Initialize();

            // StateService繧貞叙蠕・
            stateService = ServiceLocator.Get<IStateService>();
        }

        [TearDown]
        public void TearDown()
        {
            // StateManagement繧偵す繝｣繝・ヨ繝繧ｦ繝ｳ
            StateManagementBootstrapper.Shutdown();

            // ServiceLocator繧偵け繝ｪ繧｢
            ServiceLocator.Clear();
        }

        [Test]
        public void StateService_Should_Be_Registered_In_ServiceLocator()
        {
            // Assert
            Assert.IsNotNull(stateService, "StateService should be registered in ServiceLocator");
            Assert.IsInstanceOf<IStateService>(stateService, "Service should implement IStateService");
        }

        [Test]
        public void All_PlayerState_Handlers_Should_Be_Registered()
        {
            // Arrange
            var expectedStates = new[]
            {
                PlayerState.Idle,
                PlayerState.Walking,
                PlayerState.Running,
                PlayerState.Sprinting,
                PlayerState.Jumping,
                PlayerState.Falling,
                PlayerState.Landing,
                PlayerState.Combat,
                PlayerState.CombatAttacking,
                PlayerState.Interacting,
                PlayerState.Dead
            };

            // Act & Assert
            foreach (var state in expectedStates)
            {
                var hasHandler = stateService.HasHandler((int)state);
                Assert.IsTrue(hasHandler, $"Handler for {state} should be registered");
            }
        }

        [Test]
        public void GetHandler_Should_Return_Correct_Handler_Type()
        {
            // Act
            var idleHandler = stateService.GetHandler((int)PlayerState.Idle);
            var walkingHandler = stateService.GetHandler((int)PlayerState.Walking);
            var combatHandler = stateService.GetHandler((int)PlayerState.Combat);

            // Assert
            Assert.IsInstanceOf<IdleStateHandler>(idleHandler, "Should return IdleStateHandler");
            Assert.IsInstanceOf<WalkingStateHandler>(walkingHandler, "Should return WalkingStateHandler");
            Assert.IsInstanceOf<CombatStateHandler>(combatHandler, "Should return CombatStateHandler");
        }

        [Test]
        public void GetHandler_Should_Return_Null_For_Unregistered_State()
        {
            // Arrange
            const int unregisteredState = 999;

            // Act
            var handler = stateService.GetHandler(unregisteredState);

            // Assert
            Assert.IsNull(handler, "Should return null for unregistered state");
        }

        [Test]
        public void ClearHandlers_Should_Remove_All_Handlers()
        {
            // Arrange
            Assert.IsTrue(stateService.HasHandler((int)PlayerState.Idle), "Pre-condition: Handler should exist");

            // Act
            stateService.ClearHandlers();

            // Assert
            Assert.IsFalse(stateService.HasHandler((int)PlayerState.Idle), "Handler should be removed");
            var registeredStates = stateService.GetRegisteredStates();
            Assert.IsEmpty(registeredStates, "No handlers should be registered after clear");
        }

        [Test]
        public void StateHandler_OnEnter_Should_Be_Called()
        {
            // Arrange
            var handler = stateService.GetHandler((int)PlayerState.Idle);
            var mockContext = new TestStateContext();

            // Act
            handler.OnEnter(mockContext);

            // Assert
            Assert.IsTrue(mockContext.LogMessages.Contains("Entering Idle state"),
                "OnEnter should log entering message");
        }

        [Test]
        public void StateHandler_OnExit_Should_Be_Called()
        {
            // Arrange
            var handler = stateService.GetHandler((int)PlayerState.Walking);
            var mockContext = new TestStateContext();

            // Act
            handler.OnExit(mockContext);

            // Assert
            Assert.IsTrue(mockContext.LogMessages.Contains("Exiting Walking state"),
                "OnExit should log exiting message");
        }

        [Test]
        public void Three_Layer_Architecture_Should_Be_Maintained()
        {
            // Core螻､縺ｮ繧､繝ｳ繧ｿ繝ｼ繝輔ぉ繝ｼ繧ｹ繧帝壹§縺ｦ縺ｮ縺ｿ繧｢繧ｯ繧ｻ繧ｹ蜿ｯ閭ｽ
            Assert.IsInstanceOf<IStateService>(stateService, "Should access through Core interface");

            // StateHandlerRegistry縺ｯCore螻､縲ヾtateHandler縺ｯFeature螻､
            var handler = stateService.GetHandler((int)PlayerState.Idle);
            Assert.IsInstanceOf<IStateHandler>(handler, "Handler should implement Core interface");

            // PlayerState縺ｯFeature/Player螻､縺ｧ螳夂ｾｩ縺輔ｌ縺ｦ縺・ｋ
            Assert.AreEqual("asterivo.Unity60.Features.Player", typeof(PlayerState).Namespace,
                "PlayerState should be in Feature layer");
        }

        /// <summary>
        /// 繝・せ繝育畑縺ｮStateContext螳溯｣・
        /// </summary>
        private class TestStateContext : IStateContext
        {
            public bool IsDebugEnabled => true;
            public System.Collections.Generic.List<string> LogMessages { get; } = new();

            public void Log(string message)
            {
                LogMessages.Add(message);
            }
        }
    }
}


