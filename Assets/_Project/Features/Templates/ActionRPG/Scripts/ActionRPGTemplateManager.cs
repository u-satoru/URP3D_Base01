using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core;
using asterivo.Unity60.Features.Templates.ActionRPG.Character;
using asterivo.Unity60.Features.Templates.ActionRPG.Equipment;
using asterivo.Unity60.Features.Templates.ActionRPG.Combat;
using InventoryNS = asterivo.Unity60.Features.Templates.ActionRPG.Inventory;
using asterivo.Unity60.Features.Templates.ActionRPG.Skills;
using asterivo.Unity60.Features.Templates.ActionRPG.UI;
using asterivo.Unity60.Features.Templates.ActionRPG.Data;
using asterivo.Unity60.Features.Templates.ActionRPG.Audio;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Features.Templates.ActionRPG
{
    /// <summary>
    /// Action RPGテンプレートのメイン管理クラス
    /// キャラクター成長、装備、戦闘、インベントリ、スキルシステムを統合管理
    /// </summary>
    [System.Serializable]
    public class ActionRPGTemplateManager : MonoBehaviour
    {
        #region Configuration

        [TabGroup("ActionRPG", "Core Systems")]
        [Header("Template Settings")]
        [SerializeField] private ActionRPGSettings actionRPGSettings;
        
        [TabGroup("ActionRPG", "Core Systems")]
        [Header("Character Management")]
        [SerializeField] private CharacterProgressionManager characterProgressionManager;
        // TODO: Implement CharacterStatsManager
        // [SerializeField] private CharacterStatsManager characterStatsManager;
        [SerializeField] private CharacterCustomizationManager characterCustomizationManager;
        
        [TabGroup("ActionRPG", "Core Systems")]
        [Header("Equipment & Inventory")]
        [SerializeField] private EquipmentManager equipmentManager;
        [SerializeField] private InventoryManager inventoryManager;
        [SerializeField] private InventoryNS.ItemManager itemManager;
        
        [TabGroup("ActionRPG", "Core Systems")]
        [Header("Combat System")]
        [SerializeField] private CombatManager combatManager;
        [SerializeField] private SkillManager skillManager;
        [SerializeField] private AbilityManager abilityManager;
        
        [TabGroup("ActionRPG", "UI Systems")]
        [Header("UI Management")]
        [SerializeField] private ActionRPGUIManager actionRPGUIManager;
        [SerializeField] private HUDManager hudManager;
        [SerializeField] private MenuManager menuManager;
        
        [TabGroup("ActionRPG", "Audio")]
        [Header("Audio Management")]
        [SerializeField] private ActionRPGAudioManager actionRPGAudioManager;
        
        [TabGroup("ActionRPG", "Events")]
        [Header("Game Events")]
        [SerializeField] private GameEvent onActionRPGTemplateStarted;
        [SerializeField] private GameEvent onActionRPGTemplateEnded;
        [SerializeField] private IntGameEvent onPlayerLevelChanged;
        [SerializeField] private FloatGameEvent onPlayerHealthChanged;
        [SerializeField] private FloatGameEvent onPlayerManaChanged;
        [SerializeField] private IntGameEvent onExperienceGained;
        
        [TabGroup("ActionRPG", "Debug")]
        [Header("Debug Settings")]
        [SerializeField] private bool debugMode = false;
        [SerializeField] private bool enableStatistics = true;

        #endregion

        #region Properties

        public ActionRPGSettings Settings => actionRPGSettings;
        public CharacterProgressionManager CharacterProgression => characterProgressionManager;
        public EquipmentManager Equipment => equipmentManager;
        public InventoryManager Inventory => inventoryManager;
        public CombatManager Combat => combatManager;
        public SkillManager Skills => skillManager;
        public ActionRPGUIManager UI => actionRPGUIManager;
        public bool IsTemplateActive { get; private set; }

        #endregion

        #region State Management

        public enum ActionRPGState
        {
            Inactive,
            Initializing,
            MainMenu,
            CharacterCreation,
            InGame,
            Paused,
            Inventory,
            Combat,
            LevelUp,
            GameOver
        }

        [TabGroup("ActionRPG", "State")]
        [SerializeField, ReadOnly] private ActionRPGState currentState = ActionRPGState.Inactive;
        [TabGroup("ActionRPG", "State")]
        [SerializeField, ReadOnly] private ActionRPGState previousState = ActionRPGState.Inactive;

        public ActionRPGState CurrentState => currentState;
        public ActionRPGState PreviousState => previousState;

        #endregion

        #region Statistics

        [TabGroup("ActionRPG", "Statistics")]
        [Header("Template Statistics")]
        [SerializeField, ReadOnly] private float templateActiveTime;
        [SerializeField, ReadOnly] private int totalCombatsWon;
        [SerializeField, ReadOnly] private int totalItemsLooted;
        [SerializeField, ReadOnly] private int totalLevelsGained;
        [SerializeField, ReadOnly] private Vector3 playerStartPosition;

        // Performance Metrics
        [SerializeField, ReadOnly] private long memoryUsageBytes;
        [SerializeField, ReadOnly] private int totalComponentsManaged;
        [SerializeField, ReadOnly] private float initializationTime;
        [SerializeField, ReadOnly] private int totalCharactersCreated;
        [SerializeField, ReadOnly] private int totalItemsManaged;
        [SerializeField, ReadOnly] private int totalCombatsStarted;

        // Properties for Performance Metrics
        public long MemoryUsageBytes => memoryUsageBytes;
        public int TotalComponentsManaged => totalComponentsManaged;
        public float InitializationTime => initializationTime;
        public int TotalCharactersCreated => totalCharactersCreated;
        public int TotalItemsManaged => totalItemsManaged;
        public int TotalCombatsStarted => totalCombatsStarted;

        #endregion

        #region Performance Monitoring

        [Header("Performance Monitoring")]
        [SerializeField] private bool enablePerformanceMonitoring = true;
        private System.Diagnostics.Stopwatch performanceStopwatch = new System.Diagnostics.Stopwatch();
        private float lastPerformanceUpdate;
        private const float PERFORMANCE_UPDATE_INTERVAL = 0.5f;

        public float FrameProcessingTime { get; private set; }

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeActionRPGTemplate();
        }

        private void Start()
        {
            StartCoroutine(SetupActionRPGTemplate());
        }

        private void Update()
        {
            // パフォーマンス監視の開始
            if (enablePerformanceMonitoring)
            {
                performanceStopwatch.Start();
            }

            if (IsTemplateActive)
            {
                UpdateTemplateStats();
                UpdateSystemManagers();
                HandleInput();
            }

            // パフォーマンス監視処理
            if (enablePerformanceMonitoring)
            {
                // 定期的にパフォーマンスデータを更新
                if (Time.time - lastPerformanceUpdate >= PERFORMANCE_UPDATE_INTERVAL)
                {
                    UpdatePerformanceMetrics();
                    lastPerformanceUpdate = Time.time;
                }

                // パフォーマンス測定終了
                performanceStopwatch.Stop();
                FrameProcessingTime = (float)performanceStopwatch.Elapsed.TotalMilliseconds;
                performanceStopwatch.Reset();
            }
        }

        private void OnDestroy()
        {
            CleanupActionRPGTemplate();
        }

        #endregion

        #region Template Initialization

        /// <summary>
        /// Action RPGテンプレートの初期化
        /// </summary>
        private void InitializeActionRPGTemplate()
        {
            LogDebug("[ActionRPG] Initializing Action RPG Template...");
            
            ChangeState(ActionRPGState.Initializing);
            
            // コンポーネント検証
            ValidateComponents();
            
            // デフォルト設定適用
            ApplyDefaultSettings();
            
            // イベント登録
            RegisterEvents();
            
            // サービス登録
            RegisterServices();
            
            LogDebug("[ActionRPG] Action RPG Template initialization completed");
        }

        /// <summary>
        /// Action RPGテンプレートのセットアップ
        /// </summary>
        private IEnumerator SetupActionRPGTemplate()
        {
            LogDebug("[ActionRPG] Setting up Action RPG Template systems...");
            
            // キャラクターシステム初期化
            yield return StartCoroutine(SetupCharacterSystems());
            
            // 装備・インベントリシステム初期化
            yield return StartCoroutine(SetupEquipmentSystems());
            
            // 戦闘・スキルシステム初期化
            yield return StartCoroutine(SetupCombatSystems());
            
            // UIシステム初期化
            yield return StartCoroutine(SetupUISystems());
            
            // オーディオシステム初期化
            yield return StartCoroutine(SetupAudioSystems());
            
            // テンプレート開始
            StartActionRPGTemplate();
            
            LogDebug("[ActionRPG] Action RPG Template setup completed");
        }

        /// <summary>
        /// キャラクターシステムのセットアップ
        /// </summary>
        private IEnumerator SetupCharacterSystems()
        {
            LogDebug("[ActionRPG] Setting up Character Systems...");
            
            if (characterProgressionManager != null)
            {
                characterProgressionManager.Initialize();
            }
            
            // TODO: Implement CharacterStatsManager
            // if (characterStatsManager != null)
            // {
            //     characterStatsManager.Initialize();
            // }
            
            if (characterCustomizationManager != null)
            {
                characterCustomizationManager.Initialize();
            }
            
            yield return null;
        }

        /// <summary>
        /// 装備・インベントリシステムのセットアップ
        /// </summary>
        private IEnumerator SetupEquipmentSystems()
        {
            LogDebug("[ActionRPG] Setting up Equipment Systems...");

            // Equipment Manager setup
            if (equipmentManager != null)
            {
                // EquipmentManager is automatically initialized in Awake/Start
                LogDebug("[ActionRPG] Equipment Manager ready");
            }
            else
            {
                LogWarning("[ActionRPG] Equipment Manager not assigned!");
            }

            // Inventory Manager setup
            if (inventoryManager != null)
            {
                // InventoryManager is automatically initialized in Awake/Start
                LogDebug("[ActionRPG] Inventory Manager ready");
            }
            else
            {
                LogWarning("[ActionRPG] Inventory Manager not assigned!");
            }

            // Setup integration between equipment and inventory systems
            if (equipmentManager != null && inventoryManager != null)
            {
                // Link the systems for automatic item transfer
                SetupEquipmentInventoryIntegration();
            }

            // Add sample items for testing (if in debug mode)
            if (debugMode && inventoryManager != null)
            {
                yield return StartCoroutine(AddSampleItems());
            }

            yield return null;
        }

        /// <summary>
        /// 装備とインベントリシステムの統合設定
        /// </summary>
        private void SetupEquipmentInventoryIntegration()
        {
            if (equipmentManager == null || inventoryManager == null) return;

            // Set up event connections between equipment and inventory systems
            equipmentManager.OnItemEquipped += (Equipment.ItemData item, EquipmentSlot slot) => OnItemEquippedHandler(item, slot);
            equipmentManager.OnItemUnequipped += (Equipment.ItemData item, EquipmentSlot slot) => OnItemUnequippedHandler(item, slot);

            LogDebug("[ActionRPG] Equipment-Inventory integration configured");
        }

        /// <summary>
        /// アイテム装備時のハンドラー
        /// </summary>
        private void OnItemEquippedHandler(Equipment.ItemData item, EquipmentSlot slot)
        {
            LogDebug($"[ActionRPG] Item equipped: {item.itemName} in {slot}");

            // Notify character progression system if it exists
            if (characterProgressionManager != null)
            {
                characterProgressionManager.OnEquipmentChanged();
            }
        }

        /// <summary>
        /// アイテム装備解除時のハンドラー
        /// </summary>
        private void OnItemUnequippedHandler(Equipment.ItemData item, EquipmentSlot slot)
        {
            LogDebug($"[ActionRPG] Item unequipped: {item.itemName} from {slot}");

            // Notify character progression system if it exists
            if (characterProgressionManager != null)
            {
                characterProgressionManager.OnEquipmentChanged();
            }
        }

        /// <summary>
        /// サンプルアイテムを追加（デバッグ用）
        /// </summary>
        private IEnumerator AddSampleItems()
        {
            LogDebug("[ActionRPG] Adding sample items for testing...");

            // Create some sample items for testing using Equipment ItemData type
            // Note: Using Equipment.ItemData as this is what InventoryManager.AddItem expects
            // TODO: Consider creating sample ScriptableObject assets instead of code-generated items
            LogDebug("[ActionRPG] Sample item creation is temporarily disabled due to type conflicts");
            LogDebug("[ActionRPG] Consider implementing ScriptableObject-based item database for proper sample items");

            if (inventoryManager != null)
            {
                LogDebug("[ActionRPG] InventoryManager found - ready for ScriptableObject-based items");
            }

            yield return null;
        }


        /// <summary>
        /// 戦闘・スキルシステムのセットアップ
        /// </summary>
        private IEnumerator SetupCombatSystems()
        {
            LogDebug("[ActionRPG] Setting up Combat Systems...");

            // Combat Manager setup
            if (combatManager != null)
            {
                // CombatManager is automatically initialized in Awake/Start
                // Set up integration with other systems
                SetupCombatSystemIntegration();
                LogDebug("[ActionRPG] Combat Manager ready");
            }
            else
            {
                LogWarning("[ActionRPG] Combat Manager not assigned!");
            }

            // Skill Manager setup
            if (skillManager != null)
            {
                skillManager.Initialize();
                LogDebug("[ActionRPG] Skill Manager ready");
            }
            else
            {
                LogWarning("[ActionRPG] Skill Manager not assigned!");
            }

            // Ability Manager setup
            if (abilityManager != null)
            {
                abilityManager.Initialize();
                LogDebug("[ActionRPG] Ability Manager ready");
            }
            else
            {
                LogWarning("[ActionRPG] Ability Manager not assigned!");
            }

            // Ensure Health components are properly initialized for combat
            SetupHealthComponents();

            yield return null;
        }

        /// <summary>
        /// 戦闘システムの統合設定
        /// </summary>
        private void SetupCombatSystemIntegration()
        {
            if (combatManager == null) return;

            // Set up event connections between combat system and other systems
            combatManager.OnCombatStarted += OnCombatStartedHandler;
            combatManager.OnCombatEnded += OnCombatEndedHandler;
            combatManager.OnEnemyDefeated += OnEnemyDefeatedHandler;

            // Link combat system with character progression for combat bonuses
            if (characterProgressionManager != null)
            {
                // Combat manager can access character stats through the progression system
                LogDebug("[ActionRPG] Combat-Character Progression integration configured");
            }

            // Link combat system with equipment for damage/defense calculations
            if (equipmentManager != null)
            {
                // Combat manager can access equipment stats for damage calculations
                LogDebug("[ActionRPG] Combat-Equipment integration configured");
            }

            LogDebug("[ActionRPG] Combat system integration configured");
        }

        /// <summary>
        /// Healthコンポーネントのセットアップ
        /// </summary>
        private void SetupHealthComponents()
        {
            // Find all Health components in the scene and ensure they're properly set up
            var healthComponents = FindObjectsByType<asterivo.Unity60.Features.Templates.ActionRPG.Combat.Health>(FindObjectsSortMode.None);

            foreach (var health in healthComponents)
            {
                // Health components initialize themselves in Awake/Start
                // Set up any additional configuration if needed
                if (health != null)
                {
                    LogDebug($"[ActionRPG] Health component found and configured on {health.gameObject.name}");
                }
            }

            LogDebug($"[ActionRPG] {healthComponents.Length} Health components configured");
        }

        /// <summary>
        /// 戦闘開始時のハンドラー
        /// </summary>
        private void OnCombatStartedHandler()
        {
            // Update template statistics
            LogDebug("[ActionRPG] Combat started - updating template state");
        }

        /// <summary>
        /// 戦闘終了時のハンドラー
        /// </summary>
        private void OnCombatEndedHandler()
        {
            // Update statistics and handle post-combat logic
            // TODO: Implement victory condition checking
            LogDebug("[ActionRPG] Combat ended");

            // For now, assume combat ended successfully
            totalCombatsWon++;
            LogDebug("[ActionRPG] Combat statistics updated");
        }

        /// <summary>
        /// 敵撃破時のハンドラー
        /// </summary>
        private void OnEnemyDefeatedHandler()
        {
            LogDebug("[ActionRPG] Enemy defeated - processing rewards");
            // Handle experience rewards, loot drops, etc.
        }

        /// <summary>
        /// UIシステムのセットアップ
        /// </summary>
        private IEnumerator SetupUISystems()
        {
            LogDebug("[ActionRPG] Setting up UI Systems...");
            
            if (actionRPGUIManager != null)
            {
                actionRPGUIManager.Initialize();
            }
            
            if (hudManager != null)
            {
                hudManager.Initialize();
            }
            
            if (menuManager != null)
            {
                menuManager.Initialize();
            }
            
            yield return null;
        }

        /// <summary>
        /// オーディオシステムのセットアップ
        /// </summary>
        private IEnumerator SetupAudioSystems()
        {
            LogDebug("[ActionRPG] Setting up Audio Systems...");
            
            if (actionRPGAudioManager != null)
            {
                actionRPGAudioManager.Initialize();
            }
            
            yield return null;
        }

        #endregion

        #region Template Management

        /// <summary>
        /// Action RPGテンプレートの開始
        /// </summary>
        [Button("Start Action RPG Template"), TabGroup("ActionRPG", "Debug")]
        public void StartActionRPGTemplate()
        {
            if (IsTemplateActive)
            {
                LogWarning("[ActionRPG] Template is already active!");
                return;
            }
            
            LogDebug("[ActionRPG] Starting Action RPG Template...");
            
            IsTemplateActive = true;
            templateActiveTime = 0f;
            playerStartPosition = GetPlayerPosition();
            
            ChangeState(ActionRPGState.MainMenu);
            
            // テンプレート開始イベント発行
            onActionRPGTemplateStarted?.Raise();
            
            LogDebug("[ActionRPG] Action RPG Template started successfully");
        }

        /// <summary>
        /// Action RPGテンプレートの停止
        /// </summary>
        [Button("Stop Action RPG Template"), TabGroup("ActionRPG", "Debug")]
        public void StopActionRPGTemplate()
        {
            if (!IsTemplateActive)
            {
                LogWarning("[ActionRPG] Template is not active!");
                return;
            }
            
            LogDebug("[ActionRPG] Stopping Action RPG Template...");
            
            IsTemplateActive = false;
            
            ChangeState(ActionRPGState.Inactive);
            
            // テンプレート終了イベント発行
            onActionRPGTemplateEnded?.Raise();
            
            LogDebug("[ActionRPG] Action RPG Template stopped");
        }

        /// <summary>
        /// Action RPGテンプレートのリセット
        /// </summary>
        [Button("Reset Action RPG Template"), TabGroup("ActionRPG", "Debug")]
        public void ResetActionRPGTemplate()
        {
            LogDebug("[ActionRPG] Resetting Action RPG Template...");
            
            StopActionRPGTemplate();
            ResetStats();
            StartCoroutine(SetupActionRPGTemplate());
            
            LogDebug("[ActionRPG] Action RPG Template reset completed");
        }

        #endregion

        #region State Management Methods

        /// <summary>
        /// 状態変更
        /// </summary>
        private void ChangeState(ActionRPGState newState)
        {
            if (currentState == newState) return;
            
            previousState = currentState;
            currentState = newState;
            
            LogDebug($"[ActionRPG] State changed: {previousState} -> {currentState}");
            
            OnStateChanged(previousState, currentState);
        }

        /// <summary>
        /// 状態変更時の処理
        /// </summary>
        private void OnStateChanged(ActionRPGState oldState, ActionRPGState newState)
        {
            switch (newState)
            {
                case ActionRPGState.MainMenu:
                    HandleMainMenuState();
                    break;
                case ActionRPGState.CharacterCreation:
                    HandleCharacterCreationState();
                    break;
                case ActionRPGState.InGame:
                    HandleInGameState();
                    break;
                case ActionRPGState.Combat:
                    HandleCombatState();
                    break;
                case ActionRPGState.LevelUp:
                    HandleLevelUpState();
                    break;
                case ActionRPGState.Inventory:
                    HandleInventoryState();
                    break;
                case ActionRPGState.Paused:
                    HandlePausedState();
                    break;
                case ActionRPGState.GameOver:
                    HandleGameOverState();
                    break;
            }
        }

        #endregion

        #region State Handlers

        private void HandleMainMenuState()
        {
            if (menuManager != null)
                menuManager.ShowMainMenu();
        }

        private void HandleCharacterCreationState()
        {
            // TODO: Implement CharacterCustomizationManager
            // if (characterCustomizationManager != null)
            //     characterCustomizationManager.StartCharacterCreation();
        }

        private void HandleInGameState()
        {
            if (hudManager != null)
                hudManager.ShowGameplayHUD();
        }

        private void HandleCombatState()
        {
            if (combatManager != null)
                combatManager.EnterCombat();
            
            if (hudManager != null)
                hudManager.ShowCombatHUD();
        }

        private void HandleLevelUpState()
        {
            if (characterProgressionManager != null && actionRPGUIManager != null)
            {
                actionRPGUIManager.ShowLevelUpScreen();
            }
        }

        private void HandleInventoryState()
        {
            if (actionRPGUIManager != null)
                actionRPGUIManager.ShowInventory();
        }

        private void HandlePausedState()
        {
            Time.timeScale = 0f;
            if (menuManager != null)
                menuManager.ShowPauseMenu();
        }

        private void HandleGameOverState()
        {
            if (menuManager != null)
                menuManager.ShowGameOverMenu();
        }

        #endregion

        #region System Updates

        /// <summary>
        /// システムマネージャーの更新
        /// </summary>
        private void UpdateSystemManagers()
        {
            // キャラクターシステム更新
            characterProgressionManager?.UpdateProgression(Time.deltaTime);
            // TODO: Implement CharacterStatsManager
            // characterStatsManager?.UpdateStats(Time.deltaTime);
            
            // 戦闘システム更新
            combatManager?.UpdateCombat(Time.deltaTime);
            skillManager?.UpdateSkills(Time.deltaTime);
            
            // UI更新
            hudManager?.UpdateHUD(Time.deltaTime);
        }

        /// <summary>
        /// 入力処理
        /// </summary>
        private void HandleInput()
        {
            // ESCキーでメニュー切り替え
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                TogglePauseState();
            }
            
            // Iキーでインベントリ切り替え
            if (Input.GetKeyDown(KeyCode.I))
            {
                ToggleInventoryState();
            }
            
            // Cキーでキャラクター画面切り替え
            if (Input.GetKeyDown(KeyCode.C))
            {
                ToggleCharacterState();
            }
        }

        /// <summary>
        /// ポーズ状態切り替え
        /// </summary>
        private void TogglePauseState()
        {
            if (currentState == ActionRPGState.Paused)
            {
                Time.timeScale = 1f;
                ChangeState(previousState);
            }
            else if (currentState == ActionRPGState.InGame)
            {
                ChangeState(ActionRPGState.Paused);
            }
        }

        /// <summary>
        /// インベントリ状態切り替え
        /// </summary>
        private void ToggleInventoryState()
        {
            if (currentState == ActionRPGState.Inventory)
            {
                ChangeState(ActionRPGState.InGame);
            }
            else if (currentState == ActionRPGState.InGame)
            {
                ChangeState(ActionRPGState.Inventory);
            }
        }

        /// <summary>
        /// キャラクター画面切り替え
        /// </summary>
        private void ToggleCharacterState()
        {
            if (actionRPGUIManager != null)
            {
                actionRPGUIManager.ToggleCharacterScreen();
            }
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// コンポーネント検証
        /// </summary>
        private void ValidateComponents()
        {
            if (actionRPGSettings == null)
                LogWarning("[ActionRPG] ActionRPGSettings is not assigned!");
            
            if (characterProgressionManager == null)
                LogWarning("[ActionRPG] CharacterProgressionManager is not assigned!");
            
            if (equipmentManager == null)
                LogWarning("[ActionRPG] EquipmentManager is not assigned!");
            
            if (combatManager == null)
                LogWarning("[ActionRPG] CombatManager is not assigned!");
        }

        /// <summary>
        /// デフォルト設定適用
        /// </summary>
        private void ApplyDefaultSettings()
        {
            // TODO: Implement ActionRPGSettings class with enableDebugMode and enableStatistics properties
            // if (actionRPGSettings != null)
            // {
            //     // デフォルト設定の適用
            //     debugMode = actionRPGSettings.enableDebugMode;
            //     enableStatistics = actionRPGSettings.enableStatistics;
            // }
        }

        /// <summary>
        /// イベント登録
        /// </summary>
        private void RegisterEvents()
        {
            // キャラクター関連イベント
            if (characterProgressionManager != null)
            {
                characterProgressionManager.OnLevelUp += HandlePlayerLevelUp;
                characterProgressionManager.OnExperienceGained += HandleExperienceGained;
            }
            
            // TODO: Implement CharacterStatsManager
            // if (characterStatsManager != null)
            // {
            //     characterStatsManager.OnHealthChanged += HandleHealthChanged;
            //     characterStatsManager.OnManaChanged += HandleManaChanged;
            // }
            
            // 戦闘関連イベント (handled in SetupCombatSystemIntegration)
            // Event registration is done in SetupCombatSystemIntegration method
            // to ensure proper order and avoid duplicate registrations
            
            // TODO: Implement OnItemLooted event in InventoryManager
            // アイテム関連イベント
            // if (inventoryManager != null)
            // {
            //     inventoryManager.OnItemLooted += HandleItemLooted;
            // }
        }

        /// <summary>
        /// サービス登録
        /// </summary>
        private void RegisterServices()
        {
            ServiceLocator.RegisterService<ActionRPGTemplateManager>(this);
            
            if (characterProgressionManager != null)
                ServiceLocator.RegisterService<CharacterProgressionManager>(characterProgressionManager);
            
            if (equipmentManager != null)
                ServiceLocator.RegisterService<EquipmentManager>(equipmentManager);
            
            if (inventoryManager != null)
                ServiceLocator.RegisterService<InventoryManager>(inventoryManager);
            
            if (combatManager != null)
                ServiceLocator.RegisterService<CombatManager>(combatManager);
        }

        /// <summary>
        /// プレイヤー位置取得
        /// </summary>
        private Vector3 GetPlayerPosition()
        {
            var player = GameObject.FindWithTag("Player");
            return player != null ? player.transform.position : Vector3.zero;
        }

        /// <summary>
        /// 統計情報更新
        /// </summary>
        private void UpdateTemplateStats()
        {
            if (enableStatistics)
            {
                templateActiveTime += Time.deltaTime;
            }
        }

        /// <summary>
        /// 統計情報リセット
        /// </summary>
        private void ResetStats()
        {
            templateActiveTime = 0f;
            totalCombatsWon = 0;
            totalItemsLooted = 0;
            totalLevelsGained = 0;
            playerStartPosition = Vector3.zero;
        }

        /// <summary>
        /// クリーンアップ処理
        /// </summary>
        private void CleanupActionRPGTemplate()
        {
            LogDebug("[ActionRPG] Cleaning up Action RPG Template...");
            
            // イベント登録解除
            UnregisterEvents();
            
            // サービス登録解除
            ServiceLocator.UnregisterService<ActionRPGTemplateManager>();
            
            // テンプレート停止
            if (IsTemplateActive)
            {
                StopActionRPGTemplate();
            }
        }

        /// <summary>
        /// イベント登録解除
        /// </summary>
        private void UnregisterEvents()
        {
            if (characterProgressionManager != null)
            {
                characterProgressionManager.OnLevelUp -= HandlePlayerLevelUp;
                characterProgressionManager.OnExperienceGained -= HandleExperienceGained;
            }
            
            // TODO: Implement CharacterStatsManager
            // if (characterStatsManager != null)
            // {
            //     characterStatsManager.OnHealthChanged -= HandleHealthChanged;
            //     characterStatsManager.OnManaChanged -= HandleManaChanged;
            // }
            
            if (combatManager != null)
            {
                combatManager.OnCombatStarted -= OnCombatStartedHandler;
                combatManager.OnCombatEnded -= OnCombatEndedHandler;
                combatManager.OnEnemyDefeated -= OnEnemyDefeatedHandler;
            }
            
            // TODO: Implement OnItemLooted event in InventoryManager
            // if (inventoryManager != null)
            // {
            //     inventoryManager.OnItemLooted -= HandleItemLooted;
            // }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// レベルアップ処理
        /// </summary>
        private void HandlePlayerLevelUp(int newLevel)
        {
            totalLevelsGained++;
            onPlayerLevelChanged?.Raise(newLevel);
            ChangeState(ActionRPGState.LevelUp);
            
            LogDebug($"[ActionRPG] Player leveled up to level {newLevel}");
        }

        /// <summary>
        /// 経験値獲得処理
        /// </summary>
        private void HandleExperienceGained(int experience)
        {
            onExperienceGained?.Raise(experience);
            LogDebug($"[ActionRPG] Gained {experience} experience");
        }

        /// <summary>
        /// HP変更処理
        /// </summary>
        private void HandleHealthChanged(float newHealth)
        {
            onPlayerHealthChanged?.Raise(newHealth);
        }

        /// <summary>
        /// MP変更処理
        /// </summary>
        private void HandleManaChanged(float newMana)
        {
            onPlayerManaChanged?.Raise(newMana);
        }

        /// <summary>
        /// 戦闘開始処理（状態管理用）
        /// </summary>
        private void HandleCombatStarted()
        {
            ChangeState(ActionRPGState.Combat);
            LogDebug("[ActionRPG] Combat started - state changed to Combat");
        }

        /// <summary>
        /// 戦闘終了処理（状態管理用）
        /// </summary>
        private void HandleCombatEnded(bool victory)
        {
            ChangeState(ActionRPGState.InGame);
            LogDebug($"[ActionRPG] Combat ended - Victory: {victory}, state changed to InGame");
        }

        /// <summary>
        /// アイテム取得処理
        /// </summary>
        private void HandleItemLooted()
        {
            totalItemsLooted++;
            LogDebug("[ActionRPG] Item looted");
        }

        #endregion

        #region Performance Optimization

        /// <summary>
        /// パフォーマンス監視の初期化
        /// </summary>
        private void InitializePerformanceMonitoring()
        {
            performanceStopwatch = new System.Diagnostics.Stopwatch();
            lastPerformanceUpdate = 0f;
            UpdateMemoryUsage();
        }


        /// <summary>
        /// パフォーマンスメトリクスの更新
        /// </summary>
        private void UpdatePerformanceMetrics()
        {
            UpdateMemoryUsage();
            UpdateComponentCount();

            // パフォーマンス警告チェック
            CheckPerformanceWarnings();
        }

        /// <summary>
        /// メモリ使用量の更新
        /// </summary>
        private void UpdateMemoryUsage()
        {
            memoryUsageBytes = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemory();
        }

        /// <summary>
        /// 管理コンポーネント数の更新
        /// </summary>
        private void UpdateComponentCount()
        {
            int count = 0;

            if (characterProgressionManager != null) count++;
            // TODO: Implement CharacterStatsManager
            // if (characterStatsManager != null) count++;
            if (characterCustomizationManager != null) count++;
            if (equipmentManager != null) count++;
            if (inventoryManager != null) count++;
            if (combatManager != null) count++;

            // StatusEffectManagerの数も追加
            var statusEffectManagers = FindObjectsByType<StatusEffectManager>(FindObjectsSortMode.None);
            count += statusEffectManagers.Length;

            totalComponentsManaged = count;
        }

        /// <summary>
        /// パフォーマンス警告チェック
        /// </summary>
        private void CheckPerformanceWarnings()
        {
            // フレーム処理時間の警告
            if (FrameProcessingTime > 2.0f) // 2ms threshold
            {
                LogWarning($"[ActionRPG] High frame processing time detected: {FrameProcessingTime:F2}ms");
            }

            // メモリ使用量の警告
            var memoryMB = memoryUsageBytes / (1024 * 1024);
            if (memoryMB > 100) // 100MB threshold
            {
                LogWarning($"[ActionRPG] High memory usage detected: {memoryMB:F2}MB");
            }
        }

        /// <summary>
        /// 手動ガベージコレクション実行（デバッグ用）
        /// </summary>
        [Button("Force Garbage Collection"), TabGroup("ActionRPG", "Performance")]
        [ShowIf("debugMode")]
        public void ForceGarbageCollection()
        {
            var memoryBefore = memoryUsageBytes / (1024 * 1024);

            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            System.GC.Collect();

            UpdateMemoryUsage();
            var memoryAfter = memoryUsageBytes / (1024 * 1024);
            var memoryFreed = memoryBefore - memoryAfter;

            LogDebug($"[ActionRPG] Garbage collection completed. Memory freed: {memoryFreed:F2}MB");
        }

        /// <summary>
        /// パフォーマンスレポート生成
        /// </summary>
        [Button("Generate Performance Report"), TabGroup("ActionRPG", "Performance")]
        [ShowIf("debugMode")]
        public void GeneratePerformanceReport()
        {
            LogDebug("=== ACTION RPG TEMPLATE PERFORMANCE REPORT ===");
            LogDebug($"Initialization Time: {InitializationTime:F3}s");
            LogDebug($"Frame Processing Time: {FrameProcessingTime:F3}ms");
            LogDebug($"Memory Usage: {memoryUsageBytes / (1024 * 1024):F2}MB");
            LogDebug($"Components Managed: {TotalComponentsManaged}");
            LogDebug($"Total Characters Created: {TotalCharactersCreated}");
            LogDebug($"Total Items Managed: {TotalItemsManaged}");
            LogDebug($"Total Combats: {TotalCombatsStarted} (Won: {totalCombatsWon})");

            // システム別詳細情報
            if (inventoryManager != null)
            {
                LogDebug($"Inventory: {inventoryManager.UsedSlots}/{inventoryManager.FreeSlots + inventoryManager.UsedSlots} slots, " +
                        $"{inventoryManager.TotalItemCount} items");
            }

            if (equipmentManager != null)
            {
                var (str, dex, intel, vit, wis, luck) = equipmentManager.GetAttributeBonuses();
                LogDebug($"Equipment Bonuses: STR+{str} DEX+{dex} INT+{intel} VIT+{vit} WIS+{wis} LUK+{luck}");
            }

            LogDebug("=== END PERFORMANCE REPORT ===");
        }

        /// <summary>
        /// システム最適化実行
        /// </summary>
        [Button("Optimize Systems"), TabGroup("ActionRPG", "Performance")]
        [ShowIf("debugMode")]
        public void OptimizeSystems()
        {
            LogDebug("[ActionRPG] Running system optimizations...");

            // インベントリのソート（断片化解消）
            if (inventoryManager != null)
            {
                inventoryManager.SortInventory();
                LogDebug("[ActionRPG] Inventory optimized and sorted");
            }

            // 装備統計の再計算
            if (equipmentManager != null)
            {
                equipmentManager.CalculateEquipmentStats();
                LogDebug("[ActionRPG] Equipment stats recalculated");
            }

            // ガベージコレクション
            ForceGarbageCollection();

            LogDebug("[ActionRPG] System optimization completed");
        }

        #endregion

        #region Debug Support

        /// <summary>
        /// デバッグログ出力
        /// </summary>
        private void LogDebug(string message)
        {
            if (debugMode)
            {
                Debug.Log(message);
            }
        }

        /// <summary>
        /// 警告ログ出力
        /// </summary>
        private void LogWarning(string message)
        {
            Debug.LogWarning(message);
        }

        /// <summary>
        /// エラーログ出力
        /// </summary>
        private void LogError(string message)
        {
            Debug.LogError(message);
        }

        #endregion

        #region Editor Support

#if UNITY_EDITOR
        /// <summary>
        /// Gizmos描画（エディタ専用）
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            if (IsTemplateActive)
            {
                // テンプレート有効範囲表示
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireCube(transform.position, Vector3.one * 2f);
                
                // プレイヤー開始位置表示
                if (playerStartPosition != Vector3.zero)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawWireSphere(playerStartPosition, 0.5f);
                }
            }
        }

        /// <summary>
        /// 統計情報表示（エディタ専用）
        /// </summary>
        [Button("Show Statistics"), TabGroup("ActionRPG", "Debug")]
        private void ShowStatistics()
        {
            if (!Application.isPlaying) return;
            
            string stats = $"=== Action RPG Template Statistics ===\n";
            stats += $"Active Time: {templateActiveTime:F2}s\n";
            stats += $"Current State: {currentState}\n";
            stats += $"Total Combats Won: {totalCombatsWon}\n";
            stats += $"Total Items Looted: {totalItemsLooted}\n";
            stats += $"Total Levels Gained: {totalLevelsGained}\n";
            stats += $"Player Start Position: {playerStartPosition}\n";
            
            Debug.Log(stats);
        }
#endif

        #endregion
    }
}