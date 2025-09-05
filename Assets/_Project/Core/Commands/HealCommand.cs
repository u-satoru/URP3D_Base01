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

        public void Execute() => _target.Heal(_healAmount);
        

        public void Reset()
        {
            _target = null;
            _healAmount = 0;
        }

        public void Initialize(params object[] parameters)
        {
            if (parameters.Length < 2)
            {
                UnityEngine.Debug.LogError("HealCommand.Initialize: 最低2つのパラメーター（target, healAmount）が必要です。");
                return;
            }

            _target = parameters[0] as IHealthTarget;
            if (_target == null)
            {
                UnityEngine.Debug.LogError("HealCommand.Initialize: 最初のパラメーターはIHealthTargetである必要があります。");
                return;
            }

            if (parameters[1] is int healAmount)
            {
                _healAmount = healAmount;
            }
            else
            {
                UnityEngine.Debug.LogError("HealCommand.Initialize: 2番目のパラメーターはint（回復量）である必要があります。");
                return;
            }
        }
public void Undo() => _target.TakeDamage(_healAmount);
    }
}