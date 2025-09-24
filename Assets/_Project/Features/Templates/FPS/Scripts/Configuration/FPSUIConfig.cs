using UnityEngine;

namespace asterivo.Unity60.Features.Templates.FPS.Configuration
{
    /// <summary>
    /// FPS Template用UI設定
    /// クロスヘア、HUD、メニュー、インタラクション表示等の設定を管理
    /// </summary>
    [CreateAssetMenu(menuName = "FPS/Configuration/UI Config")]
    public class FPSUIConfig : ScriptableObject
    {
        [Header("Crosshair Settings")]
        [SerializeField] private bool _enableCrosshair = true;
        [SerializeField] private Color _crosshairColor = Color.white;
        [SerializeField] private float _crosshairSize = 20f;
        [SerializeField] private bool _dynamicCrosshair = true;
        [SerializeField] private float _crosshairSpreadMultiplier = 1.5f;

        [Header("HUD Settings")]
        [SerializeField] private bool _showHealthBar = true;
        [SerializeField] private bool _showAmmoCounter = true;
        [SerializeField] private bool _showMinimap = true;
        [SerializeField] private bool _showKillFeed = true;
        [SerializeField] private float _hudOpacity = 1.0f;

        [Header("Interaction UI")]
        [SerializeField] private bool _enableInteractionPrompts = true;
        [SerializeField] private float _interactionRange = 3f;
        [SerializeField] private Color _interactionPromptColor = Color.yellow;
        [SerializeField] private string _interactionKey = "E";

        [Header("Damage Indicators")]
        [SerializeField] private bool _enableDamageNumbers = true;
        [SerializeField] private bool _enableHitMarkers = true;
        [SerializeField] private Color _damageNumberColor = Color.red;
        [SerializeField] private Color _hitMarkerColor = Color.white;
        [SerializeField] private float _damageIndicatorDuration = 2f;

        [Header("Menu Settings")]
        [SerializeField] private bool _pauseGameOnMenu = true;
        [SerializeField] private float _menuTransitionTime = 0.3f;
        [SerializeField] private bool _enableMenuBlur = true;

        [Header("Accessibility")]
        [SerializeField] private bool _enableColorBlindSupport = false;
        [SerializeField] private float _textScaling = 1.0f;
        [SerializeField] private bool _enableSubtitles = false;

        // Properties
        public bool EnableCrosshair => _enableCrosshair;
        public Color CrosshairColor => _crosshairColor;
        public float CrosshairSize => _crosshairSize;
        public bool DynamicCrosshair => _dynamicCrosshair;
        public float CrosshairSpreadMultiplier => _crosshairSpreadMultiplier;
        public bool ShowHealthBar => _showHealthBar;
        public bool ShowAmmoCounter => _showAmmoCounter;
        public bool ShowMinimap => _showMinimap;
        public bool ShowKillFeed => _showKillFeed;
        public float HudOpacity => _hudOpacity;
        public bool EnableInteractionPrompts => _enableInteractionPrompts;
        public float InteractionRange => _interactionRange;
        public Color InteractionPromptColor => _interactionPromptColor;
        public string InteractionKey => _interactionKey;
        public bool EnableDamageNumbers => _enableDamageNumbers;
        public bool EnableHitMarkers => _enableHitMarkers;
        public Color DamageNumberColor => _damageNumberColor;
        public Color HitMarkerColor => _hitMarkerColor;
        public float DamageIndicatorDuration => _damageIndicatorDuration;
        public bool PauseGameOnMenu => _pauseGameOnMenu;
        public float MenuTransitionTime => _menuTransitionTime;
        public bool EnableMenuBlur => _enableMenuBlur;
        public bool EnableColorBlindSupport => _enableColorBlindSupport;
        public float TextScaling => _textScaling;
        public bool EnableSubtitles => _enableSubtitles;
    }
}
