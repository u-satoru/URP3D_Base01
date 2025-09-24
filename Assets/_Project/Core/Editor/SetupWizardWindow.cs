using UnityEngine;
using asterivo.Unity60.Core.Setup;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using asterivo.Unity60.Core.Editor.Setup;

namespace asterivo.Unity60.Core.Editor
{
    /// <summary>
    /// Interactive Setup Wizard System - Unity Editor Window基盤クラス
    /// TASK-003.3: SetupWizardWindow UI基盤実装
    /// 
    /// 30分→1分（97%短縮）のプロジェクトセットアップを実現する
    /// Clone & Create価値実現のための核心コンポーネント
    /// 
    /// 主な機能：
    /// - ウィザードステップ管理システム
    /// - Environment Diagnostics統合UI
    /// - 6ジャンル選択システム（FPS/TPS/Platformer/Stealth/Adventure/Strategy）
    /// - 1分セットアッププロトタイプ検証
    /// - モジュール選択システム（Audio/Localization/Analytics）
    /// - ProjectGenerationEngine統合
    /// 
    /// アクセス方法：Unity メニュー > asterivo.Unity60/Setup/Interactive Setup Wizard
    /// </summary>
    public class SetupWizardWindow : EditorWindow
    {
        #region Private Fields
        
        // UI状態管理
        private Vector2 scrollPosition;
        private WizardStep currentStep = WizardStep.EnvironmentCheck;
        private bool isInitialized = false;
        
        // Environment Diagnostics統合
        private SystemRequirementChecker.SystemRequirementReport environmentReport;
        private bool isRunningEnvironmentCheck = false;
                
        // ジャンル管理システム
        private GenreManager genreManager;
        private GameGenreType selectedPreviewGenre = GameGenreType.Adventure;
        private Vector2 genreScrollPosition;
        private Texture2D currentPreviewImage;
        private bool showPreviewDetails = false;
private string environmentCheckStatus = "";
        
        // ウィザードステップデータ
        private WizardConfiguration wizardConfig;
        private Dictionary<WizardStep, StepState> stepStates;
        
        // UI スタイル
        private GUIStyle headerStyle;
        private GUIStyle stepButtonStyle;
        private GUIStyle statusStyle;
        private GUIStyle cardStyle;
        private bool stylesInitialized = false;
        
        // パフォーマンス測定
        private DateTime setupStartTime;
        private TimeSpan totalSetupTime;
        private bool isSetupComplete = false;
        
        #endregion
        
        #region Data Structures
        
        /// <summary>
        /// ウィザードステップ定義
        /// </summary>
        public enum WizardStep
        {
            EnvironmentCheck,    // 環境診断
            GenreSelection,      // ジャンル選択
            ModuleSelection,     // モジュール選択
            ProjectGeneration,   // プロジェクト生成
            Verification         // 検証・完了
        }
        
        /// <summary>
        /// GenreManagerを初期化します
        /// </summary>
        private void InitializeGenreManager()
        {
            try
            {
                // GenreManager インスタンスを取得または作成
                if (genreManager == null)
                {
                    // GenreManagerはScriptableObjectなので、アセットから読み込むか作成する
                    var genreManagers = Resources.FindObjectsOfTypeAll<GenreManager>();
                    if (genreManagers.Length > 0)
                    {
                        genreManager = genreManagers[0];
                    }
                    else
                    {
                        genreManager = ScriptableObject.CreateInstance<GenreManager>();
                    }
                }
                
                // 不足しているジャンルを作成
                GenreManager.CreateMissingGenres();
                
                // GenreManagerを初期化
                genreManager.Initialize();
                
                UnityEngine.Debug.Log("[SetupWizard] GenreManager initialized successfully");
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"[SetupWizard] Failed to initialize GenreManager: {ex.Message}");
            }
        }
        
        /// <summary>
        /// ステップ状態管理
        /// </summary>
        [Serializable]
        public class StepState
        {
            public bool isCompleted = false;
            public bool isCurrentStep = false;
            public string statusMessage = "";
            public float progressPercent = 0f;
        }
        
        /// <summary>
        /// ウィザード設定データ
        /// </summary>
        [Serializable]
        public class WizardConfiguration
        {
            // ジャンル選択
            public GameGenreType selectedGenre = GameGenreType.Adventure;
            public Dictionary<string, object> genreSettings = new Dictionary<string, object>();
            
            // モジュール選択
            public List<string> selectedModules = new List<string>();
            public Dictionary<string, bool> moduleConfigurations = new Dictionary<string, bool>();
            
            // プロジェクト設定
            public string projectName = "NewUnityProject";
            public string projectPath = "";
            public bool createProjectFolder = true;
            
            // パフォーマンス設定
            public bool enableAutoOptimization = true;
            public bool enableFrameDistribution = true;
        }
        
        /// <summary>
        /// プロジェクトジャンル定義
        /// </summary>
        public enum ProjectGenre
        {
            None,
            FPS,           // First Person Shooter
            TPS,           // Third Person Shooter
            Platformer,    // 3D Platformer
            Stealth,       // Stealth Action
            Adventure,     // Adventure Game
            Strategy       // Real-time Strategy
        }
        
        #endregion
        
        #region Unity Editor Window Lifecycle
        
        /// <summary>
        /// Setup Wizardウィンドウを表示
        /// </summary>
        [MenuItem("asterivo.Unity60/Setup/Interactive Setup Wizard", priority = 1)]
        public static void ShowWindow()
        {
            var window = GetWindow<SetupWizardWindow>("Setup Wizard");
            window.minSize = new Vector2(800f, 600f);
            window.maxSize = new Vector2(1200f, 900f);
            window.Show();
        }
        
        /// <summary>
        /// ウィンドウの初期化
        /// </summary>
        private void OnEnable()
        {
            InitializeWizard();
        }
        
        /// <summary>
        /// UIの描画
        /// </summary>
        private void OnGUI()
        {
            // 初期化確認（OnGUIが最初に呼ばれる可能性があるため）
            if (!isInitialized)
            {
                InitializeWizard();
            }
            
            InitializeStyles();;
            
            EditorGUILayout.BeginVertical();
            {
                DrawHeader();
                DrawStepNavigation();
                
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                {
                    DrawCurrentStep();
                }
                EditorGUILayout.EndScrollView();
                
                DrawFooter();
            }
            EditorGUILayout.EndVertical();
        }
        
        #endregion
        
        #region Initialization
        
        /// <summary>
        /// ウィザードの初期化
        /// </summary>
private void InitializeWizard()
        {
            if (isInitialized) return;
            
            // 設定の初期化
            wizardConfig = new WizardConfiguration();
            
            // ステップ状態の初期化
            stepStates = new Dictionary<WizardStep, StepState>();
            foreach (WizardStep step in Enum.GetValues(typeof(WizardStep)))
            {
                stepStates[step] = new StepState();
            }
            
            // 最初のステップを現在のステップに設定
            stepStates[WizardStep.EnvironmentCheck].isCurrentStep = true;
            
            // セットアップ開始時間記録
                        
            // GenreManager初期化
            InitializeGenreManager();
            setupStartTime = DateTime.Now;
            
            isInitialized = true;
            
            UnityEngine.Debug.Log("[SetupWizard] Interactive Setup Wizard initialized - Starting Clone & Create value realization");
        }
        
        /// <summary>
        /// UIスタイルの初期化
        /// </summary>
        private void InitializeStyles()
        {
            if (stylesInitialized) return;
            
            // ヘッダースタイル
            headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black }
            };
            
            // ステップボタンスタイル
            stepButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                fixedHeight = 30f
            };
            
            // ステータススタイル
            statusStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                wordWrap = true,
                normal = { textColor = EditorGUIUtility.isProSkin ? Color.gray : Color.gray }
            };
            
            // カードスタイル
            cardStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(10, 10, 10, 10),
                margin = new RectOffset(5, 5, 5, 5)
            };
            
            stylesInitialized = true;
        }
        
        #endregion
        
        #region UI Drawing Methods
        
        /// <summary>
        /// ヘッダーの描画
        /// </summary>
        private void DrawHeader()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Unity 6 Interactive Setup Wizard", headerStyle);
            EditorGUILayout.LabelField("Clone & Create価値実現 - 30分→1分セットアップシステム", statusStyle);
            EditorGUILayout.Space();
            
            // プログレスバー
            float totalProgress = CalculateTotalProgress();
            Rect progressRect = EditorGUILayout.GetControlRect(false, 20f);
            EditorGUI.ProgressBar(progressRect, totalProgress, $"Overall Progress: {totalProgress:P0}");
            
            EditorGUILayout.Space();
        }
        
        /// <summary>
        /// ステップナビゲーションの描画
        /// </summary>
        private void DrawStepNavigation()
        {
            // stepStatesが初期化されていない場合は無視する
            if (stepStates == null)
            {
                EditorGUILayout.LabelField("初期化中...", EditorStyles.centeredGreyMiniLabel);
                return;
            }
            
            EditorGUILayout.BeginHorizontal();
            
            foreach (WizardStep step in Enum.GetValues(typeof(WizardStep)))
            {
                var state = stepStates[step];
                
                // ボタンの色を状態に応じて変更
                Color originalColor = GUI.backgroundColor;
                if (state.isCurrentStep)
                    GUI.backgroundColor = Color.cyan;
                else if (state.isCompleted)
                    GUI.backgroundColor = Color.green;
                else
                    GUI.backgroundColor = Color.gray;
                
                if (GUILayout.Button(GetStepDisplayName(step), stepButtonStyle, GUILayout.ExpandWidth(true)))
                {
                    if (CanNavigateToStep(step))
                    {
                        NavigateToStep(step);
                    }
                }
                
                GUI.backgroundColor = originalColor;
            }
            
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
        }
        
        /// <summary>
        /// 現在のステップの描画
        /// </summary>
        private void DrawCurrentStep()
        {
            switch (currentStep)
            {
                case WizardStep.EnvironmentCheck:
                    DrawEnvironmentCheckStep();
                    break;
                case WizardStep.GenreSelection:
                    DrawGenreSelectionStep();
                    break;
                case WizardStep.ModuleSelection:
                    DrawModuleSelectionStep();
                    break;
                case WizardStep.ProjectGeneration:
                    DrawProjectGenerationStep();
                    break;
                case WizardStep.Verification:
                    DrawVerificationStep();
                    break;
            }
        }
        
        /// <summary>
        /// フッターの描画
        /// </summary>
        private void DrawFooter()
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            
            // 前のステップボタン
            EditorGUI.BeginDisabledGroup(!CanGoToPreviousStep());
            if (GUILayout.Button("Previous", GUILayout.Width(100f)))
            {
                GoToPreviousStep();
            }
            EditorGUI.EndDisabledGroup();
            
            GUILayout.FlexibleSpace();
            
            // セットアップ時間表示
            if (setupStartTime != default(DateTime))
            {
                var elapsedTime = DateTime.Now - setupStartTime;
                EditorGUILayout.LabelField($"Setup Time: {elapsedTime:mm\\:ss}", statusStyle);
            }
            
            GUILayout.FlexibleSpace();
            
            // 次のステップボタン
            EditorGUI.BeginDisabledGroup(!CanGoToNextStep());
            if (GUILayout.Button("Next", GUILayout.Width(100f)))
            {
                GoToNextStep();
            }
            EditorGUI.EndDisabledGroup();
            
            EditorGUILayout.EndHorizontal();
        }
        
        #endregion
        
        #region Step Implementation
        
        /// <summary>
        /// 環境チェックステップの描画
        /// </summary>
        private void DrawEnvironmentCheckStep()
        {
            EditorGUILayout.BeginVertical(cardStyle);
            
            EditorGUILayout.LabelField("Environment Diagnostics", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("システム要件とUnity環境の包括的な診断を実行します", statusStyle);
            EditorGUILayout.Space();
            
            // 環境チェック実行ボタン
            EditorGUI.BeginDisabledGroup(isRunningEnvironmentCheck);
            if (GUILayout.Button("Run Environment Check", GUILayout.Height(30f)))
            {
                RunEnvironmentCheck();
            }
            EditorGUI.EndDisabledGroup();
            
            // 進行状況表示
            if (isRunningEnvironmentCheck)
            {
                EditorGUILayout.LabelField("Status: " + environmentCheckStatus, statusStyle);
                var progressRect = EditorGUILayout.GetControlRect(false, 15f);
                EditorGUI.ProgressBar(progressRect, stepStates[WizardStep.EnvironmentCheck].progressPercent / 100f, 
                    $"{stepStates[WizardStep.EnvironmentCheck].progressPercent:F0}%");
            }
            
            // 結果表示
            if (environmentReport != null)
            {
                EditorGUILayout.Space();
                DrawEnvironmentReport();
            }
            
            EditorGUILayout.EndVertical();
        }
        
        /// <summary>
        /// ジャンル選択ステップの描画
        /// </summary>
        /// <summary>
        /// ジャンル選択ステップの描画 - 6ジャンルプレビューUI対応
        /// </summary>
        private void DrawGenreSelectionStep()
        {
            EditorGUILayout.BeginVertical(cardStyle);
            
            EditorGUILayout.LabelField("Game Genre Selection", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("プロジェクトのジャンルを選択してください（各15分ゲームプレイ対応）", statusStyle);
            EditorGUILayout.Space();
            
            if (genreManager == null)
            {
                EditorGUILayout.HelpBox("GenreManagerが初期化されていません。初期化しています...", MessageType.Warning);
                InitializeGenreManager();
                return;
            }
            
            // ジャンルグリッド表示開始
            DrawGenreGrid();
            
            // 選択中ジャンルの詳細情報
            if (wizardConfig.selectedGenre != GameGenreType.Adventure || showPreviewDetails)
            {
                EditorGUILayout.Space();
                DrawGenreDetails(wizardConfig.selectedGenre);
            }
            
            EditorGUILayout.EndVertical();
        }
        
        /// <summary>
        /// ジャンルグリッドUI描画
        /// </summary>
        private void DrawGenreGrid()
        {
            var supportedGenres = genreManager.GetSupportedGenreTypes();
            if (supportedGenres.Length == 0)
            {
                EditorGUILayout.HelpBox("ジャンルテンプレートが見つかりません。'asterivo.Unity60/Setup/Create Missing Genres'で作成してください。", MessageType.Warning);
                if (GUILayout.Button("ジャンルテンプレートを作成"))
                {
                    GenreManager.CreateMissingGenres();
                    genreManager.Initialize();
                }
                return;
            }
            
            // 2x3 グリッドレイアウト
            int columns = 3;
            int index = 0;
            
            foreach (var genreType in supportedGenres)
            {
                if (index % columns == 0)
                {
                    EditorGUILayout.BeginHorizontal();
                }
                
                DrawGenreCard(genreType);
                
                index++;
                
                if (index % columns == 0 || index == supportedGenres.Length)
                {
                    EditorGUILayout.EndHorizontal();
                }
            }
        }
        
        /// <summary>
        /// 個別ジャンルカード描画
        /// </summary>
        private void DrawGenreCard(GameGenreType genreType)
        {
            var genre = genreManager.GetGenre(genreType);
            if (genre == null) return;
            
            bool isSelected = wizardConfig.selectedGenre == genreType;
            
            // カードスタイル設定
            var cardBgStyle = new GUIStyle(GUI.skin.box);
            if (isSelected)
            {
                cardBgStyle.normal.background = EditorGUIUtility.whiteTexture;
                GUI.backgroundColor = new Color(0.3f, 0.6f, 1f, 0.3f);
            }
            
            EditorGUILayout.BeginVertical(cardBgStyle, GUILayout.Width(200), GUILayout.Height(160));
            
            // プレビュー画像
            var previewImage = genre.PreviewImage;
            if (previewImage != null)
            {
                var rect = GUILayoutUtility.GetRect(180, 80, GUI.skin.box);
                GUI.DrawTexture(rect, previewImage, ScaleMode.ScaleToFit);
            }
            else
            {
                var rect = GUILayoutUtility.GetRect(180, 80, GUI.skin.box);
                EditorGUI.DrawRect(rect, new Color(0.2f, 0.2f, 0.2f));
                GUI.Label(rect, "No Preview\nImage", EditorStyles.centeredGreyMiniLabel);
            }
            
            // ジャンル名
            EditorGUILayout.LabelField(genre.DisplayName, EditorStyles.boldLabel, GUILayout.Height(20));
            
            // 簡略説明（短縮版）
            var shortDescription = genre.Description.Length > 40 
                ? genre.Description.Substring(0, 37) + "..."
                : genre.Description;
            EditorGUILayout.LabelField(shortDescription, EditorStyles.wordWrappedMiniLabel, GUILayout.Height(30));
            
            // 選択ボタン
            if (GUILayout.Button(isSelected ? "✓ Selected" : "Select"))
            {
                wizardConfig.selectedGenre = genreType;
                selectedPreviewGenre = genreType;
                showPreviewDetails = true;
            }
            
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndVertical();
        }
        
        /// <summary>
        /// ジャンル詳細情報描画
        /// </summary>
        private void DrawGenreDetails(GameGenreType genreType)
        {
            var genre = genreManager.GetGenre(genreType);
            if (genre == null) return;
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            // ヘッダー
            EditorGUILayout.LabelField($"Genre Details: {genre.DisplayName}", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            // 説明
            EditorGUILayout.LabelField("説明:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(genre.Description, EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space();
            
            // 技術仕様サマリー
            EditorGUILayout.LabelField("技術仕様:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"• カメラ: {(genre.CameraConfig.firstPersonView ? "FPS" : "TPS")}, FOV: {genre.CameraConfig.defaultFOV}°");
            EditorGUILayout.LabelField($"• 移動: 歩行 {genre.MovementConfig.walkSpeed}m/s, 走行 {genre.MovementConfig.runSpeed}m/s");
            EditorGUILayout.LabelField($"• AI: 最大NPC数 {genre.AIConfig.maxNPCCount}体, 検知範囲 {genre.AIConfig.defaultDetectionRange}m");
            EditorGUILayout.Space();
            
            // 必要モジュール
            var requiredModules = genre.RequiredModules;
            if (requiredModules.Count > 0)
            {
                EditorGUILayout.LabelField("必要モジュール:", EditorStyles.boldLabel);
                foreach (var module in requiredModules)
                {
                    EditorGUILayout.LabelField($"• {module}");
                }
            }
            
            // 推奨モジュール
            var recommendedModules = genre.RecommendedModules;
            if (recommendedModules.Count > 0)
            {
                EditorGUILayout.LabelField("推奨モジュール:", EditorStyles.boldLabel);
                foreach (var module in recommendedModules)
                {
                    EditorGUILayout.LabelField($"• {module}");
                }
            }
            
            EditorGUILayout.EndVertical();
        }
        
        /// <summary>
        /// モジュール選択ステップの描画
        /// </summary>
        /// <summary>
        /// モジュール選択ステップの描画
        /// </summary>
        private void DrawModuleSelectionStep()
        {
            EditorGUILayout.BeginVertical(cardStyle);
            
            EditorGUILayout.LabelField("Module Selection", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("プロジェクトに含める追加モジュールを選択してください", statusStyle);
            EditorGUILayout.Space();

            var genre = genreManager.GetGenre(wizardConfig.selectedGenre);
            if (genre == null)
            {
                EditorGUILayout.HelpBox("ジャンルが選択されていません。前のステップに戻ってください。", MessageType.Warning);
                EditorGUILayout.EndVertical();
                return;
            }

            // 必須モジュール
            EditorGUILayout.LabelField("必須モジュール", EditorStyles.boldLabel);
            if (genre.RequiredModules.Count > 0)
            {
                foreach (var module in genre.RequiredModules)
                {
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.Toggle(module, true);
                    EditorGUI.EndDisabledGroup();
                }
            }
            else
            {
                EditorGUILayout.LabelField("なし", statusStyle);
            }

            EditorGUILayout.Space();

            // 推奨モジュール
            EditorGUILayout.LabelField("推奨モジュール", EditorStyles.boldLabel);
            if (genre.RecommendedModules.Count > 0)
            {
                foreach (var module in genre.RecommendedModules)
                {
                    bool isSelected = wizardConfig.selectedModules.Contains(module);
                    bool newSelection = EditorGUILayout.Toggle(module, isSelected);
                    UpdateModuleSelection(module, isSelected, newSelection);
                }
            }
            else
            {
                EditorGUILayout.LabelField("なし", statusStyle);
            }

            EditorGUILayout.Space();

            // オプションモジュール
            EditorGUILayout.LabelField("オプションモジュール", EditorStyles.boldLabel);
            if (genre.OptionalModules.Count > 0)
            {
                foreach (var module in genre.OptionalModules)
                {
                    bool isSelected = wizardConfig.selectedModules.Contains(module);
                    bool newSelection = EditorGUILayout.Toggle(module, isSelected);
                    UpdateModuleSelection(module, isSelected, newSelection);
                }
            }
            else
            {
                EditorGUILayout.LabelField("なし", statusStyle);
            }

            EditorGUILayout.Space();
            
            // 追加モジュール選択
            DrawAdditionalModules();

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// モジュール選択状態を更新するヘルパーメソッド
        /// </summary>
        private void UpdateModuleSelection(string module, bool isSelected, bool newSelection)
        {
            if (newSelection && !isSelected)
            {
                wizardConfig.selectedModules.Add(module);
            }
            else if (!newSelection && isSelected)
            {
                wizardConfig.selectedModules.Remove(module);
            }
        }

        /// <summary>
        /// 追加モジュール選択の描画
        /// </summary>
        private void DrawAdditionalModules()
        {
            EditorGUILayout.LabelField("追加システムモジュール", EditorStyles.boldLabel);
            
            var additionalModules = new Dictionary<string, string>
            {
                { "Audio System", "高度なオーディオ管理システム（Timeline、Cinemachine統合）" },
                { "Localization", "多言語対応システム（Unity Localization Package）" },
                { "Analytics", "ゲーム分析・統計システム（Unity Analytics）" },
                { "Input System", "新しいInput Systemの統合" },
                { "Addressables", "動的アセット管理システム" },
                { "Visual Scripting", "ビジュアルスクリプティングサポート" },
                { "AI Navigation", "AI・ナビゲーションシステム" },
                { "Multiplayer", "マルチプレイヤー対応システム" }
            };

            foreach (var module in additionalModules)
            {
                EditorGUILayout.BeginHorizontal();
                
                bool isSelected = wizardConfig.selectedModules.Contains(module.Key);
                bool newSelection = EditorGUILayout.Toggle(isSelected, GUILayout.Width(20));
                
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField(module.Key, EditorStyles.boldLabel);
                EditorGUILayout.LabelField(module.Value, statusStyle);
                EditorGUILayout.EndVertical();
                
                EditorGUILayout.EndHorizontal();
                
                UpdateModuleSelection(module.Key, isSelected, newSelection);
                
                EditorGUILayout.Space(5);
            }
        }
        
        /// <summary>
        /// プロジェクト生成ステップの描画
        /// </summary>
        /// <summary>
        /// プロジェクト生成ステップの描画
        /// </summary>
        private void DrawProjectGenerationStep()
        {
            EditorGUILayout.BeginVertical(cardStyle);
            
            EditorGUILayout.LabelField("Project Generation", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("選択した設定でプロジェクトを生成します", statusStyle);
            EditorGUILayout.Space();
            
            // プロジェクト設定
            wizardConfig.projectName = EditorGUILayout.TextField("Project Name", wizardConfig.projectName);
            
            EditorGUILayout.Space();
            
            // 生成ボタン
            if (GUILayout.Button("Generate Project", GUILayout.Height(40f)))
            {
                // ProjectGenerationEngineを呼び出す
                StartProjectGeneration();
            }
            
            EditorGUILayout.EndVertical();
        }
        
        /// <summary>
        /// 検証ステップの描画
        /// </summary>
        private void DrawVerificationStep()
        {
            EditorGUILayout.BeginVertical(cardStyle);
            
            EditorGUILayout.LabelField("Setup Complete!", EditorStyles.boldLabel);
            
            if (isSetupComplete)
            {
                EditorGUILayout.LabelField($"Total Setup Time: {totalSetupTime:mm\\:ss}", statusStyle);
                
                // 1分達成チェック
                bool under60Seconds = totalSetupTime.TotalSeconds <= 60;
                string timeResult = under60Seconds ? "✅ 1分セットアップ達成!" : "⚠️ 1分を超過しました";
                EditorGUILayout.LabelField(timeResult, under60Seconds ? EditorStyles.helpBox : EditorStyles.boldLabel);
            }
            
            EditorGUILayout.EndVertical();
        }
        
        #endregion
        
        #region Helper Methods
        
        /// <summary>
        /// 環境診断の実行
        /// </summary>
        private async void RunEnvironmentCheck()
        {
            isRunningEnvironmentCheck = true;
            environmentCheckStatus = "Initializing environment check...";
            stepStates[WizardStep.EnvironmentCheck].progressPercent = 0f;
            
            try
            {
                // SystemRequirementCheckerを使用した環境診断
                environmentCheckStatus = "Running system diagnostics...";
                stepStates[WizardStep.EnvironmentCheck].progressPercent = 25f;
                Repaint();
                
                await Task.Delay(500); // UI更新待機
                
                environmentCheckStatus = "Checking Unity configuration...";
                stepStates[WizardStep.EnvironmentCheck].progressPercent = 50f;
                Repaint();
                
                environmentReport = SystemRequirementChecker.CheckAllRequirements();
                
                environmentCheckStatus = "Analyzing results...";
                stepStates[WizardStep.EnvironmentCheck].progressPercent = 75f;
                Repaint();
                
                await Task.Delay(300);
                
                environmentCheckStatus = "Environment check completed";
                stepStates[WizardStep.EnvironmentCheck].progressPercent = 100f;
                stepStates[WizardStep.EnvironmentCheck].isCompleted = true;
                
                UnityEngine.Debug.Log($"[SetupWizard] Environment check completed. Score: {environmentReport.environmentScore}/100");
            }
            catch (Exception ex)
            {
                environmentCheckStatus = "Environment check failed: " + ex.Message;
                UnityEngine.Debug.LogError($"[SetupWizard] Environment check failed: {ex}");
            }
            finally
            {
                isRunningEnvironmentCheck = false;
                Repaint();
            }
        }
        
        /// <summary>
        /// 環境診断結果の描画
        /// </summary>
        private void DrawEnvironmentReport()
        {
            EditorGUILayout.LabelField("Environment Report", EditorStyles.boldLabel);
            
            // スコア表示
            var scoreColor = environmentReport.environmentScore >= 80 ? Color.green : 
                           environmentReport.environmentScore >= 60 ? Color.yellow : Color.red;
                           
            var originalColor = GUI.color;
            GUI.color = scoreColor;
            EditorGUILayout.LabelField($"Environment Score: {environmentReport.environmentScore}/100", EditorStyles.helpBox);
            GUI.color = originalColor;
            
            // ハードウェア情報
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Hardware Information:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"CPU: {environmentReport.hardware.cpu.name} ({environmentReport.hardware.cpu.cores} cores)");
            EditorGUILayout.LabelField($"RAM: {environmentReport.hardware.memory.totalRAM / (1024 * 1024 * 1024):F1} GB");
            EditorGUILayout.LabelField($"GPU: {environmentReport.hardware.gpu.name}");
            
            // 重要な結果のみ表示
            var criticalResults = environmentReport.results.Where(r => !r.isPassed || r.severity == SystemRequirementChecker.RequirementSeverity.Required).ToList();
            if (criticalResults.Any())
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Issues:", EditorStyles.boldLabel);
                foreach (var result in criticalResults.Take(5)) // 最大5件表示
                {
                    EditorGUILayout.LabelField($"• {result.checkName}: {result.message}", statusStyle);
                }
            }
        }
        
        /// <summary>
        /// プロジェクト生成の開始
        /// </summary>
        /// <summary>
        /// プロジェクト生成の開始
        /// </summary>
        private async void StartProjectGeneration()
        {
            UnityEngine.Debug.Log($"[SetupWizard] Starting project generation for {wizardConfig.selectedGenre} project with modules: {string.Join(", ", wizardConfig.selectedModules)}");
            
            var engine = new ProjectGenerationEngine(wizardConfig, (progress, status) => 
            {
                stepStates[WizardStep.ProjectGeneration].progressPercent = progress;
                stepStates[WizardStep.ProjectGeneration].statusMessage = status;
                Repaint();
            });

            bool success = await engine.GenerateProjectAsync();

            if (success)
            {
                stepStates[WizardStep.ProjectGeneration].isCompleted = true;
                GoToNextStep();
                totalSetupTime = DateTime.Now - setupStartTime;
                isSetupComplete = true;
                UnityEngine.Debug.Log($"[SetupWizard] Project generation completed in {totalSetupTime:mm\\:ss}");
            }
            else
            {
                EditorUtility.DisplayDialog("Project Generation Failed", "Project generation failed. Check the console for more details.", "OK");
            }
        }
        
        /// <summary>
        /// 総合進捗の計算
        /// </summary>
                /// <summary>
        /// 総合進捗の計算
        /// </summary>
        private float CalculateTotalProgress()
        {
            // stepStatesが初期化されていない場合は0を返す
            if (stepStates == null || stepStates.Count == 0)
                return 0f;
                
            float completedSteps = stepStates.Values.Count(s => s.isCompleted);
            return completedSteps / stepStates.Count;
        }
        
        /// <summary>
        /// ステップ表示名の取得
        /// </summary>
        private string GetStepDisplayName(WizardStep step)
        {
            switch (step)
            {
                case WizardStep.EnvironmentCheck: return "Environment";
                case WizardStep.GenreSelection: return "Genre";
                case WizardStep.ModuleSelection: return "Modules";
                case WizardStep.ProjectGeneration: return "Generate";
                case WizardStep.Verification: return "Complete";
                default: return step.ToString();
            }
        }
        
        /// <summary>
        /// ジャンル表示名の取得
        /// </summary>
        private string GetGenreDisplayName(ProjectGenre genre)
        {
            switch (genre)
            {
                case ProjectGenre.FPS: return "First Person Shooter";
                case ProjectGenre.TPS: return "Third Person Shooter";
                case ProjectGenre.Platformer: return "3D Platformer";
                case ProjectGenre.Stealth: return "Stealth Action";
                case ProjectGenre.Adventure: return "Adventure Game";
                case ProjectGenre.Strategy: return "Real-time Strategy";
                default: return genre.ToString();
            }
        }
        
        /// <summary>
        /// ジャンル説明の取得
        /// </summary>
        private string GetGenreDescription(ProjectGenre genre)
        {
            switch (genre)
            {
                case ProjectGenre.FPS: return "一人称視点シューティングゲーム（15分バトルシステム）";
                case ProjectGenre.TPS: return "三人称視点シューティングゲーム（15分アクション）";
                case ProjectGenre.Platformer: return "3Dプラットフォーマーゲーム（15分ステージクリア）";
                case ProjectGenre.Stealth: return "ステルスアクションゲーム（15分潜入ミッション）";
                case ProjectGenre.Adventure: return "アドベンチャーゲーム（15分探索システム）";
                case ProjectGenre.Strategy: return "リアルタイム戦略ゲーム（15分戦略バトル）";
                default: return "";
            }
        }
        
        /// <summary>
        /// ステップナビゲーション制御
        /// </summary>
        private bool CanNavigateToStep(WizardStep step)
        {
            // Environment Checkは常にアクセス可能
            if (step == WizardStep.EnvironmentCheck) return true;
            
            // 前のステップが完了している場合のみアクセス可能
            var stepIndex = (int)step;
            if (stepIndex > 0)
            {
                var previousStep = (WizardStep)(stepIndex - 1);
                return stepStates[previousStep].isCompleted;
            }
            
            return true;
        }
        
        private void NavigateToStep(WizardStep step)
        {
            if (!CanNavigateToStep(step)) return;
            
            // 現在のステップを更新
            foreach (var state in stepStates.Values)
                state.isCurrentStep = false;
                
            stepStates[step].isCurrentStep = true;
            currentStep = step;
        }
        
        private bool CanGoToPreviousStep()
        {
            return (int)currentStep > 0;
        }
        
        private void GoToPreviousStep()
        {
            if (CanGoToPreviousStep())
            {
                var previousStep = (WizardStep)((int)currentStep - 1);
                NavigateToStep(previousStep);
            }
        }
        
        private bool CanGoToNextStep()
        {
            // stepStatesが初期化されていない場合はfalseを返す
            if (stepStates == null)
                return false;
                
            return stepStates[currentStep].isCompleted && (int)currentStep < stepStates.Count - 1;
        }
        
        private void GoToNextStep()
        {
            if (CanGoToNextStep())
            {
                // ジャンル選択からモジュール選択へ移行する際に、推奨モジュールをデフォルトで選択状態にする
                if (currentStep == WizardStep.GenreSelection)
                {
                    var genre = genreManager.GetGenre(wizardConfig.selectedGenre);
                    if (genre != null)
                    {
                        wizardConfig.selectedModules.Clear();
                        // 必須モジュールは常に含まれる
                        wizardConfig.selectedModules.AddRange(genre.RequiredModules);
                        // 推奨モジュールをデフォルトで追加
                        wizardConfig.selectedModules.AddRange(genre.RecommendedModules);
                        // 重複を削除
                        wizardConfig.selectedModules = wizardConfig.selectedModules.Distinct().ToList();
                    }
                }

                var nextStep = (WizardStep)((int)currentStep + 1);
                NavigateToStep(nextStep);
            }
        }
        
        #endregion
    }
}
