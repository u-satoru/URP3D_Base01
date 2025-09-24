namespace asterivo.Unity60.Core.Services
{
    public interface IPauseService
    {
        bool IsPaused { get; }
        void TogglePause();
        void SetPauseState(bool paused);
        void ResumeGame();
    }
}

