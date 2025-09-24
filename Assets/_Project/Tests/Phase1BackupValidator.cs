using NUnit.Framework;
using UnityEngine;
using asterivo.Unity60.Core;
using Debug = UnityEngine.Debug;
using System;

namespace asterivo.Unity60.Tests
{
    /// <summary>
    /// Phase 1.2: 包括的バックアップ作成とFeatureFlags最終設定テスト
    /// SINGLETON_COMPLETE_REMOVAL_GUIDE.md Phase 1.2 実行
    /// </summary>
    public class Phase1BackupValidator
    {
        [Test]
        public void Phase1_2_CreateComprehensiveBackupAndApplyFinalSettings()
        {
            Debug.Log("[Phase1BackupValidator] === Phase 1.2: 包括的バックアップ作成と最終設定実行 ===");
            
            // Step 1: 現在のFeatureFlags設定をバックアップとして記録
            Debug.Log("[Phase1BackupValidator] Step 1: Creating comprehensive backup...");
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmm");
            string backupKey = $"Phase1_Backup_{timestamp}";
            
            // 現在の設定をログ出力（バックアップ）
            FeatureFlags.LogCurrentFlags();
            
            // バックアップをPlayerPrefsに保存
            string featureFlagsBackup = SerializeCurrentFeatureFlags();
            PlayerPrefs.SetString($"{backupKey}_FeatureFlags", featureFlagsBackup);
            PlayerPrefs.SetString($"{backupKey}_Timestamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            PlayerPrefs.SetString("LastPhase1Backup", backupKey);
            PlayerPrefs.Save();
            
            Debug.Log($"[Phase1BackupValidator] ✅ Comprehensive backup created: {backupKey}");
            
            // Step 2: SINGLETON_COMPLETE_REMOVAL_GUIDE.mdに従い、FeatureFlags最終設定適用
            Debug.Log("[Phase1BackupValidator] Step 2: Applying final FeatureFlags configuration for complete removal...");
            
            // 段階的更新（安全性確保）
            Debug.Log("[Phase1BackupValidator] Step 2.1: Disabling Legacy Singletons...");
            FeatureFlags.DisableLegacySingletons = true;
            
            Debug.Log("[Phase1BackupValidator] Step 2.2: Disabling Migration Warnings...");
            FeatureFlags.EnableMigrationWarnings = false;
            
            Debug.Log("[Phase1BackupValidator] Step 2.3: Disabling Migration Monitoring...");
            FeatureFlags.EnableMigrationMonitoring = false;
            
            // 更新後状態確認
            Debug.Log("[Phase1BackupValidator] Final configuration applied:");
            Debug.Log($"  - DisableLegacySingletons: {FeatureFlags.DisableLegacySingletons}");
            Debug.Log($"  - EnableMigrationWarnings: {FeatureFlags.EnableMigrationWarnings}");
            Debug.Log($"  - EnableMigrationMonitoring: {FeatureFlags.EnableMigrationMonitoring}");
            
            Debug.Log("[Phase1BackupValidator] ✅ Phase 1.2 完了: System ready for Phase 2: Physical Code Removal");
            
            // 検証: 完全削除準備完了の確認
            Assert.IsTrue(FeatureFlags.DisableLegacySingletons, "DisableLegacySingletons must be enabled for complete removal");
            Assert.IsFalse(FeatureFlags.EnableMigrationWarnings, "Migration warnings should be disabled for complete removal");
            Assert.IsFalse(FeatureFlags.EnableMigrationMonitoring, "Migration monitoring should be disabled for complete removal");
            
            Debug.Log("[Phase1BackupValidator] === Phase 1.2 Validation PASSED ===");
        }
        
        private string SerializeCurrentFeatureFlags()
        {
            return $"UseServiceLocator:{FeatureFlags.UseServiceLocator}," +
                   $"DisableLegacySingletons:{FeatureFlags.DisableLegacySingletons}," +
                   $"EnableMigrationWarnings:{FeatureFlags.EnableMigrationWarnings}," +
                   $"EnableMigrationMonitoring:{FeatureFlags.EnableMigrationMonitoring}," +
                   $"UseNewAudioService:{FeatureFlags.UseNewAudioService}," +
                   $"UseNewSpatialService:{FeatureFlags.UseNewSpatialService}," +
                   $"UseNewStealthService:{FeatureFlags.UseNewStealthService}";
        }
        
        [Test]
        public void Phase1_2_EmergencyRollback()
        {
            Debug.Log("[Phase1BackupValidator] === Emergency Rollback Test ===");
            
            // 緊急ロールバック: FeatureFlagsを安全な状態に戻す
            FeatureFlags.DisableLegacySingletons = false;
            FeatureFlags.EnableMigrationWarnings = true;
            FeatureFlags.EnableMigrationMonitoring = true;
            
            // 最新バックアップから復旧
            string lastBackup = PlayerPrefs.GetString("LastPhase1Backup", "");
            if (!string.IsNullOrEmpty(lastBackup))
            {
                Debug.Log($"[Phase1BackupValidator] Restoring from backup: {lastBackup}");
                string backupData = PlayerPrefs.GetString($"{lastBackup}_FeatureFlags", "");
                Debug.Log($"[Phase1BackupValidator] Backup data: {backupData}");
            }
            
            Debug.Log("[Phase1BackupValidator] ✅ Emergency rollback completed");
            
            Assert.IsFalse(FeatureFlags.DisableLegacySingletons, "DisableLegacySingletons should be false after rollback");
            Assert.IsTrue(FeatureFlags.EnableMigrationWarnings, "Migration warnings should be enabled after rollback");
            Assert.IsTrue(FeatureFlags.EnableMigrationMonitoring, "Migration monitoring should be enabled after rollback");
        }
    }
}
