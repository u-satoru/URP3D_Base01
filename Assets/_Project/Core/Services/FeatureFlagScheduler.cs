using UnityEngine;
using System.Collections.Generic;
using asterivo.Unity60.Core;
// using asterivo.Unity60.Core.Debug;
using asterivo.Unity60.Core.Services;

namespace asterivo.Unity60.Core.Services
{
    /// <summary>
    /// Step 3.7: FeatureFlags蜍慕噪譖ｴ譁ｰ邂｡逅・け繝ｩ繧ｹ
    /// MigrationScheduler縺九ｉ縺ｮ謖・､ｺ縺ｫ繧医ｊFeatureFlags繧呈ｮｵ髫守噪縺ｫ譖ｴ譁ｰ
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
        /// FeatureFlag螟画峩險倬鹸縺ｮ讒矩菴・        /// </summary>
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
            // 迴ｾ蝨ｨ縺ｮFeatureFlags縺ｮ迥ｶ諷九ｒ蛻晄悄蛹・            SyncWithCurrentFeatureFlags();
            LogFlagInfo("FeatureFlagScheduler initialized");
        }
        
        #region Phase Configuration Application
        
        /// <summary>
        /// 繝輔ぉ繝ｼ繧ｺ險ｭ螳壹ｒFeatureFlags縺ｫ驕ｩ逕ｨ
        /// </summary>
        /// <param name="config">繝輔ぉ繝ｼ繧ｺ險ｭ螳・/param>
        public void ApplyPhaseConfiguration(MigrationScheduler.PhaseConfiguration config)
        {
            LogFlagInfo($"Applying phase configuration: {config.phaseName}");
            
            // 蜷・ヵ繝ｩ繧ｰ繧呈ｮｵ髫守噪縺ｫ譖ｴ譁ｰ
            UpdateFeatureFlag(nameof(FeatureFlags.UseServiceLocator), true, $"Phase: {config.phaseName}", config.phase);
            UpdateFeatureFlag(nameof(FeatureFlags.UseNewAudioService), config.useNewAudioService, $"Phase: {config.phaseName}", config.phase);
            UpdateFeatureFlag(nameof(FeatureFlags.UseNewSpatialService), config.useNewSpatialService, $"Phase: {config.phaseName}", config.phase);
            UpdateFeatureFlag(nameof(FeatureFlags.UseNewStealthService), config.useNewStealthService, $"Phase: {config.phaseName}", config.phase);
            UpdateFeatureFlag(nameof(FeatureFlags.AllowSingletonFallback), !config.disableLegacySingletons, $"Phase: {config.phaseName}", config.phase);
            UpdateFeatureFlag(nameof(FeatureFlags.EnablePerformanceMonitoring), config.enablePerformanceMonitoring, $"Phase: {config.phaseName}", config.phase);
            
            // 萓晏ｭ倥☆繧倶ｻ悶・繝輔Λ繧ｰ繧よ峩譁ｰ
            UpdateFeatureFlag(nameof(FeatureFlags.MigrateStealthAudioCoordinator), config.useNewStealthService, $"Phase: {config.phaseName}", config.phase);
            UpdateFeatureFlag(nameof(FeatureFlags.EnableDebugLogging), true, $"Phase: {config.phaseName}", config.phase);
            UpdateFeatureFlag(nameof(FeatureFlags.EnableMigrationMonitoring), config.enablePerformanceMonitoring, $"Phase: {config.phaseName}", config.phase);
            
            // 迴ｾ蝨ｨ縺ｮ迥ｶ諷九ｒ譖ｴ譁ｰ
            SyncWithCurrentFeatureFlags();
            
            LogFlagInfo($"Successfully applied phase configuration: {config.phaseName}");
            LogCurrentFlagStates();
        }
        
        /// <summary>
        /// 蛟句挨縺ｮFeatureFlag繧呈峩譁ｰ
        /// </summary>
        /// <param name="flagName">繝輔Λ繧ｰ蜷・/param>
        /// <param name="newValue">譁ｰ縺励＞蛟､</param>
        /// <param name="reason">螟画峩逅・罰</param>
        /// <param name="phase">迴ｾ蝨ｨ縺ｮ繝輔ぉ繝ｼ繧ｺ</param>
        private void UpdateFeatureFlag(string flagName, bool newValue, string reason, MigrationScheduler.MigrationPhase phase)
        {
            bool oldValue = GetCurrentFlagValue(flagName);
            
            // 蛟､縺悟､画峩縺輔ｌ繧句ｴ蜷医・縺ｿ蜃ｦ逅・            if (oldValue != newValue)
            {
                // FeatureFlags繧ｯ繝ｩ繧ｹ縺ｮ蟇ｾ蠢懊・繝ｭ繝代ユ繧｣繧呈峩譁ｰ
                SetFeatureFlagValue(flagName, newValue);
                
                // 螟画峩險倬鹸縺ｮ菫晏ｭ・                RecordFlagChange(flagName, oldValue, newValue, reason, phase);
                
                if (logFlagChanges)
                {
                    LogFlagInfo($"Flag changed: {flagName} {oldValue} -> {newValue} ({reason})");
                }
            }
        }
        
        #endregion
        
        #region FeatureFlags Integration
        
        /// <summary>
        /// 迴ｾ蝨ｨ縺ｮFeatureFlags縺ｨ蜷梧悄
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
        /// 繝輔Λ繧ｰ蜷阪°繧臥樟蝨ｨ縺ｮ蛟､繧貞叙蠕・        /// </summary>
        /// <param name="flagName">繝輔Λ繧ｰ蜷・/param>
        /// <returns>迴ｾ蝨ｨ縺ｮ蛟､</returns>
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
        /// 繝輔Λ繧ｰ蜷阪↓蟇ｾ蠢懊☆繧句､繧定ｨｭ螳・        /// </summary>
        /// <param name="flagName">繝輔Λ繧ｰ蜷・/param>
        /// <param name="value">險ｭ螳壼､</param>
        private void SetFeatureFlagValue(string flagName, bool value)
        {
            // FeatureFlags繧ｯ繝ｩ繧ｹ縺ｯ譌｢縺ｫPlayerPrefs邨檎罰縺ｧ蜍慕噪譖ｴ譁ｰ蟇ｾ蠢懈ｸ医∩
            
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
        /// 繝輔Λ繧ｰ螟画峩繧定ｨ倬鹸
        /// </summary>
        /// <param name="flagName">繝輔Λ繧ｰ蜷・/param>
        /// <param name="oldValue">蜿､縺・､</param>
        /// <param name="newValue">譁ｰ縺励＞蛟､</param>
        /// <param name="reason">螟画峩逅・罰</param>
        /// <param name="phase">迴ｾ蝨ｨ縺ｮ繝輔ぉ繝ｼ繧ｺ</param>
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
            
            // 螻･豁ｴ繧ｵ繧､繧ｺ繧貞宛髯撰ｼ医Γ繝｢繝ｪ菴ｿ逕ｨ驥上ｒ蛻ｶ蠕｡・・            if (changeHistory.Count > 100)
            {
                changeHistory.RemoveRange(0, changeHistory.Count - 100);
            }
        }
        
        /// <summary>
        /// 螟画峩螻･豁ｴ繧偵け繝ｪ繧｢
        /// </summary>
        [ContextMenu("Clear Change History")]
        public void ClearChangeHistory()
        {
            changeHistory.Clear();
            LogFlagInfo("Change history cleared");
        }
        
        /// <summary>
        /// 螟画峩螻･豁ｴ繧偵Ξ繝昴・繝・        /// </summary>
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
        /// 迴ｾ蝨ｨ縺ｮ繝輔Λ繧ｰ迥ｶ諷九ｒ繝ｭ繧ｰ蜃ｺ蜉・        /// </summary>
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
        /// 繝輔Λ繧ｰ邨ｱ險域ュ蝣ｱ繧貞叙蠕・        /// </summary>
        /// <returns>繝輔Λ繧ｰ邨ｱ險域ュ蝣ｱ</returns>
        public FlagStatistics GetFlagStatistics()
        {
            int enabledFlags = 0;
            int totalFlags = 6; // 邂｡逅・ｯｾ雎｡繝輔Λ繧ｰ謨ｰ
            
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
        /// 繝輔Λ繧ｰ邨ｱ險域ュ蝣ｱ縺ｮ讒矩菴・        /// </summary>
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
        /// 繝・せ繝育畑縺ｮ謇句虚繝輔Λ繧ｰ險ｭ螳・        /// </summary>
        /// <param name="flagName">繝輔Λ繧ｰ蜷・/param>
        /// <param name="value">險ｭ螳壼､</param>
        [ContextMenu("Set Flag Manually")]
        public void SetFlagManually(string flagName, bool value)
        {
            UpdateFeatureFlag(flagName, value, "Manual setting", MigrationScheduler.MigrationPhase.NotStarted);
            SyncWithCurrentFeatureFlags();
            LogFlagInfo($"Manually set {flagName} to {value}");
        }
        
        /// <summary>
        /// 蜈ｨ繝輔Λ繧ｰ繧偵Μ繧ｻ繝・ヨ・磯幕逋ｺ逕ｨ・・        /// </summary>
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
        /// 繝輔Λ繧ｰ髢｢騾｣繝ｭ繧ｰ縺ｮ蜃ｺ蜉・        /// </summary>
        /// <param name="message">繝｡繝・そ繝ｼ繧ｸ</param>
        private void LogFlagInfo(string message)
        {
            if (enableDebugLogging)
            {
                ServiceLocator.GetService<IEventLogger>()?.Log($"[FeatureFlagScheduler] {message}");
            }
        }
        
        #endregion
    }
}
