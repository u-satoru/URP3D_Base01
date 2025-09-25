using UnityEngine;
// using asterivo.Unity60.Core.Commands;

namespace asterivo.Unity60.Core.Commands.Definitions
{
    /// <summary>
    /// Item use command definition.
    /// Encapsulates player's inventory item use actions.
    ///
    /// Main features:
    /// - Various item types (consumables, weapons, tools, etc.)
    /// - Item use conditions and restrictions management
    /// - Use effect application and duration management
    /// - Animation and effect control
    /// </summary>
    [System.Serializable]
    public class UseItemCommandDefinition : ICommandDefinition
    {
        /// <summary>
        /// Item use type enumeration
        /// </summary>
        public enum UseType
        {
            Instant,        // Instant use (consumables, etc.)
            Equip,          // Equipment (weapons, armor, etc.)
            Activate,       // Activate (tools, skill items, etc.)
            Consume,        // Consumable use
            Toggle          // On/Off toggle
        }

        [Header("Item Usage Parameters")]
        public UseType useType = UseType.Instant;
        public string targetItemId = "";
        public int itemSlotIndex = -1; // -1 = auto-detect
        public bool consumeOnUse = true;

        [Header("Usage Conditions")]
        public bool requiresTargeting = false;
        public float maxTargetDistance = 5f;
        public LayerMask validTargetLayers = -1;
        public bool canUseInCombat = true;
        public bool canUseWhileMoving = true;

        [Header("Effect Application")]
        public bool applyToSelf = true;
        public bool canApplyToOthers = false;
        public float effectDuration = 0f; // 0 = permanent/instant
        public bool stackable = false;

        [Header("Animation & Timing")]
        public bool playUseAnimation = true;
        public string useAnimationTrigger = "UseItem";
        public float animationDuration = 1f;
        public float usageDelay = 0f; // Delay before effect application

        [Header("Cooldown")]
        public float cooldownDuration = 0f;
        public bool globalCooldown = false; // Whether to limit use of all items

        [Header("Effects")]
        public bool showUseEffect = true;
        public bool showTargetingUI = false;
        public string useEffectName = "";

        /// <summary>
        /// Default constructor
        /// </summary>
        public UseItemCommandDefinition()
        {
        }

        /// <summary>
        /// Parameterized constructor
        /// </summary>
        public UseItemCommandDefinition(UseType type, string itemId, bool consume = true)
        {
            useType = type;
            targetItemId = itemId;
            consumeOnUse = consume;
        }

        /// <summary>
        /// Check if item use command can be executed
        /// </summary>
        public bool CanExecute(object context = null)
        {
            // Basic executability check
            if (string.IsNullOrEmpty(targetItemId) && itemSlotIndex < 0) return false;

            if (requiresTargeting && maxTargetDistance <= 0f) return false;
            if (animationDuration < 0f || usageDelay < 0f) return false;

            // Additional checks if context exists
            if (context != null)
            {
                // Item existence check in inventory
                // Item usability state check
                // Cooldown check
                // Use condition check (in combat, moving, etc.)
                // Player state check (stun, silence, etc.)
            }

            return true;
        }

        /// <summary>
        /// Create item use command instance
        /// </summary>
        public ICommand CreateCommand(object context = null)
        {
            if (!CanExecute(context))
                return null;

            return new UseItemCommand(this, context);
        }
    }

    /// <summary>
    /// Actual implementation of item use command
    /// </summary>
    public class UseItemCommand : ICommand
    {
        private UseItemCommandDefinition definition;
        private object context;
        private bool executed = false;
        private IUsableItem targetItem;
        private GameObject targetObject;
        private bool isEffectActive = false;
        private float effectStartTime = 0f;

        public UseItemCommand(UseItemCommandDefinition useItemDefinition, object executionContext)
        {
            definition = useItemDefinition;
            context = executionContext;
        }

        /// <summary>
        /// Execute item use command
        /// </summary>
        public void Execute()
        {
            if (executed) return;

            // Get target item
            targetItem = GetTargetItem();
            if (targetItem == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.LogWarning($"Target item not found: {definition.targetItemId}");
#endif
                return;
            }

            // Process if targeting is required
            if (definition.requiresTargeting)
            {
                targetObject = FindTarget();
                if (targetObject == null && definition.requiresTargeting)
                {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    UnityEngine.Debug.LogWarning("No valid target found for item use");
#endif
                    return;
                }
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Executing {definition.useType} item use: {targetItem.GetItemName()}");
#endif

            // Play use animation
            if (definition.playUseAnimation)
            {
                PlayUseAnimation();
            }

            // If there's a delay, execute after delay, otherwise execute immediately
            if (definition.usageDelay > 0f)
            {
                // Actual implementation would use Coroutine or Timer for delayed execution
                // Currently executing immediately
                ApplyItemEffect();
            }
            else
            {
                ApplyItemEffect();
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
        /// Get target item
        /// </summary>
        private IUsableItem GetTargetItem()
        {
            // Actual implementation would integrate with InventorySystem

            // Search by item ID
            if (!string.IsNullOrEmpty(definition.targetItemId))
            {
                // return inventorySystem.GetItemById(definition.targetItemId);
            }

            // Search by slot index
            if (definition.itemSlotIndex >= 0)
            {
                // return inventorySystem.GetItemAtSlot(definition.itemSlotIndex);
            }

            // Temporary implementation
            return new MockUsableItem(definition.targetItemId);
        }

        /// <summary>
        /// Find target object
        /// </summary>
        private GameObject FindTarget()
        {
            if (context is not MonoBehaviour mono) return null;

            // Raycast forward from camera or player
            Ray ray = new Ray(mono.transform.position, mono.transform.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, definition.maxTargetDistance, definition.validTargetLayers))
            {
                return hit.collider.gameObject;
            }

            return null;
        }

        /// <summary>
        /// Play use animation
        /// </summary>
        private void PlayUseAnimation()
        {
            if (context is not MonoBehaviour mono) return;

            var animator = mono.GetComponent<Animator>();
            if (animator != null && !string.IsNullOrEmpty(definition.useAnimationTrigger))
            {
                animator.SetTrigger(definition.useAnimationTrigger);
            }
        }

        /// <summary>
        /// Apply item effect
        /// </summary>
        private void ApplyItemEffect()
        {
            if (targetItem == null) return;

            switch (definition.useType)
            {
                case UseItemCommandDefinition.UseType.Instant:
                    ApplyInstantEffect();
                    break;
                case UseItemCommandDefinition.UseType.Equip:
                    ApplyEquipEffect();
                    break;
                case UseItemCommandDefinition.UseType.Activate:
                    ApplyActivateEffect();
                    break;
                case UseItemCommandDefinition.UseType.Consume:
                    ApplyConsumeEffect();
                    break;
                case UseItemCommandDefinition.UseType.Toggle:
                    ApplyToggleEffect();
                    break;
            }

            // Item consumption processing
            if (definition.consumeOnUse)
            {
                ConsumeItem();
            }

            // Show effects
            if (definition.showUseEffect)
            {
                ShowUseEffect();
            }

            // Start cooldown
            if (definition.cooldownDuration > 0f)
            {
                StartCooldown();
            }
        }

        /// <summary>
        /// Apply instant effect
        /// </summary>
        private void ApplyInstantEffect()
        {
            var target = definition.applyToSelf ? context : targetObject;
            targetItem.ApplyInstantEffect(target);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Applied instant effect from {targetItem.GetItemName()}");
#endif
        }

        /// <summary>
        /// Apply equip effect
        /// </summary>
        private void ApplyEquipEffect()
        {
            if (context is MonoBehaviour mono)
            {
                // Actual implementation would integrate with EquipmentSystem
                // equipmentSystem.EquipItem(targetItem);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.Log($"Equipped {targetItem.GetItemName()}");
#endif
            }
        }

        /// <summary>
        /// Apply activate effect
        /// </summary>
        private void ApplyActivateEffect()
        {
            isEffectActive = true;
            effectStartTime = Time.time;

            targetItem.OnActivate(context, targetObject);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Activated {targetItem.GetItemName()}");
#endif
        }

        /// <summary>
        /// Apply consume effect
        /// </summary>
        private void ApplyConsumeEffect()
        {
            var target = definition.applyToSelf ? context : targetObject;
            targetItem.OnConsume(target);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Consumed {targetItem.GetItemName()}");
#endif
        }

        /// <summary>
        /// Apply toggle effect
        /// </summary>
        private void ApplyToggleEffect()
        {
            // Get current state of item and toggle it
            bool currentState = targetItem.GetToggleState();
            targetItem.SetToggleState(!currentState);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Toggled {targetItem.GetItemName()} to {!currentState}");
#endif
        }

        /// <summary>
        /// Consume item
        /// </summary>
        private void ConsumeItem()
        {
            // Actual implementation would integrate with InventorySystem
            // inventorySystem.ConsumeItem(targetItem);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Item consumed: {targetItem.GetItemName()}");
#endif
        }

        /// <summary>
        /// Show use effect
        /// </summary>
        private void ShowUseEffect()
        {
            if (context is not MonoBehaviour mono) return;

            // Particle effects
            if (!string.IsNullOrEmpty(definition.useEffectName))
            {
                // effectSystem.PlayEffect(definition.useEffectName, mono.transform.position);
            }

            // Sound effects
            // audioSystem.PlaySound(targetItem.GetUseSound());

            // UI effects
            // uiSystem.ShowItemUseNotification(targetItem);
        }

        /// <summary>
        /// Start cooldown
        /// </summary>
        private void StartCooldown()
        {
            // Actual implementation would integrate with CooldownSystem
            // cooldownSystem.StartCooldown(targetItem.GetItemId(), definition.cooldownDuration);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Started cooldown: {definition.cooldownDuration}s");
#endif
        }

        /// <summary>
        /// Update continuous effect (called periodically from external)
        /// </summary>
        public void UpdateItemEffect(float deltaTime)
        {
            if (!isEffectActive || definition.effectDuration <= 0f) return;

            float elapsedTime = Time.time - effectStartTime;

            if (elapsedTime >= definition.effectDuration)
            {
                EndItemEffect();
            }
            else
            {
                // Update continuous effect processing
                targetItem?.UpdateEffect(context, deltaTime);
            }
        }

        /// <summary>
        /// End item effect
        /// </summary>
        private void EndItemEffect()
        {
            isEffectActive = false;
            targetItem?.OnEffectEnd(context);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Item effect ended: {targetItem?.GetItemName()}");
#endif
        }

        /// <summary>
        /// Undo operation (cancel item use)
        /// </summary>
        public void Undo()
        {
            if (!executed) return;

            // Cancel item effect
            if (isEffectActive)
            {
                EndItemEffect();
            }

            // Restore used item (if consumed)
            if (definition.consumeOnUse && targetItem != null)
            {
                // Actual implementation would integrate with InventorySystem
                // inventorySystem.RestoreItem(targetItem);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.Log($"Restored consumed item: {targetItem.GetItemName()}");
#endif
            }

            // Unequip equipped item
            if (definition.useType == UseItemCommandDefinition.UseType.Equip)
            {
                // equipmentSystem.UnequipItem(targetItem);
            }

            executed = false;
        }

        /// <summary>
        /// Whether this command can be undone
        /// </summary>
        public bool CanUndo => executed && (definition.consumeOnUse || definition.useType == UseItemCommandDefinition.UseType.Equip);

        /// <summary>
        /// Whether item effect is currently active
        /// </summary>
        public bool IsEffectActive => isEffectActive;
    }

    /// <summary>
    /// Usable item interface
    /// </summary>
    public interface IUsableItem
    {
        string GetItemId();
        string GetItemName();
        void ApplyInstantEffect(object target);
        void OnActivate(object user, object target = null);
        void OnConsume(object user);
        void UpdateEffect(object user, float deltaTime);
        void OnEffectEnd(object user);
        bool GetToggleState();
        void SetToggleState(bool state);
    }

    /// <summary>
    /// Mock item implementation for testing
    /// </summary>
    internal class MockUsableItem : IUsableItem
    {
        private string itemId;
        private bool toggleState = false;

        public MockUsableItem(string id)
        {
            itemId = id;
        }

        public string GetItemId() => itemId;
        public string GetItemName() => $"Mock Item ({itemId})";
        public void ApplyInstantEffect(object target) { }
        public void OnActivate(object user, object target = null) { }
        public void OnConsume(object user) { }
        public void UpdateEffect(object user, float deltaTime) { }
        public void OnEffectEnd(object user) { }
        public bool GetToggleState() => toggleState;
        public void SetToggleState(bool state) => toggleState = state;
    }
}