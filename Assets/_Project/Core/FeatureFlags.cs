using UnityEngine;
using System;
using System.Collections.Generic;
// // using asterivo.Unity60.Core.Debug; // Removed to avoid circular dependency

namespace asterivo.Unity60.Core
{
    /// <summary>
    /// Phase 3 蠑ｷ蛹也沿 Feature Flag 繧ｷ繧ｹ繝・Β
    /// 谿ｵ髫守噪遘ｻ陦後√ヱ繝輔か繝ｼ繝槭Φ繧ｹ逶｣隕悶√Ο繝ｼ繝ｫ繝舌ャ繧ｯ讖溯・繧堤ｵｱ蜷・    /// </summary>
    public static class FeatureFlags
    {
        // ========== 譌｢蟄倥・蝓ｺ譛ｬ繝輔Λ繧ｰ ==========
        
        /// <summary>
        /// 譁ｰ縺励＞繧ｪ繝ｼ繝・ぅ繧ｪ繧ｷ繧ｹ繝・Β繧剃ｽｿ逕ｨ縺吶ｋ縺・        /// </summary>
        public static bool UseNewAudioSystem 
        {
            get => PlayerPrefs.GetInt("FeatureFlag_UseNewAudioSystem", 0) == 1;
            set => SetFlag("FeatureFlag_UseNewAudioSystem", value);
        }
        
        /// <summary>
        /// Service Locator繝代ち繝ｼ繝ｳ繧剃ｽｿ逕ｨ縺吶ｋ縺・        /// </summary>
        public static bool UseServiceLocator
        {
            get => PlayerPrefs.GetInt("FeatureFlag_UseServiceLocator", 1) == 1;
            set => SetFlag("FeatureFlag_UseServiceLocator", value);
        }
        
        /// <summary>
        /// 繧､繝吶Φ繝磯ｧ・虚髻ｳ髻ｿ繧ｷ繧ｹ繝・Β繧剃ｽｿ逕ｨ縺吶ｋ縺・        /// </summary>
        public static bool UseEventDrivenAudio
        {
            get => PlayerPrefs.GetInt("FeatureFlag_UseEventDrivenAudio", 0) == 1;
            set => SetFlag("FeatureFlag_UseEventDrivenAudio", value);
        }
        
        /// <summary>
        /// 譁ｰ縺励＞AudioUpdateCoordinator繧ｵ繝ｼ繝薙せ繧剃ｽｿ逕ｨ縺吶ｋ縺・        /// </summary>
        public static bool UseNewAudioUpdateSystem
        {
            get => PlayerPrefs.GetInt("FeatureFlag_UseNewAudioUpdateSystem", 0) == 1;
            set => SetFlag("FeatureFlag_UseNewAudioUpdateSystem", value);
        }
        
        /// <summary>
        /// 繝・ヰ繝・げ繝ｭ繧ｰ繧呈怏蜉ｹ縺ｫ縺吶ｋ縺・        /// </summary>
        public static bool EnableDebugLogging
        {
            get => PlayerPrefs.GetInt("FeatureFlag_EnableDebugLogging", 1) == 1;
            set => SetFlag("FeatureFlag_EnableDebugLogging", value);
        }
        
        /// <summary>
        /// 繝ｪ繝輔ぃ繧ｯ繧ｿ繝ｪ繝ｳ繧ｰ蠕後・繧｢繝ｼ繧ｭ繝・け繝√Ε繧剃ｽｿ逕ｨ縺吶ｋ縺具ｼ域ｮｵ髫守噪遘ｻ陦檎畑・・        /// </summary>
        public static bool UseRefactoredArchitecture
        {
            get => PlayerPrefs.GetInt("FeatureFlag_UseRefactoredArchitecture", 0) == 1;
            set => SetFlag("FeatureFlag_UseRefactoredArchitecture", value);
        }
        
                
        /// <summary>
        /// 譁ｰ縺励＞AudioService繧剃ｽｿ逕ｨ縺吶ｋ縺具ｼ・tep 3.7逕ｨ・・        /// </summary>
        public static bool UseNewAudioService
        {
            get => PlayerPrefs.GetInt("FeatureFlag_UseNewAudioService", 1) == 1; // 笨・Task1: 繝・ヵ繧ｩ繝ｫ繝亥､繧・縺ｫ螟画峩
            set => SetFlag("FeatureFlag_UseNewAudioService", value);
        }
        
        /// <summary>
        /// 譁ｰ縺励＞SpatialAudioService繧剃ｽｿ逕ｨ縺吶ｋ縺具ｼ・tep 3.7逕ｨ・・        /// </summary>
        public static bool UseNewSpatialService
        {
            get => PlayerPrefs.GetInt("FeatureFlag_UseNewSpatialService", 1) == 1; // 笨・Task1: 繝・ヵ繧ｩ繝ｫ繝亥､繧・縺ｫ螟画峩
            set => SetFlag("FeatureFlag_UseNewSpatialService", value);
        }
        
        /// <summary>
        /// 譁ｰ縺励＞StealthAudioService繧剃ｽｿ逕ｨ縺吶ｋ縺具ｼ・tep 3.7逕ｨ・・        /// </summary>
        public static bool UseNewStealthService
        {
            get => PlayerPrefs.GetInt("FeatureFlag_UseNewStealthService", 1) == 1; // 笨・Task1: 繝・ヵ繧ｩ繝ｫ繝亥､繧・縺ｫ螟画峩
            set => SetFlag("FeatureFlag_UseNewStealthService", value);
        }
        
        /// <summary>
        /// 繝代ヵ繧ｩ繝ｼ繝槭Φ繧ｹ逶｣隕悶ｒ譛牙柑縺ｫ縺吶ｋ縺具ｼ・tep 3.7逕ｨ・・        /// </summary>
        public static bool EnablePerformanceMonitoring
        {
            get => PlayerPrefs.GetInt("FeatureFlag_EnablePerformanceMonitoring", 1) == 1;
            set => SetFlag("FeatureFlag_EnablePerformanceMonitoring", value);
        }
// ========== Step 3.9 Legacy Singleton隴ｦ蜻翫す繧ｹ繝・Β ==========
        
        /// <summary>
        /// Legacy Singleton菴ｿ逕ｨ譎ゅ↓隴ｦ蜻翫ｒ陦ｨ遉ｺ縺吶ｋ縺・        /// </summary>
        public static bool EnableMigrationWarnings
        {
            get => PlayerPrefs.GetInt("FeatureFlag_EnableMigrationWarnings", 1) == 1;
            set => SetFlag("FeatureFlag_EnableMigrationWarnings", value);
        }
        
        /// <summary>
        /// Legacy Singleton縺ｸ縺ｮ繧｢繧ｯ繧ｻ繧ｹ繧貞ｮ悟・縺ｫ遖∵ｭ｢縺吶ｋ縺・        /// </summary>
        public static bool DisableLegacySingletons
        {
            get => PlayerPrefs.GetInt("FeatureFlag_DisableLegacySingletons", 0) == 1;
            set => SetFlag("FeatureFlag_DisableLegacySingletons", value);
        }
        
// ========== Phase 3 譁ｰ隕冗ｧｻ陦檎ｮ｡逅・ヵ繝ｩ繧ｰ ==========
        
        /// <summary>
        /// 遘ｻ陦後・繝ｭ繧ｻ繧ｹ逶｣隕悶ｒ譛牙柑縺ｫ縺吶ｋ縺・        /// </summary>
        public static bool EnableMigrationMonitoring
        {
            get => PlayerPrefs.GetInt("FeatureFlag_EnableMigrationMonitoring", 1) == 1;
            set => SetFlag("FeatureFlag_EnableMigrationMonitoring", value);
        }
        
        /// <summary>
        /// 繝代ヵ繧ｩ繝ｼ繝槭Φ繧ｹ貂ｬ螳壹ｒ譛牙柑縺ｫ縺吶ｋ縺・        /// </summary>
        public static bool EnablePerformanceMeasurement
        {
            get => PlayerPrefs.GetInt("FeatureFlag_EnablePerformanceMeasurement", 1) == 1;
            set => SetFlag("FeatureFlag_EnablePerformanceMeasurement", value);
        }
        
        /// <summary>
        /// 閾ｪ蜍輔Ο繝ｼ繝ｫ繝舌ャ繧ｯ讖溯・繧呈怏蜉ｹ縺ｫ縺吶ｋ縺・        /// </summary>
        public static bool EnableAutoRollback
        {
            get => PlayerPrefs.GetInt("FeatureFlag_EnableAutoRollback", 1) == 1;
            set => SetFlag("FeatureFlag_EnableAutoRollback", value);
        }
        
        /// <summary>
        /// Singleton縺ｮ菴ｿ逕ｨ繧定ｨｱ蜿ｯ縺吶ｋ縺具ｼ育ｷ頑･譎ゅΟ繝ｼ繝ｫ繝舌ャ繧ｯ逕ｨ・・        /// </summary>
        public static bool AllowSingletonFallback
        {
            get => PlayerPrefs.GetInt("FeatureFlag_AllowSingletonFallback", 0) == 1;
            set => SetFlag("FeatureFlag_AllowSingletonFallback", value);
        }
        
        /// <summary>
        /// 繝・せ繝医Δ繝ｼ繝峨ｒ譛牙柑縺ｫ縺吶ｋ縺・        /// </summary>
        public static bool EnableTestMode
        {
            get => PlayerPrefs.GetInt("FeatureFlag_EnableTestMode", 0) == 1;
            set => SetFlag("FeatureFlag_EnableTestMode", value);
        }
        
        // ========== 谿ｵ髫守噪遘ｻ陦悟宛蠕｡繝輔Λ繧ｰ ==========
        
        /// <summary>
        /// AudioManager 縺ｮ ServiceLocator遘ｻ陦後ｒ譛牙柑縺ｫ縺吶ｋ縺・        /// </summary>
        public static bool MigrateAudioManager
        {
            get => PlayerPrefs.GetInt("FeatureFlag_MigrateAudioManager", 0) == 1;
            set => SetFlag("FeatureFlag_MigrateAudioManager", value);
        }
        
        /// <summary>
        /// SpatialAudioManager 縺ｮ ServiceLocator遘ｻ陦後ｒ譛牙柑縺ｫ縺吶ｋ縺・        /// </summary>
        public static bool MigrateSpatialAudioManager
        {
            get => PlayerPrefs.GetInt("FeatureFlag_MigrateSpatialAudioManager", 0) == 1;
            set => SetFlag("FeatureFlag_MigrateSpatialAudioManager", value);
        }
        
        /// <summary>
        /// EffectManager 縺ｮ ServiceLocator遘ｻ陦後ｒ譛牙柑縺ｫ縺吶ｋ縺・        /// </summary>
        public static bool MigrateEffectManager
        {
            get => PlayerPrefs.GetInt("FeatureFlag_MigrateEffectManager", 0) == 1;
            set => SetFlag("FeatureFlag_MigrateEffectManager", value);
        }
        
        /// <summary>
        /// StealthAudioCoordinator 縺ｮ ServiceLocator遘ｻ陦後ｒ譛牙柑縺ｫ縺吶ｋ縺・        /// </summary>
        public static bool MigrateStealthAudioCoordinator
        {
            get => PlayerPrefs.GetInt("FeatureFlag_MigrateStealthAudioCoordinator", 0) == 1;
            set => SetFlag("FeatureFlag_MigrateStealthAudioCoordinator", value);
        }
        
        /// <summary>
        /// AudioUpdateCoordinator 縺ｮ ServiceLocator遘ｻ陦後ｒ譛牙柑縺ｫ縺吶ｋ縺・        /// </summary>
        public static bool MigrateAudioUpdateCoordinator
        {
            get => PlayerPrefs.GetInt("FeatureFlag_MigrateAudioUpdateCoordinator", 0) == 1;
            set => SetFlag("FeatureFlag_MigrateAudioUpdateCoordinator", value);
        }
        
        // ========== Phase 3 繝ｦ繝ｼ繝・ぅ繝ｪ繝・ぅ繝｡繧ｽ繝・ラ ==========
        
        /// <summary>
        /// 繝輔Λ繧ｰ縺ｮ螟画峩繧堤ｵｱ荳逧・↓邂｡逅・ｼ亥､画峩繝ｭ繧ｰ莉倥″・・        /// </summary>
        private static void SetFlag(string key, bool value)
        {
            bool oldValue = PlayerPrefs.GetInt(key, 0) == 1;
            if (oldValue != value)
            {
                PlayerPrefs.SetInt(key, value ? 1 : 0);
                if (EnableDebugLogging)
                {
                    Debug.Log($"[FeatureFlags] {key}: {oldValue} -> {value}");
                }
                
                // 螟画峩螻･豁ｴ繧定ｨ倬鹸・育ｧｻ陦檎屮隕也畑・・                if (EnableMigrationMonitoring)
                {
                    LogFlagChange(key, oldValue, value);
                }
            }
        }
        
        /// <summary>
        /// 繝輔Λ繧ｰ螟画峩螻･豁ｴ繧偵Ο繧ｰ縺ｫ險倬鹸
        /// </summary>
        private static void LogFlagChange(string flagName, bool oldValue, bool newValue)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string logEntry = $"[{timestamp}] {flagName}: {oldValue} -> {newValue}";
            
            // PlayerPrefs縺ｫ螻･豁ｴ繧定ｿｽ蜉・域怙譁ｰ10莉ｶ縺ｾ縺ｧ菫晄戟・・            List<string> history = GetFlagChangeHistory();
            history.Add(logEntry);
            
            // 譛譁ｰ10莉ｶ縺ｫ蛻ｶ髯・            if (history.Count > 10)
            {
                history.RemoveAt(0);
            }
            
            PlayerPrefs.SetString("FeatureFlag_ChangeHistory", string.Join("|", history));
        }
        
        /// <summary>
        /// 繝輔Λ繧ｰ螟画峩螻･豁ｴ繧貞叙蠕・        /// </summary>
        public static List<string> GetFlagChangeHistory()
        {
            string historyStr = PlayerPrefs.GetString("FeatureFlag_ChangeHistory", "");
            return string.IsNullOrEmpty(historyStr) ? new List<string>() : new List<string>(historyStr.Split('|'));
        }
        
        /// <summary>
        /// 谿ｵ髫守噪遘ｻ陦後・繝励Μ繧ｻ繝・ヨ險ｭ螳・        /// </summary>
        public static void SetMigrationPhase(int phase)
        {
            switch (phase)
            {
                case 0: // 繝ｪ繧ｻ繝・ヨ・亥ｮ悟・縺ｪSingleton繝｢繝ｼ繝会ｼ・                    UseServiceLocator = false;
                    UseNewAudioSystem = false;
                    UseEventDrivenAudio = false;
                    UseNewAudioUpdateSystem = false;
                    AllowSingletonFallback = true;
                    ResetAllMigrationFlags();
                    break;
                    
                case 1: // Phase 1: ServiceLocator蝓ｺ逶､貅門ｙ
                    UseServiceLocator = true;
                    UseNewAudioSystem = false;
                    UseEventDrivenAudio = false;
                    EnableMigrationMonitoring = true;
                    EnablePerformanceMeasurement = true;
                    break;
                    
                case 2: // Phase 2: AudioManager遘ｻ陦碁幕蟋・                    UseServiceLocator = true;
                    MigrateAudioManager = true;
                    UseNewAudioSystem = true;
                    EnableMigrationMonitoring = true;
                    EnablePerformanceMeasurement = true;
                    break;
                    
                case 3: // Phase 3: 蜈ｨ菴鍋ｧｻ陦悟ｮ御ｺ・                    UseServiceLocator = true;
                    UseNewAudioSystem = true;
                    UseEventDrivenAudio = true;
                    UseNewAudioUpdateSystem = true;
                    // 笨・Task1: Phase 3譁ｰ繝輔Λ繧ｰ繧定ｿｽ蜉
                    UseNewAudioService = true;
                    UseNewSpatialService = true;
                    UseNewStealthService = true;
                    EnableAllMigrationFlags();
                    AllowSingletonFallback = false;
                    break;
                    
                default:
                    Debug.LogWarning($"[FeatureFlags] Unknown migration phase: {phase}");
                    return;
            }
            
            PlayerPrefs.Save();
            Debug.Log($"[FeatureFlags] Migration phase set to: {phase}");
            LogCurrentFlags();
        }
        
        /// <summary>
        /// 縺吶∋縺ｦ縺ｮ遘ｻ陦後ヵ繝ｩ繧ｰ繧呈怏蜉ｹ蛹・        /// </summary>
        private static void EnableAllMigrationFlags()
        {
            MigrateAudioManager = true;
            MigrateSpatialAudioManager = true;
            MigrateEffectManager = true;
            MigrateStealthAudioCoordinator = true;
            MigrateAudioUpdateCoordinator = true;
        }
        
        /// <summary>
        /// 縺吶∋縺ｦ縺ｮ遘ｻ陦後ヵ繝ｩ繧ｰ繧堤┌蜉ｹ蛹・        /// </summary>
        private static void ResetAllMigrationFlags()
        {
            MigrateAudioManager = false;
            MigrateSpatialAudioManager = false;
            MigrateEffectManager = false;
            MigrateStealthAudioCoordinator = false;
            MigrateAudioUpdateCoordinator = false;
        }
        
        /// <summary>
        /// 邱頑･繝ｭ繝ｼ繝ｫ繝舌ャ繧ｯ・亥・縺ｦ繧担ingleton險ｭ螳壹↓謌ｻ縺呻ｼ・        /// </summary>
        public static void EmergencyRollback()
        {
            Debug.LogWarning("[FeatureFlags] EMERGENCY ROLLBACK - Reverting to Singleton mode");
            
            SetMigrationPhase(0); // 螳悟・繝ｪ繧ｻ繝・ヨ
            
            // 邱頑･繝ｭ繝ｼ繝ｫ繝舌ャ繧ｯ縺ｮ螻･豁ｴ繧定ｨ倬鹸
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            PlayerPrefs.SetString("FeatureFlag_LastEmergencyRollback", timestamp);
            
            PlayerPrefs.Save();
            Debug.LogError($"[FeatureFlags] Emergency rollback completed at {timestamp}");
        }
        
        /// <summary>
        /// 譛蠕後・邱頑･繝ｭ繝ｼ繝ｫ繝舌ャ繧ｯ譎ょ綾繧貞叙蠕・        /// </summary>
        public static string GetLastEmergencyRollbackTime()
        {
            return PlayerPrefs.GetString("FeatureFlag_LastEmergencyRollback", "縺ｪ縺・);
        }
        
        /// <summary>
        /// 遘ｻ陦後・螳牙・諤ｧ繝√ぉ繝・け
        /// </summary>
        public static bool IsMigrationSafe()
        {
            // ServiceLocator縺悟渕譛ｬ逧・↓蜍穂ｽ懊＠縺ｦ縺・ｋ縺薙→繧堤｢ｺ隱・            if (!UseServiceLocator) return false;
            
            // 繝代ヵ繧ｩ繝ｼ繝槭Φ繧ｹ貂ｬ螳壹′譛牙柑縺ｧ縺ゅｋ縺薙→繧堤｢ｺ隱・            if (!EnablePerformanceMeasurement) return false;
            
            // 逶｣隕悶す繧ｹ繝・Β縺梧怏蜉ｹ縺ｧ縺ゅｋ縺薙→繧堤｢ｺ隱・            if (!EnableMigrationMonitoring) return false;
            
            return true;
        }
        
        /// <summary>
        /// 迴ｾ蝨ｨ縺ｮ遘ｻ陦碁ｲ謐励ｒ蜿門ｾ暦ｼ・-100%・・        /// </summary>
        public static int GetMigrationProgress()
        {
            int completed = 0;
            int total = 5; // 遘ｻ陦悟ｯｾ雎｡縺ｮSingleton謨ｰ
            
            if (MigrateAudioManager) completed++;
            if (MigrateSpatialAudioManager) completed++;
            if (MigrateEffectManager) completed++;
            if (MigrateStealthAudioCoordinator) completed++;
            if (MigrateAudioUpdateCoordinator) completed++;
            
            return (completed * 100) / total;
        }
        
        /// <summary>
        /// 縺吶∋縺ｦ縺ｮFeature Flag繧偵Μ繧ｻ繝・ヨ
        /// </summary>
        public static void ResetAll()
        {
            // 蝓ｺ譛ｬ繝輔Λ繧ｰ縺ｮ繝ｪ繧ｻ繝・ヨ
            UseNewAudioSystem = false;
            UseServiceLocator = true;
            UseEventDrivenAudio = false;
            UseNewAudioUpdateSystem = false;
            EnableDebugLogging = true;
            
            // Phase 3 譁ｰ隕上ヵ繝ｩ繧ｰ縺ｮ繝ｪ繧ｻ繝・ヨ
            EnableMigrationMonitoring = true;
            EnablePerformanceMeasurement = true;
            EnableAutoRollback = true;
            AllowSingletonFallback = false;
            EnableTestMode = false;
            
            // 遘ｻ陦後ヵ繝ｩ繧ｰ縺ｮ繝ｪ繧ｻ繝・ヨ
            ResetAllMigrationFlags();
            
            PlayerPrefs.Save();
            
            Debug.Log("[FeatureFlags] All flags reset to default");
        }
        
        /// <summary>
        /// 迴ｾ蝨ｨ縺ｮFeature Flag險ｭ螳壹ｒ繝ｭ繧ｰ蜃ｺ蜉・        /// </summary>
        public static void LogCurrentFlags()
        {
            Debug.Log($"[FeatureFlags] === Current Settings ===");
            Debug.Log($"  蝓ｺ譛ｬ繝輔Λ繧ｰ:");
            Debug.Log($"    - UseNewAudioSystem: {UseNewAudioSystem}");
            Debug.Log($"    - UseServiceLocator: {UseServiceLocator}");
            Debug.Log($"    - UseEventDrivenAudio: {UseEventDrivenAudio}");
            Debug.Log($"    - UseNewAudioUpdateSystem: {UseNewAudioUpdateSystem}");
            Debug.Log($"    - EnableDebugLogging: {EnableDebugLogging}");
            
            Debug.Log($"  遘ｻ陦檎ｮ｡逅・ヵ繝ｩ繧ｰ:");
            Debug.Log($"    - EnableMigrationMonitoring: {EnableMigrationMonitoring}");
            Debug.Log($"    - EnablePerformanceMeasurement: {EnablePerformanceMeasurement}");
            Debug.Log($"    - EnableAutoRollback: {EnableAutoRollback}");
            Debug.Log($"    - AllowSingletonFallback: {AllowSingletonFallback}");
            Debug.Log($"    - EnableTestMode: {EnableTestMode}");
            
            Debug.Log($"  谿ｵ髫守噪遘ｻ陦後ヵ繝ｩ繧ｰ:");
            Debug.Log($"    - MigrateAudioManager: {MigrateAudioManager}");
            Debug.Log($"    - MigrateSpatialAudioManager: {MigrateSpatialAudioManager}");
            Debug.Log($"    - MigrateEffectManager: {MigrateEffectManager}");
            Debug.Log($"    - MigrateStealthAudioCoordinator: {MigrateStealthAudioCoordinator}");
            Debug.Log($"    - MigrateAudioUpdateCoordinator: {MigrateAudioUpdateCoordinator}");
            
            Debug.Log($"  遘ｻ陦碁ｲ謐・ {GetMigrationProgress()}%");
            Debug.Log($"  譛蠕後・邱頑･繝ｭ繝ｼ繝ｫ繝舌ャ繧ｯ: {GetLastEmergencyRollbackTime()}");
            Debug.Log($"  遘ｻ陦悟ｮ牙・諤ｧ: {(IsMigrationSafe() ? "OK" : "NG")}");
        }
        
        /// <summary>
        /// 繝輔Λ繧ｰ螟画峩螻･豁ｴ繧偵Ο繧ｰ蜃ｺ蜉・        /// </summary>
    
        /// <summary>
        /// 蜈ｨ縺ｦ縺ｮ繝輔Λ繧ｰ繧偵ョ繝輔か繝ｫ繝亥､縺ｫ繝ｪ繧ｻ繝・ヨ
        /// </summary>
        public static void ResetToDefaults()
        {
            SetMigrationPhase(0); // 螳悟・縺ｪSingleton繝｢繝ｼ繝峨↓繝ｪ繧ｻ繝・ヨ
            EnableMigrationMonitoring = false;
            EnablePerformanceMeasurement = false;
            EnableAutoRollback = false;
            AllowSingletonFallback = true;
            EnableTestMode = false;
            EnableDebugLogging = false;
            
            // Reset completed successfullyalues");
        }
        
        /// <summary>
        /// Task 1蟆ら畑: Phase 3繝輔Λ繧ｰ縺ｮ遒ｺ螳溘↑譛牙柑蛹・        /// </summary>
        public static void EnablePhase3Flags()
        {
            // 譌｢蟄倥・PlayerPrefs繧ｭ繝ｼ繧貞炎髯､縺励※譁ｰ縺励＞繝・ヵ繧ｩ繝ｫ繝亥､繧帝←逕ｨ
            PlayerPrefs.DeleteKey("FeatureFlag_UseNewAudioService");
            PlayerPrefs.DeleteKey("FeatureFlag_UseNewSpatialService");
            PlayerPrefs.DeleteKey("FeatureFlag_UseNewStealthService");
            
            // 譏守､ｺ逧・↓險ｭ螳夲ｼ育｢ｺ螳溘↓縺吶ｋ縺溘ａ・・            UseNewAudioService = true;
            UseNewSpatialService = true;
            UseNewStealthService = true;
            
            PlayerPrefs.Save();
            
            Debug.Log("[FeatureFlags] Phase 3 flags enabled successfully");
            LogCurrentFlags(); // 險ｭ螳夂｢ｺ隱・        }
        
        /// <summary>
        /// 險ｭ螳壹・謨ｴ蜷域ｧ繧呈､懆ｨｼ
        /// </summary>
        public static void ValidateConfiguration()
        {
            // UseServiceLocator縺掲alse縺ｪ縺ｮ縺ｫ遘ｻ陦後ヵ繝ｩ繧ｰ縺荊rue縺ｮ蝣ｴ蜷医・隴ｦ蜻・            if (!UseServiceLocator && (MigrateAudioManager || MigrateSpatialAudioManager || 
                MigrateEffectManager || MigrateAudioUpdateCoordinator || MigrateStealthAudioCoordinator))
            {
                Debug.LogWarning("[FeatureFlags] Inconsistent configuration: Migration flags are enabled but UseServiceLocator is false");
            }
            
            // 遘ｻ陦檎屮隕悶′辟｡蜉ｹ縺ｪ縺ｮ縺ｫ繝代ヵ繧ｩ繝ｼ繝槭Φ繧ｹ貂ｬ螳壹′譛牙柑縺ｮ蝣ｴ蜷医・隴ｦ蜻・            if (!EnableMigrationMonitoring && EnablePerformanceMeasurement)
            {
                Debug.LogWarning("[FeatureFlags] EnablePerformanceMeasurement requires EnableMigrationMonitoring");
            }
            
            // DisableLegacySingletons 縺ｨ AllowSingletonFallback 縺ｮ遶ｶ蜷・            if (DisableLegacySingletons && AllowSingletonFallback)
            {
                Debug.LogWarning("[FeatureFlags] Inconsistent configuration: DisableLegacySingletons=true conflicts with AllowSingletonFallback=true");
            }
            
            // Day4蜑肴署: DisableLegacySingletons縺梧怏蜉ｹ縺ｪ繧臼hase3譁ｰ繧ｵ繝ｼ繝薙せ縺ｯ蜈ｨ縺ｦ譛牙柑縺悟ｮ牙・
            if (DisableLegacySingletons && (!UseNewAudioService || !UseNewSpatialService || !UseNewStealthService))
            {
                Debug.LogWarning("[FeatureFlags] DisableLegacySingletons requires Phase 3 services enabled (UseNewAudio/Spatial/Stealth)");
            }
        }

        /// <summary>
        /// 謨ｴ蜷域ｧ繧貞ｼｷ蛻ｶ・・utoFix=true縺ｧ螳牙・蛛ｴ縺ｫ閾ｪ蜍戊｣懈ｭ｣・・        /// </summary>
        public static void EnforceConsistency(bool autoFix = false)
        {
            bool changed = false;
            
            if (!UseServiceLocator)
            {
                // 遘ｻ陦後ヵ繝ｩ繧ｰ縺ｯ辟｡蜉ｹ縺悟ｮ牙・
                if (MigrateAudioManager && autoFix) { MigrateAudioManager = false; changed = true; }
                if (MigrateSpatialAudioManager && autoFix) { MigrateSpatialAudioManager = false; changed = true; }
                if (MigrateEffectManager && autoFix) { MigrateEffectManager = false; changed = true; }
                if (MigrateStealthAudioCoordinator && autoFix) { MigrateStealthAudioCoordinator = false; changed = true; }
                if (MigrateAudioUpdateCoordinator && autoFix) { MigrateAudioUpdateCoordinator = false; changed = true; }
                
                // Legacy辟｡蜉ｹ蛹悶・蜊ｱ髯ｺ縺ｪ縺ｮ縺ｧ隗｣髯､
                if (DisableLegacySingletons && autoFix) { DisableLegacySingletons = false; changed = true; }
            }

            // 逶｣隕悶↑縺励〒險域ｸｬ縺ｯ辟｡蜉ｹ蛹・            if (!EnableMigrationMonitoring && EnablePerformanceMeasurement && autoFix)
            {
                EnablePerformanceMeasurement = false; changed = true;
            }

            // 遶ｶ蜷郁ｧ｣豸・ Legacy辟｡蜉ｹ蛹悶→Singleton繝輔か繝ｼ繝ｫ繝舌ャ繧ｯ蜷梧凾縺ｯ荳榊庄
            if (DisableLegacySingletons && AllowSingletonFallback && autoFix)
            {
                AllowSingletonFallback = false; changed = true;
            }

            // Day4螳牙・諤ｧ: Legacy繧堤┌蜉ｹ縺ｫ縺吶ｋ縺ｪ繧臼hase3譁ｰ繧ｵ繝ｼ繝薙せ繧呈怏蜉ｹ蛹・            if (DisableLegacySingletons && autoFix)
            {
                if (!UseNewAudioService) { UseNewAudioService = true; changed = true; }
                if (!UseNewSpatialService) { UseNewSpatialService = true; changed = true; }
                if (!UseNewStealthService) { UseNewStealthService = true; changed = true; }
            }

            if (changed)
            {
                PlayerPrefs.Save();
                Debug.Log("[FeatureFlags] Consistency enforced and saved");
            }
        }
        
        // ========== Task 4: DisableLegacySingletons谿ｵ髫守噪譛牙柑蛹・==========
        
        /// <summary>
        /// Task 4: Day 1 - 繝・せ繝育腸蠅・〒隴ｦ蜻翫す繧ｹ繝・Β譛牙柑蛹・        /// EnableMigrationWarnings縺ｨMigrationMonitoring繧堤｢ｺ螳溘↓譛牙柑蛹・        /// </summary>
        public static void EnableDay1TestWarnings()
        {
            PlayerPrefs.DeleteKey("FeatureFlag_EnableMigrationWarnings");
            PlayerPrefs.DeleteKey("FeatureFlag_EnableMigrationMonitoring");
            
            EnableMigrationWarnings = true;
            EnableMigrationMonitoring = true;
            EnableDebugLogging = true;
            
            PlayerPrefs.Save();
            Debug.Log("[FeatureFlags] Day 1: Test warnings enabled successfully");
            LogCurrentFlags();
        }
        
        /// <summary>
        /// Task 4: Day 1螳溯｡・- 繝・せ繝育腸蠅・〒隴ｦ蜻翫す繧ｹ繝・Β繧貞ｮ滄圀縺ｫ螳溯｡・        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void ExecuteDay1TestWarnings()
        {
            // Day 1縺ｮ螳溯｡後・荳蠎ｦ縺縺題｡後≧
            if (PlayerPrefs.GetInt("Task4_Day1_Executed", 0) == 1)
                return;
                
            EnableDay1TestWarnings();
            PlayerPrefs.SetInt("Task4_Day1_Executed", 1);
            PlayerPrefs.Save();
            
            Debug.Log("[Task 4 - Day 1] 隴ｦ蜻翫す繧ｹ繝・Β縺後ユ繧ｹ繝育腸蠅・〒譛牙柑蛹悶＆繧後∪縺励◆縲・egacy Singleton縺ｮ菴ｿ逕ｨ縺檎屮隕悶＆繧後※縺・∪縺吶・);
        }
        
        /// <summary>
        /// Task 4: Day 4 - 譛ｬ逡ｪ迺ｰ蠅・〒Singleton谿ｵ髫守噪辟｡蜉ｹ蛹・        /// DisableLegacySingletons 繧呈怏蜉ｹ蛹厄ｼ域怙邨よｮｵ髫趣ｼ・        /// </summary>
        public static void EnableDay4SingletonDisabling()
        {
            PlayerPrefs.DeleteKey("FeatureFlag_DisableLegacySingletons");
            
            DisableLegacySingletons = true;
            
            PlayerPrefs.Save();
            Debug.Log("[FeatureFlags] Day 4: Legacy Singletons disabled successfully");
            LogCurrentFlags();
            
            // MigrationValidator螳溯｡後ｒ謗ｨ螂ｨ
            Debug.Log("[FeatureFlags] RECOMMENDATION: Run MigrationValidator to verify migration completion");
        }
        
        /// <summary>
        /// Task 4: Day 4螳溯｡・- 譛ｬ逡ｪ迺ｰ蠅・〒Singleton谿ｵ髫守噪辟｡蜉ｹ蛹悶ｒ螳滄圀縺ｫ螳溯｡・        /// </summary>
        public static void ExecuteDay4SingletonDisabling()
        {
            // Day 4縺ｮ螳溯｡後・荳蠎ｦ縺縺題｡後≧
            if (PlayerPrefs.GetInt("Task4_Day4_Executed", 0) == 1)
            {
                Debug.Log("[Task 4 - Day 4] Already executed. Legacy Singletons are disabled.");
                return;
            }
            
            // 螳牙・諤ｧ繝√ぉ繝・け
            if (!IsTask4Safe())
            {
                Debug.LogError("[Task 4 - Day 4] Safety check failed. Cannot disable Legacy Singletons.");
                return;
            }
            
            EnableDay4SingletonDisabling();
            PlayerPrefs.SetInt("Task4_Day4_Executed", 1);
            PlayerPrefs.Save();
            
            Debug.Log("[Task 4 - Day 4] Legacy Singleton縺梧悽逡ｪ迺ｰ蠅・〒辟｡蜉ｹ蛹悶＆繧後∪縺励◆縲４erviceLocator螳悟・遘ｻ陦悟ｮ御ｺ・・);
            
            // 螳御ｺ・憾豕√ｒ繝ｬ繝昴・繝・            Debug.Log($"[Task 4 Complete] Migration Progress: {GetMigrationProgress()}%, Safety Status: {(IsMigrationSafe() ? "SAFE" : "UNSAFE")}");
        }
        
        /// <summary>
        /// Task 4縺ｮ螳牙・縺ｪ螳溯｡後メ繧ｧ繝・け
        /// </summary>
        public static bool IsTask4Safe()
        {
            // ServiceLocator蝓ｺ逶､縺梧紛縺｣縺ｦ縺・ｋ縺九メ繧ｧ繝・け
            if (!UseServiceLocator)
            {
                Debug.LogError("[FeatureFlags] Task 4 requires UseServiceLocator = true");
                return false;
            }
            
            // Phase 3縺ｮ譁ｰ繧ｵ繝ｼ繝薙せ縺梧怏蜉ｹ縺九メ繧ｧ繝・け  
            if (!UseNewAudioService || !UseNewSpatialService || !UseNewStealthService)
            {
                Debug.LogError("[FeatureFlags] Task 4 requires all Phase 3 services enabled");
                return false;
            }
            
            // 遘ｻ陦檎屮隕悶す繧ｹ繝・Β縺梧怏蜉ｹ縺九メ繧ｧ繝・け
            if (!EnableMigrationMonitoring)
            {
                Debug.LogError("[FeatureFlags] Task 4 requires EnableMigrationMonitoring = true");
                return false;
            }
            
            return true;
        }

        /// <summary>
        /// 襍ｷ蜍墓凾縺ｮ讒区・讀懆ｨｼ繝輔ャ繧ｯ・亥ｿ・★荳蠎ｦ螳溯｡鯉ｼ・        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        private static void RuntimeValidateOnStartup()
        {
            try
            {
                ValidateConfiguration();
                // 閾ｪ蜍穂ｿｮ豁｣縺ｯ陦後ｏ縺夊ｭｦ蜻翫・縺ｿ縲ょｿ・ｦ√↓蠢懊§縺ｦ襍ｷ蜍輔せ繧ｯ繝ｪ繝励ヨ縺九ｉEnforceConsistency(true)繧貞他縺ｳ蜃ｺ縺・                EnforceConsistency(false);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[FeatureFlags] Runtime validation failed: {e.Message}");
            }
        }
        
        /// <summary>
        /// Phase 1.2: SINGLETON_COMPLETE_REMOVAL_GUIDE.md貅匁侠 - 蛹・峡逧・ヰ繝・け繧｢繝・・菴懈・縺ｨ譛邨りｨｭ螳・        /// 螳悟・Singleton蜑企勁貅門ｙ縺ｮ縺溘ａ縺ｮFeatureFlags譛邨りｨｭ螳壹ｒ驕ｩ逕ｨ
        /// </summary>
        public static void ExecutePhase1ComprehensiveBackupAndFinalSettings()
        {
            Debug.Log("[FeatureFlags] === Phase 1.2: 蛹・峡逧・ヰ繝・け繧｢繝・・菴懈・縺ｨ譛邨りｨｭ螳壼ｮ溯｡・===");
            
            // Step 1: 迴ｾ蝨ｨ險ｭ螳壹・繝舌ャ繧ｯ繧｢繝・・菴懈・
            string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmm");
            string backupKey = $"Phase1_Backup_{timestamp}";
            
            Debug.Log($"[FeatureFlags] Step 1: Creating comprehensive backup: {backupKey}");
            
            // 迴ｾ蝨ｨ縺ｮ險ｭ螳壹ｒ繝ｭ繧ｰ蜃ｺ蜉幢ｼ医ヰ繝・け繧｢繝・・逶ｮ逧・ｼ・            LogCurrentFlags();
            
            // 繝舌ャ繧ｯ繧｢繝・・繧単layerPrefs縺ｫ菫晏ｭ・            string featureFlagsBackup = SerializeCurrentFeatureFlags();
            PlayerPrefs.SetString($"{backupKey}_FeatureFlags", featureFlagsBackup);
            PlayerPrefs.SetString($"{backupKey}_Timestamp", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            PlayerPrefs.SetString("LastPhase1Backup", backupKey);
            
            Debug.Log($"[FeatureFlags] 笨・Comprehensive backup created: {backupKey}");
            
            // Step 2: SINGLETON_COMPLETE_REMOVAL_GUIDE.md縺ｫ蠕薙▲縺滓怙邨りｨｭ螳夐←逕ｨ
            Debug.Log("[FeatureFlags] Step 2: Applying final FeatureFlags configuration for complete removal...");
            
            // 谿ｵ髫守噪譖ｴ譁ｰ・亥ｮ牙・諤ｧ遒ｺ菫晢ｼ・            Debug.Log("[FeatureFlags] Step 2.1: Disabling Legacy Singletons...");
            DisableLegacySingletons = true;
            
            Debug.Log("[FeatureFlags] Step 2.2: Disabling Migration Warnings...");
            EnableMigrationWarnings = false;
            
            Debug.Log("[FeatureFlags] Step 2.3: Disabling Migration Monitoring...");
            EnableMigrationMonitoring = false;
            
            PlayerPrefs.Save();
            
            // 譖ｴ譁ｰ蠕檎憾諷狗｢ｺ隱・            Debug.Log("[FeatureFlags] 笨・Phase 1.2 譛邨りｨｭ螳壼ｮ御ｺ・");
            Debug.Log($"  - DisableLegacySingletons: {DisableLegacySingletons}");
            Debug.Log($"  - EnableMigrationWarnings: {EnableMigrationWarnings}");
            Debug.Log($"  - EnableMigrationMonitoring: {EnableMigrationMonitoring}");
            
            Debug.Log("[FeatureFlags] === Phase 1.2 螳御ｺ・ System ready for Phase 2: Physical Code Removal ===");
        }
        
        /// <summary>
        /// Phase 1.2逕ｨ: 迴ｾ蝨ｨ縺ｮFeatureFlags繧偵す繝ｪ繧｢繝ｩ繧､繧ｺ
        /// </summary>
        private static string SerializeCurrentFeatureFlags()
        {
            return $"UseServiceLocator:{UseServiceLocator}," +
                   $"DisableLegacySingletons:{DisableLegacySingletons}," +
                   $"EnableMigrationWarnings:{EnableMigrationWarnings}," +
                   $"EnableMigrationMonitoring:{EnableMigrationMonitoring}," +
                   $"UseNewAudioService:{UseNewAudioService}," +
                   $"UseNewSpatialService:{UseNewSpatialService}," +
                   $"UseNewStealthService:{UseNewStealthService}";
        }
        
        /// <summary>
        /// Phase 1邱頑･繝ｭ繝ｼ繝ｫ繝舌ャ繧ｯ: FeatureFlags繧貞ｮ牙・縺ｪ迥ｶ諷九↓謌ｻ縺・        /// </summary>
        public static void ExecutePhase1EmergencyRollback()
        {
            Debug.LogWarning("[FeatureFlags] === EXECUTING PHASE 1 EMERGENCY ROLLBACK ===");
            
            // FeatureFlags繧貞ｮ牙・縺ｪ迥ｶ諷九↓謌ｻ縺・            DisableLegacySingletons = false;
            EnableMigrationWarnings = true;
            EnableMigrationMonitoring = true;
            
            PlayerPrefs.Save();
            
            // 譛譁ｰ繝舌ャ繧ｯ繧｢繝・・縺九ｉ蠕ｩ譌ｧ諠・ｱ繧定｡ｨ遉ｺ
            string lastBackup = PlayerPrefs.GetString("LastPhase1Backup", "");
            if (!string.IsNullOrEmpty(lastBackup))
            {
                Debug.Log($"[FeatureFlags] Backup available for restore: {lastBackup}");
                string backupData = PlayerPrefs.GetString($"{lastBackup}_FeatureFlags", "");
                Debug.Log($"[FeatureFlags] Backup data: {backupData}");
            }
            
            Debug.Log("[FeatureFlags] 笨・Phase 1 Emergency rollback completed");
            LogCurrentFlags();
        }
        
    public static void LogFlagHistory()
        {
            Debug.Log($"[FeatureFlags] === Flag Change History ===");
            var history = GetFlagChangeHistory();
            
            if (history.Count == 0)
            {
                Debug.Log($"  螻･豁ｴ縺ｪ縺・);
                return;
            }
            
            foreach (var entry in history)
            {
                Debug.Log($"  {entry}");
            }
        }
    }
}
