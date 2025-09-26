using UnityEngine;
using UnityEngine.InputSystem;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Types;
using asterivo.Unity60.Core.Commands;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Features.GameManagement.Interfaces;
using asterivo.Unity60.Features.GameManagement.Events;

namespace asterivo.Unity60.Features.GameManagement
{
    /// <summary>
    /// 譌｢蟄倥・GameManager MonoBehaviour縺ｨ譁ｰ縺励＞GameManagerService繧堤ｹ九＄繧｢繝繝励ち繝ｼ
    /// 繝ｬ繧ｬ繧ｷ繝ｼ繧ｳ繝ｼ繝峨→縺ｮ莠呈鋤諤ｧ繧剃ｿ昴■縺ｪ縺後ｉ谿ｵ髫守噪遘ｻ陦後ｒ螳溽樟
    /// </summary>
    public class GameManagerAdapter : MonoBehaviour
    {
        [Header("Legacy Compatibility")]
        [SerializeField] private bool _useLegacyEventSystem = false;
        [SerializeField] private bool _enableDebugLog = true;

        [Header("Legacy Event Channels - Output")]
        [SerializeField] private GameStateEvent gameStateChangedEvent;
        [SerializeField] private FloatGameEvent gameTimeUpdatedEvent;

        [Header("Input")]
        [SerializeField] private InputActionAsset inputActions;
        private InputAction pauseAction;

        private IGameManager _gameManagerService;
        private IEventManager _eventManager;

        // 繝励Ο繝代ユ繧｣・域里蟄倥・GameManager縺ｨ縺ｮ莠呈鋤諤ｧ・・
        public GameState CurrentGameState => _gameManagerService?.CurrentGameState ?? GameState.MainMenu;
        public GameState PreviousGameState => _gameManagerService?.PreviousGameState ?? GameState.MainMenu;
        public float GameTime => _gameManagerService?.GameTime ?? 0f;
        public bool IsPaused => _gameManagerService?.IsPaused ?? false;
        public bool IsGameOver => _gameManagerService?.IsGameOver ?? false;

        #region Unity Lifecycle

        private void Awake()
        {
            // ServiceLocator縺九ｉ繧ｵ繝ｼ繝薙せ繧貞叙蠕・
            InitializeServices();
        }

        private void OnEnable()
        {
            InitializeInput();
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
            CleanupInput();
        }

        private void Update()
        {
            // GameManagerService縺ｫ繧ｲ繝ｼ繝譎る俣縺ｮ譖ｴ譁ｰ繧貞ｧ碑ｭｲ
            if (_gameManagerService != null && CurrentGameState == GameState.Playing)
            {
                _gameManagerService.UpdateGameTime(Time.deltaTime);

                // 繝ｬ繧ｬ繧ｷ繝ｼ繧､繝吶Φ繝育匱陦鯉ｼ亥ｿ・ｦ√↓蠢懊§縺ｦ・・
                if (_useLegacyEventSystem && gameTimeUpdatedEvent != null)
                {
                    gameTimeUpdatedEvent.Raise(GameTime);
                }
            }
        }

        #endregion

        #region Initialization

        private void InitializeServices()
        {
            // ServiceLocator縺九ｉ繧ｵ繝ｼ繝薙せ繧貞叙蠕・
            if (!ServiceLocator.TryGet<IGameManager>(out _gameManagerService))
            {
                LogWarning("GameManagerService not found in ServiceLocator");
            }

            if (!ServiceLocator.TryGet<IEventManager>(out _eventManager))
            {
                LogWarning("EventManager not found in ServiceLocator");
            }
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
                LogWarning("InputActionAsset not assigned");
            }
        }

        private void CleanupInput()
        {
            if (pauseAction != null)
            {
                pauseAction.performed -= OnPauseInputPerformed;
                pauseAction.Disable();
            }
        }

        #endregion

        #region Event Handling

        private void SubscribeToEvents()
        {
            if (_eventManager != null)
            {
                // 譁ｰ縺励＞繧､繝吶Φ繝医す繧ｹ繝・Β縺ｸ縺ｮ雉ｼ隱ｭ
                _eventManager.Subscribe<GameStateChangeData>(
                    GameManagementEventNames.OnGameStateChanged,
                    OnGameStateChanged);
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (_eventManager != null)
            {
                _eventManager.Unsubscribe<GameStateChangeData>(
                    GameManagementEventNames.OnGameStateChanged,
                    OnGameStateChanged);
            }
        }

        private void OnGameStateChanged(GameStateChangeData data)
        {
            // 繝ｬ繧ｬ繧ｷ繝ｼ繧､繝吶Φ繝医す繧ｹ繝・Β縺ｸ縺ｮ繝悶Μ繝・ず
            if (_useLegacyEventSystem && gameStateChangedEvent != null)
            {
                gameStateChangedEvent.Raise(data.NewState);
            }

            Log($"Game state changed from {data.PreviousState} to {data.NewState}");
        }

        #endregion

        #region Public API・・ameManager縺ｨ縺ｮ莠呈鋤諤ｧ・・

        /// <summary>
        /// 繧ｲ繝ｼ繝繧帝幕蟋・
        /// </summary>
        public void StartGame()
        {
            _gameManagerService?.StartGame();
        }

        /// <summary>
        /// 繧ｲ繝ｼ繝繧剃ｸ譎ょ●豁｢
        /// </summary>
        public void PauseGame()
        {
            _gameManagerService?.PauseGame();
        }

        /// <summary>
        /// 繧ｲ繝ｼ繝繧貞・髢・
        /// </summary>
        public void ResumeGame()
        {
            _gameManagerService?.ResumeGame();
        }

        /// <summary>
        /// 繧ｲ繝ｼ繝繧偵Μ繧ｹ繧ｿ繝ｼ繝・
        /// </summary>
        public void RestartGame()
        {
            _gameManagerService?.RestartGame();
        }

        /// <summary>
        /// 繧ｲ繝ｼ繝繧堤ｵゆｺ・
        /// </summary>
        public void QuitGame()
        {
            _gameManagerService?.QuitGame();
        }

        /// <summary>
        /// 繝｡繧､繝ｳ繝｡繝九Η繝ｼ縺ｫ謌ｻ繧・
        /// </summary>
        public void ReturnToMenu()
        {
            _gameManagerService?.ReturnToMenu();
        }

        /// <summary>
        /// 繧ｲ繝ｼ繝繧ｪ繝ｼ繝舌・
        /// </summary>
        public void TriggerGameOver()
        {
            _gameManagerService?.TriggerGameOver();
        }

        /// <summary>
        /// 蜍晏茜
        /// </summary>
        public void TriggerVictory()
        {
            _gameManagerService?.TriggerVictory();
        }

        /// <summary>
        /// 繧ｳ繝槭Φ繝峨ｒ螳溯｡・
        /// </summary>
        public void ExecuteCommand(ICommand command)
        {
            _gameManagerService?.ExecuteCommand(command);
        }

        /// <summary>
        /// 譛蠕後・繧ｳ繝槭Φ繝峨ｒ蜿悶ｊ豸医＠
        /// </summary>
        public void UndoLastCommand()
        {
            _gameManagerService?.UndoLastCommand();
        }

        /// <summary>
        /// 譛蠕後・繧ｳ繝槭Φ繝峨ｒ繧・ｊ逶ｴ縺・
        /// </summary>
        public void RedoLastCommand()
        {
            _gameManagerService?.RedoLastCommand();
        }

        #endregion

        #region Input Handling

        private void OnPauseInputPerformed(InputAction.CallbackContext context)
        {
            if (CurrentGameState == GameState.Playing || CurrentGameState == GameState.Paused)
            {
                _gameManagerService?.TogglePause();
            }
        }

        #endregion

        #region Logging

        private void Log(string message)
        {
            if (_enableDebugLog)
                Debug.Log($"[GameManagerAdapter] {message}");
        }

        private void LogWarning(string message)
        {
            if (_enableDebugLog)
                Debug.LogWarning($"[GameManagerAdapter] {message}");
        }

        private void LogError(string message)
        {
            if (_enableDebugLog)
                Debug.LogError($"[GameManagerAdapter] {message}");
        }

        #endregion
    }
}


