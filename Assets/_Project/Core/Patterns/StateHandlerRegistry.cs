using System;
using asterivo.Unity60.Core.Services.Interfaces;
using System.Collections.Generic;
// using asterivo.Unity60.Core.Services.Interfaces;
using UnityEngine;

namespace asterivo.Unity60.Core.Patterns
{
    /// <summary>
    /// 迥ｶ諷九ワ繝ｳ繝峨Λ繝ｼ縺ｮ繝ｬ繧ｸ繧ｹ繝医Μ・・actory + Registry 繝代ち繝ｼ繝ｳ・・
    /// ServiceLocator縺ｫ逋ｻ骭ｲ縺励※菴ｿ逕ｨ縺吶ｋ繧ｵ繝ｼ繝薙せ螳溯｣・
    /// </summary>
    public class StateHandlerRegistry : IStateService
    {
        private readonly Dictionary<int, IStateHandler> handlers;
        private bool isInitialized = false;

        public StateHandlerRegistry()
        {
            handlers = new Dictionary<int, IStateHandler>();
        }

        /// <summary>
        /// 繧ｵ繝ｼ繝薙せ縺ｮ蛻晄悄蛹・
        /// </summary>
        public void Initialize()
        {
            if (!isInitialized)
            {
                Debug.Log("[StateHandlerRegistry] Initializing StateService");
                isInitialized = true;
                // Feature螻､縺九ｉ蠢・ｦ√↓蠢懊§縺ｦHandler繧堤匳骭ｲ縺吶ｋ
            }
        }

        /// <summary>
        /// 繧ｵ繝ｼ繝薙せ縺ｮ繧ｷ繝｣繝・ヨ繝繧ｦ繝ｳ
        /// </summary>
        public void Shutdown()
        {
            if (isInitialized)
            {
                Debug.Log("[StateHandlerRegistry] Shutting down StateService");
                ClearHandlers();
                isInitialized = false;
            }
        }
        
        /// <summary>
        /// 迥ｶ諷九ワ繝ｳ繝峨Λ繝ｼ繧堤匳骭ｲ
        /// </summary>
        /// <param name="handler">逋ｻ骭ｲ縺吶ｋ繝上Φ繝峨Λ繝ｼ</param>
        public void RegisterHandler(IStateHandler handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));
                
            handlers[handler.HandledState] = handler;
        }
        
        /// <summary>
        /// 迥ｶ諷九ワ繝ｳ繝峨Λ繝ｼ繧貞叙蠕・
        /// </summary>
        /// <param name="state">蟇ｾ雎｡縺ｮ迥ｶ諷・/param>
        /// <returns>蟇ｾ蠢懊☆繧九ワ繝ｳ繝峨Λ繝ｼ縲∝ｭ伜惠縺励↑縺・ｴ蜷医・null</returns>
        public IStateHandler GetHandler(int state) // Changed from PlayerState to int
        {
            handlers.TryGetValue(state, out IStateHandler handler);
            return handler;
        }
        
        /// <summary>
        /// 謖・ｮ壹＠縺溽憾諷九・繝上Φ繝峨Λ繝ｼ縺檎匳骭ｲ縺輔ｌ縺ｦ縺・ｋ縺九メ繧ｧ繝・け
        /// </summary>
        /// <param name="state">繝√ぉ繝・け縺吶ｋ迥ｶ諷・/param>
        /// <returns>繝上Φ繝峨Λ繝ｼ縺悟ｭ伜惠縺吶ｋ蝣ｴ蜷医・true</returns>
        public bool HasHandler(int state) // Changed from PlayerState to int
        {
            return handlers.ContainsKey(state);
        }
        
        /// <summary>
        /// 逋ｻ骭ｲ縺輔ｌ縺ｦ縺・ｋ蜈ｨ縺ｦ縺ｮ迥ｶ諷九ｒ蜿門ｾ・
        /// </summary>
        /// <returns>逋ｻ骭ｲ貂医∩迥ｶ諷九・繧ｳ繝ｬ繧ｯ繧ｷ繝ｧ繝ｳ</returns>
        public IEnumerable<int> GetRegisteredStates() // Changed from PlayerState to int
        {
            return handlers.Keys;
        }

        /// <summary>
        /// 蜈ｨ縺ｦ縺ｮ繝上Φ繝峨Λ繝ｼ繧偵け繝ｪ繧｢
        /// </summary>
        public void ClearHandlers()
        {
            handlers.Clear();
        }
    }
}

