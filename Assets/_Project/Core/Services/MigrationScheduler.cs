using UnityEngine;
using System;
using System.Collections.Generic;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Debug;

namespace asterivo.Unity60.Core.Services
{
    /// <summary>
    /// Step 3.7: 段階的機能有効化スケジュール管理システム
    /// Day 1-2, Day 3-4, Day 5の段階的なFeatureFlags有効化を管理
    /// </summary>
    public class MigrationScheduler : MonoBehaviour
    {
        [Header("Schedule Configuration")]
        [SerializeField] private bool enableAutomaticSchedule = false;
        [SerializeField] private bool enableDebugLogging = true;
        [SerializeField] private int currentPhaseOverride = -1; // -1 = Auto, 0-4 = Manual override

        [Header("Schedule Timing (Days)")]
        [SerializeField] private float day1Duration = 2f; // Day 1-2: 2 days
        [SerializeField] private float day3Duration = 1f; // Day 3: 1 day  
        [SerializeField] private float day4Duration = 1f; // Day 4: 1 day
        [SerializeField] private float day5Duration = 1f; // Day 5: 1 day

        [Header("Current Status")]
        [SerializeField] private MigrationPhase currentPhase;
        [SerializeField] private DateTime scheduleStartTime;
        [SerializeField] private float timeInCurrentPhase;
        [SerializeField] private bool scheduleCompleted;

        private FeatureFlagScheduler flagScheduler;
        private MigrationProgressTracker progressTracker;

        /// <summary>
        /// 段階的移行のフェーズ定義
        /// </summary>
        public enum MigrationPhase
        {
            NotStarted = 0,
            Day1_2_Staging = 1,      // ステージング環境テスト
            Day3_SpatialEnabled = 2,  // SpatialService有効化
            Day4_StealthEnabled = 3,  // StealthService有効化  
            Day5_Validation = 4,      // 全検証と安定化
            Completed = 5             // スケジュール完了
        }

        /// <summary>
        /// 各段階の設定定義
        /// </summary>
        [System.Serializable]
        public struct PhaseConfiguration
        {
            public MigrationPhase phase;
            public string phaseName;
            public bool useNewAudioService;
            public bool useNewSpatialService;
            public bool useNewStealthService;
            public bool disableLegacySingletons;
            public bool enablePerformanceMonitoring;
            public string description;
        }

        private readonly PhaseConfiguration[] phaseConfigurations = new PhaseConfiguration[]
        {
            new PhaseConfiguration
            {
                phase = MigrationPhase.Day1_2_Staging,
                phaseName = "Day 1-2: Staging Test",
                useNewAudioService = true,
                useNewSpatialService = false,
                useNewStealthService = false,
                disableLegacySingletons = false,
                enablePerformanceMonitoring = true,
                description = "ステージング環境でのAudioServiceテスト"
            },
            new PhaseConfiguration
            {
                phase = MigrationPhase.Day3_SpatialEnabled,
                phaseName = "Day 3: SpatialService Enabled",
                useNewAudioService = true,
                useNewSpatialService = true,
                useNewStealthService = false,
                disableLegacySingletons = false,
                enablePerformanceMonitoring = true,
                description = "SpatialAudioServiceの段階的有効化"
            },
            new PhaseConfiguration
            {
                phase = MigrationPhase.Day4_StealthEnabled,
                phaseName = "Day 4: StealthService Enabled",
                useNewAudioService = true,
                useNewSpatialService = true,
                useNewStealthService = true,
                disableLegacySingletons = false,
                enablePerformanceMonitoring = true,
                description = "StealthAudioServiceの段階的有効化"
            },
            new PhaseConfiguration
            {
                phase = MigrationPhase.Day5_Validation,
                phaseName = "Day 5: Full Validation",
                useNewAudioService = true,
                useNewSpatialService = true,
                useNewStealthService = true,
                disableLegacySingletons = false, // Week 4で対応予定
                enablePerformanceMonitoring = true,
                description = "全サービス有効化での検証と安定化"
            }
        };

        #region Unity Lifecycle

        private void Awake()
        {
            // 依存コンポーネントの初期化
            flagScheduler = GetComponent<FeatureFlagScheduler>() ?? gameObject.AddComponent<FeatureFlagScheduler>();
            progressTracker = GetComponent<MigrationProgressTracker>() ?? gameObject.AddComponent<MigrationProgressTracker>();
            
            // 初期状態の設定
            if (currentPhase == MigrationPhase.NotStarted)
            {
                currentPhase = MigrationPhase.Day1_2_Staging;
                scheduleStartTime = DateTime.Now;
            }
        }

        private void Start()
        {
            LogScheduleInfo("MigrationScheduler initialized");
            
            if (enableAutomaticSchedule)
            {
                StartSchedule();
            }
            else
            {
                LogScheduleInfo("Automatic schedule disabled. Use manual controls.");
            }
        }

        private void Update()
        {
            if (enableAutomaticSchedule && !scheduleCompleted)
            {
                UpdateAutomaticSchedule();
            }
            
            UpdateCurrentPhaseTime();
        }

        #endregion

        #region Schedule Management

        /// <summary>
        /// スケジュールの開始
        /// </summary>
        [ContextMenu("Start Schedule")]
        public void StartSchedule()
        {
            LogScheduleInfo("Starting migration schedule");
            
            scheduleStartTime = DateTime.Now;
            currentPhase = MigrationPhase.Day1_2_Staging;
            scheduleCompleted = false;
            timeInCurrentPhase = 0f;
            
            ApplyCurrentPhaseConfiguration();
            progressTracker.RecordPhaseStart(currentPhase);
        }

        /// <summary>
        /// 次のフェーズに進む
        /// </summary>
        [ContextMenu("Advance to Next Phase")]
        public void AdvanceToNextPhase()
        {
            if (scheduleCompleted)
            {
                LogScheduleInfo("Schedule already completed");
                return;
            }

            var previousPhase = currentPhase;
            
            // 次のフェーズに進む
            switch (currentPhase)
            {
                case MigrationPhase.Day1_2_Staging:
                    currentPhase = MigrationPhase.Day3_SpatialEnabled;
                    break;
                case MigrationPhase.Day3_SpatialEnabled:
                    currentPhase = MigrationPhase.Day4_StealthEnabled;
                    break;
                case MigrationPhase.Day4_StealthEnabled:
                    currentPhase = MigrationPhase.Day5_Validation;
                    break;
                case MigrationPhase.Day5_Validation:
                    currentPhase = MigrationPhase.Completed;
                    scheduleCompleted = true;
                    break;
                default:
                    LogScheduleInfo("Cannot advance further");
                    return;
            }
            
            timeInCurrentPhase = 0f;
            
            LogScheduleInfo($"Advanced from {previousPhase} to {currentPhase}");
            
            ApplyCurrentPhaseConfiguration();
            progressTracker.RecordPhaseTransition(previousPhase, currentPhase);
            
            if (scheduleCompleted)
            {
                progressTracker.RecordScheduleCompletion();
                LogScheduleInfo("🎉 Migration schedule completed successfully!");
            }
        }

        /// <summary>
        /// 手動でフェーズを設定
        /// </summary>
        /// <param name="targetPhase">設定するフェーズ</param>
        [ContextMenu("Set Phase Manually")]
        public void SetPhaseManually(MigrationPhase targetPhase)
        {
            if (targetPhase == MigrationPhase.NotStarted || targetPhase == MigrationPhase.Completed)
            {
                LogScheduleInfo($"Cannot manually set phase to {targetPhase}");
                return;
            }
            
            var previousPhase = currentPhase;
            currentPhase = targetPhase;
            timeInCurrentPhase = 0f;
            
            LogScheduleInfo($"Manually set phase from {previousPhase} to {currentPhase}");
            
            ApplyCurrentPhaseConfiguration();
            progressTracker.RecordPhaseTransition(previousPhase, currentPhase);
        }

        #endregion

        #region Configuration Application

        /// <summary>
        /// 現在のフェーズ設定をFeatureFlagsに適用
        /// </summary>
        private void ApplyCurrentPhaseConfiguration()
        {
            var config = GetCurrentPhaseConfiguration();
            if (config.HasValue)
            {
                flagScheduler.ApplyPhaseConfiguration(config.Value);
                LogScheduleInfo($"Applied configuration for {config.Value.phaseName}: {config.Value.description}");
            }
            else
            {
                LogScheduleInfo($"No configuration found for phase {currentPhase}");
            }
        }

        /// <summary>
        /// 現在のフェーズ設定を取得
        /// </summary>
        /// <returns>フェーズ設定</returns>
        private PhaseConfiguration? GetCurrentPhaseConfiguration()
        {
            // Manual override check
            if (currentPhaseOverride >= 0 && currentPhaseOverride < phaseConfigurations.Length)
            {
                return phaseConfigurations[currentPhaseOverride];
            }
            
            foreach (var config in phaseConfigurations)
            {
                if (config.phase == currentPhase)
                {
                    return config;
                }
            }
            
            return null;
        }

        #endregion

        #region Automatic Schedule Update

        /// <summary>
        /// 自動スケジュールの更新
        /// </summary>
        private void UpdateAutomaticSchedule()
        {
            float phaseDuration = GetCurrentPhaseDuration();
            
            if (timeInCurrentPhase >= phaseDuration)
            {
                AdvanceToNextPhase();
            }
        }

        /// <summary>
        /// 現在のフェーズの継続時間を取得
        /// </summary>
        /// <returns>継続時間（秒）</returns>
        private float GetCurrentPhaseDuration()
        {
            switch (currentPhase)
            {
                case MigrationPhase.Day1_2_Staging:
                    return day1Duration * 24f * 60f * 60f; // days to seconds
                case MigrationPhase.Day3_SpatialEnabled:
                    return day3Duration * 24f * 60f * 60f;
                case MigrationPhase.Day4_StealthEnabled:
                    return day4Duration * 24f * 60f * 60f;
                case MigrationPhase.Day5_Validation:
                    return day5Duration * 24f * 60f * 60f;
                default:
                    return float.MaxValue;
            }
        }

        /// <summary>
        /// 現在のフェーズ経過時間の更新
        /// </summary>
        private void UpdateCurrentPhaseTime()
        {
            if (!scheduleCompleted)
            {
                timeInCurrentPhase += Time.deltaTime;
            }
        }

        #endregion

        #region Status and Information

        /// <summary>
        /// 現在の進行状況情報を取得
        /// </summary>
        /// <returns>進行状況情報</returns>
        public ScheduleStatus GetCurrentStatus()
        {
            var config = GetCurrentPhaseConfiguration();
            
            return new ScheduleStatus
            {
                currentPhase = currentPhase,
                phaseName = config?.phaseName ?? "Unknown",
                phaseDescription = config?.description ?? "No description",
                timeInPhase = timeInCurrentPhase,
                totalElapsedTime = (float)(DateTime.Now - scheduleStartTime).TotalSeconds,
                isCompleted = scheduleCompleted,
                isAutomaticMode = enableAutomaticSchedule
            };
        }

        /// <summary>
        /// スケジュール状況の構造体
        /// </summary>
        [System.Serializable]
        public struct ScheduleStatus
        {
            public MigrationPhase currentPhase;
            public string phaseName;
            public string phaseDescription;
            public float timeInPhase;
            public float totalElapsedTime;
            public bool isCompleted;
            public bool isAutomaticMode;
        }

        #endregion

        #region Debugging and Logging

        /// <summary>
        /// 現在の状況をレポート
        /// </summary>
        [ContextMenu("Report Current Status")]
        public void ReportCurrentStatus()
        {
            var status = GetCurrentStatus();
            
            LogScheduleInfo("=== Migration Schedule Status ===");
            LogScheduleInfo($"Current Phase: {status.currentPhase} ({status.phaseName})");
            LogScheduleInfo($"Description: {status.phaseDescription}");
            LogScheduleInfo($"Time in Phase: {status.timeInPhase:F1} seconds");
            LogScheduleInfo($"Total Elapsed: {status.totalElapsedTime:F1} seconds");
            LogScheduleInfo($"Completed: {status.isCompleted}");
            LogScheduleInfo($"Automatic Mode: {status.isAutomaticMode}");
            
            // FeatureFlags状態の表示
            var config = GetCurrentPhaseConfiguration();
            if (config.HasValue)
            {
                LogScheduleInfo("=== Current FeatureFlags ===");
                LogScheduleInfo($"UseNewAudioService: {config.Value.useNewAudioService}");
                LogScheduleInfo($"UseNewSpatialService: {config.Value.useNewSpatialService}");
                LogScheduleInfo($"UseNewStealthService: {config.Value.useNewStealthService}");
                LogScheduleInfo($"DisableLegacySingletons: {config.Value.disableLegacySingletons}");
                LogScheduleInfo($"EnablePerformanceMonitoring: {config.Value.enablePerformanceMonitoring}");
            }
        }

        /// <summary>
        /// スケジュールログの出力
        /// </summary>
        /// <param name="message">メッセージ</param>
        private void LogScheduleInfo(string message)
        {
            if (enableDebugLogging)
            {
                EventLogger.LogStatic($"[MigrationScheduler] {message}");
            }
        }

        #endregion
    }
}