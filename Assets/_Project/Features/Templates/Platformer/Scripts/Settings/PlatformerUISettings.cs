using UnityEngine;

namespace asterivo.Unity60.Features.Templates.Platformer.Settings
{
    /// <summary>
    /// PlatformerUI設定：ユーザーインターフェース・メニュー・HUD管理
    /// </summary>
    [CreateAssetMenu(fileName = "PlatformerUISettings", menuName = "Platformer Template/Settings/UI Settings")]
    public class PlatformerUISettings : ScriptableObject
    {
        [Header("UI Settings")]
        [SerializeField] private bool _showHealthBar = true;
        [SerializeField] private bool _showScore = true;
        [SerializeField] private bool _showTimer = false;

        public bool ShowHealthBar => _showHealthBar;
        public bool ShowScore => _showScore;
        public bool ShowTimer => _showTimer;

        public void SetToDefault()
        {
            _showHealthBar = true;
            _showScore = true;
            _showTimer = false;
        }
    }
}
