namespace asterivo.Unity60.Core
{
    /// <summary>
    /// ServiceLocatorパターン基幹サービスインターフェース（ライフサイクル管理・3層アーキテクチャ対応）
    ///
    /// Unity 6における3層アーキテクチャのCore層インターフェース基盤において、
    /// 全サービスの統一ライフサイクル管理を定義する基本インターフェースです。
    /// Singletonパターンを完全排除したServiceLocatorパターンの中核として、
    /// サービスの登録・解除・状態管理の統一コントラクトを提供します。
    ///
    /// 【ライフサイクル管理システム】
    /// - OnServiceRegistered: ServiceLocator登録時の自動初期化フック
    /// - OnServiceUnregistered: ServiceLocator解除時の自動クリーンアップフック
    /// - IsServiceActive: サービス状態のリアルタイム監視
    /// - ServiceName: デバッグ・ログ出力用の統一名称管理
    ///
    /// 【Singletonパターン排除設計】
    /// - 全サービスがServiceLocator経由でアクセス必須
    /// - staticインスタンスの完全禁止と依存性明確化
    /// - ユニットテスト対応のためのモックサービス登録対応
    /// - ライフサイクルの明確な制御とリソース管理
    /// </summary>
    public interface IService
    {
        /// <summary>
        /// Called when the service is first registered with the ServiceLocator
        /// Override this to perform initialization logic
        /// </summary>
        void OnServiceRegistered() { }

        /// <summary>
        /// Called when the service is unregistered from the ServiceLocator
        /// Override this to perform cleanup logic
        /// </summary>
        void OnServiceUnregistered() { }

        /// <summary>
        /// Gets whether this service is currently active and ready to use
        /// </summary>
        bool IsServiceActive => true;

        /// <summary>
        /// Gets a descriptive name for this service (used for debugging)
        /// </summary>
        string ServiceName => GetType().Name;
    }

    /// <summary>
    /// 型安全サービスインターフェース（ジェネリック型識別・自己参照対応）
    ///
    /// Unity 6における3層アーキテクチャのCore層インターフェース基盤において、
    /// 型安全なサービス識別と自己参照機能を提供する拡張インターフェースです。
    /// ServiceLocatorパターンでの型安全なアクセスと、
    /// サービス内部での自己参照が必要な高度なシナリオに対応します。
    /// </summary>
    /// <typeparam name="T">サービス型（class制約とIService&lt;T&gt;実装制約付き）</typeparam>
    public interface IService<T> : IService where T : class, IService<T>
    {
        /// <summary>
        /// Gets the singleton instance of this service
        /// This is primarily used for services that need to reference themselves
        /// </summary>
        T Instance => this as T;
    }

    /// <summary>
    /// 設定データ初期化サービスインターフェース（ScriptableObject連携・データ駆動設計）
    ///
    /// Unity 6における3層アーキテクチャのCore層インターフェース基盤において、
    /// ScriptableObjectベースの設定データで初期化が必要なサービス用の特化インターフェースです。
    /// データ駆動設計の中核として、ノンプログラマーによる設定変更と、
    /// サービスの柔軟な初期化制御を実現します。
    /// </summary>
    /// <typeparam name="TConfig">設定データ型（class制約付き、通常ScriptableObject継承）</typeparam>
    public interface IConfigurableService<TConfig> : IService where TConfig : class
    {
        /// <summary>
        /// Initialize the service with configuration data
        /// </summary>
        /// <param name="config">The configuration data</param>
        void Initialize(TConfig config);

        /// <summary>
        /// Gets whether this service has been initialized
        /// </summary>
        bool IsInitialized { get; }
    }

    /// <summary>
    /// リソース管理サービスインターフェース（IDisposable統合・メモリリーク防止）
    ///
    /// Unity 6における3層アーキテクチャのCore層インターフェース基盤において、
    /// ネイティブリソースや大量データを扱うサービス用の特化インターフェースです。
    /// .NETのIDisposableパターンとServiceLocatorパターンの統合により、
    /// メモリリーク防止とリソースの確実な解放を保証します。
    /// </summary>
    public interface IDisposableService : IService, System.IDisposable
    {
        /// <summary>
        /// Gets whether this service has been disposed
        /// </summary>
        bool IsDisposed { get; }
    }

    /// <summary>
    /// アップデートループ管理サービスインターフェース（フレームレート対応・優先度制御）
    ///
    /// Unity 6における3層アーキテクチャのCore層インターフェース基盤において、
    /// 毎フレームの更新処理が必要なサービス用の特化インターフェースです。
    /// MonoBehaviourのUpdateメソッドに依存せずServiceLocator経由での統一管理により、
    /// パフォーマンス最適化と優先度制御を実現します。
    /// </summary>
    public interface IUpdatableService : IService
    {
        /// <summary>
        /// Called every frame to update the service
        /// </summary>
        void UpdateService();

        /// <summary>
        /// Gets whether this service needs to be updated every frame
        /// </summary>
        bool NeedsUpdate { get; }

        /// <summary>
        /// Gets the update priority (lower values update first)
        /// </summary>
        int UpdatePriority => 0;
    }

    /// <summary>
    /// ポーズ・レジューム制御サービスインターフェース（ゲーム状態連携・ユニフォーム制御）
    ///
    /// Unity 6における3層アーキテクチャのCore層インターフェース基盤において、
    /// ゲームポーズ時の一時停止・再開機能が必要なサービス用の特化インターフェースです。
    /// ゲーム状態管理との統合により、オーディオ、AI、アニメーション等の
    /// 統一されたポーズ・レジューム制御を実現します。
    /// </summary>
    public interface IPausableService : IService
    {
        /// <summary>
        /// Pause the service
        /// </summary>
        void Pause();

        /// <summary>
        /// Resume the service
        /// </summary>
        void Resume();

        /// <summary>
        /// Gets whether this service is currently paused
        /// </summary>
        bool IsPaused { get; }
    }
}