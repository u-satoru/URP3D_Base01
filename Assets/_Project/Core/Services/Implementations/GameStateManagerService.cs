using UnityEngine;
using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Audio; // GameState enum用
using asterivo.Unity60.Core.Lifecycle;

namespace asterivo.Unity60.Core
{
    /// <summary>
    /// GameManagerのゲーム状態関連を委譲するサービス実装
    /// </summary>
    public class GameStateManagerService : MonoBehaviour, IGameStateManager, IServiceLocatorRegistrable
    {
        [Header("Events")] [SerializeField] private GenericGameEvent<GameState> gameStateChangedEvent;

        [Header("Runtime")]
        [SerializeField] private GameState currentGameState = GameState.MainMenu;
        [SerializeField] private GameState previousGameState = GameState.MainMenu;

        // GameManager reference removed - Core層からFeatures層への参照禁止
        [SerializeField] private int priority = 50;

        public int Priority => priority;

        private void Reset()
        {
            // GameManager fallback removed - ServiceLocatorパターンのみ使用
        }

        public void RegisterServices()
        {
            if (FeatureFlags.UseServiceLocator)
            {
                ServiceLocator.RegisterService<IGameStateManager>(this);
            }
        }

        public void UnregisterServices()
        {
            if (FeatureFlags.UseServiceLocator)
            {
                ServiceLocator.UnregisterService<IGameStateManager>();
            }
        }

        public GameState CurrentGameState => currentGameState;
        public GameState PreviousGameState => previousGameState;
        public bool IsGameOver => false; // game over判定はScoreService側で行う

        public void ChangeGameState(GameState newState)
        {
            if (currentGameState == newState) return;
            previousGameState = currentGameState;
            currentGameState = newState;
            gameStateChangedEvent?.Raise(currentGameState);
        }
    }
}
