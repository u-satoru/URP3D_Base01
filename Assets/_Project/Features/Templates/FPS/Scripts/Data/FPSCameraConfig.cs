using UnityEngine;

namespace asterivo.Unity60.Features.Templates.FPS.Data
{
    /// <summary>
    /// FPSカメラ設定データ
    /// 詳細設計書3.3準拠：カメラFOV、遷移速度、エフェクト設定を管理
    /// </summary>
    [CreateAssetMenu(menuName = "Templates/FPS/FPS Camera Config", fileName = "FPS_CameraConfig")]
    public class FPSCameraConfig : ScriptableObject
    {
        [Header("Field of View Settings")]
        [SerializeField, Range(60f, 120f)]
        private float _firstPersonFOV = 90f;

        [SerializeField, Range(60f, 120f)]
        private float _weaponFOV = 85f;

        [SerializeField, Range(30f, 90f)]
        private float _aimingFOV = 45f;

        [Header("Camera Transition")]
        [SerializeField, Range(1f, 10f)]
        private float _fovTransitionSpeed = 5f;

        [SerializeField, Range(0.1f, 2f)]
        private float _cameraSwitchDuration = 0.3f;

        [Header("Camera Effects")]
        [SerializeField]
        private bool _enableRecoilShake = true;

        [SerializeField, Range(0f, 2f)]
        private float _recoilShakeIntensity = 0.5f;

        [SerializeField, Range(0.1f, 1f)]
        private float _recoilShakeDuration = 0.2f;

        [Header("Weapon Camera Settings")]
        [SerializeField]
        private Vector3 _weaponCameraOffset = new Vector3(0.1f, -0.05f, 0.3f);

        [SerializeField, Range(0f, 1f)]
        private float _weaponBobIntensity = 0.3f;

        [SerializeField, Range(1f, 5f)]
        private float _weaponBobFrequency = 2f;

        [Header("Aiming Camera Settings")]
        [SerializeField]
        private Vector3 _aimingCameraOffset = new Vector3(0f, 0f, 0.2f);

        [SerializeField, Range(0.1f, 1f)]
        private float _aimingSensitivityMultiplier = 0.6f;

        [SerializeField]
        private bool _enableBreathingEffect = true;

        [SerializeField, Range(0f, 0.5f)]
        private float _breathingIntensity = 0.1f;

        [Header("Performance Settings")]
        [SerializeField]
        private bool _enableCameraCulling = true;

        [SerializeField, Range(10f, 1000f)]
        private float _cullingDistance = 100f;

        [Header("Debug")]
        [SerializeField]
        private bool _showDebugInfo = false;

        // Properties
        public float firstPersonFOV => _firstPersonFOV;
        public float weaponFOV => _weaponFOV;
        public float aimingFOV => _aimingFOV;
        public float fovTransitionSpeed => _fovTransitionSpeed;
        public float cameraSwitchDuration => _cameraSwitchDuration;
        public bool enableRecoilShake => _enableRecoilShake;
        public float recoilShakeIntensity => _recoilShakeIntensity;
        public float recoilShakeDuration => _recoilShakeDuration;
        public Vector3 weaponCameraOffset => _weaponCameraOffset;
        public float weaponBobIntensity => _weaponBobIntensity;
        public float weaponBobFrequency => _weaponBobFrequency;
        public Vector3 aimingCameraOffset => _aimingCameraOffset;
        public float aimingSensitivityMultiplier => _aimingSensitivityMultiplier;
        public bool enableBreathingEffect => _enableBreathingEffect;
        public float breathingIntensity => _breathingIntensity;
        public bool enableCameraCulling => _enableCameraCulling;
        public float cullingDistance => _cullingDistance;
        public bool showDebugInfo => _showDebugInfo;

        /// <summary>
        /// 設定の検証
        /// </summary>
        public bool ValidateSettings()
        {
            bool isValid = true;

            if (_firstPersonFOV <= 0 || _firstPersonFOV > 180)
            {
                Debug.LogWarning($"[FPSCameraConfig] Invalid first person FOV: {_firstPersonFOV}");
                isValid = false;
            }

            if (_weaponFOV <= 0 || _weaponFOV > 180)
            {
                Debug.LogWarning($"[FPSCameraConfig] Invalid weapon FOV: {_weaponFOV}");
                isValid = false;
            }

            if (_aimingFOV <= 0 || _aimingFOV > 180)
            {
                Debug.LogWarning($"[FPSCameraConfig] Invalid aiming FOV: {_aimingFOV}");
                isValid = false;
            }

            if (_fovTransitionSpeed <= 0)
            {
                Debug.LogWarning($"[FPSCameraConfig] Invalid FOV transition speed: {_fovTransitionSpeed}");
                isValid = false;
            }

            return isValid;
        }

        /// <summary>
        /// デフォルト設定の適用
        /// </summary>
        [ContextMenu("Apply Default Settings")]
        public void ApplyDefaultSettings()
        {
            _firstPersonFOV = 90f;
            _weaponFOV = 85f;
            _aimingFOV = 45f;
            _fovTransitionSpeed = 5f;
            _cameraSwitchDuration = 0.3f;
            _enableRecoilShake = true;
            _recoilShakeIntensity = 0.5f;
            _recoilShakeDuration = 0.2f;
            _weaponCameraOffset = new Vector3(0.1f, -0.05f, 0.3f);
            _weaponBobIntensity = 0.3f;
            _weaponBobFrequency = 2f;
            _aimingCameraOffset = new Vector3(0f, 0f, 0.2f);
            _aimingSensitivityMultiplier = 0.6f;
            _enableBreathingEffect = true;
            _breathingIntensity = 0.1f;
            _enableCameraCulling = true;
            _cullingDistance = 100f;
            _showDebugInfo = false;

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif

            Debug.Log("[FPSCameraConfig] Default settings applied");
        }

        /// <summary>
        /// タクティカルシューター向け設定
        /// </summary>
        [ContextMenu("Apply Tactical Settings")]
        public void ApplyTacticalSettings()
        {
            _firstPersonFOV = 75f;
            _weaponFOV = 70f;
            _aimingFOV = 30f;
            _fovTransitionSpeed = 3f;
            _cameraSwitchDuration = 0.5f;
            _enableRecoilShake = true;
            _recoilShakeIntensity = 0.8f;
            _recoilShakeDuration = 0.3f;
            _aimingSensitivityMultiplier = 0.4f;
            _enableBreathingEffect = true;
            _breathingIntensity = 0.2f;

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif

            Debug.Log("[FPSCameraConfig] Tactical settings applied");
        }

        /// <summary>
        /// カジュアルシューター向け設定
        /// </summary>
        [ContextMenu("Apply Casual Settings")]
        public void ApplyCasualSettings()
        {
            _firstPersonFOV = 100f;
            _weaponFOV = 95f;
            _aimingFOV = 60f;
            _fovTransitionSpeed = 8f;
            _cameraSwitchDuration = 0.2f;
            _enableRecoilShake = true;
            _recoilShakeIntensity = 0.3f;
            _recoilShakeDuration = 0.1f;
            _aimingSensitivityMultiplier = 0.8f;
            _enableBreathingEffect = false;
            _breathingIntensity = 0.05f;

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif

            Debug.Log("[FPSCameraConfig] Casual settings applied");
        }

        /// <summary>
        /// 設定統計情報の取得
        /// </summary>
        public CameraConfigStats GetConfigStats()
        {
            return new CameraConfigStats
            {
                FOVRange = _aimingFOV - _firstPersonFOV,
                TransitionSpeed = _fovTransitionSpeed,
                RecoilEnabled = _enableRecoilShake,
                BreathingEnabled = _enableBreathingEffect,
                CullingEnabled = _enableCameraCulling
            };
        }

        private void OnValidate()
        {
            // Inspector で値が変更された時の自動検証
            ValidateSettings();

            // FOVの論理的整合性チェック
            if (_aimingFOV >= _firstPersonFOV)
            {
                Debug.LogWarning("[FPSCameraConfig] Aiming FOV should be lower than first person FOV for proper zoom effect");
            }

            if (_weaponFOV >= _firstPersonFOV)
            {
                Debug.LogWarning("[FPSCameraConfig] Weapon FOV should be lower than first person FOV");
            }
        }
    }

    /// <summary>
    /// カメラ設定統計情報
    /// </summary>
    [System.Serializable]
    public struct CameraConfigStats
    {
        public float FOVRange;
        public float TransitionSpeed;
        public bool RecoilEnabled;
        public bool BreathingEnabled;
        public bool CullingEnabled;
    }
}
