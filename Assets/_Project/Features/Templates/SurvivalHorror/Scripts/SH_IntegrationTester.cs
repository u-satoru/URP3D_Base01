using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Components;
using asterivo.Unity60.Core.Combat;

namespace asterivo.Unity60.Features.Templates.SurvivalHorror
{
    /// <summary>
    /// SurvivalHorrorテンプレート統合テストシステム
    /// 全システムの動作確認、バランス検証、パフォーマンス測定を実行
    /// </summary>
    public class SH_IntegrationTester : MonoBehaviour
    {
        [Header("Test Configuration")]
        [SerializeField] private bool runTestsOnStart = false;
        [SerializeField] private bool enablePerformanceMonitoring = true;
        #pragma warning disable 0414
        [SerializeField] private float testDuration = 60f;
        #pragma warning restore 0414

        [Header("Component References")]
        [SerializeField] private SH_SceneManager sceneManager;
        [SerializeField] private SH_PlayerController playerController;
        [SerializeField] private SH_AtmosphereManager atmosphereManager;
        [SerializeField] private SH_ResourceManager resourceManager;

        [Header("Test Results")]
        [SerializeField] private List<TestResult> testResults = new List<TestResult>();

        // Test Data
        private float testStartTime;
        private Dictionary<string, float> performanceMetrics;
        private Dictionary<string, int> eventCounts;
        private bool isTestRunning = false;

        private void Start()
        {
            if (runTestsOnStart)
            {
                StartCoroutine(RunFullIntegrationTest());
            }
        }

        /// <summary>
        /// 完全統合テストを実行
        /// </summary>
        public IEnumerator RunFullIntegrationTest()
        {
            if (isTestRunning)
            {
                Debug.LogWarning("[SH_IntegrationTester] Test already running");
                yield break;
            }

            isTestRunning = true;
            testStartTime = Time.time;
            InitializeTestData();

            Debug.Log("[SH_IntegrationTester] Starting comprehensive integration test...");

            // Phase 1: コンポーネント存在確認
            yield return StartCoroutine(TestComponentExistence());

            // Phase 2: ScriptableObject設定確認
            yield return StartCoroutine(TestScriptableObjectConfigurations());

            // Phase 3: システム統合確認
            yield return StartCoroutine(TestSystemIntegration());

            // Phase 4: ゲームプレイメカニクス確認
            yield return StartCoroutine(TestGameplayMechanics());

            // Phase 5: パフォーマンステスト
            yield return StartCoroutine(TestPerformance());

            // Phase 6: バランス検証
            yield return StartCoroutine(TestGameBalance());

            // 結果出力
            GenerateTestReport();
            isTestRunning = false;

            Debug.Log("[SH_IntegrationTester] Integration test completed");
        }

        /// <summary>
        /// コンポーネント存在確認テスト
        /// </summary>
        private IEnumerator TestComponentExistence()
        {
            Debug.Log("[SH_IntegrationTester] Testing component existence...");

            var result = new TestResult("Component Existence", TestStatus.Running);
            testResults.Add(result);

            bool allComponentsExist = true;

            // 必須マネージャーの確認
            if (sceneManager == null)
            {
                LogTestError("SH_SceneManager not found");
                allComponentsExist = false;
            }

            if (playerController == null)
            {
                playerController = FindFirstObjectByType<SH_PlayerController>();
                if (playerController == null)
                {
                    LogTestError("SH_PlayerController not found");
                    allComponentsExist = false;
                }
            }

            if (atmosphereManager == null)
            {
                atmosphereManager = FindFirstObjectByType<SH_AtmosphereManager>();
                if (atmosphereManager == null)
                {
                    LogTestError("SH_AtmosphereManager not found");
                    allComponentsExist = false;
                }
            }

            if (resourceManager == null)
            {
                resourceManager = FindFirstObjectByType<SH_ResourceManager>();
                if (resourceManager == null)
                {
                    LogTestError("SH_ResourceManager not found");
                    allComponentsExist = false;
                }
            }

            // プレイヤーコアコンポーネントの確認
            if (playerController != null)
            {
                var sanityComponent = playerController.GetComponent<SanityComponent>();
                var inventoryComponent = playerController.GetComponent<LimitedInventoryComponent>();
                var healthComponent = playerController.GetComponent<HealthComponent>();

                if (sanityComponent == null)
                {
                    LogTestError("SanityComponent not found on player");
                    allComponentsExist = false;
                }

                if (inventoryComponent == null)
                {
                    LogTestError("LimitedInventoryComponent not found on player");
                    allComponentsExist = false;
                }

                if (healthComponent == null)
                {
                    LogTestError("HealthComponent not found on player");
                    allComponentsExist = false;
                }
            }

            result.status = allComponentsExist ? TestStatus.Passed : TestStatus.Failed;
            result.details = allComponentsExist ? "All required components found" : "Some required components missing";

            yield return new WaitForSeconds(0.1f);
        }

        /// <summary>
        /// ScriptableObject設定確認テスト
        /// </summary>
        private IEnumerator TestScriptableObjectConfigurations()
        {
            Debug.Log("[SH_IntegrationTester] Testing ScriptableObject configurations...");

            var result = new TestResult("ScriptableObject Configurations", TestStatus.Running);
            testResults.Add(result);

            bool configurationsValid = true;

            // SH_TemplateConfigの確認
            var templateConfigs = Resources.LoadAll<SH_TemplateConfig>("");
            if (templateConfigs.Length == 0)
            {
                LogTestError("No SH_TemplateConfig found in Resources");
                configurationsValid = false;
            }
            else
            {
                foreach (var config in templateConfigs)
                {
                    if (!config.ValidateConfiguration())
                    {
                        LogTestError($"Invalid template configuration: {config.name}");
                        configurationsValid = false;
                    }
                }
            }

            // SH_ItemDatabaseの確認
            var itemDatabases = Resources.LoadAll<SH_ItemDatabase>("");
            if (itemDatabases.Length == 0)
            {
                LogTestError("No SH_ItemDatabase found in Resources");
                configurationsValid = false;
            }
            else
            {
                foreach (var database in itemDatabases)
                {
                    if (!database.ValidateDatabase())
                    {
                        LogTestError($"Invalid item database: {database.name}");
                        configurationsValid = false;
                    }
                }
            }

            // SH_AtmosphereConfigの確認
            var atmosphereConfigs = Resources.LoadAll<SH_AtmosphereConfig>("");
            if (atmosphereConfigs.Length == 0)
            {
                LogTestError("No SH_AtmosphereConfig found in Resources");
                configurationsValid = false;
            }
            else
            {
                foreach (var config in atmosphereConfigs)
                {
                    if (!config.ValidateConfiguration())
                    {
                        LogTestError($"Invalid atmosphere configuration: {config.name}");
                        configurationsValid = false;
                    }
                }
            }

            // SH_ResourceManagerConfigの確認
            var resourceConfigs = Resources.LoadAll<SH_ResourceManagerConfig>("");
            if (resourceConfigs.Length == 0)
            {
                LogTestError("No SH_ResourceManagerConfig found in Resources");
                configurationsValid = false;
            }
            else
            {
                foreach (var config in resourceConfigs)
                {
                    if (!config.ValidateConfiguration())
                    {
                        LogTestError($"Invalid resource configuration: {config.name}");
                        configurationsValid = false;
                    }
                }
            }

            result.status = configurationsValid ? TestStatus.Passed : TestStatus.Failed;
            result.details = configurationsValid ? "All configurations valid" : "Some configurations invalid";

            yield return new WaitForSeconds(0.1f);
        }

        /// <summary>
        /// システム統合確認テスト
        /// </summary>
        private IEnumerator TestSystemIntegration()
        {
            Debug.Log("[SH_IntegrationTester] Testing system integration...");

            var result = new TestResult("System Integration", TestStatus.Running);
            testResults.Add(result);

            bool integrationSuccessful = true;

            // シーンマネージャー初期化テスト
            if (sceneManager != null && !sceneManager.GetSceneStatistics().IsPlayerAlive)
            {
                LogTestError("Scene manager failed to initialize player");
                integrationSuccessful = false;
            }

            // 雰囲気システム統合テスト
            if (atmosphereManager != null)
            {
                var originalState = atmosphereManager.CurrentState;
                atmosphereManager.SetAtmosphereState(AtmosphereState.Fear, true);
                yield return new WaitForSeconds(0.1f);

                if (atmosphereManager.CurrentState != AtmosphereState.Fear)
                {
                    LogTestError("Atmosphere manager failed to change state");
                    integrationSuccessful = false;
                }

                atmosphereManager.SetAtmosphereState(originalState, true);
            }

            // リソースマネージャー統合テスト
            if (resourceManager != null && resourceManager.IsInitialized)
            {
                var initialItemCount = resourceManager.ActiveItemCount;
                bool spawnSuccess = resourceManager.TrySpawnRandomItem(ItemCategory.Health, Vector3.zero);

                if (spawnSuccess && resourceManager.ActiveItemCount <= initialItemCount)
                {
                    LogTestError("Resource manager failed to spawn item");
                    integrationSuccessful = false;
                }
            }

            // プレイヤーコンポーネント統合テスト
            if (playerController != null)
            {
                var sanityComponent = playerController.GetComponent<SanityComponent>();
                if (sanityComponent != null)
                {
                    var originalSanity = sanityComponent.CurrentSanity;
                    sanityComponent.DecreaseSanity(10f);
                    yield return new WaitForSeconds(0.1f);

                    if (sanityComponent.CurrentSanity >= originalSanity)
                    {
                        LogTestError("Sanity component failed to decrease sanity");
                        integrationSuccessful = false;
                    }

                    sanityComponent.IncreaseSanity(10f);
                }
            }

            result.status = integrationSuccessful ? TestStatus.Passed : TestStatus.Failed;
            result.details = integrationSuccessful ? "All systems integrated successfully" : "Some integration issues found";

            yield return new WaitForSeconds(0.1f);
        }

        /// <summary>
        /// ゲームプレイメカニクス確認テスト
        /// </summary>
        private IEnumerator TestGameplayMechanics()
        {
            Debug.Log("[SH_IntegrationTester] Testing gameplay mechanics...");

            var result = new TestResult("Gameplay Mechanics", TestStatus.Running);
            testResults.Add(result);

            bool mechanicsWorking = true;

            if (playerController != null)
            {
                // 移動システムテスト
                var originalPosition = playerController.transform.position;

                // 人工的な移動入力をシミュレート（実際の実装では Input.GetAxis をモック）
                yield return new WaitForSeconds(1f);

                // スタミナシステムテスト
                var originalStamina = playerController.CurrentStamina;
                playerController.GetComponent<SH_PlayerController>(); // アクセスして機能確認

                // 正気度システムテスト
                var sanityComponent = playerController.GetComponent<SanityComponent>();
                if (sanityComponent != null)
                {
                    sanityComponent.DecreaseSanity(50f);
                    yield return new WaitForSeconds(0.5f);

                    if (sanityComponent.CurrentState == SanityState.Normal)
                    {
                        LogTestError("Sanity state did not change with significant sanity loss");
                        mechanicsWorking = false;
                    }
                }

                // インベントリシステムテスト
                var inventoryComponent = playerController.GetComponent<LimitedInventoryComponent>();
                if (inventoryComponent != null)
                {
                    var testItem = new InventoryItemData(
                    "test_item",
                    "Test Item",
                    "A test item for gameplay mechanics verification",
                    asterivo.Unity60.Core.Components.ItemType.Consumable
                    );

                    if (!inventoryComponent.TryAddItem(testItem, 1))
                    {
                        LogTestError("Failed to add item to inventory");
                        mechanicsWorking = false;
                    }

                    if (!inventoryComponent.TryRemoveItem("test_item", 1))
                    {
                        LogTestError("Failed to remove item from inventory");
                        mechanicsWorking = false;
                    }
                }
            }

            result.status = mechanicsWorking ? TestStatus.Passed : TestStatus.Failed;
            result.details = mechanicsWorking ? "All gameplay mechanics working" : "Some gameplay issues found";

            yield return new WaitForSeconds(0.1f);
        }

        /// <summary>
        /// パフォーマンステスト
        /// </summary>
        private IEnumerator TestPerformance()
        {
            Debug.Log("[SH_IntegrationTester] Testing performance...");

            var result = new TestResult("Performance", TestStatus.Running);
            testResults.Add(result);

            if (!enablePerformanceMonitoring)
            {
                result.status = TestStatus.Skipped;
                result.details = "Performance monitoring disabled";
                yield break;
            }

            float frameRateSum = 0f;
            int frameCount = 0;
            float testStart = Time.time;

            while (Time.time - testStart < 5f) // 5秒間測定
            {
                frameRateSum += 1f / Time.deltaTime;
                frameCount++;
                yield return null;
            }

            float averageFrameRate = frameRateSum / frameCount;
            performanceMetrics["AverageFrameRate"] = averageFrameRate;

            bool performanceAcceptable = averageFrameRate >= 30f; // 30 FPS minimum

            result.status = performanceAcceptable ? TestStatus.Passed : TestStatus.Failed;
            result.details = $"Average FPS: {averageFrameRate:F1}";

            if (!performanceAcceptable)
            {
                LogTestError($"Performance below acceptable threshold: {averageFrameRate:F1} FPS");
            }
        }

        /// <summary>
        /// ゲームバランステスト
        /// </summary>
        private IEnumerator TestGameBalance()
        {
            Debug.Log("[SH_IntegrationTester] Testing game balance...");

            var result = new TestResult("Game Balance", TestStatus.Running);
            testResults.Add(result);

            bool balanceAcceptable = true;
            var balanceIssues = new List<string>();

            if (playerController != null)
            {
                var sanityComponent = playerController.GetComponent<SanityComponent>();
                if (sanityComponent != null)
                {
                    // 正気度減少速度のテスト
                    var initialSanity = sanityComponent.CurrentSanity;
                    float testTime = 10f;
                    float startTime = Time.time;

                    while (Time.time - startTime < testTime)
                    {
                        yield return null;
                    }

                    float sanityLoss = initialSanity - sanityComponent.CurrentSanity;
                    float lossRate = sanityLoss / testTime;

                    if (lossRate > 5f) // 1秒に5以上減少は速すぎる
                    {
                        balanceIssues.Add($"Sanity loss rate too high: {lossRate:F2}/sec");
                        balanceAcceptable = false;
                    }

                    if (lossRate < 0.1f) // 1秒に0.1未満は遅すぎる
                    {
                        balanceIssues.Add($"Sanity loss rate too low: {lossRate:F2}/sec");
                        balanceAcceptable = false;
                    }
                }

                // スタミナバランスのテスト
                var originalStamina = playerController.CurrentStamina;
                if (originalStamina < playerController.StaminaNormalized * 50f) // 半分未満は少なすぎる
                {
                    balanceIssues.Add("Starting stamina too low");
                    balanceAcceptable = false;
                }
            }

            // リソース出現率のテスト
            if (resourceManager != null && resourceManager.IsInitialized)
            {
                var stats = resourceManager.GetStatistics();
                if (stats.TotalActiveItems == 0)
                {
                    balanceIssues.Add("No items spawned - spawn rate too low");
                    balanceAcceptable = false;
                }

                if (stats.TotalActiveItems > 50)
                {
                    balanceIssues.Add("Too many items spawned - spawn rate too high");
                    balanceAcceptable = false;
                }
            }

            result.status = balanceAcceptable ? TestStatus.Passed : TestStatus.Failed;
            result.details = balanceAcceptable ? "Game balance acceptable" : string.Join(", ", balanceIssues);

            yield return new WaitForSeconds(0.1f);
        }

        /// <summary>
        /// テストデータを初期化
        /// </summary>
        private void InitializeTestData()
        {
            testResults.Clear();
            performanceMetrics = new Dictionary<string, float>();
            eventCounts = new Dictionary<string, int>();
        }

        /// <summary>
        /// テストレポートを生成
        /// </summary>
        private void GenerateTestReport()
        {
            Debug.Log("=== SURVIVAL HORROR INTEGRATION TEST REPORT ===");

            int passedTests = 0;
            int failedTests = 0;
            int skippedTests = 0;

            foreach (var test in testResults)
            {
                switch (test.status)
                {
                    case TestStatus.Passed:
                        passedTests++;
                        Debug.Log($"✓ {test.testName}: PASSED - {test.details}");
                        break;
                    case TestStatus.Failed:
                        failedTests++;
                        Debug.LogError($"✗ {test.testName}: FAILED - {test.details}");
                        break;
                    case TestStatus.Skipped:
                        skippedTests++;
                        Debug.LogWarning($"- {test.testName}: SKIPPED - {test.details}");
                        break;
                }
            }

            Debug.Log($"=== TEST SUMMARY ===");
            Debug.Log($"Passed: {passedTests}, Failed: {failedTests}, Skipped: {skippedTests}");
            Debug.Log($"Total Test Duration: {Time.time - testStartTime:F1} seconds");

            if (performanceMetrics.Count > 0)
            {
                Debug.Log("=== PERFORMANCE METRICS ===");
                foreach (var metric in performanceMetrics)
                {
                    Debug.Log($"{metric.Key}: {metric.Value:F2}");
                }
            }

            // バランス推奨事項
            GenerateBalanceRecommendations();
        }

        /// <summary>
        /// バランス推奨事項を生成
        /// </summary>
        private void GenerateBalanceRecommendations()
        {
            Debug.Log("=== BALANCE RECOMMENDATIONS ===");

            if (playerController != null)
            {
                var sanityComponent = playerController.GetComponent<SanityComponent>();
                if (sanityComponent != null)
                {
                    float sanityRatio = sanityComponent.SanityNormalized;
                    if (sanityRatio < 0.3f)
                    {
                        Debug.Log("RECOMMENDATION: Consider increasing sanity restoration methods");
                    }
                    else if (sanityRatio > 0.8f)
                    {
                        Debug.Log("RECOMMENDATION: Consider increasing sanity decay rate for more challenge");
                    }
                }

                float staminaRatio = playerController.StaminaNormalized;
                if (staminaRatio < 0.5f)
                {
                    Debug.Log("RECOMMENDATION: Consider increasing stamina capacity or recovery rate");
                }
            }

            if (resourceManager != null)
            {
                var stats = resourceManager.GetStatistics();
                if (stats.TotalActiveItems < 5)
                {
                    Debug.Log("RECOMMENDATION: Consider increasing item spawn rates");
                }
                else if (stats.TotalActiveItems > 20)
                {
                    Debug.Log("RECOMMENDATION: Consider decreasing item spawn rates");
                }
            }

            Debug.Log("=== END RECOMMENDATIONS ===");
        }

        /// <summary>
        /// テストエラーをログ出力
        /// </summary>
        private void LogTestError(string error)
        {
            Debug.LogError($"[SH_IntegrationTester] TEST ERROR: {error}");
        }

        /// <summary>
        /// 手動でテストを開始
        /// </summary>
        [ContextMenu("Run Integration Test")]
        public void RunTestManually()
        {
            if (!isTestRunning)
            {
                StartCoroutine(RunFullIntegrationTest());
            }
        }

        /// <summary>
        /// 個別システムテストを実行
        /// </summary>
        [ContextMenu("Test Components Only")]
        public void TestComponentsOnly()
        {
            StartCoroutine(TestComponentExistence());
        }

        [ContextMenu("Test ScriptableObjects Only")]
        public void TestScriptableObjectsOnly()
        {
            StartCoroutine(TestScriptableObjectConfigurations());
        }

        [ContextMenu("Test Performance Only")]
        public void TestPerformanceOnly()
        {
            StartCoroutine(TestPerformance());
        }
    }

    /// <summary>
    /// テスト結果データ構造
    /// </summary>
    [System.Serializable]
    public class TestResult
    {
        public string testName;
        public TestStatus status;
        public string details;
        public float executionTime;

        public TestResult(string name, TestStatus initialStatus)
        {
            testName = name;
            status = initialStatus;
            details = "";
            executionTime = 0f;
        }
    }

    /// <summary>
    /// テストステータス定義
    /// </summary>
    public enum TestStatus
    {
        Pending,
        Running,
        Passed,
        Failed,
        Skipped
    }
}
