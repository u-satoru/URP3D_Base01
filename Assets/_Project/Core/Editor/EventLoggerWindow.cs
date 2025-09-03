using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using asterivo.Unity60.Core.Debug;

namespace asterivo.Unity60.Core.Editor
{
    /// <summary>
    /// EventLogger専用のデバッグウィンドウ
    /// リアルタイムでイベントログを表示し、フィルタリング・分析機能を提供
    /// </summary>
    public class EventLoggerWindow : EditorWindow
    {
        private Vector2 scrollPosition;
        private string searchFilter = "";
        private EventLogger.LogLevel filterLevel = EventLogger.LogLevel.Info;
        private bool autoScroll = true;
        private bool showPayload = true;
        private bool showStackTrace = false;
        private bool showStatistics = true;
        private bool isPaused = false;
        
        // 統計情報
        private EventLogger.LogStatistics lastStatistics;
        private float nextStatsUpdate = 0f;
        private const float STATS_UPDATE_INTERVAL = 1f;
        
        // フィルタリング用
        private HashSet<string> selectedEventTypes = new HashSet<string>();
        private bool showEventTypeFilter = false;
        
        [MenuItem("asterivo.Unity60/Debug/Event Logger")]
        public static void ShowWindow()
        {
            var window = GetWindow<EventLoggerWindow>("Event Logger");
            window.minSize = new Vector2(600, 400);
            window.Show();
        }
        
        void OnEnable()
        {
            EditorApplication.update += OnEditorUpdate;
            lastStatistics = new EventLogger.LogStatistics();
        }
        
        void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
        }
        
        void OnEditorUpdate()
        {
            if (!isPaused && autoScroll)
            {
                Repaint();
            }
            
            // 統計情報の定期更新
            if (Time.realtimeSinceStartup >= nextStatsUpdate)
            {
                lastStatistics = EventLogger.GetStatistics();
                nextStatsUpdate = Time.realtimeSinceStartup + STATS_UPDATE_INTERVAL;
            }
        }
        
        void OnGUI()
        {
            DrawToolbar();
            
            if (showStatistics)
            {
                DrawStatistics();
            }
            
            DrawFilters();
            DrawLogContent();
        }
        
        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            
            // 基本操作ボタン
            if (GUILayout.Button("🗑️ Clear", EditorStyles.toolbarButton))
            {
                EventLogger.ClearLog();
            }
            
            if (GUILayout.Button("📊 Export CSV", EditorStyles.toolbarButton))
            {
                ExportToCSV();
            }
            
            if (GUILayout.Button("📋 Copy All", EditorStyles.toolbarButton))
            {
                CopyAllToClipboard();
            }
            
            // 一時停止ボタン
            isPaused = GUILayout.Toggle(isPaused, isPaused ? "⏸️ Paused" : "▶️ Live", EditorStyles.toolbarButton);
            
            GUILayout.FlexibleSpace();
            
            // 表示オプション
            autoScroll = GUILayout.Toggle(autoScroll, "Auto Scroll", EditorStyles.toolbarButton);
            showPayload = GUILayout.Toggle(showPayload, "Payload", EditorStyles.toolbarButton);
            showStackTrace = GUILayout.Toggle(showStackTrace, "Stack", EditorStyles.toolbarButton);
            showStatistics = GUILayout.Toggle(showStatistics, "Stats", EditorStyles.toolbarButton);
            
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawStatistics()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("📊 Event Statistics", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Total: {lastStatistics.totalEvents}", GUILayout.Width(80));
            EditorGUILayout.LabelField($"Info: {lastStatistics.infoCount}", GUILayout.Width(60));
            
            if (lastStatistics.warningCount > 0)
            {
                var warningStyle = new GUIStyle(EditorStyles.label);
                warningStyle.normal.textColor = Color.yellow;
                EditorGUILayout.LabelField($"Warnings: {lastStatistics.warningCount}", warningStyle, GUILayout.Width(80));
            }
            
            if (lastStatistics.errorCount > 0)
            {
                var errorStyle = new GUIStyle(EditorStyles.label);
                errorStyle.normal.textColor = Color.red;
                EditorGUILayout.LabelField($"Errors: {lastStatistics.errorCount}", errorStyle, GUILayout.Width(70));
            }
            
            EditorGUILayout.LabelField($"Types: {lastStatistics.uniqueEventTypes}", GUILayout.Width(70));
            EditorGUILayout.EndHorizontal();
            
            if (!string.IsNullOrEmpty(lastStatistics.mostFrequentEvent))
            {
                EditorGUILayout.LabelField($"Most Frequent: {lastStatistics.mostFrequentEvent} ({lastStatistics.mostFrequentEventCount}x)", EditorStyles.miniLabel);
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawFilters()
        {
            EditorGUILayout.BeginHorizontal();
            
            // 検索フィルター
            EditorGUILayout.LabelField("🔍", GUILayout.Width(20));
            searchFilter = EditorGUILayout.TextField(searchFilter, EditorStyles.toolbarSearchField);
            
            // レベルフィルター
            EditorGUILayout.LabelField("Level:", GUILayout.Width(40));
            filterLevel = (EventLogger.LogLevel)EditorGUILayout.EnumPopup(filterLevel, GUILayout.Width(80));
            
            // イベントタイプフィルター切り替え
            showEventTypeFilter = EditorGUILayout.Toggle("Types", showEventTypeFilter, GUILayout.Width(60));
            
            EditorGUILayout.EndHorizontal();
            
            // イベントタイプフィルター表示
            if (showEventTypeFilter)
            {
                DrawEventTypeFilter();
            }
        }
        
        private void DrawEventTypeFilter()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Event Type Filters:", EditorStyles.miniLabel);
            
            // 利用可能なイベントタイプを収集
            var availableTypes = new HashSet<string>();
            foreach (var entry in EventLogger.EventLog)
            {
                availableTypes.Add(entry.eventName);
            }
            
            // イベントタイプのトグル表示（最大10個まで）
            var typesToShow = availableTypes.Take(10).ToArray();
            
            EditorGUILayout.BeginHorizontal();
            int count = 0;
            foreach (var eventType in typesToShow)
            {
                if (count % 5 == 0 && count > 0)
                {
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                }
                
                bool isSelected = selectedEventTypes.Contains(eventType);
                bool newSelection = EditorGUILayout.Toggle(eventType, isSelected);
                
                if (newSelection != isSelected)
                {
                    if (newSelection)
                        selectedEventTypes.Add(eventType);
                    else
                        selectedEventTypes.Remove(eventType);
                }
                
                count++;
            }
            EditorGUILayout.EndHorizontal();
            
            if (availableTypes.Count > 10)
            {
                EditorGUILayout.LabelField($"... and {availableTypes.Count - 10} more types", EditorStyles.miniLabel);
            }
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Select All", EditorStyles.miniButton))
            {
                selectedEventTypes.UnionWith(availableTypes);
            }
            if (GUILayout.Button("Clear All", EditorStyles.miniButton))
            {
                selectedEventTypes.Clear();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawLogContent()
        {
            var filteredLog = GetFilteredLog();
            
            EditorGUILayout.LabelField($"Showing {filteredLog.Count} / {EventLogger.EventLog.Count} entries", EditorStyles.miniLabel);
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            foreach (var entry in filteredLog)
            {
                DrawLogEntry(entry);
            }
            
            if (filteredLog.Count == 0)
            {
                EditorGUILayout.LabelField("No entries match the current filters", EditorStyles.centeredGreyMiniLabel);
            }
            
            EditorGUILayout.EndScrollView();
            
            // 自動スクロール
            if (autoScroll && Event.current.type == EventType.Repaint && !isPaused)
            {
                scrollPosition.y = float.MaxValue;
            }
        }
        
        private void DrawLogEntry(EventLogger.EventLogEntry entry)
        {
            var originalColor = GUI.backgroundColor;
            
            // レベル別の色分け
            switch (entry.level)
            {
                case EventLogger.LogLevel.Warning:
                    GUI.backgroundColor = new Color(1f, 1f, 0.3f, 0.3f);
                    break;
                case EventLogger.LogLevel.Error:
                    GUI.backgroundColor = new Color(1f, 0.3f, 0.3f, 0.3f);
                    break;
                default:
                    GUI.backgroundColor = entry.displayColor * 0.3f;
                    break;
            }
            
            EditorGUILayout.BeginVertical("box");
            
            // メインライン
            EditorGUILayout.BeginHorizontal();
            
            // タイムスタンプ
            EditorGUILayout.LabelField($"[{entry.timestamp:F2}s]", GUILayout.Width(80));
            
            // レベルアイコン
            string levelIcon = GetLevelIcon(entry.level);
            EditorGUILayout.LabelField(levelIcon, GUILayout.Width(20));
            
            // イベント名
            var eventNameStyle = new GUIStyle(EditorStyles.label);
            eventNameStyle.fontStyle = FontStyle.Bold;
            EditorGUILayout.LabelField(entry.eventName, eventNameStyle, GUILayout.ExpandWidth(true));
            
            // リスナー数
            EditorGUILayout.LabelField($"({entry.listenerCount})", GUILayout.Width(40));
            
            // コピーボタン
            if (GUILayout.Button("📋", GUILayout.Width(25)))
            {
                CopyEntryToClipboard(entry);
            }
            
            EditorGUILayout.EndHorizontal();
            
            // ペイロード表示
            if (showPayload && !string.IsNullOrEmpty(entry.payloadData))
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField($"Payload: {entry.payloadData}", EditorStyles.wordWrappedMiniLabel);
                EditorGUI.indentLevel--;
            }
            
            // スタックトレース表示
            if (showStackTrace && !string.IsNullOrEmpty(entry.stackTrace))
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField("Stack Trace:", EditorStyles.miniLabel);
                var lines = entry.stackTrace.Split('\n');
                for (int i = 0; i < Mathf.Min(lines.Length, 3); i++) // 最初の3行のみ
                {
                    EditorGUILayout.LabelField(lines[i], EditorStyles.wordWrappedMiniLabel);
                }
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.EndVertical();
            
            GUI.backgroundColor = originalColor;
        }
        
        private List<EventLogger.EventLogEntry> GetFilteredLog()
        {
            var log = isPaused ? EventLogger.EventLog.ToList() : EventLogger.EventLog;
            var filtered = new List<EventLogger.EventLogEntry>();
            
            foreach (var entry in log)
            {
                // レベルフィルター
                if (entry.level < filterLevel) continue;
                
                // 検索フィルター
                if (!string.IsNullOrEmpty(searchFilter))
                {
                    bool matchesSearch = entry.eventName.ToLower().Contains(searchFilter.ToLower()) ||
                                       entry.payloadData.ToLower().Contains(searchFilter.ToLower());
                    if (!matchesSearch) continue;
                }
                
                // イベントタイプフィルター
                if (selectedEventTypes.Count > 0 && !selectedEventTypes.Contains(entry.eventName))
                {
                    continue;
                }
                
                filtered.Add(entry);
            }
            
            return filtered;
        }
        
        private string GetLevelIcon(EventLogger.LogLevel level)
        {
            switch (level)
            {
                case EventLogger.LogLevel.Info: return "ℹ️";
                case EventLogger.LogLevel.Warning: return "⚠️";
                case EventLogger.LogLevel.Error: return "❌";
                default: return "📝";
            }
        }
        
        private void ExportToCSV()
        {
            string path = EditorUtility.SaveFilePanel("Export Event Log", "", "EventLog.csv", "csv");
            if (!string.IsNullOrEmpty(path))
            {
                EventLogger.ExportToCSV(path);
            }
        }
        
        private void CopyEntryToClipboard(EventLogger.EventLogEntry entry)
        {
            string text = $"[{entry.timestamp:F2}s] [{entry.level}] {entry.eventName} ({entry.listenerCount} listeners)";
            if (!string.IsNullOrEmpty(entry.payloadData))
            {
                text += $" - {entry.payloadData}";
            }
            
            GUIUtility.systemCopyBuffer = text;
            UnityEngine.Debug.Log("Log entry copied to clipboard");
        }
        
        private void CopyAllToClipboard()
        {
            var filteredLog = GetFilteredLog();
            var text = "";
            
            foreach (var entry in filteredLog)
            {
                text += $"[{entry.timestamp:F2}s] [{entry.level}] {entry.eventName} ({entry.listenerCount} listeners)";
                if (!string.IsNullOrEmpty(entry.payloadData))
                {
                    text += $" - {entry.payloadData}";
                }
                text += "\n";
            }
            
            GUIUtility.systemCopyBuffer = text;
            UnityEngine.Debug.Log($"Copied {filteredLog.Count} log entries to clipboard");
        }
    }
}