using UnityEngine;
using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Audio; // GameState enum用

namespace asterivo.Unity60.Core
{
    /// <summary>
    /// GameManagerのポーズ制御を委譲するサービス実装
    /// </summary>
    public class PauseService : MonoBehaviour, IPauseService, _Project.Core.IServiceLocatorRegistrable
    {
        [Header("Events")] [SerializeField] private BoolGameEvent onPauseStateChanged;
        [Header("Settings")] [SerializeField] private bool pauseTimeOnPause = true;
        [Header("Runtime")] [SerializeField] private bool isPaused = false;
        [SerializeField] private GameManager gameManager; // optional fallback
        [SerializeField] private int priority = 80;

        public int Priority => priority;

        private void Reset()
        {
            if (gameManager == null) gameManager = FindFirstObjectByType<GameManager>();
        }

        public void RegisterServices()
        {
            if (CoreFeatureFlags.UseServiceLocator)
            {
                ServiceLocator.RegisterService<IPauseService>(this);
            }
        }

        public void UnregisterServices()
        {
            if (CoreFeatureFlags.UseServiceLocator)
            {
                ServiceLocator.UnregisterService<IPauseService>();
            }
        }

        public bool IsPaused => isPaused;

        public void TogglePause() => SetPauseState(!isPaused);

        public void SetPauseState(bool paused)
        {
            if (isPaused == paused) return;
            isPaused = paused;
            if (pauseTimeOnPause)
            {
                Time.timeScale = paused ? 0f : 1f;
            }
            var gsm = ServiceLocator.GetService<IGameStateManager>();
            gsm?.ChangeGameState(paused ? GameState.Paused : GameState.Gameplay);
            onPauseStateChanged?.Raise(isPaused);
        }

        public void ResumeGame() => SetPauseState(false);
    }
}
