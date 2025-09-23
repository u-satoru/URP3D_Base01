using UnityEngine;

namespace asterivo.Unity60.Features.Player.Events
{
    /// <summary>
    /// プレイヤーの覗き見アクションイベントデータ
    /// </summary>
    [System.Serializable]
    public class PlayerPeekEventData
    {
        /// <summary>
        /// 覗き見の方向
        /// </summary>
        public asterivo.Unity60.Features.Player.Commands.PeekDirection Direction { get; set; }

        /// <summary>
        /// 覗き見の強度（0.0～1.0）
        /// </summary>
        public float Intensity { get; set; }

        /// <summary>
        /// プレイヤーの現在位置
        /// </summary>
        public Vector3 PlayerPosition { get; set; }

        /// <summary>
        /// プレイヤーの向き
        /// </summary>
        public Quaternion PlayerRotation { get; set; }

        /// <summary>
        /// カバーポイントの位置（カバー中の場合）
        /// </summary>
        public Vector3? CoverPosition { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PlayerPeekEventData(
            asterivo.Unity60.Features.Player.Commands.PeekDirection direction,
            float intensity,
            Vector3 playerPosition,
            Quaternion playerRotation,
            Vector3? coverPosition = null)
        {
            Direction = direction;
            Intensity = intensity;
            PlayerPosition = playerPosition;
            PlayerRotation = playerRotation;
            CoverPosition = coverPosition;
        }

        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>
        public PlayerPeekEventData() { }
    }
}