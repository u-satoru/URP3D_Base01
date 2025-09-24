using UnityEngine;

namespace asterivo.Unity60.Features.Camera.States
{
    public class AimCameraState : ICameraState
    {
        private float mouseSensitivity = 1.5f;
        private float currentX = 0f;
        private float currentY = 30f;
        private float aimDistance = 2f;
        private Vector3 aimOffset;
        private float transitionSpeed = 10f;
        
        public void Enter(CameraStateMachine stateMachine)
        {
            if (stateMachine.AimSettings != null)
            {
                mouseSensitivity = stateMachine.AimSettings.aimSensitivity;
                aimDistance = stateMachine.AimSettings.aimDistance;
                aimOffset = stateMachine.AimSettings.aimOffset;
                
                stateMachine.MainCamera.fieldOfView = stateMachine.AimSettings.aimFieldOfView;
            }
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
                HandleMouseLook(stateMachine);
                UpdateAimPosition(stateMachine);
            }
        }
        
        public void HandleInput(CameraStateMachine stateMachine)
        {
            if (!Input.GetMouseButton(1))
            {
                stateMachine.ReturnToPreviousState();
            }
        }

        public void OnResetRequested(CameraStateMachine stateMachine)
        {
            // Reset camera rotation to default values
            currentX = 0f;
            currentY = 30f;

            // Reset to default values if AimSettings are available
            if (stateMachine.AimSettings != null)
            {
                mouseSensitivity = stateMachine.AimSettings.aimSensitivity;
                aimDistance = stateMachine.AimSettings.aimDistance;
                aimOffset = stateMachine.AimSettings.aimOffset;
                stateMachine.MainCamera.fieldOfView = stateMachine.AimSettings.aimFieldOfView;
            }
        }
        
        private void HandleMouseLook(CameraStateMachine stateMachine)
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
            
            currentX += mouseX;
            currentY -= mouseY;
            currentY = Mathf.Clamp(currentY, -60f, 60f);
        }
        
        private void UpdateAimPosition(CameraStateMachine stateMachine)
        {
            Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
            Vector3 targetPosition = stateMachine.FollowTarget.position + Vector3.up * 1.5f;
            
            Vector3 negDistance = new Vector3(aimOffset.x, aimOffset.y, -aimDistance);
            Vector3 desiredPosition = rotation * negDistance + targetPosition;
            
            stateMachine.CameraRig.position = Vector3.Lerp(
                stateMachine.CameraRig.position, 
                desiredPosition, 
                Time.deltaTime * transitionSpeed
            );
            
            Vector3 lookTarget = targetPosition + stateMachine.FollowTarget.forward * 10f;
            Quaternion desiredRotation = Quaternion.LookRotation(lookTarget - stateMachine.CameraRig.position);
            stateMachine.CameraRig.rotation = Quaternion.Slerp(
                stateMachine.CameraRig.rotation,
                desiredRotation,
                Time.deltaTime * transitionSpeed
            );
        }
    }
}