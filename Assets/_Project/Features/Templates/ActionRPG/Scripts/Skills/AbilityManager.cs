using System;
using System.Collections.Generic;
using UnityEngine;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Features.Templates.ActionRPG.Data;

namespace asterivo.Unity60.Features.Templates.ActionRPG.Skills
{
    /// <summary>
    /// ActionRPGテンプレート用アビリティ管理
    /// 特殊能力、魔法、スキルコンボ管理
    /// </summary>
    public class AbilityManager : MonoBehaviour
    {
        [Header("アビリティ設定")]
        [SerializeField] private float globalCooldownDuration = 1.0f;
        [SerializeField] private int maxActiveAbilities = 10;
        [SerializeField] private bool enableAbilityChaining = true;
        
        [Header("イベント")]
        [SerializeField] private StringGameEvent onAbilityActivated;
        [SerializeField] private StringGameEvent onAbilityCompleted;
        [SerializeField] private StringGameEvent onAbilityFailed;
        
        // アビリティ管理
        public Dictionary<string, AbilityData> AvailableAbilities { get; private set; } = new();
        public Dictionary<string, float> AbilityCooldowns { get; private set; } = new();
        public List<ActiveAbility> ActiveAbilities { get; private set; } = new();
        
        // グローバルクールダウン
        public float GlobalCooldownRemaining { get; private set; }
        public bool IsGlobalCooldownActive => GlobalCooldownRemaining > 0;
        
        private void Start()
        {
            InitializeAbilities();
        }
        
        private void Update()
        {
            UpdateCooldowns();
            UpdateActiveAbilities();
        }
        
        /// <summary>
        /// アビリティシステム初期化
        /// </summary>
        private void InitializeAbilities()
        {
            AvailableAbilities.Clear();
            AbilityCooldowns.Clear();
            ActiveAbilities.Clear();
            
            // 基本アビリティの登録
            RegisterBasicAbilities();
            
            Debug.Log("[AbilityManager] アビリティシステム初期化完了");
        }

        /// <summary>
        /// 外部からのアビリティマネージャー初期化
        /// </summary>
        public void Initialize()
        {
            InitializeAbilities();
            Debug.Log("[AbilityManager] External initialization completed");
        }

        /// <summary>
        /// 基本アビリティ登録
        /// </summary>
        private void RegisterBasicAbilities()
        {
            // 攻撃系
            RegisterAbility(new AbilityData("PowerStrike", "パワーストライク", AbilityType.Attack, 5.0f, 20));
            RegisterAbility(new AbilityData("FireBall", "ファイアボール", AbilityType.Magic, 8.0f, 30));
            RegisterAbility(new AbilityData("Lightning", "ライトニング", AbilityType.Magic, 12.0f, 50));
            
            // 回復系
            RegisterAbility(new AbilityData("Heal", "ヒール", AbilityType.Healing, 6.0f, 25));
            RegisterAbility(new AbilityData("Regeneration", "リジェネレーション", AbilityType.Buff, 20.0f, 40));
            
            // バフ系
            RegisterAbility(new AbilityData("Haste", "ヘイスト", AbilityType.Buff, 15.0f, 35));
            RegisterAbility(new AbilityData("Shield", "シールド", AbilityType.Buff, 10.0f, 30));
        }
        
        /// <summary>
        /// アビリティ登録
        /// </summary>
        public void RegisterAbility(AbilityData ability)
        {
            if (ability == null || string.IsNullOrEmpty(ability.Id)) return;
            
            AvailableAbilities[ability.Id] = ability;
            AbilityCooldowns[ability.Id] = 0f;
            
            Debug.Log($"[AbilityManager] アビリティ登録: {ability.Name}");
        }
        
        /// <summary>
        /// アビリティ発動
        /// </summary>
        public bool TryActivateAbility(string abilityId)
        {
            // 基本チェック
            if (string.IsNullOrEmpty(abilityId) || !AvailableAbilities.ContainsKey(abilityId))
                return false;
                
            if (IsGlobalCooldownActive)
                return false;
                
            if (AbilityCooldowns[abilityId] > 0)
                return false;
                
            var ability = AvailableAbilities[abilityId];
            
            // マナコスト確認
            var statsManager = GetComponent<CharacterStatsManager>();
            if (statsManager != null && !statsManager.TryConsumeMana(ability.ManaCost))
            {
                onAbilityFailed?.Raise($"{ability.Name}: Not enough mana");
                return false;
            }
            
            // アビリティ実行
            ExecuteAbility(ability);
            
            // クールダウン設定
            AbilityCooldowns[abilityId] = ability.Cooldown;
            GlobalCooldownRemaining = globalCooldownDuration;
            
            onAbilityActivated?.Raise(ability.Name);
            
            Debug.Log($"[AbilityManager] アビリティ発動: {ability.Name}");
            
            return true;
        }
        
        /// <summary>
        /// アビリティ実行
        /// </summary>
        private void ExecuteAbility(AbilityData ability)
        {
            switch (ability.Type)
            {
                case AbilityType.Attack:
                    ExecuteAttackAbility(ability);
                    break;
                    
                case AbilityType.Magic:
                    ExecuteMagicAbility(ability);
                    break;
                    
                case AbilityType.Healing:
                    ExecuteHealingAbility(ability);
                    break;
                    
                case AbilityType.Buff:
                    ExecuteBuffAbility(ability);
                    break;
                    
                case AbilityType.Debuff:
                    ExecuteDebuffAbility(ability);
                    break;
            }
        }
        
        /// <summary>
        /// 攻撃アビリティ実行
        /// </summary>
        private void ExecuteAttackAbility(AbilityData ability)
        {
            // 基本攻撃ダメージ計算
            var statsManager = GetComponent<CharacterStatsManager>();
            float baseDamage = statsManager?.AttackPower ?? 10f;
            
            float abilityDamage = baseDamage * GetAbilityMultiplier(ability.Id);
            
            // ダメージエフェクト（実際の実装では敵への適用）
            Debug.Log($"[AbilityManager] 攻撃アビリティ: {ability.Name} ダメージ: {abilityDamage}");
        }
        
        /// <summary>
        /// 魔法アビリティ実行
        /// </summary>
        private void ExecuteMagicAbility(AbilityData ability)
        {
            var statsManager = GetComponent<CharacterStatsManager>();
            float magicPower = statsManager?.MagicPower ?? 8f;
            
            float magicDamage = magicPower * GetAbilityMultiplier(ability.Id);
            
            Debug.Log($"[AbilityManager] 魔法アビリティ: {ability.Name} 魔法ダメージ: {magicDamage}");
        }
        
        /// <summary>
        /// 回復アビリティ実行
        /// </summary>
        private void ExecuteHealingAbility(AbilityData ability)
        {
            var statsManager = GetComponent<CharacterStatsManager>();
            
            float healAmount = 50f * GetAbilityMultiplier(ability.Id);
            statsManager?.RestoreHealth(healAmount);
            
            Debug.Log($"[AbilityManager] 回復アビリティ: {ability.Name} 回復量: {healAmount}");
        }
        
        /// <summary>
        /// バフアビリティ実行
        /// </summary>
        private void ExecuteBuffAbility(AbilityData ability)
        {
            var activeAbility = new ActiveAbility(ability, 30f); // 30秒間持続
            ActiveAbilities.Add(activeAbility);
            
            Debug.Log($"[AbilityManager] バフアビリティ: {ability.Name} 効果時間: {activeAbility.Duration}秒");
        }
        
        /// <summary>
        /// デバフアビリティ実行
        /// </summary>
        private void ExecuteDebuffAbility(AbilityData ability)
        {
            Debug.Log($"[AbilityManager] デバフアビリティ: {ability.Name}");
        }
        
        /// <summary>
        /// アビリティ倍率取得
        /// </summary>
        private float GetAbilityMultiplier(string abilityId)
        {
            return abilityId switch
            {
                "PowerStrike" => 2.0f,
                "FireBall" => 1.8f,
                "Lightning" => 2.5f,
                "Heal" => 1.5f,
                _ => 1.0f
            };
        }
        
        /// <summary>
        /// クールダウン更新
        /// </summary>
        private void UpdateCooldowns()
        {
            // グローバルクールダウン
            if (GlobalCooldownRemaining > 0)
            {
                GlobalCooldownRemaining -= Time.deltaTime;
                if (GlobalCooldownRemaining < 0)
                    GlobalCooldownRemaining = 0;
            }
            
            // 個別クールダウン
            var abilityIds = new List<string>(AbilityCooldowns.Keys);
            foreach (var abilityId in abilityIds)
            {
                if (AbilityCooldowns[abilityId] > 0)
                {
                    AbilityCooldowns[abilityId] -= Time.deltaTime;
                    if (AbilityCooldowns[abilityId] < 0)
                        AbilityCooldowns[abilityId] = 0;
                }
            }
        }
        
        /// <summary>
        /// アクティブアビリティ更新
        /// </summary>
        private void UpdateActiveAbilities()
        {
            for (int i = ActiveAbilities.Count - 1; i >= 0; i--)
            {
                var activeAbility = ActiveAbilities[i];
                activeAbility.TimeRemaining -= Time.deltaTime;
                
                if (activeAbility.TimeRemaining <= 0)
                {
                    onAbilityCompleted?.Raise(activeAbility.AbilityData.Name);
                    ActiveAbilities.RemoveAt(i);
                }
            }
        }
        
        /// <summary>
        /// アビリティ使用可能チェック
        /// </summary>
        public bool CanUseAbility(string abilityId)
        {
            if (!AvailableAbilities.ContainsKey(abilityId))
                return false;
                
            if (IsGlobalCooldownActive)
                return false;
                
            if (AbilityCooldowns[abilityId] > 0)
                return false;
                
            return true;
        }
    }
    
    /// <summary>
    /// アビリティデータ
    /// </summary>
    [System.Serializable]
    public class AbilityData
    {
        public string Id;
        public string Name;
        public AbilityType Type;
        public float Cooldown;
        public int ManaCost;
        public string Description;
        
        public AbilityData(string id, string name, AbilityType type, float cooldown, int manaCost)
        {
            Id = id;
            Name = name;
            Type = type;
            Cooldown = cooldown;
            ManaCost = manaCost;
            Description = "";
        }
    }
    
    /// <summary>
    /// アクティブアビリティ
    /// </summary>
    public class ActiveAbility
    {
        public AbilityData AbilityData;
        public float Duration;
        public float TimeRemaining;
        
        public ActiveAbility(AbilityData abilityData, float duration)
        {
            AbilityData = abilityData;
            Duration = duration;
            TimeRemaining = duration;
        }
    }
    
    /// <summary>
    /// アビリティ種別
    /// </summary>
    public enum AbilityType
    {
        Attack,     // 攻撃
        Magic,      // 魔法
        Healing,    // 回復
        Buff,       // バフ
        Debuff,     // デバフ
        Utility     // ユーティリティ
    }
}