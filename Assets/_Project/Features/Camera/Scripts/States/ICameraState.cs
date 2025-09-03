using UnityEngine;

namespace asterivo.Unity60.Camera.States
{
    public interface ICameraState
    {
        void Enter(CameraStateMachine stateMachine);
        void Exit(CameraStateMachine stateMachine);
        void Update(CameraStateMachine stateMachine);
        void LateUpdate(CameraStateMachine stateMachine);
        void HandleInput(CameraStateMachine stateMachine);
    }
}