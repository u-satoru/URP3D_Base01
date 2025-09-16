using UnityEngine;
using asterivo.Unity60.Core.Events;

namespace asterivo.Unity60.Features.Templates.FPS.Events
{
    /// <summary>
    /// 移動イベントクラス
    /// Core層GameEvent<T>を継承したEvent駆動アーキテクチャ準拠実装
    /// </summary>
    [CreateAssetMenu(menuName = "FPS Template/Events/Movement Event", fileName = "MovementEvent")]
    public class MovementEvent : GameEvent<MovementData>
    {
        [Header("Movement Event Configuration")]
        [TextArea(3, 5)]
        public string eventDescription = "移動関連のイベント通知用GameEvent";

        [Header("Debug Settings")]
        public bool logMovementEvents = false;
        public bool showPositionInLog = true;
        public bool showVelocityInLog = true;

        /// <summary>
        /// 移動データでイベントを発行
        /// </summary>
        public override void Raise(MovementData movementData)
        {
            // デバッグログ出力
            if (logMovementEvents && Application.isPlaying)
            {
                LogMovementEvent(movementData);
            }

            // ベースクラスのRaiseを呼び出し
            base.Raise(movementData);
        }

        /// <summary>
        /// 移動開始イベント発行用ヘルパー
        /// </summary>
        public void RaiseMovementStarted(Vector3 position, Vector3 velocity)
        {
            var data = new MovementData(position, velocity)
            {
                isMoving = true,
                timestamp = Time.time,
                deltaTime = Time.deltaTime
            };
            Raise(data);
        }

        /// <summary>
        /// 移動停止イベント発行用ヘルパー
        /// </summary>
        public void RaiseMovementStopped(Vector3 position)
        {
            var data = new MovementData(position, Vector3.zero)
            {
                isMoving = false,
                timestamp = Time.time,
                deltaTime = Time.deltaTime
            };
            Raise(data);
        }

        /// <summary>
        /// ジャンプイベント発行用ヘルパー
        /// </summary>
        public void RaiseJump(Vector3 position, float jumpForce)
        {
            var data = new MovementData(position, Vector3.up * jumpForce)
            {
                isJumping = true,
                jumpInput = true,
                jumpForce = jumpForce,
                timestamp = Time.time,
                deltaTime = Time.deltaTime
            };
            Raise(data);
        }

        /// <summary>
        /// 着地イベント発行用ヘルパー
        /// </summary>
        public void RaiseLanding(Vector3 position, float groundDistance)
        {
            var data = new MovementData(position, Vector3.zero)
            {
                isGrounded = true,
                isJumping = false,
                groundDistance = groundDistance,
                timestamp = Time.time,
                deltaTime = Time.deltaTime
            };
            Raise(data);
        }

        /// <summary>
        /// スプリント開始イベント発行用ヘルパー
        /// </summary>
        public void RaiseSprintStarted(Vector3 position, Vector3 velocity, float sprintSpeed)
        {
            var data = new MovementData(position, velocity)
            {
                isSprinting = true,
                sprintInput = true,
                sprintSpeed = sprintSpeed,
                currentSpeed = sprintSpeed,
                timestamp = Time.time,
                deltaTime = Time.deltaTime
            };
            Raise(data);
        }

        /// <summary>
        /// しゃがみ開始イベント発行用ヘルパー
        /// </summary>
        public void RaiseCrouchStarted(Vector3 position, Vector3 velocity, float crouchSpeed)
        {
            var data = new MovementData(position, velocity)
            {
                isCrouching = true,
                crouchInput = true,
                crouchSpeed = crouchSpeed,
                currentSpeed = crouchSpeed,
                timestamp = Time.time,
                deltaTime = Time.deltaTime
            };
            Raise(data);
        }

        /// <summary>
        /// 移動イベントのデバッグログ出力
        /// </summary>
        private void LogMovementEvent(MovementData data)
        {
            string logMessage = $"[MovementEvent] {name}: {data.GetMovementState()}";

            if (showPositionInLog)
                logMessage += $" | Pos: {data.position:F2}";

            if (showVelocityInLog)
                logMessage += $" | Vel: {data.velocity:F2}";

            Debug.Log(logMessage);
        }

        #if UNITY_EDITOR
        [UnityEditor.MenuItem("GameObject/FPS Template/Create Movement Event Listener", false, 10)]
        private static void CreateMovementEventListener()
        {
            var go = new GameObject("MovementEvent Listener");
            go.AddComponent<asterivo.Unity60.Core.Events.GameEventListener<MovementData>>();
            UnityEditor.Selection.activeGameObject = go;
        }
        #endif
    }
}