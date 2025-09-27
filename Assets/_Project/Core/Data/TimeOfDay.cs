namespace asterivo.Unity60.Core.Data
{
    /// <summary>
    /// 時間帯統合制御列挙型（環境音響・照明・AI行動統合管理）
    ///
    /// Unity 6における3層アーキテクチャのCore層データ基盤において、
    /// 時間帯に連動した環境音響、照明制御、AI行動パターンを統合管理する
    /// 中核的な時間分類列挙型です。
    /// TimeAmbientController、LightingSystem、AIBehaviorSystemとの統合により、
    /// 一日の時間サイクルに応じた包括的な環境制御を実現します。
    ///
    /// 【環境音響システム統合】
    /// - TimeAmbientController: 時間帯別環境音の自動切り替え制御
    /// - Crossfade Transitions: 時間遷移時の滑らかな音響ブレンド
    /// - Ambient Audio Profiles: 各時間帯専用の音響プロファイル管理
    /// - 3D Spatial Audio: 時間帯による音響伝播特性の動的調整
    ///
    /// 【動的照明制御統合】
    /// - Lighting Manager: 時間帯に応じた照明強度・色温度の自動調整
    /// - Shadow Quality: 時間帯による影の品質・長さの動的制御
    /// - Post-Processing: 時間帯別の色調補正・雰囲気演出
    /// - Real-time Reflection: 環境光変化に対応したリアルタイム反射
    ///
    /// 【AI行動パターン統合】
    /// - Patrol Behavior: 時間帯による巡回ルート・頻度の変更
    /// - Alertness Level: 時間帯による基本警戒レベルの調整
    /// - Vision Range: 照明条件に応じた視覚センサー範囲の動的調整
    /// - Behavior Tree: 時間帯条件ノードによるAI行動の分岐制御
    ///
    /// 【ステルスシステム連携】
    /// - Stealth Bonus: Night時の隠蔽ボーナス、Day時の発見ペナルティ
    /// - Shadow Coverage: 時間帯による影の濃度・範囲の隠蔽効果調整
    /// - Noise Masking: 時間帯別環境音によるプレイヤー音響のマスキング
    /// - Detection Threshold: 照明条件による検知閾値の自動調整
    ///
    /// 【Template層での活用】
    /// - StealthTemplate: 時間帯戦略による難易度調整システム
    /// - SurvivalHorrorTemplate: 時間進行による恐怖演出の強化
    /// - ActionRPGTemplate: 時間帯クエスト、昼夜アイテム出現制御
    /// - OpenWorldTemplate: 動的時間サイクルによる世界観演出
    ///
    /// 【データ駆動設計統合】
    /// - TimeBasedSettings.asset: 時間帯別設定パラメータの外部管理
    /// - AudioTimeProfile.asset: 時間帯別音響設定プロファイル
    /// - LightingTimeConfig.asset: 時間帯別照明設定データ
    /// - AITimeSchedule.asset: 時間帯別AI行動スケジュール設定
    ///
    /// 【リアルタイム時間同期】
    /// - System Time Integration: システム時刻との自動同期機能
    /// - Custom Time Control: ゲーム内時間の独立制御対応
    /// - Time Scale Support: 時間の加速・減速制御との連携
    /// - Transition Smoothing: 時間帯切り替え時の滑らかな遷移制御
    ///
    /// 【パフォーマンス最適化】
    /// - O(1) Time Check: 列挙型による高速時間判定
    /// - Event-Driven Updates: 時間変化時のみの効率的システム更新
    /// - LOD Integration: 時間帯による描画品質の動的調整
    /// - Caching System: 時間帯設定データの事前キャッシュによる高速化
    ///
    /// 【4段階時間分類設計】
    /// - Day (昼): 高い視認性、活発なAI、明るい環境音
    /// - Evening (夕方): 遷移期、変化する照明、警戒度上昇
    /// - Night (夜): 隠蔽有利、制限された視界、静寂な環境
    /// - Dawn (明け方): 新たな始まり、回復期、徐々に明るくなる環境
    ///
    /// 【使用パターン】
    /// - 時間判定: if (currentTime == TimeOfDay.Night) { ApplyStealthBonus(); }
    /// - 環境切替: timeController.TransitionTo(TimeOfDay.Dawn)
    /// - AI制御: aiScheduler.ApplyTimeBasedBehavior(TimeOfDay.Evening)
    /// - 音響制御: audioManager.SetTimeAmbient(TimeOfDay.Day)
    /// </summary>
    public enum TimeOfDay
    {
        /// <summary>
        /// 昼
        /// </summary>
        Day,

        /// <summary>
        /// 夕方
        /// </summary>
        Evening,

        /// <summary>
        /// 夜
        /// </summary>
        Night,

        /// <summary>
        /// 明け方
        /// </summary>
        Dawn
    }
}