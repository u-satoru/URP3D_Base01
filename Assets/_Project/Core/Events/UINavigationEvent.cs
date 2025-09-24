using UnityEngine;

namespace asterivo.Unity60.Core.Events
{
    /// <summary>
    /// UIナビゲーションイベント。メニュー間の遷移や戻る操作を管理します。
    /// </summary>
    [CreateAssetMenu(fileName = "New UI Navigation Event", menuName = "Game Events/UI Navigation Event")]
    public class UINavigationEvent : GameEvent<UINavigationData>
    {
    }
    
    /// <summary>
    /// UIナビゲーションデータを格納する構造体
    /// </summary>
    [System.Serializable]
    public struct UINavigationData
    {
        [Tooltip("遷移元のパネル名")]
        public string fromPanel;
        
        [Tooltip("遷移先のパネル名")]
        public string toPanel;
        
        [Tooltip("ナビゲーションタイプ")]
        public NavigationType navigationType;
        
        [Tooltip("アニメーション時間")]
        public float animationDuration;
        
        /// <summary>
        /// ナビゲーションの種類
        /// </summary>
        public enum NavigationType
        {
            Forward,    // 前進
            Back,       // 戻る
            Replace,    // 置き換え
            Modal       // モーダル表示
        }
        
        /// <summary>
        /// デバッグ用の文字列表現
        /// </summary>
        public override string ToString()
        {
            return $"Navigation: {fromPanel} → {toPanel} ({navigationType})";
        }
    }
}
