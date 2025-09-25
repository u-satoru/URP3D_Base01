using System.Collections.Generic;
using UnityEngine;

namespace asterivo.Unity60.Core.Debug
{
    /// <summary>
    /// EventLoggerサービスのインターフェース
    /// ServiceLocatorパターンでの依存性注入に使用
    /// 
    /// 設計思想:
    /// - 中央集権的なログ管理による一貫性確保
    /// - 複数出力形式対応（Console, File, DebugWindow, RemoteDebugger）
    /// - イベント駆動システムとの統合
    /// - パフォーマンス監視とデバッグ支援機能
    /// </summary>
    public interface IEventLogger
    {
        /// <summary>
        /// ログが有効かどうか
        /// </summary>
        bool IsEnabled { get; }
        
        /// <summary>
        /// 現在のイベントログエントリのリスト
        /// </summary>
        List<EventLogger.EventLogEntry> EventLog { get; }
        
        /// <summary>
        /// 基本ログメッセージを記録
        /// Unity標準Debug.Logの代替として使用
        /// </summary>
        /// <param name="message">ログメッセージ</param>
        void Log(string message);
        
        /// <summary>
        /// 警告ログメッセージを記録
        /// Unity標準Debug.LogWarningの代替として使用
        /// </summary>
        /// <param name="message">警告メッセージ</param>
        void LogWarning(string message);
        
        /// <summary>
        /// エラーログメッセージを記録
        /// Unity標準Debug.LogErrorの代替として使用
        /// </summary>
        /// <param name="message">エラーメッセージ</param>
        void LogError(string message);
        
        /// <summary>
        /// イベントログを記録（リスナー数とペイロード付き）
        /// </summary>
        /// <param name="eventName">イベント名</param>
        /// <param name="listenerCount">リスナー数</param>
        /// <param name="payload">オプショナルペイロードデータ</param>
        void LogEvent(string eventName, int listenerCount, string payload = "");
        
        /// <summary>
        /// 型安全なペイロード付きイベントログを記録
        /// </summary>
        /// <typeparam name="T">ペイロードの型</typeparam>
        /// <param name="eventName">イベント名</param>
        /// <param name="listenerCount">リスナー数</param>
        /// <param name="payload">ペイロードデータ</param>
        void LogEventWithPayload<T>(string eventName, int listenerCount, T payload);
        
        /// <summary>
        /// 警告レベルのイベントログを記録
        /// </summary>
        /// <param name="eventName">イベント名</param>
        /// <param name="listenerCount">リスナー数</param>
        /// <param name="message">警告メッセージ</param>
        void LogWarning(string eventName, int listenerCount, string message);
        
        /// <summary>
        /// エラーレベルのイベントログを記録
        /// </summary>
        /// <param name="eventName">イベント名</param>
        /// <param name="listenerCount">リスナー数</param>
        /// <param name="message">エラーメッセージ</param>
        void LogError(string eventName, int listenerCount, string message);
        
        /// <summary>
        /// ログをクリア
        /// </summary>
        void ClearLog();
        
        /// <summary>
        /// フィルタリングされたログエントリを取得
        /// </summary>
        /// <param name="nameFilter">イベント名フィルタ</param>
        /// <param name="minLevel">最小ログレベル</param>
        /// <returns>フィルタリングされたログエントリのリスト</returns>
        List<EventLogger.EventLogEntry> GetFilteredLog(string nameFilter = "", EventLogger.LogLevel minLevel = EventLogger.LogLevel.Info);
        
        /// <summary>
        /// ログの統計情報を取得
        /// </summary>
        /// <returns>ログ統計情報</returns>
        EventLogger.LogStatistics GetStatistics();
        
        /// <summary>
        /// ログをCSVファイルにエクスポート
        /// </summary>
        /// <param name="filePath">エクスポート先ファイルパス</param>
        void ExportToCSV(string filePath);
    }
}
