using UnityEngine;

namespace asterivo.Unity60.Features.AI.States
{
    public class AISearchingState : IAIState
    {
        private float searchTimer = 0f;
        private float searchDuration = 10f;
        private Vector3[] searchPoints;
        private int currentSearchIndex = 0;
        private bool isMovingToSearchPoint = false;
        
        public void Enter(AIStateMachine stateMachine)
        {
            searchTimer = 0f;
            currentSearchIndex = 0;
            GenerateSearchPoints(stateMachine);
            MoveToNextSearchPoint(stateMachine);
        }
        
        public void Exit(AIStateMachine stateMachine)
        {
            stateMachine.NavAgent.isStopped = true;
        }
        
        public void Update(AIStateMachine stateMachine)
        {
            searchTimer += Time.deltaTime;
            
            if (isMovingToSearchPoint)
            {
                if (stateMachine.HasReachedDestination())
                {
                    isMovingToSearchPoint = false;
                    LookAroundAtSearchPoint(stateMachine);
                    
                    if (currentSearchIndex < searchPoints.Length - 1)
                    {
                        currentSearchIndex++;
                        MoveToNextSearchPoint(stateMachine);
                    }
                }
            }
            
            if (searchTimer >= searchDuration && stateMachine.CurrentTarget == null)
            {
                stateMachine.TransitionToState(AIStateMachine.AIStateType.Patrol);
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
            if (noiseLevel > 0.6f)
            {
                stateMachine.LastKnownPosition = noisePosition;
                stateMachine.TransitionToState(AIStateMachine.AIStateType.Investigating);
            }
        }
        
        private void GenerateSearchPoints(AIStateMachine stateMachine)
        {
            searchPoints = new Vector3[4];
            Vector3 center = stateMachine.LastKnownPosition != Vector3.zero ? 
                stateMachine.LastKnownPosition : stateMachine.transform.position;
            
            float searchRadius = 8f;
            for (int i = 0; i < 4; i++)
            {
                float angle = i * 90f;
                Vector3 direction = Quaternion.Euler(0, angle, 0) * Vector3.forward;
                searchPoints[i] = center + direction * searchRadius;
            }
        }
        
        private void MoveToNextSearchPoint(AIStateMachine stateMachine)
        {
            if (currentSearchIndex < searchPoints.Length)
            {
                isMovingToSearchPoint = true;
                stateMachine.NavAgent.isStopped = false;
                stateMachine.NavAgent.SetDestination(searchPoints[currentSearchIndex]);
            }
        }
        
        private void LookAroundAtSearchPoint(AIStateMachine stateMachine)
        {
            for (int i = 0; i < 4; i++)
            {
                float angle = i * 90f;
                stateMachine.transform.rotation = Quaternion.Euler(0, angle, 0);
            }
        }
    }
}
