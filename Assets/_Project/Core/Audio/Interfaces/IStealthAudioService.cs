using UnityEngine;
// using asterivo.Unity60.Core.Data;

namespace asterivo.Unity60.Core.Audio.Interfaces
{
    /// <summary>
    /// 繧ｹ繝・Ν繧ｹ繧ｲ繝ｼ繝逕ｨ繧ｪ繝ｼ繝・ぅ繧ｪ繧ｵ繝ｼ繝薙せ縺ｮ繧､繝ｳ繧ｿ繝ｼ繝輔ぉ繝ｼ繧ｹ
    /// </summary>
    public interface IStealthAudioService
    {
        /// <summary>
        /// 雜ｳ髻ｳ繧堤函謌・        /// </summary>
        void CreateFootstep(Vector3 position, float intensity, string surfaceType);
        
        /// <summary>
        /// 迺ｰ蠅・ヮ繧､繧ｺ繝ｬ繝吶Ν繧定ｨｭ螳夲ｼ医・繧ｹ繧ｭ繝ｳ繧ｰ蜉ｹ譫懃畑・・        /// </summary>
        void SetEnvironmentNoiseLevel(float level);
        
        /// <summary>
        /// NPC縺ｫ閨槭％縺医ｋ髻ｳ繧堤函謌・        /// </summary>
        void EmitDetectableSound(Vector3 position, float radius, float intensity, string soundType);
        
        /// <summary>
        /// 豕ｨ諢上ｒ蠑輔￥髻ｳ繧貞・逕・        /// </summary>
        void PlayDistraction(Vector3 position, float radius);
        
        /// <summary>
        /// 隴ｦ謌偵Ξ繝吶Ν縺ｫ蠢懊§縺檻GM繧定ｨｭ螳・        /// </summary>
        void SetAlertLevelMusic(AlertLevel level);
        
        /// <summary>
        /// 繧ｪ繝ｼ繝・ぅ繧ｪ繝槭せ繧ｭ繝ｳ繧ｰ蜉ｹ譫懊ｒ驕ｩ逕ｨ
        /// </summary>
        void ApplyAudioMasking(float maskingLevel);
        
        /// <summary>
        /// NPC縺ｮ閨ｴ隕壹そ繝ｳ繧ｵ繝ｼ縺ｫ繧ｵ繧ｦ繝ｳ繝峨う繝吶Φ繝医ｒ騾夂衍
        /// </summary>
        void NotifyAuditorySensors(Vector3 origin, float radius, float intensity);
        
        /// <summary>
        /// 繝励Ξ繧､繝､繝ｼ縺ｮ髫蟇・ｺｦ縺ｫ蠢懊§縺滄浹髻ｿ隱ｿ謨ｴ
        /// </summary>
        void AdjustStealthAudio(float stealthLevel);
        
        /// <summary>
        /// 逶ｮ讓咎＃謌先凾縺ｮ繧ｵ繧ｦ繝ｳ繝峨ｒ蜀咲函
        /// </summary>
        /// <param name="withBonus">繝懊・繝翫せ莉倥″縺九←縺・°</param>
        void PlayObjectiveCompleteSound(bool withBonus);
    }
}
