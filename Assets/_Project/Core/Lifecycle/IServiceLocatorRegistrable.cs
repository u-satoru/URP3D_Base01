namespace _Project.Core
{
    /// <summary>
    /// ServiceLocatorへの登録/解除を標準化するためのインターフェース。
    /// SystemInitializerが起動時にPriority順でRegister、破棄時に逆順でUnregisterを呼び出します。
    /// </summary>
    public interface IServiceLocatorRegistrable
    {
        int Priority { get; }
        void RegisterServices();
        void UnregisterServices();
    }
}

