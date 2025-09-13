using UnityEngine;
using asterivo.Unity60.Core.Commands;
using asterivo.Unity60.Features.Camera.Commands.Definitions;
using asterivo.Unity60.Camera.States;

namespace asterivo.Unity60.Features.Camera
{
    /// <summary>
    /// カメラ関連のコマンド管理とサンプル実装
    /// 肩越しアングル切り替えのデモンストレーションを含みます
    /// </summary>
    public class CameraCommandManager : MonoBehaviour
    {
        [Header("Camera System")]
        [SerializeField] private CameraStateMachine cameraStateMachine;
        
        [Header("Command System")]
        [SerializeField] private CommandInvoker commandInvoker;
        
        [Header("Shoulder Switch Definitions")]
        [SerializeField] private CameraShoulderSwitchCommandDefinition toggleShoulderDefinition;
        [SerializeField] private CameraShoulderSwitchCommandDefinition rightShoulderDefinition;
        [SerializeField] private CameraShoulderSwitchCommandDefinition leftShoulderDefinition;
        [SerializeField] private CameraShoulderSwitchCommandDefinition centerDefinition;
        
        [Header("Input Settings")]
        [SerializeField] private KeyCode toggleShoulderKey = KeyCode.Q;
        [SerializeField] private KeyCode rightShoulderKey = KeyCode.E;
        [SerializeField] private KeyCode leftShoulderKey = KeyCode.R;
        [SerializeField] private KeyCode centerKey = KeyCode.T;

        private void Awake()
        {
            InitializeComponents();
            InitializeCommandDefinitions();
        }

        private void InitializeComponents()
        {
            // カメラシステムを自動検出
            if (cameraStateMachine == null)
                cameraStateMachine = FindFirstObjectByType<CameraStateMachine>();
            
            // コマンドInvokerを自動検出
            if (commandInvoker == null)
                commandInvoker = FindFirstObjectByType<CommandInvoker>();
            
            if (cameraStateMachine == null)
                Debug.LogError("CameraCommandManager: CameraStateMachine が見つかりません");
            
            if (commandInvoker == null)
                Debug.LogError("CameraCommandManager: CommandInvoker が見つかりません");
        }

        private void InitializeCommandDefinitions()
        {
            // トグル用定義を初期化
            if (toggleShoulderDefinition == null)
            {
                toggleShoulderDefinition = new CameraShoulderSwitchCommandDefinition(
                    CameraShoulderSwitchCommandDefinition.ShoulderAngle.Toggle);
            }

            // 右肩用定義を初期化
            if (rightShoulderDefinition == null)
            {
                rightShoulderDefinition = new CameraShoulderSwitchCommandDefinition(
                    CameraShoulderSwitchCommandDefinition.ShoulderAngle.RightShoulder);
            }

            // 左肩用定義を初期化
            if (leftShoulderDefinition == null)
            {
                leftShoulderDefinition = new CameraShoulderSwitchCommandDefinition(
                    CameraShoulderSwitchCommandDefinition.ShoulderAngle.LeftShoulder);
            }

            // 中央用定義を初期化
            if (centerDefinition == null)
            {
                centerDefinition = new CameraShoulderSwitchCommandDefinition(
                    CameraShoulderSwitchCommandDefinition.ShoulderAngle.Center);
            }
        }

        private void Update()
        {
            HandleInput();
        }

        private void HandleInput()
        {
            if (cameraStateMachine == null || commandInvoker == null) return;

            // トグル切り替え
            if (Input.GetKeyDown(toggleShoulderKey))
            {
                ExecuteShoulderSwitchCommand(toggleShoulderDefinition);
            }

            // 直接指定での切り替え
            if (Input.GetKeyDown(rightShoulderKey))
            {
                ExecuteShoulderSwitchCommand(rightShoulderDefinition);
            }

            if (Input.GetKeyDown(leftShoulderKey))
            {
                ExecuteShoulderSwitchCommand(leftShoulderDefinition);
            }

            if (Input.GetKeyDown(centerKey))
            {
                ExecuteShoulderSwitchCommand(centerDefinition);
            }
        }

        /// <summary>
        /// 肩越し切り替えコマンドを実行します
        /// </summary>
        private void ExecuteShoulderSwitchCommand(CameraShoulderSwitchCommandDefinition definition)
        {
            if (definition == null || !definition.CanExecute(cameraStateMachine))
            {
                Debug.LogWarning("肩越し切り替えコマンドを実行できません（カメラ状態を確認してください）");
                return;
            }

            var command = definition.CreateCommand(cameraStateMachine);
            if (command != null)
            {
                commandInvoker.ExecuteCommand(command);
            }
        }

        #region Public API Methods

        /// <summary>
        /// プログラムから肩越しアングルをトグル切り替えします
        /// </summary>
        public void ToggleShoulderAngle()
        {
            ExecuteShoulderSwitchCommand(toggleShoulderDefinition);
        }

        /// <summary>
        /// プログラムから右肩越しに切り替えします
        /// </summary>
        public void SwitchToRightShoulder()
        {
            ExecuteShoulderSwitchCommand(rightShoulderDefinition);
        }

        /// <summary>
        /// プログラムから左肩越しに切り替えします
        /// </summary>
        public void SwitchToLeftShoulder()
        {
            ExecuteShoulderSwitchCommand(leftShoulderDefinition);
        }

        /// <summary>
        /// プログラムから中央視点に切り替えします
        /// </summary>
        public void SwitchToCenter()
        {
            ExecuteShoulderSwitchCommand(centerDefinition);
        }

        /// <summary>
        /// カスタムの肩越し切り替えコマンドを実行します
        /// </summary>
        /// <param name="angle">切り替え先のアングル</param>
        /// <param name="customOffset">カスタムオフセット（オプション）</param>
        public void SwitchToCustomAngle(CameraShoulderSwitchCommandDefinition.ShoulderAngle angle, Vector3? customOffset = null)
        {
            var customDefinition = new CameraShoulderSwitchCommandDefinition(angle);
            
            if (customOffset.HasValue)
            {
                switch (angle)
                {
                    case CameraShoulderSwitchCommandDefinition.ShoulderAngle.RightShoulder:
                        customDefinition.rightShoulderOffset = customOffset.Value;
                        break;
                    case CameraShoulderSwitchCommandDefinition.ShoulderAngle.LeftShoulder:
                        customDefinition.leftShoulderOffset = customOffset.Value;
                        break;
                    case CameraShoulderSwitchCommandDefinition.ShoulderAngle.Center:
                        customDefinition.centerOffset = customOffset.Value;
                        break;
                }
            }

            ExecuteShoulderSwitchCommand(customDefinition);
        }

        #endregion

        #region Debug Methods

        [ContextMenu("Debug: Toggle Shoulder")]
        private void DebugToggleShoulder()
        {
            ToggleShoulderAngle();
        }

        [ContextMenu("Debug: Right Shoulder")]
        private void DebugRightShoulder()
        {
            SwitchToRightShoulder();
        }

        [ContextMenu("Debug: Left Shoulder")]
        private void DebugLeftShoulder()
        {
            SwitchToLeftShoulder();
        }

        [ContextMenu("Debug: Center")]
        private void DebugCenter()
        {
            SwitchToCenter();
        }

        #endregion
    }
}