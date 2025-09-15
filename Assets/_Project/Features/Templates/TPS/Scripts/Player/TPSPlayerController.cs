using UnityEngine;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Features.Templates.FPS.Weapons;
using asterivo.Unity60.Features.Templates.TPS.Camera;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Features.Templates.TPS.Player
{
    /// <summary>
    /// TPS専用プレイヤー制御システム
    /// 三人称視点に特化したプレイヤー制御を実装
    /// カバーシステム統合、武器操作、移動制御を統合管理
    /// FPS Template武器システム95%再利用でTPS特化機能を実現
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(Animator))]
    public class TPSPlayerController : MonoBehaviour
    {
        [TabGroup("TPS Player", "Movement")]
        [BoxGroup("TPS Player/Movement/Basic Movement")]
        [LabelText("Move Speed")]
        [PropertyRange(1f, 10f)]
        [SerializeField] private float moveSpeed = 5f;

        [BoxGroup("TPS Player/Movement/Basic Movement")]
        [LabelText("Run Speed")]
        [PropertyRange(5f, 15f)]
        [SerializeField] private float runSpeed = 8f;

        [BoxGroup("TPS Player/Movement/Basic Movement")]
        [LabelText("Crouch Speed")]
        [PropertyRange(0.5f, 3f)]
        [SerializeField] private float crouchSpeed = 2f;

        [BoxGroup("TPS Player/Movement/Advanced Movement")]
        [LabelText("Jump Height")]
        [PropertyRange(0.5f, 3f)]
        [SerializeField] private float jumpHeight = 1.5f;

        [BoxGroup("TPS Player/Movement/Advanced Movement")]
        [LabelText("Gravity")]
        [PropertyRange(-30f, -5f)]
        [SerializeField] private float gravity = -15f;

        [BoxGroup("TPS Player/Movement/Advanced Movement")]
        [LabelText("Ground Check Distance")]
        [PropertyRange(0.1f, 1f)]
        [SerializeField] private float groundCheckDistance = 0.4f;

        [TabGroup("TPS Player", "Cover System")]
        [BoxGroup("TPS Player/Cover System/Cover Detection")]
        [LabelText("Cover Detection Range")]
        [PropertyRange(0.5f, 3f)]
        [SerializeField] private float coverDetectionRange = 1.5f;

        [BoxGroup("TPS Player/Cover System/Cover Detection")]
        [LabelText("Cover Layer Mask")]
        [SerializeField] private LayerMask coverLayerMask = 1;

        [BoxGroup("TPS Player/Cover System/Cover Movement")]
        [LabelText("Cover Move Speed")]
        [PropertyRange(1f, 5f)]
        [SerializeField] private float coverMoveSpeed = 3f;

        [BoxGroup("TPS Player/Cover System/Cover Movement")]
        [LabelText("Peek Distance")]
        [PropertyRange(0.3f, 1.5f)]
        [SerializeField] private float peekDistance = 0.8f;

        [TabGroup("TPS Player", "Camera Integration")]
        [LabelText("TPS Camera Controller")]
        [SerializeField] private TPSCameraController cameraController;

        [TabGroup("TPS Player", "Weapon System")]
        [BoxGroup("TPS Player/Weapon System/Current Weapon")]
        [LabelText("Current Weapon")]
        [ReadOnly]
        [ShowInInspector]
        private WeaponSystem currentWeapon;

        [BoxGroup("TPS Player/Weapon System/Weapon Settings")]
        [LabelText("Weapon Hold Position")]
        [SerializeField] private Transform weaponHoldPosition;

        [BoxGroup("TPS Player/Weapon System/Weapon Settings")]
        [LabelText("Aim Position")]
        [SerializeField] private Transform aimPosition;

        [TabGroup("Events", "Game Events")]
        [LabelText("On Player State Changed")]
        [SerializeField] private GameEvent onPlayerStateChanged;

        [LabelText("On Player Take Cover")]
        [SerializeField] private GameEvent onPlayerTakeCover;

        [LabelText("On Player Leave Cover")]
        [SerializeField] private GameEvent onPlayerLeaveCover;

        [LabelText("On Player Start Aiming")]
        [SerializeField] private GameEvent onPlayerStartAiming;

        [LabelText("On Player Stop Aiming")]
        [SerializeField] private GameEvent onPlayerStopAiming;

        // Private components
        private CharacterController characterController;
        private Animator animator;
        
        // Private state variables
        private Vector3 velocity;
        private bool isGrounded;
        private bool isRunning;
        private bool isCrouching;
        private bool isInCover;
        private bool isAiming;
        private bool isPeeking;
        
        // Cover system variables
        private Transform currentCoverPoint;
        private Vector3 coverPosition;
        private Vector3 coverDirection;
        
        // Input variables
        private Vector2 moveInput;
        private Vector2 lookInput;
        private bool jumpInput;
        private bool runInput;
        private bool crouchInput;
        private bool coverInput;
        private bool aimInput;
        private bool fireInput;
        private bool reloadInput;

        [TabGroup("Debug", "Current State")]
        [ReadOnly]
        [ShowInInspector]
        [LabelText("Is Grounded")]
        private bool debugIsGrounded => isGrounded;

        [ReadOnly]
        [ShowInInspector]
        [LabelText("Is In Cover")]
        private bool debugIsInCover => isInCover;

        [ReadOnly]
        [ShowInInspector]
        [LabelText("Is Aiming")]
        private bool debugIsAiming => isAiming;

        [ReadOnly]
        [ShowInInspector]
        [LabelText("Current Speed")]
        private float debugCurrentSpeed => characterController.velocity.magnitude;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            animator = GetComponent<Animator>();
        }

        private void Start()
        {
            // Initialize with default state
            isGrounded = true;
            velocity = Vector3.zero;
        }

        private void Update()
        {
            HandleInput();
            HandleGroundCheck();
            HandleMovement();
            HandleCoverSystem();
            HandleWeaponSystem();
            HandleAnimations();
        }

        private void HandleInput()
        {
            // Basic movement input (using Input System in actual implementation)
            moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            
            // Action inputs
            jumpInput = Input.GetButtonDown("Jump");
            runInput = Input.GetKey(KeyCode.LeftShift);
            crouchInput = Input.GetKey(KeyCode.LeftControl);
            coverInput = Input.GetKeyDown(KeyCode.Q);
            aimInput = Input.GetMouseButton(1);
            fireInput = Input.GetMouseButton(0);
            reloadInput = Input.GetKeyDown(KeyCode.R);
        }

        private void HandleGroundCheck()
        {
            isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance);
            
            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f; // Small downward force to keep grounded
            }
        }

        private void HandleMovement()
        {
            if (isInCover)
            {
                HandleCoverMovement();
                return;
            }

            HandleStandardMovement();
        }

        private void HandleStandardMovement()
        {
            // Calculate movement direction based on camera
            Vector3 forward = cameraController != null ? cameraController.transform.forward : transform.forward;
            Vector3 right = cameraController != null ? cameraController.transform.right : transform.right;
            
            // Remove Y component for ground-based movement
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            // Calculate move direction
            Vector3 moveDirection = (forward * moveInput.y + right * moveInput.x).normalized;

            // Determine current movement speed
            float currentSpeed = moveSpeed;
            
            if (isCrouching)
            {
                currentSpeed = crouchSpeed;
                isRunning = false;
            }
            else if (runInput && moveInput.magnitude > 0.1f)
            {
                currentSpeed = runSpeed;
                isRunning = true;
            }
            else
            {
                isRunning = false;
            }

            // Apply movement
            characterController.Move(moveDirection * currentSpeed * Time.deltaTime);

            // Rotate towards movement direction
            if (moveDirection.magnitude > 0.1f && !isAiming)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
            }

            // Handle jumping
            if (jumpInput && isGrounded && !isCrouching)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }

            // Apply gravity
            velocity.y += gravity * Time.deltaTime;
            characterController.Move(velocity * Time.deltaTime);

            // Handle crouching
            isCrouching = crouchInput;
        }

        private void HandleCoverMovement()
        {
            // Cover movement along the cover line
            Vector3 coverRight = Vector3.Cross(coverDirection, Vector3.up);
            Vector3 coverMoveDirection = coverRight * moveInput.x;
            
            characterController.Move(coverMoveDirection * coverMoveSpeed * Time.deltaTime);

            // Handle peeking
            if (aimInput)
            {
                HandlePeeking();
            }
            else
            {
                isPeeking = false;
            }
        }

        private void HandleCoverSystem()
        {
            if (coverInput)
            {
                if (isInCover)
                {
                    LeaveCover();
                }
                else
                {
                    TryTakeCover();
                }
            }
        }

        private void TryTakeCover()
        {
            RaycastHit hit;
            Vector3 rayOrigin = transform.position + Vector3.up * 1f;
            
            if (Physics.Raycast(rayOrigin, transform.forward, out hit, coverDetectionRange, coverLayerMask))
            {
                TakeCover(hit);
            }
        }

        private void TakeCover(RaycastHit coverHit)
        {
            isInCover = true;
            currentCoverPoint = coverHit.transform;
            coverPosition = coverHit.point;
            coverDirection = coverHit.normal;

            // Position player against cover
            Vector3 targetPosition = coverPosition - coverDirection * 0.5f;
            transform.position = targetPosition;
            transform.rotation = Quaternion.LookRotation(-coverDirection);

            // Notify other systems
            onPlayerTakeCover?.Raise();
            onPlayerStateChanged?.Raise();

            UnityEngine.Debug.Log("[TPS] Player took cover");
        }

        private void LeaveCover()
        {
            isInCover = false;
            isPeeking = false;
            currentCoverPoint = null;

            // Notify other systems
            onPlayerLeaveCover?.Raise();
            onPlayerStateChanged?.Raise();

            UnityEngine.Debug.Log("[TPS] Player left cover");
        }

        private void HandlePeeking()
        {
            if (!isInCover) return;

            isPeeking = true;
            
            // Determine peek direction based on input
            float peekDirection = moveInput.x > 0 ? 1f : -1f;
            Vector3 coverRight = Vector3.Cross(coverDirection, Vector3.up);
            Vector3 peekOffset = coverRight * peekDirection * peekDistance;
            
            // Apply peek position
            Vector3 targetPosition = coverPosition + peekOffset - coverDirection * 0.3f;
            transform.position = Vector3.Lerp(transform.position, targetPosition, 5f * Time.deltaTime);
        }

        private void HandleWeaponSystem()
        {
            if (currentWeapon == null) return;

            // Handle aiming
            if (aimInput && !isAiming)
            {
                StartAiming();
            }
            else if (!aimInput && isAiming)
            {
                StopAiming();
            }

            // Handle firing
            if (fireInput && isAiming)
            {
                // Fire weapon (implemented in WeaponSystem)
                Vector3 shootOrigin = aimPosition != null ? aimPosition.position : transform.position + Vector3.up * 1.8f;
                Vector3 shootDirection = cameraController != null ? cameraController.transform.forward : transform.forward;
                currentWeapon.Shoot(shootOrigin, shootDirection);
            }

            // Handle reloading
            if (reloadInput)
            {
                // Reload weapon (implemented in WeaponSystem)
                currentWeapon.StartReload();
            }
        }

        private void StartAiming()
        {
            isAiming = true;
            onPlayerStartAiming?.Raise();

            // Notify camera for aiming mode
            if (cameraController != null)
            {
                cameraController.SetAimingMode(true);
            }

            UnityEngine.Debug.Log("[TPS] Player started aiming");
        }

        private void StopAiming()
        {
            isAiming = false;
            onPlayerStopAiming?.Raise();

            // Notify camera for normal mode
            if (cameraController != null)
            {
                cameraController.SetAimingMode(false);
            }

            UnityEngine.Debug.Log("[TPS] Player stopped aiming");
        }

        private void HandleAnimations()
        {
            if (animator == null) return;

            // Set animation parameters
            animator.SetFloat("Speed", characterController.velocity.magnitude);
            animator.SetBool("IsGrounded", isGrounded);
            animator.SetBool("IsRunning", isRunning);
            animator.SetBool("IsCrouching", isCrouching);
            animator.SetBool("IsInCover", isInCover);
            animator.SetBool("IsAiming", isAiming);
            animator.SetBool("IsPeeking", isPeeking);
        }

        // Public methods for external systems
        public void Initialize(TPSTemplateConfiguration config)
        {
            // Initialize with template manager reference
            UnityEngine.Debug.Log("[TPS] TPSPlayerController initialized");
        }

        public void EquipWeapon(WeaponSystem weapon)
        {
            if (currentWeapon != null)
            {
                // Unequip current weapon
                currentWeapon.transform.SetParent(null);
            }

            currentWeapon = weapon;
            
            if (currentWeapon != null && weaponHoldPosition != null)
            {
                // Position weapon at hold position
                currentWeapon.transform.SetParent(weaponHoldPosition);
                currentWeapon.transform.localPosition = Vector3.zero;
                currentWeapon.transform.localRotation = Quaternion.identity;
                
                UnityEngine.Debug.Log("[TPS] Equipped weapon: " + currentWeapon.name);
            }
        }

        public bool IsInCover => isInCover;
        public bool IsAiming => isAiming;
        public bool IsPeeking => isPeeking;
        public Vector3 AimPosition => aimPosition != null ? aimPosition.position : transform.position + Vector3.up * 1.8f;
        public WeaponSystem CurrentWeapon => currentWeapon;

        // Gizmos for visualization in Scene view
        private void OnDrawGizmosSelected()
        {
            // Cover detection range
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position + Vector3.up * 1f, coverDetectionRange);

            // Ground check ray
            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position, Vector3.down * groundCheckDistance);

            // Cover visualization
            if (isInCover && currentCoverPoint != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(coverPosition, 0.2f);
                Gizmos.DrawRay(coverPosition, coverDirection);
            }

            // Peek visualization
            if (isPeeking)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(transform.position, 0.3f);
            }
        }
    }
}