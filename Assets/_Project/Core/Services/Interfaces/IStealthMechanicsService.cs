using UnityEngine;
// using asterivo.Unity60.Core.Data;

namespace asterivo.Unity60.Core.Services
{
    /// <summary>
    /// 繧ｹ繝・Ν繧ｹ繝｡繧ｫ繝九け繧ｹ邨ｱ蜷医し繝ｼ繝薙せ繧､繝ｳ繧ｿ繝ｼ繝輔ぉ繝ｼ繧ｹ
    /// ServiceLocator邨ｱ蜷医↓繧医ｋ繧ｹ繝・Ν繧ｹ讖溯・縺ｮ荳蜈・ｮ｡逅・    ///
    /// 繝代ヵ繧ｩ繝ｼ繝槭Φ繧ｹ隕∽ｻｶ:
    /// - IUpdatableService邨ｱ蜷医↓繧医ｋ蜉ｹ邇・噪譖ｴ譁ｰ蛻ｶ蠕｡
    /// - UpdatePriority=10・磯ｫ伜━蜈亥ｺｦ・峨〒繧ｹ繝・Ν繧ｹ迥ｶ諷狗ｮ｡逅・    /// - NeedsUpdate蜍慕噪蛻ｶ蠕｡縺ｫ繧医ｋCPU譛驕ｩ蛹・    ///
    /// 萓｡蛟､螳溽樟:
    /// - Learn & Grow: 邨ｱ荳API縺ｫ繧医ｋ蟄ｦ鄙偵さ繧ｹ繝・0%蜑頑ｸ・    /// - Ship & Scale: Interface螂醍ｴ・↓繧医ｋ菫晏ｮ域ｧ繝ｻ繝・せ繧ｿ繝薙Μ繝・ぅ蜷台ｸ・    /// </summary>
    public interface IStealthMechanicsService : IService, IUpdatableService
    {
        #region Core Stealth State API

        /// <summary>
        /// 迴ｾ蝨ｨ縺ｮ蜿ｯ隕匁ｧ繝ｬ繝吶Ν繧貞叙蠕・        /// </summary>
        /// <returns>蜿ｯ隕匁ｧ (0.0=螳悟・髫阡ｽ, 1.0=螳悟・蜿ｯ隕・</returns>
        float GetVisibility();

        /// <summary>
        /// 迴ｾ蝨ｨ縺ｮ繝弱う繧ｺ繝ｬ繝吶Ν繧貞叙蠕・        /// </summary>
        /// <returns>繝弱う繧ｺ繝ｬ繝吶Ν (0.0=辟｡髻ｳ, 1.0=譛螟ｧ髻ｳ驥・</returns>
        float GetNoiseLevel();

        /// <summary>
        /// 繝励Ξ繧､繝､繝ｼ縺後き繝舌・蜀・↓縺・ｋ縺九ｒ蛻､螳・        /// </summary>
        /// <returns>true=繧ｫ繝舌・蜀・ false=髴ｲ蜃ｺ迥ｶ諷・/returns>
        bool IsInCover();

        /// <summary>
        /// 繝励Ξ繧､繝､繝ｼ縺悟ｽｱ縺ｮ荳ｭ縺ｫ縺・ｋ縺九ｒ蛻､螳・        /// </summary>
        /// <returns>true=蠖ｱ蜀・ false=譏弱ｋ縺・ｴ謇</returns>
        bool IsInShadow();

        /// <summary>
        /// 繝励Ξ繧､繝､繝ｼ縺梧､懷・縺輔ｌ縺ｦ縺・ｋ縺九ｒ蛻､螳・        /// </summary>
        /// <returns>true=讀懷・貂医∩, false=譛ｪ讀懷・</returns>
        bool IsDetected();

        /// <summary>
        /// 迴ｾ蝨ｨ縺ｮ讀懷・繝ｬ繝吶Ν繧貞叙蠕・        /// </summary>
        /// <returns>讀懷・繝ｬ繝吶Ν (0.0=譛ｪ讀懷・, 1.0=螳悟・讀懷・)</returns>
        float GetDetectionLevel();

        /// <summary>
        /// 迴ｾ蝨ｨ縺ｮ隴ｦ謌偵Ξ繝吶Ν繧貞叙蠕・        /// </summary>
        /// <returns>NPC縺ｮ隴ｦ謌堤憾諷・/returns>
        AlertLevel GetAlertLevel();

        /// <summary>
        /// 迴ｾ蝨ｨ縺ｮ繧ｹ繝・Ν繧ｹ迥ｶ諷九ｒ蜿門ｾ・        /// </summary>
        /// <returns>迴ｾ蝨ｨ縺ｮ繧ｹ繝・Ν繧ｹ迥ｶ諷・/returns>
        StealthState CurrentState { get; }

        #endregion

        #region Stealth Control API

        /// <summary>
        /// 蠑ｷ蛻ｶ逧・↓繧ｹ繝・Ν繧ｹ迥ｶ諷九↓蜈･繧・        /// 繝・ヰ繝・げ繝ｻ繝・せ繝医・迚ｹ谿翫う繝吶Φ繝育畑
        /// </summary>
        void ForceEnterStealth();

        /// <summary>
        /// 謖・ｮ壻ｽ咲ｽｮ縺ｫ繝・ぅ繧ｹ繝医Λ繧ｯ繧ｷ繝ｧ繝ｳ繧剃ｽ懈・
        /// NPC縺ｮ豕ｨ諢上ｒ縺昴ｉ縺吶◆繧√・髻ｳ髻ｿ蜉ｹ譫・        /// </summary>
        /// <param name="position">繝・ぅ繧ｹ繝医Λ繧ｯ繧ｷ繝ｧ繝ｳ逋ｺ逕滉ｽ咲ｽｮ</param>
        /// <param name="radius">蠖ｱ髻ｿ遽・峇蜊雁ｾ・/param>
        void CreateDistraction(Vector3 position, float radius);

        /// <summary>
        /// 髫繧悟ｴ謇縺ｫ蜈･繧・        /// 繝励Ξ繧､繝､繝ｼ縺碁國繧悟ｴ謇縺ｫ蜈･縺｣縺滓凾縺ｮ蜃ｦ逅・        /// </summary>
        /// <param name="hidingSpotTransform">蜈･繧矩國繧悟ｴ謇縺ｮTransform</param>
        void EnterHidingSpot(Transform hidingSpotTransform);

        /// <summary>
        /// 髫繧悟ｴ謇縺九ｉ蜃ｺ繧・        /// 繝励Ξ繧､繝､繝ｼ縺碁國繧悟ｴ謇縺九ｉ蜃ｺ縺滓凾縺ｮ蜃ｦ逅・        /// </summary>
        void ExitHidingSpot();

        #endregion

        #region IUpdatableService Implementation

        /// <summary>
        /// 繧ｵ繝ｼ繝薙せ譖ｴ譁ｰ蜃ｦ逅・ｼ・pdate()縺ｮ莉｣譖ｿ・・        /// ServiceLocator邨ｱ蜷医↓繧医ｋ蜉ｹ邇・噪譖ｴ譁ｰ邂｡逅・        /// </summary>
        void UpdateService();

        /// <summary>
        /// 譖ｴ譁ｰ縺悟ｿ・ｦ√°縺ｩ縺・°縺ｮ蜍慕噪蛻､螳・        /// 繝代ヵ繧ｩ繝ｼ繝槭Φ繧ｹ譛驕ｩ蛹・ 荳崎ｦ∵凾縺ｯUpdateService()繧偵せ繧ｭ繝・・
        /// </summary>
        bool NeedsUpdate { get; }

        /// <summary>
        /// 譖ｴ譁ｰ蜆ｪ蜈亥ｺｦ・磯ｫ伜━蜈亥ｺｦ=10・・        /// 繧ｹ繝・Ν繧ｹ迥ｶ諷九・莉悶す繧ｹ繝・Β縺ｮ蝓ｺ逶､縺ｨ縺ｪ繧九◆繧・ｫ伜━蜈亥ｺｦ縺ｧ螳溯｡・        /// </summary>
        int UpdatePriority => 10;

        #endregion

        #region Configuration & Events

        /// <summary>
        /// 繝励Ξ繧､繝､繝ｼ繝医Λ繝ｳ繧ｹ繝輔か繝ｼ繝險ｭ螳・        /// 蜍慕噪縺ｪ繝励Ξ繧､繝､繝ｼ螟画峩縺ｫ蟇ｾ蠢・        /// </summary>
        Transform PlayerTransform { get; set; }

        /// <summary>
        /// 繧ｹ繝・Ν繧ｹ讖溯・縺ｮ譛牙柑/辟｡蜉ｹ蛻ｶ蠕｡
        /// 繝代ヵ繧ｩ繝ｼ繝槭Φ繧ｹ蛻ｶ蠕｡繝ｻ繝・ヰ繝・げ逕ｨ
        /// </summary>
        bool EnableStealthMechanics { get; set; }

        /// <summary>
        /// 譖ｴ譁ｰ髢馴囈險ｭ螳・        /// 繝代ヵ繧ｩ繝ｼ繝槭Φ繧ｹ隱ｿ謨ｴ逕ｨ・域耳螂ｨ: 0.1f遘抵ｼ・        /// </summary>
        float UpdateInterval { get; set; }

        #endregion
    }

}
