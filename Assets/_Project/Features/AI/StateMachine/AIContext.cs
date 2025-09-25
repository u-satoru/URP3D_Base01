using UnityEngine;
using UnityEngine.AI;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Data;

namespace asterivo.Unity60.Features.AI.StateMachine
{
    /// <summary>
    /// AI階層化ステートマシンのコンテキスト
    /// AI状態間で共有されるデータとシステム参照を管理
    /// </summary>
    public class AIContext
    {
        // Core AI Properties
        public Transform Transform { get; }
        public NavMeshAgent NavAgent { get; }
        public Transform EyePosition { get; }

        // Detection and Awareness
        public Transform CurrentTarget { get; set; }
        public Vector3 LastKnownPosition { get; set; }
        public float SuspicionLevel { get; set; }
        public AlertLevel CurrentAlertLevel { get; set; }

        // Patrol System
        public Transform[] PatrolPoints { get; }
        public int CurrentPatrolIndex { get; set; }
        public float PatrolWaitTime { get; }

        // Sensor Settings
        public float SightRange { get; }
        public float SightAngle { get; }
        public float HearingRange { get; }
        public float DetectionSpeed { get; }

        // State Management
        public float StateTimer { get; set; }
        public string PreviousStateKey { get; set; }

        // Events
        public AlertLevelEvent OnAlertLevelChanged { get; }
        public DetectionEvent OnTargetDetected { get; }

        // System Interfaces (for future expansion)
        public IAISensorSystem SensorSystem { get; set; }
        public IAIMemorySystem MemorySystem { get; set; }
        public IAINavigationSystem NavigationSystem { get; set; }

        public AIContext(
            Transform transform,
            NavMeshAgent navAgent,
            Transform eyePosition,
            Transform[] patrolPoints,
            float patrolWaitTime,
            float sightRange,
            float sightAngle,
            float hearingRange,
            float detectionSpeed,
            AlertLevelEvent onAlertLevelChanged,
            DetectionEvent onTargetDetected)
        {
            Transform = transform;
            NavAgent = navAgent;
            EyePosition = eyePosition ?? transform;
            PatrolPoints = patrolPoints;
            PatrolWaitTime = patrolWaitTime;
            SightRange = sightRange;
            SightAngle = sightAngle;
            HearingRange = hearingRange;
            DetectionSpeed = detectionSpeed;
            OnAlertLevelChanged = onAlertLevelChanged;
            OnTargetDetected = onTargetDetected;

            // Initialize state
            CurrentPatrolIndex = 0;
            SuspicionLevel = 0f;
            CurrentAlertLevel = AlertLevel.Relaxed;
            StateTimer = 0f;
        }

        // Navigation Helper Methods
        public bool HasReachedDestination()
        {
            return NavAgent != null && !NavAgent.pathPending && NavAgent.remainingDistance < 0.5f;
        }

        public bool CanUseNavAgent()
        {
            return NavAgent != null && NavAgent.isOnNavMesh;
        }

        public void SafeStopAgent()
        {
            if (CanUseNavAgent())
            {
                NavAgent.isStopped = true;
            }
        }

        public void SafeStartAgent()
        {
            if (CanUseNavAgent())
            {
                NavAgent.isStopped = false;
            }
        }

        public void SetDestination(Vector3 destination)
        {
            if (CanUseNavAgent())
            {
                NavAgent.SetDestination(destination);
            }
        }

        // Patrol Helper Methods
        public Vector3 GetCurrentPatrolPoint()
        {
            if (PatrolPoints == null || PatrolPoints.Length == 0)
                return Transform.position;

            return PatrolPoints[CurrentPatrolIndex].position;
        }

        public void NextPatrolPoint()
        {
            if (PatrolPoints == null || PatrolPoints.Length == 0)
                return;

            CurrentPatrolIndex = (CurrentPatrolIndex + 1) % PatrolPoints.Length;
        }

        // Target Management
        public void SetTarget(Transform target)
        {
            CurrentTarget = target;
            if (target != null)
            {
                LastKnownPosition = target.position;
                OnTargetDetected?.Raise(new DetectionInfo
                {
                    type = DetectionType.Visual,
                    confidence = 1.0f,
                    detectedPosition = target.position,
                    detectedDirection = (target.position - Transform.position).normalized,
                    distance = Vector3.Distance(Transform.position, target.position),
                    detectedObject = target.gameObject,
                    detectionTime = System.DateTime.Now,
                    isConfirmed = true
                });
            }
        }

        public void ClearTarget()
        {
            CurrentTarget = null;
        }

        // Suspicion Management
        public void IncreaseSuspicion(float amount)
        {
            SuspicionLevel += amount * DetectionSpeed * Time.deltaTime;
            SuspicionLevel = Mathf.Clamp01(SuspicionLevel);
        }

        public void DecreaseSuspicion(float amount)
        {
            SuspicionLevel -= amount * Time.deltaTime;
            SuspicionLevel = Mathf.Max(0f, SuspicionLevel);
        }

        // Alert Level Management
        public void SetAlertLevel(AlertLevel newLevel)
        {
            if (CurrentAlertLevel != newLevel)
            {
                CurrentAlertLevel = newLevel;
                OnAlertLevelChanged?.Raise(newLevel);
            }
        }

        // Time Management
        public void ResetStateTimer()
        {
            StateTimer = 0f;
        }

        public void UpdateStateTimer()
        {
            StateTimer += Time.deltaTime;
        }
    }

    // System Interfaces for future expansion
    public interface IAISensorSystem
    {
        bool CanSeeTarget(Transform target);
        bool CanHearNoise(Vector3 noisePosition, float noiseLevel);
        float GetVisibilityFactor(Transform target);
    }

    public interface IAIMemorySystem
    {
        void RememberLocation(Vector3 location, float importance);
        Vector3? GetMostImportantLocation();
        void ForgetOldMemories(float maxAge);
    }

    public interface IAINavigationSystem
    {
        bool FindPathToTarget(Vector3 target);
        Vector3 GetFlankingPosition(Transform target);
        Vector3 GetCoverPosition(Transform threat);
    }

    
}
