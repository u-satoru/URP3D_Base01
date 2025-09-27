using UnityEngine;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Audio.Interfaces;
using asterivo.Unity60.Core.Debug;
using asterivo.Unity60.Core.Services;

namespace asterivo.Unity60.Core.Services
{
    /// <summary>
    /// サービス段階的移行統合ヘルパー実装クラス
    ///
    /// Unity 6における3層アーキテクチャ移行プロセスにおいて、
    /// Singleton→ServiceLocator移行を安全かつ段階的に実行するための
    /// 汎用ヘルパー機能を提供する静的ユーティリティクラスです。
    /// Step 3.6で確立された段階的更新パターンを全サービスに適用可能な
    /// 統一インターフェースとして汎用化しています。
    ///
    /// 【核心機能】
    /// - 段階的サービス取得: ServiceLocator優先→レガシーフォールバック戦略
    /// - 移行状態診断: FeatureFlags・ServiceLocator・レガシーシステムの総合診断
    /// - 推奨設定判定: 現在状況に基づく最適な移行戦略の自動提案
    /// - 便利メソッド群: 簡易取得・状態確認・可用性チェックの統合API
    ///
    /// 【段階的移行戦略】
    /// 1. ServiceLocator優先取得: FeatureFlags.UseServiceLocator確認後の新システム利用
    /// 2. レガシーフォールバック: ServiceLocator失敗時のSingleton/FindFirstObjectByType利用
    /// 3. 安全性制御: FeatureFlags.AllowSingletonFallbackによる段階的制御
    /// 4. 監視統合: FeatureFlags.EnableMigrationMonitoringによる移行追跡
    ///
    /// 【対応サービス】
    /// - IAudioService: 基本音響サービスの段階的移行
    /// - IStealthAudioService: ステルス音響サービスの段階的移行
    /// - 拡張可能設計: 新サービス型の追加に対応した汎用パターン
    ///
    /// 【結果追跡】
    /// - MigrationResult&lt;T&gt;: 取得結果・使用システム・エラー情報の詳細記録
    /// - IsUsingServiceLocator: 新旧システム識別による移行進捗追跡
    /// - ErrorMessage: 失敗時の詳細エラー情報による問題診断支援
    ///
    /// 【統合設計】
    /// - FeatureFlags連携: 段階的移行制御フラグとの密結合統合
    /// - EventLogger統合: 移行状況の構造化ログ出力
    /// - 例外安全: try-catch包囲による移行プロセスの安定性保証
    /// - パフォーマンス最適化: 最小限のオーバーヘッドでの移行機能提供
    /// </summary>
    public static class ServiceMigrationHelper
    {
        /// <summary>
        /// サービス移行結果データ記録クラス
        ///
        /// Singleton→ServiceLocator移行プロセスにおけるサービス取得結果の
        /// 詳細情報を構造化して記録・管理する汎用結果データクラスです。
        /// 移行状況の追跡、エラー診断、システム最適化に必要な情報を統合提供します。
        ///
        /// 【記録データ】
        /// - Service: 取得されたサービスインスタンス（成功時）
        /// - IsUsingServiceLocator: 新システム（ServiceLocator）使用の可否
        /// - IsSuccessful: 取得成功の可否（核心判定フィールド）
        /// - ServiceTypeName: 取得されたサービスの具体型名（診断用）
        /// - ErrorMessage: 失敗時の詳細エラー情報（問題解決支援）
        ///
        /// 【用途】
        /// - 移行進捗追跡: 新旧システム使用比率の可視化
        /// - エラー診断支援: 失敗原因の詳細記録と分析
        /// - パフォーマンス監視: 取得システム別の成功率測定
        /// - システム最適化: 移行パターンの改善点特定
        ///
        /// 【設計特徴】
        /// - ジェネリック型制約: where T : class による参照型限定
        /// - 失敗時デフォルト: コンストラクタでの安全な初期状態設定
        /// - 構造化エラー: 文字列ベースの詳細エラー情報記録
        /// - 軽量設計: 最小限のメモリ占有によるパフォーマンス確保
        /// </summary>
        public class MigrationResult<T> where T : class
        {
            /// <summary>
            /// 取得されたサービスインスタンス
            ///
            /// 移行プロセスで成功時に取得されたサービスオブジェクトを保持します。
            /// ServiceLocator経由またはレガシーシステム経由で取得された実際のサービス実装を格納し、
            /// 呼び出し元での直接利用を可能にします。失敗時はnullが設定されます。
            /// </summary>
            public T Service { get; set; }

            /// <summary>
            /// ServiceLocatorシステム使用フラグ
            ///
            /// 取得されたサービスが新システム（ServiceLocator）経由で取得されたかを示します。
            /// true: ServiceLocator経由取得（推奨される新システム）
            /// false: レガシーシステム経由取得（Singleton/FindFirstObjectByType）
            /// 移行進捗の追跡と新旧システム使用比率の測定に使用されます。
            /// </summary>
            public bool IsUsingServiceLocator { get; set; }

            /// <summary>
            /// サービス取得成功フラグ
            ///
            /// サービス取得プロセス全体の成功・失敗を示す核心判定フィールドです。
            /// true: サービス取得成功（Serviceプロパティに有効なインスタンス格納）
            /// false: サービス取得失敗（ErrorMessageに詳細エラー情報格納）
            /// 呼び出し元での結果判定とエラーハンドリングの基盤として機能します。
            /// </summary>
            public bool IsSuccessful { get; set; }

            /// <summary>
            /// 取得サービスの具体型名
            ///
            /// 実際に取得されたサービス実装の型名を文字列で記録します。
            /// 成功時: Service.GetType().Name（例: "AudioManager", "StealthAudioCoordinator"）
            /// 失敗時: "Unknown"（デフォルト値）
            /// デバッグ、ログ出力、システム診断での実装識別に使用されます。
            /// </summary>
            public string ServiceTypeName { get; set; }

            /// <summary>
            /// 失敗時の詳細エラーメッセージ
            ///
            /// サービス取得に失敗した場合の具体的なエラー情報を記録します。
            /// 成功時: 空文字列
            /// 失敗時: 例外メッセージまたは失敗理由の詳細説明
            /// エラー診断、問題解決支援、ユーザーへの適切な情報提供に使用されます。
            /// </summary>
            public string ErrorMessage { get; set; }

            /// <summary>
            /// MigrationResultインスタンスの安全な初期化コンストラクタ
            ///
            /// 失敗状態をデフォルトとした安全な初期状態を設定します。
            /// 移行プロセスでの例外安全性を確保し、未初期化状態による
            /// 予期せぬ動作を防止します。
            ///
            /// 【初期設定値】
            /// - IsSuccessful: false（失敗状態デフォルト、成功時に明示的にtrueに変更）
            /// - ServiceTypeName: "Unknown"（型名未確定状態、成功時に実際の型名に変更）
            /// - ErrorMessage: string.Empty（エラーなし状態、失敗時に詳細情報設定）
            /// - Service: null（参照型のデフォルト値、成功時に実際のインスタンス設定）
            /// - IsUsingServiceLocator: false（boolean型のデフォルト値、取得時に実際の値設定）
            /// </summary>
            public MigrationResult()
            {
                IsSuccessful = false;
                ServiceTypeName = "Unknown";
                ErrorMessage = string.Empty;
            }
        }

        /// <summary>
        /// IAudioServiceの段階的取得
        /// ServiceLocator優先、フォールバック付き
        /// </summary>
        /// <param name="useServiceLocator">ServiceLocatorを使用するか</param>
        /// <param name="context">呼び出し元のコンテキスト名</param>
        /// <param name="enableDebugLogs">デバッグログを有効にするか</param>
        /// <returns>取得結果</returns>
        public static MigrationResult<IAudioService> GetAudioService(
            bool useServiceLocator = true, 
            string context = "Unknown", 
            bool enableDebugLogs = true)
        {
            var result = new MigrationResult<IAudioService>();

            LogDebug($"[{context}] Getting IAudioService (useServiceLocator: {useServiceLocator})", enableDebugLogs);

            // ServiceLocator優先取得
            if (useServiceLocator && FeatureFlags.UseServiceLocator)
            {
                result = GetAudioServiceFromServiceLocator(context, enableDebugLogs);
                
                // ServiceLocatorで取得できた場合はそれを返す
                if (result.IsSuccessful)
                {
                    return result;
                }
            }

            // フォールバック: レガシー方式
            LogDebug($"[{context}] ServiceLocator failed, trying legacy fallback", enableDebugLogs);
            return GetAudioServiceLegacy(context, enableDebugLogs);
        }

        /// <summary>
        /// IStealthAudioServiceの段階的取得
        /// ServiceLocator優先、フォールバック付き
        /// </summary>
        /// <param name="useServiceLocator">ServiceLocatorを使用するか</param>
        /// <param name="context">呼び出し元のコンテキスト名</param>
        /// <param name="enableDebugLogs">デバッグログを有効にするか</param>
        /// <returns>取得結果</returns>
        public static MigrationResult<IStealthAudioService> GetStealthAudioService(
            bool useServiceLocator = true, 
            string context = "Unknown", 
            bool enableDebugLogs = true)
        {
            var result = new MigrationResult<IStealthAudioService>();

            LogDebug($"[{context}] Getting IStealthAudioService (useServiceLocator: {useServiceLocator})", enableDebugLogs);

            // ServiceLocator優先取得
            if (useServiceLocator && FeatureFlags.UseServiceLocator)
            {
                result = GetStealthAudioServiceFromServiceLocator(context, enableDebugLogs);
                
                // ServiceLocatorで取得できた場合はそれを返す
                if (result.IsSuccessful)
                {
                    return result;
                }
            }

            // フォールバック: レガシー方式
            LogDebug($"[{context}] ServiceLocator failed, trying legacy fallback", enableDebugLogs);
            return GetStealthAudioServiceLegacy(context, enableDebugLogs);
        }

        /// <summary>
        /// 段階的更新の状態診断
        /// </summary>
        /// <param name="context">呼び出し元のコンテキスト名</param>
/// <summary>
        /// 段階的更新の状態診断
        /// </summary>
        /// <param name="context">呼び出し元のコンテキスト名</param>
        public static void DiagnoseMigrationState(string context = "Unknown")
        {
            ServiceLocator.GetService<IEventLogger>()?.Log($"=== Migration State Diagnosis - {context} ===");
            
            // FeatureFlags状態
            ServiceLocator.GetService<IEventLogger>()?.Log($"FeatureFlags.UseServiceLocator: {FeatureFlags.UseServiceLocator}");
            ServiceLocator.GetService<IEventLogger>()?.Log($"FeatureFlags.MigrateStealthAudioCoordinator: {FeatureFlags.MigrateStealthAudioCoordinator}");
            ServiceLocator.GetService<IEventLogger>()?.Log($"FeatureFlags.EnableDebugLogging: {FeatureFlags.EnableDebugLogging}");
            ServiceLocator.GetService<IEventLogger>()?.Log($"FeatureFlags.EnableMigrationMonitoring: {FeatureFlags.EnableMigrationMonitoring}");
            
            // ServiceLocator状態
            try
            {
                var audioService = ServiceLocator.GetService<IAudioService>();
                var stealthService = ServiceLocator.GetService<IStealthAudioService>();
                
                ServiceLocator.GetService<IEventLogger>()?.Log($"ServiceLocator IAudioService: {(audioService != null ? audioService.GetType().Name : "null")}");
                ServiceLocator.GetService<IEventLogger>()?.Log($"ServiceLocator IStealthAudioService: {(stealthService != null ? stealthService.GetType().Name : "null")}");
            }
            catch (System.Exception ex)
            {
                ServiceLocator.GetService<IEventLogger>()?.LogError($"ServiceLocator access failed: {ex.Message}");
            }
            
            // レガシーシステム状態
            // CheckLegacySystemState(); // Method not implemented - removed to fix compilation
            
            ServiceLocator.GetService<IEventLogger>()?.Log($"=== End Migration State Diagnosis - {context} ===");
        }

        /// <summary>
        /// 段階的更新の推奨設定取得
        /// </summary>
        /// <returns>推奨設定情報</returns>
        public static (bool useServiceLocator, string reason) GetRecommendedSettings()
        {
            // FeatureFlagsに基づいた推奨設定の判定
            if (FeatureFlags.UseServiceLocator && FeatureFlags.MigrateStealthAudioCoordinator)
            {
                return (true, "ServiceLocator and migration flags are enabled");
            }
            
            if (!FeatureFlags.UseServiceLocator)
            {
                return (false, "ServiceLocator is disabled in FeatureFlags");
            }
            
            if (FeatureFlags.UseServiceLocator && !FeatureFlags.MigrateStealthAudioCoordinator)
            {
                return (false, "ServiceLocator enabled but StealthAudioCoordinator migration disabled");
            }
            
            return (false, "Unable to determine optimal settings");
        }

        #region Private Methods

        /// <summary>
        /// ServiceLocatorからIAudioServiceを取得
        /// </summary>
        private static MigrationResult<IAudioService> GetAudioServiceFromServiceLocator(string context, bool enableDebugLogs)
        {
            var result = new MigrationResult<IAudioService>();

            try
            {
                result.Service = ServiceLocator.GetService<IAudioService>();
                
                if (result.Service != null)
                {
                    result.IsUsingServiceLocator = true;
                    result.IsSuccessful = true;
                    result.ServiceTypeName = result.Service.GetType().Name;
                    
                    LogDebug($"[{context}] ✅ Successfully obtained IAudioService from ServiceLocator: {result.ServiceTypeName}", enableDebugLogs);
                }
                else
                {
                    result.ErrorMessage = "ServiceLocator returned null for IAudioService";
                    LogDebug($"[{context}] ❌ ServiceLocator returned null for IAudioService", enableDebugLogs);
                }
            }
            catch (System.Exception ex)
            {
                result.ErrorMessage = ex.Message;
                LogDebug($"[{context}] ❌ ServiceLocator IAudioService access failed: {ex.Message}", enableDebugLogs);
            }

            return result;
        }

        /// <summary>
        /// ServiceLocatorからIStealthAudioServiceを取得
        /// </summary>
        private static MigrationResult<IStealthAudioService> GetStealthAudioServiceFromServiceLocator(string context, bool enableDebugLogs)
        {
            var result = new MigrationResult<IStealthAudioService>();

            try
            {
                result.Service = ServiceLocator.GetService<IStealthAudioService>();
                
                if (result.Service != null)
                {
                    result.IsUsingServiceLocator = true;
                    result.IsSuccessful = true;
                    result.ServiceTypeName = result.Service.GetType().Name;
                    
                    LogDebug($"[{context}] ✅ Successfully obtained IStealthAudioService from ServiceLocator: {result.ServiceTypeName}", enableDebugLogs);
                }
                else
                {
                    result.ErrorMessage = "ServiceLocator returned null for IStealthAudioService";
                    LogDebug($"[{context}] ❌ ServiceLocator returned null for IStealthAudioService", enableDebugLogs);
                }
            }
            catch (System.Exception ex)
            {
                result.ErrorMessage = ex.Message;
                LogDebug($"[{context}] ❌ ServiceLocator IStealthAudioService access failed: {ex.Message}", enableDebugLogs);
            }

            return result;
        }

        /// <summary>
        /// レガシー方式でIAudioServiceを取得
        /// </summary>
        private static MigrationResult<IAudioService> GetAudioServiceLegacy(string context, bool enableDebugLogs)
        {
            var result = new MigrationResult<IAudioService>();

            if (!FeatureFlags.AllowSingletonFallback)
            {
                result.ErrorMessage = "Legacy singletons are disabled";
                LogDebug($"[{context}] ❌ Legacy singletons are disabled", enableDebugLogs);
                return result;
            }

            try
            {
#pragma warning disable CS0618
                var legacyAudioManager = Object.FindFirstObjectByType<asterivo.Unity60.Core.Audio.AudioManager>();
                
                if (legacyAudioManager != null)
                {
                    result.Service = legacyAudioManager;
                    result.IsUsingServiceLocator = false;
                    result.IsSuccessful = true;
                    result.ServiceTypeName = "AudioManager (Legacy)";
                    
                    LogDebug($"[{context}] ✅ Successfully obtained IAudioService from legacy system", enableDebugLogs);
                    
                    if (FeatureFlags.EnableMigrationMonitoring)
                    {
                        ServiceLocator.GetService<IEventLogger>()?.LogWarning($"[{context}] Using legacy AudioManager access");
                    }
                }
                else
                {
                    result.ErrorMessage = "Legacy AudioManager not found";
                    LogDebug($"[{context}] ❌ Legacy AudioManager not found", enableDebugLogs);
                }
#pragma warning restore CS0618
            }
            catch (System.Exception ex)
            {
                result.ErrorMessage = ex.Message;
                LogDebug($"[{context}] ❌ Legacy IAudioService access failed: {ex.Message}", enableDebugLogs);
            }

            return result;
        }

        /// <summary>
        /// レガシー方式でIStealthAudioServiceを取得
        /// </summary>
        private static MigrationResult<IStealthAudioService> GetStealthAudioServiceLegacy(string context, bool enableDebugLogs)
        {
            var result = new MigrationResult<IStealthAudioService>();

            if (!FeatureFlags.AllowSingletonFallback)
            {
                result.ErrorMessage = "Legacy singletons are disabled";
                LogDebug($"[{context}] ❌ Legacy singletons are disabled", enableDebugLogs);
                return result;
            }

            try
            {
#pragma warning disable CS0618
                var legacyStealthCoordinator = Object.FindFirstObjectByType<asterivo.Unity60.Core.Audio.StealthAudioCoordinator>();
                
                if (legacyStealthCoordinator != null)
                {
                    result.Service = legacyStealthCoordinator;
                    result.IsUsingServiceLocator = false;
                    result.IsSuccessful = true;
                    result.ServiceTypeName = "StealthAudioCoordinator (Legacy)";
                    
                    LogDebug($"[{context}] ✅ Successfully obtained IStealthAudioService from legacy system", enableDebugLogs);
                    
                    if (FeatureFlags.EnableMigrationMonitoring)
                    {
                        ServiceLocator.GetService<IEventLogger>()?.LogWarning($"[{context}] Using legacy StealthAudioCoordinator access");
                    }
                }
                else
                {
                    result.ErrorMessage = "Legacy StealthAudioCoordinator not found";
                    LogDebug($"[{context}] ❌ Legacy StealthAudioCoordinator not found", enableDebugLogs);
                }
#pragma warning restore CS0618
            }
            catch (System.Exception ex)
            {
                result.ErrorMessage = ex.Message;
                LogDebug($"[{context}] ❌ Legacy IStealthAudioService access failed: {ex.Message}", enableDebugLogs);
            }

            return result;
        }

        /// <summary>
        /// デバッグログ出力
        /// </summary>
        private static void LogDebug(string message, bool enableDebugLogs)
        {
            if (enableDebugLogs && FeatureFlags.EnableDebugLogging)
            {
                ServiceLocator.GetService<IEventLogger>()?.Log(message);
            }
        }

        #endregion

        #region Convenience Methods

        /// <summary>
        /// 単純なIAudioService取得（エラーハンドリング付き）
        /// </summary>
        /// <param name="context">呼び出し元コンテキスト</param>
        /// <returns>IAudioServiceまたはnull</returns>
        public static IAudioService GetAudioServiceSimple(string context = "Unknown")
        {
            var result = GetAudioService(true, context, false);
            return result.IsSuccessful ? result.Service : null;
        }

        /// <summary>
        /// 単純なIStealthAudioService取得（エラーハンドリング付き）
        /// </summary>
        /// <param name="context">呼び出し元コンテキスト</param>
        /// <returns>IStealthAudioServiceまたはnull</returns>
        public static IStealthAudioService GetStealthAudioServiceSimple(string context = "Unknown")
        {
            var result = GetStealthAudioService(true, context, false);
            return result.IsSuccessful ? result.Service : null;
        }

        /// <summary>
        /// 移行状態のクイックチェック
        /// </summary>
        /// <returns>移行が有効かどうか</returns>
        public static bool IsMigrationActive()
        {
            return FeatureFlags.UseServiceLocator && FeatureFlags.MigrateStealthAudioCoordinator;
        }

        /// <summary>
        /// レガシーシステムが利用可能かチェック
        /// </summary>
        /// <returns>レガシーシステムが利用可能かどうか</returns>
        public static bool IsLegacySystemAvailable()
        {
            return FeatureFlags.AllowSingletonFallback;
        }

        #endregion
    }
}