using UnityEngine;
using Debug = UnityEngine.Debug;
using UnityEngine.InputSystem;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Commands.Definitions;
using asterivo.Unity60.Core.Audio.Interfaces;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Debug;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Features.Player
{
    /// <summary>
    /// 繝励Ξ繧､繝､繝ｼ縺ｮ蜈･蜉帙ｒ蜃ｦ逅・＠縲∝ｯｾ蠢懊☆繧九さ繝槭Φ繝牙ｮ夂ｾｩ繧偵う繝吶Φ繝医→縺励※逋ｺ陦後＠縺ｾ縺吶・
    /// 縺薙・繧ｯ繝ｩ繧ｹ縺ｯUnity縺ｮInput System縺ｨ騾｣謳ｺ縺励∫ｧｻ蜍輔√ず繝｣繝ｳ繝励√せ繝励Μ繝ｳ繝医↑縺ｩ縺ｮ繧｢繧ｯ繧ｷ繝ｧ繝ｳ繧呈､懃衍縺励∪縺吶・
    /// </summary>
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerController : MonoBehaviour
    {
        [TabGroup("Player Control", "Command Output")]
        [LabelText("Command Definition Event")]
        [Tooltip("繝励Ξ繧､繝､繝ｼ縺ｮ繧｢繧ｯ繧ｷ繝ｧ繝ｳ縺ｫ蝓ｺ縺･縺・※逋ｺ陦後＆繧後ｋ繧ｳ繝槭Φ繝牙ｮ夂ｾｩ繧､繝吶Φ繝・)]
        [SerializeField] private CommandDefinitionGameEvent onCommandDefinitionIssued;
        
        [TabGroup("Player Control", "Animation")]
        [LabelText("Movement Animator")]
        [Tooltip("DOTween繧｢繝九Γ繝ｼ繧ｷ繝ｧ繝ｳ邂｡逅・さ繝ｳ繝昴・繝阪Φ繝・)]
        [SerializeField] private PlayerMovementAnimator movementAnimator;
        
        [TabGroup("Player Control", "Animation")]
        [LabelText("Animator")]
        [Tooltip("繧ｭ繝｣繝ｩ繧ｯ繧ｿ繝ｼ縺ｮAnimator繧ｳ繝ｳ繝昴・繝阪Φ繝・)]
        [SerializeField] private Animator animator;
        
        [TabGroup("Player Control", "Animation")]
        [LabelText("Use 2D BlendTree")]
        [Tooltip("2D BlendTree・域婿蜷第ｧ縺ゅｊ・峨ｒ菴ｿ逕ｨ縺吶ｋ縺九←縺・°")]
        [SerializeField] private bool use2DBlendTree = false;
        
        [TabGroup("Player Control", "Animation")]
        [LabelText("Animation Smooth Time")]
        [PropertyRange(0.05f, 0.5f)]
        [SuffixLabel("s", overlay: true)]
        [Tooltip("繧｢繝九Γ繝ｼ繧ｷ繝ｧ繝ｳ驕ｷ遘ｻ縺ｮ繧ｹ繝繝ｼ繧ｸ繝ｳ繧ｰ譎る俣")]
        [SerializeField] private float animationSmoothTime = 0.1f;

        [TabGroup("Player Control", "Movement Events")]
        [LabelText("Freeze Movement Listener")]
        [Tooltip("繝励Ξ繧､繝､繝ｼ縺ｮ遘ｻ蜍輔ｒ荳譎ら噪縺ｫ辟｡蜉ｹ蛹悶☆繧九う繝吶Φ繝医Μ繧ｹ繝翫・")]
        [SerializeField] private GameEventListener freezeMovementListener;
        
        [TabGroup("Player Control", "Movement Events")]
        [LabelText("Unfreeze Movement Listener")]
        [Tooltip("繝励Ξ繧､繝､繝ｼ縺ｮ遘ｻ蜍慕┌蜉ｹ蛹悶ｒ隗｣髯､縺吶ｋ繧､繝吶Φ繝医Μ繧ｹ繝翫・")]
        [SerializeField] private GameEventListener unfreezeMovementListener;

        [TabGroup("Player Control", "Audio Integration")]
        [Header("Service Dependencies")]
        [Tooltip("ServiceLocator繧貞━蜈育噪縺ｫ菴ｿ逕ｨ縺吶ｋ縺・)]
        [SerializeField] private bool useServiceLocator = true;
        [Tooltip("繧ｹ繝・Ν繧ｹ繧ｪ繝ｼ繝・ぅ繧ｪ讖溯・繧呈怏蜉ｹ縺ｫ縺吶ｋ縺・)]
        [SerializeField] private bool enableStealthAudio = true;
        [Tooltip("雜ｳ髻ｳ蜀咲函繧呈怏蜉ｹ縺ｫ縺吶ｋ縺・)]
        [SerializeField] private bool enableFootsteps = true;

        private PlayerInput playerInput;
        private InputActionMap playerActionMap;
        
        // 笨・Task 2: Service References (遘ｻ陦後ヱ繧ｿ繝ｼ繝ｳ螳溯｣・
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
        /// 繧ｹ繝励Μ繝ｳ繝医・繧ｿ繝ｳ縺檎樟蝨ｨ謚ｼ縺輔ｌ縺ｦ縺・ｋ縺九←縺・°繧貞叙蠕励＠縺ｾ縺吶・
        /// </summary>
        public bool IsSprintPressed => isSprintPressed;

        /// <summary>
        /// 繝励Ξ繧､繝､繝ｼ縺ｮ遘ｻ蜍輔′迴ｾ蝨ｨ蜃咲ｵ撰ｼ育┌蜉ｹ蛹厄ｼ峨＆繧後※縺・ｋ縺九←縺・°繧貞叙蠕励＠縺ｾ縺吶・
        /// </summary>
        public bool IsMovementFrozen => movementFrozen;
        
        // 繝代ヵ繧ｩ繝ｼ繝槭Φ繧ｹ蜷台ｸ翫・縺溘ａ繝代Λ繝｡繝ｼ繧ｿ繧偵ワ繝・す繝･蛹・
        private static readonly int MoveSpeedHash = Animator.StringToHash("MoveSpeed");
        private static readonly int MoveXHash = Animator.StringToHash("MoveX");
        private static readonly int MoveZHash = Animator.StringToHash("MoveZ");
        private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
        private static readonly int IsJumpingHash = Animator.StringToHash("IsJumping");
        private static readonly int IsCrouchingHash = Animator.StringToHash("IsCrouching");
        private static readonly int VerticalVelocityHash = Animator.StringToHash("VerticalVelocity");
        private static readonly int JumpTriggerHash = Animator.StringToHash("JumpTrigger");
        private static readonly int LandTriggerHash = Animator.StringToHash("LandTrigger");
        
        // 繧ｹ繝繝ｼ繧ｺ縺ｪ繧｢繝九Γ繝ｼ繧ｷ繝ｧ繝ｳ驕ｷ遘ｻ逕ｨ
        private Vector2 animationVelocity;
        private Vector2 animationSmoothVelocity;
        
        // 謗･蝨ｰ蛻､螳夂畑
        private Rigidbody playerRigidbody;

        private void Awake()
        {
            playerInput = GetComponent<PlayerInput>();
            
            // MovementAnimator繧定・蜍募叙蠕励＠縺ｦ縺・↑縺・ｴ蜷医・蜿門ｾ・
            if (movementAnimator == null)
                movementAnimator = GetComponent<PlayerMovementAnimator>();
                
            // Animator 縺ｮ閾ｪ蜍募叙蠕・
            if (animator == null)
                animator = GetComponent<Animator>();
                
            // Rigidbody 縺ｮ蜿門ｾ・
            playerRigidbody = GetComponent<Rigidbody>();
                
            SetupInputCallbacks();
            SetupMovementEventListeners();
        }

        /// <summary>
        /// 笨・Task 2: Step 3.6 遘ｻ陦後ヱ繧ｿ繝ｼ繝ｳ螳溯｣・
        /// 繧ｪ繝ｼ繝・ぅ繧ｪ繧ｵ繝ｼ繝薙せ縺ｮ蛻晄悄蛹厄ｼ・erviceLocator蜆ｪ蜈医ヾingleton 繝輔か繝ｼ繝ｫ繝舌ャ繧ｯ・・
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
        /// 繧ｪ繝ｼ繝・ぅ繧ｪ繧ｵ繝ｼ繝薙せ縺ｮ蛻晄悄蛹厄ｼ育ｧｻ陦後ヱ繧ｿ繝ｼ繝ｳ螳溯｣・ｼ・
        /// </summary>
        private void InitializeAudioServices()
        {
            audioServiceStatus = "Initializing...";
            
            // 譁ｰ縺励＞譁ｹ豕輔〒縺ｮ蜿門ｾ・(謗ｨ螂ｨ) - ServiceLocator蜆ｪ蜈・
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
                            ServiceLocator.GetService<IEventLogger>()?.Log("[PlayerController] Using ServiceLocator for audio services");
                        }
                        return; // 豁｣蟶ｸ縺ｫ蜿門ｾ励〒縺阪◆縺ｮ縺ｧ邨ゆｺ・
                    }
                }
                catch (System.Exception ex)
                {
                    ServiceLocator.GetService<IEventLogger>()?.LogWarning($"[PlayerController] ServiceLocator audio service failed: {ex.Message}");
                }
            }
            
            // 蠕捺擂縺ｮ譁ｹ豕・(蠕梧婿莠呈鋤諤ｧ) - Singleton 繝輔か繝ｼ繝ｫ繝舌ャ繧ｯ
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
                        EventLogger.LogWarningStatic("[PlayerController] Using legacy Singleton access");
                    }
                }
                catch (System.Exception ex)
                {
                    ServiceLocator.GetService<IEventLogger>()?.LogError($"[PlayerController] Legacy audio service fallback failed: {ex.Message}");
                }
            }
            
            // 繧ｵ繝ｼ繝薙せ蜿門ｾ励・讀懆ｨｼ
            if (audioService == null)
            {
                audioServiceStatus = "Failed: No Audio Service";
                EventLogger.LogErrorStatic("[PlayerController] Failed to get IAudioService");
            }
            
            if (enableStealthAudio && stealthAudioService == null)
            {
                EventLogger.LogWarningStatic("[PlayerController] Failed to get IStealthAudioService");
            }
        }
        
        /// <summary>
        /// 雜ｳ髻ｳ蜀咲函・育ｧｻ陦後ヱ繧ｿ繝ｼ繝ｳ螳溯｣・ｾ具ｼ・
        /// </summary>
        private void PlayFootstep() 
        {
            if (!enableFootsteps || audioService == null) return;
            
            // 蝓ｺ譛ｬ雜ｳ髻ｳ蜀咲函
            audioService.PlaySound("footstep", transform.position, 0.7f);
            
            // 繧ｹ繝・Ν繧ｹ繧ｪ繝ｼ繝・ぅ繧ｪ縺ｨ縺ｮ騾｣謳ｺ
            if (enableStealthAudio && stealthAudioService != null)
            {
                // 雜ｳ髻ｳ縺ｮ蠑ｷ蠎ｦ繧貞虚逧・↓險育ｮ暦ｼ医せ繝励Μ繝ｳ繝域凾縺ｯ蠑ｷ縺擾ｼ・
                float intensity = isSprintPressed ? 0.8f : 0.4f;
                
                stealthAudioService.CreateFootstep(transform.position, intensity, "concrete");
            }
        }
        
        /// <summary>
        /// 繝ｩ繝ｳ繝・ぅ繝ｳ繧ｰ髻ｳ蜀咲函
        /// </summary>
        private void PlayLandingSound()
        {
            if (!enableFootsteps || audioService == null) return;
            
            audioService.PlaySound("landing", transform.position, 0.9f);
            
            if (enableStealthAudio && stealthAudioService != null)
            {
                // 繝ｩ繝ｳ繝・ぅ繝ｳ繧ｰ縺ｯ豈碑ｼ・噪螟ｧ縺阪↑髻ｳ
                stealthAudioService.CreateFootstep(transform.position, 0.9f, "concrete");
            }
        }

        #endregion

        /// <summary>
        /// Input System縺ｮ繧｢繧ｯ繧ｷ繝ｧ繝ｳ縺ｫ繧ｳ繝ｼ繝ｫ繝舌ャ繧ｯ繝｡繧ｽ繝・ラ繧堤匳骭ｲ縺励∪縺吶・
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
        /// 逋ｻ骭ｲ縺励◆Input System縺ｮ繧ｳ繝ｼ繝ｫ繝舌ャ繧ｯ繧定ｧ｣髯､縺励∪縺吶・
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
        /// 遘ｻ蜍募・蜉帙′讀懷・縺輔ｌ縺溘→縺阪↓蜻ｼ縺ｳ蜃ｺ縺輔ｌ縺ｾ縺吶・
        /// </summary>
        private void OnMove(InputAction.CallbackContext context)
        {
            if (movementFrozen) return;
            var moveInput = context.ReadValue<Vector2>();
            
            // 繧ｹ繝繝ｼ繧ｺ縺ｪ繧｢繝九Γ繝ｼ繧ｷ繝ｧ繝ｳ驕ｷ遘ｻ・域･豼縺ｪ螟牙喧繧帝亟縺撰ｼ・
            animationVelocity = Vector2.SmoothDamp(
                animationVelocity, 
                moveInput, 
                ref animationSmoothVelocity, 
                animationSmoothTime
            );
            
            // 1. BlendTree 繧｢繝九Γ繝ｼ繧ｷ繝ｧ繝ｳ譖ｴ譁ｰ
            if (use2DBlendTree)
            {
                Update2DBlendTree(animationVelocity);
            }
            else
            {
                Update1DBlendTree(animationVelocity.magnitude);
            }
            
            // 2. 繧ｳ繝槭Φ繝臥匱陦鯉ｼ域里蟄倥す繧ｹ繝・Β・・
            var definition = new MoveCommandDefinition(MoveCommandDefinition.MoveType.Walk, new Vector3(moveInput.x, 0, moveInput.y));
            onCommandDefinitionIssued?.Raise(definition);
        }

        /// <summary>
        /// 遘ｻ蜍募・蜉帙′繧ｭ繝｣繝ｳ繧ｻ繝ｫ縺輔ｌ縺溘→縺阪↓蜻ｼ縺ｳ蜃ｺ縺輔ｌ縺ｾ縺吶・
        /// </summary>
        private void OnMoveCanceled(InputAction.CallbackContext context)
        {
            if (movementFrozen) return;
            
            // 繧｢繝九Γ繝ｼ繧ｷ繝ｧ繝ｳ蛛懈ｭ｢縺ｮ縺溘ａ縺ｮ繧ｹ繝繝ｼ繧ｸ繝ｳ繧ｰ
            animationVelocity = Vector2.SmoothDamp(
                animationVelocity, 
                Vector2.zero, 
                ref animationSmoothVelocity, 
                animationSmoothTime
            );
            
            // BlendTree 繧｢繝九Γ繝ｼ繧ｷ繝ｧ繝ｳ譖ｴ譁ｰ
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
        /// 繧ｸ繝｣繝ｳ繝怜・蜉帙′讀懷・縺輔ｌ縺溘→縺阪↓蜻ｼ縺ｳ蜃ｺ縺輔ｌ縺ｾ縺吶・
        /// </summary>
        private void OnJump(InputAction.CallbackContext context)
        {
            if (movementFrozen) return;
            
            // 1. BlendTree縺ｧ繧ｭ繝｣繝ｩ繧ｯ繧ｿ繝ｼ繧｢繝九Γ繝ｼ繧ｷ繝ｧ繝ｳ・育ｸｦ霆ｸ縺ｮ騾溷ｺｦ蛻ｶ蠕｡・・
            if (animator != null)
            {
                animator.SetTrigger(JumpTriggerHash);
                animator.SetBool(IsGroundedHash, false);
            }
            
            // 2. DOTween縺ｧ霑ｽ蜉貍泌・・域里蟄倥す繧ｹ繝・Β・・
            if (movementAnimator != null)
            {
                movementAnimator.AnimateJump();
            }
            
            // 3. 繧ｳ繝槭Φ繝峨ヱ繧ｿ繝ｼ繝ｳ縺ｧ繧ｲ繝ｼ繝繝ｭ繧ｸ繝・け
            var definition = new JumpCommandDefinition();
            onCommandDefinitionIssued?.Raise(definition);
        }
        
        /// <summary>
        /// 繧ｹ繝励Μ繝ｳ繝亥・蜉帙′髢句ｧ九＆繧後◆縺ｨ縺阪↓蜻ｼ縺ｳ蜃ｺ縺輔ｌ縺ｾ縺吶・
        /// </summary>
        private void OnSprintStarted(InputAction.CallbackContext context)
        {
            if (movementFrozen) return;
            isSprintPressed = true;
        }
        
        /// <summary>
        /// 繧ｹ繝励Μ繝ｳ繝亥・蜉帙′繧ｭ繝｣繝ｳ繧ｻ繝ｫ縺輔ｌ縺溘→縺阪↓蜻ｼ縺ｳ蜃ｺ縺輔ｌ縺ｾ縺吶・
        /// </summary>
        private void OnSprintCanceled(InputAction.CallbackContext context)
        {
            isSprintPressed = false;
        }
        
        /// <summary>
        /// 遘ｻ蜍募㍾邨舌ｒ蛻ｶ蠕｡縺吶ｋ繧､繝吶Φ繝医Μ繧ｹ繝翫・繧定ｨｭ螳壹＠縺ｾ縺吶・
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
        /// 險ｭ螳壹＠縺溽ｧｻ蜍募㍾邨舌う繝吶Φ繝医Μ繧ｹ繝翫・繧定ｧ｣髯､縺励∪縺吶・
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
        /// 繝励Ξ繧､繝､繝ｼ縺ｮ遘ｻ蜍輔ｒ蜃咲ｵ撰ｼ育┌蜉ｹ蛹厄ｼ峨☆繧九う繝吶Φ繝医ワ繝ｳ繝峨Λ縺ｧ縺吶・
        /// </summary>
        private void OnFreezeMovement()
        {
            movementFrozen = true;
            ProjectDebug.Log("Player movement frozen");
        }
        
        /// <summary>
        /// 繝励Ξ繧､繝､繝ｼ縺ｮ遘ｻ蜍募㍾邨舌ｒ隗｣髯､縺吶ｋ繧､繝吶Φ繝医ワ繝ｳ繝峨Λ縺ｧ縺吶・
        /// </summary>
        private void OnUnfreezeMovement()
        {
            movementFrozen = false;
            ProjectDebug.Log("Player movement unfrozen");
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
        /// Update蜃ｦ逅・〒邯咏ｶ夂噪縺ｫ繧｢繝九Γ繝ｼ繧ｷ繝ｧ繝ｳ迥ｶ諷九ｒ譖ｴ譁ｰ
        /// </summary>
        private void Update()
        {
            UpdateAnimationStates();
        }
        
        /// <summary>
        /// 繧｢繝九Γ繝ｼ繧ｷ繝ｧ繝ｳ迥ｶ諷九・譖ｴ譁ｰ蜃ｦ逅・
        /// </summary>
        private void UpdateAnimationStates()
        {
            if (animator == null || playerRigidbody == null) return;
            
            // 邵ｦ譁ｹ蜷鷹溷ｺｦ繧偵ず繝｣繝ｳ繝励・關ｽ荳九い繝九Γ繝ｼ繧ｷ繝ｧ繝ｳ縺ｫ蜿肴丐
            float verticalVelocity = playerRigidbody.linearVelocity.y;
            animator.SetFloat(VerticalVelocityHash, verticalVelocity);
            
            // 謗･蝨ｰ迥ｶ諷九・譖ｴ譁ｰ
            bool isGrounded = CheckGroundContact();
            animator.SetBool(IsGroundedHash, isGrounded);
        }
        
        /// <summary>
        /// 1D BlendTree縺ｮ譖ｴ譁ｰ蜃ｦ逅・
        /// </summary>
        private void Update1DBlendTree(float speed)
        {
            if (animator == null) return;
            
            // 繧ｹ繝励Μ繝ｳ繝育憾諷九ｒ閠・・縺励◆騾溷ｺｦ險育ｮ・
            float finalSpeed = speed;
            if (IsSprintPressed && speed > 0.1f)
            {
                finalSpeed = Mathf.Lerp(0.7f, 1.0f, speed); // 繧ｹ繝励Μ繝ｳ繝域凾縺ｯ0.7-1.0縺ｮ遽・峇
            }
            
            animator.SetFloat(MoveSpeedHash, finalSpeed);
        }
        
        /// <summary>
        /// 2D BlendTree縺ｮ譖ｴ譁ｰ蜃ｦ逅・
        /// </summary>
        private void Update2DBlendTree(Vector2 velocity)
        {
            if (animator == null) return;
            
            // 2D譁ｹ蜷代ｒ閠・・縺励◆BlendTree蛻ｶ蠕｡
            animator.SetFloat(MoveXHash, velocity.x);
            animator.SetFloat(MoveZHash, velocity.y);
            animator.SetFloat(MoveSpeedHash, velocity.magnitude);
        }
        
        /// <summary>
        /// 謗･蝨ｰ蛻､螳壹・螳溯｣・
        /// </summary>
        private bool CheckGroundContact()
        {
            // 繝ｬ繧､繧ｭ繝｣繧ｹ繝医↓繧医ｋ謗･蝨ｰ蛻､螳夲ｼ医く繝｣繝ｩ繧ｯ繧ｿ繝ｼ縺ｮ雜ｳ蜈・°繧我ｸ句髄縺阪↓繝ｬ繧､繧帝｣帙・縺呻ｼ・
            return Physics.Raycast(transform.position, Vector3.down, 1.1f);
        }
    }
}


