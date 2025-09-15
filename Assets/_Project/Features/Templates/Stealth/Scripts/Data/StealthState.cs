namespace asterivo.Unity60.Features.Templates.Stealth
{
    /// <summary>
    /// プレイヤーのステルス状態を表現する列挙型
    /// Learn & Grow価値実現: 直感的な5段階状態管理による学習コスト削減
    /// </summary>
    public enum StealthState
    {
        /// <summary>完全に視認可能 - NPCに発見される可能性が最も高い</summary>
        Visible,
        
        /// <summary>部分的に隠蔽 - 環境要素による部分的隠蔽効果</summary>
        Concealed,
        
        /// <summary>完全に隠蔽 - 理想的な隠れ状態</summary>
        Hidden,
        
        /// <summary>発見済み - NPCによって確認された状態</summary>
        Detected,
        
        /// <summary>正体バレ状態 - 警戒が最高レベルに到達</summary>
        Compromised
    }

    /// <summary>
    /// 検知の種類を表現する列挙型
    /// 多様な検知システムの統合管理による高度なステルス体験の実現
    /// </summary>
    public enum DetectionType
    {
        /// <summary>視覚検知 - NPCVisualSensorによる直接目視</summary>
        Visual,
        
        /// <summary>聴覚検知 - 音響システムによる検知</summary>
        Auditory,
        
        /// <summary>環境的手がかり - ドア開放、物体移動等</summary>
        Environmental,
        
        /// <summary>他NPCからの情報 - 協調検出システム</summary>
        Cooperative
    }

    /// <summary>
    /// ステルス動作の種類
    /// プレイヤー行動の分類による適切な検知計算
    /// </summary>
    public enum StealthAction
    {
        /// <summary>通常移動</summary>
        Walking,
        
        /// <summary>走行移動（検知リスク高）</summary>
        Running,
        
        /// <summary>しゃがみ移動（検知リスク低）</summary>
        Crouching,
        
        /// <summary>這う移動（検知リスク最低）</summary>
        Crawling,
        
        /// <summary>静止状態</summary>
        Idle,
        
        /// <summary>オブジェクトインタラクション</summary>
        Interacting
    }

    /// <summary>
    /// 環境隠蔽レベル
    /// 環境による隠蔽効果の段階的管理
    /// </summary>
    public enum ConcealmentLevel
    {
        /// <summary>隠蔽なし - 完全露出</summary>
        None = 0,
        
        /// <summary>軽微な隠蔽 - 影、草むら等</summary>
        Light = 1,
        
        /// <summary>中程度の隠蔽 - 障害物、暗闇等</summary>
        Medium = 2,
        
        /// <summary>高度な隠蔽 - 完全な遮蔽物</summary>
        High = 3,
        
        /// <summary>完全隠蔽 - ロッカー、隠し部屋等</summary>
        Complete = 4
    }
}