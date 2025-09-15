using UnityEngine;
using Sirenix.OdinInspector;
using asterivo.Unity60.Core.Templates;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Audio.Interfaces;

namespace asterivo.Unity60.Features.Templates.Platformer
{
    /// <summary>
    /// プラットフォーマーテンプレート統合設定システム
    /// 15分完全ゲームプレイ体験のための包括的設定管理
    /// DESIGN.md Layer 1: Template Configuration Layer準拠
    /// </summary>
    [CreateAssetMenu(
        fileName = "PlatformerTemplateConfiguration",
        menuName = "Platformer Template/Configuration",
        order = 1
    )]
    public class PlatformerTemplateConfiguration : GenreTemplateConfig
    {
        [TabGroup("Configuration", "Physics")]
        [Title("3D Platformer Physics Configuration", "15分ゲームプレイ最適化物理設定", TitleAlignments.Centered)]
        [SerializeField] private PlatformerPhysicsSettings physicsSettings = new PlatformerPhysicsSettings();

        [TabGroup("Configuration", "Player")]
        [Title("Player Controller Settings", "プレイヤー制御・状態管理設定", TitleAlignments.Centered)]
        [SerializeField] private PlatformerPlayerSettings playerSettings = new PlatformerPlayerSettings();

        [TabGroup("Configuration", "Camera")]
        [Title("Cinemachine Camera Configuration", "Cinemachine 3.1統合カメラ設定", TitleAlignments.Centered)]
        [SerializeField] private PlatformerCameraSettings cameraSettings = new PlatformerCameraSettings();

        [TabGroup("Configuration", "Collectibles")]
        [Title("Collectible & Progression System", "コレクタブル・進行システム", TitleAlignments.Centered)]
        [SerializeField] private PlatformerCollectibleSettings collectibleSettings = new PlatformerCollectibleSettings();

        [TabGroup("Configuration", "Platforms")]
        [Title("Dynamic Platform Systems", "動的プラットフォームシステム", TitleAlignments.Centered)]
        [SerializeField] private PlatformerPlatformSettings platformSettings = new PlatformerPlatformSettings();

        [TabGroup("Configuration", "Learning")]
        [Title("Learn & Grow Educational System", "学習支援・教育価値システム", TitleAlignments.Centered)]
        [SerializeField] private PlatformerLearningSettings learningSettings = new PlatformerLearningSettings();

        #region Public Properties
        public PlatformerPhysicsSettings Physics => physicsSettings;
        public PlatformerPlayerSettings Player => playerSettings;
        public PlatformerCameraSettings Camera => cameraSettings;
        public PlatformerCollectibleSettings Collectibles => collectibleSettings;
        public PlatformerPlatformSettings Platforms => platformSettings;
        public PlatformerLearningSettings Learning => learningSettings;
        #endregion

        #region GenreTemplateConfig Implementation
        public override void Initialize()
        {
            // 物理設定の初期化
            physicsSettings.Initialize();

            // プレイヤー設定の初期化
            playerSettings.Initialize();

            // カメラ設定の初期化
            cameraSettings.Initialize();

            // コレクタブル設定の初期化
            collectibleSettings.Initialize();

            // プラットフォーム設定の初期化
            platformSettings.Initialize();

            // 学習設定の初期化
            learningSettings.Initialize();

            Debug.Log($"[PlatformerTemplate] Configuration initialized for 15-minute gameplay experience");
        }

        public override bool CanAchieveBasicGameplayIn15Minutes()
        {
            // TASK-004受入条件：各ジャンル15分以内で基本ゲームプレイ実現
            return learningSettings.TargetCompletionTime <= 15f &&
                   physicsSettings.JumpForce > 0 &&
                   playerSettings.MovementSpeed > 0;
        }

        public override bool CanLoadSampleSceneIn30Seconds()
        {
            // TASK-004受入条件：サンプルシーン起動30秒以内
            // 軽量な3Dプラットフォーマーは高速ロード可能
            return platformSettings.MaxActivePlatforms <= 20 &&
                   platformSettings.EnableLOD;
        }

        public override bool CanSwitchTemplateIn3Minutes()
        {
            // TASK-004受入条件：テンプレート切り替え時のデータ整合性保証（3分以内）
            // プラットフォーマーテンプレートは軽量な設定のため高速切り替え可能
            return true;
        }

        public override bool Validate()
        {
            bool isValid = base.Validate();

            // 各設定の検証
            isValid &= physicsSettings.Validate();
            isValid &= playerSettings.Validate();
            isValid &= cameraSettings.Validate();
            isValid &= collectibleSettings.Validate();
            isValid &= platformSettings.Validate();
            isValid &= learningSettings.Validate();

            // 15分ゲームプレイ要件の検証
            isValid &= ValidateGameplayExperience();

            return isValid;
        }
        #endregion

        #region Gameplay Experience Validation
        private bool ValidateGameplayExperience()
        {
            // 15分ゲームプレイ体験の検証
            bool isValid = true;

            // 基本移動の検証
            if (physicsSettings.Gravity <= 0 || physicsSettings.JumpForce <= 0)
            {
                Debug.LogError("[PlatformerTemplate] Invalid physics settings for gameplay");
                isValid = false;
            }

            // コレクタブル目標の検証
            if (collectibleSettings.TargetScore <= 0)
            {
                Debug.LogError("[PlatformerTemplate] Invalid target score for 15-minute experience");
                isValid = false;
            }

            // 学習進行の検証
            if (learningSettings.TotalPhases < 5)
            {
                Debug.LogError("[PlatformerTemplate] Insufficient learning phases for Learn & Grow value");
                isValid = false;
            }

            return isValid;
        }
        #endregion

        #region Editor Support
#if UNITY_EDITOR
        [TabGroup("Configuration", "Debug")]
        [Button("Validate All Settings", ButtonSizes.Medium)]
        public void EditorValidateSettings()
        {
            bool isValid = Validate();
            string message = isValid ?
                "✅ All platformer settings are valid for 15-minute gameplay!" :
                "❌ Platformer settings validation failed. Check console for details.";

            Debug.Log($"[PlatformerTemplate] {message}");
        }

        [Button("Apply Recommended Settings", ButtonSizes.Medium)]
        public void ApplyRecommendedSettings()
        {
            physicsSettings.ApplyRecommendedSettings();
            playerSettings.ApplyRecommendedSettings();
            cameraSettings.ApplyRecommendedSettings();
            collectibleSettings.ApplyRecommendedSettings();
            platformSettings.ApplyRecommendedSettings();
            learningSettings.ApplyRecommendedSettings();

            Debug.Log("[PlatformerTemplate] Applied recommended settings for optimal 15-minute experience");
        }

        [Button("Generate Learning Report", ButtonSizes.Medium)]
        public void GenerateLearningReport()
        {
            learningSettings.GenerateProgressReport();
        }
#endif
        #endregion
    }
}