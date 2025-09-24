using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using asterivo.Unity60.Core.Commands;
using asterivo.Unity60.Core.Combat;
using asterivo.Unity60.Core.Components;

namespace asterivo.Unity60.Tests.Core.Commands
{
    /// <summary>
    /// Integration test for TASK-010.2: Generic Health & Damage System
    /// Verifies that DamageCommand and HealCommand work correctly with HealthComponent
    /// and that events are properly triggered
    /// </summary>
    public class HealthCommandIntegrationTest
    {
        private GameObject _testObject;
        private HealthComponent _healthComponent;
        private DamageCommand _damageCommand;
        private HealCommand _healCommand;

        // Event tracking
        private bool _healthChangedEventFired;
        private bool _deathEventFired;
        private float _lastCurrentHealth;
        private float _lastMaxHealth;

        [SetUp]
        public void Setup()
        {
            // Create test GameObject with HealthComponent
            _testObject = new GameObject("TestHealthTarget");
            _healthComponent = _testObject.AddComponent<HealthComponent>();

            // Initialize with test values
            _healthComponent.Initialize(100f);

            // Subscribe to events for verification
            _healthComponent.OnHealthChanged += OnHealthChanged;
            _healthComponent.OnDied += OnDied;

            // Initialize commands
            _damageCommand = new DamageCommand();
            _healCommand = new HealCommand();

            // Reset event flags
            ResetEventFlags();
        }

        [TearDown]
        public void TearDown()
        {
            // Unsubscribe from events
            if (_healthComponent != null)
            {
                _healthComponent.OnHealthChanged -= OnHealthChanged;
                _healthComponent.OnDied -= OnDied;
            }

            // Clean up
            if (_testObject != null)
            {
                Object.DestroyImmediate(_testObject);
            }
        }

        private void ResetEventFlags()
        {
            _healthChangedEventFired = false;
            _deathEventFired = false;
            _lastCurrentHealth = 0f;
            _lastMaxHealth = 0f;
        }

        private void OnHealthChanged(float currentHealth, float maxHealth)
        {
            _healthChangedEventFired = true;
            _lastCurrentHealth = currentHealth;
            _lastMaxHealth = maxHealth;
        }

        private void OnDied()
        {
            _deathEventFired = true;
        }

        [Test]
        public void DamageCommand_Execute_ReducesHealthAndTriggersEvent()
        {
            // Arrange
            _damageCommand.Initialize(_healthComponent, 25);
            float initialHealth = _healthComponent.CurrentHealthFloat;

            // Act
            _damageCommand.Execute();

            // Assert
            Assert.AreEqual(initialHealth - 25f, _healthComponent.CurrentHealthFloat, 0.01f,
                "DamageCommand should reduce health by correct amount");
            Assert.IsTrue(_healthChangedEventFired, "OnHealthChanged event should be fired");
            Assert.AreEqual(_healthComponent.CurrentHealthFloat, _lastCurrentHealth, 0.01f,
                "Event should carry correct current health value");
            Assert.IsFalse(_deathEventFired, "Death event should not fire for non-lethal damage");
        }

        [Test]
        public void DamageCommand_ExecuteLethalDamage_TriggersDeathEvent()
        {
            // Arrange
            _damageCommand.Initialize(_healthComponent, 150); // More than max health

            // Act
            _damageCommand.Execute();

            // Assert
            Assert.AreEqual(0f, _healthComponent.CurrentHealthFloat, 0.01f,
                "Health should be clamped to 0 after lethal damage");
            Assert.IsTrue(_healthChangedEventFired, "OnHealthChanged event should be fired");
            Assert.IsTrue(_deathEventFired, "OnDied event should be fired for lethal damage");
            Assert.IsTrue(_healthComponent.IsDead, "IsDead property should return true");
        }

        [Test]
        public void HealCommand_Execute_IncreasesHealthAndTriggersEvent()
        {
            // Arrange - First damage the target
            _healthComponent.TakeDamage(50f);
            ResetEventFlags();

            _healCommand.Initialize(_healthComponent, 25);
            float damagedHealth = _healthComponent.CurrentHealthFloat;

            // Act
            _healCommand.Execute();

            // Assert
            Assert.AreEqual(damagedHealth + 25f, _healthComponent.CurrentHealthFloat, 0.01f,
                "HealCommand should increase health by correct amount");
            Assert.IsTrue(_healthChangedEventFired, "OnHealthChanged event should be fired");
            Assert.AreEqual(_healthComponent.CurrentHealthFloat, _lastCurrentHealth, 0.01f,
                "Event should carry correct current health value");
        }

        [Test]
        public void HealCommand_ExecuteOverHeal_ClampsToMaxHealth()
        {
            // Arrange
            _healCommand.Initialize(_healthComponent, 50); // Try to heal above max
            float maxHealth = _healthComponent.MaxHealthFloat;

            // Act
            _healCommand.Execute();

            // Assert
            Assert.AreEqual(maxHealth, _healthComponent.CurrentHealthFloat, 0.01f,
                "Health should be clamped to max health");
            Assert.IsTrue(_healthChangedEventFired, "OnHealthChanged event should be fired");
        }

        [Test]
        public void DamageCommand_Undo_RestoresHealthCorrectly()
        {
            // Arrange
            _damageCommand.Initialize(_healthComponent, 30);
            float initialHealth = _healthComponent.CurrentHealthFloat;

            // Act
            _damageCommand.Execute();
            ResetEventFlags();
            _damageCommand.Undo();

            // Assert
            Assert.AreEqual(initialHealth, _healthComponent.CurrentHealthFloat, 0.01f,
                "Undo should restore health to original value");
            Assert.IsTrue(_healthChangedEventFired, "OnHealthChanged event should be fired during undo");
        }

        [Test]
        public void HealCommand_Undo_RemovesHealthCorrectly()
        {
            // Arrange - Damage first, then heal
            _healthComponent.TakeDamage(50f);
            float damagedHealth = _healthComponent.CurrentHealthFloat;
            _healCommand.Initialize(_healthComponent, 25);
            _healCommand.Execute();
            ResetEventFlags();

            // Act
            _healCommand.Undo();

            // Assert
            Assert.AreEqual(damagedHealth, _healthComponent.CurrentHealthFloat, 0.01f,
                "Undo should remove the healed amount");
            Assert.IsTrue(_healthChangedEventFired, "OnHealthChanged event should be fired during undo");
        }

        [Test]
        public void IHealthTarget_IntProperties_MatchFloatValues()
        {
            // Arrange
            _healthComponent.Initialize(100f);
            _healthComponent.TakeDamage(25.3f); // Float damage

            // Act & Assert
            Assert.AreEqual(Mathf.RoundToInt(_healthComponent.CurrentHealthFloat), _healthComponent.CurrentHealth,
                "IHealthTarget.CurrentHealth should match rounded float value");
            Assert.AreEqual(Mathf.RoundToInt(_healthComponent.MaxHealthFloat), _healthComponent.MaxHealth,
                "IHealthTarget.MaxHealth should match rounded float value");
        }

        [Test]
        public void IHealthTarget_ElementalDamage_WorksCorrectly()
        {
            // Arrange
            IHealthTarget target = _healthComponent;
            float initialHealth = _healthComponent.CurrentHealthFloat;

            // Act
            target.TakeDamage(20, "fire");

            // Assert
            Assert.AreEqual(initialHealth - 20f, _healthComponent.CurrentHealthFloat, 0.01f,
                "Elemental damage should work through IHealthTarget interface");
            Assert.IsTrue(_healthChangedEventFired, "OnHealthChanged event should be fired");
        }

        [Test]
        public void Commands_CanBeUsedWithObjectPoolPattern()
        {
            // Arrange
            _damageCommand.Reset();
            _healCommand.Reset();

            // Act & Assert - Commands should be resettable
            Assert.DoesNotThrow(() => _damageCommand.Initialize(_healthComponent, 10));
            Assert.DoesNotThrow(() => _healCommand.Initialize(_healthComponent, 10));
            Assert.DoesNotThrow(() => _damageCommand.Execute());
            Assert.DoesNotThrow(() => _healCommand.Execute());
        }
    }
}
