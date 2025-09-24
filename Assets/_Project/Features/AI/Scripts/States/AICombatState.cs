using UnityEngine;

namespace asterivo.Unity60.Features.AI.States
{
    public class AICombatState : IAIState
    {
        private float attackRange = 5f;
        private float meleeRange = 2f;
        private float strafeTimer = 0f;
        private float strafeDirection = 1f;
        private float nextStrafeChange = 2f;
        
        public void Enter(AIStateMachine stateMachine)
        {
            strafeTimer = 0f;
            strafeDirection = Random.Range(0, 2) == 0 ? -1f : 1f;
            nextStrafeChange = Random.Range(1.5f, 3f);
            
            stateMachine.NavAgent.stoppingDistance = attackRange;
            stateMachine.NavAgent.isStopped = false;
        }
        
        public void Exit(AIStateMachine stateMachine)
        {
            stateMachine.NavAgent.stoppingDistance = 0.5f;
        }
        
        public void Update(AIStateMachine stateMachine)
        {
            if (stateMachine.CurrentTarget == null)
            {
                stateMachine.TransitionToState(AIStateMachine.AIStateType.Searching);
                return;
            }
            
            float distanceToTarget = Vector3.Distance(
                stateMachine.transform.position,
                stateMachine.CurrentTarget.position
            );
            
            if (distanceToTarget > attackRange * 2)
            {
                ChaseTarget(stateMachine);
            }
            else if (distanceToTarget > attackRange)
            {
                MoveToAttackRange(stateMachine);
            }
            else if (distanceToTarget > meleeRange)
            {
                EngageAtRange(stateMachine);
            }
            else
            {
                MeleeAttack(stateMachine);
            }
            
            UpdateStrafe(stateMachine);
            FaceTarget(stateMachine);
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
        }
        
        private void ChaseTarget(AIStateMachine stateMachine)
        {
            stateMachine.NavAgent.isStopped = false;
            stateMachine.NavAgent.SetDestination(stateMachine.CurrentTarget.position);
        }
        
        private void MoveToAttackRange(AIStateMachine stateMachine)
        {
            stateMachine.NavAgent.isStopped = false;
            stateMachine.NavAgent.SetDestination(stateMachine.CurrentTarget.position);
        }
        
        private void EngageAtRange(AIStateMachine stateMachine)
        {
            stateMachine.NavAgent.isStopped = true;
            
            Vector3 strafeMovement = stateMachine.transform.right * strafeDirection * 2f * Time.deltaTime;
            stateMachine.transform.position += strafeMovement;
        }
        
        private void MeleeAttack(AIStateMachine stateMachine)
        {
            stateMachine.NavAgent.isStopped = true;
        }
        
        private void UpdateStrafe(AIStateMachine stateMachine)
        {
            strafeTimer += Time.deltaTime;
            if (strafeTimer >= nextStrafeChange)
            {
                strafeDirection *= -1f;
                strafeTimer = 0f;
                nextStrafeChange = Random.Range(1.5f, 3f);
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
                    Time.deltaTime * 8f
                );
            }
        }
    }
}
