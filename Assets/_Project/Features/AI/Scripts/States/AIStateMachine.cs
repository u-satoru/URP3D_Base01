using UnityEngine;
using Debug = UnityEngine.Debug;
using UnityEngine.AI;
using System.Collections.Generic;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Data;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.AI.States
{
    public class AIStateMachine : MonoBehaviour
    {
        [TabGroup("AI Control", "State Management")]
        [ReadOnly]
        [LabelText("Current State")]
        [SerializeField] private AIStateType currentStateType;
        
        [TabGroup("AI Control", "State Management")]
        [ReadOnly]
        [LabelText("Previous State")]
        [SerializeField] private AIStateType previousStateType;
        
        [TabGroup("AI Control", "State Management")]
        [ReadOnly]
        [LabelText("Alert Level")]
        [SerializeField] private AlertLevel currentAlertLevel = AlertLevel.Unaware;
        
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
        // TODO: AI視覚検知システムの実装予定（LineOfSight、Raycast使用）
#pragma warning disable CS0414 // Field assigned but never used - planned for AI sight detection system
        [SerializeField] private float sightRange = 15f;
#pragma warning restore CS0414
        
        [TabGroup("AI Control", "Senses")]
        [PropertyRange(30f, 180f)]
        [LabelText("Sight Angle")]
        [SuffixLabel("°", overlay: true)]
        // TODO: AI視野角による検知判定システムの実装予定
#pragma warning disable CS0414 // Field assigned but never used - planned for AI sight angle detection
        [SerializeField] private float sightAngle = 110f;
#pragma warning restore CS0414
        
        [TabGroup("AI Control", "Senses")]
        [PropertyRange(3f, 25f)]
        [LabelText("Hearing Range")]
        [SuffixLabel("m", overlay: true)]
        // TODO: AI聴覚検知システムの実装予定（StealthMovementController連携）
#pragma warning disable CS0414 // Field assigned but never used - planned for AI hearing detection system
        [SerializeField] private float hearingRange = 10f;
#pragma warning restore CS0414
        
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
        [ReadOnly]
        [LabelText("Current Patrol Index")]
        [SerializeField] private int currentPatrolIndex = 0;
        
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
        
        private IAIState currentState;
        private Dictionary<AIStateType, IAIState> states;
        private float stateTimer = 0f;
        
        public enum AIStateType
        {
            Idle,
            Patrol,
            Suspicious,
            Investigating,
            Searching,
            Alert,
            Combat,
            TakingCover,
            Flanking,
            Dead
        }
        
        public NavMeshAgent NavAgent => navAgent;
        public Transform EyePosition => eyePosition;
        public Transform CurrentTarget => currentTarget;
        public Vector3 LastKnownPosition 
        { 
            get => lastKnownPosition;
            set => lastKnownPosition = value;
        }
        public Transform[] PatrolPoints => patrolPoints;
        public int CurrentPatrolIndex => currentPatrolIndex;
        public float PatrolWaitTime => patrolWaitTime;
        public float StateTimer => stateTimer;
        public AlertLevel CurrentAlertLevel => currentAlertLevel;
        public float SuspicionLevel => suspicionLevel;
        
        private void Awake()
        {
            InitializeComponents();
            InitializeStates();
        }
        
        private void InitializeComponents()
        {
            if (navAgent == null)
                navAgent = GetComponent<NavMeshAgent>();
                
            if (eyePosition == null)
                eyePosition = transform;
        }
        
        private void InitializeStates()
        {
            states = new Dictionary<AIStateType, IAIState>
            {
                { AIStateType.Idle, new AIIdleState() },
                { AIStateType.Patrol, new AIPatrolState() },
                { AIStateType.Suspicious, new AISuspiciousState() },
                { AIStateType.Investigating, new AIInvestigatingState() },
                { AIStateType.Searching, new AISearchingState() },
                { AIStateType.Alert, new AIAlertState() },
                { AIStateType.Combat, new AICombatState() }
            };
        }
        
        private void Start()
        {
            if (patrolPoints != null && patrolPoints.Length > 0)
            {
                TransitionToState(AIStateType.Patrol);
            }
            else
            {
                TransitionToState(AIStateType.Idle);
            }
        }
        
        private void Update()
        {
            stateTimer += Time.deltaTime;
            UpdateSuspicion();
            currentState?.Update(this);
        }
        
        public void TransitionToState(AIStateType newStateType)
        {
            if (currentStateType == newStateType && currentState != null)
                return;
                
            if (!states.ContainsKey(newStateType))
            {
                Debug.LogWarning($"AI State {newStateType} not found");
                return;
            }
            
            previousStateType = currentStateType;
            currentState?.Exit(this);
            
            currentStateType = newStateType;
            currentState = states[newStateType];
            stateTimer = 0f;
            currentState.Enter(this);
            
            UpdateAlertLevel(newStateType);
        }
        
        private void UpdateAlertLevel(AIStateType stateType)
        {
            AlertLevel newLevel = stateType switch
            {
                AIStateType.Idle => AlertLevel.Unaware,
                AIStateType.Patrol => AlertLevel.Unaware,
                AIStateType.Suspicious => AlertLevel.Suspicious,
                AIStateType.Investigating => AlertLevel.Investigating,
                AIStateType.Searching => AlertLevel.Searching,
                AIStateType.Alert => AlertLevel.Alert,
                AIStateType.Combat => AlertLevel.Combat,
                _ => AlertLevel.Unaware
            };
            
            if (currentAlertLevel != newLevel)
            {
                currentAlertLevel = newLevel;
                onAlertLevelChanged?.Raise(currentAlertLevel);
            }
        }
        
        private void UpdateSuspicion()
        {
            if (currentTarget == null && suspicionLevel > 0)
            {
                suspicionLevel -= Time.deltaTime * 0.5f;
                suspicionLevel = Mathf.Max(0, suspicionLevel);
            }
        }
        
        public void SetTarget(Transform target)
        {
            currentTarget = target;
            if (target != null)
            {
                lastKnownPosition = target.position;
            }
        }
        
        public void ClearTarget()
        {
            currentTarget = null;
        }
        
        public void IncreaseSuspicion(float amount)
        {
            suspicionLevel += amount * detectionSpeed * Time.deltaTime;
            suspicionLevel = Mathf.Clamp01(suspicionLevel);
            
            if (suspicionLevel >= 0.3f && currentStateType == AIStateType.Patrol)
            {
                TransitionToState(AIStateType.Suspicious);
            }
            else if (suspicionLevel >= 0.7f)
            {
                TransitionToState(AIStateType.Alert);
            }
        }
        
        public void SetPatrolIndex(int index)
        {
            currentPatrolIndex = index % patrolPoints.Length;
        }
        
        public void NextPatrolPoint()
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        }
        
        public bool HasReachedDestination()
        {
            return !navAgent.pathPending && navAgent.remainingDistance < 0.5f;
        }
        
        public void OnSightTarget(Transform target)
        {
            currentState?.OnSightTarget(this, target);
        }
        
        public void OnHearNoise(Vector3 noisePosition, float noiseLevel)
        {
            currentState?.OnHearNoise(this, noisePosition, noiseLevel);
        }
        
        private void OnTriggerEnter(Collider other)
        {
            currentState?.OnTriggerEnter(this, other);
        }
        
        public AIStateType GetCurrentStateType() => currentStateType;
        public bool IsInState(AIStateType stateType) => currentStateType == stateType;
        
        // NavMeshAgentのセーフティメソッド
        public void SafeStopAgent()
        {
            if (navAgent != null && navAgent.isOnNavMesh)
            {
                navAgent.isStopped = true;
            }
        }
        
        public void SafeStartAgent()
        {
            if (navAgent != null && navAgent.isOnNavMesh)
            {
                navAgent.isStopped = false;
            }
        }
        
        public bool CanUseNavAgent()
        {
            return navAgent != null && navAgent.isOnNavMesh;
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
    }
}