using UnityEngine;

namespace asterivo.Unity60.Features.Templates.FPS.Events
{
    /// <summary>
    /// FPSテンプレート共通イベントタイプ定義
    /// ServiceLocator + Event駆動のハイブリッドアーキテクチャ準拠
    /// 全イベントクラスで使用される共通enum定義
    /// </summary>

    /// <summary>
    /// 入力デバイスタイプ
    /// </summary>
    public enum InputDeviceType
    {
        KeyboardMouse,
        Gamepad,
        Touch,
        VR,
        Unknown
    }

    /// <summary>
    /// 武器タイプ
    /// </summary>
    public enum WeaponType
    {
        AssaultRifle,
        Pistol,
        Shotgun,
        SniperRifle,
        SubmachineGun,
        RocketLauncher,
        GrenadeLauncher,
        Knife,
        Grenade,
        Unknown
    }

    /// <summary>
    /// ダメージタイプ
    /// </summary>
    public enum DamageType
    {
        Bullet,
        Explosion,
        Fire,
        Poison,
        Electric,
        Melee,
        Fall,
        Environmental,
        Unknown
    }

    /// <summary>
    /// エフェクト品質
    /// </summary>
    public enum EffectQuality
    {
        Low,
        Medium,
        High,
        Ultra
    }

    /// <summary>
    /// スクリーンエフェクトタイプ
    /// </summary>
    public enum ScreenEffectType
    {
        DamageFlash,
        LowHealthEffect,
        Flash,
        Blur,
        Desaturate
    }

    /// <summary>
    /// シェイクタイプ
    /// </summary>
    public enum ShakeType
    {
        Random,
        Directional,
        Explosion,
        Impact,
        Custom
    }

    /// <summary>
    /// ターゲット優先度
    /// </summary>
    public enum TargetPriority
    {
        Closest,
        LowestHealth,
        HighestThreat,
        MostRecent
    }

    /// <summary>
    /// カメラ状態タイプ
    /// </summary>
    public enum CameraStateType
    {
        FirstPerson,
        ThirdPerson,
        Aim,
        Cover,
        Cinematic,
        Death
    }

    /// <summary>
    /// 戦闘状態
    /// </summary>
    public enum CombatState
    {
        Idle,
        Engaging,
        InCombat,
        Retreating,
        Victory,
        Defeated
    }

    /// <summary>
    /// 弾薬設定
    /// </summary>
    [System.Serializable]
    public class AmmoConfiguration
    {
        [Header("基本設定")]
        public WeaponType weaponType;
        public int maxAmmo = 30;
        public int maxReserveAmmo = 300;
        public int startingAmmo = 30;
        public int startingReserveAmmo = 90;

        [Header("リロード設定")]
        public float reloadTime = 2.0f;
        public bool canReloadPartially = true;

        [Header("UI設定")]
        public Color ammoColor = Color.white;
        public Color lowAmmoColor = Color.red;
        public float lowAmmoThreshold = 0.25f; // 25%以下で警告

        [Header("音響設定")]
        public string reloadStartSound = "ReloadStart";
        public string reloadCompleteSound = "ReloadComplete";
        public string emptyAmmoSound = "AmmoEmpty";
    }

    /// <summary>
    /// ターゲティング設定
    /// </summary>
    [System.Serializable]
    public class TargetingConfiguration
    {
        [Header("基本設定")]
        public bool enableAutoAim = false;
        public float autoAimRange = 50f;
        public float fieldOfView = 90f;
        public float maxTargetRange = 100f;

        [Header("優先度設定")]
        public TargetPriority targetPriority = TargetPriority.Closest;

        [Header("レイヤー設定")]
        public LayerMask targetLayers = -1;
        public LayerMask obstacleLayers = 1; // Default layer

        [Header("自動照準設定")]
        public float autoAimAngle = 15f;
        public float aimAssistStrength = 0.5f;
        public AnimationCurve aimAssistCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    }

    /// <summary>
    /// エフェクト統計情報
    /// </summary>
    [System.Serializable]
    public class EffectsStatistics
    {
        public bool EffectsEnabled;
        public EffectQuality CurrentQuality;
        public int ActiveEffectCount;
        public float MemoryUsage;
        public float CPUUsage;
        public int ParticleCount;
    }

    /// <summary>
    /// UI設定
    /// </summary>
    [System.Serializable]
    public class UIConfiguration
    {
        [Header("HUD設定")]
        public bool showCrosshair = true;
        public bool showAmmoCounter = true;
        public bool showHealthBar = true;
        public bool showMinimap = true;

        [Header("通知設定")]
        public float notificationDuration = 3f;
        public int maxNotifications = 5;
        public bool playNotificationSounds = true;

        [Header("色設定")]
        public Color healthColor = Color.green;
        public Color lowHealthColor = Color.red;
        public Color ammoColor = Color.white;
        public Color interfaceColor = Color.blue;
    }
}