using UnityEngine;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Features.Templates.Adventure.Dialogue;
using asterivo.Unity60.Features.Templates.Adventure.Inventory;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Features.Templates.Adventure.Interaction
{
    /// <summary>
    /// 汎用的なインタラクション可能オブジェクト
    /// 様々なタイプのインタラクションを設定により実現
    /// </summary>
    public class InteractableObject : BaseInteractable
    {
        [TabGroup("Type", "Interaction Type")]
        [Header("Interaction Type")]
        [SerializeField]
        [Tooltip("インタラクションのタイプ")]
        private InteractionType interactionType = InteractionType.Examine;

        [TabGroup("Type", "Interaction Type")]
        [SerializeField]
        [Tooltip("繰り返し使用可能かどうか")]
        private bool isReusable = true;

        [TabGroup("Type", "Interaction Type")]
        [SerializeField]
        [Tooltip("使用回数の制限（-1で無制限）")]
        private int maxUseCount = -1;

        [TabGroup("Message", "Message Settings")]
        [Header("Message Settings")]
        [SerializeField]
        [TextArea(3, 5)]
        [Tooltip("インタラクション時に表示するメッセージ")]
        private string interactionMessage = "";

        [TabGroup("Message", "Message Settings")]
        [SerializeField]
        [Tooltip("メッセージ表示時間")]
        private float messageDuration = 3.0f;

        [TabGroup("Dialogue", "Dialogue Settings")]
        [Header("Dialogue Settings")]
        [SerializeField]
        [ShowIf("@interactionType == InteractionType.Dialogue")]
        [Tooltip("ダイアログデータ")]
        private DialogueData dialogueData;

        [TabGroup("Items", "Item Settings")]
        [Header("Item Settings")]
        [SerializeField]
        [ShowIf("@interactionType == InteractionType.GiveItem")]
        [Tooltip("与えるアイテムのデータ")]
        private AdventureItemData[] itemsToGive = new AdventureItemData[0];

        [TabGroup("Items", "Item Settings")]
        [SerializeField]
        [ShowIf("@interactionType == InteractionType.GiveItem")]
        [Tooltip("アイテムの数量")]
        private int[] itemQuantities = new int[0];

        [TabGroup("Animation", "Animation Settings")]
        [Header("Animation Settings")]
        [SerializeField]
        [Tooltip("インタラクション時のアニメーター")]
        private Animator interactionAnimator;

        [TabGroup("Animation", "Animation Settings")]
        [SerializeField]
        [Tooltip("再生するアニメーション名")]
        private string animationTriggerName = "Interact";

        [TabGroup("Audio", "Audio Settings")]
        [Header("Audio Settings")]
        [SerializeField]
        [Tooltip("インタラクション時の効果音")]
        private AudioClip interactionSound;

        [TabGroup("Audio", "Audio Settings")]
        [SerializeField]
        [Tooltip("音声ソース")]
        private AudioSource audioSource;

        [TabGroup("Status", "Runtime Status")]
        [ShowInInspector, ReadOnly]
        [Tooltip("使用回数")]
        private int currentUseCount = 0;

        [TabGroup("Status", "Runtime Status")]
        [ShowInInspector, ReadOnly]
        [Tooltip("すでに使用済みか")]
        private bool isUsed = false;

        // 依存システム参照
        private DialogueManager dialogueManager;

        protected override void Start()
        {
            base.Start();
            InitializeInteractableObject();
        }

        /// <summary>
        /// インタラクション可能オブジェクト固有の初期化
        /// </summary>
        private void InitializeInteractableObject()
        {
            dialogueManager = ServiceLocator.Instance?.GetService<DialogueManager>();

            // オーディオソースの自動取得
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null && interactionSound != null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                }
            }

            // アニメーターの自動取得
            if (interactionAnimator == null)
            {
                interactionAnimator = GetComponent<Animator>();
            }

            ValidateSettings();
        }

        /// <summary>
        /// 設定の検証
        /// </summary>
        private void ValidateSettings()
        {
            // アイテム数量配列のサイズ調整
            if (itemQuantities.Length != itemsToGive.Length)
            {
                System.Array.Resize(ref itemQuantities, itemsToGive.Length);
                for (int i = 0; i < itemQuantities.Length; i++)
                {
                    if (itemQuantities[i] <= 0)
                    {
                        itemQuantities[i] = 1;
                    }
                }
            }

            // ダイアログタイプでダイアログデータが未設定の警告
            if (interactionType == InteractionType.Dialogue && dialogueData == null)
            {
                LogWarning($"[InteractableObject] Dialogue type selected but no dialogue data assigned on {gameObject.name}");
            }

            // アイテム付与タイプでアイテムが未設定の警告
            if (interactionType == InteractionType.GiveItem && itemsToGive.Length == 0)
            {
                LogWarning($"[InteractableObject] GiveItem type selected but no items assigned on {gameObject.name}");
            }
        }

        /// <summary>
        /// カスタムインタラクション可能条件
        /// </summary>
        protected override bool CanInteractCustom()
        {
            // 使用済みで再利用不可の場合
            if (isUsed && !isReusable)
            {
                return false;
            }

            // 最大使用回数に達している場合
            if (maxUseCount > 0 && currentUseCount >= maxUseCount)
            {
                return false;
            }

            // タイプ別の条件チェック
            switch (interactionType)
            {
                case InteractionType.Dialogue:
                    return dialogueData != null && dialogueManager != null;

                case InteractionType.GiveItem:
                    return itemsToGive.Length > 0 && inventoryManager != null;

                default:
                    return true;
            }
        }

        /// <summary>
        /// 具体的なインタラクション処理
        /// </summary>
        protected override bool PerformInteraction()
        {
            bool success = false;

            switch (interactionType)
            {
                case InteractionType.Examine:
                    success = PerformExamineInteraction();
                    break;

                case InteractionType.Dialogue:
                    success = PerformDialogueInteraction();
                    break;

                case InteractionType.GiveItem:
                    success = PerformGiveItemInteraction();
                    break;

                case InteractionType.Custom:
                    success = PerformCustomInteraction();
                    break;

                default:
                    LogWarning($"[InteractableObject] Unknown interaction type: {interactionType}");
                    break;
            }

            if (success)
            {
                currentUseCount++;
                isUsed = true;

                // 効果音再生
                PlayInteractionSound();

                // アニメーション再生
                PlayInteractionAnimation();
            }

            return success;
        }

        /// <summary>
        /// 調べる系のインタラクション
        /// </summary>
        private bool PerformExamineInteraction()
        {
            if (!string.IsNullOrEmpty(interactionMessage))
            {
                ShowMessage(interactionMessage, messageDuration);
                return true;
            }

            ShowMessage($"You examine the {gameObject.name}.", messageDuration);
            return true;
        }

        /// <summary>
        /// ダイアログ系のインタラクション
        /// </summary>
        private bool PerformDialogueInteraction()
        {
            if (dialogueManager == null || dialogueData == null)
            {
                LogError("[InteractableObject] DialogueManager or DialogueData is null");
                return false;
            }

            dialogueManager.StartDialogue(dialogueData);
            return true;
        }

        /// <summary>
        /// アイテム付与系のインタラクション
        /// </summary>
        private bool PerformGiveItemInteraction()
        {
            if (inventoryManager == null || itemsToGive.Length == 0)
            {
                LogError("[InteractableObject] InventoryManager is null or no items to give");
                return false;
            }

            bool allItemsGiven = true;
            string itemNames = "";

            for (int i = 0; i < itemsToGive.Length; i++)
            {
                var item = itemsToGive[i];
                var quantity = i < itemQuantities.Length ? itemQuantities[i] : 1;

                if (item != null)
                {
                    int actuallyAdded = inventoryManager.AddItem(item, quantity);
                    if (actuallyAdded > 0)
                    {
                        if (!string.IsNullOrEmpty(itemNames))
                        {
                            itemNames += ", ";
                        }
                        itemNames += $"{item.itemName} x{actuallyAdded}";
                    }
                    else
                    {
                        allItemsGiven = false;
                    }
                }
            }

            if (!string.IsNullOrEmpty(itemNames))
            {
                ShowMessage($"Received: {itemNames}", messageDuration);
            }

            if (!allItemsGiven)
            {
                ShowMessage("Your inventory is full!", messageDuration);
            }

            return allItemsGiven;
        }

        /// <summary>
        /// カスタムインタラクション（継承先でオーバーライド可能）
        /// </summary>
        protected virtual bool PerformCustomInteraction()
        {
            LogDebug($"[InteractableObject] Performing custom interaction on {gameObject.name}");

            if (!string.IsNullOrEmpty(interactionMessage))
            {
                ShowMessage(interactionMessage, messageDuration);
            }

            return true;
        }

        /// <summary>
        /// メッセージ表示
        /// </summary>
        private void ShowMessage(string message, float duration)
        {
            LogDebug($"[InteractableObject] Message: {message}");
            // UIManager経由でメッセージ表示
            // 現在は仮実装としてログ出力
        }

        /// <summary>
        /// インタラクション効果音の再生
        /// </summary>
        private void PlayInteractionSound()
        {
            if (audioSource != null && interactionSound != null)
            {
                audioSource.PlayOneShot(interactionSound);
            }
        }

        /// <summary>
        /// インタラクションアニメーションの再生
        /// </summary>
        private void PlayInteractionAnimation()
        {
            if (interactionAnimator != null && !string.IsNullOrEmpty(animationTriggerName))
            {
                interactionAnimator.SetTrigger(animationTriggerName);
            }
        }

        /// <summary>
        /// インタラクション不可時のテキスト
        /// </summary>
        protected override string GetUnavailableInteractionText()
        {
            if (isUsed && !isReusable)
            {
                return "Already used";
            }

            if (maxUseCount > 0 && currentUseCount >= maxUseCount)
            {
                return "No more uses";
            }

            return base.GetUnavailableInteractionText();
        }

        /// <summary>
        /// オブジェクトのリセット（再利用用）
        /// </summary>
        public void ResetInteractable()
        {
            currentUseCount = 0;
            isUsed = false;
            isInteractable = true;
            gameObject.SetActive(true);
        }

        #region Debug Support

        [TabGroup("Debug", "Debug Tools")]
        [Button("Reset Object")]
        [ShowIf("debugMode")]
        private void DebugResetObject()
        {
            ResetInteractable();
            LogDebug($"[InteractableObject] Object reset: {gameObject.name}");
        }

        [TabGroup("Debug", "Debug Tools")]
        [Button("Test Give Items")]
        [ShowIf("@debugMode && interactionType == InteractionType.GiveItem")]
        private void DebugTestGiveItems()
        {
            bool success = PerformGiveItemInteraction();
            LogDebug($"[InteractableObject] Give items test result: {success}");
        }

        [TabGroup("Debug", "Debug Tools")]
        [Button("Test Dialogue")]
        [ShowIf("@debugMode && interactionType == InteractionType.Dialogue")]
        private void DebugTestDialogue()
        {
            bool success = PerformDialogueInteraction();
            LogDebug($"[InteractableObject] Dialogue test result: {success}");
        }

        #endregion
    }

    /// <summary>
    /// インタラクションタイプの定義
    /// </summary>
    public enum InteractionType
    {
        [Tooltip("オブジェクトを調べる")]
        Examine,

        [Tooltip("ダイアログを開始する")]
        Dialogue,

        [Tooltip("アイテムを与える")]
        GiveItem,

        [Tooltip("カスタムアクション")]
        Custom
    }
}