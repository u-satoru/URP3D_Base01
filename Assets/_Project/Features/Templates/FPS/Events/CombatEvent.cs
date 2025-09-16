using System;
using UnityEngine;

namespace asterivo.Unity60.Features.Templates.FPS.Events
{
    /// <summary>
    /// 戦闘関連イベント
    /// ServiceLocator + Event駆動のハイブリッドアーキテクチャ準拠
    /// Core層のGameEventシステム統合
    /// </summary>

    [CreateAssetMenu(fileName = "DamageDealtEvent", menuName = "FPS Template/Events/Damage Dealt Event")]
    public class DamageDealtEvent : ScriptableObject
    {
        [SerializeField] private asterivo.Unity60.Core.Events.GameEvent<DamageData> _damageDealtEvent;

        /// <summary>
        /// ダメージ与えたイベント発行
        /// </summary>
        public void RaiseDamageDealt(DamageData data)
        {
            _damageDealtEvent?.Raise(data);
            Debug.Log($"[CombatEvent] Damage dealt: {data.Amount} to {data.Target?.name} by {data.Source?.name}");
        }

        /// <summary>
        /// ダメージ与えたリスナー登録
        /// </summary>
        public void RegisterListener(Action<DamageData> callback)
        {
            _damageDealtEvent?.AddListener(callback);
        }

        /// <summary>
        /// ダメージ与えたリスナー解除
        /// </summary>
        public void UnregisterListener(Action<DamageData> callback)
        {
            _damageDealtEvent?.RemoveListener(callback);
        }
    }

    [CreateAssetMenu(fileName = "DamageReceivedEvent", menuName = "FPS Template/Events/Damage Received Event")]
    public class DamageReceivedEvent : ScriptableObject
    {
        [SerializeField] private asterivo.Unity60.Core.Events.GameEvent<DamageData> _damageReceivedEvent;

        /// <summary>
        /// ダメージ受けたイベント発行
        /// </summary>
        public void RaiseDamageReceived(DamageData data)
        {
            _damageReceivedEvent?.Raise(data);
            Debug.Log($"[CombatEvent] Damage received: {data.Amount} by {data.Target?.name} from {data.Source?.name}");
        }

        /// <summary>
        /// ダメージ受けたリスナー登録
        /// </summary>
        public void RegisterListener(Action<DamageData> callback)
        {
            _damageReceivedEvent?.AddListener(callback);
        }

        /// <summary>
        /// ダメージ受けたリスナー解除
        /// </summary>
        public void UnregisterListener(Action<DamageData> callback)
        {
            _damageReceivedEvent?.RemoveListener(callback);
        }
    }

    [CreateAssetMenu(fileName = "EnemyKilledEvent", menuName = "FPS Template/Events/Enemy Killed Event")]
    public class EnemyKilledEvent : ScriptableObject
    {
        [SerializeField] private asterivo.Unity60.Core.Events.GameEvent<EnemyKillData> _enemyKilledEvent;

        /// <summary>
        /// 敵撃破イベント発行
        /// </summary>
        public void RaiseEnemyKilled(EnemyKillData data)
        {
            _enemyKilledEvent?.Raise(data);
            Debug.Log($"[CombatEvent] Enemy killed: {data.KilledEnemy?.name} by {data.Killer?.name} with {data.WeaponUsed}");
        }

        /// <summary>
        /// 敵撃破リスナー登録
        /// </summary>
        public void RegisterListener(Action<EnemyKillData> callback)
        {
            _enemyKilledEvent?.AddListener(callback);
        }

        /// <summary>
        /// 敵撃破リスナー解除
        /// </summary>
        public void UnregisterListener(Action<EnemyKillData> callback)
        {
            _enemyKilledEvent?.RemoveListener(callback);
        }
    }

    [CreateAssetMenu(fileName = "CombatStateEvent", menuName = "FPS Template/Events/Combat State Event")]
    public class CombatStateEvent : ScriptableObject
    {
        [SerializeField] private asterivo.Unity60.Core.Events.GameEvent<CombatStateData> _combatStartedEvent;
        [SerializeField] private asterivo.Unity60.Core.Events.GameEvent<CombatStateData> _combatEndedEvent;

        /// <summary>
        /// 戦闘開始イベント発行
        /// </summary>
        public void RaiseCombatStarted(CombatStateData data)
        {
            _combatStartedEvent?.Raise(data);
            Debug.Log($"[CombatEvent] Combat started with intensity: {data.Intensity}");
        }

        /// <summary>
        /// 戦闘終了イベント発行
        /// </summary>
        public void RaiseCombatEnded(CombatStateData data)
        {
            _combatEndedEvent?.Raise(data);
            Debug.Log($"[CombatEvent] Combat ended. Duration: {data.Duration}s");
        }

        /// <summary>
        /// 戦闘開始リスナー登録
        /// </summary>
        public void RegisterCombatStartedListener(Action<CombatStateData> callback)
        {
            _combatStartedEvent?.AddListener(callback);
        }

        /// <summary>
        /// 戦闘終了リスナー登録
        /// </summary>
        public void RegisterCombatEndedListener(Action<CombatStateData> callback)
        {
            _combatEndedEvent?.AddListener(callback);
        }

        /// <summary>
        /// 戦闘開始リスナー解除
        /// </summary>
        public void UnregisterCombatStartedListener(Action<CombatStateData> callback)
        {
            _combatStartedEvent?.RemoveListener(callback);
        }

        /// <summary>
        /// 戦闘終了リスナー解除
        /// </summary>
        public void UnregisterCombatEndedListener(Action<CombatStateData> callback)
        {
            _combatEndedEvent?.RemoveListener(callback);
        }
    }

    /// <summary>
    /// ダメージイベントデータ
    /// </summary>
    [System.Serializable]
    public class DamageData
    {
        public float Amount;
        public DamageType DamageType;
        public Vector3 HitPoint;
        public Vector3 HitDirection;
        public GameObject Source;
        public GameObject Target;
        public string WeaponUsed;
        public bool IsHeadshot;
        public bool IsCritical;
        public float DamageMultiplier;

        public DamageData(float amount, DamageType damageType, Vector3 hitPoint, Vector3 hitDirection,
                         GameObject source, GameObject target, string weaponUsed = "",
                         bool isHeadshot = false, bool isCritical = false, float damageMultiplier = 1f)
        {
            Amount = amount;
            DamageType = damageType;
            HitPoint = hitPoint;
            HitDirection = hitDirection;
            Source = source;
            Target = target;
            WeaponUsed = weaponUsed;
            IsHeadshot = isHeadshot;
            IsCritical = isCritical;
            DamageMultiplier = damageMultiplier;
        }
    }

    /// <summary>
    /// 敵撃破イベントデータ
    /// </summary>
    [System.Serializable]
    public class EnemyKillData
    {
        public GameObject KilledEnemy;
        public GameObject Killer;
        public string WeaponUsed;
        public WeaponType WeaponType;
        public Vector3 KillPosition;
        public float Distance;
        public bool IsHeadshot;
        public int ScoreAwarded;
        public float TimeInCombat;

        public EnemyKillData(GameObject killedEnemy, GameObject killer, string weaponUsed, WeaponType weaponType,
                           Vector3 killPosition, float distance = 0f, bool isHeadshot = false,
                           int scoreAwarded = 100, float timeInCombat = 0f)
        {
            KilledEnemy = killedEnemy;
            Killer = killer;
            WeaponUsed = weaponUsed;
            WeaponType = weaponType;
            KillPosition = killPosition;
            Distance = distance;
            IsHeadshot = isHeadshot;
            ScoreAwarded = scoreAwarded;
            TimeInCombat = timeInCombat;
        }
    }

    /// <summary>
    /// 戦闘状態イベントデータ
    /// </summary>
    [System.Serializable]
    public class CombatStateData
    {
        public bool IsInCombat;
        public float Intensity;
        public float Duration;
        public Vector3 CombatLocation;
        public int EnemyCount;
        public GameObject[] Participants;
        public string CombatReason;

        public CombatStateData(bool isInCombat, float intensity = 0f, float duration = 0f,
                              Vector3 combatLocation = default, int enemyCount = 0,
                              GameObject[] participants = null, string combatReason = "")
        {
            IsInCombat = isInCombat;
            Intensity = intensity;
            Duration = duration;
            CombatLocation = combatLocation;
            EnemyCount = enemyCount;
            Participants = participants ?? new GameObject[0];
            CombatReason = combatReason;
        }
    }
}