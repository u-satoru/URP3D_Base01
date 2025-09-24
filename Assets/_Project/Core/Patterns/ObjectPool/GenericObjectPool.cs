using System;
using System.Collections.Generic;
using UnityEngine;

namespace asterivo.Unity60.Core.Patterns.ObjectPool
{
    /// <summary>
    /// ジェネリックオブジェクトプールの実装
    /// Factory + Strategy パターンを組み合わせた柔軟なプール管理
    /// </summary>
    /// <typeparam name="T">プール対象のオブジェクト型</typeparam>
    public class GenericObjectPool<T> : IObjectPool<T>
    {
        private readonly Queue<T> _pool;
        private readonly Func<T> _factory;
        private readonly Action<T> _onGet;
        private readonly Action<T> _onReturn;
        private readonly int _maxSize;
        
        // 統計情報
        private PoolStatistics _statistics;
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="factory">オブジェクト生成ファクトリ</param>
        /// <param name="onGet">取得時のコールバック</param>
        /// <param name="onReturn">返却時のコールバック</param>
        /// <param name="maxSize">プールの最大サイズ</param>
        public GenericObjectPool(
            Func<T> factory,
            Action<T> onGet = null,
            Action<T> onReturn = null,
            int maxSize = 100)
        {
            _pool = new Queue<T>();
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _onGet = onGet;
            _onReturn = onReturn;
            _maxSize = maxSize;
            _statistics = new PoolStatistics();
        }
        
        public T Get()
        {
            T obj;
            
            if (_pool.Count > 0)
            {
                obj = _pool.Dequeue();
                _statistics.TotalReused++;
            }
            else
            {
                obj = _factory();
                _statistics.TotalCreated++;
            }
            
            _onGet?.Invoke(obj);
            return obj;
        }
        
        public void Return(T obj)
        {
            if (obj == null) return;
            
            _onReturn?.Invoke(obj);
            
            if (_pool.Count < _maxSize)
            {
                _pool.Enqueue(obj);
                _statistics.TotalReturned++;
            }
            // プールが満杯の場合は自然にGCに委ねる
        }
        
        public void Prewarm(int count)
        {
            for (int i = 0; i < count && _pool.Count < _maxSize; i++)
            {
                var obj = _factory();
                _pool.Enqueue(obj);
                _statistics.TotalCreated++;
            }
        }
        
        public void Clear()
        {
            _pool.Clear();
        }
        
        public PoolStatistics GetStatistics()
        {
            _statistics.CurrentInPool = _pool.Count;
            return _statistics;
        }
    }
}