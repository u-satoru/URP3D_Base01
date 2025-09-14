using asterivo.Unity60.Core.Commands;
using asterivo.Unity60.Features.Templates.Platformer.Collectibles;

namespace asterivo.Unity60.Features.Templates.Platformer.Commands
{
    /// <summary>
    /// アイテム収集コマンド
    /// Commandパターンによる収集処理のカプセル化
    /// Undo/Redo対応でアイテム収集の取り消し・やり直しが可能
    /// </summary>
    public class CollectItemCommand : ICommand, IResettableCommand
    {
        #region Command Data

        private readonly Collectible targetCollectible;
        private readonly CollectibleManager manager;
        private readonly int scoreBeforeCollection;
        private readonly int comboBeforeCollection;
        private readonly float comboTimerBeforeCollection;
        private readonly bool wasCollectedBefore;

        #endregion

        #region Constructor

        /// <summary>
        /// アイテム収集コマンドのコンストラクタ
        /// </summary>
        /// <param name="collectible">収集対象のアイテム</param>
        /// <param name="collectibleManager">収集管理システム</param>
        public CollectItemCommand(Collectible collectible, CollectibleManager collectibleManager)
        {
            targetCollectible = collectible;
            manager = collectibleManager;

            // 現在の状態を記録（Undo用）
            if (manager != null)
            {
                scoreBeforeCollection = manager.GetCurrentScore();
                // Note: 実際の実装では、managerからcombo情報も取得する必要があります
                // この例では簡略化しています
            }

            wasCollectedBefore = collectible?.IsCollected ?? false;
        }

        #endregion

        #region ICommand Implementation

        /// <summary>
        /// コマンドの実行
        /// </summary>
        /// <returns>実行成功の可否</returns>
        public bool Execute()
        {
            if (targetCollectible == null || manager == null)
            {
                return false;
            }

            if (targetCollectible.IsCollected)
            {
                return false; // 既に収集済み
            }

            // 実際の収集処理はCollectibleManager側で既に実行済み
            // このコマンドは記録とUndo/Redo機能のために存在
            return true;
        }

        /// <summary>
        /// コマンドの取り消し
        /// </summary>
        /// <returns>取り消し成功の可否</returns>
        public bool Undo()
        {
            if (targetCollectible == null || manager == null)
            {
                return false;
            }

            // アイテムの状態を復元
            targetCollectible.IsCollected = wasCollectedBefore;
            targetCollectible.gameObject.SetActive(true);

            // スコアの復元
            // Note: 実際の実装では、managerにスコア復元メソッドが必要
            // manager.RestoreScore(scoreBeforeCollection);

            UnityEngine.Debug.Log($"[CollectItemCommand] Undone collection of {targetCollectible.Config?.displayName}");
            return true;
        }

        /// <summary>
        /// 取り消し可能かどうか
        /// </summary>
        /// <returns>取り消し可能な場合はtrue</returns>
        public bool CanUndo()
        {
            return targetCollectible != null &&
                   manager != null &&
                   targetCollectible.IsCollected;
        }

        #endregion

        #region IResettableCommand Implementation

        /// <summary>
        /// コマンドの状態をリセット
        /// ObjectPoolでの再利用時に呼び出される
        /// </summary>
        public void Reset()
        {
            // プライベートフィールドのリセットは不要
            // （readonly fieldのため）
            // 必要に応じて可変データのリセット処理をここに記述
        }

        #endregion

        #region Command Information

        /// <summary>
        /// コマンドの説明を取得
        /// </summary>
        /// <returns>コマンドの説明文字列</returns>
        public string GetDescription()
        {
            string itemName = targetCollectible?.Config?.displayName ?? "Unknown Item";
            return $"Collect {itemName}";
        }

        /// <summary>
        /// 収集対象のアイテムを取得
        /// </summary>
        /// <returns>収集対象のCollectible</returns>
        public Collectible GetTargetCollectible()
        {
            return targetCollectible;
        }

        /// <summary>
        /// 収集前のスコアを取得
        /// </summary>
        /// <returns>収集前のスコア</returns>
        public int GetScoreBeforeCollection()
        {
            return scoreBeforeCollection;
        }

        #endregion

        #region Debug Support

        /// <summary>
        /// デバッグ用の文字列表現
        /// </summary>
        /// <returns>デバッグ用文字列</returns>
        public override string ToString()
        {
            string itemName = targetCollectible?.Config?.displayName ?? "Unknown";
            string itemType = targetCollectible?.Config?.type.ToString() ?? "Unknown";
            return $"CollectItemCommand[{itemName} ({itemType})]";
        }

        #endregion
    }
}