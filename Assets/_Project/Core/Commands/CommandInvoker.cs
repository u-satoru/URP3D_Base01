using System.Collections.Generic;
using UnityEngine;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Components;

namespace asterivo.Unity60.Core.Commands
{
    /// <summary>
    /// コマンドパターンの中核をなすクラスです。
    /// コマンドの実行、およびUndo/Redoのためのコマンド履歴管理を担当します。
    /// </summary>
    public class CommandInvoker : MonoBehaviour, IGameEventListener<ICommand>
    {
        [Header("Command Events")]
        [Tooltip("実行すべきコマンドを受け取るためのイベント")]
        [SerializeField] private CommandGameEvent onCommandReceived;
        
        [Header("State Change Events")]
        [Tooltip("Undoの可否状態が変化した際に発行されるイベント")]
        [SerializeField] private BoolEventChannelSO onUndoStateChanged;
        [Tooltip("Redoの可否状態が変化した際に発行されるイベント")]
        [SerializeField] private BoolEventChannelSO onRedoStateChanged;
        
        [Header("Command History")]
        [Tooltip("保持するコマンド履歴の最大数")]
        [SerializeField] private int maxHistorySize = 100;
        [Tooltip("Undo機能を有効にするか")]
        [SerializeField] private bool enableUndo = true;
        [Tooltip("Redo機能を有効にするか")]
        [SerializeField] private bool enableRedo = true;

        [Header("Command Target")]
        [Tooltip("コマンドの実行対象となるHealthコンポーネント")]
        [SerializeField] private Component playerHealthComponent;
        private IHealthTarget playerHealth;
        
        private Stack<ICommand> undoStack = new Stack<ICommand>();
        private Stack<ICommand> redoStack = new Stack<ICommand>();
        
        /// <summary>
        /// Undoが可能かどうかを示します。
        /// </summary>
        public bool CanUndo => enableUndo && undoStack.Count > 0;
        /// <summary>
        /// Redoが可能かどうかを示します。
        /// </summary>
        public bool CanRedo => enableRedo && redoStack.Count > 0;
        /// <summary>
        /// 現在Undoスタックに積まれているコマンドの数を取得します。
        /// </summary>
        public int UndoStackCount => undoStack.Count;
        /// <summary>
        /// 現在Redoスタックに積まれているコマンドの数を取得します。
        /// </summary>
        public int RedoStackCount => redoStack.Count;

        private void Start()
        {
            // コンポーネント参照からHealthターゲットを初期化
            if (playerHealthComponent != null)
            {
                playerHealth = playerHealthComponent.GetComponent<IHealthTarget>();
                if (playerHealth == null)
                {
                    UnityEngine.Debug.LogError("CommandInvoker: playerHealthComponentがIHealthTargetを実装していません。");
                }
            }
        }
        
        private void OnEnable()
        {
            if (onCommandReceived != null)
            {
                onCommandReceived.RegisterListener(this);
            }
        }
        
        private void OnDisable()
        {
            if (onCommandReceived != null)
            {
                onCommandReceived.UnregisterListener(this);
            }
        }
        
        /// <summary>
        /// 指定されたコマンドを実行し、Undo履歴に追加します。
        /// </summary>
        /// <param name="command">実行するコマンド。</param>
        public void ExecuteCommand(ICommand command)
        {
            if (command == null)
            {
                UnityEngine.Debug.LogWarning("CommandInvoker: nullのコマンドを実行しようとしました。");
                return;
            }
            
            command.Execute();
            
            // Undoが有効かつコマンドがUndoをサポートしている場合、Undoスタックに追加
            if (enableUndo && command.CanUndo)
            {
                undoStack.Push(command);
                
                // 履歴サイズを制限
                while (undoStack.Count > maxHistorySize)
                {
                    var tempStack = new Stack<ICommand>();
                    var items = undoStack.ToArray();
                    
                    // 最も古いアイテムを除外
                    for (int i = 0; i < items.Length - 1; i++)
                    {
                        tempStack.Push(items[i]);
                    }
                    
                    undoStack.Clear();
                    while (tempStack.Count > 0)
                    {
                        undoStack.Push(tempStack.Pop());
                    }
                }
                
                // 新しいコマンドが実行されたらRedoスタックをクリア
                if (enableRedo)
                {
                    redoStack.Clear();
                }
                
                BroadcastHistoryChanges();
            }
        }
        
        /// <summary>
        /// 最後に行ったコマンドを元に戻します（Undo）。
        /// </summary>
        /// <returns>Undoが成功した場合はtrue。</returns>
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
        /// Undoしたコマンドを再度実行します（Redo）。
        /// </summary>
        /// <returns>Redoが成功した場合はtrue。</returns>
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
        /// すべてのコマンド履歴（Undo/Redo）を消去します。
        /// </summary>
        public void ClearHistory()
        {
            undoStack.Clear();
            redoStack.Clear();
            BroadcastHistoryChanges();
        }
        
        /// <summary>
        /// Undo/Redoスタックの状態変化をUIや他のシステムに通知します。
        /// </summary>
        private void BroadcastHistoryChanges()
        {
            onUndoStateChanged?.Raise(CanUndo);
            onRedoStateChanged?.Raise(CanRedo);
        }
        
        /// <summary>
        /// ゲームイベント経由でコマンドを受け取った際のリスナー処理です。
        /// </summary>
        /// <param name="value">受信したコマンド。</param>
        public void OnEventRaised(ICommand value)
        {
            ExecuteCommand(value);
        }

        /// <summary>
        /// アイテムが使用されたイベントのリスナーです。
        /// アイテムデータに含まれるコマンド定義からコマンドを生成し、実行します。
        /// </summary>
        /// <param name="itemData">使用されたアイテムのデータ。</param>
        public void OnItemUsed(ItemData itemData)
        {
            if (itemData == null)
            {
                UnityEngine.Debug.LogWarning("OnItemUsedがnullのItemDataで呼び出されました。");
                return;
            }

            foreach (var definition in itemData.commandDefinitions)
            {
                ICommand command = CreateCommandFromDefinition(definition);
                if (command != null)
                {
                    ExecuteCommand(command);
                }
            }
        }

        /// <summary>
        /// コマンド定義（ICommandDefinition）から具体的なコマンド（ICommand）を生成するファクトリメソッドです。
        /// </summary>
        /// <param name="definition">コマンドを生成するための定義。</param>
        /// <returns>生成されたコマンド。生成に失敗した場合はnull。</returns>
        private ICommand CreateCommandFromDefinition(ICommandDefinition definition)
        {
            if (playerHealth == null)
            {
                UnityEngine.Debug.LogError("CommandInvoker: コマンド実行対象（playerHealth）が設定されていません。");
                return null;
            }

            // 定義のファクトリメソッドを直接使用
            var command = definition.CreateCommand(playerHealth);
            
            if (command == null)
            {
                UnityEngine.Debug.LogWarning($"定義タイプからのコマンド生成に失敗しました: {definition.GetType()}");
            }
            
            return command;
        }
    }
}
