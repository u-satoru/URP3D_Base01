using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace asterivo.Unity60.Core.Templates
{
    /// <summary>
    /// ジャンルテンプレート登録管理システム
    /// Dictionary<GenreType, GenreTemplateConfig>による高速管理
    /// DESIGN.md Layer 1: Template Configuration Layer準拠
    /// </summary>
    [System.Serializable]
    public class TemplateRegistry
    {
        [SerializeField] private List<GenreTemplateConfig> registeredTemplates = new List<GenreTemplateConfig>();
        
        // Dictionary for O(1) access performance
        private Dictionary<GenreType, GenreTemplateConfig> _templateLookup;
        private bool _isInitialized = false;
        
        /// <summary>
        /// 登録済みテンプレート数
        /// </summary>
        public int Count => _templateLookup?.Count ?? 0;
        
        /// <summary>
        /// 全ジャンルタイプ
        /// </summary>
        public IEnumerable<GenreType> RegisteredGenres => _templateLookup?.Keys ?? Enumerable.Empty<GenreType>();
        
        /// <summary>
        /// 全テンプレート設定
        /// </summary>
        public IEnumerable<GenreTemplateConfig> AllTemplates => _templateLookup?.Values ?? Enumerable.Empty<GenreTemplateConfig>();
        
        /// <summary>
        /// レジストリの初期化
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized) return;
            
            _templateLookup = new Dictionary<GenreType, GenreTemplateConfig>();
            
            // SerializeField リストからDictionaryを構築
            foreach (var template in registeredTemplates)
            {
                if (template != null && template.IsEnabled)
                {
                    if (_templateLookup.ContainsKey(template.Genre))
                    {
                        Debug.LogWarning($"Template Registry: Duplicate genre {template.Genre} found. Keeping first registration.");
                        continue;
                    }
                    
                    _templateLookup[template.Genre] = template;
                    
                    // テンプレート個別初期化
                    try
                    {
                        template.Initialize();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Failed to initialize template {template.Genre}: {ex.Message}");
                    }
                }
            }
            
            _isInitialized = true;
            Debug.Log($"Template Registry initialized with {_templateLookup.Count} templates");
        }
        
        /// <summary>
        /// テンプレート設定の取得（O(1)アクセス）
        /// </summary>
        public GenreTemplateConfig GetTemplate(GenreType genreType)
        {
            EnsureInitialized();
            return _templateLookup.TryGetValue(genreType, out var template) ? template : null;
        }
        
        /// <summary>
        /// テンプレート設定の取得（型安全）
        /// </summary>
        public T GetTemplate<T>(GenreType genreType) where T : GenreTemplateConfig
        {
            return GetTemplate(genreType) as T;
        }
        
        /// <summary>
        /// テンプレートの登録
        /// </summary>
        public void RegisterTemplate(GenreTemplateConfig template)
        {
            if (template == null)
            {
                Debug.LogWarning("Cannot register null template");
                return;
            }
            
            EnsureInitialized();
            
            if (_templateLookup.ContainsKey(template.Genre))
            {
                Debug.LogWarning($"Template for genre {template.Genre} already registered. Replacing.");
            }
            
            _templateLookup[template.Genre] = template;
            
            // SerializedFieldリストも更新
            if (!registeredTemplates.Contains(template))
            {
                registeredTemplates.Add(template);
            }
            
            template.Initialize();
        }
        
        /// <summary>
        /// テンプレートの登録解除
        /// </summary>
        public bool UnregisterTemplate(GenreType genreType)
        {
            EnsureInitialized();
            
            if (_templateLookup.Remove(genreType))
            {
                // SerializedFieldリストからも削除
                var template = registeredTemplates.FirstOrDefault(t => t != null && t.Genre == genreType);
                if (template != null)
                {
                    registeredTemplates.Remove(template);
                }
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// ジャンル存在チェック
        /// </summary>
        public bool HasTemplate(GenreType genreType)
        {
            EnsureInitialized();
            return _templateLookup.ContainsKey(genreType);
        }
        
        /// <summary>
        /// 優先度順でソートされたテンプレート一覧
        /// REQUIREMENTS.md優先度準拠（最優先→高優先→中優先→対応可能）
        /// </summary>
        public IEnumerable<GenreTemplateConfig> GetTemplatesByPriority()
        {
            EnsureInitialized();
            return _templateLookup.Values
                .Where(t => t.IsEnabled)
                .OrderBy(t => t.Priority)
                .ThenBy(t => t.Genre.ToString());
        }
        
        /// <summary>
        /// 有効なテンプレート一覧
        /// </summary>
        public IEnumerable<GenreTemplateConfig> GetEnabledTemplates()
        {
            EnsureInitialized();
            return _templateLookup.Values.Where(t => t.IsEnabled);
        }
        
        /// <summary>
        /// レジストリの検証
        /// </summary>
        public bool ValidateRegistry()
        {
            EnsureInitialized();
            
            bool isValid = true;
            
            foreach (var template in _templateLookup.Values)
            {
                if (!template.Validate())
                {
                    Debug.LogError($"Template validation failed for {template.Genre}");
                    isValid = false;
                }
            }
            
            // 7ジャンル完全対応チェック（TASK-004.2）
            var expectedGenres = Enum.GetValues(typeof(GenreType)).Cast<GenreType>();
            var missingGenres = expectedGenres.Where(g => !HasTemplate(g)).ToList();
            
            if (missingGenres.Any())
            {
                Debug.LogWarning($"Missing template configurations for: {string.Join(", ", missingGenres)}");
            }
            
            return isValid;
        }
        
        /// <summary>
        /// レジストリの統計情報
        /// </summary>
        public TemplateRegistryStats GetStats()
        {
            EnsureInitialized();
            
            return new TemplateRegistryStats
            {
                TotalTemplates = _templateLookup.Count,
                EnabledTemplates = _templateLookup.Values.Count(t => t.IsEnabled),
                HighPriorityTemplates = _templateLookup.Values.Count(t => t.Priority <= 2),
                AverageLearningTime = _templateLookup.Values.Average(t => t.EstimatedLearningTime),
                RegistryIsValid = ValidateRegistry()
            };
        }
        
        /// <summary>
        /// レジストリクリア
        /// </summary>
        public void Clear()
        {
            _templateLookup?.Clear();
            registeredTemplates.Clear();
            _isInitialized = false;
        }
        
        private void EnsureInitialized()
        {
            if (!_isInitialized)
            {
                Initialize();
            }
        }
    }
    
    /// <summary>
    /// テンプレートレジストリ統計情報
    /// </summary>
    [System.Serializable]
    public struct TemplateRegistryStats
    {
        public int TotalTemplates;
        public int EnabledTemplates;
        public int HighPriorityTemplates;
        public float AverageLearningTime;
        public bool RegistryIsValid;
        
        public override string ToString()
        {
            return $"Registry Stats: {EnabledTemplates}/{TotalTemplates} enabled, " +
                   $"{HighPriorityTemplates} high priority, " +
                   $"avg learning: {AverageLearningTime:F1}h, " +
                   $"valid: {RegistryIsValid}";
        }
    }
}
