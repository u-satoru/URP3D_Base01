using UnityEngine;
using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Audio; // GameState enum逕ｨ
// using asterivo.Unity60.Core.Lifecycle;

namespace asterivo.Unity60.Core
{
    /// <summary>
    /// GameManager縺ｮ繝昴・繧ｺ蛻ｶ蠕｡繧貞ｧ碑ｭｲ縺吶ｋ繧ｵ繝ｼ繝薙せ螳溯｣・    /// </summary>
    public class PauseService : MonoBehaviour, IPauseService, IServiceLocatorRegistrable
    {
        [Header("Events")] [SerializeField] private BoolGameEvent onPauseStateChanged;
        [Header("Settings")] [SerializeField] private bool pauseTimeOnPause = true;
        [Header("Runtime")] [SerializeField] private bool isPaused = false;
        // GameManager reference removed - Core螻､縺九ｉFeatures螻､縺ｸ縺ｮ蜿ら・遖∵ｭ｢
        [SerializeField] private int priority = 80;

        public int Priority => priority;

        private void Reset()
        {
            // GameManager fallback removed - ServiceLocator繝代ち繝ｼ繝ｳ縺ｮ縺ｿ菴ｿ逕ｨ
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

