using UnityEngine;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Components;
using asterivo.Unity60.Features.Combat.Interfaces;
using asterivo.Unity60.Features.Combat;

namespace asterivo.Unity60.Features.Templates.FPS.Combat
{
    /// <summary>
    /// FPS Templateの基本的なヘルス管理コンポーネント
    /// Combat Feature層のHealthComponentをラップし、FPS固有の機能を追加
    /// IHealthTargetを実装してCommandパターンと統合
    /// </summary>
    [RequireComponent(typeof(asterivo.Unity60.Features.Combat.Components.HealthComponent))]
    public class HealthComponent : MonoBehaviour, IHealthTarget
    {
        [Header("FPS Events")]
        [SerializeField] private GameEvent _onHealthChanged;
        [SerializeField] private GameEvent _onDeath;

        // Reference to Combat Feature layer component
        private asterivo.Unity60.Features.Combat.Components.HealthComponent _combatHealth;

        // IHealthTarget implementation (wrapper properties)
        public int MaxHealth => Mathf.RoundToInt(_combatHealth?.MaxHealth ?? 100);
        public int CurrentHealth => Mathf.RoundToInt(_combatHealth?.CurrentHealth ?? 100);

        // Compatibility properties for FPS template
        public bool IsAlive => _combatHealth?.IsAlive ?? true;
        public bool IsDead => !IsAlive;
        public float HealthPercentage => _combatHealth?.GetHealthPercentage() ?? 1f;

        private void Awake()
        {
            // Get reference to Combat Feature layer component
            _combatHealth = GetComponent<asterivo.Unity60.Features.Combat.Components.HealthComponent>();
            if (_combatHealth == null)
            {
                _combatHealth = gameObject.AddComponent<asterivo.Unity60.Features.Combat.Components.HealthComponent>();
            }

            // Configure Combat layer component
            ConfigureCombatHealth();
        }

        private void ConfigureCombatHealth()
        {
            // Subscribe to Combat layer events
            var combatHealthComponent = GetComponent<asterivo.Unity60.Features.Combat.Components.HealthComponent>();
            if (combatHealthComponent != null)
            {
                // Use Unity Events from Combat layer to trigger FPS-specific events
                var healthChanged = combatHealthComponent.GetType().GetField("onHealthChanged",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var death = combatHealthComponent.GetType().GetField("onDeath",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (healthChanged != null && healthChanged.GetValue(combatHealthComponent) is UnityEngine.Events.UnityEvent<float> healthEvent)
                {
                    healthEvent.AddListener((health) => _onHealthChanged?.Raise());
                }

                if (death != null && death.GetValue(combatHealthComponent) is UnityEngine.Events.UnityEvent deathEvent)
                {
                    deathEvent.AddListener(() => OnDeath());
                }
            }
        }

        // IHealthTarget implementation
        /// <summary>
        /// 指定された量だけターゲットを回復させます
        /// </summary>
        public void Heal(int amount)
        {
            _combatHealth?.Heal(amount);
        }

        /// <summary>
        /// 指定された量のダメージをターゲットに与えます
        /// </summary>
        public void TakeDamage(int amount)
        {
            var damageInfo = new DamageInfo(amount);
            damageInfo.damageType = DamageType.Ranged; // FPS default damage type
            _combatHealth?.TakeDamage(damageInfo);
        }

        /// <summary>
        /// 属性タイプ付きのダメージをターゲットに与えます
        /// </summary>
        public void TakeDamage(int amount, string elementType)
        {
            var damageInfo = new DamageInfo(amount);

            // Map element type to damage type
            damageInfo.damageType = elementType?.ToLower() switch
            {
                "fire" => DamageType.Fire,
                "ice" => DamageType.Ice,
                "electric" => DamageType.Electric,
                "poison" => DamageType.Poison,
                "explosive" => DamageType.Explosive,
                _ => DamageType.Ranged
            };

            _combatHealth?.TakeDamage(damageInfo);
        }

        /// <summary>
        /// FPS Template互換のダメージ処理（float版）
        /// </summary>
        public void TakeDamage(float damage, Vector3 hitPoint, Vector3 hitDirection)
        {
            var damageInfo = DamageInfo.CreateDetailed(
                damage,
                null, // attacker will be determined from context
                hitPoint,
                hitDirection.normalized,
                DamageType.Ranged
            );

            _combatHealth?.TakeDamage(damageInfo);
        }

        /// <summary>
        /// FPS Template互換の回復処理（float版）
        /// </summary>
        public void Heal(float amount)
        {
            _combatHealth?.Heal(amount);
        }

        /// <summary>
        /// 死亡処理
        /// </summary>
        private void OnDeath()
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
            _combatHealth?.ResetHealth();
            _onHealthChanged?.Raise();
        }
    }
}