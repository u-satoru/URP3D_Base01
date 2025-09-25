using UnityEngine;

namespace asterivo.Unity60.Core.Services
{
    /// <summary>
    /// Interface for object pool management service
    /// Provides centralized object pooling for performance optimization
    /// </summary>
    public interface IPoolManager : IService
    {
        /// <summary>
        /// Set maximum pool size for all pools
        /// </summary>
        /// <param name="maxSize">Maximum number of objects per pool</param>
        void SetMaxPoolSize(int maxSize);

        /// <summary>
        /// Get maximum pool size
        /// </summary>
        int GetMaxPoolSize();

        /// <summary>
        /// Get an object from the pool
        /// </summary>
        /// <typeparam name="T">Type of object to get</typeparam>
        /// <param name="prefab">Prefab to instantiate if pool is empty</param>
        /// <returns>Pooled object instance</returns>
        T Get<T>(T prefab) where T : Component;

        /// <summary>
        /// Return an object to the pool
        /// </summary>
        /// <typeparam name="T">Type of object to return</typeparam>
        /// <param name="obj">Object to return to pool</param>
        void Return<T>(T obj) where T : Component;

        /// <summary>
        /// Get an object from the pool by name
        /// </summary>
        /// <param name="poolName">Name of the pool</param>
        /// <param name="prefab">Prefab to instantiate if pool is empty</param>
        /// <returns>Pooled object instance</returns>
        GameObject Get(string poolName, GameObject prefab);

        /// <summary>
        /// Return an object to the pool by name
        /// </summary>
        /// <param name="poolName">Name of the pool</param>
        /// <param name="obj">Object to return to pool</param>
        void Return(string poolName, GameObject obj);

        /// <summary>
        /// Create a new pool with specific settings
        /// </summary>
        /// <param name="poolName">Name of the pool</param>
        /// <param name="prefab">Prefab for the pool</param>
        /// <param name="initialSize">Initial number of objects to create</param>
        /// <param name="maxSize">Maximum size of the pool</param>
        void CreatePool(string poolName, GameObject prefab, int initialSize = 10, int maxSize = 100);

        /// <summary>
        /// Destroy a pool and all its objects
        /// </summary>
        /// <param name="poolName">Name of the pool to destroy</param>
        void DestroyPool(string poolName);

        /// <summary>
        /// Clear all objects from a pool
        /// </summary>
        /// <param name="poolName">Name of the pool to clear</param>
        void ClearPool(string poolName);

        /// <summary>
        /// Clear all pools
        /// </summary>
        void ClearAllPools();

        /// <summary>
        /// Get the number of available objects in a pool
        /// </summary>
        /// <param name="poolName">Name of the pool</param>
        /// <returns>Number of available objects</returns>
        int GetAvailableCount(string poolName);

        /// <summary>
        /// Get the total number of objects in a pool
        /// </summary>
        /// <param name="poolName">Name of the pool</param>
        /// <returns>Total number of objects</returns>
        int GetTotalCount(string poolName);

        /// <summary>
        /// Check if a pool exists
        /// </summary>
        /// <param name="poolName">Name of the pool</param>
        /// <returns>True if pool exists</returns>
        bool HasPool(string poolName);

        /// <summary>
        /// Get pool statistics for debugging
        /// </summary>
        /// <returns>Pool statistics as formatted string</returns>
        string GetPoolStatistics();

        /// <summary>
        /// Preload objects into a pool
        /// </summary>
        /// <param name="poolName">Name of the pool</param>
        /// <param name="count">Number of objects to preload</param>
        void PreloadPool(string poolName, int count);
    }
}
