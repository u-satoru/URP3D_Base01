using UnityEngine;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Commands;

namespace asterivo.Unity60.Features.Combat.Interfaces
{
    /// <summary>
    /// 謌ｦ髣倥す繧ｹ繝・Β繧ｵ繝ｼ繝薙せ縺ｮ繧､繝ｳ繧ｿ繝ｼ繝輔ぉ繝ｼ繧ｹ
    /// ServiceLocator繧帝壹§縺ｦ謠蝉ｾ帙＆繧後∵姶髣倬未騾｣縺ｮ荳ｭ螟ｮ邂｡逅・ｒ陦後≧
    /// </summary>
    public interface ICombatService : IService
    {
        /// <summary>
        /// 繝繝｡繝ｼ繧ｸ繧剃ｸ弱∴繧具ｼ医さ繝槭Φ繝峨ヱ繧ｿ繝ｼ繝ｳ菴ｿ逕ｨ・・
        /// </summary>
        /// <param name="target">繝繝｡繝ｼ繧ｸ蟇ｾ雎｡</param>
        /// <param name="damage">繝繝｡繝ｼ繧ｸ驥・/param>
        /// <param name="damageInfo">隧ｳ邏ｰ縺ｪ繝繝｡繝ｼ繧ｸ諠・ｱ・医が繝励す繝ｧ繝ｳ・・/param>
        /// <returns>螳滄圀縺ｫ荳弱∴縺溘ム繝｡繝ｼ繧ｸ驥・/returns>
        float DealDamage(GameObject target, float damage, DamageInfo damageInfo = default);

        /// <summary>
        /// 繝倥Ν繧ｹ繧貞屓蠕ｩ縺吶ｋ
        /// </summary>
        /// <param name="target">蝗槫ｾｩ蟇ｾ雎｡</param>
        /// <param name="amount">蝗槫ｾｩ驥・/param>
        /// <returns>螳滄圀縺ｫ蝗槫ｾｩ縺励◆驥・/returns>
        float HealTarget(GameObject target, float amount);

        /// <summary>
        /// 謌ｦ髣倡憾諷九ｒ髢句ｧ九☆繧・
        /// </summary>
        /// <param name="attacker">謾ｻ謦・・/param>
        /// <param name="target">讓咏噪</param>
        void StartCombat(GameObject attacker, GameObject target);

        /// <summary>
        /// 謌ｦ髣倡憾諷九ｒ邨ゆｺ・☆繧・
        /// </summary>
        /// <param name="participant">謌ｦ髣伜盾蜉閠・/param>
        void EndCombat(GameObject participant);

        /// <summary>
        /// 謌ｦ髣倅ｸｭ縺九←縺・°繧堤｢ｺ隱・
        /// </summary>
        /// <param name="participant">遒ｺ隱榊ｯｾ雎｡</param>
        /// <returns>謌ｦ髣倅ｸｭ縺ｮ蝣ｴ蜷・rue</returns>
        bool IsInCombat(GameObject participant);

        /// <summary>
        /// 繝繝｡繝ｼ繧ｸ繧ｳ繝槭Φ繝峨ｒ蜿門ｾ暦ｼ・bjectPool菴ｿ逕ｨ・・
        /// </summary>
        /// <returns>繝励・繝ｫ縺九ｉ蜿門ｾ励＠縺櫂amageCommand</returns>
        DamageCommand GetDamageCommand();

        /// <summary>
        /// 繝繝｡繝ｼ繧ｸ繧ｳ繝槭Φ繝峨ｒ霑泌唆・・bjectPool縺ｸ・・
        /// </summary>
        /// <param name="command">霑泌唆縺吶ｋ繧ｳ繝槭Φ繝・/param>
        void ReturnDamageCommand(DamageCommand command);

        /// <summary>
        /// 繝倥Ν繧ｹ繧ｳ繝ｳ繝昴・繝阪Φ繝医ｒ逋ｻ骭ｲ
        /// </summary>
        /// <param name="health">逋ｻ骭ｲ縺吶ｋ繝倥Ν繧ｹ繧ｳ繝ｳ繝昴・繝阪Φ繝・/param>
        void RegisterHealth(IHealth health);

        /// <summary>
        /// 繝倥Ν繧ｹ繧ｳ繝ｳ繝昴・繝阪Φ繝医・逋ｻ骭ｲ隗｣髯､
        /// </summary>
        /// <param name="health">逋ｻ骭ｲ隗｣髯､縺吶ｋ繝倥Ν繧ｹ繧ｳ繝ｳ繝昴・繝阪Φ繝・/param>
        void UnregisterHealth(IHealth health);

        /// <summary>
        /// 繧ｲ繝ｼ繝繧ｪ繝悶ず繧ｧ繧ｯ繝医°繧峨・繝ｫ繧ｹ繧ｳ繝ｳ繝昴・繝阪Φ繝医ｒ蜿門ｾ・
        /// </summary>
        /// <param name="gameObject">蟇ｾ雎｡縺ｮ繧ｲ繝ｼ繝繧ｪ繝悶ず繧ｧ繧ｯ繝・/param>
        /// <returns>繝倥Ν繧ｹ繧ｳ繝ｳ繝昴・繝阪Φ繝茨ｼ郁ｦ九▽縺九ｉ縺ｪ縺・ｴ蜷・ull・・/returns>
        IHealth GetHealth(GameObject gameObject);

        /// <summary>
        /// 謌ｦ髣倡ｵｱ險医ｒ蜿門ｾ・
        /// </summary>
        /// <returns>謌ｦ髣倡ｵｱ險医ョ繝ｼ繧ｿ</returns>
        CombatStatistics GetStatistics();
    }

    /// <summary>
    /// 謌ｦ髣倡ｵｱ險医ョ繝ｼ繧ｿ
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


