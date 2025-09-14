using System;
using UnityEngine;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Features.Templates.ActionRPG.Data
{
    /// <summary>
    /// ActionRPGテンプレート用設定データ
    /// ゲームバランス、システム設定、パフォーマンス設定を管理
    /// </summary>
    [CreateAssetMenu(fileName = "ActionRPGSettings", menuName = "asterivo.Unity60/Templates/ActionRPG/Settings")]
    public class ActionRPGSettings : ScriptableObject
    {
        [TabGroup("Game", "Player")]
        [Header("プレイヤー設定")]
        [SerializeField] private float baseHealth = 100f;
        [SerializeField] private float baseMana = 50f;
        [SerializeField] private float baseAttackPower = 10f;
        [SerializeField] private float baseDefense = 5f;
        [SerializeField] private int startingLevel = 1;
        [SerializeField] private int maxLevel = 99;

        [TabGroup("Game", "Experience")]
        [Header("経験値設定")]
        [SerializeField] private float baseExperienceRequired = 100f;
        [SerializeField] private float experienceGrowthRate = 1.5f;
        [SerializeField] private bool enableExperienceBonus = true;

        [TabGroup("Game", "Combat")]
        [Header("戦闘システム設定")]
        [SerializeField] private float criticalChanceBase = 5f;
        [SerializeField] private float criticalDamageMultiplier = 2f;
        [SerializeField] private float statusEffectDuration = 10f;
        [SerializeField] private bool enableAutoTargeting = true;

        [TabGroup("Game", "Items")]
        [Header("アイテムシステム設定")]
        [SerializeField] private int inventoryMaxSlots = 50;
        [SerializeField] private int equipmentSlots = 8;
        [SerializeField] private float itemDropChance = 15f;
        [SerializeField] private bool enableItemQuality = true;

        [TabGroup("Performance", "Optimization")]
        [Header("パフォーマンス設定")]
        [SerializeField] private bool enablePerformanceMonitoring = true;
        [SerializeField] private float performanceUpdateInterval = 1f;
        [SerializeField] private int maxActiveEffects = 20;
        [SerializeField] private float cullingDistance = 100f;

        [TabGroup("Performance", "Debug")]
        [Header("デバッグ設定")]
        [SerializeField] private bool debugMode = false;
        [SerializeField] private bool showDamageNumbers = true;
        [SerializeField] private bool logSystemEvents = false;

        // プロパティ
        public float BaseHealth => baseHealth;
        public float BaseMana => baseMana;
        public float BaseAttackPower => baseAttackPower;
        public float BaseDefense => baseDefense;
        public int StartingLevel => startingLevel;
        public int MaxLevel => maxLevel;

        public float BaseExperienceRequired => baseExperienceRequired;
        public float ExperienceGrowthRate => experienceGrowthRate;
        public bool EnableExperienceBonus => enableExperienceBonus;

        public float CriticalChanceBase => criticalChanceBase;
        public float CriticalDamageMultiplier => criticalDamageMultiplier;
        public float StatusEffectDuration => statusEffectDuration;
        public bool EnableAutoTargeting => enableAutoTargeting;

        public int InventoryMaxSlots => inventoryMaxSlots;
        public int EquipmentSlots => equipmentSlots;
        public float ItemDropChance => itemDropChance;
        public bool EnableItemQuality => enableItemQuality;

        public bool EnablePerformanceMonitoring => enablePerformanceMonitoring;
        public float PerformanceUpdateInterval => performanceUpdateInterval;
        public int MaxActiveEffects => maxActiveEffects;
        public float CullingDistance => cullingDistance;

        public bool DebugMode => debugMode;
        public bool ShowDamageNumbers => showDamageNumbers;
        public bool LogSystemEvents => logSystemEvents;

        /// <summary>
        /// レベル別必要経験値を計算
        /// </summary>
        public int GetRequiredExperience(int level)
        {
            if (level <= startingLevel) return 0;
            return Mathf.RoundToInt(baseExperienceRequired * Mathf.Pow(experienceGrowthRate, level - startingLevel));
        }

        /// <summary>
        /// クリティカル確率を計算（レベル補正含む）
        /// </summary>
        public float GetCriticalChance(int level)
        {
            return criticalChanceBase + (level * 0.1f);
        }

        /// <summary>
        /// 設定値の検証
        /// </summary>
        [Button("Validate Settings")]
        [ShowIf("debugMode")]
        private void ValidateSettings()
        {
            if (baseHealth <= 0) Debug.LogWarning("[ActionRPGSettings] Base Health should be greater than 0");
            if (baseMana < 0) Debug.LogWarning("[ActionRPGSettings] Base Mana should be 0 or greater");
            if (maxLevel <= startingLevel) Debug.LogWarning("[ActionRPGSettings] Max Level should be greater than Starting Level");
            if (experienceGrowthRate <= 1f) Debug.LogWarning("[ActionRPGSettings] Experience Growth Rate should be greater than 1");

            Debug.Log("[ActionRPGSettings] Validation complete");
        }

        /// <summary>
        /// デフォルト設定にリセット
        /// </summary>
        [Button("Reset to Defaults")]
        [ShowIf("debugMode")]
        private void ResetToDefaults()
        {
            baseHealth = 100f;
            baseMana = 50f;
            baseAttackPower = 10f;
            baseDefense = 5f;
            startingLevel = 1;
            maxLevel = 99;

            baseExperienceRequired = 100f;
            experienceGrowthRate = 1.5f;
            enableExperienceBonus = true;

            criticalChanceBase = 5f;
            criticalDamageMultiplier = 2f;
            statusEffectDuration = 10f;
            enableAutoTargeting = true;

            inventoryMaxSlots = 50;
            equipmentSlots = 8;
            itemDropChance = 15f;
            enableItemQuality = true;

            enablePerformanceMonitoring = true;
            performanceUpdateInterval = 1f;
            maxActiveEffects = 20;
            cullingDistance = 100f;

            debugMode = false;
            showDamageNumbers = true;
            logSystemEvents = false;

            Debug.Log("[ActionRPGSettings] Reset to default values");
        }
    }
}