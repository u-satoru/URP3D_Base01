using System;

namespace asterivo.Unity60.Core.Patterns.ObjectPool
{
    /// <summary>
    /// オブジェクトプールの基本インターフェース
    /// Strategy パターンで異なるプール戦略を実装できます
    /// </summary>
    /// <typeparam name="T">プール対象のオブジェクト型</typeparam>
    public interface IObjectPool<T>
    {
        /// <summary>
        /// プールからオブジェクトを取得します
        /// </summary>
        /// <returns>プールから取得されたオブジェクト</returns>
        T Get();
        
        /// <summary>
        /// オブジェクトをプールに返却します
        /// </summary>
        /// <param name="obj">返却するオブジェクト</param>
        void Return(T obj);
        
        /// <summary>
        /// プールを事前にウォームアップします
        /// </summary>
        /// <param name="count">事前作成する数量</param>
        void Prewarm(int count);
        
        /// <summary>
        /// プールをクリアします
        /// </summary>
        void Clear();
        
        /// <summary>
        /// プールの統計情報を取得します
        /// </summary>
        /// <returns>統計情報</returns>
        PoolStatistics GetStatistics();
    }
    
    /// <summary>
    /// プールの統計情報
    /// </summary>
    public struct PoolStatistics
    {
        public int TotalCreated;      // 累計作成数
        public int CurrentInPool;     // プール内現在数
        public int TotalReturned;     // 累計返却数
        public int TotalReused;       // 累計再利用数
        public float ReuseRatio => TotalCreated > 0 ? (float)TotalReused / TotalCreated : 0f;
    }
}
