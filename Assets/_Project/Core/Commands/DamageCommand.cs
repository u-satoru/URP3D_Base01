using UnityEngine;
using asterivo.Unity60.Core.Components;

namespace asterivo.Unity60.Core.Commands
{
    /// <summary>
    /// Command for dealing damage to health targets (ドキュメント第4章:419-432行目の実装)
    /// ダメージを与えるコマンドクラス
    /// </summary>
    public class DamageCommand : ICommand
    {
        private readonly IHealthTarget _target;
        private readonly int _damageAmount;
        private readonly string _elementType;

        public bool CanUndo => true;

        public DamageCommand(IHealthTarget target, int damageAmount, string elementType = "physical")
        {
            _target = target;
            _damageAmount = damageAmount;
            _elementType = elementType;
        }

        public void Execute()
        {
            _target.TakeDamage(_damageAmount);
            Debug.Log($"Dealt {_damageAmount} {_elementType} damage");
        }

        public void Undo()
        {
            // Undo damage by healing the same amount
            _target.Heal(_damageAmount);
            Debug.Log($"Undid {_damageAmount} {_elementType} damage (healed)");
        }
    }
}