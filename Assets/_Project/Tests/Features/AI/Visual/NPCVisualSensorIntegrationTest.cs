using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using asterivo.Unity60.Features.AI.Visual;
using asterivo.Unity60.Core.Data;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Features.AI.Visual.Tests
{
    /// <summary>
    /// NPCVisualSensorシステムの統合テストシナリオ
    /// 実際のゲームプレイに近い状況でのシステム動作を検証
    /// </summary>
    public class NPCVisualSensorIntegrationTest : MonoBehaviour
    {
        [TabGroup("Test Setup", "Scenario Configuration")]
        [BoxGroup("Test Setup/Scenario Configuration/Objects")]
        [LabelText("Test NPC")]
        [Required]
        [SerializeField] private NPCVisualSensor testNPC;
        
        [BoxGroup("Test Setup/Scenario Configuration/Objects")]
        [LabelText("Player Target")]
        [Required]
        [SerializeField] private Transform playerTarget;
        
        [BoxGroup("Test Setup/Scenario Configuration/Objects")]
        [LabelText("Cover Objects")]
        [SerializeField] private List<Transform> coverObjects = new List<Transform>();
        
        [BoxGroup("Test Setup/Scenario Configuration/Waypoints")]
        [LabelText("Player Waypoints")]
        [SerializeField] private List<Transform> playerWaypoints = new List<Transform>();
        
        [BoxGroup("Test Setup/Scenario Configuration/Waypoints")]
        [LabelText("Player Movement Speed")]
        [PropertyRange(1f, 10f)]
        // TODO: Used in MovePlayerToPosition for dynamic speed calculation
        #pragma warning disable CS0414 // Field assigned but never used - planned for player movement optimization
        [SerializeField] private float playerMovementSpeed = 3f;
        #pragma warning restore CS0414
        
        [TabGroup("Test Setup", "Test Parameters")]
        [BoxGroup("Test Setup/Test Parameters/Duration")]
        [LabelText("Scenario Duration")]
        [SuffixLabel("s")]
        // TODO: Future enhancement for time-bounded testing
#pragma warning disable CS0414 // Field assigned but never used - planned for timeout functionality
        [SerializeField] private float scenarioDuration = 60f;
#pragma warning restore CS0414
        
        [BoxGroup("Test Setup/Test Parameters/Success Criteria")]
        [LabelText("Required Detections")]
        // TODO: Future enhancement for criteria-based validation
#pragma warning disable CS0414 // Field assigned but never used - planned for success criteria validation
        [SerializeField] private int requiredDetections = 5;
#pragma warning restore CS0414
        
        [BoxGroup("Test Setup/Test Parameters/Success Criteria")]
        [LabelText("Required Alert Transitions")]
        // TODO: Future enhancement for criteria-based validation  
#pragma warning disable CS0414 // Field assigned but never used - planned for success criteria validation
        [SerializeField] private int requiredAlertTransitions = 3;
#pragma warning restore CS0414
        
        [BoxGroup("Test Setup/Test Parameters/Success Criteria")]
        [LabelText("Max Memory Leaks")]
        // TODO: Future enhancement for memory leak detection
#pragma warning disable CS0414 // Field assigned but never used - planned for memory validation
        [SerializeField] private int maxAllowedMemoryLeaks = 0;
#pragma warning restore CS0414
        
        [TabGroup("Test Results", "Current Status")]
        [ShowInInspector, ReadOnly]
        [LabelText("Test Running")]
        private bool isTestRunning = false;
        
        [ShowInInspector, ReadOnly]
        [LabelText("Current Scenario")]
        // TODO: Used for dynamic scenario switching and validation
        #pragma warning disable CS0414 // Field assigned but never used - planned for scenario management
        private string currentScenario = "None";
        #pragma warning restore CS0414
        
        [ShowInInspector, ReadOnly]
        [LabelText("Test Progress")]
        [ProgressBar(0, 1)]
        // TODO: Used to show test completion progress in inspector - planned for progress monitoring
        #pragma warning disable CS0414 // Field assigned but never used - planned for progress tracking
        private float testProgress = 0f;
        #pragma warning restore CS0414
        
        [TabGroup("Test Results", "Statistics")]
        [ShowInInspector, ReadOnly]
        [LabelText("Detections Count")]
        private int detectionsCount = 0;
        
        [ShowInInspector, ReadOnly]
        [LabelText("Alert Transitions")]
        private int alertTransitions = 0;
        
        [ShowInInspector, ReadOnly]
        [LabelText("Memory Usage")]
        [SuffixLabel("KB")]
        private float memoryUsage = 0f;
        
        [ShowInInspector, ReadOnly]
        [LabelText("Average Detection Score")]
        private float averageDetectionScore = 0f;
        
        [ShowInInspector, ReadOnly]
        [LabelText("Alert Level History")]
        private List<string> alertLevelHistory = new List<string>();
        
        [TabGroup("Test Results", "Test Results")]
        [ShowInInspector, ReadOnly]
        [LabelText("Detection System Test")]
        private TestResult detectionSystemResult = TestResult.NotRun;
        
        [ShowInInspector, ReadOnly]
        [LabelText("Alert System Test")]
        private TestResult alertSystemResult = TestResult.NotRun;
        
        [ShowInInspector, ReadOnly]
        [LabelText("Memory System Test")]
        private TestResult memorySystemResult = TestResult.NotRun;
        
        [ShowInInspector, ReadOnly]
        [LabelText("Target Tracking Test")]
        private TestResult targetTrackingResult = TestResult.NotRun;
        
        [ShowInInspector, ReadOnly]
        [LabelText("Event System Test")]
        private TestResult eventSystemResult = TestResult.NotRun;
        
        // Runtime data
        private AlertLevel previousAlertLevel;
        private List<float> detectionScores = new List<float>();
        private int eventsFiredCount = 0;
        private float testStartTime;
        private long initialMemory;
        
        public enum TestResult
        {
            NotRun,
            Running,
            Passed,
            Failed
        }
        
        #region Test Control
        
        [TabGroup("Test Controls")]
        [Button(ButtonSizes.Large, Name = "Run Integration Tests")]
        public void RunIntegrationTests()
        {
            if (isTestRunning)
            {
                Debug.LogWarning("[IntegrationTest] Test is already running!");
                return;
            }
            
            if (testNPC == null || playerTarget == null)
            {
                Debug.LogError("[IntegrationTest] Missing required components!");
                return;
            }
            
            StartCoroutine(RunAllScenariosCoroutine());
        }
        
        [TabGroup("Test Controls")]
        [Button(ButtonSizes.Medium, Name = "Stop Tests")]
        public void StopTests()
        {
            if (isTestRunning)
            {
                StopAllCoroutines();
                CleanupTest();
                Debug.Log("[IntegrationTest] Tests stopped by user");
            }
        }
        
        [TabGroup("Test Controls")]
        [Button(ButtonSizes.Medium, Name = "Reset Results")]
        public void ResetResults()
        {
            if (!isTestRunning)
            {
                ResetTestData();
                Debug.Log("[IntegrationTest] Test results reset");
            }
        }
        
        #endregion
        
        #region Test Scenarios
        
        private IEnumerator RunAllScenariosCoroutine()
        {
            isTestRunning = true;
            testStartTime = Time.time;
            initialMemory = System.GC.GetTotalMemory(false);
            
            Debug.Log("[IntegrationTest] Starting NPCVisualSensor Integration Tests...");
            
            // Setup event listeners
            SetupEventListeners();
            
            // Scenario 1: Basic Detection Test
            yield return StartCoroutine(RunBasicDetectionScenario());
            
            // Scenario 2: Alert System Test
            yield return StartCoroutine(RunAlertSystemScenario());
            
            // Scenario 3: Memory System Test
            yield return StartCoroutine(RunMemorySystemScenario());
            
            // Scenario 4: Target Tracking Test
            yield return StartCoroutine(RunTargetTrackingScenario());
            
            // Scenario 5: Event System Test
            yield return StartCoroutine(RunEventSystemScenario());
            
            // Complete tests
            CompleteIntegrationTests();
        }
        
        private IEnumerator RunBasicDetectionScenario()
        {
            currentScenario = "Basic Detection";
            detectionSystemResult = TestResult.Running;
            
            Debug.Log("[IntegrationTest] Running Basic Detection Scenario...");
            
            // Move player into NPC's detection range
            Vector3 npcPos = testNPC.transform.position;
            Vector3 detectionPos = npcPos + testNPC.transform.forward * 10f;
            
            yield return StartCoroutine(MovePlayerToPosition(detectionPos, 5f));
            
            // Wait for detection
            yield return new WaitForSeconds(2f);
            
            // Check detection results
            var detectedTargets = testNPC.GetDetectedTargets();
            bool detectionWorking = detectedTargets.Count > 0;
            
            if (detectionWorking)
            {
                detectionsCount++;
                var target = detectedTargets[0];
                detectionScores.Add(target.detectionScore);
                Debug.Log($"[IntegrationTest] Detection successful! Score: {target.detectionScore:F2}");
            }
            
            detectionSystemResult = detectionWorking ? TestResult.Passed : TestResult.Failed;
            testProgress = 0.2f;
        }
        
        private IEnumerator RunAlertSystemScenario()
        {
            currentScenario = "Alert System";
            alertSystemResult = TestResult.Running;
            
            Debug.Log("[IntegrationTest] Running Alert System Scenario...");
            
            AlertLevel initialLevel = testNPC.GetCurrentAlertLevel();
            previousAlertLevel = initialLevel;
            
            // Move player closer to trigger higher alert levels
            Vector3 npcPos = testNPC.transform.position;
            Vector3 closePos = npcPos + testNPC.transform.forward * 5f;
            
            yield return StartCoroutine(MovePlayerToPosition(closePos, 3f));
            
            // Wait for alert level changes
            float waitTime = 10f;
            float startTime = Time.time;
            
            while (Time.time - startTime < waitTime)
            {
                AlertLevel currentLevel = testNPC.GetCurrentAlertLevel();
                if (currentLevel != previousAlertLevel)
                {
                    alertTransitions++;
                    alertLevelHistory.Add($"{Time.time - testStartTime:F1}s: {previousAlertLevel} -> {currentLevel}");
                    previousAlertLevel = currentLevel;
                    Debug.Log($"[IntegrationTest] Alert level changed to: {currentLevel}");
                }
                yield return new WaitForSeconds(0.5f);
            }
            
            bool alertSystemWorking = alertTransitions >= 1;
            alertSystemResult = alertSystemWorking ? TestResult.Passed : TestResult.Failed;
            testProgress = 0.4f;
        }
        
        private IEnumerator RunMemorySystemScenario()
        {
            currentScenario = "Memory System";
            memorySystemResult = TestResult.Running;
            
            Debug.Log("[IntegrationTest] Running Memory System Scenario...");
            
            // Move player away (out of sight)
            Vector3 hiddenPos = testNPC.transform.position + (-testNPC.transform.forward * 20f);
            yield return StartCoroutine(MovePlayerToPosition(hiddenPos, 5f));
            
            // Wait a bit
            yield return new WaitForSeconds(3f);
            
            // Check if NPC has memory of player
            bool hasMemory = testNPC.HasMemoryOf(playerTarget);
            Vector3 lastKnownPos = testNPC.GetLastKnownPosition(playerTarget);
            float confidence = testNPC.GetMemoryConfidence(playerTarget);
            
            Debug.Log($"[IntegrationTest] Memory check - Has Memory: {hasMemory}, Confidence: {confidence:F2}");
            
            bool memorySystemWorking = hasMemory && confidence > 0.1f;
            memorySystemResult = memorySystemWorking ? TestResult.Passed : TestResult.Failed;
            testProgress = 0.6f;
        }
        
        private IEnumerator RunTargetTrackingScenario()
        {
            currentScenario = "Target Tracking";
            targetTrackingResult = TestResult.Running;
            
            Debug.Log("[IntegrationTest] Running Target Tracking Scenario...");
            
            // Move player back into view
            Vector3 npcPos = testNPC.transform.position;
            Vector3 trackingPos = npcPos + testNPC.transform.forward * 8f;
            
            yield return StartCoroutine(MovePlayerToPosition(trackingPos, 4f));
            
            // Wait for target tracking to engage
            yield return new WaitForSeconds(2f);
            
            // Check tracking system
            var primaryTarget = testNPC.GetPrimaryTarget();
            int activeTargets = testNPC.GetActiveTargetCount();
            bool canTrackMore = testNPC.CanTrackMoreTargets();
            
            Debug.Log($"[IntegrationTest] Tracking - Primary: {primaryTarget?.transform?.name}, Active: {activeTargets}, Can Track More: {canTrackMore}");
            
            bool trackingWorking = primaryTarget != null && activeTargets > 0;
            targetTrackingResult = trackingWorking ? TestResult.Passed : TestResult.Failed;
            testProgress = 0.8f;
        }
        
        private IEnumerator RunEventSystemScenario()
        {
            currentScenario = "Event System";
            eventSystemResult = TestResult.Running;
            
            Debug.Log("[IntegrationTest] Running Event System Scenario...");
            
            int initialEventCount = eventsFiredCount;
            
            // Trigger some events by moving around
            for (int i = 0; i < 3; i++)
            {
                Vector3 randomPos = testNPC.transform.position + new Vector3(
                    Random.Range(-15f, 15f), 0f, Random.Range(-15f, 15f));
                
                yield return StartCoroutine(MovePlayerToPosition(randomPos, 3f));
                yield return new WaitForSeconds(1f);
            }
            
            int eventsGenerated = eventsFiredCount - initialEventCount;
            Debug.Log($"[IntegrationTest] Events generated during scenario: {eventsGenerated}");
            
            bool eventSystemWorking = eventsGenerated > 0;
            eventSystemResult = eventSystemWorking ? TestResult.Passed : TestResult.Failed;
            testProgress = 1f;
        }
        
        #endregion
        
        #region Helper Methods
        
        private IEnumerator MovePlayerToPosition(Vector3 targetPos, float duration)
        {
            Vector3 startPos = playerTarget.position;
            float elapsedTime = 0f;
            
            while (elapsedTime < duration)
            {
                float t = elapsedTime / duration;
                playerTarget.position = Vector3.Lerp(startPos, targetPos, t);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            playerTarget.position = targetPos;
        }
        
        private void SetupEventListeners()
        {
            // Subscribe to visual sensor events if available
            // This would be implemented based on the actual event system
        }
        
        private void OnNPCEvent()
        {
            eventsFiredCount++;
        }
        
        #endregion
        
        #region Test Management
        
        private void CompleteIntegrationTests()
        {
            isTestRunning = false;
            currentScenario = "Completed";
            
            // Calculate final statistics
            if (detectionScores.Count > 0)
            {
                float sum = 0f;
                foreach (float score in detectionScores)
                    sum += score;
                averageDetectionScore = sum / detectionScores.Count;
            }
            
            memoryUsage = (System.GC.GetTotalMemory(false) - initialMemory) / 1024f;
            
            // Output results
            Debug.Log("\n[IntegrationTest] ===== INTEGRATION TEST RESULTS =====");
            Debug.Log($"Detection System: {detectionSystemResult}");
            Debug.Log($"Alert System: {alertSystemResult} ({alertTransitions} transitions)");
            Debug.Log($"Memory System: {memorySystemResult}");
            Debug.Log($"Target Tracking: {targetTrackingResult}");
            Debug.Log($"Event System: {eventSystemResult} ({eventsFiredCount} events)");
            Debug.Log($"Average Detection Score: {averageDetectionScore:F3}");
            Debug.Log($"Memory Usage: {memoryUsage:F2} KB");
            
            bool allPassed = detectionSystemResult == TestResult.Passed &&
                           alertSystemResult == TestResult.Passed &&
                           memorySystemResult == TestResult.Passed &&
                           targetTrackingResult == TestResult.Passed &&
                           eventSystemResult == TestResult.Passed;
            
            if (allPassed)
            {
                Debug.Log("\n<color=green>[IntegrationTest] ALL INTEGRATION TESTS PASSED! ✅</color>");
            }
            else
            {
                Debug.Log("\n<color=orange>[IntegrationTest] Some integration tests need attention. ⚠️</color>");
            }
            
            CleanupTest();
        }
        
        private void CleanupTest()
        {
            isTestRunning = false;
            currentScenario = "None";
        }
        
        private void ResetTestData()
        {
            detectionSystemResult = TestResult.NotRun;
            alertSystemResult = TestResult.NotRun;
            memorySystemResult = TestResult.NotRun;
            targetTrackingResult = TestResult.NotRun;
            eventSystemResult = TestResult.NotRun;
            
            detectionsCount = 0;
            alertTransitions = 0;
            memoryUsage = 0f;
            averageDetectionScore = 0f;
            testProgress = 0f;
            eventsFiredCount = 0;
            
            detectionScores.Clear();
            alertLevelHistory.Clear();
        }
        
        #endregion
        
        #region Unity Events
        
        private void OnDrawGizmosSelected()
        {
            if (testNPC != null)
            {
                // Draw NPC detection range
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(testNPC.transform.position, 15f);
                
                // Draw player position
                if (playerTarget != null)
                {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawWireSphere(playerTarget.position, 1f);
                    
                    // Draw line between NPC and player
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(testNPC.transform.position, playerTarget.position);
                }
                
                // Draw waypoints
                Gizmos.color = Color.green;
                foreach (Transform waypoint in playerWaypoints)
                {
                    if (waypoint != null)
                    {
                        Gizmos.DrawWireCube(waypoint.position, Vector3.one);
                    }
                }
            }
        }
        
        #endregion
    }
}
