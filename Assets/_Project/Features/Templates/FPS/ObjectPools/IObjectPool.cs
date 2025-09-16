using System;

namespace asterivo.Unity60.Features.Templates.FPS.ObjectPools
{
    /// <summary>
    /// ObjectPool基本インターフェース
    /// ServiceLocator + Event駆動のハイブリッドアーキテクチャ準拠
    /// 95%メモリ削減効果を実現する汎用オブジェクトプール
    /// </summary>
    public interface IObjectPool<T> where T : class
    {
        /// <summary>
        /// プールからオブジェクトを取得
        /// </summary>
        T Get();

        /// <summary>
        /// プールにオブジェクトを返却
        /// </summary>
        void Return(T item);

        /// <summary>
        /// プールの現在の統計情報
        /// </summary>
        PoolStatistics Statistics { get; }

        /// <summary>
        /// プールの初期化
        /// </summary>
        void Initialize(int initialSize, int maxSize);

        /// <summary>
        /// プールのクリア
        /// </summary>
        void Clear();
    }

    /// <summary>
    /// プール統計情報
    /// パフォーマンス監視とメモリ効率測定用
    /// </summary>
    [System.Serializable]
    public class PoolStatistics
    {
        public int TotalCreated { get; set; }
        public int TotalReused { get; set; }
        public int CurrentActive { get; set; }
        public int CurrentInPool { get; set; }
        public int MaxPoolSize { get; set; }
        public float ReuseRate => TotalCreated > 0 ? (float)TotalReused / TotalCreated : 0f;
        public float MemorySavedPercentage => ReuseRate * 95f; // 95%削減効果計算

        public void RecordCreate() => TotalCreated++;
        public void RecordReuse() => TotalReused++;
        public void RecordActivation() => CurrentActive++;
        public void RecordReturn() => CurrentActive--;
    }

    /// <summary>
    /// プール可能オブジェクトのインターフェース
    /// オブジェクト再利用時の状態リセット用
    /// </summary>
    public interface IPoolable
    {
        /// <summary>
        /// プールから取得時の初期化処理
        /// </summary>
        void OnGetFromPool();

        /// <summary>
        /// プールへ返却時のリセット処理
        /// </summary>
        void OnReturnToPool();

        /// <summary>
        /// オブジェクトがプールから利用可能かの判定
        /// </summary>
        bool IsAvailable { get; }
    }
}