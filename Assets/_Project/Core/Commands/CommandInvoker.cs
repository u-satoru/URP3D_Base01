using System.Collections.Generic;
using UnityEngine;
using asterivo.Unity60.Core.Events;
// using asterivo.Unity60.Core.Components;

using System.Linq;

namespace asterivo.Unity60.Core.Commands
{
    /// <summary>
    /// コマンドパターンの中核をなすクラスです、E    /// コマンド�E実行、およ�EUndo/Redoのためのコマンド履歴管琁E��拁E��します、E    /// </summary>
    public class CommandInvoker : MonoBehaviour, ICommandInvoker, IGameEventListener<object>
    {
        [Header("Command Events")]
        [Tooltip("実行すべきコマンドを受け取るためのイベンチE)]
        [SerializeField] private CommandGameEvent onCommandReceived;
        
        [Header("State Change Events")]
        [Tooltip("Undoの可否状態が変化した際に発行されるイベンチE)]
        [SerializeField] private BoolEventChannelSO onUndoStateChanged;
        [Tooltip("Redoの可否状態が変化した際に発行されるイベンチE)]
        [SerializeField] private BoolEventChannelSO onRedoStateChanged;
        
        [Header("Command History")]
        [Tooltip("保持するコマンド履歴の最大数")]
        [SerializeField] private int maxHistorySize = 100;
        [Tooltip("Undo機�Eを有効にするぁE)]
        [SerializeField] private bool enableUndo = true;
        [Tooltip("Redo機�Eを有効にするぁE)]
        [SerializeField] private bool enableRedo = true;

        [Header("Command Target")]
        [Tooltip("コマンド�E実行対象となるHealthコンポ�EネンチE)]
        [SerializeField] private Component playerHealthComponent;
        private IHealthTarget playerHealth;
        
        /// <summary>
        /// 実行されたコマンドをUndoするために保持するスタチE��、E        /// </summary>
        private Stack<ICommand> undoStack = new Stack<ICommand>();
        /// <summary>
        /// UndoされたコマンドをRedoするために保持するスタチE��、E        /// </summary>
        private Stack<ICommand> redoStack = new Stack<ICommand>();
        
        /// <summary>
        /// Undoが可能かどぁE��を示します、E        /// </summary>
        public bool CanUndo => enableUndo && undoStack.Count > 0;
        /// <summary>
        /// Redoが可能かどぁE��を示します、E        /// </summary>
        public bool CanRedo => enableRedo && redoStack.Count > 0;
        /// <summary>
        /// 現在UndoスタチE��に積まれてぁE��コマンド�E数を取得します、E        /// </summary>
        public int UndoStackCount => undoStack.Count;
        /// <summary>
        /// 現在RedoスタチE��に積まれてぁE��コマンド�E数を取得します、E        /// </summary>
        public int RedoStackCount => redoStack.Count;

        /// <summary>
        /// スクリプトが最初に有効になったときに呼び出されます、E        /// HealthターゲチE��を�E期化します、E        /// </summary>
        private void Start()
        {
            // ServiceLocatorにICommandInvokerとして登録
            ServiceLocator.RegisterService<ICommandInvoker>(this);

            // コンポ�Eネント参照からHealthターゲチE��を�E期化
            if (playerHealthComponent != null)
            {
                playerHealth = playerHealthComponent.GetComponent<IHealthTarget>();
                if (playerHealth == null)
                {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    UnityEngine.Debug.LogError("CommandInvoker: playerHealthComponentがIHealthTargetを実裁E��てぁE��せん、E);
#endif
                }
            }
        }
        
        /// <summary>
        /// オブジェクトが有効になったときに呼び出されます、E        /// コマンド受信イベント�Eリスナ�Eを登録します、E        /// </summary>
        private void OnEnable()
        {
            if (onCommandReceived != null)
            {
                onCommandReceived.RegisterListener(this);
            }
        }
        
        /// <summary>
        /// オブジェクトが無効になったときに呼び出されます、E        /// コマンド受信イベント�Eリスナ�Eを解除します、E        /// </summary>
        private void OnDisable()
        {
            if (onCommandReceived != null)
            {
                onCommandReceived.UnregisterListener(this);
            }
        }
        
        /// <summary>
        /// 持E��されたコマンドを実行し、Undo履歴に追加します、E        /// </summary>
        /// <param name="command">実行するコマンド、E/param>
        public void ExecuteCommand(ICommand command)
        {
            if (command == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.LogWarning("CommandInvoker: nullのコマンドを実行しようとしました、E);
#endif
                return;
            }
            
            command.Execute();
            
            // Undoが有効かつコマンドがUndoをサポ�EトしてぁE��場合、UndoスタチE��に追加
            if (enableUndo && command.CanUndo)
            {
                undoStack.Push(command);
                
                // 履歴サイズを制陁E                while (undoStack.Count > maxHistorySize)
                {
                    var tempStack = new Stack<ICommand>();
                    var items = undoStack.ToArray();
                    
                    // 最も古ぁE��イチE��を除夁E                    for (int i = 0; i < items.Length - 1; i++)
                    {
                        tempStack.Push(items[i]);
                    }
                    
                    undoStack.Clear();
                    while (tempStack.Count > 0)
                    {
                        undoStack.Push(tempStack.Pop());
                    }
                }
                
                // 新しいコマンドが実行されたらRedoスタチE��をクリア
                if (enableRedo)
                {
                    redoStack.Clear();
                }
                
                BroadcastHistoryChanges();
            }
        }
        
        /// <summary>
        /// 最後に行ったコマンドを允E��戻します！Endo�E�、E        /// </summary>
        /// <returns>Undoが�E功した場合�Etrue、E/returns>
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
        /// Undoしたコマンドを再度実行します！Eedo�E�、E        /// </summary>
        /// <returns>Redoが�E功した場合�Etrue、E/returns>
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
        /// すべてのコマンド履歴�E�Endo/Redo�E�を消去します、E        /// </summary>
        public void ClearHistory()
        {
            undoStack.Clear();
            redoStack.Clear();
            BroadcastHistoryChanges();
        }
        
        /// <summary>
        /// Undo/RedoスタチE��の状態変化をUIめE���EシスチE��に通知します、E        /// </summary>
        private void BroadcastHistoryChanges()
        {
            onUndoStateChanged?.Raise(CanUndo);
            onRedoStateChanged?.Raise(CanRedo);
        }
        
        /// <summary>
        /// ゲームイベント経由でコマンドを受け取った際のリスナ�E処琁E��す、E        /// </summary>
        /// <param name="value">受信したオブジェクト！ECommandにキャストされる�E�、E/param>
        public void OnEventRaised(object value)
        {
            if (value is ICommand command)
            {
                ExecuteCommand(command);
            }
            else
            {
                Debug.LogWarning($"[CommandInvoker] 受信したオブジェクトがICommandではありません: {value?.GetType().Name ?? "null"}");
            }
        }

        /// <summary>
        /// アイチE��が使用されたイベント�Eリスナ�Eです、E        /// アイチE��チE�Eタに含まれるコマンド定義からコマンドを生�Eし、実行します、E        /// </summary>
        /// <param name="itemData">使用されたアイチE��のチE�Eタ、E/param>
        public void OnItemUsed(ItemData itemData)
        {
            if (itemData == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.LogWarning("OnItemUsedがnullのItemDataで呼び出されました、E);
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
                    Debug.LogWarning($"[CommandInvoker] ItemDataのcommandDefinitionsに無効な型�Eオブジェクトが含まれてぁE��ぁE {definition?.GetType().Name ?? "null"}");
                }
            }
        }

        /// <summary>
        /// コマンド定義�E�ECommandDefinition�E�から�E体的なコマンド！ECommand�E�を生�EするファクトリメソチE��です、E        /// </summary>
        /// <param name="definition">コマンドを生�Eするための定義、E/param>
        /// <returns>生�Eされたコマンド。生成に失敗した場合�Enull、E/returns>
        private ICommand CreateCommandFromDefinition(ICommandDefinition definition)
        {
            if (playerHealth == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.LogError("CommandInvoker: コマンド実行対象�E�ElayerHealth�E�が設定されてぁE��せん、E);
#endif
                return null;
            }

            // 定義のファクトリメソチE��を直接使用
            var command = definition.CreateCommand(playerHealth);
            
            if (command == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.LogWarning($"定義タイプから�Eコマンド生成に失敗しました: {definition.GetType()}");
#endif
            }
            
            return command;
        }
    }
}
