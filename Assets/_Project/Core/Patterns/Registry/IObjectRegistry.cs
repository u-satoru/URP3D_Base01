using System;
using System.Collections.Generic;

namespace asterivo.Unity60.Core.Patterns.Registry
{
    /// <summary>
    /// オブジェクトレジストリの基本インターフェース
    /// Factory + Registry パターンの Registry 部分
    /// </summary>
    /// <typeparam name="TKey">キーの型</typeparam>
    /// <typeparam name="TValue">値の型</typeparam>
    public interface IObjectRegistry<TKey, TValue>
    {
        /// <summary>
        /// オブジェクトを登録します
        /// </summary>
        /// <param name="key">キー</param>
        /// <param name="value">値</param>
        void Register(TKey key, TValue value);
        
        /// <summary>
        /// オブジェクトの登録を解除します
        /// </summary>
        /// <param name="key">キー</param>
        /// <returns>登録解除に成功した場合true</returns>
        bool Unregister(TKey key);
        
        /// <summary>
        /// 登録されたオブジェクトを取得します
        /// </summary>
        /// <param name="key">キー</param>
        /// <returns>登録されたオブジェクト</returns>
        TValue Get(TKey key);
        
        /// <summary>
        /// 登録されたオブジェクトを安全に取得します
        /// </summary>
        /// <param name="key">キー</param>
        /// <param name="value">取得されたオブジェクト</param>
        /// <returns>取得に成功した場合true</returns>
        bool TryGet(TKey key, out TValue value);
        
        /// <summary>
        /// 指定されたキーが登録されているかチェックします
        /// </summary>
        /// <param name="key">キー</param>
        /// <returns>登録されている場合true</returns>
        bool IsRegistered(TKey key);
        
        /// <summary>
        /// 登録されている全てのキーを取得します
        /// </summary>
        /// <returns>キーのコレクション</returns>
        IEnumerable<TKey> GetKeys();
        
        /// <summary>
        /// 登録されている全ての値を取得します
        /// </summary>
        /// <returns>値のコレクション</returns>
        IEnumerable<TValue> GetValues();
        
        /// <summary>
        /// レジストリをクリアします
        /// </summary>
        void Clear();
        
        /// <summary>
        /// 登録されているアイテムの数を取得します
        /// </summary>
        int Count { get; }
    }
    
    /// <summary>
    /// 型ベースのレジストリインターフェース
    /// </summary>
    public interface ITypeRegistry<TValue> : IObjectRegistry<Type, TValue>
    {
        /// <summary>
        /// 型を指定してオブジェクトを登録します
        /// </summary>
        /// <typeparam name="T">登録する型</typeparam>
        /// <param name="value">値</param>
        void Register<T>(TValue value);
        
        /// <summary>
        /// 型を指定してオブジェクトを取得します
        /// </summary>
        /// <typeparam name="T">取得する型</typeparam>
        /// <returns>登録されたオブジェクト</returns>
        TValue Get<T>();
        
        /// <summary>
        /// 型を指定してオブジェクトを安全に取得します
        /// </summary>
        /// <typeparam name="T">取得する型</typeparam>
        /// <param name="value">取得されたオブジェクト</param>
        /// <returns>取得に成功した場合true</returns>
        bool TryGet<T>(out TValue value);
        
        /// <summary>
        /// 指定された型が登録されているかチェックします
        /// </summary>
        /// <typeparam name="T">チェックする型</typeparam>
        /// <returns>登録されている場合true</returns>
        bool IsRegistered<T>();
    }
}