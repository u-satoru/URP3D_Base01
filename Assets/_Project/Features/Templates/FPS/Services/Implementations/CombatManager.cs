using System;
using UnityEngine;
using asterivo.Unity60.Features.Templates.FPS.Data;

namespace asterivo.Unity60.Features.Templates.FPS.Services
{
    /// <summary>
    /// 戦闘管理サービス実装
    /// ServiceLocator + Event駆動のハイブリッドアーキテクチャ準拠
    /// </summary>
    public class CombatManager : ICombatManager
    {
        private bool _isInCombat = false;
        private float _lastCombatTime = 0f;
        private readonly float _combatCooldown = 3f; // 3秒間戦闘がないと非戦闘状態

        // ヘルス管理
        private float _currentHealth = 100f;
        private float _maxHealth = 100f;

        // ICombatManager必須プロパティ実装
        public bool IsInCombat => _isInCombat;
        public float Health => _currentHealth;
        public float MaxHealth => _maxHealth;
        public bool IsAlive => _currentHealth > 0f;
        public float HealthPercentage => _maxHealth > 0f ? _currentHealth / _maxHealth : 0f;
        public float CombatIntensity { get; private set; } = 0f;

        // ICombatManager必須イベント実装
        public event Action<float> OnHealthChanged;
        public event Action<bool> OnCombatStateChanged;
        public event Action OnPlayerDeath;
        public event Action<float, Vector3, GameObject> OnDamageTaken;

        // 追加イベント
        public event Action<float, GameObject> OnDamageDealt;
        public event Action<float, GameObject> OnDamageReceived;
        public event Action<GameObject> OnEnemyKilled;

        // 戦闘状態管理
        public void StartCombat()
        {
            if (!_isInCombat)
            {
                _isInCombat = true;
                _lastCombatTime = Time.time;

                OnCombatStateChanged?.Invoke(true);

                // ServiceLocator経由でオーディオサービス取得
                var audioService = asterivo.Unity60.Core.ServiceLocator.GetService<asterivo.Unity60.Core.Audio.Interfaces.IAudioService>();
                audioService?.PlayMusic("CombatMusic");

                Debug.Log("[CombatManager] Combat started");
            }
        }

        public void EndCombat()
        {
            if (_isInCombat)
            {
                _isInCombat = false;
                CombatIntensity = 0f;

                OnCombatStateChanged?.Invoke(false);

                // ServiceLocator経由でオーディオサービス取得
                var audioService = asterivo.Unity60.Core.ServiceLocator.GetService<asterivo.Unity60.Core.Audio.Interfaces.IAudioService>();
                audioService?.StopMusic();

                Debug.Log("[CombatManager] Combat ended");
            }
        }

        // ICombatManager必須メソッド実装
        public void TakeDamage(float damage, Vector3 source)
        {
            TakeDamage(damage, source, null);
        }

        public void TakeDamage(float damage, Vector3 source, GameObject attacker)
        {
            if (damage <= 0f || !IsAlive)
                return;

            float previousHealth = _currentHealth;
            _currentHealth = Mathf.Max(0f, _currentHealth - damage);

            // 戦闘状態更新
            StartCombat();
            _lastCombatTime = Time.time;
            CombatIntensity = Mathf.Min(1f, CombatIntensity + damage * 0.02f);

            // ServiceLocator経由でAudioサービス取得
            var audioService = asterivo.Unity60.Core.ServiceLocator.GetService<asterivo.Unity60.Core.Audio.Interfaces.IAudioService>();
            audioService?.PlaySFX("PlayerHit", Vector3.zero);

            // イベント発行
            OnHealthChanged?.Invoke(_currentHealth);
            OnDamageTaken?.Invoke(damage, source, attacker);

            // 死亡チェック
            if (!IsAlive && previousHealth > 0f)
            {
                OnPlayerDeath?.Invoke();
                Debug.Log("[CombatManager] Player died");
            }

            Debug.Log($"[CombatManager] Took {damage} damage from {(attacker?.name ?? "unknown")}. Health: {_currentHealth}/{_maxHealth}");
        }

        public void Heal(float amount)
        {
            if (amount <= 0f || !IsAlive)
                return;

            float previousHealth = _currentHealth;
            _currentHealth = Mathf.Min(_maxHealth, _currentHealth + amount);

            if (_currentHealth != previousHealth)
            {
                OnHealthChanged?.Invoke(_currentHealth);
                Debug.Log($"[CombatManager] Healed for {amount}. Health: {_currentHealth}/{_maxHealth}");
            }
        }

        public void SetCombatState(bool inCombat)
        {
            if (inCombat)
            {
                StartCombat();
            }
            else
            {
                EndCombat();
            }
        }

        public void ResetHealth()
        {
            float previousHealth = _currentHealth;
            _currentHealth = _maxHealth;

            if (_currentHealth != previousHealth)
            {
                OnHealthChanged?.Invoke(_currentHealth);
                Debug.Log($"[CombatManager] Health reset to {_currentHealth}");
            }
        }

        public void SetMaxHealth(float maxHealth)
        {
            if (maxHealth <= 0f)
            {
                Debug.LogWarning("[CombatManager] Invalid max health value");
                return;
            }

            float healthRatio = HealthPercentage;
            _maxHealth = maxHealth;
            _currentHealth = _maxHealth * healthRatio; // 現在の割合を維持

            OnHealthChanged?.Invoke(_currentHealth);
            Debug.Log($"[CombatManager] Max health set to {_maxHealth}. Current health: {_currentHealth}");
        }

        public void UpdateCombatState()
        {
            // 戦闘クールダウンチェック
            if (_isInCombat && Time.time - _lastCombatTime > _combatCooldown)
            {
                EndCombat();
            }

            // 戦闘強度の減衰
            if (CombatIntensity > 0f)
            {
                CombatIntensity = Mathf.Max(0f, CombatIntensity - Time.deltaTime * 0.5f);
            }
        }

        // ダメージ処理
        public void DealDamage(GameObject target, float damage, DamageType damageType)
        {
            if (target == null || damage <= 0f)
            {
                Debug.LogWarning("[CombatManager] Invalid damage parameters");
                return;
            }

            // 戦闘状態更新
            StartCombat();
            _lastCombatTime = Time.time;
            CombatIntensity = Mathf.Min(1f, CombatIntensity + damage * 0.01f);

            // ServiceLocator経由でAudioサービス取得
            var audioService = asterivo.Unity60.Core.ServiceLocator.GetService<asterivo.Unity60.Core.Audio.Interfaces.IAudioService>();

            // ダメージタイプ別の処理
            switch (damageType)
            {
                case DamageType.Physical:
                    audioService?.PlaySFX("BulletHit", target.transform.position);
                    break;
                case DamageType.Explosive:
                    audioService?.PlaySFX("ExplosionHit", target.transform.position);
                    break;
                case DamageType.Melee:
                    audioService?.PlaySFX("MeleeHit", target.transform.position);
                    break;
                case DamageType.Fire:
                    audioService?.PlaySFX("FireDamage", target.transform.position);
                    break;
                case DamageType.Electric:
                    audioService?.PlaySFX("ElectricShock", target.transform.position);
                    break;
                case DamageType.Sniper:
                    audioService?.PlaySFX("SniperHit", target.transform.position);
                    break;
                default:
                    audioService?.PlaySFX("DefaultHit", target.transform.position);
                    break;
            }

            // ダメージイベント発行
            OnDamageDealt?.Invoke(damage, target);

            Debug.Log($"[CombatManager] Dealt {damage} {damageType} damage to {target.name}");
        }

        public void ReceiveDamage(GameObject source, float damage, DamageType damageType)
        {
            if (source == null || damage <= 0f)
            {
                Debug.LogWarning("[CombatManager] Invalid damage received parameters");
                return;
            }

            // 戦闘状態更新
            StartCombat();
            _lastCombatTime = Time.time;
            CombatIntensity = Mathf.Min(1f, CombatIntensity + damage * 0.02f); // 被ダメージは強度を多めに上げる

            // ServiceLocator経由でAudioサービス取得
            var audioService = asterivo.Unity60.Core.ServiceLocator.GetService<asterivo.Unity60.Core.Audio.Interfaces.IAudioService>();
            audioService?.PlaySFX("PlayerHit", Vector3.zero);

            // ダメージイベント発行
            OnDamageReceived?.Invoke(damage, source);

            Debug.Log($"[CombatManager] Received {damage} {damageType} damage from {source.name}");
        }

        public void RegisterKill(GameObject enemy)
        {
            if (enemy == null)
            {
                Debug.LogWarning("[CombatManager] Cannot register null enemy kill");
                return;
            }

            // キルイベント発行
            OnEnemyKilled?.Invoke(enemy);

            // ServiceLocator経由でAudioサービス取得
            var audioService = asterivo.Unity60.Core.ServiceLocator.GetService<asterivo.Unity60.Core.Audio.Interfaces.IAudioService>();
            audioService?.PlaySFX("EnemyKilled", enemy.transform.position);

            Debug.Log($"[CombatManager] Enemy killed: {enemy.name}");
        }

        // 戦闘統計
        public void ResetCombatStats()
        {
            CombatIntensity = 0f;
            _lastCombatTime = 0f;

            Debug.Log("[CombatManager] Combat stats reset");
        }

        public CombatStatistics GetCombatStats()
        {
            return new CombatStatistics
            {
                IsInCombat = _isInCombat,
                CombatIntensity = CombatIntensity,
                LastCombatTime = _lastCombatTime,
                TimeSinceLastCombat = Time.time - _lastCombatTime
            };
        }
    }

    /// <summary>
    /// 戦闘統計データ
    /// </summary>
    [System.Serializable]
    public class CombatStatistics
    {
        public bool IsInCombat;
        public float CombatIntensity;
        public float LastCombatTime;
        public float TimeSinceLastCombat;
    }
}