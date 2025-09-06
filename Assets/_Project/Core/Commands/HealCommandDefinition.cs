using asterivo.Unity60.Core.Components;
using Sirenix.OdinInspector;

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
        [BoxGroup("Healing Parameters")]
        [PropertyRange(1, 500)]
        [LabelText("Heal Amount")]
        [UnityEngine.Tooltip("Amount of health to restore")]
        [SuffixLabel("HP", overlay: true)]
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
                // プール化対応: CommandPoolから取得して初期化
                var command = CommandPool.Instance != null 
                    ? CommandPool.Instance.GetCommand<HealCommand>()
                    : new HealCommand();
                    
                command.Initialize(healthTarget, healAmount);
                return command;
            }
            
            UnityEngine.Debug.LogWarning("HealCommandDefinition: Invalid context provided. Expected IHealthTarget.");
            return null;
        }
    }
}
