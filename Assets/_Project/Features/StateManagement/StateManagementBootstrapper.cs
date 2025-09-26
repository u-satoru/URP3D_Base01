using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Patterns;
using UnityEngine;

namespace asterivo.Unity60.Features.StateManagement
{
    /// <summary>
    /// StateManagement讖溯・縺ｮ蛻晄悄蛹悶け繝ｩ繧ｹ
    /// StateHandler縺ｮ逋ｻ骭ｲ縺ｨServiceLocator縺ｸ縺ｮ繧ｵ繝ｼ繝薙せ逋ｻ骭ｲ繧呈球蠖・
    /// </summary>
    public static class StateManagementBootstrapper
    {
        private static bool isInitialized = false;

        /// <summary>
        /// StateManagement讖溯・繧貞・譛溷喧縺吶ｋ
        /// </summary>
        public static void Initialize()
        {
            if (isInitialized)
            {
                Debug.LogWarning("[StateManagementBootstrapper] Already initialized");
                return;
            }

            Debug.Log("[StateManagementBootstrapper] Initializing StateManagement");

            // StateHandlerRegistry繧担erviceLocator縺ｫ逋ｻ骭ｲ
            var stateService = new StateHandlerRegistry();
            ServiceLocator.Register<IStateService>(stateService);

            // StateHandler繧堤匳骭ｲ
            RegisterStateHandlers(stateService);

            isInitialized = true;
            Debug.Log("[StateManagementBootstrapper] Initialization complete");
        }

        /// <summary>
        /// StateHandler繧堤匳骭ｲ縺吶ｋ
        /// </summary>
        private static void RegisterStateHandlers(IStateService stateService)
        {
            // 蝓ｺ譛ｬ迥ｶ諷九・繝上Φ繝峨Λ繝ｼ逋ｻ骭ｲ
            stateService.RegisterHandler(new IdleStateHandler());
            stateService.RegisterHandler(new WalkingStateHandler());
            stateService.RegisterHandler(new RunningStateHandler());
            stateService.RegisterHandler(new SprintingStateHandler());

            // 遨ｺ荳ｭ迥ｶ諷九・繝上Φ繝峨Λ繝ｼ逋ｻ骭ｲ
            stateService.RegisterHandler(new JumpingStateHandler());
            stateService.RegisterHandler(new FallingStateHandler());
            stateService.RegisterHandler(new LandingStateHandler());

            // 謌ｦ髣倡憾諷九・繝上Φ繝峨Λ繝ｼ逋ｻ骭ｲ
            stateService.RegisterHandler(new CombatStateHandler());
            stateService.RegisterHandler(new CombatAttackingStateHandler());

            // 迚ｹ谿顔憾諷九・繝上Φ繝峨Λ繝ｼ逋ｻ骭ｲ
            stateService.RegisterHandler(new InteractingStateHandler());
            stateService.RegisterHandler(new DeadStateHandler());

            Debug.Log($"[StateManagementBootstrapper] Registered {stateService.GetRegisteredStates()} state handlers");
        }

        /// <summary>
        /// StateManagement讖溯・繧偵す繝｣繝・ヨ繝繧ｦ繝ｳ縺吶ｋ
        /// </summary>
        public static void Shutdown()
        {
            if (!isInitialized)
            {
                return;
            }

            Debug.Log("[StateManagementBootstrapper] Shutting down StateManagement");

            if (ServiceLocator.TryGet<IStateService>(out var stateService))
            {
                stateService.Shutdown();
            }

            isInitialized = false;
        }
    }
}


