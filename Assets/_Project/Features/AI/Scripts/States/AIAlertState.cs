using UnityEngine;

namespace asterivo.Unity60.Features.AI.States
{
    public class AIAlertState : IAIState
    {
        private float alertTimer = 0f;
        private float alertDuration = 3f;
        private bool hasAlertedOthers = false;
        
        public void Enter(AIStateMachine stateMachine)
        {
            alertTimer = 0f;
            hasAlertedOthers = false;
            
            if (stateMachine.CurrentTarget != null)
            {
                FaceTarget(stateMachine);
            }
            
            stateMachine.NavAgent.isStopped = true;
        }
        
        public void Exit(AIStateMachine stateMachine)
        {
            stateMachine.NavAgent.isStopped = false;
        }
        
        public void Update(AIStateMachine stateMachine)
        {
            alertTimer += Time.deltaTime;
            
            if (!hasAlertedOthers && alertTimer > 0.5f)
            {
                AlertNearbyUnits(stateMachine);
                hasAlertedOthers = true;
            }
            
            if (stateMachine.CurrentTarget != null)
            {
                FaceTarget(stateMachine);
                
                if (alertTimer >= alertDuration)
                {
                    stateMachine.TransitionToState(AIStateMachine.AIStateType.Combat);
                }
            }
            else
            {
                if (alertTimer >= alertDuration)
                {
                    stateMachine.TransitionToState(AIStateMachine.AIStateType.Searching);
                }
            }
        }
        
        public void OnTriggerEnter(AIStateMachine stateMachine, Collider other)
        {
        }
        
        public void OnSightTarget(AIStateMachine stateMachine, Transform target)
        {
            stateMachine.SetTarget(target);
            stateMachine.LastKnownPosition = target.position;
        }
        
        public void OnHearNoise(AIStateMachine stateMachine, Vector3 noisePosition, float noiseLevel)
        {
            if (stateMachine.CurrentTarget == null)
            {
                stateMachine.LastKnownPosition = noisePosition;
            }
        }
        
        private void FaceTarget(AIStateMachine stateMachine)
        {
            if (stateMachine.CurrentTarget == null) return;
            
            Vector3 direction = stateMachine.CurrentTarget.position - stateMachine.transform.position;
            direction.y = 0;
            
            if (direction.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                stateMachine.transform.rotation = Quaternion.Slerp(
                    stateMachine.transform.rotation,
                    targetRotation,
                    Time.deltaTime * 10f
                );
            }
        }
        
        private void AlertNearbyUnits(AIStateMachine stateMachine)
        {
            Collider[] nearbyColliders = Physics.OverlapSphere(
                stateMachine.transform.position, 
                15f
            );
            
            foreach (Collider col in nearbyColliders)
            {
                AIStateMachine otherAI = col.GetComponent<AIStateMachine>();
                if (otherAI != null && otherAI != stateMachine)
                {
                    if (otherAI.CurrentAlertLevel < Core.Data.AlertLevel.Alert)
                    {
                        otherAI.LastKnownPosition = stateMachine.LastKnownPosition;
                        otherAI.TransitionToState(AIStateMachine.AIStateType.Alert);
                    }
                }
            }
        }
    }
}
