using UnityEngine;

namespace asterivo.Unity60.Features.Templates.FPS.Data
{
    /// <summary>
    /// FPS Template用武器データ
    /// 武器の基本性能、弾薬情報、射撃設定等を管理
    /// </summary>
    [CreateAssetMenu(menuName = "FPS/Data/Weapon Data")]
    public class WeaponData : ScriptableObject
    {
        [Header("Basic Weapon Info")]
        [SerializeField] private string _weaponName = "Default Weapon";
        [SerializeField] private WeaponType _weaponType = WeaponType.AssaultRifle;
        [SerializeField] private Sprite _weaponIcon;
        [SerializeField] private GameObject _weaponPrefab;

        [Header("Damage Settings")]
        [SerializeField] private float _baseDamage = 25f;
        [SerializeField] private float _headShotMultiplier = 2f;
        [SerializeField] private float _maxRange = 100f;
        [SerializeField] private AnimationCurve _damageFalloffCurve = AnimationCurve.Linear(0f, 1f, 1f, 0.3f);

        [Header("Fire Rate & Accuracy")]
        [SerializeField] private float _fireRate = 600f; // Rounds per minute
        [SerializeField] private float _baseAccuracy = 0.95f;
        [SerializeField] private float _recoilForce = 1f;
        [SerializeField] private RecoilPattern _recoilPattern;

        [Header("Ammunition")]
        [SerializeField] private int _magazineSize = 30;
        [SerializeField] private int _maxAmmo = 300;
        [SerializeField] private float _reloadTime = 2.5f;
        [SerializeField] private bool _allowPartialReload = true;

        [Header("Projectile Settings")]
        [SerializeField] private float _projectileSpeed = 300f;
        [SerializeField] private bool _isHitscan = true;
        [SerializeField] private LayerMask _hitLayers = -1;
        [SerializeField] private ShotType _shotType = ShotType.HitScan;
        [SerializeField] private FireMode _fireMode = FireMode.FullAuto;

        [Header("Audio & Effects")]
        [SerializeField] private AudioClip _fireSound;
        [SerializeField] private AudioClip _reloadSound;
        [SerializeField] private AudioClip _dryFireSound;
        [SerializeField] private GameObject _muzzleFlashEffect;
        [SerializeField] private GameObject _impactEffect;

        [Header("Weapon Stats")]
        [SerializeField] private WeaponStats _stats;

        // Properties
        public string WeaponName => _weaponName;
        public WeaponType WeaponType => _weaponType;
        public Sprite WeaponIcon => _weaponIcon;
        public GameObject WeaponPrefab => _weaponPrefab;
        public float BaseDamage => _baseDamage;
        public float HeadShotMultiplier => _headShotMultiplier;
        public float MaxRange => _maxRange;
        public AnimationCurve DamageFalloffCurve => _damageFalloffCurve;
        public float FireRate => _fireRate;
        public float BaseAccuracy => _baseAccuracy;
        public float RecoilForce => _recoilForce;
        public RecoilPattern RecoilPattern => _recoilPattern;
        public int MagazineSize => _magazineSize;
        public int MaxAmmo => _maxAmmo;
        public float ReloadTime => _reloadTime;
        public bool AllowPartialReload => _allowPartialReload;
        public float ProjectileSpeed => _projectileSpeed;
        public bool IsHitscan => _isHitscan;
        public LayerMask HitLayers => _hitLayers;
        public ShotType ShotType => _shotType;
        public FireMode FireMode => _fireMode;
        public AudioClip FireSound => _fireSound;
        public AudioClip ReloadSound => _reloadSound;
        public AudioClip DryFireSound => _dryFireSound;
        public GameObject MuzzleFlashEffect => _muzzleFlashEffect;
        public GameObject MuzzleFlashPrefab => _muzzleFlashEffect; // Alias for compatibility
        public GameObject ImpactEffect => _impactEffect;
        public WeaponStats Stats => _stats;

        // AmmoConfig property
        public AmmoConfig AmmoConfig => new AmmoConfig
        {
            magazineSize = _magazineSize,
            totalAmmo = _maxAmmo,
            ammoType = AmmoType.Rifle, // Default ammo type
            reloadTime = _reloadTime,
            tacticalReloadTime = _reloadTime * 0.8f
        };

        /// <summary>
        /// 発射間隔を取得（秒単位）
        /// </summary>
        public float GetFireInterval()
        {
            return 60f / _fireRate;
        }

        /// <summary>
        /// 発射間隔を取得（秒単位） - GetTimeBetweenShotsのエイリアス
        /// </summary>
        public float GetTimeBetweenShots()
        {
            return GetFireInterval();
        }

        /// <summary>
        /// フルオート武器かどうかを判定
        /// </summary>
        public bool IsFullAuto()
        {
            return _fireMode == FireMode.FullAuto;
        }

        /// <summary>
        /// 距離に応じたダメージを計算
        /// </summary>
        public float GetDamageAtDistance(float distance)
        {
            if (distance > _maxRange)
                return 0f;

            float normalizedDistance = distance / _maxRange;
            float falloffMultiplier = _damageFalloffCurve.Evaluate(normalizedDistance);
            return _baseDamage * falloffMultiplier;
        }

        /// <summary>
        /// 武器の有効射程を取得
        /// </summary>
        public float GetEffectiveRange()
        {
            // ダメージが50%以上残る距離を有効射程とする
            for (float distance = 0f; distance <= _maxRange; distance += 1f)
            {
                float damage = GetDamageAtDistance(distance);
                if (damage < _baseDamage * 0.5f)
                {
                    return distance;
                }
            }
            return _maxRange;
        }

        /// <summary>
        /// 武器のDPSを計算
        /// </summary>
        public float GetDPS()
        {
            return (_baseDamage * _fireRate) / 60f;
        }

        /// <summary>
        /// Time To Kill (TTK) を計算（100HPターゲット想定）
        /// </summary>
        public float GetTTK(float targetHealth = 100f)
        {
            if (_baseDamage <= 0f || _fireRate <= 0f)
                return float.MaxValue;

            int shotsNeeded = Mathf.CeilToInt(targetHealth / _baseDamage);
            float fireInterval = 60f / _fireRate;

            // 初弾は即座、以降はfireInterval間隔
            return (shotsNeeded - 1) * fireInterval;
        }

        /// <summary>
        /// 武器統計の更新
        /// </summary>
        public void UpdateStats(int shotsFired, int shotsHit, float damageDealt)
        {
            _stats.shotsFired += shotsFired;
            _stats.shotsHit += shotsHit;
            _stats.damageDealt += damageDealt;
            _stats.accuracy = _stats.shotsFired > 0 ? (float)_stats.shotsHit / _stats.shotsFired : 0f;
        }

        /// <summary>
        /// 武器統計のリセット
        /// </summary>
        [ContextMenu("Reset Stats")]
        public void ResetStats()
        {
            _stats.shotsFired = 0;
            _stats.shotsHit = 0;
            _stats.damageDealt = 0f;
            _stats.accuracy = 0f;
        }
    }

}
