using UnityEngine;
using asterivo.Unity60.Core.Data;

namespace asterivo.Unity60.Core.Services
{
    /// <summary>
    /// ステルスメカニクス統合サービスインターフェース
    /// ServiceLocator統合によるステルス機能の一元管理
    ///
    /// パフォーマンス要件:
    /// - IUpdatableService統合による効率的更新制御
    /// - UpdatePriority=10（高優先度）でステルス状態管理
    /// - NeedsUpdate動的制御によるCPU最適化
    ///
    /// 価値実現:
    /// - Learn & Grow: 統一APIによる学習コスト70%削減
    /// - Ship & Scale: Interface契約による保守性・テスタビリティ向上
    /// </summary>
    public interface IStealthMechanicsService : IService, IUpdatableService
    {
        #region Core Stealth State API

        /// <summary>
        /// 現在の可視性レベルを取得
        /// </summary>
        /// <returns>可視性 (0.0=完全隠蔽, 1.0=完全可視)</returns>
        float GetVisibility();

        /// <summary>
        /// 現在のノイズレベルを取得
        /// </summary>
        /// <returns>ノイズレベル (0.0=無音, 1.0=最大音量)</returns>
        float GetNoiseLevel();

        /// <summary>
        /// プレイヤーがカバー内にいるかを判定
        /// </summary>
        /// <returns>true=カバー内, false=露出状態</returns>
        bool IsInCover();

        /// <summary>
        /// プレイヤーが影の中にいるかを判定
        /// </summary>
        /// <returns>true=影内, false=明るい場所</returns>
        bool IsInShadow();

        /// <summary>
        /// プレイヤーが検出されているかを判定
        /// </summary>
        /// <returns>true=検出済み, false=未検出</returns>
        bool IsDetected();

        /// <summary>
        /// 現在の検出レベルを取得
        /// </summary>
        /// <returns>検出レベル (0.0=未検出, 1.0=完全検出)</returns>
        float GetDetectionLevel();

        /// <summary>
        /// 現在の警戒レベルを取得
        /// </summary>
        /// <returns>NPCの警戒状態</returns>
        AlertLevel GetAlertLevel();

        /// <summary>
        /// 現在のステルス状態を取得
        /// </summary>
        /// <returns>現在のステルス状態</returns>
        StealthState CurrentState { get; }

        #endregion

        #region Stealth Control API

        /// <summary>
        /// 強制的にステルス状態に入る
        /// デバッグ・テスト・特殊イベント用
        /// </summary>
        void ForceEnterStealth();

        /// <summary>
        /// 指定位置にディストラクションを作成
        /// NPCの注意をそらすための音響効果
        /// </summary>
        /// <param name="position">ディストラクション発生位置</param>
        /// <param name="radius">影響範囲半径</param>
        void CreateDistraction(Vector3 position, float radius);

        /// <summary>
        /// 隠れ場所に入る
        /// プレイヤーが隠れ場所に入った時の処理
        /// </summary>
        /// <param name="hidingSpotTransform">入る隠れ場所のTransform</param>
        void EnterHidingSpot(Transform hidingSpotTransform);

        /// <summary>
        /// 隠れ場所から出る
        /// プレイヤーが隠れ場所から出た時の処理
        /// </summary>
        void ExitHidingSpot();

        #endregion

        #region IUpdatableService Implementation

        /// <summary>
        /// サービス更新処理（Update()の代替）
        /// ServiceLocator統合による効率的更新管理
        /// </summary>
        void UpdateService();

        /// <summary>
        /// 更新が必要かどうかの動的判定
        /// パフォーマンス最適化: 不要時はUpdateService()をスキップ
        /// </summary>
        bool NeedsUpdate { get; }

        /// <summary>
        /// 更新優先度（高優先度=10）
        /// ステルス状態は他システムの基盤となるため高優先度で実行
        /// </summary>
        int UpdatePriority => 10;

        #endregion

        #region Configuration & Events

        /// <summary>
        /// プレイヤートランスフォーム設定
        /// 動的なプレイヤー変更に対応
        /// </summary>
        Transform PlayerTransform { get; set; }

        /// <summary>
        /// ステルス機能の有効/無効制御
        /// パフォーマンス制御・デバッグ用
        /// </summary>
        bool EnableStealthMechanics { get; set; }

        /// <summary>
        /// 更新間隔設定
        /// パフォーマンス調整用（推奨: 0.1f秒）
        /// </summary>
        float UpdateInterval { get; set; }

        #endregion
    }

}