using UnityEngine;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Features.Templates.Common;

namespace asterivo.Unity60.Features.Templates.SurvivalHorror
{
    /// <summary>
    /// SurvivalHorrorテンプレートのメイン設定を管理するScriptableObject
    /// 恐怖感、リソース管理、難易度設定を統合的に制御
    /// </summary>
    [CreateAssetMenu(fileName = "SH_TemplateConfig", menuName = "Templates/SurvivalHorror/Template Config")]
    public class SH_TemplateConfig : ScriptableObject, ITemplateConfig
    {
        [Header("Template Basic Info")]
        [SerializeField] private string templateName = "Survival Horror";
        [SerializeField] private string templateVersion = "1.0.0";
        [SerializeField] private string description = "心理的恐怖とリソース管理を核とするサバイバルホラーテンプレート";

        [Header("Difficulty Settings")]
        [SerializeField] private DifficultyLevel defaultDifficulty = DifficultyLevel.Normal;
        [SerializeField] private SurvivalDifficultySettings easySettings;
        [SerializeField] private SurvivalDifficultySettings normalSettings;
        [SerializeField] private SurvivalDifficultySettings hardSettings;
        [SerializeField] private SurvivalDifficultySettings nightmareSettings;

        [Header("Core Gameplay Parameters")]
        [SerializeField] private float baseSanityDecayRate = 0.5f;
        [SerializeField] private int defaultInventorySlots = 8;
        [SerializeField] private float stalkerSpawnDelay = 300f; // 5分後にストーカーAI出現
        [SerializeField] private bool allowManualSave = true;
        [SerializeField] private bool requireInkRibbonForSave = false; // クラシックスタイル

        [Header("Fear & Atmosphere")]
        [SerializeField] private SH_AtmosphereConfig atmosphereConfig;
        [SerializeField] private float darknessSanityDecayMultiplier = 2.0f;
        [SerializeField] private float isolationDecayMultiplier = 1.5f;
        [SerializeField] private AnimationCurve fearIntensityCurve;

        [Header("Resource Management")]
        [SerializeField] private SH_ResourceManagerConfig resourceConfig;
        [SerializeField] private float itemScarcityMultiplier = 1.0f;
        #pragma warning disable 0414
        [SerializeField] private bool enableResourceRespawn = false;
        #pragma warning restore 0414

        [Header("Events")]
        [SerializeField] private GameEvent<DifficultyLevel> onDifficultyChanged;
        [SerializeField] private GameEvent<bool> onTemplateActivated;
        [SerializeField] private GameEvent onTemplateConfigUpdated;

        // Public Properties
        public string TemplateName => templateName;
        public string TemplateVersion => templateVersion;
        public string Description => description;
        public DifficultyLevel DefaultDifficulty => defaultDifficulty;
        public float BaseSanityDecayRate => baseSanityDecayRate;
        public int DefaultInventorySlots => defaultInventorySlots;
        public float StalkerSpawnDelay => stalkerSpawnDelay;
        public bool AllowManualSave => allowManualSave;
        public bool RequireInkRibbonForSave => requireInkRibbonForSave;
        public SH_AtmosphereConfig AtmosphereConfig => atmosphereConfig;
        public SH_ResourceManagerConfig ResourceConfig => resourceConfig;

        // Runtime State
        private DifficultyLevel currentDifficulty;
        private bool isActive = false;

        public DifficultyLevel CurrentDifficulty => currentDifficulty;
        public bool IsActive => isActive;

        private void OnEnable()
        {
            currentDifficulty = defaultDifficulty;
        }

        /// <summary>
        /// テンプレートを有効化し、設定を適用
        /// </summary>
        public void ActivateTemplate()
        {
            if (isActive) return;

            isActive = true;
            ApplyCurrentDifficultySettings();
            onTemplateActivated?.Raise(true);

            Debug.Log($"[SH_TemplateConfig] Survival Horror template activated with {currentDifficulty} difficulty");
        }

        /// <summary>
        /// テンプレートを無効化
        /// </summary>
        public void DeactivateTemplate()
        {
            isActive = false;
            onTemplateActivated?.Raise(false);

            Debug.Log("[SH_TemplateConfig] Survival Horror template deactivated");
        }

        /// <summary>
        /// 難易度を変更し、設定を即座に適用
        /// </summary>
        public void SetDifficulty(DifficultyLevel newDifficulty)
        {
            if (currentDifficulty == newDifficulty) return;

            var previousDifficulty = currentDifficulty;
            currentDifficulty = newDifficulty;

            if (isActive)
            {
                ApplyCurrentDifficultySettings();
            }

            onDifficultyChanged?.Raise(newDifficulty);

            Debug.Log($"[SH_TemplateConfig] Difficulty changed from {previousDifficulty} to {newDifficulty}");
        }

        /// <summary>
        /// 現在の難易度設定を取得
        /// </summary>
        public SurvivalDifficultySettings GetCurrentDifficultySettings()
        {
            return currentDifficulty switch
            {
                DifficultyLevel.Easy => easySettings,
                DifficultyLevel.Normal => normalSettings,
                DifficultyLevel.Hard => hardSettings,
                DifficultyLevel.Nightmare => nightmareSettings,
                _ => normalSettings
            };
        }

        /// <summary>
        /// 恐怖強度に基づく正気度減少倍率を計算
        /// </summary>
        public float CalculateFearMultiplier(float fearIntensity)
        {
            fearIntensity = Mathf.Clamp01(fearIntensity);

            if (fearIntensityCurve != null && fearIntensityCurve.keys.Length > 0)
            {
                return fearIntensityCurve.Evaluate(fearIntensity);
            }

            // デフォルトの線形補間
            return Mathf.Lerp(1.0f, 3.0f, fearIntensity);
        }

        /// <summary>
        /// 環境要因に基づく正気度減少倍率を計算
        /// </summary>
        public float CalculateEnvironmentalMultiplier(bool inDarkness, bool isolated)
        {
            float multiplier = 1.0f;

            if (inDarkness)
            {
                multiplier *= darknessSanityDecayMultiplier;
            }

            if (isolated)
            {
                multiplier *= isolationDecayMultiplier;
            }

            return multiplier;
        }

        /// <summary>
        /// アイテム希少度に基づく出現率を計算
        /// </summary>
        public float CalculateItemSpawnRate(float baseSpawnRate)
        {
            var difficultySettings = GetCurrentDifficultySettings();
            return baseSpawnRate * difficultySettings.itemSpawnRateMultiplier * itemScarcityMultiplier;
        }

        /// <summary>
        /// 現在の難易度設定をゲーム全体に適用
        /// </summary>
        private void ApplyCurrentDifficultySettings()
        {
            var settings = GetCurrentDifficultySettings();

            // 設定更新イベントを発行（他のシステムが反応）
            onTemplateConfigUpdated?.Raise();
        }

        /// <summary>
        /// 設定の整合性を検証
        /// </summary>
        public bool ValidateConfiguration()
        {
            if (baseSanityDecayRate < 0)
            {
                Debug.LogError("[SH_TemplateConfig] BaseSanityDecayRate cannot be negative");
                return false;
            }

            if (defaultInventorySlots <= 0)
            {
                Debug.LogError("[SH_TemplateConfig] DefaultInventorySlots must be positive");
                return false;
            }

            if (atmosphereConfig == null)
            {
                Debug.LogWarning("[SH_TemplateConfig] AtmosphereConfig is not assigned");
            }

            if (resourceConfig == null)
            {
                Debug.LogWarning("[SH_TemplateConfig] ResourceConfig is not assigned");
            }

            return true;
        }

        // エディタ専用デバッグ機能
        #if UNITY_EDITOR
        [UnityEditor.MenuItem("Tools/SurvivalHorror/Validate Template Config")]
        private static void ValidateTemplateConfigFromMenu()
        {
            var configs = UnityEngine.Resources.LoadAll<SH_TemplateConfig>("");
            foreach (var config in configs)
            {
                config.ValidateConfiguration();
            }
        }
        #endif
    }

    /// <summary>
    /// 難易度レベル定義
    /// </summary>
    public enum DifficultyLevel
    {
        Easy,      // 初心者向け：リソース豊富、恐怖演出控えめ
        Normal,    // 標準：バランス型
        Hard,      // 上級者向け：リソース不足、高い恐怖演出
        Nightmare  // エクストリーム：極限のサバイバル体験
    }

    /// <summary>
    /// 難易度別詳細設定
    /// </summary>
    [System.Serializable]
    public class SurvivalDifficultySettings
    {
        [Header("Sanity & Fear")]
        public float sanityDecayMultiplier = 1.0f;
        public float fearIntensityMultiplier = 1.0f;
        public float sanityRestoreMultiplier = 1.0f;

        [Header("Resource Management")]
        public float itemSpawnRateMultiplier = 1.0f;
        public float healthItemEffectiveness = 1.0f;
        public int maxSaveSlots = 3;

        [Header("AI Behavior")]
        public float stalkerAggressiveness = 1.0f;
        public float stalkerDetectionRange = 1.0f;
        public float stalkerPersistence = 1.0f;

        [Header("Environmental")]
        public float darknessIntensity = 1.0f;
        public float environmentalHazardDamage = 1.0f;
        public bool enablePermadeath = false;
    }
}