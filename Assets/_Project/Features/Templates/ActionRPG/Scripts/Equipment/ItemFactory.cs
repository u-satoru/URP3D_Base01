using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Features.Templates.ActionRPG.Equipment
{
    /// <summary>
    /// Action RPGテンプレート用アイテムファクトリー
    /// サンプルアイテムの生成とアイテムデータベースの管理を行う
    /// </summary>
    [CreateAssetMenu(fileName = "ItemFactory", menuName = "Action RPG/Equipment/Item Factory")]
    public class ItemFactory : ScriptableObject
    {
        [BoxGroup("Configuration")]
        [SerializeField]
        private bool debugMode = false;

        [BoxGroup("Item Database")]
        [SerializeField]
        private List<ItemData> weapons = new List<ItemData>();

        [BoxGroup("Item Database")]
        [SerializeField]
        private List<ItemData> armors = new List<ItemData>();

        [BoxGroup("Item Database")]
        [SerializeField]
        private List<ItemData> consumables = new List<ItemData>();

        [BoxGroup("Item Database")]
        [SerializeField]
        private List<ItemData> materials = new List<ItemData>();

        [BoxGroup("Statistics")]
        [ShowInInspector, ReadOnly]
        public int TotalItems => weapons.Count + armors.Count + consumables.Count + materials.Count;

        [BoxGroup("Statistics")]
        [ShowInInspector, ReadOnly]
        public int WeaponCount => weapons.Count;

        [BoxGroup("Statistics")]
        [ShowInInspector, ReadOnly]
        public int ArmorCount => armors.Count;

        [BoxGroup("Statistics")]
        [ShowInInspector, ReadOnly]
        public int ConsumableCount => consumables.Count;

        [BoxGroup("Statistics")]
        [ShowInInspector, ReadOnly]
        public int MaterialCount => materials.Count;

        /// <summary>
        /// 武器を取得
        /// </summary>
        public IReadOnlyList<ItemData> GetWeapons() => weapons.AsReadOnly();

        /// <summary>
        /// 防具を取得
        /// </summary>
        public IReadOnlyList<ItemData> GetArmors() => armors.AsReadOnly();

        /// <summary>
        /// 消耗品を取得
        /// </summary>
        public IReadOnlyList<ItemData> GetConsumables() => consumables.AsReadOnly();

        /// <summary>
        /// 素材を取得
        /// </summary>
        public IReadOnlyList<ItemData> GetMaterials() => materials.AsReadOnly();

        /// <summary>
        /// 全アイテムを取得
        /// </summary>
        public List<ItemData> GetAllItems()
        {
            var allItems = new List<ItemData>();
            allItems.AddRange(weapons);
            allItems.AddRange(armors);
            allItems.AddRange(consumables);
            allItems.AddRange(materials);
            return allItems;
        }

        /// <summary>
        /// アイテムタイプで検索
        /// </summary>
        public List<ItemData> GetItemsByType(ItemType itemType)
        {
            return itemType switch
            {
                ItemType.Weapon => new List<ItemData>(weapons),
                ItemType.Armor => new List<ItemData>(armors),
                ItemType.Consumable => new List<ItemData>(consumables),
                ItemType.Material => new List<ItemData>(materials),
                _ => new List<ItemData>()
            };
        }

        /// <summary>
        /// レアリティで検索
        /// </summary>
        public List<ItemData> GetItemsByRarity(ItemRarity rarity)
        {
            var result = new List<ItemData>();
            var allItems = GetAllItems();

            foreach (var item in allItems)
            {
                if (item.rarity == rarity)
                {
                    result.Add(item);
                }
            }

            return result;
        }

        /// <summary>
        /// レベル範囲で装備を検索
        /// </summary>
        public List<ItemData> GetEquipmentByLevelRange(int minLevel, int maxLevel)
        {
            var result = new List<ItemData>();
            var equipment = new List<ItemData>();
            equipment.AddRange(weapons);
            equipment.AddRange(armors);

            foreach (var item in equipment)
            {
                if (item.requiredLevel >= minLevel && item.requiredLevel <= maxLevel)
                {
                    result.Add(item);
                }
            }

            return result;
        }

        /// <summary>
        /// ランダムアイテムを取得
        /// </summary>
        public ItemData GetRandomItem()
        {
            var allItems = GetAllItems();
            if (allItems.Count == 0)
                return null;

            int randomIndex = Random.Range(0, allItems.Count);
            return allItems[randomIndex];
        }

        /// <summary>
        /// ランダム装備を取得
        /// </summary>
        public ItemData GetRandomEquipment()
        {
            var equipment = new List<ItemData>();
            equipment.AddRange(weapons);
            equipment.AddRange(armors);

            if (equipment.Count == 0)
                return null;

            int randomIndex = Random.Range(0, equipment.Count);
            return equipment[randomIndex];
        }

        /// <summary>
        /// レアリティ重み付きランダムアイテム取得
        /// </summary>
        public ItemData GetWeightedRandomItem()
        {
            var allItems = GetAllItems();
            if (allItems.Count == 0)
                return null;

            // レアリティによる重み付け
            var weightedItems = new List<(ItemData item, float weight)>();

            foreach (var item in allItems)
            {
                float weight = item.rarity switch
                {
                    ItemRarity.Common => 1.0f,
                    ItemRarity.Uncommon => 0.7f,
                    ItemRarity.Rare => 0.4f,
                    ItemRarity.Epic => 0.2f,
                    ItemRarity.Legendary => 0.05f,
                    _ => 1.0f
                };
                weightedItems.Add((item, weight));
            }

            // 重み付きランダム選択
            float totalWeight = 0f;
            foreach (var (item, weight) in weightedItems)
            {
                totalWeight += weight;
            }

            float randomValue = Random.Range(0f, totalWeight);
            float currentWeight = 0f;

            foreach (var (item, weight) in weightedItems)
            {
                currentWeight += weight;
                if (randomValue <= currentWeight)
                {
                    return item;
                }
            }

            return allItems[0]; // フォールバック
        }

        #region Sample Item Generation

        /// <summary>
        /// サンプル武器を生成
        /// </summary>
        [Button("Generate Sample Weapons")]
        [ShowIf("debugMode")]
        public void GenerateSampleWeapons()
        {
            weapons.Clear();
            weapons.AddRange(CreateSampleWeapons());
            LogDebug($"[ItemFactory] Generated {weapons.Count} sample weapons");
        }

        /// <summary>
        /// サンプル防具を生成
        /// </summary>
        [Button("Generate Sample Armors")]
        [ShowIf("debugMode")]
        public void GenerateSampleArmors()
        {
            armors.Clear();
            armors.AddRange(CreateSampleArmors());
            LogDebug($"[ItemFactory] Generated {armors.Count} sample armors");
        }

        /// <summary>
        /// サンプル消耗品を生成
        /// </summary>
        [Button("Generate Sample Consumables")]
        [ShowIf("debugMode")]
        public void GenerateSampleConsumables()
        {
            consumables.Clear();
            consumables.AddRange(CreateSampleConsumables());
            LogDebug($"[ItemFactory] Generated {consumables.Count} sample consumables");
        }

        /// <summary>
        /// 全サンプルアイテムを生成
        /// </summary>
        [Button("Generate All Sample Items")]
        [ShowIf("debugMode")]
        public void GenerateAllSampleItems()
        {
            GenerateSampleWeapons();
            GenerateSampleArmors();
            GenerateSampleConsumables();
            LogDebug($"[ItemFactory] Generated {TotalItems} total sample items");
        }

        private List<ItemData> CreateSampleWeapons()
        {
            var sampleWeapons = new List<ItemData>();

            // 基本的な武器のサンプル
            var weaponTemplates = new[]
            {
                ("Iron Sword", ItemRarity.Common, 1, 15, 0),
                ("Steel Sword", ItemRarity.Uncommon, 5, 25, 0),
                ("Silver Sword", ItemRarity.Rare, 10, 40, 0),
                ("Mystic Blade", ItemRarity.Epic, 15, 60, 20),
                ("Dragon Slayer", ItemRarity.Legendary, 20, 100, 50)
            };

            foreach (var (name, rarity, level, attack, magic) in weaponTemplates)
            {
                var weapon = CreateWeaponData(name, rarity, level, attack, magic);
                sampleWeapons.Add(weapon);
            }

            return sampleWeapons;
        }

        private List<ItemData> CreateSampleArmors()
        {
            var sampleArmors = new List<ItemData>();

            // ヘルメット
            var helmetData = CreateArmorData("Iron Helmet", ItemRarity.Common, 1, 5, 5, EquipmentSlot.Helmet);
            sampleArmors.Add(helmetData);

            // チェストアーマー
            var chestData = CreateArmorData("Steel Chestplate", ItemRarity.Uncommon, 5, 15, 10, EquipmentSlot.Chest);
            sampleArmors.Add(chestData);

            // レッグアーマー
            var legData = CreateArmorData("Silver Leggings", ItemRarity.Rare, 10, 20, 15, EquipmentSlot.Legs);
            sampleArmors.Add(legData);

            return sampleArmors;
        }

        private List<ItemData> CreateSampleConsumables()
        {
            var sampleConsumables = new List<ItemData>();

            // HP回復ポーション
            var healthPotion = CreateConsumableData("Health Potion", ItemRarity.Common, 50,
                new ConsumableEffect[]
                {
                    new ConsumableEffect
                    {
                        effectType = ConsumableEffectType.HealHP,
                        value = 50,
                        duration = 0f,
                        description = "Restores 50 HP instantly"
                    }
                });
            sampleConsumables.Add(healthPotion);

            // MP回復ポーション
            var manaPotion = CreateConsumableData("Mana Potion", ItemRarity.Common, 30,
                new ConsumableEffect[]
                {
                    new ConsumableEffect
                    {
                        effectType = ConsumableEffectType.HealMP,
                        value = 30,
                        duration = 0f,
                        description = "Restores 30 MP instantly"
                    }
                });
            sampleConsumables.Add(manaPotion);

            return sampleConsumables;
        }

        private ItemData CreateWeaponData(string itemName, ItemRarity rarity, int requiredLevel, int attackPower, int magicPower)
        {
            var weapon = ScriptableObject.CreateInstance<ItemData>();
            weapon.itemName = itemName;
            weapon.itemType = ItemType.Weapon;
            weapon.rarity = rarity;
            weapon.requiredLevel = requiredLevel;
            weapon.equipmentSlot = EquipmentSlot.MainHand;
            weapon.stats.attackPower = attackPower;
            weapon.stats.magicPower = magicPower;
            weapon.value = attackPower * 2;
            weapon.description = $"A {rarity.ToString().ToLower()} weapon with {attackPower} attack power.";
            return weapon;
        }

        private ItemData CreateArmorData(string itemName, ItemRarity rarity, int requiredLevel, int defense, int magicDefense, EquipmentSlot slot)
        {
            var armor = ScriptableObject.CreateInstance<ItemData>();
            armor.itemName = itemName;
            armor.itemType = ItemType.Armor;
            armor.rarity = rarity;
            armor.requiredLevel = requiredLevel;
            armor.equipmentSlot = slot;
            armor.stats.defense = defense;
            armor.stats.magicDefense = magicDefense;
            armor.value = (defense + magicDefense) * 2;
            armor.description = $"A {rarity.ToString().ToLower()} {slot.ToString().ToLower()} with {defense} defense.";
            return armor;
        }

        private ItemData CreateConsumableData(string itemName, ItemRarity rarity, int value, ConsumableEffect[] effects)
        {
            var consumable = ScriptableObject.CreateInstance<ItemData>();
            consumable.itemName = itemName;
            consumable.itemType = ItemType.Consumable;
            consumable.rarity = rarity;
            consumable.value = value;
            consumable.maxStackSize = 10;
            consumable.consumableEffects = effects;
            consumable.description = $"A {rarity.ToString().ToLower()} consumable item.";
            return consumable;
        }

        #endregion

        #region Debug Support

        [Button("Clear All Items")]
        [ShowIf("debugMode")]
        public void ClearAllItems()
        {
            weapons.Clear();
            armors.Clear();
            consumables.Clear();
            materials.Clear();
            LogDebug("[ItemFactory] All items cleared");
        }

        [Button("Show Item Statistics")]
        [ShowIf("debugMode")]
        public void ShowItemStatistics()
        {
            LogDebug("=== Item Factory Statistics ===");
            LogDebug($"Weapons: {WeaponCount}");
            LogDebug($"Armors: {ArmorCount}");
            LogDebug($"Consumables: {ConsumableCount}");
            LogDebug($"Materials: {MaterialCount}");
            LogDebug($"Total: {TotalItems}");
            LogDebug("===============================");
        }

        private void LogDebug(string message)
        {
            if (debugMode)
            {
                Debug.Log(message);
            }
        }

        #endregion
    }
}