using UnityEngine;
// using asterivo.Unity60.Core.Commands;

namespace asterivo.Unity60.Core.Commands.Definitions
{
    /// <summary>
    /// 繧｢繧､繝・Β菴ｿ逕ｨ繧ｳ繝槭Φ繝峨・螳夂ｾｩ縲・    /// 繝励Ξ繧､繝､繝ｼ縺ｮ繧､繝ｳ繝吶Φ繝医Μ繧｢繧､繝・Β菴ｿ逕ｨ繧｢繧ｯ繧ｷ繝ｧ繝ｳ繧偵き繝励そ繝ｫ蛹悶＠縺ｾ縺吶・    /// 
    /// 荳ｻ縺ｪ讖溯・・・    /// - 蜷・ｨｮ繧｢繧､繝・Β繧ｿ繧､繝暦ｼ域ｶ郁怜刀縲∵ｭｦ蝎ｨ縲・亟蜈ｷ遲会ｼ峨・菴ｿ逕ｨ
    /// - 繧｢繧､繝・Β菴ｿ逕ｨ譚｡莉ｶ縺ｨ蛻ｶ邏・・邂｡逅・    /// - 菴ｿ逕ｨ蜉ｹ譫懊・驕ｩ逕ｨ縺ｨ謖∫ｶ壽凾髢鍋ｮ｡逅・    /// - 繧｢繝九Γ繝ｼ繧ｷ繝ｧ繝ｳ縺ｨ繧ｨ繝輔ぉ繧ｯ繝医・蛻ｶ蠕｡
    /// </summary>
    [System.Serializable]
    public class UseItemCommandDefinition : ICommandDefinition
    {
        /// <summary>
        /// 繧｢繧､繝・Β菴ｿ逕ｨ縺ｮ遞ｮ鬘槭ｒ螳夂ｾｩ縺吶ｋ蛻玲嫌蝙・        /// </summary>
        public enum UseType
        {
            Instant,        // 迸ｬ髢謎ｽｿ逕ｨ・域ｶ郁怜刀遲会ｼ・            Equip,          // 陬・ｙ・域ｭｦ蝎ｨ縲・亟蜈ｷ遲会ｼ・            Activate,       // 襍ｷ蜍包ｼ磯％蜈ｷ縲√せ繧ｭ繝ｫ繧｢繧､繝・Β遲会ｼ・            Consume,        // 豸郁ｲｻ菴ｿ逕ｨ
            Toggle          // 繧ｪ繝ｳ/繧ｪ繝募・繧頑崛縺・        }

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
        public float usageDelay = 0f; // 蜉ｹ譫憺←逕ｨ縺ｾ縺ｧ縺ｮ驕・ｻｶ

        [Header("Cooldown")]
        public float cooldownDuration = 0f;
        public bool globalCooldown = false; // 蜈ｨ繧｢繧､繝・Β縺ｮ菴ｿ逕ｨ繧貞宛髯舌☆繧九°

        [Header("Effects")]
        public bool showUseEffect = true;
        public bool showTargetingUI = false;
        public string useEffectName = "";

        /// <summary>
        /// 繝・ヵ繧ｩ繝ｫ繝医さ繝ｳ繧ｹ繝医Λ繧ｯ繧ｿ
        /// </summary>
        public UseItemCommandDefinition()
        {
        }

        /// <summary>
        /// 繝代Λ繝｡繝ｼ繧ｿ莉倥″繧ｳ繝ｳ繧ｹ繝医Λ繧ｯ繧ｿ
        /// </summary>
        public UseItemCommandDefinition(UseType type, string itemId, bool consume = true)
        {
            useType = type;
            targetItemId = itemId;
            consumeOnUse = consume;
        }

        /// <summary>
        /// 繧｢繧､繝・Β菴ｿ逕ｨ繧ｳ繝槭Φ繝峨′螳溯｡悟庄閭ｽ縺九←縺・°繧貞愛螳壹＠縺ｾ縺・        /// </summary>
        public bool CanExecute(object context = null)
        {
            // 蝓ｺ譛ｬ逧・↑螳溯｡悟庄閭ｽ諤ｧ繝√ぉ繝・け
            if (string.IsNullOrEmpty(targetItemId) && itemSlotIndex < 0) return false;
            
            if (requiresTargeting && maxTargetDistance <= 0f) return false;
            if (animationDuration < 0f || usageDelay < 0f) return false;

            // 繧ｳ繝ｳ繝・く繧ｹ繝医′縺ゅｋ蝣ｴ蜷医・霑ｽ蜉繝√ぉ繝・け
            if (context != null)
            {
                // 繧､繝ｳ繝吶Φ繝医Μ蜀・・繧｢繧､繝・Β蟄伜惠繝√ぉ繝・け
                // 繧｢繧､繝・Β菴ｿ逕ｨ蜿ｯ閭ｽ迥ｶ諷九メ繧ｧ繝・け
                // 繧ｯ繝ｼ繝ｫ繝繧ｦ繝ｳ繝√ぉ繝・け
                // 菴ｿ逕ｨ譚｡莉ｶ繝√ぉ繝・け・域姶髣倅ｸｭ縲∫ｧｻ蜍穂ｸｭ遲会ｼ・                // 繝励Ξ繧､繝､繝ｼ縺ｮ迥ｶ諷九メ繧ｧ繝・け・医せ繧ｿ繝ｳ縲∵ｲ磯ｻ咏ｭ会ｼ・            }

            return true;
        }

        /// <summary>
        /// 繧｢繧､繝・Β菴ｿ逕ｨ繧ｳ繝槭Φ繝峨ｒ菴懈・縺励∪縺・        /// </summary>
        public ICommand CreateCommand(object context = null)
        {
            if (!CanExecute(context))
                return null;

            return new UseItemCommand(this, context);
        }
    }

    /// <summary>
    /// UseItemCommandDefinition縺ｫ蟇ｾ蠢懊☆繧句ｮ滄圀縺ｮ繧ｳ繝槭Φ繝牙ｮ溯｣・    /// </summary>
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
        /// 繧｢繧､繝・Β菴ｿ逕ｨ繧ｳ繝槭Φ繝峨・螳溯｡・        /// </summary>
        public void Execute()
        {
            if (executed) return;

            // 菴ｿ逕ｨ蟇ｾ雎｡繧｢繧､繝・Β縺ｮ蜿門ｾ・            targetItem = GetTargetItem();
            if (targetItem == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.LogWarning($"Target item not found: {definition.targetItemId}");
#endif
                return;
            }

            // 繧ｿ繝ｼ繧ｲ繝・ぅ繝ｳ繧ｰ縺悟ｿ・ｦ√↑蝣ｴ蜷医・蜃ｦ逅・            if (definition.requiresTargeting)
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

            // 菴ｿ逕ｨ繧｢繝九Γ繝ｼ繧ｷ繝ｧ繝ｳ縺ｮ蜀咲函
            if (definition.playUseAnimation)
            {
                PlayUseAnimation();
            }

            // 驕・ｻｶ縺後≠繧句ｴ蜷医・驕・ｻｶ螳溯｡後√◎縺・〒縺ｪ縺代ｌ縺ｰ蜊ｳ蠎ｧ縺ｫ螳溯｡・            if (definition.usageDelay > 0f)
            {
                // 螳滄圀縺ｮ螳溯｣・〒縺ｯ Coroutine 縺ｾ縺溘・ Timer 縺ｧ驕・ｻｶ螳溯｡・                // 迴ｾ蝨ｨ縺ｯ蜊ｳ蠎ｧ縺ｫ螳溯｡・                ApplyItemEffect();
            }
            else
            {
                ApplyItemEffect();
            }

            executed = true;
        }

        /// <summary>
        /// 菴ｿ逕ｨ蟇ｾ雎｡縺ｮ繧｢繧､繝・Β繧貞叙蠕・        /// </summary>
        private IUsableItem GetTargetItem()
        {
            // 螳滄圀縺ｮ螳溯｣・〒縺ｯ InventorySystem 縺ｨ縺ｮ騾｣謳ｺ
            
            // 繧｢繧､繝・ΒID縺ｧ縺ｮ讀懃ｴ｢
            if (!string.IsNullOrEmpty(definition.targetItemId))
            {
                // return inventorySystem.GetItemById(definition.targetItemId);
            }
            
            // 繧ｹ繝ｭ繝・ヨ繧､繝ｳ繝・ャ繧ｯ繧ｹ縺ｧ縺ｮ讀懃ｴ｢
            if (definition.itemSlotIndex >= 0)
            {
                // return inventorySystem.GetItemAtSlot(definition.itemSlotIndex);
            }

            // 莉ｮ縺ｮ螳溯｣・            return new MockUsableItem(definition.targetItemId);
        }

        /// <summary>
        /// 繧ｿ繝ｼ繧ｲ繝・ヨ繧ｪ繝悶ず繧ｧ繧ｯ繝医・讀懃ｴ｢
        /// </summary>
        private GameObject FindTarget()
        {
            if (context is not MonoBehaviour mono) return null;

            // 繧ｫ繝｡繝ｩ縺ｾ縺溘・繝励Ξ繧､繝､繝ｼ縺ｮ蜑肴婿蜷代↓Raycast
            Ray ray = new Ray(mono.transform.position, mono.transform.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, definition.maxTargetDistance, definition.validTargetLayers))
            {
                return hit.collider.gameObject;
            }

            return null;
        }

        /// <summary>
        /// 菴ｿ逕ｨ繧｢繝九Γ繝ｼ繧ｷ繝ｧ繝ｳ縺ｮ蜀咲函
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
        /// 繧｢繧､繝・Β蜉ｹ譫懊・驕ｩ逕ｨ
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

            // 繧｢繧､繝・Β縺ｮ豸郁ｲｻ蜃ｦ逅・            if (definition.consumeOnUse)
            {
                ConsumeItem();
            }

            // 繧ｨ繝輔ぉ繧ｯ繝医・陦ｨ遉ｺ
            if (definition.showUseEffect)
            {
                ShowUseEffect();
            }

            // 繧ｯ繝ｼ繝ｫ繝繧ｦ繝ｳ縺ｮ髢句ｧ・            if (definition.cooldownDuration > 0f)
            {
                StartCooldown();
            }
        }

        /// <summary>
        /// 迸ｬ髢灘柑譫懊・驕ｩ逕ｨ
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
        /// 陬・ｙ蜉ｹ譫懊・驕ｩ逕ｨ
        /// </summary>
        private void ApplyEquipEffect()
        {
            if (context is MonoBehaviour mono)
            {
                // 螳滄圀縺ｮ螳溯｣・〒縺ｯ EquipmentSystem 縺ｨ縺ｮ騾｣謳ｺ
                // equipmentSystem.EquipItem(targetItem);
                
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.Log($"Equipped {targetItem.GetItemName()}");
#endif
            }
        }

        /// <summary>
        /// 襍ｷ蜍募柑譫懊・驕ｩ逕ｨ
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
        /// 豸郁ｲｻ蜉ｹ譫懊・驕ｩ逕ｨ
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
        /// 繝医げ繝ｫ蜉ｹ譫懊・驕ｩ逕ｨ
        /// </summary>
        private void ApplyToggleEffect()
        {
            // 繧｢繧､繝・Β縺ｮ迴ｾ蝨ｨ縺ｮ迥ｶ諷九ｒ蜿門ｾ励＠縺ｦ蛻・ｊ譖ｿ縺・            bool currentState = targetItem.GetToggleState();
            targetItem.SetToggleState(!currentState);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Toggled {targetItem.GetItemName()} to {!currentState}");
#endif
        }

        /// <summary>
        /// 繧｢繧､繝・Β縺ｮ豸郁ｲｻ蜃ｦ逅・        /// </summary>
        private void ConsumeItem()
        {
            // 螳滄圀縺ｮ螳溯｣・〒縺ｯ InventorySystem 縺ｨ縺ｮ騾｣謳ｺ
            // inventorySystem.ConsumeItem(targetItem);
            
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Item consumed: {targetItem.GetItemName()}");
#endif
        }

        /// <summary>
        /// 菴ｿ逕ｨ繧ｨ繝輔ぉ繧ｯ繝医・陦ｨ遉ｺ
        /// </summary>
        private void ShowUseEffect()
        {
            if (context is not MonoBehaviour mono) return;

            // 繝代・繝・ぅ繧ｯ繝ｫ繧ｨ繝輔ぉ繧ｯ繝・            if (!string.IsNullOrEmpty(definition.useEffectName))
            {
                // effectSystem.PlayEffect(definition.useEffectName, mono.transform.position);
            }

            // 繧ｵ繧ｦ繝ｳ繝峨お繝輔ぉ繧ｯ繝・            // audioSystem.PlaySound(targetItem.GetUseSound());

            // UI 繧ｨ繝輔ぉ繧ｯ繝・            // uiSystem.ShowItemUseNotification(targetItem);
        }

        /// <summary>
        /// 繧ｯ繝ｼ繝ｫ繝繧ｦ繝ｳ縺ｮ髢句ｧ・        /// </summary>
        private void StartCooldown()
        {
            // 螳滄圀縺ｮ螳溯｣・〒縺ｯ CooldownSystem 縺ｨ縺ｮ騾｣謳ｺ
            // cooldownSystem.StartCooldown(targetItem.GetItemId(), definition.cooldownDuration);
            
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Started cooldown: {definition.cooldownDuration}s");
#endif
        }

        /// <summary>
        /// 邯咏ｶ壼柑譫懊・譖ｴ譁ｰ・亥､夜Κ縺九ｉ螳壽悄逧・↓蜻ｼ縺ｳ蜃ｺ縺輔ｌ繧具ｼ・        /// </summary>
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
                // 邯咏ｶ壼柑譫懊・譖ｴ譁ｰ蜃ｦ逅・                targetItem?.UpdateEffect(context, deltaTime);
            }
        }

        /// <summary>
        /// 繧｢繧､繝・Β蜉ｹ譫懊・邨ゆｺ・        /// </summary>
        private void EndItemEffect()
        {
            isEffectActive = false;
            targetItem?.OnEffectEnd(context);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Item effect ended: {targetItem?.GetItemName()}");
#endif
        }

        /// <summary>
        /// Undo謫堺ｽ懶ｼ医い繧､繝・Β菴ｿ逕ｨ縺ｮ蜿悶ｊ豸医＠・・        /// </summary>
        public void Undo()
        {
            if (!executed) return;

            // 繧｢繧､繝・Β蜉ｹ譫懊・蜿悶ｊ豸医＠
            if (isEffectActive)
            {
                EndItemEffect();
            }

            // 菴ｿ逕ｨ縺励◆繧｢繧､繝・Β縺ｮ蠕ｩ蜈・ｼ域ｶ郁ｲｻ縺励※縺・◆蝣ｴ蜷茨ｼ・            if (definition.consumeOnUse && targetItem != null)
            {
                // 螳滄圀縺ｮ螳溯｣・〒縺ｯ InventorySystem 縺ｨ縺ｮ騾｣謳ｺ
                // inventorySystem.RestoreItem(targetItem);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.Log($"Restored consumed item: {targetItem.GetItemName()}");
#endif
            }

            // 陬・ｙ繧｢繧､繝・Β縺ｮ蜿悶ｊ螟悶＠
            if (definition.useType == UseItemCommandDefinition.UseType.Equip)
            {
                // equipmentSystem.UnequipItem(targetItem);
            }

            executed = false;
        }

        /// <summary>
        /// 縺薙・繧ｳ繝槭Φ繝峨′Undo蜿ｯ閭ｽ縺九←縺・°
        /// </summary>
        public bool CanUndo => executed && (definition.consumeOnUse || definition.useType == UseItemCommandDefinition.UseType.Equip);

        /// <summary>
        /// 繧｢繧､繝・Β蜉ｹ譫懊′迴ｾ蝨ｨ繧｢繧ｯ繝・ぅ繝悶°縺ｩ縺・°
        /// </summary>
        public bool IsEffectActive => isEffectActive;
    }

    /// <summary>
    /// 菴ｿ逕ｨ蜿ｯ閭ｽ繧｢繧､繝・Β縺ｮ繧､繝ｳ繧ｿ繝ｼ繝輔ぉ繝ｼ繧ｹ
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
    /// 繝・せ繝育畑縺ｮ繝｢繝・け繧｢繧､繝・Β螳溯｣・    /// </summary>
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