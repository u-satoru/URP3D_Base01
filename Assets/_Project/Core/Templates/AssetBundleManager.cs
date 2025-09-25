using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace asterivo.Unity60.Core.Templates
{
    /// <summary>
    /// ジャンル別アセット効率管理システム
    /// Asset Bundle Management（ジャンル別アセット効率管理）
    /// DESIGN.md Layer 1: Template Configuration Layer準拠
    /// </summary>
    [System.Serializable]
    public class AssetBundleManager
    {
        [Header("Asset Management Settings")]
        [SerializeField] private bool enableAssetPreloading = true;
        [SerializeField] private bool enableAssetCaching = true;
        [SerializeField] private int maxCachedAssets = 100;
        [SerializeField] private float assetTimeoutSeconds = 300f; // 5分
        
        [Header("Performance Settings")]
        [SerializeField] private int maxConcurrentLoads = 3;
        [SerializeField] private bool enableAssetCompression = true;
        [SerializeField] private bool enableMemoryOptimization = true;
        
        // Events
        public static event Action<GenreType, float> OnAssetLoadProgress;
        public static event Action<GenreType> OnAssetLoadCompleted;
        public static event Action<GenreType, string> OnAssetLoadFailed;
        
        private Dictionary<GenreType, GenreAssetBundle> _loadedBundles = new Dictionary<GenreType, GenreAssetBundle>();
        private Dictionary<string, CachedAsset> _assetCache = new Dictionary<string, CachedAsset>();
        private Queue<AssetLoadRequest> _loadQueue = new Queue<AssetLoadRequest>();
        private List<AssetLoadRequest> _activeLoads = new List<AssetLoadRequest>();
        
        // Statistics
        [System.Serializable]
        public class AssetStats
        {
            public int loadedBundles;
            public int cachedAssets;
            public long totalMemoryUsage;
            public float cacheHitRate;
            public int successfulLoads;
            public int failedLoads;
            
            public void RecordLoad(bool success, long memoryUsage = 0)
            {
                if (success)
                {
                    successfulLoads++;
                    totalMemoryUsage += memoryUsage;
                }
                else
                {
                    failedLoads++;
                }
            }
            
            public void UpdateCacheHitRate(int hits, int total)
            {
                cacheHitRate = total > 0 ? (float)hits / total : 0f;
            }
        }
        
        [SerializeField] private AssetStats stats = new AssetStats();
        
        /// <summary>
        /// ジャンル用アセットの事前読み込み
        /// </summary>
        public IEnumerator PreloadGenreAssets(GenreType genre, GenreTemplateConfig config)
        {
            if (!enableAssetPreloading)
            {
                yield break;
            }
            
            Debug.Log($"Preloading assets for genre {genre}");
            
            var assetRequest = new AssetLoadRequest
            {
                Genre = genre,
                Config = config,
                StartTime = Time.time,
                LoadType = AssetLoadType.Preload
            };
            
            yield return LoadGenreAssetsCoroutine(assetRequest);
        }
        
        /// <summary>
        /// ジャンル用アセットの読み込み
        /// </summary>
        public void LoadGenreAssets(GenreType genre, GenreTemplateConfig config, bool immediate = false)
        {
            var request = new AssetLoadRequest
            {
                Genre = genre,
                Config = config,
                StartTime = Time.time,
                LoadType = immediate ? AssetLoadType.Immediate : AssetLoadType.Background
            };
            
            if (immediate || _activeLoads.Count < maxConcurrentLoads)
            {
                _activeLoads.Add(request);
                // MonoBehaviourのコルーチンが必要な場合はTemplateManagerに委譲
                Debug.Log($"Loading assets for {genre} ({request.LoadType})");
            }
            else
            {
                _loadQueue.Enqueue(request);
                Debug.Log($"Queued asset loading for {genre}");
            }
        }
        
        /// <summary>
        /// ジャンル用アセットのアンロード
        /// </summary>
        public void UnloadGenreAssets(GenreType genre, bool immediate = false)
        {
            if (_loadedBundles.TryGetValue(genre, out var bundle))
            {
                Debug.Log($"Unloading assets for genre {genre}");
                
                // アセットの解放
                foreach (var asset in bundle.LoadedAssets)
                {
                    if (asset.Value != null)
                    {
                        // キャッシュからも削除
                        var cacheKey = GetAssetCacheKey(genre, asset.Key);
                        _assetCache.Remove(cacheKey);
                        
                        // Unityオブジェクトの場合は適切に破棄
                        if (asset.Value is UnityEngine.Object unityObj)
                        {
                            if (immediate)
                            {
                                UnityEngine.Object.DestroyImmediate(unityObj);
                            }
                            else
                            {
                                UnityEngine.Object.Destroy(unityObj);
                            }
                        }
                    }
                }
                
                _loadedBundles.Remove(genre);
                stats.loadedBundles = _loadedBundles.Count;
                
                if (immediate)
                {
                    Resources.UnloadUnusedAssets();
                    System.GC.Collect();
                }
            }
        }
        
        /// <summary>
        /// 特定アセットの取得
        /// </summary>
        public T GetAsset<T>(GenreType genre, string assetName) where T : UnityEngine.Object
        {
            var cacheKey = GetAssetCacheKey(genre, assetName);
            
            // キャッシュから取得を試行
            if (_assetCache.TryGetValue(cacheKey, out var cachedAsset))
            {
                cachedAsset.LastAccessTime = Time.time;
                return cachedAsset.Asset as T;
            }
            
            // 読み込み済みバンドルから取得を試行
            if (_loadedBundles.TryGetValue(genre, out var bundle))
            {
                if (bundle.LoadedAssets.TryGetValue(assetName, out var asset))
                {
                    // キャッシュに追加
                    CacheAsset(cacheKey, asset);
                    return asset as T;
                }
            }
            
            Debug.LogWarning($"Asset {assetName} not found for genre {genre}");
            return null;
        }
        
        /// <summary>
        /// アセットの非同期取得
        /// </summary>
        public IEnumerator GetAssetAsync<T>(GenreType genre, string assetName, System.Action<T> callback) where T : UnityEngine.Object
        {
            var asset = GetAsset<T>(genre, assetName);
            
            if (asset != null)
            {
                callback?.Invoke(asset);
                yield break;
            }
            
            // アセットが見つからない場合は読み込みを試行
            Debug.Log($"Asset {assetName} not cached, attempting to load for {genre}");
            
            // TODO: 実際の非同期読み込み実装
            yield return new WaitForSeconds(0.1f); // シミュレート
            
            asset = GetAsset<T>(genre, assetName);
            callback?.Invoke(asset);
        }
        
        /// <summary>
        /// 全アセットのアンロード
        /// </summary>
        public void UnloadAllAssets()
        {
            Debug.Log("Unloading all genre assets");
            
            foreach (var genre in _loadedBundles.Keys.ToList())
            {
                UnloadGenreAssets(genre, true);
            }
            
            _assetCache.Clear();
            _loadQueue.Clear();
            _activeLoads.Clear();
            
            Resources.UnloadUnusedAssets();
            System.GC.Collect();
            
            stats = new AssetStats();
        }
        
        /// <summary>
        /// キャッシュのクリーンアップ
        /// </summary>
        public void CleanupCache()
        {
            var currentTime = Time.time;
            var expiredAssets = _assetCache
                .Where(kvp => currentTime - kvp.Value.LastAccessTime > assetTimeoutSeconds)
                .Select(kvp => kvp.Key)
                .ToList();
            
            foreach (var key in expiredAssets)
            {
                _assetCache.Remove(key);
            }
            
            Debug.Log($"Cleaned up {expiredAssets.Count} expired cached assets");
        }
        
        /// <summary>
        /// アセット統計情報の取得
        /// </summary>
        public AssetStats GetStats()
        {
            stats.loadedBundles = _loadedBundles.Count;
            stats.cachedAssets = _assetCache.Count;
            
            // キャッシュヒット率の計算（簡易版）
            var totalAccess = stats.successfulLoads + stats.failedLoads;
            var cacheHits = _assetCache.Count; // 簡略化
            stats.UpdateCacheHitRate(cacheHits, totalAccess);
            
            return stats;
        }
        
        /// <summary>
        /// メモリ使用量の最適化
        /// </summary>
        public void OptimizeMemoryUsage()
        {
            if (!enableMemoryOptimization) return;
            
            CleanupCache();
            
            // キャッシュサイズが上限を超えている場合は古いアセットを削除
            if (_assetCache.Count > maxCachedAssets)
            {
                var sortedAssets = _assetCache
                    .OrderBy(kvp => kvp.Value.LastAccessTime)
                    .Take(_assetCache.Count - maxCachedAssets)
                    .Select(kvp => kvp.Key)
                    .ToList();
                
                foreach (var key in sortedAssets)
                {
                    _assetCache.Remove(key);
                }
                
                Debug.Log($"Removed {sortedAssets.Count} least recently used assets from cache");
            }
            
            Resources.UnloadUnusedAssets();
        }
        
        #region Private Methods
        
        private IEnumerator LoadGenreAssetsCoroutine(AssetLoadRequest request)
        {
            var progress = 0f;
            bool loadingSucceeded = false;

            // Phase 1: Essential Assets（必須アセット）
            OnAssetLoadProgress?.Invoke(request.Genre, 0.1f);
            yield return LoadEssentialAssets(request);

            // Phase 2: UI Assets（UIアセット）
            OnAssetLoadProgress?.Invoke(request.Genre, 0.3f);
            yield return LoadUIAssets(request);

            // Phase 3: Audio Assets（オーディオアセット）
            OnAssetLoadProgress?.Invoke(request.Genre, 0.5f);
            yield return LoadAudioAssets(request);

            // Phase 4: Visual Assets（ビジュアルアセット）
            OnAssetLoadProgress?.Invoke(request.Genre, 0.7f);
            yield return LoadVisualAssets(request);

            // Phase 5: Gameplay Assets（ゲームプレイアセット）
            OnAssetLoadProgress?.Invoke(request.Genre, 0.9f);
            yield return LoadGameplayAssets(request);

            loadingSucceeded = true;

            // 完了
            OnAssetLoadProgress?.Invoke(request.Genre, 1.0f);
            OnAssetLoadCompleted?.Invoke(request.Genre);

            var loadTime = Time.time - request.StartTime;
            Debug.Log($"Asset loading completed for {request.Genre} in {loadTime:F2}s");

            stats.RecordLoad(true);

            _activeLoads.Remove(request);
            ProcessLoadQueue();
        }
        
        private IEnumerator LoadEssentialAssets(AssetLoadRequest request)
        {
            // TODO: 必須アセットの読み込み実装
            yield return new WaitForSeconds(0.05f);
        }
        
        private IEnumerator LoadUIAssets(AssetLoadRequest request)
        {
            // TODO: UIアセットの読み込み実装
            yield return new WaitForSeconds(0.05f);
        }
        
        private IEnumerator LoadAudioAssets(AssetLoadRequest request)
        {
            // TODO: オーディオアセットの読み込み実装
            yield return new WaitForSeconds(0.05f);
        }
        
        private IEnumerator LoadVisualAssets(AssetLoadRequest request)
        {
            // TODO: ビジュアルアセットの読み込み実装
            yield return new WaitForSeconds(0.05f);
        }
        
        private IEnumerator LoadGameplayAssets(AssetLoadRequest request)
        {
            // TODO: ゲームプレイアセットの読み込み実装
            yield return new WaitForSeconds(0.05f);
        }
        
        private void ProcessLoadQueue()
        {
            while (_loadQueue.Count > 0 && _activeLoads.Count < maxConcurrentLoads)
            {
                var request = _loadQueue.Dequeue();
                _activeLoads.Add(request);
                // コルーチン開始（TemplateManagerに委譲が必要）
                Debug.Log($"Processing queued load for {request.Genre}");
            }
        }
        
        private string GetAssetCacheKey(GenreType genre, string assetName)
        {
            return $"{genre}_{assetName}";
        }
        
        private void CacheAsset(string cacheKey, object asset)
        {
            if (!enableAssetCaching) return;
            
            _assetCache[cacheKey] = new CachedAsset
            {
                Asset = asset,
                CacheTime = Time.time,
                LastAccessTime = Time.time
            };
        }
        
        #endregion
    }
    
    #region Data Structures
    
    [System.Serializable]
    public class GenreAssetBundle
    {
        public GenreType Genre;
        public Dictionary<string, object> LoadedAssets = new Dictionary<string, object>();
        public float LoadTime;
        public long MemoryUsage;
    }
    
    [System.Serializable]
    public class AssetLoadRequest
    {
        public GenreType Genre;
        public GenreTemplateConfig Config;
        public AssetLoadType LoadType;
        public float StartTime;
    }
    
    public enum AssetLoadType
    {
        Background,
        Immediate,
        Preload
    }
    
    [System.Serializable]
    public class CachedAsset
    {
        public object Asset;
        public float CacheTime;
        public float LastAccessTime;
    }
    
    #endregion
}
