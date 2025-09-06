using UnityEngine;
using UnityEngine.UI;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Components;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Core.UI
{
    /// <summary>
    /// ゲーム内HUD（Heads-Up Display）を管理するクラス。
    /// プレイヤーの体力、スタミナ、アイテム情報などを表示します。
    /// </summary>
    public class HUDManager : MonoBehaviour
    {
        [TabGroup("HUD Elements", "Health")]
        [LabelText("Health Bar")]
        [SerializeField] private Slider healthBar;
        
        [TabGroup("HUD Elements", "Health")]
        [LabelText("Health Text")]
        [SerializeField] private Text healthText;
        
        [TabGroup("HUD Elements", "Health")]
        [LabelText("Health Bar Fill Image")]
        [SerializeField] private Image healthBarFill;
        
        [TabGroup("HUD Elements", "Stamina")]
        [LabelText("Stamina Bar")]
        [SerializeField] private Slider staminaBar;
        
        [TabGroup("HUD Elements", "Stamina")]
        [LabelText("Stamina Text")]
        [SerializeField] private Text staminaText;
        
        [TabGroup("HUD Elements", "Info")]
        [LabelText("Score Text")]
        [SerializeField] private Text scoreText;
        
        [TabGroup("HUD Elements", "Info")]
        [LabelText("Level Text")]
        [SerializeField] private Text levelText;
        
        [TabGroup("HUD Elements", "Info")]
        [LabelText("Ammo Text")]
        [SerializeField] private Text ammoText;
        
        [TabGroup("HUD Elements", "Notifications")]
        [LabelText("Notification Panel")]
        [SerializeField] private GameObject notificationPanel;
        
        [TabGroup("HUD Elements", "Notifications")]
        [LabelText("Notification Text")]
        [SerializeField] private Text notificationText;
        
        [TabGroup("HUD Elements", "Crosshair")]
        [LabelText("Crosshair")]
        [SerializeField] private Image crosshair;
        
        [TabGroup("HUD Elements", "Minimap")]
        [LabelText("Minimap")]
        [SerializeField] private RawImage minimap;
        
        [TabGroup("HUD Events", "Health Events")]
        [LabelText("Health Changed")]
        [SerializeField] private GameEvent onHealthChanged;
        
        [TabGroup("HUD Events", "Health Events")]
        [LabelText("Player Damaged")]
        [SerializeField] private GameEvent onPlayerDamaged;
        
        [TabGroup("HUD Events", "Health Events")]
        [LabelText("Player Healed")]
        [SerializeField] private GameEvent onPlayerHealed;
        
        [TabGroup("HUD Events", "Game Events")]
        [LabelText("Score Changed")]
        [SerializeField] private GameEvent onScoreChanged;
        
        [TabGroup("HUD Events", "Game Events")]
        [LabelText("Level Changed")]
        [SerializeField] private GameEvent onLevelChanged;
        
        [TabGroup("HUD Events", "Game Events")]
        [LabelText("Ammo Changed")]
        [SerializeField] private GameEvent onAmmoChanged;
        
        [TabGroup("HUD Settings", "Animation")]
        [LabelText("Health Animation Duration")]
        [PropertyRange(0.1f, 2f)]
        [SuffixLabel("s", overlay: true)]
        [SerializeField] private float healthAnimationDuration = 0.5f;
        
        [TabGroup("HUD Settings", "Animation")]
        [LabelText("Notification Duration")]
        [PropertyRange(1f, 10f)]
        [SuffixLabel("s", overlay: true)]
        [SerializeField] private float notificationDuration = 3f;
        
        [TabGroup("HUD Settings", "Colors")]
        [LabelText("Health Full Color")]
        [SerializeField] private Color healthFullColor = Color.green;
        
        [TabGroup("HUD Settings", "Colors")]
        [LabelText("Health Low Color")]
        [SerializeField] private Color healthLowColor = Color.red;
        
        [TabGroup("HUD Settings", "Colors")]
        [LabelText("Health Critical Threshold")]
        [PropertyRange(0f, 0.5f)]
        [SerializeField] private float healthCriticalThreshold = 0.25f;
        
        [TabGroup("HUD Debug", "Current Values")]
        [ReadOnly]
        [ShowInInspector]
        [LabelText("Current Health")]
        private float currentHealth = 100f;
        
        [TabGroup("HUD Debug", "Current Values")]
        [ReadOnly]
        [ShowInInspector]
        [LabelText("Max Health")]
        private float maxHealth = 100f;
        
        [TabGroup("HUD Debug", "Current Values")]
        [ReadOnly]
        [ShowInInspector]
        [LabelText("Current Score")]
        private int currentScore = 0;
        
        private IHealthTarget playerHealth;
        private Sequence notificationSequence;
        
        private void Awake()
        {
            InitializeHUD();
        }
        
        private void OnEnable()
        {
            SubscribeToEvents();
        }
        
        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }
        
        private void OnDestroy()
        {
            notificationSequence?.Kill();
        }
        
        /// <summary>
        /// HUDを初期化します
        /// </summary>
        private void InitializeHUD()
        {
            // プレイヤーの体力コンポーネントを取得
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerHealth = player.GetComponent<IHealthTarget>();
            }
            
            // 初期値を設定
            UpdateHealthDisplay(currentHealth, maxHealth);
            UpdateScore(currentScore);
            
            // 通知パネルを非表示に
            if (notificationPanel != null)
                notificationPanel.SetActive(false);
        }
        
        /// <summary>
        /// イベントに購読します
        /// GameEventListenerコンポーネントを使用した購読システムを想定
        /// 実際のプロジェクトでは各GameEventにGameEventListenerを設定する必要があります
        /// </summary>
        private void SubscribeToEvents()
        {
            // 注意: このプロジェクトのGameEventシステムは
            // GameEventListenerコンポーネントを使用して購読を行います
            // 各イベントに対して適切なGameEventListenerを設定してください
            
            // 例: onHealthChangedイベント用のGameEventListenerを設定し、
            // そのResponseでOnHealthChangedを呼び出すようにします
        }
        
        /// <summary>
        /// イベントから購読解除します
        /// GameEventListenerコンポーネントを使用している場合は
        /// コンポーネントが自動的に購読解除を処理します
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            // GameEventListenerを使用している場合、
            // OnDisableで自動的に購読解除されます
        }
        
        /// <summary>
        /// 体力変更イベントのハンドラ
        /// </summary>
        private void OnHealthChanged()
        {
            if (playerHealth != null)
            {
                UpdateHealthDisplay(playerHealth.CurrentHealth, playerHealth.MaxHealth);
            }
        }
        
        /// <summary>
        /// ダメージイベントのハンドラ
        /// </summary>
        private void OnPlayerDamaged()
        {
            PlayDamageEffect();
        }
        
        /// <summary>
        /// 回復イベントのハンドラ
        /// </summary>
        private void OnPlayerHealed()
        {
            PlayHealEffect();
        }
        
        /// <summary>
        /// スコア変更イベントのハンドラ
        /// </summary>
        private void OnScoreChanged()
        {
            // スコアの実際の値はGameManagerから取得する想定
            // ここでは仮の実装
        }
        
        /// <summary>
        /// レベル変更イベントのハンドラ
        /// </summary>
        private void OnLevelChanged()
        {
            // レベルの実際の値はGameManagerから取得する想定
        }
        
        /// <summary>
        /// 弾薬変更イベントのハンドラ
        /// </summary>
        private void OnAmmoChanged()
        {
            // 弾薬の実際の値は武器システムから取得する想定
        }
        
        /// <summary>
        /// 体力表示を更新します
        /// </summary>
        public void UpdateHealthDisplay(float health, float maxHealth)
        {
            currentHealth = health;
            this.maxHealth = maxHealth;
            
            float healthPercentage = health / maxHealth;
            
            // 体力バーを更新
            if (healthBar != null)
            {
                healthBar.value = healthPercentage;
            }
            
            // 体力テキストを更新
            if (healthText != null)
            {
                healthText.text = $"{health:F0}/{maxHealth:F0}";
            }
            
            // 体力バーの色を更新
            if (healthBarFill != null)
            {
                Color targetColor = healthPercentage > healthCriticalThreshold ? 
                    Color.Lerp(healthLowColor, healthFullColor, healthPercentage) : 
                    healthLowColor;
                
                healthBarFill.color = targetColor;
            }
        }
        
        /// <summary>
        /// スコアを更新します
        /// </summary>
        public void UpdateScore(int score)
        {
            currentScore = score;
            
            if (scoreText != null)
            {
                scoreText.text = $"Score: {score:N0}";
                
                // スコア変更時のパルスアニメーション
                scoreText.transform.DOPunchScale(Vector3.one * 0.1f, 0.3f)
                    .SetEase(Ease.OutElastic);
            }
        }
        
        /// <summary>
        /// レベルを更新します
        /// </summary>
        public void UpdateLevel(int level)
        {
            if (levelText != null)
            {
                levelText.text = $"Level: {level}";
            }
        }
        
        /// <summary>
        /// 弾薬情報を更新します
        /// </summary>
        public void UpdateAmmo(int currentAmmo, int maxAmmo)
        {
            if (ammoText != null)
            {
                ammoText.text = $"{currentAmmo}/{maxAmmo}";
                
                // 弾薬不足時の警告色
                if (currentAmmo <= maxAmmo * 0.1f)
                {
                    ammoText.color = Color.red;
                }
                else
                {
                    ammoText.color = Color.white;
                }
            }
        }
        
        /// <summary>
        /// 通知を表示します
        /// </summary>
        public void ShowNotification(string message)
        {
            if (notificationPanel == null || notificationText == null) return;
            
            notificationSequence?.Kill();
            
            notificationText.text = message;
            notificationPanel.SetActive(true);
            
            var canvasGroup = notificationPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = notificationPanel.AddComponent<CanvasGroup>();
            }
            
            // Simple notification display without animation
            canvasGroup.alpha = 1f;
            StartCoroutine(HideNotificationAfterDelay(notificationDuration));
        }/// <summary>
        /// ダメージエフェクトを再生します
        /// </summary>
        private void PlayDamageEffect()
        {
            if (healthBarFill != null)
            {
                healthBarFill.color = Color.red;
            }
        }
        
        /// <summary>
        /// 回復エフェクトを再生します
        /// </summary>
        private void PlayHealEffect()
        {
            if (healthBarFill != null)
            {
                healthBarFill.color = Color.green;
            }
        }
        
        /// <summary>
        /// クロスヘアの表示を切り替えます
        /// </summary>
        public void SetCrosshairVisible(bool visible)
        {
            if (crosshair != null)
            {
                crosshair.gameObject.SetActive(visible);
            }
        }
        
        #if UNITY_EDITOR
        [TabGroup("HUD Debug", "Test Functions")]
        [Button("Test Damage")]
        private void TestDamage()
        {
            if (Application.isPlaying)
            {
                UpdateHealthDisplay(Mathf.Max(0, currentHealth - 25), maxHealth);
                PlayDamageEffect();
            }
        }
        
        [TabGroup("HUD Debug", "Test Functions")]
        [Button("Test Heal")]
        private void TestHeal()
        {
            if (Application.isPlaying)
            {
                UpdateHealthDisplay(Mathf.Min(maxHealth, currentHealth + 25), maxHealth);
                PlayHealEffect();
            }
        }
        
        [TabGroup("HUD Debug", "Test Functions")]
        [Button("Test Notification")]
        private void TestNotification()
        {
            if (Application.isPlaying)
            {
                ShowNotification("テスト通知メッセージ");
            }
        }
        #endif
        
        private System.Collections.IEnumerator HideNotificationAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (notificationPanel != null)
            {
                notificationPanel.SetActive(false);
            }
        }
    }
}