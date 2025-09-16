using UnityEngine;

namespace asterivo.Unity60.Features.Templates.FPS.Data
{
    /// <summary>
    /// FPSカメラ設定データ
    /// アーキテクチャ準拠: ScriptableObjectベースのデータ管理
    /// </summary>
    [CreateAssetMenu(menuName = "FPS Template/Camera Configuration", fileName = "CameraConfiguration")]
    [System.Serializable]
    public class CameraConfiguration : ScriptableObject
    {
        [Header("Default Settings")]
        [Tooltip("デフォルトの視野角")]
        [Range(30f, 120f)]
        public float DefaultFOV = 60f;

        [Header("First Person Settings")]
        [Tooltip("一人称視点の視野角")]
        [Range(30f, 120f)]
        public float FirstPersonFOV = 60f;

        [Tooltip("一人称視点のカメラ感度")]
        [Range(0.1f, 10f)]
        public float FirstPersonSensitivity = 2f;

        [Header("Third Person Settings")]
        [Tooltip("三人称視点の視野角")]
        [Range(30f, 120f)]
        public float ThirdPersonFOV = 60f;

        [Tooltip("三人称視点のカメラからの距離")]
        [Range(1f, 10f)]
        public float ThirdPersonDistance = 5f;

        [Tooltip("三人称視点の高さオフセット")]
        [Range(0f, 5f)]
        public float ThirdPersonHeightOffset = 2f;

        [Header("Aim Settings")]
        [Tooltip("照準時の視野角（ズーム効果）")]
        [Range(15f, 60f)]
        public float AimFOV = 40f;

        [Tooltip("照準時のカメラ感度（精密操作用）")]
        [Range(0.1f, 5f)]
        public float AimSensitivity = 1f;

        [Tooltip("照準時のズーム遷移時間")]
        [Range(0.1f, 2f)]
        public float AimTransitionTime = 0.3f;

        [Header("Cover Settings")]
        [Tooltip("カバー時の視野角")]
        [Range(30f, 80f)]
        public float CoverFOV = 55f;

        [Tooltip("カバー時のカメラオフセット（遮蔽物の横から覗く）")]
        public Vector3 CoverOffset = new Vector3(1.5f, 1.5f, -2f);

        [Header("Death Settings")]
        [Tooltip("死亡時の視野角")]
        [Range(40f, 90f)]
        public float DeathFOV = 70f;

        [Tooltip("死亡時のカメラ高度")]
        [Range(1f, 10f)]
        public float DeathHeight = 5f;

        [Header("Transition Settings")]
        [Tooltip("カメラ状態遷移時のブレンド時間")]
        [Range(0.1f, 3f)]
        public float TransitionBlendTime = 0.5f;

        [Tooltip("カメラシェイクの強度")]
        [Range(0f, 2f)]
        public float ShakeIntensity = 1f;

        [Header("Movement Settings")]
        [Tooltip("カメラのスムージング強度")]
        [Range(0.1f, 5f)]
        public float SmoothingStrength = 2f;

        [Tooltip("垂直軸の最大角度")]
        [Range(30f, 90f)]
        public float MaxVerticalAngle = 80f;

        [Tooltip("垂直軸の最小角度")]
        [Range(-90f, -30f)]
        public float MinVerticalAngle = -80f;

        [Header("Input Settings")]
        [Tooltip("マウス感度の倍率")]
        [Range(0.1f, 5f)]
        public float MouseSensitivityMultiplier = 1f;

        [Tooltip("ゲームパッド感度の倍率")]
        [Range(0.1f, 5f)]
        public float GamepadSensitivityMultiplier = 1.5f;

        [Tooltip("感度の反転設定")]
        public bool InvertYAxis = false;
        public bool InvertXAxis = false;

        /// <summary>
        /// 指定されたカメラ状態の推奨FOVを取得
        /// </summary>
        public float GetFOVForState(CameraState state)
        {
            return state switch
            {
                CameraState.FirstPerson => FirstPersonFOV,
                CameraState.ThirdPerson => ThirdPersonFOV,
                CameraState.Aim => AimFOV,
                CameraState.Cover => CoverFOV,
                CameraState.Death => DeathFOV,
                CameraState.Cinematic => DefaultFOV,
                _ => DefaultFOV
            };
        }

        /// <summary>
        /// 指定されたカメラ状態の推奨感度を取得
        /// </summary>
        public float GetSensitivityForState(CameraState state)
        {
            return state switch
            {
                CameraState.FirstPerson => FirstPersonSensitivity,
                CameraState.ThirdPerson => FirstPersonSensitivity * 0.8f,
                CameraState.Aim => AimSensitivity,
                CameraState.Cover => FirstPersonSensitivity * 0.7f,
                _ => FirstPersonSensitivity
            };
        }

        /// <summary>
        /// カメラ設定の妥当性検証
        /// </summary>
        public bool ValidateSettings()
        {
            bool isValid = true;

            if (FirstPersonFOV <= 0 || FirstPersonFOV > 120)
            {
                Debug.LogWarning($"[CameraConfiguration] Invalid FirstPersonFOV: {FirstPersonFOV}");
                isValid = false;
            }

            if (AimFOV >= FirstPersonFOV)
            {
                Debug.LogWarning($"[CameraConfiguration] AimFOV ({AimFOV}) should be smaller than FirstPersonFOV ({FirstPersonFOV})");
                isValid = false;
            }

            if (TransitionBlendTime <= 0)
            {
                Debug.LogWarning($"[CameraConfiguration] Invalid TransitionBlendTime: {TransitionBlendTime}");
                isValid = false;
            }

            return isValid;
        }

        /// <summary>
        /// デフォルト設定にリセット
        /// </summary>
        [ContextMenu("Reset to Default")]
        public void ResetToDefault()
        {
            DefaultFOV = 60f;
            FirstPersonFOV = 60f;
            FirstPersonSensitivity = 2f;
            ThirdPersonFOV = 60f;
            ThirdPersonDistance = 5f;
            ThirdPersonHeightOffset = 2f;
            AimFOV = 40f;
            AimSensitivity = 1f;
            AimTransitionTime = 0.3f;
            CoverFOV = 55f;
            CoverOffset = new Vector3(1.5f, 1.5f, -2f);
            DeathFOV = 70f;
            DeathHeight = 5f;
            TransitionBlendTime = 0.5f;
            ShakeIntensity = 1f;
            SmoothingStrength = 2f;
            MaxVerticalAngle = 80f;
            MinVerticalAngle = -80f;
            MouseSensitivityMultiplier = 1f;
            GamepadSensitivityMultiplier = 1.5f;
            InvertYAxis = false;
            InvertXAxis = false;

            Debug.Log("[CameraConfiguration] Settings reset to default");
        }

        #if UNITY_EDITOR
        private void OnValidate()
        {
            ValidateSettings();
        }
        #endif
    }
}