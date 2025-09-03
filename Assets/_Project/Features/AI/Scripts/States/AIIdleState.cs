using UnityEngine;

namespace asterivo.Unity60.AI.States
{
    public class AIIdleState : IAIState
    {
        private float idleTimer = 0f;
        private float lookAroundInterval = 3f;
        
        public void Enter(AIStateMachine stateMachine)
        {
            stateMachine.NavAgent.isStopped = true;
            idleTimer = 0f;
        }
        
        public void Exit(AIStateMachine stateMachine)
        {
            stateMachine.NavAgent.isStopped = false;
        }
        
        public void Update(AIStateMachine stateMachine)
        {
            idleTimer += Time.deltaTime;
            
            if (idleTimer >= lookAroundInterval)
            {
                LookAround(stateMachine);
                idleTimer = 0f;
            }
            
            if (stateMachine.PatrolPoints != null && stateMachine.PatrolPoints.Length > 0)
            {
                if (stateMachine.StateTimer > 5f)
                {
                    stateMachine.TransitionToState(AIStateMachine.AIStateType.Patrol);
                }
            }
        }
        
        public void OnTriggerEnter(AIStateMachine stateMachine, Collider other)
        {
        }
        
        public void OnSightTarget(AIStateMachine stateMachine, Transform target)
        {
            stateMachine.SetTarget(target);
            stateMachine.IncreaseSuspicion(1f);
        }
        
        public void OnHearNoise(AIStateMachine stateMachine, Vector3 noisePosition, float noiseLevel)
        {
            if (noiseLevel > 0.5f)
            {
                stateMachine.LastKnownPosition = noisePosition;
                stateMachine.TransitionToState(AIStateMachine.AIStateType.Suspicious);
            }
        }
        
        private void LookAround(AIStateMachine stateMachine)
        {
            float randomAngle = Random.Range(-45f, 45f);
            Vector3 lookDirection = Quaternion.Euler(0, randomAngle, 0) * stateMachine.transform.forward;
            stateMachine.transform.rotation = Quaternion.LookRotation(lookDirection);
        }
    }
}