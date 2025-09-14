using UnityEngine;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Features.Templates.ActionRPG.Equipment
{
    /// <summary>
    /// Action RPG用アイテムデータの基底クラス
    /// ScriptableObjectベースのアイテム定義システム
    /// </summary>
    [CreateAssetMenu(fileName = "New Item", menuName = "Action RPG/Equipment/Item Data")]
    public class ItemData : ScriptableObject
    {
        [BoxGroup("Basic Info")]
        [PreviewField(70)]
        public Sprite icon;

        [BoxGroup("Basic Info")]
        public string itemName;

        [BoxGroup("Basic Info")]
        [TextArea(3, 5)]
        public string description;

        [BoxGroup("Basic Info")]
        public ItemType itemType = ItemType.Consumable;

        [BoxGroup("Basic Info")]
        public ItemRarity rarity = ItemRarity.Common;

        [BoxGroup("Basic Info")]
        [Min(1)]
        public int maxStackSize = 1;

        [BoxGroup("Value")]
        [Min(0)]
        public int value = 1;

        [BoxGroup("Value")]
        public int sellPrice => Mathf.Max(1, value / 2);

        [BoxGroup("Equipment Stats")]
        [ShowIf("@itemType == ItemType.Weapon || itemType == ItemType.Armor")]
        public EquipmentStats stats = new EquipmentStats();

        [BoxGroup("Equipment Settings")]
        [ShowIf("@itemType == ItemType.Weapon || itemType == ItemType.Armor")]
        public EquipmentSlot equipmentSlot = EquipmentSlot.MainHand;

        [BoxGroup("Equipment Settings")]
        [ShowIf("@itemType == ItemType.Weapon || itemType == ItemType.Armor")]
        [Min(1)]
        public int requiredLevel = 1;

        [BoxGroup("Consumable Effects")]
        [ShowIf("@itemType == ItemType.Consumable")]
        public ConsumableEffect[] consumableEffects = new ConsumableEffect[0];

        [BoxGroup("Debug")]
        [ShowInInspector, ReadOnly]
        public string ItemID => name;

        [BoxGroup("Debug")]
        [ShowInInspector, ReadOnly]
        public Color RarityColor => GetRarityColor();

        private Color GetRarityColor()
        {
            return rarity switch
            {
                ItemRarity.Common => Color.white,
                ItemRarity.Uncommon => Color.green,
                ItemRarity.Rare => Color.blue,
                ItemRarity.Epic => Color.magenta,
                ItemRarity.Legendary => Color.yellow,
                _ => Color.white
            };
        }
    }

    [System.Serializable]
    public enum ItemType
    {
        Weapon,      // 武器
        Armor,       // 防具
        Accessory,   // アクセサリー
        Consumable,  // 消耗品
        KeyItem,     // 重要アイテム
        Material,    // 素材
        Misc         // その他
    }

    [System.Serializable]
    public enum ItemRarity
    {
        Common,      // 一般的 (白)
        Uncommon,    // 上質 (緑)
        Rare,        // レア (青)
        Epic,        // エピック (紫)
        Legendary    // 伝説 (金)
    }

    [System.Serializable]
    public enum EquipmentSlot
    {
        MainHand,    // 利き手 (武器)
        OffHand,     // 逆手 (盾・副武器)
        Helmet,      // ヘルメット
        Chest,       // 胸部防具
        Legs,        // 脚部防具
        Feet,        // 足部防具
        Gloves,      // グローブ
        Ring,        // 指輪
        Necklace,    // ネックレス
        Cloak        // マント
    }

    [System.Serializable]
    public class EquipmentStats
    {
        [BoxGroup("Primary Stats")]
        [Min(0)]
        public int attackPower = 0;

        [BoxGroup("Primary Stats")]
        [Min(0)]
        public int magicPower = 0;

        [BoxGroup("Primary Stats")]
        [Min(0)]
        public int defense = 0;

        [BoxGroup("Primary Stats")]
        [Min(0)]
        public int magicDefense = 0;

        [BoxGroup("Attribute Bonuses")]
        public int strengthBonus = 0;

        [BoxGroup("Attribute Bonuses")]
        public int dexterityBonus = 0;

        [BoxGroup("Attribute Bonuses")]
        public int intelligenceBonus = 0;

        [BoxGroup("Attribute Bonuses")]
        public int vitalityBonus = 0;

        [BoxGroup("Attribute Bonuses")]
        public int wisdomBonus = 0;

        [BoxGroup("Attribute Bonuses")]
        public int luckBonus = 0;

        [BoxGroup("Special Stats")]
        [Range(0f, 1f)]
        public float criticalChance = 0f;

        [BoxGroup("Special Stats")]
        [Min(1f)]
        public float criticalMultiplier = 1.5f;

        [BoxGroup("Special Stats")]
        [Range(0f, 1f)]
        public float dodgeChance = 0f;

        [BoxGroup("Special Stats")]
        [Range(0f, 1f)]
        public float blockChance = 0f;

        public EquipmentStats()
        {
            // デフォルト値設定
            criticalMultiplier = 1.5f;
        }

        /// <summary>
        /// 装備ステータスを合計する
        /// </summary>
        public static EquipmentStats operator +(EquipmentStats a, EquipmentStats b)
        {
            return new EquipmentStats
            {
                attackPower = a.attackPower + b.attackPower,
                magicPower = a.magicPower + b.magicPower,
                defense = a.defense + b.defense,
                magicDefense = a.magicDefense + b.magicDefense,
                strengthBonus = a.strengthBonus + b.strengthBonus,
                dexterityBonus = a.dexterityBonus + b.dexterityBonus,
                intelligenceBonus = a.intelligenceBonus + b.intelligenceBonus,
                vitalityBonus = a.vitalityBonus + b.vitalityBonus,
                wisdomBonus = a.wisdomBonus + b.wisdomBonus,
                luckBonus = a.luckBonus + b.luckBonus,
                criticalChance = Mathf.Clamp01(a.criticalChance + b.criticalChance),
                criticalMultiplier = a.criticalMultiplier + b.criticalMultiplier - 1f, // 基本1.0を考慮
                dodgeChance = Mathf.Clamp01(a.dodgeChance + b.dodgeChance),
                blockChance = Mathf.Clamp01(a.blockChance + b.blockChance)
            };
        }
    }

    [System.Serializable]
    public class ConsumableEffect
    {
        public ConsumableEffectType effectType = ConsumableEffectType.HealHP;

        [Min(0)]
        public int value = 10;

        [Min(0f)]
        public float duration = 0f; // 0 = 即座効果、0より大きい = 継続効果

        public string description = "";
    }

    [System.Serializable]
    public enum ConsumableEffectType
    {
        HealHP,          // HP回復
        HealMP,          // MP回復
        RestoreStamina,  // スタミナ回復
        TemporaryBoost,  // 一時的能力上昇
        Cure,            // 状態異常回復
        Buff,            // バフ効果
        Debuff           // デバフ効果（毒薬など）
    }
}