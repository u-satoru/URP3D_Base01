using System;
using UnityEngine;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core;
using asterivo.Unity60.Features.Templates.Platformer.Services;

namespace asterivo.Unity60.Features.Templates.Platformer.Controllers
{
    /// <summary>
    /// Jump Controller・壹・繝ｩ繝・ヨ繝輔か繝ｼ繝槭・霍ｳ霄榊宛蠕｡繧ｷ繧ｹ繝・Β
    /// ServiceLocator + Event鬧・虚繧｢繝ｼ繧ｭ繝・け繝√Ε縺ｫ繧医ｋ鬮伜ｺｦ縺ｪ繧ｸ繝｣繝ｳ繝励Γ繧ｫ繝九け繧ｹ
    /// Learn & Grow萓｡蛟､螳溽樟・夂峩諢溽噪縺ｪ霍ｳ霄肴─繝ｻ繝輔Ξ繝ｼ繝螳檎挑縺ｪ蜈･蜉帛ｿ懃ｭ斐・鬮伜ｺｦ縺ｪ霍ｳ霄阪ユ繧ｯ繝九ャ繧ｯ
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class JumpController : MonoBehaviour
    {
        [Header("Jump Settings")]
        [SerializeField] private int _maxJumpCount = 2; // 繝繝悶Ν繧ｸ繝｣繝ｳ繝怜ｯｾ蠢・
        [SerializeField] private float _coyoteTime = 0.1f; // 繧ｳ繝ｨ繝ｼ繝・ち繧､繝・亥慍髱｢繧帝屬繧後※繧ゅず繝｣繝ｳ繝怜庄閭ｽ縺ｪ譎る俣・・
        [SerializeField] private float _jumpBufferTime = 0.1f; // 繧ｸ繝｣繝ｳ繝励ヰ繝・ヵ繧｡・域掠謚ｼ縺怜ｯｾ蠢懶ｼ・
        [SerializeField] private LayerMask _groundLayerMask = 1; // 蝨ｰ髱｢繝ｬ繧､繝､繝ｼ
        [SerializeField] private Transform _groundCheckPoint; // 蝨ｰ髱｢讀懷・繝昴う繝ｳ繝・
        [SerializeField] private float _groundCheckRadius = 0.2f; // 蝨ｰ髱｢讀懷・蜊雁ｾ・

        [Header("Variable Jump Settings")]
        [SerializeField] private float _jumpCutMultiplier = 0.5f; // 繧ｸ繝｣繝ｳ繝礼洒邵ｮ蛟咲紫
        [SerializeField] private bool _enableVariableJumpHeight = true; // 蜿ｯ螟峨ず繝｣繝ｳ繝鈴ｫ伜ｺｦ

        [Header("Audio Integration")]
        [SerializeField] private bool _enableJumpSounds = true;
        [SerializeField] private string _jumpSoundEvent = "Jump";
        [SerializeField] private string _landSoundEvent = "Land";
        [SerializeField] private string _doubleJumpSoundEvent = "DoubleJump";

        [Header("Debug")]
        [SerializeField] private bool _showDebugInfo = false;
        [SerializeField] private bool _drawGroundCheck = true;

        // ServiceLocator邨ｱ蜷・
        private IPlatformerPhysicsService _physicsService;
        private IPlatformerInputService _inputService;
        private IPlatformerAudioService _audioService;

        // 繧ｳ繝ｳ繝昴・繝阪Φ繝亥盾辣ｧ
        private Rigidbody2D _rigidbody;
        private Collider2D _collider;

        // 霍ｳ霄咲憾諷狗ｮ｡逅・
        private int _currentJumpCount = 0;
        private bool _isGrounded = false;
        private bool _wasGrounded = false;
        private bool _isJumping = false;
        private bool _isFalling = false;

        // 繧ｿ繧､繝溘Φ繧ｰ蛻ｶ蠕｡
        private float _lastGroundedTime = 0f;
        private float _jumpBufferPressTime = -1f;
        private bool _jumpInputReleased = true;

        // Events
        public event Action OnJumpStarted;
        public event Action OnJumpEnded;
        public event Action OnLanded;
        public event Action<int> OnDoubleJump; // 繧ｸ繝｣繝ｳ繝怜屓謨ｰ莉倥″
        public event Action<bool> OnGroundedChanged;

        private void Awake()
        {
            // 繧ｳ繝ｳ繝昴・繝阪Φ繝亥叙蠕・
            _rigidbody = GetComponent<Rigidbody2D>();
            _collider = GetComponent<Collider2D>();

            // 繧ｰ繝ｩ繧ｦ繝ｳ繝峨メ繧ｧ繝・け繝昴う繝ｳ繝郁・蜍戊ｨｭ螳・
            if (_groundCheckPoint == null)
            {
                var groundCheck = new GameObject("GroundCheckPoint");
                groundCheck.transform.SetParent(transform);
                groundCheck.transform.localPosition = new Vector3(0, -_collider.bounds.size.y * 0.5f, 0);
                _groundCheckPoint = groundCheck.transform;
            }
        }

        private void Start()
        {
            // ServiceLocator邨ｱ蜷茨ｼ壹し繝ｼ繝薙せ蜿門ｾ・
            InitializeServices();
        }

        private void InitializeServices()
        {
            try
            {
                // Physics Service・壹ず繝｣繝ｳ繝苓ｨ育ｮ励→繧ｰ繝ｩ繧ｦ繝ｳ繝画､懷・
                _physicsService = ServiceLocator.GetService<IPlatformerPhysicsService>();
                if (_physicsService == null)
                {
                    Debug.LogError("[JumpController] IPlatformerPhysicsService not found in ServiceLocator!");
                }

                // Input Service・壹ず繝｣繝ｳ繝怜・蜉帛・逅・
                _inputService = ServiceLocator.GetService<IPlatformerInputService>();
                if (_inputService != null)
                {
                    // Event鬧・虚・壹ず繝｣繝ｳ繝怜・蜉帙う繝吶Φ繝郁ｳｼ隱ｭ
                    _inputService.OnJumpPressed += OnJumpInputPressed;
                    _inputService.OnJumpReleased += OnJumpInputReleased;
                }
                else
                {
                    Debug.LogError("[JumpController] IPlatformerInputService not found in ServiceLocator!");
                }

                // Audio Service・壹ず繝｣繝ｳ繝励し繧ｦ繝ｳ繝臥ｵｱ蜷・
                if (_enableJumpSounds)
                {
                    _audioService = ServiceLocator.GetService<IPlatformerAudioService>();
                    if (_audioService == null)
                    {
                        Debug.LogWarning("[JumpController] IPlatformerAudioService not found. Jump sounds will be disabled.");
                        _enableJumpSounds = false;
                    }
                }

                LogDebug("JumpController services initialized successfully.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[JumpController] Failed to initialize services: {ex.Message}");
            }
        }

        private void Update()
        {
            if (_physicsService == null) return;

            // 蝨ｰ髱｢讀懷・譖ｴ譁ｰ
            UpdateGroundDetection();

            // 繧ｸ繝｣繝ｳ繝励ヰ繝・ヵ繧｡蜃ｦ逅・
            ProcessJumpBuffer();

            // 蜿ｯ螟峨ず繝｣繝ｳ繝鈴ｫ伜ｺｦ蜃ｦ逅・
            ProcessVariableJumpHeight();

            // 迥ｶ諷区峩譁ｰ
            UpdateJumpState();
        }

        private void UpdateGroundDetection()
        {
            _wasGrounded = _isGrounded;

            // Physics Service邨ｱ蜷茨ｼ壼慍髱｢讀懷・
            _isGrounded = _physicsService.CheckGrounded(_groundCheckPoint.position, _groundLayerMask);

            // 蝨ｰ髱｢迥ｶ諷句､牙喧繧､繝吶Φ繝・
            if (_isGrounded != _wasGrounded)
            {
                OnGroundedChanged?.Invoke(_isGrounded);

                if (_isGrounded)
                {
                    // 逹蝨ｰ蜃ｦ逅・
                    HandleLanding();
                }
                else
                {
                    // 蝨ｰ髱｢髮｢閼ｱ蜃ｦ逅・ｼ壹さ繝ｨ繝ｼ繝・ち繧､繝髢句ｧ・
                    _lastGroundedTime = Time.time;
                }
            }

            // 繧ｳ繝ｨ繝ｼ繝・ち繧､繝譖ｴ譁ｰ
            if (_isGrounded)
            {
                _lastGroundedTime = Time.time;
            }
        }

        private void ProcessJumpBuffer()
        {
            // 繧ｸ繝｣繝ｳ繝励ヰ繝・ヵ繧｡譛牙柑譛滄俣蜀・・繧ｸ繝｣繝ｳ繝怜・逅・
            if (_jumpBufferPressTime >= 0 && Time.time - _jumpBufferPressTime <= _jumpBufferTime)
            {
                if (CanPerformJump())
                {
                    PerformJump();
                    _jumpBufferPressTime = -1f; // 繝舌ャ繝輔ぃ繧ｯ繝ｪ繧｢
                }
            }
        }

        private void ProcessVariableJumpHeight()
        {
            if (!_enableVariableJumpHeight || !_isJumping) return;

            // 繧ｸ繝｣繝ｳ繝励・繧ｿ繝ｳ縺碁屬縺輔ｌ縺溘ｉ荳頑・騾溷ｺｦ繧呈ｸ帛ｰ・
            if (_inputService != null && _inputService.JumpReleased && _jumpInputReleased == false)
            {
                _jumpInputReleased = true;

                if (_rigidbody.linearVelocity.y > 0)
                {
                    var velocity = _rigidbody.linearVelocity;
                    velocity.y *= _jumpCutMultiplier;
                    _rigidbody.linearVelocity = velocity;

                    LogDebug($"Jump cut applied. New velocity: {velocity.y:F2}");
                }
            }
        }

        private void UpdateJumpState()
        {
            var velocity = _rigidbody.linearVelocity;

            // 繧ｸ繝｣繝ｳ繝礼憾諷句愛螳・
            bool wasJumping = _isJumping;
            bool wasFalling = _isFalling;

            _isJumping = !_isGrounded && velocity.y > 0.1f;
            _isFalling = !_isGrounded && velocity.y < -0.1f;

            // 繧ｸ繝｣繝ｳ繝礼ｵゆｺ・う繝吶Φ繝・
            if (wasJumping && !_isJumping)
            {
                OnJumpEnded?.Invoke();
                LogDebug("Jump ended.");
            }
        }

        private void OnJumpInputPressed()
        {
            LogDebug("Jump input pressed.");

            // 繧ｸ繝｣繝ｳ繝励ヰ繝・ヵ繧｡險ｭ螳・
            _jumpBufferPressTime = Time.time;

            // 蜊ｳ蠎ｧ縺ｫ繧ｸ繝｣繝ｳ繝怜庄閭ｽ縺九メ繧ｧ繝・け
            if (CanPerformJump())
            {
                PerformJump();
                _jumpBufferPressTime = -1f; // 繝舌ャ繝輔ぃ繧ｯ繝ｪ繧｢
            }
        }

        private void OnJumpInputReleased()
        {
            LogDebug("Jump input released.");
            _jumpInputReleased = true;
        }

        private bool CanPerformJump()
        {
            if (_physicsService == null) return false;

            // 繧ｳ繝ｨ繝ｼ繝・ち繧､繝閠・・縺ｮ蝨ｰ髱｢蛻､螳・
            bool effectivelyGrounded = _isGrounded || (Time.time - _lastGroundedTime <= _coyoteTime);

            // Physics Service邨ｱ蜷茨ｼ壹ず繝｣繝ｳ繝怜庄閭ｽ諤ｧ蛻､螳・
            return _physicsService.CanJump(effectivelyGrounded, _currentJumpCount);
        }

        private void PerformJump()
        {
            if (_physicsService == null) return;

            // 繧ｳ繝ｨ繝ｼ繝・ち繧､繝閠・・縺ｮ蝨ｰ髱｢蛻､螳・
            bool effectivelyGrounded = _isGrounded || (Time.time - _lastGroundedTime <= _coyoteTime);

            // Physics Service邨ｱ蜷茨ｼ壹ず繝｣繝ｳ繝鈴溷ｺｦ險育ｮ・
            Vector2 jumpVelocity = _physicsService.CalculateJumpVelocity(effectivelyGrounded, _currentJumpCount);

            // 繧ｸ繝｣繝ｳ繝怜ｮ溯｡・
            var velocity = _rigidbody.linearVelocity;
            velocity.y = jumpVelocity.y;
            _rigidbody.linearVelocity = velocity;

            // 繧ｸ繝｣繝ｳ繝怜屓謨ｰ譖ｴ譁ｰ
            if (!effectivelyGrounded)
            {
                _currentJumpCount++;
                OnDoubleJump?.Invoke(_currentJumpCount);
                LogDebug($"Double jump performed. Jump count: {_currentJumpCount}");

                // 繝繝悶Ν繧ｸ繝｣繝ｳ繝励し繧ｦ繝ｳ繝牙・逕・
                PlayJumpSound(_doubleJumpSoundEvent);
            }
            else
            {
                _currentJumpCount = 1;
                LogDebug("Ground jump performed.");

                // 騾壼ｸｸ繧ｸ繝｣繝ｳ繝励し繧ｦ繝ｳ繝牙・逕・
                PlayJumpSound(_jumpSoundEvent);
            }

            // 迥ｶ諷九Μ繧ｻ繝・ヨ
            _jumpInputReleased = false;
            _lastGroundedTime = -_coyoteTime; // 繧ｳ繝ｨ繝ｼ繝・ち繧､繝繝ｪ繧ｻ繝・ヨ

            // 繧､繝吶Φ繝育匱陦・
            OnJumpStarted?.Invoke();

            LogDebug($"Jump performed. Velocity: {velocity.y:F2}, Jump count: {_currentJumpCount}");
        }

        private void HandleLanding()
        {
            if (_currentJumpCount > 0)
            {
                LogDebug("Player landed.");

                // 繧ｸ繝｣繝ｳ繝怜屓謨ｰ繝ｪ繧ｻ繝・ヨ
                _currentJumpCount = 0;
                _isJumping = false;
                _isFalling = false;

                // 逹蝨ｰ繧､繝吶Φ繝・
                OnLanded?.Invoke();

                // 逹蝨ｰ繧ｵ繧ｦ繝ｳ繝牙・逕・
                PlayJumpSound(_landSoundEvent);
            }
        }

        private void PlayJumpSound(string soundEvent)
        {
            if (_enableJumpSounds && _audioService != null && !string.IsNullOrEmpty(soundEvent))
            {
                try
                {
                    // Audio Service邨ｱ蜷茨ｼ壹し繧ｦ繝ｳ繝牙・逕・
                    // Note: 螳滄圀縺ｮ繝｡繧ｽ繝・ラ蜷阪・ IPlatformerAudioService 縺ｮ螳溯｣・↓繧医▲縺ｦ隱ｿ謨ｴ縺悟ｿ・ｦ・
                    // _audioService.PlaySound(soundEvent, transform.position);
                    LogDebug($"Jump sound triggered: {soundEvent}");
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[JumpController] Failed to play jump sound '{soundEvent}': {ex.Message}");
                }
            }
        }

        private void OnDestroy()
        {
            // Event鬧・虚・壹う繝吶Φ繝郁ｳｼ隱ｭ隗｣髯､
            if (_inputService != null)
            {
                _inputService.OnJumpPressed -= OnJumpInputPressed;
                _inputService.OnJumpReleased -= OnJumpInputReleased;
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!_drawGroundCheck || _groundCheckPoint == null) return;

            // 蝨ｰ髱｢讀懷・遽・峇縺ｮ蜿ｯ隕門喧
            Gizmos.color = _isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(_groundCheckPoint.position, _groundCheckRadius);

            // 繝・ヰ繝・げ諠・ｱ陦ｨ遉ｺ
            if (_showDebugInfo)
            {
                var labelPos = transform.position + Vector3.up * 2f;
                UnityEditor.Handles.Label(labelPos,
                    $"Grounded: {_isGrounded}\n" +
                    $"Jump Count: {_currentJumpCount}\n" +
                    $"Coyote Time: {Mathf.Max(0, _coyoteTime - (Time.time - _lastGroundedTime)):F2}");
            }
        }

        // 繝・ヰ繝・げ繝｡繧ｽ繝・ラ
        public void ShowJumpDebugInfo()
        {
            if (!_showDebugInfo) return;

            Debug.Log($"=== Jump Controller Debug Info ===\n" +
                     $"Grounded: {_isGrounded}\n" +
                     $"Jump Count: {_currentJumpCount}/{_maxJumpCount}\n" +
                     $"Is Jumping: {_isJumping}\n" +
                     $"Is Falling: {_isFalling}\n" +
                     $"Coyote Time Remaining: {Mathf.Max(0, _coyoteTime - (Time.time - _lastGroundedTime)):F2}\n" +
                     $"Jump Buffer Active: {(_jumpBufferPressTime >= 0 && Time.time - _jumpBufferPressTime <= _jumpBufferTime)}\n" +
                     $"Velocity Y: {_rigidbody.linearVelocity.y:F2}");
        }

        // 螟夜Κ蛻ｶ蠕｡API
        public void ForceJump(float jumpVelocity)
        {
            var velocity = _rigidbody.linearVelocity;
            velocity.y = jumpVelocity;
            _rigidbody.linearVelocity = velocity;

            _currentJumpCount = Mathf.Max(1, _currentJumpCount);
            _jumpInputReleased = false;

            OnJumpStarted?.Invoke();
            LogDebug($"Force jump executed with velocity: {jumpVelocity}");
        }

        public void ResetJumpCount()
        {
            _currentJumpCount = 0;
            LogDebug("Jump count reset.");
        }

        public void SetMaxJumpCount(int maxJumps)
        {
            _maxJumpCount = Mathf.Max(1, maxJumps);
            LogDebug($"Max jump count set to: {_maxJumpCount}");
        }

        // 繝励Ο繝代ユ繧｣
        public bool IsGrounded => _isGrounded;
        public bool IsJumping => _isJumping;
        public bool IsFalling => _isFalling;
        public int CurrentJumpCount => _currentJumpCount;
        public int MaxJumpCount => _maxJumpCount;
        public bool CanJump => CanPerformJump();

        private void LogDebug(string message)
        {
            if (_showDebugInfo)
            {
                Debug.Log($"[JumpController] {message}");
            }
        }
    }
}


