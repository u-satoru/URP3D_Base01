using UnityEngine;

namespace asterivo.Unity60.Camera.States
{
    public class ThirdPersonCameraState : ICameraState
    {
        private float mouseSensitivity = 2f;
        private float currentDistance = 5f;
        private float currentX = 0f;
        private float currentY = 30f;
        private float minDistance = 1f;
        private float maxDistance = 10f;
        private Vector3 offset;
        
        public void Enter(CameraStateMachine stateMachine)
        {
            if (stateMachine.ThirdPersonSettings != null)
            {
                mouseSensitivity = stateMachine.ThirdPersonSettings.mouseSensitivity;
                currentDistance = stateMachine.ThirdPersonSettings.cameraDistance;
                minDistance = stateMachine.ThirdPersonSettings.minDistance;
                maxDistance = stateMachine.ThirdPersonSettings.maxDistance;
                offset = stateMachine.ThirdPersonSettings.shoulderOffset;
                
                stateMachine.MainCamera.fieldOfView = stateMachine.ThirdPersonSettings.fieldOfView;
            }
            
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        
        public void Exit(CameraStateMachine stateMachine)
        {
        }
        
        public void Update(CameraStateMachine stateMachine)
        {
            float scrollInput = Input.GetAxis("Mouse ScrollWheel");
            if (scrollInput != 0)
            {
                currentDistance -= scrollInput * 2f;
                currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);
            }
        }
        
        public void LateUpdate(CameraStateMachine stateMachine)
        {
            if (stateMachine.FollowTarget != null && stateMachine.CameraRig != null)
            {
                HandleMouseLook(stateMachine);
                UpdateCameraPosition(stateMachine);
                HandleCollision(stateMachine);
            }
        }
        
        public void HandleInput(CameraStateMachine stateMachine)
        {
            if (Input.GetKeyDown(KeyCode.V))
            {
                stateMachine.TransitionToState(CameraStateMachine.CameraStateType.FirstPerson);
            }
            
            if (Input.GetMouseButton(1))
            {
                stateMachine.TransitionToState(CameraStateMachine.CameraStateType.Aim);
            }
        }
        
        private void HandleMouseLook(CameraStateMachine stateMachine)
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
            
            currentX += mouseX;
            currentY -= mouseY;
            currentY = Mathf.Clamp(currentY, -80f, 80f);
        }
        
        private void UpdateCameraPosition(CameraStateMachine stateMachine)
        {
            Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
            Vector3 targetPosition = stateMachine.FollowTarget.position;
            
            Vector3 negDistance = new Vector3(offset.x, offset.y, -currentDistance);
            Vector3 position = rotation * negDistance + targetPosition;
            
            stateMachine.CameraRig.position = position;
            stateMachine.CameraRig.LookAt(targetPosition + Vector3.up * offset.y);
        }
        
        private void HandleCollision(CameraStateMachine stateMachine)
        {
            if (stateMachine.ThirdPersonSettings == null || !stateMachine.ThirdPersonSettings.enableCollisionDetection)
                return;
                
            RaycastHit hit;
            Vector3 targetPosition = stateMachine.FollowTarget.position + Vector3.up * offset.y;
            Vector3 direction = stateMachine.CameraRig.position - targetPosition;
            
            if (Physics.SphereCast(targetPosition, stateMachine.ThirdPersonSettings.collisionRadius, 
                direction.normalized, out hit, direction.magnitude, 
                stateMachine.ThirdPersonSettings.collisionLayers))
            {
                stateMachine.CameraRig.position = hit.point + hit.normal * stateMachine.ThirdPersonSettings.collisionRadius;
            }
        }
    }
}