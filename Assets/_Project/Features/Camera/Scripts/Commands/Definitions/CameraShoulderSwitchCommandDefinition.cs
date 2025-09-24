using UnityEngine;
using asterivo.Unity60.Core.Commands;

namespace asterivo.Unity60.Features.Camera.Commands.Definitions
{
    /// <summary>
    /// カメラの肩越しアングル切り替えコマンドの定義
    /// 右肩・左肩・中央の切り替えを管理します
    /// </summary>
    [System.Serializable]
    public class CameraShoulderSwitchCommandDefinition : ICommandDefinition
    {
        /// <summary>
        /// 肩越しアングルの種類
        /// </summary>
        public enum ShoulderAngle
        {
            Center,     // 中央（通常の三人称視点）
            RightShoulder,  // 右肩越し
            LeftShoulder,   // 左肩越し
            Toggle      // 現在の設定から切り替え（右→左→中央→右...）
        }

        [Header("Shoulder Settings")]
        [Tooltip("切り替える肩越しアングルの種類")]
        public ShoulderAngle targetAngle = ShoulderAngle.Toggle;
        
        [Header("Offset Configuration")]
        [Tooltip("右肩越しの時のオフセット")]
        public Vector3 rightShoulderOffset = new Vector3(0.8f, 1.5f, 0f);
        
        [Tooltip("左肩越しの時のオフセット")]
        public Vector3 leftShoulderOffset = new Vector3(-0.8f, 1.5f, 0f);
        
        [Tooltip("中央の時のオフセット")]
        public Vector3 centerOffset = new Vector3(0f, 1.5f, 0f);
        
        [Header("Transition Settings")]
        [Tooltip("肩越し切り替え時の遷移速度")]
        [Range(1f, 20f)]
        public float transitionSpeed = 10f;
        
        [Tooltip("切り替え時にスムーズな遷移を行うか")]
        public bool smoothTransition = true;

        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>
        public CameraShoulderSwitchCommandDefinition()
        {
        }

        /// <summary>
        /// パラメータ付きコンストラクタ
        /// </summary>
        public CameraShoulderSwitchCommandDefinition(ShoulderAngle angle)
        {
            targetAngle = angle;
        }

        /// <summary>
        /// コマンドが実行可能かどうかを判定します
        /// </summary>
        public bool CanExecute(object context = null)
        {
            // カメラシステムが存在することを確認
            if (context is asterivo.Unity60.Features.Camera.States.CameraStateMachine cameraSystem)
            {
                // 三人称視点またはエイム状態の時のみ実行可能
                var currentState = cameraSystem.GetCurrentStateType();
                return currentState == asterivo.Unity60.Features.Camera.States.CameraStateMachine.CameraStateType.ThirdPerson ||
                       currentState == asterivo.Unity60.Features.Camera.States.CameraStateMachine.CameraStateType.Aim;
            }

            return false;
        }

        /// <summary>
        /// 肩越しアングル切り替えコマンドのインスタンスを作成します
        /// </summary>
        public ICommand CreateCommand(object context = null)
        {
            if (!CanExecute(context))
                return null;
                
            if (context is asterivo.Unity60.Features.Camera.States.CameraStateMachine cameraSystem)
            {
                return new CameraShoulderSwitchCommand(
                    cameraSystem, 
                    targetAngle, 
                    GetOffsetForAngle(targetAngle),
                    transitionSpeed,
                    smoothTransition
                );
            }
            
            return null;
        }

        /// <summary>
        /// 指定されたアングルに対応するオフセット値を取得します
        /// </summary>
        public Vector3 GetOffsetForAngle(ShoulderAngle angle)
        {
            switch (angle)
            {
                case ShoulderAngle.RightShoulder:
                    return rightShoulderOffset;
                case ShoulderAngle.LeftShoulder:
                    return leftShoulderOffset;
                case ShoulderAngle.Center:
                    return centerOffset;
                case ShoulderAngle.Toggle:
                    // Toggleの場合は動的に決定されるため、ここでは中央を返す
                    return centerOffset;
                default:
                    return centerOffset;
            }
        }

        /// <summary>
        /// 現在のアングルから次のアングルを決定します（Toggle用）
        /// </summary>
        public ShoulderAngle GetNextAngle(ShoulderAngle currentAngle)
        {
            switch (currentAngle)
            {
                case ShoulderAngle.Center:
                    return ShoulderAngle.RightShoulder;
                case ShoulderAngle.RightShoulder:
                    return ShoulderAngle.LeftShoulder;
                case ShoulderAngle.LeftShoulder:
                    return ShoulderAngle.Center;
                default:
                    return ShoulderAngle.RightShoulder;
            }
        }
    }
}
