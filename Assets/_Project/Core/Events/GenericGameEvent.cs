using UnityEngine;
using System.Collections.Generic;
using asterivo.Unity60.Core.Debug;
using System.Linq;

namespace asterivo.Unity60.Core.Events
{
    /// <summary>
    /// 型付きパラメータを持つジェネリックイベントチャネル
    /// ScriptableObjectベースの疎結合イベント通信システム
    /// 
    /// 設計思想:
    /// - 発行者と購読者の完全な分離を実現
    /// - ScriptableObjectによるデータ駆動設計
    /// - 型安全性を保証するジェネリック実装
    /// - 値キャッシュによる遅延参加リスナーへの対応
    /// 
    /// 使用例:
    /// // イベント発火
    /// floatEvent.Raise(100.5f);
    /// 
    /// // リスナー登録
    /// floatEvent.RegisterListener(myListener);
    /// </summary>
    /// <typeparam name="T">イベントで伝達するデータの型</typeparam>
    public abstract class GenericGameEvent<T> : ScriptableObject
    {
        /// <summary>このイベントを購読しているリスナーのコレクション</summary>
        private readonly HashSet<IGameEventListener<T>> listeners = new HashSet<IGameEventListener<T>>();
        
        [Header("Event Settings")]
        /// <summary>最後に発火された値をキャッシュするかどうか</summary>
        [SerializeField] private bool cacheLastValue = false;
        
        /// <summary>デフォルト値として使用される値</summary>
        [SerializeField] private T defaultValue;
        
        /// <summary>最後にキャッシュされた値</summary>
        private T lastValue;
        
        /// <summary>キャッシュされた値が存在するかどうか</summary>
        private bool hasValue = false;
        
        #if UNITY_EDITOR
        [Header("Debug")]
        /// <summary>デバッグモード有効時にイベント発火をログに出力</summary>
        [SerializeField] private bool debugMode = false;
        #endif
        
        /// <summary>
        /// 指定された値でイベントを発火し、全てのリスナーに通知します
        /// 値のキャッシュ、デバッグログ、イベントログ記録を行う
        /// </summary>
        /// <param name="value">イベントと共に送信される値</param>
        /// <remarks>
        /// - キャッシュが有効な場合、値は自動的に保存される
        /// - デバッグモードが有効な場合、発火情報がログに出力される
        /// - nullリスナーは自動的にスキップされる
        /// - リスナーのイテレーション中に安全性を保証するためToArray()を使用
        /// </remarks>
        public void Raise(T value)
        {
            if (cacheLastValue)
            {
                lastValue = value;
                hasValue = true;
            }
            
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (debugMode)
            {
                UnityEngine.Debug.Log($"<color=cyan>[GenericEvent]</color> '{name}' raised with value: {value}", this);
            }
            #endif
            
            // イベントログに記録（ペイロード付き）
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (EventLogger.IsEnabledStatic)
            {
                EventLogger.LogEventWithPayloadStatic(name, listeners.Count, value);
            }
            #endif
            
            // ToArrayで安全なイテレーション
            var activeListeners = listeners.Where(l => l != null).ToArray();
            
            foreach (var listener in activeListeners)
            {
                listener.OnEventRaised(value);
            }
        }
        
        /// <summary>
        /// デフォルト値を使用してイベントを発火します
        /// インスペクターで設定されたdefaultValueを使用
        /// </summary>
        /// <remarks>
        /// テスト用途やコンテクストメニューからの手動実行で使用
        /// デフォルト値が適切に設定されていることが前提
        /// </remarks>
        public void RaiseDefault()
        {
            Raise(defaultValue);
        }
        
        /// <summary>
        /// 最後にキャッシュされた値を取得します
        /// キャッシュが無効または値が未設定の場合はデフォルト値を返す
        /// </summary>
        /// <returns>キャッシュされた最後の値、または該当なしの場合はデフォルト値</returns>
        /// <remarks>
        /// 遅延参加したリスナーが現在の状態を取得する際に使用
        /// キャッシュが有効（cacheLastValue = true）である必要がある
        /// </remarks>
        public T GetLastValue()
        {
            if (!cacheLastValue || !hasValue)
            {
                return defaultValue;
            }
            return lastValue;
        }
        
        /// <summary>
        /// キャッシュされた値が存在するかどうかを確認します
        /// </summary>
        /// <returns>有効なキャッシュ値が存在する場合はtrue</returns>
        /// <remarks>
        /// キャッシュ機能が有効かつ、少なくとも一度イベントが発火された場合にtrueを返す
        /// </remarks>
        public bool HasCachedValue() => cacheLastValue && hasValue;
        
        /// <summary>
        /// イベントリスナーを登録し、イベント通知の対象に追加します
        /// 既にキャッシュ値がある場合は即座に通知を送信
        /// </summary>
        /// <param name="listener">登録するリスナー。nullの場合は無視される</param>
        /// <remarks>
        /// - 重複登録は自動的に防止される（HashSetを使用）
        /// - キャッシュが有効かつ値が存在する場合、登録時に最新値で即座に通知される
        /// - これにより遅延参加したリスナーも現在の状態を即座に取得可能
        /// </remarks>
        public void RegisterListener(IGameEventListener<T> listener)
        {
            if (listener == null) return;
            
            listeners.Add(listener);
            
            // 初回登録時にキャッシュ値があれば通知
            if (cacheLastValue && hasValue)
            {
                listener.OnEventRaised(lastValue);
            }
        }
        
        /// <summary>
        /// 指定されたリスナーの登録を解除し、以降の通知対象から除外します
        /// </summary>
        /// <param name="listener">解除するリスナー。nullの場合は無視される</param>
        /// <remarks>
        /// メモリリークを防ぐため、不要になったリスナーは必ず解除すること
        /// 特にMonoBehaviourベースのリスナーはOnDestroy時に解除を推奨
        /// </remarks>
        public void UnregisterListener(IGameEventListener<T> listener)
        {
            if (listener == null) return;
            listeners.Remove(listener);
        }
        
        
        /// <summary>
        /// 全てのリスナーを一括で登録解除します
        /// イベントのリセットや終了処理で使用
        /// </summary>
        /// <remarks>
        /// シーン遷移時やゲーム終了時のクリーンアップで使用
        /// 慎重に使用すること：予期しない通知の停止を引き起こす可能性がある
        /// </remarks>
        public void ClearAllListeners()
        {
            listeners.Clear();
        }
        
        /// <summary>
        /// キャッシュされた値をクリアし、初期状態に戻します
        /// デフォルト値にリセットされ、キャッシュフラグも無効になる
        /// </summary>
        /// <remarks>
        /// レベル再開やゲームリセット時に使用
        /// 新たにイベントが発火されるまでGetLastValue()はデフォルト値を返す
        /// </remarks>
        public void ClearCache()
        {
            lastValue = defaultValue;
            hasValue = false;
        }
        
        /// <summary>
        /// 現在登録されているリスナーの数を取得します
        /// </summary>
        /// <returns>登録中のリスナー数</returns>
        /// <remarks>デバッグや監視目的で使用。パフォーマンス分析にも有効</remarks>
        public int GetListenerCount() => listeners.Count;
        
        #if UNITY_EDITOR
        [ContextMenu("Raise with Default Value")]
        private void RaiseManually()
        {
            RaiseDefault();
        }
        #endif
    }
    
    // 具体的な型の実装クラス群
    // 各クラスはGenericGameEventの型特化版を提供し、アセット作成を可能にする
    
    /// <summary>
    /// float値を伝達するイベントチャネル
    /// HP、スコア、タイマーなどの数値情報の通知に使用
    /// </summary>
    [CreateAssetMenu(fileName = "New Float Event", menuName = "asterivo.Unity60/Events/Float Event")]
    public class FloatGameEvent : GenericGameEvent<float> { }
    
    /// <summary>
    /// int値を伝達するイベントチャネル
    /// レベル、カウンタ、ID、インデックスなどの整数値情報の通知に使用
    /// </summary>
    [CreateAssetMenu(fileName = "New Int Event", menuName = "asterivo.Unity60/Events/Int Event")]
    public class IntGameEvent : GenericGameEvent<int> { }
    
    /// <summary>
    /// bool値を伝達するイベントチャネル
    /// フラグ、状態切り替え、on/off情報の通知に使用
    /// </summary>
    [CreateAssetMenu(fileName = "New Bool Event", menuName = "asterivo.Unity60/Events/Bool Event")]
    public class BoolGameEvent : GenericGameEvent<bool> { }
    
    /// <summary>
    /// string値を伝達するイベントチャネル
    /// メッセージ、名前、テキスト情報の通知に使用
    /// </summary>
    [CreateAssetMenu(fileName = "New String Event", menuName = "asterivo.Unity60/Events/String Event")]
    public class StringGameEvent : GenericGameEvent<string> { }
    
    /// <summary>
    /// Vector3値を伝達するイベントチャネル
    /// 位置、方向、座標情報の通知に使用
    /// </summary>
    [CreateAssetMenu(fileName = "New Vector3 Event", menuName = "asterivo.Unity60/Events/Vector3 Event")]
    public class Vector3GameEvent : GenericGameEvent<Vector3> { }
    
    /// <summary>
    /// GameObject参照を伝達するイベントチャネル
    /// オブジェクト参照、ターゲット指定、インスタンス情報の通知に使用
    /// </summary>
    [CreateAssetMenu(fileName = "New GameObject Event", menuName = "asterivo.Unity60/Events/GameObject Event")]
    public class GameObjectGameEvent : GenericGameEvent<GameObject> { }
}