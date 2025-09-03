using asterivo.Unity60.Core.Commands;

namespace asterivo.Unity60.Player.Commands
{
    /// <summary>
    /// しゃがみコマンド定義 - プレイヤーのしゃがみ/立ち上がりを制御
    /// </summary>
    [System.Serializable]
    public class CrouchCommandDefinition : ICommandDefinition
    {
        [UnityEngine.Tooltip("しゃがみ状態への遷移か立ち上がりか")]
        public bool toCrouchState = true; // true: しゃがみ, false: 立ち上がり
        
        public bool CanExecute(object context = null)
        {
            // Crouchコマンドはプレイヤーが存在すれば実行可能
            return context is States.DetailedPlayerStateMachine;
        }

        public ICommand CreateCommand(object context = null)
        {
            var stateMachine = context as States.DetailedPlayerStateMachine;
            if (stateMachine == null)
                return null;
                
            return new CrouchCommand(stateMachine, this);
        }
    }
}