using asterivo.Unity60.Core.Components;

namespace asterivo.Unity60.Core.Commands
{
    /// <summary>
    /// Command definition for damage operations (ドキュメント第4章:413-417行目の実装)
    /// ダメージを与えるコマンドの定義クラス
    /// </summary>
    [System.Serializable]
    public class DamageCommandDefinition : ICommandDefinition
    {
        [UnityEngine.Tooltip("Amount of damage to deal")]
        public int damageAmount = 10;
        
        [UnityEngine.Tooltip("Type of damage element (fire, ice, etc.)")]
        public string elementType = "physical";

        public bool CanExecute(object context = null)
        {
            // Damage can always be executed if a valid target exists
            return context is IHealthTarget healthTarget && damageAmount > 0;
        }

        public ICommand CreateCommand(object context = null)
        {
            if (context is IHealthTarget healthTarget)
            {
                return new DamageCommand(healthTarget, damageAmount, elementType);
            }
            
            UnityEngine.Debug.LogWarning("DamageCommandDefinition: Invalid context provided. Expected IHealthTarget.");
            return null;
        }
    }
}