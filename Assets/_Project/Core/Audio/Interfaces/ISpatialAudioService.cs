using UnityEngine;

namespace asterivo.Unity60.Core.Audio.Interfaces
{
    /// <summary>
    /// 空間音響サービスのインターフェース
    /// </summary>
    public interface ISpatialAudioService
    {
        /// <summary>
        /// 3D空間でサウンドを再生
        /// </summary>
        void Play3DSound(string soundId, Vector3 position, float maxDistance = 50f, float volume = 1f);
        
        /// <summary>
        /// 移動する音源を作成
        /// </summary>
        void CreateMovingSound(string soundId, Transform source, float maxDistance = 50f);
        
        /// <summary>
        /// 環境音を設定
        /// </summary>
        void SetAmbientSound(string soundId, float volume = 0.5f);
        
        /// <summary>
        /// オクルージョン（遮蔽）を更新
        /// </summary>
        void UpdateOcclusion(Vector3 listenerPosition, Vector3 sourcePosition, float occlusionLevel);
        
        /// <summary>
        /// リバーブゾーンを設定
        /// </summary>
        void SetReverbZone(string zoneId, float reverbLevel);
        
        /// <summary>
        /// ドップラー効果の強度を設定
        /// </summary>
        void SetDopplerLevel(float level);
        
        /// <summary>
        /// リスナーの位置を更新
        /// </summary>
        void UpdateListenerPosition(Vector3 position, Vector3 forward);
    }
}