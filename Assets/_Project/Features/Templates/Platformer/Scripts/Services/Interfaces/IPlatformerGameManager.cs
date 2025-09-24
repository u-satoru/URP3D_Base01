using System;
using UnityEngine;
using asterivo.Unity60.Features.Templates.Platformer.Settings;

namespace asterivo.Unity60.Features.Templates.Platformer.Services
{
    /// <summary>
    /// Platformerゲームマネージャー インターフェース
    /// ServiceLocator + Event駆動ハイブリッドアーキテクチャの中核
    /// 中央ゲーム状態管理・プレイヤー制御の抽象化
    /// </summary>
    public interface IPlatformerGameManager : IDisposable
    {
        // ゲーム状態プロパティ
        GameState CurrentState { get; }
        bool IsGamePaused { get; }
        bool IsPlayerAlive { get; }
        int CurrentLevel { get; }
        float GameTime { get; }
        int PlayerScore { get; }
        int PlayerLives { get; }
        int PlayerHealth { get; }

        // ゲーム制御メソッド
        void StartGame();
        void PauseGame();
        void ResumeGame();
        void RestartLevel();
        void NextLevel();
        void GameOver();
        void CompleteLevel();

        // プレイヤー状態管理
        void SetPlayerHealth(int health);
        void DamagePlayer(int damage);
        void HealPlayer(int healing);
        void AddLife();
        void RemoveLife();
        void AddScore(int points);
        void ResetPlayerStats();

        // 設定管理
        void UpdateSettings(PlatformerGameplaySettings newSettings);
        void SetGameDifficulty(PlatformerGameplaySettings.GameDifficulty difficulty);

        // イベント（Event駆動アーキテクチャ統合）
        event Action<GameState> OnGameStateChanged;
        event Action<int> OnScoreChanged;
        event Action<int> OnHealthChanged;
        event Action<int> OnLivesChanged;
        event Action OnPlayerDied;
        event Action OnGameOver;
        event Action OnLevelCompleted;
        event Action<bool> OnGamePausedChanged;
    }

    /// <summary>
    /// ゲーム状態列挙型：状態管理パターン統合
    /// </summary>
    public enum GameState
    {
        MainMenu,
        Loading,
        Playing,
        Paused,
        LevelComplete,
        GameOver,
        Settings
    }
}