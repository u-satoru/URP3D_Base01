using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using UnityEngine;

namespace asterivo.Unity60.Core
{
    /// <summary>
    /// 高性能Service Locatorパターンの実装クラス
    ///
    /// DIフレームワークを使用せずに軽量で高速な依存関係管理を提供します。
    /// 3層アーキテクチャにおけるCore層の中核システムとして、
    /// グローバルサービスへの統一アクセスポイントを実現します。
    ///
    /// パフォーマンス最適化技術:
    /// - ConcurrentDictionary: 読み取り処理の並行実行とロック競合の最小化
    /// - Type名キャッシュ: リフレクション処理の高速化とメモリ使用量削減
    /// - 条件付きログ: リリースビルドでのパフォーマンス影響を排除
    /// - アトミック操作: スレッドセーフな統計収集とメモリ効率の向上
    ///
    /// 対応パターン:
    /// - 型ベースサービス登録: 強い型付けによる安全なサービス管理
    /// - 名前付きサービス登録: 同一型の複数インスタンス管理
    /// - ファクトリ登録: 遅延初期化による起動時間短縮
    /// </summary>
    public static partial class ServiceLocator
    {
        // 型ベースサービス格納: 高速読み取りと並行アクセス対応
        private static readonly ConcurrentDictionary<Type, object> services = new ConcurrentDictionary<Type, object>();
        private static readonly ConcurrentDictionary<Type, Func<object>> factories = new ConcurrentDictionary<Type, Func<object>>();
        
        // 型名キャッシュ: リフレクション処理の高速化とメモリ効率化
        private static readonly ConcurrentDictionary<Type, string> typeNameCache = new ConcurrentDictionary<Type, string>();
        
        // パフォーマンス統計: アクセス頻度とヒット率の監視（volatile使用でロックフリー）
        private static volatile int accessCount = 0;
        private static volatile int hitCount = 0;
        
        /// <summary>
        /// 型ベースサービス登録メソッド
        ///
        /// 指定された型をキーとしてサービスインスタンスを登録します。
        /// 同一型での重複登録は警告を出力して上書きします。
        ///
        /// パフォーマンス特性:
        /// - O(1)の高速登録処理
        /// - スレッドセーフな並行登録対応
        /// - 開発ビルドでのみログ出力（リリース時のオーバーヘッドなし）
        /// </summary>
        /// <typeparam name="T">サービスの型（クラス型のみ）</typeparam>
        /// <param name="service">登録するサービスインスタンス</param>
        public static void RegisterService<T>(T service) where T : class
        {
            var type = typeof(T);
            var typeName = GetCachedTypeName(type);
            
            var wasReplaced = services.ContainsKey(type);
            services[type] = service;
            
            // 開発時専用ログ: リリースビルドでの性能影響を回避
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
        /// ファクトリメソッド登録（遅延初期化サポート）
        ///
        /// サービスの実際の生成を初回アクセス時まで遅延させることで、
        /// アプリケーション起動時間を短縮し、不要なメモリ使用を回避します。
        ///
        /// 用途:
        /// - 重い初期化処理を持つサービス
        /// - 条件付きで使用されるサービス
        /// - 依存関係が複雑なサービス
        ///
        /// 動作:
        /// 1. ファクトリ関数のみを登録（軽量）
        /// 2. 初回GetService呼び出し時にサービス生成
        /// 3. 生成後はファクトリを削除して通常サービスとして管理
        /// </summary>
        /// <typeparam name="T">サービスの型</typeparam>
        /// <param name="factory">サービス生成用ファクトリ関数</param>
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
        /// 高速サービス取得メソッド
        ///
        /// 指定された型のサービスインスタンスを取得します。
        /// 2段階の取得戦略により最適なパフォーマンスを実現：
        ///
        /// 第1段階: 既存サービスの高速検索（最頻繁実行パス）
        /// - ConcurrentDictionaryによるO(1)検索
        /// - ロックフリーなアトミック統計更新
        ///
        /// 第2段階: ファクトリからの遅延生成（低頻度実行パス）
        /// - アトミック操作による重複生成防止
        /// - 生成後のファクトリ自動削除
        ///
        /// 戻り値:
        /// - 成功時: サービスインスタンス
        /// - 失敗時: null（例外は発生させない）
        /// </summary>
        /// <typeparam name="T">取得するサービスの型</typeparam>
        /// <returns>サービスインスタンス、または見つからない場合はnull</returns>
        public static T GetService<T>() where T : class
        {
            var type = typeof(T);
            
            // アクセス統計更新: ロックフリーなアトミック操作で性能影響を最小化
            System.Threading.Interlocked.Increment(ref accessCount);
            
            // 高速パス: 既存サービスの即座取得（最頻繁実行経路）
            if (services.TryGetValue(type, out var service))
            {
                System.Threading.Interlocked.Increment(ref hitCount);
                return service as T;
            }
            
            // 遅延生成パス: ファクトリによるサービス作成（低頻度実行経路）
            if (factories.TryGetValue(type, out var factory))
            {
                var newService = factory() as T;
                if (newService != null)
                {
                    // 並行生成制御: TryAddによる重複インスタンス生成の防止
                    services.TryAdd(type, newService);
                    factories.TryRemove(type, out _);
                    System.Threading.Interlocked.Increment(ref hitCount);
                    return newService;
                }
            }
            
            // 開発専用警告: 本番環境での性能劣化を防止
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            var typeName = GetCachedTypeName(type);
            UnityEngine.Debug.LogWarning($"[ServiceLocator] Service {typeName} not found");
#endif
            return null;
        }
        
        /// <summary>
        /// 必須サービス取得メソッド（例外発生型）
        ///
        /// 指定されたサービスが必ず存在することを前提とした取得メソッドです。
        /// サービスが見つからない場合は例外を発生させ、プログラムの実行を停止します。
        ///
        /// 用途:
        /// - ゲーム実行に必須のコアサービス
        /// - 初期化時に確実に存在すべきサービス
        /// - 欠如時に続行不可能なサービス
        ///
        /// 注意: 例外処理のオーバーヘッドがあるため、
        /// 高頻度呼び出しや任意性のあるサービスにはGetService()を使用してください。
        /// </summary>
        /// <typeparam name="T">取得するサービスの型</typeparam>
        /// <returns>サービスインスタンス</returns>
        /// <exception cref="InvalidOperationException">サービスが登録されていない場合</exception>
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
        /// サービス存在確認メソッド
        ///
        /// 指定された型のサービスまたはファクトリが登録されているかを
        /// 高速で確認します。実際のサービス取得を行わないため、
        /// 条件分岐での使用に適しています。
        ///
        /// 確認対象:
        /// - 既に生成済みのサービスインスタンス
        /// - 登録済みのファクトリメソッド
        ///
        /// パフォーマンス特性:
        /// - O(1)の高速確認
        /// - 統計更新なし（軽量操作）
        /// - メモリアロケーションなし
        /// </summary>
        /// <typeparam name="T">確認するサービスの型</typeparam>
        /// <returns>登録されている場合はtrue、そうでなければfalse</returns>
        public static bool HasService<T>() where T : class
        {
            var type = typeof(T);
            return services.ContainsKey(type) || factories.ContainsKey(type);
        }
        
        /// <summary>
        /// サービス登録状態確認メソッド（HasServiceの別名）
        ///
        /// HasService&lt;T&gt;()と同一の機能を提供する別名メソッドです。
        /// より明示的な命名を好む場合に使用してください。
        ///
        /// 用途:
        /// - 初期化前のサービス状態確認
        /// - 条件付きサービス利用
        /// - デバッグ時の状態検証
        /// </summary>
        /// <typeparam name="T">確認するサービスの型</typeparam>
        /// <returns>登録されている場合はtrue、そうでなければfalse</returns>
        public static bool IsServiceRegistered<T>() where T : class
        {
            return HasService<T>();
        }
        
        /// <summary>
        /// 特定サービスの登録解除メソッド
        ///
        /// 指定された型のサービスとファクトリを安全に削除します。
        /// 並行アクセス環境でも正常に動作し、重複削除を適切に処理します。
        ///
        /// 削除対象:
        /// - 登録済みサービスインスタンス
        /// - 登録済みファクトリメソッド
        ///
        /// 用途:
        /// - 動的なサービス入れ替え
        /// - メモリ使用量の最適化
        /// - シーン遷移時のクリーンアップ
        ///
        /// 注意: 削除後に同サービスへのアクセスがある場合は、
        /// GetService()はnullを返すため、呼び出し側での適切な処理が必要です。
        /// </summary>
        /// <typeparam name="T">削除するサービスの型</typeparam>
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
        /// 全サービス一括削除メソッド
        ///
        /// 登録されているすべてのサービス、ファクトリ、統計情報を
        /// 完全にクリアします。アプリケーション終了時やテスト環境での
        /// 初期化に使用します。
        ///
        /// クリア対象:
        /// - 型ベース登録サービス
        /// - ファクトリメソッド
        /// - 名前付きサービス
        /// - 型名キャッシュ
        /// - パフォーマンス統計
        ///
        /// 用途:
        /// - アプリケーション終了処理
        /// - テスト間のクリーンアップ
        /// - 完全なリセットが必要なシーン遷移
        ///
        /// 注意: この操作は不可逆的であり、すべてのサービス参照が
        /// 無効になるため、慎重に使用してください。
        /// </summary>
        public static void Clear()
        {
            services.Clear();
            factories.Clear();
            namedServices.Clear();
            typeNameCache.Clear();

            // パフォーマンス統計の完全リセット
            System.Threading.Interlocked.Exchange(ref accessCount, 0);
            System.Threading.Interlocked.Exchange(ref hitCount, 0);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log("[ServiceLocator] All services cleared");
#endif
        }
        
        /// <summary>
        /// 総サービス数取得メソッド
        ///
        /// 現在登録されているサービスの総数を返します。
        /// デバッグ、監視、メモリ使用量の把握に使用します。
        ///
        /// カウント対象:
        /// - 型ベース登録済みサービス
        /// - ファクトリメソッド（未生成含む）
        /// - 名前付きサービス
        ///
        /// パフォーマンス特性:
        /// - O(1)の高速カウント
        /// - スレッドセーフな読み取り
        /// - 統計更新なし
        /// </summary>
        /// <returns>登録済みサービスの総数</returns>
        public static int GetServiceCount()
        {
            return services.Count + factories.Count + namedServices.Count;
        }
        
        /// <summary>
        /// デバッグ用サービス一覧出力メソッド
        ///
        /// 現在登録されているすべてのサービスとファクトリの詳細情報を
        /// Unityコンソールに出力します。開発・デバッグ時の状態確認に使用します。
        ///
        /// 出力内容:
        /// - 型ベースサービス: インターフェース型 → 実装型
        /// - ファクトリ: 型名 → [Factory]マーク
        /// - パフォーマンス統計: アクセス数、ヒット数、ヒット率
        ///
        /// 実行条件:
        /// - Unity Editor環境でのみ実行（Conditional属性により制御）
        /// - リリースビルドでは完全に除去される
        ///
        /// 用途:
        /// - サービス登録状態の確認
        /// - 依存関係の可視化
        /// - パフォーマンス分析
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
            
            // 使用統計の詳細出力
            LogPerformanceStats();
        }

        // 名前付きサービス格納: 同一型の複数インスタンス管理用
        private static readonly ConcurrentDictionary<string, object> namedServices = new ConcurrentDictionary<string, object>();

        /// <summary>
        /// 名前付きサービス登録メソッド
        ///
        /// 文字列キーを使用してサービスを登録します。
        /// 同一型の複数インスタンスを区別する場合に使用します。
        ///
        /// 用途:
        /// - 複数の設定を持つ同一サービス（例：複数データベース接続）
        /// - 地域別・環境別サービス（例：本番用・テスト用API）
        /// - プラグイン式アーキテクチャでの動的サービス
        ///
        /// パフォーマンス特性:
        /// - 文字列ハッシュによるO(1)アクセス
        /// - 型安全性は実行時チェック
        /// - メモリ使用量は型ベースより若干増加
        ///
        /// 注意事項:
        /// - キーの命名規則を統一することを推奨
        /// - null/空文字列キーは例外発生
        /// - 型の不一致は実行時エラー
        /// </summary>
        /// <typeparam name="T">サービスの型</typeparam>
        /// <param name="key">サービス識別用の文字列キー</param>
        /// <param name="service">登録するサービスインスタンス</param>
        /// <exception cref="ArgumentException">キーがnullまたは空文字列の場合</exception>
        /// <exception cref="ArgumentNullException">サービスがnullの場合</exception>
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
        /// 名前付きサービス取得メソッド
        ///
        /// 文字列キーを使用してサービスを取得します。
        /// 型チェックを含む安全な取得処理を行います。
        ///
        /// 取得処理フロー:
        /// 1. キー検証（null/空チェック）
        /// 2. アクセス統計更新
        /// 3. サービス存在確認
        /// 4. 型安全性確認
        /// 5. キャスト済みインスタンス返却
        ///
        /// エラー処理:
        /// - キー不正: 例外発生
        /// - サービス未登録: null返却 + 警告ログ
        /// - 型不一致: null返却 + エラーログ
        ///
        /// パフォーマンス特性:
        /// - 文字列ハッシュによる高速検索
        /// - 型チェックのオーバーヘッド有り
        /// - 統計情報は型ベースサービスと共有
        /// </summary>
        /// <typeparam name="T">期待するサービスの型</typeparam>
        /// <param name="key">サービス識別用の文字列キー</param>
        /// <returns>サービスインスタンス、または見つからない・型不一致の場合はnull</returns>
        /// <exception cref="ArgumentException">キーがnullまたは空文字列の場合</exception>
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
        /// 名前付きサービス削除メソッド
        ///
        /// 指定された文字列キーのサービスを安全に削除します。
        /// 存在しないキーの削除要求は静粛に無視されます。
        ///
        /// 削除処理フロー:
        /// 1. キー検証（null/空チェック）
        /// 2. アトミック削除実行
        /// 3. 削除結果のログ出力（開発時のみ）
        ///
        /// 用途:
        /// - 動的サービスの入れ替え
        /// - 一時的サービスのクリーンアップ
        /// - メモリ使用量の最適化
        ///
        /// パフォーマンス特性:
        /// - O(1)の高速削除
        /// - スレッドセーフな操作
        /// - ログ出力は開発時のみ
        /// </summary>
        /// <param name="key">削除するサービスの文字列キー</param>
        /// <exception cref="ArgumentException">キーがnullまたは空文字列の場合</exception>
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
        /// 型名キャッシュ取得メソッド（内部使用専用）
        ///
        /// Type.Nameプロパティの重複取得を避けるためのキャッシュ機能です。
        /// リフレクション処理の高速化とメモリ効率化を実現します。
        ///
        /// 最適化効果:
        /// - 型名文字列の重複生成防止
        /// - GetOrAddによる安全なキャッシュ追加
        /// - ConcurrentDictionaryによるスレッドセーフなアクセス
        ///
        /// 用途:
        /// - ログ出力での型名表示
        /// - デバッグ情報の生成
        /// - エラーメッセージの作成
        ///
        /// 注意: このメソッドは内部実装の詳細であり、
        /// 外部からの直接呼び出しは推奨されません。
        /// </summary>
        /// <param name="type">名前を取得する型</param>
        /// <returns>キャッシュされた型名文字列</returns>
        private static string GetCachedTypeName(Type type)
        {
            return typeNameCache.GetOrAdd(type, t => t.Name);
        }
        
        /// <summary>
        /// パフォーマンス統計情報取得メソッド
        ///
        /// ServiceLocatorの使用状況に関する統計情報を取得します。
        /// 性能分析、ボトルネック特定、キャッシュ効率の評価に使用します。
        ///
        /// 統計項目:
        /// - accessCount: 総アクセス回数（GetService呼び出し回数）
        /// - hitCount: 成功取得回数（サービスが見つかった回数）
        /// - hitRate: ヒット率（成功率の百分率、0.0〜1.0）
        ///
        /// 計算方法:
        /// - ヒット率 = hitCount ÷ accessCount
        /// - アクセス数が0の場合はヒット率0%
        ///
        /// パフォーマンス特性:
        /// - volatile読み取りによる最新値取得
        /// - 統計収集自体のオーバーヘッドは最小限
        /// - スレッドセーフな値読み取り
        ///
        /// 活用例:
        /// - サービス登録の最適化判断
        /// - 不要サービスの特定
        /// - キャッシュ戦略の評価
        /// </summary>
        /// <returns>アクセス数、ヒット数、ヒット率のタプル</returns>
        public static (int accessCount, int hitCount, float hitRate) GetPerformanceStats()
        {
            var currentAccessCount = accessCount;
            var currentHitCount = hitCount;
            var hitRate = currentAccessCount > 0 ? (float)currentHitCount / currentAccessCount : 0f;
            
            return (currentAccessCount, currentHitCount, hitRate);
        }
        
        /// <summary>
        /// パフォーマンス統計ログ出力メソッド
        ///
        /// 現在のパフォーマンス統計をUnityコンソールに出力します。
        /// 開発時のパフォーマンス分析とボトルネック特定に使用します。
        ///
        /// 出力内容:
        /// - Access: 総アクセス回数
        /// - Hits: 成功取得回数
        /// - Hit Rate: ヒット率（パーセント表示）
        ///
        /// 実行制御:
        /// - Unity Editorでのみ実行（Conditional属性）
        /// - リリースビルドでは完全に除去
        /// - パフォーマンス影響なし
        ///
        /// 使用例:
        /// - 最適化前後の効果測定
        /// - サービス使用パターンの分析
        /// - キャッシュ効率の評価
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
        /// パフォーマンス統計リセットメソッド
        ///
        /// アクセス数とヒット数を0にリセットします。
        /// 測定区間を区切ってパフォーマンス分析を行う際に使用します。
        ///
        /// リセット対象:
        /// - accessCount: 総アクセス回数
        /// - hitCount: 成功取得回数
        ///
        /// 操作特性:
        /// - アトミック操作による安全なリセット
        /// - スレッドセーフな実行
        /// - 瞬時完了（ブロッキングなし）
        ///
        /// 用途:
        /// - ベンチマーク測定の開始前
        /// - A/Bテストでの測定区間分離
        /// - 長時間実行での統計クリア
        ///
        /// 注意: 統計リセットは測定の継続性を損なうため、
        /// 計画的なタイミングで実行してください。
        /// </summary>
        public static void ResetPerformanceStats()
        {
            System.Threading.Interlocked.Exchange(ref accessCount, 0);
            System.Threading.Interlocked.Exchange(ref hitCount, 0);
        }
    }
}
