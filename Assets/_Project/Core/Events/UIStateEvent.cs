using UnityEngine;
// using asterivo.Unity60.Core.UI; // UI moved to Features

namespace asterivo.Unity60.Core.Events
{
    /// <summary>
    /// UI状態変更イベント。UIパネルの表示・非表示やタイプ情報を含むイベントです。
    /// </summary>
    [CreateAssetMenu(fileName = "New UI State Event", menuName = "Game Events/UI State Event")]
    public class UIStateEvent : GenericGameEvent<object> // Changed from UIStateData to object to avoid circular dependency
    {
    }
    
    /// <summary>
    /// UI状態データを格納する構造体（一時的にコメントアウト - 型参照問題のため）
    /// </summary>
    /*
    [System.Serializable]
    public struct UIStateData
    {
        [Tooltip("パネル名")]
        public string panelName;
        
        [Tooltip("パネルタイプ")]
        public UIManager.UIPanelType panelType;
        
        [Tooltip("表示状態")]
        public bool isVisible;
        
        [Tooltip("追加のメタデータ")]
        public string metadata;
        
        /// <summary>
        /// デバッグ用の文字列表現
        /// </summary>
        public override string ToString()
        {
            return $"UIState: {panelName} ({panelType}) - Visible: {isVisible}";
        }
    }
    */
}