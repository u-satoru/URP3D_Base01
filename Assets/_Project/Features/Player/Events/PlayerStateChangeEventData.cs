using UnityEngine;

namespace asterivo.Unity60.Features.Player.Events
{
    /// <summary>
    /// プレイヤーの状態変更イベントデータ
    /// </summary>
    [System.Serializable]
    public class PlayerStateChangeEventData
    {
        /// <summary>
        /// 変更前の状態名
        /// </summary>
        public string OldStateName { get; set; }

        /// <summary>
        /// 変更後の状態名
        /// </summary>
        public string NewStateName { get; set; }

        /// <summary>
        /// 状態変更時のプレイヤー位置
        /// </summary>
        public Vector3 PlayerPosition { get; set; }

        /// <summary>
        /// 状態変更の理由（オプション）
        /// </summary>
        public string ChangeReason { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PlayerStateChangeEventData(
            string oldStateName,
            string newStateName,
            Vector3 playerPosition,
            string changeReason = null)
        {
            OldStateName = oldStateName;
            NewStateName = newStateName;
            PlayerPosition = playerPosition;
            ChangeReason = changeReason;
        }

        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>
        public PlayerStateChangeEventData() { }
    }
}
