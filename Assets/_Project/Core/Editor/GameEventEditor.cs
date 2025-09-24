using UnityEngine;
using UnityEditor;
using System.Linq;
using asterivo.Unity60.Core.Events;

namespace asterivo.Unity60.Core.Editor
{
    /// <summary>
    /// GameEvent用カスタムエディター
    /// GameEventアセットのInspectorでデバッグ機能と開発ツールを提供
    /// 
    /// 主な機能：
    /// - 手動でのイベント発生テスト
    /// - アクティブなリスナー数表示
    /// - 関連リスナーのシーン内ハイライト
    /// - プレイモード中のリアルタイム状態表示
    /// - イベント履歴ウィンドウへのショートカットアクセス
    /// 
    /// 使用シーン：
    /// - GameEventアセットの動作テスト
    /// - リスナーの接続状況確認
    /// - イベント系統のデバッグ作業
    /// - プレイモード中のイベント監視
    /// 
    /// 注意：このEditorはGameEventアセットを選択したInspectorで自動的に表示されます
    /// </summary>
    [CustomEditor(typeof(GameEvent))]
    public class GameEventEditor : UnityEditor.Editor
    {
        private GameEvent gameEvent;
        
        /// <summary>
        /// エディターが有効になった時の初期化処理
        /// 対象のGameEventアセットの参照を取得
        /// </summary>
        void OnEnable()
        {
            gameEvent = (GameEvent)target;
        }
        
        /// <summary>
        /// Inspector GUIの描画処理
        /// デフォルトのInspectorに加えて、カスタムデバッグツールを表示
        /// </summary>
        /// <remarks>
        /// 描画内容：
        /// 1. デフォルトInspector表示
        /// 2. Development Toolsセクション
        /// 3. 手動イベント発生ボタン
        /// 4. リスナー数表示
        /// 5. リスナーハイライトボタン
        /// 6. イベント履歴表示ボタン
        /// 7. プレイモード中の追加情報
        /// </remarks>
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Development Tools", EditorStyles.boldLabel);
            
            // 手動イベント発行ボタン
            if (GUILayout.Button("🔥 Raise Event"))
            {
                gameEvent.Raise();
            }
            
            // リスナー情報表示
            EditorGUILayout.LabelField($"Active Listeners: {gameEvent.GetListenerCount()}");
            
            // 全リスナーをシーンでハイライト
            if (GUILayout.Button("🎯 Highlight All Listeners in Scene"))
            {
                HighlightListenersInScene();
            }
            
            // イベント履歴表示
            if (GUILayout.Button("📊 Show Event History"))
            {
                EventHistoryWindow.ShowWindow(gameEvent);
            }
            
            // プレイモード中の追加情報
            if (Application.isPlaying)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Runtime Information", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Listener Count: {gameEvent.GetListenerCount()}");
                
                if (GUILayout.Button("🔍 Log All Listeners"))
                {
                    LogAllListeners();
                }
            }
        }
        
        /// <summary>
        /// シーン内のこのGameEventを参照している全リスナーをハイライト
        /// 対象リスナーが見つかった場合は選択状態に設定し、最初のリスナーにPingする
        /// </summary>
        /// <remarks>
        /// 処理フロー：
        /// 1. シーン内の全GameEventListenerを検索
        /// 2. EventプロパティがこのGameEventと一致するものをフィルタリング
        /// 3. 見つかったリスナーのGameObjectをSelection.objectsに設定
        /// 4. 最初のリスナーにEditorGUIUtility.PingObjectでフォーカス
        /// 5. 結果をコンソールにログ出力
        /// </remarks>
        private void HighlightListenersInScene()
        {
            var listeners = FindObjectsByType<GameEventListener>(FindObjectsSortMode.None);
            var targetListeners = System.Array.FindAll(listeners, l => l.Event == gameEvent);
            
            if (targetListeners.Length > 0)
            {
                Selection.objects = targetListeners.Select(l => l.gameObject).ToArray();
                EditorGUIUtility.PingObject(targetListeners[0]);
                UnityEngine.Debug.Log($"Found {targetListeners.Length} listeners for '{gameEvent.name}'");
            }
            else
            {
                UnityEngine.Debug.LogWarning($"No listeners found for '{gameEvent.name}'");
            }
        }
        
        /// <summary>
        /// このGameEventを参照している全リスナーの詳細情報をコンソールにログ出力
        /// プレイモード中のみ利用可能で、リスナーの名前、優先度、所属シーンを表示
        /// </summary>
        /// <remarks>
        /// 出力形式：
        /// === Listeners for '[GameEvent名]' ===
        ///   - [GameObject名] (Priority: [n]) - [シーン名]
        /// 
        /// リスナーが見つからない場合は "No listeners found." を表示
        /// 各ログにはリスナーのGameObjectへの参照が含まれる
        /// </remarks>
        private void LogAllListeners()
        {
            var listeners = FindObjectsByType<GameEventListener>(FindObjectsSortMode.None);
            var targetListeners = System.Array.FindAll(listeners, l => l.Event == gameEvent);
            
            UnityEngine.Debug.Log($"=== Listeners for '{gameEvent.name}' ===");
            foreach (var listener in targetListeners)
            {
                if (listener != null)
                {
                    UnityEngine.Debug.Log($"  - {listener.gameObject.name} (Priority: {listener.Priority}) - {listener.gameObject.scene.name}", listener);
                }
            }
            
            if (targetListeners.Length == 0)
            {
                UnityEngine.Debug.Log("  No listeners found.");
            }
        }
    }
}