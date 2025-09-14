using System;
using System.Collections.Generic;
using UnityEngine;
using asterivo.Unity60.Core.Events;

namespace asterivo.Unity60.Features.Templates.ActionRPG.Inventory
{
    /// <summary>
    /// ActionRPGテンプレート用アイテム管理
    /// インベントリ、装備、アイテム使用管理
    /// </summary>
    public class ItemManager : MonoBehaviour
    {
        [Header("インベントリ設定")]
        [SerializeField] private int maxInventorySlots = 50;
        [SerializeField] private int maxStackSize = 99;
        [SerializeField] private int startingGold = 100;
        
        [Header("装備スロット設定")]
        [SerializeField] private int weaponSlots = 2;
        [SerializeField] private int armorSlots = 6;
        [SerializeField] private int accessorySlots = 4;
        
        [Header("イベント")]
        [SerializeField] private StringGameEvent onItemAcquired;
        [SerializeField] private StringGameEvent onItemUsed;
        [SerializeField] private StringGameEvent onItemEquipped;
        [SerializeField] private IntGameEvent onGoldChanged;
        
        // インベントリデータ
        public Dictionary<string, ItemStack> Inventory { get; private set; } = new();
        public Dictionary<EquipmentSlot, ItemData> EquippedItems { get; private set; } = new();
        
        // 通貨
        public int Gold { get; private set; }
        
        // プロパティ
        public int UsedSlots => Inventory.Count;
        public int AvailableSlots => maxInventorySlots - UsedSlots;
        public bool IsInventoryFull => UsedSlots >= maxInventorySlots;
        
        public enum EquipmentSlot
        {
            MainHand,       // メインウェポン
            OffHand,        // サブウェポン・シールド
            Head,           // ヘルメット
            Chest,          // 胸当て
            Legs,           // レッグガード
            Feet,           // ブーツ
            Hands,          // グローブ
            Ring1,          // 指輪1
            Ring2,          // 指輪2
            Necklace,       // ネックレス
            Back           // クローク
        }
        
        private void Start()
        {
            InitializeInventory();
        }
        
        /// <summary>
        /// インベントリ初期化
        /// </summary>
        private void InitializeInventory()
        {
            Gold = startingGold;
            Inventory.Clear();
            EquippedItems.Clear();
            
            // スターター装備の追加
            AddStartingItems();
            
            onGoldChanged?.Raise(Gold);
            
            Debug.Log($"[ItemManager] インベントリ初期化完了 (最大スロット: {maxInventorySlots})");
        }
        
        /// <summary>
        /// スターター装備追加
        /// </summary>
        private void AddStartingItems()
        {
            // 基本武器
            var starterSword = new ItemData("StarterSword", "初心者の剣", ItemType.Weapon, ItemRarity.Common);
            TryAddItem(starterSword, 1);
            
            // 基本防具
            var starterArmor = new ItemData("StarterArmor", "初心者の鎧", ItemType.Armor, ItemRarity.Common);
            TryAddItem(starterArmor, 1);
            
            // 回復アイテム
            var healthPotion = new ItemData("HealthPotion", "体力薬", ItemType.Consumable, ItemRarity.Common);
            TryAddItem(healthPotion, 5);
        }
        
        /// <summary>
        /// アイテム追加
        /// </summary>
        public bool TryAddItem(ItemData item, int quantity = 1)
        {
            if (item == null || quantity <= 0) return false;
            
            // スタッカブルアイテムの場合
            if (item.IsStackable && Inventory.ContainsKey(item.Id))
            {
                var stack = Inventory[item.Id];
                int addableAmount = Mathf.Min(quantity, maxStackSize - stack.Quantity);
                
                if (addableAmount > 0)
                {
                    stack.Quantity += addableAmount;
                    onItemAcquired?.Raise($"{item.Name} x{addableAmount}");
                    
                    return addableAmount == quantity; // 全量追加できたかどうか
                }
                
                return false;
            }
            
            // 新しいアイテムスタック
            if (IsInventoryFull) return false;
            
            var newStack = new ItemStack(item, quantity);
            Inventory[item.Id] = newStack;
            
            onItemAcquired?.Raise($"{item.Name} x{quantity}");
            
            Debug.Log($"[ItemManager] アイテム取得: {item.Name} x{quantity}");
            
            return true;
        }
        
        /// <summary>
        /// アイテム使用
        /// </summary>
        public bool TryUseItem(string itemId, int quantity = 1)
        {
            if (!Inventory.ContainsKey(itemId)) return false;
            
            var stack = Inventory[itemId];
            if (stack.Quantity < quantity) return false;
            
            var item = stack.ItemData;
            
            // アイテム効果を適用
            ApplyItemEffect(item);
            
            // 消費処理
            stack.Quantity -= quantity;
            
            if (stack.Quantity <= 0)
            {
                Inventory.Remove(itemId);
            }
            
            onItemUsed?.Raise($"{item.Name} x{quantity}");
            
            Debug.Log($"[ItemManager] アイテム使用: {item.Name} x{quantity}");
            
            return true;
        }
        
        /// <summary>
        /// アイテム装備
        /// </summary>
        public bool TryEquipItem(string itemId)
        {
            if (!Inventory.ContainsKey(itemId)) return false;
            
            var item = Inventory[itemId].ItemData;
            var slot = GetEquipmentSlot(item);
            
            if (slot == null) return false;
            
            // 現在装備中のアイテムがあれば外す
            if (EquippedItems.ContainsKey(slot.Value))
            {
                UnequipItem(slot.Value);
            }
            
            // 新しいアイテムを装備
            EquippedItems[slot.Value] = item;
            
            // インベントリから削除（装備は別管理）
            Inventory.Remove(itemId);
            
            onItemEquipped?.Raise($"{item.Name} equipped to {slot}");
            
            Debug.Log($"[ItemManager] アイテム装備: {item.Name} -> {slot}");
            
            return true;
        }
        
        /// <summary>
        /// アイテム装備解除
        /// </summary>
        public bool UnequipItem(EquipmentSlot slot)
        {
            if (!EquippedItems.ContainsKey(slot)) return false;
            
            var item = EquippedItems[slot];
            
            // インベントリに戻す
            if (!TryAddItem(item, 1))
            {
                Debug.LogWarning($"[ItemManager] インベントリが満杯で装備を外せません: {item.Name}");
                return false;
            }
            
            EquippedItems.Remove(slot);
            
            Debug.Log($"[ItemManager] 装備解除: {item.Name}");
            
            return true;
        }
        
        /// <summary>
        /// ゴールド追加
        /// </summary>
        public void AddGold(int amount)
        {
            Gold += amount;
            onGoldChanged?.Raise(Gold);
            
            Debug.Log($"[ItemManager] ゴールド取得: +{amount} (合計: {Gold})");
        }
        
        /// <summary>
        /// ゴールド使用
        /// </summary>
        public bool TrySpendGold(int amount)
        {
            if (Gold < amount) return false;
            
            Gold -= amount;
            onGoldChanged?.Raise(Gold);
            
            Debug.Log($"[ItemManager] ゴールド使用: -{amount} (残り: {Gold})");
            
            return true;
        }
        
        /// <summary>
        /// アイテム効果適用
        /// </summary>
        private void ApplyItemEffect(ItemData item)
        {
            switch (item.Type)
            {
                case ItemType.Consumable:
                    ApplyConsumableEffect(item);
                    break;
                    
                case ItemType.Weapon:
                case ItemType.Armor:
                    // 装備効果は別途管理
                    break;
            }
        }
        
        /// <summary>
        /// 消耗品効果適用
        /// </summary>
        private void ApplyConsumableEffect(ItemData item)
        {
            // アイテムIDによる効果分岐
            switch (item.Id)
            {
                case "HealthPotion":
                    // CharacterStatsManager 経由でヘルス回復
                    var statsManager = GetComponent<CharacterStatsManager>();
                    statsManager?.RestoreHealth(50f);
                    break;
                    
                case "ManaPotion":
                    // マナ回復処理
                    break;
                    
                default:
                    Debug.Log($"[ItemManager] 未定義のアイテム効果: {item.Id}");
                    break;
            }
        }
        
        /// <summary>
        /// 装備スロット判定
        /// </summary>
        private EquipmentSlot? GetEquipmentSlot(ItemData item)
        {
            return item.Type switch
            {
                ItemType.Weapon => EquipmentSlot.MainHand,
                ItemType.Armor => EquipmentSlot.Chest, // 簡単化
                _ => null
            };
        }
        
        /// <summary>
        /// アイテム検索
        /// </summary>
        public ItemStack GetItem(string itemId)
        {
            return Inventory.ContainsKey(itemId) ? Inventory[itemId] : null;
        }
        
        /// <summary>
        /// インベントリ内容取得
        /// </summary>
        public List<ItemStack> GetAllItems()
        {
            return new List<ItemStack>(Inventory.Values);
        }
    }
    
    /// <summary>
    /// アイテムデータクラス
    /// </summary>
    [System.Serializable]
    public class ItemData
    {
        public string Id;
        public string Name;
        public ItemType Type;
        public ItemRarity Rarity;
        public bool IsStackable;
        public string Description;
        
        public ItemData(string id, string name, ItemType type, ItemRarity rarity, bool stackable = true)
        {
            Id = id;
            Name = name;
            Type = type;
            Rarity = rarity;
            IsStackable = stackable;
            Description = "";
        }
    }
    
    /// <summary>
    /// アイテムスタッククラス
    /// </summary>
    [System.Serializable]
    public class ItemStack
    {
        public ItemData ItemData;
        public int Quantity;
        
        public ItemStack(ItemData itemData, int quantity)
        {
            ItemData = itemData;
            Quantity = quantity;
        }
    }
    
    /// <summary>
    /// アイテム種別
    /// </summary>
    public enum ItemType
    {
        Consumable,     // 消耗品
        Weapon,         // 武器
        Armor,          // 防具
        Accessory,      // アクセサリ
        Material,       // 素材
        Key,           // 重要アイテム
        Quest          // クエストアイテム
    }
    
    /// <summary>
    /// アイテム品質
    /// </summary>
    public enum ItemRarity
    {
        Common,         // コモン（白）
        Uncommon,       // アンコモン（緑）
        Rare,          // レア（青）
        Epic,          // エピック（紫）
        Legendary      // レジェンダリー（橙）
    }
}