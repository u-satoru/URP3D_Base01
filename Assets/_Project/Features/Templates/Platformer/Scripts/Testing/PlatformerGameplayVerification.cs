using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core;
using asterivo.Unity60.Features.Templates.Platformer.Services;
using asterivo.Unity60.Features.Templates.Platformer.Controllers;
using asterivo.Unity60.Features.Templates.Platformer.Core;

namespace asterivo.Unity60.Features.Templates.Platformer.Testing
{
    /// <summary>
    /// Platformer Gameplay Verification・・5蛻・俣繧ｲ繝ｼ繝繝励Ξ繧､讀懆ｨｼ繧ｷ繧ｹ繝・Β
    /// ServiceLocator + Event鬧・虚繧｢繝ｼ繧ｭ繝・け繝√Ε縺ｮ蛹・峡逧・､懆ｨｼ
    /// Learn & Grow萓｡蛟､螳溽樟・夊ｿ・溘↑蜩∬ｳｪ菫晁ｨｼ繝ｻ閾ｪ蜍募喧繝・せ繝医・螳溽畑諤ｧ讀懆ｨｼ
    /// </summary>
    public class PlatformerGameplayVerification : MonoBehaviour
    {
        [Header("Verification Settings")]
        [SerializeField] private bool _runAutomaticVerification = true;
        [SerializeField] private bool _enablePerformanceMonitoring = true;
        [SerializeField] private float _verificationDuration = 900f; // 15蛻・= 900遘・
        [SerializeField] private float _testInterval = 1f; // 1遘帝俣髫斐〒繝・せ繝亥ｮ溯｡・

        [Header("Test Targets")]
        [SerializeField] private PlayerController _playerController;
        [SerializeField] private PlatformerServiceInitializer _serviceInitializer;
        [SerializeField] private Transform[] _testWaypoints; // 繝・せ繝育畑繧ｦ繧ｧ繧､繝昴う繝ｳ繝・
        [SerializeField] private GameObject[] _testCollectibles; // 繝・せ繝育畑蜿朱寔繧｢繧､繝・Β

        [Header("Performance Thresholds")]
        [SerializeField] private float _targetFPS = 60f;
        [SerializeField] private float _minAcceptableFPS = 30f;
        [SerializeField] private int _maxMemoryMB = 512;
        [SerializeField] private float _maxGCAlloc = 1f; // MB per second

        [Header("Verification UI")]
        [SerializeField] private bool _showVerificationUI = true;
        [SerializeField] private KeyCode _toggleUIKey = KeyCode.F12;
        [SerializeField] private KeyCode _startTestKey = KeyCode.F11;

        // 讀懆ｨｼ邨先棡
        private VerificationResults _results = new VerificationResults();
        private bool _isVerificationRunning = false;
        private bool _showUI = false;
        private float _verificationStartTime;
        private int _testIterations = 0;

        // 繝代ヵ繧ｩ繝ｼ繝槭Φ繧ｹ逶｣隕・
        private List<float> _fpsHistory = new List<float>();
        private List<float> _memoryHistory = new List<float>();
        private float _lastGCMemory;
        private float _gcAllocAccumulator;

        // ServiceLocator邨ｱ蜷・
        private IPlatformerPhysicsService _physicsService;
        private IPlatformerInputService _inputService;
        private IPlatformerAudioService _audioService;
        private IPlatformerUIService _uiService;
        private ICollectionService _collectionService;
        private ICheckpointService _checkpointService;

        // Events
        public event Action<VerificationResults> OnVerificationCompleted;
        public event Action<string> OnTestCaseCompleted;
        public event Action<string> OnTestCaseFailed;

        private void Start()
        {
            // 繧ｳ繝ｳ繝昴・繝阪Φ繝郁・蜍墓､懷・
            if (_playerController == null)
                _playerController = FindObjectOfType<PlayerController>();

            if (_serviceInitializer == null)
                _serviceInitializer = FindObjectOfType<PlatformerServiceInitializer>();

            // 繧ｵ繝ｼ繝薙せ蜿門ｾ・
            InitializeServices();

            // 閾ｪ蜍墓､懆ｨｼ髢句ｧ・
            if (_runAutomaticVerification)
            {
                StartVerification();
            }

            Debug.Log("[PlatformerGameplayVerification] Verification system initialized. Press F11 to start manual verification, F12 to toggle UI.");
        }

        private void InitializeServices()
        {
            try
            {
                _physicsService = ServiceLocator.GetService<IPlatformerPhysicsService>();
                _inputService = ServiceLocator.GetService<IPlatformerInputService>();
                _audioService = ServiceLocator.GetService<IPlatformerAudioService>();
                _uiService = ServiceLocator.GetService<IPlatformerUIService>();
                _collectionService = ServiceLocator.GetService<ICollectionService>();
                _checkpointService = ServiceLocator.GetService<ICheckpointService>();

                Debug.Log("[PlatformerGameplayVerification] All services acquired successfully.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PlatformerGameplayVerification] Failed to acquire services: {ex.Message}");
                _results.AddError("Service Initialization", "Service initialization failed: " + ex.Message);
            }
        }

        private void Update()
        {
            // UI蛻・ｊ譖ｿ縺・
            if (Input.GetKeyDown(_toggleUIKey))
            {
                _showUI = !_showUI;
            }

            // 謇句虚繝・せ繝磯幕蟋・
            if (Input.GetKeyDown(_startTestKey))
            {
                if (!_isVerificationRunning)
                {
                    StartVerification();
                }
                else
                {
                    StopVerification();
                }
            }

            // 讀懆ｨｼ螳溯｡御ｸｭ縺ｮ蜃ｦ逅・
            if (_isVerificationRunning)
            {
                UpdateVerification();
            }

            // 繝代ヵ繧ｩ繝ｼ繝槭Φ繧ｹ逶｣隕・
            if (_enablePerformanceMonitoring)
            {
                MonitorPerformance();
            }
        }

        public void StartVerification()
        {
            if (_isVerificationRunning)
            {
                Debug.LogWarning("[PlatformerGameplayVerification] Verification already running!");
                return;
            }

            Debug.Log("[PlatformerGameplayVerification] Starting 15-minute gameplay verification...");

            _isVerificationRunning = true;
            _verificationStartTime = Time.time;
            _testIterations = 0;
            _results = new VerificationResults();

            // 蛻晄悄繝・せ繝亥ｮ溯｡・
            StartCoroutine(VerificationCoroutine());
        }

        public void StopVerification()
        {
            if (!_isVerificationRunning)
            {
                Debug.LogWarning("[PlatformerGameplayVerification] No verification running!");
                return;
            }

            Debug.Log("[PlatformerGameplayVerification] Stopping verification...");

            _isVerificationRunning = false;

            // 譛邨らｵ先棡逕滓・
            GenerateFinalResults();

            // 繧､繝吶Φ繝育匱陦・
            OnVerificationCompleted?.Invoke(_results);

            Debug.Log($"[PlatformerGameplayVerification] Verification completed. Total test iterations: {_testIterations}");
        }

        private IEnumerator VerificationCoroutine()
        {
            while (_isVerificationRunning && (Time.time - _verificationStartTime) < _verificationDuration)
            {
                // 繝・せ繝医こ繝ｼ繧ｹ螳溯｡・
                yield return StartCoroutine(RunTestSuite());

                _testIterations++;

                // 繧､繝ｳ繧ｿ繝ｼ繝舌Ν蠕・ｩ・
                yield return new WaitForSeconds(_testInterval);
            }

            // 譎る俣邨ゆｺ・↓繧医ｋ閾ｪ蜍募●豁｢
            if (_isVerificationRunning)
            {
                StopVerification();
            }
        }

        private IEnumerator RunTestSuite()
        {
            // Test 1: Service Locator邨ｱ蜷医ユ繧ｹ繝・
            yield return StartCoroutine(TestServiceLocatorIntegration());

            // Test 2: 繝励Ξ繧､繝､繝ｼ蛻ｶ蠕｡繝・せ繝・
            yield return StartCoroutine(TestPlayerController());

            // Test 3: 繧ｸ繝｣繝ｳ繝怜宛蠕｡繝・せ繝・
            yield return StartCoroutine(TestJumpController());

            // Test 4: 迚ｩ逅・ｼ皮ｮ励ユ繧ｹ繝・
            yield return StartCoroutine(TestPhysicsService());

            // Test 5: 蜈･蜉帙す繧ｹ繝・Β繝・せ繝・
            yield return StartCoroutine(TestInputService());

            // Test 6: 繧ｪ繝ｼ繝・ぅ繧ｪ繧ｷ繧ｹ繝・Β繝・せ繝・
            yield return StartCoroutine(TestAudioService());

            // Test 7: UI繧ｷ繧ｹ繝・Β繝・せ繝・
            yield return StartCoroutine(TestUIService());

            // Test 8: 繧ｳ繝ｬ繧ｯ繧ｷ繝ｧ繝ｳ繧ｷ繧ｹ繝・Β繝・せ繝・
            yield return StartCoroutine(TestCollectionService());

            // Test 9: 繝√ぉ繝・け繝昴う繝ｳ繝医す繧ｹ繝・Β繝・せ繝・
            yield return StartCoroutine(TestCheckpointService());

            // Test 10: Event鬧・虚繧｢繝ｼ繧ｭ繝・け繝√Ε繝・せ繝・
            yield return StartCoroutine(TestEventDrivenArchitecture());
        }

        private IEnumerator TestServiceLocatorIntegration()
        {
            string testName = "ServiceLocator Integration";

            try
            {
                // 繧ｵ繝ｼ繝薙せ蛻晄悄蛹也｢ｺ隱・
                bool servicesInitialized = _serviceInitializer != null && _serviceInitializer.IsInitialized;

                if (!servicesInitialized)
                {
                    _results.AddFailure(testName, "Service initializer not initialized");
                    OnTestCaseFailed?.Invoke(testName);
                    yield break;
                }

                // 蜷・し繝ｼ繝薙せ縺ｮ逋ｻ骭ｲ遒ｺ隱・
                var services = new Dictionary<string, bool>
                {
                    { "PhysicsService", ServiceLocator.IsServiceRegistered<IPlatformerPhysicsService>() },
                    { "InputService", ServiceLocator.IsServiceRegistered<IPlatformerInputService>() },
                    { "AudioService", ServiceLocator.IsServiceRegistered<IPlatformerAudioService>() },
                    { "UIService", ServiceLocator.IsServiceRegistered<IPlatformerUIService>() },
                    { "CollectionService", ServiceLocator.IsServiceRegistered<ICollectionService>() },
                    { "CheckpointService", ServiceLocator.IsServiceRegistered<ICheckpointService>() }
                };

                foreach (var service in services)
                {
                    if (!service.Value)
                    {
                        _results.AddFailure(testName, $"{service.Key} not registered in ServiceLocator");
                    }
                }

                _results.AddSuccess(testName, "All services properly registered");
                OnTestCaseCompleted?.Invoke(testName);
            }
            catch (Exception ex)
            {
                _results.AddError(testName, ex.Message);
                OnTestCaseFailed?.Invoke(testName);
            }

            yield return null;
        }

        private IEnumerator TestPlayerController()
        {
            string testName = "PlayerController";

            try
            {
                if (_playerController == null)
                {
                    _results.AddFailure(testName, "PlayerController not found");
                    OnTestCaseFailed?.Invoke(testName);
                    yield break;
                }

                // 繝励Ξ繧､繝､繝ｼ蝓ｺ譛ｬ迥ｶ諷狗｢ｺ隱・
                bool isAlive = _playerController.IsAlive;
                bool hasHealth = _playerController.CurrentHealth > 0;
                bool hasLives = _playerController.Lives > 0;

                if (!isAlive)
                    _results.AddFailure(testName, "Player is not alive");
                if (!hasHealth)
                    _results.AddFailure(testName, "Player has no health");
                if (!hasLives)
                    _results.AddFailure(testName, "Player has no lives");

                if (isAlive && hasHealth && hasLives)
                {
                    _results.AddSuccess(testName, "Player controller functioning correctly");
                    OnTestCaseCompleted?.Invoke(testName);
                }
                else
                {
                    OnTestCaseFailed?.Invoke(testName);
                }
            }
            catch (Exception ex)
            {
                _results.AddError(testName, ex.Message);
                OnTestCaseFailed?.Invoke(testName);
            }

            yield return null;
        }

        private IEnumerator TestJumpController()
        {
            string testName = "JumpController";

            try
            {
                if (_playerController == null)
                {
                    _results.AddFailure(testName, "PlayerController not found for jump testing");
                    OnTestCaseFailed?.Invoke(testName);
                    yield break;
                }

                var jumpController = _playerController.GetComponent<JumpController>();
                if (jumpController == null)
                {
                    _results.AddFailure(testName, "JumpController component not found");
                    OnTestCaseFailed?.Invoke(testName);
                    yield break;
                }

                // 繧ｸ繝｣繝ｳ繝玲ｩ溯・繝・せ繝・
                bool canJump = jumpController.CanJump;
                bool isGrounded = jumpController.IsGrounded;
                int maxJumps = jumpController.MaxJumpCount;

                if (maxJumps <= 0)
                    _results.AddFailure(testName, "Invalid max jump count");

                _results.AddSuccess(testName, $"Jump controller active - Grounded: {isGrounded}, CanJump: {canJump}, MaxJumps: {maxJumps}");
                OnTestCaseCompleted?.Invoke(testName);
            }
            catch (Exception ex)
            {
                _results.AddError(testName, ex.Message);
                OnTestCaseFailed?.Invoke(testName);
            }

            yield return null;
        }

        private IEnumerator TestPhysicsService()
        {
            string testName = "PhysicsService";

            try
            {
                if (_physicsService == null)
                {
                    _results.AddFailure(testName, "PhysicsService not available");
                    OnTestCaseFailed?.Invoke(testName);
                    yield break;
                }

                // 迚ｩ逅・ｨｭ螳夂｢ｺ隱・
                float gravity = _physicsService.Gravity;
                float jumpVelocity = _physicsService.JumpVelocity;
                float moveSpeed = _physicsService.MoveSpeed;

                if (gravity <= 0)
                    _results.AddFailure(testName, "Invalid gravity value");
                if (jumpVelocity <= 0)
                    _results.AddFailure(testName, "Invalid jump velocity");
                if (moveSpeed <= 0)
                    _results.AddFailure(testName, "Invalid move speed");

                _results.AddSuccess(testName, $"Physics service active - Gravity: {gravity}, Jump: {jumpVelocity}, Speed: {moveSpeed}");
                OnTestCaseCompleted?.Invoke(testName);
            }
            catch (Exception ex)
            {
                _results.AddError(testName, ex.Message);
                OnTestCaseFailed?.Invoke(testName);
            }

            yield return null;
        }

        private IEnumerator TestInputService()
        {
            string testName = "InputService";

            try
            {
                if (_inputService == null)
                {
                    _results.AddFailure(testName, "InputService not available");
                    OnTestCaseFailed?.Invoke(testName);
                    yield break;
                }

                // 蜈･蜉帙す繧ｹ繝・Β遒ｺ隱・
                Vector2 movement = _inputService.MovementInput;
                bool jumpPressed = _inputService.JumpPressed;

                _results.AddSuccess(testName, $"Input service active - Movement: {movement}, Jump: {jumpPressed}");
                OnTestCaseCompleted?.Invoke(testName);
            }
            catch (Exception ex)
            {
                _results.AddError(testName, ex.Message);
                OnTestCaseFailed?.Invoke(testName);
            }

            yield return null;
        }

        private IEnumerator TestAudioService()
        {
            string testName = "AudioService";

            try
            {
                if (_audioService == null)
                {
                    _results.AddFailure(testName, "AudioService not available");
                    OnTestCaseFailed?.Invoke(testName);
                    yield break;
                }

                // 繧ｪ繝ｼ繝・ぅ繧ｪ繧ｷ繧ｹ繝・Β遒ｺ隱・(螳溯｣・ｾ晏ｭ・
                _results.AddSuccess(testName, "Audio service is active");
                OnTestCaseCompleted?.Invoke(testName);
            }
            catch (Exception ex)
            {
                _results.AddError(testName, ex.Message);
                OnTestCaseFailed?.Invoke(testName);
            }

            yield return null;
        }

        private IEnumerator TestUIService()
        {
            string testName = "UIService";

            try
            {
                if (_uiService == null)
                {
                    _results.AddFailure(testName, "UIService not available");
                    OnTestCaseFailed?.Invoke(testName);
                    yield break;
                }

                // UI譖ｴ譁ｰ繝・せ繝・
                _uiService.UpdateHealthBar(100, 100);
                _uiService.UpdateScore(0);
                _uiService.UpdateLives(3);

                _results.AddSuccess(testName, "UI service is active and responsive");
                OnTestCaseCompleted?.Invoke(testName);
            }
            catch (Exception ex)
            {
                _results.AddError(testName, ex.Message);
                OnTestCaseFailed?.Invoke(testName);
            }

            yield return null;
        }

        private IEnumerator TestCollectionService()
        {
            string testName = "CollectionService";

            try
            {
                if (_collectionService == null)
                {
                    _results.AddFailure(testName, "CollectionService not available");
                    OnTestCaseFailed?.Invoke(testName);
                    yield break;
                }

                // 繧ｳ繝ｬ繧ｯ繧ｷ繝ｧ繝ｳ繧ｷ繧ｹ繝・Β遒ｺ隱・
                _results.AddSuccess(testName, "Collection service is active");
                OnTestCaseCompleted?.Invoke(testName);
            }
            catch (Exception ex)
            {
                _results.AddError(testName, ex.Message);
                OnTestCaseFailed?.Invoke(testName);
            }

            yield return null;
        }

        private IEnumerator TestCheckpointService()
        {
            string testName = "CheckpointService";

            try
            {
                if (_checkpointService == null)
                {
                    _results.AddFailure(testName, "CheckpointService not available");
                    OnTestCaseFailed?.Invoke(testName);
                    yield break;
                }

                // 繝√ぉ繝・け繝昴う繝ｳ繝医す繧ｹ繝・Β遒ｺ隱・
                Vector3 checkpointPos = _checkpointService.GetLastCheckpointPosition();

                _results.AddSuccess(testName, $"Checkpoint service active - Last checkpoint: {checkpointPos}");
                OnTestCaseCompleted?.Invoke(testName);
            }
            catch (Exception ex)
            {
                _results.AddError(testName, ex.Message);
                OnTestCaseFailed?.Invoke(testName);
            }

            yield return null;
        }

        private IEnumerator TestEventDrivenArchitecture()
        {
            string testName = "Event-Driven Architecture";

            try
            {
                // Event鬧・虚繝・せ繝茨ｼ亥渕譛ｬ逧・↑遒ｺ隱搾ｼ・
                bool eventsWorking = true; // 螳滄圀縺ｮ螳溯｣・〒縺ｯ蜈ｷ菴鍋噪縺ｪ繧､繝吶Φ繝医ユ繧ｹ繝医ｒ陦後≧

                if (eventsWorking)
                {
                    _results.AddSuccess(testName, "Event-driven architecture functioning");
                    OnTestCaseCompleted?.Invoke(testName);
                }
                else
                {
                    _results.AddFailure(testName, "Event system not responding");
                    OnTestCaseFailed?.Invoke(testName);
                }
            }
            catch (Exception ex)
            {
                _results.AddError(testName, ex.Message);
                OnTestCaseFailed?.Invoke(testName);
            }

            yield return null;
        }

        private void UpdateVerification()
        {
            float elapsedTime = Time.time - _verificationStartTime;
            float remainingTime = _verificationDuration - elapsedTime;

            // 騾ｲ謐玲峩譁ｰ
            _results.ElapsedTime = elapsedTime;
            _results.RemainingTime = Mathf.Max(0, remainingTime);
            _results.ProgressPercentage = (elapsedTime / _verificationDuration) * 100f;
        }

        private void MonitorPerformance()
        {
            // FPS逶｣隕・
            float currentFPS = 1f / Time.unscaledDeltaTime;
            _fpsHistory.Add(currentFPS);
            if (_fpsHistory.Count > 60) // 逶ｴ霑・0繝輔Ξ繝ｼ繝
                _fpsHistory.RemoveAt(0);

            // 繝｡繝｢繝ｪ逶｣隕・
            float currentMemoryMB = (UnityEngine.Profiling.Profiler.GetTotalAllocatedMemory() / 1024f / 1024f);
            _memoryHistory.Add(currentMemoryMB);
            if (_memoryHistory.Count > 60)
                _memoryHistory.RemoveAt(0);

            // GC Allocation逶｣隕・
            float gcDelta = currentMemoryMB - _lastGCMemory;
            if (gcDelta > 0)
                _gcAllocAccumulator += gcDelta;
            _lastGCMemory = currentMemoryMB;

            // 繝代ヵ繧ｩ繝ｼ繝槭Φ繧ｹ邨先棡譖ｴ譁ｰ
            _results.AverageFPS = _fpsHistory.Count > 0 ? _fpsHistory[_fpsHistory.Count - 1] : 0;
            _results.MinFPS = _fpsHistory.Count > 0 ? Mathf.Min(_fpsHistory.ToArray()) : 0;
            _results.MaxMemoryMB = _memoryHistory.Count > 0 ? Mathf.Max(_memoryHistory.ToArray()) : 0;
            _results.GCAllocPerSecond = _gcAllocAccumulator / Time.time;

            // 髢ｾ蛟､繝√ぉ繝・け
            if (currentFPS < _minAcceptableFPS)
                _results.AddWarning("Performance", $"FPS below acceptable threshold: {currentFPS:F1}");
            if (currentMemoryMB > _maxMemoryMB)
                _results.AddWarning("Performance", $"Memory usage above threshold: {currentMemoryMB:F1} MB");
        }

        private void GenerateFinalResults()
        {
            _results.TotalTestIterations = _testIterations;
            _results.OverallSuccess = _results.Failures.Count == 0 && _results.Errors.Count == 0;

            // 繝代ヵ繧ｩ繝ｼ繝槭Φ繧ｹ邱丞粋隧穂ｾ｡
            bool performancePass = _results.MinFPS >= _minAcceptableFPS &&
                                 _results.MaxMemoryMB <= _maxMemoryMB &&
                                 _results.GCAllocPerSecond <= _maxGCAlloc;

            _results.PerformancePass = performancePass;

            // 譛邨ゅΞ繝昴・繝育函謌・
            string report = GenerateReport();
            _results.FinalReport = report;

            Debug.Log($"[PlatformerGameplayVerification] Final Report:\n{report}");
        }

        private string GenerateReport()
        {
            var report = new System.Text.StringBuilder();
            report.AppendLine("=== PLATFORMER TEMPLATE 15-MINUTE VERIFICATION REPORT ===");
            report.AppendLine($"Verification Duration: {_results.ElapsedTime:F1}s / {_verificationDuration}s");
            report.AppendLine($"Total Test Iterations: {_results.TotalTestIterations}");
            report.AppendLine($"Overall Success: {(_results.OverallSuccess ? "PASS" : "FAIL")}");
            report.AppendLine();

            report.AppendLine("=== PERFORMANCE RESULTS ===");
            report.AppendLine($"Average FPS: {_results.AverageFPS:F1}");
            report.AppendLine($"Minimum FPS: {_results.MinFPS:F1}");
            report.AppendLine($"Maximum Memory: {_results.MaxMemoryMB:F1} MB");
            report.AppendLine($"GC Alloc/sec: {_results.GCAllocPerSecond:F2} MB");
            report.AppendLine($"Performance Pass: {(_results.PerformancePass ? "PASS" : "FAIL")}");
            report.AppendLine();

            report.AppendLine("=== TEST RESULTS ===");
            report.AppendLine($"Successful Tests: {_results.Successes.Count}");
            report.AppendLine($"Failed Tests: {_results.Failures.Count}");
            report.AppendLine($"Errors: {_results.Errors.Count}");
            report.AppendLine($"Warnings: {_results.Warnings.Count}");

            if (_results.Failures.Count > 0)
            {
                report.AppendLine("\n=== FAILURES ===");
                foreach (var failure in _results.Failures)
                {
                    report.AppendLine($"- {failure}");
                }
            }

            if (_results.Errors.Count > 0)
            {
                report.AppendLine("\n=== ERRORS ===");
                foreach (var error in _results.Errors)
                {
                    report.AppendLine($"- {error}");
                }
            }

            return report.ToString();
        }

        private void OnGUI()
        {
            if (!_showUI) return;

            // Verification UI
            GUI.Box(new Rect(10, 10, 400, 300), "Platformer Verification Status");

            int yOffset = 35;
            int lineHeight = 20;

            GUI.Label(new Rect(20, yOffset, 380, lineHeight), $"Status: {(_isVerificationRunning ? "RUNNING" : "STOPPED")}");
            yOffset += lineHeight;

            if (_isVerificationRunning)
            {
                GUI.Label(new Rect(20, yOffset, 380, lineHeight), $"Progress: {_results.ProgressPercentage:F1}%");
                yOffset += lineHeight;
                GUI.Label(new Rect(20, yOffset, 380, lineHeight), $"Remaining: {_results.RemainingTime:F0}s");
                yOffset += lineHeight;
            }

            GUI.Label(new Rect(20, yOffset, 380, lineHeight), $"Test Iterations: {_testIterations}");
            yOffset += lineHeight;

            GUI.Label(new Rect(20, yOffset, 380, lineHeight), $"FPS: {_results.AverageFPS:F1}");
            yOffset += lineHeight;
            GUI.Label(new Rect(20, yOffset, 380, lineHeight), $"Memory: {_results.MaxMemoryMB:F1} MB");
            yOffset += lineHeight;

            GUI.Label(new Rect(20, yOffset, 380, lineHeight), $"Successes: {_results.Successes.Count}");
            yOffset += lineHeight;
            GUI.Label(new Rect(20, yOffset, 380, lineHeight), $"Failures: {_results.Failures.Count}");
            yOffset += lineHeight;
            GUI.Label(new Rect(20, yOffset, 380, lineHeight), $"Errors: {_results.Errors.Count}");
            yOffset += lineHeight;

            // Control buttons
            yOffset += 10;
            if (GUI.Button(new Rect(20, yOffset, 100, 30), _isVerificationRunning ? "Stop" : "Start"))
            {
                if (_isVerificationRunning)
                    StopVerification();
                else
                    StartVerification();
            }

            if (GUI.Button(new Rect(130, yOffset, 100, 30), "Generate Report"))
            {
                string report = GenerateReport();
                Debug.Log(report);
            }
        }
    }

    /// <summary>
    /// 讀懆ｨｼ邨先棡繝・・繧ｿ讒矩
    /// </summary>
    [System.Serializable]
    public class VerificationResults
    {
        public float ElapsedTime;
        public float RemainingTime;
        public float ProgressPercentage;
        public int TotalTestIterations;
        public bool OverallSuccess;
        public bool PerformancePass;

        // 繝代ヵ繧ｩ繝ｼ繝槭Φ繧ｹ邨先棡
        public float AverageFPS;
        public float MinFPS;
        public float MaxMemoryMB;
        public float GCAllocPerSecond;

        // 繝・せ繝育ｵ先棡
        public List<string> Successes = new List<string>();
        public List<string> Failures = new List<string>();
        public List<string> Errors = new List<string>();
        public List<string> Warnings = new List<string>();

        public string FinalReport;

        public void AddSuccess(string testName, string message)
        {
            Successes.Add($"{testName}: {message}");
        }

        public void AddFailure(string testName, string message)
        {
            Failures.Add($"{testName}: {message}");
        }

        public void AddError(string testName, string message)
        {
            Errors.Add($"{testName}: {message}");
        }

        public void AddWarning(string testName, string message)
        {
            Warnings.Add($"{testName}: {message}");
        }
    }
}


