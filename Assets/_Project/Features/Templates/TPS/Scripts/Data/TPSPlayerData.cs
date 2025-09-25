using UnityEngine;

namespace asterivo.Unity60.Features.Templates.TPS.Data
{
    /// <summary>
    /// ScriptableObject containing all TPS player configuration data
    /// Follows the project's data-driven design principles using ScriptableObjects
    /// </summary>
    [CreateAssetMenu(fileName = "New TPS Player Data", menuName = "TPS Template/Player Data")]
    public class TPSPlayerData : ScriptableObject
    {
        [Header("=== Movement Settings ===")]
        [Space(5)]
        
        [SerializeField] private float _walkSpeed = 3.5f;
        [SerializeField] private float _runSpeed = 6.0f;
        [SerializeField] private float _crouchSpeed = 1.5f;
        [SerializeField] private float _crouchHeight = 1.0f;
        [SerializeField] private float _aimWalkSpeed = 2.0f;
        [SerializeField] private float _airControlSpeed = 2.0f;
        
        [Header("=== Jump & Physics ===")]
        [Space(5)]
        
        [SerializeField] private float _jumpHeight = 2.0f;
        [SerializeField] private float _jumpForce = 8.0f;
        [SerializeField] private float _gravity = -15.0f;
        [SerializeField] private float _groundCheckDistance = 0.1f;
        [SerializeField] private LayerMask _groundLayerMask = -1;
        [SerializeField] private float _fallDamageThreshold = -10.0f;
        [SerializeField] private float _fallDamageMultiplier = 5.0f;
        [SerializeField] private float _landingDuration = 0.3f;
        
        [Header("=== Rotation & Look ===")]
        [Space(5)]
        
        [SerializeField] private float _rotationSpeed = 10.0f;
        [SerializeField] private float _lookSensitivity = 2.0f;
        [SerializeField] private float _aimSensitivity = 1.0f;
        
        [Header("=== Combat Settings ===")]
        [Space(5)]
        
        [SerializeField] private float _fireRate = 600f; // rounds per minute
        [SerializeField] private float _reloadTime = 2.5f;
        [SerializeField] private float _recoilAmount = 1.0f;
        [SerializeField] private float _aimDownSightTime = 0.3f;
        
        [Header("=== Melee Combat ===")]
        [Space(5)]
        
        [SerializeField] private float _meleeAttackDamage = 25f;
        [SerializeField] private float _meleeAttackRange = 1.5f;
        [SerializeField] private float _meleeAttackDuration = 0.8f;
        [SerializeField] private float _meleeAttackCooldown = 1.0f;
        
        [Header("=== Health & Damage ===")]
        [Space(5)]
        
        [SerializeField] private float _maxHealth = 100f;
        [SerializeField] private float _healthRegenRate = 5f; // health per second
        [SerializeField] private float _healthRegenDelay = 3f; // delay after taking damage
        [SerializeField] private float _damageReduction = 0.1f; // 10% damage reduction
        [SerializeField] private float _damageReactionDuration = 0.5f;
        [SerializeField] private float _maxKnockbackForce = 5.0f;
        [SerializeField] private float _autoRespawnTime = 0f; // 0 = manual respawn only
        
        [Header("=== Cover System ===")]
        [Space(5)]
        
        [SerializeField] private float _coverDetectionRange = 1.5f;
        [SerializeField] private float _coverTransitionSpeed = 5.0f;
        [SerializeField] private LayerMask _coverLayerMask = -1;
        
        [Header("=== Audio Settings ===")]
        [Space(5)]
        
        [SerializeField] private float _footstepVolume = 0.5f;
        [SerializeField] private float _footstepPitch = 1.0f;
        [SerializeField] private float _walkFootstepInterval = 0.5f;
        [SerializeField] private float _runFootstepInterval = 0.3f;
        [SerializeField] private float _crouchFootstepInterval = 0.8f;
        [SerializeField] private float _breathingVolume = 0.3f;
        [SerializeField] private bool _enableVoiceLines = true;
        
        [Header("=== Animation Settings ===")]
        [Space(5)]
        
        [SerializeField] private float _animationBlendSpeed = 5.0f;
        [SerializeField] private float _aimAnimationSpeed = 3.0f;
        [SerializeField] private bool _useRootMotion = false;

        // === Properties for easy access ===
        
        // Movement Properties
        public float WalkSpeed => _walkSpeed;
        public float RunSpeed => _runSpeed;
        public float CrouchSpeed => _crouchSpeed;
        public float CrouchHeight => _crouchHeight;
        public float AimWalkSpeed => _aimWalkSpeed;
        public float AirControlSpeed => _airControlSpeed;
        
        // Jump & Physics Properties
        public float JumpHeight => _jumpHeight;
        public float JumpForce => _jumpForce;
        public float Gravity => _gravity;
        public float GroundCheckDistance => _groundCheckDistance;
        public LayerMask GroundLayerMask => _groundLayerMask;
        public float FallDamageThreshold => _fallDamageThreshold;
        public float FallDamageMultiplier => _fallDamageMultiplier;
        public float LandingDuration => _landingDuration;
        
        // Rotation & Look Properties
        public float RotationSpeed => _rotationSpeed;
        public float LookSensitivity => _lookSensitivity;
        public float AimSensitivity => _aimSensitivity;
        
        // Combat Properties
        public float FireRate => _fireRate;
        public float ReloadTime => _reloadTime;
        public float RecoilAmount => _recoilAmount;
        public float AimDownSightTime => _aimDownSightTime;
        
        // Melee Combat Properties
        public float MeleeAttackDamage => _meleeAttackDamage;
        public float MeleeAttackRange => _meleeAttackRange;
        public float MeleeAttackDuration => _meleeAttackDuration;
        public float MeleeAttackCooldown => _meleeAttackCooldown;
        
        // Health & Damage Properties
        public float MaxHealth => _maxHealth;
        public float HealthRegenRate => _healthRegenRate;
        public float HealthRegenDelay => _healthRegenDelay;
        public float DamageReduction => _damageReduction;
        public float DamageReactionDuration => _damageReactionDuration;
        public float MaxKnockbackForce => _maxKnockbackForce;
        public float AutoRespawnTime => _autoRespawnTime;
        
        // Cover System Properties
        public float CoverDetectionRange => _coverDetectionRange;
        public float CoverTransitionSpeed => _coverTransitionSpeed;
        public LayerMask CoverLayerMask => _coverLayerMask;
        
        // Audio Properties
        public float FootstepVolume => _footstepVolume;
        public float FootstepPitch => _footstepPitch;
        public float WalkFootstepInterval => _walkFootstepInterval;
        public float RunFootstepInterval => _runFootstepInterval;
        public float CrouchFootstepInterval => _crouchFootstepInterval;
        public float BreathingVolume => _breathingVolume;
        public bool EnableVoiceLines => _enableVoiceLines;
        
        // Animation Properties
        public float AnimationBlendSpeed => _animationBlendSpeed;
        public float AimAnimationSpeed => _aimAnimationSpeed;
        public bool UseRootMotion => _useRootMotion;
        
        /// <summary>
        /// Calculate actual fire rate in shots per second
        /// </summary>
        public float FireRatePerSecond => _fireRate / 60f;
        
        /// <summary>
        /// Calculate time between shots
        /// </summary>
        public float TimeBetweenShots => 60f / _fireRate;
        
        /// <summary>
        /// Calculate jump velocity needed for desired jump height
        /// </summary>
        public float JumpVelocity => Mathf.Sqrt(_jumpHeight * -2f * _gravity);
        
        /// <summary>
        /// Validate configuration values
        /// </summary>
        private void OnValidate()
        {
            // Ensure speeds are positive
            _walkSpeed = Mathf.Max(0.1f, _walkSpeed);
            _runSpeed = Mathf.Max(_walkSpeed, _runSpeed);
            _crouchSpeed = Mathf.Max(0.1f, _crouchSpeed);
            _crouchHeight = Mathf.Max(0.1f, _crouchHeight);
            _aimWalkSpeed = Mathf.Max(0.1f, _aimWalkSpeed);
            _airControlSpeed = Mathf.Max(0.1f, _airControlSpeed);
            
            // Ensure physics values are reasonable
            _jumpHeight = Mathf.Max(0.1f, _jumpHeight);
            _gravity = Mathf.Min(-1f, _gravity);
            _groundCheckDistance = Mathf.Max(0.01f, _groundCheckDistance);
            
            // Ensure combat values are reasonable
            _fireRate = Mathf.Max(1f, _fireRate);
            _reloadTime = Mathf.Max(0.1f, _reloadTime);
            _recoilAmount = Mathf.Max(0f, _recoilAmount);
            
            // Ensure health values are positive
            _maxHealth = Mathf.Max(1f, _maxHealth);
            _healthRegenRate = Mathf.Max(0f, _healthRegenRate);
            _healthRegenDelay = Mathf.Max(0f, _healthRegenDelay);
            _damageReduction = Mathf.Clamp01(_damageReduction);
            
            // Ensure audio values are reasonable
            _footstepVolume = Mathf.Clamp01(_footstepVolume);
            _breathingVolume = Mathf.Clamp01(_breathingVolume);
            _footstepPitch = Mathf.Max(0.1f, _footstepPitch);
            _walkFootstepInterval = Mathf.Max(0.1f, _walkFootstepInterval);
            _runFootstepInterval = Mathf.Max(0.1f, _runFootstepInterval);
            _crouchFootstepInterval = Mathf.Max(0.1f, _crouchFootstepInterval);
        }
    }
}
