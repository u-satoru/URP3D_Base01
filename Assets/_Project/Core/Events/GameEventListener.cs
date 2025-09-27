using UnityEngine;
using UnityEngine.Events;

namespace asterivo.Unity60.Core.Events
{
    /// <summary>
    /// パラメータなしGameEvent専用リスナーコンポーネント実装クラス
    ///
    /// Unity 6における3層アーキテクチャのイベント駆動システムにおいて、
    /// パラメータを持たない基本GameEventを受信するMonoBehaviourベースの
    /// 具体実装クラスです。Inspector完全統合により、プログラマー以外でも
    /// 視覚的にイベント応答を設定可能な開発効率化を実現します。
    ///
    /// 【核心機能】
    /// - Inspector統合: SerializeFieldによる視覚的イベント設定
    /// - UnityEvent連携: Inspectorでのドラッグ&ドロップ応答設定
    /// - 優先度制御: Priority値による実行順序カスタマイズ
    /// - 遅延実行: responseDelayによる時間差処理制御
    /// - ワンショット: oneShotによる一回限り実行制御
    /// - 自動ライフサイクル: OnEnable/OnDisableでの自動登録・解除
    ///
    /// 【高度機能】
    /// - 柔軟登録制御: registerOnAwakeによるAwake時登録オプション
    /// - ワンショットモード: 一度実行後の自動無効化機能
    /// - 遅延レスポンス: コルーチンベースの時間差実行
    /// - 手動制御: Event設定プロパティによる動的イベント変更
    /// - エディタ支援: ContextMenuによる手動テスト実行
    ///
    /// 【開発効率化特徴】
    /// - ノーコード設定: プログラマー以外でも完全に設定可能
    /// - 即座プロトタイピング: ドラッグ&ドロップでの高速実装
    /// - 視覚的デバッグ: Inspector上での設定確認とランタイム監視
    /// - 再利用性: 同一シーンでの複数インスタンス使用
    /// - エラー防止: OnValidateによる設定値検証
    ///
    /// 【典型的使用例】
    /// - UI制御: ボタンクリック、ダイアログ表示、メニュー切り替え
    /// - サウンド制御: BGM開始、効果音再生、ボリューム調整
    /// - ゲーム制御: レベル開始、ゲームオーバー、設定変更
    /// - エフェクト制御: パーティクル開始、アニメーション再生
    /// - システム制御: セーブ実行、ロード開始、初期化完了
    ///
    /// 【パフォーマンス特性】
    /// - 軽量実装: 最小限のオーバーヘッドでの高速イベント処理
    /// - メモリ効率: 必要時のみのコルーチン実行とUnityEvent呼び出し
    /// - 自動最適化: Unity内蔵イベントシステムとの協調動作
    /// - ライフサイクル最適化: 適切な登録・解除による参照リーク防止
    /// </summary>
    public class GameEventListener : MonoBehaviour, IGameEventListener
    {
        [Header("Event Settings")]
        [SerializeField] private GameEvent gameEvent;
        [SerializeField] private int priority = 0;
        [SerializeField] private float responseDelay = 0f;
        
        [Header("Response")]
        [SerializeField] private UnityEvent response;
        
        [Header("Advanced Settings")]
        [SerializeField] private bool registerOnAwake = false;
        [SerializeField] private bool oneShot = false;
        
        private bool hasBeenTriggered = false;
        
        /// <summary>
        /// Responseプロパティ（修正：publicアクセサを追加）
        /// </summary>
        public UnityEvent Response => response;
        
        /// <summary>
        /// 優先度（高い値が先に実行される）
        /// </summary>
        public int Priority => priority;
        
        /// <summary>
        /// 動的GameEvent関連付けプロパティ
        ///
        /// このリスナーが受信対象とするGameEventを動的に変更可能な
        /// 読み書きプロパティです。設定変更時に自動的なリスナー登録・解除を実行し、
        /// ランタイムでのイベント系統の安全な切り替えを実現します。
        ///
        /// 【設定時自動処理フロー】
        /// 1. 既存登録解除: 現在のgameEventからのUnregisterListener()実行
        /// 2. 新値設定: gameEventフィールドへの新GameEvent割り当て
        /// 3. 新規登録: 新gameEventが有効かつenabled=trueの場合のRegisterListener()実行
        /// 4. 状態整合性: 常に単一GameEventとの関連付け保証
        ///
        /// 【安全性機能】
        /// - null安全処理: null値設定時の適切なクリーンアップ実行
        /// - 重複防止: 既存登録の確実な解除による重複登録回避
        /// - 状態連携: enabled状態確認による条件付き登録
        /// - 例外安全: 設定プロセス中の例外による不整合状態防止
        ///
        /// 【使用ケース】
        /// - 動的イベント切り替え: ゲーム状態に応じたイベント系統変更
        /// - 条件付き応答: プレイヤーレベルやモードによるイベント分岐
        /// - デバッグ制御: 開発時のイベント系統動的テスト
        /// - プールオブジェクト: オブジェクト再利用時のイベント再設定
        ///
        /// 【パフォーマンス特性】
        /// - O(1)操作: HashSetベースの高速登録・解除処理
        /// - 最小オーバーヘッド: 必要時のみの登録処理実行
        /// - メモリ効率: 適切な参照管理による参照リーク防止
        /// - 実行時最適化: enabled状態での条件付き処理
        /// </summary>
        public GameEvent Event
        {
            get => gameEvent;
            set
            {
                if (gameEvent != null)
                {
                    gameEvent.UnregisterListener(this);
                }
                gameEvent = value;
                if (gameEvent != null && enabled)
                {
                    gameEvent.RegisterListener(this);
                }
            }
        }
        
        private void Awake()
        {
            if (registerOnAwake && gameEvent != null)
            {
                gameEvent.RegisterListener(this);
            }
        }
        
        private void OnEnable()
        {
            if (!registerOnAwake && gameEvent != null)
            {
                gameEvent.RegisterListener(this);
            }
        }
        
        private void OnDisable()
        {
            if (gameEvent != null)
            {
                gameEvent.UnregisterListener(this);
            }
        }
        
        private void OnDestroy()
        {
            if (gameEvent != null)
            {
                gameEvent.UnregisterListener(this);
            }
        }
        
        /// <summary>
        /// GameEventイベント受信時コールバック実行メソッド
        ///
        /// IGameEventListenerインターフェースの実装として、
        /// 関連付けられたGameEventからのイベント発火時に呼び出される中核メソッドです。
        /// ワンショット制御、遅延実行、UnityEvent連携を統合し、
        /// 柔軟で高機能なイベント応答を実現します。
        ///
        /// 【実行フロー】
        /// 1. ワンショット判定: oneShot && hasBeenTriggeredでの重複実行防止
        /// 2. 実行フラグ設定: hasBeenTriggered = trueによる実行記録
        /// 3. 遅延制御分岐: responseDelayによる即座実行 vs コルーチン実行
        /// 4. レスポンス実行: ExecuteResponse()またはDelayedResponse()呼び出し
        /// 5. ワンショット後処理: 必要時の自動登録解除とコンポーネント無効化
        ///
        /// 【制御機能】
        /// - ワンショット制御: 一度限りの実行による重複防止機構
        /// - 遅延実行制御: responseDelay > 0時のコルーチンベース時間差実行
        /// - 自動無効化: ワンショット完了後の自動リスナー解除
        /// - 状態管理: hasBeenTriggeredによる実行履歴追跡
        ///
        /// 【パフォーマンス考慮】
        /// - 早期リターン: ワンショット重複実行の即座回避
        /// - 条件分岐最適化: 遅延実行の必要時のみコルーチン開始
        /// - メモリ効率: 不要なコルーチン生成の回避
        /// - ライフサイクル連携: 適切な登録解除による参照リーク防止
        /// </summary>
        public void OnEventRaised()
        {
            // ワンショットモードで既に実行済みなら無視
            if (oneShot && hasBeenTriggered)
            {
                return;
            }
            
            hasBeenTriggered = true;
            
            // 遅延実行
            if (responseDelay > 0)
            {
                StartCoroutine(DelayedResponse());
            }
            else
            {
                ExecuteResponse();
            }
        }
        
        private System.Collections.IEnumerator DelayedResponse()
        {
            yield return new WaitForSeconds(responseDelay);
            ExecuteResponse();
        }
        
        private void ExecuteResponse()
        {
            response?.Invoke();
            
            // ワンショットモードなら自動で登録解除
            if (oneShot && gameEvent != null)
            {
                gameEvent.UnregisterListener(this);
                enabled = false;
            }
        }
        
        /// <summary>
        /// ワンショットフラグをリセット
        /// </summary>
        public void ResetOneShot()
        {
            hasBeenTriggered = false;
        }
        
        #if UNITY_EDITOR
        /// <summary>
        /// エディタ用：手動でレスポンスを実行
        /// </summary>
        [ContextMenu("Trigger Response")]
        private void TriggerResponseManually()
        {
            OnEventRaised();
        }
        
        private void OnValidate()
        {
            // 優先度の範囲制限
            priority = Mathf.Clamp(priority, -999, 999);
        }
        #endif
    }
}
