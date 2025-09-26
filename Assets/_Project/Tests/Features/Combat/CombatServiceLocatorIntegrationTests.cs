using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Bootstrap;
using asterivo.Unity60.Features.Combat.Services;
using asterivo.Unity60.Features.Combat.Interfaces;
using asterivo.Unity60.Features.Combat.Components;

namespace asterivo.Unity60.Tests.Features.Combat
{
    /// <summary>
    /// ServiceLocator縺ｨCombat讖溯・縺ｮ邨ｱ蜷医ユ繧ｹ繝・
    /// GameBootstrapper縺ｨ縺ｮ騾｣謳ｺ繧呈､懆ｨｼ
    /// </summary>
    [TestFixture]
    public class CombatServiceLocatorIntegrationTests
    {
        private GameObject bootstrapperObject;
        private GameBootstrapper bootstrapper;

        [SetUp]
        public void Setup()
        {
            // ServiceLocator繧偵け繝ｪ繧｢
            ServiceLocator.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            if (bootstrapperObject != null)
            {
                Object.DestroyImmediate(bootstrapperObject);
            }
            ServiceLocator.Clear();
        }

        [UnityTest]
        public IEnumerator GameBootstrapper_RegistersCombatService()
        {
            // Arrange
            bootstrapperObject = new GameObject("TestBootstrapper");
            bootstrapper = bootstrapperObject.AddComponent<GameBootstrapper>();

            // Wait for Awake and initialization
            yield return null;

            // Act & Assert
            Assert.IsTrue(ServiceLocator.TryGet<ICombatService>(out var combatService));
            Assert.IsNotNull(combatService);
            Assert.IsTrue(combatService.IsServiceActive);
        }

        [Test]
        public void ServiceLocator_CanRegisterAndRetrieveCombatService()
        {
            // Arrange
            var combatService = new CombatService();

            // Act
            ServiceLocator.Register<ICombatService>(combatService);
            var retrieved = ServiceLocator.Get<ICombatService>();

            // Assert
            Assert.IsNotNull(retrieved);
            Assert.AreSame(combatService, retrieved);
            Assert.IsTrue(retrieved.IsServiceActive);
        }

        [Test]
        public void ServiceLocator_TryGet_ReturnsFalseWhenNotRegistered()
        {
            // Act
            bool result = ServiceLocator.TryGet<ICombatService>(out var service);

            // Assert
            Assert.IsFalse(result);
            Assert.IsNull(service);
        }

        [Test]
        public void ServiceLocator_Clear_RemovesCombatService()
        {
            // Arrange
            var combatService = new CombatService();
            ServiceLocator.Register<ICombatService>(combatService);

            // Act
            ServiceLocator.Clear();

            // Assert
            Assert.IsFalse(ServiceLocator.TryGet<ICombatService>(out var service));
            Assert.IsFalse(combatService.IsServiceActive);
        }

        [Test]
        public void CombatService_WorksWithEventManager()
        {
            // Arrange
            var eventManager = new EventManager();
            var combatService = new CombatService();

            ServiceLocator.Register<IEventManager>(eventManager);
            ServiceLocator.Register<ICombatService>(combatService);

            // Act & Assert
            Assert.IsTrue(combatService.IsServiceActive);
            Assert.IsTrue(eventManager.IsServiceActive);
        }

        [Test]
        public void MultipleServices_CanCoexist()
        {
            // Arrange
            var eventManager = new EventManager();
            var combatService = new CombatService();

            // Act
            ServiceLocator.Register<IEventManager>(eventManager);
            ServiceLocator.Register<ICombatService>(combatService);

            // Assert
            Assert.IsNotNull(ServiceLocator.Get<IEventManager>());
            Assert.IsNotNull(ServiceLocator.Get<ICombatService>());
            Assert.AreNotSame(
                ServiceLocator.Get<IEventManager>(),
                ServiceLocator.Get<ICombatService>()
            );
        }

        [UnityTest]
        public IEnumerator HealthComponent_AutoRegistersWithCombatService()
        {
            // Arrange
            var eventManager = new EventManager();
            var combatService = new CombatService();
            ServiceLocator.Register<IEventManager>(eventManager);
            ServiceLocator.Register<ICombatService>(combatService);

            var gameObject = new GameObject("TestHealth");
            var healthComponent = gameObject.AddComponent<HealthComponent>();

            // Wait for Start() to be called
            yield return null;

            // Act
            var retrieved = combatService.GetHealth(gameObject);

            // Assert
            Assert.IsNotNull(retrieved);
            Assert.AreSame(healthComponent, retrieved);

            // Cleanup
            Object.DestroyImmediate(gameObject);
        }

        [Test]
        public void CombatService_OnServiceUnregistered_ClearsData()
        {
            // Arrange
            var eventManager = new EventManager();
            var combatService = new CombatService();
            ServiceLocator.Register<IEventManager>(eventManager);
            ServiceLocator.Register<ICombatService>(combatService);

            var participant = new GameObject("Participant");
            combatService.StartCombat(participant, participant);

            // Act
            combatService.OnServiceUnregistered();

            // Assert
            Assert.IsFalse(combatService.IsServiceActive);
            Assert.IsFalse(combatService.IsInCombat(participant));

            // Cleanup
            Object.DestroyImmediate(participant);
        }

        [Test]
        public void ServiceName_IsCorrect()
        {
            // Arrange
            var combatService = new CombatService();

            // Act
            string serviceName = combatService.ServiceName;

            // Assert
            Assert.AreEqual("CombatService", serviceName);
        }

        [Test]
        public void CombatService_HandlesNullEventManagerGracefully()
        {
            // Arrange - Register CombatService without EventManager
            var combatService = new CombatService();
            ServiceLocator.Register<ICombatService>(combatService);

            var target = new GameObject("Target");

            // Act - Should not throw even without EventManager
            Assert.DoesNotThrow(() =>
            {
                combatService.StartCombat(target, target);
                combatService.EndCombat(target);
            });

            // Cleanup
            Object.DestroyImmediate(target);
        }
    }
}


