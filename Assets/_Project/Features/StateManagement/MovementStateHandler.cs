using asterivo.Unity60.Core.Player;
using asterivo.Unity60.Core.Patterns;

namespace asterivo.Unity60.Core.Patterns.StateHandlers
{
    /// <summary>
    /// 移動系状態の基底ハンドラー
    /// </summary>
    public abstract class MovementStateHandler : IStateHandler
    {
        public abstract PlayerState HandledState { get; }
        
        protected abstract string MovementType { get; }

        public virtual void OnEnter(IStateContext context)
        {
            context.Log($"Entering {MovementType} state");
        }

        public virtual void OnExit(IStateContext context)
        {
            context.Log($"Exiting {MovementType} state");
        }
    }
    
    /// <summary>
    /// 歩行状態のハンドラー
    /// </summary>
    public class WalkingStateHandler : MovementStateHandler
    {
        public override PlayerState HandledState => PlayerState.Walking;
        protected override string MovementType => "Walking";
    }
    
    /// <summary>
    /// 走行状態のハンドラー
    /// </summary>
    public class RunningStateHandler : MovementStateHandler
    {
        public override PlayerState HandledState => PlayerState.Running;
        protected override string MovementType => "Running";
    }
    
    /// <summary>
    /// 疾走状態のハンドラー
    /// </summary>
    public class SprintingStateHandler : MovementStateHandler
    {
        public override PlayerState HandledState => PlayerState.Sprinting;
        protected override string MovementType => "Sprinting";
    }
    
    /// <summary>
    /// ジャンプ状態のハンドラー
    /// </summary>
    public class JumpingStateHandler : MovementStateHandler
    {
        public override PlayerState HandledState => PlayerState.Jumping;
        protected override string MovementType => "Jumping";
    }
    
    /// <summary>
    /// 落下状態のハンドラー
    /// </summary>
    public class FallingStateHandler : MovementStateHandler
    {
        public override PlayerState HandledState => PlayerState.Falling;
        protected override string MovementType => "Falling";
    }
    
    /// <summary>
    /// 着地状態のハンドラー
    /// </summary>
    public class LandingStateHandler : MovementStateHandler
    {
        public override PlayerState HandledState => PlayerState.Landing;
        protected override string MovementType => "Landing";
    }
}