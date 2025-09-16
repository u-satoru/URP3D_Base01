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
        [Header("Core Stealth System")]
        [Tooltip("設定名")]
        public string name = "Stealth Mechanics Configuration";

        [Tooltip("ステルスメカニクスを有効にするか")]
        public bool enableStealthMechanics = true;

        [Range(0.01f, 1f)]
        [Tooltip("システム更新間隔")]
        public float updateInterval = 0.1f;

        [Range(0f, 1f)]
        [Tooltip("基本可視性レベル")]
        public float baseVisibility = 0.5f;

        [Header("Player Stealth Mechanics")]
        [Range(0f, 1f)]
        [Tooltip("しゃがみ時のステルス効果倍率")]
        public float BaseCrouchStealthMultiplier = 0.3f;

        [Range(0f, 1f)]
        [Tooltip("しゃがみ時の可視性修正値")]
        public float crouchVisibilityModifier = 0.3f;

        [Range(0f, 1f)]
        [Tooltip("歩行時のステルス効果倍率")]
        public float WalkingStealthMultiplier = 0.6f;

        [Range(0f, 1f)]
        [Tooltip("走行時のステルス効果倍率")]
        public float RunningStealthMultiplier = 1.0f;

        [Range(0f, 1f)]
        [Tooltip("伏せ時のステルス効果倍率（最高レベル）")]
        public float ProneStealthMultiplier = 0.1f;

        [Range(0f, 1f)]
        [Tooltip("伏せ時の可視性修正値")]
        public float proneVisibilityModifier = 0.1f;

        [Range(0f, 2f)]
        [Tooltip("移動時のノイズ生成倍率")]
        public float MovementNoiseMultiplier = 1.0f;

        [Range(0f, 2f)]
        [Tooltip("移動時の可視性修正値")]
        public float movementVisibilityModifier = 1.0f;

        [Range(0f, 5f)]
        [Tooltip("環境音によるマスキング効果範囲")]
        public float EnvironmentMaskingRange = 2.0f;

        [Header("Detection System")]
        [Range(0.1f, 2f)]
        [Tooltip("検知レベル減衰率")]
        public float detectionDecayRate = 0.5f;

        [Range(0.1f, 2f)]
        [Tooltip("警戒レベル減衰率")]
        public float alertDecayRate = 0.3f;

        [Range(1f, 20f)]
        [Tooltip("カバー検知半径")]
        public float coverDetectionRadius = 5.0f;

        [Range(0f, 1f)]
        [Tooltip("影による可視性削減")]
        public float shadowVisibilityReduction = 0.3f;

        [Range(0f, 1f)]
        [Tooltip("植物による可視性削減")]
        public float foliageVisibilityReduction = 0.4f;

        [Range(5f, 50f)]
        [Tooltip("最大検知範囲")]
        public float maxDetectionRange = 25.0f;

        [Range(1f, 10f)]
        [Tooltip("即座検知範囲")]
        public float instantDetectionRange = 3.0f;

        [Header("Audio & Noise System")]
        [Range(0f, 1f)]
        [Tooltip("基本ノイズレベル")]
        public float baseNoiseLevel = 0.1f;

        [Range(0f, 1f)]
        [Tooltip("歩行時のノイズレベル")]
        public float walkNoiseLevel = 0.3f;

        [Range(0f, 1f)]
        [Tooltip("走行時のノイズレベル")]
        public float runNoiseLevel = 0.8f;

        [Range(0f, 1f)]
        [Tooltip("しゃがみ時のノイズ削減効果")]
        public float crouchNoiseReduction = 0.6f;

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

        [Range(0f, 1f)]
        [Tooltip("チュートリアル用の基本ステルスレベル（学習支援）")]
        public float TutorialBaseStealthLevel = 0.3f;

        [Tooltip("チュートリアル用デバッグ情報の表示")]
        public bool ShowTutorialDebugInfo = true;

        /// <summary>
        /// デフォルト設定の適用
        /// Learn & Grow価値実現のための最適化済み設定
        /// </summary>
        public void ApplyDefaultSettings()
        {
            enableStealthMechanics = true;
            updateInterval = 0.1f;
            baseVisibility = 0.5f;
            BaseCrouchStealthMultiplier = 0.3f;
            crouchVisibilityModifier = 0.3f;
            WalkingStealthMultiplier = 0.6f;
            RunningStealthMultiplier = 1.0f;
            ProneStealthMultiplier = 0.1f;
            proneVisibilityModifier = 0.1f;
            MovementNoiseMultiplier = 1.0f;
            movementVisibilityModifier = 1.0f;
            EnvironmentMaskingRange = 2.0f;
            detectionDecayRate = 0.5f;
            alertDecayRate = 0.3f;
            coverDetectionRadius = 5.0f;
            shadowVisibilityReduction = 0.3f;
            foliageVisibilityReduction = 0.4f;
            maxDetectionRange = 25.0f;
            instantDetectionRange = 3.0f;
            baseNoiseLevel = 0.1f;
            walkNoiseLevel = 0.3f;
            runNoiseLevel = 0.8f;
            crouchNoiseReduction = 0.6f;
            HidingEffectiveness = 0.8f;
            HidingDetectionRadius = 1.5f;
            InteractionRange = 2.0f;
            InteractionDuration = 1.0f;
            AllowMovementDuringInteraction = false;
            UseTutorialFriendlySettings = true;
            BeginnerStealthBonus = 1.2f;
            TutorialBaseStealthLevel = 0.3f;
            ShowTutorialDebugInfo = true;
        }

        /// <summary>
        /// 上級者向け設定
        /// より高い難易度とリアリズム
        /// </summary>
        public void ApplyAdvancedSettings()
        {
            enableStealthMechanics = true;
            updateInterval = 0.05f;
            baseVisibility = 0.7f;
            BaseCrouchStealthMultiplier = 0.5f;
            crouchVisibilityModifier = 0.5f;
            WalkingStealthMultiplier = 0.8f;
            RunningStealthMultiplier = 1.2f;
            ProneStealthMultiplier = 0.2f;
            proneVisibilityModifier = 0.2f;
            MovementNoiseMultiplier = 1.5f;
            movementVisibilityModifier = 1.5f;
            EnvironmentMaskingRange = 1.5f;
            detectionDecayRate = 0.3f;
            alertDecayRate = 0.2f;
            coverDetectionRadius = 3.0f;
            shadowVisibilityReduction = 0.2f;
            foliageVisibilityReduction = 0.3f;
            maxDetectionRange = 30.0f;
            instantDetectionRange = 2.0f;
            baseNoiseLevel = 0.2f;
            walkNoiseLevel = 0.5f;
            runNoiseLevel = 1.0f;
            crouchNoiseReduction = 0.4f;
            HidingEffectiveness = 0.6f;
            HidingDetectionRadius = 2.0f;
            InteractionRange = 1.5f;
            InteractionDuration = 1.5f;
            AllowMovementDuringInteraction = false;
            UseTutorialFriendlySettings = false;
            BeginnerStealthBonus = 1.0f;
            TutorialBaseStealthLevel = 0.1f;
            ShowTutorialDebugInfo = false;
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