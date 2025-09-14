using System;
using UnityEngine;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Services;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Features.Templates.ActionRPG.Combat
{
    /// <summary>
    /// Action RPGテンプレート用ヘルス管理システム
    /// HP・MP管理、死亡処理、回復処理を行う
    /// </summary>
    public class Health : MonoBehaviour
    {
        [BoxGroup("Configuration")]
        [SerializeField]
        [Min(1)]
        private int maxHealth = 100;

        [BoxGroup("Configuration")]
        [SerializeField]
        [Min(0)]
        private int maxMana = 50;

        [BoxGroup("Configuration")]
        [SerializeField]
        private bool debugMode = true;

        [BoxGroup("Configuration")]
        [SerializeField]
        private bool invulnerable = false;

        [BoxGroup("Regeneration")]
        [SerializeField]
        private bool enableHealthRegen = true;

        [BoxGroup("Regeneration")]
        [SerializeField]
        private float healthRegenRate = 1f;

        [BoxGroup("Regeneration")]
        [SerializeField]
        private float healthRegenDelay = 5f;

        [BoxGroup("Regeneration")]
        [SerializeField]
        private bool enableManaRegen = true;

        [BoxGroup("Regeneration")]
        [SerializeField]
        private float manaRegenRate = 2f;

        [BoxGroup("Regeneration")]
        [SerializeField]
        private float manaRegenDelay = 3f;

        [BoxGroup("Events")]
        [SerializeField]
        private IntGameEvent onHealthChanged;

        [BoxGroup("Events")]
        [SerializeField]
        private IntGameEvent onManaChanged;

        [BoxGroup("Events")]
        [SerializeField]
        private GameEvent onDeath;

        [BoxGroup("Events")]
        [SerializeField]
        private GameEvent onRevive;

        [BoxGroup("Current State")]
        [ShowInInspector, ReadOnly]
        [ProgressBar(0, "maxHealth", ColorGetter = "GetHealthBarColor")]
        private int currentHealth;

        [BoxGroup("Current State")]
        [ShowInInspector, ReadOnly]
        [ProgressBar(0, "maxMana", r: 0, g: 0, b: 1)]
        private int currentMana;

        [BoxGroup("Current State")]
        [ShowInInspector, ReadOnly]
        private bool isDead = false;

        [BoxGroup("Statistics")]
        [ShowInInspector, ReadOnly]
        public int TotalDamageTaken { get; private set; }

        [BoxGroup("Statistics")]
        [ShowInInspector, ReadOnly]
        public int TotalHealingReceived { get; private set; }

        [BoxGroup("Statistics")]
        [ShowInInspector, ReadOnly]
        public int DeathCount { get; private set; }

        // 再生タイマー
        private float healthRegenTimer = 0f;
        private float manaRegenTimer = 0f;
        private float lastDamageTime = 0f;
        private float lastManaUseTime = 0f;

        // プロパティ
        public int CurrentHealth => currentHealth;
        public int MaxHealth => maxHealth;
        public int CurrentMana => currentMana;
        public int MaxMana => maxMana;
        public bool IsDead => isDead;
        public bool IsAlive => !isDead;
        public float HealthPercentage => maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;
        public float ManaPercentage => maxMana > 0 ? (float)currentMana / maxMana : 0f;

        // イベント
        public event Action<int> OnHealthChanged;
        public event Action<int> OnManaChanged;
        public event Action OnDeath;
        public event Action OnRevive;

        private void Awake()
        {
            InitializeHealth();
        }

        private void Start()
        {
            RegisterWithServices();
        }

        private void Update()
        {
            if (!isDead)
            {
                UpdateRegeneration();
            }
        }

        private void OnDestroy()
        {
            UnregisterFromServices();
        }

        /// <summary>
        /// ヘルスシステムの初期化
        /// </summary>
        private void InitializeHealth()
        {
            currentHealth = maxHealth;
            currentMana = maxMana;
            isDead = false;

            healthRegenTimer = 0f;
            manaRegenTimer = 0f;
            lastDamageTime = 0f;
            lastManaUseTime = 0f;

            TotalDamageTaken = 0;
            TotalHealingReceived = 0;
            DeathCount = 0;

            LogDebug($"[Health] Initialized with {maxHealth} HP and {maxMana} MP");
        }

        /// <summary>
        /// サービスへの登録
        /// </summary>
        private void RegisterWithServices()
        {
            // プレイヤーの場合のみサービスに登録
            if (gameObject.CompareTag("Player"))
            {
                if (ServiceLocator.Instance != null)
                {
                    ServiceLocator.Instance.RegisterService<Health>(this);
                    LogDebug("[Health] Player health registered with ServiceLocator");
                }
            }
        }

        /// <summary>
        /// サービスの登録解除
        /// </summary>
        private void UnregisterFromServices()
        {
            if (gameObject.CompareTag("Player"))
            {
                if (ServiceLocator.Instance != null)
                {
                    ServiceLocator.Instance.UnregisterService<Health>();
                    LogDebug("[Health] Player health unregistered from ServiceLocator");
                }
            }
        }

        /// <summary>
        /// 再生処理の更新
        /// </summary>
        private void UpdateRegeneration()
        {
            // ヘルス再生
            if (enableHealthRegen && currentHealth < maxHealth)
            {
                float timeSinceLastDamage = Time.time - lastDamageTime;
                if (timeSinceLastDamage >= healthRegenDelay)
                {
                    healthRegenTimer += Time.deltaTime;
                    if (healthRegenTimer >= 1f / healthRegenRate)
                    {
                        Heal(1, false);
                        healthRegenTimer = 0f;
                    }
                }
            }

            // マナ再生
            if (enableManaRegen && currentMana < maxMana)
            {
                float timeSinceLastManaUse = Time.time - lastManaUseTime;
                if (timeSinceLastManaUse >= manaRegenDelay)
                {
                    manaRegenTimer += Time.deltaTime;
                    if (manaRegenTimer >= 1f / manaRegenRate)
                    {
                        RestoreMana(1, false);
                        manaRegenTimer = 0f;
                    }
                }
            }
        }

        #region Health Management

        /// <summary>
        /// ダメージを受ける
        /// </summary>
        /// <param name="amount">ダメージ量</param>
        /// <param name="ignoreInvulnerability">無敵状態を無視するか</param>
        public void TakeDamage(int amount, bool ignoreInvulnerability = false)
        {
            if (isDead) return;
            if (invulnerable && !ignoreInvulnerability) return;
            if (amount <= 0) return;

            int actualDamage = Mathf.Min(amount, currentHealth);
            currentHealth -= actualDamage;
            lastDamageTime = Time.time;

            TotalDamageTaken += actualDamage;

            // イベント発行
            OnHealthChanged?.Invoke(currentHealth);
            onHealthChanged?.Raise(currentHealth);

            LogDebug($"[Health] Took {actualDamage} damage. Health: {currentHealth}/{maxHealth}");

            // 死亡チェック
            if (currentHealth <= 0)
            {
                Die();
            }
        }

        /// <summary>
        /// 回復
        /// </summary>
        /// <param name="amount">回復量</param>
        /// <param name="showMessage">ログメッセージを表示するか</param>
        public void Heal(int amount, bool showMessage = true)
        {
            if (isDead) return;
            if (amount <= 0) return;

            int actualHeal = Mathf.Min(amount, maxHealth - currentHealth);
            if (actualHeal <= 0) return;

            currentHealth += actualHeal;
            TotalHealingReceived += actualHeal;

            // イベント発行
            OnHealthChanged?.Invoke(currentHealth);
            onHealthChanged?.Raise(currentHealth);

            if (showMessage)
            {
                LogDebug($"[Health] Healed {actualHeal} HP. Health: {currentHealth}/{maxHealth}");
            }
        }

        /// <summary>
        /// 最大ヘルスを設定
        /// </summary>
        /// <param name="newMaxHealth">新しい最大ヘルス</param>
        /// <param name="healToFull">満タンまで回復するか</param>
        public void SetMaxHealth(int newMaxHealth, bool healToFull = false)
        {
            int oldMaxHealth = maxHealth;
            maxHealth = Mathf.Max(1, newMaxHealth);

            if (healToFull)
            {
                currentHealth = maxHealth;
            }
            else
            {
                // 現在のヘルス比率を維持
                float healthRatio = oldMaxHealth > 0 ? (float)currentHealth / oldMaxHealth : 1f;
                currentHealth = Mathf.RoundToInt(maxHealth * healthRatio);
            }

            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

            // イベント発行
            OnHealthChanged?.Invoke(currentHealth);
            onHealthChanged?.Raise(currentHealth);

            LogDebug($"[Health] Max health changed from {oldMaxHealth} to {maxHealth}. Current: {currentHealth}");
        }

        #endregion

        #region Mana Management

        /// <summary>
        /// マナを消費
        /// </summary>
        /// <param name="amount">消費量</param>
        /// <returns>実際に消費できた量</returns>
        public int ConsumeMana(int amount)
        {
            if (isDead) return 0;
            if (amount <= 0) return 0;

            int actualConsume = Mathf.Min(amount, currentMana);
            currentMana -= actualConsume;
            lastManaUseTime = Time.time;

            // イベント発行
            OnManaChanged?.Invoke(currentMana);
            onManaChanged?.Raise(currentMana);

            LogDebug($"[Health] Consumed {actualConsume} mana. Mana: {currentMana}/{maxMana}");

            return actualConsume;
        }

        /// <summary>
        /// マナを回復
        /// </summary>
        /// <param name="amount">回復量</param>
        /// <param name="showMessage">ログメッセージを表示するか</param>
        public void RestoreMana(int amount, bool showMessage = true)
        {
            if (isDead) return;
            if (amount <= 0) return;

            int actualRestore = Mathf.Min(amount, maxMana - currentMana);
            if (actualRestore <= 0) return;

            currentMana += actualRestore;

            // イベント発行
            OnManaChanged?.Invoke(currentMana);
            onManaChanged?.Raise(currentMana);

            if (showMessage)
            {
                LogDebug($"[Health] Restored {actualRestore} mana. Mana: {currentMana}/{maxMana}");
            }
        }

        /// <summary>
        /// マナが足りるかチェック
        /// </summary>
        /// <param name="amount">必要量</param>
        /// <returns>足りるかどうか</returns>
        public bool HasEnoughMana(int amount)
        {
            return currentMana >= amount;
        }

        /// <summary>
        /// 最大マナを設定
        /// </summary>
        /// <param name="newMaxMana">新しい最大マナ</param>
        /// <param name="restoreToFull">満タンまで回復するか</param>
        public void SetMaxMana(int newMaxMana, bool restoreToFull = false)
        {
            int oldMaxMana = maxMana;
            maxMana = Mathf.Max(0, newMaxMana);

            if (restoreToFull)
            {
                currentMana = maxMana;
            }
            else
            {
                // 現在のマナ比率を維持
                float manaRatio = oldMaxMana > 0 ? (float)currentMana / oldMaxMana : 1f;
                currentMana = Mathf.RoundToInt(maxMana * manaRatio);
            }

            currentMana = Mathf.Clamp(currentMana, 0, maxMana);

            // イベント発行
            OnManaChanged?.Invoke(currentMana);
            onManaChanged?.Raise(currentMana);

            LogDebug($"[Health] Max mana changed from {oldMaxMana} to {maxMana}. Current: {currentMana}");
        }

        #endregion

        #region Death and Revival

        /// <summary>
        /// 死亡処理
        /// </summary>
        private void Die()
        {
            if (isDead) return;

            isDead = true;
            currentHealth = 0;
            DeathCount++;

            // イベント発行
            OnDeath?.Invoke();
            onDeath?.Raise();

            LogDebug($"[Health] {gameObject.name} has died (Death #{DeathCount})");

            // プレイヤーでない場合は無効化
            if (!gameObject.CompareTag("Player"))
            {
                gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 復活
        /// </summary>
        /// <param name="healthAmount">復活時のヘルス</param>
        /// <param name="manaAmount">復活時のマナ</param>
        public void Revive(int healthAmount = -1, int manaAmount = -1)
        {
            if (!isDead) return;

            isDead = false;
            gameObject.SetActive(true);

            // ヘルス設定（-1の場合は最大値の50%）
            if (healthAmount < 0)
            {
                currentHealth = maxHealth / 2;
            }
            else
            {
                currentHealth = Mathf.Clamp(healthAmount, 1, maxHealth);
            }

            // マナ設定（-1の場合は最大値の50%）
            if (manaAmount < 0)
            {
                currentMana = maxMana / 2;
            }
            else
            {
                currentMana = Mathf.Clamp(manaAmount, 0, maxMana);
            }

            // イベント発行
            OnRevive?.Invoke();
            onRevive?.Raise();
            OnHealthChanged?.Invoke(currentHealth);
            onHealthChanged?.Raise(currentHealth);
            OnManaChanged?.Invoke(currentMana);
            onManaChanged?.Raise(currentMana);

            LogDebug($"[Health] {gameObject.name} has been revived with {currentHealth} HP and {currentMana} MP");
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// 完全回復
        /// </summary>
        public void FullRestore()
        {
            if (isDead) return;

            currentHealth = maxHealth;
            currentMana = maxMana;

            // イベント発行
            OnHealthChanged?.Invoke(currentHealth);
            onHealthChanged?.Raise(currentHealth);
            OnManaChanged?.Invoke(currentMana);
            onManaChanged?.Raise(currentMana);

            LogDebug("[Health] Fully restored HP and MP");
        }

        /// <summary>
        /// 統計をリセット
        /// </summary>
        public void ResetStatistics()
        {
            TotalDamageTaken = 0;
            TotalHealingReceived = 0;
            DeathCount = 0;

            LogDebug("[Health] Statistics reset");
        }

        /// <summary>
        /// 無敵状態を設定
        /// </summary>
        /// <param name="isInvulnerable">無敵状態にするか</param>
        public void SetInvulnerable(bool isInvulnerable)
        {
            invulnerable = isInvulnerable;
            LogDebug($"[Health] Invulnerability set to {invulnerable}");
        }

        #endregion

        #region Debug Support

        [Button("Take Test Damage")]
        [ShowIf("debugMode")]
        private void TakeTestDamage()
        {
            TakeDamage(20);
        }

        [Button("Heal to Full")]
        [ShowIf("debugMode")]
        private void HealToFull()
        {
            FullRestore();
        }

        [Button("Consume Test Mana")]
        [ShowIf("debugMode")]
        private void ConsumeTestMana()
        {
            ConsumeMana(10);
        }

        [Button("Toggle Invulnerability")]
        [ShowIf("debugMode")]
        private void ToggleInvulnerability()
        {
            SetInvulnerable(!invulnerable);
        }

        [Button("Show Health Statistics")]
        [ShowIf("debugMode")]
        private void ShowHealthStatistics()
        {
            LogDebug("=== Health Statistics ===");
            LogDebug($"Current Health: {currentHealth}/{maxHealth} ({HealthPercentage:P})");
            LogDebug($"Current Mana: {currentMana}/{maxMana} ({ManaPercentage:P})");
            LogDebug($"Is Dead: {isDead}");
            LogDebug($"Is Invulnerable: {invulnerable}");
            LogDebug($"Total Damage Taken: {TotalDamageTaken}");
            LogDebug($"Total Healing Received: {TotalHealingReceived}");
            LogDebug($"Death Count: {DeathCount}");
            LogDebug("========================");
        }

        private Color GetHealthBarColor()
        {
            float healthRatio = HealthPercentage;
            if (healthRatio > 0.6f)
                return Color.green;
            else if (healthRatio > 0.3f)
                return Color.yellow;
            else
                return Color.red;
        }

        private void LogDebug(string message)
        {
            if (debugMode)
            {
                Debug.Log(message);
            }
        }

        #endregion
    }
}