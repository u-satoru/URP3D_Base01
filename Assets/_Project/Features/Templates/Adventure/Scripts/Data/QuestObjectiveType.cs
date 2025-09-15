namespace asterivo.Unity60.Features.Templates.Adventure.Data
{
    /// <summary>
    /// クエスト目標の種類を定義する列挙型
    /// アドベンチャーテンプレート用クエストシステム
    /// </summary>
    public enum QuestObjectiveType
    {
        /// <summary>特定のアイテムを集める</summary>
        /// <summary>特定のアイテムを集める（CollectItemの別名）</summary>
        Collect = 0,
        
        CollectItem = 0,
        
        /// <summary>特定の敵を倒す</summary>
        KillEnemy = 1,
        
        /// <summary>特定の場所に行く</summary>
        ReachLocation = 2,
        
        /// <summary>特定のNPCと話す</summary>
        TalkToNPC = 3,
        
        /// <summary>特定のオブジェクトとインタラクション</summary>
        InteractWithObject = 4,
        
        /// <summary>特定のオブジェクトとインタラクション（InteractWithObjectの別名）</summary>
        Interact = 4,
        
        /// <summary>特定のアイテムを配達する</summary>
        DeliverItem = 5,
        
        /// <summary>特定の時間待機する</summary>
        WaitTime = 6,
        
        /// <summary>特定のスキルを使用する</summary>
        UseSkill = 7,
        
        /// <summary>特定の条件を満たす</summary>
        MeetCondition = 8,
        
        /// <summary>特定のクエストを完了する</summary>
        CompleteQuest = 9,
        
        /// <summary>特定のエリアを探索する</summary>
        ExploreArea = 10,
        
        /// <summary>特定のレベルに達する</summary>
        ReachLevel = 11,
        
        /// <summary>特定のアイテムを使用する</summary>
        Use = 13,
        
        /// <summary>特定のアイテムを使用する（Useの別名）</summary>
        UseItem = 13,
        
        /// <summary>特定の金額を稼ぐ</summary>
        EarnGold = 12,
        
        /// <summary>カスタム目標</summary>
        Custom = 99
    }
    
    /// <summary>
    /// QuestObjectiveType用の拡張メソッド
    /// </summary>
    public static class QuestObjectiveTypeExtensions
    {
        /// <summary>
        /// 目標タイプの日本語名を取得
        /// </summary>
        public static string GetDisplayName(this QuestObjectiveType type)
        {
            return type switch
            {
                QuestObjectiveType.CollectItem => "アイテム収集",
                QuestObjectiveType.KillEnemy => "敵の討伐",
                QuestObjectiveType.ReachLocation => "場所への到達",
                QuestObjectiveType.TalkToNPC => "NPC との会話",
                QuestObjectiveType.InteractWithObject => "オブジェクト操作",
                QuestObjectiveType.DeliverItem => "アイテム配達",
                QuestObjectiveType.WaitTime => "時間待機",
                QuestObjectiveType.UseSkill => "スキル使用",
                QuestObjectiveType.MeetCondition => "条件達成",
                QuestObjectiveType.CompleteQuest => "クエスト完了",
                QuestObjectiveType.ExploreArea => "エリア探索",
                QuestObjectiveType.ReachLevel => "レベル到達",
                QuestObjectiveType.EarnGold => "ゴールド獲得",
                QuestObjectiveType.Custom => "カスタム",
                _ => "不明"
            };
        }
        
        /// <summary>
        /// 数値進捗を持つ目標タイプかどうか判定
        /// </summary>
        public static bool HasNumericProgress(this QuestObjectiveType type)
        {
            return type switch
            {
                QuestObjectiveType.CollectItem => true,
                QuestObjectiveType.KillEnemy => true,
                QuestObjectiveType.DeliverItem => true,
                QuestObjectiveType.WaitTime => true,
                QuestObjectiveType.UseSkill => true,
                QuestObjectiveType.EarnGold => true,
                QuestObjectiveType.ReachLevel => true,
                _ => false
            };
        }
        
        /// <summary>
        /// 瞬時に完了する目標タイプかどうか判定
        /// </summary>
        public static bool IsInstantComplete(this QuestObjectiveType type)
        {
            return type switch
            {
                QuestObjectiveType.ReachLocation => true,
                QuestObjectiveType.TalkToNPC => true,
                QuestObjectiveType.InteractWithObject => true,
                QuestObjectiveType.CompleteQuest => true,
                _ => false
            };
        }
        
        /// <summary>
        /// 目標の説明文テンプレートを取得
        /// </summary>
        public static string GetDescriptionTemplate(this QuestObjectiveType type)
        {
            return type switch
            {
                QuestObjectiveType.CollectItem => "{0} を {1} 個集める",
                QuestObjectiveType.KillEnemy => "{0} を {1} 体倒す", 
                QuestObjectiveType.ReachLocation => "{0} に到達する",
                QuestObjectiveType.TalkToNPC => "{0} と話す",
                QuestObjectiveType.InteractWithObject => "{0} と相互作用する",
                QuestObjectiveType.DeliverItem => "{0} を {1} に {2} 個配達する",
                QuestObjectiveType.WaitTime => "{0} 秒間待機する",
                QuestObjectiveType.UseSkill => "{0} を {1} 回使用する",
                QuestObjectiveType.MeetCondition => "{0} の条件を満たす",
                QuestObjectiveType.CompleteQuest => "クエスト '{0}' を完了する",
                QuestObjectiveType.ExploreArea => "{0} エリアを探索する",
                QuestObjectiveType.ReachLevel => "レベル {0} に到達する",
                QuestObjectiveType.EarnGold => "{0} ゴールドを獲得する",
                QuestObjectiveType.Custom => "{0}",
                _ => "目標を達成する"
            };
        }
        
        /// <summary>
        /// デフォルトの完了メッセージを取得
        /// </summary>
        public static string GetCompletionMessage(this QuestObjectiveType type)
        {
            return type switch
            {
                QuestObjectiveType.CollectItem => "必要なアイテムを集めました",
                QuestObjectiveType.KillEnemy => "敵を討伐しました",
                QuestObjectiveType.ReachLocation => "目的地に到着しました",
                QuestObjectiveType.TalkToNPC => "会話を完了しました",
                QuestObjectiveType.InteractWithObject => "操作を完了しました",
                QuestObjectiveType.DeliverItem => "アイテムを配達しました",
                QuestObjectiveType.WaitTime => "待機時間が完了しました",
                QuestObjectiveType.UseSkill => "スキルを使用しました",
                QuestObjectiveType.MeetCondition => "条件を満たしました",
                QuestObjectiveType.CompleteQuest => "必要なクエストを完了しました",
                QuestObjectiveType.ExploreArea => "エリアの探索が完了しました",
                QuestObjectiveType.ReachLevel => "目標レベルに到達しました",
                QuestObjectiveType.EarnGold => "必要なゴールドを獲得しました",
                QuestObjectiveType.Custom => "目標を達成しました",
                _ => "目標が完了しました"
            };
        }
    }
}