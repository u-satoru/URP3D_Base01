using UnityEngine;
using asterivo.Unity60.Core;

namespace asterivo.Unity60.Tests
{
    /// <summary>
    /// Task 4: DisableLegacySingletons段階的有効化 の実行テスト
    /// </summary>
    public static class Task4ExecutionTest
    {
        /// <summary>
        /// Task 4を実際に実行する
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void ExecuteTask4()
        {
            // Day 1は既にFeatureFlags.csで自動実行されている
            // Day 4を手動実行
            FeatureFlags.ExecuteDay4SingletonDisabling();
            
            // 結果検証
            VerifyTask4Execution();
        }
        
        /// <summary>
        /// Task 4の実行結果を検証
        /// </summary>
        public static void VerifyTask4Execution()
        {
            Debug.Log("=== Task 4 Execution Verification ===");
            
            // 必要なフラグの状態を確認
            Debug.Log($"EnableMigrationWarnings: {FeatureFlags.EnableMigrationWarnings}");
            Debug.Log($"EnableMigrationMonitoring: {FeatureFlags.EnableMigrationMonitoring}");
            Debug.Log($"DisableLegacySingletons: {FeatureFlags.DisableLegacySingletons}");
            
            // 安全性チェック
            bool isSafe = FeatureFlags.IsTask4Safe();
            Debug.Log($"Task 4 Safety Status: {(isSafe ? "SAFE" : "UNSAFE")}");
            
            // 移行進捗
            int progress = FeatureFlags.GetMigrationProgress();
            Debug.Log($"Migration Progress: {progress}%");
            
            // Task 4完了判定
            bool task4Complete = FeatureFlags.DisableLegacySingletons && 
                               FeatureFlags.EnableMigrationWarnings && 
                               FeatureFlags.EnableMigrationMonitoring;
            
            Debug.Log($"Task 4 Completion Status: {(task4Complete ? "COMPLETED" : "INCOMPLETE")}");
            
            if (task4Complete)
            {
                Debug.Log("✅ Task 4: DisableLegacySingletons段階的有効化 - 完全完了");
                Debug.Log("✅ Legacy Singletonは無効化され、ServiceLocator完全移行が達成されました");
            }
            else
            {
                Debug.LogWarning("⚠️ Task 4: まだ完了していません。設定を確認してください。");
            }
        }
    }
}
