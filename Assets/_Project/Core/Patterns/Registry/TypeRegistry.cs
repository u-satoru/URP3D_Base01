using System;
using System.Collections.Generic;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace asterivo.Unity60.Core.Patterns.Registry
{
    /// <summary>
    /// 型ベースのレジストリ実装
    /// Factory + Registry パターンの具体実装
    /// </summary>
    /// <typeparam name="TValue">登録する値の型</typeparam>
    public class TypeRegistry<TValue> : ITypeRegistry<TValue>
    {
        private readonly Dictionary<Type, TValue> _registry;
        private readonly bool _allowOverwrite;
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="allowOverwrite">既存の登録を上書きするかどうか</param>
        public TypeRegistry(bool allowOverwrite = true)
        {
            _registry = new Dictionary<Type, TValue>();
            _allowOverwrite = allowOverwrite;
        }
        
        public int Count => _registry.Count;
        
        public void Register(Type key, TValue value)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            
            if (_registry.ContainsKey(key) && !_allowOverwrite)
            {
                throw new InvalidOperationException($"Type {key.Name} is already registered and overwrite is not allowed.");
            }
            
            _registry[key] = value;
        }
        
        public void Register<T>(TValue value)
        {
            Register(typeof(T), value);
        }
        
        public bool Unregister(Type key)
        {
            if (key == null) return false;
            return _registry.Remove(key);
        }
        
        public TValue Get(Type key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            
            if (_registry.TryGetValue(key, out TValue value))
            {
                return value;
            }
            
            throw new KeyNotFoundException($"Type {key.Name} is not registered.");
        }
        
        public TValue Get<T>()
        {
            return Get(typeof(T));
        }
        
        public bool TryGet(Type key, out TValue value)
        {
            value = default(TValue);
            if (key == null) return false;
            
            return _registry.TryGetValue(key, out value);
        }
        
        public bool TryGet<T>(out TValue value)
        {
            return TryGet(typeof(T), out value);
        }
        
        public bool IsRegistered(Type key)
        {
            return key != null && _registry.ContainsKey(key);
        }
        
        public bool IsRegistered<T>()
        {
            return IsRegistered(typeof(T));
        }
        
        public IEnumerable<Type> GetKeys()
        {
            return _registry.Keys;
        }
        
        public IEnumerable<TValue> GetValues()
        {
            return _registry.Values;
        }
        
        public void Clear()
        {
            _registry.Clear();
        }
        
        /// <summary>
        /// デバッグ用：登録状況をログに出力
        /// </summary>
        public void LogRegisteredTypes()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"TypeRegistry<{typeof(TValue).Name}> contains {Count} registered types:");
            foreach (var kvp in _registry)
            {
                UnityEngine.Debug.Log($"  - {kvp.Key.Name} -> {kvp.Value}");
#endif
            }
        }
    }
}
