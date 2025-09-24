using UnityEngine;
using asterivo.Unity60.Core.Patterns.StateMachine;
using asterivo.Unity60.Core.StateMachine;
using asterivo.Unity60.Core.Data;

namespace asterivo.Unity60.Features.AI.StateMachine.Hierarchical
{
    /// <summary>
    /// AI警戒状態 - 階層化ステートマシンの実装例
    /// Combat（戦闘）とSearching（捜索）を子状態として持つ
    /// </summary>
    public class AIAlertState : HierarchicalState<AIContext>
    {
        private const float COMBAT_DISTANCE_THRESHOLD = 8f;
        private const float SEARCH_TIMEOUT = 10f;
        private const float TARGET_LOST_TIMEOUT = 3f;

        protected override void InitializeChildStates()
        {
            // 子状態の定義
            var combatState = new AICombatChildState();
            var searchingState = new AISearchingChildState();

            AddChildState("Combat", combatState);
            AddChildState("Searching", searchingState);
        }

        protected override string GetDefaultChildStateKey()
        {
            return "Searching";
        }

        protected override void OnParentEnter(AIContext context)
        {
            // アラートレベルを設定
            context.SetAlertLevel(AlertLevel.Alert);

            // ナビゲーションを停止（子状態で制御）
            context.SafeStopAgent();

            Debug.Log($"[AIAlertState] Entered Alert state. Target: {context.CurrentTarget?.name ?? "None"}");
        }

        protected override void OnParentUpdate(AIContext context)
        {
            // 目標を失った場合の処理
            if (context.CurrentTarget == null && context.StateTimer > TARGET_LOST_TIMEOUT)
            {
                // 最後に見た位置に向かう
                if (context.LastKnownPosition != Vector3.zero)
                {
                    context.SetDestination(context.LastKnownPosition);
                    TransitionToChild("Searching", context);
                }
            }

            // 疑惑レベルの自然減少
            if (context.CurrentTarget == null)
            {
                context.DecreaseSuspicion(0.3f);

                // 疑惑レベルが低下したら警戒状態を解除
                if (context.SuspicionLevel < 0.2f && context.StateTimer > SEARCH_TIMEOUT)
                {
                    // 親状態（上位のステートマシン）に状態変更を要求
                    // これは通常、AIStateMachineで処理される
                    Debug.Log("[AIAlertState] Suspicion level low, requesting return to patrol");
                }
            }
        }

        protected override void OnParentExit(AIContext context)
        {
            // エージェントを再開
            context.SafeStartAgent();

            Debug.Log("[AIAlertState] Exited Alert state");
        }

        // AI固有のイベントハンドリング
        public void OnSightTarget(AIContext context, Transform target)
        {
            context.SetTarget(target);

            // 目標との距離に応じて子状態を決定
            float distance = Vector3.Distance(context.Transform.position, target.position);
            if (distance <= COMBAT_DISTANCE_THRESHOLD)
            {
                TransitionToChild("Combat", context);
            }
            else
            {
                TransitionToChild("Searching", context);
            }
        }

        public void OnHearNoise(AIContext context, Vector3 noisePosition, float noiseLevel)
        {
            if (noiseLevel > 0.5f)
            {
                context.LastKnownPosition = noisePosition;
                context.IncreaseSuspicion(noiseLevel);

                // 大きな音なら捜索状態に
                if (GetCurrentChildStateKey() != "Combat")
                {
                    TransitionToChild("Searching", context);
                }
            }
        }
    }

    /// <summary>
    /// 戦闘子状態
    /// </summary>
    public class AICombatChildState : IState<AIContext>
    {
        public string StateName => "Combat";
        public bool IsActive { get; private set; }

        public void Enter(AIContext context)
        {
            IsActive = true;
            context.SafeStopAgent();

            Debug.Log("[AICombatChildState] Entering combat mode");
        }

        public void Exit(AIContext context)
        {
            IsActive = false;
            Debug.Log("[AICombatChildState] Exiting combat mode");
        }

        public void Update(AIContext context)
        {
            if (context.CurrentTarget == null) return;

            // 目標に向けて回転
            Vector3 directionToTarget = (context.CurrentTarget.position - context.Transform.position).normalized;
            context.Transform.rotation = Quaternion.Slerp(
                context.Transform.rotation,
                Quaternion.LookRotation(directionToTarget),
                Time.deltaTime * 5f
            );

            // 戦闘ロジック（攻撃、移動など）
            // TODO: 武器システムとの統合
            // TODO: アニメーションシステムとの統合
        }

        public void FixedUpdate(AIContext context)
        {
            // Physics-related combat logic can be added here if needed
        }

        public void HandleInput(AIContext context, object inputData)
        {
            // Combat-specific input handling can be added here if needed
        }
    }

    /// <summary>
    /// 捜索子状態
    /// </summary>
    public class AISearchingChildState : IState<AIContext>
    {
        public string StateName => "Searching";
        public bool IsActive { get; private set; }

        private Vector3 searchTarget;
        private float searchTimer;
        private const float SEARCH_RADIUS = 5f;

        public void Enter(AIContext context)
        {
            IsActive = true;
            searchTimer = 0f;

            // 捜索目標を設定
            searchTarget = context.LastKnownPosition != Vector3.zero
                ? context.LastKnownPosition
                : context.Transform.position;

            context.SetDestination(searchTarget);
            context.SafeStartAgent();

            Debug.Log("[AISearchingChildState] Starting search at " + searchTarget);
        }

        public void Exit(AIContext context)
        {
            IsActive = false;
            Debug.Log("[AISearchingChildState] Ending search");
        }

        public void Update(AIContext context)
        {
            searchTimer += Time.deltaTime;

            // 目標地点に到達した場合
            if (context.HasReachedDestination())
            {
                // 周辺を捜索するための新しい地点を設定
                SetRandomSearchPoint(context);
            }

            // 目標を再発見した場合
            if (context.CurrentTarget != null)
            {
                context.SetDestination(context.CurrentTarget.position);
            }
        }

        public void FixedUpdate(AIContext context)
        {
            // Physics-related search logic can be added here if needed
        }

        public void HandleInput(AIContext context, object inputData)
        {
            // Search-specific input handling can be added here if needed
        }

        private void SetRandomSearchPoint(AIContext context)
        {
            // ランダムな方向に捜索地点を設定
            Vector2 randomCircle = Random.insideUnitCircle * SEARCH_RADIUS;
            Vector3 randomPoint = searchTarget + new Vector3(randomCircle.x, 0, randomCircle.y);

            // NavMeshで有効な地点を見つける
            if (UnityEngine.AI.NavMesh.SamplePosition(randomPoint, out UnityEngine.AI.NavMeshHit hit, SEARCH_RADIUS, UnityEngine.AI.NavMesh.AllAreas))
            {
                context.SetDestination(hit.position);
            }
        }
    }
}
