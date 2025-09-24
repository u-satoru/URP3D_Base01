using asterivo.Unity60.Core.Commands;

namespace asterivo.Unity60.Features.Player.Commands
{
    /// <summary>
    /// 匍匐コマンド定義 - プレイヤーの匍匐/立ち上がりを制御
    /// </summary>
    [System.Serializable]
    public class ProneCommandDefinition : ICommandDefinition
    {
        [UnityEngine.Tooltip("匍匐状態への遷移か立ち上がりか")]
        public bool toProneState = true; // true: 匍匐, false: 立ち上がり
        
        public bool CanExecute(object context = null)
        {
            // Proneコマンドはプレイヤーが存在すれば実行可能
            return context is States.DetailedPlayerStateMachine;
        }

        public ICommand CreateCommand(object context = null)
        {
            var stateMachine = context as States.DetailedPlayerStateMachine;
            if (stateMachine == null)
                return null;
                
            return new ProneCommand(stateMachine, this);
        }
    }
}