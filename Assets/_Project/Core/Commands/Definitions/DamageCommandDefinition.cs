using UnityEngine;
using asterivo.Unity60.Core.Commands;
using asterivo.Unity60.Core.Components;

namespace asterivo.Unity60.Core.Commands.Definitions
{
    /// <summary>
    /// ダメージコマンドの定義。
    /// プレイヤーまたはAIにダメージを与えるアクションをカプセル化します。
    /// 
    /// 主な機能：
    /// - ダメージ量と種類の指定
    /// - ダメージソースの管理
    /// - 状態異常の付与
    /// - ダメージ軽減効果への対応
    /// </summary>
    [System.Serializable]
    public class DamageCommandDefinition : ICommandDefinition
    {
        /// <summary>
        /// ダメージの種類を定義する列挙型
        /// </summary>
        public enum DamageType
        {
            Physical,   // 物理ダメージ
            Fire,       // 火炎ダメージ
            Ice,        // 氷結ダメージ
            Lightning,  // 電撃ダメージ
            Poison,     // 毒ダメージ
            Pure        // 純粋ダメージ（軽減不可）
        }

        [Header("Damage Parameters")]
        public DamageType damageType = DamageType.Physical;
        public float damageAmount = 10f;
        public bool canKill = true;
        public float armorPenetration = 0f;

        [Header("Effects")]
        public bool applyKnockback = false;
        public Vector3 knockbackDirection = Vector3.zero;
        public float knockbackForce = 5f;

        [Header("Status Effects")]
        public bool appliesStatusEffect = false;
        public float statusDuration = 0f;

        [Header("Visual/Audio")]
        public bool showDamageNumbers = true;
        public Color damageColor = Color.red;

        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>
        public DamageCommandDefinition()
        {
        }

        /// <summary>
        /// パラメータ付きコンストラクタ
        /// </summary>
        public DamageCommandDefinition(float damage, DamageType type = DamageType.Physical)
        {
            damageAmount = damage;
            damageType = type;
        }

        /// <summary>
        /// ダメージコマンドが実行可能かどうかを判定します
        /// </summary>
        public bool CanExecute(object context = null)
        {
            // 基本的な実行可能性チェック
            if (damageAmount <= 0f) return false;

            // コンテキストがある場合の追加チェック
            if (context != null)
            {
                // ターゲットの生存状態チェック等
            }

            return true;
        }

        /// <summary>
        /// ダメージコマンドのインスタンスを作成します
        /// </summary>
        public ICommand CreateCommand(object context = null)
        {
            if (!CanExecute(context))
                return null;
                
            if (context is IHealthTarget target)
            {
                return new DamageCommand(target, Mathf.RoundToInt(damageAmount), damageType.ToString().ToLower());
            }
            
            return null;
        }

        /// <summary>
        /// 実際のダメージ量を計算します（防御力等を考慮）
        /// </summary>
        public float CalculateActualDamage(float targetDefense = 0f)
        {
            float actualDamage = damageAmount;
            
            // 防御力による軽減
            if (armorPenetration < 1f)
            {
                float effectiveDefense = targetDefense * (1f - armorPenetration);
                actualDamage = Mathf.Max(1f, actualDamage - effectiveDefense);
            }
            
            return actualDamage;
        }
    }
}