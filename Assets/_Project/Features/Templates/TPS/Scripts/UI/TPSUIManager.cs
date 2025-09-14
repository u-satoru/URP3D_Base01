using UnityEngine;
using UnityEngine.UI;
using TMPro;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Features.Templates.FPS.Weapons;
using asterivo.Unity60.Features.Templates.TPS.Player;
using asterivo.Unity60.Features.Templates.TPS.Camera;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Features.Templates.TPS.UI
{
    /// <summary>
    /// TPS専用UIマネージャー
    /// 三人称視点に最適化されたHUD、クロスヘア、カバー状態表示
    /// FPS Template UIシステムの95%再利用でTPS特化UI機能を実現
    /// </summary>
    public class TPSUIManager : MonoBehaviour
    {
        [TabGroup("TPS UI", "HUD Elements")]
        [BoxGroup("TPS UI/HUD Elements/Health & Ammo")]
        [LabelText("Health Bar")]
        [SerializeField] private Slider healthBar;

        [BoxGroup("TPS UI/HUD Elements/Health & Ammo")]
        [LabelText("Health Text")]
        [SerializeField] private TextMeshProUGUI healthText;

        [BoxGroup("TPS UI/HUD Elements/Health & Ammo")]
        [LabelText("Ammo Counter")]
        [SerializeField] private TextMeshProUGUI ammoText;

        [BoxGroup("TPS UI/HUD Elements/Health & Ammo")]
        [LabelText("Weapon Name")]
        [SerializeField] private TextMeshProUGUI weaponNameText;

        [TabGroup("TPS UI", "TPS Specific")]
        [BoxGroup("TPS UI/TPS Specific/Cover System")]
        [LabelText("Cover Status Panel")]
        [SerializeField] private GameObject coverStatusPanel;

        [BoxGroup("TPS UI/TPS Specific/Cover System")]
        [LabelText("Cover Status Text")]
        [SerializeField] private TextMeshProUGUI coverStatusText;

        [BoxGroup("TPS UI/TPS Specific/Cover System")]
        [LabelText("Cover Status Icon")]
        [SerializeField] private Image coverStatusIcon;

        [BoxGroup("TPS UI/TPS Specific/Cover System")]
        [LabelText("Peek Direction Indicator")]
        [SerializeField] private Image peekDirectionIndicator;

        [BoxGroup("TPS UI/TPS Specific/Crosshair")]
        [LabelText("TPS Crosshair")]
        [SerializeField] private Image tpsCrosshair;

        [BoxGroup("TPS UI/TPS Specific/Crosshair")]
        [LabelText("Dynamic Crosshair")]
        [SerializeField] private bool dynamicCrosshair = true;

        [BoxGroup("TPS UI/TPS Specific/Crosshair")]
        [LabelText("Aiming Crosshair")]
        [SerializeField] private Image aimingCrosshair;

        [TabGroup("TPS UI", "Game State")]
        [BoxGroup("TPS UI/Game State/Score")]
        [LabelText("Kill Counter")]
        [SerializeField] private TextMeshProUGUI killCounterText;

        [BoxGroup("TPS UI/Game State/Score")]
        [LabelText("Timer")]
        [SerializeField] private TextMeshProUGUI timerText;

        [BoxGroup("TPS UI/Game State/Objective")]
        [LabelText("Objective Text")]
        [SerializeField] private TextMeshProUGUI objectiveText;

        [BoxGroup("TPS UI/Game State/Objective")]
        [LabelText("Progress Bar")]
        [SerializeField] private Slider progressBar;

        [TabGroup("TPS UI", "Interaction")]
        [BoxGroup("TPS UI/Interaction/Prompts")]
        [LabelText("Interaction Prompt")]
        [SerializeField] private GameObject interactionPrompt;

        [BoxGroup("TPS UI/Interaction/Prompts")]
        [LabelText("Interaction Text")]
        [SerializeField] private TextMeshProUGUI interactionText;

        [BoxGroup("TPS UI/Interaction/Prompts")]
        [LabelText("Take Cover Prompt")]
        [SerializeField] private GameObject takeCoverPrompt;

        [TabGroup("Events", "UI Events")]
        [LabelText("On UI Update")]
        [SerializeField] private GameEvent onUIUpdate;

        [LabelText("On Cover Status Changed")]
        [SerializeField] private GameEvent onCoverStatusChanged;

        [LabelText("On Crosshair Update")]
        [SerializeField] private GameEvent onCrosshairUpdate;

        // Private references
        private TPSPlayerController tpsPlayer;
        private TPSCameraController tpsCamera;
        private WeaponSystem currentWeapon;
        private TPSTemplateManager templateManager;

        // UI State
        private bool isInCover;
        private bool isAiming;
        private bool isPeeking;
        private string currentCoverStatus = "No Cover";
        private Color originalCrosshairColor;

        [TabGroup("Debug", "UI State")]
        [ReadOnly]
        [ShowInInspector]
        [LabelText("Cover Status")]
        private string debugCoverStatus => currentCoverStatus;

        [ReadOnly]
        [ShowInInspector]
        [LabelText("Is Aiming")]
        private bool debugIsAiming => isAiming;

        [ReadOnly]
        [ShowInInspector]
        [LabelText("Current Weapon")]
        private string debugCurrentWeapon => currentWeapon != null ? currentWeapon.name : "None";

        private void Awake()
        {
            InitializeUIReferences();
        }

        private void Start()
        {
            InitializeTPS();
            SetupEventListeners();
            InitializeUI();
        }

        private void Update()
        {
            UpdateHUD();
            UpdateCoverUI();
            UpdateCrosshair();
            UpdateInteractionPrompts();
        }

        private void InitializeUIReferences()
        {
            // Find TPS components
            tpsPlayer = FindFirstObjectByType<TPSPlayerController>();
            tpsCamera = FindFirstObjectByType<TPSCameraController>();
            templateManager = FindFirstObjectByType<TPSTemplateManager>();

            // Store original crosshair color
            if (tpsCrosshair != null)
            {
                originalCrosshairColor = tpsCrosshair.color;
            }
        }

        private void InitializeTPS()
        {
            // Initialize TPS-specific UI elements
            if (coverStatusPanel != null)
            {
                coverStatusPanel.SetActive(false);
            }

            if (takeCoverPrompt != null)
            {
                takeCoverPrompt.SetActive(false);
            }

            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }

            // Set initial objective text
            if (objectiveText != null && templateManager != null)
            {
                objectiveText.text = $"Eliminate {templateManager.KillTarget} enemies";
            }

            UnityEngine.Debug.Log("[TPS UI] TPSUIManager initialized");
        }

        private void SetupEventListeners()
        {
            // TODO: Setup event listeners when TPS-specific events are implemented
            // This will integrate with the GameEvent system for loose coupling
        }

        private void InitializeUI()
        {
            // Initialize all UI elements to default states
            UpdateHealthDisplay(100);
            UpdateAmmoDisplay(0, 0);
            UpdateKillCounter(0);
            UpdateTimer(900f); // 15 minutes
            UpdateProgressBar(0f);
        }

        private void UpdateHUD()
        {
            if (templateManager != null)
            {
                UpdateKillCounter(templateManager.CurrentKills);
                UpdateTimer(templateManager.TimeRemaining);
                UpdateProgressBar((float)templateManager.CurrentKills / templateManager.KillTarget);
            }

            if (tpsPlayer != null)
            {
                // Update cover status from player
                bool playerInCover = tpsPlayer.IsInCover;
                bool playerAiming = tpsPlayer.IsAiming;
                bool playerPeeking = tpsPlayer.IsPeeking;

                if (isInCover != playerInCover || isAiming != playerAiming || isPeeking != playerPeeking)
                {
                    isInCover = playerInCover;
                    isAiming = playerAiming;
                    isPeeking = playerPeeking;
                    
                    UpdateCoverStatusDisplay();
                }

                // Update weapon info
                WeaponSystem playerWeapon = tpsPlayer.CurrentWeapon;
                if (currentWeapon != playerWeapon)
                {
                    currentWeapon = playerWeapon;
                    UpdateWeaponDisplay();
                }
            }
        }

        private void UpdateCoverUI()
        {
            if (coverStatusPanel == null) return;

            // Show/hide cover panel based on cover state
            bool shouldShowCoverUI = isInCover || isPeeking;
            if (coverStatusPanel.activeSelf != shouldShowCoverUI)
            {
                coverStatusPanel.SetActive(shouldShowCoverUI);
            }

            if (shouldShowCoverUI)
            {
                UpdateCoverStatusDisplay();
                UpdatePeekIndicator();
            }
        }

        private void UpdateCoverStatusDisplay()
        {
            string newStatus;
            Color statusColor = Color.white;

            if (isPeeking)
            {
                newStatus = "Peeking";
                statusColor = Color.yellow;
            }
            else if (isInCover)
            {
                newStatus = "In Cover";
                statusColor = Color.green;
            }
            else
            {
                newStatus = "Exposed";
                statusColor = Color.red;
            }

            if (currentCoverStatus != newStatus)
            {
                currentCoverStatus = newStatus;
                
                if (coverStatusText != null)
                {
                    coverStatusText.text = currentCoverStatus;
                    coverStatusText.color = statusColor;
                }

                if (coverStatusIcon != null)
                {
                    coverStatusIcon.color = statusColor;
                }

                onCoverStatusChanged?.Raise();
            }
        }

        private void UpdatePeekIndicator()
        {
            if (peekDirectionIndicator == null) return;

            if (isPeeking)
            {
                peekDirectionIndicator.gameObject.SetActive(true);
                // Update peek direction based on player peek direction
                // This would integrate with the player's peek system
            }
            else
            {
                peekDirectionIndicator.gameObject.SetActive(false);
            }
        }

        private void UpdateCrosshair()
        {
            if (tpsCrosshair == null) return;

            // Dynamic crosshair based on aiming state
            if (dynamicCrosshair)
            {
                if (isAiming)
                {
                    // Smaller, more precise crosshair when aiming
                    tpsCrosshair.transform.localScale = Vector3.one * 0.7f;
                    tpsCrosshair.color = Color.white;
                    
                    // Show aiming crosshair if available
                    if (aimingCrosshair != null)
                    {
                        aimingCrosshair.gameObject.SetActive(true);
                        tpsCrosshair.gameObject.SetActive(false);
                    }
                }
                else
                {
                    // Standard crosshair when not aiming
                    tpsCrosshair.transform.localScale = Vector3.one;
                    tpsCrosshair.color = originalCrosshairColor;
                    
                    if (aimingCrosshair != null)
                    {
                        aimingCrosshair.gameObject.SetActive(false);
                        tpsCrosshair.gameObject.SetActive(true);
                    }
                }

                // Cover state affects crosshair
                if (isInCover && !isPeeking)
                {
                    tpsCrosshair.color = Color.gray; // Dimmed when in cover
                }
            }

            onCrosshairUpdate?.Raise();
        }

        private void UpdateInteractionPrompts()
        {
            // TODO: Implement interaction prompt logic
            // This would show prompts for taking cover, weapon pickup, etc.
        }

        // Public update methods for external systems
        public void UpdateHealthDisplay(int currentHealth, int maxHealth = 100)
        {
            if (healthBar != null)
            {
                healthBar.value = (float)currentHealth / maxHealth;
            }

            if (healthText != null)
            {
                healthText.text = $"{currentHealth}/{maxHealth}";
            }
        }

        public void UpdateAmmoDisplay(int currentAmmo, int totalAmmo)
        {
            if (ammoText != null)
            {
                ammoText.text = $"{currentAmmo}/{totalAmmo}";
            }
        }

        public void UpdateKillCounter(int kills)
        {
            if (killCounterText != null && templateManager != null)
            {
                killCounterText.text = $"Kills: {kills}/{templateManager.KillTarget}";
            }
        }

        public void UpdateTimer(float timeRemaining)
        {
            if (timerText != null)
            {
                int minutes = Mathf.FloorToInt(timeRemaining / 60f);
                int seconds = Mathf.FloorToInt(timeRemaining % 60f);
                timerText.text = $"{minutes:D2}:{seconds:D2}";
            }
        }

        public void UpdateProgressBar(float progress)
        {
            if (progressBar != null)
            {
                progressBar.value = Mathf.Clamp01(progress);
            }
        }

        private void UpdateWeaponDisplay()
        {
            if (weaponNameText != null)
            {
                weaponNameText.text = currentWeapon != null ? currentWeapon.name : "No Weapon";
            }

            // Update ammo display if weapon has ammo system
            if (currentWeapon != null)
            {
                // TODO: Integrate with weapon ammo system when available
                UpdateAmmoDisplay(30, 120); // Placeholder values
            }
        }

        // Public interface for template manager
        public void Initialize(TPSTemplateManager manager)
        {
            templateManager = manager;
            InitializeTPS();

            UnityEngine.Debug.Log("[TPS UI] TPSUIManager initialized with template manager");
        }

        public void ShowInteractionPrompt(string message)
        {
            if (interactionPrompt != null && interactionText != null)
            {
                interactionText.text = message;
                interactionPrompt.SetActive(true);
            }
        }

        public void HideInteractionPrompt()
        {
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }
        }

        public void ShowTakeCoverPrompt(bool show)
        {
            if (takeCoverPrompt != null)
            {
                takeCoverPrompt.SetActive(show);
            }
        }

        // Event handlers for GameEvent integration
        public void OnPlayerTakeCover()
        {
            isInCover = true;
            UpdateCoverStatusDisplay();
            UnityEngine.Debug.Log("[TPS UI] Player took cover - UI updated");
        }

        public void OnPlayerLeaveCover()
        {
            isInCover = false;
            isPeeking = false;
            UpdateCoverStatusDisplay();
            UnityEngine.Debug.Log("[TPS UI] Player left cover - UI updated");
        }

        public void OnPlayerStartAiming()
        {
            isAiming = true;
            UpdateCrosshair();
            UnityEngine.Debug.Log("[TPS UI] Player started aiming - crosshair updated");
        }

        public void OnPlayerStopAiming()
        {
            isAiming = false;
            UpdateCrosshair();
            UnityEngine.Debug.Log("[TPS UI] Player stopped aiming - crosshair updated");
        }

        public void OnEnemyKilled()
        {
            if (templateManager != null)
            {
                UpdateKillCounter(templateManager.CurrentKills);
                UpdateProgressBar((float)templateManager.CurrentKills / templateManager.KillTarget);
            }
        }

        public void OnGameStateChanged()
        {
            // Update UI based on game state changes
            onUIUpdate?.Raise();
        }

        // TPS-specific UI properties
        public bool IsShowingCoverUI => coverStatusPanel != null && coverStatusPanel.activeSelf;
        public string CurrentCoverStatus => currentCoverStatus;
        public bool DynamicCrosshair => dynamicCrosshair;
    }
}