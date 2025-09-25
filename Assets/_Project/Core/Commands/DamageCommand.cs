using UnityEngine;
using asterivo.Unity60.Core.Components;

namespace asterivo.Unity60.Core.Commands
{
    /// <summary>
    /// Command for dealing damage to health targets (ドキュメント第4章:419-432行目の実装)
    /// ダメージを与えるコマンドクラス
    /// </summary>
    public class DamageCommand : IResettableCommand
    {
        private IHealthTarget _target;
        private int _damageAmount;
        private string _elementType;
        private GameObject _damageSource;
        private string _hitType;

        // Properties for object initializer syntax (FPS Template compatibility)
        public IHealthTarget TargetHealth
        {
            get => _target;
            set => _target = value;
        }

        public int DamageAmount
        {
            get => _damageAmount;
            set => _damageAmount = value;
        }

        public GameObject DamageSource
        {
            get => _damageSource;
            set => _damageSource = value;
        }

        public string HitType
        {
            get => _hitType;
            set => _hitType = value;
        }

        public string ElementType
        {
            get => _elementType;
            set => _elementType = value;
        }

        public bool CanUndo => true;

        public DamageCommand()
        {
            // プール化対応：パラメーターなしコンストラクタ
        }

        public DamageCommand(IHealthTarget target, int damageAmount, string elementType = "physical")
        {
            _target = target;
            _damageAmount = damageAmount;
            _elementType = elementType;
        }

        public void Execute()
        {
            if (_target == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.LogWarning("DamageCommand: Target is null, cannot execute damage");
#endif
                return;
            }
            
            _target.TakeDamage(_damageAmount, _elementType);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Dealt {_damageAmount} {_elementType} damage");
#endif
        }

        public void Undo()
        {
            // Undo damage by healing the same amount
            _target.Heal(_damageAmount);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Undid {_damageAmount} {_elementType} damage (healed)");
#endif
        }

        public void Reset()
        {
            _target = null;
            _damageAmount = 0;
            _elementType = null;
            _damageSource = null;
            _hitType = null;
        }

        public void Initialize(params object[] parameters)
        {
            if (parameters.Length < 2)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.LogError("DamageCommand.Initialize: 最低2つのパラメータ（target, damageAmount）が必要です。");
#endif
                return;
            }

            _target = parameters[0] as IHealthTarget;
            if (_target == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.LogError("DamageCommand.Initialize: 最初のパラメータはIHealthTargetである必要があります。");
#endif
                return;
            }

            if (parameters[1] is int damage)
            {
                _damageAmount = damage;
            }
            else
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.LogError("DamageCommand.Initialize: 2番目のパラメータはint（ダメージ量）である必要があります。");
#endif
                return;
            }

            _elementType = parameters.Length > 2 && parameters[2] is string elementType
                ? elementType
                : "physical";
        }
        
        /// <summary>
        /// より型安全な初期化メソッド
        /// </summary>
        public void Initialize(IHealthTarget target, int damageAmount, string elementType = "physical")
        {
            _target = target;
            _damageAmount = damageAmount;
            _elementType = elementType;
        }
    }
}