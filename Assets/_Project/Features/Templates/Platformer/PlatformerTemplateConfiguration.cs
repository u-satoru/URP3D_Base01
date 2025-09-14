using UnityEngine;
using asterivo.Unity60.Features.Templates.Common;

namespace asterivo.Unity60.Features.Templates.Platformer
{
    /// <summary>
    /// プラットフォーマーテンプレート固有設定システム
    /// TASK-004: 7ジャンルテンプレート完全実装 - Platformer Template Configuration（高優先）
    /// ジャンプ物理・移動システム・コレクタブルシステム・レベルデザインツール
    /// 名前空間: asterivo.Unity60.Features.Templates.Platformer.*
    /// </summary>
    [CreateAssetMenu(fileName = "PlatformerTemplateConfiguration", menuName = "Templates/Platformer/Configuration")]
    public class PlatformerTemplateConfiguration : ScriptableObject
    {
        [Header("ジャンプ・移動物理設定")]
        [SerializeField] private float _jumpForce = 12f;
        [SerializeField] private float _movementSpeed = 6f;
        [SerializeField] private float _airMovementMultiplier = 0.8f;
        [SerializeField] private int _maxJumps = 2; // ダブルジャンプ対応
        [SerializeField] private bool _enableWallJump = true;
        [SerializeField] private bool _enableWallSliding = true;
        [SerializeField] private float _wallSlideSpeed = 2f;
        [SerializeField] private float _wallJumpForce = 10f;
        
        [Header("重力・物理演算設定")]
        [SerializeField] private float _gravityScale = 3f;
        [SerializeField] private float _fallGravityMultiplier = 2.5f;
        [SerializeField] private float _lowJumpMultiplier = 2f;
        [SerializeField] private bool _enableCoyoteTime = true;
        [SerializeField] private float _coyoteTimeDuration = 0.15f;
        [SerializeField] private bool _enableJumpBuffering = true;
        [SerializeField] private float _jumpBufferDuration = 0.2f;
        
        [Header("地面・コリジョン検知")]
        [SerializeField] private LayerMask _groundLayerMask = 1 << 0;
        [SerializeField] private LayerMask _wallLayerMask = 1 << 0;
        [SerializeField] private float _groundCheckRadius = 0.3f;
        [SerializeField] private float _wallCheckDistance = 0.6f;
        [SerializeField] private Vector2 _groundCheckOffset = new Vector2(0f, -1f);
        
        [Header("コレクタブルシステム設定")]
        [SerializeField] private bool _enableCollectibles = true;
        [SerializeField] private int _coinValue = 100;
        [SerializeField] private int _gemValue = 500;
        [SerializeField] private bool _enablePowerUps = true;
        [SerializeField] private float _powerUpDuration = 10f;
        [SerializeField] private bool _enableMagnetEffect = true;
        [SerializeField] private float _magnetRadius = 5f;
        
        [Header("チェックポイント・リスポーンシステム")]
        [SerializeField] private bool _enableCheckpoints = true;
        [SerializeField] private bool _enableAutoSave = true;
        [SerializeField] private float _respawnDelay = 1f;
        [SerializeField] private bool _enableRespawnAnimation = true;
        [SerializeField] private Vector3 _respawnOffset = new Vector3(0f, 2f, 0f);
        
        [Header("移動床・環境ギミック設定")]
        [SerializeField] private bool _enableMovingPlatforms = true;
        [SerializeField] private bool _enableSpringPlatforms = true;
        [SerializeField] private float _springForceMultiplier = 1.5f;
        [SerializeField] private bool _enableConveyorBelts = true;
        [SerializeField] private float _conveyorSpeed = 3f;
        [SerializeField] private bool _enableSpikes = true;
        [SerializeField] private int _spikeDamage = 1;
        
        [Header("カメラシステム設定")]
        [SerializeField] private bool _enableCameraFollow = true;
        [SerializeField] private float _cameraFollowSpeed = 5f;
        [SerializeField] private Vector2 _cameraOffset = new Vector2(0f, 2f);
        [SerializeField] private bool _enableLookAhead = true;
        [SerializeField] private float _lookAheadDistance = 3f;
        [SerializeField] private bool _enableCameraBounds = true;
        [SerializeField] private Bounds _cameraBounds;
        
        [Header("エフェクト・視覚表現設定")]
        [SerializeField] private bool _enableJumpEffects = true;
        [SerializeField] private bool _enableLandingEffects = true;
        [SerializeField] private bool _enableDustEffects = true;
        [SerializeField] private bool _enableCollectionEffects = true;
        [SerializeField] private string _jumpEffectName = "JumpParticle";
        [SerializeField] private string _landingEffectName = "LandingParticle";
        
        [Header("オーディオシステム設定")]
        [SerializeField] private bool _enableJumpSounds = true;
        [SerializeField] private bool _enableLandingSounds = true;
        [SerializeField] private bool _enableFootstepSounds = true;
        [SerializeField] private bool _enableCollectionSounds = true;
        [SerializeField] private bool _enableBackgroundMusic = true;
        [SerializeField] private float _musicVolume = 0.7f;
        
        [Header("UI・フィードバック設定")]
        [SerializeField] private bool _enableScoreDisplay = true;
        [SerializeField] private bool _enableLivesDisplay = true;
        [SerializeField] private bool _enableProgressBar = true;
        [SerializeField] private bool _enableMinimap = false;
        [SerializeField] private int _defaultLives = 3;
        [SerializeField] private bool _enableComboSystem = true;
        [SerializeField] private float _comboTimeout = 2f;
        
        [Header("学習目標設定（Learn & Grow対応）")]
        [SerializeField] private string[] _learningObjectives = new[]
        {
            "BasicMovement_Platformer",
            "Jump_Mechanics",
            "Double_Jump_Mastery",
            "Wall_Jump_Techniques",
            "Collectible_Gathering",
            "Checkpoint_System_Usage",
            "Environmental_Navigation",
            "Timing_Based_Challenges",
            "Platform_Variety_Mastery",
            "Advanced_Movement_Combos"
        };
        
        [Header("レベルデザインガイドライン")]
        [SerializeField] private float _recommendedLevelLength = 200f;
        [SerializeField] private float _recommendedDifficultyCurve = 1.2f;
        [SerializeField] private int _recommendedCheckpointInterval = 50;
        [SerializeField] private bool _enableDifficultyScaling = true;
        [SerializeField] private float _playerSkillAdaptation = 0.1f;
        
        // Properties - Movement & Physics
        public float JumpForce => _jumpForce;
        public float MovementSpeed => _movementSpeed;
        public float AirMovementMultiplier => _airMovementMultiplier;
        public int MaxJumps => _maxJumps;
        public bool EnableWallJump => _enableWallJump;
        public bool EnableWallSliding => _enableWallSliding;
        public float WallSlideSpeed => _wallSlideSpeed;
        public float WallJumpForce => _wallJumpForce;
        
        // Gravity Properties
        public float GravityScale => _gravityScale;
        public float FallGravityMultiplier => _fallGravityMultiplier;
        public float LowJumpMultiplier => _lowJumpMultiplier;
        public bool EnableCoyoteTime => _enableCoyoteTime;
        public float CoyoteTimeDuration => _coyoteTimeDuration;
        public bool EnableJumpBuffering => _enableJumpBuffering;
        public float JumpBufferDuration => _jumpBufferDuration;
        
        // Collision Properties
        public LayerMask GroundLayerMask => _groundLayerMask;
        public LayerMask WallLayerMask => _wallLayerMask;
        public float GroundCheckRadius => _groundCheckRadius;
        public float WallCheckDistance => _wallCheckDistance;
        public Vector2 GroundCheckOffset => _groundCheckOffset;
        
        // Collectible Properties
        public bool EnableCollectibles => _enableCollectibles;
        public int CoinValue => _coinValue;
        public int GemValue => _gemValue;
        public bool EnablePowerUps => _enablePowerUps;
        public float PowerUpDuration => _powerUpDuration;
        public bool EnableMagnetEffect => _enableMagnetEffect;
        public float MagnetRadius => _magnetRadius;
        
        // Checkpoint Properties
        public bool EnableCheckpoints => _enableCheckpoints;
        public bool EnableAutoSave => _enableAutoSave;
        public float RespawnDelay => _respawnDelay;
        public bool EnableRespawnAnimation => _enableRespawnAnimation;
        public Vector3 RespawnOffset => _respawnOffset;
        
        // Environmental Properties
        public bool EnableMovingPlatforms => _enableMovingPlatforms;
        public bool EnableSpringPlatforms => _enableSpringPlatforms;
        public float SpringForceMultiplier => _springForceMultiplier;
        public bool EnableConveyorBelts => _enableConveyorBelts;
        public float ConveyorSpeed => _conveyorSpeed;
        public bool EnableSpikes => _enableSpikes;
        public int SpikeDamage => _spikeDamage;
        
        // Camera Properties
        public bool EnableCameraFollow => _enableCameraFollow;
        public float CameraFollowSpeed => _cameraFollowSpeed;
        public Vector2 CameraOffset => _cameraOffset;
        public bool EnableLookAhead => _enableLookAhead;
        public float LookAheadDistance => _lookAheadDistance;
        public bool EnableCameraBounds => _enableCameraBounds;
        public Bounds CameraBounds => _cameraBounds;
        
        // Effect Properties
        public bool EnableJumpEffects => _enableJumpEffects;
        public bool EnableLandingEffects => _enableLandingEffects;
        public bool EnableDustEffects => _enableDustEffects;
        public bool EnableCollectionEffects => _enableCollectionEffects;
        public string JumpEffectName => _jumpEffectName;
        public string LandingEffectName => _landingEffectName;
        
        // Audio Properties
        public bool EnableJumpSounds => _enableJumpSounds;
        public bool EnableLandingSounds => _enableLandingSounds;
        public bool EnableFootstepSounds => _enableFootstepSounds;
        public bool EnableCollectionSounds => _enableCollectionSounds;
        public bool EnableBackgroundMusic => _enableBackgroundMusic;
        public float MusicVolume => _musicVolume;
        
        // UI Properties
        public bool EnableScoreDisplay => _enableScoreDisplay;
        public bool EnableLivesDisplay => _enableLivesDisplay;
        public bool EnableProgressBar => _enableProgressBar;
        public bool EnableMinimap => _enableMinimap;
        public int DefaultLives => _defaultLives;
        public bool EnableComboSystem => _enableComboSystem;
        public float ComboTimeout => _comboTimeout;
        
        // Learning Properties
        public string[] LearningObjectives => _learningObjectives;
        
        // Level Design Properties
        public float RecommendedLevelLength => _recommendedLevelLength;
        public float RecommendedDifficultyCurve => _recommendedDifficultyCurve;
        public int RecommendedCheckpointInterval => _recommendedCheckpointInterval;
        public bool EnableDifficultyScaling => _enableDifficultyScaling;
        public float PlayerSkillAdaptation => _playerSkillAdaptation;
        
        /// <summary>
        /// プレイヤー移動コントローラーセットアップ
        /// </summary>
        /// <param name="player">プレイヤーGameObject</param>
        public void SetupPlayerController(GameObject player)
        {
            if (player == null) return;
            
            Debug.Log("Setting up Platformer player controller...");
            
            // Rigidbody2Dセットアップ
            var rigidbody = player.GetComponent<Rigidbody2D>();
            if (rigidbody == null)
            {
                rigidbody = player.AddComponent<Rigidbody2D>();
            }
            
            rigidbody.gravityScale = _gravityScale;
            rigidbody.freezeRotation = true;
            
            // TODO: PlatformerMovementController追加
            // TODO: JumpController追加
            // TODO: WallJumpController追加（EnableWallJumpがtrueの場合）
            
            Debug.Log($"Player controller setup completed for {player.name}");
        }
        
        /// <summary>
        /// カメラシステムセットアップ
        /// </summary>
        /// <param name="camera">メインカメラ</param>
        /// <param name="player">追従対象プレイヤー</param>
        public void SetupCameraSystem(Camera camera, GameObject player)
        {
            if (camera == null || player == null) return;
            
            Debug.Log("Setting up Platformer camera system...");
            
            if (_enableCameraFollow)
            {
                // TODO: PlatformerCameraController追加
                Debug.Log($"Camera follow setup: Speed={_cameraFollowSpeed}, Offset={_cameraOffset}");
            }
            
            if (_enableLookAhead)
            {
                // TODO: LookAheadCameraController追加
                Debug.Log($"Look ahead setup: Distance={_lookAheadDistance}");
            }
            
            if (_enableCameraBounds)
            {
                // TODO: CameraBoundsController追加
                Debug.Log($"Camera bounds setup: {_cameraBounds}");
            }
        }
        
        /// <summary>
        /// コレクタブルシステムセットアップ
        /// </summary>
        /// <param name="levelRoot">レベルルートGameObject</param>
        public void SetupCollectibleSystem(GameObject levelRoot)
        {
            if (levelRoot == null || !_enableCollectibles) return;
            
            Debug.Log("Setting up collectible system...");
            
            // TODO: CollectibleManager追加
            // TODO: ScoreManager追加
            
            if (_enableMagnetEffect)
            {
                // TODO: MagnetController追加
                Debug.Log($"Magnet effect setup: Radius={_magnetRadius}");
            }
            
            if (_enableComboSystem)
            {
                // TODO: ComboManager追加
                Debug.Log($"Combo system setup: Timeout={_comboTimeout}s");
            }
            
            Debug.Log($"Collectible system setup: Coin={_coinValue}pts, Gem={_gemValue}pts");
        }
        
        /// <summary>
        /// 環境ギミックセットアップ
        /// </summary>
        /// <param name="levelRoot">レベルルートGameObject</param>
        public void SetupEnvironmentalElements(GameObject levelRoot)
        {
            if (levelRoot == null) return;
            
            Debug.Log("Setting up environmental elements...");
            
            if (_enableMovingPlatforms)
            {
                // TODO: MovingPlatformController追加
                Debug.Log("Moving platforms enabled");
            }
            
            if (_enableSpringPlatforms)
            {
                // TODO: SpringPlatformController追加
                Debug.Log($"Spring platforms setup: Force multiplier={_springForceMultiplier}");
            }
            
            if (_enableConveyorBelts)
            {
                // TODO: ConveyorBeltController追加
                Debug.Log($"Conveyor belts setup: Speed={_conveyorSpeed}");
            }
            
            if (_enableSpikes)
            {
                // TODO: SpikeHazardController追加
                Debug.Log($"Spikes setup: Damage={_spikeDamage}");
            }
        }
        
        /// <summary>
        /// チェックポイントシステムセットアップ
        /// </summary>
        /// <param name="levelRoot">レベルルートGameObject</param>
        public void SetupCheckpointSystem(GameObject levelRoot)
        {
            if (levelRoot == null || !_enableCheckpoints) return;
            
            Debug.Log("Setting up checkpoint system...");
            
            // TODO: CheckpointManager追加
            // TODO: RespawnManager追加
            
            Debug.Log($"Checkpoint system: AutoSave={_enableAutoSave}, RespawnDelay={_respawnDelay}s");
        }
        
        /// <summary>
        /// レベルデザインガイドライン適用
        /// </summary>
        /// <param name="levelData">レベルデータ</param>
        public void ApplyLevelDesignGuidelines(object levelData)
        {
            Debug.Log("Applying level design guidelines...");
            Debug.Log($"Recommended length: {_recommendedLevelLength}m");
            Debug.Log($"Difficulty curve: {_recommendedDifficultyCurve}");
            Debug.Log($"Checkpoint interval: {_recommendedCheckpointInterval}m");
            
            if (_enableDifficultyScaling)
            {
                Debug.Log($"Dynamic difficulty enabled: Adaptation rate={_playerSkillAdaptation}");
            }
        }
        
        /// <summary>
        /// 学習目標の進捗チェック
        /// </summary>
        /// <param name="completedObjectives">完了した学習目標</param>
        /// <returns>全体完了率（0-1）</returns>
        public float CalculateLearningProgress(string[] completedObjectives)
        {
            if (_learningObjectives.Length == 0)
                return 1f;
                
            int completedCount = 0;
            foreach (var objective in _learningObjectives)
            {
                if (System.Array.Exists(completedObjectives, completed => completed == objective))
                {
                    completedCount++;
                }
            }
            
            var progress = (float)completedCount / _learningObjectives.Length;
            Debug.Log($"Platformer learning progress: {progress:P} ({completedCount}/{_learningObjectives.Length})");
            
            return progress;
        }
        
        /// <summary>
        /// デバッグ情報を表示
        /// </summary>
        [ContextMenu("Print Platformer Configuration")]
        public void PrintConfiguration()
        {
            Debug.Log("=== Platformer Template Configuration ===");
            Debug.Log($"Movement: Speed={_movementSpeed}, Jump={_jumpForce}, MaxJumps={_maxJumps}");
            Debug.Log($"Physics: Gravity={_gravityScale}, Fall={_fallGravityMultiplier}");
            Debug.Log($"Special Abilities: WallJump={_enableWallJump}, CoyoteTime={_enableCoyoteTime}");
            Debug.Log($"Collectibles: Enabled={_enableCollectibles}, Magnet={_enableMagnetEffect}");
            Debug.Log($"Camera: Follow={_enableCameraFollow}, LookAhead={_enableLookAhead}");
            Debug.Log($"Learning Objectives: {_learningObjectives.Length} objectives");
        }
    }
}