using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using asterivo.Unity60.Core.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

namespace asterivo.Unity60.Tests.Core.Events
{
    /// <summary>
    /// Events配下の実装に対する包括的なテストクラス
    /// GameEvent、GameEventListener、およびイベントドリブンアーキテクチャの動作を検証
    /// </summary>
    [TestFixture]
    public class EventsTests
    {
        private GameEvent testEvent;
        private GameObject testGameObject;
        private MockGameEventListener mockListener1;
        private MockGameEventListener mockListener2;
        
        /// <summary>
        /// テスト用のGameEventListenerモック実装
        /// </summary>
        private class MockGameEventListener : IGameEventListener<int>
        {
            public int CallCount { get; private set; }
            public int LastValue { get; private set; }
            public int Priority { get; set; }
            public bool IsEnabled { get; set; } = true;
            public string Name { get; set; }
            
            public MockGameEventListener(string name = "MockListener", int priority = 0)
            {
                Name = name;
                Priority = priority;
            }
            
            public void OnEventRaised(int value)
            {
                if (!IsEnabled) return;
                CallCount++;
                LastValue = value;
                Debug.Log($"[{Name}] Received value: {value}, CallCount: {CallCount}");
            }
            
            // GameEventListener interface compatibility
            public void OnEventRaised()
            {
                OnEventRaised(0);
            }
            
            public void Reset()
            {
                CallCount = 0;
                LastValue = 0;
            }
        }

        [SetUp]
        public void SetUp()
        {
            // GameEventのScriptableObjectインスタンスを作成
            testEvent = ScriptableObject.CreateInstance<GameEvent>();
            
            // テスト用GameObjectとListenerを作成
            testGameObject = new GameObject("TestGameObject");
            mockListener1 = new MockGameEventListener("Listener1", 10);
            mockListener2 = new MockGameEventListener("Listener2", 5);
        }

        [TearDown]
        public void TearDown()
        {
            // GameEventのリスナーをクリア
            testEvent?.ClearAllListeners();
            
            // 作成したオブジェクトを削除
            if (testGameObject != null)
            {
                Object.DestroyImmediate(testGameObject);
            }
            
            if (testEvent != null)
            {
                Object.DestroyImmediate(testEvent);
            }
        }

        #region GameEvent Basic Tests

        /// <summary>
        /// GameEventの基本的な発火動作テスト
        /// </summary>
        [Test]
        public void GameEvent_Raise_ExecutesCorrectly()
        {
            // Arrange
            var listenerComponent = testGameObject.AddComponent<GameEventListener>();
            bool wasTriggered = false;
            listenerComponent.Response.AddListener(() => wasTriggered = true);
            
            // GameEventListenerの設定
            var eventProperty = typeof(GameEventListener).GetProperty("Event");
            eventProperty?.SetValue(listenerComponent, testEvent);
            
            // リスナーを手動登録
            testEvent.RegisterListener(listenerComponent);
            
            // Act
            testEvent.Raise();
            
            // Assert
            Assert.IsTrue(wasTriggered, "GameEventが発火された時にリスナーが呼ばれるべき");
            Assert.AreEqual(1, testEvent.GetListenerCount(), "リスナー数が正しくない");
        }

        /// <summary>
        /// GameEventのリスナー登録・登録解除テスト
        /// </summary>
        [Test]
        public void GameEvent_RegisterUnregisterListener_WorksCorrectly()
        {
            // Arrange
            var listenerComponent = testGameObject.AddComponent<GameEventListener>();
            
            // Act - 登録
            testEvent.RegisterListener(listenerComponent);
            
            // Assert - 登録確認
            Assert.AreEqual(1, testEvent.GetListenerCount(), "リスナーが登録されていない");
            
            // Act - 登録解除
            testEvent.UnregisterListener(listenerComponent);
            
            // Assert - 登録解除確認
            Assert.AreEqual(0, testEvent.GetListenerCount(), "リスナーが登録解除されていない");
        }

        /// <summary>
        /// GameEventの複数リスナー同時実行テスト
        /// </summary>
        [Test]
        public void GameEvent_MultipleListeners_AllExecuted()
        {
            // Arrange
            var listener1 = testGameObject.AddComponent<GameEventListener>();
            var listener2 = testGameObject.AddComponent<GameEventListener>();
            
            bool listener1Triggered = false;
            bool listener2Triggered = false;
            
            listener1.Response.AddListener(() => listener1Triggered = true);
            listener2.Response.AddListener(() => listener2Triggered = true);
            
            testEvent.RegisterListener(listener1);
            testEvent.RegisterListener(listener2);
            
            // Act
            testEvent.Raise();
            
            // Assert
            Assert.IsTrue(listener1Triggered, "Listener1が実行されていない");
            Assert.IsTrue(listener2Triggered, "Listener2が実行されていない");
            Assert.AreEqual(2, testEvent.GetListenerCount(), "リスナー数が正しくない");
        }

        /// <summary>
        /// GameEventの全リスナークリアテスト
        /// </summary>
        [Test]
        public void GameEvent_ClearAllListeners_RemovesAllListeners()
        {
            // Arrange
            var listener1 = testGameObject.AddComponent<GameEventListener>();
            var listener2 = testGameObject.AddComponent<GameEventListener>();
            
            testEvent.RegisterListener(listener1);
            testEvent.RegisterListener(listener2);
            
            // Act
            testEvent.ClearAllListeners();
            
            // Assert
            Assert.AreEqual(0, testEvent.GetListenerCount(), "全リスナーがクリアされていない");
        }

        /// <summary>
        /// GameEventの重複リスナー登録防止テスト
        /// </summary>
        [Test]
        public void GameEvent_DuplicateListener_PreventsDuplication()
        {
            // Arrange
            var listenerComponent = testGameObject.AddComponent<GameEventListener>();
            
            // Act - 同じリスナーを複数回登録
            testEvent.RegisterListener(listenerComponent);
            testEvent.RegisterListener(listenerComponent);
            testEvent.RegisterListener(listenerComponent);
            
            // Assert
            Assert.AreEqual(1, testEvent.GetListenerCount(), "重複リスナー登録が防止されていない");
        }

        /// <summary>
        /// GameEventのnullリスナーハンドリングテスト
        /// </summary>
        [Test]
        public void GameEvent_NullListener_HandlesGracefully()
        {
            // Act & Assert - nullリスナーの登録/登録解除でエラーが発生しないことを確認
            Assert.DoesNotThrow(() => testEvent.RegisterListener(null), "nullリスナー登録時にエラーが発生");
            Assert.DoesNotThrow(() => testEvent.UnregisterListener(null), "nullリスナー登録解除時にエラーが発生");
            
            Assert.AreEqual(0, testEvent.GetListenerCount(), "nullリスナーがカウントされている");
        }

        #endregion

        #region GameEventListener Tests

        /// <summary>
        /// GameEventListenerの基本的な応答動作テスト
        /// </summary>
        [Test]
        public void GameEventListener_OnEventRaised_ExecutesResponse()
        {
            // Arrange
            var listenerComponent = testGameObject.AddComponent<GameEventListener>();
            bool wasTriggered = false;
            int triggerCount = 0;
            
            listenerComponent.Response.AddListener(() => {
                wasTriggered = true;
                triggerCount++;
            });
            
            // Act
            listenerComponent.OnEventRaised();
            
            // Assert
            Assert.IsTrue(wasTriggered, "OnEventRaisedでResponseが実行されていない");
            Assert.AreEqual(1, triggerCount, "Response実行回数が正しくない");
        }

        /// <summary>
        /// GameEventListenerの優先度プロパティテスト
        /// </summary>
        [Test]
        public void GameEventListener_Priority_ReturnsCorrectValue()
        {
            // Arrange
            var listenerComponent = testGameObject.AddComponent<GameEventListener>();
            
            // リフレクションを使用してprivateフィールドにアクセス
            var priorityField = typeof(GameEventListener).GetField("priority", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            priorityField?.SetValue(listenerComponent, 42);
            
            // Assert
            Assert.AreEqual(42, listenerComponent.Priority, "優先度が正しく取得できていない");
        }

        /// <summary>
        /// GameEventListenerのワンショット機能テスト
        /// </summary>
        [Test]
        public void GameEventListener_OneShot_ExecutesOnlyOnce()
        {
            // Arrange
            var listenerComponent = testGameObject.AddComponent<GameEventListener>();
            int triggerCount = 0;
            
            listenerComponent.Response.AddListener(() => triggerCount++);
            
            // リフレクションを使用してoneShotフィールドを設定
            var oneShotField = typeof(GameEventListener).GetField("oneShot", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            oneShotField?.SetValue(listenerComponent, true);
            
            // Act - 複数回イベントを実行
            listenerComponent.OnEventRaised();
            listenerComponent.OnEventRaised();
            listenerComponent.OnEventRaised();
            
            // Assert
            Assert.AreEqual(1, triggerCount, "ワンショットモードで複数回実行されている");
            Assert.IsFalse(listenerComponent.enabled, "ワンショット実行後にコンポーネントが無効化されていない");
        }

        /// <summary>
        /// GameEventListenerのワンショットリセット機能テスト
        /// </summary>
        [Test]
        public void GameEventListener_ResetOneShot_AllowsRetrigger()
        {
            // Arrange
            var listenerComponent = testGameObject.AddComponent<GameEventListener>();
            int triggerCount = 0;
            
            listenerComponent.Response.AddListener(() => triggerCount++);
            
            var oneShotField = typeof(GameEventListener).GetField("oneShot", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            oneShotField?.SetValue(listenerComponent, true);
            
            // Act
            listenerComponent.OnEventRaised(); // 1回目
            listenerComponent.ResetOneShot(); // リセット
            listenerComponent.enabled = true; // 再有効化
            listenerComponent.OnEventRaised(); // 2回目
            
            // Assert
            Assert.AreEqual(2, triggerCount, "ワンショットリセット後に再実行されていない");
        }

        /// <summary>
        /// GameEventListenerのEvent設定テスト
        /// </summary>
        [Test]
        public void GameEventListener_SetEvent_RegistersAndUnregistersCorrectly()
        {
            // Arrange
            var listenerComponent = testGameObject.AddComponent<GameEventListener>();
            var secondEvent = ScriptableObject.CreateInstance<GameEvent>();
            
            try
            {
                // Act - イベント設定
                listenerComponent.Event = testEvent;
                
                // Assert - 新しいイベントに登録されている
                Assert.AreEqual(testEvent, listenerComponent.Event, "イベントが正しく設定されていない");
                Assert.AreEqual(1, testEvent.GetListenerCount(), "新しいイベントにリスナーが登録されていない");
                
                // Act - 別のイベントに変更
                listenerComponent.Event = secondEvent;
                
                // Assert - 古いイベントから登録解除され、新しいイベントに登録されている
                Assert.AreEqual(secondEvent, listenerComponent.Event, "新しいイベントが設定されていない");
                Assert.AreEqual(0, testEvent.GetListenerCount(), "古いイベントからリスナーが登録解除されていない");
                Assert.AreEqual(1, secondEvent.GetListenerCount(), "新しいイベントにリスナーが登録されていない");
            }
            finally
            {
                // Cleanup
                if (secondEvent != null)
                {
                    Object.DestroyImmediate(secondEvent);
                }
            }
        }

        #endregion

        #region Integration Tests

        /// <summary>
        /// GameEventとGameEventListenerの統合テスト
        /// </summary>
        [Test]
        public void GameEventAndListener_Integration_WorksTogether()
        {
            // Arrange
            var listener1 = testGameObject.AddComponent<GameEventListener>();
            var listener2 = testGameObject.AddComponent<GameEventListener>();
            
            int listener1Count = 0;
            int listener2Count = 0;
            
            listener1.Response.AddListener(() => listener1Count++);
            listener2.Response.AddListener(() => listener2Count++);
            
            listener1.Event = testEvent;
            listener2.Event = testEvent;
            
            // Act
            testEvent.Raise();
            testEvent.Raise();
            testEvent.Raise();
            
            // Assert
            Assert.AreEqual(3, listener1Count, "Listener1の実行回数が正しくない");
            Assert.AreEqual(3, listener2Count, "Listener2の実行回数が正しくない");
        }

        /// <summary>
        /// 複雑なイベントチェーンのテスト
        /// </summary>
        [Test]
        public void GameEvent_ComplexEventChain_ExecutesInCorrectOrder()
        {
            // Arrange
            var secondaryEvent = ScriptableObject.CreateInstance<GameEvent>();
            var primaryListener = testGameObject.AddComponent<GameEventListener>();
            var secondaryListener = testGameObject.AddComponent<GameEventListener>();
            
            var executionOrder = new List<string>();
            
            primaryListener.Response.AddListener(() => {
                executionOrder.Add("Primary");
                secondaryEvent.Raise(); // チェーンして別のイベントを発火
            });
            
            secondaryListener.Response.AddListener(() => {
                executionOrder.Add("Secondary");
            });
            
            primaryListener.Event = testEvent;
            secondaryListener.Event = secondaryEvent;
            
            try
            {
                // Act
                testEvent.Raise();
                
                // Assert
                Assert.AreEqual(2, executionOrder.Count, "イベントチェーンの実行数が正しくない");
                Assert.AreEqual("Primary", executionOrder[0], "最初に実行されるべきイベントが異なる");
                Assert.AreEqual("Secondary", executionOrder[1], "チェーンされたイベントが実行されていない");
            }
            finally
            {
                // Cleanup
                if (secondaryEvent != null)
                {
                    Object.DestroyImmediate(secondaryEvent);
                }
            }
        }

        /// <summary>
        /// 無効なGameObjectのGameEventListenerハンドリングテスト
        /// </summary>
        [Test]
        public void GameEventListener_DestroyedGameObject_HandlesGracefully()
        {
            // Arrange
            var listenerComponent = testGameObject.AddComponent<GameEventListener>();
            listenerComponent.Event = testEvent;
            
            // GameObjectを破棄
            Object.DestroyImmediate(testGameObject);
            testGameObject = null;
            
            // Act & Assert - 破棄されたリスナーがあってもイベント発火でエラーが発生しない
            Assert.DoesNotThrow(() => testEvent.Raise(), "破棄されたリスナーがあるとイベント発火でエラーが発生");
        }

        #endregion

        #region Performance Tests

        /// <summary>
        /// 大量リスナーでのパフォーマンステスト
        /// </summary>
        [Test]
        public void GameEvent_ManyListeners_PerformanceTest()
        {
            // Arrange
            const int listenerCount = 100;
            var listeners = new List<GameEventListener>();
            int totalExecutions = 0;
            
            for (int i = 0; i < listenerCount; i++)
            {
                var go = new GameObject($"Listener_{i}");
                var listener = go.AddComponent<GameEventListener>();
                listener.Response.AddListener(() => totalExecutions++);
                listener.Event = testEvent;
                listeners.Add(listener);
            }
            
            try
            {
                // Act
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                testEvent.Raise();
                stopwatch.Stop();
                
                // Assert
                Assert.AreEqual(listenerCount, totalExecutions, "全リスナーが実行されていない");
                Assert.Less(stopwatch.ElapsedMilliseconds, 100, "大量リスナーの処理に時間がかかりすぎている");
                
                Debug.Log($"100リスナーのイベント処理時間: {stopwatch.ElapsedMilliseconds}ms");
            }
            finally
            {
                // Cleanup
                foreach (var listener in listeners)
                {
                    if (listener != null && listener.gameObject != null)
                    {
                        Object.DestroyImmediate(listener.gameObject);
                    }
                }
            }
        }

        #endregion
    }
}
