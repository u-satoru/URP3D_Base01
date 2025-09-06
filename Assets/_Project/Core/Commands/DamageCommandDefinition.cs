using asterivo.Unity60.Core.Components;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Core.Commands
{
    /// <summary>
    /// Command definition for damage operations (ドキュメント第4章:413-417行目の実装)
    /// ダメージを与えるコマンドの定義クラス
    /// </summary>
    [System.Serializable]
    public class DamageCommandDefinition : ICommandDefinition
    {
        [BoxGroup("Damage Parameters")]
        [PropertyRange(1, 1000)]
        [LabelText("Damage Amount")]
        [UnityEngine.Tooltip("Amount of damage to deal")]
        public int damageAmount = 10;
        
        [BoxGroup("Damage Parameters")]
        [ValueDropdown("GetElementTypes")]
        [LabelText("Element Type")]
        [UnityEngine.Tooltip("Type of damage element")]
        public string elementType = "physical";
        
        private static string[] GetElementTypes()
        {
            return new string[] { "physical", "fire", "ice", "lightning", "poison", "holy", "dark" };
        }

        public bool CanExecute(object context = null)
        {
            // Damage can always be executed if a valid target exists
            return context is IHealthTarget healthTarget && damageAmount > 0;
        }

        public ICommand CreateCommand(object context = null)
        {
            if (context is IHealthTarget healthTarget)
            {
                // プール化対応: CommandPoolから取得して初期化
                var command = CommandPool.Instance != null 
                    ? CommandPool.Instance.GetCommand<DamageCommand>()
                    : new DamageCommand();
                    
                command.Initialize(healthTarget, damageAmount, elementType);
                return command;
            }
            
            UnityEngine.Debug.LogWarning("DamageCommandDefinition: Invalid context provided. Expected IHealthTarget.");
            return null;
        }
    }
}