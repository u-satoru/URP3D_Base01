using UnityEngine;

namespace asterivo.Unity60.Features.AI.States
{
    public class AISuspiciousState : IAIState
    {
        private float suspiciousTimer = 0f;
        private float maxSuspiciousTime = 5f;
        private Vector3 originalPosition;
        
        public void Enter(AIStateMachine stateMachine)
        {
            stateMachine.NavAgent.isStopped = true;
            suspiciousTimer = 0f;
            originalPosition = stateMachine.transform.position;
            
            if (stateMachine.LastKnownPosition != Vector3.zero)
            {
                Vector3 lookDirection = stateMachine.LastKnownPosition - stateMachine.transform.position;
                lookDirection.y = 0;
                if (lookDirection.magnitude > 0.1f)
                {
                    stateMachine.transform.rotation = Quaternion.LookRotation(lookDirection);
                }
            }
        }
        
        public void Exit(AIStateMachine stateMachine)
        {
            stateMachine.NavAgent.isStopped = false;
        }
        
        public void Update(AIStateMachine stateMachine)
        {
            suspiciousTimer += Time.deltaTime;
            
            LookAroundSuspiciously(stateMachine);
            
            if (stateMachine.CurrentTarget != null)
            {
                stateMachine.IncreaseSuspicion(2f);
                
                if (stateMachine.SuspicionLevel >= 0.7f)
                {
                    stateMachine.TransitionToState(AIStateMachine.AIStateType.Alert);
                }
            }
            else if (suspiciousTimer >= maxSuspiciousTime)
            {
                if (stateMachine.LastKnownPosition != Vector3.zero)
                {
                    stateMachine.TransitionToState(AIStateMachine.AIStateType.Investigating);
                }
                else
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
            stateMachine.IncreaseSuspicion(3f);
        }
        
        public void OnHearNoise(AIStateMachine stateMachine, Vector3 noisePosition, float noiseLevel)
        {
            if (noiseLevel > 0.7f)
            {
                stateMachine.LastKnownPosition = noisePosition;
                stateMachine.TransitionToState(AIStateMachine.AIStateType.Investigating);
            }
        }
        
        private void LookAroundSuspiciously(AIStateMachine stateMachine)
        {
            float lookSpeed = 30f;
            float angle = Mathf.Sin(Time.time * 2f) * 45f;
            Vector3 lookDirection = Quaternion.Euler(0, angle, 0) * Vector3.forward;
            
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            stateMachine.transform.rotation = Quaternion.Slerp(
                stateMachine.transform.rotation,
                targetRotation,
                lookSpeed * Time.deltaTime
            );
        }
    }
}
