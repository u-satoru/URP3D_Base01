using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
// using TMPro; // Removed to avoid dependency issues
using asterivo.Unity60.Core.Events;

namespace asterivo.Unity60.Features.Templates.ActionRPG.UI
{
    /// <summary>
    /// ActionRPGテンプレート用UIマネージャー
    /// HUD、メニュー、インベントリ、キャラクターシート管理
    /// </summary>
    public class ActionRPGUIManager : MonoBehaviour
    {
        [Header("HUD要素")]
        [SerializeField] private Slider healthBar;
        [SerializeField] private Slider manaBar;
        [SerializeField] private Slider experienceBar;
        [SerializeField] private Text levelText;
        [SerializeField] private Text healthText;
        [SerializeField] private Text manaText;
        
        [Header("メニューパネル")]
        [SerializeField] private GameObject inventoryPanel;
        [SerializeField] private GameObject characterPanel;
        [SerializeField] private GameObject skillTreePanel;
        [SerializeField] private GameObject settingsPanel;
        
        [Header("インベントリUI")]
        [SerializeField] private Transform inventoryGrid;
        [SerializeField] private GameObject itemSlotPrefab;
        [SerializeField] private Text goldText;
        
        [Header("キャラクター情報UI")]
        [SerializeField] private Text strengthText;
        [SerializeField] private Text dexterityText;
        [SerializeField] private Text intelligenceText;
        [SerializeField] private Text vitalityText;
        [SerializeField] private Text wisdomText;
        [SerializeField] private Text luckText;
        
        [Header("イベント")]
        [SerializeField] private StringGameEvent onUIStateChanged;
        [SerializeField] private IntGameEvent onItemSelected;
        
        // 状態管理
        public bool IsAnyMenuOpen { get; private set; }
        public UIState CurrentState { get; private set; } = UIState.Game;
        
        private Dictionary<UIState, GameObject> menuPanels;
        
        public enum UIState
        {
            Game,       // ゲームプレイ中
            Inventory,  // インベントリ表示
            Character,  // キャラクター情報表示
            SkillTree,  // スキルツリー表示
            Settings    // 設定メニュー表示
        }
        
        private void Start()
        {
            InitializeUI();
            SetupEventListeners();
        }
        
        private void Update()
        {
            HandleUIInput();
        }
        
        /// <summary>
        /// UI初期化
        /// </summary>
        private void InitializeUI()
        {
            // メニューパネル辞書の初期化
            menuPanels = new Dictionary<UIState, GameObject>
            {
                { UIState.Inventory, inventoryPanel },
                { UIState.Character, characterPanel },
                { UIState.SkillTree, skillTreePanel },
                { UIState.Settings, settingsPanel }
            };
            
            // 全メニューを非表示に
            foreach (var panel in menuPanels.Values)
            {
                if (panel != null)
                    panel.SetActive(false);
            }
            
            // HUDの初期化
            UpdateHealthBar(100f, 100f);
            UpdateManaBar(50f, 50f);
            UpdateExperienceBar(0f, 100f);
            UpdateLevel(1);
            
            Debug.Log("[ActionRPGUIManager] UI初期化完了");
        }

        /// <summary>
        /// 外部からのUIマネージャー初期化
        /// </summary>
        public void Initialize()
        {
            InitializeUI();
            SetupEventListeners();
            Debug.Log("[ActionRPGUIManager] External initialization completed");
        }

        /// <summary>
        /// イベントリスナーの設定
        /// </summary>
        private void SetupEventListeners()
        {
            // ここでイベント購読を設定
            // 例: プレイヤーのヘルス変更イベントに購読
        }
        
        /// <summary>
        /// UI入力処理
        /// </summary>
        private void HandleUIInput()
        {
            // Tab: インベントリ
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                ToggleUI(UIState.Inventory);
            }
            
            // C: キャラクター
            if (Input.GetKeyDown(KeyCode.C))
            {
                ToggleUI(UIState.Character);
            }
            
            // K: スキルツリー
            if (Input.GetKeyDown(KeyCode.K))
            {
                ToggleUI(UIState.SkillTree);
            }
            
            // Escape: 設定またはメニューを閉じる
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (IsAnyMenuOpen)
                    CloseAllMenus();
                else
                    ToggleUI(UIState.Settings);
            }
        }
        
        /// <summary>
        /// UI状態切り替え
        /// </summary>
        public void ToggleUI(UIState targetState)
        {
            if (targetState == UIState.Game)
            {
                CloseAllMenus();
                return;
            }
            
            // 現在の状態と同じなら閉じる
            if (CurrentState == targetState && IsAnyMenuOpen)
            {
                CloseAllMenus();
                return;
            }
            
            // 全メニューを閉じる
            CloseAllMenus();
            
            // 対象メニューを開く
            if (menuPanels.ContainsKey(targetState) && menuPanels[targetState] != null)
            {
                menuPanels[targetState].SetActive(true);
                CurrentState = targetState;
                IsAnyMenuOpen = true;
                
                onUIStateChanged?.Raise(targetState.ToString());
                
                // カーソル表示設定
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
        
        /// <summary>
        /// 全メニューを閉じる
        /// </summary>
        public void CloseAllMenus()
        {
            foreach (var panel in menuPanels.Values)
            {
                if (panel != null)
                    panel.SetActive(false);
            }
            
            CurrentState = UIState.Game;
            IsAnyMenuOpen = false;
            
            onUIStateChanged?.Raise("Game");
            
            // カーソル非表示設定
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        
        /// <summary>
        /// ヘルスバー更新
        /// </summary>
        public void UpdateHealthBar(float currentHealth, float maxHealth)
        {
            if (healthBar != null)
                healthBar.value = currentHealth / maxHealth;
                
            if (healthText != null)
                healthText.text = $"{currentHealth:F0}/{maxHealth:F0}";
        }
        
        /// <summary>
        /// マナバー更新
        /// </summary>
        public void UpdateManaBar(float currentMana, float maxMana)
        {
            if (manaBar != null)
                manaBar.value = currentMana / maxMana;
                
            if (manaText != null)
                manaText.text = $"{currentMana:F0}/{maxMana:F0}";
        }
        
        /// <summary>
        /// 経験値バー更新
        /// </summary>
        public void UpdateExperienceBar(float currentExp, float expToNext)
        {
            if (experienceBar != null)
                experienceBar.value = currentExp / expToNext;
        }
        
        /// <summary>
        /// レベル表示更新
        /// </summary>
        public void UpdateLevel(int level)
        {
            if (levelText != null)
                levelText.text = $"Lv.{level}";
        }
        
        /// <summary>
        /// キャラクターステータス更新
        /// </summary>
        public void UpdateCharacterStats(int str, int dex, int intel, int vit, int wis, int luck)
        {
            if (strengthText != null) strengthText.text = str.ToString();
            if (dexterityText != null) dexterityText.text = dex.ToString();
            if (intelligenceText != null) intelligenceText.text = intel.ToString();
            if (vitalityText != null) vitalityText.text = vit.ToString();
            if (wisdomText != null) wisdomText.text = wis.ToString();
            if (luckText != null) luckText.text = luck.ToString();
        }
        
        /// <summary>
        /// ゴールド表示更新
        /// </summary>
        public void UpdateGold(int gold)
        {
            if (goldText != null)
                goldText.text = $"Gold: {gold:N0}";
        }
        
        /// <summary>
        /// インベントリUI更新
        /// </summary>
        public void RefreshInventoryUI()
        {
            // インベントリ表示の更新処理
            // 実際の実装では ItemManager からアイテム情報を取得
        }

        /// <summary>
        /// レベルアップ画面表示
        /// </summary>
        public void ShowLevelUpScreen()
        {
            // レベルアップ画面の表示処理
            Debug.Log("[ActionRPGUIManager] Level up screen shown");

            // TODO: レベルアップ用のUIパネル実装
        }

        /// <summary>
        /// インベントリ表示
        /// </summary>
        public void ShowInventory()
        {
            ToggleUI(UIState.Inventory);
            Debug.Log("[ActionRPGUIManager] Inventory shown");
        }

        /// <summary>
        /// キャラクター画面切り替え
        /// </summary>
        public void ToggleCharacterScreen()
        {
            if (CurrentState == UIState.Character)
                ToggleUI(UIState.Game);
            else
                ToggleUI(UIState.Character);

            Debug.Log("[ActionRPGUIManager] Character screen toggled");
        }
    }
}