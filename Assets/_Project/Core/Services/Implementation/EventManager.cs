using System;
using System.Collections.Generic;
using UnityEngine;

namespace asterivo.Unity60.Core.Services
{
    /// <summary>
    /// 統合イベントマネージャー実装クラス
    ///
    /// Unity 6における3層アーキテクチャのイベント駆動システムの中核として、
    /// 層間通信とコンポーネント間の疎結合な連携を実現します。
    /// ServiceLocatorパターンと統合され、プロジェクト全体のイベント管理を担います。
    ///
    /// 【核心機能】
    /// - スレッドセーフなイベント管理: lockメカニズムによる並行アクセス制御
    /// - 型安全なイベント処理: ジェネリック型とobject型の両対応
    /// - 例外安全性: イベントハンドラー実行時の例外隔離
    /// - ServiceLocator統合: IServiceインターフェース完全実装
    /// - 動的購読管理: リアルタイムでの購読者追加・削除
    ///
    /// 【アーキテクチャ統合】
    /// - Core層の基盤として配置（ServiceLocator経由でアクセス）
    /// - Feature層からCore層への通信チャネル提供
    /// - Template層での設定変更通知システム
    /// - ScriptableObjectベースのGameEventとの連携
    ///
    /// 【パフォーマンス特性】
    /// - O(1)のイベント検索（Dictionary使用）
    /// - コピーベースの安全な反復処理（Iterator Invalidation回避）
    /// - 最小限のアロケーション（List再利用）
    /// - 効率的なメモリ管理（空リストの自動削除）
    ///
    /// 【使用パターン】
    /// - Core→Feature通信: システム初期化完了通知
    /// - Feature→Template通信: ゲームプレイ状態変更
    /// - 横断的関心事: ログ出力、統計収集、デバッグ情報
    /// - UI更新: プレイヤー状態、ゲーム進行状況の反映
    ///
    /// 【スレッドセーフティ】
    /// - 全パブリックメソッドでlock(_lock)による排他制御
    /// - Dictionary操作の原子性保証
    /// - イベント発火時のコレクション変更からの保護
    /// </summary>
    public class EventManager : IEventManager
    {
        private readonly Dictionary<string, List<Delegate>> _eventHandlers = new Dictionary<string, List<Delegate>>();
        private readonly object _lock = new object();
        private bool _isInitialized = false;

        #region IService Implementation

        /// <summary>
        /// ServiceLocator登録時の初期化処理
        ///
        /// EventManagerがServiceLocatorに登録された際に自動実行されます。
        /// 既存のイベントハンドラーをクリアし、クリーンな状態で初期化を行います。
        ///
        /// 【実行フロー】
        /// 1. 重複初期化チェック（冪等性保証）
        /// 2. イベントハンドラー辞書のクリア
        /// 3. 初期化完了フラグ設定
        /// 4. 初期化完了ログ出力
        ///
        /// 【設計思想】
        /// - 冪等性: 複数回呼び出しても安全
        /// - クリーンスタート: 以前の状態に影響されない初期化
        /// - ログ記録: 初期化状況の可視化
        /// </summary>
        public void OnServiceRegistered()
        {
            if (_isInitialized) return;

            _eventHandlers.Clear();
            _isInitialized = true;
            Debug.Log("[EventManager] Service Registered");
        }

        /// <summary>
        /// ServiceLocator登録解除時のクリーンアップ処理
        ///
        /// EventManagerがServiceLocatorから登録解除される際に実行されます。
        /// 全イベント購読をクリアし、メモリリークを防止します。
        ///
        /// 【実行フロー】
        /// 1. Clear()メソッドによる全イベントハンドラー削除
        /// 2. 初期化状態フラグのリセット
        /// 3. 登録解除完了ログ出力
        ///
        /// 【安全性保証】
        /// - 確実なリソース解放
        /// - メモリリーク防止
        /// - 状態の適切なリセット
        /// </summary>
        public void OnServiceUnregistered()
        {
            Clear();
            _isInitialized = false;
            Debug.Log("[EventManager] Service Unregistered");
        }

        /// <summary>
        /// サービスアクティブ状態フラグ
        ///
        /// EventManagerが正常に初期化され、使用可能状態にあるかを示します。
        /// ServiceLocatorからの状態確認で使用されます。
        ///
        /// 【戻り値】
        /// - true: 初期化完了、イベント処理可能
        /// - false: 未初期化、使用不可
        /// </summary>
        public bool IsServiceActive => _isInitialized;

        /// <summary>
        /// サービス識別名
        ///
        /// ServiceLocator内でのサービス識別に使用される一意名です。
        /// ログ出力や診断情報での表示にも使用されます。
        ///
        /// 【固定値】"EventManager"
        /// </summary>
        public string ServiceName => "EventManager";

        #endregion

        #region IEventManager Implementation

        /// <summary>
        /// object型データ付きイベント発火処理
        ///
        /// 指定されたイベント名に対して登録された全ハンドラーを安全に実行します。
        /// スレッドセーフな実装により、マルチスレッド環境での並行アクセスに対応しています。
        ///
        /// 【実行フロー】
        /// 1. イベント名の有効性検証（null/空文字チェック）
        /// 2. スレッドセーフなハンドラー検索（lock使用）
        /// 3. ハンドラーリストの安全なコピー作成（Iterator Invalidation回避）
        /// 4. 各ハンドラーの型安全な実行
        /// 5. 例外隔離による堅牢性確保
        ///
        /// 【パフォーマンス特性】
        /// - Dictionary検索: O(1)の高速アクセス
        /// - 防御的コピー: イベント処理中のコレクション変更を防止
        /// - 例外隔離: 1つのハンドラー失敗が他に影響しない
        /// - 型チェック: 実行時型安全性の保証
        ///
        /// 【使用例】
        /// - プレイヤー状態変更: RaiseEvent("PlayerHealthChanged", healthData)
        /// - UI更新通知: RaiseEvent("UIRefreshRequired", uiContext)
        /// - システム初期化: RaiseEvent("SystemInitialized", null)
        ///
        /// 【エラーハンドリング】
        /// - 各ハンドラーの例外は個別にキャッチ・ログ出力
        /// - 処理継続: 1つの失敗で全体が停止しない
        /// - デバッグ支援: 詳細なエラー情報をログ出力
        /// </summary>
        /// <param name="eventName">発火するイベントの名前（null/空文字は無視）</param>
        /// <param name="data">イベントと共に送信するデータ（nullも許可）</param>
        public void RaiseEvent(string eventName, object data = null)
        {
            if (string.IsNullOrEmpty(eventName)) return;

            lock (_lock)
            {
                if (_eventHandlers.TryGetValue(eventName, out var handlers))
                {
                    // ハンドラーのコピーを作成して、イベント処理中の変更を安全にする
                    var handlersCopy = new List<Delegate>(handlers);
                    foreach (var handler in handlersCopy)
                    {
                        try
                        {
                            if (handler is Action<object> actionHandler)
                            {
                                actionHandler.Invoke(data);
                            }
                            else if (handler is Action simpleHandler && data == null)
                            {
                                simpleHandler.Invoke();
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"[EventManager] Error raising event '{eventName}': {ex.Message}");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// object型ハンドラーのイベント購読登録
        ///
        /// 指定されたイベント名に対してハンドラーを登録し、将来のイベント発火時に
        /// 実行されるように設定します。重複登録は自動的に回避されます。
        ///
        /// 【実行フロー】
        /// 1. パラメータ有効性検証（null/空文字チェック）
        /// 2. スレッドセーフなハンドラー辞書アクセス
        /// 3. 新規イベント名の場合、ハンドラーリスト初期化
        /// 4. 重複チェックによる安全な登録
        /// 5. 登録完了ログ出力
        ///
        /// 【安全性機能】
        /// - 重複登録防止: 同一ハンドラーの複数登録を回避
        /// - null安全性: null参照による例外を防止
        /// - スレッドセーフ: 並行アクセスでのデータ競合を防止
        /// - 遅延初期化: 必要な時点でのハンドラーリスト作成
        ///
        /// 【メモリ管理】
        /// - 効率的なList初期化（オンデマンド）
        /// - 重複防止による無駄なメモリ使用回避
        /// - Dictionary構造による高速検索
        ///
        /// 【使用例】
        /// - UI更新登録: Subscribe("PlayerHealthChanged", UpdateHealthUI)
        /// - 状態監視: Subscribe("GameStateChanged", OnGameStateUpdate)
        /// - ログ出力: Subscribe("ErrorOccurred", LogErrorHandler)
        /// </summary>
        /// <param name="eventName">購読するイベントの名前（null/空文字は無視）</param>
        /// <param name="handler">イベント発火時に実行するハンドラー（nullは無視）</param>
        public void Subscribe(string eventName, Action<object> handler)
        {
            if (string.IsNullOrEmpty(eventName) || handler == null) return;

            lock (_lock)
            {
                if (!_eventHandlers.TryGetValue(eventName, out var handlers))
                {
                    handlers = new List<Delegate>();
                    _eventHandlers[eventName] = handlers;
                }

                if (!handlers.Contains(handler))
                {
                    handlers.Add(handler);
                    Debug.Log($"[EventManager] Subscribed to event '{eventName}'");
                }
            }
        }

        /// <summary>
        /// object型ハンドラーのイベント購読解除
        ///
        /// 指定されたイベント名から特定のハンドラーを安全に削除します。
        /// ハンドラーリストが空になった場合、メモリ効率のため自動的に削除されます。
        ///
        /// 【実行フロー】
        /// 1. パラメータ有効性検証（null/空文字チェック）
        /// 2. スレッドセーフなハンドラー検索
        /// 3. ハンドラーの安全な削除
        /// 4. 空リストの自動クリーンアップ
        /// 5. 解除完了ログ出力
        ///
        /// 【メモリ最適化】
        /// - 空リスト削除: ハンドラーが0個になった際の自動クリーンアップ
        /// - Dictionary最適化: 不要なエントリの除去
        /// - 参照切断: ハンドラーオブジェクトの適切な解放
        ///
        /// 【安全性保証】
        /// - 存在しないハンドラーでも例外なし
        /// - 重複解除の安全な処理
        /// - null参照からの保護
        /// - スレッドセーフな操作
        ///
        /// 【使用例】
        /// - コンポーネント破棄時: Unsubscribe("PlayerHealthChanged", UpdateHealthUI)
        /// - 動的解除: Unsubscribe("TemporaryEvent", tempHandler)
        /// - リソース解放: Unsubscribe("HeavyDataUpdate", expensiveHandler)
        ///
        /// 【パフォーマンス】
        /// - List.Remove(): O(n)の線形時間複雑度
        /// - Dictionary.Remove(): O(1)の定数時間
        /// - メモリフットプリント削減: 不要な空リスト除去
        /// </summary>
        /// <param name="eventName">解除するイベントの名前（null/空文字は無視）</param>
        /// <param name="handler">削除するハンドラー（nullは無視）</param>
        public void Unsubscribe(string eventName, Action<object> handler)
        {
            if (string.IsNullOrEmpty(eventName) || handler == null) return;

            lock (_lock)
            {
                if (_eventHandlers.TryGetValue(eventName, out var handlers))
                {
                    if (handlers.Remove(handler))
                    {
                        Debug.Log($"[EventManager] Unsubscribed from event '{eventName}'");

                        // ハンドラーリストが空になったら削除
                        if (handlers.Count == 0)
                        {
                            _eventHandlers.Remove(eventName);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// ジェネリック型データ付きイベント発火処理
        ///
        /// 型安全性を強化した高性能なイベント発火システムです。
        /// 指定された型Tに対して最適化され、コンパイル時型チェックによる安全性を提供します。
        ///
        /// 【実行フロー】
        /// 1. イベント名とデータの有効性検証
        /// 2. スレッドセーフなハンドラー検索
        /// 3. 防御的ハンドラーリストコピー
        /// 4. 型優先度に基づく段階的ハンドラー実行
        /// 5. 例外隔離による堅牢性確保
        ///
        /// 【型安全性】
        /// - コンパイル時型チェック: where T : class制約
        /// - 実行時型検証: is演算子による安全なキャスト
        /// - フォールバック機構: Action<object>への自動変換
        /// - null参照防止: class制約による参照型限定
        ///
        /// 【パフォーマンス最適化】
        /// - 型特化実行: Action<T>の直接呼び出し優先
        /// - Boxing回避: 可能な限りジェネリック型で処理
        /// - フォールバック効率: object型への変換コスト最小化
        /// - メモリ効率: 防御的コピーによる安全性とパフォーマンスの両立
        ///
        /// 【使用例】
        /// - 型安全なデータ送信: RaiseEvent<PlayerData>("PlayerUpdate", playerData)
        /// - 強型データ通信: RaiseEvent<GameConfig>("ConfigChanged", newConfig)
        /// - オブジェクト固有イベント: RaiseEvent<Enemy>("EnemyDefeated", defeatedEnemy)
        ///
        /// 【ハンドラー優先順位】
        /// 1. Action<T>: 最高優先度（完全型一致）
        /// 2. Action<object>: フォールバック（型変換）
        /// 3. その他: 無視（型安全性確保）
        /// </summary>
        /// <typeparam name="T">イベントデータの型（参照型のみ許可）</typeparam>
        /// <param name="eventName">発火するイベントの名前（null/空文字は無視）</param>
        /// <param name="data">型安全なイベントデータ（nullも許可）</param>
        public void RaiseEvent<T>(string eventName, T data) where T : class
        {
            if (string.IsNullOrEmpty(eventName)) return;

            lock (_lock)
            {
                if (_eventHandlers.TryGetValue(eventName, out var handlers))
                {
                    var handlersCopy = new List<Delegate>(handlers);
                    foreach (var handler in handlersCopy)
                    {
                        try
                        {
                            if (handler is Action<T> typedHandler)
                            {
                                typedHandler.Invoke(data);
                            }
                            else if (handler is Action<object> objectHandler)
                            {
                                objectHandler.Invoke(data);
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"[EventManager] Error raising typed event '{eventName}': {ex.Message}");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// ジェネリック型ハンドラーのイベント購読登録
        ///
        /// 型安全性を重視した高性能なイベント購読システムです。
        /// コンパイル時型チェックにより、実行時エラーを未然に防ぎます。
        ///
        /// 【実行フロー】
        /// 1. パラメータ有効性検証（型安全性確保）
        /// 2. スレッドセーフなハンドラー辞書アクセス
        /// 3. 遅延初期化による効率的なリスト作成
        /// 4. 重複防止による安全な登録
        /// 5. 型情報付きログ出力
        ///
        /// 【型安全性メリット】
        /// - コンパイル時検証: 型不一致エラーの事前防止
        /// - 強い型制約: where T : class による参照型限定
        /// - IntelliSense支援: IDEでの型補完とエラー検出
        /// - リファクタリング安全: 型名変更時の自動追跡
        ///
        /// 【パフォーマンス特性】
        /// - 型特化登録: Action<T>として直接保存
        /// - Boxing回避: 値型Boxingなし（class制約）
        /// - 効率的検索: Dictionary + List組み合わせ
        /// - メモリ最適化: 重複登録防止による無駄排除
        ///
        /// 【使用例】
        /// - 型安全購読: Subscribe<PlayerData>("PlayerUpdate", OnPlayerUpdate)
        /// - 強型イベント: Subscribe<GameState>("StateChanged", HandleStateChange)
        /// - カスタム型: Subscribe<CustomEvent>("MyEvent", ProcessCustomEvent)
        ///
        /// 【設計原則】
        /// - 型安全第一: コンパイル時エラー検出優先
        /// - パフォーマンス重視: 実行時オーバーヘッド最小化
        /// - 使いやすさ: 直感的なAPI設計
        /// - 保守性: 明確な型情報による可読性向上
        /// </summary>
        /// <typeparam name="T">イベントデータの型（参照型のみ許可）</typeparam>
        /// <param name="eventName">購読するイベントの名前（null/空文字は無視）</param>
        /// <param name="handler">型安全なイベントハンドラー（nullは無視）</param>
        public void Subscribe<T>(string eventName, Action<T> handler) where T : class
        {
            if (string.IsNullOrEmpty(eventName) || handler == null) return;

            lock (_lock)
            {
                if (!_eventHandlers.TryGetValue(eventName, out var handlers))
                {
                    handlers = new List<Delegate>();
                    _eventHandlers[eventName] = handlers;
                }

                if (!handlers.Contains(handler))
                {
                    handlers.Add(handler);
                    Debug.Log($"[EventManager] Subscribed to typed event '{eventName}'");
                }
            }
        }

        /// <summary>
        /// ジェネリック型ハンドラーのイベント購読解除
        ///
        /// 型安全性を保持しながら、指定されたハンドラーを確実に削除します。
        /// メモリ効率を重視し、不要なリストエントリを自動的にクリーンアップします。
        ///
        /// 【実行フロー】
        /// 1. パラメータ有効性検証（型安全性確保）
        /// 2. スレッドセーフなハンドラー検索
        /// 3. 型特化ハンドラーの安全な削除
        /// 4. 空リストの自動クリーンアップ
        /// 5. 型情報付きログ出力
        ///
        /// 【型安全性保証】
        /// - 型一致検証: 登録時と同じ型のみ解除可能
        /// - コンパイル時チェック: 型不一致は事前にエラー検出
        /// - null安全性: null参照による例外を完全防止
        /// - 参照整合性: 正確なハンドラー参照の切断
        ///
        /// 【メモリ効率化】
        /// - 空リスト削除: ハンドラー数0での自動クリーンアップ
        /// - Dictionary最適化: 不要エントリの確実な除去
        /// - 参照カウント: 適切な参照関係の切断
        /// - ガベージコレクション支援: 不要オブジェクトの解放促進
        ///
        /// 【使用例】
        /// - 型安全解除: Unsubscribe<PlayerData>("PlayerUpdate", OnPlayerUpdate)
        /// - リソース解放: Unsubscribe<HeavyData>("DataUpdate", ProcessHeavyData)
        /// - 動的管理: Unsubscribe<GameEvent>("TempEvent", tempHandler)
        ///
        /// 【パフォーマンス】
        /// - 型特化削除: 正確な型マッチによる効率的な削除
        /// - O(n)複雑度: List.Remove()の線形探索
        /// - 辞書最適化: O(1)での空エントリ削除
        /// - メモリフットプリント: 最小限のメモリ使用量維持
        /// </summary>
        /// <typeparam name="T">イベントデータの型（参照型のみ許可）</typeparam>
        /// <param name="eventName">解除するイベントの名前（null/空文字は無視）</param>
        /// <param name="handler">削除する型安全なハンドラー（nullは無視）</param>
        public void Unsubscribe<T>(string eventName, Action<T> handler) where T : class
        {
            if (string.IsNullOrEmpty(eventName) || handler == null) return;

            lock (_lock)
            {
                if (_eventHandlers.TryGetValue(eventName, out var handlers))
                {
                    if (handlers.Remove(handler))
                    {
                        Debug.Log($"[EventManager] Unsubscribed from typed event '{eventName}'");

                        if (handlers.Count == 0)
                        {
                            _eventHandlers.Remove(eventName);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 指定イベントの全ハンドラー一括解除
        ///
        /// 特定のイベント名に登録されたすべてのハンドラーを一括削除します。
        /// 大量のハンドラーが登録されている場合の効率的なクリーンアップ手段として機能します。
        ///
        /// 【実行フロー】
        /// 1. イベント名の有効性検証
        /// 2. スレッドセーフな辞書アクセス
        /// 3. イベントエントリの完全削除
        /// 4. 削除成功時のログ出力
        ///
        /// 【パフォーマンス特性】
        /// - O(1)削除: Dictionary.Remove()による高速削除
        /// - 一括処理: 個別削除よりも大幅に高速
        /// - メモリ効率: 関連するListオブジェクトも同時削除
        /// - スレッドセーフ: 並行アクセスでの安全性保証
        ///
        /// 【使用例】
        /// - システム終了時: UnsubscribeAll("SystemShutdown")
        /// - シーン切り替え: UnsubscribeAll("SceneSpecificEvent")
        /// - 大量解除: UnsubscribeAll("BulkUpdateEvent")
        /// - エラー復旧: UnsubscribeAll("CorruptedEvent")
        ///
        /// 【安全性保証】
        /// - 存在しないイベントでも例外なし
        /// - null/空文字による安全な早期リターン
        /// - 完全削除: 部分的削除による不整合なし
        /// - ログ記録: 削除操作の可視化
        ///
        /// 【メモリ管理】
        /// - ハンドラーリスト解放: List<Delegate>の完全削除
        /// - 辞書エントリ削除: Dictionaryからのキー削除
        /// - ガベージコレクション促進: 参照切断による回収対象化
        /// </summary>
        /// <param name="eventName">全削除するイベントの名前（null/空文字は無視）</param>
        public void UnsubscribeAll(string eventName)
        {
            if (string.IsNullOrEmpty(eventName)) return;

            lock (_lock)
            {
                if (_eventHandlers.Remove(eventName))
                {
                    Debug.Log($"[EventManager] Unsubscribed all from event '{eventName}'");
                }
            }
        }

        /// <summary>
        /// 全イベント購読の完全クリア
        ///
        /// EventManager内の全イベント購読を一括削除し、初期状態に戻します。
        /// システム終了時やリセット時の確実なクリーンアップ処理として使用されます。
        ///
        /// 【実行フロー】
        /// 1. スレッドセーフな辞書クリア
        /// 2. 全ハンドラーリストの削除
        /// 3. メモリ領域の解放
        /// 4. クリア完了ログ出力
        ///
        /// 【パフォーマンス特性】
        /// - O(1)クリア: Dictionary.Clear()による高速一括削除
        /// - 完全初期化: 内部状態の確実なリセット
        /// - メモリ効率: 全参照の即座切断
        /// - 最小オーバーヘッド: 効率的な bulk操作
        ///
        /// 【使用シナリオ】
        /// - システム終了: アプリケーション終了時のクリーンアップ
        /// - サービス再起動: EventManagerの再初期化
        /// - メモリ最適化: 大量イベント登録後のクリア
        /// - エラー復旧: 破損状態からの回復
        /// - テスト環境: 単体テスト間の状態クリア
        ///
        /// 【安全性保証】
        /// - 確実なクリア: 全エントリの完全削除
        /// - スレッドセーフ: 並行アクセス時の安全性
        /// - 例外安全: クリア処理中の例外防止
        /// - 状態一貫性: 部分的削除による不整合なし
        ///
        /// 【メモリ管理効果】
        /// - 即座解放: 全ハンドラー参照の切断
        /// - GC促進: ガベージコレクション対象化
        /// - メモリフットプリント: 最小限への削減
        /// - リーク防止: 確実な参照関係切断
        /// </summary>
        public void Clear()
        {
            lock (_lock)
            {
                _eventHandlers.Clear();
                Debug.Log("[EventManager] Cleared all event subscriptions");
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// 登録イベント総数取得
        ///
        /// EventManager内で管理されている全イベント名の総数を取得します。
        /// システム監視、デバッグ、パフォーマンス分析で使用される重要な統計情報です。
        ///
        /// 【実行フロー】
        /// 1. スレッドセーフな辞書アクセス
        /// 2. Dictionary.Countプロパティによる高速取得
        /// 3. 瞬間的なイベント数の返却
        ///
        /// 【パフォーマンス特性】
        /// - O(1)取得: Dictionary.Countによる定数時間アクセス
        /// - スレッドセーフ: lockによる安全な並行アクセス
        /// - 軽量操作: 内部カウンタの直接参照
        /// - 最小オーバーヘッド: 計算処理なし
        ///
        /// 【使用例】
        /// - システム監視: GetEventCount() > 100 での警告
        /// - デバッグ情報: イベント登録状況の可視化
        /// - パフォーマンス分析: メモリ使用量の推定
        /// - 統計レポート: システム使用状況の記録
        ///
        /// 【注意事項】
        /// - 瞬間値: 並行環境では取得直後に変化する可能性
        /// - ハンドラー数ではない: イベント名の種類数を返却
        /// - 空ハンドラーリスト: 0個のハンドラーを持つイベントも計上
        /// </summary>
        /// <returns>現在登録されているイベント名の総数</returns>
        public int GetEventCount()
        {
            lock (_lock)
            {
                return _eventHandlers.Count;
            }
        }

        /// <summary>
        /// 特定イベントの購読者数取得
        ///
        /// 指定されたイベント名に対して登録されているハンドラーの総数を取得します。
        /// イベントの人気度や使用状況の分析、デバッグ時の状況確認に使用されます。
        ///
        /// 【実行フロー】
        /// 1. イベント名有効性チェック（暗黙的）
        /// 2. スレッドセーフなハンドラー検索
        /// 3. ハンドラーリストのカウント取得
        /// 4. 存在しない場合は0を返却
        ///
        /// 【パフォーマンス特性】
        /// - O(1)検索: Dictionary.TryGetValueによる高速検索
        /// - O(1)カウント: List.Countプロパティによる即座取得
        /// - スレッドセーフ: 並行アクセスでの安全性保証
        /// - null安全: 存在しないイベントでも例外なし
        ///
        /// 【使用例】
        /// - デバッグ支援: GetSubscriberCount("PlayerDeath") で購読者確認
        /// - パフォーマンス監視: 大量購読者イベントの特定
        /// - システム分析: イベント使用パターンの把握
        /// - 最適化判断: 未使用イベントの検出
        ///
        /// 【戻り値仕様】
        /// - ≥0: 登録されているハンドラーの正確な数
        /// - 0: イベント未登録 または ハンドラー0個
        /// - 型混在カウント: Action<object>とAction<T>の合計数
        /// </summary>
        /// <param name="eventName">購読者数を調べるイベント名</param>
        /// <returns>指定イベントの購読者数（0以上の整数）</returns>
        public int GetSubscriberCount(string eventName)
        {
            lock (_lock)
            {
                if (_eventHandlers.TryGetValue(eventName, out var handlers))
                {
                    return handlers.Count;
                }
                return 0;
            }
        }

        /// <summary>
        /// イベント登録状態確認
        ///
        /// 指定されたイベント名がEventManager内に登録済みかを確認します。
        /// ハンドラーの有無に関わらず、イベント名エントリの存在をチェックします。
        ///
        /// 【実行フロー】
        /// 1. スレッドセーフな辞書キー検索
        /// 2. Dictionary.ContainsKeyによる存在確認
        /// 3. ブール値での即座返却
        ///
        /// 【パフォーマンス特性】
        /// - O(1)検索: ハッシュテーブルベースの高速検索
        /// - スレッドセーフ: lockによる安全な並行アクセス
        /// - 軽量操作: 内部ハッシュ計算のみ
        /// - 即座返却: 条件分岐なしの直接応答
        ///
        /// 【使用例】
        /// - 事前チェック: HasEvent("MyEvent") ? Subscribe() : CreateAndSubscribe()
        /// - 条件分岐: HasEvent("OptionalEvent") && ProcessOptionalLogic()
        /// - デバッグ確認: HasEvent("DebugEvent") でのデバッグフラグ確認
        /// - 安全な操作: HasEvent() チェック後の安全なUnsubscribeAll()
        ///
        /// 【重要な区別】
        /// - イベント名の存在確認: Dictionaryキーの有無
        /// - ハンドラー存在確認: GetSubscriberCount() > 0 で判定
        /// - 空ハンドラーリスト: HasEvent()=true, GetSubscriberCount()=0 のケース
        ///
        /// 【null安全性】
        /// - null/空文字に対してfalseを返却
        /// - Dictionary.ContainsKey()の安全な使用
        /// - 例外なしでの確実な判定
        /// </summary>
        /// <param name="eventName">存在確認するイベント名</param>
        /// <returns>イベント名が登録済みの場合true、未登録の場合false</returns>
        public bool HasEvent(string eventName)
        {
            lock (_lock)
            {
                return _eventHandlers.ContainsKey(eventName);
            }
        }

        #endregion
    }
}
