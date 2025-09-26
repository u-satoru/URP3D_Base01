using System.Collections;
using UnityEngine;
using asterivo.Unity60.Core.Components;
using asterivo.Unity60.Core.Commands.Definitions;
using asterivo.Unity60.Core;

namespace asterivo.Unity60.Core.Commands
{
    /// <summary>
    /// CommandPool縺ｮ蜍穂ｽ懊ｒ繝・せ繝医☆繧九◆繧√・繧ｹ繧ｯ繝ｪ繝励ヨ
    /// Unity Editor縺ｧ繝励・繝ｫ蛹悶・蜉ｹ譫懊ｒ遒ｺ隱阪〒縺阪∪縺吶・
    /// </summary>
    public class CommandPoolTester : MonoBehaviour
    {
        // 繝・せ繝郁ｨｭ螳壹・螳壽焚
        private const int DEFAULT_TEST_COMMAND_COUNT = 100;
        private const float DEFAULT_COMMAND_INTERVAL = 0.1f;
        private const int DEFAULT_DAMAGE_AMOUNT = 10;
        private const int DEFAULT_HEAL_AMOUNT = 5;
        private const int STATS_DISPLAY_INTERVAL = 10;
        private const float POOL_INITIALIZATION_DELAY = 1f;
        
        [Header("Test Settings")]
        [Tooltip("繝・せ繝医〒菴懈・縺吶ｋ繧ｳ繝槭Φ繝峨・謨ｰ")]
        [SerializeField] private int testCommandCount = DEFAULT_TEST_COMMAND_COUNT;
        
        [Tooltip("繧ｳ繝槭Φ繝牙ｮ溯｡碁俣髫費ｼ育ｧ抵ｼ・)]
        [SerializeField] private float commandInterval = DEFAULT_COMMAND_INTERVAL;
        
        [Header("Test Controls")]
        [Tooltip("閾ｪ蜍輔ユ繧ｹ繝医ｒ髢句ｧ九☆繧・)]
        [SerializeField] private bool autoStartTest = false;
        
        [Header("Debug Info")]
        [Tooltip("隧ｳ邏ｰ縺ｪ邨ｱ險医ｒ陦ｨ遉ｺ縺吶ｋ")]
        [SerializeField] private bool showDetailedStats = true;
        
        private IHealthTarget healthTarget;
        private bool testInProgress = false;
        private int commandsExecuted = 0;
        private float testStartTime;
        
        private void Start()
        {
            // 繝繝溘・HealthTarget繧剃ｽ懈・
            var dummyHealth = gameObject.AddComponent<DummyHealthTarget>();
            healthTarget = dummyHealth;
            
            if (autoStartTest)
            {
                StartCoroutine(DelayedTestStart());
            }
        }
        
        private IEnumerator DelayedTestStart()
        {
            // CommandPool縺ｮ蛻晄悄蛹悶ｒ蠕・▽
            yield return new WaitForSeconds(POOL_INITIALIZATION_DELAY);
            StartPoolTest();
        }
        
        /// <summary>
        /// 繝励・繝ｫ蛹悶ユ繧ｹ繝医ｒ髢句ｧ九＠縺ｾ縺吶・
        /// </summary>
        [ContextMenu("Start Pool Test")]
        public void StartPoolTest()
        {
            if (testInProgress)
            {
                UnityEngine.Debug.LogWarning("繝・せ繝医・譌｢縺ｫ螳溯｡御ｸｭ縺ｧ縺吶・);
                return;
            }
            
            if (ServiceLocator.GetService<ICommandPoolService>() == null)
            {
                UnityEngine.Debug.LogError("CommandPoolService縺瑚ｦ九▽縺九ｊ縺ｾ縺帙ｓ縲ゅす繝ｼ繝ｳ縺ｫCommandPoolService繧帝・鄂ｮ縺励※縺上□縺輔＞縲・);
                return;
            }
            
            if (healthTarget == null)
            {
                UnityEngine.Debug.LogError("HealthTarget縺瑚ｨｭ螳壹＆繧後※縺・∪縺帙ｓ縲・);
                return;
            }
            
            UnityEngine.Debug.Log($"CommandPool繝・せ繝医ｒ髢句ｧ九＠縺ｾ縺吶ゅさ繝槭Φ繝画焚: {testCommandCount}, 髢馴囈: {commandInterval}遘・);
            testInProgress = true;
            commandsExecuted = 0;
            testStartTime = Time.time;
            
            StartCoroutine(ExecuteTestCommands());
        }
        
        /// <summary>
        /// 繝励・繝ｫ邨ｱ險医ｒ陦ｨ遉ｺ縺励∪縺吶・
        /// </summary>
        [ContextMenu("Show Pool Stats")]
        public void ShowPoolStats()
        {
            if (ServiceLocator.GetService<ICommandPoolService>() == null)
            {
                UnityEngine.Debug.LogWarning("CommandPoolService縺瑚ｦ九▽縺九ｊ縺ｾ縺帙ｓ縲・);
                return;
            }
            
            var service = ServiceLocator.GetService<ICommandPoolService>();
            UnityEngine.Debug.Log("=== CommandPool邨ｱ險・===");
            service.LogDebugInfo();
            
            UnityEngine.Debug.Log($"螳溯｡後＠縺溘さ繝槭Φ繝画焚: {commandsExecuted}");
            if (testStartTime > 0)
            {
                float elapsedTime = Time.time - testStartTime;
                UnityEngine.Debug.Log($"邨碁℃譎る俣: {elapsedTime:F2}遘・ 1遘偵≠縺溘ｊ縺ｮ繧ｳ繝槭Φ繝画焚: {commandsExecuted / elapsedTime:F1}");
            }
        }
        
        private IEnumerator ExecuteTestCommands()
        {
            for (int i = 0; i < testCommandCount; i++)
            {
                // 繝繝｡繝ｼ繧ｸ縺ｨ繝偵・繝ｫ繧剃ｺ､莠偵↓螳溯｡・
                if (i % 2 == 0)
                {
                    ExecuteDamageCommand(DEFAULT_DAMAGE_AMOUNT);
                }
                else
                {
                    ExecuteHealCommand(DEFAULT_HEAL_AMOUNT);
                }
                
                commandsExecuted++;
                
                if (showDetailedStats && commandsExecuted % STATS_DISPLAY_INTERVAL == 0)
                {
                    UnityEngine.Debug.Log($"繧ｳ繝槭Φ繝牙ｮ溯｡御ｸｭ... ({commandsExecuted}/{testCommandCount})");
                }
                
                yield return new WaitForSeconds(commandInterval);
            }
            
            testInProgress = false;
            float totalTime = Time.time - testStartTime;
            
            UnityEngine.Debug.Log($"=== 繝・せ繝亥ｮ御ｺ・===");
            UnityEngine.Debug.Log($"螳溯｡後＠縺溘さ繝槭Φ繝画焚: {commandsExecuted}");
            UnityEngine.Debug.Log($"邱丞ｮ溯｡梧凾髢・ {totalTime:F2}遘・);
            UnityEngine.Debug.Log($"蟷ｳ蝮・ｮ溯｡碁溷ｺｦ: {commandsExecuted / totalTime:F1} 繧ｳ繝槭Φ繝・遘・);
            
            ShowPoolStats();
        }
        
        private void ExecuteDamageCommand(int damage)
        {
            var definition = new DamageCommandDefinition
            {
                damageAmount = damage,
                damageType = DamageCommandDefinition.DamageType.Physical
            };
            
            var command = definition.CreateCommand(healthTarget);
            if (command != null)
            {
                command.Execute();
                
                // 繝・せ繝医↑縺ｮ縺ｧ縺吶＄縺ｫ繝励・繝ｫ縺ｫ霑泌唆・磯壼ｸｸ縺ｯCommandInvoker縺檎ｮ｡逅・ｼ・
                ServiceLocator.GetService<ICommandPoolService>()?.ReturnCommand((DamageCommand)command);
            }
        }
        
        private void ExecuteHealCommand(int heal)
        {
            var definition = new HealCommandDefinition
            {
                healAmount = heal
            };
            
            var command = definition.CreateCommand(healthTarget);
            if (command != null)
            {
                command.Execute();
                
                // 繝・せ繝医↑縺ｮ縺ｧ縺吶＄縺ｫ繝励・繝ｫ縺ｫ霑泌唆・磯壼ｸｸ縺ｯCommandInvoker縺檎ｮ｡逅・ｼ・
                ServiceLocator.GetService<ICommandPoolService>()?.ReturnCommand((HealCommand)command);
            }
        }
    }
    
    /// <summary>
    /// 繝・せ繝育畑縺ｮ繝繝溘・HealthTarget繧ｯ繝ｩ繧ｹ
    /// </summary>
    public class DummyHealthTarget : MonoBehaviour, IHealthTarget
    {
        private const int DEFAULT_MAX_HEALTH = 100;
        
        private int currentHealth = DEFAULT_MAX_HEALTH;
        private int maxHealth = DEFAULT_MAX_HEALTH;
        
        public int CurrentHealth => currentHealth;
        public int MaxHealth => maxHealth;
        
        public void TakeDamage(int damage)
        {
            TakeDamage(damage, "physical");
        }
        
        public void TakeDamage(int damage, string elementType)
        {
            currentHealth = Mathf.Max(0, currentHealth - damage);
            UnityEngine.Debug.Log($"[DummyTarget] Took {damage} {elementType} damage. Health: {currentHealth}/{maxHealth}");
        }
        
        public void Heal(int healAmount)
        {
            currentHealth = Mathf.Min(maxHealth, currentHealth + healAmount);
        }
    }
}


