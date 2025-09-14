using UnityEngine;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Features.Templates.TPS.Player;
using asterivo.Unity60.Features.Templates.TPS.UI;
using asterivo.Unity60.Features.Templates.TPS.AI;
using asterivo.Unity60.Features.Templates.TPS.Camera;
using asterivo.Unity60.Features.Templates.FPS.Weapons; // Reuse FPS weapons
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Features.Templates.TPS
{
    /// <summary>
    /// TPSテンプレート統合管理システム
    /// 15分間の三人称視点シューター体験を提供する完全なTPSテンプレート
    /// プレイヤー、武器、AI、UI、カメラシステムを統合管理
    /// カバーシステムと三人称視点に特化した機能を実装
    /// </summary>
    public class TPSTemplateManager : MonoBehaviour
    {
        [TabGroup("TPS Template", "Core Systems")]
        [LabelText("TPS Player")]
        [SerializeField] private TPSPlayerController tpsPlayer;

        [LabelText("TPS Camera Controller")]
        [SerializeField] private TPSCameraController tpsCamera;

        [LabelText("TPS UI Manager")]
        [SerializeField] private TPSUIManager tpsUI;

        [TabGroup("TPS Template", "Weapon System")]
        [LabelText("Starting Weapon")]
        [SerializeField] private WeaponSystem startingWeapon;

        [LabelText("Available Weapons")]
        [SerializeField] private WeaponSystem[] availableWeapons;

        [LabelText("Weapon Spawn Points")]
        [SerializeField] private Transform[] weaponSpawnPoints;

        [TabGroup("TPS Template", "Cover System")]
        [LabelText("Cover Points")]
        [SerializeField] private Transform[] coverPoints;

        [LabelText("Auto Cover Detection")]
        [SerializeField] private bool autoCoverDetection = true;

        [LabelText("Cover Detection Range")]
        [PropertyRange(0.5f, 3f)]
        [SerializeField] private float coverDetectionRange = 1.5f;

        [TabGroup("TPS Template", "AI System")]
        [LabelText("Enemy AI Prefab")]
        [SerializeField] private GameObject enemyAIPrefab;

        [LabelText("AI Spawn Points")]
        [SerializeField] private Transform[] aiSpawnPoints;

        [LabelText("Max Active Enemies")]
        [PropertyRange(3, 15)]
        [SerializeField] private int maxActiveEnemies = 8;

        [TabGroup("TPS Template", "Game Settings")]
        [BoxGroup("TPS Template/Game Settings/Objectives")]
        [LabelText("Kill Target")]
        [PropertyRange(5, 50)]
        [SerializeField] private int killTarget = 20;

        [BoxGroup("TPS Template/Game Settings/Objectives")]
        [LabelText("Time Limit")]
        [PropertyRange(300f, 1800f)]
        [SuffixLabel("seconds")]
        [SerializeField] private float timeLimit = 900f; // 15分

        [BoxGroup("TPS Template/Game Settings/Spawning")]
        [LabelText("Enemy Spawn Interval")]
        [PropertyRange(3f, 15f)]
        [SuffixLabel("seconds")]
        [SerializeField] private float enemySpawnInterval = 8f;

        [BoxGroup("TPS Template/Game Settings/Spawning")]
        [LabelText("Weapon Respawn Time")]
        [PropertyRange(30f, 120f)]
        [SuffixLabel("seconds")]
        [SerializeField] private float weaponRespawnTime = 60f;

        [TabGroup("Events", "Game Events")]
        [LabelText("On Game Started")]
        [SerializeField] private GameEvent onGameStarted;

        [LabelText("On Game Ended")]
        [SerializeField] private GameEvent onGameEnded;

        [LabelText("On Player Take Cover")]
        [SerializeField] private GameEvent onPlayerTakeCover;

        [LabelText("On Player Leave Cover")]
        [SerializeField] private GameEvent onPlayerLeaveCover;

        [TabGroup("Events", "Score Events")]
        [LabelText("On Enemy Killed")]
        [SerializeField] private GameEvent onEnemyKilled;

        [LabelText("On Player Damaged")]
        [SerializeField] private GameEvent onPlayerDamaged;

        [LabelText("On Objective Complete")]
        [SerializeField] private GameEvent onObjectiveComplete;

        // Private game state
        private int currentKills = 0;
        private float gameTimer = 0f;
        private bool gameActive = false;
        private int activeEnemies = 0;
        private float lastSpawnTime = 0f;

        [TabGroup("Debug", "Game Status")]
        [ReadOnly]
        [ShowInInspector]
        [LabelText("Current Kills")]
        private int debugCurrentKills => currentKills;

        [ReadOnly]
        [ShowInInspector]
        [LabelText("Time Remaining")]
        private float debugTimeRemaining => Mathf.Max(0, timeLimit - gameTimer);

        [ReadOnly]
        [ShowInInspector]
        [LabelText("Active Enemies")]
        private int debugActiveEnemies => activeEnemies;

        [ReadOnly]
        [ShowInInspector]
        [LabelText("Game Active")]
        private bool debugGameActive => gameActive;

        private void Start()
        {
            InitializeTPS();
        }

        private void Update()
        {
            if (gameActive)
            {
                UpdateGameTimer();
                UpdateEnemySpawning();
                CheckWinConditions();
                CheckLoseConditions();
            }
        }

        private void InitializeTPS()
        {
            // Initialize player
            if (tpsPlayer != null)
            {
                tpsPlayer.Initialize(this);
                if (startingWeapon != null)
                {
                    tpsPlayer.EquipWeapon(startingWeapon);
                }
            }

            // Initialize camera
            if (tpsCamera != null)
            {
                tpsCamera.Initialize(tpsPlayer != null ? tpsPlayer.transform : transform);
            }

            // Initialize UI
            if (tpsUI != null)
            {
                tpsUI.Initialize(this);
            }

            // Setup event listeners
            SetupEventListeners();

            // Start game
            StartGame();
        }

        private void SetupEventListeners()
        {
            // TODO: Setup event listeners for TPS-specific events
            // This will be implemented when individual components are created
        }

        [Button("Start Game")]
        public void StartGame()
        {
            gameActive = true;
            gameTimer = 0f;
            currentKills = 0;
            activeEnemies = 0;

            onGameStarted?.Raise();

            UnityEngine.Debug.Log("[TPS] Game Started - Kill " + killTarget + " enemies in " + timeLimit + " seconds!");
        }

        [Button("End Game")]
        public void EndGame()
        {
            gameActive = false;
            onGameEnded?.Raise();

            UnityEngine.Debug.Log("[TPS] Game Ended");
        }

        private void UpdateGameTimer()
        {
            gameTimer += Time.deltaTime;
        }

        private void UpdateEnemySpawning()
        {
            if (activeEnemies < maxActiveEnemies && Time.time - lastSpawnTime >= enemySpawnInterval)
            {
                SpawnEnemy();
                lastSpawnTime = Time.time;
            }
        }

        private void SpawnEnemy()
        {
            if (aiSpawnPoints.Length > 0 && enemyAIPrefab != null)
            {
                Transform spawnPoint = aiSpawnPoints[Random.Range(0, aiSpawnPoints.Length)];
                GameObject enemy = Instantiate(enemyAIPrefab, spawnPoint.position, spawnPoint.rotation);

                // TODO: Setup enemy AI integration
                activeEnemies++;

                UnityEngine.Debug.Log("[TPS] Enemy spawned at " + spawnPoint.name);
            }
        }

        private void CheckWinConditions()
        {
            if (currentKills >= killTarget)
            {
                WinGame();
            }
        }

        private void CheckLoseConditions()
        {
            if (gameTimer >= timeLimit)
            {
                LoseGame();
            }
        }

        private void WinGame()
        {
            gameActive = false;
            onObjectiveComplete?.Raise();
            onGameEnded?.Raise();

            UnityEngine.Debug.Log("[TPS] Victory! Killed " + currentKills + " enemies in " + gameTimer.ToString("F1") + " seconds!");
        }

        private void LoseGame()
        {
            gameActive = false;
            onGameEnded?.Raise();

            UnityEngine.Debug.Log("[TPS] Time's up! Killed " + currentKills + "/" + killTarget + " enemies.");
        }

        public void OnEnemyKilledByPlayer()
        {
            currentKills++;
            activeEnemies--;
            onEnemyKilled?.Raise();

            UnityEngine.Debug.Log("[TPS] Enemy killed! Progress: " + currentKills + "/" + killTarget);
        }

        public void OnPlayerTakeCoverAction()
        {
            onPlayerTakeCover?.Raise();
            UnityEngine.Debug.Log("[TPS] Player took cover");
        }

        public void OnPlayerLeaveCoverAction()
        {
            onPlayerLeaveCover?.Raise();
            UnityEngine.Debug.Log("[TPS] Player left cover");
        }

        public void OnPlayerDamagedAction()
        {
            onPlayerDamaged?.Raise();
            UnityEngine.Debug.Log("[TPS] Player damaged");
        }

        // Public getters for other components
        public int CurrentKills => currentKills;
        public float TimeRemaining => Mathf.Max(0, timeLimit - gameTimer);
        public int KillTarget => killTarget;
        public bool IsGameActive => gameActive;
        public Transform[] CoverPoints => coverPoints;
        public float CoverDetectionRange => coverDetectionRange;
        public bool AutoCoverDetection => autoCoverDetection;
    }
}