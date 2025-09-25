using UnityEngine;
using Debug = UnityEngine.Debug;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Features.Player;
using asterivo.Unity60.Core.Data;
using System.Collections;
using System.Collections.Generic;
using asterivo.Unity60.Core.Commands;
using asterivo.Unity60.Core.Components;
using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Core.Helpers;
using asterivo.Unity60.Core.Audio; // GameState enum用

namespace asterivo.Unity60.Features.GameManagement
{
    public class GameManager : MonoBehaviour, IGameEventListener<object>
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
        

        [Header("Game Data")]
        [SerializeField] private asterivo.Unity60.Core.Data.GameData gameData;
        [SerializeField] private float gameTime = 0f;

        [Header("Settings")]
        [SerializeField] private bool enableDebugLog = true;
        [SerializeField] private bool validateEventConnections = true;
        
        // Game events are now owned by services (Score/Pause/State)
        
        [Header("Input")]
        [SerializeField] private InputActionAsset inputActions;
        private InputAction pauseAction;

        public GameState CurrentGameState
        {
            get
            {
                var gsm = ServiceHelper.GetServiceWithFallback<IGameStateManager>();
                return gsm != null ? gsm.CurrentGameState : currentGameState;
            }
        }
        public GameState PreviousGameState
        {
            get
            {
                var gsm = ServiceHelper.GetServiceWithFallback<IGameStateManager>();
                return gsm != null ? gsm.PreviousGameState : previousGameState;
            }
        }
        public int CurrentScore => ServiceHelper.GetServiceWithFallback<IScoreService>()?.CurrentScore ?? 0;
        public int CurrentLives => ServiceHelper.GetServiceWithFallback<IScoreService>()?.CurrentLives ?? 0;
        public bool IsPaused => ServiceHelper.GetServiceWithFallback<IPauseService>()?.IsPaused ?? false;
        public bool IsGameOver => CurrentLives <= 0;

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
            // no-op
        }

        private void Update()
        {
            if (CurrentGameState == GameState.Playing)
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
                // var validationResult = EventConnectionValidator.ValidateAllEventConnections(true); // TODO: EventConnectionValidator not available
                // Temporary: Skip validation
                bool validationPassed = true;
                
                if (!validationPassed)
                {
                    LogError($"Event connection validation skipped");
                    isValid = false;
                }
                else if (false && enableDebugLog) // Validation warnings check disabled
                {
                    LogWarning($"Event connection validation skipped");
                }
                else if (enableDebugLog)
                {
                    Log($"Event connection validation skipped");
                }
            }
            
            return isValid;
        }
        private void LogError(string message) {
            if (enableDebugLog) {
                Debug.LogError($"[GameManager] {message}");
            }
        }
        private void LogWarning(string message) {
            if (enableDebugLog) {
                Debug.LogWarning($"[GameManager] {message}");
            }
        }
        private void Log(string message) {
            if (enableDebugLog) {
                Debug.Log($"[GameManager] {message}");
            }
        }
        #endregion

        private void InitializeGameManager()
        {
            if (gameData == null) gameData = new asterivo.Unity60.Core.Data.GameData();
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
public void OnEventRaised(object value)
        {
            if (value is ICommandDefinition definition)
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
        private void HandleStartGame()
        {
            var loader = ServiceHelper.GetServiceWithFallback<ISceneLoadingService>();
            if (loader != null) { loader.LoadGameplaySceneWithMinTime(); return; }
            LogWarning("SceneLoadingService not found; cannot start game.");
        }
        private void HandlePauseGame() { if (CurrentGameState == GameState.Playing) ChangeGameState(GameState.Paused); }
        private void HandleResumeGame() { if (CurrentGameState == GameState.Paused) ChangeGameState(GameState.Playing); }
        private void HandleRestartGame()
        {
            var loader = ServiceHelper.GetServiceWithFallback<ISceneLoadingService>();
            if (loader != null) { loader.LoadGameplaySceneWithMinTime(); return; }
            LogWarning("SceneLoadingService not found; cannot restart game.");
        }
        private void HandleQuitGame() { Application.Quit(); }
        private void HandleReturnToMenu()
        {
            var loader = ServiceHelper.GetServiceWithFallback<ISceneLoadingService>();
            if (loader != null) { loader.LoadSceneWithMinTime(mainMenuSceneName); return; }
            LogWarning("SceneLoadingService not found; cannot return to menu.");
        }
        private void HandleGameOver() { if (CurrentGameState == GameState.Playing) ChangeGameState(GameState.GameOver); }
        private void HandleVictory() { if (CurrentGameState == GameState.Playing) ChangeGameState(GameState.Victory); }
        #endregion

        #region Game State Management
        private void ChangeGameState(GameState newState)
        {
            var gsm = ServiceHelper.GetServiceWithFallback<IGameStateManager>();
            if (gsm != null)
            {
                gsm.ChangeGameState(newState);
                Log($"Game state changed to: {newState} (via service)");
                return;
            }
            LogWarning("GameStateManagerService not found; state not changed.");
        }
        #endregion

        // Scene loading handled by SceneLoadingService

        private void OnPauseInputPerformed(InputAction.CallbackContext context)
        {
            if (CurrentGameState == GameState.Playing || CurrentGameState == GameState.Paused)
            {
                var pauseSvc = ServiceHelper.GetServiceWithFallback<IPauseService>();
                if (pauseSvc != null) { pauseSvc.TogglePause(); return; }
                onPauseGameCommand?.Raise();
            }
        }

        // Score/Lives handled by ScoreService

        // Pause handled by PauseService

        // Loading handled by SceneLoadingService

        // Coroutine management no longer required here
    }
}

