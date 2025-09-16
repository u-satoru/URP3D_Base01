using UnityEngine;

namespace asterivo.Unity60.Features.Templates.Platformer.Settings
{
    /// <summary>
    /// Platformer入力設定：入力処理・マッピング・コントローラー対応
    /// </summary>
    [CreateAssetMenu(fileName = "PlatformerInputSettings", menuName = "Platformer Template/Settings/Input Settings")]
    public class PlatformerInputSettings : ScriptableObject
    {
        [Header("Input Sensitivity")]
        [SerializeField, Range(0.1f, 5f)] private float _horizontalSensitivity = 1f;
        [SerializeField, Range(0.1f, 5f)] private float _verticalSensitivity = 1f;

        [Header("Controller Settings")]
        [SerializeField] private bool _enableController = true;
        [SerializeField, Range(0.01f, 1f)] private float _deadZone = 0.2f;

        public float HorizontalSensitivity => _horizontalSensitivity;
        public float VerticalSensitivity => _verticalSensitivity;
        public bool EnableController => _enableController;
        public float DeadZone => _deadZone;

        public void SetToDefault()
        {
            _horizontalSensitivity = 1f;
            _verticalSensitivity = 1f;
            _enableController = true;
            _deadZone = 0.2f;
        }
    }
}