using UnityEngine;
using asterivo.Unity60.Core.Components;

namespace asterivo.Unity60.Core.Commands
{
    public class HealCommand : ICommand
    {
        private readonly IHealthTarget _target;
        private readonly int _healAmount;

        // The existing CommandInvoker expects this property.
        public bool CanUndo => true; 

        public HealCommand(IHealthTarget target, int healAmount)
        {
            _target = target;
            _healAmount = healAmount;
        }

        public void Execute() => _target.Heal(_healAmount);
        public void Undo() => _target.TakeDamage(_healAmount);
    }
}