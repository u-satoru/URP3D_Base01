using UnityEngine;
using asterivo.Unity60.Features.Templates.FPS.Data;

namespace asterivo.Unity60.Features.Templates.FPS.WeaponSystem
{
    /// <summary>
    /// 射撃戦略のインターフェース
    /// Strategy パターンによる HitScan と Projectile の切り替え実装
    /// WeaponData の ShotType に応じて動的に戦略を変更
    /// </summary>
    public interface IShootingStrategy
    {
        /// <summary>
        /// 射撃実行
        /// </summary>
        /// <param name="origin">射撃開始位置</param>
        /// <param name="direction">射撃方向</param>
        /// <param name="weaponData">武器データ</param>
        /// <returns>射撃結果</returns>
        ShotResult ExecuteShot(Vector3 origin, Vector3 direction, WeaponData weaponData);
    }

    /// <summary>
    /// 射撃結果データ
    /// </summary>
    [System.Serializable]
    public struct ShotResult
    {
        public bool Hit;                    // ヒットしたかどうか
        public Vector3 HitPoint;           // ヒット位置
        public Vector3 HitNormal;          // ヒット面の法線
        public Collider HitCollider;      // ヒットしたコライダー
        public float Distance;             // 射撃距離
        public float ActualDamage;         // 実際に与えたダメージ
        public HitboxType HitboxType;      // ヒットしたボックスタイプ
        public GameObject HitGameObject;   // ヒットしたゲームオブジェクト

        public static ShotResult Miss => new ShotResult
        {
            Hit = false,
            HitPoint = Vector3.zero,
            HitNormal = Vector3.up,
            HitCollider = null,
            Distance = 0f,
            ActualDamage = 0f,
            HitboxType = HitboxType.Chest,
            HitGameObject = null
        };

        public static ShotResult CreateHit(RaycastHit hitInfo, float distance, float damage, HitboxType hitboxType)
        {
            return new ShotResult
            {
                Hit = true,
                HitPoint = hitInfo.point,
                HitNormal = hitInfo.normal,
                HitCollider = hitInfo.collider,
                Distance = distance,
                ActualDamage = damage,
                HitboxType = hitboxType,
                HitGameObject = hitInfo.collider.gameObject
            };
        }
    }
}