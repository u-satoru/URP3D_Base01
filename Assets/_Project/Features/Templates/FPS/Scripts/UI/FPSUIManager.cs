using UnityEngine;
using UnityEngine.UI;
// using TMPro; // Commented out to avoid dependency issues
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Features.Templates.FPS.Player;
using asterivo.Unity60.Features.Templates.FPS.Weapons;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Features.Templates.FPS.UI
{
    /// <summary>
    /// FPS専用UI管理システム
    /// クロスヘア、弾薬表示、ヘルス表示、ミニマップなど
    /// FPSゲーム特有のHUD要素を統合管理
    /// </summary>
    public class FPSUIManager : MonoBehaviour
    {
        [TabGroup("FPS UI", "Core Components")]
        [LabelText("FPS Player Controller")]
        [SerializeField] private FPSPlayerController fpsPlayer;
        
        [LabelText("Current Weapon")]
        [SerializeField] private WeaponSystem currentWeapon;
        
        [TabGroup("FPS UI", "Crosshair")]
        [BoxGroup("FPS UI/Crosshair/Elements")]
        [LabelText("Crosshair Image")]
        [SerializeField] private Image crosshairImage;
        
        [BoxGroup("FPS UI/Crosshair/Settings")]
        [LabelText("Default Color")]
        [SerializeField] private Color defaultCrosshairColor = Color.white;
        
        [BoxGroup("FPS UI/Crosshair/Settings")]
        [LabelText("Hit Color")]
        [SerializeField] private Color hitCrosshairColor = Color.red;
        
        [BoxGroup("FPS UI/Crosshair/Settings")]
        [LabelText("Aim Size Multiplier")]
        [PropertyRange(0.3f, 1f)]
        [SerializeField] private float aimSizeMultiplier = 0.6f;
        
        [BoxGroup("FPS UI/Crosshair/Animation")]
        [LabelText("Color Change Duration")]
        [PropertyRange(0.1f, 1f)]
        [SerializeField] private float colorChangeDuration = 0.2f;
        
        [BoxGroup("FPS UI/Crosshair/Animation")]
        [LabelText("Size Change Speed")]
        [PropertyRange(1f, 10f)]
        [SerializeField] private float sizeChangeSpeed = 5f;
        
        [TabGroup("FPS UI", "Ammo Display")]
        [BoxGroup("FPS UI/Ammo Display/Elements")]
        [LabelText("Current Ammo Text")]
        [SerializeField] private Text currentAmmoText;
        
        [BoxGroup("FPS UI/Ammo Display/Elements")]
        [LabelText("Reserve Ammo Text")]
        [SerializeField] private Text reserveAmmoText;
        
        [BoxGroup("FPS UI/Ammo Display/Elements")]
        [LabelText("Weapon Name Text")]
        [SerializeField] private Text weaponNameText;
        
        [BoxGroup("FPS UI/Ammo Display/Colors")]
        [LabelText("Normal Ammo Color")]
        [SerializeField] private Color normalAmmoColor = Color.white;
        
        [BoxGroup("FPS UI/Ammo Display/Colors")]
        [LabelText("Low Ammo Color")]
        [SerializeField] private Color lowAmmoColor = Color.yellow;
        
        [BoxGroup("FPS UI/Ammo Display/Colors")]
        [LabelText("Critical Ammo Color")]
        [SerializeField] private Color criticalAmmoColor = Color.red;
        
        [BoxGroup("FPS UI/Ammo Display/Thresholds")]
        [LabelText("Low Ammo Threshold")]
        [PropertyRange(0.1f, 0.5f)]
        [SerializeField] private float lowAmmoThreshold = 0.3f;
        
        [BoxGroup("FPS UI/Ammo Display/Thresholds")]
        [LabelText("Critical Ammo Threshold")]
        [PropertyRange(0.05f, 0.2f)]
        [SerializeField] private float criticalAmmoThreshold = 0.1f;
        
        [TabGroup("FPS UI", "Health Display")]
        [BoxGroup("FPS UI/Health Display/Elements")]
        [LabelText("Health Bar")]
        [SerializeField] private Slider healthBar;
        
        [BoxGroup("FPS UI/Health Display/Elements")]
        [LabelText("Health Text")]
        [SerializeField] private Text healthText;
        
        [BoxGroup("FPS UI/Health Display/Elements")]
        [LabelText("Health Bar Fill")]
        [SerializeField] private Image healthBarFill;
        
        [BoxGroup("FPS UI/Health Display/Colors")]
        [LabelText("Healthy Color")]
        [SerializeField] private Color healthyColor = Color.green;
        
        [BoxGroup("FPS UI/Health Display/Colors")]
        [LabelText("Damaged Color")]
        [SerializeField] private Color damagedColor = Color.yellow;
        
        [BoxGroup("FPS UI/Health Display/Colors")]
        [LabelText("Critical Health Color")]
        [SerializeField] private Color criticalHealthColor = Color.red;
        
        [TabGroup("FPS UI", "Hit Indicator")]
        [BoxGroup("FPS UI/Hit Indicator/Elements")]
        [LabelText("Hit Indicator Panel")]
        [SerializeField] private Image hitIndicatorPanel;
        
        [BoxGroup("FPS UI/Hit Indicator/Settings")]
        [LabelText("Hit Flash Duration")]
        [PropertyRange(0.1f, 1f)]
        [SerializeField] private float hitFlashDuration = 0.3f;
        
        [BoxGroup("FPS UI/Hit Indicator/Settings")]
        [LabelText("Hit Indicator Color")]
        [SerializeField] private Color hitIndicatorColor = new Color(1, 0, 0, 0.3f);
        
        [TabGroup("FPS UI", "Reticle & Minimap")]
        [BoxGroup("FPS UI/Reticle & Minimap/Elements")]
        [LabelText("Minimap Camera")]
        [SerializeField] private UnityEngine.Camera minimapCamera;
        
        [BoxGroup("FPS UI/Reticle & Minimap/Elements")]
        [LabelText("Minimap Image")]
        [SerializeField] private RawImage minimapImage;
        
        [TabGroup("Events", "UI Events")]
        [LabelText("On UI Update")]
        [SerializeField] private GameEvent onUIUpdate;
        
        // Private variables for UI state
        private Vector3 originalCrosshairScale;
        private Color currentCrosshairColor;
        private float crosshairColorTimer;
        private bool isShowingHitIndicator;
        
        // References
        private Canvas uiCanvas;
        
        [TabGroup("Debug", "UI Status")]
        [ReadOnly]
        [ShowInInspector]
        #pragma warning disable 0414 // Suppress unused field warning for Inspector display
        [LabelText("Current Health %")]
        private float debugHealthPercentage;
        
        [ReadOnly]
        [ShowInInspector]
        [LabelText("Current Ammo %")]
        private float debugAmmoPercentage;
        
        private void Start()
        {
            InitializeUI();
            SetupEventListeners();
        }
        
        private void Update()
        {
            UpdateUI();
            UpdateCrosshairColor();
            UpdateDebugInfo();
        }
        
        private void InitializeUI()
        {
            // UI Canvas取得
            uiCanvas = GetComponentInParent<Canvas>();
            
            // クロスヘアの初期設定
            if (crosshairImage != null)
            {
                originalCrosshairScale = crosshairImage.transform.localScale;
                currentCrosshairColor = defaultCrosshairColor;
                crosshairImage.color = currentCrosshairColor;
            }
            
            // ヒットインジケーターを非表示に
            if (hitIndicatorPanel != null)
            {
                hitIndicatorPanel.gameObject.SetActive(false);
            }
            
            // ミニマップの設定
            SetupMinimap();
        }
        
        private void SetupEventListeners()
        {
            // 武器発射イベント
            // TODO: 実際のイベントシステムと連携
            
            // 健康状態変更イベント
            // TODO: 実際のヘルスシステムと連携
        }
        
        private void UpdateUI()
        {
            UpdateAmmoDisplay();
            UpdateHealthDisplay();
            UpdateCrosshair();
        }
        
        private void UpdateAmmoDisplay()
        {
            if (currentWeapon == null) return;
            
            // 弾薬数の表示
            if (currentAmmoText != null)
            {
                currentAmmoText.text = currentWeapon.CurrentAmmo.ToString();
                
                // 弾薬残量による色変更
                float ammoPercentage = (float)currentWeapon.CurrentAmmo / currentWeapon.MaxAmmo;
                
                if (ammoPercentage <= criticalAmmoThreshold)
                {
                    currentAmmoText.color = criticalAmmoColor;
                }
                else if (ammoPercentage <= lowAmmoThreshold)
                {
                    currentAmmoText.color = lowAmmoColor;
                }
                else
                {
                    currentAmmoText.color = normalAmmoColor;
                }
            }
            
            // リザーブ弾薬の表示
            if (reserveAmmoText != null)
            {
                reserveAmmoText.text = currentWeapon.ReserveAmmo.ToString();
            }
            
            // 武器名の表示
            if (weaponNameText != null && currentWeapon.WeaponData != null)
            {
                weaponNameText.text = currentWeapon.WeaponData.WeaponName;
            }
        }
        
        private void UpdateHealthDisplay()
        {
            if (fpsPlayer == null) return;
            
            // TODO: FPSPlayerControllerからヘルス情報を取得
            // 現在は仮の値を使用
            float currentHealth = 75f; // 仮の値
            float maxHealth = 100f; // 仮の値
            
            float healthPercentage = currentHealth / maxHealth;
            
            // ヘルスバーの更新
            if (healthBar != null)
            {
                healthBar.value = healthPercentage;
            }
            
            // ヘルステキストの更新
            if (healthText != null)
            {
                healthText.text = $"{currentHealth:F0}/{maxHealth:F0}";
            }
            
            // ヘルスバーの色変更
            if (healthBarFill != null)
            {
                if (healthPercentage > 0.6f)
                    healthBarFill.color = healthyColor;
                else if (healthPercentage > 0.3f)
                    healthBarFill.color = damagedColor;
                else
                    healthBarFill.color = criticalHealthColor;
            }
        }
        
        private void UpdateCrosshair()
        {
            if (crosshairImage == null || currentWeapon == null) return;
            
            // エイム中のクロスヘアサイズ変更
            float targetScale = currentWeapon.IsAiming ? aimSizeMultiplier : 1f;
            Vector3 currentScale = crosshairImage.transform.localScale;
            Vector3 newScale = Vector3.Lerp(currentScale, 
                originalCrosshairScale * targetScale, 
                sizeChangeSpeed * Time.deltaTime);
            
            crosshairImage.transform.localScale = newScale;
        }
        
        private void UpdateCrosshairColor()
        {
            if (crosshairImage == null) return;
            
            // 色変更のタイマー処理
            if (crosshairColorTimer > 0)
            {
                crosshairColorTimer -= Time.deltaTime;
                if (crosshairColorTimer <= 0)
                {
                    currentCrosshairColor = defaultCrosshairColor;
                }
            }
            
            // 色の適用
            crosshairImage.color = Color.Lerp(crosshairImage.color, currentCrosshairColor, 
                Time.deltaTime / colorChangeDuration);
        }
        
        private void SetupMinimap()
        {
            if (minimapCamera != null && minimapImage != null)
            {
                // ミニマップカメラの設定
                minimapCamera.orthographic = true;
                minimapCamera.orthographicSize = 50f;
                minimapCamera.farClipPlane = 100f;
                
                // レンダーテクスチャの作成
                RenderTexture minimapTexture = new RenderTexture(256, 256, 16);
                minimapCamera.targetTexture = minimapTexture;
                minimapImage.texture = minimapTexture;
            }
        }
        
        public void SetCurrentWeapon(WeaponSystem weapon)
        {
            currentWeapon = weapon;
            if (onUIUpdate != null)
                onUIUpdate.Raise();
        }
        
        public void ShowHitMarker()
        {
            if (crosshairImage != null)
            {
                currentCrosshairColor = hitCrosshairColor;
                crosshairColorTimer = colorChangeDuration;
            }
        }
        
        public void ShowDamageIndicator()
        {
            if (hitIndicatorPanel != null && !isShowingHitIndicator)
            {
                StartCoroutine(ShowHitIndicatorCoroutine());
            }
        }
        
        private System.Collections.IEnumerator ShowHitIndicatorCoroutine()
        {
            isShowingHitIndicator = true;
            hitIndicatorPanel.gameObject.SetActive(true);
            hitIndicatorPanel.color = hitIndicatorColor;
            
            // フェードアウト
            float timer = 0f;
            Color startColor = hitIndicatorColor;
            Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);
            
            while (timer < hitFlashDuration)
            {
                timer += Time.deltaTime;
                float progress = timer / hitFlashDuration;
                hitIndicatorPanel.color = Color.Lerp(startColor, endColor, progress);
                yield return null;
            }
            
            hitIndicatorPanel.gameObject.SetActive(false);
            isShowingHitIndicator = false;
        }
        
        public void UpdateMinimap(Vector3 playerPosition, float playerRotation)
        {
            if (minimapCamera != null)
            {
                // プレイヤーの位置をミニマップカメラに反映
                Vector3 minimapPosition = new Vector3(playerPosition.x, minimapCamera.transform.position.y, playerPosition.z);
                minimapCamera.transform.position = minimapPosition;
                minimapCamera.transform.rotation = Quaternion.Euler(90f, playerRotation, 0f);
            }
        }
        
        private void UpdateDebugInfo()
        {
            if (currentWeapon != null)
            {
                debugAmmoPercentage = (float)currentWeapon.CurrentAmmo / currentWeapon.MaxAmmo * 100f;
            }
            
            // TODO: 実際のヘルス値からの計算
            debugHealthPercentage = 75f; // 仮の値
        }
        
        // 外部からの呼び出し用メソッド
        public void OnWeaponFired()
        {
            ShowHitMarker();
        }
        
        public void OnPlayerDamaged()
        {
            ShowDamageIndicator();
        }
        
        public void OnReloadStarted()
        {
            // リロードアニメーションの開始
            if (currentAmmoText != null)
            {
                // TODO: リロードアニメーション実装
            }
        }
        
        public void OnReloadCompleted()
        {
            // リロードアニメーションの完了
            if (onUIUpdate != null)
                onUIUpdate.Raise();
        }
    }
}