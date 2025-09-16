using UnityEngine;

namespace asterivo.Unity60.Features.Templates.Stealth.Configuration
{
    /// <summary>
    /// Layer 1: ステルス環境設定
    /// 環境相互作用、隠蔽場所、環境効果の設定
    /// Learn & Grow価値実現のための環境学習支援
    /// </summary>
    [CreateAssetMenu(menuName = "Stealth/Environment Config", fileName = "StealthEnvironmentConfig")]
    public class StealthEnvironmentConfig : ScriptableObject
    {
        [Header("Hiding Spots Configuration")]
        [Tooltip("隠蔽場所の自動検出")]
        public bool AutoDetectHidingSpots = true;

        [Tooltip("隠蔽場所として認識するタグ")]
        public string[] HidingSpotTags = {"Cover", "Bush", "Shadow"};

        [Range(0.1f, 5f)]
        [Tooltip("隠蔽場所の効果範囲")]
        public float HidingSpotEffectRadius = 1.5f;

        [Header("Environmental Interactions")]
        [Tooltip("環境オブジェクトとの相互作用レイヤー")]
        public LayerMask InteractableLayers = -1;

        [Range(0.5f, 10f)]
        [Tooltip("相互作用可能距離")]
        public float MaxInteractionDistance = 3f;

        [Tooltip("相互作用時のハイライト表示")]
        public bool ShowInteractionHighlights = true;

        [Header("Environmental Effects")]
        [Range(0f, 2f)]
        [Tooltip("風による草木の揺れ効果")]
        public float WindEffect = 0.5f;

        [Range(0f, 1f)]
        [Tooltip("環境光による隠蔽効果")]
        public float LightingInfluence = 0.7f;

        [Tooltip("時間による光量変化の有効化")]
        public bool EnableDynamicLighting = true;

        [Header("Noise Generation")]
        [Range(0f, 2f)]
        [Tooltip("足音の環境による減衰")]
        public float FootstepDamping = 1.0f;

        [Range(0f, 3f)]
        [Tooltip("環境ノイズレベル")]
        public float AmbientNoiseLevel = 0.3f;

        [Tooltip("天候による音響変化")]
        public bool WeatherAffectsSound = true;

        [Header("Learn & Grow Settings")]
        [Tooltip("初心者向け隠蔽場所の可視化")]
        public bool ShowHidingSpotHints = true;

        [Tooltip("環境相互作用のチュートリアルヒント")]
        public bool ShowInteractionTutorialHints = true;

        [Range(0.5f, 2f)]
        [Tooltip("初心者向け隠蔽効果ボーナス")]
        public float BeginnerConcealmentBonus = 1.3f;

        [Header("Performance Settings")]
        [Range(5f, 30f)]
        [Tooltip("環境効果の更新間隔（秒）")]
        public float EnvironmentUpdateInterval = 10f;

        [Range(0.01f, 1f)]
        [Tooltip("システム更新間隔")]
        public float UpdateInterval = 0.1f;

        [Range(10, 100)]
        [Tooltip("同時処理する環境オブジェクト数上限")]
        public int MaxConcurrentEnvironmentalEffects = 50;

        [Range(0f, 1f)]
        [Tooltip("デフォルト隠蔽レベル")]
        public float DefaultConcealmentLevel = 0.5f;

        [Range(1, 50)]
        [Tooltip("隠蔽場所のデフォルト収容能力")]
        public int DefaultCapacity = 10;

        /// <summary>
        /// デフォルト設定の適用
        /// </summary>
        public void ApplyDefaultSettings()
        {
            AutoDetectHidingSpots = true;
            HidingSpotTags = new string[] {"Cover", "Bush", "Shadow"};
            HidingSpotEffectRadius = 1.5f;
            MaxInteractionDistance = 3f;
            ShowInteractionHighlights = true;
            WindEffect = 0.5f;
            LightingInfluence = 0.7f;
            EnableDynamicLighting = true;
            FootstepDamping = 1.0f;
            AmbientNoiseLevel = 0.3f;
            WeatherAffectsSound = true;
            ShowHidingSpotHints = true;
            ShowInteractionTutorialHints = true;
            BeginnerConcealmentBonus = 1.3f;
            EnvironmentUpdateInterval = 10f;
            UpdateInterval = 0.1f;
            MaxConcurrentEnvironmentalEffects = 50;
            DefaultConcealmentLevel = 0.5f;
            DefaultCapacity = 10;
        }

        /// <summary>
        /// 高品質設定（視覚的により豊か）
        /// </summary>
        public void ApplyHighQualitySettings()
        {
            WindEffect = 1.0f;
            LightingInfluence = 1.0f;
            EnableDynamicLighting = true;
            WeatherAffectsSound = true;
            EnvironmentUpdateInterval = 5f;
            MaxConcurrentEnvironmentalEffects = 75;
        }

        /// <summary>
        /// パフォーマンス重視設定
        /// </summary>
        public void ApplyPerformanceSettings()
        {
            WindEffect = 0.2f;
            LightingInfluence = 0.5f;
            EnableDynamicLighting = false;
            WeatherAffectsSound = false;
            EnvironmentUpdateInterval = 20f;
            MaxConcurrentEnvironmentalEffects = 25;
        }

        /// <summary>
        /// 学習支援設定
        /// </summary>
        public void ApplyLearningSupportSettings()
        {
            ShowHidingSpotHints = true;
            ShowInteractionTutorialHints = true;
            BeginnerConcealmentBonus = 1.5f;
            ShowInteractionHighlights = true;
            AutoDetectHidingSpots = true;
        }

        /// <summary>
        /// 設定の検証
        /// </summary>
        public bool ValidateSettings()
        {
            bool isValid = true;

            if (HidingSpotEffectRadius <= 0f)
            {
                Debug.LogWarning("StealthEnvironmentConfig: HidingSpotEffectRadius must be greater than 0");
                isValid = false;
            }

            if (MaxInteractionDistance <= 0f)
            {
                Debug.LogWarning("StealthEnvironmentConfig: MaxInteractionDistance must be greater than 0");
                isValid = false;
            }

            if (EnvironmentUpdateInterval <= 0f)
            {
                Debug.LogWarning("StealthEnvironmentConfig: EnvironmentUpdateInterval must be greater than 0");
                isValid = false;
            }

            return isValid;
        }
    }
}