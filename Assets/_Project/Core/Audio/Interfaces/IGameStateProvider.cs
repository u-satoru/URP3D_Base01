namespace asterivo.Unity60.Core.Audio.Interfaces
{
    /// <summary>
    /// ゲーム状態提供者の最小インターフェース（Core層用）
    /// Features層からの実装注入を想定
    /// </summary>
    public interface IGameStateProvider
    {
        /// <summary>
        /// エンティティの警戒レベルを取得（0.0-1.0）
        /// </summary>
        float GetAlertLevel();
        
        /// <summary>
        /// プレイヤーが隠れ状態かどうか
        /// </summary>
        bool IsInHidingMode();
        
        /// <summary>
        /// オブジェクトが使用中かどうか
        /// </summary>
        bool IsBeingUsed();
    }
}
