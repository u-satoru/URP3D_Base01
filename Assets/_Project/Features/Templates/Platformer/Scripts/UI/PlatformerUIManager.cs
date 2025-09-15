using UnityEngine;

namespace asterivo.Unity60.Features.Templates.Platformer.UI
{
    /// <summary>
    /// Placeholder for Platformer UI Manager.
    /// This class will handle UI elements like score, lives, etc.
    /// </summary>
    public class PlatformerUIManager : MonoBehaviour
    {
        public void InitializeUI(int lives, int initialScore, int targetScore)
        {
            Debug.LogWarning("PlatformerUIManager.InitializeUI is not yet implemented.");
        }

        public void ShowGameUI()
        {
            Debug.LogWarning("PlatformerUIManager.ShowGameUI is not yet implemented.");
        }

        public void AddScore(int points)
        {
            Debug.LogWarning($"PlatformerUIManager.AddScore({points}) is not yet implemented.");
        }

        public void LoseLife()
        {
            Debug.LogWarning("PlatformerUIManager.LoseLife is not yet implemented.");
        }
    }
}
