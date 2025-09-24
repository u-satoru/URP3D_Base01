using UnityEngine;

namespace asterivo.Unity60.Features.Templates.FPS.Configuration
{
    /// <summary>
    /// FPS Template用移動設定
    /// プレイヤーの移動速度、ジャンプ、クラウチ等の設定を管理
    /// </summary>
    [CreateAssetMenu(menuName = "FPS/Configuration/Movement Config")]
    public class FPSMovementConfig : ScriptableObject
    {
        [Header("Basic Movement")]
        [SerializeField] private float _walkSpeed = 5f;
        [SerializeField] private float _runSpeed = 8f;
        [SerializeField] private float _crouchSpeed = 2.5f;
        [SerializeField] private float _acceleration = 10f;
        [SerializeField] private float _deceleration = 15f;

        [Header("Jump Settings")]
        [SerializeField] private float _jumpHeight = 1.2f;
        [SerializeField] private float _gravity = -9.81f;
        [SerializeField] private float _fallMultiplier = 2.5f;
        [SerializeField] private float _lowJumpMultiplier = 2f;

        [Header("Air Movement")]
        [SerializeField] private float _airControl = 0.5f;
        [SerializeField] private float _coyoteTime = 0.1f;
        [SerializeField] private float _jumpBufferTime = 0.1f;

        [Header("Ground Detection")]
        [SerializeField] private float _groundCheckDistance = 0.1f;
        [SerializeField] private LayerMask _groundLayerMask = -1;

        [Header("Stamina")]
        [SerializeField] private bool _enableStamina = true;
        [SerializeField] private float _maxStamina = 100f;
        [SerializeField] private float _staminaDrainRate = 20f;
        [SerializeField] private float _staminaRecoveryRate = 15f;

        // Properties
        public float WalkSpeed => _walkSpeed;
        public float RunSpeed => _runSpeed;
        public float CrouchSpeed => _crouchSpeed;
        public float Acceleration => _acceleration;
        public float Deceleration => _deceleration;
        public float JumpHeight => _jumpHeight;
        public float Gravity => _gravity;
        public float FallMultiplier => _fallMultiplier;
        public float LowJumpMultiplier => _lowJumpMultiplier;
        public float AirControl => _airControl;
        public float CoyoteTime => _coyoteTime;
        public float JumpBufferTime => _jumpBufferTime;
        public float GroundCheckDistance => _groundCheckDistance;
        public LayerMask GroundLayerMask => _groundLayerMask;
        public bool EnableStamina => _enableStamina;
        public float MaxStamina => _maxStamina;
        public float StaminaDrainRate => _staminaDrainRate;
        public float StaminaRecoveryRate => _staminaRecoveryRate;

        /// <summary>
        /// ジャンプに必要な速度を計算
        /// </summary>
        public float GetJumpVelocity()
        {
            return Mathf.Sqrt(-2f * _gravity * _jumpHeight);
        }
    }
}