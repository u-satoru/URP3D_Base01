using UnityEngine;
using Debug = UnityEngine.Debug;
using UnityEngine.InputSystem;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Commands.Definitions;
using asterivo.Unity60.Core.Audio.Interfaces;
using _Project.Core;
using asterivo.Unity60.Core;
using _Project.Core.Services;
using asterivo.Unity60.Core.Debug;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Player
{
    /// <summary>
    /// プレイヤーの入力を処理し、対応するコマンド定義をイベントとして発行します。
    /// このクラスはUnityのInput Systemと連携し、移動、ジャンプ、スプリントなどのアクションを検知します。
    /// </summary>
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerController : MonoBehaviour
    {
        [TabGroup("Player Control", "Command Output")]
        [LabelText("Command Definition Event")]
        [Tooltip("プレイヤーのアクションに基づいて発行されるコマンド定義イベント")]
        [SerializeField] private CommandDefinitionGameEvent onCommandDefinitionIssued;
        
        [TabGroup("Player Control", "Animation")]
        [LabelText("Movement Animator")]
        [Tooltip("DOTweenアニメーション管理コンポーネント")]
        [SerializeField] private PlayerMovementAnimator movementAnimator;
        
        [TabGroup("Player Control", "Animation")]
        [LabelText("Animator")]
        [Tooltip("キャラクターのAnimatorコンポーネント")]
        [SerializeField] private Animator animator;
        
        [TabGroup("Player Control", "Animation")]
        [LabelText("Use 2D BlendTree")]
        [Tooltip("2D BlendTree（方向性あり）を使用するかどうか")]
        [SerializeField] private bool use2DBlendTree = false;
        
        [TabGroup("Player Control", "Animation")]
        [LabelText("Animation Smooth Time")]
        [PropertyRange(0.05f, 0.5f)]
        [SuffixLabel("s", overlay: true)]
        [Tooltip("アニメーション遷移のスムージング時間")]
        [SerializeField] private float animationSmoothTime = 0.1f;

        [TabGroup("Player Control", "Movement Events")]
        [LabelText("Freeze Movement Listener")]
        [Tooltip("プレイヤーの移動を一時的に無効化するイベントリスナー")]
        [SerializeField] private GameEventListener freezeMovementListener;
        
        [TabGroup("Player Control", "Movement Events")]
        [LabelText("Unfreeze Movement Listener")]
        [Tooltip("プレイヤーの移動無効化を解除するイベントリスナー")]
        [SerializeField] private GameEventListener unfreezeMovementListener;

        [TabGroup("Player Control", "Audio Integration")]
        [Header("Service Dependencies")]
        [Tooltip("ServiceLocatorを優先的に使用するか")]
        [SerializeField] private bool useServiceLocator = true;
        [Tooltip("ステルスオーディオ機能を有効にするか")]
        [SerializeField] private bool enableStealthAudio = true;
        [Tooltip("足音再生を有効にするか")]
        [SerializeField] private bool enableFootsteps = true;

        private PlayerInput playerInput;
        private InputActionMap playerActionMap;
        
        // ✅ Task 2: Service References (移行パターン実装)
        private IAudioService audioService;
        private IStealthAudioService stealthAudioService;
        
        [TabGroup("Player Control", "Debug Info")]
        [ReadOnly]
        [ShowInInspector]
        [LabelText("Audio Service Status")]
        private string audioServiceStatus = "Not Initialized";
        
        [TabGroup("Player Control", "Debug Info")]
        [ReadOnly]
        [ShowInInspector]
        [LabelText("Movement Frozen")]
        private bool movementFrozen = false;
        
        [TabGroup("Player Control", "Debug Info")]
        [ReadOnly]
        [ShowInInspector]
        [LabelText("Sprint Pressed")]
        private bool isSprintPressed = false;

        /// <summary>
        /// スプリントボタンが現在押されているかどうかを取得します。
        /// </summary>
        public bool IsSprintPressed => isSprintPressed;

        /// <summary>
        /// プレイヤーの移動が現在凍結（無効化）されているかどうかを取得します。
        /// </summary>
        public bool IsMovementFrozen => movementFrozen;
        
        // パフォーマンス向上のためパラメータをハッシュ化
        private static readonly int MoveSpeedHash = Animator.StringToHash("MoveSpeed");
        private static readonly int MoveXHash = Animator.StringToHash("MoveX");
        private static readonly int MoveZHash = Animator.StringToHash("MoveZ");
        private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
        private static readonly int IsJumpingHash = Animator.StringToHash("IsJumping");
        private static readonly int IsCrouchingHash = Animator.StringToHash("IsCrouching");
        private static readonly int VerticalVelocityHash = Animator.StringToHash("VerticalVelocity");
        private static readonly int JumpTriggerHash = Animator.StringToHash("JumpTrigger");
        private static readonly int LandTriggerHash = Animator.StringToHash("LandTrigger");
        
        // スムーズなアニメーション遷移用
        private Vector2 animationVelocity;
        private Vector2 animationSmoothVelocity;
        
        // 接地判定用
        private Rigidbody playerRigidbody;

        private void Awake()
        {
            playerInput = GetComponent<PlayerInput>();
            
            // MovementAnimatorを自動取得していない場合は取得
            if (movementAnimator == null)
                movementAnimator = GetComponent<PlayerMovementAnimator>();
                
            // Animator の自動取得
            if (animator == null)
                animator = GetComponent<Animator>();
                
            // Rigidbody の取得
            playerRigidbody = GetComponent<Rigidbody>();
                
            SetupInputCallbacks();
            SetupMovementEventListeners();
        }

        /// <summary>
        /// ✅ Task 2: Step 3.6 移行パターン実装
        /// オーディオサービスの初期化（ServiceLocator優先、Singleton フォールバック）
        /// </summary>
        private void Start() 
        {
            InitializeAudioServices();
        }

        private void OnDestroy()
        {
            CleanupInputCallbacks();
            CleanupMovementEventListeners();
        }

        #region Audio Service Integration (Task 2: Step 3.6)

        /// <summary>
        /// オーディオサービスの初期化（移行パターン実装）
        /// </summary>
        private void InitializeAudioServices()
        {
            audioServiceStatus = "Initializing...";
            
            // 新しい方法での取得 (推奨) - ServiceLocator優先
            if (useServiceLocator && FeatureFlags.UseServiceLocator) 
            {
                try
                {
                    audioService = ServiceLocator.GetService<IAudioService>();
                    
                    if (enableStealthAudio)
                    {
                        stealthAudioService = ServiceLocator.GetService<IStealthAudioService>();
                    }
                    
                    if (audioService != null)
                    {
                        audioServiceStatus = "ServiceLocator: Success";
                        
                        if (FeatureFlags.EnableDebugLogging)
                        {
                            EventLogger.Log("[PlayerController] Using ServiceLocator for audio services");
                        }
                        return; // 正常に取得できたので終了
                    }
                }
                catch (System.Exception ex)
                {
                    EventLogger.LogWarning($"[PlayerController] ServiceLocator audio service failed: {ex.Message}");
                }
            }
            
            // 従来の方法 (後方互換性) - Singleton フォールバック
            if (!FeatureFlags.DisableLegacySingletons)
            {
                try 
                {
#pragma warning disable CS0618 // Obsolete warning suppression during migration
                    var audioManager = FindFirstObjectByType<asterivo.Unity60.Core.Audio.AudioManager>();
                    if (audioManager != null)
                    {
                        audioService = audioManager;
                        audioServiceStatus = "Legacy Singleton: Success";
                    }
                    
                    if (enableStealthAudio)
                    {
                        var stealthCoordinator = FindFirstObjectByType<asterivo.Unity60.Core.Audio.StealthAudioCoordinator>();
                        if (stealthCoordinator != null)
                        {
                            stealthAudioService = stealthCoordinator;
                        }
                    }
#pragma warning restore CS0618
                    
                    if (FeatureFlags.EnableMigrationWarnings)
                    {
                        EventLogger.LogWarning("[PlayerController] Using legacy Singleton access");
                    }
                }
                catch (System.Exception ex)
                {
                    EventLogger.LogError($"[PlayerController] Legacy audio service fallback failed: {ex.Message}");
                }
            }
            
            // サービス取得の検証
            if (audioService == null)
            {
                audioServiceStatus = "Failed: No Audio Service";
                EventLogger.LogError("[PlayerController] Failed to get IAudioService");
            }
            
            if (enableStealthAudio && stealthAudioService == null)
            {
                EventLogger.LogWarning("[PlayerController] Failed to get IStealthAudioService");
            }
        }
        
        /// <summary>
        /// 足音再生（移行パターン実装例）
        /// </summary>
        private void PlayFootstep() 
        {
            if (!enableFootsteps || audioService == null) return;
            
            // 基本足音再生
            audioService.PlaySound("footstep", transform.position, 0.7f);
            
            // ステルスオーディオとの連携
            if (enableStealthAudio && stealthAudioService != null)
            {
                // 足音の強度を動的に計算（スプリント時は強く）
                float intensity = isSprintPressed ? 0.8f : 0.4f;
                
                stealthAudioService.CreateFootstep(transform.position, intensity, "concrete");
            }
        }
        
        /// <summary>
        /// ランディング音再生
        /// </summary>
        private void PlayLandingSound()
        {
            if (!enableFootsteps || audioService == null) return;
            
            audioService.PlaySound("landing", transform.position, 0.9f);
            
            if (enableStealthAudio && stealthAudioService != null)
            {
                // ランディングは比較的大きな音
                stealthAudioService.CreateFootstep(transform.position, 0.9f, "concrete");
            }
        }

        #endregion

        /// <summary>
        /// Input Systemのアクションにコールバックメソッドを登録します。
        /// </summary>
        private void SetupInputCallbacks()
        {
            playerActionMap = playerInput.currentActionMap;

            var moveAction = playerActionMap.FindAction("Move");
            if (moveAction != null)
            {
                moveAction.performed += OnMove;
                moveAction.canceled += OnMoveCanceled;
            }

            var jumpAction = playerActionMap.FindAction("Jump");
            if (jumpAction != null)
            {
                jumpAction.started += OnJump;
            }
            
            var sprintAction = playerActionMap.FindAction("Sprint");
            if (sprintAction != null)
            {
                sprintAction.started += OnSprintStarted;
                sprintAction.canceled += OnSprintCanceled;
            }
        }

        /// <summary>
        /// 登録したInput Systemのコールバックを解除します。
        /// </summary>
        private void CleanupInputCallbacks()
        {
            var moveAction = playerActionMap?.FindAction("Move");
            if (moveAction != null)
            {
                moveAction.performed -= OnMove;
                moveAction.canceled -= OnMoveCanceled;
            }

            var jumpAction = playerActionMap?.FindAction("Jump");
            if (jumpAction != null)
            {
                jumpAction.started -= OnJump;
            }
            
            var sprintAction = playerActionMap?.FindAction("Sprint");
            if (sprintAction != null)
            {
                sprintAction.started -= OnSprintStarted;
                sprintAction.canceled -= OnSprintCanceled;
            }
        }

        /// <summary>
        /// 移動入力が検出されたときに呼び出されます。
        /// </summary>
        private void OnMove(InputAction.CallbackContext context)
        {
            if (movementFrozen) return;
            var moveInput = context.ReadValue<Vector2>();
            
            // スムーズなアニメーション遷移（急激な変化を防ぐ）
            animationVelocity = Vector2.SmoothDamp(
                animationVelocity, 
                moveInput, 
                ref animationSmoothVelocity, 
                animationSmoothTime
            );
            
            // 1. BlendTree アニメーション更新
            if (use2DBlendTree)
            {
                Update2DBlendTree(animationVelocity);
            }
            else
            {
                Update1DBlendTree(animationVelocity.magnitude);
            }
            
            // 2. コマンド発行（既存システム）
            var definition = new MoveCommandDefinition(MoveCommandDefinition.MoveType.Walk, new Vector3(moveInput.x, 0, moveInput.y));
            onCommandDefinitionIssued?.Raise(definition);
        }

        /// <summary>
        /// 移動入力がキャンセルされたときに呼び出されます。
        /// </summary>
        private void OnMoveCanceled(InputAction.CallbackContext context)
        {
            if (movementFrozen) return;
            
            // アニメーション停止のためのスムージング
            animationVelocity = Vector2.SmoothDamp(
                animationVelocity, 
                Vector2.zero, 
                ref animationSmoothVelocity, 
                animationSmoothTime
            );
            
            // BlendTree アニメーション更新
            if (use2DBlendTree)
            {
                Update2DBlendTree(Vector2.zero);
            }
            else
            {
                Update1DBlendTree(0f);
            }
            
            var definition = new MoveCommandDefinition(MoveCommandDefinition.MoveType.Walk, Vector3.zero);
            onCommandDefinitionIssued?.Raise(definition);
        }

        /// <summary>
        /// ジャンプ入力が検出されたときに呼び出されます。
        /// </summary>
        private void OnJump(InputAction.CallbackContext context)
        {
            if (movementFrozen) return;
            
            // 1. BlendTreeでキャラクターアニメーション（縦軸の速度制御）
            if (animator != null)
            {
                animator.SetTrigger(JumpTriggerHash);
                animator.SetBool(IsGroundedHash, false);
            }
            
            // 2. DOTweenで追加演出（既存システム）
            if (movementAnimator != null)
            {
                movementAnimator.AnimateJump();
            }
            
            // 3. コマンドパターンでゲームロジック
            var definition = new JumpCommandDefinition();
            onCommandDefinitionIssued?.Raise(definition);
        }
        
        /// <summary>
        /// スプリント入力が開始されたときに呼び出されます。
        /// </summary>
        private void OnSprintStarted(InputAction.CallbackContext context)
        {
            if (movementFrozen) return;
            isSprintPressed = true;
        }
        
        /// <summary>
        /// スプリント入力がキャンセルされたときに呼び出されます。
        /// </summary>
        private void OnSprintCanceled(InputAction.CallbackContext context)
        {
            isSprintPressed = false;
        }
        
        /// <summary>
        /// 移動凍結を制御するイベントリスナーを設定します。
        /// </summary>
        private void SetupMovementEventListeners()
        {
            if (freezeMovementListener != null)
            {
                freezeMovementListener.Response.AddListener(OnFreezeMovement);
            }
            
            if (unfreezeMovementListener != null)
            {
                unfreezeMovementListener.Response.AddListener(OnUnfreezeMovement);
            }
        }
        
        /// <summary>
        /// 設定した移動凍結イベントリスナーを解除します。
        /// </summary>
        private void CleanupMovementEventListeners()
        {
            if (freezeMovementListener != null)
            {
                freezeMovementListener.Response.RemoveListener(OnFreezeMovement);
            }
            
            if (unfreezeMovementListener != null)
            {
                unfreezeMovementListener.Response.RemoveListener(OnUnfreezeMovement);
            }
        }
        
        /// <summary>
        /// プレイヤーの移動を凍結（無効化）するイベントハンドラです。
        /// </summary>
        private void OnFreezeMovement()
        {
            movementFrozen = true;
            Debug.Log("Player movement frozen");
        }
        
        /// <summary>
        /// プレイヤーの移動凍結を解除するイベントハンドラです。
        /// </summary>
        private void OnUnfreezeMovement()
        {
            movementFrozen = false;
            Debug.Log("Player movement unfrozen");
        }
        
        #if UNITY_EDITOR
        [TabGroup("Player Control", "Debug Controls")]
        [Button("Test Jump Animation")]
        private void TestJumpAnimation()
        {
            if (Application.isPlaying && movementAnimator != null)
            {
                movementAnimator.AnimateJump();
            }
        }
        
        [TabGroup("Player Control", "Debug Controls")]
        [Button("Test Damage Animation")]
        private void TestDamageAnimation()
        {
            if (Application.isPlaying && movementAnimator != null)
            {
                movementAnimator.AnimateDamageShake();
            }
        }
        #endif
        
        /// <summary>
        /// Update処理で継続的にアニメーション状態を更新
        /// </summary>
        private void Update()
        {
            UpdateAnimationStates();
        }
        
        /// <summary>
        /// アニメーション状態の更新処理
        /// </summary>
        private void UpdateAnimationStates()
        {
            if (animator == null || playerRigidbody == null) return;
            
            // 縦方向速度をジャンプ・落下アニメーションに反映
            float verticalVelocity = playerRigidbody.linearVelocity.y;
            animator.SetFloat(VerticalVelocityHash, verticalVelocity);
            
            // 接地状態の更新
            bool isGrounded = CheckGroundContact();
            animator.SetBool(IsGroundedHash, isGrounded);
        }
        
        /// <summary>
        /// 1D BlendTreeの更新処理
        /// </summary>
        private void Update1DBlendTree(float speed)
        {
            if (animator == null) return;
            
            // スプリント状態を考慮した速度計算
            float finalSpeed = speed;
            if (IsSprintPressed && speed > 0.1f)
            {
                finalSpeed = Mathf.Lerp(0.7f, 1.0f, speed); // スプリント時は0.7-1.0の範囲
            }
            
            animator.SetFloat(MoveSpeedHash, finalSpeed);
        }
        
        /// <summary>
        /// 2D BlendTreeの更新処理
        /// </summary>
        private void Update2DBlendTree(Vector2 velocity)
        {
            if (animator == null) return;
            
            // 2D方向を考慮したBlendTree制御
            animator.SetFloat(MoveXHash, velocity.x);
            animator.SetFloat(MoveZHash, velocity.y);
            animator.SetFloat(MoveSpeedHash, velocity.magnitude);
        }
        
        /// <summary>
        /// 接地判定の実装
        /// </summary>
        private bool CheckGroundContact()
        {
            // レイキャストによる接地判定（キャラクターの足元から下向きにレイを飛ばす）
            return Physics.Raycast(transform.position, Vector3.down, 1.1f);
        }
    }
}
