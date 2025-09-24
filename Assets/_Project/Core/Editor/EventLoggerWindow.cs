using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;
// using asterivo.Unity60.Core.Debug;

namespace asterivo.Unity60.Core.Editor
{
    /// <summary>
    /// EventLogger蟆ら畑縺ｮ繝・ヰ繝・げ繧ｦ繧｣繝ｳ繝峨え
    /// 繝ｪ繧｢繝ｫ繧ｿ繧､繝縺ｧ繧､繝吶Φ繝医Ο繧ｰ繧定｡ｨ遉ｺ縺励√ヵ繧｣繝ｫ繧ｿ繝ｪ繝ｳ繧ｰ繝ｻ蛻・梵讖溯・繧呈署萓・    /// 
    /// 荳ｻ縺ｪ讖溯・・・    /// - 繝ｪ繧｢繝ｫ繧ｿ繧､繝繧､繝吶Φ繝医Ο繧ｰ陦ｨ遉ｺ
    /// - 繝ｭ繧ｰ繝ｬ繝吶Ν蛻･繝輔ぅ繝ｫ繧ｿ繝ｪ繝ｳ繧ｰ・・nfo/Warning/Error・・    /// - 繧､繝吶Φ繝亥錐縺ｫ繧医ｋ讀懃ｴ｢繝輔ぅ繝ｫ繧ｿ繝ｼ
    /// - 繧､繝吶Φ繝医ち繧､繝怜挨繝輔ぅ繝ｫ繧ｿ繝ｪ繝ｳ繧ｰ
    /// - 繝壹う繝ｭ繝ｼ繝峨ョ繝ｼ繧ｿ縺ｨ繧ｹ繧ｿ繝・け繝医Ξ繝ｼ繧ｹ陦ｨ遉ｺ
    /// - 邨ｱ險域ュ蝣ｱ・育ｷ上う繝吶Φ繝域焚縲√Ξ繝吶Ν蛻･繧ｫ繧ｦ繝ｳ繝育ｭ会ｼ・    /// - CSV繧ｨ繧ｯ繧ｹ繝昴・繝域ｩ溯・
    /// - 繧ｯ繝ｪ繝・・繝懊・繝峨さ繝斐・讖溯・
    /// 
    /// 菴ｿ逕ｨ繧ｷ繝ｼ繝ｳ・・    /// - 繧､繝吶Φ繝医・逋ｺ逕溽憾豕∫屮隕・    /// - 繧､繝吶Φ繝医す繧ｹ繝・Β縺ｮ繝・ヰ繝・げ
    /// - 繝代ヵ繧ｩ繝ｼ繝槭Φ繧ｹ蝠城｡後・迚ｹ螳・    /// - 繧､繝吶Φ繝医・騾｣邯夂匱逕溘ｄ繝ｫ繝ｼ繝励・讀懷・
    /// 
    /// 繧｢繧ｯ繧ｻ繧ｹ譁ｹ豕包ｼ啅nity 繝｡繝九Η繝ｼ > asterivo.Unity60/Debug/Event Logger
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
        
        // 邨ｱ險域ュ蝣ｱ
        private EventLogger.LogStatistics lastStatistics;
        private float nextStatsUpdate = 0f;
        private const float STATS_UPDATE_INTERVAL = 1f;
        
        // 繝輔ぅ繝ｫ繧ｿ繝ｪ繝ｳ繧ｰ逕ｨ
        private HashSet<string> selectedEventTypes = new HashSet<string>();
        private bool showEventTypeFilter = false;
        
        /// <summary>
        /// 繧､繝吶Φ繝医Ο繧ｬ繝ｼ繧ｦ繧｣繝ｳ繝峨え繧定｡ｨ遉ｺ
        /// Unity繝｡繝九Η繝ｼ縺九ｉ蜻ｼ縺ｳ蜃ｺ縺輔ｌ繧九お繝・ぅ繧ｿ諡｡蠑ｵ繝｡繝九Η繝ｼ繧｢繧､繝・Β
        /// </summary>
        /// <remarks>
        /// 繧ｦ繧｣繝ｳ繝峨え縺ｮ譛蟆上し繧､繧ｺ縺ｯ600x400縺ｫ險ｭ螳壹＆繧後・        /// 繧ｨ繝・ぅ繧ｿ縺ｮ譖ｴ譁ｰ繧､繝吶Φ繝医↓逋ｻ骭ｲ縺励※閾ｪ蜍輔Μ繝輔Ξ繝・す繝･繧定｡後＞縺ｾ縺吶・        /// </remarks>
        [MenuItem("asterivo.Unity60/Debug/Event Logger")]
        public static void ShowWindow()
        {
            var window = GetWindow<EventLoggerWindow>("Event Logger");
            window.minSize = new Vector2(600, 400);
            window.Show();
        }
        
        /// <summary>
        /// 繧ｦ繧｣繝ｳ繝峨え縺梧怏蜉ｹ縺ｫ縺ｪ縺｣縺滓凾縺ｮ蛻晄悄蛹門・逅・        /// 繧ｨ繝・ぅ繧ｿ縺ｮ譖ｴ譁ｰ繧､繝吶Φ繝医↓逋ｻ骭ｲ縺励∫ｵｱ險域ュ蝣ｱ繧貞・譛溷喧
        /// </summary>
        void OnEnable()
        {
            EditorApplication.update += OnEditorUpdate;
            lastStatistics = new EventLogger.LogStatistics();
        }
        
        /// <summary>
        /// 繧ｦ繧｣繝ｳ繝峨え縺檎┌蜉ｹ縺ｫ縺ｪ縺｣縺滓凾縺ｮ繧ｯ繝ｪ繝ｼ繝ｳ繧｢繝・・蜃ｦ逅・        /// 繧ｨ繝・ぅ繧ｿ縺ｮ譖ｴ譁ｰ繧､繝吶Φ繝医°繧臥匳骭ｲ隗｣髯､
        /// </summary>
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
            
            // 邨ｱ險域ュ蝣ｱ縺ｮ螳壽悄譖ｴ譁ｰ
            if (Time.realtimeSinceStartup >= nextStatsUpdate)
            {
                lastStatistics = EventLogger.GetStatisticsStatic();
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
            
            // 蝓ｺ譛ｬ謫堺ｽ懊・繧ｿ繝ｳ
            if (GUILayout.Button("卵・・Clear", EditorStyles.toolbarButton))
            {
                EventLogger.ClearLogStatic();
            }
            
            if (GUILayout.Button("投 Export CSV", EditorStyles.toolbarButton))
            {
                ExportToCSV();
            }
            
            if (GUILayout.Button("搭 Copy All", EditorStyles.toolbarButton))
            {
                CopyAllToClipboard();
            }
            
            // 荳譎ょ●豁｢繝懊ち繝ｳ
            isPaused = GUILayout.Toggle(isPaused, isPaused ? "竢ｸ・・Paused" : "笆ｶ・・Live", EditorStyles.toolbarButton);
            
            GUILayout.FlexibleSpace();
            
            // 陦ｨ遉ｺ繧ｪ繝励す繝ｧ繝ｳ
            autoScroll = GUILayout.Toggle(autoScroll, "Auto Scroll", EditorStyles.toolbarButton);
            showPayload = GUILayout.Toggle(showPayload, "Payload", EditorStyles.toolbarButton);
            showStackTrace = GUILayout.Toggle(showStackTrace, "Stack", EditorStyles.toolbarButton);
            showStatistics = GUILayout.Toggle(showStatistics, "Stats", EditorStyles.toolbarButton);
            
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawStatistics()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("投 Event Statistics", EditorStyles.boldLabel);
            
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
            
            EditorGUILayout.LabelField($"Types: {lastStatistics.eventCounts.Count}", GUILayout.Width(70));
            EditorGUILayout.EndHorizontal();
            
            if (lastStatistics.eventCounts.Count > 0)
            {
                var mostFrequent = lastStatistics.eventCounts.OrderByDescending(kvp => kvp.Value).First();
                EditorGUILayout.LabelField($"Most Frequent: {mostFrequent.Key} ({mostFrequent.Value}x)", EditorStyles.miniLabel);
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawFilters()
        {
            EditorGUILayout.BeginHorizontal();
            
            // 讀懃ｴ｢繝輔ぅ繝ｫ繧ｿ繝ｼ
            EditorGUILayout.LabelField("剥", GUILayout.Width(20));
            searchFilter = EditorGUILayout.TextField(searchFilter, EditorStyles.toolbarSearchField);
            
            // 繝ｬ繝吶Ν繝輔ぅ繝ｫ繧ｿ繝ｼ
            EditorGUILayout.LabelField("Level:", GUILayout.Width(40));
            filterLevel = (EventLogger.LogLevel)EditorGUILayout.EnumPopup(filterLevel, GUILayout.Width(80));
            
            // 繧､繝吶Φ繝医ち繧､繝励ヵ繧｣繝ｫ繧ｿ繝ｼ蛻・ｊ譖ｿ縺・            showEventTypeFilter = EditorGUILayout.Toggle("Types", showEventTypeFilter, GUILayout.Width(60));
            
            EditorGUILayout.EndHorizontal();
            
            // 繧､繝吶Φ繝医ち繧､繝励ヵ繧｣繝ｫ繧ｿ繝ｼ陦ｨ遉ｺ
            if (showEventTypeFilter)
            {
                DrawEventTypeFilter();
            }
        }
        
        private void DrawEventTypeFilter()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Event Type Filters:", EditorStyles.miniLabel);
            
            // 蛻ｩ逕ｨ蜿ｯ閭ｽ縺ｪ繧､繝吶Φ繝医ち繧､繝励ｒ蜿朱寔
            var availableTypes = new HashSet<string>();
            foreach (var entry in EventLogger.EventLogStatic)
            {
                availableTypes.Add(entry.eventName);
            }
            
            // 繧､繝吶Φ繝医ち繧､繝励・繝医げ繝ｫ陦ｨ遉ｺ・域怙螟ｧ10蛟九∪縺ｧ・・            var typesToShow = availableTypes.Take(10).ToArray();
            
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
            
            EditorGUILayout.LabelField($"Showing {filteredLog.Count} / {EventLogger.EventLogStatic.Count} entries", EditorStyles.miniLabel);
            
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
            
            // 閾ｪ蜍輔せ繧ｯ繝ｭ繝ｼ繝ｫ
            if (autoScroll && Event.current.type == EventType.Repaint && !isPaused)
            {
                scrollPosition.y = float.MaxValue;
            }
        }
        
        private void DrawLogEntry(EventLogger.EventLogEntry entry)
        {
            var originalColor = GUI.backgroundColor;
            
            // 繝ｬ繝吶Ν蛻･縺ｮ濶ｲ蛻・￠
            switch (entry.level)
            {
                case EventLogger.LogLevel.Warning:
                    GUI.backgroundColor = new Color(1f, 1f, 0.3f, 0.3f);
                    break;
                case EventLogger.LogLevel.Error:
                    GUI.backgroundColor = new Color(1f, 0.3f, 0.3f, 0.3f);
                    break;
                default:
                    GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f, 0.3f); // Default color for Info level
                    break;
            }
            
            EditorGUILayout.BeginVertical("box");
            
            // 繝｡繧､繝ｳ繝ｩ繧､繝ｳ
            EditorGUILayout.BeginHorizontal();
            
            // 繧ｿ繧､繝繧ｹ繧ｿ繝ｳ繝・            EditorGUILayout.LabelField($"[{entry.timestamp:F2}s]", GUILayout.Width(80));
            
            // 繝ｬ繝吶Ν繧｢繧､繧ｳ繝ｳ
            string levelIcon = GetLevelIcon(entry.level);
            EditorGUILayout.LabelField(levelIcon, GUILayout.Width(20));
            
            // 繧､繝吶Φ繝亥錐
            var eventNameStyle = new GUIStyle(EditorStyles.label);
            eventNameStyle.fontStyle = FontStyle.Bold;
            EditorGUILayout.LabelField(entry.eventName, eventNameStyle, GUILayout.ExpandWidth(true));
            
            // 繝ｪ繧ｹ繝翫・謨ｰ
            EditorGUILayout.LabelField($"({entry.listenerCount})", GUILayout.Width(40));
            
            // 繧ｳ繝斐・繝懊ち繝ｳ
            if (GUILayout.Button("搭", GUILayout.Width(25)))
            {
                CopyEntryToClipboard(entry);
            }
            
            EditorGUILayout.EndHorizontal();
            
            // 繝壹う繝ｭ繝ｼ繝芽｡ｨ遉ｺ
            if (showPayload && !string.IsNullOrEmpty(entry.payload))
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField($"Payload: {entry.payload}", EditorStyles.wordWrappedMiniLabel);
                EditorGUI.indentLevel--;
            }
            
            // Stack trace not available in current EventLogEntry structure
            // TODO: Implement stack trace collection if needed for debugging
            
            EditorGUILayout.EndVertical();
            
            GUI.backgroundColor = originalColor;
        }
        
        private List<EventLogger.EventLogEntry> GetFilteredLog()
        {
            var log = isPaused ? EventLogger.EventLogStatic.ToList() : EventLogger.EventLogStatic;
            var filtered = new List<EventLogger.EventLogEntry>();
            
            foreach (var entry in log)
            {
                // 繝ｬ繝吶Ν繝輔ぅ繝ｫ繧ｿ繝ｼ
                if (entry.level < filterLevel) continue;
                
                // 讀懃ｴ｢繝輔ぅ繝ｫ繧ｿ繝ｼ
                if (!string.IsNullOrEmpty(searchFilter))
                {
                    bool matchesSearch = entry.eventName.ToLower().Contains(searchFilter.ToLower()) ||
                                       entry.payload.ToLower().Contains(searchFilter.ToLower());
                    if (!matchesSearch) continue;
                }
                
                // 繧､繝吶Φ繝医ち繧､繝励ヵ繧｣繝ｫ繧ｿ繝ｼ
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
                case EventLogger.LogLevel.Info: return "邃ｹ・・;
                case EventLogger.LogLevel.Warning: return "笞・・;
                case EventLogger.LogLevel.Error: return "笶・;
                default: return "統";
            }
        }
        
        private void ExportToCSV()
        {
            string path = EditorUtility.SaveFilePanel("Export Event Log", "", "EventLog.csv", "csv");
            if (!string.IsNullOrEmpty(path))
            {
                EventLogger.ExportToCSVStatic(path);
            }
        }
        
        private void CopyEntryToClipboard(EventLogger.EventLogEntry entry)
        {
            string text = $"[{entry.timestamp:F2}s] [{entry.level}] {entry.eventName} ({entry.listenerCount} listeners)";
            if (!string.IsNullOrEmpty(entry.payload))
            {
                text += $" - {entry.payload}";
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
                if (!string.IsNullOrEmpty(entry.payload))
                {
                    text += $" - {entry.payload}";
                }
                text += "\n";
            }
            
            GUIUtility.systemCopyBuffer = text;
            UnityEngine.Debug.Log($"Copied {filteredLog.Count} log entries to clipboard");
        }
    }
}
