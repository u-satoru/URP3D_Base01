using asterivo.Unity60.Core.Player;
using asterivo.Unity60.Core.Patterns;

namespace asterivo.Unity60.Core.Patterns.StateHandlers
{
    /// <summary>
    /// アイドル状態のハンドラー
    /// </summary>
    public class IdleStateHandler : IStateHandler
    {
        public PlayerState HandledState => PlayerState.Idle;

        public void OnEnter(IStateContext context)
        {
            context.Log("Entering Idle state");
        }

        public void OnExit(IStateContext context)
        {
            context.Log("Exiting Idle state");
        }
    }
}