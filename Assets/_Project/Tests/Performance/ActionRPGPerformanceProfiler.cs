using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using asterivo.Unity60.Features.Templates.ActionRPG;
using asterivo.Unity60.Features.Templates.ActionRPG.Character;
using asterivo.Unity60.Features.Templates.ActionRPG.Equipment;
using asterivo.Unity60.Features.Templates.ActionRPG.Combat;
using asterivo.Unity60.Core.Services;

namespace asterivo.Unity60.Tests.Performance
{
    /// <summary>
    /// Action RPGテンプレートのパフォーマンス分析・最適化ツール
    /// 各システムの処理時間、メモリ使用量、最適化ポイントを測定
    /// </summary>
    public class ActionRPGPerformanceProfiler : MonoBehaviour
    {
        [Header("Performance Settings")]
        [SerializeField] private bool enableProfiling = true;
        [SerializeField] private float profilingInterval = 1.0f;
        [SerializeField] private int measurementSamples = 100;

        [Header("Test Parameters")]
        [SerializeField] private int testItemCount = 1000;
        [SerializeField] private int testStatusEffectCount = 50;
        [SerializeField] private int testEquipmentChanges = 100;

        // Performance metrics
        private PerformanceMetrics characterProgressionMetrics;
        private PerformanceMetrics inventoryMetrics;
        private PerformanceMetrics equipmentMetrics;
        private PerformanceMetrics combatMetrics;
        private PerformanceMetrics statusEffectMetrics;

        // System references
        private CharacterProgressionManager characterProgression;
        private InventoryManager inventoryManager;
        private EquipmentManager equipmentManager;
        private CombatManager combatManager;
        private StatusEffectManager statusEffectManager;

        private void Start()
        {
            if (enableProfiling)
            {
                InitializeProfiler();
                StartCoroutine(RunPerformanceTests());
            }
        }

        private void InitializeProfiler()
        {
            // Find system components
            characterProgression = FindFirstObjectByType<CharacterProgressionManager>();
            inventoryManager = ServiceLocator.Instance?.GetService<InventoryManager>();
            equipmentManager = ServiceLocator.Instance?.GetService<EquipmentManager>();
            combatManager = FindFirstObjectByType<CombatManager>();
            statusEffectManager = FindFirstObjectByType<StatusEffectManager>();

            // Initialize metrics
            characterProgressionMetrics = new PerformanceMetrics("Character Progression");
            inventoryMetrics = new PerformanceMetrics("Inventory System");
            equipmentMetrics = new PerformanceMetrics("Equipment System");
            combatMetrics = new PerformanceMetrics("Combat System");
            statusEffectMetrics = new PerformanceMetrics("Status Effect System");

            LogDebug("[ActionRPG Profiler] Performance profiler initialized");
        }

        private IEnumerator RunPerformanceTests()
        {
            LogDebug("[ActionRPG Profiler] Starting performance tests...");

            // Wait for systems to initialize
            yield return new WaitForSeconds(2.0f);

            // Run individual system tests
            yield return StartCoroutine(ProfileCharacterProgression());
            yield return StartCoroutine(ProfileInventoryOperations());
            yield return StartCoroutine(ProfileEquipmentOperations());
            yield return StartCoroutine(ProfileCombatOperations());
            yield return StartCoroutine(ProfileStatusEffectOperations());

            // Generate final report
            GeneratePerformanceReport();
        }

        #region Character Progression Profiling

        private IEnumerator ProfileCharacterProgression()
        {
            if (characterProgression == null)
            {
                LogWarning("[ActionRPG Profiler] Character Progression Manager not found");
                yield break;
            }

            LogDebug("[ActionRPG Profiler] Profiling Character Progression...");

            // Test experience gain performance
            var stopwatch = new Stopwatch();
            for (int i = 0; i < measurementSamples; i++)
            {
                stopwatch.Start();
                characterProgression.GainExperience(100);
                stopwatch.Stop();

                characterProgressionMetrics.AddSample("Experience Gain", stopwatch.Elapsed.TotalMilliseconds);
                stopwatch.Reset();

                if (i % 10 == 0) yield return null;
            }

            // Test level up performance
            var currentLevel = characterProgression.CurrentLevel;
            for (int i = 0; i < 10; i++)
            {
                stopwatch.Start();
                characterProgression.GainExperience(characterProgression.ExperienceToNextLevel);
                stopwatch.Stop();

                characterProgressionMetrics.AddSample("Level Up", stopwatch.Elapsed.TotalMilliseconds);
                stopwatch.Reset();
                yield return null;
            }

            characterProgressionMetrics.CalculateAverages();
            LogDebug($"[ActionRPG Profiler] Character Progression profiling completed");
        }

        #endregion

        #region Inventory Profiling

        private IEnumerator ProfileInventoryOperations()
        {
            if (inventoryManager == null)
            {
                LogWarning("[ActionRPG Profiler] Inventory Manager not found");
                yield break;
            }

            LogDebug("[ActionRPG Profiler] Profiling Inventory Operations...");

            var testItem = CreateTestItem();
            var stopwatch = new Stopwatch();

            // Test add item performance
            for (int i = 0; i < measurementSamples; i++)
            {
                stopwatch.Start();
                inventoryManager.AddItem(testItem, 1);
                stopwatch.Stop();

                inventoryMetrics.AddSample("Add Item", stopwatch.Elapsed.TotalMilliseconds);
                stopwatch.Reset();

                if (i % 10 == 0) yield return null;
            }

            // Test large batch operations
            stopwatch.Start();
            for (int i = 0; i < testItemCount; i++)
            {
                inventoryManager.AddItem(testItem, 10);
            }
            stopwatch.Stop();
            inventoryMetrics.AddSample("Large Batch Add", stopwatch.Elapsed.TotalMilliseconds);

            // Test item removal performance
            for (int i = 0; i < measurementSamples && inventoryManager.HasItem(testItem); i++)
            {
                stopwatch.Start();
                inventoryManager.RemoveItem(testItem, 1);
                stopwatch.Stop();

                inventoryMetrics.AddSample("Remove Item", stopwatch.Elapsed.TotalMilliseconds);
                stopwatch.Reset();

                if (i % 10 == 0) yield return null;
            }

            // Test inventory sorting
            stopwatch.Start();
            inventoryManager.SortInventory();
            stopwatch.Stop();
            inventoryMetrics.AddSample("Sort Inventory", stopwatch.Elapsed.TotalMilliseconds);

            inventoryMetrics.CalculateAverages();
            LogDebug($"[ActionRPG Profiler] Inventory profiling completed");
        }

        #endregion

        #region Equipment Profiling

        private IEnumerator ProfileEquipmentOperations()
        {
            if (equipmentManager == null)
            {
                LogWarning("[ActionRPG Profiler] Equipment Manager not found");
                yield break;
            }

            LogDebug("[ActionRPG Profiler] Profiling Equipment Operations...");

            var testWeapon = CreateTestWeapon();
            var testArmor = CreateTestArmor();
            var stopwatch = new Stopwatch();

            // Test equipment changes
            for (int i = 0; i < testEquipmentChanges; i++)
            {
                // Equip weapon
                stopwatch.Start();
                equipmentManager.EquipItem(testWeapon);
                stopwatch.Stop();
                equipmentMetrics.AddSample("Equip Item", stopwatch.Elapsed.TotalMilliseconds);
                stopwatch.Reset();

                // Calculate stats
                stopwatch.Start();
                equipmentManager.CalculateEquipmentStats();
                stopwatch.Stop();
                equipmentMetrics.AddSample("Calculate Stats", stopwatch.Elapsed.TotalMilliseconds);
                stopwatch.Reset();

                // Unequip weapon
                stopwatch.Start();
                equipmentManager.UnequipItem(testWeapon.equipmentSlot);
                stopwatch.Stop();
                equipmentMetrics.AddSample("Unequip Item", stopwatch.Elapsed.TotalMilliseconds);
                stopwatch.Reset();

                if (i % 10 == 0) yield return null;
            }

            equipmentMetrics.CalculateAverages();
            LogDebug($"[ActionRPG Profiler] Equipment profiling completed");
        }

        #endregion

        #region Combat Profiling

        private IEnumerator ProfileCombatOperations()
        {
            if (combatManager == null)
            {
                LogWarning("[ActionRPG Profiler] Combat Manager not found");
                yield break;
            }

            LogDebug("[ActionRPG Profiler] Profiling Combat Operations...");

            var health = GetComponent<Health>();
            if (health == null)
            {
                health = gameObject.AddComponent<Health>();
            }

            var stopwatch = new Stopwatch();

            // Test damage calculations
            for (int i = 0; i < measurementSamples; i++)
            {
                stopwatch.Start();
                health.TakeDamage(10f, gameObject);
                stopwatch.Stop();

                combatMetrics.AddSample("Damage Calculation", stopwatch.Elapsed.TotalMilliseconds);
                stopwatch.Reset();

                // Heal back to full for next test
                health.Heal(10f);

                if (i % 10 == 0) yield return null;
            }

            combatMetrics.CalculateAverages();
            LogDebug($"[ActionRPG Profiler] Combat profiling completed");
        }

        #endregion

        #region Status Effect Profiling

        private IEnumerator ProfileStatusEffectOperations()
        {
            if (statusEffectManager == null)
            {
                LogWarning("[ActionRPG Profiler] Status Effect Manager not found");
                yield break;
            }

            LogDebug("[ActionRPG Profiler] Profiling Status Effect Operations...");

            var testEffect = CreateTestStatusEffect();
            var stopwatch = new Stopwatch();

            // Test status effect application
            for (int i = 0; i < testStatusEffectCount; i++)
            {
                testEffect.effectName = $"Test Effect {i}"; // Unique name for each test

                stopwatch.Start();
                statusEffectManager.ApplyStatusEffect(testEffect);
                stopwatch.Stop();

                statusEffectMetrics.AddSample("Apply Status Effect", stopwatch.Elapsed.TotalMilliseconds);
                stopwatch.Reset();

                if (i % 10 == 0) yield return null;
            }

            // Test status effect updates (simulate multiple effects active)
            for (int i = 0; i < measurementSamples; i++)
            {
                stopwatch.Start();
                // StatusEffectManager updates automatically, we measure the time it takes
                // This is a simulation - in reality, we'd need access to the update method
                yield return null; // Allow one frame for updates
                stopwatch.Stop();

                statusEffectMetrics.AddSample("Update Status Effects", stopwatch.Elapsed.TotalMilliseconds);
                stopwatch.Reset();
            }

            statusEffectMetrics.CalculateAverages();
            LogDebug($"[ActionRPG Profiler] Status Effect profiling completed");
        }

        #endregion

        #region Report Generation

        private void GeneratePerformanceReport()
        {
            LogDebug("=== ACTION RPG TEMPLATE PERFORMANCE REPORT ===");

            PrintSystemMetrics("CHARACTER PROGRESSION", characterProgressionMetrics);
            PrintSystemMetrics("INVENTORY SYSTEM", inventoryMetrics);
            PrintSystemMetrics("EQUIPMENT SYSTEM", equipmentMetrics);
            PrintSystemMetrics("COMBAT SYSTEM", combatMetrics);
            PrintSystemMetrics("STATUS EFFECT SYSTEM", statusEffectMetrics);

            // Memory usage analysis
            var memoryUsage = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemory(0);
            var memoryMB = memoryUsage / (1024 * 1024);
            LogDebug($"MEMORY USAGE: {memoryMB:F2} MB");

            // Performance recommendations
            GenerateOptimizationRecommendations();

            LogDebug("=== END PERFORMANCE REPORT ===");
        }

        private void PrintSystemMetrics(string systemName, PerformanceMetrics metrics)
        {
            LogDebug($"\n--- {systemName} ---");
            foreach (var operation in metrics.GetOperations())
            {
                var avgTime = metrics.GetAverageTime(operation);
                var samples = metrics.GetSampleCount(operation);
                LogDebug($"{operation}: {avgTime:F3}ms avg ({samples} samples)");
            }
        }

        private void GenerateOptimizationRecommendations()
        {
            LogDebug("\n--- OPTIMIZATION RECOMMENDATIONS ---");

            // Inventory optimization
            if (inventoryMetrics.GetAverageTime("Large Batch Add") > 50.0)
            {
                LogDebug("• Consider batch operations optimization for inventory");
            }

            // Equipment optimization
            if (equipmentMetrics.GetAverageTime("Calculate Stats") > 5.0)
            {
                LogDebug("• Consider caching equipment stats calculations");
            }

            // Status effect optimization
            if (statusEffectMetrics.GetAverageTime("Update Status Effects") > 2.0)
            {
                LogDebug("• Consider optimizing status effect update frequency");
            }

            // Memory optimization
            var memoryUsage = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemory(0);
            if (memoryUsage > 100 * 1024 * 1024) // 100MB
            {
                LogDebug("• Consider object pooling for frequently created objects");
                LogDebug("• Review ScriptableObject instantiation patterns");
            }
        }

        #endregion

        #region Helper Methods

        private ItemData CreateTestItem()
        {
            var item = ScriptableObject.CreateInstance<ItemData>();
            item.itemName = "Performance Test Item";
            item.itemType = ItemType.Consumable;
            item.maxStackSize = 99;
            return item;
        }

        private ItemData CreateTestWeapon()
        {
            var weapon = ScriptableObject.CreateInstance<ItemData>();
            weapon.itemName = "Performance Test Weapon";
            weapon.itemType = ItemType.Weapon;
            weapon.equipmentSlot = EquipmentSlot.MainHand;
            weapon.maxStackSize = 1;
            weapon.stats = new EquipmentStats { attackPower = 10 };
            return weapon;
        }

        private ItemData CreateTestArmor()
        {
            var armor = ScriptableObject.CreateInstance<ItemData>();
            armor.itemName = "Performance Test Armor";
            armor.itemType = ItemType.Armor;
            armor.equipmentSlot = EquipmentSlot.Chest;
            armor.maxStackSize = 1;
            armor.stats = new EquipmentStats { defense = 5 };
            return armor;
        }

        private StatusEffectData CreateTestStatusEffect()
        {
            var effect = ScriptableObject.CreateInstance<StatusEffectData>();
            effect.effectName = "Performance Test Effect";
            effect.effectType = StatusEffectType.StatModifier;
            effect.magnitude = 5f;
            effect.duration = 1f;
            return effect;
        }

        private void LogDebug(string message)
        {
            Debug.Log($"[ActionRPG Profiler] {message}");
        }

        private void LogWarning(string message)
        {
            Debug.LogWarning($"[ActionRPG Profiler] {message}");
        }

        #endregion
    }

    /// <summary>
    /// パフォーマンスメトリクス管理クラス
    /// </summary>
    public class PerformanceMetrics
    {
        private string systemName;
        private Dictionary<string, List<double>> measurements;
        private Dictionary<string, double> averages;

        public PerformanceMetrics(string name)
        {
            systemName = name;
            measurements = new Dictionary<string, List<double>>();
            averages = new Dictionary<string, double>();
        }

        public void AddSample(string operationName, double timeMs)
        {
            if (!measurements.ContainsKey(operationName))
            {
                measurements[operationName] = new List<double>();
            }
            measurements[operationName].Add(timeMs);
        }

        public void CalculateAverages()
        {
            foreach (var kvp in measurements)
            {
                var operation = kvp.Key;
                var samples = kvp.Value;

                if (samples.Count > 0)
                {
                    averages[operation] = samples.Sum() / samples.Count;
                }
            }
        }

        public double GetAverageTime(string operationName)
        {
            return averages.ContainsKey(operationName) ? averages[operationName] : 0.0;
        }

        public int GetSampleCount(string operationName)
        {
            return measurements.ContainsKey(operationName) ? measurements[operationName].Count : 0;
        }

        public IEnumerable<string> GetOperations()
        {
            return averages.Keys;
        }
    }
}