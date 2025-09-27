using UnityEngine;
using UnityEngine.Events;

namespace asterivo.Unity60.Core.Events
{
    /// <summary>
    /// パラメータなし基本イベントリスナーインターフェース
    ///
    /// Unity 6における3層アーキテクチャのイベント駆動システムにおいて、
    /// パラメータを持たない単純な通知イベントを受信するコンポーネントが
    /// 実装すべき標準インターフェースです。GameEventクラスとの連携により、
    /// 疎結合なシステム間通信を実現します。
    ///
    /// 【核心機能】
    /// - パラメータレス通知: 単純な状態変更やトリガーイベントの受信
    /// - 優先度制御: Priority値による実行順序のカスタマイズ
    /// - 有効/無効制御: enabled状態による動的なイベント受信制御
    /// - 疎結合通信: 送信者と受信者の直接参照を排除した設計
    ///
    /// 【実装対象】
    /// - UI更新コンポーネント: メニュー表示、ダイアログ制御
    /// - システム制御: ゲーム開始、レベル遷移、設定変更
    /// - サウンド制御: BGM切り替え、効果音トリガー
    /// - ライフサイクル管理: 初期化完了、破棄処理開始
    ///
    /// 【使用パターン】
    /// - MonoBehaviour継承クラスでの実装
    /// - OnEnable/OnDisableでの自動登録・解除
    /// - Priorityによる実行順序最適化
    /// - enabledによる一時的な無効化
    /// </summary>
    public interface IGameEventListener
    {
        /// <summary>
        /// イベント受信時コールバック実行メソッド
        ///
        /// GameEventによるイベント発火時に呼び出される中核メソッドです。
        /// 実装クラスは、このメソッド内でイベント受信時の具体的な処理を実装します。
        /// Priority値に基づく実行順序で、全登録リスナーに対して順次実行されます。
        ///
        /// 【実装指針】
        /// - 軽量処理: イベント発火ループをブロックしない軽量な実装
        /// - 例外安全: try-catchによる例外伝播の防止
        /// - 状態確認: enabled状態での処理可否判定
        /// - 副作用最小化: 他システムへの意図しない影響の回避
        /// </summary>
        void OnEventRaised();

        /// <summary>
        /// イベント処理優先度プロパティ
        ///
        /// イベント発火時の実行順序を制御する優先度値を定義します。
        /// 高い値ほど先に実行され、同一Priority値の場合は登録順序に依存します。
        /// GameEventのRebuildSortedList()でOrderByDescending()により適用されます。
        ///
        /// 【優先度設計指針】
        /// - 100以上: 最高優先度（システム初期化、エラーハンドリング）
        /// - 50-99: 高優先度（ゲームロジック、状態管理）
        /// - 10-49: 標準優先度（UI更新、エフェクト）
        /// - 0-9: 低優先度（ログ出力、統計処理）
        /// - 負値: 最低優先度（クリーンアップ、最終処理）
        /// </summary>
        int Priority => 0;

        /// <summary>
        /// リスナー有効状態プロパティ
        ///
        /// このリスナーがイベントを受信すべきかどうかを示すフラグです。
        /// GameEventのRaise()実行時にチェックされ、falseの場合はOnEventRaised()が
        /// 呼び出されません。動的なリスナー制御と条件付きイベント処理を実現します。
        ///
        /// 【制御パターン】
        /// - MonoBehaviour.enabled: コンポーネント有効性との連動
        /// - 条件チェック: ゲーム状態による動的制御
        /// - 一時無効化: 処理負荷軽減やデバッグ用途
        /// - フェーズ制御: ゲームフェーズ別のリスナー制御
        /// </summary>
        bool enabled => true;
    }

    /// <summary>
    /// 型付きパラメータ対応イベントリスナーインターフェース
    ///
    /// Unity 6における3層アーキテクチャのイベント駆動システムにおいて、
    /// 型安全なデータ付きイベントを受信するコンポーネントが実装すべき
    /// ジェネリックインターフェースです。GameEvent&lt;T&gt;クラスとの連携により、
    /// コンパイル時型チェックと実行時パフォーマンスを両立します。
    ///
    /// 【核心機能】
    /// - 型安全データ受信: ジェネリック型Tによるコンパイル時型保証
    /// - 優先度ベース実行: Priority値による柔軟な実行順序制御
    /// - 条件付き有効化: enabled状態による動的なイベント処理制御
    /// - パフォーマンス最適化: 型付きイベントの効率的な配信機構
    ///
    /// 【対応データ型】
    /// - プリミティブ型: int, float, bool, string
    /// - Unity型: Vector3, Vector2, Color, Transform
    /// - カスタム型: ゲーム固有データ構造体・クラス
    /// - 列挙型: GameState, PlayerState等の状態定義
    ///
    /// 【典型的使用例】
    /// - HPシステム: float値でのダメージ・回復量通知
    /// - UIシステム: string値でのメッセージ表示要求
    /// - プレイヤー制御: Vector3値での移動指示
    /// - ゲーム状態: GameState列挙値での状態遷移通知
    /// - インベントリ: ItemData構造体でのアイテム情報
    ///
    /// 【アーキテクチャ利点】
    /// - Core層通信: 3層アーキテクチャでの安全な層間通信
    /// - 型安全性: コンパイル時の型不整合検出
    /// - 疎結合設計: 送受信者間の直接参照排除
    /// - 拡張性: 新データ型への容易な対応
    /// </summary>
    /// <typeparam name="T">イベントで送信される値の型（参照型・値型両対応）</typeparam>
    public interface IGameEventListener<T>
    {
        /// <summary>
        /// 型付きイベント受信時コールバック実行メソッド
        ///
        /// GameEvent&lt;T&gt;によるイベント発火時に呼び出される中核メソッドです。
        /// 型安全なパラメータ値と共に実行され、実装クラスは受信データを
        /// 適切に処理する責務を持ちます。Priority値に基づく実行順序で、
        /// 全登録リスナーに対して順次実行されます。
        ///
        /// 【実装指針】
        /// - 型安全処理: パラメータTの型に応じた適切な処理実装
        /// - null安全性: 参照型Tの場合のnullチェック実装
        /// - 例外安全: try-catchによる例外伝播の防止
        /// - パフォーマンス: 軽量処理によるイベントループブロック回避
        /// - 副作用制御: 他システムへの意図しない影響の最小化
        /// </summary>
        /// <param name="value">イベントと共に送信される型安全な値</param>
        void OnEventRaised(T value);

        /// <summary>
        /// イベント処理優先度プロパティ
        ///
        /// 型付きイベント発火時の実行順序を制御する優先度値を定義します。
        /// 高い値ほど先に実行され、GameEvent&lt;T&gt;のRebuildSortedList()で
        /// OrderByDescending()により適用されます。データ型に関係なく
        /// 統一的な優先度制御を実現します。
        ///
        /// 【型付きイベント優先度指針】
        /// - 100以上: システムクリティカル（エラー、ライフサイクル）
        /// - 50-99: ゲームロジック（状態管理、スコア更新）
        /// - 10-49: UI・エフェクト（表示更新、アニメーション）
        /// - 0-9: ログ・統計（データ記録、分析処理）
        /// - 負値: 後処理（クリーンアップ、最終化）
        /// </summary>
        int Priority => 0;

        /// <summary>
        /// リスナー有効状態プロパティ
        ///
        /// この型付きリスナーがイベントを受信すべきかどうかを示すフラグです。
        /// GameEvent&lt;T&gt;のRaise()実行時にチェックされ、falseの場合は
        /// OnEventRaised(T value)が呼び出されません。データ型に依存しない
        /// 統一的なリスナー制御機構を提供します。
        ///
        /// 【型付きイベント制御パターン】
        /// - 条件付き受信: データ値による動的制御（例：HP > 0時のみ）
        /// - フェーズ制御: ゲームフェーズ別の型付きリスナー制御
        /// - パフォーマンス最適化: 不要時の処理負荷軽減
        /// - デバッグ制御: 特定データ型のイベント一時停止
        /// </summary>
        bool enabled => true;
    }
    
    /// <summary>
    /// 汎用型付きイベントリスナー基底クラス実装
    ///
    /// Unity 6における3層アーキテクチャのイベント駆動システムにおいて、
    /// Inspector設定可能な型安全イベントリスナーの統一基盤を提供する
    /// 抽象MonoBehaviourクラスです。ジェネリック設計により、
    /// あらゆるデータ型に対応した再利用可能なリスナー実装を実現します。
    ///
    /// 【核心機能群】
    /// - Inspector統合: SerializeFieldによる視覚的設定インターフェース
    /// - 自動登録機構: OnEnable/OnDisableでの自動ライフサイクル管理
    /// - 優先度制御: 実行順序カスタマイズによる処理最適化
    /// - 値保存機能: 最新受信データのキャッシュ・参照機能
    /// - 遅延実行: responseDelayによる時間差処理制御
    /// - デバッグ支援: logOnReceiveによる詳細トレース機能
    /// - UnityEvent統合: Inspectorでの視覚的レスポンス設定
    ///
    /// 【アーキテクチャ設計】
    /// - ジェネリック基盤: 型パラメータによる型安全な汎用実装
    /// - 抽象クラス: 具体型リスナーの統一基盤としての設計
    /// - MonoBehaviour継承: Unity組み込みライフサイクルとの完全統合
    /// - Inspector最適化: Tooltipと適切なSerializeField配置
    /// - エディタ拡張対応: Development Build環境での開発支援
    ///
    /// 【パフォーマンス特性】
    /// - 軽量実装: 最小限のオーバーヘッドでの高速イベント処理
    /// - メモリ効率: 必要時のみの値保存とコルーチン実行
    /// - 自動最適化: Unity内蔵の最適化機構との協調動作
    /// - ガベージ最小化: 不要なアロケーションの徹底排除
    ///
    /// 【具体実装クラス】
    /// FloatGameEventListener, IntGameEventListener, BoolGameEventListener,
    /// StringGameEventListener, Vector3GameEventListener, GameObjectEventListener等の
    /// 型特化リスナーの共通基盤として機能します。
    ///
    /// 【開発効率化】
    /// - ノーコード設定: プログラマー以外でもInspector操作で設定可能
    /// - 再利用性: 同一クラスの複数インスタンス使用による設定多様化
    /// - デバッグ容易性: ランタイムでの設定確認とログ出力機能
    /// - エラー防止: 型安全性による設定ミスの事前防止
    /// </summary>
    /// <typeparam name="T">イベントデータの型（int, float, Vector3等）</typeparam>
    /// <typeparam name="TEvent">対応するGameEventの型（IntGameEvent等）</typeparam>
    /// <typeparam name="TUnityEvent">対応するUnityEventの型（UnityIntEvent等）</typeparam>
    public abstract class GenericGameEventListener<T, TEvent, TUnityEvent> : MonoBehaviour, IGameEventListener<T>
        where TEvent : GameEvent<T>
        where TUnityEvent : UnityEvent<T>
    {
        [Header("Event Settings")]
        [Tooltip("リスニングする対象のGameEventアセット")]
        [SerializeField] protected TEvent gameEvent;
        
        [Tooltip("イベント処理の優先度。高いほど先に実行される")]
        [SerializeField] protected int priority = 0;
        
        [Header("Response")]
        [Tooltip("イベント受信時に実行するUnityEvent")]
        [SerializeField] protected TUnityEvent response;
        
        [Header("Value Processing")]
        [Tooltip("受信した値を保存するかどうか")]
        [SerializeField] protected bool storeValue = false;
        
        [Tooltip("保存された最新のイベント値（storeValue=trueの場合のみ）")]
        [SerializeField] protected T storedValue;
        
        [Header("Advanced")]
        [Tooltip("イベント受信時にログを出力するかどうか")]
        [SerializeField] protected bool logOnReceive = false;
        
        [Tooltip("レスポンスの遅延時間（秒）。0の場合は即座に実行")]
        [SerializeField] protected float responseDelay = 0f;
        
        /// <summary>
        /// このリスナーの優先度を取得します。高い値ほど先に実行されます。
        /// </summary>
        public int Priority => priority;
        
        /// <summary>
        /// リスニング対象のGameEventを取得または設定します。
        /// </summary>
        public TEvent GameEvent 
        { 
            get => gameEvent; 
            set => gameEvent = value; 
        }
        
        /// <summary>
        /// イベント受信時に実行されるUnityEventを取得します。
        /// </summary>
        public TUnityEvent Response => response;
        
        /// <summary>
        /// コンポーネントが有効化された際に呼び出されます。
        /// GameEventにリスナーとして自身を登録します。
        /// </summary>
        protected virtual void OnEnable()
        {
            if (gameEvent != null)
            {
                gameEvent.RegisterListener(this);
            }
        }
        
        /// <summary>
        /// コンポーネントが無効化された際に呼び出されます。
        /// GameEventからリスナーとして自身を登録解除します。
        /// </summary>
        protected virtual void OnDisable()
        {
            if (gameEvent != null)
            {
                gameEvent.UnregisterListener(this);
            }
        }
        
        /// <summary>
        /// イベントが発生した際に呼び出されるメソッド。
        /// IGameEventListener&lt;T&gt;インターフェースの実装です。
        /// </summary>
        /// <param name="value">イベントと共に送信された値</param>
        public virtual void OnEventRaised(T value)
        {
            // 値の保存機能
            if (storeValue)
            {
                storedValue = value;
            }
            
            // デバッグログ機能
            if (logOnReceive)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.Log($"<color=lime>[Listener]</color> {gameObject.name} received: {value}", this);
#endif
            }
            
            // 遅延レスポンス機能
            if (responseDelay > 0)
            {
                StartCoroutine(DelayedResponse(value));
            }
            else
            {
                response?.Invoke(value);
            }
        }
        
        /// <summary>
        /// 指定された遅延時間後にレスポンスを実行するコルーチン。
        /// </summary>
        /// <param name="value">レスポンスに渡す値</param>
        /// <returns>コルーチン</returns>
        private System.Collections.IEnumerator DelayedResponse(T value)
        {
            yield return new WaitForSeconds(responseDelay);
            response?.Invoke(value);
        }
        
        /// <summary>
        /// 保存された最新のイベント値を取得します。
        /// storeValue=trueの場合のみ有効な値が返されます。
        /// </summary>
        /// <returns>保存された値</returns>
        public T GetStoredValue() => storedValue;
    }
    
    // ===== 具体的な型のリスナー実装 =====
    // 各型に特化したGameEventListenerクラス。
    // Inspector上で直接使用でき、対応するGameEventアセットを割り当て可能です。
    
    /// <summary>
    /// float値を扱うGameEventListener。数値データ（HP、スコア、時間など）の受信に使用。
    /// </summary>
    public class FloatGameEventListener : GenericGameEventListener<float, FloatGameEvent, UnityFloatEvent> { }
    
    /// <summary>
    /// int値を扱うGameEventListener。整数データ（レベル、アイテム数など）の受信に使用。
    /// </summary>
    public class IntGameEventListener : GenericGameEventListener<int, IntGameEvent, UnityIntEvent> { }
    
    /// <summary>
    /// bool値を扱うGameEventListener。フラグやスイッチ状態の受信に使用。
    /// </summary>
    public class BoolGameEventListener : GenericGameEventListener<bool, BoolGameEvent, UnityBoolEvent> { }
    
    /// <summary>
    /// string値を扱うGameEventListener。テキストやメッセージの受信に使用。
    /// </summary>
    public class StringGameEventListener : GenericGameEventListener<string, StringGameEvent, UnityStringEvent> { }
    
    /// <summary>
    /// Vector3値を扱うGameEventListener。位置や方向データの受信に使用。
    /// </summary>
    public class Vector3GameEventListener : GenericGameEventListener<Vector3, Vector3GameEvent, UnityVector3Event> { }
    
    /// <summary>
    /// GameObject値を扱うGameEventListener。オブジェクト参照の受信に使用。
    /// </summary>
    public class GameObjectEventListener : GenericGameEventListener<GameObject, GameObjectGameEvent, UnityGameObjectEvent> { }
    
    // ===== カスタムUnityEvent定義 =====
    // 各型に対応するUnityEventクラス。Inspector上で設定可能な型付きUnityEventです。
    
    /// <summary>
    /// float値を受け取るUnityEvent。Inspector上でfloat引数を持つメソッドを設定可能。
    /// </summary>
    [System.Serializable]
    public class UnityFloatEvent : UnityEvent<float> { }
    
    /// <summary>
    /// int値を受け取るUnityEvent。Inspector上でint引数を持つメソッドを設定可能。
    /// </summary>
    [System.Serializable]
    public class UnityIntEvent : UnityEvent<int> { }
    
    /// <summary>
    /// bool値を受け取るUnityEvent。Inspector上でbool引数を持つメソッドを設定可能。
    /// </summary>
    [System.Serializable]
    public class UnityBoolEvent : UnityEvent<bool> { }
    
    /// <summary>
    /// string値を受け取るUnityEvent。Inspector上でstring引数を持つメソッドを設定可能。
    /// </summary>
    [System.Serializable]
    public class UnityStringEvent : UnityEvent<string> { }
    
    /// <summary>
    /// Vector3値を受け取るUnityEvent。Inspector上でVector3引数を持つメソッドを設定可能。
    /// </summary>
    [System.Serializable]
    public class UnityVector3Event : UnityEvent<Vector3> { }
    
    /// <summary>
    /// GameObject値を受け取るUnityEvent。Inspector上でGameObject引数を持つメソッドを設定可能。
    /// </summary>
    [System.Serializable]
    public class UnityGameObjectEvent : UnityEvent<GameObject> { }
}
