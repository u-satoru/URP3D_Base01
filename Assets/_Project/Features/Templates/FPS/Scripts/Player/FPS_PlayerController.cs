using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Features.Player.States;
using asterivo.Unity60.Features.Templates.FPS.Data;
using asterivo.Unity60.Features.Templates.FPS.WeaponSystem;
using asterivo.Unity60.Features.Templates.FPS.Configuration;

namespace asterivo.Unity60.Features.Templates.FPS.Player
{
    /// <summary>
    /// FPS特化プレイヤーコントローラー
    /// 詳細設計書3.1準拠：既存のPlayerStateMachineを継承・拡張し、FPS特有の状態と移動ロジックを実装
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerStateMachine))]
    public class FPS_PlayerController : MonoBehaviour
    {
        [Header("FPS Controller Configuration")]
        [SerializeField] private PlayerStatsConfig _playerStats;
        [SerializeField] private FPSMovementConfig _movementConfig;
        [SerializeField] private FPS_Template_Config _fpsConfig;

        [Header("Component References")]
        [SerializeField] private CharacterController _characterController;
        [SerializeField] private PlayerStateMachine _playerStateMachine;
        [SerializeField] private WeaponController _weaponController;
        [SerializeField] private UnityEngine.Camera _playerCamera;

        [Header("FPS Movement Settings")]
        [SerializeField] private float _mouseSensitivity = 2.0f;
        [SerializeField] private float _verticalLookLimit = 90.0f;
        [SerializeField] private bool _invertYAxis = false;

        [Header("Physics Settings")]
        [SerializeField] private float _gravity = -9.81f;
        [SerializeField] private float _groundCheckDistance = 0.1f;
        [SerializeField] private LayerMask _groundLayerMask = 1;

        [Header("FPS State Events")]
        [SerializeField] private GameEvent _onAimingStarted;
        [SerializeField] private GameEvent _onAimingStopped;
        [SerializeField] private GameEvent _onReloadStarted;
        [SerializeField] private GameEvent _onReloadCompleted;
        [SerializeField] private GameEvent _onWeaponSwitchStarted;
        [SerializeField] private GameEvent _onWeaponSwitchCompleted;

        [Header("Debug")]
        [SerializeField] private bool _enableDebugLogs = false;

        // FPS特有の状態は FPSTypes.cs で定義済み

        // 内部状態
        private FPSPlayerState _currentFPSState = FPSPlayerState.Idle;
        private Vector3 _velocity = Vector3.zero;
        private Vector2 _lookRotation = Vector2.zero;
        private bool _isGrounded = false;
        private bool _isJumpButtonPressed = false;
        private bool _isRunning = false;
        private bool _isCrouching = false;

        // 入力管理
        private Vector2 _moveInput = Vector2.zero;
        private Vector2 _lookInput = Vector2.zero;
        private bool _jumpInput = false;
        private bool _runInput = false;
        private bool _crouchInput = false;
        private bool _aimInput = false;
        private bool _fireInput = false;
        private bool _reloadInput = false;

        // アクション制御
        private bool _canMove = true;
        private bool _canLook = true;
        private bool _canJump = true;
        private bool _canShoot = true;

        /// <summary>
        /// 現在のFPS状態
        /// </summary>
        public FPSPlayerState CurrentFPSState => _currentFPSState;

        /// <summary>
        /// 地面に接地しているか
        /// </summary>
        public bool IsGrounded => _isGrounded;

        /// <summary>
        /// 走行中かどうか
        /// </summary>
        public bool IsRunning => _isRunning;

        /// <summary>
        /// しゃがみ中かどうか
        /// </summary>
        public bool IsCrouching => _isCrouching;

        /// <summary>
        /// エイミング中かどうか
        /// </summary>
        public bool IsAiming => _currentFPSState == FPSPlayerState.Aiming;

        /// <summary>
        /// リロード中かどうか
        /// </summary>
        public bool IsReloading => _currentFPSState == FPSPlayerState.Reloading;

        /// <summary>
        /// 現在の移動速度を取得（状態とスタンスに基づく）
        /// </summary>
        public float CurrentMoveSpeed => GetCurrentMoveSpeed();

        /// <summary>
        /// 現在のノイズレベルを取得（ステルス連動用）
        /// </summary>
        public float CurrentNoiseLevel => GetCurrentNoiseLevel();

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeComponents();
            ValidateConfiguration();
        }

        private void Start()
        {
            InitializeFPSController();
        }

        private void Update()
        {
            UpdateGroundCheck();
            HandleMovement();
            HandleRotation();
            HandleActions();
            UpdatePhysics();
        }

        #endregion

        #region Initialization

        /// <summary>
        /// コンポーネントの初期化
        /// </summary>
        private void InitializeComponents()
        {
            _characterController = _characterController ?? GetComponent<CharacterController>();
            _playerStateMachine = _playerStateMachine ?? GetComponent<PlayerStateMachine>();
            _weaponController = _weaponController ?? GetComponent<WeaponController>();
            _playerCamera = _playerCamera ?? UnityEngine.Camera.main;

            if (_playerCamera == null)
            {
                _playerCamera = FindObjectOfType<UnityEngine.Camera>();
            }
        }

        /// <summary>
        /// FPSコントローラーの初期化
        /// </summary>
        private void InitializeFPSController()
        {
            // カメラの初期設定
            if (_playerCamera != null)
            {
                _lookRotation.y = transform.eulerAngles.y;
                _lookRotation.x = _playerCamera.transform.localEulerAngles.x;
            }

            // カーソルロック
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (_enableDebugLogs)
            {
                Debug.Log("[FPS_PlayerController] Initialized successfully");
            }
        }

        /// <summary>
        /// 設定の検証
        /// </summary>
        private void ValidateConfiguration()
        {
            if (_playerStats == null)
            {
                Debug.LogError("[FPS_PlayerController] PlayerStatsConfig is not assigned!");
            }

            if (_characterController == null)
            {
                Debug.LogError("[FPS_PlayerController] CharacterController component is missing!");
            }

            if (_playerStateMachine == null)
            {
                Debug.LogError("[FPS_PlayerController] PlayerStateMachine component is missing!");
            }
        }

        #endregion

        #region Input Handling

        /// <summary>
        /// 移動入力の処理
        /// </summary>
        public void OnMove(InputAction.CallbackContext context)
        {
            _moveInput = context.ReadValue<Vector2>();
        }

        /// <summary>
        /// カメラ回転入力の処理
        /// </summary>
        public void OnLook(InputAction.CallbackContext context)
        {
            _lookInput = context.ReadValue<Vector2>();
        }

        /// <summary>
        /// ジャンプ入力の処理
        /// </summary>
        public void OnJump(InputAction.CallbackContext context)
        {
            _jumpInput = context.performed;
            if (context.performed) _isJumpButtonPressed = true;
        }

        /// <summary>
        /// 走行入力の処理
        /// </summary>
        public void OnRun(InputAction.CallbackContext context)
        {
            _runInput = context.ReadValue<float>() > 0.5f;
        }

        /// <summary>
        /// しゃがみ入力の処理
        /// </summary>
        public void OnCrouch(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                ToggleCrouch();
            }
        }

        /// <summary>
        /// エイミング入力の処理
        /// </summary>
        public void OnAim(InputAction.CallbackContext context)
        {
            _aimInput = context.ReadValue<float>() > 0.5f;
        }

        /// <summary>
        /// 射撃入力の処理
        /// </summary>
        public void OnFire(InputAction.CallbackContext context)
        {
            _fireInput = context.ReadValue<float>() > 0.5f;
        }

        /// <summary>
        /// リロード入力の処理
        /// </summary>
        public void OnReload(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                _reloadInput = true;
            }
        }

        #endregion

        #region Movement System

        /// <summary>
        /// 地面チェック
        /// </summary>
        private void UpdateGroundCheck()
        {
            Vector3 spherePosition = transform.position + _characterController.center + Vector3.down * (_characterController.height * 0.5f);
            _isGrounded = Physics.CheckSphere(spherePosition, _groundCheckDistance, _groundLayerMask);
        }

        /// <summary>
        /// 移動処理
        /// </summary>
        private void HandleMovement()
        {
            if (!_canMove || _currentFPSState == FPSPlayerState.Reloading)
            {
                _moveInput = Vector2.zero;
                return;
            }

            // 移動ベクトルの計算
            Vector3 moveDirection = CalculateMoveDirection();

            // 現在の移動速度を取得
            float moveSpeed = GetCurrentMoveSpeed();

            // 移動速度の適用
            Vector3 move = moveDirection * moveSpeed * Time.deltaTime;

            // キャラクターコントローラーで移動
            if (moveDirection.magnitude > 0.1f)
            {
                _characterController.Move(move);
                UpdatePlayerState();
            }
            else
            {
                // 停止状態
                if (!_isCrouching)
                {
                    _playerStateMachine.TransitionToState(PlayerStateType.Idle);
                }
            }
        }

        /// <summary>
        /// 移動方向の計算
        /// </summary>
        private Vector3 CalculateMoveDirection()
        {
            Vector3 forward = transform.forward;
            Vector3 right = transform.right;

            Vector3 moveDirection = (forward * _moveInput.y + right * _moveInput.x).normalized;
            return moveDirection;
        }

        /// <summary>
        /// 現在の移動速度を取得
        /// </summary>
        private float GetCurrentMoveSpeed()
        {
            if (_movementConfig == null) return 5.0f;

            // エイミング中は速度減少
            float aimingMultiplier = IsAiming ? 0.5f : 1.0f;

            if (_isCrouching)
            {
                return _movementConfig.GetSpeedForState(FPSPlayerState.Crouching) * aimingMultiplier;
            }
            else if (_isRunning && !IsAiming)
            {
                return _movementConfig.GetSpeedForState(FPSPlayerState.Running);
            }
            else
            {
                return _movementConfig.GetSpeedForState(FPSPlayerState.Walking) * aimingMultiplier;
            }
        }

        /// <summary>
        /// 現在のノイズレベルを取得
        /// </summary>
        private float GetCurrentNoiseLevel()
        {
            if (_playerStats == null) return 1.0f;

            if (_isCrouching)
            {
                return _playerStats.GetNoiseLevelForState(FPSPlayerState.Crouching);
            }
            else if (_isRunning)
            {
                return _playerStats.GetNoiseLevelForState(FPSPlayerState.Running);
            }
            else
            {
                return _playerStats.GetNoiseLevelForState(FPSPlayerState.Walking);
            }
        }

        /// <summary>
        /// プレイヤー状態の更新
        /// </summary>
        private void UpdatePlayerState()
        {
            if (_moveInput.magnitude > 0.1f)
            {
                if (_isCrouching)
                {
                    _playerStateMachine.TransitionToState(PlayerStateType.Crouching);
                }
                else if (_isRunning && !IsAiming)
                {
                    _playerStateMachine.TransitionToState(PlayerStateType.Running);
                    _isRunning = _runInput && _moveInput.magnitude > 0.7f;
                }
                else
                {
                    _playerStateMachine.TransitionToState(PlayerStateType.Walking);
                }
            }
        }

        #endregion

        #region Rotation System

        /// <summary>
        /// カメラ回転処理
        /// </summary>
        private void HandleRotation()
        {
            if (!_canLook || _currentFPSState == FPSPlayerState.Reloading)
                return;

            // マウス感度の適用
            float mouseX = _lookInput.x * _mouseSensitivity * Time.deltaTime;
            float mouseY = _lookInput.y * _mouseSensitivity * Time.deltaTime;

            if (_invertYAxis)
            {
                mouseY = -mouseY;
            }

            // 回転値の更新
            _lookRotation.y += mouseX;
            _lookRotation.x -= mouseY;

            // 垂直回転の制限
            _lookRotation.x = Mathf.Clamp(_lookRotation.x, -_verticalLookLimit, _verticalLookLimit);

            // 回転の適用
            transform.rotation = Quaternion.Euler(0, _lookRotation.y, 0);

            if (_playerCamera != null)
            {
                _playerCamera.transform.localRotation = Quaternion.Euler(_lookRotation.x, 0, 0);
            }
        }

        #endregion

        #region Action Handling

        /// <summary>
        /// アクション処理
        /// </summary>
        private void HandleActions()
        {
            HandleRunning();
            HandleAiming();
            HandleShooting();
            HandleReloading();
            HandleJumping();
        }

        /// <summary>
        /// 走行処理
        /// </summary>
        private void HandleRunning()
        {
            _isRunning = _runInput && _moveInput.magnitude > 0.7f && !IsAiming && !_isCrouching;
        }

        /// <summary>
        /// エイミング処理
        /// </summary>
        private void HandleAiming()
        {
            if (_aimInput && (_currentFPSState == FPSPlayerState.Idle || _currentFPSState == FPSPlayerState.Walking || _currentFPSState == FPSPlayerState.Running))
            {
                StartAiming();
            }
            else if (!_aimInput && _currentFPSState == FPSPlayerState.Aiming)
            {
                StopAiming();
            }
        }

        /// <summary>
        /// 射撃処理
        /// </summary>
        private void HandleShooting()
        {
            if (_fireInput && _canShoot && _weaponController != null)
            {
                _weaponController.StartShooting();
            }
        }

        /// <summary>
        /// リロード処理
        /// </summary>
        private void HandleReloading()
        {
            if (_reloadInput && (_currentFPSState == FPSPlayerState.Idle || _currentFPSState == FPSPlayerState.Walking || _currentFPSState == FPSPlayerState.Running) && _weaponController != null)
            {
                StartReload();
            }
            _reloadInput = false;
        }

        /// <summary>
        /// ジャンプ処理
        /// </summary>
        private void HandleJumping()
        {
            if (_isJumpButtonPressed && _canJump && _isGrounded && !_isCrouching)
            {
                Jump();
                _isJumpButtonPressed = false;
            }
            else if (!_isGrounded)
            {
                _isJumpButtonPressed = false;
            }
        }

        #endregion

        #region FPS State Management

        /// <summary>
        /// エイミング開始
        /// </summary>
        private void StartAiming()
        {
            if (_currentFPSState == FPSPlayerState.Reloading || _currentFPSState == FPSPlayerState.WeaponSwitching) return;

            _currentFPSState = FPSPlayerState.Aiming;
            _onAimingStarted?.Raise();

            if (_enableDebugLogs)
            {
                Debug.Log("[FPS_PlayerController] Started aiming");
            }
        }

        /// <summary>
        /// エイミング停止
        /// </summary>
        private void StopAiming()
        {
            if (_currentFPSState != FPSPlayerState.Aiming) return;

            _currentFPSState = FPSPlayerState.Idle;
            _onAimingStopped?.Raise();

            if (_enableDebugLogs)
            {
                Debug.Log("[FPS_PlayerController] Stopped aiming");
            }
        }

        /// <summary>
        /// リロード開始
        /// </summary>
        private void StartReload()
        {
            if (_currentFPSState == FPSPlayerState.Reloading || _currentFPSState == FPSPlayerState.WeaponSwitching) return;

            _currentFPSState = FPSPlayerState.Reloading;
            _onReloadStarted?.Raise();

            if (_weaponController != null)
            {
                _weaponController.Reload();
            }

            if (_enableDebugLogs)
            {
                Debug.Log("[FPS_PlayerController] Started reload");
            }
        }

        /// <summary>
        /// リロード完了
        /// </summary>
        public void OnReloadCompleted()
        {
            if (_currentFPSState != FPSPlayerState.Reloading) return;

            _currentFPSState = FPSPlayerState.Idle;
            _onReloadCompleted?.Raise();

            if (_enableDebugLogs)
            {
                Debug.Log("[FPS_PlayerController] Reload completed");
            }
        }

        /// <summary>
        /// しゃがみ切り替え
        /// </summary>
        private void ToggleCrouch()
        {
            _isCrouching = !_isCrouching;

            if (_isCrouching)
            {
                _playerStateMachine.TransitionToState(PlayerStateType.Crouching);
            }
            else
            {
                _playerStateMachine.TransitionToState(PlayerStateType.Idle);
            }

            if (_enableDebugLogs)
            {
                Debug.Log($"[FPS_PlayerController] Crouch: {_isCrouching}");
            }
        }

        /// <summary>
        /// ジャンプ
        /// </summary>
        private void Jump()
        {
            if (_playerStats == null) return;

            _velocity.y = Mathf.Sqrt(_playerStats.JumpHeight * -2.0f * _gravity);
            _playerStateMachine.TransitionToState(PlayerStateType.Jumping);

            if (_enableDebugLogs)
            {
                Debug.Log("[FPS_PlayerController] Jumped");
            }
        }

        #endregion

        #region Physics

        /// <summary>
        /// 物理演算の更新
        /// </summary>
        private void UpdatePhysics()
        {
            // 重力の適用
            if (_isGrounded && _velocity.y < 0)
            {
                _velocity.y = -2f; // 地面に接地している時の小さな下向き速度
            }
            else
            {
                _velocity.y += _gravity * Time.deltaTime;
            }

            // 垂直方向の移動を適用
            _characterController.Move(_velocity * Time.deltaTime);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 武器コントローラーの設定
        /// </summary>
        public void SetWeaponController(WeaponController weaponController)
        {
            _weaponController = weaponController;
        }

        /// <summary>
        /// 移動可能状態の設定
        /// </summary>
        public void SetCanMove(bool canMove)
        {
            _canMove = canMove;
        }

        /// <summary>
        /// 視点回転可能状態の設定
        /// </summary>
        public void SetCanLook(bool canLook)
        {
            _canLook = canLook;
        }

        /// <summary>
        /// 射撃可能状態の設定
        /// </summary>
        public void SetCanShoot(bool canShoot)
        {
            _canShoot = canShoot;
        }

        /// <summary>
        /// デバッグ情報を取得
        /// </summary>
        public string GetDebugInfo()
        {
            return $"FPS_PlayerController - State: {_currentFPSState}, Speed: {CurrentMoveSpeed:F1}, Noise: {CurrentNoiseLevel:F1}, Grounded: {_isGrounded}";
        }

        #endregion

        #region Debug

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_characterController == null) return;

            // 地面チェックの可視化
            Vector3 spherePosition = transform.position + _characterController.center + Vector3.down * (_characterController.height * 0.5f);
            Gizmos.color = _isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(spherePosition, _groundCheckDistance);
        }
#endif

        #endregion
    }
}
