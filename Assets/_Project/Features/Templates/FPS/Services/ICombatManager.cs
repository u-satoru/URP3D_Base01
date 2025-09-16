using System;
using UnityEngine;

namespace asterivo.Unity60.Features.Templates.FPS.Services
{
    /// <summary>
    /// 戦闘管理サービス（ServiceLocator経由アクセス）
    /// FPS Template専用戦闘システムの核心インターフェース
    /// </summary>
    public interface ICombatManager
    {
        bool IsInCombat { get; }
        float Health { get; }
        float MaxHealth { get; }

        void TakeDamage(float damage, Vector3 source);
        void TakeDamage(float damage, Vector3 source, GameObject attacker);
        void Heal(float amount);
        void SetCombatState(bool inCombat);
        void ResetHealth();
        void SetMaxHealth(float maxHealth);

        bool IsAlive { get; }
        float HealthPercentage { get; }

        event Action<float> OnHealthChanged;
        event Action<bool> OnCombatStateChanged;
        event Action OnPlayerDeath;
        event Action<float, Vector3, GameObject> OnDamageTaken;
    }
}