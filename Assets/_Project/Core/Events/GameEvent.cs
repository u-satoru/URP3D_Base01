using UnityEngine;
using System.Collections.Generic;
// using asterivo.Unity60.Core.Debug; // Removed to avoid circular dependency
using System.Linq;

namespace asterivo.Unity60.Core.Events
{
    /// <summary>
    /// パラメータなしの基本イベントチャネル実装クラス
    ///
    /// Unity 6における3層アーキテクチャの核心コンポーネントとして、
    /// イベント駆動アーキテクチャパターンを実現する中央イベント配信システムです。
    /// ScriptableObjectベースの実装により、設計時の柔軟性とランタイムの高性能を両立し、
    /// システム間の疎結合通信を可能にします。
    ///
    /// 【核心機能】
    /// - パラメータレスイベント配信: 単純な通知イベントの効率的な配信
    /// - 優先度ベースリスナー管理: Priority値による実行順序制御
    /// - HashSet高速管理: O(1)のリスナー登録・解除性能
    /// - キャッシュ機構: ソート済みリスナーリストによる実行時最適化
    /// - 非同期配信対応: フレーム分散による負荷軽減
    ///
    /// 【アーキテクチャ統合】
    /// - 3層分離通信: Core↔Feature↔Template層間の安全な通信
    /// - ScriptableObject基盤: 設計時設定とランタイム実行の分離
    /// - エディタ統合: 開発支援とデバッグ機能の包括的提供
    /// - デバッグ対応: Development Build環境での詳細ログ出力
    ///
    /// 【パフォーマンス特性】
    /// - リスナー管理: HashSet使用によるO(1)操作
    /// - 遅延ソート: 変更時のみの最適化されたソート実行
    /// - 逆順実行: リスナー自己削除時の安全性保証
    /// - メモリ効率: 必要時のみのソートリスト再構築
    ///
    /// 【使用パターン】
    /// - システム初期化通知: ゲーム開始、レベルロード完了
    /// - 状態変更通知: ゲーム状態遷移、UI表示切替
    /// - ユーザーアクション: メニュー操作、設定変更
    /// - システムイベント: セーブ完了、エラー発生
    /// </summary>
    [CreateAssetMenu(fileName = "New Game Event", menuName = "asterivo.Unity60/Events/Game Event")]
    public class GameEvent : ScriptableObject
    {
        // リスナーのHashSetによる高速管理
        private readonly HashSet<IGameEventListener> listeners = new HashSet<IGameEventListener>();

        // 優先度ソート済みリスナーリスト（キャッシュ）
        private List<IGameEventListener> sortedListeners;
        private bool isDirty = true;
        
        #if UNITY_EDITOR
        [Header("Debug Settings")]
        [SerializeField] private bool debugMode = false;
        [SerializeField, TextArea(3, 5)] private string eventDescription;
        
        // エディタ用デバッグ情報
        [Header("Runtime Info (Editor Only)")]
        [SerializeField, asterivo.Unity60.Core.Attributes.ReadOnly] private int listenerCount;
        #endif

        /// <summary>
        /// イベント即座発火実行メソッド
        ///
        /// 登録された全リスナーに対してイベントを同期的に配信します。
        /// 優先度付きソート機構により、Priority値の高いリスナーから順次実行され、
        /// システム間の適切な実行順序を保証します。
        ///
        /// 【実行フロー】
        /// 1. デバッグモード時の発火ログ出力（Development Build環境）
        /// 2. リスナーカウント更新（エディタ用統計情報）
        /// 3. 遅延ソート実行（isDirtyフラグチェック後の最適化）
        /// 4. 逆順安全実行（リスナー自己削除時の安全性確保）
        /// 5. enabled状態確認（非アクティブリスナーの除外）
        ///
        /// 【パフォーマンス特性】
        /// - 遅延ソート: 変更時のみソート実行によるCPU負荷最小化
        /// - 逆順実行: foreach中のリスナー削除による例外回避
        /// - null安全性: 削除済みリスナーの安全なスキップ
        /// - キャッシュ活用: ソート済みリストの再利用による高速実行
        ///
        /// 【注意事項】
        /// - 同期実行: すべてのリスナー処理が完了するまでブロック
        /// - リエントラント非対応: リスナー内での同一イベント発火は推奨しない
        /// - 処理時間: リスナー数に比例した実行時間（O(n)）
        /// - スレッドセーフティ: メインスレッドでの実行必須
        /// </summary>
        public void Raise()
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (debugMode)
            {
                UnityEngine.Debug.Log($"<color=cyan>[GameEvent]</color> '{name}' raised at {Time.time:F2}s with {listeners.Count} listeners", this);
            }
            #endif
            #if UNITY_EDITOR
            listenerCount = listeners.Count;
            #endif
            
            // イベントログに記録（簡略化版）
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"[GameEvent] {name} raised to {listeners.Count} listeners");
            #endif
            
            // 優先度でソート（必要時のみ）
            if (isDirty)
            {
                RebuildSortedList();
            }
            
            // 逆順で実行（リスナーが自身を削除しても安全）
            for (int i = sortedListeners.Count - 1; i >= 0; i--)
            {
                if (sortedListeners[i] != null && sortedListeners[i].enabled)
                {
                    sortedListeners[i].OnEventRaised();
                }
            }
        }
        
        /// <summary>
        /// 非同期イベント配信実行メソッド（フレーム分散処理）
        ///
        /// 大量のリスナーへのイベント配信をフレーム分散して実行し、
        /// ゲームループのパフォーマンス影響を最小化します。
        /// 各リスナーの処理後にyield return nullを挿入し、
        /// フレームレート維持とレスポンシブ性を両立させます。
        ///
        /// 【実行戦略】
        /// - フレーム分散: リスナー1つずつの処理でフレーム分割
        /// - 優先度保持: ソート順序を維持した順次実行
        /// - 負荷軽減: 重い処理を含むリスナーでのフレームドロップ防止
        /// - コルーチン活用: IEnumeratorによる中断可能な実行制御
        ///
        /// 【使用ケース】
        /// - UI大量更新: 多数のUI要素への一斉更新処理
        /// - システム初期化: 複数システムの段階的初期化
        /// - エフェクト処理: 大量のパーティクル・アニメーション開始
        /// - データ処理: 重いロジックを含む複数コンポーネントへの通知
        ///
        /// 【パフォーマンス考慮】
        /// - フレーム維持: 60FPS維持のための分散実行
        /// - メモリ効率: ガベージ生成を最小化したイテレーション
        /// - 中断対応: 途中でのコルーチン停止への適切な対応
        /// - 実行時間: リスナー数×フレーム数の時間コスト
        ///
        /// 【注意事項】
        /// - 実行時間: 同期版より大幅に長い実行時間
        /// - 状態変化: 実行中のリスナー登録・解除の影響
        /// - コルーチン管理: 呼び出し元でのコルーチン生存期間管理必須
        /// </summary>
        public System.Collections.IEnumerator RaiseAsync()
        {
            if (isDirty)
            {
                RebuildSortedList();
            }
            
            foreach (var listener in sortedListeners)
            {
                if (listener != null && listener.enabled)
                {
                    listener.OnEventRaised();
                    yield return null; // 次フレームへ
                }
            }
        }

        /// <summary>
        /// イベントリスナー登録メソッド
        ///
        /// IGameEventListenerインターフェースを実装したオブジェクトを
        /// イベント受信者として登録します。HashSetベースの高速管理により、
        /// O(1)の登録性能と重複登録の自動防止を実現します。
        ///
        /// 【登録プロセス】
        /// 1. null安全性チェック: 無効なリスナーの事前排除
        /// 2. HashSet重複確認: 既存登録済みリスナーの自動検出
        /// 3. 新規登録処理: Add()成功時のみの後続処理実行
        /// 4. ダーティフラグ設定: 次回イベント発火時のソート強制実行
        /// 5. デバッグログ出力: Development Build環境での詳細追跡
        ///
        /// 【パフォーマンス特性】
        /// - O(1)登録: HashSet.Add()による定数時間操作
        /// - 重複回避: 自動的な重複登録防止機構
        /// - 遅延ソート: 登録時ではなく次回発火時の最適化ソート
        /// - メモリ効率: 最小限のオーバーヘッドでの管理
        ///
        /// 【注意事項】
        /// - スレッドセーフティ: メインスレッド専用（並行登録非対応）
        /// - 生存期間管理: リスナー削除責任は呼び出し元
        /// - 優先度変更: 登録後のPriority変更は次回発火時まで反映されない
        /// - デバッグモード: Development Buildでの詳細ログ出力
        /// </summary>
        /// <param name="listener">登録対象のIGameEventListenerインターフェース実装オブジェクト</param>
        public void RegisterListener(IGameEventListener listener)
        {
            if (listener == null) return;
            
            if (listeners.Add(listener))
            {
                isDirty = true;
                
                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                if (debugMode)
                {
                    UnityEngine.Debug.Log($"<color=green>[GameEvent]</color> Listener registered to '{name}'", this);
                }
                #endif
            }
        }

        /// <summary>
        /// イベントリスナー解除メソッド
        ///
        /// 登録済みのIGameEventListenerインターフェース実装オブジェクトを
        /// イベント受信者リストから安全に除去します。HashSetベースの高速管理により、
        /// O(1)の解除性能と存在確認の自動実行を実現します。
        ///
        /// 【解除プロセス】
        /// 1. null安全性チェック: 無効なリスナーパラメータの事前排除
        /// 2. HashSet存在確認: Remove()による登録状態確認と削除実行
        /// 3. 成功時処理: 削除成功時のみの後続処理実行
        /// 4. ダーティフラグ設定: 次回イベント発火時のソート最適化フラグ
        /// 5. デバッグログ出力: Development Build環境での解除追跡ログ
        ///
        /// 【安全性保証】
        /// - 重複解除対応: 既に削除済みリスナーの安全なスキップ
        /// - null安全: null参照による例外の事前防止
        /// - 実行中安全: イベント発火中のリスナー自己削除対応
        /// - 状態整合性: ソートリストとHashSetの一貫性維持
        ///
        /// 【パフォーマンス特性】
        /// - O(1)解除: HashSet.Remove()による定数時間操作
        /// - 存在確認: Remove()戻り値による効率的な存在チェック
        /// - 遅延ソート: 解除時ではなく次回発火時の最適化ソート
        /// - メモリ解放: 参照削除による即座のガベージコレクション対象化
        ///
        /// 【重要な使用パターン】
        /// - OnDestroy()での自動解除: MonoBehaviour破棄時の確実な解除
        /// - 一時的な無効化: 条件付きでのリスナー停止と再開
        /// - 動的システム: ランタイムでのリスナー構成変更
        /// </summary>
        /// <param name="listener">解除対象のIGameEventListenerインターフェース実装オブジェクト</param>
        public void UnregisterListener(IGameEventListener listener)
        {
            if (listener == null) return;
            
            if (listeners.Remove(listener))
            {
                isDirty = true;
                
                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                if (debugMode)
                {
                    UnityEngine.Debug.Log($"<color=yellow>[GameEvent]</color> Listener unregistered from '{name}'", this);
                }
                #endif
            }
        }
        
        /// <summary>
        /// 全リスナーをクリア
        /// </summary>
        public void ClearAllListeners()
        {
            listeners.Clear();
            sortedListeners?.Clear();
            isDirty = true;
        }
        
        /// <summary>
        /// アクティブなリスナー数を取得
        /// </summary>
        public int GetListenerCount() => listeners.Count;
        
        /// <summary>
        /// 優先度ベース・ソート済みリスナーリスト再構築メソッド
        ///
        /// HashSetで管理されているリスナーから、Priority値による降順ソートを適用した
        /// 実行用リストを構築します。遅延評価パターンにより、変更時のみの効率的な
        /// ソート実行を実現し、イベント発火時のパフォーマンスを最適化します。
        ///
        /// 【ソート戦略】
        /// - 降順ソート: OrderByDescending(Priority)による高優先度優先実行
        /// - null除外: Where(l => l != null)によるnull参照の事前除去
        /// - LINQ最適化: IEnumerable→Listの効率的な変換処理
        /// - ダーティフラグ: isDirty=falseによる重複実行防止
        ///
        /// 【実行タイミング】
        /// - 遅延実行: Raise()またはRaiseAsync()実行時のisDirtyチェック後
        /// - 変更契機: RegisterListener()/UnregisterListener()後のフラグ設定
        /// - 最適化: 変更がない限りソート処理をスキップ
        /// - キャッシュ活用: sortedListenersの再利用による性能向上
        ///
        /// 【パフォーマンス分析】
        /// - 時間複雑度: O(n log n) - リスナー数に対する対数線形時間
        /// - 空間複雑度: O(n) - ソート済みリスト用の追加メモリ
        /// - 実行頻度: リスナー変更時のみの最小化実行
        /// - ガベージ生成: ToList()によるList<T>インスタンス生成
        ///
        /// 【設計考慮】
        /// - null安全性: 削除済みリスナーの安全な除外処理
        /// - 順序保証: Priority値による決定的な実行順序
        /// - メモリ効率: 必要時のみの再構築による無駄排除
        /// - LINQ活用: 可読性とパフォーマンスのバランス実現
        /// </summary>
        private void RebuildSortedList()
        {
            sortedListeners = listeners
                .Where(l => l != null)
                .OrderByDescending(l => l.Priority)
                .ToList();
            isDirty = false;
        }
        
        #if UNITY_EDITOR
        /// <summary>
        /// エディタ用：手動でイベントを発火
        /// </summary>
        [ContextMenu("Raise Event")]
        private void RaiseManually()
        {
            Raise();
        }
        
        /// <summary>
        /// 現在のリスナーをログ出力
        /// </summary>
        [ContextMenu("Log All Listeners")]
        private void LogListeners()
        {
            UnityEngine.Debug.Log($"=== Listeners for '{name}' ===");
            foreach (var listener in listeners)
            {
                if (listener != null)
                {
                    var component = listener as Component;
                    if (component != null)
                    {
                        UnityEngine.Debug.Log($"  - {component.gameObject.name}.{listener.GetType().Name} (Priority: {listener.Priority})", component);
                    }
                    else
                    {
                        UnityEngine.Debug.Log($"  - {listener.GetType().Name} (Priority: {listener.Priority})");
                    }
                }
            }
        }
        #endif
    }
    
    #if UNITY_EDITOR
    // エディタ用のReadOnly属性
    namespace asterivo.Unity60.Core.Attributes
    {
        public class ReadOnlyAttribute : PropertyAttribute { }
    }
    #endif
}