using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using asterivo.Unity60.Core;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Core
{
    /// <summary>
    /// システム初期化マネージャー
    /// 各システムを適切な順序で初期化
    /// </summary>
    public class SystemInitializer : MonoBehaviour
    {
        [Header("Initialization Settings")]
        [SerializeField] private bool autoInitializeOnStart = true;
        [SerializeField] private bool logInitializationSteps = true;
        
        [Header("Systems to Initialize")]
        [SerializeField, ReadOnly] 
        private List<MonoBehaviour> systemComponents = new List<MonoBehaviour>();
        
        [Header("Runtime Info")]
        [SerializeField, ReadOnly] private bool isInitialized = false;
        [SerializeField, ReadOnly] private int initializedCount = 0;
        [SerializeField, ReadOnly] private int totalSystemCount = 0;
        
        // 初期化可能なシステムのリスト
        private List<IInitializable> initializableSystems = new List<IInitializable>();
        // ServiceLocator登録対象のリスト（MonoBehaviourベースで管理）
        private List<MonoBehaviour> serviceRegistrants = new List<MonoBehaviour>();
        
        private void Awake()
        {
            // ServiceLocatorに登録
            if (FeatureFlags.UseServiceLocator)
            {
                ServiceLocator.RegisterService<SystemInitializer>(this);
            }
            
            DiscoverSystems();
            DiscoverAndRegisterServices();
        }
        
        private void Start()
        {
            if (autoInitializeOnStart)
            {
                InitializeAllSystems();
            }
        }
        
        private void OnDestroy()
        {
            // 簡単なクリーンアップ処理（詳細な解除処理は別の仕組みで対応）
            if (FeatureFlags.UseServiceLocator)
            {
                ServiceLocator.UnregisterService<SystemInitializer>();
            }
        }

        /// <summary>
        /// ServiceLocator登録対象を探索（簡略化版）
        /// </summary>
        private void DiscoverAndRegisterServices()
        {
            // 簡略化された実装（必要に応じて将来拡張）
            if (logInitializationSteps && FeatureFlags.EnableDebugLogging)
            {
                UnityEngine.Debug.Log("[SystemInitializer] Service discovery completed (simplified implementation)");
            }
        }
        
        /// <summary>
        /// 初期化可能なシステムを探索
        /// </summary>
        private void DiscoverSystems()
        {
            // 子オブジェクトから初期化可能なコンポーネントを探索
            var components = GetComponentsInChildren<MonoBehaviour>(true);
            
            foreach (var component in components)
            {
                if (component is IInitializable initializable)
                {
                    initializableSystems.Add(initializable);
                    systemComponents.Add(component);
                }
            }
            
            // 優先度でソート
            initializableSystems = initializableSystems.OrderBy(s => s.Priority).ToList();
            totalSystemCount = initializableSystems.Count;
            
            if (logInitializationSteps && FeatureFlags.EnableDebugLogging)
            {
                UnityEngine.Debug.Log($"[SystemInitializer] Discovered {totalSystemCount} systems to initialize");
                foreach (var system in initializableSystems)
                {
                    UnityEngine.Debug.Log($"  - {system.GetType().Name} (Priority: {system.Priority})");
                }
            }
        }
        
        /// <summary>
        /// すべてのシステムを初期化
        /// </summary>
        [Button("Initialize All Systems")]
        public void InitializeAllSystems()
        {
            if (isInitialized)
            {
                UnityEngine.Debug.LogWarning("[SystemInitializer] Systems already initialized");
                return;
            }
            
            initializedCount = 0;
            
            foreach (var system in initializableSystems)
            {
                try
                {
                    if (!system.IsInitialized)
                    {
                        if (logInitializationSteps && FeatureFlags.EnableDebugLogging)
                        {
                            UnityEngine.Debug.Log($"[SystemInitializer] Initializing {system.GetType().Name}...");
                        }
                        
                        system.Initialize();
                        initializedCount++;
                        
                        if (logInitializationSteps && FeatureFlags.EnableDebugLogging)
                        {
                            UnityEngine.Debug.Log($"[SystemInitializer] {system.GetType().Name} initialized successfully");
                        }
                    }
                }
                catch (System.Exception e)
                {
                    UnityEngine.Debug.LogError($"[SystemInitializer] Failed to initialize {system.GetType().Name}: {e.Message}");
                    UnityEngine.Debug.LogException(e);
                }
            }
            
            isInitialized = true;
            
            if (FeatureFlags.EnableDebugLogging)
            {
                UnityEngine.Debug.Log($"[SystemInitializer] Initialization complete. {initializedCount}/{totalSystemCount} systems initialized");
            }
        }
        
        /// <summary>
        /// 特定のシステムを初期化
        /// </summary>
        public void InitializeSystem<T>() where T : IInitializable
        {
            var system = initializableSystems.FirstOrDefault(s => s is T);
            if (system != null && !system.IsInitialized)
            {
                system.Initialize();
                
                if (FeatureFlags.EnableDebugLogging)
                {
                    UnityEngine.Debug.Log($"[SystemInitializer] {typeof(T).Name} initialized");
                }
            }
        }
        
        /// <summary>
        /// システムの初期化状態を確認
        /// </summary>
        public bool IsSystemInitialized<T>() where T : IInitializable
        {
            var system = initializableSystems.FirstOrDefault(s => s is T);
            return system?.IsInitialized ?? false;
        }
        
        /// <summary>
        /// すべてのシステムが初期化されているか確認
        /// </summary>
        public bool AreAllSystemsInitialized()
        {
            return initializableSystems.All(s => s.IsInitialized);
        }
        
        /// <summary>
        /// 初期化状態をリセット（デバッグ用）
        /// </summary>
        [Button("Reset Initialization", ButtonSizes.Large)]
        [PropertySpace(SpaceBefore = 10)]
        private void ResetInitialization()
        {
            isInitialized = false;
            initializedCount = 0;
            
            if (FeatureFlags.EnableDebugLogging)
            {
                UnityEngine.Debug.Log("[SystemInitializer] Initialization state reset");
            }
        }
        
        /// <summary>
        /// 初期化状態のデバッグ情報を出力
        /// </summary>
        [Button("Log System Status")]
        public void LogSystemStatus()
        {
            UnityEngine.Debug.Log($"[SystemInitializer] === System Status ===");
            UnityEngine.Debug.Log($"Total Systems: {totalSystemCount}");
            UnityEngine.Debug.Log($"Initialized: {initializedCount}");
            UnityEngine.Debug.Log($"Is Fully Initialized: {isInitialized}");
            
            foreach (var system in initializableSystems)
            {
                var status = system.IsInitialized ? "✓" : "✗";
                UnityEngine.Debug.Log($"  {status} {system.GetType().Name} (Priority: {system.Priority})");
            }
        }
    }
}

