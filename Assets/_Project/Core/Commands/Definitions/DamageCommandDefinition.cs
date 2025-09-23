using UnityEngine;
// using asterivo.Unity60.Core.Commands;
// using asterivo.Unity60.Core.Components;

namespace asterivo.Unity60.Core.Commands.Definitions
{
    /// <summary>
    /// ダメージコマンド�E定義、E    /// プレイヤーまた�EAIにダメージを与えるアクションをカプセル化します、E    /// 
    /// 主な機�E�E�E    /// - ダメージ量と種類�E持E��E    /// - ダメージソースの管琁E    /// - 状態異常の付丁E    /// - ダメージ軽減効果への対忁E    /// </summary>
    [System.Serializable]
    public class DamageCommandDefinition : ICommandDefinition
    {
        /// <summary>
        /// ダメージの種類を定義する列挙垁E        /// </summary>
        public enum DamageType
        {
            Physical,   // 物琁E��メージ
            Fire,       // 火炎ダメージ
            Ice,        // 氷結ダメージ
            Lightning,  // 電撁E��メージ
            Poison,     // 毒ダメージ
            Pure        // 純粋ダメージ�E�軽減不可�E�E        }

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
        /// チE��ォルトコンストラクタ
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
        /// ダメージコマンドが実行可能かどぁE��を判定しまぁE        /// </summary>
        public bool CanExecute(object context = null)
        {
            // 基本皁E��実行可能性チェチE��
            if (damageAmount <= 0f) return false;

            // コンチE��ストがある場合�E追加チェチE��
            if (context != null)
            {
                // ターゲチE��の生存状態チェチE��筁E            }

            return true;
        }

        /// <summary>
        /// ダメージコマンド�Eインスタンスを作�EしまぁE        /// </summary>
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
        /// 実際のダメージ量を計算します（防御力等を老E�E�E�E        /// </summary>
        public float CalculateActualDamage(float targetDefense = 0f)
        {
            float actualDamage = damageAmount;
            
            // 防御力による軽渁E            if (armorPenetration < 1f)
            {
                float effectiveDefense = targetDefense * (1f - armorPenetration);
                actualDamage = Mathf.Max(1f, actualDamage - effectiveDefense);
            }
            
            return actualDamage;
        }
    }
}