using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using asterivo.Unity60.Core;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Core
{
    /// <summary>
    /// 統合システム初期化マネージャー
    ///
    /// Unity 6における3層アーキテクチャシステムの統一的な初期化を管理する
    /// 中央制御クラスです。IInitializableインターフェースを実装したシステムを
    /// 自動探索し、優先度ベースの安全な初期化シーケンスを提供します。
    ///
    /// 【核心機能】
    /// - 自動システム探索: GetComponentsInChildren&lt;MonoBehaviour&gt;による包括的探索
    /// - 優先度制御: IInitializable.Priorityによる決定的な初期化順序
    /// - ServiceLocator統合: 自身をサービスとして自動登録
    /// - エラー処理: 個別システム初期化失敗時の継続実行
    /// - 状態監視: リアルタイム初期化進捗とデバッグ情報
    ///
    /// 【アーキテクチャ統合】
    /// - Core層の基盤として配置（最高優先度での初期化）
    /// - FeatureFlagsによる動的制御対応
    /// - Odin Inspectorエディタ拡張による開発支援
    /// - Unity Lifecycle（Awake→Start）との適切な統合
    ///
    /// 【使用パターン】
    /// - 起動時自動初期化: autoInitializeOnStart=true（推奨）
    /// - 手動初期化: InitializeAllSystems()メソッド呼び出し
    /// - 個別初期化: InitializeSystem&lt;T&gt;()による特定システム初期化
    /// - 状態確認: IsSystemInitialized&lt;T&gt;()、AreAllSystemsInitialized()
    ///
    /// 【デバッグ支援】
    /// - logInitializationStepsによる詳細ログ出力
    /// - LogSystemStatus()による現在状態表示
    /// - Odin ButtonAttribute活用のエディタUI
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
        /// IInitializableシステム自動探索・分類機能
        ///
        /// 子オブジェクト階層を再帰的にスキャンし、IInitializableインターフェースを
        /// 実装したMonoBehaviourコンポーネントを自動探索・登録します。
        /// 発見されたシステムは優先度順にソートされ、決定的な初期化順序を確立します。
        ///
        /// 【探索スコープ】
        /// - GetComponentsInChildren&lt;MonoBehaviour&gt;(true): 非アクティブ子オブジェクト含む
        /// - IInitializableインターフェース実装確認: is演算子による型安全チェック
        /// - 階層構造: 任意の深度まで再帰的探索
        ///
        /// 【ソート・分類処理】
        /// - OrderBy(s =&gt; s.Priority): 昇順ソート（低い値ほど優先）
        /// - initializableSystems: 型安全なIInitializableリスト
        /// - systemComponents: MonoBehaviourベースの管理リスト
        ///
        /// 【パフォーマンス特性】
        /// - 実行タイミング: Awake()での1回実行（起動時オーバーヘッド最小化）
        /// - 複雑度: O(n log n) - 子オブジェクト数のログ線形時間
        /// - メモリ効率: 重複オブジェクト参照なし
        ///
        /// 【ログ出力】
        /// logInitializationSteps && EnableDebugLogging条件下で詳細情報出力
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
        /// 全システム統合初期化実行メソッド
        ///
        /// 探索済みの全IInitializableシステムを優先度順に安全に初期化します。
        /// 個別システムの初期化失敗でも処理を継続し、システム全体の可用性を確保します。
        /// Odin ButtonAttributeによりエディタからの手動実行も可能です。
        ///
        /// 【実行フロー】
        /// 1. 重複実行チェック（isInitializedフラグによる冪等性保証）
        /// 2. 優先度順による順次初期化（OrderBy適用済みリスト使用）
        /// 3. 個別システム状態チェック（IsInitialized確認）
        /// 4. try-catchによる例外安全な初期化実行
        /// 5. 進捗カウンタ更新（initializedCount）
        /// 6. 全体完了フラグ設定（isInitialized = true）
        ///
        /// 【エラーハンドリング戦略】
        /// - 個別システム失敗: ログ出力後に処理継続（Fail-Fast回避）
        /// - 例外情報: LogError + LogExceptionによる詳細記録
        /// - システム状態: 失敗システムはIsInitialized=falseを維持
        ///
        /// 【パフォーマンス特性】
        /// - 実行時間: O(n) - システム数に線形比例
        /// - 冪等性: 複数回実行でも安全（状態チェックで早期リターン）
        /// - メモリ使用: 追加アロケーションなし
        ///
        /// 【ログ出力制御】
        /// logInitializationSteps && EnableDebugLoggingによる詳細ログ切り替え
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

