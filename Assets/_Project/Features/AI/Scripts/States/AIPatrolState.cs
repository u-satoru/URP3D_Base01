using UnityEngine;

namespace asterivo.Unity60.Features.AI.States
{
    public class AIPatrolState : IAIState
    {
        private float waitTimer = 0f;
        private bool isWaiting = false;
        
        public void Enter(AIStateMachine stateMachine)
        {
            if (stateMachine.PatrolPoints != null && stateMachine.PatrolPoints.Length > 0)
            {
                MoveToNextPatrolPoint(stateMachine);
            }
        }
        
        public void Exit(AIStateMachine stateMachine)
        {
            stateMachine.NavAgent.isStopped = true;
        }
        
        public void Update(AIStateMachine stateMachine)
        {
            if (isWaiting)
            {
                waitTimer += Time.deltaTime;
                if (waitTimer >= stateMachine.PatrolWaitTime)
                {
                    isWaiting = false;
                    MoveToNextPatrolPoint(stateMachine);
                }
            }
            else if (stateMachine.HasReachedDestination())
            {
                isWaiting = true;
                waitTimer = 0f;
                stateMachine.NextPatrolPoint();
            }
        }
        
        public void OnTriggerEnter(AIStateMachine stateMachine, Collider other)
        {
        }
        
        public void OnSightTarget(AIStateMachine stateMachine, Transform target)
        {
            stateMachine.SetTarget(target);
            stateMachine.IncreaseSuspicion(1f);
            
            if (stateMachine.SuspicionLevel > 0.3f)
            {
                stateMachine.TransitionToState(AIStateMachine.AIStateType.Suspicious);
            }
        }
        
        public void OnHearNoise(AIStateMachine stateMachine, Vector3 noisePosition, float noiseLevel)
        {
            if (noiseLevel > 0.3f)
            {
                stateMachine.LastKnownPosition = noisePosition;
                stateMachine.TransitionToState(AIStateMachine.AIStateType.Suspicious);
            }
        }
        
        private void MoveToNextPatrolPoint(AIStateMachine stateMachine)
        {
            if (stateMachine.PatrolPoints == null || stateMachine.PatrolPoints.Length == 0)
                return;
                
            Transform targetPoint = stateMachine.PatrolPoints[stateMachine.CurrentPatrolIndex];
            if (targetPoint != null)
            {
                stateMachine.NavAgent.isStopped = false;
                stateMachine.NavAgent.SetDestination(targetPoint.position);
            }
        }
    }
}