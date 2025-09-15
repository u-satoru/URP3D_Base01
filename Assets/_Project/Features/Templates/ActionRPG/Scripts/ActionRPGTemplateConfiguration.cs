using UnityEngine;
using asterivo.Unity60.Features.Templates.Common;

namespace asterivo.Unity60.Features.Templates.ActionRPG
{
    /// <summary>
    /// アクションRPGテンプレート固有設定システム
    /// TASK-004: 7ジャンルテンプレート完全実装 - Action RPG Template Configuration（中優先・FR-5対応）
    /// キャラクター成長・装備管理・スキルシステム・インベントリ管理
    /// 名前空間: asterivo.Unity60.Features.Templates.ActionRPG.*
    /// </summary>
    [CreateAssetMenu(fileName = "ActionRPGTemplateConfiguration", menuName = "Templates/ActionRPG/Configuration")]
    public class ActionRPGTemplateConfiguration : ScriptableObject
    {
        [Header("キャラクター成長システム")]
        [SerializeField] private int _maxLevel = 50;
        [SerializeField] private int _baseExperience = 100;
        [SerializeField] private float _experienceMultiplier = 1.5f;
        [SerializeField] private bool _enableMultiClassSystem = true;
        [SerializeField] private bool _enablePrestigeSystem = false;
        [SerializeField] private int _prestigeUnlockLevel = 50;
        
        [Header("基本ステータス設定")]
        [SerializeField] private int _baseHealth = 100;
        [SerializeField] private int _baseMana = 50;
        [SerializeField] private int _baseStamina = 80;
        [SerializeField] private int _baseStrength = 10;
        [SerializeField] private int _baseIntelligence = 10;
        [SerializeField] private int _baseDexterity = 10;
        [SerializeField] private int _baseVitality = 10;
        [SerializeField] private int _baseDefense = 5;
        [SerializeField] private int _baseMagicResistance = 5;
        
        [Header("ステータス成長率")]
        [SerializeField] private float _healthPerLevel = 15f;
        [SerializeField] private float _manaPerLevel = 8f;
        [SerializeField] private float _staminaPerLevel = 10f;
        [SerializeField] private float _statGrowthVariance = 0.2f;
        [SerializeField] private bool _enableStatRespec = true;
        [SerializeField] private int _respecCostMultiplier = 1000;
        
        [Header("スキルシステム設定")]
        [SerializeField] private bool _enableSkillTrees = true;
        [SerializeField] private int _maxSkillPoints = 100;
        [SerializeField] private int _skillPointsPerLevel = 2;
        [SerializeField] private bool _enableActiveSkills = true;
        [SerializeField] private bool _enablePassiveSkills = true;
        [SerializeField] private int _maxActiveSkillSlots = 8;
        [SerializeField] private bool _enableSkillCombos = true;
        [SerializeField] private float _globalSkillCooldownReduction = 0f;
        
        [Header("クラスシステム設定")]
        [SerializeField] private string[] _availableClasses = new[]
        {
            "Warrior", "Mage", "Rogue", "Archer", "Cleric", "Paladin"
        };
        [SerializeField] private bool _enableClassSwitching = false;
        [SerializeField] private bool _enableHybridBuilds = true;
        [SerializeField] private int _multiClassUnlockLevel = 20;
        
        [Header("装備システム設定")]
        [SerializeField] private bool _enableEquipmentSystem = true;
        [SerializeField] private string[] _equipmentSlots = new[]
        {
            "Weapon", "Shield", "Helmet", "Chest", "Legs", "Boots", "Gloves", "Ring1", "Ring2", "Amulet"
        };
        [SerializeField] private bool _enableWeaponEnchantment = true;
        [SerializeField] private bool _enableArmorEnchantment = true;
        [SerializeField] private int _maxEnchantmentLevel = 10;
        [SerializeField] private bool _enableSetBonuses = true;
        [SerializeField] private bool _enableItemDurability = true;
        [SerializeField] private float _durabilityDecayRate = 0.1f;
        
        [Header("インベントリ管理設定")]
        [SerializeField] private int _inventorySize = 50;
        [SerializeField] private bool _enableExpandableInventory = true;
        [SerializeField] private int _maxInventoryExpansion = 100;
        [SerializeField] private bool _enableItemStacking = true;
        [SerializeField] private int _defaultStackSize = 99;
        [SerializeField] private bool _enableItemSorting = true;
        [SerializeField] private bool _enableItemFiltering = true;
        
        [Header("ドロップ・ルートシステム")]
        [SerializeField] private bool _enableRandomDrops = true;
        [SerializeField] private float _baseDropRate = 0.1f;
        [SerializeField] private float _rareDropRate = 0.01f;
        [SerializeField] private float _legendaryDropRate = 0.001f;
        [SerializeField] private bool _enableMagicFind = true;
        [SerializeField] private float _magicFindEffectiveness = 1f;
        [SerializeField] private bool _enableBossDrops = true;
        
        [Header("経済・商業システム")]
        [SerializeField] private bool _enableTradingSystem = true;
        [SerializeField] private bool _enablePlayerToPlayerTrade = false;
        [SerializeField] private bool _enableVendorSystem = true;
        [SerializeField] private float _vendorPriceMultiplier = 1.2f;
        [SerializeField] private bool _enableRepairSystem = true;
        [SerializeField] private float _repairCostMultiplier = 0.1f;
        
        [Header("戦闘システム設定")]
        [SerializeField] private bool _enableRealTimeCombat = true;
        [SerializeField] private bool _enableDodgeRoll = true;
        [SerializeField] private float _dodgeRollCooldown = 1f;
        [SerializeField] private bool _enableBlocking = true;
        [SerializeField] private float _blockEffectiveness = 0.5f;
        [SerializeField] private bool _enableCriticalHits = true;
        [SerializeField] private float _baseCriticalChance = 0.05f;
        [SerializeField] private float _criticalDamageMultiplier = 2f;
        
        [Header("クエストシステム設定")]
        [SerializeField] private bool _enableQuestSystem = true;
        [SerializeField] private int _maxActiveQuests = 10;
        [SerializeField] private bool _enableDailyQuests = true;
        [SerializeField] private bool _enableWeeklyQuests = true;
        [SerializeField] private bool _enableQuestChaining = true;
        [SerializeField] private bool _enableQuestTracking = true;
        
        [Header("実績・進捗システム")]
        [SerializeField] private bool _enableAchievementSystem = true;
        [SerializeField] private bool _enableStatTracking = true;
        [SerializeField] private bool _enableLeaderboards = false;
        [SerializeField] private bool _enableProgressSharing = false;
        
        [Header("学習目標設定（Learn & Grow対応）")]
        [SerializeField] private string[] _learningObjectives = new[]
        {
            "BasicCombat_ActionRPG",
            "CharacterProgression_Understanding",
            "EquipmentManagement_Basics",
            "SkillTree_Navigation",
            "InventoryManagement_Efficiency",
            "QuestCompletion_Strategies",
            "StatBuilding_Optimization",
            "CraftingSystem_Mastery",
            "CombatTactics_Advanced",
            "EndgameContent_Preparation"
        };
        
        [Header("バランス・難易度設定")]
        [SerializeField] private float _enemyLevelScaling = 1f;
        [SerializeField] private float _experienceScaling = 1f;
        [SerializeField] private float _lootQualityScaling = 1f;
        [SerializeField] private bool _enableDynamicDifficulty = true;
        [SerializeField] private float _difficultyAdaptationRate = 0.1f;
        
        // Properties - Character Progression
        public int MaxLevel => _maxLevel;
        public int BaseExperience => _baseExperience;
        public float ExperienceMultiplier => _experienceMultiplier;
        public bool EnableMultiClassSystem => _enableMultiClassSystem;
        public bool EnablePrestigeSystem => _enablePrestigeSystem;
        public int PrestigeUnlockLevel => _prestigeUnlockLevel;
        
        // Base Stats Properties
        public int BaseHealth => _baseHealth;
        public int BaseMana => _baseMana;
        public int BaseStamina => _baseStamina;
        public int BaseStrength => _baseStrength;
        public int BaseIntelligence => _baseIntelligence;
        public int BaseDexterity => _baseDexterity;
        public int BaseVitality => _baseVitality;
        public int BaseDefense => _baseDefense;
        public int BaseMagicResistance => _baseMagicResistance;
        
        // Growth Properties
        public float HealthPerLevel => _healthPerLevel;
        public float ManaPerLevel => _manaPerLevel;
        public float StaminaPerLevel => _staminaPerLevel;
        public float StatGrowthVariance => _statGrowthVariance;
        public bool EnableStatRespec => _enableStatRespec;
        public int RespecCostMultiplier => _respecCostMultiplier;
        
        // Skill System Properties
        public bool EnableSkillTrees => _enableSkillTrees;
        public int MaxSkillPoints => _maxSkillPoints;
        public int SkillPointsPerLevel => _skillPointsPerLevel;
        public bool EnableActiveSkills => _enableActiveSkills;
        public bool EnablePassiveSkills => _enablePassiveSkills;
        public int MaxActiveSkillSlots => _maxActiveSkillSlots;
        public bool EnableSkillCombos => _enableSkillCombos;
        public float GlobalSkillCooldownReduction => _globalSkillCooldownReduction;
        
        // Class System Properties
        public string[] AvailableClasses => _availableClasses;
        public bool EnableClassSwitching => _enableClassSwitching;
        public bool EnableHybridBuilds => _enableHybridBuilds;
        public int MultiClassUnlockLevel => _multiClassUnlockLevel;
        
        // Equipment Properties
        public bool EnableEquipmentSystem => _enableEquipmentSystem;
        public string[] EquipmentSlots => _equipmentSlots;
        public bool EnableWeaponEnchantment => _enableWeaponEnchantment;
        public bool EnableArmorEnchantment => _enableArmorEnchantment;
        public int MaxEnchantmentLevel => _maxEnchantmentLevel;
        public bool EnableSetBonuses => _enableSetBonuses;
        public bool EnableItemDurability => _enableItemDurability;
        public float DurabilityDecayRate => _durabilityDecayRate;
        
        // Inventory Properties
        public int InventorySize => _inventorySize;
        public bool EnableExpandableInventory => _enableExpandableInventory;
        public int MaxInventoryExpansion => _maxInventoryExpansion;
        public bool EnableItemStacking => _enableItemStacking;
        public int DefaultStackSize => _defaultStackSize;
        public bool EnableItemSorting => _enableItemSorting;
        public bool EnableItemFiltering => _enableItemFiltering;
        
        // Drop System Properties
        public bool EnableRandomDrops => _enableRandomDrops;
        public float BaseDropRate => _baseDropRate;
        public float RareDropRate => _rareDropRate;
        public float LegendaryDropRate => _legendaryDropRate;
        public bool EnableMagicFind => _enableMagicFind;
        public float MagicFindEffectiveness => _magicFindEffectiveness;
        public bool EnableBossDrops => _enableBossDrops;
        
        // Economy Properties
        public bool EnableTradingSystem => _enableTradingSystem;
        public bool EnablePlayerToPlayerTrade => _enablePlayerToPlayerTrade;
        public bool EnableVendorSystem => _enableVendorSystem;
        public float VendorPriceMultiplier => _vendorPriceMultiplier;
        public bool EnableRepairSystem => _enableRepairSystem;
        public float RepairCostMultiplier => _repairCostMultiplier;
        
        // Combat Properties
        public bool EnableRealTimeCombat => _enableRealTimeCombat;
        public bool EnableDodgeRoll => _enableDodgeRoll;
        public float DodgeRollCooldown => _dodgeRollCooldown;
        public bool EnableBlocking => _enableBlocking;
        public float BlockEffectiveness => _blockEffectiveness;
        public bool EnableCriticalHits => _enableCriticalHits;
        public float BaseCriticalChance => _baseCriticalChance;
        public float CriticalDamageMultiplier => _criticalDamageMultiplier;
        
        // Quest Properties
        public bool EnableQuestSystem => _enableQuestSystem;
        public int MaxActiveQuests => _maxActiveQuests;
        public bool EnableDailyQuests => _enableDailyQuests;
        public bool EnableWeeklyQuests => _enableWeeklyQuests;
        public bool EnableQuestChaining => _enableQuestChaining;
        public bool EnableQuestTracking => _enableQuestTracking;
        
        // Achievement Properties
        public bool EnableAchievementSystem => _enableAchievementSystem;
        public bool EnableStatTracking => _enableStatTracking;
        public bool EnableLeaderboards => _enableLeaderboards;
        public bool EnableProgressSharing => _enableProgressSharing;
        
        // Learning Properties
        public string[] LearningObjectives => _learningObjectives;
        
        // Balance Properties
        public float EnemyLevelScaling => _enemyLevelScaling;
        public float ExperienceScaling => _experienceScaling;
        public float LootQualityScaling => _lootQualityScaling;
        public bool EnableDynamicDifficulty => _enableDynamicDifficulty;
        public float DifficultyAdaptationRate => _difficultyAdaptationRate;
        
        /// <summary>
        /// キャラクター成長システムセットアップ
        /// </summary>
        /// <param name="player">プレイヤーGameObject</param>
        public void SetupCharacterProgressionSystem(GameObject player)
        {
            if (player == null) return;
            
            Debug.Log("Setting up ActionRPG character progression system...");
            
            // TODO: CharacterStatsManager追加
            // TODO: ExperienceManager追加
            // TODO: LevelUpSystem追加
            
            if (_enableMultiClassSystem)
            {
                // TODO: MultiClassManager追加
                Debug.Log($"Multi-class system enabled: Unlock level {_multiClassUnlockLevel}");
            }
            
            if (_enablePrestigeSystem)
            {
                // TODO: PrestigeManager追加
                Debug.Log($"Prestige system enabled: Unlock level {_prestigeUnlockLevel}");
            }
            
            Debug.Log($"Character progression setup: MaxLevel={_maxLevel}, BaseExp={_baseExperience}");
        }
        
        /// <summary>
        /// スキルシステムセットアップ
        /// </summary>
        /// <param name="player">プレイヤーGameObject</param>
        public void SetupSkillSystem(GameObject player)
        {
            if (player == null || !_enableSkillTrees) return;
            
            Debug.Log("Setting up skill system...");
            
            // TODO: SkillTreeManager追加
            // TODO: ActiveSkillManager追加
            // TODO: PassiveSkillManager追加
            
            if (_enableSkillCombos)
            {
                // TODO: SkillComboSystem追加
                Debug.Log("Skill combo system enabled");
            }
            
            Debug.Log($"Skill system setup: MaxPoints={_maxSkillPoints}, PointsPerLevel={_skillPointsPerLevel}");
        }
        
        /// <summary>
        /// 装備システムセットアップ
        /// </summary>
        /// <param name="player">プレイヤーGameObject</param>
        public void SetupEquipmentSystem(GameObject player)
        {
            if (player == null || !_enableEquipmentSystem) return;
            
            Debug.Log("Setting up equipment system...");
            
            // TODO: EquipmentManager追加
            // TODO: ItemSlotManager追加
            
            if (_enableWeaponEnchantment || _enableArmorEnchantment)
            {
                // TODO: EnchantmentSystem追加
                Debug.Log($"Enchantment system enabled: MaxLevel={_maxEnchantmentLevel}");
            }
            
            if (_enableSetBonuses)
            {
                // TODO: SetBonusManager追加
                Debug.Log("Set bonus system enabled");
            }
            
            if (_enableItemDurability)
            {
                // TODO: DurabilitySystem追加
                Debug.Log($"Item durability enabled: Decay rate={_durabilityDecayRate}");
            }
            
            Debug.Log($"Equipment system setup: {_equipmentSlots.Length} equipment slots");
        }
        
        /// <summary>
        /// インベントリシステムセットアップ
        /// </summary>
        /// <param name="player">プレイヤーGameObject</param>
        public void SetupInventorySystem(GameObject player)
        {
            if (player == null) return;
            
            Debug.Log("Setting up inventory system...");
            
            // TODO: InventoryManager追加
            // TODO: ItemStackingSystem追加
            
            if (_enableItemSorting)
            {
                // TODO: ItemSortingSystem追加
            }
            
            if (_enableItemFiltering)
            {
                // TODO: ItemFilteringSystem追加
            }
            
            Debug.Log($"Inventory setup: Size={_inventorySize}, Expandable={_enableExpandableInventory}");
        }
        
        /// <summary>
        /// 戦闘システムセットアップ
        /// </summary>
        /// <param name="player">プレイヤーGameObject</param>
        public void SetupCombatSystem(GameObject player)
        {
            if (player == null) return;
            
            Debug.Log("Setting up ActionRPG combat system...");
            
            // TODO: ActionRPGCombatController追加
            
            if (_enableDodgeRoll)
            {
                // TODO: DodgeRollController追加
                Debug.Log($"Dodge roll enabled: Cooldown={_dodgeRollCooldown}s");
            }
            
            if (_enableBlocking)
            {
                // TODO: BlockingController追加
                Debug.Log($"Blocking enabled: Effectiveness={_blockEffectiveness:P}");
            }
            
            if (_enableCriticalHits)
            {
                // TODO: CriticalHitSystem追加
                Debug.Log($"Critical hits: Base chance={_baseCriticalChance:P}, Damage={_criticalDamageMultiplier}x");
            }
        }
        
        /// <summary>
        /// クエストシステムセットアップ
        /// </summary>
        /// <param name="gameManager">ゲームマネージャー</param>
        public void SetupQuestSystem(GameObject gameManager)
        {
            if (gameManager == null || !_enableQuestSystem) return;
            
            Debug.Log("Setting up quest system...");
            
            // TODO: QuestManager追加
            // TODO: QuestTrackerUI追加
            
            if (_enableDailyQuests)
            {
                // TODO: DailyQuestSystem追加
                Debug.Log("Daily quests enabled");
            }
            
            if (_enableWeeklyQuests)
            {
                // TODO: WeeklyQuestSystem追加
                Debug.Log("Weekly quests enabled");
            }
            
            Debug.Log($"Quest system setup: Max active quests={_maxActiveQuests}");
        }
        
        /// <summary>
        /// 経済システムセットアップ
        /// </summary>
        /// <param name="gameManager">ゲームマネージャー</param>
        public void SetupEconomySystem(GameObject gameManager)
        {
            if (gameManager == null || !_enableTradingSystem) return;
            
            Debug.Log("Setting up economy system...");
            
            if (_enableVendorSystem)
            {
                // TODO: VendorManager追加
                Debug.Log($"Vendor system enabled: Price multiplier={_vendorPriceMultiplier}");
            }
            
            if (_enableRepairSystem)
            {
                // TODO: RepairSystem追加
                Debug.Log($"Repair system enabled: Cost multiplier={_repairCostMultiplier}");
            }
            
            if (_enablePlayerToPlayerTrade)
            {
                // TODO: PlayerTradeSystem追加
                Debug.Log("Player-to-player trading enabled");
            }
        }
        
        /// <summary>
        /// 経験値計算
        /// </summary>
        /// <param name="level">レベル</param>
        /// <returns>そのレベルに必要な経験値</returns>
        public int CalculateExperienceForLevel(int level)
        {
            if (level <= 1) return 0;
            
            var totalExp = 0f;
            for (int i = 2; i <= level; i++)
            {
                totalExp += _baseExperience * Mathf.Pow(_experienceMultiplier, i - 2);
            }
            
            return Mathf.RoundToInt(totalExp);
        }
        
        /// <summary>
        /// ステータス成長値計算
        /// </summary>
        /// <param name="level">レベル</param>
        /// <param name="baseStat">基本ステータス</param>
        /// <param name="growthPerLevel">レベルあたり成長量</param>
        /// <returns>計算されたステータス値</returns>
        public int CalculateStatForLevel(int level, int baseStat, float growthPerLevel)
        {
            var variance = Random.Range(-_statGrowthVariance, _statGrowthVariance);
            var growth = growthPerLevel * (level - 1) * (1f + variance);
            return Mathf.RoundToInt(baseStat + growth);
        }
        
        /// <summary>
        /// 学習目標の進捗チェック
        /// </summary>
        /// <param name="completedObjectives">完了した学習目標</param>
        /// <returns>全体完了率（0-1）</returns>
        public float CalculateLearningProgress(string[] completedObjectives)
        {
            if (_learningObjectives.Length == 0)
                return 1f;
                
            int completedCount = 0;
            foreach (var objective in _learningObjectives)
            {
                if (System.Array.Exists(completedObjectives, completed => completed == objective))
                {
                    completedCount++;
                }
            }
            
            var progress = (float)completedCount / _learningObjectives.Length;
            Debug.Log($"ActionRPG learning progress: {progress:P} ({completedCount}/{_learningObjectives.Length})");
            
            return progress;
        }
        
        /// <summary>
        /// デバッグ情報を表示
        /// </summary>
        [ContextMenu("Print ActionRPG Configuration")]
        public void PrintConfiguration()
        {
            Debug.Log("=== ActionRPG Template Configuration ===");
            Debug.Log($"Character Progression: MaxLevel={_maxLevel}, MultiClass={_enableMultiClassSystem}");
            Debug.Log($"Stats: Health={_baseHealth}+{_healthPerLevel}/lv, Mana={_baseMana}+{_manaPerLevel}/lv");
            Debug.Log($"Skills: Trees={_enableSkillTrees}, MaxPoints={_maxSkillPoints}");
            Debug.Log($"Equipment: Enabled={_enableEquipmentSystem}, Slots={_equipmentSlots.Length}");
            Debug.Log($"Inventory: Size={_inventorySize}, Expandable={_enableExpandableInventory}");
            Debug.Log($"Combat: RealTime={_enableRealTimeCombat}, Dodge={_enableDodgeRoll}, Crit={_enableCriticalHits}");
            Debug.Log($"Learning Objectives: {_learningObjectives.Length} objectives");
        }
    }
}