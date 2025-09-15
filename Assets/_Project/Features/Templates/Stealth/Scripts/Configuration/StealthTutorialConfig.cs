using UnityEngine;

namespace asterivo.Unity60.Features.Templates.Stealth
{
    /// <summary>
    /// Layer 1: ステルスチュートリアル設定
    /// Learn & Grow価値実現の核心システム
    /// 70%学習コスト削減、15分ゲームプレイ実現
    /// </summary>
    [System.Serializable]
    public class StealthTutorialConfig
    {
        [Header("Quick Start Settings")]
        [Tooltip("クイックスタート機能有効化（Learn & Grow価値実現）")]
        public bool EnableQuickStart = true;

        [Range(1f, 15f)]
        [Tooltip("クイックスタートまでの時間（分）")]
        public float QuickStartTimeMinutes = 5f;

        [Tooltip("基本操作スキップ許可")]
        public bool AllowBasicControlsSkip = true;

        [Header("5-Stage Learning System")]
        [Tooltip("段階的学習システム有効化")]
        public bool EnableProgressiveLearning = true;

        [Header("Stage 1: Basics (0-5 minutes)")]
        [Tooltip("基本移動チュートリアル")]
        public bool EnableBasicMovementTutorial = true;

        [Range(30f, 300f)]
        [Tooltip("基本移動の習得目標時間（秒）")]
        public float BasicMovementTargetTime = 120f;

        [Header("Stage 2: Core Mechanics (5-10 minutes)")]
        [Tooltip("ステルス基礎メカニクス")]
        public bool EnableStealthBasicsTutorial = true;

        [Range(60f, 600f)]
        [Tooltip("ステルス基礎の習得目標時間（秒）")]
        public float StealthBasicsTargetTime = 300f;

        [Header("Stage 3: AI Interaction (10-15 minutes)")]
        [Tooltip("AI検知システム学習")]
        public bool EnableAIDetectionTutorial = true;

        [Range(60f, 600f)]
        [Tooltip("AI検知学習の目標時間（秒）")]
        public float AIDetectionTargetTime = 300f;

        [Header("Stage 4: Advanced Tactics (15-20 minutes)")]
        [Tooltip("高度な戦術チュートリアル")]
        public bool EnableAdvancedTacticsTutorial = true;

        [Range(120f, 600f)]
        [Tooltip("高度戦術の習得目標時間（秒）")]
        public float AdvancedTacticsTargetTime = 300f;

        [Header("Stage 5: Free Play (20+ minutes)")]
        [Tooltip("自由プレイモード")]
        public bool EnableFreePlayMode = true;

        [Range(300f, 1800f)]
        [Tooltip("自由プレイでの習熟目標時間（秒）")]
        public float FreePlayTargetTime = 600f;

        [Header("Learning Cost Reduction Settings")]
        [Tooltip("従来学習時間（時間）- 測定ベースライン")]
        [Range(20f, 60f)]
        public float TraditionalLearningTimeHours = 40f;

        [Tooltip("目標学習時間（時間）- 70%削減目標")]
        [Range(5f, 20f)]
        public float TargetLearningTimeHours = 12f;

        [Range(0.5f, 1f)]
        [Tooltip("学習コスト削減率（目標70%）")]
        public float LearningCostReductionRate = 0.7f;

        [Header("Interactive Hints System")]
        [Tooltip("コンテキスト依存ヒント")]
        public bool EnableContextualHints = true;

        [Range(1f, 10f)]
        [Tooltip("ヒント表示間隔（秒）")]
        public float HintDisplayInterval = 3f;

        [Range(3f, 15f)]
        [Tooltip("ヒント表示持続時間（秒）")]
        public float HintDuration = 6f;

        [Tooltip("適応的難易度調整")]
        public bool EnableAdaptiveDifficulty = true;

        [Header("Progress Tracking")]
        [Tooltip("詳細進捗追跡")]
        public bool EnableDetailedProgressTracking = true;

        [Tooltip("学習効率測定")]
        public bool EnableLearningEfficiencyMeasurement = true;

        [Tooltip("自動チェックポイント")]
        public bool EnableAutoCheckpoints = true;

        [Range(60f, 300f)]
        [Tooltip("チェックポイント間隔（秒）")]
        public float CheckpointInterval = 120f;

        [Header("15-Minute Gameplay Target")]
        [Tooltip("15分ゲームプレイ実現機能")]
        public bool EnableFifteenMinuteGameplay = true;

        [Range(10f, 20f)]
        [Tooltip("基本ゲームプレイ到達目標時間（分）")]
        public float BasicGameplayTargetMinutes = 15f;

        [Tooltip("スキップ可能要素の自動識別")]
        public bool EnableSmartSkipping = true;

        [Header("Feedback & Motivation")]
        [Tooltip("即座フィードバック")]
        public bool EnableImmediateFeedback = true;

        [Tooltip("達成バッジシステム")]
        public bool EnableAchievementBadges = true;

        [Tooltip("プログレス可視化")]
        public bool EnableProgressVisualization = true;

        [Range(0.5f, 3f)]
        [Tooltip("成功時の称賛表示時間（秒）")]
        public float SuccessCelebrationDuration = 2f;

        /// <summary>
        /// Learn & Grow最適化設定
        /// 70%学習コスト削減と15分ゲームプレイ実現
        /// </summary>
        public void ApplyLearnAndGrowOptimizedSettings()
        {
            EnableQuickStart = true;
            QuickStartTimeMinutes = 3f; // より高速
            AllowBasicControlsSkip = true;
            
            EnableProgressiveLearning = true;
            EnableBasicMovementTutorial = true;
            BasicMovementTargetTime = 90f; // 短縮
            
            EnableStealthBasicsTutorial = true;
            StealthBasicsTargetTime = 180f; // 短縮
            
            EnableAIDetectionTutorial = true;
            AIDetectionTargetTime = 240f; // 短縮
            
            EnableAdvancedTacticsTutorial = true;
            AdvancedTacticsTargetTime = 180f; // 短縮
            
            EnableFreePlayMode = true;
            FreePlayTargetTime = 420f; // 7分
            
            // 学習効率最大化
            TargetLearningTimeHours = 12f;
            LearningCostReductionRate = 0.7f;
            
            EnableContextualHints = true;
            HintDisplayInterval = 2f; // より頻繁
            EnableAdaptiveDifficulty = true;
            
            // 15分ゲームプレイ実現
            EnableFifteenMinuteGameplay = true;
            BasicGameplayTargetMinutes = 15f;
            EnableSmartSkipping = true;
            
            EnableImmediateFeedback = true;
            EnableAchievementBadges = true;
            EnableProgressVisualization = true;
        }

        /// <summary>
        /// 従来方式設定（比較用）
        /// </summary>
        public void ApplyTraditionalSettings()
        {
            EnableQuickStart = false;
            AllowBasicControlsSkip = false;
            
            BasicMovementTargetTime = 300f;
            StealthBasicsTargetTime = 600f;
            AIDetectionTargetTime = 600f;
            AdvancedTacticsTargetTime = 600f;
            FreePlayTargetTime = 1200f;
            
            TraditionalLearningTimeHours = 40f;
            TargetLearningTimeHours = 40f;
            LearningCostReductionRate = 0f;
            
            EnableContextualHints = false;
            EnableAdaptiveDifficulty = false;
            EnableFifteenMinuteGameplay = false;
            EnableSmartSkipping = false;
        }

        /// <summary>
        /// エキスパート向け設定
        /// 最小限のガイダンス
        /// </summary>
        public void ApplyExpertSettings()
        {
            EnableQuickStart = false;
            EnableProgressiveLearning = false;
            AllowBasicControlsSkip = true;
            
            EnableContextualHints = false;
            EnableAdaptiveDifficulty = false;
            EnableImmediateFeedback = false;
            EnableAchievementBadges = false;
            
            // すべてのチュートリアルを無効化
            EnableBasicMovementTutorial = false;
            EnableStealthBasicsTutorial = false;
            EnableAIDetectionTutorial = false;
            EnableAdvancedTacticsTutorial = false;
        }

        /// <summary>
        /// 設定の検証
        /// </summary>
        public bool ValidateSettings()
        {
            bool isValid = true;

            // Learn & Grow価値実現の検証
            if (LearningCostReductionRate < 0.5f)
            {
                Debug.LogWarning("StealthTutorialConfig: LearningCostReductionRate should be at least 50% for Learn & Grow value");
            }

            if (EnableFifteenMinuteGameplay && BasicGameplayTargetMinutes > 20f)
            {
                Debug.LogWarning("StealthTutorialConfig: BasicGameplayTargetMinutes exceeds 15-minute target");
                isValid = false;
            }

            if (TargetLearningTimeHours > TraditionalLearningTimeHours)
            {
                Debug.LogWarning("StealthTutorialConfig: TargetLearningTimeHours should be less than TraditionalLearningTimeHours");
                isValid = false;
            }

            return isValid;
        }

        /// <summary>
        /// 学習効率計算
        /// </summary>
        public float CalculateLearningEfficiency()
        {
            return 1f - (TargetLearningTimeHours / TraditionalLearningTimeHours);
        }

        /// <summary>
        /// 15分ゲームプレイ達成予測時間
        /// </summary>
        public float GetEstimatedGameplayReadyTime()
        {
            if (!EnableFifteenMinuteGameplay) return float.MaxValue;
            
            return BasicMovementTargetTime + StealthBasicsTargetTime + AIDetectionTargetTime;
        }
    }
}