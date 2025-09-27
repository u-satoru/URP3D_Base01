using UnityEngine;

namespace asterivo.Unity60.Features.Templates.Stealth.Configuration
{
    /// <summary>
    /// Layer 1: ステルス習熟度設定
    /// 学習進度追跡、スキル習得、難易度調整
    /// Learn & Grow価値実現のための詳細測定システム
    /// </summary>
    [System.Serializable]
    public class StealthProgressionConfig
    {
        [Header("Skill Progression Tracking")]
        [Tooltip("スキル習得追跡有効化")]
        public bool EnableSkillTracking = true;

        [Tooltip("習得すべき基本スキル数")]
        [Range(5, 20)]
        public int RequiredBasicSkills = 8;

        [Tooltip("習得すべき上級スキル数")]
        [Range(3, 15)]
        public int RequiredAdvancedSkills = 5;

        [Header("Proficiency Levels")]
        [Tooltip("初心者レベル閾値（%）")]
        [Range(0f, 50f)]
        public float BeginnerThreshold = 25f;

        [Tooltip("中級者レベル閾値（%）")]
        [Range(25f, 75f)]
        public float IntermediateThreshold = 60f;

        [Tooltip("上級者レベル閾値（%）")]
        [Range(60f, 95f)]
        public float AdvancedThreshold = 85f;

        [Tooltip("エキスパートレベル閾値（%）")]
        [Range(85f, 100f)]
        public float ExpertThreshold = 95f;

        [Header("Learning Curve Analysis")]
        [Tooltip("学習曲線測定有効化")]
        public bool EnableLearningCurveAnalysis = true;

        [Range(10, 100)]
        [Tooltip("測定データポイント数")]
        public int LearningDataPoints = 50;

        [Range(0.1f, 2f)]
        [Tooltip("学習効率係数")]
        public float LearningEfficiencyCoefficient = 1.2f;

        [Header("Adaptive Difficulty")]
        [Tooltip("適応的難易度調整")]
        public bool EnableAdaptiveDifficulty = true;

        [Range(0.5f, 2f)]
        [Tooltip("難易度調整感度")]
        public float DifficultyAdjustmentSensitivity = 1f;

        [Range(5f, 60f)]
        [Tooltip("難易度評価間隔（秒）")]
        public float DifficultyAssessmentInterval = 30f;

        [Header("70% Learning Cost Reduction Tracking")]
        [Tooltip("学習コスト削減測定")]
        public bool EnableLearningCostReductionTracking = true;

        [Range(20f, 50f)]
        [Tooltip("ベースライン学習時間（時間）")]
        public float BaselineLearningHours = 40f;

        [Range(5f, 20f)]
        [Tooltip("目標学習時間（時間）")]
        public float TargetLearningHours = 12f;

        [Tooltip("リアルタイム効率測定")]
        public bool EnableRealTimeEfficiencyMeasurement = true;

        [Header("15-Minute Gameplay Milestone")]
        [Tooltip("15分ゲームプレイ到達追跡")]
        public bool EnableFifteenMinuteMilestoneTracking = true;

        [Range(8f, 25f)]
        [Tooltip("基本ゲームプレイ習得目標時間（分）")]
        public float BasicGameplayMasteryMinutes = 15f;

        [Tooltip("マイルストーン自動記録")]
        public bool EnableMilestoneAutoRecording = true;

        [System.Serializable]
        public class SkillCategory
        {
            public string SkillName;
            [Range(0f, 100f)]
            public float MasteryPercentage;
            public bool IsRequired;
            public float TimeToMaster; // 習得にかかった時間（秒）
        }

        [Tooltip("基本ステルススキル")]
        public SkillCategory[] BasicStealthSkills = new SkillCategory[]
        {
            new SkillCategory { SkillName = "Crouching Movement", IsRequired = true },
            new SkillCategory { SkillName = "Hiding Spot Usage", IsRequired = true },
            new SkillCategory { SkillName = "Sound Masking", IsRequired = true },
            new SkillCategory { SkillName = "AI Detection Avoidance", IsRequired = true },
            new SkillCategory { SkillName = "Environment Interaction", IsRequired = true },
            new SkillCategory { SkillName = "Stealth Movement", IsRequired = true },
            new SkillCategory { SkillName = "Timing Coordination", IsRequired = true },
            new SkillCategory { SkillName = "Situational Awareness", IsRequired = true }
        };

        [Tooltip("上級ステルススキル")]
        public SkillCategory[] AdvancedStealthSkills = new SkillCategory[]
        {
            new SkillCategory { SkillName = "Complex Route Planning", IsRequired = false },
            new SkillCategory { SkillName = "Multi-NPC Coordination", IsRequired = false },
            new SkillCategory { SkillName = "Advanced Audio Tactics", IsRequired = false },
            new SkillCategory { SkillName = "Environmental Manipulation", IsRequired = false },
            new SkillCategory { SkillName = "Adaptive Stealth Strategy", IsRequired = false }
        };

        [Header("Progress Persistence")]
        [Tooltip("進捗自動保存")]
        public bool EnableAutoProgressSave = true;

        [Range(10f, 300f)]
        [Tooltip("自動保存間隔（秒）")]
        public float AutoSaveInterval = 60f;

        [Tooltip("セッション間進捗保持")]
        public bool EnableCrossSessionProgressPersistence = true;

        /// <summary>
        /// Learn & Grow価値実現最適化設定
        /// </summary>
        public void ApplyLearnAndGrowOptimizedSettings()
        {
            EnableSkillTracking = true;
            RequiredBasicSkills = 6; // より集中的
            RequiredAdvancedSkills = 3;

            // より緩やかな習得閾値（学習コスト削減）
            BeginnerThreshold = 20f;
            IntermediateThreshold = 50f;
            AdvancedThreshold = 75f;
            ExpertThreshold = 90f;

            EnableLearningCurveAnalysis = true;
            LearningEfficiencyCoefficient = 1.5f; // 効率向上

            EnableAdaptiveDifficulty = true;
            DifficultyAdjustmentSensitivity = 1.3f; // より敏感な調整

            // 70%学習コスト削減目標
            EnableLearningCostReductionTracking = true;
            BaselineLearningHours = 40f;
            TargetLearningHours = 12f;
            EnableRealTimeEfficiencyMeasurement = true;

            // 15分ゲームプレイ実現
            EnableFifteenMinuteMilestoneTracking = true;
            BasicGameplayMasteryMinutes = 15f;
            EnableMilestoneAutoRecording = true;

            EnableAutoProgressSave = true;
            AutoSaveInterval = 30f; // より頻繁な保存
            EnableCrossSessionProgressPersistence = true;
        }

        /// <summary>
        /// 従来学習方式設定（比較用）
        /// </summary>
        public void ApplyTraditionalSettings()
        {
            RequiredBasicSkills = 12; // より多くのスキル要求
            RequiredAdvancedSkills = 8;

            BeginnerThreshold = 40f;
            IntermediateThreshold = 70f;
            AdvancedThreshold = 90f;
            ExpertThreshold = 98f;

            EnableAdaptiveDifficulty = false;
            EnableLearningCostReductionTracking = false;
            EnableFifteenMinuteMilestoneTracking = false;

            BaselineLearningHours = 40f;
            TargetLearningHours = 40f; // 削減なし
            BasicGameplayMasteryMinutes = 60f; // 1時間
        }

        /// <summary>
        /// 全体進捗率の計算
        /// </summary>
        public float CalculateOverallProgress()
        {
            float basicProgress = CalculateSkillCategoryProgress(BasicStealthSkills);
            float advancedProgress = CalculateSkillCategoryProgress(AdvancedStealthSkills);

            // 基本スキル80%、上級スキル20%の重み付け
            return (basicProgress * 0.8f) + (advancedProgress * 0.2f);
        }

        /// <summary>
        /// スキルカテゴリの進捗計算
        /// </summary>
        private float CalculateSkillCategoryProgress(SkillCategory[] skills)
        {
            if (skills == null || skills.Length == 0) return 0f;

            float totalProgress = 0f;
            foreach (var skill in skills)
            {
                totalProgress += skill.MasteryPercentage;
            }

            return totalProgress / skills.Length;
        }

        /// <summary>
        /// 学習効率の計算
        /// </summary>
        public float CalculateLearningEfficiency()
        {
            if (!EnableLearningCostReductionTracking) return 1f;

            return 1f - (TargetLearningHours / BaselineLearningHours);
        }

        /// <summary>
        /// 15分ゲームプレイ達成判定
        /// </summary>
        public bool IsBasicGameplayReady()
        {
            if (!EnableFifteenMinuteMilestoneTracking) return false;

            float requiredProgress = 50f; // 50%以上の習得で基本ゲームプレイ可能
            return CalculateOverallProgress() >= requiredProgress;
        }

        /// <summary>
        /// 習熟レベルの判定
        /// </summary>
        public ProficiencyLevel GetCurrentProficiencyLevel()
        {
            float progress = CalculateOverallProgress();

            if (progress >= ExpertThreshold) return ProficiencyLevel.Expert;
            if (progress >= AdvancedThreshold) return ProficiencyLevel.Advanced;
            if (progress >= IntermediateThreshold) return ProficiencyLevel.Intermediate;
            if (progress >= BeginnerThreshold) return ProficiencyLevel.Beginner;

            return ProficiencyLevel.Novice;
        }

        /// <summary>
        /// 設定の検証
        /// </summary>
        public bool ValidateSettings()
        {
            bool isValid = true;

            // 閾値の論理的順序確認
            if (BeginnerThreshold >= IntermediateThreshold)
            {
                Debug.LogWarning("StealthProgressionConfig: BeginnerThreshold should be less than IntermediateThreshold");
                isValid = false;
            }

            if (TargetLearningHours > BaselineLearningHours)
            {
                Debug.LogWarning("StealthProgressionConfig: TargetLearningHours should be less than BaselineLearningHours");
                isValid = false;
            }

            // Learn & Grow価値実現の検証
            float expectedReduction = CalculateLearningEfficiency();
            if (expectedReduction < 0.5f)
            {
                Debug.LogWarning("StealthProgressionConfig: Learning cost reduction is less than 50% - may not achieve Learn & Grow value");
            }

            return isValid;
        }
    }

    /// <summary>
    /// 習熟レベル列挙型
    /// </summary>
    public enum ProficiencyLevel
    {
        Novice,        // 初心者未満
        Beginner,      // 初心者
        Intermediate,  // 中級者
        Advanced,      // 上級者
        Expert         // エキスパート
    }
}
