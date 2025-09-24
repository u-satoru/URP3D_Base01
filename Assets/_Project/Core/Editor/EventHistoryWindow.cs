using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using asterivo.Unity60.Core.Events;

namespace asterivo.Unity60.Core.Editor
{
    /// <summary>
    /// イベント履歴管理ウィンドウ
    /// ゲーム実行中に発生したイベントの履歴を記録・表示するデバッグツール
    /// 
    /// 主な機能：
    /// - イベント発生履歴の時系列表示
    /// - タイムスタンプ、リスナー数の表示
    /// - イベント名による検索フィルター
    /// - 特定イベントのフィルタリング表示
    /// - スタックトレース表示の切り替え
    /// - CSVエクスポート機能
    /// - イベント名に基づいた自動色分け
    /// 
    /// 使用シーン：
    /// - イベントの発生順序と頻度の分析
    /// - イベントタイミングのデバッグ
    /// - パフォーマンス問題の特定
    /// - GameEventエディターからの特定イベント履歴表示
    /// 
    /// アクセス方法：Unity メニュー > asterivo.Unity60/Tools/Event History
    /// </summary>
    public class EventHistoryWindow : EditorWindow
    {
        private static List<EventLogEntry> eventHistory = new List<EventLogEntry>();
        private Vector2 scrollPosition;
        private GameEvent targetEvent;
        private bool autoScroll = true;
        private string searchFilter = "";
        private bool showStackTrace = false;
        
        /// <summary>
        /// イベントログエントリ
        /// イベントの発生情報を記録するデータクラス
        /// </summary>
        /// <remarks>
        /// イベント名のHashCodeを使用してHSVカラースペースで一意な色を生成
        /// </remarks>
        [System.Serializable]
        public class EventLogEntry
        {
            public string eventName;
            public float timestamp;
            public int listenerCount;
            public string stackTrace;
            public Color color;
            
            /// <summary>
            /// イベントログエントリのコンストラクタ
            /// </summary>
            /// <param name="name">イベント名</param>
            /// <param name="time">タイムスタンプ（秒）</param>
            /// <param name="listeners">リスナー数</param>
            public EventLogEntry(string name, float time, int listeners)
            {
                eventName = name;
                timestamp = time;
                listenerCount = listeners;
                stackTrace = Environment.StackTrace;
                
                // イベント名に基づいた色分け
                var hash = name.GetHashCode();
                color = Color.HSVToRGB((hash % 360) / 360f, 0.7f, 0.9f);
            }
        }
        
        /// <summary>
        /// イベント履歴ウィンドウを表示（フィルターなし）
        /// Unityメニューから呼び出されるエディタ拡張メニューアイテム
        /// </summary>
        [MenuItem("asterivo.Unity60/Tools/Event History")]
        public static void ShowWindow()
        {
            ShowWindow(null);
        }
        
        /// <summary>
        /// イベント履歴ウィンドウを表示（特定イベントフィルター付き）
        /// GameEventEditorから呼び出され、特定のイベントのみを表示
        /// </summary>
        /// <param name="gameEvent">フィルター対象のGameEvent。nullの場合は全イベントを表示</param>
        public static void ShowWindow(GameEvent gameEvent = null)
        {
            var window = GetWindow<EventHistoryWindow>("Event History");
            window.targetEvent = gameEvent;
            window.Show();
        }
        
        /// <summary>
        /// イベントの発生を履歴に記録
        /// GameEventの発生時に呼び出される静的メソッド
        /// </summary>
        /// <param name="eventName">発生したイベントの名前</param>
        /// <param name="listenerCount">その時点でのリスナー数</param>
        /// <remarks>
        /// 履歴は最大1000エントリまで保持され、古いエントリから順次削除されます。
        /// Time.realtimeSinceStartupをタイムスタンプとして使用しています。
        /// </remarks>
        public static void LogEvent(string eventName, int listenerCount)
        {
            eventHistory.Add(new EventLogEntry(eventName, Time.realtimeSinceStartup, listenerCount));
            if (eventHistory.Count > 1000) // 履歴制限
            {
                eventHistory.RemoveAt(0);
            }
        }
        
        void OnGUI()
        {
            DrawToolbar();
            DrawFilters();
            DrawEventList();
        }
        
        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            
            if (GUILayout.Button("Clear History", EditorStyles.toolbarButton))
            {
                eventHistory.Clear();
            }
            
            if (GUILayout.Button("Export to CSV", EditorStyles.toolbarButton))
            {
                ExportToCSV();
            }
            
            GUILayout.FlexibleSpace();
            
            autoScroll = GUILayout.Toggle(autoScroll, "Auto Scroll", EditorStyles.toolbarButton);
            showStackTrace = GUILayout.Toggle(showStackTrace, "Stack Trace", EditorStyles.toolbarButton);
            
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawFilters()
        {
            EditorGUILayout.BeginHorizontal();
            
            EditorGUILayout.LabelField("Search:", GUILayout.Width(50));
            searchFilter = EditorGUILayout.TextField(searchFilter);
            
            if (targetEvent != null)
            {
                EditorGUILayout.LabelField($"Filtered: {targetEvent.name}", GUILayout.Width(150));
                if (GUILayout.Button("Clear Filter", GUILayout.Width(80)))
                {
                    targetEvent = null;
                }
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.LabelField($"Total Events: {eventHistory.Count} | Filtered: {GetFilteredEvents().Count}");
        }
        
        private void DrawEventList()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            var filteredEvents = GetFilteredEvents();
            
            foreach (var entry in filteredEvents)
            {
                DrawEventEntry(entry);
            }
            
            if (filteredEvents.Count == 0)
            {
                EditorGUILayout.LabelField("No events to display", EditorStyles.centeredGreyMiniLabel);
            }
            
            EditorGUILayout.EndScrollView();
            
            if (autoScroll && Event.current.type == EventType.Repaint)
            {
                scrollPosition.y = float.MaxValue;
            }
        }
        
        private void DrawEventEntry(EventLogEntry entry)
        {
            var originalColor = GUI.backgroundColor;
            GUI.backgroundColor = entry.color;
            
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();
            
            EditorGUILayout.LabelField($"[{entry.timestamp:F2}s]", GUILayout.Width(80));
            EditorGUILayout.LabelField(entry.eventName, GUILayout.ExpandWidth(true));
            EditorGUILayout.LabelField($"Listeners: {entry.listenerCount}", GUILayout.Width(100));
            
            if (GUILayout.Button("📋", GUILayout.Width(25)))
            {
                GUIUtility.systemCopyBuffer = $"{entry.eventName} at {entry.timestamp:F2}s";
            }
            
            EditorGUILayout.EndHorizontal();
            
            if (showStackTrace && !string.IsNullOrEmpty(entry.stackTrace))
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField("Stack Trace:", EditorStyles.miniLabel);
                var lines = entry.stackTrace.Split('\n');
                for (int i = 0; i < Mathf.Min(lines.Length, 5); i++) // 最初の5行のみ表示
                {
                    EditorGUILayout.LabelField(lines[i], EditorStyles.miniLabel);
                }
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.EndVertical();
            
            GUI.backgroundColor = originalColor;
        }
        
        private List<EventLogEntry> GetFilteredEvents()
        {
            var filtered = eventHistory;
            
            if (targetEvent != null)
            {
                filtered = eventHistory.FindAll(e => e.eventName == targetEvent.name);
            }
            
            if (!string.IsNullOrEmpty(searchFilter))
            {
                filtered = filtered.FindAll(e => e.eventName.ToLower().Contains(searchFilter.ToLower()));
            }
            
            return filtered;
        }
        
        private void ExportToCSV()
        {
            var path = EditorUtility.SaveFilePanel("Export Event History", "", "event_history.csv", "csv");
            if (!string.IsNullOrEmpty(path))
            {
                var csv = "Timestamp,EventName,ListenerCount\n";
                foreach (var entry in eventHistory)
                {
                    csv += $"{entry.timestamp:F2},{entry.eventName},{entry.listenerCount}\n";
                }
                System.IO.File.WriteAllText(path, csv);
                UnityEngine.Debug.Log($"Event history exported to: {path}");
            }
        }
        
        void OnEnable()
        {
            // イベント履歴をGameEventに統合
            EditorApplication.update += Repaint;
        }
        
        void OnDisable()
        {
            EditorApplication.update -= Repaint;
        }
    }
}