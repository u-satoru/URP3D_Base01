using UnityEngine;
using UnityEditor;
using System.Linq;
using asterivo.Unity60.Core.Commands;
using asterivo.Unity60.Core.Components;
using asterivo.Unity60.Core.Constants;

using asterivo.Unity60.Core.Components;

namespace asterivo.Unity60.Core.Editor
{
    /// <summary>
    /// CommandInvoker用カスタムエディタ
    /// コマンドパターンの実行履歴とUndo/Redo機能をInspectorで視覚化・制御
    /// 
    /// 主な機能：
    /// - プレイモード中のUndo/Redoスタック状態表示
    /// - 手動でのUndo/Redo実行
    /// - コマンド履歴の視覚化
    /// - テスト用コマンドの実行（Heal/Damage）
    /// - 設定検証とエラー表示
    /// 
    /// 使用シーン：
    /// - コマンド実行システムのデバッグ
    /// - ゲームプレイ中のコマンド履歴確認
    /// - Undo/Redo機能のテスト
    /// - 設定ミスの早期発見
    /// </summary>
    [CustomEditor(typeof(CommandInvoker))]
    public class CommandInvokerEditor : UnityEditor.Editor
    {
        private CommandInvoker invoker;
        private bool showCommandDetails = false;
        
        /// <summary>
        /// エディターが有効になった時の初期化処理
        /// 対象のCommandInvokerコンポーネントの参照を取得
        /// </summary>
        void OnEnable()
        {
            invoker = (CommandInvoker)target;
        }
        
        /// <summary>
        /// Inspector GUIの描画処理
        /// デフォルトのInspectorに加えて、カスタムデバッグ機能を追加表示
        /// プレイモード中とエディットモード中で異なるUIを提供
        /// </summary>
        /// <remarks>
        /// プレイモード中：コマンド履歴とUndo/Redo操作のインターフェース
        /// エディットモード中：設定検証とエラーチェック機能
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
        /// Undo/Redoスタックの状態表示、操作ボタン、コマンド履歴の視覚化を含む
        /// </summary>
        /// <remarks>
        /// 以下の要素で構成：
        /// - Undo/Redoスタックのカウント表示
        /// - Undo/Redo/履歴クリアボタン（状態に応じて有効/無効化）
        /// - コマンド詳細表示の切り替え
        /// - テストコマンドの実行ボタン
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
            if (GUILayout.Button("⬅️ Undo"))
            {
                invoker.Undo();
            }
            
            GUI.enabled = invoker.CanRedo;
            if (GUILayout.Button("➡️ Redo"))
            {
                invoker.Redo();
            }
            
            GUI.enabled = invoker.UndoStackCount > 0 || invoker.RedoStackCount > 0;
            if (GUILayout.Button("🗑️ Clear History"))
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
        /// エディットモード中のインターフェース描画
        /// コンポーネントの設定検証とエラー表示を行う
        /// </summary>
        /// <remarks>
        /// 検証項目：
        /// - onCommandReceived イベントの設定
        /// - onUndoStateChanged イベントの設定
        /// - onRedoStateChanged イベントの設定
        /// - playerHealthComponent の設定とIHealthTarget実装チェック
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
                EditorGUILayout.HelpBox("⚠️ No Command Event assigned", MessageType.Warning);
            }
            
            if (onUndoStateChanged.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox("⚠️ No Undo State Event assigned", MessageType.Warning);
            }
            
            if (onRedoStateChanged.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox("⚠️ No Redo State Event assigned", MessageType.Warning);
            }
            
            if (playerHealthComponent.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox("⚠️ No Player Health Component assigned", MessageType.Warning);
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
                        EditorGUILayout.HelpBox("⚠️ Assigned component doesn't implement IHealthTarget", MessageType.Error);
                    }
                    else
                    {
                        EditorGUILayout.HelpBox("✅ Configuration valid", MessageType.Info);
                    }
                }
            }
        }
        
        /// <summary>
        /// コマンド履歴スタックの視覚化表示
        /// Undo/Redoスタックの内容を階層構造で表示し、現在位置を示す
        /// </summary>
        /// <remarks>
        /// 表示形式：
        /// - Redoスタック（上から順に表示）
        /// - 現在位置マーカー（━━━ CURRENT ━━━）
        /// - Undoスタック（上から順に表示）
        /// 
        /// スタックが空の場合は "No commands in history" を表示
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
            EditorGUILayout.LabelField("━━━ CURRENT ━━━", EditorStyles.centeredGreyMiniLabel);
            
            // Undo Stack visualization (top to bottom)
            if (invoker.UndoStackCount > 0)
            {
                EditorGUILayout.LabelField("Undo Stack:", EditorStyles.miniLabel);
                EditorGUI.indentLevel++;
                for (int i = 0; i < invoker.UndoStackCount; i++)
                {
                    EditorGUILayout.LabelField($"↩️ Command #{invoker.UndoStackCount - i}", EditorStyles.miniLabel);
                }
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.EndVertical();
        }
        
        /// <summary>
        /// テスト用コマンドの実行インターフェース描画
        /// プレイモード中のみ利用可能で、Heal/Damageコマンドをテスト実行できる
        /// </summary>
        /// <remarks>
        /// テストコマンド：
        /// - Heal 10/25: 指定量のヘルスを回復
        /// - Damage 10/25: 指定量のダメージを与える
        /// 
        /// IHealthTargetを実装したコンポーネントが必要
        /// </remarks>
        private void DrawTestCommands()
        {
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Test commands only available in Play mode", MessageType.Info);
                return;
            }
            
            EditorGUILayout.BeginVertical("box");
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button($"❤️ Test Heal ({GameConstants.TEST_HEAL_SMALL})"))
            {
                TestHealCommand(GameConstants.TEST_HEAL_SMALL);
            }
            
            if (GUILayout.Button($"💔 Test Damage ({GameConstants.TEST_DAMAGE_SMALL})"))
            {
                TestDamageCommand(GameConstants.TEST_DAMAGE_SMALL);
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button($"❤️ Test Heal ({GameConstants.TEST_HEAL_LARGE})"))
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
        /// テスト用ヒールコマンドの実行
        /// 指定した量のヘルス回復コマンドを作成・実行し、Undo履歴に追加
        /// </summary>
        /// <param name="amount">回復するヘルス量</param>
        /// <remarks>
        /// IHealthTargetの検索順序：
        /// 1. CommandInvoker自身のコンポーネント
        /// 2. playerHealthComponentに設定されたコンポーネント
        /// 
        /// エラー処理：
        /// - IHealthTargetが見つからない場合は警告ログ出力
        /// - 例外発生時はエラーログ出力
        /// </remarks>
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
        /// テスト用ダメージコマンドの実行
        /// 指定した量のダメージコマンドを作成・実行し、Undo履歴に追加
        /// </summary>
        /// <param name="amount">与えるダメージ量</param>
        /// <remarks>
        /// IHealthTargetの検索順序：
        /// 1. CommandInvoker自身のコンポーネント
        /// 2. playerHealthComponentに設定されたコンポーネント
        /// 
        /// エラー処理：
        /// - IHealthTargetが見つからない場合は警告ログ出力
        /// - 例外発生時はエラーログ出力
        /// </remarks>
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