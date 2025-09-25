using UnityEngine;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Features.Templates.FPS.Configuration;
using asterivo.Unity60.Features.Templates.FPS.Data;

namespace asterivo.Unity60.Features.Templates.FPS
{
    /// <summary>
    /// FPSテンプレート全体の設定を統括するメインコンフィギュレーション
    /// ScriptableObjectベースのデータ駆動設計により、デザイナーがコードを触らずにゲームバランス調整可能
    /// SPEC.md v3.0対応: Learn & Grow価値実現のための統合設定管理
    /// </summary>
    [CreateAssetMenu(menuName = "Templates/FPS/FPS_Template_Config")]
    public class FPS_Template_Config : ScriptableObject
    {
        [Header("Template Identity")]
        [SerializeField] private string _templateName = "FPS Template";
        [SerializeField] private string _templateVersion = "1.0.0";
        [SerializeField] private FPSGameMode _gameMode = FPSGameMode.TeamDeathmatch;

        [Header("Learn & Grow Target Metrics")]
        [SerializeField] private float _targetGameplayDuration = 15.0f; // 15分ゲームプレイ目標
        [SerializeField] private float _estimatedLearningTime = 12.0f; // 12時間学習目標（70%削減）

        [Header("Player Default Stats")]
        [SerializeField] private PlayerStatsConfig _defaultPlayerStats;

        [Header("Weapon Loadout Configuration")]
        [SerializeField] private WeaponLoadoutConfig _weaponLoadout;

        [Header("Game Mode Settings")]
        [SerializeField] private GameModeConfig _gameModeSettings;

        [Header("TTK Balance Configuration")]
        [SerializeField] private TTKBalanceConfig _ttkBalance;

        [Header("Camera & Movement Settings")]
        [SerializeField] private FPSCameraConfig _cameraConfig;
        [SerializeField] private FPSMovementConfig _movementConfig;

        [Header("Audio Configuration")]
        [SerializeField] private FPSAudioConfig _audioConfig;

        [Header("UI & HUD Settings")]
        [SerializeField] private FPSUIConfig _uiConfig;

        [Header("Performance Optimization")]
        [SerializeField] private FPSPerformanceConfig _performanceConfig;

        // Event Channels（Event駆動アーキテクチャ準拠）
        [Header("Event Channels")]
        [SerializeField] private GameEvent _onWeaponFired;
        [SerializeField] private GameEvent _onPlayerDamaged;
        [SerializeField] private GameEvent _onPlayerKilled;
        [SerializeField] private GameEvent _onWeaponReloaded;
        [SerializeField] private GameEvent _onGameModeStateChanged;

        // Template Identity Properties
        public string TemplateName => _templateName;
        public string TemplateVersion => _templateVersion;
        public FPSGameMode GameMode => _gameMode;
        public float TargetGameplayDuration => _targetGameplayDuration;
        public float EstimatedLearningTime => _estimatedLearningTime;

        // Configuration Properties
        public PlayerStatsConfig DefaultPlayerStats => _defaultPlayerStats;
        public WeaponLoadoutConfig WeaponLoadout => _weaponLoadout;
        public GameModeConfig GameModeSettings => _gameModeSettings;
        public TTKBalanceConfig TTKBalance => _ttkBalance;
        public FPSCameraConfig CameraConfig => _cameraConfig;
        public FPSMovementConfig MovementConfig => _movementConfig;
        public FPSAudioConfig AudioConfig => _audioConfig;
        public FPSUIConfig UIConfig => _uiConfig;
        public FPSPerformanceConfig PerformanceConfig => _performanceConfig;

        // Event Properties
        public GameEvent OnWeaponFired => _onWeaponFired;
        public GameEvent OnPlayerDamaged => _onPlayerDamaged;
        public GameEvent OnPlayerKilled => _onPlayerKilled;
        public GameEvent OnWeaponReloaded => _onWeaponReloaded;
        public GameEvent OnGameModeStateChanged => _onGameModeStateChanged;

        /// <summary>
        /// デザイン原則：「タクティカルシューター」と「カジュアル（アーケード）シューター」のスペクトラムに対応
        /// TTK (Time-to-Kill) を中心としたコアメカニクスの調整可能な設計
        /// </summary>
        public void ApplyTTKBalance(TTKProfile profile)
        {
            if (_ttkBalance != null)
            {
                _ttkBalance.ApplyProfile(profile);
                _onGameModeStateChanged?.Raise();
            }
        }

        /// <summary>
        /// ゲームモードの動的切り替え
        /// チームデスマッチの目標キル数などの設定反映
        /// </summary>
        public void ChangeGameMode(FPSGameMode newMode)
        {
            _gameMode = newMode;
            if (_gameModeSettings != null)
            {
                _gameModeSettings.ApplyGameMode(newMode);
                _onGameModeStateChanged?.Raise();
            }
        }

        /// <summary>
        /// 武器ロードアウトの動的変更
        /// 新規武器追加が容易になる設計
        /// </summary>
        public void UpdateWeaponLoadout(WeaponData[] weapons)
        {
            if (_weaponLoadout != null)
            {
                _weaponLoadout.UpdateLoadout(weapons);
            }
        }

        /// <summary>
        /// Learn & Grow価値実現：段階的学習進捗チェック
        /// </summary>
        public bool ValidateLearningProgress()
        {
            // 基本的な設定値の妥当性チェック
            if (_defaultPlayerStats == null || _weaponLoadout == null || _gameModeSettings == null)
            {
                Debug.LogWarning("FPS Template: 必須設定が不足しています。Learn & Grow学習目標の達成に影響があります。");
                return false;
            }

            return true;
        }

        private void OnValidate()
        {
            // Editor上での設定値検証
            if (_targetGameplayDuration <= 0) _targetGameplayDuration = 15.0f;
            if (_estimatedLearningTime <= 0) _estimatedLearningTime = 12.0f;
        }
    }
}
