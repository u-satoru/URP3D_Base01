using UnityEngine;

namespace asterivo.Unity60.Core.Audio.Interfaces
{
    /// <summary>
    /// ステルスゲーム用オーディオサービスのインターフェース
    /// </summary>
    public interface IStealthAudioService
    {
        /// <summary>
        /// 足音を生成
        /// </summary>
        void CreateFootstep(Vector3 position, float intensity, string surfaceType);
        
        /// <summary>
        /// 環境ノイズレベルを設定（マスキング効果用）
        /// </summary>
        void SetEnvironmentNoiseLevel(float level);
        
        /// <summary>
        /// NPCに聞こえる音を生成
        /// </summary>
        void EmitDetectableSound(Vector3 position, float radius, float intensity, string soundType);
        
        /// <summary>
        /// 注意を引く音を再生
        /// </summary>
        void PlayDistraction(Vector3 position, float radius);
        
        /// <summary>
        /// 警戒レベルに応じたBGMを設定
        /// </summary>
        void SetAlertLevelMusic(AlertLevel level);
        
        /// <summary>
        /// オーディオマスキング効果を適用
        /// </summary>
        void ApplyAudioMasking(float maskingLevel);
        
        /// <summary>
        /// NPCの聴覚センサーにサウンドイベントを通知
        /// </summary>
        void NotifyAuditorySensors(Vector3 origin, float radius, float intensity);
        
        /// <summary>
        /// プレイヤーの隠密度に応じた音響調整
        /// </summary>
        void AdjustStealthAudio(float stealthLevel);
    }
    
    /// <summary>
    /// 警戒レベル
    /// </summary>
    public enum AlertLevel
    {
        None,       // 通常
        Low,        // 低警戒
        Medium,     // 中警戒
        High,       // 高警戒
        Combat      // 戦闘中
    }
}