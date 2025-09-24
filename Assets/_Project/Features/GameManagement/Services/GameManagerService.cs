using System;
using System.Collections.Generic;
using UnityEngine;
using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Core.Services.Interfaces;
using asterivo.Unity60.Core.Commands;
using asterivo.Unity60.Core.Types;
using asterivo.Unity60.Features.GameManagement.Interfaces;
using asterivo.Unity60.Features.GameManagement.Events;

namespace asterivo.Unity60.Features.GameManagement.Services
{
    /// <summary>
    /// ゲーム管理機能のサービス実装
    /// ServiceLocatorパターンに基づき、イベント駆動で動作
    /// </summary>
    public class GameManagerService : IGameManager
    {
        // サービス基本情報
        public string ServiceName => "GameManagerService";
        public bool IsServiceActive { get; private set; }

        // ゲーム状態管理
        private GameState _currentGameState = GameState.MainMenu;
        private GameState _previousGameState = GameState.MainMenu;
        private float _gameTime = 0f;
        private bool _isPaused = false;
        private bool _isGameOver = false;

        // コマンド管理
        private readonly Stack<ICommand> _undoStack = new Stack<ICommand>();
        private readonly Stack<ICommand> _redoStack = new Stack<ICommand>();
        private const int MaxCommandHistory = 100;

        // 依存サービス参照
        private IEventManager _eventManager;
        private ISceneLoadingService _sceneLoader;
        private IPauseService _pauseService;

        // プロパティ
        public GameState CurrentGameState => _currentGameState;
        public GameState PreviousGameState => _previousGameState;
        public float GameTime => _gameTime;
        public bool IsPaused => _isPaused;
        public bool IsGameOver => _isGameOver;

        /// <summary>
        /// サービスの初期化
        /// </summary>
        public void OnServiceRegistered()
        {
            // 依存サービスの取得
            ServiceLocator.TryGet<IEventManager>(out _eventManager);
            ServiceLocator.TryGet<ISceneLoadingService>(out _sceneLoader);
            ServiceLocator.TryGet<IPauseService>(out _pauseService);

            // イベント購読
            SubscribeToEvents();

            // 初期状態設定
            _currentGameState = GameState.MainMenu;
            _previousGameState = GameState.MainMenu;
            _gameTime = 0f;
            _isPaused = false;
            _isGameOver = false;

            IsServiceActive = true;

            // 初期化完了イベント
            _eventManager?.RaiseEvent(GameManagementEventNames.OnGameManagerInitialized, this);

            Debug.Log($"[{ServiceName}] Initialized successfully");
        }

        /// <summary>
        /// サービスのシャットダウン
        /// </summary>
        public void OnServiceUnregistered()
        {
            // イベント購読解除
            UnsubscribeFromEvents();

            // コマンド履歴クリア
            _undoStack.Clear();
            _redoStack.Clear();

            IsServiceActive = false;

            // シャットダウン完了イベント
            _eventManager?.RaiseEvent(GameManagementEventNames.OnGameManagerShutdown, this);

            Debug.Log($"[{ServiceName}] Shutdown complete");
        }

        /// <summary>
        /// ゲーム状態の変更
        /// </summary>
        public void ChangeGameState(GameState newState)
        {
            if (_currentGameState == newState)
                return;

            // 状態変更前イベント
            _eventManager?.RaiseEvent(GameManagementEventNames.OnGameStateChanging,
                new GameStateChangeData(_currentGameState, newState));

            _previousGameState = _currentGameState;
            _currentGameState = newState;

            // 状態別処理
            HandleStateTransition(newState);

            // 状態変更後イベント
            _eventManager?.RaiseEvent(GameManagementEventNames.OnGameStateChanged,
                new GameStateChangeData(_previousGameState, _currentGameState));

            Debug.Log($"[{ServiceName}] Game state changed from {_previousGameState} to {_currentGameState}");
        }

        /// <summary>
        /// ゲームの開始
        /// </summary>
        public void StartGame()
        {
            if (_currentGameState == GameState.MainMenu || _currentGameState == GameState.GameOver)
            {
                ChangeGameState(GameState.Loading);

                // シーンローディングサービスに委譲
                if (_sceneLoader != null)
                {
                    _sceneLoader.LoadGameplaySceneWithMinTime();
                }
                else
                {
                    Debug.LogWarning($"[{ServiceName}] SceneLoadingService not available");
                    ChangeGameState(GameState.Playing);
                }

                _gameTime = 0f;
                _isGameOver = false;

                _eventManager?.RaiseEvent(GameManagementEventNames.OnGameStarted, null);
            }
        }

        /// <summary>
        /// ゲームの一時停止
        /// </summary>
        public void PauseGame()
        {
            if (_currentGameState == GameState.Playing)
            {
                ChangeGameState(GameState.Paused);
                _isPaused = true;

                // PauseServiceとの連携
                _pauseService?.SetPaused(true);

                _eventManager?.RaiseEvent(GameManagementEventNames.OnGamePaused, null);
            }
        }

        /// <summary>
        /// ゲームの再開
        /// </summary>
        public void ResumeGame()
        {
            if (_currentGameState == GameState.Paused)
            {
                ChangeGameState(GameState.Playing);
                _isPaused = false;

                // PauseServiceとの連携
                _pauseService?.SetPaused(false);

                _eventManager?.RaiseEvent(GameManagementEventNames.OnGameResumed, null);
            }
        }

        /// <summary>
        /// ゲームのリスタート
        /// </summary>
        public void RestartGame()
        {
            _gameTime = 0f;
            _isGameOver = false;
            _undoStack.Clear();
            _redoStack.Clear();

            ChangeGameState(GameState.Loading);

            if (_sceneLoader != null)
            {
                _sceneLoader.LoadGameplaySceneWithMinTime();
            }
            else
            {
                Debug.LogWarning($"[{ServiceName}] SceneLoadingService not available");
                ChangeGameState(GameState.Playing);
            }

            _eventManager?.RaiseEvent(GameManagementEventNames.OnGameRestarted, null);
        }

        /// <summary>
        /// ゲームの終了
        /// </summary>
        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        /// <summary>
        /// メインメニューに戻る
        /// </summary>
        public void ReturnToMenu()
        {
            _gameTime = 0f;
            _isGameOver = false;
            _undoStack.Clear();
            _redoStack.Clear();

            ChangeGameState(GameState.Loading);

            if (_sceneLoader != null)
            {
                _sceneLoader.LoadSceneWithMinTime("MainMenu");
            }
            else
            {
                Debug.LogWarning($"[{ServiceName}] SceneLoadingService not available");
                ChangeGameState(GameState.MainMenu);
            }

            _eventManager?.RaiseEvent(GameManagementEventNames.OnReturnToMainMenu, null);
        }

        /// <summary>
        /// ゲームオーバー処理
        /// </summary>
        public void TriggerGameOver()
        {
            if (_currentGameState == GameState.Playing)
            {
                _isGameOver = true;
                ChangeGameState(GameState.GameOver);

                _eventManager?.RaiseEvent(GameManagementEventNames.OnGameOver, null);
            }
        }

        /// <summary>
        /// 勝利処理
        /// </summary>
        public void TriggerVictory()
        {
            if (_currentGameState == GameState.Playing)
            {
                ChangeGameState(GameState.Victory);

                _eventManager?.RaiseEvent(GameManagementEventNames.OnGameVictory, null);
            }
        }

        /// <summary>
        /// コマンドの実行
        /// </summary>
        public void ExecuteCommand(ICommand command)
        {
            if (command == null || !command.CanExecute())
                return;

            command.Execute();

            // Undoスタックに追加
            if (command is IUndoableCommand)
            {
                _undoStack.Push(command);
                _redoStack.Clear();

                // 履歴制限
                while (_undoStack.Count > MaxCommandHistory)
                {
                    var oldCommand = _undoStack.ToArray()[_undoStack.Count - 1];
                    var tempStack = new Stack<ICommand>();

                    while (_undoStack.Count > 1)
                        tempStack.Push(_undoStack.Pop());

                    _undoStack.Clear();

                    while (tempStack.Count > 0)
                        _undoStack.Push(tempStack.Pop());
                }
            }

            _eventManager?.RaiseEvent(GameManagementEventNames.OnCommandExecuted, command);

            Debug.Log($"[{ServiceName}] Command executed: {command.GetType().Name}");
        }

        /// <summary>
        /// 最後のコマンドを取り消し
        /// </summary>
        public void UndoLastCommand()
        {
            if (_undoStack.Count > 0)
            {
                var command = _undoStack.Pop();

                if (command is IUndoableCommand undoableCommand && undoableCommand.CanUndo())
                {
                    undoableCommand.Undo();
                    _redoStack.Push(command);

                    _eventManager?.RaiseEvent(GameManagementEventNames.OnCommandUndone, command);

                    Debug.Log($"[{ServiceName}] Command undone: {command.GetType().Name}");
                }
            }
        }

        /// <summary>
        /// 最後に取り消したコマンドをやり直し
        /// </summary>
        public void RedoLastCommand()
        {
            if (_redoStack.Count > 0)
            {
                var command = _redoStack.Pop();

                if (command.CanExecute())
                {
                    command.Execute();
                    _undoStack.Push(command);

                    _eventManager?.RaiseEvent(GameManagementEventNames.OnCommandRedone, command);

                    Debug.Log($"[{ServiceName}] Command redone: {command.GetType().Name}");
                }
            }
        }

        /// <summary>
        /// ポーズ状態を切り替え
        /// </summary>
        public void TogglePause()
        {
            if (_currentGameState == GameState.Playing)
            {
                PauseGame();
            }
            else if (_currentGameState == GameState.Paused)
            {
                ResumeGame();
            }
        }

        /// <summary>
        /// ゲーム時間を更新
        /// </summary>
        public void UpdateGameTime(float deltaTime)
        {
            if (_currentGameState == GameState.Playing && !_isPaused)
            {
                _gameTime += deltaTime;

                // 定期的に時間更新イベントを発行
                if (Mathf.FloorToInt(_gameTime) != Mathf.FloorToInt(_gameTime - deltaTime))
                {
                    _eventManager?.RaiseEvent(GameManagementEventNames.OnGameTimeUpdated, _gameTime);
                }
            }
        }

        /// <summary>
        /// 状態遷移時の処理
        /// </summary>
        private void HandleStateTransition(GameState newState)
        {
            switch (newState)
            {
                case GameState.MainMenu:
                    Time.timeScale = 1f;
                    break;

                case GameState.Playing:
                    Time.timeScale = 1f;
                    break;

                case GameState.Paused:
                    Time.timeScale = 0f;
                    break;

                case GameState.GameOver:
                case GameState.Victory:
                    Time.timeScale = 0f;
                    break;

                case GameState.Loading:
                    Time.timeScale = 1f;
                    break;
            }
        }

        /// <summary>
        /// イベント購読
        /// </summary>
        private void SubscribeToEvents()
        {
            // ここでは他のサービスからのイベントを購読
            // 例: "PlayerDeath" イベントを受けてゲームオーバー処理
        }

        /// <summary>
        /// イベント購読解除
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            // イベント購読の解除処理
        }
    }

    /// <summary>
    /// ゲーム状態変更データ
    /// </summary>
    public struct GameStateChangeData
    {
        public GameState PreviousState { get; }
        public GameState NewState { get; }

        public GameStateChangeData(GameState previousState, GameState newState)
        {
            PreviousState = previousState;
            NewState = newState;
        }
    }
}