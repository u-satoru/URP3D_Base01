using UnityEngine;
using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Audio; // GameState enum用
using asterivo.Unity60.Core.Lifecycle;

namespace asterivo.Unity60.Core
{
    /// <summary>
    /// GameManagerのポーズ制御を委譲するサービス実装
    /// </summary>
    public class PauseService : MonoBehaviour, IPauseService, IServiceLocatorRegistrable
    {
        [Header("Events")] [SerializeField] private BoolGameEvent onPauseStateChanged;
        [Header("Settings")] [SerializeField] private bool pauseTimeOnPause = true;
        [Header("Runtime")] [SerializeField] private bool isPaused = false;
        // GameManager reference removed - Core層からFeatures層への参照禁止
        [SerializeField] private int priority = 80;

        public int Priority => priority;

        private void Reset()
        {
            // GameManager fallback removed - ServiceLocatorパターンのみ使用
        }

        public void RegisterServices()
        {
            if (FeatureFlags.UseServiceLocator)
            {
                ServiceLocator.RegisterService<IPauseService>(this);
            }
        }

        public void UnregisterServices()
        {
            if (FeatureFlags.UseServiceLocator)
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
