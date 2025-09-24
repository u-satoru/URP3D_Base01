using UnityEngine;

namespace asterivo.Unity60.Features.Camera.States
{
    [CreateAssetMenu(menuName = "asterivo/Camera/Settings/Aim Settings", fileName = "AimSettings")]
    public class AimSettings : ScriptableObject
    {
        [Header("Aim Position")]
        public Vector3 aimOffset = new Vector3(0.8f, 0.2f, 0f);
        public float aimDistance = 1.5f;
        
        [Header("Field of View")]
        public float aimFOV = 40f;
        public float aimFieldOfView = 40f;  // Legacy support
        public float minFOV = 15f;
        public float maxFOV = 60f;
        public float zoomSpeed = 5f;

        [Header("Sensitivity")]
        public float aimSensitivity = 1f;
        public float preciseSensitivity = 0.5f;
        public float aimSmoothing = 5f;
        public float aimAssistStrength = 0.3f;
        public float aimAssistRange = 10f;

        [Header("Rotation Limits")]
        public float minVerticalAngle = -20f;
        public float maxVerticalAngle = 20f;
        
        [Header("Sway")]
        public bool enableWeaponSway = true;
        public float swayAmount = 0.02f;
        public float swaySpeed = 2f;
        public float maxSwayAmount = 0.06f;
        
        [Header("Recoil")]
        public bool enableRecoil = true;
        public float recoilAmount = 1f;
        public float recoilSpeed = 10f;
        public float recoilRecoverySpeed = 5f;
        
        private void Reset()
        {
            aimOffset = new Vector3(0.8f, 0.2f, 0f);
            aimDistance = 1.5f;
            aimFOV = 40f;
            aimFieldOfView = 40f;
            minFOV = 15f;
            maxFOV = 60f;
            zoomSpeed = 5f;
            aimSensitivity = 1f;
            preciseSensitivity = 0.5f;
            aimSmoothing = 5f;
            aimAssistStrength = 0.3f;
            aimAssistRange = 10f;
            minVerticalAngle = -20f;
            maxVerticalAngle = 20f;
            enableWeaponSway = true;
            swayAmount = 0.02f;
            swaySpeed = 2f;
            maxSwayAmount = 0.06f;
            enableRecoil = true;
            recoilAmount = 1f;
            recoilSpeed = 10f;
            recoilRecoverySpeed = 5f;
        }
    }
}