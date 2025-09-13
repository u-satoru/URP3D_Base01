namespace asterivo.Unity60.Core.Commands
{
    /// <summary>
    /// Interface for command definitions that can be serialized and used to create commands
    /// 
    /// NOTE: ドキュメント第4章:383行目では「マーカーインターフェース」として記載されていますが、
    /// 実際の実装ではファクトリメソッドパターンを採用し、より実用的な設計としています。
    /// 
    /// This interface enables the Hybrid Architecture (第3章) by:
    /// - SerializeReference属性によるポリモーフィックシリアライゼーション対応
    /// - ランタイムでのICommand生成をサポート
    /// - デザイナーフレンドリーなInspector編集とプログラマフレンドリーな実行時生成を両立
    /// </summary>
    public interface ICommandDefinition
    {
        /// <summary>
        /// Checks if the command can be executed in the given context
        /// </summary>
        bool CanExecute(object context = null);
        
        /// <summary>
        /// Creates a command instance from this definition
        /// Factory method pattern for generating runtime commands
        /// </summary>
        ICommand CreateCommand(object context = null);
    }
}