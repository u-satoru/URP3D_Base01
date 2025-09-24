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
    /// 繧ｪ繝ｼ繝・ぅ繧ｪ繧ｷ繧ｹ繝・Β蟆ら畑繝・ヰ繝・ぎ繝ｼ繝ｻ繝｢繝九ち繝ｪ繝ｳ繧ｰ繝・・繝ｫ
    /// 邨ｱ蜷医が繝ｼ繝・ぅ繧ｪ繧ｷ繧ｹ繝・Β縺ｮ迥ｶ諷狗屮隕悶√ョ繝舌ャ繧ｰ縲√ユ繧ｹ繝域ｩ溯・繧呈署萓・    /// 
    /// 荳ｻ縺ｪ讖溯・・・    /// - 繝ｪ繧｢繝ｫ繧ｿ繧､繝繧ｪ繝ｼ繝・ぅ繧ｪ繧ｷ繧ｹ繝・Β迥ｶ諷狗屮隕・    /// - 繝懊Μ繝･繝ｼ繝繝ｬ繝吶Ν縺ｮ蜍慕噪蛻ｶ蠕｡縺ｨ繝・せ繝・    /// - 繧ｹ繝・Ν繧ｹ繝｢繝ｼ繝峨・繝・ヰ繝・げ縺ｨ繝・せ繝育腸蠅・宛蠕｡
    /// - 繝代ヵ繧ｩ繝ｼ繝槭Φ繧ｹ逶｣隕悶→繧ｰ繝ｩ繝戊｡ｨ遉ｺ
    /// - 繧ｪ繝ｼ繝・ぅ繧ｪ繧､繝吶Φ繝医・繝ｪ繧｢繝ｫ繧ｿ繧､繝霑ｽ霍｡
    /// 
    /// 菴ｿ逕ｨ繧ｷ繝ｼ繝ｳ・・    /// - 繧ｪ繝ｼ繝・ぅ繧ｪ繧ｷ繧ｹ繝・Β縺ｮ蜍穂ｽ懃｢ｺ隱・    /// - 繧ｲ繝ｼ繝荳ｭ縺ｮ繧ｪ繝ｼ繝・ぅ繧ｪ繝舌Λ繝ｳ繧ｹ隱ｿ謨ｴ
    /// - 繧ｹ繝・Ν繧ｹ讖溯・縺ｮ繝・せ繝医→繝・ヰ繝・げ
    /// - 繝代ヵ繧ｩ繝ｼ繝槭Φ繧ｹ蝠城｡後・迚ｹ螳・    /// 
    /// 繧｢繧ｯ繧ｻ繧ｹ譁ｹ豕包ｼ啅nity 繝｡繝九Η繝ｼ > asterivo.Unity60/Audio/Audio System Debugger
    /// 豕ｨ諢擾ｼ壹・繝ｬ繧､繝｢繝ｼ繝我ｸｭ縺ｮ縺ｿ蛻ｩ逕ｨ蜿ｯ閭ｽ
    /// </summary>
    public class AudioSystemDebugger : EditorWindow
    {
        #region Window Management
        
        /// <summary>
        /// 繧ｪ繝ｼ繝・ぅ繧ｪ繧ｷ繧ｹ繝・Β繝・ヰ繝・ぎ繝ｼ繧ｦ繧｣繝ｳ繝峨え繧定｡ｨ遉ｺ縺吶ｋ
        /// Unity 繝｡繝九Η繝ｼ縺九ｉ蜻ｼ縺ｳ蜃ｺ縺輔ｌ繧九お繝・ぅ繧ｿ諡｡蠑ｵ繝｡繝九Η繝ｼ繧｢繧､繝・Β
        /// </summary>
        /// <remarks>
        /// 繧ｦ繧｣繝ｳ繝峨え縺ｮ譛蟆上し繧､繧ｺ縺ｯ800x600縺ｫ險ｭ螳壹＆繧後・        /// 繝ｪ繧｢繝ｫ繧ｿ繧､繝縺ｧ繧ｪ繝ｼ繝・ぅ繧ｪ繧ｷ繧ｹ繝・Β縺ｮ迥ｶ諷九ｒ逶｣隕悶〒縺阪∪縺吶・        /// </remarks>
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
        /// 繧ｦ繧｣繝ｳ繝峨え縺梧怏蜉ｹ縺ｫ縺ｪ縺｣縺滓凾縺ｮ蛻晄悄蛹門・逅・        /// 繧ｨ繝・ぅ繧ｿ縺ｮ譖ｴ譁ｰ繧､繝吶Φ繝医↓逋ｻ骭ｲ縺励√が繝ｼ繝・ぅ繧ｪ繧ｷ繧ｹ繝・Β縺ｮ蜿ら・繧貞叙蠕励☆繧・        /// </summary>
        private void OnEnable()
        {
            EditorApplication.update += OnEditorUpdate;
            FindAudioSystemReferences();
            InitializeMonitoring();
        }
        
        /// <summary>
        /// 繧ｦ繧｣繝ｳ繝峨え縺檎┌蜉ｹ縺ｫ縺ｪ縺｣縺滓凾縺ｮ繧ｯ繝ｪ繝ｼ繝ｳ繧｢繝・・蜃ｦ逅・        /// 繧ｨ繝・ぅ繧ｿ縺ｮ譖ｴ譁ｰ繧､繝吶Φ繝医°繧臥匳骭ｲ隗｣髯､繧定｡後≧
        /// </summary>
        private void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
        }
        
        /// <summary>
        /// 繧ｨ繝・ぅ繧ｿ縺ｮ譖ｴ譁ｰ繝ｫ繝ｼ繝励〒繧ｪ繝ｼ繝・ぅ繧ｪ繧ｷ繧ｹ繝・Β繝・・繧ｿ繧貞ｮ壽悄逧・↓譖ｴ譁ｰ
        /// 閾ｪ蜍輔Μ繝輔Ξ繝・す繝･縺梧怏蜉ｹ縺ｪ蝣ｴ蜷医∵欠螳壹＆繧後◆髢馴囈縺ｧ繝・・繧ｿ繧呈峩譁ｰ縺励※繧ｦ繧｣繝ｳ繝峨え繧貞・謠冗判縺吶ｋ
        /// </summary>
        /// <remarks>
        /// 繝励Ξ繧､繝｢繝ｼ繝我ｸｭ縺ｮ縺ｿ螳溯｡後＆繧後〉efreshInterval・医ョ繝輔か繝ｫ繝・.5遘抵ｼ峨＃縺ｨ縺ｫ譖ｴ譁ｰ縺輔ｌ繧・        /// </remarks>
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
        /// 繧ｦ繧｣繝ｳ繝峨え縺ｮGUI謠冗判蜃ｦ逅・        /// 繝励Ξ繧､繝｢繝ｼ繝我ｸｭ縺ｮ縺ｿ蛻ｩ逕ｨ蜿ｯ閭ｽ縺ｧ縲√ち繝悶・繝ｼ繧ｹ縺ｮ繧､繝ｳ繧ｿ繝ｼ繝輔ぉ繝ｼ繧ｹ繧呈署萓・        /// </summary>
        /// <remarks>
        /// 莉･荳九・5縺､縺ｮ繧ｿ繝悶〒讒区・縺輔ｌ縺ｦ縺・∪縺呻ｼ・        /// 0: System Monitor - 繧ｷ繧ｹ繝・Β迥ｶ諷玖｡ｨ遉ｺ縺ｨ蝓ｺ譛ｬ謫堺ｽ・        /// 1: Volume Control - 蜷・き繝・ざ繝ｪ縺ｮ繝懊Μ繝･繝ｼ繝蛻ｶ蠕｡
        /// 2: Stealth Debug - 繧ｹ繝・Ν繧ｹ繧ｷ繧ｹ繝・Β縺ｮ繝・ヰ繝・げ
        /// 3: Performance - 繝代ヵ繧ｩ繝ｼ繝槭Φ繧ｹ逶｣隕悶→繧ｰ繝ｩ繝戊｡ｨ遉ｺ
        /// 4: Audio Events - 繧ｪ繝ｼ繝・ぅ繧ｪ繧､繝吶Φ繝医・螻･豁ｴ陦ｨ遉ｺ
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
        /// 繧ｦ繧｣繝ｳ繝峨え繝倥ャ繝繝ｼ驛ｨ蛻・・GUI謠冗判
        /// 繧ｿ繧､繝医Ν陦ｨ遉ｺ縲∬・蜍輔Μ繝輔Ξ繝・す繝･蛻・ｊ譖ｿ縺医∵焔蜍輔Μ繝輔Ξ繝・す繝･繝懊ち繝ｳ繧貞性繧
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
        /// 繧ｿ繝悶ヰ繝ｼ縺ｮGUI謠冗判
        /// 5縺､縺ｮ繧ｿ繝厄ｼ・ystem Monitor, Volume Control, Stealth Debug, Performance, Audio Events・峨ｒ陦ｨ遉ｺ
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
                    EditorGUILayout.LabelField($"窶｢ {eventText}", EditorStyles.wordWrappedLabel);
                }
            }
            
            EditorGUILayout.EndVertical();
        }
        
        #endregion
        
        #region Utility Methods
        
        /// <summary>
        /// 繧ｷ繝ｼ繝ｳ蜀・・繧ｪ繝ｼ繝・ぅ繧ｪ繧ｷ繧ｹ繝・Β髢｢騾｣繧ｳ繝ｳ繝昴・繝阪Φ繝医・蜿ら・繧貞叙蠕・        /// AudioManager縲ヾtealthAudioCoordinator縲ヾpatialAudioManager縲．ynamicAudioEnvironment繧呈､懃ｴ｢
        /// </summary>
        /// <remarks>
        /// 縺薙ｌ繧峨・繧ｳ繝ｳ繝昴・繝阪Φ繝医′隕九▽縺九ｉ縺ｪ縺・ｴ蜷医∬ｩｲ蠖薙☆繧区ｩ溯・縺ｯ蛻ｩ逕ｨ縺ｧ縺阪∪縺帙ｓ
        /// </remarks>
        private void FindAudioSystemReferences()
        {
            audioManager = FindFirstObjectByType<AudioManager>();
            stealthCoordinator = FindFirstObjectByType<StealthAudioCoordinator>();
            spatialAudioManager = FindFirstObjectByType<SpatialAudioManager>();
            dynamicEnvironment = FindFirstObjectByType<DynamicAudioEnvironment>();
        }
        
        /// <summary>
        /// 繝｢繝九ち繝ｪ繝ｳ繧ｰ逕ｨ繝・・繧ｿ縺ｮ蛻晄悄蛹・        /// 蜷・が繝ｼ繝・ぅ繧ｪ繧ｫ繝・ざ繝ｪ縺ｮ繝・ヵ繧ｩ繝ｫ繝医・繝ｪ繝･繝ｼ繝蛟､繧定ｨｭ螳・        /// </summary>
        private void InitializeMonitoring()
        {
            categoryVolumes[AudioCategory.BGM] = AudioConstants.DEFAULT_BGM_VOLUME;
            categoryVolumes[AudioCategory.Ambient] = AudioConstants.DEFAULT_AMBIENT_VOLUME;
            categoryVolumes[AudioCategory.Effect] = AudioConstants.DEFAULT_EFFECT_VOLUME;
            categoryVolumes[AudioCategory.Stealth] = AudioConstants.DEFAULT_STEALTH_VOLUME;
            categoryVolumes[AudioCategory.UI] = AudioConstants.DEFAULT_MASTER_VOLUME;
        }
        
        /// <summary>
        /// 繧ｪ繝ｼ繝・ぅ繧ｪ繧ｷ繧ｹ繝・Β縺ｮ迴ｾ蝨ｨ縺ｮ迥ｶ諷九ョ繝ｼ繧ｿ繧呈峩譁ｰ
        /// AudioManager縺ｨStealthAudioCoordinator縺九ｉ譛譁ｰ縺ｮ迥ｶ諷九ｒ蜿門ｾ励＠縲・        /// 繝代ヵ繧ｩ繝ｼ繝槭Φ繧ｹ螻･豁ｴ繝・・繧ｿ繧よ峩譁ｰ縺吶ｋ
        /// </summary>
        /// <remarks>
        /// 繝懊Μ繝･繝ｼ繝螻･豁ｴ縺ｨ繝・Φ繧ｷ繝ｧ繝ｳ螻･豁ｴ縺ｯ譛螟ｧ100繝昴う繝ｳ繝医∪縺ｧ菫晄戟縺輔ｌ繧・        /// </remarks>
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
        /// 繧ｪ繝ｼ繝・ぅ繧ｪ繧､繝吶Φ繝医Ο繧ｰ縺ｫ繧ｨ繝ｳ繝医Μ繧定ｿｽ蜉
        /// 繧ｿ繧､繝繧ｹ繧ｿ繝ｳ繝嶺ｻ倥″縺ｧ繧､繝吶Φ繝医ｒ險倬鹸縺励∵怙螟ｧ100莉ｶ縺ｾ縺ｧ螻･豁ｴ繧剃ｿ晄戟
        /// </summary>
        /// <param name="eventText">險倬鹸縺吶ｋ繧､繝吶Φ繝医・隱ｬ譏取枚</param>
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
        /// 繝代ヵ繧ｩ繝ｼ繝槭Φ繧ｹ逶｣隕也畑縺ｮ繧ｰ繝ｩ繝輔ｒ謠冗判
        /// 繝・・繧ｿ縺ｮ螻･豁ｴ繧堤ｷ壹げ繝ｩ繝輔→縺励※陦ｨ遉ｺ縺励∫樟蝨ｨ蛟､繧ゅΛ繝吶Ν陦ｨ遉ｺ縺吶ｋ
        /// </summary>
        /// <param name="rect">繧ｰ繝ｩ繝輔ｒ謠冗判縺吶ｋ遏ｩ蠖｢鬆伜沺</param>
        /// <param name="data">繧ｰ繝ｩ繝輔↓陦ｨ遉ｺ縺吶ｋ繝・・繧ｿ縺ｮ繝ｪ繧ｹ繝・/param>
        /// <param name="color">繧ｰ繝ｩ繝輔・邱壹・濶ｲ</param>
        /// <param name="label">繧ｰ繝ｩ繝輔・繝ｩ繝吶Ν蜷・/param>
        /// <remarks>
        /// 繝・・繧ｿ縺・轤ｹ譛ｪ貅縺ｮ蝣ｴ蜷医・謠冗判縺輔ｌ縺ｪ縺・よ怙蟆丞､縺ｨ譛螟ｧ蛟､縺ｫ蝓ｺ縺･縺・※閾ｪ蜍輔せ繧ｱ繝ｼ繝ｪ繝ｳ繧ｰ縺輔ｌ繧・        /// </remarks>
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
