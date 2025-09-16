using UnityEngine;
using System.Collections.Generic;

namespace asterivo.Unity60.Features.Templates.FPS.Data
{
    /// <summary>
    /// プレイヤーデータ設定
    /// ServiceLocator + Event駆動のハイブリッドアーキテクチャ準拠
    /// ScriptableObjectベースのデータ管理システム
    /// ノンプログラマーによるゲームバランス調整対応
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerData", menuName = "FPS Template/Data/Player Data", order = 2)]
    public class PlayerData : ScriptableObject
    {
        [Header("基本設定")]
        [SerializeField] private string _playerName = "Player";
        [SerializeField] private int _playerLevel = 1;
        [SerializeField] private int _experience = 0;
        [SerializeField] private int _experienceToNextLevel = 1000;

        [Header("体力・生存設定")]
        [SerializeField] private float _maxHealth = 100f;
        [SerializeField] private float _currentHealth = 100f;
        [SerializeField] private float _healthRegenRate = 5f; // 毎秒回復量
        [SerializeField] private float _healthRegenDelay = 3f; // ダメージ後の回復開始時間
        [SerializeField] private float _shield = 0f;
        [SerializeField] private float _maxShield = 50f;
        [SerializeField] private float _shieldRegenRate = 10f;

        [Header("移動設定")]
        [SerializeField] private float _walkSpeed = 5f;
        [SerializeField] private float _runSpeed = 8f;
        [SerializeField] private float _crouchSpeed = 2f;
        [SerializeField] private float _proneSpeed = 1f;
        [SerializeField] private float _jumpHeight = 2f;
        [SerializeField] private float _jumpForce = 10f;
        [SerializeField] private int _maxJumps = 1;

        [Header("スタミナ設定")]
        [SerializeField] private float _maxStamina = 100f;
        [SerializeField] private float _currentStamina = 100f;
        [SerializeField] private float _staminaDrainRate = 20f; // 走行時の消費率
        [SerializeField] private float _staminaRegenRate = 25f; // 回復率
        [SerializeField] private float _staminaRegenDelay = 1f; // 回復開始遅延

        [Header("戦闘設定")]
        [SerializeField] private float _damageMultiplier = 1f;
        [SerializeField] private float _defenseMultiplier = 1f;
        [SerializeField] private float _criticalChance = 0.1f; // 10%
        [SerializeField] private float _criticalMultiplier = 2f;
        [SerializeField] private int _maxWeapons = 2;
        [SerializeField] private WeaponData _primaryWeapon;
        [SerializeField] private WeaponData _secondaryWeapon;

        [Header("精度・操作設定")]
        [SerializeField] private float _mouseSensitivity = 2f;
        [SerializeField] private float _aimSensitivityMultiplier = 0.5f;
        [SerializeField] private float _recoilRecovery = 1f;
        [SerializeField] private bool _invertMouseY = false;
        [SerializeField] private Events.InputDeviceType _preferredInputDevice = Events.InputDeviceType.KeyboardMouse;

        [Header("視界・カメラ設定")]
        [SerializeField] private float _fieldOfView = 75f;
        [SerializeField] private float _aimFieldOfView = 45f;
        [SerializeField] private float _headBobIntensity = 1f;
        [SerializeField] private bool _enableHeadBob = true;
        [SerializeField] private Events.CameraStateType _defaultCameraState = Events.CameraStateType.FirstPerson;

        [Header("インベントリ設定")]
        [SerializeField] private int _inventoryCapacity = 20;
        [SerializeField] private List<ItemData> _startingItems = new();
        [SerializeField] private Dictionary<string, int> _itemCounts = new();

        [Header("スキル・能力")]
        [SerializeField] private List<PlayerSkill> _skills = new();
        [SerializeField] private List<PlayerAbility> _abilities = new();
        [SerializeField] private int _skillPoints = 0;
        [SerializeField] private int _abilityPoints = 0;

        [Header("統計情報")]
        [SerializeField] private PlayerStatistics _statistics = new();

        [Header("設定・フラグ")]
        [SerializeField] private bool _godMode = false;
        [SerializeField] private bool _infiniteAmmo = false;
        [SerializeField] private bool _noReload = false;
        [SerializeField] private float _difficultyModifier = 1f;

        // プロパティ（読み取り専用）
        public string PlayerName => _playerName;
        public int PlayerLevel => _playerLevel;
        public int Experience => _experience;
        public int ExperienceToNextLevel => _experienceToNextLevel;

        // 体力関連
        public float MaxHealth => _maxHealth;
        public float CurrentHealth => _currentHealth;
        public float HealthPercentage => _maxHealth > 0 ? _currentHealth / _maxHealth : 0f;
        public float HealthRegenRate => _healthRegenRate;
        public float HealthRegenDelay => _healthRegenDelay;
        public float Shield => _shield;
        public float MaxShield => _maxShield;
        public float ShieldPercentage => _maxShield > 0 ? _shield / _maxShield : 0f;
        public float ShieldRegenRate => _shieldRegenRate;

        // 移動関連
        public float WalkSpeed => _walkSpeed;
        public float RunSpeed => _runSpeed;
        public float CrouchSpeed => _crouchSpeed;
        public float ProneSpeed => _proneSpeed;
        public float JumpHeight => _jumpHeight;
        public float JumpForce => _jumpForce;
        public int MaxJumps => _maxJumps;

        // スタミナ関連
        public float MaxStamina => _maxStamina;
        public float CurrentStamina => _currentStamina;
        public float StaminaPercentage => _maxStamina > 0 ? _currentStamina / _maxStamina : 0f;
        public float StaminaDrainRate => _staminaDrainRate;
        public float StaminaRegenRate => _staminaRegenRate;
        public float StaminaRegenDelay => _staminaRegenDelay;

        // 戦闘関連
        public float DamageMultiplier => _damageMultiplier;
        public float DefenseMultiplier => _defenseMultiplier;
        public float CriticalChance => _criticalChance;
        public float CriticalMultiplier => _criticalMultiplier;
        public int MaxWeapons => _maxWeapons;
        public WeaponData PrimaryWeapon => _primaryWeapon;
        public WeaponData SecondaryWeapon => _secondaryWeapon;

        // 操作関連
        public float MouseSensitivity => _mouseSensitivity;
        public float AimSensitivityMultiplier => _aimSensitivityMultiplier;
        public float RecoilRecovery => _recoilRecovery;
        public bool InvertMouseY => _invertMouseY;
        public Events.InputDeviceType PreferredInputDevice => _preferredInputDevice;

        // カメラ関連
        public float FieldOfView => _fieldOfView;
        public float AimFieldOfView => _aimFieldOfView;
        public float HeadBobIntensity => _headBobIntensity;
        public bool EnableHeadBob => _enableHeadBob;
        public Events.CameraStateType DefaultCameraState => _defaultCameraState;

        // インベントリ関連
        public int InventoryCapacity => _inventoryCapacity;
        public IReadOnlyList<ItemData> StartingItems => _startingItems.AsReadOnly();

        // スキル・能力
        public IReadOnlyList<PlayerSkill> Skills => _skills.AsReadOnly();
        public IReadOnlyList<PlayerAbility> Abilities => _abilities.AsReadOnly();
        public int SkillPoints => _skillPoints;
        public int AbilityPoints => _abilityPoints;

        // 統計情報
        public PlayerStatistics Statistics => _statistics;

        // デバッグ・チート
        public bool GodMode => _godMode;
        public bool InfiniteAmmo => _infiniteAmmo;
        public bool NoReload => _noReload;
        public float DifficultyModifier => _difficultyModifier;

        /// <summary>
        /// 体力変更
        /// </summary>
        public void SetHealth(float health)
        {
            float previousHealth = _currentHealth;
            _currentHealth = Mathf.Clamp(health, 0f, _maxHealth);

            // 体力変更イベント発行
            if (Mathf.Abs(previousHealth - _currentHealth) > 0.001f)
            {
                var healthData = new Events.HealthChangeData(
                    gameObject: null, // 実際の使用時にはGameObjectを設定
                    previousHealth: previousHealth,
                    currentHealth: _currentHealth,
                    maxHealth: _maxHealth,
                    changeAmount: _currentHealth - previousHealth,
                    changeSource: "SetHealth"
                );

                var healthEvent = Resources.Load<Events.HealthChangeEvent>("Events/HealthChangeEvent");
                healthEvent?.RaiseHealthChanged(healthData);
            }
        }

        /// <summary>
        /// シールド変更
        /// </summary>
        public void SetShield(float shield)
        {
            _shield = Mathf.Clamp(shield, 0f, _maxShield);
        }

        /// <summary>
        /// スタミナ変更
        /// </summary>
        public void SetStamina(float stamina)
        {
            _currentStamina = Mathf.Clamp(stamina, 0f, _maxStamina);
        }

        /// <summary>
        /// 経験値獲得
        /// </summary>
        public bool AddExperience(int amount)
        {
            if (amount <= 0) return false;

            _experience += amount;
            bool leveledUp = false;

            // レベルアップ判定
            while (_experience >= _experienceToNextLevel)
            {
                _experience -= _experienceToNextLevel;
                _playerLevel++;
                _experienceToNextLevel = Mathf.RoundToInt(_experienceToNextLevel * 1.2f); // 次レベル必要経験値増加
                _skillPoints += 1;
                _abilityPoints += 1;
                leveledUp = true;
            }

            if (leveledUp)
            {
                var levelUpData = new Events.LevelUpData(
                    newLevel: _playerLevel,
                    skillPointsGained: 1,
                    abilityPointsGained: 1,
                    experienceGained: amount
                );

                var levelUpEvent = Resources.Load<Events.LevelUpEvent>("Events/LevelUpEvent");
                levelUpEvent?.RaiseLevelUp(levelUpData);
            }

            return leveledUp;
        }

        /// <summary>
        /// スキル習得
        /// </summary>
        public bool LearnSkill(PlayerSkill skill)
        {
            if (_skillPoints < skill.RequiredSkillPoints) return false;
            if (_skills.Contains(skill)) return false;

            _skills.Add(skill);
            _skillPoints -= skill.RequiredSkillPoints;
            return true;
        }

        /// <summary>
        /// 能力習得
        /// </summary>
        public bool LearnAbility(PlayerAbility ability)
        {
            if (_abilityPoints < ability.RequiredAbilityPoints) return false;
            if (_abilities.Contains(ability)) return false;

            _abilities.Add(ability);
            _abilityPoints -= ability.RequiredAbilityPoints;
            return true;
        }

        /// <summary>
        /// アイテム所持数取得
        /// </summary>
        public int GetItemCount(string itemId)
        {
            return _itemCounts.TryGetValue(itemId, out int count) ? count : 0;
        }

        /// <summary>
        /// アイテム追加
        /// </summary>
        public bool AddItem(string itemId, int count = 1)
        {
            if (_itemCounts.ContainsKey(itemId))
            {
                _itemCounts[itemId] += count;
            }
            else
            {
                _itemCounts[itemId] = count;
            }

            return true;
        }

        /// <summary>
        /// アイテム削除
        /// </summary>
        public bool RemoveItem(string itemId, int count = 1)
        {
            if (!_itemCounts.ContainsKey(itemId)) return false;

            _itemCounts[itemId] -= count;
            if (_itemCounts[itemId] <= 0)
            {
                _itemCounts.Remove(itemId);
            }

            return true;
        }

        /// <summary>
        /// 特定スキルを持っているか判定
        /// </summary>
        public bool HasSkill(string skillName)
        {
            return _skills.Exists(skill => skill.SkillName == skillName);
        }

        /// <summary>
        /// 特定能力を持っているか判定
        /// </summary>
        public bool HasAbility(string abilityName)
        {
            return _abilities.Exists(ability => ability.AbilityName == abilityName);
        }

        /// <summary>
        /// クリティカル判定
        /// </summary>
        public bool IsCriticalHit()
        {
            return Random.value <= _criticalChance;
        }

        /// <summary>
        /// 実際のダメージ計算（クリティカル・防御力適用）
        /// </summary>
        public float CalculateActualDamage(float baseDamage)
        {
            float damage = baseDamage * _damageMultiplier;

            if (IsCriticalHit())
            {
                damage *= _criticalMultiplier;
            }

            return damage;
        }

        /// <summary>
        /// 被ダメージ計算（防御力適用）
        /// </summary>
        public float CalculateDamageReceived(float incomingDamage)
        {
            if (_godMode) return 0f;

            return incomingDamage / _defenseMultiplier;
        }

        /// <summary>
        /// データリセット
        /// </summary>
        public void ResetToDefaults()
        {
            _currentHealth = _maxHealth;
            _currentStamina = _maxStamina;
            _shield = 0f;
            _experience = 0;
            _playerLevel = 1;
            _skillPoints = 0;
            _abilityPoints = 0;
            _skills.Clear();
            _abilities.Clear();
            _itemCounts.Clear();
            _statistics = new PlayerStatistics();
        }

        /// <summary>
        /// データ検証
        /// </summary>
        public bool ValidateData()
        {
            bool isValid = true;

            if (_maxHealth <= 0f)
            {
                Debug.LogError($"[PlayerData] Max health must be greater than 0 in {name}");
                isValid = false;
            }

            if (_walkSpeed <= 0f)
            {
                Debug.LogError($"[PlayerData] Walk speed must be greater than 0 in {name}");
                isValid = false;
            }

            if (_inventoryCapacity < 0)
            {
                Debug.LogError($"[PlayerData] Inventory capacity cannot be negative in {name}");
                isValid = false;
            }

            return isValid;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying) return;

            // 値の範囲制限
            _currentHealth = Mathf.Clamp(_currentHealth, 0f, _maxHealth);
            _currentStamina = Mathf.Clamp(_currentStamina, 0f, _maxStamina);
            _shield = Mathf.Clamp(_shield, 0f, _maxShield);
            _criticalChance = Mathf.Clamp01(_criticalChance);

            ValidateData();
        }
#endif
    }

    /// <summary>
    /// プレイヤースキルデータ
    /// </summary>
    [System.Serializable]
    public class PlayerSkill
    {
        public string SkillName;
        public string Description;
        public int RequiredSkillPoints;
        public int MaxLevel;
        public int CurrentLevel;
        public Sprite SkillIcon;
    }

    /// <summary>
    /// プレイヤー能力データ
    /// </summary>
    [System.Serializable]
    public class PlayerAbility
    {
        public string AbilityName;
        public string Description;
        public int RequiredAbilityPoints;
        public float Cooldown;
        public float Duration;
        public Sprite AbilityIcon;
    }

    /// <summary>
    /// プレイヤー統計情報
    /// </summary>
    [System.Serializable]
    public class PlayerStatistics
    {
        public int TotalKills;
        public int TotalDeaths;
        public float TotalDamageDealt;
        public float TotalDamageTaken;
        public int ShotsHit;
        public int ShotsFired;
        public float TotalPlayTime;
        public int LevelsCompleted;
        public int ItemsCollected;

        public float KDRatio => TotalDeaths > 0 ? (float)TotalKills / TotalDeaths : TotalKills;
        public float Accuracy => ShotsFired > 0 ? (float)ShotsHit / ShotsFired : 0f;
    }

    /// <summary>
    /// アイテムデータ（簡易版）
    /// </summary>
    [System.Serializable]
    public class ItemData
    {
        public string ItemId;
        public string ItemName;
        public int Quantity;
        public Sprite ItemIcon;
    }
}