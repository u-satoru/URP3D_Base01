using UnityEngine;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Commands;

namespace asterivo.Unity60.Features.Templates.FPS.Combat
{
    /// <summary>
    /// FPS専用ヘルス・アーマーシステム
    /// プレイヤーとエネミー両方に対応した統合ヘルスシステム
    /// </summary>
    public class FPSHealthSystem : MonoBehaviour
    {
        [Header("Health Settings")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float currentHealth;
        
        [Header("Armor Settings")]
        [SerializeField] private float maxArmor = 50f;
        [SerializeField] private float currentArmor;
        [SerializeField] private float armorDamageReduction = 0.5f;
        
        [Header("Regeneration")]
        [SerializeField] private float healthRegenRate = 5f;
        [SerializeField] private float healthRegenDelay = 3f;
        [SerializeField] private bool enableHealthRegen = true;
        
        [Header("Events")]
        [SerializeField] private GameEvent onHealthChanged;
        [SerializeField] private GameEvent onArmorChanged;
        [SerializeField] public GameEvent onPlayerDeath;
        
        private float lastDamageTime;
        private bool isDead;
        
        // Properties
        public float MaxHealth => maxHealth;
        public float CurrentHealth => currentHealth;
        public float MaxArmor => maxArmor;
        public float CurrentArmor => currentArmor;
        public bool IsDead => isDead;
        public float HealthPercentage => maxHealth > 0 ? currentHealth / maxHealth : 0f;
        public float ArmorPercentage => maxArmor > 0 ? currentArmor / maxArmor : 0f;
        
        private void Awake()
        {
            currentHealth = maxHealth;
            currentArmor = maxArmor;
        }
        
        private void Update()
        {
            if (enableHealthRegen && !isDead && currentHealth < maxHealth)
            {
                if (Time.time - lastDamageTime >= healthRegenDelay)
                {
                    RegenerateHealth();
                }
            }
        }
        
        /// <summary>
        /// 初期化メソッド
        /// </summary>
        public void Initialize(float health, float armor, float regenRate, float regenDelay)
        {
            maxHealth = health;
            maxArmor = armor;
            healthRegenRate = regenRate;
            healthRegenDelay = regenDelay;
            
            currentHealth = maxHealth;
            currentArmor = maxArmor;
            isDead = false;
        }
        
        /// <summary>
        /// ダメージを受ける
        /// </summary>
        public void TakeDamage(float damage)
        {
            if (isDead) return;
            
            float actualDamage = damage;
            
            // アーマーがある場合の軽減計算
            if (currentArmor > 0)
            {
                float armorDamage = damage * armorDamageReduction;
                float healthDamage = damage * (1 - armorDamageReduction);
                
                currentArmor -= armorDamage;
                if (currentArmor < 0)
                {
                    // アーマーがマイナスになった分をヘルスから引く
                    healthDamage += -currentArmor;
                    currentArmor = 0;
                }
                
                actualDamage = healthDamage;
                onArmorChanged?.Raise();
            }
            
            currentHealth -= actualDamage;
            lastDamageTime = Time.time;
            
            if (currentHealth <= 0)
            {
                currentHealth = 0;
                Die();
            }
            
            onHealthChanged?.Raise();
        }
        
        /// <summary>
        /// 回復
        /// </summary>
        public void Heal(float healAmount)
        {
            if (isDead) return;
            
            currentHealth = Mathf.Min(currentHealth + healAmount, maxHealth);
            onHealthChanged?.Raise();
        }
        
        /// <summary>
        /// アーマー回復
        /// </summary>
        public void RestoreArmor(float armorAmount)
        {
            if (isDead) return;
            
            currentArmor = Mathf.Min(currentArmor + armorAmount, maxArmor);
            onArmorChanged?.Raise();
        }
        
        /// <summary>
        /// ヘルス再生
        /// </summary>
        private void RegenerateHealth()
        {
            float regenAmount = healthRegenRate * Time.deltaTime;
            currentHealth = Mathf.Min(currentHealth + regenAmount, maxHealth);
            onHealthChanged?.Raise();
        }
        
        /// <summary>
        /// 死亡処理
        /// </summary>
        private void Die()
        {
            isDead = true;
            onPlayerDeath?.Raise();
            
            Debug.Log($"[FPSHealthSystem] {gameObject.name} has died");
        }
        
        /// <summary>
        /// 復活
        /// </summary>
        public void Respawn()
        {
            currentHealth = maxHealth;
            currentArmor = maxArmor;
            isDead = false;
            
            onHealthChanged?.Raise();
            onArmorChanged?.Raise();
        }
        
        /// <summary>
        /// ヘルス・アーマーをフルに設定
        /// </summary>
        public void SetFullHealthAndArmor()
        {
            currentHealth = maxHealth;
            currentArmor = maxArmor;
            
            onHealthChanged?.Raise();
            onArmorChanged?.Raise();
        }
    }
}