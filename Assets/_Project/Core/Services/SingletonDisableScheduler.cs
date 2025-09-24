using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Core;
using System;
using System.Collections.Generic;
using UnityEngine;
// using asterivo.Unity60.Core.Debug;

namespace asterivo.Unity60.Core.Services
{
    /// <summary>
    /// Step 3.10: 段階的Singleton無効化スケジュール
    /// 5日間の計画に従ってSingletonを段階的に無効化する
    /// </summary>
    public class SingletonDisableScheduler : MonoBehaviour
    {
        [Header("Schedule Configuration")]
        [SerializeField] private bool enableAutoProgression = true;
        [SerializeField] private float dayDurationHours = 24f; // 実環境では24時間、テストでは短縮可能
        [SerializeField] private bool isTestEnvironment = false;
        
        [Header("Current Status")]
        [SerializeField] private ScheduleDay currentDay = ScheduleDay.NotStarted;
        [SerializeField] private DateTime scheduleStartTime;
        [SerializeField] private string scheduleStartTimeString; // Inspector表示用
        
        // スケジュール進行の記録
        private Dictionary<ScheduleDay, DayExecutionInfo> executionHistory = new Dictionary<ScheduleDay, DayExecutionInfo>();
        
        private void Start()
        {
            LoadScheduleState();
            ValidateCurrentSchedule();
            
            if (enableAutoProgression)
            {
                InvokeRepeating(nameof(CheckScheduleProgression), 60f, 60f); // 1分ごとに確認            }
            
            EventLogger.LogStatic($"[SingletonDisableScheduler] Started - Current Day: {currentDay}, Auto: {enableAutoProgression}");
        }
        
        private void OnDestroy()
        {
            SaveScheduleState();
        }
        
        /// <summary>
        /// スケジュールを開始する        /// </summary>
        [ContextMenu("Start Schedule")]
        public void StartSchedule()
        {
            if (currentDay != ScheduleDay.NotStarted)
            {
                ServiceLocator.GetService<IEventLogger>()?.LogWarning($"[SingletonDisableScheduler] Schedule already started. Current day: {currentDay}");
                return;
            }
            
            scheduleStartTime = DateTime.Now;
            scheduleStartTimeString = scheduleStartTime.ToString("yyyy-MM-dd HH:mm:ss");
            currentDay = ScheduleDay.Day1_WarningsEnabled;
            
            ExecuteDay1Configuration();
            SaveScheduleState();
            
            EventLogger.LogStatic($"[SingletonDisableScheduler] Schedule started at {scheduleStartTimeString}");
        }
        
        /// <summary>
        /// スケジュール進行をチェックし、必要に応じて次の日に進む
        /// </summary>
        private void CheckScheduleProgression()
        {
            if (currentDay == ScheduleDay.NotStarted || currentDay == ScheduleDay.Completed)
                return;
                
            DateTime now = DateTime.Now;
            TimeSpan elapsed = now - scheduleStartTime;
            int expectedDay = Mathf.FloorToInt((float)elapsed.TotalHours / dayDurationHours) + 1;
            
            ScheduleDay targetDay = GetScheduleDayFromNumber(expectedDay);
            
            if (targetDay != currentDay && targetDay != ScheduleDay.NotStarted)
            {
                AdvanceToDay(targetDay);
            }
        }
        
        /// <summary>
        /// 指定された日まで進める（手動制御用）        /// </summary>
        [ContextMenu("Advance to Day 2")]
        public void AdvanceToDay2() => AdvanceToDay(ScheduleDay.Day2_IssueFixing);
        
        [ContextMenu("Advance to Day 3")]
        public void AdvanceToDay3() => AdvanceToDay(ScheduleDay.Day3_ContinuedFixing);
        
        [ContextMenu("Advance to Day 4")]
        public void AdvanceToDay4() => AdvanceToDay(ScheduleDay.Day4_SingletonDisabled);
        
        [ContextMenu("Advance to Day 5")]
        public void AdvanceToDay5() => AdvanceToDay(ScheduleDay.Day5_CompleteRemoval);
        
        /// <summary>
        /// 指定された日に進める
        /// </summary>
        private void AdvanceToDay(ScheduleDay targetDay)
        {
            if (targetDay <= currentDay && targetDay != ScheduleDay.NotStarted)
            {
                ServiceLocator.GetService<IEventLogger>()?.LogWarning($"[SingletonDisableScheduler] Cannot advance to previous day. Current: {currentDay}, Target: {targetDay}");
                return;
            }
            
            ScheduleDay previousDay = currentDay;
            currentDay = targetDay;
            
            ExecuteDayConfiguration(targetDay);
            RecordDayTransition(previousDay, targetDay);
            SaveScheduleState();
            
            EventLogger.LogStatic($"[SingletonDisableScheduler] Advanced from {previousDay} to {targetDay}");
        }
        
        /// <summary>
        /// Day 1: 警告システム有効化        /// </summary>
        private void ExecuteDay1Configuration()
        {
            EventLogger.LogStatic("[SingletonDisableScheduler] === Day 1: Warnings Enabled ===");
            
            // テスト環境で警告システム有効化            FeatureFlags.EnableMigrationWarnings = true;
            FeatureFlags.DisableLegacySingletons = false;
            
            if (isTestEnvironment)
            {
                EventLogger.LogStatic("[SingletonDisableScheduler] Day 1: Test environment - Migration warnings enabled");
            }
            else
            {
                EventLogger.LogStatic("[SingletonDisableScheduler] Day 1: Production environment - Migration warnings enabled");
            }
            
            // MigrationMonitorの統計リセット
            var monitor = FindFirstObjectByType<MigrationMonitor>();
            monitor?.ResetStatistics();
        }
        
        /// <summary>
        /// Day 2-3: 問題修正期間
        /// </summary>
        private void ExecuteDay2_3Configuration()
        {
            EventLogger.LogStatic("[SingletonDisableScheduler] === Day 2-3: Issue Fixing Period ===");
            
            // 警告は継続、詳細な監視を開始            FeatureFlags.EnableMigrationWarnings = true;
            FeatureFlags.DisableLegacySingletons = false;
            FeatureFlags.EnableMigrationMonitoring = true;
            
            // 使用状況レポート生成            var monitor = FindFirstObjectByType<MigrationMonitor>();
            monitor?.GenerateUsageReport();
            monitor?.GenerateMigrationRecommendations();
            
            EventLogger.LogStatic("[SingletonDisableScheduler] Day 2-3: Focus on fixing singleton usage based on warnings");
        }
        
        /// <summary>
        /// Day 4: Singleton段階的無効化        /// </summary>
        private void ExecuteDay4Configuration()
        {
            EventLogger.LogStatic("[SingletonDisableScheduler] === Day 4: Singleton Disabled ===");
            
            // 本番環境でSingleton無効化            FeatureFlags.EnableMigrationWarnings = true; // 警告は継続            FeatureFlags.DisableLegacySingletons = true;  // Singleton無効化            
            ServiceLocator.GetService<IEventLogger>()?.LogWarning("[SingletonDisableScheduler] Day 4: Legacy Singletons are now DISABLED");
            ServiceLocator.GetService<IEventLogger>()?.LogWarning("[SingletonDisableScheduler] Day 4: All code should use ServiceLocator from now on");
            
            // 最終使用状況チェック
            var monitor = FindFirstObjectByType<MigrationMonitor>();
            if (monitor != null)
            {
                monitor.GenerateUsageReport();
                monitor.SaveUsageStatistics();
            }
        }
        
        /// <summary>
        /// Day 5: 最終検証と完全削除準備
        /// </summary>
        private void ExecuteDay5Configuration()
        {
            EventLogger.LogStatic("[SingletonDisableScheduler] === Day 5: Complete Removal Preparation ===");
            
            // 完全削除準備
            FeatureFlags.EnableMigrationWarnings = false; // 警告停止
            FeatureFlags.DisableLegacySingletons = true;   // 無効化継続            
            // 最終検証実行            var migrationValidator = FindFirstObjectByType<MigrationValidator>();
            migrationValidator?.ValidateMigration();
            
            EventLogger.LogStatic("[SingletonDisableScheduler] Day 5: Ready for complete singleton code removal");
            EventLogger.LogStatic("[SingletonDisableScheduler] Day 5: Migration process completed successfully");
            
            currentDay = ScheduleDay.Completed;
        }
        
        /// <summary>
        /// 日別設定を実行        /// </summary>
        private void ExecuteDayConfiguration(ScheduleDay day)
        {
            switch (day)
            {
                case ScheduleDay.Day1_WarningsEnabled:
                    ExecuteDay1Configuration();
                    break;
                case ScheduleDay.Day2_IssueFixing:
                case ScheduleDay.Day3_ContinuedFixing:
                    ExecuteDay2_3Configuration();
                    break;
                case ScheduleDay.Day4_SingletonDisabled:
                    ExecuteDay4Configuration();
                    break;
                case ScheduleDay.Day5_CompleteRemoval:
                    ExecuteDay5Configuration();
                    break;
            }
        }
        
        /// <summary>
        /// 日変更を記録
        /// </summary>
        private void RecordDayTransition(ScheduleDay from, ScheduleDay to)
        {
            var info = new DayExecutionInfo
            {
                Day = to,
                ExecutionTime = DateTime.Now,
                PreviousDay = from,
                FeatureFlagsSnapshot = GetCurrentFeatureFlagsSnapshot()
            };
            
            executionHistory[to] = info;
        }
        
        /// <summary>
        /// 現在のFeatureFlags状態のスナップショットを取得        /// </summary>
        private Dictionary<string, bool> GetCurrentFeatureFlagsSnapshot()
        {
            return new Dictionary<string, bool>
            {
                ["EnableMigrationWarnings"] = FeatureFlags.EnableMigrationWarnings,
                ["DisableLegacySingletons"] = FeatureFlags.DisableLegacySingletons,
                ["EnableMigrationMonitoring"] = FeatureFlags.EnableMigrationMonitoring,
                ["UseServiceLocator"] = FeatureFlags.UseServiceLocator,
                ["UseNewAudioService"] = FeatureFlags.UseNewAudioService,
                ["UseNewSpatialService"] = FeatureFlags.UseNewSpatialService,
                ["UseNewStealthService"] = FeatureFlags.UseNewStealthService
            };
        }
        
        /// <summary>
        /// スケジュール状態を保存        /// </summary>
        private void SaveScheduleState()
        {
            PlayerPrefs.SetInt("SingletonDisableScheduler_CurrentDay", (int)currentDay);
            PlayerPrefs.SetString("SingletonDisableScheduler_StartTime", scheduleStartTime.ToBinary().ToString());
            PlayerPrefs.SetFloat("SingletonDisableScheduler_DayDuration", dayDurationHours);
            PlayerPrefs.SetInt("SingletonDisableScheduler_IsTest", isTestEnvironment ? 1 : 0);
            PlayerPrefs.Save();
        }
        
        /// <summary>
        /// スケジュール状態を読み込み
        /// </summary>
        private void LoadScheduleState()
        {
            currentDay = (ScheduleDay)PlayerPrefs.GetInt("SingletonDisableScheduler_CurrentDay", 0);
            
            string startTimeStr = PlayerPrefs.GetString("SingletonDisableScheduler_StartTime", "");
            if (!string.IsNullOrEmpty(startTimeStr) && long.TryParse(startTimeStr, out long startTimeBinary))
            {
                scheduleStartTime = DateTime.FromBinary(startTimeBinary);
                scheduleStartTimeString = scheduleStartTime.ToString("yyyy-MM-dd HH:mm:ss");
            }
            
            dayDurationHours = PlayerPrefs.GetFloat("SingletonDisableScheduler_DayDuration", 24f);
            isTestEnvironment = PlayerPrefs.GetInt("SingletonDisableScheduler_IsTest", 0) == 1;
        }
        
        /// <summary>
        /// 現在のスケジュールを検証
        /// </summary>
        private void ValidateCurrentSchedule()
        {
            if (currentDay != ScheduleDay.NotStarted && scheduleStartTime == default)
            {
                ServiceLocator.GetService<IEventLogger>()?.LogError("[SingletonDisableScheduler] Invalid state: Schedule is started but no start time recorded");
                ResetSchedule();
            }
        }
        
        /// <summary>
        /// スケジュールをリセット
        /// </summary>
        [ContextMenu("Reset Schedule")]
        public void ResetSchedule()
        {
            currentDay = ScheduleDay.NotStarted;
            scheduleStartTime = default;
            scheduleStartTimeString = "";
            executionHistory.Clear();
            
            // FeatureFlagsを安全な状態に戻す            FeatureFlags.EnableMigrationWarnings = false;
            FeatureFlags.DisableLegacySingletons = false;
            
            SaveScheduleState();
            EventLogger.LogStatic("[SingletonDisableScheduler] Schedule reset to initial state");
        }
        
        /// <summary>
        /// スケジュール進行状況レポートを生成
        /// </summary>
        [ContextMenu("Generate Status Report")]
        public void GenerateStatusReport()
        {
            EventLogger.LogStatic("[SingletonDisableScheduler] === Schedule Status Report ===");
            EventLogger.LogStatic($"  Current Day: {currentDay}");
            EventLogger.LogStatic($"  Start Time: {scheduleStartTimeString}");
            EventLogger.LogStatic($"  Day Duration: {dayDurationHours} hours");
            EventLogger.LogStatic($"  Environment: {(isTestEnvironment ? "Test" : "Production")}");
            EventLogger.LogStatic($"  Auto Progression: {enableAutoProgression}");
            
            if (currentDay != ScheduleDay.NotStarted)
            {
                TimeSpan elapsed = DateTime.Now - scheduleStartTime;
                EventLogger.LogStatic($"  Elapsed Time: {elapsed.TotalHours:F1} hours ({elapsed.TotalDays:F1} days)");
                
                float progress = GetScheduleProgress();
                EventLogger.LogStatic($"  Overall Progress: {progress:F1}%");
            }
            
            EventLogger.LogStatic("  Feature Flags Status:");
            var snapshot = GetCurrentFeatureFlagsSnapshot();
            foreach (var kvp in snapshot)
            {
                EventLogger.LogStatic($"    - {kvp.Key}: {kvp.Value}");
            }
        }
        
        /// <summary>
        /// スケジュール進行率を取得（0-100%）
        /// </summary>
        public float GetScheduleProgress()
        {
            if (currentDay == ScheduleDay.NotStarted) return 0f;
            if (currentDay == ScheduleDay.Completed) return 100f;
            
            return ((int)currentDay / 5f) * 100f;
        }
        
        /// <summary>
        /// 番号からScheduleDayを取得
        /// </summary>
        private ScheduleDay GetScheduleDayFromNumber(int dayNumber)
        {
            return dayNumber switch
            {
                1 => ScheduleDay.Day1_WarningsEnabled,
                2 => ScheduleDay.Day2_IssueFixing,
                3 => ScheduleDay.Day3_ContinuedFixing,
                4 => ScheduleDay.Day4_SingletonDisabled,
                5 => ScheduleDay.Day5_CompleteRemoval,
                _ => ScheduleDay.NotStarted
            };
        }
    }

    /// <summary>
    /// スケジュールの日程
    /// </summary>
    public enum ScheduleDay
    {
        NotStarted = 0,
        Day1_WarningsEnabled = 1,
        Day2_IssueFixing = 2,
        Day3_ContinuedFixing = 3,
        Day4_SingletonDisabled = 4,
        Day5_CompleteRemoval = 5,
        Completed = 6
    }

    /// <summary>
    /// 日別実行情報
    /// </summary>
    [System.Serializable]
    public class DayExecutionInfo
    {
        public ScheduleDay Day;
        public DateTime ExecutionTime;
        public ScheduleDay PreviousDay;
        public Dictionary<string, bool> FeatureFlagsSnapshot;
    }
}