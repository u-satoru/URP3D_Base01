using UnityEngine;

namespace asterivo.Unity60.Features.Templates.ActionRPG.Data
{
    /// <summary>
    /// アイテムの基本データ定義
    /// </summary>
    [CreateAssetMenu(fileName = "New Item Data", menuName = "ActionRPG/Item Data")]
    public class ItemData : ScriptableObject
    {
        [Header("アイテム情報")]
        [SerializeField] private string _itemName = "ルーン";
        [SerializeField] private string _description = "経験値として使用できる神秘の石";
        [SerializeField] private Sprite _icon;
        [SerializeField] private GameObject _worldPrefab;
        
        [Header("アイテム種別")]
        [SerializeField] private ItemType _itemType = ItemType.Consumable;
        [SerializeField] private ItemRarity _rarity = ItemRarity.Common;
        
        [Header("スタック設定")]
        [SerializeField] private bool _stackable = true;
        [SerializeField] private int _maxStackSize = 999;
        
        [Header("価値")]
        [SerializeField] private int _baseValue = 1;
        [SerializeField] private int _sellPrice = 1;
        
        [Header("使用効果（消耗品の場合）")]
        [SerializeField] private bool _consumeOnUse = true;
        [SerializeField] private int _healthRestore = 0;
        [SerializeField] private int _focusRestore = 0;
        [SerializeField] private int _experienceValue = 1;

        // プロパティ
        public string ItemName => _itemName;
        public string Description => _description;
        public Sprite Icon => _icon;
        public GameObject WorldPrefab => _worldPrefab;
        public ItemType ItemType => _itemType;
        public ItemRarity Rarity => _rarity;
        public bool Stackable => _stackable;
        public int MaxStackSize => _maxStackSize;
        public int BaseValue => _baseValue;
        public int SellPrice => _sellPrice;
        public bool ConsumeOnUse => _consumeOnUse;
        public int HealthRestore => _healthRestore;
        public int FocusRestore => _focusRestore;
        public int ExperienceValue => _experienceValue;

        /// <summary>
        /// レアリティを考慮した価値
        /// </summary>
        public int GetAdjustedValue()
        {
            float multiplier = _rarity switch
            {
                ItemRarity.Common => 1.0f,
                ItemRarity.Uncommon => 1.5f,
                ItemRarity.Rare => 2.0f,
                ItemRarity.Epic => 3.0f,
                ItemRarity.Legendary => 5.0f,
                _ => 1.0f
            };
            return Mathf.RoundToInt(_baseValue * multiplier);
        }

        /// <summary>
        /// アイテムを使用できるかチェック
        /// </summary>
        public bool CanUse()
        {
            return _itemType == ItemType.Consumable && (_healthRestore > 0 || _focusRestore > 0 || _experienceValue > 0);
        }
    }

    /// <summary>
    /// アイテムの種類
    /// </summary>
    public enum ItemType
    {
        Consumable,    // 消耗品
        Equipment,     // 装備品
        Material,      // 素材
        KeyItem,       // 重要アイテム
        Currency       // 通貨
    }

    /// <summary>
    /// アイテムのレアリティ
    /// </summary>
    public enum ItemRarity
    {
        Common,        // コモン（白）
        Uncommon,      // アンコモン（緑）
        Rare,          // レア（青）
        Epic,          // エピック（紫）
        Legendary      // レジェンダリー（金）
    }
}
