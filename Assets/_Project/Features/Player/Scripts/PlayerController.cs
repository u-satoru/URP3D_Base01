using UnityEngine;
using UnityEngine.InputSystem;
using asterivo.Unity60.Core.Events;
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

        [TabGroup("Player Control", "Movement Events")]
        [LabelText("Freeze Movement Listener")]
        [Tooltip("プレイヤーの移動を一時的に無効化するイベントリスナー")]
        [SerializeField] private GameEventListener freezeMovementListener;
        
        [TabGroup("Player Control", "Movement Events")]
        [LabelText("Unfreeze Movement Listener")]
        [Tooltip("プレイヤーの移動無効化を解除するイベントリスナー")]
        [SerializeField] private GameEventListener unfreezeMovementListener;

        private PlayerInput playerInput;
        private InputActionMap playerActionMap;
        
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

        private void Awake()
        {
            playerInput = GetComponent<PlayerInput>();
            
            // MovementAnimatorを自動取得していない場合は取得
            if (movementAnimator == null)
                movementAnimator = GetComponent<PlayerMovementAnimator>();
                
            SetupInputCallbacks();
            SetupMovementEventListeners();
        }

        private void OnDestroy()
        {
            CleanupInputCallbacks();
            CleanupMovementEventListeners();
        }

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
            var definition = new asterivo.Unity60.Player.Commands.MoveCommandDefinition(moveInput);
            onCommandDefinitionIssued?.Raise(definition);
        }

        /// <summary>
        /// 移動入力がキャンセルされたときに呼び出されます。
        /// </summary>
        private void OnMoveCanceled(InputAction.CallbackContext context)
        {
            if (movementFrozen) return;
            var definition = new asterivo.Unity60.Player.Commands.MoveCommandDefinition(Vector2.zero);
            onCommandDefinitionIssued?.Raise(definition);
        }

        /// <summary>
        /// ジャンプ入力が検出されたときに呼び出されます。
        /// </summary>
        private void OnJump(InputAction.CallbackContext context)
        {
            if (movementFrozen) return;
            
            // DOTweenアニメーションがある場合は実行
            if (movementAnimator != null)
            {
                movementAnimator.AnimateJump();
            }
            
            var definition = new asterivo.Unity60.Player.Commands.JumpCommandDefinition();
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
    }
}
