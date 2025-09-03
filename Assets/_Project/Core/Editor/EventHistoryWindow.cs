using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using asterivo.Unity60.Core.Events;

namespace asterivo.Unity60.Core.Editor
{
    public class EventHistoryWindow : EditorWindow
    {
        private static List<EventLogEntry> eventHistory = new List<EventLogEntry>();
        private Vector2 scrollPosition;
        private GameEvent targetEvent;
        private bool autoScroll = true;
        private string searchFilter = "";
        private bool showStackTrace = false;
        
        [System.Serializable]
        public class EventLogEntry
        {
            public string eventName;
            public float timestamp;
            public int listenerCount;
            public string stackTrace;
            public Color color;
            
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
        
        [MenuItem("asterivo.Unity60/Tools/Event History")]
        public static void ShowWindow()
        {
            ShowWindow(null);
        }
        
        public static void ShowWindow(GameEvent gameEvent = null)
        {
            var window = GetWindow<EventHistoryWindow>("Event History");
            window.targetEvent = gameEvent;
            window.Show();
        }
        
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
                Debug.Log($"Event history exported to: {path}");
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