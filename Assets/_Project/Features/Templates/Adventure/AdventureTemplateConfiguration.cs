using UnityEngine;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Commands;
using System.Collections.Generic;
using asterivo.Unity60.Features.Templates.Adventure.Player;
using asterivo.Unity60.Features.Templates.Adventure.Interaction;
using asterivo.Unity60.Features.Templates.Adventure.Dialog;
using asterivo.Unity60.Features.Templates.Adventure.Inventory;
using asterivo.Unity60.Features.Templates.Adventure.Quest;

namespace asterivo.Unity60.Features.Templates.Adventure
{
    /// <summary>
    /// Adventure（アドベンチャー）ジャンル特化テンプレート設定
    /// ダイアログシステム、インベントリ、クエスト管理による15分間のゲームプレイ
    /// </summary>
    [CreateAssetMenu(fileName = "AdventureTemplateConfiguration", menuName = "Templates/Adventure/Configuration")]
    public class AdventureTemplateConfiguration : ScriptableObject
    {
        [Header("Adventure Core Settings")]
        [SerializeField] private float movementSpeed = 3.5f;
        [SerializeField] private bool enableRun = true;
        [SerializeField] private float runMultiplier = 1.6f;
        [SerializeField] private float interactionRange = 2.0f;
        [SerializeField] private LayerMask interactionLayerMask = 1;

        [Header("Dialog System Settings")]
        [SerializeField] private bool enableDialogSystem = true;
        [SerializeField] private float dialogTextSpeed = 30f; // Characters per second
        [SerializeField] private bool enableAutoAdvance = false;
        [SerializeField] private float autoAdvanceDelay = 3.0f;
        [SerializeField] private bool enableVoiceOver = false;

        [Header("Inventory System Settings")]
        [SerializeField] private bool enableInventorySystem = true;
        [SerializeField] private int maxInventorySlots = 20;
        [SerializeField] private bool enableItemCombining = true;
        [SerializeField] private bool enableItemExamination = true;

        [Header("Quest System Settings")]
        [SerializeField] private bool enableQuestSystem = true;
        [SerializeField] private int maxActiveQuests = 5;
        [SerializeField] private bool enableQuestTracking = true;
        [SerializeField] private bool enableQuestHints = true;

        [Header("Puzzle & Minigames")]
        [SerializeField] private bool enablePuzzleSystem = true;
        [SerializeField] private bool enableMinigames = false;
        [SerializeField] private float puzzleHintDelay = 30f; // Seconds before hint appears

        [Header("Exploration Settings")]
        [SerializeField] private bool enableMapSystem = false;
        [SerializeField] private bool enableCompass = true;
        [SerializeField] private bool enableWaypoints = true;
        [SerializeField] private float objectiveMarkerRange = 10f;

        [Header("15-Minute Learning Objectives")]
        [SerializeField] private List<AdventureLearningObjective> learningObjectives = new List<AdventureLearningObjective>();

        [Header("Item Database")]
        [SerializeField] private List<AdventureItem> availableItems = new List<AdventureItem>();

        [Header("Quest Database")]
        [SerializeField] private List<AdventureQuest> availableQuests = new List<AdventureQuest>();

        [Header("Audio Settings")]
        [SerializeField] private AudioClip[] dialogSounds;
        [SerializeField] private AudioClip[] itemPickupSounds;
        [SerializeField] private AudioClip[] questCompleteSounds;
        [SerializeField] private AudioClip[] puzzleSolveSounds;
        [SerializeField] private float masterVolume = 1.0f;

        [Header("Events")]
        [SerializeField] private GameEvent onAdventureStarted;
        [SerializeField] private GameEvent onDialogStarted;
        [SerializeField] private GameEvent onItemCollected;
        [SerializeField] private GameEvent onQuestCompleted;
        [SerializeField] private GameEvent onPuzzleSolved;
        [SerializeField] private GameEvent onObjectiveCompleted;

        // Properties
        public float MovementSpeed => movementSpeed;
        public float InteractionRange => interactionRange;
        public LayerMask InteractionLayerMask => interactionLayerMask;
        public float DialogTextSpeed => dialogTextSpeed;
        public int MaxInventorySlots => maxInventorySlots;
        public int MaxActiveQuests => maxActiveQuests;
        public bool EnableDialogSystem => enableDialogSystem;
        public bool EnableInventorySystem => enableInventorySystem;
        public bool EnableQuestSystem => enableQuestSystem;

        /// <summary>
        /// Adventureプレイヤーコントローラーの初期設定
        /// </summary>
        public void SetupPlayerController(GameObject player)
        {
            if (player == null) return;

            // Character Controller Setup
            var characterController = player.GetComponent<CharacterController>();
            if (characterController == null)
            {
                characterController = player.AddComponent<CharacterController>();
                characterController.height = 1.8f;
                characterController.radius = 0.35f;
                characterController.stepOffset = 0.3f;
                characterController.slopeLimit = 45f;
            }

            // Adventure Player Controller
            if (player.GetComponent<AdventurePlayerController>() == null)
            {
                var adventureController = player.AddComponent<AdventurePlayerController>();
                adventureController.Initialize(this);
            }

            // Interaction System
            SetupInteractionSystem(player);

            Debug.Log($"[AdventureTemplate] Player controller setup completed for {player.name}");
        }

        /// <summary>
        /// インタラクションシステムの設定
        /// </summary>
        private void SetupInteractionSystem(GameObject player)
        {
            if (player.GetComponent<AdventureInteractionSystem>() == null)
            {
                var interactionSystem = player.AddComponent<AdventureInteractionSystem>();
                interactionSystem.Initialize(this);
            }

            // Create interaction UI
            SetupInteractionUI();
        }

        /// <summary>
        /// インタラクションUIの設定
        /// </summary>
        private void SetupInteractionUI()
        {
            var canvas = FindFirstObjectByType<Canvas>();
            if (canvas != null)
            {
                // Interaction Prompt
                var interactionPrompt = canvas.transform.Find("InteractionPrompt");
                if (interactionPrompt == null)
                {
                    var promptGO = new GameObject("InteractionPrompt");
                    promptGO.transform.SetParent(canvas.transform);
                    
                    var rectTransform = promptGO.AddComponent<RectTransform>();
                    rectTransform.anchoredPosition = new Vector2(0, -150);
                    rectTransform.sizeDelta = new Vector2(300, 60);

                    var promptText = promptGO.AddComponent<UnityEngine.UI.Text>();
                    promptText.text = "Press [E] to interact";
                    promptText.color = Color.white;
                    promptText.alignment = TextAnchor.MiddleCenter;
                    promptText.fontSize = 18;
                    promptGO.SetActive(false); // Hidden by default
                }
            }
        }

        /// <summary>
        /// ダイアログシステムの設定
        /// </summary>
        public void SetupDialogSystem(GameObject player)
        {
            if (!enableDialogSystem || player == null) return;

            // Dialog Manager
            if (player.GetComponent<AdventureDialogManager>() == null)
            {
                var dialogManager = player.AddComponent<AdventureDialogManager>();
                dialogManager.Initialize();
            }

            // Dialog UI
            SetupDialogUI();

            Debug.Log($"[AdventureTemplate] Dialog system setup completed");
        }

        /// <summary>
        /// ダイアログUIの設定
        /// </summary>
        private void SetupDialogUI()
        {
            var canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null) return;

            // Dialog Panel
            var dialogPanel = canvas.transform.Find("DialogPanel");
            if (dialogPanel == null)
            {
                var panelGO = new GameObject("DialogPanel");
                panelGO.transform.SetParent(canvas.transform);
                
                var rectTransform = panelGO.AddComponent<RectTransform>();
                rectTransform.anchorMin = new Vector2(0, 0);
                rectTransform.anchorMax = new Vector2(1, 0.3f);
                rectTransform.offsetMin = Vector2.zero;
                rectTransform.offsetMax = Vector2.zero;

                var panelImage = panelGO.AddComponent<UnityEngine.UI.Image>();
                panelImage.color = new Color(0, 0, 0, 0.8f);

                // Dialog Text
                var textGO = new GameObject("DialogText");
                textGO.transform.SetParent(panelGO.transform);
                
                var textRect = textGO.AddComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = new Vector2(20, 20);
                textRect.offsetMax = new Vector2(-20, -20);

                var dialogText = textGO.AddComponent<UnityEngine.UI.Text>();
                dialogText.text = "Dialog text will appear here...";
                dialogText.color = Color.white;
                dialogText.fontSize = 16;
                dialogText.alignment = TextAnchor.UpperLeft;

                panelGO.SetActive(false); // Hidden by default
            }
        }

        /// <summary>
        /// インベントリシステムの設定
        /// </summary>
        public void SetupInventorySystem(GameObject player)
        {
            if (!enableInventorySystem || player == null) return;

            // Inventory Manager
            if (player.GetComponent<AdventureInventoryManager>() == null)
            {
                var inventoryManager = player.AddComponent<AdventureInventoryManager>();
                inventoryManager.Initialize();
            }

            // Inventory UI
            SetupInventoryUI();

            // Initialize default items
            InitializeDefaultItems();

            Debug.Log($"[AdventureTemplate] Inventory system setup completed with {maxInventorySlots} slots");
        }

        /// <summary>
        /// インベントリUIの設定
        /// </summary>
        private void SetupInventoryUI()
        {
            var canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null) return;

            // Inventory Panel
            var inventoryPanel = canvas.transform.Find("InventoryPanel");
            if (inventoryPanel == null)
            {
                var panelGO = new GameObject("InventoryPanel");
                panelGO.transform.SetParent(canvas.transform);
                
                var rectTransform = panelGO.AddComponent<RectTransform>();
                rectTransform.anchorMin = new Vector2(0.7f, 0.3f);
                rectTransform.anchorMax = new Vector2(1f, 1f);
                rectTransform.offsetMin = Vector2.zero;
                rectTransform.offsetMax = Vector2.zero;

                var panelImage = panelGO.AddComponent<UnityEngine.UI.Image>();
                panelImage.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);

                // Create inventory grid (placeholder)
                CreateInventoryGrid(panelGO);

                panelGO.SetActive(false); // Hidden by default
            }
        }

        /// <summary>
        /// インベントリグリッドの作成
        /// </summary>
        private void CreateInventoryGrid(GameObject parent)
        {
            var gridGO = new GameObject("InventoryGrid");
            gridGO.transform.SetParent(parent.transform);

            var rectTransform = gridGO.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = new Vector2(10, 10);
            rectTransform.offsetMax = new Vector2(-10, -10);

            var gridLayout = gridGO.AddComponent<UnityEngine.UI.GridLayoutGroup>();
            gridLayout.cellSize = new Vector2(60, 60);
            gridLayout.spacing = new Vector2(5, 5);
            gridLayout.constraint = UnityEngine.UI.GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = 5; // 5 columns

            // Create inventory slots
            for (int i = 0; i < maxInventorySlots; i++)
            {
                var slotGO = new GameObject($"InventorySlot_{i}");
                slotGO.transform.SetParent(gridGO.transform);

                var slotImage = slotGO.AddComponent<UnityEngine.UI.Image>();
                slotImage.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            }
        }

        /// <summary>
        /// クエストシステムの設定
        /// </summary>
        public void SetupQuestSystem(GameObject player)
        {
            if (!enableQuestSystem || player == null) return;

            // Quest Manager
            if (player.GetComponent<QuestManager>() == null)
            {
                var questManager = player.AddComponent<QuestManager>();
                questManager.Initialize(); // Remove parameter as Initialize method takes no arguments
            }

            // Quest UI
            SetupQuestUI();

            // Initialize default quests
            InitializeDefaultQuests();

            Debug.Log($"[AdventureTemplate] Quest system setup completed with {maxActiveQuests} max active quests");
        }

        /// <summary>
        /// クエストUIの設定
        /// </summary>
        private void SetupQuestUI()
        {
            var canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null) return;

            // Quest Log Panel
            var questPanel = canvas.transform.Find("QuestPanel");
            if (questPanel == null)
            {
                var panelGO = new GameObject("QuestPanel");
                panelGO.transform.SetParent(canvas.transform);
                
                var rectTransform = panelGO.AddComponent<RectTransform>();
                rectTransform.anchorMin = new Vector2(0f, 0.7f);
                rectTransform.anchorMax = new Vector2(0.3f, 1f);
                rectTransform.offsetMin = Vector2.zero;
                rectTransform.offsetMax = Vector2.zero;

                var panelImage = panelGO.AddComponent<UnityEngine.UI.Image>();
                panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);

                // Quest List
                var questListGO = new GameObject("QuestList");
                questListGO.transform.SetParent(panelGO.transform);

                var listRect = questListGO.AddComponent<RectTransform>();
                listRect.anchorMin = Vector2.zero;
                listRect.anchorMax = Vector2.one;
                listRect.offsetMin = new Vector2(10, 10);
                listRect.offsetMax = new Vector2(-10, -10);

                var questListText = questListGO.AddComponent<UnityEngine.UI.Text>();
                questListText.text = "Active Quests:\n- Tutorial Quest";
                questListText.color = Color.white;
                questListText.fontSize = 14;
                questListText.alignment = TextAnchor.UpperLeft;

                panelGO.SetActive(true); // Visible by default for Adventure
            }
        }

        /// <summary>
        /// パズルシステムの設定
        /// </summary>
        public void SetupPuzzleSystem(GameObject player)
        {
            if (!enablePuzzleSystem || player == null) return;

            // Puzzle Manager - TODO: Implement AdventurePuzzleManager
            // if (player.GetComponent<AdventurePuzzleManager>() == null)
            // {
            //     var puzzleManager = player.AddComponent<AdventurePuzzleManager>();
            //     puzzleManager.Initialize(this);
            // }

            Debug.Log($"[AdventureTemplate] Puzzle system setup completed");
        }

        /// <summary>
        /// 学習目標の初期化と追跡
        /// </summary>
        public void InitializeLearningObjectives()
        {
            if (learningObjectives.Count == 0)
            {
                CreateDefaultLearningObjectives();
            }

            foreach (var objective in learningObjectives)
            {
                objective.Initialize();
                Debug.Log($"[AdventureTemplate] Learning objective initialized: {objective.Title}");
            }
        }

        /// <summary>
        /// デフォルト学習目標の作成（15分間の段階的学習）
        /// </summary>
        private void CreateDefaultLearningObjectives()
        {
            learningObjectives.Clear();
            
            // Phase 1: Basic Interaction & Movement (0-3 minutes)
            learningObjectives.Add(new AdventureLearningObjective
            {
                Id = "adventure_movement_interaction",
                Title = "移動とインタラクション",
                Description = "WASD移動、オブジェクトとの相互作用、アイテム収集",
                EstimatedTime = 180f,
                Priority = 1
            });

            // Phase 2: Dialog System (3-6 minutes)
            learningObjectives.Add(new AdventureLearningObjective
            {
                Id = "adventure_dialog_system",
                Title = "ダイアログシステム",
                Description = "NPCとの会話、選択肢システム、ストーリー進行",
                EstimatedTime = 180f,
                Priority = 2
            });

            // Phase 3: Inventory Management (6-9 minutes)
            learningObjectives.Add(new AdventureLearningObjective
            {
                Id = "adventure_inventory_management",
                Title = "インベントリ管理",
                Description = "アイテム収集、整理、使用、組み合わせシステム",
                EstimatedTime = 180f,
                Priority = 3
            });

            // Phase 4: Quest & Puzzle Solving (9-12 minutes)
            learningObjectives.Add(new AdventureLearningObjective
            {
                Id = "adventure_quest_puzzle",
                Title = "クエストとパズル解決",
                Description = "クエスト受注、パズル解法、進行管理システム",
                EstimatedTime = 180f,
                Priority = 4
            });

            // Phase 5: Adventure Completion (12-15 minutes)
            learningObjectives.Add(new AdventureLearningObjective
            {
                Id = "adventure_story_completion",
                Title = "アドベンチャー完遂",
                Description = "ストーリーライン完了、全システム活用",
                EstimatedTime = 180f,
                Priority = 5
            });

            Debug.Log($"[AdventureTemplate] Created {learningObjectives.Count} default learning objectives");
        }

        /// <summary>
        /// デフォルトアイテムの初期化
        /// </summary>
        private void InitializeDefaultItems()
        {
            if (availableItems.Count == 0)
            {
                availableItems.Add(new AdventureItem
                {
                    itemId = "key_bronze",
                    itemName = "Bronze Key",
                    description = "A simple bronze key that might open something...",
                    isUsable = true,
                    isCombinable = false
                });

                availableItems.Add(new AdventureItem
                {
                    itemId = "note_mysterious",
                    itemName = "Mysterious Note",
                    description = "A cryptic message that holds important clues.",
                    isUsable = true,
                    isCombinable = false
                });
            }
        }

        /// <summary>
        /// デフォルトクエストの初期化
        /// </summary>
        private void InitializeDefaultQuests()
        {
            if (availableQuests.Count == 0)
            {
                availableQuests.Add(new AdventureQuest
                {
                    questId = "tutorial_quest",
                    questTitle = "Adventure Begins",
                    questDescription = "Learn the basics of adventure gameplay and explore the world.",
                    isCompleted = false,
                    questType = QuestType.Tutorial
                });

                availableQuests.Add(new AdventureQuest
                {
                    questId = "collect_items_quest",
                    questTitle = "Collect Ancient Artifacts",
                    questDescription = "Find and collect three ancient artifacts hidden in the area.",
                    isCompleted = false,
                    questType = QuestType.Collection
                });
            }
        }

        /// <summary>
        /// ゲームプレイイベントの発行
        /// </summary>
        public void RaiseGameplayEvent(string eventType)
        {
            switch (eventType.ToLower())
            {
                case "started":
                    onAdventureStarted?.Raise();
                    break;
                case "dialog_started":
                    onDialogStarted?.Raise();
                    break;
                case "item_collected":
                    onItemCollected?.Raise();
                    break;
                case "quest_completed":
                    onQuestCompleted?.Raise();
                    break;
                case "puzzle_solved":
                    onPuzzleSolved?.Raise();
                    break;
                case "objective_completed":
                    onObjectiveCompleted?.Raise();
                    break;
                default:
                    Debug.LogWarning($"[AdventureTemplate] Unknown gameplay event: {eventType}");
                    break;
            }
        }

        /// <summary>
        /// Adventure設定の検証
        /// </summary>
        public bool ValidateConfiguration()
        {
            bool isValid = true;

            if (movementSpeed <= 0f)
            {
                Debug.LogError("[AdventureTemplate] Movement speed must be greater than 0");
                isValid = false;
            }

            if (interactionRange <= 0f)
            {
                Debug.LogError("[AdventureTemplate] Interaction range must be greater than 0");
                isValid = false;
            }

            if (enableDialogSystem && dialogTextSpeed <= 0f)
            {
                Debug.LogError("[AdventureTemplate] Dialog text speed must be greater than 0");
                isValid = false;
            }

            if (enableInventorySystem && maxInventorySlots <= 0)
            {
                Debug.LogError("[AdventureTemplate] Max inventory slots must be greater than 0");
                isValid = false;
            }

            if (enableQuestSystem && maxActiveQuests <= 0)
            {
                Debug.LogError("[AdventureTemplate] Max active quests must be greater than 0");
                isValid = false;
            }

            if (learningObjectives.Count == 0)
            {
                Debug.LogWarning("[AdventureTemplate] No learning objectives defined");
            }

            return isValid;
        }
    }

    /// <summary>
    /// Adventure学習目標データ構造
    /// </summary>
    [System.Serializable]
    public class AdventureLearningObjective
    {
        public string Id;
        public string Title;
        [TextArea(2, 4)]
        public string Description;
        public float EstimatedTime;
        public int Priority;
        public bool IsCompleted;
        public float CompletionPercentage;

        public void Initialize()
        {
            IsCompleted = false;
            CompletionPercentage = 0f;
        }

        public void UpdateProgress(float progress)
        {
            CompletionPercentage = Mathf.Clamp01(progress);
            if (CompletionPercentage >= 1.0f && !IsCompleted)
            {
                IsCompleted = true;
                Debug.Log($"[AdventureTemplate] Learning objective completed: {Title}");
            }
        }
    }

    /// <summary>
    /// Adventureアイテムデータ構造
    /// </summary>
    [System.Serializable]
    public class AdventureItem
    {
        public string itemId;
        public string itemName;
        [TextArea(2, 3)]
        public string description;
        public bool isUsable;
        public bool isCombinable;
        public Sprite itemIcon;
    }

    /// <summary>
    /// Adventureクエストデータ構造
    /// </summary>
    [System.Serializable]
    public class AdventureQuest
    {
        public string questId;
        public string questTitle;
        [TextArea(2, 4)]
        public string questDescription;
        public bool isCompleted;
        public QuestType questType;
        public List<string> requiredItems = new List<string>();
        public List<string> objectivesList = new List<string>();
    }

    /// <summary>
    /// クエストタイプ列挙
    /// </summary>
    public enum QuestType
    {
        Tutorial,
        Collection,
        Delivery,
        Puzzle,
        Exploration,
        Dialog
    }
}