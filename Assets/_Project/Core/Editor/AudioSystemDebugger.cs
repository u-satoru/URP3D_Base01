using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using asterivo.Unity60.Core.Audio;
using asterivo.Unity60.Core.Audio.Data;
// using asterivo.Unity60.Core.Shared;
// using asterivo.Unity60.Core.Debug;
using System.Reflection;
using AudioCategory = asterivo.Unity60.Core.Audio.AudioCategory;

namespace asterivo.Unity60.Core.Editor
{
    /// <summary>
    /// オーチE��オシスチE��専用チE��チE��ー・モニタリングチE�Eル
    /// 統合オーチE��オシスチE��の状態監視、デバッグ、テスト機�Eを提侁E    /// 
    /// 主な機�E�E�E    /// - リアルタイムオーチE��オシスチE��状態監要E    /// - ボリュームレベルの動的制御とチE��チE    /// - スチE��スモード�EチE��チE��とチE��ト環墁E��御
    /// - パフォーマンス監視とグラフ表示
    /// - オーチE��オイベント�Eリアルタイム追跡
    /// 
    /// 使用シーン�E�E    /// - オーチE��オシスチE��の動作確誁E    /// - ゲーム中のオーチE��オバランス調整
    /// - スチE��ス機�EのチE��トとチE��チE��
    /// - パフォーマンス問題�E特宁E    /// 
    /// アクセス方法：Unity メニュー > asterivo.Unity60/Audio/Audio System Debugger
    /// 注意：�Eレイモード中のみ利用可能
    /// </summary>
    public class AudioSystemDebugger : EditorWindow
    {
        #region Window Management
        
        /// <summary>
        /// オーチE��オシスチE��チE��チE��ーウィンドウを表示する
        /// Unity メニューから呼び出されるエチE��タ拡張メニューアイチE��
        /// </summary>
        /// <remarks>
        /// ウィンドウの最小サイズは800x600に設定され、E        /// リアルタイムでオーチE��オシスチE��の状態を監視できます、E        /// </remarks>
        [MenuItem("asterivo.Unity60/Audio/Audio System Debugger")]
        public static void ShowWindow()
        {
            var window = GetWindow<AudioSystemDebugger>("Audio System Debugger");
            window.minSize = new Vector2(800, 600);
            window.Show();
        }
        
        #endregion
        
        #region Fields and Properties
        
        private Vector2 scrollPosition;
        private int selectedTab = 0;
        private readonly string[] tabNames = { "System Monitor", "Volume Control", "Stealth Debug", "Performance", "Audio Events" };
        
        // System Monitoring
        private bool autoRefresh = true;
        private float refreshInterval = 0.5f;
        private double lastRefreshTime;
        
        // Audio System References
        private AudioManager audioManager;
        private StealthAudioCoordinator stealthCoordinator;
        private SpatialAudioManager spatialAudioManager;
        private DynamicAudioEnvironment dynamicEnvironment;
        
        // Debug Parameters
        private float testTensionLevel = 0.5f;
        private GameState testGameState = GameState.Gameplay;
        private EnvironmentType testEnvironment = EnvironmentType.Outdoor;
        private WeatherType testWeather = WeatherType.Clear;
        private TimeOfDay testTime = TimeOfDay.Day;
        
        // Monitoring Data
        private AudioSystemState currentAudioState;
        private List<string> recentEvents = new List<string>();
        private Dictionary<AudioCategory, float> categoryVolumes = new Dictionary<AudioCategory, float>();
        private float currentMaskingLevel;
        private bool isStealthModeActive;
        
        // Performance Monitoring
        private List<float> volumeHistory = new List<float>();
        private List<float> tensionHistory = new List<float>();
        private const int MAX_HISTORY_POINTS = 100;
        
        #endregion
        
        #region Unity Editor Callbacks
        
        /// <summary>
        /// ウィンドウが有効になった時の初期化�E琁E        /// エチE��タの更新イベントに登録し、オーチE��オシスチE��の参�Eを取得すめE        /// </summary>
        private void OnEnable()
        {
            EditorApplication.update += OnEditorUpdate;
            FindAudioSystemReferences();
            InitializeMonitoring();
        }
        
        /// <summary>
        /// ウィンドウが無効になった時のクリーンアチE�E処琁E        /// エチE��タの更新イベントから登録解除を行う
        /// </summary>
        private void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
        }
        
        /// <summary>
        /// エチE��タの更新ループでオーチE��オシスチE��チE�Eタを定期皁E��更新
        /// 自動リフレチE��ュが有効な場合、指定された間隔でチE�Eタを更新してウィンドウを�E描画する
        /// </summary>
        /// <remarks>
        /// プレイモード中のみ実行され、refreshInterval�E�デフォルチE.5秒）ごとに更新されめE        /// </remarks>
        private void OnEditorUpdate()
        {
            if (!Application.isPlaying) return;
            
            if (autoRefresh && EditorApplication.timeSinceStartup - lastRefreshTime >= refreshInterval)
            {
                RefreshAudioSystemData();
                lastRefreshTime = EditorApplication.timeSinceStartup;
                Repaint();
            }
        }
        
        /// <summary>
        /// ウィンドウのGUI描画処琁E        /// プレイモード中のみ利用可能で、タブ�Eースのインターフェースを提侁E        /// </summary>
        /// <remarks>
        /// 以下�E5つのタブで構�EされてぁE��す！E        /// 0: System Monitor - シスチE��状態表示と基本操佁E        /// 1: Volume Control - 吁E��チE��リのボリューム制御
        /// 2: Stealth Debug - スチE��スシスチE��のチE��チE��
        /// 3: Performance - パフォーマンス監視とグラフ表示
        /// 4: Audio Events - オーチE��オイベント�E履歴表示
        /// </remarks>
        private void OnGUI()
        {
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Audio System Debugger is only available during Play Mode.", MessageType.Info);
                return;
            }
            
            DrawHeader();
            DrawTabBar();
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            switch (selectedTab)
            {
                case 0: DrawSystemMonitorTab(); break;
                case 1: DrawVolumeControlTab(); break;
                case 2: DrawStealthDebugTab(); break;
                case 3: DrawPerformanceTab(); break;
                case 4: DrawAudioEventsTab(); break;
            }
            
            EditorGUILayout.EndScrollView();
        }
        
        #endregion
        
        #region GUI Drawing Methods
        
        /// <summary>
        /// ウィンドウヘッダー部刁E�EGUI描画
        /// タイトル表示、�E動リフレチE��ュ刁E��替え、手動リフレチE��ュボタンを含む
        /// </summary>
        private void DrawHeader()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            
            GUILayout.Label("Audio System Debugger", EditorStyles.boldLabel);
            
            GUILayout.FlexibleSpace();
            
            autoRefresh = GUILayout.Toggle(autoRefresh, "Auto Refresh", EditorStyles.toolbarButton);
            
            if (GUILayout.Button("Refresh Now", EditorStyles.toolbarButton))
            {
                RefreshAudioSystemData();
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        /// <summary>
        /// タブバーのGUI描画
        /// 5つのタブ！Eystem Monitor, Volume Control, Stealth Debug, Performance, Audio Events�E�を表示
        /// </summary>
        private void DrawTabBar()
        {
            selectedTab = GUILayout.Toolbar(selectedTab, tabNames);
        }
        
        private void DrawSystemMonitorTab()
        {
            EditorGUILayout.LabelField("System Monitor", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            if (audioManager != null)
            {
                DrawSystemStatus();
                EditorGUILayout.Space();
                DrawQuickActions();
            }
            else
            {
                EditorGUILayout.HelpBox("AudioManager not found in scene.", MessageType.Warning);
            }
        }
        
        private void DrawSystemStatus()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("System Status", EditorStyles.boldLabel);
            
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.EnumPopup("Game State", currentAudioState.gameState);
            EditorGUILayout.Slider("Tension Level", currentAudioState.tensionLevel, 0f, 1f);
            EditorGUILayout.Toggle("Stealth Mode Active", currentAudioState.isStealthModeActive);
            EditorGUI.EndDisabledGroup();
            
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Volume Levels", EditorStyles.boldLabel);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.Slider("Master", currentAudioState.masterVolume, 0f, 1f);
            EditorGUILayout.Slider("BGM", currentAudioState.bgmVolume, 0f, 1f);
            EditorGUILayout.Slider("Ambient", currentAudioState.ambientVolume, 0f, 1f);
            EditorGUILayout.Slider("Effect", currentAudioState.effectVolume, 0f, 1f);
            EditorGUILayout.Slider("Stealth", currentAudioState.stealthAudioVolume, 0f, 1f);
            EditorGUI.EndDisabledGroup();
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawQuickActions()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Pause All Audio"))
            {
                audioManager?.PauseAllAudio();
                AddEvent("All audio paused via debugger");
            }
            
            if (GUILayout.Button("Resume All Audio"))
            {
                audioManager?.ResumeAllAudio();
                AddEvent("All audio resumed via debugger");
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            testTensionLevel = EditorGUILayout.Slider("Test Tension", testTensionLevel, 0f, 1f);
            if (GUILayout.Button("Apply", GUILayout.Width(60)))
            {
                audioManager?.UpdateTensionLevel(testTensionLevel);
                AddEvent($"Tension level set to {testTensionLevel:F2}");
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            testGameState = (GameState)EditorGUILayout.EnumPopup("Test Game State", testGameState);
            if (GUILayout.Button("Apply", GUILayout.Width(60)))
            {
                audioManager?.UpdateAudioForGameState(testGameState, testTensionLevel);
                AddEvent($"Game state changed to {testGameState}");
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawVolumeControlTab()
        {
            EditorGUILayout.LabelField("Volume Control", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            if (audioManager != null)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("Master Volume Controls", EditorStyles.boldLabel);
                
                float newMaster = EditorGUILayout.Slider("Master Volume", currentAudioState.masterVolume, 0f, 1f);
                if (newMaster != currentAudioState.masterVolume)
                {
                    audioManager.SetMasterVolume(newMaster);
                }
                
                float newBGM = EditorGUILayout.Slider("BGM Volume", currentAudioState.bgmVolume, 0f, 1f);
                if (newBGM != currentAudioState.bgmVolume)
                {
                    audioManager.SetBGMVolume(newBGM);
                }
                
                float newAmbient = EditorGUILayout.Slider("Ambient Volume", currentAudioState.ambientVolume, 0f, 1f);
                if (newAmbient != currentAudioState.ambientVolume)
                {
                    audioManager.SetAmbientVolume(newAmbient);
                }
                
                float newEffect = EditorGUILayout.Slider("Effect Volume", currentAudioState.effectVolume, 0f, 1f);
                if (newEffect != currentAudioState.effectVolume)
                {
                    audioManager.SetEffectVolume(newEffect);
                }
                
                float newStealth = EditorGUILayout.Slider("Stealth Volume", currentAudioState.stealthAudioVolume, 0f, 1f);
                if (newStealth != currentAudioState.stealthAudioVolume)
                {
                    audioManager.SetStealthAudioVolume(newStealth);
                }
                
                EditorGUILayout.EndVertical();
                
                EditorGUILayout.Space();
                
                if (GUILayout.Button("Reset All Volumes to Default"))
                {
                    ResetVolumesToDefaults();
                }
            }
        }
        
        private void DrawStealthDebugTab()
        {
            EditorGUILayout.LabelField("Stealth Audio Debug", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            if (stealthCoordinator != null)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("Stealth System Status", EditorStyles.boldLabel);
                
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.Toggle("Stealth Mode Active", isStealthModeActive);
                EditorGUILayout.Slider("Current Masking Level", currentMaskingLevel, 0f, 1f);
                EditorGUI.EndDisabledGroup();
                
                EditorGUILayout.Space();
                
                if (GUILayout.Button("Force Toggle Stealth Mode"))
                {
                    stealthCoordinator.SetOverrideStealthMode(!isStealthModeActive);
                    AddEvent($"Stealth mode toggled to {!isStealthModeActive}");
                }
                
                if (GUILayout.Button("Clear Stealth Override"))
                {
                    stealthCoordinator.ClearStealthModeOverride();
                    AddEvent("Stealth mode override cleared");
                }
                
                EditorGUILayout.EndVertical();
                
                EditorGUILayout.Space();
                
                DrawEnvironmentTestControls();
            }
        }
        
        private void DrawEnvironmentTestControls()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Environment Test Controls", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            testEnvironment = (EnvironmentType)EditorGUILayout.EnumPopup("Environment", testEnvironment);
            if (GUILayout.Button("Apply", GUILayout.Width(60)) && dynamicEnvironment != null)
            {
                dynamicEnvironment.ChangeEnvironment(testEnvironment);
                AddEvent($"Environment changed to {testEnvironment}");
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            testWeather = (WeatherType)EditorGUILayout.EnumPopup("Weather", testWeather);
            if (GUILayout.Button("Apply", GUILayout.Width(60)) && dynamicEnvironment != null)
            {
                dynamicEnvironment.ChangeWeather(testWeather);
                AddEvent($"Weather changed to {testWeather}");
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            testTime = (TimeOfDay)EditorGUILayout.EnumPopup("Time of Day", testTime);
            if (GUILayout.Button("Apply", GUILayout.Width(60)) && dynamicEnvironment != null)
            {
                dynamicEnvironment.ChangeTimeOfDay(testTime);
                AddEvent($"Time of day changed to {testTime}");
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawPerformanceTab()
        {
            EditorGUILayout.LabelField("Performance Monitor", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            DrawPerformanceGraphs();
            DrawAudioSourceStatistics();
        }
        
        private void DrawPerformanceGraphs()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Volume History", EditorStyles.boldLabel);
            
            if (volumeHistory.Count > 1)
            {
                Rect graphRect = GUILayoutUtility.GetRect(100, 80);
                DrawGraph(graphRect, volumeHistory, Color.green, "Volume");
            }
            else
            {
                EditorGUILayout.LabelField("Collecting data...");
            }
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();
            
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Tension History", EditorStyles.boldLabel);
            
            if (tensionHistory.Count > 1)
            {
                Rect graphRect = GUILayoutUtility.GetRect(100, 80);
                DrawGraph(graphRect, tensionHistory, Color.red, "Tension");
            }
            else
            {
                EditorGUILayout.LabelField("Collecting data...");
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawAudioSourceStatistics()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Audio Source Statistics", EditorStyles.boldLabel);
            
            var audioSources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
            var playingCount = audioSources.Count(source => source.isPlaying);
            var pausedCount = audioSources.Count(source => !source.isPlaying && source.time > 0);
            
            EditorGUILayout.LabelField($"Total Audio Sources: {audioSources.Length}");
            EditorGUILayout.LabelField($"Currently Playing: {playingCount}");
            EditorGUILayout.LabelField($"Paused: {pausedCount}");
            EditorGUILayout.LabelField($"Stopped: {audioSources.Length - playingCount - pausedCount}");
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawAudioEventsTab()
        {
            EditorGUILayout.LabelField("Recent Audio Events", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            EditorGUILayout.BeginVertical("box");
            
            if (GUILayout.Button("Clear Event Log"))
            {
                recentEvents.Clear();
            }
            
            EditorGUILayout.Space();
            
            if (recentEvents.Count == 0)
            {
                EditorGUILayout.LabelField("No events recorded.");
            }
            else
            {
                foreach (var eventText in recentEvents.TakeLast(20))
                {
                    EditorGUILayout.LabelField($"• {eventText}", EditorStyles.wordWrappedLabel);
                }
            }
            
            EditorGUILayout.EndVertical();
        }
        
        #endregion
        
        #region Utility Methods
        
        /// <summary>
        /// シーン冁E�EオーチE��オシスチE��関連コンポ�Eネント�E参�Eを取征E        /// AudioManager、StealthAudioCoordinator、SpatialAudioManager、DynamicAudioEnvironmentを検索
        /// </summary>
        /// <remarks>
        /// これら�Eコンポ�Eネントが見つからなぁE��合、該当する機�Eは利用できません
        /// </remarks>
        private void FindAudioSystemReferences()
        {
            audioManager = FindFirstObjectByType<AudioManager>();
            stealthCoordinator = FindFirstObjectByType<StealthAudioCoordinator>();
            spatialAudioManager = FindFirstObjectByType<SpatialAudioManager>();
            dynamicEnvironment = FindFirstObjectByType<DynamicAudioEnvironment>();
        }
        
        /// <summary>
        /// モニタリング用チE�Eタの初期匁E        /// 吁E��ーチE��オカチE��リのチE��ォルト�Eリューム値を設宁E        /// </summary>
        private void InitializeMonitoring()
        {
            categoryVolumes[AudioCategory.BGM] = AudioConstants.DEFAULT_BGM_VOLUME;
            categoryVolumes[AudioCategory.Ambient] = AudioConstants.DEFAULT_AMBIENT_VOLUME;
            categoryVolumes[AudioCategory.Effect] = AudioConstants.DEFAULT_EFFECT_VOLUME;
            categoryVolumes[AudioCategory.Stealth] = AudioConstants.DEFAULT_STEALTH_VOLUME;
            categoryVolumes[AudioCategory.UI] = AudioConstants.DEFAULT_MASTER_VOLUME;
        }
        
        /// <summary>
        /// オーチE��オシスチE��の現在の状態データを更新
        /// AudioManagerとStealthAudioCoordinatorから最新の状態を取得し、E        /// パフォーマンス履歴チE�Eタも更新する
        /// </summary>
        /// <remarks>
        /// ボリューム履歴とチE��ション履歴は最大100ポイントまで保持されめE        /// </remarks>
        private void RefreshAudioSystemData()
        {
            if (audioManager != null)
            {
                currentAudioState = audioManager.GetCurrentAudioState();
                
                // Update performance history
                volumeHistory.Add(currentAudioState.masterVolume);
                tensionHistory.Add(currentAudioState.tensionLevel);
                
                if (volumeHistory.Count > MAX_HISTORY_POINTS)
                    volumeHistory.RemoveAt(0);
                if (tensionHistory.Count > MAX_HISTORY_POINTS)
                    tensionHistory.RemoveAt(0);
            }
            
            if (stealthCoordinator != null)
            {
                isStealthModeActive = stealthCoordinator.ShouldReduceNonStealthAudio();
                // Note: currentMaskingLevel would need to be exposed by StealthAudioCoordinator
            }
        }
        
        private void ResetVolumesToDefaults()
        {
            if (audioManager != null)
            {
                audioManager.SetMasterVolume(AudioConstants.DEFAULT_MASTER_VOLUME);
                audioManager.SetBGMVolume(AudioConstants.DEFAULT_BGM_VOLUME);
                audioManager.SetAmbientVolume(AudioConstants.DEFAULT_AMBIENT_VOLUME);
                audioManager.SetEffectVolume(AudioConstants.DEFAULT_EFFECT_VOLUME);
                audioManager.SetStealthAudioVolume(AudioConstants.DEFAULT_STEALTH_VOLUME);
                
                AddEvent("All volumes reset to defaults");
            }
        }
        
        /// <summary>
        /// オーチE��オイベントログにエントリを追加
        /// タイムスタンプ付きでイベントを記録し、最大100件まで履歴を保持
        /// </summary>
        /// <param name="eventText">記録するイベント�E説明文</param>
        private void AddEvent(string eventText)
        {
            string timestampedEvent = $"[{System.DateTime.Now:HH:mm:ss}] {eventText}";
            recentEvents.Add(timestampedEvent);
            
            if (recentEvents.Count > 100) // Keep only latest 100 events
            {
                recentEvents.RemoveAt(0);
            }
        }
        
        /// <summary>
        /// パフォーマンス監視用のグラフを描画
        /// チE�Eタの履歴を線グラフとして表示し、現在値もラベル表示する
        /// </summary>
        /// <param name="rect">グラフを描画する矩形領域</param>
        /// <param name="data">グラフに表示するチE�EタのリスチE/param>
        /// <param name="color">グラフ�E線�E色</param>
        /// <param name="label">グラフ�Eラベル吁E/param>
        /// <remarks>
        /// チE�EタぁE点未満の場合�E描画されなぁE��最小値と最大値に基づぁE��自動スケーリングされめE        /// </remarks>
        private void DrawGraph(Rect rect, List<float> data, Color color, string label)
        {
            if (data.Count < 2) return;
            
            EditorGUI.DrawRect(rect, new Color(0.1f, 0.1f, 0.1f, 1f));
            
            var oldColor = Handles.color;
            Handles.color = color;
            
            float minValue = data.Min();
            float maxValue = data.Max();
            float range = maxValue - minValue;
            
            if (range < 0.001f) range = 1f; // Avoid division by zero
            
            for (int i = 0; i < data.Count - 1; i++)
            {
                float x1 = rect.x + (float)i / (data.Count - 1) * rect.width;
                float y1 = rect.y + rect.height - ((data[i] - minValue) / range) * rect.height;
                
                float x2 = rect.x + (float)(i + 1) / (data.Count - 1) * rect.width;
                float y2 = rect.y + rect.height - ((data[i + 1] - minValue) / range) * rect.height;
                
                Handles.DrawLine(new Vector3(x1, y1, 0), new Vector3(x2, y2, 0));
            }
            
            Handles.color = oldColor;
            
            GUI.Label(new Rect(rect.x + 5, rect.y + 5, 100, 20), $"{label}: {data.LastOrDefault():F2}");
        }
        
        #endregion
    }
}