using UnityEngine;
using asterivo.Unity60.Core.Events;

namespace asterivo.Unity60.Features.Templates.Adventure.Quest
{
    /// <summary>
    /// QuestData値を伝達するイベントチャネル
    /// アドベンチャーテンプレート用クエストデータの開始、完了、失敗通知に使用
    ///
    /// DESIGN.md アーキテクチャ分離原則に準拠:
    /// - Feature層でのCore基盤活用
    /// - GenericGameEvent<T>ベースの型安全なイベント通信
    /// - ScriptableObjectによるデータ駆動設計
    /// </summary>
    [CreateAssetMenu(fileName = "New QuestData Event", menuName = "asterivo.Unity60/Templates/Adventure/QuestData Event")]
    public class QuestDataGameEvent : GenericGameEvent<QuestData> { }
}