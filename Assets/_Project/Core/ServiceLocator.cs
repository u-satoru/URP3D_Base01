using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using UnityEngine;

namespace asterivo.Unity60.Core
{
    /// <summary>
    /// Service Locatorパターンの実装（パフォーマンス最適化版）
    /// DIフレームワークを使わずに依存関係を管理
    /// 
    /// 最適化内容:
    /// - ConcurrentDictionaryによるロック削減
    /// - Type名キャッシュによる文字列操作削減
    /// - 条件付きログによるランタイムオーバーヘッド削減
    /// - メモリアロケーション最小化
    /// </summary>
    public static class ServiceLocator
    {
        // パフォーマンス最適化: ConcurrentDictionaryで読み取り性能向上
        private static readonly ConcurrentDictionary<Type, object> services = new ConcurrentDictionary<Type, object>();
        private static readonly ConcurrentDictionary<Type, Func<object>> factories = new ConcurrentDictionary<Type, Func<object>>();
        
        // Type名キャッシュ: ToString()の重複実行を避ける
        private static readonly ConcurrentDictionary<Type, string> typeNameCache = new ConcurrentDictionary<Type, string>();
        
        // 統計情報（パフォーマンス監視用）
        private static volatile int accessCount = 0;
        private static volatile int hitCount = 0;
        
        /// <summary>
        /// サービスを登録（パフォーマンス最適化版）
        /// </summary>
        public static void RegisterService<T>(T service) where T : class
        {
            var type = typeof(T);
            var typeName = GetCachedTypeName(type);
            
            var wasReplaced = services.ContainsKey(type);
            services[type] = service;
            
            // 条件付きログ: エディタまたは開発ビルドでのみ実行
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (wasReplaced)
            {
                UnityEngine.Debug.LogWarning($"[ServiceLocator] Service {typeName} replaced");
            }
            else
            {
                UnityEngine.Debug.Log($"[ServiceLocator] Service {typeName} registered");
            }
#endif
        }
        
        /// <summary>
        /// ファクトリメソッドを登録（遅延初期化用、パフォーマンス最適化版）
        /// </summary>
        public static void RegisterFactory<T>(Func<T> factory) where T : class
        {
            var type = typeof(T);
            factories[type] = () => factory();
            
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            var typeName = GetCachedTypeName(type);
            UnityEngine.Debug.Log($"[ServiceLocator] Factory for {typeName} registered");
#endif
        }
        
        /// <summary>
        /// サービスを取得（パフォーマンス最適化版）
        /// </summary>
        public static T GetService<T>() where T : class
        {
            var type = typeof(T);
            
            // 統計更新（volatile操作で軽量）
            System.Threading.Interlocked.Increment(ref accessCount);
            
            // 最高頻度パス: 既存サービスの高速検索
            if (services.TryGetValue(type, out var service))
            {
                System.Threading.Interlocked.Increment(ref hitCount);
                return service as T;
            }
            
            // ファクトリからの遅延生成（低頻度）
            if (factories.TryGetValue(type, out var factory))
            {
                var newService = factory() as T;
                if (newService != null)
                {
                    // アトミック操作: 重複生成を防ぐ
                    services.TryAdd(type, newService);
                    factories.TryRemove(type, out _);
                    System.Threading.Interlocked.Increment(ref hitCount);
                    return newService;
                }
            }
            
            // 条件付きログ: 本番環境のパフォーマンス保護
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            var typeName = GetCachedTypeName(type);
            UnityEngine.Debug.LogWarning($"[ServiceLocator] Service {typeName} not found");
#endif
            return null;
        }
        
        /// <summary>
        /// サービスを取得（取得できない場合は例外）
        /// </summary>
        public static T RequireService<T>() where T : class
        {
            var service = GetService<T>();
            if (service == null)
            {
                throw new InvalidOperationException($"Required service {typeof(T).Name} is not registered");
            }
            return service;
        }
        
        /// <summary>
        /// サービスが登録されているか確認（パフォーマンス最適化版）
        /// </summary>
        public static bool HasService<T>() where T : class
        {
            var type = typeof(T);
            return services.ContainsKey(type) || factories.ContainsKey(type);
        }
        
        /// <summary>
        /// サービスが登録されているか確認（HasServiceのエイリアス）
        /// </summary>
        public static bool IsServiceRegistered<T>() where T : class
        {
            return HasService<T>();
        }
        
        /// <summary>
        /// 特定のサービスを削除（パフォーマンス最適化版）
        /// </summary>
        public static void UnregisterService<T>() where T : class
        {
            var type = typeof(T);
            var serviceRemoved = services.TryRemove(type, out _);
            var factoryRemoved = factories.TryRemove(type, out _);
            
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (serviceRemoved || factoryRemoved)
            {
                var typeName = GetCachedTypeName(type);
                UnityEngine.Debug.Log($"[ServiceLocator] Service {typeName} unregistered");
            }
#endif
        }
        
        /// <summary>
        /// すべてのサービスをクリア（パフォーマンス最適化版）
        /// </summary>
        public static void Clear()
        {
            services.Clear();
            factories.Clear();
            namedServices.Clear();
            typeNameCache.Clear();

            // 統計リセット
            System.Threading.Interlocked.Exchange(ref accessCount, 0);
            System.Threading.Interlocked.Exchange(ref hitCount, 0);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log("[ServiceLocator] All services cleared");
#endif
        }
        
        /// <summary>
        /// 登録されているサービスの数を取得（パフォーマンス最適化版）
        /// </summary>
        public static int GetServiceCount()
        {
            return services.Count + factories.Count + namedServices.Count;
        }
        
        /// <summary>
        /// デバッグ用：登録されているサービス一覧を出力（パフォーマンス最適化版）
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void LogAllServices()
        {
            UnityEngine.Debug.Log($"[ServiceLocator] === Registered Services ({services.Count}) ===");
            foreach (var kvp in services)
            {
                var serviceTypeName = GetCachedTypeName(kvp.Value.GetType());
                var interfaceTypeName = GetCachedTypeName(kvp.Key);
                UnityEngine.Debug.Log($"  - {interfaceTypeName}: {serviceTypeName}");
            }
            
            UnityEngine.Debug.Log($"[ServiceLocator] === Registered Factories ({factories.Count}) ===");
            foreach (var kvp in factories)
            {
                var typeName = GetCachedTypeName(kvp.Key);
                UnityEngine.Debug.Log($"  - {typeName}: [Factory]");
            }
            
            // パフォーマンス統計も表示
            LogPerformanceStats();
        }

        // String-based service registration for scenarios requiring named services
        private static readonly ConcurrentDictionary<string, object> namedServices = new ConcurrentDictionary<string, object>();

        /// <summary>
        /// 名前付きサービスを登録（文字列キー）
        /// </summary>
        public static void RegisterService<T>(string key, T service) where T : class
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Service key cannot be null or empty", nameof(key));
            if (service == null)
                throw new ArgumentNullException(nameof(service));

            var wasReplaced = namedServices.ContainsKey(key);
            namedServices[key] = service;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (wasReplaced)
            {
                UnityEngine.Debug.LogWarning($"[ServiceLocator] Named service '{key}' replaced");
            }
            else
            {
                UnityEngine.Debug.Log($"[ServiceLocator] Named service '{key}' registered");
            }
#endif
        }

        /// <summary>
        /// 名前付きサービスを取得（文字列キー）
        /// </summary>
        public static T GetService<T>(string key) where T : class
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Service key cannot be null or empty", nameof(key));

            System.Threading.Interlocked.Increment(ref accessCount);

            if (namedServices.TryGetValue(key, out var service))
            {
                System.Threading.Interlocked.Increment(ref hitCount);
                if (service is T typedService)
                {
                    return typedService;
                }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.LogError($"[ServiceLocator] Service '{key}' exists but is not of type {typeof(T).Name}");
#endif
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.LogWarning($"[ServiceLocator] Named service '{key}' not found");
#endif
            return null;
        }

        /// <summary>
        /// 名前付きサービスを削除（文字列キー）
        /// </summary>
        public static void UnregisterService(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Service key cannot be null or empty", nameof(key));

            var removed = namedServices.TryRemove(key, out _);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (removed)
            {
                UnityEngine.Debug.Log($"[ServiceLocator] Named service '{key}' unregistered");
            }
#endif
        }

        /// <summary>
        /// Type名を取得（キャッシュ利用でパフォーマンス最適化）
        /// </summary>
        private static string GetCachedTypeName(Type type)
        {
            return typeNameCache.GetOrAdd(type, t => t.Name);
        }
        
        /// <summary>
        /// パフォーマンス統計を取得
        /// </summary>
        public static (int accessCount, int hitCount, float hitRate) GetPerformanceStats()
        {
                        var currentAccessCount = accessCount;
                        var currentHitCount = hitCount;
            var hitRate = currentAccessCount > 0 ? (float)currentHitCount / currentAccessCount : 0f;
            
            return (currentAccessCount, currentHitCount, hitRate);
        }
        
        /// <summary>
        /// パフォーマンス統計をログ出力
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void LogPerformanceStats()
        {
            var stats = GetPerformanceStats();
            UnityEngine.Debug.Log($"[ServiceLocator] Performance Stats - " +
                                $"Access: {stats.accessCount}, " +
                                $"Hits: {stats.hitCount}, " +
                                $"Hit Rate: {stats.hitRate:P1}");
        }
        
        /// <summary>
        /// パフォーマンス統計をリセット
        /// </summary>
        public static void ResetPerformanceStats()
        {
            System.Threading.Interlocked.Exchange(ref accessCount, 0);
            System.Threading.Interlocked.Exchange(ref hitCount, 0);
        }
    }
}
