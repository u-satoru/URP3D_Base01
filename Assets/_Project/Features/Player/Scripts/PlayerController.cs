using UnityEngine;
using UnityEngine.InputSystem;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Player;
using asterivo.Unity60.Core.Data;
using System.Collections;
using System.Collections.Generic;

namespace asterivo.Unity60.Player
{
    /// <summary>
    /// プレイヤーコントローラー
    /// 静的リスナー方式と自動状態遷移を実装
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerController : MonoBehaviour
    {
        #region Movement Settings
        [Header("Movement Settings")]
        [SerializeField] private float walkSpeed = 3f;
        [SerializeField] private float runSpeed = 6f;
        [SerializeField] private float sprintSpeed = 9f;
        [SerializeField] private float jumpHeight = 2f;
        [SerializeField] private float gravity = -19.81f;
        [SerializeField] private float groundCheckRadius = 0.3f;
        [SerializeField] private float rotationSpeed = 10f;
        
        [Header("Advanced Movement")]
        [SerializeField] private float airControl = 0.3f;
        [SerializeField] private float coyoteTime = 0.15f;
        [SerializeField] private float jumpBufferTime = 0.2f;
        [SerializeField] private AnimationCurve accelerationCurve;
        [SerializeField] private AnimationCurve decelerationCurve;
        
        [Header("State Transition Thresholds")]
        [SerializeField] private float walkThreshold = 0.5f;
        [SerializeField] private float runThreshold = 4.5f;
        [SerializeField] private float sprintThreshold = 7.5f;
        
        [Header("Camera")]
        [SerializeField] private Transform cameraFollow;
        [SerializeField] private float mouseSensitivity = 2f;
        [SerializeField] private float maxLookAngle = 80f;
        [SerializeField] private bool invertY = false;
        
        [Header("Ground Check")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private LayerMask groundMask = -1;
        
        [Header("Error Handling")]
        [SerializeField] private bool enableErrorLogging = true;
        [SerializeField] private bool validateReferences = true;
        #endregion
        
        #region Events - Output
        [Header("Movement Events (Output)")]
        [SerializeField] private GameEvent onPlayerJump;
        [SerializeField] private GameEvent onPlayerLanded;
        [SerializeField] private GameEvent onPlayerStartSprint;
        [SerializeField] private GameEvent onPlayerStopSprint;
        [SerializeField] private GameEvent onPlayerStartMove;
        [SerializeField] private GameEvent onPlayerStopMove;
        
        [Header("State Change Events (Output)")]
        [SerializeField] private PlayerStateEvent onStateChangeRequest;
        [SerializeField] private BoolGameEvent onGroundedChanged;
        
        [Header("Data Update Events (Output)")]
        [SerializeField] private FloatGameEvent onSpeedChanged;
        [SerializeField] private Vector3GameEvent onPositionChanged;
        [SerializeField] private FloatGameEvent onHealthChanged;
        [SerializeField] private FloatGameEvent onStaminaChanged;
        
        [Header("Player Data Events (Output)")]
        [SerializeField] private PlayerDataEvent onPlayerDataUpdated;
        
        [Header("Camera Events (Output)")]
        [SerializeField] private Vector2GameEvent onLookInputChanged;
        [SerializeField] private GameEvent onCameraShakeRequest;
        [SerializeField] private FloatGameEvent onFOVChangeRequest;
        [SerializeField] private CameraStateEvent onCameraStateChangeRequest;
        #endregion
        
        #region Event Listeners - Static
        [Header("Static Event Listeners")]
        [SerializeField] private GameEventListener damageReceivedListener;
        [SerializeField] private FloatGameEventListener damageAmountListener;
        [SerializeField] private GameEventListener respawnRequestListener;
        [SerializeField] private Vector3GameEventListener teleportRequestListener;
        [SerializeField] private GameEventListener freezeMovementListener;
        [SerializeField] private GameEventListener unfreezeMovementListener;
        [SerializeField] private PlayerStateEventListener forceStateChangeListener;
        [SerializeField] private GameEventListener enterCombatListener;
        [SerializeField] private GameEventListener exitCombatListener;
        #endregion
        
        #region Components
        private CharacterController controller;
        private PlayerInput playerInput;
        private InputActionMap playerActionMap;
        #endregion
        
        #region State Variables
        private Vector3 velocity;
        private Vector3 moveDirection;
        private Vector2 moveInput;
        private Vector2 lookInput;
        private float currentSpeed;
        private float targetSpeed;
        private float speedChangeRate = 10f;
        private float xRotation = 0f;
        
        private bool isGrounded;
        private bool wasGrounded;
        private bool isSprinting;
        private bool isJumping;
        private bool isMoving;
        private bool movementFrozen;
        
        // 現在の移動状態
        private PlayerState currentMovementState = PlayerState.Idle;
        private PlayerState previousMovementState = PlayerState.Idle;
        
        // タイマー
        private float coyoteTimeCounter;
        private float jumpBufferCounter;
        private float lastGroundedTime;
        
        // コルーチン管理
        private Coroutine landingRecoveryCoroutine;
        private Dictionary<string, Coroutine> activeCoroutines = new Dictionary<string, Coroutine>();
        
        // プレイヤーデータ
        private float currentHealth = 100f;
        private float maxHealth = 100f;
        private float currentStamina = 100f;
        private float maxStamina = 100f;
        #endregion
        
        #region Properties
        public bool IsGrounded => isGrounded;
        public bool IsSprinting => isSprinting;
        public float CurrentSpeed => currentSpeed;
        public Vector3 Velocity => velocity;
        public Vector3 MoveDirection => moveDirection;
        public bool IsMoving => isMoving;
        public PlayerState CurrentMovementState => currentMovementState;
        #endregion
        
        #region Unity Lifecycle
        private void Awake()
        {
            if (!ValidateComponents())
            {
                enabled = false;
                return;
            }
            
            controller = GetComponent<CharacterController>();
            playerInput = GetComponent<PlayerInput>();
            
            SetupInputCallbacks();
            InitializeCurves();
        }
        
        private void OnEnable()
        {
            RegisterEventListeners();
        }
        
        private void OnDisable()
        {
            UnregisterEventListeners();
            StopAllActiveCoroutines();
        }
        
        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            
            // 初期データをイベントで通知
            NotifyInitialState();
        }
        
        private void Update()
        {
            if (!movementFrozen)
            {
                UpdateGroundCheck();
                UpdateMovement();
                UpdateMovementState();
                UpdateRotation();
                UpdateJump();
            }
            
            UpdateEvents();
        }
        
        private void OnDestroy()
        {
            CleanupInputCallbacks();
            StopAllActiveCoroutines();
        }
        #endregion
        
        #region Validation
        private bool ValidateComponents()
        {
            if (!validateReferences) return true;
            
            bool isValid = true;
            
            if (groundCheck == null)
            {
                LogError("Ground Check Transform is not assigned!");
                isValid = false;
            }
            
            if (groundMask == 0)
            {
                LogWarning("Ground Mask is not set. Using default layer mask.");
                groundMask = -1;
            }
            
            return isValid;
        }
        
        private void LogError(string message)
        {
            if (enableErrorLogging)
                Debug.LogError($"[PlayerController] {message}", this);
        }
        
        private void LogWarning(string message)
        {
            if (enableErrorLogging)
                Debug.LogWarning($"[PlayerController] {message}", this);
        }
        #endregion
        
        #region Event Registration - Static
        private void RegisterEventListeners()
        {
            // 静的リスナーのUnityEventにハンドラーを登録
            if (damageReceivedListener != null)
                damageReceivedListener.Response.AddListener(OnDamageReceived);
            
            if (damageAmountListener != null)
                damageAmountListener.Response.AddListener(OnDamageAmountReceived);
            
            if (respawnRequestListener != null)
                respawnRequestListener.Response.AddListener(OnRespawnRequested);
            
            if (teleportRequestListener != null)
                teleportRequestListener.Response.AddListener(OnTeleportRequested);
            
            if (freezeMovementListener != null)
                freezeMovementListener.Response.AddListener(() => movementFrozen = true);
            
            if (unfreezeMovementListener != null)
                unfreezeMovementListener.Response.AddListener(() => movementFrozen = false);
            
            if (forceStateChangeListener != null)
                forceStateChangeListener.Response.AddListener(OnForceStateChange);
            
            if (enterCombatListener != null)
                enterCombatListener.Response.AddListener(OnEnterCombatMode);
            
            if (exitCombatListener != null)
                exitCombatListener.Response.AddListener(OnExitCombatMode);
        }
        
        private void UnregisterEventListeners()
        {
            // UnityEventからハンドラーを削除
            if (damageReceivedListener != null)
                damageReceivedListener.Response.RemoveListener(OnDamageReceived);
            
            if (damageAmountListener != null)
                damageAmountListener.Response.RemoveListener(OnDamageAmountReceived);
            
            if (respawnRequestListener != null)
                respawnRequestListener.Response.RemoveListener(OnRespawnRequested);
            
            if (teleportRequestListener != null)
                teleportRequestListener.Response.RemoveListener(OnTeleportRequested);
            
            if (freezeMovementListener != null)
                freezeMovementListener.Response.RemoveListener(() => movementFrozen = true);
            
            if (unfreezeMovementListener != null)
                unfreezeMovementListener.Response.RemoveListener(() => movementFrozen = false);
            
            if (forceStateChangeListener != null)
                forceStateChangeListener.Response.RemoveListener(OnForceStateChange);
            
            if (enterCombatListener != null)
                enterCombatListener.Response.RemoveListener(OnEnterCombatMode);
            
            if (exitCombatListener != null)
                exitCombatListener.Response.RemoveListener(OnExitCombatMode);
        }
        #endregion
        
        #region Movement State Management
        private void UpdateMovementState()
        {
            PlayerState newState = DetermineMovementState();
            
            if (newState != currentMovementState)
            {
                previousMovementState = currentMovementState;
                currentMovementState = newState;
                onStateChangeRequest?.Raise(newState);
            }
        }
        
        private PlayerState DetermineMovementState()
        {
            // 特殊状態の優先チェック
            if (!isGrounded)
            {
                if (isJumping) return PlayerState.Jumping;
                if (velocity.y < 0) return PlayerState.Falling;
            }
            
            // 速度に基づく状態決定
            if (currentSpeed < walkThreshold)
            {
                return PlayerState.Idle;
            }
            else if (currentSpeed < runThreshold)
            {
                return PlayerState.Walking;
            }
            else if (currentSpeed < sprintThreshold)
            {
                return PlayerState.Running;
            }
            else
            {
                return PlayerState.Sprinting;
            }
        }
        #endregion
        
        #region Input Setup
        private void SetupInputCallbacks()
        {
            try
            {
                playerActionMap = playerInput.currentActionMap;
                
                var moveAction = playerActionMap.FindAction("Move");
                if (moveAction != null)
                {
                    moveAction.performed += OnMove;
                    moveAction.canceled += OnMoveCanceled;
                }
                
                var lookAction = playerActionMap.FindAction("Look");
                if (lookAction != null)
                {
                    lookAction.performed += OnLook;
                    lookAction.canceled += OnLookCanceled;
                }
                
                var jumpAction = playerActionMap.FindAction("Jump");
                if (jumpAction != null)
                {
                    jumpAction.started += OnJump;
                }
                
                var sprintAction = playerActionMap.FindAction("Sprint");
                if (sprintAction != null)
                {
                    sprintAction.started += OnSprintStart;
                    sprintAction.canceled += OnSprintEnd;
                }
            }
            catch (System.Exception e)
            {
                LogError($"Failed to setup input callbacks: {e.Message}");
            }
        }
        
        private void CleanupInputCallbacks()
        {
            try
            {
                var moveAction = playerActionMap?.FindAction("Move");
                if (moveAction != null)
                {
                    moveAction.performed -= OnMove;
                    moveAction.canceled -= OnMoveCanceled;
                }
                
                var lookAction = playerActionMap?.FindAction("Look");
                if (lookAction != null)
                {
                    lookAction.performed -= OnLook;
                    lookAction.canceled -= OnLookCanceled;
                }
                
                var jumpAction = playerActionMap?.FindAction("Jump");
                if (jumpAction != null)
                {
                    jumpAction.started -= OnJump;
                }
                
                var sprintAction = playerActionMap?.FindAction("Sprint");
                if (sprintAction != null)
                {
                    sprintAction.started -= OnSprintStart;
                    sprintAction.canceled -= OnSprintEnd;
                }
            }
            catch (System.Exception e)
            {
                LogError($"Error during input cleanup: {e.Message}");
            }
        }
        #endregion
        
        #region Input Callbacks
        private void OnMove(InputAction.CallbackContext context)
        {
            if (movementFrozen) return;
            
            moveInput = context.ReadValue<Vector2>();
            
            if (!isMoving && moveInput.magnitude > 0.1f)
            {
                isMoving = true;
                onPlayerStartMove?.Raise();
            }
        }
        
        private void OnMoveCanceled(InputAction.CallbackContext context)
        {
            moveInput = Vector2.zero;
            
            if (isMoving)
            {
                isMoving = false;
                onPlayerStopMove?.Raise();
            }
        }
        
        private void OnLook(InputAction.CallbackContext context)
        {
            if (movementFrozen) return;
            lookInput = context.ReadValue<Vector2>();
        }
        
        private void OnLookCanceled(InputAction.CallbackContext context)
        {
            lookInput = Vector2.zero;
        }
        
        private void OnJump(InputAction.CallbackContext context)
        {
            if (movementFrozen) return;
            
            jumpBufferCounter = jumpBufferTime;
            
            if (CanJump())
            {
                PerformJump();
            }
        }
        
        private void OnSprintStart(InputAction.CallbackContext context)
        {
            if (movementFrozen) return;
            
            if (!isSprinting && moveInput.magnitude > 0.1f && currentStamina > 0)
            {
                isSprinting = true;
                onPlayerStartSprint?.Raise();
            }
        }
        
        private void OnSprintEnd(InputAction.CallbackContext context)
        {
            if (isSprinting)
            {
                isSprinting = false;
                onPlayerStopSprint?.Raise();
            }
        }
        #endregion
        
        #region Movement
        private void UpdateGroundCheck()
        {
            wasGrounded = isGrounded;
            
            if (groundCheck != null)
            {
                isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundMask);
            }
            
            if (isGrounded && !wasGrounded)
            {
                OnLanded();
            }
            else if (!isGrounded && wasGrounded)
            {
                OnFallStart();
            }
            
            if (isGrounded)
            {
                coyoteTimeCounter = coyoteTime;
                lastGroundedTime = Time.time;
            }
            else
            {
                coyoteTimeCounter = Mathf.Max(0, coyoteTimeCounter - Time.deltaTime);
            }
        }
        
        private void UpdateMovement()
        {
            CalculateTargetSpeed();
            
            currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * speedChangeRate);
            
            Vector3 inputDirection = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
            
            if (inputDirection.magnitude >= 0.1f)
            {
                float targetAngle = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg;
                if (cameraFollow != null)
                {
                    targetAngle += cameraFollow.eulerAngles.y;
                }
                
                Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
                
                float controlFactor = isGrounded ? 1f : airControl;
                moveDirection = Vector3.Lerp(moveDirection, moveDir, Time.deltaTime * 10f * controlFactor);
            }
            else
            {
                moveDirection = Vector3.Lerp(moveDirection, Vector3.zero, Time.deltaTime * 5f);
            }
            
            controller.Move(moveDirection * currentSpeed * Time.deltaTime);
            
            ApplyGravity();
        }
        
        private void CalculateTargetSpeed()
        {
            if (moveInput.magnitude < 0.01f)
            {
                targetSpeed = 0f;
                return;
            }
            
            if (isSprinting && isGrounded && currentStamina > 0)
            {
                targetSpeed = sprintSpeed;
                UpdateStamina(-10f * Time.deltaTime); // スタミナ消費
            }
            else if (moveInput.magnitude > 0.7f)
            {
                targetSpeed = runSpeed;
            }
            else
            {
                targetSpeed = walkSpeed;
            }
            
            // 入力の大きさに基づく速度調整
            float inputMagnitude = moveInput.magnitude;
            if (currentSpeed < targetSpeed)
            {
                targetSpeed *= accelerationCurve.Evaluate(inputMagnitude);
            }
            else
            {
                targetSpeed *= decelerationCurve.Evaluate(inputMagnitude);
            }
        }
        
        private void ApplyGravity()
        {
            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f;
            }
            else
            {
                velocity.y += gravity * Time.deltaTime;
                velocity.y = Mathf.Max(velocity.y, -50f);
            }
            
            controller.Move(velocity * Time.deltaTime);
        }
        #endregion
        
        #region Rotation
        private void UpdateRotation()
        {
            if (lookInput.magnitude < 0.01f) return;
            
            // プレイヤーのY軸回転のみ処理
            transform.Rotate(Vector3.up * lookInput.x * mouseSensitivity);
            
            // カメラ制御はイベント経由で委譲
            onLookInputChanged?.Raise(lookInput);
            
            // 従来のカメラ制御も一時的に保持（段階的移行のため）
            if (cameraFollow != null)
            {
                float yInput = invertY ? lookInput.y : -lookInput.y;
                xRotation += yInput * mouseSensitivity;
                xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);
                
                cameraFollow.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            }
        }
        #endregion
        
        #region Jump
        private void UpdateJump()
        {
            jumpBufferCounter = Mathf.Max(0, jumpBufferCounter - Time.deltaTime);
            
            if (jumpBufferCounter > 0 && CanJump())
            {
                PerformJump();
                jumpBufferCounter = 0;
            }
            
            if (isJumping && velocity.y <= 0)
            {
                isJumping = false;
            }
        }
        
        private bool CanJump()
        {
            return (coyoteTimeCounter > 0 || isGrounded) && !isJumping;
        }
        
        private void PerformJump()
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            coyoteTimeCounter = 0;
            isJumping = true;
            
            onPlayerJump?.Raise();
        }
        #endregion
        
        #region State Changes
        private void OnLanded()
        {
            isJumping = false;
            onPlayerLanded?.Raise();
            onGroundedChanged?.Raise(true);
            onStateChangeRequest?.Raise(PlayerState.Landing);
            
            float fallTime = Time.time - lastGroundedTime;
            if (fallTime > 1f)
            {
                // 落下ダメージ計算
                float fallDamage = Mathf.Clamp(fallTime * 10f, 0f, 50f);
                if (fallDamage > 0)
                {
                    TakeDamage(fallDamage);
                }
            }
            
            StartCoroutineManaged("LandingRecovery", LandingRecovery());
        }
        
        private void OnFallStart()
        {
            onGroundedChanged?.Raise(false);
        }
        
        private IEnumerator LandingRecovery()
        {
            yield return new WaitForSeconds(0.3f);
            
            if (isGrounded)
            {
                UpdateMovementState();
            }
        }
        #endregion
        
        #region Event Handlers
        private void OnDamageReceived()
        {
            TakeDamage(10f); // デフォルトダメージ
        }
        
        private void OnDamageAmountReceived(float damage)
        {
            TakeDamage(damage);
        }
        
        private void OnRespawnRequested()
        {
            ResetPlayer();
            transform.position = Vector3.zero; // リスポーン地点
        }
        
        private void OnTeleportRequested(Vector3 position)
        {
            if (controller != null)
            {
                controller.enabled = false;
                transform.position = position;
                controller.enabled = true;
                velocity = Vector3.zero;
            }
        }
        
        private void OnForceStateChange(PlayerState state)
        {
            onStateChangeRequest?.Raise(state);
        }
        
        private void OnEnterCombatMode()
        {
            onStateChangeRequest?.Raise(PlayerState.Combat);
            // TODO: カメラ状態変更イベントの実装
            // onCameraStateChangeRequest?.Raise(CameraState.Combat);
        }
        
        private void OnExitCombatMode()
        {
            UpdateMovementState();
            // TODO: カメラ状態変更イベントの実装
            // onCameraStateChangeRequest?.Raise(CameraState.Follow);
        }
        #endregion
        
        #region Health & Stamina
        private void TakeDamage(float damage)
        {
            currentHealth = Mathf.Max(0, currentHealth - damage);
            onHealthChanged?.Raise(currentHealth);
            
            if (currentHealth <= 0)
            {
                onStateChangeRequest?.Raise(PlayerState.Dead);
            }
        }
        
        private void UpdateStamina(float delta)
        {
            currentStamina = Mathf.Clamp(currentStamina + delta, 0, maxStamina);
            onStaminaChanged?.Raise(currentStamina);
            
            if (currentStamina <= 0 && isSprinting)
            {
                isSprinting = false;
                onPlayerStopSprint?.Raise();
            }
        }
        #endregion
        
        #region Coroutine Management
        private void StartCoroutineManaged(string name, IEnumerator routine)
        {
            StopCoroutineManaged(name);
            var coroutine = StartCoroutine(routine);
            activeCoroutines[name] = coroutine;
        }
        
        private void StopCoroutineManaged(string name)
        {
            if (activeCoroutines.TryGetValue(name, out var coroutine))
            {
                if (coroutine != null)
                    StopCoroutine(coroutine);
                activeCoroutines.Remove(name);
            }
        }
        
        private void StopAllActiveCoroutines()
        {
            foreach (var coroutine in activeCoroutines.Values)
            {
                if (coroutine != null)
                    StopCoroutine(coroutine);
            }
            activeCoroutines.Clear();
        }
        #endregion
        
        #region Events Update
        private float lastReportedSpeed = -1f;
        
        private void UpdateEvents()
        {
            // 速度変更イベント
            if (Mathf.Abs(currentSpeed - lastReportedSpeed) > 0.1f)
            {
                onSpeedChanged?.Raise(currentSpeed);
                lastReportedSpeed = currentSpeed;
            }
            
            // 位置変更イベント（移動時のみ）
            if (currentSpeed > 0.1f)
            {
                onPositionChanged?.Raise(transform.position);
            }
            
            // スタミナ回復
            if (!isSprinting && currentStamina < maxStamina)
            {
                UpdateStamina(5f * Time.deltaTime);
            }
        }
        
        private void NotifyInitialState()
        {
            if (onPlayerDataUpdated != null)
            {
                var playerData = new PlayerDataPayload
                {
                    playerName = "Player",
                    position = transform.position,
                    rotation = transform.rotation,
                    currentHealth = currentHealth,
                    maxHealth = maxHealth,
                    currentStamina = currentStamina,
                    maxStamina = maxStamina,
                    score = 0
                };
                
                onPlayerDataUpdated.Raise(playerData);
            }
            
            onHealthChanged?.Raise(currentHealth);
            onStaminaChanged?.Raise(currentStamina);
            onGroundedChanged?.Raise(isGrounded);
        }
        #endregion
        
        #region Utilities
        private void InitializeCurves()
        {
            if (accelerationCurve == null || accelerationCurve.keys.Length == 0)
            {
                accelerationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
            }
            
            if (decelerationCurve == null || decelerationCurve.keys.Length == 0)
            {
                decelerationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
            }
        }
        
        public void ResetPlayer()
        {
            StopAllActiveCoroutines();
            
            velocity = Vector3.zero;
            moveDirection = Vector3.zero;
            currentSpeed = 0f;
            isJumping = false;
            isSprinting = false;
            currentHealth = maxHealth;
            currentStamina = maxStamina;
            movementFrozen = false;
            currentMovementState = PlayerState.Idle;
            
            onHealthChanged?.Raise(currentHealth);
            onStaminaChanged?.Raise(currentStamina);
            onStateChangeRequest?.Raise(PlayerState.Idle);
        }
        
        private void OnDrawGizmosSelected()
        {
            if (groundCheck != null)
            {
                Gizmos.color = isGrounded ? Color.green : Color.red;
                Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
            }
        }
        #endregion
    }
}