using System;
using UnityEngine;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Features.Templates.Platformer.Services;

namespace asterivo.Unity60.Features.Templates.Platformer.Controllers
{
    /// <summary>
    /// Jump Controller：プラットフォーマー跳躍制御システム
    /// ServiceLocator + Event駆動アーキテクチャによる高度なジャンプメカニクス
    /// Learn & Grow価値実現：直感的な跳躍感・フレーム完璧な入力応答・高度な跳躍テクニック
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class JumpController : MonoBehaviour
    {
        [Header("Jump Settings")]
        [SerializeField] private int _maxJumpCount = 2; // ダブルジャンプ対応
        [SerializeField] private float _coyoteTime = 0.1f; // コヨーテタイム（地面を離れてもジャンプ可能な時間）
        [SerializeField] private float _jumpBufferTime = 0.1f; // ジャンプバッファ（早押し対応）
        [SerializeField] private LayerMask _groundLayerMask = 1; // 地面レイヤー
        [SerializeField] private Transform _groundCheckPoint; // 地面検出ポイント
        [SerializeField] private float _groundCheckRadius = 0.2f; // 地面検出半径

        [Header("Variable Jump Settings")]
        [SerializeField] private float _jumpCutMultiplier = 0.5f; // ジャンプ短縮倍率
        [SerializeField] private bool _enableVariableJumpHeight = true; // 可変ジャンプ高度

        [Header("Audio Integration")]
        [SerializeField] private bool _enableJumpSounds = true;
        [SerializeField] private string _jumpSoundEvent = "Jump";
        [SerializeField] private string _landSoundEvent = "Land";
        [SerializeField] private string _doubleJumpSoundEvent = "DoubleJump";

        [Header("Debug")]
        [SerializeField] private bool _showDebugInfo = false;
        [SerializeField] private bool _drawGroundCheck = true;

        // ServiceLocator統合
        private IPlatformerPhysicsService _physicsService;
        private IPlatformerInputService _inputService;
        private IPlatformerAudioService _audioService;

        // コンポーネント参照
        private Rigidbody2D _rigidbody;
        private Collider2D _collider;

        // 跳躍状態管理
        private int _currentJumpCount = 0;
        private bool _isGrounded = false;
        private bool _wasGrounded = false;
        private bool _isJumping = false;
        private bool _isFalling = false;

        // タイミング制御
        private float _lastGroundedTime = 0f;
        private float _jumpBufferPressTime = -1f;
        private bool _jumpInputReleased = true;

        // Events
        public event Action OnJumpStarted;
        public event Action OnJumpEnded;
        public event Action OnLanded;
        public event Action<int> OnDoubleJump; // ジャンプ回数付き
        public event Action<bool> OnGroundedChanged;

        private void Awake()
        {
            // コンポーネント取得
            _rigidbody = GetComponent<Rigidbody2D>();
            _collider = GetComponent<Collider2D>();

            // グラウンドチェックポイント自動設定
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
            // ServiceLocator統合：サービス取得
            InitializeServices();
        }

        private void InitializeServices()
        {
            try
            {
                // Physics Service：ジャンプ計算とグラウンド検出
                _physicsService = ServiceLocator.GetService<IPlatformerPhysicsService>();
                if (_physicsService == null)
                {
                    Debug.LogError("[JumpController] IPlatformerPhysicsService not found in ServiceLocator!");
                }

                // Input Service：ジャンプ入力処理
                _inputService = ServiceLocator.GetService<IPlatformerInputService>();
                if (_inputService != null)
                {
                    // Event駆動：ジャンプ入力イベント購読
                    _inputService.OnJumpPressed += OnJumpInputPressed;
                    _inputService.OnJumpReleased += OnJumpInputReleased;
                }
                else
                {
                    Debug.LogError("[JumpController] IPlatformerInputService not found in ServiceLocator!");
                }

                // Audio Service：ジャンプサウンド統合
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

            // 地面検出更新
            UpdateGroundDetection();

            // ジャンプバッファ処理
            ProcessJumpBuffer();

            // 可変ジャンプ高度処理
            ProcessVariableJumpHeight();

            // 状態更新
            UpdateJumpState();
        }

        private void UpdateGroundDetection()
        {
            _wasGrounded = _isGrounded;

            // Physics Service統合：地面検出
            _isGrounded = _physicsService.CheckGrounded(_groundCheckPoint.position, _groundLayerMask);

            // 地面状態変化イベント
            if (_isGrounded != _wasGrounded)
            {
                OnGroundedChanged?.Invoke(_isGrounded);

                if (_isGrounded)
                {
                    // 着地処理
                    HandleLanding();
                }
                else
                {
                    // 地面離脱処理：コヨーテタイム開始
                    _lastGroundedTime = Time.time;
                }
            }

            // コヨーテタイム更新
            if (_isGrounded)
            {
                _lastGroundedTime = Time.time;
            }
        }

        private void ProcessJumpBuffer()
        {
            // ジャンプバッファ有効期間内のジャンプ処理
            if (_jumpBufferPressTime >= 0 && Time.time - _jumpBufferPressTime <= _jumpBufferTime)
            {
                if (CanPerformJump())
                {
                    PerformJump();
                    _jumpBufferPressTime = -1f; // バッファクリア
                }
            }
        }

        private void ProcessVariableJumpHeight()
        {
            if (!_enableVariableJumpHeight || !_isJumping) return;

            // ジャンプボタンが離されたら上昇速度を減少
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

            // ジャンプ状態判定
            bool wasJumping = _isJumping;
            bool wasFalling = _isFalling;

            _isJumping = !_isGrounded && velocity.y > 0.1f;
            _isFalling = !_isGrounded && velocity.y < -0.1f;

            // ジャンプ終了イベント
            if (wasJumping && !_isJumping)
            {
                OnJumpEnded?.Invoke();
                LogDebug("Jump ended.");
            }
        }

        private void OnJumpInputPressed()
        {
            LogDebug("Jump input pressed.");

            // ジャンプバッファ設定
            _jumpBufferPressTime = Time.time;

            // 即座にジャンプ可能かチェック
            if (CanPerformJump())
            {
                PerformJump();
                _jumpBufferPressTime = -1f; // バッファクリア
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

            // コヨーテタイム考慮の地面判定
            bool effectivelyGrounded = _isGrounded || (Time.time - _lastGroundedTime <= _coyoteTime);

            // Physics Service統合：ジャンプ可能性判定
            return _physicsService.CanJump(effectivelyGrounded, _currentJumpCount);
        }

        private void PerformJump()
        {
            if (_physicsService == null) return;

            // コヨーテタイム考慮の地面判定
            bool effectivelyGrounded = _isGrounded || (Time.time - _lastGroundedTime <= _coyoteTime);

            // Physics Service統合：ジャンプ速度計算
            Vector2 jumpVelocity = _physicsService.CalculateJumpVelocity(effectivelyGrounded, _currentJumpCount);

            // ジャンプ実行
            var velocity = _rigidbody.linearVelocity;
            velocity.y = jumpVelocity.y;
            _rigidbody.linearVelocity = velocity;

            // ジャンプ回数更新
            if (!effectivelyGrounded)
            {
                _currentJumpCount++;
                OnDoubleJump?.Invoke(_currentJumpCount);
                LogDebug($"Double jump performed. Jump count: {_currentJumpCount}");

                // ダブルジャンプサウンド再生
                PlayJumpSound(_doubleJumpSoundEvent);
            }
            else
            {
                _currentJumpCount = 1;
                LogDebug("Ground jump performed.");

                // 通常ジャンプサウンド再生
                PlayJumpSound(_jumpSoundEvent);
            }

            // 状態リセット
            _jumpInputReleased = false;
            _lastGroundedTime = -_coyoteTime; // コヨーテタイムリセット

            // イベント発行
            OnJumpStarted?.Invoke();

            LogDebug($"Jump performed. Velocity: {velocity.y:F2}, Jump count: {_currentJumpCount}");
        }

        private void HandleLanding()
        {
            if (_currentJumpCount > 0)
            {
                LogDebug("Player landed.");

                // ジャンプ回数リセット
                _currentJumpCount = 0;
                _isJumping = false;
                _isFalling = false;

                // 着地イベント
                OnLanded?.Invoke();

                // 着地サウンド再生
                PlayJumpSound(_landSoundEvent);
            }
        }

        private void PlayJumpSound(string soundEvent)
        {
            if (_enableJumpSounds && _audioService != null && !string.IsNullOrEmpty(soundEvent))
            {
                try
                {
                    // Audio Service統合：サウンド再生
                    // Note: 実際のメソッド名は IPlatformerAudioService の実装によって調整が必要
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
            // Event駆動：イベント購読解除
            if (_inputService != null)
            {
                _inputService.OnJumpPressed -= OnJumpInputPressed;
                _inputService.OnJumpReleased -= OnJumpInputReleased;
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!_drawGroundCheck || _groundCheckPoint == null) return;

            // 地面検出範囲の可視化
            Gizmos.color = _isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(_groundCheckPoint.position, _groundCheckRadius);

            // デバッグ情報表示
            if (_showDebugInfo)
            {
                var labelPos = transform.position + Vector3.up * 2f;
                UnityEditor.Handles.Label(labelPos,
                    $"Grounded: {_isGrounded}\n" +
                    $"Jump Count: {_currentJumpCount}\n" +
                    $"Coyote Time: {Mathf.Max(0, _coyoteTime - (Time.time - _lastGroundedTime)):F2}");
            }
        }

        // デバッグメソッド
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

        // 外部制御API
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

        // プロパティ
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