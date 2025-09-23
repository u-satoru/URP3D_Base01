using UnityEngine;
using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Core.Audio; // GameState enum用
// using asterivo.Unity60.Core.Lifecycle;

namespace asterivo.Unity60.Core
{
    /// <summary>
    /// GameManagerのシーンロード�E琁E��委譲するサービス実裁E    /// </summary>
    public class SceneLoadingService : MonoBehaviour, ISceneLoadingService, IServiceLocatorRegistrable
    {
        [Header("Scenes")] [SerializeField] private string gameplaySceneName = "Gameplay";
        [Header("Settings")] [SerializeField] private float minLoadingTime = 1f;
        [Header("Runtime")] [SerializeField] private bool isTransitioning = false;
        // GameManager reference removed - Core層からFeatures層への参�E禁止
        [SerializeField] private int priority = 60;

        public int Priority => priority;

        private void Reset()
        {
            // GameManager fallback removed - ServiceLocatorパターンのみ使用
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
