using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Debug;
using Sirenix.OdinInspector;
using DG.Tweening;

using DG.Tweening.Plugins.Options;
using DG.Tweening.Core;

namespace asterivo.Unity60.Features.UI
{
    /// <summary>
    /// UI状態データを格納する構造体（UIManager内でのローカル定義）
    /// </summary>
    [System.Serializable]
    public struct UIStateData
    {
        public string panelName;
        public UIPanelType panelType;
        public bool isVisible;
        public string metadata;

        public override string ToString()
        {
            return $"UIState: {panelName} ({panelType}) - Visible: {isVisible}";
        }
    }

    /// <summary>
    /// UIパネルのタイプを定義する列挙型
    /// </summary>
    public enum UIPanelType
    {
        Menu,
        HUD,
        Popup,
        Dialog,
        Overlay
    }
    /// <summary>
    /// 汎用UIマネージャー。イベント駆動アーキテクチャを使用してUI状態を管理します。
    /// 複数のUIパネルの表示・非表示を制御し、UI間の状態遷移を管理します。
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [TabGroup("UI Control", "Panels")]
        [LabelText("UI Panels")]
        [ListDrawerSettings(ShowIndexLabels = true, ListElementLabelName = "panelName")]
        [SerializeField] private List<UIPanel> uiPanels = new List<UIPanel>();
        
        [TabGroup("UI Control", "Events - Show")]
        [LabelText("Show Main Menu")]
        [SerializeField] private GameEvent onShowMainMenu;
        
        [TabGroup("UI Control", "Events - Show")]
        [LabelText("Show Game HUD")]
        [SerializeField] private GameEvent onShowGameHUD;
        
        [TabGroup("UI Control", "Events - Show")]
        [LabelText("Show Pause Menu")]
        [SerializeField] private GameEvent onShowPauseMenu;
        
        [TabGroup("UI Control", "Events - Show")]
        [LabelText("Show Inventory")]
        [SerializeField] private GameEvent onShowInventory;
        
        [TabGroup("UI Control", "Events - Show")]
        [LabelText("Show Settings")]
        [SerializeField] private GameEvent onShowSettings;
        
        [TabGroup("UI Control", "Events - Hide")]
        [LabelText("Hide Current UI")]
        [SerializeField] private GameEvent onHideCurrentUI;
        
        [TabGroup("UI Control", "Events - Hide")]
        [LabelText("Hide All UI")]
        [SerializeField] private GameEvent onHideAllUI;
        
        [TabGroup("UI Control", "Events - State")]
        [LabelText("UI State Changed")]
        [SerializeField] private UIStateEvent onUIStateChanged;
        
        [TabGroup("UI Control", "Settings")]
        [LabelText("Fade Duration")]
        [PropertyRange(0f, 2f)]
        [SuffixLabel("s", overlay: true)]
        [SerializeField] private float fadeDuration = 0.3f;
        
        [TabGroup("UI Control", "Settings")]
        [LabelText("Use Fade Transitions")]
        [SerializeField] private bool useFadeTransitions = true;
        
        [TabGroup("UI Control", "Settings")]
        [LabelText("Fade Canvas Group")]
        [SerializeField] private CanvasGroup fadeCanvasGroup;
        
        [TabGroup("UI Control", "Debug")]
        [ReadOnly]
        [ShowInInspector]
        [LabelText("Current Active Panel")]
        private string currentActivePanelName = "None";
        
        [TabGroup("UI Control", "Debug")]
        [ReadOnly]
        [ShowInInspector]
        [LabelText("Panel Count")]
        private int panelCount => uiPanels.Count;
        
        private Dictionary<string, UIPanel> panelDictionary;
        private UIPanel currentActivePanel;
        private Stack<UIPanel> panelHistory = new Stack<UIPanel>();
        
        /// <summary>
        /// UIパネル情報を格納するクラス
        /// </summary>
        [System.Serializable]
        public class UIPanel
        {
            [LabelText("Panel Name")]
            public string panelName;
            
            [LabelText("Panel Object")]
            [Required]
            public GameObject panelObject;
            
            [LabelText("Panel Type")]
            public UIPanelType panelType = UIPanelType.Menu;
            
            [LabelText("Close on Escape")]
            public bool closeOnEscape = true;
            
            [LabelText("Disable Other Panels")]
            public bool disableOtherPanels = true;
            
            [LabelText("Add to History")]
            public bool addToHistory = true;
            
            [ShowInInspector]
            [ReadOnly]
            [LabelText("Is Active")]
            public bool IsActive => panelObject != null && panelObject.activeInHierarchy;
        }
        
        // UIPanelType moved to namespace level to avoid circular reference
        
        private void Awake()
        {
            InitializePanels();
            SubscribeToEvents();
        }
        
        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }
        
        private void Update()
        {
            HandleInput();
        }
        
        /// <summary>
        /// パネル辞書を初期化します
        /// </summary>
        private void InitializePanels()
        {
            panelDictionary = new Dictionary<string, UIPanel>();
            
            foreach (var panel in uiPanels)
            {
                if (panel.panelObject != null && !string.IsNullOrEmpty(panel.panelName))
                {
                    panelDictionary[panel.panelName] = panel;
                    
                    // 初期状態で非アクティブに設定
                    panel.panelObject.SetActive(false);
                }
            }
        }
        
        /// <summary>
        /// イベントに購読します
        /// 注意: このプロジェクトではGameEventListenerコンポーネントを使用してイベント購読を行います
        /// 各GameEventにGameEventListenerを設定し、Responseで適切なメソッドを呼び出してください
        /// </summary>
        private void SubscribeToEvents()
        {
            // GameEventListenerコンポーネントを使用した購読システムの例:
            // 1. 各GameEventアセットにGameEventListenerを追加
            // 2. GameEventListenerのResponseで適切なメソッドを呼び出す
            // 例: onShowMainMenu -> ShowPanel("MainMenu")
        }
        
        /// <summary>
        /// イベントから購読解除します
        /// GameEventListenerを使用している場合、OnDisableで自動的に解除されます
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            // GameEventListenerコンポーネントが自動的に購読解除を処理します
        }
        
        /// <summary>
        /// 指定されたパネルを表示します
        /// </summary>
        public void ShowPanel(string panelName)
        {
            if (!panelDictionary.ContainsKey(panelName))
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                ProjectDebug.LogWarning($"UIManager: Panel '{panelName}' not found!");
#endif
                return;
            }
            
            var panel = panelDictionary[panelName];
            ShowPanel(panel);
        }
        
        /// <summary>
        /// 指定されたパネルを表示します
        /// </summary>
        private void ShowPanel(UIPanel panel)
        {
            if (panel.disableOtherPanels)
            {
                HideAllPanels();
            }
            
            if (panel.addToHistory && currentActivePanel != null && currentActivePanel != panel)
            {
                panelHistory.Push(currentActivePanel);
            }
            
            panel.panelObject.SetActive(true);
            currentActivePanel = panel;
            currentActivePanelName = panel.panelName;
            
            // UI状態変更イベントを発行
            onUIStateChanged?.Raise(new UIStateData 
            { 
                panelName = panel.panelName, 
                panelType = panel.panelType,
                isVisible = true,
                metadata = ""
            });
            
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            ProjectDebug.Log($"UIManager: Showed panel '{panel.panelName}'");
#endif
        }
        
        /// <summary>
        /// 現在のパネルを非表示にします
        /// </summary>
        public void HideCurrentPanel()
        {
            if (currentActivePanel != null)
            {
                HidePanel(currentActivePanel);
            }
        }
        
        /// <summary>
        /// 指定されたパネルを非表示にします
        /// </summary>
        public void HidePanel(string panelName)
        {
            if (panelDictionary.ContainsKey(panelName))
            {
                HidePanel(panelDictionary[panelName]);
            }
        }
        
        /// <summary>
        /// 指定されたパネルを非表示にします
        /// </summary>
        private void HidePanel(UIPanel panel)
        {
            panel.panelObject.SetActive(false);
            
            // UI状態変更イベントを発行
            onUIStateChanged?.Raise(new UIStateData 
            { 
                panelName = panel.panelName, 
                panelType = panel.panelType,
                isVisible = false,
                metadata = ""
            });
            
            if (currentActivePanel == panel)
            {
                currentActivePanel = null;
                currentActivePanelName = "None";
                
                // 履歴から前のパネルを復元
                if (panelHistory.Count > 0)
                {
                    var previousPanel = panelHistory.Pop();
                    ShowPanel(previousPanel);
                }
            }
            
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            ProjectDebug.Log($"UIManager: Hidden panel '{panel.panelName}'");
#endif
        }
        
        /// <summary>
        /// すべてのパネルを非表示にします
        /// </summary>
        public void HideAllPanels()
        {
            foreach (var panel in uiPanels)
            {
                if (panel.panelObject.activeInHierarchy)
                {
                    panel.panelObject.SetActive(false);
                }
            }
            
            currentActivePanel = null;
            currentActivePanelName = "None";
            panelHistory.Clear();
            
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            ProjectDebug.Log("UIManager: Hidden all panels");
#endif
        }
        
        /// <summary>
        /// 前のパネルに戻ります
        /// </summary>
        public void GoBack()
        {
            if (panelHistory.Count > 0)
            {
                HideCurrentPanel();
            }
        }
        
        /// <summary>
        /// 入力処理を行います
        /// </summary>
        private void HandleInput()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (currentActivePanel != null && currentActivePanel.closeOnEscape)
                {
                    GoBack();
                }
            }
        }
        
        /// <summary>
        /// パネルがアクティブかどうかを確認します
        /// </summary>
        public bool IsPanelActive(string panelName)
        {
            return panelDictionary.ContainsKey(panelName) && 
                   panelDictionary[panelName].panelObject.activeInHierarchy;
        }
        
        /// <summary>
        /// 現在アクティブなパネルの名前を取得します
        /// </summary>
        public string GetCurrentActivePanelName()
        {
            return currentActivePanelName;
        }
        
        #if UNITY_EDITOR
        [TabGroup("UI Control", "Debug")]
        [Button("Show Main Menu")]
        private void TestShowMainMenu()
        {
            if (Application.isPlaying)
                ShowPanel("MainMenu");
        }
        
        [TabGroup("UI Control", "Debug")]
        [Button("Show Game HUD")]
        private void TestShowGameHUD()
        {
            if (Application.isPlaying)
                ShowPanel("GameHUD");
        }
        
        [TabGroup("UI Control", "Debug")]
        [Button("Hide All")]
        private void TestHideAll()
        {
            if (Application.isPlaying)
                HideAllPanels();
        }
        #endif
        
        #region Fade System
        /// <summary>
        /// フェードインを実行
        /// </summary>
        public void FadeIn(System.Action onComplete = null)
        {
            if (!useFadeTransitions || fadeCanvasGroup == null)
            {
                onComplete?.Invoke();
                return;
            }

            fadeCanvasGroup.gameObject.SetActive(true);
            fadeCanvasGroup.alpha = 1f;
            DOTween.To(() => fadeCanvasGroup.alpha, x => fadeCanvasGroup.alpha = x, 0f, fadeDuration)
                .SetUpdate(true) // ポーズ中でも動作
                .OnComplete(() => {
                    fadeCanvasGroup.gameObject.SetActive(false);
                    onComplete?.Invoke();
                });
        }

        /// <summary>
        /// フェードアウトを実行
        /// </summary>
        public void FadeOut(System.Action onComplete = null)
        {
            if (!useFadeTransitions || fadeCanvasGroup == null)
            {
                onComplete?.Invoke();
                return;
            }

            fadeCanvasGroup.gameObject.SetActive(true);
            fadeCanvasGroup.alpha = 0f;
            DOTween.To(() => fadeCanvasGroup.alpha, x => fadeCanvasGroup.alpha = x, 1f, fadeDuration)
                .SetUpdate(true) // ポーズ中でも動作
                .OnComplete(() => {
                    onComplete?.Invoke();
                });
        }

        /// <summary>
        /// フェードアウト → アクション実行 → フェードインの組み合わせ
        /// </summary>
        public void FadeTransition(System.Action transitionAction)
        {
            if (!useFadeTransitions)
            {
                transitionAction?.Invoke();
                return;
            }

            FadeOut(() => {
                transitionAction?.Invoke();
                FadeIn();
            });
        }

        /// <summary>
        /// UIパネル切り替えをフェード付きで実行
        /// </summary>
        public void ShowPanelWithFade(string panelName)
        {
            FadeTransition(() => ShowPanel(panelName));
        }

        /// <summary>
        /// UIパネルをフェード付きで非表示
        /// </summary>
        public void HidePanelWithFade(string panelName)
        {
            FadeTransition(() => HidePanel(panelName));
        }

        /// <summary>
        /// シーン切り替えをフェード付きで実行
        /// </summary>
        public void ChangeSceneWithFade(string sceneName)
        {
            FadeOut(() => {
                UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
            });
        }
        #endregion
    }
}
