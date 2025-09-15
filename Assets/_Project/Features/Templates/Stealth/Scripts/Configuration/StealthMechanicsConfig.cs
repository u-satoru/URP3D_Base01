using UnityEngine;

namespace asterivo.Unity60.Features.Templates.Stealth.Configuration
{
    /// <summary>
    /// Layer 1: ステルスメカニクス設定
    /// プレイヤーの基本的なステルス動作パラメータ
    /// Learn & Grow価値実現のための調整可能な設定
    /// </summary>
    [System.Serializable]
    public class StealthMechanicsConfig
    {
        [Header("Player Stealth Mechanics")]
        [Range(0f, 1f)]
        [Tooltip("しゃがみ時のステルス効果倍率")]
        public float BaseCrouchStealthMultiplier = 0.3f;

        [Range(0f, 1f)]
        [Tooltip("伏せ時のステルス効果倍率（最高レベル）")]
        public float ProneStealthMultiplier = 0.1f;

        [Range(0f, 2f)]
        [Tooltip("移動時のノイズ生成倍率")]
        public float MovementNoiseMultiplier = 1.0f;

        [Range(0f, 5f)]
        [Tooltip("環境音によるマスキング効果範囲")]
        public float EnvironmentMaskingRange = 2.0f;

        [Header("Hiding Mechanics")]
        [Tooltip("隠れ場所として認識するレイヤー")]
        public LayerMask HidingSpotLayers = -1;

        [Range(0.1f, 2f)]
        [Tooltip("隠れ場所での隠蔽効果")]
        public float HidingEffectiveness = 0.8f;

        [Range(0.1f, 5f)]
        [Tooltip("隠れ場所からの検知可能範囲")]
        public float HidingDetectionRadius = 1.5f;

        [Header("Interaction Mechanics")]
        [Range(0.5f, 5f)]
        [Tooltip("環境オブジェクトとの相互作用範囲")]
        public float InteractionRange = 2.0f;

        [Range(0.1f, 2f)]
        [Tooltip("相互作用の実行時間")]
        public float InteractionDuration = 1.0f;

        [Tooltip("相互作用中の移動許可")]
        public bool AllowMovementDuringInteraction = false;

        [Header("Learn & Grow Settings")]
        [Tooltip("チュートリアル用の簡易設定（学習コスト削減）")]
        public bool UseTutorialFriendlySettings = true;

        [Range(0.5f, 2f)]
        [Tooltip("初心者向けステルス効果ボーナス")]
        public float BeginnerStealthBonus = 1.2f;

        /// <summary>
        /// デフォルト設定の適用
        /// Learn & Grow価値実現のための最適化済み設定
        /// </summary>
        public void ApplyDefaultSettings()
        {
            BaseCrouchStealthMultiplier = 0.3f;
            ProneStealthMultiplier = 0.1f;
            MovementNoiseMultiplier = 1.0f;
            EnvironmentMaskingRange = 2.0f;
            HidingEffectiveness = 0.8f;
            HidingDetectionRadius = 1.5f;
            InteractionRange = 2.0f;
            InteractionDuration = 1.0f;
            AllowMovementDuringInteraction = false;
            UseTutorialFriendlySettings = true;
            BeginnerStealthBonus = 1.2f;
        }

        /// <summary>
        /// 上級者向け設定
        /// より高い難易度とリアリズム
        /// </summary>
        public void ApplyAdvancedSettings()
        {
            BaseCrouchStealthMultiplier = 0.5f;
            ProneStealthMultiplier = 0.2f;
            MovementNoiseMultiplier = 1.5f;
            EnvironmentMaskingRange = 1.5f;
            HidingEffectiveness = 0.6f;
            HidingDetectionRadius = 2.0f;
            InteractionRange = 1.5f;
            InteractionDuration = 1.5f;
            AllowMovementDuringInteraction = false;
            UseTutorialFriendlySettings = false;
            BeginnerStealthBonus = 1.0f;
        }

        /// <summary>
        /// 設定の検証
        /// </summary>
        public bool ValidateSettings()
        {
            bool isValid = true;

            if (BaseCrouchStealthMultiplier <= 0f)
            {
                Debug.LogWarning("StealthMechanicsConfig: BaseCrouchStealthMultiplier must be greater than 0");
                isValid = false;
            }

            if (InteractionRange <= 0f)
            {
                Debug.LogWarning("StealthMechanicsConfig: InteractionRange must be greater than 0");
                isValid = false;
            }

            return isValid;
        }
    }
}