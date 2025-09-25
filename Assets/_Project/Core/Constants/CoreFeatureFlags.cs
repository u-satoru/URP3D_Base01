using asterivo.Unity60.Core;

namespace asterivo.Unity60.Core
{
    /// <summary>
    /// Core層からのFeatureFlags参照を集約するブリッジ。
    /// 実体は asterivo.Unity60.Core.FeatureFlags への委譲。
    /// </summary>
    public static class CoreFeatureFlags
    {
        public static bool UseServiceLocator => FeatureFlags.UseServiceLocator;
        public static bool EnableDebugLogging => FeatureFlags.EnableDebugLogging;
    }
}


