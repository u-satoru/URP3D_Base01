using UnityEngine;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Features.Templates.Platformer
{
    /// <summary>
    /// プラットフォーマープレイヤー設定クラス
    /// プレイヤー制御・状態管理・移動パラメータ
    /// </summary>
    [System.Serializable]
    public class PlatformerPlayerSettings
    {
        [BoxGroup("Movement")]
        [LabelText("移動速度"), Range(3f, 15f)]
        [SerializeField] private float movementSpeed = 8f;

        [LabelText("空中移動倍率"), Range(0.3f, 1f)]
        [SerializeField] private float airMovementMultiplier = 0.8f;

        [LabelText("加速度"), Range(5f, 30f)]
        [SerializeField] private float acceleration = 15f;

        [LabelText("減速度"), Range(5f, 30f)]
        [SerializeField] private float deceleration = 20f;

        [BoxGroup("State Management")]
        [LabelText("ヘルスポイント"), Range(1, 10)]
        [SerializeField] private int maxHealth = 3;

        [LabelText("無敵時間"), Range(0.5f, 3f)]
        [SerializeField] private float invulnerabilityDuration = 1.5f;

        [LabelText("リスポーン時間"), Range(0.5f, 5f)]
        [SerializeField] private float respawnDelay = 2f;

        [BoxGroup("Advanced Movement")]
        [LabelText("ダッシュ有効")]
        [SerializeField] private bool enableDash = true;

        [LabelText("ダッシュ力"), Range(8f, 25f)]
        [ShowIf("enableDash")]
        [SerializeField] private float dashForce = 15f;

        [LabelText("ダッシュ継続時間"), Range(0.1f, 0.5f)]
        [ShowIf("enableDash")]
        [SerializeField] private float dashDuration = 0.2f;

        [LabelText("ダッシュクールダウン"), Range(0.5f, 3f)]
        [ShowIf("enableDash")]
        [SerializeField] private float dashCooldown = 1f;

        [BoxGroup("Ground Slam")]
        [LabelText("グラウンドスラム有効")]
        [SerializeField] private bool enableGroundSlam = true;

        [LabelText("スラム力"), Range(15f, 40f)]
        [ShowIf("enableGroundSlam")]
        [SerializeField] private float groundSlamForce = 25f;

        [LabelText("スラム範囲"), Range(1f, 5f)]
        [ShowIf("enableGroundSlam")]
        [SerializeField] private float groundSlamRadius = 2.5f;

        [BoxGroup("Animation")]
        [LabelText("アニメーション有効")]
        [SerializeField] private bool enableAnimations = true;

        [LabelText("アニメーション速度"), Range(0.5f, 2f)]
        [ShowIf("enableAnimations")]
        [SerializeField] private float animationSpeed = 1f;

        [LabelText("移動アニメーション閾値"), Range(0.1f, 1f)]
        [ShowIf("enableAnimations")]
        [SerializeField] private float movementAnimationThreshold = 0.3f;

        [BoxGroup("Input")]
        [LabelText("入力バッファサイズ"), Range(5, 20)]
        [SerializeField] private int inputBufferSize = 10;

        [LabelText("入力感度"), Range(0.1f, 2f)]
        [SerializeField] private float inputSensitivity = 1f;

        [LabelText("デッドゾーン"), Range(0.1f, 0.5f)]
        [SerializeField] private float inputDeadzone = 0.2f;

        [BoxGroup("Visual Effects")]
        [LabelText("移動時パーティクル有効")]
        [SerializeField] private bool enableMovementParticles = true;

        [LabelText("ジャンプエフェクト有効")]
        [SerializeField] private bool enableJumpEffects = true;

        [LabelText("着地エフェクト有効")]
        [SerializeField] private bool enableLandingEffects = true;

        [LabelText("ダメージエフェクト有効")]
        [SerializeField] private bool enableDamageEffects = true;

        [BoxGroup("Audio")]
        [LabelText("足音有効")]
        [SerializeField] private bool enableFootsteps = true;

        [LabelText("ジャンプ音有効")]
        [SerializeField] private bool enableJumpSounds = true;

        [LabelText("着地音有効")]
        [SerializeField] private bool enableLandingSounds = true;

        [LabelText("ダメージ音有効")]
        [SerializeField] private bool enableDamageSounds = true;

        #region Public Properties
        public float MovementSpeed => movementSpeed;
        public float AirMovementMultiplier => airMovementMultiplier;
        public float Acceleration => acceleration;
        public float Deceleration => deceleration;
        public int MaxHealth => maxHealth;
        public float InvulnerabilityDuration => invulnerabilityDuration;
        public float RespawnDelay => respawnDelay;
        public bool EnableDash => enableDash;
        public float DashForce => dashForce;
        public float DashDuration => dashDuration;
        public float DashCooldown => dashCooldown;
        public bool EnableGroundSlam => enableGroundSlam;
        public float GroundSlamForce => groundSlamForce;
        public float GroundSlamRadius => groundSlamRadius;
        public bool EnableAnimations => enableAnimations;
        public float AnimationSpeed => animationSpeed;
        public float MovementAnimationThreshold => movementAnimationThreshold;
        public int InputBufferSize => inputBufferSize;
        public float InputSensitivity => inputSensitivity;
        public float InputDeadzone => inputDeadzone;
        public bool EnableMovementParticles => enableMovementParticles;
        public bool EnableJumpEffects => enableJumpEffects;
        public bool EnableLandingEffects => enableLandingEffects;
        public bool EnableDamageEffects => enableDamageEffects;
        public bool EnableFootsteps => enableFootsteps;
        public bool EnableJumpSounds => enableJumpSounds;
        public bool EnableLandingSounds => enableLandingSounds;
        public bool EnableDamageSounds => enableDamageSounds;
        #endregion

        #region Initialization & Validation
        public void Initialize()
        {
            // プレイヤー設定の妥当性確認
            movementSpeed = Mathf.Clamp(movementSpeed, 3f, 15f);
            airMovementMultiplier = Mathf.Clamp01(airMovementMultiplier);
            acceleration = Mathf.Clamp(acceleration, 5f, 30f);
            deceleration = Mathf.Clamp(deceleration, 5f, 30f);
            maxHealth = Mathf.Clamp(maxHealth, 1, 10);

            Debug.Log($"[PlatformerPlayer] Initialized: Speed={movementSpeed}, Health={maxHealth}, Dash={enableDash}");
        }

        public bool Validate()
        {
            bool isValid = true;

            // 移動速度検証
            if (movementSpeed <= 0)
            {
                Debug.LogError("[PlatformerPlayer] Movement speed must be positive");
                isValid = false;
            }

            // 空中移動倍率検証
            if (airMovementMultiplier < 0.1f || airMovementMultiplier > 1f)
            {
                Debug.LogError("[PlatformerPlayer] Air movement multiplier must be between 0.1 and 1.0");
                isValid = false;
            }

            // ヘルス検証
            if (maxHealth <= 0)
            {
                Debug.LogError("[PlatformerPlayer] Max health must be positive");
                isValid = false;
            }

            // ダッシュ設定検証
            if (enableDash)
            {
                if (dashForce <= 0)
                {
                    Debug.LogError("[PlatformerPlayer] Dash force must be positive");
                    isValid = false;
                }

                if (dashDuration <= 0 || dashDuration > 1f)
                {
                    Debug.LogError("[PlatformerPlayer] Invalid dash duration");
                    isValid = false;
                }

                if (dashCooldown < 0)
                {
                    Debug.LogError("[PlatformerPlayer] Dash cooldown cannot be negative");
                    isValid = false;
                }
            }

            // グラウンドスラム設定検証
            if (enableGroundSlam)
            {
                if (groundSlamForce <= 0)
                {
                    Debug.LogError("[PlatformerPlayer] Ground slam force must be positive");
                    isValid = false;
                }

                if (groundSlamRadius <= 0)
                {
                    Debug.LogError("[PlatformerPlayer] Ground slam radius must be positive");
                    isValid = false;
                }
            }

            return isValid;
        }

        public void ApplyRecommendedSettings()
        {
            // 15分ゲームプレイ最適化設定
            movementSpeed = 9f;               // 快適な移動速度
            airMovementMultiplier = 0.85f;    // 空中制御性
            acceleration = 18f;               // レスポンシブな加速
            deceleration = 22f;               // 素早い停止

            maxHealth = 3;                    // 標準的なヘルス
            invulnerabilityDuration = 1.2f;   // 適度な無敵時間
            respawnDelay = 1.5f;              // ストレスフリーリスポーン

            enableDash = true;
            dashForce = 18f;                  // 気持ちよいダッシュ
            dashDuration = 0.15f;             // 短めの持続時間
            dashCooldown = 0.8f;              // 頻繁に使える

            enableGroundSlam = true;
            groundSlamForce = 28f;            // 強力なスラム
            groundSlamRadius = 2.8f;          // 適度な影響範囲

            enableAnimations = true;
            animationSpeed = 1.2f;            // やや早めのアニメーション
            movementAnimationThreshold = 0.2f; // 敏感な反応

            inputBufferSize = 12;             // 十分なバッファ
            inputSensitivity = 1.1f;          // やや高い感度
            inputDeadzone = 0.15f;            // 小さなデッドゾーン

            // 全エフェクト有効
            enableMovementParticles = true;
            enableJumpEffects = true;
            enableLandingEffects = true;
            enableDamageEffects = true;

            // 全サウンド有効
            enableFootsteps = true;
            enableJumpSounds = true;
            enableLandingSounds = true;
            enableDamageSounds = true;

            Debug.Log("[PlatformerPlayer] Applied recommended settings for engaging gameplay");
        }
        #endregion

        #region Movement Calculations
        /// <summary>
        /// 現在の移動速度を計算（空中・地上・状態考慮）
        /// </summary>
        /// <param name="isGrounded">地面にいるかどうか</param>
        /// <param name="isDashing">ダッシュ中かどうか</param>
        /// <returns>適用移動速度</returns>
        public float CalculateCurrentMovementSpeed(bool isGrounded, bool isDashing)
        {
            if (isDashing && enableDash)
            {
                return dashForce;
            }

            float baseSpeed = movementSpeed;
            if (!isGrounded)
            {
                baseSpeed *= airMovementMultiplier;
            }

            return baseSpeed;
        }

        /// <summary>
        /// 加速・減速カーブの計算
        /// </summary>
        /// <param name="currentVelocity">現在の速度</param>
        /// <param name="targetVelocity">目標速度</param>
        /// <param name="deltaTime">フレーム時間</param>
        /// <returns>新しい速度</returns>
        public float CalculateVelocityChange(float currentVelocity, float targetVelocity, float deltaTime)
        {
            float difference = targetVelocity - currentVelocity;
            float rate = Mathf.Abs(difference) > 0.01f ?
                (targetVelocity > currentVelocity ? acceleration : deceleration) : 0f;

            return Mathf.MoveTowards(currentVelocity, targetVelocity, rate * deltaTime);
        }

        /// <summary>
        /// ダッシュ可能性の確認
        /// </summary>
        /// <param name="lastDashTime">最後のダッシュ時間</param>
        /// <returns>ダッシュ可能かどうか</returns>
        public bool CanDash(float lastDashTime)
        {
            return enableDash && (Time.time - lastDashTime >= dashCooldown);
        }

        /// <summary>
        /// グラウンドスラム影響範囲の計算
        /// </summary>
        /// <param name="slamPosition">スラム位置</param>
        /// <param name="targetPosition">対象位置</param>
        /// <returns>影響度（0-1）</returns>
        public float CalculateGroundSlamImpact(Vector3 slamPosition, Vector3 targetPosition)
        {
            if (!enableGroundSlam) return 0f;

            float distance = Vector3.Distance(slamPosition, targetPosition);
            if (distance > groundSlamRadius) return 0f;

            // 距離に応じて減衰する影響度
            return 1f - (distance / groundSlamRadius);
        }
        #endregion

        #region Input Processing
        /// <summary>
        /// 入力値のフィルタリング（デッドゾーン適用）
        /// </summary>
        /// <param name="rawInput">生の入力値</param>
        /// <returns>フィルタ済み入力値</returns>
        public Vector2 ProcessInput(Vector2 rawInput)
        {
            // デッドゾーン適用
            if (rawInput.magnitude < inputDeadzone)
            {
                return Vector2.zero;
            }

            // 感度適用
            Vector2 processedInput = rawInput * inputSensitivity;

            // 正規化（必要に応じて）
            if (processedInput.magnitude > 1f)
            {
                processedInput = processedInput.normalized;
            }

            return processedInput;
        }

        /// <summary>
        /// アニメーション閾値チェック
        /// </summary>
        /// <param name="velocity">現在の速度</param>
        /// <returns>アニメーション再生すべきかどうか</returns>
        public bool ShouldPlayMovementAnimation(Vector2 velocity)
        {
            return enableAnimations && velocity.magnitude >= movementAnimationThreshold;
        }
        #endregion

        #region Editor Support
#if UNITY_EDITOR
        [Button("Test Movement Calculations")]
        [PropertySpace(10)]
        public void TestMovementCalculations()
        {
            Debug.Log($"Ground Speed: {CalculateCurrentMovementSpeed(true, false):F2}");
            Debug.Log($"Air Speed: {CalculateCurrentMovementSpeed(false, false):F2}");
            Debug.Log($"Dash Speed: {CalculateCurrentMovementSpeed(true, true):F2}");
        }

        [Button("Test Dash Cooldown")]
        public void TestDashCooldown()
        {
            float testTime = Time.time - dashCooldown + 0.1f;
            bool canDash = CanDash(testTime);
            Debug.Log($"Can Dash: {canDash} (Cooldown: {dashCooldown}s)");
        }

        [Button("Validate Player Settings")]
        public void EditorValidate()
        {
            bool isValid = Validate();
            string message = isValid ?
                "✅ Player settings are valid!" :
                "❌ Player settings validation failed!";
            Debug.Log($"[PlatformerPlayer] {message}");
        }
#endif
        #endregion
    }
}