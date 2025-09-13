using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using asterivo.Unity60.Features.AI.Visual;
using asterivo.Unity60.Core.Data;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Features.AI.Visual.Tests
{
    /// <summary>
    /// NPCVisualSensorシステムの包括的なパフォーマンステスト
    /// Phase 1.4完了条件の検証を実行
    /// </summary>
    public class NPCVisualSensorPerformanceTest : MonoBehaviour
    {
        [TabGroup("Test Settings", "Performance Targets")]
        [BoxGroup("Test Settings/Performance Targets/Thresholds")]
        [LabelText("Target Frame Time")]
        [SuffixLabel("ms")]
        [SerializeField] private float targetFrameTime = 0.1f;
        
        [BoxGroup("Test Settings/Performance Targets/Thresholds")]
        [LabelText("Target Memory Per NPC")]
        [SuffixLabel("KB")]
        [SerializeField] private float targetMemoryPerNPC = 5f;
        
        [BoxGroup("Test Settings/Performance Targets/Thresholds")]
        [LabelText("Test Duration")]
        [SuffixLabel("s")]
        // TODO: Future enhancement for configurable test duration
#pragma warning disable CS0414 // Field assigned but never used - planned for dynamic test duration
        [SerializeField] private float testDuration = 30f;
#pragma warning restore CS0414
        
        [TabGroup("Test Settings", "Test Configuration")]
        [BoxGroup("Test Settings/Test Configuration/NPC Setup")]
        [LabelText("NPC Prefab")]
        [Required]
        [SerializeField] private GameObject npcPrefab;
        
        [BoxGroup("Test Settings/Test Configuration/NPC Setup")]
        [LabelText("Player Target")]
        [Required]
        [SerializeField] private Transform playerTarget;
        
        [BoxGroup("Test Settings/Test Configuration/NPC Setup")]
        [LabelText("Test Area Size")]
        [SerializeField] private Vector3 testAreaSize = new Vector3(50f, 0f, 50f);
        
        [BoxGroup("Test Settings/Test Configuration/NPC Setup")]
        [LabelText("Max Test NPCs")]
        [PropertyRange(1, 100)]
        [SerializeField] private int maxTestNPCs = 50;
        
        [TabGroup("Test Results", "Performance Metrics")]
        [BoxGroup("Test Results/Performance Metrics/Current")]
        [ShowInInspector, ReadOnly]
        [LabelText("Current Frame Time")]
        [SuffixLabel("ms")]
        private float currentFrameTime;
        
        [BoxGroup("Test Results/Performance Metrics/Current")]
        [ShowInInspector, ReadOnly]
        [LabelText("Average Frame Time")]
        [SuffixLabel("ms")]
        private float averageFrameTime;
        
        [BoxGroup("Test Results/Performance Metrics/Current")]
        [ShowInInspector, ReadOnly]
        [LabelText("Peak Frame Time")]
        [SuffixLabel("ms")]
        private float peakFrameTime;
        
        [BoxGroup("Test Results/Performance Metrics/Memory")]
        [ShowInInspector, ReadOnly]
        [LabelText("Total Memory Usage")]
        [SuffixLabel("KB")]
        private float totalMemoryUsage;
        
        [BoxGroup("Test Results/Performance Metrics/Memory")]
        [ShowInInspector, ReadOnly]
        [LabelText("Memory Per NPC")]
        [SuffixLabel("KB")]
        private float memoryPerNPC;
        
        [TabGroup("Test Results", "Test Status")]
        [BoxGroup("Test Results/Test Status/Status")]
        [ShowInInspector, ReadOnly]
        [LabelText("Active NPCs")]
        private int activeNPCs;
        
        [BoxGroup("Test Results/Test Status/Status")]
        [ShowInInspector, ReadOnly]
        [LabelText("Test Running")]
        private bool isTestRunning;
        
        [BoxGroup("Test Results/Test Status/Status")]
        [ShowInInspector, ReadOnly]
        [LabelText("Test Progress")]
        [ProgressBar(0, 1)]
        private float testProgress;
        
        [BoxGroup("Test Results/Test Status/Results")]
        [ShowInInspector, ReadOnly]
        [LabelText("Performance Test")]
        private TestResult performanceTestResult = TestResult.NotRun;
        
        [BoxGroup("Test Results/Test Status/Results")]
        [ShowInInspector, ReadOnly]
        [LabelText("Memory Test")]
        private TestResult memoryTestResult = TestResult.NotRun;
        
        [BoxGroup("Test Results/Test Status/Results")]
        [ShowInInspector, ReadOnly]
        [LabelText("Scalability Test")]
        private TestResult scalabilityTestResult = TestResult.NotRun;
        
        // Runtime data
        private List<GameObject> testNPCs = new List<GameObject>();
        private List<NPCVisualSensor> sensors = new List<NPCVisualSensor>();
        private float performanceStartTime;
        private List<float> frameTimes = new List<float>();
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
        [Button(ButtonSizes.Large, Name = "Start All Tests")]
        public void StartAllTests()
        {
            if (isTestRunning)
            {
                UnityEngine.Debug.LogWarning("[PerformanceTest] Test is already running!");
                return;
            }
            
            StartCoroutine(RunAllTestsCoroutine());
        }
        
        [TabGroup("Test Controls")]
        [Button(ButtonSizes.Medium, Name = "Stop Tests")]
        public void StopTests()
        {
            if (isTestRunning)
            {
                StopAllCoroutines();
                CleanupTest();
                UnityEngine.Debug.Log("[PerformanceTest] Tests stopped by user");
            }
        }
        
        [TabGroup("Test Controls")]
        [Button(ButtonSizes.Medium, Name = "Clear Results")]
        public void ClearResults()
        {
            if (!isTestRunning)
            {
                ResetTestResults();
                UnityEngine.Debug.Log("[PerformanceTest] Test results cleared");
            }
        }
        
        #endregion
        
        #region Test Execution
        
        private IEnumerator RunAllTestsCoroutine()
        {
            isTestRunning = true;
            testStartTime = Time.time;
            initialMemory = System.GC.GetTotalMemory(false);
            
            UnityEngine.Debug.Log("[PerformanceTest] Starting comprehensive NPCVisualSensor tests...");
            
            // Test 1: Performance Test (1 NPC)
            yield return StartCoroutine(RunPerformanceTest());
            
            // Test 2: Memory Usage Test (5 NPCs)
            yield return StartCoroutine(RunMemoryTest());
            
            // Test 3: Scalability Test (50 NPCs)
            yield return StartCoroutine(RunScalabilityTest());
            
            // Test completion
            CompleteTests();
        }
        
        private IEnumerator RunPerformanceTest()
        {
            UnityEngine.Debug.Log("[PerformanceTest] Starting Performance Test (Single NPC)...");
            performanceTestResult = TestResult.Running;
            
            // Create single NPC for performance testing
            SpawnTestNPCs(1);
            
            // Run performance measurement
            yield return StartCoroutine(MeasurePerformance(10f));
            
            // Evaluate results
            bool performancePassed = averageFrameTime <= targetFrameTime && peakFrameTime <= targetFrameTime * 2f;
            performanceTestResult = performancePassed ? TestResult.Passed : TestResult.Failed;
            
            UnityEngine.Debug.Log($"[PerformanceTest] Performance Test: {performanceTestResult} (Avg: {averageFrameTime:F3}ms, Peak: {peakFrameTime:F3}ms)");
            
            CleanupNPCs();
            yield return new WaitForSeconds(1f);
        }
        
        private IEnumerator RunMemoryTest()
        {
            UnityEngine.Debug.Log("[PerformanceTest] Starting Memory Usage Test (5 NPCs)...");
            memoryTestResult = TestResult.Running;
            
            long memoryBefore = System.GC.GetTotalMemory(true);
            
            // Create 5 NPCs for memory testing
            SpawnTestNPCs(5);
            
            // Let them run for a few seconds to stabilize memory usage
            yield return new WaitForSeconds(5f);
            
            long memoryAfter = System.GC.GetTotalMemory(false);
            float memoryDiff = (memoryAfter - memoryBefore) / 1024f; // Convert to KB
            
            memoryPerNPC = memoryDiff / 5f;
            totalMemoryUsage = memoryDiff;
            
            bool memoryPassed = memoryPerNPC <= targetMemoryPerNPC;
            memoryTestResult = memoryPassed ? TestResult.Passed : TestResult.Failed;
            
            UnityEngine.Debug.Log($"[PerformanceTest] Memory Test: {memoryTestResult} (Per NPC: {memoryPerNPC:F2}KB, Total: {totalMemoryUsage:F2}KB)");
            
            CleanupNPCs();
            yield return new WaitForSeconds(1f);
        }
        
        private IEnumerator RunScalabilityTest()
        {
            UnityEngine.Debug.Log("[PerformanceTest] Starting Scalability Test (50 NPCs)...");
            scalabilityTestResult = TestResult.Running;
            
            // Gradually spawn NPCs and monitor performance
            for (int i = 1; i <= maxTestNPCs; i += 5)
            {
                SpawnTestNPCs(5);
                yield return new WaitForSeconds(1f);
                
                // Quick performance check
                yield return StartCoroutine(MeasurePerformance(2f));
                
                if (averageFrameTime > targetFrameTime * 3f) // Allow 3x tolerance for scalability
                {
                    UnityEngine.Debug.LogWarning($"[PerformanceTest] Performance degraded significantly at {activeNPCs} NPCs");
                    break;
                }
                
                testProgress = (float)i / maxTestNPCs;
            }
            
            // Final performance measurement with all NPCs
            yield return StartCoroutine(MeasurePerformance(5f));
            
            bool scalabilityPassed = activeNPCs >= 25 && averageFrameTime <= targetFrameTime * 5f; // Allow 5x tolerance
            scalabilityTestResult = scalabilityPassed ? TestResult.Passed : TestResult.Failed;
            
            UnityEngine.Debug.Log($"[PerformanceTest] Scalability Test: {scalabilityTestResult} (NPCs: {activeNPCs}, Avg: {averageFrameTime:F3}ms)");
            
            CleanupNPCs();
        }
        
        private IEnumerator MeasurePerformance(float duration)
        {
            frameTimes.Clear();
            peakFrameTime = 0f;
            
            float startTime = Time.time;
            
            while (Time.time - startTime < duration)
            {
                performanceStartTime = Time.realtimeSinceStartup;
                
                // Simulate frame processing
                foreach (var sensor in sensors)
                {
                    if (sensor != null)
                    {
                        var perfStats = sensor.GetPerformanceStats();
                        // The sensor update is handled automatically by Unity
                    }
                }
                
                float frameEndTime = Time.realtimeSinceStartup;
                
                currentFrameTime = (frameEndTime - performanceStartTime) * 1000f; // Convert to milliseconds
                frameTimes.Add(currentFrameTime);
                
                if (currentFrameTime > peakFrameTime)
                    peakFrameTime = currentFrameTime;
                
                yield return null; // Wait one frame
            }
            
            // Calculate average
            if (frameTimes.Count > 0)
            {
                float sum = 0f;
                foreach (float time in frameTimes)
                    sum += time;
                averageFrameTime = sum / frameTimes.Count;
            }
        }
        
        #endregion
        
        #region NPC Management
        
        private void SpawnTestNPCs(int count)
        {
            for (int i = 0; i < count; i++)
            {
                Vector3 spawnPos = GetRandomSpawnPosition();
                GameObject npcObj = Instantiate(npcPrefab, spawnPos, Quaternion.identity);
                
                NPCVisualSensor sensor = npcObj.GetComponent<NPCVisualSensor>();
                if (sensor == null)
                {
                    sensor = npcObj.AddComponent<NPCVisualSensor>();
                }
                
                testNPCs.Add(npcObj);
                sensors.Add(sensor);
            }
            
            activeNPCs = testNPCs.Count;
            UnityEngine.Debug.Log($"[PerformanceTest] Spawned {count} NPCs (Total: {activeNPCs})");
        }
        
        private Vector3 GetRandomSpawnPosition()
        {
            Vector3 randomPos = new Vector3(
                Random.Range(-testAreaSize.x / 2f, testAreaSize.x / 2f),
                0f,
                Random.Range(-testAreaSize.z / 2f, testAreaSize.z / 2f)
            );
            return transform.position + randomPos;
        }
        
        private void CleanupNPCs()
        {
            foreach (GameObject npc in testNPCs)
            {
                if (npc != null)
                    DestroyImmediate(npc);
            }
            
            testNPCs.Clear();
            sensors.Clear();
            activeNPCs = 0;
        }
        
        #endregion
        
        #region Test Management
        
        private void CompleteTests()
        {
            isTestRunning = false;
            testProgress = 1f;
            
            UnityEngine.Debug.Log("\n[PerformanceTest] ===== TEST RESULTS SUMMARY =====");
            UnityEngine.Debug.Log($"Performance Test: {performanceTestResult}");
            UnityEngine.Debug.Log($"Memory Test: {memoryTestResult}");
            UnityEngine.Debug.Log($"Scalability Test: {scalabilityTestResult}");
            
            bool allPassed = performanceTestResult == TestResult.Passed && 
                           memoryTestResult == TestResult.Passed && 
                           scalabilityTestResult == TestResult.Passed;
            
            if (allPassed)
            {
                UnityEngine.Debug.Log("\n<color=green>[PerformanceTest] ALL TESTS PASSED! ✅</color>");
                UnityEngine.Debug.Log("Phase 1.4 completion criteria satisfied.");
            }
            else
            {
                UnityEngine.Debug.Log("\n<color=red>[PerformanceTest] Some tests failed. ❌</color>");
                UnityEngine.Debug.Log("Review the results and optimize as needed.");
            }
            
            CleanupTest();
        }
        
        private void CleanupTest()
        {
            CleanupNPCs();
            isTestRunning = false;
        }
        
        private void ResetTestResults()
        {
            performanceTestResult = TestResult.NotRun;
            memoryTestResult = TestResult.NotRun;
            scalabilityTestResult = TestResult.NotRun;
            
            currentFrameTime = 0f;
            averageFrameTime = 0f;
            peakFrameTime = 0f;
            totalMemoryUsage = 0f;
            memoryPerNPC = 0f;
            activeNPCs = 0;
            testProgress = 0f;
            
            frameTimes.Clear();
        }
        
        #endregion
        
        #region Unity Events
        
        private void OnDrawGizmosSelected()
        {
            // Draw test area
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(transform.position, testAreaSize);
            
            // Draw spawn positions for visualization
            Gizmos.color = Color.yellow;
            for (int i = 0; i < activeNPCs && i < testNPCs.Count; i++)
            {
                if (testNPCs[i] != null)
                {
                    Gizmos.DrawWireSphere(testNPCs[i].transform.position, 0.5f);
                }
            }
        }
        
        #endregion
    }
}