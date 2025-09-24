using UnityEngine;
// using asterivo.Unity60.Core.Commands;

namespace asterivo.Unity60.Core.Commands.Definitions
{
    /// <summary>
    /// 繧｢繧､繝・Β繝斐ャ繧ｯ繧｢繝・・繧ｳ繝槭Φ繝峨・螳夂ｾｩ縲・    /// 繝励Ξ繧､繝､繝ｼ縺ｮ繧｢繧､繝・Β蜿朱寔繧｢繧ｯ繧ｷ繝ｧ繝ｳ繧偵き繝励そ繝ｫ蛹悶＠縺ｾ縺吶・    /// 
    /// 荳ｻ縺ｪ讖溯・・・    /// - 繧｢繧､繝・Β縺ｮ閾ｪ蜍・謇句虚繝斐ャ繧ｯ繧｢繝・・
    /// - 繧､繝ｳ繝吶Φ繝医Μ螳ｹ驥上→繧｢繧､繝・Β蛻ｶ邏・・邂｡逅・    /// - 繝斐ャ繧ｯ繧｢繝・・遽・峇縺ｨ繝輔ぅ繝ｫ繧ｿ繝ｪ繝ｳ繧ｰ
    /// - 繧｢繧､繝・Β諠・ｱ縺ｮ陦ｨ遉ｺ縺ｨ繝輔ぅ繝ｼ繝峨ヰ繝・け
    /// </summary>
    [System.Serializable]
    public class PickupCommandDefinition : ICommandDefinition
    {
        /// <summary>
        /// 繝斐ャ繧ｯ繧｢繝・・縺ｮ遞ｮ鬘槭ｒ螳夂ｾｩ縺吶ｋ蛻玲嫌蝙・        /// </summary>
        public enum PickupType
        {
            Manual,         // 謇句虚繝斐ャ繧ｯ繧｢繝・・
            Auto,           // 閾ｪ蜍輔ヴ繝・け繧｢繝・・
            Selective,      // 驕ｸ謚樒噪繝斐ャ繧ｯ繧｢繝・・
            Area,           // 遽・峇繝斐ャ繧ｯ繧｢繝・・
            Magnetic        // 逎∝鴨繝斐ャ繧ｯ繧｢繝・・
        }

        [Header("Pickup Parameters")]
        public PickupType pickupType = PickupType.Manual;
        public float pickupRange = 1.5f;
        public LayerMask itemLayer = -1;
        public string itemTag = "Item";

        [Header("Item Filtering")]
        [Tooltip("繝斐ャ繧ｯ繧｢繝・・蟇ｾ雎｡縺ｮ繧｢繧､繝・Β繧ｿ繧､繝・)]
        public string[] allowedItemTypes = { "Consumable", "Weapon", "Armor", "Key" };
        [Tooltip("髯､螟悶☆繧九い繧､繝・Β繧ｿ繧､繝・)]
        public string[] excludedItemTypes = { };
        public bool respectInventoryCapacity = true;

        [Header("Area Pickup")]
        [Tooltip("遽・峇繝斐ャ繧ｯ繧｢繝・・譎ゅ・蜉ｹ譫懃ｯ・峇")]
        public float areaRadius = 3f;
        [Tooltip("荳蠎ｦ縺ｫ繝斐ャ繧ｯ繧｢繝・・縺吶ｋ譛螟ｧ蛟区焚")]
        public int maxPickupCount = 10;

        [Header("Magnetic Pickup")]
        [Tooltip("繧｢繧､繝・Β繧貞ｼ輔″蟇・○繧句鴨")]
        public float magneticForce = 5f;
        [Tooltip("逎∝鴨縺ｮ蜉ｹ譫懈凾髢・)]
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
        /// 繝・ヵ繧ｩ繝ｫ繝医さ繝ｳ繧ｹ繝医Λ繧ｯ繧ｿ
        /// </summary>
        public PickupCommandDefinition()
        {
        }

        /// <summary>
        /// 繝代Λ繝｡繝ｼ繧ｿ莉倥″繧ｳ繝ｳ繧ｹ繝医Λ繧ｯ繧ｿ
        /// </summary>
        public PickupCommandDefinition(PickupType type, float range, bool autoPickup = false)
        {
            pickupType = type;
            pickupRange = range;
        }

        /// <summary>
        /// 繝斐ャ繧ｯ繧｢繝・・繧ｳ繝槭Φ繝峨′螳溯｡悟庄閭ｽ縺九←縺・°繧貞愛螳壹＠縺ｾ縺・        /// </summary>
        public bool CanExecute(object context = null)
        {
            // 蝓ｺ譛ｬ逧・↑螳溯｡悟庄閭ｽ諤ｧ繝√ぉ繝・け
            if (pickupRange <= 0f) return false;
            
            if (pickupType == PickupType.Area && areaRadius <= 0f) return false;
            if (pickupType == PickupType.Magnetic && (magneticForce <= 0f || magneticDuration <= 0f)) return false;

            // 繧ｳ繝ｳ繝・く繧ｹ繝医′縺ゅｋ蝣ｴ蜷医・霑ｽ蜉繝√ぉ繝・け
            if (context != null)
            {
                // 繧､繝ｳ繝吶Φ繝医Μ縺ｮ螳ｹ驥上メ繧ｧ繝・け
                // 繝励Ξ繧､繝､繝ｼ縺ｮ迥ｶ諷九メ繧ｧ繝・け・育ｧｻ蜍穂ｸｭ縲∵姶髣倅ｸｭ遲峨・蛻ｶ邏・ｼ・                // 遽・峇蜀・↓繝斐ャ繧ｯ繧｢繝・・蜿ｯ閭ｽ繧｢繧､繝・Β縺ｮ蟄伜惠繝√ぉ繝・け
            }

            return true;
        }

        /// <summary>
        /// 繝斐ャ繧ｯ繧｢繝・・繧ｳ繝槭Φ繝峨ｒ菴懈・縺励∪縺・        /// </summary>
        public ICommand CreateCommand(object context = null)
        {
            if (!CanExecute(context))
                return null;

            return new PickupCommand(this, context);
        }
    }

    /// <summary>
    /// PickupCommandDefinition縺ｫ蟇ｾ蠢懊☆繧句ｮ滄圀縺ｮ繧ｳ繝槭Φ繝牙ｮ溯｣・    /// </summary>
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
        /// 繝斐ャ繧ｯ繧｢繝・・繧ｳ繝槭Φ繝峨・螳溯｡・        /// </summary>
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
        /// 謇句虚繝斐ャ繧ｯ繧｢繝・・縺ｮ螳溯｡・        /// </summary>
        private void ExecuteManualPickup()
        {
            var targetItem = FindNearestPickupableItem();
            if (targetItem != null)
            {
                PickupItem(targetItem);
            }
        }

        /// <summary>
        /// 閾ｪ蜍輔ヴ繝・け繧｢繝・・縺ｮ螳溯｡・        /// </summary>
        private void ExecuteAutoPickup()
        {
            var items = FindAllPickupableItems(definition.pickupRange);
            foreach (var item in items)
            {
                if (!CanPickupItem(item)) continue;
                
                PickupItem(item);
                
                // 繧､繝ｳ繝吶Φ繝医Μ縺梧ｺ譚ｯ縺ｫ縺ｪ縺｣縺溷ｴ蜷医・蛛懈ｭ｢
                if (definition.respectInventoryCapacity && IsInventoryFull())
                    break;
            }
        }

        /// <summary>
        /// 驕ｸ謚樒噪繝斐ャ繧ｯ繧｢繝・・縺ｮ螳溯｡・        /// </summary>
        private void ExecuteSelectivePickup()
        {
            var items = FindAllPickupableItems(definition.pickupRange);
            
            // 蜆ｪ蜈亥ｺｦ縺ｮ鬮倥＞繧｢繧､繝・Β縺九ｉ鬆・↓蜿門ｾ・            var prioritizedItems = SortItemsByPriority(items);
            
            foreach (var item in prioritizedItems)
            {
                if (!CanPickupItem(item)) continue;
                
                PickupItem(item);
                
                if (definition.respectInventoryCapacity && IsInventoryFull())
                    break;
            }
        }

        /// <summary>
        /// 遽・峇繝斐ャ繧ｯ繧｢繝・・縺ｮ螳溯｡・        /// </summary>
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
        /// 逎∝鴨繝斐ャ繧ｯ繧｢繝・・縺ｮ螳溯｡・        /// </summary>
        private void ExecuteMagneticPickup()
        {
            var items = FindAllPickupableItems(definition.areaRadius);
            isMagneticActive = true;

            // 繧｢繧､繝・Β繧貞ｼ輔″蟇・○繧句・逅・ｒ髢句ｧ・            foreach (var item in items)
            {
                if (!CanPickupItem(item)) continue;
                
                StartItemAttraction(item);
            }

            // 逎∝鴨蜉ｹ譫懊・邯咏ｶ壼・逅・ｼ亥ｮ滄圀縺ｮ螳溯｣・〒縺ｯ Coroutine 縺ｾ縺溘・UpdateLoop・・#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Magnetic pickup started: attracting {items.Count} items");
#endif
        }

        /// <summary>
        /// 譛繧りｿ代＞繝斐ャ繧ｯ繧｢繝・・蜿ｯ閭ｽ繧｢繧､繝・Β繧呈､懃ｴ｢
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
        /// 縺吶∋縺ｦ縺ｮ繝斐ャ繧ｯ繧｢繝・・蜿ｯ閭ｽ繧｢繧､繝・Β繧呈､懃ｴ｢
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
        /// 繧｢繧､繝・Β縺梧怏蜉ｹ縺ｪ繝斐ャ繧ｯ繧｢繝・・蟇ｾ雎｡縺九メ繧ｧ繝・け
        /// </summary>
        private bool IsValidPickupTarget(GameObject item)
        {
            // 繧ｿ繧ｰ繝√ぉ繝・け
            if (!string.IsNullOrEmpty(definition.itemTag) && !item.CompareTag(definition.itemTag))
                return false;

            // 繧｢繧､繝・Β繧ｳ繝ｳ繝昴・繝阪Φ繝医・蟄伜惠繝√ぉ繝・け
            var itemComponent = item.GetComponent<IPickupableItem>();
            if (itemComponent == null) return false;

            // 繧｢繧､繝・Β繧ｿ繧､繝励ヵ繧｣繝ｫ繧ｿ繝ｪ繝ｳ繧ｰ
            string itemType = itemComponent.GetItemType();
            
            // 髯､螟悶Μ繧ｹ繝医メ繧ｧ繝・け
            if (definition.excludedItemTypes.Length > 0)
            {
                foreach (var excludedType in definition.excludedItemTypes)
                {
                    if (itemType == excludedType) return false;
                }
            }

            // 險ｱ蜿ｯ繝ｪ繧ｹ繝医メ繧ｧ繝・け
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
        /// 繧｢繧､繝・Β繧偵ヴ繝・け繧｢繝・・蜿ｯ閭ｽ縺九メ繧ｧ繝・け
        /// </summary>
        private bool CanPickupItem(GameObject item)
        {
            if (definition.respectInventoryCapacity && IsInventoryFull())
                return false;

            var itemComponent = item.GetComponent<IPickupableItem>();
            return itemComponent?.CanBePickedUp() ?? false;
        }

        /// <summary>
        /// 繧､繝ｳ繝吶Φ繝医Μ縺梧ｺ譚ｯ縺九メ繧ｧ繝・け
        /// </summary>
        private bool IsInventoryFull()
        {
            // 螳滄圀縺ｮ螳溯｣・〒縺ｯ InventorySystem 縺ｨ縺ｮ騾｣謳ｺ
            return false; // 莉ｮ縺ｮ蛟､
        }

        /// <summary>
        /// 繧｢繧､繝・Β繧貞━蜈亥ｺｦ鬆・↓繧ｽ繝ｼ繝・        /// </summary>
        private System.Collections.Generic.List<GameObject> SortItemsByPriority(System.Collections.Generic.List<GameObject> items)
        {
            // 螳滄圀縺ｮ螳溯｣・〒縺ｯ縲√い繧､繝・Β縺ｮ萓｡蛟､縲√Ξ繧｢繝ｪ繝・ぅ縲∝ｿ・ｦ∵ｧ遲峨〒蜆ｪ蜈亥ｺｦ繧呈ｱｺ螳・            return items; // 莉ｮ縺ｮ螳溯｣・        }

        /// <summary>
        /// 螳滄圀縺ｮ繧｢繧､繝・Β繝斐ャ繧ｯ繧｢繝・・蜃ｦ逅・        /// </summary>
        private void PickupItem(GameObject item)
        {
            var itemComponent = item.GetComponent<IPickupableItem>();
            if (itemComponent == null) return;

            // 繧｢繧､繝・Β諠・ｱ縺ｮ蜿門ｾ・            var itemData = itemComponent.GetItemData();

            // 繧､繝ｳ繝吶Φ繝医Μ縺ｫ霑ｽ蜉
            // 螳滄圀縺ｮ螳溯｣・〒縺ｯ InventorySystem 縺ｨ縺ｮ騾｣謳ｺ
            
            // 繧｢繧､繝・Β繧偵Ρ繝ｼ繝ｫ繝峨°繧牙炎髯､
            pickedUpItems.Add(item); // Undo逕ｨ縺ｫ菫晏ｭ・            item.SetActive(false);

            // 繧ｨ繝輔ぉ繧ｯ繝医→繝輔ぅ繝ｼ繝峨ヰ繝・け
            PlayPickupEffects(item, itemData);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Picked up item: {itemData.name}");
#endif
        }

        /// <summary>
        /// 繧｢繧､繝・Β縺ｮ逎∝鴨縺ｫ繧医ｋ蠑輔″蟇・○髢句ｧ・        /// </summary>
        private void StartItemAttraction(GameObject item)
        {
            // 螳滄圀縺ｮ螳溯｣・〒縺ｯ縲∫黄逅・噪縺ｪ蜉帙∪縺溘・Tween繧｢繝九Γ繝ｼ繧ｷ繝ｧ繝ｳ縺ｧ繧｢繧､繝・Β繧貞ｼ輔″蟇・○繧・            // 蠑輔″蟇・○螳御ｺ・凾縺ｫPickupItem()繧貞他縺ｳ蜃ｺ縺・        }

        /// <summary>
        /// 繝斐ャ繧ｯ繧｢繝・・繧ｨ繝輔ぉ繧ｯ繝医・蜀咲函
        /// </summary>
        private void PlayPickupEffects(GameObject item, IItemData itemData)
        {
            if (context is not MonoBehaviour mono) return;

            // 繧｢繝九Γ繝ｼ繧ｷ繝ｧ繝ｳ
            if (definition.playPickupAnimation)
            {
                var animator = mono.GetComponent<Animator>();
                if (animator != null && !string.IsNullOrEmpty(definition.pickupAnimationTrigger))
                {
                    animator.SetTrigger(definition.pickupAnimationTrigger);
                }
            }

            // 繝代・繝・ぅ繧ｯ繝ｫ繧ｨ繝輔ぉ繧ｯ繝・            if (definition.showPickupEffect)
            {
                // 繧ｨ繝輔ぉ繧ｯ繝育函謌・            }

            // 繧ｵ繧ｦ繝ｳ繝峨お繝輔ぉ繧ｯ繝・            if (definition.playPickupSound)
            {
                // AudioSystem 縺ｨ縺ｮ騾｣謳ｺ
            }

            // 繧｢繧､繝・Β諠・ｱ陦ｨ遉ｺ
            if (definition.showItemInfo)
            {
                // UI陦ｨ遉ｺ・医い繧､繝・Β蜷阪∬ｪｬ譏守ｭ会ｼ・            }
        }

        /// <summary>
        /// 逎∝鴨繝斐ャ繧ｯ繧｢繝・・縺ｮ譖ｴ譁ｰ・亥､夜Κ縺九ｉ螳壽悄逧・↓蜻ｼ縺ｳ蜃ｺ縺輔ｌ繧具ｼ・        /// </summary>
        public void UpdateMagneticPickup(float deltaTime)
        {
            if (!isMagneticActive) return;

            // 蠑輔″蟇・○蜃ｦ逅・・譖ｴ譁ｰ
            // 謖∫ｶ壽凾髢薙・邂｡逅・        }

        /// <summary>
        /// Undo謫堺ｽ懶ｼ医ヴ繝・け繧｢繝・・縺ｮ蜿悶ｊ豸医＠・・        /// </summary>
        public void Undo()
        {
            if (!executed) return;

            // 繝斐ャ繧ｯ繧｢繝・・縺励◆繧｢繧､繝・Β繧貞・縺ｮ菴咲ｽｮ縺ｫ謌ｻ縺・            foreach (var item in pickedUpItems)
            {
                if (item != null)
                {
                    item.SetActive(true);
                    // 繧､繝ｳ繝吶Φ繝医Μ縺九ｉ蜑企勁
                    // 螳滄圀縺ｮ螳溯｣・〒縺ｯ InventorySystem 縺ｨ縺ｮ騾｣謳ｺ
                }
            }

            pickedUpItems.Clear();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log("Pickup undone - items restored");
#endif

            executed = false;
        }

        /// <summary>
        /// 縺薙・繧ｳ繝槭Φ繝峨′Undo蜿ｯ閭ｽ縺九←縺・°
        /// </summary>
        public bool CanUndo => executed && pickedUpItems.Count > 0;

        /// <summary>
        /// 逎∝鴨蜉ｹ譫懊′迴ｾ蝨ｨ繧｢繧ｯ繝・ぅ繝悶°縺ｩ縺・°
        /// </summary>
        public bool IsMagneticActive => isMagneticActive;
    }

    /// <summary>
    /// 繝斐ャ繧ｯ繧｢繝・・蜿ｯ閭ｽ繧｢繧､繝・Β縺ｮ繧､繝ｳ繧ｿ繝ｼ繝輔ぉ繝ｼ繧ｹ
    /// </summary>
    public interface IPickupableItem
    {
        string GetItemType();
        IItemData GetItemData();
        bool CanBePickedUp();
        void OnPickedUp(object picker);
    }

    /// <summary>
    /// 繧｢繧､繝・Β繝・・繧ｿ縺ｮ繧､繝ｳ繧ｿ繝ｼ繝輔ぉ繝ｼ繧ｹ
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