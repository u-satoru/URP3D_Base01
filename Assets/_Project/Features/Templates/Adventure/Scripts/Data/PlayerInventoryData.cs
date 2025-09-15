using System.Collections.Generic;
using UnityEngine;

namespace asterivo.Unity60.Features.Templates.Adventure.Data
{
    /// <summary>
    /// ScriptableObject for storing player inventory data in the Adventure template
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerInventoryData", menuName = "Adventure Template/Player Inventory Data")]
    public class PlayerInventoryData : ScriptableObject
    {
        [Header("Inventory Configuration")]
        [SerializeField] private int maxSlots = 20;
        [SerializeField] private float maxWeight = 100f;
        [SerializeField] private bool allowStacking = true;
        [SerializeField] private int maxStackSize = 99;

        [Header("Starting Items")]
        [SerializeField] private List<InventorySlot> startingItems = new List<InventorySlot>();

        [Header("Runtime Data")]
        [SerializeField] private List<InventorySlot> currentItems = new List<InventorySlot>();
        [SerializeField] private float currentWeight = 0f;

        public int MaxSlots => maxSlots;
        public float MaxWeight => maxWeight;
        public bool AllowStacking => allowStacking;
        public int MaxStackSize => maxStackSize;
        public List<InventorySlot> CurrentItems => currentItems;
        public float CurrentWeight => currentWeight;

        private void OnEnable()
        {
            // Initialize with starting items if current items is empty
            if (currentItems.Count == 0 && startingItems.Count > 0)
            {
                currentItems.AddRange(startingItems);
                RecalculateWeight();
            }
        }

        public void InitializeInventory()
        {
            currentItems.Clear();
            currentItems.AddRange(startingItems);
            RecalculateWeight();
            
            Debug.Log($"[PlayerInventoryData] Initialized inventory with {currentItems.Count} starting items");
        }

        public void ClearInventory()
        {
            currentItems.Clear();
            currentWeight = 0f;
        }

        public void RecalculateWeight()
        {
            currentWeight = 0f;
            foreach (var slot in currentItems)
            {
                if (slot.item != null)
                {
                    currentWeight += slot.item.Weight * slot.quantity;
                }
            }
        }

        public bool HasSpace(ItemData item, int quantity = 1)
        {
            if (item == null) return false;

            // Check weight limit
            float totalWeight = item.Weight * quantity;
            if (currentWeight + totalWeight > maxWeight)
                return false;

            // Check for existing stacks
            if (allowStacking && item.IsStackable)
            {
                foreach (var slot in currentItems)
                {
                    if (slot.item == item && slot.quantity < maxStackSize)
                    {
                        int availableSpace = maxStackSize - slot.quantity;
                        if (availableSpace >= quantity)
                            return true;
                        quantity -= availableSpace;
                    }
                }
            }

            // Check for empty slots
            int emptySlots = maxSlots - currentItems.Count;
            int requiredSlots = Mathf.CeilToInt((float)quantity / (allowStacking && item.IsStackable ? maxStackSize : 1));
            
            return emptySlots >= requiredSlots;
        }

        public bool AddItem(ItemData item, int quantity = 1)
        {
            if (!HasSpace(item, quantity)) return false;

            // Try to stack with existing items first
            if (allowStacking && item.IsStackable)
            {
                for (int i = 0; i < currentItems.Count && quantity > 0; i++)
                {
                    var slot = currentItems[i];
                    if (slot.item == item && slot.quantity < maxStackSize)
                    {
                        int spaceInSlot = maxStackSize - slot.quantity;
                        int amountToAdd = Mathf.Min(spaceInSlot, quantity);
                        
                        slot.quantity += amountToAdd;
                        quantity -= amountToAdd;
                    }
                }
            }

            // Add remaining quantity to new slots
            while (quantity > 0 && currentItems.Count < maxSlots)
            {
                int amountForThisSlot = allowStacking && item.IsStackable ? 
                    Mathf.Min(quantity, maxStackSize) : 1;
                
                currentItems.Add(new InventorySlot
                {
                    item = item,
                    quantity = amountForThisSlot
                });
                
                quantity -= amountForThisSlot;
            }

            RecalculateWeight();
            return quantity == 0;
        }

        public bool RemoveItem(ItemData item, int quantity = 1)
        {
            if (!HasItem(item, quantity)) return false;

            for (int i = currentItems.Count - 1; i >= 0 && quantity > 0; i--)
            {
                var slot = currentItems[i];
                if (slot.item == item)
                {
                    int removeFromSlot = Mathf.Min(slot.quantity, quantity);
                    slot.quantity -= removeFromSlot;
                    quantity -= removeFromSlot;

                    if (slot.quantity <= 0)
                    {
                        currentItems.RemoveAt(i);
                    }
                }
            }

            RecalculateWeight();
            return quantity == 0;
        }

        public bool HasItem(ItemData item, int quantity = 1)
        {
            if (item == null) return false;

            int totalQuantity = 0;
            foreach (var slot in currentItems)
            {
                if (slot.item == item)
                {
                    totalQuantity += slot.quantity;
                }
            }

            return totalQuantity >= quantity;
        }

        public int GetItemQuantity(ItemData item)
        {
            if (item == null) return 0;

            int totalQuantity = 0;
            foreach (var slot in currentItems)
            {
                if (slot.item == item)
                {
                    totalQuantity += slot.quantity;
                }
            }

            return totalQuantity;
        }

        public List<ItemData> GetAllUniqueItems()
        {
            List<ItemData> uniqueItems = new List<ItemData>();
            
            foreach (var slot in currentItems)
            {
                if (slot.item != null && !uniqueItems.Contains(slot.item))
                {
                    uniqueItems.Add(slot.item);
                }
            }

            return uniqueItems;
        }

        public InventorySlot GetSlot(int index)
        {
            if (index >= 0 && index < currentItems.Count)
                return currentItems[index];
            return null;
        }

        public void SetSlot(int index, InventorySlot slot)
        {
            if (index >= 0 && index < maxSlots)
            {
                if (index >= currentItems.Count)
                {
                    // Expand list to fit index
                    while (currentItems.Count <= index)
                    {
                        currentItems.Add(new InventorySlot());
                    }
                }
                
                currentItems[index] = slot;
                RecalculateWeight();
            }
        }

        public bool SwapSlots(int indexA, int indexB)
        {
            if (indexA >= 0 && indexA < currentItems.Count && 
                indexB >= 0 && indexB < currentItems.Count)
            {
                var temp = currentItems[indexA];
                currentItems[indexA] = currentItems[indexB];
                currentItems[indexB] = temp;
                return true;
            }
            return false;
        }

        public void SortInventory()
        {
            currentItems.Sort((a, b) => {
                if (a.item == null && b.item == null) return 0;
                if (a.item == null) return 1;
                if (b.item == null) return -1;
                
                // Sort by item type, then by name
                int typeComparison = a.item.ItemType.CompareTo(b.item.ItemType);
                return typeComparison != 0 ? typeComparison : string.Compare(a.item.ItemName, b.item.ItemName);
            });
        }

        #if UNITY_EDITOR
        [ContextMenu("Add Debug Items")]
        private void AddDebugItems()
        {
            // Add some test items for debugging
            startingItems.Clear();
            
            // Create basic test items (these would need to exist as ScriptableObject assets)
            Debug.Log("[PlayerInventoryData] Debug items would be added here - create ItemData assets first");
        }

        [ContextMenu("Clear All Items")]
        private void ClearAllItems()
        {
            currentItems.Clear();
            startingItems.Clear();
            currentWeight = 0f;
            Debug.Log("[PlayerInventoryData] Cleared all items");
        }
        #endif
    }

    [System.Serializable]
    public class InventorySlot
    {
        public ItemData item;
        public int quantity = 1;
        
        public InventorySlot()
        {
            item = null;
            quantity = 0;
        }
        
        public InventorySlot(ItemData itemData, int qty)
        {
            item = itemData;
            quantity = qty;
        }
        
        public bool IsEmpty => item == null || quantity <= 0;
        public bool HasItem => item != null && quantity > 0;
    }
}