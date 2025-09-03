using UnityEngine;

namespace asterivo.Unity60.AI.States
{
    public interface IAIState
    {
        void Enter(AIStateMachine stateMachine);
        void Exit(AIStateMachine stateMachine);
        void Update(AIStateMachine stateMachine);
        void OnTriggerEnter(AIStateMachine stateMachine, Collider other);
        void OnSightTarget(AIStateMachine stateMachine, Transform target);
        void OnHearNoise(AIStateMachine stateMachine, Vector3 noisePosition, float noiseLevel);
    }
}