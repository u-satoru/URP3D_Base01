using UnityEngine;
using UnityEngine.InputSystem;
using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Core.Services.Interfaces;
using asterivo.Unity60.Core.Types;
using asterivo.Unity60.Core.Commands;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Features.GameManagement.Interfaces;
using asterivo.Unity60.Features.GameManagement.Events;

namespace asterivo.Unity60.Features.GameManagement
{
    /// <summary>
    /// 既存のGameManager MonoBehaviourと新しいGameManagerServiceを繋ぐアダプター
    /// レガシーコードとの互換性を保ちながら段階的移行を実現
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

        // プロパティ（既存のGameManagerとの互換性）
        public GameState CurrentGameState => _gameManagerService?.CurrentGameState ?? GameState.MainMenu;
        public GameState PreviousGameState => _gameManagerService?.PreviousGameState ?? GameState.MainMenu;
        public float GameTime => _gameManagerService?.GameTime ?? 0f;
        public bool IsPaused => _gameManagerService?.IsPaused ?? false;
        public bool IsGameOver => _gameManagerService?.IsGameOver ?? false;

        #region Unity Lifecycle

        private void Awake()
        {
            // ServiceLocatorからサービスを取得
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
            // GameManagerServiceにゲーム時間の更新を委譲
            if (_gameManagerService != null && CurrentGameState == GameState.Playing)
            {
                _gameManagerService.UpdateGameTime(Time.deltaTime);

                // レガシーイベント発行（必要に応じて）
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
            // ServiceLocatorからサービスを取得
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
                // 新しいイベントシステムへの購読
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
            // レガシーイベントシステムへのブリッジ
            if (_useLegacyEventSystem && gameStateChangedEvent != null)
            {
                gameStateChangedEvent.Raise(data.NewState);
            }

            Log($"Game state changed from {data.PreviousState} to {data.NewState}");
        }

        #endregion

        #region Public API（GameManagerとの互換性）

        /// <summary>
        /// ゲームを開始
        /// </summary>
        public void StartGame()
        {
            _gameManagerService?.StartGame();
        }

        /// <summary>
        /// ゲームを一時停止
        /// </summary>
        public void PauseGame()
        {
            _gameManagerService?.PauseGame();
        }

        /// <summary>
        /// ゲームを再開
        /// </summary>
        public void ResumeGame()
        {
            _gameManagerService?.ResumeGame();
        }

        /// <summary>
        /// ゲームをリスタート
        /// </summary>
        public void RestartGame()
        {
            _gameManagerService?.RestartGame();
        }

        /// <summary>
        /// ゲームを終了
        /// </summary>
        public void QuitGame()
        {
            _gameManagerService?.QuitGame();
        }

        /// <summary>
        /// メインメニューに戻る
        /// </summary>
        public void ReturnToMenu()
        {
            _gameManagerService?.ReturnToMenu();
        }

        /// <summary>
        /// ゲームオーバー
        /// </summary>
        public void TriggerGameOver()
        {
            _gameManagerService?.TriggerGameOver();
        }

        /// <summary>
        /// 勝利
        /// </summary>
        public void TriggerVictory()
        {
            _gameManagerService?.TriggerVictory();
        }

        /// <summary>
        /// コマンドを実行
        /// </summary>
        public void ExecuteCommand(ICommand command)
        {
            _gameManagerService?.ExecuteCommand(command);
        }

        /// <summary>
        /// 最後のコマンドを取り消し
        /// </summary>
        public void UndoLastCommand()
        {
            _gameManagerService?.UndoLastCommand();
        }

        /// <summary>
        /// 最後のコマンドをやり直し
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
