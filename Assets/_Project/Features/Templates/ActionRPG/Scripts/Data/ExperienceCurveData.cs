using System;
using UnityEngine;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Features.Templates.ActionRPG.Data
{
    /// <summary>
    /// 経験値カーブデータ
    /// レベルアップに必要な経験値を管理
    /// </summary>
    [CreateAssetMenu(fileName = "ExperienceCurveData", menuName = "asterivo.Unity60/Templates/ActionRPG/Experience Curve")]
    public class ExperienceCurveData : ScriptableObject
    {
        [TabGroup("Curve", "Basic")]
        [Header("基本設定")]
        [SerializeField] private int maxLevel = 99;
        [SerializeField] private int baseExperience = 100;
        [SerializeField] private float growthRate = 1.5f;
        [SerializeField] private bool useLinearGrowth = false;

        [TabGroup("Curve", "Advanced")]
        [Header("高度設定")]
        [SerializeField] private AnimationCurve customCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] private bool useCustomCurve = false;
        [SerializeField] private float curveMultiplier = 1000f;

        [TabGroup("Curve", "Milestones")]
        [Header("マイルストーン設定")]
        [SerializeField] private int[] milestones = { 10, 25, 50, 75, 90 };
        [SerializeField] private float[] milestoneMultipliers = { 1.2f, 1.5f, 2.0f, 2.5f, 3.0f };

        [TabGroup("Preview", "Chart")]
        [Header("プレビュー")]
        [ShowInInspector, ReadOnly, ProgressBar(0, 1000000)]
        private int[] experienceTable = new int[100];

        [TabGroup("Preview", "Statistics")]
        [ShowInInspector, ReadOnly]
        private int totalExperienceToMax;

        [TabGroup("Preview", "Statistics")]
        [ShowInInspector, ReadOnly]
        private float averageGrowthRate;

        // プロパティ
        public int MaxLevel => maxLevel;
        public int BaseExperience => baseExperience;
        public float GrowthRate => growthRate;

        /// <summary>
        /// 指定レベルに必要な経験値を取得
        /// </summary>
        public int GetRequiredExperience(int level)
        {
            if (level <= 1) return 0;
            if (level > maxLevel) return GetRequiredExperience(maxLevel);

            if (useCustomCurve)
            {
                float normalizedLevel = (level - 1f) / (maxLevel - 1f);
                return Mathf.RoundToInt(customCurve.Evaluate(normalizedLevel) * curveMultiplier);
            }
            else if (useLinearGrowth)
            {
                return baseExperience * (level - 1);
            }
            else
            {
                float experience = baseExperience * Mathf.Pow(growthRate, level - 2);

                // マイルストーン補正
                experience *= GetMilestoneMultiplier(level);

                return Mathf.RoundToInt(experience);
            }
        }

        /// <summary>
        /// レベル1から指定レベルまでの累計経験値を取得
        /// </summary>
        public int GetCumulativeExperience(int level)
        {
            if (level <= 1) return 0;

            int total = 0;
            for (int i = 2; i <= level; i++)
            {
                total += GetRequiredExperience(i);
            }
            return total;
        }

        /// <summary>
        /// 現在の経験値から次のレベルまでの進行率を計算
        /// </summary>
        public float GetLevelProgress(int currentLevel, int currentExp)
        {
            if (currentLevel >= maxLevel) return 1f;

            int expForCurrentLevel = GetCumulativeExperience(currentLevel);
            int expForNextLevel = GetCumulativeExperience(currentLevel + 1);
            int expNeeded = expForNextLevel - expForCurrentLevel;
            int expProgress = currentExp - expForCurrentLevel;

            return expNeeded > 0 ? Mathf.Clamp01((float)expProgress / expNeeded) : 1f;
        }

        /// <summary>
        /// 指定経験値から現在のレベルを計算
        /// </summary>
        public int CalculateLevel(int totalExperience)
        {
            for (int level = 1; level <= maxLevel; level++)
            {
                if (totalExperience < GetCumulativeExperience(level))
                    return level - 1;
            }
            return maxLevel;
        }

        /// <summary>
        /// マイルストーン倍率を取得
        /// </summary>
        private float GetMilestoneMultiplier(int level)
        {
            float multiplier = 1f;

            for (int i = 0; i < milestones.Length && i < milestoneMultipliers.Length; i++)
            {
                if (level >= milestones[i])
                {
                    multiplier = milestoneMultipliers[i];
                }
            }

            return multiplier;
        }

        /// <summary>
        /// 経験値テーブルの更新
        /// </summary>
        private void UpdateExperienceTable()
        {
            experienceTable = new int[maxLevel + 1];
            totalExperienceToMax = 0;

            for (int i = 1; i <= maxLevel; i++)
            {
                experienceTable[i] = GetRequiredExperience(i);
                totalExperienceToMax += experienceTable[i];
            }

            // 平均成長率を計算
            if (maxLevel > 1)
            {
                float totalGrowth = experienceTable[maxLevel] / (float)experienceTable[2];
                averageGrowthRate = Mathf.Pow(totalGrowth, 1f / (maxLevel - 2));
            }
        }

        /// <summary>
        /// 設定変更時の自動更新
        /// </summary>
        private void OnValidate()
        {
            maxLevel = Mathf.Clamp(maxLevel, 2, 999);
            baseExperience = Mathf.Max(1, baseExperience);
            growthRate = Mathf.Clamp(growthRate, 1.01f, 10f);
            curveMultiplier = Mathf.Max(1f, curveMultiplier);

            UpdateExperienceTable();
        }

        /// <summary>
        /// エディタでの初期化
        /// </summary>
        private void Awake()
        {
            UpdateExperienceTable();
        }

        /// <summary>
        /// カーブプリセットの適用
        /// </summary>
        [Button("Linear Curve")]
        [TabGroup("Curve", "Advanced")]
        private void SetLinearCurve()
        {
            customCurve = AnimationCurve.Linear(0, 0, 1, 1);
            UpdateExperienceTable();
        }

        [Button("Exponential Curve")]
        [TabGroup("Curve", "Advanced")]
        private void SetExponentialCurve()
        {
            customCurve = new AnimationCurve();
            customCurve.AddKey(0, 0);
            customCurve.AddKey(0.5f, 0.25f);
            customCurve.AddKey(1, 1);
            UpdateExperienceTable();
        }

        [Button("S-Curve")]
        [TabGroup("Curve", "Advanced")]
        private void SetSCurve()
        {
            customCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
            UpdateExperienceTable();
        }

        /// <summary>
        /// デバッグ情報表示
        /// </summary>
        [Button("Show Experience Table")]
        [TabGroup("Preview", "Chart")]
        private void ShowExperienceTable()
        {
            UpdateExperienceTable();

            Debug.Log($"[ExperienceCurve] Max Level: {maxLevel}, Total EXP to Max: {totalExperienceToMax:N0}");

            for (int i = 1; i <= Mathf.Min(10, maxLevel); i++)
            {
                Debug.Log($"Level {i}: {experienceTable[i]:N0} EXP (Cumulative: {GetCumulativeExperience(i):N0})");
            }

            if (maxLevel > 10)
            {
                Debug.Log("... (showing first 10 levels only)");
            }
        }

        /// <summary>
        /// カーブの最適化提案
        /// </summary>
        [Button("Optimize Curve")]
        [TabGroup("Curve", "Basic")]
        private void OptimizeCurve()
        {
            UpdateExperienceTable();

            if (averageGrowthRate < 1.1f)
            {
                Debug.LogWarning("[ExperienceCurve] Growth rate too low - progression may feel slow");
            }
            else if (averageGrowthRate > 3f)
            {
                Debug.LogWarning("[ExperienceCurve] Growth rate too high - progression may feel too fast");
            }
            else
            {
                Debug.Log("[ExperienceCurve] Growth rate looks balanced");
            }

            float lateGameJump = experienceTable[Mathf.RoundToInt(maxLevel * 0.9f)] / (float)experienceTable[Mathf.RoundToInt(maxLevel * 0.5f)];
            if (lateGameJump > 10f)
            {
                Debug.LogWarning("[ExperienceCurve] Late game progression may be too steep");
            }
        }
    }
}