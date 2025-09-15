using UnityEngine;
using UnityEngine.Audio;

namespace asterivo.Unity60.Features.Templates.Stealth.Configuration
{
    /// <summary>
    /// Layer 1: ステルスオーディオ設定
    /// 3D空間オーディオ、NPCの聴覚センサー、音響マスキング統合
    /// StealthAudioCoordinator連携設定
    /// </summary>
    [System.Serializable]
    public class StealthAudioConfig
    {
        [Header("Audio Mixer Integration")]
        [Tooltip("ステルス用オーディオミキサー")]
        public AudioMixerGroup StealthAudioMixer;

        [Tooltip("環境音用ミキサーグループ")]
        public AudioMixerGroup AmbientAudioGroup;

        [Tooltip("プレイヤーアクション音用ミキサーグループ")]
        public AudioMixerGroup PlayerActionAudioGroup;

        [Header("Spatial Audio Settings")]
        [Range(0.1f, 100f)]
        [Tooltip("3D音響の最大距離")]
        public float MaxAudioDistance = 50f;

        [Range(0f, 1f)]
        [Tooltip("ドップラー効果の強さ")]
        public float DopplerLevel = 0.3f;

        [Range(0, 360)]
        [Tooltip("3Dオーディオの拡散角度")]
        public int Spread = 90;

        [Header("Masking & Detection")]
        [Range(0f, 1f)]
        [Tooltip("環境音によるマスキング効果")]
        public float EnvironmentalMaskingStrength = 0.6f;

        [Range(1f, 20f)]
        [Tooltip("音響マスキングの効果範囲")]
        public float MaskingEffectRange = 8f;

        [Range(0.1f, 2f)]
        [Tooltip("NPCの聴覚感度")]
        public float NPCHearingSensitivity = 1f;

        [Header("Player Audio")]
        [Range(0f, 2f)]
        [Tooltip("歩行音のベース音量")]
        public float FootstepBaseVolume = 0.8f;

        [Range(0f, 2f)]
        [Tooltip("走行音のベース音量")]
        public float RunningBaseVolume = 1.2f;

        [Range(0f, 1f)]
        [Tooltip("しゃがみ歩行の音量倍率")]
        public float CrouchVolumeMultiplier = 0.3f;

        [Range(0f, 0.5f)]
        [Tooltip("伏せ移動の音量倍率")]
        public float ProneVolumeMultiplier = 0.1f;

        [Header("Dynamic Audio Effects")]
        [Tooltip("リアルタイムオーディオダッキング有効化")]
        public bool EnableDynamicDucking = true;

        [Range(0.1f, 3f)]
        [Tooltip("ダッキング効果の強さ")]
        public float DuckingStrength = 0.7f;

        [Range(0.1f, 5f)]
        [Tooltip("ダッキング効果の適用時間")]
        public float DuckingDuration = 2f;

        [Header("Audio Occlusion")]
        [Tooltip("物理的遮蔽による音響減衰")]
        public bool EnableAudioOcclusion = true;

        [Tooltip("遮蔽判定用レイヤー")]
        public LayerMask OcclusionLayers = -1;

        [Range(0f, 1f)]
        [Tooltip("遮蔽時の音量減衰率")]
        public float OcclusionDamping = 0.4f;

        [Header("Performance Optimization")]
        [Range(5, 100)]
        [Tooltip("同時再生可能なオーディオソース数")]
        public int MaxConcurrentAudioSources = 32;

        [Range(0.01f, 0.1f)]
        [Tooltip("オーディオ処理のフレーム時間上限")]
        public float MaxAudioProcessingTime = 0.05f;

        [Tooltip("距離によるLOD有効化")]
        public bool EnableAudioLOD = true;

        [Header("Learn & Grow Settings")]
        [Tooltip("チュートリアル用音響ヒント")]
        public bool EnableAudioTutorialHints = true;

        [Range(0.5f, 2f)]
        [Tooltip("初心者向けマスキング効果強化")]
        public float BeginnerMaskingBonus = 1.4f;

        [Tooltip("視覚的音響インジケーター表示")]
        public bool ShowAudioVisualIndicators = true;

        /// <summary>
        /// デフォルト設定の適用
        /// StealthAudioCoordinatorとの統合最適化
        /// </summary>
        public void ApplyDefaultSettings()
        {
            MaxAudioDistance = 50f;
            DopplerLevel = 0.3f;
            Spread = 90;
            EnvironmentalMaskingStrength = 0.6f;
            MaskingEffectRange = 8f;
            NPCHearingSensitivity = 1f;
            FootstepBaseVolume = 0.8f;
            RunningBaseVolume = 1.2f;
            CrouchVolumeMultiplier = 0.3f;
            ProneVolumeMultiplier = 0.1f;
            EnableDynamicDucking = true;
            DuckingStrength = 0.7f;
            DuckingDuration = 2f;
            EnableAudioOcclusion = true;
            OcclusionDamping = 0.4f;
            MaxConcurrentAudioSources = 32;
            MaxAudioProcessingTime = 0.05f;
            EnableAudioLOD = true;
            EnableAudioTutorialHints = true;
            BeginnerMaskingBonus = 1.4f;
            ShowAudioVisualIndicators = true;
        }

        /// <summary>
        /// 高品質オーディオ設定
        /// </summary>
        public void ApplyHighQualitySettings()
        {
            MaxAudioDistance = 100f;
            DopplerLevel = 0.5f;
            EnableDynamicDucking = true;
            EnableAudioOcclusion = true;
            MaxConcurrentAudioSources = 64;
            EnableAudioLOD = false; // 品質重視でLOD無効
        }

        /// <summary>
        /// パフォーマンス最適化設定
        /// </summary>
        public void ApplyPerformanceSettings()
        {
            MaxAudioDistance = 25f;
            DopplerLevel = 0.1f;
            EnableDynamicDucking = false;
            EnableAudioOcclusion = false;
            MaxConcurrentAudioSources = 16;
            MaxAudioProcessingTime = 0.03f;
            EnableAudioLOD = true;
        }

        /// <summary>
        /// 学習支援設定
        /// Learn & Grow価値実現特化
        /// </summary>
        public void ApplyLearningSupportSettings()
        {
            EnableAudioTutorialHints = true;
            BeginnerMaskingBonus = 1.6f;
            ShowAudioVisualIndicators = true;
            EnvironmentalMaskingStrength = 0.8f; // より強いマスキング効果
            NPCHearingSensitivity = 0.7f; // 若干低めの感度
        }

        /// <summary>
        /// 設定の検証
        /// </summary>
        public bool ValidateSettings()
        {
            bool isValid = true;

            if (MaxAudioDistance <= 0f)
            {
                Debug.LogWarning("StealthAudioConfig: MaxAudioDistance must be greater than 0");
                isValid = false;
            }

            if (MaskingEffectRange <= 0f)
            {
                Debug.LogWarning("StealthAudioConfig: MaskingEffectRange must be greater than 0");
                isValid = false;
            }

            if (MaxConcurrentAudioSources <= 0)
            {
                Debug.LogWarning("StealthAudioConfig: MaxConcurrentAudioSources must be greater than 0");
                isValid = false;
            }

            return isValid;
        }
    }
}