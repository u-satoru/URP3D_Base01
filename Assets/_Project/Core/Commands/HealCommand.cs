using UnityEngine;
using asterivo.Unity60.Core.Components;

namespace asterivo.Unity60.Core.Commands
{
    public class HealCommand : IResettableCommand
    {
        private IHealthTarget _target;
        private int _healAmount;

        // The existing CommandInvoker expects this property.
        public bool CanUndo => true; 

        public HealCommand()
        {
            // プール化対応：パラメーターなしコンストラクタ
        }

        public HealCommand(IHealthTarget target, int healAmount)
        {
            _target = target;
            _healAmount = healAmount;
        }

        public void Execute()
        {
            if (_target == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.LogWarning("HealCommand: Target is null, cannot execute heal");
#endif
                return;
            }
            
            _target.Heal(_healAmount);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Healed {_healAmount} health");
#endif
        }
        

        public void Reset()
        {
            _target = null;
            _healAmount = 0;
        }

        public void Initialize(params object[] parameters)
        {
            if (parameters.Length < 2)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.LogError("HealCommand.Initialize: 最低2つのパラメーター（target, healAmount）が必要です。");
#endif
                return;
            }

            _target = parameters[0] as IHealthTarget;
            if (_target == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.LogError("HealCommand.Initialize: 最初のパラメーターはIHealthTargetである必要があります。");
#endif
                return;
            }

            if (parameters[1] is int healAmount)
            {
                _healAmount = healAmount;
            }
            else
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.LogError("HealCommand.Initialize: 2番目のパラメーターはint（回復量）である必要があります。");
#endif
                return;
            }
        }
        
        /// <summary>
        /// より型安全な初期化メソッド
        /// </summary>
        public void Initialize(IHealthTarget target, int healAmount)
        {
            _target = target;
            _healAmount = healAmount;
        }
        
        public void Undo()
        {
            if (_target == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.LogWarning("HealCommand: Target is null, cannot undo heal");
#endif
                return;
            }
            
            _target.TakeDamage(_healAmount, "healing_undo");
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Undid {_healAmount} healing (dealt damage)");
#endif
        }
    }
}