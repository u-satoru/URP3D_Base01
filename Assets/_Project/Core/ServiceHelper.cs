using System;
using UnityEngine;

namespace asterivo.Unity60.Core
{
    /// <summary>
    /// ServiceLocatorとUnityのFindObjectメソッドを組み合わせたヘルパークラス
    /// Phase 3移行パターンの実装支援
    /// </summary>
    public static class ServiceHelper
    {
        /// <summary>
        /// ServiceLocatorから取得を試み、失敗時はFindFirstObjectByTypeでフォールバック
        /// </summary>
        public static T GetServiceWithFallback<T>() where T : UnityEngine.Object
        {
            // まずServiceLocatorから取得を試みる
            if (FeatureFlags.UseServiceLocator)
            {
                try
                {
                    // ServiceLocatorから取得（ServiceLocatorはclass制約のみ）
                    var service = ServiceLocator.GetService<T>();
                    if (service != null)
                    {
                        return service;
                    }
                }
                catch (Exception)
                {
                    // ServiceLocatorからの取得失敗時は次のフォールバックへ
                }
            }

            // フォールバック: FindFirstObjectByType
            if (FeatureFlags.AllowSingletonFallback)
            {
                try
                {
                    return UnityEngine.Object.FindFirstObjectByType<T>();
                }
                catch (Exception)
                {
                    // フォールバックも失敗
                }
            }

            return null;
        }

        /// <summary>
        /// ServiceLocatorから取得を試み、失敗時はnullを返す（フォールバックなし）
        /// </summary>
        public static T GetService<T>() where T : class
        {
            if (FeatureFlags.UseServiceLocator)
            {
                try
                {
                    return ServiceLocator.GetService<T>();
                }
                catch (Exception)
                {
                    return null;
                }
            }

            return null;
        }

        /// <summary>
        /// IEventLoggerを取得（頻繁に使用されるため専用メソッド）
        /// </summary>
        public static IEventLogger GetEventLogger()
        {
            try
            {
                return ServiceLocator.GetService<IEventLogger>();
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 安全にログを出力（IEventLoggerが存在しない場合は何もしない）
        /// </summary>
        public static void Log(string message)
        {
            var logger = GetEventLogger();
            logger?.Log(message);
        }

        /// <summary>
        /// 安全に警告ログを出力
        /// </summary>
        public static void LogWarning(string message)
        {
            var logger = GetEventLogger();
            logger?.LogWarning(message);
        }

        /// <summary>
        /// 安全にエラーログを出力
        /// </summary>
        public static void LogError(string message)
        {
            var logger = GetEventLogger();
            logger?.LogError(message);
        }
    }
}