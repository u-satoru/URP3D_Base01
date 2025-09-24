namespace asterivo.Unity60.Core.Services
{
    /// <summary>
    /// Base interface for all services in the Service Locator pattern
    /// Provides a common contract for service lifecycle management
    /// </summary>
    public interface IService
    {
        /// <summary>
        /// Called when the service is first registered with the ServiceLocator
        /// Override this to perform initialization logic
        /// </summary>
        void OnServiceRegistered() { }

        /// <summary>
        /// Called when the service is unregistered from the ServiceLocator
        /// Override this to perform cleanup logic
        /// </summary>
        void OnServiceUnregistered() { }

        /// <summary>
        /// Gets whether this service is currently active and ready to use
        /// </summary>
        bool IsServiceActive => true;

        /// <summary>
        /// Gets a descriptive name for this service (used for debugging)
        /// </summary>
        string ServiceName => GetType().Name;
    }

    /// <summary>
    /// Generic service interface with typed service identification
    /// </summary>
    /// <typeparam name="T">The service type</typeparam>
    public interface IService<T> : IService where T : class, IService<T>
    {
        /// <summary>
        /// Gets the singleton instance of this service
        /// This is primarily used for services that need to reference themselves
        /// </summary>
        T Instance => this as T;
    }

    /// <summary>
    /// Interface for services that need to be initialized with configuration data
    /// </summary>
    /// <typeparam name="TConfig">The configuration data type</typeparam>
    public interface IConfigurableService<TConfig> : IService where TConfig : class
    {
        /// <summary>
        /// Initialize the service with configuration data
        /// </summary>
        /// <param name="config">The configuration data</param>
        void Initialize(TConfig config);

        /// <summary>
        /// Gets whether this service has been initialized
        /// </summary>
        bool IsInitialized { get; }
    }

    /// <summary>
    /// Interface for services that need to be disposed properly
    /// </summary>
    public interface IDisposableService : IService, System.IDisposable
    {
        /// <summary>
        /// Gets whether this service has been disposed
        /// </summary>
        bool IsDisposed { get; }
    }

    /// <summary>
    /// Interface for services that need update lifecycle management
    /// </summary>
    public interface IUpdatableService : IService
    {
        /// <summary>
        /// Called every frame to update the service
        /// </summary>
        void UpdateService();

        /// <summary>
        /// Gets whether this service needs to be updated every frame
        /// </summary>
        bool NeedsUpdate { get; }

        /// <summary>
        /// Gets the update priority (lower values update first)
        /// </summary>
        int UpdatePriority => 0;
    }

    /// <summary>
    /// Interface for services that can be paused/resumed
    /// </summary>
    public interface IPausableService : IService
    {
        /// <summary>
        /// Pause the service
        /// </summary>
        void Pause();

        /// <summary>
        /// Resume the service
        /// </summary>
        void Resume();

        /// <summary>
        /// Gets whether this service is currently paused
        /// </summary>
        bool IsPaused { get; }
    }
}