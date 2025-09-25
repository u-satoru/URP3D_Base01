# ランタイム視点切り替え機能 - システム設計書

## 文書情報

| 項目 | 詳細 |
|------|------|
| **文書名** | ランタイム視点切り替え機能システム設計書 |
| **プロジェクト** | URP3D_Base01 |
| **作成日** | 2025年9月7日 |
| **版数** | v1.0 |
| **ステータス** | 設計確定 |
| **対象読者** | 開発チーム、システムアーキテクト |
| **関連文書** | RuntimeViewSwitching_Specification.md |

---

## 1. アーキテクチャ設計

### 1.1 システム全体構成

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                    Runtime View Switching System                           │
├─────────────────────────────────────────────────────────────────────────────┤
│  Input Layer (入力処理)                                                     │
│  ├── ViewSwitchInputHandler.cs      - 入力イベント処理                      │
│  ├── InputActionIntegration.cs      - New Input System統合                  │
│  └── CustomKeyBindingSystem.cs      - カスタムキーバインド                   │
├─────────────────────────────────────────────────────────────────────────────┤
│  Business Logic Layer (ビジネスロジック)                                    │
│  ├── RuntimeViewSwitcher.cs         - メイン制御クラス                       │
│  ├── ViewTransitionManager.cs       - 遷移アニメーション管理                 │
│  ├── ViewSwitchValidator.cs         - 切り替え可否判定                       │
│  └── ViewSwitchSettings.cs          - 設定管理 (ScriptableObject)           │
├─────────────────────────────────────────────────────────────────────────────┤
│  Integration Layer (既存システム統合)                                        │
│  ├── ViewModeController Integration  - 既存ViewModeController拡張            │
│  ├── CameraStateMachine Integration  - 既存StateMachine統合                  │
│  ├── CinemachineIntegration Extension- 既存Cinemachine統合拡張               │
│  └── GameEvent System Bridge        - イベント駆動統合                       │
├─────────────────────────────────────────────────────────────────────────────┤
│  UI/Feedback Layer (UI・フィードバック)                                     │
│  ├── ViewSwitchUI.cs               - UI表示制御                             │
│  ├── ViewIndicatorWidget.cs        - 視点インジケータ                        │
│  ├── FeedbackManager.cs            - オーディオ・触覚フィードバック           │
│  └── ViewSwitchTutorial.cs         - チュートリアル機能                      │
├─────────────────────────────────────────────────────────────────────────────┤
│  Data Layer (データ管理)                                                    │
│  ├── ViewSwitchSettingsData.cs     - 設定データ構造                          │
│  ├── ViewTransitionData.cs         - 遷移データ定義                          │
│  └── ViewSwitchAnalytics.cs        - 使用統計・分析                          │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 1.2 既存システム統合戦略

#### 1.2.1 非破壊的拡張パターン
```csharp
// 既存ViewModeControllerの拡張（既存コードを変更せずに機能追加）
namespace asterivo.Unity60.Camera.Extensions
{
    public class RuntimeViewSwitcherExtension : MonoBehaviour
    {
        [Header("Runtime Switching Integration")]
        [SerializeField] private ViewModeController existingController;
        [SerializeField] private RuntimeViewSwitcher runtimeSwitcher;
        
        // 既存システムとの協調動作
        private void Start()
        {
            // 既存ViewModeControllerとの連携初期化
            IntegrateWithExistingController();
        }
        
        private void IntegrateWithExistingController()
        {
            // 既存のイベントをリッスンして拡張機能を連動
            if (existingController != null)
            {
                // 既存のToggleViewMode()の呼び出しをインターセプト
                runtimeSwitcher.SetExistingController(existingController);
            }
        }
    }
}
```

#### 1.2.2 イベント駆動統合
```csharp
// 既存GameEventSystemとの完全統合
public class ViewSwitchEventData
{
    public ViewMode fromMode;
    public ViewMode toMode;
    public float transitionDuration;
    public bool isPlayerInitiated;
    public ViewSwitchTrigger trigger;
}

// 新しいイベント定義（既存システムと統合）
[CreateAssetMenu(menuName = "asterivo/Events/View Switch Event")]
public class ViewSwitchEvent : GenericGameEvent<ViewSwitchEventData>
{
    // 既存GameEventシステムとの互換性保証
}
```

---

## 2. Core Classes設計

### 2.1 RuntimeViewSwitcher (主制御クラス)

```csharp
using UnityEngine;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Commands;
using Cysharp.Threading.Tasks;

namespace asterivo.Unity60.Camera.Runtime
{
    /// <summary>
    /// ランタイム視点切り替えの主制御クラス
    /// 既存システムとの統合とイベント駆動アーキテクチャを維持
    /// </summary>
    public class RuntimeViewSwitcher : MonoBehaviour, IGameEventListener<ViewSwitchEventData>
    {
        #region Inspector Fields
        
        [Header("Integration References")]
        [SerializeField] private ViewModeController viewModeController;
        [SerializeField] private CameraStateMachine cameraStateMachine;
        [SerializeField] private CinemachineIntegration cinemachineIntegration;
        
        [Header("Settings")]
        [SerializeField] private ViewSwitchSettings settings;
        
        [Header("Events")]
        [SerializeField] private ViewSwitchEvent onViewSwitchRequested;
        [SerializeField] private ViewSwitchEvent onViewSwitchStarted;
        [SerializeField] private ViewSwitchEvent onViewSwitchCompleted;
        [SerializeField] private GameEvent onViewSwitchFailed;
        
        [Header("Runtime State")]
        [SerializeField] private ViewMode currentTargetMode = ViewMode.ThirdPerson;
        [SerializeField] private bool isSwitchingEnabled = true;
        [SerializeField] private bool isCurrentlySwitching = false;
        
        #endregion
        
        #region Private Fields
        
        private ViewTransitionManager transitionManager;
        private ViewSwitchValidator switchValidator;
        private ViewSwitchInputHandler inputHandler;
        
        // 既存システムとの統合用
        private ViewMode lastKnownMode;
        
        // パフォーマンス最適化用
        private readonly ObjectPool<ViewSwitchCommand> commandPool = new ObjectPool<ViewSwitchCommand>();
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            InitializeComponents();
            ValidateReferences();
        }
        
        private void Start()
        {
            InitializeIntegration();
            RegisterEventListeners();
            InitializeDefaultView();
        }
        
        private void OnEnable()
        {
            RegisterInputEvents();
        }
        
        private void OnDisable()
        {
            UnregisterInputEvents();
            UnregisterEventListeners();
        }
        
        #endregion
        
        #region Initialization
        
        private void InitializeComponents()
        {
            // コンポーネント初期化
            transitionManager = GetComponent<ViewTransitionManager>() ?? gameObject.AddComponent<ViewTransitionManager>();
            switchValidator = GetComponent<ViewSwitchValidator>() ?? gameObject.AddComponent<ViewSwitchValidator>();
            inputHandler = GetComponent<ViewSwitchInputHandler>() ?? gameObject.AddComponent<ViewSwitchInputHandler>();
            
            // 設定の初期化
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<ViewSwitchSettings>();
                Debug.LogWarning("[RuntimeViewSwitcher] No settings assigned, using default values.");
            }
        }
        
        private void InitializeIntegration()
        {
            // 既存ViewModeControllerとの統合
            if (viewModeController != null)
            {
                lastKnownMode = viewModeController.GetCurrentMode();
                currentTargetMode = lastKnownMode;
            }
            
            // TransitionManagerの設定
            transitionManager.Initialize(settings, viewModeController, cameraStateMachine);
            
            // Validatorの設定
            switchValidator.Initialize(settings, viewModeController);
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// FPS/TPS視点を切り替える
        /// </summary>
        /// <param name="trigger">切り替えのトリガー情報</param>
        public async UniTask<bool> ToggleViewAsync(ViewSwitchTrigger trigger = ViewSwitchTrigger.Manual)
        {
            if (!CanSwitchView())
            {
                HandleSwitchRejected(trigger);
                return false;
            }
            
            ViewMode targetMode = GetNextViewMode();
            return await SwitchToViewAsync(targetMode, trigger);
        }
        
        /// <summary>
        /// 指定視点に切り替える
        /// </summary>
        /// <param name="targetMode">目標視点</param>
        /// <param name="trigger">切り替えのトリガー情報</param>
        public async UniTask<bool> SwitchToViewAsync(ViewMode targetMode, ViewSwitchTrigger trigger = ViewSwitchTrigger.Manual)
        {
            if (isCurrentlySwitching)
            {
                Debug.LogWarning("[RuntimeViewSwitcher] Switch already in progress, ignoring request.");
                return false;
            }
            
            if (!switchValidator.CanSwitchTo(targetMode, trigger))
            {
                HandleSwitchRejected(trigger);
                return false;
            }
            
            return await ExecuteSwitchAsync(targetMode, trigger);
        }
        
        /// <summary>
        /// 切り替え機能の有効/無効を制御
        /// </summary>
        /// <param name="enabled">有効フラグ</param>
        /// <param name="reason">理由（デバッグ用）</param>
        public void SetSwitchingEnabled(bool enabled, string reason = "")
        {
            isSwitchingEnabled = enabled;
            
            // イベント通知
            var eventData = new ViewSwitchStateEventData
            {
                isEnabled = enabled,
                reason = reason,
                timestamp = Time.time
            };
            
            // 既存GameEventシステムとの統合
            onViewSwitchStateChanged?.Raise(eventData);
        }
        
        /// <summary>
        /// 現在の視点切り替え設定を取得
        /// </summary>
        public ViewSwitchSettings GetCurrentSettings() => settings;
        
        /// <summary>
        /// 現在切り替え中かどうか
        /// </summary>
        public bool IsSwitching() => isCurrentlySwitching;
        
        #endregion
        
        #region Core Switch Logic
        
        private async UniTask<bool> ExecuteSwitchAsync(ViewMode targetMode, ViewSwitchTrigger trigger)
        {
            try
            {
                isCurrentlySwitching = true;
                ViewMode fromMode = GetCurrentViewMode();
                
                // 切り替え開始イベント
                var startEventData = new ViewSwitchEventData
                {
                    fromMode = fromMode,
                    toMode = targetMode,
                    transitionDuration = settings.transitionDuration,
                    isPlayerInitiated = trigger == ViewSwitchTrigger.Manual,
                    trigger = trigger
                };
                
                RaiseEvent(onViewSwitchStarted, startEventData);
                
                // コマンドパターンによる実行（既存アーキテクチャ準拠）
                var switchCommand = commandPool.Get();
                switchCommand.Initialize(fromMode, targetMode, settings);
                
                // 非同期遷移実行
                bool success = await transitionManager.ExecuteTransitionAsync(switchCommand);
                
                if (success)
                {
                    currentTargetMode = targetMode;
                    
                    // 完了イベント
                    var completeEventData = startEventData;
                    completeEventData.executionTime = Time.time - startEventData.timestamp;
                    RaiseEvent(onViewSwitchCompleted, completeEventData);
                }
                else
                {
                    // 失敗時の処理
                    RaiseEvent(onViewSwitchFailed, null);
                }
                
                // ObjectPoolに返却（メモリ最適化）
                commandPool.Return(switchCommand);
                
                return success;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[RuntimeViewSwitcher] Switch execution failed: {ex.Message}");
                RaiseEvent(onViewSwitchFailed, null);
                return false;
            }
            finally
            {
                isCurrentlySwitching = false;
            }
        }
        
        private ViewMode GetNextViewMode()
        {
            ViewMode current = GetCurrentViewMode();
            
            // 設定による切り替えロジック
            switch (current)
            {
                case ViewMode.FirstPerson:
                    return settings.allowThirdPersonFromFirst ? ViewMode.ThirdPerson : ViewMode.FirstPerson;
                    
                case ViewMode.ThirdPerson:
                    return settings.allowFirstPersonFromThird ? ViewMode.FirstPerson : ViewMode.ThirdPerson;
                    
                case ViewMode.Cover:
                    // カバー状態では切り替え不可
                    return current;
                    
                default:
                    return ViewMode.ThirdPerson; // デフォルトフォールバック
            }
        }
        
        private bool CanSwitchView()
        {
            if (!isSwitchingEnabled) return false;
            if (isCurrentlySwitching) return false;
            
            return switchValidator.CanSwitchFromCurrentState();
        }
        
        private ViewMode GetCurrentViewMode()
        {
            if (viewModeController != null)
            {
                return viewModeController.GetCurrentMode();
            }
            
            return currentTargetMode; // フォールバック
        }
        
        #endregion
        
        #region Event Handling
        
        private void RegisterEventListeners()
        {
            if (onViewSwitchRequested != null)
            {
                onViewSwitchRequested.RegisterListener(this);
            }
        }
        
        private void UnregisterEventListeners()
        {
            if (onViewSwitchRequested != null)
            {
                onViewSwitchRequested.UnregisterListener(this);
            }
        }
        
        // IGameEventListener<ViewSwitchEventData>の実装
        public void OnEventRaised(ViewSwitchEventData eventData)
        {
            // 外部からのビューチェンジリクエスト処理
            _ = SwitchToViewAsync(eventData.toMode, eventData.trigger);
        }
        
        private void RaiseEvent<T>(GenericGameEvent<T> gameEvent, T data)
        {
            if (gameEvent != null)
            {
                gameEvent.Raise(data);
            }
        }
        
        private void HandleSwitchRejected(ViewSwitchTrigger trigger)
        {
            // 切り替え拒否時のフィードバック
            if (settings.enableAudioFeedback)
            {
                // オーディオフィードバック（既存AudioSystemとの統合）
                var audioCommand = new PlaySoundCommand("UI_SwitchRejected");
                CommandInvoker.Instance.ExecuteCommand(audioCommand);
            }
            
            if (settings.enableHapticFeedback)
            {
                // 触覚フィードバック
                StartCoroutine(PlayHapticFeedback(HapticFeedbackType.Warning));
            }
        }
        
        #endregion
        
        #region Input Integration
        
        private void RegisterInputEvents()
        {
            if (inputHandler != null)
            {
                inputHandler.OnViewSwitchRequested += HandleInputSwitchRequest;
            }
        }
        
        private void UnregisterInputEvents()
        {
            if (inputHandler != null)
            {
                inputHandler.OnViewSwitchRequested -= HandleInputSwitchRequest;
            }
        }
        
        private void HandleInputSwitchRequest()
        {
            _ = ToggleViewAsync(ViewSwitchTrigger.Manual);
        }
        
        #endregion
        
        #region Debug & Validation
        
        private void ValidateReferences()
        {
            if (viewModeController == null)
            {
                Debug.LogError("[RuntimeViewSwitcher] ViewModeController reference is missing!");
            }
            
            if (settings == null)
            {
                Debug.LogWarning("[RuntimeViewSwitcher] ViewSwitchSettings is missing!");
            }
        }
        
        #if UNITY_EDITOR
        [Header("Debug Info (Runtime)")]
        [SerializeField] private string debugCurrentMode;
        [SerializeField] private string debugTargetMode;
        [SerializeField] private bool debugIsSwitching;
        
        private void Update()
        {
            if (Application.isPlaying)
            {
                debugCurrentMode = GetCurrentViewMode().ToString();
                debugTargetMode = currentTargetMode.ToString();
                debugIsSwitching = isCurrentlySwitching;
            }
        }
        #endif
        
        #endregion
    }
    
    // 補助データ構造
    public enum ViewSwitchTrigger
    {
        Manual,          // プレイヤー手動
        Automatic,       // システム自動
        Script,          // スクリプト呼び出し
        Settings,        // 設定変更
        GameState       // ゲーム状態変化
    }
    
    [System.Serializable]
    public class ViewSwitchStateEventData
    {
        public bool isEnabled;
        public string reason;
        public float timestamp;
    }
}
```

### 2.2 ViewTransitionManager (遷移管理クラス)

```csharp
using UnityEngine;
using Cysharp.Threading.Tasks;
using DG.Tweening;

namespace asterivo.Unity60.Camera.Runtime
{
    /// <summary>
    /// 視点切り替え時の遷移アニメーションを管理
    /// DOTween統合により滑らかなカメラ遷移を実現
    /// </summary>
    public class ViewTransitionManager : MonoBehaviour
    {
        #region Private Fields
        
        private ViewSwitchSettings settings;
        private ViewModeController viewModeController;
        private CameraStateMachine cameraStateMachine;
        
        // DOTween用シーケンス
        private Sequence currentTransitionSequence;
        
        #endregion
        
        #region Initialization
        
        public void Initialize(ViewSwitchSettings switchSettings, 
                             ViewModeController modeController,
                             CameraStateMachine stateMachine)
        {
            settings = switchSettings;
            viewModeController = modeController;
            cameraStateMachine = stateMachine;
        }
        
        #endregion
        
        #region Transition Execution
        
        /// <summary>
        /// 視点切り替え遷移を実行
        /// </summary>
        public async UniTask<bool> ExecuteTransitionAsync(ViewSwitchCommand command)
        {
            try
            {
                // 既存遷移の停止
                StopCurrentTransition();
                
                // 遷移タイプに応じた処理
                switch (settings.transitionType)
                {
                    case TransitionType.Smooth:
                        return await ExecuteSmoothTransitionAsync(command);
                        
                    case TransitionType.Cut:
                        return await ExecuteCutTransitionAsync(command);
                        
                    case TransitionType.Fade:
                        return await ExecuteFadeTransitionAsync(command);
                        
                    default:
                        return await ExecuteSmoothTransitionAsync(command);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[ViewTransitionManager] Transition failed: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 滑らかな遷移（DOTween使用）
        /// </summary>
        private async UniTask<bool> ExecuteSmoothTransitionAsync(ViewSwitchCommand command)
        {
            float duration = settings.transitionDuration;
            
            // DOTweenシーケンス作成
            currentTransitionSequence = DOTween.Sequence();
            
            // 既存ViewModeControllerとの統合
            if (viewModeController != null)
            {
                // 遷移開始前の状態保存
                ViewMode fromMode = command.FromMode;
                ViewMode toMode = command.ToMode;
                
                // カメラ遷移の実行（既存システム活用）
                viewModeController.SwitchToView(toMode);
                
                // 追加のスムージング処理
                await ApplySmoothingEffects(fromMode, toMode, duration);
            }
            
            // シーケンス完了待機
            await currentTransitionSequence.Play().ToUniTask();
            
            return true;
        }
        
        /// <summary>
        /// 瞬時切り替え
        /// </summary>
        private async UniTask<bool> ExecuteCutTransitionAsync(ViewSwitchCommand command)
        {
            if (viewModeController != null)
            {
                viewModeController.SwitchToView(command.ToMode);
            }
            
            // 最小遅延（フレーム同期）
            await UniTask.Yield();
            
            return true;
        }
        
        /// <summary>
        /// フェード遷移
        /// </summary>
        private async UniTask<bool> ExecuteFadeTransitionAsync(ViewSwitchCommand command)
        {
            float fadeDuration = settings.transitionDuration * 0.5f;
            
            // フェードアウト
            await ExecuteFadeEffect(1f, 0f, fadeDuration);
            
            // 視点切り替え（非表示中）
            if (viewModeController != null)
            {
                viewModeController.SwitchToView(command.ToMode);
            }
            
            // フェードイン
            await ExecuteFadeEffect(0f, 1f, fadeDuration);
            
            return true;
        }
        
        #endregion
        
        #region Effects & Animation
        
        private async UniTask ApplySmoothingEffects(ViewMode fromMode, ViewMode toMode, float duration)
        {
            // FOVアニメーション
            if (settings.enableFOVAnimation)
            {
                await AnimateFOVTransition(fromMode, toMode, duration);
            }
            
            // カメラシェイク軽減
            if (settings.enableShakeReduction)
            {
                await ApplyShakeReduction(duration);
            }
        }
        
        private async UniTask AnimateFOVTransition(ViewMode fromMode, ViewMode toMode, float duration)
        {
            var camera = UnityEngine.Camera.main;
            if (camera == null) return;
            
            float fromFOV = GetFOVForMode(fromMode);
            float toFOV = GetFOVForMode(toMode);
            
            // DOTweenによるFOVアニメーション
            var fovTween = camera.DOFieldOfView(toFOV, duration)
                                 .SetEase(settings.fovAnimationCurve);
            
            currentTransitionSequence.Append(fovTween);
            
            await fovTween.ToUniTask();
        }
        
        private async UniTask ExecuteFadeEffect(float fromAlpha, float toAlpha, float duration)
        {
            // フェードエフェクト実装
            // UIのCanvasGroupまたはカメラのRenderTextureを使用
            
            var fadePanel = GetOrCreateFadePanel();
            if (fadePanel != null)
            {
                var fadeTween = fadePanel.DOFade(toAlpha, duration)
                                        .SetEase(Ease.InOutQuad);
                                        
                await fadeTween.ToUniTask();
            }
        }
        
        #endregion
        
        #region Utility Methods
        
        private void StopCurrentTransition()
        {
            if (currentTransitionSequence != null && currentTransitionSequence.IsActive())
            {
                currentTransitionSequence.Kill(true);
            }
        }
        
        private float GetFOVForMode(ViewMode mode)
        {
            // 設定から適切なFOV値を取得
            switch (mode)
            {
                case ViewMode.FirstPerson:
                    return settings.firstPersonFOV;
                case ViewMode.ThirdPerson:
                    return settings.thirdPersonFOV;
                default:
                    return 60f;
            }
        }
        
        private CanvasGroup GetOrCreateFadePanel()
        {
            // フェード用UI要素の取得または作成
            var fadePanel = GameObject.FindObjectOfType<Canvas>()?.GetComponentInChildren<CanvasGroup>();
            
            if (fadePanel == null)
            {
                // 動的にフェードパネルを作成
                var canvas = FindObjectOfType<Canvas>();
                if (canvas != null)
                {
                    var fadeGO = new GameObject("FadePanel");
                    fadeGO.transform.SetParent(canvas.transform);
                    fadePanel = fadeGO.AddComponent<CanvasGroup>();
                    
                    var image = fadeGO.AddComponent<UnityEngine.UI.Image>();
                    image.color = Color.black;
                    image.raycastTarget = false;
                    
                    // 全画面に拡張
                    var rectTransform = fadeGO.GetComponent<RectTransform>();
                    rectTransform.anchorMin = Vector2.zero;
                    rectTransform.anchorMax = Vector2.one;
                    rectTransform.offsetMin = Vector2.zero;
                    rectTransform.offsetMax = Vector2.zero;
                    
                    // 初期状態は透明
                    fadePanel.alpha = 0f;
                }
            }
            
            return fadePanel;
        }
        
        #endregion
        
        #region Cleanup
        
        private void OnDestroy()
        {
            StopCurrentTransition();
        }
        
        #endregion
    }
    
    // 遷移タイプ定義
    public enum TransitionType
    {
        Smooth,    // 滑らかな遷移
        Cut,       // 瞬時切り替え
        Fade       // フェード遷移
    }
}
```

### 2.3 ViewSwitchValidator (切り替え可否判定クラス)

```csharp
using UnityEngine;
using asterivo.Unity60.Core.Events;

namespace asterivo.Unity60.Camera.Runtime
{
    /// <summary>
    /// 視点切り替えの可否を判定するバリデーター
    /// ゲーム状態や設定に基づいて切り替え可能性を評価
    /// </summary>
    public class ViewSwitchValidator : MonoBehaviour
    {
        #region Private Fields
        
        private ViewSwitchSettings settings;
        private ViewModeController viewModeController;
        
        // 状態追跡用
        private bool isPlayerInCover;
        private bool isPlayerAiming;
        private bool isCutscenePlaying;
        private bool isMenuOpen;
        
        #endregion
        
        #region Initialization
        
        public void Initialize(ViewSwitchSettings switchSettings, ViewModeController modeController)
        {
            settings = switchSettings;
            viewModeController = modeController;
            
            RegisterStateListeners();
        }
        
        private void RegisterStateListeners()
        {
            // 既存GameEventシステムとの統合によるゲーム状態追跡
            // プレイヤー状態イベントの監視
            RegisterPlayerStateEvents();
        }
        
        private void RegisterPlayerStateEvents()
        {
            // プレイヤーカバー状態
            var coverEvent = Resources.Load<GameEvent>("Events/Player/OnCoverStateChanged");
            if (coverEvent != null)
            {
                coverEvent.RegisterListener(OnPlayerCoverStateChanged);
            }
            
            // エイム状態
            var aimEvent = Resources.Load<GameEvent>("Events/Player/OnAimStateChanged");
            if (aimEvent != null)
            {
                aimEvent.RegisterListener(OnPlayerAimStateChanged);
            }
        }
        
        #endregion
        
        #region Public Validation API
        
        /// <summary>
        /// 現在の状態から切り替え可能かチェック
        /// </summary>
        public bool CanSwitchFromCurrentState()
        {
            ViewMode currentMode = GetCurrentViewMode();
            return CanSwitchFromMode(currentMode);
        }
        
        /// <summary>
        /// 指定モードから切り替え可能かチェック
        /// </summary>
        public bool CanSwitchFromMode(ViewMode fromMode)
        {
            // 基本的な制限チェック
            if (!IsBasicSwitchingAllowed()) return false;
            
            // モード固有の制限チェック
            if (!IsModeSpecificSwitchingAllowed(fromMode)) return false;
            
            // ゲーム状態による制限チェック
            if (!IsGameStateAllowingSwitching()) return false;
            
            return true;
        }
        
        /// <summary>
        /// 特定モードへの切り替え可否チェック
        /// </summary>
        public bool CanSwitchTo(ViewMode targetMode, ViewSwitchTrigger trigger)
        {
            ViewMode currentMode = GetCurrentViewMode();
            
            // 同じモードへの切り替えは不要
            if (currentMode == targetMode) return false;
            
            // 基本的な切り替え可否
            if (!CanSwitchFromMode(currentMode)) return false;
            
            // ターゲットモード固有チェック
            if (!IsTargetModeAccessible(targetMode, trigger)) return false;
            
            // 遷移パスの有効性チェック
            if (!IsTransitionPathValid(currentMode, targetMode)) return false;
            
            return true;
        }
        
        #endregion
        
        #region Validation Logic
        
        private bool IsBasicSwitchingAllowed()
        {
            // 設定による基本制限
            if (!settings.enableRuntimeSwitching) return false;
            
            // 遷移中は切り替え不可
            if (viewModeController != null && viewModeController.IsTransitioning()) return false;
            
            return true;
        }
        
        private bool IsModeSpecificSwitchingAllowed(ViewMode fromMode)
        {
            switch (fromMode)
            {
                case ViewMode.Cover:
                    // カバー状態からの切り替えは基本的に不可
                    return settings.allowSwitchFromCover;
                    
                case ViewMode.FirstPerson:
                    return settings.allowSwitchFromFirstPerson;
                    
                case ViewMode.ThirdPerson:
                    return settings.allowSwitchFromThirdPerson;
                    
                case ViewMode.Transition:
                    // 遷移中は常に不可
                    return false;
                    
                default:
                    return true;
            }
        }
        
        private bool IsGameStateAllowingSwitching()
        {
            // カットシーン中は不可
            if (isCutscenePlaying && !settings.allowSwitchDuringCutscenes)
            {
                return false;
            }
            
            // メニュー開放中は不可
            if (isMenuOpen && !settings.allowSwitchInMenu)
            {
                return false;
            }
            
            // エイム中の制限
            if (isPlayerAiming && !settings.allowSwitchWhileAiming)
            {
                return false;
            }
            
            // プレイヤーがカバー中の制限
            if (isPlayerInCover && !settings.allowSwitchInCover)
            {
                return false;
            }
            
            return true;
        }
        
        private bool IsTargetModeAccessible(ViewMode targetMode, ViewSwitchTrigger trigger)
        {
            switch (targetMode)
            {
                case ViewMode.FirstPerson:
                    return settings.enableFirstPersonMode;
                    
                case ViewMode.ThirdPerson:
                    return settings.enableThirdPersonMode;
                    
                case ViewMode.Cover:
                    // カバーモードは通常、手動切り替えでは不可
                    return trigger != ViewSwitchTrigger.Manual;
                    
                default:
                    return false;
            }
        }
        
        private bool IsTransitionPathValid(ViewMode fromMode, ViewMode toMode)
        {
            // 禁止された遷移パスの定義
            var forbiddenTransitions = settings.forbiddenTransitions;
            
            foreach (var transition in forbiddenTransitions)
            {
                if (transition.fromMode == fromMode && transition.toMode == toMode)
                {
                    return false;
                }
            }
            
            return true;
        }
        
        #endregion
        
        #region State Event Handlers
        
        private void OnPlayerCoverStateChanged()
        {
            // プレイヤーのカバー状態変更通知
            // この情報を使って切り替え可否を動的に判定
            
            // 既存イベントシステムからの情報取得
            // 実装詳細は既存プレイヤーシステムに依存
        }
        
        private void OnPlayerAimStateChanged()
        {
            // プレイヤーのエイム状態変更通知
        }
        
        #endregion
        
        #region Utility Methods
        
        private ViewMode GetCurrentViewMode()
        {
            if (viewModeController != null)
            {
                return viewModeController.GetCurrentMode();
            }
            
            return ViewMode.ThirdPerson; // デフォルト
        }
        
        /// <summary>
        /// 現在の制限理由を取得（デバッグ用）
        /// </summary>
        public string GetRestrictionReason()
        {
            if (!settings.enableRuntimeSwitching)
                return "Runtime switching disabled in settings";
                
            if (isCutscenePlaying)
                return "Cannot switch during cutscenes";
                
            if (isMenuOpen)
                return "Cannot switch while menu is open";
                
            if (isPlayerAiming && !settings.allowSwitchWhileAiming)
                return "Cannot switch while aiming";
                
            if (isPlayerInCover)
                return "Cannot switch while in cover";
                
            return "No restrictions";
        }
        
        #endregion
        
        #region Cleanup
        
        private void OnDestroy()
        {
            // イベントリスナーの解除
            UnregisterStateListeners();
        }
        
        private void UnregisterStateListeners()
        {
            // イベントリスナーの解除処理
        }
        
        #endregion
    }
}
```

### 2.4 ViewSwitchSettings (設定管理ScriptableObject)

```csharp
using UnityEngine;
using System.Collections.Generic;

namespace asterivo.Unity60.Camera.Runtime
{
    [CreateAssetMenu(menuName = "asterivo/Camera/View Switch Settings", fileName = "ViewSwitchSettings")]
    public class ViewSwitchSettings : ScriptableObject
    {
        [Header("Basic Settings")]
        [Tooltip("ランタイム視点切り替えを有効化")]
        public bool enableRuntimeSwitching = true;
        
        [Tooltip("デフォルト視点モード")]
        public ViewMode defaultViewMode = ViewMode.ThirdPerson;
        
        [Header("View Mode Availability")]
        [Tooltip("一人称視点を有効化")]
        public bool enableFirstPersonMode = true;
        
        [Tooltip("三人称視点を有効化")]
        public bool enableThirdPersonMode = true;
        
        [Header("Switching Permissions")]
        [Tooltip("一人称からの切り替えを許可")]
        public bool allowSwitchFromFirstPerson = true;
        
        [Tooltip("三人称からの切り替えを許可")]
        public bool allowSwitchFromThirdPerson = true;
        
        [Tooltip("カバー状態からの切り替えを許可")]
        public bool allowSwitchFromCover = false;
        
        [Header("Game State Restrictions")]
        [Tooltip("エイム中の切り替えを許可")]
        public bool allowSwitchWhileAiming = false;
        
        [Tooltip("カバー中の切り替えを許可")]
        public bool allowSwitchInCover = false;
        
        [Tooltip("カットシーン中の切り替えを許可")]
        public bool allowSwitchDuringCutscenes = false;
        
        [Tooltip("メニュー表示中の切り替えを許可")]
        public bool allowSwitchInMenu = false;
        
        [Header("Transition Settings")]
        [Tooltip("遷移タイプ")]
        public TransitionType transitionType = TransitionType.Smooth;
        
        [Range(0.1f, 2.0f)]
        [Tooltip("遷移時間（秒）")]
        public float transitionDuration = 0.3f;
        
        [Tooltip("遷移カーブ")]
        public AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        
        [Header("Camera Settings")]
        [Range(30f, 120f)]
        [Tooltip("一人称視点のFOV")]
        public float firstPersonFOV = 90f;
        
        [Range(30f, 120f)]
        [Tooltip("三人称視点のFOV")]
        public float thirdPersonFOV = 60f;
        
        [Header("Animation Settings")]
        [Tooltip("FOVアニメーションを有効化")]
        public bool enableFOVAnimation = true;
        
        [Tooltip("FOVアニメーションカーブ")]
        public AnimationCurve fovAnimationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        
        [Tooltip("カメラシェイク軽減を有効化")]
        public bool enableShakeReduction = true;
        
        [Header("Input Settings")]
        [Tooltip("切り替えキー")]
        public KeyCode switchKey = KeyCode.V;
        
        [Tooltip("ゲームパッドボタン")]
        public string gamepadButton = "RightStickClick";
        
        [Tooltip("リピート入力防止時間（秒）")]
        public float inputCooldown = 0.1f;
        
        [Header("Feedback Settings")]
        [Tooltip("オーディオフィードバックを有効化")]
        public bool enableAudioFeedback = true;
        
        [Tooltip("切り替え成功音")]
        public AudioClip switchSuccessSound;
        
        [Tooltip("切り替え失敗音")]
        public AudioClip switchFailSound;
        
        [Tooltip("触覚フィードバックを有効化")]
        public bool enableHapticFeedback = true;
        
        [Range(0.1f, 1.0f)]
        [Tooltip("触覚フィードバック強度")]
        public float hapticIntensity = 0.3f;
        
        [Header("UI Settings")]
        [Tooltip("視点インジケータを表示")]
        public bool showViewIndicator = true;
        
        [Tooltip("インジケータ表示時間（秒）")]
        public float indicatorDisplayTime = 2f;
        
        [Tooltip("インジケータフェード時間（秒）")]
        public float indicatorFadeTime = 0.5f;
        
        [Header("Advanced Settings")]
        [Tooltip("デバッグログを有効化")]
        public bool enableDebugLogging = false;
        
        [Tooltip("パフォーマンス監視を有効化")]
        public bool enablePerformanceMonitoring = false;
        
        [Tooltip("使用統計を収集")]
        public bool collectUsageAnalytics = true;
        
        [Header("Forbidden Transitions")]
        [Tooltip("禁止された遷移パス")]
        public List<ViewTransitionRule> forbiddenTransitions = new List<ViewTransitionRule>();
        
        #region Validation
        
        private void OnValidate()
        {
            // 設定値の妥当性チェック
            ValidateSettings();
        }
        
        private void ValidateSettings()
        {
            // 遷移時間の範囲チェック
            transitionDuration = Mathf.Clamp(transitionDuration, 0.1f, 2.0f);
            
            // FOV値の範囲チェック
            firstPersonFOV = Mathf.Clamp(firstPersonFOV, 30f, 120f);
            thirdPersonFOV = Mathf.Clamp(thirdPersonFOV, 30f, 120f);
            
            // 触覚フィードバック強度の範囲チェック
            hapticIntensity = Mathf.Clamp(hapticIntensity, 0.1f, 1.0f);
        }
        
        #endregion
        
        #region Default Values
        
        private void Reset()
        {
            // デフォルト値の設定
            enableRuntimeSwitching = true;
            defaultViewMode = ViewMode.ThirdPerson;
            
            enableFirstPersonMode = true;
            enableThirdPersonMode = true;
            
            allowSwitchFromFirstPerson = true;
            allowSwitchFromThirdPerson = true;
            allowSwitchFromCover = false;
            
            allowSwitchWhileAiming = false;
            allowSwitchInCover = false;
            allowSwitchDuringCutscenes = false;
            allowSwitchInMenu = false;
            
            transitionType = TransitionType.Smooth;
            transitionDuration = 0.3f;
            transitionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
            
            firstPersonFOV = 90f;
            thirdPersonFOV = 60f;
            
            enableFOVAnimation = true;
            fovAnimationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
            enableShakeReduction = true;
            
            switchKey = KeyCode.V;
            gamepadButton = "RightStickClick";
            inputCooldown = 0.1f;
            
            enableAudioFeedback = true;
            enableHapticFeedback = true;
            hapticIntensity = 0.3f;
            
            showViewIndicator = true;
            indicatorDisplayTime = 2f;
            indicatorFadeTime = 0.5f;
            
            enableDebugLogging = false;
            enablePerformanceMonitoring = false;
            collectUsageAnalytics = true;
        }
        
        #endregion
    }
    
    // 補助データ構造
    [System.Serializable]
    public class ViewTransitionRule
    {
        public ViewMode fromMode;
        public ViewMode toMode;
        public string reason;
    }
}
```

---

## 3. 統合戦略

### 3.1 既存システムとの統合パターン

#### 3.1.1 ViewModeController拡張
```csharp
// 既存クラスを変更せず、拡張により機能追加
namespace asterivo.Unity60.Camera.Extensions
{
    public static class ViewModeControllerExtensions
    {
        // 拡張メソッドによる機能追加
        public static async UniTask<bool> SwitchToViewRuntimeAsync(this ViewModeController controller, ViewMode targetMode)
        {
            // RuntimeViewSwitcherとの協調処理
            var runtimeSwitcher = Object.FindObjectOfType<RuntimeViewSwitcher>();
            if (runtimeSwitcher != null)
            {
                return await runtimeSwitcher.SwitchToViewAsync(targetMode, ViewSwitchTrigger.Script);
            }
            
            // フォールバック：既存メソッド使用
            controller.SwitchToView(targetMode);
            return true;
        }
    }
}
```

#### 3.1.2 Input System統合
```csharp
// 新Input Systemとの統合
namespace asterivo.Unity60.Camera.Runtime
{
    public class ViewSwitchInputHandler : MonoBehaviour
    {
        [Header("Input Actions")]
        [SerializeField] private InputActionReference switchViewAction;
        
        // イベント定義
        public System.Action OnViewSwitchRequested;
        
        private void OnEnable()
        {
            if (switchViewAction?.action != null)
            {
                switchViewAction.action.performed += OnSwitchActionPerformed;
                switchViewAction.action.Enable();
            }
        }
        
        private void OnDisable()
        {
            if (switchViewAction?.action != null)
            {
                switchViewAction.action.performed -= OnSwitchActionPerformed;
                switchViewAction.action.Disable();
            }
        }
        
        private void OnSwitchActionPerformed(InputAction.CallbackContext context)
        {
            OnViewSwitchRequested?.Invoke();
        }
    }
}
```

### 3.2 UI統合設計

#### 3.2.1 視点インジケータUI
```csharp
namespace asterivo.Unity60.Camera.Runtime.UI
{
    public class ViewIndicatorWidget : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TMPro.TextMeshProUGUI modeText;
        [SerializeField] private UnityEngine.UI.Image modeIcon;
        
        [Header("Icons")]
        [SerializeField] private Sprite firstPersonIcon;
        [SerializeField] private Sprite thirdPersonIcon;
        
        private Sequence fadeSequence;
        
        public void ShowModeChanged(ViewMode newMode, float displayTime)
        {
            // モード表示の更新
            UpdateModeDisplay(newMode);
            
            // フェードアニメーション
            fadeSequence?.Kill();
            fadeSequence = DOTween.Sequence()
                .Append(canvasGroup.DOFade(1f, 0.2f))
                .AppendInterval(displayTime)
                .Append(canvasGroup.DOFade(0f, 0.5f));
        }
        
        private void UpdateModeDisplay(ViewMode mode)
        {
            switch (mode)
            {
                case ViewMode.FirstPerson:
                    modeText.text = "First Person";
                    modeIcon.sprite = firstPersonIcon;
                    break;
                    
                case ViewMode.ThirdPerson:
                    modeText.text = "Third Person";
                    modeIcon.sprite = thirdPersonIcon;
                    break;
            }
        }
    }
}
```

---

## 4. パフォーマンス最適化

### 4.1 メモリ最適化

#### 4.1.1 ObjectPoolパターン適用
```csharp
// コマンドオブジェクトのプール化
public class ViewSwitchCommand : ICommand, IResettableCommand
{
    public ViewMode FromMode { get; private set; }
    public ViewMode ToMode { get; private set; }
    public ViewSwitchSettings Settings { get; private set; }
    
    public void Initialize(ViewMode fromMode, ViewMode toMode, ViewSwitchSettings settings)
    {
        FromMode = fromMode;
        ToMode = toMode;
        Settings = settings;
    }
    
    public void Execute()
    {
        // 実行処理
    }
    
    public void ResetForPool()
    {
        FromMode = ViewMode.ThirdPerson;
        ToMode = ViewMode.ThirdPerson;
        Settings = null;
    }
}
```

### 4.2 フレームレート最適化

#### 4.2.1 非同期処理活用
```csharp
// UniTaskによる非同期処理でフレームレートへの影響を最小化
public async UniTask<bool> ExecuteTransitionAsync(ViewSwitchCommand command)
{
    // 重い処理を複数フレームに分散
    await UniTask.Yield();
    
    // DOTweenによる滑らかなアニメーション
    var tween = camera.DOFieldOfView(targetFOV, duration);
    await tween.ToUniTask();
    
    return true;
}
```

---

## 5. テスト戦略

### 5.1 Unit Testing

#### 5.1.1 ViewSwitchValidator テスト
```csharp
[TestFixture]
public class ViewSwitchValidatorTests
{
    private ViewSwitchValidator validator;
    private ViewSwitchSettings settings;
    
    [SetUp]
    public void Setup()
    {
        settings = ScriptableObject.CreateInstance<ViewSwitchSettings>();
        validator = new GameObject().AddComponent<ViewSwitchValidator>();
        validator.Initialize(settings, null);
    }
    
    [Test]
    public void CanSwitchFromCurrentState_WhenSwitchingDisabled_ReturnsFalse()
    {
        // Arrange
        settings.enableRuntimeSwitching = false;
        
        // Act
        bool result = validator.CanSwitchFromCurrentState();
        
        // Assert
        Assert.IsFalse(result);
    }
    
    [Test]
    public void CanSwitchTo_WhenTargetModeIsSame_ReturnsFalse()
    {
        // Same mode switching should be rejected
    }
}
```

### 5.2 Integration Testing

#### 5.2.1 システム統合テスト
```csharp
[TestFixture]
public class RuntimeViewSwitchingIntegrationTests
{
    [Test]
    public async Task ViewSwitching_WhenTriggered_UpdatesExistingSystemsCorrectly()
    {
        // 既存システムとの統合が正しく動作することを確認
    }
    
    [Test]
    public void GameEvents_WhenViewSwitched_AreRaisedCorrectly()
    {
        // GameEventSystemとの統合テスト
    }
}
```

---

## 6. デプロイメント戦略

### 6.1 段階的実装

#### Phase 1: Core Implementation (3日)
- `RuntimeViewSwitcher`基本実装
- `ViewSwitchValidator`基本実装
- 基本的な切り替え機能

#### Phase 2: Integration (2日)
- 既存システムとの統合
- Input System統合
- UI統合

#### Phase 3: Polish (2日)
- アニメーション・エフェクト
- フィードバックシステム
- 設定システム完成

### 6.2 品質保証

#### テスト計画
- Unit Test: 80%カバレッジ
- Integration Test: 主要シナリオ網羅
- Performance Test: 60FPS維持確認
- User Testing: 5名でのユーザビリティ評価

---

**承認**

| 役割 | 氏名 | 日付 | 署名 |
|------|------|------|------|
| システムアーキテクト | | | |
| 技術リーダー | | | |
| プロジェクトマネージャー | | | |
