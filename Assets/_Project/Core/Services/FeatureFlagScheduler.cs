using UnityEngine;
using System.Collections.Generic;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Debug;

namespace asterivo.Unity60.Core.Services
{
    /// <summary>
    /// Step 3.7: FeatureFlags動的更新管理クラス
    /// MigrationSchedulerからの指示によりFeatureFlagsを段階的に更新
    /// </summary>
    public class FeatureFlagScheduler : MonoBehaviour
    {
        [Header("Flag Update Configuration")]
        [SerializeField] private bool enableDebugLogging = true;
        [SerializeField] private bool logFlagChanges = true;
        
        [Header("Current Flag States")]
        [SerializeField] private bool currentUseServiceLocator;
        [SerializeField] private bool currentUseNewAudioService;
        [SerializeField] private bool currentUseNewSpatialService;
        [SerializeField] private bool currentUseNewStealthService;
        [SerializeField] private bool currentDisableLegacySingletons;
        [SerializeField] private bool currentEnablePerformanceMonitoring;
        
        [Header("Flag Change History")]
        [SerializeField] private List<FlagChangeRecord> changeHistory = new List<FlagChangeRecord>();
        
        /// <summary>
        /// FeatureFlag変更記録の構造体
        /// </summary>
        [System.Serializable]
        public struct FlagChangeRecord
        {
            public float timestamp;
            public string flagName;
            public bool oldValue;
            public bool newValue;
            public string reason;
            public MigrationScheduler.MigrationPhase phase;
        }
        
        private void Start()
        {
            // 現在のFeatureFlagsの状態を初期化
            SyncWithCurrentFeatureFlags();
            LogFlagInfo("FeatureFlagScheduler initialized");
        }
        
        #region Phase Configuration Application
        
        /// <summary>
        /// フェーズ設定をFeatureFlagsに適用
        /// </summary>
        /// <param name="config">フェーズ設定</param>
        public void ApplyPhaseConfiguration(MigrationScheduler.PhaseConfiguration config)
        {
            LogFlagInfo($"Applying phase configuration: {config.phaseName}");
            
            // 各フラグを段階的に更新
            UpdateFeatureFlag(nameof(FeatureFlags.UseServiceLocator), true, $"Phase: {config.phaseName}", config.phase);
            UpdateFeatureFlag(nameof(FeatureFlags.UseNewAudioService), config.useNewAudioService, $"Phase: {config.phaseName}", config.phase);
            UpdateFeatureFlag(nameof(FeatureFlags.UseNewSpatialService), config.useNewSpatialService, $"Phase: {config.phaseName}", config.phase);
            UpdateFeatureFlag(nameof(FeatureFlags.UseNewStealthService), config.useNewStealthService, $"Phase: {config.phaseName}", config.phase);
            UpdateFeatureFlag(nameof(FeatureFlags.AllowSingletonFallback), !config.disableLegacySingletons, $"Phase: {config.phaseName}", config.phase);
            UpdateFeatureFlag(nameof(FeatureFlags.EnablePerformanceMonitoring), config.enablePerformanceMonitoring, $"Phase: {config.phaseName}", config.phase);
            
            // 依存する他のフラグも更新
            UpdateFeatureFlag(nameof(FeatureFlags.MigrateStealthAudioCoordinator), config.useNewStealthService, $"Phase: {config.phaseName}", config.phase);
            UpdateFeatureFlag(nameof(FeatureFlags.EnableDebugLogging), true, $"Phase: {config.phaseName}", config.phase);
            UpdateFeatureFlag(nameof(FeatureFlags.EnableMigrationMonitoring), config.enablePerformanceMonitoring, $"Phase: {config.phaseName}", config.phase);
            
            // 現在の状態を更新
            SyncWithCurrentFeatureFlags();
            
            LogFlagInfo($"Successfully applied phase configuration: {config.phaseName}");
            LogCurrentFlagStates();
        }
        
        /// <summary>
        /// 個別のFeatureFlagを更新
        /// </summary>
        /// <param name="flagName">フラグ名</param>
        /// <param name="newValue">新しい値</param>
        /// <param name="reason">変更理由</param>
        /// <param name="phase">現在のフェーズ</param>
        private void UpdateFeatureFlag(string flagName, bool newValue, string reason, MigrationScheduler.MigrationPhase phase)
        {
            bool oldValue = GetCurrentFlagValue(flagName);
            
            // 値が変更される場合のみ処理
            if (oldValue != newValue)
            {
                // FeatureFlagsクラスの対応プロパティを更新
                SetFeatureFlagValue(flagName, newValue);
                
                // 変更記録の保存
                RecordFlagChange(flagName, oldValue, newValue, reason, phase);
                
                if (logFlagChanges)
                {
                    LogFlagInfo($"Flag changed: {flagName} {oldValue} -> {newValue} ({reason})");
                }
            }
        }
        
        #endregion
        
        #region FeatureFlags Integration
        
        /// <summary>
        /// 現在のFeatureFlagsと同期
        /// </summary>
        private void SyncWithCurrentFeatureFlags()
        {
            currentUseServiceLocator = FeatureFlags.UseServiceLocator;
            currentUseNewAudioService = FeatureFlags.UseNewAudioService;
            currentUseNewSpatialService = FeatureFlags.UseNewSpatialService;  
            currentUseNewStealthService = FeatureFlags.UseNewStealthService;
            currentDisableLegacySingletons = !FeatureFlags.AllowSingletonFallback;
            currentEnablePerformanceMonitoring = FeatureFlags.EnablePerformanceMonitoring;
        }
        
        /// <summary>
        /// フラグ名から現在の値を取得
        /// </summary>
        /// <param name="flagName">フラグ名</param>
        /// <returns>現在の値</returns>
        private bool GetCurrentFlagValue(string flagName)
        {
            switch (flagName)
            {
                case nameof(FeatureFlags.UseServiceLocator):
                    return FeatureFlags.UseServiceLocator;
                case nameof(FeatureFlags.UseNewAudioService):
                    return FeatureFlags.UseNewAudioService;
                case nameof(FeatureFlags.UseNewSpatialService):
                    return FeatureFlags.UseNewSpatialService;
                case nameof(FeatureFlags.UseNewStealthService):
                    return FeatureFlags.UseNewStealthService;
                case nameof(FeatureFlags.AllowSingletonFallback):
                    return FeatureFlags.AllowSingletonFallback;
                case nameof(FeatureFlags.EnablePerformanceMonitoring):
                    return FeatureFlags.EnablePerformanceMonitoring;
                case nameof(FeatureFlags.MigrateStealthAudioCoordinator):
                    return FeatureFlags.MigrateStealthAudioCoordinator;
                case nameof(FeatureFlags.EnableDebugLogging):
                    return FeatureFlags.EnableDebugLogging;
                case nameof(FeatureFlags.EnableMigrationMonitoring):
                    return FeatureFlags.EnableMigrationMonitoring;
                default:
                    LogFlagInfo($"Unknown flag: {flagName}");
                    return false;
            }
        }
        
        /// <summary>
        /// フラグ名に対応する値を設定
        /// </summary>
        /// <param name="flagName">フラグ名</param>
        /// <param name="value">設定値</param>
        private void SetFeatureFlagValue(string flagName, bool value)
        {
            // FeatureFlagsクラスは既にPlayerPrefs経由で動的更新対応済み
            
            switch (flagName)
            {
                case nameof(FeatureFlags.UseServiceLocator):
                    FeatureFlags.UseServiceLocator = value;
                    LogFlagInfo($"Set {flagName} to {value}");
                    break;
                case nameof(FeatureFlags.UseNewAudioService):
                    FeatureFlags.UseNewAudioService = value;
                    LogFlagInfo($"Set {flagName} to {value}");
                    break;
                case nameof(FeatureFlags.UseNewSpatialService):
                    FeatureFlags.UseNewSpatialService = value;
                    LogFlagInfo($"Set {flagName} to {value}");
                    break;
                case nameof(FeatureFlags.UseNewStealthService):
                    FeatureFlags.UseNewStealthService = value;
                    LogFlagInfo($"Set {flagName} to {value}");
                    break;
                case nameof(FeatureFlags.AllowSingletonFallback):
                    FeatureFlags.AllowSingletonFallback = value;
                    LogFlagInfo($"Set {flagName} to {value}");
                    break;
                case nameof(FeatureFlags.EnablePerformanceMonitoring):
                    FeatureFlags.EnablePerformanceMonitoring = value;
                    LogFlagInfo($"Set {flagName} to {value}");
                    break;
                case nameof(FeatureFlags.MigrateStealthAudioCoordinator):
                    FeatureFlags.MigrateStealthAudioCoordinator = value;
                    LogFlagInfo($"Set {flagName} to {value}");
                    break;
                case nameof(FeatureFlags.EnableDebugLogging):
                    FeatureFlags.EnableDebugLogging = value;
                    LogFlagInfo($"Set {flagName} to {value}");
                    break;
                case nameof(FeatureFlags.EnableMigrationMonitoring):
                    FeatureFlags.EnableMigrationMonitoring = value;
                    LogFlagInfo($"Set {flagName} to {value}");
                    break;
                default:
                    LogFlagInfo($"Unknown flag for setting: {flagName}");
                    break;
            }
        }
        
        #endregion
        
        #region Change History Management
        
        /// <summary>
        /// フラグ変更を記録
        /// </summary>
        /// <param name="flagName">フラグ名</param>
        /// <param name="oldValue">古い値</param>
        /// <param name="newValue">新しい値</param>
        /// <param name="reason">変更理由</param>
        /// <param name="phase">現在のフェーズ</param>
        private void RecordFlagChange(string flagName, bool oldValue, bool newValue, string reason, MigrationScheduler.MigrationPhase phase)
        {
            var record = new FlagChangeRecord
            {
                timestamp = Time.time,
                flagName = flagName,
                oldValue = oldValue,
                newValue = newValue,
                reason = reason,
                phase = phase
            };
            
            changeHistory.Add(record);
            
            // 履歴サイズを制限（メモリ使用量を制御）
            if (changeHistory.Count > 100)
            {
                changeHistory.RemoveRange(0, changeHistory.Count - 100);
            }
        }
        
        /// <summary>
        /// 変更履歴をクリア
        /// </summary>
        [ContextMenu("Clear Change History")]
        public void ClearChangeHistory()
        {
            changeHistory.Clear();
            LogFlagInfo("Change history cleared");
        }
        
        /// <summary>
        /// 変更履歴をレポート
        /// </summary>
        [ContextMenu("Report Change History")]
        public void ReportChangeHistory()
        {
            LogFlagInfo("=== FeatureFlag Change History ===");
            
            if (changeHistory.Count == 0)
            {
                LogFlagInfo("No flag changes recorded");
                return;
            }
            
            foreach (var record in changeHistory)
            {
                LogFlagInfo($"[{record.timestamp:F1}s] {record.flagName}: {record.oldValue} -> {record.newValue} " +
                           $"(Phase: {record.phase}, Reason: {record.reason})");
            }
            
            LogFlagInfo($"Total changes recorded: {changeHistory.Count}");
        }
        
        #endregion
        
        #region Status and Information
        
        /// <summary>
        /// 現在のフラグ状態をログ出力
        /// </summary>
        [ContextMenu("Log Current Flag States")]
        public void LogCurrentFlagStates()
        {
            LogFlagInfo("=== Current FeatureFlag States ===");
            LogFlagInfo($"UseServiceLocator: {currentUseServiceLocator}");
            LogFlagInfo($"UseNewAudioService: {currentUseNewAudioService}");
            LogFlagInfo($"UseNewSpatialService: {currentUseNewSpatialService}");
            LogFlagInfo($"UseNewStealthService: {currentUseNewStealthService}");
            LogFlagInfo($"DisableLegacySingletons: {currentDisableLegacySingletons}");
            LogFlagInfo($"EnablePerformanceMonitoring: {currentEnablePerformanceMonitoring}");
        }
        
        /// <summary>
        /// フラグ統計情報を取得
        /// </summary>
        /// <returns>フラグ統計情報</returns>
        public FlagStatistics GetFlagStatistics()
        {
            int enabledFlags = 0;
            int totalFlags = 6; // 管理対象フラグ数
            
            if (currentUseServiceLocator) enabledFlags++;
            if (currentUseNewAudioService) enabledFlags++;
            if (currentUseNewSpatialService) enabledFlags++;
            if (currentUseNewStealthService) enabledFlags++;
            if (currentDisableLegacySingletons) enabledFlags++;
            if (currentEnablePerformanceMonitoring) enabledFlags++;
            
            return new FlagStatistics
            {
                enabledFlags = enabledFlags,
                totalFlags = totalFlags,
                enabledPercentage = (float)enabledFlags / totalFlags * 100f,
                totalChanges = changeHistory.Count,
                lastChangeTime = changeHistory.Count > 0 ? changeHistory[changeHistory.Count - 1].timestamp : 0f
            };
        }
        
        /// <summary>
        /// フラグ統計情報の構造体
        /// </summary>
        [System.Serializable]
        public struct FlagStatistics
        {
            public int enabledFlags;
            public int totalFlags;
            public float enabledPercentage;
            public int totalChanges;
            public float lastChangeTime;
        }
        
        #endregion
        
        #region Testing and Development Support
        
        /// <summary>
        /// テスト用の手動フラグ設定
        /// </summary>
        /// <param name="flagName">フラグ名</param>
        /// <param name="value">設定値</param>
        [ContextMenu("Set Flag Manually")]
        public void SetFlagManually(string flagName, bool value)
        {
            UpdateFeatureFlag(flagName, value, "Manual setting", MigrationScheduler.MigrationPhase.NotStarted);
            SyncWithCurrentFeatureFlags();
            LogFlagInfo($"Manually set {flagName} to {value}");
        }
        
        /// <summary>
        /// 全フラグをリセット（開発用）
        /// </summary>
        [ContextMenu("Reset All Flags")]
        public void ResetAllFlags()
        {
            LogFlagInfo("Resetting all flags to default values");
            
            UpdateFeatureFlag(nameof(FeatureFlags.UseServiceLocator), true, "Reset to default", MigrationScheduler.MigrationPhase.NotStarted);
            UpdateFeatureFlag(nameof(FeatureFlags.UseNewAudioService), false, "Reset to default", MigrationScheduler.MigrationPhase.NotStarted);
            UpdateFeatureFlag(nameof(FeatureFlags.UseNewSpatialService), false, "Reset to default", MigrationScheduler.MigrationPhase.NotStarted);
            UpdateFeatureFlag(nameof(FeatureFlags.UseNewStealthService), false, "Reset to default", MigrationScheduler.MigrationPhase.NotStarted);
            UpdateFeatureFlag(nameof(FeatureFlags.AllowSingletonFallback), true, "Reset to default", MigrationScheduler.MigrationPhase.NotStarted);
            UpdateFeatureFlag(nameof(FeatureFlags.EnablePerformanceMonitoring), false, "Reset to default", MigrationScheduler.MigrationPhase.NotStarted);
            
            SyncWithCurrentFeatureFlags();
            LogFlagInfo("All flags reset to default values");
        }
        
        #endregion
        
        #region Logging
        
        /// <summary>
        /// フラグ関連ログの出力
        /// </summary>
        /// <param name="message">メッセージ</param>
        private void LogFlagInfo(string message)
        {
            if (enableDebugLogging)
            {
                EventLogger.LogStatic($"[FeatureFlagScheduler] {message}");
            }
        }
        
        #endregion
    }
}