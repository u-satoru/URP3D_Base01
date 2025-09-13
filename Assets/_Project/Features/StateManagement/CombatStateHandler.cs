using asterivo.Unity60.Core.Player;
using asterivo.Unity60.Core.Patterns;

namespace asterivo.Unity60.Core.Patterns.StateHandlers
{
    /// <summary>
    /// 戦闘状態のハンドラー
    /// </summary>
    public class CombatStateHandler : IStateHandler
    {
        public PlayerState HandledState => PlayerState.Combat;

        public void OnEnter(IStateContext context)
        {
            context.Log("Entering Combat state");
        }

        public void OnExit(IStateContext context)
        {
            context.Log("Exiting Combat state");
        }
    }
    
    /// <summary>
    /// 戦闘攻撃状態のハンドラー
    /// </summary>
    public class CombatAttackingStateHandler : IStateHandler
    {
        public PlayerState HandledState => PlayerState.CombatAttacking;

        public void OnEnter(IStateContext context)
        {
            context.Log("Entering Combat Attacking state");
        }

        public void OnExit(IStateContext context)
        {
            context.Log("Exiting Combat Attacking state");
        }
    }
    
    /// <summary>
    /// インタラクション状態のハンドラー
    /// </summary>
    public class InteractingStateHandler : IStateHandler
    {
        public PlayerState HandledState => PlayerState.Interacting;

        public void OnEnter(IStateContext context)
        {
            context.Log("Entering Interacting state");
        }

        public void OnExit(IStateContext context)
        {
            context.Log("Exiting Interacting state");
        }
    }
    
    /// <summary>
    /// 死亡状態のハンドラー
    /// </summary>
    public class DeadStateHandler : IStateHandler
    {
        public PlayerState HandledState => PlayerState.Dead;

        public void OnEnter(IStateContext context)
        {
            context.Log("Entering Dead state");
        }

        public void OnExit(IStateContext context)
        {
            context.Log("Exiting Dead state");
        }
    }
}