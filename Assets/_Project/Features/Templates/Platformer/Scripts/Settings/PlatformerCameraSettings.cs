using UnityEngine;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Features.Templates.Platformer
{
    /// <summary>
    /// プラットフォーマーカメラ設定クラス
    /// Cinemachine 3.1統合カメラ制御設定
    /// </summary>
    [System.Serializable]
    public class PlatformerCameraSettings
    {
        [BoxGroup("Follow Settings")]
        [LabelText("フォロー有効")]
        [SerializeField] private bool enableCameraFollow = true;

        [LabelText("フォロー速度"), Range(1f, 20f)]
        [ShowIf("enableCameraFollow")]
        [SerializeField] private float followSpeed = 8f;

        [LabelText("フォローオフセット")]
        [ShowIf("enableCameraFollow")]
        [SerializeField] private Vector3 followOffset = new Vector3(0f, 3f, -10f);

        [BoxGroup("Look Ahead")]
        [LabelText("先読み有効")]
        [SerializeField] private bool enableLookAhead = true;

        [LabelText("先読み距離"), Range(1f, 10f)]
        [ShowIf("enableLookAhead")]
        [SerializeField] private float lookAheadDistance = 4f;

        [LabelText("先読み速度"), Range(0.5f, 5f)]
        [ShowIf("enableLookAhead")]
        [SerializeField] private float lookAheadSpeed = 2f;

        [LabelText("先読み戻り速度"), Range(0.5f, 5f)]
        [ShowIf("enableLookAhead")]
        [SerializeField] private float lookAheadReturnSpeed = 1.5f;

        [BoxGroup("Camera Bounds")]
        [LabelText("カメラ境界有効")]
        [SerializeField] private bool enableCameraBounds = true;

        [LabelText("境界設定")]
        [ShowIf("enableCameraBounds")]
        [SerializeField] private Bounds cameraBounds = new Bounds(Vector3.zero, new Vector3(50f, 30f, 20f));

        [LabelText("境界ソフト有効")]
        [ShowIf("enableCameraBounds")]
        [SerializeField] private bool enableSoftBounds = true;

        [LabelText("ソフト境界距離"), Range(1f, 10f)]
        [ShowIf("@enableCameraBounds && enableSoftBounds")]
        [SerializeField] private float softBoundaryDistance = 3f;

        [BoxGroup("Dynamic Framing")]
        [LabelText("動的フレーミング有効")]
        [SerializeField] private bool enableDynamicFraming = true;

        [LabelText("フレーミング調整速度"), Range(0.5f, 5f)]
        [ShowIf("enableDynamicFraming")]
        [SerializeField] private float framingAdjustmentSpeed = 2f;

        [LabelText("ジャンプ時フレーミング"), Range(0.5f, 2f)]
        [ShowIf("enableDynamicFraming")]
        [SerializeField] private float jumpFramingMultiplier = 1.3f;

        [LabelText("落下時フレーミング"), Range(0.5f, 2f)]
        [ShowIf("enableDynamicFraming")]
        [SerializeField] private float fallFramingMultiplier = 1.2f;

        [BoxGroup("Camera Shake")]
        [LabelText("カメラシェイク有効")]
        [SerializeField] private bool enableCameraShake = true;

        [LabelText("着地時シェイク強度"), Range(0f, 2f)]
        [ShowIf("enableCameraShake")]
        [SerializeField] private float landingShakeIntensity = 0.3f;

        [LabelText("ダメージ時シェイク強度"), Range(0f, 2f)]
        [ShowIf("enableCameraShake")]
        [SerializeField] private float damageShakeIntensity = 0.8f;

        [LabelText("シェイク継続時間"), Range(0.1f, 1f)]
        [ShowIf("enableCameraShake")]
        [SerializeField] private float shakeDuration = 0.2f;

        [BoxGroup("Field of View")]
        [LabelText("FOV調整有効")]
        [SerializeField] private bool enableFOVAdjustment = true;

        [LabelText("基本FOV"), Range(40f, 90f)]
        [ShowIf("enableFOVAdjustment")]
        [SerializeField] private float baseFOV = 60f;

        [LabelText("高速移動時FOV"), Range(60f, 110f)]
        [ShowIf("enableFOVAdjustment")]
        [SerializeField] private float highSpeedFOV = 75f;

        [LabelText("FOV調整速度"), Range(1f, 10f)]
        [ShowIf("enableFOVAdjustment")]
        [SerializeField] private float fovAdjustmentSpeed = 3f;

        [BoxGroup("Cinemachine Integration")]
        [LabelText("バーチャルカメラ優先度"), Range(1, 20)]
        [SerializeField] private int virtualCameraPriority = 10;

        [LabelText("ブレンド時間"), Range(0.1f, 3f)]
        [SerializeField] private float blendTime = 1f;

        [LabelText("ノイズプロファイル使用")]
        [SerializeField] private bool useNoiseProfile = true;

        [LabelText("ノイズ振幅"), Range(0f, 2f)]
        [ShowIf("useNoiseProfile")]
        [SerializeField] private float noiseAmplitude = 0.1f;

        [LabelText("ノイズ周波数"), Range(0.1f, 5f)]
        [ShowIf("useNoiseProfile")]
        [SerializeField] private float noiseFrequency = 1f;

        [BoxGroup("Performance")]
        [LabelText("カメラ更新頻度"), Range(30, 120)]
        [SerializeField] private int cameraUpdateRate = 60;

        [LabelText("LOD有効")]
        [SerializeField] private bool enableLOD = true;

        [LabelText("近距離LOD範囲"), Range(5f, 20f)]
        [ShowIf("enableLOD")]
        [SerializeField] private float nearLODDistance = 10f;

        [LabelText("遠距離LOD範囲"), Range(20f, 100f)]
        [ShowIf("enableLOD")]
        [SerializeField] private float farLODDistance = 50f;

        #region Public Properties
        public bool EnableCameraFollow => enableCameraFollow;
        public float FollowSpeed => followSpeed;
        public Vector3 FollowOffset => followOffset;
        public bool EnableLookAhead => enableLookAhead;
        public float LookAheadDistance => lookAheadDistance;
        public float LookAheadSpeed => lookAheadSpeed;
        public float LookAheadReturnSpeed => lookAheadReturnSpeed;
        public bool EnableCameraBounds => enableCameraBounds;
        public Bounds CameraBounds => cameraBounds;
        public bool EnableSoftBounds => enableSoftBounds;
        public float SoftBoundaryDistance => softBoundaryDistance;
        public bool EnableDynamicFraming => enableDynamicFraming;
        public float FramingAdjustmentSpeed => framingAdjustmentSpeed;
        public float JumpFramingMultiplier => jumpFramingMultiplier;
        public float FallFramingMultiplier => fallFramingMultiplier;
        public bool EnableCameraShake => enableCameraShake;
        public float LandingShakeIntensity => landingShakeIntensity;
        public float DamageShakeIntensity => damageShakeIntensity;
        public float ShakeDuration => shakeDuration;
        public bool EnableFOVAdjustment => enableFOVAdjustment;
        public float BaseFOV => baseFOV;
        public float HighSpeedFOV => highSpeedFOV;
        public float FOVAdjustmentSpeed => fovAdjustmentSpeed;
        public int VirtualCameraPriority => virtualCameraPriority;
        public float BlendTime => blendTime;
        public bool UseNoiseProfile => useNoiseProfile;
        public float NoiseAmplitude => noiseAmplitude;
        public float NoiseFrequency => noiseFrequency;
        public int CameraUpdateRate => cameraUpdateRate;
        public bool EnableLOD => enableLOD;
        public float NearLODDistance => nearLODDistance;
        public float FarLODDistance => farLODDistance;
        #endregion

        #region Initialization & Validation
        public void Initialize()
        {
            // カメラ設定の妥当性確認
            followSpeed = Mathf.Clamp(followSpeed, 1f, 20f);
            lookAheadDistance = Mathf.Clamp(lookAheadDistance, 1f, 10f);
            baseFOV = Mathf.Clamp(baseFOV, 40f, 90f);
            highSpeedFOV = Mathf.Clamp(highSpeedFOV, 60f, 110f);

            Debug.Log($"[PlatformerCamera] Initialized: Follow={enableCameraFollow}, LookAhead={enableLookAhead}, FOV={baseFOV}");
        }

        public bool Validate()
        {
            bool isValid = true;

            // フォロー設定検証
            if (enableCameraFollow && followSpeed <= 0)
            {
                Debug.LogError("[PlatformerCamera] Follow speed must be positive");
                isValid = false;
            }

            // 先読み設定検証
            if (enableLookAhead)
            {
                if (lookAheadDistance <= 0)
                {
                    Debug.LogError("[PlatformerCamera] Look ahead distance must be positive");
                    isValid = false;
                }

                if (lookAheadSpeed <= 0 || lookAheadReturnSpeed <= 0)
                {
                    Debug.LogError("[PlatformerCamera] Look ahead speeds must be positive");
                    isValid = false;
                }
            }

            // FOV設定検証
            if (enableFOVAdjustment)
            {
                if (baseFOV <= 0 || baseFOV > 180f)
                {
                    Debug.LogError("[PlatformerCamera] Invalid base FOV");
                    isValid = false;
                }

                if (highSpeedFOV <= baseFOV)
                {
                    Debug.LogError("[PlatformerCamera] High speed FOV must be greater than base FOV");
                    isValid = false;
                }
            }

            // 境界設定検証
            if (enableCameraBounds)
            {
                if (cameraBounds.size.x <= 0 || cameraBounds.size.y <= 0)
                {
                    Debug.LogError("[PlatformerCamera] Invalid camera bounds size");
                    isValid = false;
                }
            }

            // LOD設定検証
            if (enableLOD && farLODDistance <= nearLODDistance)
            {
                Debug.LogError("[PlatformerCamera] Far LOD distance must be greater than near LOD distance");
                isValid = false;
            }

            return isValid;
        }

        public void ApplyRecommendedSettings()
        {
            // 15分ゲームプレイ最適化設定
            enableCameraFollow = true;
            followSpeed = 10f;                // スムーズなフォロー
            followOffset = new Vector3(0f, 2.5f, -8f); // 適度な視点

            enableLookAhead = true;
            lookAheadDistance = 5f;           // 十分な先読み
            lookAheadSpeed = 2.5f;            // 快適な反応速度
            lookAheadReturnSpeed = 1.8f;      // 自然な戻り

            enableCameraBounds = true;
            cameraBounds = new Bounds(Vector3.zero, new Vector3(60f, 35f, 25f));
            enableSoftBounds = true;
            softBoundaryDistance = 4f;        // ソフトな境界

            enableDynamicFraming = true;
            framingAdjustmentSpeed = 2.2f;    // 動的調整
            jumpFramingMultiplier = 1.4f;     // ジャンプ時拡大
            fallFramingMultiplier = 1.25f;    // 落下時調整

            enableCameraShake = true;
            landingShakeIntensity = 0.25f;    // 適度な着地感
            damageShakeIntensity = 0.7f;      // インパクトあるダメージ
            shakeDuration = 0.18f;            // 短時間シェイク

            enableFOVAdjustment = true;
            baseFOV = 65f;                    // 標準視野角
            highSpeedFOV = 80f;               // 高速時拡大
            fovAdjustmentSpeed = 4f;          // スムーズ調整

            virtualCameraPriority = 10;
            blendTime = 0.8f;                 // 快適なブレンド

            useNoiseProfile = true;
            noiseAmplitude = 0.08f;           // 微細なノイズ
            noiseFrequency = 1.2f;            // 自然な揺れ

            cameraUpdateRate = 60;            // 標準更新頻度
            enableLOD = true;
            nearLODDistance = 12f;
            farLODDistance = 45f;

            Debug.Log("[PlatformerCamera] Applied recommended settings for immersive platformer experience");
        }
        #endregion

        #region Camera Calculations
        /// <summary>
        /// 現在のFOVを計算（プレイヤー速度に基づく）
        /// </summary>
        /// <param name="playerVelocity">プレイヤー速度</param>
        /// <param name="maxSpeed">最大速度</param>
        /// <returns>調整後FOV</returns>
        public float CalculateCurrentFOV(float playerVelocity, float maxSpeed)
        {
            if (!enableFOVAdjustment) return baseFOV;

            // 速度に基づいてFOVを補間
            float speedRatio = Mathf.Clamp01(playerVelocity / maxSpeed);
            return Mathf.Lerp(baseFOV, highSpeedFOV, speedRatio);
        }

        /// <summary>
        /// 先読み位置の計算
        /// </summary>
        /// <param name="playerPosition">プレイヤー位置</param>
        /// <param name="playerVelocity">プレイヤー速度</param>
        /// <returns>先読み位置</returns>
        public Vector3 CalculateLookAheadPosition(Vector3 playerPosition, Vector3 playerVelocity)
        {
            if (!enableLookAhead) return playerPosition;

            Vector3 lookAheadOffset = playerVelocity.normalized * lookAheadDistance;
            return playerPosition + lookAheadOffset;
        }

        /// <summary>
        /// カメラ境界内位置の計算
        /// </summary>
        /// <param name="targetPosition">目標位置</param>
        /// <returns>境界内調整位置</returns>
        public Vector3 CalculateBoundedPosition(Vector3 targetPosition)
        {
            if (!enableCameraBounds) return targetPosition;

            Vector3 boundedPosition = targetPosition;

            // 境界内に制限
            boundedPosition.x = Mathf.Clamp(boundedPosition.x,
                cameraBounds.min.x, cameraBounds.max.x);
            boundedPosition.y = Mathf.Clamp(boundedPosition.y,
                cameraBounds.min.y, cameraBounds.max.y);
            boundedPosition.z = Mathf.Clamp(boundedPosition.z,
                cameraBounds.min.z, cameraBounds.max.z);

            // ソフト境界の適用
            if (enableSoftBounds)
            {
                boundedPosition = ApplySoftBoundary(targetPosition, boundedPosition);
            }

            return boundedPosition;
        }

        /// <summary>
        /// ソフト境界の適用
        /// </summary>
        /// <param name="targetPosition">目標位置</param>
        /// <param name="boundedPosition">境界位置</param>
        /// <returns>ソフト境界調整位置</returns>
        private Vector3 ApplySoftBoundary(Vector3 targetPosition, Vector3 boundedPosition)
        {
            Vector3 softPosition = boundedPosition;

            // 各軸でソフト境界を計算
            for (int axis = 0; axis < 3; axis++)
            {
                float target = targetPosition[axis];
                float min = cameraBounds.min[axis];
                float max = cameraBounds.max[axis];

                if (target < min + softBoundaryDistance)
                {
                    float ratio = (target - min) / softBoundaryDistance;
                    softPosition[axis] = Mathf.Lerp(min, min + softBoundaryDistance,
                        Mathf.SmoothStep(0f, 1f, ratio));
                }
                else if (target > max - softBoundaryDistance)
                {
                    float ratio = (max - target) / softBoundaryDistance;
                    softPosition[axis] = Mathf.Lerp(max, max - softBoundaryDistance,
                        Mathf.SmoothStep(0f, 1f, ratio));
                }
            }

            return softPosition;
        }

        /// <summary>
        /// 動的フレーミング倍率の計算
        /// </summary>
        /// <param name="isJumping">ジャンプ中かどうか</param>
        /// <param name="isFalling">落下中かどうか</param>
        /// <returns>フレーミング倍率</returns>
        public float CalculateFramingMultiplier(bool isJumping, bool isFalling)
        {
            if (!enableDynamicFraming) return 1f;

            if (isJumping) return jumpFramingMultiplier;
            if (isFalling) return fallFramingMultiplier;
            return 1f;
        }

        /// <summary>
        /// カメラシェイク強度の計算
        /// </summary>
        /// <param name="shakeType">シェイクタイプ</param>
        /// <param name="impactStrength">インパクト強度（0-1）</param>
        /// <returns>シェイク強度</returns>
        public float CalculateShakeIntensity(CameraShakeType shakeType, float impactStrength = 1f)
        {
            if (!enableCameraShake) return 0f;

            float baseIntensity = shakeType switch
            {
                CameraShakeType.Landing => landingShakeIntensity,
                CameraShakeType.Damage => damageShakeIntensity,
                _ => 0f
            };

            return baseIntensity * Mathf.Clamp01(impactStrength);
        }
        #endregion

        #region Editor Support
#if UNITY_EDITOR
        [Button("Test FOV Calculation")]
        [PropertySpace(10)]
        public void TestFOVCalculation()
        {
            float[] testSpeeds = { 0f, 5f, 10f, 15f };
            foreach (float speed in testSpeeds)
            {
                float fov = CalculateCurrentFOV(speed, 15f);
                Debug.Log($"Speed {speed}: FOV = {fov:F1}°");
            }
        }

        [Button("Test Look Ahead")]
        public void TestLookAhead()
        {
            Vector3 playerPos = Vector3.zero;
            Vector3 velocity = new Vector3(10f, 0f, 0f);
            Vector3 lookAheadPos = CalculateLookAheadPosition(playerPos, velocity);
            Debug.Log($"Player: {playerPos}, LookAhead: {lookAheadPos}");
        }

        [Button("Validate Camera Settings")]
        public void EditorValidate()
        {
            bool isValid = Validate();
            string message = isValid ?
                "✅ Camera settings are valid!" :
                "❌ Camera settings validation failed!";
            Debug.Log($"[PlatformerCamera] {message}");
        }
#endif
        #endregion
    }

    /// <summary>
    /// カメラシェイクタイプ
    /// </summary>
    public enum CameraShakeType
    {
        Landing,
        Damage,
        Explosion,
        Impact
    }
}