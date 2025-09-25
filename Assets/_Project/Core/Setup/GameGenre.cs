using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

namespace asterivo.Unity60.Core.Setup
{
    /// <summary>
    /// ゲームジャンル定義列挙型
    /// 6つのジャンル（FPS/TPS/Platformer/Stealth/Adventure/Strategy）をサポート
    /// </summary>
    public enum GameGenreType
    {
        FPS,            // First Person Shooter
        TPS,            // Third Person Shooter  
        Platformer,     // 3D Platformer
        Stealth,        // Stealth Action
        Adventure,      // Adventure
        Strategy        // Real-Time Strategy
    }

    /// <summary>
    /// ゲームジャンル設定ScriptableObject
    /// Phase 2 Clone & Create価値実現のための6ジャンル対応システム
    /// </summary>
    [CreateAssetMenu(fileName = "GameGenre_", menuName = "asterivo.Unity60/Setup/Game Genre", order = 1)]
    public class GameGenre : ScriptableObject
    {
        [Header("基本情報")]
        [SerializeField] private GameGenreType genreType;
        [SerializeField] private string displayName;
        [SerializeField, TextArea(3, 5)] private string description;
        
        [Header("プレビュー素材")]
        [SerializeField] private Texture2D previewImage;
        [SerializeField] private VideoClip previewVideo;
        [SerializeField] private AudioClip previewAudio;
        
        [Header("技術仕様")]
        [SerializeField] private CameraConfiguration cameraConfig;
        [SerializeField] private MovementConfiguration movementConfig;
        [SerializeField] private AIConfiguration aiConfig;
        [SerializeField] private AudioConfiguration audioConfig;
        
        [Header("推奨モジュール")]
        [SerializeField] private List<string> requiredModules = new List<string>();
        [SerializeField] private List<string> recommendedModules = new List<string>();
        [SerializeField] private List<string> optionalModules = new List<string>();
        
        [Header("パフォーマンス設定")]
        [SerializeField] private PerformanceProfile performanceProfile;
        
        [Header("プロジェクト設定")]
        [SerializeField] private ProjectSettings projectSettings;

        #region Properties
        
        public GameGenreType GenreType => genreType;
        public string DisplayName => displayName;
        public string Description => description;
        public Texture2D PreviewImage => previewImage;
        public VideoClip PreviewVideo => previewVideo;
        public AudioClip PreviewAudio => previewAudio;
        public CameraConfiguration CameraConfig => cameraConfig;
        public MovementConfiguration MovementConfig => movementConfig;
        public AIConfiguration AIConfig => aiConfig;
        public AudioConfiguration AudioConfig => audioConfig;
        public IReadOnlyList<string> RequiredModules => requiredModules.AsReadOnly();
        public IReadOnlyList<string> RecommendedModules => recommendedModules.AsReadOnly();
        public IReadOnlyList<string> OptionalModules => optionalModules.AsReadOnly();
        public PerformanceProfile PerformanceData => performanceProfile;
        public ProjectSettings ProjectData => projectSettings;
        
        #endregion
        
        #region Configuration Classes
        
        /// <summary>
        /// カメラ設定
        /// </summary>
        [Serializable]
        public class CameraConfiguration
        {
            [Header("視点設定")]
            public bool firstPersonView = false;
            public bool thirdPersonView = true;
            public float defaultFOV = 60f;
            public Vector3 thirdPersonOffset = new Vector3(0, 2, -5);
            
            [Header("カメラ制御")]
            public float mouseSensitivity = 2f;
            public bool invertY = false;
            public float smoothTime = 0.12f;
            
            [Header("Cinemachine設定")]
            public bool useCinemachine = true;
            public string virtualCameraPreset = "FreeLook";
        }
        
        /// <summary>
        /// 移動設定
        /// </summary>
        [Serializable]
        public class MovementConfiguration
        {
            [Header("基本移動")]
            public float walkSpeed = 3f;
            public float runSpeed = 6f;
            public float jumpHeight = 1.5f;
            
            [Header("特殊移動")]
            public bool crouchEnabled = false;
            public bool proneEnabled = false;
            public bool rollEnabled = false;
            public bool wallJumpEnabled = false;
            
            [Header("物理設定")]
            public float gravity = -9.81f;
            public float groundCheckDistance = 0.1f;
            public LayerMask groundLayers = 1;
        }
        
        /// <summary>
        /// AI設定
        /// </summary>
        [Serializable]
        public class AIConfiguration
        {
            [Header("NPC数")]
            public int maxNPCCount = 10;
            public int recommendedNPCCount = 5;
            
            [Header("検知設定")]
            public bool visualSensorEnabled = true;
            public bool auditorySensorEnabled = false;
            public float defaultDetectionRange = 10f;
            public float defaultFOV = 90f;
            
            [Header("行動AI")]
            public bool pathfindingEnabled = true;
            public bool combatAIEnabled = false;
            public bool stealthAIEnabled = false;
            public string defaultAIBehavior = "Patrol";
        }
        
        /// <summary>
        /// オーディオ設定
        /// </summary>
        [Serializable]
        public class AudioConfiguration
        {
            [Header("音響システム")]
            public bool use3DAudio = true;
            public bool useStealthAudio = false;
            public bool useEnvironmentalAudio = true;
            public bool useDynamicMusic = false;
            
            [Header("音量設定")]
            public float masterVolume = 1f;
            public float musicVolume = 0.7f;
            public float sfxVolume = 0.8f;
            public float voiceVolume = 0.9f;
        }
        
        /// <summary>
        /// パフォーマンス設定
        /// </summary>
        [Serializable]
        public class PerformanceProfile
        {
            [Header("描画設定")]
            public int targetFrameRate = 60;
            public bool vsyncEnabled = false;
            public int maxTextureSize = 2048;
            public int shadowResolution = 1024;
            
            [Header("最適化")]
            public bool frustumCullingEnabled = true;
            public bool occlusionCullingEnabled = false;
            public float lodBias = 1f;
            public int maximumLODLevel = 0;
        }
        
        /// <summary>
        /// プロジェクト設定
        /// </summary>
        [Serializable]
        public class ProjectSettings
        {
            [Header("ビルド設定")]
            public RuntimePlatform[] targetPlatforms = { RuntimePlatform.WindowsPlayer, RuntimePlatform.Android, RuntimePlatform.IPhonePlayer };
            public bool developmentBuild = true;
            public bool scriptDebugging = true;
            
            [Header("品質設定")]
            public string qualityLevel = "High";
            public bool hdrEnabled = true;
            public bool msaaEnabled = false;
            public int msaaLevel = 2;
            
            [Header("入力設定")]
            public bool newInputSystemEnabled = true;
            public bool legacyInputEnabled = false;
        }
        
        #endregion
        
        #region Utility Methods
        
        /// <summary>
        /// ジャンルに基づく推奨設定の適用
        /// </summary>
        public void ApplyRecommendedSettings()
        {
            switch (genreType)
            {
                case GameGenreType.FPS:
                    ApplyFPSSettings();
                    break;
                case GameGenreType.TPS:
                    ApplyTPSSettings();
                    break;
                case GameGenreType.Platformer:
                    ApplyPlatformerSettings();
                    break;
                case GameGenreType.Stealth:
                    ApplyStealthSettings();
                    break;
                case GameGenreType.Adventure:
                    ApplyAdventureSettings();
                    break;
                case GameGenreType.Strategy:
                    ApplyStrategySettings();
                    break;
            }
        }
        
        private void ApplyFPSSettings()
        {
            cameraConfig.firstPersonView = true;
            cameraConfig.thirdPersonView = false;
            cameraConfig.defaultFOV = 90f;
            
            movementConfig.walkSpeed = 4f;
            movementConfig.runSpeed = 8f;
            movementConfig.crouchEnabled = true;
            
            aiConfig.combatAIEnabled = true;
            aiConfig.maxNPCCount = 15;
            
            audioConfig.use3DAudio = true;
            audioConfig.useDynamicMusic = true;
        }
        
        private void ApplyTPSSettings()
        {
            cameraConfig.firstPersonView = false;
            cameraConfig.thirdPersonView = true;
            cameraConfig.thirdPersonOffset = new Vector3(1.5f, 2f, -4f);
            
            movementConfig.walkSpeed = 3.5f;
            movementConfig.runSpeed = 7f;
            movementConfig.rollEnabled = true;
            
            aiConfig.combatAIEnabled = true;
            aiConfig.maxNPCCount = 12;
        }
        
        private void ApplyPlatformerSettings()
        {
            cameraConfig.thirdPersonOffset = new Vector3(0, 3f, -8f);
            
            movementConfig.jumpHeight = 2.5f;
            movementConfig.wallJumpEnabled = true;
            
            aiConfig.maxNPCCount = 8;
            aiConfig.combatAIEnabled = false;
            
            performanceProfile.targetFrameRate = 60;
        }
        
        private void ApplyStealthSettings()
        {
            movementConfig.crouchEnabled = true;
            movementConfig.proneEnabled = true;
            
            aiConfig.visualSensorEnabled = true;
            aiConfig.auditorySensorEnabled = true;
            aiConfig.stealthAIEnabled = true;
            aiConfig.defaultDetectionRange = 15f;
            
            audioConfig.useStealthAudio = true;
            audioConfig.use3DAudio = true;
        }
        
        private void ApplyAdventureSettings()
        {
            movementConfig.walkSpeed = 2.5f;
            movementConfig.runSpeed = 5f;
            
            aiConfig.maxNPCCount = 5;
            aiConfig.combatAIEnabled = false;
            aiConfig.pathfindingEnabled = true;
            
            audioConfig.useEnvironmentalAudio = true;
            audioConfig.useDynamicMusic = true;
        }
        
        private void ApplyStrategySettings()
        {
            cameraConfig.defaultFOV = 45f;
            cameraConfig.thirdPersonOffset = new Vector3(0, 15f, -10f);
            
            aiConfig.maxNPCCount = 50;
            aiConfig.pathfindingEnabled = true;
            aiConfig.combatAIEnabled = true;
            
            performanceProfile.occlusionCullingEnabled = true;
            performanceProfile.lodBias = 0.7f;
        }
        
        #endregion
        
        #region Validation
        
        /// <summary>
        /// 設定の妥当性チェック
        /// </summary>
        public bool ValidateConfiguration()
        {
            if (string.IsNullOrEmpty(displayName))
            {
                UnityEngine.Debug.LogError($"GameGenre {name}: DisplayName is required");
                return false;
            }
            
            if (string.IsNullOrEmpty(description))
            {
                UnityEngine.Debug.LogError($"GameGenre {name}: Description is required");
                return false;
            }
            
            return true;
        }
        
        #endregion
        
        /// <summary>
        /// 設定のデバッグ情報出力
        /// </summary>
        public void LogConfiguration()
        {
            UnityEngine.Debug.Log($"=== Game Genre Configuration: {displayName} ===");
            UnityEngine.Debug.Log($"Type: {genreType}");
            UnityEngine.Debug.Log($"Required Modules: {string.Join(", ", requiredModules)}");
            UnityEngine.Debug.Log($"Camera: {(cameraConfig.firstPersonView ? "FPS" : "TPS")}, FOV: {cameraConfig.defaultFOV}");
            UnityEngine.Debug.Log($"Movement: Walk {movementConfig.walkSpeed}, Run {movementConfig.runSpeed}");
            UnityEngine.Debug.Log($"AI: Max NPCs {aiConfig.maxNPCCount}, Detection Range {aiConfig.defaultDetectionRange}");
        }
    }
}
