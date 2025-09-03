using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using asterivo.Unity60.Core.Events;

namespace asterivo.Unity60.Core.Editor
{
    public class EventFlowVisualizer : EditorWindow
    {
        private Dictionary<GameEvent, List<GameEventListener>> eventConnections;
        private Vector2 scrollPosition;
        private bool showOnlyActiveListeners = false;
        private string searchFilter = "";
        private bool showStatistics = true;
        private bool groupByScene = false;
        
        [MenuItem("asterivo.Unity60/Tools/Event Flow Visualizer")]
        public static void ShowWindow()
        {
            GetWindow<EventFlowVisualizer>("Event Flow Visualizer").Show();
        }
        
        void OnEnable()
        {
            RefreshConnections();
        }
        
        void OnGUI()
        {
            DrawToolbar();
            DrawFilters();
            
            if (showStatistics)
            {
                DrawStatistics();
            }
            
            DrawEventConnections();
        }
        
        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            
            if (GUILayout.Button("🔄 Refresh", EditorStyles.toolbarButton))
            {
                RefreshConnections();
            }
            
            if (GUILayout.Button("📋 Export Report", EditorStyles.toolbarButton))
            {
                ExportToConsole();
            }
            
            if (GUILayout.Button("🔍 Find Issues", EditorStyles.toolbarButton))
            {
                FindPotentialIssues();
            }
            
            GUILayout.FlexibleSpace();
            
            showStatistics = GUILayout.Toggle(showStatistics, "Statistics", EditorStyles.toolbarButton);
            groupByScene = GUILayout.Toggle(groupByScene, "Group by Scene", EditorStyles.toolbarButton);
            
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawFilters()
        {
            EditorGUILayout.BeginHorizontal();
            
            EditorGUILayout.LabelField("Search:", GUILayout.Width(50));
            searchFilter = EditorGUILayout.TextField(searchFilter);
            
            showOnlyActiveListeners = EditorGUILayout.Toggle("Active Only", showOnlyActiveListeners);
            
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawStatistics()
        {
            if (eventConnections == null)
            {
                RefreshConnections();
                return;
            }
            
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("📊 Event Statistics", EditorStyles.boldLabel);
            
            var totalEvents = eventConnections.Count;
            var totalListeners = eventConnections.Values.SelectMany(l => l).Count();
            var orphanedListeners = FindObjectsByType<GameEventListener>(FindObjectsSortMode.None).Where(l => l.Event == null).Count();
            var unusedEvents = eventConnections.Where(kvp => kvp.Value.Count == 0).Count();
            
            EditorGUILayout.LabelField($"Total Events: {totalEvents}");
            EditorGUILayout.LabelField($"Total Listeners: {totalListeners}");
            
            if (orphanedListeners > 0)
            {
                EditorGUILayout.LabelField($"⚠️ Orphaned Listeners: {orphanedListeners}", new GUIStyle(EditorStyles.label) { normal = { textColor = Color.yellow } });
            }
            
            if (unusedEvents > 0)
            {
                EditorGUILayout.LabelField($"⚠️ Unused Events: {unusedEvents}", new GUIStyle(EditorStyles.label) { normal = { textColor = Color.yellow } });
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawEventConnections()
        {
            if (eventConnections == null)
            {
                RefreshConnections();
                return;
            }
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            var filteredConnections = GetFilteredConnections();
            
            if (groupByScene)
            {
                DrawConnectionsByScene(filteredConnections);
            }
            else
            {
                DrawConnectionsFlat(filteredConnections);
            }
            
            EditorGUILayout.EndScrollView();
        }
        
        private void DrawConnectionsFlat(Dictionary<GameEvent, List<GameEventListener>> connections)
        {
            foreach (var kvp in connections)
            {
                DrawEventGroup(kvp.Key, kvp.Value);
            }
        }
        
        private void DrawConnectionsByScene(Dictionary<GameEvent, List<GameEventListener>> connections)
        {
            var sceneGroups = new Dictionary<string, List<KeyValuePair<GameEvent, List<GameEventListener>>>>();
            
            foreach (var kvp in connections)
            {
                var scenes = kvp.Value.Select(l => l.gameObject.scene.name).Distinct();
                foreach (var sceneName in scenes)
                {
                    if (!sceneGroups.ContainsKey(sceneName))
                    {
                        sceneGroups[sceneName] = new List<KeyValuePair<GameEvent, List<GameEventListener>>>();
                    }
                    
                    var sceneListeners = kvp.Value.Where(l => l.gameObject.scene.name == sceneName).ToList();
                    if (sceneListeners.Count > 0)
                    {
                        sceneGroups[sceneName].Add(new KeyValuePair<GameEvent, List<GameEventListener>>(kvp.Key, sceneListeners));
                    }
                }
            }
            
            foreach (var sceneGroup in sceneGroups)
            {
                EditorGUILayout.LabelField($"🎬 Scene: {sceneGroup.Key}", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                
                foreach (var connection in sceneGroup.Value)
                {
                    DrawEventGroup(connection.Key, connection.Value);
                }
                
                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
            }
        }
        
        private void DrawEventGroup(GameEvent gameEvent, List<GameEventListener> listeners)
        {
            EditorGUILayout.BeginVertical("box");
            
            // イベント名とボタン
            EditorGUILayout.BeginHorizontal();
            
            var eventLabel = gameEvent != null ? gameEvent.name : "<Missing Event>";
            EditorGUILayout.LabelField($"📡 {eventLabel}", EditorStyles.boldLabel);
            
            if (gameEvent != null)
            {
                EditorGUILayout.ObjectField(gameEvent, typeof(GameEvent), false, GUILayout.Width(150));
                
                if (GUILayout.Button("🔥", GUILayout.Width(30)))
                {
                    gameEvent.Raise();
                }
                
                if (GUILayout.Button("🎯", GUILayout.Width(30)))
                {
                    Selection.objects = listeners.Select(l => l.gameObject).ToArray();
                }
            }
            
            EditorGUILayout.EndHorizontal();
            
            // リスナー一覧
            if (listeners.Count > 0)
            {
                EditorGUI.indentLevel++;
                foreach (var listener in listeners)
                {
                    DrawListenerEntry(listener);
                }
                EditorGUI.indentLevel--;
            }
            else
            {
                EditorGUILayout.LabelField("  No listeners", EditorStyles.centeredGreyMiniLabel);
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawListenerEntry(GameEventListener listener)
        {
            if (listener == null)
            {
                EditorGUILayout.LabelField("  ❌ <Missing Listener>", new GUIStyle(EditorStyles.label) { normal = { textColor = Color.red } });
                return;
            }
            
            EditorGUILayout.BeginHorizontal();
            
            var listenerColor = listener.enabled ? Color.white : Color.gray;
            var style = new GUIStyle(EditorStyles.label) { normal = { textColor = listenerColor } };
            var icon = listener.enabled ? "✅" : "⏸️";
            
            EditorGUILayout.LabelField($"  {icon} {listener.gameObject.name}", style);
            EditorGUILayout.LabelField($"Priority: {listener.Priority}", GUILayout.Width(80));
            
            if (GUILayout.Button("📍", GUILayout.Width(30)))
            {
                EditorGUIUtility.PingObject(listener);
                Selection.activeObject = listener.gameObject;
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        private Dictionary<GameEvent, List<GameEventListener>> GetFilteredConnections()
        {
            if (eventConnections == null) return new Dictionary<GameEvent, List<GameEventListener>>();
            
            var filtered = new Dictionary<GameEvent, List<GameEventListener>>();
            
            foreach (var kvp in eventConnections)
            {
                if (!string.IsNullOrEmpty(searchFilter))
                {
                    var eventName = kvp.Key != null ? kvp.Key.name : "";
                    if (!eventName.ToLower().Contains(searchFilter.ToLower()))
                    {
                        continue;
                    }
                }
                
                var filteredListeners = kvp.Value;
                
                if (showOnlyActiveListeners)
                {
                    filteredListeners = filteredListeners.Where(l => l != null && l.enabled).ToList();
                }
                
                filtered[kvp.Key] = filteredListeners;
            }
            
            return filtered;
        }
        
        void RefreshConnections()
        {
            eventConnections = new Dictionary<GameEvent, List<GameEventListener>>();
            
            var allListeners = FindObjectsByType<GameEventListener>(FindObjectsSortMode.None);
            foreach (var listener in allListeners)
            {
                if (listener.Event != null)
                {
                    if (!eventConnections.ContainsKey(listener.Event))
                    {
                        eventConnections[listener.Event] = new List<GameEventListener>();
                    }
                    eventConnections[listener.Event].Add(listener);
                }
            }
            
            // 使用されていないGameEventも表示
            var allGameEvents = Resources.FindObjectsOfTypeAll<GameEvent>();
            foreach (var gameEvent in allGameEvents)
            {
                if (!eventConnections.ContainsKey(gameEvent))
                {
                    eventConnections[gameEvent] = new List<GameEventListener>();
                }
            }
        }
        
        private void ExportToConsole()
        {
            UnityEngine.Debug.Log("=== EVENT FLOW ANALYSIS ===");
            foreach (var kvp in eventConnections)
            {
                var eventName = kvp.Key != null ? kvp.Key.name : "<Missing Event>";
                UnityEngine.Debug.Log($"📡 {eventName} → {kvp.Value.Count} listeners");
                foreach (var listener in kvp.Value)
                {
                    if (listener != null)
                    {
                        UnityEngine.Debug.Log($"  └─ {listener.gameObject.name} (Priority: {listener.Priority}) [{listener.gameObject.scene.name}]", listener);
                    }
                    else
                    {
                        UnityEngine.Debug.Log($"  └─ <Missing Listener>");
                    }
                }
            }
        }
        
        private void FindPotentialIssues()
        {
            UnityEngine.Debug.Log("=== POTENTIAL ISSUES ANALYSIS ===");

            var orphanedListeners = FindObjectsByType<GameEventListener>(FindObjectsSortMode.None).Where(l => l.Event == null).ToArray();
            if (orphanedListeners.Length > 0)
            {
                UnityEngine.Debug.LogWarning($"Found {orphanedListeners.Length} orphaned GameEventListeners:");
                foreach (var listener in orphanedListeners)
                {
                    UnityEngine.Debug.LogWarning($"  - {listener.gameObject.name} [{listener.gameObject.scene.name}]", listener);
                }
            }

            var unusedEvents = eventConnections.Where(kvp => kvp.Value.Count == 0).ToArray();
            if (unusedEvents.Length > 0)
            {
                UnityEngine.Debug.LogWarning($"Found {unusedEvents.Length} unused GameEvents:");
                foreach (var kvp in unusedEvents)
                {
                    if (kvp.Key != null)
                    {
                        UnityEngine.Debug.LogWarning($"  - {kvp.Key.name}", kvp.Key);
                    }
                }
            }

            var duplicatePriorities = new Dictionary<GameEvent, List<int>>();
            foreach (var kvp in eventConnections)
            {
                var priorities = kvp.Value.Where(l => l != null).Select(l => l.Priority).ToList();
                var duplicates = priorities.GroupBy(p => p).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
                if (duplicates.Count > 0)
                {
                    duplicatePriorities[kvp.Key] = duplicates;
                }
            }

            if (duplicatePriorities.Count > 0)
            {
                UnityEngine.Debug.LogWarning("Found events with duplicate listener priorities:");
                foreach (var kvp in duplicatePriorities)
                {
                    UnityEngine.Debug.LogWarning($"  - {kvp.Key.name}: priorities {string.Join(", ", kvp.Value)}", kvp.Key);
                }
            }

            if (orphanedListeners.Length == 0 && unusedEvents.Length == 0 && duplicatePriorities.Count == 0)
            {
                UnityEngine.Debug.Log("✅ No issues found in event system!");
            }
        }
    }
}