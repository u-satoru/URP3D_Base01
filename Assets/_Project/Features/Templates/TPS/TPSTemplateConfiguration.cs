using UnityEngine;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Commands;
using System.Collections.Generic;

namespace asterivo.Unity60.Features.Templates.TPS
{
    /// <summary>
    /// TPS（三人称シューティング）ジャンル特化テンプレート設定
    /// Cinemachine 3.1統合とカバーシステムを含む15分間のゲームプレイ
    /// </summary>
    [CreateAssetMenu(fileName = "TPSTemplateConfiguration", menuName = "Templates/TPS/Configuration")]
    public class TPSTemplateConfiguration : ScriptableObject
    {
        [Header("TPS Core Settings")]
        [SerializeField] private bool enableThirdPersonView = true;
        [SerializeField] private float cameraDistance = 5.0f;
        [SerializeField] private float cameraHeight = 2.0f;
        [SerializeField] private float cameraSensitivity = 2.0f;
        [SerializeField] private float movementSpeed = 4.0f;
        [SerializeField] private bool enableSprint = true;
        [SerializeField] private float sprintMultiplier = 1.8f;

        [Header("Cover System Settings")]
        [SerializeField] private bool enableCoverSystem = true;
        [SerializeField] private float coverDetectionRange = 1.5f;
        [SerializeField] private LayerMask coverLayerMask = 1;
        [SerializeField] private float peekDistance = 0.8f;
        [SerializeField] private float coverTransitionSpeed = 5.0f;

        [Header("Combat System")]
        [SerializeField] private float baseDamage = 30.0f;
        [SerializeField] private float fireRate = 0.15f; // Slower than FPS
        [SerializeField] private int magazineSize = 25;
        [SerializeField] private float reloadTime = 2.5f;
        [SerializeField] private bool enableAiming = true;
        [SerializeField] private float aimTransitionSpeed = 8.0f;

        [Header("Camera States (Cinemachine Integration)")]
        [SerializeField] private string thirdPersonCameraName = "TPSCamera_Normal";
        [SerializeField] private string aimCameraName = "TPSCamera_Aim";
        [SerializeField] private string coverCameraName = "TPSCamera_Cover";
        [SerializeField] private float cameraBlendTime = 0.3f;

        [Header("Health & Armor System")]
        [SerializeField] private float maxHealth = 120.0f;
        [SerializeField] private float maxArmor = 80.0f;
        [SerializeField] private float healthRegenRate = 3.0f; // Slower than FPS
        [SerializeField] private float healthRegenDelay = 7.0f;

        [Header("Movement & Animation")]
        [SerializeField] private bool enableRolling = true;
        [SerializeField] private float rollDistance = 3.0f;
        [SerializeField] private float rollCooldown = 2.0f;
        [SerializeField] private bool enableWallRun = false; // Advanced feature

        [Header("15-Minute Learning Objectives")]
        [SerializeField] private List<TPSLearningObjective> learningObjectives = new List<TPSLearningObjective>();

        [Header("Weapon Configuration")]
        [SerializeField] private List<TPSWeaponSettings> availableWeapons = new List<TPSWeaponSettings>();

        [Header("Audio Settings")]
        [SerializeField] private AudioClip[] gunShotSounds;
        [SerializeField] private AudioClip[] reloadSounds;
        [SerializeField] private AudioClip[] coverSounds;
        [SerializeField] private AudioClip[] rollSounds;
        [SerializeField] private float masterVolume = 1.0f;

        [Header("Events")]
        [SerializeField] private GameEvent onTPSGameplayStarted;
        [SerializeField] private GameEvent onCoverEntered;
        [SerializeField] private GameEvent onCoverExited;
        [SerializeField] private GameEvent onEnemyEliminated;
        [SerializeField] private GameEvent onObjectiveCompleted;

        // Properties
        public float CameraDistance => cameraDistance;
        public float CameraHeight => cameraHeight;
        public float CameraSensitivity => cameraSensitivity;
        public float MovementSpeed => movementSpeed;
        public float BaseDamage => baseDamage;
        public float FireRate => fireRate;
        public int MagazineSize => magazineSize;
        public float ReloadTime => reloadTime;
        public float MaxHealth => maxHealth;
        public float MaxArmor => maxArmor;
        public bool EnableCoverSystem => enableCoverSystem;
        public float CoverDetectionRange => coverDetectionRange;

        /// <summary>
        /// TPSプレイヤーコントローラーの初期設定
        /// </summary>
        public void SetupPlayerController(GameObject player)
        {
            if (player == null) return;

            // Character Controller Setup
            var characterController = player.GetComponent<CharacterController>();
            if (characterController == null)
            {
                characterController = player.AddComponent<CharacterController>();
                characterController.height = 1.8f;
                characterController.radius = 0.4f; // Slightly larger for TPS
                characterController.stepOffset = 0.4f;
                characterController.slopeLimit = 45f;
            }

            // TPS Player Controller
            if (player.GetComponent<TPSPlayerController>() == null)
            {
                var tpsController = player.AddComponent<TPSPlayerController>();
                tpsController.Initialize(this);
            }

            // Cover System
            if (enableCoverSystem && player.GetComponent<TPSCoverSystem>() == null)
            {
                var coverSystem = player.AddComponent<TPSCoverSystem>();
                coverSystem.Initialize(this);
            }

            // Health System
            SetupHealthSystem(player);

            // Animation Controller
            SetupAnimationController(player);

            Debug.Log($"[TPSTemplate] Player controller setup completed for {player.name}");
        }

        /// <summary>
        /// Cinemachine カメラシステムの設定
        /// </summary>
        public void SetupCinemachineCameras(GameObject player)
        {
            if (player == null) return;

            // TPS Camera Manager
            if (player.GetComponent<TPSCameraManager>() == null)
            {
                var cameraManager = player.AddComponent<TPSCameraManager>();
                cameraManager.Initialize(this);
            }

            // Find or create Cinemachine Virtual Cameras
            SetupVirtualCameras(player);

            Debug.Log($"[TPSTemplate] Cinemachine camera system setup completed");
        }

        /// <summary>
        /// Virtual Camerasの設定
        /// </summary>
        private void SetupVirtualCameras(GameObject player)
        {
            var cmBrain = Camera.main?.GetComponent<Cinemachine.CinemachineBrain>();
            if (cmBrain == null)
            {
                Debug.LogWarning("[TPSTemplate] No CinemachineBrain found on main camera");
                return;
            }

            // Normal TPS Camera
            CreateVirtualCamera(thirdPersonCameraName, player, CameraType.Normal);
            
            // Aim Camera
            CreateVirtualCamera(aimCameraName, player, CameraType.Aim);
            
            // Cover Camera
            if (enableCoverSystem)
            {
                CreateVirtualCamera(coverCameraName, player, CameraType.Cover);
            }
        }

        /// <summary>
        /// 個別Virtual Cameraの作成
        /// </summary>
        private void CreateVirtualCamera(string cameraName, GameObject player, CameraType type)
        {
            var existingCamera = GameObject.Find(cameraName);
            if (existingCamera != null) return;

            var vcamGO = new GameObject(cameraName);
            var vcam = vcamGO.AddComponent<Cinemachine.CinemachineVirtualCamera>();

            // Follow and Look At setup
            vcam.Follow = player.transform;
            vcam.LookAt = player.transform;

            // Configure based on camera type
            switch (type)
            {
                case CameraType.Normal:
                    var composer = vcam.AddCinemachineComponent<Cinemachine.Cinemachine3rdPersonFollow>();
                    composer.CameraDistance = cameraDistance;
                    composer.CameraCollisionFilter = coverLayerMask;
                    vcam.Priority = 10;
                    break;

                case CameraType.Aim:
                    var aimComposer = vcam.AddCinemachineComponent<Cinemachine.Cinemachine3rdPersonFollow>();
                    aimComposer.CameraDistance = cameraDistance * 0.7f; // Closer when aiming
                    aimComposer.CameraSide = 0.5f; // Offset to the right
                    vcam.Priority = 5;
                    break;

                case CameraType.Cover:
                    var coverComposer = vcam.AddCinemachineComponent<Cinemachine.Cinemachine3rdPersonFollow>();
                    coverComposer.CameraDistance = cameraDistance * 0.8f;
                    coverComposer.CameraSide = 0.8f; // More offset for cover
                    vcam.Priority = 5;
                    break;
            }
        }

        /// <summary>
        /// ヘルス・アーマーシステムの設定
        /// </summary>
        private void SetupHealthSystem(GameObject player)
        {
            if (player.GetComponent<TPSHealthSystem>() == null)
            {
                var healthSystem = player.AddComponent<TPSHealthSystem>();
                healthSystem.Initialize(maxHealth, maxArmor, healthRegenRate, healthRegenDelay);
            }
        }

        /// <summary>
        /// アニメーションコントローラーの設定
        /// </summary>
        private void SetupAnimationController(GameObject player)
        {
            var animator = player.GetComponent<Animator>();
            if (animator == null)
            {
                animator = player.AddComponent<Animator>();
                // TODO: Set default TPS animation controller
            }

            if (player.GetComponent<TPSAnimationController>() == null)
            {
                var animController = player.AddComponent<TPSAnimationController>();
                animController.Initialize(this);
            }
        }

        /// <summary>
        /// 武器システムの設定
        /// </summary>
        public void SetupWeaponSystem(GameObject player)
        {
            if (player == null || availableWeapons.Count == 0) return;

            // Weapon Manager
            if (player.GetComponent<TPSWeaponManager>() == null)
            {
                var weaponManager = player.AddComponent<TPSWeaponManager>();
                weaponManager.Initialize(availableWeapons, this);
            }

            // Crosshair UI Setup for TPS
            SetupTPSUI();

            Debug.Log($"[TPSTemplate] Weapon system setup completed with {availableWeapons.Count} weapons");
        }

        /// <summary>
        /// TPS UI（クロスヘア、カバーインジケーター等）の設定
        /// </summary>
        private void SetupTPSUI()
        {
            var canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                // Crosshair
                var crosshair = canvas.transform.Find("TPSCrosshair");
                if (crosshair == null)
                {
                    var crosshairGO = new GameObject("TPSCrosshair");
                    crosshairGO.transform.SetParent(canvas.transform);
                    
                    var rectTransform = crosshairGO.AddComponent<RectTransform>();
                    rectTransform.anchoredPosition = Vector2.zero;
                    rectTransform.sizeDelta = new Vector2(30, 30);

                    var crosshairImage = crosshairGO.AddComponent<UnityEngine.UI.Image>();
                    crosshairImage.color = new Color(1f, 1f, 1f, 0.8f);
                }

                // Cover Indicator
                if (enableCoverSystem)
                {
                    var coverIndicator = canvas.transform.Find("CoverIndicator");
                    if (coverIndicator == null)
                    {
                        var indicatorGO = new GameObject("CoverIndicator");
                        indicatorGO.transform.SetParent(canvas.transform);
                        
                        var rectTransform = indicatorGO.AddComponent<RectTransform>();
                        rectTransform.anchoredPosition = new Vector2(0, -100);
                        rectTransform.sizeDelta = new Vector2(200, 50);

                        var indicatorText = indicatorGO.AddComponent<UnityEngine.UI.Text>();
                        indicatorText.text = "Press [E] to take cover";
                        indicatorText.color = Color.yellow;
                        indicatorText.alignment = TextAnchor.MiddleCenter;
                        indicatorGO.SetActive(false); // Hidden by default
                    }
                }
            }
        }

        /// <summary>
        /// AI敵システムの設定（TPS特化）
        /// </summary>
        public void SetupEnemySystem(GameObject[] enemies)
        {
            if (enemies == null) return;

            foreach (var enemy in enemies)
            {
                if (enemy == null) continue;

                // TPS Enemy Controller
                if (enemy.GetComponent<TPSEnemyController>() == null)
                {
                    var enemyController = enemy.AddComponent<TPSEnemyController>();
                    enemyController.Initialize(this);
                }

                // Health System for enemies
                if (enemy.GetComponent<TPSHealthSystem>() == null)
                {
                    var healthSystem = enemy.AddComponent<TPSHealthSystem>();
                    healthSystem.Initialize(maxHealth * 0.9f, maxArmor * 0.5f, 0f, 0f); // Enemies have less health, some armor
                }

                // Cover usage for enemies
                if (enableCoverSystem && enemy.GetComponent<TPSEnemyCoverSystem>() == null)
                {
                    var coverSystem = enemy.AddComponent<TPSEnemyCoverSystem>();
                    coverSystem.Initialize(this);
                }
            }

            Debug.Log($"[TPSTemplate] Enemy system setup completed for {enemies.Length} enemies");
        }

        /// <summary>
        /// 学習目標の初期化と追跡
        /// </summary>
        public void InitializeLearningObjectives()
        {
            if (learningObjectives.Count == 0)
            {
                CreateDefaultLearningObjectives();
            }

            foreach (var objective in learningObjectives)
            {
                objective.Initialize();
                Debug.Log($"[TPSTemplate] Learning objective initialized: {objective.Title}");
            }
        }

        /// <summary>
        /// デフォルト学習目標の作成（15分間の段階的学習）
        /// </summary>
        private void CreateDefaultLearningObjectives()
        {
            learningObjectives.Clear();
            
            // Phase 1: Basic Movement & Camera (0-3 minutes)
            learningObjectives.Add(new TPSLearningObjective
            {
                Id = "tps_movement_camera",
                Title = "三人称移動とカメラ操作",
                Description = "WASD移動、マウスカメラ、スプリントを習得",
                EstimatedTime = 180f,
                Priority = 1
            });

            // Phase 2: Cover System (3-6 minutes)
            learningObjectives.Add(new TPSLearningObjective
            {
                Id = "tps_cover_system",
                Title = "カバーシステム活用",
                Description = "カバーの取り方、ピーキング、カバー間移動",
                EstimatedTime = 180f,
                Priority = 2
            });

            // Phase 3: Combat & Aiming (6-9 minutes)
            learningObjectives.Add(new TPSLearningObjective
            {
                Id = "tps_combat_aiming",
                Title = "戦闘・照準システム",
                Description = "射撃、エイムモード、リロード操作の習得",
                EstimatedTime = 180f,
                Priority = 3
            });

            // Phase 4: Advanced Tactics (9-12 minutes)
            learningObjectives.Add(new TPSLearningObjective
            {
                Id = "tps_advanced_tactics",
                Title = "高度戦術テクニック",
                Description = "ローリング回避、フランキング、連続カバー移動",
                EstimatedTime = 180f,
                Priority = 4
            });

            // Phase 5: Mission Mastery (12-15 minutes)
            learningObjectives.Add(new TPSLearningObjective
            {
                Id = "tps_mission_mastery",
                Title = "戦術ミッション完遂",
                Description = "全スキル統合による実戦ミッションクリア",
                EstimatedTime = 180f,
                Priority = 5
            });

            Debug.Log($"[TPSTemplate] Created {learningObjectives.Count} default learning objectives");
        }

        /// <summary>
        /// ゲームプレイイベントの発行
        /// </summary>
        public void RaiseGameplayEvent(string eventType)
        {
            switch (eventType.ToLower())
            {
                case "started":
                    onTPSGameplayStarted?.Raise();
                    break;
                case "cover_entered":
                    onCoverEntered?.Raise();
                    break;
                case "cover_exited":
                    onCoverExited?.Raise();
                    break;
                case "enemy_eliminated":
                    onEnemyEliminated?.Raise();
                    break;
                case "objective_completed":
                    onObjectiveCompleted?.Raise();
                    break;
                default:
                    Debug.LogWarning($"[TPSTemplate] Unknown gameplay event: {eventType}");
                    break;
            }
        }

        /// <summary>
        /// TPS設定の検証
        /// </summary>
        public bool ValidateConfiguration()
        {
            bool isValid = true;

            if (cameraDistance <= 0f)
            {
                Debug.LogError("[TPSTemplate] Camera distance must be greater than 0");
                isValid = false;
            }

            if (cameraSensitivity <= 0f)
            {
                Debug.LogError("[TPSTemplate] Camera sensitivity must be greater than 0");
                isValid = false;
            }

            if (enableCoverSystem && coverDetectionRange <= 0f)
            {
                Debug.LogError("[TPSTemplate] Cover detection range must be greater than 0");
                isValid = false;
            }

            if (baseDamage <= 0f)
            {
                Debug.LogError("[TPSTemplate] Base damage must be greater than 0");
                isValid = false;
            }

            if (learningObjectives.Count == 0)
            {
                Debug.LogWarning("[TPSTemplate] No learning objectives defined");
            }

            return isValid;
        }

        private enum CameraType
        {
            Normal,
            Aim,
            Cover
        }
    }

    /// <summary>
    /// TPS学習目標データ構造
    /// </summary>
    [System.Serializable]
    public class TPSLearningObjective
    {
        public string Id;
        public string Title;
        [TextArea(2, 4)]
        public string Description;
        public float EstimatedTime;
        public int Priority;
        public bool IsCompleted;
        public float CompletionPercentage;

        public void Initialize()
        {
            IsCompleted = false;
            CompletionPercentage = 0f;
        }

        public void UpdateProgress(float progress)
        {
            CompletionPercentage = Mathf.Clamp01(progress);
            if (CompletionPercentage >= 1.0f && !IsCompleted)
            {
                IsCompleted = true;
                Debug.Log($"[TPSTemplate] Learning objective completed: {Title}");
            }
        }
    }

    /// <summary>
    /// TPS武器設定データ構造
    /// </summary>
    [System.Serializable]
    public class TPSWeaponSettings
    {
        public string weaponName;
        public float damage;
        public float fireRate;
        public int magazineSize;
        public float reloadTime;
        public float range;
        public float accuracy; // TPS has accuracy considerations
        public bool isAutomatic;
        public bool canUseInCover; // TPS-specific
        public AudioClip fireSound;
        public AudioClip reloadSound;
    }
}