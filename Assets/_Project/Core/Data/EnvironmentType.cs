namespace asterivo.Unity60.Core.Data
{
    /// <summary>
    /// 環境音響・AIセンサー統合環境分類システム（データ駆動型・3層アーキテクチャ対応）
    ///
    /// Unity 6における3層アーキテクチャのCore層データ基盤において、
    /// 環境音響制御、AIセンサー検知、ステルスシステムで共通利用される
    /// 環境タイプの標準分類を定義します。
    /// ScriptableObjectベースのデータ駆動設計により、
    /// 各Template層での環境特化システムの構築基盤を提供します。
    ///
    /// 【環境音響システム統合】
    /// - 環境別AudioMixerGroup自動選択: 各環境に最適化された音響プロファイル
    /// - リバーブ・エコー効果制御: 環境特性に応じた自動音響処理
    /// - 3D空間音響最適化: 環境ごとの距離減衰・遮蔽特性調整
    /// - 動的環境遷移: DynamicAudioEnvironmentとの統合による滑らかな環境切り替え
    ///
    /// 【AIセンサーシステム活用】
    /// - 視覚センサー調整: 環境光・視界距離の自動補正
    /// - 聴覚センサー最適化: 環境音響特性に応じた検知閾値調整
    /// - 嗅覚センサー制御: 風向・気流パターンの環境別設定
    /// - センサー統合判定: 環境要因を考慮した総合警戒レベル算出
    ///
    /// 【ステルスゲームプレイ統合】
    /// - 環境隠蔽効果: Forest（茂み隠れ）、Cave（暗闇隠蔽）等の特殊効果
    /// - 足音・移動音調整: 環境表面材質に応じた音響フィードバック
    /// - NPCパトロール行動: 環境タイプ別のAI巡回パターン最適化
    /// - プレイヤー行動制約: Underwater（水中移動制限）等の環境固有ルール
    ///
    /// 【Template層での環境特化活用】
    /// - StealthTemplate: 環境隠蔽ボーナス、検知距離補正の詳細設定
    /// - SurvivalHorrorTemplate: 環境恐怖演出、雰囲気制御の自動化
    /// - ActionRPGTemplate: 環境バフ・デバフ、特殊能力制約の実装
    /// - PlatformerTemplate: 環境ギミック、物理特性変更の基盤
    ///
    /// 【データ駆動設計統合】
    /// - EnvironmentSettings.asset: 各環境の詳細パラメータ定義
    /// - AudioEnvironmentProfile.asset: 環境別音響設定プロファイル
    /// - AIEnvironmentConfig.asset: 環境別AI行動パラメータ設定
    /// - StealthEnvironmentData.asset: 環境隠蔽・検知設定データ
    ///
    /// 【パフォーマンス特性】
    /// - O(1)環境判定: 列挙型による高速環境タイプ識別
    /// - メモリ効率: 値型による最小メモリフットプリント
    /// - キャッシュ効率: 環境別設定データの事前読み込み最適化
    /// - 実行時変更: リアルタイム環境遷移とシステム連動
    ///
    /// 【拡張性・保守性】
    /// - 新環境追加: 単純な列挙値追加による機能拡張
    /// - 後方互換性: 既存環境データの保持とマイグレーション対応
    /// - モジュラー設計: 各システムでの独立した環境対応実装
    /// - 設定外部化: ScriptableObjectによる環境パラメータの容易な調整
    ///
    /// 【使用パターン】
    /// - 環境音響: AudioManager.SetEnvironment(EnvironmentType.Forest)
    /// - AIセンサー: sensor.AdjustForEnvironment(currentEnvironment)
    /// - ステルス判定: stealthSystem.GetConcealmentBonus(EnvironmentType.Cave)
    /// - 環境遷移: environmentManager.TransitionTo(EnvironmentType.Underwater)
    /// </summary>
    public enum EnvironmentType
    {
        /// <summary>
        /// 屋外
        /// </summary>
        Outdoor,

        /// <summary>
        /// 屋内
        /// </summary>
        Indoor,

        /// <summary>
        /// 洞窟
        /// </summary>
        Cave,

        /// <summary>
        /// 森林
        /// </summary>
        Forest,

        /// <summary>
        /// 水中
        /// </summary>
        Underwater
    }
}