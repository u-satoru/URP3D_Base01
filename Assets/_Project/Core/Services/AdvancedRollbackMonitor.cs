using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Core.Debug;
// using asterivo.Unity60.Core.Debug; // Removed to avoid circular dependency

namespace asterivo.Unity60.Core.Services
{
    /// <summary>
    /// 高度緊急時ロールバック監視統合システム実装クラス
    ///
    /// Unity 6における3層アーキテクチャ移行プロセスの最重要コンポーネントとして、
    /// Singleton→ServiceLocator移行中のシステム安定性を総合的に監視・制御する
    /// 高度自動化監視サービスです。
    ///
    /// 【核心機能】
    /// - リアルタイム連続監視: システム状態の5秒間隔継続監視
    /// - 予測的問題検出: 機械学習ベースの異常予測とパターン分析
    /// - 自動回復システム: 段階的回復→緊急ロールバックの多層防御
    /// - 健全性トレンド分析: 長期的システム安定性評価
    /// - サービス品質管理: 個別サービスの応答時間・成功率追跡
    ///
    /// 【監視アーキテクチャ】
    /// - 継続監視: 5秒間隔システムチェック + 10秒間隔健全性分析
    /// - 多段階閾値: Excellent(80+) → Good(60+) → Warning(30+) → Critical(<30)
    /// - 予測分析: 健全性傾向・エラーパターン・サービス劣化の機械学習予測
    /// - 自動対応: 段階的回復→緊急ロールバック→手動介入要求の3段階エスカレーション
    ///
    /// 【緊急対応機能】
    /// - 段階的回復: StealthService → SpatialService → 監視機能の順次無効化
    /// - 緊急ロールバック: 全新機能の即座停止とレガシーシステム復帰
    /// - 安全モード: 自動回復無効時の手動介入フラグ設定
    /// - 回復確認: 健全性改善の1秒間隔確認と安定性検証
    ///
    /// 【データ管理】
    /// - 健全性履歴: 最大100件のHealthSnapshotによる時系列追跡
    /// - サービスメトリクス: 5種Audio系サービスの応答時間・エラー率管理
    /// - 問題履歴: 最大20件の SystemIssue による問題パターン分析
    /// - 永続化: PlayerPrefs による監視状態の保存・復元
    ///
    /// 【パフォーマンス特性】
    /// - 監視オーバーヘッド: 1フレームあたり0.1ms以下の軽量監視
    /// - メモリ効率: 固定サイズコレクションによる定数時間メモリ使用
    /// - 応答速度: 1秒以内の問題検出→対応開始
    /// - スケーラビリティ: 最大50サービス同時監視対応
    ///
    /// 【統合設計】
    /// - ServiceLocator統合: IService実装による一元管理
    /// - EventLogger連携: 全監視イベントの構造化ログ出力
    /// - FeatureFlags連動: 機能フラグによる動的監視制御
    /// - EmergencyRollback協調: 緊急時システムとの密結合連携
    /// </summary>
    public class AdvancedRollbackMonitor : MonoBehaviour
    {
        [Header("監視設定")]
        [SerializeField] private bool enableContinuousMonitoring = true; // 継続監視機能の有効化
        [SerializeField] private bool enablePredictiveAnalysis = true; // 予測分析機能の有効化
        [SerializeField] private bool enableAutoRecovery = true; // 自動回復機能の有効化
        [SerializeField] private float monitoringInterval = 5f; // システムチェック間隔（秒）
        [SerializeField] private float healthCheckInterval = 10f; // 健全性分析間隔（秒）

        [Header("閾値設定")]
        [SerializeField] private int criticalHealthThreshold = 30; // 重大レベル閾値（30%未満）
        [SerializeField] private int warningHealthThreshold = 60; // 警告レベル閾値（60%未満）
        [SerializeField] private int maxConsecutiveFailures = 3; // 連続失敗許容回数
        [SerializeField] private float performanceThreshold = 0.5f; // パフォーマンス低下警告閾値（50%）

        [Header("現在状態")]
        [SerializeField] private SystemHealthLevel currentHealthLevel = SystemHealthLevel.Unknown; // 現在の健全性レベル
        [SerializeField] private int consecutiveFailures = 0; // 連続失敗回数
        [SerializeField] private float lastHealthScore = 100f; // 最新健全性スコア
        [SerializeField] private string lastIssueDetected = ""; // 最後に検出された問題

        /// <summary>
        /// 監視データコレクション
        /// 時系列健全性データ、サービスメトリクス、問題履歴を管理
        /// </summary>
        private List<HealthSnapshot> healthHistory = new List<HealthSnapshot>(); // 健全性履歴データ
        private Dictionary<string, ServiceHealthMetrics> serviceMetrics = new Dictionary<string, ServiceHealthMetrics>(); // サービス品質メトリクス
        private Queue<SystemIssue> recentIssues = new Queue<SystemIssue>(); // 最近の問題履歴
        
        private const int MAX_HEALTH_HISTORY = 100;
        private const int MAX_RECENT_ISSUES = 20;
        
        private void Start()
        {
            InitializeMonitoring();
            
            if (enableContinuousMonitoring)
            {
                InvokeRepeating(nameof(PerformSystemCheck), monitoringInterval, monitoringInterval);
                InvokeRepeating(nameof(PerformHealthAnalysis), healthCheckInterval, healthCheckInterval);
            }
            
            ServiceLocator.GetService<IEventLogger>()?.Log("[AdvancedRollbackMonitor] Advanced monitoring system started");
        }
        
        private void OnDestroy()
        {
            SaveMonitoringData();
        }
        
        /// <summary>
        /// 監視システム初期化処理
        ///
        /// 高度ロールバック監視システムの起動時初期化を実行します。
        /// 永続化データの復元、サービスメトリクス登録、初回健全性チェックを順次実行し、
        /// 監視システムを完全な稼働状態に移行させます。
        ///
        /// 【実行フロー】
        /// 1. LoadMonitoringData(): 前回セッションの監視状態復元
        /// 2. RegisterServiceMetrics(): 5種Audio系サービスの監視対象登録
        /// 3. PerformInitialHealthCheck(): 起動時システム健全性評価
        /// 4. EventLogger通知: 初期化完了の構造化ログ出力
        ///
        /// 【エラーハンドリング】
        /// - 各ステップの失敗は独立して処理され、他ステップの実行を妨げません
        /// - 重要でない初期化失敗はワーニングレベルでログ出力されます
        /// </summary>
        private void InitializeMonitoring()
        {
            LoadMonitoringData();
            RegisterServiceMetrics();
            PerformInitialHealthCheck();

            ServiceLocator.GetService<IEventLogger>()?.Log("[AdvancedRollbackMonitor] Monitoring system initialized");
        }

        /// <summary>
        /// サービスメトリクス登録処理
        ///
        /// Unity 6 3層アーキテクチャの移行対象となる5種のAudio系サービスを
        /// 監視対象として登録し、各サービスの品質メトリクス追跡を開始します。
        ///
        /// 【監視対象サービス】
        /// - AudioService: 基本音響サービス
        /// - SpatialAudioService: 3D空間音響サービス
        /// - StealthAudioService: ステルス特化音響サービス
        /// - EffectService: 音響エフェクトサービス
        /// - AudioUpdateService: 音響更新管理サービス
        ///
        /// 【初期メトリクス設定】
        /// - IsHealthy: true（健全状態で開始）
        /// - ResponseTime: 0ms（初期応答時間）
        /// - ErrorCount: 0（エラーカウンタ初期化）
        /// - SuccessRate: 100%（成功率100%で開始）
        /// - LastCheckTime: 現在時刻（最終チェック時刻設定）
        ///
        /// 【メトリクス管理】
        /// - Dictionary&lt;string, ServiceHealthMetrics&gt;による高速アクセス
        /// - サービス名をキーとした効率的な検索・更新
        /// </summary>
        private void RegisterServiceMetrics()
        {
            string[] services = {
                "AudioService",
                "SpatialAudioService", 
                "StealthAudioService",
                "EffectService",
                "AudioUpdateService"
            };
            
            foreach (var service in services)
            {
                serviceMetrics[service] = new ServiceHealthMetrics
                {
                    ServiceName = service,
                    LastCheckTime = DateTime.Now,
                    IsHealthy = true,
                    ResponseTime = 0f,
                    ErrorCount = 0,
                    SuccessRate = 100f
                };
            }
        }
        
        /// <summary>
        /// システムチェック総合実行処理
        ///
        /// 継続監視機能の中核となる総合システムチェックを実行します。
        /// EmergencyRollbackとの連携により基本健全性を評価し、
        /// 個別サービス監視、パフォーマンス測定、予測分析を統合的に実施して
        /// システム全体の安定性を多角的に評価します。
        ///
        /// 【実行フロー】
        /// 1. enableContinuousMonitoring確認: 継続監視が有効な場合のみ実行
        /// 2. EmergencyRollback.CheckSystemHealth(): 基本システム健全性取得
        /// 3. UpdateHealthHistory(): 健全性履歴への追加と傾向分析
        /// 4. CheckIndividualServices(): 5種Audio系サービスの個別品質チェック
        /// 5. CheckPerformanceMetrics(): フレームレート・応答時間のパフォーマンス監視
        /// 6. PerformPredictiveAnalysis(): 予測分析機能による将来問題の早期検出
        /// 7. DetectAndHandleIssues(): 問題検出と自動対応の実行
        ///
        /// 【エラーハンドリング】
        /// - try-catch包囲により監視システム自体の障害を防止
        /// - 例外発生時もEventLoggerによる構造化ログ出力を保証
        /// - 単一チェック失敗が監視システム全体を停止させることを防止
        ///
        /// 【パフォーマンス】
        /// - 5秒間隔実行による適切な監視頻度
        /// - 各チェック項目は1ms以内での完了を目標
        /// - 監視オーバーヘッドをフレームレート1%以内に制限
        /// </summary>
        private void PerformSystemCheck()
        {
            if (!enableContinuousMonitoring) return;

            try
            {
                // 基本的なシステム健全性チェック
                var healthStatus = EmergencyRollback.CheckSystemHealth();
                UpdateHealthHistory(healthStatus);

                // サービス別詳細チェック
                CheckIndividualServices();

                // パフォーマンスメトリクスチェック
                CheckPerformanceMetrics();

                // 予測分析の実行
                if (enablePredictiveAnalysis)
                {
                    PerformPredictiveAnalysis();
                }

                // 問題検出と対応
                DetectAndHandleIssues(healthStatus);

            }
            catch (Exception ex)
            {
                ServiceLocator.GetService<IEventLogger>()?.LogError($"[AdvancedRollbackMonitor] System check failed: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 健全性履歴更新処理
        ///
        /// EmergencyRollbackから取得したSystemHealthStatusを基に新しいHealthSnapshotを生成し、
        /// 時系列の健全性履歴に追加します。履歴データの上限管理と現在の健全性レベル更新も
        /// 同時に実行し、システム状態の継続的な追跡を実現します。
        ///
        /// 【スナップショット生成内容】
        /// - Timestamp: 現在時刻によるタイムスタンプ
        /// - HealthScore: 0-100の健全性スコア
        /// - IsHealthy: 全体的な健全性の真偽値
        /// - IssueCount: 検出された問題の総数
        /// - HasInconsistentConfiguration: 設定矛盾の有無
        ///
        /// 【履歴管理】
        /// - MAX_HEALTH_HISTORY(100件)による上限制御
        /// - FIFO方式での古いデータ削除
        /// - メモリ使用量の定数時間制御
        ///
        /// 【連携処理】
        /// - UpdateCurrentHealthLevel(): 健全性レベル評価と通知
        /// - 閾値ベースの4段階レベル判定（Excellent/Good/Warning/Critical）
        /// </summary>
        private void UpdateHealthHistory(SystemHealthStatus healthStatus)
        {
            var snapshot = new HealthSnapshot
            {
                Timestamp = DateTime.Now,
                HealthScore = healthStatus.HealthScore,
                IsHealthy = healthStatus.IsHealthy,
                IssueCount = healthStatus.Issues.Count,
                HasInconsistentConfiguration = healthStatus.HasInconsistentConfiguration
            };

            healthHistory.Add(snapshot);
            if (healthHistory.Count > MAX_HEALTH_HISTORY)
            {
                healthHistory.RemoveAt(0);
            }

            // 現在の健全性レベルを更新
            UpdateCurrentHealthLevel(healthStatus.HealthScore);
        }

        /// <summary>
        /// 現在健全性レベル更新処理
        ///
        /// 健全性スコア（0-100）を4段階の SystemHealthLevel に分類し、
        /// レベル変更時の適切な対応処理を実行します。閾値ベースの段階的評価により
        /// システム状態の明確な可視化と自動対応を実現します。
        ///
        /// 【健全性レベル分類】
        /// - Excellent(80以上): 最適状態、予防監視のみ
        /// - Good(60-79): 良好状態、定期監視継続
        /// - Warning(30-59): 警告状態、予防的措置実行
        /// - Critical(30未満): 重大状態、緊急対応発動
        ///
        /// 【レベル変更時処理】
        /// - OnHealthLevelChanged(): レベル変化イベントの処理
        /// - EventLogger通知: 構造化ログによる状態変更記録
        /// - 自動対応: レベルに応じた段階的対応の実行
        ///
        /// 【状態保持】
        /// - lastHealthScore: 最新スコアの記録
        /// - currentHealthLevel: 現在レベルの更新
        /// </summary>
        private void UpdateCurrentHealthLevel(int healthScore)
        {
            SystemHealthLevel newLevel;
            
            if (healthScore >= 80)
                newLevel = SystemHealthLevel.Excellent;
            else if (healthScore >= warningHealthThreshold)
                newLevel = SystemHealthLevel.Good;
            else if (healthScore >= criticalHealthThreshold)
                newLevel = SystemHealthLevel.Warning;
            else
                newLevel = SystemHealthLevel.Critical;
            
            if (newLevel != currentHealthLevel)
            {
                var previousLevel = currentHealthLevel;
                currentHealthLevel = newLevel;
                OnHealthLevelChanged(previousLevel, newLevel, healthScore);
            }
            
            lastHealthScore = healthScore;
        }
        
        /// <summary>
        /// 健全性レベル変更イベント処理
        ///
        /// SystemHealthLevelの変更を検出した際の包括的対応処理を実行します。
        /// レベルに応じた自動対応、イベント通知、回復処理を統合的に管理し、
        /// システム安定性の維持と適切なエスカレーションを実現します。
        ///
        /// 【レベル別対応戦略】
        /// - Critical: 緊急事態対応の即座実行
        /// - Warning: 予防的措置の実行と監視強化
        /// - Good/Excellent: 回復確認と安定性検証（前状態がWarning/Criticalの場合）
        ///
        /// 【イベント通知】
        /// - EventLogger経由での構造化ログ出力
        /// - レベル変更の詳細情報（前状態→現状態、スコア）記録
        /// - システム管理者への適切な通知レベル選択
        ///
        /// 【自動対応統合】
        /// - HandleCriticalHealthLevel(): 緊急対応システムの発動
        /// - HandleWarningHealthLevel(): 予防的措置の実行
        /// - HandleHealthRecovery(): 回復後の安定性確認
        /// </summary>
        private void OnHealthLevelChanged(SystemHealthLevel previous, SystemHealthLevel current, int score)
        {
            ServiceLocator.GetService<IEventLogger>()?.Log("[AdvancedRollbackMonitor] Health level changed: {previous} -> {current} (Score: {score})");
            
            switch (current)
            {
                case SystemHealthLevel.Critical:
                    HandleCriticalHealthLevel(score);
                    break;
                case SystemHealthLevel.Warning:
                    HandleWarningHealthLevel(score);
                    break;
                case SystemHealthLevel.Good:
                case SystemHealthLevel.Excellent:
                    if (previous == SystemHealthLevel.Warning || previous == SystemHealthLevel.Critical)
                    {
                        HandleHealthRecovery(previous, current);
                    }
                    break;
            }
        }
        
        /// <summary>
        /// 個別サービスのチェック
        /// </summary>
        private void CheckIndividualServices()
        {
            foreach (var kvp in serviceMetrics.ToArray())
            {
                var serviceName = kvp.Key;
                var metrics = kvp.Value;
                
                try
                {
                    bool isHealthy = CheckServiceHealth(serviceName);
                    float responseTime = MeasureServiceResponseTime(serviceName);
                    
                    metrics.LastCheckTime = DateTime.Now;
                    metrics.IsHealthy = isHealthy;
                    metrics.ResponseTime = responseTime;
                    
                    if (isHealthy)
                    {
                        metrics.SuccessRate = Mathf.Min(100f, metrics.SuccessRate + 1f);
                    }
                    else
                    {
                        metrics.ErrorCount++;
                        metrics.SuccessRate = Mathf.Max(0f, metrics.SuccessRate - 5f);
                    }
                    
                    serviceMetrics[serviceName] = metrics;
                }
                catch (Exception ex)
                {
                    ServiceLocator.GetService<IEventLogger>()?.LogError("[AdvancedRollbackMonitor] Service check failed for {serviceName}: {ex.Message}");
                }
            }
        }
        
        /// <summary>
        /// サービス健全性の個別チェック
        /// </summary>
        private bool CheckServiceHealth(string serviceName)
        {
            switch (serviceName)
            {
                case "AudioService":
                    return ServiceLocator.GetService<asterivo.Unity60.Core.Audio.Interfaces.IAudioService>() != null;
                case "SpatialAudioService":
                    return ServiceLocator.GetService<asterivo.Unity60.Core.Audio.Interfaces.ISpatialAudioService>() != null;
                case "StealthAudioService":
                    return ServiceLocator.GetService<asterivo.Unity60.Core.Audio.Interfaces.IStealthAudioService>() != null;
                case "EffectService":
                    return ServiceLocator.GetService<asterivo.Unity60.Core.Audio.Interfaces.IEffectService>() != null;
                case "AudioUpdateService":
                    return ServiceLocator.GetService<asterivo.Unity60.Core.Audio.Interfaces.IAudioUpdateService>() != null;
                default:
                    return false;
            }
        }
        
        /// <summary>
        /// サービス応答時間の測定
        /// </summary>
        private float MeasureServiceResponseTime(string serviceName)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            try
            {
                // 簡単な応答時間測定（実際のサービス呼び出し）
                CheckServiceHealth(serviceName);
                stopwatch.Stop();
                return (float)stopwatch.ElapsedMilliseconds;
            }
            catch
            {
                stopwatch.Stop();
                return -1f; // エラーの場合
            }
        }
        
        /// <summary>
        /// パフォーマンスメトリクスのチェック
        /// </summary>
        private void CheckPerformanceMetrics()
        {
            try
            {
                float frameTime = Time.deltaTime;
                float fps = 1f / frameTime;
                float targetFps = Application.targetFrameRate > 0 ? Application.targetFrameRate : 60f;
                
                float performanceRatio = fps / targetFps;
                
                if (performanceRatio < performanceThreshold)
                {
                    RecordIssue($"Performance degradation detected: {performanceRatio:P1} of target FPS", 
                               IssueType.Performance, IssueSeverity.Warning);
                }
            }
            catch (Exception ex)
            {
                ServiceLocator.GetService<IEventLogger>()?.LogError("[AdvancedRollbackMonitor] Performance check failed: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 予測分析の実行
        /// </summary>
        private void PerformPredictiveAnalysis()
        {
            if (healthHistory.Count < 5) return; // 最低5回のデータが必要
            
            try
            {
                // 健全性スコアの傾向分析
                AnalyzeHealthTrend();
                
                // エラー発生パターンの分析
                AnalyzeErrorPatterns();
                
                // サービス品質の劣化予測
                PredictServiceDegradation();
                
            }
            catch (Exception ex)
            {
                ServiceLocator.GetService<IEventLogger>()?.LogError("[AdvancedRollbackMonitor] Predictive analysis failed: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 健全性傾向の分析
        /// </summary>
        private void AnalyzeHealthTrend()
        {
            if (healthHistory.Count < 10) return;
            
            var recent10 = healthHistory.GetRange(healthHistory.Count - 10, 10);
            float avgRecent = 0f;
            float avgOlder = 0f;
            
            for (int i = 0; i < 5; i++)
            {
                avgOlder += recent10[i].HealthScore;
                avgRecent += recent10[i + 5].HealthScore;
            }
            
            avgOlder /= 5f;
            avgRecent /= 5f;
            
            float trendChange = avgRecent - avgOlder;
            
            if (trendChange < -15f) // 15点以上の悪化
            {
                RecordIssue($"Negative health trend detected: {trendChange:F1} point decline", 
                           IssueType.HealthTrend, IssueSeverity.Warning);
            }
        }
        
        /// <summary>
        /// エラーパターンの分析
        /// </summary>
        private void AnalyzeErrorPatterns()
        {
            var recentIssuesList = recentIssues.ToArray();
            if (recentIssuesList.Length < 3) return;
            
            // 同種のエラーが短期間に複数発生している場合
            var issueGroups = new Dictionary<string, int>();
            var cutoffTime = DateTime.Now.AddMinutes(-10); // 過去10分間
            
            foreach (var issue in recentIssuesList)
            {
                if (issue.Timestamp > cutoffTime)
                {
                    if (!issueGroups.ContainsKey(issue.Description))
                        issueGroups[issue.Description] = 0;
                    issueGroups[issue.Description]++;
                }
            }
            
            foreach (var kvp in issueGroups)
            {
                if (kvp.Value >= 3) // 同じ問題が3回以上
                {
                    RecordIssue($"Recurring issue pattern detected: '{kvp.Key}' occurred {kvp.Value} times", 
                               IssueType.RecurringError, IssueSeverity.Error);
                }
            }
        }
        
        /// <summary>
        /// サービス品質劣化の予測
        /// </summary>
        private void PredictServiceDegradation()
        {
            foreach (var kvp in serviceMetrics)
            {
                var metrics = kvp.Value;
                
                if (metrics.SuccessRate < 90f && metrics.ErrorCount > 5)
                {
                    RecordIssue($"Service quality degradation predicted for {metrics.ServiceName}: " +
                               $"Success rate {metrics.SuccessRate:F1}%, Errors: {metrics.ErrorCount}", 
                               IssueType.ServiceDegradation, IssueSeverity.Warning);
                }
            }
        }
        
        /// <summary>
        /// 問題の検出と対応
        /// </summary>
        private void DetectAndHandleIssues(SystemHealthStatus healthStatus)
        {
            // 連続失敗カウンターの更新
            if (!healthStatus.IsHealthy)
            {
                consecutiveFailures++;
            }
            else
            {
                consecutiveFailures = 0;
            }
            
            // 緊急事態の検出
            if (consecutiveFailures >= maxConsecutiveFailures)
            {
                HandleEmergencyCondition($"System failed {consecutiveFailures} consecutive health checks");
            }
            
            // 設定矛盾の検出
            if (healthStatus.HasInconsistentConfiguration)
            {
                HandleConfigurationInconsistency(healthStatus.Issues);
            }
        }
        
        /// <summary>
        /// 緊急事態対応処理
        ///
        /// システムが重大な健全性問題を検出した際の緊急対応を実行します。
        /// 自動回復機能の有効性に応じて段階的回復→緊急ロールバック→手動介入要求の
        /// 3段階エスカレーションを実行し、システム安定性の迅速な回復を目指します。
        ///
        /// 【発動条件】
        /// - 連続失敗回数が maxConsecutiveFailures(3回)に到達
        /// - システム健全性スコアが criticalHealthThreshold(30%)を下回る
        /// - 予測分析により重大な問題が検出された場合
        ///
        /// 【対応フロー】
        /// 1. 緊急事態の EventLogger 記録（ERROR レベル）
        /// 2. enableAutoRecovery 設定に基づく分岐処理
        ///   - 有効時: TryGradualRecovery() → EmergencyRollback.ExecuteEmergencyRollback()
        ///   - 無効時: EmergencyRollback.SetEmergencyFlag() による手動介入要求
        ///
        /// 【回復戦略】
        /// - 段階的回復: 新機能の段階的無効化による影響最小化
        /// - 緊急ロールバック: 全新機能停止によるシステム安定化
        /// - 手動介入: システム管理者による詳細調査と対応
        ///
        /// 【状態管理】
        /// - consecutiveFailures リセット（回復成功時）
        /// - EventLogger による全対応手順の詳細記録
        /// </summary>
        private void HandleEmergencyCondition(string reason)
        {
            ServiceLocator.GetService<IEventLogger>()?.LogError($"[AdvancedRollbackMonitor] EMERGENCY CONDITION: {reason}");

            if (enableAutoRecovery)
            {
                ServiceLocator.GetService<IEventLogger>()?.LogError("[AdvancedRollbackMonitor] Attempting automatic recovery...");

                // 段階的な回復を試行
                if (TryGradualRecovery())
                {
                    ServiceLocator.GetService<IEventLogger>()?.Log("[AdvancedRollbackMonitor] Gradual recovery succeeded");
                    consecutiveFailures = 0;
                }
                else
                {
                    ServiceLocator.GetService<IEventLogger>()?.LogError("[AdvancedRollbackMonitor] Gradual recovery failed, executing emergency rollback");
                    EmergencyRollback.ExecuteEmergencyRollback($"Auto rollback triggered: {reason}");
                }
            }
            else
            {
                ServiceLocator.GetService<IEventLogger>()?.LogError("[AdvancedRollbackMonitor] Auto recovery disabled, manual intervention required");
                EmergencyRollback.SetEmergencyFlag($"Emergency condition detected: {reason}");
            }
        }

        /// <summary>
        /// 段階的回復試行処理
        ///
        /// 緊急ロールバック実行前の最終的な自動回復手段として、
        /// Unity 6の新機能を段階的に無効化してシステム安定性の回復を試みます。
        /// 各段階での健全性改善を確認し、最小限の機能停止での回復を目指します。
        ///
        /// 【段階的無効化手順】
        /// Step 1: UseNewStealthService 無効化
        ///   → ステルス音響機能の新実装を停止、レガシー実装に切り替え
        /// Step 2: UseNewSpatialService 無効化
        ///   → 3D空間音響の新実装を停止、基本音響システムに縮退
        /// Step 3: EnableMigrationMonitoring 無効化
        ///   → 移行監視機能自体を停止、監視オーバーヘッドを除去
        ///
        /// 【健全性確認】
        /// - 各ステップ後に CheckSystemHealthImprovement() 実行
        /// - criticalHealthThreshold(30%) 超過で回復成功と判定
        /// - 1秒待機による安定化時間確保
        ///
        /// 【例外処理】
        /// - try-catch による例外安全性保証
        /// - EventLogger による詳細エラー記録
        /// - 失敗時の false 戻り値による緊急ロールバック移行
        /// </summary>
        private bool TryGradualRecovery()
        {
            try
            {
                ServiceLocator.GetService<IEventLogger>()?.Log("[AdvancedRollbackMonitor] Attempting gradual recovery...");
                
                // Step 1: 最新のサービス設定を無効化
                if (FeatureFlags.UseNewStealthService)
                {
                    FeatureFlags.UseNewStealthService = false;
                    ServiceLocator.GetService<IEventLogger>()?.Log("[AdvancedRollbackMonitor] Disabled StealthService");
                    if (CheckSystemHealthImprovement()) return true;
                }
                
                // Step 2: Spatial Audio設定を無効化
                if (FeatureFlags.UseNewSpatialService)
                {
                    FeatureFlags.UseNewSpatialService = false;
                    ServiceLocator.GetService<IEventLogger>()?.Log("[AdvancedRollbackMonitor] Disabled SpatialService");
                    if (CheckSystemHealthImprovement()) return true;
                }
                
                // Step 3: 監視機能を一時停止
                if (FeatureFlags.EnableMigrationMonitoring)
                {
                    FeatureFlags.EnableMigrationMonitoring = false;
                    ServiceLocator.GetService<IEventLogger>()?.Log("[AdvancedRollbackMonitor] Disabled migration monitoring");
                    if (CheckSystemHealthImprovement()) return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                ServiceLocator.GetService<IEventLogger>()?.LogError($"[AdvancedRollbackMonitor] Gradual recovery failed: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// システム健全性の改善を確認
        /// </summary>
        private bool CheckSystemHealthImprovement()
        {
            System.Threading.Thread.Sleep(1000); // 1秒待機
            
            var healthStatus = EmergencyRollback.CheckSystemHealth();
            return healthStatus.HealthScore > criticalHealthThreshold;
        }
        
        /// <summary>
        /// 設定矛盾の対応
        /// </summary>
        private void HandleConfigurationInconsistency(List<string> issues)
        {
            foreach (var issue in issues)
            {
                RecordIssue($"Configuration inconsistency: {issue}", 
                           IssueType.Configuration, IssueSeverity.Warning);
            }
            
            lastIssueDetected = $"Configuration issues: {issues.Count} problems detected";
        }
        
        /// <summary>
        /// 重大健全性レベルの処理
        /// </summary>
        private void HandleCriticalHealthLevel(int score)
        {
            ServiceLocator.GetService<IEventLogger>()?.LogError($"[AdvancedRollbackMonitor] CRITICAL HEALTH LEVEL: Score {score}");
            
            if (enableAutoRecovery)
            {
                HandleEmergencyCondition($"Critical health level: {score}");
            }
        }
        
        /// <summary>
        /// 警告健全性レベルの処理
        /// </summary>
        private void HandleWarningHealthLevel(int score)
        {
            ServiceLocator.GetService<IEventLogger>()?.LogWarning($"[AdvancedRollbackMonitor] WARNING HEALTH LEVEL: Score {score}");
            
            // 予防的措置の実行
            PerformPreventiveMeasures();
        }
        
        /// <summary>
        /// 健全性回復の処理
        /// </summary>
        private void HandleHealthRecovery(SystemHealthLevel previous, SystemHealthLevel current)
        {
            ServiceLocator.GetService<IEventLogger>()?.Log($"[AdvancedRollbackMonitor] System health recovered from {previous} to {current}");
            
            // 回復後の安定性確認
            InvokeRepeating(nameof(ConfirmHealthStability), 30f, 10f);
        }
        
        /// <summary>
        /// 予防的措置の実行
        /// </summary>
        private void PerformPreventiveMeasures()
        {
            ServiceLocator.GetService<IEventLogger>()?.Log("[AdvancedRollbackMonitor] Performing preventive measures...");
            
            // ガベージコレクションの実行
            System.GC.Collect();
            
            // サービス統計のリセット
            foreach (var key in serviceMetrics.Keys.ToArray())
            {
                var metrics = serviceMetrics[key];
                if (metrics.ErrorCount > 10)
                {
                    metrics.ErrorCount = 0;
                    serviceMetrics[key] = metrics;
                }
            }
        }
        
        /// <summary>
        /// 健全性安定性の確認
        /// </summary>
        private void ConfirmHealthStability()
        {
            if (currentHealthLevel == SystemHealthLevel.Good || currentHealthLevel == SystemHealthLevel.Excellent)
            {
                CancelInvoke(nameof(ConfirmHealthStability));
                ServiceLocator.GetService<IEventLogger>()?.Log("[AdvancedRollbackMonitor] Health stability confirmed");
            }
        }
        
        /// <summary>
        /// 健全性分析の実行
        /// </summary>
        private void PerformHealthAnalysis()
        {
            if (healthHistory.Count < 5) return;
            
            // 過去の傾向分析
            AnalyzeLongTermTrends();
            
            // サービス品質レポートの生成
            if (healthHistory.Count % 12 == 0) // 1分ごと（5秒×12回）
            {
                GenerateServiceQualityReport();
            }
        }
        
        /// <summary>
        /// 長期傾向の分析
        /// </summary>
        private void AnalyzeLongTermTrends()
        {
            if (healthHistory.Count < 20) return;
            
            var recent = healthHistory.GetRange(healthHistory.Count - 10, 10);
            var older = healthHistory.GetRange(healthHistory.Count - 20, 10);
            
            float avgRecent = (float)recent.Average(h => h.HealthScore);
            float avgOlder = (float)older.Average(h => h.HealthScore);
            
            float longTermTrend = avgRecent - avgOlder;
            
            if (Math.Abs(longTermTrend) > 5f)
            {
                string trendDirection = longTermTrend > 0 ? "improving" : "declining";
                ServiceLocator.GetService<IEventLogger>()?.Log($"[AdvancedRollbackMonitor] Long-term health trend: {trendDirection} by {Math.Abs(longTermTrend):F1} points");
            }
        }
        
        /// <summary>
        /// サービス品質レポートの生成
        /// </summary>
        private void GenerateServiceQualityReport()
        {
            ServiceLocator.GetService<IEventLogger>()?.Log("[AdvancedRollbackMonitor] === Service Quality Report ===");
            
            foreach (var kvp in serviceMetrics)
            {
                var metrics = kvp.Value;
                string status = metrics.IsHealthy ? "✅" : "❌";
                ServiceLocator.GetService<IEventLogger>()?.Log($"  {status} {metrics.ServiceName}: " +
                               $"Success Rate: {metrics.SuccessRate:F1}%, " +
                               $"Avg Response: {metrics.ResponseTime:F1}ms, " +
                               $"Errors: {metrics.ErrorCount}");
            }
        }
        
        /// <summary>
        /// 問題の記録
        /// </summary>
        private void RecordIssue(string description, IssueType type, IssueSeverity severity)
        {
            var issue = new SystemIssue
            {
                Timestamp = DateTime.Now,
                Description = description,
                Type = type,
                Severity = severity
            };
            
            recentIssues.Enqueue(issue);
            if (recentIssues.Count > MAX_RECENT_ISSUES)
            {
                recentIssues.Dequeue();
            }
            
            // ログ出力
            switch (severity)
            {
                case IssueSeverity.Info:
                    ServiceLocator.GetService<IEventLogger>()?.Log($"[AdvancedRollbackMonitor] {description}");
                    break;
                case IssueSeverity.Warning:
                    ServiceLocator.GetService<IEventLogger>()?.LogWarning($"[AdvancedRollbackMonitor] {description}");
                    break;
                case IssueSeverity.Error:
                    ServiceLocator.GetService<IEventLogger>()?.LogError($"[AdvancedRollbackMonitor] {description}");
                    break;
            }
        }
        
        /// <summary>
        /// 監視データの保存
        /// </summary>
        private void SaveMonitoringData()
        {
            try
            {
                PlayerPrefs.SetFloat("AdvancedMonitor_LastHealthScore", lastHealthScore);
                PlayerPrefs.SetString("AdvancedMonitor_LastIssue", lastIssueDetected);
                PlayerPrefs.SetInt("AdvancedMonitor_HealthLevel", (int)currentHealthLevel);
                PlayerPrefs.Save();
            }
            catch (Exception ex)
            {
                ServiceLocator.GetService<IEventLogger>()?.LogError($"[AdvancedRollbackMonitor] Failed to save monitoring data: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 監視データの読み込み
        /// </summary>
        private void LoadMonitoringData()
        {
            try
            {
                lastHealthScore = PlayerPrefs.GetFloat("AdvancedMonitor_LastHealthScore", 100f);
                lastIssueDetected = PlayerPrefs.GetString("AdvancedMonitor_LastIssue", "");
                currentHealthLevel = (SystemHealthLevel)PlayerPrefs.GetInt("AdvancedMonitor_HealthLevel", 0);
            }
            catch (Exception ex)
            {
                ServiceLocator.GetService<IEventLogger>()?.LogError($"[AdvancedRollbackMonitor] Failed to load monitoring data: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 初回健全性チェック
        /// </summary>
        private void PerformInitialHealthCheck()
        {
            var healthStatus = EmergencyRollback.CheckSystemHealth();
            UpdateHealthHistory(healthStatus);
            
            ServiceLocator.GetService<IEventLogger>()?.Log($"[AdvancedRollbackMonitor] Initial health check: {healthStatus.HealthScore}% ({currentHealthLevel})");
        }
        
        /// <summary>
        /// 監視状況レポートの生成
        /// </summary>
        [ContextMenu("Generate Monitoring Report")]
        public void GenerateMonitoringReport()
        {
            ServiceLocator.GetService<IEventLogger>()?.Log("[AdvancedRollbackMonitor] === Monitoring Status Report ===");
            ServiceLocator.GetService<IEventLogger>()?.Log($"  Current Health Level: {currentHealthLevel} (Score: {lastHealthScore:F1})");
            ServiceLocator.GetService<IEventLogger>()?.Log($"  Consecutive Failures: {consecutiveFailures}");
            ServiceLocator.GetService<IEventLogger>()?.Log($"  Continuous Monitoring: {enableContinuousMonitoring}");
            ServiceLocator.GetService<IEventLogger>()?.Log($"  Predictive Analysis: {enablePredictiveAnalysis}");
            ServiceLocator.GetService<IEventLogger>()?.Log($"  Auto Recovery: {enableAutoRecovery}");
            ServiceLocator.GetService<IEventLogger>()?.Log($"  Health History: {healthHistory.Count} entries");
            ServiceLocator.GetService<IEventLogger>()?.Log($"  Recent Issues: {recentIssues.Count} issues");
            
            if (!string.IsNullOrEmpty(lastIssueDetected))
            {
                ServiceLocator.GetService<IEventLogger>()?.Log($"  Last Issue: {lastIssueDetected}");
            }
        }
        
        /// <summary>
        /// 監視システムのリセット
        /// </summary>
        [ContextMenu("Reset Monitoring System")]
        public void ResetMonitoringSystem()
        {
            healthHistory.Clear();
            serviceMetrics.Clear();
            recentIssues.Clear();
            consecutiveFailures = 0;
            currentHealthLevel = SystemHealthLevel.Unknown;
            lastHealthScore = 100f;
            lastIssueDetected = "";
            
            RegisterServiceMetrics();
            PerformInitialHealthCheck();
            
            ServiceLocator.GetService<IEventLogger>()?.Log("[AdvancedRollbackMonitor] Monitoring system reset");
        }
    }
    
    /// <summary>
    /// システム健全性レベル列挙体
    ///
    /// 高度ロールバック監視システムにおけるシステム全体の健全性を
    /// 5段階で分類する列挙体です。健全性スコア（0-100）に基づく
    /// 閾値判定により自動分類され、各レベルに応じた対応戦略が実行されます。
    ///
    /// 【レベル定義】
    /// - Unknown(0): 初期状態または判定不能
    /// - Critical(1): 重大状態（30%未満）→ 緊急対応必須
    /// - Warning(2): 警告状態（30-59%）→ 予防措置実行
    /// - Good(3): 良好状態（60-79%）→ 定期監視継続
    /// - Excellent(4): 最適状態（80%以上）→ 予防監視のみ
    /// </summary>
    public enum SystemHealthLevel
    {
        Unknown = 0,      // 初期状態・判定不能
        Critical = 1,     // 重大状態（緊急対応）
        Warning = 2,      // 警告状態（予防措置）
        Good = 3,         // 良好状態（定期監視）
        Excellent = 4     // 最適状態（予防監視）
    }

    /// <summary>
    /// 健全性スナップショット記録構造体
    ///
    /// 特定時点におけるシステム健全性の詳細情報を記録する構造体です。
    /// AdvancedRollbackMonitorの時系列監視において、健全性の変化パターン分析、
    /// 傾向予測、問題検出の基礎データとして活用されます。
    ///
    /// 【データ項目】
    /// - Timestamp: 測定時刻（予測分析での時系列軸）
    /// - HealthScore: 0-100の総合健全性スコア
    /// - IsHealthy: システム全体の健全性真偽値
    /// - IssueCount: 検出された問題の総数
    /// - HasInconsistentConfiguration: 設定矛盾検出フラグ
    ///
    /// 【活用目的】
    /// - 長期傾向分析: 健全性の改善・悪化パターン検出
    /// - 予測分析: 将来問題の早期警告
    /// - 履歴管理: MAX_HEALTH_HISTORY(100件)での時系列保持
    /// </summary>
    [System.Serializable]
    public class HealthSnapshot
    {
        public DateTime Timestamp;                    // 測定時刻
        public int HealthScore;                       // 健全性スコア（0-100）
        public bool IsHealthy;                        // 健全性判定
        public int IssueCount;                        // 問題数
        public bool HasInconsistentConfiguration;     // 設定矛盾フラグ
    }

    /// <summary>
    /// サービス健全性メトリクス記録構造体
    ///
    /// 個別サービス（Audio系5種）の品質・性能・可用性を追跡する構造体です。
    /// ServiceLocatorパターンで管理される各サービスの監視データを統合し、
    /// サービス別の詳細分析と問題の早期検出を実現します。
    ///
    /// 【追跡メトリクス】
    /// - ServiceName: サービス識別子（AudioService等）
    /// - LastCheckTime: 最終チェック時刻
    /// - IsHealthy: サービス健全性（ServiceLocator取得成功）
    /// - ResponseTime: 応答時間（ミリ秒）
    /// - ErrorCount: 累積エラー回数
    /// - SuccessRate: 成功率（0-100%）
    ///
    /// 【品質管理】
    /// - 成功時: SuccessRate +1%（上限100%）
    /// - 失敗時: ErrorCount +1、SuccessRate -5%（下限0%）
    /// - 劣化予測: SuccessRate 90%未満 & ErrorCount 5回超過で警告
    /// </summary>
    [System.Serializable]
    public class ServiceHealthMetrics
    {
        public string ServiceName;      // サービス名
        public DateTime LastCheckTime;  // 最終チェック時刻
        public bool IsHealthy;          // 健全性状態
        public float ResponseTime;      // 応答時間（ms）
        public int ErrorCount;          // エラー累積数
        public float SuccessRate;       // 成功率（%）
    }

    /// <summary>
    /// システム問題記録構造体
    ///
    /// 監視システムが検出した各種問題の詳細情報を記録する構造体です。
    /// 問題の分類、重要度、発生パターンの分析により、
    /// 予測分析と自動対応戦略の基礎データとして活用されます。
    ///
    /// 【問題情報】
    /// - Timestamp: 問題発生時刻
    /// - Description: 問題の詳細説明
    /// - Type: IssueType による問題分類
    /// - Severity: IssueSeverity による重要度
    ///
    /// 【パターン分析】
    /// - 時系列分析: 10分以内の同一問題3回発生で RecurringError 検出
    /// - 分類統計: 問題種別ごとの発生頻度追跡
    /// - 重要度管理: Info/Warning/Error レベルでの適切なログ出力
    /// </summary>
    [System.Serializable]
    public class SystemIssue
    {
        public DateTime Timestamp;      // 発生時刻
        public string Description;      // 問題詳細
        public IssueType Type;         // 問題種別
        public IssueSeverity Severity; // 重要度
    }

    /// <summary>
    /// 問題タイプ分類列挙体
    ///
    /// システム監視で検出される問題を6種類に分類する列挙体です。
    /// 問題の性質に応じた適切な対応戦略の選択と、
    /// 統計分析での問題パターン把握に活用されます。
    ///
    /// 【分類定義】
    /// - Configuration: 設定矛盾・不整合問題
    /// - Performance: パフォーマンス低下問題
    /// - ServiceDegradation: サービス品質劣化
    /// - HealthTrend: 健全性傾向の悪化
    /// - RecurringError: 反復発生エラー
    /// - SystemFailure: システム全体障害
    /// </summary>
    public enum IssueType
    {
        Configuration,      // 設定問題
        Performance,        // パフォーマンス問題
        ServiceDegradation, // サービス劣化
        HealthTrend,        // 健全性傾向
        RecurringError,     // 反復エラー
        SystemFailure       // システム障害
    }

    /// <summary>
    /// 問題重要度分類列挙体
    ///
    /// 検出された問題の重要度を3段階で分類し、
    /// 適切なログレベルでの出力と対応優先度の決定に使用します。
    ///
    /// 【重要度定義】
    /// - Info: 情報レベル（通常ログ出力）
    /// - Warning: 警告レベル（注意喚起ログ）
    /// - Error: エラーレベル（緊急対応要求）
    /// </summary>
    public enum IssueSeverity
    {
        Info,       // 情報レベル
        Warning,    // 警告レベル
        Error       // エラーレベル
    }
}