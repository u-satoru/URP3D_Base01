using UnityEngine;
using asterivo.Unity60.Core.Events;

namespace asterivo.Unity60.Features.Templates.Adventure.Inventory
{
    /// <summary>
    /// AdventureItemData値を伝達するイベントチャネル
    /// アドベンチャーテンプレート用アイテムデータの変更、取得、使用通知に使用
    ///
    /// DESIGN.md アーキテクチャ分離原則に準拠:
    /// - Feature層でのCore基盤活用
    /// - GameEvent<T>ベースの型安全なイベント通信
    /// - ScriptableObjectによるデータ駆動設計
    /// </summary>
    [CreateAssetMenu(fileName = "New AdventureItemData Event", menuName = "asterivo.Unity60/Templates/Adventure/AdventureItemData Event")]
    public class AdventureItemDataGameEvent : GameEvent<AdventureItemData> { }
}