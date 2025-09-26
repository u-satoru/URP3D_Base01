using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Commands;
using asterivo.Unity60.Core.Audio.Interfaces;
using asterivo.Unity60.Core;
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
    /// 繧ｹ繝・Ν繧ｹ繧ｲ繝ｼ繝繝励Ξ繧､繝槭ロ繝ｼ繧ｸ繝｣繝ｼ - 15蛻・ｮ檎ｵ舌せ繝・Ν繧ｹ繧ｲ繝ｼ繝菴馴ｨ薙・螳溽樟
    /// 譛蜆ｪ蜈亥ｺｦ繧ｸ繝｣繝ｳ繝ｫ縺ｨ縺励※縺ｮ繧ｹ繝・Ν繧ｹ繧｢繧ｯ繧ｷ繝ｧ繝ｳ螳悟・螳溯｣・
    /// Mission邉ｻ邨ｱ縲¨PC邂｡逅・∵､懃衍繧ｷ繧ｹ繝・Β縲∵・蜉・螟ｱ謨玲擅莉ｶ繧堤ｵｱ蜷育ｮ｡逅・
    /// </summary>
    public class StealthGameplayManager : MonoBehaviour
    {
        #region Gameplay Configuration

        [TabGroup("Gameplay", "Mission Settings")]
        [Title("15-Minute Stealth Gameplay Configuration", "螳檎ｵ仙梛繧ｹ繝・Ν繧ｹ繧｢繧ｯ繧ｷ繝ｧ繝ｳ菴馴ｨ・, TitleAlignments.Centered)]

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
        /// 繧ｲ繝ｼ繝繝励Ξ繧､繝槭ロ繝ｼ繧ｸ繝｣繝ｼ縺ｮ蛻晄悄蛹・
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

                LogDebug("[StealthGameplayManager] 笨・Gameplay manager initialized successfully");
            }
            catch (System.Exception ex)
            {
                LogError($"[StealthGameplayManager] 笶・Initialization failed: {ex.Message}");
            }
        }

        /// <summary>
        /// 繝溘ャ繧ｷ繝ｧ繝ｳ逶ｮ讓吶・險ｭ螳・
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
        /// 繝・ヵ繧ｩ繝ｫ繝医Α繝・す繝ｧ繝ｳ縺ｮ閾ｪ蜍慕函謌撰ｼ・5蛻・ｮ檎ｵ蝉ｽ馴ｨ難ｼ・
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
                        Title = $"Key Item {i + 1} 繧貞叙蠕・,
                        Description = $"驥崎ｦ√い繧､繝・Β{i + 1}繧堤匱隕九○縺壹↓蜿門ｾ励＠縺ｦ縺上□縺輔＞",
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
                    Title = "Target Area 縺ｸ貎懷・",
                    Description = "隴ｦ蛯吶ｒ蝗樣∩縺励※繧ｿ繝ｼ繧ｲ繝・ヨ繧ｨ繝ｪ繧｢縺ｫ蛻ｰ驕斐＠縺ｦ縺上□縺輔＞",
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
                    Description = "繝溘ャ繧ｷ繝ｧ繝ｳ螳御ｺ・ｾ後∬┳蜃ｺ繝昴う繝ｳ繝医↓蛻ｰ驕斐＠縺ｦ縺上□縺輔＞",
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
                Description = "荳蠎ｦ繧ら匱隕九＆繧後ｋ縺薙→縺ｪ縺上Α繝・す繝ｧ繝ｳ繧貞ｮ御ｺ・,
                IsOptional = true,
                BonusScore = 1000
            };
            missionObjectives.Add(stealthBonusObjective);

            SetupMissionObjectives();
            totalObjectives = missionObjectives.Count;

            LogDebug($"[StealthGameplayManager] 笨・Generated {missionObjectives.Count} objectives for 15-minute gameplay");
        }

        #endregion

        #region Game Flow Management

        /// <summary>
        /// 繧ｹ繝・Ν繧ｹ繧ｲ繝ｼ繝繝励Ξ繧､縺ｮ髢句ｧ・
        /// </summary>
        public void StartStealthGameplay()
        {
            if (currentGameState != StealthGameState.NotStarted)
            {
                LogDebug("[StealthGameplayManager] Game already started or completed");
                return;
            }

            LogDebug("[StealthGameplayManager] 式 Starting 15-minute stealth gameplay experience...");

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

            LogDebug("[StealthGameplayManager] 笨・Stealth gameplay started - 15 minutes begins now!");
        }

        /// <summary>
        /// 繧ｲ繝ｼ繝騾ｲ陦後・譖ｴ譁ｰ蜃ｦ逅・
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
        /// 繝溘ャ繧ｷ繝ｧ繝ｳ逶ｮ讓吶・螳御ｺ・メ繧ｧ繝・け
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
        /// 逶ｮ讓呎擅莉ｶ縺ｮ蜈ｷ菴鍋噪繝√ぉ繝・け
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
        /// 繧｢繧､繝・Β蜿朱寔譚｡莉ｶ縺ｮ繝√ぉ繝・け
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
        /// 蛻ｰ驕泌慍轤ｹ譚｡莉ｶ縺ｮ繝√ぉ繝・け
        /// </summary>
        private bool CheckLocationReached(StealthMissionObjective objective, Vector3 playerPosition)
        {
            if (objective.TargetObject == null) return false;

            float distance = Vector3.Distance(playerPosition, objective.TargetObject.transform.position);
            return distance <= 3.0f; // Reach range
        }

        /// <summary>
        /// 閼ｱ蜃ｺ譚｡莉ｶ縺ｮ繝√ぉ繝・け
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
        /// 繧ｹ繝・Ν繧ｹ邯ｭ謖∵擅莉ｶ縺ｮ繝√ぉ繝・け
        /// </summary>
        private bool CheckStealthMaintained(StealthMissionObjective objective)
        {
            // Check if player was never fully detected
            return globalAlertLevel < GlobalAlertLevel.FullAlert;
        }

        /// <summary>
        /// 逶ｮ讓吝ｮ御ｺ・・逅・
        /// </summary>
        private void CompleteObjective(StealthMissionObjective objective)
        {
            objective.IsCompleted = true;
            objective.CompletionTime = Time.time;
            objectivesCompleted++;

            LogDebug($"[StealthGameplayManager] 笨・Objective completed: {objective.Title}");

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
        /// NPC繝代ヨ繝ｭ繝ｼ繝ｫ繧ｷ繧ｹ繝・Β縺ｮ蛻晄悄蛹・
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

            LogDebug($"[StealthGameplayManager] 笨・{patrolNPCs.Count} NPCs initialized for patrol");
        }

        /// <summary>
        /// 繧ｰ繝ｭ繝ｼ繝舌Ν隴ｦ謌偵Ξ繝吶Ν縺ｮ譖ｴ譁ｰ
        /// </summary>
        private void UpdateGlobalAlertLevel()
        {
            GlobalAlertLevel newAlertLevel = CalculateGlobalAlertLevel();

            if (newAlertLevel != globalAlertLevel)
            {
                GlobalAlertLevel previousLevel = globalAlertLevel;
                globalAlertLevel = newAlertLevel;

                LogDebug($"[StealthGameplayManager] Global alert level changed: {previousLevel} 竊・{newAlertLevel}");

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
        /// 蜈ｨNPC縺ｮ迥ｶ諷九°繧峨げ繝ｭ繝ｼ繝舌Ν隴ｦ謌偵Ξ繝吶Ν繧定ｨ育ｮ・
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
        /// 隴ｦ謌偵Ξ繝吶Ν貂幄｡ｰ蜃ｦ逅・
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
        /// 繧ｲ繝ｼ繝繧ｿ繧､繝槭・縺ｮ繧ｳ繝ｫ繝ｼ繝√Φ
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
                    LogDebug("[StealthGameplayManager] 竢ｰ 5 minutes remaining!");
                    if (uiManager != null)
                        uiManager.ShowTimeWarning(missionTimeRemaining);
                }
                else if (missionTimeRemaining == 60f) // 1 minute remaining
                {
                    LogDebug("[StealthGameplayManager] 笞・・1 minute remaining!");
                    if (uiManager != null)
                        uiManager.ShowTimeWarning(missionTimeRemaining);
                }
            }

            // Time up
            if (currentGameState == StealthGameState.InProgress)
            {
                LogDebug("[StealthGameplayManager] 竢ｰ Time's up! Mission failed.");
                EndGame(false, "Time limit exceeded");
            }
        }

        /// <summary>
        /// 繧ｲ繝ｼ繝邨ゆｺ・擅莉ｶ縺ｮ繝√ぉ繝・け
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
        /// 繧ｲ繝ｼ繝邨ゆｺ・・逅・
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

            LogDebug($"[StealthGameplayManager] 識 15-minute stealth gameplay completed! Score: {finalScore}");
        }

        /// <summary>
        /// 譛邨ゅせ繧ｳ繧｢縺ｮ險育ｮ・
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
        /// 譎る俣縺ｮ繝輔か繝ｼ繝槭ャ繝茨ｼ・m:ss・・
        /// </summary>
        private string FormatTime(float timeInSeconds)
        {
            int minutes = (int)(timeInSeconds / 60);
            int seconds = (int)(timeInSeconds % 60);
            return $"{minutes:D2}:{seconds:D2}";
        }

        /// <summary>
        /// 繧ｰ繝ｭ繝ｼ繝舌Ν隴ｦ謌偵Ξ繝吶Ν繧帝浹髻ｿ繧ｷ繧ｹ繝・Β逕ｨ縺ｫ螟画鋤
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
        /// 繝・ヰ繝・げ繝ｭ繧ｰ蜃ｺ蜉・
        /// </summary>
        private void LogDebug(string message)
        {
            Debug.Log(message);
        }

        /// <summary>
        /// 繧ｨ繝ｩ繝ｼ繝ｭ繧ｰ蜃ｺ蜉・
        /// </summary>
        private void LogError(string message)
        {
            Debug.LogError(message);
        }

        #endregion

        #region Public API

        /// <summary>
        /// 迴ｾ蝨ｨ縺ｮ繧ｲ繝ｼ繝迥ｶ諷九ｒ蜿門ｾ・
        /// </summary>
        public StealthGameState GetCurrentGameState() => currentGameState;

        /// <summary>
        /// 谿九ｊ譎る俣繧貞叙蠕・
        /// </summary>
        public float GetTimeRemaining() => missionTimeRemaining;

        /// <summary>
        /// 螳御ｺ・＠縺溽岼讓呎焚繧貞叙蠕・
        /// </summary>
        public int GetCompletedObjectives() => objectivesCompleted;

        /// <summary>
        /// 蜈ｨ逶ｮ讓呎焚繧貞叙蠕・
        /// </summary>
        public int GetTotalObjectives() => totalObjectives;

        /// <summary>
        /// 迴ｾ蝨ｨ縺ｮ繧ｰ繝ｭ繝ｼ繝舌Ν隴ｦ謌偵Ξ繝吶Ν繧貞叙蠕・
        /// </summary>
        public GlobalAlertLevel GetGlobalAlertLevel() => globalAlertLevel;

        /// <summary>
        /// 謇句虚縺ｧ繧ｲ繝ｼ繝髢句ｧ・
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
        /// 謇句虚縺ｧ繧ｲ繝ｼ繝邨ゆｺ・
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
        /// 繧ｲ繝ｼ繝迥ｶ諷九ｒ繝ｪ繧ｻ繝・ヨ
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
        /// GlobalAlertLevel繧但IAlertLevel縺ｫ螟画鋤・郁ｨｭ險域嶌貅匁侠・・
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
            LogDebug($"Exit Point: {(missionExitPoint != null ? "笨・Set" : "笶・Missing")}");
            LogDebug($"Stealth Audio Service: {(stealthAudioService != null ? "笨・Available" : "笶・Missing")}");
            LogDebug("=== Validation Complete ===");
        }
#endif

        #endregion
    }

    #region Supporting Data Structures

    /// <summary>
    /// 繧ｹ繝・Ν繧ｹ繧ｲ繝ｼ繝迥ｶ諷九・螳夂ｾｩ
    /// </summary>
    public enum StealthGameState
    {
        NotStarted,
        InProgress,
        CompletedSuccess,
        CompletedFailure
    }

    /// <summary>
    /// 繧ｹ繝・Ν繧ｹ髮｣譏灘ｺｦ繝ｬ繝吶Ν
    /// </summary>
    public enum StealthDifficulty
    {
        Easy,    // 繧医ｊ邱ｩ縺・､懃衍縲・聞縺・渚蠢懈凾髢・
        Normal,  // 讓呎ｺ也噪縺ｪ繝舌Λ繝ｳ繧ｹ
        Hard,    // 蜴ｳ縺励＞讀懃衍縲∫洒縺・渚蠢懈凾髢・
        Expert   // 髱槫ｸｸ縺ｫ蜴ｳ縺励＞譚｡莉ｶ
    }

    /// <summary>
    /// 繧ｰ繝ｭ繝ｼ繝舌Ν隴ｦ謌偵Ξ繝吶Ν
    /// </summary>
    public enum GlobalAlertLevel
    {
        Normal,      // 騾壼ｸｸ迥ｶ諷・
        Suspicious,  // 荳驛ｨNPC縺瑚ｭｦ謌・
        Heightened,  // 隴ｦ謌偵Ξ繝吶Ν縺御ｸ頑・
        FullAlert    // 蜈ｨ髱｢隴ｦ謌堤憾諷・
    }

    /// <summary>
    /// 繧ｹ繝・Ν繧ｹ繝溘ャ繧ｷ繝ｧ繝ｳ逶ｮ讓吶・遞ｮ鬘・
    /// </summary>
    public enum StealthObjectiveType
    {
        CollectItem,     // 繧｢繧､繝・Β蜿朱寔
        ReachLocation,   // 蝨ｰ轤ｹ蛻ｰ驕・
        AvoidDetection,  // 讀懃衍蝗樣∩
        Extraction       // 閼ｱ蜃ｺ
    }

    /// <summary>
    /// 繧ｹ繝・Ν繧ｹ繝溘ャ繧ｷ繝ｧ繝ｳ逶ｮ讓吶・螳夂ｾｩ
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
    /// NPC繝代ヨ繝ｭ繝ｼ繝ｫ繧ｳ繝ｳ繝医Ο繝ｼ繝ｩ繝ｼ・・tealthGameplayManager縺ｧ菴ｿ逕ｨ・・
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


