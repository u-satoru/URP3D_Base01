using System.Collections.Generic;
using UnityEngine;
using asterivo.Unity60.Core.Events;
// using asterivo.Unity60.Core.Components;

using System.Linq;

namespace asterivo.Unity60.Core.Commands
{
    /// <summary>
    /// 繧ｳ繝槭Φ繝峨ヱ繧ｿ繝ｼ繝ｳ縺ｮ荳ｭ譬ｸ繧偵↑縺吶け繝ｩ繧ｹ縺ｧ縺吶・    /// 繧ｳ繝槭Φ繝峨・螳溯｡後√♀繧医・Undo/Redo縺ｮ縺溘ａ縺ｮ繧ｳ繝槭Φ繝牙ｱ･豁ｴ邂｡逅・ｒ諡・ｽ薙＠縺ｾ縺吶・    /// </summary>
    public class CommandInvoker : MonoBehaviour, ICommandInvoker, IGameEventListener<object>
    {
        [Header("Command Events")]
        [Tooltip("螳溯｡後☆縺ｹ縺阪さ繝槭Φ繝峨ｒ蜿励￠蜿悶ｋ縺溘ａ縺ｮ繧､繝吶Φ繝・)]
        [SerializeField] private CommandGameEvent onCommandReceived;
        
        [Header("State Change Events")]
        [Tooltip("Undo縺ｮ蜿ｯ蜷ｦ迥ｶ諷九′螟牙喧縺励◆髫帙↓逋ｺ陦後＆繧後ｋ繧､繝吶Φ繝・)]
        [SerializeField] private BoolEventChannelSO onUndoStateChanged;
        [Tooltip("Redo縺ｮ蜿ｯ蜷ｦ迥ｶ諷九′螟牙喧縺励◆髫帙↓逋ｺ陦後＆繧後ｋ繧､繝吶Φ繝・)]
        [SerializeField] private BoolEventChannelSO onRedoStateChanged;
        
        [Header("Command History")]
        [Tooltip("菫晄戟縺吶ｋ繧ｳ繝槭Φ繝牙ｱ･豁ｴ縺ｮ譛螟ｧ謨ｰ")]
        [SerializeField] private int maxHistorySize = 100;
        [Tooltip("Undo讖溯・繧呈怏蜉ｹ縺ｫ縺吶ｋ縺・)]
        [SerializeField] private bool enableUndo = true;
        [Tooltip("Redo讖溯・繧呈怏蜉ｹ縺ｫ縺吶ｋ縺・)]
        [SerializeField] private bool enableRedo = true;

        [Header("Command Target")]
        [Tooltip("繧ｳ繝槭Φ繝峨・螳溯｡悟ｯｾ雎｡縺ｨ縺ｪ繧稀ealth繧ｳ繝ｳ繝昴・繝阪Φ繝・)]
        [SerializeField] private Component playerHealthComponent;
        private IHealthTarget playerHealth;
        
        /// <summary>
        /// 螳溯｡後＆繧後◆繧ｳ繝槭Φ繝峨ｒUndo縺吶ｋ縺溘ａ縺ｫ菫晄戟縺吶ｋ繧ｹ繧ｿ繝・け縲・        /// </summary>
        private Stack<ICommand> undoStack = new Stack<ICommand>();
        /// <summary>
        /// Undo縺輔ｌ縺溘さ繝槭Φ繝峨ｒRedo縺吶ｋ縺溘ａ縺ｫ菫晄戟縺吶ｋ繧ｹ繧ｿ繝・け縲・        /// </summary>
        private Stack<ICommand> redoStack = new Stack<ICommand>();
        
        /// <summary>
        /// Undo縺悟庄閭ｽ縺九←縺・°繧堤､ｺ縺励∪縺吶・        /// </summary>
        public bool CanUndo => enableUndo && undoStack.Count > 0;
        /// <summary>
        /// Redo縺悟庄閭ｽ縺九←縺・°繧堤､ｺ縺励∪縺吶・        /// </summary>
        public bool CanRedo => enableRedo && redoStack.Count > 0;
        /// <summary>
        /// 迴ｾ蝨ｨUndo繧ｹ繧ｿ繝・け縺ｫ遨阪∪繧後※縺・ｋ繧ｳ繝槭Φ繝峨・謨ｰ繧貞叙蠕励＠縺ｾ縺吶・        /// </summary>
        public int UndoStackCount => undoStack.Count;
        /// <summary>
        /// 迴ｾ蝨ｨRedo繧ｹ繧ｿ繝・け縺ｫ遨阪∪繧後※縺・ｋ繧ｳ繝槭Φ繝峨・謨ｰ繧貞叙蠕励＠縺ｾ縺吶・        /// </summary>
        public int RedoStackCount => redoStack.Count;

        /// <summary>
        /// 繧ｹ繧ｯ繝ｪ繝励ヨ縺梧怙蛻昴↓譛牙柑縺ｫ縺ｪ縺｣縺溘→縺阪↓蜻ｼ縺ｳ蜃ｺ縺輔ｌ縺ｾ縺吶・        /// Health繧ｿ繝ｼ繧ｲ繝・ヨ繧貞・譛溷喧縺励∪縺吶・        /// </summary>
        private void Start()
        {
            // ServiceLocator縺ｫICommandInvoker縺ｨ縺励※逋ｻ骭ｲ
            ServiceLocator.RegisterService<ICommandInvoker>(this);

            // 繧ｳ繝ｳ繝昴・繝阪Φ繝亥盾辣ｧ縺九ｉHealth繧ｿ繝ｼ繧ｲ繝・ヨ繧貞・譛溷喧
            if (playerHealthComponent != null)
            {
                playerHealth = playerHealthComponent.GetComponent<IHealthTarget>();
                if (playerHealth == null)
                {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    UnityEngine.Debug.LogError("CommandInvoker: playerHealthComponent縺栗HealthTarget繧貞ｮ溯｣・＠縺ｦ縺・∪縺帙ｓ縲・);
#endif
                }
            }
        }
        
        /// <summary>
        /// 繧ｪ繝悶ず繧ｧ繧ｯ繝医′譛牙柑縺ｫ縺ｪ縺｣縺溘→縺阪↓蜻ｼ縺ｳ蜃ｺ縺輔ｌ縺ｾ縺吶・        /// 繧ｳ繝槭Φ繝牙女菫｡繧､繝吶Φ繝医・繝ｪ繧ｹ繝翫・繧堤匳骭ｲ縺励∪縺吶・        /// </summary>
        private void OnEnable()
        {
            if (onCommandReceived != null)
            {
                onCommandReceived.RegisterListener(this);
            }
        }
        
        /// <summary>
        /// 繧ｪ繝悶ず繧ｧ繧ｯ繝医′辟｡蜉ｹ縺ｫ縺ｪ縺｣縺溘→縺阪↓蜻ｼ縺ｳ蜃ｺ縺輔ｌ縺ｾ縺吶・        /// 繧ｳ繝槭Φ繝牙女菫｡繧､繝吶Φ繝医・繝ｪ繧ｹ繝翫・繧定ｧ｣髯､縺励∪縺吶・        /// </summary>
        private void OnDisable()
        {
            if (onCommandReceived != null)
            {
                onCommandReceived.UnregisterListener(this);
            }
        }
        
        /// <summary>
        /// 謖・ｮ壹＆繧後◆繧ｳ繝槭Φ繝峨ｒ螳溯｡後＠縲ゞndo螻･豁ｴ縺ｫ霑ｽ蜉縺励∪縺吶・        /// </summary>
        /// <param name="command">螳溯｡後☆繧九さ繝槭Φ繝峨・/param>
        public void ExecuteCommand(ICommand command)
        {
            if (command == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.LogWarning("CommandInvoker: null縺ｮ繧ｳ繝槭Φ繝峨ｒ螳溯｡後＠繧医≧縺ｨ縺励∪縺励◆縲・);
#endif
                return;
            }
            
            command.Execute();
            
            // Undo縺梧怏蜉ｹ縺九▽繧ｳ繝槭Φ繝峨′Undo繧偵し繝昴・繝医＠縺ｦ縺・ｋ蝣ｴ蜷医ゞndo繧ｹ繧ｿ繝・け縺ｫ霑ｽ蜉
            if (enableUndo && command.CanUndo)
            {
                undoStack.Push(command);
                
                // 螻･豁ｴ繧ｵ繧､繧ｺ繧貞宛髯・                while (undoStack.Count > maxHistorySize)
                {
                    var tempStack = new Stack<ICommand>();
                    var items = undoStack.ToArray();
                    
                    // 譛繧ょ商縺・い繧､繝・Β繧帝勁螟・                    for (int i = 0; i < items.Length - 1; i++)
                    {
                        tempStack.Push(items[i]);
                    }
                    
                    undoStack.Clear();
                    while (tempStack.Count > 0)
                    {
                        undoStack.Push(tempStack.Pop());
                    }
                }
                
                // 譁ｰ縺励＞繧ｳ繝槭Φ繝峨′螳溯｡後＆繧後◆繧嘘edo繧ｹ繧ｿ繝・け繧偵け繝ｪ繧｢
                if (enableRedo)
                {
                    redoStack.Clear();
                }
                
                BroadcastHistoryChanges();
            }
        }
        
        /// <summary>
        /// 譛蠕後↓陦後▲縺溘さ繝槭Φ繝峨ｒ蜈・↓謌ｻ縺励∪縺呻ｼ・ndo・峨・        /// </summary>
        /// <returns>Undo縺梧・蜉溘＠縺溷ｴ蜷医・true縲・/returns>
        public bool Undo()
        {
            if (!CanUndo) return false;
                
            var command = undoStack.Pop();
            command.Undo();

            if (command.CanUndo && enableRedo)
            {
                redoStack.Push(command);
            }
            
            BroadcastHistoryChanges();
            return true;
        }
        
        /// <summary>
        /// Undo縺励◆繧ｳ繝槭Φ繝峨ｒ蜀榊ｺｦ螳溯｡後＠縺ｾ縺呻ｼ・edo・峨・        /// </summary>
        /// <returns>Redo縺梧・蜉溘＠縺溷ｴ蜷医・true縲・/returns>
        public bool Redo()
        {
            if (!CanRedo) return false;
                
            var command = redoStack.Pop();
            command.Execute();
            
            if (enableUndo)
            {
                undoStack.Push(command);
            }
            
            BroadcastHistoryChanges();
            return true;
        }
        
        /// <summary>
        /// 縺吶∋縺ｦ縺ｮ繧ｳ繝槭Φ繝牙ｱ･豁ｴ・・ndo/Redo・峨ｒ豸亥悉縺励∪縺吶・        /// </summary>
        public void ClearHistory()
        {
            undoStack.Clear();
            redoStack.Clear();
            BroadcastHistoryChanges();
        }
        
        /// <summary>
        /// Undo/Redo繧ｹ繧ｿ繝・け縺ｮ迥ｶ諷句､牙喧繧旦I繧・ｻ悶・繧ｷ繧ｹ繝・Β縺ｫ騾夂衍縺励∪縺吶・        /// </summary>
        private void BroadcastHistoryChanges()
        {
            onUndoStateChanged?.Raise(CanUndo);
            onRedoStateChanged?.Raise(CanRedo);
        }
        
        /// <summary>
        /// 繧ｲ繝ｼ繝繧､繝吶Φ繝育ｵ檎罰縺ｧ繧ｳ繝槭Φ繝峨ｒ蜿励￠蜿悶▲縺滄圀縺ｮ繝ｪ繧ｹ繝翫・蜃ｦ逅・〒縺吶・        /// </summary>
        /// <param name="value">蜿嶺ｿ｡縺励◆繧ｪ繝悶ず繧ｧ繧ｯ繝茨ｼ・Command縺ｫ繧ｭ繝｣繧ｹ繝医＆繧後ｋ・峨・/param>
        public void OnEventRaised(object value)
        {
            if (value is ICommand command)
            {
                ExecuteCommand(command);
            }
            else
            {
                Debug.LogWarning($"[CommandInvoker] 蜿嶺ｿ｡縺励◆繧ｪ繝悶ず繧ｧ繧ｯ繝医′ICommand縺ｧ縺ｯ縺ゅｊ縺ｾ縺帙ｓ: {value?.GetType().Name ?? "null"}");
            }
        }

        /// <summary>
        /// 繧｢繧､繝・Β縺御ｽｿ逕ｨ縺輔ｌ縺溘う繝吶Φ繝医・繝ｪ繧ｹ繝翫・縺ｧ縺吶・        /// 繧｢繧､繝・Β繝・・繧ｿ縺ｫ蜷ｫ縺ｾ繧後ｋ繧ｳ繝槭Φ繝牙ｮ夂ｾｩ縺九ｉ繧ｳ繝槭Φ繝峨ｒ逕滓・縺励∝ｮ溯｡後＠縺ｾ縺吶・        /// </summary>
        /// <param name="itemData">菴ｿ逕ｨ縺輔ｌ縺溘い繧､繝・Β縺ｮ繝・・繧ｿ縲・/param>
        public void OnItemUsed(ItemData itemData)
        {
            if (itemData == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.LogWarning("OnItemUsed縺系ull縺ｮItemData縺ｧ蜻ｼ縺ｳ蜃ｺ縺輔ｌ縺ｾ縺励◆縲・);
#endif
                return;
            }

            foreach (var definition in itemData.commandDefinitions)
            {
                if (definition is ICommandDefinition commandDefinition)
                {
                    ICommand command = CreateCommandFromDefinition(commandDefinition);
                    if (command != null)
                    {
                        ExecuteCommand(command);
                    }
                }
                else
                {
                    Debug.LogWarning($"[CommandInvoker] ItemData縺ｮcommandDefinitions縺ｫ辟｡蜉ｹ縺ｪ蝙九・繧ｪ繝悶ず繧ｧ繧ｯ繝医′蜷ｫ縺ｾ繧後※縺・∪縺・ {definition?.GetType().Name ?? "null"}");
                }
            }
        }

        /// <summary>
        /// 繧ｳ繝槭Φ繝牙ｮ夂ｾｩ・・CommandDefinition・峨°繧牙・菴鍋噪縺ｪ繧ｳ繝槭Φ繝会ｼ・Command・峨ｒ逕滓・縺吶ｋ繝輔ぃ繧ｯ繝医Μ繝｡繧ｽ繝・ラ縺ｧ縺吶・        /// </summary>
        /// <param name="definition">繧ｳ繝槭Φ繝峨ｒ逕滓・縺吶ｋ縺溘ａ縺ｮ螳夂ｾｩ縲・/param>
        /// <returns>逕滓・縺輔ｌ縺溘さ繝槭Φ繝峨ら函謌舌↓螟ｱ謨励＠縺溷ｴ蜷医・null縲・/returns>
        private ICommand CreateCommandFromDefinition(ICommandDefinition definition)
        {
            if (playerHealth == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.LogError("CommandInvoker: 繧ｳ繝槭Φ繝牙ｮ溯｡悟ｯｾ雎｡・・layerHealth・峨′險ｭ螳壹＆繧後※縺・∪縺帙ｓ縲・);
#endif
                return null;
            }

            // 螳夂ｾｩ縺ｮ繝輔ぃ繧ｯ繝医Μ繝｡繧ｽ繝・ラ繧堤峩謗･菴ｿ逕ｨ
            var command = definition.CreateCommand(playerHealth);
            
            if (command == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.LogWarning($"螳夂ｾｩ繧ｿ繧､繝励°繧峨・繧ｳ繝槭Φ繝臥函謌舌↓螟ｱ謨励＠縺ｾ縺励◆: {definition.GetType()}");
#endif
            }
            
            return command;
        }
    }
}

