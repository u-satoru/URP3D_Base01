using UnityEngine;
using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Core.Events;
// using asterivo.Unity60.Core.Lifecycle;

namespace asterivo.Unity60.Core
{
    /// <summary>
    /// 繧ｹ繧ｳ繧｢/繝ｩ繧､繝輔・迢ｬ遶九し繝ｼ繝薙せ螳溯｣・ｼ医う繝吶Φ繝育匱轣ｫ蜷ｫ繧・・    /// </summary>
    public class ScoreService : MonoBehaviour, IScoreService, IServiceLocatorRegistrable
    {
        [Header("References")]
        [SerializeField] private IntGameEvent onScoreChanged;
        [SerializeField] private IntGameEvent onLivesChanged;
        [SerializeField] private GameEvent onGameOver;

        [Header("Settings")] 
        [SerializeField] private int initialLives = 3;
        [SerializeField] private int maxLives = 5;
        [SerializeField] private bool enableDebugLog = true;

        [Header("Runtime")]
        [SerializeField] private int currentScore;
        [SerializeField] private int currentLives;

        [SerializeField] private int priority = 70;
        public int Priority => priority;

        private void Awake()
        {
            currentLives = Mathf.Clamp(initialLives, 0, maxLives);
        }

        public void RegisterServices()
        {
            if (FeatureFlags.UseServiceLocator)
            {
                ServiceLocator.RegisterService<IScoreService>(this);
            }
        }

        public void UnregisterServices()
        {
            if (FeatureFlags.UseServiceLocator)
            {
                ServiceLocator.UnregisterService<IScoreService>();
            }
        }

        public int CurrentScore => currentScore;
        public int CurrentLives => currentLives;

        public void AddScore(int points)
        {
            if (points <= 0) return;
            currentScore += points;
            onScoreChanged?.Raise(currentScore);
            if (enableDebugLog) UnityEngine.Debug.Log($"[ScoreService] +{points} (total: {currentScore})");
        }

        public void SetScore(int newScore)
        {
            currentScore = Mathf.Max(0, newScore);
            onScoreChanged?.Raise(currentScore);
            if (enableDebugLog) UnityEngine.Debug.Log($"[ScoreService] score set: {currentScore}");
        }

        public void LoseLife()
        {
            if (currentLives <= 0) return;
            currentLives--;
            onLivesChanged?.Raise(currentLives);
            if (enableDebugLog) UnityEngine.Debug.Log($"[ScoreService] life -1 (lives: {currentLives})");
            if (currentLives <= 0)
            {
                TriggerGameOver();
            }
        }

        public void AddLife()
        {
            if (currentLives >= maxLives) return;
            currentLives++;
            onLivesChanged?.Raise(currentLives);
            if (enableDebugLog) UnityEngine.Debug.Log($"[ScoreService] life +1 (lives: {currentLives})");
        }

        public void SetLives(int lives)
        {
            currentLives = Mathf.Clamp(lives, 0, maxLives);
            onLivesChanged?.Raise(currentLives);
            if (enableDebugLog) UnityEngine.Debug.Log($"[ScoreService] lives set: {currentLives}");
            if (currentLives <= 0)
            {
                TriggerGameOver();
            }
        }

        private void TriggerGameOver()
        {
            if (enableDebugLog) UnityEngine.Debug.Log("[ScoreService] Game Over");
            onGameOver?.Raise();
        }
    }
}

