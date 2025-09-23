using UnityEngine;
// using asterivo.Unity60.Core.Data;

namespace asterivo.Unity60.Core.Audio.Interfaces
{
    /// <summary>
    /// スチE��スゲーム用オーチE��オサービスのインターフェース
    /// </summary>
    public interface IStealthAudioService
    {
        /// <summary>
        /// 足音を生戁E        /// </summary>
        void CreateFootstep(Vector3 position, float intensity, string surfaceType);
        
        /// <summary>
        /// 環墁E��イズレベルを設定（�Eスキング効果用�E�E        /// </summary>
        void SetEnvironmentNoiseLevel(float level);
        
        /// <summary>
        /// NPCに聞こえる音を生戁E        /// </summary>
        void EmitDetectableSound(Vector3 position, float radius, float intensity, string soundType);
        
        /// <summary>
        /// 注意を引く音を�E甁E        /// </summary>
        void PlayDistraction(Vector3 position, float radius);
        
        /// <summary>
        /// 警戒レベルに応じたBGMを設宁E        /// </summary>
        void SetAlertLevelMusic(AlertLevel level);
        
        /// <summary>
        /// オーチE��オマスキング効果を適用
        /// </summary>
        void ApplyAudioMasking(float maskingLevel);
        
        /// <summary>
        /// NPCの聴覚センサーにサウンドイベントを通知
        /// </summary>
        void NotifyAuditorySensors(Vector3 origin, float radius, float intensity);
        
        /// <summary>
        /// プレイヤーの隠寁E��に応じた音響調整
        /// </summary>
        void AdjustStealthAudio(float stealthLevel);
        
        /// <summary>
        /// 目標達成時のサウンドを再生
        /// </summary>
        /// <param name="withBonus">ボ�Eナス付きかどぁE��</param>
        void PlayObjectiveCompleteSound(bool withBonus);
    }
}