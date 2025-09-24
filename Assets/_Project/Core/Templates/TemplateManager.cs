using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace asterivo.Unity60.Core.Templates
{
    /// <summary>
    /// ジャンルテンプレート動的切り替えマネージャー
    /// Runtime管理、TemplateManager（動的切り替え・状態保持・アセット管理・設定同期）
    /// DESIGN.md Layer 2: Runtime Template Management準拠
    /// </summary>
    public class TemplateManager : MonoBehaviour
    {
        [Header("Template Registry")]
        [SerializeField] private TemplateRegistry templateRegistry = new TemplateRegistry();
        
        [Header("Current State")]
        [SerializeField] private GenreType currentGenre = GenreType.Stealth;
        [SerializeField] private GenreTemplateConfig currentTemplate;
        
        [Header("Switching Settings")]
        [SerializeField] private float switchingTimeout = 180f; // 3分以内切り替え要件
        [SerializeField] private bool preservePlayerProgress = true;
        [SerializeField] private bool enableAssetPreloading = true;
        
        // Events
        public static event Action<GenreType, GenreType> OnGenreSwitchStarted;
        public static event Action<GenreType> OnGenreSwitchCompleted;
        public static event Action<string> OnGenreSwitchFailed;
        
        // Singleton instance
        private static TemplateManager _instance;
        public static TemplateManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<TemplateManager>();
                    if (_instance == null)
                    {
                        var go = new GameObject("TemplateManager");
                        _instance = go.AddComponent<TemplateManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }
        
        // Current state
        public GenreType CurrentGenre => currentGenre;
        public GenreTemplateConfig CurrentTemplate => currentTemplate;
        public bool IsSwitching { get; private set; }
        
        // Statistics
        [System.Serializable]
        public class SwitchingStats
        {
            public int totalSwitches;
            public float averageSwitchTime;
            public float fastestSwitchTime = float.MaxValue;
            public float slowestSwitchTime;
            public int failedSwitches;
            
            public void RecordSwitch(float switchTime, bool success)
            {
                if (success)
                {
                    totalSwitches++;
                    averageSwitchTime = ((averageSwitchTime * (totalSwitches - 1)) + switchTime) / totalSwitches;
                    fastestSwitchTime = Mathf.Min(fastestSwitchTime, switchTime);
                    slowestSwitchTime = Mathf.Max(slowestSwitchTime, switchTime);
                }
                else
                {
                    failedSwitches++;
                }
            }
        }
        
        [SerializeField] private SwitchingStats stats = new SwitchingStats();
        
        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeTemplateManager();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }
        
        private void InitializeTemplateManager()
        {
            templateRegistry.Initialize();
            
            // 現在のテンプレートを設定
            currentTemplate = templateRegistry.GetTemplate(currentGenre);
            if (currentTemplate == null)
            {
                Debug.LogError($"No template found for genre {currentGenre}. Falling back to Stealth.");
                currentGenre = GenreType.Stealth;
                currentTemplate = templateRegistry.GetTemplate(currentGenre);
            }
            
            Debug.Log($"TemplateManager initialized with genre: {currentGenre}");
        }
        
        /// <summary>
        /// ジャンル切り替え（非同期）
        /// TASK-004受入条件：テンプレート切り替え時のデータ整合性保証（3分以内）
        /// </summary>
        public void SwitchGenre(GenreType targetGenre)
        {
            if (IsSwitching)
            {
                Debug.LogWarning("Genre switching already in progress");
                return;
            }
            
            if (targetGenre == currentGenre)
            {
                Debug.LogWarning($"Already using genre {targetGenre}");
                return;
            }
            
            var targetTemplate = templateRegistry.GetTemplate(targetGenre);
            if (targetTemplate == null)
            {
                OnGenreSwitchFailed?.Invoke($"Template not found for genre {targetGenre}");
                return;
            }
            
            StartCoroutine(SwitchGenreCoroutine(targetGenre, targetTemplate));
        }
        
        private IEnumerator SwitchGenreCoroutine(GenreType targetGenre, GenreTemplateConfig targetTemplate)
        {
            IsSwitching = true;
            var startTime = Time.time;
            var previousGenre = currentGenre;
            bool switchSuccess = false;

            OnGenreSwitchStarted?.Invoke(previousGenre, targetGenre);
            Debug.Log($"Starting genre switch: {previousGenre} → {targetGenre}");

            // Phase 1: State Preservation（状態保持）
            yield return StartCoroutine(PreservateCurrentState());

            // Phase 2: Asset Management（アセット管理）
            if (enableAssetPreloading)
            {
                yield return StartCoroutine(PreloadTargetAssets(targetTemplate));
            }

            // Phase 3: Scene Transition（シーン遷移）
            yield return StartCoroutine(TransitionToTargetGenre(targetTemplate));

            // Phase 4: Configuration Synchronization（設定同期）
            yield return StartCoroutine(ApplyTargetConfiguration(targetTemplate));

            // Phase 5: Validation（検証）
            if (!ValidateSwitchCompletion(targetTemplate))
            {
                Debug.LogError("Switch validation failed");
                OnGenreSwitchFailed?.Invoke("Switch validation failed");
                var failTime = Time.time - startTime;
                stats.RecordSwitch(failTime, false);
                StartCoroutine(RollbackGenreSwitch(previousGenre));
                IsSwitching = false;
                yield break;
            }

            // 切り替え完了
            currentGenre = targetGenre;
            currentTemplate = targetTemplate;

            var switchTime = Time.time - startTime;
            stats.RecordSwitch(switchTime, true);

            Debug.Log($"Genre switch completed: {targetGenre} (took {switchTime:F2}s)");
            OnGenreSwitchCompleted?.Invoke(targetGenre);

            // 3分以内切り替え要件チェック
            if (switchTime > switchingTimeout)
            {
                Debug.LogWarning($"Switch time {switchTime:F2}s exceeded target {switchingTimeout}s");
            }

            IsSwitching = false;
        }
        
        private IEnumerator PreservateCurrentState()
        {
            Debug.Log("Preserving current state...");
            
            if (preservePlayerProgress)
            {
                // TODO: PlayerProgress preservation implementation
                // Save current player progress, settings, achievements, etc.
                yield return null;
            }
            
            yield return new WaitForEndOfFrame();
        }
        
        private IEnumerator PreloadTargetAssets(GenreTemplateConfig targetTemplate)
        {
            Debug.Log($"Preloading assets for {targetTemplate.Genre}...");
            
            // TODO: Asset Bundle Management implementation
            // Preload required assets for target genre
            yield return new WaitForSeconds(0.1f); // Simulated loading time
        }
        
        private IEnumerator TransitionToTargetGenre(GenreTemplateConfig targetTemplate)
        {
            Debug.Log($"Transitioning to {targetTemplate.Genre}...");
            
            // TODO: Scene transition implementation
            // Load appropriate scene for target genre
            yield return new WaitForSeconds(0.1f); // Simulated transition time
        }
        
        private IEnumerator ApplyTargetConfiguration(GenreTemplateConfig targetTemplate)
        {
            Debug.Log($"Applying configuration for {targetTemplate.Genre}...");
            
            // Initialize target template
            targetTemplate.Initialize();
            
            // Apply genre-specific settings
            // TODO: Apply camera settings, input mappings, UI configuration, etc.
            
            yield return new WaitForEndOfFrame();
        }
        
        private bool ValidateSwitchCompletion(GenreTemplateConfig targetTemplate)
        {
            // Check if switch was successful
            bool isValid = targetTemplate.Validate();
            
            // Additional validation checks
            if (!targetTemplate.CanSwitchTemplateIn3Minutes())
            {
                Debug.LogError("Template switch time validation failed");
                isValid = false;
            }
            
            return isValid;
        }
        
        private IEnumerator RollbackGenreSwitch(GenreType previousGenre)
        {
            Debug.Log($"Rolling back to {previousGenre}...");
            
            var previousTemplate = templateRegistry.GetTemplate(previousGenre);
            if (previousTemplate != null)
            {
                currentGenre = previousGenre;
                currentTemplate = previousTemplate;
                yield return StartCoroutine(ApplyTargetConfiguration(previousTemplate));
            }
        }
        
        /// <summary>
        /// 利用可能なジャンル一覧を取得
        /// </summary>
        public IEnumerable<GenreType> GetAvailableGenres()
        {
            return templateRegistry.RegisteredGenres;
        }
        
        /// <summary>
        /// ジャンルテンプレート設定を取得
        /// </summary>
        public GenreTemplateConfig GetGenreTemplate(GenreType genreType)
        {
            return templateRegistry.GetTemplate(genreType);
        }
        
        /// <summary>
        /// 優先度順ジャンル一覧を取得
        /// </summary>
        public IEnumerable<GenreTemplateConfig> GetGenresByPriority()
        {
            return templateRegistry.GetTemplatesByPriority();
        }
        
        /// <summary>
        /// 切り替え統計情報を取得
        /// </summary>
        public SwitchingStats GetSwitchingStats()
        {
            return stats;
        }
        
        /// <summary>
        /// レジストリ統計情報を取得
        /// </summary>
        public TemplateRegistryStats GetRegistryStats()
        {
            return templateRegistry.GetStats();
        }
        
        #if UNITY_EDITOR
        [ContextMenu("Validate All Templates")]
        private void ValidateAllTemplates()
        {
            templateRegistry.ValidateRegistry();
        }
        
        [ContextMenu("Print Registry Stats")]
        private void PrintRegistryStats()
        {
            Debug.Log(templateRegistry.GetStats().ToString());
        }
        
        [ContextMenu("Print Switching Stats")]
        private void PrintSwitchingStats()
        {
            Debug.Log($"Switching Stats: {stats.totalSwitches} total, {stats.averageSwitchTime:F2}s avg, {stats.failedSwitches} failed");
        }
        #endif
    }
}
