namespace asterivo.Unity60.Features.Templates.Common
{
    /// <summary>
    /// テンプレート設定の共通インターフェース
    /// 全てのゲームジャンルテンプレートで共通して必要な基本機能を定義
    /// </summary>
    public interface ITemplateConfig
    {
        /// <summary>
        /// テンプレート名
        /// </summary>
        string TemplateName { get; }

        /// <summary>
        /// テンプレートバージョン
        /// </summary>
        string TemplateVersion { get; }

        /// <summary>
        /// テンプレートの説明
        /// </summary>
        string Description { get; }

        /// <summary>
        /// テンプレートが有効かどうか
        /// </summary>
        bool IsActive { get; }

        /// <summary>
        /// テンプレートを有効化
        /// </summary>
        void ActivateTemplate();

        /// <summary>
        /// テンプレートを無効化
        /// </summary>
        void DeactivateTemplate();

        /// <summary>
        /// 設定の妥当性を検証
        /// </summary>
        /// <returns>設定が正しければtrue</returns>
        bool ValidateConfiguration();
    }
}
