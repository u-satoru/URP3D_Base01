using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using asterivo.Unity60.Core.Events;

namespace asterivo.Unity60.Features.Templates.Common
{
    /// <summary>
    /// ジャンル別アセット効率管理システム
    /// TASK-004.1: Asset Bundle Management実装
    /// ジャンル別アセット効率管理、動的読み込み、メモリ効率化
    /// </summary>
    public class GenreAssetManager : MonoBehaviour
    {
        [Header("アセット管理設定")]
        [SerializeField] private bool _enablePreloading = true;
        [SerializeField] private bool _enableAutoUnloading = true;
        [SerializeField] private float _unloadDelay = 5f;
        [SerializeField] private int _maxConcurrentLoads = 3;
        
        [Header("メモリ管理設定")]
        [SerializeField] private bool _enableMemoryOptimization = true;
        [SerializeField] private long _memoryLimitBytes = 536870912; // 512MB
        [SerializeField] private float _memoryCheckInterval = 1f;
        
        [Header("キャッシュ設定")]
        [SerializeField] private bool _enableAssetCaching = true;
        [SerializeField] private int _maxCacheSize = 100;
        [SerializeField] private bool _persistCacheAcrossSessions = false;
        
        [Header("デバッグ設定")]
        [SerializeField] private bool _enableDebugLogging = false;
        [SerializeField] private bool _enablePerformanceTracking = true;
        
        // Events（Event駆動アーキテクチャ準拠）
        [Header("イベントチャネル")]
        [SerializeField] private GenreTypeGameEvent _onAssetsPreloaded;
        [SerializeField] private GenreTypeGameEvent _onAssetsUnloaded;
        [SerializeField] private GameEvent _onMemoryOptimized;
        [SerializeField] private GameEvent _onCacheCleared;
        
        // Asset管理
        private readonly Dictionary<GenreType, GenreAssetBundle> _loadedBundles = new Dictionary<GenreType, GenreAssetBundle>();
        private readonly Dictionary<string, UnityEngine.Object> _assetCache = new Dictionary<string, UnityEngine.Object>();
        private readonly Queue<AssetLoadRequest> _loadQueue = new Queue<AssetLoadRequest>();
        private readonly List<Coroutine> _activeLoadCoroutines = new List<Coroutine>();
        
        // Singleton管理
        private static GenreAssetManager _instance;
        public static GenreAssetManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<GenreAssetManager>();
                    if (_instance == null)
                    {
                        var go = new GameObject("GenreAssetManager");
                        _instance = go.AddComponent<GenreAssetManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }
        
        // Properties
        public int LoadedBundleCount => _loadedBundles.Count;
        public int CachedAssetCount => _assetCache.Count;
        public int QueuedLoadCount => _loadQueue.Count;
        public bool IsLoading => _activeLoadCoroutines.Count > 0;
        public long CurrentMemoryUsage => GetCurrentMemoryUsage();
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            // Singleton設定
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            LogDebug("GenreAssetManager initialized");
        }
        
        private void Start()
        {
            // メモリ監視開始
            if (_enableMemoryOptimization)
            {
                InvokeRepeating(nameof(CheckMemoryUsage), _memoryCheckInterval, _memoryCheckInterval);
            }
            
            // プリロード開始
            if (_enablePreloading)
            {
                StartCoroutine(InitialPreloading());
            }
        }
        
        private void Update()
        {
            // ロードキューの処理
            ProcessLoadQueue();
        }
        
        private void OnDestroy()
        {
            // 全アセットをアンロード
            UnloadAllAssets();
        }
        
        #endregion
        
        #region Asset Loading
        
        /// <summary>
        /// ジャンル固有アセットをプリロード
        /// </summary>
        /// <param name="genreType">ジャンルタイプ</param>
        /// <param name="priority">読み込み優先度</param>
        public void PreloadGenreAssets(GenreType genreType, LoadPriority priority = LoadPriority.Normal)
        {
            if (_loadedBundles.ContainsKey(genreType))
            {
                LogDebug($"Assets for {genreType} already loaded");
                return;
            }
            
            var template = GenreTemplateRegistry.Instance.GetTemplate(genreType);
            if (template == null)
            {
                LogError($"Template for {genreType} not found");
                return;
            }
            
            var request = new AssetLoadRequest
            {
                GenreType = genreType,
                Template = template,
                Priority = priority,
                RequestTime = Time.time
            };
            
            // 優先度に基づいてキューに挿入
            EnqueueLoadRequest(request);
            
            LogDebug($"Queued asset preload for {genreType} (Priority: {priority})");
        }
        
        /// <summary>
        /// ロードリクエストをキューに追加
        /// </summary>
        private void EnqueueLoadRequest(AssetLoadRequest request)
        {
            // 優先度順に挿入
            var queueArray = _loadQueue.ToArray().ToList();
            queueArray.Add(request);
            queueArray.Sort((a, b) => b.Priority.CompareTo(a.Priority));
            
            _loadQueue.Clear();
            foreach (var req in queueArray)
            {
                _loadQueue.Enqueue(req);
            }
        }
        
        /// <summary>
        /// ロードキューを処理
        /// </summary>
        private void ProcessLoadQueue()
        {
            if (_activeLoadCoroutines.Count >= _maxConcurrentLoads)
                return;
                
            if (_loadQueue.Count == 0)
                return;
            
            var request = _loadQueue.Dequeue();
            var coroutine = StartCoroutine(LoadGenreAssetsCoroutine(request));
            _activeLoadCoroutines.Add(coroutine);
        }
        
        /// <summary>
        /// ジャンルアセットを読み込み
        /// </summary>
        private IEnumerator LoadGenreAssetsCoroutine(AssetLoadRequest request)
        {
            var genreType = request.GenreType;
            var template = request.Template;
            
            LogDebug($"Loading assets for {genreType}...");
            
            var startTime = Time.time;
            var bundle = new GenreAssetBundle(genreType);
            
            try
            {
                // カメラプロファイル読み込み
                if (!string.IsNullOrEmpty(template.CameraProfilePath))
                {
                    yield return StartCoroutine(LoadAssetCoroutine<ScriptableObject>(
                        template.CameraProfilePath, 
                        asset => bundle.CameraProfile = asset));
                }
                
                // Input Action Asset読み込み
                if (!string.IsNullOrEmpty(template.InputActionAssetPath))
                {
                    yield return StartCoroutine(LoadAssetCoroutine<ScriptableObject>(
                        template.InputActionAssetPath, 
                        asset => bundle.InputActionAsset = asset));
                }
                
                // AI設定読み込み
                if (!string.IsNullOrEmpty(template.AIConfigurationPath))
                {
                    yield return StartCoroutine(LoadAssetCoroutine<ScriptableObject>(
                        template.AIConfigurationPath, 
                        asset => bundle.AIConfiguration = asset));
                }
                
                // Audio Mixer読み込み
                if (!string.IsNullOrEmpty(template.AudioMixerPath))
                {
                    yield return StartCoroutine(LoadAssetCoroutine<UnityEngine.Audio.AudioMixer>(
                        template.AudioMixerPath, 
                        asset => bundle.AudioMixer = asset));
                }
                
                // ジャンル固有アセット読み込み
                yield return StartCoroutine(LoadGenreSpecificAssets(genreType, bundle));
                
                // バンドル登録
                _loadedBundles[genreType] = bundle;
                bundle.LoadTime = Time.time - startTime;
                bundle.IsLoaded = true;
                
                LogDebug($"Assets loaded for {genreType} ({bundle.LoadTime:F2}s)");
                
                // イベント発行
                _onAssetsPreloaded?.Raise(genreType);
            }
            catch (Exception ex)
            {
                LogError($"Failed to load assets for {genreType}: {ex.Message}");
                
                // 部分的に読み込まれたアセットをクリーンアップ
                bundle.Dispose();
            }
            finally
            {
                _activeLoadCoroutines.Remove(_activeLoadCoroutines.FirstOrDefault(c => c != null));
            }
        }
        
        /// <summary>
        /// ジャンル固有アセット読み込み
        /// </summary>
        private IEnumerator LoadGenreSpecificAssets(GenreType genreType, GenreAssetBundle bundle)
        {
            var assetPaths = GetGenreSpecificAssetPaths(genreType);
            
            foreach (var assetPath in assetPaths)
            {
                yield return StartCoroutine(LoadAssetCoroutine<UnityEngine.Object>(
                    assetPath,
                    asset => bundle.SpecificAssets[assetPath] = asset));
                    
                // メモリチェック
                if (_enableMemoryOptimization && GetCurrentMemoryUsage() > _memoryLimitBytes)
                {
                    LogWarning("Memory limit reached during asset loading. Pausing...");
                    yield return new WaitForSeconds(0.5f);
                    OptimizeMemoryUsage();
                }
            }
        }
        
        /// <summary>
        /// アセットを読み込み
        /// </summary>
        private IEnumerator LoadAssetCoroutine<T>(string assetPath, Action<T> onLoaded) where T : UnityEngine.Object
        {
            // キャッシュチェック
            if (_enableAssetCaching && _assetCache.ContainsKey(assetPath))
            {
                var cachedAsset = _assetCache[assetPath] as T;
                if (cachedAsset != null)
                {
                    onLoaded?.Invoke(cachedAsset);
                    yield break;
                }
            }
            
            // Resources経由での読み込み
            var resourceRequest = Resources.LoadAsync<T>(assetPath);
            yield return resourceRequest;
            
            if (resourceRequest.asset != null)
            {
                var asset = resourceRequest.asset as T;
                onLoaded?.Invoke(asset);
                
                // キャッシュに追加
                if (_enableAssetCaching)
                {
                    AddToCache(assetPath, asset);
                }
                
                LogDebug($"Loaded asset: {assetPath}");
            }
            else
            {
                LogWarning($"Failed to load asset: {assetPath}");
            }
        }
        
        #endregion
        
        #region Asset Unloading
        
        /// <summary>
        /// ジャンルアセットをアンロード
        /// </summary>
        /// <param name="genreType">ジャンルタイプ</param>
        /// <param name="immediate">即座にアンロードするか</param>
        public void UnloadGenreAssets(GenreType genreType, bool immediate = false)
        {
            if (!_loadedBundles.ContainsKey(genreType))
            {
                LogDebug($"No assets loaded for {genreType}");
                return;
            }
            
            if (immediate || !_enableAutoUnloading)
            {
                PerformUnload(genreType);
            }
            else
            {
                StartCoroutine(DelayedUnload(genreType));
            }
        }
        
        /// <summary>
        /// 遅延アンロード
        /// </summary>
        private IEnumerator DelayedUnload(GenreType genreType)
        {
            yield return new WaitForSeconds(_unloadDelay);
            PerformUnload(genreType);
        }
        
        /// <summary>
        /// アンロードを実行
        /// </summary>
        private void PerformUnload(GenreType genreType)
        {
            if (!_loadedBundles.ContainsKey(genreType))
                return;
            
            var bundle = _loadedBundles[genreType];
            bundle.Dispose();
            _loadedBundles.Remove(genreType);
            
            LogDebug($"Unloaded assets for {genreType}");
            
            // ガベージコレクション実行
            Resources.UnloadUnusedAssets();
            System.GC.Collect();
            
            // イベント発行
            _onAssetsUnloaded?.Raise(genreType);
        }
        
        /// <summary>
        /// 全アセットをアンロード
        /// </summary>
        public void UnloadAllAssets()
        {
            var genresToUnload = _loadedBundles.Keys.ToArray();
            
            foreach (var genreType in genresToUnload)
            {
                PerformUnload(genreType);
            }
            
            ClearCache();
            
            LogDebug("All assets unloaded");
        }
        
        #endregion
        
        #region Cache Management
        
        /// <summary>
        /// アセットをキャッシュに追加
        /// </summary>
        private void AddToCache(string key, UnityEngine.Object asset)
        {
            if (_assetCache.Count >= _maxCacheSize)
            {
                // LRU形式でキャッシュクリア
                var firstKey = _assetCache.Keys.First();
                _assetCache.Remove(firstKey);
            }
            
            _assetCache[key] = asset;
        }
        
        /// <summary>
        /// キャッシュをクリア
        /// </summary>
        public void ClearCache()
        {
            _assetCache.Clear();
            
            LogDebug("Asset cache cleared");
            
            // イベント発行
            _onCacheCleared?.Raise();
        }
        
        #endregion
        
        #region Memory Management
        
        /// <summary>
        /// メモリ使用量をチェック
        /// </summary>
        private void CheckMemoryUsage()
        {
            var currentUsage = GetCurrentMemoryUsage();
            
            if (currentUsage > _memoryLimitBytes)
            {
                LogWarning($"Memory usage exceeded limit: {currentUsage / 1024 / 1024}MB / {_memoryLimitBytes / 1024 / 1024}MB");
                OptimizeMemoryUsage();
            }
        }
        
        /// <summary>
        /// メモリ使用量を最適化
        /// </summary>
        private void OptimizeMemoryUsage()
        {
            LogDebug("Optimizing memory usage...");
            
            // 使用頻度の低いアセットをアンロード
            var genresToUnload = GetLeastUsedGenres(2);
            foreach (var genreType in genresToUnload)
            {
                if (_loadedBundles.ContainsKey(genreType))
                {
                    PerformUnload(genreType);
                }
            }
            
            // キャッシュを半分に削減
            var keysToRemove = _assetCache.Keys.Take(_assetCache.Count / 2).ToArray();
            foreach (var key in keysToRemove)
            {
                _assetCache.Remove(key);
            }
            
            // 未使用アセットを強制アンロード
            Resources.UnloadUnusedAssets();
            System.GC.Collect();
            
            LogDebug("Memory optimization completed");
            
            // イベント発行
            _onMemoryOptimized?.Raise();
        }
        
        /// <summary>
        /// 現在のメモリ使用量を取得
        /// </summary>
        private long GetCurrentMemoryUsage()
        {
            return UnityEngine.Profiling.Profiler.GetTotalAllocatedMemory(0);
        }
        
        /// <summary>
        /// 使用頻度の低いジャンルを取得
        /// </summary>
        private List<GenreType> GetLeastUsedGenres(int count)
        {
            return _loadedBundles
                .OrderBy(kvp => kvp.Value.LastAccessTime)
                .Take(count)
                .Select(kvp => kvp.Key)
                .ToList();
        }
        
        #endregion
        
        #region Asset Access
        
        /// <summary>
        /// ジャンル固有アセットを取得
        /// </summary>
        /// <typeparam name="T">アセット型</typeparam>
        /// <param name="genreType">ジャンルタイプ</param>
        /// <param name="assetKey">アセットキー</param>
        /// <returns>アセット（見つからない場合はnull）</returns>
        public T GetGenreAsset<T>(GenreType genreType, string assetKey) where T : UnityEngine.Object
        {
            if (!_loadedBundles.ContainsKey(genreType))
            {
                LogWarning($"Assets for {genreType} not loaded");
                return null;
            }
            
            var bundle = _loadedBundles[genreType];
            bundle.LastAccessTime = Time.time;
            
            return bundle.GetAsset<T>(assetKey);
        }
        
        /// <summary>
        /// ジャンルアセットバンドルが読み込み済みかチェック
        /// </summary>
        /// <param name="genreType">ジャンルタイプ</param>
        /// <returns>読み込み済みの場合true</returns>
        public bool IsGenreLoaded(GenreType genreType)
        {
            return _loadedBundles.ContainsKey(genreType) && _loadedBundles[genreType].IsLoaded;
        }
        
        #endregion
        
        #region Utility Methods
        
        /// <summary>
        /// ジャンル固有アセットパス一覧を取得
        /// </summary>
        private List<string> GetGenreSpecificAssetPaths(GenreType genreType)
        {
            var paths = new List<string>();
            
            switch (genreType)
            {
                case GenreType.Stealth:
                    paths.AddRange(new[]
                    {
                        "GameData/Stealth/StealthConfig",
                        "GameData/Stealth/NPCPatrolRoutes",
                        "GameData/Stealth/HidingSpots"
                    });
                    break;
                    
                case GenreType.FPS:
                    paths.AddRange(new[]
                    {
                        "GameData/FPS/WeaponConfigs",
                        "GameData/FPS/CrosshairProfiles"
                    });
                    break;
                    
                case GenreType.ActionRPG:
                    paths.AddRange(new[]
                    {
                        "GameData/ActionRPG/CharacterClasses",
                        "GameData/ActionRPG/ItemDatabase",
                        "GameData/ActionRPG/SkillTrees"
                    });
                    break;
                    
                // 他のジャンルも同様に定義
            }
            
            return paths;
        }
        
        /// <summary>
        /// 初期プリロード
        /// </summary>
        private IEnumerator InitialPreloading()
        {
            LogDebug("Starting initial preloading...");
            
            // 高優先度ジャンルをプリロード
            var highPriorityGenres = new[] { GenreType.Stealth, GenreType.Platformer };
            
            foreach (var genreType in highPriorityGenres)
            {
                PreloadGenreAssets(genreType, LoadPriority.High);
                yield return new WaitForSeconds(0.1f); // 負荷分散
            }
            
            LogDebug("Initial preloading queued");
        }
        
        #endregion
        
        #region Debug & Logging
        
        private void LogDebug(string message)
        {
            if (_enableDebugLogging)
            {
                Debug.Log($"[GenreAssetManager] {message}");
            }
        }
        
        private void LogWarning(string message)
        {
            Debug.LogWarning($"[GenreAssetManager] {message}");
        }
        
        private void LogError(string message)
        {
            Debug.LogError($"[GenreAssetManager] {message}");
        }
        
        /// <summary>
        /// 現在の状態をコンソールに出力（デバッグ用）
        /// </summary>
        [ContextMenu("Print Asset Status")]
        public void PrintAssetStatus()
        {
            Debug.Log("=== GenreAssetManager Status ===");
            Debug.Log($"Loaded Bundles: {_loadedBundles.Count}");
            Debug.Log($"Cached Assets: {_assetCache.Count}");
            Debug.Log($"Queued Loads: {_loadQueue.Count}");
            Debug.Log($"Active Load Coroutines: {_activeLoadCoroutines.Count}");
            Debug.Log($"Memory Usage: {GetCurrentMemoryUsage() / 1024 / 1024}MB / {_memoryLimitBytes / 1024 / 1024}MB");
            
            foreach (var kvp in _loadedBundles)
            {
                var bundle = kvp.Value;
                Debug.Log($"  {kvp.Key}: {bundle.SpecificAssets.Count} assets, Load Time: {bundle.LoadTime:F2}s");
            }
        }
        
        #endregion
    }
    
    /// <summary>
    /// アセット読み込みリクエスト
    /// </summary>
    public class AssetLoadRequest
    {
        public GenreType GenreType;
        public GenreTemplateConfig Template;
        public LoadPriority Priority;
        public float RequestTime;
    }
    
    /// <summary>
    /// 読み込み優先度
    /// </summary>
    public enum LoadPriority
    {
        Low = 0,
        Normal = 1,
        High = 2,
        Critical = 3
    }
    
    /// <summary>
    /// ジャンルアセットバンドル
    /// </summary>
    public class GenreAssetBundle : IDisposable
    {
        public GenreType GenreType { get; }
        public bool IsLoaded { get; set; }
        public float LoadTime { get; set; }
        public float LastAccessTime { get; set; }
        
        // アセット参照
        public ScriptableObject CameraProfile { get; set; }
        public ScriptableObject InputActionAsset { get; set; }
        public ScriptableObject AIConfiguration { get; set; }
        public UnityEngine.Audio.AudioMixer AudioMixer { get; set; }
        public Dictionary<string, UnityEngine.Object> SpecificAssets { get; }
        
        public GenreAssetBundle(GenreType genreType)
        {
            GenreType = genreType;
            SpecificAssets = new Dictionary<string, UnityEngine.Object>();
            LastAccessTime = Time.time;
        }
        
        /// <summary>
        /// アセットを取得
        /// </summary>
        public T GetAsset<T>(string key) where T : UnityEngine.Object
        {
            if (SpecificAssets.ContainsKey(key))
            {
                return SpecificAssets[key] as T;
            }
            
            // システムアセットからも検索
            if (key == "CameraProfile") return CameraProfile as T;
            if (key == "InputActionAsset") return InputActionAsset as T;
            if (key == "AIConfiguration") return AIConfiguration as T;
            if (key == "AudioMixer") return AudioMixer as T;
            
            return null;
        }
        
        public void Dispose()
        {
            CameraProfile = null;
            InputActionAsset = null;
            AIConfiguration = null;
            AudioMixer = null;
            SpecificAssets.Clear();
            IsLoaded = false;
        }
    }
}