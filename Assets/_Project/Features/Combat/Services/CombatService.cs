using System.Collections.Generic;
using UnityEngine;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Commands;
using asterivo.Unity60.Features.Combat.Interfaces;
using asterivo.Unity60.Features.Combat.Events;

namespace asterivo.Unity60.Features.Combat.Services
{
    /// <summary>
    /// 謌ｦ髣倥す繧ｹ繝・Β繧ｵ繝ｼ繝薙せ縺ｮ螳溯｣・
    /// ServiceLocator繝代ち繝ｼ繝ｳ縺ｧ邂｡逅・＆繧後∵姶髣倬未騾｣縺ｮ荳ｭ螟ｮ蛻ｶ蠕｡繧定｡後≧
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

            // EventManager繧貞叙蠕・
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

            // 繝倥Ν繧ｹ繧ｳ繝ｳ繝昴・繝阪Φ繝医ｒ蜿門ｾ・
            var health = GetHealth(target);
            if (health == null || !health.IsAlive)
                return 0f;

            // 繝繝｡繝ｼ繧ｸ諠・ｱ縺梧署萓帙＆繧後※縺・↑縺・ｴ蜷医・菴懈・
            if (damageInfo.Equals(default(DamageInfo)))
            {
                damageInfo = new DamageInfo(damage);
            }

            // 繝繝｡繝ｼ繧ｸ繧剃ｸ弱∴繧・
            float actualDamage = health.TakeDamage(damage, damageInfo);

            // 邨ｱ險医ｒ譖ｴ譁ｰ
            _statistics.TotalDamageDealt += (int)actualDamage;

            // 繧､繝吶Φ繝医ｒ逋ｺ陦・
            RaiseDamageEvent(damageInfo.attacker, target, damageInfo, actualDamage);

            // 豁ｻ莠｡繝√ぉ繝・け
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

            // 邨ｱ險医ｒ譖ｴ譁ｰ
            _statistics.TotalHealing += (int)actualHeal;

            // 繧､繝吶Φ繝医ｒ逋ｺ陦・
            RaiseHealEvent(target, actualHeal);

            return actualHeal;
        }

        public void StartCombat(GameObject attacker, GameObject target)
        {
            if (!_isInitialized || attacker == null || target == null)
                return;

            // 謾ｻ謦・・ｒ謌ｦ髣倡憾諷九↓
            if (!_combatants.Contains(attacker))
            {
                _combatants.Add(attacker);
                _combatStartTimes[attacker] = Time.time;
                _statistics.ActiveCombatants++;

                RaiseCombatStartedEvent(attacker, target);
            }

            // 讓咏噪繧呈姶髣倡憾諷九↓
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
                // 謌ｦ髣俶凾髢薙ｒ邨ｱ險医↓霑ｽ蜉
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

            // IHealth縺隈ameObject繧呈戟縺､繧ｳ繝ｳ繝昴・繝阪Φ繝医°遒ｺ隱・
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

            // 逋ｻ骭ｲ縺輔ｌ縺ｦ縺・ｋ繝倥Ν繧ｹ繧ｳ繝ｳ繝昴・繝阪Φ繝医°繧牙炎髯､
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

            // 繧ｭ繝｣繝・す繝･縺九ｉ蜿門ｾ・
            if (_healthComponents.TryGetValue(gameObject, out var health))
            {
                return health;
            }

            // 繧ｳ繝ｳ繝昴・繝阪Φ繝医°繧画､懃ｴ｢
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

            // 謾ｻ謦・・′縺・ｋ蝣ｴ蜷医・繧ｭ繝ｫ謨ｰ繧貞｢励ｄ縺・
            if (lastDamageInfo.attacker != null)
            {
                _statistics.Kills++;
            }

            // 謌ｦ髣倡憾諷九ｒ邨ゆｺ・
            EndCombat(victim);

            // 豁ｻ莠｡繧､繝吶Φ繝医ｒ逋ｺ陦・
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


