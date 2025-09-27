namespace asterivo.Unity60.Core.Data
{
    /// <summary>
    /// 動的天候環境制御列挙型（環境音響・視覚効果・AI行動統合管理）
    ///
    /// Unity 6における3層アーキテクチャのCore層データ基盤において、
    /// 天候変化に連動した環境音響、視覚効果、AI行動パターンを統合制御する
    /// 核心的な天候分類列挙型です。
    /// WeatherAmbientController、ParticleSystem、AIBehaviorSystemとの統合により、
    /// 動的な天候変化に応じた没入感のある環境演出を実現します。
    ///
    /// 【環境音響システム統合】
    /// - WeatherAmbientController: 天候別環境音の動的レイヤード制御
    /// - Dynamic Crossfade: 天候遷移時の自然な音響ブレンド
    /// - Weather Audio Profiles: 各天候専用の多層音響構成管理
    /// - 3D Spatial Weather: 天候による音響伝播・遮蔽特性の動的調整
    ///
    /// 【視覚効果・パーティクル統合】
    /// - Weather Particles: Rain/Storm時の雨粒・雷エフェクト制御
    /// - Fog Rendering: Fog時の視界制限・雰囲気演出システム
    /// - Sky System: 天候による空の色調・雲の動的変化
    /// - Post-Processing: 天候別の画面効果・フィルター自動適用
    ///
    /// 【AI行動パターン適応】
    /// - Weather Behavior: 天候による巡回ルート・行動パターンの変更
    /// - Vision Adaptation: Rain/Fog時の視覚センサー範囲制限
    /// - Audio Masking: Storm時の強い環境音による聴覚センサー影響
    /// - Shelter Seeking: Storm時のNPC避難行動・屋内移動パターン
    ///
    /// 【ステルスシステム天候連携】
    /// - Rain Advantage: 雨音によるプレイヤー足音のマスキング効果
    /// - Fog Concealment: 霧による視界制限を活用した隠蔽ボーナス
    /// - Storm Chaos: 嵐時の混乱状態によるAI検知能力の大幅低下
    /// - Clear Penalty: 晴天時の高い視認性による発見リスク増加
    ///
    /// 【Template層での天候活用】
    /// - StealthTemplate: 天候戦術による多様なステルスアプローチ
    /// - SurvivalHorrorTemplate: Storm/Fog時の恐怖演出強化
    /// - OpenWorldTemplate: 動的天候サイクルによる世界観構築
    /// - ActionRPGTemplate: 天候クエスト・天候依存イベント制御
    ///
    /// 【データ駆動天候設定】
    /// - WeatherSettings.asset: 天候別詳細パラメータの外部管理
    /// - WeatherAudioProfile.asset: 天候別音響設定プロファイル
    /// - WeatherVFXConfig.asset: 天候別視覚効果設定データ
    /// - WeatherAIBehavior.asset: 天候別AI行動パラメータ設定
    ///
    /// 【リアルタイム天候システム】
    /// - Weather Transitions: 滑らかな天候遷移とブレンド制御
    /// - Intensity Control: 天候強度の段階的調整機能
    /// - Forecast System: 天候予報による事前準備・演出システム
    /// - Player Impact: 天候による移動速度・行動制約の動的適用
    ///
    /// 【パフォーマンス最適化】
    /// - O(1) Weather Check: 列挙型による高速天候判定
    /// - LOD Weather Effects: 距離に応じた天候エフェクトの品質調整
    /// - Culling Optimization: 天候エフェクトの効率的カリング制御
    /// - Batching System: 天候パーティクルの描画バッチング最適化
    ///
    /// 【4段階天候分類設計】
    /// - Clear (晴天): 高視認性、活発なAI、明瞭な環境音
    /// - Rain (雨): 音響マスキング、視界やや制限、湿潤効果
    /// - Storm (嵐): 強力マスキング、AI行動制限、激しい環境変化
    /// - Fog (霧): 視界大幅制限、隠蔽有利、神秘的雰囲気
    ///
    /// 【環境相互作用システム】
    /// - Surface Effects: 天候による地面・壁面の視覚的変化
    /// - Water Accumulation: Rain/Storm時の水たまり・流水エフェクト
    /// - Wind System: Storm時の風による物理オブジェクト・植生への影響
    /// - Temperature Effects: 天候による環境温度の視覚的表現
    ///
    /// 【使用パターン】
    /// - 天候判定: if (currentWeather == WeatherType.Storm) { ApplyAudioMasking(); }
    /// - 環境切替: weatherManager.TransitionTo(WeatherType.Fog)
    /// - AI制御: aiController.AdaptToWeather(WeatherType.Rain)
    /// - 音響制御: audioManager.SetWeatherAmbient(WeatherType.Clear)
    /// </summary>
    public enum WeatherType
    {
        /// <summary>
        /// 晴天
        /// </summary>
        Clear,

        /// <summary>
        /// 雨
        /// </summary>
        Rain,

        /// <summary>
        /// 嵐
        /// </summary>
        Storm,

        /// <summary>
        /// 霧
        /// </summary>
        Fog
    }
}