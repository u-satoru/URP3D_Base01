using UnityEngine;
using System.Collections.Generic;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core;
using asterivo.Unity60.Features.Templates.Stealth.Services;
using asterivo.Unity60.Features.Templates.Stealth.Player.States;
using asterivo.Unity60.Features.Player.States;

namespace asterivo.Unity60.Features.Templates.Stealth.Player
{
    /// <summary>
    /// 繧ｹ繝・Ν繧ｹ繝・Φ繝励Ξ繝ｼ繝育畑迥ｶ諷狗ｮ｡逅・す繧ｹ繝・Β
    /// 騾壼ｸｸ縺ｮ迥ｶ諷九→繧ｹ繝・Ν繧ｹ蠑ｷ蛹也憾諷九ｒ蜍慕噪縺ｫ蛻・ｊ譖ｿ縺・
    /// ServiceLocator邨ｱ蜷医↓繧医ｋ荳ｭ螟ｮ蛻ｶ蠕｡縺ｨ繧､繝吶Φ繝磯ｧ・虚繧｢繝ｼ繧ｭ繝・け繝√Ε
    /// </summary>
    public class StealthStateManager : MonoBehaviour
    {
        [Header("Stealth State Configuration")]
        [SerializeField] private bool _useStealthEnhancedStates = true;
        [SerializeField] private bool _autoEnableStealthMode = true;
        [SerializeField] private float _stealthModeToggleCooldown = 1.0f;

        [Header("Debug")]
        [SerializeField] private bool _enableDebugLogs = true;

        // 繧ｹ繝・Ν繧ｹ蠑ｷ蛹也憾諷九・邂｡逅・
        private Dictionary<PlayerStateType, IPlayerState> _stealthStates;
        private Dictionary<PlayerStateType, IPlayerState> _normalStates;

        // 繧ｵ繝ｼ繝薙せ蜿ら・
        private IStealthService _stealthService;
        private DetailedPlayerStateMachine _playerStateMachine;

        // 迥ｶ諷狗ｮ｡逅・
        private bool _isStealthModeActive = false;
        private float _lastToggleTime = 0f;

        /// <summary>
        /// 繧ｹ繝・Ν繧ｹ蠑ｷ蛹也憾諷九′譛牙柑縺九←縺・°
        /// </summary>
        public bool UseStealthEnhancedStates
        {
            get => _useStealthEnhancedStates;
            set
            {
                if (_useStealthEnhancedStates != value)
                {
                    _useStealthEnhancedStates = value;
                    RefreshStateMapping();

                    if (_enableDebugLogs)
                        Debug.Log($"[StealthStateManager] Stealth enhanced states: {(_useStealthEnhancedStates ? "Enabled" : "Disabled")}");
                }
            }
        }

        /// <summary>
        /// 迴ｾ蝨ｨ繧ｹ繝・Ν繧ｹ繝｢繝ｼ繝峨′譛牙柑縺九←縺・°
        /// </summary>
        public bool IsStealthModeActive => _isStealthModeActive;

        #region Unity Lifecycle
        private void Awake()
        {
            InitializeStates();
        }

        private void Start()
        {
            InitializeServices();

            if (_autoEnableStealthMode)
            {
                EnableStealthMode();
            }
        }

        private void Update()
        {
            HandleStealthModeToggle();
            UpdateStealthState();
        }
        #endregion

        #region Initialization
        /// <summary>
        /// 繧ｹ繝・Ν繧ｹ迥ｶ諷九→繝弱・繝槭Ν迥ｶ諷九・蛻晄悄蛹・
        /// </summary>
        private void InitializeStates()
        {
            // 繧ｹ繝・Ν繧ｹ蠑ｷ蛹也憾諷九・逋ｻ骭ｲ
            _stealthStates = new Dictionary<PlayerStateType, IPlayerState>
            {
                { PlayerStateType.Crouching, new StealthCrouchingState() },
                { PlayerStateType.Prone, new StealthProneState() },
                { PlayerStateType.InCover, new StealthInCoverState() }
            };

            // 騾壼ｸｸ迥ｶ諷九・逋ｻ骭ｲ・域里蟄伜ｮ溯｣・∈縺ｮ蜿ら・・・
            _normalStates = new Dictionary<PlayerStateType, IPlayerState>();

            if (_enableDebugLogs)
                Debug.Log($"[StealthStateManager] Initialized {_stealthStates.Count} stealth enhanced states");
        }

        /// <summary>
        /// 繧ｵ繝ｼ繝薙せ縺ｨ繧ｳ繝ｳ繝昴・繝阪Φ繝医・蛻晄悄蛹・
        /// </summary>
        private void InitializeServices()
        {
            // ServiceLocator邨檎罰縺ｧStealthService繧貞叙蠕・
            _stealthService = ServiceLocator.GetService<IStealthService>();
            if (_stealthService == null)
            {
                Debug.LogWarning("[StealthStateManager] StealthService not found in ServiceLocator");
            }

            // 繝励Ξ繧､繝､繝ｼ繧ｹ繝・・繝医・繧ｷ繝ｳ縺ｮ蜿ら・繧貞叙蠕・
            _playerStateMachine = GetComponent<DetailedPlayerStateMachine>();
            if (_playerStateMachine == null)
            {
                Debug.LogError("[StealthStateManager] DetailedPlayerStateMachine not found on this GameObject");
            }
        }
        #endregion

        #region State Management
        /// <summary>
        /// 迥ｶ諷九・繝・ヴ繝ｳ繧ｰ縺ｮ繝ｪ繝輔Ξ繝・す繝･
        /// 繧ｹ繝・Ν繧ｹ蠑ｷ蛹也憾諷九→騾壼ｸｸ迥ｶ諷九・蛻・ｊ譖ｿ縺・
        /// </summary>
        private void RefreshStateMapping()
        {
            if (_playerStateMachine == null) return;

            if (_useStealthEnhancedStates && _isStealthModeActive)
            {
                // 繧ｹ繝・Ν繧ｹ蠑ｷ蛹也憾諷九ｒ驕ｩ逕ｨ
                foreach (var kvp in _stealthStates)
                {
                    // 螳滄圀縺ｮ迥ｶ諷狗ｽｮ縺肴鋤縺医・縲√・繝ｬ繧､繝､繝ｼ繧ｹ繝・・繝医・繧ｷ繝ｳ縺ｮ螳溯｣・↓萓晏ｭ・
                    // 縺薙％縺ｧ縺ｯ讎ょｿｵ逧・↑螳溯｣・ｒ遉ｺ縺・
                    RegisterStealthState(kvp.Key, kvp.Value);
                }
            }
            else
            {
                // 騾壼ｸｸ迥ｶ諷九↓謌ｻ縺・
                RestoreNormalStates();
            }
        }

        /// <summary>
        /// 繧ｹ繝・Ν繧ｹ迥ｶ諷九・逋ｻ骭ｲ
        /// </summary>
        private void RegisterStealthState(PlayerStateType stateType, IPlayerState stealthState)
        {
            // 繝励Ξ繧､繝､繝ｼ繧ｹ繝・・繝医・繧ｷ繝ｳ縺ｫ繧ｹ繝・Ν繧ｹ蠑ｷ蛹也憾諷九ｒ逋ｻ骭ｲ
            // 螳溯｣・・譌｢蟄倥・DetailedPlayerStateMachine.cs縺ｮ讒矩縺ｫ萓晏ｭ・

            if (_enableDebugLogs)
                Debug.Log($"[StealthStateManager] Registered stealth enhanced state: {stateType}");
        }

        /// <summary>
        /// 騾壼ｸｸ迥ｶ諷九・蠕ｩ蜈・
        /// </summary>
        private void RestoreNormalStates()
        {
            // 騾壼ｸｸ迥ｶ諷九↓謌ｻ縺吝・逅・
            // 螳溯｣・・譌｢蟄倥・DetailedPlayerStateMachine.cs縺ｮ讒矩縺ｫ萓晏ｭ・

            if (_enableDebugLogs)
                Debug.Log("[StealthStateManager] Restored normal states");
        }

        /// <summary>
        /// 迴ｾ蝨ｨ縺ｮ迥ｶ諷九′繧ｹ繝・Ν繧ｹ髢｢騾｣縺九←縺・°繧貞愛螳・
        /// </summary>
        public bool IsCurrentStateStealthRelated()
        {
            if (_playerStateMachine == null) return false;

            PlayerStateType currentState = _playerStateMachine.GetCurrentStateType();
            return _stealthStates.ContainsKey(currentState);
        }

        /// <summary>
        /// 謖・ｮ壹＆繧後◆迥ｶ諷九↓繧ｹ繝・Ν繧ｹ蠑ｷ蛹也沿縺悟ｭ伜惠縺吶ｋ縺九メ繧ｧ繝・け
        /// </summary>
        public bool HasStealthEnhancedVersion(PlayerStateType stateType)
        {
            return _stealthStates.ContainsKey(stateType);
        }
        #endregion

        #region Stealth Mode Control
        /// <summary>
        /// 繧ｹ繝・Ν繧ｹ繝｢繝ｼ繝峨・譛牙柑蛹・
        /// </summary>
        public void EnableStealthMode()
        {
            if (_isStealthModeActive) return;

            _isStealthModeActive = true;

            // StealthService縺ｧ繧ｹ繝・Ν繧ｹ繝｢繝ｼ繝峨ｒ譛牙柑蛹・
            _stealthService?.SetStealthMode(true);

            // 迥ｶ諷九・繝・ヴ繝ｳ繧ｰ繧偵Μ繝輔Ξ繝・す繝･
            RefreshStateMapping();

            // 繧ｹ繝・Ν繧ｹ髢｢騾｣UI縺ｮ陦ｨ遉ｺ・亥ｰ・擂螳溯｣・ｼ・
            ShowStealthUI();

            if (_enableDebugLogs)
                Debug.Log("[StealthStateManager] Stealth mode enabled");
        }

        /// <summary>
        /// 繧ｹ繝・Ν繧ｹ繝｢繝ｼ繝峨・辟｡蜉ｹ蛹・
        /// </summary>
        public void DisableStealthMode()
        {
            if (!_isStealthModeActive) return;

            _isStealthModeActive = false;

            // StealthService縺ｧ繧ｹ繝・Ν繧ｹ繝｢繝ｼ繝峨ｒ辟｡蜉ｹ蛹・
            _stealthService?.SetStealthMode(false);

            // 騾壼ｸｸ迥ｶ諷九↓謌ｻ縺・
            RefreshStateMapping();

            // 繧ｹ繝・Ν繧ｹ髢｢騾｣UI縺ｮ髱櫁｡ｨ遉ｺ
            HideStealthUI();

            if (_enableDebugLogs)
                Debug.Log("[StealthStateManager] Stealth mode disabled");
        }

        /// <summary>
        /// 繧ｹ繝・Ν繧ｹ繝｢繝ｼ繝峨・蛻・ｊ譖ｿ縺・
        /// </summary>
        public void ToggleStealthMode()
        {
            if (Time.time - _lastToggleTime < _stealthModeToggleCooldown) return;

            if (_isStealthModeActive)
            {
                DisableStealthMode();
            }
            else
            {
                EnableStealthMode();
            }

            _lastToggleTime = Time.time;
        }
        #endregion

        #region Input Handling
        /// <summary>
        /// 繧ｹ繝・Ν繧ｹ繝｢繝ｼ繝牙・繧頑崛縺医・蜈･蜉帛・逅・
        /// </summary>
        private void HandleStealthModeToggle()
        {
            // 繝・ヵ繧ｩ繝ｫ繝医〒Tab 繧ｭ繝ｼ縺ｧ繧ｹ繝・Ν繧ｹ繝｢繝ｼ繝牙・繧頑崛縺・
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                ToggleStealthMode();
            }

            // 邱頑･繧ｹ繝・Ν繧ｹ繝｢繝ｼ繝会ｼ・hift+Tab 縺ｧ蠑ｷ蛻ｶ辟｡蜉ｹ蛹厄ｼ・
            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Tab))
            {
                DisableStealthMode();
            }
        }
        #endregion

        #region State Updates
        /// <summary>
        /// 繧ｹ繝・Ν繧ｹ迥ｶ諷九・譖ｴ譁ｰ
        /// </summary>
        private void UpdateStealthState()
        {
            if (!_isStealthModeActive || _stealthService == null) return;

            // 迴ｾ蝨ｨ縺ｮ迥ｶ諷九′繧ｹ繝・Ν繧ｹ髢｢騾｣縺ｮ蝣ｴ蜷医∬ｿｽ蜉縺ｮ蜃ｦ逅・
            if (IsCurrentStateStealthRelated())
            {
                // 繧ｹ繝・Ν繧ｹ迥ｶ諷狗音譛峨・譖ｴ譁ｰ蜃ｦ逅・
                UpdateStealthMetrics();
                CheckStealthConditions();
            }
        }

        /// <summary>
        /// 繧ｹ繝・Ν繧ｹ謖・ｨ吶・譖ｴ譁ｰ
        /// </summary>
        private void UpdateStealthMetrics()
        {
            // 繧ｹ繝・Ν繧ｹ邨ｱ險医・蜿門ｾ励→譖ｴ譁ｰ
            var stats = _stealthService.GetStealthStatistics();

            // UI譖ｴ譁ｰ縺ｪ縺ｩ縺ｮ蜃ｦ逅・ｼ亥ｰ・擂螳溯｣・ｼ・
            // UpdateStealthUI(stats);
        }

        /// <summary>
        /// 繧ｹ繝・Ν繧ｹ譚｡莉ｶ縺ｮ繝√ぉ繝・け
        /// </summary>
        private void CheckStealthConditions()
        {
            // 逋ｺ隕九Μ繧ｹ繧ｯ縺ｮ隧穂ｾ｡
            float visibilityFactor = _stealthService.PlayerVisibilityFactor;
            float noiseLevel = _stealthService.PlayerNoiseLevel;

            // 鬮倥Μ繧ｹ繧ｯ迥ｶ諷九・隴ｦ蜻・
            if (visibilityFactor > 0.8f || noiseLevel > 0.7f)
            {
                if (_enableDebugLogs)
                    Debug.Log($"[StealthStateManager] High detection risk - Visibility: {visibilityFactor:F2}, Noise: {noiseLevel:F2}");
            }
        }
        #endregion

        #region UI Integration (Future Implementation)
        /// <summary>
        /// 繧ｹ繝・Ν繧ｹUI縺ｮ陦ｨ遉ｺ
        /// </summary>
        private void ShowStealthUI()
        {
            // 繧ｹ繝・Ν繧ｹ髢｢騾｣UI縺ｮ陦ｨ遉ｺ蜃ｦ逅・
            // 蟆・擂逧・↓螳溯｣・ｺ亥ｮ・
        }

        /// <summary>
        /// 繧ｹ繝・Ν繧ｹUI縺ｮ髱櫁｡ｨ遉ｺ
        /// </summary>
        private void HideStealthUI()
        {
            // 繧ｹ繝・Ν繧ｹ髢｢騾｣UI縺ｮ髱櫁｡ｨ遉ｺ蜃ｦ逅・
            // 蟆・擂逧・↓螳溯｣・ｺ亥ｮ・
        }
        #endregion

        #region Public Interface
        /// <summary>
        /// 蠑ｷ蛻ｶ逧・↓繧ｹ繝・Ν繧ｹ蠑ｷ蛹也憾諷九∈縺ｮ驕ｷ遘ｻ
        /// </summary>
        public void ForceStealthState(PlayerStateType stateType)
        {
            if (!_stealthStates.ContainsKey(stateType))
            {
                Debug.LogWarning($"[StealthStateManager] No stealth enhanced version for state: {stateType}");
                return;
            }

            EnableStealthMode();
            _playerStateMachine?.TransitionToState(stateType);
        }

        /// <summary>
        /// 繧ｹ繝・Ν繧ｹ迥ｶ諷区ュ蝣ｱ縺ｮ蜿門ｾ・
        /// </summary>
        public StealthStateInfo GetCurrentStealthInfo()
        {
            return new StealthStateInfo
            {
                IsStealthModeActive = _isStealthModeActive,
                UseStealthEnhancedStates = _useStealthEnhancedStates,
                CurrentStateIsStealthRelated = IsCurrentStateStealthRelated(),
                VisibilityFactor = _stealthService?.PlayerVisibilityFactor ?? 1.0f,
                NoiseLevel = _stealthService?.PlayerNoiseLevel ?? 0.0f,
                IsConcealed = _stealthService?.IsPlayerConcealed ?? false
            };
        }
        #endregion

        #region Debug
        private void OnGUI()
        {
            if (!_enableDebugLogs || !Application.isPlaying) return;

            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label("=== Stealth State Manager Debug ===");
            GUILayout.Label($"Stealth Mode: {_isStealthModeActive}");
            GUILayout.Label($"Enhanced States: {_useStealthEnhancedStates}");
            GUILayout.Label($"Current State Stealth Related: {IsCurrentStateStealthRelated()}");

            if (_stealthService != null)
            {
                GUILayout.Label($"Visibility: {_stealthService.PlayerVisibilityFactor:F2}");
                GUILayout.Label($"Noise Level: {_stealthService.PlayerNoiseLevel:F2}");
                GUILayout.Label($"Concealed: {_stealthService.IsPlayerConcealed}");
            }

            GUILayout.EndArea();
        }
        #endregion
    }

    /// <summary>
    /// 繧ｹ繝・Ν繧ｹ迥ｶ諷区ュ蝣ｱ讒矩菴・
    /// </summary>
    [System.Serializable]
    public struct StealthStateInfo
    {
        public bool IsStealthModeActive;
        public bool UseStealthEnhancedStates;
        public bool CurrentStateIsStealthRelated;
        public float VisibilityFactor;
        public float NoiseLevel;
        public bool IsConcealed;
    }
}


