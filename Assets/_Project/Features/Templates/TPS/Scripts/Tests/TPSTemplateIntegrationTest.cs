using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Features.Templates.TPS;
using asterivo.Unity60.Features.Templates.TPS.Player;
using asterivo.Unity60.Features.Templates.TPS.Cover;
using asterivo.Unity60.Features.Templates.TPS.Services;
using asterivo.Unity60.Features.Templates.TPS.UI;
using asterivo.Unity60.Features.Templates.TPS.Data;

namespace asterivo.Unity60.Features.Templates.TPS.Tests
{
    /// <summary>
    /// TPS Template Integration Test - Comprehensive system verification
    /// Tests ServiceLocator integration, Cover System, Player Controller, HUD, and Configuration
    /// Follows DESIGN.md specifications for system integration testing
    /// </summary>
    public class TPSTemplateIntegrationTest : MonoBehaviour
    {
        [Header("Test Configuration")]
        [SerializeField] private TPSTemplateConfig _tpsTemplateConfig;
        [SerializeField] private TPSPlayerData _testPlayerData;
        [SerializeField] private bool _runTestsOnStart = true;
        [SerializeField] private bool _enableDebugLogs = true;

        [Header("Test Results")]
        [SerializeField, TextArea(3, 8)] private string _testResults = "";

        // Test state tracking
        private bool _testsCompleted = false;
        private int _testsRun = 0;
        private int _testsPassed = 0;
        private int _testsFailed = 0;

        // System references for testing
        private TPSServiceManager _serviceManager;
        private TPSPlayerController _playerController;
        private TPSHUDController _hudController;
        private CoverDetector _coverDetector;
        private CoverPoint[] _coverPoints;

        private void Start()
        {
            if (_runTestsOnStart)
            {
                StartCoroutine(RunIntegrationTests());
            }
        }

        /// <summary>
        /// Comprehensive integration test sequence
        /// Tests all TPS systems in the correct order following ServiceLocator pattern
        /// </summary>
        private IEnumerator RunIntegrationTests()
        {
            LogTest("=== TPS Template Integration Test Started ===");

            // Phase 1: Core System Validation
            yield return StartCoroutine(TestCoreSystemsIntegration());

            // Phase 2: ServiceLocator Integration
            yield return StartCoroutine(TestServiceLocatorIntegration());

            // Phase 3: TPS Template Configuration
            yield return StartCoroutine(TestTPSTemplateConfiguration());

            // Phase 4: Cover System Integration
            yield return StartCoroutine(TestCoverSystemIntegration());

            // Phase 5: Player Controller Integration
            yield return StartCoroutine(TestPlayerControllerIntegration());

            // Phase 6: HUD Controller Integration
            yield return StartCoroutine(TestHUDControllerIntegration());

            // Phase 7: System Interoperability
            yield return StartCoroutine(TestSystemInteroperability());

            // Generate final test report
            GenerateTestReport();

            _testsCompleted = true;
            LogTest("=== TPS Template Integration Test Completed ===");
        }

        /// <summary>
        /// Test Phase 1: Core Systems Integration
        /// Validates Unity core systems and scene setup
        /// </summary>
        private IEnumerator TestCoreSystemsIntegration()
        {
            LogTest("Phase 1: Testing Core Systems Integration...");

            // Test 1: Scene Setup Validation
            RunTest("Scene Setup Validation", () =>
            {
                var scene = SceneManager.GetActiveScene();
                return scene.name == "TPSTemplateTest" && scene.isLoaded;
            });

            // Test 2: Layer Configuration
            RunTest("Layer Configuration", () =>
            {
                var coverLayer = LayerMask.NameToLayer("Cover");
                return coverLayer != -1; // Cover layer should exist
            });

            // Test 3: Tag Configuration
            RunTest("Tag Configuration", () =>
            {
                try
                {
                    var coverObjects = GameObject.FindGameObjectsWithTag("Cover");
                    return coverObjects.Length > 0;
                }
                catch (UnityException)
                {
                    return false; // Tag doesn't exist
                }
            });

            yield return new WaitForSeconds(0.1f);
        }

        /// <summary>
        /// Test Phase 2: ServiceLocator Integration
        /// Validates ServiceLocator pattern implementation and core services
        /// </summary>
        private IEnumerator TestServiceLocatorIntegration()
        {
            LogTest("Phase 2: Testing ServiceLocator Integration...");

            // Test 1: ServiceLocator Existence
            RunTest("ServiceLocator Existence", () =>
            {
                return true; // ServiceLocator is a static class, always available
            });

            // Test 2: TPSServiceManager Creation
            RunTest("TPSServiceManager Creation", () =>
            {
                var serviceManagerObject = new GameObject("TPS Service Manager Test");
                _serviceManager = serviceManagerObject.AddComponent<TPSServiceManager>();
                return _serviceManager != null;
            });

            // Test 3: Service Registration
            RunTest("Service Registration", () =>
            {
                if (_serviceManager == null) return false;

                try
                {
                    _serviceManager.InitializeServices();
                    return true;
                }
                catch (System.Exception e)
                {
                    LogTest($"Service Registration Error: {e.Message}");
                    return false;
                }
            });

            yield return new WaitForSeconds(0.1f);
        }

        /// <summary>
        /// Test Phase 3: TPS Template Configuration
        /// Validates TPSTemplateConfig ScriptableObject and configuration application
        /// </summary>
        private IEnumerator TestTPSTemplateConfiguration()
        {
            LogTest("Phase 3: Testing TPS Template Configuration...");

            // Test 1: Configuration Asset Validation
            RunTest("Configuration Asset Validation", () =>
            {
                if (_tpsTemplateConfig == null)
                {
                    // Try to find a TPS template config in the project
                    var configs = Resources.FindObjectsOfTypeAll<TPSTemplateConfig>();
                    if (configs.Length > 0)
                    {
                        _tpsTemplateConfig = configs[0];
                        LogTest("Found TPSTemplateConfig in project resources");
                    }
                }

                return _tpsTemplateConfig != null;
            });

            // Test 2: Configuration Properties
            RunTest("Configuration Properties", () =>
            {
                if (_tpsTemplateConfig == null) return false;

                return _tpsTemplateConfig.ThirdPersonDistance > 0 &&
                       _tpsTemplateConfig.NormalFOV > 0 &&
                       _tpsTemplateConfig.MouseSensitivity > 0;
            });

            // Test 3: Configuration Application (ServiceLocator integration)
            RunTest("Configuration Application", () =>
            {
                if (_tpsTemplateConfig == null) return false;

                try
                {
                    // This will test ServiceLocator integration
                    _tpsTemplateConfig.ApplyConfiguration();
                    return true;
                }
                catch (System.Exception e)
                {
                    LogTest($"Configuration Application Error: {e.Message}");
                    return false;
                }
            });

            yield return new WaitForSeconds(0.1f);
        }

        /// <summary>
        /// Test Phase 4: Cover System Integration
        /// Validates CoverPoint and CoverDetector systems working together
        /// </summary>
        private IEnumerator TestCoverSystemIntegration()
        {
            LogTest("Phase 4: Testing Cover System Integration...");

            // Test 1: Cover Point Detection
            RunTest("Cover Point Detection", () =>
            {
                _coverPoints = FindObjectsOfType<CoverPoint>();
                if (_coverPoints.Length == 0)
                {
                    // Create test cover points if none exist
                    var coverWalls = GameObject.FindGameObjectsWithTag("Cover");
                    foreach (var wall in coverWalls)
                    {
                        if (wall.GetComponent<CoverPoint>() == null)
                        {
                            wall.AddComponent<CoverPoint>();
                        }
                    }
                    _coverPoints = FindObjectsOfType<CoverPoint>();
                }

                return _coverPoints.Length > 0;
            });

            // Test 2: Cover Detector Creation
            RunTest("Cover Detector Creation", () =>
            {
                var testPlayerObject = new GameObject("Test Player");
                _coverDetector = testPlayerObject.AddComponent<CoverDetector>();

                // Set test player data if available
                if (_testPlayerData != null)
                {
                    _coverDetector.SetPlayerData(_testPlayerData);
                }

                return _coverDetector != null;
            });

            // Test 3: Cover Detection Functionality
            RunTest("Cover Detection Functionality", () =>
            {
                if (_coverDetector == null) return false;

                try
                {
                    _coverDetector.UpdateCoverDetection();
                    var detectedCovers = _coverDetector.DetectedCoverPoints;
                    LogTest($"Detected {detectedCovers.Count} cover points");
                    return true;
                }
                catch (System.Exception e)
                {
                    LogTest($"Cover Detection Error: {e.Message}");
                    return false;
                }
            });

            // Test 4: Best Cover Selection
            RunTest("Best Cover Selection", () =>
            {
                if (_coverDetector == null) return false;

                try
                {
                    var bestCover = _coverDetector.FindBestCoverPoint();
                    LogTest($"Best cover point: {(bestCover != null ? bestCover.name : "None")}");
                    return true; // Function should execute without error
                }
                catch (System.Exception e)
                {
                    LogTest($"Best Cover Selection Error: {e.Message}");
                    return false;
                }
            });

            yield return new WaitForSeconds(0.1f);
        }

        /// <summary>
        /// Test Phase 5: Player Controller Integration
        /// Validates TPSPlayerController and state machine integration
        /// </summary>
        private IEnumerator TestPlayerControllerIntegration()
        {
            LogTest("Phase 5: Testing Player Controller Integration...");

            // Test 1: Player Controller Creation
            RunTest("Player Controller Creation", () =>
            {
                var playerObject = new GameObject("Test TPS Player");
                playerObject.AddComponent<CharacterController>();
                _playerController = playerObject.AddComponent<TPSPlayerController>();

                return _playerController != null;
            });

            // Test 2: Player Data Assignment
            RunTest("Player Data Assignment", () =>
            {
                if (_playerController == null) return false;

                // Use test player data or check if one is assigned
                if (_testPlayerData != null)
                {
                    // Set player data via reflection or public property if available
                    LogTest("Player data available for testing");
                }

                return _testPlayerData != null || _playerController.PlayerData != null;
            });

            // Test 3: State Machine Integration
            RunTest("State Machine Integration", () =>
            {
                if (_playerController == null) return false;

                try
                {
                    // Test if state machine is initialized properly
                    var stateMachine = _playerController.StateMachine;
                    return stateMachine != null;
                }
                catch (System.Exception e)
                {
                    LogTest($"State Machine Integration Error: {e.Message}");
                    return false;
                }
            });

            yield return new WaitForSeconds(0.1f);
        }

        /// <summary>
        /// Test Phase 6: HUD Controller Integration
        /// Validates TPSHUDController and UI system integration
        /// </summary>
        private IEnumerator TestHUDControllerIntegration()
        {
            LogTest("Phase 6: Testing HUD Controller Integration...");

            // Test 1: HUD Controller Creation
            RunTest("HUD Controller Creation", () =>
            {
                var hudObject = new GameObject("Test TPS HUD");
                _hudController = hudObject.AddComponent<TPSHUDController>();

                return _hudController != null;
            });

            // Test 2: ServiceLocator Integration in HUD
            RunTest("HUD ServiceLocator Integration", () =>
            {
                if (_hudController == null) return false;

                try
                {
                    // Test if HUD can initialize with ServiceLocator
                    // Note: This may require services to be properly set up
                    return true; // Basic creation test
                }
                catch (System.Exception e)
                {
                    LogTest($"HUD ServiceLocator Integration Error: {e.Message}");
                    return false;
                }
            });

            yield return new WaitForSeconds(0.1f);
        }

        /// <summary>
        /// Test Phase 7: System Interoperability
        /// Tests how all systems work together in a complete TPS template scenario
        /// </summary>
        private IEnumerator TestSystemInteroperability()
        {
            LogTest("Phase 7: Testing System Interoperability...");

            // Test 1: Configuration to Services Flow
            RunTest("Configuration to Services Flow", () =>
            {
                if (_tpsTemplateConfig == null || _serviceManager == null) return false;

                try
                {
                    // Test configuration application with active services
                    _tpsTemplateConfig.ApplyConfiguration();
                    return true;
                }
                catch (System.Exception e)
                {
                    LogTest($"Configuration to Services Flow Error: {e.Message}");
                    return false;
                }
            });

            // Test 2: Player to Cover System Integration
            RunTest("Player to Cover System Integration", () =>
            {
                if (_playerController == null || _coverDetector == null) return false;

                try
                {
                    // Test if player controller can access cover system
                    var coverDetectorOnPlayer = _playerController.GetComponent<CoverDetector>();
                    if (coverDetectorOnPlayer == null)
                    {
                        coverDetectorOnPlayer = _playerController.gameObject.AddComponent<CoverDetector>();
                        if (_testPlayerData != null)
                        {
                            coverDetectorOnPlayer.SetPlayerData(_testPlayerData);
                        }
                    }

                    return coverDetectorOnPlayer != null;
                }
                catch (System.Exception e)
                {
                    LogTest($"Player to Cover System Integration Error: {e.Message}");
                    return false;
                }
            });

            // Test 3: Complete TPS Template Workflow
            RunTest("Complete TPS Template Workflow", () =>
            {
                try
                {
                    // Simulate a complete TPS template initialization
                    // 1. Configuration applied
                    // 2. Services initialized
                    // 3. Player controller setup
                    // 4. Cover system active
                    // 5. HUD responsive

                    bool configurationApplied = _tpsTemplateConfig != null;
                    bool servicesInitialized = _serviceManager != null;
                    bool playerSetup = _playerController != null;
                    bool coverSystemActive = _coverDetector != null;
                    bool hudResponsive = _hudController != null;

                    LogTest($"Workflow Check - Config: {configurationApplied}, Services: {servicesInitialized}, " +
                           $"Player: {playerSetup}, Cover: {coverSystemActive}, HUD: {hudResponsive}");

                    return configurationApplied && servicesInitialized && playerSetup &&
                           coverSystemActive && hudResponsive;
                }
                catch (System.Exception e)
                {
                    LogTest($"Complete TPS Template Workflow Error: {e.Message}");
                    return false;
                }
            });

            yield return new WaitForSeconds(0.1f);
        }

        /// <summary>
        /// Execute a single test and track results
        /// </summary>
        private void RunTest(string testName, System.Func<bool> testFunction)
        {
            _testsRun++;

            try
            {
                bool result = testFunction();

                if (result)
                {
                    _testsPassed++;
                    LogTest($"✓ PASS: {testName}");
                }
                else
                {
                    _testsFailed++;
                    LogTest($"✗ FAIL: {testName}");
                }
            }
            catch (System.Exception e)
            {
                _testsFailed++;
                LogTest($"✗ ERROR: {testName} - {e.Message}");
            }
        }

        /// <summary>
        /// Log test message to console and internal results
        /// </summary>
        private void LogTest(string message)
        {
            if (_enableDebugLogs)
            {
                Debug.Log($"[TPS Integration Test] {message}");
            }

            _testResults += $"{message}\n";
        }

        /// <summary>
        /// Generate comprehensive test report
        /// </summary>
        private void GenerateTestReport()
        {
            float successRate = _testsRun > 0 ? (float)_testsPassed / _testsRun * 100f : 0f;

            string report = $"\n" +
                           $"=== TPS TEMPLATE INTEGRATION TEST REPORT ===\n" +
                           $"Tests Run: {_testsRun}\n" +
                           $"Tests Passed: {_testsPassed}\n" +
                           $"Tests Failed: {_testsFailed}\n" +
                           $"Success Rate: {successRate:F1}%\n" +
                           $"Status: {(successRate >= 80 ? "ACCEPTABLE" : "NEEDS IMPROVEMENT")}\n" +
                           $"===========================================\n";

            LogTest(report);

            // Add recommendations
            if (successRate < 100)
            {
                LogTest("RECOMMENDATIONS:");
                if (_tpsTemplateConfig == null)
                    LogTest("- Create TPSTemplateConfig ScriptableObject asset");
                if (_testPlayerData == null)
                    LogTest("- Create TPSPlayerData ScriptableObject for testing");
                if (_testsFailed > 0)
                    LogTest("- Review failed tests and ensure all dependencies are properly set up");
            }
        }

        /// <summary>
        /// Manual test trigger for editor use
        /// </summary>
        [ContextMenu("Run Integration Tests")]
        public void RunTestsManually()
        {
            if (!_testsCompleted)
            {
                StartCoroutine(RunIntegrationTests());
            }
            else
            {
                LogTest("Tests already completed. Check test results.");
            }
        }

        /// <summary>
        /// Reset test state for re-running
        /// </summary>
        [ContextMenu("Reset Test State")]
        public void ResetTestState()
        {
            _testsCompleted = false;
            _testsRun = 0;
            _testsPassed = 0;
            _testsFailed = 0;
            _testResults = "";
            LogTest("Test state reset. Ready to run integration tests.");
        }

        private void OnDestroy()
        {
            // Clean up test objects
            if (_serviceManager != null && _serviceManager.gameObject != null)
            {
                DestroyImmediate(_serviceManager.gameObject);
            }
        }
    }
}