using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using asterivo.Unity60.Core.Events;

namespace asterivo.Unity60.Features.Templates.Common
{
    /// <summary>
    /// GenreTemplateConfigのRegistry管理システム
    /// TASK-004.1: Template Registry System実装
    /// Dictionary<GenreType, GenreTemplateConfig>による高速管理
    /// DESIGN.md Template Configuration Layer準拠
    /// </summary>
    public class GenreTemplateRegistry : MonoBehaviour
    {
        [Header("テンプレートレジストリ設定")]
        [SerializeField] private bool _loadAllTemplatesOnStart = true;
        [SerializeField] private bool _enableDebugLogging = false;
        
        [Header("リソースパス設定")]
        [SerializeField] private string _templateResourcesPath = "GameData/Templates";
        
        [Header("登録済みテンプレート")]
        [SerializeField] private List<GenreTemplateConfig> _registeredTemplates = new List<GenreTemplateConfig>();
        
        // Registry Core: Dictionary<GenreType, GenreTemplateConfig>による高速管理
        private readonly Dictionary<GenreType, GenreTemplateConfig> _templateRegistry = new Dictionary<GenreType, GenreTemplateConfig>();
        private readonly Dictionary<GenreType, string> _templatePaths = new Dictionary<GenreType, string>();
        
        // イベント定義（Event駆動アーキテクチャ準拠）
        [Header("イベントチャネル")]
        [SerializeField] private GameEvent _onTemplateRegistered;
        [SerializeField] private GameEvent _onTemplateUnregistered;
        [SerializeField] private GenreTypeGameEvent _onTemplateChanged;
        
        // Singleton管理
        private static GenreTemplateRegistry _instance;
        public static GenreTemplateRegistry Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<GenreTemplateRegistry>();
                    if (_instance == null)
                    {
                        var go = new GameObject("GenreTemplateRegistry");
                        _instance = go.AddComponent<GenreTemplateRegistry>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }
        
        // Properties
        public int RegisteredCount => _templateRegistry.Count;
        public IReadOnlyDictionary<GenreType, GenreTemplateConfig> Templates => _templateRegistry;
        public IEnumerable<GenreType> RegisteredGenres => _templateRegistry.Keys;
        public IEnumerable<GenreTemplateConfig> RegisteredTemplates => _templateRegistry.Values;
        
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
            
            LogDebug("GenreTemplateRegistry initialized");
        }
        
        private void Start()
        {
            if (_loadAllTemplatesOnStart)
            {
                LoadAllTemplates();
            }
        }
        
        #endregion
        
        #region Template Management
        
        /// <summary>
        /// 全てのテンプレートを読み込み
        /// </summary>
        public void LoadAllTemplates()
        {
            LogDebug("Loading all templates from Resources...");
            
            // Resourcesフォルダから全てのGenreTemplateConfigを読み込み
            var templates = Resources.LoadAll<GenreTemplateConfig>(_templateResourcesPath);
            
            foreach (var template in templates)
            {
                if (template != null)
                {
                    RegisterTemplate(template);
                }
            }
            
            // シリアライズされたテンプレートも登録
            foreach (var template in _registeredTemplates)
            {
                if (template != null)
                {
                    RegisterTemplate(template);
                }
            }
            
            LogDebug($"Loaded {_templateRegistry.Count} templates");
            
            // イベント発行
            _onTemplateRegistered?.Raise();
        }
        
        /// <summary>
        /// テンプレートを登録
        /// </summary>
        /// <param name="template">登録するテンプレート</param>
        /// <returns>成功した場合true</returns>
        public bool RegisterTemplate(GenreTemplateConfig template)
        {
            if (template == null)
            {
                LogError("Cannot register null template");
                return false;
            }
            
            var genreType = template.GenreType;
            
            if (_templateRegistry.ContainsKey(genreType))
            {
                LogWarning($"Template for {genreType} already registered. Overwriting...");
            }
            
            _templateRegistry[genreType] = template;
            
            // アセットパスを記録（デバッグ用）
            #if UNITY_EDITOR
            var assetPath = UnityEditor.AssetDatabase.GetAssetPath(template);
            if (!string.IsNullOrEmpty(assetPath))
            {
                _templatePaths[genreType] = assetPath;
            }
            #endif
            
            LogDebug($"Registered template: {genreType} - {template.DisplayName}");
            
            // イベント発行
            _onTemplateChanged?.Raise(genreType);
            
            return true;
        }
        
        /// <summary>
        /// テンプレートを登録解除
        /// </summary>
        /// <param name="genreType">登録解除するジャンル</param>
        /// <returns>成功した場合true</returns>
        public bool UnregisterTemplate(GenreType genreType)
        {
            if (!_templateRegistry.ContainsKey(genreType))
            {
                LogWarning($"Template for {genreType} is not registered");
                return false;
            }
            
            _templateRegistry.Remove(genreType);
            _templatePaths.Remove(genreType);
            
            LogDebug($"Unregistered template: {genreType}");
            
            // イベント発行
            _onTemplateUnregistered?.Raise();
            _onTemplateChanged?.Raise(genreType);
            
            return true;
        }
        
        #endregion
        
        #region Template Access (高速アクセス)
        
        /// <summary>
        /// テンプレートを取得（高速Dictionary検索）
        /// </summary>
        /// <param name="genreType">ジャンルタイプ</param>
        /// <returns>テンプレート（見つからない場合はnull）</returns>
        public GenreTemplateConfig GetTemplate(GenreType genreType)
        {
            _templateRegistry.TryGetValue(genreType, out var template);
            return template;
        }
        
        /// <summary>
        /// テンプレートが登録済みかチェック
        /// </summary>
        /// <param name="genreType">ジャンルタイプ</param>
        /// <returns>登録済みの場合true</returns>
        public bool IsTemplateRegistered(GenreType genreType)
        {
            return _templateRegistry.ContainsKey(genreType);
        }
        
        /// <summary>
        /// 優先度で並べ替えたテンプレート一覧を取得
        /// </summary>
        /// <returns>優先度順のテンプレート一覧</returns>
        public List<GenreTemplateConfig> GetTemplatesSortedByPriority()
        {
            return _templateRegistry.Values
                .OrderBy(t => t.GetPriority())
                .ThenBy(t => t.DisplayName)
                .ToList();
        }
        
        /// <summary>
        /// 指定優先度のテンプレート一覧を取得
        /// </summary>
        /// <param name="priority">優先度</param>
        /// <returns>指定優先度のテンプレート一覧</returns>
        public List<GenreTemplateConfig> GetTemplatesByPriority(GenrePriority priority)
        {
            return _templateRegistry.Values
                .Where(t => t.GetPriority() == priority)
                .OrderBy(t => t.DisplayName)
                .ToList();
        }
        
        /// <summary>
        /// AI機能が必要なテンプレート一覧を取得
        /// </summary>
        /// <returns>AI機能が必要なテンプレート一覧</returns>
        public List<GenreTemplateConfig> GetTemplatesRequiringAI()
        {
            return _templateRegistry.Values
                .Where(t => t.RequiresAI())
                .ToList();
        }
        
        #endregion
        
        #region Validation & Diagnostics
        
        /// <summary>
        /// 全テンプレートの整合性チェック
        /// </summary>
        /// <returns>チェック結果</returns>
        public bool ValidateAllTemplates()
        {
            var isValid = true;
            var invalidTemplates = new List<GenreType>();
            
            foreach (var kvp in _templateRegistry)
            {
                if (!kvp.Value.IsValid())
                {
                    isValid = false;
                    invalidTemplates.Add(kvp.Key);
                    LogError($"Invalid template: {kvp.Key}");
                }
            }
            
            LogDebug($"Template validation: {(isValid ? "PASSED" : "FAILED")} ({_templateRegistry.Count - invalidTemplates.Count}/{_templateRegistry.Count})");
            
            return isValid;
        }
        
        /// <summary>
        /// レジストリ統計情報を取得
        /// </summary>
        /// <returns>統計情報</returns>
        public RegistryStatistics GetStatistics()
        {
            var stats = new RegistryStatistics
            {
                TotalTemplates = _templateRegistry.Count,
                HighestPriorityCount = GetTemplatesByPriority(GenrePriority.Highest).Count,
                HighPriorityCount = GetTemplatesByPriority(GenrePriority.High).Count,
                MediumPriorityCount = GetTemplatesByPriority(GenrePriority.Medium).Count,
                NormalPriorityCount = GetTemplatesByPriority(GenrePriority.Normal).Count,
                AIRequiredCount = GetTemplatesRequiringAI().Count,
                ValidTemplateCount = _templateRegistry.Values.Count(t => t.IsValid()),
                InvalidTemplateCount = _templateRegistry.Values.Count(t => !t.IsValid())
            };
            
            return stats;
        }
        
        #endregion
        
        #region Debug & Logging
        
        private void LogDebug(string message)
        {
            if (_enableDebugLogging)
            {
                Debug.Log($"[GenreTemplateRegistry] {message}");
            }
        }
        
        private void LogWarning(string message)
        {
            Debug.LogWarning($"[GenreTemplateRegistry] {message}");
        }
        
        private void LogError(string message)
        {
            Debug.LogError($"[GenreTemplateRegistry] {message}");
        }
        
        /// <summary>
        /// レジストリ状態をコンソールに出力（デバッグ用）
        /// </summary>
        [ContextMenu("Print Registry Status")]
        public void PrintRegistryStatus()
        {
            Debug.Log($"=== GenreTemplateRegistry Status ===");
            Debug.Log($"Registered Templates: {_templateRegistry.Count}");
            
            foreach (var kvp in _templateRegistry)
            {
                var template = kvp.Value;
                var priority = template.GetPriority();
                var aiRequired = template.RequiresAI();
                var valid = template.IsValid();
                
                Debug.Log($"  {kvp.Key}: {template.DisplayName} (Priority: {priority}, AI: {aiRequired}, Valid: {valid})");
            }
            
            var stats = GetStatistics();
            Debug.Log($"Statistics: Total={stats.TotalTemplates}, Valid={stats.ValidTemplateCount}, AI={stats.AIRequiredCount}");
        }
        
        #endregion
        
        /// <summary>
        /// レジストリ統計情報構造体
        /// </summary>
        [System.Serializable]
        public struct RegistryStatistics
        {
            public int TotalTemplates;
            public int HighestPriorityCount;
            public int HighPriorityCount;
            public int MediumPriorityCount;
            public int NormalPriorityCount;
            public int AIRequiredCount;
            public int ValidTemplateCount;
            public int InvalidTemplateCount;
        }
    }
}