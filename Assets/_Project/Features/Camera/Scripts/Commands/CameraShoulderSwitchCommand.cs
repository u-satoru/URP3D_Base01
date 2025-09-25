using UnityEngine;
using asterivo.Unity60.Core.Commands;
using asterivo.Unity60.Features.Camera.Commands.Definitions;
using asterivo.Unity60.Features.Camera.States;

namespace asterivo.Unity60.Features.Camera.Commands
{
    /// <summary>
    /// カメラの肩越しアングル切り替えコマンド
    /// 三人称視点での右肩・左肩・中央の切り替えを実行します
    /// </summary>
    public class CameraShoulderSwitchCommand : ICommand
    {
        private readonly CameraStateMachine cameraSystem;
        private readonly CameraShoulderSwitchCommandDefinition.ShoulderAngle targetAngle;
        private readonly Vector3 targetOffset;
        private readonly float transitionSpeed;
        private readonly bool smoothTransition;
        
        // Undo用の状態保存
        private Vector3 previousOffset;
        private CameraShoulderSwitchCommandDefinition.ShoulderAngle previousAngle;
        private bool wasExecuted = false;

        public bool CanUndo => true;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CameraShoulderSwitchCommand(
            CameraStateMachine cameraSystem,
            CameraShoulderSwitchCommandDefinition.ShoulderAngle targetAngle,
            Vector3 targetOffset,
            float transitionSpeed = 10f,
            bool smoothTransition = true)
        {
            this.cameraSystem = cameraSystem;
            this.targetAngle = targetAngle;
            this.targetOffset = targetOffset;
            this.transitionSpeed = transitionSpeed;
            this.smoothTransition = smoothTransition;
        }

        /// <summary>
        /// コマンドを実行します
        /// </summary>
        public void Execute()
        {
            if (cameraSystem == null) return;

            // 現在の状態を保存（Undo用）
            if (!wasExecuted)
            {
                SaveCurrentState();
                wasExecuted = true;
            }

            // 実際の角度を決定（Toggleの場合は現在の状態から次を決定）
            var actualTargetAngle = targetAngle;
            if (targetAngle == CameraShoulderSwitchCommandDefinition.ShoulderAngle.Toggle)
            {
                actualTargetAngle = DetermineNextAngle();
            }

            // 肩越しアングルを適用
            ApplyShoulderAngle(actualTargetAngle);

            Debug.Log($"カメラ肩越しアングルを {actualTargetAngle} に切り替えました");
        }

        /// <summary>
        /// コマンドを元に戻します
        /// </summary>
        public void Undo()
        {
            if (cameraSystem == null || !wasExecuted) return;

            // 以前の状態に戻す
            ApplyShoulderAngleDirectly(previousAngle, previousOffset);
            Debug.Log($"カメラ肩越しアングルを {previousAngle} に戻しました");
        }

        /// <summary>
        /// 現在の状態を保存します（Undo用）
        /// </summary>
        private void SaveCurrentState()
        {
            // ThirdPersonSettingsから現在のオフセットを取得
            if (cameraSystem.ThirdPersonSettings != null)
            {
                previousOffset = cameraSystem.ThirdPersonSettings.shoulderOffset;
                previousAngle = DetermineCurrentAngle(previousOffset);
            }
            else
            {
                previousOffset = Vector3.zero;
                previousAngle = CameraShoulderSwitchCommandDefinition.ShoulderAngle.Center;
            }
        }

        /// <summary>
        /// 現在のオフセットからアングルを判定します
        /// </summary>
        private CameraShoulderSwitchCommandDefinition.ShoulderAngle DetermineCurrentAngle(Vector3 offset)
        {
            if (offset.x > 0.1f)
                return CameraShoulderSwitchCommandDefinition.ShoulderAngle.RightShoulder;
            else if (offset.x < -0.1f)
                return CameraShoulderSwitchCommandDefinition.ShoulderAngle.LeftShoulder;
            else
                return CameraShoulderSwitchCommandDefinition.ShoulderAngle.Center;
        }

        /// <summary>
        /// 次のアングルを決定します（Toggle用）
        /// </summary>
        private CameraShoulderSwitchCommandDefinition.ShoulderAngle DetermineNextAngle()
        {
            var currentAngle = CameraShoulderSwitchCommandDefinition.ShoulderAngle.Center;
            
            if (cameraSystem.ThirdPersonSettings != null)
            {
                currentAngle = DetermineCurrentAngle(cameraSystem.ThirdPersonSettings.shoulderOffset);
            }

            // 次のアングルを決定
            switch (currentAngle)
            {
                case CameraShoulderSwitchCommandDefinition.ShoulderAngle.Center:
                    return CameraShoulderSwitchCommandDefinition.ShoulderAngle.RightShoulder;
                case CameraShoulderSwitchCommandDefinition.ShoulderAngle.RightShoulder:
                    return CameraShoulderSwitchCommandDefinition.ShoulderAngle.LeftShoulder;
                case CameraShoulderSwitchCommandDefinition.ShoulderAngle.LeftShoulder:
                    return CameraShoulderSwitchCommandDefinition.ShoulderAngle.Center;
                default:
                    return CameraShoulderSwitchCommandDefinition.ShoulderAngle.RightShoulder;
            }
        }

        /// <summary>
        /// 指定された肩越しアングルを適用します
        /// </summary>
        private void ApplyShoulderAngle(CameraShoulderSwitchCommandDefinition.ShoulderAngle angle)
        {
            Vector3 newOffset = GetOffsetForAngle(angle);
            ApplyShoulderAngleDirectly(angle, newOffset);
        }

        /// <summary>
        /// 肩越しアングルを直接適用します
        /// </summary>
        private void ApplyShoulderAngleDirectly(CameraShoulderSwitchCommandDefinition.ShoulderAngle angle, Vector3 offset)
        {
            if (cameraSystem.ThirdPersonSettings != null)
            {
                // NOTE: ThirdPersonSettingsがScriptableObjectの場合、
                // 実際の実装では専用のセッターメソッドが必要になる場合があります
                cameraSystem.ThirdPersonSettings.shoulderOffset = offset;
                
                // スムーズトランジションが有効な場合の処理
                if (smoothTransition)
                {
                    // ここでコルーチンやTweenライブラリを使用してスムーズな遷移を実装
                    // 現在の実装では即座に切り替え
                }
            }
        }

        /// <summary>
        /// アングルに対応するオフセットを取得します
        /// </summary>
        private Vector3 GetOffsetForAngle(CameraShoulderSwitchCommandDefinition.ShoulderAngle angle)
        {
            switch (angle)
            {
                case CameraShoulderSwitchCommandDefinition.ShoulderAngle.RightShoulder:
                    return new Vector3(0.8f, 1.5f, 0f);
                case CameraShoulderSwitchCommandDefinition.ShoulderAngle.LeftShoulder:
                    return new Vector3(-0.8f, 1.5f, 0f);
                case CameraShoulderSwitchCommandDefinition.ShoulderAngle.Center:
                    return new Vector3(0f, 1.5f, 0f);
                default:
                    return new Vector3(0f, 1.5f, 0f);
            }
        }
    }
}
