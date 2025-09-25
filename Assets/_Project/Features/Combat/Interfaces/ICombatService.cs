using UnityEngine;
using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Core.Commands;

namespace asterivo.Unity60.Features.Combat.Interfaces
{
    /// <summary>
    /// 戦闘システムサービスのインターフェース
    /// ServiceLocatorを通じて提供され、戦闘関連の中央管理を行う
    /// </summary>
    public interface ICombatService : IService
    {
        /// <summary>
        /// ダメージを与える（コマンドパターン使用）
        /// </summary>
        /// <param name="target">ダメージ対象</param>
        /// <param name="damage">ダメージ量</param>
        /// <param name="damageInfo">詳細なダメージ情報（オプション）</param>
        /// <returns>実際に与えたダメージ量</returns>
        float DealDamage(GameObject target, float damage, DamageInfo damageInfo = default);

        /// <summary>
        /// ヘルスを回復する
        /// </summary>
        /// <param name="target">回復対象</param>
        /// <param name="amount">回復量</param>
        /// <returns>実際に回復した量</returns>
        float HealTarget(GameObject target, float amount);

        /// <summary>
        /// 戦闘状態を開始する
        /// </summary>
        /// <param name="attacker">攻撃者</param>
        /// <param name="target">標的</param>
        void StartCombat(GameObject attacker, GameObject target);

        /// <summary>
        /// 戦闘状態を終了する
        /// </summary>
        /// <param name="participant">戦闘参加者</param>
        void EndCombat(GameObject participant);

        /// <summary>
        /// 戦闘中かどうかを確認
        /// </summary>
        /// <param name="participant">確認対象</param>
        /// <returns>戦闘中の場合true</returns>
        bool IsInCombat(GameObject participant);

        /// <summary>
        /// ダメージコマンドを取得（ObjectPool使用）
        /// </summary>
        /// <returns>プールから取得したDamageCommand</returns>
        DamageCommand GetDamageCommand();

        /// <summary>
        /// ダメージコマンドを返却（ObjectPoolへ）
        /// </summary>
        /// <param name="command">返却するコマンド</param>
        void ReturnDamageCommand(DamageCommand command);

        /// <summary>
        /// ヘルスコンポーネントを登録
        /// </summary>
        /// <param name="health">登録するヘルスコンポーネント</param>
        void RegisterHealth(IHealth health);

        /// <summary>
        /// ヘルスコンポーネントの登録解除
        /// </summary>
        /// <param name="health">登録解除するヘルスコンポーネント</param>
        void UnregisterHealth(IHealth health);

        /// <summary>
        /// ゲームオブジェクトからヘルスコンポーネントを取得
        /// </summary>
        /// <param name="gameObject">対象のゲームオブジェクト</param>
        /// <returns>ヘルスコンポーネント（見つからない場合null）</returns>
        IHealth GetHealth(GameObject gameObject);

        /// <summary>
        /// 戦闘統計を取得
        /// </summary>
        /// <returns>戦闘統計データ</returns>
        CombatStatistics GetStatistics();
    }

    /// <summary>
    /// 戦闘統計データ
    /// </summary>
    public struct CombatStatistics
    {
        public int TotalDamageDealt;
        public int TotalDamageReceived;
        public int TotalHealing;
        public int Kills;
        public int Deaths;
        public float CombatTime;
        public int ActiveCombatants;

        public void Reset()
        {
            TotalDamageDealt = 0;
            TotalDamageReceived = 0;
            TotalHealing = 0;
            Kills = 0;
            Deaths = 0;
            CombatTime = 0;
            ActiveCombatants = 0;
        }
    }
}
