using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core;
using asterivo.Unity60.Features.Templates.ActionRPG.Data;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Features.Templates.ActionRPG.Character
{
    /// <summary>
    /// キャラクター成長管理システム
    /// レベルアップ、経験値、スキルポイント、属性成長を管理
    /// </summary>
    [System.Serializable]
    public class CharacterProgressionManager : MonoBehaviour
    {
        #region Events
        
        public event System.Action<int> OnLevelUp;
        public event System.Action<int> OnExperienceGained;
        public event System.Action<int> OnSkillPointsGained;
        public event System.Action<CharacterAttribute, int> OnAttributeIncreased;
        
        #endregion
        
        #region Configuration

        [TabGroup("Progression", "Core Settings")]
        [Header("Progression Settings")]
        [SerializeField] private CharacterProgressionData progressionData;
        [SerializeField] private ExperienceCurveData experienceCurve;
        
        [TabGroup("Progression", "Core Settings")]
        [Header("Starting Values")]
        [SerializeField] private int startingLevel = 1;
        [SerializeField] private int startingExperience = 0;
        [SerializeField] private int startingSkillPoints = 0;
        [SerializeField] private int maxLevel = 100;
        
        [TabGroup("Progression", "Core Settings")]
        [Header("Level Up Rewards")]
        [SerializeField] private int skillPointsPerLevel = 1;
        [SerializeField] private int attributePointsPerLevel = 2;
        [SerializeField] private bool autoAllocateAttributes = false;
        
        [TabGroup("Progression", "Current State")]
        [Header("Current Character State")]
        [SerializeField, ReadOnly] private int currentLevel;
        [SerializeField, ReadOnly] private int currentExperience;
        [SerializeField, ReadOnly] private int experienceToNextLevel;
        [SerializeField, ReadOnly] private int availableSkillPoints;
        [SerializeField, ReadOnly] private int availableAttributePoints;
        [SerializeField, ReadOnly] private float progressToNextLevel;

        #endregion

        #region Character Attributes

        [System.Serializable]
        public class CharacterAttributes
        {
            [TabGroup("Attributes", "Core")]
            public int strength = 10;        // 物理攻撃力、持ち物重量
            public int dexterity = 10;       // 攻撃速度、回避率、クリティカル率
            public int intelligence = 10;    // 魔法攻撃力、MP最大値
            public int vitality = 10;        // HP最大値、物理防御力
            public int wisdom = 10;          // 魔法防御力、MP回復速度
            public int luck = 10;            // ドロップ率、クリティカル率
            
            [TabGroup("Attributes", "Derived")]
            [ReadOnly] public float healthMultiplier = 1f;
            [ReadOnly] public float manaMultiplier = 1f;
            [ReadOnly] public float damageMultiplier = 1f;
            [ReadOnly] public float defenseMultiplier = 1f;
            [ReadOnly] public float speedMultiplier = 1f;
            
            public void CalculateDerivedStats()
            {
                healthMultiplier = 1f + (vitality - 10) * 0.1f;
                manaMultiplier = 1f + (intelligence - 10) * 0.1f;
                damageMultiplier = 1f + (strength - 10) * 0.05f;
                defenseMultiplier = 1f + (vitality - 10) * 0.05f;
                speedMultiplier = 1f + (dexterity - 10) * 0.02f;
            }
            
            public int GetAttributeValue(CharacterAttribute attribute)
            {
                return attribute switch
                {
                    CharacterAttribute.Strength => strength,
                    CharacterAttribute.Dexterity => dexterity,
                    CharacterAttribute.Intelligence => intelligence,
                    CharacterAttribute.Vitality => vitality,
                    CharacterAttribute.Wisdom => wisdom,
                    CharacterAttribute.Luck => luck,
                    _ => 0
                };
            }
            
            public void SetAttributeValue(CharacterAttribute attribute, int value)
            {
                switch (attribute)
                {
                    case CharacterAttribute.Strength: strength = value; break;
                    case CharacterAttribute.Dexterity: dexterity = value; break;
                    case CharacterAttribute.Intelligence: intelligence = value; break;
                    case CharacterAttribute.Vitality: vitality = value; break;
                    case CharacterAttribute.Wisdom: wisdom = value; break;
                    case CharacterAttribute.Luck: luck = value; break;
                }
                CalculateDerivedStats();
            }
        }

        [TabGroup("Progression", "Attributes")]
        [SerializeField] private CharacterAttributes attributes = new CharacterAttributes();

        public enum CharacterAttribute
        {
            Strength,
            Dexterity,
            Intelligence,
            Vitality,
            Wisdom,
            Luck
        }

        #endregion

        #region Properties

        public int CurrentLevel => currentLevel;
        public int CurrentExperience => currentExperience;
        public int ExperienceToNextLevel => experienceToNextLevel;
        public int AvailableSkillPoints => availableSkillPoints;
        public int AvailableAttributePoints => availableAttributePoints;
        public float ProgressToNextLevel => progressToNextLevel;
        public CharacterAttributes Attributes => attributes;
        public CharacterAttributes CurrentAttributes => attributes;
        public bool CanLevelUp => currentLevel < maxLevel && currentExperience >= GetExperienceRequiredForLevel(currentLevel + 1);

        #endregion

        #region Skill Trees

        [System.Serializable]
        public class SkillTreeNode
        {
            public string skillName;
            public string description;
            public int maxRank = 5;
            public int currentRank = 0;
            public int skillPointCost = 1;
            public List<string> prerequisites = new List<string>();
            public bool isUnlocked = false;
            
            public bool CanUpgrade => currentRank < maxRank && isUnlocked;
            public bool IsMaxRank => currentRank >= maxRank;
        }

        [TabGroup("Progression", "Skill Trees")]
        [SerializeField] private List<SkillTreeNode> combatSkills = new List<SkillTreeNode>();
        [SerializeField] private List<SkillTreeNode> magicSkills = new List<SkillTreeNode>();
        [SerializeField] private List<SkillTreeNode> utilitySkills = new List<SkillTreeNode>();

        #endregion

        #region Debug & Statistics

        [TabGroup("Progression", "Debug")]
        [Header("Debug Settings")]
        [SerializeField] private bool debugMode = false;
        [SerializeField] private bool showLevelUpEffects = true;
        
        [TabGroup("Progression", "Statistics")]
        [Header("Progression Statistics")]
        [SerializeField, ReadOnly] private int totalExperienceGained = 0;
        [SerializeField, ReadOnly] private int totalLevelsGained = 0;
        [SerializeField, ReadOnly] private int totalSkillPointsSpent = 0;
        [SerializeField, ReadOnly] private int totalAttributePointsSpent = 0;
        [SerializeField, ReadOnly] private float timeToLastLevelUp = 0f;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeProgression();
        }

        private void Start()
        {
            RegisterServices();
        }

        private void Update()
        {
            if (Application.isPlaying)
            {
                timeToLastLevelUp += Time.deltaTime;
            }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// 成長システムの初期化
        /// </summary>
        public void Initialize(ActionRPGSettings settings = null)
        {
            LogDebug("[CharacterProgression] Initializing Character Progression...");
            
            // 初期値設定
            currentLevel = startingLevel;
            currentExperience = startingExperience;
            availableSkillPoints = startingSkillPoints;
            availableAttributePoints = 0;
            
            // 属性初期化
            attributes.CalculateDerivedStats();
            
            // スキルツリー初期化
            InitializeSkillTrees();
            
            // 経験値計算
            UpdateExperienceToNextLevel();
            
            LogDebug("[CharacterProgression] Character Progression initialized");
        }

        /// <summary>
        /// 成長システムの基本初期化
        /// </summary>
        private void InitializeProgression()
        {
            // デフォルトデータ作成（設定が無い場合）
            if (progressionData == null)
            {
                CreateDefaultProgressionData();
            }
            
            if (experienceCurve == null)
            {
                CreateDefaultExperienceCurve();
            }
        }

        /// <summary>
        /// スキルツリーの初期化
        /// </summary>
        private void InitializeSkillTrees()
        {
            // 戦闘スキルツリーのサンプル初期化
            if (combatSkills.Count == 0)
            {
                combatSkills.Add(new SkillTreeNode 
                { 
                    skillName = "Power Strike", 
                    description = "Increases physical attack damage",
                    maxRank = 5,
                    skillPointCost = 1,
                    isUnlocked = true
                });
                
                combatSkills.Add(new SkillTreeNode 
                { 
                    skillName = "Combat Reflexes", 
                    description = "Increases dodge chance and attack speed",
                    maxRank = 3,
                    skillPointCost = 2,
                    prerequisites = new List<string> { "Power Strike" }
                });
            }
            
            // 魔法スキルツリーのサンプル初期化
            if (magicSkills.Count == 0)
            {
                magicSkills.Add(new SkillTreeNode 
                { 
                    skillName = "Mana Mastery", 
                    description = "Increases maximum mana",
                    maxRank = 5,
                    skillPointCost = 1,
                    isUnlocked = true
                });
                
                magicSkills.Add(new SkillTreeNode 
                { 
                    skillName = "Spell Power", 
                    description = "Increases magic damage",
                    maxRank = 5,
                    skillPointCost = 1,
                    isUnlocked = true
                });
            }
            
            // ユーティリティスキルツリーのサンプル初期化
            if (utilitySkills.Count == 0)
            {
                utilitySkills.Add(new SkillTreeNode 
                { 
                    skillName = "Treasure Hunter", 
                    description = "Increases item drop rates",
                    maxRank = 3,
                    skillPointCost = 2
                });
                
                utilitySkills.Add(new SkillTreeNode 
                { 
                    skillName = "Fast Learning", 
                    description = "Increases experience gain",
                    maxRank = 3,
                    skillPointCost = 3
                });
            }
            
            UpdateSkillAvailability();
        }

        /// <summary>
        /// サービス登録
        /// </summary>
        private void RegisterServices()
        {
            ServiceLocator.RegisterService<CharacterProgressionManager>(this);
        }

        #endregion

        #region Experience Management

        /// <summary>
        /// 経験値を追加
        /// </summary>
        public void AddExperience(int experienceAmount)
        {
            if (experienceAmount <= 0) return;
            
            int originalLevel = currentLevel;
            currentExperience += experienceAmount;
            totalExperienceGained += experienceAmount;
            
            LogDebug($"[CharacterProgression] Gained {experienceAmount} experience (Total: {currentExperience})");
            
            // レベルアップチェック
            while (CanLevelUp && currentLevel < maxLevel)
            {
                LevelUp();
            }
            
            UpdateExperienceToNextLevel();
            OnExperienceGained?.Invoke(experienceAmount);
            
            // レベルアップした場合の処理
            if (currentLevel > originalLevel)
            {
                int levelsGained = currentLevel - originalLevel;
                totalLevelsGained += levelsGained;
                timeToLastLevelUp = 0f;
            }
        }

        /// <summary>
        /// レベルアップ処理
        /// </summary>
        private void LevelUp()
        {
            currentLevel++;
            
            // スキルポイント付与
            availableSkillPoints += skillPointsPerLevel;
            
            // 属性ポイント付与
            availableAttributePoints += attributePointsPerLevel;
            
            // 自動属性振り分け
            if (autoAllocateAttributes)
            {
                AutoAllocateAttributes();
            }
            
            LogDebug($"[CharacterProgression] Level Up! New level: {currentLevel}");
            
            // スキル解放チェック
            UpdateSkillAvailability();
            
            // レベルアップイベント発行
            OnLevelUp?.Invoke(currentLevel);
            
            // レベルアップエフェクト
            if (showLevelUpEffects)
            {
                ShowLevelUpEffects();
            }
        }

        /// <summary>
        /// 次レベルまでの経験値更新
        /// </summary>
        private void UpdateExperienceToNextLevel()
        {
            if (currentLevel >= maxLevel)
            {
                experienceToNextLevel = 0;
                progressToNextLevel = 1f;
                return;
            }
            
            int requiredExp = GetExperienceRequiredForLevel(currentLevel + 1);
            experienceToNextLevel = Mathf.Max(0, requiredExp - currentExperience);
            
            int currentLevelExp = GetExperienceRequiredForLevel(currentLevel);
            int expInCurrentLevel = currentExperience - currentLevelExp;
            int expForNextLevel = requiredExp - currentLevelExp;
            
            progressToNextLevel = expForNextLevel > 0 ? (float)expInCurrentLevel / expForNextLevel : 1f;
        }

        /// <summary>
        /// 指定レベルに必要な経験値を取得
        /// </summary>
        public int GetExperienceRequiredForLevel(int level)
        {
            if (level <= 1) return 0;
            
            if (experienceCurve != null && level >= 1 && level <= experienceCurve.MaxLevel)
            {
                return experienceCurve.GetRequiredExperience(level);
            }
            
            // デフォルト計算式（指数関数的増加）
            return Mathf.RoundToInt(100 * Mathf.Pow(level - 1, 1.5f));
        }

        #endregion

        #region Attribute Management

        /// <summary>
        /// 属性ポイントを消費して属性を上げる
        /// </summary>
        public bool IncreaseAttribute(CharacterAttribute attribute, int points = 1)
        {
            if (points <= 0 || points > availableAttributePoints)
                return false;
            
            int currentValue = attributes.GetAttributeValue(attribute);
            int newValue = currentValue + points;
            
            // 属性上限チェック（レベル * 2 + 初期値を上限とする）
            int maxAttributeValue = currentLevel * 2 + 10;
            if (newValue > maxAttributeValue)
            {
                points = maxAttributeValue - currentValue;
                if (points <= 0) return false;
                newValue = maxAttributeValue;
            }
            
            // 属性値更新
            attributes.SetAttributeValue(attribute, newValue);
            availableAttributePoints -= points;
            totalAttributePointsSpent += points;
            
            LogDebug($"[CharacterProgression] Increased {attribute} by {points} (New value: {newValue})");
            
            OnAttributeIncreased?.Invoke(attribute, newValue);
            return true;
        }

        /// <summary>
        /// 自動属性振り分け
        /// </summary>
        private void AutoAllocateAttributes()
        {
            // シンプルな自動振り分け（バランス型）
            CharacterAttribute[] attributesToIncrease = 
            {
                CharacterAttribute.Strength,
                CharacterAttribute.Vitality,
                CharacterAttribute.Dexterity,
                CharacterAttribute.Intelligence
            };
            
            int pointsToAllocate = availableAttributePoints;
            for (int i = 0; i < pointsToAllocate && availableAttributePoints > 0; i++)
            {
                CharacterAttribute attr = attributesToIncrease[i % attributesToIncrease.Length];
                IncreaseAttribute(attr, 1);
            }
        }

        /// <summary>
        /// 属性リセット
        /// </summary>
        [Button("Reset Attributes"), TabGroup("Progression", "Debug")]
        public void ResetAttributes()
        {
            if (!Application.isPlaying) return;
            
            // 初期値に戻す
            attributes.strength = 10;
            attributes.dexterity = 10;
            attributes.intelligence = 10;
            attributes.vitality = 10;
            attributes.wisdom = 10;
            attributes.luck = 10;
            
            // 使用したポイントを返還
            availableAttributePoints += totalAttributePointsSpent;
            totalAttributePointsSpent = 0;
            
            attributes.CalculateDerivedStats();
            
            LogDebug("[CharacterProgression] Attributes reset to base values");
        }

        #endregion

        #region Skill Management

        /// <summary>
        /// スキルをアップグレード
        /// </summary>
        public bool UpgradeSkill(string skillName, List<SkillTreeNode> skillTree)
        {
            var skill = skillTree.Find(s => s.skillName == skillName);
            if (skill == null || !skill.CanUpgrade || availableSkillPoints < skill.skillPointCost)
                return false;
            
            skill.currentRank++;
            availableSkillPoints -= skill.skillPointCost;
            totalSkillPointsSpent += skill.skillPointCost;
            
            LogDebug($"[CharacterProgression] Upgraded skill '{skillName}' to rank {skill.currentRank}");
            
            UpdateSkillAvailability();
            return true;
        }

        /// <summary>
        /// スキルアップグレード（カテゴリ別）
        /// </summary>
        public bool UpgradeCombatSkill(string skillName) => UpgradeSkill(skillName, combatSkills);
        public bool UpgradeMagicSkill(string skillName) => UpgradeSkill(skillName, magicSkills);
        public bool UpgradeUtilitySkill(string skillName) => UpgradeSkill(skillName, utilitySkills);

        /// <summary>
        /// スキル解放状況更新
        /// </summary>
        private void UpdateSkillAvailability()
        {
            UpdateSkillTreeAvailability(combatSkills);
            UpdateSkillTreeAvailability(magicSkills);
            UpdateSkillTreeAvailability(utilitySkills);
        }

        /// <summary>
        /// 指定スキルツリーの解放状況更新
        /// </summary>
        private void UpdateSkillTreeAvailability(List<SkillTreeNode> skillTree)
        {
            foreach (var skill in skillTree)
            {
                if (skill.isUnlocked) continue;
                
                // 前提条件チェック
                bool canUnlock = true;
                foreach (string prerequisite in skill.prerequisites)
                {
                    var prereqSkill = skillTree.Find(s => s.skillName == prerequisite);
                    if (prereqSkill == null || prereqSkill.currentRank == 0)
                    {
                        canUnlock = false;
                        break;
                    }
                }
                
                skill.isUnlocked = canUnlock;
            }
        }

        /// <summary>
        /// スキルポイント追加（デバッグ用）
        /// </summary>
        [Button("Add Skill Points"), TabGroup("Progression", "Debug")]
        public void AddSkillPoints(int amount = 5)
        {
            if (!Application.isPlaying) return;
            
            availableSkillPoints += amount;
            OnSkillPointsGained?.Invoke(amount);
            LogDebug($"[CharacterProgression] Added {amount} skill points (Total: {availableSkillPoints})");
        }

        #endregion

        #region Progression Update

        /// <summary>
        /// 成長システムの更新
        /// </summary>
        public void UpdateProgression(float deltaTime)
        {
            // 成長システムの定期更新処理があればここに追加
        }

        #endregion

        #region Visual Effects

        /// <summary>
        /// レベルアップエフェクト表示
        /// </summary>
        private void ShowLevelUpEffects()
        {
            // パーティクルエフェクトや音響効果の再生
            // 実装時には適切なエフェクトシステムを統合
            LogDebug("[CharacterProgression] Showing level up effects");
        }

        #endregion

        #region Default Data Creation

        /// <summary>
        /// デフォルト成長データ作成
        /// </summary>
        private void CreateDefaultProgressionData()
        {
            // ScriptableObject作成はランタイムでは推奨されないため、
            // 実際の実装では事前にアセットを作成しておく
            LogDebug("[CharacterProgression] Using default progression settings");
        }

        /// <summary>
        /// デフォルト経験値カーブ作成
        /// </summary>
        private void CreateDefaultExperienceCurve()
        {
            LogDebug("[CharacterProgression] Using default experience curve");
        }

        #endregion

        #region Debug Support

        /// <summary>
        /// デバッグログ出力
        /// </summary>
        private void LogDebug(string message)
        {
            if (debugMode)
            {
                Debug.Log(message);
            }
        }

        /// <summary>
        /// レベルアップテスト
        /// </summary>
        [Button("Test Level Up"), TabGroup("Progression", "Debug")]
        private void TestLevelUp()
        {
            if (!Application.isPlaying) return;
            
            int expNeeded = GetExperienceRequiredForLevel(currentLevel + 1) - currentExperience;
            AddExperience(expNeeded);
        }

        /// <summary>
        /// 経験値追加テスト
        /// </summary>
        [Button("Add Experience"), TabGroup("Progression", "Debug")]
        private void TestAddExperience()
        {
            if (!Application.isPlaying) return;
            
            AddExperience(100);
        }

        /// <summary>
        /// 統計情報表示
        /// </summary>
        [Button("Show Statistics"), TabGroup("Progression", "Debug")]
        private void ShowStatistics()
        {
            if (!Application.isPlaying) return;
            
            string stats = "=== Character Progression Statistics ===\n";
            stats += $"Current Level: {currentLevel}\n";
            stats += $"Current Experience: {currentExperience}\n";
            stats += $"Experience to Next Level: {experienceToNextLevel}\n";
            stats += $"Progress to Next Level: {progressToNextLevel:P}\n";
            stats += $"Available Skill Points: {availableSkillPoints}\n";
            stats += $"Available Attribute Points: {availableAttributePoints}\n";
            stats += $"Total Experience Gained: {totalExperienceGained}\n";
            stats += $"Total Levels Gained: {totalLevelsGained}\n";
            stats += $"Total Skill Points Spent: {totalSkillPointsSpent}\n";
            stats += $"Total Attribute Points Spent: {totalAttributePointsSpent}\n";
            stats += $"Time Since Last Level Up: {timeToLastLevelUp:F2}s\n";
            
            Debug.Log(stats);
        }

        #endregion

        #region Public API

        /// <summary>
        /// 指定スキルの現在ランクを取得
        /// </summary>
        public int GetSkillRank(string skillName)
        {
            var skill = combatSkills.Find(s => s.skillName == skillName) ??
                       magicSkills.Find(s => s.skillName == skillName) ??
                       utilitySkills.Find(s => s.skillName == skillName);
            
            return skill?.currentRank ?? 0;
        }

        /// <summary>
        /// 指定スキルが解放されているかチェック
        /// </summary>
        public bool IsSkillUnlocked(string skillName)
        {
            var skill = combatSkills.Find(s => s.skillName == skillName) ??
                       magicSkills.Find(s => s.skillName == skillName) ??
                       utilitySkills.Find(s => s.skillName == skillName);
            
            return skill?.isUnlocked ?? false;
        }

        /// <summary>
        /// レベルアップ可能かチェック
        /// </summary>
        public bool CanLevelUpTo(int targetLevel)
        {
            return targetLevel <= maxLevel && currentExperience >= GetExperienceRequiredForLevel(targetLevel);
        }

        /// <summary>
        /// 装備変更時のコールバック（装備によるステータス変更を処理）
        /// </summary>
        public void OnEquipmentChanged()
        {
            // 装備変更によるステータス再計算
            // ここで装備によるステータスボーナスを考慮した最終ステータス計算を行う
            if (debugMode)
            {
                Debug.Log($"[CharacterProgression] Equipment changed - recalculating total stats");
            }

            // TODO: Add GameEvent for stats changes if needed
            // 装備変更イベントを発行
            // if (onStatsChanged != null)
            // {
            //     onStatsChanged.Raise();
            // }
        }

        #endregion
    }
}