# Unity 6 ベーステンプレート追加実装 - システム設計書

## 文書情報

| 項目 | 詳細 |
|------|------|
| **文書名** | Unity 6 ベーステンプレート追加実装システム設計書 |
| **プロジェクト** | URP3D_Base01 |
| **作成日** | 2025年9月7日 |
| **版数** | v1.0 |
| **ステータス** | 設計確定 |
| **対象読者** | 開発チーム、アーキテクト、シニア開発者 |
| **関連文書** | BaseTemplate_Enhancement_Specification.md |

---

## 1. アーキテクチャ設計

### 1.1 システム全体アーキテクチャ

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                    URP3D_Base01 Enhanced Architecture                       │
├─────────────────────────────────────────────────────────────────────────────┤
│  Presentation Layer (UI & Interaction)                                     │
│  ├── Setup Wizard UI          ├── Settings UI           ├── Template UI   │
│  ├── Genre Selection UI       ├── Localization UI       ├── Debug Tools   │
│  └── Asset Integration UI     └── Performance Monitor   └── Community UI  │
├─────────────────────────────────────────────────────────────────────────────┤
│  Application Layer (Business Logic)                                        │
│  ├── Setup Wizard Controller     ├── Settings Manager    ├── Template Mgr │
│  ├── Genre Template Manager      ├── Localization Mgr    ├── Asset Mgr    │
│  ├── Save/Load Manager           ├── Performance Profiler├── Build Pipeline│
│  └── Plugin Architecture         └── Memory Manager      └── Validation Sys│
├─────────────────────────────────────────────────────────────────────────────┤
│  Domain Layer (Core Business Rules) - Existing Systems                     │
│  ├── Event-Driven Architecture   ├── Command Pattern     ├── ObjectPool   │
│  ├── GameEvent System            ├── Audio System        ├── State Machine │
│  └── ScriptableObject Foundation └── Detection Systems   └── Core Data     │
├─────────────────────────────────────────────────────────────────────────────┤
│  Infrastructure Layer (Technical Foundation)                               │
│  ├── Unity Engine 6              ├── Universal RP        ├── .NET Std 2.1 │
│  ├── File System                 ├── Network Services    ├── Platform APIs │
│  ├── Input System                ├── Analytics Services  ├── Cloud Services│
│  └── Third-party Integrations    └── Build System        └── CI/CD Pipeline│
└─────────────────────────────────────────────────────────────────────────────┘
```

### 1.2 アーキテクチャ原則

#### 1.2.1 継続原則（既存システムの維持）
```csharp
// 既存のイベント駆動アーキテクチャ継続
public abstract class EnhancedSystemBase : MonoBehaviour, IGameEventListener
{
    // 全新システムは既存のGameEventシステムを活用
    protected void RaiseSystemEvent<T>(GameEvent<T> gameEvent, T data)
    {
        gameEvent?.Raise(data);
    }
    
    // 既存のCommandPatternとの統合
    protected void ExecuteCommand(ICommand command)
    {
        CommandInvoker.Instance.ExecuteCommand(command);
    }
    
    // ObjectPool最適化の継続
    protected T GetPooledObject<T>() where T : class, IResettable
    {
        return ObjectPool<T>.Instance.Get();
    }
}
```

#### 1.2.2 拡張原則（段階的機能追加）
```csharp
// 機能別モジュール設計による拡張性確保
public interface ISystemModule
{
    string ModuleName { get; }
    Version ModuleVersion { get; }
    bool IsEnabled { get; set; }
    
    UniTask<bool> InitializeAsync();
    UniTask ShutdownAsync();
    void OnApplicationPause(bool pauseStatus);
    void OnApplicationFocus(bool hasFocus);
}

// 各新機能はモジュールとして実装
public class SaveLoadModule : EnhancedSystemBase, ISystemModule
{
    public string ModuleName => "Save/Load System";
    public Version ModuleVersion => new Version(1, 0, 0);
    public bool IsEnabled { get; set; } = true;
    
    public async UniTask<bool> InitializeAsync()
    {
        // 既存システムとの統合初期化
        await RegisterWithExistingSystems();
        return true;
    }
}
```

### 1.3 データフロー設計

#### 1.3.1 イベント駆動データフロー
```csharp
// 新システムのイベントデータ構造
[System.Serializable]
public class SystemEventData : EventArgs
{
    public string systemName;
    public string eventType;
    public float timestamp;
    public object payload;
    public EventPriority priority;
}

// イベントフロー制御
public class EnhancedEventSystem : MonoBehaviour
{
    private static Dictionary<Type, List<IGameEventListener>> eventListeners;
    private static Queue<SystemEventData> eventQueue;
    
    // 既存GameEventシステムとの統合
    public static void RegisterEnhancedListener<T>(IGameEventListener<T> listener)
    {
        // 既存システムへの登録 + 拡張機能追加
    }
}
```

---

## 2. Phase A: 新規開発者対応システム設計

### 2.1 Interactive Setup Wizard System

#### 2.1.1 アーキテクチャ概要
```
Setup Wizard System Architecture:
├── WizardController.cs - ウィザード制御中枢
│   ├── Step管理（Progress/Navigation）
│   ├── 検証エンジン（Environment/Dependencies）
│   ├── 自動設定エンジン（Configuration/Optimization）
│   └── レポート生成（Setup Report/Error Logging）
├── WizardSteps/ - 各ステップ実装
│   ├── EnvironmentCheckStep.cs - 環境診断
│   ├── GenreSelectionStep.cs - ジャンル選択
│   ├── ModuleSelectionStep.cs - 機能モジュール選択
│   ├── ConfigurationStep.cs - 自動設定実行
│   └── CompletionStep.cs - 完了・検証
├── WizardData/ - 設定データ管理
│   ├── WizardSettingsSO.cs - ウィザード設定
│   ├── GenreTemplateSO.cs - ジャンル別テンプレート
│   ├── ModuleConfigSO.cs - モジュール設定
│   └── EnvironmentDataSO.cs - 環境情報
└── WizardUI/ - UI制御
    ├── WizardWindow.cs - メインウィンドウ
    ├── StepNavigator.cs - ステップナビゲーション
    ├── ProgressIndicator.cs - 進捗表示
    └── StatusReporter.cs - 状態・エラー表示
```

#### 2.1.2 Core Classes設計

##### WizardController.cs
```csharp
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Commands;

namespace asterivo.Unity60.Enhancement.Setup
{
    public class WizardController : EnhancedSystemBase
    {
        [Header("Wizard Configuration")]
        [SerializeField] private WizardSettingsSO wizardSettings;
        [SerializeField] private List<WizardStepBase> wizardSteps;
        
        [Header("Events")]
        [SerializeField] private GameEvent<WizardEventData> onStepChanged;
        [SerializeField] private GameEvent<WizardEventData> onWizardCompleted;
        [SerializeField] private GameEvent<WizardEventData> onWizardCancelled;
        
        // State Management
        private int currentStepIndex = 0;
        private WizardStepData currentStepData;
        private Dictionary<string, object> wizardContext;
        
        // Async Operations
        private CancellationTokenSource cancellationTokenSource;
        
        #region Wizard Flow Control
        
        public async UniTask StartWizardAsync()
        {
            try
            {
                InitializeWizard();
                await ExecuteWizardSteps();
                await CompleteWizard();
            }
            catch (Exception ex)
            {
                HandleWizardError(ex);
            }
        }
        
        private async UniTask ExecuteWizardSteps()
        {
            for (currentStepIndex = 0; currentStepIndex < wizardSteps.Count; currentStepIndex++)
            {
                var step = wizardSteps[currentStepIndex];
                
                // Step前処理
                await PrepareStep(step);
                
                // Step実行
                var result = await ExecuteStep(step, cancellationTokenSource.Token);
                
                // 結果検証
                if (!ValidateStepResult(result))
                {
                    await HandleStepFailure(step, result);
                    return;
                }
                
                // Step後処理
                await CompleteStep(step, result);
                
                // 進捗通知
                RaiseSystemEvent(onStepChanged, new WizardEventData
                {
                    stepIndex = currentStepIndex,
                    stepName = step.StepName,
                    stepData = result,
                    timestamp = Time.time
                });
            }
        }
        
        private async UniTask<WizardStepResult> ExecuteStep(WizardStepBase step, CancellationToken cancellationToken)
        {
            try
            {
                // 既存のCommandPatternを活用したStep実行
                var stepCommand = new ExecuteWizardStepCommand(step, wizardContext);
                var commandResult = await ExecuteCommandAsync(stepCommand, cancellationToken);
                
                return commandResult.Data as WizardStepResult;
            }
            catch (OperationCanceledException)
            {
                return WizardStepResult.Cancelled();
            }
            catch (Exception ex)
            {
                return WizardStepResult.Failed(ex);
            }
        }
        
        #endregion
        
        #region Environment Validation
        
        private async UniTask<EnvironmentCheckResult> ValidateEnvironment()
        {
            var result = new EnvironmentCheckResult();
            
            // Unity Version Check
            result.UnityVersion = await CheckUnityVersion();
            
            // URP Setup Check  
            result.URPSetup = await CheckURPConfiguration();
            
            // Package Dependencies Check
            result.PackageDependencies = await CheckPackageDependencies();
            
            // Development Tools Check
            result.DevelopmentTools = await CheckDevelopmentTools();
            
            return result;
        }
        
        private async UniTask<bool> CheckUnityVersion()
        {
            var requiredVersion = new Version(6, 0, 0);
            var currentVersion = new Version(Application.unityVersion);
            
            return currentVersion >= requiredVersion;
        }
        
        #endregion
        
        #region Auto Configuration
        
        private async UniTask ApplyAutoConfiguration(GenreSelectionData genreData, ModuleSelectionData moduleData)
        {
            // ジャンル別最適化設定
            await ApplyGenreOptimizations(genreData);
            
            // モジュール別設定
            await ApplyModuleConfigurations(moduleData);
            
            // プラットフォーム別最適化
            await ApplyPlatformOptimizations();
            
            // 既存システム統合設定
            await IntegrateWithExistingSystems();
        }
        
        private async UniTask ApplyGenreOptimizations(GenreSelectionData genreData)
        {
            var genreTemplate = wizardSettings.GetGenreTemplate(genreData.selectedGenre);
            
            // Cinemachine設定
            await ApplyCinemachineSettings(genreTemplate.cameraSettings);
            
            // Input System設定
            await ApplyInputSettings(genreTemplate.inputSettings);
            
            // Audio設定
            await ApplyAudioSettings(genreTemplate.audioSettings);
            
            // UI設定
            await ApplyUISettings(genreTemplate.uiSettings);
        }
        
        #endregion
    }
    
    // Supporting Data Structures
    [System.Serializable]
    public class WizardEventData
    {
        public int stepIndex;
        public string stepName;
        public WizardStepResult stepData;
        public float timestamp;
    }
    
    [System.Serializable]
    public class WizardStepResult
    {
        public bool isSuccess;
        public string message;
        public Dictionary<string, object> data;
        public Exception error;
        
        public static WizardStepResult Success(string message = "", Dictionary<string, object> data = null)
        {
            return new WizardStepResult 
            { 
                isSuccess = true, 
                message = message, 
                data = data ?? new Dictionary<string, object>() 
            };
        }
        
        public static WizardStepResult Failed(Exception error, string message = "")
        {
            return new WizardStepResult 
            { 
                isSuccess = false, 
                message = message, 
                error = error 
            };
        }
        
        public static WizardStepResult Cancelled()
        {
            return new WizardStepResult 
            { 
                isSuccess = false, 
                message = "Operation cancelled by user" 
            };
        }
    }
}
```

##### WizardStepBase.cs (Abstract Base Class)
```csharp
public abstract class WizardStepBase : ScriptableObject
{
    [Header("Step Configuration")]
    public string stepName;
    public string stepDescription;
    public Sprite stepIcon;
    public float estimatedDuration;
    
    [Header("Dependencies")]
    public List<string> requiredSteps;
    public List<string> optionalSteps;
    
    // Abstract Methods
    public abstract UniTask<WizardStepResult> ExecuteAsync(Dictionary<string, object> context, CancellationToken cancellationToken);
    public abstract bool ValidatePreConditions(Dictionary<string, object> context);
    public abstract void CleanupStep();
    
    // Virtual Methods (Overridable)
    public virtual bool CanSkip => false;
    public virtual bool RequireUserInteraction => true;
    public virtual string StepName => stepName;
    
    // Utility Methods
    protected void LogStepProgress(string message, float progress = -1f)
    {
        Debug.Log($"[{StepName}] {message}");
        if (progress >= 0f)
        {
            EditorUtility.DisplayProgressBar(StepName, message, progress);
        }
    }
}
```

#### 2.1.3 Genre Selection System設計

##### GenreSelectionStep.cs
```csharp
[CreateAssetMenu(menuName = "asterivo/Wizard/Steps/Genre Selection", fileName = "GenreSelectionStep")]
public class GenreSelectionStep : WizardStepBase
{
    [Header("Genre Templates")]
    [SerializeField] private GenreTemplateSO[] availableGenres;
    
    [Header("UI Configuration")]
    [SerializeField] private GameObject genreSelectionPrefab;
    
    public override async UniTask<WizardStepResult> ExecuteAsync(Dictionary<string, object> context, CancellationToken cancellationToken)
    {
        try
        {
            LogStepProgress("Initializing genre selection...", 0.1f);
            
            // UI表示
            var selectionUI = await ShowGenreSelectionUI();
            
            LogStepProgress("Waiting for user selection...", 0.3f);
            
            // ユーザー選択待機
            var selectedGenre = await WaitForUserSelection(selectionUI, cancellationToken);
            
            LogStepProgress($"Selected genre: {selectedGenre.genreName}", 0.7f);
            
            // 選択結果をcontextに保存
            context["selectedGenre"] = selectedGenre;
            context["genreTemplate"] = selectedGenre.genreTemplate;
            
            LogStepProgress("Genre selection completed", 1.0f);
            
            return WizardStepResult.Success("Genre selection completed successfully", 
                new Dictionary<string, object> { ["genre"] = selectedGenre });
        }
        catch (OperationCanceledException)
        {
            return WizardStepResult.Cancelled();
        }
        catch (Exception ex)
        {
            return WizardStepResult.Failed(ex, "Failed to complete genre selection");
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }
    
    public override bool ValidatePreConditions(Dictionary<string, object> context)
    {
        // 環境チェック結果の確認
        if (!context.ContainsKey("environmentCheck"))
            return false;
            
        var envCheck = context["environmentCheck"] as EnvironmentCheckResult;
        return envCheck?.IsEnvironmentValid() ?? false;
    }
    
    private async UniTask<GenreSelectionUI> ShowGenreSelectionUI()
    {
        var uiObject = Instantiate(genreSelectionPrefab);
        var selectionUI = uiObject.GetComponent<GenreSelectionUI>();
        
        await selectionUI.InitializeAsync(availableGenres);
        
        return selectionUI;
    }
    
    private async UniTask<GenreData> WaitForUserSelection(GenreSelectionUI selectionUI, CancellationToken cancellationToken)
    {
        var tcs = new UniTaskCompletionSource<GenreData>();
        
        selectionUI.OnGenreSelected += (genre) => 
        {
            if (!tcs.Task.IsCompleted)
                tcs.TrySetResult(genre);
        };
        
        cancellationToken.Register(() => 
        {
            if (!tcs.Task.IsCompleted)
                tcs.TrySetCanceled();
        });
        
        return await tcs.Task;
    }
}
```

### 2.2 Game Genre Templates System

#### 2.2.1 Template Architecture
```
Genre Template System:
├── GenreTemplateManager.cs - テンプレート管理中枢
├── Templates/ - ジャンル別テンプレート
│   ├── FPSTemplate/ - 一人称シューティング
│   │   ├── FPSTemplateSO.cs - 設定データ
│   │   ├── FPS_Scene_Template.unity - シーンテンプレート
│   │   ├── FPS_Player_Controller.prefab - プレイヤー制御
│   │   ├── FPS_Camera_Rig.prefab - カメラ設定
│   │   ├── FPS_UI_Canvas.prefab - UI設定
│   │   └── FPS_Tutorial_Steps.asset - チュートリアル
│   ├── TPSTemplate/ - 三人称シューティング
│   ├── PlatformerTemplate/ - プラットフォーマー
│   ├── StealthTemplate/ - ステルスアクション
│   ├── AdventureTemplate/ - アドベンチャー
│   └── StrategyTemplate/ - ストラテジー
├── TemplateComponents/ - 共通コンポーネント
│   ├── TemplatePlayerController.cs - 汎用プレイヤー制御
│   ├── TemplateCameraController.cs - 汎用カメラ制御
│   ├── TemplateInputHandler.cs - 汎用入力処理
│   └── TemplateUIManager.cs - 汎用UI管理
└── TemplateAssets/ - 共通アセット
    ├── Materials/ - 基本マテリアル
    ├── Textures/ - 基本テクスチャ
    ├── Audio/ - 基本オーディオ
    └── Animations/ - 基本アニメーション
```

#### 2.2.2 Core Template Classes

##### GenreTemplateSO.cs (ScriptableObject Base)
```csharp
using asterivo.Unity60.Core.Data;

namespace asterivo.Unity60.Enhancement.Templates
{
    [CreateAssetMenu(menuName = "asterivo/Templates/Genre Template", fileName = "GenreTemplate")]
    public class GenreTemplateSO : ScriptableObject, IValidatable
    {
        [Header("Basic Information")]
        public string genreName;
        public string genreDescription;
        public Sprite genreIcon;
        public GameGenre genreType;
        
        [Header("Scene Configuration")]
        public SceneAsset mainSceneTemplate;
        public List<SceneAsset> additionalScenes;
        
        [Header("Prefab Configuration")]
        public GameObject playerControllerPrefab;
        public GameObject cameraRigPrefab;
        public GameObject uiCanvasPrefab;
        public GameObject gameManagerPrefab;
        
        [Header("Settings Configuration")]
        public CinemachineSettings cameraSettings;
        public InputSystemSettings inputSettings;
        public AudioSystemSettings audioSettings;
        public UILayoutSettings uiSettings;
        
        [Header("Tutorial Configuration")]
        public TutorialStepSO[] tutorialSteps;
        public bool enableTutorial = true;
        
        [Header("Asset Dependencies")]
        public List<PackageReference> requiredPackages;
        public List<AssetReference> requiredAssets;
        
        #region IValidatable Implementation
        
        public ValidationResult Validate()
        {
            var result = new ValidationResult();
            
            // 必須フィールド検証
            if (string.IsNullOrEmpty(genreName))
                result.AddError("Genre name is required");
                
            if (mainSceneTemplate == null)
                result.AddError("Main scene template is required");
                
            // Prefab検証
            if (playerControllerPrefab == null)
                result.AddWarning("Player controller prefab is not set");
                
            // 依存関係検証
            ValidatePackageDependencies(result);
            ValidateAssetDependencies(result);
            
            return result;
        }
        
        private void ValidatePackageDependencies(ValidationResult result)
        {
            foreach (var package in requiredPackages)
            {
                if (!PackageManager.IsPackageInstalled(package.packageName))
                {
                    result.AddWarning($"Required package not installed: {package.packageName}");
                }
            }
        }
        
        #endregion
        
        #region Template Application
        
        public async UniTask<TemplateApplicationResult> ApplyToCurrentProject(IProgress<float> progress = null)
        {
            try
            {
                var result = new TemplateApplicationResult();
                
                progress?.Report(0.1f);
                
                // Package Dependencies
                await InstallRequiredPackages(result, progress);
                
                progress?.Report(0.3f);
                
                // Scene Setup
                await CreateScenes(result, progress);
                
                progress?.Report(0.5f);
                
                // Prefab Instantiation
                await InstantiatePrefabs(result, progress);
                
                progress?.Report(0.7f);
                
                // Settings Configuration
                await ApplySettings(result, progress);
                
                progress?.Report(0.9f);
                
                // Tutorial Setup
                if (enableTutorial)
                {
                    await SetupTutorial(result, progress);
                }
                
                progress?.Report(1.0f);
                
                result.isSuccess = true;
                result.message = $"Successfully applied {genreName} template";
                
                return result;
            }
            catch (Exception ex)
            {
                return new TemplateApplicationResult
                {
                    isSuccess = false,
                    message = $"Failed to apply template: {ex.Message}",
                    error = ex
                };
            }
        }
        
        private async UniTask InstallRequiredPackages(TemplateApplicationResult result, IProgress<float> progress)
        {
            for (int i = 0; i < requiredPackages.Count; i++)
            {
                var package = requiredPackages[i];
                
                if (!PackageManager.IsPackageInstalled(package.packageName))
                {
                    await PackageManager.InstallPackageAsync(package.packageName);
                    result.installedPackages.Add(package.packageName);
                }
                
                progress?.Report(0.1f + (0.2f * (i + 1) / requiredPackages.Count));
            }
        }
        
        private async UniTask CreateScenes(TemplateApplicationResult result, IProgress<float> progress)
        {
            // Main Scene
            if (mainSceneTemplate != null)
            {
                var newScene = await SceneTemplateUtility.CreateSceneFromTemplate(mainSceneTemplate, $"{genreName}_Main");
                result.createdScenes.Add(newScene.path);
            }
            
            // Additional Scenes
            for (int i = 0; i < additionalScenes.Count; i++)
            {
                var sceneTemplate = additionalScenes[i];
                var newScene = await SceneTemplateUtility.CreateSceneFromTemplate(sceneTemplate, $"{genreName}_Scene_{i + 1}");
                result.createdScenes.Add(newScene.path);
                
                progress?.Report(0.3f + (0.2f * (i + 1) / additionalScenes.Count));
            }
        }
        
        private async UniTask InstantiatePrefabs(TemplateApplicationResult result, IProgress<float> progress)
        {
            var activeScene = SceneManager.GetActiveScene();
            
            // Player Controller
            if (playerControllerPrefab != null)
            {
                var playerInstance = PrefabUtility.InstantiatePrefab(playerControllerPrefab) as GameObject;
                SceneManager.MoveGameObjectToScene(playerInstance, activeScene);
                result.instantiatedPrefabs.Add(playerInstance);
            }
            
            progress?.Report(0.5f + 0.05f);
            
            // Camera Rig
            if (cameraRigPrefab != null)
            {
                var cameraInstance = PrefabUtility.InstantiatePrefab(cameraRigPrefab) as GameObject;
                SceneManager.MoveGameObjectToScene(cameraInstance, activeScene);
                result.instantiatedPrefabs.Add(cameraInstance);
            }
            
            progress?.Report(0.5f + 0.1f);
            
            // UI Canvas
            if (uiCanvasPrefab != null)
            {
                var uiInstance = PrefabUtility.InstantiatePrefab(uiCanvasPrefab) as GameObject;
                SceneManager.MoveGameObjectToScene(uiInstance, activeScene);
                result.instantiatedPrefabs.Add(uiInstance);
            }
            
            progress?.Report(0.5f + 0.15f);
            
            // Game Manager
            if (gameManagerPrefab != null)
            {
                var managerInstance = PrefabUtility.InstantiatePrefab(gameManagerPrefab) as GameObject;
                SceneManager.MoveGameObjectToScene(managerInstance, activeScene);
                result.instantiatedPrefabs.Add(managerInstance);
            }
            
            progress?.Report(0.5f + 0.2f);
        }
        
        #endregion
    }
    
    public enum GameGenre
    {
        FPS,           // First Person Shooter
        TPS,           // Third Person Shooter  
        Platformer,    // Platform Game
        Stealth,       // Stealth Action
        Adventure,     // Adventure Game
        Strategy       // Strategy Game
    }
    
    [System.Serializable]
    public class TemplateApplicationResult
    {
        public bool isSuccess;
        public string message;
        public Exception error;
        public List<string> installedPackages = new List<string>();
        public List<string> createdScenes = new List<string>();
        public List<GameObject> instantiatedPrefabs = new List<GameObject>();
        public List<string> appliedSettings = new List<string>();
        public float executionTime;
    }
}
```

### 2.3 Asset Store Integration System

#### 2.3.1 Integration Architecture
```
Asset Store Integration System:
├── AssetIntegrationManager.cs - 統合管理中枢
├── PopularAssets/ - 人気アセット統合データ
│   ├── PopularAssetDefinitionSO.cs - アセット定義
│   ├── IntegrationGuideSO.cs - 統合手順書
│   └── CompatibilityRulesSO.cs - 互換性ルール
├── CompatibilityChecker/ - 互換性チェック
│   ├── DependencyAnalyzer.cs - 依存関係分析
│   ├── ConflictDetector.cs - 競合検出
│   └── ResolutionSuggester.cs - 解決策提案
├── IntegrationTools/ - 統合支援ツール
│   ├── AssetImporter.cs - インポート支援
│   ├── SettingsOptimizer.cs - 設定最適化
│   └── ValidationRunner.cs - 動作確認
└── RecommendationEngine/ - 推奨システム
    ├── GenreBasedRecommender.cs - ジャンル別推奨
    ├── PopularityAnalyzer.cs - 人気度分析
    └── UserPreferenceTracker.cs - ユーザー嗜好追跡
```

---

## 3. Phase B: 高度なゲーム機能設計

### 3.1 Advanced Save/Load System

#### 3.1.1 Data Architecture
```csharp
// ScriptableObjectベースのセーブシステム
namespace asterivo.Unity60.Enhancement.SaveSystem
{
    [CreateAssetMenu(menuName = "asterivo/Save System/Save Profile", fileName = "SaveProfile")]
    public class SaveProfileSO : ScriptableObject, ISerializationCallbackReceiver
    {
        [Header("Profile Information")]
        public string profileName;
        public DateTime createdDate;
        public DateTime lastSavedDate;
        public float totalPlayTime;
        public string gameVersion;
        
        [Header("Game State")]
        [SerializeReference] private List<ISaveableData> saveableData;
        [SerializeField] private GameStateSO gameState;
        [SerializeField] private SettingsProfileSO settingsProfile;
        
        [Header("Metadata")]
        public SaveMetadata metadata;
        public byte[] checksum;
        
        #region Core Save/Load Methods
        
        public async UniTask<SaveResult> SaveAsync(IProgress<float> progress = null)
        {
            try
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                
                progress?.Report(0.1f);
                
                // 1. データ収集
                await CollectSaveableData(progress);
                
                progress?.Report(0.3f);
                
                // 2. シリアライゼーション
                var serializedData = await SerializeDataAsync(progress);
                
                progress?.Report(0.6f);
                
                // 3. 暗号化
                var encryptedData = await EncryptDataAsync(serializedData, progress);
                
                progress?.Report(0.8f);
                
                // 4. ファイル書き込み
                await WriteToFileAsync(encryptedData, progress);
                
                progress?.Report(1.0f);
                
                stopwatch.Stop();
                
                return new SaveResult
                {
                    isSuccess = true,
                    message = "Save completed successfully",
                    executionTime = stopwatch.Elapsed,
                    dataSize = encryptedData.Length
                };
            }
            catch (Exception ex)
            {
                return new SaveResult
                {
                    isSuccess = false,
                    message = $"Save failed: {ex.Message}",
                    error = ex
                };
            }
        }
        
        public async UniTask<LoadResult> LoadAsync(IProgress<float> progress = null)
        {
            try
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                
                progress?.Report(0.1f);
                
                // 1. ファイル読み込み
                var fileData = await ReadFromFileAsync(progress);
                
                progress?.Report(0.3f);
                
                // 2. 復号化
                var decryptedData = await DecryptDataAsync(fileData, progress);
                
                progress?.Report(0.5f);
                
                // 3. 整合性チェック
                if (!await ValidateDataIntegrity(decryptedData, progress))
                {
                    throw new InvalidDataException("Save data is corrupted");
                }
                
                progress?.Report(0.7f);
                
                // 4. デシリアライゼーション
                await DeserializeDataAsync(decryptedData, progress);
                
                progress?.Report(0.9f);
                
                // 5. データ適用
                await ApplyLoadedData(progress);
                
                progress?.Report(1.0f);
                
                stopwatch.Stop();
                
                return new LoadResult
                {
                    isSuccess = true,
                    message = "Load completed successfully",
                    executionTime = stopwatch.Elapsed,
                    loadedDataSize = fileData.Length
                };
            }
            catch (Exception ex)
            {
                return new LoadResult
                {
                    isSuccess = false,
                    message = $"Load failed: {ex.Message}",
                    error = ex
                };
            }
        }
        
        #endregion
        
        #region Data Collection
        
        private async UniTask CollectSaveableData(IProgress<float> progress)
        {
            saveableData.Clear();
            
            // 既存のGameEventシステムを活用してデータ収集
            var collectEvent = ScriptableObject.CreateInstance<GameEvent<SaveDataCollectionRequest>>();
            var request = new SaveDataCollectionRequest();
            
            collectEvent.Raise(request);
            
            // 非同期でデータ収集完了を待機
            while (!request.IsCompleted)
            {
                await UniTask.Yield();
            }
            
            saveableData.AddRange(request.CollectedData);
            
            // 更新日時記録
            lastSavedDate = DateTime.Now;
            
            progress?.Report(0.3f);
        }
        
        #endregion
        
        #region Serialization
        
        private async UniTask<byte[]> SerializeDataAsync(IProgress<float> progress)
        {
            using (var memoryStream = new MemoryStream())
            {
                // Binary Formatter使用（高速）
                var formatter = new BinaryFormatter();
                
                // Header情報
                var header = new SaveHeader
                {
                    version = Application.version,
                    timestamp = DateTime.Now,
                    dataCount = saveableData.Count
                };
                
                formatter.Serialize(memoryStream, header);
                
                // メインデータ
                for (int i = 0; i < saveableData.Count; i++)
                {
                    formatter.Serialize(memoryStream, saveableData[i]);
                    progress?.Report(0.3f + (0.3f * (i + 1) / saveableData.Count));
                }
                
                return memoryStream.ToArray();
            }
        }
        
        private async UniTask DeserializeDataAsync(byte[] data, IProgress<float> progress)
        {
            using (var memoryStream = new MemoryStream(data))
            {
                var formatter = new BinaryFormatter();
                
                // Header読み込み
                var header = (SaveHeader)formatter.Deserialize(memoryStream);
                
                // バージョン互換性チェック
                if (!IsVersionCompatible(header.version))
                {
                    await HandleVersionMismatch(header);
                }
                
                // メインデータ読み込み
                saveableData.Clear();
                for (int i = 0; i < header.dataCount; i++)
                {
                    var data_item = (ISaveableData)formatter.Deserialize(memoryStream);
                    saveableData.Add(data_item);
                    progress?.Report(0.7f + (0.2f * (i + 1) / header.dataCount));
                }
            }
        }
        
        #endregion
        
        #region Encryption/Decryption
        
        private async UniTask<byte[]> EncryptDataAsync(byte[] data, IProgress<float> progress)
        {
            if (!metadata.useEncryption) return data;
            
            using (var aes = Aes.Create())
            {
                aes.KeySize = 256;
                aes.Key = GetEncryptionKey();
                aes.IV = GetInitializationVector();
                
                using (var encryptor = aes.CreateEncryptor())
                using (var memoryStream = new MemoryStream())
                using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                {
                    await cryptoStream.WriteAsync(data, 0, data.Length);
                    cryptoStream.FlushFinalBlock();
                    
                    progress?.Report(0.8f);
                    
                    return memoryStream.ToArray();
                }
            }
        }
        
        #endregion
    }
    
    // 既存システムとの統合インターフェース
    public interface ISaveableData
    {
        string DataId { get; }
        int DataVersion { get; }
        byte[] SerializeData();
        void DeserializeData(byte[] data);
        bool ValidateData();
    }
    
    // ゲーム既存オブジェクトの拡張
    public abstract class SaveableGameObject : MonoBehaviour, ISaveableData
    {
        [SerializeField] protected string dataId;
        [SerializeField] protected int dataVersion = 1;
        
        public string DataId => dataId;
        public int DataVersion => dataVersion;
        
        public abstract byte[] SerializeData();
        public abstract void DeserializeData(byte[] data);
        public abstract bool ValidateData();
        
        // 既存のGameEventシステムとの統合
        protected virtual void OnEnable()
        {
            // SaveDataCollectionRequestイベントをリッスン
            RegisterForSaveEvents();
        }
        
        protected virtual void OnDisable()
        {
            UnregisterFromSaveEvents();
        }
    }
}
```

### 3.2 Comprehensive Settings System

#### 3.2.1 Settings Architecture
```csharp
namespace asterivo.Unity60.Enhancement.Settings
{
    [CreateAssetMenu(menuName = "asterivo/Settings/Settings Manager Config", fileName = "SettingsManagerConfig")]
    public class SettingsManagerConfigSO : ScriptableObject
    {
        [Header("Settings Categories")]
        public List<SettingsCategorySO> categories;
        
        [Header("Presets")]
        public List<SettingsPresetSO> presets;
        
        [Header("Platform Overrides")]
        public List<PlatformSettingsOverride> platformOverrides;
    }
    
    public class SettingsManager : EnhancedSystemBase, ISystemModule
    {
        [Header("Configuration")]
        [SerializeField] private SettingsManagerConfigSO config;
        
        [Header("Events")]
        [SerializeField] private GameEvent<SettingsEventData> onSettingChanged;
        [SerializeField] private GameEvent<SettingsEventData> onPresetApplied;
        
        // Runtime Data
        private Dictionary<string, ISettingsCategory> settingsCategories;
        private SettingsProfileSO currentProfile;
        
        #region ISystemModule Implementation
        
        public string ModuleName => "Settings System";
        public Version ModuleVersion => new Version(1, 0, 0);
        public bool IsEnabled { get; set; } = true;
        
        public async UniTask<bool> InitializeAsync()
        {
            try
            {
                await LoadSettingsCategories();
                await LoadCurrentProfile();
                await ApplyCurrentSettings();
                
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SettingsManager] Initialization failed: {ex.Message}");
                return false;
            }
        }
        
        #endregion
        
        #region Settings Management
        
        public async UniTask<T> GetSetting<T>(string categoryName, string settingName)
        {
            if (settingsCategories.TryGetValue(categoryName, out var category))
            {
                return await category.GetSetting<T>(settingName);
            }
            
            throw new InvalidOperationException($"Category '{categoryName}' not found");
        }
        
        public async UniTask SetSetting<T>(string categoryName, string settingName, T value, bool immediate = false)
        {
            if (settingsCategories.TryGetValue(categoryName, out var category))
            {
                var oldValue = await category.GetSetting<T>(settingName);
                await category.SetSetting(settingName, value);
                
                if (immediate)
                {
                    await ApplySetting(categoryName, settingName, value);
                }
                
                // 既存のEventSystemとの統合
                RaiseSystemEvent(onSettingChanged, new SettingsEventData
                {
                    categoryName = categoryName,
                    settingName = settingName,
                    oldValue = oldValue,
                    newValue = value,
                    timestamp = Time.time
                });
            }
            else
            {
                throw new InvalidOperationException($"Category '{categoryName}' not found");
            }
        }
        
        private async UniTask ApplySetting<T>(string categoryName, string settingName, T value)
        {
            // 既存のCommandPatternを活用
            var command = new ApplySettingCommand<T>(categoryName, settingName, value);
            await ExecuteCommandAsync(command);
        }
        
        #endregion
    }
    
    // Graphics Settings Implementation
    [CreateAssetMenu(menuName = "asterivo/Settings/Graphics Settings", fileName = "GraphicsSettings")]
    public class GraphicsSettingsSO : SettingsCategorySO
    {
        [Header("Quality Settings")]
        public QualityLevel defaultQualityLevel = QualityLevel.High;
        public List<QualityLevelSettings> qualityLevels;
        
        [Header("Resolution Settings")]
        public Resolution defaultResolution;
        public List<Resolution> supportedResolutions;
        public bool defaultFullscreen = true;
        
        [Header("Frame Rate Settings")]
        public int defaultTargetFrameRate = 60;
        public List<int> supportedFrameRates;
        
        [Header("URP Specific Settings")]
        public URPSettings urpSettings;
        
        public override async UniTask ApplySettings()
        {
            // Quality Level
            var qualityLevel = await GetSetting<QualityLevel>("qualityLevel");
            QualitySettings.SetQualityLevel((int)qualityLevel);
            
            // Resolution
            var resolution = await GetSetting<Resolution>("resolution");
            var fullscreen = await GetSetting<bool>("fullscreen");
            Screen.SetResolution(resolution.width, resolution.height, fullscreen);
            
            // Frame Rate
            var frameRate = await GetSetting<int>("targetFrameRate");
            Application.targetFrameRate = frameRate;
            
            // URP Settings
            await ApplyURPSettings();
            
            // Existing Event System Integration
            RaiseSystemEvent(onSettingsApplied, new SettingsEventData 
            { 
                categoryName = CategoryName,
                message = "Graphics settings applied successfully"
            });
        }
        
        private async UniTask ApplyURPSettings()
        {
            var urpAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if (urpAsset == null) return;
            
            // Shadow Settings
            var shadowDistance = await GetSetting<float>("shadowDistance");
            var shadowCascades = await GetSetting<int>("shadowCascades");
            
            // URP Asset更新（Reflection使用）
            var shadowDistanceField = typeof(UniversalRenderPipelineAsset).GetField("m_ShadowDistance", BindingFlags.NonPublic | BindingFlags.Instance);
            shadowDistanceField?.SetValue(urpAsset, shadowDistance);
            
            // Post-Processing
            var postProcessing = await GetSetting<bool>("postProcessing");
            // URP Post-Processing設定更新
        }
    }
}
```

---

## 4. パフォーマンス最適化設計

### 4.1 Memory Management Strategy

#### 4.1.1 ObjectPool拡張
```csharp
// 既存ObjectPoolの拡張（新システム用）
namespace asterivo.Unity60.Enhancement.Performance
{
    public class EnhancedObjectPool<T> : ObjectPool<T> where T : class, IResettableEnhanced
    {
        // 統計情報追加
        public PoolStatistics Statistics { get; private set; }
        
        // メモリプレッシャー対応
        public void HandleMemoryPressure()
        {
            // 使用頻度の低いオブジェクトを解放
            CleanupUnusedObjects();
        }
        
        // 診断機能
        public PoolDiagnosticData GetDiagnosticData()
        {
            return new PoolDiagnosticData
            {
                totalAllocated = Statistics.TotalAllocated,
                currentlyActive = Statistics.CurrentlyActive,
                peakUsage = Statistics.PeakUsage,
                memoryFootprint = CalculateMemoryFootprint()
            };
        }
    }
    
    // 新システム用Resetインターフェース
    public interface IResettableEnhanced : IResettable
    {
        void ResetForPool();
        bool CanBeReused();
        float LastUsedTime { get; }
        int ReuseCount { get; }
    }
}
```

### 4.2 Async Operations Strategy

#### 4.2.1 UniTask活用パターン
```csharp
// 既存UniTaskパターンの継続・拡張
public abstract class AsyncSystemModule : EnhancedSystemBase, ISystemModule
{
    protected CancellationTokenSource moduleLifetimeToken;
    protected readonly CancellationTokenSource shutdownToken = new CancellationTokenSource();
    
    public virtual async UniTask<bool> InitializeAsync()
    {
        moduleLifetimeToken = CancellationTokenSource.CreateLinkedTokenSource(shutdownToken.Token);
        
        try
        {
            await OnInitializeAsync(moduleLifetimeToken.Token);
            return true;
        }
        catch (OperationCanceledException)
        {
            return false;
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            return false;
        }
    }
    
    protected abstract UniTask OnInitializeAsync(CancellationToken cancellationToken);
    
    public virtual async UniTask ShutdownAsync()
    {
        shutdownToken.Cancel();
        
        try
        {
            await OnShutdownAsync();
        }
        catch (OperationCanceledException)
        {
            // Expected when cancelling
        }
        finally
        {
            moduleLifetimeToken?.Dispose();
            shutdownToken.Dispose();
        }
    }
    
    protected abstract UniTask OnShutdownAsync();
}
```

---

## 5. テスト戦略

### 5.1 Unit Testing Strategy

#### 5.1.1 Test Architecture
```csharp
namespace asterivo.Unity60.Enhancement.Tests
{
    [TestFixture]
    public class SetupWizardSystemTests
    {
        private WizardController wizardController;
        private TestableWizardStep[] testSteps;
        
        [SetUp]
        public void Setup()
        {
            // Test Environment準備
            wizardController = CreateTestWizardController();
            testSteps = CreateTestSteps();
        }
        
        [Test]
        public async UniTask WizardController_WhenAllStepsSucceed_ShouldCompleteSuccessfully()
        {
            // Arrange
            foreach (var step in testSteps)
            {
                step.SetupForSuccess();
            }
            
            // Act
            var result = await wizardController.StartWizardAsync();
            
            // Assert
            Assert.IsTrue(result.isSuccess);
            Assert.AreEqual(testSteps.Length, result.completedSteps);
        }
        
        [Test]
        public async UniTask WizardController_WhenStepFails_ShouldHandleGracefully()
        {
            // Arrange
            testSteps[1].SetupForFailure(new InvalidOperationException("Test error"));
            
            // Act
            var result = await wizardController.StartWizardAsync();
            
            // Assert
            Assert.IsFalse(result.isSuccess);
            Assert.IsNotNull(result.error);
            Assert.AreEqual(1, result.failedStepIndex);
        }
    }
    
    public class TestableWizardStep : WizardStepBase
    {
        private bool shouldSucceed = true;
        private Exception failureException;
        
        public void SetupForSuccess() => shouldSucceed = true;
        public void SetupForFailure(Exception ex) 
        { 
            shouldSucceed = false; 
            failureException = ex; 
        }
        
        public override async UniTask<WizardStepResult> ExecuteAsync(Dictionary<string, object> context, CancellationToken cancellationToken)
        {
            await UniTask.Delay(10, cancellationToken); // Simulate work
            
            if (shouldSucceed)
            {
                return WizardStepResult.Success("Test step completed");
            }
            else
            {
                return WizardStepResult.Failed(failureException);
            }
        }
        
        public override bool ValidatePreConditions(Dictionary<string, object> context) => true;
        public override void CleanupStep() { /* Test cleanup */ }
    }
}
```

### 5.2 Integration Testing Strategy

#### 5.2.1 システム統合テスト
```csharp
[TestFixture]
public class SystemIntegrationTests
{
    [Test]
    public async UniTask SaveLoadSystem_WhenIntegratedWithExistingEvents_ShouldWorkCorrectly()
    {
        // 既存のGameEventシステムとの統合テスト
        // Save/Load処理でイベントが正しく発行・処理されることを確認
    }
    
    [Test]
    public async UniTask SettingsSystem_WhenChangedDuringRuntime_ShouldNotAffectPerformance()
    {
        // パフォーマンス影響なしでの設定変更テスト
        // 既存60FPS保証の維持確認
    }
}
```

---

## 6. デプロイメント戦略

### 6.1 段階的リリース戦略

#### Phase A → B → C → D → E
```
Release Strategy:
├── Phase A (Week 1) - MVP Release
│   ├── Core Setup Wizard
│   ├── Basic Genre Templates (FPS, TPS, Platformer)
│   └── Essential Asset Integration
├── Phase B (Week 2-3) - Feature Release  
│   ├── Save/Load System
│   ├── Settings System
│   └── Basic Localization (JP, EN)
├── Phase C (Week 4-5) - Production Release
│   ├── Build Pipeline
│   ├── Asset Validation
│   └── Performance Monitoring
├── Phase D (Week 6-8) - Ecosystem Release
│   ├── Package Templates
│   ├── Visual Scripting Integration
│   └── Timeline Integration
└── Phase E (Week 9-12) - Community Release
    ├── Plugin Architecture
    ├── Template Marketplace
    └── Community Tools
```

### 6.2 品質ゲート

#### 各Phase共通基準
- [ ] 単体テストカバレッジ80%以上
- [ ] 統合テスト全合格
- [ ] パフォーマンス基準維持（60FPS@1080p）
- [ ] メモリ使用量基準内（既存+20%以内）
- [ ] ドキュメント完備
- [ ] 既存機能への影響なし

---

## 7. 運用・保守設計

### 7.1 監視・診断システム

#### 7.1.1 システムヘルス監視
```csharp
public class SystemHealthMonitor : EnhancedSystemBase
{
    [Header("Monitoring Configuration")]
    [SerializeField] private float healthCheckInterval = 10f;
    [SerializeField] private List<ISystemModule> monitoredModules;
    
    [Header("Events")]
    [SerializeField] private GameEvent<SystemHealthEventData> onHealthStatusChanged;
    
    private Dictionary<string, SystemHealthData> moduleHealth;
    
    private async void Start()
    {
        await InitializeHealthMonitoring();
        StartHealthCheckLoop();
    }
    
    private async UniTask StartHealthCheckLoop()
    {
        while (this != null)
        {
            await PerformHealthCheck();
            await UniTask.Delay(TimeSpan.FromSeconds(healthCheckInterval));
        }
    }
    
    private async UniTask PerformHealthCheck()
    {
        foreach (var module in monitoredModules)
        {
            var health = await CheckModuleHealth(module);
            UpdateHealthStatus(module.ModuleName, health);
        }
    }
}
```

### 7.2 アップデート・移行戦略

#### 7.2.1 バージョン管理
```csharp
public class SystemVersionManager : MonoBehaviour
{
    [Header("Version Information")]
    public Version currentVersion;
    public List<MigrationScript> availableMigrations;
    
    public async UniTask<bool> MigrateToVersion(Version targetVersion)
    {
        var currentVersion = GetCurrentVersion();
        var migrationPath = FindMigrationPath(currentVersion, targetVersion);
        
        foreach (var migration in migrationPath)
        {
            if (!await ExecuteMigration(migration))
            {
                return false;
            }
        }
        
        return true;
    }
}
```

---

**承認**

| 役割 | 氏名 | 日付 | 署名 |
|------|------|------|------|
| システムアーキテクト | | | |
| 技術リーダー | | | |
| プロジェクトマネージャー | | | |