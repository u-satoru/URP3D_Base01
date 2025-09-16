using System;
using UnityEngine;

namespace asterivo.Unity60.Features.Templates.FPS.Events
{
    /// <summary>
    /// 移動関連データクラス
    /// Event駆動アーキテクチャ準拠
    /// </summary>
    [Serializable]
    public struct MovementData
    {
        [Header("Position & Velocity")]
        public Vector3 position;
        public Vector3 velocity;
        public Vector3 direction;

        [Header("Movement State")]
        public bool isGrounded;
        public bool isMoving;
        public bool isSprinting;
        public bool isCrouching;
        public bool isJumping;

        [Header("Speed Settings")]
        public float currentSpeed;
        public float walkSpeed;
        public float sprintSpeed;
        public float crouchSpeed;

        [Header("Physics")]
        public float jumpForce;
        public float gravity;
        public float groundDistance;

        [Header("Timing")]
        public float timestamp;
        public float deltaTime;

        [Header("Input")]
        public Vector2 inputVector;
        public bool jumpInput;
        public bool sprintInput;
        public bool crouchInput;

        /// <summary>
        /// MovementDataコンストラクタ
        /// </summary>
        public MovementData(Vector3 pos, Vector3 vel)
        {
            position = pos;
            velocity = vel;
            direction = vel.normalized;

            isGrounded = false;
            isMoving = vel.magnitude > 0.1f;
            isSprinting = false;
            isCrouching = false;
            isJumping = false;

            currentSpeed = vel.magnitude;
            walkSpeed = 5f;
            sprintSpeed = 8f;
            crouchSpeed = 2f;

            jumpForce = 10f;
            gravity = -9.81f;
            groundDistance = 0f;

            timestamp = Time.time;
            deltaTime = Time.deltaTime;

            inputVector = Vector2.zero;
            jumpInput = false;
            sprintInput = false;
            crouchInput = false;
        }

        /// <summary>
        /// デフォルトMovementDataを作成
        /// </summary>
        public static MovementData Default => new MovementData(Vector3.zero, Vector3.zero);

        /// <summary>
        /// 現在の移動状態を文字列として取得
        /// </summary>
        public string GetMovementState()
        {
            if (isJumping) return "Jumping";
            if (isCrouching) return "Crouching";
            if (isSprinting) return "Sprinting";
            if (isMoving) return "Walking";
            return "Idle";
        }

        /// <summary>
        /// 有効な移動データかどうか判定
        /// </summary>
        public bool IsValid()
        {
            return !float.IsNaN(position.x) && !float.IsNaN(position.y) && !float.IsNaN(position.z) &&
                   !float.IsNaN(velocity.x) && !float.IsNaN(velocity.y) && !float.IsNaN(velocity.z);
        }

        public override string ToString()
        {
            return $"MovementData[Pos:{position:F2}, Vel:{velocity:F2}, State:{GetMovementState()}, Grounded:{isGrounded}]";
        }
    }
}