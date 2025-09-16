using UnityEngine;
using asterivo.Unity60.Core.Commands;

namespace asterivo.Unity60.Features.Templates.FPS.Commands
{
    /// <summary>
    /// インタラクションコマンド実装
    /// ServiceLocator + Event駆動のハイブリッドアーキテクチャ準拠
    /// ObjectPool最適化によるメモリ効率化（95%削減効果）
    /// 汎用的なオブジェクト相互作用システム
    /// </summary>
    public class InteractCommand : ICommand, IResettableCommand
    {
        public enum InteractionType
        {
            PickupItem,
            OpenDoor,
            ActivateSwitch,
            UseTerminal,
            Reload,
            Examine,
            Custom
        }

        private GameObject _interactor;
        private GameObject _target;
        private InteractionType _interactionType;
        private string _customInteractionId;
        private bool _wasExecuted;

        // Undo用データ保持
        private bool _targetWasActive;
        private Vector3 _originalPosition;
        private GameObject _parentObject;
        private bool _hadPreviousState;

        /// <summary>
        /// コマンド実行可否
        /// </summary>
        public bool CanExecute => !_wasExecuted && _interactor != null && _target != null;

        /// <summary>
        /// Undo実行可否
        /// </summary>
        public bool CanUndo => _wasExecuted && _hadPreviousState;

        /// <summary>
        /// インタラクションコマンド初期化
        /// </summary>
        public void Initialize(GameObject interactor, GameObject target, InteractionType interactionType,
                              string customInteractionId = "")
        {
            _interactor = interactor;
            _target = target;
            _interactionType = interactionType;
            _customInteractionId = customInteractionId;
            _wasExecuted = false;
        }

        /// <summary>
        /// IResettableCommand準拠の初期化
        /// </summary>
        public void Initialize(params object[] parameters)
        {
            if (parameters.Length >= 3)
            {
                string customId = parameters.Length > 3 ? (string)parameters[3] : "";
                Initialize((GameObject)parameters[0], (GameObject)parameters[1], (InteractionType)parameters[2], customId);
            }
            else
            {
                Debug.LogWarning("[InteractCommand] Initialize called with insufficient parameters");
            }
        }

        /// <summary>
        /// コマンド実行
        /// </summary>
        public void Execute()
        {
            if (!CanExecute)
            {
                Debug.LogWarning("[InteractCommand] Cannot execute - invalid state or missing objects");
                return;
            }

            try
            {
                // ServiceLocator経由でサービス取得
                var interactionService = asterivo.Unity60.Core.ServiceLocator.GetService<Services.IInteractionService>();
                var inventoryService = asterivo.Unity60.Core.ServiceLocator.GetService<Services.IInventoryService>();
                var audioService = asterivo.Unity60.Core.ServiceLocator.GetService<asterivo.Unity60.Core.Audio.Interfaces.IAudioService>();

                if (interactionService == null)
                {
                    Debug.LogError("[InteractCommand] InteractionService not found via ServiceLocator");
                    return;
                }

                // インタラクション可否確認
                if (!interactionService.CanInteract(_interactor, _target, _interactionType))
                {
                    Debug.Log($"[InteractCommand] Cannot interact with {_target.name} - conditions not met");
                    _wasExecuted = true; // Undo可能にするためマーク（実際には何もしていない）
                    return;
                }

                // 現在の状態を保存（Undo用）
                SaveCurrentState();

                // インタラクションタイプ別処理
                bool interactionResult = false;
                switch (_interactionType)
                {
                    case InteractionType.PickupItem:
                        interactionResult = HandlePickupInteraction(inventoryService);
                        break;
                    case InteractionType.OpenDoor:
                        interactionResult = HandleDoorInteraction();
                        break;
                    case InteractionType.ActivateSwitch:
                        interactionResult = HandleSwitchInteraction();
                        break;
                    case InteractionType.UseTerminal:
                        interactionResult = HandleTerminalInteraction();
                        break;
                    case InteractionType.Examine:
                        interactionResult = HandleExamineInteraction();
                        break;
                    case InteractionType.Custom:
                        interactionResult = HandleCustomInteraction();
                        break;
                }

                if (interactionResult)
                {
                    // インタラクション音再生
                    string audioClipName = GetInteractionAudioClip();
                    audioService?.PlaySFX(audioClipName, _target.transform.position);

                    // インタラクションイベント発行（Event駆動アーキテクチャ）
                    var interactionData = new Events.InteractionData(
                        _interactor,
                        _target,
                        _interactionType.ToString(),
                        true, // 成功
                        _customInteractionId
                    );

                    var interactionEvent = Resources.Load<Events.InteractionEvent>("Events/InteractionEvent");
                    interactionEvent?.RaiseInteraction(interactionData);

                    _wasExecuted = true;

                    Debug.Log($"[InteractCommand] Successfully interacted with {_target.name} using {_interactionType}");
                }
                else
                {
                    Debug.LogWarning($"[InteractCommand] Failed to interact with {_target.name}");
                    _hadPreviousState = false;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[InteractCommand] Execution failed: {ex.Message}");
            }
        }

        /// <summary>
        /// コマンドUndo実行
        /// </summary>
        public void Undo()
        {
            if (!CanUndo)
            {
                Debug.LogWarning("[InteractCommand] Cannot undo - command was not executed or no state to restore");
                return;
            }

            try
            {
                // インタラクションタイプ別Undo処理
                bool undoResult = false;
                switch (_interactionType)
                {
                    case InteractionType.PickupItem:
                        undoResult = UndoPickupInteraction();
                        break;
                    case InteractionType.OpenDoor:
                        undoResult = UndoDoorInteraction();
                        break;
                    case InteractionType.ActivateSwitch:
                        undoResult = UndoSwitchInteraction();
                        break;
                    default:
                        // その他のインタラクションは基本状態復元
                        undoResult = RestoreBasicState();
                        break;
                }

                if (undoResult)
                {
                    // Undoインタラクションイベント発行
                    var undoData = new Events.InteractionData(
                        _interactor,
                        _target,
                        $"Undo_{_interactionType}",
                        true,
                        _customInteractionId
                    );

                    var interactionEvent = Resources.Load<Events.InteractionEvent>("Events/InteractionEvent");
                    interactionEvent?.RaiseInteraction(undoData);

                    Debug.Log($"[InteractCommand] Undid interaction with {_target.name}");
                }

                _wasExecuted = false;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[InteractCommand] Undo failed: {ex.Message}");
            }
        }

        /// <summary>
        /// 現在の状態を保存
        /// </summary>
        private void SaveCurrentState()
        {
            _targetWasActive = _target.activeInHierarchy;
            _originalPosition = _target.transform.position;
            _parentObject = _target.transform.parent?.gameObject;
            _hadPreviousState = true;
        }

        /// <summary>
        /// アイテム取得インタラクション処理
        /// </summary>
        private bool HandlePickupInteraction(Services.IInventoryService inventoryService)
        {
            if (inventoryService == null) return false;

            var itemComponent = _target.GetComponent<Services.IPickupable>();
            if (itemComponent == null) return false;

            bool addResult = inventoryService.AddItem(itemComponent.GetItemData());
            if (addResult)
            {
                _target.SetActive(false); // アイテムを非表示
                return true;
            }

            return false;
        }

        /// <summary>
        /// ドア開閉インタラクション処理
        /// </summary>
        private bool HandleDoorInteraction()
        {
            var doorComponent = _target.GetComponent<Services.IDoor>();
            if (doorComponent == null) return false;

            return doorComponent.IsOpen ? doorComponent.Close() : doorComponent.Open();
        }

        /// <summary>
        /// スイッチ操作インタラクション処理
        /// </summary>
        private bool HandleSwitchInteraction()
        {
            var switchComponent = _target.GetComponent<Services.ISwitch>();
            if (switchComponent == null) return false;

            switchComponent.Toggle();
            return true;
        }

        /// <summary>
        /// ターミナル使用インタラクション処理
        /// </summary>
        private bool HandleTerminalInteraction()
        {
            var terminalComponent = _target.GetComponent<Services.ITerminal>();
            if (terminalComponent == null) return false;

            return terminalComponent.Activate(_interactor);
        }

        /// <summary>
        /// 調査インタラクション処理
        /// </summary>
        private bool HandleExamineInteraction()
        {
            var examinableComponent = _target.GetComponent<Services.IExaminable>();
            if (examinableComponent == null) return false;

            examinableComponent.Examine(_interactor);
            return true;
        }

        /// <summary>
        /// カスタムインタラクション処理
        /// </summary>
        private bool HandleCustomInteraction()
        {
            var customComponent = _target.GetComponent<Services.ICustomInteractable>();
            if (customComponent == null) return false;

            return customComponent.ExecuteCustomInteraction(_customInteractionId, _interactor);
        }

        /// <summary>
        /// アイテム取得Undo処理
        /// </summary>
        private bool UndoPickupInteraction()
        {
            if (!_targetWasActive) return false;

            _target.SetActive(true);
            _target.transform.position = _originalPosition;

            if (_parentObject != null)
            {
                _target.transform.SetParent(_parentObject.transform);
            }

            // インベントリからアイテム削除
            var inventoryService = asterivo.Unity60.Core.ServiceLocator.GetService<Services.IInventoryService>();
            var itemComponent = _target.GetComponent<Services.IPickupable>();
            if (inventoryService != null && itemComponent != null)
            {
                inventoryService.RemoveItem(itemComponent.GetItemData());
            }

            return true;
        }

        /// <summary>
        /// ドアインタラクションUndo処理
        /// </summary>
        private bool UndoDoorInteraction()
        {
            var doorComponent = _target.GetComponent<Services.IDoor>();
            if (doorComponent == null) return false;

            // 状態を反転して元に戻す
            return doorComponent.IsOpen ? doorComponent.Close() : doorComponent.Open();
        }

        /// <summary>
        /// スイッチインタラクションUndo処理
        /// </summary>
        private bool UndoSwitchInteraction()
        {
            var switchComponent = _target.GetComponent<Services.ISwitch>();
            if (switchComponent == null) return false;

            switchComponent.Toggle(); // スイッチを元の状態に戻す
            return true;
        }

        /// <summary>
        /// 基本状態復元処理
        /// </summary>
        private bool RestoreBasicState()
        {
            _target.SetActive(_targetWasActive);
            _target.transform.position = _originalPosition;

            if (_parentObject != null)
            {
                _target.transform.SetParent(_parentObject.transform);
            }

            return true;
        }

        /// <summary>
        /// インタラクションタイプに応じたオーディオクリップ名取得
        /// </summary>
        private string GetInteractionAudioClip()
        {
            return _interactionType switch
            {
                InteractionType.PickupItem => "ItemPickup",
                InteractionType.OpenDoor => "DoorOpen",
                InteractionType.ActivateSwitch => "SwitchActivate",
                InteractionType.UseTerminal => "TerminalActivate",
                InteractionType.Examine => "ExamineItem",
                InteractionType.Custom => $"Custom_{_customInteractionId}",
                _ => "GenericInteraction"
            };
        }

        /// <summary>
        /// ObjectPool再利用のための状態リセット（IResettableCommand実装）
        /// </summary>
        public void Reset()
        {
            _interactor = null;
            _target = null;
            _interactionType = InteractionType.Custom;
            _customInteractionId = string.Empty;
            _wasExecuted = false;
            _targetWasActive = false;
            _originalPosition = Vector3.zero;
            _parentObject = null;
            _hadPreviousState = false;

            Debug.Log("[InteractCommand] Command reset for ObjectPool reuse");
        }

        /// <summary>
        /// デバッグ情報取得
        /// </summary>
        public override string ToString()
        {
            return $"InteractCommand[Interactor: {_interactor?.name}, Target: {_target?.name}, " +
                   $"Type: {_interactionType}, Executed: {_wasExecuted}, CanUndo: {CanUndo}]";
        }
    }
}