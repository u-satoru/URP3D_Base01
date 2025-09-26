using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using asterivo.Unity60.Core;
using asterivo.Unity60.Features.StateManagement;
using asterivo.Unity60.Features.Player;

namespace asterivo.Unity60.Tests.Features.StateManagement
{
    /// <summary>
    /// StateManagement讖溯・縺ｮ邨ｱ蜷医ユ繧ｹ繝・
    /// 螳滄圀縺ｮ繧ｲ繝ｼ繝繧ｷ繝翫Μ繧ｪ縺ｧ縺ｮ蜍穂ｽ懊ｒ讀懆ｨｼ
    /// </summary>
    [TestFixture]
    public class StateManagementIntegrationTest
    {
        private GameObject testObject;
        private StateManager stateManager;

        [SetUp]
        public void Setup()
        {
            // ServiceLocator繧偵け繝ｪ繧｢
            ServiceLocator.Clear();

            // StateManagement繧貞・譛溷喧
            StateManagementBootstrapper.Initialize();

            // 繝・せ繝育畑GameObject繧剃ｽ懈・
            testObject = new GameObject("TestStateManager");
            stateManager = testObject.AddComponent<StateManager>();
        }

        [TearDown]
        public void TearDown()
        {
            // 繝・せ繝医が繝悶ず繧ｧ繧ｯ繝医ｒ遐ｴ譽・
            if (testObject != null)
            {
                Object.DestroyImmediate(testObject);
            }

            // StateManagement繧偵す繝｣繝・ヨ繝繧ｦ繝ｳ
            StateManagementBootstrapper.Shutdown();

            // ServiceLocator繧偵け繝ｪ繧｢
            ServiceLocator.Clear();
        }

        [UnityTest]
        public IEnumerator StateManager_Should_Initialize_With_Idle_State()
        {
            // Start()縺悟ｮ溯｡後＆繧後ｋ縺ｮ繧貞ｾ・▽
            yield return null;

            // Assert
            Assert.AreEqual(PlayerState.Idle, stateManager.CurrentState,
                "StateManager should start with Idle state");
        }

        [UnityTest]
        public IEnumerator StateManager_Should_Transition_Between_States()
        {
            // Start()縺悟ｮ溯｡後＆繧後ｋ縺ｮ繧貞ｾ・▽
            yield return null;

            // Act - Idle 竊・Walking
            stateManager.ChangeState(PlayerState.Walking);
            yield return null;

            // Assert
            Assert.AreEqual(PlayerState.Walking, stateManager.CurrentState,
                "Should transition to Walking state");

            // Act - Walking 竊・Running
            stateManager.ChangeState(PlayerState.Running);
            yield return null;

            // Assert
            Assert.AreEqual(PlayerState.Running, stateManager.CurrentState,
                "Should transition to Running state");

            // Act - Running 竊・Combat
            stateManager.ChangeState(PlayerState.Combat);
            yield return null;

            // Assert
            Assert.AreEqual(PlayerState.Combat, stateManager.CurrentState,
                "Should transition to Combat state");
        }

        [UnityTest]
        public IEnumerator StateManager_Should_Maintain_State_History()
        {
            // Start()縺悟ｮ溯｡後＆繧後ｋ縺ｮ繧貞ｾ・▽
            yield return null;

            // Act - 隍・焚縺ｮ迥ｶ諷矩・遘ｻ繧貞ｮ溯｡・
            stateManager.ChangeState(PlayerState.Walking);
            yield return null;

            stateManager.ChangeState(PlayerState.Running);
            yield return null;

            stateManager.ChangeState(PlayerState.Jumping);
            yield return null;

            // Assert - 螻･豁ｴ繧呈､懆ｨｼ
            var history = stateManager.StateHistory;
            Assert.IsNotNull(history, "State history should not be null");
            Assert.Greater(history.Count, 0, "State history should have entries");

            // 螻･豁ｴ縺ｫ譛溷ｾ・＆繧後ｋ迥ｶ諷九′蜷ｫ縺ｾ繧後※縺・ｋ縺狗｢ｺ隱・
            Assert.Contains(PlayerState.Idle, history as System.Collections.ICollection,
                "History should contain Idle state");
            Assert.Contains(PlayerState.Walking, history as System.Collections.ICollection,
                "History should contain Walking state");
        }

        [UnityTest]
        public IEnumerator StateManager_Should_Revert_To_Previous_State()
        {
            // Start()縺悟ｮ溯｡後＆繧後ｋ縺ｮ繧貞ｾ・▽
            yield return null;

            // Act - 迥ｶ諷矩・遘ｻ繧貞ｮ溯｡・
            stateManager.ChangeState(PlayerState.Walking);
            yield return null;

            stateManager.ChangeState(PlayerState.Running);
            yield return null;

            // 迴ｾ蝨ｨ縺ｮ迥ｶ諷九ｒ遒ｺ隱・
            Assert.AreEqual(PlayerState.Running, stateManager.CurrentState);

            // Act - 蜑阪・迥ｶ諷九↓謌ｻ繧・
            stateManager.RevertToPreviousState();
            yield return null;

            // Assert
            Assert.AreEqual(PlayerState.Walking, stateManager.CurrentState,
                "Should revert to previous state (Walking)");
        }

        [UnityTest]
        public IEnumerator StateManager_Should_Skip_Same_State_Transition()
        {
            // Start()縺悟ｮ溯｡後＆繧後ｋ縺ｮ繧貞ｾ・▽
            yield return null;

            // Act - Walking迥ｶ諷九↓驕ｷ遘ｻ
            stateManager.ChangeState(PlayerState.Walking);
            yield return null;

            var historyCountBefore = stateManager.StateHistory.Count;

            // Act - 蜷後§迥ｶ諷九↓蜀榊ｺｦ驕ｷ遘ｻ繧定ｩｦ縺ｿ繧・
            stateManager.ChangeState(PlayerState.Walking);
            yield return null;

            // Assert - 螻･豁ｴ縺悟｢励∴縺ｦ縺・↑縺・％縺ｨ繧堤｢ｺ隱・
            Assert.AreEqual(historyCountBefore, stateManager.StateHistory.Count,
                "History should not change when transitioning to the same state");
        }

        [UnityTest]
        public IEnumerator StateManager_Should_Check_Transition_Availability()
        {
            // Start()縺悟ｮ溯｡後＆繧後ｋ縺ｮ繧貞ｾ・▽
            yield return null;

            // Act & Assert - 逋ｻ骭ｲ貂医∩迥ｶ諷・
            Assert.IsTrue(stateManager.CanTransitionTo(PlayerState.Walking),
                "Should be able to transition to registered state");
            Assert.IsTrue(stateManager.CanTransitionTo(PlayerState.Combat),
                "Should be able to transition to registered state");

            // 豕ｨ: 譛ｪ逋ｻ骭ｲ縺ｮ迥ｶ諷九・繝・せ繝医・縲∫樟蝨ｨ縺ｮ螳溯｣・〒縺ｯ蜈ｨ縺ｦ縺ｮ蝓ｺ譛ｬ迥ｶ諷九′逋ｻ骭ｲ縺輔ｌ縺ｦ縺・ｋ縺溘ａ逵∫払
        }

        [UnityTest]
        public IEnumerator StateManager_Should_Handle_Complex_Scenario()
        {
            // Start()縺悟ｮ溯｡後＆繧後ｋ縺ｮ繧貞ｾ・▽
            yield return null;

            // 繧ｷ繝翫Μ繧ｪ: 繝励Ξ繧､繝､繝ｼ縺梧ｭｩ縺・※縲∬ｵｰ縺｣縺ｦ縲√ず繝｣繝ｳ繝励＠縺ｦ縲∵姶髣倥↓蜈･繧翫∵ｭｻ莠｡縺吶ｋ

            // Act & Assert - Walking
            stateManager.ChangeState(PlayerState.Walking);
            yield return new WaitForSeconds(0.1f);
            Assert.AreEqual(PlayerState.Walking, stateManager.CurrentState);

            // Act & Assert - Running
            stateManager.ChangeState(PlayerState.Running);
            yield return new WaitForSeconds(0.1f);
            Assert.AreEqual(PlayerState.Running, stateManager.CurrentState);

            // Act & Assert - Jumping
            stateManager.ChangeState(PlayerState.Jumping);
            yield return new WaitForSeconds(0.1f);
            Assert.AreEqual(PlayerState.Jumping, stateManager.CurrentState);

            // Act & Assert - Landing
            stateManager.ChangeState(PlayerState.Landing);
            yield return new WaitForSeconds(0.1f);
            Assert.AreEqual(PlayerState.Landing, stateManager.CurrentState);

            // Act & Assert - Combat
            stateManager.ChangeState(PlayerState.Combat);
            yield return new WaitForSeconds(0.1f);
            Assert.AreEqual(PlayerState.Combat, stateManager.CurrentState);

            // Act & Assert - Dead
            stateManager.ChangeState(PlayerState.Dead);
            yield return new WaitForSeconds(0.1f);
            Assert.AreEqual(PlayerState.Dead, stateManager.CurrentState);

            // 螻･豁ｴ縺梧ｭ｣縺励￥險倬鹸縺輔ｌ縺ｦ縺・ｋ縺狗｢ｺ隱・
            var history = stateManager.StateHistory;
            Assert.GreaterOrEqual(history.Count, 5, "Should have multiple states in history");
        }

        [Test]
        public void ServiceLocator_Should_Maintain_StateService_Across_Tests()
        {
            // Arrange & Act
            var stateService = ServiceLocator.Get<IStateService>();

            // Assert
            Assert.IsNotNull(stateService, "StateService should be available");
            Assert.IsTrue(stateService.HasHandler((int)PlayerState.Idle),
                "StateService should have registered handlers");
        }
    }
}


