using System;
using System.Collections.Generic;
using UnityEngine;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Commands;
using asterivo.Unity60.Core.Types;
using asterivo.Unity60.Features.GameManagement.Interfaces;
using asterivo.Unity60.Features.GameManagement.Events;

namespace asterivo.Unity60.Features.GameManagement.Services
{
    /// <summary>
    /// 繧ｲ繝ｼ繝邂｡逅・ｩ溯・縺ｮ繧ｵ繝ｼ繝薙せ螳溯｣・
    /// ServiceLocator繝代ち繝ｼ繝ｳ縺ｫ蝓ｺ縺･縺阪√う繝吶Φ繝磯ｧ・虚縺ｧ蜍穂ｽ・
    /// </summary>
    public class GameManagerService : IGameManager
    {
        // 繧ｵ繝ｼ繝薙せ蝓ｺ譛ｬ諠・ｱ
        public string ServiceName => "GameManagerService";
        public bool IsServiceActive { get; private set; }

        // 繧ｲ繝ｼ繝迥ｶ諷狗ｮ｡逅・
        private GameState _currentGameState = GameState.MainMenu;
        private GameState _previousGameState = GameState.MainMenu;
        private float _gameTime = 0f;
        private bool _isPaused = false;
        private bool _isGameOver = false;

        // 繧ｳ繝槭Φ繝臥ｮ｡逅・
        private readonly Stack<ICommand> _undoStack = new Stack<ICommand>();
        private readonly Stack<ICommand> _redoStack = new Stack<ICommand>();
        private const int MaxCommandHistory = 100;

        // 萓晏ｭ倥し繝ｼ繝薙せ蜿ら・
        private IEventManager _eventManager;
        private ISceneLoadingService _sceneLoader;
        private IPauseService _pauseService;

        // 繝励Ο繝代ユ繧｣
        public GameState CurrentGameState => _currentGameState;
        public GameState PreviousGameState => _previousGameState;
        public float GameTime => _gameTime;
        public bool IsPaused => _isPaused;
        public bool IsGameOver => _isGameOver;

        /// <summary>
        /// 繧ｵ繝ｼ繝薙せ縺ｮ蛻晄悄蛹・
        /// </summary>
        public void OnServiceRegistered()
        {
            // 萓晏ｭ倥し繝ｼ繝薙せ縺ｮ蜿門ｾ・
            ServiceLocator.TryGet<IEventManager>(out _eventManager);
            ServiceLocator.TryGet<ISceneLoadingService>(out _sceneLoader);
            ServiceLocator.TryGet<IPauseService>(out _pauseService);

            // 繧､繝吶Φ繝郁ｳｼ隱ｭ
            SubscribeToEvents();

            // 蛻晄悄迥ｶ諷玖ｨｭ螳・
            _currentGameState = GameState.MainMenu;
            _previousGameState = GameState.MainMenu;
            _gameTime = 0f;
            _isPaused = false;
            _isGameOver = false;

            IsServiceActive = true;

            // 蛻晄悄蛹門ｮ御ｺ・う繝吶Φ繝・
            _eventManager?.RaiseEvent(GameManagementEventNames.OnGameManagerInitialized, this);

            Debug.Log($"[{ServiceName}] Initialized successfully");
        }

        /// <summary>
        /// 繧ｵ繝ｼ繝薙せ縺ｮ繧ｷ繝｣繝・ヨ繝繧ｦ繝ｳ
        /// </summary>
        public void OnServiceUnregistered()
        {
            // 繧､繝吶Φ繝郁ｳｼ隱ｭ隗｣髯､
            UnsubscribeFromEvents();

            // 繧ｳ繝槭Φ繝牙ｱ･豁ｴ繧ｯ繝ｪ繧｢
            _undoStack.Clear();
            _redoStack.Clear();

            IsServiceActive = false;

            // 繧ｷ繝｣繝・ヨ繝繧ｦ繝ｳ螳御ｺ・う繝吶Φ繝・
            _eventManager?.RaiseEvent(GameManagementEventNames.OnGameManagerShutdown, this);

            Debug.Log($"[{ServiceName}] Shutdown complete");
        }

        /// <summary>
        /// 繧ｲ繝ｼ繝迥ｶ諷九・螟画峩
        /// </summary>
        public void ChangeGameState(GameState newState)
        {
            if (_currentGameState == newState)
                return;

            // 迥ｶ諷句､画峩蜑阪う繝吶Φ繝・
            _eventManager?.RaiseEvent(GameManagementEventNames.OnGameStateChanging,
                new GameStateChangeData(_currentGameState, newState));

            _previousGameState = _currentGameState;
            _currentGameState = newState;

            // 迥ｶ諷句挨蜃ｦ逅・
            HandleStateTransition(newState);

            // 迥ｶ諷句､画峩蠕後う繝吶Φ繝・
            _eventManager?.RaiseEvent(GameManagementEventNames.OnGameStateChanged,
                new GameStateChangeData(_previousGameState, _currentGameState));

            Debug.Log($"[{ServiceName}] Game state changed from {_previousGameState} to {_currentGameState}");
        }

        /// <summary>
        /// 繧ｲ繝ｼ繝縺ｮ髢句ｧ・
        /// </summary>
        public void StartGame()
        {
            if (_currentGameState == GameState.MainMenu || _currentGameState == GameState.GameOver)
            {
                ChangeGameState(GameState.Loading);

                // 繧ｷ繝ｼ繝ｳ繝ｭ繝ｼ繝・ぅ繝ｳ繧ｰ繧ｵ繝ｼ繝薙せ縺ｫ蟋碑ｭｲ
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
        /// 繧ｲ繝ｼ繝縺ｮ荳譎ょ●豁｢
        /// </summary>
        public void PauseGame()
        {
            if (_currentGameState == GameState.Playing)
            {
                ChangeGameState(GameState.Paused);
                _isPaused = true;

                // PauseService縺ｨ縺ｮ騾｣謳ｺ
                _pauseService?.SetPaused(true);

                _eventManager?.RaiseEvent(GameManagementEventNames.OnGamePaused, null);
            }
        }

        /// <summary>
        /// 繧ｲ繝ｼ繝縺ｮ蜀埼幕
        /// </summary>
        public void ResumeGame()
        {
            if (_currentGameState == GameState.Paused)
            {
                ChangeGameState(GameState.Playing);
                _isPaused = false;

                // PauseService縺ｨ縺ｮ騾｣謳ｺ
                _pauseService?.SetPaused(false);

                _eventManager?.RaiseEvent(GameManagementEventNames.OnGameResumed, null);
            }
        }

        /// <summary>
        /// 繧ｲ繝ｼ繝縺ｮ繝ｪ繧ｹ繧ｿ繝ｼ繝・
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
        /// 繧ｲ繝ｼ繝縺ｮ邨ゆｺ・
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
        /// 繝｡繧､繝ｳ繝｡繝九Η繝ｼ縺ｫ謌ｻ繧・
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
        /// 繧ｲ繝ｼ繝繧ｪ繝ｼ繝舌・蜃ｦ逅・
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
        /// 蜍晏茜蜃ｦ逅・
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
        /// 繧ｳ繝槭Φ繝峨・螳溯｡・
        /// </summary>
        public void ExecuteCommand(ICommand command)
        {
            if (command == null || !command.CanExecute())
                return;

            command.Execute();

            // Undo繧ｹ繧ｿ繝・け縺ｫ霑ｽ蜉
            if (command is IUndoableCommand)
            {
                _undoStack.Push(command);
                _redoStack.Clear();

                // 螻･豁ｴ蛻ｶ髯・
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
        /// 譛蠕後・繧ｳ繝槭Φ繝峨ｒ蜿悶ｊ豸医＠
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
        /// 譛蠕後↓蜿悶ｊ豸医＠縺溘さ繝槭Φ繝峨ｒ繧・ｊ逶ｴ縺・
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
        /// 繝昴・繧ｺ迥ｶ諷九ｒ蛻・ｊ譖ｿ縺・
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
        /// 繧ｲ繝ｼ繝譎る俣繧呈峩譁ｰ
        /// </summary>
        public void UpdateGameTime(float deltaTime)
        {
            if (_currentGameState == GameState.Playing && !_isPaused)
            {
                _gameTime += deltaTime;

                // 螳壽悄逧・↓譎る俣譖ｴ譁ｰ繧､繝吶Φ繝医ｒ逋ｺ陦・
                if (Mathf.FloorToInt(_gameTime) != Mathf.FloorToInt(_gameTime - deltaTime))
                {
                    _eventManager?.RaiseEvent(GameManagementEventNames.OnGameTimeUpdated, _gameTime);
                }
            }
        }

        /// <summary>
        /// 迥ｶ諷矩・遘ｻ譎ゅ・蜃ｦ逅・
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
        /// 繧､繝吶Φ繝郁ｳｼ隱ｭ
        /// </summary>
        private void SubscribeToEvents()
        {
            // 縺薙％縺ｧ縺ｯ莉悶・繧ｵ繝ｼ繝薙せ縺九ｉ縺ｮ繧､繝吶Φ繝医ｒ雉ｼ隱ｭ
            // 萓・ "PlayerDeath" 繧､繝吶Φ繝医ｒ蜿励￠縺ｦ繧ｲ繝ｼ繝繧ｪ繝ｼ繝舌・蜃ｦ逅・
        }

        /// <summary>
        /// 繧､繝吶Φ繝郁ｳｼ隱ｭ隗｣髯､
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            // 繧､繝吶Φ繝郁ｳｼ隱ｭ縺ｮ隗｣髯､蜃ｦ逅・
        }
    }

    /// <summary>
    /// 繧ｲ繝ｼ繝迥ｶ諷句､画峩繝・・繧ｿ
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


