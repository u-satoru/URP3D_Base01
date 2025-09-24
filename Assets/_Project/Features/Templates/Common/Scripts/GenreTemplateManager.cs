using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Services;

namespace asterivo.Unity60.Features.Templates.Common
{
    /// <summary>
    /// 実行時ジャンル管理システム（Dynamic Genre Switching）
    /// TASK-004.1: Dynamic Genre Switching実装
    /// DESIGN.md Layer 2: Runtime Template Management準拠
    /// </summary>
    public class GenreTemplateManager : MonoBehaviour
    {
        [Header("現在の状態")]
        [SerializeField] private GenreType _currentGenre = GenreType.Stealth;
        [SerializeField] private GenreTemplateConfig _currentTemplate;
        [SerializeField] private bool _isTransitioning = false;
        
        [Header("シーン遷移設定")]
        [SerializeField] private bool _enableSceneTransition = true;
        [SerializeField] private float _transitionDuration = 2.0f;
        [SerializeField] private bool _preserveProgressDuringTransition = true;
        
        [Header("データ同期設定")]
        [SerializeField] private bool _enableAutoSynchronization = true;
        [SerializeField] private float _synchronizationInterval = 0.5f;
        
        [Header("デバッグ設定")]
        [SerializeField] private bool _enableDebugLogging = false;
        
        // Events（Event駆動アーキテクチャ準拠）
        [Header("イベントチャネル")]
        [SerializeField] private GenreTypeGameEvent _onGenreChangeRequested;
        [SerializeField] private GenreTypeGameEvent _onGenreChangeStarted;
        [SerializeField] private GenreTypeGameEvent _onGenreChangeCompleted;
        [SerializeField] private GameEvent _onGenreChangeFailed;
        [SerializeField] private GameEvent _onConfigurationSynchronized;
        
        // Singleton管理
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
            // Singleton設定
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            // 初期化
            _templateRegistry = GenreTemplateRegistry.Instance;
            _transitionState = new TransitionState();
            
            LogDebug("GenreTemplateManager initialized");
        }
        
        private void Start()
        {
            // 初期ジャンル設定
            InitializeCurrentGenre();
            
            // 自動同期開始
            if (_enableAutoSynchronization)
            {
                StartAutoSynchronization();
            }
        }
        
        private void OnDestroy()
        {
            // 自動同期停止
            StopAutoSynchronization();
        }
        
        #endregion
        
        #region Genre Management
        
        /// <summary>
        /// 現在のジャンルを初期化
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
            
            // Configuration同期
            SynchronizeConfiguration();
            
            // イベント発行
            _onGenreChangeCompleted?.Raise(_currentGenre);
        }
        
        /// <summary>
        /// ジャンルを切り替え
        /// </summary>
        /// <param name="newGenre">新しいジャンル</param>
        /// <param name="preserveProgress">進捗を保持するか</param>
        /// <returns>切り替えが開始された場合true</returns>
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
            
            // イベント発行
            _onGenreChangeRequested?.Raise(newGenre);
            
            // 遷移開始
            StartCoroutine(PerformGenreTransition(newGenre, preserveProgress));
            
            return true;
        }
        
        /// <summary>
        /// ジャンル遷移を実行
        /// </summary>
        /// <param name="newGenre">新しいジャンル</param>
        /// <param name="preserveProgress">進捗を保持するか</param>
        private IEnumerator PerformGenreTransition(GenreType newGenre, bool preserveProgress)
        {
            _isTransitioning = true;
            var previousGenre = _currentGenre;
            var newTemplate = _templateRegistry.GetTemplate(newGenre);
            
            // 遷移状態初期化
            _transitionState.Reset();
            _transitionState.FromGenre = previousGenre;
            _transitionState.ToGenre = newGenre;
            _transitionState.PreserveProgress = preserveProgress;
            _transitionState.StartTime = Time.time;
            
            LogDebug($"Genre transition started: {previousGenre} -> {newGenre}");
            
            // イベント発行
            _onGenreChangeStarted?.Raise(newGenre);
            
            // Execute transition phases with error handling
            bool transitionSuccessful = true;

            // Phase 1: データ保存（進捗保持の場合）
            if (preserveProgress && _preserveProgressDuringTransition)
            {
                yield return StartCoroutine(SaveCurrentProgress());
                _transitionState.ProgressSaved = true;
                LogDebug("Progress saved successfully");
            }
            
            // Phase 2: 現在のテンプレート無効化
            if (transitionSuccessful)
            {
                yield return StartCoroutine(DeactivateCurrentTemplate());
                _transitionState.PreviousTemplateDeactivated = true;
                LogDebug("Previous template deactivated successfully");
            }
            
            // Phase 3: 新しいテンプレート適用
            if (transitionSuccessful)
            {
                _currentGenre = newGenre;
                _currentTemplate = newTemplate;
                yield return StartCoroutine(ActivateNewTemplate());
                _transitionState.NewTemplateActivated = true;
                LogDebug("New template activated successfully");
            }
            
            // Phase 4: シーン遷移（必要な場合）
            if (transitionSuccessful && _enableSceneTransition && ShouldTransitionScene(previousGenre, newGenre))
            {
                yield return StartCoroutine(PerformSceneTransition());
                _transitionState.SceneTransitioned = true;
                LogDebug("Scene transition completed successfully");
            }
            
            // Phase 5: Configuration同期
            if (transitionSuccessful)
            {
                SynchronizeConfiguration();
                _transitionState.ConfigurationSynchronized = true;
                LogDebug("Configuration synchronized successfully");
            }

            // Phase 6: データ復元（進捗保持の場合）
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
                
                // イベント発行
                _onGenreChangeCompleted?.Raise(newGenre);
            }
            else
            {
                LogError("Genre transition failed during execution");

                // ロールバック処理
                StartCoroutine(RollbackTransition(previousGenre));

                _transitionState.Success = false;
                _transitionState.ErrorMessage = "Genre transition failed during execution";

                // イベント発行
                _onGenreChangeFailed?.Raise();
            }
            
            // Cleanup
            _isTransitioning = false;
            _transitionState.EndTime = Time.time;
        }
        
        #endregion
        
        #region Template Management
        
        /// <summary>
        /// 現在のテンプレートを無効化
        /// </summary>
        private IEnumerator DeactivateCurrentTemplate()
        {
            LogDebug($"Deactivating template: {_currentGenre}");
            
            // カメラシステム停止
            // TODO: CameraStateMachine integration
            
            // AIシステム停止
            // TODO: AI system integration
            
            // オーディオシステム停止
            // TODO: Audio system integration
            
            yield return new WaitForSeconds(0.1f); // Minimal delay
        }
        
        /// <summary>
        /// 新しいテンプレートを有効化
        /// </summary>
        private IEnumerator ActivateNewTemplate()
        {
            LogDebug($"Activating template: {_currentGenre}");
            
            if (_currentTemplate == null)
            {
                throw new System.Exception("Current template is null");
            }
            
            // カメラシステム設定
            if (!string.IsNullOrEmpty(_currentTemplate.CameraProfilePath))
            {
                // TODO: CameraStateMachine integration
                LogDebug($"Applying camera profile: {_currentTemplate.CameraProfilePath}");
            }
            
            // Input System設定
            if (!string.IsNullOrEmpty(_currentTemplate.InputActionAssetPath))
            {
                // TODO: Input system integration
                LogDebug($"Applying input actions: {_currentTemplate.InputActionAssetPath}");
            }
            
            // AIシステム設定
            if (_currentTemplate.RequiresAI())
            {
                // TODO: AI system integration
                LogDebug($"Configuring AI systems for: {_currentGenre}");
            }
            
            // オーディオシステム設定
            if (_currentTemplate.UseStealthAudio)
            {
                // TODO: Audio system integration
                LogDebug($"Configuring stealth audio for: {_currentGenre}");
            }
            
            yield return new WaitForSeconds(0.1f); // Minimal delay
        }
        
        /// <summary>
        /// シーン遷移が必要かチェック
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
        /// シーン遷移を実行
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
        /// 現在の進捗を保存
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
        /// 進捗を復元
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
        /// 遷移をロールバック
        /// </summary>
        private IEnumerator RollbackTransition(GenreType previousGenre)
        {
            LogDebug($"Rolling back to: {previousGenre}");
            
            _currentGenre = previousGenre;
            _currentTemplate = _templateRegistry.GetTemplate(previousGenre);
            
            // 基本的な復元のみ実行
            yield return StartCoroutine(ActivateNewTemplate());
            
            LogDebug("Rollback completed");
        }
        
        #endregion
        
        #region Configuration Management
        
        /// <summary>
        /// Configuration同期
        /// </summary>
        private void SynchronizeConfiguration()
        {
            if (_currentTemplate == null)
                return;
                
            LogDebug("Synchronizing configuration...");
            
            // カメラ設定同期
            SynchronizeCameraConfiguration();
            
            // 入力設定同期
            SynchronizeInputConfiguration();
            
            // AI設定同期
            SynchronizeAIConfiguration();
            
            // オーディオ設定同期
            SynchronizeAudioConfiguration();
            
            LogDebug("Configuration synchronized");
            
            // イベント発行
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
        /// 自動同期開始
        /// </summary>
        private void StartAutoSynchronization()
        {
            if (_synchronizationCoroutine != null)
                StopAutoSynchronization();
                
            _synchronizationCoroutine = StartCoroutine(AutoSynchronizationLoop());
        }
        
        /// <summary>
        /// 自動同期停止
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
        /// 自動同期ループ
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
        /// 現在の状態をコンソールに出力（デバッグ用）
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
    /// ジャンル遷移状態管理
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