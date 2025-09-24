using UnityEngine;

namespace asterivo.Unity60.Core.Services
{
    /// <summary>
    /// Interface for HUD management service
    /// Provides centralized UI/HUD control for the application
    /// </summary>
    public interface IHUDManager : IService
    {
        /// <summary>
        /// Update player health display
        /// </summary>
        /// <param name="currentHealth">Current health value</param>
        /// <param name="maxHealth">Maximum health value</param>
        void UpdateHealthDisplay(float currentHealth, float maxHealth);

        /// <summary>
        /// Update ammo display
        /// </summary>
        /// <param name="currentAmmo">Current ammo in magazine</param>
        /// <param name="totalAmmo">Total ammo available</param>
        void UpdateAmmoDisplay(int currentAmmo, int totalAmmo);

        /// <summary>
        /// Update weapon display with generic weapon information
        /// </summary>
        /// <param name="weaponName">Name of the current weapon</param>
        /// <param name="weaponIconId">Icon identifier for the weapon</param>
        void UpdateWeaponDisplay(string weaponName, string weaponIconId = "");

        /// <summary>
        /// Update player state display
        /// </summary>
        /// <param name="stateName">Name of the current player state</param>
        void UpdatePlayerStateDisplay(string stateName);

        /// <summary>
        /// Show or hide crosshair
        /// </summary>
        /// <param name="show">Whether to show crosshair</param>
        void ShowCrosshair(bool show);

        /// <summary>
        /// Update crosshair style based on weapon type
        /// </summary>
        /// <param name="weaponTypeId">Identifier for weapon type (e.g., "rifle", "pistol", "shotgun")</param>
        void UpdateCrosshairStyle(string weaponTypeId);

        /// <summary>
        /// Show damage indicator
        /// </summary>
        /// <param name="damage">Damage amount</param>
        /// <param name="position">World position where damage occurred</param>
        void ShowDamageIndicator(float damage, Vector3 position);

        /// <summary>
        /// Show hit marker
        /// </summary>
        /// <param name="isHeadshot">Whether it was a headshot</param>
        void ShowHitMarker(bool isHeadshot = false);

        /// <summary>
        /// Update reticle spread based on accuracy
        /// </summary>
        /// <param name="spread">Spread value (0-1)</param>
        void UpdateReticleSpread(float spread);

        /// <summary>
        /// Show reload progress
        /// </summary>
        /// <param name="progress">Reload progress (0-1)</param>
        void ShowReloadProgress(float progress);

        /// <summary>
        /// Hide reload progress
        /// </summary>
        void HideReloadProgress();

        /// <summary>
        /// Show low health warning
        /// </summary>
        /// <param name="intensity">Warning intensity (0-1)</param>
        void ShowLowHealthWarning(float intensity);

        /// <summary>
        /// Hide low health warning
        /// </summary>
        void HideLowHealthWarning();

        /// <summary>
        /// Show interaction prompt
        /// </summary>
        /// <param name="text">Prompt text</param>
        void ShowInteractionPrompt(string text);

        /// <summary>
        /// Hide interaction prompt
        /// </summary>
        void HideInteractionPrompt();

        /// <summary>
        /// Show cover indicator
        /// </summary>
        /// <param name="show">Whether to show cover indicator</param>
        void ShowCoverIndicator(bool show);

        /// <summary>
        /// Update minimap player position
        /// </summary>
        /// <param name="position">Player world position</param>
        /// <param name="rotation">Player rotation</param>
        void UpdateMinimapPlayer(Vector3 position, float rotation);

        /// <summary>
        /// Show or hide HUD
        /// </summary>
        /// <param name="show">Whether to show HUD</param>
        void ShowHUD(bool show);

        /// <summary>
        /// Set HUD visibility for specific panels
        /// </summary>
        /// <param name="panelName">Name of the panel</param>
        /// <param name="visible">Whether panel should be visible</param>
        void SetPanelVisibility(string panelName, bool visible);

        /// <summary>
        /// Show game over screen
        /// </summary>
        void ShowGameOverScreen();

        /// <summary>
        /// Hide game over screen
        /// </summary>
        void HideGameOverScreen();

        /// <summary>
        /// Show pause menu
        /// </summary>
        void ShowPauseMenu();

        /// <summary>
        /// Hide pause menu
        /// </summary>
        void HidePauseMenu();

        /// <summary>
        /// Update score display
        /// </summary>
        /// <param name="score">Current score</param>
        void UpdateScoreDisplay(int score);

        /// <summary>
        /// Show message to player
        /// </summary>
        /// <param name="message">Message text</param>
        /// <param name="duration">Display duration in seconds</param>
        void ShowMessage(string message, float duration = 3.0f);
    }
}
