using System.Collections;
using UnityEngine;
using asterivo.Unity60.Core.Components;
using asterivo.Unity60.Core.Commands.Definitions;
using asterivo.Unity60.Core.Services;

namespace asterivo.Unity60.Core.Commands
{
    /// <summary>
    /// CommandPoolの動作をテストするためのスクリプト
    /// Unity Editorでプール化の効果を確認できます。
    /// </summary>
    public class CommandPoolTester : MonoBehaviour
    {
        // テスト設定の定数
        private const int DEFAULT_TEST_COMMAND_COUNT = 100;
        private const float DEFAULT_COMMAND_INTERVAL = 0.1f;
        private const int DEFAULT_DAMAGE_AMOUNT = 10;
        private const int DEFAULT_HEAL_AMOUNT = 5;
        private const int STATS_DISPLAY_INTERVAL = 10;
        private const float POOL_INITIALIZATION_DELAY = 1f;
        
        [Header("Test Settings")]
        [Tooltip("テストで作成するコマンドの数")]
        [SerializeField] private int testCommandCount = DEFAULT_TEST_COMMAND_COUNT;
        
        [Tooltip("コマンド実行間隔（秒）")]
        [SerializeField] private float commandInterval = DEFAULT_COMMAND_INTERVAL;
        
        [Header("Test Controls")]
        [Tooltip("自動テストを開始する")]
        [SerializeField] private bool autoStartTest = false;
        
        [Header("Debug Info")]
        [Tooltip("詳細な統計を表示する")]
        [SerializeField] private bool showDetailedStats = true;
        
        private IHealthTarget healthTarget;
        private bool testInProgress = false;
        private int commandsExecuted = 0;
        private float testStartTime;
        
        private void Start()
        {
            // ダミーHealthTargetを作成
            var dummyHealth = gameObject.AddComponent<DummyHealthTarget>();
            healthTarget = dummyHealth;
            
            if (autoStartTest)
            {
                StartCoroutine(DelayedTestStart());
            }
        }
        
        private IEnumerator DelayedTestStart()
        {
            // CommandPoolの初期化を待つ
            yield return new WaitForSeconds(POOL_INITIALIZATION_DELAY);
            StartPoolTest();
        }
        
        /// <summary>
        /// プール化テストを開始します。
        /// </summary>
        [ContextMenu("Start Pool Test")]
        public void StartPoolTest()
        {
            if (testInProgress)
            {
                UnityEngine.Debug.LogWarning("テストは既に実行中です。");
                return;
            }
            
            if (ServiceLocator.GetService<ICommandPoolService>() == null)
            {
                UnityEngine.Debug.LogError("CommandPoolServiceが見つかりません。シーンにCommandPoolServiceを配置してください。");
                return;
            }
            
            if (healthTarget == null)
            {
                UnityEngine.Debug.LogError("HealthTargetが設定されていません。");
                return;
            }
            
            UnityEngine.Debug.Log($"CommandPoolテストを開始します。コマンド数: {testCommandCount}, 間隔: {commandInterval}秒");
            testInProgress = true;
            commandsExecuted = 0;
            testStartTime = Time.time;
            
            StartCoroutine(ExecuteTestCommands());
        }
        
        /// <summary>
        /// プール統計を表示します。
        /// </summary>
        [ContextMenu("Show Pool Stats")]
        public void ShowPoolStats()
        {
            if (ServiceLocator.GetService<ICommandPoolService>() == null)
            {
                UnityEngine.Debug.LogWarning("CommandPoolServiceが見つかりません。");
                return;
            }
            
            var service = ServiceLocator.GetService<ICommandPoolService>();
            UnityEngine.Debug.Log("=== CommandPool統計 ===");
            service.LogDebugInfo();
            
            UnityEngine.Debug.Log($"実行したコマンド数: {commandsExecuted}");
            if (testStartTime > 0)
            {
                float elapsedTime = Time.time - testStartTime;
                UnityEngine.Debug.Log($"経過時間: {elapsedTime:F2}秒, 1秒あたりのコマンド数: {commandsExecuted / elapsedTime:F1}");
            }
        }
        
        private IEnumerator ExecuteTestCommands()
        {
            for (int i = 0; i < testCommandCount; i++)
            {
                // ダメージとヒールを交互に実行
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
                    UnityEngine.Debug.Log($"コマンド実行中... ({commandsExecuted}/{testCommandCount})");
                }
                
                yield return new WaitForSeconds(commandInterval);
            }
            
            testInProgress = false;
            float totalTime = Time.time - testStartTime;
            
            UnityEngine.Debug.Log($"=== テスト完了 ===");
            UnityEngine.Debug.Log($"実行したコマンド数: {commandsExecuted}");
            UnityEngine.Debug.Log($"総実行時間: {totalTime:F2}秒");
            UnityEngine.Debug.Log($"平均実行速度: {commandsExecuted / totalTime:F1} コマンド/秒");
            
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
                
                // テストなのですぐにプールに返却（通常はCommandInvokerが管理）
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
                
                // テストなのですぐにプールに返却（通常はCommandInvokerが管理）
                ServiceLocator.GetService<ICommandPoolService>()?.ReturnCommand((HealCommand)command);
            }
        }
    }
    
    /// <summary>
    /// テスト用のダミーHealthTargetクラス
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