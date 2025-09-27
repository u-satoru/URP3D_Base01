using UnityEngine;

namespace asterivo.Unity60.Features.Templates.Platformer.Settings
{
    /// <summary>
    /// Platformer物理設定：Jump & Movement Physics システム
    /// ScriptableObject基盤：ノンプログラマーでもバランス調整可能
    /// ServiceLocator統合：物理演算の中央管理
    /// </summary>
    [CreateAssetMenu(fileName = "PlatformerPhysicsSettings", menuName = "Platformer Template/Settings/Physics Settings")]
    public class PlatformerPhysicsSettings : ScriptableObject
    {
        [Header("Jump Physics")]
        [SerializeField, Range(1f, 50f)] private float _jumpForce = 12f;
        [SerializeField, Range(0.1f, 5f)] private float _jumpHeight = 2.5f;
        [SerializeField, Range(0.1f, 2f)] private float _timeToJumpApex = 0.4f;
        [SerializeField, Range(1, 5)] private int _maxJumpCount = 2; // Double jump
        [SerializeField] private bool _variableJumpHeight = true;
        [SerializeField, Range(0.1f, 1f)] private float _jumpCutoff = 0.5f;

        [Header("Gravity & Fall")]
        [SerializeField, Range(0.5f, 10f)] private float _gravityScale = 3f;
        [SerializeField, Range(1f, 10f)] private float _fallMultiplier = 2.5f;
        [SerializeField, Range(1f, 5f)] private float _lowJumpMultiplier = 2f;
        [SerializeField, Range(5f, 50f)] private float _maxFallSpeed = 25f;

        [Header("Movement")]
        [SerializeField, Range(1f, 20f)] private float _moveSpeed = 8f;
        [SerializeField, Range(0.1f, 2f)] private float _acceleration = 1.2f;
        [SerializeField, Range(0.1f, 2f)] private float _deceleration = 1.5f;
        [SerializeField, Range(0f, 1f)] private float _airControl = 0.8f;

        [Header("Ground Detection")]
        [SerializeField, Range(0.01f, 1f)] private float _groundCheckDistance = 0.2f;
        [SerializeField, Range(0.01f, 0.5f)] private float _groundCheckRadius = 0.3f;
        [SerializeField] private LayerMask _groundLayerMask = 1;
        [SerializeField, Range(0f, 1f)] private float _coyoteTime = 0.2f; // Grace period after leaving ground
        [SerializeField, Range(0f, 1f)] private float _jumpBufferTime = 0.2f; // Early jump input buffer

        [Header("Wall Physics")]
        [SerializeField] private bool _enableWallJump = true;
        [SerializeField, Range(1f, 20f)] private float _wallJumpForce = 10f;
        [SerializeField, Range(0f, 2f)] private float _wallSlideSpeed = 2f;
        [SerializeField, Range(0.01f, 1f)] private float _wallCheckDistance = 0.6f;
        [SerializeField] private LayerMask _wallLayerMask = 1;

        [Header("Performance Settings")]
        [SerializeField, Range(10, 120)] private int _physicsUpdateRate = 60; // FPS for physics
        [SerializeField] private bool _enablePhysicsOptimization = true;
        [SerializeField] private bool _useFixedDeltaTime = true;

        [Header("Debug Settings")]
        [SerializeField] private bool _showDebugGizmos = true;
        [SerializeField] private Color _groundCheckColor = Color.green;
        [SerializeField] private Color _wallCheckColor = Color.red;

        // 計算済みプロパティ（パフォーマンス最適化）
        private float _calculatedGravity;
        private float _calculatedJumpVelocity;
        private bool _isDirty = true;

        // プロパティ公開（ServiceLocator経由アクセス）
        public float JumpForce => _jumpForce;
        public float JumpHeight => _jumpHeight;
        public float TimeToJumpApex => _timeToJumpApex;
        public int MaxJumpCount => _maxJumpCount;
        public bool VariableJumpHeight => _variableJumpHeight;
        public float JumpCutoff => _jumpCutoff;

        public float GravityScale => _gravityScale;
        public float FallMultiplier => _fallMultiplier;
        public float LowJumpMultiplier => _lowJumpMultiplier;
        public float MaxFallSpeed => _maxFallSpeed;

        public float MoveSpeed => _moveSpeed;
        public float Acceleration => _acceleration;
        public float Deceleration => _deceleration;
        public float AirControl => _airControl;

        public float GroundCheckDistance => _groundCheckDistance;
        public float GroundCheckRadius => _groundCheckRadius;
        public LayerMask GroundLayerMask => _groundLayerMask;
        public float CoyoteTime => _coyoteTime;
        public float JumpBufferTime => _jumpBufferTime;

        public bool EnableWallJump => _enableWallJump;
        public float WallJumpForce => _wallJumpForce;
        public float WallSlideSpeed => _wallSlideSpeed;
        public float WallCheckDistance => _wallCheckDistance;
        public LayerMask WallLayerMask => _wallLayerMask;

        public int PhysicsUpdateRate => _physicsUpdateRate;
        public bool EnablePhysicsOptimization => _enablePhysicsOptimization;
        public bool UseFixedDeltaTime => _useFixedDeltaTime;

        public bool ShowDebugGizmos => _showDebugGizmos;
        public Color GroundCheckColor => _groundCheckColor;
        public Color WallCheckColor => _wallCheckColor;

        /// <summary>
        /// 物理演算計算済みプロパティ：パフォーマンス最適化
        /// リアルタイム計算回避のため事前計算結果を使用
        /// </summary>
        public float CalculatedGravity
        {
            get
            {
                if (_isDirty) RecalculatePhysics();
                return _calculatedGravity;
            }
        }

        public float CalculatedJumpVelocity
        {
            get
            {
                if (_isDirty) RecalculatePhysics();
                return _calculatedJumpVelocity;
            }
        }

        /// <summary>
        /// 物理演算値の事前計算：60FPS安定動作保証
        /// </summary>
        private void RecalculatePhysics()
        {
            // gravity = 2 * jumpHeight / timeToJumpApex^2
            _calculatedGravity = -(2 * _jumpHeight) / Mathf.Pow(_timeToJumpApex, 2);

            // jumpVelocity = gravity * timeToJumpApex
            _calculatedJumpVelocity = Mathf.Abs(_calculatedGravity) * _timeToJumpApex;

            _isDirty = false;
        }

        /// <summary>
        /// 設定値検証：起動時整合性確認
        /// </summary>
        public bool ValidateSettings()
        {
            bool isValid = true;

            // Jump設定の妥当性
            if (_jumpForce <= 0 || _jumpHeight <= 0 || _timeToJumpApex <= 0)
            {
                Debug.LogError("Invalid jump settings: Force, Height, and TimeToApex must be positive.");
                isValid = false;
            }

            // Movement設定の妥当性
            if (_moveSpeed <= 0 || _acceleration <= 0 || _deceleration <= 0)
            {
                Debug.LogError("Invalid movement settings: Speed, Acceleration, and Deceleration must be positive.");
                isValid = false;
            }

            // 物理演算値の再計算
            if (isValid)
            {
                RecalculatePhysics();
            }

            return isValid;
        }

        /// <summary>
        /// デフォルト設定：Learn & Grow価値実現
        /// 初心者向け推奨値の適用
        /// </summary>
        public void SetToDefault()
        {
            _jumpForce = 12f;
            _jumpHeight = 2.5f;
            _timeToJumpApex = 0.4f;
            _maxJumpCount = 2;
            _variableJumpHeight = true;
            _jumpCutoff = 0.5f;

            _gravityScale = 3f;
            _fallMultiplier = 2.5f;
            _lowJumpMultiplier = 2f;
            _maxFallSpeed = 25f;

            _moveSpeed = 8f;
            _acceleration = 1.2f;
            _deceleration = 1.5f;
            _airControl = 0.8f;

            _groundCheckDistance = 0.2f;
            _groundCheckRadius = 0.3f;
            _groundLayerMask = 1;
            _coyoteTime = 0.2f;
            _jumpBufferTime = 0.2f;

            _enableWallJump = true;
            _wallJumpForce = 10f;
            _wallSlideSpeed = 2f;
            _wallCheckDistance = 0.6f;
            _wallLayerMask = 1;

            _physicsUpdateRate = 60;
            _enablePhysicsOptimization = true;
            _useFixedDeltaTime = true;

            _showDebugGizmos = true;
            _groundCheckColor = Color.green;
            _wallCheckColor = Color.red;

            _isDirty = true;
            Debug.Log("PlatformerPhysicsSettings set to default values.");
        }

        /// <summary>
        /// パフォーマンス最適化設定：60FPS安定動作
        /// </summary>
        public void OptimizeForPerformance()
        {
            _physicsUpdateRate = 60;
            _enablePhysicsOptimization = true;
            _useFixedDeltaTime = true;
            _showDebugGizmos = false; // デバッグ描画無効化

            // 計算処理軽減設定
            _groundCheckDistance = 0.15f; // 少し短縮
            _wallCheckDistance = 0.5f; // 少し短縮

            _isDirty = true;
            Debug.Log("PlatformerPhysicsSettings optimized for 60FPS performance.");
        }

        /// <summary>
        /// 学習向け設定：わかりやすい物理挙動
        /// </summary>
        public void OptimizeForLearning()
        {
            // より予測しやすい物理挙動
            _jumpHeight = 3f; // 少し高く
            _timeToJumpApex = 0.5f; // 少しゆっくり
            _fallMultiplier = 2f; // 落下をマイルドに
            _airControl = 1f; // 空中制御を完全に

            // 初心者に優しい設定
            _coyoteTime = 0.3f; // 猶予時間延長
            _jumpBufferTime = 0.3f; // 入力バッファ延長
            _variableJumpHeight = true; // 可変ジャンプ有効

            _showDebugGizmos = true; // 学習用ビジュアル有効

            _isDirty = true;
            Debug.Log("PlatformerPhysicsSettings optimized for learning experience.");
        }

        /// <summary>
        /// 値変更時の自動再計算トリガー
        /// </summary>
        private void OnValidate()
        {
            _isDirty = true;
        }

#if UNITY_EDITOR
        /// <summary>
        /// エディタ用デバッグ情報表示
        /// </summary>
        [ContextMenu("Show Calculated Values")]
        private void ShowCalculatedValues()
        {
            RecalculatePhysics();
            Debug.Log("=== Calculated Physics Values ===");
            Debug.Log($"Calculated Gravity: {_calculatedGravity:F2}");
            Debug.Log($"Calculated Jump Velocity: {_calculatedJumpVelocity:F2}");
            Debug.Log($"Max Jump Height: {_jumpHeight:F2}m");
            Debug.Log($"Time to Apex: {_timeToJumpApex:F2}s");
        }
#endif
    }
}
