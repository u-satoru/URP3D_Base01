using asterivo.Unity60.Core.Components;

namespace asterivo.Unity60.Core.Commands
{
    /// <summary>
    /// Heal command definition for healing operations (ドキュメント第4章:391-394行目の実装)
    /// 体力回復コマンドの定義クラス
    /// 
    /// SerializeReference属性により、ItemData/SkillDataのcommandDefinitionsリストで
    /// ポリモーフィックシリアライゼーションが可能です。
    /// </summary>
    [System.Serializable]
    public class HealCommandDefinition : ICommandDefinition
    {
        [UnityEngine.Tooltip("Amount of health to restore")]
        public int healAmount = 10;

        public bool CanExecute(object context = null)
        {
            // Heal can always be executed if a valid target exists
            return context is IHealthTarget healthTarget && healAmount > 0;
        }

        public ICommand CreateCommand(object context = null)
        {
            if (context is IHealthTarget healthTarget)
            {
                return new HealCommand(healthTarget, healAmount);
            }
            
            UnityEngine.Debug.LogWarning("HealCommandDefinition: Invalid context provided. Expected IHealthTarget.");
            return null;
        }
    }
}
