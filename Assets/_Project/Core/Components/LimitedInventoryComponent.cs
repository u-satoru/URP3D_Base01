using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using asterivo.Unity60.Core.Events;

namespace asterivo.Unity60.Core.Components
{
    /// <summary>
    /// Limited inventory component for survival horror resource management.
    /// Provides slot-based inventory with strategic constraints.
    /// Integrates with Event-Driven Architecture for UI and system communication.
    /// </summary>
    public class LimitedInventoryComponent : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Inventory Configuration")]
        [SerializeField] private int maxSlots = 8;
        [SerializeField] private bool allowStacking = true;
        [SerializeField] private int maxStackSize = 3;

        [Header("Event Channels")]
        [SerializeField] private GameEvent<InventoryItemData> onItemAdded;
        [SerializeField] private GameEvent<InventoryItemData> onItemRemoved;
        [SerializeField] private GameEvent<InventoryItemData> onItemUsed;
        [SerializeField] private GameEvent onInventoryFull;
        [SerializeField] private GameEvent onInventoryChanged;

        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = false;
        [SerializeField] private List<InventorySlot> debugSlots = new List<InventorySlot>();

        #endregion

        #region Private Fields

        private List<InventorySlot> slots;
        private Dictionary<string, int> itemCounts;

        #endregion

        #region Properties

        /// <summary>
        /// Maximum number of inventory slots
        /// </summary>
        public int MaxSlots => maxSlots;

        /// <summary>
        /// Number of used slots
        /// </summary>
        public int UsedSlots => slots?.Count(slot => slot.HasItem) ?? 0;

        /// <summary>
        /// Number of available slots
        /// </summary>
        public int AvailableSlots => maxSlots - UsedSlots;

        /// <summary>
        /// Is inventory full
        /// </summary>
        public bool IsFull => AvailableSlots == 0;

        /// <summary>
        /// Is inventory empty
        /// </summary>
        public bool IsEmpty => UsedSlots == 0;

        /// <summary>
        /// All inventory slots (read-only)
        /// </summary>
        public IReadOnlyList<InventorySlot> Slots => slots?.AsReadOnly();

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeInventory();
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize inventory system
        /// </summary>
        private void InitializeInventory()
        {
            slots = new List<InventorySlot>(maxSlots);
            itemCounts = new Dictionary<string, int>();

            // Initialize empty slots
            for (int i = 0; i < maxSlots; i++)
            {
                slots.Add(new InventorySlot());
            }

            // Sync debug slots for inspector viewing
            debugSlots = slots;

            if (enableDebugLogs)
            {
                Debug.Log($"[LimitedInventoryComponent] Initialized with {maxSlots} slots");
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Try to add item to inventory
        /// </summary>
        /// <param name="itemData">Item to add</param>
        /// <param name="quantity">Quantity to add</param>
        /// <returns>True if item was added successfully</returns>
        public bool TryAddItem(InventoryItemData itemData, int quantity = 1)
        {
            if (itemData == null || quantity <= 0)
            {
                if (enableDebugLogs)
                {
                    Debug.LogWarning("[LimitedInventoryComponent] Invalid item or quantity");
                }
                return false;
            }

            if (allowStacking && itemData.IsStackable)
            {
                return TryAddStackableItem(itemData, quantity);
            }
            else
            {
                return TryAddNonStackableItem(itemData, quantity);
            }
        }

        /// <summary>
        /// Try to remove item from inventory
        /// </summary>
        /// <param name="itemId">Item ID to remove</param>
        /// <param name="quantity">Quantity to remove</param>
        /// <returns>True if item was removed successfully</returns>
        public bool TryRemoveItem(string itemId, int quantity = 1)
        {
            if (string.IsNullOrEmpty(itemId) || quantity <= 0)
                return false;

            int removedCount = 0;
            
            for (int i = 0; i < slots.Count && removedCount < quantity; i++)
            {
                var slot = slots[i];
                if (slot.HasItem && slot.ItemData.ItemId == itemId)
                {
                    int removeFromSlot = Mathf.Min(slot.Quantity, quantity - removedCount);
                    
                    if (slot.Quantity <= removeFromSlot)
                    {
                        // Remove entire stack
                        var removedItem = slot.ItemData;
                        slot.Clear();
                        removedCount += removeFromSlot;
                        
                        onItemRemoved?.Raise(removedItem);
                    }
                    else
                    {
                        // Reduce stack size
                        slot.Quantity -= removeFromSlot;
                        removedCount += removeFromSlot;
                    }
                }
            }

            if (removedCount > 0)
            {
                UpdateItemCount(itemId);
                FireInventoryChanged();

                if (enableDebugLogs)
                {
                    Debug.Log($"[LimitedInventoryComponent] Removed {removedCount}x {itemId}");
                }
                return true;
            }

            return false;
        }

        /// <summary>
        /// Use item from inventory
        /// </summary>
        /// <param name="itemId">Item ID to use</param>
        /// <returns>True if item was used successfully</returns>
        public bool TryUseItem(string itemId)
        {
            var slot = FindSlotWithItem(itemId);
            if (slot != null && slot.HasItem)
            {
                var itemData = slot.ItemData;
                
                if (slot.Quantity <= 1)
                {
                    slot.Clear();
                }
                else
                {
                    slot.Quantity--;
                }

                UpdateItemCount(itemId);
                onItemUsed?.Raise(itemData);
                FireInventoryChanged();

                if (enableDebugLogs)
                {
                    Debug.Log($"[LimitedInventoryComponent] Used item: {itemId}");
                }
                return true;
            }

            return false;
        }

        /// <summary>
        /// Get item count in inventory
        /// </summary>
        /// <param name="itemId">Item ID</param>
        /// <returns>Total quantity of item</returns>
        public int GetItemCount(string itemId)
        {
            if (string.IsNullOrEmpty(itemId))
                return 0;

            itemCounts.TryGetValue(itemId, out int count);
            return count;
        }

        /// <summary>
        /// Check if item exists in inventory
        /// </summary>
        /// <param name="itemId">Item ID</param>
        /// <returns>True if item exists</returns>
        public bool HasItem(string itemId)
        {
            return GetItemCount(itemId) > 0;
        }

        /// <summary>
        /// Clear all items from inventory
        /// </summary>
        public void ClearInventory()
        {
            for (int i = 0; i < slots.Count; i++)
            {
                slots[i].Clear();
            }

            itemCounts.Clear();
            FireInventoryChanged();

            if (enableDebugLogs)
            {
                Debug.Log("[LimitedInventoryComponent] Inventory cleared");
            }
        }

        /// <summary>
        /// Get all items in inventory
        /// </summary>
        /// <returns>List of all inventory items</returns>
        public List<InventoryItemData> GetAllItems()
        {
            var items = new List<InventoryItemData>();
            
            foreach (var slot in slots)
            {
                if (slot.HasItem)
                {
                    for (int i = 0; i < slot.Quantity; i++)
                    {
                        items.Add(slot.ItemData);
                    }
                }
            }

            return items;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Try to add stackable item
        /// </summary>
        private bool TryAddStackableItem(InventoryItemData itemData, int quantity)
        {
            int remainingQuantity = quantity;

            // Try to add to existing stacks first
            foreach (var slot in slots)
            {
                if (slot.HasItem && slot.ItemData.ItemId == itemData.ItemId)
                {
                    int canAdd = Mathf.Min(maxStackSize - slot.Quantity, remainingQuantity);
                    if (canAdd > 0)
                    {
                        slot.Quantity += canAdd;
                        remainingQuantity -= canAdd;

                        if (remainingQuantity <= 0)
                            break;
                    }
                }
            }

            // Add remaining to new slots
            while (remainingQuantity > 0)
            {
                var emptySlot = FindEmptySlot();
                if (emptySlot == null)
                {
                    onInventoryFull?.Raise();
                    
                    if (enableDebugLogs)
                    {
                        Debug.LogWarning("[LimitedInventoryComponent] Cannot add item - inventory full");
                    }
                    return false;
                }

                int addToSlot = Mathf.Min(maxStackSize, remainingQuantity);
                emptySlot.SetItem(itemData, addToSlot);
                remainingQuantity -= addToSlot;
            }

            UpdateItemCount(itemData.ItemId);
            onItemAdded?.Raise(itemData);
            FireInventoryChanged();

            if (enableDebugLogs)
            {
                Debug.Log($"[LimitedInventoryComponent] Added {quantity}x {itemData.ItemId}");
            }
            return true;
        }

        /// <summary>
        /// Try to add non-stackable item
        /// </summary>
        private bool TryAddNonStackableItem(InventoryItemData itemData, int quantity)
        {
            for (int i = 0; i < quantity; i++)
            {
                var emptySlot = FindEmptySlot();
                if (emptySlot == null)
                {
                    onInventoryFull?.Raise();
                    
                    if (enableDebugLogs)
                    {
                        Debug.LogWarning("[LimitedInventoryComponent] Cannot add item - inventory full");
                    }
                    return i > 0; // Return true if at least one item was added
                }

                emptySlot.SetItem(itemData, 1);
                onItemAdded?.Raise(itemData);
            }

            UpdateItemCount(itemData.ItemId);
            FireInventoryChanged();

            if (enableDebugLogs)
            {
                Debug.Log($"[LimitedInventoryComponent] Added {quantity}x {itemData.ItemId}");
            }
            return true;
        }

        /// <summary>
        /// Find empty slot
        /// </summary>
        private InventorySlot FindEmptySlot()
        {
            return slots.FirstOrDefault(slot => !slot.HasItem);
        }

        /// <summary>
        /// Find slot containing specific item
        /// </summary>
        private InventorySlot FindSlotWithItem(string itemId)
        {
            return slots.FirstOrDefault(slot => slot.HasItem && slot.ItemData.ItemId == itemId);
        }

        /// <summary>
        /// Update item count cache
        /// </summary>
        private void UpdateItemCount(string itemId)
        {
            int totalCount = 0;
            
            foreach (var slot in slots)
            {
                if (slot.HasItem && slot.ItemData.ItemId == itemId)
                {
                    totalCount += slot.Quantity;
                }
            }

            if (totalCount > 0)
            {
                itemCounts[itemId] = totalCount;
            }
            else
            {
                itemCounts.Remove(itemId);
            }
        }

        /// <summary>
        /// Fire inventory changed event
        /// </summary>
        private void FireInventoryChanged()
        {
            onInventoryChanged?.Raise();
            
            // Update debug slots for inspector
            debugSlots = slots;
        }

        #endregion

        #region Debug

        /// <summary>
        /// Get debug information
        /// </summary>
        public string GetDebugInfo()
        {
            return $"Inventory: {UsedSlots}/{MaxSlots} slots | Items: {itemCounts.Count} types | Full: {IsFull}";
        }

        #endregion
    }

    /// <summary>
    /// Inventory slot data structure
    /// </summary>
    [System.Serializable]
    public class InventorySlot
    {
        [SerializeField] private InventoryItemData itemData;
        [SerializeField] private int quantity;

        public InventoryItemData ItemData => itemData;
        public int Quantity 
        { 
            get => quantity; 
            set => quantity = Mathf.Max(0, value); 
        }
        public bool HasItem => itemData != null && quantity > 0;

        public void SetItem(InventoryItemData item, int qty)
        {
            itemData = item;
            quantity = qty;
        }

        public void Clear()
        {
            itemData = null;
            quantity = 0;
        }
    }

    /// <summary>
    /// Inventory item data structure
    /// </summary>
    [System.Serializable]
    public class InventoryItemData
    {
        [SerializeField] private string itemId;
        [SerializeField] private string displayName;
        [SerializeField] private string description;
        [SerializeField] private Sprite icon;
        [SerializeField] private bool isStackable = true;
        [SerializeField] private bool isConsumable = true;
        [SerializeField] private ItemType itemType = ItemType.Consumable;

        public string ItemId => itemId;
        public string DisplayName => displayName;
        public string Description => description;
        public Sprite Icon => icon;
        public bool IsStackable => isStackable;
        public bool IsConsumable => isConsumable;
        public ItemType ItemType => itemType;

        public InventoryItemData(string id, string name, string desc, ItemType type)
        {
            itemId = id;
            displayName = name;
            description = desc;
            itemType = type;
        }
    }

    /// <summary>
    /// Item type enumeration
    /// </summary>
    public enum ItemType
    {
        Consumable,
        Key,
        Tool,
        Document,
        Misc
    }
}
