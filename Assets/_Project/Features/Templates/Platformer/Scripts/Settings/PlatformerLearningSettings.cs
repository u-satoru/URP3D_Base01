using UnityEngine;

namespace asterivo.Unity60.Features.Templates.Platformer.Settings
{
    /// <summary>
    /// Platformer学習システム設定：Learn & Grow価値実現（70%学習コスト削減）
    /// </summary>
    [CreateAssetMenu(fileName = "PlatformerLearningSettings", menuName = "Platformer Template/Settings/Learning Settings")]
    public class PlatformerLearningSettings : ScriptableObject
    {
        [Header("Learning Features")]
        [SerializeField] private bool _enableTutorial = true;
        [SerializeField] private bool _showHints = true;
        [SerializeField] private bool _progressTracking = true;
        [SerializeField, Range(0.1f, 3f)] private float _learningSpeed = 1f;

        public bool EnableTutorial => _enableTutorial;
        public bool ShowHints => _showHints;
        public bool ProgressTracking => _progressTracking;
        public float LearningSpeed => _learningSpeed;

        public void SetToDefault()
        {
            _enableTutorial = true;
            _showHints = true;
            _progressTracking = true;
            _learningSpeed = 1f;
        }

        public void EnableAllLearningFeatures()
        {
            _enableTutorial = true;
            _showHints = true;
            _progressTracking = true;
            _learningSpeed = 1.2f;
        }
    }
}
