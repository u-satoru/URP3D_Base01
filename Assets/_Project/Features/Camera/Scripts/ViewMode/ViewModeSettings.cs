using UnityEngine;

namespace asterivo.Unity60.Features.Camera.ViewMode
{
    [System.Serializable]
    public abstract class ViewModeSettings : ScriptableObject
    {
        [Header("Camera Position")]
        public Vector3 cameraOffset = Vector3.zero;
        public Vector3 cameraRotation = Vector3.zero;
        
        [Header("Field of View")]
        public float fieldOfView = 60f;
        public float aimFieldOfView = 40f;
        
        [Header("Transition")]
        public float transitionDuration = 0.3f;
        public AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        [Header("Look Settings")]
        public float mouseSensitivity = 2f;
        public float maxLookAngle = 80f;
        public float minLookAngle = -80f;
        
        [Header("Collision")]
        public bool enableCollisionDetection = true;
        public float collisionRadius = 0.3f;
        public LayerMask collisionLayers = -1;
    }
    
    [CreateAssetMenu(menuName = "asterivo/Camera/Settings/First Person", fileName = "FPS_Settings")]
    public class FirstPersonSettings : ViewModeSettings
    {
        [Header("First Person Specific")]
        public float headBobAmount = 0.05f;
        public float headBobSpeed = 10f;
        public bool enableBreathing = true;
        public float breathingAmount = 0.02f;
        public float breathingSpeed = 2f;
        
        private void Reset()
        {
            cameraOffset = new Vector3(0f, 0.6f, 0.1f);
            cameraRotation = Vector3.zero;
            fieldOfView = 90f;
            aimFieldOfView = 60f;
            mouseSensitivity = 2.5f;
            enableCollisionDetection = false;
        }
    }
    
    [CreateAssetMenu(menuName = "asterivo/Camera/Settings/Third Person", fileName = "TPS_Settings")]
    public class ThirdPersonSettings : ViewModeSettings
    {
        [Header("Third Person Specific")]
        public float cameraDistance = 5f;
        public float distance = 5f; // Alias for cameraDistance
        public float minDistance = 1f;
        public float maxDistance = 10f;
        public Vector3 shoulderOffset = new Vector3(0.5f, 0f, 0f);
        public bool autoRotateWithPlayer = false;
        public float autoRotateSpeed = 5f;
        
        [Header("Camera Movement")]
        public float zoomSpeed = 3f;
        public float rotationSpeed = 100f;
        public float height = 1.5f;
        public float followSpeed = 5f;
        
        [Header("Look Constraints")]
        public float minVerticalAngle = -30f;
        public float maxVerticalAngle = 60f;
        
        private void Reset()
        {
            cameraOffset = new Vector3(0.5f, 1.5f, -3f);
            cameraRotation = new Vector3(10f, 0f, 0f);
            fieldOfView = 60f;
            aimFieldOfView = 50f;
            mouseSensitivity = 2f;
            cameraDistance = 5f;
            distance = 5f;
            zoomSpeed = 3f;
            rotationSpeed = 100f;
            height = 1.5f;
            followSpeed = 5f;
            minVerticalAngle = -30f;
            maxVerticalAngle = 60f;
            enableCollisionDetection = true;
        }
    }
    
    [CreateAssetMenu(menuName = "asterivo/Camera/Settings/Cover View", fileName = "Cover_Settings")]
    public class CoverViewSettings : ViewModeSettings
    {
        [Header("Cover Specific")]
        public float coverDistance = 3f;
        public float coverHeight = 1.2f;
        public float peekOffset = 1f;
        public bool enablePeeking = true;
        public float peekSpeed = 5f;
        
        private void Reset()
        {
            cameraOffset = new Vector3(1f, 1.2f, -2f);
            cameraRotation = new Vector3(5f, 0f, 0f);
            fieldOfView = 70f;
            aimFieldOfView = 45f;
            mouseSensitivity = 1.5f;
            coverDistance = 3f;
            enableCollisionDetection = true;
        }
    }
}
