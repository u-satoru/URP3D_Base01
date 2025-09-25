using UnityEngine;
using asterivo.Unity60.Features.Templates.FPS.Data;

namespace asterivo.Unity60.Features.Templates.FPS.Combat
{
    /// <summary>
    /// ダメージ判定を行うHitboxコンポーネント
    /// FPS Templateの戦闘システムで使用される基本的なヒットボックス
    /// </summary>
    public class HitboxComponent : MonoBehaviour
    {
        [Header("Hitbox Configuration")]
        [SerializeField] private float _damageMultiplier = 1.0f;
        [SerializeField] private bool _isHeadshot = false;
        [SerializeField] private HitboxType _hitboxType = HitboxType.Chest;

        public float DamageMultiplier => _damageMultiplier;
        public bool IsHeadshot => _isHeadshot;
        public HitboxType HitboxType => _hitboxType;

        /// <summary>
        /// ダメージ処理
        /// </summary>
        public void ProcessHit(float baseDamage, Vector3 hitPoint, Vector3 hitDirection)
        {
            float finalDamage = baseDamage * _damageMultiplier;

            // ヘッドショット判定
            if (_isHeadshot)
            {
                finalDamage *= 2.0f; // ヘッドショット倍率
            }

            // 親オブジェクトのHealthComponentにダメージを伝達
            var healthComponent = GetComponentInParent<HealthComponent>();
            if (healthComponent != null)
            {
                healthComponent.TakeDamage(finalDamage, hitPoint, hitDirection);
            }
        }

        /// <summary>
        /// HealthComponentを取得（CombatSystemとの互換性のため）
        /// </summary>
        public HealthComponent GetHealthComponent()
        {
            return GetComponentInParent<HealthComponent>();
        }
    }
}
