using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Features.StateManagement;
using asterivo.Unity60.Features.Player;

namespace asterivo.Unity60.Tests.Features.StateManagement
{
    /// <summary>
    /// StateManagement機能の統合テスト
    /// 実際のゲームシナリオでの動作を検証
    /// </summary>
    [TestFixture]
    public class StateManagementIntegrationTest
    {
        private GameObject testObject;
        private StateManager stateManager;

        [SetUp]
        public void Setup()
        {
            // ServiceLocatorをクリア
            ServiceLocator.Clear();

            // StateManagementを初期化
            StateManagementBootstrapper.Initialize();

            // テスト用GameObjectを作成
            testObject = new GameObject("TestStateManager");
            stateManager = testObject.AddComponent<StateManager>();
        }

        [TearDown]
        public void TearDown()
        {
            // テストオブジェクトを破棄
            if (testObject != null)
            {
                Object.DestroyImmediate(testObject);
            }

            // StateManagementをシャットダウン
            StateManagementBootstrapper.Shutdown();

            // ServiceLocatorをクリア
            ServiceLocator.Clear();
        }

        [UnityTest]
        public IEnumerator StateManager_Should_Initialize_With_Idle_State()
        {
            // Start()が実行されるのを待つ
            yield return null;

            // Assert
            Assert.AreEqual(PlayerState.Idle, stateManager.CurrentState,
                "StateManager should start with Idle state");
        }

        [UnityTest]
        public IEnumerator StateManager_Should_Transition_Between_States()
        {
            // Start()が実行されるのを待つ
            yield return null;

            // Act - Idle → Walking
            stateManager.ChangeState(PlayerState.Walking);
            yield return null;

            // Assert
            Assert.AreEqual(PlayerState.Walking, stateManager.CurrentState,
                "Should transition to Walking state");

            // Act - Walking → Running
            stateManager.ChangeState(PlayerState.Running);
            yield return null;

            // Assert
            Assert.AreEqual(PlayerState.Running, stateManager.CurrentState,
                "Should transition to Running state");

            // Act - Running → Combat
            stateManager.ChangeState(PlayerState.Combat);
            yield return null;

            // Assert
            Assert.AreEqual(PlayerState.Combat, stateManager.CurrentState,
                "Should transition to Combat state");
        }

        [UnityTest]
        public IEnumerator StateManager_Should_Maintain_State_History()
        {
            // Start()が実行されるのを待つ
            yield return null;

            // Act - 複数の状態遷移を実行
            stateManager.ChangeState(PlayerState.Walking);
            yield return null;

            stateManager.ChangeState(PlayerState.Running);
            yield return null;

            stateManager.ChangeState(PlayerState.Jumping);
            yield return null;

            // Assert - 履歴を検証
            var history = stateManager.StateHistory;
            Assert.IsNotNull(history, "State history should not be null");
            Assert.Greater(history.Count, 0, "State history should have entries");

            // 履歴に期待される状態が含まれているか確認
            Assert.Contains(PlayerState.Idle, history as System.Collections.ICollection,
                "History should contain Idle state");
            Assert.Contains(PlayerState.Walking, history as System.Collections.ICollection,
                "History should contain Walking state");
        }

        [UnityTest]
        public IEnumerator StateManager_Should_Revert_To_Previous_State()
        {
            // Start()が実行されるのを待つ
            yield return null;

            // Act - 状態遷移を実行
            stateManager.ChangeState(PlayerState.Walking);
            yield return null;

            stateManager.ChangeState(PlayerState.Running);
            yield return null;

            // 現在の状態を確認
            Assert.AreEqual(PlayerState.Running, stateManager.CurrentState);

            // Act - 前の状態に戻る
            stateManager.RevertToPreviousState();
            yield return null;

            // Assert
            Assert.AreEqual(PlayerState.Walking, stateManager.CurrentState,
                "Should revert to previous state (Walking)");
        }

        [UnityTest]
        public IEnumerator StateManager_Should_Skip_Same_State_Transition()
        {
            // Start()が実行されるのを待つ
            yield return null;

            // Act - Walking状態に遷移
            stateManager.ChangeState(PlayerState.Walking);
            yield return null;

            var historyCountBefore = stateManager.StateHistory.Count;

            // Act - 同じ状態に再度遷移を試みる
            stateManager.ChangeState(PlayerState.Walking);
            yield return null;

            // Assert - 履歴が増えていないことを確認
            Assert.AreEqual(historyCountBefore, stateManager.StateHistory.Count,
                "History should not change when transitioning to the same state");
        }

        [UnityTest]
        public IEnumerator StateManager_Should_Check_Transition_Availability()
        {
            // Start()が実行されるのを待つ
            yield return null;

            // Act & Assert - 登録済み状態
            Assert.IsTrue(stateManager.CanTransitionTo(PlayerState.Walking),
                "Should be able to transition to registered state");
            Assert.IsTrue(stateManager.CanTransitionTo(PlayerState.Combat),
                "Should be able to transition to registered state");

            // 注: 未登録の状態のテストは、現在の実装では全ての基本状態が登録されているため省略
        }

        [UnityTest]
        public IEnumerator StateManager_Should_Handle_Complex_Scenario()
        {
            // Start()が実行されるのを待つ
            yield return null;

            // シナリオ: プレイヤーが歩いて、走って、ジャンプして、戦闘に入り、死亡する

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

            // 履歴が正しく記録されているか確認
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
