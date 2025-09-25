using UnityEngine;

namespace asterivo.Unity60.Features.Camera.States
{
    public class CoverCameraState : ICameraState
    {
        private float coverDistance = 3f;
        private float coverHeight = 1.2f;
        private Vector3 coverOffset;
        private float transitionSpeed = 8f;
        private bool isPeeking = false;
        private PeekDirection currentPeekDirection = PeekDirection.None;
        
        private enum PeekDirection
        {
            None,
            Left,
            Right,
            Over
        }
        
        public void Enter(CameraStateMachine stateMachine)
        {
            if (stateMachine.CoverSettings != null)
            {
                coverDistance = stateMachine.CoverSettings.coverDistance;
                coverHeight = stateMachine.CoverSettings.coverHeight;
                coverOffset = stateMachine.CoverSettings.cameraOffset;
                
                stateMachine.MainCamera.fieldOfView = stateMachine.CoverSettings.fieldOfView;
            }
        }
        
        public void Exit(CameraStateMachine stateMachine)
        {
            isPeeking = false;
            currentPeekDirection = PeekDirection.None;
        }
        
        public void Update(CameraStateMachine stateMachine)
        {
            HandlePeekInput();
        }
        
        public void LateUpdate(CameraStateMachine stateMachine)
        {
            if (stateMachine.FollowTarget != null && stateMachine.CameraRig != null)
            {
                UpdateCoverCameraPosition(stateMachine);
            }
        }
        
        public void HandleInput(CameraStateMachine stateMachine)
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                stateMachine.TransitionToState(CameraStateMachine.CameraStateType.ThirdPerson);
            }

            if (Input.GetMouseButton(1))
            {
                stateMachine.TransitionToState(CameraStateMachine.CameraStateType.Aim);
            }
        }

        public void OnResetRequested(CameraStateMachine stateMachine)
        {
            // Reset peek state
            isPeeking = false;
            currentPeekDirection = PeekDirection.None;

            // Reset to default values if CoverSettings are available
            if (stateMachine.CoverSettings != null)
            {
                coverDistance = stateMachine.CoverSettings.coverDistance;
                coverHeight = stateMachine.CoverSettings.coverHeight;
                coverOffset = stateMachine.CoverSettings.cameraOffset;
                stateMachine.MainCamera.fieldOfView = stateMachine.CoverSettings.fieldOfView;
            }
        }
        
        private void HandlePeekInput()
        {
            if (Input.GetKey(KeyCode.E))
            {
                isPeeking = true;
                currentPeekDirection = PeekDirection.Right;
            }
            else if (Input.GetKey(KeyCode.Q))
            {
                isPeeking = true;
                currentPeekDirection = PeekDirection.Left;
            }
            else if (Input.GetKey(KeyCode.Space))
            {
                isPeeking = true;
                currentPeekDirection = PeekDirection.Over;
            }
            else
            {
                isPeeking = false;
                currentPeekDirection = PeekDirection.None;
            }
        }
        
        private void UpdateCoverCameraPosition(CameraStateMachine stateMachine)
        {
            Vector3 targetPosition = stateMachine.FollowTarget.position;
            Vector3 desiredPosition = targetPosition;
            
            desiredPosition += stateMachine.FollowTarget.right * coverOffset.x;
            desiredPosition += Vector3.up * coverHeight;
            desiredPosition -= stateMachine.FollowTarget.forward * coverDistance;
            
            if (isPeeking)
            {
                Vector3 peekOffset = GetPeekOffset(stateMachine);
                desiredPosition += peekOffset;
            }
            
            stateMachine.CameraRig.position = Vector3.Lerp(
                stateMachine.CameraRig.position,
                desiredPosition,
                Time.deltaTime * transitionSpeed
            );
            
            Vector3 lookTarget = targetPosition + Vector3.up * 1f;
            stateMachine.CameraRig.LookAt(lookTarget);
        }
        
        private Vector3 GetPeekOffset(CameraStateMachine stateMachine)
        {
            float peekAmount = stateMachine.CoverSettings != null ? stateMachine.CoverSettings.peekOffset : 1f;
            
            return currentPeekDirection switch
            {
                PeekDirection.Left => -stateMachine.FollowTarget.right * peekAmount,
                PeekDirection.Right => stateMachine.FollowTarget.right * peekAmount,
                PeekDirection.Over => Vector3.up * peekAmount,
                _ => Vector3.zero
            };
        }
    }
}