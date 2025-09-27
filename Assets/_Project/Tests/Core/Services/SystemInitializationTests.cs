using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using asterivo.Unity60.Core;

namespace asterivo.Unity60.Tests.Core.Services
{
    /// <summary>
    /// システム初期化テストスイート
    /// IInitializableシステムとSystemInitializerの機能を検証
    /// </summary>
    [TestFixture]
    public class SystemInitializationTests
    {
        private GameObject rootGameObject;
        private SystemInitializer systemInitializer;
        private List<GameObject> createdGameObjects;
        private List<TestInitializableSystem> testSystems;

        #region Test Implementation Classes

        /// <summary>
        /// テスト用のIInitializable実装クラス
        /// </summary>
        public class TestInitializableSystem : MonoBehaviour, IInitializable
        {
            public int Priority { get; private set; }
            public bool IsInitialized { get; private set; }
            public bool ShouldThrowOnInitialize { get; set; } = false;
            public List<string> InitializationLog { get; private set; } = new List<string>();
            
            private static int initializationCounter = 0;
            public int InitializationOrder { get; private set; } = -1;

            public TestInitializableSystem(int priority = 0)
            {
                Priority = priority;
            }

            public void SetPriority(int priority)
            {
                Priority = priority;
            }

            public void Initialize()
            {
                if (IsInitialized)
                {
                    InitializationLog.Add("Already initialized");
                    return;
                }

                if (ShouldThrowOnInitialize)
                {
                    throw new System.Exception($"Test exception from {gameObject.name}");
                }

                InitializationOrder = ++initializationCounter;
                IsInitialized = true;
                InitializationLog.Add($"Initialized with order {InitializationOrder}");
            }

            public void Reset()
            {
                IsInitialized = false;
                InitializationOrder = -1;
                InitializationLog.Clear();
            }

            public static void ResetCounter()
            {
                initializationCounter = 0;
            }
        }

        /// <summary>
        /// 高優先度システム（早く初期化される）
        /// </summary>
        public class HighPrioritySystem : TestInitializableSystem
        {
            public HighPrioritySystem() : base(1) { }
        }

        /// <summary>
        /// 中優先度システム
        /// </summary>
        public class MediumPrioritySystem : TestInitializableSystem
        {
            public MediumPrioritySystem() : base(10) { }
        }

        /// <summary>
        /// 低優先度システム（遅く初期化される）
        /// </summary>
        public class LowPrioritySystem : TestInitializableSystem
        {
            public LowPrioritySystem() : base(20) { }
        }

        #endregion

        #region Setup and Teardown

        [SetUp]
        public void SetUp()
        {
            // ServiceLocatorをクリア
            ServiceLocator.Clear();
            
            // Feature Flagsを設定
            FeatureFlags.UseServiceLocator = true;
            FeatureFlags.EnableDebugLogging = false;
            
            // 初期化カウンターをリセット
            TestInitializableSystem.ResetCounter();
            
            // テスト用リストの初期化
            createdGameObjects = new List<GameObject>();
            testSystems = new List<TestInitializableSystem>();
            
            // ルートGameObjectの作成
            rootGameObject = new GameObject("SystemInitializerTest");
            createdGameObjects.Add(rootGameObject);
            
            // SystemInitializerの追加
            systemInitializer = rootGameObject.AddComponent<SystemInitializer>();
        }

        [TearDown]
        public void TearDown()
        {
            // GameObjectsの破棄
            foreach (var go in createdGameObjects)
            {
                if (go != null)
                {
                    Object.DestroyImmediate(go);
                }
            }
            
            // ServiceLocatorのクリア
            ServiceLocator.Clear();
            
            // Feature Flagsをリセット
            FeatureFlags.UseServiceLocator = false;
            FeatureFlags.EnableDebugLogging = true;
        }

        private T CreateTestSystem<T>(string name = null) where T : TestInitializableSystem, new()
        {
            var gameObject = new GameObject(name ?? typeof(T).Name);
            gameObject.transform.SetParent(rootGameObject.transform);
            createdGameObjects.Add(gameObject);
            
            var system = gameObject.AddComponent<T>();
            testSystems.Add(system);
            return system;
        }

        #endregion

        #region Service Registration Tests

        [Test]
        public void SystemInitializer_OnAwake_RegistersWithServiceLocator()
        {
            // Act
            systemInitializer.gameObject.SetActive(true);

            // Assert
            Assert.IsTrue(ServiceLocator.HasService<SystemInitializer>());
            var registeredInitializer = ServiceLocator.GetService<SystemInitializer>();
            Assert.AreSame(systemInitializer, registeredInitializer);
        }

        #endregion

        #region System Discovery Tests

        [Test]
        public void SystemInitializer_DiscoverSystems_FindsAllIInitializableComponents()
        {
            // Arrange
            var highPriority = CreateTestSystem<HighPrioritySystem>();
            var mediumPriority = CreateTestSystem<MediumPrioritySystem>();
            var lowPriority = CreateTestSystem<LowPrioritySystem>();

            // Act
            systemInitializer.gameObject.SetActive(true);

            // Assert
            // SystemInitializerがシステムを発見できることを確認
            // 内部状態は直接アクセスできないため、初期化実行で間接的に確認
            systemInitializer.InitializeAllSystems();
            
            Assert.IsTrue(highPriority.IsInitialized);
            Assert.IsTrue(mediumPriority.IsInitialized);
            Assert.IsTrue(lowPriority.IsInitialized);
        }

        [Test]
        public void SystemInitializer_WithNoInitializableSystems_DoesNotThrow()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => 
            {
                systemInitializer.gameObject.SetActive(true);
                systemInitializer.InitializeAllSystems();
            });
        }

        #endregion

        #region Priority-Based Initialization Tests

        [Test]
        public void SystemInitializer_InitializeAllSystems_RespectsOrderPriority()
        {
            // Arrange
            var lowPriority = CreateTestSystem<LowPrioritySystem>("LowPriority");
            var highPriority = CreateTestSystem<HighPrioritySystem>("HighPriority");
            var mediumPriority = CreateTestSystem<MediumPrioritySystem>("MediumPriority");

            // Act
            systemInitializer.gameObject.SetActive(true);
            systemInitializer.InitializeAllSystems();

            // Assert
            Assert.IsTrue(highPriority.IsInitialized);
            Assert.IsTrue(mediumPriority.IsInitialized);
            Assert.IsTrue(lowPriority.IsInitialized);
            
            // 優先度順に初期化されていることを確認
            Assert.Less(highPriority.InitializationOrder, mediumPriority.InitializationOrder);
            Assert.Less(mediumPriority.InitializationOrder, lowPriority.InitializationOrder);
        }

        [Test]
        public void SystemInitializer_SamePriority_InitializesInDiscoveredOrder()
        {
            // Arrange
            var system1 = CreateTestSystem<HighPrioritySystem>("System1");
            system1.SetPriority(10);
            var system2 = CreateTestSystem<HighPrioritySystem>("System2");
            system2.SetPriority(10);
            var system3 = CreateTestSystem<MediumPrioritySystem>("System3");
            system3.SetPriority(10);

            // Act
            systemInitializer.gameObject.SetActive(true);
            systemInitializer.InitializeAllSystems();

            // Assert
            Assert.IsTrue(system1.IsInitialized);
            Assert.IsTrue(system2.IsInitialized);
            Assert.IsTrue(system3.IsInitialized);
            
            // 同じ優先度の場合、発見順で初期化されることを確認
            Assert.Less(system1.InitializationOrder, system2.InitializationOrder);
            Assert.Less(system2.InitializationOrder, system3.InitializationOrder);
        }

        #endregion

        #region Individual System Initialization Tests

        [Test]
        public void SystemInitializer_InitializeSpecificSystem_InitializesOnlyTargetSystem()
        {
            // Arrange
            var targetSystem = CreateTestSystem<HighPrioritySystem>();
            var otherSystem = CreateTestSystem<LowPrioritySystem>();

            systemInitializer.gameObject.SetActive(true);

            // Act
            systemInitializer.InitializeSystem<HighPrioritySystem>();

            // Assert
            Assert.IsTrue(targetSystem.IsInitialized);
            Assert.IsFalse(otherSystem.IsInitialized);
        }

        [Test]
        public void SystemInitializer_IsSystemInitialized_ReturnsCorrectStatus()
        {
            // Arrange
            var system = CreateTestSystem<LowPrioritySystem>();
            systemInitializer.gameObject.SetActive(true);

            // Act & Assert
            Assert.IsFalse(systemInitializer.IsSystemInitialized<TestInitializableSystem>());
            
            systemInitializer.InitializeSystem<TestInitializableSystem>();
            Assert.IsTrue(systemInitializer.IsSystemInitialized<TestInitializableSystem>());
        }

        [Test]
        public void SystemInitializer_AreAllSystemsInitialized_ReturnsCorrectStatus()
        {
            // Arrange
            var system1 = CreateTestSystem<HighPrioritySystem>();
            var system2 = CreateTestSystem<LowPrioritySystem>();
            systemInitializer.gameObject.SetActive(true);

            // Act & Assert
            Assert.IsFalse(systemInitializer.AreAllSystemsInitialized());
            
            systemInitializer.InitializeSystem<HighPrioritySystem>();
            Assert.IsFalse(systemInitializer.AreAllSystemsInitialized());
            
            systemInitializer.InitializeSystem<LowPrioritySystem>();
            Assert.IsTrue(systemInitializer.AreAllSystemsInitialized());
        }

        #endregion

        #region Error Handling Tests

        [Test]
        public void SystemInitializer_SystemThrowsException_ContinuesWithOtherSystems()
        {
            // Arrange
            var goodSystem1 = CreateTestSystem<HighPrioritySystem>();
            var badSystem = CreateTestSystem<MediumPrioritySystem>();
            var goodSystem2 = CreateTestSystem<LowPrioritySystem>();
            
            badSystem.ShouldThrowOnInitialize = true;
            systemInitializer.gameObject.SetActive(true);

            // Act
            LogAssert.Expect(LogType.Error, System.Text.RegularExpressions.Regex.Escape("[SystemInitializer] Failed to initialize MediumPrioritySystem: Test exception from MediumPrioritySystem"));
            systemInitializer.InitializeAllSystems();

            // Assert
            Assert.IsTrue(goodSystem1.IsInitialized);
            Assert.IsFalse(badSystem.IsInitialized);
            Assert.IsTrue(goodSystem2.IsInitialized);
        }

        [Test]
        public void SystemInitializer_InitializeAlreadyInitializedSystem_DoesNotReinitialize()
        {
            // Arrange
            var system = CreateTestSystem<MockAudioService>();
            systemInitializer.gameObject.SetActive(true);
            
            systemInitializer.InitializeSystem<TestInitializableSystem>();
            int firstInitOrder = system.InitializationOrder;

            // Act
            systemInitializer.InitializeSystem<TestInitializableSystem>();

            // Assert
            Assert.AreEqual(firstInitOrder, system.InitializationOrder);
            Assert.AreEqual(1, system.InitializationLog.Count(log => log.Contains("Initialized with order")));
        }

        [Test]
        public void SystemInitializer_InitializeAllSystemsTwice_DoesNotReinitialize()
        {
            // Arrange
            var system = CreateTestSystem<MockAudioUpdateCoordinator>();
            systemInitializer.gameObject.SetActive(true);

            // Act
            systemInitializer.InitializeAllSystems();
            LogAssert.Expect(LogType.Warning, "[SystemInitializer] Systems already initialized");
            systemInitializer.InitializeAllSystems();

            // Assert
            Assert.IsTrue(system.IsInitialized);
            Assert.AreEqual(1, system.InitializationLog.Count(log => log.Contains("Initialized with order")));
        }

        #endregion

        #region Auto-Initialization Tests

        [UnityTest]
        public IEnumerator SystemInitializer_AutoInitializeOnStart_InitializesSystemsAutomatically()
        {
            // Arrange
            var system = CreateTestSystem<HighPrioritySystem>();
            
            // SystemInitializerのautoInitializeOnStartはデフォルトでtrue
            systemInitializer.gameObject.SetActive(true);

            // Act
            yield return null; // Start()が呼ばれるまで待機

            // Assert
            Assert.IsTrue(system.IsInitialized);
        }

        #endregion

        #region Integration with Real Audio Systems Tests

        [Test]
        public void SystemInitializer_WithRealAudioSystems_InitializesInCorrectOrder()
        {
            // Arrange
            // 実際のAudioService（Priority: 10）をシミュレート
            var audioService = rootGameObject.AddComponent<MockAudioService>();
            
            // 実際のAudioUpdateCoordinator（Priority: 15）をシミュレート
            var audioCoordinator = rootGameObject.AddComponent<MockAudioUpdateCoordinator>();

            systemInitializer.gameObject.SetActive(true);

            // Act
            systemInitializer.InitializeAllSystems();

            // Assert
            Assert.IsTrue(audioService.IsInitialized);
            Assert.IsTrue(audioCoordinator.IsInitialized);
            
            // AudioServiceがAudioUpdateCoordinatorより先に初期化されることを確認
            Assert.Less(audioService.InitializationOrder, audioCoordinator.InitializationOrder);
        }

        #endregion

        #region Mock Classes for Integration Tests

        public class MockAudioService : TestInitializableSystem
        {
            public MockAudioService() : base(10) { } // AudioServiceのPriorityと同じ
        }

        public class MockAudioUpdateCoordinator : TestInitializableSystem
        {
            public MockAudioUpdateCoordinator() : base(15) { } // AudioUpdateCoordinatorのPriorityと同じ
        }

        #endregion

        #region Performance Tests

        [UnityTest]
        public IEnumerator SystemInitializer_ManySystemsInitialization_CompletesWithinReasonableTime()
        {
            // Arrange
            int systemCount = 50;
            for (int i = 0; i < systemCount; i++)
            {
                var system = CreateTestSystem<LowPrioritySystem>($"System_{i}");
                system.SetPriority(i % 10); // 優先度をばらつかせる
            }

            systemInitializer.gameObject.SetActive(true);
            float startTime = Time.realtimeSinceStartup;

            // Act
            systemInitializer.InitializeAllSystems();
            yield return null;

            float elapsed = Time.realtimeSinceStartup - startTime;

            // Assert
            Assert.Less(elapsed, 0.1f, "Many systems initialization should complete quickly");
            Assert.IsTrue(systemInitializer.AreAllSystemsInitialized());
            Assert.AreEqual(systemCount, testSystems.Count(s => s.IsInitialized));
        }

        #endregion
    }
}
