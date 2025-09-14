using UnityEngine;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Commands;
using System.Collections.Generic;

namespace asterivo.Unity60.Features.Templates.FPS
{
    /// <summary>
    /// FPS（一人称シューティング）ジャンル特化テンプレート設定
    /// 15分間のゲームプレイと段階的学習を提供
    /// </summary>
    [CreateAssetMenu(fileName = "FPSTemplateConfiguration", menuName = "Templates/FPS/Configuration")]
    public class FPSTemplateConfiguration : ScriptableObject
    {
        [Header("FPS Core Settings")]
        [SerializeField] private bool enableFirstPersonView = true;
        [SerializeField] private float mouseSensitivity = 2.0f;
        [SerializeField] private float movementSpeed = 5.0f;
        [SerializeField] private bool enableSprint = true;
        [SerializeField] private float sprintMultiplier = 1.5f;

        [Header("Combat System")]
        [SerializeField] private float baseDamage = 25.0f;
        [SerializeField] private float fireRate = 0.1f; // 10 rounds per second
        [SerializeField] private int magazineSize = 30;
        [SerializeField] private float reloadTime = 2.0f;
        [SerializeField] private bool enableAimDownSights = true;
        [SerializeField] private float aimFOVMultiplier = 0.6f;

        [Header("Health & Armor System")]
        [SerializeField] private float maxHealth = 100.0f;
        [SerializeField] private float maxArmor = 100.0f;
        [SerializeField] private float healthRegenRate = 5.0f; // HP per second
        [SerializeField] private float healthRegenDelay = 5.0f; // Delay after taking damage

        [Header("15-Minute Learning Objectives")]
        [SerializeField] private List<FPSLearningObjective> learningObjectives = new List<FPSLearningObjective>();

        [Header("Weapon Configuration")]
        [SerializeField] private List<FPSWeaponSettings> availableWeapons = new List<FPSWeaponSettings>();

        [Header("Audio Settings")]
        [SerializeField] private AudioClip[] gunShotSounds;
        [SerializeField] private AudioClip[] reloadSounds;
        [SerializeField] private AudioClip[] impactSounds;
        [SerializeField] private float masterVolume = 1.0f;

        [Header("Events")]
        [SerializeField] private GameEvent onFPSGameplayStarted;
        [SerializeField] private GameEvent onWeaponFired;
        [SerializeField] private GameEvent onEnemyKilled;
        [SerializeField] private GameEvent onObjectiveCompleted;

        // Properties
        public float MouseSensitivity => mouseSensitivity;
        public float MovementSpeed => movementSpeed;
        public float BaseDamage => baseDamage;
        public float FireRate => fireRate;
        public int MagazineSize => magazineSize;
        public float ReloadTime => reloadTime;
        public float MaxHealth => maxHealth;
        public float MaxArmor => maxArmor;

        /// <summary>
        /// FPSプレイヤーコントローラーの初期設定
        /// </summary>
        public void SetupPlayerController(GameObject player)
        {
            if (player == null) return;

            // First Person Camera Setup
            var playerCamera = player.GetComponentInChildren<Camera>();
            if (playerCamera != null)
            {
                playerCamera.fieldOfView = 60f;
                playerCamera.nearClipPlane = 0.01f;
                
                // Add mouse look script (placeholder)
                if (playerCamera.GetComponent<FPSMouseLook>() == null)
                {
                    var mouseLook = playerCamera.gameObject.AddComponent<FPSMouseLook>();
                    mouseLook.sensitivity = mouseSensitivity;
                }
            }

            // Movement Controller Setup
            var characterController = player.GetComponent<CharacterController>();
            if (characterController == null)
            {
                characterController = player.AddComponent<CharacterController>();
                characterController.height = 1.8f;
                characterController.radius = 0.3f;
                characterController.stepOffset = 0.3f;
                characterController.slopeLimit = 45f;
            }

            // FPS Player Controller
            if (player.GetComponent<FPSPlayerController>() == null)
            {
                var fpsController = player.AddComponent<FPSPlayerController>();
                fpsController.Initialize(this);
            }

            // Health System
            SetupHealthSystem(player);

            Debug.Log($"[FPSTemplate] Player controller setup completed for {player.name}");
        }

        /// <summary>
        /// ヘルス・アーマーシステムの設定
        /// </summary>
        private void SetupHealthSystem(GameObject player)
        {
            if (player.GetComponent<FPSHealthSystem>() == null)
            {
                var healthSystem = player.AddComponent<FPSHealthSystem>();
                healthSystem.Initialize(maxHealth, maxArmor, healthRegenRate, healthRegenDelay);
            }
        }

        /// <summary>
        /// 武器システムの設定
        /// </summary>
        public void SetupWeaponSystem(GameObject player)
        {
            if (player == null || availableWeapons.Count == 0) return;

            // Weapon Manager
            if (player.GetComponent<FPSWeaponManager>() == null)
            {
                var weaponManager = player.AddComponent<FPSWeaponManager>();
                weaponManager.Initialize(availableWeapons, this);
            }

            // Crosshair UI Setup (placeholder)
            SetupCrosshairUI();

            Debug.Log($"[FPSTemplate] Weapon system setup completed with {availableWeapons.Count} weapons");
        }

        /// <summary>
        /// クロスヘアUIの設定
        /// </summary>
        private void SetupCrosshairUI()
        {
            // Find or create crosshair UI
            var canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                var crosshair = canvas.transform.Find("CrosshairUI");
                if (crosshair == null)
                {
                    var crosshairGO = new GameObject("CrosshairUI");
                    crosshairGO.transform.SetParent(canvas.transform);
                    
                    var rectTransform = crosshairGO.AddComponent<RectTransform>();
                    rectTransform.anchoredPosition = Vector2.zero;
                    rectTransform.sizeDelta = new Vector2(20, 20);

                    var crosshairImage = crosshairGO.AddComponent<UnityEngine.UI.Image>();
                    crosshairImage.color = Color.white;
                }
            }
        }

        /// <summary>
        /// AI敵システムの設定（FPS特化）
        /// </summary>
        public void SetupEnemySystem(GameObject[] enemies)
        {
            if (enemies == null) return;

            foreach (var enemy in enemies)
            {
                if (enemy == null) continue;

                // FPS Enemy Controller
                if (enemy.GetComponent<FPSEnemyController>() == null)
                {
                    var enemyController = enemy.AddComponent<FPSEnemyController>();
                    enemyController.Initialize(this);
                }

                // Health System for enemies
                if (enemy.GetComponent<FPSHealthSystem>() == null)
                {
                    var healthSystem = enemy.AddComponent<FPSHealthSystem>();
                    healthSystem.Initialize(maxHealth * 0.8f, 0f, 0f, 0f); // Enemies have less health, no regen
                }
            }

            Debug.Log($"[FPSTemplate] Enemy system setup completed for {enemies.Length} enemies");
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
                Debug.Log($"[FPSTemplate] Learning objective initialized: {objective.Title}");
            }
        }

        /// <summary>
        /// デフォルト学習目標の作成（15分間の段階的学習）
        /// </summary>
        private void CreateDefaultLearningObjectives()
        {
            learningObjectives.Clear();
            
            // Phase 1: Basic Movement (0-3 minutes)
            learningObjectives.Add(new FPSLearningObjective
            {
                Id = "fps_movement_basic",
                Title = "基本移動操作",
                Description = "WASD移動とマウス視点操作をマスターする",
                EstimatedTime = 180f, // 3 minutes
                Priority = 1
            });

            // Phase 2: Combat Basics (3-6 minutes)
            learningObjectives.Add(new FPSLearningObjective
            {
                Id = "fps_combat_basic",
                Title = "基本戦闘システム",
                Description = "射撃、照準、リロードの操作を習得する",
                EstimatedTime = 180f, // 3 minutes
                Priority = 2
            });

            // Phase 3: Advanced Combat (6-10 minutes)
            learningObjectives.Add(new FPSLearningObjective
            {
                Id = "fps_combat_advanced",
                Title = "高度戦闘テクニック",
                Description = "エイムダウンサイト、スプリント射撃を習得",
                EstimatedTime = 240f, // 4 minutes
                Priority = 3
            });

            // Phase 4: Tactical Gameplay (10-13 minutes)
            learningObjectives.Add(new FPSLearningObjective
            {
                Id = "fps_tactics",
                Title = "戦術的ゲームプレイ",
                Description = "ポジショニング、カバーシステムを活用",
                EstimatedTime = 180f, // 3 minutes
                Priority = 4
            });

            // Phase 5: Mission Completion (13-15 minutes)
            learningObjectives.Add(new FPSLearningObjective
            {
                Id = "fps_mission_complete",
                Title = "ミッション完遂",
                Description = "習得したスキルで実戦ミッションをクリア",
                EstimatedTime = 120f, // 2 minutes
                Priority = 5
            });

            Debug.Log($"[FPSTemplate] Created {learningObjectives.Count} default learning objectives");
        }

        /// <summary>
        /// ゲームプレイイベントの発行
        /// </summary>
        public void RaiseGameplayEvent(string eventType)
        {
            switch (eventType.ToLower())
            {
                case "started":
                    onFPSGameplayStarted?.Raise();
                    break;
                case "weapon_fired":
                    onWeaponFired?.Raise();
                    break;
                case "enemy_killed":
                    onEnemyKilled?.Raise();
                    break;
                case "objective_completed":
                    onObjectiveCompleted?.Raise();
                    break;
                default:
                    Debug.LogWarning($"[FPSTemplate] Unknown gameplay event: {eventType}");
                    break;
            }
        }

        /// <summary>
        /// FPS設定の検証
        /// </summary>
        public bool ValidateConfiguration()
        {
            bool isValid = true;

            if (mouseSensitivity <= 0f)
            {
                Debug.LogError("[FPSTemplate] Mouse sensitivity must be greater than 0");
                isValid = false;
            }

            if (movementSpeed <= 0f)
            {
                Debug.LogError("[FPSTemplate] Movement speed must be greater than 0");
                isValid = false;
            }

            if (baseDamage <= 0f)
            {
                Debug.LogError("[FPSTemplate] Base damage must be greater than 0");
                isValid = false;
            }

            if (fireRate <= 0f)
            {
                Debug.LogError("[FPSTemplate] Fire rate must be greater than 0");
                isValid = false;
            }

            if (magazineSize <= 0)
            {
                Debug.LogError("[FPSTemplate] Magazine size must be greater than 0");
                isValid = false;
            }

            if (learningObjectives.Count == 0)
            {
                Debug.LogWarning("[FPSTemplate] No learning objectives defined");
            }

            return isValid;
        }
    }

    /// <summary>
    /// FPS学習目標データ構造
    /// </summary>
    [System.Serializable]
    public class FPSLearningObjective
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
                Debug.Log($"[FPSTemplate] Learning objective completed: {Title}");
            }
        }
    }

    /// <summary>
    /// FPS武器設定データ構造
    /// </summary>
    [System.Serializable]
    public class FPSWeaponSettings
    {
        public string weaponName;
        public float damage;
        public float fireRate;
        public int magazineSize;
        public float reloadTime;
        public float range;
        public bool isAutomatic;
        public AudioClip fireSound;
        public AudioClip reloadSound;
    }
}