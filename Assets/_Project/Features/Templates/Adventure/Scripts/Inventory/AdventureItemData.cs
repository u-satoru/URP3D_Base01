using UnityEngine;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Features.Templates.Adventure.Inventory
{
    /// <summary>
    /// Adventure Template用アイテムデータ
    /// アドベンチャーゲーム特有のアイテム情報を管理
    /// </summary>
    [CreateAssetMenu(fileName = "AdventureItem", menuName = "Templates/Adventure/Adventure Item Data")]
    public class AdventureItemData : ScriptableObject
    {
        [TabGroup("Basic", "Basic Information")]
        [Header("Basic Information")]
        [SerializeField]
        [Tooltip("アイテムの一意識別子")]
        public string itemId = "";

        [TabGroup("Basic", "Basic Information")]
        [SerializeField]
        [Tooltip("アイテム名")]
        public string itemName = "";

        [TabGroup("Basic", "Basic Information")]
        [SerializeField]
        [Tooltip("アイテムの説明")]
        [TextArea(3, 5)]
        public string description = "";

        [TabGroup("Basic", "Basic Information")]
        [SerializeField]
        [Tooltip("アイテムのアイコン")]
        public Sprite icon;

        [TabGroup("Properties", "Item Properties")]
        [Header("Item Properties")]
        [SerializeField]
        [Tooltip("アイテムの種類")]
        public AdventureItemType itemType = AdventureItemType.Consumable;

        [TabGroup("Properties", "Item Properties")]
        [SerializeField]
        [Tooltip("アイテムの重量")]
        [Min(0f)]
        public float weight = 1f;

        [TabGroup("Properties", "Item Properties")]
        [SerializeField]
        [Tooltip("アイテムのレア度")]
        public AdventureItemRarity rarity = AdventureItemRarity.Common;

        [TabGroup("Properties", "Item Properties")]
        [SerializeField]
        [Tooltip("最大スタック数")]
        [Min(1)]
        public int maxStackSize = 1;

        [TabGroup("Properties", "Item Properties")]
        [SerializeField]
        [Tooltip("アイテムの価値")]
        [Min(0)]
        public int value = 0;

        [TabGroup("Usage", "Usage Properties")]
        [Header("Usage Properties")]
        [SerializeField]
        [Tooltip("使用可能アイテムかどうか")]
        public bool isUsable = false;

        [TabGroup("Usage", "Usage Properties")]
        [SerializeField]
        [Tooltip("使用時に消費されるかどうか")]
        [ShowIf("isUsable")]
        public bool isConsumable = true;

        [TabGroup("Usage", "Usage Properties")]
        [SerializeField]
        [Tooltip("使用時のクールダウン時間（秒）")]
        [ShowIf("isUsable")]
        [Min(0)]
        public float cooldownTime = 0f;

        [TabGroup("Effects", "Effect Properties")]
        [Header("Effect Properties")]
        [SerializeField]
        [Tooltip("使用時の効果の説明")]
        [ShowIf("isUsable")]
        [TextArea(2, 4)]
        public string effectDescription = "";

        [TabGroup("Effects", "Effect Properties")]
        [SerializeField]
        [Tooltip("効果の強度")]
        [ShowIf("isUsable")]
        public float effectPower = 1.0f;

        [TabGroup("Effects", "Effect Properties")]
        [SerializeField]
        [Tooltip("効果の持続時間（秒）")]
        [ShowIf("isUsable")]
        [Min(0)]
        public float effectDuration = 0f;

        [TabGroup("Quest", "Quest Properties")]
        [Header("Quest Integration")]
        [SerializeField]
        [Tooltip("クエストアイテムかどうか")]
        public bool isQuestItem = false;

        [TabGroup("Quest", "Quest Properties")]
        [SerializeField]
        [Tooltip("関連するクエストID")]
        [ShowIf("isQuestItem")]
        public string relatedQuestId = "";

        [TabGroup("Quest", "Quest Properties")]
        [SerializeField]
        [Tooltip("クエスト完了時に自動削除されるかどうか")]
        [ShowIf("isQuestItem")]
        public bool autoRemoveOnQuestComplete = true;

        [TabGroup("Audio", "Audio Properties")]
        [Header("Audio")]
        [SerializeField]
        [Tooltip("アイテム取得時のサウンド")]
        public AudioClip pickupSound;

        [TabGroup("Audio", "Audio Properties")]
        [SerializeField]
        [Tooltip("アイテム使用時のサウンド")]
        [ShowIf("isUsable")]
        public AudioClip useSound;

        [TabGroup("Validation", "Validation")]
        [Button("Validate Item Data")]
        private void ValidateItemData()
        {
            bool isValid = true;
            System.Text.StringBuilder errors = new System.Text.StringBuilder();

            if (string.IsNullOrEmpty(itemId))
            {
                errors.AppendLine("Item ID cannot be empty");
                isValid = false;
            }

            if (string.IsNullOrEmpty(itemName))
            {
                errors.AppendLine("Item Name cannot be empty");
                isValid = false;
            }

            if (maxStackSize <= 0)
            {
                errors.AppendLine("Max Stack Size must be greater than 0");
                isValid = false;
            }

            if (value < 0)
            {
                errors.AppendLine("Value cannot be negative");
                isValid = false;
            }

            if (isUsable && effectPower < 0)
            {
                errors.AppendLine("Effect Power cannot be negative");
                isValid = false;
            }

            if (isUsable && effectDuration < 0)
            {
                errors.AppendLine("Effect Duration cannot be negative");
                isValid = false;
            }

            if (isQuestItem && string.IsNullOrEmpty(relatedQuestId))
            {
                errors.AppendLine("Quest Item must have a related Quest ID");
                isValid = false;
            }

            if (isValid)
            {
                Debug.Log($"[AdventureItemData] '{itemName}' validation passed!");
            }
            else
            {
                Debug.LogError($"[AdventureItemData] '{itemName}' validation failed:\n{errors}");
            }
        }

        [TabGroup("Validation", "Validation")]
        [Button("Log Item Info")]
        private void LogItemInfo()
        {
            Debug.Log($"[AdventureItemData] Item Info:\n" +
                     $"ID: {itemId}\n" +
                     $"Name: {itemName}\n" +
                     $"Type: {itemType}\n" +
                     $"Rarity: {rarity}\n" +
                     $"Stack Size: {maxStackSize}\n" +
                     $"Value: {value}\n" +
                     $"Usable: {isUsable}\n" +
                     $"Consumable: {isConsumable}\n" +
                     $"Quest Item: {isQuestItem}\n" +
                     $"Description: {description}");
        }

        private void OnValidate()
        {
            // 実行時の値制限
            if (string.IsNullOrEmpty(itemId) && !string.IsNullOrEmpty(itemName))
            {
                itemId = itemName.Replace(" ", "_").ToLower();
            }

            maxStackSize = Mathf.Max(1, maxStackSize);
            value = Mathf.Max(0, value);
            effectPower = Mathf.Max(0f, effectPower);
            effectDuration = Mathf.Max(0f, effectDuration);
            cooldownTime = Mathf.Max(0f, cooldownTime);
        }
    }

    /// <summary>
    /// Adventure Template用アイテム種類
    /// </summary>
    public enum AdventureItemType
    {
        [Tooltip("消費アイテム（ポーション、食べ物など）")]
        Consumable,
        
        [Tooltip("ツール・道具（鍵、ロープ、懐中電灯など）")]
        Tool,
        
        [Tooltip("キーアイテム（ストーリー進行に必要）")]
        Key,
        
        [Tooltip("クエストアイテム（クエスト専用アイテム）")]
        QuestItem,
        
        [Tooltip("文書・資料（日記、手紙、地図など）")]
        Document,
        
        [Tooltip("収集アイテム（宝石、コイン、記念品など）")]
        Collectible,
        
        [Tooltip("材料・素材（クラフト用）")]
        Material
    }

    /// <summary>
    /// Adventure Template用アイテムレア度
    /// </summary>
    public enum AdventureItemRarity
    {
        [Tooltip("一般的（グレー）")]
        Common,
        
        [Tooltip("珍しい（緑）")]
        Uncommon,
        
        [Tooltip("稀少（青）")]
        Rare,
        
        [Tooltip("史詩（紫）")]
        Epic,
        
        [Tooltip("伝説（オレンジ）")]
        Legendary
    }
}