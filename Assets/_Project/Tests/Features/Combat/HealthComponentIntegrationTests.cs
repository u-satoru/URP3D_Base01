using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using System.Collections.Generic;
using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Features.Combat.Services;
using asterivo.Unity60.Features.Combat.Interfaces;
using asterivo.Unity60.Features.Combat.Components;
using asterivo.Unity60.Features.Combat;
using asterivo.Unity60.Features.Combat.Events;

namespace asterivo.Unity60.Tests.Features.Combat
{
    /// <summary>
    /// HealthComponentの統合テスト
    /// CombatService、EventManagerとの連携を検証
    /// </summary>
    [TestFixture]
    public class HealthComponentIntegrationTests
    {
        private GameObject testObject;
        private HealthComponent healthComponent;
        private ICombatService combatService;
        private IEventManager eventManager;

        [SetUp]
        public void Setup()
        {
            // ServiceLocatorをクリア
            ServiceLocator.Clear();

            // EventManagerを登録
            eventManager = new EventManager();
            ServiceLocator.Register<IEventManager>(eventManager);

            // CombatServiceを登録
            combatService = new CombatService();
            ServiceLocator.Register<ICombatService>(combatService);

            // テスト用GameObjectを作成
            testObject = new GameObject("TestHealthObject");
            healthComponent = testObject.AddComponent<HealthComponent>();
        }

        [TearDown]
        public void TearDown()
        {
            if (testObject != null)
            {
                Object.DestroyImmediate(testObject);
            }
            ServiceLocator.Clear();
        }

        [UnityTest]
        public IEnumerator HealthComponent_RegistersWithCombatService()
        {
            // Wait for Start() to be called
            yield return null;

            // Assert
            var health = combatService.GetHealth(testObject);
            Assert.IsNotNull(health);
            Assert.AreEqual(healthComponent, health);
        }

        [Test]
        public void HealthComponent_TakeDamage_ReducesHealth()
        {
            // Arrange
            float initialHealth = healthComponent.MaxHealth;
            float damage = 30f;

            // Act
            float actualDamage = healthComponent.TakeDamage(damage);

            // Assert
            Assert.AreEqual(damage, actualDamage);
            Assert.AreEqual(initialHealth - damage, healthComponent.CurrentHealth);
        }

        [Test]
        public void HealthComponent_TakeDamage_WithDamageInfo()
        {
            // Arrange
            var attacker = new GameObject("Attacker");
            var damageInfo = new DamageInfo(25f, attacker, DamageType.Normal);

            // Act
            float actualDamage = healthComponent.TakeDamage(25f, damageInfo);

            // Assert
            Assert.AreEqual(25f, actualDamage);
            Assert.AreEqual(75f, healthComponent.CurrentHealth);

            // Cleanup
            Object.DestroyImmediate(attacker);
        }

        [Test]
        public void HealthComponent_Heal_RestoresHealth()
        {
            // Arrange
            healthComponent.TakeDamage(50f);
            float healthBeforeHeal = healthComponent.CurrentHealth;

            // Act
            float actualHeal = healthComponent.Heal(20f);

            // Assert
            Assert.AreEqual(20f, actualHeal);
            Assert.AreEqual(healthBeforeHeal + 20f, healthComponent.CurrentHealth);
        }

        [Test]
        public void HealthComponent_Heal_CappedAtMaxHealth()
        {
            // Arrange - Start with full health
            float maxHealth = healthComponent.MaxHealth;

            // Act
            float actualHeal = healthComponent.Heal(50f);

            // Assert
            Assert.AreEqual(0f, actualHeal);
            Assert.AreEqual(maxHealth, healthComponent.CurrentHealth);
        }

        [Test]
        public void HealthComponent_Kill_SetsHealthToZero()
        {
            // Act
            healthComponent.Kill();

            // Assert
            Assert.AreEqual(0f, healthComponent.CurrentHealth);
            Assert.IsFalse(healthComponent.IsAlive);
        }

        [Test]
        public void HealthComponent_ResetHealth_RestoresToMax()
        {
            // Arrange
            healthComponent.TakeDamage(50f);

            // Act
            healthComponent.ResetHealth();

            // Assert
            Assert.AreEqual(healthComponent.MaxHealth, healthComponent.CurrentHealth);
            Assert.IsTrue(healthComponent.IsAlive);
        }

        [Test]
        public void HealthComponent_Invulnerability_BlocksDamage()
        {
            // Arrange
            healthComponent.IsInvulnerable = true;
            float initialHealth = healthComponent.CurrentHealth;

            // Act
            float actualDamage = healthComponent.TakeDamage(50f);

            // Assert
            Assert.AreEqual(0f, actualDamage);
            Assert.AreEqual(initialHealth, healthComponent.CurrentHealth);
        }

        [Test]
        public void HealthComponent_GetHealthPercentage()
        {
            // Arrange
            healthComponent.TakeDamage(25f); // 100 - 25 = 75

            // Act
            float percentage = healthComponent.GetHealthPercentage();

            // Assert
            Assert.AreEqual(0.75f, percentage, 0.01f);
        }

        [Test]
        public void HealthComponent_SetMaxHealth_AdjustsCurrentHealth()
        {
            // Arrange
            healthComponent.TakeDamage(50f); // CurrentHealth = 50

            // Act
            healthComponent.SetMaxHealth(200f, true); // Double max health

            // Assert
            Assert.AreEqual(200f, healthComponent.MaxHealth);
            Assert.AreEqual(100f, healthComponent.CurrentHealth); // Proportionally adjusted
        }

        [Test]
        public void HealthComponent_DamageMultiplier_AffectsDamage()
        {
            // Arrange
            healthComponent.SetDamageMultiplier(2f);
            float initialHealth = healthComponent.CurrentHealth;

            // Act
            healthComponent.TakeDamage(10f);

            // Assert
            Assert.AreEqual(initialHealth - 20f, healthComponent.CurrentHealth);
        }

        [Test]
        public void HealthComponent_HealMultiplier_AffectsHealing()
        {
            // Arrange
            healthComponent.TakeDamage(50f);
            healthComponent.SetHealMultiplier(1.5f);
            float healthBeforeHeal = healthComponent.CurrentHealth;

            // Act
            healthComponent.Heal(20f);

            // Assert
            Assert.AreEqual(healthBeforeHeal + 30f, healthComponent.CurrentHealth);
        }

        [Test]
        public void HealthComponent_Revive_RestoresLife()
        {
            // Arrange
            healthComponent.Kill();
            Assert.IsFalse(healthComponent.IsAlive);

            // Act
            healthComponent.Revive(50f);

            // Assert
            Assert.IsTrue(healthComponent.IsAlive);
            Assert.AreEqual(50f, healthComponent.CurrentHealth);
        }

        [Test]
        public void HealthComponent_CannotHealWhenDead()
        {
            // Arrange
            healthComponent.Kill();

            // Act
            float healAmount = healthComponent.Heal(50f);

            // Assert
            Assert.AreEqual(0f, healAmount);
            Assert.AreEqual(0f, healthComponent.CurrentHealth);
        }

        [Test]
        public void HealthComponent_IDamageableInterface_Implementation()
        {
            // Arrange
            IDamageable damageable = healthComponent;
            var damageInfo = new DamageInfo(30f);

            // Act
            Assert.IsTrue(damageable.CanTakeDamage);
            Assert.AreEqual(testObject.transform, damageable.Transform);
            Assert.AreEqual(testObject, damageable.GameObject);

            damageable.TakeDamage(damageInfo);

            // Assert
            Assert.AreEqual(70f, healthComponent.CurrentHealth);
        }
    }
}
