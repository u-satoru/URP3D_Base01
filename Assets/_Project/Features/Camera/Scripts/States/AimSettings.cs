using UnityEngine;

namespace asterivo.Unity60.Camera.States
{
    [CreateAssetMenu(menuName = "asterivo/Camera/Settings/Aim Settings", fileName = "AimSettings")]
    public class AimSettings : ScriptableObject
    {
        [Header("Aim Position")]
        public Vector3 aimOffset = new Vector3(0.8f, 0.2f, 0f);
        public float aimDistance = 1.5f;
        
        [Header("Field of View")]
        public float aimFieldOfView = 40f;
        public float zoomSpeed = 5f;
        
        [Header("Sensitivity")]
        public float aimSensitivity = 1f;
        public float aimAssistStrength = 0.3f;
        public float aimAssistRange = 10f;
        
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
            aimFieldOfView = 40f;
            zoomSpeed = 5f;
            aimSensitivity = 1f;
            aimAssistStrength = 0.3f;
            aimAssistRange = 10f;
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