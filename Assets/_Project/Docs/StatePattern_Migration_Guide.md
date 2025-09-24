ご指摘の通り、現在の実装状況を正確に反映したドキュメントに修正します。既存のディレクトリ構成とアセンブリ定義を維持しながら、ステルスゲーム機能を追加する形で更新いたします。

## 📋 1. **requirements.md** - 要求仕様書（修正版）

### 記載内容サンプル
```markdown
# ステルスゲーム要求仕様書 (FPS/TPS切り替え対応)

## 1. プロジェクト概要
- **プロジェクト名**: [Stealth Game Title]
- **Unity バージョン**: 6000.0.42f1
- **ゲームジャンル**: ステルスアクション（FPS/TPS視点切り替え可能）
- **パッケージプレフィックス**: asterivo.Unity60
- **プラットフォーム**: PC (Windows/Mac), Console (PS5/Xbox)
- **レンダリングパイプライン**: URP 17.x
- **プロジェクトパス**: D:\UnityProjects\URP3D_Base01

## 2. 開発アーキテクチャ

### 2.1 既存アセンブリ構成
```
現在実装済み:
├── asterivo.Unity60.Core      # コアシステム（実装済み）
├── asterivo.Unity60.Camera    # カメラ機能（実装済み）
├── asterivo.Unity60.Player    # プレイヤー機能（実装済み）
└── asterivo.Unity60.Systems   # ゲームシステム（実装済み）

追加予定:
├── asterivo.Unity60.Stealth   # ステルスシステム（新規）
├── asterivo.Unity60.ViewSystem # 視点切替システム（新規）
├── asterivo.Unity60.AI        # AI システム（新規）
├── asterivo.Unity60.Gadgets   # ガジェットシステム（新規）
└── asterivo.Unity60.Environment # 環境インタラクション（新規）
```

### 2.2 ディレクトリ構成
```
D:\UnityProjects\URP3D_Base01\Assets\_Project\
├── Core/                    # 実装済み
│   ├── Data/               # データ構造
│   ├── Events/             # イベントシステム
│   ├── Input/              # 入力システム
│   ├── Optimization/       # 最適化
│   ├── Player/             # プレイヤー状態定義
│   ├── RenderingSettings/  # レンダリング設定
│   ├── Services/           # サービス層
│   └── Shared/             # 共有コンポーネント
│
├── Features/               # 機能実装
│   ├── Camera/            # カメラシステム（実装済み）
│   ├── Player/            # プレイヤー実装（実装済み）
│   ├── Stealth/           # ステルスシステム（新規追加）
│   ├── ViewSystem/        # 視点切替（新規追加）
│   ├── AI/                # AI機能（新規追加）
│   ├── Gadgets/           # ガジェット（新規追加）
│   └── Environment/       # 環境要素（新規追加）
│
├── Scenes/                # シーンファイル
├── Docs/                  # ドキュメント
└── _Sandbox/              # 実験用
```

## 3. 実装フェーズ

### Phase 1: 既存システムの拡張（現在）
- Core システムへのステルス要素追加
- Camera システムのFPS/TPS対応
- Player システムのステルス動作追加

### Phase 2: ステルスコア実装
- Detection システム構築
- Visibility 計算システム
- Alert レベル管理

### Phase 3: AI実装
- 敵AI の知覚システム
- パトロール・捜索行動
- グループAI連携

### Phase 4: ゲームプレイ要素
- ガジェットシステム
- 環境インタラクション
- UI/HUD実装
```

## 🔧 2. **assembly-spec.md** - アセンブリ仕様書（修正版）

### 記載内容サンプル
```markdown
# アセンブリ定義仕様書

## 1. 既存アセンブリの拡張

### asterivo.Unity60.Core.asmdef（既存・拡張）
#### 現在のディレクトリ: Assets/_Project/Core/

```json
{
  "name": "asterivo.Unity60.Core",
  "rootNamespace": "Asterivo.Unity60.Core",
  "references": [
    "Unity.InputSystem",
    "Unity.TextMeshPro",
    "Unity.Mathematics"
  ]
}
```

#### ステルス向け追加実装
```csharp
// Assets/_Project/Core/Data/StealthData.cs
namespace Asterivo.Unity60.Core.Data
{
    [System.Serializable]
    public struct DetectionInfo
    {
        public float visibility;
        public float noiseLevel;
        public Vector3 lastKnownPosition;
        public float suspicionLevel;
    }
    
    [System.Serializable]
    public enum AlertLevel
    {
        Unaware = 0,
        Suspicious = 1,
        Investigating = 2,
        Searching = 3,
        Alert = 4,
        Combat = 5
    }
}

// Assets/_Project/Core/Events/StealthEvents.cs
namespace Asterivo.Unity60.Core.Events
{
    [CreateAssetMenu(menuName = "Asterivo/Core/Events/Alert Event")]
    public class AlertLevelEvent : GameEvent<AlertLevel> { }
    
    [CreateAssetMenu(menuName = "Asterivo/Core/Events/Detection Event")]
    public class DetectionEvent : GameEvent<DetectionInfo> { }
}
```

### asterivo.Unity60.Camera.asmdef（既存・拡張）
#### 現在のディレクトリ: Assets/_Project/Features/Camera/

```csharp
// Assets/_Project/Features/Camera/Scripts/ViewModeController.cs
namespace Asterivo.Unity60.Camera
{
    public enum ViewMode
    {
        FirstPerson,
        ThirdPerson,
        Cover,      // カバー時の特殊視点
        Transition  // 遷移中
    }
    
    [System.Serializable]
    public class ViewModeSettings : ScriptableObject
    {
        [Header("FPS Settings")]
        public float fpsFieldOfView = 90f;
        public float fpsAimFOV = 60f;
        public Vector3 fpsOffset;
        
        [Header("TPS Settings")]
        public float tpsFieldOfView = 60f;
        public Vector3 tpsOffset = new Vector3(0.5f, 1.5f, -3f);
        public float tpsDistance = 5f;
        
        [Header("Transition")]
        public float transitionDuration = 0.2f;
        public AnimationCurve transitionCurve;
    }
}
```

### asterivo.Unity60.Player.asmdef（既存・拡張）
#### 現在のディレクトリ: Assets/_Project/Features/Player/

```csharp
// Assets/_Project/Features/Player/Scripts/StealthMovement.cs
namespace Asterivo.Unity60.Player
{
    public class StealthMovementController : MonoBehaviour
    {
        [Header("Movement Modes")]
        public MovementMode currentMode;
        
        [System.Serializable]
        public class MovementMode
        {
            public string name;
            public float speed;
            public float noiseLevel;
            public float visibilityMultiplier;
        }
        
        public MovementMode[] modes = new[]
        {
            new MovementMode { name = "Prone", speed = 1f, noiseLevel = 0.1f },
            new MovementMode { name = "Crouch", speed = 2.5f, noiseLevel = 0.3f },
            new MovementMode { name = "Walk", speed = 4f, noiseLevel = 0.5f },
            new MovementMode { name = "Run", speed = 7f, noiseLevel = 1.0f }
        };
    }
}
```

## 2. 新規アセンブリの追加

### asterivo.Unity60.Stealth.asmdef（新規）
#### 作成場所: Assets/_Project/Features/Stealth/

```json
{
  "name": "asterivo.Unity60.Stealth",
  "rootNamespace": "Asterivo.Unity60.Stealth",
  "references": [
    "asterivo.Unity60.Core",
    "asterivo.Unity60.Player",
    "Unity.Mathematics",
    "Unity.Collections"
  ]
}
```

#### 実装内容
```csharp
// Assets/_Project/Features/Stealth/Scripts/Detection/VisibilityCalculator.cs
namespace Asterivo.Unity60.Stealth.Detection
{
    public class VisibilityCalculator : MonoBehaviour
    {
        [SerializeField] private DetectionConfiguration config;
        
        public float CalculateVisibility(Transform target, Transform observer)
        {
            // 距離、角度、光量、姿勢による視認性計算
        }
    }
}

// Assets/_Project/Features/Stealth/Scripts/Detection/DetectionConfiguration.cs
[CreateAssetMenu(menuName = "Asterivo/Stealth/Detection Config")]
public class DetectionConfiguration : ScriptableObject
{
    public float maxDetectionRange = 30f;
    public float fieldOfView = 110f;
    public AnimationCurve lightVisibilityCurve;
    public AnimationCurve distanceFalloffCurve;
}
```

### asterivo.Unity60.ViewSystem.asmdef（新規）
#### 作成場所: Assets/_Project/Features/ViewSystem/

```json
{
  "name": "asterivo.Unity60.ViewSystem",
  "rootNamespace": "Asterivo.Unity60.ViewSystem",
  "references": [
    "asterivo.Unity60.Core",
    "asterivo.Unity60.Camera",
    "Unity.Cinemachine"
  ]
}
```

### asterivo.Unity60.AI.asmdef（新規）
#### 作成場所: Assets/_Project/Features/AI/

```json
{
  "name": "asterivo.Unity60.AI",
  "rootNamespace": "Asterivo.Unity60.AI",
  "references": [
    "asterivo.Unity60.Core",
    "asterivo.Unity60.Stealth",
    "Unity.AI.Navigation"
  ]
}
```
```

## 💻 3. **technical-spec.md** - 技術仕様書（修正版）

### 記載内容サンプル
```markdown
# ステルスゲーム技術仕様書

## 1. 既存プロジェクト構造への統合

### 1.1 現在のディレクトリ構成
```
D:\UnityProjects\URP3D_Base01\Assets\_Project\
├── Core/                              # 実装済み
│   ├── Data/
│   │   ├── [既存ファイル]
│   │   └── Stealth/                  # 新規追加
│   │       ├── DetectionData.cs
│   │       └── AlertData.cs
│   ├── Events/
│   │   ├── [既存ファイル]
│   │   └── StealthEvents/            # 新規追加
│   │       ├── AlertEvents.cs
│   │       └── DetectionEvents.cs
│   ├── Input/                        # 既存（拡張）
│   │   ├── InputActions.inputactions
│   │   └── StealthInputHandler.cs    # 新規追加
│   ├── Optimization/                 # 既存（活用）
│   ├── Player/                       # 既存（拡張）
│   │   └── States/
│   │       ├── [既存ステート]
│   │       └── StealthStates/        # 新規追加
│   ├── RenderingSettings/            # 既存
│   ├── Services/                     # 既存（拡張）
│   │   └── StealthService.cs         # 新規追加
│   └── Shared/                       # 既存
│
├── Features/
│   ├── Camera/                       # 実装済み（拡張）
│   │   ├── Scripts/
│   │   │   ├── [既存スクリプト]
│   │   │   └── ViewMode/             # 新規追加
│   │   │       ├── FPSViewController.cs
│   │   │       └── TPSViewController.cs
│   │   └── Settings/
│   │
│   ├── Player/                       # 実装済み（拡張）
│   │   ├── Scripts/
│   │   │   ├── [既存スクリプト]
│   │   │   └── Stealth/              # 新規追加
│   │   │       ├── StealthMovement.cs
│   │   │       └── CoverSystem.cs
│   │   └── Prefabs/
│   │
│   ├── Stealth/                      # 新規追加
│   │   ├── Scripts/
│   │   │   ├── Detection/
│   │   │   ├── Visibility/
│   │   │   └── Alert/
│   │   ├── Settings/
│   │   └── Prefabs/
│   │
│   ├── ViewSystem/                   # 新規追加
│   │   ├── Scripts/
│   │   ├── Settings/
│   │   └── Prefabs/
│   │
│   ├── AI/                           # 新規追加
│   │   ├── Scripts/
│   │   ├── BehaviorTrees/
│   │   └── Prefabs/
│   │
│   ├── Gadgets/                      # 新規追加
│   │   ├── Scripts/
│   │   ├── Data/
│   │   └── Prefabs/
│   │
│   └── Environment/                  # 新規追加
│       ├── Scripts/
│       ├── Settings/
│       └── Prefabs/
│
├── Scenes/                           # 既存
│   ├── [既存シーン]
│   └── StealthTestScenes/           # 新規追加
│
├── Docs/                             # 既存（更新）
│   ├── requirements.md
│   ├── assembly-spec.md
│   ├── technical-spec.md
│   ├── systems-spec.md
│   ├── test-spec.md
│   └── prefab-spec.md
│
└── _Sandbox/                         # 既存
```

## 2. 既存システムとの統合

### 2.1 Core システムの拡張
```csharp
// Assets/_Project/Core/Services/StealthService.cs
namespace Asterivo.Unity60.Core.Services
{
    public class StealthService : IGameService
    {
        private readonly Dictionary<int, AlertLevel> guardAlertStates;
        private readonly VisibilityCalculator visibilityCalculator;
        
        public void Initialize()
        {
            // 既存のサービスロケータに登録
            ServiceLocator.Register<StealthService>(this);
        }
        
        public AlertLevel GetGlobalAlertLevel()
        {
            // 全体の警戒レベルを計算
        }
    }
}
```

### 2.2 Input システムの拡張
```csharp
// 既存のInputActionsに追加するアクション
InputActions:
  Player:
    # 既存のアクション
    Move: [既存]
    Look: [既存]
    Jump: [既存]
    
    # ステルス用追加アクション
    Crouch: Keyboard/C, Gamepad/ButtonEast
    Prone: Keyboard/Z, Gamepad/ButtonSouth
    ToggleView: Keyboard/V, Gamepad/RightStickButton
    Lean: Keyboard/Q&E, Gamepad/LeftBumper&RightBumper
    TakeDown: Keyboard/F, Gamepad/ButtonWest
    UseGadget: Keyboard/G, Gamepad/DPadUp
```

### 2.3 既存Playerシステムとの統合
```csharp
// Assets/_Project/Features/Player/Scripts/PlayerController.cs の拡張
namespace Asterivo.Unity60.Player
{
    public partial class PlayerController : MonoBehaviour
    {
        // 既存のコンポーネント
        [Header("Existing Systems")]
        [SerializeField] private CharacterController characterController;
        [SerializeField] private PlayerInputHandler inputHandler;
        
        // ステルス用追加コンポーネント
        [Header("Stealth Systems")]
        [SerializeField] private StealthMovementController stealthMovement;
        [SerializeField] private VisibilityCalculator visibility;
        [SerializeField] private NoiseEmitter noiseEmitter;
        [SerializeField] private CoverDetector coverDetector;
        
        private void IntegrateStealthSystems()
        {
            // 既存システムとの連携
            inputHandler.OnCrouchInput += stealthMovement.ToggleCrouch;
            stealthMovement.OnMovementModeChanged += UpdateVisibility;
        }
    }
}
```

## 3. ScriptableObject 設計

### 3.1 設定ファイル配置
```
Assets/_Project/
├── ScriptableObjects/               # 新規作成
│   ├── Events/
│   │   ├── Core/                   # 既存イベント
│   │   └── Stealth/                # ステルスイベント
│   │       ├── SE_OnAlertLevelChanged.asset
│   │       ├── SE_OnPlayerDetected.asset
│   │       └── SE_OnStealthKill.asset
│   │
│   ├── Settings/
│   │   ├── Player/
│   │   │   ├── PS_PlayerMovement.asset      # 既存
│   │   │   └── PS_StealthMovement.asset     # 新規
│   │   ├── Camera/
│   │   │   ├── CS_ThirdPerson.asset         # 既存
│   │   │   ├── CS_FirstPerson.asset         # 新規
│   │   │   └── CS_ViewTransition.asset      # 新規
│   │   └── Stealth/
│   │       ├── SS_DetectionConfig.asset
│   │       ├── SS_VisibilityConfig.asset
│   │       └── SS_AlertConfig.asset
│   │
│   └── Data/
│       ├── Weapons/                 # 将来実装
│       ├── Gadgets/                 # ガジェットデータ
│       └── AI/                      # AIプロファイル
```
```

## 🔌 4. **systems-spec.md** - システム仕様書（修正版）

### 記載内容サンプル
```markdown
# ステルスシステム仕様書

## 1. 既存システムとの連携

### 1.1 Event System の活用
```csharp
// 既存のEventシステムを使用したステルス通知
namespace Asterivo.Unity60.Core.Events
{
    // Assets/_Project/Core/Events/StealthEvents/AlertChannels.cs
    public static class StealthEventChannels
    {
        // イベントアセットパス
        public const string ALERT_LEVEL_CHANGED = "Events/Stealth/OnAlertLevelChanged";
        public const string PLAYER_DETECTED = "Events/Stealth/OnPlayerDetected";
        public const string STEALTH_KILL = "Events/Stealth/OnStealthKill";
        public const string COVER_ENTERED = "Events/Stealth/OnCoverEntered";
        public const string GADGET_USED = "Events/Stealth/OnGadgetUsed";
    }
}

// 使用例
public class GuardAI : MonoBehaviour
{
    [SerializeField] private AlertLevelEvent onAlertChanged;
    
    private void ChangeAlertLevel(AlertLevel newLevel)
    {
        currentAlert = newLevel;
        onAlertChanged?.Raise(newLevel);  // 既存のイベントシステムを使用
    }
}
```

### 1.2 Service Locator パターンの活用
```csharp
// Assets/_Project/Core/Services/StealthServiceRegistry.cs
namespace Asterivo.Unity60.Core.Services
{
    public static class StealthServiceRegistry
    {
        public static void RegisterStealthServices()
        {
            // 既存のServiceLocatorに登録
            ServiceLocator.Register<IVisibilityService>(new VisibilityService());
            ServiceLocator.Register<IDetectionService>(new DetectionService());
            ServiceLocator.Register<IAlertService>(new AlertService());
            ServiceLocator.Register<IGadgetService>(new GadgetService());
        }
    }
    
    // サービスインターフェース
    public interface IVisibilityService
    {
        float CalculateVisibility(Transform observer, Transform target);
        bool IsInShadow(Vector3 position);
        float GetLightLevel(Vector3 position);
    }
}
```

## 2. Camera システムの拡張

### 2.1 視点切り替え実装
```csharp
// Assets/_Project/Features/Camera/Scripts/ViewModeManager.cs
namespace Asterivo.Unity60.Camera
{
    public class ViewModeManager : MonoBehaviour
    {
        [Header("Camera References")]
        [SerializeField] private Camera mainCamera;
        [SerializeField] private Transform cameraRig;
        
        [Header("View Configurations")]
        [SerializeField] private ViewModeSettings fpsSettings;
        [SerializeField] private ViewModeSettings tpsSettings;
        
        private ViewMode currentMode = ViewMode.ThirdPerson;
        
        public void ToggleViewMode()
        {
            var targetMode = currentMode == ViewMode.FirstPerson ? 
                ViewMode.ThirdPerson : ViewMode.FirstPerson;
            
            StartCoroutine(TransitionToView(targetMode));
        }
        
        private IEnumerator TransitionToView(ViewMode targetMode)
        {
            // スムーズな視点切り替え
            float elapsed = 0f;
            var startPos = cameraRig.localPosition;
            var startRot = cameraRig.localRotation;
            var startFOV = mainCamera.fieldOfView;
            
            var targetSettings = targetMode == ViewMode.FirstPerson ? 
                fpsSettings : tpsSettings;
            
            while (elapsed < targetSettings.transitionDuration)
            {
                elapsed += Time.deltaTime;
                float t = targetSettings.transitionCurve.Evaluate(
                    elapsed / targetSettings.transitionDuration);
                
                cameraRig.localPosition = Vector3.Lerp(
                    startPos, targetSettings.cameraOffset, t);
                cameraRig.localRotation = Quaternion.Slerp(
                    startRot, Quaternion.Euler(targetSettings.cameraRotation), t);
                mainCamera.fieldOfView = Mathf.Lerp(
                    startFOV, targetSettings.fieldOfView, t);
                
                yield return null;
            }
            
            currentMode = targetMode;
            OnViewModeChanged?.Invoke(currentMode);
        }
    }
}
```

## 3. Player システムの拡張

### 3.1 ステルス動作の統合
```csharp
// Assets/_Project/Features/Player/Scripts/Stealth/StealthController.cs
namespace Asterivo.Unity60.Player
{
    public class StealthController : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private PlayerController playerController;
        [SerializeField] private CharacterController characterController;
        
        [Header("Stealth Settings")]
        [SerializeField] private StealthMovementSettings settings;
        
        [Header("Current State")]
        public MovementStance currentStance = MovementStance.Standing;
        public float currentVisibility = 1.0f;
        public float currentNoiseLevel = 0.5f;
        
        private void Start()
        {
            // 既存のPlayerControllerと連携
            playerController = GetComponent<PlayerController>();
            IntegrateWithExistingSystems();
        }
        
        private void IntegrateWithExistingSystems()
        {
            // 既存の入力システムと連携
            var inputHandler = playerController.InputHandler;
            inputHandler.OnCrouchPressed += ToggleCrouch;
            inputHandler.OnPronePressed += ToggleProne;
            
            // 既存のステートマシンと連携
            var stateMachine = playerController.StateMachine;
            stateMachine.RegisterState(new CrouchState());
            stateMachine.RegisterState(new ProneState());
        }
    }
}
```

## 4. Optimization システムの活用

### 4.1 Object Pool の適用
```csharp
// Assets/_Project/Core/Optimization/StealthObjectPools.cs
namespace Asterivo.Unity60.Core.Optimization
{
    [CreateAssetMenu(menuName = "Asterivo/Optimization/Stealth Pool Config")]
    public class StealthPoolConfiguration : ScriptableObject
    {
        [System.Serializable]
        public class PoolDefinition
        {
            public GameObject prefab;
            public int initialSize;
            public int maxSize;
            public bool prewarm;
        }
        
        public PoolDefinition[] pools = new[]
        {
            // ガジェット用プール
            new PoolDefinition { prefab = noiseArrowPrefab, initialSize = 10 },
            new PoolDefinition { prefab = smokeGrenadePrefab, initialSize = 5 },
            
            // エフェクト用プール
            new PoolDefinition { prefab = detectionIndicatorPrefab, initialSize = 20 },
            new PoolDefinition { prefab = footstepVFXPrefab, initialSize = 30 }
        };
    }
}
```

## 5. 段階的実装計画

### Phase 1: 基盤拡張（Week 1-2）
```yaml
Tasks:
  - Core拡張:
      - DetectionData構造体追加
      - StealthEventsの定義
      - ServiceLocatorへの登録
      
  - Camera拡張:
      - ViewModeController実装
      - FPS/TPS設定の定義
      
  - Player拡張:
      - StealthMovementController追加
      - 姿勢システム実装
```

### Phase 2: ステルスコア（Week 3-4）
```yaml
Tasks:
  - Detection実装:
      - VisibilityCalculator
      - LightSampling
      - ShadowDetection
      
  - Alert実装:
      - AlertStateMachine
      - AlertPropagation
```

### Phase 3: AI実装（Week 5-6）
```yaml
Tasks:
  - Perception:
      - FieldOfView
      - HearingSystem
      - MemorySystem
      
  - Behavior:
      - PatrolBehavior
      - SearchBehavior
      - CombatBehavior
```
```

## 📝 プロジェクト統合後の構成

```bash
D:\UnityProjects\URP3D_Base01\
├── Assets/
│   ├── _Project/
│   │   ├── Core/                    # 既存（拡張）
│   │   ├── Features/
│   │   │   ├── Camera/             # 既存（拡張）
│   │   │   ├── Player/             # 既存（拡張）
│   │   │   ├── Stealth/            # 新規追加
│   │   │   ├── ViewSystem/         # 新規追加
│   │   │   ├── AI/                 # 新規追加
│   │   │   ├── Gadgets/            # 新規追加
│   │   │   └── Environment/        # 新規追加
│   │   ├── ScriptableObjects/      # 新規追加
│   │   ├── Prefabs/                # 新規追加
│   │   ├── Scenes/                 # 既存
│   │   ├── Docs/                   # 既存（更新）
│   │   └── _Sandbox/               # 既存
│   └── _ThirdParty/                # 既存
└── Packages/                        # 既存
```

この修正版では、既存の実装を維持しながら、段階的にステルス機能を追加できる構成になっています。

