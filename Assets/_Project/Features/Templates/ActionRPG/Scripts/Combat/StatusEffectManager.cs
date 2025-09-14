using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Services;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Features.Templates.ActionRPG.Combat
{
    /// <summary>
    /// ステータス効果管理システム
    /// バフ、デバフ、継続ダメージ、継続回復などの効果を管理
    /// </summary>
    public class StatusEffectManager : MonoBehaviour
    {
        [Header("Status Effect Settings")]
        [SerializeField] private bool debugMode = false;
        [SerializeField] private int maxActiveEffects = 20;

        [Header("Event References")]
        [SerializeField] private StringGameEvent onStatusEffectApplied;
        [SerializeField] private StringGameEvent onStatusEffectRemoved;
        [SerializeField] private GameEvent onStatusEffectsUpdated;

        [ShowInInspector, ReadOnly]
        private List<ActiveStatusEffect> activeEffects = new List<ActiveStatusEffect>();

        // コンポーネント参照
        private Health healthComponent;
        private CombatManager combatManager;

        // イベント
        public event Action<StatusEffectData> OnStatusEffectApplied;
        public event Action<StatusEffectData> OnStatusEffectRemoved;
        public event Action OnStatusEffectsUpdated;

        // プロパティ
        public int ActiveEffectCount => activeEffects.Count;
        public IReadOnlyList<ActiveStatusEffect> ActiveEffects => activeEffects.AsReadOnly();

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeStatusEffectManager();
        }

        private void Start()
        {
            RegisterServices();
        }

        private void Update()
        {
            UpdateStatusEffects();
        }

        private void OnDestroy()
        {
            UnregisterServices();
            ClearAllEffects();
        }

        #endregion

        #region Initialization

        /// <summary>
        /// ステータス効果マネージャーの初期化
        /// </summary>
        private void InitializeStatusEffectManager()
        {
            // コンポーネント参照取得
            healthComponent = GetComponent<Health>();
            combatManager = FindFirstObjectByType<CombatManager>();

            activeEffects = new List<ActiveStatusEffect>();

            LogDebug("[StatusEffect] Status Effect Manager initialized");
        }

        /// <summary>
        /// サービス登録
        /// </summary>
        private void RegisterServices()
        {
            if (ServiceLocator.Instance != null)
            {
                ServiceLocator.Instance.RegisterService<StatusEffectManager>(this);
                LogDebug("[StatusEffect] Registered with ServiceLocator");
            }
        }

        /// <summary>
        /// サービス登録解除
        /// </summary>
        private void UnregisterServices()
        {
            if (ServiceLocator.Instance != null)
            {
                ServiceLocator.Instance.UnregisterService<StatusEffectManager>();
                LogDebug("[StatusEffect] Unregistered from ServiceLocator");
            }
        }

        #endregion

        #region Status Effect Management

        /// <summary>
        /// ステータス効果を適用
        /// </summary>
        public bool ApplyStatusEffect(StatusEffectData effectData, GameObject source = null)
        {
            if (effectData == null)
            {
                LogWarning("[StatusEffect] Cannot apply null status effect");
                return false;
            }

            if (activeEffects.Count >= maxActiveEffects)
            {
                LogWarning("[StatusEffect] Maximum active effects reached");
                return false;
            }

            // 既存の同じ効果をチェック
            var existingEffect = FindActiveEffect(effectData.effectName);
            if (existingEffect != null)
            {
                if (effectData.isStackable)
                {
                    // スタック可能な場合は重ねる
                    StackStatusEffect(existingEffect, effectData);
                }
                else
                {
                    // スタック不可の場合は更新
                    RefreshStatusEffect(existingEffect, effectData);
                }
            }
            else
            {
                // 新しい効果を追加
                AddNewStatusEffect(effectData, source);
            }

            return true;
        }

        /// <summary>
        /// 新しいステータス効果を追加
        /// </summary>
        private void AddNewStatusEffect(StatusEffectData effectData, GameObject source)
        {
            var activeEffect = new ActiveStatusEffect
            {
                effectData = effectData,
                source = source,
                remainingDuration = effectData.duration,
                stacks = 1,
                isActive = true,
                tickTimer = effectData.tickInterval
            };

            activeEffects.Add(activeEffect);

            // 即座に効果を適用
            ApplyEffectImmediate(activeEffect);

            // イベント発行
            OnStatusEffectApplied?.Invoke(effectData);
            onStatusEffectApplied?.Raise(effectData.effectName);
            OnStatusEffectsUpdated?.Invoke();
            onStatusEffectsUpdated?.Raise();

            LogDebug($"[StatusEffect] Applied status effect: {effectData.effectName}");
        }

        /// <summary>
        /// ステータス効果をスタック
        /// </summary>
        private void StackStatusEffect(ActiveStatusEffect existingEffect, StatusEffectData newEffectData)
        {
            if (existingEffect.stacks < newEffectData.maxStacks)
            {
                existingEffect.stacks++;
                existingEffect.remainingDuration = Mathf.Max(existingEffect.remainingDuration, newEffectData.duration);

                LogDebug($"[StatusEffect] Stacked status effect: {newEffectData.effectName} (Stacks: {existingEffect.stacks})");

                OnStatusEffectsUpdated?.Invoke();
                onStatusEffectsUpdated?.Raise();
            }
        }

        /// <summary>
        /// ステータス効果を更新
        /// </summary>
        private void RefreshStatusEffect(ActiveStatusEffect existingEffect, StatusEffectData newEffectData)
        {
            existingEffect.remainingDuration = newEffectData.duration;
            existingEffect.tickTimer = newEffectData.tickInterval;

            LogDebug($"[StatusEffect] Refreshed status effect: {newEffectData.effectName}");

            OnStatusEffectsUpdated?.Invoke();
            onStatusEffectsUpdated?.Raise();
        }

        /// <summary>
        /// ステータス効果を削除
        /// </summary>
        public bool RemoveStatusEffect(string effectName)
        {
            var effect = FindActiveEffect(effectName);
            if (effect == null) return false;

            RemoveStatusEffect(effect);
            return true;
        }

        /// <summary>
        /// ステータス効果を削除
        /// </summary>
        private void RemoveStatusEffect(ActiveStatusEffect effect)
        {
            if (effect == null) return;

            // 効果解除時の処理
            RemoveEffectImmediate(effect);

            activeEffects.Remove(effect);

            // イベント発行
            OnStatusEffectRemoved?.Invoke(effect.effectData);
            onStatusEffectRemoved?.Raise(effect.effectData.effectName);
            OnStatusEffectsUpdated?.Invoke();
            onStatusEffectsUpdated?.Raise();

            LogDebug($"[StatusEffect] Removed status effect: {effect.effectData.effectName}");
        }

        /// <summary>
        /// 全てのステータス効果をクリア
        /// </summary>
        public void ClearAllEffects()
        {
            var effectsToRemove = new List<ActiveStatusEffect>(activeEffects);
            foreach (var effect in effectsToRemove)
            {
                RemoveStatusEffect(effect);
            }

            LogDebug("[StatusEffect] All status effects cleared");
        }

        /// <summary>
        /// 特定タイプのステータス効果をクリア
        /// </summary>
        public void ClearEffectsByType(StatusEffectType effectType)
        {
            var effectsToRemove = new List<ActiveStatusEffect>();

            foreach (var effect in activeEffects)
            {
                if (effect.effectData.effectType == effectType)
                {
                    effectsToRemove.Add(effect);
                }
            }

            foreach (var effect in effectsToRemove)
            {
                RemoveStatusEffect(effect);
            }

            LogDebug($"[StatusEffect] Cleared all effects of type: {effectType}");
        }

        #endregion

        #region Effect Processing

        /// <summary>
        /// ステータス効果の更新
        /// </summary>
        private void UpdateStatusEffects()
        {
            var effectsToRemove = new List<ActiveStatusEffect>();

            foreach (var effect in activeEffects)
            {
                if (!effect.isActive) continue;

                // 継続時間の更新
                if (effect.effectData.duration > 0)
                {
                    effect.remainingDuration -= Time.deltaTime;

                    if (effect.remainingDuration <= 0)
                    {
                        effectsToRemove.Add(effect);
                        continue;
                    }
                }

                // ティック処理
                if (effect.effectData.tickInterval > 0)
                {
                    effect.tickTimer -= Time.deltaTime;

                    if (effect.tickTimer <= 0)
                    {
                        ProcessEffectTick(effect);
                        effect.tickTimer = effect.effectData.tickInterval;
                    }
                }
            }

            // 期限切れの効果を削除
            foreach (var effect in effectsToRemove)
            {
                RemoveStatusEffect(effect);
            }
        }

        /// <summary>
        /// 効果の即座適用
        /// </summary>
        private void ApplyEffectImmediate(ActiveStatusEffect effect)
        {
            switch (effect.effectData.effectType)
            {
                case StatusEffectType.InstantDamage:
                    ApplyInstantDamage(effect);
                    break;
                case StatusEffectType.InstantHeal:
                    ApplyInstantHeal(effect);
                    break;
                case StatusEffectType.StatModifier:
                    ApplyStatModifier(effect, true);
                    break;
            }
        }

        /// <summary>
        /// 効果の解除処理
        /// </summary>
        private void RemoveEffectImmediate(ActiveStatusEffect effect)
        {
            switch (effect.effectData.effectType)
            {
                case StatusEffectType.StatModifier:
                    ApplyStatModifier(effect, false);
                    break;
            }
        }

        /// <summary>
        /// 効果のティック処理
        /// </summary>
        private void ProcessEffectTick(ActiveStatusEffect effect)
        {
            switch (effect.effectData.effectType)
            {
                case StatusEffectType.DamageOverTime:
                    ApplyDamageOverTime(effect);
                    break;
                case StatusEffectType.HealOverTime:
                    ApplyHealOverTime(effect);
                    break;
                case StatusEffectType.ManaOverTime:
                    ApplyManaOverTime(effect);
                    break;
            }
        }

        #endregion

        #region Effect Implementations

        /// <summary>
        /// 即座ダメージ適用
        /// </summary>
        private void ApplyInstantDamage(ActiveStatusEffect effect)
        {
            if (healthComponent != null)
            {
                int damage = Mathf.RoundToInt(effect.effectData.magnitude * effect.stacks);
                healthComponent.TakeDamage(damage);
                LogDebug($"[StatusEffect] Applied instant damage: {damage}");
            }
        }

        /// <summary>
        /// 即座回復適用
        /// </summary>
        private void ApplyInstantHeal(ActiveStatusEffect effect)
        {
            if (healthComponent != null)
            {
                int healing = Mathf.RoundToInt(effect.effectData.magnitude * effect.stacks);
                healthComponent.Heal(healing);
                LogDebug($"[StatusEffect] Applied instant heal: {healing}");
            }
        }

        /// <summary>
        /// 継続ダメージ適用
        /// </summary>
        private void ApplyDamageOverTime(ActiveStatusEffect effect)
        {
            if (healthComponent != null)
            {
                int damage = Mathf.RoundToInt(effect.effectData.magnitude * effect.stacks);
                healthComponent.TakeDamage(damage);
                LogDebug($"[StatusEffect] Applied DoT damage: {damage}");
            }
        }

        /// <summary>
        /// 継続回復適用
        /// </summary>
        private void ApplyHealOverTime(ActiveStatusEffect effect)
        {
            if (healthComponent != null)
            {
                int healing = Mathf.RoundToInt(effect.effectData.magnitude * effect.stacks);
                healthComponent.Heal(healing);
                LogDebug($"[StatusEffect] Applied HoT heal: {healing}");
            }
        }

        /// <summary>
        /// 継続マナ回復適用
        /// </summary>
        private void ApplyManaOverTime(ActiveStatusEffect effect)
        {
            if (healthComponent != null)
            {
                int manaRestore = Mathf.RoundToInt(effect.effectData.magnitude * effect.stacks);
                healthComponent.RestoreMana(manaRestore);
                LogDebug($"[StatusEffect] Applied MoT restore: {manaRestore}");
            }
        }

        /// <summary>
        /// ステータス修正適用
        /// </summary>
        private void ApplyStatModifier(ActiveStatusEffect effect, bool apply)
        {
            // ここでキャラクターステータスに修正を適用
            // CharacterProgressionManager との統合が必要
            var progressionManager = ServiceLocator.Instance?.GetService<CharacterProgressionManager>();
            if (progressionManager != null)
            {
                float modifier = effect.effectData.magnitude * effect.stacks;
                LogDebug($"[StatusEffect] {(apply ? "Applied" : "Removed")} stat modifier: {modifier}");

                // 実際のステータス修正はCharacterProgressionManagerで実装される必要がある
            }
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// アクティブな効果を検索
        /// </summary>
        private ActiveStatusEffect FindActiveEffect(string effectName)
        {
            return activeEffects.Find(e => e.effectData.effectName == effectName);
        }

        /// <summary>
        /// 特定タイプの効果があるかチェック
        /// </summary>
        public bool HasEffectOfType(StatusEffectType effectType)
        {
            return activeEffects.Exists(e => e.effectData.effectType == effectType);
        }

        /// <summary>
        /// 特定の効果があるかチェック
        /// </summary>
        public bool HasEffect(string effectName)
        {
            return FindActiveEffect(effectName) != null;
        }

        /// <summary>
        /// 効果の残り時間を取得
        /// </summary>
        public float GetEffectRemainingTime(string effectName)
        {
            var effect = FindActiveEffect(effectName);
            return effect?.remainingDuration ?? 0f;
        }

        /// <summary>
        /// 効果のスタック数を取得
        /// </summary>
        public int GetEffectStacks(string effectName)
        {
            var effect = FindActiveEffect(effectName);
            return effect?.stacks ?? 0;
        }

        #endregion

        #region Debug Support

        /// <summary>
        /// デバッグログ出力
        /// </summary>
        private void LogDebug(string message)
        {
            if (debugMode)
            {
                Debug.Log(message);
            }
        }

        /// <summary>
        /// 警告ログ出力
        /// </summary>
        private void LogWarning(string message)
        {
            Debug.LogWarning(message);
        }

        /// <summary>
        /// エラーログ出力
        /// </summary>
        private void LogError(string message)
        {
            Debug.LogError(message);
        }

        [Button("Show Active Effects"), ShowIf("debugMode")]
        private void ShowActiveEffectsDebug()
        {
            if (!Application.isPlaying) return;

            LogDebug("=== Active Status Effects ===");
            foreach (var effect in activeEffects)
            {
                LogDebug($"Effect: {effect.effectData.effectName}, " +
                        $"Duration: {effect.remainingDuration:F1}s, " +
                        $"Stacks: {effect.stacks}");
            }
            LogDebug("=============================");
        }

        [Button("Clear All Effects"), ShowIf("debugMode")]
        private void ClearAllEffectsDebug()
        {
            if (Application.isPlaying)
            {
                ClearAllEffects();
            }
        }

        #endregion
    }

    #region Data Structures

    /// <summary>
    /// ステータス効果の種類
    /// </summary>
    public enum StatusEffectType
    {
        InstantDamage,      // 即座ダメージ
        InstantHeal,        // 即座回復
        DamageOverTime,     // 継続ダメージ
        HealOverTime,       // 継続回復
        ManaOverTime,       // 継続マナ回復
        StatModifier,       // ステータス修正
        Stun,              // スタン
        Slow,              // スロー
        Haste,             // ヘイスト
        Shield,            // シールド
        Invisibility       // 透明化
    }

    /// <summary>
    /// ステータス効果データ
    /// </summary>
    [CreateAssetMenu(fileName = "StatusEffect", menuName = "ActionRPG/Combat/Status Effect")]
    public class StatusEffectData : ScriptableObject
    {
        [Header("Basic Properties")]
        public string effectName = "";
        public string description = "";
        public StatusEffectType effectType = StatusEffectType.InstantDamage;
        public Sprite icon = null;

        [Header("Effect Properties")]
        public float magnitude = 0f;           // 効果の強さ
        public float duration = 0f;            // 継続時間 (0 = 永続)
        public float tickInterval = 1f;        // ティック間隔

        [Header("Stacking")]
        public bool isStackable = false;       // スタック可能か
        public int maxStacks = 1;              // 最大スタック数

        [Header("Visual Effects")]
        public Color effectColor = Color.white;
        public GameObject vfxPrefab = null;
        public AudioClip soundEffect = null;
    }

    /// <summary>
    /// アクティブなステータス効果
    /// </summary>
    [System.Serializable]
    public class ActiveStatusEffect
    {
        public StatusEffectData effectData;
        public GameObject source;
        public float remainingDuration;
        public int stacks;
        public bool isActive;
        public float tickTimer;
    }

    #endregion
}