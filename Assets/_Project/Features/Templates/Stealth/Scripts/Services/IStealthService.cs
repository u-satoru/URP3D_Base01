using UnityEngine;
using asterivo.Unity60.Core;
using asterivo.Unity60.Features.Templates.Stealth.Environment;
using asterivo.Unity60.Features.Templates.Stealth.Events;

namespace asterivo.Unity60.Features.Templates.Stealth.Services
{
    /// <summary>
    /// 繧ｹ繝・Ν繧ｹ繧ｷ繧ｹ繝・Β縺ｮ荳ｭ螟ｮ蛻ｶ蠕｡繧ｵ繝ｼ繝薙せ繧､繝ｳ繧ｿ繝ｼ繝輔ぉ繝ｼ繧ｹ
    /// 繝励Ξ繧､繝､繝ｼ縺ｮ髫阡ｽ迥ｶ諷九∫腸蠅・→縺ｮ逶ｸ莠剃ｽ懃畑縲、I讀懷・繧ｷ繧ｹ繝・Β縺ｨ縺ｮ邨ｱ蜷医ｒ邂｡逅・
    /// </summary>
    public interface IStealthService : IService
    {
        #region Visibility Management
        /// <summary>
        /// 繝励Ξ繧､繝､繝ｼ縺ｮ迴ｾ蝨ｨ縺ｮ隕冶ｪ肴ｧ菫よ焚繧貞叙蠕・(0.0 = 螳悟・縺ｫ髫繧後※縺・ｋ, 1.0 = 螳悟・縺ｫ隕九∴繧・
        /// </summary>
        float PlayerVisibilityFactor { get; }

        /// <summary>
        /// 繝励Ξ繧､繝､繝ｼ縺ｮ迴ｾ蝨ｨ縺ｮ髻ｳ髻ｿ繝ｬ繝吶Ν繧貞叙蠕・(0.0 = 辟｡髻ｳ, 1.0 = 譛螟ｧ髻ｳ驥・
        /// </summary>
        float PlayerNoiseLevel { get; }

        /// <summary>
        /// 謖・ｮ壻ｽ咲ｽｮ縺ｧ縺ｮ蜈蛾㍼繝ｬ繝吶Ν繧定ｨ育ｮ・
        /// </summary>
        /// <param name="position">險育ｮ怜ｯｾ雎｡縺ｮ菴咲ｽｮ</param>
        /// <returns>蜈蛾㍼繝ｬ繝吶Ν (0.0 = 螳悟・縺ｪ髣・ 1.0 = 螳悟・縺ｪ蜈・</returns>
        float CalculateLightLevel(Vector3 position);

        /// <summary>
        /// 繝励Ξ繧､繝､繝ｼ縺ｮ隕冶ｪ肴ｧ菫よ焚繧呈峩譁ｰ
        /// </summary>
        /// <param name="visibilityFactor">譁ｰ縺励＞隕冶ｪ肴ｧ菫よ焚</param>
        void UpdatePlayerVisibility(float visibilityFactor);

        /// <summary>
        /// 繝励Ξ繧､繝､繝ｼ縺ｮ髻ｳ髻ｿ繝ｬ繝吶Ν繧呈峩譁ｰ
        /// </summary>
        /// <param name="noiseLevel">譁ｰ縺励＞髻ｳ髻ｿ繝ｬ繝吶Ν</param>
        void UpdatePlayerNoiseLevel(float noiseLevel);
        #endregion

        #region Concealment System
        /// <summary>
        /// 髫阡ｽ繧ｾ繝ｼ繝ｳ縺ｫ繝励Ξ繧､繝､繝ｼ縺悟・縺｣縺溘％縺ｨ繧帝夂衍
        /// </summary>
        /// <param name="concealmentZone">髫阡ｽ繧ｾ繝ｼ繝ｳ縺ｮ繧ｳ繝ｳ繝昴・繝阪Φ繝・/param>
        void EnterConcealmentZone(IConcealmentZone concealmentZone);

        /// <summary>
        /// 髫阡ｽ繧ｾ繝ｼ繝ｳ縺九ｉ繝励Ξ繧､繝､繝ｼ縺悟・縺溘％縺ｨ繧帝夂衍
        /// </summary>
        /// <param name="concealmentZone">髫阡ｽ繧ｾ繝ｼ繝ｳ縺ｮ繧ｳ繝ｳ繝昴・繝阪Φ繝・/param>
        void ExitConcealmentZone(IConcealmentZone concealmentZone);

        /// <summary>
        /// 繝励Ξ繧､繝､繝ｼ縺檎樟蝨ｨ髫阡ｽ繧ｾ繝ｼ繝ｳ蜀・↓縺・ｋ縺九←縺・°
        /// </summary>
        bool IsPlayerConcealed { get; }

        /// <summary>
        /// 迴ｾ蝨ｨ縺ｮ繧｢繧ｯ繝・ぅ繝悶↑髫阡ｽ繧ｾ繝ｼ繝ｳ
        /// </summary>
        IConcealmentZone CurrentConcealmentZone { get; }
        #endregion

        #region Environmental Interaction
        /// <summary>
        /// 迺ｰ蠅・が繝悶ず繧ｧ繧ｯ繝医→縺ｮ逶ｸ莠剃ｽ懃畑繧貞ｮ溯｡・
        /// </summary>
        /// <param name="interactableObject">逶ｸ莠剃ｽ懃畑蟇ｾ雎｡繧ｪ繝悶ず繧ｧ繧ｯ繝・/param>
        /// <param name="interactionType">逶ｸ莠剃ｽ懃畑縺ｮ遞ｮ鬘・/param>
        /// <returns>逶ｸ莠剃ｽ懃畑縺梧・蜉溘＠縺溘°縺ｩ縺・°</returns>
        bool InteractWithEnvironment(GameObject interactableObject, StealthInteractionType interactionType);

        /// <summary>
        /// 謖・ｮ壻ｽ咲ｽｮ縺ｫ髯ｽ蜍慕畑縺ｮ髻ｳ繧堤匱逕・
        /// </summary>
        /// <param name="position">髻ｳ縺ｮ逋ｺ逕滉ｽ咲ｽｮ</param>
        /// <param name="noiseLevel">髻ｳ縺ｮ螟ｧ縺阪＆ (0.0 - 1.0)</param>
        void CreateDistraction(Vector3 position, float noiseLevel);
        #endregion

        #region Detection Integration
        /// <summary>
        /// AI讀懷・繧ｷ繧ｹ繝・Β縺九ｉ縺ｮ逍大ｿ・Ξ繝吶Ν譖ｴ譁ｰ繧貞女菫｡
        /// </summary>
        /// <param name="detector">讀懷・繧定｡後▲縺蘗I</param>
        /// <param name="suspicionLevel">逍大ｿ・Ξ繝吶Ν (0.0 - 1.0)</param>
        void OnAISuspicionChanged(GameObject detector, float suspicionLevel);

        /// <summary>
        /// 繝励Ξ繧､繝､繝ｼ縺檎匱隕九＆繧後◆譎ゅ・蜃ｦ逅・
        /// </summary>
        /// <param name="detector">逋ｺ隕九＠縺蘗I</param>
        void OnPlayerSpotted(GameObject detector);

        /// <summary>
        /// 繝励Ξ繧､繝､繝ｼ縺瑚ｦ也阜縺九ｉ螟悶ｌ縺滓凾縺ｮ蜃ｦ逅・
        /// </summary>
        /// <param name="detector">隕句､ｱ縺｣縺蘗I</param>
        void OnPlayerLost(GameObject detector);
        #endregion

        #region State Management
        /// <summary>
        /// 繧ｹ繝・Ν繧ｹ繝｢繝ｼ繝峨・譛牙柑/辟｡蜉ｹ繧貞・繧頑崛縺・
        /// </summary>
        /// <param name="enabled">譛牙柑縺ｫ縺吶ｋ縺九←縺・°</param>
        void SetStealthMode(bool enabled);

        /// <summary>
        /// 迴ｾ蝨ｨ繧ｹ繝・Ν繧ｹ繝｢繝ｼ繝峨′譛牙柑縺九←縺・°
        /// </summary>
        bool IsStealthModeActive { get; }

        /// <summary>
        /// 繧ｹ繝・Ν繧ｹ邨ｱ險域ュ蝣ｱ繧貞叙蠕・
        /// </summary>
        StealthStatistics GetStealthStatistics();
        #endregion
    }
}


