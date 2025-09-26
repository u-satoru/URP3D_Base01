using UnityEngine;
using asterivo.Unity60.Core;
using asterivo.Unity60.Features.ActionRPG.Services;

namespace asterivo.Unity60.Features.ActionRPG
{
    /// <summary>
    /// ActionRPG讖溯・縺ｮ繝悶・繝医せ繝医Λ繝・ヱ繝ｼ
    /// ServiceLocator縺ｸ縺ｮ繧ｵ繝ｼ繝薙せ逋ｻ骭ｲ縺ｨ蛻晄悄蛹悶ｒ諡・ｽ・
    /// </summary>
    public static class ActionRPGBootstrapper
    {
        private static bool _isInitialized = false;
        private static ActionRPGServiceRegistry _serviceRegistry;

        /// <summary>
        /// ActionRPG讖溯・繧貞・譛溷喧
        /// </summary>
        public static void Initialize()
        {
            if (_isInitialized)
            {
                Debug.LogWarning("ActionRPGBootstrapper: 譌｢縺ｫ蛻晄悄蛹悶＆繧後※縺・∪縺・);
                return;
            }

            Debug.Log("ActionRPGBootstrapper: 蛻晄悄蛹夜幕蟋・);

            // 繧ｵ繝ｼ繝薙せ繧剃ｽ懈・縺励※逋ｻ骭ｲ
            _serviceRegistry = new ActionRPGServiceRegistry();
            ServiceLocator.Register<IActionRPGService>(_serviceRegistry);

            _isInitialized = true;
            Debug.Log("ActionRPGBootstrapper: 蛻晄悄蛹門ｮ御ｺ・- ActionRPG繧ｵ繝ｼ繝薙せ縺郡erviceLocator縺ｫ逋ｻ骭ｲ縺輔ｌ縺ｾ縺励◆");
        }

        /// <summary>
        /// ActionRPG讖溯・繧偵す繝｣繝・ヨ繝繧ｦ繝ｳ
        /// </summary>
        public static void Shutdown()
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("ActionRPGBootstrapper: 蛻晄悄蛹悶＆繧後※縺・∪縺帙ｓ");
                return;
            }

            Debug.Log("ActionRPGBootstrapper: 繧ｷ繝｣繝・ヨ繝繧ｦ繝ｳ髢句ｧ・);

            // ServiceLocator縺九ｉ隗｣髯､
            if (ServiceLocator.TryGet<IActionRPGService>(out var service))
            {
                service.Shutdown();
                // ServiceLocator縺ｫ隗｣髯､繝｡繧ｽ繝・ラ縺後≠繧後・蜻ｼ縺ｳ蜃ｺ縺・
                // ServiceLocator.Unregister<IActionRPGService>();
            }

            _serviceRegistry = null;
            _isInitialized = false;
            Debug.Log("ActionRPGBootstrapper: 繧ｷ繝｣繝・ヨ繝繧ｦ繝ｳ螳御ｺ・);
        }

        /// <summary>
        /// 迴ｾ蝨ｨ縺ｮ繧ｵ繝ｼ繝薙せ繧､繝ｳ繧ｹ繧ｿ繝ｳ繧ｹ繧貞叙蠕・
        /// </summary>
        public static ActionRPGServiceRegistry GetServiceRegistry()
        {
            return _serviceRegistry;
        }

        /// <summary>
        /// 蛻晄悄蛹也憾諷九ｒ蜿門ｾ・
        /// </summary>
        public static bool IsInitialized => _isInitialized;
    }
}


