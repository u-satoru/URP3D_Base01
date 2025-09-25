using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Commands;
using asterivo.Unity60.Core.Audio.Interfaces;
using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Core.Data;
using asterivo.Unity60.Features.Player.Scripts;
using asterivo.Unity60.Player;
using asterivo.Unity60.Features.Templates.Stealth.UI;
using asterivo.Unity60.Features.Templates.Stealth.Data;
using Sirenix.OdinInspector;
using UnityEngine.AI;

namespace asterivo.Unity60.Features.Templates.Stealth
{
    /// <summary>
    /// ステルスゲームプレイマネージャー - 15分完結ステルスゲーム体験の実現
    /// 最優先度ジャンルとしてのステルスアクション完全実装
    /// Mission系統、NPC管理、検知システム、成功/失敗条件を統合管理
    /// </summary>
    public class StealthGameplayManager : MonoBehaviour
    {
        #region Gameplay Configuration

        [TabGroup("Gameplay", "Mission Settings")]
        [Title("15-Minute Stealth Gameplay Configuration", "完結型ステルスアクション体験", TitleAlignments.Centered)]

        [Header("Game Duration Settings")]
        [SerializeField] private float missionTimeLimit = 900f; // 15 minutes
        [SerializeField] private bool enableTimeLimit = true;
        [SerializeField] private bool enableMissionObjectives = true;

        [Header("Difficulty Configuration")]
        [SerializeField] private StealthDifficulty difficultyLevel = StealthDifficulty.Normal;
        [SerializeField] private float detectionSensitivityMultiplier = 1.0f;
        [SerializeField] private float npcReactionTimeMultiplier = 1.0f;

        [TabGroup("Gameplay", "Mission Objectives")]
        [Header("Mission Objective System")]
        [SerializeField] private List<StealthMissionObjective> missionObjectives = new List<StealthMissionObjective>();
        [SerializeField] private Transform[] keyItems;
        [SerializeField] private Transform[] targetLocations;
        [SerializeField] private Transform missionExitPoint;

        [TabGroup("Gameplay", "NPC Management")]
        [Header("NPC Patrol & Behavior Management")]
        [SerializeField] private List<NPCPatrolController> patrolNPCs = new List<NPCPatrolController>();
        [SerializeField] private float npcAlertDecayRate = 0.1f;
        [SerializeField] private float globalAlertLevelDecayTime = 30f;
        [SerializeField] private bool enableNPCCommunication = true;

        #endregion

        #region Game State Management

        [TabGroup("Gameplay", "Runtime Status")]
        [Header("Runtime Game State")]
        [SerializeField, ReadOnly] private StealthGameState currentGameState = StealthGameState.NotStarted;
        [SerializeField, ReadOnly] private float missionTimeRemaining;
        [SerializeField, ReadOnly] private int objectivesCompleted = 0;
        [SerializeField, ReadOnly] private int totalObjectives = 0;
        [SerializeField, ReadOnly] private GlobalAlertLevel globalAlertLevel = GlobalAlertLevel.Normal;

        // Game State Events
        [Header("Game Events")]
        [SerializeField] private GameEvent onGameStarted;
        [SerializeField] private GameEvent onGameCompleted;
        [SerializeField] private GameEvent onGameFailed;
        [SerializeField] private GameEvent onObjectiveCompleted;
        [SerializeField] private GameEvent onAlertLevelChanged;

        #endregion

        #region Service References

        private IStealthAudioService stealthAudioService;
        private ICommandInvoker commandInvoker;
        private PlayerStealthController playerController;
        private StealthTemplateManager templateManager;
        private StealthUIManager uiManager;

        // Runtime Collections
        private Dictionary<string, StealthMissionObjective> objectiveMap = new Dictionary<string, StealthMissionObjective>();
        private List<GameObject> collectedItems = new List<GameObject>();
        private Coroutine gameTimerCoroutine;
        private Coroutine alertDecayCoroutine;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeGameplayManager();
            SetupMissionObjectives();
        }

        private void Start()
        {
            if (enableMissionObjectives && missionObjectives.Count == 0)
            {
                GenerateDefaultMission();
            }
            StartStealthGameplay();
        }

        private void Update()
        {
            if (currentGameState == StealthGameState.InProgress)
            {
                UpdateGameplaySystems();
            }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// ゲームプレイマネージャーの初期化
        /// </summary>
        private void InitializeGameplayManager()
        {
            LogDebug("[StealthGameplayManager] Initializing 15-minute stealth gameplay system...");

            try
            {
                // Service References
                stealthAudioService = ServiceLocator.GetService<IStealthAudioService>();
                commandInvoker = ServiceLocator.GetService<ICommandInvoker>();

                // Component References
                templateManager = FindFirstObjectByType<StealthTemplateManager>();
                uiManager = FindFirstObjectByType<StealthUIManager>();
                playerController = FindFirstObjectByType<PlayerStealthController>();

                // Initialize Runtime State
                missionTimeRemaining = missionTimeLimit;
                totalObjectives = missionObjectives.Count;
                currentGameState = StealthGameState.NotStarted;

                LogDebug("[StealthGameplayManager] ✅ Gameplay manager initialized successfully");
            }
            catch (System.Exception ex)
            {
                LogError($"[StealthGameplayManager] ❌ Initialization failed: {ex.Message}");
            }
        }

        /// <summary>
        /// ミッション目標の設定
        /// </summary>
        private void SetupMissionObjectives()
        {
            objectiveMap.Clear();

            foreach (var objective in missionObjectives)
            {
                if (objective != null && !string.IsNullOrEmpty(objective.ObjectiveID))
                {
                    objectiveMap[objective.ObjectiveID] = objective;
                    objective.IsCompleted = false;
                }
            }

            LogDebug($"[StealthGameplayManager] Mission objectives configured: {objectiveMap.Count}");
        }

        #endregion

        #region Mission Generation

        /// <summary>
        /// デフォルトミッションの自動生成（15分完結体験）
        /// </summary>
        private void GenerateDefaultMission()
        {
            LogDebug("[StealthGameplayManager] Generating default 15-minute stealth mission...");

            missionObjectives.Clear();

            // Objective 1: Collect Key Items (5 minutes)
            if (keyItems != null && keyItems.Length > 0)
            {
                for (int i = 0; i < Mathf.Min(keyItems.Length, 3); i++)
                {
                    var collectObjective = new StealthMissionObjective
                    {
                        ObjectiveID = $"collect_key_{i}",
                        ObjectiveType = StealthObjectiveType.CollectItem,
                        Title = $"Key Item {i + 1} を取得",
                        Description = $"重要アイテム{i + 1}を発見せずに取得してください",
                        TargetObject = keyItems[i].gameObject,
                        IsOptional = false,
                        TimeLimit = 300f // 5 minutes
                    };
                    missionObjectives.Add(collectObjective);
                }
            }

            // Objective 2: Reach Target Location (7 minutes)
            if (targetLocations != null && targetLocations.Length > 0)
            {
                var infiltrateObjective = new StealthMissionObjective
                {
                    ObjectiveID = "infiltrate_target",
                    ObjectiveType = StealthObjectiveType.ReachLocation,
                    Title = "Target Area へ潜入",
                    Description = "警備を回避してターゲットエリアに到達してください",
                    TargetObject = targetLocations[0].gameObject,
                    IsOptional = false,
                    TimeLimit = 420f // 7 minutes additional
                };
                missionObjectives.Add(infiltrateObjective);
            }

            // Objective 3: Extract Successfully (3 minutes)
            if (missionExitPoint != null)
            {
                var extractObjective = new StealthMissionObjective
                {
                    ObjectiveID = "extract_mission",
                    ObjectiveType = StealthObjectiveType.Extraction,
                    Title = "Mission Exit",
                    Description = "ミッション完了後、脱出ポイントに到達してください",
                    TargetObject = missionExitPoint.gameObject,
                    IsOptional = false,
                    TimeLimit = 180f // 3 minutes additional
                };
                missionObjectives.Add(extractObjective);
            }

            // Bonus Objective: Remain Undetected
            var stealthBonusObjective = new StealthMissionObjective
            {
                ObjectiveID = "stealth_bonus",
                ObjectiveType = StealthObjectiveType.AvoidDetection,
                Title = "Perfect Stealth Bonus",
                Description = "一度も発見されることなくミッションを完了",
                IsOptional = true,
                BonusScore = 1000
            };
            missionObjectives.Add(stealthBonusObjective);

            SetupMissionObjectives();
            totalObjectives = missionObjectives.Count;

            LogDebug($"[StealthGameplayManager] ✅ Generated {missionObjectives.Count} objectives for 15-minute gameplay");
        }

        #endregion

        #region Game Flow Management

        /// <summary>
        /// ステルスゲームプレイの開始
        /// </summary>
        public void StartStealthGameplay()
        {
            if (currentGameState != StealthGameState.NotStarted)
            {
                LogDebug("[StealthGameplayManager] Game already started or completed");
                return;
            }

            LogDebug("[StealthGameplayManager] 🎮 Starting 15-minute stealth gameplay experience...");

            currentGameState = StealthGameState.InProgress;
            missionTimeRemaining = missionTimeLimit;
            objectivesCompleted = 0;

            // Start Game Systems
            if (enableTimeLimit)
            {
                gameTimerCoroutine = StartCoroutine(GameTimerCoroutine());
            }

            alertDecayCoroutine = StartCoroutine(AlertDecayCoroutine());

            // Initialize NPCs
            InitializeNPCPatrols();

            // Setup UI
            if (uiManager != null)
            {
                // TODO: Implement ShowMissionObjectives method in StealthUIManager
                // uiManager.ShowMissionObjectives(missionObjectives);
                // TODO: Implement UpdateTimeRemaining method in StealthUIManager
                // uiManager.UpdateTimeRemaining(missionTimeRemaining);
            }

            // Trigger Events
            onGameStarted?.Raise();
            stealthAudioService?.SetAlertLevelMusic(AlertLevel.Relaxed);

            LogDebug("[StealthGameplayManager] ✅ Stealth gameplay started - 15 minutes begins now!");
        }

        /// <summary>
        /// ゲーム進行の更新処理
        /// </summary>
        private void UpdateGameplaySystems()
        {
            // Check Objectives
            CheckObjectiveCompletion();

            // Update Global Alert Level
            UpdateGlobalAlertLevel();

            // Check Win/Lose Conditions
            CheckGameEndConditions();

            // Update UI
            if (uiManager != null)
            {
                // TODO: Implement UpdateTimeRemaining method in StealthUIManager
                // uiManager.UpdateTimeRemaining(missionTimeRemaining);
                // TODO: Implement UpdateObjectiveProgress method in StealthUIManager
                // uiManager.UpdateObjectiveProgress(objectivesCompleted, totalObjectives);
            }
        }

        /// <summary>
        /// ミッション目標の完了チェック
        /// </summary>
        private void CheckObjectiveCompletion()
        {
            foreach (var objective in missionObjectives)
            {
                if (!objective.IsCompleted && CheckObjectiveCondition(objective))
                {
                    CompleteObjective(objective);
                }
            }
        }

        /// <summary>
        /// 目標条件の具体的チェック
        /// </summary>
        private bool CheckObjectiveCondition(StealthMissionObjective objective)
        {
            if (playerController == null) return false;

            Vector3 playerPosition = playerController.transform.position;

            switch (objective.ObjectiveType)
            {
                case StealthObjectiveType.CollectItem:
                    return CheckItemCollection(objective, playerPosition);

                case StealthObjectiveType.ReachLocation:
                    return CheckLocationReached(objective, playerPosition);

                case StealthObjectiveType.Extraction:
                    return CheckExtractionCondition(objective, playerPosition);

                case StealthObjectiveType.AvoidDetection:
                    return CheckStealthMaintained(objective);

                default:
                    return false;
            }
        }

        /// <summary>
        /// アイテム収集条件のチェック
        /// </summary>
        private bool CheckItemCollection(StealthMissionObjective objective, Vector3 playerPosition)
        {
            if (objective.TargetObject == null) return false;

            float distance = Vector3.Distance(playerPosition, objective.TargetObject.transform.position);

            if (distance <= 2.0f) // Collection range
            {
                // Hide the collected item
                objective.TargetObject.SetActive(false);
                collectedItems.Add(objective.TargetObject);

                LogDebug($"[StealthGameplayManager] Item collected: {objective.Title}");
                return true;
            }

            return false;
        }

        /// <summary>
        /// 到達地点条件のチェック
        /// </summary>
        private bool CheckLocationReached(StealthMissionObjective objective, Vector3 playerPosition)
        {
            if (objective.TargetObject == null) return false;

            float distance = Vector3.Distance(playerPosition, objective.TargetObject.transform.position);
            return distance <= 3.0f; // Reach range
        }

        /// <summary>
        /// 脱出条件のチェック
        /// </summary>
        private bool CheckExtractionCondition(StealthMissionObjective objective, Vector3 playerPosition)
        {
            // Must complete all required objectives first
            int requiredCompleted = 0;
            foreach (var obj in missionObjectives)
            {
                if (!obj.IsOptional && obj.IsCompleted)
                    requiredCompleted++;
            }

            int totalRequired = 0;
            foreach (var obj in missionObjectives)
            {
                if (!obj.IsOptional && obj.ObjectiveType != StealthObjectiveType.Extraction)
                    totalRequired++;
            }

            if (requiredCompleted < totalRequired) return false;

            // Check extraction point proximity
            return CheckLocationReached(objective, playerPosition);
        }

        /// <summary>
        /// ステルス維持条件のチェック
        /// </summary>
        private bool CheckStealthMaintained(StealthMissionObjective objective)
        {
            // Check if player was never fully detected
            return globalAlertLevel < GlobalAlertLevel.FullAlert;
        }

        /// <summary>
        /// 目標完了処理
        /// </summary>
        private void CompleteObjective(StealthMissionObjective objective)
        {
            objective.IsCompleted = true;
            objective.CompletionTime = Time.time;
            objectivesCompleted++;

            LogDebug($"[StealthGameplayManager] ✅ Objective completed: {objective.Title}");

            // UI Update
            if (uiManager != null)
            {
                uiManager.ShowObjectiveCompleted(objective.Title);
            }

            // Audio Feedback
            stealthAudioService?.PlayObjectiveCompleteSound(objective.BonusScore > 0);

            // Events
            onObjectiveCompleted?.Raise();

            // Bonus Score
            if (objective.BonusScore > 0)
            {
                LogDebug($"[StealthGameplayManager] Bonus score earned: {objective.BonusScore}");
            }
        }

        #endregion

        #region NPC Management

        /// <summary>
        /// NPCパトロールシステムの初期化
        /// </summary>
        private void InitializeNPCPatrols()
        {
            LogDebug("[StealthGameplayManager] Initializing NPC patrol systems...");

            // Auto-find NPCs if not manually assigned
            if (patrolNPCs.Count == 0)
            {
                var foundNPCs = FindObjectsByType<NPCPatrolController>(FindObjectsSortMode.None);
                patrolNPCs.AddRange(foundNPCs);
            }

            foreach (var npc in patrolNPCs)
            {
                if (npc != null)
                {
                    npc.StartPatrol();
                    npc.SetDifficultyMultipliers(detectionSensitivityMultiplier, npcReactionTimeMultiplier);
                }
            }

            LogDebug($"[StealthGameplayManager] ✅ {patrolNPCs.Count} NPCs initialized for patrol");
        }

        /// <summary>
        /// グローバル警戒レベルの更新
        /// </summary>
        private void UpdateGlobalAlertLevel()
        {
            GlobalAlertLevel newAlertLevel = CalculateGlobalAlertLevel();

            if (newAlertLevel != globalAlertLevel)
            {
                GlobalAlertLevel previousLevel = globalAlertLevel;
                globalAlertLevel = newAlertLevel;

                LogDebug($"[StealthGameplayManager] Global alert level changed: {previousLevel} → {newAlertLevel}");

                // Update Audio
                AlertLevel audioAlertLevel = ConvertToAudioAlertLevel(globalAlertLevel);
                stealthAudioService?.SetAlertLevelMusic(audioAlertLevel);

                // Trigger Events
                onAlertLevelChanged?.Raise();

                // Update UI
                if (uiManager != null)
                {
                    uiManager.UpdateGlobalAlertLevel(ConvertGlobalToAIAlertLevel(globalAlertLevel));
                }
            }
        }

        /// <summary>
        /// 全NPCの状態からグローバル警戒レベルを計算
        /// </summary>
        private GlobalAlertLevel CalculateGlobalAlertLevel()
        {
            int alertNPCs = 0;
            int highAlertNPCs = 0;
            int totalNPCs = patrolNPCs.Count;

            foreach (var npc in patrolNPCs)
            {
                if (npc != null)
                {
                    var npcAlertLevel = npc.GetCurrentAlertLevel();

                    if (npcAlertLevel >= AlertLevel.Investigating)
                        alertNPCs++;

                    if (npcAlertLevel >= AlertLevel.Alert)
                        highAlertNPCs++;
                }
            }

            if (totalNPCs == 0) return GlobalAlertLevel.Normal;

            float alertRatio = (float)alertNPCs / totalNPCs;
            float highAlertRatio = (float)highAlertNPCs / totalNPCs;

            if (highAlertRatio >= 0.5f) return GlobalAlertLevel.FullAlert;
            if (alertRatio >= 0.3f) return GlobalAlertLevel.Heightened;
            if (alertNPCs > 0) return GlobalAlertLevel.Suspicious;

            return GlobalAlertLevel.Normal;
        }

        /// <summary>
        /// 警戒レベル減衰処理
        /// </summary>
        private IEnumerator AlertDecayCoroutine()
        {
            while (currentGameState == StealthGameState.InProgress)
            {
                yield return new WaitForSeconds(globalAlertLevelDecayTime);

                // Gradually reduce NPC alert levels
                foreach (var npc in patrolNPCs)
                {
                    if (npc != null)
                    {
                        npc.DecayAlertLevel(npcAlertDecayRate);
                    }
                }

                LogDebug("[StealthGameplayManager] Alert levels decayed naturally");
            }
        }

        #endregion

        #region Game Timer & End Conditions

        /// <summary>
        /// ゲームタイマーのコルーチン
        /// </summary>
        private IEnumerator GameTimerCoroutine()
        {
            while (missionTimeRemaining > 0 && currentGameState == StealthGameState.InProgress)
            {
                yield return new WaitForSeconds(1f);
                missionTimeRemaining--;

                // Warning notifications
                if (missionTimeRemaining == 300f) // 5 minutes remaining
                {
                    LogDebug("[StealthGameplayManager] ⏰ 5 minutes remaining!");
                    if (uiManager != null)
                        uiManager.ShowTimeWarning(missionTimeRemaining);
                }
                else if (missionTimeRemaining == 60f) // 1 minute remaining
                {
                    LogDebug("[StealthGameplayManager] ⚠️ 1 minute remaining!");
                    if (uiManager != null)
                        uiManager.ShowTimeWarning(missionTimeRemaining);
                }
            }

            // Time up
            if (currentGameState == StealthGameState.InProgress)
            {
                LogDebug("[StealthGameplayManager] ⏰ Time's up! Mission failed.");
                EndGame(false, "Time limit exceeded");
            }
        }

        /// <summary>
        /// ゲーム終了条件のチェック
        /// </summary>
        private void CheckGameEndConditions()
        {
            // Success Condition: All required objectives completed
            bool allRequiredCompleted = true;
            foreach (var objective in missionObjectives)
            {
                if (!objective.IsOptional && !objective.IsCompleted)
                {
                    allRequiredCompleted = false;
                    break;
                }
            }

            if (allRequiredCompleted)
            {
                string completionMessage = $"Mission accomplished in {FormatTime(missionTimeLimit - missionTimeRemaining)}!";
                EndGame(true, completionMessage);
                return;
            }

            // Failure Condition: Full Alert for too long
            if (globalAlertLevel == GlobalAlertLevel.FullAlert)
            {
                EndGame(false, "Mission compromised - full alert status");
                return;
            }
        }

        /// <summary>
        /// ゲーム終了処理
        /// </summary>
        public void EndGame(bool success, string reason)
        {
            if (currentGameState != StealthGameState.InProgress) return;

            LogDebug($"[StealthGameplayManager] Game ended - Success: {success}, Reason: {reason}");

            currentGameState = success ? StealthGameState.CompletedSuccess : StealthGameState.CompletedFailure;

            // Stop Coroutines
            if (gameTimerCoroutine != null)
            {
                StopCoroutine(gameTimerCoroutine);
                gameTimerCoroutine = null;
            }

            if (alertDecayCoroutine != null)
            {
                StopCoroutine(alertDecayCoroutine);
                alertDecayCoroutine = null;
            }

            // Calculate Final Score
            int finalScore = CalculateFinalScore();

            // UI Updates
            if (uiManager != null)
            {
                string endMessage = $"{reason} - Score: {finalScore}, Objectives: {objectivesCompleted}/{totalObjectives}";
                uiManager.ShowGameEndScreen(success, endMessage);
            }

            // Events
            if (success)
            {
                onGameCompleted?.Raise();
            }
            else
            {
                onGameFailed?.Raise();
            }

            LogDebug($"[StealthGameplayManager] 🎯 15-minute stealth gameplay completed! Score: {finalScore}");
        }

        /// <summary>
        /// 最終スコアの計算
        /// </summary>
        private int CalculateFinalScore()
        {
            int baseScore = objectivesCompleted * 100;
            int timeBonus = (int)(missionTimeRemaining * 2); // 2 points per remaining second
            int stealthBonus = globalAlertLevel == GlobalAlertLevel.Normal ? 500 : 0;
            int bonusScore = 0;

            foreach (var objective in missionObjectives)
            {
                if (objective.IsCompleted)
                    bonusScore += objective.BonusScore;
            }

            return baseScore + timeBonus + stealthBonus + bonusScore;
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// 時間のフォーマット（mm:ss）
        /// </summary>
        private string FormatTime(float timeInSeconds)
        {
            int minutes = (int)(timeInSeconds / 60);
            int seconds = (int)(timeInSeconds % 60);
            return $"{minutes:D2}:{seconds:D2}";
        }

        /// <summary>
        /// グローバル警戒レベルを音響システム用に変換
        /// </summary>
        private AlertLevel ConvertToAudioAlertLevel(GlobalAlertLevel globalLevel)
        {
            return globalLevel switch
            {
                GlobalAlertLevel.Normal => AlertLevel.Relaxed,
                GlobalAlertLevel.Suspicious => AlertLevel.Suspicious,
                GlobalAlertLevel.Heightened => AlertLevel.Investigating,
                GlobalAlertLevel.FullAlert => AlertLevel.Alert,
                _ => AlertLevel.Relaxed
            };
        }

        /// <summary>
        /// デバッグログ出力
        /// </summary>
        private void LogDebug(string message)
        {
            Debug.Log(message);
        }

        /// <summary>
        /// エラーログ出力
        /// </summary>
        private void LogError(string message)
        {
            Debug.LogError(message);
        }

        #endregion

        #region Public API

        /// <summary>
        /// 現在のゲーム状態を取得
        /// </summary>
        public StealthGameState GetCurrentGameState() => currentGameState;

        /// <summary>
        /// 残り時間を取得
        /// </summary>
        public float GetTimeRemaining() => missionTimeRemaining;

        /// <summary>
        /// 完了した目標数を取得
        /// </summary>
        public int GetCompletedObjectives() => objectivesCompleted;

        /// <summary>
        /// 全目標数を取得
        /// </summary>
        public int GetTotalObjectives() => totalObjectives;

        /// <summary>
        /// 現在のグローバル警戒レベルを取得
        /// </summary>
        public GlobalAlertLevel GetGlobalAlertLevel() => globalAlertLevel;

        /// <summary>
        /// 手動でゲーム開始
        /// </summary>
        [Button("Start Game")]
        public void ManualStartGame()
        {
            if (currentGameState == StealthGameState.NotStarted)
            {
                StartStealthGameplay();
            }
        }

        /// <summary>
        /// 手動でゲーム終了
        /// </summary>
        [Button("End Game")]
        public void ManualEndGame()
        {
            if (currentGameState == StealthGameState.InProgress)
            {
                EndGame(false, "Manual termination");
            }
        }

        /// <summary>
        /// ゲーム状態をリセット
        /// </summary>
        [Button("Reset Game")]
        public void ResetGame()
        {
            currentGameState = StealthGameState.NotStarted;
            missionTimeRemaining = missionTimeLimit;
            objectivesCompleted = 0;
            globalAlertLevel = GlobalAlertLevel.Normal;

            // Reset objectives
            foreach (var objective in missionObjectives)
            {
                objective.IsCompleted = false;
                objective.CompletionTime = 0f;
            }

            // Reset collected items
            foreach (var item in collectedItems)
            {
                if (item != null)
                    item.SetActive(true);
            }
            collectedItems.Clear();

            // Reset NPCs
            foreach (var npc in patrolNPCs)
            {
                if (npc != null)
                    npc.ResetToInitialState();
            }

            LogDebug("[StealthGameplayManager] Game state reset to initial conditions");
        }

        #endregion

        #region Type Conversion Helper Methods

        /// <summary>
        /// GlobalAlertLevelをAIAlertLevelに変換（設計書準拠）
        /// </summary>
        private AIAlertLevel ConvertGlobalToAIAlertLevel(GlobalAlertLevel globalLevel)
        {
            return globalLevel switch
            {
                GlobalAlertLevel.Normal => AIAlertLevel.Unaware,
                GlobalAlertLevel.Suspicious => AIAlertLevel.Suspicious,
                GlobalAlertLevel.Heightened => AIAlertLevel.Investigating,
                GlobalAlertLevel.FullAlert => AIAlertLevel.Alert,
                _ => AIAlertLevel.Unaware
            };
        }

        #endregion

        #region Editor Support

#if UNITY_EDITOR
        [TabGroup("Gameplay", "Editor Tools")]
        [Button("Generate Test Mission")]
        public void EditorGenerateTestMission()
        {
            GenerateDefaultMission();
        }

        [Button("Validate Setup")]
        public void EditorValidateSetup()
        {
            LogDebug("=== StealthGameplayManager Setup Validation ===");
            LogDebug($"Mission Time Limit: {missionTimeLimit}s ({FormatTime(missionTimeLimit)})");
            LogDebug($"Mission Objectives: {missionObjectives.Count}");
            LogDebug($"NPC Controllers: {patrolNPCs.Count}");
            LogDebug($"Key Items: {(keyItems?.Length ?? 0)}");
            LogDebug($"Target Locations: {(targetLocations?.Length ?? 0)}");
            LogDebug($"Exit Point: {(missionExitPoint != null ? "✅ Set" : "❌ Missing")}");
            LogDebug($"Stealth Audio Service: {(stealthAudioService != null ? "✅ Available" : "❌ Missing")}");
            LogDebug("=== Validation Complete ===");
        }
#endif

        #endregion
    }

    #region Supporting Data Structures

    /// <summary>
    /// ステルスゲーム状態の定義
    /// </summary>
    public enum StealthGameState
    {
        NotStarted,
        InProgress,
        CompletedSuccess,
        CompletedFailure
    }

    /// <summary>
    /// ステルス難易度レベル
    /// </summary>
    public enum StealthDifficulty
    {
        Easy,    // より緩い検知、長い反応時間
        Normal,  // 標準的なバランス
        Hard,    // 厳しい検知、短い反応時間
        Expert   // 非常に厳しい条件
    }

    /// <summary>
    /// グローバル警戒レベル
    /// </summary>
    public enum GlobalAlertLevel
    {
        Normal,      // 通常状態
        Suspicious,  // 一部NPCが警戒
        Heightened,  // 警戒レベルが上昇
        FullAlert    // 全面警戒状態
    }

    /// <summary>
    /// ステルスミッション目標の種類
    /// </summary>
    public enum StealthObjectiveType
    {
        CollectItem,     // アイテム収集
        ReachLocation,   // 地点到達
        AvoidDetection,  // 検知回避
        Extraction       // 脱出
    }

    /// <summary>
    /// ステルスミッション目標の定義
    /// </summary>
    [System.Serializable]
    public class StealthMissionObjective
    {
        [Header("Objective Definition")]
        public string ObjectiveID;
        public StealthObjectiveType ObjectiveType;
        public string Title;
        [TextArea(2, 4)]
        public string Description;

        [Header("Objective Configuration")]
        public GameObject TargetObject;
        public bool IsOptional = false;
        public float TimeLimit = 0f; // 0 = no time limit
        public int BonusScore = 0;

        [Header("Runtime Status")]
        [ReadOnly] public bool IsCompleted = false;
        [ReadOnly] public float CompletionTime = 0f;
    }

    #endregion
}

// Supporting classes for NPC management (these would typically be in separate files)
namespace asterivo.Unity60.Features.Templates.Stealth
{
    /// <summary>
    /// NPCパトロールコントローラー（StealthGameplayManagerで使用）
    /// </summary>
    public class NPCPatrolController : MonoBehaviour
    {
        [Header("Patrol Configuration")]
        [SerializeField] private Transform[] patrolPoints;
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private float waitTime = 2f;
        [SerializeField] private AlertLevel currentAlertLevel = AlertLevel.Relaxed;

        private NavMeshAgent navAgent;
        private int currentPatrolIndex = 0;
        private bool isPatrolling = false;
        private Vector3 initialPosition;
        private Quaternion initialRotation;

        private void Awake()
        {
            navAgent = GetComponent<NavMeshAgent>();
            initialPosition = transform.position;
            initialRotation = transform.rotation;
        }

        public void StartPatrol()
        {
            if (patrolPoints == null || patrolPoints.Length == 0) return;

            isPatrolling = true;
            navAgent.speed = moveSpeed;
            MoveToNextPatrolPoint();
        }

        public void SetDifficultyMultipliers(float detectionMultiplier, float reactionMultiplier)
        {
            // Apply difficulty settings to this NPC
        }

        public AlertLevel GetCurrentAlertLevel() => currentAlertLevel;

        public void DecayAlertLevel(float decayRate)
        {
            // Implement alert level decay logic
        }

        public void ResetToInitialState()
        {
            transform.position = initialPosition;
            transform.rotation = initialRotation;
            currentAlertLevel = AlertLevel.Relaxed;
            currentPatrolIndex = 0;
            isPatrolling = false;
        }

        private void MoveToNextPatrolPoint()
        {
            if (patrolPoints == null || patrolPoints.Length == 0) return;

            navAgent.SetDestination(patrolPoints[currentPatrolIndex].position);
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        }
    }
}
