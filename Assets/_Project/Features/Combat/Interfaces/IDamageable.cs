using UnityEngine;

namespace asterivo.Unity60.Features.Combat.Interfaces
{
    /// <summary>
    /// ダメージを受けることができるエンティティのインターフェース
    /// </summary>
    public interface IDamageable
    {
        /// <summary>
        /// ダメージを受ける
        /// </summary>
        /// <param name="damageInfo">ダメージ情報</param>
        void TakeDamage(DamageInfo damageInfo);

        /// <summary>
        /// このエンティティがダメージを受けることができるかどうか
        /// </summary>
        bool CanTakeDamage { get; }

        /// <summary>
        /// エンティティのTransform
        /// </summary>
        Transform Transform { get; }

        /// <summary>
        /// エンティティのGameObject
        /// </summary>
        GameObject GameObject { get; }
    }
}
