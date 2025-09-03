using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Data;

namespace asterivo.Unity60.AI.States
{
    public class AIStateMachine : MonoBehaviour
    {
        [Header("State Management")]
        [SerializeField] private AIStateType currentStateType;
        [SerializeField] private AIStateType previousStateType;
        [SerializeField] private AlertLevel currentAlertLevel = AlertLevel.Unaware;
        
        [Header("AI Components")]
        [SerializeField] private NavMeshAgent navAgent;
        [SerializeField] private Transform eyePosition;
        [SerializeField] private float sightRange = 15f;
        [SerializeField] private float sightAngle = 110f;
        [SerializeField] private float hearingRange = 10f;
        
        [Header("Detection")]
        [SerializeField] private Transform currentTarget;
        [SerializeField] private Vector3 lastKnownPosition;
        [SerializeField] private float suspicionLevel = 0f;
        [SerializeField] private float detectionSpeed = 1f;
        
        [Header("Patrol Settings")]
        [SerializeField] private Transform[] patrolPoints;
        [SerializeField] private int currentPatrolIndex = 0;
        [SerializeField] private float patrolWaitTime = 2f;
        
        [Header("Events")]
        [SerializeField] private AlertLevelEvent onAlertLevelChanged;
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
    }
}