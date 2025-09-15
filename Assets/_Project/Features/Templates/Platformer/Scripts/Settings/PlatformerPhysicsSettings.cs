using UnityEngine;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Features.Templates.Platformer
{
    /// <summary>
    /// プラットフォーマー物理設定クラス
    /// 15分ゲームプレイ最適化物理パラメータ管理
    /// </summary>
    [System.Serializable]
    public class PlatformerPhysicsSettings
    {
        [BoxGroup("Basic Physics")]
        [LabelText("重力（-9.81f標準）"), Range(-30f, 0f)]
        [SerializeField] private float gravity = -9.81f;

        [LabelText("ジャンプ力"), Range(5f, 20f)]
        [SerializeField] private float jumpForce = 12f;

        [LabelText("最大ジャンプ回数"), Range(1, 3)]
        [SerializeField] private int maxJumps = 2;

        [BoxGroup("Advanced Physics")]
        [LabelText("落下時重力倍率"), Range(1f, 5f)]
        [SerializeField] private float fallGravityMultiplier = 2.5f;

        [LabelText("低ジャンプ時重力倍率"), Range(1f, 5f)]
        [SerializeField] private float lowJumpGravityMultiplier = 2f;

        [LabelText("ターミナル速度"), Range(10f, 50f)]
        [SerializeField] private float terminalVelocity = 30f;

        [BoxGroup("Coyote Time & Jump Buffer")]
        [LabelText("コヨーテタイム有効")]
        [SerializeField] private bool enableCoyoteTime = true;

        [LabelText("コヨーテタイム時間"), Range(0.05f, 0.5f)]
        [ShowIf("enableCoyoteTime")]
        [SerializeField] private float coyoteTime = 0.15f;

        [LabelText("ジャンプバッファ有効")]
        [SerializeField] private bool enableJumpBuffer = true;

        [LabelText("ジャンプバッファ時間"), Range(0.05f, 0.5f)]
        [ShowIf("enableJumpBuffer")]
        [SerializeField] private float jumpBufferTime = 0.2f;

        [BoxGroup("Wall Physics")]
        [LabelText("壁ジャンプ有効")]
        [SerializeField] private bool enableWallJump = true;

        [LabelText("壁ジャンプ力"), Range(5f, 20f)]
        [ShowIf("enableWallJump")]
        [SerializeField] private float wallJumpForce = 10f;

        [LabelText("壁滑り有効")]
        [SerializeField] private bool enableWallSlide = true;

        [LabelText("壁滑り速度"), Range(1f, 10f)]
        [ShowIf("enableWallSlide")]
        [SerializeField] private float wallSlideSpeed = 3f;

        [BoxGroup("Ground Detection")]
        [LabelText("地面レイヤーマスク")]
        [SerializeField] private LayerMask groundLayerMask = 1;

        [LabelText("地面検知半径"), Range(0.1f, 1f)]
        [SerializeField] private float groundCheckRadius = 0.3f;

        [LabelText("地面検知オフセット")]
        [SerializeField] private Vector2 groundCheckOffset = new Vector2(0f, -1f);

        [BoxGroup("Optimization")]
        [LabelText("物理更新頻度"), Range(30, 120)]
        [SerializeField] private int physicsUpdateRate = 60;

        [LabelText("睡眠しきい値"), Range(0.1f, 2f)]
        [SerializeField] private float sleepThreshold = 0.5f;

        #region Public Properties
        public float Gravity => gravity;
        public float JumpForce => jumpForce;
        public int MaxJumps => maxJumps;
        public float FallGravityMultiplier => fallGravityMultiplier;
        public float LowJumpGravityMultiplier => lowJumpGravityMultiplier;
        public float TerminalVelocity => terminalVelocity;
        public bool EnableCoyoteTime => enableCoyoteTime;
        public float CoyoteTime => coyoteTime;
        public bool EnableJumpBuffer => enableJumpBuffer;
        public float JumpBufferTime => jumpBufferTime;
        public bool EnableWallJump => enableWallJump;
        public float WallJumpForce => wallJumpForce;
        public bool EnableWallSlide => enableWallSlide;
        public float WallSlideSpeed => wallSlideSpeed;
        public LayerMask GroundLayerMask => groundLayerMask;
        public float GroundCheckRadius => groundCheckRadius;
        public Vector2 GroundCheckOffset => groundCheckOffset;
        public int PhysicsUpdateRate => physicsUpdateRate;
        public float SleepThreshold => sleepThreshold;
        #endregion

        #region Initialization & Validation
        public void Initialize()
        {
            // 物理設定の妥当性確認
            gravity = Mathf.Clamp(gravity, -30f, 0f);
            jumpForce = Mathf.Clamp(jumpForce, 5f, 20f);
            maxJumps = Mathf.Clamp(maxJumps, 1, 3);

            Debug.Log($"[PlatformerPhysics] Initialized: Gravity={gravity}, Jump={jumpForce}, MaxJumps={maxJumps}");
        }

        public bool Validate()
        {
            bool isValid = true;

            // 重力検証
            if (gravity >= 0)
            {
                Debug.LogError("[PlatformerPhysics] Gravity must be negative");
                isValid = false;
            }

            // ジャンプ力検証
            if (jumpForce <= 0)
            {
                Debug.LogError("[PlatformerPhysics] Jump force must be positive");
                isValid = false;
            }

            // 落下重力倍率検証
            if (fallGravityMultiplier < 1f)
            {
                Debug.LogError("[PlatformerPhysics] Fall gravity multiplier must be >= 1");
                isValid = false;
            }

            // 時間設定検証
            if (enableCoyoteTime && (coyoteTime <= 0 || coyoteTime > 1f))
            {
                Debug.LogError("[PlatformerPhysics] Invalid coyote time duration");
                isValid = false;
            }

            if (enableJumpBuffer && (jumpBufferTime <= 0 || jumpBufferTime > 1f))
            {
                Debug.LogError("[PlatformerPhysics] Invalid jump buffer time duration");
                isValid = false;
            }

            return isValid;
        }

        public void ApplyRecommendedSettings()
        {
            // 15分ゲームプレイ最適化設定
            gravity = -12f;                    // やや軽快な重力
            jumpForce = 14f;                  // 気持ちよいジャンプ感
            maxJumps = 2;                     // ダブルジャンプ対応
            fallGravityMultiplier = 2.8f;      // 落下時の重量感
            lowJumpGravityMultiplier = 2.2f;   // 短押し調整
            terminalVelocity = 25f;           // 適度な落下上限

            enableCoyoteTime = true;
            coyoteTime = 0.12f;               // プレイヤーフレンドリー
            enableJumpBuffer = true;
            jumpBufferTime = 0.15f;           // 入力受付猶予

            enableWallJump = true;
            wallJumpForce = 12f;              // ジャンプと同等
            enableWallSlide = true;
            wallSlideSpeed = 2.5f;            // 制御可能な滑り

            groundCheckRadius = 0.25f;        // 適度な検知範囲
            groundCheckOffset = new Vector2(0f, -0.9f);

            physicsUpdateRate = 60;           // 標準更新頻度
            sleepThreshold = 0.3f;           // パフォーマンス最適化

            Debug.Log("[PlatformerPhysics] Applied recommended settings for 15-minute gameplay");
        }
        #endregion

        #region Physics Calculations
        /// <summary>
        /// 現在の重力値を計算（状況に応じた重力倍率適用）
        /// </summary>
        /// <param name="isRising">上昇中かどうか</param>
        /// <param name="isLowJump">低ジャンプかどうか</param>
        /// <returns>適用重力値</returns>
        public float CalculateCurrentGravity(bool isRising, bool isLowJump)
        {
            if (isRising && isLowJump)
            {
                return gravity * lowJumpGravityMultiplier;
            }
            else if (!isRising)
            {
                return gravity * fallGravityMultiplier;
            }
            return gravity;
        }

        /// <summary>
        /// ジャンプ高度の計算
        /// </summary>
        /// <param name="jumpNumber">ジャンプ回数（1-3）</param>
        /// <returns>予想ジャンプ高度</returns>
        public float CalculateJumpHeight(int jumpNumber = 1)
        {
            float effectiveJumpForce = jumpForce;

            // 複数回ジャンプの場合は若干減衰
            if (jumpNumber > 1)
            {
                effectiveJumpForce *= Mathf.Pow(0.9f, jumpNumber - 1);
            }

            // v² = u² + 2as より高度計算
            return (effectiveJumpForce * effectiveJumpForce) / (2 * Mathf.Abs(gravity));
        }

        /// <summary>
        /// 壁ジャンプベクトルの計算
        /// </summary>
        /// <param name="wallNormal">壁の法線ベクトル</param>
        /// <returns>壁ジャンプベクトル</returns>
        public Vector2 CalculateWallJumpVector(Vector2 wallNormal)
        {
            if (!enableWallJump) return Vector2.zero;

            // 壁から離れる方向 + 上方向
            Vector2 awayFromWall = wallNormal.normalized;
            Vector2 upward = Vector2.up;

            // 45度角度での壁ジャンプ
            Vector2 jumpDirection = (awayFromWall + upward).normalized;
            return jumpDirection * wallJumpForce;
        }
        #endregion

        #region Editor Support
#if UNITY_EDITOR
        [Button("Test Jump Height Calculation")]
        [PropertySpace(10)]
        public void TestJumpHeightCalculation()
        {
            for (int i = 1; i <= maxJumps; i++)
            {
                float height = CalculateJumpHeight(i);
                Debug.Log($"Jump {i}: Height = {height:F2}m");
            }
        }

        [Button("Validate Physics Settings")]
        public void EditorValidate()
        {
            bool isValid = Validate();
            string message = isValid ?
                "✅ Physics settings are valid!" :
                "❌ Physics settings validation failed!";
            Debug.Log($"[PlatformerPhysics] {message}");
        }
#endif
        #endregion
    }
}