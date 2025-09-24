using System.Collections.Generic;
using UnityEngine;
using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Core.Commands;
using asterivo.Unity60.Features.Combat.Interfaces;
using asterivo.Unity60.Features.Combat.Events;

namespace asterivo.Unity60.Features.Combat.Services
{
    /// <summary>
    /// 戦闘システムサービスの実装
    /// ServiceLocatorパターンで管理され、戦闘関連の中央制御を行う
    /// </summary>
    public class CombatService : ICombatService
    {
        private readonly Dictionary<GameObject, IHealth> _healthComponents = new Dictionary<GameObject, IHealth>();
        private readonly HashSet<GameObject> _combatants = new HashSet<GameObject>();
        private readonly Dictionary<GameObject, float> _combatStartTimes = new Dictionary<GameObject, float>();
        private CombatStatistics _statistics;
        private IEventManager _eventManager;
        private bool _isInitialized = false;

        #region IService Implementation

        public void OnServiceRegistered()
        {
            if (_isInitialized) return;

            // EventManagerを取得
            _eventManager = ServiceLocator.TryGet<IEventManager>(out var eventManager) ? eventManager : null;

            _statistics.Reset();
            _healthComponents.Clear();
            _combatants.Clear();
            _combatStartTimes.Clear();
            _isInitialized = true;

            Debug.Log("[CombatService] Service Registered");
        }

        public void OnServiceUnregistered()
        {
            _healthComponents.Clear();
            _combatants.Clear();
            _combatStartTimes.Clear();
            _isInitialized = false;

            Debug.Log("[CombatService] Service Unregistered");
        }

        public bool IsServiceActive => _isInitialized;

        public string ServiceName => "CombatService";

        #endregion

        #region ICombatService Implementation

        public float DealDamage(GameObject target, float damage, DamageInfo damageInfo = default)
        {
            if (!_isInitialized || target == null || damage <= 0)
                return 0f;

            // ヘルスコンポーネントを取得
            var health = GetHealth(target);
            if (health == null || !health.IsAlive)
                return 0f;

            // ダメージ情報が提供されていない場合は作成
            if (damageInfo.Equals(default(DamageInfo)))
            {
                damageInfo = new DamageInfo(damage);
            }

            // ダメージを与える
            float actualDamage = health.TakeDamage(damage, damageInfo);

            // 統計を更新
            _statistics.TotalDamageDealt += (int)actualDamage;

            // イベントを発行
            RaiseDamageEvent(damageInfo.attacker, target, damageInfo, actualDamage);

            // 死亡チェック
            if (!health.IsAlive)
            {
                HandleDeath(target, damageInfo);
            }

            return actualDamage;
        }

        public float HealTarget(GameObject target, float amount)
        {
            if (!_isInitialized || target == null || amount <= 0)
                return 0f;

            var health = GetHealth(target);
            if (health == null)
                return 0f;

            float actualHeal = health.Heal(amount);

            // 統計を更新
            _statistics.TotalHealing += (int)actualHeal;

            // イベントを発行
            RaiseHealEvent(target, actualHeal);

            return actualHeal;
        }

        public void StartCombat(GameObject attacker, GameObject target)
        {
            if (!_isInitialized || attacker == null || target == null)
                return;

            // 攻撃者を戦闘状態に
            if (!_combatants.Contains(attacker))
            {
                _combatants.Add(attacker);
                _combatStartTimes[attacker] = Time.time;
                _statistics.ActiveCombatants++;

                RaiseCombatStartedEvent(attacker, target);
            }

            // 標的を戦闘状態に
            if (!_combatants.Contains(target))
            {
                _combatants.Add(target);
                _combatStartTimes[target] = Time.time;
                _statistics.ActiveCombatants++;

                RaiseCombatStartedEvent(target, attacker);
            }
        }

        public void EndCombat(GameObject participant)
        {
            if (!_isInitialized || participant == null)
                return;

            if (_combatants.Remove(participant))
            {
                // 戦闘時間を統計に追加
                if (_combatStartTimes.TryGetValue(participant, out float startTime))
                {
                    _statistics.CombatTime += Time.time - startTime;
                    _combatStartTimes.Remove(participant);
                }

                _statistics.ActiveCombatants--;
                RaiseCombatEndedEvent(participant);
            }
        }

        public bool IsInCombat(GameObject participant)
        {
            return participant != null && _combatants.Contains(participant);
        }

        public DamageCommand GetDamageCommand()
        {
            return CommandPoolManager.GetCommand<DamageCommand>();
        }

        public void ReturnDamageCommand(DamageCommand command)
        {
            if (command != null)
            {
                CommandPoolManager.ReturnCommand(command);
            }
        }

        public void RegisterHealth(IHealth health)
        {
            if (health == null)
                return;

            // IHealthがGameObjectを持つコンポーネントか確認
            if (health is Component component && component != null)
            {
                _healthComponents[component.gameObject] = health;
                Debug.Log($"[CombatService] Registered health component for {component.gameObject.name}");
            }
        }

        public void UnregisterHealth(IHealth health)
        {
            if (health == null)
                return;

            // 登録されているヘルスコンポーネントから削除
            if (health is Component component && component != null)
            {
                if (_healthComponents.Remove(component.gameObject))
                {
                    Debug.Log($"[CombatService] Unregistered health component for {component.gameObject.name}");
                }
            }
        }

        public IHealth GetHealth(GameObject gameObject)
        {
            if (gameObject == null)
                return null;

            // キャッシュから取得
            if (_healthComponents.TryGetValue(gameObject, out var health))
            {
                return health;
            }

            // コンポーネントから検索
            health = gameObject.GetComponent<IHealth>();
            if (health != null)
            {
                RegisterHealth(health);
            }

            return health;
        }

        public CombatStatistics GetStatistics()
        {
            return _statistics;
        }

        #endregion

        #region Private Methods

        private void HandleDeath(GameObject victim, DamageInfo lastDamageInfo)
        {
            _statistics.Deaths++;

            // 攻撃者がいる場合はキル数を増やす
            if (lastDamageInfo.attacker != null)
            {
                _statistics.Kills++;
            }

            // 戦闘状態を終了
            EndCombat(victim);

            // 死亡イベントを発行
            RaiseDeathEvent(victim, lastDamageInfo);
        }

        #endregion

        #region Event Methods

        private void RaiseDamageEvent(GameObject attacker, GameObject target, DamageInfo damageInfo, float actualDamage)
        {
            if (_eventManager == null) return;

            var eventData = new DamageEventData(attacker, target, damageInfo, actualDamage);
            _eventManager.RaiseEvent(CombatEventNames.OnDamageDealt, eventData);
            _eventManager.RaiseEvent(CombatEventNames.OnDamageReceived, eventData);

            if (damageInfo.isCritical)
            {
                _eventManager.RaiseEvent(CombatEventNames.OnCriticalHit, eventData);
            }
        }

        private void RaiseHealEvent(GameObject target, float healAmount)
        {
            if (_eventManager == null) return;

            var eventData = new HealEventData(target, target, healAmount, HealType.Instant);
            _eventManager.RaiseEvent(CombatEventNames.OnHeal, eventData);
            _eventManager.RaiseEvent(CombatEventNames.OnHealthRestored, eventData);
        }

        private void RaiseDeathEvent(GameObject victim, DamageInfo lastDamageInfo)
        {
            if (_eventManager == null) return;

            var eventData = new DeathEventData(victim, lastDamageInfo.attacker, lastDamageInfo);
            _eventManager.RaiseEvent(CombatEventNames.OnDeath, eventData);
        }

        private void RaiseCombatStartedEvent(GameObject participant, GameObject opponent)
        {
            if (_eventManager == null) return;

            _eventManager.RaiseEvent(CombatEventNames.OnCombatStarted,
                new { Participant = participant, Opponent = opponent });
        }

        private void RaiseCombatEndedEvent(GameObject participant)
        {
            if (_eventManager == null) return;

            _eventManager.RaiseEvent(CombatEventNames.OnCombatEnded, participant);
        }

        #endregion
    }
}