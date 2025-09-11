using System;
using System.Collections.Generic;
using UnityEngine;

namespace asterivo.Unity60.Core
{
    /// <summary>
    /// Service Locatorパターンの実装
    /// DIフレームワークを使わずに依存関係を管理
    /// </summary>
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> services = new Dictionary<Type, object>();
        private static readonly Dictionary<Type, Func<object>> factories = new Dictionary<Type, Func<object>>();
        private static readonly object lockObject = new object();
        
        /// <summary>
        /// サービスを登録
        /// </summary>
        public static void RegisterService<T>(T service) where T : class
        {
            lock (lockObject)
            {
                var type = typeof(T);
                if (services.ContainsKey(type))
                {
                    UnityEngine.Debug.LogWarning($"[ServiceLocator] Service {type.Name} is already registered. Replacing...");
                }
                services[type] = service;
                UnityEngine.Debug.Log($"[ServiceLocator] Service {type.Name} registered successfully");
            }
        }
        
        /// <summary>
        /// ファクトリメソッドを登録（遅延初期化用）
        /// </summary>
        public static void RegisterFactory<T>(Func<T> factory) where T : class
        {
            lock (lockObject)
            {
                var type = typeof(T);
                factories[type] = () => factory();
                UnityEngine.Debug.Log($"[ServiceLocator] Factory for {type.Name} registered");
            }
        }
        
        /// <summary>
        /// サービスを取得
        /// </summary>
        public static T GetService<T>() where T : class
        {
            lock (lockObject)
            {
                var type = typeof(T);
                
                // 既に登録されているサービスがあれば返す
                if (services.TryGetValue(type, out var service))
                {
                    return service as T;
                }
                
                // ファクトリが登録されていれば、サービスを生成して登録
                if (factories.TryGetValue(type, out var factory))
                {
                    var newService = factory() as T;
                    if (newService != null)
                    {
                        services[type] = newService;
                        factories.Remove(type); // 一度生成したらファクトリは不要
                        return newService;
                    }
                }
                
                UnityEngine.Debug.LogWarning($"[ServiceLocator] Service {type.Name} not found");
                return null;
            }
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
        /// サービスが登録されているか確認
        /// </summary>
        public static bool HasService<T>() where T : class
        {
            lock (lockObject)
            {
                var type = typeof(T);
                return services.ContainsKey(type) || factories.ContainsKey(type);
            }
        }
        
        /// <summary>
        /// サービスが登録されているか確認（HasServiceのエイリアス）
        /// </summary>
        public static bool IsServiceRegistered<T>() where T : class
        {
            return HasService<T>();
        }
        
        /// <summary>
        /// 特定のサービスを削除
        /// </summary>
        public static void UnregisterService<T>() where T : class
        {
            lock (lockObject)
            {
                var type = typeof(T);
                services.Remove(type);
                factories.Remove(type);
                UnityEngine.Debug.Log($"[ServiceLocator] Service {type.Name} unregistered");
            }
        }
        
        /// <summary>
        /// すべてのサービスをクリア
        /// </summary>
        public static void Clear()
        {
            lock (lockObject)
            {
                services.Clear();
                factories.Clear();
                UnityEngine.Debug.Log("[ServiceLocator] All services cleared");
            }
        }
        
        /// <summary>
        /// 登録されているサービスの数を取得
        /// </summary>
        public static int GetServiceCount()
        {
            lock (lockObject)
            {
                return services.Count + factories.Count;
            }
        }
        
        /// <summary>
        /// デバッグ用：登録されているサービス一覧を出力
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void LogAllServices()
        {
            lock (lockObject)
            {
                UnityEngine.Debug.Log($"[ServiceLocator] === Registered Services ({services.Count}) ===");
                foreach (var kvp in services)
                {
                    UnityEngine.Debug.Log($"  - {kvp.Key.Name}: {kvp.Value.GetType().Name}");
                }
                
                UnityEngine.Debug.Log($"[ServiceLocator] === Registered Factories ({factories.Count}) ===");
                foreach (var kvp in factories)
                {
                    UnityEngine.Debug.Log($"  - {kvp.Key.Name}: [Factory]");
                }
            }
        }
    }
}