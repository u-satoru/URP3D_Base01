using System;

namespace asterivo.Unity60.Features.Templates.Adventure.Data
{
    /// <summary>
    /// クエスト状態の列挙型
    /// クエストの進行状況を管理
    /// </summary>
    [Serializable]
    public enum QuestState
    {
        /// <summary>利用可能（開始可能）</summary>
        Available = 0,

        /// <summary>進行中</summary>
        InProgress = 1,

        /// <summary>完了済み</summary>
        Completed = 2,

        /// <summary>失敗</summary>
        Failed = 3,

        /// <summary>報酬受領済み</summary>
        Rewarded = 4,

        /// <summary>ロック済み（前提条件未満）</summary>
        Locked = 5,

        /// <summary>期限切れ</summary>
        Expired = 6,

        /// <summary>キャンセル済み</summary>
        Cancelled = 7,

        /// <summary>一時停止</summary>
        Paused = 8,

        /// <summary>条件待ち</summary>
        WaitingForCondition = 9
    }

    /// <summary>
    /// QuestState用の拡張メソッド
    /// </summary>
    public static class QuestStateExtensions
    {
        /// <summary>
        /// アクティブな状態かどうかを判定
        /// </summary>
        public static bool IsActive(this QuestState state)
        {
            return state == QuestState.InProgress ||
                   state == QuestState.WaitingForCondition ||
                   state == QuestState.Paused;
        }

        /// <summary>
        /// 完了した状態かどうかを判定
        /// </summary>
        public static bool IsFinished(this QuestState state)
        {
            return state == QuestState.Completed ||
                   state == QuestState.Failed ||
                   state == QuestState.Rewarded ||
                   state == QuestState.Expired ||
                   state == QuestState.Cancelled;
        }

        /// <summary>
        /// 開始可能な状態かどうかを判定
        /// </summary>
        public static bool CanStart(this QuestState state)
        {
            return state == QuestState.Available;
        }

        /// <summary>
        /// キャンセル可能な状態かどうかを判定
        /// </summary>
        public static bool CanCancel(this QuestState state)
        {
            return state == QuestState.InProgress ||
                   state == QuestState.WaitingForCondition ||
                   state == QuestState.Paused;
        }

        /// <summary>
        /// 再開可能な状態かどうかを判定
        /// </summary>
        public static bool CanResume(this QuestState state)
        {
            return state == QuestState.Paused;
        }

        /// <summary>
        /// 報酬受領可能な状態かどうかを判定
        /// </summary>
        public static bool CanClaimReward(this QuestState state)
        {
            return state == QuestState.Completed;
        }

        /// <summary>
        /// 状態の日本語名を取得
        /// </summary>
        public static string GetDisplayName(this QuestState state)
        {
            return state switch
            {
                QuestState.Available => "開始可能",
                QuestState.InProgress => "進行中",
                QuestState.Completed => "完了",
                QuestState.Failed => "失敗",
                QuestState.Rewarded => "報酬受領済み",
                QuestState.Locked => "ロック中",
                QuestState.Expired => "期限切れ",
                QuestState.Cancelled => "キャンセル",
                QuestState.Paused => "一時停止",
                QuestState.WaitingForCondition => "条件待ち",
                _ => "不明"
            };
        }

        /// <summary>
        /// 状態の色を取得（UI表示用）
        /// </summary>
        public static UnityEngine.Color GetStateColor(this QuestState state)
        {
            return state switch
            {
                QuestState.Available => UnityEngine.Color.white,
                QuestState.InProgress => UnityEngine.Color.yellow,
                QuestState.Completed => UnityEngine.Color.green,
                QuestState.Failed => UnityEngine.Color.red,
                QuestState.Rewarded => UnityEngine.Color.cyan,
                QuestState.Locked => UnityEngine.Color.gray,
                QuestState.Expired => new UnityEngine.Color(0.8f, 0.4f, 0.0f), // オレンジ
                QuestState.Cancelled => UnityEngine.Color.magenta,
                QuestState.Paused => UnityEngine.Color.blue,
                QuestState.WaitingForCondition => new UnityEngine.Color(1f, 1f, 0.5f), // 薄い黄色
                _ => UnityEngine.Color.white
            };
        }

        /// <summary>
        /// 次の有効な状態遷移先を取得
        /// </summary>
        public static QuestState[] GetValidTransitions(this QuestState currentState)
        {
            return currentState switch
            {
                QuestState.Available => new[] { QuestState.InProgress, QuestState.Locked },
                QuestState.InProgress => new[] { QuestState.Completed, QuestState.Failed, QuestState.Paused, QuestState.Cancelled, QuestState.WaitingForCondition },
                QuestState.Completed => new[] { QuestState.Rewarded },
                QuestState.Failed => new[] { QuestState.Available }, // 再挑戦可能
                QuestState.Rewarded => new QuestState[0], // 終了状態
                QuestState.Locked => new[] { QuestState.Available },
                QuestState.Expired => new QuestState[0], // 終了状態
                QuestState.Cancelled => new[] { QuestState.Available }, // 再開可能
                QuestState.Paused => new[] { QuestState.InProgress, QuestState.Cancelled },
                QuestState.WaitingForCondition => new[] { QuestState.InProgress, QuestState.Failed },
                _ => new QuestState[0]
            };
        }

        /// <summary>
        /// 指定した状態に遷移可能かどうかを判定
        /// </summary>
        public static bool CanTransitionTo(this QuestState currentState, QuestState targetState)
        {
            var validTransitions = currentState.GetValidTransitions();
            return Array.IndexOf(validTransitions, targetState) >= 0;
        }
    }
}