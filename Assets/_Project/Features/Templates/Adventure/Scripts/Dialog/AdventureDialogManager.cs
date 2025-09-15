using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Features.Templates.Adventure.Data;
using asterivo.Unity60.Features.Templates.Adventure.Player;

namespace asterivo.Unity60.Features.Templates.Adventure.Dialog
{
    /// <summary>
    /// Adventure template dialog manager
    /// Handles dialogue interactions, character conversations, and story progression
    /// </summary>
    public class AdventureDialogManager : MonoBehaviour
    {
        [Header("Dialog UI References")]
        [SerializeField] private GameObject dialogPanel;
        [SerializeField] private Text characterNameText;
        [SerializeField] private Text dialogText;
        [SerializeField] private Image characterPortrait;
        [SerializeField] private Transform choiceButtonContainer;
        [SerializeField] private Button continueButton;
        [SerializeField] private GameObject choiceButtonPrefab;

        [Header("Dialog Settings")]
        [SerializeField] private float textTypewriterSpeed = 0.03f;
        [SerializeField] private bool allowSkipTyping = true;
        [SerializeField] private KeyCode skipKey = KeyCode.Space;
        [SerializeField] private KeyCode nextKey = KeyCode.Return;

        [Header("Dialog Audio")]
        [SerializeField] private AudioSource dialogAudioSource;
        [SerializeField] private AudioClip dialogStartSound;
        [SerializeField] private AudioClip dialogEndSound;
        [SerializeField] private AudioClip choiceSelectSound;
        [SerializeField] private AudioClip typewriterSound;

        // Dialog state
        private DialogData currentDialog;
        private DialogNode currentNode;
        private bool isDialogActive = false;
        private bool isTypingText = false;
        private Coroutine typingCoroutine;
        private List<Button> choiceButtons = new List<Button>();

        // Components and references
        // private AdventureTemplateConfiguration templateConfig; // TODO: Create this class
        private AdventurePlayerController playerController;
        private Canvas dialogCanvas;

        // Events
        [Header("Events")]
        [SerializeField] private DialogStartedGameEvent onDialogStarted;
        [SerializeField] private DialogEndedGameEvent onDialogEnded;
        [SerializeField] private DialogChoiceMadeGameEvent onDialogChoiceMade;
        [SerializeField] private DialogNodeChangedGameEvent onDialogNodeChanged;

        public bool IsDialogActive => isDialogActive;
        public DialogData CurrentDialog => currentDialog;
        public DialogNode CurrentNode => currentNode;

        private void Awake()
        {
            playerController = GetComponent<AdventurePlayerController>();
            
            if (dialogAudioSource == null)
                dialogAudioSource = gameObject.AddComponent<AudioSource>();
        }

        private void Start()
        {
            InitializeUI();
            SetupEventListeners();
        }

        private void Update()
        {
            if (isDialogActive)
            {
                HandleDialogInput();
            }
        }

        public void Initialize()
        {
            // templateConfig = config; // TODO: Implement when AdventureTemplateConfiguration is created
            Debug.Log("[AdventureDialogManager] Adventure dialog manager initialized");
        }

        #region Dialog Management

        public void StartDialog(DialogData dialog, string startNodeId = null)
        {
            if (dialog == null)
            {
                Debug.LogWarning("[AdventureDialogManager] Cannot start dialog - dialog data is null");
                return;
            }

            currentDialog = dialog;
            isDialogActive = true;

            // Find starting node
            string nodeId = string.IsNullOrEmpty(startNodeId) ? dialog.StartNodeId : startNodeId;
            currentNode = dialog.GetNode(nodeId);

            if (currentNode == null)
            {
                Debug.LogError($"[AdventureDialogManager] Starting dialog node '{nodeId}' not found");
                EndDialog();
                return;
            }

            // Show dialog UI
            ShowDialogUI(true);
            
            // Display first node
            DisplayCurrentNode();

            // Play start sound
            PlaySound(dialogStartSound);

            // Disable player movement if needed
            if (playerController != null)
            {
                // playerController.SetMovementEnabled(false);
            }

            // Raise event
            var startCharacter = currentDialog.GetCharacter(currentNode.characterId);
            onDialogStarted?.Raise(new DialogStartedEventData(
                currentDialog,
                currentNode,
                this.gameObject
            ));

            Debug.Log($"[AdventureDialogManager] Started dialog: {dialog.DialogTitle}");
        }

        public void EndDialog()
        {
            if (!isDialogActive) return;

            var previousDialog = currentDialog;
            var previousNode = currentNode;

            // Stop typing if in progress
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
                typingCoroutine = null;
            }

            // Hide dialog UI
            ShowDialogUI(false);

            // Re-enable player movement
            if (playerController != null)
            {
                // playerController.SetMovementEnabled(true);
            }

            // Play end sound
            PlaySound(dialogEndSound);

            // Clear state
            isDialogActive = false;
            isTypingText = false;
            currentDialog = null;
            currentNode = null;

            // Raise event
            onDialogEnded?.Raise(new DialogEndedEventData(
                previousDialog,
                previousNode,
                true
            ));

            Debug.Log("[AdventureDialogManager] Dialog ended");
        }

        private void DisplayCurrentNode()
        {
            if (currentNode == null) return;

            // Update character info
            var character = currentDialog.GetCharacter(currentNode.characterId);
            if (characterNameText != null && character != null)
            {
                characterNameText.text = character.characterName;
            }

            if (characterPortrait != null && character != null && character.characterPortrait != null)
            {
                characterPortrait.sprite = character.characterPortrait;
                characterPortrait.gameObject.SetActive(true);
            }
            else if (characterPortrait != null)
            {
                characterPortrait.gameObject.SetActive(false);
            }

            // Display text with typewriter effect
            if (dialogText != null)
            {
                if (typingCoroutine != null)
                {
                    StopCoroutine(typingCoroutine);
                }

                typingCoroutine = StartCoroutine(TypewriterEffect(currentNode.dialogText));
            }

            // Setup choices or continue button
            SetupNodeChoices();

            // Raise node changed event
            onDialogNodeChanged?.Raise(new DialogNodeChangedEventData(
                currentDialog,
                null, // Could track this if needed
                currentNode
            ));
        }

        #endregion

        #region Node Navigation

        private void SetupNodeChoices()
        {
            // Clear existing choice buttons
            ClearChoiceButtons();

            if (currentNode.choices != null && currentNode.choices.Count > 0)
            {
                // Show choices
                foreach (var choice in currentNode.choices)
                {
                    CreateChoiceButton(choice);
                }

                // Hide continue button
                if (continueButton != null)
                {
                    continueButton.gameObject.SetActive(false);
                }
            }
            else
            {
                // No choices - show continue button or auto-advance
                if (continueButton != null)
                {
                    continueButton.gameObject.SetActive(true);
                    continueButton.onClick.RemoveAllListeners();
                    
                    if (string.IsNullOrEmpty(currentNode.nextNodeId))
                    {
                        // End of dialog
                        continueButton.onClick.AddListener(() => EndDialog());
                        continueButton.GetComponentInChildren<Text>().text = "End";
                    }
                    else
                    {
                        // Continue to next node
                        continueButton.onClick.AddListener(() => GoToNode(currentNode.nextNodeId));
                        continueButton.GetComponentInChildren<Text>().text = "Continue";
                    }
                }
            }
        }

        private void CreateChoiceButton(DialogChoice choice)
        {
            if (choiceButtonPrefab == null || choiceButtonContainer == null) return;

            GameObject buttonObj = Instantiate(choiceButtonPrefab, choiceButtonContainer);
            Button button = buttonObj.GetComponent<Button>();
            Text buttonText = buttonObj.GetComponentInChildren<Text>();

            if (button != null && buttonText != null)
            {
                buttonText.text = choice.choiceText;
                button.onClick.AddListener(() => MakeChoice(choice));
                
                // Check if choice is available
                button.interactable = IsChoiceAvailable(choice);
                
                choiceButtons.Add(button);
            }
        }

        private void ClearChoiceButtons()
        {
            foreach (var button in choiceButtons)
            {
                if (button != null)
                {
                    Destroy(button.gameObject);
                }
            }
            choiceButtons.Clear();
        }

        private void MakeChoice(DialogChoice choice)
        {
            PlaySound(choiceSelectSound);

            // Raise choice made event
            onDialogChoiceMade?.Raise(new DialogChoiceMadeEventData(
                choice,
                currentNode,
                choice.nextNodeId
            ));

            // Apply choice consequences if any
            ApplyChoiceConsequences(choice);

            // Navigate to next node
            if (!string.IsNullOrEmpty(choice.nextNodeId))
            {
                GoToNode(choice.nextNodeId);
            }
            else
            {
                EndDialog();
            }

            Debug.Log($"[AdventureDialogManager] Choice made: {choice.choiceText}");
        }

        private void GoToNode(string nodeId)
        {
            if (currentDialog == null) return;

            DialogNode nextNode = currentDialog.GetNode(nodeId);
            if (nextNode == null)
            {
                Debug.LogError($"[AdventureDialogManager] Dialog node '{nodeId}' not found");
                EndDialog();
                return;
            }

            currentNode = nextNode;
            DisplayCurrentNode();
        }

        private bool IsChoiceAvailable(DialogChoice choice)
        {
            // Use the choice's CanSelect method which checks conditions
            return choice.CanSelect();
        }

        private void ApplyChoiceConsequences(DialogChoice choice)
        {
            // Execute choice actions
            choice.ExecuteChoice();

            // Apply additional consequences if needed
            // Implementation would depend on game-specific systems
        }

        #endregion

        #region Text Effects

        private System.Collections.IEnumerator TypewriterEffect(string text)
        {
            if (dialogText == null) yield break;

            isTypingText = true;
            dialogText.text = "";

            for (int i = 0; i < text.Length; i++)
            {
                dialogText.text += text[i];
                
                // Play typing sound occasionally
                if (typewriterSound != null && i % 3 == 0)
                {
                    PlaySound(typewriterSound, 0.3f);
                }

                yield return new WaitForSeconds(textTypewriterSpeed);
            }

            isTypingText = false;
            typingCoroutine = null;
        }

        private void SkipTyping()
        {
            if (!isTypingText || currentNode == null) return;

            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
                typingCoroutine = null;
            }

            if (dialogText != null)
            {
                dialogText.text = currentNode.dialogText;
            }

            isTypingText = false;
        }

        #endregion

        #region Input Handling

        private void HandleDialogInput()
        {
            if (Input.GetKeyDown(skipKey) && allowSkipTyping)
            {
                if (isTypingText)
                {
                    SkipTyping();
                }
            }

            if (Input.GetKeyDown(nextKey) && !isTypingText)
            {
                // Advance dialog if no choices
                if (currentNode.choices == null || currentNode.choices.Count == 0)
                {
                    if (!string.IsNullOrEmpty(currentNode.nextNodeId))
                    {
                        GoToNode(currentNode.nextNodeId);
                    }
                    else
                    {
                        EndDialog();
                    }
                }
            }
        }

        #endregion

        #region UI Management

        private void InitializeUI()
        {
            // Find or create dialog canvas
            dialogCanvas = FindObjectOfType<Canvas>();
            
            if (dialogPanel == null)
            {
                CreateDialogUI();
            }

            if (dialogPanel != null)
            {
                dialogPanel.SetActive(false);
            }
        }

        private void ShowDialogUI(bool show)
        {
            if (dialogPanel != null)
            {
                dialogPanel.SetActive(show);
            }
        }

        private void CreateDialogUI()
        {
            if (dialogCanvas == null) return;

            // Create basic dialog UI
            GameObject panel = new GameObject("DialogPanel");
            panel.transform.SetParent(dialogCanvas.transform, false);

            RectTransform panelRect = panel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.1f, 0.1f);
            panelRect.anchorMax = new Vector2(0.9f, 0.4f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            Image panelImage = panel.AddComponent<Image>();
            panelImage.color = new Color(0, 0, 0, 0.8f);

            // Create dialog text
            GameObject textObj = new GameObject("DialogText");
            textObj.transform.SetParent(panel.transform, false);

            Text text = textObj.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 16;
            text.color = Color.white;

            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(20, 20);
            textRect.offsetMax = new Vector2(-20, -20);

            dialogPanel = panel;
            dialogText = text;

            Debug.Log("[AdventureDialogManager] Created basic dialog UI");
        }

        #endregion

        #region Audio

        private void PlaySound(AudioClip clip, float volume = 1f)
        {
            if (dialogAudioSource != null && clip != null)
            {
                dialogAudioSource.PlayOneShot(clip, volume);
            }
        }

        #endregion

        #region Event Listeners

        private void SetupEventListeners()
        {
            // Could listen to game events that trigger dialogs
        }

        #endregion

        #region Public Interface

        public void TriggerDialog(string dialogId)
        {
            // Load dialog data by ID and start it
            // This would typically load from a dialog database
            Debug.Log($"[AdventureDialogManager] Triggered dialog: {dialogId}");
        }

        public void SetTypewriterSpeed(float speed)
        {
            textTypewriterSpeed = speed;
        }

        public void SetAllowSkipping(bool allow)
        {
            allowSkipTyping = allow;
        }

        #endregion

        private void OnDisable()
        {
            if (isDialogActive)
            {
                EndDialog();
            }
        }
    }

    // Event data structures are defined in DialogChoice.cs
}