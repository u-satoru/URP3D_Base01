using UnityEngine;

namespace asterivo.Unity60.Features.Camera.States
{
    public class FirstPersonCameraState : ICameraState
    {
        private float mouseSensitivity = 2f;
        private float verticalRotation = 0f;
        private float maxLookAngle = 80f;
        private float minLookAngle = -80f;
        
        public void Enter(CameraStateMachine stateMachine)
        {
            if (stateMachine.FirstPersonSettings != null)
            {
                mouseSensitivity = stateMachine.FirstPersonSettings.mouseSensitivity;
                maxLookAngle = stateMachine.FirstPersonSettings.maxLookAngle;
                minLookAngle = stateMachine.FirstPersonSettings.minLookAngle;
                
                stateMachine.MainCamera.fieldOfView = stateMachine.FirstPersonSettings.fieldOfView;
                
                if (stateMachine.CameraRig != null)
                {
                    stateMachine.CameraRig.localPosition = stateMachine.FirstPersonSettings.cameraOffset;
                    stateMachine.CameraRig.localRotation = Quaternion.Euler(stateMachine.FirstPersonSettings.cameraRotation);
                }
            }
            
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        
        public void Exit(CameraStateMachine stateMachine)
        {
        }
        
        public void Update(CameraStateMachine stateMachine)
        {
        }
        
        public void LateUpdate(CameraStateMachine stateMachine)
        {
            if (stateMachine.FollowTarget != null && stateMachine.CameraRig != null)
            {
                stateMachine.CameraRig.position = stateMachine.FollowTarget.position + 
                    stateMachine.FollowTarget.TransformDirection(stateMachine.FirstPersonSettings.cameraOffset);
                    
                HandleMouseLook(stateMachine);
            }
        }
        
        public void HandleInput(CameraStateMachine stateMachine)
        {
            if (Input.GetKeyDown(KeyCode.V))
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
            // Reset vertical rotation
            verticalRotation = 0f;

            // Reset to default values if FirstPersonSettings are available
            if (stateMachine.FirstPersonSettings != null)
            {
                mouseSensitivity = stateMachine.FirstPersonSettings.mouseSensitivity;
                maxLookAngle = stateMachine.FirstPersonSettings.maxLookAngle;
                minLookAngle = stateMachine.FirstPersonSettings.minLookAngle;
                stateMachine.MainCamera.fieldOfView = stateMachine.FirstPersonSettings.fieldOfView;

                if (stateMachine.CameraRig != null)
                {
                    stateMachine.CameraRig.localPosition = stateMachine.FirstPersonSettings.cameraOffset;
                    stateMachine.CameraRig.localRotation = Quaternion.Euler(stateMachine.FirstPersonSettings.cameraRotation);
                }
            }

            // Reset cursor state
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        
        private void HandleMouseLook(CameraStateMachine stateMachine)
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
            
            verticalRotation -= mouseY;
            verticalRotation = Mathf.Clamp(verticalRotation, minLookAngle, maxLookAngle);
            
            stateMachine.CameraRig.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
            
            if (stateMachine.FollowTarget != null)
            {
                stateMachine.FollowTarget.Rotate(Vector3.up * mouseX);
            }
        }
    }
}
