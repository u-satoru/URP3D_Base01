using UnityEngine;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Features.Templates.FPS.Player;
using asterivo.Unity60.Features.Templates.FPS.Weapons;
using asterivo.Unity60.Features.Templates.FPS.UI;
using asterivo.Unity60.Features.Templates.FPS.AI;
using asterivo.Unity60.Features.Templates.FPS.Camera;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Features.Templates.FPS
{
    /// <summary>
    /// FPSテンプレート統合管理システム
    /// 15分間のゲームプレイ体験を提供する完全なFPSテンプレート
    /// プレイヤー、武器、AI、UI、カメラシステムを統合管理
    /// </summary>
    public class FPSTemplateManager : MonoBehaviour
    {
        [TabGroup("FPS Template", "Core Systems")]
        [LabelText("FPS Player")]
        [SerializeField] private FPSPlayerController fpsPlayer;
        
        [LabelText("FPS Camera Controller")]
        [SerializeField] private FPSCameraController fpsCamera;
        
        [LabelText("FPS UI Manager")]
        [SerializeField] private FPSUIManager fpsUI;
        
        [TabGroup("FPS Template", "Weapon System")]
        [LabelText("Starting Weapon")]
        [SerializeField] private WeaponSystem startingWeapon;
        
        [LabelText("Available Weapons")]
        [SerializeField] private WeaponSystem[] availableWeapons;
        
        [LabelText("Weapon Spawn Points")]
        [SerializeField] private Transform[] weaponSpawnPoints;
        
        [TabGroup("FPS Template", "AI System")]
        [LabelText("Enemy AI Prefab")]
        [SerializeField] private GameObject enemyAIPrefab;
        
        [LabelText("AI Spawn Points")]
        [SerializeField] private Transform[] aiSpawnPoints;
        
        [LabelText("Max Active Enemies")]
        [PropertyRange(3, 15)]
        [SerializeField] private int maxActiveEnemies = 8;
        
        [TabGroup("FPS Template", "Game Settings")]
        [BoxGroup("FPS Template/Game Settings/Objectives")]
        [LabelText("Kill Target")]
        [PropertyRange(5, 50)]
        [SerializeField] private int killTarget = 20;
        
        [BoxGroup("FPS Template/Game Settings/Objectives")]
        [LabelText("Time Limit")]
        [PropertyRange(300f, 1800f)]
        [SuffixLabel("seconds")]
        [SerializeField] private float timeLimit = 900f; // 15分
        
        [BoxGroup("FPS Template/Game Settings/Spawning")]
        [LabelText("Enemy Spawn Interval")]
        [PropertyRange(3f, 15f)]
        [SuffixLabel("seconds")]
        [SerializeField] private float enemySpawnInterval = 8f;
        
        [BoxGroup("FPS Template/Game Settings/Spawning")]
        [LabelText("Weapon Respawn Time")]
        [PropertyRange(30f, 120f)]
        #pragma warning disable 0414 // Suppress unused field warning for Inspector configuration
        [SuffixLabel("seconds")]
        [SerializeField] private float weaponRespawnTime = 60f;
        
        [TabGroup("Events", "Game Events")]
        [LabelText("On Game Started")]
        [SerializeField] private GameEvent onGameStarted;
        
        [LabelText("On Game Ended")]
        [SerializeField] private GameEvent onGameEnded;
        
        [LabelText("On Enemy Killed")]
        [SerializeField] private GameEvent onEnemyKilled;
        
        [LabelText("On Player Death")]
        [SerializeField] private GameEvent onPlayerDeath;
        
        // Game state variables
        private bool isGameActive;
        private float gameStartTime;
        private float remainingTime;
        private int enemiesKilled;
        private int currentActiveEnemies;
        private bool isGameWon;
        
        [TabGroup("Debug", "Game Status")]
        [ReadOnly]
        [ShowInInspector]
        [LabelText("Game State")]
        private string gameState = "Not Started";
        
        [ReadOnly]
        [ShowInInspector]
        [LabelText("Kills Progress")]
        private string killsProgress = "0/20";
        
        [ReadOnly]
        [ShowInInspector]
        [LabelText("Time Remaining")]
        private string timeRemaining = "15:00";
        
        [ReadOnly]
        [ShowInInspector]
        [LabelText("Active Enemies")]
        private int debugActiveEnemies = 0;
        
        private void Start()
        {
            InitializeFPSTemplate();
            StartGame();
        }
        
        private void Update()
        {
            if (isGameActive)
            {
                UpdateGameTime();
                UpdateEnemySpawning();
                CheckWinConditions();
                UpdateDebugInfo();
            }
        }
        
        private void InitializeFPSTemplate()
        {
            // プレイヤーシステムの初期化
            InitializePlayer();
            
            // 武器システムの初期化
            InitializeWeapons();
            
            // UIシステムの初期化
            InitializeUI();
            
            // AIシステムの初期化
            InitializeAI();
            
            // イベントリスナーの設定
            SetupEventListeners();
            
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log("FPSTemplateManager: Template initialized successfully");
#endif
        }
        
        private void InitializePlayer()
        {
            if (fpsPlayer == null)
            {
                fpsPlayer = FindFirstObjectByType<FPSPlayerController>();
            }
            
            if (fpsCamera == null)
            {
                fpsCamera = FindFirstObjectByType<FPSCameraController>();
            }
            
            // プレイヤーに初期武器を装備
            if (fpsPlayer != null && startingWeapon != null)
            {
                fpsPlayer.EquipWeapon(startingWeapon);
            }
        }
        
        private void InitializeWeapons()
        {
            // 武器スポーンポイントに武器を配置
            for (int i = 0; i < weaponSpawnPoints.Length && i < availableWeapons.Length; i++)
            {
                if (weaponSpawnPoints[i] != null && availableWeapons[i] != null)
                {
                    SpawnWeapon(availableWeapons[i], weaponSpawnPoints[i].position);
                }
            }
        }
        
private void InitializeUI()
        {
            if (fpsUI == null)
            {
                fpsUI = FindFirstObjectByType<FPSUIManager>();
            }
            
            if (fpsUI != null && startingWeapon != null)
            {
                fpsUI.SetCurrentWeapon(startingWeapon);
            }
        }
        
        private void InitializeAI()
        {
            // 初期敵AIの生成
            int initialEnemies = Mathf.Min(3, maxActiveEnemies);
            for (int i = 0; i < initialEnemies; i++)
            {
                SpawnEnemy();
            }
        }
        
        private void SetupEventListeners()
        {
            // プレイヤー死亡イベント
            // TODO: 実際のイベントシステムとの連携
            
            // 敵撃破イベント
            // TODO: 実際のイベントシステムとの連携
        }
        
        private void StartGame()
        {
            isGameActive = true;
            gameStartTime = Time.time;
            remainingTime = timeLimit;
            enemiesKilled = 0;
            gameState = "Active";
            
            onGameStarted?.Raise();
            
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log("FPSTemplateManager: Game started - 15 minute FPS experience begins!");
#endif
        }
        
        private void UpdateGameTime()
        {
            remainingTime = timeLimit - (Time.time - gameStartTime);
            
            if (remainingTime <= 0)
            {
                EndGame(false); // 時間切れ
            }
        }
        
        private void UpdateEnemySpawning()
        {
            // 敵の自動生成
            if (currentActiveEnemies < maxActiveEnemies && 
                Time.time - gameStartTime > enemySpawnInterval)
            {
                if (Time.time % enemySpawnInterval < Time.deltaTime)
                {
                    SpawnEnemy();
                }
            }
        }
        
        private void CheckWinConditions()
        {
            // 勝利条件：目標キル数達成
            if (enemiesKilled >= killTarget)
            {
                EndGame(true);
            }
        }
        
        private void SpawnEnemy()
        {
            if (enemyAIPrefab == null || aiSpawnPoints.Length == 0) return;
            
            // ランダムなスポーンポイントを選択
            Transform spawnPoint = aiSpawnPoints[Random.Range(0, aiSpawnPoints.Length)];
            
            // 敵AI生成
            GameObject enemy = Instantiate(enemyAIPrefab, spawnPoint.position, spawnPoint.rotation);
            
            // FPSAIIntegrationコンポーネントを設定
            var aiIntegration = enemy.GetComponent<FPSAIIntegration>();
            if (aiIntegration != null)
            {
                // プレイヤー参照を設定
                // TODO: aiIntegration.SetPlayer(fpsPlayer);
            }
            
            currentActiveEnemies++;
            
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"FPSTemplateManager: Enemy spawned at {spawnPoint.name}. Active enemies: {currentActiveEnemies}");
#endif
        }
        
        private void SpawnWeapon(WeaponSystem weaponPrefab, Vector3 position)
        {
            if (weaponPrefab != null)
            {
                Instantiate(weaponPrefab, position, Quaternion.identity);
            }
        }
        
        public void OnEnemyKilled(GameObject enemy)
        {
            enemiesKilled++;
            currentActiveEnemies--;
            
            onEnemyKilled?.Raise();
            
            // 敵破棄
            Destroy(enemy, 2f); // 2秒後に破棄（死亡アニメーション用）
            
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"FPSTemplateManager: Enemy killed! Progress: {enemiesKilled}/{killTarget}");
#endif
        }
        
        public void OnPlayerDeath()
        {
            EndGame(false);
            onPlayerDeath?.Raise();
        }
        
        private void EndGame(bool won)
        {
            isGameActive = false;
            isGameWon = won;
            gameState = won ? "Victory" : "Defeat";
            
            onGameEnded?.Raise();
            
            // ゲーム終了処理
            Time.timeScale = 0.1f; // スローモーション効果
            
            Invoke("RestoreTimeScale", 2f);
            
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            string result = won ? "Victory" : "Defeat";
            string reason = won ? "Target eliminated" : (remainingTime <= 0 ? "Time up" : "Player defeated");
            Debug.Log($"FPSTemplateManager: Game ended - {result} ({reason})");
            Debug.Log($"Final stats: Kills: {enemiesKilled}/{killTarget}, Time: {(timeLimit - remainingTime)/60f:F1} minutes");
#endif
        }
        
        private void RestoreTimeScale()
        {
            Time.timeScale = 1f;
        }
        
        private void UpdateDebugInfo()
        {
            killsProgress = $"{enemiesKilled}/{killTarget}";
            
            int minutes = Mathf.FloorToInt(remainingTime / 60f);
            int seconds = Mathf.FloorToInt(remainingTime % 60f);
            timeRemaining = $"{minutes:D2}:{seconds:D2}";
            
            debugActiveEnemies = currentActiveEnemies;
        }
        
        // 外部呼び出し用メソッド
        public void RestartGame()
        {
            // ゲームリスタート
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
        
        public void PauseGame()
        {
            Time.timeScale = isGameActive ? 0f : 1f;
            isGameActive = !isGameActive;
        }
        
        // プロパティ
        public bool IsGameActive => isGameActive;
        public int EnemiesKilled => enemiesKilled;
        public int KillTarget => killTarget;
        public float RemainingTime => remainingTime;
        public bool IsGameWon => isGameWon;
        public int ActiveEnemies => currentActiveEnemies;
        
        // Unity Editor用のテストメソッド
        [Button("Start New Game")]
        [TabGroup("Debug", "Controls")]
        private void DebugStartGame()
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
            {
                RestartGame();
            }
#endif
        }
        
        [Button("Spawn Enemy")]
        [TabGroup("Debug", "Controls")]
        private void DebugSpawnEnemy()
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
            {
                SpawnEnemy();
            }
#endif
        }
        
        [Button("Add 5 Kills")]
        [TabGroup("Debug", "Controls")]
        private void DebugAddKills()
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
            {
                enemiesKilled += 5;
            }
#endif
        }
    }
}