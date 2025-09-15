using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using asterivo.Unity60.Features.Templates.ActionRPG;
using asterivo.Unity60.Features.Templates.ActionRPG.Character;
using asterivo.Unity60.Features.Templates.ActionRPG.Combat;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Services;

namespace asterivo.Unity60.Tests.Integration
{
    /// <summary>
    /// Action RPGテンプレート統合テスト
    /// 全システム間の連携動作を検証
    /// </summary>
    public class ActionRPGTemplateIntegrationTests
    {
        private GameObject templateManagerObject;
        private ActionRPGTemplateManager templateManager;
        private GameObject playerObject;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            // Create template manager
            templateManagerObject = new GameObject("ActionRPGTemplateManager");
            templateManager = templateManagerObject.AddComponent<ActionRPGTemplateManager>();

            // Create player object with required components
            playerObject = new GameObject("Player");
            var characterProgression = playerObject.AddComponent<CharacterProgressionManager>();
            var inventoryManager = playerObject.AddComponent<asterivo.Unity60.Features.Templates.ActionRPG.Equipment.InventoryManager>();
            var equipmentManager = playerObject.AddComponent<asterivo.Unity60.Features.Templates.ActionRPG.Equipment.EquipmentManager>();
            var combatManager = playerObject.AddComponent<CombatManager>();
            var health = playerObject.AddComponent<Health>();
            var statusEffectManager = playerObject.AddComponent<StatusEffectManager>();

            // Assign references
            templateManager.SetCharacterProgressionManager(characterProgression);
            templateManager.SetInventoryManager(inventoryManager);
            templateManager.SetEquipmentManager(equipmentManager);
            templateManager.SetCombatManager(combatManager);

            // Wait for initialization
            yield return new WaitForSeconds(0.5f);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (templateManagerObject != null)
                Object.DestroyImmediate(templateManagerObject);
            if (playerObject != null)
                Object.DestroyImmediate(playerObject);

            yield return null;
        }

        /// <summary>
        /// テンプレートマネージャーの基本初期化テスト
        /// </summary>
        [UnityTest]
        public IEnumerator TemplateManager_InitializesAllSystems()
        {
            // Given: Template manager with all systems
            yield return new WaitForSeconds(1.0f);

            // When: Template initializes
            // Then: All systems should be ready
            var characterProgression = playerObject.GetComponent<CharacterProgressionManager>();
            var inventory = playerObject.GetComponent<asterivo.Unity60.Features.Templates.ActionRPG.Equipment.InventoryManager>();
            var equipment = playerObject.GetComponent<asterivo.Unity60.Features.Templates.ActionRPG.Equipment.EquipmentManager>();
            var combat = playerObject.GetComponent<CombatManager>();

            Assert.IsNotNull(characterProgression, "Character Progression Manager should be initialized");
            Assert.IsNotNull(inventory, "Inventory Manager should be initialized");
            Assert.IsNotNull(equipment, "Equipment Manager should be initialized");
            Assert.IsNotNull(combat, "Combat Manager should be initialized");

            // Test Service Locator registration
            Assert.IsNotNull(ServiceLocator.GetService<asterivo.Unity60.Features.Templates.ActionRPG.Equipment.InventoryManager>(),
                "Inventory Manager should be registered with ServiceLocator");
            Assert.IsNotNull(ServiceLocator.GetService<asterivo.Unity60.Features.Templates.ActionRPG.Equipment.EquipmentManager>(),
                "Equipment Manager should be registered with ServiceLocator");
        }

        /// <summary>
        /// キャラクター成長とステータス連携テスト
        /// </summary>
        [UnityTest]
        public IEnumerator CharacterProgression_IntegratesWithStats()
        {
            // Given: Initialized character progression system
            var characterProgression = playerObject.GetComponent<CharacterProgressionManager>();
            var health = playerObject.GetComponent<Health>();

            yield return new WaitForSeconds(0.5f);

            // When: Character levels up
            var initialLevel = characterProgression.CurrentLevel;
            var initialMaxHealth = health.MaxHealth;

            characterProgression.AddExperience(1000); // Should trigger level up

            yield return new WaitForSeconds(0.1f);

            // Then: Stats should be updated accordingly
            Assert.Greater(characterProgression.CurrentLevel, initialLevel,
                "Character should level up after gaining experience");
            Assert.GreaterOrEqual(health.MaxHealth, initialMaxHealth,
                "Max health should increase or stay same after level up");
        }

        /// <summary>
        /// 装備・インベントリ・ステータス統合テスト
        /// </summary>
        [UnityTest]
        public IEnumerator Equipment_IntegratesWithInventoryAndStats()
        {
            var inventory = playerObject.GetComponent<asterivo.Unity60.Features.Templates.ActionRPG.Equipment.InventoryManager>();
            var equipment = playerObject.GetComponent<asterivo.Unity60.Features.Templates.ActionRPG.Equipment.EquipmentManager>();
            var health = playerObject.GetComponent<Health>();

            yield return new WaitForSeconds(0.5f);

            // Create test weapon with stats
            var testWeapon = CreateTestWeapon();
            var initialMaxHealth = health.MaxHealth;

            // When: Add weapon to inventory and equip it
            var addedCount = inventory.AddItem(testWeapon, 1);
            Assert.AreEqual(1, addedCount, "Weapon should be added to inventory");

            bool equipped = equipment.EquipItem(testWeapon);
            Assert.IsTrue(equipped, "Weapon should be equipped successfully");

            yield return new WaitForSeconds(0.1f);

            // Then: Item should be removed from inventory and stats should change
            Assert.AreEqual(0, inventory.GetItemCount(testWeapon),
                "Weapon should be removed from inventory after equipping");

            var equippedWeapon = equipment.GetEquippedItem(testWeapon.equipmentSlot);
            Assert.AreEqual(testWeapon, equippedWeapon, "Weapon should be equipped in correct slot");

            // Test stat bonuses from equipment
            var equipmentStats = equipment.TotalEquipmentStats;
            Assert.Greater(equipmentStats.attackPower, 0, "Equipment should provide attack power bonus");
        }

        /// <summary>
        /// 戦闘システム統合テスト
        /// </summary>
        [UnityTest]
        public IEnumerator Combat_IntegratesWithHealthAndStatusEffects()
        {
            var combat = playerObject.GetComponent<CombatManager>();
            var health = playerObject.GetComponent<Health>();
            var statusEffects = playerObject.GetComponent<StatusEffectManager>();

            yield return new WaitForSeconds(0.5f);

            var initialHealth = health.CurrentHealth;
            var testDamage = 20;

            // When: Take damage through combat system
            health.TakeDamage(testDamage, playerObject);

            yield return new WaitForSeconds(0.1f);

            // Then: Health should be reduced
            Assert.Less(health.CurrentHealth, initialHealth, "Health should be reduced after taking damage");
            Assert.AreEqual(initialHealth - testDamage, health.CurrentHealth,
                "Health should be reduced by exact damage amount");

            // Test status effect integration
            var testStatusEffect = CreateTestStatusEffect();
            bool applied = statusEffects.ApplyStatusEffect(testStatusEffect);
            Assert.IsTrue(applied, "Status effect should be applied successfully");

            yield return new WaitForSeconds(0.1f);

            // Verify status effect is active
            Assert.IsTrue(statusEffects.HasEffect(testStatusEffect.effectName),
                "Applied status effect should be active");
        }

        /// <summary>
        /// 経験値獲得から装備強化までの完全フローテスト
        /// </summary>
        [UnityTest]
        public IEnumerator CompleteProgression_ExperienceToEquipmentFlow()
        {
            var characterProgression = playerObject.GetComponent<CharacterProgressionManager>();
            var inventory = playerObject.GetComponent<asterivo.Unity60.Features.Templates.ActionRPG.Equipment.InventoryManager>();
            var equipment = playerObject.GetComponent<asterivo.Unity60.Features.Templates.ActionRPG.Equipment.EquipmentManager>();

            yield return new WaitForSeconds(0.5f);

            // Given: Starting state
            var initialLevel = characterProgression.CurrentLevel;

            // When: Complete progression flow
            // 1. Gain experience and level up
            characterProgression.AddExperience(2000);
            yield return new WaitForSeconds(0.1f);

            // 2. Get better equipment
            var betterWeapon = CreateTestWeapon();
            betterWeapon.stats.attackPower = 50; // Higher than basic weapon

            inventory.AddItem(betterWeapon, 1);
            equipment.EquipItem(betterWeapon);
            yield return new WaitForSeconds(0.1f);

            // Then: Character should be stronger
            Assert.Greater(characterProgression.CurrentLevel, initialLevel,
                "Character should have leveled up");
            Assert.Greater(equipment.TotalEquipmentStats.attackPower, 0,
                "Equipment should provide attack power");

            LogDebug($"Character progression test completed - Level: {characterProgression.CurrentLevel}, " +
                    $"Attack Power: {equipment.TotalEquipmentStats.attackPower}");
        }

        /// <summary>
        /// パフォーマンステスト - 大量アイテム処理
        /// </summary>
        [UnityTest]
        public IEnumerator Performance_LargeInventoryOperations()
        {
            var inventory = playerObject.GetComponent<asterivo.Unity60.Features.Templates.ActionRPG.Equipment.InventoryManager>();

            yield return new WaitForSeconds(0.5f);

            var startTime = Time.realtimeSinceStartup;

            // Add many items
            var testItem = CreateTestConsumable();
            for (int i = 0; i < 100; i++)
            {
                inventory.AddItem(testItem, 10);
                if (i % 10 == 0) yield return null; // Prevent frame drops
            }

            var endTime = Time.realtimeSinceStartup;
            var elapsedTime = endTime - startTime;

            Assert.Less(elapsedTime, 1.0f, "Large inventory operations should complete within 1 second");
            Assert.Greater(inventory.TotalItemCount, 900, "Should have added most items successfully");

            LogDebug($"Performance test completed - Added 1000 items in {elapsedTime:F3} seconds");
        }

        #region Helper Methods

        private asterivo.Unity60.Features.Templates.ActionRPG.Equipment.ItemData CreateTestWeapon()
        {
            var weapon = ScriptableObject.CreateInstance<asterivo.Unity60.Features.Templates.ActionRPG.Equipment.ItemData>();
            weapon.itemName = "Test Sword";
            weapon.itemType = asterivo.Unity60.Features.Templates.ActionRPG.Equipment.ItemType.Weapon;
            weapon.equipmentSlot = asterivo.Unity60.Features.Templates.ActionRPG.Equipment.EquipmentSlot.MainHand;
            weapon.maxStackSize = 1;
            weapon.requiredLevel = 1;
            weapon.stats = new asterivo.Unity60.Features.Templates.ActionRPG.Equipment.EquipmentStats
            {
                attackPower = 25,
                defense = 0,
                strengthBonus = 2,
                dexterityBonus = 1
            };
            return weapon;
        }

        private asterivo.Unity60.Features.Templates.ActionRPG.Equipment.ItemData CreateTestConsumable()
        {
            var consumable = ScriptableObject.CreateInstance<asterivo.Unity60.Features.Templates.ActionRPG.Equipment.ItemData>();
            consumable.itemName = "Test Potion";
            consumable.itemType = asterivo.Unity60.Features.Templates.ActionRPG.Equipment.ItemType.Consumable;
            consumable.maxStackSize = 50;
            return consumable;
        }

        private StatusEffectData CreateTestStatusEffect()
        {
            var effect = ScriptableObject.CreateInstance<StatusEffectData>();
            effect.effectName = "Test Buff";
            effect.effectType = StatusEffectType.StatModifier;
            effect.magnitude = 10f;
            effect.duration = 5f;
            return effect;
        }

        private void LogDebug(string message)
        {
            Debug.Log($"[ActionRPG Integration Test] {message}");
        }

        #endregion
    }

    // Extension methods for easier testing
    public static class ActionRPGTemplateManagerExtensions
    {
        public static void SetCharacterProgressionManager(this ActionRPGTemplateManager manager, CharacterProgressionManager characterProgression)
        {
            var field = typeof(ActionRPGTemplateManager).GetField("characterProgressionManager",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(manager, characterProgression);
        }

        public static void SetInventoryManager(this ActionRPGTemplateManager manager, asterivo.Unity60.Features.Templates.ActionRPG.Equipment.InventoryManager inventory)
        {
            var field = typeof(ActionRPGTemplateManager).GetField("inventoryManager",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(manager, inventory);
        }

        public static void SetEquipmentManager(this ActionRPGTemplateManager manager, asterivo.Unity60.Features.Templates.ActionRPG.Equipment.EquipmentManager equipment)
        {
            var field = typeof(ActionRPGTemplateManager).GetField("equipmentManager",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(manager, equipment);
        }

        public static void SetCombatManager(this ActionRPGTemplateManager manager, CombatManager combat)
        {
            var field = typeof(ActionRPGTemplateManager).GetField("combatManager",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(manager, combat);
        }
    }
}
