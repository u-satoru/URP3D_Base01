using UnityEngine;
using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Core.Audio; // GameState enum逕ｨ
// using asterivo.Unity60.Core.Lifecycle;

namespace asterivo.Unity60.Core
{
    /// <summary>
    /// GameManager縺ｮ繧ｷ繝ｼ繝ｳ繝ｭ繝ｼ繝牙・逅・ｒ蟋碑ｭｲ縺吶ｋ繧ｵ繝ｼ繝薙せ螳溯｣・    /// </summary>
    public class SceneLoadingService : MonoBehaviour, ISceneLoadingService, IServiceLocatorRegistrable
    {
        [Header("Scenes")] [SerializeField] private string gameplaySceneName = "Gameplay";
        [Header("Settings")] [SerializeField] private float minLoadingTime = 1f;
        [Header("Runtime")] [SerializeField] private bool isTransitioning = false;
        // GameManager reference removed - Core螻､縺九ｉFeatures螻､縺ｸ縺ｮ蜿ら・遖∵ｭ｢
        [SerializeField] private int priority = 60;

        public int Priority => priority;

        private void Reset()
        {
            // GameManager fallback removed - ServiceLocator繝代ち繝ｼ繝ｳ縺ｮ縺ｿ菴ｿ逕ｨ
        }

        public void RegisterServices()
        {
            if (FeatureFlags.UseServiceLocator)
            {
                ServiceLocator.RegisterService<ISceneLoadingService>(this);
            }
        }

        public void UnregisterServices()
        {
            if (FeatureFlags.UseServiceLocator)
            {
                ServiceLocator.UnregisterService<ISceneLoadingService>();
            }
        }

        public void LoadGameplaySceneWithMinTime() => LoadSceneWithMinTime(gameplaySceneName);

        public void LoadSceneWithMinTime(string sceneName)
        {
            if (!string.IsNullOrEmpty(sceneName))
            {
                StartCoroutine(LoadSceneRoutine(sceneName));
            }
        }

        private System.Collections.IEnumerator LoadSceneRoutine(string sceneName)
        {
            var gsm = ServiceLocator.GetService<IGameStateManager>();
            isTransitioning = true;
            gsm?.ChangeGameState(GameState.Loading);

            float startTime = Time.realtimeSinceStartup;
            var asyncOp = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);

            while (Time.realtimeSinceStartup - startTime < minLoadingTime || (asyncOp != null && !asyncOp.isDone))
            {
                yield return null;
            }

            isTransitioning = false;
            gsm?.ChangeGameState(GameState.Playing);
        }
    }
}
