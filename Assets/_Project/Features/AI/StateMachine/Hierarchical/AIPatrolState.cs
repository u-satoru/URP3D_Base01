using UnityEngine;
using asterivo.Unity60.Core.Patterns.StateMachine;
using asterivo.Unity60.Core.StateMachine;
using asterivo.Unity60.Core.Data;

namespace asterivo.Unity60.Features.AI.StateMachine.Hierarchical
{
    /// <summary>
    /// AI巡回状態 - 階層化ステートマシンの実装例
    /// Walking（移動）とWaiting（待機）を子状態として持つ
    /// </summary>
    public class AIPatrolState : HierarchicalState<AIContext>
    {
        private const float PATROL_POINT_THRESHOLD = 1f;
        private const float SUSPICION_THRESHOLD = 0.3f;

        protected override void InitializeChildStates()
        {
            // 子状態の定義
            var walkingState = new AIWalkingChildState();
            var waitingState = new AIWaitingChildState();

            AddChildState("Walking", walkingState);
            AddChildState("Waiting", waitingState);
        }

        protected override string GetDefaultChildStateKey()
        {
            return "Walking";
        }

        protected override void OnParentEnter(AIContext context)
        {
            // アラートレベルを設定
            context.SetAlertLevel(AlertLevel.Relaxed);

            // 最初の巡回地点に向かう
            if (context.PatrolPoints != null && context.PatrolPoints.Length > 0)
            {
                context.SetDestination(context.GetCurrentPatrolPoint());
                TransitionToChild("Walking", context);
            }
            else
            {
                // 巡回地点がない場合は待機
                TransitionToChild("Waiting", context);
            }

            Debug.Log($"[AIPatrolState] Started patrolling. Current patrol index: {context.CurrentPatrolIndex}");
        }

        protected override void OnParentUpdate(AIContext context)
        {
            // 疑惑レベルの自然減少
            context.DecreaseSuspicion(0.1f);

            // 疑惑レベルが閾値を超えた場合は巡回を中断
            if (context.SuspicionLevel >= SUSPICION_THRESHOLD)
            {
                Debug.Log("[AIPatrolState] Suspicion level exceeded, requesting state change");
                // 上位ステートマシンに状態変更を要求
                // 実際の実装では外部から処理される
            }
        }

        protected override void OnParentExit(AIContext context)
        {
            Debug.Log("[AIPatrolState] Exited patrol state");
        }

        // AI固有のイベントハンドリング
        public void OnSightTarget(AIContext context, Transform target)
        {
            context.SetTarget(target);
            context.IncreaseSuspicion(1f);

            Debug.Log($"[AIPatrolState] Target sighted: {target.name}");
        }

        public void OnHearNoise(AIContext context, Vector3 noisePosition, float noiseLevel)
        {
            if (noiseLevel > 0.3f)
            {
                context.LastKnownPosition = noisePosition;
                context.IncreaseSuspicion(noiseLevel);

                // 音の方向を向く
                Vector3 directionToNoise = (noisePosition - context.Transform.position).normalized;
                context.Transform.rotation = Quaternion.LookRotation(directionToNoise);

                Debug.Log($"[AIPatrolState] Heard noise at {noisePosition}, level: {noiseLevel}");
            }
        }
    }

    /// <summary>
    /// 移動子状態
    /// </summary>
    public class AIWalkingChildState : IState<AIContext>
    {
        public string StateName => "Walking";
        public bool IsActive { get; private set; }

        public void Enter(AIContext context)
        {
            IsActive = true;
            context.SafeStartAgent();

            // 現在の巡回地点に向かう
            Vector3 targetPoint = context.GetCurrentPatrolPoint();
            context.SetDestination(targetPoint);

            Debug.Log($"[AIWalkingChildState] Moving to patrol point {context.CurrentPatrolIndex}: {targetPoint}");
        }

        public void Exit(AIContext context)
        {
            IsActive = false;
            context.SafeStopAgent();

            Debug.Log("[AIWalkingChildState] Stopped walking");
        }

        public void Update(AIContext context)
        {
            // 移動先に到達しているかチェック
            if (context.HasReachedDestination())
            {
                Debug.Log($"[AIWalkingChildState] Reached patrol point {context.CurrentPatrolIndex}");
                // 遷移条件は親状態で処理される
            }

            // TODO: アニメーションシステムとの統合
            // TODO: 移動音の生成
        }

        public void FixedUpdate(AIContext context)
        {
            // Physics-related movement logic can be added here if needed
        }

        public void HandleInput(AIContext context, object inputData)
        {
            // Walking-specific input handling can be added here if needed
        }
    }

    /// <summary>
    /// 待機子状態
    /// </summary>
    public class AIWaitingChildState : IState<AIContext>
    {
        public string StateName => "Waiting";
        public bool IsActive { get; private set; }

        private float waitTimer;

        public void Enter(AIContext context)
        {
            IsActive = true;
            waitTimer = 0f;
            context.ResetStateTimer();

            Debug.Log($"[AIWaitingChildState] Waiting at patrol point {context.CurrentPatrolIndex} for {context.PatrolWaitTime} seconds");
        }

        public void Exit(AIContext context)
        {
            IsActive = false;
            Debug.Log("[AIWaitingChildState] Finished waiting");
        }

        public void Update(AIContext context)
        {
            waitTimer += Time.deltaTime;

            // 周囲を見回す動作
            PerformLookAround(context);

            // 待機時間が経過したかチェック
            if (waitTimer >= context.PatrolWaitTime)
            {
                // 次の巡回地点に移動
                context.NextPatrolPoint();
                Debug.Log($"[AIWaitingChildState] Wait complete, moving to next patrol point: {context.CurrentPatrolIndex}");
                // 遷移条件は親状態で処理される
            }
        }

        public void FixedUpdate(AIContext context)
        {
            // Physics-related waiting logic can be added here if needed
        }

        public void HandleInput(AIContext context, object inputData)
        {
            // Waiting-specific input handling can be added here if needed
        }

        private void PerformLookAround(AIContext context)
        {
            // 簡単な見回し動作（周期的な回転）
            float lookAroundSpeed = 30f; // degrees per second
            float lookAroundAngle = Mathf.Sin(waitTimer * 2f) * 45f; // -45°から+45°まで

            Vector3 baseForward = context.Transform.forward;
            Quaternion lookRotation = Quaternion.AngleAxis(lookAroundAngle, Vector3.up) * Quaternion.LookRotation(baseForward);

            context.Transform.rotation = Quaternion.Slerp(
                context.Transform.rotation,
                lookRotation,
                Time.deltaTime * lookAroundSpeed / 45f
            );
        }
    }
}