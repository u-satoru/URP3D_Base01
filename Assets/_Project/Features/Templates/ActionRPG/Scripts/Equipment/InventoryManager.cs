using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Services;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Features.Templates.ActionRPG.Equipment
{
    /// <summary>
    /// Action RPGテンプレート用インベントリ管理システム
    /// アイテムの追加、削除、検索、スタック管理を行う
    /// </summary>
    public class InventoryManager : MonoBehaviour
    {
        [BoxGroup("Configuration")]
        [SerializeField]
        [Min(1)]
        private int maxSlots = 50;

        [BoxGroup("Configuration")]
        [SerializeField]
        private bool autoSort = true;

        [BoxGroup("Configuration")]
        [SerializeField]
        private bool debugMode = false;

        [BoxGroup("Events")]
        [SerializeField]
        private GameEvent onInventoryChanged;

        [BoxGroup("Events")]
        // [SerializeField]
        // private GameEvent<ItemData> onItemAdded; // TODO: Implement after creating ItemData class

        [BoxGroup("Events")]
        // [SerializeField]
        // private GameEvent<ItemData> onItemRemoved; // TODO: Implement after creating ItemData class

        [BoxGroup("Events")]
        [SerializeField]
        private GameEvent onInventoryFull;

        [BoxGroup("Current State")]
        [ShowInInspector, ReadOnly]
        private List<InventorySlot> inventorySlots = new List<InventorySlot>();

        [BoxGroup("Statistics")]
        [ShowInInspector, ReadOnly]
        public int UsedSlots => inventorySlots.Count(slot => slot.HasItem);

        [BoxGroup("Statistics")]
        [ShowInInspector, ReadOnly]
        public int FreeSlots => maxSlots - UsedSlots;

        [BoxGroup("Statistics")]
        [ShowInInspector, ReadOnly]
        public bool IsFull => UsedSlots >= maxSlots;

        [BoxGroup("Statistics")]
        [ShowInInspector, ReadOnly]
        public int TotalItemCount => inventorySlots.Where(slot => slot.HasItem).Sum(slot => slot.quantity);

        // プロパティ
        public IReadOnlyList<InventorySlot> Slots => inventorySlots.AsReadOnly();

        // イベント
        public event Action<ItemData, int> OnItemAdded;
        public event Action<ItemData, int> OnItemRemoved;
        public event Action OnInventoryChanged;
        public event Action OnInventoryFull;

        private void Awake()
        {
            InitializeInventory();
        }

        private void Start()
        {
            RegisterWithServices();
        }

        private void OnDestroy()
        {
            UnregisterFromServices();
        }

        /// <summary>
        /// インベントリの初期化
        /// </summary>
        private void InitializeInventory()
        {
            inventorySlots = new List<InventorySlot>();

            for (int i = 0; i < maxSlots; i++)
            {
                inventorySlots.Add(new InventorySlot(i));
            }

            LogDebug($"[InventoryManager] Initialized with {maxSlots} slots");
        }

        /// <summary>
        /// サービスへの登録
        /// </summary>
        private void RegisterWithServices()
        {
            if (ServiceLocator.Instance != null)
            {
                ServiceLocator.Instance.RegisterService<InventoryManager>(this);
                LogDebug("[InventoryManager] Registered with ServiceLocator");
            }
        }

        /// <summary>
        /// サービスの登録解除
        /// </summary>
        private void UnregisterFromServices()
        {
            if (ServiceLocator.Instance != null)
            {
                ServiceLocator.Instance.UnregisterService<InventoryManager>();
                LogDebug("[InventoryManager] Unregistered from ServiceLocator");
            }
        }

        #region Item Management

        /// <summary>
        /// アイテムをインベントリに追加
        /// </summary>
        /// <param name="itemData">追加するアイテム</param>
        /// <param name="quantity">追加する数量</param>
        /// <returns>実際に追加できた数量</returns>
        public int AddItem(ItemData itemData, int quantity = 1)
        {
            if (itemData == null)
            {
                LogError("[InventoryManager] Cannot add null item");
                return 0;
            }

            if (quantity <= 0)
            {
                LogWarning($"[InventoryManager] Invalid quantity {quantity} for item {itemData.itemName}");
                return 0;
            }

            int originalQuantity = quantity;
            LogDebug($"[InventoryManager] Attempting to add {quantity} x {itemData.itemName}");

            // スタック可能なアイテムの場合、既存のスロットを探す
            if (itemData.maxStackSize > 1)
            {
                foreach (var slot in inventorySlots.Where(s => s.HasItem && s.itemData == itemData))
                {
                    int addedToSlot = slot.AddItem(quantity);
                    quantity -= addedToSlot;

                    if (quantity <= 0)
                        break;
                }
            }

            // 新しいスロットに追加
            while (quantity > 0 && !IsFull)
            {
                var emptySlot = inventorySlots.FirstOrDefault(s => !s.HasItem);
                if (emptySlot == null)
                    break;

                int addedToSlot = emptySlot.SetItem(itemData, quantity);
                quantity -= addedToSlot;
            }

            int actuallyAdded = originalQuantity - quantity;

            if (actuallyAdded > 0)
            {
                OnItemAdded?.Invoke(itemData, actuallyAdded);
                onItemAdded?.Raise(itemData);
                NotifyInventoryChanged();

                LogDebug($"[InventoryManager] Successfully added {actuallyAdded} x {itemData.itemName}");
            }

            if (quantity > 0)
            {
                OnInventoryFull?.Invoke();
                onInventoryFull?.Raise();
                LogWarning($"[InventoryManager] Could not add {quantity} x {itemData.itemName} - inventory full");
            }

            if (autoSort && actuallyAdded > 0)
            {
                SortInventory();
            }

            return actuallyAdded;
        }

        /// <summary>
        /// アイテムをインベントリから削除
        /// </summary>
        /// <param name="itemData">削除するアイテム</param>
        /// <param name="quantity">削除する数量</param>
        /// <returns>実際に削除できた数量</returns>
        public int RemoveItem(ItemData itemData, int quantity = 1)
        {
            if (itemData == null)
            {
                LogError("[InventoryManager] Cannot remove null item");
                return 0;
            }

            if (quantity <= 0)
            {
                LogWarning($"[InventoryManager] Invalid quantity {quantity} for item {itemData.itemName}");
                return 0;
            }

            int originalQuantity = quantity;
            var slotsWithItem = inventorySlots.Where(s => s.HasItem && s.itemData == itemData).ToList();

            foreach (var slot in slotsWithItem)
            {
                int removedFromSlot = slot.RemoveItem(quantity);
                quantity -= removedFromSlot;

                if (quantity <= 0)
                    break;
            }

            int actuallyRemoved = originalQuantity - quantity;

            if (actuallyRemoved > 0)
            {
                OnItemRemoved?.Invoke(itemData, actuallyRemoved);
                onItemRemoved?.Raise(itemData);
                NotifyInventoryChanged();

                LogDebug($"[InventoryManager] Successfully removed {actuallyRemoved} x {itemData.itemName}");
            }

            return actuallyRemoved;
        }

        /// <summary>
        /// 特定のスロットからアイテムを削除
        /// </summary>
        public bool RemoveItemFromSlot(int slotIndex, int quantity = 1)
        {
            if (!IsValidSlotIndex(slotIndex))
                return false;

            var slot = inventorySlots[slotIndex];
            if (!slot.HasItem)
                return false;

            var itemData = slot.itemData;
            int removed = slot.RemoveItem(quantity);

            if (removed > 0)
            {
                OnItemRemoved?.Invoke(itemData, removed);
                onItemRemoved?.Raise(itemData);
                NotifyInventoryChanged();
                return true;
            }

            return false;
        }

        /// <summary>
        /// アイテムの所持数を取得
        /// </summary>
        public int GetItemCount(ItemData itemData)
        {
            if (itemData == null)
                return 0;

            return inventorySlots
                .Where(s => s.HasItem && s.itemData == itemData)
                .Sum(s => s.quantity);
        }

        /// <summary>
        /// アイテムを所持しているかチェック
        /// </summary>
        public bool HasItem(ItemData itemData, int requiredQuantity = 1)
        {
            return GetItemCount(itemData) >= requiredQuantity;
        }

        /// <summary>
        /// アイテムを使用（消耗品用）
        /// </summary>
        public bool UseItem(ItemData itemData, int quantity = 1)
        {
            if (itemData == null || itemData.itemType != ItemType.Consumable)
                return false;

            if (!HasItem(itemData, quantity))
                return false;

            // アイテムの効果を適用（ここでは削除のみ）
            int removed = RemoveItem(itemData, quantity);
            return removed >= quantity;
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// インベントリをソート
        /// </summary>
        public void SortInventory()
        {
            var itemsWithSlots = inventorySlots
                .Where(s => s.HasItem)
                .Select(s => new { Item = s.itemData, Quantity = s.quantity })
                .OrderBy(x => x.Item.itemType)
                .ThenBy(x => x.Item.rarity)
                .ThenBy(x => x.Item.itemName)
                .ToList();

            // すべてのスロットをクリア
            foreach (var slot in inventorySlots)
            {
                slot.Clear();
            }

            // ソート順でアイテムを再配置
            int slotIndex = 0;
            foreach (var item in itemsWithSlots)
            {
                if (slotIndex >= maxSlots)
                    break;

                inventorySlots[slotIndex].SetItem(item.Item, item.Quantity);
                slotIndex++;
            }

            NotifyInventoryChanged();
            LogDebug("[InventoryManager] Inventory sorted");
        }

        /// <summary>
        /// インベントリをクリア
        /// </summary>
        public void ClearInventory()
        {
            foreach (var slot in inventorySlots)
            {
                slot.Clear();
            }

            NotifyInventoryChanged();
            LogDebug("[InventoryManager] Inventory cleared");
        }

        /// <summary>
        /// スロットインデックスの有効性チェック
        /// </summary>
        private bool IsValidSlotIndex(int index)
        {
            return index >= 0 && index < inventorySlots.Count;
        }

        /// <summary>
        /// インベントリ変更の通知
        /// </summary>
        private void NotifyInventoryChanged()
        {
            OnInventoryChanged?.Invoke();
            onInventoryChanged?.Raise();
        }

        #endregion

        #region Debug Support

        [Button("Add Test Items")]
        [ShowIf("debugMode")]
        private void AddTestItems()
        {
            LogDebug("[InventoryManager] Adding test items...");
        }

        [Button("Clear All Items")]
        [ShowIf("debugMode")]
        private void ClearAllItems()
        {
            ClearInventory();
        }

        [Button("Sort Inventory")]
        [ShowIf("debugMode")]
        private void SortInventoryDebug()
        {
            SortInventory();
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
    /// インベントリスロットクラス
    /// </summary>
    [System.Serializable]
    public class InventorySlot
    {
        [SerializeField]
        private int slotIndex;

        [SerializeField]
        private ItemData _itemData;

        [SerializeField]
        private int _quantity;

        public ItemData itemData => _itemData;
        public int quantity => _quantity;
        public bool HasItem => _itemData != null && _quantity > 0;
        public bool IsEmpty => !HasItem;
        public int SlotIndex => slotIndex;

        public InventorySlot(int index)
        {
            slotIndex = index;
            Clear();
        }

        /// <summary>
        /// アイテムをスロットに設定
        /// </summary>
        public int SetItem(ItemData item, int qty)
        {
            if (item == null || qty <= 0)
                return 0;

            if (HasItem && _itemData != item)
                return 0; // 異なるアイテムは設定できない

            int maxCanAdd = item.maxStackSize - _quantity;
            int actualAdd = Mathf.Min(qty, maxCanAdd);

            _itemData = item;
            _quantity += actualAdd;

            return actualAdd;
        }

        /// <summary>
        /// アイテムをスロットに追加
        /// </summary>
        public int AddItem(int qty)
        {
            if (!HasItem || qty <= 0)
                return 0;

            int maxCanAdd = _itemData.maxStackSize - _quantity;
            int actualAdd = Mathf.Min(qty, maxCanAdd);

            _quantity += actualAdd;
            return actualAdd;
        }

        /// <summary>
        /// アイテムをスロットから削除
        /// </summary>
        public int RemoveItem(int qty)
        {
            if (!HasItem || qty <= 0)
                return 0;

            int actualRemove = Mathf.Min(qty, _quantity);
            _quantity -= actualRemove;

            if (_quantity <= 0)
            {
                Clear();
            }

            return actualRemove;
        }

        /// <summary>
        /// スロットをクリア
        /// </summary>
        public void Clear()
        {
            _itemData = null;
            _quantity = 0;
        }
    }
}