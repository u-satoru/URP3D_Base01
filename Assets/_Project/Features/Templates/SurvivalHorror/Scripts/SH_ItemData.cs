using UnityEngine;
using asterivo.Unity60.Core.Events;

namespace asterivo.Unity60.Features.Templates.SurvivalHorror
{
    /// <summary>
    /// SurvivalHorrorテンプレート用個別アイテムデータ
    /// アイテムの基本情報、効果、使用条件を定義
    /// </summary>
    [CreateAssetMenu(fileName = "SH_ItemData", menuName = "Templates/SurvivalHorror/Item Data")]
    public class SH_ItemData : ScriptableObject
    {
        [Header("Basic Information")]
        [SerializeField] private string itemId;
        [SerializeField] private string itemName;
        [SerializeField] private string description;
        [SerializeField] private Sprite icon;
        [SerializeField] private GameObject prefab;

        [Header("Item Classification")]
        [SerializeField] private ItemCategory category;
        [SerializeField] private ItemRarity rarity = ItemRarity.Common;
        [SerializeField] private ItemType itemType = ItemType.Consumable;

        [Header("Inventory Properties")]
        [SerializeField] private bool isStackable = true;
        [SerializeField] private int maxStackSize = 99;
        [SerializeField] private bool isUsable = true;
        [SerializeField] private bool isDroppable = true;
        [SerializeField] private bool isDiscardable = true;

        [Header("Spawn Settings")]
        [SerializeField] private float baseSpawnRate = 1.0f;
        [SerializeField] private float spawnWeight = 1.0f;
        [SerializeField] private bool canSpawnMultiple = false;
        [SerializeField] private Vector2Int spawnQuantityRange = new Vector2Int(1, 1);

        [Header("Usage Settings")]
        [SerializeField] private float useCooldown = 0f;
        [SerializeField] private bool consumeOnUse = true;
        [SerializeField] private AudioClip useSound;
        [SerializeField] private ParticleSystem useEffect;

        [Header("Item Effects")]
        [SerializeField] private ItemEffect[] effects;

        [Header("Conditional Usage")]
        [SerializeField] private bool requiresSpecificCondition = false;
        [SerializeField] private UsageCondition[] usageConditions;

        [Header("Save Game Integration")]
        [SerializeField] private bool isSaveItem = false;
        [SerializeField] private int saveUses = 1; // インクリボン等の使用回数制限

        [Header("Events")]
        [SerializeField] private GameEvent<SH_ItemData> onItemUsed;
        [SerializeField] private GameEvent<SH_ItemData> onItemPickedUp;
        [SerializeField] private GameEvent<SH_ItemData> onItemDropped;

        // Public Properties
        public string ItemId => itemId;
        public string ItemName => itemName;
        public string Description => description;
        public Sprite Icon => icon;
        public GameObject Prefab => prefab;
        public ItemCategory Category => category;
        public ItemRarity Rarity => rarity;
        public ItemType ItemType => itemType;
        public bool IsStackable => isStackable;
        public int MaxStackSize => maxStackSize;
        public bool IsUsable => isUsable;
        public bool IsDroppable => isDroppable;
        public bool IsDiscardable => isDiscardable;
        public float BaseSpawnRate => baseSpawnRate;
        public float SpawnWeight => spawnWeight;
        public bool CanSpawnMultiple => canSpawnMultiple;
        public Vector2Int SpawnQuantityRange => spawnQuantityRange;
        public float UseCooldown => useCooldown;
        public bool ConsumeOnUse => consumeOnUse;
        public ItemEffect[] Effects => effects;
        public bool IsSaveItem => isSaveItem;
        public int SaveUses => saveUses;
        public AudioClip UseSound => useSound;

        /// <summary>
        /// アイテムデータを初期化（動的生成用）
        /// </summary>
        public void Initialize(string id, string name, string desc, ItemCategory cat, ItemRarity rare)
        {
            itemId = id;
            itemName = name;
            description = desc;
            category = cat;
            rarity = rare;
        }

        /// <summary>
        /// アイテムを使用
        /// </summary>
        public bool UseItem(GameObject user)
        {
            if (!IsUsable) return false;

            // 使用条件をチェック
            if (requiresSpecificCondition && !CheckUsageConditions(user))
            {
                Debug.Log($"[SH_ItemData] Cannot use {itemName}: Conditions not met");
                return false;
            }

            // 効果を適用
            ApplyEffects(user);

            // 音響・視覚効果
            PlayUseEffects(user);

            // イベント発行
            onItemUsed?.Raise(this);

            Debug.Log($"[SH_ItemData] Used item: {itemName}");
            return true;
        }

        /// <summary>
        /// アイテムをピックアップ
        /// </summary>
        public void OnPickup(GameObject picker)
        {
            onItemPickedUp?.Raise(this);
            Debug.Log($"[SH_ItemData] Picked up: {itemName}");
        }

        /// <summary>
        /// アイテムをドロップ
        /// </summary>
        public void OnDrop(GameObject dropper)
        {
            if (!IsDroppable) return;

            onItemDropped?.Raise(this);
            Debug.Log($"[SH_ItemData] Dropped: {itemName}");
        }

        /// <summary>
        /// アイテムの効果を説明文として取得
        /// </summary>
        public string GetEffectDescription()
        {
            if (effects == null || effects.Length == 0)
                return "効果なし";

            var descriptions = new System.Text.StringBuilder();
            foreach (var effect in effects)
            {
                descriptions.AppendLine(effect.GetDescription());
            }

            return descriptions.ToString().Trim();
        }

        /// <summary>
        /// 難易度に基づく実際のスポーン率を計算
        /// </summary>
        public float GetAdjustedSpawnRate(DifficultyLevel difficulty)
        {
            float difficultyMultiplier = difficulty switch
            {
                DifficultyLevel.Easy => 1.5f,
                DifficultyLevel.Normal => 1.0f,
                DifficultyLevel.Hard => 0.7f,
                DifficultyLevel.Nightmare => 0.4f,
                _ => 1.0f
            };

            float rarityMultiplier = rarity switch
            {
                ItemRarity.Common => 1.0f,
                ItemRarity.Uncommon => 0.7f,
                ItemRarity.Rare => 0.4f,
                ItemRarity.Legendary => 0.1f,
                _ => 1.0f
            };

            return baseSpawnRate * difficultyMultiplier * rarityMultiplier;
        }

        /// <summary>
        /// 使用条件をチェック
        /// </summary>
        private bool CheckUsageConditions(GameObject user)
        {
            if (usageConditions == null || usageConditions.Length == 0)
                return true;

            foreach (var condition in usageConditions)
            {
                if (!condition.IsMet(user))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// アイテム効果を適用
        /// </summary>
        private void ApplyEffects(GameObject user)
        {
            if (effects == null) return;

            foreach (var effect in effects)
            {
                effect.Apply(user);
            }
        }

        /// <summary>
        /// 使用時の音響・視覚効果を再生
        /// </summary>
        private void PlayUseEffects(GameObject user)
        {
            // 音響効果
            if (useSound != null)
            {
                AudioSource.PlayClipAtPoint(useSound, user.transform.position);
            }

            // 視覚効果
            if (useEffect != null)
            {
                var effect = Instantiate(useEffect, user.transform.position, user.transform.rotation);
                effect.Play();

                // 一定時間後に削除
                if (effect.main.duration > 0)
                {
                    Destroy(effect.gameObject, effect.main.duration + 1f);
                }
            }
        }

        /// <summary>
        /// データの整合性を検証
        /// </summary>
        public bool ValidateData()
        {
            bool isValid = true;

            if (string.IsNullOrEmpty(itemId))
            {
                Debug.LogError($"[SH_ItemData] Item ID is empty for item: {name}");
                isValid = false;
            }

            if (string.IsNullOrEmpty(itemName))
            {
                Debug.LogError($"[SH_ItemData] Item name is empty for item: {itemId}");
                isValid = false;
            }

            if (isStackable && maxStackSize <= 0)
            {
                Debug.LogError($"[SH_ItemData] Stackable item {itemName} has invalid max stack size");
                isValid = false;
            }

            if (baseSpawnRate < 0)
            {
                Debug.LogError($"[SH_ItemData] Item {itemName} has negative spawn rate");
                isValid = false;
            }

            return isValid;
        }

        // エディタ専用機能
        #if UNITY_EDITOR
        private void OnValidate()
        {
            // エディタでの値変更時に自動検証
            ValidateData();

            // IDが空の場合、アセット名から生成
            if (string.IsNullOrEmpty(itemId) && !string.IsNullOrEmpty(name))
            {
                itemId = name.ToLower().Replace(" ", "_");
            }
        }

        [ContextMenu("Generate Unique ID")]
        private void GenerateUniqueId()
        {
            itemId = System.Guid.NewGuid().ToString("N")[..8]; // 8文字のユニークID
        }
        #endif
    }

    /// <summary>
    /// アイテムカテゴリー定義
    /// </summary>
    public enum ItemCategory
    {
        All,        // 全カテゴリー（検索用）
        Health,     // 回復アイテム（薬草、包帯など）
        Weapon,     // 武器（ナイフ、銃など）
        Ammunition, // 弾薬・消耗品
        Key,        // キーアイテム（鍵、カードキーなど）
        Document,   // 文書・ファイル（ストーリー関連）
        Special,    // 特殊アイテム（パズル要素など）
        Save        // セーブ関連（インクリボンなど）
    }

    /// <summary>
    /// アイテムレアリティ定義
    /// </summary>
    public enum ItemRarity
    {
        Common,    // 一般的（白）
        Uncommon,  // やや珍しい（緑）
        Rare,      // 珍しい（青）
        Legendary  // 伝説的（紫）
    }

    /// <summary>
    /// アイテムタイプ定義
    /// </summary>
    public enum ItemType
    {
        Consumable,  // 消耗品（使用すると消える）
        Equipment,   // 装備品（装備スロットを占有）
        KeyItem,     // キーアイテム（ストーリー進行用）
        Collectible, // 収集品（コレクション要素）
        Document     // 文書（読み物）
    }

    /// <summary>
    /// アイテム効果定義
    /// </summary>
    [System.Serializable]
    public class ItemEffect
    {
        [Header("Effect Configuration")]
        public EffectType effectType;
        public float value;
        public float duration;
        public bool isInstant = true;

        [Header("Target Settings")]
        public EffectTarget target = EffectTarget.User;
        public string targetComponentName; // 特定コンポーネント指定用

        public string GetDescription()
        {
            return effectType switch
            {
                EffectType.RestoreHealth => $"体力を{value}回復",
                EffectType.RestoreSanity => $"正気度を{value}回復",
                EffectType.ReduceFear => $"恐怖を{value}軽減",
                EffectType.TemporaryBoost => $"{duration}秒間能力向上",
                EffectType.UnlockDoor => "扉の解錠",
                EffectType.SaveGame => "ゲームをセーブ",
                _ => "不明な効果"
            };
        }

        public void Apply(GameObject target)
        {
            switch (effectType)
            {
                case EffectType.RestoreHealth:
                    // HealthComponentを探して体力回復
                    var healthComp = target.GetComponent<asterivo.Unity60.Core.Combat.HealthComponent>();
                    if (healthComp != null)
                    {
                        healthComp.Heal(value);
                    }
                    break;

                case EffectType.RestoreSanity:
                    // SanityComponentを探して正気度回復
                    var sanityComp = target.GetComponent<asterivo.Unity60.Core.Components.SanityComponent>();
                    if (sanityComp != null)
                    {
                        sanityComp.IncreaseSanity(value);
                    }
                    break;

                case EffectType.ReduceFear:
                    // 恐怖軽減効果の実装
                    break;

                case EffectType.SaveGame:
                    // セーブシステムとの連携
                    break;
            }
        }
    }

    /// <summary>
    /// 効果タイプ定義
    /// </summary>
    public enum EffectType
    {
        RestoreHealth,   // 体力回復
        RestoreSanity,   // 正気度回復
        ReduceFear,      // 恐怖軽減
        TemporaryBoost,  // 一時的能力向上
        UnlockDoor,      // 扉解錠
        SaveGame,        // ゲームセーブ
        Custom           // カスタム効果
    }

    /// <summary>
    /// 効果対象定義
    /// </summary>
    public enum EffectTarget
    {
        User,        // 使用者
        Area,        // 周辺エリア
        Global,      // ゲーム全体
        Specific     // 特定オブジェクト
    }

    /// <summary>
    /// 使用条件定義
    /// </summary>
    [System.Serializable]
    public class UsageCondition
    {
        public ConditionType conditionType;
        public float requiredValue;
        public string requiredItem;
        public string requiredLocation;

        public bool IsMet(GameObject user)
        {
            return conditionType switch
            {
                ConditionType.MinimumHealth => CheckHealthCondition(user),
                ConditionType.MaximumSanity => CheckSanityCondition(user),
                ConditionType.RequiredItem => CheckItemCondition(user),
                ConditionType.RequiredLocation => CheckLocationCondition(user),
                _ => true
            };
        }

        private bool CheckHealthCondition(GameObject user)
        {
            var healthComp = user.GetComponent<asterivo.Unity60.Core.Combat.HealthComponent>();
            return healthComp != null && healthComp.CurrentHealth >= requiredValue;
        }

        private bool CheckSanityCondition(GameObject user)
        {
            var sanityComp = user.GetComponent<asterivo.Unity60.Core.Components.SanityComponent>();
            return sanityComp != null && sanityComp.CurrentSanity <= requiredValue;
        }

        private bool CheckItemCondition(GameObject user)
        {
            // インベントリシステムとの連携
            var inventory = user.GetComponent<asterivo.Unity60.Core.Components.LimitedInventoryComponent>();
            return inventory != null && inventory.GetItemCount(requiredItem) > 0;
        }

        private bool CheckLocationCondition(GameObject user)
        {
            // 場所チェックの実装（将来の拡張用）
            return true;
        }
    }

    /// <summary>
    /// 条件タイプ定義
    /// </summary>
    public enum ConditionType
    {
        MinimumHealth,    // 最低体力要求
        MaximumSanity,    // 最大正気度制限
        RequiredItem,     // 必要アイテム
        RequiredLocation, // 必要場所
        TimeOfDay         // 時間制限
    }
}
