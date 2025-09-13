using asterivo.Unity60.Core.Commands;

namespace asterivo.Unity60.Player.Commands
{
    /// <summary>
    /// カバー解除コマンド定義 - カバー状態からの離脱を制御
    /// </summary>
    [System.Serializable]
    public class ExitCoverCommandDefinition : ICommandDefinition
    {
        [UnityEngine.Tooltip("カバー解除後の移動方向")]
        public ExitDirection exitDirection = ExitDirection.Backward;
        
        [UnityEngine.Tooltip("カバー解除の速度倍率")]
        [UnityEngine.Range(0.5f, 2f)]
        public float exitSpeedMultiplier = 1f;
        
        public bool CanExecute(object context = null)
        {
            // ExitCoverコマンドはプレイヤーがカバー状態の時のみ実行可能
            var stateMachine = context as States.DetailedPlayerStateMachine;
            return stateMachine != null && stateMachine.GetCurrentStateType() == States.PlayerStateType.InCover;
        }

        public ICommand CreateCommand(object context = null)
        {
            var stateMachine = context as States.DetailedPlayerStateMachine;
            if (stateMachine == null || !CanExecute(context))
                return null;
                
            return new ExitCoverCommand(stateMachine, this);
        }
    }
    
    /// <summary>
    /// カバー解除方向の列挙型
    /// </summary>
    public enum ExitDirection
    {
        Backward,   // 後退
        Left,       // 左へ
        Right,      // 右へ
        Roll        // ローリング
    }
}