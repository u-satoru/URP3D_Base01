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
        [Header("Events")] [SerializeField] private GameStateEvent gameStateChangedEvent;

        [Header("Runtime")]
        [SerializeField] private GameState currentGameState = GameState.MainMenu;
        [SerializeField] private GameState previousGameState = GameState.MainMenu;

        [Header("Back-Compat")] [SerializeField] private GameManager gameManager; // optional for early boot
        [SerializeField] private int priority = 50;

        public int Priority => priority;

        private void Reset()
        {
            if (gameManager == null) gameManager = FindFirstObjectByType<GameManager>();
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

        public GameState CurrentGameState => gameManager != null ? gameManager.CurrentGameState : currentGameState;
        public GameState PreviousGameState => gameManager != null ? gameManager.PreviousGameState : previousGameState;
        public bool IsGameOver => false; // game over判定はScoreService側で行う

        public void ChangeGameState(GameState newState)
        {
            if (gameManager != null)
            {
                // 旧システムが優先の場合はGameManagerに委譲（後方互換）
                // GameManager側がServiceに委譲するためループは起きない
            }

            if (currentGameState == newState) return;
            previousGameState = currentGameState;
            currentGameState = newState;
            gameStateChangedEvent?.Raise(currentGameState);
        }
    }
}
