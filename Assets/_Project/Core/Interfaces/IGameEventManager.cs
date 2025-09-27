using asterivo.Unity60.Core.Events;

namespace asterivo.Unity60.Core.Services
{
    /// <summary>
    /// GameEventレジストリ管理サービスインターフェース（ScriptableObject統合・3層アーキテクチャ対応）
    ///
    /// Unity 6における3層アーキテクチャのCore層GameEvent基盤において、
    /// ScriptableObjectベースのGameEventを一元登録・管理する中核インターフェースです。
    /// ServiceLocatorパターンとIService統合により、
    /// 高性能なGameEvent検索、型安全な登録、デバッグ支援機能を実現します。
    ///
    /// 【ScriptableObject統合GameEvent管理】
    /// - GameEvent Registry: ScriptableObjectアセットの中央登録・検索
    /// - Asset-Based Events: Inspectorでの直感的GameEvent設定
    /// - Runtime Registration: 動的GameEvent生成と登録システム
    /// - Type Safety: ジェネリック型による型安全なGameEvent管理
    ///
    /// 【ServiceLocator統合アーキテクチャ】
    /// - Central Registry Hub: ServiceLocator.Get&lt;IGameEventManager&gt;()による統一アクセス
    /// - Cross-Layer Management: Core/Feature/Template層での統一GameEvent管理
    /// - Singleton Alternative: ServiceLocatorによる依存性注入とライフサイクル管理
    /// - Mock Support: ユニットテスト用モックレジストリ登録対応
    ///
    /// 【高性能GameEvent検索システム】
    /// - Dictionary-Based Lookup: O(1)パフォーマンスでのGameEvent名前解決
    /// - Generic Type Caching: 型情報によるGameEvent高速検索
    /// - Memory Efficient: 弱参照によるメモリリーク防止
    /// - Batch Operations: 複数GameEventの効率的一括処理
    ///
    /// 【3層アーキテクチャ統合設計】
    /// - Core Layer Foundation: GameEventの基盤レジストリ機能提供
    /// - Feature Layer Integration: 具体機能でのGameEvent登録・使用
    /// - Template Layer Configuration: ジャンル特化GameEvent設定
    /// - Designer Friendly: ScriptableObjectによるノンプログラマー対応
    ///
    /// 【デバッグ・統計機能】
    /// - Event Statistics: 登録数、発行回数、パフォーマンス統計
    /// - Event Logging: 開発時のイベントフロー追跡
    /// - Registration Validation: 重複登録・型不整合の検出
    /// - Runtime Inspection: エディタでのリアルタイムGameEvent状態表示
    ///
    /// 【使用パターンとベストプラクティス】
    /// - Asset-Based Setup: ProjectSettings/GameEvents/フォルダでの事前登録
    /// - Runtime Discovery: GetEvent&lt;T&gt;()による型安全なGameEvent取得
    /// - Event-Driven UI: UIコンポーネントとGameEventの自動連携
    /// - Game State Integration: ゲーム状態変更のGameEvent自動発行
    /// </summary>
    public interface IGameEventManager : IService
    {
        /// <summary>
        /// Register a game event
        /// </summary>
        /// <param name="eventName">Name of the event</param>
        /// <param name="gameEvent">Game event instance</param>
        void RegisterEvent(string eventName, GameEvent gameEvent);

        /// <summary>
        /// Register a generic game event
        /// </summary>
        /// <typeparam name="T">Type of event data</typeparam>
        /// <param name="eventName">Name of the event</param>
        /// <param name="gameEvent">Generic game event instance</param>
        void RegisterEvent<T>(string eventName, GameEvent<T> gameEvent);

        /// <summary>
        /// Unregister a game event
        /// </summary>
        /// <param name="eventName">Name of the event to unregister</param>
        void UnregisterEvent(string eventName);

        /// <summary>
        /// Get a registered game event
        /// </summary>
        /// <param name="eventName">Name of the event</param>
        /// <returns>Game event instance or null if not found</returns>
        GameEvent GetEvent(string eventName);

        /// <summary>
        /// Get a registered generic game event
        /// </summary>
        /// <typeparam name="T">Type of event data</typeparam>
        /// <param name="eventName">Name of the event</param>
        /// <returns>Generic game event instance or null if not found</returns>
        GameEvent<T> GetEvent<T>(string eventName);

        /// <summary>
        /// Raise a game event by name
        /// </summary>
        /// <param name="eventName">Name of the event to raise</param>
        void RaiseEvent(string eventName);

        /// <summary>
        /// Raise a generic game event by name
        /// </summary>
        /// <typeparam name="T">Type of event data</typeparam>
        /// <param name="eventName">Name of the event to raise</param>
        /// <param name="data">Event data</param>
        void RaiseEvent<T>(string eventName, T data);

        /// <summary>
        /// Check if an event is registered
        /// </summary>
        /// <param name="eventName">Name of the event</param>
        /// <returns>True if event is registered</returns>
        bool HasEvent(string eventName);

        /// <summary>
        /// Get all registered event names
        /// </summary>
        /// <returns>Array of event names</returns>
        string[] GetRegisteredEventNames();

        /// <summary>
        /// Clear all registered events
        /// </summary>
        void ClearAllEvents();

        /// <summary>
        /// Get event statistics for debugging
        /// </summary>
        /// <returns>Event statistics as formatted string</returns>
        string GetEventStatistics();

        /// <summary>
        /// Enable or disable event logging
        /// </summary>
        /// <param name="enabled">Whether to enable event logging</param>
        void SetEventLogging(bool enabled);

        /// <summary>
        /// Check if event logging is enabled
        /// </summary>
        bool IsEventLoggingEnabled();
    }
}