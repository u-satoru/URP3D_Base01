using UnityEngine;
using UnityEngine.UI;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Features.UI;
using asterivo.Unity60.Features.Templates.TPS.Combat;
using asterivo.Unity60.Features.Templates.TPS.Combat.Data;
using asterivo.Unity60.Features.Templates.TPS.Data;
using asterivo.Unity60.Features.Templates.TPS.Player;
using asterivo.Unity60.Features.Templates.TPS.Player.StateMachine;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Features.Templates.TPS.UI
{
    /// <summary>
    /// TPS Template HUD Controller - Manages TPS-specific UI elements
    /// Integrates with Core HUDManager and ServiceLocator pattern
    /// Follows event-driven architecture for responsive UI updates
    /// </summary>
    public class TPSHUDController : MonoBehaviour,
        IGameEventListener<PlayerState>,
        IGameEventListener<WeaponData>,
        IGameEventTwoArgsListener<int, int>,
        IGameEventListener<bool>
    {
        [TabGroup("Core Integration", "HUD Manager")]
        [LabelText("Core HUD Manager")]
        [SerializeField] private HUDManager coreHUDManager;

        [TabGroup("TPS Elements", "Weapon")]
        [LabelText("Weapon Name Text")]
        [SerializeField] private Text weaponNameText;
        
        [TabGroup("TPS Elements", "Weapon")]
        [LabelText("Ammo Display")]
        [SerializeField] private Text ammoDisplayText;
        
        [TabGroup("TPS Elements", "Weapon")]
        [LabelText("Reload Indicator")]
        [SerializeField] private GameObject reloadIndicator;
        
        [TabGroup("TPS Elements", "Cover")]
        [LabelText("Cover Indicator")]
        [SerializeField] private Image coverIndicator;
        
        [TabGroup("TPS Elements", "Cover")]
        [LabelText("Peek Direction Indicator")]
        [SerializeField] private Image peekDirectionIndicator;
        
        [TabGroup("TPS Elements", "Crosshair")]
        [LabelText("Dynamic Crosshair")]
        [SerializeField] private RectTransform dynamicCrosshair;
        
        [TabGroup("TPS Elements", "Crosshair")]
        [LabelText("Aim Crosshair")]
        [SerializeField] private Image aimCrosshair;
        
        [TabGroup("TPS Elements", "Interaction")]
        [LabelText("Interaction Prompt")]
        [SerializeField] private GameObject interactionPrompt;
        
        [TabGroup("TPS Elements", "Interaction")]
        [LabelText("Interaction Text")]
        [SerializeField] private Text interactionText;

        [TabGroup("TPS Events", "Player Events")]
        [LabelText("Player State Changed")]
        [SerializeField] private GameEvent<PlayerState> onPlayerStateChanged;
        
        [TabGroup("TPS Events", "Weapon Events")]
        [LabelText("Weapon Equipped")]
        [SerializeField] private GameEvent<WeaponData> onWeaponEquipped;
        
        [TabGroup("TPS Events", "Weapon Events")]
        [LabelText("Ammo Changed")]
        [SerializeField] private GameEvent<int, int> onAmmoChanged;
        
        [TabGroup("TPS Events", "Cover Events")]
        [LabelText("Cover State Changed")]
        [SerializeField] private GameEvent<bool> onCoverStateChanged;

        [TabGroup("TPS Settings", "Crosshair")]
        [LabelText("Crosshair Spread Scale")]
        [PropertyRange(0.5f, 3.0f)]
        [SerializeField] private float crosshairSpreadScale = 1.0f;
        
        [TabGroup("TPS Settings", "Colors")]
        [LabelText("Cover Available Color")]
        [SerializeField] private Color coverAvailableColor = Color.green;
        
        [TabGroup("TPS Settings", "Colors")]
        [LabelText("Cover Active Color")]
        [SerializeField] private Color coverActiveColor = Color.blue;

        [TabGroup("TPS Settings", "Audio")]
        [LabelText("Weapon Equip Sound")]
        [SerializeField] private AudioClip _weaponEquipSound;

        [TabGroup("TPS Settings", "Audio")]
        [LabelText("Low Ammo Warning Sound")]
        [SerializeField] private AudioClip _lowAmmoWarningSound;

        [TabGroup("TPS Settings", "Audio")]
        [LabelText("Enter Cover Sound")]
        [SerializeField] private AudioClip _enterCoverSound;

        [TabGroup("TPS Settings", "Audio")]
        [LabelText("Exit Cover Sound")]
        [SerializeField] private AudioClip _exitCoverSound;

        [TabGroup("TPS Settings", "Audio")]
        [LabelText("Aim Start Sound")]
        [SerializeField] private AudioClip _aimStartSound;

        [TabGroup("TPS Settings", "Audio")]
        [LabelText("Footstep Run Sound")]
        [SerializeField] private AudioClip _footstepRunSound;

        [TabGroup("TPS Settings", "Audio")]
        [LabelText("Reload Start Sound")]
        [SerializeField] private AudioClip _reloadStartSound;

        // ServiceLocator managed dependencies
        private IAudioManager audioManager;
        private IInputManager inputManager;
        private IGameEventManager eventManager;

        // TPS-specific state tracking
        private TPSWeaponManager weaponManager;
        private TPSPlayerController playerController;
        private bool isInCover = false;
        private bool isAiming = false;
        private WeaponData currentWeapon;

        // UI state
        private Vector3 originalCrosshairScale;

        // IGameEventListener implementation
        public int Priority => 0;

        private void Awake()
        {
            InitializeComponents();
            InitializeServiceLocator();
        }

        private void Start()
        {
            InitializeTPS();
        }

        private void OnEnable()
        {
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        /// <summary>
        /// Initialize core components and validate setup
        /// </summary>
        private void InitializeComponents()
        {
            // Auto-find core HUD manager if not assigned
            if (coreHUDManager == null)
            {
                coreHUDManager = FindFirstObjectByType<HUDManager>();
                if (coreHUDManager == null)
                {
                    Debug.LogWarning("[TPSHUDController] Core HUDManager not found in scene!");
                }
            }

            // Store original crosshair scale for dynamic scaling
            if (dynamicCrosshair != null)
            {
                originalCrosshairScale = dynamicCrosshair.localScale;
            }

            // Initialize UI elements to default states
            SetCoverIndicatorVisible(false);
            SetReloadIndicatorVisible(false);
            SetInteractionPromptVisible(false);
        }

        /// <summary>
        /// Initialize ServiceLocator dependencies
        /// </summary>
        private void InitializeServiceLocator()
        {
            try
            {
                audioManager = ServiceLocator.GetService<IAudioManager>();
                inputManager = ServiceLocator.GetService<IInputManager>();
                eventManager = ServiceLocator.GetService<IGameEventManager>();
                
                Debug.Log("[TPSHUDController] ServiceLocator dependencies initialized successfully");
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[TPSHUDController] Failed to initialize some ServiceLocator dependencies: {ex.Message}");
            }
        }

        /// <summary>
        /// Initialize TPS-specific components
        /// </summary>
        private void InitializeTPS()
        {
            // Find TPS components in scene
            weaponManager = FindFirstObjectByType<TPSWeaponManager>();
            playerController = FindFirstObjectByType<TPSPlayerController>();

            if (weaponManager == null)
            {
                Debug.LogWarning("[TPSHUDController] TPSWeaponManager not found in scene!");
            }

            if (playerController == null)
            {
                Debug.LogWarning("[TPSHUDController] TPSPlayerController not found in scene!");
            }

            // Set initial UI state
            UpdateCrosshairForState(PlayerState.Idle);
        }

        /// <summary>
        /// Subscribe to TPS-specific events
        /// </summary>
        private void SubscribeToEvents()
        {
            // Player state events
            if (onPlayerStateChanged != null)
            {
                onPlayerStateChanged.RegisterListener(this);
            }

            // Weapon events
            if (onWeaponEquipped != null)
            {
                onWeaponEquipped.RegisterListener(this);
            }

            if (onAmmoChanged != null)
            {
                onAmmoChanged.RegisterListener(this);
            }

            // Cover events
            if (onCoverStateChanged != null)
            {
                onCoverStateChanged.RegisterListener(this);
            }
        }

        /// <summary>
        /// Unsubscribe from TPS-specific events
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            // Player state events
            if (onPlayerStateChanged != null)
            {
                onPlayerStateChanged.UnregisterListener(this);
            }

            // Weapon events
            if (onWeaponEquipped != null)
            {
                onWeaponEquipped.UnregisterListener(this);
            }

            if (onAmmoChanged != null)
            {
                onAmmoChanged.UnregisterListener(this);
            }

            // Cover events
            if (onCoverStateChanged != null)
            {
                onCoverStateChanged.UnregisterListener(this);
            }
        }

        #region Event Handlers

        /// <summary>
        /// Handle player state changes for UI updates
        /// </summary>
        private void OnPlayerStateChanged(PlayerState newState)
        {
            UpdateCrosshairForState(newState);
            UpdateUIForPlayerState(newState);

            // Audio feedback through ServiceLocator
            if (audioManager != null)
            {
                PlayStateTransitionAudio(newState);
            }
        }

        /// <summary>
        /// Handle weapon equip events
        /// </summary>
        private void OnWeaponEquipped(WeaponData weaponData)
        {
            currentWeapon = weaponData;
            UpdateWeaponDisplay(weaponData);

            // Audio feedback
            if (audioManager != null && _weaponEquipSound != null)
            {
                audioManager.PlaySFX(_weaponEquipSound);
            }
        }

        /// <summary>
        /// Handle ammo change events
        /// </summary>
        private void OnAmmoChanged(int currentAmmo, int totalAmmo)
        {
            UpdateAmmoDisplay(currentAmmo, totalAmmo);

            // Low ammo warning
            if (currentAmmo <= 5 && audioManager != null && _lowAmmoWarningSound != null)
            {
                audioManager.PlaySFX(_lowAmmoWarningSound);
            }
        }

        /// <summary>
        /// Handle cover state changes
        /// </summary>
        private void OnCoverStateChanged(bool inCover)
        {
            isInCover = inCover;
            UpdateCoverIndicator(inCover);
            
            if (audioManager != null)
            {
                AudioClip coverSound = inCover ? _enterCoverSound : _exitCoverSound;
                if (coverSound != null)
                {
                    audioManager.PlaySFX(coverSound);
                }
            }
        }

        /// <summary>
        /// Explicit interface implementations for event listeners
        /// </summary>
        void IGameEventListener<PlayerState>.OnEventRaised(PlayerState value) => OnPlayerStateChanged(value);
        void IGameEventListener<WeaponData>.OnEventRaised(WeaponData value) => OnWeaponEquipped(value);
        void IGameEventTwoArgsListener<int, int>.OnEventRaised(int arg1, int arg2) => OnAmmoChanged(arg1, arg2);
        void IGameEventListener<bool>.OnEventRaised(bool value) => OnCoverStateChanged(value);

        #endregion

        #region UI Update Methods

        /// <summary>
        /// Update crosshair appearance based on player state
        /// </summary>
        private void UpdateCrosshairForState(PlayerState state)
        {
            if (dynamicCrosshair == null) return;

            switch (state)
            {
                case PlayerState.Aiming:
                    isAiming = true;
                    SetCrosshairScale(0.5f); // Smaller when aiming
                    SetAimCrosshairVisible(true);
                    break;

                case PlayerState.Running:
                    isAiming = false;
                    SetCrosshairScale(1.5f * crosshairSpreadScale); // Larger when moving
                    SetAimCrosshairVisible(false);
                    break;

                case PlayerState.InCover:
                    isAiming = false;
                    SetCrosshairScale(0.8f);
                    SetAimCrosshairVisible(false);
                    break;

                default:
                    isAiming = false;
                    SetCrosshairScale(1.0f);
                    SetAimCrosshairVisible(false);
                    break;
            }
        }

        /// <summary>
        /// Update UI elements based on player state
        /// </summary>
        private void UpdateUIForPlayerState(PlayerState state)
        {
            switch (state)
            {
                case PlayerState.InCover:
                    SetCoverIndicatorVisible(true);
                    break;
                
                case PlayerState.Reloading:
                    SetReloadIndicatorVisible(true);
                    break;
                
                default:
                    if (state != PlayerState.InCover)
                        SetCoverIndicatorVisible(false);
                    if (state != PlayerState.Reloading)
                        SetReloadIndicatorVisible(false);
                    break;
            }
        }

        /// <summary>
        /// Update weapon display information
        /// </summary>
        private void UpdateWeaponDisplay(WeaponData weaponData)
        {
            if (weaponNameText != null)
            {
                weaponNameText.text = weaponData.WeaponName;
            }

            // Delegate ammo display to core HUD manager
            if (coreHUDManager != null && weaponManager != null)
            {
                coreHUDManager.UpdateAmmo(weaponManager.CurrentAmmoInMagazine, 
                                        weaponManager.GetTotalAmmo(weaponData.AmmoType));
            }
        }

        /// <summary>
        /// Update ammo display
        /// </summary>
        private void UpdateAmmoDisplay(int currentAmmo, int totalAmmo)
        {
            if (ammoDisplayText != null)
            {
                ammoDisplayText.text = $"{currentAmmo}/{totalAmmo}";
                
                // Warning color for low ammo
                ammoDisplayText.color = currentAmmo <= 10 ? Color.red : Color.white;
            }

            // Also update core HUD manager
            if (coreHUDManager != null)
            {
                coreHUDManager.UpdateAmmo(currentAmmo, totalAmmo);
            }
        }

        /// <summary>
        /// Update cover indicator
        /// </summary>
        private void UpdateCoverIndicator(bool inCover)
        {
            if (coverIndicator != null)
            {
                coverIndicator.color = inCover ? coverActiveColor : coverAvailableColor;
                SetCoverIndicatorVisible(true);
            }
        }

        #endregion

        #region UI Control Methods

        /// <summary>
        /// Set crosshair scale with smooth animation
        /// </summary>
        private void SetCrosshairScale(float scale)
        {
            if (dynamicCrosshair != null)
            {
                Vector3 targetScale = originalCrosshairScale * scale;
                // Use DOTween for smooth scaling if available, otherwise set directly
                dynamicCrosshair.localScale = targetScale;
            }
        }

        /// <summary>
        /// Set aim crosshair visibility
        /// </summary>
        private void SetAimCrosshairVisible(bool visible)
        {
            if (aimCrosshair != null)
            {
                aimCrosshair.gameObject.SetActive(visible);
            }
        }

        /// <summary>
        /// Set cover indicator visibility
        /// </summary>
        private void SetCoverIndicatorVisible(bool visible)
        {
            if (coverIndicator != null)
            {
                coverIndicator.gameObject.SetActive(visible);
            }
        }

        /// <summary>
        /// Set reload indicator visibility
        /// </summary>
        private void SetReloadIndicatorVisible(bool visible)
        {
            if (reloadIndicator != null)
            {
                reloadIndicator.SetActive(visible);
            }
        }

        /// <summary>
        /// Set interaction prompt visibility
        /// </summary>
        private void SetInteractionPromptVisible(bool visible)
        {
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(visible);
            }
        }

        /// <summary>
        /// Show interaction prompt with custom text
        /// </summary>
        public void ShowInteractionPrompt(string promptText)
        {
            if (interactionText != null)
            {
                interactionText.text = promptText;
            }
            SetInteractionPromptVisible(true);
        }

        /// <summary>
        /// Hide interaction prompt
        /// </summary>
        public void HideInteractionPrompt()
        {
            SetInteractionPromptVisible(false);
        }

        #endregion

        #region Audio Integration

        /// <summary>
        /// Play audio feedback for state transitions
        /// </summary>
        private void PlayStateTransitionAudio(PlayerState newState)
        {
            AudioClip audioClip = newState switch
            {
                PlayerState.Aiming => _aimStartSound,
                PlayerState.Running => _footstepRunSound,
                PlayerState.InCover => _enterCoverSound,
                PlayerState.Reloading => _reloadStartSound,
                _ => null
            };

            if (audioClip != null && audioManager != null)
            {
                audioManager.PlaySFX(audioClip);
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Update health display (delegates to core HUD)
        /// </summary>
        public void UpdateHealth(float health, float maxHealth)
        {
            if (coreHUDManager != null)
            {
                coreHUDManager.UpdateHealthDisplay(health, maxHealth);
            }
        }

        /// <summary>
        /// Show notification (delegates to core HUD)
        /// </summary>
        public void ShowNotification(string message)
        {
            if (coreHUDManager != null)
            {
                coreHUDManager.ShowNotification(message);
            }
        }

        /// <summary>
        /// Update crosshair spread dynamically (for weapon accuracy)
        /// </summary>
        public void SetCrosshairSpread(float spreadMultiplier)
        {
            float totalSpread = spreadMultiplier * crosshairSpreadScale;
            SetCrosshairScale(isAiming ? 0.5f * totalSpread : totalSpread);
        }

        #endregion

        #region Editor Debug Functions

        #if UNITY_EDITOR
        [TabGroup("TPS Debug", "Test Functions")]
        [Button("Test Cover Enter")]
        private void TestCoverEnter()
        {
            if (Application.isPlaying)
            {
                OnCoverStateChanged(true);
            }
        }

        [TabGroup("TPS Debug", "Test Functions")]
        [Button("Test Cover Exit")]
        private void TestCoverExit()
        {
            if (Application.isPlaying)
            {
                OnCoverStateChanged(false);
            }
        }

        [TabGroup("TPS Debug", "Test Functions")]
        [Button("Test Weapon Switch")]
        private void TestWeaponSwitch()
        {
            if (Application.isPlaying && currentWeapon != null)
            {
                OnWeaponEquipped(currentWeapon);
            }
        }

        [TabGroup("TPS Debug", "Test Functions")]
        [Button("Test Low Ammo")]
        private void TestLowAmmo()
        {
            if (Application.isPlaying)
            {
                OnAmmoChanged(3, 30);
            }
        }

        [TabGroup("TPS Debug", "Test Functions")]
        [Button("Test Interaction Prompt")]
        private void TestInteractionPrompt()
        {
            if (Application.isPlaying)
            {
                ShowInteractionPrompt("Press E to interact");
            }
        }
        #endif

        #endregion
    }
}