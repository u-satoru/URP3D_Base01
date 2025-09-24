using UnityEngine;

namespace asterivo.Unity60.Core.Audio.Interfaces
{
    /// <summary>
    /// 効果音サービスのインターフェース
    /// </summary>
    public interface IEffectService
    {
        /// <summary>
        /// 効果音を再生
        /// </summary>
        void PlayEffect(string effectId, Vector3 position = default, float volume = 1f);
        
        /// <summary>
        /// ループ効果音を開始
        /// </summary>
        int StartLoopingEffect(string effectId, Vector3 position, float volume = 1f);
        
        /// <summary>
        /// ループ効果音を停止
        /// </summary>
        void StopLoopingEffect(int loopId);
        
        /// <summary>
        /// 一度だけ再生する効果音（重複防止）
        /// </summary>
        void PlayOneShot(string effectId, Vector3 position = default, float volume = 1f);
        
        /// <summary>
        /// ランダムな効果音を再生
        /// </summary>
        void PlayRandomEffect(string[] effectIds, Vector3 position = default, float volume = 1f);
        
        /// <summary>
        /// 効果音のピッチを設定
        /// </summary>
        void SetEffectPitch(string effectId, float pitch);
        
        /// <summary>
        /// 効果音プールをプリロード
        /// </summary>
        void PreloadEffects(string[] effectIds);
        
        /// <summary>
        /// 効果音プールをクリア
        /// </summary>
        void ClearEffectPool();
    }
}