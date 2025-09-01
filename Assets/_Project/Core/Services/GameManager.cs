using UnityEngine;
using UnityEngine.SceneManagement;
using Unity6.Core.Events;
using Unity6.Core.Player;
using Unity6.Core.Data;
using System.Collections;
using System.Collections.Generic;

namespace Unity6.Systems
{
    /// <summary>
    /// ゲームマネージャー
    /// 完全イベント駆動・エラーハンドリング強化
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [Header("Game State")]
        [SerializeField] private GameState currentGameState = GameState.MainMenu;
        [SerializeField] private GameState previousGameState = GameState.MainMenu;
        
        [Header("Event Channels - Input")]
        [SerializeField] private GameEvent onStartGameCommand;
        [SerializeField] private GameEvent onPauseGameCommand;
        [SerializeField] private GameEvent onResumeGameCommand;
        [SerializeField] private GameEvent onRestartGameCommand;
        [SerializeField] private GameEvent onQuitGameCommand;
        [SerializeField] private GameEvent onReturnToMenuCommand;
        [SerializeField] private GameEvent onTriggerGameOverCommand;
        [SerializeField] private GameEvent onTriggerVictoryCommand;
        
        [Header("Event Channels - Output")]
        [SerializeField] private GameStateEvent gameStateChangedEvent;
        [SerializeField] private GameDataEvent gameDataUpdatedEvent;
        [SerializeField] private PlayerDataEvent playerDataUpdatedEvent;
        [SerializeField] private FloatGameEvent loadingProgressEvent;
        [SerializeField] private StringGameEvent errorMessageEvent;
        
        [Header("Event Listeners")]
        [SerializeField] private GameEventListener startGameListener;
        [SerializeField] private GameEventListener pauseGameListener;
        [SerializeField] private GameEventListener resumeGameListener;
        [SerializeField] private GameEventListener restartGameListener;
        [SerializeField] private GameEventListener quitGameListener;
        [SerializeField] private GameEventListener returnToMenuListener;
        [SerializeField] private GameEventListener gameOverListener;
        [SerializeField] private GameEventListener victoryListener;
        
        [Header("Scene Management")]
        [SerializeField] private string mainMenuSceneName = "MainMenu";
        [SerializeField] private string gameplaySceneName = "Gameplay";
        [SerializeField] private float minLoadingTime = 1f;
        
        [Header("Game Data")]
        [SerializeField] private GameData gameData;
        [SerializeField] private float gameTime = 0f;
        [SerializeField] private int gameScore = 0;
        [SerializeField] private int playerLives = 3;
        
        [Header("Settings")]
        [SerializeField] private bool enableDebugLog = true;
        [SerializeField] private bool validateEventConnections = true;
        [SerializeField] private bool pauseTimeOnPause = true;
        
        // プロパティ
        public GameState CurrentGameState => currentGameState;
        public GameState PreviousGameState => previousGameState;
        public float GameTime => gameTime;
        public int GameScore => gameScore;
        public int PlayerLives => playerLives;
        public GameData GameData => gameData;
        
        // イベント
        public event System.Action<GameState, GameState> OnGameStateChanged;
        public event System.Action<float> OnGameTimeUpdated;
        public event System.Action<int> OnScoreUpdated;
        public event System.Action<int> OnLivesUpdated;
        
        // コルーチン管理
        private Dictionary<string, Coroutine> activeCoroutines = new Dictionary<string, Coroutine>();
        private bool isTransitioning = false;
        
        private void Awake()
        {
            if (!ValidateSetup())
            {
                enabled = false;
                return;
            }
            
            InitializeGameManager();
        }
        
        private void OnEnable()
        {
            RegisterEventListeners();
        }
        
        private void OnDisable()
        {
            UnregisterEventListeners();
            StopAllActiveCoroutines();
        }
        
        private void Start()
        {
            // 初期化完了後の処理
            if (enableDebugLog)
                Debug.Log($"GameManager started with state: {currentGameState}");
        }
        
        private void Update()
        {
            // ゲーム時間の更新（Playing状態の時のみ）
            if (currentGameState == GameState.Playing)
            {
                UpdateGameTime();
            }
            
            // ESCキーでポーズ
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                HandlePauseInput();
            }
        }
        
        private void OnDestroy()
        {
            StopAllActiveCoroutines();
        }
        
        #region Validation
        private bool ValidateSetup()
        {
            if (!validateEventConnections)
                return true;
            
            bool isValid = true;
            
            // 必須イベントチャネルの確認
            if (gameStateChangedEvent == null)
            {
                LogError("Game State Changed Event is not assigned!");
                isValid = false;
            }
            
            // リスナーの確認
            int connectedListeners = 0;
            if (startGameListener != null) connectedListeners++;
            if (pauseGameListener != null) connectedListeners++;
            if (resumeGameListener != null) connectedListeners++;
            
            if (connectedListeners == 0)
            {
                LogWarning("No event listeners are connected. GameManager will have limited functionality.");
            }
            
            return isValid;
        }
        
        private void LogError(string message)
        {
            if (enableDebugLog)
            {
                Debug.LogError($"[GameManager] {message}", this);
                errorMessageEvent?.Raise($"Error: {message}");
            }
        }
        
        private void LogWarning(string message)
        {
            if (enableDebugLog)
            {
                Debug.LogWarning($"[GameManager] {message}", this);
            }
        }
        
        private void Log(string message)
        {
            if (enableDebugLog)
            {
                Debug.Log($"[GameManager] {message}", this);
            }
        }
        #endregion
        
        #region Initialization
        private void InitializeGameManager()
        {
            // GameDataの初期化
            if (gameData == null)
            {
                gameData = new GameData
                {
                    gameTime = 0f,
                    score = 0,
                    lives = playerLives,
                    level = 1,
                    playerName = "Player",
                    highScore = PlayerPrefs.GetInt("HighScore", 0)
                };
            }
            
            Log("GameManager initialized");
        }
        #endregion
        
        #region Event Registration
        private void RegisterEventListeners()
        {
            // 静的リスナーにハンドラーを登録
            if (startGameListener != null)
                startGameListener.Response.AddListener(HandleStartGame);
            
            if (pauseGameListener != null)
                pauseGameListener.Response.AddListener(HandlePauseGame);
            
            if (resumeGameListener != null)
                resumeGameListener.Response.AddListener(HandleResumeGame);
            
            if (restartGameListener != null)
                restartGameListener.Response.AddListener(HandleRestartGame);
            
            if (quitGameListener != null)
                quitGameListener.Response.AddListener(HandleQuitGame);
            
            if (returnToMenuListener != null)
                returnToMenuListener.Response.AddListener(HandleReturnToMenu);
            
            if (gameOverListener != null)
                gameOverListener.Response.AddListener(HandleGameOver);
            
            if (victoryListener != null)
                victoryListener.Response.AddListener(HandleVictory);
        }
        
        private void UnregisterEventListeners()
        {
            // リスナーからハンドラーを削除
            if (startGameListener != null)
                startGameListener.Response.RemoveListener(HandleStartGame);
            
            if (pauseGameListener != null)
                pauseGameListener.Response.RemoveListener(HandlePauseGame);
            
            if (resumeGameListener != null)
                resumeGameListener.Response.RemoveListener(HandleResumeGame);
            
            if (restartGameListener != null)
                restartGameListener.Response.RemoveListener(HandleRestartGame);
            
            if (quitGameListener != null)
                quitGameListener.Response.RemoveListener(HandleQuitGame);
            
            if (returnToMenuListener != null)
                returnToMenuListener.Response.RemoveListener(HandleReturnToMenu);
            
            if (gameOverListener != null)
                gameOverListener.Response.RemoveListener(HandleGameOver);
            
            if (victoryListener != null)
                victoryListener.Response.RemoveListener(HandleVictory);
        }
        #endregion
        
        #region Event Handlers
        private void HandleStartGame()
        {
            if (!isTransitioning)
            {
                StartGame();
            }
        }
        
        private void HandlePauseGame()
        {
            if (currentGameState == GameState.Playing)
            {
                PauseGame();
            }
        }
        
        private void HandleResumeGame()
        {
            if (currentGameState == GameState.Paused)
            {
                ResumeGame();
            }
        }
        
        private void HandleRestartGame()
        {
            if (!isTransitioning)
            {
                RestartGame();
            }
        }
        
        private void HandleQuitGame()
        {
            QuitGame();
        }
        
        private void HandleReturnToMenu()
        {
            if (!isTransitioning)
            {
                ReturnToMenu();
            }
        }
        
        private void HandleGameOver()
        {
            if (currentGameState == GameState.Playing)
            {
                EndGame(false);
            }
        }
        
        private void HandleVictory()
        {
            if (currentGameState == GameState.Playing)
            {
                EndGame(true);
            }
        }
        #endregion
        
        #region Game State Management
        private void ChangeGameState(GameState newState)
        {
            if (currentGameState == newState)
                return;
            
            GameState oldState = currentGameState;
            
            // 現在の状態を終了
            ExitGameState(currentGameState);
            
            // 状態を更新
            previousGameState = currentGameState;
            currentGameState = newState;
            
            // 新しい状態を開始
            EnterGameState(currentGameState);
            
            // イベント通知
            OnGameStateChanged?.Invoke(oldState, currentGameState);
            
            // グローバルイベント通知
            gameStateChangedEvent?.Raise(currentGameState);
            
            Log($"Game state changed: {oldState} -> {currentGameState}");
        }
        
        private void EnterGameState(GameState state)
        {
            switch (state)
            {
                case GameState.MainMenu:
                    OnEnterMainMenu();
                    break;
                case GameState.Playing:
                    OnEnterPlaying();
                    break;
                case GameState.Paused:
                    OnEnterPaused();
                    break;
                case GameState.GameOver:
                    OnEnterGameOver();
                    break;
                case GameState.Victory:
                    OnEnterVictory();
                    break;
                case GameState.Loading:
                    OnEnterLoading();
                    break;
            }
        }
        
        private void ExitGameState(GameState state)
        {
            switch (state)
            {
                case GameState.MainMenu:
                    OnExitMainMenu();
                    break;
                case GameState.Playing:
                    OnExitPlaying();
                    break;
                case GameState.Paused:
                    OnExitPaused();
                    break;
                case GameState.GameOver:
                    OnExitGameOver();
                    break;
                case GameState.Victory:
                    OnExitVictory();
                    break;
                case GameState.Loading:
                    OnExitLoading();
                    break;
            }
        }
        
        #region State Enter Methods
        private void OnEnterMainMenu()
        {
            Log("Entering MainMenu state");
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        
        private void OnEnterPlaying()
        {
            Log("Entering Playing state");
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        
        private void OnEnterPaused()
        {
            Log("Entering Paused state");
            if (pauseTimeOnPause)
                Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        
        private void OnEnterGameOver()
        {
            Log("Entering GameOver state");
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            // ハイスコア更新
            if (gameScore > gameData.highScore)
            {
                gameData.highScore = gameScore;
                PlayerPrefs.SetInt("HighScore", gameData.highScore);
                PlayerPrefs.Save();
            }
        }
        
        private void OnEnterVictory()
        {
            Log("Entering Victory state");
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            // ハイスコア更新
            if (gameScore > gameData.highScore)
            {
                gameData.highScore = gameScore;
                PlayerPrefs.SetInt("HighScore", gameData.highScore);
                PlayerPrefs.Save();
            }
        }
        
        private void OnEnterLoading()
        {
            Log("Entering Loading state");
        }
        #endregion
        
        #region State Exit Methods
        private void OnExitMainMenu()
        {
            Log("Exiting MainMenu state");
        }
        
        private void OnExitPlaying()
        {
            Log("Exiting Playing state");
        }
        
        private void OnExitPaused()
        {
            Log("Exiting Paused state");
            Time.timeScale = 1f;
        }
        
        private void OnExitGameOver()
        {
            Log("Exiting GameOver state");
            Time.timeScale = 1f;
        }
        
        private void OnExitVictory()
        {
            Log("Exiting Victory state");
            Time.timeScale = 1f;
        }
        
        private void OnExitLoading()
        {
            Log("Exiting Loading state");
        }
        #endregion
        #endregion
        
        #region Game Control
        public void StartGame()
        {
            ResetGameData();
            StartCoroutineManaged("LoadGameplay", LoadGameplayScene());
        }
        
        public void PauseGame()
        {
            if (currentGameState == GameState.Playing)
            {
                ChangeGameState(GameState.Paused);
            }
        }
        
        public void ResumeGame()
        {
            if (currentGameState == GameState.Paused)
            {
                ChangeGameState(GameState.Playing);
            }
        }
        
        public void RestartGame()
        {
            ResetGameData();
            StartCoroutineManaged("LoadGameplay", LoadGameplayScene());
        }
        
        public void EndGame(bool victory)
        {
            ChangeGameState(victory ? GameState.Victory : GameState.GameOver);
        }
        
        public void ReturnToMenu()
        {
            StartCoroutineManaged("LoadMenu", LoadMainMenuScene());
        }
        
        public void QuitGame()
        {
            Log("Quitting game...");
            
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        #endregion
        
        #region Scene Loading
        private IEnumerator LoadGameplayScene()
        {
            isTransitioning = true;
            ChangeGameState(GameState.Loading);
            
            float startTime = Time.time;
            
            // シーンの非同期ロード
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(gameplaySceneName);
            if (asyncLoad == null)
            {
                LogError($"Failed to load scene: {gameplaySceneName}");
                isTransitioning = false;
                ChangeGameState(GameState.MainMenu);
                yield break;
            }
            
            while (!asyncLoad.isDone)
            {
                float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
                loadingProgressEvent?.Raise(progress);
                yield return null;
            }
            
            // 最小ロード時間の確保
            float elapsedTime = Time.time - startTime;
            if (elapsedTime < minLoadingTime)
            {
                yield return new WaitForSeconds(minLoadingTime - elapsedTime);
            }
            
            loadingProgressEvent?.Raise(1f);
            isTransitioning = false;
            ChangeGameState(GameState.Playing);
        }
        
        private IEnumerator LoadMainMenuScene()
        {
            isTransitioning = true;
            ChangeGameState(GameState.Loading);
            
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(mainMenuSceneName);
            if (asyncLoad == null)
            {
                LogError($"Failed to load scene: {mainMenuSceneName}");
                isTransitioning = false;
                yield break;
            }
            
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
            
            isTransitioning = false;
            ChangeGameState(GameState.MainMenu);
        }
        #endregion
        
        #region Game Data Management
        private void UpdateGameTime()
        {
            gameTime += Time.deltaTime;
            gameData.gameTime = gameTime;
            OnGameTimeUpdated?.Invoke(gameTime);
        }
        
        public void AddScore(int points)
        {
            gameScore += points;
            gameData.score = gameScore;
            OnScoreUpdated?.Invoke(gameScore);
            
            // イベント通知
            gameDataUpdatedEvent?.Raise(gameData);
            
            Log($"Score updated: +{points} (Total: {gameScore})");
        }
        
        public void LoseLife()
        {
            if (playerLives > 0)
            {
                playerLives--;
                gameData.lives = playerLives;
                OnLivesUpdated?.Invoke(playerLives);
                
                // イベント通知
                gameDataUpdatedEvent?.Raise(gameData);
                
                Log($"Life lost. Remaining lives: {playerLives}");
                
                // ライフが0になったらゲームオーバー
                if (playerLives <= 0)
                {
                    EndGame(false);
                }
            }
        }
        
        public void GainLife()
        {
            playerLives++;
            gameData.lives = playerLives;
            OnLivesUpdated?.Invoke(playerLives);
            
            // イベント通知
            gameDataUpdatedEvent?.Raise(gameData);
            
            Log($"Life gained. Total lives: {playerLives}");
        }
        
        public void ResetGameData()
        {
            gameTime = 0f;
            gameScore = 0;
            playerLives = 3;
            
            gameData.gameTime = gameTime;
            gameData.score = gameScore;
            gameData.lives = playerLives;
            gameData.level = 1;
            
            gameDataUpdatedEvent?.Raise(gameData);
            
            Log("Game data reset");
        }
        
        public void UpdatePlayerData(PlayerDataPayload playerData)
        {
            if (playerData != null)
            {
                playerDataUpdatedEvent?.Raise(playerData);
                Log($"Player data updated: {playerData.playerName}");
            }
        }
        #endregion
        
        #region Input Handling
        private void HandlePauseInput()
        {
            if (currentGameState == GameState.Playing)
            {
                PauseGame();
            }
            else if (currentGameState == GameState.Paused)
            {
                ResumeGame();
            }
        }
        #endregion
        
        #region Coroutine Management
        private void StartCoroutineManaged(string name, IEnumerator routine)
        {
            StopCoroutineManaged(name);
            var coroutine = StartCoroutine(routine);
            activeCoroutines[name] = coroutine;
        }
        
        private void StopCoroutineManaged(string name)
        {
            if (activeCoroutines.TryGetValue(name, out var coroutine))
            {
                if (coroutine != null)
                    StopCoroutine(coroutine);
                activeCoroutines.Remove(name);
            }
        }
        
        private void StopAllActiveCoroutines()
        {
            foreach (var coroutine in activeCoroutines.Values)
            {
                if (coroutine != null)
                    StopCoroutine(coroutine);
            }
            activeCoroutines.Clear();
        }
        #endregion
        
        #region Utility
        public bool IsInGameState(GameState state)
        {
            return currentGameState == state;
        }
        
        public bool IsGameActive()
        {
            return currentGameState == GameState.Playing && !isTransitioning;
        }
        #endregion
    }
}