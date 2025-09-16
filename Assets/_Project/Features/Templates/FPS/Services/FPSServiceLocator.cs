using UnityEngine;

namespace asterivo.Unity60.Features.Templates.FPS.Services
{
    /// <summary>
    /// FPS Template専用ServiceLocator拡張
    /// Core ServiceLocatorを基盤として活用
    /// </summary>
    public static class FPSServiceLocator
    {
        // Core ServiceLocatorを基盤として活用
        public static T GetService<T>() where T : class
            => asterivo.Unity60.Core.ServiceLocator.GetService<T>();

        // FPS専用サービス登録
        public static void RegisterFPSServices()
        {
            RegisterService<IWeaponManager>(new WeaponManager());
            RegisterService<ICombatManager>(new CombatManager());
            RegisterService<IFPSCameraService>(new FPSCameraService());
            RegisterService<IFPSInputService>(new FPSInputService());
            RegisterService<IFPSUIService>(new FPSUIService());
            RegisterService<IAmmoManager>(new AmmoManager());
            RegisterService<ITargetingService>(new TargetingService());
            RegisterService<IEffectsService>(new EffectsService());
        }

        private static void RegisterService<T>(T service) where T : class
        {
            asterivo.Unity60.Core.ServiceLocator.RegisterService<T>(service);
        }

        // サービス登録解除（デバッグ・テスト用）
        public static void UnregisterFPSServices()
        {
            asterivo.Unity60.Core.ServiceLocator.UnregisterService<IWeaponManager>();
            asterivo.Unity60.Core.ServiceLocator.UnregisterService<ICombatManager>();
            asterivo.Unity60.Core.ServiceLocator.UnregisterService<IFPSCameraService>();
            asterivo.Unity60.Core.ServiceLocator.UnregisterService<IFPSInputService>();
            asterivo.Unity60.Core.ServiceLocator.UnregisterService<IFPSUIService>();
            asterivo.Unity60.Core.ServiceLocator.UnregisterService<IAmmoManager>();
            asterivo.Unity60.Core.ServiceLocator.UnregisterService<ITargetingService>();
            asterivo.Unity60.Core.ServiceLocator.UnregisterService<IEffectsService>();
        }

        // FPS専用サービス確認
        public static bool HasFPSServices()
        {
            return asterivo.Unity60.Core.ServiceLocator.HasService<IWeaponManager>() &&
                   asterivo.Unity60.Core.ServiceLocator.HasService<ICombatManager>() &&
                   asterivo.Unity60.Core.ServiceLocator.HasService<IFPSCameraService>();
        }
    }
}