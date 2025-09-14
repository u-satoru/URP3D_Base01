using System;
using System.Collections.Generic;
using UnityEngine;
using asterivo.Unity60.Core.Events;

namespace asterivo.Unity60.Features.Templates.ActionRPG.Skills
{
    /// <summary>
    /// ActionRPGテンプレート用スキルマネージャー
    /// スキルツリー、アクティブ・パッシブスキル管理
    /// </summary>
    public class SkillManager : MonoBehaviour
    {
        [Header("スキル設定")]
        [SerializeField] private int maxSkillPoints = 100;
        [SerializeField] private float skillCooldownMultiplier = 1.0f;
        [SerializeField] private bool enableSkillCombos = true;
        
        [Header("イベント")]
        [SerializeField] private StringGameEvent onSkillLearned;
        [SerializeField] private StringGameEvent onSkillActivated;
        [SerializeField] private IntGameEvent onSkillPointsChanged;
        
        // プロパティ
        public int SkillPoints { get; private set; }
        public int MaxSkillPoints => maxSkillPoints;
        public Dictionary<string, SkillData> LearnedSkills { get; private set; } = new();
        public Dictionary<string, float> SkillCooldowns { get; private set; } = new();
        
        private void Start()
        {
            SkillPoints = 0;
            InitializeSkillSystem();
        }
        
        private void Update()
        {
            UpdateSkillCooldowns();
        }
        
        /// <summary>
        /// スキルシステムの初期化
        /// </summary>
        private void InitializeSkillSystem()
        {
            // 基本スキルの設定
            LearnedSkills.Clear();
            SkillCooldowns.Clear();
            
            // デフォルトスキルの追加
            var basicAttack = new SkillData("BasicAttack", "基本攻撃", 0, 1.0f);
            LearnedSkills.Add(basicAttack.Id, basicAttack);
            
            Debug.Log("[SkillManager] スキルシステム初期化完了");
        }
        
        /// <summary>
        /// スキル習得
        /// </summary>
        public bool TryLearnSkill(string skillId, int cost = 1)
        {
            if (SkillPoints < cost || LearnedSkills.ContainsKey(skillId))
                return false;
                
            var skillData = GetSkillData(skillId);
            if (skillData == null) return false;
            
            LearnedSkills.Add(skillId, skillData);
            SkillPoints -= cost;
            
            onSkillLearned?.Raise(skillId);
            onSkillPointsChanged?.Raise(SkillPoints);
            
            return true;
        }
        
        /// <summary>
        /// スキル発動
        /// </summary>
        public bool TryActivateSkill(string skillId)
        {
            if (!LearnedSkills.ContainsKey(skillId))
                return false;
                
            if (SkillCooldowns.ContainsKey(skillId) && SkillCooldowns[skillId] > 0)
                return false;
                
            var skill = LearnedSkills[skillId];
            SkillCooldowns[skillId] = skill.Cooldown * skillCooldownMultiplier;
            
            onSkillActivated?.Raise(skillId);
            
            return true;
        }
        
        /// <summary>
        /// スキルポイント追加
        /// </summary>
        public void AddSkillPoints(int points)
        {
            SkillPoints = Mathf.Clamp(SkillPoints + points, 0, maxSkillPoints);
            onSkillPointsChanged?.Raise(SkillPoints);
        }
        
        /// <summary>
        /// スキルクールダウン更新
        /// </summary>
        private void UpdateSkillCooldowns()
        {
            var skillsToUpdate = new List<string>(SkillCooldowns.Keys);
            
            foreach (var skillId in skillsToUpdate)
            {
                if (SkillCooldowns[skillId] > 0)
                {
                    SkillCooldowns[skillId] -= Time.deltaTime;
                    if (SkillCooldowns[skillId] <= 0)
                        SkillCooldowns[skillId] = 0;
                }
            }
        }
        
        /// <summary>
        /// スキルデータ取得
        /// </summary>
        private SkillData GetSkillData(string skillId)
        {
            // 実際のプロジェクトではScriptableObjectから読み込む
            return skillId switch
            {
                "FireBall" => new SkillData("FireBall", "火球術", 3.0f, 10),
                "Heal" => new SkillData("Heal", "回復", 5.0f, 5),
                "Shield" => new SkillData("Shield", "防御", 8.0f, 15),
                _ => null
            };
        }
    }
    
    /// <summary>
    /// スキルデータクラス
    /// </summary>
    [System.Serializable]
    public class SkillData
    {
        public string Id;
        public string Name;
        public float Cooldown;
        public int ManaCost;
        
        public SkillData(string id, string name, float cooldown, int manaCost = 0)
        {
            Id = id;
            Name = name;
            Cooldown = cooldown;
            ManaCost = manaCost;
        }
    }
}