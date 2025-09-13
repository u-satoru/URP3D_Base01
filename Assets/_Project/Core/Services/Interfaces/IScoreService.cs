namespace asterivo.Unity60.Core.Services
{
    public interface IScoreService
    {
        int CurrentScore { get; }
        int CurrentLives { get; }
        void AddScore(int points);
        void SetScore(int newScore);
        void LoseLife();
        void AddLife();
        void SetLives(int lives);
    }
}

