using UnityEngine;
using System;
using System.Collections.Generic;
using asterivo.Unity60.Core;
// using asterivo.Unity60.Core.Debug;
using asterivo.Unity60.Core.Services;

namespace asterivo.Unity60.Core.Services
{
    /// <summary>
    /// Step 3.7: 谿ｵ髫守噪讖溯・譛牙柑蛹悶せ繧ｱ繧ｸ繝･繝ｼ繝ｫ邂｡逅・す繧ｹ繝・Β
    /// Day 1-2, Day 3-4, Day 5縺ｮ谿ｵ髫守噪縺ｪFeatureFlags譛牙柑蛹悶ｒ邂｡逅・    /// </summary>
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
        /// 谿ｵ髫守噪遘ｻ陦後・繝輔ぉ繝ｼ繧ｺ螳夂ｾｩ
        /// </summary>
        public enum MigrationPhase
        {
            NotStarted = 0,
            Day1_2_Staging = 1,      // 繧ｹ繝・・繧ｸ繝ｳ繧ｰ迺ｰ蠅・ユ繧ｹ繝・            Day3_SpatialEnabled = 2,  // SpatialService譛牙柑蛹・            Day4_StealthEnabled = 3,  // StealthService譛牙柑蛹・ 
            Day5_Validation = 4,      // 蜈ｨ讀懆ｨｼ縺ｨ螳牙ｮ壼喧
            Completed = 5             // 繧ｹ繧ｱ繧ｸ繝･繝ｼ繝ｫ螳御ｺ・        }

        /// <summary>
        /// 蜷・ｮｵ髫弱・險ｭ螳壼ｮ夂ｾｩ
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
                description = "繧ｹ繝・・繧ｸ繝ｳ繧ｰ迺ｰ蠅・〒縺ｮAudioService繝・せ繝・
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
                description = "SpatialAudioService縺ｮ谿ｵ髫守噪譛牙柑蛹・
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
                description = "StealthAudioService縺ｮ谿ｵ髫守噪譛牙柑蛹・
            },
            new PhaseConfiguration
            {
                phase = MigrationPhase.Day5_Validation,
                phaseName = "Day 5: Full Validation",
                useNewAudioService = true,
                useNewSpatialService = true,
                useNewStealthService = true,
                disableLegacySingletons = false, // Week 4縺ｧ蟇ｾ蠢應ｺ亥ｮ・                enablePerformanceMonitoring = true,
                description = "蜈ｨ繧ｵ繝ｼ繝薙せ譛牙柑蛹悶〒縺ｮ讀懆ｨｼ縺ｨ螳牙ｮ壼喧"
            }
        };

        #region Unity Lifecycle

        private void Awake()
        {
            // 萓晏ｭ倥さ繝ｳ繝昴・繝阪Φ繝医・蛻晄悄蛹・            flagScheduler = GetComponent<FeatureFlagScheduler>() ?? gameObject.AddComponent<FeatureFlagScheduler>();
            progressTracker = GetComponent<MigrationProgressTracker>() ?? gameObject.AddComponent<MigrationProgressTracker>();
            
            // 蛻晄悄迥ｶ諷九・險ｭ螳・            if (currentPhase == MigrationPhase.NotStarted)
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
        /// 繧ｹ繧ｱ繧ｸ繝･繝ｼ繝ｫ縺ｮ髢句ｧ・        /// </summary>
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
        /// 谺｡縺ｮ繝輔ぉ繝ｼ繧ｺ縺ｫ騾ｲ繧
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
            
            // 谺｡縺ｮ繝輔ぉ繝ｼ繧ｺ縺ｫ騾ｲ繧
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
                LogScheduleInfo("脂 Migration schedule completed successfully!");
            }
        }

        /// <summary>
        /// 謇句虚縺ｧ繝輔ぉ繝ｼ繧ｺ繧定ｨｭ螳・        /// </summary>
        /// <param name="targetPhase">險ｭ螳壹☆繧九ヵ繧ｧ繝ｼ繧ｺ</param>
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
        /// 迴ｾ蝨ｨ縺ｮ繝輔ぉ繝ｼ繧ｺ險ｭ螳壹ｒFeatureFlags縺ｫ驕ｩ逕ｨ
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
        /// 迴ｾ蝨ｨ縺ｮ繝輔ぉ繝ｼ繧ｺ險ｭ螳壹ｒ蜿門ｾ・        /// </summary>
        /// <returns>繝輔ぉ繝ｼ繧ｺ險ｭ螳・/returns>
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
        /// 閾ｪ蜍輔せ繧ｱ繧ｸ繝･繝ｼ繝ｫ縺ｮ譖ｴ譁ｰ
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
        /// 迴ｾ蝨ｨ縺ｮ繝輔ぉ繝ｼ繧ｺ縺ｮ邯咏ｶ壽凾髢薙ｒ蜿門ｾ・        /// </summary>
        /// <returns>邯咏ｶ壽凾髢難ｼ育ｧ抵ｼ・/returns>
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
        /// 迴ｾ蝨ｨ縺ｮ繝輔ぉ繝ｼ繧ｺ邨碁℃譎る俣縺ｮ譖ｴ譁ｰ
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
        /// 迴ｾ蝨ｨ縺ｮ騾ｲ陦檎憾豕∵ュ蝣ｱ繧貞叙蠕・        /// </summary>
        /// <returns>騾ｲ陦檎憾豕∵ュ蝣ｱ</returns>
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
        /// 繧ｹ繧ｱ繧ｸ繝･繝ｼ繝ｫ迥ｶ豕√・讒矩菴・        /// </summary>
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
        /// 迴ｾ蝨ｨ縺ｮ迥ｶ豕√ｒ繝ｬ繝昴・繝・        /// </summary>
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
            
            // FeatureFlags迥ｶ諷九・陦ｨ遉ｺ
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
        /// 繧ｹ繧ｱ繧ｸ繝･繝ｼ繝ｫ繝ｭ繧ｰ縺ｮ蜃ｺ蜉・        /// </summary>
        /// <param name="message">繝｡繝・そ繝ｼ繧ｸ</param>
        private void LogScheduleInfo(string message)
        {
            if (enableDebugLogging)
            {
                ServiceLocator.GetService<IEventLogger>()?.Log($"[MigrationScheduler] {message}");
            }
        }

        #endregion
    }
}