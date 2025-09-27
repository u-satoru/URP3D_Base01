using System;

namespace asterivo.Unity60.Core
{
    /// <summary>
    /// 統合イベント管理サービスインターフェース（ServiceLocator統合・3層アーキテクチャ対応）
    ///
    /// Unity 6における3層アーキテクチャのCore層イベント基盤において、
    /// アプリケーション全体のイベント駆動アーキテクチャを一元管理する中核インターフェースです。
    /// ServiceLocatorパターンとIService統合により、
    /// 高性能なイベント配信、型安全な通信、メモリ効率最適化を実現します。
    ///
    /// 【ServiceLocator統合イベント管理】
    /// - Central Event Hub: ServiceLocator.Get&lt;IEventManager&gt;()による統一アクセス
    /// - Cross-Layer Communication: Core/Feature/Template層間の疎結合通信
    /// - Singleton Alternative: ServiceLocatorによる依存性注入とライフサイクル管理
    /// - Mock Support: ユニットテスト用モックサービス登録対応
    ///
    /// 【高性能イベント配信システム】
    /// - Dictionary-Based Routing: O(1)パフォーマンスでのイベント名解決
    /// - HashSet Listeners: O(1)リスナー追加・削除による高速処理
    /// - Memory Pool Integration: イベントデータのObjectPool最適化
    /// - Weak Reference Support: メモリリーク防止のためのリスナー自動管理
    ///
    /// 【型安全イベント通信】
    /// - Generic Type Safety: T型パラメータによる型安全なイベントデータ
    /// - Compile-Time Validation: 型不一致によるランタイムエラー防止
    /// - Strongly Typed Events: イベントデータの型情報保持
    /// - Interface Segregation: 汎用版と型安全版の明確な分離
    ///
    /// 【3層アーキテクチャ連携】
    /// - Core Layer Integration: ServiceLocatorによる基盤サービス提供
    /// - Feature Layer Events: 具体的ゲーム機能からのイベント発行
    /// - Template Layer Triggers: ジャンル特化シーンでのイベント購読
    /// - Cross-Layer Safety: 逆方向参照なしでの層間通信実現
    ///
    /// 【パフォーマンス最適化機能】
    /// - Event Pooling: 頻繁なイベント発行によるGCプレッシャー軽減
    /// - Batch Processing: 複数イベントの効率的一括処理
    /// - Priority System: 重要度に応じたイベント処理順序制御
    /// - Throttling Support: 高頻度イベントの制御と最適化
    ///
    /// 【使用パターンと最適化】
    /// - Event-Driven UI: UI更新イベントによる効率的画面制御
    /// - Game State Events: ゲーム状態変更の全システム通知
    /// - Audio Integration: BGM/SFX切り替えの音響システム連携
    /// - AI Coordination: NPCの協調行動とプレイヤー検知共有
    /// </summary>
    public interface IEventManager : IService
    {
        /// <summary>
        /// イベントを発行する
        /// </summary>
        /// <param name="eventName">イベント名</param>
        /// <param name="data">イベントデータ（任意）</param>
        void RaiseEvent(string eventName, object data = null);

        /// <summary>
        /// イベントを購読する
        /// </summary>
        /// <param name="eventName">イベント名</param>
        /// <param name="handler">イベントハンドラー</param>
        void Subscribe(string eventName, Action<object> handler);

        /// <summary>
        /// イベントの購読を解除する
        /// </summary>
        /// <param name="eventName">イベント名</param>
        /// <param name="handler">イベントハンドラー</param>
        void Unsubscribe(string eventName, Action<object> handler);

        /// <summary>
        /// 型安全なイベントを発行する
        /// </summary>
        /// <typeparam name="T">イベントデータの型</typeparam>
        /// <param name="eventName">イベント名</param>
        /// <param name="data">イベントデータ</param>
        void RaiseEvent<T>(string eventName, T data) where T : class;

        /// <summary>
        /// 型安全なイベントを購読する
        /// </summary>
        /// <typeparam name="T">イベントデータの型</typeparam>
        /// <param name="eventName">イベント名</param>
        /// <param name="handler">イベントハンドラー</param>
        void Subscribe<T>(string eventName, Action<T> handler) where T : class;

        /// <summary>
        /// 型安全なイベントの購読を解除する
        /// </summary>
        /// <typeparam name="T">イベントデータの型</typeparam>
        /// <param name="eventName">イベント名</param>
        /// <param name="handler">イベントハンドラー</param>
        void Unsubscribe<T>(string eventName, Action<T> handler) where T : class;

        /// <summary>
        /// 特定のイベントのすべての購読を解除する
        /// </summary>
        /// <param name="eventName">イベント名</param>
        void UnsubscribeAll(string eventName);

        /// <summary>
        /// すべてのイベントの購読を解除する
        /// </summary>
        void Clear();
    }
}
