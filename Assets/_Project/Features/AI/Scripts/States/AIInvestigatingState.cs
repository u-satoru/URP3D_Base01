using UnityEngine;

namespace asterivo.Unity60.Features.AI.States
{
    public class AIInvestigatingState : IAIState
    {
        private bool hasReachedInvestigationPoint = false;
        private float investigationTimer = 0f;
        private float investigationDuration = 4f;
        
        public void Enter(AIStateMachine stateMachine)
        {
            hasReachedInvestigationPoint = false;
            investigationTimer = 0f;
            
            if (stateMachine.LastKnownPosition != Vector3.zero)
            {
                stateMachine.NavAgent.isStopped = false;
                stateMachine.NavAgent.SetDestination(stateMachine.LastKnownPosition);
            }
        }
        
        public void Exit(AIStateMachine stateMachine)
        {
            stateMachine.NavAgent.isStopped = true;
        }
        
        public void Update(AIStateMachine stateMachine)
        {
            if (!hasReachedInvestigationPoint)
            {
                if (stateMachine.HasReachedDestination())
                {
                    hasReachedInvestigationPoint = true;
                    stateMachine.NavAgent.isStopped = true;
                }
            }
            else
            {
                investigationTimer += Time.deltaTime;
                InvestigateArea(stateMachine);
                
                if (investigationTimer >= investigationDuration)
                {
                    if (stateMachine.CurrentTarget == null)
                    {
                        stateMachine.TransitionToState(AIStateMachine.AIStateType.Searching);
                    }
                }
            }
            
            if (stateMachine.CurrentTarget != null)
            {
                stateMachine.TransitionToState(AIStateMachine.AIStateType.Alert);
            }
        }
        
        public void OnTriggerEnter(AIStateMachine stateMachine, Collider other)
        {
        }
        
        public void OnSightTarget(AIStateMachine stateMachine, Transform target)
        {
            stateMachine.SetTarget(target);
            stateMachine.TransitionToState(AIStateMachine.AIStateType.Alert);
        }
        
        public void OnHearNoise(AIStateMachine stateMachine, Vector3 noisePosition, float noiseLevel)
        {
            if (noiseLevel > 0.5f)
            {
                stateMachine.LastKnownPosition = noisePosition;
                hasReachedInvestigationPoint = false;
                investigationTimer = 0f;
                stateMachine.NavAgent.SetDestination(noisePosition);
            }
        }
        
        private void InvestigateArea(AIStateMachine stateMachine)
        {
            float rotationSpeed = 60f;
            stateMachine.transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
        }
    }
}