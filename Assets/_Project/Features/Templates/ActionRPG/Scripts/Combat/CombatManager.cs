using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Features.Templates.ActionRPG.Character;
using asterivo.Unity60.Features.Templates.ActionRPG.Equipment;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Features.Templates.ActionRPG.Combat
{
    /// <summary>
    /// Action RPGテンプレート用戦闘管理システム
    /// ダメージ計算、戦闘状態管理、スキル使用を統合管理する
    /// </summary>
    public class CombatManager : MonoBehaviour
    {
        [BoxGroup("Configuration")]
        [SerializeField]
        private bool debugMode = true;

        [BoxGroup("Configuration")]
        [SerializeField]
        private float combatTimeout = 5f;

        [BoxGroup("Events")]
        // [SerializeField]
        // private GameEvent<DamageInfo> onDamageDealt; // TODO: Implement after creating DamageInfo class

        [BoxGroup("Events")]
        // [SerializeField]
        // private GameEvent<DamageInfo> onDamageReceived; // TODO: Implement after creating DamageInfo class

        [BoxGroup("Events")]
        [SerializeField]
        private GameEvent onCombatStart;

        [BoxGroup("Events")]
        [SerializeField]
        private GameEvent onCombatEnd;

        [BoxGroup("Current State")]
        [ShowInInspector, ReadOnly]
        private bool isInCombat = false;

        [BoxGroup("Current State")]
        [ShowInInspector, ReadOnly]
        private float combatTimer = 0f;

        [BoxGroup("Current State")]
        [ShowInInspector, ReadOnly]
        private List<GameObject> combatTargets = new List<GameObject>();

        [BoxGroup("Statistics")]
        [ShowInInspector, ReadOnly]
        public int TotalDamageDealt { get; private set; }

        [BoxGroup("Statistics")]
        [ShowInInspector, ReadOnly]
        public int TotalDamageReceived { get; private set; }

        [BoxGroup("Statistics")]
        [ShowInInspector, ReadOnly]
        public int CombatSessionsCount { get; private set; }

        // 依存システム
        private CharacterProgressionManager characterProgression;
        private EquipmentManager equipmentManager;
        private Health playerHealth;

        // プロパティ
        public bool IsInCombat => isInCombat;
        public IReadOnlyList<GameObject> CombatTargets => combatTargets.AsReadOnly();

        // イベント
        public event Action<DamageInfo> OnDamageDealt;
        public event Action<DamageInfo> OnDamageReceived;
        public event Action OnCombatStart;
        public event Action OnCombatEnd;

        private void Awake()
        {
            InitializeCombat();
        }

        private void Start()
        {
            InitializeDependencies();
            RegisterWithServices();
        }

        private void Update()
        {
            if (isInCombat)
            {
                UpdateCombatTimer();
            }
        }

        private void OnDestroy()
        {
            UnregisterFromServices();
        }

        /// <summary>
        /// 戦闘システムの初期化
        /// </summary>
        private void InitializeCombat()
        {
            isInCombat = false;
            combatTimer = 0f;
            combatTargets.Clear();

            TotalDamageDealt = 0;
            TotalDamageReceived = 0;
            CombatSessionsCount = 0;

            LogDebug("[CombatManager] Combat system initialized");
        }

        /// <summary>
        /// 依存システムの初期化
        /// </summary>
        private void InitializeDependencies()
        {
            characterProgression = GetComponent<CharacterProgressionManager>();
            if (characterProgression == null)
            {
                characterProgression = FindFirstObjectByType<CharacterProgressionManager>();
            }

            equipmentManager = GetComponent<EquipmentManager>();
            if (equipmentManager == null)
            {
                equipmentManager = FindFirstObjectByType<EquipmentManager>();
            }

            playerHealth = GetComponent<Health>();
            if (playerHealth == null)
            {
                playerHealth = FindFirstObjectByType<Health>();
            }

            LogDebug("[CombatManager] Dependencies initialized");
        }

        /// <summary>
        /// サービスへの登録
        /// </summary>
        private void RegisterWithServices()
        {
            if (ServiceLocator.Instance != null)
            {
                ServiceLocator.Instance.RegisterService<CombatManager>(this);
                LogDebug("[CombatManager] Registered with ServiceLocator");
            }
        }

        /// <summary>
        /// サービスの登録解除
        /// </summary>
        private void UnregisterFromServices()
        {
            if (ServiceLocator.Instance != null)
            {
                ServiceLocator.Instance.UnregisterService<CombatManager>();
                LogDebug("[CombatManager] Unregistered from ServiceLocator");
            }
        }

        #region Combat Management

        /// <summary>
        /// 戦闘開始
        /// </summary>
        /// <param name="target">戦闘対象</param>
        public void StartCombat(GameObject target)
        {
            if (!isInCombat)
            {
                isInCombat = true;
                combatTimer = combatTimeout;
                CombatSessionsCount++;

                OnCombatStart?.Invoke();
                onCombatStart?.Raise();

                LogDebug($"[CombatManager] Combat started - Session #{CombatSessionsCount}");
            }

            // 戦闘対象に追加
            if (target != null && !combatTargets.Contains(target))
            {
                combatTargets.Add(target);
                LogDebug($"[CombatManager] Added combat target: {target.name}");
            }

            // タイマーリセット
            combatTimer = combatTimeout;
        }

        /// <summary>
        /// 戦闘終了
        /// </summary>
        public void EndCombat()
        {
            if (isInCombat)
            {
                isInCombat = false;
                combatTimer = 0f;
                combatTargets.Clear();

                OnCombatEnd?.Invoke();
                onCombatEnd?.Raise();

                LogDebug("[CombatManager] Combat ended");
            }
        }

        /// <summary>
        /// 戦闘タイマー更新
        /// </summary>
        private void UpdateCombatTimer()
        {
            combatTimer -= Time.deltaTime;

            if (combatTimer <= 0f)
            {
                EndCombat();
            }
        }

        #endregion

        #region Damage System

        /// <summary>
        /// ダメージを与える
        /// </summary>
        /// <param name="target">対象</param>
        /// <param name="baseDamage">基本ダメージ</param>
        /// <param name="damageType">ダメージタイプ</param>
        /// <param name="isCritical">クリティカルヒットかどうか</param>
        public void DealDamage(GameObject target, int baseDamage, DamageType damageType = DamageType.Physical, bool isCritical = false)
        {
            if (target == null) return;

            var targetHealth = target.GetComponent<Health>();
            if (targetHealth == null) return;

            // ダメージ計算
            int finalDamage = CalculateDamage(baseDamage, damageType, isCritical);

            // ダメージ情報作成
            var damageInfo = new DamageInfo
            {
                attacker = gameObject,
                target = target,
                baseDamage = baseDamage,
                finalDamage = finalDamage,
                damageType = damageType,
                isCritical = isCritical,
                timestamp = Time.time
            };

            // ダメージ適用
            targetHealth.TakeDamage(finalDamage);

            // 統計更新
            TotalDamageDealt += finalDamage;

            // イベント発行
            OnDamageDealt?.Invoke(damageInfo);
            onDamageDealt?.Raise(damageInfo);

            // 戦闘開始（まだ戦闘中でない場合）
            StartCombat(target);

            LogDebug($"[CombatManager] Dealt {finalDamage} {damageType} damage to {target.name} (Critical: {isCritical})");
        }

        /// <summary>
        /// ダメージを受ける
        /// </summary>
        /// <param name="attacker">攻撃者</param>
        /// <param name="damage">ダメージ量</param>
        /// <param name="damageType">ダメージタイプ</param>
        public void ReceiveDamage(GameObject attacker, int damage, DamageType damageType = DamageType.Physical)
        {
            // ダメージ軽減計算
            int finalDamage = CalculateDamageReduction(damage, damageType);

            // ダメージ情報作成
            var damageInfo = new DamageInfo
            {
                attacker = attacker,
                target = gameObject,
                baseDamage = damage,
                finalDamage = finalDamage,
                damageType = damageType,
                isCritical = false,
                timestamp = Time.time
            };

            // 統計更新
            TotalDamageReceived += finalDamage;

            // イベント発行
            OnDamageReceived?.Invoke(damageInfo);
            onDamageReceived?.Raise(damageInfo);

            // 戦闘開始
            StartCombat(attacker);

            LogDebug($"[CombatManager] Received {finalDamage} {damageType} damage from {(attacker ? attacker.name : "Unknown")}");
        }

        /// <summary>
        /// 与えるダメージの計算
        /// </summary>
        private int CalculateDamage(int baseDamage, DamageType damageType, bool isCritical)
        {
            int finalDamage = baseDamage;

            // 装備による攻撃力ボーナス
            if (equipmentManager != null)
            {
                var equipmentStats = equipmentManager.TotalEquipmentStats;

                switch (damageType)
                {
                    case DamageType.Physical:
                        finalDamage += equipmentStats.attackPower;
                        break;
                    case DamageType.Magical:
                        finalDamage += equipmentStats.magicPower;
                        break;
                }
            }

            // キャラクター属性によるボーナス
            if (characterProgression != null)
            {
                switch (damageType)
                {
                    case DamageType.Physical:
                        finalDamage += characterProgression.CurrentAttributes.strength * 2;
                        break;
                    case DamageType.Magical:
                        finalDamage += characterProgression.CurrentAttributes.intelligence * 2;
                        break;
                }
            }

            // クリティカルダメージ
            if (isCritical)
            {
                finalDamage = Mathf.RoundToInt(finalDamage * GetCriticalMultiplier());
            }

            return Mathf.Max(1, finalDamage); // 最低1ダメージ保証
        }

        /// <summary>
        /// 受けるダメージの軽減計算
        /// </summary>
        private int CalculateDamageReduction(int baseDamage, DamageType damageType)
        {
            int finalDamage = baseDamage;
            int defense = 0;

            // 装備による防御力
            if (equipmentManager != null)
            {
                var equipmentStats = equipmentManager.TotalEquipmentStats;

                switch (damageType)
                {
                    case DamageType.Physical:
                        defense = equipmentStats.defense;
                        break;
                    case DamageType.Magical:
                        defense = equipmentStats.magicDefense;
                        break;
                }
            }

            // キャラクター属性による防御ボーナス
            if (characterProgression != null)
            {
                switch (damageType)
                {
                    case DamageType.Physical:
                        defense += characterProgression.CurrentAttributes.vitality;
                        break;
                    case DamageType.Magical:
                        defense += characterProgression.CurrentAttributes.wisdom;
                        break;
                }
            }

            // ダメージ軽減計算（防御力の50%がダメージ軽減）
            finalDamage = Mathf.Max(1, finalDamage - (defense / 2));

            return finalDamage;
        }

        /// <summary>
        /// クリティカル倍率を取得
        /// </summary>
        private float GetCriticalMultiplier()
        {
            float baseMultiplier = 2.0f;

            // キャラクター属性による倍率向上
            if (characterProgression != null)
            {
                float dexterityBonus = characterProgression.CurrentAttributes.dexterity * 0.05f;
                baseMultiplier += dexterityBonus;
            }

            return baseMultiplier;
        }

        /// <summary>
        /// クリティカル率を取得
        /// </summary>
        public float GetCriticalChance()
        {
            float baseCriticalChance = 0.05f; // 5%

            // キャラクター属性によるクリティカル率向上
            if (characterProgression != null)
            {
                float dexterityBonus = characterProgression.CurrentAttributes.dexterity * 0.01f;
                baseCriticalChance += dexterityBonus;
            }

            // 装備による幸運ボーナス
            if (equipmentManager != null)
            {
                var equipmentStats = equipmentManager.TotalEquipmentStats;
                baseCriticalChance += equipmentStats.luckBonus * 0.005f;
            }

            return Mathf.Clamp01(baseCriticalChance);
        }

        #endregion

        #region Skill System Integration

        /// <summary>
        /// スキルを使用
        /// </summary>
        /// <param name="skillName">スキル名</param>
        /// <param name="target">対象（nullの場合は自身）</param>
        public bool UseSkill(string skillName, GameObject target = null)
        {
            if (characterProgression == null) return false;

            // スキルが習得済みかチェック
            if (!characterProgression.IsSkillUnlocked(skillName))
            {
                LogDebug($"[CombatManager] Skill '{skillName}' is not unlocked");
                return false;
            }

            int skillRank = characterProgression.GetSkillRank(skillName);
            if (skillRank <= 0)
            {
                LogDebug($"[CombatManager] Skill '{skillName}' has no ranks");
                return false;
            }

            // スキル効果の適用
            ApplySkillEffect(skillName, skillRank, target ?? gameObject);

            LogDebug($"[CombatManager] Used skill '{skillName}' (Rank {skillRank})");
            return true;
        }

        /// <summary>
        /// スキル効果の適用
        /// </summary>
        private void ApplySkillEffect(string skillName, int skillRank, GameObject target)
        {
            switch (skillName)
            {
                case "Power Strike":
                    // 強力な物理攻撃
                    int damage = 20 + (skillRank * 10);
                    bool isCritical = UnityEngine.Random.value < GetCriticalChance() * 1.5f;
                    DealDamage(target, damage, DamageType.Physical, isCritical);
                    break;

                case "Fireball":
                    // 魔法攻撃
                    int magicDamage = 15 + (skillRank * 8);
                    DealDamage(target, magicDamage, DamageType.Magical);
                    break;

                case "Healing":
                    // 回復スキル
                    var targetHealth = target.GetComponent<Health>();
                    if (targetHealth != null)
                    {
                        int healAmount = 10 + (skillRank * 5);
                        targetHealth.Heal(healAmount);
                        LogDebug($"[CombatManager] Healed {target.name} for {healAmount} HP");
                    }
                    break;

                default:
                    LogDebug($"[CombatManager] Unknown skill: {skillName}");
                    break;
            }
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// 戦闘統計をリセット
        /// </summary>
        public void ResetCombatStatistics()
        {
            TotalDamageDealt = 0;
            TotalDamageReceived = 0;
            CombatSessionsCount = 0;

            LogDebug("[CombatManager] Combat statistics reset");
        }

        #endregion

        #region Debug Support

        [Button("Deal Test Damage")]
        [ShowIf("debugMode")]
        private void DealTestDamage()
        {
            var target = GameObject.FindGameObjectWithTag("Enemy");
            if (target != null)
            {
                DealDamage(target, 25, DamageType.Physical);
            }
            else
            {
                LogDebug("[CombatManager] No enemy target found for test damage");
            }
        }

        [Button("Use Test Skill")]
        [ShowIf("debugMode")]
        private void UseTestSkill()
        {
            UseSkill("Power Strike");
        }

        [Button("Show Combat Statistics")]
        [ShowIf("debugMode")]
        private void ShowCombatStatistics()
        {
            LogDebug("=== Combat Statistics ===");
            LogDebug($"Total Damage Dealt: {TotalDamageDealt}");
            LogDebug($"Total Damage Received: {TotalDamageReceived}");
            LogDebug($"Combat Sessions: {CombatSessionsCount}");
            LogDebug($"Is In Combat: {isInCombat}");
            LogDebug($"Combat Targets: {combatTargets.Count}");
            LogDebug($"Critical Chance: {GetCriticalChance():P}");
            LogDebug("========================");
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

    /// <summary>
    /// ダメージ情報
    /// </summary>
    [System.Serializable]
    public class DamageInfo
    {
        public GameObject attacker;
        public GameObject target;
        public int baseDamage;
        public int finalDamage;
        public DamageType damageType;
        public bool isCritical;
        public float timestamp;
    }

    /// <summary>
    /// ダメージタイプ
    /// </summary>
    public enum DamageType
    {
        Physical,
        Magical,
        Fire,
        Ice,
        Lightning,
        Poison,
        True // 軽減不可ダメージ
    }
}