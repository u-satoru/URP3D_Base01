using UnityEngine;
// using asterivo.Unity60.Core.Data;

namespace asterivo.Unity60.Core.Services
{
    /// <summary>
    /// スチE��スメカニクス統合サービスインターフェース
    /// ServiceLocator統合によるスチE��ス機�Eの一允E��琁E    ///
    /// パフォーマンス要件:
    /// - IUpdatableService統合による効玁E��更新制御
    /// - UpdatePriority=10�E�高優先度�E�でスチE��ス状態管琁E    /// - NeedsUpdate動的制御によるCPU最適匁E    ///
    /// 価値実現:
    /// - Learn & Grow: 統一APIによる学習コスチE0%削渁E    /// - Ship & Scale: Interface契紁E��よる保守性・チE��タビリチE��向丁E    /// </summary>
    public interface IStealthMechanicsService : IService, IUpdatableService
    {
        #region Core Stealth State API

        /// <summary>
        /// 現在の可視性レベルを取征E        /// </summary>
        /// <returns>可視性 (0.0=完�E隠蔽, 1.0=完�E可要E</returns>
        float GetVisibility();

        /// <summary>
        /// 現在のノイズレベルを取征E        /// </summary>
        /// <returns>ノイズレベル (0.0=無音, 1.0=最大音釁E</returns>
        float GetNoiseLevel();

        /// <summary>
        /// プレイヤーがカバ�E冁E��ぁE��かを判宁E        /// </summary>
        /// <returns>true=カバ�E冁E false=露出状慁E/returns>
        bool IsInCover();

        /// <summary>
        /// プレイヤーが影の中にぁE��かを判宁E        /// </summary>
        /// <returns>true=影冁E false=明るぁE��所</returns>
        bool IsInShadow();

        /// <summary>
        /// プレイヤーが検�EされてぁE��かを判宁E        /// </summary>
        /// <returns>true=検�E済み, false=未検�E</returns>
        bool IsDetected();

        /// <summary>
        /// 現在の検�Eレベルを取征E        /// </summary>
        /// <returns>検�Eレベル (0.0=未検�E, 1.0=完�E検�E)</returns>
        float GetDetectionLevel();

        /// <summary>
        /// 現在の警戒レベルを取征E        /// </summary>
        /// <returns>NPCの警戒状慁E/returns>
        AlertLevel GetAlertLevel();

        /// <summary>
        /// 現在のスチE��ス状態を取征E        /// </summary>
        /// <returns>現在のスチE��ス状慁E/returns>
        StealthState CurrentState { get; }

        #endregion

        #region Stealth Control API

        /// <summary>
        /// 強制皁E��スチE��ス状態に入めE        /// チE��チE��・チE��ト�E特殊イベント用
        /// </summary>
        void ForceEnterStealth();

        /// <summary>
        /// 持E��位置にチE��ストラクションを作�E
        /// NPCの注意をそらすため�E音響効极E        /// </summary>
        /// <param name="position">チE��ストラクション発生位置</param>
        /// <param name="radius">影響篁E��半征E/param>
        void CreateDistraction(Vector3 position, float radius);

        /// <summary>
        /// 隠れ場所に入めE        /// プレイヤーが隠れ場所に入った時の処琁E        /// </summary>
        /// <param name="hidingSpotTransform">入る隠れ場所のTransform</param>
        void EnterHidingSpot(Transform hidingSpotTransform);

        /// <summary>
        /// 隠れ場所から出めE        /// プレイヤーが隠れ場所から出た時の処琁E        /// </summary>
        void ExitHidingSpot();

        #endregion

        #region IUpdatableService Implementation

        /// <summary>
        /// サービス更新処琁E��Epdate()の代替�E�E        /// ServiceLocator統合による効玁E��更新管琁E        /// </summary>
        void UpdateService();

        /// <summary>
        /// 更新が忁E��かどぁE��の動的判宁E        /// パフォーマンス最適匁E 不要時はUpdateService()をスキチE�E
        /// </summary>
        bool NeedsUpdate { get; }

        /// <summary>
        /// 更新優先度�E�高優先度=10�E�E        /// スチE��ス状態�E他シスチE��の基盤となるためE��優先度で実衁E        /// </summary>
        int UpdatePriority => 10;

        #endregion

        #region Configuration & Events

        /// <summary>
        /// プレイヤートランスフォーム設宁E        /// 動的なプレイヤー変更に対忁E        /// </summary>
        Transform PlayerTransform { get; set; }

        /// <summary>
        /// スチE��ス機�Eの有効/無効制御
        /// パフォーマンス制御・チE��チE��用
        /// </summary>
        bool EnableStealthMechanics { get; set; }

        /// <summary>
        /// 更新間隔設宁E        /// パフォーマンス調整用�E�推奨: 0.1f秒！E        /// </summary>
        float UpdateInterval { get; set; }

        #endregion
    }

}