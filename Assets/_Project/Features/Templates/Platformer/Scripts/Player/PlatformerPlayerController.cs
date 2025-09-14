using UnityEngine;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Commands;
using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Features.Player.Scripts;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Features.Templates.Platformer
{
    /// <summary>
    /// プラットフォーマー特化プレイヤーコントローラー
    /// 3Dプラットフォームアクションに最適化されたジャンプ物理と移動制御
    /// コヨーテタイム、ジャンプバッファ、可変ジャンプ高度を実装
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(Animator))]
    public class PlatformerPlayerController : MonoBehaviour
    {
        #region Inspector Settings

        [TabGroup("Platformer", "Movement")]
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private float sprintSpeed = 9f;
        [SerializeField] private float acceleration = 10f;
        [SerializeField] private float deceleration = 10f;
        [SerializeField, Range(0f, 1f)] private float airControlRatio = 0.5f;

        [TabGroup("Platformer", "Jumping")]
        [Header("Jump Physics")]
        [SerializeField] private float jumpForce = 8f;
        [SerializeField] private float gravity = -9.81f;
        [SerializeField] private float maxFallSpeed = -15f;
        [SerializeField] private float jumpGravityMultiplier = 0.5f; // 上昇時の軽い重力
        [SerializeField] private float fallGravityMultiplier = 1.5f; // 降下時の強い重力

        [TabGroup("Platformer", "Advanced")]
        [Header("Advanced Jump Mechanics")]
        [SerializeField] private float coyoteTime = 0.2f; // 地面を離れてもジャンプできる時間
        [SerializeField] private float jumpBufferTime = 0.2f; // ジャンプ入力の先行入力時間
        [SerializeField] private int maxJumps = 2; // 多段ジャンプ
        [SerializeField] private float doubleJumpForce = 6f;
        [SerializeField] private bool enableWallJump = false;
        [SerializeField] private float wallJumpForce = 7f;

        [TabGroup("Platformer", "Detection")]
        [Header("Ground Detection")]
        [SerializeField] private LayerMask groundLayerMask = 1;
        [SerializeField] private float groundCheckDistance = 0.1f;
        [SerializeField] private float groundCheckRadius = 0.3f;
        [SerializeField] private Transform groundCheckPoint;

        [TabGroup("Platformer", "Animation")]
        [Header("Animation Parameters")]
        [SerializeField] private string speedParameterName = "Speed";
        [SerializeField] private string isGroundedParameterName = "IsGrounded";
        [SerializeField] private string isJumpingParameterName = "IsJumping";
        [SerializeField] private string isFallingParameterName = "IsFalling";
        [SerializeField] private string jumpTriggerName = "Jump";

        [TabGroup("Platformer", "Debug")]
        [Header("Debug Information")]
        [SerializeField, ReadOnly] private bool isGrounded;
        [SerializeField, ReadOnly] private bool wasGroundedLastFrame;
        [SerializeField, ReadOnly] private float currentSpeed;
        [SerializeField, ReadOnly] private float verticalVelocity;
        [SerializeField, ReadOnly] private int currentJumpCount;
        [SerializeField, ReadOnly] private float coyoteTimeCounter;
        [SerializeField, ReadOnly] private float jumpBufferCounter;

        #endregion

        #region Components

        private CharacterController characterController;
        private Animator animator;
        private Transform cameraTransform;

        #endregion

        #region Movement Variables

        private Vector3 moveDirection;
        private Vector3 velocity;
        private Vector2 inputVector;
        private bool jumpInput;
        private bool sprintInput;

        // Advanced jump mechanics
        private bool canCoyoteJump;
        private bool hasJumpedThisFrame;

        // Wall jump detection
        private bool isTouchingWall;
        private Vector3 wallNormal;

        #endregion

        #region Events

        [Header("Events")]
        [SerializeField] private GameEvent onPlayerJump;
        [SerializeField] private GameEvent onPlayerLand;
        [SerializeField] private GameEvent onPlayerDoubleJump;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeComponents();
        }

        private void Start()
        {
            InitializeGroundCheck();
        }

        private void Update()
        {
            HandleInput();
            UpdateGroundDetection();
            UpdateJumpMechanics();
            UpdateMovement();
            UpdateAnimation();
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            HandleWallDetection(hit);
        }

        #endregion

        #region Initialization

        private void InitializeComponents()
        {
            characterController = GetComponent<CharacterController>();
            animator = GetComponent<Animator>();

            // カメラ参照の取得
            if (Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
            }
        }

        private void InitializeGroundCheck()
        {
            if (groundCheckPoint == null)
            {
                // Ground check pointが設定されていない場合は自動で作成
                GameObject groundCheck = new GameObject("GroundCheckPoint");
                groundCheck.transform.SetParent(transform);
                groundCheck.transform.localPosition = Vector3.down * (characterController.height * 0.5f + groundCheckDistance);
                groundCheckPoint = groundCheck.transform;
            }
        }

        #endregion

        #region Input Handling

        private void HandleInput()
        {
            // 移動入力の取得
            inputVector.x = Input.GetAxis("Horizontal");
            inputVector.y = Input.GetAxis("Vertical");

            // ジャンプ入力の取得
            if (Input.GetButtonDown("Jump"))
            {
                jumpInput = true;
                jumpBufferCounter = jumpBufferTime;
            }

            // スプリント入力の取得
            sprintInput = Input.GetButton("Fire3"); // Left Shift
        }

        #endregion

        #region Ground Detection

        private void UpdateGroundDetection()
        {
            wasGroundedLastFrame = isGrounded;

            // 地面検知
            Vector3 checkPosition = groundCheckPoint.position;
            isGrounded = Physics.CheckSphere(checkPosition, groundCheckRadius, groundLayerMask);

            // 着地イベントの発行
            if (!wasGroundedLastFrame && isGrounded)
            {
                OnLanded();
            }

            // コヨーテタイムの更新
            if (isGrounded)
            {
                coyoteTimeCounter = coyoteTime;
                canCoyoteJump = true;
                currentJumpCount = 0;
            }
            else
            {
                coyoteTimeCounter -= Time.deltaTime;
                if (coyoteTimeCounter <= 0)
                {
                    canCoyoteJump = false;
                }
            }
        }

        #endregion

        #region Jump Mechanics

        private void UpdateJumpMechanics()
        {
            // ジャンプバッファの減少
            if (jumpBufferCounter > 0)
            {
                jumpBufferCounter -= Time.deltaTime;
            }

            hasJumpedThisFrame = false;

            // ジャンプ判定
            if (jumpBufferCounter > 0)
            {
                // 通常ジャンプまたはコヨーテジャンプ
                if (isGrounded || canCoyoteJump)
                {
                    PerformJump(jumpForce);
                    jumpBufferCounter = 0;
                    canCoyoteJump = false;
                    hasJumpedThisFrame = true;
                }
                // 多段ジャンプ
                else if (currentJumpCount < maxJumps - 1)
                {
                    PerformDoubleJump();
                    jumpBufferCounter = 0;
                    hasJumpedThisFrame = true;
                }
                // 壁ジャンプ
                else if (enableWallJump && isTouchingWall && currentJumpCount < maxJumps)
                {
                    PerformWallJump();
                    jumpBufferCounter = 0;
                    hasJumpedThisFrame = true;
                }
            }

            // 重力の適用
            ApplyGravity();

            // ジャンプ入力のリセット
            jumpInput = false;
        }

        private void PerformJump(float force)
        {
            verticalVelocity = force;
            currentJumpCount++;
            
            // イベント発行
            onPlayerJump?.Raise();
            
            // デバッグログ
            Debug.Log($"[PlatformerPlayer] Jump performed: {force}");
        }

        private void PerformDoubleJump()
        {
            verticalVelocity = doubleJumpForce;
            currentJumpCount++;
            
            // イベント発行
            onPlayerDoubleJump?.Raise();
            
            // デバッグログ
            Debug.Log($"[PlatformerPlayer] Double jump performed: {doubleJumpForce}");
        }

        private void PerformWallJump()
        {
            verticalVelocity = wallJumpForce;
            currentJumpCount++;
            
            // 壁から離れる方向に移動
            Vector3 wallJumpDirection = wallNormal.normalized;
            velocity.x = wallJumpDirection.x * wallJumpForce * 0.5f;
            velocity.z = wallJumpDirection.z * wallJumpForce * 0.5f;
            
            Debug.Log($"[PlatformerPlayer] Wall jump performed: {wallJumpForce}");
        }

        private void ApplyGravity()
        {
            if (isGrounded && verticalVelocity < 0)
            {
                verticalVelocity = -2f; // 地面に軽く押し付ける
            }
            else
            {
                float gravityMultiplier = 1f;
                
                // ジャンプ中（上昇）は軽い重力
                if (verticalVelocity > 0 && !Input.GetButton("Jump"))
                {
                    gravityMultiplier = jumpGravityMultiplier;
                }
                // 降下中は強い重力
                else if (verticalVelocity < 0)
                {
                    gravityMultiplier = fallGravityMultiplier;
                }

                verticalVelocity += gravity * gravityMultiplier * Time.deltaTime;
                
                // 最大落下速度の制限
                if (verticalVelocity < maxFallSpeed)
                {
                    verticalVelocity = maxFallSpeed;
                }
            }
        }

        #endregion

        #region Movement

        private void UpdateMovement()
        {
            // カメラ基準の移動方向計算
            Vector3 cameraForward = cameraTransform != null ? cameraTransform.forward : Vector3.forward;
            Vector3 cameraRight = cameraTransform != null ? cameraTransform.right : Vector3.right;
            
            // Y軸成分を除去（水平移動のみ）
            cameraForward.y = 0;
            cameraRight.y = 0;
            cameraForward.Normalize();
            cameraRight.Normalize();

            // 入力に基づく移動方向の計算
            Vector3 desiredDirection = cameraForward * inputVector.y + cameraRight * inputVector.x;
            
            // 現在の移動速度を取得
            float targetSpeed = sprintInput ? sprintSpeed : moveSpeed;
            float speedMultiplier = isGrounded ? 1f : airControlRatio;
            targetSpeed *= speedMultiplier;

            // 加速・減速の適用
            if (desiredDirection.magnitude > 0.1f)
            {
                // 加速
                moveDirection = Vector3.Lerp(moveDirection, desiredDirection.normalized * targetSpeed, acceleration * Time.deltaTime);
            }
            else
            {
                // 減速
                moveDirection = Vector3.Lerp(moveDirection, Vector3.zero, deceleration * Time.deltaTime);
            }

            // 垂直方向の速度を設定
            velocity = moveDirection;
            velocity.y = verticalVelocity;

            // キャラクターコントローラーによる移動
            characterController.Move(velocity * Time.deltaTime);

            // 現在の速度を記録（デバッグ用）
            currentSpeed = new Vector3(velocity.x, 0, velocity.z).magnitude;
        }

        #endregion

        #region Wall Detection

        private void HandleWallDetection(ControllerColliderHit hit)
        {
            // 壁判定（一定以上の角度の壁面のみ）
            float wallAngle = Vector3.Angle(hit.normal, Vector3.up);
            if (wallAngle > 45f && wallAngle < 135f)
            {
                isTouchingWall = true;
                wallNormal = hit.normal;
            }
            else
            {
                isTouchingWall = false;
            }
        }

        #endregion

        #region Animation

        private void UpdateAnimation()
        {
            if (animator == null) return;

            // アニメーションパラメータの更新
            animator.SetFloat(speedParameterName, currentSpeed);
            animator.SetBool(isGroundedParameterName, isGrounded);
            animator.SetBool(isJumpingParameterName, verticalVelocity > 0.1f);
            animator.SetBool(isFallingParameterName, verticalVelocity < -0.1f);

            // ジャンプトリガー
            if (hasJumpedThisFrame)
            {
                animator.SetTrigger(jumpTriggerName);
            }
        }

        #endregion

        #region Events

        private void OnLanded()
        {
            // 着地イベントの発行
            onPlayerLand?.Raise();
            
            Debug.Log("[PlatformerPlayer] Player landed");
        }

        #endregion

        #region Public API

        /// <summary>
        /// 重力値を設定
        /// </summary>
        public void SetGravity(float newGravity)
        {
            gravity = newGravity;
        }

        /// <summary>
        /// ジャンプ力を設定
        /// </summary>
        public void SetJumpForce(float newJumpForce)
        {
            jumpForce = newJumpForce;
        }

        /// <summary>
        /// 地面レイヤーマスクを設定
        /// </summary>
        public void SetGroundLayerMask(LayerMask mask)
        {
            groundLayerMask = mask;
        }

        /// <summary>
        /// コヨーテタイムを設定
        /// </summary>
        public void SetCoyoteTime(float time)
        {
            coyoteTime = time;
        }

        /// <summary>
        /// ジャンプバッファ時間を設定
        /// </summary>
        public void SetJumpBufferTime(float time)
        {
            jumpBufferTime = time;
        }

        /// <summary>
        /// プレイヤーが地面にいるかどうかを取得
        /// </summary>
        public bool IsGrounded()
        {
            return isGrounded;
        }

        /// <summary>
        /// 現在の移動速度を取得
        /// </summary>
        public float GetCurrentSpeed()
        {
            return currentSpeed;
        }

        /// <summary>
        /// プレイヤーを指定位置にテレポート
        /// </summary>
        public void TeleportTo(Vector3 position)
        {
            characterController.enabled = false;
            transform.position = position;
            characterController.enabled = true;
            
            // 速度をリセット
            velocity = Vector3.zero;
            verticalVelocity = 0f;
            moveDirection = Vector3.zero;
        }

        #endregion

        #region Debug Gizmos

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (groundCheckPoint != null)
            {
                // 地面検知範囲の表示
                Gizmos.color = isGrounded ? Color.green : Color.red;
                Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
            }

            // 移動方向の表示
            if (Application.isPlaying)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(transform.position, moveDirection.normalized * 2f);
                
                // 速度ベクトルの表示
                Gizmos.color = Color.yellow;
                Gizmos.DrawRay(transform.position, velocity.normalized * 3f);
            }

            // 壁ジャンプの法線表示
            if (isTouchingWall)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawRay(transform.position, wallNormal * 2f);
            }
        }
#endif

        #endregion
    }
}