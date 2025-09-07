using UnityEngine;
using Debug = UnityEngine.Debug;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Player;
using asterivo.Unity60.Core.Data;
using System.Collections;
using System.Collections.Generic;
using asterivo.Unity60.Core.Debug;
using asterivo.Unity60.Core.Commands;
using asterivo.Unity60.Core.Components;
using asterivo.Unity60.Core.Validation;

namespace asterivo.Unity60.Core.Services
{
    public class GameManager : MonoBehaviour, IGameEventListener<ICommandDefinition>
    {
        [Header("Game State")]
        [SerializeField] private GameState currentGameState = GameState.MainMenu;
        [SerializeField] private GameState previousGameState = GameState.MainMenu;

        [Header("Command System")]
        [SerializeField] private CommandDefinitionGameEvent onCommandDefinitionReceived;
        [SerializeField] private CommandInvoker commandInvoker;

        private readonly Stack<ICommand> _undoStack = new Stack<ICommand>();
        private readonly Stack<ICommand> _redoStack = new Stack<ICommand>();

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
        [SerializeField] private int maxLives = 5;

        [Header("Settings")]
        [SerializeField] private bool enableDebugLog = true;
        [SerializeField] private bool validateEventConnections = true;
        [SerializeField] private bool pauseTimeOnPause = true;
        
        [Header("Game Events")]
        [SerializeField] private IntGameEvent onScoreChanged;
        [SerializeField] private IntGameEvent onLivesChanged;
        [SerializeField] private GameEvent onGameOver;
        [SerializeField] private BoolGameEvent onPauseStateChanged;
        
        [Header("Input")]
        [SerializeField] private InputActionAsset inputActions;
        private InputAction pauseAction;

        public GameState CurrentGameState => currentGameState;
        public GameState PreviousGameState => previousGameState;
        public int CurrentScore => gameScore;
        public int CurrentLives => playerLives;
        public bool IsPaused { get; private set; }
        public bool IsGameOver => playerLives <= 0;

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

        private void Update()
        {
            if (currentGameState == GameState.Playing)
            {
                gameTime += Time.deltaTime;
            }
        }

        #region Validation & Logging
        private bool ValidateSetup()
        {
            bool isValid = true;
            
            if (validateEventConnections)
            {
                // 基本的な重要イベントの検証
                if (gameStateChangedEvent == null)
                {
                    LogError("Game State Changed Event is not assigned!");
                    isValid = false;
                }
                
                // 包括的なイベント接続検証
                var validationResult = EventConnectionValidator.ValidateAllEventConnections(true);
                
                if (validationResult.HasErrors)
                {
                    LogError($"Event connection validation failed: {validationResult.GetSummary()}");
                    isValid = false;
                }
                else if (validationResult.HasWarnings && enableDebugLog)
                {
                    LogWarning($"Event connection validation completed with warnings: {validationResult.GetSummary()}");
                }
                else if (enableDebugLog)
                {
                    Log($"Event connection validation passed: {validationResult.GetSummary()}");
                }
            }
            
            return isValid;
        }
        private void LogError(string message) { if (enableDebugLog) EventLogger.LogError($"[GameManager] {message}"); }
        private void LogWarning(string message) { if (enableDebugLog) EventLogger.LogWarning($"[GameManager] {message}"); }
        private void Log(string message) { if (enableDebugLog) EventLogger.Log($"[GameManager] {message}"); }
        #endregion

        private void InitializeGameManager()
        {
            if (gameData == null) gameData = new GameData();
            InitializeInput();
            Log("GameManager initialized");
        }
        
        private void InitializeInput()
        {
            if (inputActions != null)
            {
                pauseAction = inputActions.FindAction("Pause");
                if (pauseAction != null)
                {
                    pauseAction.performed += OnPauseInputPerformed;
                    pauseAction.Enable();
                }
            }
            else
            {
                LogWarning("InputActionAsset not assigned, using fallback input handling");
            }
        }

        #region Event Registration
        private void RegisterEventListeners()
        {
            if (startGameListener != null) startGameListener.Response.AddListener(HandleStartGame);
            if (pauseGameListener != null) pauseGameListener.Response.AddListener(HandlePauseGame);
            if (resumeGameListener != null) resumeGameListener.Response.AddListener(HandleResumeGame);
            if (restartGameListener != null) restartGameListener.Response.AddListener(HandleRestartGame);
            if (quitGameListener != null) quitGameListener.Response.AddListener(HandleQuitGame);
            if (returnToMenuListener != null) returnToMenuListener.Response.AddListener(HandleReturnToMenu);
            if (gameOverListener != null) gameOverListener.Response.AddListener(HandleGameOver);
            if (victoryListener != null) victoryListener.Response.AddListener(HandleVictory);
            if (onCommandDefinitionReceived != null) onCommandDefinitionReceived.RegisterListener(this);
        }

        private void UnregisterEventListeners()
        {
            if (startGameListener != null) startGameListener.Response.RemoveListener(HandleStartGame);
            if (pauseGameListener != null) pauseGameListener.Response.RemoveListener(HandlePauseGame);
            if (resumeGameListener != null) resumeGameListener.Response.RemoveListener(HandleResumeGame);
            if (restartGameListener != null) restartGameListener.Response.RemoveListener(HandleRestartGame);
            if (quitGameListener != null) quitGameListener.Response.RemoveListener(HandleQuitGame);
            if (returnToMenuListener != null) returnToMenuListener.Response.RemoveListener(HandleReturnToMenu);
            if (gameOverListener != null) gameOverListener.Response.RemoveListener(HandleGameOver);
            if (victoryListener != null) victoryListener.Response.RemoveListener(HandleVictory);
            if (onCommandDefinitionReceived != null) onCommandDefinitionReceived.UnregisterListener(this);
            
            // Input actions cleanup
            if (pauseAction != null)
            {
                pauseAction.performed -= OnPauseInputPerformed;
                pauseAction.Disable();
            }
        }
        #endregion

        #region Command System
        public void OnEventRaised(ICommandDefinition definition)
        {
            Log($"Command definition received: {definition.GetType().Name}");
            ICommand command = CreateCommandFromDefinition(definition);
            if (command != null) 
            {
                if (commandInvoker != null)
                {
                    commandInvoker.ExecuteCommand(command);
                }
                else
                {
                    ExecuteCommand(command);
                }
            }
        }

        private ICommand CreateCommandFromDefinition(ICommandDefinition definition)
        {
            if (commandInvoker == null)
            {
                LogError("CommandInvoker is not assigned!");
                return null;
            }

            // IHealthTargetを実装するコンポーネントを検索
            var healthTargetComponents = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            IHealthTarget playerHealth = null;
            
            foreach (var component in healthTargetComponents)
            {
                if (component is IHealthTarget healthTarget)
                {
                    playerHealth = healthTarget;
                    break;
                }
            }
            
            if (playerHealth == null)
            {
                LogError("IHealthTarget implementation not found in scene!");
                return null;
            }

            return definition.CreateCommand(playerHealth);
        }

        public void ExecuteCommand(ICommand command)
        {
            command.Execute();
            _undoStack.Push(command);
            _redoStack.Clear();
            Log($"Command Executed: {command.GetType().Name}");
        }

        public void UndoLastCommand() { /* ... */ }
        public void RedoLastCommand() { /* ... */ }
        #endregion

        #region Event Handlers
        private void HandleStartGame() { if (!isTransitioning) StartCoroutineManaged("LoadGameplay", LoadGameplayScene(gameplaySceneName)); }
        private void HandlePauseGame() { if (currentGameState == GameState.Playing) ChangeGameState(GameState.Paused); }
        private void HandleResumeGame() { if (currentGameState == GameState.Paused) ChangeGameState(GameState.Playing); }
        private void HandleRestartGame() { if (!isTransitioning) StartCoroutineManaged("LoadGameplay", LoadGameplayScene(gameplaySceneName)); }
        private void HandleQuitGame() { Application.Quit(); }
        private void HandleReturnToMenu() { if (!isTransitioning) StartCoroutineManaged("LoadMenu", LoadGameplayScene(mainMenuSceneName)); }
        private void HandleGameOver() { if (currentGameState == GameState.Playing) ChangeGameState(GameState.GameOver); }
        private void HandleVictory() { if (currentGameState == GameState.Playing) ChangeGameState(GameState.Victory); }
        #endregion

        #region Game State Management
        private void ChangeGameState(GameState newState)
        {
            if (currentGameState == newState) return;
            previousGameState = currentGameState;
            currentGameState = newState;
            gameStateChangedEvent?.Raise(currentGameState);
            Log($"Game state changed to: {newState}");
        }
        #endregion

        #region Scene Loading
        private IEnumerator LoadGameplayScene(string sceneName)
        {
            isTransitioning = true;
            ChangeGameState(GameState.Loading);
            yield return SceneManager.LoadSceneAsync(sceneName);
            isTransitioning = false;
            ChangeGameState(GameState.Playing);
        }
        #endregion

        private void OnPauseInputPerformed(InputAction.CallbackContext context)
        {
            if (currentGameState == GameState.Playing || currentGameState == GameState.Paused)
            {
                onPauseGameCommand?.Raise();
            }
        }

        #region Score and Lives System
        /// <summary>
        /// スコアを追加
        /// </summary>
        public void AddScore(int points)
        {
            if (points <= 0) return;
            
            gameScore += points;
            onScoreChanged?.Raise(gameScore);
            Log($"Score increased by {points}. Total score: {gameScore}");
        }

        /// <summary>
        /// スコアを設定
        /// </summary>
        public void SetScore(int newScore)
        {
            if (newScore < 0) newScore = 0;
            
            gameScore = newScore;
            onScoreChanged?.Raise(gameScore);
            Log($"Score set to: {gameScore}");
        }

        /// <summary>
        /// ライフを失う
        /// </summary>
        public void LoseLife()
        {
            if (playerLives <= 0) return;

            playerLives--;
            onLivesChanged?.Raise(playerLives);
            Log($"Life lost. Remaining lives: {playerLives}");

            if (playerLives <= 0)
            {
                TriggerGameOver();
            }
        }

        /// <summary>
        /// ライフを追加
        /// </summary>
        public void AddLife()
        {
            if (playerLives >= maxLives) return;

            playerLives++;
            onLivesChanged?.Raise(playerLives);
            Log($"Life gained. Current lives: {playerLives}");
        }

        /// <summary>
        /// ライフを設定
        /// </summary>
        public void SetLives(int lives)
        {
            lives = Mathf.Clamp(lives, 0, maxLives);
            playerLives = lives;
            onLivesChanged?.Raise(playerLives);
            Log($"Lives set to: {playerLives}");

            if (playerLives <= 0)
            {
                TriggerGameOver();
            }
        }

        /// <summary>
        /// ゲームオーバーをトリガー
        /// </summary>
        private void TriggerGameOver()
        {
            Log("Game Over triggered!");
            ChangeGameState(GameState.GameOver);
            onGameOver?.Raise();
        }
        #endregion

        #region Pause System
        /// <summary>
        /// ポーズ状態を切り替え
        /// </summary>
        public void TogglePause()
        {
            SetPauseState(!IsPaused);
        }

        /// <summary>
        /// ポーズ状態を設定
        /// </summary>
        public void SetPauseState(bool paused)
        {
            if (IsPaused == paused) return;

            IsPaused = paused;

            if (pauseTimeOnPause)
            {
                Time.timeScale = paused ? 0f : 1f;
            }

            ChangeGameState(paused ? GameState.Paused : GameState.Playing);
            onPauseStateChanged?.Raise(IsPaused);
            Log($"Game {(paused ? "paused" : "unpaused")}");
        }

        /// <summary>
        /// ゲームを再開
        /// </summary>
        public void ResumeGame()
        {
            SetPauseState(false);
        }
        #endregion

        #region Loading System  
        /// <summary>
        /// 最小ロード時間を保証したシーンロード
        /// </summary>
        private IEnumerator LoadSceneWithMinTime(string sceneName)
        {
            isTransitioning = true;
            ChangeGameState(GameState.Loading);
            
            float startTime = Time.realtimeSinceStartup;
            var asyncOperation = SceneManager.LoadSceneAsync(sceneName);
            
            // 最小ロード時間を待機
            while (Time.realtimeSinceStartup - startTime < minLoadingTime || !asyncOperation.isDone)
            {
                yield return null;
            }
            
            isTransitioning = false;
            ChangeGameState(GameState.Playing);
            Log($"Scene '{sceneName}' loaded with minimum load time guarantee");
        }
        
        /// <summary>
        /// 最小ロード時間保証付きでゲームプレイシーンをロード
        /// </summary>
        public void LoadGameplaySceneWithMinTime()
        {
            StartCoroutineManaged("LoadGameplayScene", LoadSceneWithMinTime(gameplaySceneName));
        }
        #endregion

        #region Coroutine Management
        private void StartCoroutineManaged(string name, IEnumerator routine)
        {
            StopCoroutineManaged(name);
            activeCoroutines[name] = StartCoroutine(routine);
        }

        private void StopCoroutineManaged(string name)
        {
            if (activeCoroutines.TryGetValue(name, out Coroutine coroutine) && coroutine != null)
            {
                StopCoroutine(coroutine);
                activeCoroutines.Remove(name);
            }
        }

        private void StopAllActiveCoroutines()
        {
            foreach (var coroutine in activeCoroutines.Values) if (coroutine != null) StopCoroutine(coroutine);
            activeCoroutines.Clear();
        }
        #endregion
    }
}
