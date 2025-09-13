using UnityEngine;
using UnityEngine.InputSystem;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Commands;
using asterivo.Unity60.Core;
using asterivo.Unity60.Player;
using asterivo.Unity60.Features.Templates.FPS.Weapons;
using asterivo.Unity60.Camera.States;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Features.Templates.FPS.Player
{
    /// <summary>
    /// FPS専用プレイヤーコントローラー
    /// 既存のPlayerControllerを拡張してFPS特有の機能を追加
    /// 武器操作、一人称視点、FPS特有の移動を実装
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerInput))]
    public class FPSPlayerController : MonoBehaviour
    {
        [TabGroup("FPS Player", "Base Components")]
        [LabelText("Base Player Controller")]
        [SerializeField] private PlayerController basePlayerController;
        
        [LabelText("Character Controller")]
        [SerializeField] private CharacterController characterController;
        
        [LabelText("Camera State Machine")]
        [SerializeField] private CameraStateMachine cameraStateMachine;
        
        [TabGroup("FPS Player", "Weapon System")]
        [LabelText("Current Weapon")]
        [SerializeField] private WeaponSystem currentWeapon;
        
        [LabelText("Weapon Holder")]
        [SerializeField] private Transform weaponHolder;
        
        [TabGroup("FPS Player", "Movement Settings")]
        [BoxGroup("FPS Player/Movement Settings/Speed")]
        [LabelText("Walk Speed")]
        [PropertyRange(1f, 10f)]
        [SerializeField] private float walkSpeed = 5f;
        
        [BoxGroup("FPS Player/Movement Settings/Speed")]
        [LabelText("Run Speed")]
        [PropertyRange(5f, 15f)]
        [SerializeField] private float runSpeed = 8f;
        
        [BoxGroup("FPS Player/Movement Settings/Speed")]
        [LabelText("Crouch Speed")]
        [PropertyRange(1f, 5f)]
        [SerializeField] private float crouchSpeed = 2.5f;
        
        [BoxGroup("FPS Player/Movement Settings/Speed")]
        [LabelText("Aim Speed Multiplier")]
        [PropertyRange(0.3f, 0.8f)]
        [SerializeField] private float aimSpeedMultiplier = 0.5f;
        
        [TabGroup("FPS Player", "Jump & Gravity")]
        [BoxGroup("FPS Player/Jump & Gravity/Jump")]
        [LabelText("Jump Height")]
        [PropertyRange(1f, 5f)]
        [SerializeField] private float jumpHeight = 2f;
        
        [BoxGroup("FPS Player/Jump & Gravity/Gravity")]
        [LabelText("Gravity Multiplier")]
        [PropertyRange(1f, 3f)]
        [SerializeField] private float gravityMultiplier = 1.5f;
        
        [TabGroup("FPS Player", "Audio")]
        [LabelText("Footstep Audio Source")]
        [SerializeField] private AudioSource footstepAudioSource;
        
        [LabelText("Footstep Sounds")]
        [SerializeField] private AudioClip[] footstepSounds;
        
        [TabGroup("Events", "FPS Events")]
        [LabelText("On Weapon Changed")]
        [SerializeField] private GameEvent onWeaponChanged;
        
        [LabelText("On Movement State Changed")]
        [SerializeField] private GameEvent onMovementStateChanged;
        
        // Input System
        private PlayerInput playerInput;
        private Vector2 moveInput;
        private Vector2 lookInput;
        private bool isRunning;
        private bool isCrouching;
        private bool isAiming;
        private bool jumpPressed;
        private bool shootPressed;
        private bool reloadPressed;
        
        // Movement variables
        private Vector3 velocity;
        private bool isGrounded;
        private float currentSpeed;
        
        // Health System
        [TabGroup("FPS Player", "Health")]
        [LabelText("Max Health")]
        [PropertyRange(50f, 200f)]
        [SerializeField] private float maxHealth = 100f;
        
        private float currentHealth;
        
        [TabGroup("Debug", "Status")]
        [ReadOnly]
        [ShowInInspector]
        [LabelText("Current State")]
        private string currentState = "Idle";
        
        [ReadOnly]
        [ShowInInspector]
        [LabelText("Health Status")]
        private string healthStatus = "100/100";
        
        [ReadOnly]
        [ShowInInspector]
        [LabelText("Speed")]
        private float debugSpeed = 0f;
        
        private void Awake()
        {
            InitializeComponents();
            InitializeHealth();
        }
        
        private void Start()
        {
            SetupInputCallbacks();
            SwitchToFirstPersonView();
        }
        
        private void Update()
        {
            HandleMovement();
            HandleWeaponInput();
            UpdateGroundCheck();
            UpdateDebugInfo();
        }
        
        private void InitializeComponents()
        {
            if (characterController == null)
                characterController = GetComponent<CharacterController>();
            
            if (playerInput == null)
                playerInput = GetComponent<PlayerInput>();
            
            if (basePlayerController == null)
                basePlayerController = GetComponent<PlayerController>();
            
            if (footstepAudioSource == null)
            {
                footstepAudioSource = gameObject.AddComponent<AudioSource>();
                footstepAudioSource.volume = 0.3f;
                footstepAudioSource.spatialBlend = 1f; // 3D sound
            }
        }
        
        private void InitializeHealth()
        {
            currentHealth = maxHealth;
            UpdateHealthStatus();
        }
        
        private void SetupInputCallbacks()
        {
            var actionMap = playerInput.actions.FindActionMap("Player");
            
            actionMap.FindAction("Move").performed += OnMove;
            actionMap.FindAction("Move").canceled += OnMove;
            
            actionMap.FindAction("Look").performed += OnLook;
            actionMap.FindAction("Look").canceled += OnLook;
            
            actionMap.FindAction("Jump").performed += OnJump;
            actionMap.FindAction("Run").performed += OnRun;
            actionMap.FindAction("Run").canceled += OnRun;
            
            actionMap.FindAction("Crouch").performed += OnCrouch;
            actionMap.FindAction("Crouch").canceled += OnCrouch;
            
            actionMap.FindAction("Aim").performed += OnAim;
            actionMap.FindAction("Aim").canceled += OnAim;
            
            actionMap.FindAction("Shoot").performed += OnShoot;
            actionMap.FindAction("Shoot").canceled += OnShoot;
            
            actionMap.FindAction("Reload").performed += OnReload;
        }
        
        private void HandleMovement()
        {
            // 速度計算
            currentSpeed = CalculateCurrentSpeed();
            
            // 水平移動
            Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
            characterController.Move(move * currentSpeed * Time.deltaTime);
            
            // 重力とジャンプ
            HandleVerticalMovement();
            
            // 足音再生
            HandleFootsteps(move.magnitude);
            
            // 移動状態の更新
            UpdateMovementState();
        }
        
        private float CalculateCurrentSpeed()
        {
            float speed = walkSpeed;
            
            if (isRunning && !isCrouching)
            {
                speed = runSpeed;
            }
            else if (isCrouching)
            {
                speed = crouchSpeed;
            }
            
            // エイム中の速度減少
            if (isAiming)
            {
                speed *= aimSpeedMultiplier;
            }
            
            return speed;
        }
        
        private void HandleVerticalMovement()
        {
            // 地面判定
            isGrounded = characterController.isGrounded;
            
            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f; // 地面に密着させる
            }
            
            // ジャンプ
            if (jumpPressed && isGrounded)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * Physics.gravity.y);
                jumpPressed = false;
            }
            
            // 重力適用
            velocity.y += Physics.gravity.y * gravityMultiplier * Time.deltaTime;
            characterController.Move(velocity * Time.deltaTime);
        }
        
        private void HandleWeaponInput()
        {
            if (currentWeapon == null) return;
            
            // 射撃
            if (shootPressed && currentWeapon.CanShoot)
            {
                Vector3 shootOrigin = cameraStateMachine.MainCamera.transform.position;
                Vector3 shootDirection = cameraStateMachine.MainCamera.transform.forward;
                currentWeapon.Shoot(shootOrigin, shootDirection);
            }
            
            // リロード
            if (reloadPressed)
            {
                currentWeapon.StartReload();
                reloadPressed = false;
            }
        }
        
        private void HandleFootsteps(float moveAmount)
        {
            if (isGrounded && moveAmount > 0.1f && footstepSounds.Length > 0)
            {
                if (!footstepAudioSource.isPlaying)
                {
                    AudioClip footstep = footstepSounds[Random.Range(0, footstepSounds.Length)];
                    footstepAudioSource.PlayOneShot(footstep);
                }
            }
        }
        
        private void UpdateMovementState()
        {
            string newState = "Idle";
            
            if (moveInput.magnitude > 0.1f)
            {
                if (isRunning && !isCrouching)
                    newState = "Running";
                else if (isCrouching)
                    newState = "Crouching";
                else
                    newState = "Walking";
            }
            else if (isCrouching)
            {
                newState = "Crouching";
            }
            
            if (isAiming)
                newState += " + Aiming";
            
            if (currentState != newState)
            {
                currentState = newState;
                onMovementStateChanged?.Raise();
            }
        }
        
        private void UpdateGroundCheck()
        {
            // より正確な地面判定のための追加チェック
            RaycastHit hit;
            isGrounded = Physics.Raycast(transform.position, Vector3.down, out hit, 1.1f);
        }
        
        private void SwitchToFirstPersonView()
        {
            if (cameraStateMachine != null)
            {
                cameraStateMachine.TransitionToState(CameraStateMachine.CameraStateType.FirstPerson);
            }
        }
        
        public void TakeDamage(float damage)
        {
            currentHealth = Mathf.Max(0, currentHealth - damage);
            UpdateHealthStatus();
            
            if (currentHealth <= 0)
            {
                HandleDeath();
            }
        }
        
        public void Heal(float amount)
        {
            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
            UpdateHealthStatus();
        }
        
        private void HandleDeath()
        {
            currentState = "Dead";
            // 死亡処理の実装
        }
        
        private void UpdateHealthStatus()
        {
            healthStatus = $"{currentHealth:F0}/{maxHealth:F0}";
        }
        
        private void UpdateDebugInfo()
        {
            debugSpeed = characterController.velocity.magnitude;
        }
        
        // Input callbacks
        private void OnMove(InputAction.CallbackContext context)
        {
            moveInput = context.ReadValue<Vector2>();
        }
        
        private void OnLook(InputAction.CallbackContext context)
        {
            lookInput = context.ReadValue<Vector2>();
        }
        
        private void OnJump(InputAction.CallbackContext context)
        {
            if (context.performed)
                jumpPressed = true;
        }
        
        private void OnRun(InputAction.CallbackContext context)
        {
            isRunning = context.performed;
        }
        
        private void OnCrouch(InputAction.CallbackContext context)
        {
            isCrouching = context.performed;
        }
        
        private void OnAim(InputAction.CallbackContext context)
        {
            isAiming = context.performed;
            if (currentWeapon != null)
            {
                if (isAiming)
                    currentWeapon.StartAiming();
                else
                    currentWeapon.StopAiming();
            }
        }
        
        private void OnShoot(InputAction.CallbackContext context)
        {
            shootPressed = context.performed;
        }
        
        private void OnReload(InputAction.CallbackContext context)
        {
            if (context.performed)
                reloadPressed = true;
        }
        
        public void EquipWeapon(WeaponSystem weapon)
        {
            if (currentWeapon != null)
            {
                currentWeapon.gameObject.SetActive(false);
            }
            
            currentWeapon = weapon;
            if (currentWeapon != null)
            {
                currentWeapon.transform.SetParent(weaponHolder);
                currentWeapon.transform.localPosition = Vector3.zero;
                currentWeapon.transform.localRotation = Quaternion.identity;
                currentWeapon.gameObject.SetActive(true);
                
                onWeaponChanged?.Raise();
            }
        }
    }
}