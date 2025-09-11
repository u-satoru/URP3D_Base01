using UnityEngine;
using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Core.Audio; // GameState enum用

namespace asterivo.Unity60.Core
{
    /// <summary>
    /// GameManagerのシーンロード処理を委譲するサービス実装
    /// </summary>
    public class SceneLoadingService : MonoBehaviour, ISceneLoadingService, _Project.Core.IServiceLocatorRegistrable
    {
        [Header("Scenes")] [SerializeField] private string gameplaySceneName = "Gameplay";
        [Header("Settings")] [SerializeField] private float minLoadingTime = 1f;
        [Header("Runtime")] [SerializeField] private bool isTransitioning = false;
        [SerializeField] private GameManager gameManager; // optional fallback
        [SerializeField] private int priority = 60;

        public int Priority => priority;

        private void Reset()
        {
            if (gameManager == null) gameManager = FindFirstObjectByType<GameManager>();
        }

        public void RegisterServices()
        {
            if (CoreFeatureFlags.UseServiceLocator)
            {
                ServiceLocator.RegisterService<ISceneLoadingService>(this);
            }
        }

        public void UnregisterServices()
        {
            if (CoreFeatureFlags.UseServiceLocator)
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
