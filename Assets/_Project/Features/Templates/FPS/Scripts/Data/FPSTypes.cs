using UnityEngine;

namespace asterivo.Unity60.Features.Templates.FPS.Data
{
    /// <summary>
    /// FPSテンプレートで使用する基本データ型定義
    /// 詳細設計書準拠：データ駆動設計とTTK中心設計
    /// </summary>

    [System.Serializable]
    public enum FPSGameMode
    {
        TeamDeathmatch,     // チームデスマッチ（基本モード）
        FreeForAll,         // フリーフォーオール
        Elimination,        // エリミネーション
        Training           // トレーニングモード
    }

    [System.Serializable]
    public enum FireMode
    {
        SemiAuto,          // セミオート
        FullAuto,          // フルオート
        Burst              // バースト
    }

    [System.Serializable]
    public enum ShotType
    {
        HitScan,           // 即着弾（Raycastを使用）
        Projectile         // 物理弾（弾丸オブジェクトを生成）
    }

    [System.Serializable]
    public enum TTKProfile
    {
        Tactical,          // 低TTKのタクティカルシューター
        Balanced,          // バランス型
        Casual,            // 高TTKのカジュアルシューター
        Custom             // カスタム設定
    }

    [System.Serializable]
    public enum WeaponType
    {
        AssaultRifle,      // アサルトライフル
        SMG,               // サブマシンガン
        Sniper,            // スナイパーライフル
        Shotgun,           // ショットガン
        Pistol,            // ピストル
        LMG                // ライトマシンガン
    }

    [System.Serializable]
    public enum AmmoType
    {
        Rifle,             // ライフル弾
        Pistol,            // ピストル弾
        Sniper,            // スナイパー弾
        Shotgun,           // ショットガン弾
        LMG                // LMG弾
    }

    /// <summary>
    /// 武器の反動パターンデータ
    /// 垂直・水平方向の反動を定義
    /// </summary>
    [System.Serializable]
    public struct RecoilSettings
    {
        [Header("Vertical Recoil")]
        public float verticalRecoilMin;
        public float verticalRecoilMax;

        [Header("Horizontal Recoil")]
        public float horizontalRecoilMin;
        public float horizontalRecoilMax;

        [Header("Recoil Recovery")]
        public float recoilRecoverySpeed;
        public AnimationCurve recoilCurve;

        public static RecoilSettings Default => new RecoilSettings
        {
            verticalRecoilMin = 0.5f,
            verticalRecoilMax = 1.0f,
            horizontalRecoilMin = -0.3f,
            horizontalRecoilMax = 0.3f,
            recoilRecoverySpeed = 2.0f,
            recoilCurve = AnimationCurve.EaseInOut(0, 0, 1, 1)
        };
    }

    /// <summary>
    /// 弾薬設定データ
    /// マガジンサイズ、総弾薬数、弾薬タイプを管理
    /// </summary>
    [System.Serializable]
    public struct AmmoConfig
    {
        [Header("Magazine Settings")]
        public int magazineSize;
        public int totalAmmo;
        public AmmoType ammoType;
        
        [Header("Reload Settings")]
        public float reloadTime;
        public float tacticalReloadTime; // 弾薬が残っている状態でのリロード

        public static AmmoConfig DefaultRifle => new AmmoConfig
        {
            magazineSize = 30,
            totalAmmo = 210,
            ammoType = AmmoType.Rifle,
            reloadTime = 2.5f,
            tacticalReloadTime = 2.0f
        };
    }

    /// <summary>
    /// 武器の基本性能データ
    /// TTK計算の中心となるダメージと発射レート
    /// </summary>
    [System.Serializable]
    public struct WeaponStats
    {
        [Header("Damage & TTK")]
        public float damage;              // 1発あたりの基礎ダメージ
        public float fireRate;           // RPM (Rounds Per Minute)

        [Header("Accuracy")]
        public float baseAccuracy;       // 基本精度
        public float movingAccuracy;     // 移動時精度
        public float aimingAccuracy;     // エイム時精度

        [Header("Range")]
        public float effectiveRange;     // 有効射程
        public float maxRange;           // 最大射程
        public float damageDropoffStart; // ダメージ減衰開始距離

        [Header("Statistics")]
        public int shotsFired;           // 発射回数
        public int shotsHit;             // 命中回数
        public float damageDealt;        // 与えたダメージ総量
        public float accuracy;           // 精度（命中率）
        
        /// <summary>
        /// TTK (Time-to-Kill) の理論値計算
        /// (敵体力 / (ダメージ * (発射レート/60))) から算出
        /// </summary>
        public float CalculateTTK(float enemyHealth)
        {
            if (damage <= 0 || fireRate <= 0) return float.MaxValue;
            
            float shotsToKill = Mathf.Ceil(enemyHealth / damage);
            float timeBetweenShots = 60.0f / fireRate;
            return (shotsToKill - 1) * timeBetweenShots;
        }

        public static WeaponStats DefaultAssaultRifle => new WeaponStats
        {
            damage = 34.0f,
            fireRate = 600.0f,
            baseAccuracy = 0.8f,
            movingAccuracy = 0.6f,
            aimingAccuracy = 0.95f,
            effectiveRange = 50.0f,
            maxRange = 100.0f,
            damageDropoffStart = 30.0f
        };
    }

    /// <summary>
    /// プレイヤー状態データ
    /// FPS特有の状態を含む拡張
    /// </summary>
    [System.Serializable]
    public enum FPSPlayerState
    {
        Idle,              // 待機
        Walking,           // 歩行
        Running,           // 走行（スプリント）
        Crouching,         // しゃがみ
        Aiming,            // 照準状態
        Reloading,         // リロード状態
        WeaponSwitching,   // 武器切り替え状態
        Climbing,          // 登攀
        Sliding            // スライディング
    }

    /// <summary>
    /// FPSカメラモード
    /// 詳細設計書3.3準拠：FirstPerson、Weapon、Aimingの3モード
    /// </summary>
    [System.Serializable]
    public enum FPSCameraMode
    {
        FirstPerson,       // 一人称基本視点
        Weapon,            // 武器装備時視点
        Aiming             // 照準時視点
    }

    /// <summary>
    /// ヒットボックス部位定義
    /// 部位別ダメージ倍率システム用
    /// </summary>
    [System.Serializable]
    public enum HitboxType
    {
        Head,              // 頭部（ヘッドショット）
        Chest,             // 胸部
        Stomach,           // 腹部
        Arms,              // 腕部
        Legs               // 脚部
    }

    /// <summary>
    /// ヒットボックスダメージ倍率設定
    /// </summary>
    [System.Serializable]
    public struct HitboxMultiplier
    {
        public HitboxType hitboxType;
        public float damageMultiplier;

        public static HitboxMultiplier[] DefaultMultipliers => new[]
        {
            new HitboxMultiplier { hitboxType = HitboxType.Head, damageMultiplier = 2.0f },
            new HitboxMultiplier { hitboxType = HitboxType.Chest, damageMultiplier = 1.0f },
            new HitboxMultiplier { hitboxType = HitboxType.Stomach, damageMultiplier = 0.9f },
            new HitboxMultiplier { hitboxType = HitboxType.Arms, damageMultiplier = 0.8f },
            new HitboxMultiplier { hitboxType = HitboxType.Legs, damageMultiplier = 0.7f }
        };
    }
}
