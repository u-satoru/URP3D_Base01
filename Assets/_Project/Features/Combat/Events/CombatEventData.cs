using UnityEngine;

namespace asterivo.Unity60.Features.Combat.Events
{
    /// <summary>
    /// 戦闘関連イベントの基底データクラス
    /// </summary>
    public abstract class CombatEventData
    {
        public GameObject Source { get; protected set; }
        public float Timestamp { get; protected set; }

        protected CombatEventData(GameObject source)
        {
            Source = source;
            Timestamp = Time.time;
        }
    }

    /// <summary>
    /// ダメージイベントデータ
    /// </summary>
    public class DamageEventData : CombatEventData
    {
        public DamageInfo DamageInfo { get; private set; }
        public GameObject Target { get; private set; }
        public float ActualDamage { get; private set; }

        public DamageEventData(GameObject source, GameObject target, DamageInfo damageInfo, float actualDamage)
            : base(source)
        {
            Target = target;
            DamageInfo = damageInfo;
            ActualDamage = actualDamage;
        }
    }

    /// <summary>
    /// 死亡イベントデータ
    /// </summary>
    public class DeathEventData : CombatEventData
    {
        public GameObject Killer { get; private set; }
        public DamageInfo LastDamageInfo { get; private set; }

        public DeathEventData(GameObject victim, GameObject killer, DamageInfo lastDamageInfo)
            : base(victim)
        {
            Killer = killer;
            LastDamageInfo = lastDamageInfo;
        }
    }

    /// <summary>
    /// 回復イベントデータ
    /// </summary>
    public class HealEventData : CombatEventData
    {
        public GameObject Target { get; private set; }
        public float HealAmount { get; private set; }
        public HealType HealType { get; private set; }

        public HealEventData(GameObject source, GameObject target, float healAmount, HealType healType)
            : base(source)
        {
            Target = target;
            HealAmount = healAmount;
            HealType = healType;
        }
    }

    /// <summary>
    /// 武器装備イベントデータ
    /// </summary>
    public class WeaponEquipEventData : CombatEventData
    {
        public string WeaponId { get; private set; }
        public WeaponType WeaponType { get; private set; }

        public WeaponEquipEventData(GameObject source, string weaponId, WeaponType weaponType)
            : base(source)
        {
            WeaponId = weaponId;
            WeaponType = weaponType;
        }
    }

    /// <summary>
    /// 回復タイプ
    /// </summary>
    public enum HealType
    {
        Instant,        // 即座に回復
        OverTime,       // 時間経過で回復
        Regeneration,   // 自然回復
        Item,          // アイテムによる回復
        Skill          // スキルによる回復
    }

    /// <summary>
    /// 武器タイプ
    /// </summary>
    public enum WeaponType
    {
        None,
        Melee,         // 近接武器
        Ranged,        // 遠距離武器
        Explosive,     // 爆発武器
        Special        // 特殊武器
    }
}