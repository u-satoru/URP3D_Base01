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
    /// CommandInvoker逕ｨ繧ｫ繧ｹ繧ｿ繝繧ｨ繝・ぅ繧ｿ
    /// 繧ｳ繝槭Φ繝峨ヱ繧ｿ繝ｼ繝ｳ縺ｮ螳溯｡悟ｱ･豁ｴ縺ｨUndo/Redo讖溯・繧棚nspector縺ｧ隕冶ｦ壼喧繝ｻ蛻ｶ蠕｡
    /// 
    /// 荳ｻ縺ｪ讖溯・・・    /// - 繝励Ξ繧､繝｢繝ｼ繝我ｸｭ縺ｮUndo/Redo繧ｹ繧ｿ繝・け迥ｶ諷玖｡ｨ遉ｺ
    /// - 謇句虚縺ｧ縺ｮUndo/Redo螳溯｡・    /// - 繧ｳ繝槭Φ繝牙ｱ･豁ｴ縺ｮ隕冶ｦ壼喧
    /// - 繝・せ繝育畑繧ｳ繝槭Φ繝峨・螳溯｡鯉ｼ・eal/Damage・・    /// - 險ｭ螳壽､懆ｨｼ縺ｨ繧ｨ繝ｩ繝ｼ陦ｨ遉ｺ
    /// 
    /// 菴ｿ逕ｨ繧ｷ繝ｼ繝ｳ・・    /// - 繧ｳ繝槭Φ繝牙ｮ溯｡後す繧ｹ繝・Β縺ｮ繝・ヰ繝・げ
    /// - 繧ｲ繝ｼ繝繝励Ξ繧､荳ｭ縺ｮ繧ｳ繝槭Φ繝牙ｱ･豁ｴ遒ｺ隱・    /// - Undo/Redo讖溯・縺ｮ繝・せ繝・    /// - 險ｭ螳壹Α繧ｹ縺ｮ譌ｩ譛溽匱隕・    /// </summary>
    [CustomEditor(typeof(CommandInvoker))]
    public class CommandInvokerEditor : UnityEditor.Editor
    {
        private CommandInvoker invoker;
        private bool showCommandDetails = false;
        
        /// <summary>
        /// 繧ｨ繝・ぅ繧ｿ繝ｼ縺梧怏蜉ｹ縺ｫ縺ｪ縺｣縺滓凾縺ｮ蛻晄悄蛹門・逅・        /// 蟇ｾ雎｡縺ｮCommandInvoker繧ｳ繝ｳ繝昴・繝阪Φ繝医・蜿ら・繧貞叙蠕・        /// </summary>
        void OnEnable()
        {
            invoker = (CommandInvoker)target;
        }
        
        /// <summary>
        /// Inspector GUI縺ｮ謠冗判蜃ｦ逅・        /// 繝・ヵ繧ｩ繝ｫ繝医・Inspector縺ｫ蜉縺医※縲√き繧ｹ繧ｿ繝繝・ヰ繝・げ讖溯・繧定ｿｽ蜉陦ｨ遉ｺ
        /// 繝励Ξ繧､繝｢繝ｼ繝我ｸｭ縺ｨ繧ｨ繝・ぅ繝・ヨ繝｢繝ｼ繝我ｸｭ縺ｧ逡ｰ縺ｪ繧偽I繧呈署萓・        /// </summary>
        /// <remarks>
        /// 繝励Ξ繧､繝｢繝ｼ繝我ｸｭ・壹さ繝槭Φ繝牙ｱ･豁ｴ縺ｨUndo/Redo謫堺ｽ懊・繧､繝ｳ繧ｿ繝ｼ繝輔ぉ繝ｼ繧ｹ
        /// 繧ｨ繝・ぅ繝・ヨ繝｢繝ｼ繝我ｸｭ・夊ｨｭ螳壽､懆ｨｼ縺ｨ繧ｨ繝ｩ繝ｼ繝√ぉ繝・け讖溯・
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
        /// 繝励Ξ繧､繝｢繝ｼ繝我ｸｭ縺ｮ繧､繝ｳ繧ｿ繝ｼ繝輔ぉ繝ｼ繧ｹ謠冗判
        /// Undo/Redo繧ｹ繧ｿ繝・け縺ｮ迥ｶ諷玖｡ｨ遉ｺ縲∵桃菴懊・繧ｿ繝ｳ縲√さ繝槭Φ繝牙ｱ･豁ｴ縺ｮ隕冶ｦ壼喧繧貞性繧
        /// </summary>
        /// <remarks>
        /// 莉･荳九・隕∫ｴ縺ｧ讒区・・・        /// - Undo/Redo繧ｹ繧ｿ繝・け縺ｮ繧ｫ繧ｦ繝ｳ繝郁｡ｨ遉ｺ
        /// - Undo/Redo/螻･豁ｴ繧ｯ繝ｪ繧｢繝懊ち繝ｳ・育憾諷九↓蠢懊§縺ｦ譛牙柑/辟｡蜉ｹ蛹厄ｼ・        /// - 繧ｳ繝槭Φ繝芽ｩｳ邏ｰ陦ｨ遉ｺ縺ｮ蛻・ｊ譖ｿ縺・        /// - 繝・せ繝医さ繝槭Φ繝峨・螳溯｡後・繧ｿ繝ｳ
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
            if (GUILayout.Button("筮・ｸ・Undo"))
            {
                invoker.Undo();
            }
            
            GUI.enabled = invoker.CanRedo;
            if (GUILayout.Button("筐｡・・Redo"))
            {
                invoker.Redo();
            }
            
            GUI.enabled = invoker.UndoStackCount > 0 || invoker.RedoStackCount > 0;
            if (GUILayout.Button("卵・・Clear History"))
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
        /// 繧ｨ繝・ぅ繝・ヨ繝｢繝ｼ繝我ｸｭ縺ｮ繧､繝ｳ繧ｿ繝ｼ繝輔ぉ繝ｼ繧ｹ謠冗判
        /// 繧ｳ繝ｳ繝昴・繝阪Φ繝医・險ｭ螳壽､懆ｨｼ縺ｨ繧ｨ繝ｩ繝ｼ陦ｨ遉ｺ繧定｡後≧
        /// </summary>
        /// <remarks>
        /// 讀懆ｨｼ鬆・岼・・        /// - onCommandReceived 繧､繝吶Φ繝医・險ｭ螳・        /// - onUndoStateChanged 繧､繝吶Φ繝医・險ｭ螳・        /// - onRedoStateChanged 繧､繝吶Φ繝医・險ｭ螳・        /// - playerHealthComponent 縺ｮ險ｭ螳壹→IHealthTarget螳溯｣・メ繧ｧ繝・け
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
                EditorGUILayout.HelpBox("笞・・No Command Event assigned", MessageType.Warning);
            }
            
            if (onUndoStateChanged.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox("笞・・No Undo State Event assigned", MessageType.Warning);
            }
            
            if (onRedoStateChanged.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox("笞・・No Redo State Event assigned", MessageType.Warning);
            }
            
            if (playerHealthComponent.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox("笞・・No Player Health Component assigned", MessageType.Warning);
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
                        EditorGUILayout.HelpBox("笞・・Assigned component doesn't implement IHealthTarget", MessageType.Error);
                    }
                    else
                    {
                        EditorGUILayout.HelpBox("笨・Configuration valid", MessageType.Info);
                    }
                }
            }
        }
        
        /// <summary>
        /// 繧ｳ繝槭Φ繝牙ｱ･豁ｴ繧ｹ繧ｿ繝・け縺ｮ隕冶ｦ壼喧陦ｨ遉ｺ
        /// Undo/Redo繧ｹ繧ｿ繝・け縺ｮ蜀・ｮｹ繧帝嚴螻､讒矩縺ｧ陦ｨ遉ｺ縺励∫樟蝨ｨ菴咲ｽｮ繧堤､ｺ縺・        /// </summary>
        /// <remarks>
        /// 陦ｨ遉ｺ蠖｢蠑擾ｼ・        /// - Redo繧ｹ繧ｿ繝・け・井ｸ翫°繧蛾・↓陦ｨ遉ｺ・・        /// - 迴ｾ蝨ｨ菴咲ｽｮ繝槭・繧ｫ繝ｼ・遺煤笏≫煤 CURRENT 笏≫煤笏・ｼ・        /// - Undo繧ｹ繧ｿ繝・け・井ｸ翫°繧蛾・↓陦ｨ遉ｺ・・        /// 
        /// 繧ｹ繧ｿ繝・け縺檎ｩｺ縺ｮ蝣ｴ蜷医・ "No commands in history" 繧定｡ｨ遉ｺ
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
                    EditorGUILayout.LabelField($"売 Command #{invoker.RedoStackCount - i}", EditorStyles.miniLabel);
                }
                EditorGUI.indentLevel--;
            }
            
            // Current position marker
            EditorGUILayout.LabelField("笏≫煤笏・CURRENT 笏≫煤笏・, EditorStyles.centeredGreyMiniLabel);
            
            // Undo Stack visualization (top to bottom)
            if (invoker.UndoStackCount > 0)
            {
                EditorGUILayout.LabelField("Undo Stack:", EditorStyles.miniLabel);
                EditorGUI.indentLevel++;
                for (int i = 0; i < invoker.UndoStackCount; i++)
                {
                    EditorGUILayout.LabelField($"竊ｩ・・Command #{invoker.UndoStackCount - i}", EditorStyles.miniLabel);
                }
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.EndVertical();
        }
        
        /// <summary>
        /// 繝・せ繝育畑繧ｳ繝槭Φ繝峨・螳溯｡後う繝ｳ繧ｿ繝ｼ繝輔ぉ繝ｼ繧ｹ謠冗判
        /// 繝励Ξ繧､繝｢繝ｼ繝我ｸｭ縺ｮ縺ｿ蛻ｩ逕ｨ蜿ｯ閭ｽ縺ｧ縲？eal/Damage繧ｳ繝槭Φ繝峨ｒ繝・せ繝亥ｮ溯｡後〒縺阪ｋ
        /// </summary>
        /// <remarks>
        /// 繝・せ繝医さ繝槭Φ繝会ｼ・        /// - Heal 10/25: 謖・ｮ夐㍼縺ｮ繝倥Ν繧ｹ繧貞屓蠕ｩ
        /// - Damage 10/25: 謖・ｮ夐㍼縺ｮ繝繝｡繝ｼ繧ｸ繧剃ｸ弱∴繧・        /// 
        /// IHealthTarget繧貞ｮ溯｣・＠縺溘さ繝ｳ繝昴・繝阪Φ繝医′蠢・ｦ・        /// </remarks>
        private void DrawTestCommands()
        {
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Test commands only available in Play mode", MessageType.Info);
                return;
            }
            
            EditorGUILayout.BeginVertical("box");
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button($"笶､・・Test Heal ({GameConstants.TEST_HEAL_SMALL})"))
            {
                TestHealCommand(GameConstants.TEST_HEAL_SMALL);
            }
            
            if (GUILayout.Button($"樗 Test Damage ({GameConstants.TEST_DAMAGE_SMALL})"))
            {
                TestDamageCommand(GameConstants.TEST_DAMAGE_SMALL);
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button($"笶､・・Test Heal ({GameConstants.TEST_HEAL_LARGE})"))
            {
                TestHealCommand(GameConstants.TEST_HEAL_LARGE);
            }
            
            if (GUILayout.Button($"樗 Test Damage ({GameConstants.TEST_DAMAGE_LARGE})"))
            {
                TestDamageCommand(GameConstants.TEST_DAMAGE_LARGE);
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }
        
        /// <summary>
        /// 繝・せ繝育畑繝偵・繝ｫ繧ｳ繝槭Φ繝峨・螳溯｡・        /// 謖・ｮ壹＠縺滄㍼縺ｮ繝倥Ν繧ｹ蝗槫ｾｩ繧ｳ繝槭Φ繝峨ｒ菴懈・繝ｻ螳溯｡後＠縲ゞndo螻･豁ｴ縺ｫ霑ｽ蜉
        /// </summary>
        /// <param name="amount">蝗槫ｾｩ縺吶ｋ繝倥Ν繧ｹ驥・/param>
        /// <remarks>
        /// IHealthTarget縺ｮ讀懃ｴ｢鬆・ｺ擾ｼ・        /// 1. CommandInvoker閾ｪ霄ｫ縺ｮ繧ｳ繝ｳ繝昴・繝阪Φ繝・        /// 2. playerHealthComponent縺ｫ險ｭ螳壹＆繧後◆繧ｳ繝ｳ繝昴・繝阪Φ繝・        /// 
        /// 繧ｨ繝ｩ繝ｼ蜃ｦ逅・ｼ・        /// - IHealthTarget縺瑚ｦ九▽縺九ｉ縺ｪ縺・ｴ蜷医・隴ｦ蜻翫Ο繧ｰ蜃ｺ蜉・        /// - 萓句､也匱逕滓凾縺ｯ繧ｨ繝ｩ繝ｼ繝ｭ繧ｰ蜃ｺ蜉・        /// </remarks>
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
        /// 繝・せ繝育畑繝繝｡繝ｼ繧ｸ繧ｳ繝槭Φ繝峨・螳溯｡・        /// 謖・ｮ壹＠縺滄㍼縺ｮ繝繝｡繝ｼ繧ｸ繧ｳ繝槭Φ繝峨ｒ菴懈・繝ｻ螳溯｡後＠縲ゞndo螻･豁ｴ縺ｫ霑ｽ蜉
        /// </summary>
        /// <param name="amount">荳弱∴繧九ム繝｡繝ｼ繧ｸ驥・/param>
        /// <remarks>
        /// IHealthTarget縺ｮ讀懃ｴ｢鬆・ｺ擾ｼ・        /// 1. CommandInvoker閾ｪ霄ｫ縺ｮ繧ｳ繝ｳ繝昴・繝阪Φ繝・        /// 2. playerHealthComponent縺ｫ險ｭ螳壹＆繧後◆繧ｳ繝ｳ繝昴・繝阪Φ繝・        /// 
        /// 繧ｨ繝ｩ繝ｼ蜃ｦ逅・ｼ・        /// - IHealthTarget縺瑚ｦ九▽縺九ｉ縺ｪ縺・ｴ蜷医・隴ｦ蜻翫Ο繧ｰ蜃ｺ蜉・        /// - 萓句､也匱逕滓凾縺ｯ繧ｨ繝ｩ繝ｼ繝ｭ繧ｰ蜃ｺ蜉・        /// </remarks>
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
