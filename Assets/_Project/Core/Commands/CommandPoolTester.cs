using System.Collections;
using UnityEngine;
using asterivo.Unity60.Core.Components;

namespace asterivo.Unity60.Core.Commands
{
    /// <summary>
    /// CommandPoolの動作をテストするためのスクリプト
    /// Unity Editorでプール化の効果を確認できます。
    /// </summary>
    public class CommandPoolTester : MonoBehaviour
    {
        [Header("Test Settings")]
        [Tooltip("テストで作成するコマンドの数")]
        [SerializeField] private int testCommandCount = 100;
        
        [Tooltip("コマンド実行間隔（秒）")]
        [SerializeField] private float commandInterval = 0.1f;
        
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
            yield return new WaitForSeconds(1f);
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
            
            if (CommandPool.Instance == null)
            {
                UnityEngine.Debug.LogError("CommandPoolが見つかりません。シーンにCommandPoolを配置してください。");
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
            if (CommandPool.Instance == null)
            {
                UnityEngine.Debug.LogWarning("CommandPoolが見つかりません。");
                return;
            }
            
            var stats = CommandPool.Instance.GetPoolStats();
            UnityEngine.Debug.Log("=== CommandPool統計 ===");
            
            foreach (var kvp in stats)
            {
                UnityEngine.Debug.Log($"{kvp.Key.Name}: プール内オブジェクト数 = {kvp.Value}");
            }
            
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
                    ExecuteDamageCommand(10);
                }
                else
                {
                    ExecuteHealCommand(5);
                }
                
                commandsExecuted++;
                
                if (showDetailedStats && commandsExecuted % 10 == 0)
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
                elementType = "test"
            };
            
            var command = definition.CreateCommand(healthTarget);
            if (command != null)
            {
                command.Execute();
                
                // テストなのですぐにプールに返却（通常はCommandInvokerが管理）
                CommandPool.Instance?.ReturnCommand((DamageCommand)command);
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
                CommandPool.Instance?.ReturnCommand((HealCommand)command);
            }
        }
    }
    
    /// <summary>
    /// テスト用のダミーHealthTargetクラス
    /// </summary>
    public class DummyHealthTarget : MonoBehaviour, IHealthTarget
    {
        private int currentHealth = 100;
        private int maxHealth = 100;
        
        public int CurrentHealth => currentHealth;
        public int MaxHealth => maxHealth;
        
        public void TakeDamage(int damage)
        {
            currentHealth = Mathf.Max(0, currentHealth - damage);
        }
        
        public void Heal(int healAmount)
        {
            currentHealth = Mathf.Min(maxHealth, currentHealth + healAmount);
        }
    }
}