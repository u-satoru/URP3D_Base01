using UnityEngine;
using asterivo.Unity60.Core.Commands;

namespace asterivo.Unity60.Core.Commands.Definitions
{
    /// <summary>
    /// アイテムピックアップコマンドの定義。
    /// プレイヤーのアイテム収集アクションをカプセル化します。
    /// 
    /// 主な機能：
    /// - アイテムの自動/手動ピックアップ
    /// - インベントリ容量とアイテム制約の管理
    /// - ピックアップ範囲とフィルタリング
    /// - アイテム情報の表示とフィードバック
    /// </summary>
    [System.Serializable]
    public class PickupCommandDefinition : ICommandDefinition
    {
        /// <summary>
        /// ピックアップの種類を定義する列挙型
        /// </summary>
        public enum PickupType
        {
            Manual,         // 手動ピックアップ
            Auto,           // 自動ピックアップ
            Selective,      // 選択的ピックアップ
            Area,           // 範囲ピックアップ
            Magnetic        // 磁力ピックアップ
        }

        [Header("Pickup Parameters")]
        public PickupType pickupType = PickupType.Manual;
        public float pickupRange = 1.5f;
        public LayerMask itemLayer = -1;
        public string itemTag = "Item";

        [Header("Item Filtering")]
        [Tooltip("ピックアップ対象のアイテムタイプ")]
        public string[] allowedItemTypes = { "Consumable", "Weapon", "Armor", "Key" };
        [Tooltip("除外するアイテムタイプ")]
        public string[] excludedItemTypes = { };
        public bool respectInventoryCapacity = true;

        [Header("Area Pickup")]
        [Tooltip("範囲ピックアップ時の効果範囲")]
        public float areaRadius = 3f;
        [Tooltip("一度にピックアップする最大個数")]
        public int maxPickupCount = 10;

        [Header("Magnetic Pickup")]
        [Tooltip("アイテムを引き寄せる力")]
        public float magneticForce = 5f;
        [Tooltip("磁力の効果時間")]
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
        /// デフォルトコンストラクタ
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
        /// ピックアップコマンドが実行可能かどうかを判定します
        /// </summary>
        public bool CanExecute(object context = null)
        {
            // 基本的な実行可能性チェック
            if (pickupRange <= 0f) return false;
            
            if (pickupType == PickupType.Area && areaRadius <= 0f) return false;
            if (pickupType == PickupType.Magnetic && (magneticForce <= 0f || magneticDuration <= 0f)) return false;

            // コンテキストがある場合の追加チェック
            if (context != null)
            {
                // インベントリの容量チェック
                // プレイヤーの状態チェック（移動中、戦闘中等の制約）
                // 範囲内にピックアップ可能アイテムの存在チェック
            }

            return true;
        }

        /// <summary>
        /// ピックアップコマンドを作成します
        /// </summary>
        public ICommand CreateCommand(object context = null)
        {
            if (!CanExecute(context))
                return null;

            return new PickupCommand(this, context);
        }
    }

    /// <summary>
    /// PickupCommandDefinitionに対応する実際のコマンド実装
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
        /// ピックアップコマンドの実行
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
        /// 手動ピックアップの実行
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
        /// 自動ピックアップの実行
        /// </summary>
        private void ExecuteAutoPickup()
        {
            var items = FindAllPickupableItems(definition.pickupRange);
            foreach (var item in items)
            {
                if (!CanPickupItem(item)) continue;
                
                PickupItem(item);
                
                // インベントリが満杯になった場合は停止
                if (definition.respectInventoryCapacity && IsInventoryFull())
                    break;
            }
        }

        /// <summary>
        /// 選択的ピックアップの実行
        /// </summary>
        private void ExecuteSelectivePickup()
        {
            var items = FindAllPickupableItems(definition.pickupRange);
            
            // 優先度の高いアイテムから順に取得
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
        /// 範囲ピックアップの実行
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
        /// 磁力ピックアップの実行
        /// </summary>
        private void ExecuteMagneticPickup()
        {
            var items = FindAllPickupableItems(definition.areaRadius);
            isMagneticActive = true;

            // アイテムを引き寄せる処理を開始
            foreach (var item in items)
            {
                if (!CanPickupItem(item)) continue;
                
                StartItemAttraction(item);
            }

            // 磁力効果の継続処理（実際の実装では Coroutine またはUpdateLoop）
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Magnetic pickup started: attracting {items.Count} items");
#endif
        }

        /// <summary>
        /// 最も近いピックアップ可能アイテムを検索
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
        /// すべてのピックアップ可能アイテムを検索
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
        /// アイテムが有効なピックアップ対象かチェック
        /// </summary>
        private bool IsValidPickupTarget(GameObject item)
        {
            // タグチェック
            if (!string.IsNullOrEmpty(definition.itemTag) && !item.CompareTag(definition.itemTag))
                return false;

            // アイテムコンポーネントの存在チェック
            var itemComponent = item.GetComponent<IPickupableItem>();
            if (itemComponent == null) return false;

            // アイテムタイプフィルタリング
            string itemType = itemComponent.GetItemType();
            
            // 除外リストチェック
            if (definition.excludedItemTypes.Length > 0)
            {
                foreach (var excludedType in definition.excludedItemTypes)
                {
                    if (itemType == excludedType) return false;
                }
            }

            // 許可リストチェック
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
        /// アイテムをピックアップ可能かチェック
        /// </summary>
        private bool CanPickupItem(GameObject item)
        {
            if (definition.respectInventoryCapacity && IsInventoryFull())
                return false;

            var itemComponent = item.GetComponent<IPickupableItem>();
            return itemComponent?.CanBePickedUp() ?? false;
        }

        /// <summary>
        /// インベントリが満杯かチェック
        /// </summary>
        private bool IsInventoryFull()
        {
            // 実際の実装では InventorySystem との連携
            return false; // 仮の値
        }

        /// <summary>
        /// アイテムを優先度順にソート
        /// </summary>
        private System.Collections.Generic.List<GameObject> SortItemsByPriority(System.Collections.Generic.List<GameObject> items)
        {
            // 実際の実装では、アイテムの価値、レアリティ、必要性等で優先度を決定
            return items; // 仮の実装
        }

        /// <summary>
        /// 実際のアイテムピックアップ処理
        /// </summary>
        private void PickupItem(GameObject item)
        {
            var itemComponent = item.GetComponent<IPickupableItem>();
            if (itemComponent == null) return;

            // アイテム情報の取得
            var itemData = itemComponent.GetItemData();

            // インベントリに追加
            // 実際の実装では InventorySystem との連携
            
            // アイテムをワールドから削除
            pickedUpItems.Add(item); // Undo用に保存
            item.SetActive(false);

            // エフェクトとフィードバック
            PlayPickupEffects(item, itemData);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Picked up item: {itemData.name}");
#endif
        }

        /// <summary>
        /// アイテムの磁力による引き寄せ開始
        /// </summary>
        private void StartItemAttraction(GameObject item)
        {
            // 実際の実装では、物理的な力またはTweenアニメーションでアイテムを引き寄せる
            // 引き寄せ完了時にPickupItem()を呼び出す
        }

        /// <summary>
        /// ピックアップエフェクトの再生
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

            // パーティクルエフェクト
            if (definition.showPickupEffect)
            {
                // エフェクト生成
            }

            // サウンドエフェクト
            if (definition.playPickupSound)
            {
                // AudioSystem との連携
            }

            // アイテム情報表示
            if (definition.showItemInfo)
            {
                // UI表示（アイテム名、説明等）
            }
        }

        /// <summary>
        /// 磁力ピックアップの更新（外部から定期的に呼び出される）
        /// </summary>
        public void UpdateMagneticPickup(float deltaTime)
        {
            if (!isMagneticActive) return;

            // 引き寄せ処理の更新
            // 持続時間の管理
        }

        /// <summary>
        /// Undo操作（ピックアップの取り消し）
        /// </summary>
        public void Undo()
        {
            if (!executed) return;

            // ピックアップしたアイテムを元の位置に戻す
            foreach (var item in pickedUpItems)
            {
                if (item != null)
                {
                    item.SetActive(true);
                    // インベントリから削除
                    // 実際の実装では InventorySystem との連携
                }
            }

            pickedUpItems.Clear();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log("Pickup undone - items restored");
#endif

            executed = false;
        }

        /// <summary>
        /// このコマンドがUndo可能かどうか
        /// </summary>
        public bool CanUndo => executed && pickedUpItems.Count > 0;

        /// <summary>
        /// 磁力効果が現在アクティブかどうか
        /// </summary>
        public bool IsMagneticActive => isMagneticActive;
    }

    /// <summary>
    /// ピックアップ可能アイテムのインターフェース
    /// </summary>
    public interface IPickupableItem
    {
        string GetItemType();
        IItemData GetItemData();
        bool CanBePickedUp();
        void OnPickedUp(object picker);
    }

    /// <summary>
    /// アイテムデータのインターフェース
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