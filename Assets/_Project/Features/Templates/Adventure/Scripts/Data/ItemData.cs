using UnityEngine;

namespace asterivo.Unity60.Features.Templates.Adventure.Data
{
    /// <summary>
    /// ScriptableObject for storing item data in the Adventure template
    /// </summary>
    [CreateAssetMenu(fileName = "ItemData", menuName = "Adventure Template/Item Data")]
    public class ItemData : ScriptableObject
    {
        [Header("Basic Information")]
        [SerializeField] private string itemName = "New Item";
        [SerializeField, TextArea(3, 5)] private string description = "";
        [SerializeField] private Sprite itemIcon;
        [SerializeField] private GameObject itemPrefab;

        [Header("Item Properties")]
        [SerializeField] private ItemType itemType = ItemType.Consumable;
        [SerializeField] private float weight = 1f;
        [SerializeField] private bool isStackable = true;
        [SerializeField] private int maxStackSize = 99;
        [SerializeField] private int value = 10;

        [Header("Usage Properties")]
        [SerializeField] private bool isUsable = true;
        [SerializeField] private bool consumeOnUse = true;
        [SerializeField] private bool requiresTarget = false;
        [SerializeField] private float useCooldown = 0f;

        // Public Properties
        public string ItemName => itemName;
        public string Description => description;
        public Sprite ItemIcon => itemIcon;
        public GameObject ItemPrefab => itemPrefab;
        public ItemType ItemType => itemType;
        public float Weight => weight;
        public bool IsStackable => isStackable;
        public int MaxStackSize => maxStackSize;
        public int Value => value;
        public bool IsUsable => isUsable;
        public bool ConsumeOnUse => consumeOnUse;
        public bool RequiresTarget => requiresTarget;
        public float UseCooldown => useCooldown;

        /// <summary>
        /// Use the item - override in derived classes for specific behavior
        /// </summary>
        /// <param name="user">The character using the item</param>
        /// <param name="target">The target of the item use (if required)</param>
        /// <returns>True if the item was used successfully</returns>
        public virtual bool UseItem(GameObject user, GameObject target = null)
        {
            if (!isUsable) return false;
            
            if (requiresTarget && target == null)
            {
                Debug.LogWarning($"[ItemData] {itemName} requires a target to be used");
                return false;
            }

            Debug.Log($"[ItemData] Used item: {itemName}");
            return true;
        }

        /// <summary>
        /// Get the display name for UI
        /// </summary>
        public virtual string GetDisplayName()
        {
            return itemName;
        }

        /// <summary>
        /// Get the full description for UI
        /// </summary>
        public virtual string GetFullDescription()
        {
            string fullDesc = description;
            
            if (weight > 0)
                fullDesc += $"\nWeight: {weight}";
                
            if (value > 0)
                fullDesc += $"\nValue: {value}";
                
            return fullDesc;
        }

        /// <summary>
        /// Check if this item can be stacked with another
        /// </summary>
        public virtual bool CanStackWith(ItemData other)
        {
            return other != null && 
                   isStackable && 
                   other.isStackable && 
                   other == this; // Same ScriptableObject reference
        }

        #if UNITY_EDITOR
        private void OnValidate()
        {
            // Ensure reasonable values
            weight = Mathf.Max(0f, weight);
            maxStackSize = Mathf.Max(1, maxStackSize);
            value = Mathf.Max(0, value);
            useCooldown = Mathf.Max(0f, useCooldown);
            
            // Auto-generate name from file name if empty
            if (string.IsNullOrEmpty(itemName))
            {
                itemName = name;
            }
        }
        #endif
    }

    /// <summary>
    /// Types of items in the Adventure template
    /// </summary>
    public enum ItemType
    {
        Consumable,     // Items that can be consumed (potions, food)
        Equipment,      // Items that can be equipped (weapons, armor)
        Tool,           // Items used for interactions (keys, rope)
        QuestItem,      // Items related to quests
        Collectible,    // Items collected for achievements
        Material,       // Crafting materials
        Miscellaneous   // Other items
    }
}