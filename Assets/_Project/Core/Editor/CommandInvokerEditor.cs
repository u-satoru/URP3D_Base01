using UnityEngine;
using UnityEditor;
using System.Linq;
// using asterivo.Unity60.Core.Commands;
// using asterivo.Unity60.Core.Components;
using asterivo.Unity60.Core.Constants;

// using asterivo.Unity60.Core.Components;

namespace asterivo.Unity60.Core.Editor
{
    /// <summary>
    /// CommandInvoker用カスタムエチE��タ
    /// コマンドパターンの実行履歴とUndo/Redo機�EをInspectorで視覚化・制御
    /// 
    /// 主な機�E�E�E    /// - プレイモード中のUndo/RedoスタチE��状態表示
    /// - 手動でのUndo/Redo実衁E    /// - コマンド履歴の視覚化
    /// - チE��ト用コマンド�E実行！Eeal/Damage�E�E    /// - 設定検証とエラー表示
    /// 
    /// 使用シーン�E�E    /// - コマンド実行シスチE��のチE��チE��
    /// - ゲームプレイ中のコマンド履歴確誁E    /// - Undo/Redo機�EのチE��チE    /// - 設定ミスの早期発要E    /// </summary>
    [CustomEditor(typeof(CommandInvoker))]
    public class CommandInvokerEditor : UnityEditor.Editor
    {
        private CommandInvoker invoker;
        private bool showCommandDetails = false;
        
        /// <summary>
        /// エチE��ターが有効になった時の初期化�E琁E        /// 対象のCommandInvokerコンポ�Eネント�E参�Eを取征E        /// </summary>
        void OnEnable()
        {
            invoker = (CommandInvoker)target;
        }
        
        /// <summary>
        /// Inspector GUIの描画処琁E        /// チE��ォルト�EInspectorに加えて、カスタムチE��チE��機�Eを追加表示
        /// プレイモード中とエチE��チE��モード中で異なるUIを提侁E        /// </summary>
        /// <remarks>
        /// プレイモード中�E�コマンド履歴とUndo/Redo操作�Eインターフェース
        /// エチE��チE��モード中�E�設定検証とエラーチェチE��機�E
        /// </remarks>
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Command History Debug", EditorStyles.boldLabel);
            
            if (Application.isPlaying)
            {
                DrawPlayModeInterface();
            }
            else
            {
                DrawEditModeInterface();
            }
        }
        
        /// <summary>
        /// プレイモード中のインターフェース描画
        /// Undo/RedoスタチE��の状態表示、操作�Eタン、コマンド履歴の視覚化を含む
        /// </summary>
        /// <remarks>
        /// 以下�E要素で構�E�E�E        /// - Undo/RedoスタチE��のカウント表示
        /// - Undo/Redo/履歴クリアボタン�E�状態に応じて有効/無効化！E        /// - コマンド詳細表示の刁E��替ぁE        /// - チE��トコマンド�E実行�Eタン
        /// </remarks>
        private void DrawPlayModeInterface()
        {
            // Current state display
            EditorGUILayout.BeginHorizontal("box");
            EditorGUILayout.LabelField($"Undo Stack: {invoker.UndoStackCount}", GUILayout.Width(120));
            EditorGUILayout.LabelField($"Redo Stack: {invoker.RedoStackCount}", GUILayout.Width(120));
            EditorGUILayout.EndHorizontal();
            
            // Control buttons
            EditorGUILayout.BeginHorizontal();
            
            GUI.enabled = invoker.CanUndo;
            if (GUILayout.Button("⬁E��EUndo"))
            {
                invoker.Undo();
            }
            
            GUI.enabled = invoker.CanRedo;
            if (GUILayout.Button("➡�E�ERedo"))
            {
                invoker.Redo();
            }
            
            GUI.enabled = invoker.UndoStackCount > 0 || invoker.RedoStackCount > 0;
            if (GUILayout.Button("🗑�E�EClear History"))
            {
                invoker.ClearHistory();
            }
            
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
            
            // Command history visualization toggle
            showCommandDetails = EditorGUILayout.Toggle("Show Command Details", showCommandDetails);
            
            if (showCommandDetails)
            {
                DrawCommandHistory();
            }
            
            // Test command buttons
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Test Commands", EditorStyles.boldLabel);
            DrawTestCommands();
        }
        
        /// <summary>
        /// エチE��チE��モード中のインターフェース描画
        /// コンポ�Eネント�E設定検証とエラー表示を行う
        /// </summary>
        /// <remarks>
        /// 検証頁E���E�E        /// - onCommandReceived イベント�E設宁E        /// - onUndoStateChanged イベント�E設宁E        /// - onRedoStateChanged イベント�E設宁E        /// - playerHealthComponent の設定とIHealthTarget実裁E��ェチE��
        /// </remarks>
        private void DrawEditModeInterface()
        {
            EditorGUILayout.HelpBox("Command history is only available during Play mode", MessageType.Info);
            
            // Configuration validation
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Configuration Validation", EditorStyles.boldLabel);
            
            var onCommandReceived = serializedObject.FindProperty("onCommandReceived");
            var onUndoStateChanged = serializedObject.FindProperty("onUndoStateChanged");
            var onRedoStateChanged = serializedObject.FindProperty("onRedoStateChanged");
            var playerHealthComponent = serializedObject.FindProperty("playerHealthComponent");
            
            // Check for missing references
            if (onCommandReceived.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox("⚠�E�ENo Command Event assigned", MessageType.Warning);
            }
            
            if (onUndoStateChanged.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox("⚠�E�ENo Undo State Event assigned", MessageType.Warning);
            }
            
            if (onRedoStateChanged.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox("⚠�E�ENo Redo State Event assigned", MessageType.Warning);
            }
            
            if (playerHealthComponent.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox("⚠�E�ENo Player Health Component assigned", MessageType.Warning);
            }
            else
            {
                // Check if the component implements IHealthTarget
                var component = playerHealthComponent.objectReferenceValue as Component;
                if (component != null)
                {
                    var healthTarget = component.GetComponent<IHealthTarget>();
                    if (healthTarget == null)
                    {
                        EditorGUILayout.HelpBox("⚠�E�EAssigned component doesn't implement IHealthTarget", MessageType.Error);
                    }
                    else
                    {
                        EditorGUILayout.HelpBox("✁EConfiguration valid", MessageType.Info);
                    }
                }
            }
        }
        
        /// <summary>
        /// コマンド履歴スタチE��の視覚化表示
        /// Undo/RedoスタチE��の冁E��を階層構造で表示し、現在位置を示ぁE        /// </summary>
        /// <remarks>
        /// 表示形式！E        /// - RedoスタチE���E�上から頁E��表示�E�E        /// - 現在位置マ�Eカー�E�━━━ CURRENT ━━━E��E        /// - UndoスタチE���E�上から頁E��表示�E�E        /// 
        /// スタチE��が空の場合�E "No commands in history" を表示
        /// </remarks>
        private void DrawCommandHistory()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Command Stack Visualization:", EditorStyles.boldLabel);
            
            if (invoker.UndoStackCount == 0 && invoker.RedoStackCount == 0)
            {
                EditorGUILayout.LabelField("No commands in history", EditorStyles.centeredGreyMiniLabel);
                EditorGUILayout.EndVertical();
                return;
            }
            
            // This is a simplified visualization since we can't access the actual stacks
            // In a real implementation, you'd need to expose the stacks or their contents
            
            // Redo Stack visualization (top to bottom)
            if (invoker.RedoStackCount > 0)
            {
                EditorGUILayout.LabelField("Redo Stack:", EditorStyles.miniLabel);
                EditorGUI.indentLevel++;
                for (int i = 0; i < invoker.RedoStackCount; i++)
                {
                    EditorGUILayout.LabelField($"🔄 Command #{invoker.RedoStackCount - i}", EditorStyles.miniLabel);
                }
                EditorGUI.indentLevel--;
            }
            
            // Current position marker
            EditorGUILayout.LabelField("━━━ECURRENT ━━━E, EditorStyles.centeredGreyMiniLabel);
            
            // Undo Stack visualization (top to bottom)
            if (invoker.UndoStackCount > 0)
            {
                EditorGUILayout.LabelField("Undo Stack:", EditorStyles.miniLabel);
                EditorGUI.indentLevel++;
                for (int i = 0; i < invoker.UndoStackCount; i++)
                {
                    EditorGUILayout.LabelField($"↩�E�ECommand #{invoker.UndoStackCount - i}", EditorStyles.miniLabel);
                }
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.EndVertical();
        }
        
        /// <summary>
        /// チE��ト用コマンド�E実行インターフェース描画
        /// プレイモード中のみ利用可能で、Heal/DamageコマンドをチE��ト実行できる
        /// </summary>
        /// <remarks>
        /// チE��トコマンド！E        /// - Heal 10/25: 持E��量のヘルスを回復
        /// - Damage 10/25: 持E��量のダメージを与えめE        /// 
        /// IHealthTargetを実裁E��たコンポ�Eネントが忁E��E        /// </remarks>
        private void DrawTestCommands()
        {
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Test commands only available in Play mode", MessageType.Info);
                return;
            }
            
            EditorGUILayout.BeginVertical("box");
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button($"❤�E�ETest Heal ({GameConstants.TEST_HEAL_SMALL})"))
            {
                TestHealCommand(GameConstants.TEST_HEAL_SMALL);
            }
            
            if (GUILayout.Button($"💔 Test Damage ({GameConstants.TEST_DAMAGE_SMALL})"))
            {
                TestDamageCommand(GameConstants.TEST_DAMAGE_SMALL);
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button($"❤�E�ETest Heal ({GameConstants.TEST_HEAL_LARGE})"))
            {
                TestHealCommand(GameConstants.TEST_HEAL_LARGE);
            }
            
            if (GUILayout.Button($"💔 Test Damage ({GameConstants.TEST_DAMAGE_LARGE})"))
            {
                TestDamageCommand(GameConstants.TEST_DAMAGE_LARGE);
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }
        
        /// <summary>
        /// チE��ト用ヒ�Eルコマンド�E実衁E        /// 持E��した量のヘルス回復コマンドを作�E・実行し、Undo履歴に追加
        /// </summary>
        /// <param name="amount">回復するヘルス釁E/param>
        /// <remarks>
        /// IHealthTargetの検索頁E��！E        /// 1. CommandInvoker自身のコンポ�EネンチE        /// 2. playerHealthComponentに設定されたコンポ�EネンチE        /// 
        /// エラー処琁E��E        /// - IHealthTargetが見つからなぁE��合�E警告ログ出劁E        /// - 例外発生時はエラーログ出劁E        /// </remarks>
        private void TestHealCommand(int amount)
        {
            try
            {
                var healthComponent = invoker.GetComponent<IHealthTarget>();
                if (healthComponent == null)
                {
                    // Try to find it in children or get from the assigned component
                    var playerHealthComp = serializedObject.FindProperty("playerHealthComponent").objectReferenceValue as Component;
                    if (playerHealthComp != null)
                    {
                        healthComponent = playerHealthComp.GetComponent<IHealthTarget>();
                    }
                }
                
                if (healthComponent != null)
                {
                    var healCommand = new HealCommand(healthComponent, amount);
                    invoker.ExecuteCommand(healCommand);
                    UnityEngine.Debug.Log($"Test heal command executed: +{amount} HP");
                }
                else
                {
                    UnityEngine.Debug.LogWarning("No IHealthTarget found for test command");
                }
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"Failed to execute test heal command: {e.Message}");
            }
        }
        
        /// <summary>
        /// チE��ト用ダメージコマンド�E実衁E        /// 持E��した量のダメージコマンドを作�E・実行し、Undo履歴に追加
        /// </summary>
        /// <param name="amount">与えるダメージ釁E/param>
        /// <remarks>
        /// IHealthTargetの検索頁E��！E        /// 1. CommandInvoker自身のコンポ�EネンチE        /// 2. playerHealthComponentに設定されたコンポ�EネンチE        /// 
        /// エラー処琁E��E        /// - IHealthTargetが見つからなぁE��合�E警告ログ出劁E        /// - 例外発生時はエラーログ出劁E        /// </remarks>
        private void TestDamageCommand(int amount)
        {
            try
            {
                var healthComponent = invoker.GetComponent<IHealthTarget>();
                if (healthComponent == null)
                {
                    var playerHealthComp = serializedObject.FindProperty("playerHealthComponent").objectReferenceValue as Component;
                    if (playerHealthComp != null)
                    {
                        healthComponent = playerHealthComp.GetComponent<IHealthTarget>();
                    }
                }
                
                if (healthComponent != null)
                {
                    var damageCommand = new DamageCommand(healthComponent, amount);
                    invoker.ExecuteCommand(damageCommand);
                    UnityEngine.Debug.Log($"Test damage command executed: -{amount} HP");
                }
                else
                {
                    UnityEngine.Debug.LogWarning("No IHealthTarget found for test command");
                }
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"Failed to execute test damage command: {e.Message}");
            }
        }
    }
}