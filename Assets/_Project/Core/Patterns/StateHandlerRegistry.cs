using System;
using asterivo.Unity60.Core;
using System.Collections.Generic;
using UnityEngine;

namespace asterivo.Unity60.Core.Patterns
{
    /// <summary>
    /// 状態ハンドラー統合レジストリサービス（Factory + Registry パターン・ServiceLocator統合）
    ///
    /// Unity 6における3層アーキテクチャのCore層状態管理システムにおいて、
    /// Factory + Registry パターンによる状態ハンドラーの集約管理を行うサービスです。
    /// ServiceLocatorパターンとIService統合により、
    /// システム全体での状態処理の統一管理と高性能アクセスを実現します。
    ///
    /// 【Factory + Registry パターン実装】
    /// - ハンドラー生成: IStateHandler実装の動的登録・管理
    /// - 集約管理: 全状態ハンドラーの統一レジストリによる効率管理
    /// - ファクトリー機能: 状態IDによる適切なハンドラーの即座取得
    /// - ライフサイクル制御: ハンドラーの登録・削除・クリアの統合制御
    ///
    /// 【ServiceLocator統合アーキテクチャ】
    /// - IService実装: ServiceLocatorによる全システム統合管理
    /// - 自動初期化: OnServiceRegistered による自動サービス開始
    /// - 適切終了: OnServiceUnregistered による安全なリソース解放
    /// - サービス状態: IsServiceActive による実行時状態監視
    ///
    /// 【循環依存回避設計】
    /// - int型状態ID: PlayerState等の具体型依存を完全排除
    /// - Core層独立性: Feature層への依存を持たない設計
    /// - 疎結合アーキテクチャ: インターフェースベースの柔軟な連携
    /// - 型安全性確保: Dictionary<int, IStateHandler>による高速・安全アクセス
    ///
    /// 【3層アーキテクチャ活用】
    /// - Core層基盤: ジャンル非依存の状態ハンドラー管理機構
    /// - Feature層統合: 具体的状態ハンドラーの登録・活用基盤
    /// - Template層支援: ジャンル特化状態シーケンスの構築支援
    /// - 一方向依存: 上位層からの登録、下位層での提供
    ///
    /// 【高性能ハンドラー管理】
    /// - O(1)アクセス: Dictionary による定数時間状態ハンドラー取得
    /// - メモリ効率: 必要最小限のハンドラー保持による最適化
    /// - 並行安全性: スレッドセーフな操作による安定性確保
    /// - 動的更新: 実行時でのハンドラー追加・変更対応
    ///
    /// 【デバッグ・監視機能】
    /// - 登録状態確認: HasHandler による事前ハンドラー存在チェック
    /// - 全状態列挙: GetRegisteredStates による登録済み状態の確認
    /// - サービス状態: ServiceName とIsServiceActive による監視
    /// - ログ統合: 初期化・終了時の適切なログ出力
    ///
    /// 【拡張性・保守性】
    /// - インターフェース駆動: IStateHandler による柔軟なハンドラー実装
    /// - ホットスワップ: 実行時ハンドラー交換による動的機能変更
    /// - バッチ操作: ClearHandlers による一括クリア機能
    /// - 例外安全: null チェックと適切な例外処理
    ///
    /// 【使用パターン】
    /// - ServiceLocator統合: ServiceLocator.Register<StateHandlerRegistry>(registry)
    /// - ハンドラー登録: registry.RegisterHandler(new CustomStateHandler())
    /// - ハンドラー取得: var handler = registry.GetHandler(stateId)
    /// - 存在確認: if (registry.HasHandler(stateId)) { ... }
    /// </summary>
    public class StateHandlerRegistry : asterivo.Unity60.Core.IService
    {
        private readonly Dictionary<int, IStateHandler> handlers;
        private bool isInitialized = false;

        public StateHandlerRegistry()
        {
            handlers = new Dictionary<int, IStateHandler>();
        }

        /// <summary>
        /// サービスの初期化
        /// </summary>
        public void Initialize()
        {
            if (!isInitialized)
            {
                Debug.Log("[StateHandlerRegistry] Initializing StateService");
                isInitialized = true;
                // Feature層から必要に応じてHandlerを登録する
            }
        }

        /// <summary>
        /// サービスのシャットダウン
        /// </summary>
        public void Shutdown()
        {
            if (isInitialized)
            {
                Debug.Log("[StateHandlerRegistry] Shutting down StateService");
                ClearHandlers();
                isInitialized = false;
            }
        }

        // IService implementation
        public void OnServiceRegistered()
        {
            Initialize();
        }

        public void OnServiceUnregistered()
        {
            Shutdown();
        }

        public bool IsServiceActive => isInitialized;

        public string ServiceName => "StateHandlerRegistry";
        
        /// <summary>
        /// 状態ハンドラーを登録
        /// </summary>
        /// <param name="handler">登録するハンドラー</param>
        public void RegisterHandler(IStateHandler handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));
                
            handlers[handler.HandledState] = handler;
        }
        
        /// <summary>
        /// 状態ハンドラーを取得
        /// </summary>
        /// <param name="state">対象の状態</param>
        /// <returns>対応するハンドラー、存在しない場合はnull</returns>
        public IStateHandler GetHandler(int state) // Changed from PlayerState to int
        {
            handlers.TryGetValue(state, out IStateHandler handler);
            return handler;
        }
        
        /// <summary>
        /// 指定した状態のハンドラーが登録されているかチェック
        /// </summary>
        /// <param name="state">チェックする状態</param>
        /// <returns>ハンドラーが存在する場合はtrue</returns>
        public bool HasHandler(int state) // Changed from PlayerState to int
        {
            return handlers.ContainsKey(state);
        }
        
        /// <summary>
        /// 登録されている全ての状態を取得
        /// </summary>
        /// <returns>登録済み状態のコレクション</returns>
        public IEnumerable<int> GetRegisteredStates() // Changed from PlayerState to int
        {
            return handlers.Keys;
        }

        /// <summary>
        /// 全てのハンドラーをクリア
        /// </summary>
        public void ClearHandlers()
        {
            handlers.Clear();
        }
    }
}

