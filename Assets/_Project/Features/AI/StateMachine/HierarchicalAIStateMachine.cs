using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Data;
using asterivo.Unity60.Core.Patterns.StateMachine;
using asterivo.Unity60.Core.StateMachine;
using asterivo.Unity60.Features.AI.StateMachine.Hierarchical;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Features.AI.StateMachine
{
    /// <summary>
    /// 階層化ステートマシンを使用するAIコントローラー
    /// 従来のAIStateMachineの階層化版
    /// </summary>
    public class HierarchicalAIStateMachine : MonoBehaviour
    {
        [TabGroup("AI Control", "State Management")]
        [ReadOnly]
        [LabelText("Current State")]
        [SerializeField] private string currentStateName;

        [TabGroup("AI Control", "State Management")]
        [ReadOnly]
        [LabelText("Current Child State")]
        [SerializeField] private string currentChildStateName;

        [TabGroup("AI Control", "State Management")]
        [ReadOnly]
        [LabelText("Alert Level")]
        [SerializeField] private AlertLevel currentAlertLevel = AlertLevel.Relaxed;

        [TabGroup("AI Control", "Components")]
        [Required]
        [LabelText("Navigation Agent")]
        [SerializeField] private NavMeshAgent navAgent;

        [TabGroup("AI Control", "Components")]
        [LabelText("Eye Position")]
        [SerializeField] private Transform eyePosition;

        [TabGroup("AI Control", "Senses")]
        [PropertyRange(5f, 50f)]
        [LabelText("Sight Range")]
        [SuffixLabel("m", overlay: true)]
        [SerializeField] private float sightRange = 15f;

        [TabGroup("AI Control", "Senses")]
        [PropertyRange(30f, 180f)]
        [LabelText("Sight Angle")]
        [SuffixLabel("°", overlay: true)]
        [SerializeField] private float sightAngle = 110f;

        [TabGroup("AI Control", "Senses")]
        [PropertyRange(3f, 25f)]
        [LabelText("Hearing Range")]
        [SuffixLabel("m", overlay: true)]
        [SerializeField] private float hearingRange = 10f;

        [TabGroup("AI Control", "Detection")]
        [ReadOnly]
        [LabelText("Current Target")]
        [SerializeField] private Transform currentTarget;

        [TabGroup("AI Control", "Detection")]
        [ReadOnly]
        [LabelText("Last Known Position")]
        [SerializeField] private Vector3 lastKnownPosition;

        [TabGroup("AI Control", "Detection")]
        [ProgressBar(0f, 1f, ColorGetter = "GetSuspicionColor")]
        [LabelText("Suspicion Level")]
        [SerializeField] private float suspicionLevel = 0f;

        [TabGroup("AI Control", "Detection")]
        [PropertyRange(0.5f, 3f)]
        [LabelText("Detection Speed")]
        [SerializeField] private float detectionSpeed = 1f;

        [TabGroup("AI Control", "Patrol")]
        [ListDrawerSettings(ShowIndexLabels = true, ListElementLabelName = "name")]
        [LabelText("Patrol Points")]
        [SerializeField] private Transform[] patrolPoints;

        [TabGroup("AI Control", "Patrol")]
        [PropertyRange(0.5f, 10f)]
        [LabelText("Wait Time at Points")]
        [SuffixLabel("s", overlay: true)]
        [SerializeField] private float patrolWaitTime = 2f;

        [TabGroup("AI Control", "Events")]
        [LabelText("Alert Level Changed")]
        [SerializeField] private AlertLevelEvent onAlertLevelChanged;

        [TabGroup("AI Control", "Events")]
        [LabelText("Target Detected")]
        [SerializeField] private DetectionEvent onTargetDetected;

        // Hierarchical State Machine
        private AIContext context;
        private HierarchicalState<AIContext> currentState;
        private Dictionary<string, HierarchicalState<AIContext>> states;

        // State instances
        private AIPatrolState patrolState;
        private AIAlertState alertState;

        // Properties for external access
        public AIContext Context => context;
        public Transform CurrentTarget => currentTarget;
        public AlertLevel CurrentAlertLevel => currentAlertLevel;
        public float SuspicionLevel => suspicionLevel;

        private void Awake()
        {
            InitializeComponents();
            InitializeContext();
            InitializeStates();
        }

        private void InitializeComponents()
        {
            if (navAgent == null)
                navAgent = GetComponent<NavMeshAgent>();

            if (eyePosition == null)
                eyePosition = transform;
        }

        private void InitializeContext()
        {
            context = new AIContext(
                transform,
                navAgent,
                eyePosition,
                patrolPoints,
                patrolWaitTime,
                sightRange,
                sightAngle,
                hearingRange,
                detectionSpeed,
                onAlertLevelChanged,
                onTargetDetected
            );
        }

        private void InitializeStates()
        {
            // 階層化ステートの作成
            patrolState = new AIPatrolState();
            alertState = new AIAlertState();

            states = new Dictionary<string, HierarchicalState<AIContext>>
            {
                { "Patrol", patrolState },
                { "Alert", alertState }
            };

            // 状態を初期化 - HierarchicalStateは自動で初期化されるため処理不要
            // foreach (var state in states.Values)
            // {
            //     state.Initialize(); // HierarchicalStateにInitialize()メソッドは存在しない
            // }
        }

        private void Start()
        {
            // 初期状態の設定
            if (patrolPoints != null && patrolPoints.Length > 0)
            {
                TransitionToState("Patrol");
            }
            else
            {
                TransitionToState("Alert");
            }
        }

        private void Update()
        {
            UpdateContext();
            UpdateCurrentState();
            UpdateInspectorValues();
            CheckStateTransitions();
        }

        private void UpdateContext()
        {
            context.UpdateStateTimer();

            // コンテキストから値を同期
            currentTarget = context.CurrentTarget;
            lastKnownPosition = context.LastKnownPosition;
            suspicionLevel = context.SuspicionLevel;
            currentAlertLevel = context.CurrentAlertLevel;
        }

        private void UpdateCurrentState()
        {
            currentState?.Update(context);
        }

        private void UpdateInspectorValues()
        {
            currentStateName = GetCurrentStateName() ?? "None";
            currentChildStateName = currentState?.GetCurrentChildStateKey() ?? "None";
        }

        private void CheckStateTransitions()
        {
            // 状態遷移ロジック
            if (currentState == patrolState)
            {
                // Patrol -> Alert: 疑惑レベルが高いか、目標を発見
                if (context.SuspicionLevel >= 0.3f || context.CurrentTarget != null)
                {
                    TransitionToState("Alert");
                }
            }
            else if (currentState == alertState)
            {
                // Alert -> Patrol: 疑惑レベルが低く、目標がいない
                if (context.SuspicionLevel < 0.2f && context.CurrentTarget == null && context.StateTimer > 10f)
                {
                    TransitionToState("Patrol");
                }
            }
        }

        public void TransitionToState(string stateName)
        {
            if (!states.ContainsKey(stateName))
            {
                Debug.LogWarning($"[HierarchicalAIStateMachine] State '{stateName}' not found");
                return;
            }

            if (currentState != null && GetCurrentStateName() == stateName)
                return;

            // 現在の状態を終了
            currentState?.Exit(context);

            // 新しい状態に遷移
            currentState = states[stateName];
            context.ResetStateTimer();
            currentState.Enter(context);

            Debug.Log($"[HierarchicalAIStateMachine] Transitioned to state: {stateName}");
        }

        // 外部からのイベント
        public void OnSightTarget(Transform target)
        {
            context.SetTarget(target);

            // 現在の状態にイベントを通知
            if (currentState == patrolState)
            {
                patrolState.OnSightTarget(context, target);
            }
            else if (currentState == alertState)
            {
                alertState.OnSightTarget(context, target);
            }
        }

        public void OnHearNoise(Vector3 noisePosition, float noiseLevel)
        {
            // 現在の状態にイベントを通知
            if (currentState == patrolState)
            {
                patrolState.OnHearNoise(context, noisePosition, noiseLevel);
            }
            else if (currentState == alertState)
            {
                alertState.OnHearNoise(context, noisePosition, noiseLevel);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // TODO: 階層化状態での物理接触処理
            Debug.Log($"[HierarchicalAIStateMachine] Trigger entered: {other.name}");
        }

                // Helper method to get current state name
        private string GetCurrentStateName()
        {
            if (currentState == null) return null;
            
            // Get state name from the dictionary by finding the key for the current state
            foreach (var kvp in states)
            {
                if (kvp.Value == currentState)
                    return kvp.Key;
            }
            
            return currentState.GetType().Name; // Fallback to type name
        }

        
// Utility Methods
        public bool IsInState(string stateName)
        {
            return GetCurrentStateName() == stateName;
        }

        public bool IsInChildState(string childStateName)
        {
            return currentState?.GetCurrentChildStateKey() == childStateName;
        }

        // Odin Inspector用のカラーゲッター
        private UnityEngine.Color GetSuspicionColor()
        {
            return suspicionLevel switch
            {
                < 0.3f => UnityEngine.Color.green,
                < 0.7f => UnityEngine.Color.yellow,
                _ => UnityEngine.Color.red
            };
        }

        // デバッグ用の状態情報取得
        [Button("Get State Info")]
        private void LogStateInfo()
        {
            if (currentState != null)
            {
                Debug.Log($"Current State: {GetCurrentStateName()}");
                Debug.Log($"Current Child: {currentState.GetCurrentChildStateKey()}");
                Debug.Log($"State History: {string.Join(", ", currentState.GetStateHistory())}");
                Debug.Log($"Alert Level: {context.CurrentAlertLevel}");
                Debug.Log($"Suspicion Level: {context.SuspicionLevel:F2}");
            }
        }

        // エディタでの状態強制遷移（デバッグ用）
        [Button("Force Patrol State")]
        private void ForcePatrolState()
        {
            if (Application.isPlaying)
            {
                TransitionToState("Patrol");
            }
        }

        [Button("Force Alert State")]
        private void ForceAlertState()
        {
            if (Application.isPlaying)
            {
                TransitionToState("Alert");
            }
        }
    }
}
