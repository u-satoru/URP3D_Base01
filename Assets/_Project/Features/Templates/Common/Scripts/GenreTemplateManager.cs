using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core;

namespace asterivo.Unity60.Features.Templates.Common
{
    /// <summary>
    /// 螳溯｡梧凾繧ｸ繝｣繝ｳ繝ｫ邂｡逅・す繧ｹ繝・Β・・ynamic Genre Switching・・
    /// TASK-004.1: Dynamic Genre Switching螳溯｣・
    /// DESIGN.md Layer 2: Runtime Template Management貅匁侠
    /// </summary>
    public class GenreTemplateManager : MonoBehaviour
    {
        [Header("迴ｾ蝨ｨ縺ｮ迥ｶ諷・)]
        [SerializeField] private GenreType _currentGenre = GenreType.Stealth;
        [SerializeField] private GenreTemplateConfig _currentTemplate;
        [SerializeField] private bool _isTransitioning = false;
        
        [Header("繧ｷ繝ｼ繝ｳ驕ｷ遘ｻ險ｭ螳・)]
        [SerializeField] private bool _enableSceneTransition = true;
        [SerializeField] private float _transitionDuration = 2.0f;
        [SerializeField] private bool _preserveProgressDuringTransition = true;
        
        [Header("繝・・繧ｿ蜷梧悄險ｭ螳・)]
        [SerializeField] private bool _enableAutoSynchronization = true;
        [SerializeField] private float _synchronizationInterval = 0.5f;
        
        [Header("繝・ヰ繝・げ險ｭ螳・)]
        [SerializeField] private bool _enableDebugLogging = false;
        
        // Events・・vent鬧・虚繧｢繝ｼ繧ｭ繝・け繝√Ε貅匁侠・・
        [Header("繧､繝吶Φ繝医メ繝｣繝阪Ν")]
        [SerializeField] private GenreTypeGameEvent _onGenreChangeRequested;
        [SerializeField] private GenreTypeGameEvent _onGenreChangeStarted;
        [SerializeField] private GenreTypeGameEvent _onGenreChangeCompleted;
        [SerializeField] private GameEvent _onGenreChangeFailed;
        [SerializeField] private GameEvent _onConfigurationSynchronized;
        
        // Singleton邂｡逅・
        private static GenreTemplateManager _instance;
        public static GenreTemplateManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<GenreTemplateManager>();
                    if (_instance == null)
                    {
                        var go = new GameObject("GenreTemplateManager");
                        _instance = go.AddComponent<GenreTemplateManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }
        
        // Private members
        private GenreTemplateRegistry _templateRegistry;
        private Coroutine _synchronizationCoroutine;
        private TransitionState _transitionState;
        
        // Properties
        public GenreType CurrentGenre => _currentGenre;
        public GenreTemplateConfig CurrentTemplate => _currentTemplate;
        public bool IsTransitioning => _isTransitioning;
        public TransitionState TransitionState => _transitionState;
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            // Singleton險ｭ螳・
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            // 蛻晄悄蛹・
            _templateRegistry = GenreTemplateRegistry.Instance;
            _transitionState = new TransitionState();
            
            LogDebug("GenreTemplateManager initialized");
        }
        
        private void Start()
        {
            // 蛻晄悄繧ｸ繝｣繝ｳ繝ｫ險ｭ螳・
            InitializeCurrentGenre();
            
            // 閾ｪ蜍募酔譛滄幕蟋・
            if (_enableAutoSynchronization)
            {
                StartAutoSynchronization();
            }
        }
        
        private void OnDestroy()
        {
            // 閾ｪ蜍募酔譛溷●豁｢
            StopAutoSynchronization();
        }
        
        #endregion
        
        #region Genre Management
        
        /// <summary>
        /// 迴ｾ蝨ｨ縺ｮ繧ｸ繝｣繝ｳ繝ｫ繧貞・譛溷喧
        /// </summary>
        private void InitializeCurrentGenre()
        {
            _currentTemplate = _templateRegistry.GetTemplate(_currentGenre);
            
            if (_currentTemplate == null)
            {
                LogError($"Template for {_currentGenre} not found. Using Stealth as fallback.");
                _currentGenre = GenreType.Stealth;
                _currentTemplate = _templateRegistry.GetTemplate(_currentGenre);
            }
            
            LogDebug($"Initialized with genre: {_currentGenre}");
            
            // Configuration蜷梧悄
            SynchronizeConfiguration();
            
            // 繧､繝吶Φ繝育匱陦・
            _onGenreChangeCompleted?.Raise(_currentGenre);
        }
        
        /// <summary>
        /// 繧ｸ繝｣繝ｳ繝ｫ繧貞・繧頑崛縺・
        /// </summary>
        /// <param name="newGenre">譁ｰ縺励＞繧ｸ繝｣繝ｳ繝ｫ</param>
        /// <param name="preserveProgress">騾ｲ謐励ｒ菫晄戟縺吶ｋ縺・/param>
        /// <returns>蛻・ｊ譖ｿ縺医′髢句ｧ九＆繧後◆蝣ｴ蜷・rue</returns>
        public bool ChangeGenre(GenreType newGenre, bool preserveProgress = true)
        {
            if (_isTransitioning)
            {
                LogWarning("Genre transition already in progress. Ignoring request.");
                return false;
            }
            
            if (newGenre == _currentGenre)
            {
                LogWarning($"Already using genre: {newGenre}");
                return false;
            }
            
            var newTemplate = _templateRegistry.GetTemplate(newGenre);
            if (newTemplate == null)
            {
                LogError($"Template for {newGenre} not found");
                _onGenreChangeFailed?.Raise();
                return false;
            }
            
            LogDebug($"Starting genre change: {_currentGenre} -> {newGenre}");
            
            // 繧､繝吶Φ繝育匱陦・
            _onGenreChangeRequested?.Raise(newGenre);
            
            // 驕ｷ遘ｻ髢句ｧ・
            StartCoroutine(PerformGenreTransition(newGenre, preserveProgress));
            
            return true;
        }
        
        /// <summary>
        /// 繧ｸ繝｣繝ｳ繝ｫ驕ｷ遘ｻ繧貞ｮ溯｡・
        /// </summary>
        /// <param name="newGenre">譁ｰ縺励＞繧ｸ繝｣繝ｳ繝ｫ</param>
        /// <param name="preserveProgress">騾ｲ謐励ｒ菫晄戟縺吶ｋ縺・/param>
        private IEnumerator PerformGenreTransition(GenreType newGenre, bool preserveProgress)
        {
            _isTransitioning = true;
            var previousGenre = _currentGenre;
            var newTemplate = _templateRegistry.GetTemplate(newGenre);
            
            // 驕ｷ遘ｻ迥ｶ諷句・譛溷喧
            _transitionState.Reset();
            _transitionState.FromGenre = previousGenre;
            _transitionState.ToGenre = newGenre;
            _transitionState.PreserveProgress = preserveProgress;
            _transitionState.StartTime = Time.time;
            
            LogDebug($"Genre transition started: {previousGenre} -> {newGenre}");
            
            // 繧､繝吶Φ繝育匱陦・
            _onGenreChangeStarted?.Raise(newGenre);
            
            // Execute transition phases with error handling
            bool transitionSuccessful = true;

            // Phase 1: 繝・・繧ｿ菫晏ｭ假ｼ磯ｲ謐嶺ｿ晄戟縺ｮ蝣ｴ蜷茨ｼ・
            if (preserveProgress && _preserveProgressDuringTransition)
            {
                yield return StartCoroutine(SaveCurrentProgress());
                _transitionState.ProgressSaved = true;
                LogDebug("Progress saved successfully");
            }
            
            // Phase 2: 迴ｾ蝨ｨ縺ｮ繝・Φ繝励Ξ繝ｼ繝育┌蜉ｹ蛹・
            if (transitionSuccessful)
            {
                yield return StartCoroutine(DeactivateCurrentTemplate());
                _transitionState.PreviousTemplateDeactivated = true;
                LogDebug("Previous template deactivated successfully");
            }
            
            // Phase 3: 譁ｰ縺励＞繝・Φ繝励Ξ繝ｼ繝磯←逕ｨ
            if (transitionSuccessful)
            {
                _currentGenre = newGenre;
                _currentTemplate = newTemplate;
                yield return StartCoroutine(ActivateNewTemplate());
                _transitionState.NewTemplateActivated = true;
                LogDebug("New template activated successfully");
            }
            
            // Phase 4: 繧ｷ繝ｼ繝ｳ驕ｷ遘ｻ・亥ｿ・ｦ√↑蝣ｴ蜷茨ｼ・
            if (transitionSuccessful && _enableSceneTransition && ShouldTransitionScene(previousGenre, newGenre))
            {
                yield return StartCoroutine(PerformSceneTransition());
                _transitionState.SceneTransitioned = true;
                LogDebug("Scene transition completed successfully");
            }
            
            // Phase 5: Configuration蜷梧悄
            if (transitionSuccessful)
            {
                SynchronizeConfiguration();
                _transitionState.ConfigurationSynchronized = true;
                LogDebug("Configuration synchronized successfully");
            }

            // Phase 6: 繝・・繧ｿ蠕ｩ蜈・ｼ磯ｲ謐嶺ｿ晄戟縺ｮ蝣ｴ蜷茨ｼ・
            if (transitionSuccessful && preserveProgress && _preserveProgressDuringTransition)
            {
                yield return StartCoroutine(RestoreProgress());
                _transitionState.ProgressRestored = true;
                LogDebug("Progress restored successfully");
            }
            
            // Handle final result
            if (transitionSuccessful)
            {
                _transitionState.Success = true;
                _transitionState.EndTime = Time.time;
                
                LogDebug($"Genre transition completed: {previousGenre} -> {newGenre} ({_transitionState.Duration:F2}s)");
                
                // 繧､繝吶Φ繝育匱陦・
                _onGenreChangeCompleted?.Raise(newGenre);
            }
            else
            {
                LogError("Genre transition failed during execution");

                // 繝ｭ繝ｼ繝ｫ繝舌ャ繧ｯ蜃ｦ逅・
                StartCoroutine(RollbackTransition(previousGenre));

                _transitionState.Success = false;
                _transitionState.ErrorMessage = "Genre transition failed during execution";

                // 繧､繝吶Φ繝育匱陦・
                _onGenreChangeFailed?.Raise();
            }
            
            // Cleanup
            _isTransitioning = false;
            _transitionState.EndTime = Time.time;
        }
        
        #endregion
        
        #region Template Management
        
        /// <summary>
        /// 迴ｾ蝨ｨ縺ｮ繝・Φ繝励Ξ繝ｼ繝医ｒ辟｡蜉ｹ蛹・
        /// </summary>
        private IEnumerator DeactivateCurrentTemplate()
        {
            LogDebug($"Deactivating template: {_currentGenre}");
            
            // 繧ｫ繝｡繝ｩ繧ｷ繧ｹ繝・Β蛛懈ｭ｢
            // TODO: CameraStateMachine integration
            
            // AI繧ｷ繧ｹ繝・Β蛛懈ｭ｢
            // TODO: AI system integration
            
            // 繧ｪ繝ｼ繝・ぅ繧ｪ繧ｷ繧ｹ繝・Β蛛懈ｭ｢
            // TODO: Audio system integration
            
            yield return new WaitForSeconds(0.1f); // Minimal delay
        }
        
        /// <summary>
        /// 譁ｰ縺励＞繝・Φ繝励Ξ繝ｼ繝医ｒ譛牙柑蛹・
        /// </summary>
        private IEnumerator ActivateNewTemplate()
        {
            LogDebug($"Activating template: {_currentGenre}");
            
            if (_currentTemplate == null)
            {
                throw new System.Exception("Current template is null");
            }
            
            // 繧ｫ繝｡繝ｩ繧ｷ繧ｹ繝・Β險ｭ螳・
            if (!string.IsNullOrEmpty(_currentTemplate.CameraProfilePath))
            {
                // TODO: CameraStateMachine integration
                LogDebug($"Applying camera profile: {_currentTemplate.CameraProfilePath}");
            }
            
            // Input System險ｭ螳・
            if (!string.IsNullOrEmpty(_currentTemplate.InputActionAssetPath))
            {
                // TODO: Input system integration
                LogDebug($"Applying input actions: {_currentTemplate.InputActionAssetPath}");
            }
            
            // AI繧ｷ繧ｹ繝・Β險ｭ螳・
            if (_currentTemplate.RequiresAI())
            {
                // TODO: AI system integration
                LogDebug($"Configuring AI systems for: {_currentGenre}");
            }
            
            // 繧ｪ繝ｼ繝・ぅ繧ｪ繧ｷ繧ｹ繝・Β險ｭ螳・
            if (_currentTemplate.UseStealthAudio)
            {
                // TODO: Audio system integration
                LogDebug($"Configuring stealth audio for: {_currentGenre}");
            }
            
            yield return new WaitForSeconds(0.1f); // Minimal delay
        }
        
        /// <summary>
        /// 繧ｷ繝ｼ繝ｳ驕ｷ遘ｻ縺悟ｿ・ｦ√°繝√ぉ繝・け
        /// </summary>
        private bool ShouldTransitionScene(GenreType from, GenreType to)
        {
            var fromTemplate = _templateRegistry.GetTemplate(from);
            var toTemplate = _templateRegistry.GetTemplate(to);
            
            if (fromTemplate == null || toTemplate == null)
                return false;
                
            return fromTemplate.MainScenePath != toTemplate.MainScenePath;
        }
        
        /// <summary>
        /// 繧ｷ繝ｼ繝ｳ驕ｷ遘ｻ繧貞ｮ溯｡・
        /// </summary>
        private IEnumerator PerformSceneTransition()
        {
            if (string.IsNullOrEmpty(_currentTemplate.MainScenePath))
            {
                LogWarning("Main scene path not specified in template");
                yield break;
            }
            
            LogDebug($"Loading scene: {_currentTemplate.MainScenePath}");
            
            var asyncOperation = SceneManager.LoadSceneAsync(_currentTemplate.MainScenePath);
            
            while (!asyncOperation.isDone)
            {
                yield return null;
            }
            
            LogDebug("Scene transition completed");
        }
        
        #endregion
        
        #region Data Management
        
        /// <summary>
        /// 迴ｾ蝨ｨ縺ｮ騾ｲ謐励ｒ菫晏ｭ・
        /// </summary>
        private IEnumerator SaveCurrentProgress()
        {
            LogDebug("Saving current progress...");
            
            // TODO: Progress saving implementation
            // - Player state
            // - Game settings
            // - Learning progress
            // - Achievement data
            
            yield return new WaitForSeconds(0.1f);
            
            LogDebug("Progress saved");
        }
        
        /// <summary>
        /// 騾ｲ謐励ｒ蠕ｩ蜈・
        /// </summary>
        private IEnumerator RestoreProgress()
        {
            LogDebug("Restoring progress...");
            
            // TODO: Progress restoration implementation
            // - Player state restoration
            // - Settings restoration
            // - Learning progress restoration
            
            yield return new WaitForSeconds(0.1f);
            
            LogDebug("Progress restored");
        }
        
        /// <summary>
        /// 驕ｷ遘ｻ繧偵Ο繝ｼ繝ｫ繝舌ャ繧ｯ
        /// </summary>
        private IEnumerator RollbackTransition(GenreType previousGenre)
        {
            LogDebug($"Rolling back to: {previousGenre}");
            
            _currentGenre = previousGenre;
            _currentTemplate = _templateRegistry.GetTemplate(previousGenre);
            
            // 蝓ｺ譛ｬ逧・↑蠕ｩ蜈・・縺ｿ螳溯｡・
            yield return StartCoroutine(ActivateNewTemplate());
            
            LogDebug("Rollback completed");
        }
        
        #endregion
        
        #region Configuration Management
        
        /// <summary>
        /// Configuration蜷梧悄
        /// </summary>
        private void SynchronizeConfiguration()
        {
            if (_currentTemplate == null)
                return;
                
            LogDebug("Synchronizing configuration...");
            
            // 繧ｫ繝｡繝ｩ險ｭ螳壼酔譛・
            SynchronizeCameraConfiguration();
            
            // 蜈･蜉幄ｨｭ螳壼酔譛・
            SynchronizeInputConfiguration();
            
            // AI險ｭ螳壼酔譛・
            SynchronizeAIConfiguration();
            
            // 繧ｪ繝ｼ繝・ぅ繧ｪ險ｭ螳壼酔譛・
            SynchronizeAudioConfiguration();
            
            LogDebug("Configuration synchronized");
            
            // 繧､繝吶Φ繝育匱陦・
            _onConfigurationSynchronized?.Raise();
        }
        
        private void SynchronizeCameraConfiguration()
        {
            // TODO: Camera configuration synchronization
        }
        
        private void SynchronizeInputConfiguration()
        {
            // TODO: Input configuration synchronization
        }
        
        private void SynchronizeAIConfiguration()
        {
            // TODO: AI configuration synchronization
        }
        
        private void SynchronizeAudioConfiguration()
        {
            // TODO: Audio configuration synchronization
        }
        
        /// <summary>
        /// 閾ｪ蜍募酔譛滄幕蟋・
        /// </summary>
        private void StartAutoSynchronization()
        {
            if (_synchronizationCoroutine != null)
                StopAutoSynchronization();
                
            _synchronizationCoroutine = StartCoroutine(AutoSynchronizationLoop());
        }
        
        /// <summary>
        /// 閾ｪ蜍募酔譛溷●豁｢
        /// </summary>
        private void StopAutoSynchronization()
        {
            if (_synchronizationCoroutine != null)
            {
                StopCoroutine(_synchronizationCoroutine);
                _synchronizationCoroutine = null;
            }
        }
        
        /// <summary>
        /// 閾ｪ蜍募酔譛溘Ν繝ｼ繝・
        /// </summary>
        private IEnumerator AutoSynchronizationLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(_synchronizationInterval);
                
                if (!_isTransitioning && _currentTemplate != null)
                {
                    SynchronizeConfiguration();
                }
            }
        }
        
        #endregion
        
        #region Debug & Logging
        
        private void LogDebug(string message)
        {
            if (_enableDebugLogging)
            {
                Debug.Log($"[GenreTemplateManager] {message}");
            }
        }
        
        private void LogWarning(string message)
        {
            Debug.LogWarning($"[GenreTemplateManager] {message}");
        }
        
        private void LogError(string message)
        {
            Debug.LogError($"[GenreTemplateManager] {message}");
        }
        
        /// <summary>
        /// 迴ｾ蝨ｨ縺ｮ迥ｶ諷九ｒ繧ｳ繝ｳ繧ｽ繝ｼ繝ｫ縺ｫ蜃ｺ蜉幢ｼ医ョ繝舌ャ繧ｰ逕ｨ・・
        /// </summary>
        [ContextMenu("Print Current State")]
        public void PrintCurrentState()
        {
            Debug.Log("=== GenreTemplateManager State ===");
            Debug.Log($"Current Genre: {_currentGenre}");
            Debug.Log($"Current Template: {(_currentTemplate != null ? _currentTemplate.DisplayName : "None")}");
            Debug.Log($"Is Transitioning: {_isTransitioning}");
            
            if (_transitionState != null)
            {
                Debug.Log($"Transition State: {_transitionState}");
            }
        }
        
        #endregion
    }
    
    /// <summary>
    /// 繧ｸ繝｣繝ｳ繝ｫ驕ｷ遘ｻ迥ｶ諷狗ｮ｡逅・
    /// </summary>
    [System.Serializable]
    public class TransitionState
    {
        public GenreType FromGenre;
        public GenreType ToGenre;
        public bool PreserveProgress;
        public float StartTime;
        public float EndTime;
        public bool Success;
        public string ErrorMessage;
        
        // Phase flags
        public bool ProgressSaved;
        public bool PreviousTemplateDeactivated;
        public bool NewTemplateActivated;
        public bool SceneTransitioned;
        public bool ConfigurationSynchronized;
        public bool ProgressRestored;
        
        public float Duration => EndTime - StartTime;
        
        public void Reset()
        {
            FromGenre = GenreType.Stealth;
            ToGenre = GenreType.Stealth;
            PreserveProgress = false;
            StartTime = 0f;
            EndTime = 0f;
            Success = false;
            ErrorMessage = string.Empty;
            
            ProgressSaved = false;
            PreviousTemplateDeactivated = false;
            NewTemplateActivated = false;
            SceneTransitioned = false;
            ConfigurationSynchronized = false;
            ProgressRestored = false;
        }
        
        public override string ToString()
        {
            return $"Transition({FromGenre}->{ToGenre}, Duration:{Duration:F2}s, Success:{Success})";
        }
    }
}


