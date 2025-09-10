namespace _Project.Core
{
    /// <summary>
    /// 初期化可能なシステムのインターフェース
    /// </summary>
    public interface IInitializable
    {
        /// <summary>
        /// 初期化優先度（低い値ほど先に初期化）
        /// </summary>
        int Priority { get; }
        
        /// <summary>
        /// 初期化処理
        /// </summary>
        void Initialize();
        
        /// <summary>
        /// 初期化が完了したかどうか
        /// </summary>
        bool IsInitialized { get; }
    }
}