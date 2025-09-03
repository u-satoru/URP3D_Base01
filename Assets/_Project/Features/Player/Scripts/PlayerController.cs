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

        // TODO: これらのイベントをリッスンしてmovementFrozenを制御する
        // [SerializeField] private GameEventListener freezeMovementListener;
        // [SerializeField] private GameEventListener unfreezeMovementListener;

        private PlayerInput playerInput;
        private InputActionMap playerActionMap;
        private bool movementFrozen = false;

        private void Awake()
        {
            playerInput = GetComponent<PlayerInput>();
            SetupInputCallbacks();
        }

        private void OnDestroy()
        {
            CleanupInputCallbacks();
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
    }
}
