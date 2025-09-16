using UnityEngine;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Components;

namespace asterivo.Unity60.Features.Templates.FPS.Combat
{
    /// <summary>
    /// FPS Templateの基本的なヘルス管理コンポーネント
    /// ダメージ処理、死亡処理を担当
    /// IHealthTargetを実装してCommandパターンと統合
    /// </summary>
    public class HealthComponent : MonoBehaviour, IHealthTarget
    {
        [Header("Health Configuration")]
        [SerializeField] private int _maxHealth = 100;
        [SerializeField] private int _currentHealth;

        [Header("Events")]
        [SerializeField] private GameEvent _onHealthChanged;
        [SerializeField] private GameEvent _onDeath;

        // IHealthTarget implementation
        public int MaxHealth => _maxHealth;
        public int CurrentHealth => _currentHealth;

        // Compatibility properties for FPS template
        public bool IsAlive => _currentHealth > 0;
        public bool IsDead => _currentHealth <= 0;
        public float HealthPercentage => (float)_currentHealth / _maxHealth;

        private void Awake()
        {
            _currentHealth = _maxHealth;
        }

        // IHealthTarget implementation
        /// <summary>
        /// 指定された量だけターゲットを回復させます
        /// </summary>
        public void Heal(int amount)
        {
            if (!IsAlive) return;

            _currentHealth = Mathf.Min(_maxHealth, _currentHealth + amount);
            _onHealthChanged?.Raise();
        }

        /// <summary>
        /// 指定された量のダメージをターゲットに与えます
        /// </summary>
        public void TakeDamage(int amount)
        {
            if (!IsAlive) return;

            _currentHealth = Mathf.Max(0, _currentHealth - amount);

            // ヘルス変更イベント発行
            _onHealthChanged?.Raise();

            // 死亡判定
            if (_currentHealth <= 0)
            {
                Die();
            }
        }

        /// <summary>
        /// 属性タイプ付きのダメージをターゲットに与えます
        /// </summary>
        public void TakeDamage(int amount, string elementType)
        {
            // FPS Templateでは基本的に物理ダメージとして処理
            TakeDamage(amount);
        }

        /// <summary>
        /// FPS Template互換のダメージ処理（float版）
        /// </summary>
        public void TakeDamage(float damage, Vector3 hitPoint, Vector3 hitDirection)
        {
            TakeDamage(Mathf.RoundToInt(damage));
        }

        /// <summary>
        /// FPS Template互換の回復処理（float版）
        /// </summary>
        public void Heal(float amount)
        {
            Heal(Mathf.RoundToInt(amount));
        }

        /// <summary>
        /// 死亡処理
        /// </summary>
        private void Die()
        {
            _onDeath?.Raise();

            // 基本的な死亡処理
            if (CompareTag("Player"))
            {
                // プレイヤー死亡処理
                HandlePlayerDeath();
            }
            else
            {
                // NPCの死亡処理
                HandleNPCDeath();
            }
        }

        private void HandlePlayerDeath()
        {
            // プレイヤー死亡時の処理
            // 例：リスポーン処理、ゲームオーバー画面等
            Debug.Log("Player died!");
        }

        private void HandleNPCDeath()
        {
            // NPC死亡時の処理
            // 例：アイテムドロップ、経験値付与等
            Debug.Log($"{gameObject.name} died!");

            // 簡易的な消去処理（実際のプロジェクトでは死亡アニメーション等を実装）
            Destroy(gameObject, 2f);
        }

        /// <summary>
        /// ヘルスのリセット
        /// </summary>
        public void ResetHealth()
        {
            _currentHealth = _maxHealth;
            _onHealthChanged?.Raise();
        }
    }
}