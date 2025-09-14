using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using asterivo.Unity60.Core.Events;

namespace asterivo.Unity60.Features.Templates.ActionRPG.UI
{
    /// <summary>
    /// ActionRPGテンプレート用HUD管理
    /// ゲーム中のヘッズアップディスプレイ要素管理
    /// </summary>
    public class HUDManager : MonoBehaviour
    {
        [Header("基本HUD要素")]
        [SerializeField] private Canvas hudCanvas;
        [SerializeField] private CanvasGroup hudCanvasGroup;
        [SerializeField] private bool hideHUDInMenus = true;
        
        [Header("ヘルスバー")]
        [SerializeField] private Slider healthBar;
        [SerializeField] private Slider healthBarBackground;
        [SerializeField] private Image healthBarFill;
        [SerializeField] private Color healthColorNormal = Color.green;
        [SerializeField] private Color healthColorWarning = Color.yellow;
        [SerializeField] private Color healthColorCritical = Color.red;
        [SerializeField] private float healthWarningThreshold = 0.5f;
        [SerializeField] private float healthCriticalThreshold = 0.25f;
        
        [Header("マナバー")]
        [SerializeField] private Slider manaBar;
        [SerializeField] private Image manaBarFill;
        [SerializeField] private Color manaColorNormal = Color.blue;
        [SerializeField] private Color manaColorLow = Color.magenta;
        [SerializeField] private float manaLowThreshold = 0.3f;
        
        [Header("経験値バー")]
        [SerializeField] private Slider experienceBar;
        [SerializeField] private Image experienceBarFill;
        [SerializeField] private Color expColorNormal = Color.cyan;
        
        [Header("スキルクールダウン表示")]
        [SerializeField] private Transform skillCooldownParent;
        [SerializeField] private GameObject skillCooldownPrefab;
        [SerializeField] private int maxSkillSlots = 10;
        
        [Header("ステータス表示")]
        [SerializeField] private GameObject damageNumberPrefab;
        [SerializeField] private Transform damageNumberParent;
        [SerializeField] private float damageNumberDuration = 2.0f;
        [SerializeField] private Vector3 damageNumberOffset = new Vector3(0, 2f, 0);
        
        [Header("ミニマップ")]
        [SerializeField] private GameObject minimapContainer;
        [SerializeField] private Camera minimapCamera;
        [SerializeField] private RectTransform minimapFrame;
        [SerializeField] private bool enableMinimap = true;
        
        [Header("通知システム")]
        [SerializeField] private Transform notificationParent;
        [SerializeField] private GameObject notificationPrefab;
        [SerializeField] private int maxNotifications = 5;
        [SerializeField] private float notificationDuration = 3.0f;
        
        [Header("イベント")]
        [SerializeField] private FloatGameEvent onHealthChanged;
        [SerializeField] private FloatGameEvent onManaChanged;
        [SerializeField] private FloatGameEvent onExperienceChanged;
        
        // 内部状態
        private bool isHUDVisible = true;
        private Coroutine healthAnimationCoroutine;
        private Coroutine manaAnimationCoroutine;
        
        // 統計
        private float currentHealthRatio = 1f;
        private float currentManaRatio = 1f;
        private float currentExpRatio = 0f;
        
        private void Start()
        {
            InitializeHUD();
            SetupEventListeners();
        }
        
        private void Update()
        {
            UpdateHUDElements();
        }
        
        /// <summary>
        /// HUD初期化
        /// </summary>
        private void InitializeHUD()
        {
            if (hudCanvas == null)
                hudCanvas = GetComponent<Canvas>();
                
            if (hudCanvasGroup == null)
                hudCanvasGroup = GetComponent<CanvasGroup>();
                
            // HUDの初期設定
            SetHUDVisibility(true);
            
            // バーの初期化
            UpdateHealthBar(1f, true);
            UpdateManaBar(1f, true);
            UpdateExperienceBar(0f, true);
            
            // ミニマップ設定
            SetMinimapVisibility(enableMinimap);
            
            Debug.Log("[HUDManager] HUD初期化完了");
        }
        
        /// <summary>
        /// イベントリスナー設定
        /// </summary>
        private void SetupEventListeners()
        {
            // ヘルス変更イベント
            if (onHealthChanged != null)
            {
                onHealthChanged.AddListener(OnHealthChanged);
            }
            
            // マナ変更イベント
            if (onManaChanged != null)
            {
                onManaChanged.AddListener(OnManaChanged);
            }
            
            // 経験値変更イベント
            if (onExperienceChanged != null)
            {
                onExperienceChanged.AddListener(OnExperienceChanged);
            }
        }
        
        /// <summary>
        /// HUD要素更新
        /// </summary>
        private void UpdateHUDElements()
        {
            // スキルクールダウン表示更新
            UpdateSkillCooldowns();
            
            // ミニマップ更新
            if (enableMinimap && minimapCamera != null)
            {
                UpdateMinimap();
            }
        }
        
        /// <summary>
        /// HUD可視性設定
        /// </summary>
        public void SetHUDVisibility(bool visible)
        {
            isHUDVisible = visible;
            
            if (hudCanvasGroup != null)
            {
                hudCanvasGroup.alpha = visible ? 1f : 0f;
                hudCanvasGroup.interactable = visible;
                hudCanvasGroup.blocksRaycasts = visible;
            }
            else if (hudCanvas != null)
            {
                hudCanvas.enabled = visible;
            }
        }
        
        /// <summary>
        /// ヘルスバー更新
        /// </summary>
        public void UpdateHealthBar(float ratio, bool immediate = false)
        {
            if (healthBar == null) return;
            
            currentHealthRatio = Mathf.Clamp01(ratio);
            
            if (immediate)
            {
                healthBar.value = currentHealthRatio;
                UpdateHealthBarColor();
            }
            else
            {
                // スムーズなアニメーション
                if (healthAnimationCoroutine != null)
                    StopCoroutine(healthAnimationCoroutine);
                    
                healthAnimationCoroutine = StartCoroutine(AnimateHealthBar(currentHealthRatio));
            }
        }
        
        /// <summary>
        /// ヘルスバーアニメーション
        /// </summary>
        private IEnumerator AnimateHealthBar(float targetRatio)
        {
            float startRatio = healthBar.value;
            float duration = 0.5f;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                
                healthBar.value = Mathf.Lerp(startRatio, targetRatio, t);
                UpdateHealthBarColor();
                
                yield return null;
            }
            
            healthBar.value = targetRatio;
            UpdateHealthBarColor();
        }
        
        /// <summary>
        /// ヘルスバー色更新
        /// </summary>
        private void UpdateHealthBarColor()
        {
            if (healthBarFill == null) return;
            
            Color targetColor;
            
            if (currentHealthRatio <= healthCriticalThreshold)
                targetColor = healthColorCritical;
            else if (currentHealthRatio <= healthWarningThreshold)
                targetColor = healthColorWarning;
            else
                targetColor = healthColorNormal;
                
            healthBarFill.color = targetColor;
        }
        
        /// <summary>
        /// マナバー更新
        /// </summary>
        public void UpdateManaBar(float ratio, bool immediate = false)
        {
            if (manaBar == null) return;
            
            currentManaRatio = Mathf.Clamp01(ratio);
            
            if (immediate)
            {
                manaBar.value = currentManaRatio;
                UpdateManaBarColor();
            }
            else
            {
                if (manaAnimationCoroutine != null)
                    StopCoroutine(manaAnimationCoroutine);
                    
                manaAnimationCoroutine = StartCoroutine(AnimateManaBar(currentManaRatio));
            }
        }
        
        /// <summary>
        /// マナバーアニメーション
        /// </summary>
        private IEnumerator AnimateManaBar(float targetRatio)
        {
            float startRatio = manaBar.value;
            float duration = 0.3f;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                
                manaBar.value = Mathf.Lerp(startRatio, targetRatio, t);
                UpdateManaBarColor();
                
                yield return null;
            }
            
            manaBar.value = targetRatio;
            UpdateManaBarColor();
        }
        
        /// <summary>
        /// マナバー色更新
        /// </summary>
        private void UpdateManaBarColor()
        {
            if (manaBarFill == null) return;
            
            Color targetColor = currentManaRatio <= manaLowThreshold ? manaColorLow : manaColorNormal;
            manaBarFill.color = targetColor;
        }
        
        /// <summary>
        /// 経験値バー更新
        /// </summary>
        public void UpdateExperienceBar(float ratio, bool immediate = false)
        {
            if (experienceBar == null) return;
            
            currentExpRatio = Mathf.Clamp01(ratio);
            
            if (immediate)
            {
                experienceBar.value = currentExpRatio;
            }
            else
            {
                StartCoroutine(AnimateExperienceBar(currentExpRatio));
            }
        }
        
        /// <summary>
        /// 経験値バーアニメーション
        /// </summary>
        private IEnumerator AnimateExperienceBar(float targetRatio)
        {
            float startRatio = experienceBar.value;
            float duration = 1.0f;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                
                experienceBar.value = Mathf.Lerp(startRatio, targetRatio, t);
                yield return null;
            }
            
            experienceBar.value = targetRatio;
        }
        
        /// <summary>
        /// ダメージ数値表示
        /// </summary>
        public void ShowDamageNumber(Vector3 worldPosition, float damage, bool isCritical = false)
        {
            if (damageNumberPrefab == null || damageNumberParent == null) return;
            
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldPosition + damageNumberOffset);
            
            var damageNumberObj = Instantiate(damageNumberPrefab, damageNumberParent);
            var rectTransform = damageNumberObj.GetComponent<RectTransform>();
            
            if (rectTransform != null)
            {
                rectTransform.position = screenPosition;
            }
            
            // ダメージ数値の設定（コンポーネントがある場合）
            var damageText = damageNumberObj.GetComponentInChildren<UnityEngine.UI.Text>();
            if (damageText != null)
            {
                damageText.text = damage.ToString("F0");
                damageText.color = isCritical ? Color.red : Color.white;
            }
            
            // 自動削除
            StartCoroutine(DestroyDamageNumber(damageNumberObj));
        }
        
        /// <summary>
        /// ダメージ数値削除
        /// </summary>
        private IEnumerator DestroyDamageNumber(GameObject damageNumber)
        {
            yield return new WaitForSeconds(damageNumberDuration);
            
            if (damageNumber != null)
                Destroy(damageNumber);
        }
        
        /// <summary>
        /// スキルクールダウン更新
        /// </summary>
        private void UpdateSkillCooldowns()
        {
            // 実際の実装では SkillManager からクールダウン情報を取得
        }
        
        /// <summary>
        /// ミニマップ更新
        /// </summary>
        private void UpdateMinimap()
        {
            // プレイヤーの位置に基づいてミニマップカメラを更新
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null && minimapCamera != null)
            {
                Vector3 playerPos = player.transform.position;
                minimapCamera.transform.position = new Vector3(playerPos.x, minimapCamera.transform.position.y, playerPos.z);
            }
        }
        
        /// <summary>
        /// ミニマップ可視性設定
        /// </summary>
        public void SetMinimapVisibility(bool visible)
        {
            if (minimapContainer != null)
                minimapContainer.SetActive(visible);
        }
        
        /// <summary>
        /// 通知表示
        /// </summary>
        public void ShowNotification(string message)
        {
            if (notificationPrefab == null || notificationParent == null) return;
            
            var notificationObj = Instantiate(notificationPrefab, notificationParent);
            var notificationText = notificationObj.GetComponentInChildren<UnityEngine.UI.Text>();
            
            if (notificationText != null)
            {
                notificationText.text = message;
            }
            
            // 自動削除
            StartCoroutine(DestroyNotification(notificationObj));
            
            // 最大数制限
            if (notificationParent.childCount > maxNotifications)
            {
                Destroy(notificationParent.GetChild(0).gameObject);
            }
        }
        
        /// <summary>
        /// 通知削除
        /// </summary>
        private IEnumerator DestroyNotification(GameObject notification)
        {
            yield return new WaitForSeconds(notificationDuration);
            
            if (notification != null)
                Destroy(notification);
        }
        
        // イベントハンドラー
        private void OnHealthChanged(float healthRatio)
        {
            UpdateHealthBar(healthRatio);
        }
        
        private void OnManaChanged(float manaRatio)
        {
            UpdateManaBar(manaRatio);
        }
        
        private void OnExperienceChanged(float expRatio)
        {
            UpdateExperienceBar(expRatio);
        }
        
        private void OnDestroy()
        {
            // イベントリスナー解除
            if (onHealthChanged != null)
                onHealthChanged.RemoveListener(OnHealthChanged);
                
            if (onManaChanged != null)
                onManaChanged.RemoveListener(OnManaChanged);
                
            if (onExperienceChanged != null)
                onExperienceChanged.RemoveListener(OnExperienceChanged);
        }
    }
}