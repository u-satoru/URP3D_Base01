using UnityEngine;
using asterivo.Unity60.Core.Events;

namespace asterivo.Unity60.Features.Templates.Common
{
    /// <summary>
    /// GenreType値を伝達するイベントチャネル
    /// ゲームジャンルの切り替え、テンプレート状態変更の通知に使用
    ///
    /// DESIGN.md アーキテクチャ分離原則に準拠:
    /// - Feature層でのCore基盤活用
    /// - GenericGameEvent<T>ベースの型安全なイベント通信
    /// - ScriptableObjectによるデータ駆動設計
    /// </summary>
    [CreateAssetMenu(fileName = "New GenreType Event", menuName = "asterivo.Unity60/Templates/GenreType Event")]
    public class GenreTypeGameEvent : GameEvent<GenreType> { }
}