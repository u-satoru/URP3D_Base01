using UnityEngine;
using UnityEditor;
using asterivo.Unity60.Features.AI.Visual;
using asterivo.Unity60.Core.Data;
using System.Text;

namespace asterivo.Unity60.Features.AI.Visual.Editor
{
    /// <summary>
    /// NPCVisualSensorのカスタムエディター
    /// 各モジュールの状態をリアルタイムで表示し、デバッグ情報を提供
    /// </summary>
    [CustomEditor(typeof(NPCVisualSensor))]
    public class NPCVisualSensorEditor : UnityEditor.Editor
    {
        private NPCVisualSensor sensor;
        private bool showRuntimeDebug = true;
        private bool showModuleStats = true;
        private bool showTargetDetails = false;
        private Vector2 scrollPosition;
        
        // GUI Styles
        private GUIStyle headerStyle;
        private GUIStyle boxStyle;
        private GUIStyle statusStyle;
        private bool stylesInitialized = false;
        
        private void OnEnable()
        {
            sensor = (NPCVisualSensor)target;
        }
        
        private void InitializeStyles()
        {
            if (stylesInitialized) return;
            
            headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 12,
                normal = { textColor = Color.white }
            };
            
            boxStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(10, 10, 5, 5)
            };
            
            statusStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                richText = true
            };
            
            stylesInitialized = true;
        }
        
        public override void OnInspectorGUI()
        {
            InitializeStyles();
            
            serializedObject.Update();
            
            // Header
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("NPCVisualSensor System", headerStyle);
            EditorGUILayout.Space();
            
            // Default Inspector（Odin Inspectorが有効な場合はそれが優先される）
            DrawDefaultInspector();
            
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Runtime debug information is only available during play mode.", MessageType.Info);
                serializedObject.ApplyModifiedProperties();
                return;
            }
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Runtime Debug Information", headerStyle);
            
            // Runtime Debug Controls
            EditorGUILayout.BeginHorizontal();
            showRuntimeDebug = EditorGUILayout.Toggle("Show Runtime Debug", showRuntimeDebug);
            showModuleStats = EditorGUILayout.Toggle("Show Module Stats", showModuleStats);
            EditorGUILayout.EndHorizontal();
            
            showTargetDetails = EditorGUILayout.Toggle("Show Target Details", showTargetDetails);
            
            if (showRuntimeDebug)
            {
                DrawRuntimeDebugInfo();
            }
            
            if (showModuleStats)
            {
                DrawModuleStatistics();
            }
            
            if (showTargetDetails)
            {
                DrawTargetDetails();
            }
            
            // Refresh button
            EditorGUILayout.Space();
            if (GUILayout.Button("Refresh Debug Info"))
            {
                Repaint();
            }
            
            serializedObject.ApplyModifiedProperties();
            
            // Auto refresh during play mode
            if (Application.isPlaying)
            {
                EditorUtility.SetDirty(target);
                Repaint();
            }
        }
        
        private void DrawRuntimeDebugInfo()
        {
            EditorGUILayout.BeginVertical(boxStyle);
            EditorGUILayout.LabelField("System Status", headerStyle);
            
            if (sensor != null)
            {
                // System State
                string systemStatus = GetSystemStatusText();
                EditorGUILayout.LabelField(systemStatus, statusStyle);
                
                EditorGUILayout.Space(5);
                
                // Performance Info
                EditorGUILayout.LabelField("Performance Metrics", EditorStyles.boldLabel);
                DrawPerformanceMetrics();
            }
            else
            {
                EditorGUILayout.LabelField("Sensor reference is null", statusStyle);
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawModuleStatistics()
        {
            EditorGUILayout.BeginVertical(boxStyle);
            EditorGUILayout.LabelField("Module Statistics", headerStyle);
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));
            
            if (sensor != null)
            {
                // Detection Module Stats
                DrawDetectionModuleStats();
                
                EditorGUILayout.Space(10);
                
                // Alert System Stats
                DrawAlertSystemStats();
                
                EditorGUILayout.Space(10);
                
                // Memory System Stats
                DrawMemorySystemStats();
                
                EditorGUILayout.Space(10);
                
                // Target Tracking Stats
                DrawTargetTrackingStats();
                
                EditorGUILayout.Space(10);
                
                // Event Manager Stats
                DrawEventManagerStats();
            }
            
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }
        
        private void DrawTargetDetails()
        {
            EditorGUILayout.BeginVertical(boxStyle);
            EditorGUILayout.LabelField("Target Details", headerStyle);
            
            if (sensor != null)
            {
                // ここでは仮のターゲット情報を表示
                EditorGUILayout.LabelField("Active Targets: N/A", statusStyle);
                EditorGUILayout.LabelField("Primary Target: N/A", statusStyle);
                
                // 実際の実装では、TargetTrackingModuleからターゲット情報を取得
                // var targets = sensor.GetTrackedTargets();
                // foreach (var target in targets) { ... }
                
                EditorGUILayout.HelpBox("Target details will be implemented when modules are integrated.", MessageType.Info);
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private string GetSystemStatusText()
        {
            StringBuilder sb = new StringBuilder();
            
            if (sensor != null)
            {
                sb.AppendLine($"<color=green>Status:</color> {(sensor.enabled ? "Active" : "Inactive")}");
                sb.AppendLine($"<color=yellow>Update Mode:</color> {(Application.isPlaying ? "Runtime" : "Editor")}");
                sb.AppendLine($"<color=cyan>Component:</color> {sensor.GetType().Name}");
                
                // GameObject情報
                if (sensor.gameObject != null)
                {
                    sb.AppendLine($"<color=magenta>GameObject:</color> {sensor.gameObject.name}");
                    sb.AppendLine($"<color=gray>Position:</color> {sensor.transform.position}");
                }
            }
            
            return sb.ToString();
        }
        
        private void DrawPerformanceMetrics()
        {
            if (sensor == null) return;
            
            // パフォーマンス統計の取得
            var perfStats = sensor.GetPerformanceStats();
            
            // パフォーマンス関連の表示
            EditorGUILayout.LabelField($"Scan Frequency: {perfStats.currentScanFrequency:F1} Hz", statusStyle);
            EditorGUILayout.LabelField($"Active Targets: {perfStats.activeTargets}", statusStyle);
            EditorGUILayout.LabelField($"Potential Targets: {perfStats.potentialTargets}", statusStyle);
            EditorGUILayout.LabelField($"Pooled Objects: {perfStats.pooledObjects}", statusStyle);
            EditorGUILayout.LabelField($"Culled This Frame: {perfStats.culledTargetsThisFrame}", statusStyle);
            
            // 最適化状態の表示
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Optimization Status:", EditorStyles.boldLabel);
            DrawOptimizationStatus("LOD Optimization", perfStats.lodOptimizationEnabled);
            DrawOptimizationStatus("Early Culling", perfStats.earlyCullingEnabled);
            DrawOptimizationStatus("Memory Pool", perfStats.memoryPoolEnabled);
            
            // パフォーマンスバーの表示
            float cpuUsage = Mathf.Clamp01(perfStats.currentScanFrequency / 30f);
            EditorGUI.ProgressBar(GUILayoutUtility.GetRect(18, 18), cpuUsage, $"CPU Usage: {cpuUsage * 100f:F1}%");
        }
        
        private void DrawDetectionModuleStats()
        {
            EditorGUILayout.LabelField("Detection Module", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            
            if (sensor != null)
            {
                var detectedTargets = sensor.GetDetectedTargets();
                float highestScore = 0f;
                float averageScore = 0f;
                
                if (detectedTargets.Count > 0)
                {
                    foreach (var target in detectedTargets)
                    {
                        if (target.detectionScore > highestScore)
                            highestScore = target.detectionScore;
                        averageScore += target.detectionScore;
                    }
                    averageScore /= detectedTargets.Count;
                }
                
                EditorGUILayout.LabelField($"Active Detections: {detectedTargets.Count}", statusStyle);
                EditorGUILayout.LabelField($"Highest Score: {highestScore:F3}", statusStyle);
                EditorGUILayout.LabelField($"Average Score: {averageScore:F3}", statusStyle);
                
                // Detection scoreのプログレスバー
                EditorGUI.ProgressBar(GUILayoutUtility.GetRect(18, 18), highestScore, $"Peak Detection: {highestScore:F2}");
            }
            else
            {
                EditorGUILayout.LabelField("Sensor reference is null", statusStyle);
            }
            
            EditorGUI.indentLevel--;
        }
        
        private void DrawAlertSystemStats()
        {
            EditorGUILayout.LabelField("Alert System", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            
            if (sensor != null)
            {
                var alertModule = sensor.GetAlertSystemModule();
                var currentLevel = sensor.GetCurrentAlertLevel();
                
                EditorGUILayout.LabelField($"Current Alert Level: {currentLevel}", statusStyle);
                EditorGUILayout.LabelField($"Alert Intensity: {alertModule.AlertIntensity:F3}", statusStyle);
                EditorGUILayout.LabelField($"Alert Timer: {alertModule.AlertTimer:F1}s", statusStyle);
                EditorGUILayout.LabelField($"Decay Rate: {alertModule.AlertDecayRate:F3}/s", statusStyle);
                
                // Alert levelのカラー表示
                Color alertColor = GetAlertLevelColor(currentLevel);
                var oldColor = GUI.backgroundColor;
                GUI.backgroundColor = alertColor;
                EditorGUI.ProgressBar(GUILayoutUtility.GetRect(18, 18), 
                    alertModule.AlertIntensity, $"Alert: {currentLevel} ({alertModule.AlertIntensity:F2})");
                GUI.backgroundColor = oldColor;
            }
            else
            {
                EditorGUILayout.LabelField("Alert module not available", statusStyle);
            }
            
            EditorGUI.indentLevel--;
        }
        
        private void DrawMemorySystemStats()
        {
            EditorGUILayout.LabelField("Memory System", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            
            if (sensor != null)
            {
                var memoryModule = sensor.GetMemoryModule();
                
                EditorGUILayout.LabelField($"Short Term: {memoryModule.ShortTermMemoryCount}", statusStyle);
                EditorGUILayout.LabelField($"Long Term: {memoryModule.LongTermMemoryCount}", statusStyle);
                EditorGUILayout.LabelField($"Total Memories: {memoryModule.TotalMemoryCount}", statusStyle);
                EditorGUILayout.LabelField($"Has Memories: {memoryModule.HasMemories}", statusStyle);
                
                // メモリ使用量のプログレスバー（最大20と仮定）
                float memoryUsage = memoryModule.TotalMemoryCount / 20f;
                EditorGUI.ProgressBar(GUILayoutUtility.GetRect(18, 18), memoryUsage, 
                    $"Memory Usage: {memoryModule.TotalMemoryCount}/20 ({memoryUsage * 100f:F0}%)");
            }
            else
            {
                EditorGUILayout.LabelField("Memory module not available", statusStyle);
            }
            
            EditorGUI.indentLevel--;
        }
        
        private void DrawTargetTrackingStats()
        {
            EditorGUILayout.LabelField("Target Tracking", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            
            if (sensor != null)
            {
                var trackingModule = sensor.GetTargetTrackingModule();
                var primaryTarget = sensor.GetPrimaryTarget();
                
                EditorGUILayout.LabelField($"Active Targets: {sensor.GetActiveTargetCount()}", statusStyle);
                EditorGUILayout.LabelField($"Primary Target: {primaryTarget?.transform?.name ?? "None"}", statusStyle);
                EditorGUILayout.LabelField($"Can Track More: {sensor.CanTrackMoreTargets()}", statusStyle);
                EditorGUILayout.LabelField($"Max Capacity: {trackingModule.MaxTargets}", statusStyle);
                
                // 追跡容量のプログレスバー
                float capacity = (float)sensor.GetActiveTargetCount() / trackingModule.MaxTargets;
                EditorGUI.ProgressBar(GUILayoutUtility.GetRect(18, 18), capacity, 
                    $"Capacity: {sensor.GetActiveTargetCount()}/{trackingModule.MaxTargets} ({capacity * 100f:F0}%)");
                
                // 追跡中の目標の詳細表示
                if (showTargetDetails)
                {
                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField("Tracked Targets:", EditorStyles.miniLabel);
                    var trackedTargets = sensor.GetTrackedTargets();
                    EditorGUI.indentLevel++;
                    foreach (var target in trackedTargets)
                    {
                        if (target?.transform != null)
                        {
                            EditorGUILayout.LabelField($"• {target.transform.name} (Score: {target.detectionScore:F2})", EditorStyles.miniLabel);
                        }
                    }
                    EditorGUI.indentLevel--;
                }
            }
            else
            {
                EditorGUILayout.LabelField("Tracking module not available", statusStyle);
            }
            
            EditorGUI.indentLevel--;
        }
        
        // Scene View用のGizmos描画
        private void OnSceneGUI()
        {
            if (sensor == null) return;
            
            Vector3 eyePos = sensor.EyePosition.position;
            
            // 検出範囲の可視化　-　異なる警戒レベルに応じた色分け
            var currentLevel = sensor.GetCurrentAlertLevel();
            Handles.color = GetAlertLevelColor(currentLevel);
            
            // 警戒レベルに応じた範囲表示
            float sightRange = GetSightRange();
            Handles.DrawWireDisc(eyePos, Vector3.up, sightRange);
            
            // 視野角の可視化
            float fieldOfView = GetFieldOfView();
            Vector3 forward = sensor.EyePosition.forward;
            
            Vector3 leftBoundary = Quaternion.AngleAxis(-fieldOfView / 2, Vector3.up) * forward * sightRange;
            Vector3 rightBoundary = Quaternion.AngleAxis(fieldOfView / 2, Vector3.up) * forward * sightRange;
            
            // 視野角の境界線
            Handles.color = Color.white;
            Handles.DrawLine(eyePos, eyePos + leftBoundary);
            Handles.DrawLine(eyePos, eyePos + rightBoundary);
            
            // 視野角の円弧
            Handles.color = new Color(1f, 1f, 1f, 0.3f);
            Handles.DrawWireArc(eyePos, Vector3.up, leftBoundary, fieldOfView, sightRange);
            
            // 検出された目標の表示
            DrawDetectedTargetsInScene();
            
            // 追跡中の目標の表示
            DrawTrackedTargetsInScene();
            
            // メモリの予測位置表示
            DrawMemoryPredictionsInScene();
        }
        
        private void DrawEventManagerStats()
        {
            EditorGUILayout.LabelField("Event Manager", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            
            if (sensor != null)
            {
                var eventStats = sensor.GetEventManagerStats();
                
                EditorGUILayout.LabelField($"Events Buffered: {eventStats.eventsBuffered}", statusStyle);
                EditorGUILayout.LabelField($"Events Sent: {eventStats.eventsSentThisFrame}", statusStyle);
                EditorGUILayout.LabelField($"Events Suppressed: {eventStats.eventsSuppressed}", statusStyle);
                EditorGUILayout.LabelField($"Buffer Enabled: {eventStats.bufferEnabled}", statusStyle);
                EditorGUILayout.LabelField($"Cooldown Time: {eventStats.cooldownTime:F3}s", statusStyle);
                
                // イベント効率のプログレスバー
                float efficiency = eventStats.eventsSuppressed > 0 ? 
                    (float)eventStats.eventsSentThisFrame / (eventStats.eventsSentThisFrame + eventStats.eventsSuppressed) : 1f;
                Color oldColor = GUI.backgroundColor;
                GUI.backgroundColor = efficiency > 0.8f ? Color.green : (efficiency > 0.5f ? Color.yellow : Color.red);
                EditorGUI.ProgressBar(GUILayoutUtility.GetRect(18, 18), efficiency, 
                    $"Event Efficiency: {efficiency * 100f:F0}%");
                GUI.backgroundColor = oldColor;
            }
            else
            {
                EditorGUILayout.LabelField("Event manager not available", statusStyle);
            }
            
            EditorGUI.indentLevel--;
        }
        
        #region Helper Methods
        
        /// <summary>
        /// 最適化状態を表示するヘルパーメソッド
        /// </summary>
        private void DrawOptimizationStatus(string label, bool enabled)
        {
            Color oldColor = GUI.color;
            GUI.color = enabled ? Color.green : Color.gray;
            EditorGUILayout.LabelField($"• {label}: {(enabled ? "Enabled" : "Disabled")}", EditorStyles.miniLabel);
            GUI.color = oldColor;
        }
        
        /// <summary>
        /// 警戒レベルに応じた色を取得
        /// </summary>
        private Color GetAlertLevelColor(AlertLevel level)
        {
            return level switch
            {
                AlertLevel.Relaxed => Color.green,
                AlertLevel.Suspicious => Color.yellow,
                AlertLevel.Investigating => new Color(1f, 0.5f, 0f), // Orange
                AlertLevel.Alert => Color.red,
                _ => Color.white
            };
        }
        
        /// <summary>
        /// 検出範囲を取得
        /// </summary>
        private float GetSightRange()
        {
            if (sensor == null) return 15f;
            // reflectionでprivateフィールドにアクセスまたはデフォルト値を使用
            return 15f;
        }
        
        /// <summary>
        /// 視野角を取得
        /// </summary>
        private float GetFieldOfView()
        {
            if (sensor == null) return 110f;
            // reflectionでprivateフィールドにアクセスまたはデフォルト値を使用
            return 110f;
        }
        
        /// <summary>
        /// Scene Viewで検出された目標を描画
        /// </summary>
        private void DrawDetectedTargetsInScene()
        {
            if (sensor == null) return;
            
            var detectedTargets = sensor.GetDetectedTargets();
            foreach (var target in detectedTargets)
            {
                if (target?.transform != null)
                {
                    // 検出スコアに応じた色
                    Handles.color = Color.Lerp(Color.green, Color.red, target.detectionScore);
                    
                    // 目標へのライン
                    Handles.DrawLine(sensor.EyePosition.position, target.transform.position);
                    
                    // 目標のマーカー
                    Handles.DrawWireCube(target.transform.position, Vector3.one * (0.5f + target.detectionScore));
                    
                    // ラベル表示
                    Handles.Label(target.transform.position + Vector3.up, 
                        $"{target.transform.name}\nScore: {target.detectionScore:F2}");
                }
            }
        }
        
        /// <summary>
        /// Scene Viewで追跡中の目標を描画
        /// </summary>
        private void DrawTrackedTargetsInScene()
        {
            if (sensor == null) return;
            
            var trackedTargets = sensor.GetTrackedTargets();
            var primaryTarget = sensor.GetPrimaryTarget();
            
            foreach (var target in trackedTargets)
            {
                if (target?.transform != null)
                {
                    // プライマリターゲットは特別な色
                    bool isPrimary = primaryTarget == target;
                    Handles.color = isPrimary ? Color.cyan : Color.blue;
                    
                    // 追跡マーカー - Fixed
                    float radius = isPrimary ? 1.5f : 1f;
                    Handles.DrawWireDisc(target.transform.position, Vector3.up, radius);
                    Handles.DrawWireDisc(target.transform.position, Vector3.forward, radius);
                    Handles.DrawWireDisc(target.transform.position, Vector3.right, radius);
                    
                    if (isPrimary)
                    {
                        Handles.Label(target.transform.position + Vector3.up * 2f, "PRIMARY TARGET");
                    }
                }
            }
        }
        
        /// <summary>
        /// Scene Viewでメモリの予測位置を描画
        /// </summary>
        private void DrawMemoryPredictionsInScene()
        {
            if (sensor == null) return;
            
            var trackedTargets = sensor.GetTrackedTargets();
            
            foreach (var target in trackedTargets)
            {
                if (target?.transform != null)
                {
                    // 予測位置を取得
                    Vector3 predictedPos = sensor.GetPredictedPosition(target.transform);
                    Vector3 currentPos = target.transform.position;
                    
                    if (Vector3.Distance(predictedPos, currentPos) > 0.1f)
                    {
                        Handles.color = Color.magenta;
                        
                        // 現在位置から予測位置への矢印
                        Handles.ArrowHandleCap(0, currentPos, 
                            Quaternion.LookRotation(predictedPos - currentPos), 
                            Vector3.Distance(predictedPos, currentPos), EventType.Repaint);
                        
                        // 予測位置マーカー
                        Handles.color = new Color(1f, 0f, 1f, 0.5f);
                        Handles.DrawWireCube(predictedPos, Vector3.one * 0.8f);
                        
                        Handles.Label(predictedPos, "PREDICTED");
                    }
                }
            }
        }
        
        #endregion
    }
    
    /// <summary>
    /// VisualSensorSettingsのカスタムエディター
    /// </summary>
    [CustomEditor(typeof(VisualSensorSettings))]
    public class VisualSensorSettingsEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Visual Sensor System Settings", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("このScriptableObjectはNPCVisualSensorシステム全体の設定を管理します。", MessageType.Info);
            
            DrawDefaultInspector();
            
            EditorGUILayout.Space();
            
            // 設定の妥当性チェック
            VisualSensorSettings settings = (VisualSensorSettings)target;
            if (settings != null)
            {
                ValidateSettings(settings);
            }
        }
        
        private void ValidateSettings(VisualSensorSettings settings)
        {
            EditorGUILayout.LabelField("Settings Validation", EditorStyles.boldLabel);
            
            // パフォーマンス警告
            if (settings.UpdateFrequency > 30f)
            {
                EditorGUILayout.HelpBox("High update frequency may impact performance.", MessageType.Warning);
            }
            
            if (settings.MaxScanTargets > 30)
            {
                EditorGUILayout.HelpBox("Large number of scan targets may cause frame drops.", MessageType.Warning);
            }
            
            // 設定の整合性チェック
            if (settings.SuspiciousThreshold >= settings.InvestigatingThreshold)
            {
                EditorGUILayout.HelpBox("Suspicious threshold should be lower than investigating threshold.", MessageType.Error);
            }
            
            if (settings.InvestigatingThreshold >= settings.AlertThreshold)
            {
                EditorGUILayout.HelpBox("Investigating threshold should be lower than alert threshold.", MessageType.Error);
            }
            
            // 推奨設定の表示
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Recommended Values", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Update Frequency: 20 Hz", EditorStyles.miniLabel);
            EditorGUILayout.LabelField("Max Scan Targets: 20", EditorStyles.miniLabel);
            EditorGUILayout.LabelField("Max Tracked Targets: 5", EditorStyles.miniLabel);
        }
    }
}
