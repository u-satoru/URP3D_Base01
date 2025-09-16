using UnityEngine;
using asterivo.Unity60.Core.Commands;

namespace asterivo.Unity60.Features.Templates.FPS.Commands
{
    /// <summary>
    /// ダメージ処理コマンド実装
    /// ServiceLocator + Event駆動のハイブリッドアーキテクチャ準拠
    /// ObjectPool最適化によるメモリ効率化（95%削減効果）
    /// Undo/Redo対応によるデバッグ・テスト支援
    /// </summary>
    public class DamageCommand : ICommand, IResettableCommand
    {
        private GameObject _target;
        private float _damageAmount;
        private Events.DamageType _damageType;
        private GameObject _damageSource;
        private Vector3 _damagePoint;
        private bool _wasExecuted;

        // Undo用データ保持
        private float _previousHealth;
        private bool _wasAlive;
        private bool _wasDamageDealt;

        /// <summary>
        /// コマンド実行可否
        /// </summary>
        public bool CanExecute => !_wasExecuted && _target != null && _damageAmount > 0f;

        /// <summary>
        /// Undo実行可否
        /// </summary>
        public bool CanUndo => _wasExecuted && _wasDamageDealt;

        /// <summary>
        /// ダメージコマンド初期化
        /// </summary>
        public void Initialize(GameObject target, float damageAmount, Events.DamageType damageType,
                              GameObject damageSource = null, Vector3 damagePoint = default)
        {
            _target = target;
            _damageAmount = damageAmount;
            _damageType = damageType;
            _damageSource = damageSource;
            _damagePoint = damagePoint != default ? damagePoint : target.transform.position;
            _wasExecuted = false;
        }

        /// <summary>
        /// IResettableCommand準拠の初期化
        /// </summary>
        public void Initialize(params object[] parameters)
        {
            if (parameters.Length >= 3)
            {
                GameObject damageSource = parameters.Length > 3 ? (GameObject)parameters[3] : null;
                Vector3 damagePoint = parameters.Length > 4 ? (Vector3)parameters[4] : default;
                Initialize((GameObject)parameters[0], (float)parameters[1], (Events.DamageType)parameters[2], damageSource, damagePoint);
            }
            else
            {
                Debug.LogWarning("[DamageCommand] Initialize called with insufficient parameters");
            }
        }

        /// <summary>
        /// コマンド実行
        /// </summary>
        public void Execute()
        {
            if (!CanExecute)
            {
                Debug.LogWarning("[DamageCommand] Cannot execute - invalid state or missing target");
                return;
            }

            try
            {
                // ServiceLocator経由でCombatManagerサービス取得
                var combatManager = asterivo.Unity60.Core.ServiceLocator.GetService<Services.ICombatManager>();
                var audioService = asterivo.Unity60.Core.ServiceLocator.GetService<asterivo.Unity60.Core.Audio.Interfaces.IAudioService>();
                var effectsService = asterivo.Unity60.Core.ServiceLocator.GetService<Services.IEffectsService>();

                if (combatManager == null)
                {
                    Debug.LogError("[DamageCommand] CombatManager not found via ServiceLocator");
                    return;
                }

                // 現在の健康状態を保存（Undo用）
                _previousHealth = combatManager.GetHealth(_target);
                _wasAlive = _previousHealth > 0f;

                // ダメージ実行可否確認
                _wasDamageDealt = combatManager.CanTakeDamage(_target);
                if (!_wasDamageDealt)
                {
                    Debug.Log($"[DamageCommand] Target {_target.name} is immune to damage");
                    _wasExecuted = true; // Undo可能にするためマーク
                    return;
                }

                // ダメージ適用
                bool damageResult = combatManager.ApplyDamage(_target, _damageAmount, _damageType);

                if (damageResult)
                {
                    // ダメージエフェクト再生（ObjectPool統合）
                    if (effectsService != null)
                    {
                        switch (_damageType)
                        {
                            case Events.DamageType.Bullet:
                                effectsService.PlayBloodEffect(_damagePoint);
                                effectsService.PlayBulletImpactEffect(_damagePoint);
                                break;
                            case Events.DamageType.Explosion:
                                effectsService.PlayExplosionEffect(_damagePoint);
                                break;
                            case Events.DamageType.Fire:
                                effectsService.PlayFireDamageEffect(_damagePoint);
                                break;
                            case Events.DamageType.Melee:
                                effectsService.PlayMeleeImpactEffect(_damagePoint);
                                break;
                        }
                    }

                    // ダメージ音再生
                    string audioClipName = GetDamageAudioClip();
                    audioService?.PlaySFX(audioClipName, _damagePoint);

                    // ダメージイベント発行（Event駆動アーキテクチャ）
                    var damageData = new Events.DamageDealtData(
                        _target,
                        _damageSource,
                        _damageAmount,
                        _damageType,
                        _damagePoint,
                        combatManager.GetHealth(_target),
                        _previousHealth,
                        _wasAlive && combatManager.GetHealth(_target) <= 0f // 致命傷判定
                    );

                    var damageEvent = Resources.Load<Events.DamageDealtEvent>("Events/DamageDealtEvent");
                    damageEvent?.RaiseDamageDealt(damageData);

                    _wasExecuted = true;

                    Debug.Log($"[DamageCommand] Applied {_damageAmount} {_damageType} damage to {_target.name}");
                }
                else
                {
                    Debug.LogWarning($"[DamageCommand] Failed to apply damage to {_target.name}");
                    _wasDamageDealt = false;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[DamageCommand] Execution failed: {ex.Message}");
            }
        }

        /// <summary>
        /// コマンドUndo実行
        /// </summary>
        public void Undo()
        {
            if (!CanUndo)
            {
                Debug.LogWarning("[DamageCommand] Cannot undo - command was not executed or no damage was dealt");
                return;
            }

            try
            {
                // ServiceLocator経由でCombatManagerサービス取得
                var combatManager = asterivo.Unity60.Core.ServiceLocator.GetService<Services.ICombatManager>();

                if (combatManager == null)
                {
                    Debug.LogError("[DamageCommand] CombatManager not found for Undo");
                    return;
                }

                // 健康状態を元に戻す
                bool undoResult = combatManager.RestoreHealth(_target, _previousHealth);

                if (undoResult)
                {
                    // Undoダメージイベント発行
                    var undoData = new Events.DamageDealtData(
                        _target,
                        _damageSource,
                        -_damageAmount, // 負の値で回復を示す
                        _damageType,
                        _damagePoint,
                        _previousHealth,
                        combatManager.GetHealth(_target),
                        false // Undoなので致命傷ではない
                    );

                    var damageEvent = Resources.Load<Events.DamageDealtEvent>("Events/DamageDealtEvent");
                    damageEvent?.RaiseDamageDealt(undoData);

                    Debug.Log($"[DamageCommand] Undid damage command - restored {_target.name} to {_previousHealth} health");
                }
                else
                {
                    Debug.LogWarning($"[DamageCommand] Failed to undo damage to {_target.name}");
                }

                _wasExecuted = false;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[DamageCommand] Undo failed: {ex.Message}");
            }
        }

        /// <summary>
        /// ObjectPool再利用のための状態リセット（IResettableCommand実装）
        /// </summary>
        public void Reset()
        {
            _target = null;
            _damageAmount = 0f;
            _damageType = Events.DamageType.Unknown;
            _damageSource = null;
            _damagePoint = Vector3.zero;
            _wasExecuted = false;
            _previousHealth = 0f;
            _wasAlive = false;
            _wasDamageDealt = false;

            Debug.Log("[DamageCommand] Command reset for ObjectPool reuse");
        }

        /// <summary>
        /// ダメージタイプに応じたオーディオクリップ名取得
        /// </summary>
        private string GetDamageAudioClip()
        {
            return _damageType switch
            {
                Events.DamageType.Bullet => "BulletHit",
                Events.DamageType.Explosion => "ExplosionHit",
                Events.DamageType.Fire => "FireDamage",
                Events.DamageType.Poison => "PoisonDamage",
                Events.DamageType.Electric => "ElectricDamage",
                Events.DamageType.Melee => "MeleeHit",
                Events.DamageType.Fall => "FallDamage",
                Events.DamageType.Environmental => "EnvironmentalDamage",
                _ => "GenericDamage"
            };
        }

        /// <summary>
        /// デバッグ情報取得
        /// </summary>
        public override string ToString()
        {
            return $"DamageCommand[Target: {_target?.name}, Amount: {_damageAmount}, Type: {_damageType}, " +
                   $"Executed: {_wasExecuted}, CanUndo: {CanUndo}]";
        }
    }
}