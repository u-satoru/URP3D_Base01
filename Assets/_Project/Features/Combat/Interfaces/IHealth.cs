using UnityEngine;

namespace asterivo.Unity60.Features.Combat.Interfaces
{
    /// <summary>
    /// ヘルスシステムの基本インターフェース
    /// すべてのダメージを受けるエンティティが実装すべき契約
    /// </summary>
    public interface IHealth
    {
        /// <summary>
        /// 現在のヘルス値
        /// </summary>
        float CurrentHealth { get; }

        /// <summary>
        /// 最大ヘルス値
        /// </summary>
        float MaxHealth { get; }

        /// <summary>
        /// 生存状態
        /// </summary>
        bool IsAlive { get; }

        /// <summary>
        /// 無敵状態
        /// </summary>
        bool IsInvulnerable { get; set; }

        /// <summary>
        /// ダメージを受ける
        /// </summary>
        /// <param name="damage">ダメージ量</param>
        /// <returns>実際に受けたダメージ量</returns>
        float TakeDamage(float damage);

        /// <summary>
        /// ダメージを受ける（詳細情報付き）
        /// </summary>
        /// <param name="damage">ダメージ量</param>
        /// <param name="damageInfo">ダメージ詳細情報</param>
        /// <returns>実際に受けたダメージ量</returns>
        float TakeDamage(float damage, DamageInfo damageInfo);

        /// <summary>
        /// 回復する
        /// </summary>
        /// <param name="amount">回復量</param>
        /// <returns>実際に回復した量</returns>
        float Heal(float amount);

        /// <summary>
        /// ヘルスをリセット
        /// </summary>
        void ResetHealth();

        /// <summary>
        /// 即死させる
        /// </summary>
        void Kill();
    }
}
