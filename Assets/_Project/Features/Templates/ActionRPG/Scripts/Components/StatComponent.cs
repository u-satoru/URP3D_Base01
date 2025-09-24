using UnityEngine;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Features.Templates.ActionRPG.Data;

namespace asterivo.Unity60.Features.Templates.ActionRPG.Components
{
    /// <summary>
    /// プレイヤーの基礎ステータスを管理するコンポーネント
    /// レベルアップによる成長とステータスポイント配分を処理します
    /// </summary>
    public class StatComponent : MonoBehaviour
    {
        [Header("初期設定")]
        [SerializeField] private CharacterClassData _initialClassData;
        [SerializeField] private LevelUpCurveData _levelCurve;

        [Header("現在のステータス")]
        [SerializeField] private int _currentLevel = 1;
        [SerializeField] private int _currentExperience = 0;
        [SerializeField] private int _availableStatPoints = 0;

        [Header("基礎ステータス")]
        [SerializeField] private int _vitality = 10;
        [SerializeField] private int _strength = 10;
        [SerializeField] private int _dexterity = 5;
        [SerializeField] private int _intelligence = 5;
        [SerializeField] private int _faith = 5;
        [SerializeField] private int _luck = 5;

        [Header("イベント")]
        [SerializeField] private GameEvent _onStatChanged;
        [SerializeField] private GameEvent _onLevelUp;
        [SerializeField] private GameEvent _onExperienceGained;

        // 派生ステータス（読み取り専用）
        public int MaxHealth => CalculateMaxHealth();
        public int MaxFocus => CalculateMaxFocus();
        public int AttackPower => CalculateAttackPower();
        public int Defense => CalculateDefense();

        // プロパティ
        public int CurrentLevel => _currentLevel;
        public int CurrentExperience => _currentExperience;
        public int AvailableStatPoints => _availableStatPoints;
        public int Vitality => _vitality;
        public int Strength => _strength;
        public int Dexterity => _dexterity;
        public int Intelligence => _intelligence;
        public int Faith => _faith;
        public int Luck => _luck;

        void Start()
        {
            InitializeFromClassData();
        }

        /// <summary>
        /// 初期クラスデータからステータスを初期化
        /// </summary>
        private void InitializeFromClassData()
        {
            if (_initialClassData == null) return;

            _vitality = _initialClassData.Vitality;
            _strength = _initialClassData.Strength;
            _dexterity = _initialClassData.Dexterity;
            _intelligence = _initialClassData.Intelligence;
            _faith = _initialClassData.Faith;
            _luck = _initialClassData.Luck;

            NotifyStatChanged();
        }

        /// <summary>
        /// 経験値を追加
        /// </summary>
        public void AddExperience(int amount)
        {
            if (amount <= 0) return;

            _currentExperience += amount;
            
            // イベント発行
            if (_onExperienceGained != null)
                _onExperienceGained.Raise();

            // レベルアップ判定
            CheckForLevelUp();
        }

        /// <summary>
        /// レベルアップ判定と処理
        /// </summary>
        private void CheckForLevelUp()
        {
            if (_levelCurve == null) return;

            while (_levelCurve.CanLevelUp(_currentExperience, _currentLevel))
            {
                _currentLevel++;
                _availableStatPoints += _levelCurve.BaseStatPointsPerLevel;
                
                // レベルアップイベント発行
                if (_onLevelUp != null)
                    _onLevelUp.Raise();

                // 設定に応じてHP/フォーカス全回復
                if (_levelCurve.FullHealOnLevelUp)
                {
                    var healthComponent = GetComponent<asterivo.Unity60.Core.Combat.HealthComponent>();
                    if (healthComponent != null)
                    {
                        healthComponent.SetHealth(MaxHealth);
                    }
                }

                Debug.Log($"レベルアップ！ レベル {_currentLevel} になりました。利用可能ステータスポイント: {_availableStatPoints}");
            }
        }

        /// <summary>
        /// ステータスポイントを配分
        /// </summary>
        public bool AllocateStatPoint(StatType statType, int points = 1)
        {
            if (_availableStatPoints < points) return false;

            switch (statType)
            {
                case StatType.Vitality:
                    _vitality += points;
                    break;
                case StatType.Strength:
                    _strength += points;
                    break;
                case StatType.Dexterity:
                    _dexterity += points;
                    break;
                case StatType.Intelligence:
                    _intelligence += points;
                    break;
                case StatType.Faith:
                    _faith += points;
                    break;
                case StatType.Luck:
                    _luck += points;
                    break;
                default:
                    return false;
            }

            _availableStatPoints -= points;
            NotifyStatChanged();
            return true;
        }

        /// <summary>
        /// 最大HPを計算
        /// </summary>
        private int CalculateMaxHealth()
        {
            int baseHealth = _initialClassData != null ? _initialClassData.MaxHealthBase : 100;
            int levelBonus = _levelCurve != null ? _levelCurve.GetHealthBonus(_currentLevel) : 0;
            int vitalityBonus = _vitality * 10;
            
            return baseHealth + levelBonus + vitalityBonus;
        }

        /// <summary>
        /// 最大フォーカスを計算
        /// </summary>
        private int CalculateMaxFocus()
        {
            int baseFocus = _initialClassData != null ? _initialClassData.MaxFocusBase : 50;
            int levelBonus = _levelCurve != null ? _levelCurve.GetFocusBonus(_currentLevel) : 0;
            int intelligenceBonus = _intelligence * 5;
            
            return baseFocus + levelBonus + intelligenceBonus;
        }

        /// <summary>
        /// 攻撃力を計算
        /// </summary>
        private int CalculateAttackPower()
        {
            int baseAttack = Mathf.FloorToInt(_strength * 1.5f);
            int dexterityBonus = Mathf.FloorToInt(_dexterity * 0.5f);
            
            return baseAttack + dexterityBonus;
        }

        /// <summary>
        /// 防御力を計算
        /// </summary>
        private int CalculateDefense()
        {
            int vitalityDefense = Mathf.FloorToInt(_vitality * 0.8f);
            int faithDefense = Mathf.FloorToInt(_faith * 0.3f);
            
            return vitalityDefense + faithDefense;
        }

        /// <summary>
        /// ステータス変更通知
        /// </summary>
        private void NotifyStatChanged()
        {
            // HealthComponentの最大値を更新
            var healthComponent = GetComponent<asterivo.Unity60.Core.Combat.HealthComponent>();
            if (healthComponent != null)
            {
                healthComponent.Initialize(MaxHealth);
            }

            // イベント発行
            if (_onStatChanged != null)
                _onStatChanged.Raise();
        }

        /// <summary>
        /// 次のレベルまでの必要経験値を取得
        /// </summary>
        public int GetExperienceToNextLevel()
        {
            if (_levelCurve == null) return 0;
            return _levelCurve.GetExperienceToNextLevel(_currentExperience);
        }

        /// <summary>
        /// 現在レベルの進捗率を取得（0.0-1.0）
        /// </summary>
        public float GetLevelProgress()
        {
            if (_levelCurve == null) return 0f;
            
            int currentLevelExp = _levelCurve.GetRequiredExperience(_currentLevel);
            int nextLevelExp = _levelCurve.GetRequiredExperience(_currentLevel + 1);
            
            if (nextLevelExp <= currentLevelExp) return 1f;
            
            float progress = (float)(_currentExperience - currentLevelExp) / (nextLevelExp - currentLevelExp);
            return Mathf.Clamp01(progress);
        }
    }

    /// <summary>
    /// ステータスの種類
    /// </summary>
    public enum StatType
    {
        Vitality,      // 生命力
        Strength,      // 筋力
        Dexterity,     // 器用さ
        Intelligence,  // 知力
        Faith,         // 信仰
        Luck          // 運
    }
}
