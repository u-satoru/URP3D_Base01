using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core;
using asterivo.Unity60.Features.Templates.ActionRPG.Character;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Features.Templates.ActionRPG.Equipment
{
    /// <summary>
    /// Action RPGテンプレート用装備管理システム
    /// 装備アイテムの着脱、ステータス計算、視覚的表現を管理する
    /// </summary>
    public class EquipmentManager : MonoBehaviour
    {
        [BoxGroup("Configuration")]
        [SerializeField]
        private bool autoCalculateStats = true;

        [BoxGroup("Configuration")]
        [SerializeField]
        private bool debugMode = false;

        [BoxGroup("Events")]
        [SerializeField]
        private GameEvent onEquipmentChanged;

        [BoxGroup("Events")]
        [SerializeField]
        private GameEvent<ItemData> onItemEquipped;

        [BoxGroup("Events")]
        [SerializeField]
        private GameEvent<ItemData> onItemUnequipped;

        [BoxGroup("Current Equipment")]
        [ShowInInspector]
        private Dictionary<EquipmentSlot, EquipmentSlotData> equippedItems = new Dictionary<EquipmentSlot, EquipmentSlotData>();

        [BoxGroup("Calculated Stats")]
        [ShowInInspector, ReadOnly]
        private EquipmentStats totalEquipmentStats = new EquipmentStats();

        [BoxGroup("Statistics")]
        [ShowInInspector, ReadOnly]
        public int EquippedItemCount => equippedItems.Count(kvp => kvp.Value.hasItem);

        [BoxGroup("Statistics")]
        [ShowInInspector, ReadOnly]
        public int EmptySlotCount => System.Enum.GetValues(typeof(EquipmentSlot)).Length - EquippedItemCount;

        // 依存システム
        private CharacterProgressionManager characterProgression;
        private InventoryManager inventoryManager;

        // プロパティ
        public EquipmentStats TotalEquipmentStats => totalEquipmentStats;
        public IReadOnlyDictionary<EquipmentSlot, EquipmentSlotData> EquippedItems =>
            equippedItems.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        // イベント
        public event Action<ItemData, EquipmentSlot> OnItemEquipped;
        public event Action<ItemData, EquipmentSlot> OnItemUnequipped;
        public event Action OnEquipmentChanged;

        private void Awake()
        {
            InitializeEquipment();
        }

        private void Start()
        {
            InitializeDependencies();
            RegisterWithServices();
        }

        private void OnDestroy()
        {
            UnregisterFromServices();
        }

        /// <summary>
        /// 装備システムの初期化
        /// </summary>
        private void InitializeEquipment()
        {
            equippedItems = new Dictionary<EquipmentSlot, EquipmentSlotData>();

            // すべての装備スロットを初期化
            foreach (EquipmentSlot slot in System.Enum.GetValues(typeof(EquipmentSlot)))
            {
                equippedItems[slot] = new EquipmentSlotData
                {
                    slot = slot,
                    equippedItem = null,
                    hasItem = false
                };
            }

            CalculateEquipmentStats();
            LogDebug($"[EquipmentManager] Initialized {equippedItems.Count} equipment slots");
        }

        /// <summary>
        /// 依存システムの初期化
        /// </summary>
        private void InitializeDependencies()
        {
            characterProgression = GetComponent<CharacterProgressionManager>();
            if (characterProgression == null)
            {
                characterProgression = FindFirstObjectByType<CharacterProgressionManager>();
            }

            inventoryManager = ServiceLocator.GetService<InventoryManager>();

            LogDebug("[EquipmentManager] Dependencies initialized");
        }

        /// <summary>
        /// サービスへの登録
        /// </summary>
        private void RegisterWithServices()
        {
            ServiceLocator.RegisterService<EquipmentManager>(this);
            LogDebug("[EquipmentManager] Registered with ServiceLocator");
        }

        /// <summary>
        /// サービスの登録解除
        /// </summary>
        private void UnregisterFromServices()
        {
            ServiceLocator.UnregisterService<EquipmentManager>();
            LogDebug("[EquipmentManager] Unregistered from ServiceLocator");
        }

        #region Equipment Management

        /// <summary>
        /// アイテムを装備する
        /// </summary>
        /// <param name="itemData">装備するアイテム</param>
        /// <returns>装備に成功したかどうか</returns>
        public bool EquipItem(ItemData itemData)
        {
            if (itemData == null)
            {
                LogError("[EquipmentManager] Cannot equip null item");
                return false;
            }

            if (itemData.itemType != ItemType.Weapon && itemData.itemType != ItemType.Armor)
            {
                LogWarning($"[EquipmentManager] Item {itemData.itemName} is not equippable");
                return false;
            }

            // レベル要件チェック
            if (characterProgression != null && characterProgression.CurrentLevel < itemData.requiredLevel)
            {
                LogWarning($"[EquipmentManager] Level {itemData.requiredLevel} required to equip {itemData.itemName}");
                return false;
            }

            var targetSlot = itemData.equipmentSlot;

            // 既に装備しているアイテムがある場合は外す
            if (equippedItems[targetSlot].hasItem)
            {
                UnequipItem(targetSlot);
            }

            // アイテムを装備
            equippedItems[targetSlot] = new EquipmentSlotData
            {
                slot = targetSlot,
                equippedItem = itemData,
                hasItem = true
            };

            // インベントリからアイテムを削除
            if (inventoryManager != null)
            {
                inventoryManager.RemoveItem(itemData, 1);
            }

            // ステータス再計算
            if (autoCalculateStats)
            {
                CalculateEquipmentStats();
            }

            // イベント発行
            OnItemEquipped?.Invoke(itemData, targetSlot);
            onItemEquipped?.Raise(itemData);
            NotifyEquipmentChanged();

            LogDebug($"[EquipmentManager] Equipped {itemData.itemName} to {targetSlot}");
            return true;
        }

        /// <summary>
        /// 装備を外す
        /// </summary>
        /// <param name="slot">外すスロット</param>
        /// <returns>外すことに成功したかどうか</returns>
        public bool UnequipItem(EquipmentSlot slot)
        {
            if (!equippedItems[slot].hasItem)
            {
                LogWarning($"[EquipmentManager] No item equipped in {slot}");
                return false;
            }

            var itemData = equippedItems[slot].equippedItem;

            // インベントリに空きがあるかチェック
            if (inventoryManager != null && inventoryManager.IsFull)
            {
                LogWarning($"[EquipmentManager] Cannot unequip {itemData.itemName} - inventory full");
                return false;
            }

            // 装備を外す
            equippedItems[slot] = new EquipmentSlotData
            {
                slot = slot,
                equippedItem = null,
                hasItem = false
            };

            // インベントリにアイテムを戻す
            if (inventoryManager != null)
            {
                inventoryManager.AddItem(itemData, 1);
            }

            // ステータス再計算
            if (autoCalculateStats)
            {
                CalculateEquipmentStats();
            }

            // イベント発行
            OnItemUnequipped?.Invoke(itemData, slot);
            onItemUnequipped?.Raise(itemData);
            NotifyEquipmentChanged();

            LogDebug($"[EquipmentManager] Unequipped {itemData.itemName} from {slot}");
            return true;
        }

        /// <summary>
        /// 特定のスロットの装備を取得
        /// </summary>
        public ItemData GetEquippedItem(EquipmentSlot slot)
        {
            return equippedItems[slot].hasItem ? equippedItems[slot].equippedItem : null;
        }

        /// <summary>
        /// アイテムが装備可能かチェック
        /// </summary>
        public bool CanEquipItem(ItemData itemData)
        {
            if (itemData == null)
                return false;

            if (itemData.itemType != ItemType.Weapon && itemData.itemType != ItemType.Armor)
                return false;

            // レベル要件チェック
            if (characterProgression != null && characterProgression.CurrentLevel < itemData.requiredLevel)
                return false;

            return true;
        }

        /// <summary>
        /// 装備を交換
        /// </summary>
        public bool SwapEquipment(EquipmentSlot slot, ItemData newItem)
        {
            if (!CanEquipItem(newItem))
                return false;

            // 既存装備を外してから新しい装備を装着
            if (equippedItems[slot].hasItem)
            {
                UnequipItem(slot);
            }

            return EquipItem(newItem);
        }

        #endregion

        #region Stats Calculation

        /// <summary>
        /// 装備ステータスを計算
        /// </summary>
        public void CalculateEquipmentStats()
        {
            totalEquipmentStats = new EquipmentStats();

            foreach (var kvp in equippedItems)
            {
                if (kvp.Value.hasItem)
                {
                    totalEquipmentStats = totalEquipmentStats + kvp.Value.equippedItem.stats;
                }
            }

            LogDebug($"[EquipmentManager] Equipment stats calculated - Attack: {totalEquipmentStats.attackPower}, Defense: {totalEquipmentStats.defense}");
        }

        /// <summary>
        /// 装備による属性ボーナスを取得
        /// </summary>
        public (int str, int dex, int intel, int vit, int wis, int luck) GetAttributeBonuses()
        {
            return (
                totalEquipmentStats.strengthBonus,
                totalEquipmentStats.dexterityBonus,
                totalEquipmentStats.intelligenceBonus,
                totalEquipmentStats.vitalityBonus,
                totalEquipmentStats.wisdomBonus,
                totalEquipmentStats.luckBonus
            );
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// 全装備を外す
        /// </summary>
        public void UnequipAll()
        {
            var slotsToUnequip = equippedItems.Where(kvp => kvp.Value.hasItem).Select(kvp => kvp.Key).ToList();

            foreach (var slot in slotsToUnequip)
            {
                UnequipItem(slot);
            }

            LogDebug("[EquipmentManager] All equipment unequipped");
        }

        /// <summary>
        /// 装備変更の通知
        /// </summary>
        private void NotifyEquipmentChanged()
        {
            OnEquipmentChanged?.Invoke();
            onEquipmentChanged?.Raise();
        }

        #endregion

        #region Debug Support

        [Button("Calculate Stats")]
        [ShowIf("debugMode")]
        private void CalculateStatsDebug()
        {
            CalculateEquipmentStats();
        }

        [Button("Unequip All")]
        [ShowIf("debugMode")]
        private void UnequipAllDebug()
        {
            UnequipAll();
        }

        [Button("Show Equipment Status")]
        [ShowIf("debugMode")]
        private void ShowEquipmentStatus()
        {
            LogDebug("=== Equipment Status ===");
            foreach (var kvp in equippedItems)
            {
                if (kvp.Value.hasItem)
                {
                    LogDebug($"{kvp.Key}: {kvp.Value.equippedItem.itemName}");
                }
                else
                {
                    LogDebug($"{kvp.Key}: Empty");
                }
            }
            LogDebug("========================");
        }

        private void LogDebug(string message)
        {
            if (debugMode)
            {
                Debug.Log(message);
            }
        }

        private void LogWarning(string message)
        {
            Debug.LogWarning(message);
        }

        private void LogError(string message)
        {
            Debug.LogError(message);
        }

        #endregion
    }

    /// <summary>
    /// 装備スロットデータ
    /// </summary>
    [System.Serializable]
    public class EquipmentSlotData
    {
        public EquipmentSlot slot;
        public ItemData equippedItem;
        public bool hasItem;

        public EquipmentSlotData()
        {
            hasItem = false;
            equippedItem = null;
        }
    }
}