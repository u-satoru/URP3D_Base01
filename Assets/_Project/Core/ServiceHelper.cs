using System;
using UnityEngine;

namespace asterivo.Unity60.Core
{
    /// <summary>
    /// ServiceLocator統合ヘルパークラス
    ///
    /// Unity 6における3層アーキテクチャ移行時のハイブリッド型サービス取得を提供します。
    /// ServiceLocatorパターンとUnityの従来型FindObjectメソッドを統合し、
    /// 段階的移行やフォールバック機能を通じた安全なサービス解決を実現します。
    ///
    /// 【主要機能】
    /// - ServiceLocator優先でのサービス取得と自動フォールバック
    /// - フィーチャーフラグによる移行制御（UseServiceLocator/AllowSingletonFallback）
    /// - IEventLoggerの高速取得とnull安全なロギング機能
    /// - 例外処理による堅牢なサービス解決メカニズム
    ///
    /// 【用途】
    /// - Phase 3移行パターンの実装支援
    /// - レガシーコードからServiceLocatorパターンへの段階的移行
    /// - 開発環境とプロダクション環境での動作切り替え
    /// </summary>
    public static class ServiceHelper
    {
        /// <summary>
        /// ハイブリッド型サービス取得（フォールバック付き）
        ///
        /// ServiceLocatorパターンを優先使用し、失敗時にUnityEngine.Objectベースの
        /// FindFirstObjectByTypeメソッドによるフォールバック機能を提供します。
        ///
        /// 【実行フロー】
        /// 1. FeatureFlags.UseServiceLocatorがtrueの場合、ServiceLocatorから取得を試行
        /// 2. ServiceLocator取得が失敗（例外またはnull）の場合、自動的にフォールバックへ移行
        /// 3. FeatureFlags.AllowSingletonFallbackがtrueの場合、FindFirstObjectByType&lt;T&gt;()で検索
        /// 4. すべての取得方法が失敗した場合、nullを返却
        ///
        /// 【パフォーマンス特性】
        /// - ServiceLocator取得: O(1) - ConcurrentDictionaryベース
        /// - FindFirstObjectByType取得: O(n) - シーン内全オブジェクトスキャン
        ///
        /// 【制約】
        /// - 型パラメータTはUnityEngine.Object制約のみ（MonoBehaviour、ScriptableObject等対応）
        /// - フィーチャーフラグによる動作制御のため、予測可能性が重要な箇所では注意が必要
        ///
        /// 【用途】
        /// - Phase 3移行期における互換性確保
        /// - 開発時デバッグとプロダクション時パフォーマンス最適化の両立
        /// - ServiceLocator移行が未完了のレガシーコンポーネントとの連携
        /// </summary>
        /// <typeparam name="T">取得対象のサービス型（UnityEngine.Object派生型）</typeparam>
        /// <returns>取得されたサービスインスタンス。すべての取得方法が失敗した場合はnull</returns>
        public static T GetServiceWithFallback<T>() where T : UnityEngine.Object
        {
            // Phase 1: ServiceLocatorによる高速サービス取得（O(1)）
            // FeatureFlagsによる動的制御 - 開発時はtrue、レガシー互換時はfalse
            if (FeatureFlags.UseServiceLocator)
            {
                try
                {
                    // ConcurrentDictionary&lt;Type, object&gt;ベースの高速ルックアップ
                    // ServiceLocatorはclass制約のみのため、UnityEngine.Objectも対応
                    var service = ServiceLocator.GetService<T>();
                    if (service != null)
                    {
                        // ServiceLocator取得成功 - 最高速パス
                        return service;
                    }
                    // null取得時は例外を発生させずフォールバックへ移行
                }
                catch (Exception)
                {
                    // ServiceLocator未初期化、型未登録、またはその他の例外時
                    // ログ出力は行わず、サイレントにフォールバックへ移行
                    // これにより開発中のサービス移行作業を阻害しない
                }
            }

            // Phase 2: Unity従来型FindObjectによるフォールバック（O(n)）
            // AllowSingletonFallbackフラグによる制御 - 段階的移行時のセーフティネット
            if (FeatureFlags.AllowSingletonFallback)
            {
                try
                {
                    // Unity 2023.1以降推奨のFindFirstObjectByType使用
                    // シーン内全MonoBehaviourおよびScriptableObjectを線形検索
                    // パフォーマンス注意: 大規模シーンでは数十ミリ秒の遅延可能性
                    return UnityEngine.Object.FindFirstObjectByType<T>();
                }
                catch (Exception)
                {
                    // FindFirstObjectByType失敗時（型が見つからない、破損オブジェクト等）
                    // 最終的にnullを返却してサービス取得失敗を通知
                }
            }

            // 両方の取得方法が失敗またはフィーチャーフラグで無効化されている場合
            // nullを返却して呼び出し元での適切なエラーハンドリングを促進
            return null;
        }

        /// <summary>
        /// 純粋ServiceLocator型サービス取得（フォールバックなし）
        ///
        /// ServiceLocatorパターンのみを使用したサービス取得を行います。
        /// フォールバック機能を意図的に排除し、ServiceLocator移行の完了度を
        /// 明確に把握できるよう設計されています。
        ///
        /// 【実行フロー】
        /// 1. FeatureFlags.UseServiceLocatorがfalseの場合、即座にnullを返却
        /// 2. ServiceLocator.GetService&lt;T&gt;()を呼び出し、例外時はnullを返却
        ///
        /// 【パフォーマンス特性】
        /// - 常時O(1) - ConcurrentDictionaryベースの高速ルックアップのみ
        /// - FindFirstObjectByTypeなどの線形検索は一切実行されない
        ///
        /// 【制約】
        /// - 型パラメータTはclass制約（参照型のみ対応）
        /// - UnityEngine.Object派生型以外のサービス（C#クラス、インターフェース）にも対応
        ///
        /// 【用途】
        /// - ServiceLocator移行完了後のプロダクション環境
        /// - パフォーマンス重視の処理での確実な高速取得
        /// - 依存関係の明確化とアーキテクチャ準拠の確認
        /// </summary>
        /// <typeparam name="T">取得対象のサービス型（class制約）</typeparam>
        /// <returns>取得されたサービスインスタンス。取得失敗時はnull</returns>
        public static T GetService<T>() where T : class
        {
            // フィーチャーフラグチェック - ServiceLocator機能の有効性確認
            // 無効時は即座にnullを返却し、従来型検索パスを完全回避
            if (FeatureFlags.UseServiceLocator)
            {
                try
                {
                    // ServiceLocator.GetService&lt;T&gt;()による型安全な高速取得
                    // 内部的にConcurrentDictionary&lt;Type, object&gt;を使用したO(1)ルックアップ
                    return ServiceLocator.GetService<T>();
                }
                catch (Exception)
                {
                    // ServiceLocator例外時の安全な処理
                    // - サービス未登録（KeyNotFoundException）
                    // - ServiceLocator未初期化（NullReferenceException）
                    // - 型キャスト失敗（InvalidCastException）
                    // 例外詳細はログ出力せず、呼び出し元でのnullチェックに委ねる
                    return null;
                }
            }

            // FeatureFlags.UseServiceLocatorがfalseの場合のデフォルト動作
            // フォールバック機能を持たないため、確実にnullを返却
            return null;
        }

        /// <summary>
        /// IEventLogger専用高速取得メソッド
        ///
        /// 頻繁にアクセスされるIEventLoggerサービスのための最適化された取得メソッドです。
        /// フィーチャーフラグのチェックを省略し、ServiceLocatorから直接取得することで
        /// ロギング処理のオーバーヘッドを最小限に抑制します。
        ///
        /// 【最適化ポイント】
        /// - FeatureFlags.UseServiceLocatorチェックを省略（高速化）
        /// - try-catch構文による安全な例外処理
        /// - nullcoalescing演算子による簡潔な戻り値処理
        ///
        /// 【パフォーマンス特性】
        /// - 最高速O(1)取得 - 条件分岐を最小化
        /// - ホットパス最適化 - ログ出力時の呼び出し頻度を考慮
        ///
        /// 【用途】
        /// - Log(), LogWarning(), LogError()メソッドでの内部使用
        /// - 頻繁なロギング処理における性能確保
        /// - デバッグ情報の高速出力
        /// </summary>
        /// <returns>IEventLoggerインスタンス。取得失敗時はnull</returns>
        public static IEventLogger GetEventLogger()
        {
            try
            {
                // IEventLoggerサービスの直接取得 - フィーチャーフラグチェック省略による高速化
                // ロギング処理は常時有効であることを前提とした設計
                return ServiceLocator.GetService<IEventLogger>();
            }
            catch (Exception)
            {
                // IEventLogger取得失敗時の安全な処理
                // - EventLoggerサービス未登録時
                // - ServiceLocator未初期化時
                // ロギング機能の失敗はアプリケーション全体を停止させるべきではないため
                // サイレントにnullを返却し、呼び出し元でのnull安全処理に委ねる
                return null;
            }
        }

        /// <summary>
        /// null安全型情報ログ出力
        ///
        /// IEventLoggerサービスを通じて情報レベルのログメッセージを出力します。
        /// IEventLoggerが取得できない場合でも例外を発生させず、サイレントに処理を続行します。
        ///
        /// 【設計思想】
        /// - ロギング処理の失敗はアプリケーション動作を阻害すべきではない
        /// - null条件演算子（?.）による簡潔で安全な呼び出し
        /// - GetEventLogger()メソッドとの連携による最適化
        ///
        /// 【パフォーマンス特性】
        /// - IEventLogger取得成功時: O(1) + ログ出力処理時間
        /// - IEventLogger取得失敗時: O(1) - 即座にリターン
        ///
        /// 【用途】
        /// - 一般的な処理状況の記録
        /// - デバッグ情報の出力
        /// - システム状態の監視ログ
        /// </summary>
        /// <param name="message">出力するログメッセージ</param>
        public static void Log(string message)
        {
            // GetEventLogger()による最適化されたIEventLogger取得
            var logger = GetEventLogger();
            // null条件演算子による安全な呼び出し - loggerがnullでも例外なし
            logger?.Log(message);
        }

        /// <summary>
        /// null安全型警告ログ出力
        ///
        /// IEventLoggerサービスを通じて警告レベルのログメッセージを出力します。
        /// 注意が必要な状況や潜在的な問題の報告に使用されます。
        ///
        /// 【設計思想】
        /// - 警告ログの失敗でアプリケーションを停止させない
        /// - null条件演算子（?.）による堅牢性確保
        /// - GetEventLogger()の高速取得メカニズムを活用
        ///
        /// 【パフォーマンス特性】
        /// - IEventLogger取得成功時: O(1) + 警告ログ出力処理時間
        /// - IEventLogger取得失敗時: O(1) - 即座にリターン
        ///
        /// 【用途】
        /// - パフォーマンス劣化の警告
        /// - 設定ミスや推奨されない使用法の通知
        /// - リソース不足やメモリリークの検出
        /// - 非クリティカルエラーの報告
        /// </summary>
        /// <param name="message">出力する警告メッセージ</param>
        public static void LogWarning(string message)
        {
            // GetEventLogger()による最適化されたIEventLogger取得
            var logger = GetEventLogger();
            // null条件演算子による安全な警告ログ呼び出し - loggerがnullでも例外なし
            logger?.LogWarning(message);
        }

        /// <summary>
        /// null安全型エラーログ出力
        ///
        /// IEventLoggerサービスを通じてエラーレベルのログメッセージを出力します。
        /// 重大な問題やアプリケーションの動作に影響を与えるエラーの報告に使用されます。
        ///
        /// 【設計思想】
        /// - エラーログ出力の失敗でさらなる例外を発生させない
        /// - null条件演算子（?.）による例外安全性の確保
        /// - GetEventLogger()による高速なサービス取得
        ///
        /// 【パフォーマンス特性】
        /// - IEventLogger取得成功時: O(1) + エラーログ出力処理時間
        /// - IEventLogger取得失敗時: O(1) - 即座にリターン
        ///
        /// 【用途】
        /// - クリティカルなシステムエラーの記録
        /// - 例外キャッチ処理でのエラー詳細出力
        /// - ファイルI/O失敗やネットワークエラーの報告
        /// - データ整合性エラーの通知
        /// - アーキテクチャ制約違反の検出
        /// </summary>
        /// <param name="message">出力するエラーメッセージ</param>
        public static void LogError(string message)
        {
            // GetEventLogger()による最適化されたIEventLogger取得
            var logger = GetEventLogger();
            // null条件演算子による安全なエラーログ呼び出し - loggerがnullでも例外なし
            logger?.LogError(message);
        }
    }
}