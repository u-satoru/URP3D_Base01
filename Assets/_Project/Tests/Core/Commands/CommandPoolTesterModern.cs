using System.Collections;
using UnityEngine;
using asterivo.Unity60.Core.Components;
using asterivo.Unity60.Core.Services;
using Debug = UnityEngine.Debug;

namespace asterivo.Unity60.Core.Commands
{
    /// <summary>
    /// 新しいCommandPoolManagerとCommandPoolServiceの動作をテストするためのスクリプト
    /// 旧CommandPoolTesterを新しいデザインパターンに対応させた版
    /// </summary>
    public class CommandPoolTesterModern : MonoBehaviour
    {
        [Header("Modern Pool Test Settings")]
        [Tooltip("テストで作成するコマンドの数")]
        [SerializeField] private int testCommandCount = 100;
        
        [Tooltip("コマンド実行間隔（秒）")]
        [SerializeField] private float commandInterval = 0.1f;
        
        [Header("Test Controls")]
        [Tooltip("自動テストを開始する")]
        [SerializeField] private bool autoStartTest = false;
        
        [Tooltip("詳細な統計を表示する")]
        [SerializeField] private bool showDetailedStats = true;
        
        [Header("Modern Pool System")]
        [Tooltip("CommandPoolServiceを自動で作成する")]
        [SerializeField] private bool createPoolService = true;
        
        private IHealthTarget healthTarget;
        private bool testInProgress = false;
        private int commandsExecuted = 0;
        private float testStartTime;
        private CommandPoolService poolService;
        
        private void Start()
        {
            // ダミーHealthTargetを作成
            var dummyHealth = gameObject.AddComponent<DummyHealthTarget>();
            healthTarget = dummyHealth;
            
            // CommandPoolServiceの初期化
            if (createPoolService && ServiceLocator.GetService<ICommandPoolService>() == null)
            {
                var poolServiceGO = new GameObject("CommandPoolService");
                poolService = poolServiceGO.AddComponent<CommandPoolService>();
                DontDestroyOnLoad(poolServiceGO);
            }
            
            if (autoStartTest)
            {
                StartCoroutine(DelayedTestStart());
            }
        }
        
        private IEnumerator DelayedTestStart()
        {
            yield return new WaitForSeconds(1f);
            StartTest();
        }
        
        [ContextMenu("Start Modern Pool Test")]
        public void StartTest()
        {
            if (testInProgress)
            {
                UnityEngine.Debug.LogWarning("Test already in progress!");
                return;
            }
            
            var service = ServiceLocator.GetService<ICommandPoolService>();
            if (service == null)
            {
                UnityEngine.Debug.LogError("CommandPoolService not found! Please ensure it exists in the scene.");
                return;
            }
            
            UnityEngine.Debug.Log($"=== Modern CommandPool Test Started ===");
            UnityEngine.Debug.Log($"Commands: {testCommandCount}, Interval: {commandInterval}s");
            
            testInProgress = true;
            commandsExecuted = 0;
            testStartTime = Time.time;
            
            StartCoroutine(RunModernTest());
        }
        
        private IEnumerator RunModernTest()
        {
            var service = ServiceLocator.GetService<ICommandPoolService>();
            
            for (int i = 0; i < testCommandCount; i++)
            {
                // DamageCommandとHealCommandを交互にテスト
                if (i % 2 == 0)
                {
                    TestDamageCommand(service);
                }
                else
                {
                    TestHealCommand(service);
                }
                
                commandsExecuted++;
                
                // 統計を定期的に表示
                if (showDetailedStats && commandsExecuted % 20 == 0)
                {
                    LogCurrentStats(service);
                }
                
                yield return new WaitForSeconds(commandInterval);
            }
            
            // 最終統計を表示
            LogFinalStats(service);
            testInProgress = false;
        }
        
        private void TestDamageCommand(ICommandPoolService service)
        {
            // 新しいAPI使用: CommandPoolServiceから取得
            var command = service.GetCommand<DamageCommand>();
            command.Initialize(healthTarget, 10, "physical");
            
            // コマンド実行
            command.Execute();
            
            // プールに返却
            service.ReturnCommand(command);
        }
        
        private void TestHealCommand(ICommandPoolService service)
        {
            // 新しいAPI使用: CommandPoolServiceから取得
            var command = service.GetCommand<HealCommand>();
            command.Initialize(healthTarget, 5);
            
            // コマンド実行
            command.Execute();
            
            // プールに返却
            service.ReturnCommand(command);
        }
        
        private void LogCurrentStats(ICommandPoolService service)
        {
            var damageStats = service.GetStatistics<DamageCommand>();
            var healStats = service.GetStatistics<HealCommand>();
            
            UnityEngine.Debug.Log($"[{commandsExecuted}/{testCommandCount}] " +
                     $"DamageCommand - Gets: {damageStats.TotalGets}, Returns: {damageStats.TotalReturns}, Reuse: {damageStats.ReuseRatio:P1} | " +
                     $"HealCommand - Gets: {healStats.TotalGets}, Returns: {healStats.TotalReturns}, Reuse: {healStats.ReuseRatio:P1}");
        }
        
        private void LogFinalStats(ICommandPoolService service)
        {
            float testDuration = Time.time - testStartTime;
            
            UnityEngine.Debug.Log("=== Modern CommandPool Test Completed ===");
            UnityEngine.Debug.Log($"Executed {commandsExecuted} commands in {testDuration:F2} seconds");
            UnityEngine.Debug.Log($"Average: {commandsExecuted / testDuration:F1} commands/sec");
            
            // 詳細統計をCommandPoolManagerから取得
            service.LogDebugInfo();
            
            // パフォーマンス比較メトリクス
            var damageStats = service.GetStatistics<DamageCommand>();
            var healStats = service.GetStatistics<HealCommand>();
            
            int totalReused = (int)(damageStats.TotalGets * damageStats.ReuseRatio + healStats.TotalGets * healStats.ReuseRatio);
            int totalCreated = damageStats.TotalGets + healStats.TotalGets - totalReused;
            
            float memoryReduction = totalReused > 0 ? (float)totalReused / (totalCreated + totalReused) : 0f;
            
            UnityEngine.Debug.Log($"Performance Metrics:");
            UnityEngine.Debug.Log($"  Memory Reduction: {memoryReduction:P1} (reused {totalReused}/{totalCreated + totalReused})");
            UnityEngine.Debug.Log($"  Commands Created: {totalCreated} (vs {commandsExecuted} without pooling)");
        }
        
        [ContextMenu("Log Current Pool Statistics")]
        public void LogCurrentPoolStatistics()
        {
            var service = ServiceLocator.GetService<ICommandPoolService>();
            if (service != null)
            {
                UnityEngine.Debug.Log("=== Current Pool Statistics ===");
                service.LogDebugInfo();
            }
            else
            {
                UnityEngine.Debug.LogWarning("CommandPoolService not found!");
            }
        }
        
        [ContextMenu("Compare with Legacy System")]
        public void CompareWithLegacySystem()
        {
            UnityEngine.Debug.Log("=== Modern vs Legacy Comparison ===");
            UnityEngine.Debug.Log("Modern System Benefits:");
            UnityEngine.Debug.Log("  + Type-safe generic pools");
            UnityEngine.Debug.Log("  + Factory pattern for flexible creation");
            UnityEngine.Debug.Log("  + Registry pattern for organized management");
            UnityEngine.Debug.Log("  + Lifecycle management support");
            UnityEngine.Debug.Log("  + Singleton pattern access");
            UnityEngine.Debug.Log("  + Better statistics and debugging");
            UnityEngine.Debug.Log("  + Simple service access");
            UnityEngine.Debug.Log("  + Unity lifecycle independent");
        }
    }
    
    /// <summary>
    /// テスト用のダミーヘルスターゲット
    /// </summary>
    public class DummyHealthTargetModern : MonoBehaviour, IHealthTarget
    {
        private int currentHealth = 100;
        private int maxHealth = 100;
        
        public int CurrentHealth => currentHealth;
        public int MaxHealth => maxHealth;
        
        public void TakeDamage(int damage)
        {
            TakeDamage(damage, "physical");
        }
        
        public void TakeDamage(int damage, string elementType)
        {
            currentHealth = Mathf.Max(0, currentHealth - damage);
            if (showDetailedLogs)
                UnityEngine.Debug.Log($"[DummyTarget] Took {damage} {elementType} damage. Health: {currentHealth}/{maxHealth}");
        }
        
        public void Heal(int amount)
        {
            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
            if (showDetailedLogs)
                UnityEngine.Debug.Log($"[DummyTarget] Healed {amount}. Health: {currentHealth}/{maxHealth}");
        }
        
        [SerializeField] private bool showDetailedLogs = false;
    }
}