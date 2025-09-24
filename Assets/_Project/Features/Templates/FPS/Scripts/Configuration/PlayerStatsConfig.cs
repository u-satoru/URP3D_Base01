using UnityEngine;

namespace asterivo.Unity60.Features.Templates.FPS.Configuration
{
    /// <summary>
    /// FPS Template用プレイヤーステータス設定
    /// 健康値、スタミナ、移動能力等の基本ステータスを管理
    /// </summary>
    [CreateAssetMenu(menuName = "FPS/Configuration/Player Stats Config")]
    public class PlayerStatsConfig : ScriptableObject
    {
        [Header("Health Settings")]
        [SerializeField] private float _maxHealth = 100f;
        [SerializeField] private float _healthRegenRate = 5f;
        [SerializeField] private float _healthRegenDelay = 3f;
        [SerializeField] private bool _enableHealthRegeneration = true;

        [Header("Stamina Settings")]
        [SerializeField] private float _maxStamina = 100f;
        [SerializeField] private float _staminaRegenRate = 20f;
        [SerializeField] private float _staminaDrainRate = 15f;
        [SerializeField] private float _staminaRegenDelay = 1f;

        [Header("Movement Stats")]
        [SerializeField] private float _baseMovementSpeed = 5f;
        [SerializeField] private float _sprintMultiplier = 1.5f;
        [SerializeField] private float _crouchMultiplier = 0.5f;
        [SerializeField] private float _jumpHeight = 1.2f;

        [Header("Combat Stats")]
        [SerializeField] private float _meleeDamage = 25f;
        [SerializeField] private float _meleeRange = 2f;
        [SerializeField] private float _damageResistance = 0f;
        [SerializeField] private float _criticalHitMultiplier = 2f;

        [Header("Experience & Progression")]
        [SerializeField] private int _currentLevel = 1;
        [SerializeField] private float _currentExperience = 0f;
        [SerializeField] private float _experienceToNextLevel = 100f;
        [SerializeField] private float _experienceMultiplier = 1f;

        [Header("Special Abilities")]
        [SerializeField] private bool _hasDoubleJump = false;
        [SerializeField] private bool _hasWallRun = false;
        [SerializeField] private bool _hasDash = false;
        [SerializeField] private float _dashDistance = 5f;
        [SerializeField] private float _dashCooldown = 3f;

        // Properties
        public float MaxHealth => _maxHealth;
        public float HealthRegenRate => _healthRegenRate;
        public float HealthRegenDelay => _healthRegenDelay;
        public bool EnableHealthRegeneration => _enableHealthRegeneration;
        public float MaxStamina => _maxStamina;
        public float StaminaRegenRate => _staminaRegenRate;
        public float StaminaDrainRate => _staminaDrainRate;
        public float StaminaRegenDelay => _staminaRegenDelay;
        public float BaseMovementSpeed => _baseMovementSpeed;
        public float SprintMultiplier => _sprintMultiplier;
        public float CrouchMultiplier => _crouchMultiplier;
        public float JumpHeight => _jumpHeight;
        public float MeleeDamage => _meleeDamage;
        public float MeleeRange => _meleeRange;
        public float DamageResistance => _damageResistance;
        public float CriticalHitMultiplier => _criticalHitMultiplier;
        public int CurrentLevel => _currentLevel;
        public float CurrentExperience => _currentExperience;
        public float ExperienceToNextLevel => _experienceToNextLevel;
        public float ExperienceMultiplier => _experienceMultiplier;
        public bool HasDoubleJump => _hasDoubleJump;
        public bool HasWallRun => _hasWallRun;
        public bool HasDash => _hasDash;
        public float DashDistance => _dashDistance;
        public float DashCooldown => _dashCooldown;

        /// <summary>
        /// 現在の移動速度を計算（基本速度 + 状態修正）
        /// </summary>
        public float GetCurrentMovementSpeed(bool isSprinting = false, bool isCrouching = false)
        {
            float speed = _baseMovementSpeed;

            if (isSprinting)
                speed *= _sprintMultiplier;
            else if (isCrouching)
                speed *= _crouchMultiplier;

            return speed;
        }

        /// <summary>
        /// ダメージ軽減を適用した実際のダメージ量を計算
        /// </summary>
        public float CalculateActualDamage(float incomingDamage)
        {
            return incomingDamage * (1f - _damageResistance);
        }

        /// <summary>
        /// 次のレベルに必要な経験値を取得
        /// </summary>
        public float GetRequiredExperienceForNextLevel()
        {
            return _experienceToNextLevel - _currentExperience;
        }

        /// <summary>
        /// 経験値を加算してレベルアップチェック
        /// </summary>
        public bool AddExperience(float experience)
        {
            _currentExperience += experience * _experienceMultiplier;

            if (_currentExperience >= _experienceToNextLevel)
            {
                LevelUp();
                return true;
            }

            return false;
        }

        /// <summary>
        /// レベルアップ処理
        /// </summary>
        private void LevelUp()
        {
            _currentLevel++;
            _currentExperience -= _experienceToNextLevel;
            _experienceToNextLevel *= 1.2f; // 次のレベルの必要経験値を20%増加

            // レベルアップ時のステータス向上
            _maxHealth += 10f;
            _maxStamina += 5f;
            _baseMovementSpeed += 0.1f;
        }

        /// <summary>
        /// ステータスをリセット（デバッグ用）
        /// </summary>
        [ContextMenu("Reset Stats")]
        public void ResetStats()
        {
            _currentLevel = 1;
            _currentExperience = 0f;
            _experienceToNextLevel = 100f;
            _maxHealth = 100f;
            _maxStamina = 100f;
            _baseMovementSpeed = 5f;
        }
    }
}