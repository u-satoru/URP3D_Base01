using UnityEngine;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Features.Templates.FPS.Weapons
{
    /// <summary>
    /// FPS武器の設定データ（ScriptableObject）
    /// ダメージ、射撃間隔、弾薬容量などの武器パラメータを定義
    /// </summary>
    [CreateAssetMenu(fileName = "WeaponData_", menuName = "asterivo/FPS Template/Weapon Data", order = 1)]
    public class WeaponData : ScriptableObject
    {
        [TabGroup("Basic", "Weapon Info")]
        [LabelText("Weapon Name")]
        [SerializeField] private string weaponName = "Assault Rifle";
        
        [TabGroup("Basic", "Weapon Info")]
        [LabelText("Weapon Type")]
        [SerializeField] private WeaponType weaponType = WeaponType.AssaultRifle;
        
        [TabGroup("Basic", "Combat")]
        [BoxGroup("Basic/Combat/Damage")]
        [LabelText("Base Damage")]
        [PropertyRange(10f, 200f)]
        [SerializeField] private float baseDamage = 30f;
        
        [BoxGroup("Basic/Combat/Damage")]
        [LabelText("Headshot Multiplier")]
        [PropertyRange(1.5f, 3f)]
        [SerializeField] private float headshotMultiplier = 2f;
        
        [TabGroup("Basic", "Firing")]
        [BoxGroup("Basic/Firing/Rate")]
        [LabelText("Fire Rate (RPM)")]
        [PropertyRange(60f, 1200f)]
        [SerializeField] private float fireRate = 600f;
        
        [BoxGroup("Basic/Firing/Rate")]
        [LabelText("Fire Mode")]
        [SerializeField] private FireMode fireMode = FireMode.Automatic;
        
        [BoxGroup("Basic/Firing/Rate")]
        [LabelText("Burst Count")]
        [ShowIf("fireMode", FireMode.Burst)]
        [PropertyRange(2, 5)]
        [SerializeField] private int burstCount = 3;
        
        [TabGroup("Basic", "Ammo")]
        [BoxGroup("Basic/Ammo/Capacity")]
        [LabelText("Magazine Size")]
        [PropertyRange(5, 100)]
        [SerializeField] private int magazineSize = 30;
        
        [BoxGroup("Basic/Ammo/Capacity")]
        [LabelText("Max Reserve Ammo")]
        [PropertyRange(60, 500)]
        [SerializeField] private int maxReserveAmmo = 210;
        
        [BoxGroup("Basic/Ammo/Reload")]
        [LabelText("Reload Time")]
        [PropertyRange(0.5f, 5f)]
        [SuffixLabel("s")]
        [SerializeField] private float reloadTime = 2.5f;
        
        [TabGroup("Advanced", "Accuracy")]
        [BoxGroup("Advanced/Accuracy/Spread")]
        [LabelText("Base Accuracy")]
        [PropertyRange(0.01f, 0.2f)]
        [SerializeField] private float baseAccuracy = 0.05f;
        
        [BoxGroup("Advanced/Accuracy/Spread")]
        [LabelText("Moving Penalty")]
        [PropertyRange(1f, 3f)]
        [SerializeField] private float movingAccuracyPenalty = 1.5f;
        
        [BoxGroup("Advanced/Accuracy/Spread")]
        [LabelText("Aiming Improvement")]
        [PropertyRange(2f, 5f)]
        [SerializeField] private float aimingAccuracyImprovement = 3f;
        
        [TabGroup("Advanced", "Range")]
        [BoxGroup("Advanced/Range/Distance")]
        [LabelText("Effective Range")]
        [PropertyRange(20f, 300f)]
        [SuffixLabel("m")]
        [SerializeField] private float effectiveRange = 100f;
        
        [BoxGroup("Advanced/Range/Distance")]
        [LabelText("Max Range")]
        [PropertyRange(50f, 500f)]
        [SuffixLabel("m")]
        [SerializeField] private float maxRange = 200f;
        
        [TabGroup("Advanced", "Audio")]
        [BoxGroup("Advanced/Audio/Sound")]
        [LabelText("Fire Sound")]
        [SerializeField] private AudioClip fireSound;
        
        [BoxGroup("Advanced/Audio/Sound")]
        [LabelText("Reload Sound")]
        [SerializeField] private AudioClip reloadSound;
        
        [BoxGroup("Advanced/Audio/Sound")]
        [LabelText("Empty Sound")]
        [SerializeField] private AudioClip emptySound;
        
        [BoxGroup("Advanced/Audio/Volume")]
        [LabelText("Audio Volume")]
        [PropertyRange(0.1f, 1f)]
        [SerializeField] private float audioVolume = 0.8f;
        
        [TabGroup("Effects", "Visual")]
        [LabelText("Muzzle Flash")]
        [SerializeField] private GameObject muzzleFlashPrefab;
        
        [LabelText("Shell Ejection")]
        [SerializeField] private GameObject shellEjectionPrefab;
        
        [LabelText("Impact Effect")]
        [SerializeField] private GameObject impactEffectPrefab;
        
        [TabGroup("Effects", "Recoil")]
        [BoxGroup("Effects/Recoil/Pattern")]
        [LabelText("Vertical Recoil")]
        [PropertyRange(0.1f, 2f)]
        [SerializeField] private float verticalRecoil = 0.5f;
        
        [BoxGroup("Effects/Recoil/Pattern")]
        [LabelText("Horizontal Recoil")]
        [PropertyRange(0.05f, 1f)]
        [SerializeField] private float horizontalRecoil = 0.2f;
        
        [BoxGroup("Effects/Recoil/Recovery")]
        [LabelText("Recoil Recovery Speed")]
        [PropertyRange(1f, 10f)]
        [SerializeField] private float recoilRecoverySpeed = 4f;
        
        // Public Properties
        public string WeaponName => weaponName;
        public WeaponType WeaponType => weaponType;
        public float BaseDamage => baseDamage;
        public float HeadshotMultiplier => headshotMultiplier;
        public float FireRate => fireRate;
        public FireMode FireMode => fireMode;
        public int BurstCount => burstCount;
        public int MagazineSize => magazineSize;
        public int MaxReserveAmmo => maxReserveAmmo;
        public float ReloadTime => reloadTime;
        public float BaseAccuracy => baseAccuracy;
        public float MovingAccuracyPenalty => movingAccuracyPenalty;
        public float AimingAccuracyImprovement => aimingAccuracyImprovement;
        public float EffectiveRange => effectiveRange;
        public float MaxRange => maxRange;
        public AudioClip FireSound => fireSound;
        public AudioClip ReloadSound => reloadSound;
        public AudioClip EmptySound => emptySound;
        public float AudioVolume => audioVolume;
        public GameObject MuzzleFlashPrefab => muzzleFlashPrefab;
        public GameObject ShellEjectionPrefab => shellEjectionPrefab;
        public GameObject ImpactEffectPrefab => impactEffectPrefab;
        public float VerticalRecoil => verticalRecoil;
        public float HorizontalRecoil => horizontalRecoil;
        public float RecoilRecoverySpeed => recoilRecoverySpeed;
        
        /// <summary>
        /// 1発あたりの射撃間隔（秒）
        /// </summary>
        public float FireInterval => 60f / fireRate;
    }
    
    public enum WeaponType
    {
        Pistol,
        AssaultRifle,
        Shotgun,
        SniperRifle,
        SMG,
        LMG
    }
    
    public enum FireMode
    {
        SemiAutomatic,
        Automatic,
        Burst
    }
}