using UnityEngine;

namespace asterivo.Unity60.Core.Audio.Interfaces
{
    /// <summary>
    /// オーディオサービスの基本インターフェース
    /// </summary>
    public interface IAudioService
    {
        /// <summary>
        /// サウンドを再生
        /// </summary>
        void PlaySound(string soundId, Vector3 position = default, float volume = 1f);
        
        /// <summary>
        /// サウンドを停止
        /// </summary>
        void StopSound(string soundId);
        
        /// <summary>
        /// すべてのサウンドを停止
        /// </summary>
        void StopAllSounds();
        
        /// <summary>
        /// マスターボリュームを取得
        /// </summary>
        float GetMasterVolume();
        
        /// <summary>
        /// マスターボリュームを設定
        /// </summary>
        void SetMasterVolume(float volume);
        
        /// <summary>
        /// BGMボリュームを取得
        /// </summary>
        float GetBGMVolume();
        
        /// <summary>
        /// アンビエントボリュームを取得
        /// </summary>
        float GetAmbientVolume();
        
        /// <summary>
        /// エフェクトボリュームを取得
        /// </summary>
        float GetEffectVolume();
        
        /// <summary>
        /// カテゴリ別のボリュームを設定
        /// </summary>
        void SetCategoryVolume(string category, float volume);
        
        /// <summary>
        /// サウンドが再生中か確認
        /// </summary>
        bool IsPlaying(string soundId);
        
        /// <summary>
        /// 一時停止
        /// </summary>
        void Pause();
        
        /// <summary>
        /// 再開
        /// </summary>
        void Resume();
    }
}