using UnityEngine;
using UnityEditor;
using System.Linq;
using asterivo.Unity60.Core.Events;

namespace asterivo.Unity60.Core.Editor
{
    [CustomEditor(typeof(GameEvent))]
    public class GameEventEditor : UnityEditor.Editor
    {
        private GameEvent gameEvent;
        
        void OnEnable()
        {
            gameEvent = (GameEvent)target;
        }
        
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
        
        private void HighlightListenersInScene()
        {
            var listeners = FindObjectsByType<GameEventListener>(FindObjectsSortMode.None);
            var targetListeners = System.Array.FindAll(listeners, l => l.Event == gameEvent);
            
            if (targetListeners.Length > 0)
            {
                Selection.objects = targetListeners.Select(l => l.gameObject).ToArray();
                EditorGUIUtility.PingObject(targetListeners[0]);
                Debug.Log($"Found {targetListeners.Length} listeners for '{gameEvent.name}'");
            }
            else
            {
                Debug.LogWarning($"No listeners found for '{gameEvent.name}'");
            }
        }
        
        private void LogAllListeners()
        {
            var listeners = FindObjectsByType<GameEventListener>(FindObjectsSortMode.None);
            var targetListeners = System.Array.FindAll(listeners, l => l.Event == gameEvent);
            
            Debug.Log($"=== Listeners for '{gameEvent.name}' ===");
            foreach (var listener in targetListeners)
            {
                if (listener != null)
                {
                    Debug.Log($"  - {listener.gameObject.name} (Priority: {listener.Priority}) - {listener.gameObject.scene.name}", listener);
                }
            }
            
            if (targetListeners.Length == 0)
            {
                Debug.Log("  No listeners found.");
            }
        }
    }
}