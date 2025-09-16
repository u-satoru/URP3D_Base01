using UnityEngine;
using System.Collections.Generic;

namespace asterivo.Unity60.Features.Templates.FPS.Data
{
    /// <summary>
    /// 武器データ設定
    /// ServiceLocator + Event駆動のハイブリッドアーキテクチャ準拠
    /// ScriptableObjectベースのデータ管理システム
    /// ノンプログラマーによるゲームバランス調整対応
    /// </summary>
    [CreateAssetMenu(fileName = "WeaponData", menuName = "FPS Template/Data/Weapon Data", order = 1)]
    public class WeaponData : ScriptableObject
    {
        [Header("基本設定")]
        [SerializeField] private string _weaponName = "Default Weapon";
        [SerializeField] private Events.WeaponType _weaponType = Events.WeaponType.AssaultRifle;
        [SerializeField] private string _description = "";
        [SerializeField] private Sprite _weaponIcon;
        [SerializeField] private GameObject _weaponPrefab;
        [SerializeField] private GameObject _worldModelPrefab;

        [Header("戦闘設定")]
        [SerializeField] private float _damage = 25f;
        [SerializeField] private Events.DamageType _damageType = Events.DamageType.Bullet;
        [SerializeField] private float _fireRate = 600f; // 発射速度（RPM）
        [SerializeField] private float _range = 100f;
        [SerializeField] private float _accuracy = 0.95f;
        [SerializeField] private float _recoil = 1.0f;
        [SerializeField] private bool _isAutomatic = true;

        [Header("弾薬設定")]
        [SerializeField] private int _magazineSize = 30;
        [SerializeField] private int _maxReserveAmmo = 300;
        [SerializeField] private float _reloadTime = 2.5f;
        [SerializeField] private Events.WeaponType _ammoType;

        [Header("エフェクト設定")]
        [SerializeField] private GameObject _muzzleFlashPrefab;
        [SerializeField] private GameObject _bulletTracerPrefab;
        [SerializeField] private GameObject _shellCasingPrefab;
        [SerializeField] private ParticleSystem _smokeEffect;
        [SerializeField] private Light _muzzleLight;

        [Header("オーディオ設定")]
        [SerializeField] private string _fireSound = "WeaponFire";
        [SerializeField] private string _reloadSound = "WeaponReload";
        [SerializeField] private string _emptyFireSound = "WeaponEmpty";
        [SerializeField] private string _switchSound = "WeaponSwitch";
        [SerializeField] private AudioClip _fireSoundClip;
        [SerializeField] private AudioClip _reloadSoundClip;

        [Header("アニメーション設定")]
        [SerializeField] private AnimationClip _fireAnimation;
        [SerializeField] private AnimationClip _reloadAnimation;
        [SerializeField] private AnimationClip _idleAnimation;
        [SerializeField] private AnimationClip _drawAnimation;
        [SerializeField] private AnimationClip _holsterAnimation;

        [Header("UI設定")]
        [SerializeField] private Color _crosshairColor = Color.white;
        [SerializeField] private Sprite _crosshairSprite;
        [SerializeField] private Vector2 _crosshairSize = Vector2.one;
        [SerializeField] private float _aimFOV = 45f;
        [SerializeField] private float _aimSensitivity = 0.5f;

        [Header("物理設定")]
        [SerializeField] private float _weight = 3.5f;
        [SerializeField] private Vector3 _centerOfMass = Vector3.zero;
        [SerializeField] private float _stabilityFactor = 1.0f;

        [Header("特殊能力")]
        [SerializeField] private List<WeaponAbility> _abilities = new();
        [SerializeField] private List<WeaponAttachment> _availableAttachments = new();

        [Header("経済設定")]
        [SerializeField] private int _cost = 100;
        [SerializeField] private int _sellValue = 50;
        [SerializeField] private bool _canBeSold = true;
        [SerializeField] private bool _isUnlocked = true;

        [Header("レアリティ・分類")]
        [SerializeField] private WeaponRarity _rarity = WeaponRarity.Common;
        [SerializeField] private WeaponClass _weaponClass = WeaponClass.Primary;
        [SerializeField] private List<string> _tags = new();

        // プロパティ（読み取り専用）
        public string WeaponName => _weaponName;
        public Events.WeaponType WeaponType => _weaponType;
        public string Description => _description;
        public Sprite WeaponIcon => _weaponIcon;
        public GameObject WeaponPrefab => _weaponPrefab;
        public GameObject WorldModelPrefab => _worldModelPrefab;

        // 戦闘データ
        public float Damage => _damage;
        public Events.DamageType DamageType => _damageType;
        public float FireRate => _fireRate;
        public float FireRatePerSecond => _fireRate / 60f;
        public float TimeBetweenShots => 60f / _fireRate;
        public float Range => _range;
        public float Accuracy => _accuracy;
        public float Recoil => _recoil;
        public bool IsAutomatic => _isAutomatic;

        // 弾薬データ
        public int MagazineSize => _magazineSize;
        public int MaxReserveAmmo => _maxReserveAmmo;
        public float ReloadTime => _reloadTime;
        public Events.WeaponType AmmoType => _ammoType;

        // エフェクトデータ
        public GameObject MuzzleFlashPrefab => _muzzleFlashPrefab;
        public GameObject BulletTracerPrefab => _bulletTracerPrefab;
        public GameObject ShellCasingPrefab => _shellCasingPrefab;
        public ParticleSystem SmokeEffect => _smokeEffect;
        public Light MuzzleLight => _muzzleLight;

        // オーディオデータ
        public string FireSound => _fireSound;
        public string ReloadSound => _reloadSound;
        public string EmptyFireSound => _emptyFireSound;
        public string SwitchSound => _switchSound;
        public AudioClip FireSoundClip => _fireSoundClip;
        public AudioClip ReloadSoundClip => _reloadSoundClip;

        // アニメーションデータ
        public AnimationClip FireAnimation => _fireAnimation;
        public AnimationClip ReloadAnimation => _reloadAnimation;
        public AnimationClip IdleAnimation => _idleAnimation;
        public AnimationClip DrawAnimation => _drawAnimation;
        public AnimationClip HolsterAnimation => _holsterAnimation;

        // UIデータ
        public Color CrosshairColor => _crosshairColor;
        public Sprite CrosshairSprite => _crosshairSprite;
        public Vector2 CrosshairSize => _crosshairSize;
        public float AimFOV => _aimFOV;
        public float AimSensitivity => _aimSensitivity;

        // 物理データ
        public float Weight => _weight;
        public Vector3 CenterOfMass => _centerOfMass;
        public float StabilityFactor => _stabilityFactor;

        // 特殊能力・拡張
        public IReadOnlyList<WeaponAbility> Abilities => _abilities.AsReadOnly();
        public IReadOnlyList<WeaponAttachment> AvailableAttachments => _availableAttachments.AsReadOnly();

        // 経済データ
        public int Cost => _cost;
        public int SellValue => _sellValue;
        public bool CanBeSold => _canBeSold;
        public bool IsUnlocked => _isUnlocked;

        // 分類データ
        public WeaponRarity Rarity => _rarity;
        public WeaponClass WeaponClass => _weaponClass;
        public IReadOnlyList<string> Tags => _tags.AsReadOnly();

        /// <summary>
        /// 武器の有効射程内判定
        /// </summary>
        public bool IsInRange(float distance) => distance <= _range;

        /// <summary>
        /// 精度による命中判定（0-1の確率）
        /// </summary>
        public bool IsHit(float distance)
        {
            float accuracyAtDistance = _accuracy * (1f - (distance / _range) * 0.5f);
            return Random.value <= accuracyAtDistance;
        }

        /// <summary>
        /// 距離による実際のダメージ計算
        /// </summary>
        public float GetDamageAtDistance(float distance)
        {
            if (!IsInRange(distance)) return 0f;

            // 距離による威力減衰（最大50%まで）
            float damageMultiplier = Mathf.Lerp(1f, 0.5f, distance / _range);
            return _damage * damageMultiplier;
        }

        /// <summary>
        /// 反動による弾道分散計算
        /// </summary>
        public Vector3 GetRecoilSpread()
        {
            float spreadX = Random.Range(-_recoil, _recoil);
            float spreadY = Random.Range(-_recoil, _recoil);
            float spreadZ = Random.Range(-_recoil * 0.5f, _recoil * 0.5f);

            return new Vector3(spreadX, spreadY, spreadZ);
        }

        /// <summary>
        /// 特定の能力を持っているか判定
        /// </summary>
        public bool HasAbility(string abilityName)
        {
            return _abilities.Exists(ability => ability.AbilityName == abilityName);
        }

        /// <summary>
        /// 特定のアタッチメントが取り付け可能か判定
        /// </summary>
        public bool CanAttach(WeaponAttachment attachment)
        {
            return _availableAttachments.Contains(attachment);
        }

        /// <summary>
        /// 武器がタグを持っているか判定
        /// </summary>
        public bool HasTag(string tag)
        {
            return _tags.Contains(tag);
        }

        /// <summary>
        /// データ検証（Editor用）
        /// </summary>
        public bool ValidateData()
        {
            bool isValid = true;

            if (string.IsNullOrEmpty(_weaponName))
            {
                Debug.LogError($"[WeaponData] Weapon name is empty in {name}");
                isValid = false;
            }

            if (_damage <= 0f)
            {
                Debug.LogError($"[WeaponData] Damage must be greater than 0 in {name}");
                isValid = false;
            }

            if (_fireRate <= 0f)
            {
                Debug.LogError($"[WeaponData] Fire rate must be greater than 0 in {name}");
                isValid = false;
            }

            if (_magazineSize <= 0)
            {
                Debug.LogError($"[WeaponData] Magazine size must be greater than 0 in {name}");
                isValid = false;
            }

            if (_reloadTime <= 0f)
            {
                Debug.LogError($"[WeaponData] Reload time must be greater than 0 in {name}");
                isValid = false;
            }

            return isValid;
        }

        /// <summary>
        /// 武器統計情報取得
        /// </summary>
        public WeaponStats GetWeaponStats()
        {
            return new WeaponStats
            {
                DPS = _damage * FireRatePerSecond,
                DPM = _damage * _magazineSize, // Damage Per Magazine
                ReloadRatio = _reloadTime / (_magazineSize / FireRatePerSecond),
                EffectiveRange = _range * _accuracy,
                Mobility = 1f / _weight,
                OverallRating = CalculateOverallRating()
            };
        }

        /// <summary>
        /// 総合評価計算
        /// </summary>
        private float CalculateOverallRating()
        {
            var stats = GetWeaponStats();

            // 各要素の重み付け評価（正規化）
            float damageScore = Mathf.Clamp01(_damage / 100f) * 0.25f;
            float rateScore = Mathf.Clamp01(FireRatePerSecond / 20f) * 0.20f;
            float accuracyScore = _accuracy * 0.25f;
            float rangeScore = Mathf.Clamp01(_range / 200f) * 0.15f;
            float mobilityScore = Mathf.Clamp01(stats.Mobility / 0.5f) * 0.15f;

            return damageScore + rateScore + accuracyScore + rangeScore + mobilityScore;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Inspector表示用デバッグ情報
        /// </summary>
        [Space]
        [Header("デバッグ情報（読み取り専用）")]
        [SerializeField] private WeaponStats _debugStats;

        private void OnValidate()
        {
            if (Application.isPlaying) return;

            _debugStats = GetWeaponStats();
            ValidateData();
        }
#endif
    }

    /// <summary>
    /// 武器能力データ
    /// </summary>
    [System.Serializable]
    public class WeaponAbility
    {
        public string AbilityName;
        public string Description;
        public float Value;
        public bool IsActive;
    }

    /// <summary>
    /// 武器アタッチメントデータ
    /// </summary>
    [System.Serializable]
    public class WeaponAttachment
    {
        public string AttachmentName;
        public AttachmentType Type;
        public GameObject AttachmentPrefab;
        public WeaponStatModifier StatModifier;
    }

    /// <summary>
    /// 武器統計情報
    /// </summary>
    [System.Serializable]
    public class WeaponStats
    {
        public float DPS;           // Damage Per Second
        public float DPM;           // Damage Per Magazine
        public float ReloadRatio;   // リロード効率
        public float EffectiveRange;// 有効射程
        public float Mobility;      // 機動性
        public float OverallRating; // 総合評価
    }

    /// <summary>
    /// 武器ステータス修正データ
    /// </summary>
    [System.Serializable]
    public class WeaponStatModifier
    {
        public float DamageMultiplier = 1f;
        public float FireRateMultiplier = 1f;
        public float AccuracyMultiplier = 1f;
        public float RangeMultiplier = 1f;
        public float RecoilMultiplier = 1f;
        public float ReloadTimeMultiplier = 1f;
    }

    /// <summary>
    /// 武器レアリティ
    /// </summary>
    public enum WeaponRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary,
        Artifact
    }

    /// <summary>
    /// 武器クラス
    /// </summary>
    public enum WeaponClass
    {
        Primary,    // メイン武器
        Secondary,  // サブ武器
        Melee,      // 近接武器
        Throwable,  // 投擲武器
        Special     // 特殊武器
    }

    /// <summary>
    /// アタッチメントタイプ
    /// </summary>
    public enum AttachmentType
    {
        Scope,      // スコープ
        Barrel,     // バレル
        Grip,       // グリップ
        Stock,      // ストック
        Magazine,   // マガジン
        Laser,      // レーザー
        Flashlight, // フラッシュライト
        Suppressor  // サプレッサー
    }
}