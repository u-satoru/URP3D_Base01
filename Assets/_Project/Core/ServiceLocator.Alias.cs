using System;

namespace asterivo.Unity60.Core
{
    /// <summary>
    /// ServiceLocatorの互換性エイリアスメソッド
    /// 既存コードとの後方互換性を保つために必要なメソッドのエイリアス実装
    /// </summary>
    public static partial class ServiceLocator
    {
        // ========================================
        // 既存コードとの互換性のためのエイリアスメソッド
        // ========================================

        /// <summary>
        /// サービスを登録（RegisterServiceのエイリアス）
        /// </summary>
        /// <typeparam name="T">サービスの型</typeparam>
        /// <param name="service">登録するサービスインスタンス</param>
        public static void Register<T>(T service) where T : class
        {
            RegisterService<T>(service);
        }

        /// <summary>
        /// サービスを取得（GetServiceのエイリアス）
        /// </summary>
        /// <typeparam name="T">サービスの型</typeparam>
        /// <returns>サービスインスタンス（見つからない場合はnull）</returns>
        public static T Get<T>() where T : class
        {
            return GetService<T>();
        }

        /// <summary>
        /// サービスの取得を試みる（outパラメータ版）
        /// </summary>
        /// <typeparam name="T">サービスの型</typeparam>
        /// <param name="service">サービスインスタンス（取得成功時）</param>
        /// <returns>取得成功時はtrue、失敗時はfalse</returns>
        public static bool TryGet<T>(out T service) where T : class
        {
            service = GetService<T>();
            return service != null;
        }

        /// <summary>
        /// サービスを取得（取得できない場合は例外）（RequireServiceのエイリアス）
        /// </summary>
        /// <typeparam name="T">サービスの型</typeparam>
        /// <returns>サービスインスタンス</returns>
        /// <exception cref="InvalidOperationException">サービスが見つからない場合</exception>
        public static T Require<T>() where T : class
        {
            return RequireService<T>();
        }

        /// <summary>
        /// サービスが登録されているか確認（HasServiceのエイリアス）
        /// </summary>
        /// <typeparam name="T">サービスの型</typeparam>
        /// <returns>登録されている場合はtrue</returns>
        public static bool Has<T>() where T : class
        {
            return HasService<T>();
        }

        /// <summary>
        /// サービスを削除（UnregisterServiceのエイリアス）
        /// </summary>
        /// <typeparam name="T">サービスの型</typeparam>
        public static void Unregister<T>() where T : class
        {
            UnregisterService<T>();
        }
    }
}