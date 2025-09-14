using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Features.Templates.Adventure.Quest;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Features.Templates.Adventure.Inventory
{
    /// <summary>
    /// Adventure Template用インベントリ管理システム
    /// アドベンチャーゲーム特有のアイテム収集、管理、使用を統合
    /// </summary>
    public class AdventureInventoryManager : MonoBehaviour
    {
        [TabGroup("Inventory", "Configuration")]
        [Header("Inventory Settings")]
        [SerializeField]
        [Tooltip("インベントリの最大スロット数")]
        [Min(10)]
        private int maxInventorySlots = 50;

        [TabGroup("Inventory", "Configuration")]
        [SerializeField]
        [Tooltip("同じアイテムの最大スタック数")]
        [Min(1)]
        private int maxStackSize = 99;

        [TabGroup("Inventory", "Configuration")]
        [SerializeField]
        [Tooltip("自動でアイテムをソートするかどうか")]
        private bool autoSort = true;

        [TabGroup("Events", "Event Configuration")]
        [Header("Events")]
        [SerializeField]
        [Tooltip("アイテム追加時に発行されるイベント")]
        private AdventureItemDataGameEvent onItemAdded;

        [TabGroup("Events", "Event Configuration")]
        [SerializeField]
        [Tooltip("アイテム削除時に発行されるイベント")]
        private AdventureItemDataGameEvent onItemRemoved;

        [TabGroup("Events", "Event Configuration")]
        [SerializeField]
        [Tooltip("アイテム使用時に発行されるイベント")]
        private AdventureItemDataGameEvent onItemUsed;

        [TabGroup("Events", "Event Configuration")]
        [SerializeField]
        [Tooltip("インベントリが満杯時に発行されるイベント")]
        private GameEvent onInventoryFull;

        [TabGroup("Inventory", "Current State")]
        [ShowInInspector]
        [ReadOnly]
        private List<AdventureInventorySlot> inventorySlots = new List<AdventureInventorySlot>();

        [TabGroup("Statistics", "Statistics")]
        [ShowInInspector]
        [ReadOnly]
        public int UsedSlots => inventorySlots.Count(slot => slot.HasItem);

        [TabGroup("Statistics", "Statistics")]
        [ShowInInspector]
        [ReadOnly]
        public int FreeSlots => maxInventorySlots - UsedSlots;

        [TabGroup("Statistics", "Statistics")]
        [ShowInInspector]
        [ReadOnly]
        public bool IsFull => UsedSlots >= maxInventorySlots;

        [TabGroup("Statistics", "Statistics")]
        [ShowInInspector]
        [ReadOnly]
        public int TotalItemCount => inventorySlots.Where(slot => slot.HasItem).Sum(slot => slot.quantity);

        // イベント
        public event System.Action<AdventureItemData, int> OnItemAdded;
        public event System.Action<AdventureItemData, int> OnItemRemoved;
        public event System.Action<AdventureItemData> OnItemUsed;
        public event System.Action OnInventoryChanged;
        public event System.Action OnInventoryFull;

        // プロパティ
        public IReadOnlyList<AdventureInventorySlot> Slots => inventorySlots.AsReadOnly();

        private QuestManager questManager;

        private void Awake()
        {
            InitializeInventory();
        }

        private void Start()
        {
            RegisterWithServices();
            FindQuestManager();
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
            inventorySlots = new List<AdventureInventorySlot>();

            for (int i = 0; i < maxInventorySlots; i++)
            {
                inventorySlots.Add(new AdventureInventorySlot(i));
            }

            Debug.Log($"[AdventureInventoryManager] Initialized with {maxInventorySlots} slots");
        }

        /// <summary>
        /// サービスへの登録
        /// </summary>
        private void RegisterWithServices()
        {
            if (ServiceLocator.Instance != null)
            {
                ServiceLocator.Instance.RegisterService<AdventureInventoryManager>(this);
                Debug.Log("[AdventureInventoryManager] Registered with ServiceLocator");
            }
        }

        /// <summary>
        /// サービスの登録解除
        /// </summary>
        private void UnregisterFromServices()
        {
            if (ServiceLocator.Instance != null)
            {
                ServiceLocator.Instance.UnregisterService<AdventureInventoryManager>();
                Debug.Log("[AdventureInventoryManager] Unregistered from ServiceLocator");
            }
        }

        /// <summary>
        /// QuestManagerの取得
        /// </summary>
        private void FindQuestManager()
        {
            questManager = FindFirstObjectByType<QuestManager>();
            if (questManager == null)
            {
                Debug.LogWarning("[AdventureInventoryManager] QuestManager not found in scene");
            }
        }

        #region Item Management

        /// <summary>
        /// アイテムをインベントリに追加
        /// </summary>
        /// <param name="itemData">追加するアイテム</param>
        /// <param name="quantity">追加する数量</param>
        /// <returns>実際に追加できた数量</returns>
        public int AddItem(AdventureItemData itemData, int quantity = 1)
        {
            if (itemData == null)
            {
                Debug.LogError("[AdventureInventoryManager] Cannot add null item");
                return 0;
            }

            if (quantity <= 0)
            {
                Debug.LogWarning($"[AdventureInventoryManager] Invalid quantity {quantity} for item {itemData.itemName}");
                return 0;
            }

            int originalQuantity = quantity;
            Debug.Log($"[AdventureInventoryManager] Attempting to add {quantity} x {itemData.itemName}");

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
                OnInventoryChanged?.Invoke();

                // クエストの目標進捗を更新
                UpdateQuestObjectiveProgress(QuestObjectiveType.Collect, itemData.itemId, actuallyAdded);

                Debug.Log($"[AdventureInventoryManager] Successfully added {actuallyAdded} x {itemData.itemName}");
            }

            if (quantity > 0)
            {
                OnInventoryFull?.Invoke();
                onInventoryFull?.Raise();
                Debug.LogWarning($"[AdventureInventoryManager] Could not add {quantity} x {itemData.itemName} - inventory full");
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
        public int RemoveItem(AdventureItemData itemData, int quantity = 1)
        {
            if (itemData == null)
            {
                Debug.LogError("[AdventureInventoryManager] Cannot remove null item");
                return 0;
            }

            if (quantity <= 0)
            {
                Debug.LogWarning($"[AdventureInventoryManager] Invalid quantity {quantity} for item {itemData.itemName}");
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
                OnInventoryChanged?.Invoke();

                Debug.Log($"[AdventureInventoryManager] Successfully removed {actuallyRemoved} x {itemData.itemName}");
            }

            return actuallyRemoved;
        }

        /// <summary>
        /// アイテムを使用
        /// </summary>
        /// <param name="itemData">使用するアイテム</param>
        /// <returns>使用に成功したかどうか</returns>
        public bool UseItem(AdventureItemData itemData)
        {
            if (itemData == null || !itemData.isUsable)
            {
                Debug.LogWarning("[AdventureInventoryManager] Item is not usable");
                return false;
            }

            if (!HasItem(itemData))
            {
                Debug.LogWarning($"[AdventureInventoryManager] Don't have item {itemData.itemName}");
                return false;
            }

            // アイテム効果を適用（Adventure Template特有の効果）
            ApplyItemEffect(itemData);

            // アイテムを消費（消費アイテムの場合）
            if (itemData.isConsumable)
            {
                RemoveItem(itemData, 1);
            }

            OnItemUsed?.Invoke(itemData);
            onItemUsed?.Raise(itemData);

            // クエストの目標進捗を更新
            UpdateQuestObjectiveProgress(QuestObjectiveType.Use, itemData.itemId, 1);

            Debug.Log($"[AdventureInventoryManager] Used item {itemData.itemName}");
            return true;
        }

        /// <summary>
        /// アイテムの所持数を取得
        /// </summary>
        public int GetItemCount(AdventureItemData itemData)
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
        public bool HasItem(AdventureItemData itemData, int requiredQuantity = 1)
        {
            return GetItemCount(itemData) >= requiredQuantity;
        }

        /// <summary>
        /// アイテム効果の適用
        /// </summary>
        private void ApplyItemEffect(AdventureItemData itemData)
        {
            switch (itemData.itemType)
            {
                case AdventureItemType.Consumable:
                    ApplyConsumableEffect(itemData);
                    break;
                case AdventureItemType.Tool:
                    ApplyToolEffect(itemData);
                    break;
                case AdventureItemType.Key:
                    // キーアイテムは使用時に特別な処理が必要
                    Debug.Log($"[AdventureInventoryManager] Key item {itemData.itemName} used");
                    break;
                case AdventureItemType.Document:
                    // 文書アイテムは読み物として扱う
                    Debug.Log($"[AdventureInventoryManager] Reading document: {itemData.itemName}");
                    break;
            }
        }

        /// <summary>
        /// 消費アイテム効果の適用
        /// </summary>
        private void ApplyConsumableEffect(AdventureItemData itemData)
        {
            Debug.Log($"[AdventureInventoryManager] Applied consumable effect for {itemData.itemName}");
            // 実際の効果実装はここに追加
            // 例: HP回復、MP回復、バフ効果など
        }

        /// <summary>
        /// ツールアイテム効果の適用
        /// </summary>
        private void ApplyToolEffect(AdventureItemData itemData)
        {
            Debug.Log($"[AdventureInventoryManager] Applied tool effect for {itemData.itemName}");
            // 実際の効果実装はここに追加
            // 例: 環境とのインタラクション、パズル解決など
        }

        #endregion

        #region Quest Integration

        /// <summary>
        /// クエスト目標の進捗を更新
        /// </summary>
        private void UpdateQuestObjectiveProgress(QuestObjectiveType objectiveType, string targetId, int amount)
        {
            if (questManager == null)
                return;

            // クエストマネージャーに進捗更新を通知
            // QuestManagerに対応するメソッドを呼び出し
            Debug.Log($"[AdventureInventoryManager] Updating quest objective: {objectiveType} {targetId} +{amount}");
        }

        /// <summary>
        /// クエスト報酬としてアイテムを受け取る
        /// </summary>
        public void ReceiveQuestReward(List<QuestReward> rewards)
        {
            foreach (var reward in rewards)
            {
                if (reward.rewardType == QuestRewardType.Item && reward.itemReward != null)
                {
                    AddItem(reward.itemReward, reward.quantity);
                    Debug.Log($"[AdventureInventoryManager] Received quest reward: {reward.quantity}x {reward.itemReward.itemName}");
                }
            }
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
                if (slotIndex >= maxInventorySlots)
                    break;

                inventorySlots[slotIndex].SetItem(item.Item, item.Quantity);
                slotIndex++;
            }

            OnInventoryChanged?.Invoke();
            Debug.Log("[AdventureInventoryManager] Inventory sorted");
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

            OnInventoryChanged?.Invoke();
            Debug.Log("[AdventureInventoryManager] Inventory cleared");
        }

        #endregion

        #region Debug Tools

        [TabGroup("Debug", "Debug Tools")]
        [Button("Add Test Items")]
        private void AddTestItems()
        {
            Debug.Log("[AdventureInventoryManager] Adding test items...");
            // テスト用のアイテム追加機能
        }

        [TabGroup("Debug", "Debug Tools")]
        [Button("Clear All Items")]
        private void ClearAllItems()
        {
            ClearInventory();
        }

        [TabGroup("Debug", "Debug Tools")]
        [Button("Sort Inventory")]
        private void SortInventoryDebug()
        {
            SortInventory();
        }

        [TabGroup("Debug", "Debug Tools")]
        [Button("Show Inventory Status")]
        private void ShowInventoryStatus()
        {
            Debug.Log("=== Adventure Inventory Status ===");
            Debug.Log($"Used Slots: {UsedSlots}/{maxInventorySlots}");
            Debug.Log($"Total Items: {TotalItemCount}");
            Debug.Log($"Is Full: {IsFull}");

            int itemTypeCount = 0;
            foreach (var slot in inventorySlots.Where(s => s.HasItem))
            {
                Debug.Log($"Slot {slot.SlotIndex}: {slot.quantity}x {slot.itemData.itemName} ({slot.itemData.itemType})");
                itemTypeCount++;
            }
            Debug.Log($"Unique Item Types: {itemTypeCount}");
            Debug.Log("================================");
        }

        #endregion
    }

    /// <summary>
    /// Adventure Template用インベントリスロット
    /// </summary>
    [System.Serializable]
    public class AdventureInventorySlot
    {
        [SerializeField]
        private int slotIndex;

        [SerializeField]
        private AdventureItemData _itemData;

        [SerializeField]
        private int _quantity;

        public AdventureItemData itemData => _itemData;
        public int quantity => _quantity;
        public bool HasItem => _itemData != null && _quantity > 0;
        public bool IsEmpty => !HasItem;
        public int SlotIndex => slotIndex;

        public AdventureInventorySlot(int index)
        {
            slotIndex = index;
            Clear();
        }

        /// <summary>
        /// アイテムをスロットに設定
        /// </summary>
        public int SetItem(AdventureItemData item, int qty)
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