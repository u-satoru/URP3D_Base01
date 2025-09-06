using UnityEngine;
using Debug = UnityEngine.Debug;
using UnityEngine.SceneManagement;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Player;
using asterivo.Unity60.Core.Data;
using System.Collections;
using System.Collections.Generic;
using asterivo.Unity60.Core.Commands;
// using asterivo.Unity60.Player.Commands; // asmdef参照エラーのため一時的にコメントアウト
// using asterivo.Unity60.Player.States; // asmdef参照エラーのため一時的にコメントアウト

namespace asterivo.Unity60.Systems
{
    public class GameManager : MonoBehaviour, IGameEventListener<ICommandDefinition>
    {
        [Header("Game State")]
        [SerializeField] private GameState currentGameState = GameState.MainMenu;
        [SerializeField] private GameState previousGameState = GameState.MainMenu;

        [Header("Command System")]
        [SerializeField] private CommandDefinitionGameEvent onCommandDefinitionReceived;
        // [SerializeField] private PlayerStateMachine playerStateMachine; // asmdef参照エラーのため一時的にコメントアウト

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

        [Header("Settings")]
        [SerializeField] private bool enableDebugLog = true;
        [SerializeField] private bool validateEventConnections = true;
        [SerializeField] private bool pauseTimeOnPause = true;

        public GameState CurrentGameState => currentGameState;
        public GameState PreviousGameState => previousGameState;

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
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                HandlePauseInput();
            }
        }

        #region Validation & Logging
        private bool ValidateSetup()
        {
            bool isValid = true;
            if (validateEventConnections && gameStateChangedEvent == null)
            {
                LogError("Game State Changed Event is not assigned!");
                isValid = false;
            }
            return isValid;
        }
        private void LogError(string message) { if (enableDebugLog) Debug.LogError($"[GameManager] {message}", this); }
        private void LogWarning(string message) { if (enableDebugLog) Debug.LogWarning($"[GameManager] {message}", this); }
        private void Log(string message) { if (enableDebugLog) Debug.Log($"[GameManager] {message}", this); }
        #endregion

        private void InitializeGameManager()
        {
            if (gameData == null) gameData = new GameData();
            Log("GameManager initialized");
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
        }
        #endregion

        #region Command System
        public void OnEventRaised(ICommandDefinition definition)
        {
            Log($"Command definition received: {definition.GetType().Name}");
            // ICommand command = CreateCommandFromDefinition(definition);
            // if (command != null) ExecuteCommand(command);
        }

        private ICommand CreateCommandFromDefinition(ICommandDefinition definition)
        {
            // NOTE: This logic is disabled until the assembly reference issue is resolved.
            return null;
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

        private void HandlePauseInput() { if (currentGameState == GameState.Playing || currentGameState == GameState.Paused) onPauseGameCommand?.Raise(); }

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
