using UnityEngine;
using asterivo.Unity60.Core;
// using asterivo.Unity60.Core.Services; // Removed to avoid circular dependency

namespace asterivo.Unity60.Core.Debug
{
    /// <summary>
    /// プロジェクト統一Debugシステム
    /// EventLoggerと連携し、環境別ログ制御とパフォーマンス最適化を提供
    /// </summary>
    public static class ProjectDebug
    {
        private const string EDITOR_PREFIX = "[EDITOR] ";
        private const string RUNTIME_PREFIX = "[RUNTIME] ";

        /// <summary>
        /// 情報レベルログ出力
        /// Editor環境とRuntime環境の両方に対応
        /// </summary>
        public static void Log(string message)
        {
            LogInternal(message, LogLevel.Info);
        }

        /// <summary>
        /// 警告レベルログ出力
        /// </summary>
        public static void LogWarning(string message)
        {
            LogInternal(message, LogLevel.Warning);
        }

        /// <summary>
        /// エラーレベルログ出力
        /// </summary>
        public static void LogError(string message)
        {
            LogInternal(message, LogLevel.Error);
        }

        /// <summary>
        /// 条件付きログ出力
        /// FeatureFlagsやデバッグ条件に基づいて出力制御
        /// </summary>
        public static void LogConditional(string message, bool condition)
        {
            if (condition)
            {
                Log(message);
            }
        }

        /// <summary>
        /// Editor専用ログ出力
        /// Editorでのみ表示され、ビルドには含まれない
        /// </summary>
        public static void LogEditor(string message)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.Log($"{EDITOR_PREFIX}{message}");
#endif
        }

        /// <summary>
        /// Editor専用警告ログ出力
        /// </summary>
        public static void LogEditorWarning(string message)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.LogWarning($"{EDITOR_PREFIX}{message}");
#endif
        }

        /// <summary>
        /// Editor専用エラーログ出力
        /// </summary>
        public static void LogEditorError(string message)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.LogError($"{EDITOR_PREFIX}{message}");
#endif
        }

        /// <summary>
        /// パフォーマンス測定付きログ出力
        /// 実行時間を測定してログに含める
        /// </summary>
        public static void LogWithTiming(string message, System.Action action)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            action?.Invoke();
            stopwatch.Stop();
            
            Log($"{message} (Execution time: {stopwatch.ElapsedMilliseconds}ms)");
        }

        /// <summary>
        /// 内部ログ処理
        /// Editor/Runtime環境の判定とEventLoggerとの統合
        /// </summary>
        private static void LogInternal(string message, LogLevel level)
        {
            // Editor環境での出力
#if UNITY_EDITOR
            var editorMessage = $"{EDITOR_PREFIX}{message}";
            switch (level)
            {
                case LogLevel.Info:
                    UnityEngine.Debug.Log(editorMessage);
                    break;
                case LogLevel.Warning:
                    UnityEngine.Debug.LogWarning(editorMessage);
                    break;
                case LogLevel.Error:
                    UnityEngine.Debug.LogError(editorMessage);
                    break;
            }
#endif

            // Runtime環境でのEventLogger統合
            if (ShouldLogToEventLogger(level))
            {
                var runtimeMessage = $"{RUNTIME_PREFIX}{message}";
                
                // ServiceLocator経由でEventLoggerを取得
                var eventLogger = ServiceLocator.GetService<IEventLogger>();
                if (eventLogger != null)
                {
                    switch (level)
                    {
                        case LogLevel.Info:
                            eventLogger.Log(runtimeMessage);
                            break;
                        case LogLevel.Warning:
                            eventLogger.LogWarning(runtimeMessage);
                            break;
                        case LogLevel.Error:
                            eventLogger.LogError(runtimeMessage);
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// EventLoggerへの出力判定
        /// FeatureFlagsに基づく制御
        /// </summary>
        private static bool ShouldLogToEventLogger(LogLevel level)
        {
            // デバッグログが無効な場合はErrorレベルのみ出力
            if (!FeatureFlags.EnableDebugLogging && level != LogLevel.Error)
            {
                return false;
            }

            return true;
        }
    }

    /// <summary>
    /// ログレベル定義
    /// </summary>
    public enum LogLevel
    {
        Info,
        Warning,
        Error
    }
}
