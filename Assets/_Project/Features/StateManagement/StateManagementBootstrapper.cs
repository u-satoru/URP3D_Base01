using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Core.Patterns;
using UnityEngine;

namespace asterivo.Unity60.Features.StateManagement
{
    /// <summary>
    /// StateManagement機能の初期化クラス
    /// StateHandlerの登録とServiceLocatorへのサービス登録を担当
    /// </summary>
    public static class StateManagementBootstrapper
    {
        private static bool isInitialized = false;

        /// <summary>
        /// StateManagement機能を初期化する
        /// </summary>
        public static void Initialize()
        {
            if (isInitialized)
            {
                Debug.LogWarning("[StateManagementBootstrapper] Already initialized");
                return;
            }

            Debug.Log("[StateManagementBootstrapper] Initializing StateManagement");

            // StateHandlerRegistryをServiceLocatorに登録
            var stateService = new StateHandlerRegistry();
            ServiceLocator.Register<IStateService>(stateService);

            // StateHandlerを登録
            RegisterStateHandlers(stateService);

            isInitialized = true;
            Debug.Log("[StateManagementBootstrapper] Initialization complete");
        }

        /// <summary>
        /// StateHandlerを登録する
        /// </summary>
        private static void RegisterStateHandlers(IStateService stateService)
        {
            // 基本状態のハンドラー登録
            stateService.RegisterHandler(new IdleStateHandler());
            stateService.RegisterHandler(new WalkingStateHandler());
            stateService.RegisterHandler(new RunningStateHandler());
            stateService.RegisterHandler(new SprintingStateHandler());

            // 空中状態のハンドラー登録
            stateService.RegisterHandler(new JumpingStateHandler());
            stateService.RegisterHandler(new FallingStateHandler());
            stateService.RegisterHandler(new LandingStateHandler());

            // 戦闘状態のハンドラー登録
            stateService.RegisterHandler(new CombatStateHandler());
            stateService.RegisterHandler(new CombatAttackingStateHandler());

            // 特殊状態のハンドラー登録
            stateService.RegisterHandler(new InteractingStateHandler());
            stateService.RegisterHandler(new DeadStateHandler());

            Debug.Log($"[StateManagementBootstrapper] Registered {stateService.GetRegisteredStates()} state handlers");
        }

        /// <summary>
        /// StateManagement機能をシャットダウンする
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
