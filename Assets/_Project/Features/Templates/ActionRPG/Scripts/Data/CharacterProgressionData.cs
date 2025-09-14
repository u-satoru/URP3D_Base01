using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Features.Templates.ActionRPG.Data
{
    /// <summary>
    /// キャラクター成長データ
    /// レベル、ステータス、スキル進行を管理
    /// </summary>
    [CreateAssetMenu(fileName = "CharacterProgressionData", menuName = "asterivo.Unity60/Templates/ActionRPG/Character Progression")]
    public class CharacterProgressionData : ScriptableObject
    {
        [TabGroup("Basic", "Stats")]
        [Header("基本ステータス")]
        [SerializeField] private int level = 1;
        [SerializeField] private int experience = 0;
        [SerializeField] private int skillPoints = 0;
        [SerializeField] private int attributePoints = 0;

        [TabGroup("Basic", "Attributes")]
        [Header("属性値")]
        [SerializeField] private int strength = 10;
        [SerializeField] private int dexterity = 10;
        [SerializeField] private int intelligence = 10;
        [SerializeField] private int vitality = 10;
        [SerializeField] private int wisdom = 10;
        [SerializeField] private int luck = 10;

        [TabGroup("Advanced", "Derived Stats")]
        [Header("派生ステータス")]
        [ShowInInspector, ReadOnly]
        public float Health => vitality * 10 + level * 5;

        [ShowInInspector, ReadOnly]
        public float Mana => intelligence * 8 + wisdom * 6 + level * 3;

        [ShowInInspector, ReadOnly]
        public float AttackPower => strength * 2 + dexterity * 1.5f + level;

        [ShowInInspector, ReadOnly]
        public float Defense => vitality * 1.5f + strength * 0.5f;

        [ShowInInspector, ReadOnly]
        public float CriticalChance => dexterity * 0.1f + luck * 0.15f;

        [ShowInInspector, ReadOnly]
        public float MagicPower => intelligence * 2.5f + wisdom * 1.8f;

        [TabGroup("Advanced", "Skills")]
        [Header("習得スキル")]
        [SerializeField] private List<string> learnedSkillIds = new List<string>();
        [SerializeField] private Dictionary<string, int> skillLevels = new Dictionary<string, int>();

        [TabGroup("Advanced", "History")]
        [Header("進行履歴")]
        [SerializeField, ReadOnly] private DateTime creationTime;
        [SerializeField, ReadOnly] private float totalPlayTime = 0f;
        [SerializeField, ReadOnly] private int totalKills = 0;
        [SerializeField, ReadOnly] private int totalDeaths = 0;
        [SerializeField, ReadOnly] private int totalQuestsCompleted = 0;

        // プロパティ
        public int Level
        {
            get => level;
            set => level = Mathf.Clamp(value, 1, int.MaxValue);
        }

        public int Experience
        {
            get => experience;
            set => experience = Mathf.Max(0, value);
        }

        public int SkillPoints
        {
            get => skillPoints;
            set => skillPoints = Mathf.Max(0, value);
        }

        public int AttributePoints
        {
            get => attributePoints;
            set => attributePoints = Mathf.Max(0, value);
        }

        public int Strength
        {
            get => strength;
            set => strength = Mathf.Clamp(value, 1, 999);
        }

        public int Dexterity
        {
            get => dexterity;
            set => dexterity = Mathf.Clamp(value, 1, 999);
        }

        public int Intelligence
        {
            get => intelligence;
            set => intelligence = Mathf.Clamp(value, 1, 999);
        }

        public int Vitality
        {
            get => vitality;
            set => vitality = Mathf.Clamp(value, 1, 999);
        }

        public int Wisdom
        {
            get => wisdom;
            set => wisdom = Mathf.Clamp(value, 1, 999);
        }

        public int Luck
        {
            get => luck;
            set => luck = Mathf.Clamp(value, 1, 999);
        }

        public IReadOnlyList<string> LearnedSkills => learnedSkillIds.AsReadOnly();
        public IReadOnlyDictionary<string, int> SkillLevels => skillLevels;

        public DateTime CreationTime => creationTime;
        public float TotalPlayTime => totalPlayTime;
        public int TotalKills => totalKills;
        public int TotalDeaths => totalDeaths;
        public int TotalQuestsCompleted => totalQuestsCompleted;

        /// <summary>
        /// 初期化
        /// </summary>
        private void Awake()
        {
            if (creationTime == default)
            {
                creationTime = DateTime.Now;
            }
        }

        /// <summary>
        /// レベルアップ処理
        /// </summary>
        public bool TryLevelUp(int requiredExperience)
        {
            if (experience >= requiredExperience)
            {
                level++;
                experience -= requiredExperience;

                // レベルアップ報酬
                skillPoints += 2;
                attributePoints += 5;

                return true;
            }
            return false;
        }

        /// <summary>
        /// 経験値獲得
        /// </summary>
        public void AddExperience(int amount)
        {
            experience = Mathf.Max(0, experience + amount);
        }

        /// <summary>
        /// 属性値を上昇
        /// </summary>
        public bool TryIncreaseAttribute(string attribute, int cost = 1)
        {
            if (attributePoints < cost) return false;

            bool success = attribute.ToLower() switch
            {
                "strength" => TryIncreaseStrength(),
                "dexterity" => TryIncreaseDexterity(),
                "intelligence" => TryIncreaseIntelligence(),
                "vitality" => TryIncreaseVitality(),
                "wisdom" => TryIncreaseWisdom(),
                "luck" => TryIncreaseLuck(),
                _ => false
            };

            if (success)
            {
                attributePoints -= cost;
            }

            return success;
        }

        /// <summary>
        /// スキル習得
        /// </summary>
        public bool TryLearnSkill(string skillId, int cost = 1)
        {
            if (skillPoints < cost || learnedSkillIds.Contains(skillId))
                return false;

            learnedSkillIds.Add(skillId);
            skillLevels[skillId] = 1;
            skillPoints -= cost;

            return true;
        }

        /// <summary>
        /// スキルレベル上昇
        /// </summary>
        public bool TryUpgradeSkill(string skillId, int cost = 1)
        {
            if (skillPoints < cost || !skillLevels.ContainsKey(skillId))
                return false;

            skillLevels[skillId]++;
            skillPoints -= cost;

            return true;
        }

        /// <summary>
        /// 統計更新
        /// </summary>
        public void UpdatePlayTime(float deltaTime)
        {
            totalPlayTime += deltaTime;
        }

        public void RecordKill() => totalKills++;
        public void RecordDeath() => totalDeaths++;
        public void RecordQuestCompletion() => totalQuestsCompleted++;

        // 属性値上昇メソッド
        private bool TryIncreaseStrength() { if (strength < 999) { strength++; return true; } return false; }
        private bool TryIncreaseDexterity() { if (dexterity < 999) { dexterity++; return true; } return false; }
        private bool TryIncreaseIntelligence() { if (intelligence < 999) { intelligence++; return true; } return false; }
        private bool TryIncreaseVitality() { if (vitality < 999) { vitality++; return true; } return false; }
        private bool TryIncreaseWisdom() { if (wisdom < 999) { wisdom++; return true; } return false; }
        private bool TryIncreaseLuck() { if (luck < 999) { luck++; return true; } return false; }

        /// <summary>
        /// データリセット
        /// </summary>
        [Button("Reset Character")]
        [ShowIf("@Application.isEditor")]
        private void ResetCharacter()
        {
            level = 1;
            experience = 0;
            skillPoints = 0;
            attributePoints = 0;

            strength = dexterity = intelligence = vitality = wisdom = luck = 10;

            learnedSkillIds.Clear();
            skillLevels.Clear();

            totalPlayTime = 0f;
            totalKills = totalDeaths = totalQuestsCompleted = 0;

            Debug.Log("[CharacterProgressionData] Character reset to default values");
        }

        /// <summary>
        /// デバッグ情報表示
        /// </summary>
        [Button("Show Character Info")]
        [ShowIf("@Application.isEditor")]
        private void ShowCharacterInfo()
        {
            Debug.Log($"[Character] Lv.{level} | HP:{Health:F0} MP:{Mana:F0} | ATK:{AttackPower:F1} DEF:{Defense:F1}");
            Debug.Log($"[Stats] STR:{strength} DEX:{dexterity} INT:{intelligence} VIT:{vitality} WIS:{wisdom} LUK:{luck}");
            Debug.Log($"[Progress] EXP:{experience} | SP:{skillPoints} AP:{attributePoints} | Skills:{learnedSkillIds.Count}");
        }
    }
}