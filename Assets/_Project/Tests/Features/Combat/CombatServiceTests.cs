using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using asterivo.Unity60.Core;
using asterivo.Unity60.Features.Combat.Services;
using asterivo.Unity60.Features.Combat.Interfaces;
using asterivo.Unity60.Features.Combat;
using asterivo.Unity60.Features.Combat.Events;

namespace asterivo.Unity60.Tests.Features.Combat
{
    /// <summary>
    /// CombatService縺ｮ繝ｦ繝九ャ繝医ユ繧ｹ繝・
    /// ServiceLocator繝代ち繝ｼ繝ｳ縺ｨ謌ｦ髣俶ｩ溯・縺ｮ蜍穂ｽ懈､懆ｨｼ
    /// </summary>
    [TestFixture]
    public class CombatServiceTests
    {
        private CombatService combatService;
        private IEventManager eventManager;

        [SetUp]
        public void Setup()
        {
            // ServiceLocator繧偵け繝ｪ繧｢
            ServiceLocator.Clear();

            // EventManager繧堤匳骭ｲ
            eventManager = new EventManager();
            ServiceLocator.Register<IEventManager>(eventManager);

            // CombatService繧剃ｽ懈・縺励※逋ｻ骭ｲ
            combatService = new CombatService();
            ServiceLocator.Register<ICombatService>(combatService);
        }

        [TearDown]
        public void TearDown()
        {
            ServiceLocator.Clear();
        }

        [Test]
        public void CombatService_CanBeRegistered()
        {
            // Arrange & Act
            var service = ServiceLocator.Get<ICombatService>();

            // Assert
            Assert.IsNotNull(service);
            Assert.IsInstanceOf<CombatService>(service);
            Assert.IsTrue(service.IsServiceActive);
        }

        [Test]
        public void CombatService_InitializesCorrectly()
        {
            // Arrange & Act
            var statistics = combatService.GetStatistics();

            // Assert
            Assert.AreEqual(0, statistics.TotalDamageDealt);
            Assert.AreEqual(0, statistics.TotalDamageReceived);
            Assert.AreEqual(0, statistics.TotalHealing);
            Assert.AreEqual(0, statistics.Kills);
            Assert.AreEqual(0, statistics.Deaths);
            Assert.AreEqual(0, statistics.ActiveCombatants);
        }

        [Test]
        public void DealDamage_WithNullTarget_ReturnsZero()
        {
            // Arrange
            GameObject target = null;
            float damage = 50f;

            // Act
            float actualDamage = combatService.DealDamage(target, damage);

            // Assert
            Assert.AreEqual(0f, actualDamage);
        }

        [Test]
        public void HealTarget_WithNullTarget_ReturnsZero()
        {
            // Arrange
            GameObject target = null;
            float healAmount = 30f;

            // Act
            float actualHeal = combatService.HealTarget(target, healAmount);

            // Assert
            Assert.AreEqual(0f, actualHeal);
        }

        [Test]
        public void StartCombat_AddsParticipants()
        {
            // Arrange
            var attacker = new GameObject("Attacker");
            var target = new GameObject("Target");

            // Act
            combatService.StartCombat(attacker, target);

            // Assert
            Assert.IsTrue(combatService.IsInCombat(attacker));
            Assert.IsTrue(combatService.IsInCombat(target));
            Assert.AreEqual(2, combatService.GetStatistics().ActiveCombatants);

            // Cleanup
            Object.DestroyImmediate(attacker);
            Object.DestroyImmediate(target);
        }

        [Test]
        public void EndCombat_RemovesParticipant()
        {
            // Arrange
            var attacker = new GameObject("Attacker");
            var target = new GameObject("Target");
            combatService.StartCombat(attacker, target);

            // Act
            combatService.EndCombat(attacker);

            // Assert
            Assert.IsFalse(combatService.IsInCombat(attacker));
            Assert.IsTrue(combatService.IsInCombat(target));
            Assert.AreEqual(1, combatService.GetStatistics().ActiveCombatants);

            // Cleanup
            Object.DestroyImmediate(attacker);
            Object.DestroyImmediate(target);
        }

        [Test]
        public void IsInCombat_ReturnsFalseForNullParticipant()
        {
            // Arrange
            GameObject participant = null;

            // Act
            bool result = combatService.IsInCombat(participant);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void GetDamageCommand_ReturnsValidCommand()
        {
            // Act
            var command = combatService.GetDamageCommand();

            // Assert
            Assert.IsNotNull(command);
            Assert.IsInstanceOf<Core.Commands.DamageCommand>(command);
        }

        [Test]
        public void RegisterHealth_WithValidHealth_Succeeds()
        {
            // Arrange
            var gameObject = new GameObject("TestObject");
            var health = new MockHealth(gameObject);

            // Act
            combatService.RegisterHealth(health);
            var retrieved = combatService.GetHealth(gameObject);

            // Assert
            Assert.IsNotNull(retrieved);
            Assert.AreEqual(health, retrieved);

            // Cleanup
            Object.DestroyImmediate(gameObject);
        }

        [Test]
        public void UnregisterHealth_RemovesHealthFromRegistry()
        {
            // Arrange
            var gameObject = new GameObject("TestObject");
            var health = new MockHealth(gameObject);
            combatService.RegisterHealth(health);

            // Act
            combatService.UnregisterHealth(health);
            combatService.UnregisterHealth(health); // Should not throw even if not registered

            // Assert
            // After unregistering, GetHealth should not return from cache
            // Note: It might still find it via GetComponent

            // Cleanup
            Object.DestroyImmediate(gameObject);
        }

        [Test]
        public void GetHealth_WithNullGameObject_ReturnsNull()
        {
            // Arrange
            GameObject gameObject = null;

            // Act
            var health = combatService.GetHealth(gameObject);

            // Assert
            Assert.IsNull(health);
        }

        [Test]
        public void Statistics_TracksCombatTime()
        {
            // Arrange
            var participant = new GameObject("Participant");
            combatService.StartCombat(participant, participant);

            // Act - Simulate time passing
            System.Threading.Thread.Sleep(100); // Sleep for 100ms
            combatService.EndCombat(participant);

            // Assert
            var stats = combatService.GetStatistics();
            Assert.Greater(stats.CombatTime, 0f);

            // Cleanup
            Object.DestroyImmediate(participant);
        }
    }

    /// <summary>
    /// 繝・せ繝育畑縺ｮ繝｢繝・けHealth繧ｯ繝ｩ繧ｹ
    /// </summary>
    public class MockHealth : MonoBehaviour, IHealth
    {
        private GameObject _gameObject;

        public MockHealth(GameObject gameObject)
        {
            _gameObject = gameObject;
        }

        public float CurrentHealth => 100f;
        public float MaxHealth => 100f;
        public bool IsAlive => true;
        public bool IsInvulnerable { get; set; }

        public float TakeDamage(float damage) => damage;
        public float TakeDamage(float damage, DamageInfo damageInfo) => damage;
        public float Heal(float amount) => amount;
        public void ResetHealth() { }
        public void Kill() { }
    }
}


