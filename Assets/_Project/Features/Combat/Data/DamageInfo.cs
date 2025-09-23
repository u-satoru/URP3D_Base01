using UnityEngine;

namespace asterivo.Unity60.Features.Combat
{
    /// <summary>
    /// ダメージ情報を格納する構造体
    /// 攻撃者、ダメージタイプ、位置などの詳細情報を保持
    /// </summary>
    [System.Serializable]
    public struct DamageInfo
    {
        /// <summary>
        /// ダメージ量
        /// </summary>
        public float damage;

        /// <summary>
        /// ダメージタイプ
        /// </summary>
        public DamageType damageType;

        /// <summary>
        /// 攻撃者
        /// </summary>
        public GameObject attacker;

        /// <summary>
        /// ヒット位置
        /// </summary>
        public Vector3 hitPoint;

        /// <summary>
        /// ヒット法線
        /// </summary>
        public Vector3 hitNormal;

        /// <summary>
        /// 攻撃の発生源位置
        /// </summary>
        public Vector3 sourcePosition;

        /// <summary>
        /// 攻撃の方向
        /// </summary>
        public Vector3 direction;

        /// <summary>
        /// クリティカルヒットフラグ
        /// </summary>
        public bool isCritical;

        /// <summary>
        /// ヘッドショットフラグ
        /// </summary>
        public bool isHeadshot;

        /// <summary>
        /// ノックバック力
        /// </summary>
        public float knockbackForce;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public DamageInfo(float damage, GameObject attacker = null, DamageType damageType = DamageType.Normal)
        {
            this.damage = damage;
            this.damageType = damageType;
            this.attacker = attacker;
            this.hitPoint = Vector3.zero;
            this.hitNormal = Vector3.up;
            this.sourcePosition = attacker != null ? attacker.transform.position : Vector3.zero;
            this.direction = Vector3.forward;
            this.isCritical = false;
            this.isHeadshot = false;
            this.knockbackForce = 0f;
        }

        /// <summary>
        /// 詳細なダメージ情報を作成
        /// </summary>
        public static DamageInfo CreateDetailed(
            float damage,
            GameObject attacker,
            Vector3 hitPoint,
            Vector3 hitNormal,
            DamageType damageType = DamageType.Normal)
        {
            var info = new DamageInfo(damage, attacker, damageType);
            info.hitPoint = hitPoint;
            info.hitNormal = hitNormal;

            if (attacker != null)
            {
                info.sourcePosition = attacker.transform.position;
                info.direction = (hitPoint - info.sourcePosition).normalized;
            }

            return info;
        }
    }

    /// <summary>
    /// ダメージタイプ
    /// </summary>
    public enum DamageType
    {
        Normal,      // 通常ダメージ
        Fire,        // 火炎ダメージ
        Ice,         // 氷結ダメージ
        Electric,    // 電撃ダメージ
        Poison,      // 毒ダメージ
        Explosive,   // 爆発ダメージ
        Melee,       // 近接ダメージ
        Ranged,      // 遠距離ダメージ
        Fall,        // 落下ダメージ
        Environmental // 環境ダメージ
    }
}