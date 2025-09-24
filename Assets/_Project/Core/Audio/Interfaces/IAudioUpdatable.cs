namespace asterivo.Unity60.Core.Audio.Interfaces
{
    /// <summary>
    /// オーディオシステムで更新可能なコンポーネント
    /// </summary>
    public interface IAudioUpdatable
    {
        /// <summary>
        /// 更新優先度（低い値ほど先に更新）
        /// </summary>
        int UpdatePriority { get; }
        
        /// <summary>
        /// オーディオ更新処理
        /// </summary>
        void UpdateAudio(float deltaTime);
        
        /// <summary>
        /// 更新が有効かどうか
        /// </summary>
        bool IsUpdateEnabled { get; }
    }
}
