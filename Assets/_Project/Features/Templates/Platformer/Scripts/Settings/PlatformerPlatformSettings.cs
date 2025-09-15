using UnityEngine;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Features.Templates.Platformer
{
    /// <summary>
    /// プラットフォーマー動的プラットフォーム設定クラス
    /// 移動床・落下床・回転プラットフォーム・エレベータ等の動的環境要素
    /// </summary>
    [System.Serializable]
    public class PlatformerPlatformSettings
    {
        [BoxGroup("Moving Platforms")]
        [LabelText("移動プラットフォーム有効")]
        [SerializeField] private bool enableMovingPlatforms = true;

        [LabelText("移動速度"), Range(1f, 10f)]
        [ShowIf("enableMovingPlatforms")]
        [SerializeField] private float movingSpeed = 3f;

        [LabelText("待機時間"), Range(0f, 5f)]
        [ShowIf("enableMovingPlatforms")]
        [SerializeField] private float waitTime = 1f;

        [LabelText("スムーズ移動")]
        [ShowIf("enableMovingPlatforms")]
        [SerializeField] private bool smoothMovement = true;

        [BoxGroup("Falling Platforms")]
        [LabelText("落下床有効")]
        [SerializeField] private bool enableFallingPlatforms = true;

        [LabelText("落下遅延時間"), Range(0.1f, 3f)]
        [ShowIf("enableFallingPlatforms")]
        [SerializeField] private float fallDelay = 0.8f;

        [LabelText("落下重力倍率"), Range(1f, 5f)]
        [ShowIf("enableFallingPlatforms")]
        [SerializeField] private float fallGravityMultiplier = 2.5f;

        [LabelText("リスポーン時間"), Range(3f, 10f)]
        [ShowIf("enableFallingPlatforms")]
        [SerializeField] private float respawnTime = 5f;

        [BoxGroup("Rotating Platforms")]
        [LabelText("回転プラットフォーム有効")]
        [SerializeField] private bool enableRotatingPlatforms = true;

        [LabelText("回転速度"), Range(10f, 180f)]
        [ShowIf("enableRotatingPlatforms")]
        [SerializeField] private float rotationSpeed = 45f;

        [LabelText("回転軸")]
        [ShowIf("enableRotatingPlatforms")]
        [SerializeField] private Vector3 rotationAxis = Vector3.up;

        [LabelText("一時停止有効")]
        [ShowIf("enableRotatingPlatforms")]
        [SerializeField] private bool enableRotationPause = false;

        [LabelText("停止間隔"), Range(2f, 8f)]
        [ShowIf("@enableRotatingPlatforms && enableRotationPause")]
        [SerializeField] private float pauseInterval = 4f;

        [BoxGroup("Elevator Platforms")]
        [LabelText("エレベータ有効")]
        [SerializeField] private bool enableElevators = true;

        [LabelText("上昇速度"), Range(1f, 8f)]
        [ShowIf("enableElevators")]
        [SerializeField] private float elevatorSpeed = 4f;

        [LabelText("各階停止時間"), Range(1f, 5f)]
        [ShowIf("enableElevators")]
        [SerializeField] private float floorStopTime = 2f;

        [LabelText("自動運転")]
        [ShowIf("enableElevators")]
        [SerializeField] private bool autoElevator = false;

        [BoxGroup("Jump Pads")]
        [LabelText("ジャンプパッド有効")]
        [SerializeField] private bool enableJumpPads = true;

        [LabelText("ジャンプ力"), Range(10f, 30f)]
        [ShowIf("enableJumpPads")]
        [SerializeField] private float jumpPadForce = 18f;

        [LabelText("方向制御")]
        [ShowIf("enableJumpPads")]
        [SerializeField] private bool allowDirectionalJump = true;

        [LabelText("エフェクト有効")]
        [ShowIf("enableJumpPads")]
        [SerializeField] private bool enableJumpPadEffects = true;

        [BoxGroup("Conveyor Belts")]
        [LabelText("コンベア有効")]
        [SerializeField] private bool enableConveyorBelts = true;

        [LabelText("ベルト速度"), Range(1f, 8f)]
        [ShowIf("enableConveyorBelts")]
        [SerializeField] private float conveyorSpeed = 3f;

        [LabelText("プレイヤー影響度"), Range(0.5f, 2f)]
        [ShowIf("enableConveyorBelts")]
        [SerializeField] private float playerInfluence = 1f;

        [LabelText("アニメーション有効")]
        [ShowIf("enableConveyorBelts")]
        [SerializeField] private bool enableConveyorAnimation = true;

        [BoxGroup("Pressure Plates")]
        [LabelText("圧力プレート有効")]
        [SerializeField] private bool enablePressurePlates = true;

        [LabelText("作動重量"), Range(1f, 100f)]
        [ShowIf("enablePressurePlates")]
        [SerializeField] private float activationWeight = 50f;

        [LabelText("作動遅延"), Range(0f, 2f)]
        [ShowIf("enablePressurePlates")]
        [SerializeField] private float activationDelay = 0.2f;

        [LabelText("リセット時間"), Range(0.5f, 5f)]
        [ShowIf("enablePressurePlates")]
        [SerializeField] private float resetTime = 1f;

        [BoxGroup("Performance")]
        [LabelText("最大同時動作数"), Range(5, 50)]
        [SerializeField] private int maxActivePlatforms = 20;

        [LabelText("距離カリング"), Range(20f, 100f)]
        [SerializeField] private float cullingDistance = 50f;

        [LabelText("LOD有効")]
        [SerializeField] private bool enableLOD = true;

        #region Public Properties
        public bool EnableMovingPlatforms => enableMovingPlatforms;
        public float MovingSpeed => movingSpeed;
        public float WaitTime => waitTime;
        public bool SmoothMovement => smoothMovement;
        public bool EnableFallingPlatforms => enableFallingPlatforms;
        public float FallDelay => fallDelay;
        public float FallGravityMultiplier => fallGravityMultiplier;
        public float RespawnTime => respawnTime;
        public bool EnableRotatingPlatforms => enableRotatingPlatforms;
        public float RotationSpeed => rotationSpeed;
        public Vector3 RotationAxis => rotationAxis;
        public bool EnableRotationPause => enableRotationPause;
        public float PauseInterval => pauseInterval;
        public bool EnableElevators => enableElevators;
        public float ElevatorSpeed => elevatorSpeed;
        public float FloorStopTime => floorStopTime;
        public bool AutoElevator => autoElevator;
        public bool EnableJumpPads => enableJumpPads;
        public float JumpPadForce => jumpPadForce;
        public bool AllowDirectionalJump => allowDirectionalJump;
        public bool EnableJumpPadEffects => enableJumpPadEffects;
        public bool EnableConveyorBelts => enableConveyorBelts;
        public float ConveyorSpeed => conveyorSpeed;
        public float PlayerInfluence => playerInfluence;
        public bool EnableConveyorAnimation => enableConveyorAnimation;
        public bool EnablePressurePlates => enablePressurePlates;
        public float ActivationWeight => activationWeight;
        public float ActivationDelay => activationDelay;
        public float ResetTime => resetTime;
        public int MaxActivePlatforms => maxActivePlatforms;
        public float CullingDistance => cullingDistance;
        public bool EnableLOD => enableLOD;
        #endregion

        #region Initialization & Validation
        public void Initialize()
        {
            // プラットフォーム設定の妥当性確認
            movingSpeed = Mathf.Clamp(movingSpeed, 1f, 10f);
            fallDelay = Mathf.Clamp(fallDelay, 0.1f, 3f);
            rotationSpeed = Mathf.Clamp(rotationSpeed, 10f, 180f);
            jumpPadForce = Mathf.Clamp(jumpPadForce, 10f, 30f);

            Debug.Log($"[PlatformerPlatforms] Initialized: Moving={enableMovingPlatforms}, Falling={enableFallingPlatforms}, Rotating={enableRotatingPlatforms}");
        }

        public bool Validate()
        {
            bool isValid = true;

            // 移動速度検証
            if (enableMovingPlatforms && movingSpeed <= 0)
            {
                Debug.LogError("[PlatformerPlatforms] Moving speed must be positive");
                isValid = false;
            }

            // 落下設定検証
            if (enableFallingPlatforms && (fallDelay <= 0 || respawnTime <= 0))
            {
                Debug.LogError("[PlatformerPlatforms] Fall delay and respawn time must be positive");
                isValid = false;
            }

            // 回転軸検証
            if (enableRotatingPlatforms && rotationAxis.magnitude < 0.1f)
            {
                Debug.LogError("[PlatformerPlatforms] Rotation axis must be non-zero");
                isValid = false;
            }

            // ジャンプパッド検証
            if (enableJumpPads && jumpPadForce <= 0)
            {
                Debug.LogError("[PlatformerPlatforms] Jump pad force must be positive");
                isValid = false;
            }

            // パフォーマンス設定検証
            if (maxActivePlatforms < 1)
            {
                Debug.LogError("[PlatformerPlatforms] Max active platforms must be at least 1");
                isValid = false;
            }

            return isValid;
        }

        public void ApplyRecommendedSettings()
        {
            // 15分ゲームプレイ最適化設定
            enableMovingPlatforms = true;
            movingSpeed = 4f;                  // 適度な移動速度
            waitTime = 1.2f;                   // 適切な待機時間
            smoothMovement = true;             // スムーズな動き

            enableFallingPlatforms = true;
            fallDelay = 1f;                    // 適度な緊張感
            fallGravityMultiplier = 3f;        // しっかりとした落下感
            respawnTime = 4f;                  // 適度なリトライ時間

            enableRotatingPlatforms = true;
            rotationSpeed = 60f;               // 予測可能な回転
            rotationAxis = Vector3.up;         // 標準的な回転軸
            enableRotationPause = true;        // タイミング調整用
            pauseInterval = 3f;                // 適度な停止間隔

            enableElevators = true;
            elevatorSpeed = 5f;                // 快適な上昇速度
            floorStopTime = 1.5f;              // 適度な停止時間
            autoElevator = false;              // プレイヤー制御重視

            enableJumpPads = true;
            jumpPadForce = 20f;                // 気持ちよいジャンプ
            allowDirectionalJump = true;       // 柔軟性重視
            enableJumpPadEffects = true;       // 視覚的フィードバック

            enableConveyorBelts = true;
            conveyorSpeed = 4f;                // 適度な流速
            playerInfluence = 1.2f;            // プレイヤーに優しい設定
            enableConveyorAnimation = true;    // 視覚的理解促進

            enablePressurePlates = true;
            activationWeight = 60f;            // プレイヤー体重想定
            activationDelay = 0.1f;            // 即座反応
            resetTime = 0.8f;                  // 適度なリセット時間

            // パフォーマンス最適化
            maxActivePlatforms = 15;           // 15分プレイ想定
            cullingDistance = 40f;             // 適度な描画範囲
            enableLOD = true;                  // パフォーマンス重視

            Debug.Log("[PlatformerPlatforms] Applied recommended settings for engaging 15-minute gameplay");
        }
        #endregion

        #region Platform Calculations
        /// <summary>
        /// 移動プラットフォームの位置計算
        /// </summary>
        /// <param name="startPos">開始位置</param>
        /// <param name="endPos">終了位置</param>
        /// <param name="time">経過時間</param>
        /// <returns>現在位置</returns>
        public Vector3 CalculateMovingPlatformPosition(Vector3 startPos, Vector3 endPos, float time)
        {
            if (!enableMovingPlatforms) return startPos;

            float totalDistance = Vector3.Distance(startPos, endPos);
            float travelTime = totalDistance / movingSpeed;
            float totalCycleTime = (travelTime * 2) + (waitTime * 2);

            float normalizedTime = (time % totalCycleTime) / totalCycleTime;

            if (normalizedTime < waitTime / totalCycleTime)
            {
                // 開始地点での待機
                return startPos;
            }
            else if (normalizedTime < (waitTime + travelTime) / totalCycleTime)
            {
                // 開始→終了への移動
                float moveProgress = (normalizedTime - (waitTime / totalCycleTime)) / (travelTime / totalCycleTime);
                if (smoothMovement)
                {
                    moveProgress = Mathf.SmoothStep(0f, 1f, moveProgress);
                }
                return Vector3.Lerp(startPos, endPos, moveProgress);
            }
            else if (normalizedTime < (waitTime * 2 + travelTime) / totalCycleTime)
            {
                // 終了地点での待機
                return endPos;
            }
            else
            {
                // 終了→開始への移動
                float moveProgress = (normalizedTime - ((waitTime * 2 + travelTime) / totalCycleTime)) / (travelTime / totalCycleTime);
                if (smoothMovement)
                {
                    moveProgress = Mathf.SmoothStep(0f, 1f, moveProgress);
                }
                return Vector3.Lerp(endPos, startPos, moveProgress);
            }
        }

        /// <summary>
        /// ジャンプパッドの力計算
        /// </summary>
        /// <param name="playerVelocity">プレイヤーの現在速度</param>
        /// <param name="padDirection">パッドの方向</param>
        /// <returns>適用する力</returns>
        public Vector3 CalculateJumpPadForce(Vector3 playerVelocity, Vector3 padDirection)
        {
            if (!enableJumpPads) return Vector3.zero;

            Vector3 baseForce = padDirection.normalized * jumpPadForce;

            if (allowDirectionalJump)
            {
                // プレイヤーの移動方向を考慮
                Vector3 horizontalVelocity = new Vector3(playerVelocity.x, 0, playerVelocity.z);
                Vector3 horizontalDirection = new Vector3(padDirection.x, 0, padDirection.z).normalized;

                // 移動方向が一致している場合はボーナスを追加
                float directionAlignment = Vector3.Dot(horizontalVelocity.normalized, horizontalDirection);
                if (directionAlignment > 0.5f)
                {
                    baseForce *= 1.2f; // 20%ボーナス
                }
            }

            return baseForce;
        }

        /// <summary>
        /// エレベータの現在階計算
        /// </summary>
        /// <param name="currentHeight">現在の高さ</param>
        /// <param name="floorHeights">各階の高さ配列</param>
        /// <returns>現在階とその階への到達度</returns>
        public (int floor, float progress) CalculateElevatorPosition(float currentHeight, float[] floorHeights)
        {
            if (!enableElevators || floorHeights == null || floorHeights.Length == 0)
            {
                return (0, 0f);
            }

            // 最も近い階を見つける
            int nearestFloor = 0;
            float nearestDistance = Mathf.Abs(currentHeight - floorHeights[0]);

            for (int i = 1; i < floorHeights.Length; i++)
            {
                float distance = Mathf.Abs(currentHeight - floorHeights[i]);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestFloor = i;
                }
            }

            // 進捗計算
            float progress = 0f;
            if (nearestFloor > 0)
            {
                float floorRange = floorHeights[nearestFloor] - floorHeights[nearestFloor - 1];
                float heightInRange = currentHeight - floorHeights[nearestFloor - 1];
                progress = Mathf.Clamp01(heightInRange / floorRange);
            }

            return (nearestFloor, progress);
        }

        /// <summary>
        /// コンベアベルトの影響力計算
        /// </summary>
        /// <param name="playerPosition">プレイヤー位置</param>
        /// <param name="beltDirection">ベルト方向</param>
        /// <param name="contactTime">接触時間</param>
        /// <returns>プレイヤーに適用する速度</returns>
        public Vector3 CalculateConveyorInfluence(Vector3 playerPosition, Vector3 beltDirection, float contactTime)
        {
            if (!enableConveyorBelts) return Vector3.zero;

            Vector3 baseInfluence = beltDirection.normalized * conveyorSpeed * playerInfluence;

            // 接触時間による影響度調整
            float timeMultiplier = Mathf.Clamp01(contactTime / 0.5f); // 0.5秒で最大効果
            baseInfluence *= timeMultiplier;

            return baseInfluence;
        }
        #endregion

        #region Editor Support
#if UNITY_EDITOR
        [Button("Test Platform Movement")]
        [PropertySpace(10)]
        public void TestPlatformMovement()
        {
            Vector3 start = Vector3.zero;
            Vector3 end = Vector3.right * 10f;
            Vector3 pos = CalculateMovingPlatformPosition(start, end, 5f);
            Debug.Log($"Platform position at 5s: {pos}");
        }

        [Button("Test Jump Pad Force")]
        public void TestJumpPadForce()
        {
            Vector3 playerVel = Vector3.forward * 5f;
            Vector3 padDir = (Vector3.up + Vector3.forward).normalized;
            Vector3 force = CalculateJumpPadForce(playerVel, padDir);
            Debug.Log($"Jump pad force: {force} (magnitude: {force.magnitude:F2})");
        }

        [Button("Validate Platform Settings")]
        public void EditorValidate()
        {
            bool isValid = Validate();
            string message = isValid ?
                "✅ Platform settings are valid!" :
                "❌ Platform settings validation failed!";
            Debug.Log($"[PlatformerPlatforms] {message}");
        }
#endif
        #endregion
    }
}