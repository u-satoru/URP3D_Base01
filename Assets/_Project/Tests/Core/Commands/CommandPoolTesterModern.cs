using System.Collections;
using UnityEngine;
using asterivo.Unity60.Core.Components;
using asterivo.Unity60.Core;
using Debug = UnityEngine.Debug;

namespace asterivo.Unity60.Core.Commands
{
    /// <summary>
    /// 譁ｰ縺励＞CommandPoolManager縺ｨCommandPoolService縺ｮ蜍穂ｽ懊ｒ繝・せ繝医☆繧九◆繧√・繧ｹ繧ｯ繝ｪ繝励ヨ
    /// 譌ｧCommandPoolTester繧呈眠縺励＞繝・じ繧､繝ｳ繝代ち繝ｼ繝ｳ縺ｫ蟇ｾ蠢懊＆縺帙◆迚・
    /// </summary>
    public class CommandPoolTesterModern : MonoBehaviour
    {
        [Header("Modern Pool Test Settings")]
        [Tooltip("繝・せ繝医〒菴懈・縺吶ｋ繧ｳ繝槭Φ繝峨・謨ｰ")]
        [SerializeField] private int testCommandCount = 100;
        
        [Tooltip("繧ｳ繝槭Φ繝牙ｮ溯｡碁俣髫費ｼ育ｧ抵ｼ・)]
        [SerializeField] private float commandInterval = 0.1f;
        
        [Header("Test Controls")]
        [Tooltip("閾ｪ蜍輔ユ繧ｹ繝医ｒ髢句ｧ九☆繧・)]
        [SerializeField] private bool autoStartTest = false;
        
        [Tooltip("隧ｳ邏ｰ縺ｪ邨ｱ險医ｒ陦ｨ遉ｺ縺吶ｋ")]
        [SerializeField] private bool showDetailedStats = true;
        
        [Header("Modern Pool System")]
        [Tooltip("CommandPoolService繧定・蜍輔〒菴懈・縺吶ｋ")]
        [SerializeField] private bool createPoolService = true;
        
        private IHealthTarget healthTarget;
        private bool testInProgress = false;
        private int commandsExecuted = 0;
        private float testStartTime;
        private CommandPoolService poolService;
        
        private void Start()
        {
            // 繝繝溘・HealthTarget繧剃ｽ懈・
            var dummyHealth = gameObject.AddComponent<DummyHealthTarget>();
            healthTarget = dummyHealth;
            
            // CommandPoolService縺ｮ蛻晄悄蛹・
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
                // DamageCommand縺ｨHealCommand繧剃ｺ､莠偵↓繝・せ繝・
                if (i % 2 == 0)
                {
                    TestDamageCommand(service);
                }
                else
                {
                    TestHealCommand(service);
                }
                
                commandsExecuted++;
                
                // 邨ｱ險医ｒ螳壽悄逧・↓陦ｨ遉ｺ
                if (showDetailedStats && commandsExecuted % 20 == 0)
                {
                    LogCurrentStats(service);
                }
                
                yield return new WaitForSeconds(commandInterval);
            }
            
            // 譛邨らｵｱ險医ｒ陦ｨ遉ｺ
            LogFinalStats(service);
            testInProgress = false;
        }
        
        private void TestDamageCommand(ICommandPoolService service)
        {
            // 譁ｰ縺励＞API菴ｿ逕ｨ: CommandPoolService縺九ｉ蜿門ｾ・
            var command = service.GetCommand<DamageCommand>();
            command.Initialize(healthTarget, 10, "physical");
            
            // 繧ｳ繝槭Φ繝牙ｮ溯｡・
            command.Execute();
            
            // 繝励・繝ｫ縺ｫ霑泌唆
            service.ReturnCommand(command);
        }
        
        private void TestHealCommand(ICommandPoolService service)
        {
            // 譁ｰ縺励＞API菴ｿ逕ｨ: CommandPoolService縺九ｉ蜿門ｾ・
            var command = service.GetCommand<HealCommand>();
            command.Initialize(healthTarget, 5);
            
            // 繧ｳ繝槭Φ繝牙ｮ溯｡・
            command.Execute();
            
            // 繝励・繝ｫ縺ｫ霑泌唆
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
            
            // 隧ｳ邏ｰ邨ｱ險医ｒCommandPoolManager縺九ｉ蜿門ｾ・
            service.LogDebugInfo();
            
            // 繝代ヵ繧ｩ繝ｼ繝槭Φ繧ｹ豈碑ｼ・Γ繝医Μ繧ｯ繧ｹ
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
    /// 繝・せ繝育畑縺ｮ繝繝溘・繝倥Ν繧ｹ繧ｿ繝ｼ繧ｲ繝・ヨ
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


