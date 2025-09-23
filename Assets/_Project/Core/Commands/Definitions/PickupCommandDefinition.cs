using UnityEngine;
// using asterivo.Unity60.Core.Commands;

namespace asterivo.Unity60.Core.Commands.Definitions
{
    /// <summary>
    /// アイチE��ピックアチE�Eコマンド�E定義、E    /// プレイヤーのアイチE��収集アクションをカプセル化します、E    /// 
    /// 主な機�E�E�E    /// - アイチE��の自勁E手動ピックアチE�E
    /// - インベントリ容量とアイチE��制紁E�E管琁E    /// - ピックアチE�E篁E��とフィルタリング
    /// - アイチE��惁E��の表示とフィードバチE��
    /// </summary>
    [System.Serializable]
    public class PickupCommandDefinition : ICommandDefinition
    {
        /// <summary>
        /// ピックアチE�Eの種類を定義する列挙垁E        /// </summary>
        public enum PickupType
        {
            Manual,         // 手動ピックアチE�E
            Auto,           // 自動ピチE��アチE�E
            Selective,      // 選択的ピックアチE�E
            Area,           // 篁E��ピックアチE�E
            Magnetic        // 磁力ピックアチE�E
        }

        [Header("Pickup Parameters")]
        public PickupType pickupType = PickupType.Manual;
        public float pickupRange = 1.5f;
        public LayerMask itemLayer = -1;
        public string itemTag = "Item";

        [Header("Item Filtering")]
        [Tooltip("ピックアチE�E対象のアイチE��タイチE)]
        public string[] allowedItemTypes = { "Consumable", "Weapon", "Armor", "Key" };
        [Tooltip("除外するアイチE��タイチE)]
        public string[] excludedItemTypes = { };
        public bool respectInventoryCapacity = true;

        [Header("Area Pickup")]
        [Tooltip("篁E��ピックアチE�E時�E効果篁E��")]
        public float areaRadius = 3f;
        [Tooltip("一度にピックアチE�Eする最大個数")]
        public int maxPickupCount = 10;

        [Header("Magnetic Pickup")]
        [Tooltip("アイチE��を引き寁E��る力")]
        public float magneticForce = 5f;
        [Tooltip("磁力の効果時閁E)]
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
        /// チE��ォルトコンストラクタ
        /// </summary>
        public PickupCommandDefinition()
        {
        }

        /// <summary>
        /// パラメータ付きコンストラクタ
        /// </summary>
        public PickupCommandDefinition(PickupType type, float range, bool autoPickup = false)
        {
            pickupType = type;
            pickupRange = range;
        }

        /// <summary>
        /// ピックアチE�Eコマンドが実行可能かどぁE��を判定しまぁE        /// </summary>
        public bool CanExecute(object context = null)
        {
            // 基本皁E��実行可能性チェチE��
            if (pickupRange <= 0f) return false;
            
            if (pickupType == PickupType.Area && areaRadius <= 0f) return false;
            if (pickupType == PickupType.Magnetic && (magneticForce <= 0f || magneticDuration <= 0f)) return false;

            // コンチE��ストがある場合�E追加チェチE��
            if (context != null)
            {
                // インベントリの容量チェチE��
                // プレイヤーの状態チェチE���E�移動中、戦闘中等�E制紁E��E                // 篁E��冁E��ピックアチE�E可能アイチE��の存在チェチE��
            }

            return true;
        }

        /// <summary>
        /// ピックアチE�Eコマンドを作�EしまぁE        /// </summary>
        public ICommand CreateCommand(object context = null)
        {
            if (!CanExecute(context))
                return null;

            return new PickupCommand(this, context);
        }
    }

    /// <summary>
    /// PickupCommandDefinitionに対応する実際のコマンド実裁E    /// </summary>
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
        /// ピックアチE�Eコマンド�E実衁E        /// </summary>
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
        /// 手動ピックアチE�Eの実衁E        /// </summary>
        private void ExecuteManualPickup()
        {
            var targetItem = FindNearestPickupableItem();
            if (targetItem != null)
            {
                PickupItem(targetItem);
            }
        }

        /// <summary>
        /// 自動ピチE��アチE�Eの実衁E        /// </summary>
        private void ExecuteAutoPickup()
        {
            var items = FindAllPickupableItems(definition.pickupRange);
            foreach (var item in items)
            {
                if (!CanPickupItem(item)) continue;
                
                PickupItem(item);
                
                // インベントリが満杯になった場合�E停止
                if (definition.respectInventoryCapacity && IsInventoryFull())
                    break;
            }
        }

        /// <summary>
        /// 選択的ピックアチE�Eの実衁E        /// </summary>
        private void ExecuteSelectivePickup()
        {
            var items = FindAllPickupableItems(definition.pickupRange);
            
            // 優先度の高いアイチE��から頁E��取征E            var prioritizedItems = SortItemsByPriority(items);
            
            foreach (var item in prioritizedItems)
            {
                if (!CanPickupItem(item)) continue;
                
                PickupItem(item);
                
                if (definition.respectInventoryCapacity && IsInventoryFull())
                    break;
            }
        }

        /// <summary>
        /// 篁E��ピックアチE�Eの実衁E        /// </summary>
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
        /// 磁力ピックアチE�Eの実衁E        /// </summary>
        private void ExecuteMagneticPickup()
        {
            var items = FindAllPickupableItems(definition.areaRadius);
            isMagneticActive = true;

            // アイチE��を引き寁E��る�E琁E��開姁E            foreach (var item in items)
            {
                if (!CanPickupItem(item)) continue;
                
                StartItemAttraction(item);
            }

            // 磁力効果�E継続�E琁E��実際の実裁E��は Coroutine また�EUpdateLoop�E�E#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Magnetic pickup started: attracting {items.Count} items");
#endif
        }

        /// <summary>
        /// 最も近いピックアチE�E可能アイチE��を検索
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
        /// すべてのピックアチE�E可能アイチE��を検索
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
        /// アイチE��が有効なピックアチE�E対象かチェチE��
        /// </summary>
        private bool IsValidPickupTarget(GameObject item)
        {
            // タグチェチE��
            if (!string.IsNullOrEmpty(definition.itemTag) && !item.CompareTag(definition.itemTag))
                return false;

            // アイチE��コンポ�Eネント�E存在チェチE��
            var itemComponent = item.GetComponent<IPickupableItem>();
            if (itemComponent == null) return false;

            // アイチE��タイプフィルタリング
            string itemType = itemComponent.GetItemType();
            
            // 除外リストチェチE��
            if (definition.excludedItemTypes.Length > 0)
            {
                foreach (var excludedType in definition.excludedItemTypes)
                {
                    if (itemType == excludedType) return false;
                }
            }

            // 許可リストチェチE��
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
        /// アイチE��をピチE��アチE�E可能かチェチE��
        /// </summary>
        private bool CanPickupItem(GameObject item)
        {
            if (definition.respectInventoryCapacity && IsInventoryFull())
                return false;

            var itemComponent = item.GetComponent<IPickupableItem>();
            return itemComponent?.CanBePickedUp() ?? false;
        }

        /// <summary>
        /// インベントリが満杯かチェチE��
        /// </summary>
        private bool IsInventoryFull()
        {
            // 実際の実裁E��は InventorySystem との連携
            return false; // 仮の値
        }

        /// <summary>
        /// アイチE��を優先度頁E��ソーチE        /// </summary>
        private System.Collections.Generic.List<GameObject> SortItemsByPriority(System.Collections.Generic.List<GameObject> items)
        {
            // 実際の実裁E��は、アイチE��の価値、レアリチE��、忁E��性等で優先度を決宁E            return items; // 仮の実裁E        }

        /// <summary>
        /// 実際のアイチE��ピックアチE�E処琁E        /// </summary>
        private void PickupItem(GameObject item)
        {
            var itemComponent = item.GetComponent<IPickupableItem>();
            if (itemComponent == null) return;

            // アイチE��惁E��の取征E            var itemData = itemComponent.GetItemData();

            // インベントリに追加
            // 実際の実裁E��は InventorySystem との連携
            
            // アイチE��をワールドから削除
            pickedUpItems.Add(item); // Undo用に保孁E            item.SetActive(false);

            // エフェクトとフィードバチE��
            PlayPickupEffects(item, itemData);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Picked up item: {itemData.name}");
#endif
        }

        /// <summary>
        /// アイチE��の磁力による引き寁E��開姁E        /// </summary>
        private void StartItemAttraction(GameObject item)
        {
            // 実際の実裁E��は、物琁E��な力また�ETweenアニメーションでアイチE��を引き寁E��めE            // 引き寁E��完亁E��にPickupItem()を呼び出ぁE        }

        /// <summary>
        /// ピックアチE�Eエフェクト�E再生
        /// </summary>
        private void PlayPickupEffects(GameObject item, IItemData itemData)
        {
            if (context is not MonoBehaviour mono) return;

            // アニメーション
            if (definition.playPickupAnimation)
            {
                var animator = mono.GetComponent<Animator>();
                if (animator != null && !string.IsNullOrEmpty(definition.pickupAnimationTrigger))
                {
                    animator.SetTrigger(definition.pickupAnimationTrigger);
                }
            }

            // パ�EチE��クルエフェクチE            if (definition.showPickupEffect)
            {
                // エフェクト生戁E            }

            // サウンドエフェクチE            if (definition.playPickupSound)
            {
                // AudioSystem との連携
            }

            // アイチE��惁E��表示
            if (definition.showItemInfo)
            {
                // UI表示�E�アイチE��名、説明等！E            }
        }

        /// <summary>
        /// 磁力ピックアチE�Eの更新�E�外部から定期皁E��呼び出される！E        /// </summary>
        public void UpdateMagneticPickup(float deltaTime)
        {
            if (!isMagneticActive) return;

            // 引き寁E��処琁E�E更新
            // 持続時間�E管琁E        }

        /// <summary>
        /// Undo操作（ピチE��アチE�Eの取り消し�E�E        /// </summary>
        public void Undo()
        {
            if (!executed) return;

            // ピックアチE�EしたアイチE��を�Eの位置に戻ぁE            foreach (var item in pickedUpItems)
            {
                if (item != null)
                {
                    item.SetActive(true);
                    // インベントリから削除
                    // 実際の実裁E��は InventorySystem との連携
                }
            }

            pickedUpItems.Clear();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log("Pickup undone - items restored");
#endif

            executed = false;
        }

        /// <summary>
        /// こ�EコマンドがUndo可能かどぁE��
        /// </summary>
        public bool CanUndo => executed && pickedUpItems.Count > 0;

        /// <summary>
        /// 磁力効果が現在アクチE��ブかどぁE��
        /// </summary>
        public bool IsMagneticActive => isMagneticActive;
    }

    /// <summary>
    /// ピックアチE�E可能アイチE��のインターフェース
    /// </summary>
    public interface IPickupableItem
    {
        string GetItemType();
        IItemData GetItemData();
        bool CanBePickedUp();
        void OnPickedUp(object picker);
    }

    /// <summary>
    /// アイチE��チE�Eタのインターフェース
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