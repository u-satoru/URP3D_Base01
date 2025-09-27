using UnityEngine;

namespace asterivo.Unity60.Features.Combat.Events
{
    /// <summary>
    /// 戦闘関連イベント名の定義
    /// 文字列の誤字を防ぎ、コンパイル時チェックを可能にする
    /// </summary>
    public static class CombatEventNames
    {
        // ダメージ関連イベント
        public const string OnDamageDealt = "Combat.DamageDealt";
        public const string OnDamageReceived = "Combat.DamageReceived";
        public const string OnCriticalHit = "Combat.CriticalHit";
        public const string OnDamageBlocked = "Combat.DamageBlocked";
        public const string OnDamageAbsorbed = "Combat.DamageAbsorbed";

        // ヘルス関連イベント
        public const string OnHealthChanged = "Combat.HealthChanged";
        public const string OnHealthDepleted = "Combat.HealthDepleted";
        public const string OnHealthRestored = "Combat.HealthRestored";
        public const string OnHeal = "Combat.Heal";
        public const string OnOverheal = "Combat.Overheal";

        // 死亡・復活関連イベント
        public const string OnDeath = "Combat.Death";
        public const string OnRevive = "Combat.Revive";
        public const string OnRespawn = "Combat.Respawn";

        // 戦闘状態関連イベント
        public const string OnCombatStarted = "Combat.Started";
        public const string OnCombatEnded = "Combat.Ended";
        public const string OnEnemyEngaged = "Combat.EnemyEngaged";
        public const string OnEnemyDisengaged = "Combat.EnemyDisengaged";

        // 武器関連イベント
        public const string OnWeaponEquipped = "Combat.WeaponEquipped";
        public const string OnWeaponUnequipped = "Combat.WeaponUnequipped";
        public const string OnWeaponFired = "Combat.WeaponFired";
        public const string OnWeaponReload = "Combat.WeaponReload";
        public const string OnAmmoChanged = "Combat.AmmoChanged";

        // バフ・デバフ関連イベント
        public const string OnBuffApplied = "Combat.BuffApplied";
        public const string OnDebuffApplied = "Combat.DebuffApplied";
        public const string OnStatusEffectExpired = "Combat.StatusEffectExpired";

        // コンボ関連イベント
        public const string OnComboStarted = "Combat.ComboStarted";
        public const string OnComboIncreased = "Combat.ComboIncreased";
        public const string OnComboEnded = "Combat.ComboEnded";

        // 防御関連イベント
        public const string OnBlock = "Combat.Block";
        public const string OnParry = "Combat.Parry";
        public const string OnDodge = "Combat.Dodge";
        public const string OnCounterAttack = "Combat.CounterAttack";
    }
}
