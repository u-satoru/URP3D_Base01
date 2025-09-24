using UnityEngine;
using UnityEngine.Events;
using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Features.Combat.Interfaces;
using asterivo.Unity60.Features.Combat.Events;

namespace asterivo.Unity60.Features.Combat.Components
{
    /// <summary>
    /// 汎用ヘルスコンポーネント
    /// IHealthとIDamageableの両方を実装
    /// </summary>
    public class HealthComponent : MonoBehaviour, IHealth, IDamageable
    {
        [Header("Health Settings")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float currentHealth;
        [SerializeField] private bool startWithMaxHealth = true;

        [Header("Damage Settings")]
        [SerializeField] private bool isInvulnerable = false;
        [SerializeField] private float damageMultiplier = 1f;
        [SerializeField] private float healMultiplier = 1f;

        [Header("Events")]
        [SerializeField] private UnityEvent<float> onHealthChanged;
        [SerializeField] private UnityEvent<DamageInfo> onDamaged;
        [SerializeField] private UnityEvent<float> onHealed;
        [SerializeField] private UnityEvent onDeath;
        [SerializeField] private UnityEvent onRevive;

        private bool isDead = false;

        #region IHealth Implementation
        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public bool IsAlive => !isDead && currentHealth > 0;
        public bool IsInvulnerable
        {
            get => isInvulnerable;
            set => isInvulnerable = value;
        }

        public float TakeDamage(float damage)
        {
            var damageInfo = new DamageInfo(damage);
            return TakeDamage(damage, damageInfo);
        }

        public float TakeDamage(float damage, DamageInfo damageInfo)
        {
            if (isDead || isInvulnerable || damage <= 0)
                return 0f;

            float actualDamage = damage * damageMultiplier;
            float previousHealth = currentHealth;
            currentHealth = Mathf.Max(0, currentHealth - actualDamage);
            float damageTaken = previousHealth - currentHealth;

            // 健康値変更通知
            onHealthChanged?.Invoke(currentHealth);
            onDamaged?.Invoke(damageInfo);

            // GameEvent経由での通知
            RaiseDamageEvent(damageInfo, damageTaken);

            // 死亡処理
            if (currentHealth <= 0 && !isDead)
            {
                Die(damageInfo);
            }

            return damageTaken;
        }

        public float Heal(float amount)
        {
            if (isDead || amount <= 0)
                return 0f;

            float actualHeal = amount * healMultiplier;
            float previousHealth = currentHealth;
            currentHealth = Mathf.Min(maxHealth, currentHealth + actualHeal);
            float healAmount = currentHealth - previousHealth;

            // 健康値変更通知
            onHealthChanged?.Invoke(currentHealth);
            onHealed?.Invoke(healAmount);

            // GameEvent経由での通知
            RaiseHealEvent(healAmount);

            return healAmount;
        }

        public void ResetHealth()
        {
            currentHealth = maxHealth;
            isDead = false;
            onHealthChanged?.Invoke(currentHealth);
        }

        public void Kill()
        {
            if (!isDead)
            {
                var damageInfo = new DamageInfo(currentHealth);
                damageInfo.damageType = DamageType.Environmental;
                TakeDamage(currentHealth, damageInfo);
            }
        }
        #endregion

        #region IDamageable Implementation
        public bool CanTakeDamage => IsAlive && !IsInvulnerable;
        public Transform Transform => transform;
        public GameObject GameObject => gameObject;

        public void TakeDamage(DamageInfo damageInfo)
        {
            TakeDamage(damageInfo.damage, damageInfo);
        }
        #endregion

        #region Unity Lifecycle
        void Awake()
        {
            if (startWithMaxHealth)
            {
                currentHealth = maxHealth;
            }
        }

        void Start()
        {
            // CombatServiceに登録
            if (ServiceLocator.TryGet<ICombatService>(out var combatService))
            {
                combatService.RegisterHealth(this);
            }

            // 初期健康値を通知
            onHealthChanged?.Invoke(currentHealth);
        }

        void OnDestroy()
        {
            // CombatServiceから登録解除
            if (ServiceLocator.TryGet<ICombatService>(out var combatService))
            {
                combatService.UnregisterHealth(this);
            }
        }
        #endregion

        #region Private Methods
        private void Die(DamageInfo lastDamageInfo)
        {
            isDead = true;
            onDeath?.Invoke();

            // GameEvent経由での死亡通知
            RaiseDeathEvent(lastDamageInfo);
        }

        public void Revive(float healthAmount = -1)
        {
            if (!isDead)
                return;

            isDead = false;
            currentHealth = healthAmount > 0 ? Mathf.Min(healthAmount, maxHealth) : maxHealth;
            onRevive?.Invoke();
            onHealthChanged?.Invoke(currentHealth);
        }

        private void RaiseDamageEvent(DamageInfo damageInfo, float actualDamage)
        {
            if (ServiceLocator.TryGet<IEventManager>(out var eventManager))
            {
                var eventData = new DamageEventData(
                    damageInfo.attacker,
                    gameObject,
                    damageInfo,
                    actualDamage
                );
                eventManager.RaiseEvent(CombatEventNames.OnDamageReceived, eventData);
            }
        }

        private void RaiseHealEvent(float healAmount)
        {
            if (ServiceLocator.TryGet<IEventManager>(out var eventManager))
            {
                var eventData = new HealEventData(
                    gameObject,
                    gameObject,
                    healAmount,
                    HealType.Instant
                );
                eventManager.RaiseEvent(CombatEventNames.OnHeal, eventData);
            }
        }

        private void RaiseDeathEvent(DamageInfo lastDamageInfo)
        {
            if (ServiceLocator.TryGet<IEventManager>(out var eventManager))
            {
                var eventData = new DeathEventData(
                    gameObject,
                    lastDamageInfo.attacker,
                    lastDamageInfo
                );
                eventManager.RaiseEvent(CombatEventNames.OnDeath, eventData);
            }
        }
        #endregion

        #region Public Utility Methods
        public float GetHealthPercentage()
        {
            return maxHealth > 0 ? currentHealth / maxHealth : 0f;
        }

        public void SetMaxHealth(float newMaxHealth, bool adjustCurrentHealth = true)
        {
            float ratio = currentHealth / maxHealth;
            maxHealth = Mathf.Max(1f, newMaxHealth);

            if (adjustCurrentHealth)
            {
                currentHealth = maxHealth * ratio;
                onHealthChanged?.Invoke(currentHealth);
            }
        }

        public void SetDamageMultiplier(float multiplier)
        {
            damageMultiplier = Mathf.Max(0f, multiplier);
        }

        public void SetHealMultiplier(float multiplier)
        {
            healMultiplier = Mathf.Max(0f, multiplier);
        }
        #endregion
    }
}