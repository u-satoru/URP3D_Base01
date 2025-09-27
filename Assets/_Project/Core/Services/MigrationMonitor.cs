using System;
using System.Collections.Generic;
using UnityEngine;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Debug;
using asterivo.Unity60.Core.Services;
// using asterivo.Unity60.Core.Debug; // Removed to avoid circular dependency

namespace asterivo.Unity60.Core.Services
{
    /// <summary>
    /// Singleton→ServiceLocator移行監視統合サービス実装クラス
    ///
    /// Unity 6における3層アーキテクチャ移行プロセスの中核として、
    /// Legacy Singletonパターンの使用状況を詳細に監視・追跡し、
    /// ServiceLocatorパターンへの安全な移行を支援する専門監視サービスです。
    /// リアルタイム監視、統計収集、安全性評価、移行推奨機能を統合提供します。
    ///
    /// 【核心機能】
    /// - Singleton使用監視: Legacy Singletonアクセスのリアルタイム検出・記録
    /// - ServiceLocator監視: 新パターン使用の肯定的追跡
    /// - 移行進捗測定: Phase 1-3の段階的進捗の定量的評価
    /// - 安全性評価: 重要サービス登録状況と使用量に基づく安全性判定
    /// - 統計レポート: 詳細な使用パターン分析と移行推奨事項生成
    /// - 永続化機能: PlayerPrefsによる統計データの永続保存
    ///
    /// 【アーキテクチャ移行支援】
    /// - Phase 1監視: DisableLegacySingletons フラグ状態追跡
    /// - Phase 2監視: UseServiceLocator フラグと実際の使用状況追跡
    /// - Phase 3監視: Legacy使用量減少とServiceLocator普及度測定
    /// - 安全性確保: 移行中の重要サービス可用性保証
    ///
    /// 【監視対象パターン】
    /// - Legacy Singleton: AudioManager.Instance, GameManager.Instance等
    /// - ServiceLocator: ServiceLocator.GetService<T>()パターン
    /// - 重要サービス: AudioService, SpatialAudioService, CommandPoolService等
    ///
    /// 【データ収集・分析】
    /// - 使用統計: アクセス回数、頻度、使用パターンの詳細記録
    /// - イベント履歴: 最新100件のアクセスイベント保持
    /// - 時系列分析: 初回・最終アクセス時刻による使用期間追跡
    /// - スタックトレース: デバッグ用の呼び出し元特定情報
    ///
    /// 【移行品質保証】
    /// - 進捗可視化: 0.0-1.0スケールでの移行完了度表示
    /// - 安全性評価: 重要サービス80%登録 + Legacy使用50回未満基準
    /// - 推奨事項生成: 使用量に基づく優先度付き移行計画提案
    /// - リアルタイム警告: EnableMigrationWarnings時の即座警告
    ///
    /// 【開発支援機能】
    /// - Context Menu: Unity Editor右クリック메뉴による即座レポート生成
    /// - リアルタイムログ: 開発中の移行状況即座確認
    /// - 統計リセット: 移行テスト時の統計クリーンアップ
    /// - 永続化統計: セッション跨ぎでの長期移行追跡
    /// </summary>
    public class MigrationMonitor : MonoBehaviour
    {
        [Header("Monitoring Settings")]
        [SerializeField] private bool enableRealTimeLogging = true;
        [SerializeField] private bool enableUsageTracking = true;
        [SerializeField] private float reportingInterval = 30f; // 30秒ごとにレポート

        [Header("Usage Statistics")]
        [SerializeField] private int totalSingletonAccesses = 0;
        [SerializeField] private int uniqueSingletonClasses = 0;
        
        // 使用状況の記録
        private Dictionary<Type, SingletonUsageInfo> usageStats = new Dictionary<Type, SingletonUsageInfo>();
        private List<SingletonUsageEvent> recentEvents = new List<SingletonUsageEvent>();
        private List<ServiceLocatorUsageEvent> recentServiceLocatorEvents = new List<ServiceLocatorUsageEvent>();
        private const int MAX_RECENT_EVENTS = 100;
        
        private void Start()
        {
            if (enableUsageTracking && reportingInterval > 0)
            {
                InvokeRepeating(nameof(GenerateUsageReport), reportingInterval, reportingInterval);
            }
            
            ServiceLocator.GetService<IEventLogger>()?.Log("[MigrationMonitor] Started monitoring singleton usage");
        }
        
        private void OnDestroy()
        {
            if (enableUsageTracking)
            {
                GenerateUsageReport();
                SaveUsageStatistics();
            }
        }
        
        /// <summary>
        /// Singleton使用を記録する
        /// </summary>
        /// <param name="singletonType">使用されたSingletonの型</param>
        /// <param name="accessMethod">アクセス方法 (例: "AudioManager.Instance")</param>
        public void LogSingletonUsage(Type singletonType, string accessMethod)
        {
            if (!enableUsageTracking) return;
            
            totalSingletonAccesses++;
            
            // 統計情報を更新
            if (!usageStats.ContainsKey(singletonType))
            {
                usageStats[singletonType] = new SingletonUsageInfo
                {
                    SingletonType = singletonType,
                    AccessMethod = accessMethod,
                    FirstAccessTime = DateTime.Now,
                    AccessCount = 0
                };
                uniqueSingletonClasses++;
            }
            
            var info = usageStats[singletonType];
            info.AccessCount++;
            info.LastAccessTime = DateTime.Now;
            
            // 最近のイベントを記録
            var usageEvent = new SingletonUsageEvent
            {
                Timestamp = DateTime.Now,
                SingletonType = singletonType.Name,
                AccessMethod = accessMethod,
                StackTrace = enableRealTimeLogging ? Environment.StackTrace : null
            };
            
            recentEvents.Add(usageEvent);
            if (recentEvents.Count > MAX_RECENT_EVENTS)
            {
                recentEvents.RemoveAt(0);
            }
            
            // リアルタイムログ出力
            if (enableRealTimeLogging)
            {
                if (FeatureFlags.EnableMigrationWarnings)
                {
                    ServiceLocator.GetService<IEventLogger>()?.LogWarning($"[MigrationMonitor] Singleton access detected: {singletonType.Name}.{accessMethod} (Total: {info.AccessCount})");
                }
                else
                {
                    ServiceLocator.GetService<IEventLogger>()?.Log($"[MigrationMonitor] Singleton access: {singletonType.Name}.{accessMethod}");
                }
            }
        }

        /// <summary>
        /// ServiceLocator使用を記録する
        /// </summary>
        /// <param name="serviceType">使用されたサービスの型</param>
        /// <param name="accessMethod">アクセス方法 (例: "ServiceLocator.GetService<IAudioService>()")</param>
        public void LogServiceLocatorUsage(Type serviceType, string accessMethod)
        {
            if (!enableUsageTracking) return;
            
            // ServiceLocator使用の記録（ポジティブな指標として扱う）
            
            // リアルタイムログ出力
            if (enableRealTimeLogging)
            {
                ServiceLocator.GetService<IEventLogger>()?.Log($"[MigrationMonitor] ServiceLocator usage: {serviceType.Name} via {accessMethod}");
            }
            
            // ServiceLocator使用イベントを記録
            var usageEvent = new ServiceLocatorUsageEvent
            {
                Timestamp = DateTime.Now,
                ServiceType = serviceType.Name,
                AccessMethod = accessMethod
            };
            
            // 最近のイベントに追加（ServiceLocatorイベント用のリストがあればそこに、なければ既存のリストに追加）
            if (recentServiceLocatorEvents == null)
            {
                recentServiceLocatorEvents = new List<ServiceLocatorUsageEvent>();
            }
            
            recentServiceLocatorEvents.Add(usageEvent);
            if (recentServiceLocatorEvents.Count > MAX_RECENT_EVENTS)
            {
                recentServiceLocatorEvents.RemoveAt(0);
            }
        }

        
        /// <summary>
        /// 現在の使用状況レポートを生成
        /// </summary>
        [ContextMenu("Generate Usage Report")]
        public void GenerateUsageReport()
        {
            if (usageStats.Count == 0)
            {
                ServiceLocator.GetService<IEventLogger>()?.Log("[MigrationMonitor] No singleton usage detected");
                return;
            }
            
            ServiceLocator.GetService<IEventLogger>()?.Log("[MigrationMonitor] === Singleton Usage Report ===");
            ServiceLocator.GetService<IEventLogger>()?.Log($"  Total Accesses: {totalSingletonAccesses}");
            ServiceLocator.GetService<IEventLogger>()?.Log($"  Unique Classes: {uniqueSingletonClasses}");
            ServiceLocator.GetService<IEventLogger>()?.Log($"  Monitoring Period: {Time.time:F1} seconds");
            
            ServiceLocator.GetService<IEventLogger>()?.Log("  Usage Details:");
            foreach (var kvp in usageStats)
            {
                var info = kvp.Value;
                var duration = info.LastAccessTime - info.FirstAccessTime;
                ServiceLocator.GetService<IEventLogger>()?.Log($"    - {info.SingletonType.Name}: {info.AccessCount} accesses " +
                               $"(First: {info.FirstAccessTime:HH:mm:ss}, Last: {info.LastAccessTime:HH:mm:ss}, " +
                               $"Duration: {duration.TotalSeconds:F1}s)");
            }
        }
        
        /// <summary>
        /// 最近の使用イベントを表示
        /// </summary>
        [ContextMenu("Show Recent Events")]
        public void ShowRecentEvents()
        {
            if (recentEvents.Count == 0)
            {
                ServiceLocator.GetService<IEventLogger>()?.Log("[MigrationMonitor] No recent singleton events");
                return;
            }
            
            ServiceLocator.GetService<IEventLogger>()?.Log("[MigrationMonitor] === Recent Singleton Events ===");
            int displayCount = Mathf.Min(recentEvents.Count, 10); // 最新10件を表示
            
            for (int i = recentEvents.Count - displayCount; i < recentEvents.Count; i++)
            {
                var evt = recentEvents[i];
                ServiceLocator.GetService<IEventLogger>()?.Log($"  [{evt.Timestamp:HH:mm:ss}] {evt.SingletonType}.{evt.AccessMethod}");
            }
        }
        
        /// <summary>
        /// 使用統計をPlayerPrefsに保存
        /// </summary>
        public void SaveUsageStatistics()
        {
            try
            {
                PlayerPrefs.SetInt("MigrationMonitor_TotalAccesses", totalSingletonAccesses);
                PlayerPrefs.SetInt("MigrationMonitor_UniqueClasses", uniqueSingletonClasses);
                PlayerPrefs.SetString("MigrationMonitor_LastReportTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                
                // 各Singletonの使用回数を保存
                foreach (var kvp in usageStats)
                {
                    string key = $"MigrationMonitor_Usage_{kvp.Key.Name}";
                    PlayerPrefs.SetInt(key, kvp.Value.AccessCount);
                }
                
                PlayerPrefs.Save();
                ServiceLocator.GetService<IEventLogger>()?.Log("[MigrationMonitor] Usage statistics saved");
            }
            catch (System.Exception ex)
            {
                ServiceLocator.GetService<IEventLogger>()?.LogError($"[MigrationMonitor] Failed to save statistics: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 保存された統計を読み込み
        /// </summary>
        public void LoadUsageStatistics()
        {
            try
            {
                totalSingletonAccesses = PlayerPrefs.GetInt("MigrationMonitor_TotalAccesses", 0);
                uniqueSingletonClasses = PlayerPrefs.GetInt("MigrationMonitor_UniqueClasses", 0);
                string lastReportTime = PlayerPrefs.GetString("MigrationMonitor_LastReportTime", "Never");
                
                ServiceLocator.GetService<IEventLogger>()?.Log($"[MigrationMonitor] Loaded statistics - Total: {totalSingletonAccesses}, " +
                               $"Unique: {uniqueSingletonClasses}, Last Report: {lastReportTime}");
            }
            catch (System.Exception ex)
            {
                ServiceLocator.GetService<IEventLogger>()?.LogError($"[MigrationMonitor] Failed to load statistics: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 統計をリセット
        /// </summary>
        [ContextMenu("Reset Statistics")]
        public void ResetStatistics()
        {
            usageStats.Clear();
            recentEvents.Clear();
            totalSingletonAccesses = 0;
            uniqueSingletonClasses = 0;
            
            // PlayerPrefsからも削除
            PlayerPrefs.DeleteKey("MigrationMonitor_TotalAccesses");
            PlayerPrefs.DeleteKey("MigrationMonitor_UniqueClasses");
            PlayerPrefs.DeleteKey("MigrationMonitor_LastReportTime");
            
            ServiceLocator.GetService<IEventLogger>()?.Log("[MigrationMonitor] Statistics reset");
        }
        
        /// <summary>
        /// 移行の推奨事項を生成
        /// </summary>
        [ContextMenu("Generate Migration Recommendations")]
        public void GenerateMigrationRecommendations()
        {
            ServiceLocator.GetService<IEventLogger>()?.Log("[MigrationMonitor] === Migration Recommendations ===");
            
            if (usageStats.Count == 0)
            {
                ServiceLocator.GetService<IEventLogger>()?.Log("  ✅ No singleton usage detected - migration appears complete!");
                return;
            }
            
            foreach (var kvp in usageStats)
            {
                var info = kvp.Value;
                string recommendation = GetMigrationRecommendation(info);
                ServiceLocator.GetService<IEventLogger>()?.Log($"  📋 {info.SingletonType.Name}: {recommendation}");
            }
        }
        
                
        /// <summary>
        /// 移行進捗を0.0-1.0の範囲で取得
        /// ServiceLocator使用率と Legacy Singleton無効化状態から算出
        /// </summary>
        /// <returns>移行進捗 (0.0 = 未開始, 1.0 = 完了)</returns>
        /// <summary>
        /// 移行進捗を0.0-1.0の範囲で取得
        /// ServiceLocator使用率と Legacy Singleton無効化状態から算出
        /// </summary>
        /// <returns>移行進捗 (0.0 = 未開始, 1.0 = 完了)</returns>
        /// <summary>
        /// 移行進捗を0.0-1.0の範囲で取得
        /// ServiceLocator使用率と Legacy Singleton無効化状態から算出
        /// </summary>
        /// <returns>移行進捗 (0.0 = 未開始, 1.0 = 完了)</returns>
        public float GetMigrationProgress()
        {
            // Phase 1: Legacy Singleton無効化チェック
            float phase1Progress = FeatureFlags.DisableLegacySingletons ? 0.6f : 0.0f;
            
            // Phase 2: ServiceLocator使用チェック
            float phase2Progress = FeatureFlags.UseServiceLocator ? 0.3f : 0.0f;
            
            // Phase 3: Legacy使用状況チェック（使用量が少ないほど進捗が高い）
            float phase3Progress = 0.0f;
            if (totalSingletonAccesses == 0)
            {
                // Legacy使用なし = 完了
                phase3Progress = 0.1f;
            }
            else if (totalSingletonAccesses < 10)
            {
                // 低使用量
                phase3Progress = 0.05f;
            }
            // 高使用量の場合はphase3Progress = 0.0f
            
            float totalProgress = phase1Progress + phase2Progress + phase3Progress;
            
            // デバッグログ出力
            if (enableRealTimeLogging)
            {
                ServiceLocator.GetService<IEventLogger>()?.Log($"[MigrationMonitor] Migration Progress: {totalProgress:P1} " +
                               $"(Phase1: {phase1Progress:P1}, Phase2: {phase2Progress:P1}, Phase3: {phase3Progress:P1})");
            }
            
            return totalProgress;
        }

        /// <summary>
        /// 移行の安全性を判定する
        /// 重要なサービスの登録状態とLegacy Singleton使用状況を総合的に評価
        /// </summary>
        /// <returns>true=安全, false=危険, null=判定不能</returns>
        public bool? IsMigrationSafe()
        {
            try
            {
                // 1. ServiceLocatorの基本動作確認
                if (!FeatureFlags.UseServiceLocator)
                {
                    if (enableRealTimeLogging)
                        ServiceLocator.GetService<IEventLogger>()?.LogWarning("[MigrationMonitor] ServiceLocator is disabled - migration safety uncertain");
                    return null; // ServiceLocatorが無効の場合は判定不能
                }
                
                // 2. 重要なサービスの登録状態チェック
                int criticalServicesCount = 0;
                int registeredServicesCount = 0;
                
                // 重要サービスのチェック
                var audioService = ServiceLocator.GetService<asterivo.Unity60.Core.Audio.Interfaces.IAudioService>();
                if (audioService != null) registeredServicesCount++;
                criticalServicesCount++;
                
                var spatialService = ServiceLocator.GetService<asterivo.Unity60.Core.Audio.Interfaces.ISpatialAudioService>();
                if (spatialService != null) registeredServicesCount++;
                criticalServicesCount++;
                
                var effectService = ServiceLocator.GetService<asterivo.Unity60.Core.Audio.Interfaces.IEffectService>();
                if (effectService != null) registeredServicesCount++;
                criticalServicesCount++;
                
                var commandService = ServiceLocator.GetService<asterivo.Unity60.Core.Commands.ICommandPoolService>();
                if (commandService != null) registeredServicesCount++;
                criticalServicesCount++;
                
                var eventLogger = ServiceLocator.GetService<asterivo.Unity60.Core.Debug.IEventLogger>();
                if (eventLogger != null) registeredServicesCount++;
                criticalServicesCount++;
                
                // サービス登録率を算出
                float serviceRegistrationRatio = criticalServicesCount > 0 ? 
                    (float)registeredServicesCount / criticalServicesCount : 0f;
                
                // 3. Legacy Singleton使用量チェック
                bool legacySingletonUsageAcceptable = totalSingletonAccesses < 50; // 50回未満なら安全範囲
                
                // 4. 総合判定
                bool isServicesSafe = serviceRegistrationRatio >= 0.8f; // 80%以上のサービスが登録済み
                bool isLegacyUsageSafe = legacySingletonUsageAcceptable;
                bool isFeatureFlagsSafe = FeatureFlags.UseServiceLocator; // ServiceLocatorが有効
                
                bool overallSafety = isServicesSafe && isLegacyUsageSafe && isFeatureFlagsSafe;
                
                // デバッグ情報出力
                if (enableRealTimeLogging)
                {
                    ServiceLocator.GetService<IEventLogger>()?.Log($"[MigrationMonitor] Safety Assessment:");
                    ServiceLocator.GetService<IEventLogger>()?.Log($"  Services: {registeredServicesCount}/{criticalServicesCount} ({serviceRegistrationRatio:P1}) - {(isServicesSafe ? "安全" : "危険")}");
                    ServiceLocator.GetService<IEventLogger>()?.Log($"  Legacy Usage: {totalSingletonAccesses} accesses - {(isLegacyUsageSafe ? "安全" : "危険")}");
                    ServiceLocator.GetService<IEventLogger>()?.Log($"  FeatureFlags: ServiceLocator={FeatureFlags.UseServiceLocator} - {(isFeatureFlagsSafe ? "安全" : "危険")}");
                    ServiceLocator.GetService<IEventLogger>()?.Log($"  Overall Safety: {(overallSafety ? "✅ SAFE" : "⚠️ UNSAFE")}");
                }
                
                return overallSafety;
            }
            catch (System.Exception ex)
            {
                if (enableRealTimeLogging)
                    ServiceLocator.GetService<IEventLogger>()?.LogError($"[MigrationMonitor] Safety assessment failed: {ex.Message}");
                return null; // 例外発生時は判定不能
            }
        }

        
        /// <summary>
        /// Singleton使用統計を取得
        /// </summary>
        /// <returns>Singleton使用統計のディクショナリ</returns>
        public Dictionary<Type, SingletonUsageInfo> GetSingletonUsageStats()
        {
            return new Dictionary<Type, SingletonUsageInfo>(usageStats);
        }
        
        /// <summary>
        /// ServiceLocator使用統計を取得
        /// </summary>
        /// <returns>ServiceLocator使用イベントのリスト</returns>
        public List<ServiceLocatorUsageEvent> GetServiceLocatorUsageStats()
        {
            return new List<ServiceLocatorUsageEvent>(recentServiceLocatorEvents ?? new List<ServiceLocatorUsageEvent>());
        }
/// <summary>
        /// 簡易版の安全性チェック (コンテキストメニュー用)
        /// </summary>
        [ContextMenu("Check Migration Safety")]
        public void CheckMigrationSafety()
        {
            var safetyResult = IsMigrationSafe();
            
            if (safetyResult == null)
            {
                ServiceLocator.GetService<IEventLogger>()?.LogWarning("[MigrationMonitor] ⚠️ Migration safety assessment inconclusive");
            }
            else if (safetyResult.Value)
            {
                ServiceLocator.GetService<IEventLogger>()?.Log("[MigrationMonitor] ✅ Migration is SAFE to proceed");
            }
            else
            {
                ServiceLocator.GetService<IEventLogger>()?.LogError("[MigrationMonitor] ⚠️ Migration is UNSAFE - review issues before proceeding");
            }
        }


/// <summary>
        /// 特定のSingletonに対する移行推奨事項を取得
        /// </summary>
        private string GetMigrationRecommendation(SingletonUsageInfo info)
        {
            if (info.AccessCount > 50)
            {
                return "❗ High usage detected - Priority migration recommended";
            }
            else if (info.AccessCount > 10)
            {
                return "⚠️  Medium usage - Schedule migration soon";
            }
            else
            {
                return "💡 Low usage - Can be migrated when convenient";
            }
        }
    }
    
    /// <summary>
    /// Singleton使用統計情報データ構造体
    ///
    /// 特定のSingletonクラスの使用統計を詳細に記録するデータコンテナ。
    /// MigrationMonitorによる長期的な使用パターン分析と移行計画策定に使用されます。
    ///
    /// 【統計データ要素】
    /// - SingletonType: 監視対象Singletonクラスの型情報
    /// - AccessMethod: アクセスパターン（例: "Instance", "GetInstance()"）
    /// - FirstAccessTime: 初回検出時刻（監視開始基準点）
    /// - LastAccessTime: 最終アクセス時刻（最新活動指標）
    /// - AccessCount: 累積アクセス回数（使用頻度指標）
    ///
    /// 【分析用途】
    /// - 移行優先度決定: AccessCountによる高使用Singleton特定
    /// - 使用期間分析: First～Last時刻による活用期間測定
    /// - パターン分析: AccessMethodによる使用方法の分類
    /// - 進捗追跡: 時系列でのアクセス減少傾向監視
    /// </summary>
    [System.Serializable]
    public class SingletonUsageInfo
    {
        /// <summary>監視対象Singletonクラスの型情報</summary>
        public Type SingletonType;
        /// <summary>アクセス方法パターン（例: "Instance", "GetInstance()"）</summary>
        public string AccessMethod;
        /// <summary>初回検出時刻（監視開始基準点）</summary>
        public DateTime FirstAccessTime;
        /// <summary>最終アクセス時刻（最新活動指標）</summary>
        public DateTime LastAccessTime;
        /// <summary>累積アクセス回数（使用頻度指標）</summary>
        public int AccessCount;
    }

    /// <summary>
    /// Singleton使用イベント記録データ構造体
    ///
    /// 個別のSingletonアクセスイベントを詳細に記録するデータコンテナ。
    /// リアルタイム監視とデバッグ支援のため、最新100件のイベント履歴を保持します。
    ///
    /// 【イベントデータ要素】
    /// - Timestamp: イベント発生時刻（精密なタイムライン分析用）
    /// - SingletonType: アクセスされたSingletonクラス名
    /// - AccessMethod: 具体的なアクセス方法（メソッド名、プロパティ名）
    /// - StackTrace: 呼び出し元トレース（デバッグ時のみ記録）
    ///
    /// 【活用場面】
    /// - リアルタイム監視: 即座のLegacy使用検出と警告
    /// - デバッグ支援: StackTraceによる使用箇所特定
    /// - パターン分析: 時系列でのアクセスパターン把握
    /// - 移行検証: 変更後のLegacy使用消失確認
    /// </summary>
    [System.Serializable]
    public class SingletonUsageEvent
    {
        /// <summary>イベント発生時刻（精密なタイムライン分析用）</summary>
        public DateTime Timestamp;
        /// <summary>アクセスされたSingletonクラス名</summary>
        public string SingletonType;
        /// <summary>具体的なアクセス方法（メソッド名、プロパティ名）</summary>
        public string AccessMethod;
        /// <summary>呼び出し元トレース（デバッグ時のみ記録）</summary>
        public string StackTrace;
    }

    /// <summary>
    /// ServiceLocator使用イベント記録データ構造体
    ///
    /// ServiceLocatorパターンの使用を肯定的に追跡するデータコンテナ。
    /// 移行の成功指標として、新パターンの普及状況を定量的に測定します。
    ///
    /// 【イベントデータ要素】
    /// - Timestamp: ServiceLocatorアクセス時刻
    /// - ServiceType: 取得されたサービスインターフェース名
    /// - AccessMethod: ServiceLocator使用パターン（通常"GetService<T>()"）
    ///
    /// 【移行指標としての活用】
    /// - 成功測定: ServiceLocator使用増加による移行成功確認
    /// - パターン分析: 各サービスの使用頻度と普及度測定
    /// - 安全性確認: 重要サービスの正常アクセス継続確認
    /// - 移行完了判定: Legacy使用ゼロ + ServiceLocator使用継続
    /// </summary>
    [System.Serializable]
    public class ServiceLocatorUsageEvent
    {
        /// <summary>ServiceLocatorアクセス時刻</summary>
        public DateTime Timestamp;
        /// <summary>取得されたサービスインターフェース名</summary>
        public string ServiceType;
        /// <summary>ServiceLocator使用パターン（通常"GetService<T>()"）</summary>
        public string AccessMethod;
    }
}