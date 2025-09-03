using asterivo.Unity60.Core.Commands;

namespace asterivo.Unity60.Player.Commands
{
    /// <summary>
    /// 覗き見コマンド定義 - カバー状態での覗き見動作を制御
    /// </summary>
    [System.Serializable]
    public class PeekCommandDefinition : ICommandDefinition
    {
        [UnityEngine.Tooltip("覗き見の方向")]
        public PeekDirection peekDirection = PeekDirection.Left;
        
        [UnityEngine.Tooltip("覗き見の強度（0.0-1.0）")]
        [UnityEngine.Range(0f, 1f)]
        public float peekIntensity = 0.5f;
        
        public bool CanExecute(object context = null)
        {
            // Peekコマンドはプレイヤーがカバー状態の時のみ実行可能
            var stateMachine = context as States.DetailedPlayerStateMachine;
            return stateMachine != null && stateMachine.GetCurrentStateType() == States.PlayerStateType.InCover;
        }

        public ICommand CreateCommand(object context = null)
        {
            var stateMachine = context as States.DetailedPlayerStateMachine;
            if (stateMachine == null || !CanExecute(context))
                return null;
                
            return new PeekCommand(stateMachine, this);
        }
    }
    
    /// <summary>
    /// 覗き見方向の列挙型
    /// </summary>
    public enum PeekDirection
    {
        Left,
        Right,
        Up,
        Down
    }
}