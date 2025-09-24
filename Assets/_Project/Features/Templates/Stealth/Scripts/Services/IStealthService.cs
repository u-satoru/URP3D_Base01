using UnityEngine;
using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Features.Templates.Stealth.Environment;
using asterivo.Unity60.Features.Templates.Stealth.Events;

namespace asterivo.Unity60.Features.Templates.Stealth.Services
{
    /// <summary>
    /// ステルスシステムの中央制御サービスインターフェース
    /// プレイヤーの隠蔽状態、環境との相互作用、AI検出システムとの統合を管理
    /// </summary>
    public interface IStealthService : IService
    {
        #region Visibility Management
        /// <summary>
        /// プレイヤーの現在の視認性係数を取得 (0.0 = 完全に隠れている, 1.0 = 完全に見える)
        /// </summary>
        float PlayerVisibilityFactor { get; }

        /// <summary>
        /// プレイヤーの現在の音響レベルを取得 (0.0 = 無音, 1.0 = 最大音量)
        /// </summary>
        float PlayerNoiseLevel { get; }

        /// <summary>
        /// 指定位置での光量レベルを計算
        /// </summary>
        /// <param name="position">計算対象の位置</param>
        /// <returns>光量レベル (0.0 = 完全な闇, 1.0 = 完全な光)</returns>
        float CalculateLightLevel(Vector3 position);

        /// <summary>
        /// プレイヤーの視認性係数を更新
        /// </summary>
        /// <param name="visibilityFactor">新しい視認性係数</param>
        void UpdatePlayerVisibility(float visibilityFactor);

        /// <summary>
        /// プレイヤーの音響レベルを更新
        /// </summary>
        /// <param name="noiseLevel">新しい音響レベル</param>
        void UpdatePlayerNoiseLevel(float noiseLevel);
        #endregion

        #region Concealment System
        /// <summary>
        /// 隠蔽ゾーンにプレイヤーが入ったことを通知
        /// </summary>
        /// <param name="concealmentZone">隠蔽ゾーンのコンポーネント</param>
        void EnterConcealmentZone(IConcealmentZone concealmentZone);

        /// <summary>
        /// 隠蔽ゾーンからプレイヤーが出たことを通知
        /// </summary>
        /// <param name="concealmentZone">隠蔽ゾーンのコンポーネント</param>
        void ExitConcealmentZone(IConcealmentZone concealmentZone);

        /// <summary>
        /// プレイヤーが現在隠蔽ゾーン内にいるかどうか
        /// </summary>
        bool IsPlayerConcealed { get; }

        /// <summary>
        /// 現在のアクティブな隠蔽ゾーン
        /// </summary>
        IConcealmentZone CurrentConcealmentZone { get; }
        #endregion

        #region Environmental Interaction
        /// <summary>
        /// 環境オブジェクトとの相互作用を実行
        /// </summary>
        /// <param name="interactableObject">相互作用対象オブジェクト</param>
        /// <param name="interactionType">相互作用の種類</param>
        /// <returns>相互作用が成功したかどうか</returns>
        bool InteractWithEnvironment(GameObject interactableObject, StealthInteractionType interactionType);

        /// <summary>
        /// 指定位置に陽動用の音を発生
        /// </summary>
        /// <param name="position">音の発生位置</param>
        /// <param name="noiseLevel">音の大きさ (0.0 - 1.0)</param>
        void CreateDistraction(Vector3 position, float noiseLevel);
        #endregion

        #region Detection Integration
        /// <summary>
        /// AI検出システムからの疑心レベル更新を受信
        /// </summary>
        /// <param name="detector">検出を行ったAI</param>
        /// <param name="suspicionLevel">疑心レベル (0.0 - 1.0)</param>
        void OnAISuspicionChanged(GameObject detector, float suspicionLevel);

        /// <summary>
        /// プレイヤーが発見された時の処理
        /// </summary>
        /// <param name="detector">発見したAI</param>
        void OnPlayerSpotted(GameObject detector);

        /// <summary>
        /// プレイヤーが視界から外れた時の処理
        /// </summary>
        /// <param name="detector">見失ったAI</param>
        void OnPlayerLost(GameObject detector);
        #endregion

        #region State Management
        /// <summary>
        /// ステルスモードの有効/無効を切り替え
        /// </summary>
        /// <param name="enabled">有効にするかどうか</param>
        void SetStealthMode(bool enabled);

        /// <summary>
        /// 現在ステルスモードが有効かどうか
        /// </summary>
        bool IsStealthModeActive { get; }

        /// <summary>
        /// ステルス統計情報を取得
        /// </summary>
        StealthStatistics GetStealthStatistics();
        #endregion
    }
}