using UnityEngine;
// using asterivo.Unity60.Core.Components;

namespace asterivo.Unity60.Core.Commands
{
    /// <summary>
    /// Command for dealing damage to health targets (繝峨く繝･繝｡繝ｳ繝育ｬｬ4遶:419-432陦檎岼縺ｮ螳溯｣・
    /// 繝繝｡繝ｼ繧ｸ繧剃ｸ弱∴繧九さ繝槭Φ繝峨け繝ｩ繧ｹ
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
            // 繝励・繝ｫ蛹門ｯｾ蠢懶ｼ壹ヱ繝ｩ繝｡繝ｼ繧ｿ繝ｼ縺ｪ縺励さ繝ｳ繧ｹ繝医Λ繧ｯ繧ｿ
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
                UnityEngine.Debug.LogError("DamageCommand.Initialize: 譛菴・縺､縺ｮ繝代Λ繝｡繝ｼ繧ｿ・・arget, damageAmount・峨′蠢・ｦ√〒縺吶・);
#endif
                return;
            }

            _target = parameters[0] as IHealthTarget;
            if (_target == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.LogError("DamageCommand.Initialize: 譛蛻昴・繝代Λ繝｡繝ｼ繧ｿ縺ｯIHealthTarget縺ｧ縺ゅｋ蠢・ｦ√′縺ゅｊ縺ｾ縺吶・);
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
                UnityEngine.Debug.LogError("DamageCommand.Initialize: 2逡ｪ逶ｮ縺ｮ繝代Λ繝｡繝ｼ繧ｿ縺ｯint・医ム繝｡繝ｼ繧ｸ驥擾ｼ峨〒縺ゅｋ蠢・ｦ√′縺ゅｊ縺ｾ縺吶・);
#endif
                return;
            }

            _elementType = parameters.Length > 2 && parameters[2] is string elementType
                ? elementType
                : "physical";
        }
        
        /// <summary>
        /// 繧医ｊ蝙句ｮ牙・縺ｪ蛻晄悄蛹悶Γ繧ｽ繝・ラ
        /// </summary>
        public void Initialize(IHealthTarget target, int damageAmount, string elementType = "physical")
        {
            _target = target;
            _damageAmount = damageAmount;
            _elementType = elementType;
        }
    }
}
