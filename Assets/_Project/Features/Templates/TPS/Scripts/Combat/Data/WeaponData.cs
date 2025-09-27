using UnityEngine;

namespace asterivo.Unity60.Features.Templates.TPS.Combat.Data
{
    /// <summary>
    /// ScriptableObject containing weapon configuration data
    /// Follows the project's data-driven design principles
    /// </summary>
    [CreateAssetMenu(fileName = "New Weapon Data", menuName = "TPS Template/Combat/Weapon Data")]
    public class WeaponData : ScriptableObject
    {
        [Header("Basic Properties")]
        [SerializeField] private string _weaponName = "Default Weapon";
        [SerializeField] private WeaponType _weaponType = WeaponType.AssaultRifle;
        [SerializeField] private GameObject _weaponPrefab;
        [SerializeField] private Sprite _weaponIcon;

        [Header("Damage Properties")]
        [SerializeField] private float _baseDamage = 25f;
        [SerializeField] private float _headShotMultiplier = 2.0f;
        [SerializeField] private float _damageRange = 100f;
        [SerializeField] private AnimationCurve _damageFalloff = AnimationCurve.Linear(0f, 1f, 1f, 0.5f);

        [Header("Fire Rate & Accuracy")]
        [SerializeField] private float _fireRate = 600f; // Rounds per minute
        [SerializeField] private FireMode _fireMode = FireMode.Automatic;
        [SerializeField] private float _accuracy = 0.9f; // 0-1 scale
        [SerializeField] private float _recoil = 0.1f;
        [SerializeField] private float _spreadAngle = 2f; // Degrees

        [Header("Ammunition")]
        [SerializeField] private int _magazineSize = 30;
        [SerializeField] private int _maxAmmo = 300;
        [SerializeField] private float _reloadTime = 2.5f;
        [SerializeField] private AmmoType _ammoType = AmmoType.Rifle;

        [Header("Handling")]
        [SerializeField] private float _equipTime = 0.5f;
        [SerializeField] private float _aimDownSightTime = 0.3f;
        [SerializeField] private float _movementSpeedMultiplier = 0.8f;
        [SerializeField] private float _aimMovementSpeedMultiplier = 0.5f;

        [Header("Audio")]
        [SerializeField] private AudioClip _fireSound;
        [SerializeField] private AudioClip _reloadSound;
        [SerializeField] private AudioClip _equipSound;
        [SerializeField] private AudioClip _dryFireSound;

        [Header("Effects")]
        [SerializeField] private GameObject _muzzleFlashPrefab;
        [SerializeField] private GameObject _bulletImpactPrefab;
        [SerializeField] private GameObject _shellEjectPrefab;

        // Properties for external access
        public string WeaponName => _weaponName;
        public WeaponType WeaponType => _weaponType;
        public GameObject WeaponPrefab => _weaponPrefab;
        public Sprite WeaponIcon => _weaponIcon;
        
        public float BaseDamage => _baseDamage;
        public float HeadShotMultiplier => _headShotMultiplier;
        public float DamageRange => _damageRange;
        public AnimationCurve DamageFalloff => _damageFalloff;
        
        public float FireRate => _fireRate;
        public float FireInterval => 60f / _fireRate; // Time between shots
        public FireMode FireMode => _fireMode;
        public float Accuracy => _accuracy;
        public float Recoil => _recoil;
        public float SpreadAngle => _spreadAngle;
        
        public int MagazineSize => _magazineSize;
        public int MaxAmmo => _maxAmmo;
        public float ReloadTime => _reloadTime;
        public AmmoType AmmoType => _ammoType;
        
        public float EquipTime => _equipTime;
        public float AimDownSightTime => _aimDownSightTime;
        public float MovementSpeedMultiplier => _movementSpeedMultiplier;
        public float AimMovementSpeedMultiplier => _aimMovementSpeedMultiplier;
        
        public AudioClip FireSound => _fireSound;
        public AudioClip ReloadSound => _reloadSound;
        public AudioClip EquipSound => _equipSound;
        public AudioClip DryFireSound => _dryFireSound;
        
        public GameObject MuzzleFlashPrefab => _muzzleFlashPrefab;
        public GameObject BulletImpactPrefab => _bulletImpactPrefab;
        public GameObject ShellEjectPrefab => _shellEjectPrefab;

        /// <summary>
        /// Calculate damage at specific distance
        /// </summary>
        public float GetDamageAtDistance(float distance)
        {
            if (distance > _damageRange) return 0f;
            
            float normalizedDistance = distance / _damageRange;
            float damageMultiplier = _damageFalloff.Evaluate(normalizedDistance);
            return _baseDamage * damageMultiplier;
        }

        /// <summary>
        /// Validate weapon data in editor
        /// </summary>
        private void OnValidate()
        {
            _baseDamage = Mathf.Max(0f, _baseDamage);
            _headShotMultiplier = Mathf.Max(1f, _headShotMultiplier);
            _damageRange = Mathf.Max(1f, _damageRange);
            _fireRate = Mathf.Max(1f, _fireRate);
            _accuracy = Mathf.Clamp01(_accuracy);
            _recoil = Mathf.Max(0f, _recoil);
            _spreadAngle = Mathf.Clamp(_spreadAngle, 0f, 45f);
            _magazineSize = Mathf.Max(1, _magazineSize);
            _maxAmmo = Mathf.Max(_magazineSize, _maxAmmo);
            _reloadTime = Mathf.Max(0.1f, _reloadTime);
            _equipTime = Mathf.Max(0.1f, _equipTime);
            _aimDownSightTime = Mathf.Max(0.1f, _aimDownSightTime);
            _movementSpeedMultiplier = Mathf.Clamp01(_movementSpeedMultiplier);
            _aimMovementSpeedMultiplier = Mathf.Clamp01(_aimMovementSpeedMultiplier);
        }
    }

    /// <summary>
    /// Weapon type enumeration
    /// </summary>
    public enum WeaponType
    {
        AssaultRifle,
        Pistol,
        Shotgun,
        SniperRifle,
        SubmachineGun,
        LightMachineGun,
        Grenade,
        Melee
    }

    /// <summary>
    /// Fire mode enumeration
    /// </summary>
    public enum FireMode
    {
        Single,      // Semi-automatic
        Burst,       // 3-round burst
        Automatic    // Full automatic
    }

    /// <summary>
    /// Ammunition type enumeration
    /// </summary>
    public enum AmmoType
    {
        Pistol,
        Rifle,
        Shotgun,
        Sniper,
        Grenade
    }
}
