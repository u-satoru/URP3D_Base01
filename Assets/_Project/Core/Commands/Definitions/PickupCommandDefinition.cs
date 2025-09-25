using UnityEngine;
// using asterivo.Unity60.Core.Commands;

namespace asterivo.Unity60.Core.Commands.Definitions
{
    /// <summary>
    /// Item pickup command definition.
    /// Encapsulates player's item pickup actions.
    ///
    /// Main features:
    /// - Manual or automatic item pickup
    /// - Inventory capacity and item restriction management
    /// - Pickup range and filtering
    /// - Item information display and feedback
    /// </summary>
    [System.Serializable]
    public class PickupCommandDefinition : ICommandDefinition
    {
        /// <summary>
        /// Pickup type enumeration
        /// </summary>
        public enum PickupType
        {
            Manual,         // Manual pickup
            Auto,           // Automatic pickup
            Selective,      // Selective pickup
            Area,           // Area pickup
            Magnetic        // Magnetic pickup
        }

        [Header("Pickup Parameters")]
        public PickupType pickupType = PickupType.Manual;
        public float pickupRange = 1.5f;
        public LayerMask itemLayer = -1;
        public string itemTag = "Item";

        [Header("Item Filtering")]
        [Tooltip("Allowed item types for pickup")]
        public string[] allowedItemTypes = { "Consumable", "Weapon", "Armor", "Key" };
        [Tooltip("Excluded item types")]
        public string[] excludedItemTypes = { };
        public bool respectInventoryCapacity = true;

        [Header("Area Pickup")]
        [Tooltip("Effective radius for area pickup")]
        public float areaRadius = 3f;
        [Tooltip("Maximum number of items to pickup at once")]
        public int maxPickupCount = 10;

        [Header("Magnetic Pickup")]
        [Tooltip("Force to attract items")]
        public float magneticForce = 5f;
        [Tooltip("Duration of magnetic effect")]
        public float magneticDuration = 2f;

        [Header("Animation & Effects")]
        public bool playPickupAnimation = true;
        public string pickupAnimationTrigger = "Pickup";
        public bool showPickupEffect = true;
        public bool showItemInfo = true;

        [Header("Audio")]
        public bool playPickupSound = true;
        public string pickupSoundName = "ItemPickup";

        /// <summary>
        /// Default constructor
        /// </summary>
        public PickupCommandDefinition()
        {
        }

        /// <summary>
        /// Parameterized constructor
        /// </summary>
        public PickupCommandDefinition(PickupType type, float range, bool autoPickup = false)
        {
            pickupType = type;
            pickupRange = range;
        }

        /// <summary>
        /// Check if pickup command can be executed
        /// </summary>
        public bool CanExecute(object context = null)
        {
            // Basic executability check
            if (pickupRange <= 0f) return false;

            if (pickupType == PickupType.Area && areaRadius <= 0f) return false;
            if (pickupType == PickupType.Magnetic && (magneticForce <= 0f || magneticDuration <= 0f)) return false;

            // Additional checks if context exists
            if (context != null)
            {
                // Inventory capacity check
                // Player state check (moving, combat restrictions, etc.)
                // Check for pickupable items in range
            }

            return true;
        }

        /// <summary>
        /// Create pickup command instance
        /// </summary>
        public ICommand CreateCommand(object context = null)
        {
            if (!CanExecute(context))
                return null;

            return new PickupCommand(this, context);
        }
    }

    /// <summary>
    /// Actual implementation of pickup command
    /// </summary>
    public class PickupCommand : ICommand
    {
        private PickupCommandDefinition definition;
        private object context;
        private bool executed = false;
        private System.Collections.Generic.List<GameObject> pickedUpItems = new System.Collections.Generic.List<GameObject>();
        private bool isMagneticActive = false;

        public PickupCommand(PickupCommandDefinition pickupDefinition, object executionContext)
        {
            definition = pickupDefinition;
            context = executionContext;
        }

        /// <summary>
        /// Execute pickup command
        /// </summary>
        public void Execute()
        {
            if (executed) return;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Executing {definition.pickupType} pickup: range={definition.pickupRange}");
#endif

            switch (definition.pickupType)
            {
                case PickupCommandDefinition.PickupType.Manual:
                    ExecuteManualPickup();
                    break;
                case PickupCommandDefinition.PickupType.Auto:
                    ExecuteAutoPickup();
                    break;
                case PickupCommandDefinition.PickupType.Selective:
                    ExecuteSelectivePickup();
                    break;
                case PickupCommandDefinition.PickupType.Area:
                    ExecuteAreaPickup();
                    break;
                case PickupCommandDefinition.PickupType.Magnetic:
                    ExecuteMagneticPickup();
                    break;
            }

            executed = true;
        }

        /// <summary>
        /// Check if command can be executed
        /// </summary>
        public bool CanExecute()
        {
            return !executed && definition.CanExecute(context);
        }

        /// <summary>
        /// Execute manual pickup
        /// </summary>
        private void ExecuteManualPickup()
        {
            var targetItem = FindNearestPickupableItem();
            if (targetItem != null)
            {
                PickupItem(targetItem);
            }
        }

        /// <summary>
        /// Execute automatic pickup
        /// </summary>
        private void ExecuteAutoPickup()
        {
            var items = FindAllPickupableItems(definition.pickupRange);
            foreach (var item in items)
            {
                if (!CanPickupItem(item)) continue;

                PickupItem(item);

                // Stop if inventory becomes full
                if (definition.respectInventoryCapacity && IsInventoryFull())
                    break;
            }
        }

        /// <summary>
        /// Execute selective pickup
        /// </summary>
        private void ExecuteSelectivePickup()
        {
            var items = FindAllPickupableItems(definition.pickupRange);

            // Get items in priority order
            var prioritizedItems = SortItemsByPriority(items);

            foreach (var item in prioritizedItems)
            {
                if (!CanPickupItem(item)) continue;

                PickupItem(item);

                if (definition.respectInventoryCapacity && IsInventoryFull())
                    break;
            }
        }

        /// <summary>
        /// Execute area pickup
        /// </summary>
        private void ExecuteAreaPickup()
        {
            var items = FindAllPickupableItems(definition.areaRadius);
            int pickupCount = 0;

            foreach (var item in items)
            {
                if (!CanPickupItem(item)) continue;
                if (pickupCount >= definition.maxPickupCount) break;

                PickupItem(item);
                pickupCount++;

                if (definition.respectInventoryCapacity && IsInventoryFull())
                    break;
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Area pickup completed: {pickupCount} items collected");
#endif
        }

        /// <summary>
        /// Execute magnetic pickup
        /// </summary>
        private void ExecuteMagneticPickup()
        {
            var items = FindAllPickupableItems(definition.areaRadius);
            isMagneticActive = true;

            // Start attracting items
            foreach (var item in items)
            {
                if (!CanPickupItem(item)) continue;

                StartItemAttraction(item);
            }

            // Magnetic effect duration processing (actual implementation would use Coroutine or UpdateLoop)
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Magnetic pickup started: attracting {items.Count} items");
#endif
        }

        /// <summary>
        /// Find nearest pickupable item
        /// </summary>
        private GameObject FindNearestPickupableItem()
        {
            if (context is not MonoBehaviour mono) return null;

            Collider[] nearbyItems = Physics.OverlapSphere(mono.transform.position, definition.pickupRange, definition.itemLayer);

            GameObject nearestItem = null;
            float nearestDistance = float.MaxValue;

            foreach (var itemCollider in nearbyItems)
            {
                if (!IsValidPickupTarget(itemCollider.gameObject)) continue;

                float distance = Vector3.Distance(mono.transform.position, itemCollider.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestItem = itemCollider.gameObject;
                }
            }

            return nearestItem;
        }

        /// <summary>
        /// Find all pickupable items
        /// </summary>
        private System.Collections.Generic.List<GameObject> FindAllPickupableItems(float searchRange)
        {
            var items = new System.Collections.Generic.List<GameObject>();

            if (context is not MonoBehaviour mono) return items;

            Collider[] nearbyItems = Physics.OverlapSphere(mono.transform.position, searchRange, definition.itemLayer);

            foreach (var itemCollider in nearbyItems)
            {
                if (IsValidPickupTarget(itemCollider.gameObject))
                {
                    items.Add(itemCollider.gameObject);
                }
            }

            return items;
        }

        /// <summary>
        /// Check if item is valid pickup target
        /// </summary>
        private bool IsValidPickupTarget(GameObject item)
        {
            // Tag check
            if (!string.IsNullOrEmpty(definition.itemTag) && !item.CompareTag(definition.itemTag))
                return false;

            // Item component existence check
            var itemComponent = item.GetComponent<IPickupableItem>();
            if (itemComponent == null) return false;

            // Item type filtering
            string itemType = itemComponent.GetItemType();

            // Exclusion list check
            if (definition.excludedItemTypes.Length > 0)
            {
                foreach (var excludedType in definition.excludedItemTypes)
                {
                    if (itemType == excludedType) return false;
                }
            }

            // Allowed list check
            if (definition.allowedItemTypes.Length > 0)
            {
                bool isAllowed = false;
                foreach (var allowedType in definition.allowedItemTypes)
                {
                    if (itemType == allowedType)
                    {
                        isAllowed = true;
                        break;
                    }
                }
                if (!isAllowed) return false;
            }

            return true;
        }

        /// <summary>
        /// Check if item can be picked up
        /// </summary>
        private bool CanPickupItem(GameObject item)
        {
            if (definition.respectInventoryCapacity && IsInventoryFull())
                return false;

            var itemComponent = item.GetComponent<IPickupableItem>();
            return itemComponent?.CanBePickedUp() ?? false;
        }

        /// <summary>
        /// Check if inventory is full
        /// </summary>
        private bool IsInventoryFull()
        {
            // Actual implementation would integrate with InventorySystem
            return false; // Temporary value
        }

        /// <summary>
        /// Sort items by priority
        /// </summary>
        private System.Collections.Generic.List<GameObject> SortItemsByPriority(System.Collections.Generic.List<GameObject> items)
        {
            // Actual implementation would determine priority by item value, rarity, necessity, etc.
            return items; // Temporary implementation
        }

        /// <summary>
        /// Actual item pickup processing
        /// </summary>
        private void PickupItem(GameObject item)
        {
            var itemComponent = item.GetComponent<IPickupableItem>();
            if (itemComponent == null) return;

            // Get item data
            var itemData = itemComponent.GetItemData();

            // Add to inventory
            // Actual implementation would integrate with InventorySystem

            // Remove item from world
            pickedUpItems.Add(item); // Save for Undo
            item.SetActive(false);

            // Effects and feedback
            PlayPickupEffects(item, itemData);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Picked up item: {itemData.name}");
#endif
        }

        /// <summary>
        /// Start item magnetic attraction
        /// </summary>
        private void StartItemAttraction(GameObject item)
        {
            // Actual implementation would use physics force or Tween animation to attract items
            // Call PickupItem() when attraction is complete
        }

        /// <summary>
        /// Play pickup effects
        /// </summary>
        private void PlayPickupEffects(GameObject item, IItemData itemData)
        {
            if (context is not MonoBehaviour mono) return;

            // Animation
            if (definition.playPickupAnimation)
            {
                var animator = mono.GetComponent<Animator>();
                if (animator != null && !string.IsNullOrEmpty(definition.pickupAnimationTrigger))
                {
                    animator.SetTrigger(definition.pickupAnimationTrigger);
                }
            }

            // Particle effects
            if (definition.showPickupEffect)
            {
                // Generate effects
            }

            // Sound effects
            if (definition.playPickupSound)
            {
                // Integration with AudioSystem
            }

            // Item info display
            if (definition.showItemInfo)
            {
                // UI display (item name, description, etc.)
            }
        }

        /// <summary>
        /// Update magnetic pickup (called periodically from external)
        /// </summary>
        public void UpdateMagneticPickup(float deltaTime)
        {
            if (!isMagneticActive) return;

            // Update attraction processing
            // Duration management
        }

        /// <summary>
        /// Undo operation (cancel pickup)
        /// </summary>
        public void Undo()
        {
            if (!executed) return;

            // Return picked up items to original positions
            foreach (var item in pickedUpItems)
            {
                if (item != null)
                {
                    item.SetActive(true);
                    // Remove from inventory
                    // Actual implementation would integrate with InventorySystem
                }
            }

            pickedUpItems.Clear();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log("Pickup undone - items restored");
#endif

            executed = false;
        }

        /// <summary>
        /// Whether this command can be undone
        /// </summary>
        public bool CanUndo => executed && pickedUpItems.Count > 0;

        /// <summary>
        /// Whether magnetic effect is currently active
        /// </summary>
        public bool IsMagneticActive => isMagneticActive;
    }

    /// <summary>
    /// Pickupable item interface
    /// </summary>
    public interface IPickupableItem
    {
        string GetItemType();
        IItemData GetItemData();
        bool CanBePickedUp();
        void OnPickedUp(object picker);
    }

    /// <summary>
    /// Item data interface
    /// </summary>
    public interface IItemData
    {
        string name { get; }
        string description { get; }
        int value { get; }
        float weight { get; }
        Sprite icon { get; }
    }
}