using UnityEngine;

namespace asterivo.Unity60.Features.Templates.Adventure.Interaction
{
    /// <summary>
    /// インタラクション可能オブジェクトの基本インターフェース
    /// Adventure Templateでのプレイヤーとの相互作用を定義
    /// </summary>
    public interface IInteractable
    {
        /// <summary>
        /// インタラクション可能かどうかを判定
        /// </summary>
        /// <returns>インタラクション可能な場合true</returns>
        bool CanInteract();

        /// <summary>
        /// インタラクションを実行
        /// </summary>
        /// <returns>インタラクションが成功した場合true</returns>
        bool Interact();

        /// <summary>
        /// インタラクション時に表示するテキストを取得
        /// </summary>
        /// <returns>表示テキスト</returns>
        string GetInteractionText();

        /// <summary>
        /// インタラクションの優先度を取得
        /// </summary>
        /// <returns>優先度（高いほど優先）</returns>
        int GetInteractionPriority();

        /// <summary>
        /// プレイヤーがこのオブジェクトをターゲットした時の処理
        /// </summary>
        void OnTargetSelected();

        /// <summary>
        /// プレイヤーがこのオブジェクトのターゲットを外した時の処理
        /// </summary>
        void OnTargetLost();

        /// <summary>
        /// インタラクション範囲を取得
        /// </summary>
        /// <returns>インタラクション可能な範囲</returns>
        float GetInteractionRange();

        /// <summary>
        /// インタラクションに必要なアイテムがあるかチェック
        /// </summary>
        /// <returns>必要なアイテムがある場合true</returns>
        bool HasRequiredItems();

        /// <summary>
        /// インタラクション後にオブジェクトが無効になるかどうか
        /// </summary>
        /// <returns>無効になる場合true</returns>
        bool IsConsumedOnInteraction();
    }
}