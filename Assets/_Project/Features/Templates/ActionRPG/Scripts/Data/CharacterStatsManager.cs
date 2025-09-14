using System;
using UnityEngine;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Features.Templates.ActionRPG.Data;

namespace asterivo.Unity60.Features.Templates.ActionRPG.Data
{
    /// <summary>
    /// ActionRPGテンプレート用キャラクターステータス管理
    /// レベル、属性値、派生ステータス管理
    /// </summary>
    public class CharacterStatsManager : MonoBehaviour
    {
        [Header("キャラクター進行設定")]
        [SerializeField] private CharacterProgressionData progressionData;
        [SerializeField] private ExperienceCurveData experienceCurve;
        
        [Header("イベント")]
        [SerializeField] private IntGameEvent onLevelUp;
        [SerializeField] private IntGameEvent onExperienceGained;
        [SerializeField] private StringGameEvent onStatChanged;
        [SerializeField] private FloatGameEvent onHealthChanged;
        [SerializeField] private FloatGameEvent onManaChanged;
        
        // 現在のステータス
        public CharacterProgressionData Data => progressionData;
        public ExperienceCurveData Curve => experienceCurve;
        
        // 派生ステータス（キャッシュ）
        private float _currentHealth;
        private float _currentMana;
        private float _currentStamina;
        
        // プロパティ
        public float CurrentHealth 
        { 
            get => _currentHealth; 
            set 
            { 
                _currentHealth = Mathf.Clamp(value, 0, MaxHealth);
                onHealthChanged?.Raise(_currentHealth);
            }
        }
        
        public float CurrentMana 
        { 
            get => _currentMana; 
            set 
            { 
                _currentMana = Mathf.Clamp(value, 0, MaxMana);
                onManaChanged?.Raise(_currentMana);
            }
        }
        
        public float MaxHealth => progressionData?.Health ?? 100f;
        public float MaxMana => progressionData?.Mana ?? 50f;
        public float AttackPower => progressionData?.AttackPower ?? 10f;
        public float Defense => progressionData?.Defense ?? 5f;
        public float CriticalChance => progressionData?.CriticalChance ?? 5f;
        public float MagicPower => progressionData?.MagicPower ?? 8f;
        
        private void Start()
        {
            InitializeStats();
        }
        
        private void Update()
        {
            // 自然回復処理
            ProcessNaturalRegeneration();
        }
        
        /// <summary>
        /// ステータス初期化
        /// </summary>
        private void InitializeStats()
        {
            if (progressionData == null)
            {
                Debug.LogError("[CharacterStatsManager] CharacterProgressionData が設定されていません");
                return;
            }
            
            // 初期値設定
            _currentHealth = MaxHealth;
            _currentMana = MaxMana;
            _currentStamina = 100f;
            
            Debug.Log($"[CharacterStatsManager] ステータス初期化: Lv.{progressionData.Level} HP:{MaxHealth} MP:{MaxMana}");
        }
        
        /// <summary>
        /// 経験値獲得
        /// </summary>
        public void GainExperience(int amount)
        {
            if (progressionData == null || experienceCurve == null) return;
            
            int oldLevel = progressionData.Level;
            progressionData.AddExperience(amount);
            
            onExperienceGained?.Raise(amount);
            
            // レベルアップチェック
            CheckLevelUp(oldLevel);
        }
        
        /// <summary>
        /// レベルアップチェック
        /// </summary>
        private void CheckLevelUp(int previousLevel)
        {
            if (progressionData == null || experienceCurve == null) return;
            
            int requiredExp = experienceCurve.GetRequiredExperience(progressionData.Level + 1);
            
            while (progressionData.Experience >= requiredExp && progressionData.Level < experienceCurve.MaxLevel)
            {
                if (progressionData.TryLevelUp(requiredExp))
                {
                    OnLevelUp();
                    requiredExp = experienceCurve.GetRequiredExperience(progressionData.Level + 1);
                }
                else
                {
                    break;
                }
            }
        }
        
        /// <summary>
        /// レベルアップ処理
        /// </summary>
        private void OnLevelUp()
        {
            // ヘルス・マナを全回復
            float healthPercent = _currentHealth / MaxHealth;
            float manaPercent = _currentMana / MaxMana;
            
            _currentHealth = MaxHealth;
            _currentMana = MaxMana;
            
            onLevelUp?.Raise(progressionData.Level);
            onHealthChanged?.Raise(_currentHealth);
            onManaChanged?.Raise(_currentMana);
            
            Debug.Log($"[CharacterStatsManager] レベルアップ! Lv.{progressionData.Level}");
        }
        
        /// <summary>
        /// 属性値アップ
        /// </summary>
        public bool TryIncreaseAttribute(string attributeName, int cost = 1)
        {
            if (progressionData == null) return false;
            
            bool success = progressionData.TryIncreaseAttribute(attributeName, cost);
            
            if (success)
            {
                // 派生ステータスの更新
                RefreshDerivedStats();
                onStatChanged?.Raise(attributeName);
                
                Debug.Log($"[CharacterStatsManager] {attributeName} がアップしました");
            }
            
            return success;
        }
        
        /// <summary>
        /// 派生ステータス更新
        /// </summary>
        private void RefreshDerivedStats()
        {
            // ヘルス・マナの最大値が変わった場合の処理
            float healthRatio = _currentHealth / MaxHealth;
            float manaRatio = _currentMana / MaxMana;
            
            // 比率を維持して更新
            _currentHealth = MaxHealth * healthRatio;
            _currentMana = MaxMana * manaRatio;
            
            onHealthChanged?.Raise(_currentHealth);
            onManaChanged?.Raise(_currentMana);
        }
        
        /// <summary>
        /// ダメージ処理
        /// </summary>
        public void TakeDamage(float damage)
        {
            float actualDamage = Mathf.Max(0, damage - Defense);
            CurrentHealth -= actualDamage;
            
            Debug.Log($"[CharacterStatsManager] {actualDamage} ダメージを受けました (残りHP: {CurrentHealth})");
            
            if (CurrentHealth <= 0)
            {
                OnCharacterDeath();
            }
        }
        
        /// <summary>
        /// 回復処理
        /// </summary>
        public void RestoreHealth(float amount)
        {
            CurrentHealth += amount;
            Debug.Log($"[CharacterStatsManager] {amount} 回復しました (現在HP: {CurrentHealth})");
        }
        
        /// <summary>
        /// マナ消費
        /// </summary>
        public bool TryConsumeMana(float amount)
        {
            if (_currentMana < amount) return false;
            
            CurrentMana -= amount;
            return true;
        }
        
        /// <summary>
        /// 自然回復処理
        /// </summary>
        private void ProcessNaturalRegeneration()
        {
            float regenRate = Time.deltaTime * 2f; // 秒間2ポイント回復
            
            // マナ自然回復
            if (_currentMana < MaxMana)
            {
                CurrentMana += regenRate;
            }
            
            // ヘルス微量回復（戦闘外のみ）
            if (_currentHealth < MaxHealth && _currentHealth > 0)
            {
                float healthRegen = regenRate * 0.5f; // マナより遅い回復
                CurrentHealth += healthRegen;
            }
        }
        
        /// <summary>
        /// キャラクター死亡処理
        /// </summary>
        private void OnCharacterDeath()
        {
            Debug.Log("[CharacterStatsManager] キャラクターが死亡しました");
            
            if (progressionData != null)
            {
                progressionData.RecordDeath();
            }
            
            // 死亡イベントを発行（別途 GameEvent で実装予定）
            // onCharacterDeath?.Raise();
        }
        
        /// <summary>
        /// キル記録
        /// </summary>
        public void RecordKill()
        {
            progressionData?.RecordKill();
        }
        
        /// <summary>
        /// クエスト完了記録
        /// </summary>
        public void RecordQuestCompletion()
        {
            progressionData?.RecordQuestCompletion();
        }
    }
}