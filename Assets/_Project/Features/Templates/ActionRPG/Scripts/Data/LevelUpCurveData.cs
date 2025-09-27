using UnityEngine;

namespace asterivo.Unity60.Features.Templates.ActionRPG.Data
{
    /// <summary>
    /// レベルアップのための経験値カーブデータ
    /// </summary>
    [CreateAssetMenu(fileName = "New Level Curve", menuName = "ActionRPG/Level Up Curve Data")]
    public class LevelUpCurveData : ScriptableObject
    {
        [Header("レベルカーブ設定")]
        [SerializeField] private AnimationCurve _experiencePerLevel = new AnimationCurve(
            new Keyframe(1, 0),      // レベル1: 0経験値
            new Keyframe(2, 200),    // レベル2: 200経験値
            new Keyframe(3, 500),    // レベル3: 500経験値
            new Keyframe(10, 5000)   // レベル10: 5000経験値
        );

        [Header("レベル制限")]
        [SerializeField] private int _maxLevel = 50;
        [SerializeField] private int _startingLevel = 1;

        [Header("ステータス成長")]
        [SerializeField] private int _healthPerLevel = 10;
        [SerializeField] private int _focusPerLevel = 5;
        [SerializeField] private int _baseStatPointsPerLevel = 1;

        [Header("レベルアップボーナス")]
        [SerializeField] private bool _fullHealOnLevelUp = true;
        [SerializeField] private bool _fullFocusOnLevelUp = true;

        // プロパティ
        public AnimationCurve ExperiencePerLevel => _experiencePerLevel;
        public int MaxLevel => _maxLevel;
        public int StartingLevel => _startingLevel;
        public int HealthPerLevel => _healthPerLevel;
        public int FocusPerLevel => _focusPerLevel;
        public int BaseStatPointsPerLevel => _baseStatPointsPerLevel;
        public bool FullHealOnLevelUp => _fullHealOnLevelUp;
        public bool FullFocusOnLevelUp => _fullFocusOnLevelUp;

        /// <summary>
        /// 指定レベルに必要な累積経験値を取得
        /// </summary>
        public int GetRequiredExperience(int level)
        {
            if (level <= _startingLevel) return 0;
            if (level > _maxLevel) level = _maxLevel;
            
            return Mathf.RoundToInt(_experiencePerLevel.Evaluate(level));
        }

        /// <summary>
        /// 現在の経験値からレベルを計算
        /// </summary>
        public int CalculateLevel(int currentExperience)
        {
            for (int level = _startingLevel; level <= _maxLevel; level++)
            {
                int requiredExp = GetRequiredExperience(level + 1);
                if (currentExperience < requiredExp)
                {
                    return level;
                }
            }
            return _maxLevel;
        }

        /// <summary>
        /// 次のレベルまでに必要な経験値を計算
        /// </summary>
        public int GetExperienceToNextLevel(int currentExperience)
        {
            int currentLevel = CalculateLevel(currentExperience);
            if (currentLevel >= _maxLevel) return 0;
            
            int nextLevelExp = GetRequiredExperience(currentLevel + 1);
            return nextLevelExp - currentExperience;
        }

        /// <summary>
        /// レベルアップ判定
        /// </summary>
        public bool CanLevelUp(int currentExperience, int currentLevel)
        {
            if (currentLevel >= _maxLevel) return false;
            
            int requiredExp = GetRequiredExperience(currentLevel + 1);
            return currentExperience >= requiredExp;
        }

        /// <summary>
        /// レベルに応じた基本HP増加量を取得
        /// </summary>
        public int GetHealthBonus(int level)
        {
            return (level - _startingLevel) * _healthPerLevel;
        }

        /// <summary>
        /// レベルに応じた基本フォーカス増加量を取得
        /// </summary>
        public int GetFocusBonus(int level)
        {
            return (level - _startingLevel) * _focusPerLevel;
        }

        /// <summary>
        /// レベルに応じた累積ステータスポイントを取得
        /// </summary>
        public int GetStatPoints(int level)
        {
            return (level - _startingLevel) * _baseStatPointsPerLevel;
        }
    }
}
