using UnityEngine;

namespace asterivo.Unity60.Core.Events
{
    /// <summary>
    /// 警戒レベル（一時的なプレースホルダー - アーキテクチャ分離対応）
    /// </summary>
    public enum AlertLevel
    {
        None = 0,
        Low = 1,
        Medium = 2,
        High = 3,
        Critical = 4
    }

    /// <summary>
    /// 警戒レベル変更イベント（一時的なプレースホルダー）
    /// </summary>
    [CreateAssetMenu(fileName = "AlertLevelEvent", menuName = "asterivo.Unity60/Events/Alert Level Event")]
    public class AlertLevelEvent : GenericGameEvent<AlertLevel>
    {
    }

    /// <summary>
    /// 警戒状態変更イベント（一時的なプレースホルダー）
    /// </summary>
    [CreateAssetMenu(fileName = "AlertStateEvent", menuName = "asterivo.Unity60/Events/Alert State Event")]
    public class AlertStateEvent : GenericGameEvent<bool>
    {
    }

    /// <summary>
    /// 検知イベント（一時的なプレースホルダー）
    /// </summary>
    [CreateAssetMenu(fileName = "DetectionEvent", menuName = "asterivo.Unity60/Events/Detection Event")]
    public class DetectionEvent : GenericGameEvent<object>
    {
    }
}