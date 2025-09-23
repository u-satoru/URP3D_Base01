using UnityEngine;
// using asterivo.Unity60.Core.Components;

namespace asterivo.Unity60.Core.Commands
{
    /// <summary>
    /// 蟇ｾ雎｡縺ｮ菴灘鴨繧貞屓蠕ｩ縺輔○繧九さ繝槭Φ繝牙ｮ溯｣・・    /// IResettableCommand繧貞ｮ溯｣・＠縺ｦ縺翫ｊ縲＾bjectPool縺ｫ繧医ｋ蜀榊茜逕ｨ縺ｫ蟇ｾ蠢懊＠縺ｦ縺・∪縺吶・    /// 
    /// 荳ｻ縺ｪ讖溯・・・    /// - 菴灘鴨縺ｮ蝗槫ｾｩ蜃ｦ逅・・螳溯｡・    /// - Undo謫堺ｽ懊↓繧医ｋ繝繝｡繝ｼ繧ｸ縺ｮ驕ｩ逕ｨ・亥屓蠕ｩ縺ｮ蜿悶ｊ豸医＠・・    /// - ObjectPool菴ｿ逕ｨ譎ゅ・迥ｶ諷九Μ繧ｻ繝・ヨ讖溯・
    /// - 繝代Λ繝｡繝ｼ繧ｿ繝ｼ縺ｫ繧医ｋ譟碑ｻ溘↑蛻晄悄蛹・    /// </summary>
    public class HealCommand : IResettableCommand
    {
        /// <summary>
        /// 蝗槫ｾｩ蜃ｦ逅・・蟇ｾ雎｡縺ｨ縺ｪ繧九・繝ｫ繧ｹ繧ｿ繝ｼ繧ｲ繝・ヨ
        /// </summary>
        private IHealthTarget _target;
        
        /// <summary>
        /// 蝗槫ｾｩ縺吶ｋ菴灘鴨縺ｮ驥・        /// </summary>
        private int _healAmount;

        /// <summary>
        /// 縺薙・繧ｳ繝槭Φ繝峨′Undo謫堺ｽ懊ｒ繧ｵ繝昴・繝医☆繧九°縺ｩ縺・°繧堤､ｺ縺励∪縺吶・        /// 蝗槫ｾｩ繧ｳ繝槭Φ繝峨・蟶ｸ縺ｫUndo蜿ｯ閭ｽ・医ム繝｡繝ｼ繧ｸ縺ｫ螟画鋤・峨〒縺吶・        /// </summary>
        public bool CanUndo => true; 

        /// <summary>
        /// 繝励・繝ｫ蛹門ｯｾ蠢懊・繝・ヵ繧ｩ繝ｫ繝医さ繝ｳ繧ｹ繝医Λ繧ｯ繧ｿ縲・        /// ObjectPool菴ｿ逕ｨ譎ゅ↓蠢・ｦ√↑蠑墓焚縺ｪ縺励さ繝ｳ繧ｹ繝医Λ繧ｯ繧ｿ縺ｧ縺吶・        /// 螳滄圀縺ｮ繝代Λ繝｡繝ｼ繧ｿ縺ｯ蠕後〒Initialize()繝｡繧ｽ繝・ラ縺ｧ險ｭ螳壹＠縺ｾ縺吶・        /// </summary>
        public HealCommand()
        {
            // 繝励・繝ｫ蛹門ｯｾ蠢懶ｼ壹ヱ繝ｩ繝｡繝ｼ繧ｿ繝ｼ縺ｪ縺励さ繝ｳ繧ｹ繝医Λ繧ｯ繧ｿ
        }

        /// <summary>
        /// 繝代Λ繝｡繝ｼ繧ｿ繝ｼ莉倥″繧ｳ繝ｳ繧ｹ繝医Λ繧ｯ繧ｿ縲ら峩謗･繧､繝ｳ繧ｹ繧ｿ繝ｳ繧ｹ蛹匁凾縺ｫ菴ｿ逕ｨ縺輔ｌ縺ｾ縺吶・        /// </summary>
        /// <param name="target">蝗槫ｾｩ蟇ｾ雎｡縺ｮ繝倥Ν繧ｹ繧ｿ繝ｼ繧ｲ繝・ヨ</param>
        /// <param name="healAmount">蝗槫ｾｩ縺吶ｋ菴灘鴨驥擾ｼ域ｭ｣縺ｮ蛟､・・/param>
        public HealCommand(IHealthTarget target, int healAmount)
        {
            _target = target;
            _healAmount = healAmount;
        }

        /// <summary>
        /// 蝗槫ｾｩ繧ｳ繝槭Φ繝峨ｒ螳溯｡後＠縺ｾ縺吶・        /// 蟇ｾ雎｡縺ｮIHealthTarget縺ｫ蟇ｾ縺励※Heal()繝｡繧ｽ繝・ラ繧貞他縺ｳ蜃ｺ縺励∵欠螳壹＆繧後◆驥上・菴灘鴨繧貞屓蠕ｩ縺輔○縺ｾ縺吶・        /// </summary>
        public void Execute()
        {
            if (_target == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.LogWarning("HealCommand: Target is null, cannot execute heal");
#endif
                return;
            }
            
            _target.Heal(_healAmount);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Healed {_healAmount} health");
#endif
        }
        

        /// <summary>
        /// 繧ｳ繝槭Φ繝峨・迥ｶ諷九ｒ繝ｪ繧ｻ繝・ヨ縺励＾bjectPool縺ｫ霑泌唆縺吶ｋ貅門ｙ繧偵＠縺ｾ縺吶・        /// IResettableCommand縺ｮ螳溯｣・→縺励※縲√・繝ｼ繝ｫ蛹悶＆繧後◆髫帙・蜀榊茜逕ｨ蜑阪↓蜻ｼ縺ｳ蜃ｺ縺輔ｌ縺ｾ縺吶・        /// </summary>
        public void Reset()
        {
            _target = null;
            _healAmount = 0;
        }

        /// <summary>
        /// ObjectPool菴ｿ逕ｨ譎ゅ↓譁ｰ縺励＞繝代Λ繝｡繝ｼ繧ｿ繝ｼ縺ｧ繧ｳ繝槭Φ繝峨ｒ蛻晄悄蛹悶＠縺ｾ縺吶・        /// IResettableCommand縺ｮ螳溯｣・→縺励※縲√・繝ｼ繝ｫ縺九ｉ縺ｮ蜿門ｾ玲凾縺ｫ蜻ｼ縺ｳ蜃ｺ縺輔ｌ縺ｾ縺吶・        /// </summary>
        /// <param name="parameters">蛻晄悄蛹悶ヱ繝ｩ繝｡繝ｼ繧ｿ繝ｼ驟榊・縲・0]=IHealthTarget, [1]=int・亥屓蠕ｩ驥擾ｼ・/param>
        public void Initialize(params object[] parameters)
        {
            if (parameters.Length < 2)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.LogError("HealCommand.Initialize: 譛菴・縺､縺ｮ繝代Λ繝｡繝ｼ繧ｿ繝ｼ・・arget, healAmount・峨′蠢・ｦ√〒縺吶・);
#endif
                return;
            }

            _target = parameters[0] as IHealthTarget;
            if (_target == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.LogError("HealCommand.Initialize: 譛蛻昴・繝代Λ繝｡繝ｼ繧ｿ繝ｼ縺ｯIHealthTarget縺ｧ縺ゅｋ蠢・ｦ√′縺ゅｊ縺ｾ縺吶・);
#endif
                return;
            }

            if (parameters[1] is int healAmount)
            {
                _healAmount = healAmount;
            }
            else
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.LogError("HealCommand.Initialize: 2逡ｪ逶ｮ縺ｮ繝代Λ繝｡繝ｼ繧ｿ繝ｼ縺ｯint・亥屓蠕ｩ驥擾ｼ峨〒縺ゅｋ蠢・ｦ√′縺ゅｊ縺ｾ縺吶・);
#endif
                return;
            }
        }
        
        /// <summary>
        /// 繧医ｊ蝙句ｮ牙・縺ｪ蛻晄悄蛹悶Γ繧ｽ繝・ラ縲・        /// object[]繧剃ｽｿ逕ｨ縺吶ｋ豎守畑迚医→逡ｰ縺ｪ繧翫∝梛螳牙・諤ｧ縺御ｿ晁ｨｼ縺輔ｌ縺ｦ縺・∪縺吶・        /// </summary>
        /// <param name="target">蝗槫ｾｩ蟇ｾ雎｡縺ｮ繝倥Ν繧ｹ繧ｿ繝ｼ繧ｲ繝・ヨ</param>
        /// <param name="healAmount">蝗槫ｾｩ縺吶ｋ菴灘鴨驥擾ｼ域ｭ｣縺ｮ蛟､・・/param>
        public void Initialize(IHealthTarget target, int healAmount)
        {
            _target = target;
            _healAmount = healAmount;
        }
        
        /// <summary>
        /// 蝗槫ｾｩ繧ｳ繝槭Φ繝峨ｒ蜿悶ｊ豸医＠縺ｾ縺呻ｼ・ndo・峨・        /// 蝗槫ｾｩ縺励◆驥上→蜷後§繝繝｡繝ｼ繧ｸ繧貞ｯｾ雎｡縺ｫ荳弱∴繧九％縺ｨ縺ｧ縲∝屓蠕ｩ繧貞叙繧頑ｶ医＠縺ｾ縺吶・        /// </summary>
        public void Undo()
        {
            if (_target == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.LogWarning("HealCommand: Target is null, cannot undo heal");
#endif
                return;
            }
            
            _target.TakeDamage(_healAmount, "healing_undo");
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Undid {_healAmount} healing (dealt damage)");
#endif
        }
    }
}