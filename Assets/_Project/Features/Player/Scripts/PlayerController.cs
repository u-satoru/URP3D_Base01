using UnityEngine;
using UnityEngine.InputSystem;
using asterivo.Unity60.Core.Events;

namespace asterivo.Unity60.Player
{
    /// <summary>
    /// プレイヤーの入力を検知し、対応するコマンド定義を発行する責務を持つ
    /// </summary>
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Command Output")]
        [SerializeField] private CommandDefinitionGameEvent onCommandDefinitionIssued;

        [Header("Movement Control Events")]
        [SerializeField] private GameEventListener freezeMovementListener;
        [SerializeField] private GameEventListener unfreezeMovementListener;

        private PlayerInput playerInput;
        private InputActionMap playerActionMap;
        private bool movementFrozen = false;
        private bool isSprintPressed = false;

        private void Awake()
        {
            playerInput = GetComponent<PlayerInput>();
            SetupInputCallbacks();
            SetupMovementEventListeners();
        }

        private void OnDestroy()
        {
            CleanupInputCallbacks();
            CleanupMovementEventListeners();
        }

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

        private void OnMove(InputAction.CallbackContext context)
        {
            if (movementFrozen) return;
            var moveInput = context.ReadValue<Vector2>();
            var definition = new asterivo.Unity60.Player.Commands.MoveCommandDefinition(moveInput);
            onCommandDefinitionIssued?.Raise(definition);
        }

        private void OnMoveCanceled(InputAction.CallbackContext context)
        {
            if (movementFrozen) return;
            var definition = new asterivo.Unity60.Player.Commands.MoveCommandDefinition(Vector2.zero);
            onCommandDefinitionIssued?.Raise(definition);
        }

        private void OnJump(InputAction.CallbackContext context)
        {
            if (movementFrozen) return;
            var definition = new asterivo.Unity60.Player.Commands.JumpCommandDefinition();
            onCommandDefinitionIssued?.Raise(definition);
        }
        
        private void OnSprintStarted(InputAction.CallbackContext context)
        {
            if (movementFrozen) return;
            isSprintPressed = true;
        }
        
        private void OnSprintCanceled(InputAction.CallbackContext context)
        {
            isSprintPressed = false;
        }
        
        /// <summary>
        /// スプリント状態を取得（状態マシンから参照用）
        /// </summary>
        public bool IsSprintPressed => isSprintPressed;
        
        /// <summary>
        /// 移動凍結状態を取得
        /// </summary>
        public bool IsMovementFrozen => movementFrozen;
        
        /// <summary>
        /// 移動イベントリスナーの設定
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
        /// 移動イベントリスナーのクリーンアップ
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
        /// 移動凍結イベントハンドラ
        /// </summary>
        private void OnFreezeMovement()
        {
            movementFrozen = true;
            Debug.Log("Player movement frozen");
        }
        
        /// <summary>
        /// 移動凍結解除イベントハンドラ
        /// </summary>
        private void OnUnfreezeMovement()
        {
            movementFrozen = false;
            Debug.Log("Player movement unfrozen");
        }
    }
}
