using asterivo.Unity60.Core.Audio; // GameState enum用

namespace asterivo.Unity60.Core.Services
{
    public interface IGameStateManager
    {
        GameState CurrentGameState { get; }
        GameState PreviousGameState { get; }
        bool IsGameOver { get; }

        void ChangeGameState(GameState newState);
    }
}
